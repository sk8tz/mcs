//
// SvcHttpHandler.cs
//
// Author:
//	Ankit Jain  <jankit@novell.com>
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006,2009 Novell, Inc.  http://www.novell.com
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
using System.Linq;
using System.Web;
using System.Threading;

using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace System.ServiceModel.Channels {

	internal class SvcHttpHandler : IHttpHandler
	{
		Type type;
		Type factory_type;
		string path;
		Uri request_url;
		ServiceHostBase host;
		Queue<HttpContext> pending = new Queue<HttpContext> ();
		int close_state;

		AutoResetEvent process_request_wait = new AutoResetEvent (false);
		AutoResetEvent listening = new AutoResetEvent (false);

		public SvcHttpHandler (Type type, Type factoryType, string path)
		{
			this.type = type;
			this.factory_type = factoryType;
			this.path = path;
		}

		public bool IsReusable 
		{
			get { return true; }
		}

		public ServiceHostBase Host {
			get { return host; }
		}

		public HttpContext WaitForRequest (TimeSpan timeout)
		{
			if (close_state > 0)
				return null;
			DateTime start = DateTime.Now;

			if (close_state > 0)
				return null;
			if (pending.Count == 0) {
				if (!process_request_wait.WaitOne (timeout - (DateTime.Now - start), false) || close_state > 0)
					return null;
			}
			HttpContext ctx;
			lock (pending) {
				if (pending.Count == 0)
					return null;
				ctx = pending.Dequeue ();
			}
			if (ctx.AllErrors != null && ctx.AllErrors.Length > 0)
				return WaitForRequest (timeout - (DateTime.Now - start));
			return ctx;
		}

		public void ProcessRequest (HttpContext context)
		{
			request_url = context.Request.Url;
			EnsureServiceHost ();
			pending.Enqueue (context);
			process_request_wait.Set ();

			if (close_state == 0)
				listening.WaitOne ();
		}

		public void EndRequest (HttpContext context)
		{
			listening.Set ();
		}

		// called from SvcHttpHandlerFactory's remove callback (i.e.
		// unloading asp.net). It closes ServiceHost, then the host
		// in turn closes the listener and the channels it opened.
		// The channel listener calls CloseServiceChannel() to stop
		// accepting further requests on its shutdown.
		public void Close ()
		{
			host.Close ();
			host = null;
		}

		// called from AspNetChannelListener.Close() or .Abort().
		public void CloseServiceChannel ()
		{
			if (close_state > 0)
				return;
			close_state = 1;
			process_request_wait.Set ();
			listening.Set ();
			close_state = 2;
		}

		void EnsureServiceHost ()
		{
			if (host != null)
				return;

			//ServiceHost for this not created yet
			var baseUri = new Uri (HttpContext.Current.Request.Url.GetLeftPart (UriPartial.Path));
			if (factory_type != null) {
				host = ((ServiceHostFactory) Activator.CreateInstance (factory_type)).CreateServiceHost (type, new Uri [] {baseUri});
			}
			else
				host = new ServiceHost (type, baseUri);
			host.Extensions.Add (new VirtualPathExtension (baseUri.AbsolutePath));

			host.Open ();
		}
	}
}
