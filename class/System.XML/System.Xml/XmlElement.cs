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

using System;
using System.Collections;
using System.Xml.XPath;
using System.IO;
using System.Text;

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
			XmlDocument doc) : base (doc)
		{
			this.prefix = prefix;
			this.localName = localName;
			this.namespaceURI = namespaceURI;

			attributes = new XmlAttributeCollection (this);

			// TODO: Adds default attributes
			if(doc.DocumentType != null)
			{
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
					if(FirstChild != null) {
						foreach (XmlNode n in ChildNodes)
							this.RemoveChild (n);
					}
					// creates new Text node
					AppendChild(OwnerDocument.CreateTextNode(value));
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

				// I hope there are any well-performance logic...
				XmlNameTable nt = this.OwnerDocument.NameTable;
				XmlNamespaceManager nsmgr = this.ConstructNamespaceManager ();
				XmlParserContext ctx = new XmlParserContext (OwnerDocument.NameTable, nsmgr,
					OwnerDocument.DocumentType != null ? OwnerDocument.DocumentType.DTD : null,
					BaseURI, XmlLang, XmlSpace, null);
				XmlTextReader xmlReader = new XmlTextReader (value, XmlNodeType.Element, ctx);

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

		[MonoTODO]
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
				Exception ex;
				if (!XmlConstructs.IsValidNCName (value, out ex))
					throw ex;

				prefix = value;
			}
		}

		#endregion

		#region Methods
		
		public override XmlNode CloneNode (bool deep)
		{
			XmlElement node = new XmlElement (
				prefix, localName, namespaceURI, OwnerDocument);

			for (int i = 0; i < Attributes.Count; i++)
				node.SetAttributeNode ((XmlAttribute) 
					Attributes [i].CloneNode (true));

			if (deep) {
				foreach (XmlNode child in this.ChildNodes)
					node.AppendChild (child.CloneNode (true));
			} // shallow cloning

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
			this.searchNodesRecursively (this, name, nodeArrayList);
			return new XmlNodeArrayList (nodeArrayList);
		}

		private void searchNodesRecursively (XmlNode argNode, string argName, 
			ArrayList argArrayList)
		{
			XmlNodeList xmlNodeList = argNode.ChildNodes;
			foreach (XmlNode node in xmlNodeList){
				if (node.Name.Equals (argName))
					argArrayList.Add (node);
				else	
					this.searchNodesRecursively (node, argName, argArrayList);
			}
		}

		private void searchNodesRecursively (XmlNode argNode, string argName, string argNamespaceURI, 
			ArrayList argArrayList)
		{
			XmlNodeList xmlNodeList = argNode.ChildNodes;
			foreach (XmlNode node in xmlNodeList)
			{
				if (node.LocalName.Equals (argName) && node.NamespaceURI.Equals (argNamespaceURI))
					argArrayList.Add (node);
				else	
					this.searchNodesRecursively (node, argName, argNamespaceURI, argArrayList);
			}
		}

		public virtual XmlNodeList GetElementsByTagName (string localName, string namespaceURI)
		{
			ArrayList nodeArrayList = new ArrayList ();
			this.searchNodesRecursively (this, localName, namespaceURI, nodeArrayList);
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

		[MonoTODO ("confirm not removing default attributes [when DTD feature was implemented.")]
		public override void RemoveAll ()
		{
			// Remove the child nodes.
			base.RemoveAll ();

			// Remove all attributes.
			attributes.RemoveAll ();
		}

		[MonoTODO ("confirm not removing default attributes [when DTD feature was implemented.")]
		public virtual void RemoveAllAttributes ()
		{
			attributes.RemoveAll ();
		}

		[MonoTODO ("confirm not resetting default attributes [when DTD feature was implemented.")]
		public virtual void RemoveAttribute (string name)
		{
			XmlAttribute attr = attributes.GetNamedItem (name) as XmlAttribute;
			if (attr != null)
				attributes.Remove(attr);
		}

		[MonoTODO ("confirm not resetting default attributes [when DTD feature was implemented.")]
		public virtual void RemoveAttribute (string localName, string namespaceURI)
		{
			XmlAttribute attr = attributes.GetNamedItem(localName, namespaceURI) as XmlAttribute;
			if (attr != null)
				attributes.Remove(attr);
		}

		[MonoTODO ("confirm not resetting default attributes [when DTD feature was implemented.")]
		public virtual XmlNode RemoveAttributeAt (int i)
		{
			return attributes.RemoveAt (i);
		}

		[MonoTODO ("confirm not resetting default attributes [when DTD feature was implemented.")]
		public virtual XmlAttribute RemoveAttributeNode (XmlAttribute oldAttr)
		{
			return attributes.Remove(oldAttr);
		}

		[MonoTODO ("confirm not resetting default attributes [when DTD feature was implemented.")]
		public virtual XmlAttribute RemoveAttributeNode (string localName, string namespaceURI)
		{
			return attributes.Remove(attributes[localName, namespaceURI]);
		}

		public virtual void SetAttribute (string name, string value)
		{
			XmlAttribute attribute = OwnerDocument.CreateAttribute (name);
			attribute.SetOwnerElement(this);
			attribute.Value = value;
			Attributes.SetNamedItem (attribute);
		}

		public virtual string SetAttribute (string localName, string namespaceURI, string value)
		{
			XmlAttribute attr = attributes[localName, namespaceURI];
			if(attr == null)
			{
				attr = OwnerDocument.CreateAttribute(localName, namespaceURI);
				attr.Value = value;
				attributes.SetNamedItem(attr);
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

			newAttr.SetOwnerElement(this);
			XmlNode oldAttr = Attributes.SetNamedItem(newAttr);
			return oldAttr != null ? oldAttr as XmlAttribute : null;
		}

		public virtual XmlAttribute SetAttributeNode (string localName, string namespaceURI)
		{
			XmlDocument xmlDoc = this.OwnerDocument;
			XmlAttribute xmlAttribute = new XmlAttribute (String.Empty, localName, namespaceURI, xmlDoc);
			return this.attributes.Append (xmlAttribute);
		}

		public override void WriteContentTo (XmlWriter w)
		{
			foreach(XmlNode childNode in ChildNodes)
				childNode.WriteTo(w);
		}

		public override void WriteTo (XmlWriter w)
		{
			w.WriteStartElement(Prefix, LocalName, NamespaceURI);

			foreach(XmlAttribute attributeNode in Attributes)
				if (attributeNode.Specified)
					attributeNode.WriteTo(w);
			if (IsEmpty)
				w.WriteEndElement ();
			else {
				WriteContentTo(w);
				w.WriteFullEndElement();
			}
		}

		#endregion
	}
}
