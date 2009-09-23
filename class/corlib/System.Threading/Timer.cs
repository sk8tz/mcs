//
// System.Threading.Timer.cs
//
// Authors:
// 	Dick Porter (dick@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2001, 2002 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2009 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;
using System.Collections;

namespace System.Threading
{
#if NET_2_0
	[ComVisible (true)]
#endif
	public sealed class Timer : MarshalByRefObject, IDisposable
	{
		static Scheduler scheduler = Scheduler.Instance;
#region Timer instance fields
		TimerCallback callback;
		object state;
		long due_time_ms;
		long period_ms;
		long next_run; // in ticks. Only 'Scheduler' can change it except for new timers without due time.
		bool disposed;
#endregion
		public Timer (TimerCallback callback, object state, int dueTime, int period)
		{
			if (dueTime < -1)
				throw new ArgumentOutOfRangeException ("dueTime");

			if (period < -1)
				throw new ArgumentOutOfRangeException ("period");

			Init (callback, state, dueTime, period);
		}

		public Timer (TimerCallback callback, object state, long dueTime, long period)
		{
			if (dueTime < -1)
				throw new ArgumentOutOfRangeException ("dueTime");

			if (period < -1)
				throw new ArgumentOutOfRangeException ("period");

			Init (callback, state, dueTime, period);
		}

		public Timer (TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
			: this (callback, state, (long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds)
		{
		}

		[CLSCompliant(false)]
		public Timer (TimerCallback callback, object state, uint dueTime, uint period)
			: this (callback, state, (long) dueTime, (long) period)
		{
		}

#if NET_2_0
		public Timer (TimerCallback callback)
		{
			Init (callback, this, Timeout.Infinite, Timeout.Infinite);
		}
#endif

		void Init (TimerCallback callback, object state, long dueTime, long period)
		{
			if (callback == null)
				throw new ArgumentNullException ("callback");
			
			this.callback = callback;
			this.state = state;

			Change (dueTime, period, true);
		}

		public bool Change (int dueTime, int period)
		{
			return Change ((long)dueTime, (long)period);
		}

		public bool Change (TimeSpan dueTime, TimeSpan period)
		{
			return Change ((long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds);
		}

		[CLSCompliant(false)]
		public bool Change (uint dueTime, uint period)
		{
			if (dueTime > Int32.MaxValue)
				throw new NotSupportedException ("Due time too large");

			if (period > Int32.MaxValue)
				throw new NotSupportedException ("Period too large");

			return Change ((long) dueTime, (long) period);
		}

		public void Dispose ()
		{
			if (disposed)
				return;

			disposed = true;
			scheduler.Remove (this);
		}

		public bool Change (long dueTime, long period)
		{
			return Change (dueTime, period, false);
		}

		bool Change (long dueTime, long period, bool first)
		{
			if(dueTime > 4294967294)
				throw new NotSupportedException ("Due time too large");

			if(period > 4294967294)
				throw new NotSupportedException ("Period too large");

			if (dueTime < -1)
				throw new ArgumentOutOfRangeException ("dueTime");

			if (period < -1)
				throw new ArgumentOutOfRangeException ("period");

			if (disposed)
				return false;

			due_time_ms = dueTime;
			period_ms = period;
			long nr;
			if (dueTime == 0) {
				nr = 0; // Due now
			} else if (dueTime == Timeout.Infinite) {
				nr = long.MaxValue;
				/* No need to call Change () */
				if (first) {
					next_run = nr;
					return true;
				}
			} else {
				nr = dueTime * TimeSpan.TicksPerMillisecond + DateTime.GetTimeMonotonic ();
			}

			scheduler.Change (this, nr);
			return true;
		}

		public bool Dispose (WaitHandle notifyObject)
		{
			Dispose ();
			NativeEventCalls.SetEvent_internal (notifyObject.Handle);
			return true;
		}

		class TimerComparer : IComparer {
			public int Compare (object x, object y)
			{
				if (!(x is Timer))
					return -1;
				if (!(y is Timer))
					return 1;
				long nr1 = ((Timer) x).next_run;
				long nr2 = ((Timer) y).next_run;
				long result = nr1 - nr2;
				return result > 0 ? 1 : result < 0 ? -1 : 0;
			}

		}

		class Scheduler {
			static Scheduler instance;
			SortedList list;

			static Scheduler ()
			{
				instance = new Scheduler ();
			}

			public static Scheduler Instance {
				get { return instance; }
			}

			private Scheduler ()
			{
				list = new SortedList (new TimerComparer (), 1024);
				Thread thread = new Thread (SchedulerThread);
				thread.IsBackground = true;
				thread.Start ();
			}

			public void Remove (Timer timer)
			{
				lock (this) {
					int pos = InternalRemove (timer);
					if (pos == 0)
						Monitor.Pulse (this);
				}
			}

			public void Change (Timer timer, long new_next_run)
			{
				lock (this) {
					int pos = -1;
					// Don't try to remove items that are not initialized or not in the list
					if (timer.next_run != 0 && timer.next_run != Int64.MaxValue)
						pos = InternalRemove (timer);
					int new_pos = -1;
					if (!timer.disposed) {
						// We should only change next_run after removing and before adding
						timer.next_run = new_next_run;
						Add (timer);
						if (list.GetByIndex (0) == timer)
							new_pos = 0;
					}
					if (pos == 0 || new_pos == 0)
						Monitor.Pulse (this);
				}
			}

			// This should be the only caller to list.Add!
			void Add (Timer timer)
			{
				// Make sure there are no collisions (10000 ticks == 1ms, so we should be safe here)
				int idx = list.IndexOfKey (timer);
				if (idx != -1) {
					while (true) {
						idx++;
						timer.next_run++;
						if (idx >= list.Count)
							break;
						Timer t2 = (Timer) list.GetByIndex (idx);
						if (t2.next_run != timer.next_run)
							break;
					}
				}
				list.Add (timer, timer);
				//PrintList ();
			}

			int InternalRemove (Timer timer)
			{
				int idx = list.IndexOfKey (timer);
				if (idx >= 0)
					list.RemoveAt (idx);
				return idx;
			}

			void SchedulerThread ()
			{
				Thread.CurrentThread.Name = "Timer-Scheduler";
				ArrayList new_time = new ArrayList (512);
				while (true) {
					long ticks = DateTime.GetTimeMonotonic ();
					lock (this) {
						//PrintList ();
						int i;
						int count = list.Count;
						for (i = 0; i < count; i++) {
							Timer timer = (Timer) list.GetByIndex (i);
							if (timer.next_run > ticks)
								break;

							list.RemoveAt (i);
							count--;
							i--;
							ThreadPool.QueueUserWorkItem (new WaitCallback (timer.callback), timer.state);
							long period = timer.period_ms;
							long due_time = timer.due_time_ms;
							bool no_more = (period == -1 || ((period == 0 || period == Timeout.Infinite) && due_time != Timeout.Infinite));
							if (no_more) {
								timer.next_run = Int64.MaxValue;
							} else {
								timer.next_run = DateTime.GetTimeMonotonic () + TimeSpan.TicksPerMillisecond * timer.period_ms;
								if (timer.next_run > ticks) {
									Add (timer); // Safe to add here
									count++;
								} else {
									new_time.Add (timer); // Not safe, add it once we're done
								}
							}
						}

						// Reschedule timers with a new due time
						count = new_time.Count;
						for (i = 0; i < count; i++) {
							Timer timer = (Timer) new_time [i];
							Add (timer);
						}
						new_time.Clear ();
						ShrinkIfNeeded (new_time, 512);

						// Shrink the list
						int capacity = list.Capacity;
						count = list.Count;
						if (capacity > 1024 && count > 0 && (capacity / count) > 3)
							list.Capacity = count * 2;

						int ms_wait = -1;
						long min_next_run = Int64.MaxValue;
						if (list.Count > 0)
							min_next_run = ((Timer) list.GetByIndex (0)).next_run;

						//PrintList ();
						if (min_next_run != Int64.MaxValue) {
							long diff = min_next_run - ticks; 
							ms_wait = (int)(diff / TimeSpan.TicksPerMillisecond);
							if (ms_wait < 0)
								ms_wait = 0;
						}

						// Wait until due time or a timer is changed and moves from/to the first place in the list.
						Monitor.Wait (this, ms_wait);
					}
				}
			}

			void ShrinkIfNeeded (ArrayList list, int initial)
			{
				int capacity = list.Capacity;
				int count = list.Count;
				if (capacity > initial && count > 0 && (capacity / count) > 3)
					list.Capacity = count * 2;
			}

			void PrintList ()
			{
				Console.WriteLine ("BEGIN--");
				for (int i = 0; i < list.Count; i++) {
					Timer timer = (Timer) list.GetByIndex (i);
					Console.WriteLine ("{0}: {1}", i, timer.next_run);
				}
				Console.WriteLine ("END----");
			}
		}
	}
}

