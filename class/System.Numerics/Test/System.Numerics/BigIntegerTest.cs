// BigIntegerTest.cs
//
// Authors:
// Rodrigo Kumpera <rkumpera@novell.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
//

using System;
using System.Numerics;
using NUnit.Framework;


namespace MonoTests.System.Numerics
{
	[TestFixture]
	public class BigIntegerTest
	{
		static byte[] huge_a = new byte[] {0x1D, 0x33, 0xFB, 0xFE, 0xB1, 0x2, 0x85, 0x44, 0xCA, 0xDC, 0xFB, 0x70, 0xD, 0x39, 0xB1, 0x47, 0xB6, 0xE6, 0xA2, 0xD1, 0x19, 0x1E, 0x9F, 0xE4, 0x3C, 0x1E, 0x16, 0x56, 0x13, 0x9C, 0x4D, 0xD3, 0x5C, 0x74, 0xC9, 0xBD, 0xFA, 0x56, 0x40, 0x58, 0xAC, 0x20, 0x6B, 0x55, 0xA2, 0xD5, 0x41, 0x38, 0xA4, 0x6D, 0xF6, 0x8C, };

		static byte[] huge_b = new byte[] {0x96, 0x5, 0xDA, 0xFE, 0x93, 0x17, 0xC1, 0x93, 0xEC, 0x2F, 0x30, 0x2D, 0x8F, 0x28, 0x13, 0x99, 0x70, 0xF4, 0x4C, 0x60, 0xA6, 0x49, 0x24, 0xF9, 0xB3, 0x4A, 0x41, 0x67, 0xDC, 0xDD, 0xB1, 0xA5, 0xA6, 0xC0, 0x3D, 0x57, 0x9A, 0xCB, 0x29, 0xE2, 0x94, 0xAC, 0x6C, 0x7D, 0xEF, 0x3E, 0xC6, 0x7A, 0xC1, 0xA8, 0xC8, 0xB0, 0x20, 0x95, 0xE6, 0x4C, 0xE1, 0xE0, 0x4B, 0x49, 0xD5, 0x5A, 0xB7, };

		static byte[] huge_add = new byte[] {0xB3, 0x38, 0xD5, 0xFD, 0x45, 0x1A, 0x46, 0xD8, 0xB6, 0xC, 0x2C, 0x9E, 0x9C, 0x61, 0xC4, 0xE0, 0x26, 0xDB, 0xEF, 0x31, 0xC0, 0x67, 0xC3, 0xDD, 0xF0, 0x68, 0x57, 0xBD, 0xEF, 0x79, 0xFF, 0x78, 0x3, 0x35, 0x7, 0x15, 0x95, 0x22, 0x6A, 0x3A, 0x41, 0xCD, 0xD7, 0xD2, 0x91, 0x14, 0x8, 0xB3, 0x65, 0x16, 0xBF, 0x3D, 0x20, 0x95, 0xE6, 0x4C, 0xE1, 0xE0, 0x4B, 0x49, 0xD5, 0x5A, 0xB7, };

		static byte[] a_m_b = new byte[] { 0x87, 0x2D, 0x21, 0x0, 0x1E, 0xEB, 0xC3, 0xB0, 0xDD, 0xAC, 0xCB, 0x43, 0x7E, 0x10, 0x9E, 0xAE, 0x45, 0xF2, 0x55, 0x71, 0x73, 0xD4, 0x7A, 0xEB, 0x88, 0xD3, 0xD4, 0xEE, 0x36, 0xBE, 0x9B, 0x2D, 0xB6, 0xB3, 0x8B, 0x66, 0x60, 0x8B, 0x16, 0x76, 0x17, 0x74, 0xFE, 0xD7, 0xB2, 0x96, 0x7B, 0xBD, 0xE2, 0xC4, 0x2D, 0xDC, 0xDE, 0x6A, 0x19, 0xB3, 0x1E, 0x1F, 0xB4, 0xB6, 0x2A, 0xA5, 0x48, };
		static byte[] b_m_a = new byte[] { 0x79, 0xD2, 0xDE, 0xFF, 0xE1, 0x14, 0x3C, 0x4F, 0x22, 0x53, 0x34, 0xBC, 0x81, 0xEF, 0x61, 0x51, 0xBA, 0xD, 0xAA, 0x8E, 0x8C, 0x2B, 0x85, 0x14, 0x77, 0x2C, 0x2B, 0x11, 0xC9, 0x41, 0x64, 0xD2, 0x49, 0x4C, 0x74, 0x99, 0x9F, 0x74, 0xE9, 0x89, 0xE8, 0x8B, 0x1, 0x28, 0x4D, 0x69, 0x84, 0x42, 0x1D, 0x3B, 0xD2, 0x23, 0x21, 0x95, 0xE6, 0x4C, 0xE1, 0xE0, 0x4B, 0x49, 0xD5, 0x5A, 0xB7, };

		static byte[][] add_a = new byte[][] {
			new byte[] {1},
			new byte[] {0xFF},
			huge_a
		};

		static byte[][] add_b = new byte[][] {
			new byte[] {1},
			new byte[] {1},
			huge_b
		};

		static byte[][] add_c = new byte[][] {
			new byte[] {2},
			new byte[] {0},
			huge_add
		};

		[Test]
		public void TestAdd () {
			for (int i = 0; i < add_a.Length; ++i) {
				var a = new BigInteger (add_a [i]);
				var b = new BigInteger (add_b [i]);
				var c = new BigInteger (add_c [i]);

				Assert.AreEqual (c, a + b, "#" + i + "a");
				Assert.AreEqual (c, b + a, "#" + i + "b");
				Assert.AreEqual (c, BigInteger.Add (a, b), "#" + i + "c");
				Assert.AreEqual (add_c [i], (a + b).ToByteArray (), "#" + i + "d");
			}
		}

		[Test]
		public void TestAdd2 () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };
			for (int i = 0; i < values.Length; ++i) {
				for (int j = 0; j < values.Length; ++j) {
					var a = new BigInteger (values [i]);
					var b = new BigInteger (values [j]);
					var c = a + b;
					Assert.AreEqual (values [i] + values [j], (long)c, "#_" + i + "_" + j);
				}
			}
		}

		[Test]
		public void TestHugeSub () {
			var a = new BigInteger (huge_a);
			var b = new BigInteger (huge_b);
			Assert.AreEqual (a_m_b, (a - b).ToByteArray (), "#1");
			Assert.AreEqual (b_m_a, (b - a).ToByteArray (), "#2");
		}

		[Test]
		public void TestSub () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };
			for (int i = 0; i < values.Length; ++i) {
				for (int j = 0; j < values.Length; ++j) {
					var a = new BigInteger (values [i]);
					var b = new BigInteger (values [j]);
					var c = a - b;
					var d = BigInteger.Subtract (a, b);

					Assert.AreEqual (values [i] - values [j], (long)c, "#_" + i + "_" + j);
					Assert.AreEqual (values [i] - values [j], (long)d, "#_" + i + "_" + j);
				}
			}
		}

		[Test]
		public void TestMin () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };
			for (int i = 0; i < values.Length; ++i) {
				for (int j = 0; j < values.Length; ++j) {
					var a = new BigInteger (values [i]);
					var b = new BigInteger (values [j]);
					var c = BigInteger.Min (a, b);

					Assert.AreEqual (Math.Min (values [i], values [j]), (long)c, "#_" + i + "_" + j);
				}
			}
		}

		[Test]
		public void TestMax () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };
			for (int i = 0; i < values.Length; ++i) {
				for (int j = 0; j < values.Length; ++j) {
					var a = new BigInteger (values [i]);
					var b = new BigInteger (values [j]);
					var c = BigInteger.Max (a, b);

					Assert.AreEqual (Math.Max (values [i], values [j]), (long)c, "#_" + i + "_" + j);
				}
			}
		}

		[Test]
		public void TestAbs () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };
			for (int i = 0; i < values.Length; ++i) {
				var a = new BigInteger (values [i]);
				var c = BigInteger.Abs (a);

				Assert.AreEqual (Math.Abs (values [i]), (long)c, "#_" + i);
			}
		}

		[Test]
		public void TestNegate () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };
			for (int i = 0; i < values.Length; ++i) {
				var a = new BigInteger (values [i]);
				var c = -a;
				var d = BigInteger.Negate (a);

				Assert.AreEqual (-values [i], (long)c, "#_" + i);
				Assert.AreEqual (-values [i], (long)d, "#_" + i);
			}
		}

		[Test]
		public void TestInc () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };
			for (int i = 0; i < values.Length; ++i) {
				var a = new BigInteger (values [i]);
				var b = ++a;

				Assert.AreEqual (++values [i], (long)b, "#_" + i);
			}
		}

		[Test]
		public void TestDec () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };
			for (int i = 0; i < values.Length; ++i) {
				var a = new BigInteger (values [i]);
				var b = --a;

				Assert.AreEqual (--values [i], (long)b, "#_" + i);
			}
		}

		[Test]
		public void TestBitwiseOps () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L, 0xFFFF00000000L };
			for (int i = 0; i < values.Length; ++i) {
				for (int j = 0; j < values.Length; ++j) {
					var a = new BigInteger (values [i]);
					var b = new BigInteger (values [j]);


					Assert.AreEqual (values [i] | values [j], (long)(a | b) , "#b_" + i + "_" + j);
					Assert.AreEqual (values [i] & values [j], (long)(a & b) , "#a_" + i + "_" + j);
					Assert.AreEqual (values [i] ^ values [j], (long)(a ^ b) , "#c_" + i + "_" + j);
					Assert.AreEqual (~values [i], (long)~a , "#d_" + i + "_" + j);
				}
			}
		}

		[Test]
		public void TestLeftShift () {
			Assert.AreEqual (new byte[] {0x00, 0x28},
				(new BigInteger(0x0A) << 10).ToByteArray (), "#1");
			Assert.AreEqual (new byte[] {0x00, 0xD8},
				(new BigInteger(-10) << 10).ToByteArray (), "#2");
			Assert.AreEqual (new byte[] {0x00, 0x00, 0xFF},
				(new BigInteger(-1) << 16).ToByteArray (), "#3");
			Assert.AreEqual (new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A},
				(new BigInteger(0x0A) << 80).ToByteArray (), "#4");
			Assert.AreEqual (new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF6},
				(new BigInteger(-10) << 80).ToByteArray (), "#5");
			Assert.AreEqual (new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF},
				(new BigInteger(-1) << 80).ToByteArray (), "#6");
			Assert.AreEqual (new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x70, 0xD9},
				(new BigInteger(-1234) << 75).ToByteArray (), "#7");
			Assert.AreEqual (new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x91, 0x00},
				(new BigInteger(0x1234) << 75).ToByteArray (), "#8");

			Assert.AreEqual (new byte[] {0xFF, 0x00}, (new BigInteger(0xFF00) << -8).ToByteArray (), "#9");
		}

		[Test]
		public void TestRightShift () {
			Assert.AreEqual (new byte[] {0x16, 0xB0, 0x4C, 0x02},
				(new BigInteger(1234567899L) >> 5).ToByteArray (), "#1");

			Assert.AreEqual (new byte[] {0x2C, 0x93, 0x00},
				(new BigInteger(1234567899L) >> 15).ToByteArray (), "#2");

			Assert.AreEqual (new byte[] {0xFF, 0xFF, 0x7F},
				(new BigInteger(long.MaxValue - 100) >> 40).ToByteArray (), "#3");

			Assert.AreEqual (new byte[] {0xE9, 0x4F, 0xB3, 0xFD},
				(new BigInteger(-1234567899L) >> 5).ToByteArray (), "#4");

			Assert.AreEqual (new byte[] {0xD3, 0x6C, 0xFF},
				(new BigInteger(-1234567899L) >> 15).ToByteArray (), "#5");

			Assert.AreEqual (new byte[] {0x00, 0x00, 0x80},
				(new BigInteger(long.MinValue + 100) >> 40).ToByteArray (), "#6");

			Assert.AreEqual (new byte[] { 0xFF },
				(new BigInteger(-1234567899L) >> 90).ToByteArray (), "#7");

			Assert.AreEqual (new byte[] {0x00},
				(new BigInteger(999999) >> 90).ToByteArray (), "#8");

			Assert.AreEqual (new byte[] {0x00, 0x00, 0xFF, 0x00}, (new BigInteger(0xFF00) >> -8).ToByteArray (), "#9");
		}

		[Test]
		public void CompareOps () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };
			for (int i = 0; i < values.Length; ++i) {
				for (int j = 0; j < values.Length; ++j) {
					var a = new BigInteger (values [i]);
					var b = new BigInteger (values [j]);
					
					Assert.AreEqual (values [i].CompareTo (values [j]), a.CompareTo (b), "#a_" + i + "_" + j);
					Assert.AreEqual (values [i].CompareTo (values [j]), BigInteger.Compare (a, b), "#b_" + i + "_" + j);

					Assert.AreEqual (values [i] < values [j], a < b, "#c_" + i + "_" + j);
					Assert.AreEqual (values [i] <= values [j], a <= b, "#d_" + i + "_" + j);
					Assert.AreEqual (values [i] == values [j], a == b, "#e_" + i + "_" + j);
					Assert.AreEqual (values [i] != values [j], a != b, "#f_" + i + "_" + j);
					Assert.AreEqual (values [i] >= values [j], a >= b, "#g_" + i + "_" + j);
					Assert.AreEqual (values [i] > values [j], a > b, "#h_" + i + "_" + j);
				}
			}
		}

		[Test]
		public void CompareOps2 () {
			BigInteger a = new BigInteger (100000000000L);
			BigInteger b = new BigInteger (28282828282UL);

			Assert.IsTrue (a >= b, "#1");
			Assert.IsTrue (a >= b, "#2");
			Assert.IsFalse (a < b, "#3");
			Assert.IsFalse (a <= b, "#4");
			Assert.AreEqual (1, a.CompareTo (b), "#5");
		}

		[Test]
		public void CompareULong () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L, 0xAA00000000L };
			ulong[] uvalues = new ulong [] {0, 1, 1000, 100000000000L, 999999, 28282828282, 0xAA00000000, ulong.MaxValue };
			for (int i = 0; i < values.Length; ++i) {
				for (int j = 0; j < uvalues.Length; ++j) {
					var a = new BigInteger (values [i]);
					var b = uvalues [j];
					var c = new BigInteger (b);
					
					Assert.AreEqual (a.CompareTo (c), a.CompareTo (b), "#a_" + i + "_" + j);

					Assert.AreEqual (a > c, a > b, "#b_" + i + "_" + j);
					Assert.AreEqual (a < c, a < b, "#c_" + i + "_" + j);
					Assert.AreEqual (a <= c, a <= b, "#d_" + i + "_" + j);
					Assert.AreEqual (a == c, a == b, "#e_" + i + "_" + j);
					Assert.AreEqual (a != c, a != b, "#f_" + i + "_" + j);
					Assert.AreEqual (a >= c, a >= b, "#g_" + i + "_" + j);

					Assert.AreEqual (c > a, b > a, "#ib_" + i + "_" + j);
					Assert.AreEqual (c < a, b < a, "#ic_" + i + "_" + j);
					Assert.AreEqual (c <= a, b <= a, "#id_" + i + "_" + j);
					Assert.AreEqual (c == a, b == a, "#ie_" + i + "_" + j);
					Assert.AreEqual (c != a, b != a, "#if_" + i + "_" + j);
					Assert.AreEqual (c >= a, b >= a, "#ig_" + i + "_" + j);
				}
			}
		}

		[Test]
		public void CompareLong () {
			long[] values = new long [] { -100000000000L, -1000, -1, 0, 1, 1000, 9999999, 100000000000L, 0xAA00000000, long.MaxValue, long.MinValue };

			for (int i = 0; i < values.Length; ++i) {
				for (int j = 0; j < values.Length; ++j) {
					var a = new BigInteger (values [i]);
					var b = values [j];
					var c = new BigInteger (b);
					
					Assert.AreEqual (a.CompareTo (c), a.CompareTo (b), "#a_" + i + "_" + j);

					Assert.AreEqual (a > c, a > b, "#b_" + i + "_" + j);
					Assert.AreEqual (a < c, a < b, "#c_" + i + "_" + j);
					Assert.AreEqual (a <= c, a <= b, "#d_" + i + "_" + j);
					Assert.AreEqual (a == c, a == b, "#e_" + i + "_" + j);
					Assert.AreEqual (a != c, a != b, "#f_" + i + "_" + j);
					Assert.AreEqual (a >= c, a >= b, "#g_" + i + "_" + j);

					Assert.AreEqual (c > a, b > a, "#ib_" + i + "_" + j);
					Assert.AreEqual (c < a, b < a, "#ic_" + i + "_" + j);
					Assert.AreEqual (c <= a, b <= a, "#id_" + i + "_" + j);
					Assert.AreEqual (c == a, b == a, "#ie_" + i + "_" + j);
					Assert.AreEqual (c != a, b != a, "#if_" + i + "_" + j);
					Assert.AreEqual (c >= a, b >= a, "#ig_" + i + "_" + j);
				}
			}
		}

		[Test]
		public void TestEquals () {
				var a = new BigInteger (10);
				var b = new BigInteger (10);
				var c = new BigInteger (-10);

				Assert.AreEqual (a, b, "#1");
				Assert.AreNotEqual (a, c, "#2");
				Assert.AreNotEqual (a, 10, "#3");
		}

		[Test]
		public void ByteArrayCtor ()
		{
			try {
				new BigInteger (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {}

			Assert.AreEqual (0, (int)new BigInteger (new byte [0]), "#2");
		}

		[Test]
		public void IntCtorRoundTrip ()
		{
			int[] values = new int [] {
				int.MinValue, -0x2F33BB, -0x1F33, -0x33, 0, 0x33,
				0x80, 0x8190, 0xFF0011, 0x1234, 0x11BB99, 0x44BB22CC,
				int.MaxValue };
			foreach (var val in values) {
				var a = new BigInteger (val);
				var b = new BigInteger (a.ToByteArray ());

				Assert.AreEqual (val, (int)a, "#a_" + val);
				Assert.AreEqual (val, (int)b, "#b_" + val);
			}
		}

		[Test]
		public void LongCtorRoundTrip ()
		{
			long[] values = new long [] {
				0, long.MinValue, long.MaxValue, -1, 1L + int.MaxValue, -1L + int.MinValue, 0x1234, 0xFFFFFFFFL, 0x1FFFFFFFFL, -0xFFFFFFFFL, -0x1FFFFFFFFL,
				0x100000000L, -0x100000000L, 0x100000001L, -0x100000001L };
			foreach (var val in values) {
				var a = new BigInteger (val);
				var b = new BigInteger (a.ToByteArray ());

				Assert.AreEqual (val, (long)a, "#a_" + val);
				Assert.AreEqual (val, (long)b, "#b_" + val);
			}
		}

		[Test]
		public void ByteArrayCtorRoundTrip ()
		{
			var arr = new byte [] { 1,2,3,4,5,6,7,8,9 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#1");

			arr = new byte [] { 1,2,3,4,5,6,7,8,0xFF, 0x0};
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#2");

			arr = new byte [] { 1,2,3,4,5,6,7,8,9, 0xF0 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#3");

			arr = new byte [] { 1};
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#4");

			arr = new byte [] { 1,2 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#5");

			arr = new byte [] { 1,2,3 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#6");

			arr = new byte [] { 1,2,3,4 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#7");

			arr = new byte [] { 1,2,3,4,5 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#8");

			arr = new byte [] { 1,2,3,4,5,6 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#9");

			arr = new byte [] { 1,2,3,4,5,6,7 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#10");

			arr = new byte [] { 1,2,3,4,5 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#11");

			arr = new byte [] { 0 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#12");

			arr = new byte [] { 0xFF, 00 };
			Assert.AreEqual (arr, new BigInteger (arr).ToByteArray (), "#13");

			arr = new byte [] { 1, 0, 0, 0, 0, 0, };
			Assert.AreEqual (new byte [1] {1}, new BigInteger (arr).ToByteArray (), "#14");
		}

		[Test]
		public void TestIntCtorProperties ()
		{
			BigInteger a = new BigInteger (10);
			Assert.IsTrue (a.IsEven, "#1");
			Assert.IsFalse (a.IsOne, "#2");
			Assert.IsFalse (a.IsPowerOfTwo, "#3");
			Assert.IsFalse (a.IsZero, "#4");
			Assert.AreEqual (1, a.Sign, "#5");

			Assert.IsFalse (new BigInteger (11).IsEven, "#6");
			Assert.IsTrue (new BigInteger (1).IsOne, "#7");
			Assert.IsTrue (new BigInteger (32).IsPowerOfTwo, "#8");
			Assert.IsTrue (new BigInteger (0).IsZero, "#9");
			Assert.AreEqual (0, new BigInteger (0).Sign, "#10");
			Assert.AreEqual (-1, new BigInteger (-99999).Sign, "#11");

			Assert.IsFalse (new BigInteger (0).IsPowerOfTwo, "#12");
			Assert.IsFalse (new BigInteger (-16).IsPowerOfTwo, "#13");
			Assert.IsTrue (new BigInteger (1).IsPowerOfTwo, "#14");
		}

		/*[Test]
		public void TestIntCtorToString ()
		{
			Assert.AreEqual ("5555", new BigInteger (5555).ToString (), "#1");
			Assert.AreEqual ("-99999", new BigInteger (-99999).ToString (), "#2");
		}*/

		[Test]
		public void TestToIntOperator ()
		{
			try {
				int v = (int)new BigInteger (huge_a);
				Assert.Fail ("#1");
			} catch (OverflowException) {}

			try {
				int v = (int)new BigInteger (1L + int.MaxValue);
				Assert.Fail ("#2");
			} catch (OverflowException) {}

			try {
				int v = (int)new BigInteger (-1L + int.MinValue);
				Assert.Fail ("#3");
			} catch (OverflowException) {}

			Assert.AreEqual (int.MaxValue, (int)new BigInteger (int.MaxValue), "#4");
			Assert.AreEqual (int.MinValue, (int)new BigInteger (int.MinValue), "#5");
		}


		[Test]
		public void TestToLongOperator ()
		{
			try {
				long v = (long)new BigInteger (huge_a);
				Assert.Fail ("#1");
			} catch (OverflowException) {}

			//long.MaxValue + 1
			try {
				long v = (long)new BigInteger (new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00 });
				Assert.Fail ("#2");
			} catch (OverflowException) {}

			//TODO long.MinValue - 1
			try {
				long v = (long)new BigInteger (new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F, 0xFF });
				Assert.Fail ("#3");
			} catch (OverflowException) {}

			Assert.AreEqual (long.MaxValue, (long)new BigInteger (long.MaxValue), "#4");
			Assert.AreEqual (long.MinValue, (long)new BigInteger (long.MinValue), "#5");
		}

		[Test]
		public void TestIntCtorToByteArray ()
		{
			Assert.AreEqual (new byte[] { 0xFF }, new BigInteger (-1).ToByteArray (), "#1");
			Assert.AreEqual (new byte[] { 0xD4, 0xFE }, new BigInteger (-300).ToByteArray (), "#2");
			Assert.AreEqual (new byte[] { 0x80, 0x00 }, new BigInteger (128).ToByteArray (), "#3");
			Assert.AreEqual (new byte[] { 0x00, 0x60 }, new BigInteger (0x6000).ToByteArray (), "#4");
			Assert.AreEqual (new byte[] { 0x00, 0x80, 0x00 }, new BigInteger (0x8000).ToByteArray (), "#5");
			Assert.AreEqual (new byte[] { 0xDD, 0xBC, 0x00, 0x7A }, new BigInteger (0x7A00BCDD).ToByteArray (), "#6");
			Assert.AreEqual (new byte[] { 0xFF, 0xFF, 0xFF, 0x7F }, new BigInteger (int.MaxValue).ToByteArray (), "#7");
			Assert.AreEqual (new byte[] { 0x00, 0x00, 0x00, 0x80 }, new BigInteger (int.MinValue).ToByteArray (), "#8");
			Assert.AreEqual (new byte[] { 0x01, 0x00, 0x00, 0x80 }, new BigInteger (int.MinValue + 1).ToByteArray (), "#9");
			Assert.AreEqual (new byte[] { 0x7F }, new BigInteger (0x7F).ToByteArray (), "#10");
			Assert.AreEqual (new byte[] { 0x45, 0xCC, 0xD0 }, new BigInteger (-0x2F33BB).ToByteArray (), "#11");
			Assert.AreEqual (new byte[] { 0 }, new BigInteger (0).ToByteArray (), "#12");
		}

		[Test]
		public void TestLongCtorToByteArray ()
		{
			Assert.AreEqual (new byte[] { 0x01 }, new BigInteger (0x01L).ToByteArray (), "#1");
			Assert.AreEqual (new byte[] { 0x02, 0x01 }, new BigInteger (0x0102L).ToByteArray (), "#2");
			Assert.AreEqual (new byte[] { 0x03, 0x02, 0x01 }, new BigInteger (0x010203L).ToByteArray (), "#3");
			Assert.AreEqual (new byte[] { 0x04, 0x03, 0x2, 0x01 }, new BigInteger (0x01020304L).ToByteArray (), "#4");
			Assert.AreEqual (new byte[] { 0x05, 0x04, 0x03, 0x2, 0x01 }, new BigInteger (0x0102030405L).ToByteArray (), "#5");
			Assert.AreEqual (new byte[] { 0x06, 0x05, 0x04, 0x03, 0x2, 0x01 }, new BigInteger (0x010203040506L).ToByteArray (), "#6");
			Assert.AreEqual (new byte[] { 0x07, 0x06, 0x05, 0x04, 0x03, 0x2, 0x01 }, new BigInteger (0x01020304050607L).ToByteArray (), "#7");
			Assert.AreEqual (new byte[] { 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x2, 0x01 }, new BigInteger (0x0102030405060708L).ToByteArray (), "#8");

			Assert.AreEqual (new byte[] { 0xFF }, new BigInteger (-0x01L).ToByteArray (), "#1m");
			Assert.AreEqual (new byte[] { 0xFE, 0xFE}, new BigInteger (-0x0102L).ToByteArray (), "#2m");
			Assert.AreEqual (new byte[] { 0xFD, 0xFD, 0xFE }, new BigInteger (-0x010203L).ToByteArray (), "#3m");
			Assert.AreEqual (new byte[] { 0xFC, 0xFC, 0xFD, 0xFE}, new BigInteger (-0x01020304L).ToByteArray (), "#4m");
			Assert.AreEqual (new byte[] { 0xFB, 0xFB, 0xFC, 0xFD, 0xFE }, new BigInteger (-0x0102030405L).ToByteArray (), "#5m");
			Assert.AreEqual (new byte[] { 0xFA, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE }, new BigInteger (-0x010203040506L).ToByteArray (), "#6m");
			Assert.AreEqual (new byte[] { 0xF9, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE }, new BigInteger (-0x01020304050607L).ToByteArray (), "#7m");
			Assert.AreEqual (new byte[] { 0xF8, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE }, new BigInteger (-0x0102030405060708L).ToByteArray (), "#8m");


			Assert.AreEqual (new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F }, new BigInteger (long.MaxValue).ToByteArray (), "#9");
			Assert.AreEqual (new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 }, new BigInteger (long.MinValue).ToByteArray (), "#10");

			Assert.AreEqual (new byte[] { 0xFF, 0xFF, 0xFF, 0x7F, 0xFF }, new BigInteger (-2147483649L).ToByteArray (), "11");


		}

		[Test]
		public void IComparable () {
			var a = new BigInteger (99);
			Assert.AreEqual (-1, a.CompareTo (100), "#1");
			Assert.AreEqual (1, a.CompareTo (null), "#2");
		}
	}
}
