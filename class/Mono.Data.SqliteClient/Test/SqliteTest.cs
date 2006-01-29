//
// SqliteTest.cs - Test for the Sqlite ADO.NET Provider in Mono.Data.SqliteClient
//                 This provider works on Linux and Windows and uses the native
//                 sqlite.dll or sqlite.so library.
//
// Modify or add to this test as needed...
//
// SQL Lite can be downloaded from
// http://www.hwaci.com/sw/sqlite/download.html
//
// There are binaries for Windows and Linux.
//
// To compile:
//  mcs SqliteTest.cs -r System.Data.dll -r Mono.Data.SqliteClient.dll
//
// Author:
//     Daniel Morgan <danmorg@sc.rr.com>
//

using System;
using System.Data;
using Mono.Data.SqliteClient;

namespace Test.Mono.Data.SqliteClient
{
	class SqliteTest
	{
		[STAThread]
		static void Main(string[] args)
		{
			Test(false, false);
			Console.WriteLine();
			Test(false, true);
			Console.WriteLine();
			Test(true, false);
		}
		
		static void Test(bool v3, bool utf8) {
			if (!v3)
				Console.WriteLine("Testing Version 2" + (utf8 ? " with UTF-8 encoding" : ""));
			else
				Console.WriteLine("Testing Version 3");
				
			System.IO.File.Delete("SqliteTest.db");
		
			SqliteConnection dbcon = new SqliteConnection();
			
			// the connection string is a URL that points
			// to a file.  If the file does not exist, a 
			// file is created.

			// "URI=file:some/path"
			string connectionString =
				"URI=file:SqliteTest.db";
			if (v3)
				connectionString += ",Version=3";
			if (utf8)
				connectionString += ",encoding=UTF-8";
			dbcon.ConnectionString = connectionString;
				
			dbcon.Open();

			SqliteCommand dbcmd = new SqliteCommand();
			dbcmd.Connection = dbcon;
			
			dbcmd.CommandText = 
				"CREATE TABLE MONO_TEST ( " +
				"NID INT, " +
				"NDESC TEXT, " +
				"NTIME DATETIME); " +
				"INSERT INTO MONO_TEST  " +
				"(NID, NDESC, NTIME) " +
				"VALUES(1,'One" + ((v3 || utf8) ? " (unicode test: \u05D0)" : "") + "', '2006-01-01')";
			Console.WriteLine("Create & insert modified rows = 1: " + dbcmd.ExecuteNonQuery());

			dbcmd.CommandText =
				"INSERT INTO MONO_TEST  " +
				"(NID, NDESC, NTIME) " +
				"VALUES(:NID,:NDESC,:NTIME)";
			dbcmd.Parameters.Add( new SqliteParameter("NID", 2) );
			dbcmd.Parameters.Add( new SqliteParameter(":NDESC", "Two" + ((v3 || utf8) ? " (unicode test: \u05D1)" : "")) );
			dbcmd.Parameters.Add( new SqliteParameter(":NTIME", DateTime.Now) );
			Console.WriteLine("Insert modified rows with parameters = 1, 2: " + dbcmd.ExecuteNonQuery() + " , " + dbcmd.LastInsertRowID());

			dbcmd.CommandText =
				"INSERT INTO MONO_TEST  " +
				"(NID, NDESC, NTIME) " +
				"VALUES(3,'Three, quoted parameter test, and next is null; :NTIME', NULL)";
			Console.WriteLine("Insert with null modified rows and ID = 1, 3: " + dbcmd.ExecuteNonQuery() + " , " + dbcmd.LastInsertRowID());

			dbcmd.CommandText =
				"SELECT * FROM MONO_TEST";
			SqliteDataReader reader;
			reader = dbcmd.ExecuteReader();

			Console.WriteLine("read and display data...");
			while(reader.Read())
				for (int i = 0; i < reader.FieldCount; i++)
					Console.WriteLine(" Col {0}: {1} (type: {2}, data type: {3})",
						i, reader[i] == null ? "(null)" : reader[i].ToString(), reader[i] == null ? "(null)" : reader[i].GetType().FullName, reader.GetDataTypeName(i));

			dbcmd.CommandText = "SELECT NDESC FROM MONO_TEST WHERE NID=2";
			Console.WriteLine("read and display a scalar = 'Two': " + dbcmd.ExecuteScalar());

			dbcmd.CommandText = "SELECT count(*) FROM MONO_TEST";
			Console.WriteLine("read and display a non-column scalar = 3: " + dbcmd.ExecuteScalar());

			Console.WriteLine("read and display data using DataAdapter/DataSet...");
			SqliteDataAdapter adapter = new SqliteDataAdapter("SELECT * FROM MONO_TEST", connectionString);
			DataSet dataset = new DataSet();
			adapter.Fill(dataset);
			foreach(DataTable myTable in dataset.Tables){
				foreach(DataRow myRow in myTable.Rows){
					foreach (DataColumn myColumn in myTable.Columns){
						Console.WriteLine(" " + myRow[myColumn]);
					}
				}
			}

			/*Console.WriteLine("read and display data using DataAdapter/DataTable...");
			DataTable dt = new DataTable();
			adapter.Fill(dt);
			DataView dv = new DataView(dt);
			foreach (DataRowView myRow in dv) {
				foreach (DataColumn myColumn in myRow.Row.Table.Columns) {
					Console.WriteLine(" " + myRow[myColumn.ColumnName]);
				}
			}*/
       		       		            
			try {
				dbcmd.CommandText = "SELECT NDESC INVALID SYNTAX FROM MONO_TEST WHERE NID=2";
				dbcmd.ExecuteNonQuery();
				Console.WriteLine("Should not reach here.");
			} catch (Exception e) {
				Console.WriteLine("Testing a syntax error: " + e.GetType().Name + ": " + e.Message);
			}

			/*try {
				dbcmd.CommandText = "SELECT 0/0 FROM MONO_TEST WHERE NID=2";
				Console.WriteLine("Should not reach here: " + dbcmd.ExecuteScalar());
			} catch (Exception e) {
				Console.WriteLine("Testing an execution error: " + e.GetType().Name + ": " + e.Message);
			}*/

			dataset.Dispose();
			adapter.Dispose();
			reader.Close();
			dbcmd.Dispose();
			dbcon.Close();
		}
	}
}
