//
// System.Xml.DeserializationTests
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2003 Atsushi Enomoto
//
//
using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;
using MonoTests.System.Xml.TestClasses;

namespace MonoTests.System.XmlSerialization
{
	public class Sample
	{
		public string Text;
		public string [] ArrayText;
	}

	[TestFixture]
	public class DeserializationTests
	{
		const string XmlSchemaNamespace = "http://www.w3.org/2001/XMLSchema";
		const string XmlSchemaInstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
		const string SoapEncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";
		const string WsdlTypesNamespace = "http://microsoft.com/wsdl/types/";
		const string ANamespace = "some:urn";
		const string AnotherNamespace = "another:urn";

		object result;

		private object Deserialize (Type t, string xml)
		{
			StringReader sr = new StringReader (xml);
			XmlReader xr = new XmlTextReader (sr);
			return Deserialize (t, xr);
		}

		private object Deserialize (Type t, string xml, string defaultNamespace)
		{
			StringReader sr = new StringReader (xml);
			XmlReader xr = new XmlTextReader (sr);
			return Deserialize (t, xr, defaultNamespace);
		}

		private object Deserialize (Type t, string xml, XmlAttributeOverrides ao)
		{
			StringReader sr = new StringReader (xml);
			XmlReader xr = new XmlTextReader (sr);
			return Deserialize (t, xr, ao);
		}

		private object DeserializeEncoded (Type t, string xml)
		{
			StringReader sr = new StringReader (xml);
			XmlReader xr = new XmlTextReader (sr);
			return DeserializeEncoded (t, xr);
		}

		private object Deserialize (Type t, XmlReader xr)
		{
			XmlSerializer ser = new XmlSerializer (t);
			result = ser.Deserialize (xr);
			return result;
		}

		private object Deserialize (Type t, XmlReader xr, string defaultNamespace)
		{
			XmlSerializer ser = new XmlSerializer (t, defaultNamespace);
			result = ser.Deserialize (xr);
			return result;
		}

		private object Deserialize (Type t, XmlReader xr, XmlAttributeOverrides ao)
		{
			XmlSerializer ser = new XmlSerializer (t, ao);
			result = ser.Deserialize (xr);
			return result;
		}

		private object DeserializeEncoded (Type t, XmlReader xr)
		{
			SoapReflectionImporter im = new SoapReflectionImporter ();
			XmlTypeMapping tm = im.ImportTypeMapping (t);
			XmlSerializer ser = new XmlSerializer (tm);
			result = ser.Deserialize (xr);
			return result;
		}

		[Test]
		public void SimpleDeserialize ()
		{
			Deserialize (typeof (Sample), "<Sample><Text>Test.</Text></Sample>");
			Assert.AreEqual (typeof (Sample), result.GetType ());
			Sample sample = result as Sample;
			Assert.AreEqual ("Test.", sample.Text);
		}

		[Test]
		public void DeserializeInt ()
		{
			Deserialize (typeof (int), "<int>10</int>");
			Assert.AreEqual (typeof (int), result.GetType ());
			Assert.AreEqual (10, result);
		}

		[Test]
		public void DeserializeSimpleArray ()
		{
			Deserialize (typeof (Sample), "<Sample><ArrayText><string>Test1</string><string>Test2</string></ArrayText></Sample>");
			Assert.AreEqual (typeof (Sample), result.GetType ());
			Sample sample = result as Sample;
			Assert.AreEqual ("Test1", sample.ArrayText[0]);
			Assert.AreEqual ("Test2", sample.ArrayText[1]);
		}

		[Test]
		public void DeserializeEmptyEnum ()
		{
			Field f = Deserialize (typeof (Field), "<field modifiers=\"\" />") as Field;
			Assert.AreEqual (MapModifiers.Public, f.Modifiers);
		}
		
		[Test]
		public void DeserializePrivateCollection ()
		{
			MemoryStream ms = new MemoryStream ();
			Container c = new Container();
			c.Items.Add(1);
			
			XmlSerializer serializer = new XmlSerializer(typeof(Container));
			serializer.Serialize(ms, c);
			
			ms.Position = 0;
			c = (Container) serializer.Deserialize (ms);
			Assert.AreEqual (1, c.Items[0]);
		}
		
		[Test]
		[Category("NotDotNet")]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DeserializeEmptyPrivateCollection ()
		{
			MemoryStream ms = new MemoryStream ();
			Container2 c = new Container2(true);
			c.Items.Add(1);
			
			XmlSerializer serializer = new XmlSerializer(typeof(Container2));
			serializer.Serialize(ms, c);
			
			ms.Position = 0;
			c = (Container2) serializer.Deserialize (ms);
		}
		
		[Test]
		[Category("NotDotNet")]
		public void DeserializeArrayReferences ()
		{
			string s = "<Sample xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">";
			s += "<ArrayText xmlns:n3=\"http://schemas.xmlsoap.org/soap/encoding/\" xsi:type=\"n3:Array\" n3:arrayType=\"xsd:string[2]\">";
			s += "<item href=\"#id-606830706\"></item>";
			s += "<item xsi:type=\"xsd:string\">Hola</item>";
			s += "</ArrayText>";
			s += "<string id=\"id-606830706\" xsi:type=\"xsd:string\">Adeu</string>";
			s += "</Sample>";
			DeserializeEncoded (typeof(Sample), s);
		}
		
		
		[Test]
		public void TestDeserializeXmlNodeArray ()
		{
			object ob = Deserialize (typeof(object), "<anyType at=\"1\"><elem1/><elem2/></anyType>");
			Assert.IsTrue (ob is XmlNode[], "Is node array");
			
			XmlNode[] nods = (XmlNode[]) ob;
			Assert.AreEqual (3, nods.Length, "lengh");
			Assert.IsTrue (nods[0] is XmlAttribute, "#1");
			Assert.AreEqual ("at", ((XmlAttribute) nods[0]).LocalName, "#2");
			Assert.AreEqual ("1", ((XmlAttribute) nods[0]).Value, "#3");
			Assert.IsTrue (nods[1] is XmlElement, "#4");
			Assert.AreEqual ("elem1", ((XmlElement) nods[1]).LocalName, "#5");
			Assert.IsTrue (nods[2] is XmlElement, "#6");
			Assert.AreEqual ("elem2", ((XmlElement) nods[2]).LocalName, "#7");
		}
		
		[Test]
		public void TestDeserializeXmlElement ()
		{
			object ob = Deserialize (typeof(XmlElement), "<elem/>");
			Assert.IsTrue (ob is XmlElement, "#1");
			Assert.AreEqual ("elem", ((XmlElement) ob).LocalName, "#2");
		}
		
		[Test]
		public void TestDeserializeXmlCDataSection ()
		{
			CDataContainer c = (CDataContainer) Deserialize (typeof(CDataContainer), "<CDataContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><cdata><![CDATA[data section contents]]></cdata></CDataContainer>");
			Assert.IsNotNull (c.cdata, "#1");
			Assert.AreEqual ("data section contents", c.cdata.Value, "#2");
		}
		
		[Test]
		public void TestDeserializeXmlNode ()
		{
			NodeContainer c = (NodeContainer) Deserialize (typeof(NodeContainer), "<NodeContainer xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><node>text</node></NodeContainer>");
			Assert.IsTrue (c.node is XmlText, "#1");
			Assert.AreEqual ("text", c.node.Value, "#2");
		}
		
		[Test]
		public void TestDeserializeChoices ()
		{
			Choices ch = (Choices) Deserialize (typeof(Choices), "<Choices><ChoiceZero>choice text</ChoiceZero></Choices>");
			Assert.AreEqual ("choice text", ch.MyChoice, "#A1");
			Assert.AreEqual (ItemChoiceType.ChoiceZero, ch.ItemType, "#A2");
			
			ch = (Choices) Deserialize (typeof(Choices), "<Choices><ChoiceOne>choice text</ChoiceOne></Choices>");
			Assert.AreEqual ("choice text", ch.MyChoice, "#B1");
			Assert.AreEqual (ItemChoiceType.StrangeOne, ch.ItemType, "#B2");
			
			ch = (Choices) Deserialize (typeof(Choices), "<Choices><ChoiceTwo>choice text</ChoiceTwo></Choices>");
			Assert.AreEqual ("choice text", ch.MyChoice, "#C1");
			Assert.AreEqual (ItemChoiceType.ChoiceTwo, ch.ItemType, "#C2");
		}
		
		[Test]
		public void TestDeserializeNamesWithSpaces ()
		{
			TestSpace ts = (TestSpace) Deserialize (typeof(TestSpace), "<Type_x0020_with_x0020_space xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' Attribute_x0020_with_x0020_space='5'><Element_x0020_with_x0020_space>4</Element_x0020_with_x0020_space></Type_x0020_with_x0020_space>");
			Assert.AreEqual (4, ts.elem, "#1");
			Assert.AreEqual (5, ts.attr, "#2");
		}
		
		[Test]
		public void TestDeserializeDefaults ()
		{
			ListDefaults d2 = (ListDefaults) Deserialize (typeof(ListDefaults), "<root/>");

			Assert.IsNotNull (d2.list2, "#A1");
			Assert.IsNull (d2.list3, "#A2");
			Assert.IsNull (d2.list4, "#A3");
			Assert.IsNotNull (d2.list5, "#A4");
			Assert.IsNotNull (d2.ed, "#A5");
			Assert.IsNotNull (d2.str, "#A6");

			d2 = (ListDefaults) Deserialize (typeof(ListDefaults), "<root></root>");

			Assert.IsNotNull (d2.list2, "#B1");
			Assert.IsNull (d2.list3, "#B2");
			Assert.IsNull (d2.list4, "#B3");
			Assert.IsNotNull (d2.list5, "#B4");
			Assert.IsNotNull (d2.ed, "#B5");
			Assert.IsNotNull (d2.str, "#B6");
		}
		
		[Test]
		public void TestDeserializeChoiceArray ()
		{
			CompositeValueType v = (CompositeValueType) Deserialize (typeof(CompositeValueType), "<?xml version=\"1.0\" encoding=\"utf-16\"?><CompositeValueType xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><In>1</In><Es>2</Es></CompositeValueType>");
			Assert.IsNotNull (v.Items, "#1");
			Assert.IsNotNull (v.ItemsElementName, "#2");
			Assert.AreEqual (2, v.Items.Length, "#3");
			Assert.AreEqual (2, v.ItemsElementName.Length, "#4");
			Assert.AreEqual (1, v.Items[0], "#5");
			Assert.AreEqual (2, v.Items[1], "#6");
			Assert.AreEqual (ItemsChoiceType.In, v.ItemsElementName[0], "#7");
			Assert.AreEqual (ItemsChoiceType.Es, v.ItemsElementName[1], "#8");
		}
		
		[Test]
		public void TestDeserializeCollection ()
		{
			string s0 = "";
			s0+="	<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s0+="		<Entity Name='node1'/>";
			s0+="		<Entity Name='node2'/>";
			s0+="	</ArrayOfEntity>";
			
			EntityCollection col = (EntityCollection) Deserialize (typeof(EntityCollection), s0);
			Assert.IsNotNull (col, "#1");
			Assert.AreEqual (2, col.Count, "#2");
			Assert.IsNull (col[0].Parent, "#3");
			Assert.IsNull (col[1].Parent, "#4");
		}
		
		[Test]
		public void TestDeserializeEmptyCollection ()
		{
			string s1 = "";
			s1+="	<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			
			EntityCollection col = (EntityCollection) Deserialize (typeof(EntityCollection), s1);
			Assert.IsNotNull (col, "#A1");
			Assert.AreEqual (0, col.Count, "#A2");
			
			string s1_1 = "";
			s1_1+="	<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s1_1+="	</ArrayOfEntity>";
			
			col = (EntityCollection) Deserialize (typeof(EntityCollection), s1_1);
			Assert.IsNotNull (col, "#B1");
			Assert.AreEqual (0, col.Count, "#B2");
		}
		
		[Test]
		public void TestDeserializeNilCollectionIsNotNull ()
		{
			string s2 = "";
			s2+="	<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true'/>";
			
			EntityCollection col = (EntityCollection) Deserialize (typeof(EntityCollection), s2);
			Assert.IsNotNull (col, "#1");
			Assert.AreEqual (0, col.Count, "#2");
		}
		
		[Test]
		public void TestDeserializeObjectCollections ()
		{
			string s3 = "";
			s3+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3+="	<Collection1 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3+="		<Entity Name='node1'/>";
			s3+="		<Entity Name='node2'/>";
			s3+="	</Collection1>";
			s3+="	<Collection2 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3+="		<Entity Name='node1'/>";
			s3+="		<Entity Name='node2'/>";
			s3+="	</Collection2>";
			s3+="	<Collection3 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3+="		<Entity Name='node1'/>";
			s3+="		<Entity Name='node2'/>";
			s3+="	</Collection3>";
			s3+="	<Collection4 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3+="		<Entity Name='node1'/>";
			s3+="		<Entity Name='node2'/>";
			s3+="	</Collection4>";
			s3+="</Container>";
			
			EntityContainer cont = (EntityContainer) Deserialize (typeof(EntityContainer), s3);
			Assert.IsNotNull (cont, "#A1");

			Assert.IsNotNull (cont.Collection1, "#B1");
			Assert.AreEqual (2, cont.Collection1.Count, "#B2");
			Assert.AreEqual ("assigned", cont.Collection1.Container, "#B3");
			Assert.AreEqual ("assigned", cont.Collection1[0].Parent, "#B4");
			Assert.AreEqual ("assigned", cont.Collection1[1].Parent, "#B5");

			Assert.IsNotNull (cont.Collection2, "#C1");
			Assert.AreEqual (2, cont.Collection2.Count, "#C2");
			Assert.AreEqual ("assigned", cont.Collection2.Container, "#C3");
			Assert.AreEqual ("assigned", cont.Collection2[0].Parent, "#C4");
			Assert.AreEqual ("assigned", cont.Collection2[1].Parent, "#C5");

			Assert.IsNotNull (cont.Collection3, "#D1");
			Assert.AreEqual (2, cont.Collection3.Count, "#D2");
			Assert.AreEqual ("root", cont.Collection3.Container, "#D3");
			Assert.AreEqual ("root", cont.Collection3[0].Parent, "#D4");
			Assert.AreEqual ("root", cont.Collection3[1].Parent, "#D5");

			Assert.IsNotNull (cont.Collection4, "#E1");
			Assert.AreEqual (2, cont.Collection4.Count, "#E2");
			Assert.AreEqual ("root", cont.Collection4.Container, "#E3");
			Assert.AreEqual ("root", cont.Collection4[0].Parent, "#E4");
			Assert.AreEqual ("root", cont.Collection4[1].Parent, "#E5");
		}
		
		[Test]
		public void TestDeserializeEmptyObjectCollections ()
		{
			string s4 = "";
			s4+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s4+="</Container>";
			
			EntityContainer cont = (EntityContainer) Deserialize (typeof(EntityContainer), s4);
			Assert.IsNotNull (cont, "#A1");

			Assert.IsNotNull (cont.Collection1, "#B1");
			Assert.AreEqual (0, cont.Collection1.Count, "#B2");
			Assert.AreEqual ("assigned", cont.Collection1.Container, "#B3");

			Assert.IsNotNull (cont.Collection2, "#C1");
			Assert.AreEqual (0, cont.Collection2.Count, "#C2");
			Assert.AreEqual ("assigned", cont.Collection2.Container, "#C3");

			Assert.IsNotNull (cont.Collection3, "#D1");
			Assert.AreEqual (0, cont.Collection3.Count, "#D2");
			Assert.AreEqual ("root", cont.Collection3.Container, "#D3");

			Assert.IsNotNull (cont.Collection4, "#E1");
			Assert.AreEqual (0, cont.Collection4.Count, "#E2");
			Assert.AreEqual ("root", cont.Collection4.Container, "#E3");
		}

		[Test]
		public void TestDeserializeObjectNilCollectionsAreNotNull ()
		{
			string s5 = "";
			s5+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s5+="	<Collection1 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5+="	<Collection2 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5+="	<Collection3 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5+="	<Collection4 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5+="</Container>";
			
			EntityContainer cont = (EntityContainer) Deserialize (typeof(EntityContainer), s5);
			Assert.IsNotNull (cont, "#A1");

			Assert.IsNotNull (cont.Collection1, "#B1");
			Assert.AreEqual (0, cont.Collection1.Count, "#B2");
			Assert.AreEqual ("assigned", cont.Collection1.Container, "#B3");

			Assert.IsNotNull (cont.Collection2, "#C1");
			Assert.AreEqual (0, cont.Collection2.Count, "#C2");
			Assert.AreEqual ("assigned", cont.Collection2.Container, "#C3");

			Assert.IsNotNull (cont.Collection3, "#D1");
			Assert.AreEqual (0, cont.Collection3.Count, "#D2");
			Assert.AreEqual ("root", cont.Collection3.Container, "#D3");

			Assert.IsNotNull (cont.Collection4, "#E1");
			Assert.AreEqual (0, cont.Collection4.Count, "#E2");
			Assert.AreEqual ("root", cont.Collection4.Container, "#E3");
		}
		
		[Test]
		public void TestDeserializeObjectEmptyCollections ()
		{
			string s6 = "";
			s6+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s6+="	<Collection1 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6+="	<Collection2 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6+="	<Collection3 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6+="	<Collection4 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6+="</Container>";
			
			EntityContainer cont = (EntityContainer) Deserialize (typeof(EntityContainer), s6);
			Assert.IsNotNull (cont, "#A1");

			Assert.IsNotNull (cont.Collection1, "#B1");
			Assert.AreEqual (0, cont.Collection1.Count, "#B2");
			Assert.AreEqual ("assigned", cont.Collection1.Container, "#B3");

			Assert.IsNotNull (cont.Collection2, "#C1");
			Assert.AreEqual (0, cont.Collection2.Count, "#C2");
			Assert.AreEqual ("assigned", cont.Collection2.Container, "#C3");

			Assert.IsNotNull (cont.Collection3, "#D1");
			Assert.AreEqual (0, cont.Collection3.Count, "#D2");
			Assert.AreEqual ("root", cont.Collection3.Container, "#D3");

			Assert.IsNotNull (cont.Collection4, "#E1");
			Assert.AreEqual (0, cont.Collection4.Count, "#E2");
			Assert.AreEqual ("root", cont.Collection4.Container, "#E3");
		}
		
		[Test]
		public void TestDeserializeObjectEmptyArrays ()
		{
			string s6 = "";
			s6+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s6+="	<Collection1 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6+="	<Collection2 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6+="	<Collection3 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6+="	<Collection4 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			s6+="</Container>";
			
			ArrayEntityContainer cont = (ArrayEntityContainer) Deserialize (typeof(ArrayEntityContainer), s6);
			Assert.IsNotNull (cont, "#A1");

			Assert.IsNotNull (cont.Collection1, "#B1");
			Assert.AreEqual (0, cont.Collection1.Length, "#B2");

			Assert.IsNotNull (cont.Collection2, "#C1");
			Assert.AreEqual (0, cont.Collection2.Length, "#C2");

			Assert.IsNotNull (cont.Collection3, "#D1");
			Assert.AreEqual (0, cont.Collection3.Length, "#D2");

			Assert.IsNotNull (cont.Collection4, "#E1");
			Assert.AreEqual (0, cont.Collection4.Length, "#E2");
		}
		
		[Test]
		public void TestDeserializeEmptyObjectArrays ()
		{
			string s4 = "";
			s4+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s4+="</Container>";
			
			ArrayEntityContainer cont = (ArrayEntityContainer) Deserialize (typeof(ArrayEntityContainer), s4);
			Assert.IsNotNull (cont, "#A1");

			Assert.IsNull (cont.Collection1, "#B1");
			Assert.IsNull (cont.Collection2, "#B2");

			Assert.IsNotNull (cont.Collection3, "#C1");
			Assert.AreEqual (0, cont.Collection3.Length, "#C2");

			Assert.IsNotNull (cont.Collection4, "#D1");
			Assert.AreEqual (0, cont.Collection4.Length, "#D2");
		}
		
		[Test]
		public void TestDeserializeObjectNilArrays ()
		{
			string s5 = "";
			s5+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s5+="	<Collection1 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5+="	<Collection2 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5+="	<Collection3 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5+="	<Collection4 xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true' />";
			s5+="</Container>";
			
			ArrayEntityContainer cont = (ArrayEntityContainer) Deserialize (typeof(ArrayEntityContainer), s5);
			Assert.IsNotNull (cont, "#A1");

			Assert.IsNull (cont.Collection1, "#B1");
			Assert.IsNull (cont.Collection2, "#B2");
			Assert.IsNull (cont.Collection3, "#B3");

			Assert.IsNotNull (cont.Collection4, "#C1");
			Assert.AreEqual (0, cont.Collection4.Length, "#C2");
		}
		
		[Test]
		public void TestDeserializeEmptyArray ()
		{
			string s1 = "";
			s1+="<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' />";
			
			Entity[] col = (Entity[]) Deserialize (typeof(Entity[]), s1);
			Assert.IsNotNull (col, "#A1");
			Assert.AreEqual (0, col.Length, "#A2");
			
			string s1_1 = "";
			s1_1+="	<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s1_1+="	</ArrayOfEntity>";
			
			col = (Entity[]) Deserialize (typeof(Entity[]), s1_1);
			Assert.IsNotNull (col, "#B1");
			Assert.AreEqual (0, col.Length, "#B2");
		}
		
		[Test]
		public void TestDeserializeNilArray ()
		{
			string s2 = "";
			s2 += "<ArrayOfEntity xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:nil='true'/>";
			
			Entity[] col = (Entity[]) Deserialize (typeof(Entity[]), s2);
			Assert.IsNull (col, "#1");
		}
		
		[Test]
		public void TestDeserializeObjectWithReadonlyCollection ()
		{
			string s3 = "";
			s3+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3+="	<Collection1>";
			s3+="		<Entity Name='node1'/>";
			s3+="		<Entity Name='node2'/>";
			s3+="	</Collection1>";
			s3+="</Container>";
			
			ObjectWithReadonlyCollection cont = (ObjectWithReadonlyCollection) Deserialize (typeof(ObjectWithReadonlyCollection), s3);
			Assert.IsNotNull (cont, "#1");
			Assert.IsNotNull (cont.Collection1, "#2");
			Assert.AreEqual (2, cont.Collection1.Count, "#3");
			Assert.AreEqual ("root", cont.Collection1.Container, "#4");
			Assert.AreEqual ("root", cont.Collection1[0].Parent, "#5");
			Assert.AreEqual ("root", cont.Collection1[1].Parent, "#6");
		}
		
		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		public void TestDeserializeObjectWithReadonlyNulCollection ()
		{
			string s3 = "";
			s3+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3+="	<Collection1>";
			s3+="		<Entity Name='node1'/>";
			s3+="		<Entity Name='node2'/>";
			s3+="	</Collection1>";
			s3+="</Container>";
			
			Deserialize (typeof(ObjectWithReadonlyNulCollection), s3);
		}
		
		[Test]
		public void TestDeserializeObjectWithReadonlyArray ()
		{
			string s3 = "";
			s3+="<Container xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>";
			s3+="	<Collection1>";
			s3+="		<Entity Name='node1'/>";
			s3+="		<Entity Name='node2'/>";
			s3+="	</Collection1>";
			s3+="</Container>";
			
			ObjectWithReadonlyArray cont = (ObjectWithReadonlyArray) Deserialize (typeof(ObjectWithReadonlyArray), s3);
			Assert.IsNotNull (cont, "#1");
			Assert.IsNotNull (cont.Collection1, "#2");
			Assert.AreEqual (0, cont.Collection1.Length, "#3");
		}

		[Test]
		public void TestDeserialize_EnumDefaultValue ()
		{
			EnumDefaultValue e;

			e = (EnumDefaultValue) Deserialize (typeof (EnumDefaultValue), "<EnumDefaultValue />");
			Assert.AreEqual (0, (int) e, "#1");

			e = (EnumDefaultValue) Deserialize (typeof (EnumDefaultValue), "<EnumDefaultValue> e3</EnumDefaultValue>");
			Assert.AreEqual (EnumDefaultValue.e3, e, "#2");

			e = (EnumDefaultValue) Deserialize (typeof (EnumDefaultValue), "<EnumDefaultValue>e1 e2</EnumDefaultValue>");
			Assert.AreEqual (EnumDefaultValue.e3, e, "#3");

			e = (EnumDefaultValue) Deserialize (typeof (EnumDefaultValue), "<EnumDefaultValue>  e1   e2 </EnumDefaultValue>");
			Assert.AreEqual (EnumDefaultValue.e1 | EnumDefaultValue.e2, e, "#4");
		}

		[Test]
		public void TestDeserialize_EnumDefaultValueNF ()
		{
			EnumDefaultValueNF e;

			e = (EnumDefaultValueNF) Deserialize (typeof (EnumDefaultValueNF), "<EnumDefaultValueNF>e3</EnumDefaultValueNF>");
			Assert.AreEqual (EnumDefaultValueNF.e3, e, "#A1");

			try {
				Deserialize (typeof (EnumDefaultValueNF), "<EnumDefaultValueNF />");
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.InnerException, "#B3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#B4");
				Assert.IsNotNull (ex.InnerException.Message, "#B5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("''") != -1, "#B6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (EnumDefaultValueNF).Name) != -1, "#B7");
			}

			try {
				Deserialize (typeof (EnumDefaultValueNF), "<EnumDefaultValueNF>e1 e3</EnumDefaultValueNF>");
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.InnerException, "#C3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#C4");
				Assert.IsNotNull (ex.InnerException.Message, "#C5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'e1 e3'") != -1, "#C6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (EnumDefaultValueNF).Name) != -1, "#C7");
			}

			try {
				Deserialize (typeof (EnumDefaultValueNF), "<EnumDefaultValueNF> e3</EnumDefaultValueNF>");
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.InnerException, "#D3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#D4");
				Assert.IsNotNull (ex.InnerException.Message, "#D5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("' e3'") != -1, "#D6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (EnumDefaultValueNF).Name) != -1, "#D7");
			}

			try {
				Deserialize (typeof (EnumDefaultValueNF), "<EnumDefaultValueNF> </EnumDefaultValueNF>");
				Assert.Fail ("#E1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
				Assert.IsNotNull (ex.InnerException, "#E3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#E4");
				Assert.IsNotNull (ex.InnerException.Message, "#E5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("' '") != -1, "#E6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (EnumDefaultValueNF).Name) != -1, "#E7");
			}

			try {
				Deserialize (typeof (EnumDefaultValueNF), "<EnumDefaultValueNF>1</EnumDefaultValueNF>");
				Assert.Fail ("#F1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#F2");
				Assert.IsNotNull (ex.InnerException, "#F3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#F4");
				Assert.IsNotNull (ex.InnerException.Message, "#F5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'1'") != -1, "#F6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (EnumDefaultValueNF).Name) != -1, "#F7");
			}
		}

		[Test]
		[Category ("NotWorking")] // DefaultValue should not be used when deserializing
		public void TestDeserialize_Field ()
		{
			Field f = null;

			f = (Field) Deserialize (typeof (Field),
				string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
				"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag1='' flag2='' flag4='' modifiers='public' modifiers2='public' modifiers4='public' />",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace));
			Assert.AreEqual ((FlagEnum) 0, f.Flags1, "#A1");
			Assert.AreEqual ((FlagEnum) 0, f.Flags2, "#A2");
			Assert.AreEqual ((FlagEnum) 0, f.Flags3, "#A3");
			Assert.AreEqual ((FlagEnum) 0, f.Flags4, "#A4");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers, "#A5");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers2, "#A6");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers3, "#A7");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers4, "#A8");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers5, "#A9");
			Assert.IsNull (f.Names, "#A10");
			Assert.IsNull (f.Street, "#A11");

			f = (Field) Deserialize (typeof (Field),
				string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
				"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag3='two' flag4='' modifiers='protected' modifiers2='public' />",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace));
			Assert.AreEqual ((FlagEnum) 0, f.Flags1, "#B1");
			Assert.AreEqual ((FlagEnum) 0, f.Flags2, "#B2");
			Assert.AreEqual (FlagEnum.e2, f.Flags3, "#B3");
			Assert.AreEqual ((FlagEnum) 0, f.Flags4, "#B4");
			Assert.AreEqual (MapModifiers.Protected, f.Modifiers, "#B5");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers2, "#B6");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers3, "#B7");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers4, "#B8");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers5, "#B9");
			Assert.IsNull (f.Names, "#B10");
			Assert.IsNull (f.Street, "#B11");

			f = (Field) Deserialize (typeof (Field),
				string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
				"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag1='two' flag2='two' flag4='' modifiers='public' modifiers2='protected' modifiers3='protected' modifiers4='public' modifiers5='protected' />",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace));
			Assert.AreEqual (FlagEnum.e2, f.Flags1, "#C1");
			Assert.AreEqual (FlagEnum.e2, f.Flags2, "#C2");
			Assert.AreEqual ((FlagEnum) 0, f.Flags3, "#C3");
			Assert.AreEqual ((FlagEnum) 0, f.Flags4, "#C4");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers, "#C5");
			Assert.AreEqual (MapModifiers.Protected, f.Modifiers2, "#C6");
			Assert.AreEqual (MapModifiers.Protected, f.Modifiers3, "#C7");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers4, "#C8");
			Assert.AreEqual (MapModifiers.Protected, f.Modifiers5, "#C9");
			Assert.IsNull (f.Names, "#C10");
			Assert.IsNull (f.Street, "#C11");

			try {
				f = (Field) Deserialize (typeof (Field),
					string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
					"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag2='444' />",
					XmlSchemaNamespace, XmlSchemaInstanceNamespace));
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				// There was an error generating the XML document
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.Message, "#D3");
				Assert.IsNotNull (ex.InnerException, "#D4");

				// '444' is not a valid value for MonoTests.System.Xml.TestClasses.FlagEnum
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#D5");
				Assert.IsNotNull (ex.InnerException.Message, "#D6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'444'") != -1, "#D7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (FlagEnum).FullName) != -1, "#D8");
				Assert.IsNull (ex.InnerException.InnerException, "#D9");
			}

			try {
				f = (Field) Deserialize (typeof (Field),
					string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
					"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag2='Garbage' />",
					XmlSchemaNamespace, XmlSchemaInstanceNamespace));
				Assert.Fail ("#E1");
			} catch (InvalidOperationException ex) {
				// There was an error generating the XML document
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
				Assert.IsNotNull (ex.Message, "#E3");
				Assert.IsNotNull (ex.InnerException, "#E4");

				// 'Garbage' is not a valid value for MonoTests.System.Xml.TestClasses.FlagEnum
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#E5");
				Assert.IsNotNull (ex.InnerException.Message, "#E6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'Garbage'") != -1, "#E7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (FlagEnum).FullName) != -1, "#E8");
				Assert.IsNull (ex.InnerException.InnerException, "#E9");
			}

			try {
				f = (Field) Deserialize (typeof (Field),
					string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
					"<field xmlns:xsd='{0}' xmlns:xsi='{1}' flag2='{2}' />",
					XmlSchemaNamespace, XmlSchemaInstanceNamespace, ((int) FlagEnum.e2).ToString (CultureInfo.InvariantCulture)));
				Assert.Fail ("#F1");
			} catch (InvalidOperationException ex) {
				// There was an error generating the XML document
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#F2");
				Assert.IsNotNull (ex.Message, "#F3");
				Assert.IsNotNull (ex.InnerException, "#F4");

				// '2' is not a valid value for MonoTests.System.Xml.TestClasses.FlagEnum
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#F5");
				Assert.IsNotNull (ex.InnerException.Message, "#F6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'2'") != -1, "#F7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (FlagEnum).FullName) != -1, "#F8");
				Assert.IsNull (ex.InnerException.InnerException, "#F9");
			}
		}

		[Test]
		[Category ("NotDotNet")] // MS.NET does not allow SoapAttribute backed by enum ??
		public void TestDeserialize_Field_Encoded ()
		{
			Field_Encoded f = null;

			f = (Field_Encoded) DeserializeEncoded (typeof (Field_Encoded),
				string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
				"<q1:field xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' flag1='' flag2='' flag4='' modifiers='PuBlIc' modifiers2='PuBlIc' modifiers4='PuBlIc' xmlns:q1='{2}' />",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace, ANamespace));
			Assert.AreEqual ((FlagEnum_Encoded) 0, f.Flags1, "#A1");
			Assert.AreEqual ((FlagEnum_Encoded) 0, f.Flags2, "#A2");
			Assert.AreEqual (FlagEnum_Encoded.e1 | FlagEnum_Encoded.e2, f.Flags3, "#A3");
			Assert.AreEqual ((FlagEnum_Encoded) 0, f.Flags4, "#A4");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers, "#A5");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers2, "#A6");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers3, "#A7");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers4, "#A8");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers5, "#A9");
			Assert.IsNull (f.Names, "#A10");
			Assert.IsNull (f.Street, "#A11");

			f = (Field_Encoded) DeserializeEncoded (typeof (Field_Encoded),
				string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
				"<q1:field xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' flag3='two' flag4='' modifiers='Protected' modifiers2='PuBlIc' xmlns:q1='{2}' />",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace, ANamespace));
			Assert.AreEqual (FlagEnum_Encoded.e1, f.Flags1, "#B1");
			Assert.AreEqual (FlagEnum_Encoded.e1, f.Flags2, "#B2");
			Assert.AreEqual (FlagEnum_Encoded.e2, f.Flags3, "#B3");
			Assert.AreEqual ((FlagEnum_Encoded) 0, f.Flags4, "#B4");
			Assert.AreEqual (MapModifiers.Protected, f.Modifiers, "#B5");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers2, "#B6");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers3, "#B7");
			Assert.AreEqual (MapModifiers.Protected, f.Modifiers4, "#B8");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers5, "#B9");
			Assert.IsNull (f.Names, "#B10");
			Assert.IsNull (f.Street, "#B11");

			f = (Field_Encoded) DeserializeEncoded (typeof (Field_Encoded),
				string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
				"<q1:field xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' flag1='two' flag2='two' flag4='' modifiers='PuBlIc' modifiers2='Protected' modifiers3='Protected' modifiers4='PuBlIc' modifiers5='Protected' xmlns:q1='{2}' />",
				XmlSchemaNamespace, XmlSchemaInstanceNamespace, ANamespace));
			Assert.AreEqual (FlagEnum_Encoded.e2, f.Flags1, "#C1");
			Assert.AreEqual (FlagEnum_Encoded.e2, f.Flags2, "#C2");
			Assert.AreEqual (FlagEnum_Encoded.e1 | FlagEnum_Encoded.e2, f.Flags3, "#C3");
			Assert.AreEqual ((FlagEnum_Encoded) 0, f.Flags4, "#C4");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers, "#C5");
			Assert.AreEqual (MapModifiers.Protected, f.Modifiers2, "#C6");
			Assert.AreEqual (MapModifiers.Protected, f.Modifiers3, "#C7");
			Assert.AreEqual (MapModifiers.Public, f.Modifiers4, "#C8");
			Assert.AreEqual (MapModifiers.Protected, f.Modifiers5, "#C9");
			Assert.IsNull (f.Names, "#C10");
			Assert.IsNull (f.Street, "#C11");

			try {
				f = (Field_Encoded) DeserializeEncoded (typeof (Field_Encoded),
					string.Format (CultureInfo.InvariantCulture, "<?xml version='1.0' encoding='utf-16'?>" +
					"<q1:field xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' flag2='444' flag3='555' flag4='' modifiers='666' modifiers2='777' modifiers4='888' modifiers5='999' xmlns:q1='{2}' />",
					XmlSchemaNamespace, XmlSchemaInstanceNamespace, ANamespace));
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				// There was an error generating the XML document
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.Message, "#D3");
				Assert.IsNotNull (ex.InnerException, "#D4");

				// '444' is not a valid value for MonoTests.System.Xml.TestClasses.FlagEnum_Encoded
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#D5");
				Assert.IsNotNull (ex.InnerException.Message, "#D6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'444'") != -1, "#D7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (FlagEnum_Encoded).FullName) != -1, "#D8");
				Assert.IsNull (ex.InnerException.InnerException, "#D9");
			}
		}

		[Test]
		public void TestDeserialize_FlagEnum ()
		{
			FlagEnum e;

			e = (FlagEnum) Deserialize (typeof (FlagEnum), "<FlagEnum />");
			Assert.AreEqual (0, (int) e, "#A1");

			e = (FlagEnum) Deserialize (typeof (FlagEnum), "<FlagEnum>one</FlagEnum>");
			Assert.AreEqual (FlagEnum.e1, e, "#A2");

			e = (FlagEnum) Deserialize (typeof (FlagEnum), "<FlagEnum>one\u200atwo</FlagEnum>");
			Assert.AreEqual (FlagEnum.e1 | FlagEnum.e2, e, "#A3");

			e = (FlagEnum) Deserialize (typeof (FlagEnum), "<FlagEnum>one two four</FlagEnum>");
			Assert.AreEqual (FlagEnum.e1 | FlagEnum.e2 | FlagEnum.e4, e, "#A4");

			e = (FlagEnum) Deserialize (typeof (FlagEnum), "<FlagEnum> two  four </FlagEnum>");
			Assert.AreEqual (FlagEnum.e2 | FlagEnum.e4, e, "#A5");

			e = (FlagEnum) Deserialize (typeof (FlagEnum), "<FlagEnum>two four two</FlagEnum>");
			Assert.AreEqual (FlagEnum.e2 | FlagEnum.e4, e, "#A6");

			e = (FlagEnum) Deserialize (typeof (FlagEnum), "<FlagEnum>two four two\tone\u2002four\u200btwo one</FlagEnum>");
			Assert.AreEqual (FlagEnum.e1 | FlagEnum.e2 | FlagEnum.e4, e, "#A7");

			e = (FlagEnum) Deserialize (typeof (FlagEnum), "<FlagEnum></FlagEnum>");
			Assert.AreEqual (0, (int) e, "#A8");

			e = (FlagEnum) Deserialize (typeof (FlagEnum), "<FlagEnum> </FlagEnum>");
			Assert.AreEqual (0, (int) e, "#A9");

			try {
				Deserialize (typeof (FlagEnum), "<FlagEnum>1</FlagEnum>");
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.InnerException, "#B3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#B4");
				Assert.IsNotNull (ex.InnerException.Message, "#B5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'1'") != -1, "#B6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (FlagEnum).FullName) != -1, "#B7");
			}

			try {
				Deserialize (typeof (FlagEnum), "<FlagEnum>one,two</FlagEnum>");
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.InnerException, "#C3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#C4");
				Assert.IsNotNull (ex.InnerException.Message, "#C5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'one,two'") != -1, "#C6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (FlagEnum).FullName) != -1, "#C7");
			}

			try {
				Deserialize (typeof (FlagEnum), "<FlagEnum>one something</FlagEnum>");
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.InnerException, "#D3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#D4");
				Assert.IsNotNull (ex.InnerException.Message, "#D5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'something'") != -1, "#D6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (FlagEnum).FullName) != -1, "#D7");
			}
		}

		[Test]
		public void TestDeserialize_Group ()
		{
			string xml = string.Format (CultureInfo.InvariantCulture,
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
				"http://www.w3.org/2001/XMLSchema", "http://www.w3.org/2001/XMLSchema-instance");

			XmlTextReader xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.ReadStartElement ("Wrapper");

			Group group = (Group) DeserializeEncoded (typeof (Group), xtr);

			Assert.AreEqual (new DateTime (2002, 5, 2), group.Today, "#A1");
			Assert.AreEqual (".NET", group.GroupName, "#A2");
			Assert.AreEqual (new byte [] { 0x64, 0x32 }, group.GroupNumber, "#A3");
			Assert.AreEqual (GroupType.A, group.Grouptype, "#A4");
			Assert.AreEqual ("10000", group.PostitiveInt, "#A5");
			Assert.IsFalse (group.IgnoreThis, "#A6");
			Assert.IsNotNull (group.MyVehicle, "#A7");
			Assert.AreEqual (typeof (Car), group.MyVehicle.GetType (), "#A8");
			Assert.AreEqual ("1234566", group.MyVehicle.licenseNumber, "#A9");
			Assert.AreEqual (new DateTime (1, 1, 1), group.MyVehicle.makeDate, "#A10");
			Assert.IsNull (group.MyVehicle.weight, "#A11");

			xml = string.Format (CultureInfo.InvariantCulture,
				"<Wrapper>" +
				"<Group xmlns:xsd='{0}' xmlns:xsi='{1}' CreationDate='2002-05-02' GroupNumber='ZDI=' id='id1'>" +
				"<PosInt xsi:type='xsd:nonNegativeInteger'>10000</PosInt>" +
				"<Grouptype xsi:type='GroupType'>Large</Grouptype>" +
				"<MyVehicle href='#id2' />" +
				"</Group>" +
				"<Car xmlns:d2p1='{1}' id='id2' d2p1:type='Car'>" +
				"<weight xmlns:q2='{0}' d2p1:type='q2:string'>450</weight>" +
				"</Car>" +
				"</Wrapper>",
				"http://www.w3.org/2001/XMLSchema", "http://www.w3.org/2001/XMLSchema-instance");

			xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.ReadStartElement ("Wrapper");

			group = (Group) DeserializeEncoded (typeof (Group), xtr);

			Assert.AreEqual (new DateTime (2002, 5, 2), group.Today, "#B1");
			Assert.IsNull (group.GroupName, "#B2");
			Assert.AreEqual (new byte [] { 0x64, 0x32 }, group.GroupNumber, "#B3");
			Assert.AreEqual (GroupType.B, group.Grouptype, "#B4");
			Assert.AreEqual ("10000", group.PostitiveInt, "#B5");
			Assert.IsFalse (group.IgnoreThis, "#B6");
			Assert.IsNotNull (group.MyVehicle, "#B7");
			Assert.AreEqual (typeof (Car), group.MyVehicle.GetType (), "#B8");
			Assert.IsNull (group.MyVehicle.licenseNumber, "#B9");
			Assert.AreEqual (DateTime.MinValue, group.MyVehicle.makeDate, "#B10");
			Assert.AreEqual ("450", group.MyVehicle.weight, "#B11");

			xml = string.Format (CultureInfo.InvariantCulture,
				"<Wrapper>" +
				"<Group xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' />" +
				"</Wrapper>",
				"http://www.w3.org/2001/XMLSchema", "http://www.w3.org/2001/XMLSchema-instance");

			xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.ReadStartElement ("Wrapper");

			group = (Group) DeserializeEncoded (typeof (Group), xtr);

			Assert.AreEqual (DateTime.MinValue, group.Today, "#C1");
			Assert.IsNull (group.GroupName, "#C2");
			Assert.AreEqual (null, group.GroupNumber, "#C3");
			Assert.AreEqual (GroupType.A, group.Grouptype, "#C4");
			Assert.IsNull (group.PostitiveInt, "#C5");
			Assert.IsFalse (group.IgnoreThis, "#C6");
			Assert.IsNull (group.MyVehicle, "#C7");

			xml = string.Format (CultureInfo.InvariantCulture,
				"<Wrapper>" +
				"<Group xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1'>" +
				"<Grouptype xsi:type='GroupType'>666</Grouptype>" +
				"</Group>" +
				"</Wrapper>",
				"http://www.w3.org/2001/XMLSchema", "http://www.w3.org/2001/XMLSchema-instance");

			xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.ReadStartElement ("Wrapper");

			try {
				group = (Group) DeserializeEncoded (typeof (Group), xtr);
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				// There is an error in XML document (1, 174)
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.Message, "#D3");
				Assert.IsNotNull (ex.InnerException, "#D4");

				// '666' is not a valid value for GroupType
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#D5");
				Assert.IsNotNull (ex.InnerException.Message, "#D6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'666'") != -1, "#D7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (GroupType).Name) != -1, "#D8");
				Assert.IsNull (ex.InnerException.InnerException, "#D9");
			}

			xml = string.Format (CultureInfo.InvariantCulture,
				"<Wrapper>" +
				"<Group xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1'>" +
				"<Grouptype xsi:type='GroupType'>Garbage</Grouptype>" +
				"</Group>" +
				"</Wrapper>",
				"http://www.w3.org/2001/XMLSchema", "http://www.w3.org/2001/XMLSchema-instance");

			xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.ReadStartElement ("Wrapper");

			try {
				group = (Group) DeserializeEncoded (typeof (Group), xtr);
				Assert.Fail ("#E1");
			} catch (InvalidOperationException ex) {
				// There is an error in XML document (1, 178)
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
				Assert.IsNotNull (ex.Message, "#E3");
				Assert.IsNotNull (ex.InnerException, "#E4");

				// 'Garbage' is not a valid value for GroupType
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#E5");
				Assert.IsNotNull (ex.InnerException.Message, "#E6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'Garbage'") != -1, "#E7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (GroupType).Name) != -1, "#E8");
				Assert.IsNull (ex.InnerException.InnerException, "#E9");
			}

			xml = string.Format (CultureInfo.InvariantCulture,
				"<Wrapper>" +
				"<Group xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1'>" +
				"<Grouptype xsi:type='GroupType'>{2}</Grouptype>" +
				"</Group>" +
				"</Wrapper>",
				"http://www.w3.org/2001/XMLSchema", "http://www.w3.org/2001/XMLSchema-instance",
				((int) GroupType.B).ToString (CultureInfo.InvariantCulture));

			xtr = new XmlTextReader (xml, XmlNodeType.Document, null);
			xtr.ReadStartElement ("Wrapper");

			try {
				group = (Group) DeserializeEncoded (typeof (Group), xtr);
				Assert.Fail ("#F1");
			} catch (InvalidOperationException ex) {
				// There is an error in XML document (1, 172)
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#F2");
				Assert.IsNotNull (ex.Message, "#F3");
				Assert.IsNotNull (ex.InnerException, "#F4");

				// '1' is not a valid value for GroupType
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#F5");
				Assert.IsNotNull (ex.InnerException.Message, "#F6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'1'") != -1, "#F7");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (GroupType).Name) != -1, "#F8");
				Assert.IsNull (ex.InnerException.InnerException, "#F9");
			}
		}

		[Test]
		public void TestDeserialize_ZeroFlagEnum ()
		{
			ZeroFlagEnum e;

			e = (ZeroFlagEnum) Deserialize (typeof (ZeroFlagEnum), "<ZeroFlagEnum />");
			Assert.AreEqual (ZeroFlagEnum.e0, e, "#A1");
			e = (ZeroFlagEnum) Deserialize (typeof (ZeroFlagEnum), "<ZeroFlagEnum></ZeroFlagEnum>");
			Assert.AreEqual (ZeroFlagEnum.e0, e, "#A2");

			try {
				Deserialize (typeof (ZeroFlagEnum), "<ZeroFlagEnum>four</ZeroFlagEnum>");
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.InnerException, "#B3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#B4");
				Assert.IsNotNull (ex.InnerException.Message, "#B5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'four'") != -1, "#B6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (ZeroFlagEnum).FullName) != -1, "#B7");
			}

			try {
				Deserialize (typeof (ZeroFlagEnum), "<ZeroFlagEnum> o&lt;n&gt;e  four </ZeroFlagEnum>");
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.InnerException, "#C3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#C4");
				Assert.IsNotNull (ex.InnerException.Message, "#C5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'four'") != -1, "#C6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (ZeroFlagEnum).FullName) != -1, "#C7");
			}

			try {
				Deserialize (typeof (ZeroFlagEnum), "<ZeroFlagEnum>four o&lt;n&gt;e</ZeroFlagEnum>");
				Assert.Fail ("#D1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.InnerException, "#D3");
				Assert.AreEqual (typeof (InvalidOperationException), ex.InnerException.GetType (), "#D4");
				Assert.IsNotNull (ex.InnerException.Message, "#D5");
				Assert.IsTrue (ex.InnerException.Message.IndexOf ("'four'") != -1, "#D6");
				Assert.IsTrue (ex.InnerException.Message.IndexOf (typeof (ZeroFlagEnum).FullName) != -1, "#D7");
			}
		}

		[Test]
		public void TestDeserialize_PrimitiveTypesContainer ()
		{
			Deserialize (typeof (PrimitiveTypesContainer), string.Format (CultureInfo.InvariantCulture, 
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<PrimitiveTypesContainer xmlns:xsd='{0}' xmlns:xsi='{1}' xmlns='{2}'>" +
				"<Number xsi:type='xsd:int'>2004</Number>" +
				"<Name xsi:type='xsd:string'>some name</Name>" +
				"<Index xsi:type='xsd:unsignedByte'>56</Index>" +
				"<Password xsi:type='xsd:base64Binary'>8w8=</Password>" +
				"<PathSeparatorCharacter xmlns:q1='{3}' xsi:type='q1:char'>47</PathSeparatorCharacter>" +
				"</PrimitiveTypesContainer>", XmlSchemaNamespace,
				XmlSchemaInstanceNamespace, ANamespace, WsdlTypesNamespace));
			Assert.AreEqual (typeof (PrimitiveTypesContainer), result.GetType (), "#A1");

			PrimitiveTypesContainer deserialized = (PrimitiveTypesContainer) result;
			Assert.AreEqual (2004, deserialized.Number, "#A2");
			Assert.AreEqual ("some name", deserialized.Name, "#A3");
			Assert.AreEqual ((byte) 56, deserialized.Index, "#A4");
			Assert.AreEqual (new byte[] { 243, 15 }, deserialized.Password, "#A5");
			Assert.AreEqual ('/', deserialized.PathSeparatorCharacter, "#A6");

			DeserializeEncoded (typeof (PrimitiveTypesContainer), string.Format (CultureInfo.InvariantCulture,
				"<?xml version='1.0' encoding='utf-16'?>" +
				"<q1:PrimitiveTypesContainer xmlns:xsd='{0}' xmlns:xsi='{1}' id='id1' xmlns:q1='{2}'>" +
				"<Number xsi:type='xsd:int'>2004</Number>" +
				"<Name xsi:type='xsd:string'>some name</Name>" +
				"<Index xsi:type='xsd:unsignedByte'>56</Index>" +
				"<Password xsi:type='xsd:base64Binary'>8w8=</Password>" +
				"<PathSeparatorCharacter xmlns:q1='{3}' xsi:type='q1:char'>47</PathSeparatorCharacter>" +
				"</q1:PrimitiveTypesContainer>", XmlSchemaNamespace,
				XmlSchemaInstanceNamespace, AnotherNamespace, WsdlTypesNamespace));
			Assert.AreEqual (typeof (PrimitiveTypesContainer), result.GetType (), "#B1");

			deserialized = (PrimitiveTypesContainer) result;
			Assert.AreEqual (2004, deserialized.Number, "#B2");
			Assert.AreEqual ("some name", deserialized.Name, "#B3");
			Assert.AreEqual ((byte) 56, deserialized.Index, "#B4");
			Assert.AreEqual (new byte[] { 243, 15 }, deserialized.Password, "#B5");
			Assert.AreEqual ('/', deserialized.PathSeparatorCharacter, "#B6");
		}
	}
}
