using System;
using System.Xml;

using NUnit.Framework;

namespace Ximian.Mono.Tests
{
	public class XmlDocumentTests : TestCase
	{
		public XmlDocumentTests () : base ("Ximian.Mono.Tests.XmlDocumentTests testsuite") {}
		public XmlDocumentTests (string name) : base (name) {}

		private XmlDocument document;

		protected override void SetUp ()
		{
			document = new XmlDocument ();
		}

		public void TestDocumentElement ()
		{
			AssertNull (document.DocumentElement);
			XmlElement element = document.CreateElement ("foo", "bar", "http://foo/");
			AssertNotNull (element);

			AssertEquals ("foo", element.Prefix);
			AssertEquals ("bar", element.LocalName);
			AssertEquals ("http://foo/", element.NamespaceURI);

			AssertEquals ("foo:bar", element.Name);

			AssertSame (element, document.AppendChild (element));

			AssertSame (element, document.DocumentElement);
		}

		public void TestDocumentEmpty()
		{
			AssertEquals ("Incorrect output for empty document.", "", document.OuterXml);
		}

		public void TestLoadXmlCDATA ()
		{
			document.LoadXml ("<foo><![CDATA[bar]]></foo>");
			Assert (document.DocumentElement.FirstChild.NodeType == XmlNodeType.CDATA);
			AssertEquals ("bar", document.DocumentElement.FirstChild.Value);
		}

		public void TestLoadXMLComment()
		{
// XmlTextReader needs to throw this exception
//			try {
//				document.LoadXml("<!--foo-->");
//				Fail("XmlException should have been thrown.");
//			}
//			catch (XmlException e) {
//				AssertEquals("Exception message doesn't match.", "The root element is missing.", e.Message);
//			}

			document.LoadXml ("<foo><!--Comment--></foo>");
			Assert (document.DocumentElement.FirstChild.NodeType == XmlNodeType.Comment);
			AssertEquals ("Comment", document.DocumentElement.FirstChild.Value);

			document.LoadXml (@"<foo><!--bar--></foo>");
			AssertEquals ("Incorrect target.", "bar", ((XmlComment)document.FirstChild.FirstChild).Data);
		}

		public void TestLoadXmlElementSingle ()
		{
			AssertNull (document.DocumentElement);
			document.LoadXml ("<foo/>");

			AssertNotNull (document.DocumentElement);
			AssertSame (document.FirstChild, document.DocumentElement);

			AssertEquals (String.Empty, document.DocumentElement.Prefix);
			AssertEquals ("foo", document.DocumentElement.LocalName);
			AssertEquals (String.Empty, document.DocumentElement.NamespaceURI);
			AssertEquals ("foo", document.DocumentElement.Name);
		}

		public void TestLoadXmlElementWithAttributes ()
		{
			AssertNull (document.DocumentElement);
			document.LoadXml ("<foo bar='baz' quux='quuux'/>");

			XmlElement documentElement = document.DocumentElement;

			AssertEquals ("baz", documentElement.GetAttribute ("bar"));
			AssertEquals ("quuux", documentElement.GetAttribute ("quux"));
		}
		public void TestLoadXmlElementWithChildElement ()
		{
			document.LoadXml ("<foo><bar/></foo>");
			Assert (document.ChildNodes.Count == 1);
			Assert (document.FirstChild.ChildNodes.Count == 1);
			AssertEquals ("foo", document.DocumentElement.LocalName);
			AssertEquals ("bar", document.DocumentElement.FirstChild.LocalName);
		}

		public void TestLoadXmlElementWithTextNode ()
		{
			document.LoadXml ("<foo>bar</foo>");
			Assert (document.DocumentElement.FirstChild.NodeType == XmlNodeType.Text);
			AssertEquals ("bar", document.DocumentElement.FirstChild.Value);
		}

		public void TestLoadXmlExceptionClearsDocument ()
		{
			document.LoadXml ("<foo/>");
			Assert (document.FirstChild != null);
			
			try {
				document.LoadXml ("<123/>");
				Fail ("An XmlException should have been thrown.");
			} catch (XmlException) {}

			Assert (document.FirstChild == null);
		}

		public void TestLoadXmlProcessingInstruction ()
		{
			document.LoadXml (@"<?foo bar='baaz' quux='quuux'?><quuuux></quuuux>");
			AssertEquals ("Incorrect target.", "foo", ((XmlProcessingInstruction)document.FirstChild).Target);
			AssertEquals ("Incorrect data.", "bar='baaz' quux='quuux'", ((XmlProcessingInstruction)document.FirstChild).Data);
		}

		public void TestOuterXml ()
		{
			string xml;
			
			xml = "<root><![CDATA[foo]]></root>";
			document.LoadXml (xml);
			AssertEquals("XmlDocument with cdata OuterXml is incorrect.", xml, document.OuterXml);

			xml = "<root><!--foo--></root>";
			document.LoadXml (xml);
			AssertEquals("XmlDocument with comment OuterXml is incorrect.", xml, document.OuterXml);

			xml = "<root><?foo bar?></root>";
			document.LoadXml (xml);
			AssertEquals("XmlDocument with processing instruction OuterXml is incorrect.", xml, document.OuterXml);
		}

		public void TestParentNodes ()
		{
			document.LoadXml ("<foo><bar><baz/></bar></foo>");
			XmlNode node = document.FirstChild.FirstChild.FirstChild;
			AssertEquals ("Wrong child found.", "baz", node.LocalName);
			AssertEquals ("Wrong parent.", "bar", node.ParentNode.LocalName);
			AssertEquals ("Wrong parent.", "foo", node.ParentNode.ParentNode.LocalName);
			AssertEquals ("Wrong parent.", "#document", node.ParentNode.ParentNode.ParentNode.LocalName);
			AssertNull ("Expected parent to be null.", node.ParentNode.ParentNode.ParentNode.ParentNode);
		}

		public void TestRemovedElementNextSibling ()
		{
			XmlNode node;
			XmlNode nextSibling;

			document.LoadXml ("<foo><child1/><child2/></foo>");
			node = document.DocumentElement.FirstChild;
			document.DocumentElement.RemoveChild (node);
			nextSibling = node.NextSibling;
			AssertNull ("Expected removed node's next sibling to be null.", nextSibling);
		}
	}
}
