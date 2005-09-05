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
using System.Collections.Specialized;

namespace Mono.CSharp {

	/// <summary>
	///   Keeps track of the namespaces defined in the C# code.
	///
	///   This is an Expression to allow it to be referenced in the
	///   compiler parse/intermediate tree during name resolution.
	/// </summary>
	public class Namespace : FullNamedExpression {
		static ArrayList all_namespaces;
		static Hashtable namespaces_map;
		
		Namespace parent;
		string fullname;
		ArrayList entries;
		Hashtable namespaces;
		IDictionary declspaces;
		Hashtable cached_types;

		public readonly MemberName MemberName;

		public static Namespace Root;

		static Namespace ()
		{
			Reset ();
		}

		public static void Reset ()
		{
			all_namespaces = new ArrayList ();
			namespaces_map = new Hashtable ();

			Root = new Namespace (null, "");
		}

		/// <summary>
		///   Constructor Takes the current namespace and the
		///   name.  This is bootstrapped with parent == null
		///   and name = ""
		/// </summary>
		public Namespace (Namespace parent, string name)
		{
			// Expression members.
			this.eclass = ExprClass.Namespace;
			this.Type = null;
			this.loc = Location.Null;

			this.parent = parent;

			string pname = parent != null ? parent.Name : "";
				
			if (pname == "")
				fullname = name;
			else
				fullname = parent.Name + "." + name;

			if (fullname == null)
				throw new InternalErrorException ("Namespace has a null fullname");

			if (parent != null && parent.MemberName != MemberName.Null)
				MemberName = new MemberName (
					parent.MemberName, name, parent.MemberName.Location);
			else if (name == "")
				MemberName = MemberName.Null;
			else
				MemberName = new MemberName (name, Location.Null);

			entries = new ArrayList ();
			namespaces = new Hashtable ();
			cached_types = new Hashtable ();

			all_namespaces.Add (this);
			if (namespaces_map.Contains (fullname))
				return;
			namespaces_map [fullname] = true;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			throw new InternalErrorException ("Expression tree referenced namespace " + fullname + " during Emit ()");
		}

		public static bool IsNamespace (string name)
		{
			return namespaces_map [name] != null;
		}

		public override string GetSignatureForError ()
		{
			return Name.Length == 0 ? "::global" : Name;
		}
		
		public Namespace GetNamespace (string name, bool create)
		{
			int pos = name.IndexOf ('.');

			Namespace ns;
			string first;
			if (pos >= 0)
				first = name.Substring (0, pos);
			else
				first = name;

			ns = (Namespace) namespaces [first];
			if (ns == null) {
				if (!create)
					return null;

				ns = new Namespace (this, first);
				namespaces.Add (first, ns);
			}

			if (pos >= 0)
				ns = ns.GetNamespace (name.Substring (pos + 1), create);

			return ns;
		}

		public static Namespace LookupNamespace (string name, bool create)
		{
			return Root.GetNamespace (name, create);
		}

		TypeExpr LookupType (string name, Location loc)
		{
			if (cached_types.Contains (name))
				return cached_types [name] as TypeExpr;

			Type t = null;
			if (declspaces != null) {
				DeclSpace tdecl = declspaces [name] as DeclSpace;
				if (tdecl != null) {
					//
					// Note that this is not:
					//
					//   t = tdecl.DefineType ()
					//
					// This is to make it somewhat more useful when a DefineType
					// fails due to problems in nested types (more useful in the sense
					// of fewer misleading error messages)
					//
					tdecl.DefineType ();
					t = tdecl.TypeBuilder;
				}
			}
			string lookup = t != null ? t.FullName : (fullname == "" ? name : fullname + "." + name);
			Type rt = TypeManager.LookupTypeReflection (lookup, loc);
			if (t == null)
				t = rt;

			TypeExpr te = t == null ? null : new TypeExpression (t, Location.Null);
			cached_types [name] = te;
			return te;
		}

		public FullNamedExpression Lookup (DeclSpace ds, string name, Location loc)
		{
			Namespace ns = GetNamespace (name, false);
			if (ns != null)
				return ns;

			TypeExpr te = LookupType (name, loc);
			if (te == null || !ds.CheckAccessLevel (te.Type))
				return null;

			return te;
		}

		public void AddNamespaceEntry (NamespaceEntry entry)
		{
			entries.Add (entry);
		}

		public void AddDeclSpace (string name, DeclSpace ds)
		{
			if (declspaces == null)
				declspaces = new HybridDictionary ();
			declspaces.Add (name, ds);
		}

		static public ArrayList UserDefinedNamespaces {
			get { return all_namespaces; }
		}

		/// <summary>
		///   The qualified name of the current namespace
		/// </summary>
		public string Name {
			get { return fullname; }
		}

		public override string FullName {
			get { return fullname; }
		}

		/// <summary>
		///   The parent of this namespace, used by the parser to "Pop"
		///   the current namespace declaration
		/// </summary>
		public Namespace Parent {
			get { return parent; }
		}

		public static void DefineNamespaces (SymbolWriter symwriter)
		{
			foreach (Namespace ns in all_namespaces) {
				foreach (NamespaceEntry entry in ns.entries)
					entry.DefineNamespace (symwriter);
			}
		}

		/// <summary>
		///   Used to validate that all the using clauses are correct
		///   after we are finished parsing all the files.  
		/// </summary>
		public static void VerifyUsing ()
		{
			foreach (Namespace ns in all_namespaces) {
				foreach (NamespaceEntry entry in ns.entries)
					entry.VerifyUsing ();
			}
		}

		public override string ToString ()
		{
			if (this == Root)
				return "Namespace (<root>)";
			else
				return String.Format ("Namespace ({0})", Name);
		}
	}

	public class NamespaceEntry
	{
		Namespace ns;
		NamespaceEntry parent, implicit_parent;
		SourceFile file;
		int symfile_id;
		Hashtable aliases;
		ArrayList using_clauses;
		public bool DeclarationFound = false;

		//
		// This class holds the location where a using definition is
		// done, and whether it has been used by the program or not.
		//
		// We use this to flag using clauses for namespaces that do not
		// exist.
		//
		public class UsingEntry {
			public readonly MemberName Name;
			readonly Expression Expr;
			readonly NamespaceEntry NamespaceEntry;
			readonly Location Location;
			
			public UsingEntry (NamespaceEntry entry, MemberName name, Location loc)
			{
				Name = name;
				Expr = name.GetTypeExpression ();
				NamespaceEntry = entry;
				Location = loc;
			}

			internal Namespace resolved;

			public Namespace Resolve ()
			{
				if (resolved != null)
					return resolved;

				DeclSpace root = RootContext.Tree.Types;
				root.NamespaceEntry = NamespaceEntry;
				FullNamedExpression fne = Expr.ResolveAsTypeStep (root.EmitContext, false);
				root.NamespaceEntry = null;

				if (fne == null) {
					Error_NamespaceNotFound (Location, Name.ToString ());
					return null;
				}

				resolved = fne as Namespace;
				if (resolved == null) {
					Report.Error (138, Location,
						"`{0} is a type not a namespace. A using namespace directive can only be applied to namespaces", Name.ToString ());
				}
				return resolved;
			}
		}

		public class AliasEntry {
			public readonly string Name;
			public readonly Expression Alias;
			public readonly NamespaceEntry NamespaceEntry;
			public readonly Location Location;
			
			public AliasEntry (NamespaceEntry entry, string name, MemberName alias, Location loc)
			{
				Name = name;
				Alias = alias.GetTypeExpression ();
				NamespaceEntry = entry;
				Location = loc;
			}

			FullNamedExpression resolved;

			public FullNamedExpression Resolve ()
			{
				if (resolved != null)
					return resolved;

				DeclSpace root = RootContext.Tree.Types;
				root.NamespaceEntry = NamespaceEntry;
				resolved = Alias.ResolveAsTypeStep (root.EmitContext, false);
				root.NamespaceEntry = null;

				return resolved;
			}
		}

		public NamespaceEntry (NamespaceEntry parent, SourceFile file, string name, Location loc)
		{
			this.parent = parent;
			this.file = file;
			this.IsImplicit = false;
			this.ID = ++next_id;

			if (parent != null)
				ns = parent.NS.GetNamespace (name, true);
			else if (name != null)
				ns = Namespace.LookupNamespace (name, true);
			else
				ns = Namespace.Root;
			ns.AddNamespaceEntry (this);
		}


		private NamespaceEntry (NamespaceEntry parent, SourceFile file, Namespace ns)
		{
			this.parent = parent;
			this.file = file;
			this.IsImplicit = true;
			this.ID = ++next_id;
			this.ns = ns;
		}

		//
		// According to section 16.3.1 (using-alias-directive), the namespace-or-type-name is
		// resolved as if the immediately containing namespace body has no using-directives.
		//
		// Section 16.3.2 says that the same rule is applied when resolving the namespace-name
		// in the using-namespace-directive.
		//
		// To implement these rules, the expressions in the using directives are resolved using 
		// the "doppelganger" (ghostly bodiless duplicate).
		//
		NamespaceEntry doppelganger;
		NamespaceEntry Doppelganger {
			get {
				if (!IsImplicit && doppelganger == null)
					doppelganger = new NamespaceEntry (ImplicitParent, file, ns);
				return doppelganger;
			}
		}

		static int next_id = 0;
		public readonly int ID;
		public readonly bool IsImplicit;

		public Namespace NS {
			get { return ns; }
		}

		public NamespaceEntry Parent {
			get { return parent; }
		}

		public NamespaceEntry ImplicitParent {
			get {
				if (parent == null)
					return null;
				if (implicit_parent == null) {
					implicit_parent = (parent.NS == ns.Parent)
						? parent
						: new NamespaceEntry (parent, file, ns.Parent);
				}
				return implicit_parent;
			}
		}

		/// <summary>
		///   Records a new namespace for resolving name references
		/// </summary>
		public void Using (MemberName name, Location loc)
		{
			if (DeclarationFound){
				Report.Error (1529, loc, "A using clause must precede all other namespace elements except extern alias declarations");
				return;
			}

			if (name.Equals (ns.MemberName))
				return;
			
			if (using_clauses == null)
				using_clauses = new ArrayList ();

			foreach (UsingEntry old_entry in using_clauses) {
				if (name.Equals (old_entry.Name)) {
					if (RootContext.WarningLevel >= 3)
						Report.Warning (105, loc, "The using directive for `{0}' appeared previously in this namespace", name);
						return;
					}
				}


			UsingEntry ue = new UsingEntry (Doppelganger, name, loc);
			using_clauses.Add (ue);
		}

		public void UsingAlias (string name, MemberName alias, Location loc)
		{
			if (DeclarationFound){
				Report.Error (1529, loc, "A using clause must precede all other namespace elements except extern alias declarations");
				return;
			}

			if (aliases == null)
				aliases = new Hashtable ();

			if (aliases.Contains (name)) {
				AliasEntry ae = (AliasEntry)aliases [name];
				Report.SymbolRelatedToPreviousError (ae.Location, ae.Name);
				Report.Error (1537, loc, "The using alias `" + name +
					      "' appeared previously in this namespace");
				return;
			}

			aliases [name] = new AliasEntry (Doppelganger, name, alias, loc);
		}

		public FullNamedExpression LookupNamespaceOrType (DeclSpace ds, string name, Location loc, bool ignore_cs0104)
		{
			// Precondition: Only simple names (no dots) will be looked up with this function.
			FullNamedExpression resolved = null;
			for (NamespaceEntry curr_ns = this; curr_ns != null; curr_ns = curr_ns.ImplicitParent) {
				if ((resolved = curr_ns.Lookup (ds, name, loc, ignore_cs0104)) != null)
					break;
			}
			return resolved;
		}

		static void Error_AmbiguousTypeReference (Location loc, string name, FullNamedExpression t1, FullNamedExpression t2)
		{
			Report.Error (104, loc, "`{0}' is an ambiguous reference between `{1}' and `{2}'",
				name, t1.FullName, t2.FullName);
		}

		private FullNamedExpression Lookup (DeclSpace ds, string name, Location loc, bool ignore_cs0104)
		{
			//
			// Check whether it's in the namespace.
			//
			FullNamedExpression fne = NS.Lookup (ds, name, loc);
			if (fne != null)
				return fne;

			if (IsImplicit)
				return null;

			//
			// Check aliases.
			//
			if (aliases != null) {
				AliasEntry entry = aliases [name] as AliasEntry;
				if (entry != null)
					return entry.Resolve ();
			}

			//
			// Check using entries.
			//
			FullNamedExpression match = null;
			foreach (Namespace using_ns in GetUsingTable ()) {
				match = using_ns.Lookup (ds, name, loc);
				if (match == null || !(match is TypeExpr))
					continue;
				if (fne != null) {
					if (!ignore_cs0104)
						Error_AmbiguousTypeReference (loc, name, fne, match);
					return null;
				}
				fne = match;
			}

			return fne;
		}

		// Our cached computation.
		readonly Namespace [] empty_namespaces = new Namespace [0];
		Namespace [] namespace_using_table;
		Namespace [] GetUsingTable ()
		{
			if (namespace_using_table != null)
				return namespace_using_table;

			if (using_clauses == null) {
				namespace_using_table = empty_namespaces;
				return namespace_using_table;
			}

			ArrayList list = new ArrayList (using_clauses.Count);

			foreach (UsingEntry ue in using_clauses) {
				Namespace using_ns = ue.Resolve ();
				if (using_ns == null)
					continue;

				list.Add (using_ns);
			}

			namespace_using_table = new Namespace [list.Count];
			list.CopyTo (namespace_using_table, 0);
			return namespace_using_table;
		}

		readonly string [] empty_using_list = new string [0];

		public void DefineNamespace (SymbolWriter symwriter)
		{
			if (symfile_id != 0)
				return;
			if (parent != null)
				parent.DefineNamespace (symwriter);

			string [] using_list = empty_using_list;
			if (using_clauses != null) {
				using_list = new string [using_clauses.Count];
				for (int i = 0; i < using_clauses.Count; i++)
					using_list [i] = ((UsingEntry) using_clauses [i]).Name.ToString ();
			} 

			int parent_id = parent != null ? parent.symfile_id : 0;
			if (file.SourceFileEntry == null)
				return;

			symfile_id = symwriter.DefineNamespace (
				ns.Name, file.SourceFileEntry, using_list, parent_id);
		}

		public int SymbolFileID {
			get { return symfile_id; }
		}

		static void MsgtryRef (string s)
		{
			Console.WriteLine ("    Try using -r:" + s);
		}

		static void MsgtryPkg (string s)
		{
			Console.WriteLine ("    Try using -pkg:" + s);
		}

		public static void Error_NamespaceNotFound (Location loc, string name)
		{
			Report.Error (246, loc, "The type or namespace name `{0}' could not be found. Are you missing a using directive or an assembly reference?",
				name);

			switch (name) {
			case "Gtk": case "GtkSharp":
				MsgtryPkg ("gtk-sharp");
				break;

			case "Gdk": case "GdkSharp":
				MsgtryPkg ("gdk-sharp");
				break;

			case "Glade": case "GladeSharp":
				MsgtryPkg ("glade-sharp");
				break;

			case "System.Drawing":
			case "System.Web.Services":
			case "System.Web":
			case "System.Data":
			case "System.Windows.Forms":
				MsgtryRef (name);
				break;
			}
		}

		/// <summary>
		///   Used to validate that all the using clauses are correct
		///   after we are finished parsing all the files.  
		/// </summary>
		public void VerifyUsing ()
		{
			if (using_clauses != null) {
				foreach (UsingEntry ue in using_clauses)
					ue.Resolve ();
			}

			if (aliases != null) {
				foreach (DictionaryEntry de in aliases) {
					AliasEntry alias = (AliasEntry) de.Value;
					if (alias.Resolve () == null)
						Error_NamespaceNotFound (alias.Location, alias.Alias.ToString ());
				}
			}
		}

		public string GetSignatureForError ()
		{
			if (NS == Namespace.Root)
				return "::global";
			else
				return ns.Name;
		}

		public override string ToString ()
		{
			if (NS == Namespace.Root)
				return "NamespaceEntry (<root>)";
			else
				return String.Format ("NamespaceEntry ({0},{1},{2})", ns.Name, IsImplicit, ID);
		}
	}
}
