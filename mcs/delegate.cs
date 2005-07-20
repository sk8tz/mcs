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
		
		Type [] param_types;
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

 		public Delegate (NamespaceEntry ns, TypeContainer parent, Expression type,
				 int mod_flags, MemberName name, Parameters param_list,
				 Attributes attrs, Location l)
			: base (ns, parent, name, attrs, l)

		{
			this.ReturnType = type;
			ModFlags        = Modifiers.Check (AllowedModifiers, mod_flags,
							   IsTopLevel ? Modifiers.INTERNAL :
							   Modifiers.PRIVATE, l);
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

			ec = new EmitContext (this, this, Location, null, null, ModFlags, false);

			if (TypeManager.multicast_delegate_type == null && !RootContext.StdLib) {
				Namespace system = Namespace.LookupNamespace ("System", true);
				TypeExpr expr = system.Lookup (this, "MulticastDelegate", Location) as TypeExpr;
				TypeManager.multicast_delegate_type = expr.ResolveType (ec);
			}

			if (TypeManager.multicast_delegate_type == null)
				throw new InternalErrorException ("System.MulticastDelegate unresolved");

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

			TypeManager.AddUserType (Name, this);

			return TypeBuilder;
		}

 		public override bool Define ()
		{
			MethodAttributes mattr;
			int i;

			if (ec == null)
				throw new InternalErrorException ("Define called before DefineType?");

			// FIXME: POSSIBLY make this static, as it is always constant
			//
			Type [] const_arg_types = new Type [2];
			const_arg_types [0] = TypeManager.object_type;
			const_arg_types [1] = TypeManager.intptr_type;

			mattr = MethodAttributes.RTSpecialName | MethodAttributes.SpecialName |
				MethodAttributes.HideBySig | MethodAttributes.Public;

			ConstructorBuilder = TypeBuilder.DefineConstructor (mattr,
									    CallingConventions.Standard,
									    const_arg_types);

			ConstructorBuilder.DefineParameter (1, ParameterAttributes.None, "object");
			ConstructorBuilder.DefineParameter (2, ParameterAttributes.None, "method");
			//
			// HACK because System.Reflection.Emit is lame
			//
			Parameter [] fixed_pars = new Parameter [2];
			fixed_pars [0] = new Parameter (TypeManager.system_object_expr, "object",
							Parameter.Modifier.NONE, null, Location);
			fixed_pars [1] = new Parameter (TypeManager.system_intptr_expr, "method", 
							Parameter.Modifier.NONE, null, Location);
			Parameters const_parameters = new Parameters (fixed_pars, null);
			
			TypeManager.RegisterMethod (
				ConstructorBuilder,
				new InternalParameters (const_arg_types, const_parameters),
				const_arg_types);
				
			
			ConstructorBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			//
			// Here the various methods like Invoke, BeginInvoke etc are defined
			//
			// First, call the `out of band' special method for
			// defining recursively any types we need:
			
 			param_types = Parameters.GetParameterInfo (ec);
			if (param_types == null)
				return false;

			//
			// Invoke method
			//

			// Check accessibility
			foreach (Type partype in param_types){
				if (!Parent.AsAccessible (partype, ModFlags)) {
					Report.Error (59, Location,
						      "Inconsistent accessibility: parameter type `" +
						      TypeManager.CSharpName (partype) + "' is less " +
						      "accessible than delegate `" + Name + "'");
					return false;
				}
				if (partype.IsPointer && !UnsafeOK (Parent))
					return false;
			}
			
 			ReturnType = ReturnType.ResolveAsTypeTerminal (ec, false);
                        if (ReturnType == null)
                            return false;
                        
   			ret_type = ReturnType.Type;
			if (ret_type == null)
				return false;

			if (!Parent.AsAccessible (ret_type, ModFlags)) {
				Report.Error (58, Location,
					      "Inconsistent accessibility: return type `" +
					      TypeManager.CSharpName (ret_type) + "' is less " +
					      "accessible than delegate `" + Name + "'");
				return false;
			}

			if (ret_type.IsPointer && !UnsafeOK (Parent))
				return false;

			if (RootContext.StdLib && (ret_type == TypeManager.arg_iterator_type || ret_type == TypeManager.typed_reference_type)) {
				Method.Error1599 (Location, ret_type);
				return false;
			}

			//
			// We don't have to check any others because they are all
			// guaranteed to be accessible - they are standard types.
			//
			
  			CallingConventions cc = Parameters.GetCallingConvention ();

 			mattr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;

 			InvokeBuilder = TypeBuilder.DefineMethod ("Invoke", 
 								  mattr,		     
 								  cc,
 								  ret_type,		     
 								  param_types);

			//
			// Define parameters, and count out/ref parameters
			//
			int out_params = 0;
			i = 0;
			if (Parameters.FixedParameters != null){
				int top = Parameters.FixedParameters.Length;
				Parameter p;
				
				for (; i < top; i++) {
					p = Parameters.FixedParameters [i];
					p.DefineParameter (ec, InvokeBuilder, null, i + 1);

					if ((p.ModFlags & Parameter.Modifier.ISBYREF) != 0)
						out_params++;
				}
			}
			if (Parameters.ArrayParameter != null){
				Parameter p = Parameters.ArrayParameter;

				if (TypeManager.param_array_type == null && !RootContext.StdLib) {
					Namespace system = Namespace.LookupNamespace ("System", true);
					TypeExpr expr = system.Lookup (this, "ParamArrayAttribute", Location) as TypeExpr;
					TypeManager.param_array_type = expr.ResolveType (ec);
				}

				if (TypeManager.cons_param_array_attribute == null) {
					Type [] void_arg = { };
					TypeManager.cons_param_array_attribute = TypeManager.GetConstructor (
						TypeManager.param_array_type, void_arg);
				}

				ParameterBuilder pb = InvokeBuilder.DefineParameter (
					i + 1, Parameters.ArrayParameter.Attributes,Parameters.ArrayParameter.Name);
				
				pb.SetCustomAttribute (
					new CustomAttributeBuilder (TypeManager.cons_param_array_attribute, new object [0]));
			}
			
			InvokeBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			TypeManager.RegisterMethod (InvokeBuilder,
						    new InternalParameters (param_types, Parameters),
						    param_types);

			//
			// BeginInvoke
			//
			int params_num = param_types.Length;
			Type [] async_param_types = new Type [params_num + 2];

			param_types.CopyTo (async_param_types, 0);

			async_param_types [params_num] = TypeManager.asynccallback_type;
			async_param_types [params_num + 1] = TypeManager.object_type;

			mattr = MethodAttributes.Public | MethodAttributes.HideBySig |
				MethodAttributes.Virtual | MethodAttributes.NewSlot;
			
			BeginInvokeBuilder = TypeBuilder.DefineMethod ("BeginInvoke",
								       mattr,
								       cc,
								       TypeManager.iasyncresult_type,
								       async_param_types);

			i = 0;
			if (Parameters.FixedParameters != null){
				int top = Parameters.FixedParameters.Length;
				Parameter p;
				
				for (i = 0 ; i < top; i++) {
					p = Parameters.FixedParameters [i];

					p.DefineParameter (ec, BeginInvokeBuilder, null, i + 1);
				}
			}
			if (Parameters.ArrayParameter != null){
				Parameter p = Parameters.ArrayParameter;
				p.DefineParameter (ec, BeginInvokeBuilder, null, i + 1);

				i++;
			}

			BeginInvokeBuilder.DefineParameter (i + 1, ParameterAttributes.None, "callback");
			BeginInvokeBuilder.DefineParameter (i + 2, ParameterAttributes.None, "object");
			
			BeginInvokeBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			Parameter [] async_params = new Parameter [params_num + 2];
			int n = 0;
			if (Parameters.FixedParameters != null){
				Parameters.FixedParameters.CopyTo (async_params, 0);
				n = Parameters.FixedParameters.Length;
			}
			if (Parameters.ArrayParameter != null)
				async_params [n] = Parameters.ArrayParameter;
			
			async_params [params_num] = new Parameter (
				TypeManager.system_asynccallback_expr, "callback",
								   Parameter.Modifier.NONE, null, Location);
			async_params [params_num + 1] = new Parameter (
				TypeManager.system_object_expr, "object",
								   Parameter.Modifier.NONE, null, Location);

			Parameters async_parameters = new Parameters (async_params, null);

			TypeManager.RegisterMethod (BeginInvokeBuilder,
						    new InternalParameters (async_parameters.GetParameterInfo (ec), async_parameters),
						    async_param_types);

			//
			// EndInvoke is a bit more interesting, all the parameters labeled as
			// out or ref have to be duplicated here.
			//
			
			Type [] end_param_types = new Type [out_params + 1];
			Parameter [] end_params = new Parameter [out_params + 1];
			int param = 0; 
			if (out_params > 0){
				int top = Parameters.FixedParameters.Length;
				for (i = 0; i < top; i++){
					Parameter p = Parameters.FixedParameters [i];
					if ((p.ModFlags & Parameter.Modifier.ISBYREF) == 0)
						continue;

					end_param_types [param] = param_types [i];
					end_params [param] = p;
					param++;
				}
			}
			end_param_types [out_params] = TypeManager.iasyncresult_type;
			end_params [out_params] = new Parameter (TypeManager.system_iasyncresult_expr, "result", Parameter.Modifier.NONE, null, Location);

			//
			// Create method, define parameters, register parameters with type system
			//
			EndInvokeBuilder = TypeBuilder.DefineMethod ("EndInvoke", mattr, cc, ret_type, end_param_types);
			EndInvokeBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			//
			// EndInvoke: Label the parameters
			//
			EndInvokeBuilder.DefineParameter (out_params + 1, ParameterAttributes.None, "result");
			for (i = 0; i < end_params.Length-1; i++){
				EndInvokeBuilder.DefineParameter (i + 1, end_params [i].Attributes, end_params [i].Name);
			}

			Parameters end_parameters = new Parameters (end_params, null);

			TypeManager.RegisterMethod (
				EndInvokeBuilder,
				new InternalParameters (end_parameters.GetParameterInfo (ec), end_parameters),
				end_param_types);

			return true;
		}

		public override void Emit ()
		{
			if (OptAttributes != null) {
				Parameters.LabelParameters (ec, InvokeBuilder);
				OptAttributes.Emit (ec, this);
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
		protected override bool VerifyClsCompliance (DeclSpace ds)
		{
			if (!base.VerifyClsCompliance (ds)) {
				return false;
			}

			AttributeTester.AreParametersCompliant (Parameters.FixedParameters, Location);

			if (!AttributeTester.IsClsCompliant (ReturnType.Type)) {
				Report.Error (3002, Location, "Return type of `{0}' is not CLS-compliant", GetSignatureForError ());
			}
			return true;
		}

		//
		// Returns the MethodBase for "Invoke" from a delegate type, this is used
		// to extract the signature of a delegate.
		//
		public static MethodInfo GetInvokeMethod (EmitContext ec, Type delegate_type, Location loc)
		{
			Expression ml = Expression.MemberLookup (
				ec, delegate_type, "Invoke", loc);

			if (!(ml is MethodGroupExpr)) {
				Report.Error (-100, loc, "Internal error: could not find Invoke method!");
				return null;
			}

			return (MethodInfo) (((MethodGroupExpr) ml).Methods [0]);
		}
		
		/// <summary>
		///  Verifies whether the method in question is compatible with the delegate
		///  Returns the method itself if okay and null if not.
		/// </summary>
		public static MethodBase VerifyMethod (EmitContext ec, Type delegate_type, MethodBase mb,
						       Location loc)
		{
			ParameterData pd = TypeManager.GetParameterData (mb);

			int pd_count = pd.Count;

			MethodBase invoke_mb = GetInvokeMethod (ec, delegate_type, loc);
			if (invoke_mb == null)
				return null;

			ParameterData invoke_pd = TypeManager.GetParameterData (invoke_mb);

			if (invoke_pd.Count != pd_count)
				return null;

			for (int i = pd_count; i > 0; ) {
				i--;

				Type invoke_pd_type = invoke_pd.ParameterType (i);
				Type pd_type = pd.ParameterType (i);
				Parameter.Modifier invoke_pd_type_mod = invoke_pd.ParameterModifier (i);
				Parameter.Modifier pd_type_mod = pd.ParameterModifier (i);

				if (invoke_pd_type == pd_type &&
				    invoke_pd_type_mod == pd_type_mod)
					continue;
				
				if (invoke_pd_type.IsSubclassOf (pd_type) && 
						invoke_pd_type_mod == pd_type_mod)
					if (RootContext.Version == LanguageVersion.ISO_1) {
						Report.FeatureIsNotStandardized (loc, "contravariance");
						return null;
					} else
						continue;
					
				return null;
			}

			Type invoke_mb_retval = ((MethodInfo) invoke_mb).ReturnType;
			Type mb_retval = ((MethodInfo) mb).ReturnType;
			if (invoke_mb_retval == mb_retval)
				return mb;
			
			if (mb_retval.IsSubclassOf (invoke_mb_retval))
				if (RootContext.Version == LanguageVersion.ISO_1) {
					Report.FeatureIsNotStandardized (loc, "covariance");
					return null;
				}
				else
					return mb;
			
			return null;
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
				ec, delegate_type, "Invoke", loc);

			MethodGroupExpr me = ml as MethodGroupExpr;
			if (me == null) {
				Report.Error (-100, loc, "Internal error: could not find Invoke method!" + delegate_type);
				return false;
			}
			
			MethodBase mb = me.Methods [0];
			ParameterData pd = TypeManager.GetParameterData (mb);

			int pd_count = pd.Count;

			bool params_method = (pd_count != 0) &&
				(pd.ParameterModifier (pd_count - 1) == Parameter.Modifier.PARAMS);

			bool is_params_applicable = false;
			bool is_applicable = Invocation.IsApplicable (ec, me, args, arg_count, ref mb);

			if (!is_applicable && params_method &&
			    Invocation.IsParamsMethodApplicable (ec, me, args, arg_count, ref mb))
				is_applicable = is_params_applicable = true;

			if (!is_applicable && !params_method && arg_count != pd_count) {
				Report.Error (1593, loc, "Delegate `{0}' does not take `{1}' arguments",
					TypeManager.CSharpName (delegate_type), arg_count);
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
		public static bool VerifyDelegate (EmitContext ec, Type delegate_type, Type probe_type, Location loc)
		{
			Expression ml = Expression.MemberLookup (
				ec, delegate_type, "Invoke", loc);
			
			if (!(ml is MethodGroupExpr)) {
				Report.Error (-100, loc, "Internal error: could not find Invoke method!");
				return false;
			}
			
			MethodBase mb = ((MethodGroupExpr) ml).Methods [0];
			ParameterData pd = TypeManager.GetParameterData (mb);

			Expression probe_ml = Expression.MemberLookup (
				ec, delegate_type, "Invoke", loc);
			
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
			ArrayList members = new ArrayList ();

			if ((mt & MemberTypes.Method) != 0) {
				if (ConstructorBuilder != null)
				if (filter (ConstructorBuilder, criteria))
					members.Add (ConstructorBuilder);

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

		public Type [] ParameterTypes {
			get {
				return param_types;
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

		protected override void VerifyObsoleteAttribute()
		{
			CheckUsageOfObsoleteAttribute (ret_type);

			foreach (Type type in param_types) {
				CheckUsageOfObsoleteAttribute (type);
			}
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

		public DelegateCreation () {}

		public static void Error_NoMatchingMethodForDelegate (EmitContext ec, MethodGroupExpr mg, Type type, Location loc)
		{
			string method_desc;
			MethodInfo found_method = (MethodInfo)mg.Methods [0];
			
			if (mg.Methods.Length > 1)
				method_desc = found_method.Name;
			else
				method_desc = Invocation.FullMethodDesc (found_method);

			Expression invoke_method = Expression.MemberLookup (
				ec, type, "Invoke", MemberTypes.Method,
				Expression.AllBindingFlags, loc);
			MethodInfo method = ((MethodGroupExpr) invoke_method).Methods [0] as MethodInfo;

			ParameterData param = TypeManager.GetParameterData (method);
			string delegate_desc = Delegate.FullDelegateDesc (type, method, param);

			if (method.ReturnType != found_method.ReturnType) {
				Report.Error (407, loc, "`{0}' has the wrong return type to match the delegate `{1}'", method_desc, delegate_desc);
			} else {
				Report.Error (123, loc, "Method `{0}' does not match delegate `{1}'", method_desc, delegate_desc);
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
			Expression ml = Expression.MemberLookup (
				ec, type, ".ctor", loc);

			if (!(ml is MethodGroupExpr)) {
				Report.Error (-100, loc, "Internal error: Could not find delegate constructor!");
				return false;
			}

			constructor_method = ((MethodGroupExpr) ml).Methods [0];
			return true;
		}

		protected Expression ResolveMethodGroupExpr (EmitContext ec, MethodGroupExpr mg,
							     bool check_only)
		{
			foreach (MethodInfo mi in mg.Methods){
				delegate_method  = Delegate.VerifyMethod (ec, type, mi, loc);
				
				if (delegate_method != null)
					break;
			}
			
			if (delegate_method == null) {
				if (!check_only)
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
					Report.Error (1618, loc, "Cannot create delegate with `{0}' because it has a Conditional attribute", TypeManager.CSharpSignature (delegate_method));
				}
			} else {
				md.SetMemberIsUsed ();
				if (md.OptAttributes != null && md.OptAttributes.Search (TypeManager.conditional_attribute_type, ec) != null) {
					Report.Error (1618, loc, "Cannot create delegate with `{0}' because it has a Conditional attribute", TypeManager.CSharpSignature (delegate_method));
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
				delegate_instance_expression = new BoxedCast (delegate_instance_expression);
			
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
						 Type target_type, bool check_only, Location loc)
		{
			ImplicitDelegateCreation d = new ImplicitDelegateCreation (target_type, loc);
			if (d.ResolveConstructorMethod (ec))
				return d.ResolveMethodGroupExpr (ec, mge, check_only);
			else
				return null;
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
			if (Arguments == null || Arguments.Count != 1) {
				Report.Error (149, loc,
					      "Method name expected");
				return null;
			}

			if (!ResolveConstructorMethod (ec))
				return null;

			Argument a = (Argument) Arguments [0];

			if (!a.ResolveMethodGroup (ec, loc))
				return null;
			
			Expression e = a.Expr;

			if (e is AnonymousMethod && RootContext.Version != LanguageVersion.ISO_1)
				return ((AnonymousMethod) e).Compatible (ec, type, false);

			MethodGroupExpr mg = e as MethodGroupExpr;
			if (mg != null)
				return ResolveMethodGroupExpr (ec, mg, false);

			Type e_type = e.Type;

			if (!TypeManager.IsDelegateType (e_type)) {
				Report.Error (149, loc, "Method name expected");
				return null;
			}

			method_group = Expression.MemberLookup (
				ec, type, "Invoke", MemberTypes.Method,
				Expression.AllBindingFlags, loc) as MethodGroupExpr;

			if (method_group == null) {
				Report.Error (-200, loc, "Internal error ! Could not find Invoke method!");
				return null;
			}

			// This is what MS' compiler reports. We could always choose
			// to be more verbose and actually give delegate-level specifics			
			if (!Delegate.VerifyDelegate (ec, type, e_type, loc)) {
				Report.Error (29, loc, "Cannot implicitly convert type '" + e_type + "' " +
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
					ec, ec.ContainerType, ei.Name,
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

			Expression lookup = Expression.MemberLookup (ec, del_type, "Invoke", loc);
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
				if (((MethodInfo) method).ReturnType != TypeManager.void_type)
					ec.ig.Emit (OpCodes.Pop);
			}
		}

	}
}
