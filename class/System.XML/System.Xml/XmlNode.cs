// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.XmlNode
//
// Author:
//   Daniel Weber (daniel-weber@austin.rr.com)
//
// (C) 2001 Daniel Weber

using System;
using System.Collections;
using System.Xml.XPath;

namespace System.Xml 
{
	public abstract class XmlNode : ICloneable, IEnumerable, IXPathNavigable 
	{
		//======= Private data members ==============================================
		private XmlNodeListAsArrayList _childNodes;
		protected XmlDocument FOwnerDocument;
		protected XmlNode _parent;

		// Names of node
		// for <foo:bar xmlns:foo="http://www.foobar.com/schema/foobar">... </foo:bar>
		//	qualified name: foo:bar
		//	namespaceURI = "http://www.foobar.com/schema/foobar"
		//  localName = bar
		//	prefix = foo
		// Note that namespaces can be nested (child namespace != parent namespace)
		// namespaces are optional
		protected string Fname;
		protected string FnamespaceURI;
		protected string Fprefix;
		protected string FlocalName;

		// baseURI holds the location from which the document was loaded
		// If the node was created from a document at c:\tmp.xml, then that's what will be here
		protected string FbaseURI;

		// value of the node (overriden in classes that do something different)
		//	default behavior is just to store it
		protected string Fvalue;
		
		//=====================================================================
		// ============ Properties ============================================
		//=====================================================================
		/// <summary>
		/// Get the XmlAttributeCollection representing the attributes
		///   on the node type.  Returns null if the node type is not XmlElement.
		/// </summary>
		public virtual XmlAttributeCollection Attributes 
		{
			get 
			{
				return null;
			}
		}
		/// <summary>
		///  Return the base Uniform Resource Indicator (URI) used to resolve
		///  this node, or String.Empty.
		/// </summary>
		public virtual string BaseURI 
		{
			get 
			{
				return FbaseURI;
			}

		}
		/// <summary>
		/// Return all child nodes of this node.  If there are no children,
		///  return an empty XmlNodeList;
		/// </summary>
		public virtual XmlNodeList ChildNodes 
		{
			get 
			{
				if (_childNodes == null)
					_childNodes = new XmlNodeListAsArrayList();

				return _childNodes as XmlNodeList;
			}
		}
		
		/// <summary>
		/// Return first child node as XmlNode or null
		/// if the node has no children
		/// </summary>
		public virtual XmlNode FirstChild 
		{
			get
			{
				if (ChildNodes.Count == 0)
					return null;
				else
					return ChildNodes[0];
			}
		}

		/// <summary>
		///		Return true if the node has children
		/// </summary>
		public virtual bool HasChildNodes 
		{
			get 
			{
				if (ChildNodes.Count == 0)
					return true;
				else
					return false;
			}
		}

		/// <summary>
		/// Get or Set the concatenated values of node and children
		/// </summary>
		public virtual string InnerText
		{
			get
			{
				// TODO - implement set InnerText()
				throw new NotImplementedException();
			}

			set 
			{
				// TODO - implement set InnerText()
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Get/Set the XML representing just the child nodes of this node
		/// </summary>
		public virtual string InnerXml
		{
			get
			{
				// TODO - implement set InnerXml()
				throw new NotImplementedException();
			}

			set 
			{
				// TODO - implement set InnerXml()
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Property Get - true if node is read-only
		/// </summary>
		public virtual bool IsReadOnly
		{
			get
			{
				return OwnerDocument.IsReadOnly;
			}
		}

		/// <summary>
		/// Return the child element named [string].  Returns XmlElement
		/// Indexer for XmlNode class.
		/// </summary>
		[System.Runtime.CompilerServices.IndexerNameAttribute("Item")]
		public virtual XmlElement this [String index]
		{
			get 
			{
				// TODO - implement XmlNode.Item(string)
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Get the last child node, or null if there are no nodes
		/// </summary>
		public virtual XmlNode LastChild
		{
			get
			{
                if (_childNodes.Count == 0)
				 return null;
				else
				 return _childNodes.Item(_childNodes.Count - 1);
			}

		}

		/// <summary>
		/// Returns the local name of the node with qualifiers removed
		/// LocalName of ns:elementName = "elementName"
		/// </summary>
		public abstract string LocalName {get;}

		/// <summary>
		/// Get the qualified node name
		/// derived classes must implement as behavior varies
		/// by tag type.
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		/// Get the namespace URI or String.Empty if none
		/// </summary>
		public virtual string NamespaceURI
		{
			get
			{
				// TODO - implement Namespace URI, or determine abstractness
				throw new NotImplementedException("XmlNode.NamespaceURI not implemented");
			}
		}

		/// <summary>
		/// Get the node immediatelly following this node, or null
		/// </summary>
		public virtual XmlNode NextSibling 
		{
			get
			{
				
				if (_parent != null)
				{
					XmlNodeListAsArrayList children = _parent.ChildNodes as XmlNodeListAsArrayList;
					int ourIndex = children.data.IndexOf(this);
					return children[ourIndex + 1];
				}
				else
					return null;
			}
		}

		public virtual XmlNodeType NodeType 
		{
			get
			{
				return XmlNodeType.None;
			}
		}

		/// <summary>
		/// Return the string representing this node and all it's children
		/// </summary>
		public virtual string OuterXml
		{
			get
			{
				// TODO - implement OuterXml {get;}
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Return owning document.
		/// If this nodeType is a document, return null
		/// </summary>
		public virtual XmlDocument OwnerDocument
		{
			get
			{
				return FOwnerDocument;
			}
		}

		/// <summary>
		/// Returns the parent node, or null
		/// Return value depends on superclass node type
		/// </summary>
		public virtual XmlNode ParentNode
		{
			get
			{
				return _parent;
			}
		}
		
		/// <summary>
		/// set/get the namespace prefix for this node, or 
		/// string.empty if it does not exist
		/// </summary>
		public virtual string Prefix 
		{
			get
			{
				return Fprefix;
			}
			
			set
			{
				// TODO - validation on XmlNode.Prefix {set;}? (no)
				Fprefix = value;
			}
		}

		/// <summary>
		/// The preceding XmlNode or null
		/// </summary>
		public virtual XmlNode PreviousSibling {
			get
			{
				if (_parent != null)
				{
					XmlNodeListAsArrayList children = _parent.ChildNodes as XmlNodeListAsArrayList;
					int ourIndex = children.data.IndexOf(this);
					return children[ourIndex - 1];
				}
				else
					return null;
			}
		}

		/// <summary>
		/// Get/Set the value for this node
		/// </summary>
		public virtual string Value 
		{
			get
			{
				return Fvalue;
			}
			
			set
			{
				Fvalue = value;
			}
		}

		//=====================================================================
		//======= Methods =====================================================
		//=====================================================================
		/// <summary>
		/// Appends the specified node to the end of the child node list
		/// </summary>
		/// <param name="newChild"></param>
		/// <returns></returns>
		public virtual XmlNode AppendChild (XmlNode newChild)
		{
			_childNodes.Add(newChild);
			return newChild;
		}

		/// <summary>
		/// Return a clone of this node
		/// </summary>
		/// <returns></returns>
		public virtual object Clone()
		{
			// TODO - implement XmlNode.Clone() as object
			throw new NotImplementedException("object XmlNode.Clone() not implmented");
		}

		/// <summary>
		/// Return a clone of the node
		/// </summary>
		/// <param name="deep">Make copy of all children</param>
		/// <returns>Cloned node</returns>
		public abstract XmlNode CloneNode( bool deep);

		/// <summary>
		/// Return an XPathNavigator for navigating this node
		/// </summary>
		/// <returns></returns>
		public System.Xml.XPath.XPathNavigator CreateNavigator()
		{
			// TODO - implement CreateNavigator()
			throw new NotImplementedException();
		}

		/// <summary>
		/// Provide support for "for each" 
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return _childNodes.data.GetEnumerator();
		}

		/// <summary>
		/// Look up the closest namespace for this node that is in scope for the given prefix
		/// </summary>
		/// <param name="prefix"></param>
		/// <returns>Namespace URI</returns>
		public virtual string GetNamespaceOfPrefix(string prefix)
		{
			// TODO - implement GetNamespaceOfPrefix()
			throw new NotImplementedException();
		}

		/// <summary>
		/// Get the closest xmlns declaration for the given namespace URI that is in scope.
		/// Returns the prefix defined in that declaration.
		/// </summary>
		/// <param name="namespaceURI"></param>
		/// <returns></returns>
		public virtual string GetPrefixOfNamespace(string namespaceURI)
		{
			// TODO - implement GetPrefixOfNamespace
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Insert newChild directly after the reference node.
		/// If refChild is null, newChild is inserted at the beginning of childnodes.
		/// If newChild is a document fragment, all nodes are inserted after refChild.
		/// If newChild is already in the tree, it is first removed.
		/// </summary>
		/// <exception cref="ArgumentException">NewChild was created from differant document.
		/// RefChild not a child of this node or null.
		/// Node is read-only</exception>
		/// <exception cref="InvalidOperationException">Node is of type that does not have children
		/// Node to insert is an ancestor of this node.</exception>
		/// <param name="newChild">Child node to insert.</param>
		/// <param name="refChild">Reference node to insert after</param>
		/// <returns>Removed node, or null if no node removed.</returns>
		public virtual XmlNode InsertAfter(XmlNode newChild, XmlNode refChild)
		{
			// Checks parent not ancestor, arguments valid, etc.  Throws exception on error
			InsertionCheck(newChild, refChild);

			// Scan the node list, looking for refChild and seeing if newChild is in the list
			// Note that if refNode is null (prepend), we don't want to do the .Equals(null)
			XmlNode retval = null;
			int refNodeIndex = -1;
			
			for (int i = 0; i < _childNodes.Count; i++)
			{
				XmlNode e = _childNodes.data[i] as XmlNode;
				if (e.Equals(newChild))
				{
					retval = e;
					FOwnerDocument.onNodeRemoving(newChild, newChild.ParentNode);
					_childNodes.data.RemoveAt(i);
					newChild.setParent(null);
					FOwnerDocument.onNodeRemoved(newChild, null);
					break;
		
				}

				if ( (refChild != null ) & ( e.Equals(refChild) ) )
				{
					refNodeIndex = i;

					if (retval != null)
						break;
				}
			}

			if ( ( refNodeIndex == -1 ) & (refChild != null) )
				throw new ArgumentException("Reference node not found (and not null) in call to XmlNode.InsertAfter()");

			FOwnerDocument.onNodeInserting(newChild, this);

			if (refChild == null)
				refNodeIndex = 0;
			else
				refNodeIndex++;			// insert after reference...

			if (newChild.NodeType == XmlNodeType.DocumentFragment)
			{
				// Insert all children, starting from refNodeIndex (0,1,2...n)
				for (int i = 0; i < newChild.ChildNodes.Count; i++)
				{
					XmlNode e = newChild.ChildNodes[i] as XmlNode;
					FOwnerDocument.onNodeInserting(e, this);
					_childNodes.data.Insert(refNodeIndex, newChild.ChildNodes[i]);
					e.setParent(this);
					FOwnerDocument.onNodeInserted(newChild, this);
					refNodeIndex ++;
				}
			}
			else
			{
				FOwnerDocument.onNodeInserting(newChild, this);
				_childNodes.data.Insert(refNodeIndex, newChild);
				newChild.setParent(this);
				FOwnerDocument.onNodeInserted(newChild, this);
			}

			return retval;
		}
		
		/// <summary>
		/// Insert newChild directly before the reference node.
		/// If refChild is null, newChild is inserted at the end of childnodes.
		/// If newChild is a document fragment, all nodes are inserted before refChild.
		/// If newChild is already in the tree, it is first removed.
		/// </summary>
		/// <exception cref="ArgumentException">NewChild was created from different document.
		/// RefChild not a child of this node, or is null.
		/// Node is read-only</exception>
		/// <exception cref="InvalidOperationException">Node is of type that does not have children.
		/// Node to insert is an ancestor of this node.</exception>
		/// <param name="newChild">Child node to insert.</param>
		/// <param name="refChild">Reference node to insert after</param>
		/// <returns>Removed node, or null if no node removed.</returns>
		public virtual XmlNode InsertBefore(XmlNode newChild, XmlNode refChild)
		{
			// Checks parent not ancestor, arguments valid, etc.  Throws exception on error
			InsertionCheck(newChild, refChild);

			// Scan the node list, looking for refChild and seeing if newChild is in the list
			XmlNode retval = null;
			int refNodeIndex = -1;
			
			for (int i = 0; i < _childNodes.Count; i++)
			{
				XmlNode e = _childNodes.data[i] as XmlNode;
				if (e.Equals(newChild))
				{
					retval = e;
					FOwnerDocument.onNodeRemoving(newChild, newChild.ParentNode);
					_childNodes.data.RemoveAt(i);
					newChild.setParent(null);
					FOwnerDocument.onNodeRemoved(newChild, null);
					break;
				}

				if ( (refChild != null ) & ( e.Equals(refChild) ) )
				{
					refNodeIndex = i;

					if (retval != null)
						break;
				}
			}

			if ( ( refNodeIndex == -1 ) & (refChild != null) )
				throw new ArgumentException("Reference node not found (and not null) in call to XmlNode.InsertAfter()");

			if (refChild == null)
				refNodeIndex = _childNodes.Count;

			if (newChild.NodeType == XmlNodeType.DocumentFragment)
			{
				// Insert all children, starting from refNodeIndex (0,1,2...n)
				for (int i = 0; i < newChild.ChildNodes.Count; i++)
				{
					XmlNode e = newChild.ChildNodes[i] as XmlNode;
					FOwnerDocument.onNodeInserting(e, this);
					_childNodes.data.Insert(refNodeIndex, newChild.ChildNodes[i]);
					e.setParent(this);
					FOwnerDocument.onNodeInserted(newChild, this);
					refNodeIndex ++;
				}
			}
			else
			{
				newChild.OwnerDocument.onNodeInserting(newChild, this);
				_childNodes.data.Insert(refNodeIndex, newChild);
				newChild.setParent(this);
				newChild.OwnerDocument.onNodeInserted(newChild, this);
			}

			return retval;
		}

		/// <summary>
		/// Put all nodes under this node in "normal" form
		/// Whatever that means...
		/// </summary>
		public virtual void Normalize()
		{
			// TODO - Implement Normalize()
			throw new NotImplementedException();
		}

		/// <summary>
		/// Add the specified child to the beginning of the child node list
		/// </summary>
		/// <param name="newChild">Node to add</param>
		/// <returns>The node added</returns>
		public virtual XmlNode PrependChild(XmlNode newChild)
		{
			return InsertAfter(newChild, null);
		}

		/// <summary>
		/// Remove all children and attributes
		/// </summary>
		public virtual void RemoveAll()
		{
			if (_childNodes == null)
				return;
			else
			{
				// Remove in order, 0..n
				while (_childNodes.Count > 0)
				{
					XmlNode e = _childNodes[0];
					FOwnerDocument.onNodeRemoving(e, this);
					e.setParent(null);
					_childNodes.data.RemoveAt(0);
					FOwnerDocument.onNodeRemoved(e, null);
				}
			}
		}

		/// <summary>
		/// Remove specified child node
		/// </summary>
		/// <param name="oldChild"></param>
		/// <returns>Removed node</returns>
		public virtual XmlNode RemoveChild(XmlNode oldChild)
		{
			// TODO - implement RemoveChild(oldChild)
			throw new NotImplementedException();
		}

		/// <summary>
		/// Select a list of nodes matching the xpath
		/// </summary>
		/// <param name="xpath"></param>
		/// <returns>matching nodes</returns>
		public XmlNodeList SelectNodes( string xpath)
		{
			// TODO - imlement SelectNodes(xpath)
			throw new NotImplementedException();
		}

		/// <summary>
		/// Select a list of nodes matching the xpath.  Any prefixes are resolved
		/// using the passed namespace manager
		/// </summary>
		/// <param name="xpath"></param>
		/// <param name="nsmgr"></param>
		/// <returns></returns>
		public XmlNodeList SelectNodes(string xpath, XmlNamespaceManager nsmgr)
		{
			// TODO - implement SelectNodes(xpath, nsmgr)
			throw new NotImplementedException();
		}

		/// <summary>
		/// Selects the first node that matches xpath
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		public XmlNode SelectSingleNode(string xpatch)
		{
			// TODO - implement SelectSingeNode(xpath)
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the first node that matches xpath
		/// Uses the passed namespace manager to resolve namespace URI's
		/// </summary>
		/// <param name="xpath"></param>
		/// <param name="nsmgr"></param>
		/// <returns></returns>
		public XmlNode SelectSingleNode(string xpath, XmlNamespaceManager nsmgr)
		{
			// Implement SelectSingleNode(xpath, nsmgr)
			throw new NotImplementedException();
		}

		/// <summary>
		/// Tests if the DOM implementation supports the passed feature
		/// </summary>
		/// <param name="feature"></param>
		/// <param name="version"></param>
		/// <returns></returns>
		public virtual bool Supports(string feature, string version)
		{
			//TODO - implement Supports(feature, version)
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns a string representation of the current node and it's children
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			// TODO - implement ToString()
			throw new NotImplementedException();
		}

		/// <summary>
		/// Saves all children of the current node to the passed writer
		/// </summary>
		/// <param name="w"></param>
		public abstract void WriteContentTo(XmlWriter w);
		
		/// <summary>
		/// Saves the current node to writer w
		/// </summary>
		/// <param name="w"></param>
		public abstract void WriteTo(XmlWriter w);

		//======= Internal methods    ===============================================
		/// <summary>
		/// accessor {set;} for parentNode only visible internally.
		/// </summary>
		/// <param name="newParent">new parent node.</param>
		internal void setParent( XmlNode newParent)
		{
			_parent = newParent;
		}
		
		//======= Protected methods    ==============================================

		//======= Private Methods ===================================================
		/// <summary>
		/// Helper function to perform checks required before insrting a node.
		/// Throws applicable exceptions on error.
		/// </summary>
		/// <param name="newChild"></param>
		/// <param name="refChild"></param>
		private void InsertionCheck( XmlNode newChild, XmlNode refChild)
		{
			if (newChild == null)
				throw new ArgumentNullException("Null newNode passed to InsertAfter()");

			if (newChild.Equals(this))
				throw new ArgumentException("Cannot insert node onto itself");

			if (NodeType != XmlNodeType.Document && !FOwnerDocument.Equals( newChild.OwnerDocument) )
				throw new ArgumentException("Reference node has different owner document than this node");
		
			XmlDocument ownerDocument = newChild.OwnerDocument;

			if (ownerDocument.IsReadOnly )
				throw new ArgumentException("Operation not supported - tree is read-only");

			//Check that insert node is not in our path to the root
			XmlNode curParent = _parent;
			while ( (curParent != null) & (! ownerDocument.Equals(curParent) ))
			{
				if (curParent.Equals(newChild) )
					throw new ArgumentException("Cannot insert ancestor a node");
				curParent = curParent.ParentNode;
			}
		}

		// Constructors
		//===========================================================================
		//When we're first created, we won't know parent, etc.
		internal XmlNode( XmlDocument aOwnerDoc )
		{
			// Don't create childnodes object, since not all derived classes have children
			FOwnerDocument = aOwnerDoc;
		}

	}	// XmlNode
}	// using namespace System.Xml

		
