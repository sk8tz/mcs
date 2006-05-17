//
// SwitchesTest.cs:
// 		NUnit Test Cases for System.Diagnostics.BooleanSwitch and
// 		System.Diagnostics.TraceSwitch
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2002 Jonathan Pryor
// (C) 2003 Martin Willemoes Hansen
//

using NUnit.Framework;
using System;
using System.Text;
using System.Collections;
using System.Configuration;
using System.Diagnostics;

namespace MonoTests.System.Diagnostics {

	class TestNewSwitch : Switch {
		private string v;
		private StringBuilder ops = new StringBuilder ();
		private const string expected = 
			".ctor\n" +
			"get_Value\n" +
			"OnSwitchSettingChanged\n" +
			"GetSetting\n";

		public TestNewSwitch (string name, string desc)
			: base (name, desc)
		{
			ops.Append (".ctor\n");
		}

		public string Value {
			get {
				ops.Append ("get_Value\n");
				// ensure that the .config file is read in
				int n = base.SwitchSetting;
				// remove warning about unused variable
				n = n;
				return v;
			}
		}

		public bool Validate ()
		{
			return expected == ops.ToString();
		}

		private void GetSetting ()
		{
			ops.Append ("GetSetting\n");
			IDictionary d = (IDictionary) ConfigurationSettings.GetConfig ("system.diagnostics");
			if (d != null) {
				d = (IDictionary) d ["switches"];
				if (d != null) {
					v = d [DisplayName].ToString();
				}
			}
		}

		protected override void OnSwitchSettingChanged ()
		{
			ops.Append ("OnSwitchSettingChanged\n");

			GetSetting ();
		}
	}

	[TestFixture]
	public class SwitchesTest : Assertion {
    
		private static BooleanSwitch bon = new BooleanSwitch ("bool-true", "");
		private static BooleanSwitch bon2 = new BooleanSwitch ("bool-true-2", "");
		private static BooleanSwitch bon3 = new BooleanSwitch ("bool-true-3", "");
		private static BooleanSwitch boff = new BooleanSwitch ("bool-false", "");
		private static BooleanSwitch boff2 = new BooleanSwitch ("bool-default", "");

		private static TraceSwitch toff = new TraceSwitch ("trace-off", "");
		private static TraceSwitch terror = new TraceSwitch ("trace-error", "");
		private static TraceSwitch twarning = new TraceSwitch ("trace-warning", "");
		private static TraceSwitch tinfo = new TraceSwitch ("trace-info", "");
		private static TraceSwitch tverbose = new TraceSwitch ("trace-verbose", "");
		private static TraceSwitch tdefault = new TraceSwitch ("no-value", "");
		private static TraceSwitch tsv = new TraceSwitch ("string-value", "");
		private static TraceSwitch tnegative = new TraceSwitch ("trace-negative", "");

		private static TestNewSwitch tns = new TestNewSwitch ("custom-switch", "");

		[Test]
		public void BooleanSwitches ()
		{
			Assert ("#BS:T:1", bon.Enabled);
			Assert ("#BS:T:2", bon2.Enabled);
			Assert ("#BS:T:3", bon3.Enabled);
			Assert ("#BS:F:1", !boff.Enabled);
			Assert ("#BS:F:2", !boff2.Enabled);
		}

		[Test]
		public void TraceSwitches ()
		{
			// The levels 0..4:
			CheckTraceSwitch (toff,      false, false, false, false);
			CheckTraceSwitch (terror,    true,  false, false, false);
			CheckTraceSwitch (twarning,  true,  true,  false, false);
			CheckTraceSwitch (tinfo,     true,  true,  true,  false);
			CheckTraceSwitch (tverbose,  true,  true,  true,  true);

			// Default value is 0
			CheckTraceSwitch (tdefault,  false, false, false, false);

			// string value can't be converted to int, so default is 0
			CheckTraceSwitch (tsv,       false, false, false, false);

			// negative number is < 0, so all off
			CheckTraceSwitch (tnegative, false, false, false, false);
		}

		private void CheckTraceSwitch (TraceSwitch ts, bool te, bool tw, bool ti, bool tv)
		{
			string desc = string.Format ("#TS:{0}", ts.DisplayName);
			AssertEquals (desc + ":TraceError",   te, ts.TraceError);
			AssertEquals (desc + ":TraceWarning", tw, ts.TraceWarning);
			AssertEquals (desc + ":TraceInfo",    ti, ts.TraceInfo);
			AssertEquals (desc + ":TraceVerbose", tv, ts.TraceVerbose);
		}

		[Test]
#if NET_2_0
		[Ignore ("this test depends on 1.x configuration type")]
#endif
		public void NewSwitch ()
		{
			AssertEquals ("#NS:Value", "42", tns.Value);
			Assert ("#NS:Validate", tns.Validate());
		}
	}
}

