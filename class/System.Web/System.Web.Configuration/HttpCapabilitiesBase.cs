//
// System.Web.Configuration.HttpCapabilitiesBase
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (C) 2003,2004 Novell, Inc (http://www.novell.com)
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

namespace System.Web.Configuration
{
	using System;
	using System.Collections;
	using System.Web;
	
	public partial class HttpCapabilitiesBase
	{
		Hashtable capabilities;

		public HttpCapabilitiesBase () { }

		public virtual string this [string key]
		{
			get { return capabilities [key] as string; }
		}

		public static HttpCapabilitiesBase GetConfigCapabilities (string configKey, HttpRequest request)
		{
			string ua = request.ClientTarget;
			if (ua == null)
				ua = request.UserAgent;

			HttpBrowserCapabilities bcap = new HttpBrowserCapabilities ();
			bcap.useragent = ua;
			bcap.capabilities = CapabilitiesLoader.GetCapabilities (ua);
			bcap.Init ();
			return bcap;
		}

		protected virtual void Init ()
		{
		}
	}
}

