// Author:
//   Mario Martinez (mariom925@home.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using NUnit.Framework;

namespace Ximian.Mono.Tests
{
	/// <summary>
	///   Combines all unit tests for the System.XML.dll assembly
	///   into one test suite.
	/// </summary>
	public class AllTests : TestCase
	{
		public AllTests (string name) : base (name) {}

		public static ITest Suite {
			get {
				TestSuite suite =  new TestSuite ();
				suite.AddTest (new TestSuite (typeof (XmlTextReaderTests)));
				suite.AddTest (new TestSuite (typeof (XmlTextWriterTests)));
				suite.AddTest (new TestSuite (typeof (XmlNamespaceManagerTests)));
				suite.AddTest (new TestSuite (typeof (XmlAttributeTests)));
				suite.AddTest (new TestSuite (typeof (XmlDocumentTests)));
				suite.AddTest (new TestSuite (typeof (NameTableTests)));
				suite.AddTest (new TestSuite (typeof (XmlElementTests)));
				suite.AddTest (new TestSuite (typeof (XmlNodeListTests)));
				return suite;
			}
		}
	}
}
