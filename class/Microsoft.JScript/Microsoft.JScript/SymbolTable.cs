//
// SymbolTable.cs: 
//
// Author:
//	Cesar Lopez Nataren
//
// (C) 2003, Cesar Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;
using System.Collections;
using  System.Text;

namespace Microsoft.JScript {

	internal class SymbolTable {

		internal SymbolTable parent;
		internal Hashtable symbols;
		
		internal SymbolTable (SymbolTable parent)
		{
			symbols = new Hashtable ();
			this.parent = parent;
		}
		
		internal void Add (string id, object d)
		{
			symbols.Add (id, d);
		}

		internal object Contains (string id)
		{
			return symbols [id];
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			ICollection keys = symbols.Keys;

			foreach (object o in keys)
				sb.Append (o.ToString ());

			return sb.ToString ();
		}

		internal int size {
			get { return symbols.Count; }
		}

		internal DictionaryEntry [] current_symbols {
			get {
				int n = symbols.Count;
				if (n == 0)
					return null;
				else {
					DictionaryEntry [] e = new DictionaryEntry [symbols.Count];
					symbols.CopyTo (e, 0);
					return e;
				}
			}
		}
	}
}
