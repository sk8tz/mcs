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
using System.Web.Services;

namespace System.Web.Services.Protocols {
	internal class WebServiceHandler: IHttpHandler {

		public virtual bool IsReusable 
		{
			get { return false; }
		}

		public virtual void ProcessRequest (HttpContext context)
		{
		}

		[MonoTODO]
		protected IAsyncResult BeginCoreProcessRequest (AsyncCallback callback, object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void CoreProcessRequest ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void EndCoreProcessRequest (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected SoapServerMessage Invoke (SoapServerMessage requestMessage)
		{
			MethodStubInfo methodInfo = requestMessage.MethodStubInfo;

			// Assign header values to web service members

			requestMessage.UpdateHeaderValues (requestMessage.Server, methodInfo.Headers);

			// Fill an array with the input parameters at the right position

			object[] parameters = new object[methodInfo.MethodInfo.Parameters.Length];
			ParameterInfo[] inParams = methodInfo.MethodInfo.InParameters;
			for (int n=0; n<inParams.Length; n++)
				parameters [inParams[n].Position] = requestMessage.InParameters [n];

			// Invoke the method

			try
			{
				object[] results = methodInfo.MethodInfo.Invoke (requestMessage.Server, parameters);
				requestMessage.OutParameters = results;
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}

			// Check that headers with MustUnderstand flag have been understood
			
			foreach (SoapHeader header in requestMessage.Headers)
			{
				if (header.MustUnderstand && !header.DidUnderstand)
					throw new SoapHeaderException ("Header not understood: " + header.GetType(), SoapException.MustUnderstandFaultCode);
			}

			// Collect headers that must be sent to the client
			requestMessage.CollectHeaders (requestMessage.Server, methodInfo.Headers, SoapHeaderDirection.Out);

			return requestMessage;
		}

		[MonoTODO]
		private void WriteReturns (object[] returnValues)
		{
			//protocol.WriteReturns (returnValues, outputStream);
			throw new NotImplementedException ();
		}
	}
}
