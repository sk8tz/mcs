//
// LateBinding.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
// (C) 2005, Novell Inc, (http://novell.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Reflection;
using System.Diagnostics;
using Microsoft.JScript.Vsa;
using System.Collections;

namespace Microsoft.JScript {

	public sealed class LateBinding {

		public object obj;
		private static BindingFlags bind_flags = BindingFlags.Public;
		private string right_hand_side;

		public LateBinding (string name)
		{
			this.right_hand_side = name;
		}


		public LateBinding (string name, object obj)
		{
			throw new NotImplementedException ();
		}


		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public object Call (object [] arguments, bool construct, bool brackets,
					VsaEngine engine)
		{
			if (construct) {
				if (brackets) {
				} else {
				}
			} else {
				if (brackets) {
				} else {
					MethodInfo method = null;

					JSFieldInfo field = null;
					JSObject js_obj = obj as JSObject;
					if (js_obj != null)
						field = js_obj.GetField (right_hand_side) as JSFieldInfo;
					if (field != null) {
						object value = field.GetValue (right_hand_side);
						if (value is ScriptFunction)
							method = ((ScriptFunction) value).method;
					}

					if (method == null) {
						Type type = null;

						if (obj is JSObject)
							type = SemanticAnalyser.map_to_prototype ((JSObject) obj);

						if (type == null)
							type = obj.GetType ();

						string name = right_hand_side == "lastIndexOf" ? "lastIndexOfGood" : right_hand_side;
						method = type.GetMethod (name, BindingFlags.Public | BindingFlags.Static);
					}
					
					if (method == null)
						Console.WriteLine ("LateBinding:Call: method is null for {0}.{1}!", obj, right_hand_side);

					object [] args = assemble_args (obj, method, arguments, engine);
					return method.Invoke (obj, args);
				}
			}
			throw new NotImplementedException ();
		}

		internal static void GetMethodFlags (MethodInfo method, out bool has_engine, out bool has_var_args, out bool has_this)
		{
			JSFunctionAttribute [] custom_attrs = (JSFunctionAttribute [])
				method.GetCustomAttributes (typeof (JSFunctionAttribute), true);
			//
			// We need to iterate through the JSFunctionAttributes to find out whether the function wants
			// to get passed the vsaEngine or not so we can pass the right arguments to it.
			//
			has_engine = false;
			has_var_args = false;
			has_this = false;
			foreach (JSFunctionAttribute attr in custom_attrs) {
				JSFunctionAttributeEnum flags = attr.GetAttributeValue ();
				if ((flags & JSFunctionAttributeEnum.HasEngine) != 0)
					has_engine = true;
				if ((flags & JSFunctionAttributeEnum.HasVarArgs) != 0)
					has_var_args = true;
				if ((flags & JSFunctionAttributeEnum.HasThisObject) != 0)
					has_this = true;
			}
		}

		internal static int GetRequiredArgumentCount (MethodInfo method)
		{
			bool has_engine, has_var_args, has_this;
			GetMethodFlags (method, out has_engine, out has_var_args, out has_this);
			return GetRequiredArgumentCount (method.GetParameters ().Length, has_engine, has_var_args, has_this);
		}

		private static int GetRequiredArgumentCount (int argc, bool has_engine, bool has_var_args, bool has_this)
		{
			if (has_this)
				argc--;
			if (has_engine)
				argc--;
			if (has_var_args)
				argc--;
			return argc;
		}

		internal static object [] assemble_args (object obj, MethodInfo method, object [] arguments, VsaEngine engine)
		{
			bool has_engine, has_var_args, has_this;
			GetMethodFlags (method, out has_engine, out has_var_args, out has_this);

			ParameterInfo [] args = method.GetParameters ();
			int total_argc = args.Length;
			int req_argc = GetRequiredArgumentCount (total_argc, has_engine, has_var_args, has_this);
			Type [] arg_types = new Type [req_argc];

			int j = total_argc - req_argc;
			for (int i = 0; i < req_argc; i++, j++)
				arg_types [i] = args [j].ParameterType;

			return assemble_args (obj, has_engine, has_var_args, has_this, arg_types, arguments, engine);
		}

		internal static object [] assemble_args (object obj, bool has_engine, bool has_var_args, bool has_this,
			Type [] arg_types, object [] arguments, VsaEngine engine)
		{
			ArrayList arg_list = new ArrayList (arguments);
			int req_argc = arg_types.Length;
			int missing_args = req_argc - arg_list.Count;

			// Add missing args
			for (int i = 0; i < missing_args; i++)
				arg_list.Add (null);

			// Convert types of argument to match method signature if necessary
			for (int i = 0; i < req_argc; i++) {
				Type arg_type = arg_types [i];
				object arg = arg_list [i];
				if (!arg_type.IsInstanceOfType (arg)) {
					object new_arg = null;
					if (arg_type == typeof (object)) {
						if (arg != null && arg != DBNull.Value)
							new_arg = Convert.ToObject (arg, engine);
						else
							new_arg = arg;
					} else if (arg_type == typeof (double))
						new_arg = Convert.ToNumber (arg);
					else if (arg_type == typeof (string))
						new_arg = Convert.ToString (arg);
					else {
						Console.WriteLine ("assemble_args: Can not convert to type {0}", arg_type);
						throw new NotImplementedException ();
					}

					arg_list [i] = new_arg;
				}
			}

			if (!has_var_args) {
				// Remove unneeded args
				int added_args = -missing_args;
				/*if (added_args > 0)
					Console.WriteLine ("warning JS1148: There are too many arguments. The extra arguments will be ignored");*/
				for (int i = 0; i < added_args; i++)
					arg_list.RemoveAt (arg_list.Count - 1);
			} else {
				int va_idx = req_argc;
				if (!has_this)
					va_idx--;
				int va_count = arg_list.Count - va_idx;

				object [] var_args = new object [va_count];

				int j = va_idx;
				object arg;
				for (int i = 0; i < va_count; i++, j++) {
					arg = arg_list [j];
					if (arg != null)
						var_args [i] = arg;
				}

				arg_list.RemoveRange (va_idx, va_count);
				arg_list.Add (var_args);
			}

			return build_args (obj, arg_list.ToArray (), engine, has_engine, has_this);
		}

		internal static object [] build_args (object obj, object [] arguments, VsaEngine engine,
			bool has_engine, bool has_this)
		{
			ArrayList args = new ArrayList ();
			if (has_this)
				args.Add (obj);
			if (has_engine)
				args.Add (engine);
			foreach (object o in arguments)
				args.Add (o);
			return args.ToArray ();
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public static object CallValue (object thisObj, object val, object [] arguments,
						bool construct, bool brackets, VsaEngine engine)
		{
			if (construct) {
				if (brackets) {
				}

				if (val is Closure)
					return ((Closure) val).func.Invoke (thisObj, arguments);
				else if (val is FunctionObject)
					return ((FunctionObject) val).Invoke (thisObj, arguments);
			} else if (brackets) {
				if (!(val is JSObject))
					throw new Exception ("val has to be a JSObject, but is " + (val == null ? "null" : val.GetType ().ToString ()));

				JSObject js_val = (JSObject) val;
				JSFieldInfo field = js_val.GetField (Convert.ToString (arguments [0]));
				if (field != null)
					return field.GetValue (arguments [0]);
				else {
					object result = js_val.elems [Convert.ToUint32 (arguments [0])];
					if (result != null)
						return result;
					else
						return null;
				}
			} else {
				if (val is Closure)
					return ((Closure) val).func.Invoke (thisObj, arguments);
				else if (val is FunctionObject)
					return ((FunctionObject) val).Invoke (thisObj, arguments);
				else if (val is RegExpObject)
					return RegExpPrototype.exec (val, arguments [0]);
			}

			Console.WriteLine ("CallValue: construct = {0}, brackets = {1}, this = {2}, val = {3} ({4}), arg[0] = {5}",
				construct, brackets, thisObj.GetType (), val, val.GetType (), arguments [0]);
			throw new NotImplementedException ();
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public static object CallValue2 (object val, object thisObj, object [] arguments,
						 bool construct, bool brackets, VsaEngine engine)
		{
			throw new NotImplementedException ();
		}


		public bool Delete ()
		{
			throw new NotImplementedException ();
		}


		public static bool DeleteMember (object obj, string name)
		{
			throw new NotImplementedException ();
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public object GetNonMissingValue ()
		{
			Type type = obj.GetType ();
			if (obj is GlobalScope)
				type = typeof (GlobalObject);

			MemberInfo [] members = type.GetMember (right_hand_side,
				BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty |
				BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
			if (obj is ScriptObject && members.Length == 0) {
				ScriptObject jsobj = obj as ScriptObject;
				JSFieldInfo field = jsobj.GetField (right_hand_side);
				if (field != null)
					return field.GetValue (right_hand_side);

				type = SemanticAnalyser.map_to_prototype (jsobj);
				members = type.GetMember (right_hand_side);
			}

			if (members.Length > 0) {
				MemberInfo member = members [0];
				MemberTypes member_type = member.MemberType;

				switch (member_type) {
				case MemberTypes.Field:
					return ((FieldInfo) member).GetValue (obj);
				case MemberTypes.Property:
					MethodInfo method = ((PropertyInfo) member).GetGetMethod ();
					return method.Invoke (obj, new object [] { });
				case MemberTypes.Method:
					return new FunctionObject ((MethodInfo) member);
				default:
					Console.WriteLine ("GetNonMissingValue: type = {0}, member_type = {1}", type, member_type);
					break;
				}
			}

			Console.WriteLine ("members.Length = {0}, obj = {1}, type = {2}, rhs = {3}",
				members.Length, obj, type, right_hand_side);
			throw new NotImplementedException ();
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public object GetValue2 ()
		{
			throw new NotImplementedException ();
		}

		private static void SetArrayLength (ArrayObject ary, object field)
		{
			uint old_len = (uint) ary.length;
			uint index = Convert.ToUint32 (field);
			if (index > 0 && index != 4294967295 && index > old_len &&
				Convert.ToString (index) == Convert.ToString (field))
				ary.length = index + 1;
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public static void SetIndexedPropertyValueStatic (object obj, object [] arguments,
								  object value)
		{
			if (!(obj is JSObject))
				throw new Exception ("obj should be a JSObject");

			JSObject js_obj = (JSObject) obj;
			foreach (object o in arguments) {
				if (js_obj.elems.ContainsKey (o)) {
					object old_value = js_obj.elems [o];
					JSFieldInfo field = old_value as JSFieldInfo;
					if (field != null)
						field.SetValue (o, value);
					else
						js_obj.elems [o] = value;
				} else {
					ArrayObject ary = js_obj as ArrayObject;
					if (ary != null)
						SetArrayLength(ary, o);

					js_obj.AddField (o, value);
				}
			}
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public void SetValue (object value)
		{
			Type type = obj.GetType ();
			if (obj is GlobalScope)
				type = typeof (GlobalObject);
			
			MemberInfo [] members = type.GetMember (right_hand_side);
			if (obj is JSObject && members.Length == 0) {
				type = SemanticAnalyser.map_to_prototype ((JSObject) obj);
				members = type.GetMember (right_hand_side);
			}

			if (members.Length > 0) {
				MemberInfo member = members [0];
				MemberTypes member_type = member.MemberType;

				switch (member_type) {
				case MemberTypes.Property:
					MethodInfo method = ((PropertyInfo) member).GetSetMethod ();
					method.Invoke (obj, new object [] { value });
					return;
				}
			}

			if (obj is JSObject) {
				JSObject js_obj = (JSObject) obj;
				if (js_obj.elems.ContainsKey (right_hand_side)) {
					object old_value = js_obj.elems [right_hand_side];
					if (old_value is JSFieldInfo) {
						JSFieldInfo field = (JSFieldInfo) old_value;
						field.SetValue (js_obj, value);
					} else
						js_obj.elems [right_hand_side] = value;
				} else {
					ArrayObject ary = js_obj as ArrayObject;
					if (ary != null)
						SetArrayLength (ary, right_hand_side);

					js_obj.AddField (right_hand_side, value);
				}
				return;
			}

			Console.WriteLine ("SetValue: obj = {0}, rhs = {1}, count = {2}", obj, right_hand_side, members.Length);
			throw new NotImplementedException ();
		}
	}
}
