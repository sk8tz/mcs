// Tests for System.Drawing.Size.cs
//
// Author: Ravindra (rkumar@novell.com)
//
// Copyright (c) 2004 Novell, Inc.
//

using NUnit.Framework;
using System;
using System.Drawing;

namespace MonoTests.System.Drawing 
{
	[TestFixture]	
	public class SizeTest : Assertion 
	{
		Size sz1_1;
		Size sz1_0;
		Size sz0_1;

		[TearDown]
		public void TearDown () {}

		[SetUp]
		public void SetUp ()		
		{
			sz1_1 = new Size (1, 1);
			sz1_0 = new Size (1, 0);
			sz0_1 = new Size (0, 1);
		}

		[Test]
		public void TestConstructors ()
		{
			Size sz_wh = new Size (1, 5);
			AssertEquals ("C#1", 1, sz_wh.Width);
			AssertEquals ("C#2", 5, sz_wh.Height);

			Size sz_pt = new Size (new Point (1, 5));
			AssertEquals ("C#3", 1, sz_pt.Width);
			AssertEquals ("C#4", 5, sz_pt.Height);

			AssertEquals ("C#5", sz_wh, sz_pt);
		}

		[Test]
		public void TestEmptyField () 
		{
			Size sz = new Size (0, 0);
			AssertEquals ("EMP#1", sz, Size.Empty);
		}

		[Test]
		public void TestProperties () 
		{
			Size sz = new Size (0, 0);
	
			Assert ("P#1", sz.IsEmpty);
			Assert ("P#2", ! sz1_1.IsEmpty);
			AssertEquals ("P#3", 1, sz1_0.Width);
			AssertEquals ("P#4", 1, sz0_1.Height);
		}

		[Test]
		public void TestCeiling ()
		{
			SizeF sf = new SizeF (0.5F, 0.6F);
			AssertEquals ("CL#1", sz1_1, Size.Ceiling (sf));

			sf = new SizeF (1.0F, 1.0F);
			AssertEquals ("CL#2", sz1_1, Size.Ceiling (sf));
		}

		[Test]
		public void TestEquals () 
		{
			AssertEquals ("EQ#1", sz1_1, sz1_1);
			AssertEquals ("EQ#2", sz1_1, new Size (1, 1));
			Assert ("EQ#3", ! sz1_1.Equals (sz1_0));
			Assert ("EQ#4", ! sz1_1.Equals (sz0_1));
			Assert ("EQ#5", ! sz1_0.Equals (sz0_1));
		}

		[Test]
		public void TestRound ()
		{
			SizeF sf = new SizeF (0.3F, 0.7F);
			AssertEquals ("CL#1", sz0_1, Size.Round (sf));

			sf = new SizeF (0.6F, 0.6F);
			AssertEquals ("CL#2", sz1_1, Size.Round (sf));

			sf = new SizeF (1.0F, 1.0F);
			AssertEquals ("CL#3", sz1_1, Size.Round (sf));
		}

		[Test]
		public void Test2String ()
		{
			AssertEquals ("2STR#1", "{Width=1, Height=1}", sz1_1.ToString ());
		}

		[Test]
		public void TestTruncate ()
		{
			SizeF sf = new SizeF (0.8f, 1.3f);
			AssertEquals ("TR#1", sz0_1, Size.Truncate (sf));

			sf = new SizeF (1.9f, 1.9f);
			AssertEquals ("TR#2", sz1_1, Size.Truncate (sf));

			sf = new SizeF (1.0f, 1.0f);
			AssertEquals ("TR#3", sz1_1, Size.Truncate (sf));
		}

		[Test]
		public void TestAddition ()
		{
			AssertEquals ("ADD#1", sz1_1, sz1_0 + sz0_1);
			AssertEquals ("ADD#2", sz1_1, sz1_1 + new Size (0, 0));
		}

		[Test]
		public void TestEqualityOp () 
		{
			Assert ("EOP#1", sz1_1 == sz1_1);
			Assert ("EOP#2", sz1_1 == new Size (1, 1));
			Assert ("EOP#3", ! (sz1_1 == sz1_0));
			Assert ("EOP#4", ! (sz1_1 == sz0_1));
			Assert ("EOP#5", ! (sz1_0 == sz0_1));
		}

		[Test]
		public void TestInequalityOp () 
		{
			Assert ("IOP#1", ! (sz1_1 != sz1_1));
			Assert ("IOP#2", ! (sz1_1 != new Size (1, 1)));
			Assert ("IOP#3", sz1_1 != sz1_0);
			Assert ("IOP#4", sz1_1 != sz0_1);
			Assert ("IOP#5", sz1_0 != sz0_1);
		}
	
		[Test]
		public void TestSubtraction () 
		{
			AssertEquals ("SUB#1", sz1_0, sz1_1 - sz0_1);
			AssertEquals ("SUB#2", sz0_1, sz1_1 - sz1_0);
		}

		[Test]
		public void TestSize2Point ()
		{
			Point pt1 = new Point (1, 1);
			Point pt2 = (Point) sz1_1;
	
			AssertEquals ("SZ2PT#1", pt1, pt2);
		}
	
		[Test]
		public void TestSize2SizeF ()
		{
			SizeF sf1 = new SizeF (1.0F, 1.0F);
			SizeF sf2 = (SizeF) sz1_1;

			AssertEquals ("SZ2SF#1", sf1, sf2);
		}
	}
}

