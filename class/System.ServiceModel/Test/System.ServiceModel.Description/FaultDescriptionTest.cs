//
// FaultDescriptionTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc.  http://www.novell.com
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
using System;
using System.Collections.ObjectModel;
using System.Net.Security;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class FaultDescriptionTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullAction ()
		{
			new FaultDescription (null);
		}

		[Test]
		public void SimpleUse ()
		{
			var cd = ContractDescription.GetContract (typeof (ITestContract));
			var od = cd.Operations [0];
			Assert.AreEqual (1, od.Faults.Count, "#1");
			var fd = od.Faults [0];
			// automatically filled names
			Assert.AreEqual ("http://tempuri.org/ITestContract/EchoMyDetailFault", fd.Action, "#2");
			Assert.AreEqual ("MyDetailFault", fd.Name, "#3");
			Assert.AreEqual ("http://tempuri.org/", fd.Namespace, "#4");
		}

		class MyDetail
		{
		}

		[ServiceContract]
		interface ITestContract
		{
			[OperationContract]
			[FaultContract (typeof (MyDetail))]
			string Echo (string input);
		}
	}
}
