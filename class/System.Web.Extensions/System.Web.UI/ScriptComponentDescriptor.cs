﻿//
// ScriptComponentDescriptor.cs
//
// Author:
//   Igor Zelmanovich <igorz@mainsoft.com>
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
using System.Web.Script.Serialization;

namespace System.Web.UI
{
	public class ScriptComponentDescriptor : ScriptDescriptor
	{
		string _type;
		string _id;
		Dictionary<string, string> _properties;
		Dictionary<string, string> _events;
		Dictionary<string, string> _references;

		public ScriptComponentDescriptor (string type) {
			if (String.IsNullOrEmpty (type))
				throw new ArgumentException ("Value cannot be null or empty.", "type");
			_type = type;
		}

		public virtual string ClientID {
			get {
				return ID;
			}
		}

		public virtual string ID {
			get {
				if (_id == null)
					return String.Empty;
				return _id;
			}
			set {
				_id = value;
			}
		}

		public string Type {
			get {
				return _type;
			}
			set {
				if (String.IsNullOrEmpty (value))
					throw new ArgumentException ("Value cannot be null or empty.", "value");
				_type = value;
			}
		}

		public void AddComponentProperty (string name, string componentID) {
			if (name == null)
				throw new ArgumentException ("Value cannot be null or empty.", "name");
			if (componentID == null)
				throw new ArgumentException ("Value cannot be null or empty.", "componentID");

			AddEntry (ref _references, String.Format ("\"{0}\"", name), String.Format ("\"{0}\"", componentID));
		}

		public void AddElementProperty (string name, string elementID) {
			if (name == null)
				throw new ArgumentException ("Value cannot be null or empty.", "name");
			if (elementID == null)
				throw new ArgumentException ("Value cannot be null or empty.", "elementID");

			AddEntry (ref _properties, String.Format ("\"{0}\"", name), String.Format ("$get(\"{0}\")", elementID));
		}

		public void AddEvent (string name, string handler) {
			if (name == null)
				throw new ArgumentException ("Value cannot be null or empty.", "name");
			if (handler == null)
				throw new ArgumentException ("Value cannot be null or empty.", "handler");

			AddEntry (ref _events, String.Format ("\"{0}\"", name), handler);
		}

		public void AddProperty (string name, object value) {
			if (name == null)
				throw new ArgumentException ("Value cannot be null or empty.", "name");

			string valueString;
			if (value == null)
				valueString = "null";
			else
				valueString = JavaScriptSerializer.DefaultSerializer.Serialize (value);

			AddEntry (ref _properties, String.Format ("\"{0}\"", name), valueString);
		}

		public void AddScriptProperty (string name, string script) {
			if (name == null)
				throw new ArgumentException ("Value cannot be null or empty.", "name");
			if (script == null)
				throw new ArgumentException ("Value cannot be null or empty.", "script");

			AddEntry (ref _properties, String.Format ("\"{0}\"", name), script);
		}

		void AddEntry (ref Dictionary<string, string> dictionary, string key, string value) {
			if (dictionary == null)
				dictionary = new Dictionary<string, string> ();
			dictionary.Add (key, value);
		}

		protected internal override string GetScript () {
			return String.Format ("$create({0}, {1}, {2}, {3});", Type, SerializeDictionary (_properties), SerializeDictionary (_events), SerializeDictionary (_references));
		}

		string SerializeDictionary (Dictionary<string, string> dictionary) {
			if (dictionary == null || dictionary.Count == 0)
				return "null";
			StringBuilder sb = new StringBuilder ("{");
			foreach (string key in dictionary.Keys) {
				sb.AppendFormat ("{0}:{1},", key, dictionary [key]);
			}
			sb.Length--;
			sb.Append ("}");
			return sb.ToString ();
		}
	}
}