//
// System.Xml.XmlSerializerTests
//
// Author:
//   Erik LeBel <eriklebel@yahoo.ca>
//
// (C) 2003 Erik LeBel
//
//
// NOTES:
//  Where possible, these tests avoid testing the order of
//  an object's members serialization. Mono and .NET do not
//  reflect members in the same order.
//
//  Only serializations tests so far, no deserialization.
//
// FIXME
//  test XmlArrayAttribute
//  test XmlArrayItemAttribute
//  test serialization of decimal type
//  test serialization of Guid type
//  test XmlNode serialization with and without modifying attributes.
//  test deserialization
//  FIXMEs found in this file

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using NUnit.Framework;

using MonoTests.System.Xml.TestClasses;

namespace MonoTests.System.XmlSerialization
{
	[TestFixture]
	public class XmlSerializerTests
	{
		const string SoapEncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";
		const string WsdlTypesNamespace = "http://microsoft.com/wsdl/types/";
		const string ANamespace = "some:urn";
		const string AnotherNamespace = "another:urn";

		StringWriter sw;
		XmlTextWriter xtw;
		XmlSerializer xs;

		private void SetUpWriter ()
		{
			sw = new StringWriter ();
			xtw = new XmlTextWriter (sw);
			xtw.QuoteChar = '\'';
			xtw.Formatting = Formatting.None;
		}

		private string WriterText {
			get {
				string val = sw.GetStringBuilder ().ToString ();
				int offset = val.IndexOf ('>') + 1;
				val = val.Substring (offset);
				return Infoset (val);
			}
		}

		private void Serialize (object o)
		{
			SetUpWriter ();
			xs = new XmlSerializer (o.GetType ());
			xs.Serialize (xtw, o);
		}

		private void Serialize (object o, Type type)
		{
			SetUpWriter ();
			xs = new XmlSerializer (type);
			xs.Serialize (xtw, o);
		}

		private void Serialize (object o, XmlSerializerNamespaces ns)
		{
			SetUpWriter ();
			xs = new XmlSerializer (o.GetType ());
			xs.Serialize (xtw, o, ns);
		}

		private void Serialize (object o, XmlAttributeOverrides ao)
		{
			SetUpWriter ();
			xs = new XmlSerializer (o.GetType (), ao);
			xs.Serialize (xtw, o);
		}

		private void Serialize (object o, XmlAttributeOverrides ao, string defaultNamespace)
		{
			SetUpWriter ();
			xs = new XmlSerializer (o.GetType (), ao, Type.EmptyTypes,
				(XmlRootAttribute) null, defaultNamespace);
			xs.Serialize (xtw, o);
		}

		private void Serialize (object o, XmlRootAttribute root)
		{
			SetUpWriter ();
			xs = new XmlSerializer (o.GetType (), root);
			xs.Serialize (xtw, o);
		}

		private void Serialize (object o, XmlTypeMapping typeMapping)
		{
			SetUpWriter ();
			xs = new XmlSerializer (typeMapping);
			xs.Serialize (xtw, o);
		}

		private void SerializeEncoded (object o)
		{
			SerializeEncoded (o, o.GetType ());
		}

		private void SerializeEncoded (object o, SoapAttributeOverrides ao)
		{
			XmlTypeMapping mapping = CreateSoapMapping (o.GetType (), ao);
			SetUpWriter ();
			xs = new XmlSerializer (mapping);
			xs.Serialize (xtw, o);
		}

		private void SerializeEncoded (object o, SoapAttributeOverrides ao, string defaultNamespace)
		{
			XmlTypeMapping mapping = CreateSoapMapping (o.GetType (), ao, defaultNamespace);
			SetUpWriter ();
			xs = new XmlSerializer (mapping);
			xs.Serialize (xtw, o);
		}

		private void SerializeEncoded (object o, Type type)
		{
			XmlTypeMapping mapping = CreateSoapMapping (type);
			SetUpWriter ();
			xs = new XmlSerializer (mapping);
			xs.Serialize (xtw, o);
		}

		private void SerializeEncoded (XmlTextWriter xtw, object o, Type type)
		{
			XmlTypeMapping mapping = CreateSoapMapping (type);
			xs = new XmlSerializer (mapping);
			xs.Serialize (xtw, o);
		}

		// test constructors
#if USE_VERSION_1_1	// It doesn't pass on MS.NET 1.1.
		[Test]
		public void TestConstructor()
		{
			XmlSerializer ser = new XmlSerializer (null, "");
		}
#else
#endif

		// test basic types ////////////////////////////////////////////////////////
		[Test]
		public void TestSerializeInt ()
		{
			Serialize (10);
			Assert.AreEqual (Infoset ("<int>10</int>"), WriterText);
		}

		[Test]
		public void TestSerializeBool ()
		{
			Serialize (true);
			Assert.AreEqual (Infoset ("<boolean>true</boolean>"), WriterText);

			Serialize (false);
			Assert.AreEqual (Infoset ("<boolean>false</boolean>"), WriterText);
		}

		[Test]
		public void TestSerializeString ()
		{
			Serialize ("hello");
			Assert.AreEqual (Infoset ("<string>hello</string>"), WriterText);
		}

		[Test]
		public void TestSerializeEmptyString ()
		{
			Serialize (String.Empty);
			Assert.AreEqual (Infoset ("<string />"), WriterText);
		}

		[Test]
		public void TestSerializeNullObject ()
		{
			Serialize (null, typeof (object));
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<anyType xmlns:xsd='{0}' xmlns:xsi='{1}' xsi:nil='true' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)), WriterText);
		}

		[Test]
		[Ignore ("The generated XML is not exact but it is equivalent")]
		public void TestSerializeNullString ()
		{
			Serialize (null, typeof (string));
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<string xmlns:xsd='{0}' xmlns:xsi='{1}' xsi:nil='true' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)), WriterText);
		}

		[Test]
		public void TestSerializeIntArray ()
		{
			Serialize (new int [] { 1, 2, 3, 4 });
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<ArrayOfInt xmlns:xsd='{0}' xmlns:xsi='{1}'><int>1</int><int>2</int><int>3</int><int>4</int></ArrayOfInt>",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)), WriterText);
		}

		[Test]
		public void TestSerializeEmptyArray ()
		{
			Serialize (new int [] { });
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<ArrayOfInt xmlns:xsd='{0}' xmlns:xsi='{1}' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)), WriterText);
		}

		[Test]
		public void TestSerializeChar ()
		{
			Serialize ('A');
			Assert.AreEqual (Infoset ("<char>65</char>"), WriterText);

			Serialize ('\0');
			Assert.AreEqual (Infoset ("<char>0</char>"), WriterText);

			Serialize ('\n');
			Assert.AreEqual (Infoset ("<char>10</char>"), WriterText);

			Serialize ('\uFF01');
			Assert.AreEqual (Infoset ("<char>65281</char>"), WriterText);
		}

		[Test]
		public void TestSerializeFloat ()
		{
			Serialize (10.78);
			Assert.AreEqual (Infoset ("<double>10.78</double>"), WriterText);

			Serialize (-1e8);
			Assert.AreEqual (Infoset ("<double>-100000000</double>"), WriterText);

			// FIXME test INF and other boundary conditions that may exist with floats
		}

		[Test]
		public void TestSerializeEnumeration_FromValue ()
		{
			Serialize ((int) SimpleEnumeration.SECOND, typeof (SimpleEnumeration));
			Assert.AreEqual (
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<SimpleEnumeration>SECOND</SimpleEnumeration>",
				sw.ToString ());
		}

		[Test]
		[Category ("NotWorking")]
		public void TestSerializeEnumeration_FromValue_Encoded ()
		{
			SerializeEncoded ((int) SimpleEnumeration.SECOND, typeof (SimpleEnumeration));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<SimpleEnumeration d1p1:type='SimpleEnumeration' xmlns:d1p1='{0}'>SECOND</SimpleEnumeration>",
				XmlSchema.InstanceNamespace), sw.ToString ());
		}

		[Test]
		public void TestSerializeEnumeration ()
		{
			Serialize (SimpleEnumeration.FIRST);
			Assert.AreEqual (Infoset ("<SimpleEnumeration>FIRST</SimpleEnumeration>"), WriterText, "#1");

			Serialize (SimpleEnumeration.SECOND);
			Assert.AreEqual (Infoset ("<SimpleEnumeration>SECOND</SimpleEnumeration>"), WriterText, "#2");
		}

		[Test]
		public void TestSerializeEnumeration_Encoded ()
		{
			SerializeEncoded (SimpleEnumeration.FIRST);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<SimpleEnumeration d1p1:type='SimpleEnumeration' xmlns:d1p1='{0}'>FIRST</SimpleEnumeration>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#B1");

			SerializeEncoded (SimpleEnumeration.SECOND);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<SimpleEnumeration d1p1:type='SimpleEnumeration' xmlns:d1p1='{0}'>SECOND</SimpleEnumeration>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#B2");
		}

		[Test]
		public void TestSerializeEnumDefaultValue ()
		{
			Serialize (new EnumDefaultValue ());
			Assert.AreEqual (Infoset ("<EnumDefaultValue />"), WriterText, "#1");

			Serialize (new SimpleEnumeration ());
			Assert.AreEqual (Infoset ("<SimpleEnumeration>FIRST</SimpleEnumeration>"), WriterText, "#2");

			Serialize (3, typeof (EnumDefaultValue));
			Assert.AreEqual (Infoset ("<EnumDefaultValue>e3</EnumDefaultValue>"), WriterText, "#3");

			Serialize (EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (Infoset ("<EnumDefaultValue>e3</EnumDefaultValue>"), WriterText, "#4");

			Serialize (EnumDefaultValue.e1 | EnumDefaultValue.e2, typeof (EnumDefaultValue));
			Assert.AreEqual (Infoset ("<EnumDefaultValue>e3</EnumDefaultValue>"), WriterText, "#5");

			Serialize (EnumDefaultValue.e1 | EnumDefaultValue.e2 | EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (Infoset ("<EnumDefaultValue>e3</EnumDefaultValue>"), WriterText, "#6");

			Serialize (EnumDefaultValue.e1 | EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (Infoset ("<EnumDefaultValue>e3</EnumDefaultValue>"), WriterText, "#7");

			Serialize (EnumDefaultValue.e2 | EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (Infoset ("<EnumDefaultValue>e3</EnumDefaultValue>"), WriterText, "#8");

			Serialize (3, typeof (FlagEnum));
			Assert.AreEqual (Infoset ("<FlagEnum>one two</FlagEnum>"), WriterText, "#9");

			Serialize (5, typeof (FlagEnum));
			Assert.AreEqual (Infoset ("<FlagEnum>one four</FlagEnum>"), WriterText, "#10");

			Serialize (FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (Infoset ("<FlagEnum>four</FlagEnum>"), WriterText, "#11");

			Serialize (FlagEnum.e1 | FlagEnum.e2, typeof (FlagEnum));
			Assert.AreEqual (Infoset ("<FlagEnum>one two</FlagEnum>"), WriterText, "#12");

			Serialize (FlagEnum.e1 | FlagEnum.e2 | FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (Infoset ("<FlagEnum>one two four</FlagEnum>"), WriterText, "#13");

			Serialize (FlagEnum.e1 | FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (Infoset ("<FlagEnum>one four</FlagEnum>"), WriterText, "#14");

			Serialize (FlagEnum.e2 | FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (Infoset ("<FlagEnum>two four</FlagEnum>"), WriterText, "#15");

			Serialize (3, typeof (EnumDefaultValueNF));
			Assert.AreEqual (Infoset ("<EnumDefaultValueNF>e3</EnumDefaultValueNF>"), WriterText, "#16");

			Serialize (EnumDefaultValueNF.e2, typeof (EnumDefaultValueNF));
			Assert.AreEqual (Infoset ("<EnumDefaultValueNF>e2</EnumDefaultValueNF>"), WriterText, "#17");

			Serialize (2, typeof (ZeroFlagEnum));
			Assert.AreEqual (Infoset ("<ZeroFlagEnum>tns:t&lt;w&gt;o</ZeroFlagEnum>"), WriterText, "#18");

			Serialize (new ZeroFlagEnum ()); // enum actually has a field with value 0
			Assert.AreEqual (Infoset ("<ZeroFlagEnum>zero</ZeroFlagEnum>"), WriterText, "#19");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestSerializeEnumDefaultValue_Encoded ()
		{
			SerializeEncoded (new EnumDefaultValue ());
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}' />",
				XmlSchema.InstanceNamespace), sw.ToString (), "#1");

			SerializeEncoded (new SimpleEnumeration ());
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<SimpleEnumeration d1p1:type='SimpleEnumeration' xmlns:d1p1='{0}'>FIRST</SimpleEnumeration>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#2");

			SerializeEncoded (3, typeof (EnumDefaultValue));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}'>e3</EnumDefaultValue>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#3");

			SerializeEncoded (EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}'>e3</EnumDefaultValue>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#4");

			SerializeEncoded (EnumDefaultValue.e1 | EnumDefaultValue.e2, typeof (EnumDefaultValue));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}'>e3</EnumDefaultValue>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#5");

			SerializeEncoded (EnumDefaultValue.e1 | EnumDefaultValue.e2 | EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}'>e3</EnumDefaultValue>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#6");

			SerializeEncoded (EnumDefaultValue.e1 | EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}'>e3</EnumDefaultValue>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#7");

			SerializeEncoded (EnumDefaultValue.e2 | EnumDefaultValue.e3, typeof (EnumDefaultValue));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValue d1p1:type='EnumDefaultValue' xmlns:d1p1='{0}'>e3</EnumDefaultValue>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#8");

			SerializeEncoded (3, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e1 e2</FlagEnum>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#9");

			SerializeEncoded (5, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e1 e4</FlagEnum>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#10");

			SerializeEncoded (FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e4</FlagEnum>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#11");

			SerializeEncoded (FlagEnum.e1 | FlagEnum.e2, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e1 e2</FlagEnum>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#12");

			SerializeEncoded (FlagEnum.e1 | FlagEnum.e2 | FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e1 e2 e4</FlagEnum>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#13");

			SerializeEncoded (FlagEnum.e1 | FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e1 e4</FlagEnum>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#14");

			SerializeEncoded (FlagEnum.e2 | FlagEnum.e4, typeof (FlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<FlagEnum d1p1:type='FlagEnum' xmlns:d1p1='{0}'>e2 e4</FlagEnum>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#15");

			SerializeEncoded (3, typeof (EnumDefaultValueNF));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValueNF d1p1:type='EnumDefaultValueNF' xmlns:d1p1='{0}'>e3</EnumDefaultValueNF>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#16");

			SerializeEncoded (EnumDefaultValueNF.e2, typeof (EnumDefaultValueNF));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<EnumDefaultValueNF d1p1:type='EnumDefaultValueNF' xmlns:d1p1='{0}'>e2</EnumDefaultValueNF>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#17");

			SerializeEncoded (2, typeof (ZeroFlagEnum));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<ZeroFlagEnum d1p1:type='ZeroFlagEnum' xmlns:d1p1='{0}'>e2</ZeroFlagEnum>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#18");

			SerializeEncoded (new ZeroFlagEnum ()); // enum actually has a field with value 0
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<ZeroFlagEnum d1p1:type='ZeroFlagEnum' xmlns:d1p1='{0}'>e0</ZeroFlagEnum>",
				XmlSchema.InstanceNamespace), sw.ToString (), "#19");
		}

		[Test]
		public void TestSerializeEnumDefaultValue_InvalidValue1 ()
		{
			try {
				Serialize ("b", typeof (EnumDefaultValue));
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#A2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#A3");
			}

			try {
				Serialize ("e1", typeof (EnumDefaultValue));
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#B2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#B3");
			}

			try {
				Serialize ("e1,e2", typeof (EnumDefaultValue));
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#C2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#C3");
			}

			try {
				Serialize (string.Empty, typeof (EnumDefaultValue));
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#D2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#D3");
			}

			try {
				Serialize ("1", typeof (EnumDefaultValue));
				Assert.Fail ("#E1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#E2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#E3");
			}

			try {
				Serialize ("0", typeof (EnumDefaultValue));
				Assert.Fail ("#F1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#F2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#F3");
			}

			try {
				Serialize (new SimpleClass (), typeof (EnumDefaultValue));
				Assert.Fail ("#G1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#G2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#G3");
			}
		}

		[Test]
		public void TestSerializeEnumDefaultValue_InvalidValue2 ()
		{
#if NET_2_0
			try {
				Serialize (5, typeof (EnumDefaultValue));
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#4");
				Assert.IsNotNull (ex.InnerException.Message, "#5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'5'") != -1, "#6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (EnumDefaultValue).FullName) != -1, "#7");
			}
#else
			Serialize (5, typeof (EnumDefaultValue));
			Assert.AreEqual (Infoset ("<EnumDefaultValue>5</EnumDefaultValue>"), WriterText);
#endif
		}

		[Test]
		public void TestSerializeEnumDefaultValueNF_InvalidValue1 ()
		{
#if NET_2_0
			try {
				Serialize (new EnumDefaultValueNF ());
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#4");
				Assert.IsNotNull (ex.InnerException.Message, "#5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'0'") != -1, "#6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (EnumDefaultValueNF).FullName) != -1, "#7");
			}
#else
			Serialize (new EnumDefaultValueNF ());
			Assert.AreEqual (Infoset ("<EnumDefaultValueNF>0</EnumDefaultValueNF>"), WriterText);
#endif
		}

		[Test]
		public void TestSerializeEnumDefaultValueNF_InvalidValue2 ()
		{
#if NET_2_0
			try {
				Serialize (15, typeof (EnumDefaultValueNF));
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#4");
				Assert.IsNotNull (ex.InnerException.Message, "#5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'15'") != -1, "#6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (EnumDefaultValueNF).FullName) != -1, "#7");
			}
#else
			Serialize (15, typeof (EnumDefaultValueNF));
			Assert.AreEqual (Infoset ("<EnumDefaultValueNF>15</EnumDefaultValueNF>"), WriterText);
#endif
		}

		[Test]
		public void TestSerializeEnumDefaultValueNF_InvalidValue3 ()
		{
			try {
				Serialize ("b", typeof (EnumDefaultValueNF));
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#A2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#A3");
			}

			try {
				Serialize ("e2", typeof (EnumDefaultValueNF));
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#B2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#B3");
			}

			try {
				Serialize (string.Empty, typeof (EnumDefaultValueNF));
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#C2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#C3");
			}

			try {
				Serialize ("1", typeof (EnumDefaultValueNF));
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#D2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#D3");
			}

			try {
				Serialize ("0", typeof (EnumDefaultValueNF));
				Assert.Fail ("#E1");
			} catch (InvalidOperationException ex) {
				Assert.IsNotNull (ex.InnerException, "#E2");
				Assert.AreEqual (typeof (InvalidCastException), ex.InnerException.GetType (), "#E3");
			}
		}

		[Test]
		public void TestSerializeField ()
		{
			Field f = new Field ();
			Serialize (f, typeof (Field));
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag1='' flag2='' flag3=''" +
				" flag4='' modifiers='public' modifiers2='public' modifiers4='public' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)), WriterText, "#A");

			f.Flags1 = FlagEnum.e1;
			f.Flags2 = FlagEnum.e1;
			f.Flags3 = FlagEnum.e2;
			f.Modifiers = MapModifiers.Protected;
			f.Modifiers2 = MapModifiers.Public;
			f.Modifiers3 = MapModifiers.Public;
			f.Modifiers4 = MapModifiers.Protected;
			f.Modifiers5 = MapModifiers.Public;
			Serialize (f, typeof (Field));
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag3='two' flag4=''" +
				" modifiers='protected' modifiers2='public' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)), WriterText, "#B");

			f.Flags1 = (FlagEnum) 1;
			f.Flags1 = FlagEnum.e2;
			f.Flags2 = FlagEnum.e2;
			f.Flags3 = FlagEnum.e1 | FlagEnum.e2;
			f.Modifiers = MapModifiers.Public;
			f.Modifiers2 = MapModifiers.Protected;
			f.Modifiers3 = MapModifiers.Protected;
			f.Modifiers4 = MapModifiers.Public;
			f.Modifiers5 = MapModifiers.Protected;
			Serialize (f, typeof (Field));
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag1='two' flag2='two'" +
				" flag4='' modifiers='public' modifiers2='protected'" +
				" modifiers3='protected' modifiers4='public'" +
				" modifiers5='protected' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)), WriterText, "#C");

			f.Flags1 = FlagEnum.e1 | FlagEnum.e2;
			f.Flags2 = FlagEnum.e2;
			f.Flags3 = FlagEnum.e4;
			f.Flags4 = FlagEnum.e1 | FlagEnum.e2 | FlagEnum.e4;
			f.Modifiers3 = MapModifiers.Public;
			f.Modifiers4 = MapModifiers.Protected;
			f.Modifiers5 = MapModifiers.Public;
			f.Names = new string [] { "a", "b" };
			Serialize (f, typeof (Field));
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag1='one two' flag2='two'" +
				" flag3='four' flag4='one two four' modifiers='public'" +
				" modifiers2='protected' names='a b' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)), WriterText, "#D");

			f.Flags2 = (FlagEnum) 444;
			f.Flags3 = (FlagEnum) 555;
			f.Modifiers = (MapModifiers) 666;
			f.Modifiers2 = (MapModifiers) 777;
			f.Modifiers3 = (MapModifiers) 0;
			f.Modifiers4 = (MapModifiers) 888;
			f.Modifiers5 = (MapModifiers) 999;
#if NET_2_0
			try {
				Serialize (f, typeof (Field));
				Assert.Fail ("#E1");
			} catch (InvalidOperationException ex) {
				// There was an error generating the XML document
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
				Assert.IsNotNull (ex.Message, "#E3");
				Assert.IsNotNull (ex.InnerException, "#E4");

				// Instance validation error: '444' is not a valid value for
				// MonoTests.System.Xml.TestClasses.FlagEnum
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#E5");
				Assert.IsNotNull (ex.InnerException.Message, "#E6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'444'") != -1, "#E7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (FlagEnum).FullName) != -1, "#E8");
				Assert.IsNull (ex.InnerException.InnerException, "#E9");
			}
#else
			Serialize (f, typeof (Field));
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag1='one two' flag2='444'" +
				" flag3='555' flag4='one two four' modifiers='666' modifiers2='777'" +
				" modifiers4='888' modifiers5='999' names='a b' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)), WriterText, "#E");
#endif
		}

		[Test]
		[Category ("NotDotNet")] // MS bug
		public void TestSerializeField_Encoded ()
		{
			Field_Encoded f = new Field_Encoded ();
			SerializeEncoded (f, typeof (Field_Encoded));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
#if NET_2_0
				"<q1:field xmlns:xsi='{1}' xmlns:xsd='{0}' id='id1' flag1=''" +
				" flag2='' flag3='' flag4='' modifiers='PuBlIc'" +
				" modifiers2='PuBlIc' modifiers4='PuBlIc' xmlns:q1='some:urn' />",
#else
				"<q1:field xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' flag1=''" +
				" flag2='' flag3='' flag4='' modifiers='PuBlIc'" +
				" modifiers2='PuBlIc' modifiers4='PuBlIc' xmlns:q1='some:urn' />",
#endif
				XmlSchema.Namespace, XmlSchema.InstanceNamespace),
				sw.GetStringBuilder ().ToString (), "#A");

			f.Flags1 = FlagEnum_Encoded.e1;
			f.Flags2 = FlagEnum_Encoded.e1;
			f.Flags3 = FlagEnum_Encoded.e2;
			f.Modifiers = MapModifiers.Protected;
			f.Modifiers2 = MapModifiers.Public;
			f.Modifiers3 = MapModifiers.Public;
			f.Modifiers4 = MapModifiers.Protected;
			f.Modifiers5 = MapModifiers.Public;
			SerializeEncoded (f, typeof (Field_Encoded));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
#if NET_2_0
				"<q1:field xmlns:xsi='{1}' xmlns:xsd='{0}' id='id1' flag3='two'" +
				" flag4='' modifiers='Protected' modifiers2='PuBlIc'" +
				" xmlns:q1='some:urn' />",
#else
				"<q1:field xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' flag3='two'" +
				" flag4='' modifiers='Protected' modifiers2='PuBlIc'" +
				" xmlns:q1='some:urn' />",
#endif
				XmlSchema.Namespace, XmlSchema.InstanceNamespace),
				sw.GetStringBuilder ().ToString (), "#B");

			f.Flags1 = FlagEnum_Encoded.e2;
			f.Flags2 = FlagEnum_Encoded.e2;
			f.Flags3 = FlagEnum_Encoded.e1 | FlagEnum_Encoded.e2;
			f.Modifiers = MapModifiers.Public;
			f.Modifiers2 = MapModifiers.Protected;
			f.Modifiers3 = MapModifiers.Protected;
			f.Modifiers4 = MapModifiers.Public;
			f.Modifiers5 = MapModifiers.Protected;
			SerializeEncoded (f, typeof (Field_Encoded));
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
#if NET_2_0
				"<q1:field xmlns:xsi='{1}' xmlns:xsd='{0}' id='id1' flag1='two'" +
				" flag2='two' flag4='' modifiers='PuBlIc' modifiers2='Protected'" +
				" modifiers3='Protected' modifiers4='PuBlIc' modifiers5='Protected'" +
				" xmlns:q1='some:urn' />",
#else
				"<q1:field xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' flag1='two'" +
				" flag2='two' flag4='' modifiers='PuBlIc' modifiers2='Protected'" +
				" modifiers3='Protected' modifiers4='PuBlIc' modifiers5='Protected'" +
				" xmlns:q1='some:urn' />",
#endif
 				XmlSchema.Namespace, XmlSchema.InstanceNamespace),
				sw.GetStringBuilder ().ToString (), "#C");

			f.Flags1 = (FlagEnum_Encoded) 1;
			f.Flags2 = (FlagEnum_Encoded) 444;
			f.Flags3 = (FlagEnum_Encoded) 555;
			f.Modifiers = (MapModifiers) 666;
			f.Modifiers2 = (MapModifiers) 777;
			f.Modifiers3 = (MapModifiers) 0;
			f.Modifiers4 = (MapModifiers) 888;
			f.Modifiers5 = (MapModifiers) 999;
#if NET_2_0
			try {
#endif
			SerializeEncoded (f, typeof (Field_Encoded));
#if NET_2_0
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				// There was an error generating the XML document
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.Message, "#D3");
				Assert.IsNotNull (ex.InnerException, "#D4");

				// Instance validation error: '444' is not a valid value for
				// MonoTests.System.Xml.TestClasses.FlagEnum_Encoded
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#D5");
				Assert.IsNotNull (ex.InnerException.Message, "#D6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'444'") != -1, "#D7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (FlagEnum_Encoded).FullName) != -1, "#D8");
				Assert.IsNull (ex.InnerException.InnerException, "#D9");
			}
#else
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<q1:field xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' flag2='444'" +
				" flag3='555' flag4='' modifiers='666' modifiers2='777'" +
				" modifiers4='888' modifiers5='999' xmlns:q1='some:urn' />",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace),
				sw.GetStringBuilder ().ToString (), "#D");
#endif
		}

		[Test]
		public void TestSerializeGroup ()
		{
			Group myGroup = new Group ();
			myGroup.GroupName = ".NET";

			Byte [] hexByte = new Byte [] { 0x64, 0x32 };
			myGroup.GroupNumber = hexByte;

			DateTime myDate = new DateTime (2002, 5, 2);
			myGroup.Today = myDate;
			myGroup.PostitiveInt = "10000";
			myGroup.IgnoreThis = true;
			Car thisCar = (Car) myGroup.myCar ("1234566");
			myGroup.MyVehicle = thisCar;

			SetUpWriter ();
			xtw.WriteStartDocument (true);
			xtw.WriteStartElement ("Wrapper");
			SerializeEncoded (xtw, myGroup, typeof (Group));
			xtw.WriteEndElement ();
			xtw.Close ();

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<Wrapper>" +
				"<Group xmlns:xsd='{0}' xmlns:xsi='{1}' xmlns:d2p1='http://www.cpandl.com' CreationDate='2002-05-02' d2p1:GroupName='.NET' GroupNumber='ZDI=' id='id1'>" +
				"<PosInt xsi:type='xsd:nonNegativeInteger'>10000</PosInt>" +
				"<Grouptype xsi:type='GroupType'>Small</Grouptype>" +
				"<MyVehicle href='#id2' />" +
				"</Group>" +
				"<Car xmlns:d2p1='{1}' id='id2' d2p1:type='Car'>" +
				"<licenseNumber xmlns:q1='{0}' d2p1:type='q1:string'>1234566</licenseNumber>" +
				"<makeDate xmlns:q2='{0}' d2p1:type='q2:date'>0001-01-01</makeDate>" +
				"</Car>" +
				"</Wrapper>",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#1");

			myGroup.GroupName = null;
			myGroup.Grouptype = GroupType.B;
			myGroup.MyVehicle.licenseNumber = null;
			myGroup.MyVehicle.weight = "450";

			SetUpWriter ();
			xtw.WriteStartDocument (true);
			xtw.WriteStartElement ("Wrapper");
			SerializeEncoded (xtw, myGroup, typeof (Group));
			xtw.WriteEndElement ();
			xtw.Close ();

			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<Wrapper>" +
				"<Group xmlns:xsd='{0}' xmlns:xsi='{1}' CreationDate='2002-05-02' GroupNumber='ZDI=' id='id1'>" +
				"<PosInt xsi:type='xsd:nonNegativeInteger'>10000</PosInt>" +
				"<Grouptype xsi:type='GroupType'>Large</Grouptype>" +
				"<MyVehicle href='#id2' />" +
				"</Group>" +
				"<Car xmlns:d2p1='{1}' id='id2' d2p1:type='Car'>" +
				"<makeDate xmlns:q1='{0}' d2p1:type='q1:date'>0001-01-01</makeDate>" +
				"<weight xmlns:q2='{0}' d2p1:type='q2:string'>450</weight>" +
				"</Car>" +
				"</Wrapper>",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#2");
		}

		[Test]
		public void TestSerializeZeroFlagEnum_InvalidValue ()
		{
#if NET_2_0
			try {
				Serialize (4, typeof (ZeroFlagEnum)); // corresponding enum field is marked XmlIgnore
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#4");
				Assert.IsNotNull (ex.InnerException.Message, "#5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'4'") != -1, "#6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (ZeroFlagEnum).FullName) != -1, "#7");
			}
#else
			Serialize (4, typeof (ZeroFlagEnum)); // corresponding enum field is marked XmlIgnore
			Assert.AreEqual (Infoset ("<ZeroFlagEnum>4</ZeroFlagEnum>"), WriterText);
#endif
		}

		[Test]
		public void TestSerializeQualifiedName ()
		{
			Serialize (new XmlQualifiedName ("me", "home.urn"));
			Assert.AreEqual (Infoset ("<QName xmlns:q1='home.urn'>q1:me</QName>"), WriterText);
		}

		[Test]
		public void TestSerializeBytes ()
		{
			Serialize ((byte) 0xAB);
			Assert.AreEqual (Infoset ("<unsignedByte>171</unsignedByte>"), WriterText);

			Serialize ((byte) 15);
			Assert.AreEqual (Infoset ("<unsignedByte>15</unsignedByte>"), WriterText);
		}

		[Test]
		public void TestSerializeByteArrays ()
		{
			Serialize (new byte [] { });
			Assert.AreEqual (Infoset ("<base64Binary />"), WriterText);

			Serialize (new byte [] { 0xAB, 0xCD });
			Assert.AreEqual (Infoset ("<base64Binary>q80=</base64Binary>"), WriterText);
		}

		[Test]
		public void TestSerializeDateTime ()
		{
			DateTime d = new DateTime ();
			Serialize (d);

			TimeZone tz = TimeZone.CurrentTimeZone;
			TimeSpan off = tz.GetUtcOffset (d);
			string sp = string.Format ("{0}{1:00}:{2:00}", off.Ticks >= 0 ? "+" : "", off.Hours, off.Minutes);
			Assert.AreEqual (Infoset ("<dateTime>0001-01-01T00:00:00.0000000" + sp + "</dateTime>"), WriterText);
		}

		/*
		FIXME
		 - decimal
		 - Guid
		 - XmlNode objects
		
		[Test]
		public void TestSerialize()
		{
			Serialize();
			Assert.AreEqual (WriterText, "");
		}
		*/

		// test basic class serialization /////////////////////////////////////		
		[Test]
		public void TestSerializeSimpleClass ()
		{
			SimpleClass simple = new SimpleClass ();
			Serialize (simple);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);

			simple.something = "hello";

			Serialize (simple);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>hello</something></SimpleClass>"), WriterText);
		}

		[Test]
		public void TestSerializeStringCollection ()
		{
			StringCollection strings = new StringCollection ();
			Serialize (strings);
			Assert.AreEqual (Infoset ("<ArrayOfString xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);

			strings.Add ("hello");
			strings.Add ("goodbye");
			Serialize (strings);
			Assert.AreEqual (Infoset ("<ArrayOfString xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><string>hello</string><string>goodbye</string></ArrayOfString>"), WriterText);
		}

		[Test]
		public void TestSerializeOptionalValueTypeContainer ()
		{
			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();
			XmlAttributes attr;
			OptionalValueTypeContainer optionalValue = new OptionalValueTypeContainer ();

			Serialize (optionalValue);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
#if NET_2_0
				"<optionalValue xmlns:xsi='{1}' xmlns:xsd='{0}' xmlns='{2}' />",
#else
				"<optionalValue xmlns:xsd='{0}' xmlns:xsi='{1}' xmlns='{2}' />",
#endif
				XmlSchema.Namespace, XmlSchema.InstanceNamespace, AnotherNamespace),
				sw.ToString (), "#1");

			attr = new XmlAttributes ();

			// remove the DefaultValue attribute on the Flags member
			overrides.Add (typeof (OptionalValueTypeContainer), "Flags", attr);
			// remove the DefaultValue attribute on the Attributes member
			overrides.Add (typeof (OptionalValueTypeContainer), "Attributes", attr);

			Serialize (optionalValue, overrides);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
#if NET_2_0
				"<optionalValue xmlns:xsi='{1}' xmlns:xsd='{0}' xmlns='{2}'>" +
#else
				"<optionalValue xmlns:xsd='{0}' xmlns:xsi='{1}' xmlns='{2}'>" +
#endif
				"<Attributes xmlns='{3}'>one four</Attributes>" +
				"</optionalValue>", XmlSchema.Namespace, XmlSchema.InstanceNamespace,
				AnotherNamespace, ANamespace), sw.ToString (), "#2");

			optionalValue.FlagsSpecified = true;
			Serialize (optionalValue, overrides);
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
#if NET_2_0
				"<optionalValue xmlns:xsi='{1}' xmlns:xsd='{0}' xmlns='{2}'>" +
#else
				"<optionalValue xmlns:xsd='{0}' xmlns:xsi='{1}' xmlns='{2}'>" +
#endif
				"<Attributes xmlns='{3}'>one four</Attributes>" +
				"<Flags xmlns='{3}'>one</Flags>" +
				"</optionalValue>",
				XmlSchema.Namespace, XmlSchema.InstanceNamespace, AnotherNamespace,
				ANamespace), sw.ToString (), "#3");
		}

		[Test]
		public void TestSerializePlainContainer ()
		{
			StringCollectionContainer container = new StringCollectionContainer ();
			Serialize (container);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages /></StringCollectionContainer>"), WriterText);

			container.Messages.Add ("hello");
			container.Messages.Add ("goodbye");
			Serialize (container);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages><string>hello</string><string>goodbye</string></Messages></StringCollectionContainer>"), WriterText);
		}

		[Test]
		public void TestSerializeArrayContainer ()
		{
			ArrayContainer container = new ArrayContainer ();
			Serialize (container);
			Assert.AreEqual (Infoset ("<ArrayContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);

			container.items = new object [] { 10, 20 };
			Serialize (container);
			Assert.AreEqual (Infoset ("<ArrayContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' ><items><anyType xsi:type='xsd:int'>10</anyType><anyType xsi:type='xsd:int'>20</anyType></items></ArrayContainer>"), WriterText);

			container.items = new object [] { 10, "hello" };
			Serialize (container);
			Assert.AreEqual (Infoset ("<ArrayContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' ><items><anyType xsi:type='xsd:int'>10</anyType><anyType xsi:type='xsd:string'>hello</anyType></items></ArrayContainer>"), WriterText);
		}

		[Test]
		public void TestSerializeClassArrayContainer ()
		{
			ClassArrayContainer container = new ClassArrayContainer ();
			Serialize (container);
			Assert.AreEqual (Infoset ("<ClassArrayContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);

			SimpleClass simple1 = new SimpleClass ();
			simple1.something = "hello";
			SimpleClass simple2 = new SimpleClass ();
			simple2.something = "hello";
			container.items = new SimpleClass [2];
			container.items [0] = simple1;
			container.items [1] = simple2;
			Serialize (container);
			Assert.AreEqual (Infoset ("<ClassArrayContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' ><items><SimpleClass><something>hello</something></SimpleClass><SimpleClass><something>hello</something></SimpleClass></items></ClassArrayContainer>"), WriterText);
		}

		// test basic attributes ///////////////////////////////////////////////
		[Test]
		public void TestSerializeSimpleClassWithXmlAttributes ()
		{
			SimpleClassWithXmlAttributes simple = new SimpleClassWithXmlAttributes ();
			Serialize (simple);
			Assert.AreEqual (Infoset ("<simple xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);

			simple.something = "hello";
			Serialize (simple);
			Assert.AreEqual (Infoset ("<simple xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' member='hello' />"), WriterText);
		}

		// test overrides ///////////////////////////////////////////////////////
		[Test]
		public void TestSerializeSimpleClassWithOverrides ()
		{
			// Also tests XmlIgnore
			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();

			XmlAttributes attr = new XmlAttributes ();
			attr.XmlIgnore = true;
			overrides.Add (typeof (SimpleClassWithXmlAttributes), "something", attr);

			SimpleClassWithXmlAttributes simple = new SimpleClassWithXmlAttributes ();
			simple.something = "hello";
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<simple xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);
		}

		[Test]
		public void TestSerializeSchema ()
		{
			XmlSchema schema = new XmlSchema ();
			schema.Items.Add (new XmlSchemaAttribute ());
			schema.Items.Add (new XmlSchemaAttributeGroup ());
			schema.Items.Add (new XmlSchemaComplexType ());
			schema.Items.Add (new XmlSchemaNotation ());
			schema.Items.Add (new XmlSchemaSimpleType ());
			schema.Items.Add (new XmlSchemaGroup ());
			schema.Items.Add (new XmlSchemaElement ());

			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			xtw.QuoteChar = '\'';
			xtw.Formatting = Formatting.Indented;
			XmlSerializer xs = new XmlSerializer (schema.GetType ());
			xs.Serialize (xtw, schema);

			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>{0}" +
				"<xsd:schema xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>{0}" +
				"  <xsd:attribute />{0}" +
				"  <xsd:attributeGroup />{0}" +
				"  <xsd:complexType />{0}" +
				"  <xsd:notation />{0}" +
				"  <xsd:simpleType />{0}" +
				"  <xsd:group />{0}" +
				"  <xsd:element />{0}" +
				"</xsd:schema>", Environment.NewLine), sw.ToString ());
		}

		// test xmlText //////////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlTextAttribute ()
		{
			SimpleClass simple = new SimpleClass ();
			simple.something = "hello";

			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();
			XmlAttributes attr = new XmlAttributes ();
			overrides.Add (typeof (SimpleClass), "something", attr);

			attr.XmlText = new XmlTextAttribute ();
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>hello</SimpleClass>"), WriterText, "#1");

			attr.XmlText = new XmlTextAttribute (typeof (string));
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>hello</SimpleClass>"), WriterText, "#2");

			try {
				attr.XmlText = new XmlTextAttribute (typeof (byte []));
				Serialize (simple, overrides);
				Assert.Fail ("#A1: XmlText.Type does not match the type it serializes: this should have failed");
			} catch (InvalidOperationException ex) {
				// there was an error reflecting type 'MonoTests.System.Xml.TestClasses.SimpleClass'
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf (typeof (SimpleClass).FullName) != -1, "#A4");
				Assert.IsNotNull (ex.InnerException, "#A5");

				// there was an error reflecting field 'something'.
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#A6");
				Assert.IsNotNull (ex.InnerException.Message, "#A7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("something") != -1, "#A8");
				Assert.IsNotNull (ex.InnerException.InnerException, "#A9");

				// the type for XmlText may not be specified for primitive types.
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.InnerException.GetType (), "#A10");
				Assert.IsNotNull (ex.InnerException.InnerException.Message, "#A11");
				Assert.IsNull (ex.InnerException.InnerException.InnerException, "#A12");
			}

			try {
				attr.XmlText = new XmlTextAttribute ();
				attr.XmlText.DataType = "sometype";
				Serialize (simple, overrides);
				Assert.Fail ("#B1: XmlText.DataType does not match the type it serializes: this should have failed");
			} catch (InvalidOperationException ex) {
				// There was an error reflecting type 'MonoTests.System.Xml.TestClasses.SimpleClass'.
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf (typeof (SimpleClass).FullName) != -1, "#B4");
				Assert.IsNotNull (ex.InnerException, "#B5");

				// There was an error reflecting field 'something'.
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#B6");
				Assert.IsNotNull (ex.InnerException.Message, "#B7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("something") != -1, "#B8");
				Assert.IsNotNull (ex.InnerException.InnerException, "#B9");

				//FIXME
				/*
				// There was an error reflecting type 'System.String'.
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.InnerException.GetType (), "#B10");
				Assert.IsNotNull (ex.InnerException.InnerException.Message, "#B11");
				Assert.IsTrue (ex.InnerException.InnerException.Message.IndexOf (typeof (string).FullName) != -1, "#B12");
				Assert.IsNotNull (ex.InnerException.InnerException.InnerException, "#B13");

				// Value 'sometype' cannot be used for the XmlElementAttribute.DataType property. 
				// The datatype 'http://www.w3.org/2001/XMLSchema:sometype' is missing.
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.InnerException.InnerException.GetType (), "#B14");
				Assert.IsNotNull (ex.InnerException.InnerException.InnerException.Message, "#B15");
				Assert.IsTrue (ex.InnerException.InnerException.InnerException.Message.IndexOf ("http://www.w3.org/2001/XMLSchema:sometype") != -1, "#B16");
				Assert.IsNull (ex.InnerException.InnerException.InnerException.InnerException, "#B17");
				*/
			}
		}

		// test xmlRoot //////////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlRootAttribute ()
		{
			// constructor override & element name
			XmlRootAttribute root = new XmlRootAttribute ();
			root.ElementName = "renamed";

			SimpleClassWithXmlAttributes simpleWithAttributes = new SimpleClassWithXmlAttributes ();
			Serialize (simpleWithAttributes, root);
			Assert.AreEqual (Infoset ("<renamed xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);

			SimpleClass simple = null;
			root.IsNullable = false;
			try {
				Serialize (simple, root);
				Assert.Fail ("Cannot serialize null object if XmlRoot's IsNullable == false");
			} catch (NullReferenceException) {
			}

			root.IsNullable = true;
			try {
				Serialize (simple, root);
				Assert.Fail ("Cannot serialize null object if XmlRoot's IsNullable == true");
			} catch (NullReferenceException) {
			}

			simple = new SimpleClass ();
			root.ElementName = null;
			root.Namespace = "some.urn";
			Serialize (simple, root);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='some.urn' />"), WriterText);
		}

		[Test]
		public void TestSerializeXmlRootAttributeOnMember ()
		{
			// nested root
			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();
			XmlAttributes childAttr = new XmlAttributes ();
			childAttr.XmlRoot = new XmlRootAttribute ("simple");
			overrides.Add (typeof (SimpleClass), childAttr);

			XmlAttributes attr = new XmlAttributes ();
			attr.XmlRoot = new XmlRootAttribute ("simple");
			overrides.Add (typeof (ClassArrayContainer), attr);

			ClassArrayContainer container = new ClassArrayContainer ();
			container.items = new SimpleClass [1];
			container.items [0] = new SimpleClass ();
			Serialize (container, overrides);
			Assert.AreEqual (Infoset ("<simple xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' ><items><SimpleClass /></items></simple>"), WriterText);

			// FIXME test data type
		}

		// test XmlAttribute /////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlAttributeAttribute ()
		{
			// null
			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();
			XmlAttributes attr = new XmlAttributes ();
			attr.XmlAttribute = new XmlAttributeAttribute ();
			overrides.Add (typeof (SimpleClass), "something", attr);

			SimpleClass simple = new SimpleClass (); ;
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#1");

			// regular
			simple.something = "hello";
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' something='hello' />"), WriterText, "#2");

			// AttributeName
			attr.XmlAttribute.AttributeName = "somethingelse";
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' somethingelse='hello' />"), WriterText, "#3");

			// Type
			// FIXME this should work, shouldnt it?
			// attr.XmlAttribute.Type = typeof(string);
			// Serialize(simple, overrides);
			// Assert(WriterText.EndsWith(" something='hello' />"));

			// Namespace
			attr.XmlAttribute.Namespace = "some:urn";
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' d1p1:somethingelse='hello' xmlns:d1p1='some:urn' />"), WriterText, "#4");

			// FIXME DataType
			// FIXME XmlSchemaForm Form

			// FIXME write XmlQualifiedName as attribute
		}

		// test XmlElement ///////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlElementAttribute ()
		{
			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();
			XmlAttributes attr = new XmlAttributes ();
			XmlElementAttribute element = new XmlElementAttribute ();
			attr.XmlElements.Add (element);
			overrides.Add (typeof (SimpleClass), "something", attr);

			// null
			SimpleClass simple = new SimpleClass (); ;
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#1");

			// not null
			simple.something = "hello";
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>hello</something></SimpleClass>"), WriterText, "#2");

			//ElementName
			element.ElementName = "saying";
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><saying>hello</saying></SimpleClass>"), WriterText, "#3");

			//IsNullable
			element.IsNullable = false;
			simple.something = null;
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#4");

			element.IsNullable = true;
			simple.something = null;
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><saying xsi:nil='true' /></SimpleClass>"), WriterText, "#5");

			//Namespace
			element.ElementName = null;
			element.IsNullable = false;
			element.Namespace = "some:urn";
			simple.something = "hello";
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something xmlns='some:urn'>hello</something></SimpleClass>"), WriterText, "#6");

			//FIXME DataType
			//FIXME Form
			//FIXME Type
		}

		// test XmlElementAttribute with arrays and collections //////////////////
		[Test]
		public void TestSerializeCollectionWithXmlElementAttribute ()
		{
			// the rule is:
			// if no type is specified or the specified type 
			//    matches the contents of the collection, 
			//    serialize each element in an element named after the member.
			// if the type does not match, or matches the collection itself,
			//    create a base wrapping element for the member, and then
			//    wrap each collection item in its own wrapping element based on type.

			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();
			XmlAttributes attr = new XmlAttributes ();
			XmlElementAttribute element = new XmlElementAttribute ();
			attr.XmlElements.Add (element);
			overrides.Add (typeof (StringCollectionContainer), "Messages", attr);

			// empty collection & no type info in XmlElementAttribute
			StringCollectionContainer container = new StringCollectionContainer ();
			Serialize (container, overrides);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#1");

			// non-empty collection & no type info in XmlElementAttribute
			container.Messages.Add ("hello");
			Serialize (container, overrides);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages>hello</Messages></StringCollectionContainer>"), WriterText, "#2");

			// non-empty collection & only type info in XmlElementAttribute
			element.Type = typeof (StringCollection);
			Serialize (container, overrides);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages><string>hello</string></Messages></StringCollectionContainer>"), WriterText, "#3");

			// non-empty collection & only type info in XmlElementAttribute
			element.Type = typeof (string);
			Serialize (container, overrides);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages>hello</Messages></StringCollectionContainer>"), WriterText, "#4");

			// two elements
			container.Messages.Add ("goodbye");
			element.Type = null;
			Serialize (container, overrides);
			Assert.AreEqual (Infoset ("<StringCollectionContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><Messages>hello</Messages><Messages>goodbye</Messages></StringCollectionContainer>"), WriterText, "#5");
		}

		// test DefaultValue /////////////////////////////////////////////////////
		[Test]
		public void TestSerializeDefaultValueAttribute ()
		{
			XmlAttributeOverrides overrides = new XmlAttributeOverrides ();

			XmlAttributes attr = new XmlAttributes ();
			string defaultValueInstance = "nothing";
			attr.XmlDefaultValue = defaultValueInstance;
			overrides.Add (typeof (SimpleClass), "something", attr);

			// use the default
			SimpleClass simple = new SimpleClass ();
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#A1");

			// same value as default
			simple.something = defaultValueInstance;
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#A2");

			// some other value
			simple.something = "hello";
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something>hello</something></SimpleClass>"), WriterText, "#A3");

			overrides = new XmlAttributeOverrides ();
			attr = new XmlAttributes ();
			attr.XmlAttribute = new XmlAttributeAttribute ();
			attr.XmlDefaultValue = defaultValueInstance;
			overrides.Add (typeof (SimpleClass), "something", attr);

			// use the default
			simple = new SimpleClass ();
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#B1");

			// same value as default
			simple.something = defaultValueInstance;
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#B2");

			// some other value
			simple.something = "hello";
			Serialize (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass something='hello' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#B3");

			overrides = new XmlAttributeOverrides ();
			attr = new XmlAttributes ();
			attr.XmlAttribute = new XmlAttributeAttribute ("flagenc");
			overrides.Add (typeof (TestDefault), "flagencoded", attr);

			// use the default
			TestDefault testDefault = new TestDefault ();
			Serialize (testDefault);
			Assert.AreEqual (Infoset ("<testDefault xmlns='urn:myNS' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#C1");

			// use the default with overrides
			Serialize (testDefault, overrides);
			Assert.AreEqual (Infoset ("<testDefault flagenc='e1 e4' xmlns='urn:myNS' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#C2");

			overrides = new XmlAttributeOverrides ();
			attr = new XmlAttributes ();
			attr.XmlAttribute = new XmlAttributeAttribute ("flagenc");
			attr.XmlDefaultValue = (FlagEnum_Encoded.e1 | FlagEnum_Encoded.e4); // add default again
			overrides.Add (typeof (TestDefault), "flagencoded", attr);

			// use the default with overrides
			Serialize (testDefault, overrides);
			Assert.AreEqual (Infoset ("<testDefault xmlns='urn:myNS' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#C3");

			// use the default with overrides and default namspace
			Serialize (testDefault, overrides, AnotherNamespace);
			Assert.AreEqual (Infoset ("<testDefault xmlns='urn:myNS' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#C4");

			// non-default values
			testDefault.strDefault = "Some Text";
			testDefault.boolT = false;
			testDefault.boolF = true;
			testDefault.decimalval = 20m;
			testDefault.flag = FlagEnum.e2;
			testDefault.flagencoded = FlagEnum_Encoded.e2 | FlagEnum_Encoded.e1;
			Serialize (testDefault);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<testDefault xmlns='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault>Some Text</strDefault>" +
				"    <boolT>false</boolT>" +
				"    <boolF>true</boolF>" +
				"    <decimalval>20</decimalval>" +
				"    <flag>two</flag>" +
				"    <flagencoded>e1 e2</flagencoded>" +
				"</testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#C5");

			Serialize (testDefault, overrides);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<testDefault flagenc='e1 e2' xmlns='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault>Some Text</strDefault>" +
				"    <boolT>false</boolT>" +
				"    <boolF>true</boolF>" +
				"    <decimalval>20</decimalval>" +
				"    <flag>two</flag>" +
				"</testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#C6");

			Serialize (testDefault, overrides, AnotherNamespace);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<testDefault flagenc='e1 e2' xmlns='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault>Some Text</strDefault>" +
				"    <boolT>false</boolT>" +
				"    <boolF>true</boolF>" +
				"    <decimalval>20</decimalval>" +
				"    <flag>two</flag>" +
				"</testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#C7");

			attr = new XmlAttributes ();
			XmlTypeAttribute xmlType = new XmlTypeAttribute ("flagenum");
			xmlType.Namespace = "yetanother:urn";
			attr.XmlType = xmlType;
			overrides.Add (typeof (FlagEnum_Encoded), attr);

			Serialize (testDefault, overrides, AnotherNamespace);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<testDefault flagenc='e1 e2' xmlns='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault>Some Text</strDefault>" +
				"    <boolT>false</boolT>" +
				"    <boolF>true</boolF>" +
				"    <decimalval>20</decimalval>" +
				"    <flag>two</flag>" +
				"</testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#C8");

			attr = new XmlAttributes ();
			attr.XmlType = new XmlTypeAttribute ("testDefault");
			overrides.Add (typeof (TestDefault), attr);

			Serialize (testDefault, overrides, AnotherNamespace);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<testDefault flagenc='e1 e2' xmlns='{2}' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault>Some Text</strDefault>" +
				"    <boolT>false</boolT>" +
				"    <boolF>true</boolF>" +
				"    <decimalval>20</decimalval>" +
				"    <flag>two</flag>" +
				"</testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace,
				AnotherNamespace)), WriterText, "#C9");
		}

		[Test]
		[Category ("NotWorking")] // SerializationCodeGenerator outputs wrong xsi:type for flagencoded in #C1
		public void TestSerializeDefaultValueAttribute_Encoded ()
		{
			SoapAttributeOverrides overrides = new SoapAttributeOverrides ();
			SoapAttributes attr = new SoapAttributes ();
			attr.SoapAttribute = new SoapAttributeAttribute ();
			string defaultValueInstance = "nothing";
			attr.SoapDefaultValue = defaultValueInstance;
			overrides.Add (typeof (SimpleClass), "something", attr);

			// use the default
			SimpleClass simple = new SimpleClass ();
			SerializeEncoded (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass id='id1' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#A1");

			// same value as default
			simple.something = defaultValueInstance;
			SerializeEncoded (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass id='id1' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#A2");

			// some other value
			simple.something = "hello";
			SerializeEncoded (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass id='id1' something='hello' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#A3");

			attr.SoapAttribute = null;
			attr.SoapElement = new SoapElementAttribute ();

			// use the default
			simple = new SimpleClass ();
			SerializeEncoded (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass id='id1' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText, "#B1");

			// same value as default
			simple.something = defaultValueInstance;
			SerializeEncoded (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass id='id1' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something xsi:type='xsd:string'>nothing</something></SimpleClass>"), WriterText, "#B2");

			// some other value
			simple.something = "hello";
			SerializeEncoded (simple, overrides);
			Assert.AreEqual (Infoset ("<SimpleClass id='id1' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><something xsi:type='xsd:string'>hello</something></SimpleClass>"), WriterText, "#B3");

			overrides = new SoapAttributeOverrides ();
			attr = new SoapAttributes ();
			attr.SoapElement = new SoapElementAttribute ("flagenc");
			overrides.Add (typeof (TestDefault), "flagencoded", attr);

			// use the default (from MS KB325691)
			TestDefault testDefault = new TestDefault ();
			SerializeEncoded (testDefault);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<q1:testDefault id='id1' xmlns:q1='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault xsi:type='xsd:string'>Default Value</strDefault>" +
				"    <boolT xsi:type='xsd:boolean'>true</boolT>" +
				"    <boolF xsi:type='xsd:boolean'>false</boolF>" +
				"    <decimalval xsi:type='xsd:decimal'>10</decimalval>" +
				"    <flag xsi:type='FlagEnum'>e1 e4</flag>" +
				"    <flagencoded xsi:type='flagenum'>one four</flagencoded>" +
				"</q1:testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#C1");

			SerializeEncoded (testDefault, overrides);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<q1:testDefault id='id1' xmlns:q1='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault xsi:type='xsd:string'>Default Value</strDefault>" +
				"    <boolT xsi:type='xsd:boolean'>true</boolT>" +
				"    <boolF xsi:type='xsd:boolean'>false</boolF>" +
				"    <decimalval xsi:type='xsd:decimal'>10</decimalval>" +
				"    <flag xsi:type='FlagEnum'>e1 e4</flag>" +
				"    <flagenc xsi:type='flagenum'>one four</flagenc>" +
				"</q1:testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#C2");

			SerializeEncoded (testDefault, overrides, AnotherNamespace);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<q1:testDefault id='id1' xmlns:q1='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault xsi:type='xsd:string'>Default Value</strDefault>" +
				"    <boolT xsi:type='xsd:boolean'>true</boolT>" +
				"    <boolF xsi:type='xsd:boolean'>false</boolF>" +
				"    <decimalval xsi:type='xsd:decimal'>10</decimalval>" +
				"    <flag xmlns:q2='{2}' xsi:type='q2:FlagEnum'>e1 e4</flag>" +
				"    <flagenc xmlns:q3='{2}' xsi:type='q3:flagenum'>one four</flagenc>" +
				"</q1:testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace,
				AnotherNamespace)), WriterText, "#C3");

			// non-default values
			testDefault.strDefault = "Some Text";
			testDefault.boolT = false;
			testDefault.boolF = true;
			testDefault.decimalval = 20m;
			testDefault.flag = FlagEnum.e2;
			testDefault.flagencoded = FlagEnum_Encoded.e2 | FlagEnum_Encoded.e1;
			SerializeEncoded (testDefault);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<q1:testDefault id='id1' xmlns:q1='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault xsi:type='xsd:string'>Some Text</strDefault>" +
				"    <boolT xsi:type='xsd:boolean'>false</boolT>" +
				"    <boolF xsi:type='xsd:boolean'>true</boolF>" +
				"    <decimalval xsi:type='xsd:decimal'>20</decimalval>" +
				"    <flag xsi:type='FlagEnum'>e2</flag>" +
				"    <flagencoded xsi:type='flagenum'>one two</flagencoded>" +
				"</q1:testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#C4");

			SerializeEncoded (testDefault, overrides);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<q1:testDefault id='id1' xmlns:q1='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault xsi:type='xsd:string'>Some Text</strDefault>" +
				"    <boolT xsi:type='xsd:boolean'>false</boolT>" +
				"    <boolF xsi:type='xsd:boolean'>true</boolF>" +
				"    <decimalval xsi:type='xsd:decimal'>20</decimalval>" +
				"    <flag xsi:type='FlagEnum'>e2</flag>" +
				"    <flagenc xsi:type='flagenum'>one two</flagenc>" +
				"</q1:testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace)),
				WriterText, "#C5");

			attr = new SoapAttributes ();
			attr.SoapType = new SoapTypeAttribute ("flagenum", "yetanother:urn");
			overrides.Add (typeof (FlagEnum_Encoded), attr);

			SerializeEncoded (testDefault, overrides, AnotherNamespace);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<q1:testDefault id='id1' xmlns:q1='urn:myNS' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault xsi:type='xsd:string'>Some Text</strDefault>" +
				"    <boolT xsi:type='xsd:boolean'>false</boolT>" +
				"    <boolF xsi:type='xsd:boolean'>true</boolF>" +
				"    <decimalval xsi:type='xsd:decimal'>20</decimalval>" +
				"    <flag xmlns:q2='{2}' xsi:type='q2:FlagEnum'>e2</flag>" +
				"    <flagenc xmlns:q3='yetanother:urn' xsi:type='q3:flagenum'>one two</flagenc>" +
				"</q1:testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace,
				AnotherNamespace)), WriterText, "#C6");

			attr = new SoapAttributes ();
			attr.SoapType = new SoapTypeAttribute ("testDefault");
			overrides.Add (typeof (TestDefault), attr);

			SerializeEncoded (testDefault, overrides, AnotherNamespace);
			Assert.AreEqual (Infoset (string.Format (CultureInfo.InvariantCulture,
				"<q1:testDefault id='id1' xmlns:q1='{2}' xmlns:xsd='{0}' xmlns:xsi='{1}'>" +
				"    <strDefault xsi:type='xsd:string'>Some Text</strDefault>" +
				"    <boolT xsi:type='xsd:boolean'>false</boolT>" +
				"    <boolF xsi:type='xsd:boolean'>true</boolF>" +
				"    <decimalval xsi:type='xsd:decimal'>20</decimalval>" +
				"    <flag xsi:type='q1:FlagEnum'>e2</flag>" +
				"    <flagenc xmlns:q2='yetanother:urn' xsi:type='q2:flagenum'>one two</flagenc>" +
				"</q1:testDefault>", XmlSchema.Namespace, XmlSchema.InstanceNamespace,
				AnotherNamespace)), WriterText, "#C7");
		}

		// test XmlEnum //////////////////////////////////////////////////////////
		[Test]
		public void TestSerializeXmlEnumAttribute ()
		{
			Serialize (XmlSchemaForm.Qualified);
			Assert.AreEqual (Infoset ("<XmlSchemaForm>qualified</XmlSchemaForm>"), WriterText, "#1");

			Serialize (XmlSchemaForm.Unqualified);
			Assert.AreEqual (Infoset ("<XmlSchemaForm>unqualified</XmlSchemaForm>"), WriterText, "#2");
		}

		[Test]
		public void TestSerializeXmlEnumAttribute_IgnoredValue ()
		{
			// technically XmlSchemaForm.None has an XmlIgnore attribute,
			// but it is not being serialized as a member.

#if NET_2_0
			try {
				Serialize (XmlSchemaForm.None);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#4");
				Assert.IsNotNull (ex.InnerException.Message, "#5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'0'") != -1, "#6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (XmlSchemaForm).FullName) != -1, "#7");
			}
#else
			Serialize (XmlSchemaForm.None);
			Assert.AreEqual (Infoset ("<XmlSchemaForm>0</XmlSchemaForm>"), WriterText);
#endif
		}

		[Test]
		public void TestSerializeXmlNodeArray ()
		{
			XmlDocument doc = new XmlDocument ();
			Serialize (new XmlNode [] { doc.CreateAttribute ("at"), doc.CreateElement ("elem1"), doc.CreateElement ("elem2") }, typeof (object));
			Assert.AreEqual (Infoset ("<anyType at=\"\"><elem1/><elem2/></anyType>"), WriterText);
		}

		[Test]
		public void TestSerializeXmlElement ()
		{
			XmlDocument doc = new XmlDocument ();
			Serialize (doc.CreateElement ("elem"), typeof (XmlElement));
			Assert.AreEqual (Infoset ("<elem/>"), WriterText);
		}

		[Test]
		public void TestSerializeXmlElementSubclass ()
		{
			XmlDocument doc = new XmlDocument ();
			Serialize (new MyElem (doc), typeof (XmlElement));
			Assert.AreEqual (Infoset ("<myelem aa=\"1\"/>"), WriterText, "#1");

			Serialize (new MyElem (doc), typeof (MyElem));
			Assert.AreEqual (Infoset ("<myelem aa=\"1\"/>"), WriterText, "#2");
		}

		[Test]
		public void TestSerializeXmlCDataSection ()
		{
			XmlDocument doc = new XmlDocument ();
			CDataContainer c = new CDataContainer ();
			c.cdata = doc.CreateCDataSection ("data section contents");
			Serialize (c);
			Assert.AreEqual (Infoset ("<CDataContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><cdata><![CDATA[data section contents]]></cdata></CDataContainer>"), WriterText);
		}

		[Test]
		public void TestSerializeXmlNode ()
		{
			XmlDocument doc = new XmlDocument ();
			NodeContainer c = new NodeContainer ();
			c.node = doc.CreateTextNode ("text");
			Serialize (c);
			Assert.AreEqual (Infoset ("<NodeContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><node>text</node></NodeContainer>"), WriterText);
		}

		[Test]
		public void TestSerializeChoice ()
		{
			Choices ch = new Choices ();
			ch.MyChoice = "choice text";
			ch.ItemType = ItemChoiceType.ChoiceZero;
			Serialize (ch);
			Assert.AreEqual (Infoset ("<Choices xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><ChoiceZero>choice text</ChoiceZero></Choices>"), WriterText, "#1");
			ch.ItemType = ItemChoiceType.StrangeOne;
			Serialize (ch);
			Assert.AreEqual (Infoset ("<Choices xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><ChoiceOne>choice text</ChoiceOne></Choices>"), WriterText, "#2");
			ch.ItemType = ItemChoiceType.ChoiceTwo;
			Serialize (ch);
			Assert.AreEqual (Infoset ("<Choices xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><ChoiceTwo>choice text</ChoiceTwo></Choices>"), WriterText, "#3");
		}

		[Test]
		public void TestSerializeNamesWithSpaces ()
		{
			TestSpace ts = new TestSpace ();
			ts.elem = 4;
			ts.attr = 5;
			Serialize (ts);
			Assert.AreEqual (Infoset ("<Type_x0020_with_x0020_space xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' Attribute_x0020_with_x0020_space='5'><Element_x0020_with_x0020_space>4</Element_x0020_with_x0020_space></Type_x0020_with_x0020_space>"), WriterText);
		}

		[Test]
		public void TestSerializeReadOnlyProps ()
		{
			ReadOnlyProperties ts = new ReadOnlyProperties ();
			Serialize (ts);
			Assert.AreEqual (Infoset ("<ReadOnlyProperties xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />"), WriterText);
		}

		[Test]
		public void TestSerializeIList ()
		{
			clsPerson k = new clsPerson ();
			k.EmailAccounts = new ArrayList ();
			k.EmailAccounts.Add ("a");
			k.EmailAccounts.Add ("b");
			Serialize (k);
			Assert.AreEqual (Infoset ("<clsPerson xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><EmailAccounts><anyType xsi:type=\"xsd:string\">a</anyType><anyType xsi:type=\"xsd:string\">b</anyType></EmailAccounts></clsPerson>"), WriterText);
		}

		[Test]
		public void TestSerializeArrayEnc ()
		{
			SoapReflectionImporter imp = new SoapReflectionImporter ();
			XmlTypeMapping map = imp.ImportTypeMapping (typeof (ArrayClass));
			XmlSerializer ser = new XmlSerializer (map);
			StringWriter sw = new StringWriter ();
			XmlTextWriter tw = new XmlTextWriter (sw);
			tw.WriteStartElement ("aa");
			ser.Serialize (tw, new ArrayClass ());
			tw.WriteEndElement ();
		}

		[Test]
		public void TestIncludeType ()
		{
			// Test for bug #76049
			XmlReflectionImporter imp = new XmlReflectionImporter ();
			XmlTypeMapping map = imp.ImportTypeMapping (typeof (object));
			imp.IncludeType (typeof (TestSpace));
			XmlSerializer ser = new XmlSerializer (map);
			ser.Serialize (new StringWriter (), new TestSpace ());
		}

		[Test]
		public void TestSerializeChoiceArray ()
		{
			CompositeValueType v = new CompositeValueType ();
			v.Init ();
			Serialize (v);
			Assert.AreEqual (Infoset ("<?xml version=\"1.0\" encoding=\"utf-16\"?><CompositeValueType xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><In>1</In><Es>2</Es></CompositeValueType>"), WriterText);
		}

		[Test]
		public void TestArrayAttributeWithDataType ()
		{
			Serialize (new ArrayAttributeWithType ());
			string res = "<ArrayAttributeWithType xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' ";
			res += "at='a b' bin1='AQI= AQI=' bin2='AQI=' />";
			Assert.AreEqual (Infoset (res), WriterText);
		}

		[Test]
		public void TestSubclassElementType ()
		{
			SubclassTestContainer c = new SubclassTestContainer ();
			c.data = new SubclassTestSub ();
			Serialize (c);

			string res = "<SubclassTestContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			res += "<a xsi:type=\"SubclassTestSub\"/></SubclassTestContainer>";
			Assert.AreEqual (Infoset (res), WriterText);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestArrayAttributeWithWrongDataType ()
		{
			Serialize (new ArrayAttributeWithWrongType ());
		}

		[Test]
		[Category ("NotWorking")]
		public void TestSerializePrimitiveTypesContainer ()
		{
			Serialize (new PrimitiveTypesContainer ());
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
#if NET_2_0
				"<PrimitiveTypesContainer xmlns:xsi='{1}' xmlns:xsd='{0}' xmlns='some:urn'>" +
#else
				"<PrimitiveTypesContainer xmlns:xsd='{0}' xmlns:xsi='{1}' xmlns='some:urn'>" +
#endif
				"<Number>2004</Number>" +
				"<Name>some name</Name>" +
				"<Index>56</Index>" +
				"<Password>8w8=</Password>" +
				"<PathSeparatorCharacter>47</PathSeparatorCharacter>" +
				"</PrimitiveTypesContainer>", XmlSchema.Namespace,
				XmlSchema.InstanceNamespace), sw.ToString (), "#1");

			SerializeEncoded (new PrimitiveTypesContainer ());
			Assert.AreEqual (string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
#if NET_2_0
				"<q1:PrimitiveTypesContainer xmlns:xsi='{1}' xmlns:xsd='{0}' id='id1' xmlns:q1='{2}'>" +
#else
				"<q1:PrimitiveTypesContainer xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' xmlns:q1='{2}'>" +
#endif
				"<Number xsi:type='xsd:int'>2004</Number>" +
				"<Name xsi:type='xsd:string'>some name</Name>" +
				"<Index xsi:type='xsd:unsignedByte'>56</Index>" +
				"<Password xsi:type='xsd:base64Binary'>8w8=</Password>" +
				"<PathSeparatorCharacter xmlns:q2='{3}' xsi:type='q2:char'>47</PathSeparatorCharacter>" +
				"</q1:PrimitiveTypesContainer>", XmlSchema.Namespace,
				XmlSchema.InstanceNamespace, AnotherNamespace, WsdlTypesNamespace),
				sw.ToString (), "#2");
		}

		[Test]
		public void TestSchemaForm ()
		{
			TestSchemaForm1 t1 = new TestSchemaForm1 ();
			t1.p1 = new PrintTypeResponse ();
			t1.p1.Init ();
			t1.p2 = new PrintTypeResponse ();
			t1.p2.Init ();

			TestSchemaForm2 t2 = new TestSchemaForm2 ();
			t2.p1 = new PrintTypeResponse ();
			t2.p1.Init ();
			t2.p2 = new PrintTypeResponse ();
			t2.p2.Init ();

			Serialize (t1);
			string res = "";
			res += "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
			res += "<TestSchemaForm1 xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">";
			res += "  <p1>";
			res += "    <result>";
			res += "      <data>data1</data>";
			res += "    </result>";
			res += "    <intern xmlns=\"urn:responseTypes\">";
			res += "      <result xmlns=\"\">";
			res += "        <data>data2</data>";
			res += "      </result>";
			res += "    </intern>";
			res += "  </p1>";
			res += "  <p2 xmlns=\"urn:oo\">";
			res += "    <result xmlns=\"\">";
			res += "      <data>data1</data>";
			res += "    </result>";
			res += "    <intern xmlns=\"urn:responseTypes\">";
			res += "      <result xmlns=\"\">";
			res += "        <data>data2</data>";
			res += "      </result>";
			res += "    </intern>";
			res += "  </p2>";
			res += "</TestSchemaForm1>";
			Assert.AreEqual (Infoset (res), WriterText);

			Serialize (t2);
			res = "";
			res += "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
			res += "<TestSchemaForm2 xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">";
			res += "  <p1 xmlns=\"urn:testForm\">";
			res += "    <result xmlns=\"\">";
			res += "      <data>data1</data>";
			res += "    </result>";
			res += "    <intern xmlns=\"urn:responseTypes\">";
			res += "      <result xmlns=\"\">";
			res += "        <data>data2</data>";
			res += "      </result>";
			res += "    </intern>";
			res += "  </p1>";
			res += "  <p2 xmlns=\"urn:oo\">";
			res += "    <result xmlns=\"\">";
			res += "      <data>data1</data>";
			res += "    </result>";
			res += "    <intern xmlns=\"urn:responseTypes\">";
			res += "      <result xmlns=\"\">";
			res += "        <data>data2</data>";
			res += "      </result>";
			res += "    </intern>";
			res += "  </p2>";
			res += "</TestSchemaForm2>";
			Assert.AreEqual (Infoset (res), WriterText);

			XmlReflectionImporter imp = new XmlReflectionImporter ();
			XmlTypeMapping map = imp.ImportTypeMapping (typeof (TestSchemaForm1), "urn:extra");
			Serialize (t1, map);
			res = "";
			res += "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
			res += "<TestSchemaForm1 xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"urn:extra\">";
			res += "  <p1>";
			res += "    <result xmlns=\"\">";
			res += "      <data>data1</data>";
			res += "    </result>";
			res += "    <intern xmlns=\"urn:responseTypes\">";
			res += "      <result xmlns=\"\">";
			res += "        <data>data2</data>";
			res += "      </result>";
			res += "    </intern>";
			res += "  </p1>";
			res += "  <p2 xmlns=\"urn:oo\">";
			res += "    <result xmlns=\"\">";
			res += "      <data>data1</data>";
			res += "    </result>";
			res += "    <intern xmlns=\"urn:responseTypes\">";
			res += "      <result xmlns=\"\">";
			res += "        <data>data2</data>";
			res += "      </result>";
			res += "    </intern>";
			res += "  </p2>";
			res += "</TestSchemaForm1>";
			Assert.AreEqual (Infoset (res), WriterText);

			imp = new XmlReflectionImporter ();
			map = imp.ImportTypeMapping (typeof (TestSchemaForm2), "urn:extra");
			Serialize (t2, map);
			res = "";
			res += "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
			res += "<TestSchemaForm2 xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"urn:extra\">";
			res += "  <p1 xmlns=\"urn:testForm\">";
			res += "    <result xmlns=\"\">";
			res += "      <data>data1</data>";
			res += "    </result>";
			res += "    <intern xmlns=\"urn:responseTypes\">";
			res += "      <result xmlns=\"\">";
			res += "        <data>data2</data>";
			res += "      </result>";
			res += "    </intern>";
			res += "  </p1>";
			res += "  <p2 xmlns=\"urn:oo\">";
			res += "    <result xmlns=\"\">";
			res += "      <data>data1</data>";
			res += "    </result>";
			res += "    <intern xmlns=\"urn:responseTypes\">";
			res += "      <result xmlns=\"\">";
			res += "        <data>data2</data>";
			res += "      </result>";
			res += "    </intern>";
			res += "  </p2>";
			res += "</TestSchemaForm2>";
			Assert.AreEqual (Infoset (res), WriterText);
		}

		// bug #78536
		[Test]
		public void CDataTextNodes ()
		{
			XmlSerializer ser = new XmlSerializer (typeof (CDataTextNodesType));
			ser.UnknownNode += new XmlNodeEventHandler (CDataTextNodes_BadNode);
			string xml = @"<CDataTextNodesType>
  <foo><![CDATA[
(?<filename>^([A-Z]:)?[^\(]+)\((?<line>\d+),(?<column>\d+)\):
\s((?<warning>warning)|(?<error>error))\s[^:]+:(?<message>.+$)|
(?<error>(fatal\s)?error)[^:]+:(?<message>.+$)
	]]></foo>
</CDataTextNodesType>";
			ser.Deserialize (new XmlTextReader (xml, XmlNodeType.Document, null));
		}

		public class CDataTextNodesType
		{
			public CDataTextNodesInternal foo;
		}

		public class CDataTextNodesInternal
		{
			[XmlText]
			public string Value;
		}

		void CDataTextNodes_BadNode (object s, XmlNodeEventArgs e)
		{
			Assert.Fail ();
		}

		// Helper methods

		public static string Infoset (string sx)
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (sx);
			StringBuilder sb = new StringBuilder ();
			GetInfoset (doc.DocumentElement, sb);
			return sb.ToString ();
		}

		public static string Infoset (XmlNode nod)
		{
			StringBuilder sb = new StringBuilder ();
			GetInfoset (nod, sb);
			return sb.ToString ();
		}

		static void GetInfoset (XmlNode nod, StringBuilder sb)
		{
			switch (nod.NodeType) {
			case XmlNodeType.Attribute:
				if (nod.LocalName == "xmlns" && nod.NamespaceURI == "http://www.w3.org/2000/xmlns/") return;
				sb.Append (" " + nod.NamespaceURI + ":" + nod.LocalName + "='" + nod.Value + "'");
				break;

			case XmlNodeType.Element:
				XmlElement elem = (XmlElement) nod;
				sb.Append ("<" + elem.NamespaceURI + ":" + elem.LocalName);

				ArrayList ats = new ArrayList ();
				foreach (XmlAttribute at in elem.Attributes)
					ats.Add (at.LocalName + " " + at.NamespaceURI);

				ats.Sort ();

				foreach (string name in ats) {
					string [] nn = name.Split (' ');
					GetInfoset (elem.Attributes [nn [0], nn [1]], sb);
				}

				sb.Append (">");
				foreach (XmlNode cn in elem.ChildNodes)
					GetInfoset (cn, sb);
				sb.Append ("</>");
				break;

			default:
				sb.Append (nod.OuterXml);
				break;
			}
		}

		static XmlTypeMapping CreateSoapMapping (Type type)
		{
			SoapReflectionImporter importer = new SoapReflectionImporter ();
			return importer.ImportTypeMapping (type);
		}

		static XmlTypeMapping CreateSoapMapping (Type type, SoapAttributeOverrides ao)
		{
			SoapReflectionImporter importer = new SoapReflectionImporter (ao);
			return importer.ImportTypeMapping (type);
		}

		static XmlTypeMapping CreateSoapMapping (Type type, SoapAttributeOverrides ao, string defaultNamespace)
		{
			SoapReflectionImporter importer = new SoapReflectionImporter (ao, defaultNamespace);
			return importer.ImportTypeMapping (type);
		}
	}
}
