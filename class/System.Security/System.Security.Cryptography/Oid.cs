//
// Oid.cs - System.Security.Cryptography.Oid
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
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

#if NET_2_0

using System;

namespace System.Security.Cryptography {

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public sealed class Oid {

		private string _value;
		private string _name;

		// constructors

		public Oid () {}

		public Oid (string oid) 
		{
			if (oid == null)
				throw new ArgumentNullException ("oid");

			_value = oid;
			_name = GetName (oid);
		}

		public Oid (string value, string friendlyName)
		{
			_value = value;
			_name = friendlyName;
		}

		public Oid (Oid oid) 
		{
// FIXME: compatibility with fx 1.2.3400.0
//			if (oid == null)
//				throw new ArgumentNullException ("oid");

			_value = oid.Value;
			_name = oid.FriendlyName;
		}

		// properties

		public string FriendlyName {
			get { return _name; }
			set { 
				if (value == null)
					throw new ArgumentNullException ("value");

				_name = value;
				_value = GetValue (_name);
			}
		}

		public string Value { 
			get { return _value; }
			set { 
				if (value == null)
					throw new ArgumentNullException ("value");

				_value = value; 
				_name = GetName (_value);
			}
		}

		// private methods

		// TODO - find the complete list
		private string GetName (string value) 
		{
			switch (value) {
				case "1.2.840.113549.1.1.1":
					return "RSA";
				default:
					return _name;
			}
		}

		// TODO - find the complete list
		private string GetValue (string name) 
		{
			switch (name) {
				case "RSA":
					return "1.2.840.113549.1.1.1";
				default:
					return _value;
			}
		}
	}
}

#endif