// MonoTests.AllTests, System.dll
//
// Author:
//   Mario Martinez (mariom925@home.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using NUnit.Framework;
namespace MonoTests
{
	/// <summary>
	///   Combines all unit tests for the System.dll assembly
	///   into one test suite.
	/// </summary>
	public class AllTests : TestCase
	{
		public AllTests(string name) : base(name) {}

		public static ITest Suite {
			get {
				TestSuite suite = new TestSuite();
				suite.AddTest (System.Net.DnsTest.Suite);
				suite.AddTest (System.Collections.Specialized.NameValueCollectionTest.Suite);
				suite.AddTest (System.Collections.Specialized.StringCollectionTest.Suite);
				suite.AddTest (System.Text.RegularExpressions.AllTests.Suite);
			        suite.AddTest (System.Diagnostics.AllTests.Suite);
				return suite;
			}
		}
	}
}
