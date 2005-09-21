//
// SqlDataAdapterTest.cs - NUnit Test Cases for testing the
//                          SqlDataAdapter class
// Author:
//      Senganal T (tsenganal@novell.com)
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
using System.Data.SqlClient;
using System.Net;
using NUnit.Framework;
using System.Collections;

namespace MonoTests.System.Data
{
	[TestFixture]
	[Category ("sqlserver")]
	public class SqlConnectionTest
	{
		SqlConnection conn = null ; 
		String connectionString = ConnectionManager.Singleton.ConnectionString;

		ArrayList invalidConnectionStrings = null;
		int stateChangeEventCount = 0;
		int disposedEventCount = 0;
		int infoMessageEventCount = 0;

		void populateTestData () 
		{
			invalidConnectionStrings = new ArrayList ();
			// shud be got from  a config file .. 
			//list of invalid and valid conn strings; 
			invalidConnectionStrings.Add ("InvalidConnectionString");
			invalidConnectionStrings.Add ("invalidKeyword=10");
			invalidConnectionStrings.Add ("Packet Size=511");
			invalidConnectionStrings.Add ("Packet Size=32768");
			invalidConnectionStrings.Add ("Connect Timeout=-1");			
			invalidConnectionStrings.Add ("Max Pool Size=-1");
			invalidConnectionStrings.Add ("Min Pool Size=-1");				
		}

		[SetUp]
		public void SetUp ()
		{
		}

		[TearDown]
		public void TearDown ()
		{
			if (conn != null)
				conn.Dispose ();
		}

		[Test]
		public void DefaultConstructorTest ()
		{
			SqlConnection conn = new SqlConnection ();  
			Assert.AreEqual ("", conn.ConnectionString, 
					"#1 Default Connection String should be empty");
			Assert.AreEqual (15, conn.ConnectionTimeout, 
					"#2 Default ConnectionTimeout should be 15" ); 
			Assert.AreEqual ("", conn.Database, 
					"#3 Default Database should be empty");
			Assert.AreEqual ("", conn.DataSource,
					"#4 Default DataSource should be empty");
			Assert.AreEqual (8192, conn.PacketSize,"#5 Default Packet Size is 8192");
			Assert.AreEqual (Dns.GetHostName(), conn.WorkstationId, 
					"#6 Default Workstationid shud be hostname");
			Assert.AreEqual (ConnectionState.Closed, conn.State, 
					"#7 Connection State shud be closed by default");
		}

		[Test]
		public void OverloadedConstructorTest ()
		{
			// Test Exceptions are thrown for Invalid Connection Strings
			int count=0 ;
			populateTestData ();
			foreach (String invalidConnString in invalidConnectionStrings) {
				count++;
				try {
					conn = new SqlConnection ((string)invalidConnString);
					Assert.Fail ("#1 Exception must be thrown");
				}catch (AssertionException e) {
					throw e; 
				}catch (Exception e) {
					Assert.AreEqual (typeof(ArgumentException), e.GetType(), 
						"Incorrect Exception" + e.StackTrace);
				}
			}

			//check synonyms..
			//do i need to check for all the synonyms.. 
			conn = new SqlConnection (
					"Timeout=10;Connect Timeout=20;Connection Timeout=30");
			Assert.AreEqual (30, conn.ConnectionTimeout,
				"## The last set value shud be taken");
			conn = new SqlConnection (
					"Connect Timeout=100;Connection Timeout=200;Timeout=300");
			Assert.AreEqual (300, conn.ConnectionTimeout,
				"## The last set value shud be taken");
			conn = new SqlConnection (
					"Connection Timeout=1000;Timeout=2000;Connect Timeout=3000");
			Assert.AreEqual (3000, conn.ConnectionTimeout,
				"## The last set value shud be taken");

			// Test if properties are set correctly
			
			//'==' doesent work correctly in both msdotnet and mono
			/*
			conn = new SqlConnection ("server=local==host;database=tmp;");
			Assert.AreEqual ("local==host", conn.DataSource, 
				"# Datasource name is set incorrectly");
			*/
			string connStr = "Server='loca\"lhost';Database='''Db'; packet Size=\"512\";";
			connStr += "connect Timeout=20;Workstation Id=\"'\"\"desktop\";";
			conn = new SqlConnection (connStr);
			Assert.AreEqual (connStr , conn.ConnectionString , "#1");
			Assert.AreEqual ("loca\"lhost" , conn.DataSource , "#2");
			Assert.AreEqual ("'Db" , conn.Database , "#3");
			Assert.AreEqual (512 , conn.PacketSize , "#4");
			Assert.AreEqual (20 , conn.ConnectionTimeout , "#5");
			Assert.AreEqual ("'\"desktop" , conn.WorkstationId , "#6");
			Assert.AreEqual (ConnectionState.Closed , conn.State , "#7");
		}

		[Test]
		public void OpenTest ()
		{
			conn = new SqlConnection (connectionString);
			ArrayList validIncorrectConnStrings = new ArrayList();
			string validConnString = connectionString;

			validIncorrectConnStrings.Add (
					validConnString+"user id=invalidLogin");
			validIncorrectConnStrings.Add (
					validConnString+"database=invalidDB");
			validIncorrectConnStrings.Add (
					validConnString+";password=invalidPassword");
			validIncorrectConnStrings.Add (
					validConnString+";server=invalidServerName");

			int count=0;
			foreach (string connString in validIncorrectConnStrings) {
				count++;
				try {
					conn.ConnectionString = connString;
					conn.Open();
					Assert.AreEqual (typeof (SqlException), null,
						string.Format (
							"#1_{0} Incorrect Connection String",count));				
				}catch (AssertionException e) {
					throw e;
				}catch (Exception e) {
					Assert.AreEqual (typeof (SqlException), e.GetType (),
						"#2 Incorrect Exception" + e.StackTrace);
				}
			}
			
			// Test connection is Opened for a valid Connection String
			conn.ConnectionString = connectionString; 
			conn.Open ();
			Assert.AreEqual (ConnectionState.Open, conn.State,
				"#3 Connection State Should be OPEN");

			// Test Exception is thrown on opening an OPEN Connection 
			try {
				conn.Open ();
				Assert.AreEqual (typeof (InvalidOperationException), null,
					"#1 Connection is Already Open");
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof(InvalidOperationException), e.GetType (),
					 "#2 Incorect Exception."); 
			}
			conn.Close();				

			/*
			// Test if localhost is assumed when servername is empty/missing
			// NOTE : msdotnet contradicts doc

			Assumes the server is localhost .. need to test this with mono on windows 
			conn.ConnectionString = connectionString + "server=;";
			try {
				conn.Open ();
			}catch (Exception e) {
				Assert.Fail ("## If server name is not given or empty ,localhost shud be tried");
			}
			ex = null; 
			conn.Close ();
			 */
		}

		[Test]
		public void CreateCommandTest ()
		{
			conn = new SqlConnection (connectionString);
			IDbCommand cmd = conn.CreateCommand ();
			Assert.AreSame (conn, cmd.Connection,
				"#1 Connection instance should be the same");
		}

		[Test]
		public void CloseTest ()
		{	
			conn = new SqlConnection (connectionString);
			conn.Open ();
			Assert.AreEqual (ConnectionState.Open, conn.State,
				"#1 Connection State should be : Open");
			conn.Close ();
			Assert.AreEqual (ConnectionState.Closed, conn.State,
				"#1 Connection State Should : Closed");
			// Test Closing an already closed connection is Valid..
			conn.Close ();
		}

		[Test]
		public void DisposeTest ()
		{
			SqlConnection conn = new SqlConnection (connectionString);
			conn.Dispose ();
			Assert.AreEqual ("", conn.ConnectionString, 
				"#1 Dispose shud make the Connection String empty");
			Assert.AreEqual (15, conn.ConnectionTimeout, 
				"#2 Default ConnectionTimeout : 15" ); 
			Assert.AreEqual ("", conn.Database,
				"#3 Default Database : empty");
			Assert.AreEqual ("", conn.DataSource, 
				"#4 Default DataSource : empty");
			Assert.AreEqual (8192, conn.PacketSize, 
				"#5 Default Packet Size : 8192");
			Assert.AreEqual (Dns.GetHostName(), conn.WorkstationId, 
				"#6 Default Workstationid : hostname");
			Assert.AreEqual (ConnectionState.Closed, conn.State, 
				"#7 Default State : CLOSED ");
		}

		[Test]
		public void ChangeDatabaseTest ()
		{
			conn = new SqlConnection (connectionString);
			String database = conn.Database;

			//Test if exception is thrown if connection is closed
			try {
				conn.ChangeDatabase ("database");
				Assert.AreEqual (typeof (InvalidOperationException), null,
					"#1 Connection is Closed");
			}catch (AssertionException e){
				throw e; 
			}catch (Exception e) {
				Assert.AreEqual (typeof (InvalidOperationException), e.GetType(),
					"#2 Incorrect Exception : " + e.StackTrace);
			}

			//Test if exception is thrown for invalid Database Names 
			//need to add more to the list 
			conn.Open ();
			String[] InvalidDatabaseNames = {"", null, "       "};
			for (int i = 0; i < InvalidDatabaseNames.Length ; ++i) {
				try {
					conn.ChangeDatabase (InvalidDatabaseNames[i]);
					Assert.AreEqual (typeof (ArgumentException), null,
						string.Format ("#3_{0} Exception not thrown",i));
				}catch (AssertionException e) {
					throw e;
				}catch (Exception e) {
					Assert.AreEqual (typeof(ArgumentException), e.GetType (),
						string.Format( "#4_{0} Incorrect Exception : {1}",
							i, e.StackTrace));
				}
				Assert.AreEqual (database, conn.Database,
					"#4 The Database shouldnt get changed if Operation Failed");
			}
			
			//Test if exception is thrown if database name is non-existent
			try {
				conn.ChangeDatabase ("invalidDB");
				Assert.Fail ("#5 Exception must be thrown if database doesent exist");
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof(SqlException), e.GetType (),
					"#6 Incorrect Exception" + e.StackTrace);
			}
			conn.Close (); 

			//Test if '-' is a valid character in a database name
			//TODO : Check for database names that have more special Characters..
			conn.ConnectionString = connectionString;
			conn.Open ();
			try {
				conn.ChangeDatabase ("mono-test");
				Assert.AreEqual ("mono-test", conn.Database,
					"#7 Database name should be mono-test");
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e){
				Assert.Fail ("#8 Unexpected Exception : DB Name can have a '-' : "
					 + e);
			}
		}

		[Test]
		public void BeginTransactionTest()
		{
			conn = new SqlConnection (connectionString);
			SqlTransaction trans = null ;

			try {
				trans = conn.BeginTransaction ();
				Assert.Fail ("#1 Connection must be Open to Begin a Transaction");
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof (InvalidOperationException), e.GetType(),
					"#2 Incorrect Exception" + e.StackTrace);
			}

			conn.Open ();
			trans = conn.BeginTransaction ();
			Assert.AreSame (conn, trans.Connection, 
					"#3 Transaction should reference the same connection");
			Assert.AreEqual (IsolationLevel.ReadCommitted, trans.IsolationLevel,
					"#4 Isolation Level shud be ReadCommitted");
			trans.Rollback ();

			try {
				trans = conn.BeginTransaction ();
				trans = conn.BeginTransaction ();
				conn.BeginTransaction ();
				Assert.Fail ("#5 Parallel Transactions are not supported");
			}catch (AssertionException e) {
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof(InvalidOperationException), e.GetType(),
					"#6 Incorrect Exception" + e.StackTrace); 
			}finally {
				trans.Rollback();
			}

			try {
				trans = conn.BeginTransaction ();
				trans.Rollback ();
				trans = conn.BeginTransaction ();
				trans.Commit();
				trans = conn.BeginTransaction ();
			}catch (Exception e) {
				Assert.Fail ("#7 Transaction can be opened after a rollback/commit");
			}finally {
				trans.Rollback ();
			}
		}

		[Test]
		public void ConnectionStringPropertyTest ()
		{
			conn = new SqlConnection (connectionString) ;
			// Test Repeated Keyoword ..Should take the latest value 
			conn.ConnectionString = conn.ConnectionString + ";server=RepeatedServer;" ;
			Assert.AreEqual ("RepeatedServer", ((SqlConnection)conn).DataSource,
					"#1 if keyword is repeated, the latest value should be taken");
			conn.ConnectionString += ";database=gen;Initial Catalog=gen1";
			Assert.AreEqual ("gen1", conn.Database,
					"#2 database and initial catalog are synonyms .. ");

			// Test if properties are set correctly 
			string str = "server=localhost1;database=db;user id=user;";
			str += "password=pwd;Workstation ID=workstation;Packet Size=512;";
			str += "Connect Timeout=10";
			conn.ConnectionString = str;

			Assert.AreEqual ("localhost1", conn.DataSource,
					"#3 DataSource name should be same as passed");
			Assert.AreEqual ("db", conn.Database,
					"#4 Database name shud be same as passed");
			Assert.AreEqual (ConnectionState.Closed, conn.State,
					"#5 Connection shud be in closed state");
			Assert.AreEqual ("workstation", conn.WorkstationId,
					"#6 Workstation Id shud be same as passed");
			Assert.AreEqual (512, conn.PacketSize,
					"#7 Packetsize shud be same as passed");
			Assert.AreEqual (10, conn.ConnectionTimeout,
					"#8 ConnectionTimeout shud be same as passed");
			
			// Test if any leftover values exist from previous invocation. 
			conn.ConnectionString = connectionString;
			conn.ConnectionString = "";
			Assert.AreEqual ("", conn.DataSource,
					"#9 Datasource shud be reset to Default : Empty");
			Assert.AreEqual ("", conn.Database, 
					"#10 Database shud reset to Default : Empty");
			Assert.AreEqual (8192, conn.PacketSize, 
					"#11 Packetsize shud be reset to Default : 8192");
			Assert.AreEqual (15, conn.ConnectionTimeout, 
					"#12 ConnectionTimeour shud be reset to Default : 15");
			Assert.AreEqual (Dns.GetHostName (), conn.WorkstationId,
					"#13 WorkstationId shud be reset to Default : Hostname");
			
			// Test Argument Exception is thrown for Invalid Connection Strings
			foreach (string connString in invalidConnectionStrings) {
				try {
					conn.ConnectionString = connString;
					Assert.Fail (
						"#14 Exception should be thrown");
				}catch (AssertionException e){
					throw e;
				}catch (Exception e) {
					Assert.AreEqual (typeof (ArgumentException), e.GetType(),
						"#15 Incorrect Exception" + e.StackTrace);
				}
			}

			// Test if ConnectionString is read-only when Connection is OPEN
			conn.ConnectionString = connectionString;
			conn.Open() ;
			try {
				Assert.AreEqual (conn.State, ConnectionState.Open,
						"#16 Connection shud be open");
				conn.ConnectionString =  "server=localhost;database=tmp;" ;
				Assert.Fail (
					"#17 ConnectionString should Read-Only when Connection is Open");
			}catch (AssertionException e){
				throw e;
			}catch (Exception e) {
				Assert.AreEqual (typeof(InvalidOperationException), e.GetType(),
					"#18 Incorrect Exception" + e.StackTrace); 
			}
			conn.Close ();
		}

		[Test]
		public void ServerVersionTest ()
		{	
			conn = new SqlConnection (connectionString);

			// Test InvalidOperation Exception is thrown if Connection is CLOSED
			try{
				string s = conn.ServerVersion;
				Assert.Fail (
					"#1 InvalidOperation Exception Must be thrown if conn is closed");
			}catch (AssertionException e){
				throw e;
			}catch (Exception e){
				Assert.AreEqual(typeof (InvalidOperationException), e.GetType (),
					"#2 Incorrect Exception" + e.StackTrace);
			}
			
			// Test if Release Version is as per specification.
			conn.Open ();
			String[] version = conn.ServerVersion.Split ('.') ;
			Assert.AreEqual (2, version[0].Length,
				"#2 The Major release shud be exactly 2 characters");
			Assert.AreEqual (2, version[1].Length,
				"#3 The Minor release shud be exactly 2 characters");
			Assert.AreEqual (4, version[2].Length,
				"#4 The Release version should be exactly 4 digits");
		}

		[Test]
		public void DatabasePropertyTest ()
		{
			conn = new SqlConnection (connectionString);
			string database = conn.Database ;

			// Test if database property is updated when a query changes database
			conn.Open ();
			SqlCommand cmd = new SqlCommand ("use [mono-test]" , conn);
			cmd.ExecuteNonQuery ();
			Assert.AreEqual ("mono-test", conn.Database,
				"#1 DATABASE name shud change if query changes the db");
			conn.Close ();
			Assert.AreEqual (database, conn.Database,
				"#2 Shud be back to default value");

			// Test if the database property is reset on re-opening the connection
			conn.ConnectionString = connectionString;
			conn.Open ();	
			Assert.AreEqual (database, conn.Database,
				"#3 Shud be back to original value");
			conn.Close ();
		}

		[Test]
		public void StateChangeEventTest () 
		{
			conn = new SqlConnection (connectionString);
			conn.StateChange += new StateChangeEventHandler (
							StateChangeHandlerTest1);
			using (conn) {
				conn.Open ();
			}
			Assert.AreEqual (2, stateChangeEventCount,
				"#1 The handler shud be called twice");
			stateChangeEventCount =0 ; 
			conn.StateChange -= new StateChangeEventHandler (
							StateChangeHandlerTest1);
			// NOTE : Need to check  the behavior if an exception is raised 
			// in a handler 
		}

		[Test]
		public void DisposedEventTest ()
		{
			conn = new SqlConnection (connectionString);
			conn.Disposed += new EventHandler (DisposedEventHandlerTest1);
			conn.Dispose ();
			Assert.AreEqual (1, disposedEventCount,
				 "#1 Disposed eventhandler shud be called");
		}

		void StateChangeHandlerTest1 (object sender , StateChangeEventArgs e)
		{
			Assert.IsTrue ((e.CurrentState != e.OriginalState),
				"#1 Current and Original state shud be different");
			Assert.AreEqual (e.CurrentState, conn.State,
				"The conn state and the arg received in event shud be same");
			stateChangeEventCount++ ;
		}

		void DisposedEventHandlerTest1 (object sender , EventArgs e)
		{
			disposedEventCount++; 
		}
	}
}
