//
// Ximian.Mono.Tests.TextWriterTraceListenerTest.cs
//
// Author:
//	John R. Hicks (angryjohn69@nc.rr.com)
//
// (C) 2002
using System;
using System.Diagnostics;
using NUnit.Framework;

namespace MonoTests
{
	public class DebugTest
	{
		private DebugTest()
		{
			
		}
		
		public static ITest Suite
		{
			get
			{
				TestSuite suite = new TestSuite();
				suite.AddTest(DebugTest1.Suite);
				return suite;
			}
		}
		
		private class DebugTest1 : TestCase
		{
			public DebugTest1(string name) : base(name)
			{
				
			}
			
			internal static ITest Suite
			{
				get
				{
					return new TestSuite(typeof(DebugTest1));
				}
			}
			
			protected override void SetUp()
			{
				Debug.Listeners.Add(new TextWriterTraceListener(Console.Error));	
			}
			
			protected override void TearDown()
			{
				
			}
			
			public void TestAssert()
			{
				Debug.Assert(false, "Testing Assertions");
			}
			
			public void TestFail()
			{
				Debug.Fail("Testing Fail method");
			}
			
			public void TestWrite()
			{
				Debug.Write("Testing Write", "Testing the output of the Write method");
			}
			
			public void TestWriteIf()
			{
				Debug.WriteIf(true, "Testing WriteIf");
				Debug.WriteIf(false, "Testing WriteIf", "Passed false");
			}
			
			public void TestWriteLine()
			{
				Debug.WriteLine("Testing WriteLine method");
			}
			
			public void TestWriteLineIf()
			{
				Debug.WriteLineIf(true, "Testing WriteLineIf");
				Debug.WriteLineIf(false, "Testing WriteLineIf", "Passed false");
			}
		}
	}
}
