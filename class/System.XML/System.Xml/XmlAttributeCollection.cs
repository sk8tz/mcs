//
// System.Xml.XmlAttributeCollection
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
using Mono.Xml;

namespace System.Xml
{
	public class XmlAttributeCollection : XmlNamedNodeMap, ICollection
	{
		XmlElement ownerElement;
		XmlDocument ownerDocument;

		internal XmlAttributeCollection (XmlNode parent) : base (parent)
		{
			ownerElement = parent as XmlElement;
			ownerDocument = parent.OwnerDocument;
			if(ownerElement == null)
				throw new XmlException ("invalid construction for XmlAttributeCollection.");
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		bool IsReadOnly {
			get { return ownerElement.IsReadOnly; }
		}

		[System.Runtime.CompilerServices.IndexerName ("ItemOf")]
		public virtual XmlAttribute this [string name] {
			get {
				return (XmlAttribute) GetNamedItem (name);
			}
		}

		[System.Runtime.CompilerServices.IndexerName ("ItemOf")]
		public virtual XmlAttribute this [int i] {
			get {
				return (XmlAttribute) Nodes [i];
			}
		}

		[System.Runtime.CompilerServices.IndexerName ("ItemOf")]
		public virtual XmlAttribute this [string localName, string namespaceURI] {
			get {
				return (XmlAttribute) GetNamedItem (localName, namespaceURI);
			}
		}

		object ICollection.SyncRoot {
			get { return this; }
		}

		
		public virtual XmlAttribute Append (XmlAttribute node) 
		{
			XmlNode xmlNode = this.SetNamedItem (node);
			return node;
		}	

		public void CopyTo (XmlAttribute[] array, int index)
		{
			// assuming that Nodes is a correct collection.
			for(int i=0; i<Nodes.Count; i++)
				array [index + i] = Nodes [i] as XmlAttribute;
		}

		void ICollection.CopyTo (Array array, int index)
		{
			// assuming that Nodes is a correct collection.
			array.CopyTo (Nodes.ToArray (typeof(XmlAttribute)), index);
		}

		public virtual XmlAttribute InsertAfter (XmlAttribute newNode, XmlAttribute refNode)
		{
			if (refNode == null) {
				if (Nodes.Count == 0)
					return InsertBefore (newNode, null);
				else
					return InsertBefore (newNode, this [0]);
			}
			for (int i = 0; i < Nodes.Count; i++)
				if (refNode == Nodes [i])
					return InsertBefore (newNode, Nodes.Count == i + 1 ? null : this [i + 1]);

			throw new ArgumentException ("refNode not found in this collection.");
		}

		public virtual XmlAttribute InsertBefore (XmlAttribute newNode, XmlAttribute refNode)
		{
			if (newNode.OwnerDocument != ownerDocument)
				throw new ArgumentException ("different document created this newNode.");

			ownerDocument.onNodeInserting (newNode, null);

			int pos = Nodes.Count;
			if (refNode != null) {
				for (int i = 0; i < Nodes.Count; i++) {
					XmlNode n = Nodes [i] as XmlNode;
					if (n == refNode) {
						pos = i;
						break;
					}
				}
				if (pos == Nodes.Count)
					throw new ArgumentException ("refNode not found in this collection.");
			}
			SetNamedItem (newNode, pos, false);

			ownerDocument.onNodeInserted (newNode, null);

			return newNode;
		}

		public virtual XmlAttribute Prepend (XmlAttribute node) 
		{
			return this.InsertAfter (node, null);
		}

		public virtual XmlAttribute Remove (XmlAttribute node) 
		{
			if (node == null)
				throw new ArgumentException ("Specified node is null.");
			if (node.OwnerDocument != ownerDocument)
				throw new ArgumentException ("Specified node is in a different document.");

			XmlAttribute retAttr = null;
			foreach (XmlAttribute attr in Nodes) {
				if (attr == node) {
					retAttr = attr;
					break;
				}
			}

			if(retAttr != null) {
				ownerDocument.onNodeRemoving (node, null);
				base.RemoveNamedItem (retAttr.LocalName, retAttr.NamespaceURI);
				RemoveIdenticalAttribute (retAttr);
				ownerDocument.onNodeRemoved (node, null);
			}
			// If it is default, then directly create new attribute.
			DTDAttListDeclaration attList = ownerDocument.DocumentType != null ? ownerDocument.DocumentType.DTD.AttListDecls [ownerElement.Name] : null;
			DTDAttributeDefinition def = attList != null ? attList [retAttr.Name] : null;
			if (def != null && def.DefaultValue != null) {
				XmlAttribute attr = ownerDocument.CreateAttribute (
					retAttr.Prefix, retAttr.LocalName, retAttr.NamespaceURI);
				attr.Value = def.DefaultValue;
				attr.SetDefault ();
				this.SetNamedItem (attr);
			}
			retAttr.SetOwnerElement (null);
			return retAttr;
		}

		public virtual void RemoveAll () 
		{
			int current = 0;
			while (current < Count) {
				XmlAttribute attr = this [current];
				if (!attr.Specified)
					current++;
				// It is called for the purpose of event support.
				Remove (attr);
			}
		}

		public virtual XmlAttribute RemoveAt (int i) 
		{
			if(Nodes.Count <= i)
				return null;
			return Remove ((XmlAttribute)Nodes [i]);
		}

		public override XmlNode SetNamedItem (XmlNode node)
		{
			if(IsReadOnly)
				throw new XmlException ("this AttributeCollection is read only.");

			XmlAttribute attr = node as XmlAttribute;
			if (attr.OwnerElement != null)
				throw new InvalidOperationException ("This attribute is already set to another element.");

			ownerElement.OwnerDocument.onNodeInserting (node, ownerElement);

			attr.SetOwnerElement (ownerElement);
			XmlNode n = AdjustIdenticalAttributes (node as XmlAttribute, base.SetNamedItem (node, -1, false));

			ownerElement.OwnerDocument.onNodeInserted (node, ownerElement);

			return n as XmlAttribute;
		}

		internal void AddIdenticalAttribute ()
		{
			SetIdenticalAttribute (false);
		}

		internal void RemoveIdenticalAttribute ()
		{
			SetIdenticalAttribute (true);
		}

		private void SetIdenticalAttribute (bool remove)
		{
			if (ownerElement == null)
				return;

			// Check if new attribute's datatype is ID.
			XmlDocumentType doctype = ownerDocument.DocumentType;
			if (doctype == null || doctype.DTD == null)
				return;
			DTDElementDeclaration elem = doctype.DTD.ElementDecls [ownerElement.Name];
			foreach (XmlAttribute node in this) {
				DTDAttributeDefinition attdef = elem == null ? null : elem.Attributes [node.Name];
				if (attdef == null || attdef.Datatype.TokenizedType != XmlTokenizedType.ID)
					continue;

				if (remove) {
					if (ownerDocument.GetIdenticalAttribute (node.Value) != null) {
						ownerDocument.RemoveIdenticalAttribute (node.Value);
						return;
					}
				} else {
					// adding new identical attribute, but 
					// MS.NET is pity for ID support, so I'm wondering how to correct it...
					if (ownerDocument.GetIdenticalAttribute (node.Value) != null)
						throw new XmlException (String.Format (
							"ID value {0} already exists in this document.", node.Value));
					ownerDocument.AddIdenticalAttribute (node);
					return;
				}
			}

		}

		private XmlNode AdjustIdenticalAttributes (XmlAttribute node, XmlNode existing)
		{
			// If owner element is not appended to the document,
			// ID table should not be filled.
			if (ownerElement == null)
				return existing;

			RemoveIdenticalAttribute (existing);

			// Check if new attribute's datatype is ID.
			XmlDocumentType doctype = node.OwnerDocument.DocumentType;
			if (doctype == null || doctype.DTD == null)
				return existing;
			DTDAttListDeclaration attList = doctype.DTD.AttListDecls [ownerElement.Name];
			DTDAttributeDefinition attdef = attList == null ? null : attList.Get (node.Name);
			if (attdef == null || attdef.Datatype.TokenizedType != XmlTokenizedType.ID)
				return existing;

			// adding new identical attribute, but 
			// MS.NET is pity for ID support, so I'm wondering how to correct it...
			if (ownerDocument.GetIdenticalAttribute (node.Value) != null)
				throw new XmlException (String.Format (
					"ID value {0} already exists in this document.", node.Value));
			ownerDocument.AddIdenticalAttribute (node);

			return existing;
		}

		private XmlNode RemoveIdenticalAttribute (XmlNode existing)
		{
			// If owner element is not appended to the document,
			// ID table should not be filled.
			if (ownerElement == null)
				return existing;

			if (existing != null) {
				// remove identical attribute (if it is).
				if (ownerDocument.GetIdenticalAttribute (existing.Value) != null)
					ownerDocument.RemoveIdenticalAttribute (existing.Value);
			}

			return existing;
		}
	}
}
