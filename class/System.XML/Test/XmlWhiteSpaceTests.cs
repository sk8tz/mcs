//
// System.Xml.XmlWhitespaceTests.cs
//
// Author:
//	Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Xml;

using NUnit.Framework;

namespace Ximian.Mono.Tests
{
	public class XmlWhitespaceTests : TestCase
	{
		XmlDocument document;
		XmlWhitespace whitespace;
		XmlWhitespace broken;
                XmlNode original;
                XmlNode deep;
                XmlNode shallow;
		
		public XmlWhitespaceTests ()
			: base ("Ximian.Mono.Tests.XmlWhitespaceTests testsuite")
		{
		}

		public XmlWhitespaceTests (string name)
			: base (name)
		{
		}

		protected override void SetUp ()
		{
			document = new XmlDocument ();
			document.LoadXml ("<root><foo></foo></root>");
			XmlElement element = document.CreateElement ("foo");
			whitespace = document.CreateWhitespace ("\r\n");
			element.AppendChild (whitespace);
		}

		internal void TestXmlNodeBaseProperties (XmlNode original, XmlNode cloned)
		{
//			assertequals (original.nodetype + " was incorrectly cloned.",
//				      original.baseuri, cloned.baseuri);			
			AssertNull (cloned.ParentNode);
			AssertEquals ("Value incorrectly cloned",
				       cloned.Value, original.Value);
			
                        Assert ("Copies, not pointers", !Object.ReferenceEquals (original,cloned));
		}

		public void TestXmlWhitespaceBadConstructor ()
		{
			try {
				broken = document.CreateWhitespace ("black");				
			} catch (Exception e) {
				AssertEquals ("Incorrect Exception thrown",
					      e.GetType (), Type.GetType ("System.ArgumentException"));
			}
		}

		public void TestXmlWhitespaceConstructor ()
		{
			AssertEquals ("whitespace char didn't get copied right",
				      "\r\n", whitespace.Data);
		}
		
	       
		public void TestXmlWhitespaceName ()
		{
			AssertEquals (whitespace.NodeType + " Name property broken",
				      whitespace.Name, "#whitespace");
		}

		public void TestXmlWhitespaceLocalName ()
		{
			AssertEquals (whitespace.NodeType + " LocalName property broken",
				      whitespace.LocalName, "#whitespace");
		}

		public void TestXmlWhitespaceNodeType ()
		{
			AssertEquals ("XmlWhitespace NodeType property broken",
				      whitespace.NodeType.ToString (), "Whitespace");
		}

		public void TestXmlWhitespaceIsReadOnly ()
		{
			AssertEquals ("XmlWhitespace IsReadOnly property broken",
				      whitespace.IsReadOnly, false);
		}

		public void TestXmlWhitespaceCloneNode ()
		{
			original = whitespace;

			shallow = whitespace.CloneNode (false); // shallow
			TestXmlNodeBaseProperties (original, shallow);
						
			deep = whitespace.CloneNode (true); // deep
			TestXmlNodeBaseProperties (original, deep);
			

                        AssertEquals ("deep cloning differs from shallow cloning",
				      deep.OuterXml, shallow.OuterXml);
		}
	}
}
