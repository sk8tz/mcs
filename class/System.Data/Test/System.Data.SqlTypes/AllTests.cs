// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Copyright 2002 Tim Coleman
//

using NUnit.Framework;

namespace MonoTests.System.Data.SqlTypes
{
	/// <summary>
	///   Combines all unit tests for the System.Data.dll assembly
	///   into one test suite.
	/// </summary>
	public class AllTests : TestCase
	{
		public AllTests (string name) : base (name) {}

		public static ITest Suite {
			get {
				TestSuite suite =  new TestSuite ();
				suite.AddTest (new TestSuite (typeof (SqlBooleanTest)));
				suite.AddTest (new TestSuite (typeof (SqlByteTest)));
				suite.AddTest (new TestSuite (typeof (SqlInt32Test)));
				return suite;
			}
		}
	}
}
