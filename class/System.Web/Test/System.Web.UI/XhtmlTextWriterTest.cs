//
// XhtmlTextWriterTest.cs
//	- Unit tests for System.Web.UI.XhtmlTextWriter
//
// Author:
//	Cesar Lopez Nataren <cnataren@novell.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

using System;
using System.IO;
using System.Web.UI;
using NUnit.Framework;
using System.Collections;

namespace MonoTests.System.Web.UI {

	public class XhtmlTextWriterTester : XhtmlTextWriter
	{
		public XhtmlTextWriterTester (TextWriter writer)
			: this (writer, HtmlTextWriter.DefaultTabString)
		{
		}

		public XhtmlTextWriterTester (TextWriter writer, string tabString)
			: base (writer, tabString)
		{
		}

		public Hashtable PublicElementSpecificAttributes {
			get { return ElementSpecificAttributes; }
		}

		public bool PublicOnStyleAttributeRender (string name, string value, HtmlTextWriterStyle style)
		{
			return OnStyleAttributeRender (name, value, style);
		}

		public bool PublicOnAttributeRender (string name, string value, HtmlTextWriterAttribute attr)
		{
			return OnAttributeRender (name, value, attr);
		}

		public string PublicGetAttributeName (HtmlTextWriterAttribute attrKey)
		{
			return GetAttributeName (attrKey);
		}

		public string PublicGetStyleName (HtmlTextWriterStyle styleKey)
		{
			return GetStyleName (styleKey);
		}
	}

	[TestFixture]
	public class XhtmlTextWriterTest {

		XhtmlTextWriterTester xhtml;
		StringWriter writer;

		// attributes
		string absent_attr = "absent-attr";
		string a_attr = "accesskey";

		// elements
		string elem_name = "a";
		string absent_elem = "absent-elem";

		Hashtable attrs;

		[SetUp]
		public void SetupTests ()
		{
			writer = new StringWriter ();
			xhtml = new XhtmlTextWriterTester (writer);
			attrs = (Hashtable) xhtml.PublicElementSpecificAttributes;
		}

		[Test]
		public void AddRecognizedAttributeTest ()
		{
			Hashtable elem_attrs = (Hashtable) attrs [elem_name];

			// absent attr
			Assertion.AssertEquals ("#A01", null, elem_attrs [absent_attr]);

			// recently added attr
			xhtml.AddRecognizedAttribute (elem_name, absent_attr);
			Assertion.AssertEquals ("A02", true, elem_attrs [absent_attr]);

			// ensure there's no absent_elem
			Assertion.AssertEquals ("#A03", null, attrs [absent_elem]);

			// Given absent_elem and absent_attr, we must add the element 
			// and bind the given attr to it
			xhtml.AddRecognizedAttribute (absent_elem, absent_attr);
			Assertion.AssertEquals ("#A04", true, ((Hashtable) attrs [absent_elem]) [absent_attr]);

			// Given a known element and attribute
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddRecognizedAttributeTest2 ()
		{
			// if attr is already there we must throw ArgumentException
			xhtml.AddRecognizedAttribute (elem_name, a_attr);
		}

		[Test]
		[Ignore ("NUNIT 2.4 issue - temporarily disabled")]
		public void RemoveRecognizedAttribute ()
		{
			// ensure we add it
			xhtml.AddRecognizedAttribute (elem_name, absent_attr);
			Assertion.AssertEquals ("#B01", true, ((Hashtable) attrs [elem_name]) [absent_attr]);

			// ensure we remove it
			xhtml.RemoveRecognizedAttribute (elem_name, absent_attr);
			Assertion.AssertEquals ("#B02", null, ((Hashtable) attrs [elem_name]) [absent_attr]);

			// if the element does not exist we must resume cleanly
			xhtml.RemoveRecognizedAttribute (absent_elem, absent_attr);

			// if the attr does not exist we must resume cleanly
			xhtml.RemoveRecognizedAttribute (elem_name, a_attr);
		}

		[Test]
		public void OnStyleAttributeRenderTest ()
		{
			int i = 0;

			foreach (HtmlTextWriterStyle style in Enum.GetValues (typeof (HtmlTextWriterStyle)))
				Assertion.AssertEquals ("#C0" + i++, false,
						xhtml.PublicOnStyleAttributeRender (xhtml.PublicGetStyleName (style), 
									    "foo", style));
		}

		[Test]
		public void WriteBreakTest ()
		{
			xhtml.WriteBreak ();
			Assertion.AssertEquals ("#D01", "<br/>", writer.ToString ());
		}

		[Test]
		public void OnAttributeRenderTest ()
		{
			int i = 0;
			Array attrs = Enum.GetValues (typeof (HtmlTextWriterAttribute));

			foreach (HtmlTextWriterAttribute attr in attrs) {
				try {
					xhtml.PublicOnAttributeRender (xhtml.PublicGetAttributeName (attr), "foo", attr);
				} catch (ArgumentNullException e) {
					i++;
				}
			}
			Assertion.AssertEquals ("#F01", attrs.Length, i);
		}
	}
}

#endif
