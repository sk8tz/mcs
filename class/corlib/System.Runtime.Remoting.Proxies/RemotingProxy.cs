//
// System.Runtime.Remoting.Proxies.RemotingProxy.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Contexts;
using System.Runtime.CompilerServices;
using System.Threading;


namespace System.Runtime.Remoting.Proxies
{

	public class RemotingProxy : RealProxy 
	{
		static MethodInfo _cache_GetTypeMethod = typeof(System.Object).GetMethod("GetType");
		static MethodInfo _cache_GetHashCodeMethod = typeof(System.Object).GetMethod("GetHashCode");

		IMessageSink _sink;
		bool _hasEnvoySink;
		ConstructionCall _ctorCall;
		string _targetUri;

		internal RemotingProxy (Type type, ClientIdentity identity) : base (type, identity)
		{
			_sink = identity.ChannelSink;
			_hasEnvoySink = false;
			_targetUri = identity.TargetUri;
		}

		internal RemotingProxy (Type type, string activationUrl, object[] activationAttributes) : base (type)
		{
			_hasEnvoySink = false;
			_ctorCall = ActivationServices.CreateConstructionCall (type, activationUrl, activationAttributes);
		}

		public override IMessage Invoke (IMessage request)
		{
			MonoMethodMessage mMsg = (MonoMethodMessage) request;

			if (mMsg.CallType == CallType.EndInvoke)
				return mMsg.AsyncResult.EndInvoke ();
				
			if (mMsg.MethodBase.IsConstructor)
				return ActivateRemoteObject (mMsg);

			if (mMsg.MethodBase == _cache_GetHashCodeMethod)
				return new MethodResponse(ObjectIdentity.GetHashCode(), null, null, request as IMethodCallMessage);

			if (mMsg.MethodBase == _cache_GetTypeMethod)
				return new MethodResponse(GetProxiedType(), null, null, request as IMethodCallMessage);

			mMsg.Uri = _targetUri;
			((IInternalMessage)mMsg).TargetIdentity = _objectIdentity;

			_objectIdentity.NotifyClientDynamicSinks (true, request, true, false);

			IMessage response;
			IMessageSink sink;

			// Needs to go through the client context sink?
			if (Thread.CurrentContext.HasExitSinks && !_hasEnvoySink)
				sink = Thread.CurrentContext.GetClientContextSinkChain ();
			else
				sink = _sink;

			if (mMsg.CallType == CallType.Sync)
				response = sink.SyncProcessMessage (request);
			else
			{
				AsyncResult ares = ((MonoMethodMessage)request).AsyncResult;
				IMessageCtrl mctrl = sink.AsyncProcessMessage (request, ares);
				if (ares != null) ares.SetMessageCtrl (mctrl);
				response = new ReturnMessage (null, new object[0], 0, null, mMsg);
			}

			_objectIdentity.NotifyClientDynamicSinks (false, request, true, false);

			return response;
		}

		internal void AttachIdentity (Identity identity)
		{
			_objectIdentity = identity;

			if (identity is ClientActivatedIdentity)	// It is a CBO
			{
				ClientActivatedIdentity cai = (ClientActivatedIdentity)identity;
				_targetContext = cai.Context;
				AttachServer (cai.GetServerObject ());
			}

			if (identity is ClientIdentity)
			{
				((ClientIdentity)identity).ClientProxy = (MarshalByRefObject) GetTransparentProxy();
				_targetUri = ((ClientIdentity)identity).TargetUri;
			}
			else
				_targetUri = identity.ObjectUri;

			if (_objectIdentity.EnvoySink != null)
			{
				_sink = _objectIdentity.EnvoySink;
				_hasEnvoySink = true;
			}
			else 
				_sink = _objectIdentity.ChannelSink;

			_ctorCall = null;	// Object already constructed
		}

		IMessage ActivateRemoteObject (IMethodMessage request)
		{
			if (_ctorCall == null)	// It must be a WKO
				return new ReturnMessage (this, new object[0], 0, null, (IMethodCallMessage) request);	// Ignore constructor call for WKOs

			_ctorCall.CopyFrom (request);
			return ActivationServices.Activate (this, _ctorCall);
		}
		
		~RemotingProxy()
		{
			if (_objectIdentity != null)
			{
				if (!(_objectIdentity is ClientActivatedIdentity))	// Local CBO proxy?
					RemotingServices.DisposeIdentity (_objectIdentity);
			}
		}
		
	}
}
