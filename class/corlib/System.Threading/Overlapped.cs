//
// System.Threading.Overlapped.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Threading
{
	public class Overlapped
	{
		[CLSCompliant(false)]
		unsafe public static void Free(NativeOverlapped *nativeOverlappedPtr) {
			// FIXME
		}

		[CLSCompliant(false)]
		unsafe public static Overlapped Unpack(NativeOverlapped *nativeOverlappedPtr) {
			// FIXME
			return(new Overlapped());
		}

		public Overlapped() {
			// FIXME
		}

		public Overlapped(int offsetLo, int offsetHi, int hEvent, IAsyncResult ar) {
			// FIXME
		}

		public IAsyncResult AsyncResult {
			get {
				// FIXME
				return(null);
			}
			
			set {
			}
		}

		public int EventHandle {
			get {
				// FIXME
				return(0);
			}
			
			set {
			}
		}

		public int OffsetHigh {
			get {
				// FIXME
				return(0);
			}
			
			set {
			}
		}

		public int OffsetLow {
			get {
				// FIXME
				return(0);
			}
			
			set {
			}
		}

		[CLSCompliant(false)]
		unsafe public NativeOverlapped *Pack(IOCompletionCallback iocb) {
			// FIXME
			return(null);
		}
		
		[CLSCompliant(false)]
		unsafe public NativeOverlapped *UnsafePack(IOCompletionCallback iocb) {
			// FIXME
			return(null);
		}
	}
}
