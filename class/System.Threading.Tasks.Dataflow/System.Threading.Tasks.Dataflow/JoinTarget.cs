// JoinBlock.cs
//
// Copyright (c) 2011 J�r�mie "garuma" Laval
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

using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow {
	internal class JoinTarget<TTarget> : MessageBox<TTarget>, ITargetBlock<TTarget>
	{
		readonly IDataflowBlock joinBlock;
		readonly Action signal;

		public JoinTarget (
			IDataflowBlock joinBlock, Action signal, CompletionHelper helper,
			Func<bool> externalCompleteTester, DataflowBlockOptions options,
			bool greedy)
			: base (null, new BlockingCollection<TTarget> (), helper, externalCompleteTester,
				options, greedy)
		{
			this.joinBlock = joinBlock;
			this.signal = signal;
			Target = this;
		}

		protected override void EnsureProcessing (bool newItem)
		{
			signal ();
		}

		public BlockingCollection<TTarget> Buffer {
			get {
				return MessageQueue;
			}
		}

		DataflowMessageStatus ITargetBlock<TTarget>.OfferMessage (DataflowMessageHeader messageHeader,
		                                                          TTarget messageValue,
		                                                          ISourceBlock<TTarget> source,
		                                                          bool consumeToAccept)
		{
			return OfferMessage (messageHeader, messageValue, source, consumeToAccept);
		}

		void IDataflowBlock.Complete ()
		{
			Complete ();
		}

		Task IDataflowBlock.Completion {
			get {
				throw new NotSupportedException();
			}
		}

		void IDataflowBlock.Fault (Exception e)
		{
			joinBlock.Fault (e);
		}
	}
}