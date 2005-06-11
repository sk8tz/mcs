//
// RegExpConstructor.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
// (C) Copyright 2005, Novell Inc (http://novell.com)
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

namespace Microsoft.JScript {

	public class RegExpConstructor : ScriptFunction {

		internal static RegExpConstructor Ctr = new RegExpConstructor ();

		internal RegExpConstructor ()
		{
		}

		public Object Construct (string pattern, bool ignoreCase, bool global, bool multiLine)
		{
			RegExpObject re = new RegExpObject (pattern, ignoreCase, global, multiLine);
			return re;
		}

		//
		// Invoked when we do: new RegExp (...)
		//
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs)]
		public new RegExpObject CreateInstance (params object [] args)
		{
			return Invoke (args);
		}

		//
		// Invoked when we do: x = RegExp (...)
		//
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs)]
		public RegExpObject Invoke (params object [] args)
		{
			if (args != null) {
				int length = args.Length;
				if (length > 0) {
					object o = args [0];
					if (o is RegExpObject)
						return (RegExpObject) o;

					string flags = "";
					if (length > 1)
						flags += Convert.ToString (args [1]);
					return new RegExpObject (Convert.ToString (args [0]),
								 flags.IndexOfAny (new char [] {'i'}) > -1,
								 flags.IndexOfAny (new char [] {'g'}) > -1,
								 flags.IndexOfAny (new char [] {'m'}) > -1);
				}
			}
			throw new NotImplementedException ();
		}

		public Object index {
			get { throw new NotImplementedException (); }
		}

		public Object input {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public Object lastIndex {
			get { throw new NotImplementedException (); }
		}

		public Object lastMatch {
			get { throw new NotImplementedException (); }
		}

		public Object lastParen {
			get { throw new NotImplementedException (); }
		}

		public Object leftContext {
			get { throw new NotImplementedException (); }
		}

		public Object rightContext {
			get { throw new NotImplementedException (); }
		}
	}
}
