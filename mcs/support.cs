//
// support.cs: Support routines to work around the fact that System.Reflection.Emit
// can not introspect types that are being constructed
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;

namespace Mono.CSharp {

	public interface ParameterData {
		Type ParameterType (int pos);
		int  Count { get; }
		string ParameterDesc (int pos);
		Parameter.Modifier ParameterModifier (int pos);
	}

	public class ReflectionParameters : ParameterData {
		ParameterInfo [] pi;
		bool last_arg_is_params;
		
		public ReflectionParameters (ParameterInfo [] pi)
		{
			object [] attrs;
			
			this.pi = pi;

			int count = pi.Length-1;

			if (count >= 0) {
				attrs = pi [count].GetCustomAttributes (TypeManager.param_array_type, true);

				if (attrs == null)
					return;
				
				if (attrs.Length == 0)
					return;
				last_arg_is_params = true;
			}
		}
		       
		public Type ParameterType (int pos)
		{
			if (last_arg_is_params && pos >= pi.Length - 1)
				return pi [pi.Length -1].ParameterType;
			else 
				return pi [pos].ParameterType;
		}

		public string ParameterDesc (int pos)
		{
			StringBuilder sb = new StringBuilder ();

			if (pi [pos].IsOut)
				sb.Append ("out ");

			if (pi [pos].IsIn)
				sb.Append ("in ");

			if (pos >= pi.Length - 1 && last_arg_is_params)
				sb.Append ("params ");
			
			sb.Append (TypeManager.CSharpName (ParameterType (pos)));

			return sb.ToString ();
			
		}

		public Parameter.Modifier ParameterModifier (int pos)
		{
			int len = pi.Length;
			
			if (pos >= len - 1)
				if (last_arg_is_params)
					return Parameter.Modifier.PARAMS;
			
			Type t = pi [pos].ParameterType;
			if (t.IsByRef)
				return Parameter.Modifier.OUT;
			
			return Parameter.Modifier.NONE;
		}

		public int Count {
			get {
				return pi.Length;
			}
		}
		
	}

	public class InternalParameters : ParameterData {
		Type [] param_types;

		Parameters parameters;
		
		public InternalParameters (Type [] param_types, Parameters parameters)
		{
			this.param_types = param_types;
			this.parameters = parameters;
		}

		public InternalParameters (DeclSpace ds, Parameters parameters)
			: this (parameters.GetParameterInfo (ds), parameters)
		{
		}

		public int Count {
			get {
				if (param_types == null)
					return 0;

				return param_types.Length;
			}
		}

		public Type ParameterType (int pos)
		{
			if (param_types == null)
				return null;

			Parameter [] fixed_pars = parameters.FixedParameters;
			if (fixed_pars != null){
				int len = fixed_pars.Length;
				if (pos < len)
					return parameters.FixedParameters [pos].ParameterType;
				else 
					return parameters.ArrayParameter.ParameterType;
			} else
				return parameters.ArrayParameter.ParameterType;
		}

		public string ParameterDesc (int pos)
		{
			string tmp = null;
			Parameter p;

			if (pos >= parameters.FixedParameters.Length)
				p = parameters.ArrayParameter;
			else
				p = parameters.FixedParameters [pos];
			
			if (p.ModFlags == Parameter.Modifier.REF)
				tmp = "ref ";
			else if (p.ModFlags == Parameter.Modifier.OUT)
				tmp = "out ";
			else if (p.ModFlags == Parameter.Modifier.PARAMS)
				tmp = "params ";

			Type t = ParameterType (pos);

			return tmp + TypeManager.CSharpName (t);
		}

		public Parameter.Modifier ParameterModifier (int pos)
		{
			if (parameters.FixedParameters == null) {
				if (parameters.ArrayParameter != null) 
					return parameters.ArrayParameter.ModFlags;
				else
					return Parameter.Modifier.NONE;
			}
			
			if (pos >= parameters.FixedParameters.Length)
				return parameters.ArrayParameter.ModFlags;
			else {
				Parameter.Modifier m = parameters.FixedParameters [pos].ModFlags;

				//
				// We use a return value of "OUT" for "reference" parameters.
				// both out and ref flags in the source map to reference parameters.
				//
				if (m == Parameter.Modifier.OUT || m == Parameter.Modifier.REF)
					return Parameter.Modifier.OUT;
				
				return Parameter.Modifier.NONE;
			}
		}
		
	}

	class PtrHashtable : Hashtable {
		class PtrComparer : IComparer {
			public int Compare (object x, object y)
			{
				if (x == y)
					return 0;
				else
					return 1;
			}
		}
		
		public PtrHashtable ()
		{
			comparer = new PtrComparer ();
		}
	}

	//
	// Compares member infos based on their name and
	// also allows one argument to be a string
	//
	class MemberInfoCompare : IComparer {

		public int Compare (object a, object b)
		{
			if (a == null || b == null){
				Console.WriteLine ("Invalid information passed");
				throw new Exception ();
			}
			
			if (a is string)
				return String.Compare ((string) a, ((MemberInfo)b).Name);

			if (b is string)
				return String.Compare (((MemberInfo)a).Name, (string) b);

			return String.Compare (((MemberInfo)a).Name, ((MemberInfo)b).Name);
		}
	}

	struct Pair {
		public object First;
		public object Second;
		
		public Pair (object f, object s)
		{
			First = f;
			Second = s;
		}
	}
}
