//
// namespace.cs: Tracks namespaces
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//
using System;
using System.Collections;
using Mono.Languages;

namespace Mono.CSharp {

	/// <summary>
	///   Keeps track of the namespaces defined in the C# code.
	/// </summary>
	public class Namespace {
		static ArrayList all_namespaces = new ArrayList ();
		
		Namespace parent;
		string name;
		ArrayList using_clauses;
		Hashtable using_namespaces;
		Hashtable aliases;
		public bool DeclarationFound = false;

		//
		// This class holds the location where a using definition is
		// done, and whether it has been used by the program or not.
		//
		// We use this to flag using clauses for namespaces that do not
		// exist.
		//
		public class UsingEntry {
			public string Name;
			public bool Used;
			public Location Location;
			
			public UsingEntry (string name, Location loc)
			{
				Name = name;
				Location = loc;
				Used = false;
			}
		}
		
		/// <summary>
		///   Constructor Takes the current namespace and the
		///   name.  This is bootstrapped with parent == null
		///   and name = ""
		/// </summary>
		public Namespace (Namespace parent, string name)
		{
			this.name = name;
			this.parent = parent;

			all_namespaces.Add (this);
		}

		/// <summary>
		///   The qualified name of the current namespace
		/// </summary>
		public string Name {
			get {
				string pname = parent != null ? parent.Name : "";
				
				if (pname == "")
					return name;
				else
					return parent.Name + "." + name;
			}
		}

		/// <summary>
		///   The parent of this namespace, used by the parser to "Pop"
		///   the current namespace declaration
		/// </summary>
		public Namespace Parent {
			get {
				return parent;
			}
		}

		/// <summary>
		///   Records a new namespace for resolving name references
		/// </summary>
		public void Using (string ns, Location loc)
		{
			if (DeclarationFound){
				Report.Error (1529, loc, "A using clause must precede all other namespace elements");
				return;
			}

			if (using_clauses == null)
				using_clauses = new ArrayList ();

			if (using_namespaces == null)
				using_namespaces = new Hashtable ();

			if (using_namespaces.ContainsKey (ns)) {
				Report.Warning (105, loc, "The using directive for '" + ns +
					                  "' appeared previously in this namespace");
				return;
			}

			UsingEntry ue = new UsingEntry (ns, loc);
			using_clauses.Add (ue);
			using_namespaces.Add (ns, ns);
		}

		public ArrayList UsingTable {
			get {
				return using_clauses;
			}
		}

		public void UsingAlias (string alias, string namespace_or_type, Location loc)
		{
			if (aliases == null)
				aliases = new Hashtable ();
			
			if (aliases.Contains (alias)){
				Report.Error (1537, loc, "The using alias `" + alias +
					      "' appeared previously in this namespace");
				return;
			}
					
			aliases [alias] = namespace_or_type;
		}

		public string LookupAlias (string alias)
		{
			string value = null;

			// System.Console.WriteLine ("Lookup " + alias + " in " + name);

			if (aliases != null)
				value = (string) (aliases [alias]);
			if (value == null && Parent != null)
				value = Parent.LookupAlias (alias);

			return value;
		}

		/// <summary>
		///   Used to validate that all the using clauses are correct
		///   after we are finished parsing all the files.  
		/// </summary>
		public static bool VerifyUsing ()
		{
			ArrayList unused = new ArrayList ();
			int errors = 0;
			
			foreach (Namespace ns in all_namespaces){
				ArrayList uses = ns.UsingTable;
				if (uses == null)
					continue;
				
				foreach (UsingEntry ue in uses){
					if (ue.Used)
						continue;
					unused.Add (ue);
				}
			}

			//
			// If we have unused using aliases, load all namespaces and check
			// whether it is unused, or it was missing
			//
			if (unused.Count > 0){
				Hashtable namespaces = TypeManager.GetNamespaces ();

				foreach (UsingEntry ue in unused){
					if (namespaces.Contains (ue.Name)){
						Report.Warning (6024, ue.Location, "Unused namespace in `using' declaration");
						continue;
					}

					errors++;
					Report.Error (246, ue.Location, "The namespace `" + ue.Name +
						      "' can not be found (missing assembly reference?)");
				}
			}
			
			return errors == 0;
		}

	}
}
