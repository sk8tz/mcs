﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace MonoTests.System.ServiceModel.Dispatcher
{
	[TestFixture]
	public class ChannelDispatcherTest
	{
		[Test]			
		public void Collection_Add_Remove () {
			Console.WriteLine ("STart test Collection_Add_Remove");
			var uri = new Uri ("http://localhost:37564");
			ServiceHost h = new ServiceHost (typeof (TestContract), uri);
			h.AddServiceEndpoint (typeof (TestContract).FullName, new BasicHttpBinding (), "address");
			MyChannelDispatcher d = new MyChannelDispatcher (new MyChannelListener (uri));
			h.ChannelDispatchers.Add (d);
			Assert.IsTrue (d.Attached, "#1");
			h.ChannelDispatchers.Remove (d);
			Assert.IsFalse (d.Attached, "#2");
			h.ChannelDispatchers.Insert (0, d);
			Assert.IsTrue (d.Attached, "#3");
			h.ChannelDispatchers.Add (new MyChannelDispatcher (new MyChannelListener (uri)));
			h.ChannelDispatchers.Clear ();
			Assert.IsFalse (d.Attached, "#4");
		}

		[Test]
		public void EndpointDispatcherAddTest ()
		{
			var uri = new Uri ("http://localhost:8080");
			MyChannelDispatcher d = new MyChannelDispatcher (new MyChannelListener (uri));
			d.Endpoints.Add (new EndpointDispatcher (new EndpointAddress (uri), "", ""));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))] 
		public void EndpointDispatcherAddTest2 () {
			var uri = new Uri ("http://localhost:8080");
			MyChannelDispatcher d = new MyChannelDispatcher (new MyChannelListener (uri));
			d.Endpoints.Add (new EndpointDispatcher (new EndpointAddress (uri), "", ""));
			d.Open (); // the dispatcher must be attached.
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void EndpointDispatcherAddTest3 ()
		{
			var uri = new Uri ("http://localhost:37564");
			ServiceHost h = new ServiceHost (typeof (TestContract), uri);
			MyChannelDispatcher d = new MyChannelDispatcher (new MyChannelListener (uri));
			d.Endpoints.Add (new EndpointDispatcher (new EndpointAddress (uri), "", ""));
			h.ChannelDispatchers.Add (d);
			d.Open (); // missing MessageVersion
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))] // i.e. it is thrown synchronously in current thread.
		public void EndpointDispatcherAddTest4 ()
		{
			var uri = new Uri ("http://localhost:37564");
			ServiceHost h = new ServiceHost (typeof (TestContract), uri);
			var listener = new MyChannelListener (uri);
			MyChannelDispatcher d = new MyChannelDispatcher (listener);
			var ed = new EndpointDispatcher (new EndpointAddress (uri), "", "");
			Assert.IsNotNull (ed.DispatchRuntime, "#1");
			Assert.IsNull (ed.DispatchRuntime.InstanceProvider, "#2");
			Assert.IsNull (ed.DispatchRuntime.InstanceContextProvider, "#3");
			Assert.IsNull (ed.DispatchRuntime.SingletonInstanceContext, "#4");
			d.Endpoints.Add (ed);
			d.MessageVersion = MessageVersion.Default;
			h.ChannelDispatchers.Add (d);
			// it misses DispatchRuntime.Type, which seems set
			// automatically when the dispatcher is created in
			// ordinal process but need to be set manually in this case.
			try {
				d.Open ();
			} finally {
				Assert.AreEqual (CommunicationState.Opened, listener.State, "#5");
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))] // i.e. it is thrown synchronously in current thread.
		public void EndpointDispatcherAddTest5 ()
		{
			var uri = new Uri ("http://localhost:37564");
			ServiceHost h = new ServiceHost (typeof (TestContract), uri);
			var binding = new BasicHttpBinding ();
			var listener = new MyChannelListener (uri);
			MyChannelDispatcher d = new MyChannelDispatcher (listener);
			var ed = new EndpointDispatcher (new EndpointAddress (uri), "", "");
			d.Endpoints.Add (ed);

			ed.DispatchRuntime.Type = typeof (TestContract); // different from Test4

			d.MessageVersion = MessageVersion.Default;
			h.ChannelDispatchers.Add (d);
			// It rejects "unrecognized type" of the channel listener.
			// Test6 uses IChannelListener<IReplyChannel> and works.
			d.Open ();
		}

		[Test]
		public void EndpointDispatcherAddTest6 ()
		{
			var uri = new Uri ("http://localhost:37564");
			ServiceHost h = new ServiceHost (typeof (TestContract), uri);
			var binding = new BasicHttpBinding ();
			var listener = new MyChannelListener<IReplyChannel> (uri);
			MyChannelDispatcher d = new MyChannelDispatcher (listener);
			var ed = new EndpointDispatcher (new EndpointAddress (uri), "", "");
			d.Endpoints.Add (ed);

			ed.DispatchRuntime.Type = typeof (TestContract);

			d.MessageVersion = MessageVersion.Default;
			h.ChannelDispatchers.Add (d);
			d.Open (); // At this state, it does *not* call AcceptChannel() yet.
			Assert.IsFalse (listener.AcceptChannelTried, "#1");
			Assert.IsFalse (listener.WaitForChannelTried, "#2");

			Assert.IsNotNull (ed.DispatchRuntime, "#3");
			Assert.IsNull (ed.DispatchRuntime.InstanceProvider, "#4");
			Assert.IsNull (ed.DispatchRuntime.InstanceContextProvider, "#5"); // it is not set after ChannelDispatcher.Open().
			Assert.IsNull (ed.DispatchRuntime.SingletonInstanceContext, "#6");

			// d.Close (); // we don't have to even close it.
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void EndpointDispatcherAddTest7 ()
		{
			var uri = new Uri ("http://localhost:37564");
			ServiceHost h = new ServiceHost (typeof (TestContract), uri);
			var binding = new BasicHttpBinding ();
			var listener = new MyChannelListener<IReplyChannel> (uri);
			MyChannelDispatcher d = new MyChannelDispatcher (listener);
			var ed = new EndpointDispatcher (new EndpointAddress (uri), "", "");
			d.Endpoints.Add (ed);

			ed.DispatchRuntime.Type = typeof (TestContract);

			d.MessageVersion = MessageVersion.Default;

			// add service endpoint to open the host (unlike all tests above).
			h.AddServiceEndpoint (typeof (TestContract),
				new BasicHttpBinding (), uri.ToString ());
			h.ChannelDispatchers.Clear ();

			h.ChannelDispatchers.Add (d);
			d.Open (); // At this state, it does *not* call AcceptChannel() yet.

			// This rejects already-opened ChannelDispatcher.
			h.Open (TimeSpan.FromSeconds (10));
			// In case it is kept opened, it will block following tests, so close it explicitly.
			if (h.State == CommunicationState.Opened)
				h.Close ();
		}

		[Test]
		public void EndpointDispatcherAddTest8 ()
		{
			var uri = new Uri ("http://localhost:37564");
			ServiceHost h = new ServiceHost (typeof (TestContract), uri);
			var binding = new BasicHttpBinding ();
			var listener = new MyChannelListener<IReplyChannel> (uri);
			MyChannelDispatcher d = new MyChannelDispatcher (listener);
			var ed = new EndpointDispatcher (new EndpointAddress (uri), "", "");
			d.Endpoints.Add (ed);

			ed.DispatchRuntime.Type = typeof (TestContract);

			d.MessageVersion = MessageVersion.Default;

			// add service endpoint to open the host (unlike all tests above).
			h.AddServiceEndpoint (typeof (TestContract),
				new BasicHttpBinding (), uri.ToString ());
			h.ChannelDispatchers.Clear ();

			h.ChannelDispatchers.Add (d);

			Assert.AreEqual (h, d.Host, "#0");

			try {
				h.Open (TimeSpan.FromSeconds (10));
				Assert.IsTrue (listener.AcceptChannelTried, "#1"); // while it throws NIE ...
				Assert.IsFalse (listener.WaitForChannelTried, "#2");
				Assert.IsNotNull (ed.DispatchRuntime, "#3");
				Assert.IsNull (ed.DispatchRuntime.InstanceProvider, "#4");
				Assert.IsNotNull (ed.DispatchRuntime.InstanceContextProvider, "#5"); // it was set after ServiceHost.Open().
				Assert.IsNull (ed.DispatchRuntime.SingletonInstanceContext, "#6");
				/*
				var l = new HttpListener ();
				l.Prefixes.Add (uri.ToString ());
				l.Start ();
				l.Stop ();
				*/
			} finally {
				h.Close ();
			}
		}

		[ServiceContract]
		public class TestContract
		{
			[OperationContract]
			public void Process (string input) {
			}
		}

		class MyChannelDispatcher : ChannelDispatcher
		{
			public bool Attached = false;

			public MyChannelDispatcher (IChannelListener l) : base (l) { }
			protected override void Attach (ServiceHostBase host) {
				base.Attach (host);
				Attached = true;				
			}

			protected override void Detach (ServiceHostBase host) {
				base.Detach (host);
				Attached = false;				
			}
		}

		class MyChannelListener<TChannel> : MyChannelListener, IChannelListener<TChannel> where TChannel : class, IChannel
		{
			public MyChannelListener (Uri uri)
				: base (uri)
			{
			}

			public bool AcceptChannelTried { get; set; }

			public TChannel AcceptChannel ()
			{
				AcceptChannelTried = true;
				throw new NotImplementedException ();
			}

			public TChannel AcceptChannel (TimeSpan timeout)
			{
				AcceptChannelTried = true;
				throw new NotImplementedException ();
			}

			public IAsyncResult BeginAcceptChannel (AsyncCallback callback, object state)
			{
				AcceptChannelTried = true;
				throw new NotImplementedException ();
			}

			public IAsyncResult BeginAcceptChannel (TimeSpan timeout, AsyncCallback callback, object state)
			{
				AcceptChannelTried = true;
				throw new NotImplementedException ();
			}

			public TChannel EndAcceptChannel (IAsyncResult result)
			{
				throw new NotImplementedException ();
			}
		}

		class MyChannelListener : IChannelListener
		{
			public MyChannelListener (Uri uri)
			{
				Uri = uri;
			}

			public bool WaitForChannelTried { get; set; }

			public CommunicationState State { get; set; }

			#region IChannelListener Members

			public IAsyncResult BeginWaitForChannel (TimeSpan timeout, AsyncCallback callback, object state)
			{
				WaitForChannelTried = true;
				throw new NotImplementedException ();
			}

			public bool EndWaitForChannel (IAsyncResult result)
			{
				throw new NotImplementedException ();
			}

			public T GetProperty<T> () where T : class
			{
				throw new NotImplementedException ();
			}

			public Uri Uri { get; set; }

			public bool WaitForChannel (TimeSpan timeout)
			{
				WaitForChannelTried = true;
				throw new NotImplementedException ();
			}

			#endregion

			#region ICommunicationObject Members

			public void Abort ()
			{
				State = CommunicationState.Closed;
			}

			public IAsyncResult BeginClose (TimeSpan timeout, AsyncCallback callback, object state) {
				throw new NotImplementedException ();
			}

			public IAsyncResult BeginClose (AsyncCallback callback, object state) {
				throw new NotImplementedException ();
			}

			public IAsyncResult BeginOpen (TimeSpan timeout, AsyncCallback callback, object state) {
				throw new NotImplementedException ();
			}

			public IAsyncResult BeginOpen (AsyncCallback callback, object state) {
				throw new NotImplementedException ();
			}

			public void Close (TimeSpan timeout)
			{
				State = CommunicationState.Closed;
			}

			public void Close ()
			{
				State = CommunicationState.Closed;
			}

			public event EventHandler Closed;

			public event EventHandler Closing;

			public void EndClose (IAsyncResult result) {
				throw new NotImplementedException ();
			}

			public void EndOpen (IAsyncResult result) {
				throw new NotImplementedException ();
			}

			public event EventHandler Faulted;

			public void Open (TimeSpan timeout)
			{
				State = CommunicationState.Opened;
			}

			public void Open () 
			{
				State = CommunicationState.Opened;
			}

			public event EventHandler Opened;

			public event EventHandler Opening;

			#endregion
		}
	}
}
