//
// XmlNamespaceManager.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2001 Jason Diamond  http://injektilo.org/
//

using System.Collections;

namespace System.Xml
{
	public class XmlNamespaceManager : IEnumerable
	{
		#region Fields

		private XmlNameTable nameTable;
		private NamespaceScope currentScope;

		#endregion

		#region Constructor

		public XmlNamespaceManager (XmlNameTable nameTable)
		{
			this.nameTable = nameTable;

			nameTable.Add ("xmlns");
			nameTable.Add ("xml");
			nameTable.Add (String.Empty);
			nameTable.Add ("http://www.w3.org/2000/xmlns/");
			nameTable.Add ("http://www.w3.org/XML/1998/namespace");

			PushScope ();
		}

		#endregion

		#region Properties

		public virtual string DefaultNamespace {
			get { return LookupNamespace (String.Empty); }
		}

		public XmlNameTable NameTable {
			get { return nameTable; }
		}

		#endregion

		#region Methods

		public virtual void AddNamespace (string prefix, string uri)
		{
			if (prefix == null)
				throw new ArgumentNullException ("prefix", "Value cannot be null.");

			if (uri == null)
				throw new ArgumentNullException ("uri", "Value cannot be null.");

			if (prefix.Length > 2 && prefix.Substring (0, 3).ToLower () == "xml")
				throw new ArgumentException ("Prefixes beginning with \"xml\" (regardless of whether the characters are uppercase, lowercase, or some combination thereof) are reserved for use by XML.", "prefix");

			if (currentScope.Namespaces == null)
				currentScope.Namespaces = new Hashtable ();

			currentScope.Namespaces.Add (nameTable.Add (prefix), nameTable.Add (uri));
		}

		[MonoTODO]
		public virtual IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		public virtual bool HasNamespace (string prefix)
		{
			return currentScope != null && currentScope.Namespaces != null && currentScope.Namespaces.Contains (prefix);
		}

		public virtual string LookupNamespace (string prefix)
		{
			NamespaceScope scope = currentScope;

			while (scope != null) {
				if (scope.Namespaces != null && scope.Namespaces.Contains (prefix))
					return scope.Namespaces[prefix] as string;
				scope = scope.Next;
			}

			switch (prefix) {
			case "xmlns":
				return nameTable.Get ("http://www.w3.org/2000/xmlns/");
			case "xml":
				return nameTable.Get ("http://www.w3.org/XML/1998/namespace");
			case "":
				return nameTable.Get (String.Empty);
			}

			return null;
		}

		[MonoTODO]
		public virtual string LookupPrefix (string uri)
		{
			throw new NotImplementedException ();
		}

		public virtual bool PopScope ()
		{
			if (currentScope != null)
				currentScope = currentScope.Next;

			return currentScope != null;
		}

		public virtual void PushScope ()
		{
			NamespaceScope newScope = new NamespaceScope ();
			newScope.Next = currentScope;
			currentScope = newScope;
		}

		[MonoTODO]
		public virtual void RemoveNamespace (string prefix, string uri)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}

	internal class NamespaceScope
	{
		internal NamespaceScope Next;
		internal Hashtable Namespaces;
	}
}
