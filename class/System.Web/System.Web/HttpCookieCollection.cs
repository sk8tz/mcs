//
// System.Web.HttpCookieCollection.cs 
//
// Author:
//	Chris Toshok (toshok@novell.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Collections.Specialized;

namespace System.Web {

	public sealed class HttpCookieCollection : NameObjectCollectionBase {

		private bool auto_fill = false;

		[Obsolete ("Don't use this constructor, use the (bool, bool) one, as it's more clear what it does")]
		internal HttpCookieCollection (HttpResponse Response, bool ReadOnly)
		{
			auto_fill = Response != null;
			IsReadOnly = ReadOnly;
		}

		internal HttpCookieCollection (bool auto_fill, bool read_only)
		{
			this.auto_fill = auto_fill;
			IsReadOnly = read_only;
		}

		internal HttpCookieCollection (string cookies)
		{
			if (cookies == null || cookies == "")
				return;

			string[] cookie_components = cookies.Split (';');
			foreach (string kv in cookie_components) {
				int pos = kv.IndexOf ('=');
				if (pos == -1) {
					/* XXX ugh */
					continue;
				}
				else {
					string key = kv.Substring (0, pos);
					string val = kv.Substring (pos+1);

					Add (new HttpCookie (key.Trim (), val.Trim()));
				}
			}
		}

		public HttpCookieCollection ()
		{
		}

		public void Add (HttpCookie cookie)
		{
			if (BaseGet (cookie.Name) != null)
				return;

			BaseAdd (cookie.Name, cookie);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		public void CopyTo (Array array, int index)
		{
			/* XXX this is kind of gross and inefficient
			 * since it makes a copy of the superclass's
			 * list */
			object[] values = BaseGetAllValues();
			values.CopyTo (array, index);
		}

		public string GetKey (int index)
		{
			HttpCookie cookie = (HttpCookie)BaseGet (index);
			if (cookie == null)
				return null;
			else
				return cookie.Name;
		}

		public void Remove (string name)
		{
			BaseRemove (name);
		}

		public void Set (HttpCookie cookie)
		{
			BaseSet (cookie.Name, cookie);
		}

		public HttpCookie Get (int index)
		{
			return (HttpCookie)BaseGet (index);
		}

		public HttpCookie Get (string name)
		{
			return (HttpCookie)BaseGet (name);
		}

		public HttpCookie this [int index]
		{
			get {
				return (HttpCookie)BaseGet (index);
			}
		}

		public HttpCookie this [string name]
		{
			get {
				HttpCookie cookie = (HttpCookie)BaseGet (name);
				if (!IsReadOnly && auto_fill && cookie == null) {
					cookie = new HttpCookie (name);
					BaseAdd (name, cookie);
				}
				return cookie;
			}
		}

		public string[] AllKeys {
			get {
				/* XXX another inefficient copy due to
				 * lack of exposure from the base
				 * class */
				string[] keys = new string [Keys.Count];
				for (int i = 0; i < Keys.Count; i ++)
					keys[i] = Keys[i];
				
				return keys;
			}
		}
	}
}

