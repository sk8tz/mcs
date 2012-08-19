// JoinBlock.cs
//
// Copyright (c) 2011 Jérémie "garuma" Laval
// Copyright (c) 2012 Petr Onderka
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections.Generic;

namespace System.Threading.Tasks.Dataflow
{
	public sealed class JoinBlock<T1, T2> : IReceivableSourceBlock<Tuple<T1, T2>>
	{
		static readonly GroupingDataflowBlockOptions defaultOptions = new GroupingDataflowBlockOptions ();

		readonly CompletionHelper compHelper;
		readonly GroupingDataflowBlockOptions dataflowBlockOptions;
		readonly OutgoingQueue<Tuple<T1, T2>> outgoing;

		readonly JoinTarget<T1> target1;
		readonly JoinTarget<T2> target2;

		SpinLock targetLock = new SpinLock(false);
		readonly AtomicBoolean nonGreedyProcessing = new AtomicBoolean ();

		long target1Count;
		long target2Count;
		long numberOfGroups;

		public JoinBlock () : this (defaultOptions)
		{
		}

		public JoinBlock (GroupingDataflowBlockOptions dataflowBlockOptions)
		{
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");

			this.dataflowBlockOptions = dataflowBlockOptions;
			compHelper = CompletionHelper.GetNew (dataflowBlockOptions);
			target1 = new JoinTarget<T1> (this, SignalArrivalTargetImpl, compHelper,
				() => outgoing.IsCompleted, dataflowBlockOptions,
				dataflowBlockOptions.Greedy, TryAdd1);
			target2 = new JoinTarget<T2> (this, SignalArrivalTargetImpl, compHelper,
				() => outgoing.IsCompleted, dataflowBlockOptions,
				dataflowBlockOptions.Greedy, TryAdd2);
			outgoing = new OutgoingQueue<Tuple<T1, T2>> (this, compHelper,
				() => target1.Buffer.IsCompleted || target2.Buffer.IsCompleted,
				_ =>
				{
					target1.DecreaseCount ();
					target2.DecreaseCount ();
				}, dataflowBlockOptions);
		}

		public IDisposable LinkTo (ITargetBlock<Tuple<T1, T2>> target, DataflowLinkOptions linkOptions)
		{
			return outgoing.AddTarget (target, linkOptions);
		}

		public bool TryReceive (Predicate<Tuple<T1, T2>> filter, out Tuple<T1, T2> item)
		{
			return outgoing.TryReceive (filter, out item);
		}

		public bool TryReceiveAll (out IList<Tuple<T1, T2>> items)
		{
			return outgoing.TryReceiveAll (out items);
		}

		public Tuple<T1, T2> ConsumeMessage (DataflowMessageHeader messageHeader, ITargetBlock<Tuple<T1, T2>> target, out bool messageConsumed)
		{
			return outgoing.ConsumeMessage (messageHeader, target, out messageConsumed);
		}

		public void ReleaseReservation (DataflowMessageHeader messageHeader, ITargetBlock<Tuple<T1, T2>> target)
		{
			outgoing.ReleaseReservation (messageHeader, target);
		}

		public bool ReserveMessage (DataflowMessageHeader messageHeader, ITargetBlock<Tuple<T1, T2>> target)
		{
			return outgoing.ReserveMessage (messageHeader, target);
		}

		public void Complete ()
		{
			target1.Complete ();
			target2.Complete ();
			outgoing.Complete ();
		}

		public void Fault (Exception exception)
		{
			compHelper.RequestFault (exception);
		}

		public Task Completion {
			get {
				return compHelper.Completion;
			}
		}

		bool TryAdd1 ()
		{
			return dataflowBlockOptions.MaxNumberOfGroups == -1
			       || Interlocked.Increment (ref target1Count)
			       <= dataflowBlockOptions.MaxNumberOfGroups;
		}

		bool TryAdd2 ()
		{
			return dataflowBlockOptions.MaxNumberOfGroups == -1
			       || Interlocked.Increment (ref target2Count)
			       <= dataflowBlockOptions.MaxNumberOfGroups;
		}

		void SignalArrivalTargetImpl()
		{
			if (dataflowBlockOptions.Greedy) {
				bool taken = false;
				T1 value1;
				T2 value2;

				try {
					targetLock.Enter (ref taken);

					if (target1.Buffer.Count == 0 || target2.Buffer.Count == 0)
						return;

					value1 = target1.Buffer.Take ();
					value2 = target2.Buffer.Take ();
				} finally {
					if (taken)
						targetLock.Exit ();
				}

				TriggerMessage (value1, value2);
			} else {
				if (ShouldProcessNonGreedy ())
					EnsureNonGreedyProcessing ();
			}
		}

		bool ShouldProcessNonGreedy ()
		{
			return target1.PostponedMessagesCount >= 1
			       && target2.PostponedMessagesCount >= 1
			       && (dataflowBlockOptions.BoundedCapacity == -1
			           || outgoing.Count < dataflowBlockOptions.BoundedCapacity);
		}

		void EnsureNonGreedyProcessing ()
		{
			if (nonGreedyProcessing.TrySet ())
				Task.Factory.StartNew (NonGreedyProcess,
					dataflowBlockOptions.CancellationToken,
					TaskCreationOptions.PreferFairness,
					dataflowBlockOptions.TaskScheduler);
		}

		void NonGreedyProcess()
		{
			while (ShouldProcessNonGreedy ()) {
				var reservation1 = target1.ReserveMessage ();

				if (reservation1 == null)
					break;

				var reservation2 = target2.ReserveMessage ();
				if (reservation2 == null) {
					target1.RelaseReservation (reservation1);
					break;
				}

				var value1 = target1.ConsumeReserved (reservation1);
				var value2 = target2.ConsumeReserved (reservation2);

				TriggerMessage (value1, value2);
			}

			nonGreedyProcessing.Value = false;

			if (ShouldProcessNonGreedy ())
				EnsureNonGreedyProcessing ();
		}


		void TriggerMessage (T1 val1, T2 val2)
		{
			outgoing.AddData (Tuple.Create (val1, val2));

			if (dataflowBlockOptions.MaxNumberOfGroups != -1
			    && Interlocked.Increment (ref numberOfGroups)
			    >= dataflowBlockOptions.MaxNumberOfGroups)
				Complete ();
		}

		public ITargetBlock<T1> Target1 {
			get {
				return target1;
			}
		}

		public ITargetBlock<T2> Target2 {
			get {
				return target2;
			}
		}

		public override string ToString ()
		{
			return NameHelper.GetName (this, dataflowBlockOptions);
		}
	}
}