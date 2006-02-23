// System.Net.Sockets.TcpClient.cs
//
// Author:
// 	Phillip Pearson (pp@myelin.co.nz)
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (C) 2001, Phillip Pearson
//    http://www.myelin.co.nz
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
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

using System;
using System.Net;

namespace System.Net.Sockets
{
	public class TcpClient : IDisposable {
		enum Properties : uint {
			LingerState = 1,
			NoDelay = 2,
			ReceiveBufferSize = 4,
			ReceiveTimeout = 8,
			SendBufferSize = 16,
			SendTimeout = 32
		}

		// private data
		NetworkStream stream;
		bool active;
		Socket client;
		bool disposed;
		Properties values;
		int recv_timeout, send_timeout;
		int recv_buffer_size, send_buffer_size;
		LingerOption linger_state;
		bool no_delay;
		
		private void Init (AddressFamily family)
		{
			active = false;

			if(client != null) {
				client.Close();
				client = null;
			}

			client = new Socket(family, SocketType.Stream, ProtocolType.Tcp);
		}

		public TcpClient ()
		{
			Init(AddressFamily.InterNetwork);
			client.Bind(new IPEndPoint(IPAddress.Any, 0));
		}
	
#if NET_1_1
		public TcpClient (AddressFamily family)
		{
			if (family != AddressFamily.InterNetwork &&
			    family != AddressFamily.InterNetworkV6) {
				throw new ArgumentException ("Family must be InterNetwork or InterNetworkV6", "family");
			}
			
			Init (family);
			client.Bind (new IPEndPoint (IPAddress.Any, 0));
		}
#endif
		
		public TcpClient (IPEndPoint local_end_point)
		{
			Init(local_end_point.AddressFamily);
			client.Bind(local_end_point);
		}
		
		public TcpClient (string hostname, int port)
		{
			Connect(hostname, port);
		}
				
		protected bool Active {
			get { return active; }
			set { active = value; }
		}
		
#if NET_2_0
		public Socket Client {
#else
		protected Socket Client {
#endif
			get { return client; }
			set {
				client = value;
				stream = null;
			}
		}

#if NET_2_0
		public int Available {
			get { return client.Available; }
		}

		public bool Connected {
			get { return client.Connected; }
		}

		public bool ExclusiveAddressUse {
			get {
				return !((bool) client.GetSocketOption (SocketOptionLevel.Socket,
								SocketOptionName.ReuseAddress));
			}
			set {
				client.SetSocketOption (SocketOptionLevel.Socket,
							SocketOptionName.ReuseAddress, !value);
			}
		}
#endif
		internal void SetTcpClient (Socket s) 
		{
			Client = s;
		}

		public LingerOption LingerState {
			get {
				if ((values & Properties.LingerState) != 0)
					return linger_state;

				return (LingerOption) client.GetSocketOption (SocketOptionLevel.Socket,
									SocketOptionName.Linger);
			}
			set {
				if (!client.Connected) {
					linger_state = value;
					values |= Properties.LingerState;
					return;
				}
				client.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.Linger, value);
			}
		}
				
		public bool NoDelay {
			get {
				if ((values & Properties.NoDelay) != 0)
					return no_delay;

				return (bool)client.GetSocketOption(
					SocketOptionLevel.Tcp,
					SocketOptionName.NoDelay);
			}
			set {
				if (!client.Connected) {
					no_delay = value;
					values |= Properties.NoDelay;
					return;
				}
				client.SetSocketOption(
					SocketOptionLevel.Tcp,
					SocketOptionName.NoDelay, value ? 1 : 0);
			}
		}
				
		public int ReceiveBufferSize {
			get {
				if ((values & Properties.ReceiveBufferSize) != 0)
					return recv_buffer_size;

				return (int)client.GetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReceiveBuffer);
			}
			set {
				if (!client.Connected) {
					recv_buffer_size = value;
					values |= Properties.ReceiveBufferSize;
					return;
				}
				client.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReceiveBuffer, value);
			}
		}
			
		public int ReceiveTimeout {
			get {
				if ((values & Properties.ReceiveTimeout) != 0)
					return recv_timeout;

				return (int)client.GetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReceiveTimeout);
			}
			set {
				if (!client.Connected) {
					recv_timeout = value;
					values |= Properties.ReceiveTimeout;
					return;
				}
				client.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReceiveTimeout, value);
			}
		}
		
		public int SendBufferSize {
			get {
				if ((values & Properties.SendBufferSize) != 0)
					return send_buffer_size;

				return (int)client.GetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.SendBuffer);
			}
			set {
				if (!client.Connected) {
					send_buffer_size = value;
					values |= Properties.SendBufferSize;
					return;
				}
				client.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.SendBuffer, value);
			}
		}
		
		public int SendTimeout {
			get {
				if ((values & Properties.SendTimeout) != 0)
					return send_timeout;

				return (int)client.GetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.SendTimeout);
			}
			set {
				if (!client.Connected) {
					send_timeout = value;
					values |= Properties.SendTimeout;
					return;
				}
				client.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.SendTimeout, value);
			}
		}
		
		
		// methods
		
		public void Close ()
		{
			((IDisposable) this).Dispose ();
		}
		
		public void Connect (IPEndPoint remote_end_point)
		{
			try {
				client.Connect(remote_end_point);
				stream = new NetworkStream(client, true);
				active = true;
			} finally {
				CheckDisposed ();
			}
		}
		
		public void Connect (IPAddress address, int port)
		{
			Connect(new IPEndPoint(address, port));
		}

		void SetOptions ()
		{
			Properties props = values;
			values = 0;

			if ((props & Properties.LingerState) != 0)
				LingerState = linger_state;
			if ((props & Properties.NoDelay) != 0)
				NoDelay = no_delay;
			if ((props & Properties.ReceiveBufferSize) != 0)
				ReceiveBufferSize = recv_buffer_size;
			if ((props & Properties.ReceiveTimeout) != 0)
				ReceiveTimeout = recv_timeout;
			if ((props & Properties.SendBufferSize) != 0)
				SendBufferSize = send_buffer_size;
			if ((props & Properties.SendTimeout) != 0)
				SendTimeout = send_timeout;
		}

		public void Connect (string hostname, int port)
		{
			CheckDisposed ();

			IPHostEntry host = Dns.GetHostByName(hostname);

			for(int i=0; i<host.AddressList.Length; i++) {
				try {
					Init(host.AddressList[i].AddressFamily);

					if(host.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
						client.Bind(new IPEndPoint(IPAddress.Any, 0));
#if NET_1_1
					else if(host.AddressList[i].AddressFamily == AddressFamily.InterNetworkV6)
						client.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));
#endif

					Connect(new IPEndPoint(host.AddressList[i], port));
					if (values != 0)
						SetOptions ();
					break;
				} catch (Exception e) {
					if(client != null) {
						client.Close();
						client = null;
					}

					/// This is the last known address, re-throw the exception
					if(i == host.AddressList.Length-1)
						throw e;
				}
			}
		}
		
		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposed)
				return;
			disposed = true;

			if (disposing){
				// release managed resources
				NetworkStream s = stream;
				stream = null;
				if (s != null) {
					// This closes the socket as well, as the NetworkStream
					// owns the socket.
					s.Close();
					active = false;
					s = null;
				} else if (client != null){
					client.Close ();
				}
				client = null;
			}
		}
		
		~TcpClient ()
		{
			Dispose (false);
		}
		
		public NetworkStream GetStream()
		{
			try {
				if (stream == null)
					stream = new NetworkStream (client, true);
				return stream;
			}
			finally { CheckDisposed (); }
		}
		
		private void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType().FullName);
		}		
	}
}

