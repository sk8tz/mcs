// UInt64Test.cs - NUnit Test Cases for the System.UInt64 struct
//
// Mario Martinez (mariom925@home.om)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.Globalization;

namespace MonoTests.System
{

public class UInt64Test : TestCase
{
	private const UInt64 MyUInt64_1 = 42;
	private const UInt64 MyUInt64_2 = 0;
	private const UInt64 MyUInt64_3 = 18446744073709551615;
	private const string MyString1 = "42";
	private const string MyString2 = "0";
	private const string MyString3 = "18446744073709551615";
	private string[] Formats1 = {"c", "d", "e", "f", "g", "n", "p", "x" };
	private string[] Formats2 = {"c5", "d5", "e5", "f5", "g5", "n5", "p5", "x5" };
	private string[] Results1 = {"$0.00", "0", "0.000000e+000", "0.00",
	                                  "0", "0.00", "0.00 %", "0"};
	private string[] Results2 = {"$18,446,744,073,709,551,615.00000", "18446744073709551615", "1.84467e+019", "18446744073709551615.00000",
	                                  "1.8447e+19", "18,446,744,073,709,551,615.00000", "1,844,674,407,370,955,161,500.00000 %", "ffffffffffffffff"};
	private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;
	
	public UInt64Test(string name) : base(name) {}

	protected override void SetUp() 
	{
	}

	public static ITest Suite {
		get { 
			return new TestSuite(typeof(UInt64Test)); 
		}
	}
    
	public void TestMinMax()
	{
		
		AssertEquals(UInt64.MinValue, MyUInt64_2);
		AssertEquals(UInt64.MaxValue, MyUInt64_3);
	}
	
	public void TestCompareTo()
	{
		Assert(MyUInt64_3.CompareTo(MyUInt64_2) > 0);
		Assert(MyUInt64_2.CompareTo(MyUInt64_2) == 0);
		Assert(MyUInt64_1.CompareTo((UInt64)(42)) == 0);
		Assert(MyUInt64_2.CompareTo(MyUInt64_3) < 0);
		try {
			MyUInt64_2.CompareTo((Int16)100);
			Fail("Should raise a System.ArgumentException");
		}
		catch (Exception e) {
			Assert(typeof(ArgumentException) == e.GetType());
		}
	}

	public void TestEquals()
	{
		Assert(MyUInt64_1.Equals(MyUInt64_1));
		Assert(MyUInt64_1.Equals((UInt64)(42)));
		Assert(MyUInt64_1.Equals((SByte)(42)) == false);
		Assert(MyUInt64_1.Equals(MyUInt64_2) == false);
	}
	
	public void TestGetHashCode()
	{
		try {
			MyUInt64_1.GetHashCode();
			MyUInt64_2.GetHashCode();
			MyUInt64_3.GetHashCode();
		}
		catch {
			Fail("GetHashCode should not raise an exception here");
		}
	}
	
	public void TestParse()
	{
		//test Parse(string s)
		Assert(MyUInt64_1 == UInt64.Parse(MyString1));
		Assert(MyUInt64_2 == UInt64.Parse(MyString2));
		Assert(MyUInt64_3 == UInt64.Parse(MyString3));
		try {
			UInt64.Parse(null);
			Fail("Should raise a ArgumentNullException");
		}
		catch (Exception e) {
			Assert(typeof(ArgumentNullException) == e.GetType());
		}
		try {
			UInt64.Parse("not-a-number");
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style)
		try {
			double OverInt = (double)UInt64.MaxValue + 1;
			UInt64.Parse(OverInt.ToString(), NumberStyles.Float);
			Fail("Should raise a OverflowException");
		}
		catch (Exception e) {
			Assert(typeof(OverflowException) == e.GetType());
		}
		try {
			double OverInt = (double)UInt64.MaxValue + 1;
			UInt64.Parse(OverInt.ToString(), NumberStyles.Integer);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}
		Assert(42 == UInt64.Parse(" $42 ", NumberStyles.Currency));
		try {
			UInt64.Parse("$42", NumberStyles.Integer);
			Fail("Should raise a FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, IFormatProvider provider)
		Assert(42 == UInt64.Parse(" 42 ", Nfi));
		try {
			UInt64.Parse("%42", Nfi);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}
		//test Parse(string s, NumberStyles style, IFormatProvider provider)
		Assert(16 == UInt64.Parse(" 10 ", NumberStyles.HexNumber, Nfi));
		try {
			UInt64.Parse("$42", NumberStyles.Integer, Nfi);
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}
	}
	
	public void TestToString()
	{
		//test ToString()
		Assert(String.Compare(MyString1, MyUInt64_1.ToString()) == 0);
		Assert(String.Compare(MyString2, MyUInt64_2.ToString()) == 0);
		Assert(String.Compare(MyString3, MyUInt64_3.ToString()) == 0);
		//test ToString(string format)
		for (int i=0; i < Formats1.Length; i++) {
			Assert(String.Compare(Results1[i], MyUInt64_2.ToString(Formats1[i])) == 0);
			Assert(String.Compare(Results2[i], MyUInt64_3.ToString(Formats2[i])) == 0);
		}
		//test ToString(string format, IFormatProvider provider);
		for (int i=0; i < Formats1.Length; i++) {
			Assert(String.Compare(Results1[i], MyUInt64_2.ToString(Formats1[i], Nfi)) == 0);
			Assert(String.Compare(Results2[i], MyUInt64_3.ToString(Formats2[i], Nfi)) == 0);
		}
		try {
			MyUInt64_1.ToString("z");
			Fail("Should raise a System.FormatException");
		}
		catch (Exception e) {
			Assert(typeof(FormatException) == e.GetType());
		}
	}
}

}
