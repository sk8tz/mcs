// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Oren Gurfinkel   <oreng@mainsoft.com>
//   Ofer Borstein
//
// Copyright (c) 2004 Mainsoft Co.
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
using System.ComponentModel;
using System.Data;
using MonoTests.System.Data.Test.Utils;

namespace MonoTests.System.Data
{
	[TestFixture] public class DataColumnCollectionTest2
	{
		private int counter = 0;

		[Test] public void Add()
		{
			DataColumn dc = null;
			DataTable dt = new DataTable();

			//----------------------------- check default --------------------
			dc = dt.Columns.Add();
			// Add column 1
			Assert.AreEqual("Column1", dc.ColumnName, "DCC1");

			// Add column 2
			dc = dt.Columns.Add();
			Assert.AreEqual("Column2", dc.ColumnName, "DCC2");

			dc = dt.Columns.Add();
			// Add column 3
			Assert.AreEqual("Column3", dc.ColumnName, "DCC3");

			dc = dt.Columns.Add();
			// Add column 4
			Assert.AreEqual("Column4", dc.ColumnName, "DCC4");

			dc = dt.Columns.Add();
			// Add column 5
			Assert.AreEqual("Column5", dc.ColumnName, "DCC5");
			Assert.AreEqual(5, dt.Columns.Count, "DCC6");

			//----------------------------- check Add/Remove from begining --------------------
			dt = initTable();

			dt.Columns.Remove(dt.Columns[0]);
			dt.Columns.Remove(dt.Columns[0]);
			dt.Columns.Remove(dt.Columns[0]);

			// check column 4 - remove - from begining
			Assert.AreEqual("Column4", dt.Columns[0].ColumnName, "DCC7");

			// check column 5 - remove - from begining
			Assert.AreEqual("Column5", dt.Columns[1].ColumnName , "DCC8");
			Assert.AreEqual(2, dt.Columns.Count, "DCC9");

			dt.Columns.Add();
			dt.Columns.Add();
			dt.Columns.Add();
			dt.Columns.Add();

			// check column 0 - Add new  - from begining
			Assert.AreEqual("Column4", dt.Columns[0].ColumnName , "DCC10");

			// check column 1 - Add new - from begining
			Assert.AreEqual("Column5", dt.Columns[1].ColumnName , "DCC11");

			// check column 2 - Add new - from begining
			Assert.AreEqual("Column6", dt.Columns[2].ColumnName , "DCC12");

			// check column 3 - Add new - from begining
			Assert.AreEqual("Column7", dt.Columns[3].ColumnName , "DCC13");

			// check column 4 - Add new - from begining
			Assert.AreEqual("Column8", dt.Columns[4].ColumnName , "DCC14");

			// check column 5 - Add new - from begining
			Assert.AreEqual("Column9", dt.Columns[5].ColumnName , "DCC15");

			//----------------------------- check Add/Remove from middle --------------------

			dt = initTable();

			dt.Columns.Remove(dt.Columns[2]);
			dt.Columns.Remove(dt.Columns[2]);
			dt.Columns.Remove(dt.Columns[2]);

			// check column 0 - remove - from Middle
			Assert.AreEqual("Column1", dt.Columns[0].ColumnName, "DCC16");

			// check column 1 - remove - from Middle
			Assert.AreEqual("Column2", dt.Columns[1].ColumnName , "DCC17");

			dt.Columns.Add();
			dt.Columns.Add();
			dt.Columns.Add();
			dt.Columns.Add();

			// check column 0 - Add new  - from Middle
			Assert.AreEqual("Column1", dt.Columns[0].ColumnName , "DCC18");

			// check column 1 - Add new - from Middle
			Assert.AreEqual("Column2", dt.Columns[1].ColumnName , "DCC19");

			// check column 2 - Add new - from Middle
			Assert.AreEqual("Column3", dt.Columns[2].ColumnName , "DCC20");

			// check column 3 - Add new - from Middle
			Assert.AreEqual("Column4", dt.Columns[3].ColumnName , "DCC21");

			// check column 4 - Add new - from Middle
			Assert.AreEqual("Column5", dt.Columns[4].ColumnName , "DCC22");

			// check column 5 - Add new - from Middle
			Assert.AreEqual("Column6", dt.Columns[5].ColumnName , "DCC23");

			//----------------------------- check Add/Remove from end --------------------

			dt = initTable();

			dt.Columns.Remove(dt.Columns[4]);
			dt.Columns.Remove(dt.Columns[3]);
			dt.Columns.Remove(dt.Columns[2]);

			// check column 0 - remove - from end
			Assert.AreEqual("Column1", dt.Columns[0].ColumnName, "DCC24");

			// check column 1 - remove - from end
			Assert.AreEqual("Column2", dt.Columns[1].ColumnName , "DCC25");

			dt.Columns.Add();
			dt.Columns.Add();
			dt.Columns.Add();
			dt.Columns.Add();

			// check column 0 - Add new  - from end
			Assert.AreEqual("Column1", dt.Columns[0].ColumnName , "DCC26");

			// check column 1 - Add new - from end
			Assert.AreEqual("Column2", dt.Columns[1].ColumnName , "DCC27");

			// check column 2 - Add new - from end
			Assert.AreEqual("Column3", dt.Columns[2].ColumnName , "DCC28");

			// check column 3 - Add new - from end
			Assert.AreEqual("Column4", dt.Columns[3].ColumnName , "DCC29");

			// check column 4 - Add new - from end
			Assert.AreEqual("Column5", dt.Columns[4].ColumnName , "DCC30");

			// check column 5 - Add new - from end
			Assert.AreEqual("Column6", dt.Columns[5].ColumnName , "DCC31");
		}

		private DataTable initTable()
		{
			DataTable dt = new DataTable();
			for (int i=0; i<5; i++)
			{
				dt.Columns.Add();
			}
			return dt;
	   }

		[Test] public void TestAdd_ByTableName()
		{
			//this test is from boris

			DataSet ds = new DataSet();
			DataTable dt = new DataTable();
			ds.Tables.Add(dt);

			// add one column
			dt.Columns.Add("id1",typeof(int));

			// DataColumnCollection add
			Assert.AreEqual(1, dt.Columns.Count , "DCC32");

			// add row
			DataRow dr = dt.NewRow();
			dt.Rows.Add(dr);

			// remove column
			dt.Columns.Remove("id1");

			// DataColumnCollection remove
			Assert.AreEqual(0, dt.Columns.Count , "DCC33");

			//row is still there

			// now add column
			dt.Columns.Add("id2",typeof(int));

			// DataColumnCollection add again
			Assert.AreEqual(1, dt.Columns.Count , "DCC34");
		}

		[Test] public void TestCanRemove_ByDataColumn()
		{
			DataTable dt = DataProvider.CreateUniqueConstraint();
			DataColumn dummyCol = new DataColumn();
			Assert.AreEqual(false, dt.Columns.CanRemove(null), "DCC35"); //Cannot remove null column
			Assert.AreEqual(false, dt.Columns.CanRemove(dummyCol), "DCC36"); //Don't belong to this table
			Assert.AreEqual(false, dt.Columns.CanRemove(dt.Columns[0]), "DCC37"); //It belongs to unique constraint
			Assert.AreEqual(true, dt.Columns.CanRemove(dt.Columns[1]), "DCC38");
		}
		[Test] public void TestCanRemove_ForigenConstraint()
		{
			DataSet ds = DataProvider.CreateForigenConstraint();

			Assert.AreEqual(false, ds.Tables["child"].Columns.CanRemove(ds.Tables["child"].Columns["parentId"]), "DCC39");//Forigen
			Assert.AreEqual(false, ds.Tables["parent"].Columns.CanRemove(ds.Tables["child"].Columns["parentId"]), "DCC40");//Parent
		}
		[Test] public void TestCanRemove_ParentRelations()
		{
			DataSet ds = new DataSet();

			ds.Tables.Add("table1");
			ds.Tables.Add("table2");
			ds.Tables["table1"].Columns.Add("col1");
			ds.Tables["table2"].Columns.Add("col1");

			ds.Tables[1].ParentRelations.Add("name1",ds.Tables[0].Columns["col1"],ds.Tables[1].Columns["col1"],false);

			Assert.AreEqual(false, ds.Tables[1].Columns.CanRemove(ds.Tables[1].Columns["col1"]), "DCC41"); //Part of a parent
			Assert.AreEqual(false, ds.Tables[0].Columns.CanRemove(ds.Tables[0].Columns["col1"]), "DCC42"); //Part of a child
		}

		[Test] public void TestCanRemove_Expression()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("col1",typeof(string));
			dt.Columns.Add("col2",typeof(string),"sum(col1)");

			Assert.AreEqual(false, dt.Columns.CanRemove(dt.Columns["col1"]), "DCC43"); //Col1 is a part of expression
		}

		[Test] public void CollectionChanged()
		{
			counter = 0;
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.Columns.CollectionChanged+=new CollectionChangeEventHandler(Columns_CollectionChanged);
			dt.Columns.Add("tempCol");
			dt.Columns.Remove("tempCol");
			Assert.AreEqual(2, counter, "DCC44");
		}

		private void Columns_CollectionChanged(object sender, CollectionChangeEventArgs e)
		{
			counter++;
		}

		[Test] public void TestContains_ByColumnName()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			Assert.AreEqual(true, dt.Columns.Contains("ParentId"), "DCC45");
			Assert.AreEqual(true, dt.Columns.Contains("String1"), "DCC46");
			Assert.AreEqual(true, dt.Columns.Contains("ParentBool"), "DCC47");

			Assert.AreEqual(false, dt.Columns.Contains("ParentId1"), "DCC48");
			dt.Columns.Remove("ParentId");
			Assert.AreEqual(false, dt.Columns.Contains("ParentId"), "DCC49");

			dt.Columns["String1"].ColumnName = "Temp1";

			Assert.AreEqual(false, dt.Columns.Contains("String1"), "DCC50");
			Assert.AreEqual(true, dt.Columns.Contains("Temp1"), "DCC51");
		}
		public void NotReadyTestContains_S2() // FIXME: fails in MS
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			Assert.AreEqual(false, dt.Columns.Contains(null), "DCC52");
		}


		[Test] public void Count()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			Assert.AreEqual(6, dt.Columns.Count, "DCC55");

			dt.Columns.Add("temp1");
			Assert.AreEqual(7, dt.Columns.Count, "DCC56");

			dt.Columns.Remove("temp1");
			Assert.AreEqual(6, dt.Columns.Count, "DCC57");

			dt.Columns.Remove("ParentId");
			Assert.AreEqual(5, dt.Columns.Count, "DCC58");
		}

		[Test] public void TestIndexOf_ByDataColumn()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			for (int i=0;i<dt.Columns.Count;i++)
			{
				Assert.AreEqual(i, dt.Columns.IndexOf(dt.Columns[i]), "DCC59");
			}

			DataColumn col = new DataColumn();

			Assert.AreEqual(-1, dt.Columns.IndexOf(col), "DCC60");

			Assert.AreEqual(-1, dt.Columns.IndexOf((DataColumn)null), "DCC61");
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")] 
		public void TestIndexOf_ByColumnName()
		{
			DataTable dt = DataProvider.CreateParentDataTable();

			for (int i=0;i<dt.Columns.Count;i++)
			{
				Assert.AreEqual(i, dt.Columns.IndexOf(dt.Columns[i].ColumnName), "DCC62");
			}

			DataColumn col = new DataColumn();

			Assert.AreEqual(-1, dt.Columns.IndexOf("temp1"), "DCC63");

			Assert.AreEqual(-1, dt.Columns.IndexOf((string)null), "DCC64");
		}

		[Test] public void TestRemove_ByDataColumn()
		{
			//prepare a DataSet with DataTable to be checked
			DataTable dtSource = new DataTable();
			dtSource.Columns.Add("Col_0", typeof(int));
			dtSource.Columns.Add("Col_1", typeof(int));
			dtSource.Columns.Add("Col_2", typeof(int));
			dtSource.Rows.Add(new object[] {0,1,2});

			DataTable dt = null;

			//------Check Remove first column---------
			dt = dtSource.Clone();
			dt.ImportRow(dtSource.Rows[0]);

			dt.Columns.Remove(dt.Columns[0]);
			// Remove first column - check column count
			Assert.AreEqual(2, dt.Columns.Count , "DCC65");

			// Remove first column - check column removed
			Assert.AreEqual(false, dt.Columns.Contains("Col_0"), "DCC66");

			// Remove first column - check column 0 data
			Assert.AreEqual(1, dt.Rows[0][0], "DCC67");

			// Remove first column - check column 1 data
			Assert.AreEqual(2, dt.Rows[0][1], "DCC68");

			//------Check Remove middle column---------
			dt = dtSource.Clone();
			dt.ImportRow(dtSource.Rows[0]);

			dt.Columns.Remove(dt.Columns[1]);
			// Remove middle column - check column count
			Assert.AreEqual(2, dt.Columns.Count , "DCC69");

			// Remove middle column - check column removed
			Assert.AreEqual(false, dt.Columns.Contains("Col_1"), "DCC70");

			// Remove middle column - check column 0 data
			Assert.AreEqual(0, dt.Rows[0][0], "DCC71");

			// Remove middle column - check column 1 data
			Assert.AreEqual(2, dt.Rows[0][1], "DCC72");

			//------Check Remove last column---------
			dt = dtSource.Clone();
			dt.ImportRow(dtSource.Rows[0]);

			dt.Columns.Remove(dt.Columns[2]);
			// Remove last column - check column count
			Assert.AreEqual(2, dt.Columns.Count , "DCC73");

			// Remove last column - check column removed
			Assert.AreEqual(false, dt.Columns.Contains("Col_2"), "DCC74");

			// Remove last column - check column 0 data
			Assert.AreEqual(0, dt.Rows[0][0], "DCC75");

			// Remove last column - check column 1 data
			Assert.AreEqual(1, dt.Rows[0][1], "DCC76");

			//------Check Remove column exception---------
			dt = dtSource.Clone();
			dt.ImportRow(dtSource.Rows[0]);
			// Check Remove column exception - Column name not exists
			try {
				DataColumn dc = new DataColumn();
				dt.Columns.Remove(dc);
				Assert.Fail("DCC77: Remove failed to throw ArgmentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DCC78: Remove. Wrong exception type. Got:" + exc);
			}
		}
	}
}
