// MathTest.cs
//
// Jon Guymon (guymon@slackworks.com)
// Pedro Mart�nez Juli� (yoros@wanadoo.es)
//
// (C) 2002 Jon Guymon
// Copyright (C) 2003 Pedro Mart�nez Juli� <yoros@wanadoo.es>
// 

using System;
using NUnit.Framework;

namespace MonoTests.System 
{

public class MathTest : TestCase {

	public MathTest() {}

	protected override void SetUp() {}
	protected override void TearDown() {}

	static double x = 0.1234;
	static double y = 12.345;

	public void TestDecimalAbs() {
		decimal a = -9.0M;

		Assert(9.0M == Math.Abs(a));
		Assert(Decimal.MaxValue == Math.Abs(Decimal.MaxValue));
		Assert(Decimal.MaxValue == Math.Abs(Decimal.MinValue));
		Assert(Decimal.Zero == Math.Abs(Decimal.Zero));
		Assert(Decimal.One == Math.Abs(Decimal.One));
		Assert(Decimal.One == Math.Abs(Decimal.MinusOne));
	}


	public void TestDoubleAbs() {
		double a = -9.0D;
		Assert(9.0D == Math.Abs(a));
		Assert(0.0D == Math.Abs(0.0D));
		Assert(Double.MaxValue == Math.Abs(Double.MaxValue));
		Assert(Double.MaxValue == Math.Abs(Double.MinValue));
		Assert(Double.IsPositiveInfinity(Math.Abs(Double.PositiveInfinity)));
		Assert(Double.IsPositiveInfinity(Math.Abs(Double.NegativeInfinity)));
		Assert(Double.IsNaN(Math.Abs(Double.NaN)));
	}

	public void TestFloatAbs() {
		float a = -9.0F;

		Assert(9.0F == Math.Abs(a));
		Assert(0.0F == Math.Abs(0.0F));
		Assert(Single.MaxValue == Math.Abs(Single.MaxValue));
		Assert(Single.MaxValue == Math.Abs(Single.MinValue));
		Assert(Single.PositiveInfinity == Math.Abs(Single.PositiveInfinity));
		Assert(Single.PositiveInfinity == Math.Abs(Single.NegativeInfinity));
		Assert(Single.IsNaN(Math.Abs(Single.NaN)));
	}

	public void TestLongAbs() {
		long a = -9L;
		long b = Int64.MinValue;

		Assert(9L == Math.Abs(a));
		try {
			Math.Abs(b);
			Fail("Should raise System.OverflowException");
		} catch(Exception e) {
			Assert(typeof(OverflowException) == e.GetType());
		}
		Assert(Int64.MaxValue == Math.Abs(Int64.MaxValue));
	}

	public void TestIntAbs() {
		int a = -9;
		int b = Int32.MinValue;

		Assert(9 == Math.Abs(a));
		try {
			Math.Abs(b);
			Fail("Should raise System.OverflowException");
		} catch(Exception e) {
			Assert(typeof(OverflowException) == e.GetType());
		}
		Assert(Int32.MaxValue == Math.Abs(Int32.MaxValue));
	}

	public void TestSbyteAbs() {
		sbyte a = -9;
		sbyte b = SByte.MinValue;

		Assert(9 == Math.Abs(a));
		try {
			Math.Abs(b);
			Fail("Should raise System.OverflowException");
		} catch(Exception e) {
			Assert(typeof(OverflowException) == e.GetType());
		}
		Assert(SByte.MaxValue == Math.Abs(SByte.MaxValue));
	}

	public void TestShortAbs() {
		short a = -9;
		short b = Int16.MinValue;

		Assert(9 == Math.Abs(a));
		try {
			Math.Abs(b);
			Fail("Should raise System.OverflowException");
		} catch(Exception e) {
			Assert(typeof(OverflowException) == e.GetType());
		}
		Assert(Int16.MaxValue == Math.Abs(Int16.MaxValue));
	}

	public void TestAcos() {
		double a = Math.Acos(x);
		double b = 1.4470809809523457;

		Assert(a.ToString("G99") + " != " + b.ToString("G99"), 
		       (Math.Abs(a - b) <= double.Epsilon));
		Assert(double.IsNaN(Math.Acos(-1.01D)));
		Assert(double.IsNaN(Math.Acos(1.01D)));
		Assert(double.IsNaN(Math.Acos(Double.MinValue)));
		Assert(double.IsNaN(Math.Acos(Double.MaxValue)));
		Assert(double.IsNaN(Math.Acos(Double.NegativeInfinity)));
		Assert(double.IsNaN(Math.Acos(Double.PositiveInfinity)));
	}

	public void TestAsin() {
		double a = Math.Asin(x);
		double b = 0.12371534584255098;

		Assert(a.ToString("G99") + " != " + b.ToString("G99"), 
		       (Math.Abs(a - b) <= double.Epsilon));
		Assert(double.IsNaN(Math.Asin(-1.01D)));
		Assert(double.IsNaN(Math.Asin(1.01D)));
		Assert(double.IsNaN(Math.Asin(Double.MinValue)));
		Assert(double.IsNaN(Math.Asin(Double.MaxValue)));
		Assert(double.IsNaN(Math.Asin(Double.NegativeInfinity)));
		Assert(double.IsNaN(Math.Asin(Double.PositiveInfinity)));
	}

	public void TestAtan() {
		double a = Math.Atan(x);
		double b = 0.12277930094473837;
		double c = 1.5707963267948966;
		double d = -1.5707963267948966;

		Assert(a.ToString("G99") + " != " + b.ToString("G99"), 
		       (Math.Abs(a - b) <= double.Epsilon));
		Assert("should return NaN", 
		       double.IsNaN(Math.Atan(double.NaN)));
		Assert(Math.Atan(double.PositiveInfinity).ToString("G99")+" != "+c.ToString("G99"), 
		       Math.Abs((double)Math.Atan(double.PositiveInfinity) - c) <= double.Epsilon);
		Assert(Math.Atan(double.NegativeInfinity).ToString("G99")+" != "+d.ToString("G99"),
		       Math.Abs((double)Math.Atan(double.NegativeInfinity) - d) <= double.Epsilon);
	}

	public void TestAtan2() {
		double a = Math.Atan2(x, y);
		double b = 0.0099956168687207747;

		Assert(a.ToString("G99") + " != " + b.ToString("G99"), 
		       (Math.Abs(a - b) <= double.Epsilon));
		Assert(double.IsNaN(Math.Acos(-2D)));
		Assert(double.IsNaN(Math.Acos(2D)));
	}

	// The following test is for methods that are in ECMA but they are
	// not implemented in MS.NET. I leave them commented.
	/*
	public void TestBigMul () {
		int a = int.MaxValue;
		int b = int.MaxValue;

		Assert(((long)a * (long)b) == Math.BigMul(a,b));
	}
	*/

	public void TestCos() {
		double a = Math.Cos(x);
		double b = 0.99239587670489104;

		Assert(a.ToString("G99") + " != " + b.ToString("G99"), 
		       (Math.Abs(a - b) <= double.Epsilon));
		Assert(double.IsNaN(Math.Cos(Double.NaN)));
		Assert(double.IsNaN(Math.Cos(Double.NegativeInfinity)));
		Assert(double.IsNaN(Math.Cos(Double.PositiveInfinity)));
	}

	public void TestCosh() {
		double a = Math.Cosh(x);
		double b = 1.0076234465130722;

		Assert(a.ToString("G99") + " != " + b.ToString("G99"), 
		       (Math.Abs(a - b) <= double.Epsilon));
		Assert(Math.Cosh(double.NegativeInfinity) == double.PositiveInfinity);
		Assert(Math.Cosh(double.PositiveInfinity) == double.PositiveInfinity);
		Assert(double.IsNaN(Math.Cosh(double.NaN)));
	}

	// The following test is for methods that are in ECMA but they are
	// not implemented in MS.NET. I leave them commented.
	/*
	public void TestIntDivRem () {
		int a = 5;
		int b = 2;
		int div = 0, rem = 0;

		div = Math.DivRem(a, b, out rem);

		Assert(rem == 1);
		Assert(div == 2);
	}

	public void TestLongDivRem () {
		long a = 5;
		long b = 2;
		long div = 0, rem = 0;

		div = Math.DivRem(a, b, out rem);

		Assert(rem == 1);
		Assert(div == 2);
	}
	*/

	public void TestSin() {
		double a = Math.Sin(x);
		double b = 0.12308705821137626;

		Assert(a.ToString("G99") + " != " + b.ToString("G99"), 
		       (Math.Abs(a - b) <= double.Epsilon));
		Assert(double.IsNaN(Math.Sin(Double.NaN)));
		Assert(double.IsNaN(Math.Sin(Double.NegativeInfinity)));
		Assert(double.IsNaN(Math.Sin(Double.PositiveInfinity)));
	}

	public void TestSinh() {
		double a = Math.Sinh(x);
		double b = 0.12371341868561381;

		Assert(a.ToString("G99") + " != " + b.ToString("G99"), 
		       (Math.Abs(a - b) <= double.Epsilon));
		Assert(double.IsNaN(Math.Sinh(Double.NaN)));
		Assert(double.IsNegativeInfinity(Math.Sinh(Double.NegativeInfinity)));
		Assert(double.IsPositiveInfinity(Math.Sinh(Double.PositiveInfinity)));
	}

	public void TestTan() {
		double a = Math.Tan(x);
		double b = 0.12403019913793806;

		Assert(a.ToString("G99") + " != " + b.ToString("G99"), 
		       (Math.Abs(a - b) <= double.Epsilon));
		Assert(Double.IsNaN(Math.Tan(Double.NaN)));
		Assert(Double.IsNaN(Math.Tan(Double.PositiveInfinity)));
		Assert(Double.IsNaN(Math.Tan(Double.NegativeInfinity)));
	}

	public void TestTanh() {
		double a = Math.Tanh(x);
		double b = 0.12277743150353424;

		Assert(a.ToString("G99") + " != " + b.ToString("G99"), 
		       (Math.Abs(a - b) <= double.Epsilon));
		Assert("Tanh(NaN) should be NaN",
			Double.IsNaN(Math.Tanh(Double.NaN)));
		Assert("Tanh(+Infinity) should be 1",
			1 == Math.Tanh(Double.PositiveInfinity));
		Assert("Tanh(-Infinity) should be -1",
			-1 == Math.Tanh(Double.NegativeInfinity));
	}

	public void TestSqrt() {
		double a = Math.Sqrt(x);
		double b = 0.35128336140500593;

		Assert(a.ToString("G99") + " != " + b.ToString("G99"), 
		       (Math.Abs(a - b) <= double.Epsilon));
		Assert(Double.IsNaN(Math.Sqrt(Double.NaN)));
		Assert(Double.IsPositiveInfinity(Math.Sqrt(Double.PositiveInfinity)));
		Assert(Double.IsNaN(Math.Sqrt(Double.NegativeInfinity)));
	}

	public void TestExp() {
		double a = Math.Exp(x);
		double b = 1.1313368651986859;

		Assert(a.ToString("G99") + " != " + b.ToString("G99"), 
		       (Math.Abs(a - b) <= double.Epsilon));
		Assert(double.IsNaN(Math.Exp(double.NaN)));
		Assert(Math.Exp(double.NegativeInfinity) == 0);
		Assert(Math.Exp(double.PositiveInfinity) == double.PositiveInfinity);
	}

	public void TestCeiling() {
		int iTest = 1;
		try {
			double a = Math.Ceiling(1.5);
			double b = 2;
	
			iTest++;
			Assert(a.ToString("G99") + " != " + b.ToString("G99"), 
			       (Math.Abs(a - b) <= double.Epsilon));
			iTest++;
			Assert(Math.Ceiling(double.NegativeInfinity) == double.NegativeInfinity);
			iTest++;
			Assert(Math.Ceiling(double.PositiveInfinity) == double.PositiveInfinity);
			iTest++;
			Assert(double.IsNaN(Math.Ceiling(double.NaN)));

			iTest++;
			Assert(Double.MaxValue == Math.Ceiling(Double.MaxValue));

			iTest++;
			Assert(Double.MinValue == Math.Ceiling(Double.MinValue));
		} catch (Exception e) {
			Fail("Unexpected Exception at iTest=" + iTest + ": " + e);
		}
	}

	public void TestFloor() {
		try {
			double a = Math.Floor(1.5);
			double b = 1;

			Assert(a.ToString("G99") + " != " + b.ToString("G99"), 
			       (Math.Abs(a - b) <= double.Epsilon));
			Assert(Math.Floor(double.NegativeInfinity) == double.NegativeInfinity);
			Assert(Math.Floor(double.PositiveInfinity) == double.PositiveInfinity);
			Assert(double.IsNaN(Math.Floor(double.NaN)));

			Assert(Double.MaxValue == Math.Floor(Double.MaxValue));

			Assert(Double.MinValue == Math.Floor(Double.MinValue));
		} catch (Exception e) {
			Fail("Unexpected Exception: " + e.ToString());
		}
	}

	public void TestIEEERemainder() {
		double a = Math.IEEERemainder(y, x);
		double b = 0.0050000000000007816;

		Assert(a.ToString("G99") + " != " + b.ToString("G99"), 
		       (Math.Abs(a - b) <= double.Epsilon));
		Assert(double.IsNaN(Math.IEEERemainder(y, 0)));
	}

	public void TestLog() {
		double a = Math.Log(y);
		double b = 2.513251122797143;

		Assert(a.ToString("G99") + " != " + b.ToString("G99"), 
		       (Math.Abs(a - b) <= double.Epsilon));
		Assert(double.IsNaN(Math.Log(-1)));
		Assert(double.IsNaN(Math.Log(double.NaN)));

		// MS docs say this should be PositiveInfinity
		Assert(Math.Log(0) == double.NegativeInfinity);
		Assert(Math.Log(double.PositiveInfinity) == double.PositiveInfinity);
	}

	public void TestLog2() {
		double a = Math.Log(x, y);
		double b = -0.83251695325303621;

		Assert(a + " != " + b + " because diff is " + Math.Abs(a - b), (Math.Abs(a - b) <= 1e-14));
		Assert(double.IsNaN(Math.Log(-1, y)));
		Assert(double.IsNaN(Math.Log(double.NaN, y)));
		Assert(double.IsNaN(Math.Log(x, double.NaN)));
		Assert(double.IsNaN(Math.Log(double.NegativeInfinity, y)));
		Assert(double.IsNaN(Math.Log(x, double.NegativeInfinity)));
		Assert(double.IsNaN(Math.Log(double.PositiveInfinity, double.PositiveInfinity)));

		// MS docs say this should be PositiveInfinity
		Assert(Math.Log(0, y) == double.NegativeInfinity);
		Assert(Math.Log(double.PositiveInfinity, y) == double.PositiveInfinity);
		Assert(Math.Log(x, double.PositiveInfinity) == 0);
	}

 	public void TestLog10() {
		double a = Math.Log10(x);
		double b = -0.90868484030277719;

		Assert(a.ToString("G99") + " != " + b.ToString("G99"), 
		       (Math.Abs(a - b) <= double.Epsilon));
		Assert(double.IsNaN(Math.Log10(-1)));
		Assert(double.IsNaN(Math.Log10(double.NaN)));

		// MS docs say this should be PositiveInfinity
		Assert(Math.Log10(0) == double.NegativeInfinity);
		Assert(Math.Log10(double.PositiveInfinity) == double.PositiveInfinity);

	}	

 	public void TestPow() {
		int iTest = 1;

		try {
			double a = Math.Pow(y, x);
			double b = 1.363609446060212;

			Assert(a.ToString("G99") + " != " + b.ToString("G99"), (Math.Abs(a - b) <= double.Epsilon));
			iTest++;
			Assert (double.IsNaN(Math.Pow(y, double.NaN)));
			iTest++;
			Assert (double.IsNaN(Math.Pow(double.NaN, x)));
			iTest++;
			Assert ("Math.Pow(double.NegativeInfinity, 1) should be NegativeInfinity", double.IsNegativeInfinity(Math.Pow(double.NegativeInfinity, 1)));
			iTest++;
			Assert ("Math.Pow(double.NegativeInfinity, 2) should be PositiveInfinity", double.IsPositiveInfinity(Math.Pow(double.NegativeInfinity, 2)));

			// MS docs say this should be 0
			iTest++;
			Assert(double.IsNaN(Math.Pow(1, double.NegativeInfinity)));
			iTest++;
			AssertEquals ("Math.Pow(double.PositiveInfinity, double.NegativeInfinity)", (double)0, Math.Pow(double.PositiveInfinity, double.NegativeInfinity));
			iTest++;
			Assert ("Math.Pow(double.PositiveInfinity, 1) should be PositiveInfinity", double.IsPositiveInfinity(Math.Pow(double.PositiveInfinity, 1)));

			// MS docs say this should be PositiveInfinity
			iTest++;
			Assert ("Math.Pow(1, double.PositiveInfinity) should be NaN", double.IsNaN(Math.Pow(1, double.PositiveInfinity)));

			iTest++;
			Assert("Math.Pow(1, NaN) should be NaN",
				Double.IsNaN(Math.Pow(1, Double.NaN)));

			iTest++;
			Assert("Math.Pow(NaN, 0) should be NaN",
				Double.IsNaN(Math.Pow(Double.NaN, 0)));

			iTest++;
			Assert("Math.Pow(-1, MaxValue) should be 1.0",
				1.0 == Math.Pow(-1, Double.MaxValue));

			iTest++;
			Assert("Math.Pow(-1, MinValue) should be 1.0",
				1.0 == Math.Pow(-1, Double.MinValue));

			iTest++;
			Assert("Math.Pow(MinValue, MaxValue) should be +Infinity",
				Double.IsPositiveInfinity(
					Math.Pow(Double.MinValue, Double.MaxValue)));

			iTest++;
			Assert("Math.Pow(MinValue, MinValue) should be 0.0",
				0.0 == Math.Pow(Double.MinValue, Double.MinValue));

			//
			// The following bugs were present because we tried to outsmart the C Pow:
			//
			double infinity = Double.PositiveInfinity;
			Assert ("Math.Pow(0.5, Infinity) == 0.0",
				Math.Pow(0.5, infinity) == 0.0);
			Assert ("Math.Pow(0.5, -Infinity) == Infinity",
				Math.Pow(0.5, -infinity) == infinity);
			Assert ("Math.Pow(2, Infinity) == Infinity",
				Math.Pow(2, infinity) == infinity);
			Assert ("Math.Pow(2, -Infinity) == 0",
				Math.Pow(2, -infinity) == 0.0);
			Assert ("Math.Pow(Infinity, 0) == 1.0",
				Math.Pow(infinity, 0) == 1.0);
			Assert ("Math.Pow(-Infinity, 0) == 1.0",
				Math.Pow(-infinity, 0) == 1.0);
		} catch (Exception e) {
			Fail ("Unexpected exception at iTest=" + iTest + ". e=" + e);
		}
	}	

	public void TestByteMax() {
		byte a = 1;
		byte b = 2;

		Assert(b == Math.Max(a, b));
		Assert(b == Math.Max(b, a));
	}

	public void TestDecimalMax() {
		decimal a = 1.5M;
		decimal b = 2.5M;

		Assert(b == Math.Max(a, b));
		Assert(b == Math.Max(b, a));
	}

	public void TestDoubleMax() {
		double a = 1.5D;
		double b = 2.5D;

		Assert(b == Math.Max(a, b));
		Assert(b == Math.Max(b, a));
		Assert("Max(NaN,NaN) should be NaN",
			Double.IsNaN(Math.Max(Double.NaN, Double.NaN)));
		Assert("Max(NaN,x) should be NaN",
			Double.IsNaN(Math.Max(Double.NaN, a)));
		Assert("Max(x,NaN) should be NaN",
			Double.IsNaN(Math.Max(b, Double.NaN)));
	}

	public void TestFloatMax() {
		float a = 1.5F;
		float b = 2.5F;

		Assert(b == Math.Max(a, b));
		Assert(b == Math.Max(b, a));
		Assert("Max(NaN,NaN) should be NaN",
			Single.IsNaN(Math.Max(Single.NaN, Single.NaN)));
		Assert("Max(NaN,x) should be NaN",
			Single.IsNaN(Math.Max(Single.NaN, a)));
		Assert("Max(x,NaN) should be NaN",
			Single.IsNaN(Math.Max(b, Single.NaN)));
	}

	public void TestIntMax() {
		int a = 1;
		int b = 2;

		Assert(b == Math.Max(a, b));
		Assert(b == Math.Max(b, a));
	}

	public void TestLongMax() {
		long a = 1L;
		long b = 2L;

		Assert(b == Math.Max(a, b));
		Assert(b == Math.Max(b, a));
	}

	public void TestSbyteMax() {
		sbyte a = 1;
		sbyte b = 2;

		Assert(b == Math.Max(a, b));
		Assert(b == Math.Max(b, a));
	}

	public void TestShortMax() {
		short a = 1;
		short b = 2;

		Assert(b == Math.Max(a, b));
		Assert(b == Math.Max(b, a));
	}

	public void TestUintMax() {
		uint a = 1U;
		uint b = 2U;

		Assert(b == Math.Max(a, b));
		Assert(b == Math.Max(b, a));
	}

	public void TestUlongMax() {
		ulong a = 1UL;
		ulong b = 2UL;

		Assert(b == Math.Max(a, b));
		Assert(b == Math.Max(b, a));
	}

	public void TestUshortMax() {
		ushort a = 1;
		ushort b = 2;

		Assert(b == Math.Max(a, b));
		Assert(b == Math.Max(b, a));
	}

	public void TestByteMin() {
		byte a = 1;
		byte b = 2;

		Assert(a == Math.Min(a, b));
		Assert(a == Math.Min(b, a));
	}

	public void TestDecimalMin() {
		decimal a = 1.5M;
		decimal b = 2.5M;

		Assert(a == Math.Min(a, b));
		Assert(a == Math.Min(b, a));
	}

	public void TestDoubleMin() {
		double a = 1.5D;
		double b = 2.5D;

		Assert(a == Math.Min(a, b));
		Assert(a == Math.Min(b, a));
		Assert("Min(NaN,NaN) should be NaN",
			Double.IsNaN(Math.Min(Double.NaN, Double.NaN)));
		Assert("Min(NaN,x) should be NaN",
			Double.IsNaN(Math.Min(Double.NaN, a)));
		Assert("Min(x,NaN) should be NaN",
			Double.IsNaN(Math.Min(b, Double.NaN)));
	}

	public void TestFloatMin() {
		float a = 1.5F;
		float b = 2.5F;

		Assert(a == Math.Min(a, b));
		Assert(a == Math.Min(b, a));
		Assert("Min(NaN,NaN) should be NaN",
			Single.IsNaN(Math.Min(Single.NaN, Single.NaN)));
		Assert("Min(NaN,x) should be NaN",
			Single.IsNaN(Math.Min(Single.NaN, a)));
		Assert("Min(x,NaN) should be NaN",
			Single.IsNaN(Math.Min(b, Single.NaN)));
	}

	public void TestIntMin() {
		int a = 1;
		int b = 2;

		Assert(a == Math.Min(a, b));
		Assert(a == Math.Min(b, a));
	}

	public void TestLongMin() {
		long a = 1L;
		long b = 2L;

		Assert(a == Math.Min(a, b));
		Assert(a == Math.Min(b, a));
	}

	public void TestSbyteMin() {
		sbyte a = 1;
		sbyte b = 2;

		Assert(a == Math.Min(a, b));
		Assert(a == Math.Min(b, a));
	}

	public void TestShortMin() {
		short a = 1;
		short b = 2;

		Assert(a == Math.Min(a, b));
		Assert(a == Math.Min(b, a));
	}

	public void TestUintMin() {
		uint a = 1U;
		uint b = 2U;

		Assert(a == Math.Min(a, b));
		Assert(a == Math.Min(b, a));
	}

	public void TestUlongMin() {
		ulong a = 1UL;
		ulong b = 2UL;

		Assert(a == Math.Min(a, b));
		Assert(a == Math.Min(b, a));
	}

	public void TestUshortMin() {
		ushort a = 1;
		ushort b = 2;

		Assert(a == Math.Min(a, b));
		Assert(a == Math.Min(b, a));
	}

	public void TestDecimalRound() {
		decimal a = 1.5M;
		decimal b = 2.5M;

		Assert(Math.Round(a) + " != 2", Math.Round(a) == 2);
		Assert(Math.Round(b) + " != 2", Math.Round(b) == 2);
		Assert(Decimal.MaxValue == Math.Round(Decimal.MaxValue));
		Assert(Decimal.MinValue == Math.Round(Decimal.MinValue));
	}

	public void TestDecimalRound2() {
		decimal a = 3.45M;
		decimal b = 3.46M;

		AssertEquals ("Should round down", Math.Round(a, 1), 3.4M);
		AssertEquals ("Should round up", Math.Round(b, 1), 3.5M);
	}

	public void TestDoubleRound() {
		double a = 1.5D;
		double b = 2.5D;

		AssertEquals ("Should round up", Math.Round(a), 2D);
		AssertEquals ("Should round down", Math.Round(b), 2D);
		Assert(Double.MaxValue == Math.Round(Double.MaxValue));
		Assert(Double.MinValue == Math.Round(Double.MinValue));
	}

	public void TestDoubleRound2() {
		double a = 3.45D;
		double b = 3.46D;

		AssertEquals ("Should round down", Math.Round(a, 1), 3.4D);
		AssertEquals ("Should round up", Math.Round(b, 1), 3.5D);
	}
	
	public void TestDecimalSign() {
		decimal a = -5M;
		decimal b = 5M;

		Assert(Math.Sign(a) == -1);
		Assert(Math.Sign(b) == 1);
		Assert(Math.Sign(0M) == 0);
	}

	public void TestDoubleSign() {
		double a = -5D;
		double b = 5D;

		Assert(Math.Sign(a) == -1);
		Assert(Math.Sign(b) == 1);
		Assert(Math.Sign(0D) == 0);
	}

	public void TestFloatSign() {
		float a = -5F;
		float b = 5F;

		Assert(Math.Sign(a) == -1);
		Assert(Math.Sign(b) == 1);
		Assert(Math.Sign(0F) == 0);
	}

	public void TestIntSign() {
		int a = -5;
		int b = 5;

		Assert(Math.Sign(a) == -1);
		Assert(Math.Sign(b) == 1);
		Assert(Math.Sign(0) == 0);
	}

	public void TestLongSign() {
		long a = -5L;
		long b = 5L;

		Assert(Math.Sign(a) == -1);
		Assert(Math.Sign(b) == 1);
		Assert(Math.Sign(0L) == 0);
	}

	public void TestSbyteSign() {
		sbyte a = -5;
		sbyte b = 5;

		Assert(Math.Sign(a) == -1);
		Assert(Math.Sign(b) == 1);
		Assert(Math.Sign(0) == 0);
	}

	public void TestShortSign() {
		short a = -5;
		short b = 5;

		Assert(Math.Sign(a) == -1);
		Assert(Math.Sign(b) == 1);
		Assert(Math.Sign(0) == 0);
	}

}

}
