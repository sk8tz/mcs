//
// XPathEditableNavigator.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C)2003 Atsushi Enomoto
//

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
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Security.Policy;
using System.Xml.Schema;
using System.Xml.XPath;
//using Mono.Xml.XPath2;
//using MS.Internal.Xml;

namespace System.Xml.XPath
{
	public abstract class XPathEditableNavigator 
		: XPathNavigator, IXPathEditable
	{
		protected XPathEditableNavigator ()
		{
		}

		[MonoTODO ("No implementation as yet")]
		public abstract XmlWriter AppendChild ();

		[MonoTODO]
		public virtual XPathEditableNavigator AppendChild (
			string xmlFragments)
		{
			// FIXME: should XmlParserContext be something?
			return AppendChild (new XmlTextReader (xmlFragments, XmlNodeType.Element, null));
		}

		[MonoTODO]
		public virtual XPathEditableNavigator AppendChild (
			XmlReader reader)
		{
			XmlWriter w = AppendChild ();
			w.WriteNode (reader, false);
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XPathEditableNavigator AppendChild (
			XPathNavigator nav)
		{
			throw new NotImplementedException ();
//			AppendChild (new XPathNavigatorReader (nav));
		}

		public void AppendChildElement (string prefix, string name, string ns, string value)
		{
			XmlWriter xw = AppendChild ();
			xw.WriteStartElement (prefix, name, ns);
			xw.WriteString (value);
			xw.WriteEndElement ();
			xw.Close ();
		}

		[MonoTODO]
		protected void BuildSubTree (XmlReader reader, XmlWriter writer, bool useValidity)
		{
			throw new NotImplementedException ();
		}

		public virtual void CreateAttribute (string prefix, string localName, string namespaceURI, string value)
		{
			using (XmlWriter w = CreateAttributes ()) {
				w.WriteAttributeString (prefix, localName, namespaceURI, value);
			}
		}

		[MonoTODO ("No implementation as yet")]
		public abstract XmlWriter CreateAttributes ();

		public virtual XPathEditableNavigator CreateEditor ()
		{
			return (XPathEditableNavigator) Clone ();
		}

		// LAMESPEC: documented as public abstract, but it conflicts
		// with XPathNavigator.CreateNavigator ().
		[MonoTODO]
		public override XPathNavigator CreateNavigator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("No implementation as yet")]
		public abstract bool DeleteCurrent ();

		public virtual XmlWriter InsertAfter ()
		{
			XPathEditableNavigator nav = (XPathEditableNavigator) Clone ();
			if (nav.MoveToNext ())
				return nav.InsertBefore ();
			else
				return AppendChild ();
		}

		public virtual XPathEditableNavigator InsertAfter (string xmlFragments)
		{
			return InsertAfter (new XmlTextReader (xmlFragments, XmlNodeType.Element, null));
		}

		[MonoTODO]
		public virtual XPathEditableNavigator InsertAfter (XmlReader reader)
		{
			using (XmlWriter w = InsertAfter ()) {
				w.WriteNode (reader, false);
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XPathEditableNavigator InsertAfter (XPathNavigator nav)
		{
//			InsertAfter (new XPathNavigatorReader (nav));
			throw new NotImplementedException ();
		}

		[MonoTODO ("No implementation as yet")]
		public abstract XmlWriter InsertBefore ();

		public virtual XPathEditableNavigator InsertBefore (string xmlFragments)
		{
			return InsertBefore (new XmlTextReader (xmlFragments, XmlNodeType.Element, null));
		}

		[MonoTODO]
		public virtual XPathEditableNavigator InsertBefore (XmlReader reader)
		{
			using (XmlWriter w = InsertBefore ()) {
				w.WriteNode (reader, false);
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XPathEditableNavigator InsertBefore (XPathNavigator nav)
		{
//			InsertBefore (new XPathNavigatorReader (nav));
			throw new NotImplementedException ();
		}

		public virtual void InsertElementAfter (string prefix, 
			string localName, string namespaceURI, string value)
		{
			using (XmlWriter w = InsertAfter ()) {
				w.WriteElementString (prefix, localName, namespaceURI, value);
			}
		}

		public virtual void InsertElementBefore (string prefix, 
			string localName, string namespaceURI, string value)
		{
			using (XmlWriter w = InsertBefore ()) {
				w.WriteElementString (prefix, localName, namespaceURI, value);
			}
		}

		public virtual XmlWriter PrependChild ()
		{
			XPathEditableNavigator nav = (XPathEditableNavigator) Clone ();
			if (nav.MoveToFirstChild ())
				return nav.InsertBefore ();
			else
				return InsertBefore ();
		}

		public virtual XPathEditableNavigator PrependChild (string xmlFragments)
		{
			return PrependChild (new XmlTextReader (xmlFragments, XmlNodeType.Element, null));
		}

		[MonoTODO]
		public virtual XPathEditableNavigator PrependChild (XmlReader reader)
		{
			using (XmlWriter w = PrependChild ()) {
				w.WriteNode (reader, false);
			}
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual XPathEditableNavigator PrependChild (XPathNavigator nav)
		{
//			PrependChild (new XPathNavigatorReader (nav));
			throw new NotImplementedException ();
		}

		public virtual void PrependChildElement (string prefix, 
			string localName, string namespaceURI, string value)
		{
			using (XmlWriter w = PrependChild ()) {
				w.WriteElementString (prefix, localName, namespaceURI, value);
			}
		}

		// Dunno the exact purpose, but maybe internal editor use
		[MonoTODO]
		public virtual void SetFromObject (object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("No implementation as yet")]
		public abstract void SetValue (object value);

		[MonoTODO]
		public virtual void Validate (XmlSchemaSet schemas, ValidationEventHandler handler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string InnerXml {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override string OuterXml {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}

#endif
