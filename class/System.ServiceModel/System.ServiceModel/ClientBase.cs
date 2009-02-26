//
// generic ClientBase.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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
using System.ComponentModel;
using System.Reflection;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;

namespace System.ServiceModel
{
	[MonoTODO ("It somehow rejects classes, but dunno how we can do that besides our code wise.")]
	public abstract class ClientBase<TChannel>
		: IDisposable, ICommunicationObject where TChannel : class
	{
		static InstanceContext initialContxt = new InstanceContext (null);

		ChannelFactory<TChannel> factory;
		ChannelBase<TChannel> inner_channel;
		CommunicationState state;

		protected delegate IAsyncResult BeginOperationDelegate (object[] inValues, AsyncCallback asyncCallback, object state);
		protected delegate object[] EndOperationDelegate (IAsyncResult result);

		protected ClientBase ()
			: this (initialContxt)
		{
		}

		protected ClientBase (string endpointConfigurationName)
			: this (initialContxt, endpointConfigurationName)
		{
		}

		protected ClientBase (Binding binding, EndpointAddress remoteAddress)
			: this (initialContxt, binding, remoteAddress)
		{
		}

		protected ClientBase (string endpointConfigurationName, EndpointAddress remoteAddress)
			: this (initialContxt, endpointConfigurationName, remoteAddress)
		{
		}

		protected ClientBase (string endpointConfigurationName, string remoteAddress)
			: this (initialContxt, endpointConfigurationName, remoteAddress)
		{
		}

		protected ClientBase (InstanceContext instance)
			: this (instance, "*")
		{
		}

		protected ClientBase (InstanceContext instance, string endpointConfigurationName)
		{
			if (instance == null)
				throw new ArgumentNullException ("instanceContext");
			if (endpointConfigurationName == null)
				throw new ArgumentNullException ("endpointConfigurationName");

			Initialize (instance, endpointConfigurationName, null);
		}

		protected ClientBase (InstanceContext instance,
			string endpointConfigurationName, EndpointAddress remoteAddress)
		{
			if (instance == null)
				throw new ArgumentNullException ("instanceContext");
			if (endpointConfigurationName == null)
				throw new ArgumentNullException ("endpointConfigurationName");
			if (remoteAddress == null)
				throw new ArgumentNullException ("remoteAddress");

			Initialize (instance, endpointConfigurationName, remoteAddress);
		}

		protected ClientBase (InstanceContext instance,
			string endpointConfigurationName, string remoteAddress)
		{
			if (instance == null)
				throw new ArgumentNullException ("instanceContext");
			if (remoteAddress == null)
				throw new ArgumentNullException ("endpointAddress");
			if (endpointConfigurationName == null)
				throw new ArgumentNullException ("endpointConfigurationName");

			Initialize (instance, endpointConfigurationName, new EndpointAddress (remoteAddress));
		}

		protected ClientBase (InstanceContext instance,
			Binding binding, EndpointAddress remoteAddress)
		{
			if (instance == null)
				throw new ArgumentNullException ("instanceContext");
			if (binding == null)
				throw new ArgumentNullException ("binding");
			if (remoteAddress == null)
				throw new ArgumentNullException ("remoteAddress");

			Initialize (instance, binding, remoteAddress);
		}

		internal ClientBase (ChannelFactory<TChannel> factory)
		{
			this.factory = factory;
		}

		void Initialize (InstanceContext instance,
			string endpointConfigurationName, EndpointAddress remoteAddress)
		{
			factory = new ChannelFactory<TChannel> (endpointConfigurationName, remoteAddress);
			factory.OwnerClientBase = this;
		}

		void Initialize (InstanceContext instance,
			Binding binding, EndpointAddress remoteAddress)
		{
			factory = new ChannelFactory<TChannel> (binding, remoteAddress);
			factory.OwnerClientBase = this;
		}

		public ChannelFactory<TChannel> ChannelFactory {
			get { return factory; }
		}

#if !NET_2_1
		public ClientCredentials ClientCredentials {
			get { return ChannelFactory.Credentials; }
		}
#endif

		public ServiceEndpoint Endpoint {
			get { return factory.Endpoint; }
		}

		public IClientChannel InnerChannel {
			get {
				if (inner_channel == null)
					// FIXME: "factory." might be extraneous.
					inner_channel = (ChannelBase<TChannel>) (object) ChannelFactory.CreateChannel ();
				return inner_channel;
			}
		}

		protected TChannel Channel {
			get { return (TChannel) (object) InnerChannel; }
		}

		public CommunicationState State {
			get { return InnerChannel.State; }
		}

		public void Abort ()
		{
			InnerChannel.Abort ();
		}

		public void Close ()
		{
			InnerChannel.Close ();
		}

		public void DisplayInitializationUI ()
		{
			InnerChannel.DisplayInitializationUI ();
		}

#if NET_2_1
		protected T GetDefaultValueForInitialization<T> ()
		{
			return default (T);
		}

		IAsyncResult delegate_async;

		protected void InvokeAsync (BeginOperationDelegate beginOperationDelegate,
			object [] inValues, EndOperationDelegate endOperationDelegate,
			SendOrPostCallback operationCompletedCallback, object userState)
		{
			if (beginOperationDelegate == null)
				throw new ArgumentNullException ("beginOperationDelegate");
			if (endOperationDelegate == null)
				throw new ArgumentNullException ("endOperationDelegate");
			if (delegate_async != null)
				throw new InvalidOperationException ("Another async operation is in progress");

			AsyncCallback cb = delegate (IAsyncResult ar) {
				object [] results = null;
				Exception error = null;
				bool cancelled = false; // FIXME: fill it in case it is cancelled
				try {
					results = endOperationDelegate (ar);
				} catch (Exception ex) {
					error = ex;
				}
				try {
					if (operationCompletedCallback != null)
						operationCompletedCallback (new InvokeAsyncCompletedEventArgs (results, error, cancelled, userState));
				} catch (Exception ex) {
					Console.WriteLine ("Exception during operationCompletedCallback" + ex);
					throw;
				}
				Console.WriteLine ("System.ServiceModel.ClientBase<TChannel>: web service invocation is successfully done.");
			};
			begin_async_result = beginOperationDelegate (inValues, cb, userState);
		}
		IAsyncResult begin_async_result;
#endif
		
		void IDisposable.Dispose ()
		{
			Close ();
		}

		protected virtual TChannel CreateChannel ()
		{
			return ChannelFactory.CreateChannel ();
		}

		public void Open ()
		{
			InnerChannel.Open ();
		}

		#region ICommunicationObject implementation

		IAsyncResult ICommunicationObject.BeginOpen (
			AsyncCallback callback, object state)
		{
			return InnerChannel.BeginOpen (callback, state);
		}

		IAsyncResult ICommunicationObject.BeginOpen (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return InnerChannel.BeginOpen (timeout, callback, state);
		}

		void ICommunicationObject.EndOpen (IAsyncResult result)
		{
			InnerChannel.EndOpen (result);
		}

		IAsyncResult ICommunicationObject.BeginClose (
			AsyncCallback callback, object state)
		{
			return InnerChannel.BeginClose (callback, state);
		}

		IAsyncResult ICommunicationObject.BeginClose (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return InnerChannel.BeginClose (timeout, callback, state);
		}

		void ICommunicationObject.EndClose (IAsyncResult result)
		{
			InnerChannel.EndClose (result);
		}

		void ICommunicationObject.Close (TimeSpan timeout)
		{
			InnerChannel.Close (timeout);
		}

		void ICommunicationObject.Open (TimeSpan timeout)
		{
			InnerChannel.Open (timeout);
		}

		event EventHandler ICommunicationObject.Opening {
			add { InnerChannel.Opening += value; }
			remove { InnerChannel.Opening -= value; }
		}
		event EventHandler ICommunicationObject.Opened {
			add { InnerChannel.Opened += value; }
			remove { InnerChannel.Opened -= value; }
		}
		event EventHandler ICommunicationObject.Closing {
			add { InnerChannel.Closing += value; }
			remove { InnerChannel.Closing -= value; }
		}
		event EventHandler ICommunicationObject.Closed {
			add { InnerChannel.Closed += value; }
			remove { InnerChannel.Closed -= value; }
		}
		event EventHandler ICommunicationObject.Faulted {
			add { InnerChannel.Faulted += value; }
			remove { InnerChannel.Faulted -= value; }
		}

		#endregion

#if NET_2_1
		protected
#else
		internal
#endif
		class InvokeAsyncCompletedEventArgs : AsyncCompletedEventArgs
		{
			internal InvokeAsyncCompletedEventArgs (object [] results, Exception error, bool cancelled, object userState)
				: base (error, cancelled, userState)
			{
				Results = results;
			}

			public object [] Results { get; private set; }
		}

#if NET_2_1
		protected internal
#else
		internal
#endif
		class ChannelBase<T> : IClientChannel, IOutputChannel, IRequestChannel where T : class
		{
			ClientBase<T> client;
			ClientRuntimeChannel inner_channel;

			protected ChannelBase (ClientBase<T> client)
			{
				this.client = client;
			}

			internal ClientRuntimeChannel Inner {
				get {
					if (inner_channel == null)
						inner_channel = new ClientRuntimeChannel (client.Endpoint.CreateRuntime (), client.ChannelFactory);
					return inner_channel;
				}
			}

#if !NET_2_1
			public object Invoke (string methodName, object [] args)
			{
				var cd = client.Endpoint.Contract;
				var od = cd.Operations.Find (methodName);
				if (od == null)
					throw new ArgumentException (String.Format ("Operation '{0}' not found in the service contract '{1}' in namespace '{2}'", methodName, cd.Name, cd.Namespace));
				return Inner.Process (od.SyncMethod, methodName, args);
			}
#endif

			protected IAsyncResult BeginInvoke (string methodName, object [] args, AsyncCallback callback, object state)
			{
				var cd = client.Endpoint.Contract;
				var od = cd.Operations.Find (methodName);
				if (od == null)
					throw new ArgumentException (String.Format ("Operation '{0}' not found in the service contract '{1}' in namespace '{2}'", methodName, cd.Name, cd.Namespace));
				return Inner.BeginProcess (od.BeginMethod, methodName, args, callback, state);
			}

			protected object EndInvoke (string methodName, object [] args, IAsyncResult result)
			{
				var cd = client.Endpoint.Contract;
				var od = cd.Operations.Find (methodName);
				if (od == null)
					throw new ArgumentException (String.Format ("Operation '{0}' not found in the service contract '{1}' in namespace '{2}'", methodName, cd.Name, cd.Namespace));
				return Inner.EndProcess (od.EndMethod, methodName, args, result);
			}

			#region ICommunicationObject

			IAsyncResult ICommunicationObject.BeginClose (AsyncCallback callback, object state)
			{
				return Inner.BeginClose (callback, state);
			}

			IAsyncResult ICommunicationObject.BeginClose (TimeSpan timeout, AsyncCallback callback, object state)
			{
				return Inner.BeginClose (timeout, callback, state);
			}

			void ICommunicationObject.Close ()
			{
				Inner.Close ();
			}

			void ICommunicationObject.Close (TimeSpan timeout)
			{
				Inner.Close (timeout);
			}

			IAsyncResult ICommunicationObject.BeginOpen (AsyncCallback callback, object state)
			{
				return Inner.BeginOpen (callback, state);
			}

			IAsyncResult ICommunicationObject.BeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
			{
				return Inner.BeginOpen (timeout, callback, state);
			}

			void ICommunicationObject.Open ()
			{
				Inner.Open ();
			}

			void ICommunicationObject.Open (TimeSpan timeout)
			{
				Inner.Open (timeout);
			}

			void ICommunicationObject.Abort ()
			{
				Inner.Abort ();
			}

			void ICommunicationObject.EndClose (IAsyncResult result)
			{
				Inner.EndClose (result);
			}

			void ICommunicationObject.EndOpen (IAsyncResult result)
			{
				Inner.EndOpen (result);
			}

			CommunicationState ICommunicationObject.State {
				get { return Inner.State; }
			}

			event EventHandler ICommunicationObject.Opened {
				add { Inner.Opened += value; }
				remove { Inner.Opened -= value; }
			}

			event EventHandler ICommunicationObject.Opening {
				add { Inner.Opening += value; }
				remove { Inner.Opening -= value; }
			}

			event EventHandler ICommunicationObject.Closed {
				add { Inner.Closed += value; }
				remove { Inner.Closed -= value; }
			}

			event EventHandler ICommunicationObject.Closing {
				add { Inner.Closing += value; }
				remove { Inner.Closing -= value; }
			}

			event EventHandler ICommunicationObject.Faulted {
				add { Inner.Faulted += value; }
				remove { Inner.Faulted -= value; }
			}

			#endregion

			#region IClientChannel

			public bool AllowInitializationUI {
				get { return Inner.AllowInitializationUI; }
				set { Inner.AllowInitializationUI = value; }
			}

			public bool DidInteractiveInitialization {
				get { return Inner.DidInteractiveInitialization; }
			}

			public Uri Via {
				get { return Inner.Via; }
			}

			public IAsyncResult BeginDisplayInitializationUI (
				AsyncCallback callback, object state)
			{
				return Inner.BeginDisplayInitializationUI (callback, state);
			}

			public void EndDisplayInitializationUI (
				IAsyncResult result)
			{
				Inner.EndDisplayInitializationUI (result);
			}

			public void DisplayInitializationUI ()
			{
				Inner.DisplayInitializationUI ();
			}

			public void Dispose ()
			{
				Inner.Dispose ();
			}

			public event EventHandler<UnknownMessageReceivedEventArgs> UnknownMessageReceived {
				add { Inner.UnknownMessageReceived += value; }
				remove { Inner.UnknownMessageReceived -= value; }
			}

			#endregion

			#region IContextChannel

			[MonoTODO]
			public bool AllowOutputBatching {
				get { return Inner.AllowOutputBatching; }

				set { Inner.AllowOutputBatching = value; }
			}

			[MonoTODO]
			public IInputSession InputSession {
				get { return Inner.InputSession; }
			}

			public EndpointAddress LocalAddress {
				get { return Inner.LocalAddress; }
			}

			[MonoTODO]
			public TimeSpan OperationTimeout {
				get { return Inner.OperationTimeout; }
				set { Inner.OperationTimeout = value; }
			}

			[MonoTODO]
			public IOutputSession OutputSession {
				get { return Inner.OutputSession; }
			}

			public EndpointAddress RemoteAddress {
				get { return Inner.RemoteAddress; }
			}

			[MonoTODO]
			public string SessionId {
				get { return Inner.SessionId; }
			}

			#endregion

			#region IRequestChannel

			IAsyncResult IRequestChannel.BeginRequest (Message message, AsyncCallback callback, object state)
			{
				return ((IRequestChannel) this).BeginRequest (message, client.Endpoint.Binding.SendTimeout, callback, state);
			}

			IAsyncResult IRequestChannel.BeginRequest (Message message, TimeSpan timeout, AsyncCallback callback, object state)
			{
				return Inner.BeginRequest (message, timeout, callback, state);
			}

			Message IRequestChannel.EndRequest (IAsyncResult result)
			{
				return Inner.EndRequest (result);
			}

			Message IRequestChannel.Request (Message message)
			{
				return ((IRequestChannel) this).Request (message, client.Endpoint.Binding.SendTimeout);
			}

			Message IRequestChannel.Request (Message message, TimeSpan timeout)
			{
				return Inner.Request (message, timeout);
			}

			EndpointAddress IRequestChannel.RemoteAddress {
				get { return client.Endpoint.Address; }
			}

			Uri IRequestChannel.Via {
				get { return Via; }
			}

			#endregion

			#region IOutputChannel

			IAsyncResult IOutputChannel.BeginSend (Message message, AsyncCallback callback, object state)
			{
				return ((IOutputChannel) this).BeginSend (message, client.Endpoint.Binding.SendTimeout, callback, state);
			}

			IAsyncResult IOutputChannel.BeginSend (Message message, TimeSpan timeout, AsyncCallback callback, object state)
			{
				return Inner.BeginSend (message, timeout, callback, state);
			}

			void IOutputChannel.EndSend (IAsyncResult result)
			{
				Inner.EndSend (result);
			}

			void IOutputChannel.Send (Message message)
			{
				((IOutputChannel) this).Send (message, client.Endpoint.Binding.SendTimeout);
			}

			void IOutputChannel.Send (Message message, TimeSpan timeout)
			{
				Inner.Send (message, timeout);
			}

			#endregion

			IExtensionCollection<IContextChannel> IExtensibleObject<IContextChannel>.Extensions {
				get { return Inner.Extensions; }
			}

			TProperty IChannel.GetProperty<TProperty> ()
			{
				return Inner.GetProperty<TProperty> ();
			}
		}
	}
}
