//
// Mono.ILASM.GenericParameters
//
// Author(s):
//  Ankit Jain  <jankit@novell.com>
//
// Copyright 2006 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.Text;

namespace Mono.ILASM {

	public class GenericParameter {
		string id;
		int num;
                PEAPI.GenericParamAttributes attr;
		ArrayList constraintsList;
		
		public GenericParameter (string id) 
			: this (id, 0, null)
		{
		}

		public GenericParameter (string id, PEAPI.GenericParamAttributes attr, ArrayList constraints)
		{
			this.id = id;
			this.attr = attr;
			num = -1;
			constraintsList = null;
				
			if (constraints != null)
				foreach (ITypeRef typeref in constraints)
					AddConstraint (typeref);
		}

		public string Id {
			get { return id; }
		}

		public int Num {
			get { return num; }
			set { num = value; }
		}

		public void AddConstraint (ITypeRef constraint)
		{
			if (constraint == null)
				throw new ArgumentException ("constraint");

			if (constraintsList == null)
				constraintsList = new ArrayList ();

			constraintsList.Add (constraint);
		}

		public override string ToString ()
		{
			return Id;
		}

		public void Resolve (CodeGen code_gen, PEAPI.MethodDef methoddef)
		{
			PEAPI.GenericParameter gp = methoddef.AddGenericParameter ((short) num, id, attr);
			ResolveConstraints (code_gen, gp);
		}

		public void Resolve (CodeGen code_gen, PEAPI.ClassDef classdef)
		{
			PEAPI.GenericParameter gp = classdef.AddGenericParameter ((short) num, id, attr);
			ResolveConstraints (code_gen, gp);
		}

		public void ResolveConstraints (GenericParameters type_gen_params, GenericParameters method_gen_params)
		{
			if (constraintsList == null)
				return;
				
			foreach (ITypeRef constraint in constraintsList) {
				IGenericTypeRef gtr = constraint as IGenericTypeRef;
				if (gtr != null)
					gtr.Resolve (type_gen_params, method_gen_params);
			}
		}

		public void ResolveConstraints (CodeGen code_gen, PEAPI.GenericParameter gp)
		{
			if (constraintsList == null)
				return;

			foreach (ITypeRef constraint in constraintsList) {
				constraint.Resolve (code_gen);
				gp.AddConstraint (constraint.PeapiType);
			}
		}

	}

	public class GenericParameters {
		ArrayList param_list;
		string param_str;

		public GenericParameters ()
		{
			param_list = null;
			param_str = null;
		}

		public int Count {
			get { return (param_list == null ? 0 : param_list.Count); }
		}

		public GenericParameter this [int index] {
			get { return (param_list != null ? (GenericParameter) param_list [index] : null); }
			set { Add (value); }
		}

		public void Add (GenericParameter gen_param)
		{
			if (gen_param == null)
				throw new ArgumentException ("gen_param");

			if (param_list == null)
				param_list = new ArrayList ();
			gen_param.Num = param_list.Count;
			param_list.Add (gen_param);
			param_str = null;
		}
		
		public int GetGenericParamNum (string id)
		{
			if (param_list == null)
				//FIXME: Report error
				throw new Exception (String.Format ("Invalid type parameter '{0}'", id));

			foreach (GenericParameter param in param_list)
				if (param.Id == id)
					return param.Num;
			return -1;
		}

		public void Resolve (CodeGen code_gen, PEAPI.ClassDef classdef)
		{
			foreach (GenericParameter param in param_list)
				param.Resolve (code_gen, classdef);
		}
		
		public void Resolve (CodeGen code_gen, PEAPI.MethodDef methoddef)
		{
			foreach (GenericParameter param in param_list)
				param.Resolve (code_gen, methoddef);
		}

		public void ResolveConstraints (GenericParameters type_gen_params, GenericParameters method_gen_params)
		{
			foreach (GenericParameter param in param_list)
				param.ResolveConstraints (type_gen_params, method_gen_params);
			param_str = null;
		}

		private void MakeString ()
		{
			//Build full_name (foo < , >)
			StringBuilder sb = new StringBuilder ();
			sb.Append ("<");
			foreach (GenericParameter param in param_list)
				sb.AppendFormat ("{0}, ", param);
			//Remove the extra ', ' at the end
			sb.Length -= 2;
			sb.Append (">");
			param_str = sb.ToString ();
		}

		public override string ToString ()
		{
			if (param_str == null)
				MakeString ();
			return param_str;
		}
	}

}
