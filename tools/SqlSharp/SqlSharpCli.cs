//
// SqlSharpCli.cs - main driver for SqlSharp
//
//                    Currently, only working on a command line interface for SqlSharp
//
//                    However, once GTK# and System.Windows.Forms are good-to-go,
//                    I would like to create a SqlSharpGui using this.
//
//                    It would be nice if this is included as part of Mono
//                    extra goodies under Mono.Data.SqlSharp.
//
//                    Also, this makes a good Test program for Mono System.Data.
//                    For more information about Mono::, 
//                    visit http://www.go-mono.com/
//
// To build SqlSharpCli.cs:
// $ mcs SqlSharpCli.cs -r System.Data.dll
//
// To run:
// $ mono SqlSharpCli.exe
//
// To run batch commands and get the output, do something like:
// $ cat commands.txt | mono SqlSharpCli.exe > results.txt
//
// Author:
//    Daniel Morgan <danmorg@sc.rr.com>
//
// (C)Copyright 2002 Daniel Morgan
//

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace Mono.Data.SqlSharp {
	
	// SQL Sharp - Command Line Interface
	public class SqlSharpCli {
	
		private IDbConnection conn = null;
		private string provider = "POSTGRESCLIENT";
		private StringBuilder build = null; // SQL string to build
		private string connectionString = 
			"host=localhost;dbname=test;user=postgres";
		private string inputFilename = "";
		private string outputFilename = "";
		private bool silent = false;
		
		// DisplayResult - used to Read() display a result set
		//                   called by DisplayData()
		public void DisplayResult(IDataReader reader, DataTable schemaTable) {

			StringBuilder line = null;
			StringBuilder hdrUnderline = null;
			
			int spacing = 0;
			int columnSize = 0;
			int c;
			
			char spacingChar = ' '; // a space
			char underlineChar = '='; // an equal sign

			string dataType; // .NET Type
			string dataTypeName; // native Database type
			DataRow row; // schema row

			line = new StringBuilder();
			hdrUnderline = new StringBuilder();

			Console.WriteLine("Fields in Query Result: " + 
						reader.FieldCount);
			Console.WriteLine();
			
			for(c = 0; c < schemaTable.Rows.Count; c++) {
							
				DataRow schemaRow = schemaTable.Rows[c];
				string columnHeader = (string) schemaRow["ColumnName"];
				int columnHeaderSize = columnHeader.Length;
				
				line.Append(columnHeader);
				hdrUnderline.Append(underlineChar, columnHeaderSize);
					
				// spacing
				columnSize = (int) schemaRow["ColumnSize"];
				dataType = (string) schemaRow["DataType"];
				dataTypeName = reader.GetDataTypeName(c);
				
				// columnSize correction based on data type
				if(dataType.Equals("System.Boolean")) {
					columnSize = 5;
				}
				if(provider.Equals("POSTGRESCLIENT"))
					if(dataTypeName.Equals("text"))				
						columnSize = 32; // text will be truncated to 32

				if(columnHeaderSize < columnSize) {
					spacing = columnSize - columnHeaderSize;
					line.Append(spacingChar, spacing);
					hdrUnderline.Append(underlineChar, spacing);
				}
				line.Append(" ");
				hdrUnderline.Append(" ");
			}
			Console.WriteLine(line.ToString());
			line = null;
			
			Console.WriteLine(hdrUnderline);
			Console.WriteLine();
			hdrUnderline = null;
			
			// DEBUG - need to know the columnSize
			/*
			line = new StringBuilder();
			foreach(DataRow schemaRow in schemaTable.Rows) {
				columnSize = (int) schemaRow["ColumnSize"];
				line.Append(columnSize.ToString());
				line.Append(" ");
			}		
			Console.WriteLine(line.ToString());
			Console.WriteLine();
			line = null;
			*/
								
			int rows = 0;

			// column data
			while(reader.Read()) {
				rows++;

				line = new StringBuilder();
				for(c = 0; c < reader.FieldCount; c++) {
					int dataLen = 0;
					string dataValue;
					
					row = schemaTable.Rows[c];
					string colhdr = (string) row["ColumnName"];
					columnSize = (int) row["ColumnSize"];
					dataType = (string) row["DataType"];
					dataTypeName = reader.GetDataTypeName(c);
					
					// certain types need to have the
					// columnSize adjusted for display
					// so the column will line up for each
					// row and match the column header size
					if(dataType.Equals("System.Boolean")) {
						columnSize = 5;
					}
					if(provider.Equals("POSTGRESCLIENT"))
						if(dataTypeName.Equals("text"))				
							columnSize = 32; // text will be truncated to 32
												
					if(reader.IsDBNull(c)) {
						dataValue = "";
						dataLen = 0;
					}
					else {
						object obj = reader.GetValue(c);						
							
						dataValue = obj.ToString();
						dataLen = dataValue.Length;
						line.Append(dataValue);
					}
					line.Append(" ");

					// spacing
					spacingChar = ' ';
					if(dataLen < columnSize) {
						spacing = columnSize - dataLen;
						line.Append(spacingChar, spacing);
					}
					spacingChar = ' ';
					if(columnSize < colhdr.Length) {
						spacing = colhdr.Length - columnSize;
						line.Append(spacingChar, spacing);
					}
						
				}
				Console.WriteLine(line.ToString());
				line = null;
			}
			Console.WriteLine("\nRows retrieved: " + rows);
		}

		// DisplayData - used to display any Result Sets
		//                 from execution of SQL SELECT Query or Queries
		//                 called by DisplayData. 
		//                 ExecuteSql() only calls this function
		//                 for a Query, it does not get
		//                 for a Command.
		public void DisplayData(IDataReader reader) {

			DataTable schemaTable = null;
			int ResultSet = 0;

			Console.WriteLine("Display any result sets...");

			do {
				// by Default, SqlDataReader has the 
				// first Result set if any

				ResultSet++;
				Console.WriteLine("Display the result set " + ResultSet);
				
				schemaTable = reader.GetSchemaTable();
				
				if(reader.RecordsAffected >= 0) {
					// SQL Command (INSERT, UPDATE, or DELETE)
					// RecordsAffected >= 0
					Console.WriteLine("SQL Command Records Affected: " + reader.RecordsAffected);
				}
				else if(schemaTable == null) {
					// SQL Command (not INSERT, UPDATE, nor DELETE)
					// RecordsAffected -1 and DataTable has a null reference
					Console.WriteLine("SQL Command Executed.");
				}
				else {
					// SQL Query (SELECT)
					// RecordsAffected -1 and DataTable has a reference
					DisplayResult(reader, schemaTable);
				}

			// get next result set (if anymore is left)
			} while(reader.NextResult());
		}

		// ExecuteSql - Execute the SQL Command(s) and/or Query(ies)
		public void ExecuteSql(string sql) {
			
			Console.WriteLine("Execute SQL: " + sql);

			IDbCommand cmd = null;
			IDataReader reader = null;

			// create a Command object based on the provider
			switch(provider) {
			case "POSTGRESCLIENT":
				cmd = new SqlCommand();
				break;
			default:
				Console.WriteLine("Error: PostgreSQL is only supported, and it through SqlClient.");
				return;
			}

			// set command properties
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = sql;
			cmd.Connection = conn;

			try {
				reader = cmd.ExecuteReader();
				DisplayData(reader);
				reader.Close();
				//reader = null;
				//cmd.Dispose();
				//cmd = null;
			}
			catch(Exception e) {
				Console.WriteLine("Exception Caught Executing SQL: " + e);
				//if(reader != null) {
				//	if(reader.IsClosed == false)
				//		reader.Close();
				//	reader = null;
				//}
				// cmd.Dispose();
				//cmd = null;
			}
		}

		// ShowHelp - show the help - command a user can enter
		public void ShowHelp() {
			Console.WriteLine("");
			Console.WriteLine(@"Type:  \Q to quit");
			Console.WriteLine(@"       \ConnectionString to set the ConnectionString");
			Console.WriteLine(@"       \Provider to set the Provider:");
			Console.WriteLine(@"                 {OleDb,SqlClient,");
			Console.WriteLine(@"                  OracleClient,PostgresClient}");
			Console.WriteLine(@"       \Open to open the connection");
			Console.WriteLine(@"       \Close to close the connection");
			Console.WriteLine(@"       \Execute to execute SQL command(s)/queries(s)");
			Console.WriteLine(@"       \f FILENAME to read a batch of commands from");
			Console.WriteLine(@"       \o FILENAME to read a batch of commands from");
			Console.WriteLine(@"       \h to show this help.");
			Console.WriteLine(@"       \s {TRUE, FALSE} to silent messages.");
			Console.WriteLine();
		}

		// ShowDefaults - show defaults for connection variables
		public void ShowDefaults() {
			Console.WriteLine();
			Console.WriteLine("The default Provider is " + provider);
			Console.WriteLine();
			Console.WriteLine("The default ConnectionString is: ");
			Console.WriteLine("    \"" + connectionString + "\"");
			Console.WriteLine();
		}

		// OpenDataSource - open connection to the data source
		public void OpenDataSource() {
			
			Console.WriteLine("Attempt to Open...");

			switch(provider) {
			case "POSTGRESCLIENT":
				conn = new SqlConnection();
				break;
			default:
				Console.WriteLine("Error: Currently, PostgreSQL is the only provider supported, and it through SqlClient.");
				break;
			}

			conn.ConnectionString = connectionString;
			
			try {
				conn.Open();
				if(conn.State == ConnectionState.Open)
					Console.WriteLine("Open was successfully.");
			}
			catch(Exception e) {
				Console.WriteLine("Exception Caught Opening. " + e);
				conn = null;
			}
		}

		// CloseDataSource - close the connection to the data source
		public void CloseDataSource() {
			Console.WriteLine("Attempt to Close...");

			try {
				conn.Close();
				Console.WriteLine("Close was successfull.");
			}
			catch(Exception e) {
				Console.WriteLine("Exeception Caught Closing. " + e);
			}
			conn = null;
		}

		// ChangeProvider - change the provider string variable
		public void ChangeProvider(string[] parms) {

			if(parms.Length == 2) {
				string parm = parms[1].ToUpper();
				switch(parm) {
				case "OLEDB":
				case "ORACLECLIENT":
					Console.WriteLine("Error: Provider not currently supported.");
					break;
				case "SQLCLIENT":
					provider = "POSTGRESCLIENT";
					Console.WriteLine("Warning: Currently, the SqlClient provider is the PostgreSQL provider.");
					break;
				case "POSTGRESCLIENT":
					provider = parm;
					break;
				default:
					Console.WriteLine("Error: " + "Bad argument or Provider not supported.");
					break;
				}
				Console.WriteLine("Provider: " + provider);
			}
			else
				Console.WriteLine("Error: provider only has one parameter.");
		}

		// ChangeConnectionString - change the connection string variable
		public void ChangeConnectionString(string entry) {
			
			if(entry.Length > 18)
				connectionString = entry.Substring(18, entry.Length - 18);
			else
				connectionString = "";
		}

		public void SetupInputFile(string[] parms) {
			if(parms.Length >= 2) {
				Console.WriteLine("Error: wrong number of parameters");
				return;
			}
			inputFilename = parms[1];
			// TODO:
			// open input file
			// while each line, do the SqlSharpCli command or SQL
			// close input file
		}

		public void SetupOutputFile(string[] parms) {
			if(parms.Length == 1) {
				outputFilename = "";
				// TODO: close the output file
			}
			else if(parms.Length > 2) {
				Console.WriteLine("Error: wrong number of parameters");
			}
			else {
				outputFilename = parms[1];
				// TODO: open the output file
			}
		}

		public void SetupSilentMode(string[] parms) {
			if(parms.Length != 2) {
				Console.WriteLine("Error: wrong number of parameters");
				return;
			}
			string parm = parms[1].ToUpper();
			if(parm.Equals("TRUE"))
				silent = true;
			else if(parm.Equals("FALSE"))
				silent = false;
			else
				Console.WriteLine("Error: invalid parameter.");
		}

		public void OutputLine(string line) {
			if(silent == false)
				Console.WriteLine(line);
		}

		public void ExecuteBatch() {
			// TODO:
			Console.WriteLine("Error: Execution of Batch Commands not implemented yet");
		}

		// HandleCommand - handle SqlSharpCli commands entered
		public void HandleCommand(string entry) {
			
			string[] parms;
			
			// maybe a SQL# Command was found
			parms = entry.Split(new char[1] {' '});
			string userCmd = parms[0].ToUpper();

			switch(userCmd) {
			case "\\PROVIDER":
				ChangeProvider(parms);
				break;
			case "\\CONNECTIONSTRING":
				ChangeConnectionString(entry);
				break;
			case "\\OPEN":
				OpenDataSource();
				break;
			case "\\CLOSE":
				CloseDataSource();
				break;
			case "\\S":
				SetupSilentMode(parms);
				break;
			case "\\E":
			case "\\EXECUTE":
				// Execute SQL Commands or Queries
				if(conn == null)
					Console.WriteLine("Error: connection is not Open.");
				else if(conn.State == ConnectionState.Closed)
					Console.WriteLine("Error: connection is not Open.");
				else {
					if(build == null)
						Console.WriteLine("Error: SQL Buffer is empty.");
					else {
						ExecuteSql(build.ToString());
					}
					build = null;
				}
				break;
			case "\\F":
				// Batch Input File: \f FILENAME
				SetupInputFile(parms);
				ExecuteBatch();
				break;
			case "\\O":
				// Batch Output File: \o FILENAME
				SetupOutputFile(parms);
				break;
			case "\\H":
			case "\\HELP":
				// Help
				ShowHelp();
				break;
			case "\\Q": 
			case "\\QUIT":
				// Quit
				break;
			default:
				// Error
				Console.WriteLine("Error: Unknown user command.");
				break;
			}
		}

		public void DealWithArgs(string[] args) {
			for(int a = 0; a < args.Length; a++) {
				if(args[a].Substring(0,1).Equals("-")) {
					string arg = args[a].ToUpper().Substring(1, args[a].Length - 1);
					switch(arg) {
					case "S":
						silent = true;
						break;
					case "F":		
						if(a + 1 >= args.Length)
							Console.WriteLine("Error: Missing FILENAME for -f switch");
						else {
							inputFilename = args[a + 1];
							ExecuteBatch();
						}
						break;
					case "O":
						if(a + 1 >= args.Length)
							Console.WriteLine("Error: Missing FILENAME for -o switch");
						else
							outputFilename = args[a + 1];
						break;
					default:
						Console.WriteLine("Error: Unknow switch: " + args[a]);
						break;
					}
				}
			}
		}
		
		public void Run(string[] args) {

			DealWithArgs(args);

			string entry = "";
			build = null;

			if(silent == false) {
				Console.WriteLine("Welcome to SQL#. The interactive SQL command-line client ");
				Console.WriteLine("for Mono.Data.  See http://www.go-mono.com/ for more details.\n");
						
				ShowHelp();
				ShowDefaults();
			}
			
			while(entry.ToUpper().Equals("\\Q") == false &&
				entry.ToUpper().Equals("\\QUIT") == false) {
				
				Console.Write("\nSQL# ");
				entry = Console.ReadLine();

				Console.WriteLine("Entered: " + entry);
				
				if(entry.Substring(0,1).Equals("\\")) {
					HandleCommand(entry);
				}
				else if(entry.IndexOf(";") >= 0) {
					// most likely the end of SQL Command or Query found
					// execute the SQL
					if(conn == null)
						Console.WriteLine("Error: connection is not Open.");
					else if(conn.State == ConnectionState.Closed)
						Console.WriteLine("Error: connection is not Open.");
					else {
						if(build == null) {
							build = new StringBuilder();
						}
						build.Append(entry);
						ExecuteSql(build.ToString());
						build = null;
					}
				}
				else {
					// most likely a part of a SQL Command or Query found
					// append this part of the SQL
					if(build == null) {
						build = new StringBuilder();
					}
					build.Append(entry + " ");
				}
			}			
		}
	}

	public class SqlSharpDriver {

		public static void Main(string[] args) {
			SqlSharpCli sqlCommandLineEngine = new SqlSharpCli();
			sqlCommandLineEngine.Run(args);
		}
	}
}
