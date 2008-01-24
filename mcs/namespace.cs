//
// namespace.cs: Tracks namespaces
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@seznam.cz)
//
// (C) 2001 Ximian, Inc.
//
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

namespace Mono.CSharp {

	public class RootNamespace : Namespace {
		static MethodInfo get_namespaces_method;

		string alias_name;
		Assembly referenced_assembly;

		Hashtable all_namespaces;

		static Hashtable root_namespaces;
		public static GlobalRootNamespace Global;
		
		static RootNamespace ()
		{
			get_namespaces_method = typeof (Assembly).GetMethod ("GetNamespaces", BindingFlags.Instance | BindingFlags.NonPublic);

			Reset ();
		}

		public static void Reset ()
		{
			root_namespaces = new Hashtable ();
			Global = new GlobalRootNamespace ();
			root_namespaces ["global"] = Global;
		}

		protected RootNamespace (string alias_name, Assembly assembly)
			: base (null, String.Empty)
		{
			this.alias_name = alias_name;
			referenced_assembly = assembly;

			all_namespaces = new Hashtable ();
			all_namespaces.Add ("", this);

			if (referenced_assembly != null)
				ComputeNamespaces (this.referenced_assembly);
		}

		public static void DefineRootNamespace (string name, Assembly assembly)
		{
			if (name == "global") {
				NamespaceEntry.Error_GlobalNamespaceRedefined (Location.Null);
				return;
			}
			RootNamespace retval = GetRootNamespace (name);
			if (retval == null || retval.referenced_assembly != assembly)
				root_namespaces [name] = new RootNamespace (name, assembly);
		}

		public static RootNamespace GetRootNamespace (string name)
		{
			return (RootNamespace) root_namespaces [name];
		}

		public virtual Type LookupTypeReflection (string name, Location loc)
		{
			return GetTypeInAssembly (referenced_assembly, name);
		}

		public void RegisterNamespace (Namespace child)
		{
			if (child != this)
				all_namespaces.Add (child.Name, child);
		}

		public bool IsNamespace (string name)
		{
			return all_namespaces.Contains (name);
		}

		protected void RegisterNamespace (string dotted_name)
		{
			if (dotted_name != null && dotted_name.Length != 0 && ! IsNamespace (dotted_name))
				GetNamespace (dotted_name, true);
		}

		void RegisterExtensionMethodClass (Type t)
 		{
			string n = t.Namespace;
 			Namespace ns = n == null ? Global : (Namespace)all_namespaces [n];
 			if (ns == null)
 				ns = GetNamespace (n, true);
 
 			ns.RegisterExternalExtensionMethodClass (t);
 		}

  		protected void ComputeNamespaces (Assembly assembly)
  		{
			// How to test whether attribute exists without loading the assembly :-(
#if NET_2_1
			const string SystemCore = "System.Core, Version=2.1.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"; 
#else
			const string SystemCore = "System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"; 
#endif
 			if (TypeManager.extension_attribute_type == null &&
 				assembly.FullName == SystemCore) {
 				TypeManager.extension_attribute_type = assembly.GetType("System.Runtime.CompilerServices.ExtensionAttribute");
 			}
 
 			bool contains_extension_methods = TypeManager.extension_attribute_type != null &&
 					assembly.IsDefined(TypeManager.extension_attribute_type, false);
 
 			if (get_namespaces_method != null && !contains_extension_methods) {
  				string [] namespaces = (string []) get_namespaces_method.Invoke (assembly, null);
  				foreach (string ns in namespaces)
 					RegisterNamespace (ns);
  				return;
  			}
  
 			foreach (Type t in assembly.GetExportedTypes ()) {
 				if ((t.Attributes & Class.StaticClassAttribute) == Class.StaticClassAttribute &&
 					contains_extension_methods && t.IsDefined (TypeManager.extension_attribute_type, false))
 					RegisterExtensionMethodClass (t);
 				else
 					RegisterNamespace (t.Namespace);
 			}
  		}

		protected static Type GetTypeInAssembly (Assembly assembly, string name)
		{
			Type t = assembly.GetType (name);
			if (t == null)
				return null;

			if (t.IsPointer)
				throw new InternalErrorException ("Use GetPointerType() to get a pointer");

			TypeAttributes ta = t.Attributes & TypeAttributes.VisibilityMask;
			if (ta == TypeAttributes.NestedPrivate)
				return null;

			if ((ta == TypeAttributes.NotPublic ||
			     ta == TypeAttributes.NestedAssembly ||
			     ta == TypeAttributes.NestedFamANDAssem) &&
			    !TypeManager.IsFriendAssembly (t.Assembly))
				return null;

			return t;
		}

		public override string ToString ()
		{
			return String.Format ("RootNamespace ({0}::)", alias_name);
		}

		public override string GetSignatureForError ()
		{
			return alias_name + "::";
		}
	}

	public class GlobalRootNamespace : RootNamespace {
		Assembly [] assemblies;
		Module [] modules;

		public GlobalRootNamespace ()
			: base ("global", null)
		{
			assemblies = new Assembly [0];
		}

		public Assembly [] Assemblies {
			get { return assemblies; }
		}

		public Module [] Modules {
			get { return modules; }
		}

		public void AddAssemblyReference (Assembly a)
		{
			foreach (Assembly assembly in assemblies) {
				if (a == assembly)
					return;
			}

			int top = assemblies.Length;
			Assembly [] n = new Assembly [top + 1];
			assemblies.CopyTo (n, 0);
			n [top] = a;
			assemblies = n;

			ComputeNamespaces (a);
		}

		public void AddModuleReference (Module m)
		{
			int top = modules != null ? modules.Length : 0;
			Module [] n = new Module [top + 1];
			if (modules != null)
				modules.CopyTo (n, 0);
			n [top] = m;
			modules = n;

			if (m == CodeGen.Module.Builder)
				return;

			foreach (Type t in m.GetTypes ())
				RegisterNamespace (t.Namespace);
		}

		public override void Error_NamespaceDoesNotExist(DeclSpace ds, Location loc, string name)
		{
			Report.Error (400, loc, "The type or namespace name `{0}' could not be found in the global namespace (are you missing an assembly reference?)",
				name);
		}

		public override Type LookupTypeReflection (string name, Location loc)
		{
			Type found_type = null;
		
			foreach (Assembly a in assemblies) {
				Type t = GetTypeInAssembly (a, name);
				if (t == null)
					continue;
					
				if (found_type == null) {
					found_type = t;
					continue;
				}

				Report.SymbolRelatedToPreviousError (found_type);
				Report.SymbolRelatedToPreviousError (t);
				Report.Error (433, loc, "The imported type `{0}' is defined multiple times", name);
					
				return found_type;
			}

			if (modules != null) {
				foreach (Module module in modules) {
					Type t = module.GetType (name);
					if (t == null)
						continue;

					if (found_type == null) {
						found_type = t;
						continue;
					}

					Report.SymbolRelatedToPreviousError (found_type);
					if (loc.IsNull) {
						DeclSpace ds = TypeManager.LookupDeclSpace (t);
						Report.Warning (1685, 1, ds.Location, "The type `{0}' conflicts with the predefined type `{1}' and will be ignored",
							ds.GetSignatureForError (), TypeManager.CSharpName (found_type));
						return found_type;
					}
					Report.SymbolRelatedToPreviousError (t);
					Report.Warning (436, 2, loc, "The type `{0}' conflicts with the imported type `{1}'. Ignoring the imported type definition",
						TypeManager.CSharpName (t), TypeManager.CSharpName (found_type));
					return t;
				}
			}

			return found_type;
		}
	}

	/// <summary>
	///   Keeps track of the namespaces defined in the C# code.
	///
	///   This is an Expression to allow it to be referenced in the
	///   compiler parse/intermediate tree during name resolution.
	/// </summary>
	public class Namespace : FullNamedExpression {
		
		Namespace parent;
		string fullname;
		IDictionary namespaces;
		IDictionary declspaces;
		Hashtable cached_types;
		RootNamespace root;
		ArrayList external_exmethod_classes;

		public readonly MemberName MemberName;

		/// <summary>
		///   Constructor Takes the current namespace and the
		///   name.  This is bootstrapped with parent == null
		///   and name = ""
		/// </summary>
		public Namespace (Namespace parent, string name)
		{
			// Expression members.
			this.eclass = ExprClass.Namespace;
			this.Type = typeof (Namespace);
			this.loc = Location.Null;

			this.parent = parent;

			if (parent != null)
				this.root = parent.root;
			else
				this.root = this as RootNamespace;

			if (this.root == null)
				throw new InternalErrorException ("Root namespaces must be created using RootNamespace");
			
			string pname = parent != null ? parent.Name : "";
				
			if (pname == "")
				fullname = name;
			else
				fullname = parent.Name + "." + name;

			if (fullname == null)
				throw new InternalErrorException ("Namespace has a null fullname");

			if (parent != null && parent.MemberName != MemberName.Null)
				MemberName = new MemberName (parent.MemberName, name);
			else if (name.Length == 0)
				MemberName = MemberName.Null;
			else
				MemberName = new MemberName (name);

			namespaces = new HybridDictionary ();
			cached_types = new Hashtable ();

			root.RegisterNamespace (this);
		}

		public override Expression DoResolve (EmitContext ec)
		{
			return this;
		}

		public virtual void Error_NamespaceDoesNotExist (DeclSpace ds, Location loc, string name)
		{
			if (name.IndexOf ('`') > 0) {
				FullNamedExpression retval = Lookup (ds, SimpleName.RemoveGenericArity (name), loc);
				if (retval != null) {
					Error_TypeArgumentsCannotBeUsed (retval.Type, loc);
					return;
				}
			} else {
				Type t = LookForAnyGenericType (name);
				if (t != null) {
					Error_InvalidNumberOfTypeArguments (t, loc);
					return;
				}
			}

			Report.Error (234, loc, "The type or namespace name `{0}' does not exist in the namespace `{1}'. Are you missing an assembly reference?",
				name, FullName);
		}

		public static void Error_InvalidNumberOfTypeArguments (Type t, Location loc)
		{
			Report.SymbolRelatedToPreviousError (t);
			Report.Error (305, loc, "Using the generic type `{0}' requires `{1}' type argument(s)",
				TypeManager.CSharpName(t), TypeManager.GetNumberOfTypeArguments(t).ToString());
		}

		public static void Error_TypeArgumentsCannotBeUsed (Type t, Location loc)
		{
			Report.SymbolRelatedToPreviousError (t);
			Error_TypeArgumentsCannotBeUsed (loc, "type", TypeManager.CSharpName (t));
		}

		public static void Error_TypeArgumentsCannotBeUsed (MethodBase mi, Location loc)
		{
			Report.SymbolRelatedToPreviousError (mi);
			Error_TypeArgumentsCannotBeUsed (loc, "method", TypeManager.CSharpSignature (mi));
		}

		static void Error_TypeArgumentsCannotBeUsed (Location loc, string type, string name)
		{
			Report.Error(308, loc, "The non-generic {0} `{1}' cannot be used with the type arguments",
				type, name);
		}

		public override void Emit (EmitContext ec)
		{
			throw new InternalErrorException ("Expression tree referenced namespace " + fullname + " during Emit ()");
		}

		public override string GetSignatureForError ()
		{
			return Name;
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
			string lookup = t != null ? t.FullName : (fullname.Length == 0 ? name : fullname + "." + name);
			Type rt = root.LookupTypeReflection (lookup, loc);

			// HACK: loc.IsNull when the type is core type
			if (t == null || (rt != null && loc.IsNull))
				t = rt;

			TypeExpr te = t == null ? null : new TypeExpression (t, Location.Null);
			cached_types [name] = te;
			return te;
		}

		///
		/// Used for better error reporting only
		/// 
		public Type LookForAnyGenericType (string typeName)
		{
			if (declspaces == null)
				return null;

			typeName = SimpleName.RemoveGenericArity (typeName);

			foreach (DictionaryEntry de in declspaces) {
				string type_item = (string) de.Key;
				int pos = type_item.LastIndexOf ('`');
				if (pos == typeName.Length && String.Compare (typeName, 0, type_item, 0, pos) == 0)
					return ((DeclSpace) de.Value).TypeBuilder;
			}
			return null;
		}

		public FullNamedExpression Lookup (DeclSpace ds, string name, Location loc)
		{
			if (namespaces.Contains (name))
				return (Namespace) namespaces [name];

			return LookupType (name, loc);
		}

		public void RegisterExternalExtensionMethodClass (Type type)
		{
			if (external_exmethod_classes == null)
				external_exmethod_classes = new ArrayList ();

			external_exmethod_classes.Add (type);
		}

		/// 
		/// Looks for extension method in this namespace
		/// 
		public ArrayList LookupExtensionMethod (Type extensionType, ClassOrStruct currentClass, string name, NamespaceEntry ns)
		{
			ArrayList found = null;

			if (declspaces != null) {
				IEnumerator e = declspaces.Values.GetEnumerator ();
				e.Reset ();
				while (e.MoveNext ()) {
					Class c = e.Current as Class;
					if (c == null)
						continue;

					if (!c.IsStaticClass)
						continue;

					ArrayList res = c.MemberCache.FindExtensionMethods (extensionType, name, c != currentClass);
					if (res == null)
						continue;

					if (found == null)
						found = res;
					else
						found.AddRange (res);
				}
			}

			if (external_exmethod_classes == null)
				return found;

			foreach (Type t in external_exmethod_classes) {
				MemberCache m = TypeHandle.GetMemberCache (t);
				ArrayList res = m.FindExtensionMethods (extensionType, name, true);
				if (res == null)
					continue;

				if (found == null)
					found = res;
				else
					found.AddRange (res);
			}

			return found;
		}

		public void AddDeclSpace (string name, DeclSpace ds)
		{
			if (declspaces == null)
				declspaces = new HybridDictionary ();
			declspaces.Add (name, ds);
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

		public override string ToString ()
		{
			return String.Format ("Namespace ({0})", Name);
		}
	}

	public class NamespaceEntry {
		Namespace ns;
		NamespaceEntry parent, implicit_parent;
		SourceFile file;
		int symfile_id;
		Hashtable aliases;
		ArrayList using_clauses;
		public bool DeclarationFound = false;
		bool UsingFound;

		public readonly DeclSpace SlaveDeclSpace;

		ListDictionary extern_aliases;

		static ArrayList entries = new ArrayList ();

		public static void Reset ()
		{
			entries = new ArrayList ();
		}

		//
		// This class holds the location where a using definition is
		// done, and whether it has been used by the program or not.
		//
		// We use this to flag using clauses for namespaces that do not
		// exist.
		//
		public class UsingEntry : IResolveContext {
			public readonly MemberName Name;
			readonly NamespaceEntry NamespaceEntry;
			readonly Location Location;
			
			public UsingEntry (NamespaceEntry entry, MemberName name, Location loc)
			{
				Name = name;
				NamespaceEntry = entry;
				Location = loc;
			}

			internal Namespace resolved;

			public Namespace Resolve ()
			{
				if (resolved != null)
					return resolved;

				FullNamedExpression fne = Name.GetTypeExpression ().ResolveAsTypeStep (this, false);
				if (fne == null)
					return null;

				resolved = fne as Namespace;
				if (resolved == null) {
					Report.SymbolRelatedToPreviousError (fne.Type);
					Report.Error (138, Location,
						"`{0}' is a type not a namespace. A using namespace directive can only be applied to namespaces", Name.GetSignatureForError ());
				}
				return resolved;
			}

			DeclSpace IResolveContext.DeclContainer {
				get { return NamespaceEntry.SlaveDeclSpace; }
			}

			DeclSpace IResolveContext.GenericDeclContainer {
				get { return NamespaceEntry.SlaveDeclSpace; }
			}

			bool IResolveContext.IsInObsoleteScope {
				get { return false; }
			}
			bool IResolveContext.IsInUnsafeScope {
				get { return false; }
			}
		}

		public abstract class AliasEntry {
			public readonly string Name;
			public readonly NamespaceEntry NamespaceEntry;
			public readonly Location Location;
			
			protected AliasEntry (NamespaceEntry entry, string name, Location loc)
			{
				Name = name;
				NamespaceEntry = entry;
				Location = loc;
			}
			
			protected FullNamedExpression resolved;
			bool error;

			public FullNamedExpression Resolve ()
			{
				if (resolved != null || error)
					return resolved;
				resolved = DoResolve ();
				if (resolved == null)
					error = true;
				return resolved;
			}

			protected abstract FullNamedExpression DoResolve ();
		}

		public class LocalAliasEntry : AliasEntry, IResolveContext {
			public readonly Expression Alias;
			
			public LocalAliasEntry (NamespaceEntry entry, string name, MemberName alias, Location loc) :
				base (entry, name, loc)
			{
				Alias = alias.GetTypeExpression ();
			}

			protected override FullNamedExpression DoResolve ()
			{
				resolved = Alias.ResolveAsTypeStep (this, false);
				if (resolved == null)
					return null;

				if (resolved.Type != null) {
					TypeAttributes attr = resolved.Type.Attributes & TypeAttributes.VisibilityMask;
					if (attr == TypeAttributes.NestedPrivate || attr == TypeAttributes.NestedFamily ||
					 	((attr == TypeAttributes.NestedFamORAssem || attr == TypeAttributes.NestedAssembly) && 
						TypeManager.LookupDeclSpace (resolved.Type) == null)) {
						Expression.ErrorIsInaccesible (Alias.Location, Alias.ToString ());
						return null;
					}
				}

				return resolved;
			}

			DeclSpace IResolveContext.DeclContainer {
				get { return NamespaceEntry.SlaveDeclSpace; }
			}

			DeclSpace IResolveContext.GenericDeclContainer {
				get { return NamespaceEntry.SlaveDeclSpace; }
			}

			bool IResolveContext.IsInObsoleteScope {
				get { return false; }
			}
			bool IResolveContext.IsInUnsafeScope {
				get { return false; }
			}
		}

		public class ExternAliasEntry : AliasEntry {
			public ExternAliasEntry (NamespaceEntry entry, string name, Location loc) :
				base (entry, name, loc)
			{
			}

			protected override FullNamedExpression DoResolve ()
			{
				resolved = RootNamespace.GetRootNamespace (Name);
				if (resolved == null)
					Report.Error (430, Location, "The extern alias '" + Name +
									"' was not specified in a /reference option");

				return resolved;
			}
		}

		public NamespaceEntry (NamespaceEntry parent, SourceFile file, string name)
		{
			this.parent = parent;
			this.file = file;
			entries.Add (this);
			this.ID = entries.Count;

			if (parent != null)
				ns = parent.NS.GetNamespace (name, true);
			else if (name != null)
				ns = RootNamespace.Global.GetNamespace (name, true);
			else
				ns = RootNamespace.Global;
			SlaveDeclSpace = new RootDeclSpace (this);
		}

		private NamespaceEntry (NamespaceEntry parent, SourceFile file, Namespace ns, bool slave)
		{
			this.parent = parent;
			this.file = file;
			// no need to add self to 'entries', since we don't have any aliases or using entries.
			this.ID = -1;
			this.IsImplicit = true;
			this.ns = ns;
			this.SlaveDeclSpace = slave ? new RootDeclSpace (this) : null;
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
					doppelganger = new NamespaceEntry (ImplicitParent, file, ns, true);
				return doppelganger;
			}
		}

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
						: new NamespaceEntry (parent, file, ns.Parent, false);
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

			UsingFound = true;

			if (name.Equals (ns.MemberName))
				return;
			
			if (using_clauses == null)
				using_clauses = new ArrayList ();

			foreach (UsingEntry old_entry in using_clauses) {
				if (name.Equals (old_entry.Name)) {
					Report.Warning (105, 3, loc, "The using directive for `{0}' appeared previously in this namespace", name.GetName ());
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

			UsingFound = true;

			if (aliases == null)
				aliases = new Hashtable ();

			if (aliases.Contains (name)) {
				AliasEntry ae = (AliasEntry) aliases [name];
				Report.SymbolRelatedToPreviousError (ae.Location, ae.Name);
				Report.Error (1537, loc, "The using alias `{0}' appeared previously in this namespace", name);
				return;
			}

			if (RootContext.Version != LanguageVersion.ISO_1 && name == "global")
				Report.Warning (440, 2, loc, "An alias named `global' will not be used when resolving 'global::';" +
					" the global namespace will be used instead");

			// FIXME: get correct error number.  See if the above check can be merged
			if (extern_aliases != null && extern_aliases.Contains (name)) {
				AliasEntry ae = (AliasEntry) extern_aliases [name];
				Report.SymbolRelatedToPreviousError (ae.Location, ae.Name);
				Report.Error (1537, loc, "The using alias `{0}' appeared previously in this namespace", name);
				return;
			}

			aliases [name] = new LocalAliasEntry (Doppelganger, name, alias, loc);
		}

		public void UsingExternalAlias (string name, Location loc)
		{
			if (UsingFound || DeclarationFound) {
				Report.Error (439, loc, "An extern alias declaration must precede all other elements");
				return;
			}
			
			// Share the extern_aliases field with the Doppelganger
			if (extern_aliases == null) {
				extern_aliases = new ListDictionary ();
				Doppelganger.extern_aliases = extern_aliases;
			}

			if (extern_aliases.Contains (name)) {
				AliasEntry ae = (AliasEntry) extern_aliases [name];
				Report.SymbolRelatedToPreviousError (ae.Location, ae.Name);
				Report.Error (1537, loc, "The using alias `{0}' appeared previously in this namespace", name);
				return;
			}

			if (name == "global") {
				Error_GlobalNamespaceRedefined (loc);
				return;
			}

			// Register the alias in aliases and extern_aliases, since we need both of them
			// to keep things simple (different resolution scenarios)
			ExternAliasEntry alias = new ExternAliasEntry (Doppelganger, name, loc);
			extern_aliases [name] = alias;
		}

		///
		/// Does extension methods look up to find a method which matches name and extensionType.
		/// Search starts from this namespace and continues hierarchically up to top level.
		///
		public ExtensionMethodGroupExpr LookupExtensionMethod (Type extensionType, ClassOrStruct currentClass, string name)
		{
			ArrayList candidates = null;
			if (currentClass != null) {
				candidates = ns.LookupExtensionMethod (extensionType, currentClass, name, this);
				if (candidates != null)
					return new ExtensionMethodGroupExpr (candidates, this, extensionType, Location.Null);
			}

			foreach (Namespace n in GetUsingTable ()) {
				ArrayList a = n.LookupExtensionMethod (extensionType, null, name, this);
				if (a == null)
					continue;

				if (candidates == null)
					candidates = a;
				else
					candidates.AddRange (a);
			}

			if (candidates != null)
				return new ExtensionMethodGroupExpr (candidates, parent, extensionType, Location.Null);

			if (parent == null)
				return null;

			return parent.LookupExtensionMethod (extensionType, currentClass, name);
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
			Report.SymbolRelatedToPreviousError (t1.Type);
			Report.SymbolRelatedToPreviousError (t2.Type);
			Report.Error (104, loc, "`{0}' is an ambiguous reference between `{1}' and `{2}'",
				name, t1.FullName, t2.FullName);
		}

		// Looks-up a alias named @name in this and surrounding namespace declarations
		public FullNamedExpression LookupAlias (string name)
		{
			AliasEntry entry = null;
			for (NamespaceEntry n = this; n != null; n = n.ImplicitParent) {
				if (n.extern_aliases != null && (entry = n.extern_aliases [name] as AliasEntry) != null)
					break;
				if (n.aliases != null && (entry = n.aliases [name] as AliasEntry) != null)
					break;
			}
			return entry == null ? null : entry.Resolve ();
		}

		private FullNamedExpression Lookup (DeclSpace ds, string name, Location loc, bool ignore_cs0104)
		{
			//
			// Check whether it's in the namespace.
			//
			FullNamedExpression fne = ns.Lookup (ds, name, loc);
			if (fne != null)
				return fne;

			if (extern_aliases != null) {
				AliasEntry entry = extern_aliases [name] as AliasEntry;
				if (entry != null)
					return entry.Resolve ();
			}
			
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
		static readonly Namespace [] empty_namespaces = new Namespace [0];
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

		static readonly string [] empty_using_list = new string [0];

		public int SymbolFileID {
			get {
				if (symfile_id == 0 && file.SourceFileEntry != null) {
					int parent_id = parent == null ? 0 : parent.SymbolFileID;

					string [] using_list = empty_using_list;
					if (using_clauses != null) {
						using_list = new string [using_clauses.Count];
						for (int i = 0; i < using_clauses.Count; i++)
							using_list [i] = ((UsingEntry) using_clauses [i]).Name.ToString ();
					}

					symfile_id = CodeGen.SymbolWriter.DefineNamespace (ns.Name, file.SourceFileEntry, using_list, parent_id);
				}
				return symfile_id;
			}
		}

		static void MsgtryRef (string s)
		{
			Console.WriteLine ("    Try using -r:" + s);
		}

		static void MsgtryPkg (string s)
		{
			Console.WriteLine ("    Try using -pkg:" + s);
		}

		public static void Error_GlobalNamespaceRedefined (Location loc)
		{
			Report.Error (1681, loc, "You cannot redefine the global extern alias");
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
		void VerifyUsing ()
		{
			if (extern_aliases != null) {
				foreach (DictionaryEntry de in extern_aliases)
					((AliasEntry) de.Value).Resolve ();
			}		

			if (using_clauses != null) {
				foreach (UsingEntry ue in using_clauses)
					ue.Resolve ();
			}

			if (aliases != null) {
				foreach (DictionaryEntry de in aliases)
					((AliasEntry) de.Value).Resolve ();
			}
		}

		/// <summary>
		///   Used to validate that all the using clauses are correct
		///   after we are finished parsing all the files.  
		/// </summary>
		static public void VerifyAllUsing ()
		{
			foreach (NamespaceEntry entry in entries)
				entry.VerifyUsing ();
		}

		public string GetSignatureForError ()
		{
			return ns.GetSignatureForError ();
		}

		public override string ToString ()
		{
			return ns.ToString ();
		}
	}
}
