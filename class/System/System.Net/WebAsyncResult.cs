//
// System.Net.WebAsyncResult
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
	class WebAsyncResult : IAsyncResult
	{
		ManualResetEvent handle;
		bool synch;
		bool isCompleted;
		AsyncCallback cb;
		object state;
		int nbytes;
		IAsyncResult innerAsyncResult;
		bool callbackDone;
		Exception exc;
		HttpWebRequest request;
		HttpWebResponse response;
		Stream writeStream;
		byte [] buffer;
		int offset;
		int size;
		object locker = new object ();

		public WebAsyncResult (AsyncCallback cb, object state)
		{
			this.cb = cb;
			this.state = state;
		}

		public WebAsyncResult (HttpWebRequest request, AsyncCallback cb, object state)
		{
			this.request = request;
			this.cb = cb;
			this.state = state;
		}

		public WebAsyncResult (AsyncCallback cb, object state, byte [] buffer, int offset, int size)
		{
			this.cb = cb;
			this.state = state;
			this.buffer = buffer;
			this.offset = offset;
			this.size = size;
		}

		internal void SetCompleted (bool synch, Exception e)
		{
			this.synch = synch;
			exc = e;
			lock (locker) {
				isCompleted = true;
				if (handle != null)
					handle.Set ();
			}
		}
		
		internal void Reset ()
		{
			callbackDone = false;
			exc = null;
			request = null;
			response = null;
			writeStream = null;
			exc = null;
			lock (locker) {
				isCompleted = false;
				if (handle != null)
					handle.Reset ();
			}
		}

		internal void SetCompleted (bool synch, int nbytes)
		{
			this.synch = synch;
			this.nbytes = nbytes;
			exc = null;
			lock (locker) {
				isCompleted = true;
				if (handle != null)
					handle.Set ();
			}
		}
		
		internal void SetCompleted (bool synch, Stream writeStream)
		{
			this.synch = synch;
			this.writeStream = writeStream;
			exc = null;
			lock (locker) {
				isCompleted = true;
				if (handle != null)
					handle.Set ();
			}
		}
		
		internal void SetCompleted (bool synch, HttpWebResponse response)
		{
			this.synch = synch;
			this.response = response;
			exc = null;
			lock (locker) {
				isCompleted = true;
				if (handle != null)
					handle.Set ();
			}
		}
		
		internal void DoCallback ()
		{
			if (!callbackDone && cb != null) {
				callbackDone = true;
				cb (this);
			}
		}
		
		internal void WaitUntilComplete ()
		{
			if (IsCompleted)
				return;

			AsyncWaitHandle.WaitOne ();
		}

		internal bool WaitUntilComplete (int timeout, bool exitContext)
		{
			if (IsCompleted)
				return true;

			return AsyncWaitHandle.WaitOne (timeout, exitContext);
		}

		public object AsyncState {
			get { return state; }
		}

		public WaitHandle AsyncWaitHandle {
			get {
				lock (locker) {
					if (handle == null)
						handle = new ManualResetEvent (isCompleted);
				}
				
				return handle;
			}
		}

		public bool CompletedSynchronously {
			get { return synch; }
		}

		public bool IsCompleted {
			get {
				lock (locker) {
					return isCompleted;
				}
			}
		}

		internal bool GotException {
			get { return (exc != null); }
		}
		
		internal Exception Exception {
			get { return exc; }
		}
		
		internal int NBytes {
			get { return nbytes; }
			set { nbytes = value; }
		}

		internal IAsyncResult InnerAsyncResult {
			get { return innerAsyncResult; }
			set { innerAsyncResult = value; }
		}

		internal Stream WriteStream {
			get { return writeStream; }
		}

		internal HttpWebResponse Response {
			get { return response; }
		}

		internal byte [] Buffer {
			get { return buffer; }
		}

		internal int Offset {
			get { return offset; }
		}

		internal int Size {
			get { return size; }
		}
	}
}

