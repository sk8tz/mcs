//
// TimerTest.cs - NUnit test cases for System.Threading.Timer
//
// Author:
//   Zoltan Varga (vargaz@freemail.hu)
//   Rafael Ferreira (raf@ophion.org)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Threading;
using System.Collections;

namespace MonoTests.System.Threading {

	//
	// This whole test seems to fail randomly. Either
	// - It is relying on a race it might not win (that the timer code runs)
	// - We have a very obscure bug with appdomains.
	//
	// Am going with door #1, but it would be nice to investigate this.
	// -- Ben
	//
	[TestFixture]
	public class TimerTest {
		// this bucket is used to avoid non-theadlocal issues
		class Bucket {
			public int count;
		}
		[SetUp]
		public void setup() {
			//creating a timer that will never run just to make sure the
			// scheduler is warm for the unit tests
			// this makes fair for the "DueTime" test since it 
			// doesn't have to wait for the scheduler thread to be 
			// created. 
			new Timer(null,null,Timeout.Infinite,0);
		}
		
		[Test]
		public void TestDueTime ()
		{
			Bucket bucket = new Bucket();
			Timer t = new Timer (new TimerCallback (Callback), bucket, 200, Timeout.Infinite);
			Thread.Sleep (50);
			Assert.AreEqual (0, bucket.count);
			Thread.Sleep (200);
			Assert.AreEqual (1, bucket.count);
			Thread.Sleep (500);
			Assert.AreEqual (1, bucket.count);
			t.Change (10, 10);
			Thread.Sleep (1000);
			Assert.IsTrue(bucket.count > 20);
			t.Dispose ();
		}

		[Test]
		public void TestChange ()
		{
			Bucket bucket = new Bucket();
			Timer t = new Timer (new TimerCallback (Callback), bucket, 1, 1);
			Thread.Sleep (500);
			int c = bucket.count;
			Assert.IsTrue(c > 20);
			t.Change (100, 100);
			Thread.Sleep (500);
			Assert.IsTrue(bucket.count <= c + 6);
			t.Dispose ();
		}

		[Test]
		public void TestZeroDueTime () {
			Bucket bucket = new Bucket();

			Timer t = new Timer (new TimerCallback (Callback), bucket, 0, Timeout.Infinite);
			Thread.Sleep (100);
			Assert.AreEqual (1, bucket.count);
			t.Change (0, Timeout.Infinite);
			Thread.Sleep (100);
			Assert.AreEqual (2, bucket.count);
			t.Dispose ();
		}
		[Test]
		public void TestDispose ()
		{	
			Bucket bucket = new Bucket();
			Timer t = new Timer (new TimerCallback (Callback), bucket, 10, 10);
			Thread.Sleep (200);
			t.Dispose ();
			Thread.Sleep (20);
			int c = bucket.count;
			Assert.IsTrue(bucket.count > 5);
			Thread.Sleep (200);
			Assert.AreEqual (c, bucket.count);
		}

		[Test] // bug #78208
		public void TestDispose2 ()
		{
			Timer t = new Timer (new TimerCallback (Callback), null, 10, 10);
			t.Dispose ();
			t.Dispose ();
		}
		
		[Test]
		public void TestHeavyCreationLoad() {
			int i = 0;
			Bucket b = new Bucket();
			while (i < 500) {
				new Timer(new TimerCallback(Callback),b,10,Timeout.Infinite);
				i++;
			}
			// 1000 * 10 msec = 10,000 msec or 10 sec - if everything goes well
			Thread.Sleep(12*500);
			Assert.AreEqual(500,b.count);
			
		}
		[Test]
		public void TestQuickDisposeDeadlockBug() {
			int i = 0;
			Bucket b = new Bucket();
			ArrayList timers = new ArrayList();
			while (i < 500) {
				Timer t = new Timer(new TimerCallback(Callback),b,10,Timeout.Infinite);
				timers.Add(t);
				i++;
				t.Dispose();
			}
			Thread.Sleep(11*500);
		}
		[Test]
		public void TestInt32MaxDelay() {
			Bucket b = new Bucket();
			new Timer(new TimerCallback(Callback),b,Int32.MaxValue,Timeout.Infinite);
			Thread.Sleep(50);
			Assert.AreEqual(0,b.count);
			
		}
		[Test]
		public void TestInt32MaxPeriod() {
			Bucket b = new Bucket();
			new Timer(new TimerCallback(Callback),b,0,Int32.MaxValue);
			Thread.Sleep(50);
			Assert.AreEqual(1,b.count);
			
		}
		[Test]
		public void TestNegativeDelay() {
			Bucket b = new Bucket();
			try {
				new Timer(new TimerCallback(Callback),b,-10,Timeout.Infinite);
			} catch (ArgumentOutOfRangeException) {
				return;
			}
			Assert.Fail();
			
		}
		[Test]
		public void TestNegativePeriod() {
			Bucket b = new Bucket();
			try {
				new Timer(new TimerCallback(Callback),b,0,-10);
			} catch (ArgumentOutOfRangeException) {
				return;
			}
			Assert.Fail();
		}

		[Test]
		public void TestDelayZeroPeriodZero() {
			Bucket b = new Bucket();
			new Timer(new TimerCallback(Callback),b,0,0);
			Thread.Sleep(50);
			Assert.IsTrue(b.count > 45);
			
		}

		[Category("NotWorking")]
		public void TestDisposeOnCallback () {
		
			Timer t1 = null;
			t1 = new Timer (new TimerCallback (CallbackTestDisposeOnCallback), t1, 0, 10);
			Thread.Sleep (200);
			Assert.IsNull(t1);
			
		}
		private void CallbackTestDisposeOnCallback (object foo)
		{
			((Timer)foo).Dispose();
		}
		
		private void Callback (object foo)
		{
			Bucket b = foo as Bucket;
			b.count++;
		}
	}
}
