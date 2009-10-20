//
// System.Web.DefaultHttpHandler
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
//
// Copyright (C) 2006-2009 Novell, Inc (http://www.novell.com)
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

namespace System.Web
{
	public class DefaultHttpHandler : IHttpAsyncHandler
	{
		protected HttpContext Context {
			get { return null; }
		}

		public virtual bool IsReusable {
			get { return false; }
		}

		[MonoTODO("Not implemented, always returns null")]
		protected NameValueCollection ExecuteUrlHeaders {
			get { return null; }
		}

		[MonoTODO("Not implemented, always returns null")]
		public virtual IAsyncResult BeginProcessRequest (HttpContext context, AsyncCallback callback, object state)
		{
			return null;
		}

		[MonoTODO("Not implemented, does nothing")]
		public virtual void EndProcessRequest (IAsyncResult result)
		{
		}

		[MonoTODO("Not implemented, does nothing")]
		public virtual void ProcessRequest (HttpContext context)
		{
		}

		public virtual void OnExecuteUrlPreconditionFailure ()
		{
		}

		public virtual string OverrideExecuteUrlPath ()
		{
			return null;
		}
	}
}

