// 
// System.Web.Services.Protocols.WebServiceHandlerFactory.cs
//
// Authors:
// 	Tim Coleman (tim@timcoleman.com)
//	Dave Bettin (dave@opendotnet.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
// Copyright (c) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System.IO;
using System.Web.Services;
using System.Web.Services.Configuration;
using System.Web.SessionState;
using System.Web.UI;
using System.Collections.Specialized;

namespace System.Web.Services.Protocols
{
	class DummyHttpHandler : IHttpHandler
	{
		bool IHttpHandler.IsReusable {
			get { return false; }
		}

		void IHttpHandler.ProcessRequest (HttpContext context)
		{
			// Do nothing
		}
	}
	
	class SessionWrapperHandler : IHttpHandler, IRequiresSessionState
	{
		IHttpHandler handler;

		public SessionWrapperHandler (IHttpHandler handler)
		{
			this.handler = handler;
		}
		
		public bool IsReusable {
			get { return handler.IsReusable; }
		}

		public void ProcessRequest (HttpContext context)
		{
			handler.ProcessRequest (context);
		}
	}

	class ReadOnlySessionWrapperHandler : IHttpHandler, IRequiresSessionState, IReadOnlySessionState
	{
		IHttpHandler handler;

		public ReadOnlySessionWrapperHandler (IHttpHandler handler)
		{
			this.handler = handler;
		}
		
		public bool IsReusable {
			get { return handler.IsReusable; }
		}

		public void ProcessRequest (HttpContext context)
		{
			handler.ProcessRequest (context);
		}
	}
	public class WebServiceHandlerFactory : IHttpHandlerFactory
	{

		#region Constructors

		public WebServiceHandlerFactory () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		public IHttpHandler GetHandler (HttpContext context, string verb, string url, string filePath)
		{
			Type type = WebServiceParser.GetCompiledType (filePath, context);

			WSProtocol protocol = GuessProtocol (context, verb);
			IHttpHandler handler = null;

			if (!WSConfig.IsSupported (protocol))
				return new DummyHttpHandler ();

			switch (protocol) {
			case WSProtocol.HttpSoap:
				handler = GetTypeHandler (context, new HttpSoapWebServiceHandler (type));
				break;
			case WSProtocol.HttpPost:
			case WSProtocol.HttpGet:
				handler = GetTypeHandler (context, new HttpSimpleWebServiceHandler (type, protocol.ToString ()));
				break;
			case WSProtocol.Documentation:
				SoapDocumentationHandler soapHandler;
				soapHandler = new SoapDocumentationHandler (type, context);
				if (soapHandler.PageHandler is IRequiresSessionState) {
					if (soapHandler.PageHandler is IReadOnlySessionState)
						handler = new ReadOnlySessionWrapperHandler (soapHandler);
					else
						handler = new SessionWrapperHandler (soapHandler);
				} else {
					handler = soapHandler;
				}
				break;
			}

			return handler;
		}
		
		IHttpHandler GetTypeHandler (HttpContext context, WebServiceHandler handler)
		{
			MethodStubInfo method = handler.GetRequestMethod (context);
			if (method == null) return null;
			
			if (method.MethodInfo.EnableSession)
				return new SessionWrapperHandler (handler);
			else
				return handler;
		}

		static WSProtocol GuessProtocol (HttpContext context, string verb)
		{
			if (context.Request.PathInfo == null || context.Request.PathInfo == "")
			{
				if (context.Request.RequestType == "GET")
					return WSProtocol.Documentation;
				else
					return WSProtocol.HttpSoap;
			}
			else
			{
				if (context.Request.RequestType == "GET")
					return WSProtocol.HttpGet;
				else
					return WSProtocol.HttpPost;
			}
		}

		public void ReleaseHandler (IHttpHandler handler)
		{
		}

		#endregion // Methods
	}
}
