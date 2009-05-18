//
// DataRowComparerTest.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc. http://www.novell.com
//

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

using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataRowComparerTest
	{
		[Test]
		public void Default ()
		{
			DataRowComparer<DataRow> c1 = DataRowComparer.Default;
			DataRowComparer<DataRow> c2 = DataRowComparer.Default;
			Assert.AreSame (c1, c2);
		}

		[Test]
		public void Equals ()
		{
			DataRowComparer<DataRow> c = DataRowComparer.Default;

			DataTable dtA = new DataTable ("tableA");
			dtA.Columns.Add ("col1", typeof (int));
			dtA.Columns.Add ("col2", typeof (string));
			dtA.Columns.Add ("col3", typeof (DateTime));
			DataRow r1 = dtA.Rows.Add (3, "bar", new DateTime (2008, 5, 7));
			DataRow r2 = dtA.Rows.Add (3, "bar", new DateTime (2008, 5, 7));

			Assert.IsTrue (c.Equals (r1, r2), "#A1");
			r1 ["col1"] = 4;
			Assert.IsFalse (c.Equals (r1, r2), "#A2");
			r1 ["col1"] = 3;
			Assert.IsTrue (c.Equals (r1, r2), "#A3");

			r1 ["col2"] = null;
			Assert.IsFalse (c.Equals (r1, r2), "#B1");
			r2 ["col2"] = null;
			Assert.IsTrue (c.Equals (r1, r2), "#B2");
			r1 ["col2"] = "bar";
			Assert.IsFalse (c.Equals (r1, r2), "#B3");
			r2 ["col2"] = "bar";
			Assert.IsTrue (c.Equals (r1, r2), "#B4");

			r1 ["col3"] = DBNull.Value;
			Assert.IsFalse (c.Equals (r1, r2), "#C1");
			r2 ["col3"] = DBNull.Value;
			Assert.IsTrue (c.Equals (r1, r2), "#C2");
			r1 ["col3"] = new DateTime (2008, 5, 7);
			Assert.IsFalse (c.Equals (r1, r2), "#C3");
			r2 ["col3"] = new DateTime (2008, 5, 7);
			Assert.IsTrue (c.Equals (r1, r2), "#C4");

			Assert.IsFalse (c.Equals (r1, null), "#D1");
			Assert.IsFalse (c.Equals (null, r1), "#D2");
			Assert.IsTrue (c.Equals (null, null), "#D3");

			// rows do not have to share the same parent

			DataTable dtB = new DataTable ("tableB");
			dtB.Columns.Add ("colB1", typeof (int));
			dtB.Columns.Add ("colB2", typeof (string));
			dtB.Columns.Add ("colB3", typeof (DateTime));

			DataRow r3 = dtB.Rows.Add (3, "bar", new DateTime (2008, 5, 7));

			Assert.IsTrue (c.Equals (r1, r3), "#E1");
			r1 ["col1"] = 4;
			Assert.IsFalse (c.Equals (r1, r3), "#E2");
			r1 ["col1"] = 3;
			Assert.IsTrue (c.Equals (r1, r3), "#E3");

			// difference in rowstate is ignored

			r1.AcceptChanges ();

			Assert.IsTrue (c.Equals (r1, r2), "#G1");
			r1 ["col1"] = 4;
			Assert.IsFalse (c.Equals (r1, r2), "#G2");
			r1 ["col1"] = 3;
			Assert.IsTrue (c.Equals (r1, r2), "#G3");

			// rows have different number of columns

			DataTable dtC = new DataTable ("tableC");
			dtC.Columns.Add ("colC1", typeof (int));
			dtC.Columns.Add ("colC2", typeof (string));

			DataRow r4 = dtC.Rows.Add (3, "bar");

			Assert.IsFalse (c.Equals (r1, r4), "#H1");
			r1 ["col3"] = DBNull.Value;
			Assert.IsFalse (c.Equals (r1, r4), "#H2");
		}

		[Test]
		public void Equals_Rows_Detached ()
		{
			DataRowComparer<DataRow> c = DataRowComparer.Default;

			DataTable dt = new DataTable ("tableA");
			dt.Columns.Add ("col1", typeof (int));
			dt.Columns.Add ("col2", typeof (string));
			dt.Columns.Add ("col3", typeof (DateTime));
			DataRow r1 = dt.Rows.Add (3, "bar", new DateTime (2008, 5, 7));
			DataRow r2 = dt.NewRow ();
			r2.ItemArray = new object [] { 3, "bar", new DateTime (2008, 5, 7) };
			DataRow r3 = dt.NewRow ();
			r3.ItemArray = new object [] { 3, "bar", new DateTime (2008, 5, 7) };

			// left row detached
			Assert.IsTrue (c.Equals (r2, r1), "#A1");
			r1 ["col1"] = 4;
			Assert.IsFalse (c.Equals (r2, r1), "#A2");
			r1 ["col1"] = 3;
			Assert.IsTrue (c.Equals (r2, r1), "#A3");

			// right row detached
			Assert.IsTrue (c.Equals (r1, r2), "#B1");
			r1 ["col2"] = "baz";
			Assert.IsFalse (c.Equals (r1, r2), "#B2");
			r1 ["col2"] = "bar";
			Assert.IsTrue (c.Equals (r1, r2), "#B3");

			// both rows detached
			Assert.IsTrue (c.Equals (r2, r3), "#C1");
			r2 ["col3"] = new DateTime (2008, 6, 7);
			Assert.IsFalse (c.Equals (r2, r3), "#C2");
			r2 ["col3"] = new DateTime (2008, 5, 7);
			Assert.IsTrue (c.Equals (r2, r3), "#C3");
		}

		[Test]
		public void Equals_Rows_Deleted ()
		{
			DataRowComparer<DataRow> c = DataRowComparer.Default;

			DataTable dtA = new DataTable ("tableA");
			dtA.Columns.Add ("col1", typeof (int));
			dtA.Columns.Add ("col2", typeof (string));
			dtA.Columns.Add ("col3", typeof (DateTime));
			DataRow r1 = dtA.Rows.Add (3, "bar", new DateTime (2008, 5, 7));
			DataRow r2 = dtA.Rows.Add (3, "bar", new DateTime (2008, 5, 7));

			r1.Delete ();

			// left row deleted
			try {
				c.Equals (r1, r2);
				Assert.Fail ("#A1");
			} catch (RowNotInTableException ex) {
				Assert.AreEqual (typeof (RowNotInTableException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			// right row deleted
			try {
				c.Equals (r2, r1);
				Assert.Fail ("#B1");
			} catch (RowNotInTableException ex) {
				Assert.AreEqual (typeof (RowNotInTableException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}

			r2.Delete ();

			// both rows deleted
			try {
				c.Equals (r2, r1);
				Assert.Fail ("#C1");
			} catch (RowNotInTableException ex) {
				Assert.AreEqual (typeof (RowNotInTableException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
			}
		}

		[Test]
		public void GetHashCodeWithVersions ()
		{
			DataSet ds = new DataSet ();
			DataTable dt = new DataTable ("MyTable");
			ds.Tables.Add (dt);
			dt.Columns.Add ("col1");
			dt.Columns.Add ("col2");
			DataRow r1 = dt.Rows.Add (new object [] {"foo", "bar"});
			DataRow r2 = dt.Rows.Add (new object [] {"foo", "bar"});
			ds.AcceptChanges ();
			DataRowComparer<DataRow> c = DataRowComparer.Default;
			Assert.IsTrue (c.GetHashCode (r1) == c.GetHashCode (r2), "#1");
			/*
			// LAMESPEC: .NET fails here
			r2 ["col2"] = "baz";
			r2.AcceptChanges ();
			Assert.IsFalse (c.GetHashCode (r1) == c.GetHashCode (r2), "#2");
			ds.AcceptChanges (); // now r2 original value is "baz"
			r2 ["col2"] = "bar";
			Assert.IsFalse (c.GetHashCode (r1) == c.GetHashCode (r2), "#3");
			// LAMESPEC: .NET fails here
			DataRow r3 = dt.Rows.Add (new object [] {"foo", "baz"});
			Assert.IsFalse (c.GetHashCode (r1) == c.GetHashCode (r3), "#4");
			*/
		}

		[Test]
		public void GetHashCode_Row_Null ()
		{
			DataRowComparer<DataRow> c = DataRowComparer.Default;

			try {
				c.GetHashCode (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("row", ex.ParamName, "#5");
			}
		}
	}
}
