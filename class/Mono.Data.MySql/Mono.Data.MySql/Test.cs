// 
// Test.cs
//
// Used to Test the C# bindings to MySQL.  This test
// is based on the test that comes with MySQL.
// Part of the C# bindings to MySQL library libMySQL.dll
//
// Author:
//    Brad Merrill <zbrad@cybercom.net>
//
// (C)Copyright 2002 Brad Merril
//
// http://www.cybercom.net/~zbrad/DotNet/MySql/
//
// Mono has gotten permission from Brad Merrill to include in 
// the Mono Class Library
// his C# bindings to MySQL under the X11 License
//
// Mono can be found at http://www.go-mono.com/
// The X11/MIT License can be found 
// at http://www.opensource.org/licenses/mit-license.html
//

using System;
using System.Runtime.InteropServices;
using Mono.Data.MySql;

///<remarks>
///<para>
/// MySql P/Invoke implementation test program
/// Brad Merrill
/// 3-Mar-2002
///</para>
///<para>
/// This is based on the myTest.c program in the
/// libmysqltest example directory of the mysql distribution.
///</para>
///<para>
/// I noticed during implementation that the C api libraries are
/// thread sensitive, in that they store information in the
/// currently executing thread local storage.  This is
/// incompatible with the thread pool model in the CLR, in which
/// you could be executing the same context on different
/// threads.
///</para>
///<para>
/// A better implementation would be to rewrite the libmysql
/// layer in managed code, and do all the socket APIs, and mysql
/// protocol using the .NET Framework APIs.  However, that's a
/// bit more than a weekend of work.
///</para>
///</remarks>
public class Test {
	[STAThread]
	public static void Main() {
		string myDb = "mysql";
		string myStmt = "SELECT * FROM db";

		IntPtr db = MySql.Init(IntPtr.Zero);
		if (db == IntPtr.Zero)
			throw new ApplicationException("?Init failed");

		IntPtr conn = MySql.Connect(db, null, null, null, null, MySql.Port,
			null, (uint)0);
		if (conn == IntPtr.Zero) {
			Console.WriteLine("?Connect failed, "+MySql.Error(db));
			return;
		}

		int sdb = MySql.SelectDb(db, myDb);
		if (sdb != 0) {
			Console.WriteLine("?Can't select the "+myDb+" database ,"
				+ MySql.Error(db));
			return;
		}

		int rcq = MySql.Query(db, myStmt);
		if (rcq != 0) {
			Console.WriteLine("?Couldn't execute ["+myStmt+"] on server, "
				+ MySql.Error(db));
			return;
		}
		Console.WriteLine("Query:  "+myStmt);
		procResults(db);

		Console.WriteLine("==== Diagnostic info  ====");
		Console.WriteLine("Client info: "+MySql.GetClientInfo());
		Console.WriteLine("Host info: "+MySql.GetHostInfo(db));
		Console.WriteLine("Server info: "+MySql.GetServerInfo(db));

		listProcesses(db);
		listTables(db);

		Console.WriteLine(MySql.Stat(db));

		MySql.Close(db);
		MySql.ThreadEnd();
		Console.WriteLine("Exiting...");
	}

	static void procResults(IntPtr db) {
		IntPtr res = MySql.StoreResult(db);
		int numRows = MySql.NumRows(res);
		Console.WriteLine("Number of records found: " + numRows);
		int numFields = MySql.NumFields(res);
		string[] fields = new string[numFields];
		for (int i = 0; i < numFields; i++) {
			Field fd = (Field) Marshal.PtrToStructure(MySql.FetchField(res), typeof(Field));
			fields[i] = fd.Name;
		}
		IntPtr row;
		int recCnt = 1;
		while ((row = MySql.FetchRow(res)) != IntPtr.Zero) {
			Console.WriteLine("Record #" + recCnt + ":");
			for (int i = 0, j = 1; i < numFields; i++, j++) {
				Console.WriteLine("  Fld #"+j+" ("+fields[i]+"): "+rowVal(row, i));
			}
			Console.WriteLine("==============================");
		}
		MySql.FreeResult(res);
	}

	static string rowVal(IntPtr res, int index) {
		IntPtr str = Marshal.ReadIntPtr(res, index*IntPtr.Size);
		if (str == IntPtr.Zero)
			return "NULL";
		string s = Marshal.PtrToStringAnsi(str);
		return s;
	}

	static void listProcesses(IntPtr db) {
		IntPtr res = MySql.ListProcesses(db);
		if (res == IntPtr.Zero) {
			Console.WriteLine("Got error "+MySql.Error(db)+" when retreiving processlist");
			return;
		}
		int numRows = MySql.NumRows(res);
		//    Console.WriteLine("Number of records found: "+i);
		int numFields = MySql.NumFields(res);
		string[] fields = new string[numFields];
		for (int i = 0; i < numFields; i++) {
			Field fd = (Field) Marshal.PtrToStructure(MySql.FetchField(res), typeof(Field));
			fields[i] = fd.Name;
		}
		IntPtr row;
		int recCnt = 1;
		while ((row = MySql.FetchRow(res)) != IntPtr.Zero) {
			Console.WriteLine("Process #" + recCnt + ":");
			for (int i = 0, j = 1; i < numFields; i++, j++) {
				Console.WriteLine("  Fld #"+j+" ("+fields[i]+"): "+rowVal(row, i));
			}
			Console.WriteLine("==============================");
		}
		MySql.FreeResult(res);
	}

	static void listTables(IntPtr db) {
		IntPtr res = MySql.ListTables(db, "%");
		if (res == IntPtr.Zero)
			return;
		int numRows = MySql.NumRows(res);
		//    Console.WriteLine("Number of records found: "+i);
		int numFields = MySql.NumFields(res);
		string[] fields = new string[numFields];
		for (int i = 0; i < numFields; i++) {
			Field fd = (Field) Marshal.PtrToStructure(MySql.FetchField(res), typeof(Field));
			fields[i] = fd.Name;
		}
		IntPtr row;
		int recCnt = 1;
		while ((row = MySql.FetchRow(res)) != IntPtr.Zero) {
			Console.WriteLine("Process #" + recCnt + ":");
			for (int i = 0, j = 1; i < numFields; i++, j++) {
				Console.WriteLine("  Fld #"+j+" ("+fields[i]+"): "+rowVal(row, i));
			}
			Console.WriteLine("==============================");
		}
		MySql.FreeResult(res);
	}

}
