//
// System.Net.Sockets.NetworkStream.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using System.IO;

namespace System.Net.Sockets
{
	public class NetworkStream : Stream, IDisposable {
		FileAccess access;
		Socket socket;
		bool owns_socket;
		bool readable, writeable;
		
		public NetworkStream (Socket socket)
			: this (socket, FileAccess.ReadWrite, false)
		{
		}

		public NetworkStream (Socket socket, bool owns_socket)
			: this (socket, FileAccess.ReadWrite, owns_socket)
		{
		}

		public NetworkStream (Socket socket, FileAccess access)
			: this (socket, access, false)
		{
		}
		
		public NetworkStream (Socket socket, FileAccess access, bool owns_socket)
		{
			if (socket == null)
				throw new ArgumentNullException ();
			if (!socket.Connected)
				throw new ArgumentException ("Not connected", "socket");
			if (socket.SocketType != SocketType.Stream)
				throw new ArgumentException ("Socket is not of type Stream", "socket");
			if (!socket.Blocking)
				throw new IOException ();
			
			this.socket = socket;
			this.owns_socket = owns_socket;
			this.access = access;

			readable = CanRead;
			writeable = CanWrite;
		}

		public override bool CanRead {
			get {
				return access == FileAccess.ReadWrite || access == FileAccess.Read;
			}
		}

		public override bool CanSeek {
			get {
				// network sockets cant seek.
				return false;
			}
		}

		public override bool CanWrite {
			get {
				return access == FileAccess.ReadWrite || access == FileAccess.Write;
			}
		}

		public virtual bool DataAvailable {
			get {
				return socket.Available > 0;
			}
		}

		public override long Length {
			get {
				// Network sockets always throw an exception
				throw new NotSupportedException ();
			}
		}

		public override long Position {
			get {
				// Network sockets always throw an exception
				throw new NotSupportedException ();
			}
			
			set {
				// Network sockets always throw an exception
				throw new NotSupportedException ();
			}
		}

		protected bool Readable {
			get {
				return readable;
			}

			set {
				readable = value;
			}
		}

		protected Socket Socket {
			get {
				return socket;
			}
		}

		protected bool Writeable {
			get {
				return writeable;
			}

			set {
				writeable = value;
			}
		}

		public override IAsyncResult BeginRead (byte [] buffer, int offset, int size,
							AsyncCallback callback, object state)
		{
			IAsyncResult retval;
			
			if (buffer == null)
				throw new ArgumentNullException ();
			if (socket == null)
				throw new ObjectDisposedException ("socket");
			int len = buffer.Length;
			if (offset >= len || size != len)
				throw new ArgumentOutOfRangeException ();

			try {
				retval = socket.BeginReceive (buffer, offset, size, 0, callback, state);
			} catch {
				throw new IOException ("BeginReceive failure");
			}

			return retval;
		}

		public override IAsyncResult BeginWrite (byte [] buffer, int offset, int size,
							AsyncCallback callback, object state)
		{
			IAsyncResult retval;
			
			if (buffer == null)
				throw new ArgumentNullException ();
			if (socket == null)
				throw new ObjectDisposedException ("socket");
			int len = buffer.Length;
			if (len < size)
				throw new ArgumentException ();

			try {
				retval = socket.BeginSend (buffer, offset, size, 0, callback, state);
			} catch {
				throw new IOException ("BeginWrite failure");
			}

			return retval;
		}

		~NetworkStream ()
		{
			Dispose (false);
		}
		
		public override void Close ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (owns_socket)
				if (socket != null)
					socket.Close ();
			socket = null;
		}

		public override int EndRead (IAsyncResult ar)
		{
			int res;
			
			if (ar == null)
				throw new ArgumentNullException ();
			if (socket == null)
				throw new ObjectDisposedException ("socket");

			try {
				res = socket.EndReceive (ar);
			} catch {
				throw new IOException ("EndRead failure");
			}
			return res;
		}

		public override void EndWrite (IAsyncResult ar)
		{
			if (ar == null)
				throw new ArgumentNullException ();
			if (socket == null)
				throw new ObjectDisposedException ("socket");

			try {
				socket.EndSend (ar);
			} catch {
				throw new IOException ("EndWrite failure");
			}
		}

		public override void Flush ()
		{
			// network streams are non-buffered, this is a no-op
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
		}

		public override int Read (byte [] buffer, int offset, int size)
		{
			int res;
					
			if (buffer == null)
				throw new ArgumentNullException ();
			if (buffer.Length < size)
				throw new ArgumentException ();

			try {
				res = socket.Receive (buffer, offset, size, 0);
			} catch {
				throw new IOException ("Read failure");
			}
			return res;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			// NetworkStream objects do not support seeking.
			
			throw new NotSupportedException ();
		}

		public override void SetLength (long value)
		{
			// NetworkStream objects do not support SetLength
			
			throw new NotSupportedException ();
		}

		public override void Write (byte [] buffer, int offset, int size)
		{
			if (buffer == null)
				throw new ArgumentNullException ();
			if (buffer.Length < size)
				throw new ArgumentException ();
			try {
				socket.Send (buffer, offset, size, 0);
			} catch {
				throw new IOException ("Write failure"); 
			}
		}
	     
	}
}
