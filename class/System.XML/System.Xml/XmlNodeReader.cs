//
// System.Xml.XmlNodeReader.cs
//
// Author:
//	Duncan Mak  (duncan@ximian.com)
//	Atsushi Enomoto  (ginga@kit.hi-ho.ne.jp)
//
// (C) Ximian, Inc.
// (C) Atsushi Enomoto
//

using System;
using System.Collections;
using System.Xml;
using System.Text;

namespace System.Xml
{
	public class XmlNodeReader : XmlReader
	{
		XmlDocument document;
		XmlNode startNode;
		XmlNode current;
		ReadState state = ReadState.Initial;
		int depth;
		bool isEndElement;
		bool nextIsEndElement;	// used for ReadString()
		bool alreadyRead;
		StringBuilder valueBuilder = new StringBuilder ();
		XmlNamespaceManager defaultNsmgr;
		Stack entityReaderStack = new Stack ();
		XmlTextReader entityReader;

		private XmlNode ownerElement {
			get {
				if (current.ParentNode != null && current.ParentNode.NodeType == XmlNodeType.Attribute)
					return ((XmlAttribute) current.ParentNode).OwnerElement;
				return (current.NodeType == XmlNodeType.Attribute) ? ((XmlAttribute)current).OwnerElement : current;
			}
		}

		#region Constructor

		public XmlNodeReader (XmlNode node)
		{
			startNode = node;
			document = startNode.NodeType == XmlNodeType.Document ?
				startNode as XmlDocument : startNode.OwnerDocument;

			if (node.NodeType != XmlNodeType.Document
				&& node.NodeType != XmlNodeType.DocumentFragment)
				alreadyRead = true;
			defaultNsmgr = new XmlNamespaceManager (this.NameTable);

		}
		
		#endregion

		#region Properties

		public override int AttributeCount {
			get {
				if (entityReader != null)
					return entityReader.ReadState == ReadState.Interactive ?
						entityReader.AttributeCount : 0;

				if (isEndElement || current == null || current.Attributes == null)
					return 0;
				return ownerElement.Attributes.Count;
			}
		}

		public override string BaseURI {
			get {
				if (entityReader != null && entityReader.ReadState != ReadState.Initial)
					return entityReader.BaseURI;

				if (current == null)
					return String.Empty;
				return current.BaseURI;
			}
		}

		public override bool CanResolveEntity {
			get {
				return true;
			}
		}

		public override int Depth {
			get {
				if (entityReader != null && entityReader.ReadState == ReadState.Interactive)
					return entityReader.Depth + depth + entityReaderStack.Count + 1;

				return depth;
			}
		}

		public override bool EOF {
			get {
				return this.ReadState == ReadState.EndOfFile 
				|| this.ReadState == ReadState.Error;
			}
		}

		public override bool HasAttributes {
			get {
				if (entityReader != null)
					return entityReader.ReadState == ReadState.Interactive ?
					      entityReader.HasAttributes : false;

				if (isEndElement || current == null)
					return false;

				if (current.Attributes == null ||
					current.Attributes.Count == 0)
					return false;
				else
					return true;
			}
		}

		public override bool HasValue {
			get {
				if (entityReader != null)
					return entityReader.ReadState == ReadState.Interactive ?
						entityReader.IsDefault : false;

				if (current == null)
					return false;

				if (current.NodeType == XmlNodeType.Element ||
				    current.NodeType == XmlNodeType.EntityReference ||
				    current.NodeType == XmlNodeType.Document ||
				    current.NodeType == XmlNodeType.DocumentFragment ||
				    current.NodeType == XmlNodeType.Notation ||
				    current.NodeType == XmlNodeType.EndElement ||
				    current.NodeType == XmlNodeType.EndEntity)
					return false;
				else
					return true;
			}
			      
		}

		public override bool IsDefault {
			get {
				if (entityReader != null)
					return entityReader.ReadState == ReadState.Interactive ?
						entityReader.IsDefault : false;

				if (current == null)
					return false;

				if (current.NodeType != XmlNodeType.Attribute)
					return false;
				else
				{
					return ((XmlAttribute) current).isDefault;
				}
			}
		}

		public override bool IsEmptyElement {
			get {
				if (entityReader != null)
					return entityReader.ReadState == ReadState.Interactive ?
						entityReader.IsDefault : false;

				if (current == null)
					return false;

				if(current.NodeType == XmlNodeType.Element)
					return ((XmlElement) current).IsEmpty;
				else 
					return false;
			}
		}

		public override string this [int i] {
			get { return GetAttribute (i); }
		}

		public override string this [string name] {
			get { return GetAttribute (name); }
		}

		public override string this [string name, string namespaceURI] {
			get { return GetAttribute (name, namespaceURI); }
		}

		public override string LocalName {
			get {
				if (entityReader != null && entityReader.ReadState != ReadState.Initial)
					return entityReader.LocalName;

				if (current == null)
					return String.Empty;

				switch (current.NodeType) {
				case XmlNodeType.Attribute:
				case XmlNodeType.DocumentType:
				case XmlNodeType.Element:
				case XmlNodeType.EntityReference:
				case XmlNodeType.ProcessingInstruction:
				case XmlNodeType.XmlDeclaration:
					return current.LocalName;
				}

				return String.Empty;
			}
		}

		public override string Name {
			get {
				if (entityReader != null && entityReader.ReadState != ReadState.Initial)
					return entityReader.Name;

				if (current == null)
					return String.Empty;

				switch (current.NodeType) {
				case XmlNodeType.Attribute:
				case XmlNodeType.DocumentType:
				case XmlNodeType.Element:
				case XmlNodeType.EntityReference:
				case XmlNodeType.ProcessingInstruction:
				case XmlNodeType.XmlDeclaration:
					return current.Name;
				}

				return String.Empty;
			}
		}

		public override string NamespaceURI {
			get {
				if (entityReader != null && entityReader.ReadState != ReadState.Initial)
					return entityReader.NamespaceURI;

				if (current == null)
					return String.Empty;

				return current.NamespaceURI;
			}
		}

		public override XmlNameTable NameTable {
			get { return document.NameTable; }
		}

		public override XmlNodeType NodeType {
			get {
				if (entityReader != null)
					switch (entityReader.ReadState) {
					case ReadState.Interactive:
						return entityReader.NodeType;
					case ReadState.Initial:
						return XmlNodeType.EntityReference;
					case ReadState.EndOfFile:
						return XmlNodeType.EndEntity;
					}

				if (current == null)
					return XmlNodeType.None;

				return isEndElement ? XmlNodeType.EndElement : current.NodeType;
			}
		}

		public override string Prefix {
			get { 
				if (entityReader != null && entityReader.ReadState != ReadState.Initial)
					return entityReader.Prefix;

				if (current == null)
					return String.Empty;

				return current.Prefix;
			}
		}

		public override char QuoteChar {
			get {
				if (entityReader != null && entityReader.ReadState != ReadState.Initial)
					return entityReader.QuoteChar;

				return '"';
			}
		}

		public override ReadState ReadState {
			get { return state; }
		}

		public override string Value {
			get {
				if (entityReader != null && entityReader.ReadState != ReadState.Initial)
					return entityReader.Value;

				if (NodeType == XmlNodeType.DocumentType)
					return ((XmlDocumentType) current).InternalSubset;
				else
					return HasValue ? current.Value : String.Empty;
			}
		}

		public override string XmlLang {
			get {
				if (entityReader != null && entityReader.ReadState != ReadState.Initial)
					return entityReader.XmlLang;

				if (current == null)
					return String.Empty;

				return current.XmlLang;
			}
		}

		public override XmlSpace XmlSpace {
			get {
				if (entityReader != null && entityReader.ReadState != ReadState.Initial)
					return entityReader.XmlSpace;

				if (current == null)
					return XmlSpace.None;

				return current.XmlSpace;
			}
		}
		#endregion

		#region Methods

		// If current entityReference is a child of an attribute,
		// then MoveToAttribute simply means that we no more need this entity reader.
		// Otherwise, this invokation means that
		// it is expected to move to resolved (maybe) element's attribute.
		//
		// This rule applies to many methods like MoveTo*Attribute().
		private bool CheckAndResetEntityReaderOnMoveToAttribute ()
		{
			if (entityReader == null)
				return false;

			if (current != null && current.ParentNode != null &&
				current.ParentNode.NodeType == XmlNodeType.Attribute) {
				entityReader.Close ();
				entityReader = entityReaderStack.Count > 0 ?
					entityReaderStack.Pop () as XmlTextReader : null;
				return true;
			}
			else
				return false;
		}

		public override void Close ()
		{
			if (entityReader != null)
				entityReader.Close ();
			while (entityReaderStack.Count > 0)
				((XmlTextReader) entityReaderStack.Pop ()).Close ();

			current = null;
			state = ReadState.Closed;
		}

		public override string GetAttribute (int attributeIndex)
		{
			if (entityReader != null && entityReader.ReadState != ReadState.Initial)
				return entityReader.GetAttribute (attributeIndex);

			if (NodeType == XmlNodeType.XmlDeclaration) {
				XmlDeclaration decl = current as XmlDeclaration;
				if (attributeIndex == 0)
					return decl.Version;
				else if (attributeIndex == 1) {
					if (decl.Encoding != String.Empty)
						return decl.Encoding;
					else if (decl.Standalone != String.Empty)
						return decl.Standalone;
				}
				else if (attributeIndex == 2 &&
						decl.Encoding != String.Empty && decl.Standalone != null)
					return decl.Standalone;
				throw new ArgumentOutOfRangeException ("Index out of range.");
			} else if (NodeType == XmlNodeType.DocumentType) {
				XmlDocumentType doctype = current as XmlDocumentType;
				if (attributeIndex == 0) {
					if (doctype.PublicId != "")
						return doctype.PublicId;
					else if (doctype.SystemId != "")
						return doctype.SystemId;
				} else if (attributeIndex == 1)
					if (doctype.PublicId == "" && doctype.SystemId != "")
						return doctype.SystemId;
				throw new ArgumentOutOfRangeException ("Index out of range.");
			}

			// This is MS.NET bug which returns attributes in spite of EndElement.
			if (isEndElement || current == null)
				return null;

			if (attributeIndex < 0 || attributeIndex > AttributeCount)
				throw new ArgumentOutOfRangeException ("Index out of range.");

			return ownerElement.Attributes [attributeIndex].Value;
		}

		public override string GetAttribute (string name)
		{
			if (entityReader != null && entityReader.ReadState != ReadState.Initial)
				return entityReader.GetAttribute (name);

			// This is MS.NET bug which returns attributes in spite of EndElement.
			if (isEndElement || current == null)
				return null;

			if (NodeType == XmlNodeType.XmlDeclaration)
				return GetXmlDeclarationAttribute (name);
			else if (NodeType == XmlNodeType.DocumentType)
				return GetDocumentTypeAttribute (name);

			XmlAttribute attr = ownerElement.Attributes [name];
			if (attr == null)
				return null;
			else
				return attr.Value;
		}

		public override string GetAttribute (string name, string namespaceURI)
		{
			if (entityReader != null && entityReader.ReadState != ReadState.Initial)
				return entityReader.GetAttribute (name, namespaceURI);

			// This is MS.NET bug which returns attributes in spite of EndElement.
			if (isEndElement || current == null)
				return null;

			if (NodeType == XmlNodeType.XmlDeclaration)
				return GetXmlDeclarationAttribute (name);
			else if (NodeType == XmlNodeType.DocumentType)
				return GetDocumentTypeAttribute (name);

			XmlAttribute attr = ownerElement.Attributes [name, namespaceURI];
			if (attr == null)
				return null;	// In fact MS.NET returns null instead of String.Empty.
			else
				return attr.Value;
		}

		private string GetXmlDeclarationAttribute (string name)
		{
			XmlDeclaration decl = current as XmlDeclaration;
			switch (name) {
			case "version":
				return decl.Version;
			case "encoding":
				// This is MS.NET bug that XmlNodeReturns in case of string.empty.
				return decl.Encoding != String.Empty ? decl.Encoding : null;
			case "standalone":
				return decl.Standalone;
			}
			return null;
		}

		private string GetDocumentTypeAttribute (string name)
		{
			XmlDocumentType doctype = current as XmlDocumentType;
			switch (name) {
			case "PUBLIC":
				return doctype.PublicId;
			case "SYSTEM":
				return doctype.SystemId;
			}
			return null;
		}

		public override string LookupNamespace (string prefix)
		{
			if (entityReader != null && entityReader.ReadState != ReadState.Initial)
				return entityReader.LookupNamespace (prefix);

			if (current == null)
				return null;

			XmlAttribute curAttr = current as XmlAttribute;
			XmlNode target = curAttr != null ? curAttr.OwnerElement : current;

			if (prefix == "") {
				do {
					XmlAttribute attr = target.Attributes ["xmlns"];
					if (attr != null)
						return attr.Value;
					target = target.ParentNode;
				} while (target.NodeType != XmlNodeType.Document);
			} else {
				string name = "xmlns:" + prefix;
				do {
					XmlAttribute attr = target.Attributes [name];
					if (attr != null)
						return attr.Value;
					target = target.ParentNode;
				} while (target.NodeType != XmlNodeType.Document);
			}
			return defaultNsmgr.LookupNamespace (prefix);
		}

		public override void MoveToAttribute (int attributeIndex)
		{
			if (entityReader != null) {
				if (!this.CheckAndResetEntityReaderOnMoveToAttribute ()) {
					entityReader.MoveToAttribute (attributeIndex);
					return;
				}
				// And in case of abondoning entityReader, go on...
			}

			if (isEndElement || attributeIndex < 0 || attributeIndex > AttributeCount)
				throw new ArgumentOutOfRangeException ();
			
			state = ReadState.Interactive;
			current = ownerElement.Attributes [attributeIndex];
		}

		public override bool MoveToAttribute (string name)
		{
			if (entityReader != null) {
				if (!this.CheckAndResetEntityReaderOnMoveToAttribute ())
					return entityReader.MoveToAttribute (name);
				// And in case of abondoning entityReader, go on...
			}

			if (isEndElement || current == null)
				return false;
			XmlNode tmpCurrent = current;
			if (current.ParentNode.NodeType == XmlNodeType.Attribute)
				current = current.ParentNode;

			XmlAttribute attr = ownerElement.Attributes [name];
			if (attr == null) {
				current = tmpCurrent;
				return false;
			}
			else {
				current = attr;
				return true;
			}
		}

		public override bool MoveToAttribute (string name, string namespaceURI)
		{
			if (entityReader != null) {
				if (!this.CheckAndResetEntityReaderOnMoveToAttribute ())
					return entityReader.MoveToAttribute (name, namespaceURI);
				// And in case of abondoning entityReader, go on...
			}

			if (isEndElement || current == null)
				return false;

			XmlAttribute attr = ownerElement.Attributes [name, namespaceURI];
			if (attr == null)
				return false;
			else {
				current = attr;
				return true;
			}
		}

		private void MoveToParentElement ()
		{
			// This is buggy. It is not only the case when EndElement = true.
			isEndElement = true;
			depth--;
			current = current.ParentNode;
		}

		public override bool MoveToElement ()
		{
			if (entityReader != null) {
				if (!this.CheckAndResetEntityReaderOnMoveToAttribute ())
					return entityReader.MoveToElement ();
				// And in case of abondoning entityReader, go on...
			}

			if (current == null)
				return false;
			if (current.NodeType == XmlNodeType.Attribute) {
				current = ((XmlAttribute) current).OwnerElement;
				return true;
			} else 
				return false;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (entityReader != null) {
				if (!this.CheckAndResetEntityReaderOnMoveToAttribute ())
					return entityReader.MoveToFirstAttribute ();
				// And in case of abondoning entityReader, go on...
			}

			if (current == null)
				return false;

			if(ownerElement.Attributes.Count > 0)
			{
				current = ownerElement.Attributes [0];
				return true;
			}
			else
				return false;
		}

		public override bool MoveToNextAttribute ()
		{
			if (entityReader != null) {
				if (!this.CheckAndResetEntityReaderOnMoveToAttribute ())
					return entityReader.MoveToNextAttribute ();
				// And in case of abondoning entityReader, go on...
			}

			if (current == null)
				return false;

			if (current.NodeType != XmlNodeType.Attribute)
				return MoveToFirstAttribute ();
			else
			{
				XmlAttributeCollection ac = ((XmlAttribute) current).OwnerElement.Attributes;
				for (int i=0; i<ac.Count-1; i++)
				{
					XmlAttribute attr = ac [i];
					if (attr == current)
					{
						i++;
						if (i == ac.Count)
							return false;
						current = ac [i];
						return true;
					}
				}
				return false;
			}
		}

		private bool MoveToNextSibling ()
		{
			if (nextIsEndElement) {
				// nextIsEndElement is set only by ReadString.
				nextIsEndElement = false;
				MoveToParentElement ();
			} else if (alreadyRead) {
				alreadyRead = false;
				return current != null;
			}
			if (current.NextSibling != null) {
				isEndElement = false;
				current = current.NextSibling;
			} else {
				MoveToParentElement ();
			}
			if (current == null) {
				state = ReadState.EndOfFile;
				return false;
			}
			else
				return true;
		}

		public override bool Read ()
		{
			if (EOF)
				return false;

			this.CheckAndResetEntityReaderOnMoveToAttribute ();
			if (entityReader != null) {
				// Read finalizes entity reader.
				switch (entityReader.ReadState) {
				case ReadState.Interactive:
				case ReadState.Initial:
					// If it is ended, then other properties/methods will take care.
					entityReader.Read ();
					return true;
				default:
					entityReader = entityReaderStack.Count > 0 ?
						entityReaderStack.Pop () as XmlTextReader : null;
					return Read ();
				}
				// and go on ...
			}

			if (ReadState == ReadState.Initial) {
				current = startNode;
				state = ReadState.Interactive;
				// when startNode is document or fragment
				if (!alreadyRead)
					current = startNode.FirstChild;
				else
					alreadyRead = false;
				if (current == null) {
					state = ReadState.Error;
					return false;
				} else
					return true;
			}

			MoveToElement ();

			if (IsEmptyElement || isEndElement) {
				// Then go up and move to next.
				// If no more nodes, then set EOF.
				isEndElement = false;
				if (current.ParentNode == null
					|| current.ParentNode.NodeType == XmlNodeType.Document
					|| current.ParentNode.NodeType == XmlNodeType.DocumentFragment) {
					current = null;
					state = ReadState.EndOfFile;
					return false;
				} else if (current.NextSibling == null) {
					depth--;
					current = current.ParentNode;
					isEndElement = true;
					return true;
				} else {
					current = current.NextSibling;
					return true;
				}

			} else if (nextIsEndElement) {
				// nextIsEndElement is set only by ReadString.
				nextIsEndElement = false;
				isEndElement = true;
				return current != null;

			} else if (alreadyRead) {
				alreadyRead = false;
				return current != null;
			}

			if (!isEndElement && current.FirstChild != null) {
				isEndElement = false;
				current = current.FirstChild;
				depth++;
			} else if (current.NodeType == XmlNodeType.Element) {
				isEndElement = true;
				if (current.FirstChild != null)
					depth--;
			} else
				MoveToNextSibling ();

			return current != null;
		}

		public override bool ReadAttributeValue ()
		{
			if (entityReader != null) {
				switch (entityReader.ReadState) {
				case ReadState.Interactive:
				case ReadState.Initial:
					// If it is ended, then other properties/methods will take care.
					return entityReader.ReadAttributeValue ();
				default:
					entityReader = entityReaderStack.Count > 0 ?
						entityReaderStack.Pop () as XmlTextReader : null;
					// and go on ...
					return ReadAttributeValue ();
				}
			}

			if (current.NodeType == XmlNodeType.Attribute) {
				current = current.FirstChild;
				return current != null;
			} else if (current.ParentNode.NodeType == XmlNodeType.Attribute) {
				if (current.NextSibling == null)
					return false;
				current = current.NextSibling;
				return true;
			} else
				return false;
		}

#if USE_VERSION_1_0
		// Its traversal behavior is almost same as Read().
		public override string ReadInnerXml ()
		{
			if (entityReader != null) {
				if (entityReader.EOF) {
					entityReader = entityReaderStack.Count > 0 ?
						entityReaderStack.Pop () as XmlTextReader : null;
					return ReadInnerXml ();
				} else
					return entityReader.ReadInnerXml ();
			}

			if (this.state != ReadState.Interactive)
				return String.Empty;

			XmlNode initial = current;
			// Almost copied from XmlTextReader.
			switch (NodeType) {
			case XmlNodeType.Attribute:
				return Value;
			case XmlNodeType.Element:
				if (IsEmptyElement)
					return String.Empty;

				int startDepth = depth;

				bool loop = true;
				do {
					Read ();
					if (NodeType ==XmlNodeType.None)
						throw new XmlException ("unexpected end of xml.");
					else if (NodeType == XmlNodeType.EndElement && depth == startDepth) {
						loop = false;
						Read ();
					}
				} while (loop);
				return initial.InnerXml;
			case XmlNodeType.None:
				return String.Empty;
			default:
				Read ();
				return String.Empty;
			}
		}

		// Its traversal behavior is almost same as Read().
		public override string ReadOuterXml ()
		{
			if (entityReader != null) {
				if (entityReader.EOF) {
					entityReader = entityReaderStack.Count > 0 ?
						entityReaderStack.Pop () as XmlTextReader : null;
					return ReadOuterXml ();
				} else
					return entityReader.ReadOuterXml ();
			}

			if (NodeType == XmlNodeType.EndElement)
				return String.Empty;
			XmlNode initial = current;

			switch (NodeType) {
			case XmlNodeType.Attribute:
				return current.OuterXml;
			case XmlNodeType.Element:
				if (NodeType == XmlNodeType.Element && !IsEmptyElement)
					ReadInnerXml ();
				else
					Read ();
				return initial.OuterXml;
			case XmlNodeType.None:
				return String.Empty;
			default:
				Read ();
				return String.Empty;
			}
		}
#endif

		public override string ReadString ()
		{
			return ReadStringInternal ();
		}

		public override void ResolveEntity ()
		{
			if (NodeType != XmlNodeType.EntityReference)
				throw new InvalidOperationException ("The current node is not an Entity Reference");
			XmlEntity entity = document.DocumentType != null ?
				document.DocumentType.Entities.GetNamedItem (Name) as XmlEntity : null;

			// MS.NET seems simply ignoring undeclared entity reference ;-(
			string replacementText =
				(entity != null) ? entity.InnerText : String.Empty;

			XmlNodeType xmlReaderNodeType = 
				(current.ParentNode != null && current.ParentNode.NodeType == XmlNodeType.Attribute) ?
				XmlNodeType.Attribute : XmlNodeType.Element;

			XmlParserContext ctx = null;
			if (entityReader != null) {
				entityReaderStack.Push (entityReader);
				ctx = entityReader.GetInternalParserContext ();
			}
			if (ctx == null) {
				ctx = new XmlParserContext (document.NameTable,
					current.ConstructNamespaceManager (),
					document.DocumentType.DTD,
					BaseURI, XmlLang, XmlSpace, Encoding.Unicode);
			}
			entityReader = new XmlTextReader (replacementText, xmlReaderNodeType, ctx);
		}

		public override void Skip ()
		{
			// Why is this overriden? Such skipping might raise
			// (or ignore) unexpected validation error.
			base.Skip ();
		}
		#endregion
	}
}
