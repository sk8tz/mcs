// Int16Test.cs - NUnit Test Cases for the System.Int16 struct
//
// Mario Martinez (mariom925@home.om)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.Globalization;

public class Int16Test : TestCase
{
	private const Int16 MyInt16_1 = -42;
	private const Int16 MyInt16_2 = -32768;
	private const Int16 MyInt16_3 = 32767;
	private const string MyString1 = "-42";
	private const string MyString2 = "-32768";
	private const string MyString3 = "32767";
	private string[] Formats1 = {"c", "d", "e", "f", "g", "n", "p", "x" };
	private string[] Formats2 = {"c5", "d5", "e5", "f5", "g5", "n5", "p5", "x5" };
	private string[] Results1 = {"($32,768.00)", "-32768", "-3.276800e+004", "-32768.00",
	                                  "-32768", "-32,768.00", "-3,276,800.00 %", "8000"};
	private string[] Results2 = {"$32,767.00000", "32767", "3.27670e+004", "32767.00000",
	                                  "32767", "32,767.00000", "3,276,700.00000 %", "07fff"};
	private string[] ResultsNfi1 = {"($32,768.00)", "-32768", "-3.276800e+004", "-32768.00",
	                                  "-32768", "(32,768.00)", "-3,276,800.00 %", "8000"};
	private string[] ResultsNfi2 = {"$32,767.00000", "32767", "3.27670e+004", "32767.00000",
	                                  "32767", "32,767.00000", "3,276,700.00000 %", "07fff"};
	private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;
	
	public Int16Test(string name) : base(name) {}

	protected override void SetUp() 
	{
	}

	public static ITest Suite {
		get { 
			return new TestSuite(typeof(Int16Test)); 
		}
	}
    
	public void TestMinMax()
	{
		
		AssertEquals(Int16.MinValue, MyInt16_2);
		AssertEquals(Int16.MaxValue, MyInt16_3);
	}
	
	public void TestCompareTo()
	{
		Assert(MyInt16_3.CompareTo(MyInt16_2) > 0);
		Assert(MyInt16_2.CompareTo(MyInt16_2) == 0);
		Assert(MyInt16_1.CompareTo((Int16)(-42)) == 0);
		Assert(MyInt16_2.CompareTo(MyInt16_3) < 0);
		try {
			MyInt16_2.CompareTo(100);
			Fail("Should raise a System.ArgumentException");
		}
		catch (Exception e) {
			Assert(typeof(System.ArgumentException) == e.GetType());
		}
	}

	public void TestEquals()
	{
		Assert(MyInt16_1.Equals(MyInt16_1));
		Assert(MyInt16_1.Equals((Int16)(-42)));
		Assert(MyInt16_1.Equals((SByte)(-42)) == false);
		Assert(MyInt16_1.Equals(MyInt16_2) == false);
	}
	
	public void TestGetHashCode()
	{
		try {
			MyInt16_1.GetHashCode();
			MyInt16_2.GetHashCode();
			MyInt16_3.GetHashCode();
		}
		catch {
			Fail("GetHashCode should not raise an exception here");
		}
	}
	
	public void TestParse()
	{
		//test Parse(string s)
		Assert(MyInt16_1 == Int16.Parse(MyString1));
		Assert(MyInt16_2 == Int16.Parse(MyString2));
		Assert(MyInt16_3 == Int16.Parse(MyString3));
		try {
			Int16.Parse(null);
			Fail("Should raise a System.ArgumentNullException");
		}
		catch (Exception e) {
			Assert(typeof(System.ArgumentNullException) == e.GetType());
		}
		try {
			Int16.Parse("not-a-number");
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(System.FormatException) == e.GetType());
		}
		try {
			int OverInt = Int16.MaxValue + 1;
			Int16.Parse(OverInt.ToString());
			Fail("Should raise a System.OverflowException");
		}
		catch (Exception e) {
			Assert(typeof(System.OverflowException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style)
		Assert(42 == Int16.Parse(" $42 ", NumberStyles.Currency));
		try {
			Int16.Parse("$42", NumberStyles.Integer);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(System.FormatException) == e.GetType());
		}
		//test Parse(string s, IFormatProvider provider)
		Assert(-42 == Int16.Parse(" -42 ", Nfi));
		try {
			Int16.Parse("%42", Nfi);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(System.FormatException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style, IFormatProvider provider)
		Assert(16 == Int16.Parse(" 10 ", NumberStyles.HexNumber, Nfi));
		try {
			Int16.Parse("$42", NumberStyles.Integer, Nfi);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(System.FormatException) == e.GetType());
		}
	}
	
	public void TestToString()
	{
		//test ToString()
		Assert(String.Compare(MyString1, MyInt16_1.ToString()) == 0);
		Assert(String.Compare(MyString2, MyInt16_2.ToString()) == 0);
		Assert(String.Compare(MyString3, MyInt16_3.ToString()) == 0);
		//test ToString(string format)
		for (int i=0; i < Formats1.Length; i++) {
			Assert(String.Compare(Results1[i], MyInt16_2.ToString(Formats1[i])) == 0);
			Assert(String.Compare(Results2[i], MyInt16_3.ToString(Formats2[i])) == 0);
		}
		//test ToString(string format, IFormatProvider provider);
		for (int i=0; i < Formats1.Length; i++) {
			Assert(String.Compare(ResultsNfi1[i], MyInt16_2.ToString(Formats1[i], Nfi)) == 0);
			Assert(String.Compare(ResultsNfi2[i], MyInt16_3.ToString(Formats2[i], Nfi)) == 0);
		}
		try {
			MyInt16_1.ToString("z");
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(System.FormatException) == e.GetType());
		}
	}
}

