// XmlAttributeCollectionTests.cs : Tests for the XmlAttributeCollection class
//
// Author: Matt Hunter <xrkune@tconl.com>
//
// <c> 2002 Matt Hunter

using System;
using System.Xml;
using System.Text;
using System.IO;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XmlAttributeCollectionTests : TestCase
	{
		public XmlAttributeCollectionTests() : base("MonoTests.System.Xml.XmlAttributeCollectionTests testsuite") { }
		public XmlAttributeCollectionTests(string name) : base(name) { }

		private XmlDocument document;

		protected override void SetUp()
		{
			document = new XmlDocument ();
		}
		public void TestRemoveAll ()
		{
			StringBuilder xml = new StringBuilder ();
			xml.Append ("<?xml version=\"1.0\" ?><library><book type=\"non-fiction\" price=\"34.95\"> ");
			xml.Append ("<title type=\"intro\">XML Fun</title> " );
			xml.Append ("<author>John Doe</author></book></library>");

			MemoryStream memoryStream = new MemoryStream (Encoding.UTF8.GetBytes (xml.ToString ()));
			document = new XmlDocument ();
			document.Load (memoryStream);
			XmlNodeList bookList = document.GetElementsByTagName ("book");
			XmlNode xmlNode = bookList.Item (0);
			XmlElement xmlElement = xmlNode as XmlElement;
			XmlAttributeCollection attributes = xmlElement.Attributes;
			attributes.RemoveAll ();
			AssertEquals ("not all attributes removed.", false, xmlElement.HasAttribute ("type"));
		}

	}
}
