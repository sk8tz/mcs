//
// anonymous.cs: Support for anonymous methods and types
//
// Author:
//   Miguel de Icaza (miguel@ximain.com)
//   Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
// Copyright 2003-2008 Novell, Inc.
//

using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {

	public abstract class CompilerGeneratedClass : Class
	{
		public static string MakeName (string host, string typePrefix, string name, int id)
		{
			return "<" + host + ">" + typePrefix + "__" + name + id.ToString ();
		}
		
		protected CompilerGeneratedClass (DeclSpace parent, MemberName name, int mod, Location loc)
			: base (parent.NamespaceEntry, parent, name, mod | Modifiers.COMPILER_GENERATED | Modifiers.SEALED, null)
		{
		}

		protected CompilerGeneratedClass (DeclSpace parent, GenericMethod generic, MemberName name, int mod, Location loc)
			: this (parent, name, mod, loc)
		{
			if (generic != null) {
				ArrayList list = new ArrayList ();
				foreach (TypeParameter tparam in generic.TypeParameters) {
					if (tparam.Constraints != null)
						list.Add (tparam.Constraints.Clone ());
				}
				SetParameterInfo (list);
			}
		}

		protected void CheckMembersDefined ()
		{
			if (members_defined)
				throw new InternalErrorException ("Helper class already defined!");
		}
	}

	//
	// Anonymous method storey is created when an anonymous method uses
	// variable or parameter from outer scope. They are then hoisted to
	// anonymous method storey (captured)
	//
	public class AnonymousMethodStorey : CompilerGeneratedClass
	{
		class StoreyFieldPair {
			public AnonymousMethodStorey Storey;
			public Field Field;

			public StoreyFieldPair (AnonymousMethodStorey storey)
			{
				this.Storey = storey;
			}

			public override int GetHashCode ()
			{
				return Storey.ID.GetHashCode ();
			}

			public override bool Equals (object obj)
			{
				return (AnonymousMethodStorey)obj == Storey;
			}
		}

		class HoistedGenericField : Field
		{
			public HoistedGenericField (DeclSpace parent, FullNamedExpression type, int mod, string name,
				  Attributes attrs, Location loc)
				: base (parent, type, mod, name, attrs, loc)
			{
			}

			public override bool Define ()
			{
				type_name.Type = ((AnonymousMethodStorey) Parent).MutateType (type_name.Type);
				return base.Define ();
			}
		}

		// TODO: Why is it required by debugger ?
		public readonly int ID;
		static int unique_id;

		public readonly Block OriginalSourceBlock;

		// A list of StoreyFieldPair with local field keeping parent storey instance
		ArrayList used_parent_storeys;

		// A list of hoisted parameters
		protected ArrayList hoisted_params;

		// Hoisted this
		HoistedThis hoisted_this;

		// Local variable which holds this storey instance
		public LocalTemporary Instance;

		bool references_defined;
		bool has_hoisted_variable;

		public AnonymousMethodStorey (Block block, DeclSpace parent, MemberBase host, GenericMethod generic, string name)
			: base (parent, generic, MakeMemberName (host, name, generic, block.StartLocation), Modifiers.PRIVATE, block.StartLocation)
		{
			Parent = parent;
			OriginalSourceBlock = block;
			ID = unique_id++;
		}

		static MemberName MakeMemberName (MemberBase host, string name, GenericMethod generic, Location loc)
		{
			string host_name = host == null ? null : host.Name;
			string tname = MakeName (host_name, "c", name, unique_id);
			TypeArguments args = null;
			if (generic != null) {
				args = new TypeArguments (loc);
				foreach (TypeParameter tparam in generic.CurrentTypeParameters)
					args.Add (new SimpleName (tparam.Name, loc));
			}

			return new MemberName (tname, args, loc);
		}

		public Field AddCapturedVariable (string name, Type type)
		{
			CheckMembersDefined ();

			FullNamedExpression field_type = new TypeExpression (type, Location);
			if (!IsGeneric)
				return AddCompilerGeneratedField (name, field_type);

			const int mod = Modifiers.INTERNAL | Modifiers.COMPILER_GENERATED;
			Field f = new HoistedGenericField (this, field_type, mod, name, null, Location);
			AddField (f);
			return f;
		}

		protected Field AddCompilerGeneratedField (string name, FullNamedExpression type)
		{
			const int mod = Modifiers.INTERNAL | Modifiers.COMPILER_GENERATED;
			Field f = new Field (this, type, mod, name, null, Location);
			AddField (f);
			return f;
		}

		public void AddParentStoreyReference (AnonymousMethodStorey s)
		{
			CheckMembersDefined ();

			if (used_parent_storeys == null)
				used_parent_storeys = new ArrayList ();
			else if (used_parent_storeys.IndexOf (s) != -1)
				return;

			has_hoisted_variable = true;
			used_parent_storeys.Add (new StoreyFieldPair (s));
		}

		public void CaptureLocalVariable (EmitContext ec, LocalInfo local_info)
		{
			if (local_info.HoistedVariableReference != null)
				return;

			HoistedVariable var = new HoistedLocalVariable (this, local_info, GetVariableMangledName (local_info));
			local_info.HoistedVariableReference = var;
			has_hoisted_variable = true;
		}

		public void CaptureParameter (EmitContext ec, ParameterReference param_ref)
		{
			if (param_ref.HoistedVariable != null)
				return;

			if (hoisted_params == null)
				hoisted_params = new ArrayList ();

			HoistedVariable expr = new HoistedParameter (this, param_ref);
			param_ref.Parameter.HoistedVariableReference = expr;
			hoisted_params.Add (expr);
		}

		public HoistedThis CaptureThis (EmitContext ec, This t)
		{
			hoisted_this = new HoistedThis (this, t);
			return hoisted_this;
		}

		void DefineStoreyReferences ()
		{
			if (used_parent_storeys == null || references_defined)
				return;

			references_defined = true;

			//
			// For each used variable from parent scope we allocate its local reference point
			//
			for (int i = 0; i < used_parent_storeys.Count; ++i) {
				StoreyFieldPair sf = (StoreyFieldPair) used_parent_storeys [i];
				AnonymousMethodStorey p_storey = sf.Storey;
				TypeExpr type_expr = new TypeExpression (p_storey.TypeBuilder, Location);

				sf.Field = AddCompilerGeneratedField ("<>f__ref$" + p_storey.ID, type_expr);
				sf.Field.Define ();
			}
		}

		//
		// Initializes all hoisted variables
		//
		public void EmitHoistedVariables (EmitContext ec)
		{
			// There can be only one instance variable for each storey type
			if (Instance != null)
				throw new InternalErrorException ();

			//
			// A storey with hoisted `this' is an instance method
			//
			if (!HasHoistedVariables) {
				hoisted_this.RemoveHoisting ();
				return;
			}

			DefineStoreyReferences ();

			//
			// Create an instance of storey type
			//
			Expression storey_type_expr;
			if (is_generic) {
				//
				// Use current method type parameter (MVAR) for top level storey only. All
				// nested storeys use class type parameter (VAR)
				//
				TypeParameter[] tparams = ec.CurrentAnonymousMethod != null ?
					ec.CurrentAnonymousMethod.Storey.CurrentTypeParameters :
					ec.GenericDeclContainer.CurrentTypeParameters;

				if (tparams.Length != CountTypeParameters) {
					TypeParameter [] full = new TypeParameter [CountTypeParameters];
					DeclSpace parent = ec.DeclContainer.Parent;
					parent.CurrentTypeParameters.CopyTo (full, 0);
					tparams.CopyTo (full, parent.CountTypeParameters);
					tparams = full;
				}

				storey_type_expr = new ConstructedType (TypeBuilder, tparams, Location);
			} else {
				storey_type_expr = new TypeExpression (TypeBuilder, Location);
			}

			Expression e = new New (storey_type_expr, new ArrayList (0), Location).Resolve (ec);
			e.Emit (ec);

			Instance = new LocalTemporary (storey_type_expr.Type);
			Instance.Store (ec);

			EmitHoistedFieldsInitialization (ec);
		}

		void EmitHoistedFieldsInitialization (EmitContext ec)
		{
			//
			// Initialize all storey reference fields by using local or hoisted variables
			//
			if (used_parent_storeys != null) {
				foreach (StoreyFieldPair sf in used_parent_storeys) {
					//
					// Setting local field
					//
					FieldExpr f_set_expr = new FieldExpr (sf.Field.FieldBuilder, Location);
					f_set_expr.InstanceExpression = GetStoreyInstanceExpression (ec);

					SimpleAssign a = new SimpleAssign (f_set_expr, sf.Storey.GetStoreyInstanceExpression (ec));
					if (a.Resolve (ec) != null)
						a.EmitStatement (ec);
				}
			}

			//
			// Setting currect anonymous method to null blocks any further variable hoisting
			//
			AnonymousExpression ae = ec.CurrentAnonymousMethod;
			ec.CurrentAnonymousMethod = null;

			if (hoisted_params != null) {
				foreach (HoistedParameter hp in hoisted_params) {
					hp.EmitHoistingAssignment (ec);
				}
			}

			if (hoisted_this != null) {
				hoisted_this.EmitHoistingAssignment (ec);
			}

			ec.CurrentAnonymousMethod = ae;
		}

		public override void EmitType ()
		{
			DefineStoreyReferences ();
			base.EmitType ();
		}

		//
		// Returns a field which holds referenced storey instance
		//
		Field GetReferencedStoreyField (AnonymousMethodStorey storey)
		{
			if (used_parent_storeys == null)
				return null;

			foreach (StoreyFieldPair sf in used_parent_storeys) {
				if (sf.Storey == storey)
					return sf.Field;
			}

			return null;
		}

		//
		// Creates storey instance expression regardless of currect IP
		//
		public Expression GetStoreyInstanceExpression (EmitContext ec)
		{
			AnonymousExpression am = ec.CurrentAnonymousMethod;

			//
			// Access from original block -> storey
			//
			if (am == null)
				return Instance;

			//
			// Access from anonymous method implemented as a static -> storey
			//
			if (am.Storey == null)
				return Instance;

			Field f = am.Storey.GetReferencedStoreyField (this);
			if (f == null) {
				if (am.Storey == this) {
					//
					// Access inside of same storey (S -> S)
					//
					return new CompilerGeneratedThis (TypeBuilder, Location);
				}
				//
				// External field access
				//
				return Instance;
			}

			//
			// Storey was cached to local field
			//
			FieldExpr f_ind = new FieldExpr (f.FieldBuilder, Location);
			f_ind.InstanceExpression = new CompilerGeneratedThis (TypeBuilder, Location);
			return f_ind;
		}

		protected virtual string GetVariableMangledName (LocalInfo local_info)
		{
			//
			// No need to mangle anonymous method hoisted variables cause they
			// are hoisted in their own scopes
			//
			return local_info.Name;
		}

		//
		// Returns true when at least one local variable or parameter is
		// hoisted, or story is transitioned
		//
		public bool HasHoistedVariables {
			get {
				return has_hoisted_variable || hoisted_params != null;
			}
			set {
				has_hoisted_variable = value;
			}
		}

		//
		// Mutate type dispatcher
		//
		public Type MutateType (Type type)
		{
#if GMCS_SOURCE
			if (TypeManager.IsGenericType (type))
				return MutateGenericType (type);

			if (TypeManager.IsGenericParameter (type))
				return MutateGenericArgument (type);

			if (type.IsArray)
				return MutateArrayType (type);
#endif
			return type;
		}

		//
		// Changes method type arguments (MVAR) to storey (VAR) type arguments
		//
		public MethodInfo MutateGenericMethod (MethodInfo method)
		{
#if GMCS_SOURCE
			Type [] t_args = TypeManager.GetGenericArguments (method);
			if (TypeManager.IsGenericType (method.DeclaringType)) {
				Type t = MutateGenericType (method.DeclaringType);
				if (t != method.DeclaringType)
					method = TypeBuilder.GetMethod (t, method);
			}

			if (t_args == null || t_args.Length == 0)
				return method;

			for (int i = 0; i < t_args.Length; ++i)
				t_args [i] = MutateType (t_args [i]);

			return method.GetGenericMethodDefinition ().MakeGenericMethod (t_args);
#else
			throw new NotSupportedException ();
#endif
		}

		public ConstructorInfo MutateConstructor (ConstructorInfo ctor)
		{
#if GMCS_SOURCE		
			if (TypeManager.IsGenericType (ctor.DeclaringType)) {
				Type t = MutateGenericType (ctor.DeclaringType);
				if (t != ctor.DeclaringType) {
					// TODO: It should throw on imported types
					return TypeBuilder.GetConstructor (t, ctor);
				}
			}
#endif
			return ctor;
		}
		
		public FieldInfo MutateField (FieldInfo field)
		{
#if GMCS_SOURCE
			if (TypeManager.IsGenericType (field.DeclaringType)) {
				Type t = MutateGenericType (field.DeclaringType);
				if (t != field.DeclaringType) {
					// TODO: It should throw on imported types
					return TypeBuilder.GetField (t, field);
				}
			}
#endif
			return field;
		}		

#if GMCS_SOURCE
		protected Type MutateArrayType (Type array)
		{
			int rank = array.GetArrayRank ();
			Type element = TypeManager.GetElementType (array);
			if (element.IsArray)
				throw new NotImplementedException ();

			if (TypeManager.IsGenericParameter (element)) {
				element = MutateGenericArgument (element);
			} else if (TypeManager.IsGenericType (element)) {
				element = MutateGenericType (element);
			} else {
				return array;
			}

			return element.MakeArrayType (rank);
		}

		protected Type MutateGenericType (Type type)
		{
			Type [] t_args = TypeManager.GetTypeArguments (type);
			if (t_args == null || t_args.Length == 0)
				return type;

			for (int i = 0; i < t_args.Length; ++i)
				t_args [i] = MutateType (t_args [i]);

			return type.GetGenericTypeDefinition ().MakeGenericType (t_args);
		}
#endif

		//
		// Changes method generic argument (MVAR) to type generic argument (VAR)
		//
		public Type MutateGenericArgument (Type type)
		{
			foreach (TypeParameter tp in CurrentTypeParameters) {
				if (tp.Name == type.Name) {
					return tp.Type;
				}
			}

			return type;
		}

		public static void Reset ()
		{
			unique_id = 0;
		}
		
		public void Undo ()
		{
			if (hoisted_this != null)
				hoisted_this.RemoveHoisting ();
		}
	}

	public abstract class HoistedVariable
	{
		class ExpressionTreeProxy : Expression
		{
			readonly HoistedVariable hv;

			public ExpressionTreeProxy (HoistedVariable hv)
			{
				this.hv = hv;
			}

			public override Expression CreateExpressionTree (EmitContext ec)
			{
				throw new NotSupportedException ("ET");
			}

			public override Expression DoResolve (EmitContext ec)
			{
				eclass = ExprClass.MethodGroup;
				type = TypeManager.expression_type_expr.Type;
				return this;
			}

			public override void Emit (EmitContext ec)
			{
				Expression e = hv.GetFieldExpression (ec).CreateExpressionTree (ec);
				// This should never fail
				e = e.Resolve (ec);
				if (e != null)
					e.Emit (ec);
			}
		}
	
		protected readonly AnonymousMethodStorey storey;
		protected Field field;
		Hashtable cached_inner_access; // TODO: Hashtable is too heavyweight

		protected HoistedVariable (AnonymousMethodStorey storey, string name, Type type)
		{
			this.storey = storey;

			this.field = storey.AddCapturedVariable (name, type);
		}

		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			GetFieldExpression (ec).AddressOf (ec, mode);
		}

		public Expression CreateExpressionTree (EmitContext ec)
		{
			return new ExpressionTreeProxy (this);
		}

		public void Emit (EmitContext ec)
		{
			GetFieldExpression (ec).Emit (ec);
		}

		//
		// Creates field access expression for hoisted variable
		//
		protected FieldExpr GetFieldExpression (EmitContext ec)
		{
			if (ec.CurrentAnonymousMethod == null) {
				//
				// When setting top-level hoisted variable in generic storey
				// change storey generic types to method generic types (VAR -> MVAR)
				//
				FieldExpr outer_access = storey.MemberName.IsGeneric ?
					new FieldExpr (field.FieldBuilder, storey.Instance.Type, field.Location) :
					new FieldExpr (field.FieldBuilder, field.Location);

				outer_access.InstanceExpression = storey.GetStoreyInstanceExpression (ec);
				outer_access.Resolve (ec);
				return outer_access;
			}

			FieldExpr inner_access;
			if (cached_inner_access != null) {
				inner_access = (FieldExpr) cached_inner_access [ec.CurrentAnonymousMethod];
			} else {
				inner_access = null;
				cached_inner_access = new Hashtable (4);
			}

			if (inner_access == null) {
				inner_access = new FieldExpr (field.FieldBuilder, field.Location);
				inner_access.InstanceExpression = storey.GetStoreyInstanceExpression (ec);
				inner_access.Resolve (ec);
				cached_inner_access.Add (ec.CurrentAnonymousMethod, inner_access);
			}

			return inner_access;
		}

		public abstract void EmitSymbolInfo ();

		public void Emit (EmitContext ec, bool leave_copy)
		{
			GetFieldExpression (ec).Emit (ec, leave_copy);
		}

		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			GetFieldExpression (ec).EmitAssign (ec, source, leave_copy, false);
		}
	}

	class HoistedParameter : HoistedVariable
	{
		class HoistedFieldAssign : Assign
		{
			public HoistedFieldAssign (Expression target, Expression source)
				: base (target, source, source.Location)
			{
			}

			protected override Expression ResolveConversions (EmitContext ec)
			{
				//
				// Implicit conversion check fails for hoisted type arguments
				// as they are of different types (!!0 x !0)
				//
				return this;
			}
		}

		readonly ParameterReference parameter;

		public HoistedParameter (AnonymousMethodStorey scope, ParameterReference par)
			: base (scope, par.Name, par.Type)
		{
			this.parameter = par;
		}

		public void EmitHoistingAssignment (EmitContext ec)
		{
			//
			// Remove hoisted redirection to emit assignment from original parameter
			//
			HoistedVariable temp = parameter.Parameter.HoistedVariableReference;
			parameter.Parameter.HoistedVariableReference = null;

			Assign a = new HoistedFieldAssign (GetFieldExpression (ec), parameter);
			if (a.Resolve (ec) != null)
				a.EmitStatement (ec);

			parameter.Parameter.HoistedVariableReference = temp;
		}

		public override void EmitSymbolInfo ()
		{
			SymbolWriter.DefineCapturedParameter (storey.ID, field.Name, field.Name);
		}

		public Field Field {
			get { return field; }
		}
	}

	class HoistedLocalVariable : HoistedVariable
	{
		public HoistedLocalVariable (AnonymousMethodStorey scope, LocalInfo local, string name)
			: base (scope, name, local.VariableType)
		{
		}

		public override void EmitSymbolInfo ()
		{
			SymbolWriter.DefineCapturedLocal (storey.ID, field.Name, field.Name);
		}
	}

	public class HoistedThis : HoistedVariable
	{
		readonly This this_reference;

		public HoistedThis (AnonymousMethodStorey storey, This this_reference)
			: base (storey, "<>f__this", this_reference.Type)
		{
			this.this_reference = this_reference;
		}

		public void EmitHoistingAssignment (EmitContext ec)
		{
			SimpleAssign a = new SimpleAssign (GetFieldExpression (ec), this_reference);
			if (a.Resolve (ec) != null)
				a.EmitStatement (ec);
		}

		public override void EmitSymbolInfo ()
		{
			SymbolWriter.DefineCapturedThis (storey.ID, field.Name);
		}

		public void RemoveHoisting ()
		{
			this_reference.RemoveHoisting ();
		}
	}

	//
	// Anonymous method expression as created by parser
	//
	public class AnonymousMethodExpression : Expression
	{
		protected readonly TypeContainer Host;
		public readonly Parameters Parameters;
		ListDictionary compatibles;
		public ToplevelBlock Block;

		public AnonymousMethodExpression (TypeContainer host, Parameters parameters, Location loc)
		{
			this.Host = host;
			this.Parameters = parameters;
			this.loc = loc;
			this.compatibles = new ListDictionary ();
		}

		public override string ExprClassName {
			get {
				return "anonymous method";
			}
		}

		public virtual bool HasExplicitParameters {
			get {
				return Parameters != null;
			}
		}

		//
		// Returns true if the body of lambda expression can be implicitly
		// converted to the delegate of type `delegate_type'
		//
		public bool ImplicitStandardConversionExists (EmitContext ec, Type delegate_type)
		{
			using (ec.Set (EmitContext.Flags.ProbingMode)) {
				return Compatible (ec, delegate_type) != null;
			}
		}

		protected Type CompatibleChecks (EmitContext ec, Type delegate_type)
		{
			if (TypeManager.IsDelegateType (delegate_type))
				return delegate_type;

#if GMCS_SOURCE
			if (TypeManager.DropGenericTypeArguments (delegate_type) == TypeManager.expression_type) {
				delegate_type = TypeManager.GetTypeArguments (delegate_type) [0];
				if (TypeManager.IsDelegateType (delegate_type))
					return delegate_type;

				Report.Error (835, loc, "Cannot convert `{0}' to an expression tree of non-delegate type `{1}'",
					GetSignatureForError (), TypeManager.CSharpName (delegate_type));
				return null;
			}
#endif

			Report.Error (1660, loc, "Cannot convert `{0}' to non-delegate type `{1}'",
				      GetSignatureForError (), TypeManager.CSharpName (delegate_type));
			return null;
		}

		protected bool VerifyExplicitParameters (Type delegate_type, ParameterData parameters, bool ignore_error)
		{
			if (VerifyParameterCompatibility (delegate_type, parameters, ignore_error))
				return true;

			if (!ignore_error)
				Report.Error (1661, loc,
					"Cannot convert `{0}' to delegate type `{1}' since there is a parameter mismatch",
					GetSignatureForError (), TypeManager.CSharpName (delegate_type));

			return false;
		}

		protected bool VerifyParameterCompatibility (Type delegate_type, ParameterData invoke_pd, bool ignore_errors)
		{
			if (Parameters.Count != invoke_pd.Count) {
				if (ignore_errors)
					return false;
				
				Report.Error (1593, loc, "Delegate `{0}' does not take `{1}' arguments",
					      TypeManager.CSharpName (delegate_type), Parameters.Count.ToString ());
				return false;
			}
			
			if (!HasExplicitParameters)
				return true;			

			bool error = false;
			for (int i = 0; i < Parameters.Count; ++i) {
				Parameter.Modifier p_mod = invoke_pd.ParameterModifier (i);
				if (Parameters.ParameterModifier (i) != p_mod && p_mod != Parameter.Modifier.PARAMS) {
					if (ignore_errors)
						return false;
					
					if (p_mod == Parameter.Modifier.NONE)
						Report.Error (1677, loc, "Parameter `{0}' should not be declared with the `{1}' keyword",
							      (i + 1).ToString (), Parameter.GetModifierSignature (Parameters.ParameterModifier (i)));
					else
						Report.Error (1676, loc, "Parameter `{0}' must be declared with the `{1}' keyword",
							      (i+1).ToString (), Parameter.GetModifierSignature (p_mod));
					error = true;
					continue;
				}

				Type type = invoke_pd.Types [i];
				
				// We assume that generic parameters are always inflated
				if (TypeManager.IsGenericParameter (type))
					continue;
				
				if (TypeManager.HasElementType (type) && TypeManager.IsGenericParameter (TypeManager.GetElementType (type)))
					continue;
				
				if (invoke_pd.ParameterType (i) != Parameters.ParameterType (i)) {
					if (ignore_errors)
						return false;
					
					Report.Error (1678, loc, "Parameter `{0}' is declared as type `{1}' but should be `{2}'",
						      (i+1).ToString (),
						      TypeManager.CSharpName (Parameters.ParameterType (i)),
						      TypeManager.CSharpName (invoke_pd.ParameterType (i)));
					error = true;
				}
			}

			return !error;
		}

		//
		// Infers type arguments based on explicit arguments
		//
		public bool ExplicitTypeInference (TypeInferenceContext type_inference, Type delegate_type)
		{
			if (!HasExplicitParameters)
				return false;

			if (!TypeManager.IsDelegateType (delegate_type)) {
#if GMCS_SOURCE
				if (TypeManager.DropGenericTypeArguments (delegate_type) != TypeManager.expression_type)
					return false;

				delegate_type = delegate_type.GetGenericArguments () [0];
				if (!TypeManager.IsDelegateType (delegate_type))
					return false;
#else
				return false;
#endif
			}
			
			ParameterData d_params = TypeManager.GetDelegateParameters (delegate_type);
			if (d_params.Count != Parameters.Count)
				return false;

			for (int i = 0; i < Parameters.Count; ++i) {
				Type itype = d_params.Types [i];
				if (!TypeManager.IsGenericParameter (itype)) {
					if (!TypeManager.HasElementType (itype))
						continue;
					
					if (!TypeManager.IsGenericParameter (itype.GetElementType ()))
					    continue;
				}
				type_inference.ExactInference (Parameters.FixedParameters[i].ParameterType, itype);
			}
			return true;
		}

		public Type InferReturnType (EmitContext ec, TypeInferenceContext tic, Type delegate_type)
		{
			AnonymousMethodBody am;
			using (ec.Set (EmitContext.Flags.ProbingMode | EmitContext.Flags.InferReturnType)) {
				am = CompatibleMethod (ec, tic, GetType (), delegate_type);
			}
			
			if (am == null)
				return null;

			if (am.ReturnType == TypeManager.null_type)
				am.ReturnType = null;

			return am.ReturnType;
		}

		//
		// Returns AnonymousMethod container if this anonymous method
		// expression can be implicitly converted to the delegate type `delegate_type'
		//
		public Expression Compatible (EmitContext ec, Type type)
		{
			Expression am = (Expression) compatibles [type];
			if (am != null)
				return am;

			Type delegate_type = CompatibleChecks (ec, type);
			if (delegate_type == null)
				return null;

			//
			// At this point its the first time we know the return type that is 
			// needed for the anonymous method.  We create the method here.
			//

			MethodInfo invoke_mb = Delegate.GetInvokeMethod (
				ec.ContainerType, delegate_type);
			Type return_type = TypeManager.TypeToCoreType (invoke_mb.ReturnType);

#if MS_COMPATIBLE
			Type[] g_args = delegate_type.GetGenericArguments ();
			if (return_type.IsGenericParameter)
				return_type = g_args [return_type.GenericParameterPosition];
#endif

			//
			// Second: the return type of the delegate must be compatible with 
			// the anonymous type.   Instead of doing a pass to examine the block
			// we satisfy the rule by setting the return type on the EmitContext
			// to be the delegate type return type.
			//

			try {
				int errors = Report.Errors;
				am = CompatibleMethod (ec, null, return_type, delegate_type);
				if (am != null && delegate_type != type && errors == Report.Errors)
					am = CreateExpressionTree (ec, delegate_type);

				if (!ec.IsInProbingMode)
					compatibles.Add (type, am);

				return am;
			} catch (Exception e) {
				throw new InternalErrorException (e, loc);
			}
		}

		protected virtual Expression CreateExpressionTree (EmitContext ec, Type delegate_type)
		{
			return CreateExpressionTree (ec);
		}

		public override Expression CreateExpressionTree (EmitContext ec)
		{
			Report.Error (1946, loc, "An anonymous method cannot be converted to an expression tree");
			return null;
		}

		protected virtual Parameters ResolveParameters (EmitContext ec, TypeInferenceContext tic, Type delegate_type)
		{
			ParameterData delegate_parameters = TypeManager.GetDelegateParameters (delegate_type);

			if (Parameters == null) {
				//
				// We provide a set of inaccessible parameters
				//
				Parameter[] fixedpars = new Parameter[delegate_parameters.Count];

				for (int i = 0; i < delegate_parameters.Count; i++) {
					Parameter.Modifier i_mod = delegate_parameters.ParameterModifier (i);
					if ((i_mod & Parameter.Modifier.OUTMASK) != 0) {
						Report.Error (1688, loc, "Cannot convert anonymous " +
								  "method block without a parameter list " +
								  "to delegate type `{0}' because it has " +
								  "one or more `out' parameters.",
								  TypeManager.CSharpName (delegate_type));
						return null;
					}
					fixedpars[i] = new Parameter (
						delegate_parameters.ParameterType (i), null,
						delegate_parameters.ParameterModifier (i), null, loc);
				}

				return Parameters.CreateFullyResolved (fixedpars, delegate_parameters.Types);
			}

			if (!VerifyExplicitParameters (delegate_type, delegate_parameters, ec.IsInProbingMode)) {
				return null;
			}

			return Parameters;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			if (!ec.IsAnonymousMethodAllowed) {
				Report.Error (1706, loc, "Anonymous methods and lambda expressions cannot be used in the current context");
				return null;
			}

			//
			// Set class type, set type
			//

			eclass = ExprClass.Value;

			//
			// This hack means `The type is not accessible
			// anywhere', we depend on special conversion
			// rules.
			// 
			type = TypeManager.anonymous_method_type;

			if ((Parameters != null) && !Parameters.Resolve (ec))
				return null;

			// FIXME: The emitted code isn't very careful about reachability
			// so, ensure we have a 'ret' at the end
			if (ec.CurrentBranching != null &&
			    ec.CurrentBranching.CurrentUsageVector.IsUnreachable)
				ec.NeedReturnLabel ();

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			// nothing, as we only exist to not do anything.
		}

		public override string GetSignatureForError ()
		{
			return ExprClassName;
		}

		protected AnonymousMethodBody CompatibleMethod (EmitContext ec, TypeInferenceContext tic, Type return_type, Type delegate_type)
		{
			Parameters p = ResolveParameters (ec, tic, delegate_type);
			if (p == null)
				return null;

			ToplevelBlock b = ec.IsInProbingMode ? (ToplevelBlock) Block.PerformClone () : Block;

			AnonymousMethodBody anonymous = CompatibleMethodFactory (return_type, delegate_type, p, b);
			if (!anonymous.Compatible (ec))
				return null;

			return anonymous;
		}

		protected virtual AnonymousMethodBody CompatibleMethodFactory (Type return_type, Type delegate_type, Parameters p, ToplevelBlock b)
		{
			return new AnonymousMethodBody (Host,
				p, b, return_type,
				delegate_type, loc);
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			AnonymousMethodExpression target = (AnonymousMethodExpression) t;

			target.Block = (ToplevelBlock) clonectx.LookupBlock (Block);
		}
	}

	//
	// Abstract expression for any block which requires variables hoisting
	//
	public abstract class AnonymousExpression : Expression
	{
		protected class AnonymousMethodMethod : Method
		{
			public readonly AnonymousExpression AnonymousMethod;
			public readonly AnonymousMethodStorey Storey;
			readonly string RealName;

			public AnonymousMethodMethod (DeclSpace parent, AnonymousExpression am, AnonymousMethodStorey storey,
							  GenericMethod generic, TypeExpr return_type,
							  int mod, string real_name, MemberName name,
							  Parameters parameters)
				: base (parent, generic, return_type, mod | Modifiers.COMPILER_GENERATED,
						false, name, parameters, null)
			{
				this.AnonymousMethod = am;
				this.Storey = storey;
				this.RealName = real_name;

				Parent.PartialContainer.AddMethod (this);
				Block = am.Block;
			}

			public override EmitContext CreateEmitContext (DeclSpace tc, ILGenerator ig)
			{
				EmitContext aec = AnonymousMethod.aec;
				aec.ig = ig;
				aec.IsStatic = (ModFlags & Modifiers.STATIC) != 0;
				return aec;
			}

			public override bool Define ()
			{
				if (Storey != null && Storey.IsGeneric && Storey.HasHoistedVariables) {
					if (!Parameters.Empty) {
						Type [] ptypes = Parameters.Types;
						for (int i = 0; i < ptypes.Length; ++i)
							ptypes [i] = Storey.MutateType (ptypes [i]);
					}

					member_type = Storey.MutateType (ReturnType);
				}

				return base.Define ();
			}

			public override void Emit ()
			{
				//
				// Before emitting any code we have to change all MVAR references to VAR
				// when the method is of generic type and has hoisted variables
				//
				if (Storey == Parent && Storey.IsGeneric) {
					AnonymousMethod.aec.ReturnType = Storey.MutateType (ReturnType);
					block.MutateHoistedGenericType (Storey);
				}

				if (MethodBuilder == null) {
					ResolveMembers ();
					Define ();
				}

				base.Emit ();
			}

			public override void EmitExtraSymbolInfo (SourceMethod source)
			{
				source.SetRealMethodName (RealName);
			}
		}

		//
		// The block that makes up the body for the anonymous method
		//
		protected readonly ToplevelBlock Block;

		public Type ReturnType;
		public readonly TypeContainer Host;

		//
		// The implicit method we create
		//
		protected AnonymousMethodMethod method;
		protected EmitContext aec;

		protected AnonymousExpression (TypeContainer host, ToplevelBlock block, Type return_type, Location loc)
		{
			this.ReturnType = return_type;
			this.Host = host;

			this.Block = block;
			this.loc = loc;
		}

		public abstract void AddStoreyReference (AnonymousMethodStorey storey);
		public abstract string ContainerType { get; }
		public abstract bool IsIterator { get; }
		public abstract AnonymousMethodStorey Storey { get; }

		public bool Compatible (EmitContext ec)
		{
			// TODO: Implement clone
			aec = new EmitContext (
				ec.ResolveContext, ec.TypeContainer, ec.DeclContainer,
				Location, null, ReturnType,
				(ec.InUnsafe ? Modifiers.UNSAFE : 0), /* No constructor */ false);

			aec.CurrentAnonymousMethod = this;
			aec.IsStatic = ec.IsStatic;

			IDisposable aec_dispose = null;
			EmitContext.Flags flags = 0;
			if (ec.InferReturnType)
				flags |= EmitContext.Flags.InferReturnType;

			if (ec.IsInProbingMode)
				flags |= EmitContext.Flags.ProbingMode;

			if (ec.IsInFieldInitializer)
				flags |= EmitContext.Flags.InFieldInitializer;

			if (ec.IsInUnsafeScope)
				flags |= EmitContext.Flags.InUnsafe;

			// HACK: Flag with 0 cannot be set 
			if (flags != 0)
				aec_dispose = aec.Set (flags);

			bool unreachable;
			bool res = aec.ResolveTopBlock (ec, Block, Block.Parameters, null, out unreachable);

			if (ec.InferReturnType)
				ReturnType = aec.ReturnType;

			if (aec_dispose != null) {
				aec_dispose.Dispose ();
			}

			return res;
		}
	}

	public class AnonymousMethodBody : AnonymousExpression
	{
		ArrayList referenced_storeys;
		protected readonly Parameters parameters;
		static int unique_id;

		public AnonymousMethodBody (TypeContainer host, Parameters parameters,
					ToplevelBlock block, Type return_type, Type delegate_type,
					Location loc)
			: base (host, block, return_type, loc)
		{
			this.type = delegate_type;
			this.parameters = parameters;
		}

		public override string ContainerType {
			get { return "anonymous method"; }
		}

		public override AnonymousMethodStorey Storey {
			get { return method.Storey; }
		}

		public override bool IsIterator {
			get { return false; }
		}

		//
		// Adds new storey reference to track out of scope variables
		//
		public override void AddStoreyReference (AnonymousMethodStorey storey)
		{
			if (referenced_storeys == null) {
				referenced_storeys = new ArrayList (2);
			} else {
				foreach (AnonymousMethodStorey ams in referenced_storeys) {
					if (ams == storey)
						return;
				}
			}

			referenced_storeys.Add (storey);
		}

		public override Expression CreateExpressionTree (EmitContext ec)
		{
			Report.Error (1945, loc, "An expression tree cannot contain an anonymous method expression");
			return null;
		}

		bool Define (EmitContext ec)
		{
			if (aec == null && !Compatible (ec))
				return false;

			// Don't define anything when we are in probing scope (nested anonymous methods)
			if (!ec.IsInProbingMode)
				method = DoCreateMethodHost (ec);

			return true;
		}

		//
		// Creates a host for the anonymous method
		//
		AnonymousMethodMethod DoCreateMethodHost (EmitContext ec)
		{
			AnonymousMethodStorey storey = FindBestMethodStorey ();
			
			if (referenced_storeys != null) {
				foreach (AnonymousMethodStorey s in referenced_storeys) {
					if (s == storey)
						continue;

					storey.AddParentStoreyReference (s);
					s.HasHoistedVariables = true;
					Block.Parent.Explicit.PropagateStoreyReference (s);
				}
				referenced_storeys = null;
			} else {
				//
				// Ensure we have a reference between this block and a storey
				// where this anymous method is created
				//
				if (Block.Parent != null)
					Block.Parent.Explicit.PropagateStoreyReference (storey);
			}

			//
			// Anonymous method body can be converted to
			//
			// 1, an instance method in current scope when only `this' is hoisted
			// 2, a static method in current scope when neither `this' nor any variable is hoisted
			// 3, an instance method in compiler generated storey when any hoisted variable exists
			//

			int modifiers;
			if (storey != null) {
				modifiers = storey.HasHoistedVariables ? Modifiers.INTERNAL : Modifiers.PRIVATE;
			} else {
				modifiers = Modifiers.STATIC | Modifiers.PRIVATE;
			}

			DeclSpace parent = (modifiers & Modifiers.PRIVATE) != 0 ? Host : storey;

			MemberCore mc = ec.ResolveContext as MemberCore;
			string name = CompilerGeneratedClass.MakeName (parent != storey ? mc.Name : null,
				"m", null, unique_id++);

			MemberName member_name;
			GenericMethod generic_method;
			if ((modifiers & Modifiers.PRIVATE) != 0 && mc.MemberName.IsGeneric) {
				member_name = new MemberName (name, mc.MemberName.TypeArguments.Clone (), Location);

				generic_method = new GenericMethod (
					Host.NamespaceEntry, storey, member_name,
					new TypeExpression (ReturnType, Location), parameters);
				generic_method.SetParameterInfo (null);
			} else {
				member_name = new MemberName (name, Location);
				generic_method = null;
			}

			string real_name = String.Format (
				"{0}~{1}{2}", mc.GetSignatureForError (), GetSignatureForError (),
				parameters.GetSignatureForError ());

			return new AnonymousMethodMethod (parent,
				this, storey, generic_method, new TypeExpression (ReturnType, Location), modifiers,
				real_name, member_name, parameters);
		}

		public override Expression DoResolve (EmitContext ec)
		{
			if (eclass == ExprClass.Invalid) {
				if (!Define (ec))
					return null;
			}

			eclass = ExprClass.Value;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			//
			// It has to be delayed not to polute expression trees
			//
			if (method.MethodBuilder == null) {
				method.ResolveMembers ();
				method.Define ();
			}

			//
			// Don't cache generic delegates when contains MVAR argument
			//
			Field am_cache = null;
			if ((method.ModFlags & Modifiers.STATIC) != 0 && !HasGenericParameter (type)) {
				TypeContainer parent = method.Parent.PartialContainer;
				int id = parent.Fields == null ? 0 : parent.Fields.Count;
				am_cache = new Field (Host, new TypeExpression (type, loc),
					Modifiers.STATIC | Modifiers.PRIVATE | Modifiers.COMPILER_GENERATED,
					CompilerGeneratedClass.MakeName (null, "f", "am$cache", id), null, loc);
				am_cache.Define ();
				parent.AddField (am_cache);
			}

			ILGenerator ig = ec.ig;
			Label l_initialized = ig.DefineLabel ();

			if (am_cache != null) {
				ig.Emit (OpCodes.Ldsfld, am_cache.FieldBuilder);
				ig.Emit (OpCodes.Brtrue_S, l_initialized);
			}

			//
			// Load method delegate implementation
			//
			if ((method.ModFlags & Modifiers.STATIC) != 0) {
				ig.Emit (OpCodes.Ldnull);
			} else if (Storey.HasHoistedVariables) {
				Expression e = Storey.GetStoreyInstanceExpression (ec).Resolve (ec);
				if (e != null)
					e.Emit (ec);
			} else {
				ig.Emit (OpCodes.Ldarg_0);
			}

			MethodInfo delegate_method = method.MethodBuilder;
#if GMCS_SOURCE
			if (Storey != null && Storey.MemberName.IsGeneric && Storey.HasHoistedVariables)
				delegate_method = TypeBuilder.GetMethod (Storey.Instance.Type, delegate_method);
#endif
			ig.Emit (OpCodes.Ldftn, delegate_method);

			ConstructorInfo constructor_method = Delegate.GetConstructor (ec.ContainerType, type);
#if MS_COMPATIBLE
            if (type.IsGenericType && type is TypeBuilder)
                constructor_method = TypeBuilder.GetConstructor (type, constructor_method);
#endif
			ig.Emit (OpCodes.Newobj, constructor_method);

			if (am_cache != null) {
				ig.Emit (OpCodes.Stsfld, am_cache.FieldBuilder);
				ig.MarkLabel (l_initialized);
				ig.Emit (OpCodes.Ldsfld, am_cache.FieldBuilder);
			}
		}

		//
		// Look for the best storey for this anonymous method
		//
		AnonymousMethodStorey FindBestMethodStorey ()
		{
			//
			// Use the nearest parent block which has a storey
			//
			for (Block b = Block.Parent; b != null; b = b.Parent) {
				AnonymousMethodStorey s = b.Explicit.AnonymousMethodStorey;
				if (s != null)
					return s;
			}
					
			return null;
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.CSharpName (type);
		}

		static bool HasGenericParameter (Type type)
		{
#if GMCS_SOURCE
			if (type.IsGenericParameter)
				return type.DeclaringMethod != null;
				
			if (!type.IsGenericType)
				return false;

			foreach (Type t in type.GetGenericArguments ()) {
				if (HasGenericParameter (t))
					return true;
			}
#endif
			return false;
		}
		
		public override void MutateHoistedGenericType (AnonymousMethodStorey storey)
		{
			type = storey.MutateType (type);
		}

		public static void Error_AddressOfCapturedVar (string name, Location loc)
		{
			Report.Error (1686, loc,
				      "Local variable `{0}' or its members cannot have their " +
				      "address taken and be used inside an anonymous method block",
				      name);
		}

		public static void Reset ()
		{
			unique_id = 0;
		}
	}

	//
	// Anonymous type container
	//
	public class AnonymousTypeClass : CompilerGeneratedClass
	{
		static int types_counter;
		public const string ClassNamePrefix = "<>__AnonType";
		public const string SignatureForError = "anonymous type";
		
		readonly ArrayList parameters;

		private AnonymousTypeClass (DeclSpace parent, MemberName name, ArrayList parameters, Location loc)
			: base (parent, name, Modifiers.SEALED, loc)
		{
			this.parameters = parameters;
		}

		public static AnonymousTypeClass Create (TypeContainer parent, ArrayList parameters, Location loc)
		{
			if (RootContext.Version <= LanguageVersion.ISO_2)
				Report.FeatureIsNotAvailable (loc, "anonymous types");
			
			string name = ClassNamePrefix + types_counter++;

			SimpleName [] t_args = new SimpleName [parameters.Count];
			Parameter [] ctor_params = new Parameter [parameters.Count];
			for (int i = 0; i < parameters.Count; ++i) {
				AnonymousTypeParameter p = (AnonymousTypeParameter) parameters [i];

				t_args [i] = new SimpleName ("<" + p.Name + ">__T", p.Location);
				ctor_params [i] = new Parameter (t_args [i], p.Name, 0, null, p.Location);
			}

			//
			// Create generic anonymous type host with generic arguments
			// named upon properties names
			//
			AnonymousTypeClass a_type = new AnonymousTypeClass (parent.NamespaceEntry.SlaveDeclSpace,
				new MemberName (name, new TypeArguments (loc, t_args), loc), parameters, loc);

			if (parameters.Count > 0)
				a_type.SetParameterInfo (null);

			Constructor c = new Constructor (a_type, name, Modifiers.PUBLIC | Modifiers.DEBUGGER_HIDDEN,
				new Parameters (ctor_params), null, loc);
			c.Block = new ToplevelBlock (c.Parameters, loc);

			// 
			// Create fields and contructor body with field initialization
			//
			bool error = false;
			for (int i = 0; i < parameters.Count; ++i) {
				AnonymousTypeParameter p = (AnonymousTypeParameter) parameters [i];

				Field f = new Field (a_type, t_args [i], Modifiers.PRIVATE | Modifiers.READONLY,
					"<" + p.Name + ">", null, p.Location);

				if (!a_type.AddField (f)) {
					error = true;
					Report.Error (833, p.Location, "`{0}': An anonymous type cannot have multiple properties with the same name",
						p.Name);
					continue;
				}

				c.Block.AddStatement (new StatementExpression (
					new SimpleAssign (new MemberAccess (new This (p.Location), f.Name),
						c.Block.GetParameterReference (p.Name, p.Location))));

				ToplevelBlock get_block = new ToplevelBlock (p.Location);
				get_block.AddStatement (new Return (
					new MemberAccess (new This (p.Location), f.Name), p.Location));
				Accessor get_accessor = new Accessor (get_block, 0, null, p.Location);
				Property prop = new Property (a_type, t_args [i], Modifiers.PUBLIC, false,
					new MemberName (p.Name, p.Location), null, get_accessor, null, false);
				a_type.AddProperty (prop);
			}

			if (error)
				return null;

			a_type.AddConstructor (c);
			return a_type;
		}
		
		public static void Reset ()
		{
			types_counter = 0;
		}

		protected override bool AddToContainer (MemberCore symbol, string name)
		{
			MemberCore mc = (MemberCore) defined_names [name];

			if (mc == null) {
				defined_names.Add (name, symbol);
				return true;
			}

			Report.SymbolRelatedToPreviousError (mc);
			return false;
		}

		void DefineOverrides ()
		{
			Location loc = Location;

			Method equals = new Method (this, null, TypeManager.system_boolean_expr,
				Modifiers.PUBLIC | Modifiers.OVERRIDE | Modifiers.DEBUGGER_HIDDEN, false, new MemberName ("Equals", loc),
				new Parameters (new Parameter (TypeManager.system_object_expr, "obj", 0, null, loc)), null);

			Method tostring = new Method (this, null, TypeManager.system_string_expr,
				Modifiers.PUBLIC | Modifiers.OVERRIDE | Modifiers.DEBUGGER_HIDDEN, false, new MemberName ("ToString", loc),
				Mono.CSharp.Parameters.EmptyReadOnlyParameters, null);

			ToplevelBlock equals_block = new ToplevelBlock (equals.Parameters, loc);
			TypeExpr current_type;
			if (IsGeneric)
				current_type = new ConstructedType (TypeBuilder, TypeParameters, loc);
			else
				current_type = new TypeExpression (TypeBuilder, loc);

			equals_block.AddVariable (current_type, "other", loc);
			LocalVariableReference other_variable = new LocalVariableReference (equals_block, "other", loc);

			MemberAccess system_collections_generic = new MemberAccess (new MemberAccess (
				new QualifiedAliasMember ("global", "System", loc), "Collections", loc), "Generic", loc);

			Expression rs_equals = null;
			Expression string_concat = new StringConstant ("<empty type>", loc);
			Expression rs_hashcode = new IntConstant (-2128831035, loc);
			for (int i = 0; i < parameters.Count; ++i) {
				AnonymousTypeParameter p = (AnonymousTypeParameter) parameters [i];
				Field f = (Field) Fields [i];

				MemberAccess equality_comparer = new MemberAccess (new MemberAccess (
					system_collections_generic, "EqualityComparer",
						new TypeArguments (loc, new SimpleName (TypeParameters [i].Name, loc)), loc),
						"Default", loc);

				ArrayList arguments_equal = new ArrayList (2);
				arguments_equal.Add (new Argument (new MemberAccess (new This (f.Location), f.Name)));
				arguments_equal.Add (new Argument (new MemberAccess (other_variable, f.Name)));

				Expression field_equal = new Invocation (new MemberAccess (equality_comparer,
					"Equals", loc), arguments_equal);

				ArrayList arguments_hashcode = new ArrayList (1);
				arguments_hashcode.Add (new Argument (new MemberAccess (new This (f.Location), f.Name)));
				Expression field_hashcode = new Invocation (new MemberAccess (equality_comparer,
					"GetHashCode", loc), arguments_hashcode);

				IntConstant FNV_prime = new IntConstant (16777619, loc);				
				rs_hashcode = new Binary (Binary.Operator.Multiply,
					new Binary (Binary.Operator.ExclusiveOr, rs_hashcode, field_hashcode),
					FNV_prime);

				Expression field_to_string = new Conditional (new Binary (Binary.Operator.Inequality,
					new MemberAccess (new This (f.Location), f.Name), new NullLiteral (loc)),
					new Invocation (new MemberAccess (
						new MemberAccess (new This (f.Location), f.Name), "ToString"), null),
					new StringConstant ("<null>", loc));

				if (rs_equals == null) {
					rs_equals = field_equal;
					string_concat = new Binary (Binary.Operator.Addition,
						new StringConstant (p.Name + " = ", loc),
						field_to_string);
					continue;
				}

				//
				// Implementation of ToString () body using string concatenation
				//				
				string_concat = new Binary (Binary.Operator.Addition,
					new Binary (Binary.Operator.Addition,
						string_concat,
						new StringConstant (", " + p.Name + " = ", loc)),
					field_to_string);

				rs_equals = new Binary (Binary.Operator.LogicalAnd, rs_equals, field_equal);
			}

			//
			// Equals (object obj) override
			//
			equals_block.AddStatement (new StatementExpression (
				new SimpleAssign (other_variable,
					new As (equals_block.GetParameterReference ("obj", loc),
						current_type, loc), loc)));

			Expression equals_test = new Binary (Binary.Operator.Inequality, other_variable, new NullLiteral (loc));
			if (rs_equals != null)
				equals_test = new Binary (Binary.Operator.LogicalAnd, equals_test, rs_equals);
			equals_block.AddStatement (new Return (equals_test, loc));

			equals.Block = equals_block;
			equals.ResolveMembers ();
			AddMethod (equals);

			//
			// GetHashCode () override
			//
			Method hashcode = new Method (this, null, TypeManager.system_int32_expr,
				Modifiers.PUBLIC | Modifiers.OVERRIDE | Modifiers.DEBUGGER_HIDDEN,
				false, new MemberName ("GetHashCode", loc),
				Mono.CSharp.Parameters.EmptyReadOnlyParameters, null);

			//
			// Modified FNV with good avalanche behavior and uniform
			// distribution with larger hash sizes.
			//
			// const int FNV_prime = 16777619;
			// int hash = (int) 2166136261;
			// foreach (int d in data)
			//     hash = (hash ^ d) * FNV_prime;
			// hash += hash << 13;
			// hash ^= hash >> 7;
			// hash += hash << 3;
			// hash ^= hash >> 17;
			// hash += hash << 5;

			ToplevelBlock hashcode_block = new ToplevelBlock (loc);
			hashcode_block.AddVariable (TypeManager.system_int32_expr, "hash", loc);
			LocalVariableReference hash_variable = new LocalVariableReference (hashcode_block, "hash", loc);
			hashcode_block.AddStatement (new StatementExpression (
				new SimpleAssign (hash_variable, rs_hashcode)));

			hashcode_block.AddStatement (new StatementExpression (
				new CompoundAssign (Binary.Operator.Addition, hash_variable,
					new Binary (Binary.Operator.LeftShift, hash_variable, new IntConstant (13, loc)))));
			hashcode_block.AddStatement (new StatementExpression (
				new CompoundAssign (Binary.Operator.ExclusiveOr, hash_variable,
					new Binary (Binary.Operator.RightShift, hash_variable, new IntConstant (7, loc)))));
			hashcode_block.AddStatement (new StatementExpression (
				new CompoundAssign (Binary.Operator.Addition, hash_variable,
					new Binary (Binary.Operator.LeftShift, hash_variable, new IntConstant (3, loc)))));
			hashcode_block.AddStatement (new StatementExpression (
				new CompoundAssign (Binary.Operator.ExclusiveOr, hash_variable,
					new Binary (Binary.Operator.RightShift, hash_variable, new IntConstant (17, loc)))));
			hashcode_block.AddStatement (new StatementExpression (
				new CompoundAssign (Binary.Operator.Addition, hash_variable,
					new Binary (Binary.Operator.LeftShift, hash_variable, new IntConstant (5, loc)))));

			hashcode_block.AddStatement (new Return (hash_variable, loc));
			hashcode.Block = hashcode_block;
			hashcode.ResolveMembers ();
			AddMethod (hashcode);

			//
			// ToString () override
			//

			ToplevelBlock tostring_block = new ToplevelBlock (loc);
			tostring_block.AddStatement (new Return (string_concat, loc));
			tostring.Block = tostring_block;
			tostring.ResolveMembers ();
			AddMethod (tostring);
		}

		public override bool DefineMembers ()
		{
			DefineOverrides ();

			return base.DefineMembers ();
		}

		public override string GetSignatureForError ()
		{
			return SignatureForError;
		}

		public ArrayList Parameters {
			get {
				return parameters;
			}
		}
	}
}
