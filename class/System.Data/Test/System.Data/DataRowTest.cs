// DataRowTest.cs - NUnit Test Cases for System.DataRow
//
// Authors:
//   Franklin Wise (gracenote@earthlink.net)
//   Daniel Morgan <danmorg@sc.rr.com>
//
// (C) Copyright 2002 Franklin Wise
// (C) Copyright 2003 Daniel Morgan
// (C) Copyright 2003 Martin Willemoes Hansen
// 

using NUnit.Framework;
using System;
using System.Data;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataRowTest : Assertion {
	
		private DataTable _tbl;	

		[SetUp]
		public void GetReady() {
			_tbl = new DataTable();
		}

		// tests item at row, column in table to be DBNull.Value
		private void DBNullTest (string message, DataTable dt, int row, int column) 
		{
			object val = dt.Rows[row].ItemArray[column];
			AssertEquals(message, DBNull.Value, val);
		}

		// tests item at row, column in table to be null
		private void NullTest (string message, DataTable dt, int row, int column) 
		{
			object val = dt.Rows[row].ItemArray[column];
			AssertEquals(message, null, val);
		}

		// tests item at row, column in table to be 
		private void ValueTest (string message, DataTable dt, int row, int column, object value) 
		{
			object val = dt.Rows[row].ItemArray[column];
			AssertEquals(message, value, val);
		}

		// test set null, DBNull.Value, and ItemArray short count
		[Test]
		public void NullInItemArray () 
		{
			string zero = "zero";
			string one = "one";
			string two = "two";

			DataTable table = new DataTable();
			table.Columns.Add(new DataColumn(zero, typeof(string)));
			table.Columns.Add(new DataColumn(one, typeof(string)));
			table.Columns.Add(new DataColumn(two, typeof(string)));

			object[] obj = new object[3];
			// -- normal -----------------
			obj[0] = zero;
			obj[1] = one;
			obj[2] = two;
			// results:
			//   table.Rows[0].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[0].ItemArray.ItemArray[1] = "one"
			//   table.Rows[0].ItemArray.ItemArray[2] = "two"
			
			DataRow row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e1) {
				Fail("DR1: Exception Caught: " + e1);
			}
			
			table.Rows.Add(row);

			// -- null ----------
			obj[1] = null;
			// results:
			//   table.Rows[1].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[1].ItemArray.ItemArray[1] = DBNull.Value
			//   table.Rows[1].ItemArray.ItemArray[2] = "two"

			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e2) {
				Fail("DR2: Exception Caught: " + e2);
			}
			
			table.Rows.Add(row);

			// -- DBNull.Value -------------
			obj[1] = DBNull.Value;
			// results:
			//   table.Rows[2].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[2].ItemArray.ItemArray[1] = DBNull.Value
			//   table.Rows[2].ItemArray.ItemArray[2] = "two"

			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e3) {
				Fail("DR3: Exception Caught: " + e3);
			}
			
			table.Rows.Add(row);

			// -- object array smaller than number of columns -----
			string abc = "abc";
			string def = "def";
			obj = new object[2];
			obj[0] = abc;
			obj[1] = def;
			// results:
			//   table.Rows[3].ItemArray.ItemArray[0] = "abc"
			//   table.Rows[3].ItemArray.ItemArray[1] = "def"
			//   table.Rows[3].ItemArray.ItemArray[2] = DBNull.Value;
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e3) {
				Fail("DR4: Exception Caught: " + e3);
			}
			
			table.Rows.Add(row);

			// -- normal -----------------
			ValueTest("DR5: normal value test", table, 0, 0, zero);
			ValueTest("DR6: normal value test", table, 0, 1, one);
			ValueTest("DR7: normal value test", table, 0, 2, two);

			// -- null ----------
			ValueTest("DR8: null value test", table, 1, 0, zero);
			ValueTest("DR9: null value test", table, 1, 1, DBNull.Value);
			ValueTest("DR10: null value test", table, 1, 2, two);

			// -- DBNull.Value -------------
			ValueTest("DR11: DBNull.Value value test", table, 2, 0, zero);
			ValueTest("DR12: DBNull.Value value test", table, 2, 1, DBNull.Value);
			ValueTest("DR13: DBNull.Value value test", table, 2, 2, two);

			// -- object array smaller than number of columns -----
			ValueTest("DR14: array smaller value test", table, 3, 0, abc);
			ValueTest("DR15: array smaller value test", table, 3, 1, def);
			ValueTest("DR16: array smaller value test", table, 3, 2, DBNull.Value);
		}
	
		// test DefaultValue when setting ItemArray
		[Test]
		public void DefaultValueInItemArray () {		
			string zero = "zero";

			DataTable table = new DataTable();
			table.Columns.Add(new DataColumn("zero", typeof(string)));		
			
			DataColumn column = new DataColumn("num", typeof(int));
			column.DefaultValue = 15;
			table.Columns.Add(column);
			
			object[] obj = new object[2];
			// -- normal -----------------
			obj[0] = "zero";
			obj[1] = 8;
			// results:
			//   table.Rows[0].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[0].ItemArray.ItemArray[1] = 8
						
			DataRow row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e1) {
				Fail("DR17: Exception Caught: " + e1);
			}
			
			table.Rows.Add(row);

			// -- null ----------
			obj[1] = null;
			// results:
			//   table.Rows[1].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[1].ItemArray.ItemArray[1] = 15
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e2) {
				Fail("DR18: Exception Caught: " + e2);
			}
			
			table.Rows.Add(row);

			// -- DBNull.Value -------------
			obj[1] = DBNull.Value;
			// results:
			//   table.Rows[2].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[2].ItemArray.ItemArray[1] = DBNull.Value
			//      even though internally, the v
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e3) {
				Fail("DR19: Exception Caught: " + e3);
			}
			
			table.Rows.Add(row);

			// -- object array smaller than number of columns -----
			string abc = "abc";
			string def = "def";
			obj = new object[2];
			obj[0] = abc;
			// results:
			//   table.Rows[3].ItemArray.ItemArray[0] = "abc"
			//   table.Rows[3].ItemArray.ItemArray[1] = DBNull.Value
						
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e3) {
				Fail("DR20: Exception Caught: " + e3);
			}
			
			table.Rows.Add(row);

			// -- normal -----------------
			ValueTest("DR20: normal value test", table, 0, 0, zero);
			ValueTest("DR21: normal value test", table, 0, 1, 8);
			
			// -- null ----------
			ValueTest("DR22: null value test", table, 1, 0, zero);
			ValueTest("DR23: null value test", table, 1, 1, 15);
			
			// -- DBNull.Value -------------
			ValueTest("DR24: DBNull.Value value test", table, 2, 0, zero);
			DBNullTest("DR25: DBNull.Value value test", table, 2, 1);
			
			// -- object array smaller than number of columns -----
			ValueTest("DR26: array smaller value test", table, 3, 0, abc);
			ValueTest("DR27: array smaller value test", table, 3, 1, 15);
		}

		// test AutoIncrement when setting ItemArray
		[Test]
		public void AutoIncrementInItemArray () {
			string zero = "zero";
			string num = "num";
			
			DataTable table = new DataTable();
			table.Columns.Add(new DataColumn(zero, typeof(string)));		
			
			DataColumn column = new DataColumn("num", typeof(int));
			column.AutoIncrement = true;
			table.Columns.Add(column);
			
			object[] obj = new object[2];
			// -- normal -----------------
			obj[0] = "zero";
			obj[1] = 8;
			// results:
			//   table.Rows[0].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[0].ItemArray.ItemArray[1] = 8
						
			DataRow row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e1) {
				Fail("DR28:  Exception Caught: " + e1);
			}
			
			table.Rows.Add(row);

			// -- null 1----------
			obj[1] = null;
			// results:
			//   table.Rows[1].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[1].ItemArray.ItemArray[1] = 9
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e2) {
				Fail("DR29:  Exception Caught: " + e2);
			}
			
			table.Rows.Add(row);

			// -- null 2----------
			obj[1] = null;
			// results:
			//   table.Rows[1].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[1].ItemArray.ItemArray[1] = 10
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e2) {
				Fail("DR30: Exception Caught: " + e2);
			}
			
			table.Rows.Add(row);

			// -- null 3----------
			obj[1] = null;
			// results:
			//   table.Rows[1].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[1].ItemArray.ItemArray[1] = 11
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e2) {
				Fail("DR31: Exception Caught: " + e2);
			}
			
			table.Rows.Add(row);

			// -- DBNull.Value -------------
			obj[1] = DBNull.Value;
			// results:
			//   table.Rows[2].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[2].ItemArray.ItemArray[1] = DBNull.Value
			//      even though internally, the AutoIncrement value
			//      is incremented
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e3) {
				Fail("DR32: Exception Caught: " + e3);
			}
			
			table.Rows.Add(row);

			// -- null 4----------
			obj[1] = null;
			// results:
			//   table.Rows[1].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[1].ItemArray.ItemArray[1] = 13
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e2) {
				Fail("DR48: Exception Caught: " + e2);
			}
			
			table.Rows.Add(row);

			// -- object array smaller than number of columns -----
			string abc = "abc";
			string def = "def";
			obj = new object[2];
			obj[0] = abc;
			// results:
			//   table.Rows[3].ItemArray.ItemArray[0] = "abc"
			//   table.Rows[3].ItemArray.ItemArray[1] = 14
						
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e3) {
				Fail("DR33: Exception Caught: " + e3);
			}
			
			table.Rows.Add(row);

			// -- normal -----------------
			ValueTest("DR34: normal value test", table, 0, 0, zero);
			ValueTest("DR35: normal value test", table, 0, 1, 8);
			
			// -- null 1----------
			ValueTest("DR36: null value test", table, 1, 0, zero);
			ValueTest("DR37: null value test", table, 1, 1, 9);

			// -- null 2----------
			ValueTest("DR38: null value test", table, 2, 0, zero);
			ValueTest("DR39: null value test", table, 2, 1, 10);

			// -- null 3----------
			ValueTest("DR40: null value test", table, 3, 0, zero);
			ValueTest("DR41: null value test", table, 3, 1, 11);

			// -- DBNull.Value -------------
			ValueTest("DR42: DBNull.Value value test", table, 4, 0, zero);
			ValueTest("DR43: DBNull.Value value test", table, 4, 1, DBNull.Value);

			// -- null 4----------
			ValueTest("DR44: null value test", table, 5, 0, zero);
			ValueTest("DR45: null value test", table, 5, 1, 13);

			// -- object array smaller than number of columns -----
			ValueTest("DR46: array smaller value test", table, 6, 0, abc);
			ValueTest("DR47: array smaller value test", table, 6, 1, 14);
		}
	}
}
