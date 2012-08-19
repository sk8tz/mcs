// BatchBlock.cs
//
// Copyright (c) 2011 Jérémie "garuma" Laval
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
//
//

using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow
{
	public sealed class BatchBlock<T> : IPropagatorBlock<T, T[]>, ITargetBlock<T>, IDataflowBlock, ISourceBlock<T[]>, IReceivableSourceBlock<T[]>
	{
		static readonly DataflowBlockOptions defaultOptions = new DataflowBlockOptions ();

		CompletionHelper compHelper;
		BlockingCollection<T> messageQueue = new BlockingCollection<T> ();
		MessageBox<T> messageBox;
		MessageVault<T[]> vault;
		DataflowBlockOptions dataflowBlockOptions;
		readonly int batchSize;
		int batchCount;
		MessageOutgoingQueue<T[]> outgoing;
		TargetBuffer<T[]> targets = new TargetBuffer<T[]> ();
		DataflowMessageHeader headers = DataflowMessageHeader.NewValid ();
		SpinLock batchLock;

		public BatchBlock (int batchSize) : this (batchSize, defaultOptions)
		{

		}

		public BatchBlock (int batchSize, DataflowBlockOptions dataflowBlockOptions)
		{
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");

			this.batchSize = batchSize;
			this.dataflowBlockOptions = dataflowBlockOptions;
			this.compHelper = CompletionHelper.GetNew (dataflowBlockOptions);
			this.messageBox = new PassingMessageBox<T> (messageQueue, compHelper, () => outgoing.IsCompleted, BatchProcess, dataflowBlockOptions);
			this.outgoing = new MessageOutgoingQueue<T[]> (compHelper, () => messageQueue.IsCompleted);
			this.vault = new MessageVault<T[]> ();
		}

		public DataflowMessageStatus OfferMessage (DataflowMessageHeader messageHeader,
		                                           T messageValue,
		                                           ISourceBlock<T> source,
		                                           bool consumeToAccept)
		{
			return messageBox.OfferMessage (this, messageHeader, messageValue, source, consumeToAccept);
		}

		public IDisposable LinkTo (ITargetBlock<T[]> target, bool unlinkAfterOne)
		{
			var result = targets.AddTarget (target, unlinkAfterOne);
			outgoing.ProcessForTarget (target, this, false, ref headers);

			return result;
		}

		public T[] ConsumeMessage (DataflowMessageHeader messageHeader, ITargetBlock<T[]> target, out bool messageConsumed)
		{
			return vault.ConsumeMessage (messageHeader, target, out messageConsumed);
		}

		public void ReleaseReservation (DataflowMessageHeader messageHeader, ITargetBlock<T[]> target)
		{
			vault.ReleaseReservation (messageHeader, target);
		}

		public bool ReserveMessage (DataflowMessageHeader messageHeader, ITargetBlock<T[]> target)
		{
			return vault.ReserveMessage (messageHeader, target);
		}

		public bool TryReceive (Predicate<T[]> filter, out T[] item)
		{
			return outgoing.TryReceive (filter, out item);
		}

		public bool TryReceiveAll (out IList<T[]> items)
		{
			return outgoing.TryReceiveAll (out items);
		}

		public void TriggerBatch ()
		{
			int earlyBatchSize;
			do {
				earlyBatchSize = batchCount;
				if (earlyBatchSize == 0)
					return;
			} while (Interlocked.CompareExchange (ref batchCount, 0, earlyBatchSize) != earlyBatchSize);

			MakeBatch (earlyBatchSize);
		}

		void BatchProcess ()
		{
			// has to deal correctly with concurrent TriggerBatch

			int current;
			int previousCount;
			do {
				previousCount = batchCount;
				current = previousCount + 1;

				if (current == batchSize)
					current = 0;
			} while (Interlocked.CompareExchange (ref batchCount, current, previousCount)
			         != previousCount);

			if (current == 0)
				MakeBatch (batchSize);
		}

		void MakeBatch (int size)
		{
			T[] batch = new T[size];

			// lock is necessary here to make sure items are in the correct order
			bool taken = false;
			try {
				batchLock.Enter (ref taken);

				for (int i = 0; i < size; ++i)
					messageQueue.TryTake (out batch[i]);
			} finally {
				if (taken)
					batchLock.Exit();
			}

			var target = targets.Current;
			if (target == null)
				outgoing.AddData (batch);
			else
				target.OfferMessage (headers.Increment (), batch, this, false);

			if (!outgoing.IsEmpty && targets.Current != null)
				outgoing.ProcessForTarget (targets.Current, this, false, ref headers);
		}

		public void Complete ()
		{
			messageBox.Complete ();
		}

		public void Fault (Exception ex)
		{
			compHelper.Fault (ex);
		}

		public Task Completion {
			get {
				return compHelper.Completion;
			}
		}

		public int OutputCount {
			get {
				return outgoing.Count;
			}
		}

		public override string ToString ()
		{
			return NameHelper.GetName (this, dataflowBlockOptions);
		}
	}
}

