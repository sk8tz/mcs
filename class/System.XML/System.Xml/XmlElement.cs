//
// System.Xml.XmlElement
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//   Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
// (C) 2002 Atsushi Enomoto
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

using System;
using System.Collections;
using System.Xml.XPath;
using System.IO;
using System.Text;
using Mono.Xml;

namespace System.Xml
{
	public class XmlElement : XmlLinkedNode
	{
		#region Fields

		private XmlAttributeCollection attributes;
		private string localName;
		private string namespaceURI;
		private string prefix;
		private bool isNotEmpty;

		#endregion

		#region Constructor

		protected internal XmlElement (
			string prefix, 
			string localName, 
			string namespaceURI, 
			XmlDocument doc) : this (prefix, localName, namespaceURI, doc, false)
		{
		}

		internal XmlElement (
			string prefix, 
			string localName, 
			string namespaceURI, 
			XmlDocument doc,
			bool atomizedNames) : base (doc)
		{
			XmlConvert.VerifyName (localName);

			if (atomizedNames) {
				this.prefix = prefix;
				this.localName = localName;
				this.namespaceURI = namespaceURI;
			} else {
				this.prefix = doc.NameTable.Add (prefix);
				this.localName = doc.NameTable.Add (localName);
				this.namespaceURI = doc.NameTable.Add (namespaceURI);
			}

			attributes = new XmlAttributeCollection (this);

			if(doc.DocumentType != null)
			{
				DTDAttListDeclaration attlist = doc.DocumentType.DTD.AttListDecls [localName];
				if (attlist != null) {
					for (int i = 0; i < attlist.Definitions.Count; i++) {
						DTDAttributeDefinition def = attlist [i];
						if (def.DefaultValue != null) {
							SetAttribute (def.Name, def.DefaultValue);
							attributes [def.Name].SetDefault ();
						}
					}
				}
			}
		}

		#endregion

		#region Properties

		public override XmlAttributeCollection Attributes {
			get { return attributes; }
		}

		public virtual bool HasAttributes {
			get { return attributes.Count > 0; }
		}

		public override string InnerText {
			get {
				return base.InnerText;
			}
			set {
				// Why its behavior (of MS FCL) is different from InnerXml...?
				if (FirstChild != null && FirstChild.NodeType == XmlNodeType.Text)
					FirstChild.Value = value;
				else {
					if (FirstChild != null) {
						for (int i = 0; i < ChildNodes.Count; i++)
							this.RemoveChild (ChildNodes [i]);
					}
					// creates new Text node
					AppendChild (OwnerDocument.CreateTextNode (value));
				}
			}
		}

		public override string InnerXml {
			get {
				return base.InnerXml;
			}
			set {
				while (FirstChild != null)
					this.RemoveChild (FirstChild);

				XmlNameTable nt = this.OwnerDocument.NameTable;
				XmlNamespaceManager nsmgr = this.ConstructNamespaceManager ();
				XmlParserContext ctx = new XmlParserContext (OwnerDocument.NameTable, nsmgr,
					OwnerDocument.DocumentType != null ? OwnerDocument.DocumentType.DTD : null,
					BaseURI, XmlLang, XmlSpace, null);
				XmlTextReader xmlReader = new XmlTextReader (value, XmlNodeType.Element, ctx);
				xmlReader.XmlResolver = OwnerDocument.Resolver;

				do {
					XmlNode n = OwnerDocument.ReadNode (xmlReader);
					if(n == null) break;
					AppendChild (n);
				} while (true);
			}
		}

		public bool IsEmpty {
			get {
				return !isNotEmpty && (FirstChild == null);
			}

			set {
				isNotEmpty = !value;
				if(value) {
					while (FirstChild != null)
						RemoveChild (FirstChild);
				}
			}
		}

		public override string LocalName {
			get { return localName; }
		}

		public override string Name {
			get {
				if (prefix == String.Empty || prefix == null)
					return localName;
				else
					return prefix + ":" + localName;
			}
		}

		public override string NamespaceURI {
			get { return namespaceURI; }
		}

		// Why is this override?
		public override XmlNode NextSibling {
			get { 
				return base.NextSibling; 
			}
		}

		public override XmlNodeType NodeType {
			get { 
				return XmlNodeType.Element; 
			}
		}

		internal override XPathNodeType XPathNodeType {
			get {
				return XPathNodeType.Element;
			}
		}

		public override XmlDocument OwnerDocument {
			get { 
				return base.OwnerDocument; 
			}
		}

		public override string Prefix {
			get { return prefix; }
			set {
				if (IsReadOnly)
					throw new XmlException ("This node is readonly.");
				if (!XmlChar.IsNCName (value))
					throw new ArgumentException ("Specified name is not a valid NCName: " + value);

				prefix = OwnerDocument.NameTable.Add (value);
			}
		}

		#endregion

		#region Methods
		
		public override XmlNode CloneNode (bool deep)
		{
			XmlElement node = new XmlElement (
				prefix, localName, namespaceURI, OwnerDocument, true);

			for (int i = 0; i < Attributes.Count; i++)
				node.SetAttributeNode ((XmlAttribute) 
					Attributes [i].CloneNode (true));

			if (deep) {
				for (int i = 0; i < ChildNodes.Count; i++)
					node.AppendChild (ChildNodes [i].CloneNode (true));
			}

			if (IsReadOnly)
				node.SetReadOnly ();
			return node;
		}

		public virtual string GetAttribute (string name)
		{
			XmlNode attributeNode = Attributes.GetNamedItem (name);
			return attributeNode != null ? attributeNode.Value : String.Empty;
		}

		public virtual string GetAttribute (string localName, string namespaceURI)
		{
			XmlNode attributeNode = Attributes.GetNamedItem (localName, namespaceURI);
			return attributeNode != null ? attributeNode.Value : String.Empty;
		}

		public virtual XmlAttribute GetAttributeNode (string name)
		{
			XmlNode attributeNode = Attributes.GetNamedItem (name);
			return attributeNode != null ? attributeNode as XmlAttribute : null;
		}

		public virtual XmlAttribute GetAttributeNode (string localName, string namespaceURI)
		{
			XmlNode attributeNode = Attributes.GetNamedItem (localName, namespaceURI);
			return attributeNode != null ? attributeNode as XmlAttribute : null;
		}

		public virtual XmlNodeList GetElementsByTagName (string name)
		{
			ArrayList nodeArrayList = new ArrayList ();
			this.SearchDescendantElements (name, name == "*", nodeArrayList);
			return new XmlNodeArrayList (nodeArrayList);
		}

		public virtual XmlNodeList GetElementsByTagName (string localName, string namespaceURI)
		{
			ArrayList nodeArrayList = new ArrayList ();
			this.SearchDescendantElements (localName, localName == "*", namespaceURI, namespaceURI == "*", nodeArrayList);
			return new XmlNodeArrayList (nodeArrayList);
		}

		public virtual bool HasAttribute (string name)
		{
			XmlNode attributeNode = Attributes.GetNamedItem (name);
			return attributeNode != null;
		}

		public virtual bool HasAttribute (string localName, string namespaceURI)
		{
			XmlNode attributeNode = Attributes.GetNamedItem (localName, namespaceURI);
			return attributeNode != null;
		}

		public override void RemoveAll ()
		{
			// Remove all attributes and child nodes.
			base.RemoveAll ();
		}

		public virtual void RemoveAllAttributes ()
		{
			attributes.RemoveAll ();
		}

		public virtual void RemoveAttribute (string name)
		{
			XmlAttribute attr = attributes.GetNamedItem (name) as XmlAttribute;
			if (attr != null)
				attributes.Remove(attr);
		}

		public virtual void RemoveAttribute (string localName, string namespaceURI)
		{
			XmlAttribute attr = attributes.GetNamedItem(localName, namespaceURI) as XmlAttribute;
			if (attr != null)
				attributes.Remove(attr);
		}

		public virtual XmlNode RemoveAttributeAt (int i)
		{
			return attributes.RemoveAt (i);
		}

		public virtual XmlAttribute RemoveAttributeNode (XmlAttribute oldAttr)
		{
			return attributes.Remove(oldAttr);
		}

		public virtual XmlAttribute RemoveAttributeNode (string localName, string namespaceURI)
		{
			return attributes.Remove(attributes[localName, namespaceURI]);
		}

		public virtual void SetAttribute (string name, string value)
		{
			XmlAttribute attribute = OwnerDocument.CreateAttribute (name);
			attribute.Value = value;
			Attributes.SetNamedItem (attribute);
		}

		public virtual string SetAttribute (string localName, string namespaceURI, string value)
		{
			XmlAttribute attr = attributes [localName, namespaceURI];
			if (attr == null) {
				attr = OwnerDocument.CreateAttribute (localName, namespaceURI);
				attr.Value = value;
				attributes.SetNamedItem (attr);
			}
			else
				attr.Value = value;
			return attr.Value;
		}

		public virtual XmlAttribute SetAttributeNode (XmlAttribute newAttr)
		{
			if (newAttr.OwnerElement != null)
				throw new InvalidOperationException (
					"Specified attribute is already an attribute of another element.");

			XmlAttribute ret = Attributes.SetNamedItem (newAttr) as XmlAttribute;
			return ret == newAttr ? null : ret;
		}

		public virtual XmlAttribute SetAttributeNode (string localName, string namespaceURI)
		{
			// Note that this constraint is only for this method.
			// SetAttribute() allows prefixed name.
			XmlConvert.VerifyNCName (localName);

			return Attributes.Append (OwnerDocument.CreateAttribute (String.Empty, localName, namespaceURI, false, true));
		}

		public override void WriteContentTo (XmlWriter w)
		{
			int count = ChildNodes.Count;
			for (int i = 0; i < count; i++)
				ChildNodes [i].WriteTo (w);
		}

		public override void WriteTo (XmlWriter w)
		{
			w.WriteStartElement (NamespaceURI == null || NamespaceURI.Length == 0 ? String.Empty : Prefix, LocalName, NamespaceURI);

			for (int i = 0; i < Attributes.Count; i++)
				if (Attributes [i].Specified)
					Attributes [i].WriteTo(w);

			if (IsEmpty)
				w.WriteEndElement ();
			else {
				WriteContentTo (w);
				w.WriteFullEndElement ();
			}
		}

		#endregion
	}
}
