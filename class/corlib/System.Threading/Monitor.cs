//
// System.Threading.Monitor.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;

namespace System.Threading
{
	public sealed class Monitor
	{
		private Monitor () {}

		// Grabs the mutex on object 'obj', with a maximum
		// wait time 'ms' but doesn't block - if it can't get
		// the lock it returns false, true if it can
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool Monitor_try_enter(object obj, int ms);
		public static void Enter(object obj) {
			if(obj==null) {
				throw new ArgumentNullException("Object is null");
			}
			//if(obj.GetType().IsValueType==true) {
			//	throw new ArgumentException("Value type");
			//}

			Monitor_try_enter(obj, Timeout.Infinite);
		}

		// Releases the mutex on object 'obj'
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Monitor_exit(object obj);

		public static void Exit(object obj) {
			if(obj==null) {
				throw new ArgumentNullException("Object is null");
			}
			//if(obj.GetType().IsValueType==true) {
			//	throw new ArgumentException("Value type");
			//}

			Monitor_exit(obj);
		}

		// Signals one of potentially many objects waiting on
		// object 'obj'
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Monitor_pulse(object obj);

		// Checks whether object 'obj' is currently synchronised
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool Monitor_test_synchronised(object obj);

		public static void Pulse(object obj) {
			if(obj==null) {
				throw new ArgumentNullException("Object is null");
			}
			if(Monitor_test_synchronised(obj)==false) {
				throw new SynchronizationLockException("Object is not synchronized");
			}

			Monitor_pulse(obj);
		}

		// Signals all of potentially many objects waiting on
		// object 'obj'
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Monitor_pulse_all(object obj);

		public static void PulseAll(object obj) {
			if(obj==null) {
				throw new ArgumentNullException("Object is null");
			}
			if(Monitor_test_synchronised(obj)==false) {
				throw new SynchronizationLockException("Object is not synchronized");
			}

			Monitor_pulse_all(obj);
		}

		public static bool TryEnter(object obj) {
			if(obj==null) {
				throw new ArgumentNullException("Object is null");
			}
			//if(obj.GetType().IsValueType==true) {
			//	throw new ArgumentException("Value type");
			//}
			
			return(Monitor_try_enter(obj, 0));
		}

		public static bool TryEnter(object obj, int millisecondsTimeout) {
			if(obj==null) {
				throw new ArgumentNullException("Object is null");
			}
			//if(obj.GetType().IsValueType==true) {
			//	throw new ArgumentException("Value type");
			//}

			// LAMESPEC: should throw an exception when ms<0, but
			// Timeout.Infinite is -1
			if(millisecondsTimeout == Timeout.Infinite) {
				Enter(obj);
				return(true);
			}
			
			if(millisecondsTimeout<0) {
				throw new ArgumentException("millisecondsTimeout negative");
			}
			
			return(Monitor_try_enter(obj, millisecondsTimeout));
		}

		public static bool TryEnter(object obj, TimeSpan timeout) {
			if(obj==null) {
				throw new ArgumentNullException("Object is null");
			}
			//if(obj.GetType().IsValueType==true) {
			//	throw new ArgumentException("Value type");
			//}

			// LAMESPEC: should throw an exception when ms<0, but
			// Timeout.Infinite is -1
			int ms=Convert.ToInt32(timeout.TotalMilliseconds);
			
			if(ms == Timeout.Infinite) {
				Enter(obj);
				return(true);
			}

			if(ms < 0 || ms > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("timeout out of range");
			}
			
			return(Monitor_try_enter(obj, ms));
		}

		// Waits for a signal on object 'obj' with maximum
		// wait time 'ms'. Returns true if the object was
		// signalled, false if it timed out
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool Monitor_wait(object obj, int ms);

		public static bool Wait(object obj) {
			if(obj==null) {
				throw new ArgumentNullException("Object is null");
			}
			if(Monitor_test_synchronised(obj)==false) {
				throw new SynchronizationLockException("Object is not synchronized");
			}

			return(Monitor_wait(obj, Timeout.Infinite));
		}

		public static bool Wait(object obj, int millisecondsTimeout) {
			if(obj==null) {
				throw new ArgumentNullException("Object is null");
			}
			if(Monitor_test_synchronised(obj)==false) {
				throw new SynchronizationLockException("Object is not synchronized");
			}
			// LAMESPEC: no mention of timeout sanity checking

			return(Monitor_wait(obj, millisecondsTimeout));
		}

		public static bool Wait(object obj, TimeSpan timeout) {
			if(obj==null) {
				throw new ArgumentNullException("Object is null");
			}
			// LAMESPEC: says to throw ArgumentException too
			int ms=Convert.ToInt32(timeout.TotalMilliseconds);
			
			if(ms < 0 || ms > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("timeout out of range");
			}
			if(Monitor_test_synchronised(obj)==false) {
				throw new SynchronizationLockException("Object is not synchronized");
			}

			return(Monitor_wait(obj, ms));
		}

		public static bool Wait(object obj, int millisecondsTimeout, bool exitContext) {
			try {
				if (exitContext) SynchronizationAttribute.ExitContext ();
				return Wait (obj, millisecondsTimeout);
			}
			finally {
				if (exitContext) SynchronizationAttribute.EnterContext ();
			}
		}

		public static bool Wait(object obj, TimeSpan timeout, bool exitContext) {
			try {
				if (exitContext) SynchronizationAttribute.ExitContext ();
				return Wait (obj, timeout);
			}
			finally {
				if (exitContext) SynchronizationAttribute.EnterContext ();
			}
		}
	}
}
