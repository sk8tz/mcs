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
// To run with mono:
// $ mono SqlSharpCli.exe
//
// To run with mint:
// $ mint SqlSharpCli.exe
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
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
//using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;

namespace Mono.Data.SqlSharp {

	public enum FileFormat {
		Html,
		Xml,
		CommaSeparatedValues,
		TabSeparated,
		Normal
	}

	// SQL Sharp - Command Line Interface
	public class SqlSharpCli {

		// provider supports
		private bool UseParameters = true;
		private bool UseSimpleReader = false;
	
		private IDbConnection conn = null;
                		
		private string provider = "LOADEXTPROVIDER"; // name of internal provider
		// {OleDb,SqlClient,MySql,Odbc,Oracle,PostgreSql,SqlLite} however, it
		// can be set to LOADEXTPROVIDER to load an external provider
		private string providerAssembly = "Mono.Data.PostgreSqlClient";
		// filename of assembly
		// for example: "Mono.Data.MySql"
		private string providerConnectionClass = "Mono.Data.PostgreSqlClient.PgSqlConnection";
		// Connection class
		// in the provider assembly that implements the IDbConnection 
		// interface.  for example: "Mono.Data.MySql.MySqlConnection"

		private StringBuilder build = null; // SQL string to build
		private string buff = ""; // SQL string buffer

		private string connectionString = 
			"host=localhost;dbname=test;user=postgres";

		private string inputFilename = "";
		private string outputFilename = "";
		private StreamReader inputFilestream = null;
		private StreamWriter outputFilestream = null;

		private FileFormat outputFileFormat = FileFormat.Html;

		private bool silent = false;
		private bool showHeader = true;

		private Hashtable internalVariables = new Hashtable();
				
		// DisplayResult - used to Read() display a result set
		//                   called by DisplayData()
		public void DisplayResult(IDataReader reader, DataTable schemaTable) {

			const string zero = "0";
			StringBuilder column = null;
			StringBuilder line = null;
			StringBuilder hdrUnderline = null;
			string outData = "";
			int hdrLen = 0;
			
			int spacing = 0;
			int columnSize = 0;
			int c;
			
			char spacingChar = ' '; // a space
			char underlineChar = '='; // an equal sign

			string dataType; // .NET Type
			Type theType; 
			string dataTypeName; // native Database type
			DataRow row; // schema row

			line = new StringBuilder();
			hdrUnderline = new StringBuilder();

			OutputLine("Fields in Query Result: " + 
				reader.FieldCount);
			OutputLine("");
			
			for(c = 0; c < schemaTable.Rows.Count; c++) {
							
				DataRow schemaRow = schemaTable.Rows[c];
				string columnHeader = (string) schemaRow["ColumnName"];
				if(columnHeader.Equals(""))
					columnHeader = "?column?";
				if(columnHeader.Length > 32)
					columnHeader = columnHeader.Substring(0,32);											
					
				// spacing
				columnSize = (int) schemaRow["ColumnSize"];
				theType = (Type) schemaRow["DataType"];
				dataType = theType.ToString();
				dataTypeName = reader.GetDataTypeName(c);

				switch(dataType) {
				case "System.DateTime":
					columnSize = 19;
					break;
				case "System.Boolean":
					columnSize = 5;
					break;
				}

				if(provider.Equals("POSTGRESQL") ||
					provider.Equals("MYSQL"))
					if(dataTypeName.Equals("text"))				
						columnSize = 32; // text will be truncated to 32

				hdrLen = (columnHeader.Length > columnSize) ? 
					columnHeader.Length : columnSize;

				if(hdrLen < 0)
					hdrLen = 0;
				if(hdrLen > 32)
					hdrLen = 32;

				line.Append(columnHeader);
				if(columnHeader.Length < hdrLen) {
					spacing = hdrLen - columnHeader.Length;
					line.Append(spacingChar, spacing);
				}
				hdrUnderline.Append(underlineChar, hdrLen);

				line.Append(" ");
				hdrUnderline.Append(" ");
			}
			OutputHeader(line.ToString());
			line = null;
			
			OutputHeader(hdrUnderline.ToString());
			OutputHeader("");
			hdrUnderline = null;		
								
			int rows = 0;

			// column data
			while(reader.Read()) {
				rows++;
				
				line = new StringBuilder();
				for(c = 0; c < reader.FieldCount; c++) {
					int dataLen = 0;
					string dataValue = "";
					column = new StringBuilder();
					outData = "";
					
					row = schemaTable.Rows[c];
					string colhdr = (string) row["ColumnName"];
					if(colhdr.Equals(""))
						colhdr = "?column?";
					if(colhdr.Length > 32)
						colhdr = colhdr.Substring(0, 32);

					columnSize = (int) row["ColumnSize"];
					theType = (Type) row["DataType"];
					dataType = theType.ToString();
					
					dataTypeName = reader.GetDataTypeName(c);

					switch(dataType) {
					case "System.DateTime":
						columnSize = 19;
						break;
					case "System.Boolean":
						columnSize = 5;
						break;
					}

					if(provider.Equals("POSTGRESQL") ||
						provider.Equals("MYSQL"))
						if(dataTypeName.Equals("text"))				
							columnSize = 32; // text will be truncated to 32

					columnSize = (colhdr.Length > columnSize) ? 
						colhdr.Length : columnSize;

					if(columnSize < 0)
						columnSize = 0;
					if(columnSize > 32)
						columnSize = 32;							
					
					dataValue = "";	
					
					if(reader.IsDBNull(c)) {
						dataValue = "";
						dataLen = 0;
					}
					else {											
						StringBuilder sb;
						DateTime dt;
						if(dataType.Equals("System.DateTime")) {
							// display date in ISO format
							// "YYYY-MM-DD HH:MM:SS"
							dt = reader.GetDateTime(c);
							sb = new StringBuilder();
							// year
							if(dt.Year < 10)
								sb.Append("000" + dt.Year);
							else if(dt.Year < 100)
								sb.Append("00" + dt.Year);
							else if(dt.Year < 1000)
								sb.Append("0" + dt.Year);
							else
								sb.Append(dt.Year);
							sb.Append("-");
							// month
							if(dt.Month < 10)
								sb.Append(zero + dt.Month);
							else
								sb.Append(dt.Month);
							sb.Append("-");
							// day
							if(dt.Day < 10)
								sb.Append(zero + dt.Day);
							else
								sb.Append(dt.Day);
							sb.Append(" ");
							// hour
							if(dt.Hour < 10)
								sb.Append(zero + dt.Hour);
							else
								sb.Append(dt.Hour);
							sb.Append(":");
							// minute
							if(dt.Minute < 10)
								sb.Append(zero + dt.Minute);
							else
								sb.Append(dt.Minute);
							sb.Append(":");
							// second
							if(dt.Second < 10)
								sb.Append(zero + dt.Second);
							else
								sb.Append(dt.Second);

							dataValue = sb.ToString();
						}
						else {
							dataValue = reader.GetValue(c).ToString();
						}

						dataLen = dataValue.Length;
						if(dataLen < 0) {
							dataValue = "";
							dataLen = 0;
						}
						if(dataLen > 32) {
							dataValue = dataValue.Substring(0,32);
							dataLen = 32;
						}
					}
					columnSize = columnSize > dataLen ? columnSize : dataLen;
					
					// spacing
					spacingChar = ' ';										
					if(columnSize < colhdr.Length) {
						spacing = colhdr.Length - columnSize;
						column.Append(spacingChar, spacing);
					}
					if(dataLen < columnSize) {
						spacing = columnSize - dataLen;
						column.Append(spacingChar, spacing);
						switch(dataType) {
						case "System.Int16":
						case "System.Int32":
						case "System.Int64":
						case "System.Single":
						case "System.Double":
						case "System.Decimal":
							outData = column.ToString() + dataValue;
							break;
						default:
							outData = dataValue + column.ToString();
							break;
						}
					}
					else
						outData = dataValue;

					line.Append(outData);
					line.Append(" ");
				}
				OutputData(line.ToString());
				line = null;
			}
			OutputLine("\nRows retrieved: " + rows.ToString());
		}
		
		public void OutputDataToHtmlFile(IDataReader rdr, DataTable dt) {
                        		
			StringBuilder strHtml = new StringBuilder();

			strHtml.Append("<html> \n <head> <title>");
			strHtml.Append("Results");
			strHtml.Append("</title> </head>");
			strHtml.Append("<body>");
			strHtml.Append("<h1> Results </h1>");
			strHtml.Append("<table border=1>");
		
			outputFilestream.WriteLine(strHtml.ToString());

			strHtml = null;
			strHtml = new StringBuilder();

			strHtml.Append("<tr>");
			foreach (DataRow schemaRow in dt.Rows) {
				strHtml.Append("<td> <b>");
				object dataObj = schemaRow["ColumnName"];
				string sColumnName = dataObj.ToString();
				strHtml.Append(sColumnName);
				strHtml.Append("</b> </td>");
			}
			strHtml.Append("</tr>");
			outputFilestream.WriteLine(strHtml.ToString());
			strHtml = null;

			int col = 0;
			string dataValue = "";
			
			while(rdr.Read()) {
				strHtml = new StringBuilder();

				strHtml.Append("<tr>");
				for(col = 0; col < rdr.FieldCount; col++) {
						
					// column data
					if(rdr.IsDBNull(col) == true)
						dataValue = "NULL";
					else {
						object obj = rdr.GetValue(col);
						dataValue = obj.ToString();
					}
					strHtml.Append("<td>");
					strHtml.Append(dataValue);
					strHtml.Append("</td>");
				}
				strHtml.Append("\t\t</tr>");
				outputFilestream.WriteLine(strHtml.ToString());
				strHtml = null;
			}
			outputFilestream.WriteLine(" </table> </body> \n </html>");
			strHtml = null;
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

			OutputLine("Display any result sets...");

			do {
				// by Default, SqlDataReader has the 
				// first Result set if any

				ResultSet++;
				OutputLine("Display the result set " + ResultSet);
				
				schemaTable = reader.GetSchemaTable();
				
				if(reader.FieldCount > 0) {
					// SQL Query (SELECT)
					// RecordsAffected -1 and DataTable has a reference
					OutputQueryResult(reader, schemaTable);
				}
				else if(reader.RecordsAffected >= 0) {
					// SQL Command (INSERT, UPDATE, or DELETE)
					// RecordsAffected >= 0
					Console.WriteLine("SQL Command Records Affected: " + reader.RecordsAffected);
				}
				else {
					// SQL Command (not INSERT, UPDATE, nor DELETE)
					// RecordsAffected -1 and DataTable has a null reference
					Console.WriteLine("SQL Command Executed.");
				}
				
				// get next result set (if anymore is left)
			} while(reader.NextResult());
		}

		// display the result in a simple way
		// new ADO.NET providers may have not certain
		// things implemented yet, such as, TableSchema
		// support
		public void DisplayDataSimple(IDataReader reader) {
						
			int row = 0;
			Console.WriteLine("Reading Data using simple reader...");
			while(reader.Read()){
				row++;
				Console.WriteLine("Row: " + row);
				for(int col = 0; col < reader.FieldCount; col++) {
					Console.WriteLine("  Field: " + col);
					//string dname = reader.GetName(col);
					//Console.WriteLine("      Name: " + dname);
					string dvalue = reader.GetValue(col).ToString();
					Console.WriteLine("      Value: " + dvalue);
				}
			}
			Console.WriteLine("\n" + row + " ROWS RETRIEVED\n");
		}

		public void OutputQueryResult(IDataReader dreader, DataTable dtable) {
			if(outputFilestream == null) {
				DisplayResult(dreader, dtable);
			}
			else {
				switch(outputFileFormat) {
				case FileFormat.Normal:
					DisplayResult(dreader, dtable);
					break;
				case FileFormat.Html:
					OutputDataToHtmlFile(dreader, dtable);
					break;
				default:
					Console.WriteLine("Error: Output data file format not supported.");
					break;
				}
			}
		}

		public void BuildParameters(IDbCommand cmd) {
			if(UseParameters == true) {

				ParametersBuilder parmsBuilder = 
					new ParametersBuilder(cmd, 
					BindVariableCharacter.Colon);
			
				Console.WriteLine("Get Parameters (if any)...");
				parmsBuilder.ParseParameters();
				IList parms = (IList) cmd.Parameters;
		
				Console.WriteLine("Print each parm...");
				for(int p = 0; p < parms.Count; p++) {
					string theParmName;

					IDataParameter prm = (IDataParameter) parms[p];
					theParmName = prm.ParameterName;
				
					string inValue = "";
					bool found;
					if(parmsBuilder.ParameterMarkerCharacter == '?') {
						Console.Write("Enter Parameter " + 
							(p + 1).ToString() +
							": ");
						inValue = Console.ReadLine();
						prm.Value = inValue;
					}
					else {
						found = GetInternalVariable(theParmName, out inValue);
						if(found == true) {
							prm.Value = inValue;
						}
						else {
							Console.Write("Enter Parameter " + (p + 1).ToString() +
								": " + theParmName + ": ");
							inValue = Console.ReadLine();
							prm.Value = inValue;
						}
					}
				}
				parmsBuilder = null;
			}
		}

		// ExecuteSql - Execute the SQL Command(s) and/or Query(ies)
		public void ExecuteSql(string sql) {
			
			Console.WriteLine("Execute SQL: " + sql);

			IDbCommand cmd = null;
			IDataReader reader = null;

			cmd = conn.CreateCommand();

			// set command properties
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = sql;
			cmd.Connection = conn;

			BuildParameters(cmd);

			try {
				reader = cmd.ExecuteReader();

				if(UseSimpleReader == false)
					DisplayData(reader);
				else
					DisplayDataSimple(reader);

				reader.Close();
				reader = null;
			}
			catch(Exception e) {
				Console.WriteLine("Exception Caught Executing SQL: " + e);
				//if(reader != null) {
				//	if(reader.IsClosed == false)
				//		reader.Close();
				reader = null;
				//}
			}
			finally {
				// cmd.Dispose();
				cmd = null;
			}
		}

		// ExecuteSql - Execute the SQL Commands (no SELECTs)
		public void ExecuteSqlNonQuery(string sql) {
			
			Console.WriteLine("Execute SQL Non Query: " + sql);

			IDbCommand cmd = null;
			int rowsAffected = -1;
			
			cmd = conn.CreateCommand();

			// set command properties
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = sql;
			cmd.Connection = conn;

			BuildParameters(cmd);

			try {
				rowsAffected = cmd.ExecuteNonQuery();
				cmd = null;
				Console.WriteLine("Rows affected: " + rowsAffected);
			}
			catch(Exception e) {
				Console.WriteLine("Exception Caught Executing SQL: " + e);
			}
			finally {
				// cmd.Dispose();
				cmd = null;
			}
		}

		public void ExecuteSqlScalar(string sql) {
			Console.WriteLine("Execute SQL Scalar: " + sql);

			IDbCommand cmd = null;
			string retrievedValue = "";
			
			cmd = conn.CreateCommand();

			// set command properties
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = sql;
			cmd.Connection = conn;

			BuildParameters(cmd);

			try {
				retrievedValue = (string) cmd.ExecuteScalar().ToString();
				Console.WriteLine("Retrieved value: " + retrievedValue);
			}
			catch(Exception e) {
				Console.WriteLine("Exception Caught Executing SQL: " + e);
			}
			finally {
				// cmd.Dispose();
				cmd = null;
			}
		}

		public void ExecuteSqlXml(string sql) {
			Console.WriteLine("Error: Not implemented yet.");
		}

		// like ShowHelp - but only show at the beginning
		// only the most important commands are shown
		// like help and quit
		public void StartupHelp() {
			Console.WriteLine(@"Type:  \Q to quit");
			Console.WriteLine(@"       \ConnectionString to set the ConnectionString");
			Console.WriteLine(@"       \Provider to set the Provider:");
			Console.WriteLine(@"                 {OleDb,SqlClient,MySql,Odbc,");
			Console.WriteLine(@"                  Oracle,PostgreSql,Sqlite)");
			Console.WriteLine(@"       \Open to open the connection");
			Console.WriteLine(@"       \Close to close the connection");
			Console.WriteLine(@"       \Execute to execute SQL command(s)/queries(s)");
			Console.WriteLine(@"       \h to show this help.");
			Console.WriteLine(@"       \defaults to show default variables.");
			Console.WriteLine();
		}
		
		// ShowHelp - show the help - command a user can enter
		public void ShowHelp() {
			Console.WriteLine("");
			Console.WriteLine(@"Type:  \Q to quit");
			Console.WriteLine(@"       \ConnectionString to set the ConnectionString");
			Console.WriteLine(@"       \Provider to set the Provider:");
			Console.WriteLine(@"                 {OleDb,SqlClient,MySql,Odbc,");
			Console.WriteLine(@"                  Oracle,PostgreSql, Sqlite}");
			Console.WriteLine(@"       \Open to open the connection");
			Console.WriteLine(@"       \Close to close the connection");
			Console.WriteLine(@"       \Execute to execute SQL command(s)/queries(s)");
			Console.WriteLine(@"       \exenonquery execute an SQL non query (not a SELECT).");
			Console.WriteLine(@"       \exescalar execute SQL to get a single row/single column result.");
			Console.WriteLine(@"       \f FILENAME to read a batch of Sql# commands/queries from.");
			Console.WriteLine(@"       \o FILENAME to write out the result of commands executed.");
			Console.WriteLine(@"       \load FILENAME to load from file SQL commands into SQL buffer.");
			Console.WriteLine(@"       \save FILENAME to save SQL commands from SQL buffer to file.");
			Console.WriteLine(@"       \h to show this help.");
			Console.WriteLine(@"       \defaults to show default variables, such as,");
			Console.WriteLine(@"            Provider and ConnectionString.");
			Console.WriteLine(@"       \s {TRUE, FALSE} to silent messages.");
			Console.WriteLine(@"       \r reset (clear) the query buffer.");
			Console.WriteLine(@"       \set NAME VALUE - set an internal variable.");
			WaitForEnterKey();
			Console.WriteLine(@"       \unset NAME - remove an internal variable.");
			Console.WriteLine(@"       \variable NAME - display the value of an internal variable.");
			Console.WriteLine(@"       \loadprovider CLASS - load the provider");
			Console.WriteLine(@"            use the complete name of its connection class.");
			Console.WriteLine(@"       \loadextprovider ASSEMBLY CLASS - load the provider"); 
			Console.WriteLine(@"            use the complete name of its assembly and");
			Console.WriteLine(@"            its Connection class.");
			Console.WriteLine(@"       \print - show what's in the SQL buffer now.");
			Console.WriteLine(@"       \UseParameters (TRUE,FALSE) - use parameters when executing SQL.");
			Console.WriteLine(@"       \UseSimpleReader (TRUE,FALSE) - use simple reader when displaying results.");
			Console.WriteLine();
		}

		public void WaitForEnterKey() {
                        Console.Write("Waiting... Press Enter key to continue. ");
			string entry = Console.ReadLine();
		}

		// ShowDefaults - show defaults for connection variables
		public void ShowDefaults() {
			Console.WriteLine();
			Console.WriteLine("The default Provider is " + provider);
			if(provider.Equals("LOADEXTPROVIDER")) {
				Console.WriteLine("          Assembly: " + 
					providerAssembly);
				Console.WriteLine("  Connection Class: " + 
					providerConnectionClass);
			}
			Console.WriteLine();
			Console.WriteLine("The default ConnectionString is: ");
			Console.WriteLine("    \"" + connectionString + "\"");
			Console.WriteLine();
		}

		// OpenDataSource - open connection to the data source
		public void OpenDataSource() {
			
			Console.WriteLine("Attempt to open connection...");

			try {
				switch(provider) {
				case "ODBC":
					conn = new OdbcConnection();
					break;
				case "OLEDB":
					conn = new OleDbConnection();
					break;
				case "SQLCLIENT":
					Console.WriteLine("Error: SqlClient not currently supported.");
					throw new Exception();
					// conn = new SqlConnection();
					break;
				case "LOADEXTPROVIDER":
					if(LoadExternalProvider() == false)
						return;
					break;
				default:
					Console.WriteLine("Error: Bad argument or provider not supported.");
					return;
				}
			}
			catch(Exception e) {
				Console.WriteLine("Error: Unable to create Connection object. " + e);
				return;
			}

			conn.ConnectionString = connectionString;
			
			try {
				conn.Open();
				if(conn.State == ConnectionState.Open)
					Console.WriteLine("Open was successfull.");
			}
			catch(Exception e) {
				Console.WriteLine("Exception Caught Opening. " + e);
				conn = null;
			}
		}

		// CloseDataSource - close the connection to the data source
		public void CloseDataSource() {
			
			if(conn != null) {
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
		}

		// ChangeProvider - change the provider string variable
		public void ChangeProvider(string[] parms) {

			string[] extp;

			if(parms.Length == 2) {
				string parm = parms[1].ToUpper();
				switch(parm) {
				case "ORACLE":
				case "TDS":
				case "SYBASE":
					Console.WriteLine("Error: Provider not currently supported.");
					break;
				case "MYSQL":
					extp = new string[3] {
									      "\\loadextprovider",
									      "Mono.Data.MySql",
									      "Mono.Data.MySql.MySqlConnection"};
					SetupExternalProvider(extp);
					UseParameters = false;
					UseSimpleReader = false;
					break;
				case "SQLITE":
					extp = new string[3] {
									      "\\loadextprovider",
									      "Mono.Data.SqliteClient",
									      "Mono.Data.SqliteClient.SqliteConnection"};
					SetupExternalProvider(extp);
					UseParameters = false;
					UseSimpleReader = true;
					break;
				case "SQLCLIENT":
					provider = parm;
					UseParameters = false;
					UseSimpleReader = true;
					Console.WriteLine("Error: SqlClient provider is not currently supported.");
					break;
				case "ODBC":
					UseParameters = false;
					UseSimpleReader = false;
					provider = parm;
					break;
				case "GDA":
				case "OLEDB":
					UseParameters = false;
					UseSimpleReader = true;
					provider = parm;
					break;
				case "POSTGRESQL":
					UseParameters = true;
					UseSimpleReader = false;
					provider = parm;
					extp = new string[3] {
								     "\\loadextprovider",
								     "Mono.Data.PostgreSqlClient",
								     "Mono.Data.PostgreSqlClient.PgSqlConnection"};
					SetupExternalProvider(extp);
					UseParameters = false;
					UseSimpleReader = true;
					break;
				default:
					Console.WriteLine("Error: " + "Bad argument or Provider not supported.");
					break;
				}
				Console.WriteLine("The default Provider is " + provider);
				if(provider.Equals("LOADEXTPROVIDER")) {
					Console.WriteLine("          Assembly: " + 
						providerAssembly);
					Console.WriteLine("  Connection Class: " + 
						providerConnectionClass);
				}
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

		public void SetupOutputResultsFile(string[] parms) {
			if(parms.Length != 2) {
				Console.WriteLine("Error: wrong number of parameters");
				return;
			}
			try {
				outputFilestream = new StreamWriter(parms[1]);
			}
			catch(Exception e) {
				Console.WriteLine("Error: Unable to setup output results file. " + e);
				return;
			}
		}

		public void SetupInputCommandsFile(string[] parms) {
			if(parms.Length != 2) {
				Console.WriteLine("Error: wrong number of parameters");
				return;
			}
			try {
				inputFilestream = new StreamReader(parms[1]);
			}
			catch(Exception e) {
				Console.WriteLine("Error: Unable to setup input commmands file. " + e);
				return;
			}	
		}

		public void LoadBufferFromFile(string[] parms) {
			if(parms.Length != 2) {
				Console.WriteLine("Error: wrong number of parameters");
				return;
			}
			string inFilename = parms[1];
			try {
				StreamReader sr = new StreamReader( inFilename);
				StringBuilder buffer = new StringBuilder();
				string NextLine;
			
				while((NextLine = sr.ReadLine()) != null) {
					buffer.Append(NextLine);
					buffer.Append("\n");
				}
				sr.Close();
				buff = buffer.ToString();
				build = null;
				build = new StringBuilder();
				build.Append(buff);
			}
			catch(Exception e) {
				Console.WriteLine("Error: Unable to read file into SQL Buffer. " + e);
			}
		}

		public void SaveBufferToFile(string[] parms) {
			if(parms.Length != 2) {
				Console.WriteLine("Error: wrong number of parameters");
				return;
			}
			string outFilename = parms[1];
			try {
				StreamWriter sw = new StreamWriter(outFilename);
				sw.WriteLine(buff);
				sw.Close();
			}
			catch(Exception e) {
				Console.WriteLine("Error: Could not save SQL Buffer to file." + e);
			}
		}

		public void SetUseParameters(string[] parms) {
			if(parms.Length != 2) {
				Console.WriteLine("Error: wrong number of parameters");
				return;
			}
			string parm = parms[1].ToUpper();
			if(parm.Equals("TRUE"))
				UseParameters = true;
			else if(parm.Equals("FALSE"))
				UseParameters = false;
			else
				Console.WriteLine("Error: invalid parameter.");

		}

		public void SetUseSimpleReader(string[] parms) {
			if(parms.Length != 2) {
				Console.WriteLine("Error: wrong number of parameters");
				return;
			}
			string parm = parms[1].ToUpper();
			if(parm.Equals("TRUE"))
				UseSimpleReader = true;
			else if(parm.Equals("FALSE"))
				UseSimpleReader = false;
			else
				Console.WriteLine("Error: invalid parameter.");
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

		public void SetInternalVariable(string[] parms) {
			if(parms.Length < 2) {
				Console.WriteLine("Error: wrong number of parameters.");
				return;
			}
			string parm = parms[1];
			StringBuilder ps = new StringBuilder();
			
			for(int i = 2; i < parms.Length; i++)
				ps.Append(parms[i]);

			internalVariables[parm] = ps.ToString();
		}

		public void UnSetInternalVariable(string[] parms) {
			if(parms.Length != 2) {
				Console.WriteLine("Error: wrong number of parameters.");
				return;
			}
			string parm = parms[1];

			try {
				internalVariables.Remove(parm);
			}
			catch(Exception e) {
				Console.WriteLine("Error: internal variable does not exist.");
			}
		}

		public void ShowInternalVariable(string[] parms) {
			string internalVariableValue = "";

			if(parms.Length != 2) {
				Console.WriteLine("Error: wrong number of parameters.");
				return;
			}
						
			string parm = parms[1];

			if(GetInternalVariable(parm, out internalVariableValue) == true)
				Console.WriteLine("Internal Variable - Name: " + 
					parm + "  Value: " + internalVariableValue);
		}

		public bool GetInternalVariable(string name, out string sValue) {
			sValue = "";
			bool valueReturned = false;

			try {
				if(internalVariables.ContainsKey(name) == true) {
					sValue = (string) internalVariables[name];
					valueReturned = true;
				}
				else
					Console.WriteLine("Error: internal variable does not exist.");

			}
			catch(Exception e) {
				Console.WriteLine("Error: internal variable does not exist.");
			}
			return valueReturned;
		}

		// to be used for loading .NET Data Providers that exist in
		// the System.Data assembly, but are not explicitly handling
		// in SQL#
		public void LoadProvider(string[] parms) {
			Console.WriteLine("Error: not implemented yet.");			
		}

		public void SetupExternalProvider(string[] parms) {
			if(parms.Length != 3) {
				Console.WriteLine("Error: Wrong number of parameters.");
				return;
			}
			provider = "LOADEXTPROVIDER";
			providerAssembly = parms[1];
			providerConnectionClass = parms[2];
		}

		public bool LoadExternalProvider() {
			
			bool success = false;

			// For example: for the MySQL provider in Mono.Data.MySql
			//   \LoadExtProvider Mono.Data.MySql Mono.Data.MySql.MySqlConnection
			//   \ConnectionString dbname=test
			//   \open
			//   insert into sometable (tid, tdesc, aint) values ('abc','def',12)
			//   \exenonquery
			//   \close
			//   \quit

			try {
				Console.WriteLine("Loading external provider...");
				Console.Out.Flush();

				Assembly ps = Assembly.Load(providerAssembly);
				Type typ = ps.GetType(providerConnectionClass);
				conn = (IDbConnection) Activator.CreateInstance(typ);
				success = true;
				
				Console.WriteLine("External provider loaded.");
				Console.Out.Flush();
			}
			catch(FileNotFoundException f) {
				Console.WriteLine("Error: unable to load the assembly of the provider: " + 
					providerAssembly);
			}
			return success;
		}

		// used for outputting message, but if silent is set,
		// don't display
		public void OutputLine(string line) {
			if(silent == false)
				OutputData(line);
		}

		// used for outputting the header columns of a result
		public void OutputHeader(string line) {
			if(showHeader == true)
				OutputData(line);
		}

		// OutputData() - used for outputting data
		//  if an output filename is set, then the data will
		//  go to a file; otherwise, it will go to the Console.
		public void OutputData(string line) {
			if(outputFilestream == null)
				Console.WriteLine(line);
			else
				outputFilestream.WriteLine(line);
		}

		// HandleCommand - handle SqlSharpCli commands entered
		public void HandleCommand(string entry) {		
			string[] parms;
			
			parms = entry.Split(new char[1] {' '});
			string userCmd = parms[0].ToUpper();

			switch(userCmd) {
			case "\\PROVIDER":
				ChangeProvider(parms);
				break;
			case "\\CONNECTIONSTRING":
				ChangeConnectionString(entry);
				break;
			case "\\LOADPROVIDER":
				// TODO:
				//SetupProvider(parms);
				break;
			case "\\LOADEXTPROVIDER":
				SetupExternalProvider(parms);
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
						buff = build.ToString();
						ExecuteSql(buff);
					}
					build = null;
				}
				break;
			case "\\EXENONQUERY":
				if(conn == null)
					Console.WriteLine("Error: connection is not Open.");
				else if(conn.State == ConnectionState.Closed)
					Console.WriteLine("Error: connection is not Open.");
				else {
					if(build == null)
						Console.WriteLine("Error: SQL Buffer is empty.");
					else {
						buff = build.ToString();
						ExecuteSqlNonQuery(buff);
					}
					build = null;
				}
				break;
			case "\\EXESCALAR":
				if(conn == null)
					Console.WriteLine("Error: connection is not Open.");
				else if(conn.State == ConnectionState.Closed)
					Console.WriteLine("Error: connection is not Open.");
				else {
					if(build == null)
						Console.WriteLine("Error: SQL Buffer is empty.");
					else {
						buff = build.ToString();
						ExecuteSqlScalar(buff);
					}
					build = null;
				}
				break;
			case "\\F":
				SetupInputCommandsFile(parms);
				break;
			case "\\O":
				SetupOutputResultsFile(parms);
				break;
			case "\\LOAD":
				// Load file into SQL buffer: \load FILENAME
				LoadBufferFromFile(parms);
				break;
			case "\\SAVE":
				// Save SQL buffer to file: \save FILENAME
				SaveBufferToFile(parms);
				break;
			case "\\H":
			case "\\HELP":
				// Help
				ShowHelp();
				break;
			case "\\DEFAULTS":
				// show the defaults for provider and connection strings
				ShowDefaults();
				break;
			case "\\Q": 
			case "\\QUIT":
				// Quit
				break;
			case "\\R": 
				// reset (clear) the query buffer
				build = null;
				break;
			case "\\SET":
				// sets internal variable
				// \set name value
				SetInternalVariable(parms);
				break;
			case "\\UNSET":
				// deletes internal variable
				// \unset name
				UnSetInternalVariable(parms);
				break;
			case "\\VARIABLE":
				ShowInternalVariable(parms);
				break;
			case "\\PRINT":
				if(build == null)
					Console.WriteLine("SQL Buffer is empty.");
				else
					Console.WriteLine("SQL Bufer:\n" + buff);
				break;
			case "\\USEPARAMETERS":
				SetUseParameters(parms);
				break;
			case "\\USESIMPLEREADER":
				SetUseSimpleReader(parms);
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
							inputFilestream = new StreamReader(inputFilename);
						}
						break;
					case "O":
						if(a + 1 >= args.Length)
							Console.WriteLine("Error: Missing FILENAME for -o switch");
						else {
							outputFilename = args[a + 1];
							outputFilestream = new StreamWriter(outputFilename);
						}
						break;
					default:
						Console.WriteLine("Error: Unknow switch: " + args[a]);
						break;
					}
				}
			}
		}
		
		public string ReadSqlSharpCommand() {
			string entry = "";

			if(inputFilestream == null) {
				Console.Write("\nSQL# ");
				entry = Console.ReadLine();		
			}
			else {
				try {
					entry = inputFilestream.ReadLine();
					if(entry == null) {
						Console.WriteLine("Executing SQL# Commands from file done.");
					}
				}
				catch(Exception e) {
					Console.WriteLine("Error: Reading command from file.");
				}
				Console.Write("\nSQL# ");
				entry = Console.ReadLine();
			}
			return entry;
		}
		
		public void Run(string[] args) {

			DealWithArgs(args);

			string entry = "";
			build = null;

			if(silent == false) {
				Console.WriteLine("Welcome to SQL#. The interactive SQL command-line client ");
				Console.WriteLine("for Mono.Data.  See http://www.go-mono.com/ for more details.\n");
						
				StartupHelp();
				ShowDefaults();
			}
			
			while(entry.ToUpper().Equals("\\Q") == false &&
				entry.ToUpper().Equals("\\QUIT") == false) {
				
				while((entry = ReadSqlSharpCommand()) == "") {}
			
				
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
						//build.Append("\n");
						buff = build.ToString();
						ExecuteSql(buff);
						build = null;
					}
				}
				else {
					// most likely a part of a SQL Command or Query found
					// append this part of the SQL
					if(build == null) {
						build = new StringBuilder();
					}
					build.Append(entry + "\n");
					buff = build.ToString();
				}
			}			
			CloseDataSource();
			if(outputFilestream != null)
				outputFilestream.Close();
		}
	}

	public enum BindVariableCharacter {
		Colon,         // ':'  - named parameter - :name
		At,            // '@'  - named parameter - @name
		QuestionMark,  // '?'  - positioned parameter - ?
		SquareBrackets // '[]' - delimited named parameter - [name]
	}

	public class ParametersBuilder {

		private BindVariableCharacter bindCharSetting;
		private char bindChar;
		private IDataParameterCollection parms;
		private string sql;
		private IDbCommand cmd;
			
		private void SetBindCharacter() {
			switch(bindCharSetting) {
			case BindVariableCharacter.Colon:
				bindChar = ':';
				break;
			case BindVariableCharacter.At:
				bindChar = '@';
				break;
			case BindVariableCharacter.SquareBrackets:
				bindChar = '[';
				break;
			case BindVariableCharacter.QuestionMark:
				bindChar = '?';
				break;
			}
		}

		public ParametersBuilder(IDbCommand command, BindVariableCharacter bindVarChar) {
			cmd = command;
			sql = cmd.CommandText;
			parms = cmd.Parameters;
			bindCharSetting = bindVarChar;
			SetBindCharacter();
		}	

		public char ParameterMarkerCharacter {
			get {
				return bindChar;
			}
		}

		public int ParseParameters() {	

			int numParms = 0;

			IDataParameterCollection parms = cmd.Parameters;

			char[] chars = sql.ToCharArray();
			bool bStringConstFound = false;

			for(int i = 0; i < chars.Length; i++) {
				if(chars[i] == '\'') {
					if(bStringConstFound == true)
						bStringConstFound = false;
					else
						bStringConstFound = true;
				}
				else if(chars[i] == bindChar && 
					bStringConstFound == false) {
					if(bindChar != '?') {
						StringBuilder parm = new StringBuilder();
						i++;
						if(bindChar.Equals('[')) {
							bool endingBracketFound = false;
							while(i <= chars.Length) {
								char ch;
								if(i == chars.Length)
									ch = ' '; // a space
								else
									ch = chars[i];

								if(Char.IsLetterOrDigit(ch) || ch == ' ') {
									parm.Append(ch);
								}
								else if (ch == ']') {
									endingBracketFound = true;
									string p = parm.ToString();
									AddParameter(p);
									numParms ++;
									break;
								}
								else throw new Exception("SQL Parser Error: Invalid character in parameter name");
								i++;
							}
							i--;
							if(endingBracketFound == false)
								throw new Exception("SQL Parser Error: Ending bracket not found for parameter");
						}
						else {
							while(i <= chars.Length) {
								char ch;
								if(i == chars.Length)
									ch = ' '; // a space
								else
									ch = chars[i];

								if(Char.IsLetterOrDigit(ch)) {
									parm.Append(ch);
								}
								else {

									string p = parm.ToString();
									AddParameter(p);
									numParms ++;
									break;
								}
								i++;
							}
							i--;
						}
					}
					else {
						// placeholder paramaeter for ?
						string p = numParms.ToString();
						AddParameter(p);
						numParms ++;
					}
				}			
			}
			return numParms;
		}

		public void AddParameter (string p) {
			Console.WriteLine("Add Parameter: " + p);
			if(parms.Contains(p) == false) {
				IDataParameter prm = cmd.CreateParameter();
				prm.ParameterName = p;
				prm.Direction = ParameterDirection.Input;
				prm.DbType = DbType.String; // default
				prm.Value = ""; // default
				cmd.Parameters.Add(prm);
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
