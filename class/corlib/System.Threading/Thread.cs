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
		private bool threadpool_thread = false;
		private ThreadState state = ThreadState.Unstarted;
		private object abort_exc;
		internal object abort_state;
		/* thread_id is only accessed from unmanaged code */
		private int thread_id;
		
		/* start_notify is used by the runtime to signal that Start()
		 * is ok to return
		 */
		private IntPtr start_notify;
		
		public static Context CurrentContext {
			get {
				Context ctx = AppDomain.InternalGetContext ();
				if (ctx == null) {
					ctx = Context.DefaultContext;
					AppDomain.InternalSetContext (ctx);
				}

				return ctx;
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

		public static void FreeNamedDataSlot(string name) {
			if (datastorehash == null)
				InitDataStoreHash ();
			LocalDataStoreSlot slot=(LocalDataStoreSlot)datastorehash[name];

			if(slot!=null) {
				datastorehash.Remove(slot);
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
		public extern static void ResetAbort();

		public static void SetData(LocalDataStoreSlot slot,
					   object data) {
			Hashtable slothash=GetTLSSlotHash();

			if(slothash[slot]!=null) {
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

			// This is a two-stage thread launch.  Thread_internal
			// creates the new thread, but blocks it until
			// Start() is called later.
			system_thread_handle=Thread_internal(start);

			// Should throw an exception here if
			// Thread_internal returns NULL
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

		[MonoTODO]
		public CultureInfo CurrentCulture {
			get {
				if (current_culture == null)
					current_culture = new CultureInfo ("");
				return current_culture;
			}
			
			set {
				current_culture = value;
			}
		}

		[MonoTODO]
		public CultureInfo CurrentUICulture {
			get {
				// FIXME
				return(CurrentCulture);
			}
			
			set {
				// FIXME
				CurrentCulture=value;
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
				// LAMESPEC: is a Stopped or Suspended
				// thread dead?
				ThreadState curstate=state;
				
				if((curstate & ThreadState.Aborted) != 0 ||
				   (curstate & ThreadState.AbortRequested) != 0 ||
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

		private string thread_name=null;
		
		public string Name {
			get {
				return(thread_name);
			}
			
			set {
				thread_name=value;
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

		public void Abort() {
			Abort (null);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern void Abort (object stateInfo);

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
	}
}
