//
// System.Net.Sockets.UdpClient.cs
//
// Author:
//    Gonzalo Paniagua Javier <gonzalo@ximian.com>
//
// Copyright (C) Ximian, Inc. http://www.ximian.com
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
	public class UdpClient : IDisposable
	{
		private bool disposed = false;
		private bool active = false;
		private Socket socket;
		private AddressFamily family = AddressFamily.InterNetwork;
		
#region Constructors
		public UdpClient () : this(AddressFamily.InterNetwork)
		{
		}

#if NET_1_1
		public UdpClient(AddressFamily family)
		{
			if(family != AddressFamily.InterNetwork && family != AddressFamily.InterNetwork)
				throw new ArgumentException("family");

			this.family = family;
			InitSocket (null);
		}
#endif

		public UdpClient (int port)
		{
			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				throw new ArgumentOutOfRangeException ("port");

			this.family = AddressFamily.InterNetwork;

			IPEndPoint localEP = new IPEndPoint (IPAddress.Any, port);
			InitSocket (localEP);
		}

		public UdpClient (IPEndPoint localEP)
		{
			if (localEP == null)
				throw new ArgumentNullException ("localEP");

			this.family = localEP.AddressFamily;

			InitSocket (localEP);
		}

#if NET_1_1
		public UdpClient (int port, AddressFamily family)
		{
			if (family != AddressFamily.InterNetwork &&
			    family != AddressFamily.InterNetworkV6) {
				throw new ArgumentException ("Family must be InterNetwork or InterNetworkV6", "family");
			}
			
			if (port < IPEndPoint.MinPort ||
			    port > IPEndPoint.MaxPort) {
				throw new ArgumentOutOfRangeException ("port");
			}
			
			this.family = family;

			IPEndPoint localEP = new IPEndPoint (IPAddress.Any, port);
			InitSocket (localEP);
		}
#endif
		
		public UdpClient (string hostname, int port)
		{
			if (hostname == null)
				throw new ArgumentNullException ("hostname");

			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				throw new ArgumentOutOfRangeException ("port");

			InitSocket (null);
			Connect (hostname, port);
		}

		private void InitSocket (EndPoint localEP)
		{
			if(socket != null) {
				socket.Close();
				socket = null;
			}

			socket = new Socket (family, SocketType.Dgram, ProtocolType.Udp);

			if (localEP != null)
				socket.Bind (localEP);
		}

#endregion // Constructors
#region Public methods
#region Close
		public void Close ()
		{
			((IDisposable) this).Dispose ();	
		}
#endregion
#region Connect
		public void Connect (IPEndPoint endPoint)
		{
			CheckDisposed ();
			if (endPoint == null)
				throw new ArgumentNullException ("endPoint");

			socket.Connect (endPoint);
			active = true;
		}

		public void Connect (IPAddress addr, int port)
		{
			if (addr == null)
				throw new ArgumentNullException ("addr");

			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				throw new ArgumentOutOfRangeException ("port");

			Connect (new IPEndPoint (addr, port));
		}

		public void Connect (string hostname, int port)
		{
			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				throw new ArgumentOutOfRangeException ("port");

			IPAddress[] addresses = Dns.Resolve (hostname).AddressList;
			for(int i=0; i<addresses.Length; i++) {
				try {
					this.family = addresses[i].AddressFamily;
					Connect (new IPEndPoint (addresses[i], port));
					break;
				} catch(Exception e) {
					if(i == addresses.Length - 1){
						if(socket != null) {
							socket.Close();
							socket = null;
						}

						/// This is the last entry, re-throw the exception
						throw e;
					}
				}
			}
		}
#endregion
		#region Multicast methods
		public void DropMulticastGroup (IPAddress multicastAddr)
		{
			CheckDisposed ();
			if (multicastAddr == null)
				throw new ArgumentNullException ("multicastAddr");

			if(family == AddressFamily.InterNetwork)
				socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.DropMembership,
					new MulticastOption (multicastAddr));
#if NET_1_1
			else
				socket.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.DropMembership,
					new IPv6MulticastOption (multicastAddr));
#endif
		}

#if NET_1_1
		public void DropMulticastGroup (IPAddress multicastAddr,
						int ifindex)
		{
			CheckDisposed ();

			/* LAMESPEC: exceptions haven't been specified
			 * for this overload.
			 */
			if (multicastAddr == null) {
				throw new ArgumentNullException ("multicastAddr");
			}

			/* Does this overload only apply to IPv6?
			 * Only the IPv6MulticastOption has an
			 * ifindex-using constructor.  The MS docs
			 * don't say.
			 */
			if (family == AddressFamily.InterNetworkV6) {
				socket.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.DropMembership, new IPv6MulticastOption (multicastAddr, ifindex));
			}
		}
#endif
		
		public void JoinMulticastGroup (IPAddress multicastAddr)
		{
			CheckDisposed ();

			if(family == AddressFamily.InterNetwork)
				socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.AddMembership,
					new MulticastOption (multicastAddr));
#if NET_1_1
			else
				socket.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.AddMembership,
					new IPv6MulticastOption (multicastAddr));
#endif
		}

#if NET_1_1
		public void JoinMulticastGroup (int ifindex,
						IPAddress multicastAddr)
		{
			CheckDisposed ();

			/* Does this overload only apply to IPv6?
			 * Only the IPv6MulticastOption has an
			 * ifindex-using constructor.  The MS docs
			 * don't say.
			 */
			if (family == AddressFamily.InterNetworkV6) {
				socket.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.AddMembership, new IPv6MulticastOption (multicastAddr, ifindex));
			}
		}
#endif
		
		public void JoinMulticastGroup (IPAddress multicastAddr, int timeToLive)
		{
			CheckDisposed ();
			JoinMulticastGroup (multicastAddr);
			if (timeToLive < 0 || timeToLive > 255)
				throw new ArgumentOutOfRangeException ("timeToLive");

			if(family == AddressFamily.InterNetwork)
				socket.SetSocketOption (SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive,
					timeToLive);
#if NET_1_1
			else
				socket.SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.MulticastTimeToLive,
					timeToLive);
#endif
		}
		#endregion
#region Data I/O
		public byte [] Receive (ref IPEndPoint remoteEP)
		{
			CheckDisposed ();

			// Bug 45633: the spec states that we should block until a datagram arrives:
			// remove the 512 hardcoded value.

			// Block until we get it.
			socket.Poll (-1, SelectMode.SelectRead);
			
			byte [] recBuffer;
			int available = socket.Available;

			recBuffer = new byte [available];
			EndPoint endPoint = new IPEndPoint (IPAddress.Any, 0);
			int dataRead = socket.ReceiveFrom (recBuffer, ref endPoint);
			if (dataRead < recBuffer.Length)
				recBuffer = CutArray (recBuffer, dataRead);

			remoteEP = (IPEndPoint) endPoint;
			return recBuffer;
		}

		public int Send (byte [] dgram, int bytes)
		{
			CheckDisposed ();
			if (dgram == null)
				throw new ArgumentNullException ("dgram");

			if (!active)
				throw new InvalidOperationException ("Operation not allowed on " + 
								     "non-connected sockets.");

			return socket.Send (dgram, 0, bytes, SocketFlags.None);
		}

		public int Send (byte [] dgram, int bytes, IPEndPoint endPoint)
		{
			CheckDisposed ();
			if (dgram == null)
				throw new ArgumentNullException ("dgram is null");
			
			if (active) {
				if (endPoint != null)
					throw new InvalidOperationException ("Cannot send packets to an " +
									     "arbitrary host while connected.");

				return socket.Send (dgram, 0, bytes, SocketFlags.None);
			}
			
			return socket.SendTo (dgram, 0, bytes, SocketFlags.None, endPoint);
		}

		public int Send (byte [] dgram, int bytes, string hostname, int port)
		{
			return Send (dgram, bytes, 
				     new IPEndPoint (Dns.Resolve (hostname).AddressList [0], port));
		}

		private byte [] CutArray (byte [] orig, int length)
		{
			byte [] newArray = new byte [length];
			Buffer.BlockCopy (orig, 0, newArray, 0, length);

			return newArray;
		}
#endregion
#region Properties
		protected bool Active {
			get { return active; }
			set { active = value; }
		}

		protected Socket Client {
			get { return socket; }
			set { socket = value; }
		}
#endregion
#region Disposing
		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		void Dispose (bool disposing)
		{
			if (disposed)
				return;
			disposed = true;

			if (disposing){
				if (socket != null)
					socket.Close ();

				socket = null;
			}
		}
		
		~UdpClient ()
		{
			Dispose (false);
		}
		
		private void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType().FullName);
		}		
#endregion
#endregion
	}
}

