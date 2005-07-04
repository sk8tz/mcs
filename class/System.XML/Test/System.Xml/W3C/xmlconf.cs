using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Text;

namespace MonoTests.W3C_xmlconf {
	using NUnit.Core;
	using NUnit.Framework;

	abstract class BaseTests
	{
		TestSuite _suite;


		#region test list fields
		protected readonly ArrayList ignoredTests = new ArrayList ();
		protected readonly ArrayList knownFailures = new ArrayList ();
		protected readonly ArrayList fixmeList = new ArrayList ();
		protected readonly ArrayList netFailures = new ArrayList ();
		#endregion

		#region ReadStrings ()
		static void ReadStrings (ArrayList array, string filename) {
			if (!File.Exists (filename))
				return;

			using (StreamReader reader = new StreamReader (filename)) {
				foreach (string s_ in reader.ReadToEnd ().Split ("\n".ToCharArray ())) {
					string s = s_.Trim ();
					if (s.Length > 0)
						array.Add (s);
				}
			}
		}
		#endregion

		protected BaseTests (TestSuite suite)
			:this ()
		{
			_suite = suite;
		}

		private BaseTests ()
		{
			ReadStrings (ignoredTests, "ignored.lst");
			ReadStrings (knownFailures, "knownFailures.lst");
			ReadStrings (fixmeList, "fixme.lst");
			ReadStrings (netFailures, "net-failed.lst");
		}

		protected void BuildSuite ()
		{
			XmlDocument catalog = new XmlDocument ();
			catalog.Load ("xmlconf/xmlconf.xml");
			
			foreach (XmlElement test in catalog.SelectNodes ("//TEST")) {
				string testId = test.GetAttribute ("ID");
				
				ProcessTest (testId, test);
			}
		}

		protected virtual bool InverseResult {
			get {return false;}
		}

		protected virtual void ProcessTest (string testId, XmlElement test)
		{
			if (ignoredTests.Contains (testId))
				return;

			if (netFailures.Contains (testId))
				return;

			_suite.Add (new TestFromCatalog (testId, test, InverseResult));
		}
	}

	class AllTests: BaseTests
	{
		[Suite]
		static public TestSuite Suite{
			get {
				TestSuite suite = new TestSuite ("W3C_xmlconf.All");
				AllTests tests = new AllTests (suite);
				tests.BuildSuite ();
				return suite;
			}
		}

		AllTests (TestSuite suite)
			: base (suite)
		{
		}
	}

	class CleanTests: BaseTests {
		[Suite]
		static public TestSuite Suite{
			get {
				TestSuite suite = new TestSuite ("W3C_xmlconf.Clean");
				CleanTests tests = new CleanTests (suite);
				tests.BuildSuite ();
				return suite;
			}
		}

		CleanTests (TestSuite suite)
			: base (suite)
		{
		}

		protected override void ProcessTest(string testId, XmlElement test)
		{
			if (knownFailures.Contains (testId) || fixmeList.Contains (testId))
				return;

			base.ProcessTest (testId, test);
		}
	}

	class KnownFailureTests: BaseTests {
		[Suite]
		static public TestSuite Suite{
			get {
				TestSuite suite = new TestSuite ("W3C_xmlconf.KnownFailures");
				KnownFailureTests tests = new KnownFailureTests (suite);
				tests.BuildSuite ();
				return suite;
			}
		}

		KnownFailureTests (TestSuite suite)
			: base (suite)
		{
		}

		protected override bool InverseResult {
			get {return true;}
		}

		protected override void ProcessTest(string testId, XmlElement test)
		{
			if (!knownFailures.Contains (testId) && !fixmeList.Contains (testId))
				return;

			base.ProcessTest (testId, test);
		}
	}

	class TestFromCatalog: NUnit.Core.TestCase
	{
		XmlElement _test;
		string _stackTrace;
		bool _inverseResult;

		public TestFromCatalog (string testId, XmlElement test, bool inverseResult)
			:base (null, testId)
		{
			_test = test;
			_inverseResult = inverseResult;
		}

		bool TestNonValidating (string uri)
		{
			try {
				XmlTextReader trd = new XmlTextReader (uri);
				new XmlDocument ().Load (trd);
				return true;
			}
			catch (Exception e) {
				_stackTrace = e.StackTrace;
				return false;
			}
		}

		bool TestValidating (string uri)
		{
			try {
				XmlTextReader rd = new XmlTextReader (uri);
				XmlValidatingReader vrd = new XmlValidatingReader (rd);
				new XmlDocument ().Load (vrd);
				return true;
			}
			catch (Exception e) {
				_stackTrace = e.StackTrace; //rewrites existing, possibly, but it's ok
				return false;
			}
		}

		public override void Run (TestCaseResult res)
		{
			string type = _test.GetAttribute ("TYPE");
			if (type == "error")
				res.Success ();

			Uri baseUri = new Uri (_test.BaseURI);
			Uri testUri = new Uri (baseUri, _test.GetAttribute ("URI"));

			bool nonValidatingPassed = TestNonValidating (testUri.ToString ());
			bool validatingPassed = TestValidating (testUri.ToString ());
		
			bool isok = isOK (type, nonValidatingPassed, validatingPassed);
			string message="";
			if (_inverseResult) {
				isok = !isok;
				message = "The following test was FIXED:\n";
			}

			if (isok)
				res.Success ();
			else {
				message += "type:"+type;
				message += " non-validating passed:"+nonValidatingPassed.ToString();
				message += " validating passed:"+validatingPassed.ToString();
				message += " description:"+_test.InnerText;
				res.Failure (message, _stackTrace);
			}
		}

		static bool isOK (string type, bool nonValidatingPassed, bool validatingPassed)
		{
			switch (type) {
			case "valid":
				return nonValidatingPassed && validatingPassed;
			case "invalid":
				return nonValidatingPassed && !validatingPassed;
			case "not-wf":
				return !nonValidatingPassed && !validatingPassed;
			case "error":
				return true; //readers can optionally accept or reject errors
			default:
				throw new ArgumentException ("Wrong test type", "type");
			}
		}
	}
}
