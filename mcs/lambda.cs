//
// lambda.cs: support for lambda expressions
//
// Authors: Miguel de Icaza (miguel@gnu.org)
//          Marek Safar (marek.safar@gmail.com)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2007 Novell, Inc
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {
	public class LambdaExpression : AnonymousMethodExpression
	{
		readonly bool explicit_parameters;

		//
		// The parameters can either be:
		//    A list of Parameters (explicitly typed parameters)
		//    An ImplicitLambdaParameter
		//
		public LambdaExpression (AnonymousMethodExpression parent,
					 GenericMethod generic, TypeContainer host,
					 Parameters parameters, Block container,
					 Location loc)
			: base (parent, generic, host, parameters, container, loc)
		{
			if (parameters.Count > 0)
				explicit_parameters = !(parameters.FixedParameters [0] is ImplicitLambdaParameter);
		}

		public override bool HasExplicitParameters {
			get {
				return explicit_parameters;
			}
		}

		protected override Parameters ResolveParameters (EmitContext ec, TypeInferenceContext tic, Type delegateType)
		{
			if (!TypeManager.IsDelegateType (delegateType))
				return null;

			ParameterData d_params = TypeManager.GetDelegateParameters (delegateType);

			if (explicit_parameters) {
				if (!VerifyExplicitParameters (delegateType, d_params, ec.IsInProbingMode))
					return null;

				return Parameters;
			}

			//
			// If L has an implicitly typed parameter list we make implicit parameters explicit
			// Set each parameter of L is given the type of the corresponding parameter in D
			//
			if (Parameters.Count != d_params.Count)
				return null;

			if (Parameters.Types == null)
				Parameters.Types = new Type [Parameters.Count];

			for (int i = 0; i < d_params.Count; i++) {
				// D has no ref or out parameters
				if ((d_params.ParameterModifier (i) & Parameter.Modifier.ISBYREF) != 0)
					return null;

				Type d_param = d_params.Types [i];

#if MS_COMPATIBLE
				// Blablabla, because reflection does not work with dynamic types
				if (d_param.IsGenericParameter)
					d_param = delegateType.GetGenericArguments () [d_param.GenericParameterPosition];
#endif
				// When inferred context exists all generics parameters have type replacements
				if (tic != null) {
					d_param = tic.InflateGenericArgument (d_param);
				}

				Parameters.Types [i] = Parameters.FixedParameters[i].ParameterType = d_param;
			}
			return Parameters;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			//
			// Only explicit parameters can be resolved at this point
			//
			if (explicit_parameters) {
				if (!Parameters.Resolve (ec))
					return null;
			}

			eclass = ExprClass.Value;
			type = TypeManager.anonymous_method_type;						
			return this;
		}

		protected override AnonymousMethod CompatibleMethodFactory (Type returnType, Type delegateType, Parameters p, ToplevelBlock b)
		{
			return new LambdaMethod (RootScope, Host,
				GenericMethod, p, Container, b, returnType,
				delegateType, loc);
		}

		public override string GetSignatureForError ()
		{
			return "lambda expression";
		}
	}

	public class LambdaMethod : AnonymousMethod
	{
		public LambdaMethod (RootScopeInfo root_scope,
					DeclSpace host, GenericMethod generic,
					Parameters parameters, Block container,
					ToplevelBlock block, Type return_type, Type delegate_type,
					Location loc)
			: base (root_scope, host, generic, parameters, container, block,
				return_type, delegate_type, loc)
		{
		}

		public override string ContainerType {
			get {
				return "lambda expression";
			}
		}
	}

	//
	// This is a return statement that is prepended lambda expression bodies that happen
	// to be expressions.  Depending on the return type of the delegate this will behave
	// as either { expr (); return (); } or { return expr (); }

	public class ContextualReturn : Return
	{
		public ContextualReturn (Expression expr)
			: base (expr, expr.Location)
		{
		}
	}
}
