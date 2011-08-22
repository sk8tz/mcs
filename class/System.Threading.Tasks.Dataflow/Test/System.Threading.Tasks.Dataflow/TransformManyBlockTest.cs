#if NET_4_0
// 
// TransformManyBlockTest.cs
//  
// Author:
//       Jérémie "garuma" Laval <jeremie.laval@gmail.com>
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

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow
{
	[TestFixture]
	public class TransformManyBlockTest
	{
		[Test]
		public void BasicUsageTest ()
		{
			int insIndex = -1;
			int[] array = new int[5 + 3];

			var block = new ActionBlock<int> (i => array[Interlocked.Increment (ref insIndex)] = i);
			var trsm = new TransformManyBlock<int, int> (i => Enumerable.Range (0, i));
			trsm.LinkTo (block);

			trsm.Post (5);
			trsm.Post (3);

			Thread.Sleep (1600);
			
			CollectionAssert.AreEquivalent (new int[] { 0, 1, 2, 3, 4, 0, 1, 2 }, array);
		}

		[Test]
		public void DeferredUsageTest ()
		{
			int insIndex = -1;
			int[] array = new int[5 + 3];

			var block = new ActionBlock<int> (i => array[Interlocked.Increment (ref insIndex)] = i);
			var trsm = new TransformManyBlock<int, int> (i => Enumerable.Range (0, i));

			trsm.Post (5);
			trsm.Post (3);

			Thread.Sleep (1600);
			trsm.LinkTo (block);
			Thread.Sleep (500);

			CollectionAssert.AreEquivalent (new int[] { 0, 1, 2, 3, 4, 0, 1, 2 }, array);
		}
	}
}
#endif
