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
using System.IO;
using System.ComponentModel;
using System.Data;
using MonoTests.System.Data.Utils;

namespace MonoTests.System.Data
{
	[TestFixture] public class DataRowTest2
	{
		bool _rowChanged;

		[SetUp]
		public void SetUp ()
		{
			_rowChanged = false;
		}

		[Test] public void AcceptChanges()
		{
			DataTable myTable = new DataTable("myTable"); 
			DataRow myRow;
			myRow = myTable.NewRow();
			myTable.Rows.Add(myRow);

			// DataRow AcceptChanges
			// DataRowState.Added -> DataRowState.Unchanged 
			myTable.AcceptChanges();
			Assert.AreEqual(DataRowState.Unchanged , myRow.RowState , "DRW1");
		}

		[Test] public void CancelEdit()
		{
			DataTable myTable = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Id",typeof(int));
			dc.Unique=true;
			myTable.Columns.Add(dc);
			myTable.Rows.Add(new object[] {1});
			myTable.Rows.Add(new object[] {2});
			myTable.Rows.Add(new object[] {3});

			DataRow myRow = myTable.Rows[0];
			myRow.BeginEdit();
			myRow[0] = 7;
			myRow.CancelEdit();

			// DataRow CancelEdit
			Assert.AreEqual(true ,  (int)myRow[0] == 1, "DRW2");
		}

		[Test] public void ClearErrors()
		{
			DataTable dt = new DataTable("myTable"); 
			DataRow dr = dt.NewRow();
			dr.RowError = "err";

			// DataRow ClearErrors
			Assert.AreEqual(true ,  dr.HasErrors , "DRW3");

			// DataRow ClearErrors
			dr.ClearErrors();
			Assert.AreEqual(false ,  dr.HasErrors , "DRW4");
		}

		[Test] public void Delete()
		{
			DataTable myTable = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Id",typeof(int));
			dc.Unique=true;
			myTable.Columns.Add(dc);
			myTable.Rows.Add(new object[] {1});
			myTable.Rows.Add(new object[] {2});
			myTable.Rows.Add(new object[] {3});
			myTable.AcceptChanges();

			DataRow myRow = myTable.Rows[0];
			myRow.Delete();

			// Delete1
			Assert.AreEqual(DataRowState.Deleted  ,  myRow.RowState , "DRW5");

			// Delete2
			myTable.AcceptChanges();
			Assert.AreEqual(DataRowState.Detached  ,  myRow.RowState , "DRW6");
		}

		[Test] public void EndEdit()
		{
			DataTable myTable = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Id",typeof(int));
			dc.Unique=true;
			myTable.Columns.Add(dc);
			myTable.Rows.Add(new object[] {1});
			myTable.Rows.Add(new object[] {2});
			myTable.Rows.Add(new object[] {3});

			DataRow myRow = myTable.Rows[0];

			int iProposed;
			//After calling the DataRow object's BeginEdit method, if you change the value, the Current and Proposed values become available
			myRow.BeginEdit();
			myRow[0] = 7;
			iProposed = (int)myRow[0,DataRowVersion.Proposed];
			myRow.EndEdit();

			// EndEdit
			Assert.AreEqual(iProposed ,  (int)myRow[0,DataRowVersion.Current] , "DRW7");
		}

		[Test] public void Equals()
		{
			DataTable myTable = new DataTable("myTable"); 
			DataRow dr1,dr2;
			dr1 = myTable.NewRow();
			dr2 = myTable.NewRow();

			// not equals
			Assert.AreEqual(false  , dr1.Equals(dr2), "DRW8");

        	dr1=dr2;
			// equals
			Assert.AreEqual(true , dr1.Equals(dr2), "DRW9");
		}

		[Test] public void GetChildRows_ByDataRealtion()
		{
			DataRow dr;
			DataRow[] drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();

			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 

			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			dr = dtParent.Rows[0];

			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);
			//Get Excepted result
			drArrExcepted = dtChild.Select("ParentId=" + dr["ParentId"]);
			//Get Result
			drArrResult = dr.GetChildRows(dRel);

			// GetChildRows_D
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW10");
		}

		[Test] public void GetChildRows_ByDataRealtionDataRowVersion()
		{
			DataRow drParent;
			DataRow[] drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);

			drParent = dtParent.Rows[0];

			// Teting: DateTime.Now.ToShortTimeString()
			//Get Excepted result
			drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows );
			//Get Result DataRowVersion.Current
			drArrResult = drParent.GetChildRows(dRel,DataRowVersion.Current);
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW11");

			// Teting: DataRow.GetParentRows_D_D
			//Get Excepted result
			drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.OriginalRows );
			//Get Result DataRowVersion.Current
			drArrResult = drParent.GetChildRows(dRel,DataRowVersion.Original );
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW12");

			// Teting: DataRow.GetParentRows_D_D
			//Get Excepted result, in this case Current = Default
			drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows);
			//Get Result DataRowVersion.Current
			drArrResult = drParent.GetChildRows(dRel,DataRowVersion.Default  );
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW13");

			// Teting: DataRow.GetParentRows_D_D
			drParent.BeginEdit();
			drParent["String1"] = "Value";
			//Get Excepted result
			drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows );
			//Get Result DataRowVersion.Current
			drArrResult = drParent.GetChildRows(dRel,DataRowVersion.Proposed  );
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW14");
		}

		[Test] public void GetChildRows_ByName()
		{
			DataRow dr;
			DataRow[] drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();

			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 

			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			dr = dtParent.Rows[0];

			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);
			//Get Excepted result
			drArrExcepted = dtChild.Select("ParentId=" + dr["ParentId"]);
			//Get Result
			drArrResult = dr.GetChildRows("Parent-Child");

			// GetChildRows_S
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW15");
		}

		[Test] public void GetChildRows_ByNameDataRowVersion()
		{
			DataRow drParent;
			DataRow[] drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);

			drParent = dtParent.Rows[0];

			// GetChildRows_SD 1
			//Get Excepted result
			drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows );
			//Get Result DataRowVersion.Current
			drArrResult = drParent.GetChildRows("Parent-Child",DataRowVersion.Current);
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW16");

			// GetChildRows_SD 2
			//Get Excepted result
			drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.OriginalRows );
			//Get Result DataRowVersion.Current
			drArrResult = drParent.GetChildRows("Parent-Child",DataRowVersion.Original );
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW17");

			// GetParentRows_SD 3
			//Get Excepted result, in this case Current = Default
			drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows);
			//Get Result DataRowVersion.Current
			drArrResult = drParent.GetChildRows("Parent-Child",DataRowVersion.Default  );
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW18");

			// GetParentRows_SD 4
			drParent.BeginEdit();
			drParent["String1"] = "Value";
			//Get Excepted result
			drArrExcepted = dtChild.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows );
			//Get Result DataRowVersion.Current
			drArrResult = drParent.GetChildRows("Parent-Child",DataRowVersion.Proposed  );
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW19");
		}

		[Test] public void GetColumnError_ByIndex()
		{
			string sColErr = "Error!";
			DataTable dt = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Column1"); 
			dt.Columns.Add(dc);
			DataRow dr = dt.NewRow();

			// GetColumnError 1
			Assert.AreEqual(String.Empty ,  dr.GetColumnError(0) , "DRW20");

			dr.SetColumnError(0,sColErr );

			// GetColumnError 2
			Assert.AreEqual(sColErr ,  dr.GetColumnError(0) , "DRW21");
		}

		[Test] public void GetColumnError_ByName()
		{
			string sColErr = "Error!";
			DataTable dt = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Column1"); 
			dt.Columns.Add(dc);
			DataRow dr = dt.NewRow();

			// GetColumnError 1
			Assert.AreEqual(String.Empty ,  dr.GetColumnError("Column1") , "DRW22");

			dr.SetColumnError("Column1",sColErr );

			// GetColumnError 2
			Assert.AreEqual(sColErr ,  dr.GetColumnError("Column1") , "DRW23");
		}

		[Test] public void GetColumnsInError()
		{
			string sColErr = "Error!";
			DataColumn[] dcArr;
			DataTable dt = new DataTable("myTable"); 
			//init some columns
			dt.Columns.Add(new DataColumn());
			dt.Columns.Add(new DataColumn());
			dt.Columns.Add(new DataColumn());
			dt.Columns.Add(new DataColumn());
			dt.Columns.Add(new DataColumn());

			//init some rows
			dt.Rows.Add(new object[] {});
			dt.Rows.Add(new object[] {});
			dt.Rows.Add(new object[] {});

			DataRow dr = dt.Rows[1];

			dcArr = dr.GetColumnsInError();

			// GetColumnsInError 1
			Assert.AreEqual(0,  dcArr.Length , "DRW24");

			dr.SetColumnError(0,sColErr);
			dr.SetColumnError(2,sColErr);
			dr.SetColumnError(4,sColErr);

			dcArr = dr.GetColumnsInError();

			// GetColumnsInError 2
			Assert.AreEqual(3, dcArr.Length , "DRW25");

			//check that the right columns taken
			// GetColumnsInError 3
			Assert.AreEqual(dt.Columns[0], dcArr[0], "DRW26");

			// GetColumnsInError 4
			Assert.AreEqual(dt.Columns[2], dcArr[1], "DRW27");

			// GetColumnsInError 5
			Assert.AreEqual(dt.Columns[4], dcArr[2], "DRW28");
		}

		[Test] public new void GetHashCode()
		{
			int iHashCode;
			DataRow dr;
			DataTable dt = new DataTable();
			dr = dt.NewRow();

			iHashCode = dr.GetHashCode();
			for (int i=0; i<10; i++)
			{	//must return the same value each time
				// GetHashCode #" + i
				Assert.AreEqual(dr.GetHashCode() ,  iHashCode , "DRW29");
			}
		}

		[Test] public void GetParentRow_ByDataRelation()
		{
			DataRow drExcepted,drResult,drChild;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();

			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent = DataProvider.CreateParentDataTable(); 

			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);

			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);

			//Excepted result
			drExcepted = dtParent.Rows[0];

			//Get Result
			drChild = dtChild.Select("ParentId=" + drExcepted["ParentId"])[0]; 
			drResult = drChild.GetParentRow(dRel);

			// GetParentRow_D
			Assert.AreEqual(drExcepted.ItemArray,  drResult.ItemArray , "DRW30");
		}

		[Test] public void GetParentRow_ByDataRelationDataRowVersion()
		{
			DataRow drParent,drChild;
			DataRow drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);

			drParent = dtParent.Rows[0];
			drChild = dtChild.Select("ParentId=" + drParent["ParentId"])[0];

			// GetParentRow_DD 1
			//Get Excepted result
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow(dRel,DataRowVersion.Current);
			Assert.AreEqual(drArrExcepted.ItemArray,  drArrResult.ItemArray , "DRW31");

			// GetParentRow_DD 2
			//Get Excepted result
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow(dRel,DataRowVersion.Original );
			Assert.AreEqual(drArrExcepted.ItemArray,  drArrResult.ItemArray , "DRW32");

			// GetParentRow_DD 3
			//Get Excepted result, in this case Current = Default
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow(dRel,DataRowVersion.Default  );
			Assert.AreEqual(drArrExcepted.ItemArray,  drArrResult.ItemArray , "DRW33");

			// GetParentRow_DD 4
			drChild.BeginEdit();
			drChild["String1"] = "Value";
			//Get Excepted result
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow(dRel,DataRowVersion.Proposed  );
			Assert.AreEqual(drArrExcepted.ItemArray,  drArrResult.ItemArray , "DRW34");
		}

		[Test] public void GetParentRow_ByName()
		{
			DataRow drExcepted,drResult,drChild;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();

			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent = DataProvider.CreateParentDataTable(); 

			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);

			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);

			//Excepted result
			drExcepted = dtParent.Rows[0];

			//Get Result
			drChild = dtChild.Select("ParentId=" + drExcepted["ParentId"])[0]; 
			drResult = drChild.GetParentRow("Parent-Child");

			// GetParentRow_S
			Assert.AreEqual(drExcepted.ItemArray,  drResult.ItemArray , "DRW35");
		}

		[Test] public void GetParentRow_ByNameDataRowVersion()
		{
			DataRow drParent,drChild;
			DataRow drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);

			drParent = dtParent.Rows[0];
			drChild = dtChild.Select("ParentId=" + drParent["ParentId"])[0];

			// GetParentRow_SD 1
			//Get Excepted result
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow("Parent-Child",DataRowVersion.Current);
			Assert.AreEqual(drArrExcepted.ItemArray,  drArrResult.ItemArray , "DRW36");

			// GetParentRow_SD 2
			//Get Excepted result
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow("Parent-Child",DataRowVersion.Original );
			Assert.AreEqual(drArrExcepted.ItemArray,  drArrResult.ItemArray , "DRW37");

			// GetParentRow_SD 3
			//Get Excepted result, in this case Current = Default
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow("Parent-Child",DataRowVersion.Default  );
			Assert.AreEqual(drArrExcepted.ItemArray,  drArrResult.ItemArray , "DRW38");

			// GetParentRow_SD 4
			drChild.BeginEdit();
			drChild["String1"] = "Value";
			//Get Excepted result
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow("Parent-Child",DataRowVersion.Proposed  );
			Assert.AreEqual(drArrExcepted.ItemArray,  drArrResult.ItemArray , "DRW39");
		}

		[Test] public void GetParentRows_ByDataRelation()
		{
			DataRow dr;
			DataRow[] drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();

			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent = DataProvider.CreateParentDataTable(); 

			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			dr = dtParent.Rows[0];

			//Duplicate several rows in order to create Many to Many relation
			dtParent.ImportRow(dr); 
			dtParent.ImportRow(dr); 
			dtParent.ImportRow(dr); 

			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"],false);
			ds.Relations.Add(dRel);
			//Get Excepted result
			drArrExcepted = dtParent.Select("ParentId=" + dr["ParentId"]);
			dr = dtChild.Select("ParentId=" + dr["ParentId"])[0];
			//Get Result
			drArrResult = dr.GetParentRows(dRel);

			// GetParentRows_D
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW40");
		}

		[Test] public void GetParentRows_ByName()
		{
			DataRow dr;
			DataRow[] drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();

			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent = DataProvider.CreateParentDataTable(); 

			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			dr = dtParent.Rows[0];

			//Duplicate several rows in order to create Many to Many relation
			dtParent.ImportRow(dr); 
			dtParent.ImportRow(dr); 
			dtParent.ImportRow(dr); 

			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"],false);
			ds.Relations.Add(dRel);
			//Get Excepted result
			drArrExcepted = dtParent.Select("ParentId=" + dr["ParentId"]);
			dr = dtChild.Select("ParentId=" + dr["ParentId"])[0];
			//Get Result
			drArrResult = dr.GetParentRows("Parent-Child");

			// GetParentRows_S
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW41");
		}

		[Test] public void GetParentRows_ByNameDataRowVersion()
		{
			DataRow drParent,drChild;
			DataRow[] drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"],false);
			ds.Relations.Add(dRel);

			//Create several copies of the first row
			drParent = dtParent.Rows[0];	//row[0] has versions: Default,Current,Original
			dtParent.ImportRow(drParent);	//row[1] has versions: Default,Current,Original
			dtParent.ImportRow(drParent);	//row[2] has versions: Default,Current,Original
			dtParent.ImportRow(drParent);	//row[3] has versions: Default,Current,Original
			dtParent.ImportRow(drParent);	//row[4] has versions: Default,Current,Original
			dtParent.ImportRow(drParent);	//row[5] has versions: Default,Current,Original
			dtParent.AcceptChanges();

			//Get the first child row for drParent
			drChild = dtChild.Select("ParentId=" + drParent["ParentId"])[0];

			DataRow[] drTemp = dtParent.Select("ParentId=" + drParent["ParentId"]);
			//				Console.WriteLine("********");
			//				foreach (DataRow d in drTemp)
			//				{
			//					CheckRowVersion(d);
			//				}
			drTemp[0].BeginEdit();
			drTemp[0]["String1"] = "NewValue"; //row now has versions: Proposed,Current,Original,Default
			drTemp[1].BeginEdit();
			drTemp[1]["String1"] = "NewValue"; //row now has versions: Proposed,Current,Original,Default

			//		Console.WriteLine("********");
			//		foreach (DataRow d in drTemp)
			//		{
			//			CheckRowVersion(d);
			//		}
			//		Console.WriteLine("********");

			// Check DataRowVersion.Current
			//Check DataRowVersion.Current 
			drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows );
			drArrResult = drChild.GetParentRows("Parent-Child",DataRowVersion.Current);
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW42");

			//Check DataRowVersion.Current 
			// Teting: DataRow.GetParentRows_D_D ,DataRowVersion.Original
			drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.OriginalRows );
			drArrResult = drChild.GetParentRows("Parent-Child",DataRowVersion.Original );
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW43");

			//Check DataRowVersion.Default
			// Teting: DataRow.GetParentRows_D_D ,DataRowVersion.Default
			drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows);
			drArrResult = drChild.GetParentRows("Parent-Child",DataRowVersion.Default  );
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW44");

		/* .Net don't work as expected
			//Check DataRowVersion.Proposed
			// Teting: DataRow.GetParentRows_D_D ,DataRowVersion.Proposed
			drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.ModifiedCurrent);
			//drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.ModifiedOriginal );

			drArrResult = drChild.GetParentRows("Parent-Child",DataRowVersion.Proposed  );
			Assert.AreEqual(drArrExcepted,  drArrResult, "DRW45");
		*/		
		}

		private void CheckRowVersion(DataRow dr)
		{
			Console.WriteLine("");
			if (dr.HasVersion(DataRowVersion.Current)) Console.WriteLine("Has " + DataRowVersion.Current.ToString());
			if (dr.HasVersion(DataRowVersion.Default)) Console.WriteLine("Has " + DataRowVersion.Default.ToString());
			if (dr.HasVersion(DataRowVersion.Original)) Console.WriteLine("Has " + DataRowVersion.Original.ToString());
			if (dr.HasVersion(DataRowVersion.Proposed)) Console.WriteLine("Has " + DataRowVersion.Proposed.ToString());
		}

		[Test] public new void GetType()
		{
			Type myType;	
			DataTable dt = new DataTable(); 
			DataRow dr = dt.NewRow();
			myType = typeof(DataRow);

			// GetType
			Assert.AreEqual(typeof(DataRow), myType , "DRW46");
		}

		[Test] public void HasErrors()
		{
			DataTable dt = new DataTable("myTable"); 
			DataRow dr = dt.NewRow();

			// HasErrors (default)
			Assert.AreEqual(false, dr.HasErrors, "DRW47");

			dr.RowError = "Err";

			// HasErrors (set/get)
			Assert.AreEqual(true , dr.HasErrors , "DRW48");
		}

		[Test] public void HasVersion_ByDataRowVersion()
		{
			DataTable t = new DataTable("atable");
			t.Columns.Add("id", typeof(int));
			t.Columns.Add("name", typeof(string));
			t.Columns[0].DefaultValue = 1;
			t.Columns[1].DefaultValue = "something";

			// row r is detached
			DataRow r = t.NewRow();

			// HasVersion Test #10
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Current) , "DRW49");

			// HasVersion Test #11
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Original) , "DRW50");

			// HasVersion Test #12
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Default) , "DRW51");

			// HasVersion Test #13
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Proposed) , "DRW52");

			r[0] = 4; 
			r[1] = "four";

			// HasVersion Test #20
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Current) , "DRW53");

			// HasVersion Test #21
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Original) , "DRW54");

			// HasVersion Test #22
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Default) , "DRW55");

			// HasVersion Test #23
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Proposed) , "DRW56");

			t.Rows.Add(r);
			// now it is "added"

			// HasVersion Test #30
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Current) , "DRW57");

			// HasVersion Test #31
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Original) , "DRW58");

			// HasVersion Test #32
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Default) , "DRW59");

			// HasVersion Test #33
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Proposed) , "DRW60");

			t.AcceptChanges();
			// now it is "unchanged"

			// HasVersion Test #40
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Current) , "DRW61");

			// HasVersion Test #41
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Original) , "DRW62");

			// HasVersion Test #42
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Default) , "DRW63");

			// HasVersion Test #43
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Proposed) , "DRW64");

			r.BeginEdit();
			r[1] = "newvalue";

			// HasVersion Test #50
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Current) , "DRW65");

			// HasVersion Test #51
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Original) , "DRW66");

			// HasVersion Test #52
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Default) , "DRW67");

			// HasVersion Test #53
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Proposed) , "DRW68");

			r.EndEdit();
			// now it is "modified"
			// HasVersion Test #60
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Current) , "DRW69");

			// HasVersion Test #61
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Original) , "DRW70");

			// HasVersion Test #62
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Default) , "DRW71");

			// HasVersion Test #63
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Proposed) , "DRW72");

			// this or t.AcceptChanges
			r.AcceptChanges(); 
			// now it is "unchanged" again
			// HasVersion Test #70
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Current) , "DRW73");

			// HasVersion Test #71
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Original) , "DRW74");

			// HasVersion Test #72
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Default) , "DRW75");

			// HasVersion Test #73
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Proposed) , "DRW76");

			r.Delete();
			// now it is "deleted"

			// HasVersion Test #80
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Current) , "DRW77");

			// HasVersion Test #81
			Assert.AreEqual(true , r.HasVersion(DataRowVersion.Original) , "DRW78");

			// HasVersion Test #82
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Default) , "DRW79");

			// HasVersion Test #83
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Proposed) , "DRW80");

			r.AcceptChanges();
			// back to detached
			// HasVersion Test #90
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Current) , "DRW81");

			// HasVersion Test #91
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Original) , "DRW82");

			// HasVersion Test #92
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Default) , "DRW83");

			// HasVersion Test #93
			Assert.AreEqual(false , r.HasVersion(DataRowVersion.Proposed) , "DRW84");
		}

		[Test] // Object this [DataColumn]
		public void Indexer1 ()
		{
			DataTable dt = new DataTable ();
			DataColumn dc0 = new DataColumn ("Col0", typeof (Address));
			dt.Columns.Add (dc0);
			DataColumn dc1 = new DataColumn ("Col1", typeof (Person));
			dt.Columns.Add (dc1);

			Person personA = new Person ("Miguel");
			Address addressA = new Address ("X", 5);
			Person personB = new Person ("Chris");
			Address addressB = new Address ("Y", 4);
			Person personC = new Person ("Jackson");
			Address addressC = new Address ("Z", 3);

			dt.Rows.Add (new object [] { addressA, personA });
			dt.Rows.Add (new object [] { addressB, personB });
			DataRow dr;

			dr = dt.Rows [0];
			Assert.AreEqual (addressA, dr [dc0], "#A1");
			Assert.AreSame (personA, dr [dc1], "#A2");

			dr = dt.Rows [1];
			Assert.AreEqual (addressB, dr [dc0], "#B1");
			Assert.AreSame (personB, dr [dc1], "#B2");

			dr = dt.Rows [0];
			dr [dc0] = addressC;
			Assert.AreEqual (addressC, dr [dc0], "#C1");
			Assert.AreSame (personA, dr [dc1], "#C2");

			dr = dt.Rows [1];
			dr [dc1] = personC;
			Assert.AreEqual (addressB, dr [dc0], "#D1");
			Assert.AreSame (personC, dr [dc1], "#D2");
		}

		[Test] // Object this [DataColumn]
		public void Indexer1_Column_Null ()
		{
			DataTable dt = new DataTable ();
			DataColumn dc0 = new DataColumn ("Col0", typeof (Address));
			dt.Columns.Add (dc0);
			DataColumn dc1 = new DataColumn ("Col1", typeof (Person));
			dt.Columns.Add (dc1);

			Person personA = new Person ("Miguel");
			Address addressA = new Address ("X", 5);
			Person personB = new Person ("Chris");

			dt.Rows.Add (new object [] { addressA, personA });
			DataRow dr = dt.Rows [0];

			try {
				object value = dr [(DataColumn) null];
				Assert.Fail ("#A1:" + value);
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("column", ex.ParamName, "#A5");
			}

			try {
				dr [(DataColumn) null] = personB;
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("column", ex.ParamName, "#B5");
			}
		}

		[Test] // Object this [DataColumn]
		[NUnit.Framework.Category ("NotWorking")]
		public void Indexer1_Value_Null ()
		{
			DataTable dt = new DataTable ();
			DataColumn dc0 = new DataColumn ("Col0", typeof (Address));
			dt.Columns.Add (dc0);
			DataColumn dc1 = new DataColumn ("Col1", typeof (Person));
			dt.Columns.Add (dc1);

			Person personA = new Person ("Miguel");
			Address addressA = new Address ("X", 5);
			Person personB = new Person ("Chris");
			Address addressB = new Address ("Y", 4);
			Person personC = new Person ("Jackson");
			Address addressC = new Address ("Z", 3);

			dt.Rows.Add (new object [] { addressA, personA });
			dt.Rows.Add (new object [] { addressB, personB });

			DataRow dr = dt.Rows [0];

			try {
				dr [dc0] = null;
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Cannot set Column 'Col0' to be null.
				// Please use DBNull instead
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("Col0") != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("DBNull") != -1, "#A6");
			}

			Assert.AreEqual (addressA, dr [dc0], "#B1");
			Assert.IsFalse (dr.IsNull (dc0), "#B2");
			Assert.AreSame (personA, dr [dc1], "#B3");
			Assert.IsFalse (dr.IsNull (dc1), "#B4");

#if NET_2_0
			dr [dc1] = null;
#else
			try {
				dr [dc1] = null;
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Cannot set Column 'Col1' to be null.
				// Please use DBNull instead
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("Col1") != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("DBNull") != -1, "#B6");
			}
			dr [dc1] = DBNull.Value;
#endif

			Assert.AreEqual (addressA, dr [dc0], "#C1");
			Assert.IsFalse (dr.IsNull (dc0), "#C2");
			Assert.AreSame (DBNull.Value, dr [dc1], "#C3");
			Assert.IsTrue (dr.IsNull (dc1), "#C4");

			dr [dc0] = DBNull.Value;

			Assert.AreSame (DBNull.Value, dr [dc0], "#D1");
			Assert.IsTrue (dr.IsNull (dc0), "#D2");
			Assert.AreSame (DBNull.Value, dr [dc1], "#D3");
			Assert.IsTrue (dr.IsNull (dc1), "#D4");

			dr [dc1] = personC;
			Assert.AreSame (DBNull.Value, dr [dc0], "#E1");
			Assert.IsTrue (dr.IsNull (dc0), "#E2");
			Assert.AreEqual (personC, dr [dc1], "#E3");
			Assert.IsFalse (dr.IsNull (dc1), "#E4");

			dr [dc1] = DBNull.Value;

			Assert.AreSame (DBNull.Value, dr [dc0], "#F1");
			Assert.IsTrue (dr.IsNull (dc0), "#F2");
			Assert.AreSame (DBNull.Value, dr [dc1], "#F3");
			Assert.IsTrue (dr.IsNull (dc1), "#F4");
		}

		[Test] // Object this [Int32]
		public void Indexer2 ()
		{
			DataTable dt = new DataTable ();
			DataColumn dc0 = new DataColumn ("Col0", typeof (Address));
			dt.Columns.Add (dc0);
			DataColumn dc1 = new DataColumn ("Col1", typeof (Person));
			dt.Columns.Add (dc1);

			Person personA = new Person ("Miguel");
			Address addressA = new Address ("X", 5);
			Person personB = new Person ("Chris");
			Address addressB = new Address ("Y", 4);
			Person personC = new Person ("Jackson");
			Address addressC = new Address ("Z", 3);

			dt.Rows.Add (new object [] { addressA, personA });
			dt.Rows.Add (new object [] { addressB, personB });
			DataRow dr;
			
			dr = dt.Rows [0];
			Assert.AreEqual (addressA, dr [0], "#A1");
			Assert.AreSame (personA, dr [1], "#A2");

			dr = dt.Rows [1];
			Assert.AreEqual (addressB, dr [0], "#B1");
			Assert.AreSame (personB, dr [1], "#B2");

			dr = dt.Rows [0];
			dr [0] = addressC;
			Assert.AreEqual (addressC, dr [0], "#C1");
			Assert.AreSame (personA, dr [1], "#C2");

			dr = dt.Rows [1];
			dr [1] = personC;
			Assert.AreEqual (addressB, dr [0], "#D1");
			Assert.AreSame (personC, dr [1], "#D2");
		}

		[Test] // Object this [Int32]
		[NUnit.Framework.Category ("NotWorking")]
		public void Indexer2_Value_Null ()
		{
			DataTable dt = new DataTable ();
			DataColumn dc0 = new DataColumn ("Col0", typeof (Address));
			dt.Columns.Add (dc0);
			DataColumn dc1 = new DataColumn ("Col1", typeof (Person));
			dt.Columns.Add (dc1);

			Person personA = new Person ("Miguel");
			Address addressA = new Address ("X", 5);
			Person personB = new Person ("Chris");
			Address addressB = new Address ("Y", 4);
			Person personC = new Person ("Jackson");
			Address addressC = new Address ("Z", 3);

			dt.Rows.Add (new object [] { addressA, personA });
			dt.Rows.Add (new object [] { addressB, personB });

			DataRow dr = dt.Rows [0];

			try {
				dr [0] = null;
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Cannot set Column 'Col0' to be null.
				// Please use DBNull instead
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("Col0") != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("DBNull") != -1, "#A6");
			}

			Assert.AreEqual (addressA, dr [0], "#B1");
			Assert.IsFalse (dr.IsNull (0), "#B2");
			Assert.AreSame (personA, dr [1], "#B3");
			Assert.IsFalse (dr.IsNull (1), "#B4");

#if NET_2_0
			dr [1] = null;
#else
			try {
				dr [1] = null;
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Cannot set Column 'Col1' to be null.
				// Please use DBNull instead
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("Col1") != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("DBNull") != -1, "#B6");
			}
			dr [1] = DBNull.Value;
#endif

			Assert.AreEqual (addressA, dr [0], "#C1");
			Assert.IsFalse (dr.IsNull (0), "#C2");
			Assert.AreSame (DBNull.Value, dr [1], "#C3");
			Assert.IsTrue (dr.IsNull (1), "#C4");

			dr [0] = DBNull.Value;

			Assert.AreSame (DBNull.Value, dr [0], "#D1");
			Assert.IsTrue (dr.IsNull (0), "#D2");
			Assert.AreSame (DBNull.Value, dr [1], "#D3");
			Assert.IsTrue (dr.IsNull (1), "#D4");

			dr [1] = personC;
			Assert.AreSame (DBNull.Value, dr [0], "#E1");
			Assert.IsTrue (dr.IsNull (0), "#E2");
			Assert.AreEqual (personC, dr [1], "#E3");
			Assert.IsFalse (dr.IsNull (1), "#E4");

			dr [1] = DBNull.Value;

			Assert.AreSame (DBNull.Value, dr [0], "#F1");
			Assert.IsTrue (dr.IsNull (0), "#F2");
			Assert.AreSame (DBNull.Value, dr [1], "#F3");
			Assert.IsTrue (dr.IsNull (1), "#F4");
		}

		[Test] // Object this [String]
		public void Indexer3 ()
		{
			DataTable dt = new DataTable ();
			DataColumn dc0 = new DataColumn ("Col0", typeof (Address));
			dt.Columns.Add (dc0);
			DataColumn dc1 = new DataColumn ("Col1", typeof (Person));
			dt.Columns.Add (dc1);

			Person personA = new Person ("Miguel");
			Address addressA = new Address ("X", 5);
			Person personB = new Person ("Chris");
			Address addressB = new Address ("Y", 4);
			Person personC = new Person ("Jackson");
			Address addressC = new Address ("Z", 3);

			dt.Rows.Add (new object [] { addressA, personA });
			dt.Rows.Add (new object [] { addressB, personB });
			DataRow dr;

			dr = dt.Rows [0];
			Assert.AreEqual (addressA, dr ["Col0"], "#A1");
			Assert.AreSame (personA, dr ["Col1"], "#A2");

			dr = dt.Rows [1];
			Assert.AreEqual (addressB, dr ["Col0"], "#B1");
			Assert.AreSame (personB, dr ["Col1"], "#B2");

			dr = dt.Rows [0];
			dr ["Col0"] = addressC;
			Assert.AreEqual (addressC, dr ["Col0"], "#C1");
			Assert.AreSame (personA, dr ["Col1"], "#C2");

			dr = dt.Rows [1];
			dr ["Col1"] = personC;
			Assert.AreEqual (addressB, dr ["Col0"], "#D1");
			Assert.AreSame (personC, dr ["Col1"], "#D2");
		}

		[Test] // Object this [String]
		public void Indexer3_ColumnName_Empty ()
		{
			DataTable dt = new DataTable ("Persons");
			DataColumn dc0 = new DataColumn ("Col0", typeof (Address));
			dt.Columns.Add (dc0);
			DataColumn dc1 = new DataColumn (string.Empty, typeof (Person));
			dt.Columns.Add (dc1);

			Person personA = new Person ("Miguel");
			Address addressA = new Address ("X", 5);
			Person personB = new Person ("Chris");

			dt.Rows.Add (new object [] { addressA, personA });

			DataRow dr = dt.Rows [0];

			try {
				object value = dr [string.Empty];
				Assert.Fail ("#A1:" + value);
			} catch (ArgumentException ex) {
				//  Column '' does not belong to table Persons
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("''") != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("Persons") != -1, "#A6");
				Assert.IsNull (ex.ParamName, "#A7");
			}

			try {
				dr [string.Empty] = personB;
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Column '' does not belong to table Persons
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("''") != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("Persons") != -1, "#B6");
				Assert.IsNull (ex.ParamName, "#B7");
			}
		}

		[Test] // Object this [String]
		[NUnit.Framework.Category ("NotWorking")]
		public void Indexer3_ColumnName_Null ()
		{
			DataTable dt = new DataTable ();
			DataColumn dc0 = new DataColumn ("Col0", typeof (Address));
			dt.Columns.Add (dc0);
			DataColumn dc1 = new DataColumn ("Col1", typeof (Person));
			dt.Columns.Add (dc1);

			Person personA = new Person ("Miguel");
			Address addressA = new Address ("X", 5);
			Person personB = new Person ("Chris");

			dt.Rows.Add (new object [] { addressA, personA });

			DataRow dr = dt.Rows [0];

			try {
				object value = dr [(string) null];
				Assert.Fail ("#A1:" + value);
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
#if NET_2_0
				Assert.AreEqual ("name", ex.ParamName, "#A5");
#else
				Assert.AreEqual ("key", ex.ParamName, "#A5");
#endif
				throw;
			}

			try {
				dr [(string) null] = personB;
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
#if NET_2_0
				Assert.AreEqual ("name", ex.ParamName, "#B5");
#else
				Assert.AreEqual ("key", ex.ParamName, "#B5");
#endif
			}
		}

		[Test] // Object this [String]
		[NUnit.Framework.Category ("NotWorking")]
		public void Indexer3_Value_Null ()
		{
			DataTable dt = new DataTable ();
			DataColumn dc0 = new DataColumn ("Col0", typeof (Address));
			dt.Columns.Add (dc0);
			DataColumn dc1 = new DataColumn ("Col1", typeof (Person));
			dt.Columns.Add (dc1);

			Person personA = new Person ("Miguel");
			Address addressA = new Address ("X", 5);
			Person personB = new Person ("Chris");
			Address addressB = new Address ("Y", 4);
			Person personC = new Person ("Jackson");
			Address addressC = new Address ("Z", 3);

			dt.Rows.Add (new object [] { addressA, personA });
			dt.Rows.Add (new object [] { addressB, personB });

			DataRow dr = dt.Rows [0];

			try {
				dr ["Col0"] = null;
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Cannot set Column 'Col0' to be null.
				// Please use DBNull instead
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("Col0") != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("DBNull") != -1, "#A6");
			}

			Assert.AreEqual (addressA, dr ["Col0"], "#B1");
			Assert.IsFalse (dr.IsNull ("Col0"), "#B2");
			Assert.AreSame (personA, dr ["Col1"], "#B3");
			Assert.IsFalse (dr.IsNull ("Col1"), "#B4");

#if NET_2_0
			dr ["Col1"] = null;
#else
			try {
				dr ["Col1"] = null;
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Cannot set Column 'Col1' to be null.
				// Please use DBNull instead
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("Col1") != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("DBNull") != -1, "#B6");
			}
			dr ["Col1"] = DBNull.Value;
#endif

			Assert.AreEqual (addressA, dr ["Col0"], "#C1");
			Assert.IsFalse (dr.IsNull ("Col0"), "#C2");
			Assert.AreSame (DBNull.Value, dr ["Col1"], "#C3");
			Assert.IsTrue (dr.IsNull ("Col1"), "#C4");

			dr ["Col0"] = DBNull.Value;

			Assert.AreSame (DBNull.Value, dr ["Col0"], "#D1");
			Assert.IsTrue (dr.IsNull ("Col0"), "#D2");
			Assert.AreSame (DBNull.Value, dr ["Col1"], "#D3");
			Assert.IsTrue (dr.IsNull ("Col1"), "#D4");

			dr ["Col1"] = personC;
			Assert.AreSame (DBNull.Value, dr ["Col0"], "#E1");
			Assert.IsTrue (dr.IsNull ("Col0"), "#E2");
			Assert.AreEqual (personC, dr ["Col1"], "#E3");
			Assert.IsFalse (dr.IsNull ("Col1"), "#E4");

			dr ["Col1"] = DBNull.Value;

			Assert.AreSame (DBNull.Value, dr ["Col0"], "#F1");
			Assert.IsTrue (dr.IsNull ("Col0"), "#F2");
			Assert.AreSame (DBNull.Value, dr ["Col1"], "#F3");
			Assert.IsTrue (dr.IsNull ("Col1"), "#F4");
		}

		[Test] public void IsNull_ByDataColumn()
		{
			DataTable dt = new DataTable(); 
			DataColumn dc0 = new DataColumn("Col0",typeof(int));
			DataColumn dc1 = new DataColumn("Col1",typeof(int));
			dt.Columns.Add(dc0);
			dt.Columns.Add(dc1);
			dt.Rows.Add(new object[] {1234});
			DataRow dr = dt.Rows[0];

			// IsNull_I 2
			Assert.AreEqual(false , dr.IsNull(dc0) , "DRW85");

			// IsNull_I 2
			Assert.AreEqual(true , dr.IsNull(dc1) , "DRW86");
		}

		[Test] public void IsNull_ByDataColumnDataRowVersion()
		{
			DataTable dt = new DataTable(); 
			DataColumn dc0 = new DataColumn("Col0",typeof(int));
			DataColumn dc1 = new DataColumn("Col1",typeof(int));
			dt.Columns.Add(dc0);
			dt.Columns.Add(dc1);
			dt.Rows.Add(new object[] {1234});
			DataRow dr = dt.Rows[0];

			// IsNull - col0 Current
			Assert.AreEqual(false, dr.IsNull(dc0,DataRowVersion.Current) , "DRW87");

			// IsNull - col1 Current
			Assert.AreEqual(true, dr.IsNull(dc1,DataRowVersion.Current) , "DRW88");

			// IsNull - col0 Default
			Assert.AreEqual(false, dr.IsNull(dc0,DataRowVersion.Default) , "DRW89");
			// IsNull - col1 Default
			Assert.AreEqual(true, dr.IsNull(dc1,DataRowVersion.Default) , "DRW90");

			dr.BeginEdit();
			dr[0] = 9; //Change value, Create RowVersion Proposed

			// IsNull - col0 Proposed
			Assert.AreEqual(false, dr.IsNull(dc0,DataRowVersion.Proposed) , "DRW91");
			// IsNull - col1 Proposed
			Assert.AreEqual(true, dr.IsNull(dc1,DataRowVersion.Proposed) , "DRW92");

			dr.AcceptChanges();
			dr.Delete();

			// IsNull - col0 Original
			Assert.AreEqual(false, dr.IsNull(dc0,DataRowVersion.Original) , "DRW93");
		}

		[Test] public void IsNull_ByIndex()
		{
			DataTable dt = new DataTable(); 
			DataColumn dc0 = new DataColumn("Col0",typeof(int));
			DataColumn dc1 = new DataColumn("Col1",typeof(int));
			dt.Columns.Add(dc0);
			dt.Columns.Add(dc1);
			dt.Rows.Add(new object[] {1234});
			DataRow dr = dt.Rows[0];

			// IsNull_I 2
			Assert.AreEqual(false , dr.IsNull(0) , "DRW94");

			// IsNull_I 2
			Assert.AreEqual(true , dr.IsNull(1) , "DRW95");
		}

		[Test] public void IsNull_ByName()
		{
			DataTable dt = new DataTable(); 
			DataColumn dc0 = new DataColumn("Col0",typeof(int));
			DataColumn dc1 = new DataColumn("Col1",typeof(int));
			dt.Columns.Add(dc0);
			dt.Columns.Add(dc1);
			dt.Rows.Add(new object[] {1234});
			DataRow dr = dt.Rows[0];

#region --- assignment  ----
			// IsNull_S 1
			Assert.AreEqual(false, dr.IsNull("Col0"), "DRW96");

			// IsNull_S 2
			Assert.AreEqual(true, dr.IsNull("Col1"), "DRW97");
#endregion

#region --- bug 3124 ---

			// IsNull_S 1
			MemoryStream st = new MemoryStream();
			StreamWriter sw = new StreamWriter(st);
			sw.Write("<?xml version=\"1.0\" standalone=\"yes\"?><NewDataSet>");
			sw.Write("<Table><EmployeeNo>9</EmployeeNo></Table>");
			sw.Write("</NewDataSet>");
			sw.Flush();
			st.Position=0;
			DataSet ds = new DataSet();
			ds.ReadXml(st);
			//  Here we add the expression column
			ds.Tables[0].Columns.Add("ValueListValueMember", typeof(object), "EmployeeNo");

			foreach( DataRow row in ds.Tables[0].Rows )
			{
				// Console.WriteLine(row["ValueListValueMember"].ToString() + " " );
				if( row.IsNull("ValueListValueMember") == true )
					Assert.AreEqual("Failed", "SubTest", "DRW98");
				else
					Assert.AreEqual("Passed", "Passed", "DRW99");
			}

#endregion
		}

		[Test] public void Item()
		{
			// init table with columns
			DataTable myTable = new DataTable("myTable"); 

			myTable.Columns.Add(new DataColumn("Id",typeof(int)));
			myTable.Columns.Add(new DataColumn("Name",typeof(string)));
			DataColumn dc = myTable.Columns[0];

			myTable.Rows.Add(new object[] {1,"Ofer"});
			myTable.Rows.Add(new object[] {2,"Ofer"});

			myTable.AcceptChanges();

			DataRow myRow = myTable.Rows[0];

			//Start checking

			// Item - index
			Assert.AreEqual(1 , (int)myRow[0] , "DRW100");

			// Item - string
			Assert.AreEqual(1 ,  (int)myRow["Id"] , "DRW101");

			// Item - Column
			Assert.AreEqual(1 ,  (int)myRow[dc] , "DRW102");

			// Item - index,Current
			Assert.AreEqual(1 ,  (int)myRow[0,DataRowVersion.Current ] , "DRW103");

			// Item - string,Current
			Assert.AreEqual(1 ,  (int)myRow["Id",DataRowVersion.Current] , "DRW104");

			// Item - columnn,Current
			Assert.AreEqual(1 ,  (int)myRow[dc,DataRowVersion.Current] , "DRW105");

			//	testMore();
		}

		/*public void testMore()
		{
			DataTable dt = DataProvider.CreateParentDataTable();
			dt.Rows[0].BeginEdit();
			dt.Rows[0][0] = 10;
			dt.Rows[0].EndEdit();
			dt.AcceptChanges();
		}*/

		[Test] public void RejectChanges()
		{
			DataTable dt = new DataTable(); 
			DataColumn dc0 = new DataColumn("Col0",typeof(int));
			DataColumn dc1 = new DataColumn("Col1",typeof(int));
			dt.Columns.Add(dc0);
			dt.Columns.Add(dc1);
			dt.Rows.Add(new object[] {1234});
			dt.AcceptChanges();
			DataRow dr = dt.Rows[0];

			dr[0] = 567;
			dr[1] = 789;
			dr.RejectChanges();

			// RejectChanges - row 0
			Assert.AreEqual(1234  ,  (int)dr[0], "DRW106");

			// RejectChanges - row 1
			Assert.AreEqual(DBNull.Value  ,  dr[1] , "DRW107");

			dr.Delete();
			dr.RejectChanges();

			// RejectChanges - count
			Assert.AreEqual(1 ,  dt.Rows.Count , "DRW108");
		}

		[Test] public void RowState()
		{
			DataTable myTable = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Name",typeof(string));
			myTable.Columns.Add(dc);
			DataRow myRow;

			// Create a new DataRow.
			myRow = myTable.NewRow();

			// Detached row.

			// Detached
			Assert.AreEqual(DataRowState.Detached ,  myRow.RowState , "DRW109");

			myTable.Rows.Add(myRow);
			// New row.

			// Added
			Assert.AreEqual(DataRowState.Added ,  myRow.RowState , "DRW110");

			myTable.AcceptChanges();
			// Unchanged row.

			// Unchanged
			Assert.AreEqual(DataRowState.Unchanged ,  myRow.RowState , "DRW111");

			myRow["Name"] = "Scott";
			// Modified row.

			// Modified
			Assert.AreEqual(DataRowState.Modified ,  myRow.RowState , "DRW112");

			myRow.Delete();
			// Deleted row.

			// Deleted
			Assert.AreEqual(DataRowState.Deleted ,  myRow.RowState , "DRW113");
		}

		[Test] public void SetColumnError_ByDataColumnError()
		{
			string sColErr = "Error!";
			DataTable dt = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Column1"); 
			dt.Columns.Add(dc);
			DataRow dr = dt.NewRow();

			// empty string
			Assert.AreEqual(String.Empty,  dr.GetColumnError(dc) , "DRW114");

			dr.SetColumnError(dc,sColErr );

			// error string
			Assert.AreEqual(sColErr, dr.GetColumnError(dc) , "DRW115");
		}

		[Test] public void SetColumnError_ByIndexError()
		{
			string sColErr = "Error!";
			DataTable dt = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Column1"); 
			dt.Columns.Add(dc);
			DataRow dr = dt.NewRow();

			// empty string
			Assert.AreEqual(String.Empty ,  dr.GetColumnError(0) , "DRW116");

			dr.SetColumnError(0,sColErr );

			// error string
			Assert.AreEqual(sColErr  ,  dr.GetColumnError(0) , "DRW117");
			dr.SetColumnError (0, "");
			Assert.AreEqual("",  dr.GetColumnError (0) , "DRW118");
		}

		[Test] public void SetColumnError_ByColumnNameError()
		{
			string sColErr = "Error!";
			DataTable dt = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Column1"); 
			dt.Columns.Add(dc);
			DataRow dr = dt.NewRow();

			// empty string
			Assert.AreEqual(String.Empty,  dr.GetColumnError("Column1") , "DRW118");

			dr.SetColumnError("Column1",sColErr );

			// error string
			Assert.AreEqual(sColErr,  dr.GetColumnError("Column1") , "DRW119");
		}

		[Test] public void SetParentRow_ByDataRow()
		{
			DataRow drParent,drChild;
			DataRow drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);

			drParent = dtParent.Rows[0];
			drChild = dtChild.Select("ParentId=" + drParent["ParentId"])[0];

			drChild.SetParentRow(drParent);

			//Get Excepted result
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow("Parent-Child",DataRowVersion.Current);

			// SetParentRow
			Assert.AreEqual(drArrExcepted ,  drArrResult, "DRW120");
		}

		[Test]
		public void testMore()
		{
			DataSet ds = DataProvider.CreateForigenConstraint();
			DataRow drParent = ds.Tables[0].Rows[0];
			//DataRow[] drArray =  ds.Tables[1].Rows[0].GetParentRows(ds.Tables[1].ParentRelations[0]);
			ds.Tables[1].Rows[0].SetParentRow(drParent);
		}

		[Test]
		public void test()
		{
			// test SetParentRow
			DataTable parent = DataProvider.CreateParentDataTable();
			DataTable child = DataProvider.CreateChildDataTable();
			DataRow dr = parent.Rows[0];
			dr.Delete();
			parent.AcceptChanges();

			child.Rows[0].SetParentRow(dr);
		}

		public void checkForLoops()
		{
			DataSet ds = new DataSet();
			//Create tables
			DataTable  dtChild = DataProvider.CreateChildDataTable();
			DataTable dtParent= DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);

			dtChild.Rows.Clear();
			dtParent.Rows.Clear();

			dtParent.ChildRelations.Add(dtParent.Columns[0],dtChild.Columns[0]);
			dtChild.ChildRelations.Add(dtChild.Columns[0],dtParent.Columns[0]);

			dtChild.Rows[0].SetParentRow(dtParent.Rows[0]);
			dtParent.Rows[0].SetParentRow(dtChild.Rows[0]);
		}

		public void checkForLoopsAdvenced()
		{
			//Create tables
			DataTable  dtChild = new DataTable();
			dtChild.Columns.Add("Col1",typeof(int));
			dtChild.Columns.Add("Col2",typeof(int));

			DataRelation drl = new DataRelation("drl1",dtChild.Columns[0],dtChild.Columns[1]);
			dtChild.ChildRelations.Add(drl);
			dtChild.Rows[0].SetParentRow(dtChild.Rows[1]);
			dtChild.Rows[1].SetParentRow(dtChild.Rows[0]);
		}

		[Test] public void SetParentRow_ByDataRowDataRelation()
		{
			DataRow drParent,drChild;
			DataRow drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);

			drParent = dtParent.Rows[0];
			drChild = dtChild.Select("ParentId=" + drParent["ParentId"])[0];

			drChild.SetParentRow(drParent ,dRel);

			//Get Excepted result
			drArrExcepted = drParent;
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRow("Parent-Child",DataRowVersion.Current);

			// SetParentRow
			Assert.AreEqual(drArrExcepted ,  drArrResult, "DRW121");
		}

		[Test] public void Table()
		{
			DataTable dt1,dt2;
			dt2 = new DataTable("myTable"); 
			DataRow dr = dt2.NewRow();
			dt1 = dr.Table;

			// ctor
			Assert.AreEqual(dt2, dt1 , "DRW122");
		}

		[Test] public new  void ToString()
		{
			DataRow dr;
			DataTable dtParent;
			dtParent= DataProvider.CreateParentDataTable(); 
			dr = dtParent.Rows[0];

			// ToString
			Assert.AreEqual(true, dr.ToString().ToLower().StartsWith("system.data.datarow") , "DRW123");
		}
		
		[Test] public void DataRow_RowError()
		{
			DataTable dt = new DataTable ("myTable"); 
			DataRow dr = dt.NewRow ();
	
			Assert.AreEqual ( dr.RowError, string.Empty );
						
			dr.RowError = "Err";
			Assert.AreEqual ( dr.RowError , "Err" );
		}
		
		[Test] 
		[ExpectedException (typeof (ConstraintException))]
		public void DataRow_RowError2()
		{
			DataTable dt1 = DataProvider.CreateUniqueConstraint();

			dt1.BeginLoadData();

			DataRow  dr = dt1.NewRow();
			dr[0] = 3;
			dt1.Rows.Add(dr);
			dt1.EndLoadData();
		}
		
		[Test] 
		[ExpectedException (typeof (ConstraintException))]
		public void DataRow_RowError3()
		{
			DataSet ds= DataProvider.CreateForigenConstraint();
			ds.Tables[0].BeginLoadData();
			ds.Tables[0].Rows[0][0] = 10; 
			ds.Tables[0].EndLoadData(); //Foreign constraint violation
		}


		[Test]
		public void TestRowErrors ()
		{
			DataTable table = new DataTable ();
			DataColumn col1 = table.Columns.Add ("col1", typeof (int));
			DataColumn col2 = table.Columns.Add ("col2", typeof (int));
			DataColumn col3 = table.Columns.Add ("col3", typeof (int));

			col1.AllowDBNull = false;
			table.Constraints.Add ("uc", new DataColumn[] {col2,col3}, false);
			table.BeginLoadData ();
			table.Rows.Add (new object[] {null,1,1});
			table.Rows.Add (new object[] {1,1,1});
			try {
				table.EndLoadData ();
				Assert.Fail ("#0");
			} catch (ConstraintException) {}
			Assert.IsTrue (table.HasErrors, "#1");
			DataRow[] rows = table.GetErrors ();

			Assert.AreEqual (2, rows.Length, "#2");
			Assert.AreEqual ("Column 'col1' does not allow DBNull.Value.", table.Rows [0].RowError, "#3");
			Assert.AreEqual ("Column 'col2, col3' is constrained to be unique.  Value '1, 1' is already present."
					, table.Rows [1].RowError, "#4");

			Assert.AreEqual (table.Rows [0].RowError, table.Rows [0].GetColumnError (0), "#5");
			Assert.AreEqual (table.Rows [1].RowError, table.Rows [0].GetColumnError (1), "#6");
			Assert.AreEqual (table.Rows [1].RowError, table.Rows [0].GetColumnError (2), "#7");

			Assert.AreEqual ("", table.Rows [1].GetColumnError (0), "#8");
			Assert.AreEqual (table.Rows [1].RowError, table.Rows [1].GetColumnError (1), "#9");
			Assert.AreEqual (table.Rows [1].RowError, table.Rows [1].GetColumnError (2), "#10");
		}

		[Test]
		public void BeginEdit()
		{
			DataTable myTable = new DataTable("myTable"); 
			DataColumn dc = new DataColumn("Id",typeof(int));
			dc.Unique=true;
			myTable.Columns.Add(dc);
			myTable.Rows.Add(new object[] {1});
			myTable.Rows.Add(new object[] {2});
			myTable.Rows.Add(new object[] {3});
			
			DataRow myRow = myTable.Rows[0];
			
			try	
			{ 
				myRow[0] = 2; //row[0] now conflict with row[1] 
				Assert.Fail("DRW121: failed to throw ConstraintException");
			}
			catch (ConstraintException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRW122: Add. Wrong exception type. Got:" + exc);
			}

			//Will NOT! throw exception
			myRow.BeginEdit();
			myRow[0] = 2; //row[0] now conflict with row[1] 
					
			DataTable dt = DataProvider.CreateParentDataTable();
			DataRow dr = dt.Rows[0];
			dr.Delete();
			try
			{
				dr.BeginEdit();				
				Assert.Fail("DRW123: failed to throw DeletedRowInaccessibleException");
			}
			catch (DeletedRowInaccessibleException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRW124: Add. Wrong exception type. Got:" + exc);
			}
		}

		[Test]
		public void GetChildRows_DataRelation()
		{
			DataRow dr;
			DataRow[] drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();

			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 

			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
			dr = dtParent.Rows[0];

			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"]);
			ds.Relations.Add(dRel);
			//Get Excepted result
			drArrExcepted = dtChild.Select("ParentId=" + dr["ParentId"]);
			//Get Result
			drArrResult = dr.GetChildRows(dRel);
			
			Assert.AreEqual(drArrExcepted, drArrResult, "DRW125");
		}

		[Test]
		public void GetParentRows_DataRelation_DataRowVersion()
		{
			DataRow drParent,drChild;
			DataRow[] drArrExcepted,drArrResult;
			DataTable dtChild,dtParent;
			DataSet ds = new DataSet();
			//Create tables
			dtChild = DataProvider.CreateChildDataTable();
			dtParent= DataProvider.CreateParentDataTable(); 
			//Add tables to dataset
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);
					
			drParent = dtParent.Rows[0];
			drChild = dtChild.Select("ParentId=" + drParent["ParentId"])[0];

			//Duplicate several rows in order to create Many to Many relation
			dtParent.ImportRow(drParent); 
			dtParent.ImportRow(drParent); 
			dtParent.ImportRow(drParent); 				
					
			//Add Relation
			DataRelation dRel = new DataRelation("Parent-Child",dtParent.Columns["ParentId"],dtChild.Columns["ParentId"],false);
			ds.Relations.Add(dRel);

			//Get Excepted result
			drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows );
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRows(dRel,DataRowVersion.Current);
			Assert.AreEqual(drArrExcepted, drArrResult, "DRW126");

			//Get Excepted result
			drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.OriginalRows );
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRows(dRel,DataRowVersion.Original );
			Assert.AreEqual(drArrExcepted, drArrResult, "DRW127");

			//Get Excepted result, in this case Current = Default
			drArrExcepted = dtParent.Select("ParentId=" + drParent["ParentId"],"",DataViewRowState.CurrentRows);
			//Get Result DataRowVersion.Current
			drArrResult = drChild.GetParentRows(dRel,DataRowVersion.Default  );
			Assert.AreEqual(drArrExcepted, drArrResult, "DRW128");
			
			try
			{
				DataTable dtOtherParent = DataProvider.CreateParentDataTable();
				DataTable dtOtherChild = DataProvider.CreateChildDataTable();

				DataRelation drl = new DataRelation("newRelation",dtOtherParent.Columns[0],dtOtherChild.Columns[0]);
				drChild.GetParentRows(drl,DataRowVersion.Current); 
				Assert.Fail("DRW129: failed to throw ArgumentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRW130: Add. Wrong exception type. Got:" + exc);
			}
		}

		[Test]
		public void ItemArray()
		{
			DataTable dt = GetDataTable();
			DataRow dr = dt.Rows[0];

			Assert.AreEqual(1, (int)dr.ItemArray[0] , "DRW131" );

			Assert.AreEqual("Ofer", (string)dr.ItemArray[1] , "DRW132" );

			dt = GetDataTable();

			dr = dt.Rows[0];
			
			//Changing row via itemArray

			dt.Rows[0].ItemArray = new object[] {2,"Oren"};

			Assert.AreEqual(2, (Int32)dr.ItemArray[0] , "DRW133" );
			Assert.AreEqual("Oren", (string)dr.ItemArray[1] , "DRW134" );

			try
			{
				dt.Rows[0].ItemArray = new object[] {2,"Oren","some1else"};
				Assert.Fail("DRW135: failed to throw ArgumentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRW136: Add. Wrong exception type. Got:" + exc);
			}
		}

		[Test]
		public void ItemArray_NewTable ()
		{
			DataTable dt = new DataTable("Customers");

			dt.Columns.Add("name", typeof (string));
			dt.Columns.Add("address", typeof (string));
			dt.Columns.Add("phone", typeof (string));

			DataRow dr = dt.NewRow();
			dr["name"] = "myName";
			dr["address"] = "myAddress";
			dr["phone"] = "myPhone";

			// Should not throw RowNotInTableException
			object[] obj = dr.ItemArray;
		}

		private DataTable GetDataTable()
		{
			DataTable dt = new DataTable("myTable"); 
			dt.Columns.Add("Id",typeof(int));
			dt.Columns.Add("Name",typeof(string));

			DataRow dr = dt.NewRow();
			dr.ItemArray = new object[] {1,"Ofer"};

			dt.Rows.Add(dr);

			return dt;
		}

		[Test]
		public void RowError()
		{
			DataTable dt = new DataTable("myTable"); 
			DataRow dr = dt.NewRow();

			Assert.AreEqual(string.Empty , dr.RowError, "DRW137");

			dr.RowError = "Err";

			Assert.AreEqual("Err", dr.RowError , "DRW138" );

			DataTable dt1 = DataProvider.CreateUniqueConstraint();

			try
			{
				dt1.BeginLoadData();

				dr = dt1.NewRow();
				dr[0] = 3;
				dt1.Rows.Add(dr);
				dt1.EndLoadData();
				Assert.Fail("DRW139: failed to throw ConstraintException");
			}
			catch (ConstraintException) 
			{
				Assert.AreEqual(2,dt1.GetErrors().Length,"DRW141");
				Assert.AreEqual(true,dt1.GetErrors()[0].RowError.Length > 10,"DRW142");
				Assert.AreEqual(true,dt1.GetErrors()[1].RowError.Length > 10,"DRW143");
			}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRW144: Wrong exception type. Got:" + exc);
			}


			DataSet ds=null;
			try
			{
				ds= DataProvider.CreateForigenConstraint();
				ds.Tables[0].BeginLoadData();
				ds.Tables[0].Rows[0][0] = 10; //Forigen constraint violation
				//ds.Tables[0].AcceptChanges();
				ds.Tables[0].EndLoadData();
				Assert.Fail("DRW139: failed to throw ConstraintException");
			}
			catch (ConstraintException) 
			{
				Assert.AreEqual(3,ds.Tables[1].GetErrors().Length,"DRW145");
				for(int index=0;index<3;index++)
				{
					Assert.AreEqual(true,ds.Tables[1].GetErrors()[index].RowError.Length > 10,"RDW146");
				}
			}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DRW147: Wrong exception type. Got:" + exc);
			}
		}

		[Test]
		public void bug78885 ()
		{
			DataSet ds = new DataSet ();
			DataTable t = ds.Tables.Add ("table");
			DataColumn id;

			id = t.Columns.Add ("userID", Type.GetType ("System.Int32"));
			id.AutoIncrement = true;
			t.Columns.Add ("name", Type.GetType ("System.String"));
			t.Columns.Add ("address", Type.GetType ("System.String"));
			t.Columns.Add ("zipcode", Type.GetType ("System.Int32"));
			t.PrimaryKey = new DataColumn [] { id };

			DataRow tempRow;
			tempRow = t.NewRow ();
			tempRow ["name"] = "Joan";
			tempRow ["address"] = "Balmes 152";
			tempRow ["zipcode"] = "1";
			t.Rows.Add (tempRow);

			t.RowChanged += new DataRowChangeEventHandler (RowChangedHandler);

			/* neither of the calls to EndEdit below generate a RowChangedHandler on MS.  the first one does on mono */
			t.DefaultView [0].BeginEdit ();
			t.DefaultView [0].EndEdit (); /* this generates a call to the row changed handler */
			t.DefaultView [0].EndEdit (); /* this doesn't */

			Assert.IsFalse (_rowChanged);
		}

		private void RowChangedHandler (object sender, DataRowChangeEventArgs e)
		{
			_rowChanged = true;
		}

#if NET_2_0
		string SetAddedModified_ErrMsg = "SetAdded and SetModified can only be called on DataRows with Unchanged DataRowState.";
		[Test]
		public void SetAdded_test()
		{
			DataTable table = new DataTable();

			DataRow row = table.NewRow();
			try {
				row.SetAdded();
				Assert.Fail ("#1");
			} catch (InvalidOperationException e) {
				// Never premise English.
				//Assert.AreEqual (SetAddedModified_ErrMsg, e.Message, "#2");
			}

			table.Columns.Add("col1", typeof(int));
			table.Columns.Add("col2", typeof(int));
			table.Columns.Add("col3", typeof(int));

			row = table.Rows.Add(new object[] { 1, 2, 3 });
			Assert.AreEqual(DataRowState.Added, row.RowState, "#1");
			try {
				row.SetAdded();
				Assert.Fail ("#2");
			} catch (InvalidOperationException e) {
				// Never premise English.
				//Assert.AreEqual (SetAddedModified_ErrMsg, e.Message, "#2");
			}
			Assert.AreEqual(DataRowState.Added, row.RowState, "#2");

			row.AcceptChanges();
			row[0] = 10;
			Assert.AreEqual(DataRowState.Modified, row.RowState, "#5");
			try {
				row.SetAdded();
				Assert.Fail ("#3");
			} catch (InvalidOperationException e) {
				// Never premise English.
				//Assert.AreEqual (SetAddedModified_ErrMsg, e.Message, "#2");
			}

			row.AcceptChanges();
			Assert.AreEqual(DataRowState.Unchanged, row.RowState, "#3");
			row.SetAdded();
			Assert.AreEqual(DataRowState.Added, row.RowState, "#4");
		}

		[Test]
		public void setAdded_testRollback ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("col1", typeof (int));
			table.Columns.Add ("col2", typeof (int));

			table.Rows.Add (new object[] {1,1});
			table.AcceptChanges ();

			table.Rows [0].SetAdded ();
			table.RejectChanges ();
			Assert.AreEqual (0, table.Rows.Count, "#1");
		}

		[Test]
		public void SetModified_test()
		{
			DataTable table = new DataTable();

			DataRow row = table.NewRow();
			try {
				row.SetModified ();
			} catch (InvalidOperationException) {}

			table.Columns.Add("col1", typeof(int));
			table.Columns.Add("col2", typeof(int));
			table.Columns.Add("col3", typeof(int));

			row = table.Rows.Add(new object[] { 1, 2, 3 });
			Assert.AreEqual(DataRowState.Added, row.RowState, "#1");
			try {
				row.SetModified();
				Assert.Fail ("#1");
			} catch (InvalidOperationException e) {
				// Never premise English.
				//Assert.AreEqual (SetAddedModified_ErrMsg, e.Message, "#2");
			}

			row.AcceptChanges();
			row[0] = 10;
			Assert.AreEqual(DataRowState.Modified, row.RowState, "#5");
			try {
				row.SetModified ();
				Assert.Fail ("#2");
			} catch (InvalidOperationException e) {
				// Never premise English.
				//Assert.AreEqual (SetAddedModified_ErrMsg, e.Message, "#2");
			}

			row.AcceptChanges();
			Assert.AreEqual(DataRowState.Unchanged, row.RowState, "#3");
			row.SetModified ();
			Assert.AreEqual(DataRowState.Modified, row.RowState, "#4");
		}

		[Test]
		public void setModified_testRollback()
		{
			DataTable table = new DataTable();
			table.Columns.Add("col1", typeof(int));
			table.Columns.Add("col2", typeof(int));

			DataRow row = table.Rows.Add(new object[] { 1, 1 });
			table.AcceptChanges();

			row.SetModified ();
			Assert.AreEqual(row.RowState, DataRowState.Modified, "#0");
			Assert.AreEqual(1, row [0, DataRowVersion.Current], "#1");
			Assert.AreEqual(1, row [0, DataRowVersion.Original], "#2");
			table.RejectChanges ();
			Assert.AreEqual(row.RowState, DataRowState.Unchanged, "#3");
		}
#endif
		[Test]
		public void DataRowExpressionDefaultValueTest ()
		{
			DataSet ds = new DataSet ();
			DataTable custTable = ds.Tables.Add ("CustTable");

			DataColumn column = new DataColumn ("units", typeof (int));
			column.AllowDBNull = false;
			column.Caption = "Units";
			column.DefaultValue = 1;
			custTable.Columns.Add (column);

			column = new DataColumn ("price", typeof (decimal));
			column.AllowDBNull = false;
			column.Caption = "Price";
			column.DefaultValue = 25;
			custTable.Columns.Add (column);

			column = new DataColumn ("total", typeof (string));
			column.Caption = "Total";
			column.Expression = "price*units";
			custTable.Columns.Add (column);

			DataRow row = custTable.NewRow ();

			Assert.AreEqual (DBNull.Value, row["Total"] , "#1 Should be System.DBNull");
			custTable.Rows.Add (row);

			Assert.AreEqual ("25", row["Total"] , "#2 Should not be emptry string");
		}

		class Person
		{
			public Person (string name)
			{
				this.Name = name;
			}

			public string Name;
			public Address HomeAddress;
		}

		struct Address
		{
			public Address (string street, int houseNumber)
			{
				Street = street;
				HouseNumber = houseNumber;
			}

			public override bool Equals (object o)
			{
				if (!(o is Address))
					return false;

				Address address = (Address) o;
				if (address.HouseNumber != HouseNumber)
					return false;
				if (address.Street != Street)
					return false;
				return true;
			}

			public override int GetHashCode ()
			{
				if (Street == null)
					return HouseNumber.GetHashCode ();
				return (Street.GetHashCode () ^ HouseNumber.GetHashCode ());
			}

			public string Street;
			public int HouseNumber;
		}
	}
}
