//
// Mono.ILASM.TypeArguments
//
// Author(s):
//  Ankit Jain  <jankit@novell.com>
//
// Copyright 2005 Novell, Inc (http://www.novell.com)
//

//Note: This is shared by modified types of the same generic instance
//      Eg. foo`1<int32> and foo`1<int32> [] would share their GenericArguments

using System;
using System.Collections;
using System.Text;

namespace Mono.ILASM {

	public class GenericArguments {
		ArrayList type_list;
		string type_str;
		ITypeRef [] type_arr;

		public GenericArguments ()
		{
			type_list = null;
			type_arr = null;
			type_str = null;
		}

		public int Count {
			get { return type_list.Count; }
		}

		public void Add (ITypeRef type)
		{
			if (type == null)
				throw new ArgumentException ("type");

			if (type_list == null)
				type_list = new ArrayList ();
			type_list.Add (type);
			type_str = null;
			type_arr = null;
		}
		
		public ITypeRef [] ToArray ()
		{
			if (type_list == null)
				return null;
			if (type_arr == null)
				type_arr = (ITypeRef []) type_list.ToArray (typeof (ITypeRef));

			return type_arr;
		}

		public PEAPI.Type [] Resolve (CodeGen code_gen)
		{
			int i = 0;
			PEAPI.Type [] p_type_list = new PEAPI.Type [type_list.Count];
			foreach (ITypeRef type in type_list) {
				type.Resolve (code_gen);
				p_type_list [i ++] = type.PeapiType;
			}

			return p_type_list;
		}

		private void MakeString ()
		{
			//Build full_name (foo < , >)
			StringBuilder sb = new StringBuilder ();
			sb.Append ("<");
			foreach (ITypeRef tr in type_list)
				sb.AppendFormat ("{0}, ", tr.FullName);
			//Remove the extra ', ' at the end
			sb.Length -= 2;
			sb.Append (">");
			type_str = sb.ToString ();
		}

		public override string ToString ()
		{
			if (type_str == null)
				MakeString ();
			return type_str;
		}
	}

}

