//
// System.Runtime.Remoting.Activation.AppDomainLevelActivator.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
//

using System;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Activation
{
	internal class AppDomainLevelActivator: IActivator
	{
		string _activationUrl;
		IActivator _next;

		public AppDomainLevelActivator(string activationUrl, IActivator next)
		{
			_activationUrl = activationUrl;
			_next = next;
		}

		public ActivatorLevel Level 
		{
			get { return ActivatorLevel.AppDomain; }
		}

		public IActivator NextActivator 
		{
			get { return _next; }
			set { _next = value; }
		}

		public IConstructionReturnMessage Activate (IConstructionCallMessage ctorCall)
		{
			IConstructionReturnMessage response;

			// Create the object by calling the remote activation service

			RemoteActivator remoteActivator = (RemoteActivator) RemotingServices.Connect (typeof (RemoteActivator), _activationUrl);
			ctorCall.Activator = ctorCall.Activator.NextActivator;

			response = remoteActivator.Activate (ctorCall);

			// Create the client identity for the remote object

			ObjRef objRef = (ObjRef) response.ReturnValue;
			if (RemotingServices.GetIdentityForUri (objRef.URI) != null)
				throw new RemotingException("Inconsistent state during activation; there may be two proxies for the same object");

			object proxy;
			
			// We pass null for proxyType because we don't really to attach the identity
			// to a proxy, we already have one.
			Identity identity = RemotingServices.GetOrCreateClientIdentity (objRef, null, out proxy);
			RemotingServices.SetMessageTargetIdentity (ctorCall, identity);
			return response;
		}
	}
}
