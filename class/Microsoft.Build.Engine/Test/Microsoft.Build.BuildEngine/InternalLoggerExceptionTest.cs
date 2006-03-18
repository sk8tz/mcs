//
// InternalLoggerExceptionTest.cs:
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2005 Marek Sieradzki
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using Microsoft.Build.BuildEngine;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class InternalLoggerExceptionTest {
		[Test]
		[ExpectedException (typeof (System.InvalidOperationException),
		"An InternalLoggerException can only be thrown by the MSBuild engine." +
		" The public constructors of this class cannot be used to create an instance of the exception.")]
		public void TestCtorMessage ()
		{
			InternalLoggerException ile;
			string message = "message";
			
			ile = new InternalLoggerException (message);
		}
		
		[Test]
		[ExpectedException (typeof (System.InvalidOperationException),
		"An InternalLoggerException can only be thrown by the MSBuild engine." +
		" The public constructors of this class cannot be used to create an instance of the exception.")]
		public void TestCtorMessageException ()
		{
			InternalLoggerException ile;
			string message = "message";
			Exception e = new Exception ("Inner exception message.");
			
			ile = new InternalLoggerException (message, e);
		}
	}
}
