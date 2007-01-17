//
// class.cs: Class and Struct handlers
//
// Authors: Miguel de Icaza (miguel@gnu.org)
//          Martin Baulig (martin@ximian.com)
//          Marek Safar (marek.safar@seznam.cz)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
// (C) 2004 Novell, Inc
//
//
//  2002-10-11  Miguel de Icaza  <miguel@ximian.com>
//
//	* class.cs: Following the comment from 2002-09-26 to AddMethod, I
//	have fixed a remaining problem: not every AddXXXX was adding a
//	fully qualified name.  
//
//	Now everyone registers a fully qualified name in the DeclSpace as
//	being defined instead of the partial name.  
//
//	Downsides: we are slower than we need to be due to the excess
//	copies and the names being registered this way.  
//
//	The reason for this is that we currently depend (on the corlib
//	bootstrap for instance) that types are fully qualified, because
//	we dump all the types in the namespace, and we should really have
//	types inserted into the proper namespace, so we can only store the
//	basenames in the defined_names array.
//
//
#define CACHE
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;

#if BOOTSTRAP_WITH_OLDLIB
using XmlElement = System.Object;
#else
using System.Xml;
#endif

using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp {

	public enum Kind {
		Root,
		Struct,
		Class,
		Interface
	}

	/// <summary>
	///   This is the base class for structs and classes.  
	/// </summary>
	public abstract class TypeContainer : DeclSpace, IMemberContainer {

 		public class MemberCoreArrayList: ArrayList
 		{
			/// <summary>
			///   Defines the MemberCore objects that are in this array
			/// </summary>
			public virtual void DefineContainerMembers ()
			{
				foreach (MemberCore mc in this) {
					try {
						mc.Define ();
					} catch (Exception e) {
						throw new InternalErrorException (mc.Location, mc.GetSignatureForError (), e);
					}
				}
			}

			public virtual void Emit ()
			{
				foreach (MemberCore mc in this)
					mc.Emit ();
			}
 		}

 		public class MethodArrayList : MemberCoreArrayList
 		{
 			[Flags]
 			enum CachedMethods {
 				Equals			= 1,
 				GetHashCode		= 1 << 1
 			}
 
 			CachedMethods cached_method;
			TypeContainer container;

			public MethodArrayList (TypeContainer container)
			{
				this.container = container;
			}
 
 			/// <summary>
 			/// Method container contains Equals method
 			/// </summary>
 			public bool HasEquals {
 				set {
 					cached_method |= CachedMethods.Equals;
 				}
 
 				get {
 					return (cached_method & CachedMethods.Equals) != 0;
 				}
 			}
 
 			/// <summary>
 			/// Method container contains GetHashCode method
 			/// </summary>
 			public bool HasGetHashCode {
 				set {
 					cached_method |= CachedMethods.GetHashCode;
 				}
 
 				get {
 					return (cached_method & CachedMethods.GetHashCode) != 0;
 				}
 			}
 
 			public override void DefineContainerMembers ()
 			{
 				base.DefineContainerMembers ();
 
 				if (HasEquals && !HasGetHashCode) {
 					Report.Warning (659, 3, container.Location, "`{0}' overrides Object.Equals(object) but does not override Object.GetHashCode()", container.GetSignatureForError ());
 				}
 			}
 		}

		public sealed class IndexerArrayList : MemberCoreArrayList
		{
			/// <summary>
			/// The indexer name for this container
			/// </summary>
 			public string IndexerName = DefaultIndexerName;

			bool seen_normal_indexers = false;

			TypeContainer container;

			public IndexerArrayList (TypeContainer container)
			{
				this.container = container;
			}

			/// <summary>
			/// Defines the indexers, and also verifies that the IndexerNameAttribute in the
			/// class is consistent.  Either it is `Item' or it is the name defined by all the
			/// indexers with the `IndexerName' attribute.
			///
			/// Turns out that the IndexerNameAttribute is applied to each indexer,
			/// but it is never emitted, instead a DefaultMember attribute is attached
			/// to the class.
			/// </summary>
			public override void DefineContainerMembers()
			{
				base.DefineContainerMembers ();

				string class_indexer_name = null;

				//
				// If there's both an explicit and an implicit interface implementation, the
				// explicit one actually implements the interface while the other one is just
				// a normal indexer.  See bug #37714.
				//

				// Invariant maintained by AddIndexer(): All explicit interface indexers precede normal indexers
				foreach (Indexer i in this) {
					if (i.InterfaceType != null) {
						if (seen_normal_indexers)
							throw new Exception ("Internal Error: 'Indexers' array not sorted properly.");
						continue;
					}

					seen_normal_indexers = true;

					if (class_indexer_name == null) {
						class_indexer_name = i.ShortName;
						continue;
					}

					if (i.ShortName != class_indexer_name)
						Report.Error (668, i.Location, "Two indexers have different names; the IndexerName attribute must be used with the same name on every indexer within a type");
				}

				if (class_indexer_name != null)
					IndexerName = class_indexer_name;
			}

			public override void Emit ()
			{
				base.Emit ();

				if (!seen_normal_indexers)
					return;

				CustomAttributeBuilder cb = new CustomAttributeBuilder (TypeManager.default_member_ctor, new string [] { IndexerName });
				container.TypeBuilder.SetCustomAttribute (cb);
			}
		}

 		public class OperatorArrayList: MemberCoreArrayList
		{
			TypeContainer container;

			public OperatorArrayList (TypeContainer container)
			{
				this.container = container;
			}

			//
			// Operator pair checking
			//
			class OperatorEntry
			{
				public int flags;
				public Type ret_type;
				public Type type1, type2;
				public Operator op;
				public Operator.OpType ot;
				
				public OperatorEntry (int f, Operator o)
				{
					flags = f;

					ret_type = o.MemberType;
					Type [] pt = o.ParameterTypes;
					type1 = pt [0];
					type2 = pt [1];
					op = o;
					ot = o.OperatorType;
				}

				public override int GetHashCode ()
				{	
					return ret_type.GetHashCode ();
				}

				public override bool Equals (object o)
				{
					OperatorEntry other = (OperatorEntry) o;

					if (other.ret_type != ret_type)
						return false;
					if (other.type1 != type1)
						return false;
					if (other.type2 != type2)
						return false;
					return true;
				}
			}
				
			//
			// Checks that some operators come in pairs:
			//  == and !=
			// > and <
			// >= and <=
			// true and false
			//
			// They are matched based on the return type and the argument types
			//
			void CheckPairedOperators ()
			{
				IDictionary pairs = new HybridDictionary ();
				Operator true_op = null;
				Operator false_op = null;
				bool has_equality_or_inequality = false;
				
				// Register all the operators we care about.
				foreach (Operator op in this){
					int reg = 0;

					// Skip erroneous code.
					if (op.MethodBuilder == null)
						continue;

					switch (op.OperatorType){
					case Operator.OpType.Equality:
						reg = 1;
						has_equality_or_inequality = true;
						break;
					case Operator.OpType.Inequality:
						reg = 2;
						has_equality_or_inequality = true;
						break;

					case Operator.OpType.True:
						true_op = op;
						break;
					case Operator.OpType.False:
						false_op = op;
						break;
						
					case Operator.OpType.GreaterThan:
						reg = 1; break;
					case Operator.OpType.LessThan:
						reg = 2; break;
						
					case Operator.OpType.GreaterThanOrEqual:
						reg = 1; break;
					case Operator.OpType.LessThanOrEqual:
						reg = 2; break;
					}
					if (reg == 0)
						continue;

					OperatorEntry oe = new OperatorEntry (reg, op);

					object o = pairs [oe];
					if (o == null)
						pairs [oe] = oe;
					else {
						oe = (OperatorEntry) o;
						oe.flags |= reg;
					}
				}

				if (true_op != null){
					if (false_op == null)
						Report.Error (216, true_op.Location, "The operator `{0}' requires a matching operator `false' to also be defined",
							true_op.GetSignatureForError ());
				} else if (false_op != null)
					Report.Error (216, false_op.Location, "The operator `{0}' requires a matching operator `true' to also be defined",
						false_op.GetSignatureForError ());
				
				//
				// Look for the mistakes.
				//
				foreach (DictionaryEntry de in pairs){
					OperatorEntry oe = (OperatorEntry) de.Key;

					if (oe.flags == 3)
						continue;

					string s = "";
					switch (oe.ot){
					case Operator.OpType.Equality:
						s = "!=";
						break;
					case Operator.OpType.Inequality: 
						s = "==";
						break;
					case Operator.OpType.GreaterThan: 
						s = "<";
						break;
					case Operator.OpType.LessThan:
						s = ">";
						break;
					case Operator.OpType.GreaterThanOrEqual:
						s = "<=";
						break;
					case Operator.OpType.LessThanOrEqual:
						s = ">=";
						break;
					}
					Report.Error (216, oe.op.Location,
						"The operator `{0}' requires a matching operator `{1}' to also be defined",
						oe.op.GetSignatureForError (), s);
				}

 				if (has_equality_or_inequality && (RootContext.WarningLevel > 2)) {
 					if (container.Methods == null || !container.Methods.HasEquals)
 						Report.Warning (660, 2, container.Location, "`{0}' defines operator == or operator != but does not override Object.Equals(object o)", container.GetSignatureForError ());
 
 					if (container.Methods == null || !container.Methods.HasGetHashCode)
 						Report.Warning (661, 2, container.Location, "`{0}' defines operator == or operator != but does not override Object.GetHashCode()", container.GetSignatureForError ());
 				}
			}

	 		public override void DefineContainerMembers ()
	 		{
	 			base.DefineContainerMembers ();
	 			CheckPairedOperators ();
			}
		}


		// Whether this is a struct, class or interface
		public readonly Kind Kind;

		// Holds a list of classes and structures
		protected ArrayList types;

		// Holds the list of properties
		MemberCoreArrayList properties;

		// Holds the list of enumerations
		MemberCoreArrayList enums;

		// Holds the list of delegates
		MemberCoreArrayList delegates;
		
		// Holds the list of constructors
		protected MemberCoreArrayList instance_constructors;

		// Holds the list of fields
		MemberCoreArrayList fields;

		// Holds a list of fields that have initializers
		protected ArrayList initialized_fields;

		// Holds a list of static fields that have initializers
		protected ArrayList initialized_static_fields;

		// Holds the list of constants
		MemberCoreArrayList constants;

		// Holds the list of
		MemberCoreArrayList interfaces;

		// Holds the methods.
		MethodArrayList methods;

		// Holds the events
		protected MemberCoreArrayList events;

		// Holds the indexers
		IndexerArrayList indexers;

		// Holds the operators
		MemberCoreArrayList operators;

		// Holds the compiler generated classes
		ArrayList compiler_generated;

		//
		// Pointers to the default constructor and the default static constructor
		//
		protected Constructor default_constructor;
		protected Constructor default_static_constructor;

		//
		// Points to the first non-static field added to the container.
		//
		// This is an arbitrary choice.  We are interested in looking at _some_ non-static field,
		// and the first one's as good as any.
		//
		FieldBase first_nonstatic_field = null;

		//
		// This one is computed after we can distinguish interfaces
		// from classes from the arraylist `type_bases' 
		//
		TypeExpr base_type;
		TypeExpr[] iface_exprs;
		Type GenericType;

		ArrayList type_bases;

		bool members_resolved;
		bool members_resolved_ok;
		bool members_defined;
		bool members_defined_ok;

		// The interfaces we implement.
		protected Type[] ifaces;

		// The base member cache and our member cache
		MemberCache base_cache;
		MemberCache member_cache;

		public const string DefaultIndexerName = "Item";

#if GMCS_SOURCE
		GenericTypeParameterBuilder[] gen_params;
#endif

		ArrayList partial_parts;

		/// <remarks>
		///  The pending methods that need to be implemented
		//   (interfaces or abstract methods)
		/// </remarks>
		PendingImplementation pending;

		public TypeContainer (NamespaceEntry ns, DeclSpace parent, MemberName name,
				      Attributes attrs, Kind kind)
			: base (ns, parent, name, attrs)
		{
			if (parent != null && parent.NamespaceEntry != ns)
				throw new InternalErrorException ("A nested type should be in the same NamespaceEntry as its enclosing class");

			this.Kind = kind;
			this.PartialContainer = this;
		}

		public bool AddMember (MemberCore symbol)
		{
			return AddToContainer (symbol, symbol.MemberName.MethodName);
		}

		protected virtual bool AddMemberType (DeclSpace ds)
		{
			return AddToContainer (ds, ds.Basename);
		}

		public void AddConstant (Const constant)
		{
			if (!AddMember (constant))
				return;

			if (constants == null)
				constants = new MemberCoreArrayList ();
			
			constants.Add (constant);
		}

		public void AddEnum (Mono.CSharp.Enum e)
		{
			if (!AddMemberType (e))
				return;

			if (enums == null)
				enums = new MemberCoreArrayList ();

			enums.Add (e);
		}

		public TypeContainer AddTypeContainer (TypeContainer tc, bool is_interface)
		{
			if (!AddMemberType (tc))
				return tc;

			if (is_interface) {
				if (interfaces == null)
					interfaces = new MemberCoreArrayList ();
				interfaces.Add (tc);
			} else {
				if (types == null)
					types = new ArrayList (2);
				types.Add (tc);
			}
			return tc;
		}

		public virtual TypeContainer AddPartial (TypeContainer nextPart, bool is_interface)
		{
			return AddPartial (nextPart, nextPart.Basename, is_interface);
		}

		protected TypeContainer AddPartial (TypeContainer nextPart, string name, bool is_interface)
		{
			nextPart.ModFlags |= Modifiers.PARTIAL;
			TypeContainer tc = defined_names [name] as TypeContainer;

			if (tc == null)
				return AddTypeContainer (nextPart, is_interface);

			if ((tc.ModFlags & Modifiers.PARTIAL) == 0) {
				Report.SymbolRelatedToPreviousError (tc);
				Error_MissingPartialModifier (nextPart);
				return tc;
			}

			if (tc.Kind != nextPart.Kind) {
				Report.SymbolRelatedToPreviousError (tc);
				Report.Error (261, nextPart.Location,
					"Partial declarations of `{0}' must be all classes, all structs or all interfaces",
					nextPart.GetSignatureForError ());
				return tc;
			}

			if ((tc.ModFlags & Modifiers.Accessibility) != (nextPart.ModFlags & Modifiers.Accessibility) &&
				((tc.ModFlags & Modifiers.DEFAULT_ACCESS_MODIFER) == 0 &&
				 (nextPart.ModFlags & Modifiers.DEFAULT_ACCESS_MODIFER) == 0)) {
				Report.SymbolRelatedToPreviousError (tc);
				Report.Error (262, nextPart.Location,
					"Partial declarations of `{0}' have conflicting accessibility modifiers",
					nextPart.GetSignatureForError ());
				return tc;
			}

			if (tc.MemberName.IsGeneric) {
				TypeParameter[] tc_names = tc.TypeParameters;
				TypeParameterName[] part_names = nextPart.MemberName.TypeArguments.GetDeclarations ();

				for (int i = 0; i < tc_names.Length; ++i) {
					if (tc_names[i].Name == part_names[i].Name)
						continue;

					Report.SymbolRelatedToPreviousError (part_names[i].Location, "");
					Report.Error (264, tc.Location, "Partial declarations of `{0}' must have the same type parameter names in the same order",
						tc.GetSignatureForError ());
					return tc;
				}
			}

			if (tc.partial_parts == null)
				tc.partial_parts = new ArrayList (1);

			tc.ModFlags |= nextPart.ModFlags;
			if (nextPart.attributes != null) {
				if (tc.attributes == null)
					tc.attributes = nextPart.attributes;
				else
					tc.attributes.AddAttributes (nextPart.attributes.Attrs);
			}

			nextPart.PartialContainer = tc;
			tc.partial_parts.Add (nextPart);
			return tc;
		}

		public void AddDelegate (Delegate d)
		{
			if (!AddMemberType (d))
				return;

			if (delegates == null)
				delegates = new MemberCoreArrayList ();

			delegates.Add (d);
		}

		public void AddMethod (Method method)
		{
			if (!AddMember (method))
				return;

			if (methods == null)
				methods = new MethodArrayList (this);
			
			if (method.MemberName.Left != null)
				methods.Insert (0, method);
			else 
				methods.Add (method);
		}

		//
		// Do not use this method: use AddMethod.
		//
		// This is only used by iterators.
		//
		public void AppendMethod (Method method)
		{
			if (!AddMember (method))
				return;

			if (methods == null)
				methods = new MethodArrayList (this);

			methods.Add (method);
		}

		public void AddConstructor (Constructor c)
		{
			if (c.Name != MemberName.Name) {
				Report.Error (1520, c.Location, "Class, struct, or interface method must have a return type");
			}

			bool is_static = (c.ModFlags & Modifiers.STATIC) != 0;
			
			if (is_static){
				if (default_static_constructor != null) {
					Report.SymbolRelatedToPreviousError (default_static_constructor);
					Report.Error (111, c.Location, Error111, c.GetSignatureForError ());
					return;
				}

				default_static_constructor = c;
			} else {
				if (c.Parameters.Empty){
					if (default_constructor != null) {
						Report.SymbolRelatedToPreviousError (default_constructor);
						Report.Error (111, c.Location, Error111, c.GetSignatureForError ());
						return;
					}
					default_constructor = c;
				}
				
				if (instance_constructors == null)
					instance_constructors = new MemberCoreArrayList ();
				
				instance_constructors.Add (c);
			}
		}

		internal static string Error111 {
			get {
				return "`{0}' is already defined. Rename this member or use different parameter types";
			}
		}

		public void AddField (FieldBase field)
		{
			if (!AddMember (field))
				return;

			if (fields == null)
				fields = new MemberCoreArrayList ();

			fields.Add (field);

			if ((field.ModFlags & Modifiers.STATIC) != 0)
				return;

			if (first_nonstatic_field == null) {
				first_nonstatic_field = field;
				return;
			}

			if (Kind == Kind.Struct &&
			    first_nonstatic_field.Parent != field.Parent &&
			    RootContext.WarningLevel >= 3) {
				Report.SymbolRelatedToPreviousError (first_nonstatic_field.Parent);
				Report.Warning (282, 3, field.Location,
					"struct instance field `{0}' found in different declaration from instance field `{1}'",
					field.GetSignatureForError (), first_nonstatic_field.GetSignatureForError ());
			}
		}

		public void AddProperty (Property prop)
		{
			if (!AddMember (prop) || 
				!AddMember (prop.Get) || !AddMember (prop.Set))
				return;

			if (properties == null)
				properties = new MemberCoreArrayList ();

			if (prop.MemberName.Left != null)
				properties.Insert (0, prop);
			else
				properties.Add (prop);
		}

		public void AddEvent (Event e)
		{
			if (!AddMember (e))
				return;

			if (e is EventProperty) {
				if (!AddMember (e.Add))
					return;

				if (!AddMember (e.Remove))
					return;
			}

			if (events == null)
				events = new MemberCoreArrayList ();

			events.Add (e);
		}

		/// <summary>
		/// Indexer has special handling in constrast to other AddXXX because the name can be driven by IndexerNameAttribute
		/// </summary>
		public void AddIndexer (Indexer i)
		{
			if (indexers == null)
				indexers = new IndexerArrayList (this);

			if (i.IsExplicitImpl)
				indexers.Insert (0, i);
			else
				indexers.Add (i);
		}

		public void AddOperator (Operator op)
		{
			if (!AddMember (op))
				return;

			if (operators == null)
				operators = new OperatorArrayList (this);

			operators.Add (op);
		}

		public void AddCompilerGeneratedClass (CompilerGeneratedClass c)
		{
			Report.Debug (64, "ADD COMPILER GENERATED CLASS", this, c);

			if (compiler_generated == null)
				compiler_generated = new ArrayList ();

			compiler_generated.Add (c);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.default_member_type) {
				if (Indexers != null) {
					Report.Error (646, a.Location, "Cannot specify the `DefaultMember' attribute on type containing an indexer");
					return;
				}
			}
			
			base.ApplyAttributeBuilder (a, cb);
		} 

		public override AttributeTargets AttributeTargets {
			get {
				throw new NotSupportedException ();
			}
		}

		public ArrayList Types {
			get {
				return types;
			}
		}

		public MethodArrayList Methods {
			get {
				return methods;
			}
		}

		public ArrayList Constants {
			get {
				return constants;
			}
		}

		public ArrayList Interfaces {
			get {
				return interfaces;
			}
		}

		public ArrayList CompilerGenerated {
			get {
				return compiler_generated;
			}
		}

		protected Type BaseType {
			get {
				return TypeBuilder.BaseType;
			}
		}
		
		public ArrayList Bases {
			get {
				return type_bases;
			}

			set {
				type_bases = value;
			}
		}

		public ArrayList Fields {
			get {
				return fields;
			}
		}

		public ArrayList InstanceConstructors {
			get {
				return instance_constructors;
			}
		}

		public ArrayList Properties {
			get {
				return properties;
			}
		}

		public ArrayList Events {
			get {
				return events;
			}
		}
		
		public ArrayList Enums {
			get {
				return enums;
			}
		}

		public ArrayList Indexers {
			get {
				return indexers;
			}
		}

		public ArrayList Operators {
			get {
				return operators;
			}
		}

		public ArrayList Delegates {
			get {
				return delegates;
			}
		}
		
		protected override TypeAttributes TypeAttr {
			get {
				return Modifiers.TypeAttr (ModFlags, IsTopLevel) | base.TypeAttr;
			}
		}

		public string IndexerName {
			get {
				return indexers == null ? DefaultIndexerName : indexers.IndexerName;
			}
		}

		public bool IsComImport {
			get {
				if (OptAttributes == null)
					return false;

				return OptAttributes.Contains (TypeManager.comimport_attr_type);
			}
		}

		public virtual void RegisterFieldForInitialization (MemberCore field, FieldInitializer expression)
		{
			if ((field.ModFlags & Modifiers.STATIC) != 0){
				if (initialized_static_fields == null)
					initialized_static_fields = new ArrayList (4);

				initialized_static_fields.Add (expression);
			} else {
				if (initialized_fields == null)
					initialized_fields = new ArrayList (4);

				initialized_fields.Add (expression);
			}
		}

		//
		// Emits the instance field initializers
		//
		public bool EmitFieldInitializers (EmitContext ec)
		{
			ArrayList fields;
			
			if (ec.IsStatic){
				fields = initialized_static_fields;
			} else {
				fields = initialized_fields;
			}

			if (fields == null)
				return true;

			foreach (FieldInitializer f in fields) {
				f.EmitStatement (ec);
			}
			return true;
		}
		
		public override string DocComment {
			get {
				return comment;
			}
			set {
				if (value == null)
					return;

				comment += value;
			}
		}

		public PendingImplementation PendingImplementations {
			get { return pending; }
		}

		public override bool GetClsCompliantAttributeValue ()
		{
			if (PartialContainer != this)
				return PartialContainer.GetClsCompliantAttributeValue ();

			return base.GetClsCompliantAttributeValue ();
		}

		public void AddBasesForPart (DeclSpace part, ArrayList bases)
		{
			// FIXME: get rid of partial_parts and store lists of bases of each part here
			// assumed, not verified: 'part' is in 'partial_parts' 
			((TypeContainer) part).Bases = bases;
		}

		TypeExpr[] GetNormalBases (out TypeExpr base_class)
		{
			base_class = null;
			if (Bases == null)
				return null;

			int count = Bases.Count;
			int start = 0, i, j;

			if (Kind == Kind.Class){
				TypeExpr name = ((Expression) Bases [0]).ResolveAsBaseTerminal (this, false);

				if (name == null){
					return null;
				}

				if (!name.IsInterface) {
					// base_class could be a class, struct, enum, delegate.
					// This is validated in GetClassBases.
					base_class = name;
					start = 1;
				}
			}

			TypeExpr [] ifaces = new TypeExpr [count-start];
			
			for (i = start, j = 0; i < count; i++, j++){
				TypeExpr resolved = ((Expression) Bases [i]).ResolveAsBaseTerminal (this, false);
				if (resolved == null) {
					return null;
				}
				
				ifaces [j] = resolved;
			}

			return ifaces.Length == 0 ? null : ifaces;
		}


		TypeExpr[] GetNormalPartialBases (ref TypeExpr base_class)
		{
			ArrayList ifaces = new ArrayList (0);
			if (iface_exprs != null)
				ifaces.AddRange (iface_exprs);

			foreach (TypeContainer part in partial_parts) {
				TypeExpr new_base_class;
				TypeExpr[] new_ifaces = part.GetClassBases (out new_base_class);
				if (new_base_class != TypeManager.system_object_expr) {
					if (base_class == TypeManager.system_object_expr)
						base_class = new_base_class;
					else {
						if (new_base_class != null && !new_base_class.Equals (base_class)) {
							Report.SymbolRelatedToPreviousError (base_class.Location, "");
							Report.Error (263, part.Location,
								"Partial declarations of `{0}' must not specify different base classes",
								part.GetSignatureForError ());

							return null;
						}
					}
				}

				if (new_ifaces == null)
					continue;

				foreach (TypeExpr iface in new_ifaces) {
					if (ifaces.Contains (iface))
						continue;

					ifaces.Add (iface);
				}
			}

			if (ifaces.Count == 0)
				return null;

			return (TypeExpr[])ifaces.ToArray (typeof (TypeExpr));
		}

		/// <summary>
		///   This function computes the Base class and also the
		///   list of interfaces that the class or struct @c implements.
		///   
		///   The return value is an array (might be null) of
		///   interfaces implemented (as Types).
		///   
		///   The @base_class argument is set to the base object or null
		///   if this is `System.Object'. 
		/// </summary>
		public virtual TypeExpr [] GetClassBases (out TypeExpr base_class)
		{
			TypeExpr[] ifaces = GetNormalBases (out base_class);

			if (ifaces == null)
				return null;

			int count = ifaces.Length;

			for (int i = 0; i < count; i++) {
				TypeExpr iface = (TypeExpr) ifaces [i];

				if (!iface.IsInterface) {
					if (Kind != Kind.Class) {
						// TODO: location of symbol related ....
						Error_TypeInListIsNotInterface (Location, iface.GetSignatureForError ());
					}
					else if (base_class != null)
						Report.Error (1721, Location, "`{0}': Classes cannot have multiple base classes (`{1}' and `{2}')",
							GetSignatureForError (), base_class.GetSignatureForError (), iface.GetSignatureForError ());
					else {
						Report.Error (1722, Location, "`{0}': Base class `{1}' must be specified as first",
							GetSignatureForError (), iface.GetSignatureForError ());
					}
					return null;
				}

				for (int x = 0; x < i; x++) {
					if (iface.Equals (ifaces [x])) {
						Report.Error (528, Location,
							      "`{0}' is already listed in " +
							      "interface list", iface.GetSignatureForError ());
						return null;
					}
				}

				if ((Kind == Kind.Interface) &&
				    !iface.AsAccessible (Parent, ModFlags)) {
					Report.Error (61, Location,
						      "Inconsistent accessibility: base " +
						      "interface `{0}' is less accessible " +
						      "than interface `{1}'", iface.Name,
						      Name);
					return null;
				}
			}
			return ifaces;
		}

		bool CheckGenericInterfaces (Type[] ifaces)
		{
#if GMCS_SOURCE
			ArrayList already_checked = new ArrayList ();

			for (int i = 0; i < ifaces.Length; i++) {
				Type iface = ifaces [i];
				foreach (Type t in already_checked) {
					if (iface == t)
						continue;

					Type[] infered = new Type [CountTypeParameters];
					if (!TypeManager.MayBecomeEqualGenericInstances (iface, t, infered, null))
						continue;

					Report.Error (695, Location,
						"`{0}' cannot implement both `{1}' and `{2}' " +
						"because they may unify for some type parameter substitutions",
						TypeManager.CSharpName (TypeBuilder), TypeManager.CSharpName (iface),
						TypeManager.CSharpName (t));
					return false;
				}

				already_checked.Add (iface);
			}
#endif

			return true;
		}

		bool error = false;
		
		protected void Error_TypeInListIsNotInterface (Location loc, string type)
		{
			Report.Error (527, loc, "Type `{0}' in interface list is not an interface", type);
		}

		bool DefineTypeBuilder ()
		{
			try {
				if (IsTopLevel){
					if (TypeManager.NamespaceClash (Name, Location)) {
						return false;
					}

					ModuleBuilder builder = CodeGen.Module.Builder;
					Type default_parent = null;
					if (Kind == Kind.Struct)
						default_parent = TypeManager.value_type;
					TypeBuilder = builder.DefineType (
						Name, TypeAttr, default_parent, null);
				} else {
					TypeBuilder builder = Parent.TypeBuilder;

					TypeBuilder = builder.DefineNestedType (
						Basename, TypeAttr, null, null);
				}
			} catch (ArgumentException) {
				Report.RuntimeMissingSupport (Location, "static classes");
				return false;
			}

			TypeManager.AddUserType (this);

#if GMCS_SOURCE
			if (IsGeneric) {
				string[] param_names = new string [TypeParameters.Length];
				for (int i = 0; i < TypeParameters.Length; i++)
					param_names [i] = TypeParameters [i].Name;

				gen_params = TypeBuilder.DefineGenericParameters (param_names);

				int offset = CountTypeParameters - CurrentTypeParameters.Length;
				for (int i = offset; i < gen_params.Length; i++)
					CurrentTypeParameters [i - offset].Define (gen_params [i]);
			}
#endif

			iface_exprs = GetClassBases (out base_type);
			if (partial_parts != null) {
				iface_exprs = GetNormalPartialBases (ref base_type);
			}

			//
			// GetClassBases calls ResolveBaseTypeExpr() on the various type expressions involved,
			// which in turn should have called DefineType()s on base types if necessary.
			//
			// None of the code below should trigger DefineType()s on classes that we depend on.
			// Thus, we are eligible to be on the topological sort `type_container_resolve_order'.
			//
			// Let's do it as soon as possible, since code below can call DefineType() on classes
			// that depend on us to be populated before they are.
			//
			if (!(this is CompilerGeneratedClass))
				RootContext.RegisterOrder (this); 

			if (IsGeneric && base_type != null && TypeManager.IsAttributeType (base_type.Type)) {
				Report.Error (698, base_type.Location,
					      "A generic type cannot derive from `{0}' because it is an attribute class",
					      base_type.Name);
				return false;
			}

			if (!CheckRecursiveDefinition (this))
				return false;

			if (base_type != null) {
				TypeBuilder.SetParent (base_type.Type);

				ObsoleteAttribute obsolete_attr = AttributeTester.GetObsoleteAttribute (base_type.Type);
				if (obsolete_attr != null && !IsInObsoleteScope)
					AttributeTester.Report_ObsoleteMessage (obsolete_attr, base_type.GetSignatureForError (), Location);
			}

			// add interfaces that were not added at type creation
			if (iface_exprs != null) {
				ifaces = TypeManager.ExpandInterfaces (iface_exprs);
				if (ifaces == null)
					return false;

				foreach (Type itype in ifaces)
 					TypeBuilder.AddInterfaceImplementation (itype);

				foreach (TypeExpr ie in iface_exprs) {
					ObsoleteAttribute oa = AttributeTester.GetObsoleteAttribute (ie.Type);
					if ((oa != null) && !IsInObsoleteScope)
						AttributeTester.Report_ObsoleteMessage (
							oa, ie.GetSignatureForError (), Location);
				}

				if (!CheckGenericInterfaces (ifaces))
					return false;

				TypeManager.RegisterBuilder (TypeBuilder, ifaces);
			}

			return true;
		}

		//
		// Defines the type in the appropriate ModuleBuilder or TypeBuilder.
		//
		public override TypeBuilder DefineType ()
		{
			if (TypeBuilder != null)
				return TypeBuilder;

			if (error)
				return null;
			
			if (!DefineTypeBuilder ()) {
				error = true;
				return null;
			}

			if (partial_parts != null) {
				foreach (TypeContainer part in partial_parts)
					part.TypeBuilder = TypeBuilder;
			}

			if (!(this is CompilerGeneratedClass)) {
				if (!ResolveMembers ()) {
					error = true;
					return null;
				}
			}

			if (!DefineNestedTypes ()) {
				error = true;
				return null;
			}

			return TypeBuilder;
		}

		public bool ResolveMembers ()
		{
			if (members_resolved)
				return members_resolved_ok;

			members_resolved_ok = DoResolveMembers ();
			members_resolved = true;

			return members_resolved_ok;
		}

		protected virtual bool DoResolveMembers ()
		{
			if (methods != null) {
				foreach (Method method in methods) {
					if (!method.ResolveMembers ())
						return false;
				}
			}

			if (instance_constructors != null) {
				foreach (Constructor c in instance_constructors) {
					if (!c.ResolveMembers ())
						return false;
				}
			}

			if (default_static_constructor != null) {
				if (!default_static_constructor.ResolveMembers ())
					return false;
			}

			if (operators != null) {
				foreach (Operator o in operators) {
					if (!o.ResolveMembers ())
						return false;
				}
			}

			if (properties != null) {
				foreach (PropertyBase p in properties) {
					if (!p.Get.IsDummy && !p.Get.ResolveMembers ())
						return false;
					if (!p.Set.IsDummy && !p.Set.ResolveMembers ())
						return false;
				}
			}

			if (indexers != null) {
				foreach (PropertyBase p in indexers) {
					if (!p.Get.IsDummy && !p.Get.ResolveMembers ())
						return false;
					if (!p.Set.IsDummy && !p.Set.ResolveMembers ())
						return false;
				}
			}

			return true;
		}

		Constraints [] constraints;
		public override void SetParameterInfo (ArrayList constraints_list)
		{
			if (PartialContainer == this) {
				base.SetParameterInfo (constraints_list);
				return;
			}

			if (constraints_list == null)
				return;

			constraints = new Constraints [PartialContainer.CountCurrentTypeParameters];

			TypeParameter[] current_params = PartialContainer.CurrentTypeParameters;
			for (int i = 0; i < constraints.Length; i++) {
				foreach (Constraints constraint in constraints_list) {
					if (constraint.TypeParameter == current_params [i].Name) {
						constraints [i] = constraint;
						break;
					}
				}
			}
		}

		bool UpdateTypeParameterConstraints ()
		{
			if (constraints == null)
				return true;

			TypeParameter[] current_params = PartialContainer.CurrentTypeParameters;
			for (int i = 0; i < current_params.Length; i++) {
				if (!current_params [i].UpdateConstraints (this, constraints [i])) {
					Report.SymbolRelatedToPreviousError (Location, "");
					Report.Error (265, PartialContainer.Location,
						"Partial declarations of `{0}' have inconsistent constraints for type parameter `{1}'",
						PartialContainer.GetSignatureForError (), current_params [i].Name);
					return false;
				}
			}

			return true;
		}

		public bool ResolveType ()
		{
			if (!DoResolveType ())
				return false;

			if (compiler_generated != null) {
				foreach (CompilerGeneratedClass c in compiler_generated)
					if (!c.ResolveType ())
						return false;
			}

			return true;
		}

		protected virtual bool DoResolveType ()
		{
			if ((base_type != null) &&
			    (base_type.ResolveAsTypeTerminal (this, false) == null)) {
				error = true;
				return false;
			}

			if (!IsGeneric)
				return true;

			if (PartialContainer != this)
				throw new InternalErrorException ();

			TypeExpr current_type = null;

			foreach (TypeParameter type_param in CurrentTypeParameters) {
				if (!type_param.Resolve (this)) {
					error = true;
					return false;
				}
			}

			if (partial_parts != null) {
				foreach (TypeContainer part in partial_parts) {
					if (!part.UpdateTypeParameterConstraints ()) {
						error = true;
						return false;
					}
				}
			}

			foreach (TypeParameter type_param in TypeParameters) {
				if (!type_param.DefineType (this)) {
					error = true;
					return false;
				}
			}

			current_type = new ConstructedType (TypeBuilder, TypeParameters, Location);

			foreach (TypeParameter type_param in TypeParameters)
				if (!type_param.CheckDependencies ()) {
					error = true;
					return false;
				}

			if (current_type != null) {
				current_type = current_type.ResolveAsTypeTerminal (this, false);
				if (current_type == null) {
					error = true;
					return false;
				}

				CurrentType = current_type.Type;
			}

			return true;
		}

		protected virtual bool DefineNestedTypes ()
		{
			if (Interfaces != null) {
				foreach (TypeContainer iface in Interfaces)
					if (iface.DefineType () == null)
						return false;
			}
			
			if (Types != null) {
				foreach (TypeContainer tc in Types)
					if (tc.DefineType () == null)
						return false;
			}

			if (Delegates != null) {
				foreach (Delegate d in Delegates)
					if (d.DefineType () == null)
						return false;
			}

			if (Enums != null) {
				foreach (Enum en in Enums)
					if (en.DefineType () == null)
						return false;
			}

			if (compiler_generated != null) {
				foreach (CompilerGeneratedClass c in compiler_generated) {
					if (c.DefineType () == null)
						return false;
				}
			}

			return true;
		}

		TypeContainer InTransit;

		protected bool CheckRecursiveDefinition (TypeContainer tc)
		{
			if (InTransit != null) {
				Report.SymbolRelatedToPreviousError (this);
				if (this is Interface)
					Report.Error (
						529, tc.Location, "Inherited interface `{0}' causes a " +
						"cycle in the interface hierarchy of `{1}'",
						GetSignatureForError (), tc.GetSignatureForError ());
				else
					Report.Error (
						146, tc.Location, "Circular base class dependency " +
						"involving `{0}' and `{1}'",
						tc.GetSignatureForError (), GetSignatureForError ());
				return false;
			}

			InTransit = tc;

			if (base_type != null) {
				Type t = TypeManager.DropGenericTypeArguments (base_type.Type);
				TypeContainer ptc = TypeManager.LookupTypeContainer (t);
				if ((ptc != null) && !ptc.CheckRecursiveDefinition (this))
					return false;
			}

			if (iface_exprs != null) {
				foreach (TypeExpr iface in iface_exprs) {
					Type itype = TypeManager.DropGenericTypeArguments (iface.Type);
					TypeContainer ptc = TypeManager.LookupTypeContainer (itype);
					if ((ptc != null) && !ptc.CheckRecursiveDefinition (this))
						return false;
				}
			}

			if (!IsTopLevel && !Parent.PartialContainer.CheckRecursiveDefinition (this))
				return false;

			InTransit = null;
			return true;
		}

		public static void Error_KeywordNotAllowed (Location loc)
		{
			Report.Error (1530, loc, "Keyword `new' is not allowed on namespace elements");
		}

		/// <summary>
		///   Populates our TypeBuilder with fields and methods
		/// </summary>
		public override bool DefineMembers ()
		{
			if (members_defined)
				return members_defined_ok;

			if (!base.DefineMembers ())
				return false;

			members_defined_ok = DoDefineMembers ();
			members_defined = true;

			return members_defined_ok;
		}

		protected virtual bool DoDefineMembers ()
		{
			if (iface_exprs != null) {
				foreach (TypeExpr iface in iface_exprs) {
					ConstructedType ct = iface as ConstructedType;
					if ((ct != null) && !ct.CheckConstraints (this))
						return false;
				}
			}

			if (base_type != null) {
				ConstructedType ct = base_type as ConstructedType;
				if ((ct != null) && !ct.CheckConstraints (this))
					return false;
			}

			if (!IsTopLevel) {
				MemberInfo conflict_symbol = Parent.PartialContainer.FindBaseMemberWithSameName (Basename, false);
				if (conflict_symbol == null) {
					if ((RootContext.WarningLevel >= 4) && ((ModFlags & Modifiers.NEW) != 0))
						Report.Warning (109, 4, Location, "The member `{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError ());
				} else {
					if ((ModFlags & Modifiers.NEW) == 0) {
						Report.SymbolRelatedToPreviousError (conflict_symbol);
						Report.Warning (108, 2, Location, "`{0}' hides inherited member `{1}'. Use the new keyword if hiding was intended",
							GetSignatureForError (), TypeManager.GetFullNameSignature (conflict_symbol));
					}
				}
			}

			DefineContainerMembers (constants);
			DefineContainerMembers (fields);

			if (Kind == Kind.Struct || Kind == Kind.Class) {
				pending = PendingImplementation.GetPendingImplementations (this);
			}
		
			//
			// Constructors are not in the defined_names array
			//
			DefineContainerMembers (instance_constructors);
		
			DefineContainerMembers (properties);
			DefineContainerMembers (events);
			DefineContainerMembers (indexers);
			DefineContainerMembers (methods);
			DefineContainerMembers (operators);
			DefineContainerMembers (enums);
			DefineContainerMembers (delegates);

			if (CurrentType != null) {
				GenericType = CurrentType;
			}


#if CACHE
			member_cache = new MemberCache (this);
			if (partial_parts != null) {
				foreach (TypeContainer part in partial_parts)
					part.member_cache = member_cache;
			}
#endif

			return true;
		}

		protected virtual void DefineContainerMembers (MemberCoreArrayList mcal)
		{
			if (mcal != null)
				mcal.DefineContainerMembers ();
		}

		public override bool Define ()
		{
			if (compiler_generated != null) {
				foreach (CompilerGeneratedClass c in compiler_generated) {
					if (!c.Define ())
						return false;
				}
			}

			return true;
		}

		public MemberInfo FindBaseMemberWithSameName (string name, bool ignore_methods)
		{
			return BaseCache == null ? null : BaseCache.FindMemberWithSameName (name, ignore_methods, null);
		}

		/// <summary>
		///   This function is based by a delegate to the FindMembers routine
		/// </summary>
		static bool AlwaysAccept (MemberInfo m, object filterCriteria)
		{
			return true;
		}

		/// <summary>
		///   This filter is used by FindMembers, and we just keep
		///   a global for the filter to `AlwaysAccept'
		/// </summary>
		static MemberFilter accepting_filter;

		
		static TypeContainer ()
		{
			accepting_filter = new MemberFilter (AlwaysAccept);
		}

		public MethodInfo[] GetMethods ()
		{
			ArrayList members = new ArrayList ();

			DefineMembers ();

			if (methods != null) {
				int len = methods.Count;
				for (int i = 0; i < len; i++) {
					Method m = (Method) methods [i];

					members.Add (m.MethodBuilder);
				}
			}

			if (operators != null) {
				int len = operators.Count;
				for (int i = 0; i < len; i++) {
					Operator o = (Operator) operators [i];

					members.Add (o.MethodBuilder);
				}
			}

			if (properties != null) {
				int len = properties.Count;
				for (int i = 0; i < len; i++) {
					Property p = (Property) properties [i];

					if (p.GetBuilder != null)
						members.Add (p.GetBuilder);
					if (p.SetBuilder != null)
						members.Add (p.SetBuilder);
				}
			}
				
			if (indexers != null) {
				int len = indexers.Count;
				for (int i = 0; i < len; i++) {
					Indexer ix = (Indexer) indexers [i];

					if (ix.GetBuilder != null)
						members.Add (ix.GetBuilder);
					if (ix.SetBuilder != null)
						members.Add (ix.SetBuilder);
				}
			}

			if (events != null) {
				int len = events.Count;
				for (int i = 0; i < len; i++) {
					Event e = (Event) events [i];

					if (e.AddBuilder != null)
						members.Add (e.AddBuilder);
					if (e.RemoveBuilder != null)
						members.Add (e.RemoveBuilder);
				}
			}

			MethodInfo[] retMethods = new MethodInfo [members.Count];
			members.CopyTo (retMethods, 0);
			return retMethods;
		}
		
		// Indicated whether container has StructLayout attribute set Explicit
		public bool HasExplicitLayout {
			get { return (caching_flags & Flags.HasExplicitLayout) != 0; }
			set { caching_flags |= Flags.HasExplicitLayout; }
		}

		//
		// Return the nested type with name @name.  Ensures that the nested type
		// is defined if necessary.  Do _not_ use this when you have a MemberCache handy.
		//
		public Type FindNestedType (string name)
		{
			if (PartialContainer != this)
				throw new InternalErrorException ("should not happen");

			ArrayList [] lists = { types, enums, delegates, interfaces };

			for (int j = 0; j < lists.Length; ++j) {
				ArrayList list = lists [j];
				if (list == null)
					continue;
				
				int len = list.Count;
				for (int i = 0; i < len; ++i) {
					DeclSpace ds = (DeclSpace) list [i];
					if (ds.Basename == name) {
						return ds.DefineType ();
					}
				}
			}

			return null;
		}

		private void FindMembers_NestedTypes (int modflags,
						      BindingFlags bf, MemberFilter filter, object criteria,
						      ref ArrayList members)
		{
			ArrayList [] lists = { types, enums, delegates, interfaces };

			for (int j = 0; j < lists.Length; ++j) {
				ArrayList list = lists [j];
				if (list == null)
					continue;
			
				int len = list.Count;
				for (int i = 0; i < len; i++) {
					DeclSpace ds = (DeclSpace) list [i];
					
					if ((ds.ModFlags & modflags) == 0)
						continue;
					
					TypeBuilder tb = ds.TypeBuilder;
					if (tb == null) {
						if (!(criteria is string) || ds.Basename.Equals (criteria))
							tb = ds.DefineType ();
					}
					
					if (tb != null && (filter (tb, criteria) == true)) {
						if (members == null)
							members = new ArrayList ();
						
						members.Add (tb);
					}
				}
			}
		}
		
		/// <summary>
		///   This method returns the members of this type just like Type.FindMembers would
		///   Only, we need to use this for types which are _being_ defined because MS' 
		///   implementation can't take care of that.
		/// </summary>
		//
		// FIXME: return an empty static array instead of null, that cleans up
		// some code and is consistent with some coding conventions I just found
		// out existed ;-)
		//
		//
		// Notice that in various cases we check if our field is non-null,
		// something that would normally mean that there was a bug elsewhere.
		//
		// The problem happens while we are defining p-invoke methods, as those
		// will trigger a FindMembers, but this happens before things are defined
		//
		// Since the whole process is a no-op, it is fine to check for null here.
		//
		public override MemberList FindMembers (MemberTypes mt, BindingFlags bf,
							MemberFilter filter, object criteria)
		{
			ArrayList members = null;

			int modflags = 0;
			if ((bf & BindingFlags.Public) != 0)
				modflags |= Modifiers.PUBLIC | Modifiers.PROTECTED |
					Modifiers.INTERNAL;
			if ((bf & BindingFlags.NonPublic) != 0)
				modflags |= Modifiers.PRIVATE;

			int static_mask = 0, static_flags = 0;
			switch (bf & (BindingFlags.Static | BindingFlags.Instance)) {
			case BindingFlags.Static:
				static_mask = static_flags = Modifiers.STATIC;
				break;

			case BindingFlags.Instance:
				static_mask = Modifiers.STATIC;
				static_flags = 0;
				break;

			default:
				static_mask = static_flags = 0;
				break;
			}

			Timer.StartTimer (TimerType.TcFindMembers);

			if (filter == null)
				filter = accepting_filter; 

			if ((mt & MemberTypes.Field) != 0) {
				if (fields != null) {
					int len = fields.Count;
					for (int i = 0; i < len; i++) {
						FieldBase f = (FieldBase) fields [i];
						
						if ((f.ModFlags & modflags) == 0)
							continue;
						if ((f.ModFlags & static_mask) != static_flags)
							continue;

						FieldBuilder fb = f.FieldBuilder;
						if (fb != null && filter (fb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (fb);
						}
					}
				}

				if (constants != null) {
					int len = constants.Count;
					for (int i = 0; i < len; i++) {
						Const con = (Const) constants [i];
						
						if ((con.ModFlags & modflags) == 0)
							continue;
						if ((con.ModFlags & static_mask) != static_flags)
							continue;

						FieldBuilder fb = con.FieldBuilder;
						if (fb == null) {
							if (con.Define ())
								fb = con.FieldBuilder;
						}
						if (fb != null && filter (fb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (fb);
						}
					}
				}
			}

			if ((mt & MemberTypes.Method) != 0) {
				if (methods != null) {
					int len = methods.Count;
					for (int i = 0; i < len; i++) {
						Method m = (Method) methods [i];
						
						if ((m.ModFlags & modflags) == 0)
							continue;
						if ((m.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder mb = m.MethodBuilder;

						if (mb != null && filter (mb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
                                                        members.Add (mb);
						}
					}
				}

				if (operators != null) {
					int len = operators.Count;
					for (int i = 0; i < len; i++) {
						Operator o = (Operator) operators [i];
						
						if ((o.ModFlags & modflags) == 0)
							continue;
						if ((o.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder ob = o.MethodBuilder;
						if (ob != null && filter (ob, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (ob);
						}
					}
				}

				if (events != null) {
					foreach (Event e in events) {
						if ((e.ModFlags & modflags) == 0)
							continue;
						if ((e.ModFlags & static_mask) != static_flags)
							continue;

						MethodBuilder b = e.AddBuilder;
						if (b != null && filter (b, criteria)) {
							if (members == null)
								members = new ArrayList (4);

							members.Add (b);
						}

						b = e.RemoveBuilder;
						if (b != null && filter (b, criteria)) {
							if (members == null) 
								members = new ArrayList (4);

							members.Add (b);
						}
					}
				}

				if (properties != null) {
					int len = properties.Count;
					for (int i = 0; i < len; i++) {
						Property p = (Property) properties [i];
						
						if ((p.ModFlags & modflags) == 0)
							continue;
						if ((p.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder b;

						b = p.GetBuilder;
						if (b != null && filter (b, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (b);
						}

						b = p.SetBuilder;
						if (b != null && filter (b, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (b);
						}
					}
				}
				
				if (indexers != null) {
					int len = indexers.Count;
					for (int i = 0; i < len; i++) {
						Indexer ix = (Indexer) indexers [i];
						
						if ((ix.ModFlags & modflags) == 0)
							continue;
						if ((ix.ModFlags & static_mask) != static_flags)
							continue;
						
						MethodBuilder b;

						b = ix.GetBuilder;
						if (b != null && filter (b, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (b);
						}

						b = ix.SetBuilder;
						if (b != null && filter (b, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (b);
						}
					}
				}
			}

			if ((mt & MemberTypes.Event) != 0) {
				if (events != null) {
					int len = events.Count;
					for (int i = 0; i < len; i++) {
						Event e = (Event) events [i];
						
						if ((e.ModFlags & modflags) == 0)
							continue;
						if ((e.ModFlags & static_mask) != static_flags)
							continue;

						MemberInfo eb = e.EventBuilder;
						if (eb != null && filter (eb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
						        members.Add (e.EventBuilder);
						}
					}
				}
			}
			
			if ((mt & MemberTypes.Property) != 0){
				if (properties != null) {
					int len = properties.Count;
					for (int i = 0; i < len; i++) {
						Property p = (Property) properties [i];
						
						if ((p.ModFlags & modflags) == 0)
							continue;
						if ((p.ModFlags & static_mask) != static_flags)
							continue;

						MemberInfo pb = p.PropertyBuilder;
						if (pb != null && filter (pb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (p.PropertyBuilder);
						}
					}
				}

				if (indexers != null) {
					int len = indexers.Count;
					for (int i = 0; i < len; i++) {
						Indexer ix = (Indexer) indexers [i];
						
						if ((ix.ModFlags & modflags) == 0)
							continue;
						if ((ix.ModFlags & static_mask) != static_flags)
							continue;

						MemberInfo ib = ix.PropertyBuilder;
						if (ib != null && filter (ib, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (ix.PropertyBuilder);
						}
					}
				}
			}
			
			if ((mt & MemberTypes.NestedType) != 0)
				FindMembers_NestedTypes (modflags, bf, filter, criteria, ref members);

			if ((mt & MemberTypes.Constructor) != 0){
				if (((bf & BindingFlags.Instance) != 0) && (instance_constructors != null)){
					int len = instance_constructors.Count;
					for (int i = 0; i < len; i++) {
						Constructor c = (Constructor) instance_constructors [i];
						
						ConstructorBuilder cb = c.ConstructorBuilder;
						if (cb != null && filter (cb, criteria) == true) {
							if (members == null)
								members = new ArrayList ();
							
							members.Add (cb);
						}
					}
				}

				if (((bf & BindingFlags.Static) != 0) && (default_static_constructor != null)){
					ConstructorBuilder cb =
						default_static_constructor.ConstructorBuilder;
					
					if (cb != null && filter (cb, criteria) == true) {
						if (members == null)
							members = new ArrayList ();
						
						members.Add (cb);
					}
				}
			}

			//
			// Lookup members in base if requested.
			//
			if ((bf & BindingFlags.DeclaredOnly) == 0) {
				if (TypeBuilder.BaseType != null) {
					MemberList list = FindMembers (TypeBuilder.BaseType, mt, bf, filter, criteria);
					if (list.Count > 0) {
						if (members == null)
							members = new ArrayList ();
					
						members.AddRange (list);
					}
				}
			}

			Timer.StopTimer (TimerType.TcFindMembers);

			if (members == null)
				return MemberList.Empty;
			else
				return new MemberList (members);
		}

		public override MemberCache MemberCache {
			get {
				return member_cache;
			}
		}

		public static MemberList FindMembers (Type t, MemberTypes mt, BindingFlags bf,
						      MemberFilter filter, object criteria)
		{
			DeclSpace ds = TypeManager.LookupDeclSpace (t);

			if (ds != null)
				return ds.FindMembers (mt, bf, filter, criteria);
			else
				return new MemberList (t.FindMembers (mt, bf, filter, criteria));
                }

		/// <summary>
		///   Emits the values for the constants
		/// </summary>
		public void EmitConstants ()
		{
			if (constants != null)
				foreach (Const con in constants)
					con.Emit ();
			return;
		}

		static void CheckMemberUsage (MemberCoreArrayList al, string member_type)
		{
			if (al == null)
				return;

			foreach (MemberCore mc in al) {
				if ((mc.ModFlags & Modifiers.Accessibility) != Modifiers.PRIVATE)
					continue;

				if (!mc.IsUsed) {
					Report.Warning (169, 3, mc.Location, "The private {0} `{1}' is never used", member_type, mc.GetSignatureForError ());
				}
			}
		}

		public virtual void VerifyMembers ()
		{
			//
			// Check for internal or private fields that were never assigned
			//
			if (RootContext.WarningLevel >= 3) {
				CheckMemberUsage (properties, "property");
				CheckMemberUsage (methods, "method");
				CheckMemberUsage (constants, "constant");

				if (fields != null){
					foreach (FieldBase f in fields) {
						if ((f.ModFlags & Modifiers.Accessibility) != Modifiers.PRIVATE)
							continue;
						
						if (!f.IsUsed){
							if ((f.caching_flags & Flags.IsAssigned) == 0)
								Report.Warning (169, 3, f.Location, "The private field `{0}' is never used", f.GetSignatureForError ());
							else {
#if NET_2_0
								const int error_code = 414;
#else
								const int error_code = 169;
#endif
								Report.Warning (error_code, 3, f.Location, "The private field `{0}' is assigned but its value is never used",
									f.GetSignatureForError ());
							}
							continue;
						}
						
						//
						// Only report 649 on level 4
						//
						if (RootContext.WarningLevel < 4)
							continue;
						
						if ((f.caching_flags & Flags.IsAssigned) != 0)
							continue;
						
						Constant c = New.Constantify (f.Type.Type);
						Report.Warning (649, 4, f.Location, "Field `{0}' is never assigned to, and will always have its default value `{1}'",
							f.GetSignatureForError (), c == null ? "null" : c.AsString ());
					}
				}
			}
		}

		// TODO: move to ClassOrStruct
		void EmitConstructors ()
		{
			if (instance_constructors == null)
				return;

			if (TypeBuilder.IsSubclassOf (TypeManager.attribute_type) && RootContext.VerifyClsCompliance && IsClsComplianceRequired ()) {
				bool has_compliant_args = false;

				foreach (Constructor c in instance_constructors) {
					c.Emit ();

					if (has_compliant_args)
						continue;

					has_compliant_args = c.HasCompliantArgs;
				}
				if (!has_compliant_args)
					Report.Error (3015, Location, "`{0}' has no accessible constructors which use only CLS-compliant types", GetSignatureForError ());
			} else {
				foreach (Constructor c in instance_constructors)
					c.Emit ();
			}
		}

		/// <summary>
		///   Emits the code, this step is performed after all
		///   the types, enumerations, constructors
		/// </summary>
		public virtual void EmitType ()
		{
			if (OptAttributes != null)
				OptAttributes.Emit ();

#if GMCS_SOURCE
			if (IsGeneric) {
				int offset = CountTypeParameters - CurrentTypeParameters.Length;
				for (int i = offset; i < gen_params.Length; i++)
					CurrentTypeParameters [i - offset].EmitAttributes ();
			}
#endif

			//
			// Structs with no fields need to have at least one byte.
			// The right thing would be to set the PackingSize in a DefineType
			// but there are no functions that allow interfaces *and* the size to
			// be specified.
			//

			if (Kind == Kind.Struct && first_nonstatic_field == null){
				FieldBuilder fb = TypeBuilder.DefineField ("$PRIVATE$", TypeManager.byte_type,
									   FieldAttributes.Private);

				if (HasExplicitLayout){
					object [] ctor_args = new object [1];
					ctor_args [0] = 0;
				
					CustomAttributeBuilder cba = new CustomAttributeBuilder (
						TypeManager.field_offset_attribute_ctor, ctor_args);
					fb.SetCustomAttribute (cba);
				}
			}

			Emit ();

			EmitConstructors ();

			// Can not continue if constants are broken
			EmitConstants ();
			if (Report.Errors > 0)
				return;

			if (default_static_constructor != null)
				default_static_constructor.Emit ();
			
			if (methods != null){
				foreach (Method m in methods)
					m.Emit ();
			}

			if (operators != null)
				foreach (Operator o in operators)
					o.Emit ();

			if (properties != null)
				foreach (Property p in properties)
					p.Emit ();

			if (indexers != null){
				indexers.Emit ();
			}
			
			if (fields != null)
				foreach (FieldBase f in fields)
					f.Emit ();

			if (events != null){
				foreach (Event e in Events)
					e.Emit ();
			}

			if (delegates != null) {
				foreach (Delegate d in Delegates) {
					d.Emit ();
				}
			}

			if (enums != null) {
				foreach (Enum e in enums) {
					e.Emit ();
				}
			}

			if (pending != null)
				if (pending.VerifyPendingMethods ())
					return;

			if (Report.Errors > 0)
				return;

			if (compiler_generated != null) {
				foreach (CompilerGeneratedClass c in compiler_generated) {
					if (!c.DefineMembers ())
						throw new InternalErrorException ();
				}
				foreach (CompilerGeneratedClass c in compiler_generated)
					c.EmitType ();
			}
		}
		
		public override void CloseType ()
		{
			if ((caching_flags & Flags.CloseTypeCreated) != 0)
				return;

			try {
				caching_flags |= Flags.CloseTypeCreated;
				TypeBuilder.CreateType ();
			} catch (TypeLoadException){
				//
				// This is fine, the code still created the type
				//
//				Report.Warning (-20, "Exception while creating class: " + TypeBuilder.Name);
//				Console.WriteLine (e.Message);
			} catch {
				Console.WriteLine ("In type: " + Name);
				throw;
			}
			
			if (Enums != null)
				foreach (Enum en in Enums)
					en.CloseType ();

			if (Types != null){
				foreach (TypeContainer tc in Types)
					if (tc.Kind == Kind.Struct)
						tc.CloseType ();

				foreach (TypeContainer tc in Types)
					if (tc.Kind != Kind.Struct)
						tc.CloseType ();
			}

			if (Delegates != null)
				foreach (Delegate d in Delegates)
					d.CloseType ();

			if (CompilerGenerated != null)
				foreach (CompilerGeneratedClass c in CompilerGenerated)
					c.CloseType ();
			
			types = null;
			properties = null;
			enums = null;
			delegates = null;
			fields = null;
			initialized_fields = null;
			initialized_static_fields = null;
			constants = null;
			interfaces = null;
			methods = null;
			events = null;
			indexers = null;
			operators = null;
			compiler_generated = null;
			default_constructor = null;
			default_static_constructor = null;
			type_bases = null;
			OptAttributes = null;
			ifaces = null;
			base_cache = null;
			member_cache = null;
		}

		//
		// Performs the validation on a Method's modifiers (properties have
		// the same properties).
		//
		public bool MethodModifiersValid (MemberCore mc)
		{
			const int vao = (Modifiers.VIRTUAL | Modifiers.ABSTRACT | Modifiers.OVERRIDE);
			const int va = (Modifiers.VIRTUAL | Modifiers.ABSTRACT);
			const int nv = (Modifiers.NEW | Modifiers.VIRTUAL);
			bool ok = true;
			int flags = mc.ModFlags;
			
			//
			// At most one of static, virtual or override
			//
			if ((flags & Modifiers.STATIC) != 0){
				if ((flags & vao) != 0){
					Report.Error (112, mc.Location, "A static member `{0}' cannot be marked as override, virtual or abstract",
						mc.GetSignatureForError ());
					ok = false;
				}
			}

			if (Kind == Kind.Struct){
				if ((flags & va) != 0){
					Modifiers.Error_InvalidModifier (mc.Location, "virtual or abstract");
					ok = false;
				}
			}

			if ((flags & Modifiers.OVERRIDE) != 0 && (flags & nv) != 0){
				Report.Error (113, mc.Location, "A member `{0}' marked as override cannot be marked as new or virtual",
					mc.GetSignatureForError ());
				ok = false;
			}

			//
			// If the declaration includes the abstract modifier, then the
			// declaration does not include static, virtual or extern
			//
			if ((flags & Modifiers.ABSTRACT) != 0){
				if ((flags & Modifiers.EXTERN) != 0){
					Report.Error (
						180, mc.Location, "`{0}' cannot be both extern and abstract", mc.GetSignatureForError ());
					ok = false;
				}

				if ((flags & Modifiers.SEALED) != 0) {
					Report.Error (502, mc.Location, "`{0}' cannot be both abstract and sealed", mc.GetSignatureForError ());
					ok = false;
				}

				if ((flags & Modifiers.VIRTUAL) != 0){
					Report.Error (503, mc.Location, "The abstract method `{0}' cannot be marked virtual", mc.GetSignatureForError ());
					ok = false;
				}

				if ((ModFlags & Modifiers.ABSTRACT) == 0){
					Report.Error (513, mc.Location, "`{0}' is abstract but it is contained in nonabstract class", mc.GetSignatureForError ());
					ok = false;
				}
			}

			if ((flags & Modifiers.PRIVATE) != 0){
				if ((flags & vao) != 0){
					Report.Error (621, mc.Location, "`{0}': virtual or abstract members cannot be private", mc.GetSignatureForError ());
					ok = false;
				}
			}

			if ((flags & Modifiers.SEALED) != 0){
				if ((flags & Modifiers.OVERRIDE) == 0){
					Report.Error (238, mc.Location, "`{0}' cannot be sealed because it is not an override", mc.GetSignatureForError ());
					ok = false;
				}
			}

			return ok;
		}

		public Constructor DefaultStaticConstructor {
			get { return default_static_constructor; }
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			VerifyClsName ();

			Type base_type = TypeBuilder.BaseType;
			if (base_type != null && !AttributeTester.IsClsCompliant (base_type)) {
				Report.Error (3009, Location, "`{0}': base type `{1}' is not CLS-compliant", GetSignatureForError (), TypeManager.CSharpName (base_type));
			}
			return true;
		}


		/// <summary>
		/// Checks whether container name is CLS Compliant
		/// </summary>
		void VerifyClsName ()
		{
			Hashtable base_members = base_cache == null ? 
				new Hashtable () :
				base_cache.GetPublicMembers ();
			Hashtable this_members = new Hashtable ();

			foreach (DictionaryEntry entry in defined_names) {
				MemberCore mc = (MemberCore)entry.Value;
				if (!mc.IsClsComplianceRequired ())
					continue;

				string name = (string) entry.Key;
				string basename = name.Substring (name.LastIndexOf ('.') + 1);

				string lcase = basename.ToLower (System.Globalization.CultureInfo.InvariantCulture);
				object found = base_members [lcase];
				if (found == null) {
					found = this_members [lcase];
					if (found == null) {
						this_members.Add (lcase, mc);
						continue;
					}
				}

				if ((mc.ModFlags & Modifiers.OVERRIDE) != 0)
					continue;					

				if (found is MemberInfo) {
					if (basename == ((MemberInfo) found).Name)
						continue;
					Report.SymbolRelatedToPreviousError ((MemberInfo) found);
				} else {
					Report.SymbolRelatedToPreviousError ((MemberCore) found);
				}
#if GMCS_SOURCE
				Report.Warning (3005, 1, mc.Location, "Identifier `{0}' differing only in case is not CLS-compliant", mc.GetSignatureForError ());
#else
				Report.Error (3005, mc.Location, "Identifier `{0}' differing only in case is not CLS-compliant", mc.GetSignatureForError ());
#endif
			}
		}


		/// <summary>
		///   Performs checks for an explicit interface implementation.  First it
		///   checks whether the `interface_type' is a base inteface implementation.
		///   Then it checks whether `name' exists in the interface type.
		/// </summary>
		public virtual bool VerifyImplements (InterfaceMemberBase mb)
		{
			if (ifaces != null) {
				foreach (Type t in ifaces){
					if (t == mb.InterfaceType)
						return true;
				}
			}
			
			Report.SymbolRelatedToPreviousError (mb.InterfaceType);
			Report.Error (540, mb.Location, "`{0}': containing type does not implement interface `{1}'",
				mb.GetSignatureForError (), TypeManager.CSharpName (mb.InterfaceType));
			return false;
		}

		public void Mark_HasEquals ()
		{
			Methods.HasEquals = true;
		}

		public void Mark_HasGetHashCode ()
		{
			Methods.HasGetHashCode = true;
		}

		//
		// IMemberContainer
		//

		string IMemberContainer.Name {
			get {
				return Name;
			}
		}

		Type IMemberContainer.Type {
			get {
				return TypeBuilder;
			}
		}

		MemberCache IMemberContainer.MemberCache {
			get {
				return member_cache;
			}
		}

		bool IMemberContainer.IsInterface {
			get {
				return Kind == Kind.Interface;
			}
		}

		MemberList IMemberContainer.GetMembers (MemberTypes mt, BindingFlags bf)
		{
			BindingFlags new_bf = bf | BindingFlags.DeclaredOnly;

			if (GenericType != null)
				return TypeManager.FindMembers (GenericType, mt, new_bf,
								null, null);
			else
				return FindMembers (mt, new_bf, null, null);
		}

		//
		// Generates xml doc comments (if any), and if required,
		// handle warning report.
		//
		internal override void GenerateDocComment (DeclSpace ds)
		{
			DocUtil.GenerateTypeDocComment (this, ds);
		}

		public override string DocCommentHeader {
			get { return "T:"; }
		}

		public virtual MemberCache BaseCache {
			get {
				if (base_cache != null)
					return base_cache;
				if (TypeBuilder.BaseType != null)
					base_cache = TypeManager.LookupMemberCache (TypeBuilder.BaseType);
				if (TypeBuilder.IsInterface)
					base_cache = TypeManager.LookupBaseInterfacesCache (TypeBuilder);
				return base_cache;
			}
		}
	}

	public abstract class ClassOrStruct : TypeContainer {
		ListDictionary declarative_security;

		public ClassOrStruct (NamespaceEntry ns, DeclSpace parent,
				      MemberName name, Attributes attrs, Kind kind)
			: base (ns, parent, name, attrs, kind)
		{
		}

		protected override bool AddToContainer (MemberCore symbol, string name)
		{
			if (name == MemberName.Name) {
				if (symbol is TypeParameter) {
					Report.Error (694, symbol.Location,
						      "Type parameter `{0}' has same name as " +
						      "containing type, or method", name);
					return false;
				}

				Report.SymbolRelatedToPreviousError (this);
				Report.Error (542, symbol.Location, "`{0}': member names cannot be the same as their enclosing type",
					symbol.GetSignatureForError ());
				return false;
			}

			return base.AddToContainer (symbol, name);
		}

		public override void VerifyMembers ()
		{
			base.VerifyMembers ();

			if ((events != null) && (RootContext.WarningLevel >= 3)) {
				foreach (Event e in events){
					if ((e.ModFlags & Modifiers.Accessibility) != Modifiers.PRIVATE)
						continue;

					if ((e.caching_flags & Flags.IsUsed) == 0)
						Report.Warning (67, 3, e.Location, "The event `{0}' is never used", e.GetSignatureForError ());
				}
			}
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type.IsSubclassOf (TypeManager.security_attr_type) && a.CheckSecurityActionValidity (false)) {
				if (declarative_security == null)
					declarative_security = new ListDictionary ();

				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (a.Type == TypeManager.struct_layout_attribute_type && a.GetLayoutKindValue () == LayoutKind.Explicit) {
				HasExplicitLayout = true;
			}

			base.ApplyAttributeBuilder (a, cb);
		}

		/// <summary>
		/// Defines the default constructors 
		/// </summary>
		protected void DefineDefaultConstructor (bool is_static)
		{
			// The default instance constructor is public
			// If the class is abstract, the default constructor is protected
			// The default static constructor is private

			int mods;
			if (is_static) {
				mods = Modifiers.STATIC | Modifiers.PRIVATE;
			} else {
				mods = ((ModFlags & Modifiers.ABSTRACT) != 0) ? Modifiers.PROTECTED : Modifiers.PUBLIC;
			}

			Constructor c = new Constructor (this, MemberName.Name, mods,
				Parameters.EmptyReadOnlyParameters,
				new GeneratedBaseInitializer (Location),
				Location);
			
			AddConstructor (c);
			c.Block = new ToplevelBlock (null, Location);
		}

		void DefineFieldInitializers ()
		{
			if (initialized_fields != null) {
				EmitContext ec = new EmitContext (this, this, Location, null, null, ModFlags);
				ec.IsFieldInitializer = true;

				for (int i = 0; i < initialized_fields.Count; ++i) {
					FieldInitializer fi = (FieldInitializer)initialized_fields[i];
					fi.ResolveStatement (ec);
					if (fi.IsDefaultInitializer && RootContext.Optimize) {
						// Field is re-initialized to its default value => removed
						initialized_fields.RemoveAt (i);
						--i;
					}
				}
			}

			if (initialized_static_fields != null) {
				bool has_complex_initializer = false;
				EmitContext ec = new EmitContext (this, this, Location, null, null, ModFlags);
				ec.IsStatic = true;
				ec.IsFieldInitializer = true;

				foreach (FieldInitializer fi in initialized_static_fields) {
					fi.ResolveStatement (ec);
					if (!fi.IsComplexInitializer)
						continue;

					has_complex_initializer = true;
				}

				// Need special check to not optimize code like this
				// static int a = b = 5;
				// static int b = 0;
				if (!has_complex_initializer && RootContext.Optimize) {
					for (int i = 0; i < initialized_static_fields.Count; ++i) {
						FieldInitializer fi = (FieldInitializer)initialized_static_fields[i];
						if (fi.IsDefaultInitializer) {
							initialized_static_fields.RemoveAt (i);
							--i;
						}
					}
				}

				if (default_static_constructor == null && initialized_static_fields.Count > 0) {
					DefineDefaultConstructor (true);
				}
			}

		}

		public override bool Define ()
		{
			DefineFieldInitializers ();

			if (default_static_constructor != null)
				default_static_constructor.Define ();

			return base.Define ();
		}

		public override void Emit ()
		{
			base.Emit ();

			if (declarative_security != null) {
				foreach (DictionaryEntry de in declarative_security) {
					TypeBuilder.AddDeclarativeSecurity ((SecurityAction)de.Key, (PermissionSet)de.Value);
				}
			}
		}

		protected override TypeAttributes TypeAttr {
			get {
				if (default_static_constructor == null)
					return base.TypeAttr | TypeAttributes.BeforeFieldInit;

				return base.TypeAttr;
			}
		}
	}


	// TODO: should be sealed
	public class Class : ClassOrStruct {
		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.ABSTRACT |
			Modifiers.SEALED |
			Modifiers.STATIC |
			Modifiers.UNSAFE;

		public Class (NamespaceEntry ns, DeclSpace parent, MemberName name, int mod,
			      Attributes attrs)
			: base (ns, parent, name, attrs, Kind.Class)
		{
			int accmods = Parent.Parent == null ? Modifiers.INTERNAL : Modifiers.PRIVATE;
			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, Location);

			if (IsStatic && RootContext.Version == LanguageVersion.ISO_1) {
				Report.FeatureIsNotStandardized (Location, "static classes");
			}
		}

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.attribute_usage_type) {
				if (BaseType != TypeManager.attribute_type && !BaseType.IsSubclassOf (TypeManager.attribute_type) &&
					TypeBuilder.FullName != "System.Attribute") {
					Report.Error (641, a.Location, "Attribute `{0}' is only valid on classes derived from System.Attribute", a.GetSignatureForError ());
				}
			}

			if (a.Type == TypeManager.conditional_attribute_type &&
				!(BaseType == TypeManager.attribute_type || BaseType.IsSubclassOf (TypeManager.attribute_type))) {
				Report.Error (1689, a.Location, "Attribute `System.Diagnostics.ConditionalAttribute' is only valid on methods or attribute classes");
				return;
			}

			if (a.Type == TypeManager.comimport_attr_type &&
				!attributes.Contains (TypeManager.guid_attr_type)) {
					a.Error_MissingGuidAttribute ();
					return;
			}

			if (AttributeTester.IsAttributeExcluded (a.Type))
				return;

			base.ApplyAttributeBuilder (a, cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Class;
			}
		}

		protected override void DefineContainerMembers (MemberCoreArrayList list)
		{
			if (list == null)
				return;

			if (!IsStatic) {
				base.DefineContainerMembers (list);
				return;
			}

			foreach (MemberCore m in list) {
				if (m is Operator) {
					Report.Error (715, m.Location, "`{0}': Static classes cannot contain user-defined operators", m.GetSignatureForError ());
					continue;
				}

				if (m is Destructor) {
					Report.Error (711, m.Location, "`{0}': Static classes cannot contain destructor", GetSignatureForError ());
					continue;
				}

				if ((m.ModFlags & Modifiers.PROTECTED) != 0) {
					Report.Error (1057, m.Location, "`{0}': Static classes cannot contain protected members", m.GetSignatureForError ());
					continue;
				}

				if (m is Indexer) {
					Report.Error (720, m.Location, "`{0}': cannot declare indexers in a static class", m.GetSignatureForError ());
					continue;
				}

				if ((m.ModFlags & Modifiers.STATIC) != 0 || m is Enum || m is Delegate)
					continue;

				if (m is Constructor) {
					Report.Error (710, m.Location, "`{0}': Static classes cannot have instance constructors", GetSignatureForError ());
					continue;
				}

				Report.Error (708, m.Location, "`{0}': cannot declare instance members in a static class", m.GetSignatureForError ());
			}

			base.DefineContainerMembers (list);
		}

		public override TypeBuilder DefineType ()
		{
			if ((ModFlags & Modifiers.ABSTRACT) == Modifiers.ABSTRACT && (ModFlags & (Modifiers.SEALED | Modifiers.STATIC)) != 0) {
				Report.Error (418, Location, "`{0}': an abstract class cannot be sealed or static", GetSignatureForError ());
				return null;
			}

			if ((ModFlags & (Modifiers.SEALED | Modifiers.STATIC)) == (Modifiers.SEALED | Modifiers.STATIC)) {
				Report.Error (441, Location, "`{0}': a class cannot be both static and sealed", GetSignatureForError ());
				return null;
			}

			return base.DefineType ();
		}

		protected override bool DoDefineMembers ()
		{
			if (InstanceConstructors == null && !IsStatic)
				DefineDefaultConstructor (false);

			return base.DoDefineMembers ();
		}

		public override TypeExpr[] GetClassBases (out TypeExpr base_class)
		{
			TypeExpr[] ifaces = base.GetClassBases (out base_class);

			if (base_class == null) {
				if (RootContext.StdLib)
					base_class = TypeManager.system_object_expr;
				else if (Name != "System.Object")
					base_class = TypeManager.system_object_expr;
			} else {
				if (Kind == Kind.Class && base_class is TypeParameterExpr){
					Report.Error (
						689, base_class.Location,
						"Cannot derive from `{0}' because it is a type parameter",
						base_class.GetSignatureForError ());
					return ifaces;
				}

				if (base_class.Type.IsArray || base_class.Type.IsPointer) {
					Report.Error (1521, base_class.Location, "Invalid base type");
					return ifaces;
				}

				if (base_class.IsSealed){
					Report.SymbolRelatedToPreviousError (base_class.Type);
					if (base_class.Type.IsAbstract) {
						Report.Error (709, Location, "`{0}': Cannot derive from static class `{1}'",
							GetSignatureForError (), TypeManager.CSharpName (base_class.Type));
					} else {
						Report.Error (509, Location, "`{0}': cannot derive from sealed class `{1}'",
							GetSignatureForError (), TypeManager.CSharpName (base_class.Type));
					}
					return ifaces;
				}

				if (!base_class.CanInheritFrom ()){
					Report.Error (644, Location, "`{0}' cannot derive from special class `{1}'",
						GetSignatureForError (), base_class.GetSignatureForError ());
					return ifaces;
				}

				if (!base_class.AsAccessible (this, ModFlags)) {
					Report.SymbolRelatedToPreviousError (base_class.Type);
					Report.Error (60, Location, "Inconsistent accessibility: base class `{0}' is less accessible than class `{1}'", 
						TypeManager.CSharpName (base_class.Type), GetSignatureForError ());
				}
			}

			if (IsStatic) {
				if (base_class != TypeManager.system_object_expr) {
					Report.Error (713, Location, "Static class `{0}' cannot derive from type `{1}'. Static classes must derive from object",
						GetSignatureForError (), base_class.GetSignatureForError ());
					return ifaces;
				}

				if (ifaces != null) {
					foreach (TypeExpr t in ifaces)
						Report.SymbolRelatedToPreviousError (t.Type);
					Report.Error (714, Location, "`{0}': static classes cannot implement interfaces", GetSignatureForError ());
				}
			}

			return ifaces;
		}

		/// Search for at least one defined condition in ConditionalAttribute of attribute class
		/// Valid only for attribute classes.
		public bool IsExcluded ()
		{
			if ((caching_flags & Flags.Excluded_Undetected) == 0)
				return (caching_flags & Flags.Excluded) != 0;

			caching_flags &= ~Flags.Excluded_Undetected;

			if (OptAttributes == null)
				return false;

			Attribute[] attrs = OptAttributes.SearchMulti (TypeManager.conditional_attribute_type);

			if (attrs == null)
				return false;

			foreach (Attribute a in attrs) {
				string condition = a.GetConditionalAttributeValue ();
				if (RootContext.AllDefines.Contains (condition))
					return false;
			}

			caching_flags |= Flags.Excluded;
			return true;
		}

		bool IsStatic {
			get {
				return (ModFlags & Modifiers.STATIC) != 0;
			}
		}

		//
		// FIXME: How do we deal with the user specifying a different
		// layout?
		//
		protected override TypeAttributes TypeAttr {
			get {
				TypeAttributes ta = base.TypeAttr | TypeAttributes.AutoLayout | TypeAttributes.Class;
				if (IsStatic)
					ta |= TypeAttributes.Abstract | TypeAttributes.Sealed;
				return ta;
			}
		}
	}

	public sealed class Struct : ClassOrStruct {
		// <summary>
		//   Modifiers allowed in a struct declaration
		// </summary>
		const int AllowedModifiers =
			Modifiers.NEW       |
			Modifiers.PUBLIC    |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL  |
			Modifiers.UNSAFE    |
			Modifiers.PRIVATE;

		public Struct (NamespaceEntry ns, DeclSpace parent, MemberName name,
			       int mod, Attributes attrs)
			: base (ns, parent, name, attrs, Kind.Struct)
		{
			int accmods;
			
			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;
			
			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, Location);

			this.ModFlags |= Modifiers.SEALED;
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Struct;
			}
		}

		const TypeAttributes DefaultTypeAttributes =
			TypeAttributes.SequentialLayout |
			TypeAttributes.Sealed |
			TypeAttributes.BeforeFieldInit;


		public override TypeExpr[] GetClassBases (out TypeExpr base_class)
		{
			TypeExpr[] ifaces = base.GetClassBases (out base_class);
			//
			// If we are compiling our runtime,
			// and we are defining ValueType, then our
			// base is `System.Object'.
			//
			if (base_class == null) {
				if (!RootContext.StdLib && Name == "System.ValueType")
					base_class = TypeManager.system_object_expr;
				else
					base_class = TypeManager.system_valuetype_expr;
			}

			return ifaces;
		}

		//
		// FIXME: Allow the user to specify a different set of attributes
		// in some cases (Sealed for example is mandatory for a class,
		// but what SequentialLayout can be changed
		//
		protected override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr | DefaultTypeAttributes;
			}
		}

		public override void RegisterFieldForInitialization (MemberCore field, FieldInitializer expression)
		{
			if ((field.ModFlags & Modifiers.STATIC) == 0) {
				Report.Error (573, field.Location, "`{0}': Structs cannot have instance field initializers",
					field.GetSignatureForError ());
				return;
			}
			base.RegisterFieldForInitialization (field, expression);
		}

	}

	/// <summary>
	///   Interfaces
	/// </summary>
	public sealed class Interface : TypeContainer, IMemberContainer {

		/// <summary>
		///   Modifiers allowed in a class declaration
		/// </summary>
		public const int AllowedModifiers =
			Modifiers.NEW       |
			Modifiers.PUBLIC    |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL  |
		 	Modifiers.UNSAFE    |
			Modifiers.PRIVATE;

		public Interface (NamespaceEntry ns, DeclSpace parent, MemberName name, int mod,
				  Attributes attrs)
			: base (ns, parent, name, attrs, Kind.Interface)
		{
			int accmods;

			if (parent.Parent == null)
				accmods = Modifiers.INTERNAL;
			else
				accmods = Modifiers.PRIVATE;

			this.ModFlags = Modifiers.Check (AllowedModifiers, mod, accmods, name.Location);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.comimport_attr_type &&
				!attributes.Contains (TypeManager.guid_attr_type)) {
					a.Error_MissingGuidAttribute ();
					return;
			}
			base.ApplyAttributeBuilder (a, cb);
		}


		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Interface;
			}
		}

		const TypeAttributes DefaultTypeAttributes =
			TypeAttributes.AutoLayout |
			TypeAttributes.Abstract |
			TypeAttributes.Interface;

		protected override TypeAttributes TypeAttr {
			get {
				return base.TypeAttr | DefaultTypeAttributes;
			}
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if (ifaces != null) {
				foreach (Type t in ifaces) {
					if (AttributeTester.IsClsCompliant (t))
						continue;

					Report.SymbolRelatedToPreviousError (t);
					Report.Warning (3027, 1, Location, "`{0}' is not CLS-compliant because base interface `{1}' is not CLS-compliant",
						GetSignatureForError (), TypeManager.CSharpName (t));
				}
			}

			return true;
		}
	}

	// It is used as a base class for all property based members
	// This includes properties, indexers, and events
	public abstract class PropertyBasedMember : InterfaceMemberBase
	{
		public PropertyBasedMember (DeclSpace parent, GenericMethod generic,
			Expression type, int mod, int allowed_mod, bool is_iface,
			MemberName name, Attributes attrs)
			: base (parent, generic, type, mod, allowed_mod, is_iface, name, attrs)
		{
		}

		protected override bool CheckForDuplications ()
		{
			throw new NotSupportedException ();
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if (!AttributeTester.IsClsCompliant (MemberType)) {
				Report.Error (3003, Location, "Type of `{0}' is not CLS-compliant",
					GetSignatureForError ());
			}
			return true;
		}

	}


	public abstract class MethodCore : InterfaceMemberBase
	{
		public readonly Parameters Parameters;
		protected ToplevelBlock block;

		public MethodCore (DeclSpace parent, GenericMethod generic,
			Expression type, int mod, int allowed_mod, bool is_iface,
			MemberName name, Attributes attrs, Parameters parameters)
			: base (parent, generic, type, mod, allowed_mod, is_iface, name, attrs)
		{
			Parameters = parameters;
		}

		//
		//  Returns the System.Type array for the parameters of this method
		//
		public Type [] ParameterTypes {
			get {
				return Parameters.Types;
			}
		}

		public Parameters ParameterInfo
		{
			get {
				return Parameters;
			}
		}
		
		public ToplevelBlock Block {
			get {
				return block;
			}

			set {
				block = value;
			}
		}

		protected bool DoDefineParameters ()
		{
			IResolveContext rc = GenericMethod == null ? this : (IResolveContext)ds;

			// Check if arguments were correct
			if (!Parameters.Resolve (rc))
				return false;

			return CheckParameters (Parameters);
		}

		protected override bool CheckBase ()
		{
			// Check whether arguments were correct.
			if (!DoDefineParameters ())
				return false;

			if (!base.CheckBase ())
				return false;

			return true;
		}

		// TODO: create a special method for operators only to make code better
		protected bool IsDuplicateImplementation (MethodCore method)
		{
			if (method == this)
				return false;

			Operator op2 = null;
			Operator op1 = null;

			if (!(method.MemberName.Equals (MemberName)))
			{
				op1 = this as Operator;
				if (op1 == null || !(op1.OperatorType == Operator.OpType.Explicit || op1.OperatorType == Operator.OpType.Implicit))
					return false;

				op2 = method as Operator;
				if (op2 == null || !(op2.OperatorType == Operator.OpType.Explicit || op2.OperatorType == Operator.OpType.Implicit))
					return false;
			} else {
				op1 = this as Operator;
				op2 = method as Operator;
			}

			Type[] param_types = method.ParameterTypes;
			// This never happen. Rewrite this as Equal
			if (param_types == null && ParameterTypes == null)
				return true;
			if (param_types == null || ParameterTypes == null)
				return false;

			if (param_types.Length != ParameterTypes.Length)
				return false;

			if (method.Parameters.HasArglist != Parameters.HasArglist)
				return false;
			
			bool equal = true;

			for (int i = 0; i < param_types.Length; i++) {
				if (param_types [i] != ParameterTypes [i])
					equal = false;
			}

			if (IsExplicitImpl && (method.InterfaceType != InterfaceType))
				equal = false;

			// TODO: make operator compatible with MethodCore to avoid this
			if (op1 != null && op2 != null) {
				if (MemberType != method.MemberType)
					equal = false;
			}

			if (equal) {
				//
				// Try to report 663: method only differs on out/ref
				//
				Parameters info = ParameterInfo;
				Parameters other_info = method.ParameterInfo;
				for (int i = 0; i < info.Count; i++){
					if (info.ParameterModifier (i) != other_info.ParameterModifier (i)){
						Report.SymbolRelatedToPreviousError (method);
						Report.Error (663, Location, "`{0}': Methods cannot differ only on their use of ref and out on a parameters",
							      GetSignatureForError ());
						return false;
					}
				}

				Report.SymbolRelatedToPreviousError (method);
				if (this is Operator && method is Operator)
					Report.Error (557, Location, "Duplicate user-defined conversion in type `{0}'", Parent.Name);
				else
					Report.Error (111, Location, TypeContainer.Error111, GetSignatureForError ());

				return true;
			}

			return false;
		}

		//
		// Returns a string that represents the signature for this 
		// member which should be used in XML documentation.
		//
		public override string GetDocCommentName (DeclSpace ds)
		{
			return DocUtil.GetMethodDocCommentName (this, Parameters, ds);
		}

		//
		// Raised (and passed an XmlElement that contains the comment)
		// when GenerateDocComment is writing documentation expectedly.
		//
		// FIXME: with a few effort, it could be done with XmlReader,
		// that means removal of DOM use.
		//
		internal override void OnGenerateDocComment (XmlElement el)
		{
			DocUtil.OnMethodGenerateDocComment (this, el);
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader 
		{
			get { return "M:"; }
		}

		public virtual void SetYields ()
		{
			ModFlags |= Modifiers.METHOD_YIELDS;
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if (Parameters.HasArglist) {
				Report.Error (3000, Location, "Methods with variable arguments are not CLS-compliant");
			}

			if (!AttributeTester.IsClsCompliant (MemberType)) {
				Report.Error (3002, Location, "Return type of `{0}' is not CLS-compliant",
					GetSignatureForError ());
			}

			Parameters.VerifyClsCompliance ();
			return true;
		}

	}

	public abstract class InterfaceMemberBase : MemberBase {
		//
		// Whether this is an interface member.
		//
		public bool IsInterface;

		//
		// If true, this is an explicit interface implementation
		//
		public bool IsExplicitImpl;

		//
		// The interface type we are explicitly implementing
		//
		public Type InterfaceType;

		//
		// The method we're overriding if this is an override method.
		//
		protected MethodInfo base_method;

		readonly int explicit_mod_flags;
		public MethodAttributes flags;

		public InterfaceMemberBase (DeclSpace parent, GenericMethod generic,
				   Expression type, int mod, int allowed_mod, bool is_iface,
				   MemberName name, Attributes attrs)
			: base (parent, generic, type, mod, allowed_mod, Modifiers.PRIVATE,
				name, attrs)
		{
			IsInterface = is_iface;
			IsExplicitImpl = (MemberName.Left != null);
			explicit_mod_flags = mod;
		}
		
		protected override bool CheckBase ()
		{
			if (!base.CheckBase ())
				return false;
			
			if ((caching_flags & Flags.TestMethodDuplication) != 0 && !CheckForDuplications ())
				return false;

			if (IsExplicitImpl)
				return true;

			// Is null for System.Object while compiling corlib and base interfaces
			if (Parent.PartialContainer.BaseCache == null) {
				if ((ModFlags & Modifiers.NEW) != 0) {
					Report.Warning (109, 4, Location, "The member `{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError ());
				}
				return true;
			}

			Type base_ret_type = null;
			base_method = FindOutBaseMethod (ref base_ret_type);

			// method is override
			if (base_method != null) {
				if (!CheckMethodAgainstBase (base_ret_type))
					return false;

				if ((ModFlags & Modifiers.OVERRIDE) != 0) {
					ObsoleteAttribute oa = AttributeTester.GetMethodObsoleteAttribute (base_method);
					if (oa != null) {
						if (OptAttributes == null || !OptAttributes.Contains (TypeManager.obsolete_attribute_type)) {
							Report.SymbolRelatedToPreviousError (base_method);
								Report.Warning (672, 1, Location, "Member `{0}' overrides obsolete member `{1}'. Add the Obsolete attribute to `{0}'",
									GetSignatureForError (), TypeManager.CSharpSignature (base_method));
						}
					} else {
						if (OptAttributes != null && OptAttributes.Contains (TypeManager.obsolete_attribute_type)) {
							Report.Warning (809, 1, Location, "Obsolete member `{0}' overrides non-obsolete member `{1}'",
								GetSignatureForError (), TypeManager.CSharpSignature (base_method));
						}
					}
				}
				return true;
			}

			MemberInfo conflict_symbol = Parent.PartialContainer.FindBaseMemberWithSameName (Name, !((this is Event) || (this is Property)));
			if ((ModFlags & Modifiers.OVERRIDE) != 0) {
				if (conflict_symbol != null) {
					Report.SymbolRelatedToPreviousError (conflict_symbol);
					if (this is Event)
						Report.Error (72, Location, "`{0}': cannot override because `{1}' is not an event", GetSignatureForError (), TypeManager.GetFullNameSignature (conflict_symbol));
					else if (this is PropertyBase)
						Report.Error (544, Location, "`{0}': cannot override because `{1}' is not a property", GetSignatureForError (), TypeManager.GetFullNameSignature (conflict_symbol));
					else
						Report.Error (505, Location, "`{0}': cannot override because `{1}' is not a method", GetSignatureForError (), TypeManager.GetFullNameSignature (conflict_symbol));
				} else {
					Report.Error (115, Location, "`{0}' is marked as an override but no suitable {1} found to override",
						GetSignatureForError (), SimpleName.GetMemberType (this));
				}
				return false;
			}

			if (conflict_symbol == null) {
				if ((RootContext.WarningLevel >= 4) && ((ModFlags & Modifiers.NEW) != 0)) {
					Report.Warning (109, 4, Location, "The member `{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError ());
				}
				return true;
			}

			if ((ModFlags & Modifiers.NEW) == 0) {
				if (this is Method && conflict_symbol is MethodBase)
					return true;

				Report.SymbolRelatedToPreviousError (conflict_symbol);
				Report.Warning (108, 2, Location, "`{0}' hides inherited member `{1}'. Use the new keyword if hiding was intended",
					GetSignatureForError (), TypeManager.GetFullNameSignature (conflict_symbol));
			}

			return true;
		}

		//
		// Performs various checks on the MethodInfo `mb' regarding the modifier flags
		// that have been defined.
		//
		// `name' is the user visible name for reporting errors (this is used to
		// provide the right name regarding method names and properties)
		//
		bool CheckMethodAgainstBase (Type baseMethodType)
		{
			bool ok = true;

			if ((ModFlags & Modifiers.OVERRIDE) != 0){
				if (!(base_method.IsAbstract || base_method.IsVirtual)){
					Report.Error (506, Location,
						"`{0}': cannot override inherited member `{1}' because it is not marked virtual, abstract or override",
						 GetSignatureForError (), TypeManager.CSharpSignature (base_method));
					ok = false;
				}
				
				// Now we check that the overriden method is not final
				
				if (base_method.IsFinal) {
					Report.SymbolRelatedToPreviousError (base_method);
					Report.Error (239, Location, "`{0}': cannot override inherited member `{1}' because it is sealed",
							      GetSignatureForError (), TypeManager.CSharpSignature (base_method));
					ok = false;
				}
				//
				// Check that the permissions are not being changed
				//
				MethodAttributes thisp = flags & MethodAttributes.MemberAccessMask;
				MethodAttributes base_classp = base_method.Attributes & MethodAttributes.MemberAccessMask;

				if (!CheckAccessModifiers (thisp, base_classp, base_method)) {
					Error_CannotChangeAccessModifiers (base_method, base_classp, null);
					ok = false;
				}

				if (!TypeManager.IsEqual (MemberType, TypeManager.TypeToCoreType (baseMethodType))) {
					Report.SymbolRelatedToPreviousError (base_method);
					if (this is PropertyBasedMember) {
						Report.Error (1715, Location, "`{0}': type must be `{1}' to match overridden member `{2}'", 
							GetSignatureForError (), TypeManager.CSharpName (baseMethodType), TypeManager.CSharpSignature (base_method));
					}
					else {
						Report.Error (508, Location, "`{0}': return type must be `{1}' to match overridden member `{2}'",
							GetSignatureForError (), TypeManager.CSharpName (baseMethodType), TypeManager.CSharpSignature (base_method));
					}
					ok = false;
				}
			}

			if ((ModFlags & Modifiers.NEW) == 0) {
				if ((ModFlags & Modifiers.OVERRIDE) == 0 && Name != "Finalize") {
					ModFlags |= Modifiers.NEW;
					Report.SymbolRelatedToPreviousError (base_method);
					if (!IsInterface && (base_method.IsVirtual || base_method.IsAbstract)) {
						Report.Warning (114, 2, Location, "`{0}' hides inherited member `{1}'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword",
							GetSignatureForError (), TypeManager.CSharpSignature (base_method));
					} else {
						Report.Warning (108, 2, Location, "`{0}' hides inherited member `{1}'. Use the new keyword if hiding was intended",
							GetSignatureForError (), TypeManager.CSharpSignature (base_method));
					}
				}
			} else {
				if (base_method.IsAbstract && !IsInterface) {
					Report.SymbolRelatedToPreviousError (base_method);
					Report.Error (533, Location, "`{0}' hides inherited abstract member `{1}'",
						GetSignatureForError (), TypeManager.CSharpSignature (base_method));
					return ok = false;
				}
			}

			return ok;
		}
		
		protected bool CheckAccessModifiers (MethodAttributes thisp, MethodAttributes base_classp, MethodInfo base_method)
		{
			if ((base_classp & MethodAttributes.FamORAssem) == MethodAttributes.FamORAssem){
				//
				// when overriding protected internal, the method can be declared
				// protected internal only within the same assembly
				//

				if ((thisp & MethodAttributes.FamORAssem) == MethodAttributes.FamORAssem){
					if (Parent.TypeBuilder.Assembly != base_method.DeclaringType.Assembly){
						//
						// assemblies differ - report an error
						//
						
						return false;
					} else if (thisp != base_classp) {
						//
						// same assembly, but other attributes differ - report an error
						//
						
						return false;
					};
				} else if ((thisp & MethodAttributes.Family) != MethodAttributes.Family) {
					//
					// if it's not "protected internal", it must be "protected"
					//

					return false;
				} else if (Parent.TypeBuilder.Assembly == base_method.DeclaringType.Assembly) {
					//
					// protected within the same assembly - an error
					//
					return false;
				} else if ((thisp & ~(MethodAttributes.Family | MethodAttributes.FamORAssem)) != 
					   (base_classp & ~(MethodAttributes.Family | MethodAttributes.FamORAssem))) {
					//
					// protected ok, but other attributes differ - report an error
					//
					return false;
				}
				return true;
			} else {
				return (thisp == base_classp);
			}
		}

		public bool CheckAbstractAndExtern (bool has_block)
		{
			if (Parent.PartialContainer.Kind == Kind.Interface)
				return true;

			if (has_block) {
				if ((ModFlags & Modifiers.EXTERN) != 0) {
					Report.Error (179, Location, "`{0}' cannot declare a body because it is marked extern",
						GetSignatureForError ());
					return false;
				}

				if ((ModFlags & Modifiers.ABSTRACT) != 0) {
					Report.Error (500, Location, "`{0}' cannot declare a body because it is marked abstract",
						GetSignatureForError ());
					return false;
				}
			} else {
				if ((ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) == 0) {
					Report.Error (501, Location, "`{0}' must declare a body because it is not marked abstract or extern",
						GetSignatureForError ());
					return false;
				}
			}

			return true;
		}

		protected bool CheckParameters (Parameters parameters)
		{
			bool error = false;

			foreach (Type partype in parameters.Types){
				if (partype.IsPointer){
					if (!TypeManager.VerifyUnManaged (TypeManager.GetElementType (partype), Location))
						error = true;
				}

				if (ds.AsAccessible (partype, ModFlags))
					continue;

				Report.SymbolRelatedToPreviousError (partype);
				if (this is Indexer)
					Report.Error (55, Location,
						"Inconsistent accessibility: parameter type `" +
						TypeManager.CSharpName (partype) + "' is less " +
						"accessible than indexer `" + GetSignatureForError () + "'");
				else if (this is Operator)
					Report.Error (57, Location,
						"Inconsistent accessibility: parameter type `" +
						TypeManager.CSharpName (partype) + "' is less " +
						"accessible than operator `" + GetSignatureForError () + "'");
				else
					Report.Error (51, Location,
						"Inconsistent accessibility: parameter type `" +
						TypeManager.CSharpName (partype) + "' is less " +
						"accessible than method `" + GetSignatureForError () + "'");
				error = true;
			}

			return !error;
		}

		protected override bool DoDefine()
		{
			if (!base.DoDefine ())
				return false;

			if (IsExplicitImpl) {
				Expression expr = MemberName.Left.GetTypeExpression ();
				TypeExpr texpr = expr.ResolveAsTypeTerminal (this, false);
				if (texpr == null)
					return false;

				InterfaceType = texpr.Type;

				if (!InterfaceType.IsInterface) {
					Report.Error (538, Location, "`{0}' in explicit interface declaration is not an interface", TypeManager.CSharpName (InterfaceType));
					return false;
				}
				
				if (!Parent.PartialContainer.VerifyImplements (this))
					return false;
				
			}
			return true;
		}

		protected virtual bool DoDefineBase ()
		{
			if (Name == null)
				throw new InternalErrorException ();

			if (IsInterface) {
				ModFlags = Modifiers.PUBLIC |
					Modifiers.ABSTRACT |
					Modifiers.VIRTUAL | (ModFlags & Modifiers.UNSAFE) | (ModFlags & Modifiers.NEW);

				flags = MethodAttributes.Public |
					MethodAttributes.Abstract |
					MethodAttributes.HideBySig |
					MethodAttributes.NewSlot |
					MethodAttributes.Virtual;
			} else {
				if (!Parent.PartialContainer.MethodModifiersValid (this))
					return false;

				flags = Modifiers.MethodAttr (ModFlags);
			}

			if (IsExplicitImpl) {
				Expression expr = MemberName.Left.GetTypeExpression ();
				TypeExpr iface_texpr = expr.ResolveAsTypeTerminal (this, false);
				if (iface_texpr == null)
					return false;

				InterfaceType = iface_texpr.Type;

				if (!InterfaceType.IsInterface) {
					Report.Error (538, Location, "'{0}' in explicit interface declaration is not an interface", TypeManager.CSharpName (InterfaceType));
					return false;
				}

				if (!Parent.PartialContainer.VerifyImplements (this))
					return false;
				
				Modifiers.Check (Modifiers.AllowedExplicitImplFlags, explicit_mod_flags, 0, Location);
			}

			return true;
		}

		protected void Error_CannotChangeAccessModifiers (MemberInfo base_method, MethodAttributes ma, string suffix)
		{
			Report.SymbolRelatedToPreviousError (base_method);
			string base_name = TypeManager.GetFullNameSignature (base_method);
			string this_name = GetSignatureForError ();
			if (suffix != null) {
				base_name += suffix;
				this_name += suffix;
			}

			Report.Error (507, Location, "`{0}': cannot change access modifiers when overriding `{1}' inherited member `{2}'",
				this_name, Modifiers.GetDescription (ma), base_name);
		}

		protected static string Error722 {
			get {
				return "`{0}': static types cannot be used as return types";
			}
		}

		/// <summary>
		/// For custom member duplication search in a container
		/// </summary>
		protected abstract bool CheckForDuplications ();

		/// <summary>
		/// Gets base method and its return type
		/// </summary>
		protected abstract MethodInfo FindOutBaseMethod (ref Type base_ret_type);

		//
		// The "short" name of this property / indexer / event.  This is the
		// name without the explicit interface.
		//
		public string ShortName 
		{
			get { return MemberName.Name; }
			set { SetMemberName (new MemberName (MemberName.Left, value, Location)); }
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ()) {
				if (IsInterface && HasClsCompliantAttribute && Parent.IsClsComplianceRequired ()) {
					Report.Error (3010, Location, "`{0}': CLS-compliant interfaces must have only CLS-compliant members", GetSignatureForError ());
				}

				if ((ModFlags & Modifiers.ABSTRACT) != 0 && Parent.TypeBuilder.IsClass && IsExposedFromAssembly () && Parent.IsClsComplianceRequired ()) {
					Report.Error (3011, Location, "`{0}': only CLS-compliant members can be abstract", GetSignatureForError ());
				}
				return false;
			}

			if (GenericMethod != null)
				GenericMethod.VerifyClsCompliance ();

			return true;
		}

		public override bool IsUsed 
		{
			get { return IsExplicitImpl || base.IsUsed; }
		}

	}

	public abstract class MethodOrOperator : MethodCore, IMethodData
	{
		public MethodBuilder MethodBuilder;
		ReturnParameter return_attributes;
		ListDictionary declarative_security;
		protected MethodData MethodData;

		Iterator iterator;
		ArrayList anonymous_methods;

		static string[] attribute_targets = new string [] { "method", "return" };

		protected MethodOrOperator (DeclSpace parent, GenericMethod generic, Expression type, int mod,
				int allowed_mod, bool is_interface, MemberName name,
				Attributes attrs, Parameters parameters)
			: base (parent, generic, type, mod, allowed_mod, is_interface, name,
					attrs, parameters)
		{
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Target == AttributeTargets.ReturnValue) {
				if (return_attributes == null)
					return_attributes = new ReturnParameter (MethodBuilder, Location);

				return_attributes.ApplyAttributeBuilder (a, cb);
				return;
			}

			if (a.Type == TypeManager.methodimpl_attr_type &&
				(a.GetMethodImplOptions () & MethodImplOptions.InternalCall) != 0) {
				MethodBuilder.SetImplementationFlags (MethodImplAttributes.InternalCall | MethodImplAttributes.Runtime);
			}

			if (a.Type == TypeManager.dllimport_type) {
				const int extern_static = Modifiers.EXTERN | Modifiers.STATIC;
				if ((ModFlags & extern_static) != extern_static) {
					Report.Error (601, a.Location, "The DllImport attribute must be specified on a method marked `static' and `extern'");
				}

				return;
			}

			if (a.Type.IsSubclassOf (TypeManager.security_attr_type) && a.CheckSecurityActionValidity (false)) {
				if (declarative_security == null)
					declarative_security = new ListDictionary ();
				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			MethodBuilder.SetCustomAttribute (cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Method; 
			}
		}

		public virtual EmitContext CreateEmitContext (DeclSpace tc, ILGenerator ig)
		{
			return new EmitContext (
				this, tc, this.ds, Location, ig, MemberType, ModFlags, false);
		}

		public void AddAnonymousMethod (AnonymousMethodExpression anonymous)
		{
			if (anonymous_methods == null)
				anonymous_methods = new ArrayList ();
			anonymous_methods.Add (anonymous);
		}

		protected bool DefineGenericMethod ()
		{
			if (!DoDefineBase ())
				return false;

#if GMCS_SOURCE
			if (GenericMethod != null) {
				string method_name = MemberName.Name;

				if (IsExplicitImpl) {
					method_name = TypeManager.CSharpName (InterfaceType) +
						'.' + method_name;
				}

				MethodBuilder = Parent.TypeBuilder.DefineMethod (method_name, flags);

				if (!GenericMethod.Define (MethodBuilder, block))
					return false;
			}
#endif

			return true;
		}

		public bool ResolveMembers ()
		{
			if (!DefineGenericMethod ())
				return false;

			if ((ModFlags & Modifiers.METHOD_YIELDS) != 0) {
				iterator = Iterator.CreateIterator (this, Parent, GenericMethod, ModFlags);
				if (iterator == null)
					return false;
			}

			if (anonymous_methods != null) {
				foreach (AnonymousMethodExpression ame in anonymous_methods) {
					if (!ame.CreateAnonymousHelpers ())
						return false;
				}
			}

			return true;
		}

		public override bool Define ()
		{
			if (!DoDefine ())
				return false;

			if (!CheckAbstractAndExtern (block != null))
				return false;

			if (!CheckBase ())
				return false;

			MethodData = new MethodData (
				this, ModFlags, flags, this, MethodBuilder, GenericMethod, base_method);

			if (!MethodData.Define (Parent.PartialContainer))
				return false;

			MethodBuilder = MethodData.MethodBuilder;

			if (MemberType.IsAbstract && MemberType.IsSealed) {
				Report.Error (722, Location, Error722, TypeManager.CSharpName (MemberType));
				return false;
			}

			return true;
		}

		public override void Emit ()
		{
			if (OptAttributes != null)
				OptAttributes.Emit ();

			if (declarative_security != null) {
				foreach (DictionaryEntry de in declarative_security) {
					MethodBuilder.AddDeclarativeSecurity ((SecurityAction)de.Key, (PermissionSet)de.Value);
				}
			}

			base.Emit ();
		}

		protected void Error_ConditionalAttributeIsNotValid ()
		{
			Report.Error (577, Location,
				"Conditional not valid on `{0}' because it is a constructor, destructor, operator or explicit interface implementation",
				GetSignatureForError ());
		}

		public override bool MarkForDuplicationCheck ()
		{
			caching_flags |= Flags.TestMethodDuplication;
			return true;
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		#region IMethodData Members

		public CallingConventions CallingConventions {
			get {
				CallingConventions cc = Parameters.CallingConvention;
				if (Parameters.HasArglist)
					block.HasVarargs = true;

				if (!IsInterface)
					if ((ModFlags & Modifiers.STATIC) == 0)
						cc |= CallingConventions.HasThis;

				// FIXME: How is `ExplicitThis' used in C#?
			
				return cc;
			}
		}

		public Type ReturnType {
			get {
				return MemberType;
			}
		}

		public MemberName MethodName {
			get {
				return MemberName;
			}
		}

		public Iterator Iterator {
			get { return iterator; }
		}

		public new Location Location {
			get {
				return base.Location;
			}
		}

		protected override bool CheckBase ()
		{
			if (!base.CheckBase ())
				return false;

			// TODO: Destructor should derive from MethodCore
			if (base_method != null && (ModFlags & Modifiers.OVERRIDE) != 0 && Name == "Finalize" &&
				base_method.DeclaringType == TypeManager.object_type && !(this is Destructor)) {
				Report.Error (249, Location, "Do not override object.Finalize. Instead, provide a destructor");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Returns true if method has conditional attribute and the conditions is not defined (method is excluded).
		/// </summary>
		public bool IsExcluded () {
			if ((caching_flags & Flags.Excluded_Undetected) == 0)
				return (caching_flags & Flags.Excluded) != 0;

			caching_flags &= ~Flags.Excluded_Undetected;

			if (base_method == null) {
				if (OptAttributes == null)
					return false;

				Attribute[] attrs = OptAttributes.SearchMulti (TypeManager.conditional_attribute_type);

				if (attrs == null)
					return false;

				foreach (Attribute a in attrs) {
					string condition = a.GetConditionalAttributeValue ();
					if (condition == null)
						return false;

					if (RootContext.AllDefines.Contains (condition))
						return false;
				}

				caching_flags |= Flags.Excluded;
				return true;
			}

			IMethodData md = TypeManager.GetMethod (base_method);
			if (md == null) {
				if (AttributeTester.IsConditionalMethodExcluded (base_method)) {
					caching_flags |= Flags.Excluded;
					return true;
				}
				return false;
			}

			if (md.IsExcluded ()) {
				caching_flags |= Flags.Excluded;
				return true;
			}
			return false;
		}

		GenericMethod IMethodData.GenericMethod {
			get {
				return GenericMethod;
			}
		}

		#endregion

	}

	public class SourceMethod : ISourceMethod
	{
		DeclSpace parent;
		MethodBase builder;

		protected SourceMethod (DeclSpace parent, MethodBase builder,
					ISourceFile file, Location start, Location end)
		{
			this.parent = parent;
			this.builder = builder;
			
			CodeGen.SymbolWriter.OpenMethod (file, this, start.Row, start.Column, end.Row, start.Column);
		}

		public string Name {
			get { return builder.Name; }
		}

		public int NamespaceID {
			get { return parent.NamespaceEntry.SymbolFileID; }
		}

		public int Token {
			get {
				if (builder is MethodBuilder)
					return ((MethodBuilder) builder).GetToken ().Token;
				else if (builder is ConstructorBuilder)
					return ((ConstructorBuilder) builder).GetToken ().Token;
				else
					throw new NotSupportedException ();
			}
		}

		public void CloseMethod ()
		{
			if (CodeGen.SymbolWriter != null)
				CodeGen.SymbolWriter.CloseMethod ();
		}

		public static SourceMethod Create (DeclSpace parent, MethodBase builder, Block block)
		{
			if (CodeGen.SymbolWriter == null)
				return null;
			if (block == null)
				return null;

			Location start_loc = block.StartLocation;
			if (start_loc.IsNull)
				return null;

			Location end_loc = block.EndLocation;
			if (end_loc.IsNull)
				return null;

			ISourceFile file = start_loc.SourceFile;
			if (file == null)
				return null;

			return new SourceMethod (
				parent, builder, file, start_loc, end_loc);
		}
	}

	public class Method : MethodOrOperator, IAnonymousHost {

		/// <summary>
		///   Modifiers allowed in a class declaration
		/// </summary>
		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
			Modifiers.VIRTUAL |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.ABSTRACT |
		        Modifiers.UNSAFE |
			Modifiers.METHOD_YIELDS | 
			Modifiers.EXTERN;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW | Modifiers.UNSAFE;

		//
		// return_type can be "null" for VOID values.
		//
		public Method (DeclSpace parent, GenericMethod generic,
			       Expression return_type, int mod, bool is_iface,
			       MemberName name, Parameters parameters, Attributes attrs)
			: base (parent, generic, return_type, mod,
				is_iface ? AllowedInterfaceModifiers : AllowedModifiers,
				is_iface, name, attrs, parameters)
		{
		}
		
		public override string GetSignatureForError()
		{
			return base.GetSignatureForError () + Parameters.GetSignatureForError ();
		}

		void Error_DuplicateEntryPoint (MethodInfo b, Location location)
		{
			Report.Error (17, location,
				"Program `{0}' has more than one entry point defined: `{1}'",
				CodeGen.FileName, TypeManager.CSharpSignature(b));
		}

		bool IsEntryPoint (Parameters pinfo)
		{
			if (ReturnType != TypeManager.void_type &&
				ReturnType != TypeManager.int32_type)
				return false;

			if (pinfo.Count == 0)
				return true;

			if (pinfo.Count > 1)
				return false;

			Type t = pinfo.ParameterType (0);
			if (t.IsArray &&
				(t.GetArrayRank () == 1) &&
				(TypeManager.GetElementType (t) == TypeManager.string_type) &&
				(pinfo.ParameterModifier (0) == Parameter.Modifier.NONE))
				return true;
			else
				return false;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.conditional_attribute_type) {
				if (IsExplicitImpl) {
					Error_ConditionalAttributeIsNotValid ();
					return;
				}

				if (ReturnType != TypeManager.void_type) {
					Report.Error (578, Location, "Conditional not valid on `{0}' because its return type is not void", GetSignatureForError ());
					return;
				}

				if ((ModFlags & Modifiers.OVERRIDE) != 0) {
					Report.Error (243, Location, "Conditional not valid on `{0}' because it is an override method", GetSignatureForError ());
					return;
				}

				if (IsInterface) {
					Report.Error (582, Location, "Conditional not valid on interface members");
					return;
				}

				if (MethodData.implementing != null) {
					Report.Error (629, Location, "Conditional member `{0}' cannot implement interface member `{1}'",
						GetSignatureForError (), TypeManager.CSharpSignature (MethodData.implementing));
					return;
				}

				for (int i = 0; i < ParameterInfo.Count; ++i) {
					if ((ParameterInfo.ParameterModifier (i) & Parameter.Modifier.OUTMASK) != 0) {
						Report.Error (685, Location, "Conditional method `{0}' cannot have an out parameter", GetSignatureForError ());
						return;
					}
				}
			}

			base.ApplyAttributeBuilder (a, cb);
		}

  		protected override bool CheckForDuplications ()
   		{
  			ArrayList ar = Parent.PartialContainer.Methods;
  			if (ar != null) {
  				int arLen = ar.Count;
   					
  				for (int i = 0; i < arLen; i++) {
  					Method m = (Method) ar [i];
  					if (IsDuplicateImplementation (m))
  						return false;
   				}
  			}

			ar = Parent.PartialContainer.Properties;
			if (ar != null) {
				for (int i = 0; i < ar.Count; ++i) {
					PropertyBase pb = (PropertyBase) ar [i];
					if (pb.AreAccessorsDuplicateImplementation (this))
						return false;
				}
			}

			ar = Parent.PartialContainer.Indexers;
			if (ar != null) {
				for (int i = 0; i < ar.Count; ++i) {
					PropertyBase pb = (PropertyBase) ar [i];
					if (pb.AreAccessorsDuplicateImplementation (this))
						return false;
				}
			}

			ar = Parent.PartialContainer.Events;
			if (ar != null) {
				for (int i = 0; i < ar.Count; ++i) {
					Event ev = (Event) ar [i];
					if (ev.AreAccessorsDuplicateImplementation (this))
						return false;
				}
			}

			return true;
		}

		//
		// Creates the type
		//
		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			if (RootContext.StdLib && (ReturnType == TypeManager.arg_iterator_type || ReturnType == TypeManager.typed_reference_type)) {
				Error1599 (Location, ReturnType);
				return false;
			}

			if (ReturnType == TypeManager.void_type && ParameterTypes.Length == 0 && 
				Name == "Finalize" && !(this is Destructor)) {
				Report.Warning (465, 1, Location, "Introducing a 'Finalize' method can interfere with destructor invocation. Did you intend to declare a destructor?");
			}

			if (base_method != null) {
				if (Parameters.Count == 1 && ParameterTypes [0] == TypeManager.object_type && Name == "Equals")
					Parent.PartialContainer.Mark_HasEquals ();
				else if (Parameters.Empty && Name == "GetHashCode")
					Parent.PartialContainer.Mark_HasGetHashCode ();
			}

			//
			// This is used to track the Entry Point,
			//
			if (RootContext.NeedsEntryPoint &&  ((ModFlags & Modifiers.STATIC) != 0) &&
				Name == "Main" &&
				(RootContext.MainClass == null ||
				RootContext.MainClass == Parent.TypeBuilder.FullName)){
				if (IsEntryPoint (ParameterInfo)) {

					if (RootContext.EntryPoint == null) {
						if (Parent.IsGeneric || MemberName.IsGeneric) {
							Report.Warning (402, 4, Location, "`{0}': an entry point cannot be generic or in a generic type",
								GetSignatureForError ());
						} else {
							IMethodData md = TypeManager.GetMethod (MethodBuilder);
							md.SetMemberIsUsed ();

							RootContext.EntryPoint = MethodBuilder;
							RootContext.EntryPointLocation = Location;
						}
					} else {
						Error_DuplicateEntryPoint (RootContext.EntryPoint, RootContext.EntryPointLocation);
						Error_DuplicateEntryPoint (MethodBuilder, Location);
					}
				} else {
					Report.Warning (28, 4, Location, "`{0}' has the wrong signature to be an entry point",
						GetSignatureForError ());
				}
			}

			return true;
		}

		//
		// Emits the code
		// 
		public override void Emit ()
		{
			Report.Debug (64, "METHOD EMIT", this, MethodBuilder, Location, Block, MethodData);
			MethodData.Emit (Parent);
			base.Emit ();

			Block = null;
			MethodData = null;
		}

		public static void Error1599 (Location loc, Type t)
		{
			Report.Error (1599, loc, "Method or delegate cannot return type `{0}'", TypeManager.CSharpName (t));
		}

		protected override MethodInfo FindOutBaseMethod (ref Type base_ret_type)
		{
			MethodInfo mi = (MethodInfo) Parent.PartialContainer.BaseCache.FindMemberToOverride (
				Parent.TypeBuilder, Name, ParameterTypes, GenericMethod, false);

			if (mi == null)
				return null;

			if (mi.IsSpecialName)
				return null;

			base_ret_type = mi.ReturnType;
			return mi;
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if (ParameterInfo.Count > 0) {
				ArrayList al = (ArrayList)Parent.PartialContainer.MemberCache.Members [Name];
				if (al.Count > 1)
					MemberCache.VerifyClsParameterConflict (al, this, MethodBuilder);
			}

			return true;
		}
	}

	public abstract class ConstructorInitializer {
		ArrayList argument_list;
		protected ConstructorInfo base_constructor;
		Location loc;
		
		public ConstructorInitializer (ArrayList argument_list, Location loc)
		{
			this.argument_list = argument_list;
			this.loc = loc;
		}

		public ArrayList Arguments {
			get {
				return argument_list;
			}
		}

		public bool Resolve (ConstructorBuilder caller_builder, Block block, EmitContext ec)
		{
			Expression base_constructor_group;
			Type t;
			bool error = false;

			ec.CurrentBlock = block;

			if (argument_list != null){
				foreach (Argument a in argument_list){
					if (!a.Resolve (ec, loc))
						return false;
				}
			}
			ec.CurrentBlock = null;

			if (this is ConstructorBaseInitializer) {
				if (ec.ContainerType.BaseType == null)
					return true;

				t = ec.ContainerType.BaseType;
				if (ec.ContainerType.IsValueType) {
					Report.Error (522, loc,
						"`{0}': Struct constructors cannot call base constructors", TypeManager.CSharpSignature (caller_builder));
					return false;
				}
			} else {
				//
				// It is legal to have "this" initializers that take no arguments
				// in structs, they are just no-ops.
				//
				// struct D { public D (int a) : this () {}
				//
				if (ec.ContainerType.IsValueType && argument_list == null)
					return true;
				
				t = ec.ContainerType;
			}

			base_constructor_group = Expression.MemberLookup (
				ec.ContainerType, t, ".ctor", MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
				loc);
			
			if (base_constructor_group == null){
				error = true;
				base_constructor_group = Expression.MemberLookup (
					t, null, t, ".ctor", MemberTypes.Constructor,
					BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly,
					loc);
			}

			int errors = Report.Errors;
			if (base_constructor_group != null)
				base_constructor = (ConstructorInfo) Invocation.OverloadResolve (
					ec, (MethodGroupExpr) base_constructor_group, argument_list,
					false, loc);
			
			if (base_constructor == null) {
				if (errors == Report.Errors)
					Invocation.Error_WrongNumArguments (loc, TypeManager.CSharpSignature (caller_builder),
						argument_list == null ? 0 : argument_list.Count);
				return false;
			}

			if (error) {
				Report.SymbolRelatedToPreviousError (base_constructor);
				Expression.ErrorIsInaccesible (loc, TypeManager.CSharpSignature (base_constructor));
				base_constructor = null;
				return false;
			}
			
			if (base_constructor == caller_builder){
				Report.Error (516, loc, "Constructor `{0}' cannot call itself", TypeManager.CSharpSignature (caller_builder));
				return false;
			}
			
			return true;
		}

		public virtual void Emit (EmitContext ec)
		{
			if (base_constructor != null){
				ec.Mark (loc, false);
				if (ec.IsStatic)
					Invocation.EmitCall (ec, true, true, null, base_constructor, argument_list, loc);
				else
					Invocation.EmitCall (ec, true, false, ec.GetThis (loc), base_constructor, argument_list, loc);
			}
		}
	}

	public class ConstructorBaseInitializer : ConstructorInitializer {
		public ConstructorBaseInitializer (ArrayList argument_list, Location l) :
			base (argument_list, l)
		{
		}
	}

	class GeneratedBaseInitializer: ConstructorBaseInitializer {
		public GeneratedBaseInitializer (Location loc):
			base (null, loc)
		{
		}
	}

	public class ConstructorThisInitializer : ConstructorInitializer {
		public ConstructorThisInitializer (ArrayList argument_list, Location l) :
			base (argument_list, l)
		{
		}
	}
	
	public class Constructor : MethodCore, IMethodData, IAnonymousHost {
		public ConstructorBuilder ConstructorBuilder;
		public ConstructorInitializer Initializer;
		ListDictionary declarative_security;
		ArrayList anonymous_methods;

		// <summary>
		//   Modifiers allowed for a constructor.
		// </summary>
		public const int AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.STATIC |
			Modifiers.UNSAFE |
			Modifiers.EXTERN |		
			Modifiers.PRIVATE;

		static string[] attribute_targets = new string [] { "method" };

		public Iterator Iterator {
			get { return null; }
		}

		bool has_compliant_args = false;
		//
		// The spec claims that static is not permitted, but
		// my very own code has static constructors.
		//
		public Constructor (DeclSpace parent, string name, int mod, Parameters args,
				    ConstructorInitializer init, Location loc)
			: base (parent, null, null, mod, AllowedModifiers, false,
				new MemberName (name, loc), null, args)
		{
			Initializer = init;
		}

		public bool HasCompliantArgs {
			get { return has_compliant_args; }
		}

		public override AttributeTargets AttributeTargets {
			get { return AttributeTargets.Constructor; }
		}


		//
		// Returns true if this is a default constructor
		//
		public bool IsDefault ()
		{
			if ((ModFlags & Modifiers.STATIC) != 0)
				return Parameters.Empty;
			
			return Parameters.Empty &&
					(Initializer is ConstructorBaseInitializer) &&
					(Initializer.Arguments == null);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type.IsSubclassOf (TypeManager.security_attr_type) && a.CheckSecurityActionValidity (false)) {
				if (declarative_security == null) {
					declarative_security = new ListDictionary ();
				}
				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (a.Type == TypeManager.methodimpl_attr_type &&
				(a.GetMethodImplOptions () & MethodImplOptions.InternalCall) != 0) {
				ConstructorBuilder.SetImplementationFlags (MethodImplAttributes.InternalCall | MethodImplAttributes.Runtime);
			}

			ConstructorBuilder.SetCustomAttribute (cb);
		}

		public void AddAnonymousMethod (AnonymousMethodExpression anonymous)
		{
			if (anonymous_methods == null)
				anonymous_methods = new ArrayList ();
			anonymous_methods.Add (anonymous);
		}

		public bool ResolveMembers ()
		{
			if (anonymous_methods != null) {
				foreach (AnonymousMethodExpression ame in anonymous_methods) {
					if (!ame.CreateAnonymousHelpers ())
						return false;
				}
			}

			return true;
		}

 		protected override bool CheckForDuplications ()
		{
			ArrayList ar = Parent.PartialContainer.InstanceConstructors;
			if (ar != null) {
				int arLen = ar.Count;
					
				for (int i = 0; i < arLen; i++) {
					Constructor m = (Constructor) ar [i];
					if (IsDuplicateImplementation (m))
						return false;
				}
			}
			return true;
		}

		protected override bool CheckBase ()
		{
			if ((ModFlags & Modifiers.STATIC) != 0) {
				if (!Parameters.Empty) {
					Report.Error (132, Location, "`{0}': The static constructor must be parameterless",
						GetSignatureForError ());
					return false;
				}

				// the rest can be ignored
				return true;
			}

			// Check whether arguments were correct.
			if (!DoDefineParameters ())
				return false;

			if (!CheckForDuplications ())
				return false;

			if (Parent.PartialContainer.Kind == Kind.Struct) {
				if (ParameterTypes.Length == 0) {
					Report.Error (568, Location, 
						"Structs cannot contain explicit parameterless constructors");
					return false;
				}

				if ((ModFlags & Modifiers.PROTECTED) != 0) {
					Report.Error (666, Location, "`{0}': new protected member declared in struct", GetSignatureForError ());
					return false;
				}
			}

			if ((RootContext.WarningLevel >= 4) && ((Parent.ModFlags & Modifiers.SEALED) != 0 && (ModFlags & Modifiers.PROTECTED) != 0)) {
				Report.Warning (628, 4, Location, "`{0}': new protected member declared in sealed class", GetSignatureForError ());
			}
			
			return true;
		}
		
		//
		// Creates the ConstructorBuilder
		//
		public override bool Define ()
		{
			if (ConstructorBuilder != null)
				return true;

			MethodAttributes ca = (MethodAttributes.RTSpecialName |
					       MethodAttributes.SpecialName);
			
			if ((ModFlags & Modifiers.STATIC) != 0) {
				ca |= MethodAttributes.Static | MethodAttributes.Private;
			} else {
				ca |= MethodAttributes.HideBySig;

				if ((ModFlags & Modifiers.PUBLIC) != 0)
					ca |= MethodAttributes.Public;
				else if ((ModFlags & Modifiers.PROTECTED) != 0){
					if ((ModFlags & Modifiers.INTERNAL) != 0)
						ca |= MethodAttributes.FamORAssem;
					else 
						ca |= MethodAttributes.Family;
				} else if ((ModFlags & Modifiers.INTERNAL) != 0)
					ca |= MethodAttributes.Assembly;
				else
					ca |= MethodAttributes.Private;
			}

			if (!CheckAbstractAndExtern (block != null))
				return false;
			
			// Check if arguments were correct.
			if (!CheckBase ())
				return false;

			ConstructorBuilder = Parent.TypeBuilder.DefineConstructor (
				ca, CallingConventions,
				ParameterTypes);

			if ((ModFlags & Modifiers.UNSAFE) != 0)
				ConstructorBuilder.InitLocals = false;

			if (Parent.PartialContainer.IsComImport) {
				if (!IsDefault ()) {
					Report.Error (669, Location, "`{0}': A class with the ComImport attribute cannot have a user-defined constructor",
						Parent.GetSignatureForError ());
					return false;
				}
				ConstructorBuilder.SetImplementationFlags (MethodImplAttributes.InternalCall);
			}
			
			TypeManager.AddMethod (ConstructorBuilder, this);

			return true;
		}

		//
		// Emits the code
		//
		public override void Emit ()
		{
			if (OptAttributes != null)
				OptAttributes.Emit ();

			EmitContext ec = CreateEmitContext (null, null);

			if (block != null) {
				// If this is a non-static `struct' constructor and doesn't have any
				// initializer, it must initialize all of the struct's fields.
				if ((Parent.PartialContainer.Kind == Kind.Struct) &&
					((ModFlags & Modifiers.STATIC) == 0) && (Initializer == null))
					block.AddThisVariable (Parent, Location);

				if (!block.ResolveMeta (ec, ParameterInfo))
					block = null;
			}

			if ((ModFlags & Modifiers.STATIC) == 0){
				if (Parent.PartialContainer.Kind == Kind.Class && Initializer == null)
					Initializer = new GeneratedBaseInitializer (Location);


				//
				// Spec mandates that Initializers will not have
				// `this' access
				//
				ec.IsStatic = true;
				if ((Initializer != null) &&
				    !Initializer.Resolve (ConstructorBuilder, block, ec))
					return;
				ec.IsStatic = false;
			}

			Parameters.ApplyAttributes (ConstructorBuilder);
			
			SourceMethod source = SourceMethod.Create (
				Parent, ConstructorBuilder, block);

			//
			// Classes can have base initializers and instance field initializers.
			//
			if (Parent.PartialContainer.Kind == Kind.Class){
				if ((ModFlags & Modifiers.STATIC) == 0){

					//
					// If we use a "this (...)" constructor initializer, then
					// do not emit field initializers, they are initialized in the other constructor
					//
					if (!(Initializer != null && Initializer is ConstructorThisInitializer))
						Parent.PartialContainer.EmitFieldInitializers (ec);
				}
			}

			bool unreachable = false;
			if (block != null) {
				ec.ResolveTopBlock (null, block, ParameterInfo, this, out unreachable);
				ec.EmitMeta (block);

				if (block.ScopeInfo != null) {
					ExpressionStatement init = block.ScopeInfo.GetScopeInitializer (ec);
					init.EmitStatement (ec);
				}
			}

			if (Initializer != null) {
				Initializer.Emit (ec);
			}
			
			if ((ModFlags & Modifiers.STATIC) != 0)
				Parent.PartialContainer.EmitFieldInitializers (ec);

			if (block != null)
				ec.EmitResolvedTopBlock (block, unreachable);

			if (source != null)
				source.CloseMethod ();

			base.Emit ();

			if (declarative_security != null) {
				foreach (DictionaryEntry de in declarative_security) {
					ConstructorBuilder.AddDeclarativeSecurity ((SecurityAction)de.Key, (PermissionSet)de.Value);
				}
			}

			block = null;
		}

		// Is never override
		protected override MethodInfo FindOutBaseMethod (ref Type base_ret_type)
		{
			return null;
		}

		public override string GetSignatureForError()
		{
			return base.GetSignatureForError () + Parameters.GetSignatureForError ();
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance () || !IsExposedFromAssembly ()) {
				return false;
			}
			
 			if (ParameterInfo.Count > 0) {
 				ArrayList al = (ArrayList)Parent.MemberCache.Members [".ctor"];
 				if (al.Count > 3)
 					MemberCache.VerifyClsParameterConflict (al, this, ConstructorBuilder);
 
				if (Parent.TypeBuilder.IsSubclassOf (TypeManager.attribute_type)) {
					foreach (Type param in ParameterTypes) {
						if (param.IsArray) {
							return true;
						}
					}
				}
			}
			has_compliant_args = true;
			return true;
		}

		#region IMethodData Members

		public System.Reflection.CallingConventions CallingConventions {
			get {
				CallingConventions cc = Parameters.CallingConvention;

				if (Parent.PartialContainer.Kind == Kind.Class)
					if ((ModFlags & Modifiers.STATIC) == 0)
						cc |= CallingConventions.HasThis;

				// FIXME: How is `ExplicitThis' used in C#?
			
				return cc;
			}
		}

		public new Location Location {
			get {
				return base.Location;
			}
		}

		public MemberName MethodName {
			get {
				return MemberName;
			}
		}

		public Type ReturnType {
			get {
				return MemberType;
			}
		}

		public EmitContext CreateEmitContext (DeclSpace ds, ILGenerator ig)
		{
			ILGenerator ig_ = ConstructorBuilder.GetILGenerator ();
			return new EmitContext (this, Parent, Location, ig_, null, ModFlags, true);
		}

		public bool IsExcluded()
		{
			return false;
		}

		GenericMethod IMethodData.GenericMethod {
			get {
				return null;
			}
		}

		#endregion
	}

	/// <summary>
	/// Interface for MethodData class. Holds links to parent members to avoid member duplication.
	/// </summary>
	public interface IMethodData
	{
		CallingConventions CallingConventions { get; }
		Location Location { get; }
		MemberName MethodName { get; }
		Type ReturnType { get; }
		GenericMethod GenericMethod { get; }
		Parameters ParameterInfo { get; }

		Iterator Iterator { get; }

		Attributes OptAttributes { get; }
		ToplevelBlock Block { get; set; }

		EmitContext CreateEmitContext (DeclSpace ds, ILGenerator ig);
		ObsoleteAttribute GetObsoleteAttribute ();
		string GetSignatureForError ();
		bool IsExcluded ();
		bool IsClsComplianceRequired ();
		void SetMemberIsUsed ();
	}

	//
	// Encapsulates most of the Method's state
	//
	public class MethodData {

		readonly IMethodData method;

		public readonly GenericMethod GenericMethod;

		//
		// Are we implementing an interface ?
		//
		public MethodInfo implementing;

		//
		// Protected data.
		//
		protected InterfaceMemberBase member;
		protected int modifiers;
		protected MethodAttributes flags;
		protected Type declaring_type;
		protected MethodInfo parent_method;

		MethodBuilder builder = null;
		public MethodBuilder MethodBuilder {
			get {
				return builder;
			}
		}

		public Type DeclaringType {
			get {
				return declaring_type;
			}
		}

		public MethodData (InterfaceMemberBase member,
				   int modifiers, MethodAttributes flags, IMethodData method)
		{
			this.member = member;
			this.modifiers = modifiers;
			this.flags = flags;

			this.method = method;
		}

		public MethodData (InterfaceMemberBase member, 
				   int modifiers, MethodAttributes flags, 
				   IMethodData method, MethodBuilder builder,
				   GenericMethod generic, MethodInfo parent_method)
			: this (member, modifiers, flags, method)
		{
			this.builder = builder;
			this.GenericMethod = generic;
			this.parent_method = parent_method;
		}

		public bool Define (DeclSpace parent)
		{
			string name = method.MethodName.Basename;
			string method_name = method.MethodName.FullName;

			TypeContainer container = parent.PartialContainer;

			PendingImplementation pending = container.PendingImplementations;
			if (pending != null){
				if (member is Indexer) // TODO: test it, but it should work without this IF
					implementing = pending.IsInterfaceIndexer (
						member.InterfaceType, method.ReturnType, method.ParameterInfo);
				else
					implementing = pending.IsInterfaceMethod (
						member.InterfaceType, name, method.ReturnType, method.ParameterInfo);

				if (member.InterfaceType != null){
					if (implementing == null){
						if (member is PropertyBase) {
							Report.Error (550, method.Location, "`{0}' is an accessor not found in interface member `{1}{2}'",
								      method.GetSignatureForError (), TypeManager.CSharpName (member.InterfaceType),
								      member.GetSignatureForError ().Substring (member.GetSignatureForError ().LastIndexOf ('.')));

						} else {
							Report.Error (539, method.Location,
								      "`{0}.{1}' in explicit interface declaration is not a member of interface",
								      TypeManager.CSharpName (member.InterfaceType), member.ShortName);
						}
						return false;
					}
					if (implementing.IsSpecialName && !(method is AbstractPropertyEventMethod)) {
						Report.SymbolRelatedToPreviousError (implementing);
						Report.Error (683, method.Location, "`{0}' explicit method implementation cannot implement `{1}' because it is an accessor",
							member.GetSignatureForError (), TypeManager.CSharpSignature (implementing));
						return false;
					}

					method_name = TypeManager.GetFullName (member.InterfaceType) +
						'.' + method_name;
				} else {
					if (implementing != null) {
						AbstractPropertyEventMethod prop_method = method as AbstractPropertyEventMethod;
						if (prop_method == null) {
							if (implementing.IsSpecialName) {
								Report.SymbolRelatedToPreviousError (implementing);
								Report.Error (470, method.Location, "Method `{0}' cannot implement interface accessor `{1}.{2}'",
									method.GetSignatureForError (), TypeManager.CSharpSignature (implementing),
									implementing.Name.StartsWith ("get_") ? "get" : "set");
								return false;
							}
						} else if (implementing.DeclaringType.IsInterface) {
							if (!implementing.IsSpecialName) {
								Report.SymbolRelatedToPreviousError (implementing);
								Report.Error (686, method.Location, "Accessor `{0}' cannot implement interface member `{1}' for type `{2}'. Use an explicit interface implementation",
									method.GetSignatureForError (), TypeManager.CSharpSignature (implementing), container.GetSignatureForError ());
								return false;
							}
							PropertyBase.PropertyMethod pm = prop_method as PropertyBase.PropertyMethod;
							if (pm != null && pm.HasCustomAccessModifier && (pm.ModFlags & Modifiers.PUBLIC) == 0) {
								Report.SymbolRelatedToPreviousError (implementing);
								Report.Error (277, method.Location, "Accessor `{0}' must be declared public to implement interface member `{1}'",
									method.GetSignatureForError (), TypeManager.CSharpSignature (implementing, true));
								return false;
							}
						}
					}
				}
			}

			//
			// For implicit implementations, make sure we are public, for
			// explicit implementations, make sure we are private.
			//
			if (implementing != null){
				//
				// Setting null inside this block will trigger a more
				// verbose error reporting for missing interface implementations
				//
				// The "candidate" function has been flagged already
				// but it wont get cleared
				//
				if (member.IsExplicitImpl){
					if (method.ParameterInfo.HasParams && !TypeManager.GetParameterData (implementing).HasParams) {
						Report.SymbolRelatedToPreviousError (implementing);
						Report.Error (466, method.Location, "`{0}': the explicit interface implementation cannot introduce the params modifier",
							method.GetSignatureForError ());
						return false;
					}
				} else {
					if (implementing.DeclaringType.IsInterface) {
						//
						// If this is an interface method implementation,
						// check for public accessibility
						//
						if ((flags & MethodAttributes.MemberAccessMask) != MethodAttributes.Public)
							implementing = null;
					} else if ((flags & MethodAttributes.MemberAccessMask) == MethodAttributes.Private){
						// We may never be private.
						implementing = null;
					} else if ((modifiers & Modifiers.OVERRIDE) == 0){
						//
						// We may be protected if we're overriding something.
						//
						implementing = null;
					}
				}
					
				//
				// Static is not allowed
				//
				if ((modifiers & Modifiers.STATIC) != 0){
					implementing = null;
				}
			}
			
			//
			// If implementing is still valid, set flags
			//
			if (implementing != null){
				//
				// When implementing interface methods, set NewSlot
				// unless, we are overwriting a method.
				//
				if (implementing.DeclaringType.IsInterface){
					if ((modifiers & Modifiers.OVERRIDE) == 0)
						flags |= MethodAttributes.NewSlot;
				}
				flags |=
					MethodAttributes.Virtual |
					MethodAttributes.HideBySig;

				// Set Final unless we're virtual, abstract or already overriding a method.
				if ((modifiers & (Modifiers.VIRTUAL | Modifiers.ABSTRACT | Modifiers.OVERRIDE)) == 0)
					flags |= MethodAttributes.Final;
			}

			EmitContext ec = method.CreateEmitContext (container, null);

			DefineMethodBuilder (container, method_name, method.ParameterInfo.Types);

			if (builder == null)
				return false;

			if (container.CurrentType != null)
				declaring_type = container.CurrentType;
			else
				declaring_type = container.TypeBuilder;

			if ((modifiers & Modifiers.UNSAFE) != 0)
				builder.InitLocals = false;

			if (implementing != null){
				//
				// clear the pending implemntation flag
				//
				if (member is Indexer) {
					pending.ImplementIndexer (
						member.InterfaceType, builder, method.ReturnType,
						method.ParameterInfo, member.IsExplicitImpl);
				} else
					pending.ImplementMethod (
						member.InterfaceType, name, method.ReturnType,
						method.ParameterInfo, member.IsExplicitImpl);

				if (member.IsExplicitImpl)
					container.TypeBuilder.DefineMethodOverride (
						builder, implementing);

			}

			TypeManager.AddMethod (builder, method);

			if (GenericMethod != null) {
				bool is_override = member.IsExplicitImpl |
					((modifiers & Modifiers.OVERRIDE) != 0);

				if (implementing != null)
					parent_method = implementing;

				if (!GenericMethod.DefineType (ec, builder, parent_method, is_override))
					return false;
			}

			return true;
		}


		/// <summary>
		/// Create the MethodBuilder for the method 
		/// </summary>
		void DefineMethodBuilder (TypeContainer container, string method_name, Type[] ParameterTypes)
		{
			if ((modifiers & Modifiers.EXTERN) != 0) {

				if (method.OptAttributes != null) {
					Attribute dllimport_attribute = method.OptAttributes.Search (TypeManager.dllimport_type);
					if (dllimport_attribute != null) {
						flags |= MethodAttributes.PinvokeImpl;
						builder = dllimport_attribute.DefinePInvokeMethod (
							container.TypeBuilder, method_name, flags,
							method.ReturnType, ParameterTypes);

						return;
					}
				}

				// for extern static method must be specified either DllImport attribute or MethodImplAttribute.
				// We are more strict than Microsoft and report CS0626 like error
				if (method.OptAttributes == null ||
					!method.OptAttributes.Contains (TypeManager.methodimpl_attr_type)) {
					Report.Error (626, method.Location,
						"`{0}' is marked as an external but has no DllImport attribute. Consider adding a DllImport attribute to specify the external implementation",
						member.GetSignatureForError ());
					return;
				}
			}

			if (builder == null) {
				builder = container.TypeBuilder.DefineMethod (
					method_name, flags, method.CallingConventions,
					method.ReturnType, ParameterTypes);
				return;
			}

#if GMCS_SOURCE && !MS_COMPATIBLE
			builder.SetGenericMethodSignature (
				flags, method.CallingConventions,
				method.ReturnType, ParameterTypes);
#endif
		}

		//
		// Emits the code
		// 
		public void Emit (DeclSpace parent)
		{
			EmitContext ec;
			if ((flags & MethodAttributes.PinvokeImpl) == 0)
				ec = method.CreateEmitContext (parent, builder.GetILGenerator ());
			else
				ec = method.CreateEmitContext (parent, null);

			method.ParameterInfo.ApplyAttributes (MethodBuilder);

			if (GenericMethod != null)
				GenericMethod.EmitAttributes ();

			ToplevelBlock block = method.Block;
			
			SourceMethod source = SourceMethod.Create (parent, MethodBuilder, method.Block);

			//
			// Handle destructors specially
			//
			// FIXME: This code generates buggy code
			//
			if (member is Destructor)
				EmitDestructor (ec, block);
			else
				ec.EmitTopBlock (method, block);

			if (source != null)
				source.CloseMethod ();
		}

		void EmitDestructor (EmitContext ec, ToplevelBlock block)
		{
			ILGenerator ig = ec.ig;
			
			Label finish = ig.DefineLabel ();

			block.SetDestructor ();
			
			ig.BeginExceptionBlock ();
			ec.ReturnLabel = finish;
			ec.HasReturnLabel = true;
			ec.EmitTopBlock (method, block);
			
			// ig.MarkLabel (finish);
			ig.BeginFinallyBlock ();
			
			if (ec.ContainerType.BaseType != null) {
				Expression member_lookup = Expression.MemberLookup (
					ec.ContainerType.BaseType, null, ec.ContainerType.BaseType,
					"Finalize", MemberTypes.Method, Expression.AllBindingFlags, method.Location);

				if (member_lookup != null){
					MethodGroupExpr base_destructor = ((MethodGroupExpr) member_lookup);
				
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Call, (MethodInfo) base_destructor.Methods [0]);
				}
			}
			
			ig.EndExceptionBlock ();
			//ig.MarkLabel (ec.ReturnLabel);
			ig.Emit (OpCodes.Ret);
		}
	}

	// TODO: Should derive from MethodCore
	public class Destructor : Method {

		static string[] attribute_targets = new string [] { "method" };

		public Destructor (DeclSpace parent, Expression return_type, int mod,
				   string name, Parameters parameters, Attributes attrs,
				   Location l)
			: base (parent, null, return_type, mod, false, new MemberName (name, l),
				parameters, attrs)
		{ }

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.conditional_attribute_type) {
				Error_ConditionalAttributeIsNotValid ();
				return;
			}

			base.ApplyAttributeBuilder (a, cb);
		}

		public override string GetSignatureForError ()
		{
			return Parent.GetSignatureForError () + ".~" + Parent.MemberName.Name + "()";
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}
	}
	
	abstract public class MemberBase : MemberCore {
		public Expression Type;
		public readonly DeclSpace ds;
		public readonly GenericMethod GenericMethod;

		//
		// The type of this property / indexer / event
		//
		protected Type member_type;
		public Type MemberType {
			get {
				if (member_type == null && Type != null) {
					IResolveContext rc = GenericMethod == null ? this : (IResolveContext)ds;
					Type = Type.ResolveAsTypeTerminal (rc, false);
					if (Type != null) {
						member_type = Type.Type;
					}
				}
				return member_type;
			}
		}

		//
		// The constructor is only exposed to our children
		//
		protected MemberBase (DeclSpace parent, GenericMethod generic,
				      Expression type, int mod, int allowed_mod, int def_mod,
				      MemberName name, Attributes attrs)
			: base (parent, name, attrs)
		{
			this.ds = generic != null ? generic : (DeclSpace) parent;
			Type = type;
			ModFlags = Modifiers.Check (allowed_mod, mod, def_mod, Location);
			GenericMethod = generic;
			if (GenericMethod != null)
				GenericMethod.ModFlags = ModFlags;
		}

		protected virtual bool CheckBase ()
		{
  			if ((ModFlags & Modifiers.PROTECTED) != 0 && Parent.PartialContainer.Kind == Kind.Struct) {
  				Report.Error (666, Location, "`{0}': new protected member declared in struct", GetSignatureForError ());
  				return false;
   			}
   
  			if ((RootContext.WarningLevel >= 4) &&
			    ((Parent.ModFlags & Modifiers.SEALED) != 0) &&
			    ((ModFlags & Modifiers.PROTECTED) != 0) &&
			    ((ModFlags & Modifiers.OVERRIDE) == 0) && (Name != "Finalize")) {
  				Report.Warning (628, 4, Location, "`{0}': new protected member declared in sealed class", GetSignatureForError ());
   			}
				return true;
		}

		protected virtual bool DoDefine ()
		{
			if (MemberType == null)
				return false;

			if ((Parent.ModFlags & Modifiers.SEALED) != 0 && 
				(ModFlags & (Modifiers.VIRTUAL|Modifiers.ABSTRACT)) != 0) {
					Report.Error (549, Location, "New virtual member `{0}' is declared in a sealed class `{1}'",
						GetSignatureForError (), Parent.GetSignatureForError ());
					return false;
			}
			
			// verify accessibility
			if (!Parent.AsAccessible (MemberType, ModFlags)) {
				Report.SymbolRelatedToPreviousError (MemberType);
				if (this is Property)
					Report.Error (53, Location,
						      "Inconsistent accessibility: property type `" +
						      TypeManager.CSharpName (MemberType) + "' is less " +
						      "accessible than property `" + GetSignatureForError () + "'");
				else if (this is Indexer)
					Report.Error (54, Location,
						      "Inconsistent accessibility: indexer return type `" +
						      TypeManager.CSharpName (MemberType) + "' is less " +
						      "accessible than indexer `" + GetSignatureForError () + "'");
				else if (this is MethodCore) {
					if (this is Operator)
						Report.Error (56, Location,
							      "Inconsistent accessibility: return type `" +
							      TypeManager.CSharpName (MemberType) + "' is less " +
							      "accessible than operator `" + GetSignatureForError () + "'");
					else
						Report.Error (50, Location,
							      "Inconsistent accessibility: return type `" +
							      TypeManager.CSharpName (MemberType) + "' is less " +
							      "accessible than method `" + GetSignatureForError () + "'");
				} else {
					Report.Error (52, Location,
						      "Inconsistent accessibility: field type `" +
						      TypeManager.CSharpName (MemberType) + "' is less " +
						      "accessible than field `" + GetSignatureForError () + "'");
				}
				return false;
			}

			return true;
		}

		protected bool IsTypePermitted ()
		{
			if (MemberType == TypeManager.arg_iterator_type || MemberType == TypeManager.typed_reference_type) {
				Report.Error (610, Location, "Field or property cannot be of type `{0}'", TypeManager.CSharpName (MemberType));
				return false;
			}
			return true;
		}
	}

	//
	// Abstract class for all fields
	//
	abstract public class FieldBase : MemberBase {
		public FieldBuilder FieldBuilder;
		public Status status;
		protected Expression initializer;

		[Flags]
		public enum Status : byte {
			HAS_OFFSET = 4		// Used by FieldMember.
		}

		static readonly string[] attribute_targets = new string [] { "field" };

		protected FieldBase (DeclSpace parent, Expression type, int mod,
				     int allowed_mod, MemberName name, Attributes attrs)
			: base (parent, null, type, mod, allowed_mod | Modifiers.ABSTRACT, Modifiers.PRIVATE,
				name, attrs)
		{
			if ((mod & Modifiers.ABSTRACT) != 0)
				Report.Error (681, Location, "The modifier 'abstract' is not valid on fields. Try using a property instead");
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Field;
			}
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.field_offset_attribute_type) {
				status |= Status.HAS_OFFSET;

				if (!Parent.PartialContainer.HasExplicitLayout) {
					Report.Error (636, Location, "The FieldOffset attribute can only be placed on members of types marked with the StructLayout(LayoutKind.Explicit)");
					return;
				}

				if ((ModFlags & Modifiers.STATIC) != 0 || this is Const) {
					Report.Error (637, Location, "The FieldOffset attribute is not allowed on static or const fields");
					return;
				}
			}

#if NET_2_0
			if (a.Type == TypeManager.fixed_buffer_attr_type) {
				Report.Error (1716, Location, "Do not use 'System.Runtime.CompilerServices.FixedBuffer' attribute. Use the 'fixed' field modifier instead");
				return;
			}
#endif

			if (a.Type == TypeManager.marshal_as_attr_type) {
				UnmanagedMarshal marshal = a.GetMarshal (this);
				if (marshal != null) {
					FieldBuilder.SetMarshal (marshal);
				}
				return;
			}

			if (a.Type.IsSubclassOf (TypeManager.security_attr_type)) {
				a.Error_InvalidSecurityParent ();
				return;
			}

			FieldBuilder.SetCustomAttribute (cb);
		}

 		protected override bool CheckBase ()
		{
 			if (!base.CheckBase ())
 				return false;
 
 			MemberInfo conflict_symbol = Parent.PartialContainer.FindBaseMemberWithSameName (Name, false);
 			if (conflict_symbol == null) {
 				if ((ModFlags & Modifiers.NEW) != 0) {
 					Report.Warning (109, 4, Location, "The member `{0}' does not hide an inherited member. The new keyword is not required", GetSignatureForError ());
 				}
 				return true;
 			}
 
 			if ((ModFlags & (Modifiers.NEW | Modifiers.OVERRIDE)) == 0) {
				Report.SymbolRelatedToPreviousError (conflict_symbol);
				Report.Warning (108, 2, Location, "`{0}' hides inherited member `{1}'. Use the new keyword if hiding was intended",
					GetSignatureForError (), TypeManager.GetFullNameSignature (conflict_symbol));
			}
 
 			return true;
 		}

		public override bool Define()
		{
			if (MemberType == null || Type == null)
				return false;

			if (MemberType == TypeManager.void_type) {
				// TODO: wrong location
				Expression.Error_VoidInvalidInTheContext (Location);
				return false;
			}

			if (MemberType.IsSealed && MemberType.IsAbstract) {
				Error_VariableOfStaticClass (Location, GetSignatureForError (), MemberType);
				return false;
			}

			if (!CheckBase ())
				return false;
			
			if (!Parent.AsAccessible (MemberType, ModFlags)) {
				Report.Error (52, Location,
					"Inconsistent accessibility: field type `" +
					TypeManager.CSharpName (MemberType) + "' is less " +
					"accessible than field `" + GetSignatureForError () + "'");
				return false;
			}

			if (!IsTypePermitted ())
				return false;

			return true;
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "F:"; }
		}

		public override void Emit ()
		{
			if (OptAttributes != null) {
				OptAttributes.Emit ();
			}

			if (((status & Status.HAS_OFFSET) == 0) && (ModFlags & Modifiers.STATIC) == 0 && Parent.PartialContainer.HasExplicitLayout) {
				Report.Error (625, Location, "`{0}': Instance field types marked with StructLayout(LayoutKind.Explicit) must have a FieldOffset attribute.", GetSignatureForError ());
			}

			base.Emit ();
		}

		public static void Error_VariableOfStaticClass (Location loc, string variableName, Type staticClass)
		{
			Report.SymbolRelatedToPreviousError (staticClass);
			Report.Error (723, loc, "`{0}': cannot declare variables of static types",
				variableName);
		}

		public Expression Initializer {
			set {
				if (value != null) {
					this.initializer = value;
				}
			}
		}

		protected virtual bool IsFieldClsCompliant {
			get {
				if (FieldBuilder == null)
					return true;

				return AttributeTester.IsClsCompliant (FieldBuilder.FieldType);
			}
		}

		public override string[] ValidAttributeTargets 
		{
			get {
				return attribute_targets;
			}
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if (!IsFieldClsCompliant) {
				Report.Error (3003, Location, "Type of `{0}' is not CLS-compliant", GetSignatureForError ());
			}
			return true;
		}

		public void SetAssigned ()
		{
			caching_flags |= Flags.IsAssigned;
		}
	}

	interface IFixedBuffer
	{
		FieldInfo Element { get; }
		Type ElementType { get; }
	}

	public class FixedFieldExternal: IFixedBuffer
	{
		FieldInfo element_field;

		public FixedFieldExternal (FieldInfo fi)
		{
			element_field = fi.FieldType.GetField (FixedField.FixedElementName);
		}

		#region IFixedField Members

		public FieldInfo Element {
			get {
				return element_field;
			}
		}

		public Type ElementType {
			get {
				return element_field.FieldType;
			}
		}

		#endregion
	}

	/// <summary>
	/// Fixed buffer implementation
	/// </summary>
	public class FixedField : FieldBase, IFixedBuffer
	{
		public const string FixedElementName = "FixedElementField";
		static int GlobalCounter = 0;
		static object[] ctor_args = new object[] { (short)LayoutKind.Sequential };
		static FieldInfo[] fi;

		TypeBuilder fixed_buffer_type;
		FieldBuilder element;
		Expression size_expr;
		int buffer_size;

		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public FixedField (DeclSpace parent, Expression type, int mod, string name,
			Expression size_expr, Attributes attrs, Location loc):
			base (parent, type, mod, AllowedModifiers, new MemberName (name, loc), attrs)
		{
			if (RootContext.Version == LanguageVersion.ISO_1)
				Report.FeatureIsNotStandardized (loc, "fixed size buffers");

			this.size_expr = size_expr;
		}

		public override bool Define()
		{
#if !NET_2_0
			if ((ModFlags & (Modifiers.PUBLIC | Modifiers.PROTECTED)) != 0)
				Report.Warning (-23, 1, Location, "Only private or internal fixed sized buffers are supported by .NET 1.x");
#endif

			if (Parent.PartialContainer.Kind != Kind.Struct) {
				Report.Error (1642, Location, "`{0}': Fixed size buffer fields may only be members of structs",
					GetSignatureForError ());
				return false;
			}

			if (!base.Define ())
				return false;

			if (!TypeManager.IsPrimitiveType (MemberType)) {
				Report.Error (1663, Location, "`{0}': Fixed size buffers type must be one of the following: bool, byte, short, int, long, char, sbyte, ushort, uint, ulong, float or double",
					GetSignatureForError ());
				return false;
			}

			EmitContext ec = new EmitContext (this, Parent, Location, null, null, ModFlags);
			Constant c = size_expr.ResolveAsConstant (ec, this);
			if (c == null)
				return false;

			IntConstant buffer_size_const = c.ImplicitConversionRequired (TypeManager.int32_type, Location) as IntConstant;
			if (buffer_size_const == null)
				return false;

			buffer_size = buffer_size_const.Value;

			if (buffer_size <= 0) {
				Report.Error (1665, Location, "`{0}': Fixed size buffers must have a length greater than zero", GetSignatureForError ());
				return false;
			}

			int type_size = Expression.GetTypeSize (MemberType);

			if (buffer_size > int.MaxValue / type_size) {
				Report.Error (1664, Location, "Fixed size buffer `{0}' of length `{1}' and type `{2}' exceeded 2^31 limit",
					GetSignatureForError (), buffer_size.ToString (), TypeManager.CSharpName (MemberType));
				return false;
			}

			buffer_size *= type_size;

			// Define nested
			string name = String.Format ("<{0}>__FixedBuffer{1}", Name, GlobalCounter++);

			fixed_buffer_type = Parent.TypeBuilder.DefineNestedType (name,
				TypeAttributes.NestedPublic | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit, TypeManager.value_type);
			element = fixed_buffer_type.DefineField (FixedElementName, MemberType, FieldAttributes.Public);
			RootContext.RegisterCompilerGeneratedType (fixed_buffer_type);

			FieldBuilder = Parent.TypeBuilder.DefineField (Name, fixed_buffer_type, Modifiers.FieldAttr (ModFlags));
			TypeManager.RegisterFieldBase (FieldBuilder, this);

			return true;
		}

		public override void Emit()
		{
			if (fi == null)
				fi = new FieldInfo [] { TypeManager.struct_layout_attribute_type.GetField ("Size") };

			object[] fi_val = new object[1];
			fi_val [0] = buffer_size;

			CustomAttributeBuilder cab = new CustomAttributeBuilder (TypeManager.struct_layout_attribute_ctor, 
				ctor_args, fi, fi_val);
			fixed_buffer_type.SetCustomAttribute (cab);

#if NET_2_0
			cab = new CustomAttributeBuilder (TypeManager.fixed_buffer_attr_ctor, new object[] { MemberType, buffer_size } );
			FieldBuilder.SetCustomAttribute (cab);
#endif
			base.Emit ();
		}

		protected override bool IsFieldClsCompliant {
			get {
				return false;
			}
		}

		#region IFixedField Members

		public FieldInfo Element {
			get {
				return element;
			}
		}

		public Type ElementType {
			get {
				return MemberType;
			}
		}

		#endregion
	}

	//
	// The Field class is used to represents class/struct fields during parsing.
	//
	public class Field : FieldBase {
		// <summary>
		//   Modifiers allowed in a class declaration
		// </summary>
		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
		        Modifiers.VOLATILE |
		        Modifiers.UNSAFE |
			Modifiers.READONLY;

		public Field (DeclSpace parent, Expression type, int mod, string name,
			      Attributes attrs, Location loc)
			: base (parent, type, mod, AllowedModifiers, new MemberName (name, loc),
				attrs)
		{
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			if (RootContext.WarningLevel > 1){
				Type ptype = Parent.TypeBuilder.BaseType;

				// ptype is only null for System.Object while compiling corlib.
				if (ptype != null){
					TypeContainer.FindMembers (
						ptype, MemberTypes.Method,
						BindingFlags.Public |
						BindingFlags.Static | BindingFlags.Instance,
						System.Type.FilterName, Name);
				}
			}

			if ((ModFlags & Modifiers.VOLATILE) != 0){
				if (!MemberType.IsClass){
					Type vt = MemberType;
					
					if (TypeManager.IsEnumType (vt))
						vt = TypeManager.EnumToUnderlying (MemberType);

					if (!((vt == TypeManager.bool_type) ||
					      (vt == TypeManager.sbyte_type) ||
					      (vt == TypeManager.byte_type) ||
					      (vt == TypeManager.short_type) ||
					      (vt == TypeManager.ushort_type) ||
					      (vt == TypeManager.int32_type) ||
					      (vt == TypeManager.uint32_type) ||    
					      (vt == TypeManager.char_type) ||
					      (vt == TypeManager.float_type) ||
					      (!vt.IsValueType))){
						Report.Error (677, Location, "`{0}': A volatile field cannot be of the type `{1}'",
							GetSignatureForError (), TypeManager.CSharpName (vt));
						return false;
					}
				}

				if ((ModFlags & Modifiers.READONLY) != 0){
					Report.Error (678, Location, "`{0}': A field cannot be both volatile and readonly",
						GetSignatureForError ());
					return false;
				}
			}

			FieldAttributes fa = Modifiers.FieldAttr (ModFlags);

			if (Parent.PartialContainer.Kind == Kind.Struct && 
			    ((fa & FieldAttributes.Static) == 0) &&
			    MemberType == Parent.TypeBuilder &&
			    !TypeManager.IsBuiltinType (MemberType)){
				Report.Error (523, Location, "Struct member `" + Parent.Name + "." + Name + 
					      "' causes a cycle in the structure layout");
				return false;
			}

			try {
				FieldBuilder = Parent.TypeBuilder.DefineField (
					Name, MemberType, Modifiers.FieldAttr (ModFlags));

				TypeManager.RegisterFieldBase (FieldBuilder, this);
			}
			catch (ArgumentException) {
				Report.Warning (-24, 1, Location, "The Microsoft runtime is unable to use [void|void*] as a field type, try using the Mono runtime.");
				return false;
			}

			if (initializer != null)
				Parent.PartialContainer.RegisterFieldForInitialization (this,
					new FieldInitializer (FieldBuilder, initializer));

			return true;
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if ((ModFlags & Modifiers.VOLATILE) != 0) {
				Report.Warning (3026, 1, Location, "CLS-compliant field `{0}' cannot be volatile", GetSignatureForError ());
			}

			return true;
		}
	}

	//
	// `set' and `get' accessors are represented with an Accessor.
	// 
	public class Accessor : IAnonymousHost {
		//
		// Null if the accessor is empty, or a Block if not
		//
		public const int AllowedModifiers = 
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;
		
		public ToplevelBlock Block;
		public Attributes Attributes;
		public Location Location;
		public int ModFlags;
		public bool Yields;
		public ArrayList AnonymousMethods;
		
		public Accessor (ToplevelBlock b, int mod, Attributes attrs, Location loc)
		{
			Block = b;
			Attributes = attrs;
			Location = loc;
			ModFlags = Modifiers.Check (AllowedModifiers, mod, 0, loc);
		}

		public void SetYields ()
		{
			Yields = true;
		}

		public void AddAnonymousMethod (AnonymousMethodExpression ame)
		{
			if (AnonymousMethods == null)
				AnonymousMethods = new ArrayList ();
			AnonymousMethods.Add (ame);
		}
	}

	// Ooouh Martin, templates are missing here.
	// When it will be possible move here a lot of child code and template method type.
	public abstract class AbstractPropertyEventMethod : MemberCore, IMethodData {
		protected MethodData method_data;
		protected ToplevelBlock block;
		protected ListDictionary declarative_security;

		// The accessor are created event if they are not wanted.
		// But we need them because their names are reserved.
		// Field says whether accessor will be emited or not
		public readonly bool IsDummy;

		protected readonly string prefix;

		ReturnParameter return_attributes;

		public AbstractPropertyEventMethod (PropertyBasedMember member, string prefix)
			: base (member.Parent, SetupName (prefix, member, member.Location), null)
		{
			this.prefix = prefix;
			IsDummy = true;
		}

		public AbstractPropertyEventMethod (InterfaceMemberBase member, Accessor accessor,
						    string prefix)
			: base (member.Parent, SetupName (prefix, member, accessor.Location),
				accessor.Attributes)
		{
			this.prefix = prefix;
			this.block = accessor.Block;
		}

		static MemberName SetupName (string prefix, InterfaceMemberBase member, Location loc)
		{
			return new MemberName (member.MemberName.Left, prefix + member.ShortName, loc);
		}

		public void UpdateName (InterfaceMemberBase member)
		{
			SetMemberName (SetupName (prefix, member, Location));
		}

		#region IMethodData Members

		public abstract Iterator Iterator {
			get;
		}

		public ToplevelBlock Block {
			get {
				return block;
			}

			set {
				block = value;
			}
		}

		public CallingConventions CallingConventions {
			get {
				return CallingConventions.Standard;
			}
		}

		public bool IsExcluded ()
		{
			return false;
		}

		GenericMethod IMethodData.GenericMethod {
			get {
				return null;
			}
		}

		public MemberName MethodName {
			get {
				return MemberName;
			}
		}

		public Type[] ParameterTypes { 
			get {
				return ParameterInfo.Types;
			}
		}

		public abstract Parameters ParameterInfo { get ; }
		public abstract Type ReturnType { get; }
		public abstract EmitContext CreateEmitContext (DeclSpace ds, ILGenerator ig);

		#endregion

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.cls_compliant_attribute_type || a.Type == TypeManager.obsolete_attribute_type ||
					a.Type == TypeManager.conditional_attribute_type) {
				Report.Error (1667, a.Location,
					"Attribute `{0}' is not valid on property or event accessors. It is valid on `{1}' declarations only",
					TypeManager.CSharpName (a.Type), a.GetValidTargets ());
				return;
			}

			if (a.Type.IsSubclassOf (TypeManager.security_attr_type) && a.CheckSecurityActionValidity (false)) {
				if (declarative_security == null)
					declarative_security = new ListDictionary ();
				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (a.Target == AttributeTargets.Method) {
				method_data.MethodBuilder.SetCustomAttribute (cb);
				return;
			}

			if (a.Target == AttributeTargets.ReturnValue) {
				if (return_attributes == null)
					return_attributes = new ReturnParameter (method_data.MethodBuilder, Location);

				return_attributes.ApplyAttributeBuilder (a, cb);
				return;
			}

			ApplyToExtraTarget (a, cb);
		}

		virtual protected void ApplyToExtraTarget (Attribute a, CustomAttributeBuilder cb)
		{
			System.Diagnostics.Debug.Fail ("You forgot to define special attribute target handling");
		}

		// It is not supported for the accessors
		public sealed override bool Define()
		{
			throw new NotSupportedException ();
		}

		public void Emit (DeclSpace parent)
		{
			EmitMethod (parent);

			if (OptAttributes != null)
				OptAttributes.Emit ();

			if (declarative_security != null) {
				foreach (DictionaryEntry de in declarative_security) {
					method_data.MethodBuilder.AddDeclarativeSecurity ((SecurityAction)de.Key, (PermissionSet)de.Value);
				}
			}

			block = null;
		}

		protected virtual void EmitMethod (DeclSpace parent)
		{
			method_data.Emit (parent);
		}

		public override bool IsClsComplianceRequired()
		{
			return false;
		}

		public bool IsDuplicateImplementation (MethodCore method)
		{
			if (!MemberName.Equals (method.MemberName))
				return false;

			Type[] param_types = method.ParameterTypes;

			if (param_types.Length != ParameterTypes.Length)
				return false;

			for (int i = 0; i < param_types.Length; i++)
				if (param_types [i] != ParameterTypes [i])
					return false;

			Report.SymbolRelatedToPreviousError (method);
			Report.Error (111, Location, TypeContainer.Error111, method.GetSignatureForError ());
			return true;
		}

		public override bool IsUsed
		{
			get {
				if (IsDummy)
					return false;

				return base.IsUsed;
			}
		}

		public new Location Location { 
			get {
				return base.Location;
			}
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { throw new InvalidOperationException ("Unexpected attempt to get doc comment from " + this.GetType () + "."); }
		}

	}

	//
	// Properties and Indexers both generate PropertyBuilders, we use this to share 
	// their common bits.
	//
	abstract public class PropertyBase : PropertyBasedMember {

		public class GetMethod : PropertyMethod
		{
			static string[] attribute_targets = new string [] { "method", "return" };

			public GetMethod (PropertyBase method):
				base (method, "get_")
			{
			}

			public GetMethod (PropertyBase method, Accessor accessor):
				base (method, accessor, "get_")
			{
			}

			public override MethodBuilder Define (DeclSpace parent)
			{
				base.Define (parent);
				
				method_data = new MethodData (method, ModFlags, flags, this);

				if (!method_data.Define (parent))
					return null;

				return method_data.MethodBuilder;
			}

			public override Type ReturnType {
				get {
					return method.MemberType;
				}
			}

			public override Parameters ParameterInfo {
				get {
					return Parameters.EmptyReadOnlyParameters;
				}
			}

			public override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
				}
			}
		}

		public class SetMethod : PropertyMethod {

			static string[] attribute_targets = new string [] { "method", "param", "return" };
			ImplicitParameter param_attr;
			protected Parameters parameters;

			public SetMethod (PropertyBase method):
				base (method, "set_")
			{
			}

			public SetMethod (PropertyBase method, Accessor accessor):
				base (method, accessor, "set_")
			{
			}

			protected override void ApplyToExtraTarget(Attribute a, CustomAttributeBuilder cb)
			{
				if (a.Target == AttributeTargets.Parameter) {
					if (param_attr == null)
						param_attr = new ImplicitParameter (method_data.MethodBuilder);

					param_attr.ApplyAttributeBuilder (a, cb);
					return;
				}

				base.ApplyAttributeBuilder (a, cb);
			}

			public override Parameters ParameterInfo {
				get {
					if (parameters == null)
						DefineParameters ();
					return parameters;
				}
			}

			protected virtual void DefineParameters ()
			{
				parameters = new Parameters (
					new Parameter[] { new Parameter (method.MemberType, "value", Parameter.Modifier.NONE, null, Location) },
					new Type[] { method.MemberType });
			}

			public override MethodBuilder Define (DeclSpace parent)
			{
				if (IsDummy)
					return null;

				base.Define (parent);

				method_data = new MethodData (method, ModFlags, flags, this);

				if (!method_data.Define (parent))
					return null;

				return method_data.MethodBuilder;
			}

			public override Type ReturnType {
				get {
					return TypeManager.void_type;
				}
			}

			public override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
				}
			}
		}

		static string[] attribute_targets = new string [] { "property" };

		public abstract class PropertyMethod : AbstractPropertyEventMethod
		{
			protected readonly PropertyBase method;
			protected MethodAttributes flags;
			Iterator iterator;
			ArrayList anonymous_methods;
			bool yields;

			public PropertyMethod (PropertyBase method, string prefix)
				: base (method, prefix)
			{
				this.method = method;
			}

			public PropertyMethod (PropertyBase method, Accessor accessor,
					       string prefix)
				: base (method, accessor, prefix)
			{
				this.method = method;
				this.ModFlags = accessor.ModFlags;
				yields = accessor.Yields;
				anonymous_methods = accessor.AnonymousMethods;

				if (accessor.ModFlags != 0 && RootContext.Version == LanguageVersion.ISO_1) {
					Report.FeatureIsNotStandardized (Location, "access modifiers on properties");
				}
			}

			public override Iterator Iterator {
				get { return iterator; }
			}

			public override AttributeTargets AttributeTargets {
				get {
					return AttributeTargets.Method;
				}
			}

			public override bool IsClsComplianceRequired ()
			{
				return method.IsClsComplianceRequired ();
			}

			public bool ResolveMembers ()
			{
				if (yields) {
					iterator = Iterator.CreateIterator (this, Parent, null, ModFlags);
					if (iterator == null)
						return false;
				}

				if (anonymous_methods != null) {
					foreach (AnonymousMethodExpression ame in anonymous_methods) {
						if (!ame.CreateAnonymousHelpers ())
							return false;
					}
				}

				return true;
			}

			public virtual MethodBuilder Define (DeclSpace parent)
			{
				if (!method.CheckAbstractAndExtern (block != null))
					return null;

				TypeContainer container = parent.PartialContainer;

				//
				// Check for custom access modifier
				//
				if (ModFlags == 0) {
					ModFlags = method.ModFlags;
					flags = method.flags;
				} else {
					if (container.Kind == Kind.Interface)
						Report.Error (275, Location, "`{0}': accessibility modifiers may not be used on accessors in an interface",
							GetSignatureForError ());

					if ((method.ModFlags & Modifiers.ABSTRACT) != 0 && (ModFlags & Modifiers.PRIVATE) != 0) {
						Report.Error (442, Location, "`{0}': abstract properties cannot have private accessors", GetSignatureForError ());
					}

					CheckModifiers (ModFlags);
					ModFlags |= (method.ModFlags & (~Modifiers.Accessibility));
					ModFlags |= Modifiers.PROPERTY_CUSTOM;
					flags = Modifiers.MethodAttr (ModFlags);
					flags |= (method.flags & (~MethodAttributes.MemberAccessMask));
				}

				return null;
			}

			public bool HasCustomAccessModifier
			{
				get {
					return (ModFlags & Modifiers.PROPERTY_CUSTOM) != 0;
				}
			}

			public override EmitContext CreateEmitContext (DeclSpace ds, ILGenerator ig)
			{
				return new EmitContext (method,
					ds, method.ds, method.Location, ig, ReturnType,
					method.ModFlags, false);
			}

			public override ObsoleteAttribute GetObsoleteAttribute ()
			{
				return method.GetObsoleteAttribute ();
			}

			public override string GetSignatureForError()
			{
				return method.GetSignatureForError () + '.' + prefix.Substring (0, 3);
			}
			
			void CheckModifiers (int modflags)
			{
				int flags = 0;
				int mflags = method.ModFlags & Modifiers.Accessibility;

				if ((mflags & Modifiers.PUBLIC) != 0) {
					flags |= Modifiers.PROTECTED | Modifiers.INTERNAL | Modifiers.PRIVATE;
				}
				else if ((mflags & Modifiers.PROTECTED) != 0) {
					if ((mflags & Modifiers.INTERNAL) != 0)
						flags |= Modifiers.PROTECTED | Modifiers.INTERNAL;

					flags |= Modifiers.PRIVATE;
				}
				else if ((mflags & Modifiers.INTERNAL) != 0)
					flags |= Modifiers.PRIVATE;

				if ((mflags == modflags) || (modflags & (~flags)) != 0) {
					Report.Error (273, Location,
						"The accessibility modifier of the `{0}' accessor must be more restrictive than the modifier of the property or indexer `{1}'",
						GetSignatureForError (), method.GetSignatureForError ());
				}
			}

			public override bool MarkForDuplicationCheck ()
			{
				caching_flags |= Flags.TestMethodDuplication;
				return true;
			}
		}

		public PropertyMethod Get, Set;
		public PropertyBuilder PropertyBuilder;
		public MethodBuilder GetBuilder, SetBuilder;

		protected bool define_set_first = false;

		public PropertyBase (DeclSpace parent, Expression type, int mod_flags,
				     int allowed_mod, bool is_iface, MemberName name,
				     Attributes attrs, bool define_set_first)
			: base (parent, null, type, mod_flags, allowed_mod, is_iface, name,	attrs)
		{
			 this.define_set_first = define_set_first;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type.IsSubclassOf (TypeManager.security_attr_type)) {
				a.Error_InvalidSecurityParent ();
				return;
			}

			PropertyBuilder.SetCustomAttribute (cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Property;
			}
		}

		public override bool Define ()
		{
			if (!DoDefine ())
				return false;

			if (!IsTypePermitted ())
				return false;

			return true;
		}

		protected override bool DoDefine ()
		{
			if (!base.DoDefine ())
				return false;

			//
			// Accessors modifiers check
			//
			if (Get.ModFlags != 0 && Set.ModFlags != 0) {
				Report.Error (274, Location, "`{0}': Cannot specify accessibility modifiers for both accessors of the property or indexer",
						GetSignatureForError ());
				return false;
			}

			if ((Get.IsDummy || Set.IsDummy)
					&& (Get.ModFlags != 0 || Set.ModFlags != 0) && (ModFlags & Modifiers.OVERRIDE) == 0) {
				Report.Error (276, Location, 
					"`{0}': accessibility modifiers on accessors may only be used if the property or indexer has both a get and a set accessor",
					GetSignatureForError ());
				return false;
			}

			if (MemberType.IsAbstract && MemberType.IsSealed) {
				Report.Error (722, Location, Error722, TypeManager.CSharpName (MemberType));
				return false;
			}

			return true;
		}

		bool DefineGet ()
		{
			if (Get.IsDummy)
				return true;

			GetBuilder = Get.Define (Parent);
			return GetBuilder != null;
		}

		bool DefineSet (bool define)
		{
			if (!define || Set.IsDummy)
				return true;

			SetBuilder = Set.Define (Parent);
			return SetBuilder != null;
		}

		protected bool DefineAccessors ()
		{
			return DefineSet (define_set_first) &&
				DefineGet () &&
				DefineSet (!define_set_first);
		}

		protected abstract PropertyInfo ResolveBaseProperty ();

		// TODO: rename to Resolve......
 		protected override MethodInfo FindOutBaseMethod (ref Type base_ret_type)
 		{
 			PropertyInfo base_property = ResolveBaseProperty ();
 			if (base_property == null)
 				return null;
  
 			base_ret_type = base_property.PropertyType;
			MethodInfo get_accessor = base_property.GetGetMethod (true);
			MethodInfo set_accessor = base_property.GetSetMethod (true);
			MethodAttributes get_accessor_access, set_accessor_access;

			if ((ModFlags & Modifiers.OVERRIDE) != 0) {
				if (Get != null && !Get.IsDummy && get_accessor == null) {
					Report.SymbolRelatedToPreviousError (base_property);
					Report.Error (545, Location, "`{0}.get': cannot override because `{1}' does not have an overridable get accessor", GetSignatureForError (), TypeManager.GetFullNameSignature (base_property));
				}

				if (Set != null && !Set.IsDummy && set_accessor == null) {
					Report.SymbolRelatedToPreviousError (base_property);
					Report.Error (546, Location, "`{0}.set': cannot override because `{1}' does not have an overridable set accessor", GetSignatureForError (), TypeManager.GetFullNameSignature (base_property));
				}
			}
			
			//
			// Check base class accessors access
			//

			// TODO: rewrite to reuse Get|Set.CheckAccessModifiers and share code there
			get_accessor_access = set_accessor_access = 0;
			if ((ModFlags & Modifiers.NEW) == 0) {
				if (get_accessor != null) {
					MethodAttributes get_flags = Modifiers.MethodAttr (Get.ModFlags != 0 ? Get.ModFlags : ModFlags);
					get_accessor_access = (get_accessor.Attributes & MethodAttributes.MemberAccessMask);

					if (!Get.IsDummy && !CheckAccessModifiers (get_flags & MethodAttributes.MemberAccessMask, get_accessor_access, get_accessor))
						Error_CannotChangeAccessModifiers (get_accessor, get_accessor_access,  ".get");
				}

				if (set_accessor != null)  {
					MethodAttributes set_flags = Modifiers.MethodAttr (Set.ModFlags != 0 ? Set.ModFlags : ModFlags);
					set_accessor_access = (set_accessor.Attributes & MethodAttributes.MemberAccessMask);

					if (!Set.IsDummy && !CheckAccessModifiers (set_flags & MethodAttributes.MemberAccessMask, set_accessor_access, set_accessor))
						Error_CannotChangeAccessModifiers (set_accessor, set_accessor_access, ".set");
				}
			}

			//
			// Get the less restrictive access
			//
			return get_accessor_access > set_accessor_access ? get_accessor : set_accessor;
		}

		public override void Emit ()
		{
			//
			// The PropertyBuilder can be null for explicit implementations, in that
			// case, we do not actually emit the ".property", so there is nowhere to
			// put the attribute
			//
			if (PropertyBuilder != null && OptAttributes != null)
				OptAttributes.Emit ();

			if (!Get.IsDummy)
				Get.Emit (Parent);

			if (!Set.IsDummy)
				Set.Emit (Parent);

			base.Emit ();
		}

		/// <summary>
		/// Tests whether accessors are not in collision with some method (CS0111)
		/// </summary>
		public bool AreAccessorsDuplicateImplementation (MethodCore mc)
		{
			return Get.IsDuplicateImplementation (mc) || Set.IsDuplicateImplementation (mc);
		}

		public override bool IsUsed
		{
			get {
				if (IsExplicitImpl)
					return true;

				return Get.IsUsed | Set.IsUsed;
			}
		}

		protected override void SetMemberName (MemberName new_name)
		{
			base.SetMemberName (new_name);

			Get.UpdateName (this);
			Set.UpdateName (this);
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "P:"; }
		}
	}
			
	public class Property : PropertyBase {
		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.ABSTRACT |
		        Modifiers.UNSAFE |
			Modifiers.EXTERN |
			Modifiers.METHOD_YIELDS |
			Modifiers.VIRTUAL;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW;

		public Property (DeclSpace parent, Expression type, int mod, bool is_iface,
				 MemberName name, Attributes attrs, Accessor get_block,
				 Accessor set_block, bool define_set_first)
			: base (parent, type, mod,
				is_iface ? AllowedInterfaceModifiers : AllowedModifiers,
				is_iface, name, attrs, define_set_first)
		{
			if (get_block == null)
				Get = new GetMethod (this);
			else
				Get = new GetMethod (this, get_block);

			if (set_block == null)
				Set = new SetMethod (this);
			else
				Set = new SetMethod (this, set_block);
		}

		public override bool Define ()
		{
			if (!DoDefineBase ())
				return false;

			if (!base.Define ())
				return false;

			if (!CheckBase ())
				return false;

			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;

			if (!DefineAccessors ())
				return false;

			// FIXME - PropertyAttributes.HasDefault ?

			PropertyBuilder = Parent.TypeBuilder.DefineProperty (
				MemberName.ToString (), PropertyAttributes.None, MemberType, null);

			if (!Get.IsDummy)
				PropertyBuilder.SetGetMethod (GetBuilder);
				
			if (!Set.IsDummy)
				PropertyBuilder.SetSetMethod (SetBuilder);
			
			TypeManager.RegisterProperty (PropertyBuilder, this);
			return true;
		}

		protected override PropertyInfo ResolveBaseProperty ()
		{
			return Parent.PartialContainer.BaseCache.FindMemberToOverride (
				Parent.TypeBuilder, Name, Parameters.EmptyReadOnlyParameters.Types, null, true) as PropertyInfo;
		}
	}

	/// </summary>
	///  Gigantic workaround  for lameness in SRE follows :
	///  This class derives from EventInfo and attempts to basically
	///  wrap around the EventBuilder so that FindMembers can quickly
	///  return this in it search for members
	/// </summary>
	public class MyEventBuilder : EventInfo {
		
		//
		// We use this to "point" to our Builder which is
		// not really a MemberInfo
		//
		EventBuilder MyBuilder;
		
		//
		// We "catch" and wrap these methods
		//
		MethodInfo raise, remove, add;

		EventAttributes attributes;
		Type declaring_type, reflected_type, event_type;
		string name;

		Event my_event;

		public MyEventBuilder (Event ev, TypeBuilder type_builder, string name, EventAttributes event_attr, Type event_type)
		{
			MyBuilder = type_builder.DefineEvent (name, event_attr, event_type);

			// And now store the values in our own fields.
			
			declaring_type = type_builder;

			reflected_type = type_builder;
			
			attributes = event_attr;
			this.name = name;
			my_event = ev;
			this.event_type = event_type;
		}
		
		//
		// Methods that you have to override.  Note that you only need 
		// to "implement" the variants that take the argument (those are
		// the "abstract" methods, the others (GetAddMethod()) are 
		// regular.
		//
		public override MethodInfo GetAddMethod (bool nonPublic)
		{
			return add;
		}
		
		public override MethodInfo GetRemoveMethod (bool nonPublic)
		{
			return remove;
		}
		
		public override MethodInfo GetRaiseMethod (bool nonPublic)
		{
			return raise;
		}
		
		//
		// These methods make "MyEventInfo" look like a Builder
		//
		public void SetRaiseMethod (MethodBuilder raiseMethod)
		{
			raise = raiseMethod;
			MyBuilder.SetRaiseMethod (raiseMethod);
		}

		public void SetRemoveOnMethod (MethodBuilder removeMethod)
		{
			remove = removeMethod;
			MyBuilder.SetRemoveOnMethod (removeMethod);
		}

		public void SetAddOnMethod (MethodBuilder addMethod)
		{
			add = addMethod;
			MyBuilder.SetAddOnMethod (addMethod);
		}

		public void SetCustomAttribute (CustomAttributeBuilder cb)
		{
			MyBuilder.SetCustomAttribute (cb);
		}
		
		public override object [] GetCustomAttributes (bool inherit)
		{
			// FIXME : There's nothing which can be seemingly done here because
			// we have no way of getting at the custom attribute objects of the
			// EventBuilder !
			return null;
		}

		public override object [] GetCustomAttributes (Type t, bool inherit)
		{
			// FIXME : Same here !
			return null;
		}

		public override bool IsDefined (Type t, bool b)
		{
			return true;
		}

		public override EventAttributes Attributes {
			get {
				return attributes;
			}
		}

		public override string Name {
			get {
				return name;
			}
		}

		public override Type DeclaringType {
			get {
				return declaring_type;
			}
		}

		public override Type ReflectedType {
			get {
				return reflected_type;
			}
		}

		public Type EventType {
			get {
				return event_type;
			}
		}
		
		public void SetUsed ()
		{
			if (my_event != null) {
//				my_event.SetAssigned ();
				my_event.SetMemberIsUsed ();
			}
		}
	}
	
	/// <summary>
	/// For case when event is declared like property (with add and remove accessors).
	/// </summary>
	public class EventProperty: Event {
		abstract class AEventPropertyAccessor : AEventAccessor
		{
			public AEventPropertyAccessor (Event method, Accessor accessor, string prefix):
				base (method, accessor, prefix)
			{
			}

			public override MethodBuilder Define (DeclSpace ds)
			{
				method.CheckAbstractAndExtern (block != null);
				return base.Define (ds);
			}
		}

		sealed class AddDelegateMethod: AEventPropertyAccessor
		{
			public AddDelegateMethod (Event method, Accessor accessor):
				base (method, accessor, "add_")
			{
			}

			protected override MethodInfo DelegateMethodInfo {
				get {
					return TypeManager.delegate_combine_delegate_delegate;
				}
			}
		}

		sealed class RemoveDelegateMethod: AEventPropertyAccessor
		{
			public RemoveDelegateMethod (Event method, Accessor accessor):
				base (method, accessor, "remove_")
			{
			}

			protected override MethodInfo DelegateMethodInfo {
				get {
					return TypeManager.delegate_remove_delegate_delegate;
				}
			}
		}


		static readonly string[] attribute_targets = new string [] { "event" }; // "property" target was disabled for 2.0 version

		public EventProperty (DeclSpace parent, Expression type, int mod_flags,
				      bool is_iface, MemberName name,
				      Attributes attrs, Accessor add, Accessor remove)
			: base (parent, type, mod_flags, is_iface, name, attrs)
		{
			Add = new AddDelegateMethod (this, add);
			Remove = new RemoveDelegateMethod (this, remove);
		}

		public override bool Define()
		{
			if (!base.Define ())
				return false;

			if (IsExplicitImpl)
				SetMemberIsUsed ();

			return true;
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}
	}

	/// <summary>
	/// Event is declared like field.
	/// </summary>
	public class EventField : Event {
		abstract class EventFieldAccessor : AEventAccessor
		{
			protected EventFieldAccessor (Event method, string prefix)
				: base (method, prefix)
			{
			}

			protected override void EmitMethod(DeclSpace parent)
			{
				if ((method.ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0)
					return;

				ILGenerator ig = method_data.MethodBuilder.GetILGenerator ();

				// TODO: because we cannot use generics yet
				FieldInfo field_info = ((EventField)method).FieldBuilder;

				method_data.MethodBuilder.SetImplementationFlags (MethodImplAttributes.Synchronized);
				if ((method.ModFlags & Modifiers.STATIC) != 0) {
					ig.Emit (OpCodes.Ldsfld, field_info);
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Call, DelegateMethodInfo);
					ig.Emit (OpCodes.Castclass, method.MemberType);
					ig.Emit (OpCodes.Stsfld, field_info);
				} else {
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Ldfld, field_info);
					ig.Emit (OpCodes.Ldarg_1);
					ig.Emit (OpCodes.Call, DelegateMethodInfo);
					ig.Emit (OpCodes.Castclass, method.MemberType);
					ig.Emit (OpCodes.Stfld, field_info);
				}
				ig.Emit (OpCodes.Ret);
			}
		}

		sealed class AddDelegateMethod: EventFieldAccessor
		{
			public AddDelegateMethod (Event method):
				base (method, "add_")
			{
			}

			protected override MethodInfo DelegateMethodInfo {
				get {
					return TypeManager.delegate_combine_delegate_delegate;
				}
			}
		}

		sealed class RemoveDelegateMethod: EventFieldAccessor
		{
			public RemoveDelegateMethod (Event method):
				base (method, "remove_")
			{
			}

			protected override MethodInfo DelegateMethodInfo {
				get {
					return TypeManager.delegate_remove_delegate_delegate;
				}
			}
		}


		static readonly string[] attribute_targets = new string [] { "event", "field", "method" };
		static readonly string[] attribute_targets_interface = new string[] { "event", "method" };

		public FieldBuilder FieldBuilder;
		public Expression Initializer;

		public EventField (DeclSpace parent, Expression type, int mod_flags,
				   bool is_iface, MemberName name,
				   Attributes attrs)
			: base (parent, type, mod_flags, is_iface, name, attrs)
		{
			Add = new AddDelegateMethod (this);
			Remove = new RemoveDelegateMethod (this);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Target == AttributeTargets.Field) {
				FieldBuilder.SetCustomAttribute (cb);
				return;
			}

			if (a.Target == AttributeTargets.Method) {
				int errors = Report.Errors;
				Add.ApplyAttributeBuilder (a, cb);
				if (errors == Report.Errors)
					Remove.ApplyAttributeBuilder (a, cb);
				return;
			}

			base.ApplyAttributeBuilder (a, cb);
		}

		public override bool Define()
		{
			if (!base.Define ())
				return false;

			if (IsInterface)
				return true;

			// FIXME: We are unable to detect whether generic event is used because
			// we are using FieldExpr instead of EventExpr for event access in that
			// case.  When this issue will be fixed this hack can be removed.
			if (TypeManager.IsGenericType (MemberType))
				SetMemberIsUsed();

			FieldBuilder = Parent.TypeBuilder.DefineField (
				Name, MemberType,
				FieldAttributes.Private | ((ModFlags & Modifiers.STATIC) != 0 ? FieldAttributes.Static : 0));
			TypeManager.RegisterEventField (EventBuilder, this);

			if (Initializer != null) {
				if (((ModFlags & Modifiers.ABSTRACT) != 0)) {
					Report.Error (74, Location, "`{0}': abstract event cannot have an initializer",
						GetSignatureForError ());
					return false;
				}

				Parent.PartialContainer.RegisterFieldForInitialization (this,
					new FieldInitializer (FieldBuilder, Initializer));
			}

			return true;
		}

		public override string[] ValidAttributeTargets 
		{
			get {
				return IsInterface ? attribute_targets_interface : attribute_targets;
			}
		}
	}

	public abstract class Event : PropertyBasedMember {
		public abstract class AEventAccessor : AbstractPropertyEventMethod
		{
			protected readonly Event method;
			ImplicitParameter param_attr;

			static readonly string[] attribute_targets = new string [] { "method", "param", "return" };

			protected AEventAccessor (Event method, string prefix)
				: base (method, prefix)
			{
				this.method = method;
			}

			protected AEventAccessor (Event method, Accessor accessor, string prefix)
				: base (method, accessor, prefix)
			{
				this.method = method;
			}

			public override Iterator Iterator {
				get { return null; }
			}

			protected override void ApplyToExtraTarget(Attribute a, CustomAttributeBuilder cb)
			{
				if (a.Target == AttributeTargets.Parameter) {
					if (param_attr == null)
						param_attr = new ImplicitParameter (method_data.MethodBuilder);

					param_attr.ApplyAttributeBuilder (a, cb);
					return;
				}

				base.ApplyAttributeBuilder (a, cb);
			}

			public override AttributeTargets AttributeTargets {
				get {
					return AttributeTargets.Method;
				}
			}

			public override bool IsClsComplianceRequired ()
			{
				return method.IsClsComplianceRequired ();
			}

			public virtual MethodBuilder Define (DeclSpace parent)
			{
				method_data = new MethodData (method, method.ModFlags,
					method.flags | MethodAttributes.HideBySig | MethodAttributes.SpecialName, this);

				if (!method_data.Define (parent))
					return null;

				MethodBuilder mb = method_data.MethodBuilder;
				ParameterInfo.ApplyAttributes (mb);
				return mb;
			}

			protected abstract MethodInfo DelegateMethodInfo { get; }

			public override Type ReturnType {
				get {
					return TypeManager.void_type;
				}
			}

			public override EmitContext CreateEmitContext (DeclSpace ds, ILGenerator ig)
			{
				return new EmitContext (
					ds, method.Parent, Location, ig, ReturnType,
					method.ModFlags, false);
			}

			public override ObsoleteAttribute GetObsoleteAttribute ()
			{
				return method.GetObsoleteAttribute ();
			}

			public override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
				}
			}

			public override Parameters ParameterInfo {
				get {
					return method.parameters;
				}
			}
		}


		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
			Modifiers.VIRTUAL |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.UNSAFE |
			Modifiers.ABSTRACT |
			Modifiers.EXTERN;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW;

		public AEventAccessor Add, Remove;
		public MyEventBuilder     EventBuilder;
		public MethodBuilder AddBuilder, RemoveBuilder;

		Parameters parameters;

		protected Event (DeclSpace parent, Expression type, int mod_flags,
			      bool is_iface, MemberName name, Attributes attrs)
			: base (parent, null, type, mod_flags,
				is_iface ? AllowedInterfaceModifiers : AllowedModifiers, is_iface,
				name, attrs)
		{
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type.IsSubclassOf (TypeManager.security_attr_type)) {
				a.Error_InvalidSecurityParent ();
				return;
			}
			
			EventBuilder.SetCustomAttribute (cb);
		}

		public bool AreAccessorsDuplicateImplementation (MethodCore mc)
		{
			return Add.IsDuplicateImplementation (mc) || Remove.IsDuplicateImplementation (mc);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Event;
			}
		}

		public override bool Define ()
		{
			if (!DoDefineBase ())
				return false;

			if (!DoDefine ())
				return false;

			if (!TypeManager.IsDelegateType (MemberType)) {
				Report.Error (66, Location, "`{0}': event must be of a delegate type", GetSignatureForError ());
				return false;
			}

			parameters = new Parameters (
				new Parameter[] { new Parameter (MemberType, "value", Parameter.Modifier.NONE, null, Location) },
				new Type[] { MemberType } );

			if (!CheckBase ())
				return false;

			//
			// Now define the accessors
			//

			AddBuilder = Add.Define (Parent);
			if (AddBuilder == null)
				return false;

			RemoveBuilder = Remove.Define (Parent);
			if (RemoveBuilder == null)
				return false;

			EventBuilder = new MyEventBuilder (this, Parent.TypeBuilder, Name, EventAttributes.None, MemberType);						
			EventBuilder.SetAddOnMethod (AddBuilder);
			EventBuilder.SetRemoveOnMethod (RemoveBuilder);
			return true;
		}

		public override void Emit ()
		{
			if (OptAttributes != null) {
				OptAttributes.Emit ();
			}

			Add.Emit (Parent);
			Remove.Emit (Parent);

			base.Emit ();
		}

		protected override MethodInfo FindOutBaseMethod (ref Type base_ret_type)
		{
			MethodInfo mi = (MethodInfo) Parent.PartialContainer.BaseCache.FindBaseEvent (
				Parent.TypeBuilder, Name);

			if (mi == null)
				return null;

			ParameterData pd = TypeManager.GetParameterData (mi);
			base_ret_type = pd.ParameterType (0);
			return mi;
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "E:"; }
		}
	}

 
	public class Indexer : PropertyBase {

		class GetIndexerMethod : GetMethod
		{
			public GetIndexerMethod (PropertyBase method):
				base (method)
			{
			}

			public GetIndexerMethod (PropertyBase method, Accessor accessor):
				base (method, accessor)
			{
			}

			public override Parameters ParameterInfo {
				get {
					return ((Indexer)method).parameters;
				}
			}
		}

		class SetIndexerMethod: SetMethod
		{
			public SetIndexerMethod (PropertyBase method):
				base (method)
			{
			}

			public SetIndexerMethod (PropertyBase method, Accessor accessor):
				base (method, accessor)
			{
			}

			protected override void DefineParameters ()
			{
				parameters = Parameters.MergeGenerated (((Indexer)method).parameters,
					new Parameter (method.MemberType, "value", Parameter.Modifier.NONE, null, method.Location));
			}
		}

		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.VIRTUAL |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.UNSAFE |
			Modifiers.EXTERN |
			Modifiers.ABSTRACT;

		const int AllowedInterfaceModifiers =
			Modifiers.NEW;

		public readonly Parameters parameters;

		public Indexer (DeclSpace parent, Expression type, MemberName name, int mod,
				bool is_iface, Parameters parameters, Attributes attrs,
				Accessor get_block, Accessor set_block, bool define_set_first)
			: base (parent, type, mod,
				is_iface ? AllowedInterfaceModifiers : AllowedModifiers,
				is_iface, name, attrs, define_set_first)
		{
			this.parameters = parameters;

			if (get_block == null)
				Get = new GetIndexerMethod (this);
			else
				Get = new GetIndexerMethod (this, get_block);

			if (set_block == null)
				Set = new SetIndexerMethod (this);
			else
				Set = new SetIndexerMethod (this, set_block);
		}
		
		protected override bool CheckForDuplications ()
		{
			ArrayList ar = Parent.PartialContainer.Indexers;
			if (ar != null) {
				int arLen = ar.Count;
					
				for (int i = 0; i < arLen; i++) {
					Indexer m = (Indexer) ar [i];
					if (IsDuplicateImplementation (m))
						return false;
				}
			}

			return true;
		}

		bool IsDuplicateImplementation (Indexer indexer)
		{
			if (this == indexer)
				return false;

			if (!MemberName.Equals (indexer.MemberName))
				return false;

			Type[] param_types = indexer.parameters.Types;

			// When it is not yet defined
			if (param_types == null)
				return false;

			if (param_types.Length != parameters.Count)
				return false;

			for (int i = 0; i < param_types.Length; i++)
				if (param_types [i] != parameters.Types [i])
					return false;

			Report.SymbolRelatedToPreviousError (indexer);
			Report.Error (111, Location, TypeContainer.Error111, indexer.GetSignatureForError ());
			return true;
		}


		public override bool Define ()
		{
			if (!DoDefineBase ())
				return false;

			if (!base.Define ())
				return false;

			if (!parameters.Resolve (ds))
				return false;

			if (!CheckParameters (parameters))
				return false;

			if (MemberType == TypeManager.void_type) {
				Report.Error (620, Location, "Indexers cannot have void type");
				return false;
			}

			if (OptAttributes != null) {
				Attribute indexer_attr = OptAttributes.Search (TypeManager.indexer_name_type);
				if (indexer_attr != null) {
					// Remove the attribute from the list because it is not emitted
					OptAttributes.Attrs.Remove (indexer_attr);

					string name = indexer_attr.GetIndexerAttributeValue ();
					if (name == null)
						return false;

					ShortName = name;

					if (IsExplicitImpl) {
						Report.Error (415, indexer_attr.Location,
							      "The `IndexerName' attribute is valid only on an " +
							      "indexer that is not an explicit interface member declaration");
						return false;
					}

					if ((ModFlags & Modifiers.OVERRIDE) != 0) {
						Report.Error (609, indexer_attr.Location,
							      "Cannot set the `IndexerName' attribute on an indexer marked override");
						return false;
					}
				}
			}

			if (InterfaceType != null) {
				string base_IndexerName = TypeManager.IndexerPropertyName (InterfaceType);
				if (base_IndexerName != Name)
					ShortName = base_IndexerName;
			}

			if (!Parent.PartialContainer.AddMember (this) ||
				!Parent.PartialContainer.AddMember (Get) || !Parent.PartialContainer.AddMember (Set))
				return false;

			if (!CheckBase ())
				return false;

			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;
			
			if (!DefineAccessors ())
				return false;

			if (!Get.IsDummy) {
				// Setup iterator if we are one
				if ((ModFlags & Modifiers.METHOD_YIELDS) != 0){
					Iterator iterator = Iterator.CreateIterator (Get, Parent, null, ModFlags);
					if (iterator == null)
						return false;
				}
			}

			//
			// Now name the parameters
			//
			Parameter [] p = parameters.FixedParameters;
			if (p != null) {
				// TODO: should be done in parser and it needs to do cycle
				if ((p [0].ModFlags & Parameter.Modifier.ISBYREF) != 0) {
					CSharpParser.Error_ParameterModifierNotValid (Location);
					return false;
				}
			}

			PropertyBuilder = Parent.TypeBuilder.DefineProperty (
				Name, PropertyAttributes.None, MemberType, parameters.Types);
			
			if (!Get.IsDummy)
				PropertyBuilder.SetGetMethod (GetBuilder);

			if (!Set.IsDummy)
				PropertyBuilder.SetSetMethod (SetBuilder);
				
			TypeManager.RegisterIndexer (PropertyBuilder, GetBuilder, SetBuilder, parameters.Types);

			return true;
		}

		public override string GetDocCommentName (DeclSpace ds)
		{
			return DocUtil.GetMethodDocCommentName (this, parameters, ds);
		}

		public override string GetSignatureForError ()
		{
			StringBuilder sb = new StringBuilder (Parent.GetSignatureForError ());
			if (MemberName.Left != null) {
				sb.Append ('.');
				sb.Append (MemberName.Left);
			}

			sb.Append (".this");
			sb.Append (parameters.GetSignatureForError ().Replace ('(', '[').Replace (')', ']'));
			return sb.ToString ();
		}

		public override bool MarkForDuplicationCheck ()
		{
			caching_flags |= Flags.TestMethodDuplication;
			return true;
		}

		protected override PropertyInfo ResolveBaseProperty ()
		{
			return Parent.PartialContainer.BaseCache.FindMemberToOverride (
				Parent.TypeBuilder, Name, parameters.Types, null, true) as PropertyInfo;
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			parameters.VerifyClsCompliance ();
			return true;
		}
	}

	public class Operator : MethodOrOperator, IAnonymousHost {

		const int AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.UNSAFE |
			Modifiers.EXTERN |
			Modifiers.STATIC;

		public enum OpType : byte {

			// Unary operators
			LogicalNot,
			OnesComplement,
			Increment,
			Decrement,
			True,
			False,

			// Unary and Binary operators
			Addition,
			Subtraction,

			UnaryPlus,
			UnaryNegation,
			
			// Binary operators
			Multiply,
			Division,
			Modulus,
			BitwiseAnd,
			BitwiseOr,
			ExclusiveOr,
			LeftShift,
			RightShift,
			Equality,
			Inequality,
			GreaterThan,
			LessThan,
			GreaterThanOrEqual,
			LessThanOrEqual,

			// Implicit and Explicit
			Implicit,
			Explicit,

			// Just because of enum
			TOP
		};

		public readonly OpType OperatorType;
		
		public Operator (DeclSpace parent, OpType type, Expression ret_type,
				 int mod_flags, Parameters parameters,
				 ToplevelBlock block, Attributes attrs, Location loc)
			: base (parent, null, ret_type, mod_flags, AllowedModifiers, false,
				new MemberName ("op_" + type.ToString(), loc), attrs, parameters)
		{
			OperatorType = type;
			Block = block;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb) 
		{
			if (a.Type == TypeManager.conditional_attribute_type) {
				Error_ConditionalAttributeIsNotValid ();
				return;
			}

			base.ApplyAttributeBuilder (a, cb);
		}
		
		protected override bool CheckForDuplications ()
		{
			ArrayList ar = Parent.PartialContainer.Operators;
			if (ar != null) {
				int arLen = ar.Count;
   					
				for (int i = 0; i < arLen; i++) {
					Operator o = (Operator) ar [i];
					if (IsDuplicateImplementation (o))
						return false;
				}
			}

			ar = Parent.PartialContainer.Methods;
			if (ar != null) {
				int arLen = ar.Count;
   					
				for (int i = 0; i < arLen; i++) {
					Method m = (Method) ar [i];
					if (IsDuplicateImplementation (m))
						return false;
				}
			}

			return true;
		}

		public override bool Define ()
		{
			const int RequiredModifiers = Modifiers.PUBLIC | Modifiers.STATIC;
			if ((ModFlags & RequiredModifiers) != RequiredModifiers){
				Report.Error (558, Location, "User-defined operator `{0}' must be declared static and public", GetSignatureForError ());
				return false;
			}

			// imlicit and explicit operator of same types are not allowed
			if (OperatorType == OpType.Explicit || OperatorType == OpType.Implicit)
				MarkForDuplicationCheck ();

			if (!base.Define ())
				return false;

			if (MemberType == TypeManager.void_type) {
				Report.Error (590, Location, "User-defined operators cannot return void");
				return false;
			}

			Type declaring_type = MethodData.DeclaringType;
			Type return_type = MemberType;
			Type first_arg_type = ParameterTypes [0];

			// Rules for conversion operators
			
			if (OperatorType == OpType.Implicit || OperatorType == OpType.Explicit) {
				if (first_arg_type == return_type && first_arg_type == declaring_type){
					Report.Error (555, Location,
						"User-defined operator cannot take an object of the enclosing type and convert to an object of the enclosing type");
					return false;
				}
				
				if ((first_arg_type != declaring_type) && (return_type != declaring_type) &&
				    !TypeManager.IsNullableTypeOf (first_arg_type, declaring_type) &&
				    !TypeManager.IsNullableTypeOf (return_type, declaring_type)) {
					Report.Error (
						556, Location, 
						"User-defined conversion must convert to or from the " +
						"enclosing type");
					return false;
				}

				if (first_arg_type.IsInterface || return_type.IsInterface){
					Report.Error (552, Location, "User-defined conversion `{0}' cannot convert to or from an interface type",
						GetSignatureForError ());
					return false;
				}
				
				if (first_arg_type.IsSubclassOf (return_type) || return_type.IsSubclassOf (first_arg_type)) {
					if (declaring_type.IsSubclassOf (return_type) || declaring_type.IsSubclassOf (first_arg_type)) {
						Report.Error (553, Location, "User-defined conversion `{0}' cannot convert to or from base class",
							GetSignatureForError ());
						return false;
					}
					Report.Error (554, Location, "User-defined conversion `{0}' cannot convert to or from derived class",
						GetSignatureForError ());
					return false;
				}
			} else if (OperatorType == OpType.LeftShift || OperatorType == OpType.RightShift) {
				if (first_arg_type != declaring_type || ParameterTypes [1] != TypeManager.int32_type) {
					Report.Error (564, Location, "Overloaded shift operator must have the type of the first operand be the containing type, and the type of the second operand must be int");
					return false;
				}
			} else if (Parameters.Count == 1) {
				// Checks for Unary operators

				if (OperatorType == OpType.Increment || OperatorType == OpType.Decrement) {
					if (return_type != declaring_type && !TypeManager.IsSubclassOf (return_type, declaring_type)) {
						Report.Error (448, Location,
							"The return type for ++ or -- operator must be the containing type or derived from the containing type");
						return false;
					}
					if (first_arg_type != declaring_type) {
						Report.Error (
							559, Location, "The parameter type for ++ or -- operator must be the containing type");
						return false;
					}
				}
				
				if (first_arg_type != declaring_type){
					Report.Error (
						562, Location,
						"The parameter of a unary operator must be the " +
						"containing type");
					return false;
				}
				
				if (OperatorType == OpType.True || OperatorType == OpType.False) {
					if (return_type != TypeManager.bool_type){
						Report.Error (
							215, Location,
							"The return type of operator True or False " +
							"must be bool");
						return false;
					}
				}
				
			} else {
				// Checks for Binary operators
				
				if (first_arg_type != declaring_type &&
				    ParameterTypes [1] != declaring_type){
					Report.Error (
						563, Location,
						"One of the parameters of a binary operator must " +
						"be the containing type");
					return false;
				}
			}

			return true;
		}

		protected override bool DoDefine ()
		{
			if (!base.DoDefine ())
				return false;

			flags |= MethodAttributes.SpecialName | MethodAttributes.HideBySig;
			return true;
		}
		
		public override void Emit ()
		{
			base.Emit ();

			Parameters.ApplyAttributes (MethodBuilder);

			//
			// abstract or extern methods have no bodies
			//
			if ((ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) != 0)
				return;
			
			EmitContext ec;
			if ((flags & MethodAttributes.PinvokeImpl) == 0)
				ec = CreateEmitContext (Parent, MethodBuilder.GetILGenerator ());
			else
				ec = CreateEmitContext (Parent, null);
			
			SourceMethod source = SourceMethod.Create (Parent, MethodBuilder, Block);
			ec.EmitTopBlock (this, Block);

			if (source != null)
				source.CloseMethod ();

			Block = null;
		}

		// Operator cannot be override
		protected override MethodInfo FindOutBaseMethod (ref Type base_ret_type)
		{
			return null;
		}

		public static string GetName (OpType ot)
		{
			switch (ot){
			case OpType.LogicalNot:
				return "!";
			case OpType.OnesComplement:
				return "~";
			case OpType.Increment:
				return "++";
			case OpType.Decrement:
				return "--";
			case OpType.True:
				return "true";
			case OpType.False:
				return "false";
			case OpType.Addition:
				return "+";
			case OpType.Subtraction:
				return "-";
			case OpType.UnaryPlus:
				return "+";
			case OpType.UnaryNegation:
				return "-";
			case OpType.Multiply:
				return "*";
			case OpType.Division:
				return "/";
			case OpType.Modulus:
				return "%";
			case OpType.BitwiseAnd:
				return "&";
			case OpType.BitwiseOr:
				return "|";
			case OpType.ExclusiveOr:
				return "^";
			case OpType.LeftShift:
				return "<<";
			case OpType.RightShift:
				return ">>";
			case OpType.Equality:
				return "==";
			case OpType.Inequality:
				return "!=";
			case OpType.GreaterThan:
				return ">";
			case OpType.LessThan:
				return "<";
			case OpType.GreaterThanOrEqual:
				return ">=";
			case OpType.LessThanOrEqual:
				return "<=";
			case OpType.Implicit:
				return "implicit";
			case OpType.Explicit:
				return "explicit";
			default: return "";
			}
		}

		public static OpType GetOperatorType (string name)
		{
			if (name.StartsWith ("op_")){
				for (int i = 0; i < Unary.oper_names.Length; ++i) {
					if (Unary.oper_names [i] == name)
						return (OpType)i;
				}

				for (int i = 0; i < Binary.oper_names.Length; ++i) {
					if (Binary.oper_names [i] == name)
						return (OpType)i;
				}
			}
			return OpType.TOP;
		}

		public override string GetSignatureForError ()
		{
			StringBuilder sb = new StringBuilder ();
			if (OperatorType == OpType.Implicit || OperatorType == OpType.Explicit) {
				sb.AppendFormat ("{0}.{1} operator {2}", Parent.GetSignatureForError (), GetName (OperatorType), Type.Type == null ? Type.ToString () : TypeManager.CSharpName (Type.Type));
			}
			else {
				sb.AppendFormat ("{0}.operator {1}", Parent.GetSignatureForError (), GetName (OperatorType));
			}

			sb.Append (Parameters.GetSignatureForError ());
			return sb.ToString ();
		}
	}

	//
	// This is used to compare method signatures
	//
	struct MethodSignature {
		public string Name;
		public Type RetType;
		public Type [] Parameters;
		
		/// <summary>
		///    This delegate is used to extract methods which have the
		///    same signature as the argument
		/// </summary>
		public static MemberFilter method_signature_filter = new MemberFilter (MemberSignatureCompare);
		
		public MethodSignature (string name, Type ret_type, Type [] parameters)
		{
			Name = name;
			RetType = ret_type;

			if (parameters == null)
				Parameters = Type.EmptyTypes;
			else
				Parameters = parameters;
		}

		public override string ToString ()
		{
			string pars = "";
			if (Parameters.Length != 0){
				System.Text.StringBuilder sb = new System.Text.StringBuilder ();
				for (int i = 0; i < Parameters.Length; i++){
					sb.Append (Parameters [i]);
					if (i+1 < Parameters.Length)
						sb.Append (", ");
				}
				pars = sb.ToString ();
			}

			return String.Format ("{0} {1} ({2})", RetType, Name, pars);
		}
		
		public override int GetHashCode ()
		{
			return Name.GetHashCode ();
		}

		public override bool Equals (Object o)
		{
			MethodSignature other = (MethodSignature) o;

			if (other.Name != Name)
				return false;

			if (other.RetType != RetType)
				return false;
			
			if (Parameters == null){
				if (other.Parameters == null)
					return true;
				return false;
			}

			if (other.Parameters == null)
				return false;
			
			int c = Parameters.Length;
			if (other.Parameters.Length != c)
				return false;

			for (int i = 0; i < c; i++)
				if (other.Parameters [i] != Parameters [i])
					return false;

			return true;
		}

		static bool MemberSignatureCompare (MemberInfo m, object filter_criteria)
		{
			MethodSignature sig = (MethodSignature) filter_criteria;

			if (m.Name != sig.Name)
				return false;

			Type ReturnType;
			MethodInfo mi = m as MethodInfo;
			PropertyInfo pi = m as PropertyInfo;

			if (mi != null)
				ReturnType = mi.ReturnType;
			else if (pi != null)
				ReturnType = pi.PropertyType;
			else
				return false;
			
			//
			// we use sig.RetType == null to mean `do not check the
			// method return value.  
			//
			if (sig.RetType != null)
				if (ReturnType != sig.RetType)
					return false;

			Type [] args;
			if (mi != null)
				args = TypeManager.GetParameterData (mi).Types;
			else
				args = TypeManager.GetArgumentTypes (pi);
			Type [] sigp = sig.Parameters;

			if (args.Length != sigp.Length)
				return false;

			for (int i = args.Length; i > 0; ){
				i--;
				if (args [i] != sigp [i])
					return false;
			}
			return true;
		}
	}
}
