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
			object [] a;
			
			this.pi = pi;

			int count = pi.Length-1;

			if (count > 0) {
				a = pi [count].GetCustomAttributes (TypeManager.param_array_type, false);

				if (a != null)
					if (a.Length != 0)
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

			if (pos == pi.Length - 1)
				sb.Append ("params ");
			
			sb.Append (TypeManager.CSharpName (ParameterType (pos)));

			return sb.ToString ();
			
		}

		public Parameter.Modifier ParameterModifier (int pos)
		{
			if (pos >= pi.Length - 1) 
				if (last_arg_is_params)
					return Parameter.Modifier.PARAMS;

			if (pi [pos].IsOut)
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

		public InternalParameters (TypeContainer tc, Parameters parameters)
			: this (parameters.GetParameterInfo (tc), parameters)
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

			int len = parameters.FixedParameters.Length;

			if (pos < len)
				return parameters.FixedParameters [pos].ParameterType;
			else 
				return parameters.ArrayParameter.ParameterType;

			//
			// Return the internal type.
			//
			//return p.ParameterType;
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
			if (pos >= parameters.FixedParameters.Length)
				return parameters.ArrayParameter.ModFlags;
			else
				return parameters.FixedParameters [pos].ModFlags;
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
