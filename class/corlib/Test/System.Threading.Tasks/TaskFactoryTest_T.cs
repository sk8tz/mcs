//
// TaskFactory_T_Test.cs
//
// Author:
//       Marek Safar <marek.safargmail.com>
//
// Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
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

#if NET_4_0

using System;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks
{
	[TestFixture]
	public class TaskFactory_T_Tests
	{
		[SetUp]
		public void Setup ()
		{
		}
		
		[Test]
		public void ConstructorTest ()
		{
			try {
				new TaskFactory<int> (TaskCreationOptions.None, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.LongRunning);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException e) {
			}

			try {
				new TaskFactory<int> (TaskCreationOptions.None, TaskContinuationOptions.OnlyOnRanToCompletion);
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				new TaskFactory<int> (TaskCreationOptions.None, TaskContinuationOptions.NotOnRanToCompletion);
				Assert.Fail ("#3");
			} catch (ArgumentOutOfRangeException) {
			}
		}
	}
}

#endif