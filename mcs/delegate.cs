//
// delegate.cs: Delegate Handler
//
// Authors:
//     Ravi Pratap (ravi@ximian.com)
//     Miguel de Icaza (miguel@ximian.com)
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
using System.Text;

namespace Mono.CSharp {

	/// <summary>
	///   Holds Delegates
	/// </summary>
	public class Delegate : DeclSpace {
 		public Expression ReturnType;
		public Parameters      Parameters;

		public ConstructorBuilder ConstructorBuilder;
		public MethodBuilder      InvokeBuilder;
		public MethodBuilder      BeginInvokeBuilder;
		public MethodBuilder      EndInvokeBuilder;
		
		Type ret_type;

		static string[] attribute_targets = new string [] { "type", "return" };
		
		Expression instance_expr;
		MethodBase delegate_method;
		ReturnParameter return_attributes;
	
		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
		        Modifiers.UNSAFE |
			Modifiers.PRIVATE;

 		public Delegate (NamespaceEntry ns, DeclSpace parent, Expression type,
				 int mod_flags, MemberName name, Parameters param_list,
				 Attributes attrs)
			: base (ns, parent, name, attrs)

		{
			this.ReturnType = type;
			ModFlags        = Modifiers.Check (AllowedModifiers, mod_flags,
							   IsTopLevel ? Modifiers.INTERNAL :
							   Modifiers.PRIVATE, name.Location);
			Parameters      = param_list;
		}

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Target == AttributeTargets.ReturnValue) {
				if (return_attributes == null)
					return_attributes = new ReturnParameter (InvokeBuilder, Location);

				return_attributes.ApplyAttributeBuilder (a, cb);
				return;
			}

			base.ApplyAttributeBuilder (a, cb);
		}

		public override TypeBuilder DefineType ()
		{
			if (TypeBuilder != null)
				return TypeBuilder;

			if (TypeManager.multicast_delegate_type == null && !RootContext.StdLib) {
				Namespace system = RootNamespace.Global.GetNamespace ("System", true);
				TypeExpr expr = system.Lookup (this, "MulticastDelegate", Location) as TypeExpr;
				TypeManager.multicast_delegate_type = expr.Type;
			}

			if (TypeManager.multicast_delegate_type == null)
				Report.Error (-100, Location, "Internal error: delegate used before " +
					      "System.MulticastDelegate is resolved.  This can only " +
					      "happen during corlib compilation, when using a delegate " +
					      "in any of the `core' classes.  See bug #72015 for details.");

			if (IsTopLevel) {
				if (TypeManager.NamespaceClash (Name, Location))
					return null;
				
				ModuleBuilder builder = CodeGen.Module.Builder;

				TypeBuilder = builder.DefineType (
					Name, TypeAttr, TypeManager.multicast_delegate_type);
			} else {
				TypeBuilder builder = Parent.TypeBuilder;

				string name = Name.Substring (1 + Name.LastIndexOf ('.'));
				TypeBuilder = builder.DefineNestedType (
					name, TypeAttr, TypeManager.multicast_delegate_type);
			}

			TypeManager.AddUserType (this);

#if GMCS_SOURCE
			if (IsGeneric) {
				string[] param_names = new string [TypeParameters.Length];
				for (int i = 0; i < TypeParameters.Length; i++)
					param_names [i] = TypeParameters [i].Name;

				GenericTypeParameterBuilder[] gen_params;
				gen_params = TypeBuilder.DefineGenericParameters (param_names);

				int offset = CountTypeParameters - CurrentTypeParameters.Length;
				for (int i = offset; i < gen_params.Length; i++)
					CurrentTypeParameters [i - offset].Define (gen_params [i]);

				foreach (TypeParameter type_param in CurrentTypeParameters) {
					if (!type_param.Resolve (this))
						return null;
				}

				Expression current = new SimpleName (
					MemberName.Basename, TypeParameters, Location);
				current = current.ResolveAsTypeTerminal (this, false);
				if (current == null)
					return null;

				CurrentType = current.Type;
			}
#endif

			return TypeBuilder;
		}

 		public override bool Define ()
		{
#if GMCS_SOURCE
			if (IsGeneric) {
				foreach (TypeParameter type_param in TypeParameters) {
					if (!type_param.Resolve (this))
						return false;
				}

				foreach (TypeParameter type_param in TypeParameters) {
					if (!type_param.DefineType (this))
						return false;
				}

				foreach (TypeParameter type_param in TypeParameters) {
					if (!type_param.CheckDependencies ())
						return false;
				}
			}
#endif

			// FIXME: POSSIBLY make this static, as it is always constant
			//
			Type [] const_arg_types = new Type [2];
			const_arg_types [0] = TypeManager.object_type;
			const_arg_types [1] = TypeManager.intptr_type;

			const MethodAttributes ctor_mattr = MethodAttributes.RTSpecialName | MethodAttributes.SpecialName |
				MethodAttributes.HideBySig | MethodAttributes.Public;

			ConstructorBuilder = TypeBuilder.DefineConstructor (ctor_mattr,
									    CallingConventions.Standard,
									    const_arg_types);

			ConstructorBuilder.DefineParameter (1, ParameterAttributes.None, "object");
			ConstructorBuilder.DefineParameter (2, ParameterAttributes.None, "method");
			//
			// HACK because System.Reflection.Emit is lame
			//
			Parameter [] fixed_pars = new Parameter [2];
			fixed_pars [0] = new Parameter (TypeManager.object_type, "object",
							Parameter.Modifier.NONE, null, Location);
			fixed_pars [1] = new Parameter (TypeManager.intptr_type, "method", 
							Parameter.Modifier.NONE, null, Location);
			Parameters const_parameters = new Parameters (fixed_pars);
			const_parameters.Resolve (null);
			
			TypeManager.RegisterMethod (ConstructorBuilder, const_parameters);
				
			
			ConstructorBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			//
			// Here the various methods like Invoke, BeginInvoke etc are defined
			//
			// First, call the `out of band' special method for
			// defining recursively any types we need:
			
			if (!Parameters.Resolve (this))
				return false;

			//
			// Invoke method
			//

			// Check accessibility
			foreach (Type partype in Parameters.Types){
				if (!Parent.AsAccessible (partype, ModFlags)) {
					Report.Error (59, Location,
						      "Inconsistent accessibility: parameter type `" +
						      TypeManager.CSharpName (partype) + "' is less " +
						      "accessible than delegate `" + Name + "'");
					return false;
				}
			}
			
			ReturnType = ReturnType.ResolveAsTypeTerminal (this, false);
			if (ReturnType == null)
				return false;

			ret_type = ReturnType.Type;
            
			if (!Parent.AsAccessible (ret_type, ModFlags)) {
				Report.Error (58, Location,
					      "Inconsistent accessibility: return type `" +
					      TypeManager.CSharpName (ret_type) + "' is less " +
					      "accessible than delegate `" + Name + "'");
				return false;
			}

			if (RootContext.StdLib && (ret_type == TypeManager.arg_iterator_type || ret_type == TypeManager.typed_reference_type)) {
				Method.Error1599 (Location, ret_type);
				return false;
			}

			//
			// We don't have to check any others because they are all
			// guaranteed to be accessible - they are standard types.
			//
			
  			CallingConventions cc = Parameters.CallingConvention;

 			const MethodAttributes mattr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot;

 			InvokeBuilder = TypeBuilder.DefineMethod ("Invoke", 
 								  mattr,		     
 								  cc,
 								  ret_type,		     
 								  Parameters.Types);
			
			InvokeBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			TypeManager.RegisterMethod (InvokeBuilder, Parameters);

			//
			// BeginInvoke
			//
			
			Parameters async_parameters = Parameters.MergeGenerated (Parameters, 
				new Parameter (TypeManager.asynccallback_type, "callback", Parameter.Modifier.NONE, null, Location),
				new Parameter (TypeManager.object_type, "object", Parameter.Modifier.NONE, null, Location));
			
			BeginInvokeBuilder = TypeBuilder.DefineMethod ("BeginInvoke",
				mattr, cc, TypeManager.iasyncresult_type, async_parameters.Types);

			BeginInvokeBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);
			async_parameters.ApplyAttributes (BeginInvokeBuilder);
			TypeManager.RegisterMethod (BeginInvokeBuilder, async_parameters);

			//
			// EndInvoke is a bit more interesting, all the parameters labeled as
			// out or ref have to be duplicated here.
			//

			//
			// Define parameters, and count out/ref parameters
			//
			Parameters end_parameters;
			int out_params = 0;

			foreach (Parameter p in Parameters.FixedParameters) {
				if ((p.ModFlags & Parameter.Modifier.ISBYREF) != 0)
					++out_params;
			}

			if (out_params > 0) {
				Type [] end_param_types = new Type [out_params];
				Parameter [] end_params = new Parameter [out_params ];

				int param = 0; 
				for (int i = 0; i < Parameters.FixedParameters.Length; ++i) {
					Parameter p = Parameters.FixedParameters [i];
					if ((p.ModFlags & Parameter.Modifier.ISBYREF) == 0)
						continue;

					end_param_types [param] = p.ExternalType();
					end_params [param] = p;
					++param;
				}
				end_parameters = new Parameters (end_params, end_param_types);
			}
			else {
				end_parameters = Parameters.EmptyReadOnlyParameters;
			}

			end_parameters = Parameters.MergeGenerated (end_parameters,
				new Parameter (TypeManager.iasyncresult_type, "result", Parameter.Modifier.NONE, null, Location));
			
			//
			// Create method, define parameters, register parameters with type system
			//
			EndInvokeBuilder = TypeBuilder.DefineMethod ("EndInvoke", mattr, cc, ret_type, end_parameters.Types);
			EndInvokeBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			end_parameters.ApplyAttributes (EndInvokeBuilder);
			TypeManager.RegisterMethod (EndInvokeBuilder, end_parameters);

			return true;
		}

		public override void Emit ()
		{
			Parameters.ApplyAttributes (InvokeBuilder);

			if (OptAttributes != null) {
				OptAttributes.Emit ();
			}

			base.Emit ();
		}

		protected override TypeAttributes TypeAttr {
			get {
				return Modifiers.TypeAttr (ModFlags, IsTopLevel) |
					TypeAttributes.Class | TypeAttributes.Sealed |
					base.TypeAttr;
			}
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		//TODO: duplicate
		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ()) {
				return false;
			}

			Parameters.VerifyClsCompliance ();

			if (!AttributeTester.IsClsCompliant (ReturnType.Type)) {
				Report.Error (3002, Location, "Return type of `{0}' is not CLS-compliant", GetSignatureForError ());
			}
			return true;
		}

		//
		// Returns the MethodBase for "Invoke" from a delegate type, this is used
		// to extract the signature of a delegate.
		//
		public static MethodGroupExpr GetInvokeMethod (Type container_type, Type delegate_type, Location loc)
		{
			Expression ml = Expression.MemberLookup (container_type, null, delegate_type,
				"Invoke", loc);

			MethodGroupExpr mg = ml as MethodGroupExpr;
			if (mg == null) {
				Report.Error (-100, loc, "Internal error: could not find Invoke method!");
				return null;
			}

			return mg;
		}
		
		/// <summary>
		///  Verifies whether the method in question is compatible with the delegate
		///  Returns the method itself if okay and null if not.
		/// </summary>
		public static MethodBase VerifyMethod (Type container_type, Type delegate_type,
						       MethodGroupExpr old_mg, MethodBase mb,
						       Location loc)
		{
			MethodGroupExpr mg = GetInvokeMethod (container_type, delegate_type, loc);
			if (mg == null)
				return null;

			if (old_mg.HasTypeArguments)
				mg.HasTypeArguments = true;

			MethodBase invoke_mb = mg.Methods [0];
			ParameterData invoke_pd = TypeManager.GetParameterData (invoke_mb);

#if GMCS_SOURCE
			if (!mg.HasTypeArguments &&
			    !TypeManager.InferTypeArguments (invoke_pd, ref mb))
				return null;
#endif

			ParameterData pd = TypeManager.GetParameterData (mb);

			if (invoke_pd.Count != pd.Count)
				return null;

			for (int i = pd.Count; i > 0; ) {
				i--;

				Type invoke_pd_type = invoke_pd.ParameterType (i);
				Type pd_type = pd.ParameterType (i);
				Parameter.Modifier invoke_pd_type_mod = invoke_pd.ParameterModifier (i);
				Parameter.Modifier pd_type_mod = pd.ParameterModifier (i);

				invoke_pd_type_mod &= ~Parameter.Modifier.PARAMS;
				pd_type_mod &= ~Parameter.Modifier.PARAMS;

				if (invoke_pd_type_mod != pd_type_mod)
					return null;

				if (invoke_pd_type == pd_type)
					continue;

				if (!Convert.ImplicitReferenceConversionExists (new EmptyExpression (invoke_pd_type), pd_type))
					return null;

				if (RootContext.Version == LanguageVersion.ISO_1)
					return null;
			}

			Type invoke_mb_retval = ((MethodInfo) invoke_mb).ReturnType;
			Type mb_retval = ((MethodInfo) mb).ReturnType;
			if (invoke_mb_retval == mb_retval)
				return mb;

			if (!Convert.ImplicitReferenceConversionExists (new EmptyExpression (mb_retval), invoke_mb_retval))
				return null;

			if (RootContext.Version == LanguageVersion.ISO_1) 
				return null;

			return mb;
		}

		// <summary>
		//  Verifies whether the invocation arguments are compatible with the
		//  delegate's target method
		// </summary>
		public static bool VerifyApplicability (EmitContext ec, Type delegate_type,
							ArrayList args, Location loc)
		{
			int arg_count;

			if (args == null)
				arg_count = 0;
			else
				arg_count = args.Count;

			Expression ml = Expression.MemberLookup (
				ec.ContainerType, delegate_type, "Invoke", loc);

			MethodGroupExpr me = ml as MethodGroupExpr;
			if (me == null) {
				Report.Error (-100, loc, "Internal error: could not find Invoke method!" + delegate_type);
				return false;
			}
			
			MethodBase mb = me.Methods [0];
			ParameterData pd = TypeManager.GetParameterData (mb);

			int pd_count = pd.Count;

			bool params_method = pd.HasParams;
			bool is_params_applicable = false;
			bool is_applicable = Invocation.IsApplicable (ec, me, args, arg_count, ref mb);

			if (!is_applicable && params_method &&
			    Invocation.IsParamsMethodApplicable (ec, me, args, arg_count, ref mb))
				is_applicable = is_params_applicable = true;

			if (!is_applicable && !params_method && arg_count != pd_count) {
				Report.Error (1593, loc, "Delegate `{0}' does not take `{1}' arguments",
					TypeManager.CSharpName (delegate_type), arg_count.ToString ());
				return false;
			}

			return Invocation.VerifyArgumentsCompat (
					ec, args, arg_count, mb, 
					is_params_applicable || (!is_applicable && params_method),
					delegate_type, false, loc);
		}
		
		/// <summary>
		///  Verifies whether the delegate in question is compatible with this one in
		///  order to determine if instantiation from the same is possible.
		/// </summary>
		public static bool VerifyDelegate (EmitContext ec, Type delegate_type, Location loc)
		{
			Expression ml = Expression.MemberLookup (
				ec.ContainerType, delegate_type, "Invoke", loc);
			
			if (!(ml is MethodGroupExpr)) {
				Report.Error (-100, loc, "Internal error: could not find Invoke method!");
				return false;
			}
			
			MethodBase mb = ((MethodGroupExpr) ml).Methods [0];
			ParameterData pd = TypeManager.GetParameterData (mb);

			Expression probe_ml = Expression.MemberLookup (
				ec.ContainerType, delegate_type, "Invoke", loc);
			
			if (!(probe_ml is MethodGroupExpr)) {
				Report.Error (-100, loc, "Internal error: could not find Invoke method!");
				return false;
			}
			
			MethodBase probe_mb = ((MethodGroupExpr) probe_ml).Methods [0];
			ParameterData probe_pd = TypeManager.GetParameterData (probe_mb);
			
			if (((MethodInfo) mb).ReturnType != ((MethodInfo) probe_mb).ReturnType)
				return false;

			if (pd.Count != probe_pd.Count)
				return false;

			for (int i = pd.Count; i > 0; ) {
				i--;

				if (pd.ParameterType (i) != probe_pd.ParameterType (i) ||
				    pd.ParameterModifier (i) != probe_pd.ParameterModifier (i))
					return false;
			}
			
			return true;
		}
		
		public static string FullDelegateDesc (Type del_type, MethodBase mb, ParameterData pd)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (TypeManager.CSharpName (((MethodInfo) mb).ReturnType));
			sb.Append (" ");
			sb.Append (TypeManager.CSharpName (del_type));
			sb.Append (pd.GetSignatureForError ());
			return sb.ToString ();			
		}
		
		// Hack around System.Reflection as found everywhere else
		public override MemberList FindMembers (MemberTypes mt, BindingFlags bf,
							MemberFilter filter, object criteria)
		{
			ArrayList members = new ArrayList (2);

			if ((mt & MemberTypes.Constructor) != 0) {
				if (ConstructorBuilder != null && filter (ConstructorBuilder, criteria))
					members.Add (ConstructorBuilder);
			}

			if ((mt & MemberTypes.Method) != 0) {
				if (InvokeBuilder != null)
				if (filter (InvokeBuilder, criteria))
					members.Add (InvokeBuilder);

				if (BeginInvokeBuilder != null)
				if (filter (BeginInvokeBuilder, criteria))
					members.Add (BeginInvokeBuilder);

				if (EndInvokeBuilder != null)
				if (filter (EndInvokeBuilder, criteria))
					members.Add (EndInvokeBuilder);
			}

			return new MemberList (members);
		}

		public override MemberCache MemberCache {
			get {
				return null;
			}
		}

		public Expression InstanceExpression {
			get {
				return instance_expr;
			}
			set {
				instance_expr = value;
			}
		}

		public MethodBase TargetMethod {
			get {
				return delegate_method;
			}
			set {
				delegate_method = value;
			}
		}

		public Type TargetReturnType {
			get {
				return ret_type;
			}
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Delegate;
			}
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "T:"; }
		}
	}

	//
	// Base class for `NewDelegate' and `ImplicitDelegateCreation'
	//
	public abstract class DelegateCreation : Expression {
		protected MethodBase constructor_method;
		protected MethodBase delegate_method;
		protected MethodGroupExpr method_group;
		protected Expression delegate_instance_expression;

		protected DelegateCreation () {}

		public static void Error_NoMatchingMethodForDelegate (EmitContext ec, MethodGroupExpr mg, Type type, Location loc)
		{
			string method_desc;
			MethodBase found_method = mg.Methods [0];

			if (mg.Methods.Length > 1)
				method_desc = found_method.Name;
			else
				method_desc = Invocation.FullMethodDesc (found_method);

			Expression invoke_method = Expression.MemberLookup (
				ec.ContainerType, type, "Invoke", MemberTypes.Method,
				Expression.AllBindingFlags, loc);
			MethodInfo method = ((MethodGroupExpr) invoke_method).Methods [0] as MethodInfo;

			ParameterData param = TypeManager.GetParameterData (method);
			string delegate_desc = Delegate.FullDelegateDesc (type, method, param);

#if GMCS_SOURCE
			if (!mg.HasTypeArguments &&
			    !TypeManager.InferTypeArguments (param, ref found_method)) {
				Report.Error (411, loc, "The type arguments for " +
					      "method `{0}' cannot be inferred from " +
					      "the usage. Try specifying the type " +
					      "arguments explicitly.", method_desc);
				return;
			}
#endif
			Report.SymbolRelatedToPreviousError (found_method);

			if (RootContext.Version == LanguageVersion.ISO_1) {
				Report.Error (410, loc, "The method `{0}' parameters and return type must be same as delegate `{1}' parameters and return type",
					method_desc, delegate_desc);
				return;
			}

			Type delegateType = method.ReturnType;
			Type methodType = ((MethodInfo) found_method).ReturnType;
			if (delegateType != methodType &&
				!Convert.ImplicitReferenceConversionExists (new EmptyExpression (methodType), delegateType)) {
				Report.Error (407, loc, "`{0}' has the wrong return type to match the delegate `{1}'", method_desc, delegate_desc);
			} else {
				Report.Error (123, loc, "The method `{0}' parameters do not match delegate `{1}' parameters",
					TypeManager.CSharpSignature (found_method), delegate_desc);
			}
		}
		
		public override void Emit (EmitContext ec)
		{
			if (delegate_instance_expression == null || delegate_method.IsStatic)
				ec.ig.Emit (OpCodes.Ldnull);
			else
				delegate_instance_expression.Emit (ec);
			
			if (delegate_method.IsVirtual && !method_group.IsBase) {
				ec.ig.Emit (OpCodes.Dup);
				ec.ig.Emit (OpCodes.Ldvirtftn, (MethodInfo) delegate_method);
			} else
				ec.ig.Emit (OpCodes.Ldftn, (MethodInfo) delegate_method);
			ec.ig.Emit (OpCodes.Newobj, (ConstructorInfo) constructor_method);
		}

		protected bool ResolveConstructorMethod (EmitContext ec)
		{
			Expression ml = Expression.MemberLookupFinal(ec, 
				null, type, ".ctor", MemberTypes.Constructor, AllBindingFlags | BindingFlags.DeclaredOnly, loc);

			if (!(ml is MethodGroupExpr)) {
				Report.Error (-100, loc, "Internal error: Could not find delegate constructor!");
				return false;
			}

			constructor_method = ((MethodGroupExpr) ml).Methods [0];
			return true;
		}

		public static MethodBase ImplicitStandardConversionExists (MethodGroupExpr mg, Type targetType)
		{
			foreach (MethodInfo mi in mg.Methods){
				MethodBase mb = Delegate.VerifyMethod (mg.DeclaringType, targetType, mg, mi, Location.Null);
				if (mb != null)
					return mb;
			}
			return null;
		}

		protected Expression ResolveMethodGroupExpr (EmitContext ec, MethodGroupExpr mg)
		{
			delegate_method = ImplicitStandardConversionExists (mg, type);

			if (delegate_method == null) {
				Error_NoMatchingMethodForDelegate (ec, mg, type, loc);
				return null;
			}
			
			//
			// Check safe/unsafe of the delegate
			//
			if (!ec.InUnsafe){
				ParameterData param = TypeManager.GetParameterData (delegate_method);
				int count = param.Count;
				
				for (int i = 0; i < count; i++){
					if (param.ParameterType (i).IsPointer){
						Expression.UnsafeError (loc);
						return null;
					}
				}
			}
						
			//TODO: implement caching when performance will be low
			IMethodData md = TypeManager.GetMethod (delegate_method);
			if (md == null) {
				if (System.Attribute.GetCustomAttribute (delegate_method, TypeManager.conditional_attribute_type) != null) {
					Report.SymbolRelatedToPreviousError (delegate_method);
					Report.Error (1618, loc, "Cannot create delegate with `{0}' because it has a Conditional attribute", TypeManager.CSharpSignature (delegate_method));
					return null;
				}
			} else {
				md.SetMemberIsUsed ();
				if (md.OptAttributes != null && md.OptAttributes.Search (TypeManager.conditional_attribute_type) != null) {
					Report.SymbolRelatedToPreviousError (delegate_method);
					Report.Error (1618, loc, "Cannot create delegate with `{0}' because it has a Conditional attribute", TypeManager.CSharpSignature (delegate_method));
					return null;
				}
			}
			
			if (mg.InstanceExpression != null)
				delegate_instance_expression = mg.InstanceExpression.Resolve (ec);
			else if (ec.IsStatic) {
				if (!delegate_method.IsStatic) {
					Report.Error (120, loc, "`{0}': An object reference is required for the nonstatic field, method or property",
						      TypeManager.CSharpSignature (delegate_method));
					return null;
				}
				delegate_instance_expression = null;
			} else
				delegate_instance_expression = ec.GetThis (loc);

			if (delegate_instance_expression != null && delegate_instance_expression.Type.IsValueType)
				delegate_instance_expression = new BoxedCast (
					delegate_instance_expression, TypeManager.object_type);

			method_group = mg;
			eclass = ExprClass.Value;
			return this;
		}
	}

	//
	// Created from the conversion code
	//
	public class ImplicitDelegateCreation : DelegateCreation {

		ImplicitDelegateCreation (Type t, Location l)
		{
			type = t;
			loc = l;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			return this;
		}

		static public Expression Create (EmitContext ec, MethodGroupExpr mge,
						 Type target_type, Location loc)
		{
			ImplicitDelegateCreation d = new ImplicitDelegateCreation (target_type, loc);
			if (!d.ResolveConstructorMethod (ec))
				return null;

			return d.ResolveMethodGroupExpr (ec, mge);
		}
	}
	
	//
	// A delegate-creation-expression, invoked from the `New' class 
	//
	public class NewDelegate : DelegateCreation {
		public ArrayList Arguments;

		//
		// This constructor is invoked from the `New' expression
		//
		public NewDelegate (Type type, ArrayList Arguments, Location loc)
		{
			this.type = type;
			this.Arguments = Arguments;
			this.loc  = loc; 
		}

		public override Expression DoResolve (EmitContext ec)
		{
			if (Arguments == null) {
				Invocation.Error_WrongNumArguments (loc, GetSignatureForError (), 0);
				return null;
			}

			if (!ResolveConstructorMethod (ec))
				return null;

			Argument a = (Argument) Arguments [0];
			
			if (!a.ResolveMethodGroup (ec))
				return null;
			
			Expression e = a.Expr;

			if (e is AnonymousMethodExpression && RootContext.Version != LanguageVersion.ISO_1)
				return ((AnonymousMethodExpression) e).Compatible (ec, type);

			MethodGroupExpr mg = e as MethodGroupExpr;
			if (mg != null) {
				if (TypeManager.IsNullableType (mg.DeclaringType)) {
					Report.Error (1728, loc, "Cannot use method `{0}' as delegate creation expression because it is member of Nullable type",
						mg.GetSignatureForError ());
					return null;
				}

				return ResolveMethodGroupExpr (ec, mg);
			}

			if (!TypeManager.IsDelegateType (e.Type)) {
				Report.Error (149, loc, "Method name expected");
				return null;
			}

			method_group = Expression.MemberLookup (
				ec.ContainerType, type, "Invoke", MemberTypes.Method,
				Expression.AllBindingFlags, loc) as MethodGroupExpr;

			if (method_group == null) {
				Report.Error (-200, loc, "Internal error ! Could not find Invoke method!");
				return null;
			}

			// This is what MS' compiler reports. We could always choose
			// to be more verbose and actually give delegate-level specifics
			if (!Delegate.VerifyDelegate (ec, type, loc)) {
				Report.Error (29, loc, "Cannot implicitly convert type '" + e.Type + "' " +
					      "to type '" + type + "'");
				return null;
			}
				
			delegate_instance_expression = e;
			delegate_method = method_group.Methods [0];

			eclass = ExprClass.Value;
			return this;
		}
	}

	public class DelegateInvocation : ExpressionStatement {

		public Expression InstanceExpr;
		public ArrayList  Arguments;

		MethodBase method;
		
		public DelegateInvocation (Expression instance_expr, ArrayList args, Location loc)
		{
			this.InstanceExpr = instance_expr;
			this.Arguments = args;
			this.loc = loc;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			if (InstanceExpr is EventExpr) {
				
				EventInfo ei = ((EventExpr) InstanceExpr).EventInfo;
				
				Expression ml = MemberLookup (
					ec.ContainerType, ec.ContainerType, ei.Name,
					MemberTypes.Event, AllBindingFlags | BindingFlags.DeclaredOnly, loc);

				if (ml == null) {
				        //
					// If this is the case, then the Event does not belong 
					// to this Type and so, according to the spec
					// cannot be accessed directly
					//
					// Note that target will not appear as an EventExpr
					// in the case it is being referenced within the same type container;
					// it will appear as a FieldExpr in that case.
					//
					
					Assign.error70 (ei, loc);
					return null;
				}
			}
			
			
			Type del_type = InstanceExpr.Type;
			if (del_type == null)
				return null;
			
			if (Arguments != null){
				foreach (Argument a in Arguments){
					if (!a.Resolve (ec, loc))
						return null;
				}
			}
			
			if (!Delegate.VerifyApplicability (ec, del_type, Arguments, loc))
				return null;

			Expression lookup = Expression.MemberLookup (ec.ContainerType, del_type, "Invoke", loc);
			if (!(lookup is MethodGroupExpr)) {
				Report.Error (-100, loc, "Internal error: could not find Invoke method!");
				return null;
			}
			
			method = ((MethodGroupExpr) lookup).Methods [0];
			type = ((MethodInfo) method).ReturnType;
			eclass = ExprClass.Value;
			
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			//
			// Invocation on delegates call the virtual Invoke member
			// so we are always `instance' calls
			//
			Invocation.EmitCall (ec, false, false, InstanceExpr, method, Arguments, loc);
		}

		public override void EmitStatement (EmitContext ec)
		{
			Emit (ec);
			// 
			// Pop the return value if there is one
			//
			if (method is MethodInfo){
				Type ret = ((MethodInfo)method).ReturnType;
				if (TypeManager.TypeToCoreType (ret) != TypeManager.void_type)
					ec.ig.Emit (OpCodes.Pop);
			}
		}

	}
}
