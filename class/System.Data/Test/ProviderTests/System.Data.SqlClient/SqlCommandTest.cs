//
// SqlCommandTest.cs - NUnit Test Cases for testing the
//                          SqlCommand class
// Author:
//      Umadevi S (sumadevi@novell.com)
//	Sureshkumar T (tsureshkumar@novell.com)
//	Senganal T (tsenganal@novell.com)
//
// Copyright (c) 2004 Novell Inc., and the individuals listed
// on the ChangeLog entries.
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
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient 
{
	[TestFixture]
	[Category ("sqlserver")]
	public class SqlCommandTest 
	{

		public SqlConnection conn = null ;
		SqlCommand cmd = null;
		string connectionString = ConnectionManager.Singleton.ConnectionString;

		[SetUp]
		public void Setup ()
		{
		}

		[TearDown]
		public void TearDown ()
		{
			if (conn != null)
				conn.Close ();
		}

		[Test]
		public void ConstructorTest ()
		{
			// Test Default Constructor 
			cmd = new SqlCommand ();
			Assert.AreEqual (String.Empty, cmd.CommandText,
				 "#1 Command Test should be empty");
			Assert.AreEqual (30, cmd.CommandTimeout, 
				"#2 CommandTimeout should be 30");
			Assert.AreEqual (CommandType.Text, cmd.CommandType, 
				"#3 CommandType should be text");
			Assert.IsNull (cmd.Connection, "#4 Connection Should be null");
			Assert.AreEqual (0, cmd.Parameters.Count,
				"#5 Parameter shud be empty");

			// Test Overloaded Constructor 
			String cmdText = "select * from tbl1" ;
			cmd = new SqlCommand (cmdText);
			Assert.AreEqual (cmdText, cmd.CommandText,
				"#5 CommandText should be the same as passed");
			Assert.AreEqual (30, cmd.CommandTimeout,
				"#6 CommandTimeout should be 30");
			Assert.AreEqual (CommandType.Text, cmd.CommandType,
				"#7 CommandType should be text");
			Assert.IsNull (cmd.Connection , "#8 Connection Should be null");
			
			// Test Overloaded Constructor 
			SqlConnection conn = new SqlConnection ();
			cmd = new SqlCommand (cmdText , conn);
			Assert.AreEqual (cmdText, cmd.CommandText,
				"#9 CommandText should be the same as passed");
			Assert.AreEqual (30, cmd.CommandTimeout,
				"#10 CommandTimeout should be 30");
			Assert.AreEqual (CommandType.Text, cmd.CommandType,
				"#11 CommandType should be text");
			Assert.AreSame (cmd.Connection, conn, "#12 Connection Should be same");	

			// Test Overloaded Constructor 
			SqlTransaction trans = null ; 
			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();
				trans = conn.BeginTransaction ();
				cmd = new SqlCommand (cmdText, conn, trans); 
				Assert.AreEqual (cmdText, cmd.CommandText,
					"#9 CommandText should be the same as passed");
				Assert.AreEqual (30, cmd.CommandTimeout,
					"#10 CommandTimeout should be 30");
				Assert.AreEqual (CommandType.Text, cmd.CommandType,
					"#11 CommandType should be text");
				Assert.AreEqual (cmd.Connection, conn,
					"#12 Connection Should be null");	
				Assert.AreEqual (cmd.Transaction, trans,
					 "#13 Transaction Property should be set");
				
				// Test if parameters are reset to Default Values	
				cmd = new SqlCommand ();
				Assert.AreEqual (String.Empty, cmd.CommandText,
					"#1 Command Test should be empty");
				Assert.AreEqual (30, cmd.CommandTimeout,
					"#2 CommandTimeout should be 30");
				Assert.AreEqual (CommandType.Text, cmd.CommandType,
					"#3 CommandType should be text");
				Assert.IsNull (cmd.Connection, "#4 Connection Should be null");
			}finally {
				trans.Rollback ();
			}
		}

		[Test]
		public void ExecuteScalarTest ()
		{
			conn = new SqlConnection (connectionString);
			cmd = new SqlCommand ("" , conn);
			cmd.CommandText = "Select count(*) from numeric_family where id<=4";

			//Check Exception is thrown when executed on a closed connection 
			try {
				cmd.ExecuteScalar ();
				Assert.Fail ("#1 InvalidOperation Exception must be thrown");
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof (InvalidOperationException), e.GetType (),
					"#2 Incorrect Exception : " + e.StackTrace);
			}

			// Check the Return value for a Correct Query 
			object result = 0;
			conn.Open ();
			result = cmd.ExecuteScalar ();
			Assert.AreEqual (4, (int)result, "#3 Query Result returned is incorrect");

			cmd.CommandText = "select id , type_bit from numeric_family order by id asc" ;
			result = Convert.ToInt32 (cmd.ExecuteScalar ());
			Assert.AreEqual (1, result,
				"#4 ExecuteScalar Should return (1,1) the result set" );

			cmd.CommandText = "select id from numeric_family where id=-1";
			result = cmd.ExecuteScalar ();
			Assert.IsNull (result, "#5 Null should be returned if result set is empty");

			// Check SqlException is thrown for Invalid Query 
			cmd.CommandText = "select count* from numeric_family";
			try {
				result = cmd.ExecuteScalar ();
				Assert.Fail ("#6 InCorrect Query should cause an SqlException");
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof(SqlException), e.GetType(),
					"#7 Incorrect Exception : " + e.StackTrace);
			}


			// Parameterized stored procedure calls

			int int_value = 20;
			string string_value = "output value changed";
			string return_value = "first column of first rowset";
			
			cmd.CommandText = 
				"create procedure #tmp_executescalar_outparams "+
				" (@p1 int, @p2 int out, @p3 varchar(200) out) "+
				"as " +
				"select '" + return_value + "' as 'col1', @p1 as 'col2' "+
				"set @p2 = @p2 * 2 "+
				"set @p3 = N'" + string_value + "' "+
				"select 'second rowset' as 'col1', 2 as 'col2' "+
				"return 1";
			
			cmd.CommandType = CommandType.Text;
			cmd.ExecuteNonQuery ();

			cmd.CommandText = "#tmp_executescalar_outparams";
			cmd.CommandType = CommandType.StoredProcedure;

			SqlParameter p1 = new SqlParameter ();
			p1.ParameterName = "@p1";
			p1.Direction = ParameterDirection.Input;
			p1.DbType = DbType.Int32;
			p1.Value = int_value;
			cmd.Parameters.Add (p1);

			SqlParameter p2 = new SqlParameter ();
			p2.ParameterName = "@p2";
			p2.Direction = ParameterDirection.InputOutput;
			p2.DbType = DbType.Int32;
			p2.Value = int_value;
			cmd.Parameters.Add (p2);

			SqlParameter p3 = new SqlParameter ();
			p3.ParameterName = "@p3";
			p3.Direction = ParameterDirection.Output;
			p3.DbType = DbType.String;
			p3.Size = 200;
			cmd.Parameters.Add (p3);

			result = cmd.ExecuteScalar ();
			Assert.AreEqual (return_value, result, "#8 ExecuteScalar Should return 'first column of first rowset'");
			Assert.AreEqual (int_value * 2, p2.Value, "#9 ExecuteScalar should fill the parameter collection with the outputted values");
			Assert.AreEqual (string_value, p3.Value, "#10 ExecuteScalar should fill the parameter collection with the outputted values");
		}

		[Test]
		public void ExecuteNonQuery ()
		{
			conn = new SqlConnection (connectionString);
			cmd = new SqlCommand ("", conn);
			int result = 0;

			// Test for exceptions
			// Test exception is thrown if connection is closed
			cmd.CommandText = "Select id from numeric_family where id=1";
			try {
				cmd.ExecuteNonQuery ();
				Assert.Fail ("#1 Connextion shud be open"); 
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof(InvalidOperationException), e.GetType(),
					"#2 Incorrect Exception : " + e);
			}
			
			// Test Exception is thrown if Query is incorrect 
			conn.Open ();
			cmd.CommandText = "Select id1 from numeric_family";
			try {
				cmd.ExecuteNonQuery ();	
				Assert.Fail ("#1 invalid Query");
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof(SqlException), e.GetType(),
					"#2 Incorrect Exception : " + e);
			}

			// Test Select/Insert/Update/Delete Statements 
			SqlTransaction trans = conn.BeginTransaction ();
			cmd.Transaction = trans; 

			try {
				cmd.CommandText = "Select id from numeric_family where id=1";
				result = cmd.ExecuteNonQuery ();
				Assert.AreEqual (-1, result, "#1");

				cmd.CommandText = "Insert into numeric_family (id,type_int) values (100,200)";
				result = cmd.ExecuteNonQuery ();
				Assert.AreEqual (1, result, "#2 One row shud be inserted");

				cmd.CommandText = "Update numeric_family set type_int=300 where id=100";
				result = cmd.ExecuteNonQuery ();
				Assert.AreEqual (1, result, "#3 One row shud be updated");

				// Test Batch Commands 
				cmd.CommandText = "Select id from numeric_family where id=1;";	
				cmd.CommandText += "update numeric_family set type_int=10 where id=1000";
				cmd.CommandText += "update numeric_family set type_int=10 where id=100";
				result = cmd.ExecuteNonQuery ();	
				Assert.AreEqual (1, result, "#4 One row shud be updated");
				
				cmd.CommandText = "Delete from numeric_family where id=100";
				result = cmd.ExecuteNonQuery ();
				Assert.AreEqual (1, result, "#5 One row shud be deleted");

			}finally {
				trans.Rollback ();
			}


			// Parameterized stored procedure calls

			int int_value = 20;
			string string_value = "output value changed";

			cmd.CommandText =
				"create procedure #tmp_executescalar_outparams " +
				" (@p1 int, @p2 int out, @p3 varchar(200) out) " +
				"as " +
				"select 'test' as 'col1', @p1 as 'col2' " +
				"set @p2 = @p2 * 2 " +
				"set @p3 = N'" + string_value + "' " +
				"select 'second rowset' as 'col1', 2 as 'col2' " +
				"return 1";

			cmd.CommandType = CommandType.Text;
			cmd.ExecuteNonQuery ();

			cmd.CommandText = "#tmp_executescalar_outparams";
			cmd.CommandType = CommandType.StoredProcedure;

			SqlParameter p1 = new SqlParameter ();
			p1.ParameterName = "@p1";
			p1.Direction = ParameterDirection.Input;
			p1.DbType = DbType.Int32;
			p1.Value = int_value;
			cmd.Parameters.Add (p1);

			SqlParameter p2 = new SqlParameter ();
			p2.ParameterName = "@p2";
			p2.Direction = ParameterDirection.InputOutput;
			p2.DbType = DbType.Int32;
			p2.Value = int_value;
			cmd.Parameters.Add (p2);

			SqlParameter p3 = new SqlParameter ();
			p3.ParameterName = "@p3";
			p3.Direction = ParameterDirection.Output;
			p3.DbType = DbType.String;
			p3.Size = 200;
			cmd.Parameters.Add (p3);

			cmd.ExecuteNonQuery ();
			Assert.AreEqual (int_value * 2, p2.Value, "#6 ExecuteNonQuery should fill the parameter collection with the outputted values");
			Assert.AreEqual (string_value, p3.Value, "#7 ExecuteNonQuery should fill the parameter collection with the outputted values");
		}

		[Test]
		public void ExecuteReaderTest ()
		{
			SqlDataReader reader = null; 
			conn = new SqlConnection (connectionString);

			// Test exception is thrown if conn is closed
			cmd = new SqlCommand ("Select count(*) from numeric_family");
			try {
				reader = cmd.ExecuteReader ();
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof(InvalidOperationException), e.GetType(),
					"#1 Incorrect Exception");
			}

			conn.Open ();
			// Test exception is thrown for Invalid Query
			cmd = new SqlCommand ("InvalidQuery", conn);
			try {
				reader = cmd.ExecuteReader ();
				Assert.Fail ("#1 Exception shud be thrown");
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof(SqlException), e.GetType (),
					"#2 Incorrect Exception : " + e);
			}
			
			// NOTE 	
			// Test SqlException is thrown if a row is locked 
			// should lock a particular row and then modify it
			/*
			*/
	
			// Test Connection  cannot be modified when reader is in use
			// NOTE : msdotnet contradicts documented behavior 	
			cmd.CommandText = "select * from numeric_family where id=1";
			reader = cmd.ExecuteReader ();
			reader.Read ();
			conn.Close (); // valid operation 
			conn = new SqlConnection (connectionString);

			/*
			// NOTE msdotnet contradcits documented behavior 
			// If the above testcase fails, then this shud be tested 	
			// Test connection can be modified once reader is closed
			conn.Close ();
			reader.Close ();
			conn = new SqlConnection (connectionString); // valid operation 
			*/
		}

		[Test]
		public void ExecuteReaderCommandBehaviorTest ()
		{
			// Test for command behaviors	
			DataTable schemaTable = null; 
			SqlDataReader reader = null; 

			conn = new SqlConnection (connectionString);
			conn.Open ();
			cmd = new SqlCommand ("", conn);
			cmd.CommandText = "Select id from numeric_family where id <=4 order by id asc;";
			cmd.CommandText += "Select type_bit from numeric_family where id <=4 order by id asc";

			// Test for default command behavior	
			reader = cmd.ExecuteReader ();
			int rows = 0; 
			int results = 0;
			do {
				while (reader.Read ())
					rows++ ; 
				Assert.AreEqual (4, rows, "#1 Multiple rows shud be returned");
				results++; 
				rows = 0;
			}while (reader.NextResult());
			Assert.AreEqual (2, results, "#2 Multiple result sets shud be returned");
			reader.Close ();

			// Test if closing reader, closes the connection 
			reader = cmd.ExecuteReader (CommandBehavior.CloseConnection);
			reader.Close ();
			Assert.AreEqual (ConnectionState.Closed, conn.State,
				"#3 Command Behavior is not followed");
			conn.Open(); 

			// Test if row info and primary Key info is returned
			reader = cmd.ExecuteReader (CommandBehavior.KeyInfo);
			schemaTable = reader.GetSchemaTable ();
			Assert.IsTrue(reader.HasRows, "#4 Data Rows shud also be returned");
			Assert.IsTrue ((bool)schemaTable.Rows[0]["IsKey"],
				"#5 Primary Key info shud be returned");
			reader.Close ();	

			// Test only column information is returned 
			reader = cmd.ExecuteReader (CommandBehavior.SchemaOnly);
			schemaTable = reader.GetSchemaTable ();
			Assert.IsFalse (reader.HasRows, "#6 row data shud not be returned");
			Assert.AreEqual(DBNull.Value, schemaTable.Rows[0]["IsKey"],
				"#7 Primary Key info shud not be returned");
			Assert.AreEqual ("id", schemaTable.Rows[0]["ColumnName"],
				"#8 Schema Data is Incorrect");
			reader.Close ();

			// Test only one result set (first) is returned 
			reader = cmd.ExecuteReader (CommandBehavior.SingleResult);
			schemaTable = reader.GetSchemaTable ();
			Assert.IsFalse (reader.NextResult(), 
				"#9 Only one result set shud be returned");
			Assert.AreEqual ("id", schemaTable.Rows[0]["ColumnName"],
				"#10 The result set returned shud be the first result set");
			reader.Close ();

			// Test only one row is returned for all result sets 
			// msdotnet doesnt work correctly.. returns only one result set
			reader = cmd.ExecuteReader (CommandBehavior.SingleRow);
			rows=0;
			results=0;
			do {
				while (reader.Read ())
					rows++ ; 
				Assert.AreEqual (1, rows, "#11 Only one row shud be returned");
				results++; 
				rows = 0;
			}while (reader.NextResult());
			// NOTE msdotnet contradicts documented behavior.
			// Multiple result sets shud be returned , and in this case : 2 
			//Assert.AreEqual (2, results, "# Multiple result sets shud be returned");
			Assert.AreEqual (2, results, "#12 Multiple result sets shud be returned");
			reader.Close ();
		}

		[Test]
		public void PrepareTest_CheckValidStatement ()
		{
			cmd = new SqlCommand ();
			conn = new SqlConnection (connectionString);
			conn.Open ();
			
			cmd.CommandText = "Select id from numeric_family where id=@ID" ; 
			cmd.Connection = conn ; 

			// Test if Parameters are correctly populated 
			cmd.Parameters.Clear ();
			cmd.Parameters.Add ("@ID", SqlDbType.TinyInt);
			cmd.Parameters["@ID"].Value = 2 ;
			cmd.Prepare ();
			Assert.AreEqual (2, cmd.ExecuteScalar (), "#3 Prepared Stmt not working");

			cmd.Parameters[0].Value = 3;
			Assert.AreEqual (3, cmd.ExecuteScalar (), "#4 Prep Stmt not working");
			conn.Close ();
		}

		[Test]
		public void PrepareTest ()
		{
			cmd = new SqlCommand ();
			conn = new SqlConnection (connectionString);
			conn.Open ();
			
			cmd.CommandText = "Select id from numeric_family where id=@ID" ; 
			cmd.Connection = conn ; 

			// Test InvalidOperation Exception is thrown if Parameter Type
			// is not explicitly set
			cmd.Parameters.Add ("@ID", 2);
			try {
				cmd.Prepare ();
				Assert.Fail ("#1 Parameter Type shud be explicitly Set");
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof(InvalidOperationException), e.GetType (),
					"#2 Incorrect Exception : " + e.StackTrace);
			}

			// Test Exception is thrown for variable size data  if precision/scale
			// is not set
			cmd.CommandText = "select type_varchar from string_family where type_varchar=@p1";
			cmd.Parameters.Clear ();
			cmd.Parameters.Add ("@p1", SqlDbType.VarChar);
			cmd.Parameters["@p1"].Value = "afasasadadada";
			try {
				cmd.Prepare ();
				Assert.Fail ("#5 Exception shud be thrown");
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof(InvalidOperationException), e.GetType(),
					"#6 Incorrect Exception " + e.StackTrace);
			}

 			// Test Exception is not thrown for Stored Procs 
			try {
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = "ABFSDSFSF" ;
				cmd.Prepare ();
			}catch (Exception e) {
				Assert.Fail ("#7 Exception shud not be thrown for Stored Procs");
			}
			cmd.CommandType = CommandType.Text;	
			conn.Close ();

			//Test InvalidOperation Exception is thrown if connection is not set
			cmd.Connection = null; 
			try {
				cmd.Prepare ();
				Assert.Fail ("#8 InvalidOperation Exception shud be thrown");
			}catch (AssertionException e) {
				throw e; 
			}catch (Exception e) {
				Assert.AreEqual (typeof(InvalidOperationException), e.GetType(),
					"#9 Incorrect Exception : " + e.StackTrace);
			}

			//Test InvalidOperation Exception is thrown if connection is closed
			cmd.Connection = conn ;
			try{
				cmd.Prepare ();
				Assert.Fail ("#4 InvalidOperation Exception shud be thrown");
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof(InvalidOperationException), e.GetType(),
					"Incorrect Exception : " + e.StackTrace);
			}
		}

		[Test]
		public void ResetTimeOut ()
		{
			SqlCommand cmd = new SqlCommand ();
			cmd.CommandTimeout = 50 ;
			Assert.AreEqual ( cmd.CommandTimeout, 50,
				"#1 CommandTimeout should be modfiable"); 
			cmd.ResetCommandTimeout ();
			Assert.AreEqual (cmd.CommandTimeout, 30,
				"#2 Reset Should set the Timeout to default value");
		}

		[Test]
		[ExpectedException (typeof(ArgumentException))]
		public void CommandTimeout ()
		{
			cmd = new SqlCommand ();
			cmd.CommandTimeout = 10; 
			Assert.AreEqual (10, cmd.CommandTimeout, "#1");
			cmd.CommandTimeout = -1;
		}
		
		[Test]
		[ExpectedException (typeof(ArgumentException))]
		public void CommandTypeTest ()
		{
			cmd = new SqlCommand ();
			Assert.AreEqual (CommandType.Text ,cmd.CommandType,
				"Default CommandType is text");
			cmd.CommandType = (CommandType)(-1);	
		}
		
		[Test]
		[Ignore ("msdotnet contradicts documented behavior")]
		[ExpectedException (typeof(InvalidOperationException))]
		public void ConnectionTest ()
		{
			SqlTransaction trans = null; 
			try {
				conn = new SqlConnection (connectionString);
				conn.Open ();
				trans = conn.BeginTransaction ();
				cmd = new SqlCommand ("", conn,trans);
				cmd.CommandText = "Select id from numeric_family where id=1";
				cmd.Connection = new SqlConnection ();
			}finally {
				trans.Rollback();
				conn.Close ();
			}
		}
		
		[Test]
		public void TransactionTest ()
		{
			conn = new SqlConnection (connectionString);
			cmd = new SqlCommand ("", conn);
			Assert.IsNull (cmd.Transaction, "#1 Default value is null");
		
			SqlConnection conn1 = new SqlConnection (connectionString);
			conn1.Open ();
			SqlTransaction trans1 = conn1.BeginTransaction ();
			cmd.Transaction = trans1 ; 
			try {
				cmd.ExecuteNonQuery (); 
				Assert.Fail ("#2 Connection cannot be different");
			}catch (Exception e) {
				Assert.AreEqual (typeof(InvalidOperationException), e.GetType(),
					"#3 Incorrect Exception : " + e);
			}finally {
				conn1.Close ();
				conn.Close ();
			}
		}

		// Need to add more tests
		[Test]
		[ExpectedException (typeof(ArgumentException))]
		public void UpdatedRowSourceTest ()
		{
			cmd = new SqlCommand ();
			Assert.AreEqual (UpdateRowSource.Both, cmd.UpdatedRowSource,
				"#1 Default value is both");
			cmd.UpdatedRowSource = UpdateRowSource.None;	
			Assert.AreEqual (UpdateRowSource.None, cmd.UpdatedRowSource,
				"#2");

			cmd.UpdatedRowSource = (UpdateRowSource) (-1);
		}

		[Test]
		public void ExecuteNonQueryTempProcedureTest () {
			conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				// create temp sp here, should normally be created in Setup of test 
				// case, but cannot be done right now because of ug #68978
				DBHelper.ExecuteNonQuery (conn, CREATE_TMP_SP_TEMP_INSERT_PERSON);
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = conn;
				cmd.CommandText = "#sp_temp_insert_employee";
				cmd.CommandType = CommandType.StoredProcedure;
				Object TestPar = "test";
				cmd.Parameters.Add("@fname", SqlDbType.VarChar);
				cmd.Parameters ["@fname"].Value = TestPar;
				Assert.AreEqual(1,cmd.ExecuteNonQuery());
			} finally {
				DBHelper.ExecuteNonQuery (conn, DROP_TMP_SP_TEMP_INSERT_PERSON);
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_person_table");
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		// Test for bug #76778
		// Test for a case, when query size is greater than the block size
		[Test]
		public void LongQueryTest ()
		{
			SqlConnection conn = new SqlConnection (
							connectionString + ";Pooling=false");
			using (conn) {
				conn.Open ();
				SqlCommand cmd = conn.CreateCommand ();
				String value =  new String ('a', 10000);
				cmd.CommandText = String.Format ("Select '{0}'", value); 
				cmd.ExecuteNonQuery ();
			}
		}

		// Test for bug #76778
		// To make sure RPC (when implemented) works ok.. 
		[Test]
		public void LongStoredProcTest()
		{
			SqlConnection conn = new SqlConnection (
							connectionString + ";Pooling=false");
			using (conn) {
				conn.Open ();
				int size = conn.PacketSize ; 
				SqlCommand cmd = conn.CreateCommand ();
				// create a temp stored proc .. 
				cmd.CommandText  = "Create Procedure #sp_tmp_long_params ";
				cmd.CommandText += "@p1 nvarchar (4000), ";
				cmd.CommandText += "@p2 nvarchar (4000), ";
				cmd.CommandText += "@p3 nvarchar (4000), ";
				cmd.CommandText += "@p4 nvarchar (4000) out ";
				cmd.CommandText += "As ";
				cmd.CommandText += "Begin ";
				cmd.CommandText += "Set @p4 = N'Hello' ";
				cmd.CommandText += "Return 2 "; 
				cmd.CommandText += "End"; 
				cmd.ExecuteNonQuery ();

				//execute the proc 
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = "#sp_tmp_long_params"; 

				String value =  new String ('a', 4000);
				SqlParameter p1 = new SqlParameter ("@p1",
							SqlDbType.NVarChar,4000);
				p1.Value = value;

				SqlParameter p2 = new SqlParameter ("@p2",
							SqlDbType.NVarChar,4000);
				p2.Value = value;

				SqlParameter p3 = new SqlParameter ("@p3",
							SqlDbType.NVarChar,4000);
				p3.Value = value;

				SqlParameter p4 = new SqlParameter ("@p4",
							SqlDbType.NVarChar,4000);
				p4.Direction = ParameterDirection.Output; 

				// for now, name shud be @RETURN_VALUE  
				// can be changed once RPC is implemented 
				SqlParameter p5 = new SqlParameter ("@RETURN_VALUE", SqlDbType.Int);
				p5.Direction = ParameterDirection.ReturnValue ;

				cmd.Parameters.Add (p1);
				cmd.Parameters.Add (p2);
				cmd.Parameters.Add (p3);
				cmd.Parameters.Add (p4);
				cmd.Parameters.Add (p5);

				cmd.ExecuteNonQuery ();
				Assert.AreEqual ("Hello", p4.Value, "#1");
				Assert.AreEqual (2, p5.Value, "#2");
			}
		}

		// Test for bug #76880
		[Test]
		public void DateTimeParameterTest ()
		{
			SqlConnection conn = new SqlConnection (connectionString); 
			using (conn) {
				conn.Open ();
				SqlCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "select * from datetime_family where type_datetime=@p1";
				cmd.Parameters.Add ("@p1", SqlDbType.DateTime).Value= "10-10-2005";
				// shudnt cause and exception
				SqlDataReader rdr = cmd.ExecuteReader ();
				rdr.Close ();
			}
		}

		/**
		 * Verifies whether an enum value is converted to a numeric value when
		 * used as value for a numeric parameter (bug #66630)
		 */
		[Test]
		public void EnumParameterTest() {
			conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				// create temp sp here, should normally be created in Setup of test 
				// case, but cannot be done right now because of ug #68978
				DBHelper.ExecuteNonQuery (conn, "CREATE PROCEDURE #Bug66630 (" 
							  + "@Status smallint = 7"
							  + ")"
							  + "AS" + Environment.NewLine
							  + "BEGIN" + Environment.NewLine
							  + "SELECT CAST(5 AS int), @Status" + Environment.NewLine
							  + "END");
				
				SqlCommand cmd = new SqlCommand("#Bug66630", conn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Status", SqlDbType.Int).Value = Status.Error;

				using (SqlDataReader dr = cmd.ExecuteReader()) {
					// one record should be returned
					Assert.IsTrue(dr.Read(), "EnumParameterTest#1");
					// we should get two field in the result
					Assert.AreEqual(2, dr.FieldCount, "EnumParameterTest#2");
					// field 1
					Assert.AreEqual("int", dr.GetDataTypeName(0), "EnumParameterTest#3");
					Assert.AreEqual(5, dr.GetInt32(0), "EnumParameterTest#4");
					// field 2
					Assert.AreEqual("smallint", dr.GetDataTypeName(1), "EnumParameterTest#5");
					Assert.AreEqual((short) Status.Error, dr.GetInt16(1), "EnumParameterTest#6");
					// only one record should be returned
					Assert.IsFalse(dr.Read(), "EnumParameterTest#7");
				}
			} finally {
				DBHelper.ExecuteNonQuery (conn, "if exists (select name from sysobjects " +
							  " where name like '#temp_Bug66630' and type like 'P') " +
							  " drop procedure #temp_Bug66630; ");
				ConnectionManager.Singleton.CloseConnection ();
			}
		}

		/**
		 * The below test does not need a connection but since the setup opens 
		 * the connection i will need to close it
		 */
		[Test]
		public void CloneTest() {
			ConnectionManager.Singleton.OpenConnection ();
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = null;
			cmd.CommandText = "sp_insert";
			cmd.CommandType = CommandType.StoredProcedure;
			Object TestPar = DBNull.Value;
			cmd.Parameters.Add("@TestPar1", SqlDbType.Int);
			cmd.Parameters["@TestPar1"].Value = TestPar;
			cmd.Parameters.Add("@BirthDate", DateTime.Now);
			cmd.DesignTimeVisible = true;
			cmd.CommandTimeout = 100;
			Object clone1 = ((ICloneable)(cmd)).Clone();
			SqlCommand cmd1 = (SqlCommand) clone1;
			Assert.AreEqual(2, cmd1.Parameters.Count);
			Assert.AreEqual(100, cmd1.CommandTimeout);
			cmd1.Parameters.Add("@test", DateTime.Now);
			// to check that it is deep copy and not a shallow copy of the
			// parameter collection
			Assert.AreEqual(3, cmd1.Parameters.Count);
			Assert.AreEqual(2, cmd.Parameters.Count);
		}

		private enum Status { 
			OK = 0,
			Error = 3
		}

		private readonly string CREATE_TMP_SP_TEMP_INSERT_PERSON = ("create procedure #sp_temp_insert_employee ( " + Environment.NewLine + 
									    "@fname varchar (20)) " + Environment.NewLine + 
									    "as " + Environment.NewLine + 
									    "begin" + Environment.NewLine + 
									    "declare @id int;" + Environment.NewLine + 
									    "select @id = max (id) from employee;" + Environment.NewLine + 
									    "set @id = @id + 6000 + 1;" + Environment.NewLine + 
									    "insert into employee (id, fname, dob, doj) values (@id, @fname, '1980-02-11', getdate ());" + Environment.NewLine + 
									    "return @id;" + Environment.NewLine + 
									    "end");

		private readonly string DROP_TMP_SP_TEMP_INSERT_PERSON = ("if exists (select name from sysobjects where " + Environment.NewLine + 
									  "name = '#sp_temp_insert_employee' and type = 'P') " + Environment.NewLine + 
									  "drop procedure #sp_temp_insert_employee; ");
	}
}

