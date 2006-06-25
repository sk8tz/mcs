//
// Tests for System.Web.UI.WebControls.DataKeyTest.cs
//
// Author:
//	Yoni Klein (yonik@mainsoft.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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


#if NET_2_0

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;
using System.Data;
using System.ComponentModel;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]
	public class DataKeyTest
	{
		// Keys and values can be assigned only from constractor 

		[Test]
		public void DataKey_Functionality()
		{
			OrderedDictionary  dictionary = new OrderedDictionary ();
			IOrderedDictionary iDictionary;
			dictionary.Add ("key", "value");
			DataKey key = new DataKey (dictionary);
			Assert.AreEqual ("value", key[0].ToString(), "DataKeyItemIndex");
			Assert.AreEqual ("value", key["key"].ToString (), "DataKeyItemKeyName");
			Assert.AreEqual ("value", key.Value, "FirstIndexValue");
			iDictionary = key.Values;
			Assert.AreEqual (1, iDictionary.Count, "AllValuesReferringToKey");
			Assert.AreEqual ("value", iDictionary[0], "ValueReferringToKey");
			dictionary.Add("key1", "value1");
			key = new DataKey (dictionary);
			iDictionary = key.Values;
			Assert.AreEqual (2, iDictionary.Count, "AllValuesReferringToKey#1");
			Assert.AreEqual ("value1", iDictionary[1], "ValueReferringToKey#1");
		}
	}
}
#endif