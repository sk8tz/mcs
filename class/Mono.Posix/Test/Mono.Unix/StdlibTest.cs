//
// StdlibTest.cs:
// 	NUnit Test Cases for Mono.Unix.Stdlib
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004-2005 Jonathan Pryor
// 

using System;
using System.Text;

using NUnit.Framework;

using Mono.Unix;

namespace MonoTests.Mono.Unix {

	[TestFixture]
	public class StdlibTest
	{
		private class SignalTest {
			public Signum signalReceived;

			public void Handler (int sn)
			{
				signalReceived = UnixConvert.ToSignum (sn);
			}
		}

		[Test, Ignore ("Sending Signals from inside Mono hangs the program")]
#if !NET_2_0
		// .NET 1.1 marshals delegates as Stdcall functions, while signal(3)
		// expects a Cdecl function.  Result: stack corruption.
		// DO NOT USE Stdlib.signal under .NET 1.1!
		// .NET 2.0 allows us to specify how delegates should be marshaled, so
		// this isn't an issue there.
		[Category ("NotDotNet")]
#endif
		public void Signal ()
		{
			SignalTest st = new SignalTest ();
			// Make sure handler is JITed so we don't JIT from signal context
			st.Handler (9);

			// Insert handler
			SignalHandler oh = Stdlib.signal (Signum.SIGUSR1, 
					new SignalHandler (st.Handler));

			st.signalReceived = ~Signum.SIGUSR1;

			// Send signal
			Stdlib.raise (Signum.SIGUSR1);

			Assert.IsTrue (st.signalReceived == Signum.SIGUSR1,
					"#IH: Signal handler not invoked for SIGUSR1");

			// Reset old signal
			Stdlib.signal (Signum.SIGUSR1, oh);

#if IGNORE
			signalReceived = ~Signum.SIGUSR1;
			Stdlib.raise (Signum.SIGUSR1);

			Assert.IsFalse (signalReceived == Signum.SIGUSR1,
					"#IH: Signal Handler invoked when it should have been removed!");
#endif
		}

		[Test]
		// MSVCRT.DLL doesn't export snprintf(3).
		[Category ("NotDotNet")]
		public void Snprintf ()
		{
			StringBuilder s = new StringBuilder (1000);
			Stdlib.snprintf (s, "hello, %s world!\n");
			Assert.AreEqual (s.ToString(), "hello, %s world!\n", 
					"#SNPF: string not echoed");
			s = new StringBuilder (1000);
			Stdlib.snprintf (s, "yet another %s test", "simple");
			Assert.AreEqual (s.ToString(), "yet another simple test",
					"#SNPF: string argument not printed");
			s = new StringBuilder (1000);
			string fmt = 
@"this is another test:
	  char: '%c'
	 short: %i
	   int: %i
	  long: %lli
	 float: %g
	double: %g" + "\n";
		Stdlib.snprintf (s, fmt, 'a', (short) 16, 32, (long) 64, 32.23f, 64.46);
			string expected = 
@"this is another test:
	  char: 'a'
	 short: 16
	   int: 32
	  long: 64
	 float: 32.23
	double: 64.46" + "\n";
			Assert.AreEqual (s.ToString(), expected,
					"#SNPF: printf of many builtin types failed");
		}
	}
}

