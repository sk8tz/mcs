//
// CryptographicAttributeTest.cs - NUnit tests for CryptographicAttribute
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;

using System;
using System.Collections;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class CryptographicAttributeTest : Assertion {

		static string defaultOid = "1.2.840.113549.1.7.1";
		static string defaultName = "PKCS 7 Data";

		[Test]
		public void ConstructorOid () 
		{
			Oid o = new Oid (defaultOid);
			CryptographicAttribute ca = new CryptographicAttribute (o);
			AssertEquals ("Oid.FriendlyName", defaultName, ca.Oid.FriendlyName);
			AssertEquals ("Oid.Value", defaultOid, ca.Oid.Value);
			AssertEquals ("Values", 0, ca.Values.Count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOidNull () 
		{
			CryptographicAttribute ca = new CryptographicAttribute (null);
		}

		[Test]
		public void ConstructorOidCollection () 
		{
			Oid o = new Oid (defaultOid);
			AsnEncodedDataCollection coll = new AsnEncodedDataCollection ();
			CryptographicAttribute ca = new CryptographicAttribute (o, coll);
			AssertEquals ("Oid.FriendlyName", defaultName, ca.Oid.FriendlyName);
			AssertEquals ("Oid.Value", defaultOid, ca.Oid.Value);
			AssertEquals ("Values", 0, ca.Values.Count);
			coll.Add (new AsnEncodedData (new byte [0]));
			AssertEquals ("Values", 1, ca.Values.Count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOidNullCollection ()
		{
			AsnEncodedDataCollection coll = new AsnEncodedDataCollection ();
			CryptographicAttribute ca = new CryptographicAttribute (null, coll);
		}

		[Test]
		public void ConstructorOidAsnEncodedDataCollectionNull ()
		{
			Oid o = new Oid (defaultOid);
			AsnEncodedDataCollection coll = null;
			CryptographicAttribute ca = new CryptographicAttribute (o, coll);
			AssertEquals ("Oid.FriendlyName", defaultName, ca.Oid.FriendlyName);
			AssertEquals ("Oid.Value", defaultOid, ca.Oid.Value);
			AssertEquals ("Values", 0, ca.Values.Count);
		}
	}
}

#endif
