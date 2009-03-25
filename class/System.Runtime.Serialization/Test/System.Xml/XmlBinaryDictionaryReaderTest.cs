//
// XmlSimpleDictionaryReaderTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com

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
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlBinaryDictionaryReaderTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullQuotas ()
		{
			XmlDictionaryReader.CreateBinaryReader (usecase1,null);
		}

		[Test]
		public void UseCase1 ()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-16""?><root a=""""><!---->     <AAA xmlns=""urn:AAA""></AAA><ePfix:AAA xmlns:ePfix=""urn:AAABBB""></ePfix:AAA><AAA>CCC" + "\u3005\u4E00" + @"CCCAAA&amp;AAADDD&amp;DDD" + '\u4E01' + @"<!--COMMENT--></AAA><AAA BBB=""bbb"" pfix:BBB=""bbbbb"" xml:lang=""ja"" xml:space=""preserve"" xml:base=""local:hogehoge"" xmlns:pfix=""urn:bbb"">CCCICAg/4Aw</AAA></root>";

			XmlDictionaryReader reader =
				XmlDictionaryReader.CreateBinaryReader (usecase1,new XmlDictionaryReaderQuotas ());
			StringWriter sw = new StringWriter ();
			XmlWriter xw = XmlWriter.Create (sw);
			reader.Read ();
			while (!reader.EOF)
				xw.WriteNode (reader, false);
			xw.Close ();
			Assert.AreEqual (xml, sw.ToString ());
		}

		// $ : kind
		// ! : length
		static readonly byte [] usecase1 = new byte [] {
			// $!root$!  a....!__  ___.!AAA  $!urn:AA  A$$!ePfi
			0x40, 0x04, 0x72, 0x6F, 0x6F, 0x74, 0x04, 0x01,
			0x61, 0xA8, 0x02, 0x00, 0x98, 0x05, 0x20, 0x20,
			0x20, 0x20, 0x20, 0x40, 0x03, 0x41, 0x41, 0x41,
			0x08, 0x07, 0x75, 0x72, 0x6E, 0x3A, 0x41, 0x41,
			0x41, 0x01, 0x41, 0x05, 0x65, 0x50, 0x66, 0x69,// 40
			// x!AAA$!e  Pfix!urn  :AAABBB$  $!AAA$!C  CC......
			0x78, 0x03, 0x41, 0x41, 0x41, 0x09, 0x05, 0x65,
			0x50, 0x66, 0x69, 0x78, 0x0A, 0x75, 0x72, 0x6E,
			0x3A, 0x41, 0x41, 0x41, 0x42, 0x42, 0x42, 0x01,
			0x40, 0x03, 0x41, 0x41, 0x41, 0x98, 0x0C, 0x43,
			0x43, 0x43, 0xE3, 0x80, 0x85, 0xE4, 0xB8, 0x80,// 80
			// AAA$!DDD  $AAA$!DD  D$DDD...  ..$!COMM  ENT$$!AA
			0x43, 0x43, 0x43, 0x98, 0x07, 0x41, 0x41, 0x41,
			0x26, 0x41, 0x41, 0x41, 0x98, 0x07, 0x44, 0x44,
			0x44, 0x26, 0x44, 0x44, 0x44, 0x98, 0x03, 0xE4,
			0xB8, 0x81, 0x02, 0x07, 0x43, 0x4F, 0x4D, 0x4D,
			0x45, 0x4E, 0x54, 0x01, 0x40, 0x03, 0x41, 0x41,// 120
			// A$!BBB$!  bbb$!pfi  x!BBB$!b  bbbb$!xm  l!lang$!
			0x41, 0x04, 0x03, 0x42, 0x42, 0x42, 0x98, 0x03,
			0x62, 0x62, 0x62, 0x05, 0x04, 0x70, 0x66, 0x69,
			0x78, 0x03, 0x42, 0x42, 0x42, 0x98, 0x05, 0x62,
			0x62, 0x62, 0x62, 0x62, 0x05, 0x03, 0x78, 0x6D,
			0x6C, 0x04, 0x6C, 0x61, 0x6E, 0x67, 0x98, 0x02,// 160
			// ja$!xml!  space$!p  reserve
			0x6A, 0x61, 0x05, 0x03, 0x78, 0x6D, 0x6C, 0x05,
			0x73, 0x70, 0x61, 0x63, 0x65, 0x98, 0x08, 0x70,
			0x72, 0x65, 0x73, 0x65, 0x72, 0x76, 0x65, 0x05,
			0x03, 0x78, 0x6D, 0x6C, 0x04, 0x62, 0x61, 0x73,
			0x65, 0x98, 0x0E, 0x6C, 0x6F, 0x63, 0x61, 0x6C,// 200
			// ..hogehog  e$!pfix!  urn:bbb$  $CCC$!BA  SE64$
			0x3A, 0x68, 0x6F, 0x67, 0x65, 0x68, 0x6F, 0x67,
			0x65, 0x09, 0x04, 0x70, 0x66, 0x69, 0x78, 0x07,
			0x75, 0x72, 0x6E, 0x3A, 0x62, 0x62, 0x62, 0x98,
			0x03, 0x43, 0x43, 0x43, 0x9F, 0x06, 0x20, 0x20,
			0x20, 0xFF, 0x80, 0x30, 0x01,
			};

		[Test]
		public void UseCase2 ()
		{
			XmlDictionary dic = new XmlDictionary ();

			dic.Add (String.Empty);
			dic.Add ("FOO");
			dic.Add ("BAR");
			dic.Add ("urn:bar");

			XmlDictionaryReader reader =
				XmlDictionaryReader.CreateBinaryReader (new MemoryStream (usecase2), dic, new XmlDictionaryReaderQuotas ());
			while (!reader.EOF)
				reader.Read ();
		}

		// $ : kind
		// / : especially. EndElement
		// ! : length
		// @ : dictionary index
		// ^ : missing ns decl?
		static readonly byte [] usecase2 = new byte [] {
			// $@$!BAR$  @$@///$@  ^@$!ppp!  $!ppp@$!  xyz$!bbb
			0x42, 2, 0x40, 3, 0x42, 0x41, 0x52, 0x42,
			2, 0x42, 4, 1, 1, 1, 0x42, 4,
			10, 6, 0x43, 3, 0x70, 0x70, 0x70, 4,
			11, 3, 0x70, 0x70, 0x70, 6, 0x99, 3,
			0x78, 0x79, 0x7A, 0x98, 4, 0x62, 0x62, 0x62,
			// b$!ccc$G  UIDGUIDG  UIDGUID$  !FOO$!GU  IDGUIDGU
			0x62, 0x98, 3, 0x63, 0x63, 0x63, 0xB1, 0x22,
			0x22, 0x11, 0x11, 0x33, 0x33, 0x44, 0x44, 0x55,
			0x55, 0x66, 0x66, 0x77, 0x77, 0x88, 0x88, 0x40,
			3, 0x46, 0x4F, 0x4F, 0x04, 3, 0x41, 0x41,
			0x41, 0xB0, 0x22, 0x22, 0x11, 0x11, 0x33, 0x33,
			// IDGUIDGU  ID$!BBB$T  IMESPAN  $!CC$!UN  IQUEIDUN
			0x44, 0x44, 0x55, 0x55, 0x66, 0x66, 0x77, 0x77,
			0x88, 0x88, 0x04, 3, 0x42, 0x42, 0x42, 0xAE,
			0, 0, 0, 0, 0, 0, 0, 0,
			0x04, 2, 0x43, 0x43, 0x98, 0x2B, 0x75, 0x75,
			0x69, 0x64, 0x2D, 0x30, 0x30, 0x30, 0x30, 0x30,
			// IQUEIDUN  IQUEIDUN  IQUEIDUN  IQUEID..  .$!XX$$$!
			0x30, 0x30, 0x30, 0x2D, 0x30, 0x30, 0x30, 0x30,
			0x2D, 0x30, 0x30, 0x30, 0x30, 0x2D, 0x30, 0x30,
			0x30, 0x30, 0x2D, 0x30, 0x30, 0x30, 0x30, 0x30,
			0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x2D,
			0x31, 0x40, 2, 0x58, 0x58, 0x86, 0x85, 0x41, 2,
			// xx!aaa$!x  x!urn:xxx
			0x78, 0x78, 3, 0x61, 0x61, 0x61, 0x09, 2, 0x78,
			0x78, 0x07, 0x75, 0x72, 0x6E, 0x3A, 0x7A, 0x7A,
			0x7A, 1, 1, 1
			};

		[Test]
		public void ElementWithNS ()
		{
			byte [] bytes = new byte [] {
				0x42, 0, 10, 2, 0x98, 3, 0x61, 0x61,
				0x61, 0x42, 0, 0x42, 2, 1, 1, 1};

			XmlDictionary dic = new XmlDictionary ();
			dic.Add ("FOO");
			dic.Add ("foo");

			XmlDictionaryReader reader =
				XmlDictionaryReader.CreateBinaryReader (new MemoryStream (bytes), dic, new XmlDictionaryReaderQuotas ());
			while (!reader.EOF)
				reader.Read ();
		}

		[Test]
		public void ContainsInvalidIndex ()
		{
			byte [] bytes = new byte [] {
				0x42, 1, 10, 2, 0x98, 3, 0x61, 0x61,
				0x61, 0x42, 0, 0x42, 2, 1, 1, 1};

			XmlDictionary dic = new XmlDictionary ();
			dic.Add ("FOO");
			dic.Add ("foo");

			XmlDictionaryReader dr = XmlDictionaryReader.CreateBinaryReader (new MemoryStream (bytes), dic, new XmlDictionaryReaderQuotas ());
			try {
				dr.Read ();
				Assert.Fail ("dictionary index 1 should be regarded as invalid.");
			} catch (XmlException) {
			}
		}

		[Test]
//		[Category ("NotWorking")]
		public void Beyond128DictionaryEntries ()
		{
			XmlDictionaryString ds;
			MemoryStream ms = new MemoryStream ();
			XmlDictionary dic = new XmlDictionary ();
			for (int i = 0; i < 260; i++)
				Assert.AreEqual (i, dic.Add ("n" + i).Key, "dic");
			XmlDictionary dic2 = new XmlDictionary ();
			XmlBinaryReaderSession session = new XmlBinaryReaderSession ();
			int idx;
			for (int i = 0; i < 260; i++)
				Assert.AreEqual (i, session.Add (i, "s" + i).Key, "session");

			byte [] bytes = new byte [] {
				// so, when it went beyond 128, the index
				// becomes 2 bytes, where
				// - the first byte always becomes > 80, and
				// - the second byte becomes (n / 0x80) * 2.
				0x42, 0x80, 2, 0x0A, 0x82, 2,
				0x42, 0x85, 2, 0x0A, 0x87, 2,
				0x42, 0x88, 2, 0x0A, 0x8B, 2,
				0x42, 0x80, 4, 0x0A, 0x81, 4,
				1, 1, 1, 1};

			XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader (new MemoryStream (bytes), dic, new XmlDictionaryReaderQuotas (), session);
			Assert.IsTrue (reader.Read (), "#r1");
			Assert.AreEqual ("n128", reader.LocalName, "#l1");
			Assert.IsTrue (reader.Read (), "#r2");
			Assert.AreEqual ("s130", reader.LocalName, "#l1");
			Assert.IsTrue (reader.Read (), "#r3");
			Assert.AreEqual ("n132", reader.LocalName, "#l1");
			Assert.IsTrue (reader.Read (), "#r4");
			Assert.AreEqual ("n256", reader.LocalName, "#l1");
			for (int i = 0; i < 4; i++) {
				Assert.IsTrue (reader.Read (), "#re" + i);
				Assert.AreEqual (XmlNodeType.EndElement, reader.NodeType, "#ne" + i);
			}
			Assert.IsFalse (reader.Read ()); // EOF
		}

		[Test]
		public void GlobalAttributes ()
		{
			XmlDictionary dic = new XmlDictionary ();
			dic.Add ("n1");
			dic.Add ("urn:foo");
			dic.Add ("n2");
			dic.Add ("n3");
			dic.Add ("n4");
			dic.Add ("urn:bar");
			dic.Add ("n7");

			// 0x0C nameidx (value) 0x0D nameidx (value)
			// 0x07 (prefix) nameidx (value)
			// 0x05 (prefix) (name) (value)
			// 0x04...  0x06...  0x05...
			// 0x0A nsidx
			// 0x0B (prefix) nsidx
			// 0x0B...  0x0B...
			// 0x09 (prefix) (ns)
			byte [] bytes = new byte [] {
				// $@$@$$@$  !v$!aaa@
				// $@!bbb!n  5$$@$!a@
				// $!aaa!$!  bbb$urn:foo$
				0x42, 0,
				0x0C, 4, 0xA8,
				0x0D, 6, 0x98, 1, 0x76,
				0x07, 3, 0x61, 0x61, 0x61, 8, 0xA8, // 16
				0x05, 3, 0x62, 0x62, 0x62, 2, 0x6E, 0x35, 0xA8,
				0x04, 2, 0x6E, 0x36, 0xA8, // 30
				0x06, 12, 0xA8,
				0x05, 3, 0x62, 0x62, 0x62, 2, 0x6E, 0x38, 0xA8,
				0x0A, 2,
				0x0B, 1, 0x61, 10, // 48
				0x0B, 1, 0x62, 2,
				0x0B, 3, 0x61, 0x61, 0x61, 10,
				0x09, 3, 0x62, 0x62, 0x62,
				0x07, 0x75, 0x72, 0x6E, 0x3A, 0x66, 0x6F, 0x6F,
				1};

			XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader (new MemoryStream (bytes), dic, new XmlDictionaryReaderQuotas ());
			while (!reader.EOF)
				reader.Read ();
		}

		[Test]
		public void AttributeXmlns ()
		{
			// equivalent to WriteXmlnsAttribute()
			XmlDictionaryString ds;
			XmlDictionary dic = new XmlDictionary ();
			dic.Add ("xmlns");
			dic.Add ("http://www.w3.org/2000/xmlns/");

			byte [] bytes = new byte [] {
				// 40 (root) 04 (a) A8
				// 09 (foo) (urn:foo) 08 (urn:bar)
				0x40, 4, 0x72, 0x6F, 0x6F, 0x74,
				0x04, 1, 0x61, 0xA8,
				0x09, 3, 0x66, 0x6F, 0x6F, 7, 0x75, 0x72, 0x6E, 0x3A, 0x66, 0x6F, 0x6F,
				0x08, 7, 0x75, 0x72, 0x6E, 0x3A, 0x62, 0x61, 0x72, 1
				};

			XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader (new MemoryStream (bytes), dic, new XmlDictionaryReaderQuotas ());
			while (!reader.EOF)
				reader.Read ();
		}

		[Test]
		public void ReadTypedValues ()
		{
			XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader (new MemoryStream (typed_values), new XmlDictionaryReaderQuotas ());
			while (!reader.EOF)
				reader.Read ();
		}

		byte [] typed_values = new byte [] {
			0x40, 4, 0x72, 0x6F, 0x6F, 0x74,
			0x88, 5,
			0x8A, 0x7D, 0x03,
			0x8C, 0xBC, 0x92, 0, 0, // int
			0x8C, 0x2C, 0xEB, 0x6D, 0x08, // 20
			0x8E, 0x80, 0x55, 0xF5, 0x51, 0x01, 0, 0, 0,
			0x90, 0xD7, 0xB3, 0xDD, 0x3F, // float
			0x92, 0x4C, 0x15, 0x31, 0x91, 0x77, 0xE3, 0x01, 0x40, // 43
			0x94, 0, 0, 6, 0, 0, 0, 0, 0, 0xD8, 0xEF, 0x2F, 0, 0, 0, 0, 0,
			0x97, 0x80, 0x40, 0xA3, 0x29, 0xE5, 0x22, 0xC1, 8
			};
	}
}
