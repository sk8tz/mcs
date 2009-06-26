//
// WebHeaderCollectionTest.cs - NUnit Test Cases for System.Net.WebHeaderCollection
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace MonoTests.System.Net
{
	[TestFixture]
	public class WebHeaderCollectionTest
	{
		WebHeaderCollection col;

		[SetUp]
		public void GetReady ()
		{
			col = new WebHeaderCollection ();
			col.Add ("Name1: Value1");
			col.Add ("Name2: Value2");
		}

		[Test]
		public void Add ()
		{
			try {
				col.Add (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) { }
			try {
				col.Add ("");
				Assert.Fail ("#2");
			} catch (ArgumentException) { }
			try {
				col.Add ("  ");
				Assert.Fail ("#3");
			} catch (ArgumentException) { }
			try {
				col.Add (":");
				Assert.Fail ("#4");
			} catch (ArgumentException) { }
			try {
				col.Add (" : ");
				Assert.Fail ("#5");
			} catch (ArgumentException) { }

			try {
				col.Add ("XHost: foo");
			} catch (ArgumentException) {
				Assert.Fail ("#7");
			}

			// invalid values
			try {
				col.Add ("XHost" + ((char) 0xa9) + ": foo");
				Assert.Fail ("#8");
			} catch (ArgumentException) { }
			try {
				col.Add ("XHost: foo" + (char) 0xa9);
			} catch (ArgumentException) {
				Assert.Fail ("#9");
			}
			try {
				col.Add ("XHost: foo" + (char) 0x7f);
				Assert.Fail ("#10");
			} catch (ArgumentException) {

			}

			try {
				col.Add ("XHost", null);
			} catch (ArgumentException) {
				Assert.Fail ("#11");
			}
			try {
				col.Add ("XHost:");
			} catch (ArgumentException) {
				Assert.Fail ("#12");
			}

			// restricted
			/*
			// this can only be tested in namespace System.Net
			try {
				WebHeaderCollection col2 = new WebHeaderCollection (true);
				col2.Add ("Host: foo");
				Assert.Fail ("#13: should fail according to spec");
			} catch (ArgumentException) {}		
			*/
		}

		[Test]
		[Category ("NotWorking")]
		public void GetValues ()
		{
			WebHeaderCollection w = new WebHeaderCollection ();
			w.Add ("Hello", "H1");
			w.Add ("Hello", "H2");
			w.Add ("Hello", "H3,H4");

			string [] sa = w.GetValues ("Hello");
			Assert.AreEqual (3, sa.Length, "#1");
			Assert.AreEqual ("H1, H2,H3,H4", w.Get ("Hello"), "#2");

			w = new WebHeaderCollection ();
			w.Add ("Accept", "H1");
			w.Add ("Accept", "H2");
			w.Add ("Accept", "H3,H4");
			Assert.AreEqual (3, w.GetValues (0).Length, "#3a");
			Assert.AreEqual (4, w.GetValues ("Accept").Length, "#3b");
			Assert.AreEqual ("H1, H2,H3,H4", w.Get ("Accept"), "#4");

			w = new WebHeaderCollection ();
			w.Add ("Allow", "H1");
			w.Add ("Allow", "H2");
			w.Add ("Allow", "H3,H4");
			sa = w.GetValues ("Allow");
			Assert.AreEqual (4, sa.Length, "#5");
			Assert.AreEqual ("H1, H2,H3,H4", w.Get ("Allow"), "#6");

			w = new WebHeaderCollection ();
			w.Add ("AUTHorization", "H1, H2, H3");
			sa = w.GetValues ("authorization");
			Assert.AreEqual (3, sa.Length, "#9");

			w = new WebHeaderCollection ();
			w.Add ("proxy-authenticate", "H1, H2, H3");
			sa = w.GetValues ("Proxy-Authenticate");
			Assert.AreEqual (3, sa.Length, "#9");

			w = new WebHeaderCollection ();
			w.Add ("expect", "H1,\tH2,   H3  ");
			sa = w.GetValues ("EXPECT");
			Assert.AreEqual (3, sa.Length, "#10");
			Assert.AreEqual ("H2", sa [1], "#11");
			Assert.AreEqual ("H3", sa [2], "#12");

			try {
				w.GetValues (null);
				Assert.Fail ("#13");
			} catch (ArgumentNullException) { }
			Assert.AreEqual (null, w.GetValues (""), "#14");
			Assert.AreEqual (null, w.GetValues ("NotExistent"), "#15");
		}

		[Test]
		public void Indexers ()
		{
#if NET_2_0
		Assert.AreEqual ("Value1", ((NameValueCollection)col)[0], "#1.1");
		//FIXME: test also HttpRequestHeader and HttpResponseHeader
#else
			Assert.AreEqual ("Value1", col [0], "#1");
#endif
			Assert.AreEqual ("Value1", col ["Name1"], "#2");
			Assert.AreEqual ("Value1", col ["NAME1"], "#3");
		}

		[Test]
		public void Remove ()
		{
			col.Remove ("Name1");
			col.Remove ("NameNotExist");
			Assert.AreEqual (1, col.Count, "#1");

			/*
			// this can only be tested in namespace System.Net
			try {
				WebHeaderCollection col2 = new WebHeaderCollection (true);
				col2.Add ("Host", "foo");
				col2.Remove ("Host");
				Assert.Fail ("#2: should fail according to spec");
			} catch (ArgumentException) {}
			*/
		}

		[Test]
		public void Set ()
		{
			col.Add ("Name1", "Value1b");
			col.Set ("Name1", "\t  X  \t");
			Assert.AreEqual ("X", col.Get ("Name1"), "#1");
		}

		[Test]
		public void IsRestricted ()
		{
			Assert.IsTrue (!WebHeaderCollection.IsRestricted ("Xhost"), "#1");
			Assert.IsTrue (WebHeaderCollection.IsRestricted ("Host"), "#2");
			Assert.IsTrue (WebHeaderCollection.IsRestricted ("HOST"), "#3");
			Assert.IsTrue (WebHeaderCollection.IsRestricted ("Transfer-Encoding"), "#4");
			Assert.IsTrue (WebHeaderCollection.IsRestricted ("user-agent"), "#5");
			Assert.IsTrue (WebHeaderCollection.IsRestricted ("accept"), "#6");
			Assert.IsTrue (!WebHeaderCollection.IsRestricted ("accept-charset"), "#7");
		}

		[Test]
		public void ToStringTest ()
		{
			col.Add ("Name1", "Value1b");
			col.Add ("Name3", "Value3a\r\n Value3b");
			col.Add ("Name4", "   Value4   ");
			Assert.AreEqual ("Name1: Value1,Value1b\r\nName2: Value2\r\nName3: Value3a\r\n Value3b\r\nName4: Value4\r\n\r\n", col.ToString (), "#1");
		}

		[Test]
#if TARGET_JVM
		//FIXME: include Java serialization compliant tests - the order of object
		// in SerializationInfo should stay same to MS format...
		[Ignore ("The MS compliant binary serialization is not supported")]
#endif			
		public void GetObjectData ()
		{
			SerializationInfo si = new SerializationInfo (typeof (WebHeaderCollection),
				new FormatterConverter ());

			WebHeaderCollection headers = new WebHeaderCollection ();
			headers.Add ("Content-Type", "image/png");
			headers.Add ("No-Cache:off");
			headers.Add ("Disposition", "attach");

			((ISerializable) headers).GetObjectData (si, new StreamingContext ());
			Assert.AreEqual (7, si.MemberCount, "#A");
			int i = 0;
			foreach (SerializationEntry entry in si) {
				Assert.IsNotNull (entry.Name, "#B1:" + i);
				Assert.IsNotNull (entry.ObjectType, "#B2:" + i);
				Assert.IsNotNull (entry.Value, "#B3:" + i);

				switch (i) {
				case 0:
					Assert.AreEqual ("Count", entry.Name, "#B4:" + i);
					Assert.AreEqual (typeof (int), entry.ObjectType, "#B5:" + i);
					Assert.AreEqual (3, entry.Value, "#B6:" + i);
					break;
				case 1:
					Assert.AreEqual ("0", entry.Name, "#B4:" + i);
					Assert.AreEqual (typeof (string), entry.ObjectType, "#B5:" + i);
					Assert.AreEqual ("Content-Type", entry.Value, "#B6:" + i);
					break;
				case 2:
					Assert.AreEqual ("3", entry.Name, "#B4:" + i);
					Assert.AreEqual (typeof (string), entry.ObjectType, "#B5:" + i);
					Assert.AreEqual ("image/png", entry.Value, "#B6:" + i);
					break;
				case 3:
					Assert.AreEqual ("1", entry.Name, "#B4:" + i);
					Assert.AreEqual (typeof (string), entry.ObjectType, "#B5:" + i);
					Assert.AreEqual ("No-Cache", entry.Value, "#B6:" + i);
					break;
				case 4:
					Assert.AreEqual ("4", entry.Name, "#B4:" + i);
					Assert.AreEqual (typeof (string), entry.ObjectType, "#B5:" + i);
					Assert.AreEqual ("off", entry.Value, "#B6:" + i);
					break;
				case 5:
					Assert.AreEqual ("2", entry.Name, "#B4:" + i);
					Assert.AreEqual (typeof (string), entry.ObjectType, "#B5:" + i);
					Assert.AreEqual ("Disposition", entry.Value, "#B6:" + i);
					break;
				case 6:
					Assert.AreEqual ("5", entry.Name, "#B4:" + i);
					Assert.AreEqual (typeof (string), entry.ObjectType, "#B5:" + i);
					Assert.AreEqual ("attach", entry.Value, "#B6:" + i);
					break;
				}
				i++;
			}
		}

		[Test]
#if TARGET_JVM
		//FIXME: include Java serialization compliant tests
		[Ignore ("The MS compliant binary serialization is not supported")]
#endif		
		public void Serialize ()
		{
			WebHeaderCollection headers = new WebHeaderCollection ();
			headers.Add ("Content-Type", "image/png");
			headers.Add ("No-Cache:off");
			headers.Add ("Disposition", "attach");

			BinaryFormatter bf = new BinaryFormatter ();
			bf.AssemblyFormat = FormatterAssemblyStyle.Full;

			MemoryStream ms = new MemoryStream ();
			bf.Serialize (ms, headers);
			ms.Position = 0;

			byte [] buffer = new byte [ms.Length];
			ms.Read (buffer, 0, buffer.Length);
			Assert.AreEqual (_serialized, buffer);
		}

		[Test]
#if TARGET_JVM
		//FIXME: include Java serialization compliant tests
		[Ignore ("The MS compliant binary serialization format is not supported")]
#endif				
		public void Deserialize ()
		{
			MemoryStream ms = new MemoryStream ();
			ms.Write (_serialized, 0, _serialized.Length);
			ms.Position = 0;

			BinaryFormatter bf = new BinaryFormatter ();
			WebHeaderCollection headers = (WebHeaderCollection) bf.Deserialize (ms);
		}

		private static readonly byte [] _serialized = new byte [] {
#if NET_2_0
			0x00, 0x01, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0c, 0x02, 0x00, 0x00, 0x00,
			0x49, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2c, 0x20, 0x56, 0x65,
			0x72, 0x73, 0x69, 0x6f, 0x6e, 0x3d, 0x32, 0x2e, 0x30, 0x2e, 0x30,
			0x2e, 0x30, 0x2c, 0x20, 0x43, 0x75, 0x6c, 0x74, 0x75, 0x72, 0x65,
			0x3d, 0x6e, 0x65, 0x75, 0x74, 0x72, 0x61, 0x6c, 0x2c, 0x20, 0x50,
			0x75, 0x62, 0x6c, 0x69, 0x63, 0x4b, 0x65, 0x79, 0x54, 0x6f, 0x6b,
			0x65, 0x6e, 0x3d, 0x62, 0x37, 0x37, 0x61, 0x35, 0x63, 0x35, 0x36,
			0x31, 0x39, 0x33, 0x34, 0x65, 0x30, 0x38, 0x39, 0x05, 0x01, 0x00,
			0x00, 0x00, 0x1e, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e, 0x4e,
			0x65, 0x74, 0x2e, 0x57, 0x65, 0x62, 0x48, 0x65, 0x61, 0x64, 0x65,
			0x72, 0x43, 0x6f, 0x6c, 0x6c, 0x65, 0x63, 0x74, 0x69, 0x6f, 0x6e,
			0x07, 0x00, 0x00, 0x00, 0x05, 0x43, 0x6f, 0x75, 0x6e, 0x74, 0x01,
			0x30, 0x01, 0x33, 0x01, 0x31, 0x01, 0x34, 0x01, 0x32, 0x01, 0x35,
			0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x08, 0x02, 0x00, 0x00,
			0x00, 0x03, 0x00, 0x00, 0x00, 0x06, 0x03, 0x00, 0x00, 0x00, 0x0c,
			0x43, 0x6f, 0x6e, 0x74, 0x65, 0x6e, 0x74, 0x2d, 0x54, 0x79, 0x70,
			0x65, 0x06, 0x04, 0x00, 0x00, 0x00, 0x09, 0x69, 0x6d, 0x61, 0x67,
			0x65, 0x2f, 0x70, 0x6e, 0x67, 0x06, 0x05, 0x00, 0x00, 0x00, 0x08,
			0x4e, 0x6f, 0x2d, 0x43, 0x61, 0x63, 0x68, 0x65, 0x06, 0x06, 0x00,
			0x00, 0x00, 0x03, 0x6f, 0x66, 0x66, 0x06, 0x07, 0x00, 0x00, 0x00,
			0x0b, 0x44, 0x69, 0x73, 0x70, 0x6f, 0x73, 0x69, 0x74, 0x69, 0x6f,
			0x6e, 0x06, 0x08, 0x00, 0x00, 0x00, 0x06, 0x61, 0x74, 0x74, 0x61,
			0x63, 0x68, 0x0b
#else
			0x00, 0x01, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0x01, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0c, 0x02, 0x00, 0x00, 0x00,
			0x4c, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2c, 0x20, 0x56, 0x65,
			0x72, 0x73, 0x69, 0x6f, 0x6e, 0x3d, 0x31, 0x2e, 0x30, 0x2e, 0x35,
			0x30, 0x30, 0x30, 0x2e, 0x30, 0x2c, 0x20, 0x43, 0x75, 0x6c, 0x74,
			0x75, 0x72, 0x65, 0x3d, 0x6e, 0x65, 0x75, 0x74, 0x72, 0x61, 0x6c,
			0x2c, 0x20, 0x50, 0x75, 0x62, 0x6c, 0x69, 0x63, 0x4b, 0x65, 0x79,
			0x54, 0x6f, 0x6b, 0x65, 0x6e, 0x3d, 0x62, 0x37, 0x37, 0x61, 0x35,
			0x63, 0x35, 0x36, 0x31, 0x39, 0x33, 0x34, 0x65, 0x30, 0x38, 0x39,
			0x05, 0x01, 0x00, 0x00, 0x00, 0x1e, 0x53, 0x79, 0x73, 0x74, 0x65,
			0x6d, 0x2e, 0x4e, 0x65, 0x74, 0x2e, 0x57, 0x65, 0x62, 0x48, 0x65,
			0x61, 0x64, 0x65, 0x72, 0x43, 0x6f, 0x6c, 0x6c, 0x65, 0x63, 0x74,
			0x69, 0x6f, 0x6e, 0x07, 0x00, 0x00, 0x00, 0x05, 0x43, 0x6f, 0x75,
			0x6e, 0x74, 0x01, 0x30, 0x01, 0x33, 0x01, 0x31, 0x01, 0x34, 0x01,
			0x32, 0x01, 0x35, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x08,
			0x02, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x06, 0x03, 0x00,
			0x00, 0x00, 0x0c, 0x43, 0x6f, 0x6e, 0x74, 0x65, 0x6e, 0x74, 0x2d,
			0x54, 0x79, 0x70, 0x65, 0x06, 0x04, 0x00, 0x00, 0x00, 0x09, 0x69,
			0x6d, 0x61, 0x67, 0x65, 0x2f, 0x70, 0x6e, 0x67, 0x06, 0x05, 0x00,
			0x00, 0x00, 0x08, 0x4e, 0x6f, 0x2d, 0x43, 0x61, 0x63, 0x68, 0x65,
			0x06, 0x06, 0x00, 0x00, 0x00, 0x03, 0x6f, 0x66, 0x66, 0x06, 0x07,
			0x00, 0x00, 0x00, 0x0b, 0x44, 0x69, 0x73, 0x70, 0x6f, 0x73, 0x69,
			0x74, 0x69, 0x6f, 0x6e, 0x06, 0x08, 0x00, 0x00, 0x00, 0x06, 0x61,
			0x74, 0x74, 0x61, 0x63, 0x68, 0x0b
#endif
		};
	}
}
