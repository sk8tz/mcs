//
// System.Threading.ThreadPool
//
// Author:
//   Patrik Torstensson
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) Patrik Torstensson
//
using System;
using System.Collections;

namespace System.Threading {
	/// <summary> (Patrik T notes)
	/// This threadpool is focused on saving resources not giving max performance. 
	/// 
	/// Note, this class is not perfect but it works. ;-) Should also replace
	/// the queue with an internal one (performance)
	/// 
	/// This class should also use a specialized queue to increase performance..
	/// </summary>
	/// 
	public sealed class ThreadPool {
		internal struct ThreadPoolWorkItem {
			public WaitCallback _CallBack;
			public object _Context;
		}

		private int _ThreadTimeout;

		private long _MaxThreads;
		private long _CurrentThreads;
		private long _ThreadsInUse;
		private long _RequestInQueue;
		private long _ThreadCreateTriggerRequests;

		private Thread _MonitorThread;
		private Queue _RequestQueue;

		private ArrayList _Threads;
		private ManualResetEvent _DataInQueue; 

		static ThreadPool _Threadpool;

		static ThreadPool() {
			_Threadpool = new ThreadPool();
		}

		private ThreadPool() {
			// 30 sec timeout default
			_ThreadTimeout = 30 * 1000; 

			// Used to signal that there is data in the queue
			_DataInQueue = new ManualResetEvent(false);
         
			_Threads = ArrayList.Synchronized(new ArrayList());

			// Holds requests..
			_RequestQueue = Queue.Synchronized(new Queue(128));

			// TODO: This should be 2 x number of CPU:s in the box
			_MaxThreads = 16;
			_CurrentThreads = 0;
			_RequestInQueue = 0;
			_ThreadsInUse = 0;
			_ThreadCreateTriggerRequests = 5;

			// TODO: This temp starts one thread, remove this..
			CheckIfStartThread();

			// Keeps track of requests in the queue and increases the number of threads if needed

			// PT: Disabled - causes problems during shutdown
			//_MonitorThread = new Thread(new ThreadStart(MonitorThread));
			//_MonitorThread.Start();
		}

		internal void RemoveThread() {
			Interlocked.Decrement(ref _CurrentThreads);
			_Threads.Remove(Thread.CurrentThread);
		}

		internal void CheckIfStartThread() {
			bool bCreateThread = false;

			if (_CurrentThreads == 0) {
				bCreateThread = true;
			}

			if ((	_MaxThreads == -1 || _CurrentThreads < _MaxThreads) && 
				_ThreadsInUse > 0 && 
				_RequestInQueue >= _ThreadCreateTriggerRequests) {
				bCreateThread = true;
			}

			if (bCreateThread) {
				Interlocked.Increment(ref _CurrentThreads);
      
				Thread Start = new Thread(new ThreadStart(WorkerThread));
				Start.IsThreadPoolThreadInternal = true;
				Start.IsBackground = true;
				Start.Start();
            
				_Threads.Add(Start);
			}
		}

		internal void AddItem(ref ThreadPoolWorkItem Item) {
			_RequestQueue.Enqueue(Item);
			if (Interlocked.Increment(ref _RequestInQueue) == 1) {
				_DataInQueue.Set();
			}
		}

		// Work Thread main function
		internal void WorkerThread() {
			bool bWaitForData = true;

			while (true) {
				if (bWaitForData) {
					if (!_DataInQueue.WaitOne(_ThreadTimeout, false)) {
						// Keep one thread running
						if (_CurrentThreads > 1) {
							// timeout
							RemoveThread();
							return;
						}
						continue;
					}
				}

				Interlocked.Increment(ref _ThreadsInUse);

				// TODO: Remove when we know how to stop the watch thread
				CheckIfStartThread();

				try {
					ThreadPoolWorkItem oItem = (ThreadPoolWorkItem) _RequestQueue.Dequeue();

					if (Interlocked.Decrement(ref _RequestInQueue) == 0) {
						_DataInQueue.Reset();
					}

					oItem._CallBack(oItem._Context);
				}
				catch (InvalidOperationException) {
					// Queue empty
					bWaitForData = true;
				}
				catch (ThreadAbortException) {
					// We will leave here.. (thread abort can't be handled)
					RemoveThread();
				}
				finally {
					Interlocked.Decrement(ref _ThreadsInUse);
				}
			}
		}
		
		/* This is currently not in use
		 
		internal void MonitorThread() {
			while (true) {
			if (_DataInQueue.WaitOne ()) {
				CheckIfStartThread();
			}

			Thread.Sleep(500);
			}
		}
		
		*/
		internal bool QueueUserWorkItemInternal(WaitCallback callback) {
			return QueueUserWorkItem(callback, null);
		}

		internal bool QueueUserWorkItemInternal(WaitCallback callback, object context) {
			ThreadPoolWorkItem Item = new ThreadPoolWorkItem();

			Item._CallBack = callback;
			Item._Context = context;

			AddItem(ref Item);

			// LAMESPEC: Return value? should use exception here if anything goes wrong
			return true;
		}

		public static bool BindHandle(IntPtr osHandle) {
			throw new NotSupportedException("This is a win32 specific method, not supported Mono");
		}

		public static bool QueueUserWorkItem(WaitCallback callback) {
			return _Threadpool.QueueUserWorkItemInternal(callback);
		}

		public static bool QueueUserWorkItem(WaitCallback callback, object state) {
			return _Threadpool.QueueUserWorkItemInternal(callback, state);
		}

		public static bool UnsafeQueueUserWorkItem(WaitCallback callback, object state) {
			return _Threadpool.QueueUserWorkItemInternal(callback, state);
		}

		static TimeSpan GetTSFromMS (long ms)
		{
			if (ms < -1)
				throw new ArgumentOutOfRangeException ("millisecondsTimeOutInterval", "timeout < -1");

			return new TimeSpan (0, 0, 0, 0, (int) ms);
		}

		public static RegisteredWaitHandle RegisterWaitForSingleObject (WaitHandle waitObject,
										WaitOrTimerCallback callback,
										object state,
										int millisecondsTimeOutInterval,
										bool executeOnlyOnce)
		{
			TimeSpan ts = GetTSFromMS ((long) millisecondsTimeOutInterval);
			return RegisterWaitForSingleObject (waitObject, callback, state, ts, executeOnlyOnce);
		}

		public static RegisteredWaitHandle RegisterWaitForSingleObject (WaitHandle waitObject,
										WaitOrTimerCallback callback,
										object state,
										long millisecondsTimeOutInterval,
										bool executeOnlyOnce)
		{
			TimeSpan ts = GetTSFromMS (millisecondsTimeOutInterval);
			return RegisterWaitForSingleObject (waitObject, callback, state, ts, executeOnlyOnce);
		}

		public static RegisteredWaitHandle RegisterWaitForSingleObject (WaitHandle waitObject,
										WaitOrTimerCallback callback,
										object state,
										TimeSpan timeout,
										bool executeOnlyOnce)
		{
			long ms = (long) timeout.TotalMilliseconds;
			if (ms < -1)
				throw new ArgumentOutOfRangeException ("timeout", "timeout < -1");

			if (ms > Int32.MaxValue)
				throw new NotSupportedException ("Timeout is too big. Maximum is Int32.MaxValue");

			RegisteredWaitHandle waiter = new RegisteredWaitHandle (waitObject, callback, state, timeout, executeOnlyOnce);
			_Threadpool.QueueUserWorkItemInternal (new WaitCallback (waiter.Wait), null);
			return waiter;
		}

		[CLSCompliant(false)]
		public static RegisteredWaitHandle RegisterWaitForSingleObject (WaitHandle waitObject,
										WaitOrTimerCallback callback,
										object state,
										uint millisecondsTimeOutInterval,
										bool executeOnlyOnce)
		{
			TimeSpan ts = GetTSFromMS ((long) millisecondsTimeOutInterval);
			return RegisterWaitForSingleObject (waitObject, callback, state, ts, executeOnlyOnce);
		}

		[MonoTODO]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callback, object state, int millisecondsTimeOutInterval, bool executeOnlyOnce) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callback, object state, long millisecondsTimeOutInterval, bool executeOnlyOnce) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callback, object state, TimeSpan timeout, bool executeOnlyOnce) {
			throw new NotImplementedException();
		}

		[CLSCompliant(false)][MonoTODO]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callback, object state, uint millisecondsTimeOutInterval, bool executeOnlyOnce) {
			throw new NotImplementedException();
		}
	}
}
