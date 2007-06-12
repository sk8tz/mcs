﻿//
// JavaScriptSerializer.cs
//
// Author:
//   Konstantin Triger <kostat@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using System.Reflection;
using Newtonsoft.Json.Utilities;
using System.ComponentModel;

namespace System.Web.Script.Serialization
{
	public class JavaScriptSerializer
	{
		public JavaScriptSerializer () {
		}

		public JavaScriptSerializer (JavaScriptTypeResolver resolver) {
			throw new NotImplementedException ();
		}

		public int MaxJsonLength {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public int RecursionLimit {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public T ConvertToType<T> (object obj) {
			if (obj == null)
				return default (T);

			return (T) ConvertToType (typeof (T), obj);
		}

		object ConvertToType (Type type, object obj) {
			if (obj == null)
				return null;

			if (obj is IDictionary<string, object>) {
				if (type == null && !(obj is Dictionary<string, object>))
					obj = Evaluate ((IDictionary<string, object>) obj);
				return Deserialize ((IDictionary<string, object>) obj, type);
			}
			if (obj is IEnumerable<object>)
				return Deserialize ((IEnumerable<object>) obj, type);

			if (type == null)
				return obj;

			Type sourceType = obj.GetType ();
			if (type.IsAssignableFrom (sourceType))
				return obj;

			TypeConverter c = TypeDescriptor.GetConverter (type);
			if (c.CanConvertFrom(sourceType)) {
				if (obj is string)
					return c.ConvertFromInvariantString((string)obj);

				return c.ConvertFrom (obj);
			}
			
			return Convert.ChangeType (obj, type);
		}

		public T Deserialize<T> (string input) {
			JsonSerializer ser = new JsonSerializer ();
			return ConvertToType<T> (ser.Deserialize (new StringReader (input)));
		}

		static object Evaluate (object value) {
			if (value is IDictionary<string, object>)
				value = Evaluate ((IDictionary<string, object>) value);
			else
			if (value is IEnumerable<object>)
				value = Evaluate ((IEnumerable<object>) value);
			return value;
		}

		static object Evaluate (IEnumerable<object> e) {
			ArrayList list = new ArrayList ();
			foreach (object value in e)
				list.Add (Evaluate(value));

			return list;
		}

		static object Evaluate (IDictionary<string, object> dict) {
			Dictionary<string, object> d = new Dictionary<string, object> (StringComparer.Ordinal);
			foreach (KeyValuePair<string, object> entry in dict)
				d.Add (entry.Key, Evaluate(entry.Value));

			return d;
		}

		static readonly Type typeofObject = typeof(object);
		static readonly Type typeofGenList = typeof (List<>);

		object Deserialize (IEnumerable<object> col, Type type) {
			Type elementType = null;
			if (type != null && type.HasElementType)
				elementType = type.GetElementType ();

			IList list;
			if (type == null || type.IsArray || typeofObject == type)
				list = new ArrayList ();
			else if (ReflectionUtils.IsInstantiatableType (type))
				// non-generic typed list
				list = (IList) Activator.CreateInstance (type, true);
			else if (ReflectionUtils.IsAssignable (type, typeofGenList)) {
				if (type.IsGenericType) {
					Type [] genArgs = type.GetGenericArguments ();
					elementType = genArgs [0];
					// generic list
					list = (IList) Activator.CreateInstance (typeofGenList.MakeGenericType (genArgs));
				}
				else
					list = new ArrayList ();
			}
			else
				throw new JsonSerializationException (string.Format ("Deserializing list type '{0}' not supported.", type.GetType ().Name));

			if (list.IsReadOnly) {
				Evaluate (col);
				return list;
			}

			foreach (object value in col)
				list.Add (ConvertToType (elementType, value));

			if (type != null && type.IsArray)
				list = ((ArrayList) list).ToArray (elementType);

			return list;
		}

		object Deserialize (IDictionary<string, object> dict, Type type) {
			if (type == null)
				type = Type.GetType ((string) dict ["__type"]);

			object target = Activator.CreateInstance (type, true);

			foreach (KeyValuePair<string, object> entry in dict) {
				object value = entry.Value;
				if (target is IDictionary) {
					((IDictionary) target).Add (entry.Key, ConvertToType (ReflectionUtils.GetTypedDictionaryValueType (type), value));
					continue;
				}
				MemberInfo [] memberCollection = type.GetMember (entry.Key);
				if (memberCollection == null || memberCollection.Length == 0) {
					//must evaluate value
					Evaluate (value);
					continue;
				}

				MemberInfo member = memberCollection [0];

				if (!ReflectionUtils.CanSetMemberValue (member)) {
					//must evaluate value
					Evaluate (value);
					continue;
				}
				
				ReflectionUtils.SetMemberValue (member, target, ConvertToType(ReflectionUtils.GetMemberUnderlyingType (member), value));
			}

			return target;
		}

		public object DeserializeObject (string input) {
			throw new NotImplementedException ();
		}

		public void RegisterConverters (IEnumerable<JavaScriptConverter> converters) {
			throw new NotImplementedException ();
		}

		public string Serialize (object obj) {
			StringBuilder b = new StringBuilder ();
			Serialize (obj, b);
			return b.ToString ();
		}

		public void Serialize (object obj, StringBuilder output) {
			JsonSerializer ser = new JsonSerializer ();
			ser.Serialize (new StringWriter (output), obj);
		}
	}
}
