//
// System.Xml.XmlNode
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2002 Kral Ferch
// (C) 2002 Atsushi Enomoto
//

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.XPath;

namespace System.Xml
{
	public abstract class XmlNode : ICloneable, IEnumerable, IXPathNavigable
	{
		#region Fields

		XmlDocument ownerDocument;
		XmlNode parentNode;
		StringBuilder tmpBuilder;
		XmlLinkedNode lastLinkedChild;
		XmlNodeListChildren childNodes;
		bool isReadOnly;

		#endregion

		#region Constructors

		internal XmlNode (XmlDocument ownerDocument)
		{
			this.ownerDocument = ownerDocument;
		}

		#endregion

		#region Properties

		public virtual XmlAttributeCollection Attributes {
			get { return null; }
		}

		public virtual string BaseURI {
			get {
				// Isn't it conformant to W3C XML Base Recommendation?
				// As far as I tested, there are not...
				return (ParentNode != null) ? ParentNode.BaseURI : OwnerDocument.BaseURI;
			}
		}

		public virtual XmlNodeList ChildNodes {
			get {
				if (childNodes == null)
					childNodes = new XmlNodeListChildren (this);
				return childNodes;
			}
		}

		public virtual XmlNode FirstChild {
			get {
				if (LastChild != null) {
					return LastLinkedChild.NextLinkedSibling;
				}
				else {
					return null;
				}
			}
		}

		public virtual bool HasChildNodes {
			get { return LastChild != null; }
		}

		public virtual string InnerText {
			get {
				StringBuilder builder = new StringBuilder ();
				AppendChildValues (this, builder);
				return builder.ToString ();
			}

			set { throw new InvalidOperationException ("This node is read only. Cannot be modified."); }
		}

		private void AppendChildValues (XmlNode parent, StringBuilder builder)
		{
			XmlNode node = parent.FirstChild;

			while (node != null) {
				switch (node.NodeType) {
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
				case XmlNodeType.SignificantWhitespace:
				case XmlNodeType.Whitespace:
 					builder.Append (node.Value);
					break;
				}
				AppendChildValues (node, builder);
				node = node.NextSibling;
			}
		}

		public virtual string InnerXml {
			get {
				StringWriter sw = new StringWriter ();
				XmlTextWriter xtw = new XmlTextWriter (sw);

				WriteContentTo (xtw);

				return sw.GetStringBuilder ().ToString ();
			}

			set {
				throw new InvalidOperationException ("This node is readonly or doesn't have any children.");
			}
		}

		public virtual bool IsReadOnly {
			get { return isReadOnly; }
		}

		[System.Runtime.CompilerServices.IndexerName("Item")]
		public virtual XmlElement this [string name] {
			get { 
				for (int i = 0; i < ChildNodes.Count; i++) {
					XmlNode node = ChildNodes [i];
					if ((node.NodeType == XmlNodeType.Element) &&
					    (node.Name == name)) {
						return (XmlElement) node;
					}
				}

				return null;
			}
		}

		[System.Runtime.CompilerServices.IndexerName("Item")]
		public virtual XmlElement this [string localname, string ns] {
			get {
				for (int i = 0; i < ChildNodes.Count; i++) {
					XmlNode node = ChildNodes [i];
					if ((node.NodeType == XmlNodeType.Element) &&
					    (node.LocalName == localname) && 
					    (node.NamespaceURI == ns)) {
						return (XmlElement) node;
					}
				}

				return null;
			}
		}

		public virtual XmlNode LastChild {
			get { return LastLinkedChild; }
		}

		internal virtual XmlLinkedNode LastLinkedChild {
			get { return lastLinkedChild; }
			set { lastLinkedChild = value; }
		}

		public abstract string LocalName { get;	}

		public abstract string Name	{ get; }

		public virtual string NamespaceURI {
			get { return String.Empty; }
		}

		public virtual XmlNode NextSibling {
			get { return null; }
		}

		public abstract XmlNodeType NodeType { get;	}

		internal virtual XPathNodeType XPathNodeType {
			get {
				throw new InvalidOperationException ("Can not get XPath node type from " + this.GetType ().ToString ());
			}
		}

		public virtual string OuterXml {
			get {
				StringWriter sw = new StringWriter ();
				XmlTextWriter xtw = new XmlTextWriter (sw);

				WriteTo (xtw);

				return sw.ToString ();
			}
		}

		public virtual XmlDocument OwnerDocument {
			get { return ownerDocument; }
		}

		public virtual XmlNode ParentNode {
			get { return parentNode; }
		}

		public virtual string Prefix {
			get { return String.Empty; }
			set {}
		}

		public virtual XmlNode PreviousSibling {
			get { return null; }
		}

		public virtual string Value {
			get { return null; }
			set { throw new InvalidOperationException ("This node does not have a value"); }
		}

		internal virtual string XmlLang {
			get {
				if(Attributes != null)
					for (int i = 0; i < Attributes.Count; i++) {
						XmlAttribute attr = Attributes [i];
						if(attr.Name == "xml:lang")
							return attr.Value;
					}
				return (ParentNode != null) ? ParentNode.XmlLang : OwnerDocument.XmlLang;
			}
		}

		internal virtual XmlSpace XmlSpace {
			get {
				if(Attributes != null) {
					for (int i = 0; i < Attributes.Count; i++) {
						XmlAttribute attr = Attributes [i];
						if(attr.Name == "xml:space") {
							switch(attr.Value) {
							case "preserve": return XmlSpace.Preserve;
							case "default": return XmlSpace.Default;
							}
							break;
						}
					}
				}
				return (ParentNode != null) ? ParentNode.XmlSpace : OwnerDocument.XmlSpace;
			}
		}

		#endregion

		#region Methods

		public virtual XmlNode AppendChild (XmlNode newChild)
		{
			// I assume that AppendChild(n) equals to InsertAfter(n, this.LastChild) or InsertBefore(n, null)
			return InsertBefore (newChild, null);
		}

		public virtual XmlNode Clone ()
		{
			// By MS document, it is equivalent to CloneNode(true).
			return this.CloneNode (true);
		}

		public abstract XmlNode CloneNode (bool deep);

		public XPathNavigator CreateNavigator ()
		{
			XmlDocument document = this.NodeType == XmlNodeType.Document ?
				this as XmlDocument : this.ownerDocument;
			return document.CreateNavigator (this);
		}

		public IEnumerator GetEnumerator ()
		{
			return new XmlNodeListChildren (this).GetEnumerator ();
		}

		public virtual string GetNamespaceOfPrefix (string prefix)
		{
			if (prefix == null)
				throw new ArgumentNullException ("prefix");

			XmlNode node;
			switch (NodeType) {
			case XmlNodeType.Attribute:
				node = ((XmlAttribute) this).OwnerElement;
				if (node == null)
					return String.Empty;
				break;
			case XmlNodeType.Element:
				node = this;
				break;
			default:
				node = ParentNode;
				break;
			}

			while (node != null) {
				if (node.Prefix == prefix)
					return node.NamespaceURI;
				if (node.Attributes != null) {
					int count = node.Attributes.Count;
					for (int i = 0; i < count; i++) {
						XmlAttribute attr = node.Attributes [i];
						if (prefix == attr.LocalName && attr.Prefix == "xmlns"
							|| attr.Name == "xmlns" && prefix == String.Empty)
							return attr.Value;
					}
				}
				node = node.ParentNode;
			}
			return String.Empty;
		}

		public virtual string GetPrefixOfNamespace (string namespaceURI)
		{
			XmlNode node;
			switch (NodeType) {
			case XmlNodeType.Attribute:
				node = ((XmlAttribute) this).OwnerElement;
				break;
			case XmlNodeType.Element:
				node = this;
				break;
			default:
				node = ParentNode;
				break;
			}

			while (node != null && node.Attributes != null) {
				for (int i = 0; i < Attributes.Count; i++) {
					XmlAttribute attr = Attributes [i];
					if (attr.Prefix == "xmlns" && attr.Value == namespaceURI)
						return attr.LocalName;
					else if (attr.Name == "xmlns" && attr.Value == namespaceURI)
						return String.Empty;
				}
				node = node.ParentNode;
			}
			return String.Empty;
		}

		object ICloneable.Clone ()
		{
			return Clone ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public virtual XmlNode InsertAfter (XmlNode newChild, XmlNode refChild)
		{
			// InsertAfter(n1, n2) is equivalent to InsertBefore(n1, n2.PreviousSibling).

			// I took this way because current implementation 
			// Calling InsertBefore() in this method is faster than
			// the counterpart, since NextSibling is faster than 
			// PreviousSibling (these children are forward-only list).
			XmlNode argNode = null;
			if (refChild != null)
				argNode = refChild.NextSibling;
			else if (ChildNodes.Count > 0)
				argNode = FirstChild;
			return InsertBefore (newChild, argNode);
		}

		public virtual XmlNode InsertBefore (XmlNode newChild, XmlNode refChild)
		{
			return InsertBefore (newChild, refChild, true, true);
		}

		// check for the node to be one of node ancestors
		internal bool IsAncestor (XmlNode newChild)
		{
			XmlNode currNode = this.ParentNode;
			while(currNode != null)
			{
				if(currNode == newChild)
					return true;
				currNode = currNode.ParentNode;
			}
			return false;
		}

		internal XmlNode InsertBefore (XmlNode newChild, XmlNode refChild, bool checkNodeType, bool raiseEvent)
		{
			if (checkNodeType)
				CheckNodeInsertion (newChild, refChild);

			XmlDocument ownerDoc = (NodeType == XmlNodeType.Document) ? (XmlDocument) this : OwnerDocument;

			if (raiseEvent)
				ownerDoc.onNodeInserting (newChild, this);

			if (newChild.ParentNode != null)
				newChild.ParentNode.RemoveChild (newChild, checkNodeType);

			if (newChild.NodeType == XmlNodeType.DocumentFragment) {
				int x = newChild.ChildNodes.Count;
				for (int i = 0; i < x; i++) {
					XmlNode n = newChild.ChildNodes [0];
					this.InsertBefore (n, refChild);	// recursively invokes events. (It is compatible with MS implementation.)
				}
			}
			else {
				XmlLinkedNode newLinkedChild = (XmlLinkedNode) newChild;
				XmlLinkedNode lastLinkedChild = LastLinkedChild;

				newLinkedChild.parentNode = this;

				if (refChild == null) {
					// newChild is the last child:
					// * set newChild as NextSibling of the existing lastchild
					// * set LastChild = newChild
					// * set NextSibling of newChild as FirstChild
					if (LastLinkedChild != null) {
						XmlLinkedNode formerFirst = (XmlLinkedNode) FirstChild;
						LastLinkedChild.NextLinkedSibling = newLinkedChild;
						LastLinkedChild = newLinkedChild;
						newLinkedChild.NextLinkedSibling = formerFirst;
					} else {
						LastLinkedChild = newLinkedChild;
						LastLinkedChild.NextLinkedSibling = newLinkedChild;	// FirstChild
					}
				} else {
					// newChild is not the last child:
					// * if newchild is first, then set next of lastchild is newChild.
					//   otherwise, set next of previous sibling to newChild
					// * set next of newChild to refChild
					XmlLinkedNode prev = refChild.PreviousSibling as XmlLinkedNode;
					if (prev == null)
						LastLinkedChild.NextLinkedSibling = newLinkedChild;
					else
						prev.NextLinkedSibling = newLinkedChild;
					newLinkedChild.NextLinkedSibling = refChild as XmlLinkedNode;
				}
				switch (newChild.NodeType) {
				case XmlNodeType.EntityReference:
					((XmlEntityReference) newChild).SetReferencedEntityContent ();
					break;
				case XmlNodeType.Entity:
					((XmlEntity) newChild).SetEntityContent ();
					break;
				case XmlNodeType.DocumentType:
					foreach (XmlEntity ent in ((XmlDocumentType)newChild).Entities)
						ent.SetEntityContent ();
					break;
				}

				if (raiseEvent)
					ownerDoc.onNodeInserted (newChild, newChild.ParentNode);
			}
			return newChild;
		}

		private void CheckNodeInsertion (XmlNode newChild, XmlNode refChild)
		{
			XmlDocument ownerDoc = (NodeType == XmlNodeType.Document) ? (XmlDocument) this : OwnerDocument;

			if (NodeType != XmlNodeType.Element &&
			    NodeType != XmlNodeType.Attribute &&
			    NodeType != XmlNodeType.Document &&
			    NodeType != XmlNodeType.DocumentFragment)
				throw new InvalidOperationException (String.Format ("Node cannot be appended to current node " + NodeType + "."));

			switch (NodeType) {
			case XmlNodeType.Attribute:
				switch (newChild.NodeType) {
				case XmlNodeType.Text:
				case XmlNodeType.EntityReference:
					break;
				default:
					throw new InvalidOperationException (String.Format (
						"Cannot insert specified type of node {0} as a child of this node {0}.", 
						newChild.NodeType, NodeType));
				}
				break;
			case XmlNodeType.Element:
				switch (newChild.NodeType) {
				case XmlNodeType.Attribute:
				case XmlNodeType.Document:
				case XmlNodeType.DocumentType:
				case XmlNodeType.Entity:
				case XmlNodeType.Notation:
				case XmlNodeType.XmlDeclaration:
					throw new InvalidOperationException ("Cannot insert specified type of node as a child of this node.");
				}
				break;
			}

			if (IsReadOnly)
				throw new InvalidOperationException ("The node is readonly.");

			if (newChild.OwnerDocument != ownerDoc)
				throw new ArgumentException ("Can't append a node created by another document.");

			if (refChild != null) {
				if (refChild.ParentNode != this)
					throw new ArgumentException ("The reference node is not a child of this node.");
			}

			if(this == ownerDoc && ownerDoc.DocumentElement != null && (newChild is XmlElement))
				throw new XmlException ("multiple document element not allowed.");

			// checking validity finished. then appending...

			
			if (newChild == this || IsAncestor (newChild))
				throw new ArgumentException("Cannot insert a node or any ancestor of that node as a child of itself.");

		}

		public virtual void Normalize ()
		{
			StringBuilder tmpBuilder = new StringBuilder ();
			int count = this.ChildNodes.Count;
			int start = 0;
			for (int i = 0; i < count; i++) {
				XmlNode c = ChildNodes [i];
				switch (c.NodeType) {
				case XmlNodeType.Text:
				case XmlNodeType.Whitespace:
				case XmlNodeType.SignificantWhitespace:
					tmpBuilder.Append (c.Value);
					break;
				default:
					c.Normalize ();
					NormalizeRange (start, i, tmpBuilder);
					// Continue to normalize from next node.
					start = i + 1;
					break;
				}
			}
			if (start < count) {
				NormalizeRange (start, count, tmpBuilder);
			}
		}

		private void NormalizeRange (int start, int i, StringBuilder tmpBuilder)
		{
			int keepPos = -1;
			// If Texts and Whitespaces are mixed, Text takes precedence to remain.
			// i.e. Whitespace should be removed.
			for (int j = start; j < i; j++) {
				XmlNode keep = ChildNodes [j];
				if (keep.NodeType == XmlNodeType.Text) {
					keepPos = j;
					break;
				}
				else if (keep.NodeType == XmlNodeType.SignificantWhitespace)
					keepPos = j;
					// but don't break up to find Text nodes.
			}

			if (keepPos >= 0) {
				for (int del = start; del < keepPos; del++)
					RemoveChild (ChildNodes [start]);
				int rest = i - keepPos - 1;
				for (int del = 0; del < rest; del++) {
					RemoveChild (ChildNodes [start + 1]);
}
			}

			if (keepPos >= 0)
				ChildNodes [start].Value = tmpBuilder.ToString ();
			// otherwise nothing to be normalized

			tmpBuilder.Length = 0;
		}

		public virtual XmlNode PrependChild (XmlNode newChild)
		{
			return InsertAfter (newChild, null);
		}

		public virtual void RemoveAll ()
		{
			if (Attributes != null)
				Attributes.RemoveAll ();
			XmlNode next = null;
			for (XmlNode node = FirstChild; node != null; node = next) {
				next = node.NextSibling;
				RemoveChild (node);
			}
		}

		public virtual XmlNode RemoveChild (XmlNode oldChild)
		{
			return RemoveChild (oldChild, true);
		}

		private void CheckNodeRemoval ()
		{
			if (NodeType != XmlNodeType.Attribute && 
				NodeType != XmlNodeType.Element && 
				NodeType != XmlNodeType.Document && 
				NodeType != XmlNodeType.DocumentFragment)
				throw new ArgumentException (String.Format ("This {0} node cannot remove its child.", NodeType));

			if (IsReadOnly)
				throw new ArgumentException (String.Format ("This {0} node is read only.", NodeType));
		}

		internal XmlNode RemoveChild (XmlNode oldChild, bool checkNodeType)
		{
			if (oldChild == null)
				throw new NullReferenceException ();
			XmlDocument ownerDoc = (NodeType == XmlNodeType.Document) ? (XmlDocument)this : OwnerDocument;
			if(oldChild.ParentNode != this)
				throw new ArgumentException ("The node to be removed is not a child of this node.");

			if (checkNodeType)
				ownerDoc.onNodeRemoving (oldChild, oldChild.ParentNode);

			if (checkNodeType)
				CheckNodeRemoval ();

			if (Object.ReferenceEquals (LastLinkedChild, LastLinkedChild.NextLinkedSibling) && Object.ReferenceEquals (LastLinkedChild, oldChild))
				// If there is only one children, simply clear.
				LastLinkedChild = null;
			else {
				XmlLinkedNode oldLinkedChild = (XmlLinkedNode) oldChild;
				XmlLinkedNode beforeLinkedChild = LastLinkedChild;
				XmlLinkedNode firstChild = (XmlLinkedNode) FirstChild;
				
				while (Object.ReferenceEquals (beforeLinkedChild.NextLinkedSibling, LastLinkedChild) == false && 
					Object.ReferenceEquals (beforeLinkedChild.NextLinkedSibling, oldLinkedChild) == false)
					beforeLinkedChild = beforeLinkedChild.NextLinkedSibling;

				if (Object.ReferenceEquals (beforeLinkedChild.NextLinkedSibling, oldLinkedChild) == false)
					throw new ArgumentException ();

				beforeLinkedChild.NextLinkedSibling = oldLinkedChild.NextLinkedSibling;

				// Each derived class may have its own LastLinkedChild, so we must set it explicitly.
				if (oldLinkedChild.NextLinkedSibling == firstChild)
					this.LastLinkedChild = beforeLinkedChild;

				oldLinkedChild.NextLinkedSibling = null;
				}

			if (checkNodeType)
				ownerDoc.onNodeRemoved (oldChild, oldChild.ParentNode);
			oldChild.parentNode = null;	// clear parent 'after' above logic.

			return oldChild;
		}

		public virtual XmlNode ReplaceChild (XmlNode newChild, XmlNode oldChild)
		{
			if(oldChild.ParentNode != this)
				throw new ArgumentException ("The node to be removed is not a child of this node.");
			
			if (newChild == this || IsAncestor (newChild))
				throw new InvalidOperationException("Cannot insert a node or any ancestor of that node as a child of itself.");
			
			for (int i = 0; i < ChildNodes.Count; i++) {
				XmlNode n = ChildNodes [i];
				if(n == oldChild) {
					XmlNode prev = oldChild.PreviousSibling;
					RemoveChild (oldChild);
					InsertAfter (newChild, prev);
					break;
				}
			}
			return oldChild;
		}

		internal void SearchDescendantElements (string name, bool matchAll, ArrayList list)
		{
			for (int i = 0; i < ChildNodes.Count; i++) {
				XmlNode n = ChildNodes [i];
				if (n.NodeType != XmlNodeType.Element)
					continue;
				if (matchAll || n.Name == name)
					list.Add (n);
				n.SearchDescendantElements (name, matchAll, list);
			}
		}

		internal void SearchDescendantElements (string name, bool matchAllName, string ns, bool matchAllNS, ArrayList list)
		{
			for (int i = 0; i < ChildNodes.Count; i++) {
				XmlNode n = ChildNodes [i];
				if (n.NodeType != XmlNodeType.Element)
					continue;
				if ((matchAllName || n.LocalName == name)
					&& (matchAllNS || n.NamespaceURI == ns))
					list.Add (n);
				n.SearchDescendantElements (name, matchAllName, ns, matchAllNS, list);
			}
		}

		public XmlNodeList SelectNodes (string xpath)
		{
			return SelectNodes (xpath, null);
		}

		public XmlNodeList SelectNodes (string xpath, XmlNamespaceManager nsmgr)
		{
			XPathNavigator nav = CreateNavigator ();
			XPathExpression expr = nav.Compile (xpath);
			if (nsmgr != null)
				expr.SetContext (nsmgr);
			XPathNodeIterator iter = nav.Select (expr);
			ArrayList rgNodes = new ArrayList ();
			while (iter.MoveNext ())
			{
				rgNodes.Add (((XmlDocumentNavigator) iter.Current).Node);
			}
			return new XmlNodeArrayList (rgNodes);
		}

		public XmlNode SelectSingleNode (string xpath)
		{
			return SelectSingleNode (xpath, null);
		}

		public XmlNode SelectSingleNode (string xpath, XmlNamespaceManager nsmgr)
		{
			XPathNavigator nav = CreateNavigator ();
			XPathExpression expr = nav.Compile (xpath);
			if (nsmgr != null)
				expr.SetContext (nsmgr);
			XPathNodeIterator iter = nav.Select (expr);
			if (!iter.MoveNext ())
				return null;
			return ((XmlDocumentNavigator) iter.Current).Node;
		}

		internal static void SetReadOnly (XmlNode n)
		{
			if (n.Attributes != null)
				for (int i = 0; i < n.Attributes.Count; i++)
					SetReadOnly (n.Attributes [i]);
			for (int i = 0; i < n.ChildNodes.Count; i++)
				SetReadOnly (n.ChildNodes [i]);
			n.isReadOnly = true;
		}

		internal void SetReadOnly ()
		{
			isReadOnly = true;
		}

		public virtual bool Supports (string feature, string version)
		{
			if (String.Compare (feature, "xml", true, CultureInfo.InvariantCulture) == 0 // not case-sensitive
			    && (String.Compare (version, "1.0", true, CultureInfo.InvariantCulture) == 0
				|| String.Compare (version, "2.0", true, CultureInfo.InvariantCulture) == 0))
				return true;
			else
				return false;
		}

		public abstract void WriteContentTo (XmlWriter w);

		public abstract void WriteTo (XmlWriter w);

		// It parses this and all the ancestor elements,
		// find 'xmlns' declarations, stores and then return them.
		internal XmlNamespaceManager ConstructNamespaceManager ()
		{
			XmlDocument doc = this is XmlDocument ? (XmlDocument)this : this.OwnerDocument;
			XmlNamespaceManager nsmgr = new XmlNamespaceManager (doc.NameTable);
			XmlElement el = null;
			switch(this.NodeType) {
			case XmlNodeType.Attribute:
				el = ((XmlAttribute)this).OwnerElement;
				break;
			case XmlNodeType.Element:
				el = this as XmlElement;
				break;
			default:
				el = this.ParentNode as XmlElement;
				break;
			}

			while (el != null) {
				for (int i = 0; i < el.Attributes.Count; i++) {
					XmlAttribute attr = el.Attributes [i];
					if(attr.Prefix == "xmlns") {
						if (nsmgr.LookupNamespace (attr.LocalName) != attr.Value)
							nsmgr.AddNamespace (attr.LocalName, attr.Value);
					} else if(attr.Name == "xmlns") {
						if(nsmgr.LookupNamespace (String.Empty) != attr.Value)
							nsmgr.AddNamespace (String.Empty, attr.Value);
					}
				}
				// When reached to document, then it will set null value :)
				el = el.ParentNode as XmlElement;
			}
			return nsmgr;
		}
		#endregion
	}
}
