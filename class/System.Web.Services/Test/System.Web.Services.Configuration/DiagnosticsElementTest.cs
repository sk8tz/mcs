//
// System.Web.Services.Configuration.DiagnosticsElementTest.cs - Unit tests
// for System.Web.Services.Configuration.DiagnosticsElement
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
//

#if NET_2_0

using System;
using System.Web.Services.Configuration;
using NUnit.Framework;

namespace MonoTests.System.Web.Services {
	[TestFixture]
	public class DiagnosticsElementTest
	{
		[Test]
		public void Defaults ()
		{
			DiagnosticsElement el = new DiagnosticsElement ();

			Assert.IsFalse (el.SuppressReturningExceptions, "A1");
		}

		[Test]
		public void GetSet ()
		{
			DiagnosticsElement el = new DiagnosticsElement ();

			el.SuppressReturningExceptions = true;
			Assert.IsTrue (el.SuppressReturningExceptions, "A1");
			el.SuppressReturningExceptions = false;
			Assert.IsFalse (el.SuppressReturningExceptions, "A2");
		}
	}
}

#endif
