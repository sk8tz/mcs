//
// System.Data.Odbc.OdbcCommandBuilder
//
// Author:
//   Umadevi S (sumadevi@novell.com)
//   Sureshkumar T (tsureshkumar@novell.com)
//
// Copyright (C) Novell Inc, 2004
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

using System.Text;
using System.Data;
using System.Data.Common;
using System.ComponentModel;

namespace System.Data.Odbc
{
	/// <summary>
	/// Provides a means of automatically generating single-table commands used to reconcile changes made to a DataSet with the associated database. This class cannot be inherited.
	/// </summary>

#if NET_2_0
        public sealed class OdbcCommandBuilder : DbCommandBuilder
#else // 1_1
	public sealed class OdbcCommandBuilder : Component
#endif // NET_2_0
	{
		#region Fields

		private OdbcDataAdapter 	_adapter;
		private string 			_quotePrefix;
		private string 			_quoteSuffix;

		private DataTable		_schema;
		private string			_tableName;
		private OdbcCommand		_insertCommand;
		private OdbcCommand		_updateCommand;
		private OdbcCommand		_deleteCommand;

		bool _disposed;

		#endregion // Fields

		#region Constructors
		
		public OdbcCommandBuilder ()
		{
			_adapter = null;
			_quotePrefix = String.Empty;
			_quoteSuffix = String.Empty;
		}

		public OdbcCommandBuilder (OdbcDataAdapter adapter) 
			: this ()
		{
			DataAdapter = adapter;
		}

		#endregion // Constructors

		#region Properties

		[OdbcDescriptionAttribute ("The DataAdapter for which to automatically generate OdbcCommands")]
		[DefaultValue (null)]
		public
#if NET_2_0
                new
#endif // NET_2_0
                OdbcDataAdapter DataAdapter {
			get {
				return _adapter;
			}
			set {
				if (_adapter == value)
					return;
				
				if (_adapter != null)
					_adapter.RowUpdating -= new OdbcRowUpdatingEventHandler (OnRowUpdating);
				_adapter = value;
				if (_adapter != null)
					_adapter.RowUpdating += new OdbcRowUpdatingEventHandler (OnRowUpdating);

			}
		}

		private OdbcCommand SelectCommand
		{
			get {
				if (DataAdapter == null)
					return null;
				return DataAdapter.SelectCommand;
			}
		}

		private DataTable Schema 
		{
			get {
				if (_schema == null)
					RefreshSchema ();
				return _schema;
			}
		}
		
		private string TableName 
		{
			get {
				if (_tableName != String.Empty)
					return _tableName;

				DataRow [] schemaRows = Schema.Select ("BaseTableName is not null and BaseTableName <> ''");
				if (schemaRows.Length > 1) {
					string tableName = (string) schemaRows [0] ["BaseTableName"];
					foreach (DataRow schemaRow in schemaRows) {
						if ( (string) schemaRow ["BaseTableName"] != tableName)
							throw new InvalidOperationException ("Dynamic SQL generation is not supported against multiple base tables.");
					}
				}
				if (schemaRows.Length == 0)
					throw new InvalidOperationException ("Cannot determine the base table name. Cannot proceed");
				_tableName = schemaRows [0] ["BaseTableName"].ToString ();
				return _tableName;
			}
		}

		[BrowsableAttribute (false)]
		[OdbcDescriptionAttribute ("The prefix string wrapped around sql objects")]
                [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public
#if NET_2_0
                override
#endif // NET_2_0
                string QuotePrefix {
			get {
				return _quotePrefix;
			}
			set {
				_quotePrefix = value;
			}
		}

		[BrowsableAttribute (false)]
                [OdbcDescriptionAttribute ("The suffix string wrapped around sql objects")]
                [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public
#if NET_2_0
                override
#endif // NET_2_0
                string QuoteSuffix {
			get {
				return _quoteSuffix;
			}
			set {
				_quoteSuffix = value;
			}
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public static void DeriveParameters (OdbcCommand command) 
		{
			throw new NotImplementedException ();
		}

		protected override void Dispose (bool disposing) 
		{
			if (_disposed)
				return;
			
			if (disposing) {
				// dispose managed resource
				if (_insertCommand != null) _insertCommand.Dispose ();
				if (_updateCommand != null) _updateCommand.Dispose ();
				if (_deleteCommand != null) _deleteCommand.Dispose ();
				if (_schema != null) _insertCommand.Dispose ();

				_insertCommand = null;
				_updateCommand = null;
				_deleteCommand = null;
				_schema = null;
			}
			_disposed = true;
		}

		private bool IsUpdatable (DataRow schemaRow)
		{
			if ( (! schemaRow.IsNull ("IsAutoIncrement") && (bool) schemaRow ["IsAutoIncrement"])
			     || (! schemaRow.IsNull ("IsHidden") && (bool) schemaRow ["IsHidden"])
			     || (! schemaRow.IsNull ("IsExpression") && (bool) schemaRow ["IsExpression"])
			     || (! schemaRow.IsNull ("IsRowVersion") && (bool) schemaRow ["IsRowVersion"])
			     || (! schemaRow.IsNull ("IsReadOnly") && (bool) schemaRow ["IsReadOnly"])
			     )
				return false;
			return true;
		}
		
		private string GetColumnName (DataRow schemaRow)
		{
			string columnName = schemaRow.IsNull ("BaseColumnName") ? String.Empty : (string) schemaRow ["BaseColumnName"];
			if (columnName == String.Empty)
				columnName = schemaRow.IsNull ("ColumnName") ? String.Empty : (string) schemaRow ["ColumnName"];
			return columnName;
		}

		private OdbcParameter AddParameter (OdbcCommand cmd, string paramName, OdbcType odbcType,
						    int length, string sourceColumnName, DataRowVersion rowVersion)
		{
			OdbcParameter param;
			if (length >= 0 && sourceColumnName != String.Empty)
				param = cmd.Parameters.Add (paramName, odbcType, length, sourceColumnName);
			else
				param = cmd.Parameters.Add (paramName, odbcType);
			param.SourceVersion = rowVersion;
			return param;
		}

		/*
		 * creates where clause for optimistic concurrency
		 */
		private string CreateOptWhereClause (OdbcCommand command)
		{
			string [] whereClause = new string [Schema.Rows.Count];

			int count = 0;

			foreach (DataRow schemaRow in Schema.Rows) {
				
				// exclude non updatable columns
				if (! IsUpdatable (schemaRow))
					continue;

				string columnName = GetColumnName (schemaRow);
				if (columnName == String.Empty)
					throw new InvalidOperationException ("Cannot form delete command. Column name is missing!");

				bool 	allowNull 	= schemaRow.IsNull ("AllowDBNull") || (bool) schemaRow ["AllowDBNull"];
				OdbcType sqlDbType 	= schemaRow.IsNull ("ProviderType") ? OdbcType.VarChar : (OdbcType) schemaRow ["ProviderType"];
				int 	length 		= schemaRow.IsNull ("ColumnSize") ? -1 : (int) schemaRow ["ColumnSize"];

				if (allowNull) {
					whereClause [count] = String.Format ("((? = 1 AND {0} IS NULL) OR ({0} = ?))",
									      columnName);
					AddParameter (command, columnName, sqlDbType, length, columnName, DataRowVersion.Original);
					AddParameter (command, columnName, sqlDbType, length, columnName, DataRowVersion.Original);
				} else {
					whereClause [count] = String.Format ( "({0} = ?)", columnName);
					AddParameter (command, columnName, sqlDbType, length, columnName, DataRowVersion.Original);
				}

				count++;
			}

			return String.Join (" AND ", whereClause, 0, count);
		}

		public
#if NET_2_0
                new
#endif // NET_2_0
                OdbcCommand GetInsertCommand ()
		{
			// FIXME: check validity of adapter
			if (_insertCommand != null)
				return _insertCommand;

			if (_schema == null)
				RefreshSchema ();
			
			_insertCommand = new OdbcCommand ();
			_insertCommand.Connection = DataAdapter.SelectCommand.Connection;
			_insertCommand.Transaction = DataAdapter.SelectCommand.Transaction;
			_insertCommand.CommandType = CommandType.Text;
			_insertCommand.UpdatedRowSource = UpdateRowSource.None;

			string query = String.Format ("INSERT INTO {0}", QuoteIdentifier (TableName));
			string [] columns = new string [Schema.Rows.Count];
			string [] values  = new string [Schema.Rows.Count];

			int count = 0;

			foreach (DataRow schemaRow in Schema.Rows) {
				
				// exclude non updatable columns
				if (! IsUpdatable (schemaRow))
					continue;

				string columnName = GetColumnName (schemaRow);
				if (columnName == String.Empty)
					throw new InvalidOperationException ("Cannot form insert command. Column name is missing!");

				// create column string & value string
				columns [count] = QuoteIdentifier(columnName);
				values [count++] = "?";

				// create parameter and add
				OdbcType sqlDbType = schemaRow.IsNull ("ProviderType") ? OdbcType.VarChar : (OdbcType) schemaRow ["ProviderType"];
				int length = schemaRow.IsNull ("ColumnSize") ? -1 : (int) schemaRow ["ColumnSize"];

				AddParameter (_insertCommand, columnName, sqlDbType, length, columnName, DataRowVersion.Current);
			}

			query = String.Format ("{0} ({1}) VALUES ({2})", 
					       query, 
					       String.Join (", ", columns, 0, count),
					       String.Join (", ", values, 0, count) );
			_insertCommand.CommandText = query;
			return _insertCommand;
		}

#if NET_2_0
		[MonoTODO]
		public new OdbcCommand GetInsertCommand (bool option)
		{
			// FIXME: check validity of adapter
			if (_insertCommand != null)
				return _insertCommand;

			if (_schema == null)
				RefreshSchema ();

			if (option == false) {
				return GetInsertCommand ();
			} else {
				throw new NotImplementedException ();
			}
		}
#endif // NET_2_0
			
		public
#if NET_2_0
                new
#endif // NET_2_0
                OdbcCommand GetUpdateCommand ()
		{
			// FIXME: check validity of adapter
			if (_updateCommand != null)
				return _updateCommand;

			if (_schema == null)
				RefreshSchema ();
			
			_updateCommand = new OdbcCommand ();
			_updateCommand.Connection = DataAdapter.SelectCommand.Connection;
			_updateCommand.Transaction = DataAdapter.SelectCommand.Transaction;
			_updateCommand.CommandType = CommandType.Text;
			_updateCommand.UpdatedRowSource = UpdateRowSource.None;

			string query = String.Format ("UPDATE {0} SET", QuoteIdentifier (TableName));
			string [] setClause = new string [Schema.Rows.Count];

			int count = 0;

			foreach (DataRow schemaRow in Schema.Rows) {
				
				// exclude non updatable columns
				if (! IsUpdatable (schemaRow))
					continue;

				string columnName = GetColumnName (schemaRow);
				if (columnName == String.Empty)
					throw new InvalidOperationException ("Cannot form update command. Column name is missing!");

				OdbcType sqlDbType = schemaRow.IsNull ("ProviderType") ? OdbcType.VarChar : (OdbcType) schemaRow ["ProviderType"];
				int length = schemaRow.IsNull ("ColumnSize") ? -1 : (int) schemaRow ["ColumnSize"];

				// create column = value string
				setClause [count] = String.Format ("{0} = ?", QuoteIdentifier(columnName));
				AddParameter (_updateCommand, columnName, sqlDbType, length, columnName, DataRowVersion.Current);
				count++;
			}

			// create where clause. odbc uses positional parameters. so where class
			// is created seperate from the above loop.
			string whereClause = CreateOptWhereClause (_updateCommand);
			
			query = String.Format ("{0} {1} WHERE ({2})", 
					       query, 
					       String.Join (", ", setClause, 0, count),
					       whereClause);
			_updateCommand.CommandText = query;
			return _updateCommand;
		}

#if NET_2_0
		[MonoTODO]
                public new OdbcCommand GetUpdateCommand (bool option)
		{
			// FIXME: check validity of adapter
			if (_updateCommand != null)
				return _updateCommand;

			if (_schema == null)
				RefreshSchema ();

			if (option == false) {
				return GetUpdateCommand ();
			} else {
				throw new NotImplementedException ();
			}
		}
#endif // NET_2_0
			
		public
#if NET_2_0
                new
#endif // NET_2_0
                OdbcCommand GetDeleteCommand ()
		{
			// FIXME: check validity of adapter
			if (_deleteCommand != null)
				return _deleteCommand;

			if (_schema == null)
				RefreshSchema ();
			
			_deleteCommand = new OdbcCommand ();
			_deleteCommand.Connection = DataAdapter.SelectCommand.Connection;
			_deleteCommand.Transaction = DataAdapter.SelectCommand.Transaction;
			_deleteCommand.CommandType = CommandType.Text;
			_deleteCommand.UpdatedRowSource = UpdateRowSource.None;

			string query = String.Format ("DELETE FROM {0}", QuoteIdentifier (TableName));
			string whereClause = CreateOptWhereClause (_deleteCommand);
			
			query = String.Format ("{0} WHERE ({1})", query, whereClause);
			_deleteCommand.CommandText = query;
			return _deleteCommand;
		}

#if NET_2_0
		[MonoTODO]
                public new OdbcCommand GetDeleteCommand (bool option)
		{
			// FIXME: check validity of adapter
			if (_deleteCommand != null)
				return _deleteCommand;

			if (_schema == null)
				RefreshSchema ();

			if (option == false) {
				return GetDeleteCommand ();
			} else {
				throw new NotImplementedException ();
			}
		}
#endif // NET_2_0

		public
#if NET_2_0
                override
#endif // NET_2_0
                void RefreshSchema ()
		{
			// creates metadata
			if (SelectCommand == null)
				throw new InvalidOperationException ("SelectCommand should be valid");
			if (SelectCommand.Connection == null)
				throw new InvalidOperationException ("SelectCommand's Connection should be valid");
			
			CommandBehavior behavior = CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo;
			if (SelectCommand.Connection.State != ConnectionState.Open) {
				SelectCommand.Connection.Open ();
				behavior |= CommandBehavior.CloseConnection;
			}
			
			OdbcDataReader reader = SelectCommand.ExecuteReader (behavior);
			_schema = reader.GetSchemaTable ();
			reader.Close ();
			
			// force creation of commands
			_insertCommand 	= null;
			_updateCommand 	= null;
			_deleteCommand 	= null;
			_tableName	= String.Empty;
		}
                
#if NET_2_0
                [MonoTODO]
                protected override void ApplyParameterInfo (DbParameter dbParameter,
							    DataRow row,
							    StatementType statementType,
							    bool whereClause)
                {
                        throw new NotImplementedException ();
                }

                [MonoTODO]
                protected override string GetParameterName (int position)
                {
                        throw new NotImplementedException ();                        
                }

                [MonoTODO]
                protected override string GetParameterName (string parameterName)
                {
                        throw new NotImplementedException ();                        
                }
                
                [MonoTODO]
                protected override string GetParameterPlaceholder (int position)
                {
                        throw new NotImplementedException ();                        
                }
                
                [MonoTODO]
                protected override void SetRowUpdatingHandler (DbDataAdapter adapter)
                {
                        throw new NotImplementedException ();
                }

#endif // NET_2_0
		
#if NET_2_0
		[MonoTODO]
		public override
#else
		private
#endif		
		string QuoteIdentifier (string unquotedIdentifier)
		{
#if NET_2_0
			throw new NotImplementedException ();
#else
			if (unquotedIdentifier == null || unquotedIdentifier == String.Empty)
				return unquotedIdentifier;
			return String.Format ("{0}{1}{2}", QuotePrefix, 
					      unquotedIdentifier, QuoteSuffix);
#endif			
		}

#if NET_2_0
		[MonoTODO]
		public override
#else
		private
#endif		
		string UnquoteIdentifier (string quotedIdentifier)
		{
#if NET_2_0
			throw new NotImplementedException ();
#else
			if (quotedIdentifier == null || quotedIdentifier == String.Empty)
				return quotedIdentifier;
			
			StringBuilder sb = new StringBuilder (quotedIdentifier.Length);
			sb.Append (quotedIdentifier);
			if (quotedIdentifier.StartsWith (QuotePrefix))
				sb.Remove (0,QuotePrefix.Length);
			if (quotedIdentifier.EndsWith (QuoteSuffix))
				sb.Remove (sb.Length - QuoteSuffix.Length, QuoteSuffix.Length );
			return sb.ToString ();
#endif			
		}

		private void OnRowUpdating (object sender, OdbcRowUpdatingEventArgs args)
		{
			if (args.Command != null)
				return;
			try {
				switch (args.StatementType) {
				case StatementType.Insert:
					args.Command = GetInsertCommand ();
					break;
				case StatementType.Update:
					args.Command = GetUpdateCommand ();
					break;
				case StatementType.Delete:
					args.Command = GetDeleteCommand ();
					break;
				}
			} catch (Exception e) {
				args.Errors = e;
				args.Status = UpdateStatus.ErrorsOccurred;
			}
		}
		

		#endregion // Methods
	}
}
