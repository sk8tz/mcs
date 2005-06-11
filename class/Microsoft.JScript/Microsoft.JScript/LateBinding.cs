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
					Type type = null;

					if (obj is JSObject)
						type = SemanticAnalyser.map_to_prototype ((JSObject) obj);

					MethodInfo method = type.GetMethod (right_hand_side, BindingFlags.Public | BindingFlags.Static);
					object [] args = build_args (arguments, engine);
					return method.Invoke (type, args);
				}
			}
			throw new NotImplementedException ();
		}

		private object [] build_args (object [] arguments, VsaEngine engine)
		{
			ArrayList args = new ArrayList ();
			if (obj != null)
				args.Add (obj);
			if (engine != null)
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
					return null;
				} 
				return null;
			} else if (brackets) {
				if (!(val is JSObject))
					throw new Exception ("val has to be a JSObject");

				JSObject js_val = (JSObject) val;
				object res = js_val.GetField (Convert.ToString (arguments [0]));
				if (res is JSFieldInfo) 
					return ((JSFieldInfo) res).GetValue (arguments [0]);
				else 
					throw new NotImplementedException ();
			} else {
				return null;
			}
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
			MemberInfo [] members = type.GetMember (right_hand_side);
			if (members.Length > 0) {
				MemberInfo member = members [0];
				MemberTypes member_type = member.MemberType;

				switch (member_type) {
				case MemberTypes.Property:
					MethodInfo method = ((PropertyInfo) member).GetGetMethod ();
					return method.Invoke (obj, new object [] {});
				}
			}
			throw new NotImplementedException ();
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public object GetValue2 ()
		{
			throw new NotImplementedException ();
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public static void SetIndexedPropertyValueStatic (object obj, object [] arguments,
								  object value)
		{
			if (!(obj is JSObject))
				throw new Exception ("obj should be a JSObject");

			JSObject js_obj = (JSObject) obj;
			foreach (object o in arguments)
				js_obj.AddField (o, value);
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public void SetValue (object value)
		{
			throw new NotImplementedException ();
		}
	}
}
