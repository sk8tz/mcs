//
// tree.cs: keeps a tree representation of the generated code
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;

namespace Mono.CSharp
{

	public interface ITreeDump {
		int  Dump (Tree tree, StreamWriter output);
		void ParseOptions (string options);
	}

	// <summary>
	//   
	//   We store here all the toplevel types that we have parsed,
	//   this is the root of all information we have parsed.
	// 
	// </summary>
	
	public class Tree {
		TypeContainer root_types;

		// <summary>
		//   Keeps track of all the types definied (classes, structs, ifaces, enums)
		// </summary>
		Hashtable decls;
		
		public Tree ()
		{
			root_types = new TypeContainer ();

			decls = new Hashtable ();
		}

		DoubleHash decl_ns_name = new DoubleHash ();
		
		public void RecordDecl (string name, DeclSpace ds)
		{
			if (decls.Contains (name)){
				DeclSpace other = (DeclSpace) decls [name];
				Report.SymbolRelatedToPreviousError (other.Location, other.GetSignatureForError ());

				Report.Error (
					101, ds.Location,
					"There is already a definition for `" + name + "'");
				return;
			}

			ds.RecordDecl ();

			int p = name.LastIndexOf ('.');
			if (p == -1)
				decl_ns_name.Insert ("", name, ds);
			else {
				decl_ns_name.Insert (name.Substring (0, p), name.Substring (p+1), ds);
			}

			decls.Add (name, ds);
		}

		public DeclSpace LookupByNamespace (string ns, string name)
		{
			object res;
			
			decl_ns_name.Lookup (ns, name, out res);
			return (DeclSpace) res;
		}
		
		//
		// FIXME: Why are we using Types?
		//
                public TypeContainer Types {
                        get {
                                return root_types;
                        }
                }

		public Hashtable Decls {
			get {
				return decls;
			}
		}
	}
}
