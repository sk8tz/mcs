//
// System.Data.SqlClient.SqlCommand.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002 http://www.ximian.com/
// (C) Daniel Morgan, 2002
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
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Xml;

namespace System.Data.SqlClient {
	/// <summary>
	/// Represents a SQL statement that is executed 
	/// while connected to a SQL database.
	/// </summary>
	// public sealed class SqlCommand : Component, IDbCommand, ICloneable
	public sealed class SqlCommand : IDbCommand {
		// FIXME: Console.WriteLine() is used for debugging throughout

		#region Fields

		string sql = "";
		int timeout = 30; 
		// default is 30 seconds 
		// for command execution

		SqlConnection conn = null;
		SqlTransaction trans = null;
		CommandType cmdType = CommandType.Text;
		bool designTime = false;
		SqlParameterCollection parmCollection = new 
			SqlParameterCollection();

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

			if(conn.State != ConnectionState.Open)
				throw new InvalidOperationException(
					"ConnnectionState is not Open");

			// FIXME: PQexec blocks 
			// while PQsendQuery is non-blocking
			// which is better to use?
			// int PQsendQuery(PGconn *conn,
			//        const char *query);

			// execute SQL command
			// uses internal property to get the PGConn IntPtr
			pgResult = PostgresLibrary.
				PQexec (conn.PostgresConnection, sql);

			execStatus = PostgresLibrary.
				PQresultStatus (pgResult);
			
			if(execStatus == ExecStatusType.PGRES_COMMAND_OK) {
				rowsAffectedString = PostgresLibrary.
					PQcmdTuples (pgResult);

				if(rowsAffectedString != null)
					if(rowsAffectedString.Equals("") == false)
						rowsAffected = int.Parse(rowsAffectedString);

				PostgresLibrary.PQclear (pgResult);
			}
			else {
				String errorMessage;
				
				errorMessage = PostgresLibrary.
					PQresStatus(execStatus);

				errorMessage += " " + PostgresLibrary.
					PQresultErrorMessage(pgResult);
				
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
		public SqlDataReader ExecuteReader (CommandBehavior behavior) {
			// FIXME: currently only works for a 
			//        single result set
			//        ExecuteReader can be used 
			//        for multiple result sets
			SqlDataReader dataReader = null;
			
			IntPtr pgResult; // PGresult
			ExecStatusType execStatus;	

			if(conn.State != ConnectionState.Open)
				throw new InvalidOperationException(
					"ConnnectionState is not Open");

			// FIXME: PQexec blocks 
			// while PQsendQuery is non-blocking
			// which is better to use?
			// int PQsendQuery(PGconn *conn,
			//        const char *query);

			// execute SQL command
			// uses internal property to get the PGConn IntPtr
			pgResult = PostgresLibrary.
				PQexec (conn.PostgresConnection, sql);

			execStatus = PostgresLibrary.
				PQresultStatus (pgResult);
			
			if(execStatus == ExecStatusType.PGRES_TUPLES_OK) {
				DataTable dt = null;
				int rows, cols;
				string[] types;
				
				// FIXME: maybe i should move the
				//        BuildTableSchema code
				//        to the SqlDataReader?
				dt = BuildTableSchema(pgResult, 
					out rows, out cols, out types);
				dataReader = new SqlDataReader(this, dt, pgResult,
					rows, cols, types);
			}
			else {
				String errorMessage;
				
				errorMessage = PostgresLibrary.
					PQresStatus(execStatus);

				errorMessage += " " + PostgresLibrary.
					PQresultErrorMessage(pgResult);
				
				throw new SqlException(0, 0,
					errorMessage, 0, "",
					conn.DataSource, "SqlCommand", 0);
			}
					
			return dataReader;
		}

		internal DataTable BuildTableSchema (IntPtr pgResult, 
			out int nRows, 
			out int nFields, 
			out string[] types) {

			int nCol;
			
			DataTable dt = new DataTable();

			nRows = PostgresLibrary.
				PQntuples(pgResult);

			nFields = PostgresLibrary.
				PQnfields(pgResult);
			
			int oid;
			types = new string[nFields];
			
			for(nCol = 0; nCol < nFields; nCol++) {						
				
				DbType dbType;

				// get column name
				String fieldName;
				fieldName = PostgresLibrary.
					PQfname(pgResult, nCol);

				// get PostgreSQL data type (OID)
				oid = PostgresLibrary.
					PQftype(pgResult, nCol);
				types[nCol] = PostgresHelper.
					OidToTypname (oid, conn.Types);
				
				int definedSize;
				// get defined size of column
				definedSize = PostgresLibrary.
					PQfsize(pgResult, nCol);
								
				// build the data column and add it the table
				DataColumn dc = new DataColumn(fieldName);

				dbType = PostgresHelper.
						TypnameToSqlDbType(types[nCol]);
				dc.DataType = PostgresHelper.
						DbTypeToSystemType(dbType);
				dc.MaxLength = definedSize;
				dc.SetTable(dt);
				
				dt.Columns.Add(dc);
			}
			return dt;
		}

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

			if(conn.State != ConnectionState.Open)
				throw new InvalidOperationException(
					"ConnnectionState is not Open");

			// FIXME: PQexec blocks 
			// while PQsendQuery is non-blocking
			// which is better to use?
			// int PQsendQuery(PGconn *conn,
			//        const char *query);

			// execute SQL command
			// uses internal property to get the PGConn IntPtr
			pgResult = PostgresLibrary.
				PQexec (conn.PostgresConnection, sql);

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

			}
			else {
				String errorMessage;
				
				errorMessage = PostgresLibrary.
					PQresStatus(execStatus);

				errorMessage += " " + PostgresLibrary.
					PQresultErrorMessage(pgResult);
				
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

		SqlParameterCollection Parameters {
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
}
