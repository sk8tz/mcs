// 
// System.Web.HttpCookieCollection
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   (First impl Bob Smith <bob@thestuff.net>)
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
using System.Web;
using System.Collections.Specialized;

namespace System.Web
{
	public sealed class HttpCookieCollection : NameObjectCollectionBase
	{
		HttpCookie [] _AllCookies;
		string [] _AllKeys;

		HttpResponse _Response;

		internal HttpCookieCollection(HttpResponse Response, bool ReadOnly) : base()
		{
			_Response = Response;
			IsReadOnly = ReadOnly;
		}

		public HttpCookieCollection () { }

		public string [] AllKeys
		{
			get {
				if (null == _AllKeys)
					_AllKeys = BaseGetAllKeys ();

				return _AllKeys;
			}
		}

		public HttpCookie this [int index]
		{
			get { return Get (index); }
		}

		public HttpCookie this [string name]
		{
			get { return Get (name); }
		}

		public void Add (HttpCookie cookie)
		{
			if (null != _Response)
				_Response.GoingToChangeCookieColl ();

			// empy performance cache
			_AllCookies = null;
			_AllKeys = null;

			if (BaseGet (cookie.Name) == null)
				BaseAdd (cookie.Name, cookie);
			else
				BaseSet (cookie.Name, cookie);

			if (null != _Response)
				_Response.OnCookieAdd (cookie);
		}

		public void Clear ()
		{
			_AllCookies = null;
			_AllKeys = null;
			BaseClear ();
		}

		public void CopyTo (Array dest, int index)
		{
			if (null == _AllCookies) {
				_AllCookies = new HttpCookie [Count];

				for (int i = 0; i != Count; i++)
					_AllCookies [i] = Get (i);
			}

			_AllCookies.CopyTo (dest, index);
		}

		public HttpCookie Get (int index)
		{
			return BaseGet (index) as HttpCookie;
		}

		public HttpCookie Get (string name)
		{
			HttpCookie oRet = BaseGet (name) as HttpCookie;
			if (null == oRet && _Response != null) {
				_AllCookies = null;
				_AllKeys = null;

				_Response.GoingToChangeCookieColl ();

				oRet = new HttpCookie (name);
				BaseAdd (name, oRet);

				_Response.OnCookieAdd (oRet);
			}

			return oRet;
		}

		public string GetKey (int index)
		{
			return BaseGetKey (index);
		}

		public void Remove (string name)
		{
			if (null != _Response)
				_Response.GoingToChangeCookieColl ();

			_AllCookies = null;
			_AllKeys = null;
			BaseRemove (name);

			if (null != _Response)
				_Response.ChangedCookieColl ();
		}

		public void Set (HttpCookie cookie)
		{
			if (null != _Response)
				_Response.GoingToChangeCookieColl ();

			_AllCookies = null;
			_AllKeys = null;
			BaseSet (cookie.Name, cookie);

			if (null != _Response)
				_Response.ChangedCookieColl();
		}

		internal void MakeCookieExpire (string name, string path)
		{
			DateTime expirationTime = new DateTime (1999, 10, 12); // This is the date MS sends!
			HttpCookie willExpire = new HttpCookie (name, String.Empty, path, expirationTime);
			Remove (name);
			Add (willExpire);
		}
	}
}

