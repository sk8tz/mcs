//
// TimeSpanTest.cs - NUnit Test Cases for the System.TimeSpan struct
//
// Authors:
//	Duco Fijma (duco@lorentz.xs4all.nl)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2001 Duco Fijma
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using NUnit.Framework;
using System;

namespace MonoTests.System
{

[TestFixture]
public class TimeSpanTest : Assertion {

	public void TestCtors ()
	{
		TimeSpan t1 = new TimeSpan (1234567890);

		AssertEquals ("A1", "00:02:03.4567890", t1.ToString ());
		t1 = new TimeSpan (1,2,3);
		AssertEquals ("A2", "01:02:03", t1.ToString ());
		t1 = new TimeSpan (1,2,3,4);
		AssertEquals ("A3", "1.02:03:04", t1.ToString ());
		t1 = new TimeSpan (1,2,3,4,5);
		AssertEquals ("A4", "1.02:03:04.0050000", t1.ToString ());
		t1 = new TimeSpan (-1,2,-3,4,-5);
		AssertEquals ("A5", "-22:02:56.0050000", t1.ToString ());
		t1 = new TimeSpan (0,25,0,0,0);
		AssertEquals ("A6", "1.01:00:00", t1.ToString ());
        }

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void DaysOverflow () 
	{
		int days = (int) (Int64.MaxValue / TimeSpan.TicksPerDay) + 1;
		TimeSpan ts = new TimeSpan (days, 0, 0, 0, 0);
	}

	[Test]
	public void TemporaryOverflow () 
	{
		// calculating part of this results in overflow (days)
		// but the negative hours, minutes, seconds & ms correct this
		int days = (int) (Int64.MaxValue / TimeSpan.TicksPerDay) + 1;
		TimeSpan ts = new TimeSpan (days, Int32.MinValue, Int32.MinValue, Int32.MinValue, Int32.MinValue);
		AssertEquals ("Ticks", 9201876488683520000, ts.Ticks);
	}

	[Test]
	public void NoOverflowInHoursMinsSecondsMS () 
	{
		TimeSpan ts = new TimeSpan (0, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue);
		AssertEquals ("Ticks", 21496274706470000, ts.Ticks);
	}

	[Test]
	public void NegativeTimeSpan () 
	{
		TimeSpan ts = new TimeSpan (-23, -59, -59);
		AssertEquals ("Hours", -23, ts.Hours);
		AssertEquals ("Minutes", -59, ts.Minutes);
		AssertEquals ("Seconds", -59, ts.Seconds);
		AssertEquals ("Ticks", -863990000000, ts.Ticks);
	}

	public void TestProperties ()
	{
		TimeSpan t1 = new TimeSpan (1,2,3,4,5);
		TimeSpan t2 = -t1;

		AssertEquals ("A1", 1, t1.Days);
		AssertEquals ("A2", 2, t1.Hours);
		AssertEquals ("A3", 3, t1.Minutes);
		AssertEquals ("A4", 4, t1.Seconds);
		AssertEquals ("A5", 5, t1.Milliseconds);
		AssertEquals ("A6", -1, t2.Days);
		AssertEquals ("A7", -2, t2.Hours);
		AssertEquals ("A8", -3, t2.Minutes);
		AssertEquals ("A9", -4, t2.Seconds);
		AssertEquals ("A10", -5, t2.Milliseconds);
	}

	public void TestAdd ()
	{
		TimeSpan t1 = new TimeSpan (2,3,4,5,6);
		TimeSpan t2 = new TimeSpan (1,2,3,4,5);
		TimeSpan t3 = t1 + t2;
		TimeSpan t4 = t1.Add (t2);
		TimeSpan t5;
		bool exception;

		AssertEquals ("A1", 3, t3.Days);
		AssertEquals ("A2", 5, t3.Hours);
		AssertEquals ("A3", 7, t3.Minutes);
		AssertEquals ("A4", 9, t3.Seconds);
		AssertEquals ("A5", 11, t3.Milliseconds);
		AssertEquals ("A6", "3.05:07:09.0110000", t4.ToString ());
		try
		{
			t5 = TimeSpan.MaxValue + new TimeSpan (1);			
			exception = false;
		}
		catch (OverflowException)
		{
			exception = true;
		}
		Assert ("A7", exception);
	}

	public void TestCompare ()
	{
		TimeSpan t1 = new TimeSpan (-1);
		TimeSpan t2 = new TimeSpan (1);
		int res;
		bool exception;

		AssertEquals ("A1", -1, TimeSpan.Compare (t1, t2));
		AssertEquals ("A2", 1, TimeSpan.Compare (t2, t1));
		AssertEquals ("A3", 0, TimeSpan.Compare (t2, t2));
		AssertEquals ("A4", -1, TimeSpan.Compare (TimeSpan.MinValue, TimeSpan.MaxValue));
		AssertEquals ("A5", -1, t1.CompareTo (t2));
		AssertEquals ("A6", 1, t2.CompareTo (t1));
		AssertEquals ("A7", 0, t2.CompareTo (t2));
		AssertEquals ("A8", -1, TimeSpan.Compare (TimeSpan.MinValue, TimeSpan.MaxValue));

		AssertEquals ("A9", 1, TimeSpan.Zero.CompareTo (null));
		
		try
		{
			res = TimeSpan.Zero.CompareTo("");
			exception = false;	
		}
		catch (ArgumentException)
		{
			exception = true;
		}
		Assert ("A10", exception);

		AssertEquals ("A11", false, t1 == t2);
		AssertEquals ("A12", false, t1 > t2);
		AssertEquals ("A13", false, t1 >= t2);
		AssertEquals ("A14", true, t1 != t2);
		AssertEquals ("A15", true, t1 < t2);
		AssertEquals ("A16", true, t1 <= t2);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void NoNegateMinValue() {
		TimeSpan t1 = TimeSpan.MinValue.Negate ();
	}

	public void TestNegateAndDuration ()
	{
		TimeSpan t1;
		bool exception;

		AssertEquals ("A1", "-00:00:00.0012345", new TimeSpan (12345).Negate ().ToString ());
		AssertEquals ("A2", "00:00:00.0012345", new TimeSpan (-12345).Duration ().ToString ());
			
		try
		{
			t1 = TimeSpan.MinValue.Duration ();
			exception = false;
		}
		catch (OverflowException) {
			exception = true;
		}
		Assert ("A4", exception);

		AssertEquals ("A5", "-00:00:00.0000077", (-(new TimeSpan (77))).ToString ());
		AssertEquals("A6", "00:00:00.0000077", (+(new TimeSpan(77))).ToString());
	}

	public void TestEquals ()
	{
		TimeSpan t1 = new TimeSpan (1);
		TimeSpan t2 = new TimeSpan (2);
		string s = "justastring";

		AssertEquals ("A1", true, t1.Equals (t1));
		AssertEquals ("A2", false, t1.Equals (t2));
		AssertEquals ("A3", false, t1.Equals (s));
		AssertEquals ("A4", false, t1.Equals (null));
		AssertEquals ("A5", true, TimeSpan.Equals (t1, t1));
		AssertEquals ("A6", false, TimeSpan.Equals (t1, t2));
		AssertEquals ("A7", false, TimeSpan.Equals (t1, null));
		AssertEquals ("A8", false, TimeSpan.Equals (t1, s));
		AssertEquals ("A9", false, TimeSpan.Equals (s, t2));
		AssertEquals ("A10", true, TimeSpan.Equals (null,null));
	}

	public void TestFromXXXX ()
	{
		AssertEquals ("A1", "12.08:16:48", TimeSpan.FromDays (12.345).ToString ());
		AssertEquals ("A2", "12:20:42", TimeSpan.FromHours (12.345).ToString ());
		AssertEquals ("A3", "00:12:20.7000000", TimeSpan.FromMinutes (12.345).ToString ());
		AssertEquals ("A4", "00:00:12.3450000", TimeSpan.FromSeconds (12.345).ToString ());
		AssertEquals ("A5", "00:00:00.0120000", TimeSpan.FromMilliseconds (12.345).ToString ());
		AssertEquals ("A6", "00:00:00.0012345", TimeSpan.FromTicks (12345).ToString ());
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromDays_MinValue ()
	{
		TimeSpan.FromDays (Double.MinValue);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromDays_MaxValue ()
	{
		TimeSpan.FromDays (Double.MaxValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void FromDays_NaN ()
	{
		TimeSpan.FromDays (Double.NaN);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromDays_PositiveInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MaxValue
		AssertEquals (TimeSpan.MaxValue, TimeSpan.FromDays (Double.PositiveInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromDays_NegativeInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MinValue
		AssertEquals (TimeSpan.MinValue, TimeSpan.FromDays (Double.NegativeInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromHours_MinValue ()
	{
		TimeSpan.FromHours (Double.MinValue);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromHours_MaxValue ()
	{
		TimeSpan.FromHours (Double.MaxValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void FromHours_NaN ()
	{
		TimeSpan.FromHours (Double.NaN);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromHours_PositiveInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MaxValue
		AssertEquals (TimeSpan.MaxValue, TimeSpan.FromHours (Double.PositiveInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromHours_NegativeInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MinValue
		AssertEquals (TimeSpan.MinValue, TimeSpan.FromHours (Double.NegativeInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromMilliseconds_MinValue ()
	{
		TimeSpan.FromMilliseconds (Double.MinValue);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromMilliseconds_MaxValue ()
	{
		TimeSpan.FromMilliseconds (Double.MaxValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void FromMilliseconds_NaN ()
	{
		TimeSpan.FromMilliseconds (Double.NaN);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromMilliseconds_PositiveInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MaxValue
		AssertEquals (TimeSpan.MaxValue, TimeSpan.FromMilliseconds (Double.PositiveInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromMilliseconds_NegativeInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MinValue
		AssertEquals (TimeSpan.MinValue, TimeSpan.FromMilliseconds (Double.NegativeInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromMinutes_MinValue ()
	{
		TimeSpan.FromMinutes (Double.MinValue);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromMinutes_MaxValue ()
	{
		TimeSpan.FromMinutes (Double.MaxValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void FromMinutes_NaN ()
	{
		TimeSpan.FromMinutes (Double.NaN);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromMinutes_PositiveInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MaxValue
		AssertEquals (TimeSpan.MaxValue, TimeSpan.FromMinutes (Double.PositiveInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromMinutes_NegativeInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MinValue
		AssertEquals (TimeSpan.MinValue, TimeSpan.FromMinutes (Double.NegativeInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromSeconds_MinValue ()
	{
		TimeSpan.FromSeconds (Double.MinValue);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromSeconds_MaxValue ()
	{
		TimeSpan.FromSeconds (Double.MaxValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void FromSeconds_NaN ()
	{
		TimeSpan.FromSeconds (Double.NaN);
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromSeconds_PositiveInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MaxValue
		AssertEquals (TimeSpan.MaxValue, TimeSpan.FromSeconds (Double.PositiveInfinity));
	}

	[Test]
	[ExpectedException (typeof (OverflowException))]
	public void FromSeconds_NegativeInfinity ()
	{
		// LAMESPEC: Document to return TimeSpan.MinValue
		AssertEquals (TimeSpan.MinValue, TimeSpan.FromSeconds (Double.NegativeInfinity));
	}

	public void TestGetHashCode ()
	{
		AssertEquals ("A1", 77, new TimeSpan (77).GetHashCode ());
	}

	private void ParseHelper (string s, bool expectFormat, bool expectOverflow, string expect)
	{
		bool formatException = false;
		bool overflowException = false;
		string result = "junk ";

		try {
			result =  TimeSpan.Parse (s).ToString ();
		}
		catch (OverflowException) {
			overflowException = true;
		}
		catch (FormatException) {
			formatException = true;
		}
		AssertEquals ("A1", expectFormat, formatException);
		AssertEquals ("A2", expectOverflow, overflowException);

		if (!expectOverflow && !expectFormat) {
			AssertEquals ("A3", expect, result);
		}
	}

	public void TestParse ()
	{
		ParseHelper (" 13:45:15 ",false, false, "13:45:15");
		ParseHelper (" -1:2:3 ", false, false, "-01:02:03");

		ParseHelper (" 25:0:0 ",false, true, "dontcare");
		ParseHelper ("aaa", true, false, "dontcare");

		ParseHelper ("-21.23:59:59.9999999", false, false, "-21.23:59:59.9999999");

		ParseHelper ("100000000000000.1:1:1", false, true, "dontcare");
		ParseHelper ("24:60:60", false, true, "dontcare");
		ParseHelper ("0001:0002:0003.12     ", false, false, "01:02:03.1200000");

		ParseHelper (" 1:2:3:12345678 ", true, false, "dontcare"); 
	}

	// LAMESPEC: timespan in documentation is wrong - hh:mm:ss isn't mandatory
	[Test]
	public void Parse_Days_WithoutColon () 
	{
		TimeSpan ts = TimeSpan.Parse ("1");
		AssertEquals ("Days", 1, ts.Days);
	}

	public void TestSubstract ()
	{
		TimeSpan t1 = new TimeSpan (2,3,4,5,6);
		TimeSpan t2 = new TimeSpan (1,2,3,4,5);
		TimeSpan t3 = t1 - t2;
		TimeSpan t4 = t1.Subtract (t2);
		TimeSpan t5;
		bool exception;

		AssertEquals ("A1", "1.01:01:01.0010000", t3.ToString ());
		AssertEquals ("A2", "1.01:01:01.0010000", t4.ToString ());
		try {
			t5 = TimeSpan.MinValue - new TimeSpan (1);
			exception = false;
		}
		catch (OverflowException) {
			exception = true;
		}
		Assert ("A3", exception);
	}

	public void TestToString () 
	{
		TimeSpan t1 = new TimeSpan (1,2,3,4,5);
		TimeSpan t2 = -t1;
		
		AssertEquals ("A1", "1.02:03:04.0050000", t1.ToString ());
		AssertEquals ("A2", "-1.02:03:04.0050000", t2.ToString ());
		AssertEquals ("A3", "10675199.02:48:05.4775807", TimeSpan.MaxValue.ToString ());
		AssertEquals ("A4", "-10675199.02:48:05.4775808", TimeSpan.MinValue.ToString ());
	}

}

}
