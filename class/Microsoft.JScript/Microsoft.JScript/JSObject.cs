//
// JSObject.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
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
using System.Collections;
using System.Runtime.InteropServices.Expando;

namespace Microsoft.JScript {

	public class JSObject : ScriptObject, IEnumerable, IExpando {

		public JSObject ()
		{		
			elems = new Hashtable ();
		}

		public FieldInfo AddField (string name)
		{
			JSFieldInfo fi = new JSFieldInfo (name);
			elems.Add (name, fi);
			return fi;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return elems.GetEnumerator ();
		}

		PropertyInfo IExpando.AddProperty (string name)
		{
			JSPropertyInfo pi = new JSPropertyInfo (name);
			elems.Add (name, pi);
			return pi;
		}
		
		MethodInfo IExpando.AddMethod (String name, Delegate method)
		{
			JSMethodInfo minfo = new JSMethodInfo (name, method.Method);
			elems.Add (name, minfo);
			return minfo;
		}
		
		public override MemberInfo [] GetMember (string name, BindingFlags bindFlags)
		{
			throw new NotImplementedException ();
		}

		public override MemberInfo [] GetMembers (BindingFlags bindFlags)
		{
			MemberInfo [] members;
			IEnumerator enumerator = elems.GetEnumerator ();
			ArrayList tmp = new ArrayList ();

			while (enumerator.MoveNext ())
				tmp.Add (((Node) enumerator.Current).Element);
			
			members = new MemberInfo [tmp.Count];
			tmp.CopyTo (members);
			return members;
		}

		public void SetMemberValue2 (string name, Object value)
		{
			AddField (name, value);
		}

		void IExpando.RemoveMember (MemberInfo m)
		{
			elems.Remove (m);
		}

		public override string ToString ()
		{
			return String.Format ("[{0}]", ClassName);
		}

		internal string ClassName {
			get {
				if (this is ObjectPrototype)
					return "Object";
				else if (this is ArrayPrototype)
					return "Array";
				else if (this is StringPrototype)
					return "String";
				else if (this is NumberPrototype)
					return "Number";
				else if (this is DatePrototype)
					return "Date";
				else if (this is EvalErrorObject)
					return "EvalError";
				else if (this is RangeErrorObject)
					return "RangeError";
				else if (this is ReferenceErrorObject)
					return "ReferenceError";
				else if (this is SyntaxErrorObject)
					return "SyntaxError";
				else if (this is TypeErrorObject)
					return "TypeError";
				else if (this is URIErrorObject)
					return "URIError";
				else if (this is ErrorPrototype)
					return "Error";
				else
					return this.GetType ().ToString ();
			}
		}

		internal void AddField (object name, object value)
		{
			string str_name = Convert.ToString (name);
			
			if (proper_array_index (str_name)) {
				JSFieldInfo field = (JSFieldInfo) AddField (str_name);
				field.SetValue (str_name, value);
			} else
				throw new Exception ("Not a valid index array");
		}

		//
		// FIXME: 
		//
		private bool proper_array_index (object name)
		{
			return true;
		}

		internal virtual object GetDefaultValue (Type hint, bool avoid_toString)
		{
			if (avoid_toString)
				return this;
			else
				return ObjectPrototype.toString (this);				
		}

		internal object GetDefaultValue (Type hint)
		{
			return GetDefaultValue (hint, false);
		}
	}
}
