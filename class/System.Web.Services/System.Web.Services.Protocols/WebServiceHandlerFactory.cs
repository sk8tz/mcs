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

using System.IO;
using System.Net;
using System.Web.Services;
using System.Web.Services.Configuration;
using System.Web.SessionState;
using System.Web.UI;
using System.Collections.Specialized;
#if NET_2_0
using WSConfig = System.Web.Services.Configuration.WebServicesSection;
using WSProtocol = System.Web.Services.Configuration.WebServiceProtocols;
#endif

namespace System.Web.Services.Protocols
{
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
#if NET_2_0
			context.Items ["WebServiceSoapVersion"] =
				protocol == WSProtocol.HttpSoap12 ?
				SoapProtocolVersion.Soap12 :
				SoapProtocolVersion.Default;
#endif
			bool supported = false;
			IHttpHandler handler = null;

			supported = WSConfig.IsSupported (protocol);
			if (!supported) {
				switch (protocol) {
					case WSProtocol.HttpSoap:
						supported = WSConfig.IsSupported (WSProtocol.HttpSoap12);
						break;
					case WSProtocol.HttpPost:
						if (WSConfig.IsSupported (WSProtocol.HttpPostLocalhost)) {
							string localAddr = context.Request.ServerVariables ["LOCAL_ADDR"];

							supported = localAddr != null &&
								(localAddr == context.Request.ServerVariables ["REMOTE_ADDR"] ||
								IPAddress.IsLoopback (IPAddress.Parse (localAddr)));
						}
						break;
				}
			}
			if (!supported)
				throw new InvalidOperationException ("Unsupported request format.");

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
