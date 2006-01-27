//
// System.Data.SqlClient.SqlCommand.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//   Diego Caravana (diego@toth.it)
//
// (C) Ximian, Inc 2002 http://www.ximian.com/
// (C) Daniel Morgan, 2002
// Copyright (C) Tim Coleman, 2002
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using Mono.Data.Tds;
using Mono.Data.Tds.Protocol;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
#if NET_2_0
using System.Data.ProviderBase;
#endif // NET_2_0
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace System.Data.SqlClient {
	[DesignerAttribute ("Microsoft.VSDesigner.Data.VS.SqlCommandDesigner, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.IDesigner")]
	 [ToolboxItemAttribute ("System.Drawing.Design.ToolboxItem, "+ Consts.AssemblySystem_Drawing)]
#if NET_2_0
	public sealed class SqlCommand : DbCommandBase, IDbCommand, ICloneable
#else
        public sealed class SqlCommand : Component, IDbCommand, ICloneable
#endif // NET_2_0
	{
		#region Fields

		bool disposed = false;
		int commandTimeout;
		bool designTimeVisible;
		string commandText;
		CommandType commandType;
		SqlConnection connection;
		SqlTransaction transaction;
		UpdateRowSource updatedRowSource;
		CommandBehavior behavior = CommandBehavior.Default;
		SqlParameterCollection parameters;
		string preparedStatement = null;

		#endregion // Fields

		#region Constructors

		public SqlCommand() 
			: this (String.Empty, null, null)
		{
		}

		public SqlCommand (string commandText) 
			: this (commandText, null, null)
		{
		}

		public SqlCommand (string commandText, SqlConnection connection) 
			: this (commandText, connection, null)
		{
		}

		public SqlCommand (string commandText, SqlConnection connection, SqlTransaction transaction) 
		{
			this.commandText = commandText;
			this.connection = connection;
			this.transaction = transaction;
			this.commandType = CommandType.Text;
			this.updatedRowSource = UpdateRowSource.Both;

			this.designTimeVisible = false;
			this.commandTimeout = 30;
			parameters = new SqlParameterCollection (this);
		}

		private SqlCommand(string commandText, SqlConnection connection, SqlTransaction transaction, CommandType commandType, UpdateRowSource updatedRowSource, bool designTimeVisible, int commandTimeout, SqlParameterCollection parameters)
		{
			this.commandText = commandText;
			this.connection = connection;
			this.transaction = transaction;
			this.commandType = commandType;
			this.updatedRowSource = updatedRowSource;
			this.designTimeVisible = designTimeVisible;
			this.commandTimeout = commandTimeout;
			this.parameters = new SqlParameterCollection(this);
			for (int i = 0;i < parameters.Count;i++)
			      this.parameters.Add(((ICloneable)parameters[i]).Clone());	
		}
		#endregion // Constructors

		#region Properties

		internal CommandBehavior CommandBehavior {
			get { return behavior; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Command text to execute.")]
		[DefaultValue ("")]
		[EditorAttribute ("Microsoft.VSDesigner.Data.SQL.Design.SqlCommandTextEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		[RefreshProperties (RefreshProperties.All)]
		public 
#if NET_2_0
                override 
#endif //NET_2_0
                string CommandText {
			get { return commandText; }
			set { 
				if (value != commandText && preparedStatement != null)
					Unprepare ();
				commandText = value; 
			}
		}

		[DataSysDescription ("Time to wait for command to execute.")]
		[DefaultValue (30)]
		public 
#if NET_2_0
                override
#endif //NET_2_0
                int CommandTimeout {
			get { return commandTimeout;  }
			set { 
				if (value < 0)
					throw new ArgumentException ("The property value assigned is less than 0.");
				commandTimeout = value; 
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("How to interpret the CommandText.")]
		[DefaultValue (CommandType.Text)]
		[RefreshProperties (RefreshProperties.All)]
		public 
#if NET_2_0
                override 
#endif //NET_2_0
                CommandType CommandType	{
			get { return commandType; }
			set { 
				if (value == CommandType.TableDirect)
					throw new ArgumentException ("CommandType.TableDirect is not supported by the Mono SqlClient Data Provider.");

				if (!Enum.IsDefined (typeof (CommandType), value))
					throw ExceptionHelper.InvalidEnumValueException ("CommandType", value);
				commandType = value; 
			}
		}

		[DataCategory ("Behavior")]
		[DefaultValue (null)]
		[DataSysDescription ("Connection used by the command.")]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DbConnectionEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]		
                public 
#if NET_2_0
                new
#endif //NET_2_0
                SqlConnection Connection {
			get { return connection; }
			set { 
				if (transaction != null && connection.Transaction != null && connection.Transaction.IsOpen)
					throw new InvalidOperationException ("The Connection property was changed while a transaction was in progress.");
				transaction = null;
				connection = value; 
			}
		}

		[Browsable (false)]
		[DefaultValue (true)]
		[DesignOnly (true)]
		public 
#if NET_2_0
                override
#endif //NET_2_0
                bool DesignTimeVisible {
			get { return designTimeVisible; } 
			set { designTimeVisible = value; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("The parameters collection.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public 
#if NET_2_0
                new 
#endif //NET_2_0
                SqlParameterCollection Parameters {
			get { return parameters; }
		}

		internal ITds Tds {
			get { return Connection.Tds; }
		}

		IDbConnection IDbCommand.Connection {
			get { return Connection; }
			set { 
				if (!(value is SqlConnection))
					throw new InvalidCastException ("The value was not a valid SqlConnection.");
				Connection = (SqlConnection) value;
			}
		}

		IDataParameterCollection IDbCommand.Parameters	{
			get { return Parameters; }
		}

		IDbTransaction IDbCommand.Transaction {
			get { return Transaction; }
			set { 
				if (!(value is SqlTransaction))
					throw new ArgumentException ();
				Transaction = (SqlTransaction) value; 
			}
		}

		[Browsable (false)]
		[DataSysDescription ("The transaction used by the command.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new SqlTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
		}	

		[DataCategory ("Behavior")]
		[DataSysDescription ("When used by a DataAdapter.Update, how command results are applied to the current DataRow.")]
		[DefaultValue (UpdateRowSource.Both)]
		public 
#if NET_2_0
                override
#endif // NET_2_0
                UpdateRowSource UpdatedRowSource	{
			get { return updatedRowSource; }
			set { 
				if (!Enum.IsDefined (typeof (UpdateRowSource), value))
					throw ExceptionHelper.InvalidEnumValueException ("UpdateRowSource", value);
				updatedRowSource = value;
			}
		}

		#endregion // Fields

		#region Methods

		public 
#if NET_2_0
                override
#endif // NET_2_0
                void Cancel () 
		{
			if (Connection == null || Connection.Tds == null)
				return;
			Connection.Tds.Cancel ();
		}

		internal void CloseDataReader (bool moreResults)
		{
			Connection.DataReader = null;

			if ((behavior & CommandBehavior.CloseConnection) != 0)
				Connection.Close ();
		}

		public new SqlParameter CreateParameter () 
		{
			return new SqlParameter ();
		}

		internal void DeriveParameters ()
		{
			if (commandType != CommandType.StoredProcedure)
				throw new InvalidOperationException (String.Format ("SqlCommand DeriveParameters only supports CommandType.StoredProcedure, not CommandType.{0}", commandType));
			ValidateCommand ("DeriveParameters");

			SqlParameterCollection localParameters = new SqlParameterCollection (this);
			localParameters.Add ("@procedure_name", SqlDbType.NVarChar, commandText.Length).Value = commandText;

			string sql = "sp_procedure_params_rowset";

			Connection.Tds.ExecProc (sql, localParameters.MetaParameters, 0, true);

			SqlDataReader reader = new SqlDataReader (this);
			parameters.Clear ();
			object[] dbValues = new object[reader.FieldCount];

			while (reader.Read ()) {
				reader.GetValues (dbValues);
				parameters.Add (new SqlParameter (dbValues));
			}
			reader.Close ();	

		}

		private void Execute (CommandBehavior behavior, bool wantResults)
		{
                        Connection.Tds.RecordsAffected = -1;
			TdsMetaParameterCollection parms = Parameters.MetaParameters;
			if (preparedStatement == null) {
				bool schemaOnly = ((behavior & CommandBehavior.SchemaOnly) > 0);
				bool keyInfo = ((behavior & CommandBehavior.KeyInfo) > 0);

				StringBuilder sql1 = new StringBuilder ();
				StringBuilder sql2 = new StringBuilder ();

				if (schemaOnly || keyInfo)
					sql1.Append ("SET FMTONLY OFF;");
				if (keyInfo) {
					sql1.Append ("SET NO_BROWSETABLE ON;");
					sql2.Append ("SET NO_BROWSETABLE OFF;");
				}
				if (schemaOnly) {
					sql1.Append ("SET FMTONLY ON;");
					sql2.Append ("SET FMTONLY OFF;");
				}

				switch (CommandType) {
				case CommandType.StoredProcedure:
					if (keyInfo || schemaOnly)
						Connection.Tds.Execute (sql1.ToString ());
					Connection.Tds.ExecProc (CommandText, parms, CommandTimeout, wantResults);
					if (keyInfo || schemaOnly)
						Connection.Tds.Execute (sql2.ToString ());
					break;
				case CommandType.Text:
					string sql = String.Format ("{0}{1};{2}", sql1.ToString (), CommandText, sql2.ToString ());
					Connection.Tds.Execute (sql, parms, CommandTimeout, wantResults);
					break;
				}
			}
			else 
				Connection.Tds.ExecPrepared (preparedStatement, parms, CommandTimeout, wantResults);
		}

		public 
#if NET_2_0
                override
#endif // NET_2_0
                int ExecuteNonQuery ()
		{
			ValidateCommand ("ExecuteNonQuery");
			int result = 0;
                        behavior = CommandBehavior.Default;

			try {
				Execute (CommandBehavior.Default, false);
				result = Connection.Tds.RecordsAffected;
			}
			catch (TdsTimeoutException e) {
				throw SqlException.FromTdsInternalException ((TdsInternalException) e);
			}

			GetOutputParameters ();
			return result;
		}

		public new SqlDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

		public new SqlDataReader ExecuteReader (CommandBehavior behavior)
		{
			ValidateCommand ("ExecuteReader");
			try {
                                this.behavior = behavior;
				Execute (behavior, true);
                                Connection.DataReader = new SqlDataReader (this);
			}
			catch (TdsTimeoutException e) {
                                // if behavior is closeconnection, even if it throws exception
                                // the connection has to be closed.
                                if ((behavior & CommandBehavior.CloseConnection) != 0)
                                        Connection.Close ();
                                throw SqlException.FromTdsInternalException ((TdsInternalException) e);
			} catch (SqlException) {
                                // if behavior is closeconnection, even if it throws exception
                                // the connection has to be closed.
                                if ((behavior & CommandBehavior.CloseConnection) != 0)
                                        Connection.Close ();

                                throw;
                        }

                        return Connection.DataReader;
		}

		public 
#if NET_2_0
                override
#endif // NET_2_0
                object ExecuteScalar ()
		{
			ValidateCommand ("ExecuteScalar");
                        behavior = CommandBehavior.Default;
			try {
				Execute (CommandBehavior.Default, true);
			}
			catch (TdsTimeoutException e) {
				throw SqlException.FromTdsInternalException ((TdsInternalException) e);
			}

			if (!Connection.Tds.NextResult () || !Connection.Tds.NextRow ())
				return null;

			object result = Connection.Tds.ColumnValues [0];
			CloseDataReader (true);
			return result;
		}

		public XmlReader ExecuteXmlReader ()
		{
			ValidateCommand ("ExecuteXmlReader");
                        behavior = CommandBehavior.Default;
			try { 
				Execute (CommandBehavior.Default, true);
			}
			catch (TdsTimeoutException e) {
				throw SqlException.FromTdsInternalException ((TdsInternalException) e);
			}

			SqlDataReader dataReader = new SqlDataReader (this);
			SqlXmlTextReader textReader = new SqlXmlTextReader (dataReader);
			XmlReader xmlReader = new XmlTextReader (textReader);
			return xmlReader;
		}

		internal void GetOutputParameters ()
		{
			IList list = Connection.Tds.OutputParameters;

			if (list != null && list.Count > 0) {

				int index = 0;
				foreach (SqlParameter parameter in parameters) {
					if (parameter.Direction != ParameterDirection.Input) {
						parameter.Value = list [index];
						index += 1;
					}
					if (index >= list.Count)
						break;
				}
			}
		}

		object ICloneable.Clone ()
		{
			return new SqlCommand (commandText, connection, transaction, commandType, updatedRowSource, designTimeVisible, commandTimeout,  parameters);

		}

		IDbDataParameter IDbCommand.CreateParameter ()
		{
			return CreateParameter ();
		}

		IDataReader IDbCommand.ExecuteReader ()
		{
			return ExecuteReader ();
		}

		IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
		{
			return ExecuteReader (behavior);
		}

		public 
#if NET_2_0
                override
#endif // NET_2_0
                void Prepare ()
		{
			ValidateCommand ("Prepare");

			if (CommandType == CommandType.StoredProcedure)
				return;

			try {
				foreach (SqlParameter param in Parameters)
					param.CheckIfInitialized ();
			}catch (Exception e) {
				throw new InvalidOperationException ("SqlCommand.Prepare requires " + e.Message);
			}

			preparedStatement = Connection.Tds.Prepare (CommandText, Parameters.MetaParameters);
		}

		public 
#if NET_2_0
                override
#endif // NET_2_0
                void ResetCommandTimeout ()
		{
			commandTimeout = 30;
		}

		private void Unprepare ()
		{
			Connection.Tds.Unprepare (preparedStatement);
			preparedStatement = null;
		}

		private void ValidateCommand (string method)
		{
			if (Connection == null)
				throw new InvalidOperationException (String.Format ("{0} requires a Connection object to continue.", method));
			if (Connection.Transaction != null && transaction != Connection.Transaction)
				throw new InvalidOperationException ("The Connection object does not have the same transaction as the command object.");
			if (Connection.State != ConnectionState.Open)
				throw new InvalidOperationException (String.Format ("ExecuteNonQuery requires an open Connection object to continue. This connection is closed.", method));
			if (commandText == String.Empty || commandText == null)
				throw new InvalidOperationException ("The command text for this Command has not been set.");
			if (Connection.DataReader != null)
				throw new InvalidOperationException ("There is already an open DataReader associated with this Connection which must be closed first.");
			if (Connection.XmlReader != null)
				throw new InvalidOperationException ("There is already an open XmlReader associated with this Connection which must be closed first.");
#if NET_2_0
                        if (method.StartsWith ("Begin") && !Connection.AsyncProcessing)
                                throw new InvalidOperationException ("This Connection object is not " + 
                                                                     "in Asynchronous mode. Use 'Asynchronous" +
                                                                     " Processing = true' to set it.");
#endif // NET_2_0
		}

#if NET_2_0
                [MonoTODO]
                protected override DbParameter CreateDbParameter ()
                {
                        return (DbParameter) CreateParameter ();
                }

                [MonoTODO]
                protected override DbDataReader ExecuteDbDataReader (CommandBehavior behavior)
                {
                        return (DbDataReader) ExecuteReader (behavior);
                }

                [MonoTODO]                
                protected override DbConnection DbConnection 
                {
                        get { return (DbConnection) Connection;  }
                        set { Connection = (SqlConnection) value; }
                }
                
                [MonoTODO]
                protected override DbParameterCollection DbParameterCollection
                {
                        get { return (DbParameterCollection) Parameters; }
                }

                [MonoTODO]
                protected override DbTransaction DbTransaction 
                {
                        get { return (DbTransaction) Transaction; }
                        set { Transaction = (SqlTransaction) value; }
                }
#endif // NET_2_0

		#endregion // Methods

#if NET_2_0
                #region Asynchronous Methods

                internal IAsyncResult BeginExecuteInternal (CommandBehavior behavior, 
                                                            bool wantResults,
                                                            AsyncCallback callback, 
                                                            object state)
                {
                        IAsyncResult ar = null;
                        Connection.Tds.RecordsAffected = -1;
			TdsMetaParameterCollection parms = Parameters.MetaParameters;
			if (preparedStatement == null) {
				bool schemaOnly = ((behavior & CommandBehavior.SchemaOnly) > 0);
				bool keyInfo = ((behavior & CommandBehavior.KeyInfo) > 0);

				StringBuilder sql1 = new StringBuilder ();
				StringBuilder sql2 = new StringBuilder ();

				if (schemaOnly || keyInfo)
					sql1.Append ("SET FMTONLY OFF;");
				if (keyInfo) {
					sql1.Append ("SET NO_BROWSETABLE ON;");
					sql2.Append ("SET NO_BROWSETABLE OFF;");
				}
				if (schemaOnly) {
					sql1.Append ("SET FMTONLY ON;");
					sql2.Append ("SET FMTONLY OFF;");
				}

				switch (CommandType) {
				case CommandType.StoredProcedure:
                                        string prolog = "";
                                        string epilog = "";
					if (keyInfo || schemaOnly)
						prolog = sql1.ToString ();
                                        if (keyInfo || schemaOnly)
						epilog = sql2.ToString ();
                                        Connection.Tds.BeginExecuteProcedure (prolog,
                                                                              epilog,
                                                                              CommandText,
                                                                              !wantResults,
                                                                              parms,
                                                                              callback,
                                                                              state);
                                                                              
					break;
				case CommandType.Text:
					string sql = String.Format ("{0}{1};{2}", sql1.ToString (), CommandText, sql2.ToString ());
                                        if (wantResults)
                                                ar = Connection.Tds.BeginExecuteQuery (sql, parms, 
                                                                                       callback, state);
                                        else
                                                ar = Connection.Tds.BeginExecuteNonQuery (sql, parms, callback, state);
					break;
				}
			}
			else 
				Connection.Tds.ExecPrepared (preparedStatement, parms, CommandTimeout, wantResults);

                        return ar;

                }

                internal void EndExecuteInternal (IAsyncResult ar)
                {
                        SqlAsyncResult sqlResult = ( (SqlAsyncResult) ar);
                        Connection.Tds.WaitFor (sqlResult.InternalResult);
                        Connection.Tds.CheckAndThrowException (sqlResult.InternalResult);
                }

                public IAsyncResult BeginExecuteNonQuery ()
                {
                        return BeginExecuteNonQuery (null, null);
                }

                public IAsyncResult BeginExecuteNonQuery (AsyncCallback callback, object state)
                {
                        ValidateCommand ("BeginExecuteNonQuery");
                        SqlAsyncResult ar = new SqlAsyncResult (callback, state);
                        ar.EndMethod = "EndExecuteNonQuery";
                        ar.InternalResult = BeginExecuteInternal (CommandBehavior.Default, false, ar.BubbleCallback, ar);
                        return ar;
                }

                public int EndExecuteNonQuery (IAsyncResult ar)
                {
                        ValidateAsyncResult (ar, "EndExecuteNonQuery");
                        EndExecuteInternal (ar);
                        
			int ret = Connection.Tds.RecordsAffected;

                        GetOutputParameters ();
                        ( (SqlAsyncResult) ar).Ended = true;
                        return ret;
                }

                public IAsyncResult BeginExecuteReader ()
                {
                        return BeginExecuteReader (null, null, CommandBehavior.Default);
                }

                public IAsyncResult BeginExecuteReader (CommandBehavior behavior)
                {
                        return BeginExecuteReader (null, null, behavior);
                }
                
                public IAsyncResult BeginExecuteReader (AsyncCallback callback, object state)
                {
                        return BeginExecuteReader (callback, state, CommandBehavior.Default);
                }

                public IAsyncResult BeginExecuteReader (AsyncCallback callback, object state, CommandBehavior behavior)
                {
                        ValidateCommand ("BeginExecuteReader");
                        this.behavior = behavior;
                        SqlAsyncResult ar = new SqlAsyncResult (callback, state);
                        ar.EndMethod = "EndExecuteReader";
                        IAsyncResult tdsResult = BeginExecuteInternal (behavior, true, 
                                                                       ar.BubbleCallback, state);
                        ar.InternalResult = tdsResult;
                        return ar;
                }

                public SqlDataReader EndExecuteReader (IAsyncResult ar)
                {
                        ValidateAsyncResult (ar, "EndExecuteReader");
                        EndExecuteInternal (ar);
                        SqlDataReader reader = null;
                        try {
                                reader = new SqlDataReader (this);
			}
			catch (TdsTimeoutException e) {
                                // if behavior is closeconnection, even if it throws exception
                                // the connection has to be closed.
                                if ((behavior & CommandBehavior.CloseConnection) != 0)
                                        Connection.Close ();
                                throw SqlException.FromTdsInternalException ((TdsInternalException) e);
			} catch (SqlException) {
                                // if behavior is closeconnection, even if it throws exception
                                // the connection has to be closed.
                                if ((behavior & CommandBehavior.CloseConnection) != 0)
                                        Connection.Close ();

                                throw;
                        }

                        ( (SqlAsyncResult) ar).Ended = true;
                        return reader;
                }

                public IAsyncResult BeginExecuteXmlReader (AsyncCallback callback, object state)
                {
                        ValidateCommand ("BeginExecuteXmlReader");
                        SqlAsyncResult ar = new SqlAsyncResult (callback, state);
                        ar.EndMethod = "EndExecuteXmlReader";
                        ar.InternalResult = BeginExecuteInternal (behavior, true, 
                                                                       ar.BubbleCallback, state);
                        return ar;
                }

                public XmlReader EndExecuteXmlReader (IAsyncResult ar)
                {
                        ValidateAsyncResult (ar, "EndExecuteXmlReader");
                        EndExecuteInternal (ar);
                        SqlDataReader reader = new SqlDataReader (this);
                        SqlXmlTextReader textReader = new SqlXmlTextReader (reader);
                        XmlReader xmlReader = new XmlTextReader (textReader);
                        ( (SqlAsyncResult) ar).Ended = true;
                        return xmlReader;
                }


                internal void ValidateAsyncResult (IAsyncResult ar, string endMethod)
                {
                        if (ar == null)
                                throw new ArgumentException ("result passed is null!");
                        if (! (ar is SqlAsyncResult))
                                throw new ArgumentException (String.Format ("cannot test validity of types {0}",
                                                                            ar.GetType ()
                                                                            ));
                        SqlAsyncResult result = (SqlAsyncResult) ar;
                        
                        if (result.EndMethod != endMethod)
                                throw new InvalidOperationException (String.Format ("Mismatched {0} called for AsyncResult. " + 
                                                                                    "Expected call to {1} but {0} is called instead.",
                                                                                    endMethod,
                                                                                    result.EndMethod
                                                                                    ));
                        if (result.Ended)
                                throw new InvalidOperationException (String.Format ("The method {0}  cannot be called " + 
                                                                                    "more than once for the same AsyncResult.",
                                                                                    endMethod));

                }

                #endregion // Asynchronous Methods
#endif // NET_2_0
	}
}
