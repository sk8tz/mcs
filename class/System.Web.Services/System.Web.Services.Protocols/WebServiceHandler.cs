// 
// System.Web.Services.Protocols.WebServiceHandler.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Reflection;
using System.Web;
using System.Web.Services;
using System.Web.SessionState;

namespace System.Web.Services.Protocols 
{
	internal class WebServiceHandler: IHttpHandler 
	{
		Type _type;
		HttpContext _context;
		HttpSessionState session;

		
		public WebServiceHandler (Type type)
		{
			_type = type;
		}

		public Type ServiceType
		{
			get { return _type; }
		}
		
		public virtual bool IsReusable 
		{
			get { return false; }
		}

		protected HttpContext Context {
			set { _context = value; }
		}

		protected HttpSessionState Session {
			set { this.session = value; }
		}

		internal virtual MethodStubInfo GetRequestMethod (HttpContext context)
		{
			return null;
		}
		
		public virtual void ProcessRequest (HttpContext context)
		{
		}
		
		protected object CreateServerInstance ()
		{
			object ws = Activator.CreateInstance (ServiceType);
			WebService wsi = ws as WebService;
			if (wsi != null) {
				wsi.SetContext (_context);
			}

			return ws;
		}
	}
}

