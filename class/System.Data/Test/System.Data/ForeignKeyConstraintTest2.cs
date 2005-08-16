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
using System.Data;
using MonoTests.System.Data.Utils;

namespace MonoTests_System.Data
{
	[TestFixture] public class ForeignKeyConstraintTest2
	{
		[Test] public void Columns()
		{
			//int RowCount;
			DataSet ds = new DataSet();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			// Columns
			Assert.AreEqual(dtChild.Columns[0] , fc.Columns[0]  , "FKC");

			// Columns count
			Assert.AreEqual(1 , fc.Columns.Length , "FKC");
		}

		[Test] public void Equals()
		{
			DataSet ds = new DataSet();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);
			dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns[0]};
			ds.EnforceConstraints = true;

			ForeignKeyConstraint fc1,fc2;
			fc1 = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			fc2 = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[1]);
			// different columnn
			Assert.AreEqual(false, fc1.Equals(fc2), "FKC");

			//Two System.Data.ForeignKeyConstraint are equal if they constrain the same columns.
			// same column
			fc2 = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);
			Assert.AreEqual(true, fc1.Equals(fc2), "FKC");
		}

		[Test] public void RelatedColumns()
		{
			DataSet ds = new DataSet();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);
			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			// RelatedColumns
			Assert.AreEqual(new DataColumn[] {dtParent.Columns[0]}, fc.RelatedColumns , "FKC");
		}

		[Test] public void RelatedTable()
		{
			DataSet ds = new DataSet();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);
			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			// RelatedTable
			Assert.AreEqual(dtParent, fc.RelatedTable , "FKC");
		}

		[Test] public void Table()
		{
			DataSet ds = new DataSet();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);
			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			// Table
			Assert.AreEqual(dtChild , fc.Table , "FKC");
		}

		[Test] public new void ToString()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();

			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			// ToString - default
			Assert.AreEqual(string.Empty , fc.ToString(), "FKC");

			fc = new ForeignKeyConstraint("myConstraint",dtParent.Columns[0],dtChild.Columns[0]);
			// Tostring - Constraint name
			Assert.AreEqual("myConstraint", fc.ToString(), "FKC");
		}

		[Test] public void acceptRejectRule()
		{
			DataSet ds = getNewDataSet();

			ForeignKeyConstraint fc = new ForeignKeyConstraint(ds.Tables[0].Columns[0],ds.Tables[1].Columns[0]);
			fc.AcceptRejectRule= AcceptRejectRule.Cascade;
			ds.Tables[1].Constraints.Add(fc);

			//Update the parent 

			ds.Tables[0].Rows[0]["ParentId"] = 777;
			Assert.AreEqual(true, ds.Tables[1].Select("ParentId=777").Length > 0 , "FKC");
			ds.Tables[0].RejectChanges();
			Assert.AreEqual(0, ds.Tables[1].Select("ParentId=777").Length , "FKC");
		}
		private DataSet getNewDataSet()
		{
			DataSet ds1 = new DataSet();
			ds1.Tables.Add(DataProvider.CreateParentDataTable());
			ds1.Tables.Add(DataProvider.CreateChildDataTable());
			//	ds1.Tables.Add(DataProvider.CreateChildDataTable());
			ds1.Tables[0].PrimaryKey=  new DataColumn[] {ds1.Tables[0].Columns[0]};

			return ds1;
		}

		[Test] public void constraintName()
		{
			DataSet ds = new DataSet();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);

			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			// default 
			Assert.AreEqual(string.Empty , fc.ConstraintName , "FKC");

			fc.ConstraintName  = "myConstraint";

			// set/get 
			Assert.AreEqual("myConstraint" , fc.ConstraintName , "FKC");
		}

		[Test] public void ctor_ParentColChildCol()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			DataSet ds = new DataSet();
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);

			ForeignKeyConstraint fc = null;

			// Ctor ArgumentException
			try 
			{
				fc = new ForeignKeyConstraint(new DataColumn[] {dtParent.Columns[0]} ,new DataColumn[] {dtChild.Columns[0],dtChild.Columns[1]});				
				Assert.Fail("DS333: ctor Indexer Failed to throw ArgumentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DS334: ctor. Wrong exception type. Got:" + exc);
			}

			fc = new ForeignKeyConstraint(new DataColumn[] {dtParent.Columns[0],dtParent.Columns[1]} ,new DataColumn[] {dtChild.Columns[0],dtChild.Columns[2]});				

			// Add constraint to table - ArgumentException
			try 
			{
				dtChild.Constraints.Add(fc);
				Assert.Fail("DS333: ctor Indexer Failed to throw ArgumentException");
			}
			catch (ArgumentException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DS334: ctor. Wrong exception type. Got:" + exc);
			}

			// Child Table Constraints Count - two columnns
			Assert.AreEqual(0, dtChild.Constraints.Count , "FKC");

			// Parent Table Constraints Count - two columnns
			Assert.AreEqual(1, dtParent.Constraints.Count , "FKC");

			// DataSet relations Count
			Assert.AreEqual(0, ds.Relations.Count , "FKC");

			dtParent.Constraints.Clear();
			dtChild.Constraints.Clear();

			fc = new ForeignKeyConstraint(new DataColumn[] {dtParent.Columns[0]} ,new DataColumn[] {dtChild.Columns[0]});
			// Ctor
			Assert.AreEqual(false , fc == null , "FKC");

			// Child Table Constraints Count
			Assert.AreEqual(0, dtChild.Constraints.Count , "FKC");

			// Parent Table Constraints Count
			Assert.AreEqual(0, dtParent.Constraints.Count , "FKC");

			// DataSet relations Count
			Assert.AreEqual(0, ds.Relations.Count , "FKC");

			dtChild.Constraints.Add(fc);

			// Child Table Constraints Count, Add
			Assert.AreEqual(1, dtChild.Constraints.Count , "FKC");

			// Parent Table Constraints Count, Add
			Assert.AreEqual(1, dtParent.Constraints.Count , "FKC");

			// DataSet relations Count, Add
			Assert.AreEqual(0, ds.Relations.Count , "FKC");

			// Parent Table Constraints type
			Assert.AreEqual(typeof(UniqueConstraint), dtParent.Constraints[0].GetType() , "FKC");

			// Parent Table Constraints type
			Assert.AreEqual(typeof(ForeignKeyConstraint), dtChild.Constraints[0].GetType() , "FKC");

			// Parent Table Primary key
			Assert.AreEqual(0, dtParent.PrimaryKey.Length , "FKC");

			dtChild.Constraints.Clear();
			dtParent.Constraints.Clear();
			ds.Relations.Add(new DataRelation("myRelation",dtParent.Columns[0],dtChild.Columns[0]));

			// Relation - Child Table Constraints Count
			Assert.AreEqual(1, dtChild.Constraints.Count , "FKC");

			// Relation - Parent Table Constraints Count
			Assert.AreEqual(1, dtParent.Constraints.Count , "FKC");

			// Relation - Parent Table Constraints type
			Assert.AreEqual(typeof(UniqueConstraint), dtParent.Constraints[0].GetType() , "FKC");

			// Relation - Parent Table Constraints type
			Assert.AreEqual(typeof(ForeignKeyConstraint), dtChild.Constraints[0].GetType() , "FKC");

			// Relation - Parent Table Primary key
			Assert.AreEqual(0, dtParent.PrimaryKey.Length , "FKC");
		}

		[Test] public void ctor_NameParentColChildCol()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();

			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint("myForeignKey",dtParent.Columns[0],dtChild.Columns[0]);

			// Ctor
			Assert.AreEqual(false , fc == null , "FKC");

			// Ctor - name
			Assert.AreEqual("myForeignKey" , fc.ConstraintName  , "FKC");
		}

		[Test] public void ctor_NameParentColsChildCols()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();

			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint("myForeignKey",new DataColumn[] {dtParent.Columns[0]} ,new DataColumn[] {dtChild.Columns[0]});

			// Ctor
			Assert.AreEqual(false , fc == null , "FKC");

			// Ctor - name
			Assert.AreEqual("myForeignKey" , fc.ConstraintName  , "FKC");
		}

		[Test] public void deleteRule()
		{
			DataSet ds = new DataSet();
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateChildDataTable();
			ds.Tables.Add(dtParent);
			ds.Tables.Add(dtChild);
			dtParent.PrimaryKey = new DataColumn[] {dtParent.Columns[0]};
			ds.EnforceConstraints = true;

			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			//checking default
			// Default
			Assert.AreEqual(Rule.Cascade , fc.DeleteRule , "FKC");

			//checking set/get
			foreach (Rule rule in Enum.GetValues(typeof(Rule)))
			{
				// Set/Get - rule
				fc.DeleteRule = rule;
				Assert.AreEqual(rule, fc.DeleteRule , "FKC");
			}

			dtChild.Constraints.Add(fc);

			//checking delete rule

			// Rule = None, Delete Exception
			fc.DeleteRule = Rule.None;
			//Exception = "Cannot delete this row because constraints are enforced on relation Constraint1, and deleting this row will strand child rows."
			try 
			{
				dtParent.Rows.Find(1).Delete();
				Assert.Fail("DS333: Find Indexer Failed to throw InvalidConstraintException");
			}
			catch (InvalidConstraintException) {}
			catch (AssertionException exc) {throw  exc;}
			catch (Exception exc)
			{
				Assert.Fail("DS334: Find. Wrong exception type. Got:" + exc);
			}

			// Rule = None, Delete succeed
			fc.DeleteRule = Rule.None;
			foreach (DataRow dr in dtChild.Select("ParentId = 1"))
				dr.Delete();
			dtParent.Rows.Find(1).Delete();
			Assert.AreEqual(0, dtParent.Select("ParentId=1").Length , "FKC");

			// Rule = Cascade
			fc.DeleteRule = Rule.Cascade;
			dtParent.Rows.Find(2).Delete();
			Assert.AreEqual(0, dtChild.Select("ParentId=2").Length , "FKC");

			// Rule = SetNull
			DataSet ds1 = new DataSet();
			ds1.Tables.Add(DataProvider.CreateParentDataTable());
			ds1.Tables.Add(DataProvider.CreateChildDataTable());

			ForeignKeyConstraint fc1 = new ForeignKeyConstraint(ds1.Tables[0].Columns[0],ds1.Tables[1].Columns[1]); 
			fc1.DeleteRule = Rule.SetNull;
			ds1.Tables[1].Constraints.Add(fc1);

			Assert.AreEqual(0, ds1.Tables[1].Select("ChildId is null").Length, "FKC");

			ds1.Tables[0].PrimaryKey=  new DataColumn[] {ds1.Tables[0].Columns[0]};
			ds1.Tables[0].Rows.Find(3).Delete();

			ds1.Tables[0].AcceptChanges();
			ds1.Tables[1].AcceptChanges();	

			DataRow[] arr =  ds1.Tables[1].Select("ChildId is null");

			/*foreach (DataRow dr in arr)
					{
						Assert.AreEqual(null, dr["ChildId"], "FKC");
					}*/

			Assert.AreEqual(4, arr.Length , "FKC");

			// Rule = SetDefault
			//fc.DeleteRule = Rule.SetDefault;
			ds1 = new DataSet();
			ds1.Tables.Add(DataProvider.CreateParentDataTable());
			ds1.Tables.Add(DataProvider.CreateChildDataTable());

			fc1 = new ForeignKeyConstraint(ds1.Tables[0].Columns[0],ds1.Tables[1].Columns[1]); 
			fc1.DeleteRule = Rule.SetDefault;
			ds1.Tables[1].Constraints.Add(fc1);
			ds1.Tables[1].Columns[1].DefaultValue="777";

			//Add new row  --> in order to apply the forigen key rules
			DataRow dr2 = ds1.Tables[0].NewRow();
			dr2["ParentId"] = 777;
			ds1.Tables[0].Rows.Add(dr2);

			ds1.Tables[0].PrimaryKey=  new DataColumn[] {ds1.Tables[0].Columns[0]};
			ds1.Tables[0].Rows.Find(3).Delete();
			Assert.AreEqual(4, ds1.Tables[1].Select("ChildId=777").Length  , "FKC");
		}

		[Test] public void extendedProperties()
		{
			DataTable dtParent = DataProvider.CreateParentDataTable();
			DataTable dtChild = DataProvider.CreateParentDataTable();

			ForeignKeyConstraint fc = null;
			fc = new ForeignKeyConstraint(dtParent.Columns[0],dtChild.Columns[0]);

			PropertyCollection pc = fc.ExtendedProperties ;

			// Checking ExtendedProperties default 
			Assert.AreEqual(true, fc != null, "FKC");

			// Checking ExtendedProperties count 
			Assert.AreEqual(0, pc.Count , "FKC");
		}
	}
}
