//
// decl.cs: Declaration base class for structs, classes, enums and interfaces.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
// TODO: Move the method verification stuff from the class.cs and interface.cs here
//

using System;
using System.Collections;
using System.Reflection.Emit;
using System.Reflection;

namespace Mono.CSharp {

	/// <summary>
	///   Base representation for members.  This is only used to keep track
	///   of Name, Location and Modifier flags.
	/// </summary>
	public abstract class MemberCore {
		/// <summary>
		///   Public name
		/// </summary>
		public string Name;

		/// <summary>
		///   Modifier flags that the user specified in the source code
		/// </summary>
		public int ModFlags;

		/// <summary>
		///   Location where this declaration happens
		/// </summary>
		public readonly Location Location;

		public MemberCore (string name, Location loc)
		{
			Name = name;
			Location = loc;
		}

		public abstract bool Define (TypeContainer parent);

		// 
		// Whehter is it ok to use an unsafe pointer in this type container
		//
		public bool UnsafeOK (DeclSpace parent)
		{
			//
			// First check if this MemberCore modifier flags has unsafe set
			//
			if ((ModFlags & Modifiers.UNSAFE) != 0)
				return true;

			if (parent.UnsafeContext)
				return true;

			Expression.UnsafeError (Location);
			return false;
		}
	}

	/// <summary>
	///   Base class for structs, classes, enumerations and interfaces.  
	/// </summary>
	/// <remarks>
	///   They all create new declaration spaces.  This
	///   provides the common foundation for managing those name
	///   spaces.
	/// </remarks>
	public abstract class DeclSpace : MemberCore {
		/// <summary>
		///   this points to the actual definition that is being
		///   created with System.Reflection.Emit
		/// </summary>
		public TypeBuilder TypeBuilder;

		/// <summary>
		///   This variable tracks whether we have Closed the type
		/// </summary>
		public bool Created = false;
		
		//
		// This is the namespace in which this typecontainer
		// was declared.  We use this to resolve names.
		//
		public NamespaceEntry NamespaceEntry;

		public Hashtable Cache = new Hashtable ();
		
		public string Basename;
		
		/// <summary>
		///   defined_names is used for toplevel objects
		/// </summary>
		protected Hashtable defined_names;

		TypeContainer parent;		

		public DeclSpace (NamespaceEntry ns, TypeContainer parent, string name, Location l)
			: base (name, l)
		{
			NamespaceEntry = ns;
			Basename = name.Substring (1 + name.LastIndexOf ('.'));
			defined_names = new Hashtable ();
			this.parent = parent;
		}

		public void RecordDecl ()
		{
			if ((NamespaceEntry != null) && (parent == RootContext.Tree.Types))
				NamespaceEntry.DefineName (Basename, this);
		}

		/// <summary>
		///   The result value from adding an declaration into
		///   a struct or a class
		/// </summary>
		public enum AdditionResult {
			/// <summary>
			/// The declaration has been successfully
			/// added to the declation space.
			/// </summary>
			Success,

			/// <summary>
			///   The symbol has already been defined.
			/// </summary>
			NameExists,

			/// <summary>
			///   Returned if the declation being added to the
			///   name space clashes with its container name.
			///
			///   The only exceptions for this are constructors
			///   and static constructors
			/// </summary>
			EnclosingClash,

			/// <summary>
			///   Returned if a constructor was created (because syntactically
			///   it looked like a constructor) but was not (because the name
			///   of the method is not the same as the container class
			/// </summary>
			NotAConstructor,

			/// <summary>
			///   This is only used by static constructors to emit the
			///   error 111, but this error for other things really
			///   happens at another level for other functions.
			/// </summary>
			MethodExists,

			/// <summary>
			///   Some other error.
			/// </summary>
			Error
		}

		/// <summary>
		///   Returns a status code based purely on the name
		///   of the member being added
		/// </summary>
		protected AdditionResult IsValid (string basename, string name)
		{
			if (basename == Basename)
				return AdditionResult.EnclosingClash;

			if (defined_names.Contains (name))
				return AdditionResult.NameExists;

			return AdditionResult.Success;
		}

		public static int length;
		public static int small;
		
		/// <summary>
		///   Introduce @name into this declaration space and
		///   associates it with the object @o.  Note that for
		///   methods this will just point to the first method. o
		/// </summary>
		protected void DefineName (string name, object o)
		{
			defined_names.Add (name, o);

#if DEBUGME
			int p = name.LastIndexOf (".");
			int l = name.Length;
			length += l;
			small += l -p;
#endif
		}

		/// <summary>
		///   Returns the object associated with a given name in the declaration
		///   space.  This is the inverse operation of `DefineName'
		/// </summary>
		public object GetDefinition (string name)
		{
			return defined_names [name];
		}
		
		bool in_transit = false;
		
		/// <summary>
		///   This function is used to catch recursive definitions
		///   in declarations.
		/// </summary>
		public bool InTransit {
			get {
				return in_transit;
			}

			set {
				in_transit = value;
			}
		}

		public TypeContainer Parent {
			get {
				return parent;
			}
		}

		/// <summary>
		///   Looks up the alias for the name
		/// </summary>
		public string LookupAlias (string name)
		{
			if (NamespaceEntry != null)
				return NamespaceEntry.LookupAlias (name);
			else
				return null;
		}
		
		// 
		// root_types contains all the types.  All TopLevel types
		// hence have a parent that points to `root_types', that is
		// why there is a non-obvious test down here.
		//
		public bool IsTopLevel {
			get {
				if (parent != null){
					if (parent.parent == null)
						return true;
				}
				return false;
			}
		}

		public virtual void CloseType ()
		{
			if (!Created){
				try {
					TypeBuilder.CreateType ();
				} catch {
					//
					// The try/catch is needed because
					// nested enumerations fail to load when they
					// are defined.
					//
					// Even if this is the right order (enumerations
					// declared after types).
					//
					// Note that this still creates the type and
					// it is possible to save it
				}
				Created = true;
			}
		}

		/// <remarks>
		///  Should be overriten by the appropriate declaration space
		/// </remarks>
		public abstract TypeBuilder DefineType ();
		
		/// <summary>
		///   Define all members, but don't apply any attributes or do anything which may
		///   access not-yet-defined classes.  This method also creates the MemberCache.
		/// </summary>
		public abstract bool DefineMembers (TypeContainer parent);

		//
		// Whether this is an `unsafe context'
		//
		public bool UnsafeContext {
			get {
				if ((ModFlags & Modifiers.UNSAFE) != 0)
					return true;
				if (parent != null)
					return parent.UnsafeContext;
				return false;
			}
		}

		public static string MakeFQN (string nsn, string name)
		{
			if (nsn == "")
				return name;
			return String.Concat (nsn, ".", name);
		}

		EmitContext type_resolve_ec;
		EmitContext GetTypeResolveEmitContext (TypeContainer parent, Location loc)
		{
			type_resolve_ec = new EmitContext (parent, this, loc, null, null, ModFlags, false);
			type_resolve_ec.ResolvingTypeTree = true;

			return type_resolve_ec;
		}

		// <summary>
		//    Looks up the type, as parsed into the expression `e' 
		// </summary>
		public Type ResolveType (Expression e, bool silent, Location loc)
		{
			if (type_resolve_ec == null)
				type_resolve_ec = GetTypeResolveEmitContext (parent, loc);
			type_resolve_ec.loc = loc;
			type_resolve_ec.ContainerType = TypeBuilder;

			int errors = Report.Errors;
			Expression d = e.ResolveAsTypeTerminal (type_resolve_ec);
			
			if (d == null || d.eclass != ExprClass.Type){
				if (!silent && errors == Report.Errors){
					Report.Error (246, loc, "Cannot find type `"+ e.ToString () +"'");
				}
				return null;
			}

			if (!CheckAccessLevel (d.Type)) {
				Report.	Error (122, loc,  "`" + d.Type + "' " +
				       "is inaccessible because of its protection level");
				return null;
			}

			return d.Type;
		}

		// <summary>
		//    Resolves the expression `e' for a type, and will recursively define
		//    types. 
		// </summary>
		public Expression ResolveTypeExpr (Expression e, bool silent, Location loc)
		{
			if (type_resolve_ec == null)
				type_resolve_ec = GetTypeResolveEmitContext (parent, loc);
			type_resolve_ec.loc = loc;
			type_resolve_ec.ContainerType = TypeBuilder;

			Expression d = e.ResolveAsTypeTerminal (type_resolve_ec);
			 
			if (d == null || d.eclass != ExprClass.Type){
				if (!silent){
					Report.Error (246, loc, "Cannot find type `"+ e +"'");
				}
				return null;
			}

			return d;
		}
		
		public bool CheckAccessLevel (Type check_type) 
		{
			if (check_type == TypeBuilder)
				return true;
			
			TypeAttributes check_attr = check_type.Attributes & TypeAttributes.VisibilityMask;
			
			//
			// Broken Microsoft runtime, return public for arrays, no matter what 
			// the accessibility is for their underlying class, and they return 
			// NonPublic visibility for pointers
			//
			if (check_type.IsArray || check_type.IsPointer)
				return CheckAccessLevel (TypeManager.GetElementType (check_type));

			switch (check_attr){
			case TypeAttributes.Public:
				return true;

			case TypeAttributes.NotPublic:
				//
				// This test should probably use the declaringtype.
				//
				if (check_type.Assembly == TypeBuilder.Assembly){
					return true;
				}
				return false;
				
			case TypeAttributes.NestedPublic:
				return true;

			case TypeAttributes.NestedPrivate:
				string check_type_name = check_type.FullName;
				string type_name = TypeBuilder.FullName;
				
				int cio = check_type_name.LastIndexOf ("+");
				string container = check_type_name.Substring (0, cio);

				//
				// Check if the check_type is a nested class of the current type
				//
				if (check_type_name.StartsWith (type_name + "+")){
					return true;
				}
				
				if (type_name.StartsWith (container)){
					return true;
				}

				return false;

			case TypeAttributes.NestedFamily:
				//
				// Only accessible to methods in current type or any subtypes
				//
				return FamilyAccessible (check_type);

			case TypeAttributes.NestedFamANDAssem:
				return (check_type.Assembly == TypeBuilder.Assembly) &&
					FamilyAccessible (check_type);

			case TypeAttributes.NestedFamORAssem:
				return (check_type.Assembly == TypeBuilder.Assembly) ||
					FamilyAccessible (check_type);

			case TypeAttributes.NestedAssembly:
				return check_type.Assembly == TypeBuilder.Assembly;
			}

			Console.WriteLine ("HERE: " + check_attr);
			return false;

		}

		protected bool FamilyAccessible (Type check_type)
		{
			Type declaring = check_type.DeclaringType;
			if (TypeBuilder.IsSubclassOf (declaring))
				return true;

			string check_type_name = check_type.FullName;
			string type_name = TypeBuilder.FullName;
			
			int cio = check_type_name.LastIndexOf ("+");
			string container = check_type_name.Substring (0, cio);
			
			//
			// Check if the check_type is a nested class of the current type
			//
			if (check_type_name.StartsWith (container + "+"))
				return true;

			return false;
		}

		// Access level of a type.
		enum AccessLevel {
			Public			= 0,
			ProtectedInternal	= 1,
			Internal		= 2,
			Protected		= 3,
			Private			= 4
		}

		// Check whether `flags' denotes a more restricted access than `level'
		// and return the new level.
		static AccessLevel CheckAccessLevel (AccessLevel level, int flags)
		{
			AccessLevel old_level = level;

			if ((flags & Modifiers.INTERNAL) != 0) {
				if ((flags & Modifiers.PROTECTED) != 0) {
					if ((int) level < (int) AccessLevel.ProtectedInternal)
						level = AccessLevel.ProtectedInternal;
				} else {
					if ((int) level < (int) AccessLevel.Internal)
						level = AccessLevel.Internal;
				}
			} else if ((flags & Modifiers.PROTECTED) != 0) {
				if ((int) level < (int) AccessLevel.Protected)
					level = AccessLevel.Protected;
			} else if ((flags & Modifiers.PRIVATE) != 0)
				level = AccessLevel.Private;

			return level;
		}

		// Return the access level for a new member which is defined in the current
		// TypeContainer with access modifiers `flags'.
		AccessLevel GetAccessLevel (int flags)
		{
			if ((flags & Modifiers.PRIVATE) != 0)
				return AccessLevel.Private;

			AccessLevel level;
			if (!IsTopLevel && (Parent != null))
				level = Parent.GetAccessLevel (flags);
			else
				level = AccessLevel.Public;

			return CheckAccessLevel (CheckAccessLevel (level, flags), ModFlags);
		}

		// Return the access level for type `t', but don't give more access than `flags'.
		static AccessLevel GetAccessLevel (Type t, int flags)
		{
			if (((flags & Modifiers.PRIVATE) != 0) || t.IsNestedPrivate)
				return AccessLevel.Private;

			AccessLevel level;
			if (TypeManager.IsBuiltinType (t))
				return AccessLevel.Public;
			else if ((t.DeclaringType != null) && (t != t.DeclaringType))
				level = GetAccessLevel (t.DeclaringType, flags);
			else {
				level = CheckAccessLevel (AccessLevel.Public, flags);
			}

			if (t.IsNestedPublic)
				return level;

			if (t.IsNestedAssembly || t.IsNotPublic) {
				if ((int) level < (int) AccessLevel.Internal)
					level = AccessLevel.Internal;
			}

			if (t.IsNestedFamily) {
				if ((int) level < (int) AccessLevel.Protected)
					level = AccessLevel.Protected;
			}

			if (t.IsNestedFamORAssem) {
				if ((int) level < (int) AccessLevel.ProtectedInternal)
					level = AccessLevel.ProtectedInternal;
			}

			return level;
		}

		//
		// Returns true if `parent' is as accessible as the flags `flags'
		// given for this member.
		//
		public bool AsAccessible (Type parent, int flags)
		{
			if (parent.IsUnboundGenericParameter)
				return true; // FIXME

			while (parent.IsArray || parent.IsPointer || parent.IsByRef)
				parent = TypeManager.GetElementType (parent);

			AccessLevel level = GetAccessLevel (flags);
			AccessLevel level2 = GetAccessLevel (parent, flags);

			return (int) level >= (int) level2;
		}
		
		static DoubleHash dh = new DoubleHash ();

		Type LookupInterfaceOrClass (string ns, string name, out bool error)
		{
			DeclSpace parent;
			Type t;
			object r;
			
			error = false;

			if (dh.Lookup (ns, name, out r))
				t = (Type) r;
			else {
				if (ns != ""){
					if (Namespace.IsNamespace (ns)){
						string fullname = (ns != "") ? ns + "." + name : name;
						t = TypeManager.LookupType (fullname);
					} else
						t = null;
				} else
					t = TypeManager.LookupType (name);
			}
			
			if (t != null)
				return t;

			//
			// In case we are fed a composite name, normalize it.
			//
			int p = name.LastIndexOf ('.');
			if (p != -1){
				ns = MakeFQN (ns, name.Substring (0, p));
				name = name.Substring (p+1);
			}
			
			parent = RootContext.Tree.LookupByNamespace (ns, name);
			if (parent == null)
				return null;

			t = parent.DefineType ();
			dh.Insert (ns, name, t);
			if (t == null){
				error = true;
				return null;
			}
			return t;
		}

		public static void Error_AmbiguousTypeReference (Location loc, string name, Type t1, Type t2)
		{
			Report.Error (104, loc,
				      String.Format ("`{0}' is an ambiguous reference ({1} or {2}) ", name,
						     t1.FullName, t2.FullName));
		}

		/// <summary>
		///   GetType is used to resolve type names at the DeclSpace level.
		///   Use this to lookup class/struct bases, interface bases or 
		///   delegate type references
		/// </summary>
		///
		/// <remarks>
		///   Contrast this to LookupType which is used inside method bodies to 
		///   lookup types that have already been defined.  GetType is used
		///   during the tree resolution process and potentially define
		///   recursively the type
		/// </remarks>
		public Type FindType (Location loc, string name)
		{
			Type t;
			bool error;

			//
			// For the case the type we are looking for is nested within this one
			// or is in any base class
			//
			DeclSpace containing_ds = this;

			while (containing_ds != null){
				Type container_type = containing_ds.TypeBuilder;
				Type current_type = container_type;

				while (current_type != null) {
					string pre = current_type.FullName;

					t = LookupInterfaceOrClass (pre, name, out error);
					if (error)
						return null;
				
					if ((t != null) && containing_ds.CheckAccessLevel (t))
						return t;

					current_type = current_type.BaseType;
				}
				containing_ds = containing_ds.Parent;
			}

			//
			// Attempt to lookup the class on our namespace and all it's implicit parents
			//
			for (NamespaceEntry ns = NamespaceEntry; ns != null; ns = ns.ImplicitParent) {
				t = LookupInterfaceOrClass (ns.FullName, name, out error);
				if (error)
					return null;
				
				if (t != null) 
					return t;
			}
			
			//
			// Attempt to do a direct unqualified lookup
			//
			t = LookupInterfaceOrClass ("", name, out error);
			if (error)
				return null;
			
			if (t != null)
				return t;
			
			//
			// Attempt to lookup the class on any of the `using'
			// namespaces
			//

			for (NamespaceEntry ns = NamespaceEntry; ns != null; ns = ns.Parent){

				t = LookupInterfaceOrClass (ns.FullName, name, out error);
				if (error)
					return null;

				if (t != null)
					return t;

				//
				// Now check the using clause list
				//
				Type match = null;
				foreach (Namespace using_ns in ns.GetUsingTable ()) {
					match = LookupInterfaceOrClass (using_ns.Name, name, out error);
					if (error)
						return null;

					if (match != null){
						if (t != null){
							if (CheckAccessLevel (match)) {
								Error_AmbiguousTypeReference (loc, name, t, match);
								return null;
							}
							continue;
						}
						
						t = match;
					}
				}
				if (t != null)
					return t;
			}

			//Report.Error (246, Location, "Can not find type `"+name+"'");
			return null;
		}

		/// <remarks>
		///   This function is broken and not what you're looking for.  It should only
		///   be used while the type is still being created since it doesn't use the cache
		///   and relies on the filter doing the member name check.
		/// </remarks>
		public abstract MemberList FindMembers (MemberTypes mt, BindingFlags bf,
							MemberFilter filter, object criteria);

		/// <remarks>
		///   If we have a MemberCache, return it.  This property may return null if the
		///   class doesn't have a member cache or while it's still being created.
		/// </remarks>
		public abstract MemberCache MemberCache {
			get;
		}
	}

	/// <summary>
	///   This is a readonly list of MemberInfo's.      
	/// </summary>
	public class MemberList : IList {
		public readonly IList List;
		int count;

		/// <summary>
		///   Create a new MemberList from the given IList.
		/// </summary>
		public MemberList (IList list)
		{
			if (list != null)
				this.List = list;
			else
				this.List = new ArrayList ();
			count = List.Count;
		}

		/// <summary>
		///   Concatenate the ILists `first' and `second' to a new MemberList.
		/// </summary>
		public MemberList (IList first, IList second)
		{
			ArrayList list = new ArrayList ();
			list.AddRange (first);
			list.AddRange (second);
			count = list.Count;
			List = list;
		}

		public static readonly MemberList Empty = new MemberList (new ArrayList ());

		/// <summary>
		///   Cast the MemberList into a MemberInfo[] array.
		/// </summary>
		/// <remarks>
		///   This is an expensive operation, only use it if it's really necessary.
		/// </remarks>
		public static explicit operator MemberInfo [] (MemberList list)
		{
			Timer.StartTimer (TimerType.MiscTimer);
			MemberInfo [] result = new MemberInfo [list.Count];
			list.CopyTo (result, 0);
			Timer.StopTimer (TimerType.MiscTimer);
			return result;
		}

		// ICollection

		public int Count {
			get {
				return count;
			}
		}

		public bool IsSynchronized {
			get {
				return List.IsSynchronized;
			}
		}

		public object SyncRoot {
			get {
				return List.SyncRoot;
			}
		}

		public void CopyTo (Array array, int index)
		{
			List.CopyTo (array, index);
		}

		// IEnumerable

		public IEnumerator GetEnumerator ()
		{
			return List.GetEnumerator ();
		}

		// IList

		public bool IsFixedSize {
			get {
				return true;
			}
		}

		public bool IsReadOnly {
			get {
				return true;
			}
		}

		object IList.this [int index] {
			get {
				return List [index];
			}

			set {
				throw new NotSupportedException ();
			}
		}

		// FIXME: try to find out whether we can avoid the cast in this indexer.
		public MemberInfo this [int index] {
			get {
				return (MemberInfo) List [index];
			}
		}

		public int Add (object value)
		{
			throw new NotSupportedException ();
		}

		public void Clear ()
		{
			throw new NotSupportedException ();
		}

		public bool Contains (object value)
		{
			return List.Contains (value);
		}

		public int IndexOf (object value)
		{
			return List.IndexOf (value);
		}

		public void Insert (int index, object value)
		{
			throw new NotSupportedException ();
		}

		public void Remove (object value)
		{
			throw new NotSupportedException ();
		}

		public void RemoveAt (int index)
		{
			throw new NotSupportedException ();
		}
	}

	/// <summary>
	///   This interface is used to get all members of a class when creating the
	///   member cache.  It must be implemented by all DeclSpace derivatives which
	///   want to support the member cache and by TypeHandle to get caching of
	///   non-dynamic types.
	/// </summary>
	public interface IMemberContainer {
		/// <summary>
		///   The name of the IMemberContainer.  This is only used for
		///   debugging purposes.
		/// </summary>
		string Name {
			get;
		}

		/// <summary>
		///   The type of this IMemberContainer.
		/// </summary>
		Type Type {
			get;
		}

		/// <summary>
		///   Returns the IMemberContainer of the parent class or null if this
		///   is an interface or TypeManger.object_type.
		///   This is used when creating the member cache for a class to get all
		///   members from the parent class.
		/// </summary>
		IMemberContainer Parent {
			get;
		}

		/// <summary>
		///   Whether this is an interface.
		/// </summary>
		bool IsInterface {
			get;
		}

		/// <summary>
		///   Returns all members of this class with the corresponding MemberTypes
		///   and BindingFlags.
		/// </summary>
		/// <remarks>
		///   When implementing this method, make sure not to return any inherited
		///   members and check the MemberTypes and BindingFlags properly.
		///   Unfortunately, System.Reflection is lame and doesn't provide a way to
		///   get the BindingFlags (static/non-static,public/non-public) in the
		///   MemberInfo class, but the cache needs this information.  That's why
		///   this method is called multiple times with different BindingFlags.
		/// </remarks>
		MemberList GetMembers (MemberTypes mt, BindingFlags bf);

		/// <summary>
		///   Return the container's member cache.
		/// </summary>
		MemberCache MemberCache {
			get;
		}
	}

	/// <summary>
	///   The MemberCache is used by dynamic and non-dynamic types to speed up
	///   member lookups.  It has a member name based hash table; it maps each member
	///   name to a list of CacheEntry objects.  Each CacheEntry contains a MemberInfo
	///   and the BindingFlags that were initially used to get it.  The cache contains
	///   all members of the current class and all inherited members.  If this cache is
	///   for an interface types, it also contains all inherited members.
	///
	///   There are two ways to get a MemberCache:
	///   * if this is a dynamic type, lookup the corresponding DeclSpace and then
	///     use the DeclSpace.MemberCache property.
	///   * if this not a dynamic type, call TypeHandle.GetTypeHandle() to get a
	///     TypeHandle instance for the type and then use TypeHandle.MemberCache.
	/// </summary>
	public class MemberCache {
		public readonly IMemberContainer Container;
		protected Hashtable member_hash;
		protected Hashtable method_hash;
		protected Hashtable interface_hash;

		/// <summary>
		///   Create a new MemberCache for the given IMemberContainer `container'.
		/// </summary>
		public MemberCache (IMemberContainer container)
		{
			this.Container = container;

			Timer.IncrementCounter (CounterType.MemberCache);
			Timer.StartTimer (TimerType.CacheInit);

			interface_hash = new Hashtable ();

			// If we have a parent class (we have a parent class unless we're
			// TypeManager.object_type), we deep-copy its MemberCache here.
			if (Container.IsInterface) {
				MemberCache parent;
				if (Container.Parent != null)
					parent = Container.Parent.MemberCache;
				else
					parent = TypeHandle.ObjectType.MemberCache;
				member_hash = SetupCacheForInterface (parent);
			} else if (Container.Parent != null)
				member_hash = SetupCache (Container.Parent.MemberCache);
			else
				member_hash = new Hashtable ();

			// If this is neither a dynamic type nor an interface, create a special
			// method cache with all declared and inherited methods.
			Type type = container.Type;
			if (!(type is TypeBuilder) && !type.IsInterface) {
				method_hash = new Hashtable ();
				AddMethods (type);
			}

			// Add all members from the current class.
			AddMembers (Container);

			Timer.StopTimer (TimerType.CacheInit);
		}

		/// <summary>
		///   Bootstrap this member cache by doing a deep-copy of our parent.
		/// </summary>
		Hashtable SetupCache (MemberCache parent)
		{
			Hashtable hash = new Hashtable ();

			IDictionaryEnumerator it = parent.member_hash.GetEnumerator ();
			while (it.MoveNext ()) {
				hash [it.Key] = ((ArrayList) it.Value).Clone ();
                        }
                                
			return hash;
		}

		void AddInterfaces (MemberCache parent)
		{
			foreach (Type iface in parent.interface_hash.Keys) {
				if (!interface_hash.Contains (iface))
					interface_hash.Add (iface, true);
			}
		}

		/// <summary>
		///   Add the contents of `new_hash' to `hash'.
		/// </summary>
		void AddHashtable (Hashtable hash, Hashtable new_hash)
		{
			IDictionaryEnumerator it = new_hash.GetEnumerator ();
			while (it.MoveNext ()) {
				ArrayList list = (ArrayList) hash [it.Key];
				if (list != null)
					list.AddRange ((ArrayList) it.Value);
				else
					hash [it.Key] = ((ArrayList) it.Value).Clone ();
			}
		}

		/// <summary>
		///   Bootstrap the member cache for an interface type.
		///   Type.GetMembers() won't return any inherited members for interface types,
		///   so we need to do this manually.  Interfaces also inherit from System.Object.
		/// </summary>
		Hashtable SetupCacheForInterface (MemberCache parent)
		{
			Hashtable hash = SetupCache (parent);
			Type [] ifaces = TypeManager.GetInterfaces (Container.Type);

			foreach (Type iface in ifaces) {
				if (interface_hash.Contains (iface))
					continue;
				interface_hash.Add (iface, true);

				IMemberContainer iface_container =
					TypeManager.LookupMemberContainer (iface);

				MemberCache iface_cache = iface_container.MemberCache;

				AddHashtable (hash, iface_cache.member_hash);
				AddInterfaces (iface_cache);
			}

			return hash;
		}

		/// <summary>
		///   Add all members from class `container' to the cache.
		/// </summary>
		void AddMembers (IMemberContainer container)
		{
			// We need to call AddMembers() with a single member type at a time
			// to get the member type part of CacheEntry.EntryType right.
			AddMembers (MemberTypes.Constructor, container);
			AddMembers (MemberTypes.Field, container);
			AddMembers (MemberTypes.Method, container);
			AddMembers (MemberTypes.Property, container);
			AddMembers (MemberTypes.Event, container);
			// Nested types are returned by both Static and Instance searches.
			AddMembers (MemberTypes.NestedType,
				    BindingFlags.Static | BindingFlags.Public, container);
			AddMembers (MemberTypes.NestedType,
				    BindingFlags.Static | BindingFlags.NonPublic, container);
		}

		void AddMembers (MemberTypes mt, IMemberContainer container)
		{
			AddMembers (mt, BindingFlags.Static | BindingFlags.Public, container);
			AddMembers (mt, BindingFlags.Static | BindingFlags.NonPublic, container);
			AddMembers (mt, BindingFlags.Instance | BindingFlags.Public, container);
			AddMembers (mt, BindingFlags.Instance | BindingFlags.NonPublic, container);
		}

		/// <summary>
		///   Add all members from class `container' with the requested MemberTypes and
		///   BindingFlags to the cache.  This method is called multiple times with different
		///   MemberTypes and BindingFlags.
		/// </summary>
		void AddMembers (MemberTypes mt, BindingFlags bf, IMemberContainer container)
		{
			MemberList members = container.GetMembers (mt, bf);
			BindingFlags new_bf = (container == Container) ?
				bf | BindingFlags.DeclaredOnly : bf;

			foreach (MemberInfo member in members) {
				string name = member.Name;

				// We use a name-based hash table of ArrayList's.
				ArrayList list = (ArrayList) member_hash [name];
				if (list == null) {
					list = new ArrayList ();
					member_hash.Add (name, list);
				}

				// When this method is called for the current class, the list will
				// already contain all inherited members from our parent classes.
				// We cannot add new members in front of the list since this'd be an
				// expensive operation, that's why the list is sorted in reverse order
				// (ie. members from the current class are coming last).
				list.Add (new CacheEntry (container, member, mt, bf));
			}
		}

		/// <summary>
		///   Add all declared and inherited methods from class `type' to the method cache.
		/// </summary>
		void AddMethods (Type type)
		{
			AddMethods (BindingFlags.Static | BindingFlags.Public |
				    BindingFlags.FlattenHierarchy, type);
			AddMethods (BindingFlags.Static | BindingFlags.NonPublic |
				    BindingFlags.FlattenHierarchy, type);
			AddMethods (BindingFlags.Instance | BindingFlags.Public, type);
			AddMethods (BindingFlags.Instance | BindingFlags.NonPublic, type);
		}

		void AddMethods (BindingFlags bf, Type type)
		{
			MemberInfo [] members = type.GetMethods (bf);

                        Array.Reverse (members);
                        
			foreach (MethodBase member in members) {
				string name = member.Name;

				// Varargs methods aren't allowed in C# code.
				if ((member.CallingConvention & CallingConventions.VarArgs) != 0)
					continue;

				// We use a name-based hash table of ArrayList's.
				ArrayList list = (ArrayList) method_hash [name];
				if (list == null) {
					list = new ArrayList ();
					method_hash.Add (name, list);
				}

				// Unfortunately, the elements returned by Type.GetMethods() aren't
				// sorted so we need to do this check for every member.
				BindingFlags new_bf = bf;
				if (member.DeclaringType == type)
					new_bf |= BindingFlags.DeclaredOnly;

				list.Add (new CacheEntry (Container, member, MemberTypes.Method, new_bf));
			}

                        
		}

		/// <summary>
		///   Compute and return a appropriate `EntryType' magic number for the given
		///   MemberTypes and BindingFlags.
		/// </summary>
		protected static EntryType GetEntryType (MemberTypes mt, BindingFlags bf)
		{
			EntryType type = EntryType.None;

			if ((mt & MemberTypes.Constructor) != 0)
				type |= EntryType.Constructor;
			if ((mt & MemberTypes.Event) != 0)
				type |= EntryType.Event;
			if ((mt & MemberTypes.Field) != 0)
				type |= EntryType.Field;
			if ((mt & MemberTypes.Method) != 0)
				type |= EntryType.Method;
			if ((mt & MemberTypes.Property) != 0)
				type |= EntryType.Property;
			// Nested types are returned by static and instance searches.
			if ((mt & MemberTypes.NestedType) != 0)
				type |= EntryType.NestedType | EntryType.Static | EntryType.Instance;

			if ((bf & BindingFlags.Instance) != 0)
				type |= EntryType.Instance;
			if ((bf & BindingFlags.Static) != 0)
				type |= EntryType.Static;
			if ((bf & BindingFlags.Public) != 0)
				type |= EntryType.Public;
			if ((bf & BindingFlags.NonPublic) != 0)
				type |= EntryType.NonPublic;
			if ((bf & BindingFlags.DeclaredOnly) != 0)
				type |= EntryType.Declared;

			return type;
		}

		/// <summary>
		///   The `MemberTypes' enumeration type is a [Flags] type which means that it may
		///   denote multiple member types.  Returns true if the given flags value denotes a
		///   single member types.
		/// </summary>
		public static bool IsSingleMemberType (MemberTypes mt)
		{
			switch (mt) {
			case MemberTypes.Constructor:
			case MemberTypes.Event:
			case MemberTypes.Field:
			case MemberTypes.Method:
			case MemberTypes.Property:
			case MemberTypes.NestedType:
				return true;

			default:
				return false;
			}
		}

		/// <summary>
		///   We encode the MemberTypes and BindingFlags of each members in a "magic"
		///   number to speed up the searching process.
		/// </summary>
		[Flags]
		protected enum EntryType {
			None		= 0x000,

			Instance	= 0x001,
			Static		= 0x002,
			MaskStatic	= Instance|Static,

			Public		= 0x004,
			NonPublic	= 0x008,
			MaskProtection	= Public|NonPublic,

			Declared	= 0x010,

			Constructor	= 0x020,
			Event		= 0x040,
			Field		= 0x080,
			Method		= 0x100,
			Property	= 0x200,
			NestedType	= 0x400,

			MaskType	= Constructor|Event|Field|Method|Property|NestedType
		}

		protected struct CacheEntry {
			public readonly IMemberContainer Container;
			public readonly EntryType EntryType;
			public readonly MemberInfo Member;

			public CacheEntry (IMemberContainer container, MemberInfo member,
					   MemberTypes mt, BindingFlags bf)
			{
				this.Container = container;
				this.Member = member;
				this.EntryType = GetEntryType (mt, bf);
			}
		}

		/// <summary>
		///   This is called each time we're walking up one level in the class hierarchy
		///   and checks whether we can abort the search since we've already found what
		///   we were looking for.
		/// </summary>
		protected bool DoneSearching (ArrayList list)
		{
			//
			// We've found exactly one member in the current class and it's not
			// a method or constructor.
			//
			if (list.Count == 1 && !(list [0] is MethodBase))
				return true;

			//
			// Multiple properties: we query those just to find out the indexer
			// name
			//
			if ((list.Count > 0) && (list [0] is PropertyInfo))
				return true;

			return false;
		}

		/// <summary>
		///   Looks up members with name `name'.  If you provide an optional
		///   filter function, it'll only be called with members matching the
		///   requested member name.
		///
		///   This method will try to use the cache to do the lookup if possible.
		///
		///   Unlike other FindMembers implementations, this method will always
		///   check all inherited members - even when called on an interface type.
		///
		///   If you know that you're only looking for methods, you should use
		///   MemberTypes.Method alone since this speeds up the lookup a bit.
		///   When doing a method-only search, it'll try to use a special method
		///   cache (unless it's a dynamic type or an interface) and the returned
		///   MemberInfo's will have the correct ReflectedType for inherited methods.
		///   The lookup process will automatically restart itself in method-only
		///   search mode if it discovers that it's about to return methods.
		/// </summary>
		ArrayList global = new ArrayList ();
		bool using_global = false;
		
		public MemberList FindMembers (MemberTypes mt, BindingFlags bf, string name,
					       MemberFilter filter, object criteria)
		{
			if (using_global)
				throw new Exception ();
			
			bool declared_only = (bf & BindingFlags.DeclaredOnly) != 0;
			bool method_search = mt == MemberTypes.Method;
			// If we have a method cache and we aren't already doing a method-only search,
			// then we restart a method search if the first match is a method.
			bool do_method_search = !method_search && (method_hash != null);

			ArrayList applicable;

			// If this is a method-only search, we try to use the method cache if
			// possible; a lookup in the method cache will return a MemberInfo with
			// the correct ReflectedType for inherited methods.
			
			if (method_search && (method_hash != null))
				applicable = (ArrayList) method_hash [name];
			else
				applicable = (ArrayList) member_hash [name];
			
			if (applicable == null)
				return MemberList.Empty;

			//
			// 32  slots gives 53 rss/54 size
			// 2/4 slots gives 55 rss
			//
			// Strange: from 25,000 calls, only 1,800
			// are above 2.  Why does this impact it?
			//
			global.Clear ();
			using_global = true;

			Timer.StartTimer (TimerType.CachedLookup);

			EntryType type = GetEntryType (mt, bf);

			IMemberContainer current = Container;

			// `applicable' is a list of all members with the given member name `name'
			// in the current class and all its parent classes.  The list is sorted in
			// reverse order due to the way how the cache is initialy created (to speed
			// things up, we're doing a deep-copy of our parent).

			for (int i = applicable.Count-1; i >= 0; i--) {
				CacheEntry entry = (CacheEntry) applicable [i];

				// This happens each time we're walking one level up in the class
				// hierarchy.  If we're doing a DeclaredOnly search, we must abort
				// the first time this happens (this may already happen in the first
				// iteration of this loop if there are no members with the name we're
				// looking for in the current class).
				if (entry.Container != current) {
					if (declared_only || DoneSearching (global))
						break;

					current = entry.Container;
				}

				// Is the member of the correct type ?
				if ((entry.EntryType & type & EntryType.MaskType) == 0)
					continue;

				// Is the member static/non-static ?
				if ((entry.EntryType & type & EntryType.MaskStatic) == 0)
					continue;

				// Apply the filter to it.
				if (filter (entry.Member, criteria)) {
					if ((entry.EntryType & EntryType.MaskType) != EntryType.Method)
						do_method_search = false;
					global.Add (entry.Member);
				}
			}

			Timer.StopTimer (TimerType.CachedLookup);

			// If we have a method cache and we aren't already doing a method-only
			// search, we restart in method-only search mode if the first match is
			// a method.  This ensures that we return a MemberInfo with the correct
			// ReflectedType for inherited methods.
			if (do_method_search && (global.Count > 0)){
				using_global = false;

				return FindMembers (MemberTypes.Method, bf, name, filter, criteria);
			}

			using_global = false;
			MemberInfo [] copy = new MemberInfo [global.Count];
			global.CopyTo (copy);
			return new MemberList (copy);
		}
	}
}
