//
// System.Xml.XmlDeclarationTests.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
// Author: Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Ximian, Inc.
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlDeclarationTests
	{
		XmlDocument document;
		XmlDeclaration declaration;
		
		[SetUp]
		public void GetReady ()
		{
			document = new XmlDocument ();
			document.LoadXml ("<foo><bar></bar></foo>");
			declaration = document.CreateXmlDeclaration ("1.0", null, null);
		}

		[Test]
		public void InnerAndOuterXml ()
		{
			declaration = document.CreateXmlDeclaration ("1.0", null, null);
			Assertion.AssertEquals (String.Empty, declaration.InnerXml);
			Assertion.AssertEquals ("<?xml version=\"1.0\"?>", declaration.OuterXml);

			declaration = document.CreateXmlDeclaration ("1.0", "doesn't check", null);
			Assertion.AssertEquals (String.Empty, declaration.InnerXml);
			Assertion.AssertEquals ("<?xml version=\"1.0\" encoding=\"doesn't check\"?>", declaration.OuterXml);

			declaration = document.CreateXmlDeclaration ("1.0", null, "yes");
			Assertion.AssertEquals (String.Empty, declaration.InnerXml);
			Assertion.AssertEquals ("<?xml version=\"1.0\" standalone=\"yes\"?>", declaration.OuterXml);

			declaration = document.CreateXmlDeclaration ("1.0", "foo", "no");
			Assertion.AssertEquals (String.Empty, declaration.InnerXml);
			Assertion.AssertEquals ("<?xml version=\"1.0\" encoding=\"foo\" standalone=\"no\"?>", declaration.OuterXml);
		}

		internal void XmlNodeBaseProperties (XmlNode original, XmlNode cloned)
		{
//			assertequals (original.nodetype + " was incorrectly cloned.",
//				      original.baseuri, cloned.baseuri);			
			Assertion.AssertNull (cloned.ParentNode);

			Assertion.AssertEquals ("Value incorrectly cloned",
				      original.Value, cloned.Value);
			
                        Assertion.Assert ("Copies, not pointers", !Object.ReferenceEquals (original,cloned));
		}

		[Test]
		public void Constructor ()
		{
			try {
				XmlDeclaration broken = document.CreateXmlDeclaration ("2.0", null, null);
			} catch (ArgumentException) {
				return;

			} catch (Exception e) {
				Assertion.Fail("first arg null, wrong exception: " + e.ToString());
			}
		}

		[Test]
		public void NodeType ()
		{
			Assertion.AssertEquals ("incorrect NodeType returned", XmlNodeType.XmlDeclaration, declaration.NodeType);
		}

		[Test]
		public void Names ()
		{
			Assertion.AssertEquals ("Name is incorrect", "xml", declaration.Name);
			Assertion.AssertEquals ("LocalName is incorrect", "xml", declaration.LocalName);
		}

		[Test]
		public void EncodingProperty ()
		{
			XmlDeclaration d1 = document.CreateXmlDeclaration ("1.0", "foo", null);
			Assertion.AssertEquals ("Encoding property", "foo", d1.Encoding);

			XmlDeclaration d2 = document.CreateXmlDeclaration ("1.0", null, null);
			Assertion.AssertEquals ("null Encoding property", String.Empty, d2.Encoding);
		}

		[Test]
		public void StandaloneProperty ()
		{
			XmlDeclaration d1 = document.CreateXmlDeclaration ("1.0", null, "yes");
			Assertion.AssertEquals ("Yes standalone property", "yes", d1.Standalone);

			XmlDeclaration d2 = document.CreateXmlDeclaration ("1.0", null, "no");
			Assertion.AssertEquals ("No standalone property", "no", d2.Standalone);

			XmlDeclaration d3 = document.CreateXmlDeclaration ("1.0", null, null);
			Assertion.AssertEquals ("null Standalone property", String.Empty, d3.Standalone);
		}

		[Test]
		public void ValueProperty ()
		{
			string expected = "version=\"1.0\" encoding=\"ISO-8859-1\" standalone=\"yes\"" ;

			XmlDeclaration d = document.CreateXmlDeclaration ("1.0", "ISO-8859-1", "yes");
			Assertion.AssertEquals ("Value property", expected, d.Value);

			d.Value = expected;
			Assertion.AssertEquals ("Value round-trip", expected, d.Value);

			d.Value = "   " + expected;
			Assertion.AssertEquals ("Value round-trip (padded)", expected, d.Value);

			d.Value = "version=\"1.0\"     encoding=\"ISO-8859-1\" standalone=\"yes\"" ;
			Assertion.AssertEquals ("Value round-trip (padded 2)", expected, d.Value);

			d.Value = "version=\"1.0\"\tencoding=\"ISO-8859-1\" standalone=\"yes\"" ;
			Assertion.AssertEquals ("Value round-trip (\\t)", expected, d.Value);

			d.Value = "version=\"1.0\"\n    encoding=\"ISO-8859-1\" standalone=\"yes\"" ;
			Assertion.AssertEquals ("Value round-trip (\\n)", expected, d.Value);

			d.Value = "version=\"1.0\"    encoding	=   \"ISO-8859-1\" standalone = \"yes\"" ;
			Assertion.AssertEquals ("Value round-trip (spaces)", expected, d.Value);

			d.Value = "version='1.0' encoding='ISO-8859-1' standalone='yes'" ;
			Assertion.AssertEquals ("Value round-trip ('s)", expected, d.Value);

		}

		[Test]
		public void XmlCommentCloneNode ()
		{
			XmlNode original = declaration;

			XmlNode shallow = declaration.CloneNode (false); // shallow
			XmlNodeBaseProperties (original, shallow);
			
			XmlNode deep = declaration.CloneNode (true); // deep
			XmlNodeBaseProperties (original, deep);

                        Assertion.AssertEquals ("deep cloning differs from shallow cloning",
				      deep.OuterXml, shallow.OuterXml);
		}
	}
}
