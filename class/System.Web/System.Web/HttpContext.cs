//
// System.Web.HttpContext.cs 
//
// Author:
//	Miguel de Icaza (miguel@novell.com)
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
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

using System.Collections;
using System.Configuration;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.Util;

namespace System.Web {
	
	public sealed class HttpContext : IServiceProvider {
		internal HttpWorkerRequest WorkerRequest;
		HttpApplication app_instance;
		HttpRequest request;
		HttpResponse response;
		HttpSessionState session_state;
		HttpServerUtility server;
		TraceContext trace_context;
		IHttpHandler handler;
		string error_page;
		bool skip_authorization = false;
		IPrincipal user;
		object errors;
		Hashtable items;
		object config_timeout;
		int timeout_possible;
		DateTime time_stamp = DateTime.UtcNow;
		
		public HttpContext (HttpWorkerRequest wr)
		{
			WorkerRequest = wr;
			request = new HttpRequest (WorkerRequest, this);
			response = new HttpResponse (WorkerRequest, this);
		}

		public HttpContext (HttpRequest request, HttpResponse response)
		{
			this.request = request;
			this.response = response;
			
		}

		public Exception [] AllErrors {
			get {
				if (errors == null)
					return null;

				if (errors is Exception){
					Exception [] all = new Exception [1];
					all [0] = (Exception) errors;
					return all;
				} 
				return (Exception []) (((ArrayList) errors).ToArray (typeof (Exception)));
			}
		}

		public HttpApplicationState Application {
			get {
				return HttpApplicationFactory.ApplicationState;
			}
		}

		public HttpApplication ApplicationInstance {
			get {
				return app_instance;
			}

			set {
				app_instance = value;
			}
			      
		}

		public Cache Cache {
			get {
				return HttpRuntime.Cache;
			}
		}

		//
		// The "Current" property is set just after we have constructed it with 
		// the 'HttpContext (HttpWorkerRequest)' constructor.
		//
		public static HttpContext Current {
			get {
				return (HttpContext) CallContext.GetData ("c");
			}

			set {
				CallContext.SetData ("c", value);
			}
		}

		public Exception Error {
			get {
				if (errors == null || (errors is Exception))
					return (Exception) errors;
				return (Exception) (((ArrayList) errors) [0]);
			}
		}

		public IHttpHandler Handler {
			get {
				return handler;
			}

			set {
				handler = value;
			}
		}

		public bool IsCustomErrorEnabled {
			get {
				CustomErrorsConfig cfg = null;
				try {
					cfg = (CustomErrorsConfig) GetConfig ("system.web/customErrors");
				} catch {
				}
				if (cfg == null)
					return false;

				if (cfg.Mode == CustomErrorMode.On)
					return true;

				return (cfg.Mode == CustomErrorMode.RemoteOnly) &&
					(Request.WorkerRequest.GetLocalAddress () != Request.UserHostAddress);
			}
		}
#if TARGET_JVM
		public bool IsDebuggingEnabled { get { return false; } }
#else
		public bool IsDebuggingEnabled {
			get {
				return CompilationConfiguration.GetInstance (this).Debug;
			}
		}
#endif
		public IDictionary Items {
			get {
				if (items == null)
					items = new Hashtable ();
				return items;
			}
		}

		public HttpRequest Request {
			get {
				return request;
			}
		}

		public HttpResponse Response {
			get {
				return response;
			}
		}

		public HttpServerUtility Server {
			get {
				if (server == null)
					server = new HttpServerUtility (this);
				return server;
			}
		}

		public HttpSessionState Session {
			get {
				return session_state;
			}
		}

		public bool SkipAuthorization {
			get {
				return skip_authorization;
			}

			[SecurityPermission (SecurityAction.Demand, ControlPrincipal = true)]
			set {
				skip_authorization = value;
			}
		}

		public DateTime Timestamp {
			get {
				return time_stamp.ToLocalTime ();
			}
		}
		
		public TraceContext Trace {
			get {
				if (trace_context == null)
					trace_context = new TraceContext (this);
				return trace_context;
			}
		}

		public IPrincipal User {
			get {
				return user;
			}

			[SecurityPermission (SecurityAction.Demand, ControlPrincipal = true)]
			set {
				user = value;
			}
		}

		public void AddError (Exception errorInfo)
		{
			if (errors == null){
				errors = errorInfo;
				return;
			}
			ArrayList l;
			if (errors is Exception){
				l = new ArrayList ();
				l.Add (errors);
				errors = l;
			} else 
				l = (ArrayList) errors;
			l.Add (errorInfo);
		}

		public void ClearError ()
		{
			errors = null;
		}

		public static object GetAppConfig (string name)
		{
			object o = ConfigurationSettings.GetConfig (name);

			return o;
		}

		public object GetConfig (string name)
		{
			return WebConfigurationSettings.GetConfig (name, this);
		}

		object IServiceProvider.GetService (Type service)
		{
			if (service == typeof (HttpWorkerRequest))
				return WorkerRequest;

			//
			// We return everything out of properties in case
			// they are dynamically computed in some form in the future.
			//
			if (service == typeof (HttpApplication))
				return ApplicationInstance;

			if (service == typeof (HttpRequest))
				return Request;

			if (service == typeof (HttpResponse))
				return Response;

			if (service == typeof (HttpSessionState))
				return Session;

			if (service == typeof (HttpApplicationState))
				return Application;

			if (service == typeof (IPrincipal))
				return User;

			if (service == typeof (Cache))
				return Cache;

			if (service == typeof (HttpContext))
				return Current;

			if (service == typeof (IHttpHandler))
				return Handler;

			if (service == typeof (HttpServerUtility))
				return Server;
			
			if (service == typeof (TraceContext))
				return Trace;
			
			return null;
		}

		public void RewritePath (string path)
		{
			int qmark = path.IndexOf ('?');
			if (qmark != -1)
				RewritePath (path.Substring (0, qmark), "", path.Substring (qmark + 1));
			else
				RewritePath (path, null, null);
		}

		public void RewritePath (string filePath, string pathInfo, string queryString)
		{
			filePath = UrlUtils.Combine (Request.BaseVirtualDir, filePath);
			if (!filePath.StartsWith (HttpRuntime.AppDomainAppVirtualPath))
				throw new HttpException (404, "The virtual path '" + filePath +
					"' maps to another application.");

			Request.SetCurrentExePath (filePath);
			// A null pathInfo or queryString is ignored and previous values remain untouched
			if (pathInfo != null)
				Request.SetPathInfo (pathInfo);

			if (queryString != null)
				Request.QueryStringRaw = queryString;
		}

#region internals
		
		internal void SetSession (HttpSessionState state)
		{
			session_state = state;
		}

		// URL of a page used for error redirection.
		internal string ErrorPage {
			get {
				return error_page;
			}

			set {
				error_page = value;
			}
		}

		internal TimeSpan ConfigTimeout {
			get {
				if (config_timeout == null) {
					HttpRuntimeConfig config = (HttpRuntimeConfig)
								GetConfig ("system.web/httpRuntime");
					config_timeout = new TimeSpan (0, 0, config.ExecutionTimeout);
				}

				return (TimeSpan) config_timeout;
			}

			set {
				config_timeout = value;
			}
		}

		internal bool CheckIfTimeout (DateTime t)
		{
			if (Interlocked.CompareExchange (ref timeout_possible, 0, 0) == 0)
				return false;

			TimeSpan ts = t - time_stamp;
			return (ts > ConfigTimeout);
		}

		internal bool TimeoutPossible {
			get { return (Interlocked.CompareExchange (ref timeout_possible, 1, 1) == 1); }
		}

		internal void BeginTimeoutPossible ()
		{
			timeout_possible = 1;
		}

		internal void EndTimeoutPossible ()
		{
			Interlocked.CompareExchange (ref timeout_possible, 0, 1);
		}
#endregion

#if NET_2_0
		Page last_page;
		
		internal Page LastPage {
			get {
				return last_page;
			}

			set {
				last_page = value;
			}
		}
#endif
	}
}
