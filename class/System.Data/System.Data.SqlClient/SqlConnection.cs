//
// System.Data.SqlClient.SqlConnection.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
// (C) Daniel Morgan 2002
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

// use #define DEBUG_SqlConnection if you want to spew debug messages
// #define DEBUG_SqlConnection

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.SqlClient {

	/// <summary>
	/// Represents an open connection to a SQL data source
	/// </summary>
	//public sealed class SqlConnection : Component, IDbConnection,
	//	ICloneable

	public sealed class SqlConnection : IDbConnection, IDisposable
	{
		// FIXME: Need to implement class Component, 
		// and interfaces: ICloneable and IDisposable	

		#region Fields

		private PostgresTypes types = null;
		private IntPtr pgConn = IntPtr.Zero;    

		// PGConn (Postgres Connection)
		private string connectionString = "";    
		// OLE DB Connection String
		private string pgConnectionString = ""; 
		// PostgreSQL Connection String
		private SqlTransaction trans = null;
		private int connectionTimeout = 15;     
		// default for 15 seconds
		
		// connection parameters in connection string
		private string host = "";     
		// Name of host to connect to
		private string hostaddr = ""; 
		// IP address of host to connect to
		// should be in "n.n.n.n" format
		private string port = "";     
		// Port number to connect to at the server host
		private string dbname = "";   // The database name. 
		private string user = "";     // User name to connect as. 
		private string password = "";
		// Password to be used if the server 
		// demands password authentication.  		
		private string options = ""; 
		// Trace/debug options to be sent to the server. 
		private string tty = ""; 
		// A file or tty for optional 
		// debug output from the backend. 
		private string requiressl = "";
		// Set to 1 to require 
		// SSL connection to the backend. 
		// Libpq will then refuse to connect 
		// if the server does not 
		// support SSL. Set to 0 (default) to 
		// negotiate with server. 

		// connection state
		private ConnectionState conState = ConnectionState.Closed;
		
		// DataReader state
		private SqlDataReader rdr = null;
		private bool dataReaderOpen = false;
		// FIXME: if true, throw an exception if SqlConnection 
		//        is used for anything other than reading
		//        data using SqlDataReader
		
		private string versionString = "Unknown";

		#endregion // Fields

		#region Constructors

		// A lot of the defaults were initialized in the Fields
		[MonoTODO]
		public SqlConnection () {

		}
	
		[MonoTODO]
		public SqlConnection (String connectionString) {
			SetConnectionString (connectionString);
		}

		#endregion // Constructors

		#region Destructors

		[MonoTODO]
		public void Dispose () {	
			// FIXME: release resources properly
			Close ();
			// Dispose (true);
		}
	
		// aka Finalize
		// [ClassInterface(ClassInterfaceType.AutoDual)]
		[MonoTODO]
		~SqlConnection() {
			// FIXME: this class need 
			//        a destructor to release resources
			//        Also, take a look at Dispose
			// Dispose (false);
		}
		
		#endregion // Destructors

		#region Public Methods

		IDbTransaction IDbConnection.BeginTransaction () {
			return BeginTransaction ();
		}

		public SqlTransaction BeginTransaction () {
			return TransactionBegin (); // call private method
		}

		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel 
			il) {
			return BeginTransaction (il);
		}

		public SqlTransaction BeginTransaction (IsolationLevel il) {
			return TransactionBegin (il); // call private method
		}

		// PostgreSQL does not support named transactions/savepoint
		//            nor nested transactions
		[Obsolete]
		public SqlTransaction BeginTransaction(string transactionName) {
			return TransactionBegin (); // call private method
		}

		[Obsolete]
		public SqlTransaction BeginTransaction(IsolationLevel iso,
			string transactionName) {
			return TransactionBegin (iso); // call private method
		}

		[MonoTODO]
		public void ChangeDatabase (string databaseName) {
			throw new NotImplementedException ();
		}
				
		[MonoTODO]
		public void Close () {
			if(dataReaderOpen == true) {
				// TODO: what do I do if
				// the user Closes the connection
				// without closing the Reader first?

			}			
			CloseDataSource ();
		}

		IDbCommand IDbConnection.CreateCommand () {
			return CreateCommand ();
		}

		public SqlCommand CreateCommand () {
			SqlCommand sqlcmd = new SqlCommand ("", this);

			return sqlcmd;
		}

		[MonoTODO]
		public void Open () {
			if(dbname.Equals(""))
				throw new InvalidOperationException(
					"dbname missing");
			else if(conState == ConnectionState.Open)
				throw new InvalidOperationException(
					"ConnnectionState is already Open");

			ConnStatusType connStatus;

			// FIXME: check to make sure we have 
			//        everything to connect,
			//        otherwise, throw an exception

			pgConn = PostgresLibrary.PQconnectdb 
				(pgConnectionString);

			// FIXME: should we use PQconnectStart/PQconnectPoll
			//        instead of PQconnectdb?  
			// PQconnectdb blocks 
			// PQconnectStart/PQconnectPoll is non-blocking
			
			connStatus = PostgresLibrary.PQstatus (pgConn);
			if(connStatus == ConnStatusType.CONNECTION_OK) {
				// Successfully Connected
				SetupConnection();
			}
			else {
				String errorMessage = PostgresLibrary.
					PQerrorMessage (pgConn);
				errorMessage += ": Could not connect to database.";

				throw new SqlException(0, 0,
					errorMessage, 0, "",
					host, "SqlConnection", 0);
			}
		}

		#endregion // Public Methods

		#region Protected Methods

		// FIXME: protected override void Dispose overrides Component
		//        however, including Component causes other problems
		/*
		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}
		*/

		#endregion

		#region Internal Methods

		// Used to prevent SqlConnection
		// from doing anything while
		// SqlDataReader is open.
		// Open the Reader. (called from SqlCommand)
		internal void OpenReader(SqlDataReader reader) 
		{	
			if(dataReaderOpen == true) {
				// TODO: throw exception here?
				//       because a reader
				//       is already open
			}
			else {
				rdr = reader;
				dataReaderOpen = true;
			}
		}

		// Used to prevent SqlConnection
		// from doing anything while
		// SqlDataReader is open
		// Close the Reader (called from SqlCommand)
		// if closeConnection true, Close() the connection
		// this is based on CommandBehavior.CloseConnection
		internal void CloseReader(bool closeConnection)
		{	if(closeConnection == true)
				CloseDataSource();
			else
				dataReaderOpen = false;
		}

		#endregion // Internal Methods

		#region Private Methods

		private void SetupConnection() {
			conState = ConnectionState.Open;

			// FIXME: load types into hashtable
			types = new PostgresTypes(this);
			types.Load();

			versionString = GetDatabaseServerVersion();

			// set DATE style to YYYY/MM/DD
			IntPtr pgResult = IntPtr.Zero;
			pgResult = PostgresLibrary.PQexec (pgConn, "SET DATESTYLE TO 'ISO'");
			PostgresLibrary.PQclear (pgResult);
			pgResult = IntPtr.Zero;
		}

		private string GetDatabaseServerVersion() 
		{
			SqlCommand cmd = new SqlCommand("select version()",this);
			return (string) cmd.ExecuteScalar();
		}

		private void CloseDataSource () {
			// FIXME: just a quick hack
			if(conState == ConnectionState.Open) {
				if(trans != null)
					if(trans.DoingTransaction == true) {
						trans.Rollback();
						// trans.Dispose();
						trans = null;
					}

				conState = ConnectionState.Closed;
				PostgresLibrary.PQfinish (pgConn);
				pgConn = IntPtr.Zero;
			}
		}

		private void SetConnectionString (string connectionString) {
			// FIXME: perform error checking on string
			// while translating string from 
			// OLE DB format to PostgreSQL 
			// connection string format
			//
			//     OLE DB: "host=localhost;dbname=test;user=joe;password=smoe"
			// PostgreSQL: "host=localhost dbname=test user=joe password=smoe"
			//
			// For OLE DB, you would have the additional 
			// "provider=postgresql"
			// OleDbConnection you would be using libgda, maybe
			// it would be 
			// "provider=OAFIID:GNOME_Database_Postgres_Provider"
			// instead.
			//
			// Also, parse the connection string into properties

			// FIXME: if connection is open, you can 
			//        not set the connection
			//        string, throw an exception

			this.connectionString = connectionString;
			pgConnectionString = ConvertStringToPostgres (
				connectionString);

#if DEBUG_SqlConnection
			Console.WriteLine(
				"OLE-DB Connection String    [in]: " +
				this.ConnectionString);
			Console.WriteLine(
				"Postgres Connection String [out]: " +
				pgConnectionString);
#endif // DEBUG_SqlConnection
		}

		private String ConvertStringToPostgres (String 
			oleDbConnectionString) {
			StringBuilder postgresConnection = 
				new StringBuilder();
			string result;
			string[] connectionParameters;

			char[] semicolon = new Char[1];
			semicolon[0] = ';';
			
			// FIXME: what is the max number of value pairs 
			//        can there be for the OLE DB 
			//	  connnection string? what about libgda max?
			//        what about postgres max?

			// FIXME: currently assuming value pairs are like:
			//        "key1=value1;key2=value2;key3=value3"
			//        Need to deal with values that have
			//        single or double quotes.  And error 
			//        handling of that too.
			//        "key1=value1;key2='value2';key=\"value3\""

			// FIXME: put the connection parameters 
			//        from the connection
			//        string into a 
			//        Hashtable (System.Collections)
			//        instead of using private variables 
			//        to store them
			connectionParameters = oleDbConnectionString.
				Split (semicolon);
			foreach (string sParameter in connectionParameters) {
				if(sParameter.Length > 0) {
					BreakConnectionParameter (sParameter);
					postgresConnection.
						Append (sParameter + 
						" ");
				}
			}
			result = postgresConnection.ToString ();
			return result;
		}

		private bool BreakConnectionParameter (String sParameter) {	
			bool addParm = true;
			int index;

			index = sParameter.IndexOf ("=");
			if (index > 0) {	
				string parmKey, parmValue;

				// separate string "key=value" to 
				// string "key" and "value"
				parmKey = sParameter.Substring (0, index);
				parmValue = sParameter.Substring (index + 1,
					sParameter.Length - index - 1);

				switch(parmKey.ToLower()) {
				case "hostaddr":
					hostaddr = parmValue;
					break;

				case "port":
					port = parmValue;
					break;

				case "host":
					// set DataSource property
					host = parmValue;
					break;

				case "dbname":
					// set Database property
					dbname = parmValue;
					break;

				case "user":
					user = parmValue;
					break;

				case "password":
					password = parmValue;
					//	addParm = false;
					break;

				case "options":
					options = parmValue;
					break;

				case "tty":
					tty = parmValue;
					break;
							
				case "requiressl":
					requiressl = parmValue;
					break;
				}
			}
			return addParm;
		}

		private SqlTransaction TransactionBegin () {
			// FIXME: need to keep track of 
			// transaction in-progress
			trans = new SqlTransaction ();
			// using internal methods of SqlTransaction
			trans.SetConnection (this);
			trans.Begin();

			return trans;
		}

		private SqlTransaction TransactionBegin (IsolationLevel il) {
			// FIXME: need to keep track of 
			// transaction in-progress
			TransactionBegin();
			trans.SetIsolationLevel (il);
			
			return trans;
		}

		#endregion

		#region Public Properties

		[MonoTODO]
		public ConnectionState State 		{
			get { 
				return conState;
			}
		}

		public string ConnectionString	{
			get { 
				return connectionString;
			}
			set { 
				SetConnectionString (value);
			}
		}
		
		public int ConnectionTimeout {
			get { 
				return connectionTimeout; 
			}
		}

		public string Database	{
			get { 
				return dbname; 
			}
		}

		public string DataSource {
			get {
				return host;
			}
		}

		/*
		 * FIXME: this is here because of Component?
		[MonoTODO]
		protected bool DesignMode {
			get { 
				throw new NotImplementedException (); 
			}
		}
		*/
		public int PacketSize {
			get { 
				throw new NotImplementedException ();
			}
		}

		public string ServerVersion {
			get { 
				return versionString;
			}
		}

		#endregion // Public Properties

		#region Internal Properties

		// For System.Data.SqlClient classes
		// to get the current transaction
		// in progress - if any
		internal SqlTransaction Transaction {
			get {
				return trans;
			}
		}

		// For System.Data.SqlClient classes 
		// to get the unmanaged PostgreSQL connection
		internal IntPtr PostgresConnection {
			get {
				return pgConn;
			}
		}

		// For System.Data.SqlClient classes
		// to get the list PostgreSQL types
		// so can look up based on OID to
		// get the .NET System type.
		internal ArrayList Types {
			get {
				return types.List;
			}
		}

		// Used to prevent SqlConnection
		// from doing anything while
		// SqlDataReader is open
		internal bool IsReaderOpen {
			get {
				return dataReaderOpen;
			}
		}

		#endregion // Internal Properties

		#region Events
                
		public event 
		SqlInfoMessageEventHandler InfoMessage;

		public event 
		StateChangeEventHandler StateChange;
		
		#endregion

		#region Inner Classes

		private class PostgresTypes {
			// TODO: create hashtable for 
			// PostgreSQL types to .NET types
			// containing: oid, typname, SqlDbType

			private Hashtable hashTypes;
			private ArrayList pgTypes;
			private SqlConnection con;

			// Got this SQL with the permission from 
			// the authors of libgda
			private const string SEL_SQL_GetTypes = 
				"SELECT oid, typname FROM pg_type " +
				"WHERE typrelid = 0 AND typname !~ '^_' " +
				" AND  typname not in ('SET', 'cid', " +
				"'int2vector', 'oidvector', 'regproc', " +
				"'smgr', 'tid', 'unknown', 'xid') " +
				"ORDER BY typname";

			internal PostgresTypes(SqlConnection sqlcon) {
				
				con = sqlcon;
				hashTypes = new Hashtable();
			}

			private void AddPgType(Hashtable types, 
				string typname, DbType dbType) {

				PostgresType pgType = new PostgresType();
			
				pgType.typname = typname;
				pgType.dbType = dbType;	

				types.Add(pgType.typname, pgType);
			}

			private void BuildTypes(IntPtr pgResult, 
				int nRows, int nFields) {

				String value;

				int r;
				for(r = 0; r < nRows; r++) {
					PostgresType pgType = 
						new PostgresType();

					// get data value (oid)
					value = PostgresLibrary.
						PQgetvalue(
							pgResult,
							r, 0);
						
					pgType.oid = Int32.Parse(value);

					// get data value (typname)
					value = PostgresLibrary.
						PQgetvalue(
						pgResult,
						r, 1);	
					pgType.typname = String.Copy(value);
					pgType.dbType = PostgresHelper.
							TypnameToSqlDbType(
								pgType.typname);

					pgTypes.Add(pgType);
				}
				pgTypes = ArrayList.ReadOnly(pgTypes);
			}

			internal void Load() {
				pgTypes = new ArrayList();
				IntPtr pgResult = IntPtr.Zero; // PGresult
				
				if(con.State != ConnectionState.Open)
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
					PQexec (con.PostgresConnection, SEL_SQL_GetTypes);

				if(pgResult.Equals(IntPtr.Zero)) {
					throw new SqlException(0, 0,
						"No Resultset from PostgreSQL", 0, "",
						con.DataSource, "SqlConnection", 0);
				}
				else {
					ExecStatusType execStatus;

					execStatus = PostgresLibrary.
						PQresultStatus (pgResult);
			
					if(execStatus == ExecStatusType.PGRES_TUPLES_OK) {
						int nRows;
						int nFields;

						nRows = PostgresLibrary.
							PQntuples(pgResult);

						nFields = PostgresLibrary.
							PQnfields(pgResult);

						BuildTypes (pgResult, nRows, nFields);

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

						// close result set
						PostgresLibrary.PQclear (pgResult);
						pgResult = IntPtr.Zero;

						throw new SqlException(0, 0,
							errorMessage, 0, "",
							con.DataSource, "SqlConnection", 0);
					}
				}
			}

			public ArrayList List {
				get {
					return pgTypes;
				}
			}
		}

		#endregion
	}
}
