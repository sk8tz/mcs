//
// generic.cs: Support classes for generics
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2003 Ximian, Inc.
//
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Collections;
using System.Text;
	
namespace Mono.CSharp {

	public abstract class GenericConstraints {
		public abstract GenericParameterAttributes Attributes {
			get;
		}

		public bool HasConstructorConstraint {
			get { return (Attributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0; }
		}

		public bool HasReferenceTypeConstraint {
			get { return (Attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0; }
		}

		public bool HasValueTypeConstraint {
			get { return (Attributes & GenericParameterAttributes.ValueTypeConstraint) != 0; }
		}

		public virtual bool HasClassConstraint {
			get { return ClassConstraint != null; }
		}

		public abstract Type ClassConstraint {
			get;
		}

		public abstract Type[] InterfaceConstraints {
			get;
		}

		public abstract Type EffectiveBaseClass {
			get;
		}

		// <summary>
		//   Returns whether the type parameter is "known to be a reference type".
		// </summary>
		public virtual bool IsReferenceType {
			get {
				if (HasReferenceTypeConstraint)
					return true;
				if (HasValueTypeConstraint)
					return false;

				if (ClassConstraint != null) {
					if (ClassConstraint.IsValueType)
						return false;

					if (ClassConstraint != TypeManager.object_type)
						return true;
				}

				foreach (Type t in InterfaceConstraints) {
					if (!t.IsGenericParameter)
						continue;

					GenericConstraints gc = TypeManager.GetTypeParameterConstraints (t);
					if ((gc != null) && gc.IsReferenceType)
						return true;
				}

				return false;
			}
		}

		// <summary>
		//   Returns whether the type parameter is "known to be a value type".
		// </summary>
		public virtual bool IsValueType {
			get {
				if (HasValueTypeConstraint)
					return true;
				if (HasReferenceTypeConstraint)
					return false;

				if (ClassConstraint != null) {
					if (!ClassConstraint.IsValueType)
						return false;

					if (ClassConstraint != TypeManager.value_type)
						return true;
				}

				foreach (Type t in InterfaceConstraints) {
					if (!t.IsGenericParameter)
						continue;

					GenericConstraints gc = TypeManager.GetTypeParameterConstraints (t);
					if ((gc != null) && gc.IsValueType)
						return true;
				}

				return false;
			}
		}
	}

	public enum SpecialConstraint
	{
		Constructor,
		ReferenceType,
		ValueType
	}

	//
	// Tracks the constraints for a type parameter
	//
	public class Constraints : GenericConstraints {
		string name;
		ArrayList constraints;
		Location loc;
		
		//
		// name is the identifier, constraints is an arraylist of
		// Expressions (with types) or `true' for the constructor constraint.
		// 
		public Constraints (string name, ArrayList constraints,
				    Location loc)
		{
			this.name = name;
			this.constraints = constraints;
			this.loc = loc;
		}

		public string TypeParameter {
			get {
				return name;
			}
		}

		GenericParameterAttributes attrs;
		TypeExpr class_constraint;
		ArrayList iface_constraints;
		ArrayList type_param_constraints;
		int num_constraints, first_constraint;
		Type class_constraint_type;
		Type[] iface_constraint_types;
		Type effective_base_type;

		public bool Resolve (EmitContext ec)
		{
			DeclSpace ds = ec.DeclSpace;

			iface_constraints = new ArrayList ();
			type_param_constraints = new ArrayList ();

			foreach (object obj in constraints) {
				if (HasConstructorConstraint) {
					Report.Error (401, loc,
						      "The new() constraint must be last.");
					return false;
				}

				if (obj is SpecialConstraint) {
					SpecialConstraint sc = (SpecialConstraint) obj;

					if (sc == SpecialConstraint.Constructor) {
						if (!HasValueTypeConstraint) {
							attrs |= GenericParameterAttributes.DefaultConstructorConstraint;
							continue;
						}

						Report.Error (
							451, loc, "The new () constraint " +
							"cannot be used with the `struct' " +
							"constraint.");
						return false;
					}

					if ((num_constraints > 0) || HasReferenceTypeConstraint || HasValueTypeConstraint) {
						Report.Error (449, loc,
							      "The `class' or `struct' " +
							      "constraint must be first");
						return false;
					}

					if (sc == SpecialConstraint.ReferenceType)
						attrs |= GenericParameterAttributes.ReferenceTypeConstraint;
					else
						attrs |= GenericParameterAttributes.ValueTypeConstraint;
					continue;
				}

				TypeExpr expr;
				if (obj is ConstructedType) {
					ConstructedType cexpr = (ConstructedType) obj;
					if (!cexpr.ResolveConstructedType (ec))
						return false;
					expr = cexpr;
				} else
					expr = ((Expression) obj).ResolveAsTypeTerminal (ec);

				if (expr == null)
					return false;

				TypeParameterExpr texpr = expr as TypeParameterExpr;
				if (texpr != null)
					type_param_constraints.Add (expr);
				else if (expr.IsInterface)
					iface_constraints.Add (expr);
				else if (class_constraint != null) {
					Report.Error (406, loc,
						      "`{0}': the class constraint for `{1}' " +
						      "must come before any other constraints.",
						      expr.Name, name);
					return false;
				} else if (HasReferenceTypeConstraint || HasValueTypeConstraint) {
					Report.Error (450, loc, "`{0}': cannot specify both " +
						      "a constraint class and the `class' " +
						      "or `struct' constraint.", expr.Name);
					return false;
				} else
					class_constraint = expr;

				num_constraints++;
			}

			return true;
		}

		bool CheckTypeParameterConstraints (TypeParameter tparam, Hashtable seen)
		{
			seen.Add (tparam, true);

			Constraints constraints = tparam.Constraints;
			if (constraints == null)
				return true;

			if (constraints.HasValueTypeConstraint) {
				Report.Error (456, loc, "Type parameter `{0}' has " +
					      "the `struct' constraint, so it cannot " +
					      "be used as a constraint for `{1}'",
					      tparam.Name, name);
				return false;
			}

			if (constraints.type_param_constraints == null)
				return true;

			foreach (TypeParameterExpr expr in constraints.type_param_constraints) {
				if (seen.Contains (expr.TypeParameter)) {
					Report.Error (454, loc, "Circular constraint " +
						      "dependency involving `{0}' and `{1}'",
						      tparam.Name, expr.Name);
					return false;
				}

				if (!CheckTypeParameterConstraints (expr.TypeParameter, seen))
					return false;
			}

			return true;
		}

		public bool ResolveTypes (EmitContext ec)
		{
			foreach (object obj in constraints) {
				ConstructedType cexpr = obj as ConstructedType;
				if (cexpr == null)
					continue;

				if (!cexpr.CheckConstraints (ec))
					return false;
			}

			foreach (TypeParameterExpr expr in type_param_constraints) {
				Hashtable seen = new Hashtable ();
				if (!CheckTypeParameterConstraints (expr.TypeParameter, seen))
					return false;
			}

			ArrayList list = new ArrayList ();

			foreach (TypeExpr iface_constraint in iface_constraints) {
				foreach (Type type in list) {
					if (!type.Equals (iface_constraint.Type))
						continue;

					Report.Error (405, loc,
						      "Duplicate constraint `{0}' for type " +
						      "parameter `{1}'.", iface_constraint.Type,
						      name);
					return false;
				}

				list.Add (iface_constraint.Type);
			}

			foreach (TypeParameterExpr expr in type_param_constraints) {
				foreach (Type type in list) {
					if (!type.Equals (expr.Type))
						continue;

					Report.Error (405, loc,
						      "Duplicate constraint `{0}' for type " +
						      "parameter `{1}'.", expr.Type, name);
					return false;
				}

				list.Add (expr.Type);
			}

			iface_constraint_types = new Type [list.Count];
			list.CopyTo (iface_constraint_types, 0);

			if (class_constraint != null) {
				class_constraint_type = class_constraint.Type;
				if (class_constraint_type == null)
					return false;

				if (class_constraint_type.IsSealed) {
					Report.Error (701, loc,
						      "`{0}' is not a valid bound.  Bounds " +
						      "must be interfaces or non sealed " +
						      "classes", class_constraint_type);
					return false;
				}

				if ((class_constraint_type == TypeManager.array_type) ||
				    (class_constraint_type == TypeManager.delegate_type) ||
				    (class_constraint_type == TypeManager.enum_type) ||
				    (class_constraint_type == TypeManager.value_type) ||
				    (class_constraint_type == TypeManager.object_type)) {
					Report.Error (702, loc,
						      "Bound cannot be special class `{0}'",
						      class_constraint_type);
					return false;
				}
			}

			if (class_constraint_type != null)
				effective_base_type = class_constraint_type;
			else if (HasValueTypeConstraint)
				effective_base_type = TypeManager.value_type;
			else
				effective_base_type = TypeManager.object_type;

			return true;
		}

		public bool CheckDependencies (EmitContext ec)
		{
			foreach (TypeParameterExpr expr in type_param_constraints) {
				if (!CheckDependencies (expr.TypeParameter, ec))
					return false;
			}

			return true;
		}

		bool CheckDependencies (TypeParameter tparam, EmitContext ec)
		{
			Constraints constraints = tparam.Constraints;
			if (constraints == null)
				return true;

			if (HasValueTypeConstraint && constraints.HasClassConstraint) {
				Report.Error (455, loc, "Type parameter `{0}' inherits " +
					      "conflicting constraints `{1}' and `{2}'",
					      name, constraints.ClassConstraint,
					      "System.ValueType");
				return false;
			}

			if (HasClassConstraint && constraints.HasClassConstraint) {
				Type t1 = ClassConstraint;
				TypeExpr e1 = class_constraint;
				Type t2 = constraints.ClassConstraint;
				TypeExpr e2 = constraints.class_constraint;

				if (!Convert.ImplicitReferenceConversionExists (ec, e1, t2) &&
				    !Convert.ImplicitReferenceConversionExists (ec, e2, t1)) {
					Report.Error (455, loc,
						      "Type parameter `{0}' inherits " +
						      "conflicting constraints `{1}' and `{2}'",
						      name, t1, t2);
					return false;
				}
			}

			if (constraints.type_param_constraints == null)
				return true;

			foreach (TypeParameterExpr expr in constraints.type_param_constraints) {
				if (!CheckDependencies (expr.TypeParameter, ec))
					return false;
			}

			return true;
		}

		public void Define (GenericTypeParameterBuilder type)
		{
			type.SetGenericParameterAttributes (attrs);
		}

		public override GenericParameterAttributes Attributes {
			get { return attrs; }
		}

		public override bool HasClassConstraint {
			get { return class_constraint != null; }
		}

		public override Type ClassConstraint {
			get { return class_constraint_type; }
		}

		public override Type[] InterfaceConstraints {
			get { return iface_constraint_types; }
		}

		public override Type EffectiveBaseClass {
			get { return effective_base_type; }
		}

		internal bool IsSubclassOf (Type t)
		{
			if ((class_constraint_type != null) &&
			    class_constraint_type.IsSubclassOf (t))
				return true;

			if (iface_constraint_types == null)
				return false;

			foreach (Type iface in iface_constraint_types) {
				if (TypeManager.IsSubclassOf (iface, t))
					return true;
			}

			return false;
		}

		public bool CheckInterfaceMethod (EmitContext ec, GenericConstraints gc)
		{
			if (!ResolveTypes (ec))
				return false;

			if (gc.Attributes != attrs)
				return false;

			if (HasClassConstraint != gc.HasClassConstraint)
				return false;
			if (HasClassConstraint && !TypeManager.IsEqual (gc.ClassConstraint, ClassConstraint))
				return false;

			int gc_icount = gc.InterfaceConstraints != null ?
				gc.InterfaceConstraints.Length : 0;
			int icount = InterfaceConstraints != null ?
				InterfaceConstraints.Length : 0;

			if (gc_icount != icount)
				return false;

			foreach (Type iface in gc.InterfaceConstraints) {
				bool ok = false;
				foreach (Type check in InterfaceConstraints) {
					if (TypeManager.IsEqual (iface, check)) {
						ok = true;
						break;
					}
				}

				if (!ok)
					return false;
			}

			return true;
		}
	}

	//
	// This type represents a generic type parameter
	//
	public class TypeParameter : MemberCore, IMemberContainer {
		string name;
		GenericConstraints gc;
		Constraints constraints;
		Location loc;
		GenericTypeParameterBuilder type;

		public TypeParameter (TypeContainer parent, string name,
				      Constraints constraints, Location loc)
			: base (parent, new MemberName (name), null, loc)
		{
			this.name = name;
			this.constraints = constraints;
			this.loc = loc;
		}

		public GenericConstraints GenericConstraints {
			get {
				return gc != null ? gc : constraints;
			}
		}

		public Constraints Constraints {
			get {
				return constraints;
			}
		}

		public bool HasConstructorConstraint {
			get {
				if (constraints != null)
					return constraints.HasConstructorConstraint;

				return false;
			}
		}

		public Type Type {
			get {
				return type;
			}
		}

		public bool Resolve (DeclSpace ds)
		{
			if (constraints != null)
				return constraints.Resolve (ds.EmitContext);

			return true;
		}

		public void Define (GenericTypeParameterBuilder type)
		{
			if (this.type != null)
				throw new InvalidOperationException ();

			this.type = type;
			TypeManager.AddTypeParameter (type, this);
		}

		public void DefineConstraints ()
		{
			if (constraints != null)
				constraints.Define (type);
		}

		public bool DefineType (EmitContext ec)
		{
			return DefineType (ec, null, null, false);
		}

		public bool DefineType (EmitContext ec, MethodBuilder builder,
					MethodInfo implementing, bool is_override)
		{
			if (implementing != null) {
				if (is_override && (constraints != null)) {
					Report.Error (
						460, loc, "Constraints for override and " +
						"explicit interface implementation methods " +
						"are inherited from the base method so they " +
						"cannot be specified directly");
					return false;
				}

				MethodBase mb = implementing;
				if (mb.Mono_IsInflatedMethod)
					mb = mb.GetGenericMethodDefinition ();

				int pos = type.GenericParameterPosition;
				ParameterData pd = Invocation.GetParameterData (mb);
				GenericConstraints temp_gc = pd.GenericConstraints (pos);
				Type mparam = mb.GetGenericArguments () [pos];

				if (temp_gc != null)
					gc = new InflatedConstraints (temp_gc, implementing.DeclaringType);
				else if (constraints != null)
					gc = new InflatedConstraints (constraints, implementing.DeclaringType);

				bool ok = true;
				if (constraints != null) {
					if (temp_gc == null)
						ok = false;
					else if (!constraints.CheckInterfaceMethod (ec, gc))
						ok = false;
				} else {
					if (!is_override && (temp_gc != null))
						ok = false;
				}

				if (!ok) {
					Report.SymbolRelatedToPreviousError (implementing);

					Report.Error (
						425, loc, "The constraints for type " +
						"parameter `{0}' of method `{1}' must match " +
						"the constraints for type parameter `{2}' " +
						"of interface method `{3}'.  Consider using " +
						"an explicit interface implementation instead",
						Name, TypeManager.CSharpSignature (builder),
						mparam, TypeManager.CSharpSignature (mb));
					return false;
				}
			} else {
				if (constraints != null) {
					if (!constraints.ResolveTypes (ec))
						return false;
				}

				gc = (GenericConstraints) constraints;
			}

			if (gc == null)
				return true;

			if (gc.HasClassConstraint)
				type.SetBaseTypeConstraint (gc.ClassConstraint);

			type.SetInterfaceConstraints (gc.InterfaceConstraints);
			TypeManager.RegisterBuilder (type, gc.InterfaceConstraints);

			return true;
		}

		public bool CheckDependencies (EmitContext ec)
		{
			if (constraints != null)
				return constraints.CheckDependencies (ec);

			return true;
		}

		//
		// MemberContainer
		//

		public override bool Define ()
		{
			return true;
		}

		protected override void VerifyObsoleteAttribute ()
		{ }

		public override void ApplyAttributeBuilder (Attribute a,
							    CustomAttributeBuilder cb)
		{ }

		public override AttributeTargets AttributeTargets {
			get {
				return (AttributeTargets) 0;
			}
		}

		public override string[] ValidAttributeTargets {
			get {
				return new string [0];
			}
		}

		//
		// IMemberContainer
		//

		string IMemberContainer.Name {
			get { return Name; }
		}

		MemberCache IMemberContainer.ParentCache {
			get { return null; }
		}

		bool IMemberContainer.IsInterface {
			get { return true; }
		}

		MemberList IMemberContainer.GetMembers (MemberTypes mt, BindingFlags bf)
		{
			return FindMembers (mt, bf, null, null);
		}

		MemberCache IMemberContainer.MemberCache {
			get { return null; }
		}

		public MemberList FindMembers (MemberTypes mt, BindingFlags bf,
					       MemberFilter filter, object criteria)
		{
			if (constraints == null)
				return MemberList.Empty;

			ArrayList members = new ArrayList ();

			GenericConstraints gc = (GenericConstraints) constraints;

			if (gc.HasClassConstraint) {
				MemberList list = TypeManager.FindMembers (
					gc.ClassConstraint, mt, bf, filter, criteria);

				members.AddRange (list);
			}

			foreach (Type t in gc.InterfaceConstraints) {
				MemberList list = TypeManager.FindMembers (
					t, mt, bf, filter, criteria);

				members.AddRange (list);
			}

			return new MemberList (members);
		}

		public bool IsSubclassOf (Type t)
		{
			if (type.Equals (t))
				return true;

			if (constraints != null)
				return constraints.IsSubclassOf (t);

			return false;
		}

		public override string ToString ()
		{
			return "TypeParameter[" + name + "]";
		}

		protected class InflatedConstraints : GenericConstraints
		{
			GenericConstraints gc;
			Type base_type;
			Type class_constraint;
			Type[] iface_constraints;
			Type[] dargs;
			Type declaring;

			public InflatedConstraints (GenericConstraints gc, Type declaring)
			{
				this.gc = gc;
				this.declaring = declaring;

				dargs = TypeManager.GetTypeArguments (declaring);

				ArrayList list = new ArrayList ();
				if (gc.HasClassConstraint)
					list.Add (inflate (gc.ClassConstraint));
				foreach (Type iface in gc.InterfaceConstraints)
					list.Add (inflate (iface));

				bool has_class_constr = false;
				if (list.Count > 0) {
					Type first = (Type) list [0];
					has_class_constr = !first.IsInterface && !first.IsGenericParameter;
				}

				if ((list.Count > 0) && has_class_constr) {
					class_constraint = (Type) list [0];
					iface_constraints = new Type [list.Count - 1];
					list.CopyTo (1, iface_constraints, 0, list.Count - 1);
				} else {
					iface_constraints = new Type [list.Count];
					list.CopyTo (iface_constraints, 0);
				}

				if (HasValueTypeConstraint)
					base_type = TypeManager.value_type;
				else if (class_constraint != null)
					base_type = class_constraint;
				else
					base_type = TypeManager.object_type;
			}

			Type inflate (Type t)
			{
				if (t == null)
					return null;
				if (t.IsGenericParameter)
					return dargs [t.GenericParameterPosition];
				if (t.IsGenericInstance) {
					t = t.GetGenericTypeDefinition ();
					t = t.BindGenericParameters (dargs);
				}

				return t;
			}

			public override GenericParameterAttributes Attributes {
				get { return gc.Attributes; }
			}

			public override Type ClassConstraint {
				get { return class_constraint; }
			}

			public override Type EffectiveBaseClass {
				get { return base_type; }
			}

			public override Type[] InterfaceConstraints {
				get { return iface_constraints; }
			}
		}
	}

	//
	// This type represents a generic type parameter reference.
	//
	// These expressions are born in a fully resolved state.
	//
	public class TypeParameterExpr : TypeExpr {
		TypeParameter type_parameter;

		public override string Name {
			get {
				return type_parameter.Name;
			}
		}

		public TypeParameter TypeParameter {
			get {
				return type_parameter;
			}
		}
		
		public TypeParameterExpr (TypeParameter type_parameter, Location loc)
		{
			this.type_parameter = type_parameter;
			this.loc = loc;
		}

		public override TypeExpr DoResolveAsTypeStep (EmitContext ec)
		{
			type = type_parameter.Type;

			return this;
		}

		public override bool IsInterface {
			get { return false; }
		}

		public override bool CheckAccessLevel (DeclSpace ds)
		{
			return true;
		}

		public void Error_CannotUseAsUnmanagedType (Location loc)
		{
			Report.Error (-203, loc, "Can not use type parameter as unamanged type");
		}
	}

	public class TypeArguments {
		public readonly Location Location;
		ArrayList args;
		Type[] atypes;
		int dimension;
		bool has_type_args;
		bool created;
		
		public TypeArguments (Location loc)
		{
			args = new ArrayList ();
			this.Location = loc;
		}

		public TypeArguments (int dimension, Location loc)
		{
			this.dimension = dimension;
			this.Location = loc;
		}

		public void Add (Expression type)
		{
			if (created)
				throw new InvalidOperationException ();

			args.Add (type);
		}

		public void Add (TypeArguments new_args)
		{
			if (created)
				throw new InvalidOperationException ();

			args.AddRange (new_args.args);
		}

		public string[] GetDeclarations ()
		{
			string[] ret = new string [args.Count];
			for (int i = 0; i < args.Count; i++) {
				SimpleName sn = args [i] as SimpleName;
				if (sn != null) {
					ret [i] = sn.Name;
					continue;
				}

				Report.Error (81, Location, "Type parameter declaration " +
					      "must be an identifier not a type");
				return null;
			}
			return ret;
		}

		public Type[] Arguments {
			get {
				return atypes;
			}
		}

		public bool HasTypeArguments {
			get {
				return has_type_args;
			}
		}

		public int Count {
			get {
				if (dimension > 0)
					return dimension;
				else
					return args.Count;
			}
		}

		public bool IsUnbound {
			get {
				return dimension > 0;
			}
		}

		public override string ToString ()
		{
			StringBuilder s = new StringBuilder ();

			int count = Count;
			for (int i = 0; i < count; i++){
				//
				// FIXME: Use TypeManager.CSharpname once we have the type
				//
				if (args != null)
					s.Append (args [i].ToString ());
				if (i+1 < count)
					s.Append (",");
			}
			return s.ToString ();
		}

		public bool Resolve (EmitContext ec)
		{
			DeclSpace ds = ec.DeclSpace;
			int count = args.Count;
			bool ok = true;

			atypes = new Type [count];

			for (int i = 0; i < count; i++){
				TypeExpr te = ((Expression) args [i]).ResolveAsTypeTerminal (ec);
				if (te == null) {
					ok = false;
					continue;
				}
				if (te is TypeParameterExpr)
					has_type_args = true;

				atypes [i] = te.Type;
			}
			return ok;
		}
	}
	
	public class ConstructedType : TypeExpr {
		string name, full_name;
		TypeArguments args;
		Type[] gen_params, atypes;
		Type gt;
		
		public ConstructedType (string name, TypeArguments args, Location l)
		{
			loc = l;
			this.name = MemberName.MakeName (name, args.Count);
			this.args = args;

			eclass = ExprClass.Type;
			full_name = name + "<" + args.ToString () + ">";
		}

		public ConstructedType (string name, TypeParameter[] type_params, Location l)
			: this (type_params, l)
		{
			loc = l;

			this.name = name;
			full_name = name + "<" + args.ToString () + ">";
		}

		protected ConstructedType (TypeArguments args, Location l)
		{
			loc = l;
			this.args = args;

			eclass = ExprClass.Type;
		}

		protected ConstructedType (TypeParameter[] type_params, Location l)
		{
			loc = l;

			args = new TypeArguments (l);
			foreach (TypeParameter type_param in type_params)
				args.Add (new TypeParameterExpr (type_param, l));

			eclass = ExprClass.Type;
		}

		public ConstructedType (Type t, TypeParameter[] type_params, Location l)
			: this (type_params, l)
		{
			gt = t.GetGenericTypeDefinition ();

			this.name = gt.FullName;
			full_name = gt.FullName + "<" + args.ToString () + ">";
		}

		public ConstructedType (Type t, TypeArguments args, Location l)
			: this (args, l)
		{
			gt = t.GetGenericTypeDefinition ();

			this.name = gt.FullName;
			full_name = gt.FullName + "<" + args.ToString () + ">";
		}

		public TypeArguments TypeArguments {
			get { return args; }
		}

		protected string DeclarationName {
			get {
				StringBuilder sb = new StringBuilder ();
				sb.Append (gt.FullName);
				sb.Append ("<");
				for (int i = 0; i < gen_params.Length; i++) {
					if (i > 0)
						sb.Append (",");
					sb.Append (gen_params [i]);
				}
				sb.Append (">");
				return sb.ToString ();
			}
		}

		protected bool CheckConstraint (EmitContext ec, Type ptype, Expression expr,
						Type ctype)
		{
			if (TypeManager.HasGenericArguments (ctype)) {
				Type[] types = TypeManager.GetTypeArguments (ctype);

				TypeArguments new_args = new TypeArguments (loc);

				for (int i = 0; i < types.Length; i++) {
					Type t = types [i];

					if (t.IsGenericParameter) {
						int pos = t.GenericParameterPosition;
						t = args.Arguments [pos];
					}
					new_args.Add (new TypeExpression (t, loc));
				}

				TypeExpr ct = new ConstructedType (ctype, new_args, loc);
				if (ct.ResolveAsTypeTerminal (ec) == null)
					return false;
				ctype = ct.Type;
			}

			return Convert.ImplicitStandardConversionExists (ec, expr, ctype);
		}

		protected bool CheckConstraints (EmitContext ec, int index)
		{
			Type atype = atypes [index];
			Type ptype = gen_params [index];

			if (atype == ptype)
				return true;

			Expression aexpr = new EmptyExpression (atype);

			GenericConstraints gc = TypeManager.GetTypeParameterConstraints (ptype);
			if (gc == null)
				return true;

			//
			// First, check the `class' and `struct' constraints.
			//
			if (gc.HasReferenceTypeConstraint && !atype.IsClass) {
				Report.Error (452, loc, "The type `{0}' must be " +
					      "a reference type in order to use it " +
					      "as type parameter `{1}' in the " +
					      "generic type or method `{2}'.",
					      atype, ptype, DeclarationName);
				return false;
			} else if (gc.HasValueTypeConstraint && !atype.IsValueType) {
				Report.Error (453, loc, "The type `{0}' must be " +
					      "a value type in order to use it " +
					      "as type parameter `{1}' in the " +
					      "generic type or method `{2}'.",
					      atype, ptype, DeclarationName);
				return false;
			}

			//
			// The class constraint comes next.
			//
			if (gc.HasClassConstraint) {
				if (!CheckConstraint (ec, ptype, aexpr, gc.ClassConstraint)) {
					Report.Error (309, loc, "The type `{0}' must be " +
						      "convertible to `{1}' in order to " +
						      "use it as parameter `{2}' in the " +
						      "generic type or method `{3}'",
						      atype, gc.ClassConstraint, ptype, DeclarationName);
					return false;
				}
			}

			//
			// Now, check the interface constraints.
			//
			foreach (Type it in gc.InterfaceConstraints) {
				Type itype;
				if (it.IsGenericParameter)
					itype = atypes [it.GenericParameterPosition];
				else
					itype = it;

				if (!CheckConstraint (ec, ptype, aexpr, itype)) {
					Report.Error (309, loc, "The type `{0}' must be " +
						      "convertible to `{1}' in order to " +
						      "use it as parameter `{2}' in the " +
						      "generic type or method `{3}'",
						      atype, itype, ptype, DeclarationName);
					return false;
				}
			}

			//
			// Finally, check the constructor constraint.
			//

			if (!gc.HasConstructorConstraint)
				return true;

			if (TypeManager.IsBuiltinType (atype))
				return true;

			MethodGroupExpr mg = Expression.MemberLookup (
				ec, atype, ".ctor", MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance |
				BindingFlags.DeclaredOnly, loc)
				as MethodGroupExpr;

			if (atype.IsAbstract || (mg == null) || !mg.IsInstance) {
				Report.Error (310, loc, "The type `{0}' must have a public " +
					      "parameterless constructor in order to use it " +
					      "as parameter `{1}' in the generic type or " +
					      "method `{2}'", atype, ptype, DeclarationName);
				return false;
			}

			return true;
		}

		public override TypeExpr DoResolveAsTypeStep (EmitContext ec)
		{
			if (!ResolveConstructedType (ec))
				return null;

			return this;
		}

		public bool CheckConstraints (EmitContext ec)
		{
			for (int i = 0; i < gen_params.Length; i++) {
				if (!CheckConstraints (ec, i))
					return false;
			}

			return true;
		}

		public override TypeExpr ResolveAsTypeTerminal (EmitContext ec)
		{
			if (base.ResolveAsTypeTerminal (ec) == null)
				return null;

			if (!CheckConstraints (ec))
				return null;

			return this;
		}

		public bool ResolveConstructedType (EmitContext ec)
		{
			if (type != null)
				return true;
			if (gt != null)
				return DoResolveType (ec);

			//
			// First, resolve the generic type.
			//
			DeclSpace ds;
			Type nested = ec.DeclSpace.FindNestedType (loc, name, out ds);
			if (nested != null) {
				gt = nested.GetGenericTypeDefinition ();

				TypeArguments new_args = new TypeArguments (loc);
				if (ds.IsGeneric) {
					foreach (TypeParameter param in ds.TypeParameters)
						new_args.Add (new TypeParameterExpr (param, loc));
				}
				new_args.Add (args);

				args = new_args;
				return DoResolveType (ec);
			}

			Type t;
			int num_args;

			SimpleName sn = new SimpleName (name, loc);
			TypeExpr resolved = sn.ResolveAsTypeTerminal (ec);
			if (resolved == null)
				return false;

			t = resolved.Type;
			if (t == null) {
				Report.Error (246, loc, "Cannot find type `{0}'<...>",
					      Basename);
				return false;
			}

			num_args = TypeManager.GetNumberOfTypeArguments (t);
			if (num_args == 0) {
				Report.Error (308, loc,
					      "The non-generic type `{0}' cannot " +
					      "be used with type arguments.",
					      TypeManager.CSharpName (t));
				return false;
			}

			gt = t.GetGenericTypeDefinition ();
			return DoResolveType (ec);
		}

		bool DoResolveType (EmitContext ec)
		{
			//
			// Resolve the arguments.
			//
			if (args.Resolve (ec) == false)
				return false;

			gen_params = gt.GetGenericArguments ();
			atypes = args.Arguments;

			if (atypes.Length != gen_params.Length) {
				Report.Error (305, loc,
					      "Using the generic type `{0}' " +
					      "requires {1} type arguments",
					      TypeManager.GetFullName (gt),
					      gen_params.Length);
				return false;
			}

			//
			// Now bind the parameters.
			//
			type = gt.BindGenericParameters (atypes);
			return true;
		}

		public Expression GetSimpleName (EmitContext ec)
		{
			return new SimpleName (Basename, args, loc);
		}

		public override bool CheckAccessLevel (DeclSpace ds)
		{
			return ds.CheckAccessLevel (gt);
		}

		public override bool AsAccessible (DeclSpace ds, int flags)
		{
			return ds.AsAccessible (gt, flags);
		}

		public override bool IsClass {
			get { return gt.IsClass; }
		}

		public override bool IsValueType {
			get { return gt.IsValueType; }
		}

		public override bool IsInterface {
			get { return gt.IsInterface; }
		}

		public override bool IsSealed {
			get { return gt.IsSealed; }
		}

		public override bool IsAttribute {
			get { return false; }
		}

		public override bool Equals (object obj)
		{
			ConstructedType cobj = obj as ConstructedType;
			if (cobj == null)
				return false;

			if ((type == null) || (cobj.type == null))
				return false;

			return type == cobj.type;
		}

		public string Basename {
			get {
				int pos = name.LastIndexOf ('`');
				if (pos >= 0)
					return name.Substring (0, pos);
				else
					return name;
			}
		}

		public override string Name {
			get {
				return full_name;
			}
		}
	}

	public class GenericMethod : DeclSpace
	{
		public GenericMethod (NamespaceEntry ns, TypeContainer parent,
				      MemberName name, Location l)
			: base (ns, parent, name, null, l)
		{ }

		public override TypeBuilder DefineType ()
		{
			throw new Exception ();
		}

		public override bool Define ()
		{
			for (int i = 0; i < TypeParameters.Length; i++)
				if (!TypeParameters [i].Resolve (Parent))
					return false;

			return true;
		}

		public bool Define (MethodBuilder mb, Type return_type)
		{
			if (!Define ())
				return false;

			GenericTypeParameterBuilder[] gen_params;
			string[] names = MemberName.TypeArguments.GetDeclarations ();
			gen_params = mb.DefineGenericParameters (names);
			for (int i = 0; i < TypeParameters.Length; i++)
				TypeParameters [i].Define (gen_params [i]);

			ec = new EmitContext (
				this, this, Location, null, return_type, ModFlags, false);

			return true;
		}

		public bool DefineType (EmitContext ec, MethodBuilder mb,
					MethodInfo implementing, bool is_override)
		{
			for (int i = 0; i < TypeParameters.Length; i++)
				if (!TypeParameters [i].DefineType (
					    ec, mb, implementing, is_override))
					return false;

			return true;
		}

		public override bool DefineMembers (TypeContainer parent)
		{
			return true;
		}

		public override MemberList FindMembers (MemberTypes mt, BindingFlags bf,
							MemberFilter filter, object criteria)
		{
			throw new Exception ();
		}		

		public override MemberCache MemberCache {
			get {
				throw new Exception ();
			}
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			// FIXME
		}

		protected override void VerifyObsoleteAttribute()
		{
			// FIXME
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Method | AttributeTargets.ReturnValue;
			}
		}
	}

	public class DefaultValueExpression : Expression
	{
		Expression expr;
		LocalTemporary temp_storage;

		public DefaultValueExpression (Expression expr, Location loc)
		{
			this.expr = expr;
			this.loc = loc;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			TypeExpr texpr = expr.ResolveAsTypeTerminal (ec);
			if (texpr == null)
				return null;

			type = texpr.Type;
			if (type.IsGenericParameter || TypeManager.IsValueType (type))
				temp_storage = new LocalTemporary (ec, type);

			eclass = ExprClass.Variable;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			if (temp_storage != null) {
				temp_storage.AddressOf (ec, AddressOp.LoadStore);
				ec.ig.Emit (OpCodes.Initobj, type);
				temp_storage.Emit (ec);
			} else
				ec.ig.Emit (OpCodes.Ldnull);
		}
	}
}
