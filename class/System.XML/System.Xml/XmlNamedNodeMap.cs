//
// System.Xml.XmlNamedNodeMap
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//   Duncan Mak  (duncan@ximian.com)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;
using System.Collections;

namespace System.Xml
{
	public class XmlNamedNodeMap : IEnumerable
	{
		XmlNode parent;
		ArrayList nodeList;
		bool readOnly = false;

		internal XmlNamedNodeMap (XmlNode parent)
		{
			this.parent = parent;
			nodeList = new ArrayList ();
		}

		public virtual int Count {
			get { return nodeList.Count; }
		}

		public virtual IEnumerator GetEnumerator () 
		{
			return nodeList.GetEnumerator ();
		}

		public virtual XmlNode GetNamedItem (string name)
		{
			foreach (XmlNode node in nodeList) {
				if (node.Name == name)
					return node;
			}
			return null;
		}

		public virtual XmlNode GetNamedItem (string localName, string namespaceURI)
		{
			foreach (XmlNode node in nodeList) {
				if ((node.LocalName == localName)
				    && (node.NamespaceURI == namespaceURI))
					return node;
			}

			return null;
		}
		
		public virtual XmlNode Item (int index)
		{
			if (index < 0 || index > nodeList.Count)
				return null;
			else
				return (XmlNode) nodeList [index];
		}

		public virtual XmlNode RemoveNamedItem (string name)
		{			
			foreach (XmlNode node in nodeList)
				if (node.Name == name) {
					nodeList.Remove (node);
					return node;
				}
			
			return null;
		}

		public virtual XmlNode RemoveNamedItem (string localName, string namespaceURI)
		{
			foreach (XmlNode node in nodeList)
				if ((node.LocalName == localName)
				    && (node.NamespaceURI == namespaceURI)) {
					nodeList.Remove (node);
					return node;
				}

			return null;
		}

		public virtual XmlNode SetNamedItem (XmlNode node)
		{
			if (readOnly || (node.OwnerDocument != parent.OwnerDocument))
				throw new ArgumentException ("Cannot add to NodeMap.");
						
			foreach (XmlNode x in nodeList)
				if (x.Name == node.Name) {
					nodeList.Remove (x);
					nodeList.Add (node);
					return x;
				}
			
			nodeList.Add (node);
			return null;
		}

		internal ArrayList Nodes { get { return nodeList; } }
	}
}
