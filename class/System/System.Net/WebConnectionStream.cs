//
// System.Net.WebConnectionStream
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System.IO;
using System.Threading;

namespace System.Net
{
	class WebConnectionStream : Stream
	{
		bool isRead;
		WebConnection cnc;
		HttpWebRequest request;
		byte [] readBuffer;
		int readBufferOffset;
		int readBufferSize;
		int contentLength;
		int totalRead;
		bool nextReadCalled;
		int pendingReads;
		ManualResetEvent pending;
		bool allowBuffering;
		bool sendChunked;
		MemoryStream writeBuffer;
		bool requestWritten;
		byte [] headers;
		bool disposed;

		public WebConnectionStream (WebConnection cnc)
		{
			isRead = true;
			pending = new ManualResetEvent (true);
			this.cnc = cnc;
			try {
				contentLength = Int32.Parse (cnc.Data.Headers ["Content-Length"]);
			} catch {
				contentLength = Int32.MaxValue;
			}
		}

		public WebConnectionStream (WebConnection cnc, HttpWebRequest request)
		{
			isRead = false;
			this.cnc = cnc;
			this.request = request;
			allowBuffering = request.InternalAllowBuffering;
			sendChunked = request.SendChunked;
			if (allowBuffering)
				writeBuffer = new MemoryStream ();
		}

		internal byte [] ReadBuffer {
			set { readBuffer = value; }
		}

		internal int ReadBufferOffset {
			set { readBufferOffset = value;}
		}
		
		internal int ReadBufferSize {
			set { readBufferSize = value; }
		}
		
		internal void CheckComplete ()
		{
			if (!nextReadCalled && readBufferSize - readBufferOffset == contentLength) {
				nextReadCalled = true;
				cnc.NextRead ();
			}
		}

		internal void ReadAll ()
		{
			if (!isRead || totalRead >= contentLength || nextReadCalled)
				return;

			pending.WaitOne ();
			lock (this) {
				if (totalRead >= contentLength)
					return;
				
				byte [] b = null;
				int diff = readBufferSize - readBufferOffset;
				int new_size;

				if (contentLength == Int32.MaxValue) {
					MemoryStream ms = new MemoryStream ();
					if (readBuffer != null && diff > 0)
						ms.Write (readBuffer, readBufferOffset, diff);

					byte [] buffer = new byte [2048];
					int read;
					while ((read = cnc.Read (buffer, 0, 2048)) != 0)
						ms.Write (buffer, 0, read);

					b = ms.GetBuffer ();
					new_size = (int) ms.Length;
					contentLength = new_size;
				} else {
					new_size = contentLength - totalRead;
					b = new byte [new_size];
					if (readBuffer != null && diff > 0)
						Buffer.BlockCopy (readBuffer, readBufferOffset, b, 0, diff);
					
					int remaining = new_size - diff;
					int r = -1;
					while (remaining > 0 && r != 0) {
						r = cnc.Read (b, diff, remaining);
						remaining -= r;
						diff += r;
					}
				}

				readBuffer = b;
				readBufferOffset = 0;
				readBufferSize = new_size;
				totalRead = 0;
				nextReadCalled = true;
			}

			cnc.NextRead ();
		}
		
	   	static void CallbackWrapper (IAsyncResult r)
		{
			WebAsyncResult result = (WebAsyncResult) r.AsyncState;
			result.InnerAsyncResult = r;
			result.DoCallback ();
		}

		public override int Read (byte [] buffer, int offset, int size)
		{
			if (!isRead)
				throw new NotSupportedException ("this stream does not allow reading");

			if (totalRead >= contentLength)
				return 0;

			IAsyncResult res = BeginRead (buffer, offset, size, null, null);
			return EndRead (res);
		}

		public override IAsyncResult BeginRead (byte [] buffer, int offset, int size,
							AsyncCallback cb, object state)
		{
			if (!isRead)
				throw new NotSupportedException ("this stream does not allow reading");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			int length = buffer.Length;
			if (size < 0 || offset < 0 || length < offset || length - offset < size)
				throw new ArgumentOutOfRangeException ();

			WebAsyncResult result = new WebAsyncResult (cb, state, buffer, offset, size);
			if (totalRead >= contentLength) {
				result.SetCompleted (true, -1);
				result.DoCallback ();
				return result;
			}
			
			int remaining = readBufferSize - readBufferOffset;
			if (remaining > 0) {
				int copy = (remaining > size) ? size : remaining;
				Buffer.BlockCopy (readBuffer, readBufferOffset, buffer, offset, copy);
				readBufferOffset += copy;
				offset += copy;
				size -= copy;
				totalRead += copy;
				if (size == 0 || totalRead >= contentLength) {
					result.SetCompleted (true, copy);
					result.DoCallback ();
					return result;
				}
				result.NBytes = copy;
			}

			lock (this) {
				pendingReads++;
				pending.Reset ();
			}

			if (cb != null)
				cb = new AsyncCallback (CallbackWrapper);

			if (contentLength != Int32.MaxValue && contentLength - totalRead < size)
				size = contentLength - totalRead;

			result.InnerAsyncResult = cnc.BeginRead (buffer, offset, size, cb, result);
			return result;
		}

		public override int EndRead (IAsyncResult r)
		{
			WebAsyncResult result = (WebAsyncResult) r;

			if (!result.IsCompleted) {
				int nbytes = cnc.EndRead (result.InnerAsyncResult);
				lock (this) {
					pendingReads--;
					if (pendingReads == 0)
						pending.Set ();
				}

				bool finished = (nbytes == -1);
				if (finished && result.NBytes > 0)
					nbytes = 0;

				result.SetCompleted (false, nbytes + result.NBytes);
				totalRead += nbytes;
				if (finished || nbytes == 0)
					contentLength = totalRead;
			}

			if (totalRead >= contentLength && !nextReadCalled) {
				nextReadCalled = true;
				cnc.NextRead ();
			}

			return result.NBytes;
		}
		
		public override IAsyncResult BeginWrite (byte [] buffer, int offset, int size,
							AsyncCallback cb, object state)
		{
			if (isRead)
				throw new NotSupportedException ("this stream does not allow writing");

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			int length = buffer.Length;
			if (size < 0 || offset < 0 || length < offset || length - offset < size)
				throw new ArgumentOutOfRangeException ();

			WebAsyncResult result = new WebAsyncResult (cb, state);
			if (allowBuffering) {
				writeBuffer.Write (buffer, offset, size);
				result.SetCompleted (true, 0);
				result.DoCallback ();
			} else {
				if (cb != null)
					cb = new AsyncCallback (CallbackWrapper);

				result.InnerAsyncResult = cnc.BeginWrite (buffer, offset, size, cb, result);
				if (result.InnerAsyncResult == null)
					throw new WebException ("Aborted");
			}

			return result;
		}

		public override void EndWrite (IAsyncResult r)
		{
			if (r == null)
				throw new ArgumentNullException ("r");

			if (allowBuffering)
				return;

			WebAsyncResult result = r as WebAsyncResult;
			if (result == null)
				throw new ArgumentException ("Invalid IAsyncResult");

			cnc.EndWrite (result.InnerAsyncResult);
			return;
		}
		
		public override void Write (byte [] buffer, int offset, int size)
		{
			if (isRead)
				throw new NotSupportedException ("this stream does not allow writing");

			IAsyncResult res = BeginWrite (buffer, offset, size, null, null);
			EndWrite (res);
		}

		public override void Flush ()
		{
		}

		internal void SetHeaders (byte [] buffer, int offset, int size)
		{
			if (!allowBuffering) {
				try {
					Write (buffer, offset, size);
				} catch (IOException) {
					if (cnc.Connected)
						throw;

					if (!cnc.TryReconnect ())
						throw;

					Write (buffer, offset, size);
				}
			} else {
				headers = new byte [size];
				Buffer.BlockCopy (buffer, offset, headers, 0, size);
			}
		}

		internal void WriteRequest ()
		{
			if (!allowBuffering || writeBuffer == null || requestWritten)
				return;

			byte [] bytes = writeBuffer.GetBuffer ();
			int length = (int) writeBuffer.Length;
			if (request.ContentLength != -1 && request.ContentLength < length) {
				throw new ProtocolViolationException ("Specified Content-Length is less than the " +
								      "number of bytes to write");
			}

			request.InternalContentLength = length;
			request.SendRequestHeaders ();
			requestWritten = true;
			while (true) {
				cnc.WaitForContinue (headers, 0, headers.Length);
				if (!cnc.Connected) {
					if (!cnc.TryReconnect ())
						return;

					continue;
				}

				if (cnc.Data.StatusCode != 0 && cnc.Data.StatusCode != 100)
					return;

				cnc.Write (bytes, 0, length);
				if (!cnc.Connected && cnc.TryReconnect ())
					continue;

				break;
			}
		}

		internal void InternalClose ()
		{
			disposed = true;
		}
		
		public override void Close ()
		{
			if (isRead || !allowBuffering || disposed)
				return;

			disposed = true;

			long length = request.ContentLength;
			if (length != -1 && length > writeBuffer.Length)
				throw new IOException ("Cannot close the stream until all bytes are written");

			WriteRequest ();
		}

		internal void ResetWriteBuffer ()
		{
			if (!allowBuffering)
				return;

			writeBuffer = new MemoryStream ();
			requestWritten = false;
		}
		
		public override long Seek (long a, SeekOrigin b)
		{
			throw new NotSupportedException ();
		}
		
		public override void SetLength (long a)
		{
			throw new NotSupportedException ();
		}
		
		public override bool CanSeek {
			get { return false; }
		}

		public override bool CanRead {
			get { return isRead && (contentLength == Int32.MaxValue || totalRead < contentLength); }
		}

		public override bool CanWrite {
			get { return !isRead; }
		}

		public override long Length {
			get { throw new NotSupportedException (); }
		}

		public override long Position {
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
	}
}

