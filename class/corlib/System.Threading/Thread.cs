//
// System.Threading.Thread.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Remoting.Contexts;
using System.Security.Principal;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Collections;

namespace System.Threading
{
	public sealed class Thread
	{
		// stores a thread handle
		private IntPtr system_thread_handle;
		
		private CultureInfo current_culture;
		private CultureInfo current_ui_culture;
		private bool threadpool_thread;
		private ThreadState state = ThreadState.Unstarted;
		private object abort_exc;
		internal object abort_state;
		/* thread_id is only accessed from unmanaged code */
		private int thread_id;
		
		/* start_notify is used by the runtime to signal that Start()
		 * is ok to return
		 */
		private IntPtr start_notify;
		private IntPtr stack_ptr;
		private IntPtr static_data;
		private IntPtr jit_data;
		private IntPtr lock_data;
		
		private ThreadStart threadstart;
		private string thread_name=null;
		
		
		public static Context CurrentContext {
			get {
				return(AppDomain.InternalGetContext ());
			}
		}

		[MonoTODO]
		public static IPrincipal CurrentPrincipal {
			get {
				// FIXME -
				// System.Security.Principal.IPrincipal
				// not yet implemented
				return(null);
			}
			
			set {
			}
		}

		// Looks up the object associated with the current thread
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static Thread CurrentThread_internal();
		
		public static Thread CurrentThread {
			get {
				return(CurrentThread_internal());
			}
		}

		internal static int CurrentThreadId {
			get {
				return CurrentThread.thread_id;
			}
		}

		// Looks up the slot hash for the current thread
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static Hashtable SlotHash_lookup();

		// Stores the slot hash for the current thread
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void SlotHash_store(Hashtable slothash);

		private static Hashtable GetTLSSlotHash() {
			Hashtable slothash=SlotHash_lookup();
			if(slothash==null) {
				// Not synchronised, because this is
				// thread specific anyway.
				slothash=new Hashtable();
				SlotHash_store(slothash);
			}

			return(slothash);
		}
		
		internal static object ResetDataStoreStatus () {
			Hashtable slothash=SlotHash_lookup();
			SlotHash_store(null);
			return slothash;
		}

		internal static void RestoreDataStoreStatus (object data) {
			SlotHash_store((Hashtable)data);
		}

		public static LocalDataStoreSlot AllocateDataSlot() {
			LocalDataStoreSlot slot = new LocalDataStoreSlot();

			return(slot);
		}

		// Stores a hash keyed by strings of LocalDataStoreSlot objects
		static Hashtable datastorehash;

		private static void InitDataStoreHash () {
			lock (typeof (Thread)) {
				if (datastorehash == null) {
					datastorehash = Hashtable.Synchronized(new Hashtable());
				}
			}
		}
		
		public static LocalDataStoreSlot AllocateNamedDataSlot(string name) {
			lock (typeof (Thread)) {
				if (datastorehash == null)
					InitDataStoreHash ();
				LocalDataStoreSlot slot = (LocalDataStoreSlot)datastorehash[name];
				if(slot!=null) {
					// This exception isnt documented (of
					// course) but .net throws it
					throw new ArgumentException("Named data slot already added");
				}
			
				slot = new LocalDataStoreSlot();

				datastorehash.Add(name, slot);

				return(slot);
			}
		}

		public static void FreeNamedDataSlot(string name) {
			lock (typeof (Thread)) {
				if (datastorehash == null)
					InitDataStoreHash ();
				LocalDataStoreSlot slot=(LocalDataStoreSlot)datastorehash[name];

				if(slot!=null) {
					datastorehash.Remove(slot);
				}
			}
		}

		public static object GetData(LocalDataStoreSlot slot) {
			Hashtable slothash=GetTLSSlotHash();
			return(slothash[slot]);
		}

		public static AppDomain GetDomain() {
			return AppDomain.CurrentDomain;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int GetDomainID();

		public static LocalDataStoreSlot GetNamedDataSlot(string name) {
			if (datastorehash == null)
				InitDataStoreHash ();
			LocalDataStoreSlot slot=(LocalDataStoreSlot)datastorehash[name];

			if(slot==null) {
				slot=AllocateNamedDataSlot(name);
			}
			
			return(slot);
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void ResetAbort_internal();

		public static void ResetAbort()
		{
			Thread thread=CurrentThread;

			thread.clr_state(ThreadState.AbortRequested);
			ResetAbort_internal();
		}
		

		public static void SetData(LocalDataStoreSlot slot,
					   object data) {
			Hashtable slothash=GetTLSSlotHash();

			if(slothash.Contains(slot)) {
				slothash.Remove(slot);
			}
			
			slothash.Add(slot, data);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Sleep_internal(int ms);

		public static void Sleep(int millisecondsTimeout) {
			if((millisecondsTimeout<0) && (millisecondsTimeout != Timeout.Infinite)) {
				throw new ArgumentException("Negative timeout");
			}
			Thread thread=CurrentThread;
				
			thread.set_state(ThreadState.WaitSleepJoin);
				
			Sleep_internal(millisecondsTimeout);
			thread.clr_state(ThreadState.WaitSleepJoin);
		}

		public static void Sleep(TimeSpan timeout) {
			// LAMESPEC: says to throw ArgumentException too
			int ms=Convert.ToInt32(timeout.TotalMilliseconds);
			
			if(ms < 0 || ms > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("Timeout out of range");
			}

			Thread thread=CurrentThread;
				
			thread.set_state(ThreadState.WaitSleepJoin);
			Sleep_internal(ms);
			thread.clr_state(ThreadState.WaitSleepJoin);
		}

		// Returns the system thread handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern IntPtr Thread_internal(ThreadStart start);

		public Thread(ThreadStart start) {
			if(start==null) {
				throw new ArgumentNullException("Null ThreadStart");
			}
			threadstart=start;
		}

		[MonoTODO]
		public ApartmentState ApartmentState {
			get {
				// FIXME
				return(ApartmentState.Unknown);
			}
			
			set {
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern int current_lcid ();

		/* If the current_lcid() isn't known by CultureInfo,
		 * it will throw an exception which may cause
		 * String.Concat to try and recursively look up the
		 * CurrentCulture, which will throw an exception, etc.
		 * Use a boolean to short-circuit this scenario.
		 */
		private static bool in_currentculture=false;
		
		public CultureInfo CurrentCulture {
			get {
				if (current_culture == null) {
					lock (typeof (Thread)) {
						if(current_culture==null) {
							if(in_currentculture==true) {
								/* Bail out */
								current_culture = CultureInfo.InvariantCulture;
							} else {
								in_currentculture=true;
							
								try {
									current_culture = new CultureInfo (current_lcid ());
								} catch (ArgumentException) {
									
									current_culture = CultureInfo.InvariantCulture;
								}
							}
						}
						
						in_currentculture=false;
					}
				}
				
				return(current_culture);
			}
			
			set {
				current_culture = value;
			}
		}

		public CultureInfo CurrentUICulture {
			get {
				if (current_ui_culture == null) {
					lock (this) {
						if(current_ui_culture==null) {
							/* We don't
							 * distinguish
							 * between
							 * System and
							 * UI cultures
							 */
							try {
								current_ui_culture = new CultureInfo (current_lcid ());
							} catch (ArgumentException) {
							
								current_ui_culture = CultureInfo.InvariantCulture;
							}
						}
					}
				}
				
				return(current_ui_culture);
			}
			
			set {
				current_ui_culture = value;
			}
		}

		public bool IsThreadPoolThread {
			get {
				return IsThreadPoolThreadInternal;
			}
		}

		internal bool IsThreadPoolThreadInternal {
			get {
				return threadpool_thread;
			}
			set {
				threadpool_thread = value;
			}
		}

		public bool IsAlive {
			get {
				ThreadState curstate=state;
				
				if((curstate & ThreadState.Aborted) != 0 ||
				   (curstate & ThreadState.Stopped) != 0 ||
				   (curstate & ThreadState.Unstarted) != 0) {
					return(false);
				} else {
					return(true);
				}
			}
		}

		public bool IsBackground {
			get {
				if((state & ThreadState.Background) != 0) {
					return(true);
				} else {
					return(false);
				}
			}
			
			set {
				if(value==true) {
					set_state(ThreadState.Background);
				} else {
					clr_state(ThreadState.Background);
				}
			}
		}

		public string Name {
			get {
				return(thread_name);
			}
			
			set {
				lock (this) {
					if(thread_name!=null) {
						throw new InvalidOperationException ("Thread.Name can only be set once.");
					}
				
					thread_name=value;
				}
			}
		}

		[MonoTODO]
		public ThreadPriority Priority {
			get {
				// FIXME
				return(ThreadPriority.Lowest);
			}
			
			set {
			}
		}

		public ThreadState ThreadState {
			get {
				return(state);
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void Abort_internal (object stateInfo);

		public void Abort() {
			set_state(ThreadState.AbortRequested);
			Abort_internal (null);
		}

		public void Abort(object stateInfo) {
			set_state(ThreadState.AbortRequested);
			Abort_internal(stateInfo);
		}
		

		[MonoTODO]
		public void Interrupt() {
			// FIXME
		}

		// The current thread joins with 'this'. Set ms to 0 to block
		// until this actually exits.
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern bool Join_internal(int ms, IntPtr handle);
		
		public void Join() {
			if((state & ThreadState.Unstarted) != 0) {
				throw new ThreadStateException("Thread has not been started");
			}
			
			Thread thread=CurrentThread;
				
			thread.set_state(ThreadState.WaitSleepJoin);
			Join_internal(Timeout.Infinite, system_thread_handle);
			thread.clr_state(ThreadState.WaitSleepJoin);
		}

		public bool Join(int millisecondsTimeout) {
			if (millisecondsTimeout != Timeout.Infinite && millisecondsTimeout < 0)
				throw new ArgumentException ("Timeout less than zero", "millisecondsTimeout");

			if((state & ThreadState.Unstarted) != 0) {
				throw new ThreadStateException("Thread has not been started");
			}

			Thread thread=CurrentThread;
				
			thread.set_state(ThreadState.WaitSleepJoin);
			bool ret=Join_internal(millisecondsTimeout,
					       system_thread_handle);
			thread.clr_state(ThreadState.WaitSleepJoin);

			return(ret);
		}

		public bool Join(TimeSpan timeout) {
			// LAMESPEC: says to throw ArgumentException too
			int ms=Convert.ToInt32(timeout.TotalMilliseconds);
			
			if(ms < 0 || ms > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("timeout out of range");
			}
			if((state & ThreadState.Unstarted) != 0) {
				throw new ThreadStateException("Thread has not been started");
			}

			Thread thread=CurrentThread;

			thread.set_state(ThreadState.WaitSleepJoin);
			bool ret=Join_internal(ms, system_thread_handle);
			thread.clr_state(ThreadState.WaitSleepJoin);

			return(ret);
		}

		[MonoTODO]
		public void Resume() {
			throw new NotImplementedException ();
		}

		// Launches the thread
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void Start_internal(IntPtr handle);
		
		public void Start() {
			lock(this) {
				if((state & ThreadState.Unstarted) == 0) {
					throw new ThreadStateException("Thread has already been started");
				}
				

				// Thread_internal creates the new thread, but
				// blocks it until Start() is called later.
				system_thread_handle=Thread_internal(threadstart);

				if (system_thread_handle == (IntPtr) 0) {
					throw new SystemException ("Thread creation failed");
				}

				// Launch this thread
				Start_internal(system_thread_handle);

				// Mark the thread state as Running
				// (which is all bits
				// cleared). Therefore just remove the
				// Unstarted bit
				clr_state(ThreadState.Unstarted);
			}
		}

		[MonoTODO]
		public void Suspend() {
			if((state & ThreadState.Unstarted) != 0 || !IsAlive) {
				throw new ThreadStateException("Thread has not been started, or is dead");
			}

			set_state(ThreadState.SuspendRequested);
			// FIXME - somehow let the interpreter know that
			// this thread should now suspend
			Console.WriteLine ("WARNING: Thread.Suspend () partially implemented");
		}

		// Closes the system thread handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void Thread_free_internal(IntPtr handle);

		~Thread() {
			// Free up the handle
			Thread_free_internal(system_thread_handle);
		}

		private void set_state(ThreadState set) {
			lock(this) {
				state |= set;
			}
		}
		private void clr_state(ThreadState clr) {
			lock(this) {
				state &= ~clr;
			}
		}

#if NET_1_1
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static byte VolatileRead (ref byte address);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static double VolatileRead (ref double address);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static short VolatileRead (ref short address);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static int VolatileRead (ref int address);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static long VolatileRead (ref long address);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static IntPtr VolatileRead (ref IntPtr address);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static object VolatileRead (ref object address);

		[CLSCompliant(false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static sbyte VolatileRead (ref sbyte address);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static float VolatileRead (ref float address);

		[CLSCompliant (false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static ushort VolatileRead (ref ushort address);

		[CLSCompliant (false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static uint VolatileRead (ref uint address);

		[CLSCompliant (false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static ulong VolatileRead (ref ulong address);

		[CLSCompliant (false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static byte VolatileRead (ref UIntPtr address);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref byte address, byte value);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref double address, double value);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref short address, short value);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref int address, int value);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref long address, long value);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref IntPtr address, IntPtr value);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref object address, object value);

		[CLSCompliant(false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref sbyte address, sbyte value);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref float address, float value);

		[CLSCompliant (false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref ushort address, ushort value);

		[CLSCompliant (false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref uint address, uint value);

		[CLSCompliant (false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref ulong address, ulong value);

		[CLSCompliant (false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref UIntPtr address, UIntPtr value);
		
#endif
		
	}
}
