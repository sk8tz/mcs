// System.Net.Sockets.TcpListenerTest.cs
//
// Authors:
//    Phillip Pearson (pp@myelin.co.nz)
//    Martin Willemoes Hansen (mwh@sysrq.dk)
//    Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Copyright 2001 Phillip Pearson (http://www.myelin.co.nz)
// (C) Copyright 2003 Martin Willemoes Hansen (mwh@sysrq.dk)
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;

namespace MonoTests.System.Net.Sockets
{
	[TestFixture]
	public class TcpListenerTest : Assertion
	{
		[Test]
		[Category("NotDotNet")]
		public void TcpListener ()
		{
			// listen with a new listener (IPv4 is the default)
			TcpListener inListener = new TcpListener (8766);
			inListener.Start();
			

			// connect to it from a new socket
			IPHostEntry hostent = Dns.GetHostByAddress (IPAddress.Loopback);
			Socket outSock = null;

			foreach (IPAddress address in hostent.AddressList) {
				if (address.AddressFamily == AddressFamily.InterNetwork) {
					/// Only keep IPv4 addresses, our Server is in IPv4 only mode.
					outSock = new Socket (address.AddressFamily, SocketType.Stream,
						ProtocolType.IP);
					IPEndPoint remote = new IPEndPoint (address, 8766);
					outSock.Connect (remote);
					break;
				}
			}
			
			// make sure the connection arrives
			Assert (inListener.Pending ());
			Socket inSock = inListener.AcceptSocket ();

			// now send some data and see if it comes out the other end
			const int len = 1024;
			byte[] outBuf = new Byte [len];
			for (int i=0; i<len; i++) 
				outBuf [i] = (byte) (i % 256);

			outSock.Send (outBuf, 0, len, 0);

			byte[] inBuf = new Byte[len];
			int ret = inSock.Receive (inBuf, 0, len, 0);


			// let's see if it arrived OK
			Assert(ret != 0);
			for (int i=0; i<len; i++) 
				Assert (inBuf[i] == outBuf [i]);

			// tidy up after ourselves
			inSock.Close ();

			inListener.Stop ();
		}

		[Test]
		public void CtorInt1 ()
		{
			int nex = 0;
			try { new TcpListener (-1); } catch { nex++; }
			new TcpListener (0);
			new TcpListener (65535);
			try { new TcpListener (65536); } catch { nex++; }
			try { new TcpListener (100000); } catch { nex++; }
			Assert (nex == 3);			
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorIPEndPoint ()
		{
			new TcpListener (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorIPAddressInt1 ()
		{
			new TcpListener (null, 100000);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CtorIPAddressInt2 ()
		{
			new TcpListener (IPAddress.Any, 100000);
		}

		class MyListener : TcpListener
		{
			public MyListener ()
				: base (IPAddress.Loopback, 5000)
			{
			}

			public Socket GetSocket ()
			{
				return Server;
			}

			public bool IsActive {
				get { return Active; }
			}
		}

		[Test]
		public void PreStartStatus ()
		{
			MyListener listener = new MyListener ();
			AssertEquals ("#01", false, listener.IsActive);
			Assert ("#02", null != listener.GetSocket ());
			try {
				listener.AcceptSocket ();
				Fail ("Exception not thrown");
			} catch (InvalidOperationException) {
			}

			try {
				listener.AcceptTcpClient ();
				Fail ("Exception not thrown");
			} catch (InvalidOperationException) {
			}

			try {
				listener.Pending ();
				Fail ("Exception not thrown");
			} catch (InvalidOperationException) {
			}

			listener.Stop ();
		}

		[Test]
		public void PostStartStatus ()
		{
			MyListener listener = new MyListener ();
			listener.Start ();
			AssertEquals ("#01", true, listener.IsActive);
			Assert ("#02", null != listener.GetSocket ());
			
			Socket sock = listener.GetSocket ();
			listener.Start (); // Start called twice
			AssertEquals ("#03", true, listener.IsActive);
			Assert ("#04", null != listener.GetSocket ());

			AssertEquals ("#05", false, listener.Pending ());

			listener.Stop ();
			AssertEquals ("#06", false, listener.IsActive);
			Assert ("#07", null != listener.GetSocket ());
			Assert ("#08", sock != listener.GetSocket ());
		}
	}
}
