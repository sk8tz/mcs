//
// System.Data.SqlClient.SqlCommand.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002 http://www.ximian.com/
// (C) Daniel Morgan, 2002
// (C) Copyright 2002 Tim Coleman
//
// Credits:
//    SQL and concepts were used from libgda 0.8.190 (GNOME Data Access)
//    http://www.gnome-db.org/
//    with permission from the authors of the
//    PostgreSQL provider in libgda:
//        Michael Lausch <michael@lausch.at>
//        Rodrigo Moya <rodrigo@gnome-db.org>
//        Vivien Malerba <malerba@gnome-db.org>
//        Gonzalo Paniagua Javier <gonzalo@gnome-db.org>
//

// use #define DEBUG_SqlCommand if you want to spew debug messages
// #define DEBUG_SqlCommand

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace System.Data.SqlClient {
	/// <summary>
	/// Represents a SQL statement that is executed 
	/// while connected to a SQL database.
	/// </summary>
	// public sealed class SqlCommand : Component, IDbCommand, ICloneable
	public sealed class SqlCommand : IDbCommand {

		#region Fields

		private string sql = "";
		private int timeout = 30; 
		// default is 30 seconds 
		// for command execution

		private SqlConnection conn = null;
		private SqlTransaction trans = null;
		private CommandType cmdType = CommandType.Text;
		private bool designTime = false;
		private SqlParameterCollection parmCollection = new 
			SqlParameterCollection();

		// SqlDataReader state data for ExecuteReader()
		private SqlDataReader dataReader = null;
		private string[] queries = null;
		private int currentQuery;
		private CommandBehavior cmdBehavior = CommandBehavior.Default;

		private ParmUtil parmUtil = null;
		
		#endregion // Fields

		#region Constructors

		public SqlCommand() {
			sql = "";
		}

		public SqlCommand (string cmdText) {
			sql = cmdText;
		}

		public SqlCommand (string cmdText, SqlConnection connection) {
			sql = cmdText;
			conn = connection;
		}

		public SqlCommand (string cmdText, SqlConnection connection, 
			SqlTransaction transaction) {
			sql = cmdText;
			conn = connection;
			trans = transaction;
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public void Cancel () {
			// FIXME: use non-blocking Exec for this
			throw new NotImplementedException ();
		}

		// FIXME: is this the correct way to return a stronger type?
		[MonoTODO]
		IDbDataParameter IDbCommand.CreateParameter () {
			return CreateParameter ();
		}

		[MonoTODO]
		public SqlParameter CreateParameter () {
			return new SqlParameter ();
		}

		public int ExecuteNonQuery () {	
			IntPtr pgResult; // PGresult
			int rowsAffected = -1;
			ExecStatusType execStatus;
			String rowsAffectedString;
			string query;

			if(conn.State != ConnectionState.Open)
				throw new InvalidOperationException(
					"ConnnectionState is not Open");

			query = TweakQuery(sql, cmdType);

			// FIXME: PQexec blocks 
			// while PQsendQuery is non-blocking
			// which is better to use?
			// int PQsendQuery(PGconn *conn,
			//        const char *query);

			// execute SQL command
			// uses internal property to get the PGConn IntPtr
			pgResult = PostgresLibrary.
				PQexec (conn.PostgresConnection, query);

			execStatus = PostgresLibrary.
				PQresultStatus (pgResult);
			
			if(execStatus == ExecStatusType.PGRES_COMMAND_OK) {
				rowsAffectedString = PostgresLibrary.
					PQcmdTuples (pgResult);

				if(rowsAffectedString != null)
					if(rowsAffectedString.Equals("") == false)
						rowsAffected = int.Parse(rowsAffectedString);

				PostgresLibrary.PQclear (pgResult);
				pgResult = IntPtr.Zero;
			}
			else {
				String errorMessage;
				
				errorMessage = PostgresLibrary.
					PQresStatus(execStatus);

				errorMessage += " " + PostgresLibrary.
					PQresultErrorMessage(pgResult);

				PostgresLibrary.PQclear (pgResult);
				pgResult = IntPtr.Zero;

				throw new SqlException(0, 0,
					errorMessage, 0, "",
					conn.DataSource, "SqlCommand", 0);
			}
			
			return rowsAffected;
		}
		
		[MonoTODO]
		IDataReader IDbCommand.ExecuteReader () {
			return ExecuteReader ();
		}

		[MonoTODO]
		public SqlDataReader ExecuteReader () {
			return ExecuteReader(CommandBehavior.Default);
		}

		[MonoTODO]
		IDataReader IDbCommand.ExecuteReader (
			CommandBehavior behavior) {
			return ExecuteReader (behavior);
		}

		[MonoTODO]
		public SqlDataReader ExecuteReader (CommandBehavior behavior) 
		{
			if(conn.State != ConnectionState.Open)
				throw new InvalidOperationException(
					"ConnectionState is not Open");

			cmdBehavior = behavior;

			queries = null;
			currentQuery = -1;
			dataReader = new SqlDataReader(this);

			if((behavior & CommandBehavior.SingleResult) == CommandBehavior.SingleResult) {
				queries = new String[1];
				queries[0] = sql;
			}
			else {
				queries = sql.Split(new Char[] {';'});			
			}

			dataReader.NextResult();
					
			return dataReader;
		}

		internal SqlResult NextResult() 
		{
			SqlResult res = new SqlResult();
			res.Connection = this.Connection;
			string statement;
		
			currentQuery++;

			if(currentQuery < queries.Length && queries[currentQuery].Equals("") == false) {
				statement = TweakQuery(queries[currentQuery], cmdType);
				ExecuteQuery(statement, res);
				res.ResultReturned = true;
			}
			else {
				res.ResultReturned = false;
			}

			return res;
		}

		private string TweakQuery(string query, CommandType commandType) {
			string statement = "";
			StringBuilder td;

#if DEBUG_SqlCommand
			Console.WriteLine("---------[][] TweakQuery() [][]--------");
			Console.WriteLine("CommandType: " + commandType + " CommandBehavior: " + cmdBehavior);
			Console.WriteLine("SQL before command type: " + query);
#endif						
			// finish building SQL based on CommandType
			switch(commandType) {
			case CommandType.Text:
				statement = query;
				break;
			case CommandType.StoredProcedure:
				statement = 
					"SELECT " + query + "()";
				break;
			case CommandType.TableDirect:
				// NOTE: this is for the PostgreSQL provider
				//       and for OleDb, according to the docs,
				//       an exception is thrown if you try to use
				//       this with SqlCommand
				string[] directTables = query.Split(
					new Char[] {','});	
										 
				td = new StringBuilder("SELECT * FROM ");
				
				for(int tab = 0; tab < directTables.Length; tab++) {
					if(tab > 0)
						td.Append(',');
					td.Append(directTables[tab]);
					// FIXME: if multipe tables, how do we
					//        join? based on Primary/Foreign Keys?
					//        Otherwise, a Cartesian Product happens
				}
				statement = td.ToString();
				break;
			default:
				// FIXME: throw an exception?
				statement = query;
				break;
			}
#if DEBUG_SqlCommand			
			Console.WriteLine("SQL after command type: " + statement);
#endif
			// TODO: this parameters utility
			//       currently only support input variables
			//       need todo output, input/output, and return.
#if DEBUG_SqlCommand
			Console.WriteLine("using ParmUtil in TweakQuery()...");
#endif
			parmUtil = new ParmUtil(statement, parmCollection);
#if DEBUG_SqlCommand
			Console.WriteLine("ReplaceWithParms...");
#endif

			statement = parmUtil.ReplaceWithParms();

#if DEBUG_SqlCommand
			Console.WriteLine("SQL after ParmUtil: " + statement);
#endif	
			return statement;
		}

		private void ExecuteQuery (string query, SqlResult res)
		{			
			IntPtr pgResult;
		
			ExecStatusType execStatus;	

			if(conn.State != ConnectionState.Open)
				throw new InvalidOperationException(
					"ConnectionState is not Open");

			// FIXME: PQexec blocks 
			// while PQsendQuery is non-blocking
			// which is better to use?
			// int PQsendQuery(PGconn *conn,
			//        const char *query);

			// execute SQL command
			// uses internal property to get the PGConn IntPtr
			pgResult = PostgresLibrary.
				PQexec (conn.PostgresConnection, query);

			execStatus = PostgresLibrary.
				PQresultStatus (pgResult);
			
			if(execStatus == ExecStatusType.PGRES_TUPLES_OK) {
				res.BuildTableSchema(pgResult);
			}
			else {
				String errorMessage;
				
				errorMessage = PostgresLibrary.
					PQresStatus(execStatus);

				errorMessage += " " + PostgresLibrary.
					PQresultErrorMessage(pgResult);

				PostgresLibrary.PQclear (pgResult);
				pgResult = IntPtr.Zero;

				throw new SqlException(0, 0,
					errorMessage, 0, "",
					conn.DataSource, "SqlCommand", 0);
			}
		}

		// since SqlCommand has resources so SqlDataReader
		// can do Read() and NextResult(), need to free
		// those resources.  Also, need to allow this SqlCommand
		// and this SqlConnection to do things again.
		internal void CloseReader() {
			conn.OpenReader = false;
			dataReader = null;
			queries = null;
		}

		/// <summary>
		/// ExecuteScalar is used to retrieve one object
		/// from one result set 
		/// that has one row and one column.
		/// It is lightweight compared to ExecuteReader.
		/// </summary>
		[MonoTODO]
		public object ExecuteScalar () {
			IntPtr pgResult; // PGresult
			ExecStatusType execStatus;	
			object obj = null; // return
			int nRow = 0; // first row
			int nCol = 0; // first column
			String value;
			int nRows;
			int nFields;
			string query;

			if(conn.State != ConnectionState.Open)
				throw new InvalidOperationException(
					"ConnnectionState is not Open");

			query = TweakQuery(sql, cmdType);

			// FIXME: PQexec blocks 
			// while PQsendQuery is non-blocking
			// which is better to use?
			// int PQsendQuery(PGconn *conn,
			//        const char *query);

			// execute SQL command
			// uses internal property to get the PGConn IntPtr
			pgResult = PostgresLibrary.
				PQexec (conn.PostgresConnection, query);

			execStatus = PostgresLibrary.
				PQresultStatus (pgResult);
			
			if(execStatus == ExecStatusType.PGRES_TUPLES_OK) {
				nRows = PostgresLibrary.
					PQntuples(pgResult);

				nFields = PostgresLibrary.
					PQnfields(pgResult);

				if(nRows > 0 && nFields > 0) {

					// get column name
					//String fieldName;
					//fieldName = PostgresLibrary.
					//	PQfname(pgResult, nCol);

					int oid;
					string sType;
					DbType dbType;
					// get PostgreSQL data type (OID)
					oid = PostgresLibrary.
						PQftype(pgResult, nCol);
					sType = PostgresHelper.
						OidToTypname (oid, conn.Types);
					dbType = PostgresHelper.
						TypnameToSqlDbType(sType);

					int definedSize;
					// get defined size of column
					definedSize = PostgresLibrary.
						PQfsize(pgResult, nCol);

					// get data value
					value = PostgresLibrary.
						PQgetvalue(
						pgResult,
						nRow, nCol);

					int columnIsNull;
					// is column NULL?
					columnIsNull = PostgresLibrary.
						PQgetisnull(pgResult,
						nRow, nCol);

					int actualLength;
					// get Actual Length
					actualLength = PostgresLibrary.
						PQgetlength(pgResult,
						nRow, nCol);
						
					obj = PostgresHelper.
						ConvertDbTypeToSystem (
						dbType,
						value);
				}

				// close result set
				PostgresLibrary.PQclear (pgResult);
				pgResult = IntPtr.Zero;

			}
			else {
				String errorMessage;
				
				errorMessage = PostgresLibrary.
					PQresStatus(execStatus);

				errorMessage += " " + PostgresLibrary.
					PQresultErrorMessage(pgResult);

				PostgresLibrary.PQclear (pgResult);
				pgResult = IntPtr.Zero;

				throw new SqlException(0, 0,
					errorMessage, 0, "",
					conn.DataSource, "SqlCommand", 0);
			}
					
			return obj;
		}

		[MonoTODO]
		public XmlReader ExecuteXmlReader () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Prepare () {
			// FIXME: parameters have to be implemented for this
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlCommand Clone () {
			throw new NotImplementedException ();
		}

		#endregion // Methods

		#region Properties

		public string CommandText {
			get { 
				return sql; 
			}

			set { 
				sql = value; 
			}
		}

		public int CommandTimeout {
			get { 
				return timeout;  
			}
			
			set {
				// FIXME: if value < 0, throw
				// ArgumentException
				// if (value < 0)
				//	throw ArgumentException;
				timeout = value;
			}
		}

		public CommandType CommandType	{
			get {
				return cmdType;
			}

			set { 
				cmdType = value;
			}
		}

		// FIXME: for property Connection, is this the correct
		//        way to handle a return of a stronger type?
		IDbConnection IDbCommand.Connection {
			get { 
				return Connection;
			}

			set { 
				// FIXME: throw an InvalidOperationException
				// if the change was during a 
				// transaction in progress

				// csc
				Connection = (SqlConnection) value; 
				// mcs
				// Connection = value; 
				
				// FIXME: set Transaction property to null
			}
		}
		
		public SqlConnection Connection {
			get { 
				// conn defaults to null
				return conn;
			}

			set { 
				// FIXME: throw an InvalidOperationException
				// if the change was during 
				// a transaction in progress
				conn = value; 
				// FIXME: set Transaction property to null
			}
		}

		public bool DesignTimeVisible {
			get {
				return designTime;
			} 
			
			set{
				designTime = value;
			}
		}

		// FIXME; for property Parameters, is this the correct
		//        way to handle a stronger return type?
		IDataParameterCollection IDbCommand.Parameters	{
			get { 
				return Parameters;
			}
		}

		public SqlParameterCollection Parameters {
			get { 
				return parmCollection;
			}
		}

		// FIXME: for property Transaction, is this the correct
		//        way to handle a return of a stronger type?
		IDbTransaction IDbCommand.Transaction 	{
			get { 
				return Transaction;
			}

			set { 
				// FIXME: error handling - do not allow
				// setting of transaction if transaction
				// has already begun

				// csc
				Transaction = (SqlTransaction) value;
				// mcs
				// Transaction = value; 
			}
		}

		public SqlTransaction Transaction {
			get { 
				return trans; 
			}

			set { 
				// FIXME: error handling
				trans = value; 
			}
		}	

		[MonoTODO]
		public UpdateRowSource UpdatedRowSource	{
			// FIXME: do this once DbDataAdaptor 
			// and DataRow are done
			get { 		
				throw new NotImplementedException (); 
			}
			set { 
				throw new NotImplementedException (); 
			}
		}

		#endregion // Properties

		#region Inner Classes

		#endregion // Inner Classes

		#region Destructors

		[MonoTODO]
		public void Dispose() {
			// FIXME: need proper way to release resources
			// Dispose(true);
		}

		[MonoTODO]
		~SqlCommand() {
			// FIXME: need proper way to release resources
			// Dispose(false);
		}

		#endregion //Destructors
	}

	// SqlResult is used for passing Result Set data 
	// from SqlCommand to SqlDataReader
	internal class SqlResult {

		private DataTable dataTableSchema; // only will contain the schema
		private IntPtr pg_result; // native PostgreSQL PGresult
		private int rowCount; 
		private int fieldCount;
		private string[] pgtypes; // PostgreSQL types (typname)
		private bool resultReturned = false;
		private SqlConnection con;

		internal SqlConnection Connection {
			set {
				con = value;
			}
		}

		internal bool ResultReturned {
			get {
				return resultReturned;
			}
			set {
				resultReturned = value;
			}
		}

		internal DataTable Table {
			get { 
				return dataTableSchema;
			}
		}

		internal IntPtr PgResult {
			get {
				return pg_result;
			}
		}

		internal int RowCount {
			get {
				return rowCount;
			}
		}

		internal int FieldCount {
			get {
				return fieldCount;
			}
		}

		internal string[] PgTypes {
			get {
				return pgtypes;
			}
		}

		internal void BuildTableSchema (IntPtr pgResult) 
		{
			dataTableSchema = new DataTable ();
			dataTableSchema.Columns.Add ("ColumnName", typeof (string));
			dataTableSchema.Columns.Add ("ColumnOrdinal", typeof (int));
			dataTableSchema.Columns.Add ("ColumnSize", typeof (int));
			dataTableSchema.Columns.Add ("NumericPrecision", typeof (int));
			dataTableSchema.Columns.Add ("NumericScale", typeof (int));
			dataTableSchema.Columns.Add ("IsUnique", typeof (bool));
			dataTableSchema.Columns.Add ("IsKey", typeof (bool));
			dataTableSchema.Columns.Add ("BaseCatalogName", typeof (string));
			dataTableSchema.Columns.Add ("BaseColumnName", typeof (string));
			dataTableSchema.Columns.Add ("BaseSchemaName", typeof (string));
			dataTableSchema.Columns.Add ("BaseTableName", typeof (string));
			dataTableSchema.Columns.Add ("DataType", typeof (System.Type));
			dataTableSchema.Columns.Add ("AllowDBNull", typeof (bool));
			dataTableSchema.Columns.Add ("ProviderType", typeof (string));
			dataTableSchema.Columns.Add ("IsAliased", typeof (bool));
			dataTableSchema.Columns.Add ("IsExpression", typeof (bool));
			dataTableSchema.Columns.Add ("IsIdentity", typeof (bool));
			dataTableSchema.Columns.Add ("IsAutoIncrement", typeof (bool));
			dataTableSchema.Columns.Add ("IsRowVersion", typeof (bool));
			dataTableSchema.Columns.Add ("IsHidden", typeof (bool));
			dataTableSchema.Columns.Add ("IsLong", typeof (bool));
			dataTableSchema.Columns.Add ("IsReadOnly", typeof (bool));

			int fieldCount = PostgresLibrary.PQnfields (pgResult);
			DataRow schemaRow;
			int oid;
			string pgType;
			DbType dbType;
			

			for (int i = 0; i < fieldCount; i += 1 )
			{
				oid = PostgresLibrary.PQftype (pgResult, i);

				schemaRow = dataTableSchema.NewRow ();

				schemaRow["ColumnName"] = PostgresLibrary.PQfname (pgResult, i);
				schemaRow["ColumnOrdinal"] = i+1;
				schemaRow["ColumnSize"] = PostgresLibrary.PQfsize (pgResult, i);
				schemaRow["NumericPrecision"] = 0; // ? tim
				schemaRow["NumericScale"] = 0; // ? tim
				schemaRow["IsUnique"] = false; // ? tim
				schemaRow["IsKey"] = false; // ? tim
				schemaRow["BaseCatalogName"] = ""; // ? tim
				schemaRow["BaseSchemaName"] = ""; // ? tim
				schemaRow["BaseTableName"]  = ""; // ? tim
				
				pgType = PostgresHelper.OidToTypname (oid, con.Types);
				dbType = PostgresHelper.TypnameToSqlDbType (pgType);
				//schemaRow["DataType"] = PostgresHelper.DbTypeToSystemType (dbType); ??? this gives a bad cast.

				schemaRow["AllowDBNull"] = false; // ? tim
				schemaRow["ProviderType"] = ""; // ? tim
				schemaRow["IsAliased"] = false; // ? tim
				schemaRow["IsExpression"] = false; // ? tim
				schemaRow["IsIdentity"] = false; // ? tim
				schemaRow["IsAutoIncrement"] = false; // ? tim
				schemaRow["IsRowVersion"] = false; // ? tim
				schemaRow["IsHidden"] = false; // ? tim
				schemaRow["IsLong"] = false; // ? tim
				schemaRow["IsReadOnly"] = false; // ? tim

				dataTableSchema.Rows.Add (schemaRow);
			}
		}
	}
}
