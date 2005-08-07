// Tests for System.Drawing.Point.cs
//
// Author: Mike Kestner (mkestner@speakeasy.net)
// 		   Improvements by Jordi Mas i Hern�ndez <jmas@softcatala.org>
// Copyright (c) 2001 Ximian, Inc.

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;
using System;
using System.Drawing;

namespace MonoTests.System.Drawing{

	[TestFixture]	
	public class PointTest : Assertion {
		Point pt1_1;
		Point pt1_0;
		Point pt0_1;
	
		[TearDown]
		public void Clean() {}
		
		[SetUp]
		public void GetReady()		
		{
			pt1_1 = new Point (1, 1);
			pt1_0 = new Point (1, 0);
			pt0_1 = new Point (0, 1);
		}
				
	
		[Test]
		public void EqualsTest () 
		{
			AssertEquals (pt1_1, pt1_1);
			AssertEquals (pt1_1, new Point (1, 1));
			Assert (!pt1_1.Equals (pt1_0));
			Assert (!pt1_1.Equals (pt0_1));
			Assert (!pt1_0.Equals (pt0_1));
		}
		
		[Test]
		public void EqualityOpTest () 
		{
			Assert (pt1_1 == pt1_1);
			Assert (pt1_1 == new Point (1, 1));
			Assert (!(pt1_1 == pt1_0));
			Assert (!(pt1_1 == pt0_1));
			Assert (!(pt1_0 == pt0_1));
		}

		[Test]
		public void InequalityOpTest () 
		{
			Assert (!(pt1_1 != pt1_1));
			Assert (!(pt1_1 != new Point (1, 1)));
			Assert (pt1_1 != pt1_0);
			Assert (pt1_1 != pt0_1);
			Assert (pt1_0 != pt0_1);
		}
	
		[Test]
		public void CeilingTest () 
		{
			PointF ptf = new PointF (0.8f, 0.3f);
			AssertEquals (pt1_1, Point.Ceiling (ptf));
		}
	
		[Test]
		public void RoundTest () 
		{
			PointF ptf = new PointF (0.8f, 1.3f);
			AssertEquals (pt1_1, Point.Round (ptf));
		}
	
		[Test]
		public void TruncateTest () 
		{
			PointF ptf = new PointF (0.8f, 1.3f);
			AssertEquals (pt0_1, Point.Truncate (ptf));
		}
	
		[Test]
		public void NullTest () 
		{
			Point pt = new Point (0, 0);
			AssertEquals (pt, Point.Empty);
		}
	
		[Test]
		public void AdditionTest () 
		{
			AssertEquals (pt1_1, pt1_0 + new Size (0, 1));
			AssertEquals (pt1_1, pt0_1 + new Size (1, 0));
		}
	
		[Test]
		public void SubtractionTest () 
		{
			AssertEquals (pt1_0, pt1_1 - new Size (0, 1));
			AssertEquals (pt0_1, pt1_1 - new Size (1, 0));
		}
	
		[Test]
		public void Point2SizeTest () 
		{
			Size sz1 = new Size (1, 1);
			Size sz2 = (Size) pt1_1;
	
			AssertEquals (sz1, sz2);
		}
	
		[Test]
		public void Point2PointFTest () 
		{
			PointF ptf1 = new PointF (1, 1);
			PointF ptf2 = pt1_1;
	
			AssertEquals (ptf1, ptf2);
		}
	
		[Test]
		public void ConstructorTest () 
		{
			int i = (1 << 16) + 1;
			Size sz = new Size (1, 1);
			Point pt_i = new Point (i);
			Point pt_sz = new Point (sz);
	
			AssertEquals (pt_i, pt_sz);
			AssertEquals (pt_i, pt1_1);
			AssertEquals (pt_sz, pt1_1);
		}
		
		[Test]
		public void PropertyTest () 
		{
			Point pt = new Point (0, 0);
	
			Assert (pt.IsEmpty);
			Assert (!pt1_1.IsEmpty);
			AssertEquals (1, pt1_0.X);
			AssertEquals (1, pt0_1.Y);
		}
		
		[Test]
		public void OffsetTest () 
		{
			Point pt = new Point (0, 0);
			pt.Offset (0, 1);
			AssertEquals (pt, pt0_1);
			pt.Offset (1, 0);
			AssertEquals (pt, pt1_1);
			pt.Offset (0, -1);
			AssertEquals (pt, pt1_0);
		}
		
		[Test]
		public void GetHashCodeTest ()
		{
			AssertEquals (0, pt1_1.GetHashCode ());
			AssertEquals (1, pt1_0.GetHashCode ());
			AssertEquals (1, pt0_1.GetHashCode ());
			Point pt = new Point(0xFF, 0xFF00);
			AssertEquals (0xFFFF, pt.GetHashCode ());
		}

		[Test]
		public void ToStringTest ()
		{
			AssertEquals ("{X=1,Y=1}", pt1_1.ToString ());
			AssertEquals ("{X=1,Y=0}", pt1_0.ToString ());
			AssertEquals ("{X=0,Y=1}", pt0_1.ToString ());
		}
	}
}

