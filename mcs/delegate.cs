//
// delegate.cs: Delegate Handler
//
// Author: Ravi Pratap (ravi@ximian.com)
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

namespace CIR {
	
	public class Delegate {

		public readonly string Name;
		public readonly string ReturnType;
		public int             mod_flags;
		public Parameters      Parameters;
		public Attributes      OptAttributes;
		public TypeBuilder     TypeBuilder;

		public ConstructorBuilder ConstructorBuilder;
		public MethodBuilder      InvokeBuilder;
		public MethodBuilder      BeginInvokeBuilder;
		public MethodBuilder      EndInvokeBuilder;
		
		public readonly RootContext RootContext;

		Type [] param_types;
		Type ret_type;
		
		Expression instance_expr;
		MethodBase delegate_method;
	
		Location loc;

		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Delegate (RootContext rc, string type, int mod_flags, string name, Parameters param_list,
				 Attributes attrs, Location loc)
		{
			this.RootContext = rc;
			this.Name       = name;
			this.ReturnType = type;
			this.mod_flags  = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PUBLIC);
			Parameters      = param_list;
			OptAttributes   = attrs;
			this.loc        = loc;
		}

		public void DefineDelegate (object parent_builder)
		{
			TypeAttributes attr;
			
			if (parent_builder is ModuleBuilder) {
				ModuleBuilder builder = (ModuleBuilder) parent_builder;
				
				attr = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed;

				TypeBuilder = builder.DefineType (Name, attr, TypeManager.delegate_type);
								  
			} else {
				TypeBuilder builder = (TypeBuilder) parent_builder;
				
				attr = TypeAttributes.NestedPublic | TypeAttributes.Class | TypeAttributes.Sealed;

				TypeBuilder = builder.DefineNestedType (Name, attr, TypeManager.delegate_type);

			}

			RootContext.TypeManager.AddDelegateType (Name, TypeBuilder, this);
		}

		public void Populate (TypeContainer parent)
		{

			MethodAttributes mattr;
			
			Type [] const_arg_types = new Type [2];

			const_arg_types [0] = TypeManager.object_type;
			const_arg_types [1] = TypeManager.intptr_type;

			mattr = MethodAttributes.RTSpecialName | MethodAttributes.SpecialName |
				MethodAttributes.HideBySig | MethodAttributes.Public;

			ConstructorBuilder = TypeBuilder.DefineConstructor (mattr,
									    CallingConventions.Standard,
									    const_arg_types);
			
			ConstructorBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);
			
			// Here the various methods like Invoke, BeginInvoke etc are defined

 			param_types = Parameters.GetParameterInfo (parent);
  			ret_type = parent.LookupType (ReturnType, false);
  			CallingConventions cc = Parameters.GetCallingConvention ();

 			mattr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;

 			InvokeBuilder = TypeBuilder.DefineMethod ("Invoke", 
 								  mattr,		     
 								  cc,
 								  ret_type,		     
 								  param_types);
			
			InvokeBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			int params_num = param_types.Length;
			Type [] async_param_types = new Type [params_num + 2];

			param_types.CopyTo (async_param_types, 0);

			async_param_types [params_num] = TypeManager.asynccallback_type;
			async_param_types [params_num + 1] = TypeManager.object_type;

			mattr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual |
				MethodAttributes.NewSlot;
			
			BeginInvokeBuilder = TypeBuilder.DefineMethod ("BeginInvoke",
								       mattr,
								       cc,
								       TypeManager.iasyncresult_type,
								       async_param_types);
			
			BeginInvokeBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			Type [] end_param_types = new Type [1];

			end_param_types [0] = TypeManager.iasyncresult_type;
			
			EndInvokeBuilder = TypeBuilder.DefineMethod ("EndInvoke",
								     mattr,
								     cc,
								     ret_type,
								     end_param_types);

			EndInvokeBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);
			
		}

		public MethodBase VerifyMethod (MethodBase mb, Location loc)
		{
			ParameterData pd = Invocation.GetParameterData (mb);

			bool mismatch = false;
			for (int i = param_types.Length; i > 0; ) {
				i--;

				if (param_types [i] == pd.ParameterType (i))
					continue;
				else {
					mismatch = true;
					break;
				}
			}

			if (mismatch) {
				Report.Error (123, loc, "Method '" + Invocation.FullMethodDesc (mb) + "' does not match " +
					      "delegate '" + FullDelegateDesc () + "'");
				return null;
			}

			if (ret_type == ((MethodInfo) mb).ReturnType)
				return mb;
			else
				mismatch = true;

			if (mismatch) {
				Report.Error (123, loc, "Method '" + Invocation.FullMethodDesc (mb) + "' does not match " +
					      "delegate '" + FullDelegateDesc () + "'");
				return null;
			}

			return null;
		}

		public bool VerifyApplicability (EmitContext ec, ArrayList args, Location loc)
		{
			int arg_count;

			if (args == null)
				arg_count = 0;
			else
				arg_count = args.Count;
			
			if (param_types.Length != arg_count) {
				Report.Error (1593, loc,
					      "Delegate '" + Name + "' does not take '" + arg_count + "' arguments");
				return false;
			}

			for (int i = arg_count; i > 0;) {
				i--;
				Expression conv;
				Argument a = (Argument) args [i];
				Expression a_expr = a.Expr;
				
				if (param_types [i] != a_expr.Type) {
					
					conv = Expression.ConvertImplicitStandard (ec, a_expr, param_types [i], loc);

					if (conv == null) {
						Report.Error (1594, loc,
							      "Delegate '" + Name + "' has some invalid arguments.");

						Report.Error (1503, loc,
						       "Argument " + (i+1) +
						       ": Cannot convert from '" +
						       TypeManager.CSharpName (a_expr.Type)
						       + "' to '" + TypeManager.CSharpName (param_types [i]) + "'");
						return false;
					}

					if (a_expr != conv)
						a.Expr = conv;
				}
			}

			return true;
		}
  		
		public string FullDelegateDesc ()
		{
			StringBuilder sb = new StringBuilder (TypeManager.CSharpName (System.Type.GetType (ReturnType)));
			
			sb.Append (" " + Name);
			sb.Append (" (");

			int length = param_types.Length;
			
			for (int i = length; i > 0; ) {
				i--;
				
				sb.Append (TypeManager.CSharpName (param_types [length - i - 1]));
				if (i != 0)
					sb.Append (", ");
			}
			
			sb.Append (")");
			return sb.ToString ();
			
		}
		
		public void CloseDelegate ()
		{
			TypeBuilder.CreateType ();
		}
		
		public int ModFlags {
			get {
				return mod_flags;
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
	}
	
}
