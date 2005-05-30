//
// System.Data.ProviderBase.AbstractDbCommand
//
// Author:
//   Boris Kirzner (borisk@mainsoft.com)
//   Konstantin Triger (kostat@mainsoft.com)
//

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Data;
using System.Data.Common;

using java.sql;
using java.io;

#if !USE_DOTNET_REGEXP
using java.util.regex;
#endif

namespace System.Data.ProviderBase
{
	public abstract class AbstractDbCommand : DbCommandBase
	{
		#region ProcedureColumnCache

		internal sealed class ProcedureColumnCache : AbstractDbMetaDataCache
		{
			internal ArrayList GetProcedureColumns(AbstractDBConnection connection, String commandText,AbstractDbCommand command) 
			{
				string connectionCatalog = connection.JdbcConnection.getCatalog();
				string key = String.Concat(connection.ConnectionString, connectionCatalog, commandText);
				System.Collections.Hashtable cache = Cache;

				ArrayList col = cache[key] as ArrayList;

				if (null != col) {
					return col;
				}
	
				col = connection.GetProcedureColumns(commandText,command);
				if (col != null)
					cache[key] = col;
				return col;				
			}
		}

		#endregion

		#region SqlStatementsHelper

		internal sealed class SqlStatementsHelper
		{
			#region Fields
#if USE_DOTNET_REGEXP			
			internal static readonly Regex NamedParameterStoredProcedureRegExp = new Regex(@"^\s*{?\s*((?<RETVAL>@\w+)\s*=\s*)?call\s+(?<PROCNAME>(((\[[^\]]*\])|([^\.\(])*)\s*\.\s*){0,2}(\[[^\]]*\]|((\s*[^\.\(\)\{\}\s])+)))\s*(\(\s*(?<USERPARAM>((""([^""]|(""""))*"")|('([^']|(''))*')|[^,])*)?\s*(,\s*(?<USERPARAM>((""([^""]|(""""))*"")|('([^']|(''))*')|[^,])*)\s*)*\))?\s*}?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			internal static readonly Regex SimpleParameterStoredProcedureRegExp = new Regex(@"^\s*{?\s*((?<RETVAL>\?)\s*=\s*)?call\s+(?<PROCNAME>(((\[[^\]]*\])|([^\.\(])*)\s*\.\s*){0,2}(\[[^\]]*\]|((\s*[^\.\(\)\{\}\s])+)))\s*(\(\s*(?<USERPARAM>((""([^""]|(""""))*"")|('([^']|(''))*')|[^,])*)?\s*(,\s*(?<USERPARAM>((""([^""]|(""""))*"")|('([^']|(''))*')|[^,])*)\s*)*\))?\s*}?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
#else
			internal static readonly Pattern NamedParameterStoredProcedureRegExp = Pattern.compile(@"^\s*\{?\s*(?:(@\w+)\s*=\s*)?call\s+((?:(?:(?:\[[^\]]*\])|(?:[^\.\(\)\{\}\[\]])*)\s*\.\s*){0,2}(?:\[[^\]]*\]|(?:(?:\s*[^\.\(\)\{\}\[\]])+)))\s*(?:\((.*)\))?\s*\}?\s*$", Pattern.CASE_INSENSITIVE);
			internal static readonly Pattern SimpleParameterStoredProcedureRegExp = Pattern.compile(@"^\s*\{?\s*(?:(\?)\s*=\s*)?call\s+((?:(?:(?:\[[^\]]*\])|(?:[^\.\(\)\{\}\[\]])*)\s*\.\s*){0,2}(?:\[[^\]]*\]|(?:(?:\s*[^\.\(\)\{\}\[\]])+)))\s*(?:\((.*)\))?\s*\}?\s*$", Pattern.CASE_INSENSITIVE);
#endif

//			internal static readonly Regex NamedParameterRegExp = new Regex(@"((?<USERPARAM>@\w+)|(\[[^\[\]]*\])|(""([^""]|(""""))*"")|('([^']|(''))*'))*", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
//			internal static readonly Regex SimpleParameterRegExp = new Regex(@"((?<USERPARAM>\?)|(\[[^\[\]]*\])|(""([^""]|(""""))*"")|('([^']|(''))*'))*", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			internal static readonly SimpleRegex NamedParameterRegExp = new SqlParamsRegex();
			internal static readonly SimpleRegex SimpleParameterRegExp = new OleDbParamsRegex();

			internal static readonly Regex SelectFromStatementReqExp = new Regex(@"^\s*SELECT\s+(((\[[^\[\]]*\])|(""([^""]|(""""))*"")|('([^']|(''))*')|[^'""\[])*\s+)*FROM\s+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			internal static readonly Regex ForBrowseStatementReqExp = new Regex(@"\s+FOR\s+BROWSE\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

			internal static readonly SimpleRegex CompoundStatementSplitterReqExp = new CharacterSplitterRegex(';');
			internal static readonly SimpleRegex ProcedureParameterSplitterReqExp = new CharacterSplitterRegex(',');

			#endregion // Fields
		}

		#endregion // SqlStatementsHelper

		#region Fields

		protected DbParameterCollection _parameters;
		protected java.sql.Statement _statement;
		protected AbstractDBConnection _connection;
		protected AbstractTransaction _transaction;
		private bool _isCommandPrepared;
		protected CommandBehavior _behavior;
		private ArrayList _internalParameters;
		string _javaCommandText;
		private int _recordsAffected;
		private ResultSet _currentResultSet;
		private DbDataReader _currentReader;
		private bool _nullParametersInPrepare;
		private bool _hasResultSet;

		internal static ProcedureColumnCache _procedureColumnCache = new ProcedureColumnCache();

		#endregion // Fields

		#region Constructors

		public AbstractDbCommand(
			String cmdText,
			AbstractDBConnection connection,
			AbstractTransaction transaction)
		{
			_connection = connection;
			base.CommandText = cmdText;
			_transaction = transaction;
			_isCommandPrepared = false;
			_recordsAffected = -1;
			if (connection != null) {
				connection.AddReference(this);
			}
		}

		#endregion // Constructors

		#region Properties

		protected override DbParameterCollection DbParameterCollection
		{
			get {
				if (_parameters == null) {
					_parameters = CreateParameterCollection(this);
				}
				return _parameters; 
			}
		}

		protected override DbConnection DbConnection
		{
			get { return (DbConnection)_connection; }
			set {
				if (value == _connection) {
					return;
				}

				if (_currentReader != null && !_currentReader.IsClosed) {
					throw ExceptionHelper.ConnectionIsBusy(this.GetType().Name,((AbstractDBConnection)_connection).InternalState);
				}
				if (_connection != null) {
					_connection.RemoveReference(this);
				}
				_connection = (AbstractDBConnection) value;
				if (_connection != null) {
					_connection.AddReference(this);
				}
			}
		}

		protected override DbTransaction DbTransaction
		{
			get { return _transaction != null ? _transaction.ActiveTransaction : null; }
			set { _transaction = (AbstractTransaction)value; }
		}

		public override string CommandText
		{
			get { return base.CommandText; }
			set { 
				if (CommandText == null || String.Compare(CommandText, value,  true) != 0) {
					base.CommandText = value;
					_isCommandPrepared = false;
				}
			}
		}

		internal CommandBehavior Behavior
		{
			get { return _behavior; }
			set { _behavior = value; }
		}

		protected bool IsCommandPrepared
		{
			get { return _isCommandPrepared; }
			set { _isCommandPrepared = value; }
		}

		protected bool NullParametersInPrepare
		{
			get { return _nullParametersInPrepare; }
			set { _nullParametersInPrepare = value; }
		}

		protected ArrayList InternalParameters
		{
			get {
				if (_internalParameters == null) {
					_internalParameters = new ArrayList();
				}
				return _internalParameters;
			}
		}

		// Number of records affected by execution of batch statement
		// -1 for SELECT statements.
		internal int RecordsAffected
		{
			get {
				return _recordsAffected;
			}
		}

		// AbstractDbCommand acts as IEnumerator over JDBC statement
		// AbstractDbCommand.CurrentResultSet corresponds to IEnumerator.Current
		internal virtual ResultSet CurrentResultSet
		{
			get { 
				try {
					if (_currentResultSet == null && _hasResultSet) {
						_currentResultSet = _statement.getResultSet(); 
					}
					return _currentResultSet;
				}
				catch(SQLException e) {
					throw new Exception(e.Message, e);
				}
			}
		}

		internal java.sql.Statement JdbcStatement
		{
			get { return _statement; }
		}
#if USE_DOTNET_REGEX
		protected virtual Regex StoredProcedureRegExp
#else
		protected virtual Pattern StoredProcedureRegExp
#endif
		{
			get { return SqlStatementsHelper.SimpleParameterStoredProcedureRegExp; }
		}

		protected virtual SimpleRegex ParameterRegExp
		{
			get { return SqlStatementsHelper.SimpleParameterRegExp; }
		}

		#endregion // Properties

		#region Methods

		protected abstract DbParameter CreateParameterInternal();

		protected abstract void CheckParameters();

		protected abstract DbDataReader CreateReader();

		protected abstract DbParameterCollection CreateParameterCollection(AbstractDbCommand parent);

		protected abstract SystemException CreateException(SQLException e);

		protected internal void CopyTo(AbstractDbCommand target)
		{
			target._behavior = _behavior;
			target.CommandText = CommandText;
			target.CommandTimeout = CommandTimeout;
			target.CommandType = CommandType;
			target._connection = _connection;
			target._transaction = _transaction;
			target.UpdatedRowSource = UpdatedRowSource;

			if (Parameters != null && Parameters.Count > 0) {
				target._parameters = CreateParameterCollection(target);
				for(int i=0 ; i < Parameters.Count; i++) {
					target.Parameters.Add(((AbstractDbParameter)Parameters[i]).Clone());
				}
			}
		}

		public override void Cancel()
		{
			try {
				if (_statement != null)
					_statement.cancel();
			}
			catch {
				// MSDN says : "If there is nothing to cancel, nothing happens. 
				// However, if there is a command in process, and the attempt to cancel fails, 
				// no exception is generated."
			}
		}
		
		protected virtual bool SkipParameter(DbParameter parameter)
		{
			return false;
		}

		protected override DbParameter CreateDbParameter()
		{
			return CreateParameterInternal();
		}

		internal void DeriveParameters ()
		{
			if(CommandType != CommandType.StoredProcedure) {
				throw ExceptionHelper.DeriveParametersNotSupported(this.GetType(),CommandType);
			}

			ArrayList parameters = DeriveParameters(CommandText, true);
			Parameters.Clear();
			foreach (AbstractDbParameter param in parameters) {
				Parameters.Add(param.Clone());
			}
		}

		protected ArrayList DeriveParameters(string procedureName, bool throwIfNotExist)
		{
			try {
				ArrayList col = _procedureColumnCache.GetProcedureColumns((AbstractDBConnection)Connection, procedureName, this);
				if (col == null) {
					if (throwIfNotExist)
						throw ExceptionHelper.NoStoredProcedureExists(procedureName);
					col = new ArrayList();
				}

				return col;
			}
			catch(SQLException e) {
				throw CreateException(e);
			}
		}

		string CreateTableDirectCommandText(string tableNames) {
			string forBrowse = String.Empty;
			if ((Behavior & CommandBehavior.KeyInfo) != 0) {
				AbstractDBConnection connection = (AbstractDBConnection)Connection;
				if (connection != null) {
					string dbname = connection.JdbcConnection.getMetaData().getDatabaseProductName();
					if (dbname == "Microsoft SQL Server")	//must add "FOR BROWSE" for selects
						forBrowse = " FOR BROWSE";
				}
			}

			string[] names = tableNames.Split(',');
			StringBuilder sb = new StringBuilder();

			for(int i = 0; i < names.Length; i++) {
				sb.Append("SELECT * FROM ");
				sb.Append(names[i]);
				sb.Append(forBrowse);
				sb.Append(';');
			}
				
			if(names.Length <= 1) {
				sb.Remove(sb.Length - 1,1);
			}
			return sb.ToString();
		}

		private string PrepareCommandTextAndParameters()
		{
			NullParametersInPrepare = false;
			switch (CommandType) {
				case CommandType.TableDirect :
					return CreateTableDirectCommandText(CommandText);
				case CommandType.StoredProcedure :
					return CreateStoredProcedureCommandTextSimple(CommandText, Parameters, DeriveParameters(CommandText, false));
				case CommandType.Text :

					int userParametersPosition = 0;
					int charsConsumed = 0;
					StringBuilder sb = new StringBuilder(CommandText.Length);

					for (SimpleMatch match = SqlStatementsHelper.CompoundStatementSplitterReqExp.Match(CommandText);
						match.Success;
						match = match.NextMatch()) {

						int length = match.Length;

						if (length == 0)
							continue;

						int start = match.Index;
						string value = match.Value;

						sb.Append(CommandText, charsConsumed, start-charsConsumed);
						charsConsumed = start + length;

#if USE_DOTNET_REGEX
						Match storedProcMatch = StoredProcedureRegExp.Match(value);
						// count parameters for all kinds of simple statements 
						userParametersPosition +=
							(storedProcMatch.Success) ?
							// statement is stored procedure call
							CreateStoredProcedureCommandText(sb, value, storedProcMatch, Parameters, userParametersPosition) :
							// statement is a simple SQL query				
							PrepareSimpleQuery(sb, value, Parameters, userParametersPosition);	
#else
						Matcher storedProcMatch = StoredProcedureRegExp.matcher((java.lang.CharSequence)(object)value);
						userParametersPosition +=
							(storedProcMatch.find()) ?
							// statement is stored procedure call
							CreateStoredProcedureCommandText(sb, value, storedProcMatch, Parameters, userParametersPosition) :
							// statement is a simple SQL query				
							PrepareSimpleQuery(sb, value, Parameters, userParametersPosition);
#endif
					}

					sb.Append(CommandText, charsConsumed, CommandText.Length-charsConsumed);

					return sb.ToString();
			}
			return null;
		}

		string CreateStoredProcedureCommandTextSimple(string procedureName, IDataParameterCollection userParams, IList derivedParams) {
			StringBuilder sb = new StringBuilder();

			int curUserPos = 0;
			int curDerivedPos = 0;
			bool addParas = true;
			string trimedProcedureName = (procedureName != null) ? procedureName.TrimEnd() : String.Empty;
			if (trimedProcedureName.Length > 0 && trimedProcedureName[trimedProcedureName.Length-1] == ')')
				addParas = false;
			
			if (derivedParams.Count > 0 && ((AbstractDbParameter)derivedParams[curDerivedPos]).Direction == ParameterDirection.ReturnValue) {
				AbstractDbParameter derivedParam = (AbstractDbParameter)derivedParams[curDerivedPos++];

				AbstractDbParameter userParameter = GetUserParameter(derivedParam.Placeholder, userParams, curUserPos);
				if (userParameter != null && userParameter.Direction == ParameterDirection.ReturnValue) {
					curUserPos++;
					InternalParameters.Add(userParameter);
					sb.Append("{? = call ");

					if (derivedParam != null && !userParameter.IsDbTypeSet) {
						userParameter.JdbcType = derivedParam.JdbcType;
					}
				}
				else {
					sb.Append("{call ");
				}
			}
			else {
				if (userParams.Count > 0 && 
					((AbstractDbParameter)userParams[0]).Direction == ParameterDirection.ReturnValue) {
					curUserPos++;
					InternalParameters.Add(userParams[0]);
					sb.Append("{? = call ");
				}
				else
					sb.Append("{call ");
			}

			sb.Append(procedureName);
			if (addParas)
				sb.Append('(');

			bool needComma = false;
			for (int i = curDerivedPos; i < derivedParams.Count; i++) {
				AbstractDbParameter derivedParameter = (AbstractDbParameter)derivedParams[curDerivedPos++];
				
				bool addParam = false;

				if (derivedParameter.IsSpecial) {
					// derived parameter is special - never appears in user parameters or user values
					InternalParameters.Add((AbstractDbParameter)derivedParameter.Clone());
					addParam = true;
				}
				else {
					AbstractDbParameter userParameter = GetUserParameter(derivedParameter.Placeholder, userParams, curUserPos);
					if (userParameter != null) {
						curUserPos++;
						InternalParameters.Add(userParameter);
						addParam = true;

						if (derivedParameter != null && !userParameter.IsDbTypeSet) {
							userParameter.JdbcType = derivedParameter.JdbcType;
						}
					}
				}

				if (addParam) {
					if (needComma)
						sb.Append(',');
					else
						needComma = true;

					sb.Append('?');
				}
			}

			for (int i = curUserPos; i < userParams.Count; i++) {
				if (needComma)
					sb.Append(',');
				else
					needComma = true;

				AbstractDbParameter userParameter = (AbstractDbParameter)userParams[curUserPos++];
				InternalParameters.Add(userParameter);

				sb.Append('?');
			}

			if (addParas)
				sb.Append(')');
			sb.Append('}');
			return sb.ToString();
		}

		/// <summary>
		/// We suppose that user parameters are in the same order as devived parameters except the special cases
		/// (return value, oracle ref cursors etc.)
		/// </summary>
		//protected virtual string CreateStoredProcedureCommandText(string procedureName, IList userParametersList, int userParametersListStart/*, int userParametersListCount*/, string[] userValuesList, ArrayList derivedParametersList)
#if USE_DOTNET_REGEX
		int CreateStoredProcedureCommandText(StringBuilder sb, string sql, Match match, IDataParameterCollection userParams, int userParamsStartPosition)
#else
		int CreateStoredProcedureCommandText(StringBuilder sb, string sql, Matcher match, IDataParameterCollection userParams, int userParamsStartPosition)
#endif
		{
			int curUserPos = userParamsStartPosition;
#if USE_DOTNET_REGEX
			Group procNameGroup = null;

			for (Match procNameMatch = match; procNameMatch.Success; procNameMatch = procNameMatch.NextMatch()){
				procNameGroup = match.Groups["PROCNAME"];
				if (!procNameGroup.Success) {
					continue;
				}
			}

			if (procNameGroup == null || !procNameGroup.Success)
				throw new ArgumentException("Not a stored procedure call: '{0}'", sql);

			ArrayList derivedParameters = DeriveParameters(procNameGroup.Value, false);
#else
			ArrayList derivedParameters = DeriveParameters(match.group(2).Trim(), false);
#endif
			int curDerivedPos = 0;

			AbstractDbParameter retValderivedParameter = curDerivedPos < derivedParameters.Count ?
				(AbstractDbParameter)derivedParameters[curDerivedPos] : null;
			if (retValderivedParameter != null && retValderivedParameter.Direction == ParameterDirection.ReturnValue)
				curDerivedPos++;

			int queryCurrentPosition = 0;
			
#if USE_DOTNET_REGEX
			for (Match retValMatch = match; retValMatch.Success; retValMatch = retValMatch.NextMatch()){
				Group retval = retValMatch.Groups["RETVAL"];
				if (!retval.Success) {
					continue;
				}

				int retvalIndex = retval.Index;
				string retvalValue = retval.Value;
				int retvalLength = retval.Length;
#else
			int retvalIndex = match.start(1);
			for (;retvalIndex >= 0;) {
				string retvalValue = match.group(1);
				int retvalLength = retvalValue.Length;
#endif

				sb.Append(sql, queryCurrentPosition, retvalIndex);
				AbstractDbParameter userParameter = GetUserParameter(retvalValue, userParams, curUserPos);
				if (userParameter != null) {
					sb.Append('?');
					InternalParameters.Add(userParameter);

					if (retValderivedParameter != null && !userParameter.IsDbTypeSet) {
						userParameter.JdbcType = retValderivedParameter.JdbcType;
					}

					curUserPos++;
				}
				else {
					sb.Append(retvalValue);
				}

				queryCurrentPosition = (retvalIndex + retvalLength);

				break;
			}

#if USE_DOTNET_REGEX
			sb.Append(sql, queryCurrentPosition, procNameGroup.Index + procNameGroup.Length - queryCurrentPosition);
			queryCurrentPosition = procNameGroup.Index + procNameGroup.Length;
#else
			sb.Append(sql, queryCurrentPosition, match.end(2) - queryCurrentPosition);
			queryCurrentPosition = match.end(2);
#endif

			bool hasUserParams = false;

#if USE_DOTNET_REGEX
			must rewrite the regex to not parse params to have single code with java regex
#else
			int paramsStart = match.start(3);
			if (paramsStart >= 0) {
#endif

				hasUserParams = true;
				sb.Append(sql,queryCurrentPosition,paramsStart - queryCurrentPosition);
				queryCurrentPosition = paramsStart;

				for (SimpleMatch m = SqlStatementsHelper.ProcedureParameterSplitterReqExp.Match(match.group(3));
					m.Success;m = m.NextMatch()) {

					SimpleCapture parameterCapture = m;
					sb.Append(sql,queryCurrentPosition,paramsStart + parameterCapture.Index - queryCurrentPosition);

					// advance in query
					queryCurrentPosition = paramsStart + parameterCapture.Index + parameterCapture.Length;

					AbstractDbParameter derivedParameter = curDerivedPos < derivedParameters.Count ?
						(AbstractDbParameter)derivedParameters[curDerivedPos++] : null;
					
					//check for special params
					while (derivedParameter != null && derivedParameter.IsSpecial) {
						// derived parameter is special - never appears in user parameters or user values
						InternalParameters.Add((AbstractDbParameter)derivedParameter.Clone());
						sb.Append('?');
						sb.Append(',');

						derivedParameter = curDerivedPos < derivedParameters.Count ?
							(AbstractDbParameter)derivedParameters[curDerivedPos++] : null;
					}

					AbstractDbParameter userParameter = GetUserParameter(parameterCapture.Value.Trim(), userParams, curUserPos);

					if (userParameter != null) {
						sb.Append('?');
						InternalParameters.Add(userParameter);
						if (derivedParameter != null && !userParameter.IsDbTypeSet) {
							userParameter.JdbcType = derivedParameter.JdbcType;
						}
						// advance in user parameters
						curUserPos++;				
					}
					else {
						sb.Append(parameterCapture.Value);
					}									
				}					
			}

			bool addedSpecialParams = false;

			for (int i = curDerivedPos; i < derivedParameters.Count;) {
				AbstractDbParameter derivedParameter = (AbstractDbParameter)derivedParameters[i++];
				if (derivedParameter.IsSpecial) {
					// derived parameter is special - never appears in user parameters or user values
					if (!hasUserParams && !addedSpecialParams) {
						addedSpecialParams = true;
						curDerivedPos++;
						sb.Append('(');
					}

					for (;curDerivedPos < i;curDerivedPos++)
						sb.Append(',');

					InternalParameters.Add((AbstractDbParameter)derivedParameter.Clone());
					sb.Append('?');
				}
			}

			if (!hasUserParams && addedSpecialParams)
				sb.Append(')');

			sb.Append(sql,queryCurrentPosition,sql.Length - queryCurrentPosition);
			return curUserPos - userParamsStartPosition;
		}

		protected virtual AbstractDbParameter GetUserParameter(string parameterName, IList userParametersList, int userParametersListPosition)
		{
			if (userParametersListPosition < userParametersList.Count) {
				AbstractDbParameter param = (AbstractDbParameter)userParametersList[userParametersListPosition];
				if (param.Placeholder == parameterName)
					return param;
			}
			return null;
		}

		int PrepareSimpleQuery(StringBuilder sb, string query, IList userParametersList, int userParametersListStart)
		{
			int queryCurrentPosition = 0;
			int userParametersListPosition = userParametersListStart;

			if (userParametersList.Count > 0) {
				for (SimpleMatch m = ParameterRegExp.Match(query);
					m.Success;m = m.NextMatch()) {

					SimpleCapture parameterCapture = m;
					sb.Append(query,queryCurrentPosition,parameterCapture.Index - queryCurrentPosition);

					// advance in query
					queryCurrentPosition = parameterCapture.Index + parameterCapture.Length;	

					AbstractDbParameter userParameter = GetUserParameter(parameterCapture.Value, userParametersList, userParametersListPosition);

					if (userParameter != null) {
						if (IsNullParameter(userParameter)) {
							sb.Append("null");
							NullParametersInPrepare = true;
						}
						else {
							sb.Append('?');
							InternalParameters.Add(userParameter);	
						}	
						// advance in user parameters
						userParametersListPosition++;				
					}
					else {
						sb.Append(parameterCapture.Value);
					}
				}
			}

			sb.Append(query,queryCurrentPosition,query.Length - queryCurrentPosition);
			int userParamsConsumed = userParametersListPosition - userParametersListStart;

			if ((Behavior & CommandBehavior.KeyInfo) == 0)
				return userParamsConsumed;

			AbstractDBConnection connection = (AbstractDBConnection)Connection;
			if (connection == null)
				return userParamsConsumed;

			string dbname = connection.JdbcConnection.getMetaData().getDatabaseProductName();
			if (dbname == "Microsoft SQL Server") {	//must add "FOR BROWSE" for selects
				if (SqlStatementsHelper.SelectFromStatementReqExp.IsMatch(query))
					if (!SqlStatementsHelper.ForBrowseStatementReqExp.IsMatch(query))
						sb.Append(" FOR BROWSE");
			}

			return userParamsConsumed;
		}

		protected virtual bool IsNullParameter(AbstractDbParameter parameter)
		{
			return ((parameter.Value == null || parameter.Value == DBNull.Value) && !parameter.IsDbTypeSet);
		}

		protected virtual void PrepareInternalParameters()
		{
			InternalParameters.Clear();
		}
        
		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
		{
			AbstractDBConnection connection = (AbstractDBConnection)Connection;
			if (connection == null) {
				throw ExceptionHelper.ConnectionNotInitialized("ExecuteReader");
			}

			IDbTransaction transaction = Transaction;
			if ((transaction != null && transaction.Connection != connection) ||
				(transaction == null && !connection.JdbcConnection.getAutoCommit())) {
				throw ExceptionHelper.TransactionNotInitialized();
			}

			connection.IsExecuting = true;

			try {
				Behavior = behavior;

				PrepareInternalParameters();			
				PrepareInternal(false);

				// For SchemaOnly there is no need for statement execution
				if (Behavior != CommandBehavior.SchemaOnly) {
					_recordsAffected = -1;

					if ((Behavior & CommandBehavior.SingleRow) != 0) {
						_statement.setMaxRows(1);
					}
				
					if(_statement is PreparedStatement) {
						BindParameters(InternalParameters);
						_hasResultSet = ((PreparedStatement)_statement).execute();
					}
					else {
						_hasResultSet =_statement.execute(_javaCommandText);					
					}
		
					if (!_hasResultSet) {
						int updateCount = _statement.getUpdateCount();
						if (updateCount >= 0) {
							AccumulateRecordsAffected(updateCount);
							_hasResultSet = true; //play as if we have resultset
							NextResultSet();
						}
					}					
				}
				connection.IsFetching = true;
				try {
					_currentReader = CreateReader();
				}
				catch(Exception e) {
					connection.IsFetching = false;
					throw e;
				}
				return _currentReader;
			}
			catch(SQLException e) {				
				throw CreateException(e);
			}
			finally {
				connection.IsExecuting = false;
				NullParametersInPrepare = false;
			}
		}

		public override void Prepare()
		{
			((AbstractDBConnection)Connection).IsExecuting = true;
			try {
				CheckParameters();
				PrepareInternal(true);	
			}
			finally {
				((AbstractDBConnection)Connection).IsExecuting = false;
			}
		}

		private void PrepareInternal(bool isExplicit)
		{
			if ((Connection == null) || (Connection.State != ConnectionState.Open)) {
				throw ExceptionHelper.ConnectionNotOpened("Prepare",(Connection != null) ? Connection.State.ToString() : "");
			}

			if (IsCommandPrepared) {
				// maybe we have to prepare the command again
				bool hasNullParameters = false;
				for(int i = 0; (i < Parameters.Count) && !hasNullParameters; i++) {
					AbstractDbParameter parameter = (AbstractDbParameter)Parameters[i];
					if (IsNullParameter(parameter)) {
						// if we still have null parameters - have to prepare agail
						IsCommandPrepared = false;
						hasNullParameters = true;
					}
				}

				if (!NullParametersInPrepare && hasNullParameters) {
					// if we prepeared using null parameters and now there is no null parameters - need to prepare again
					IsCommandPrepared = false;
				}
			}

			if (!IsCommandPrepared) {

				_javaCommandText = PrepareCommandTextAndParameters();

				java.sql.Connection jdbcCon = _connection.JdbcConnection;

				// For SchemaOnly we just prepare statement (for future use in GetSchemaTable)
				if (Behavior == CommandBehavior.SchemaOnly) {
					if (CommandType == CommandType.StoredProcedure)
						_statement = jdbcCon.prepareCall(_javaCommandText);
					else
						_statement = jdbcCon.prepareStatement(_javaCommandText);	
					return;
				}

				if (CommandType == CommandType.StoredProcedure)
					_statement = jdbcCon.prepareCall(_javaCommandText);
				else {
					int internalParametersCount = InternalParameters.Count;
					if ( internalParametersCount > 0) {
						bool hasOnlyInputParameters = true;
						for(int i=0; i < internalParametersCount; i++) {
							AbstractDbParameter internalParameter = (AbstractDbParameter)InternalParameters[i];
							if (IsNullParameter(internalParameter)) {
								NullParametersInPrepare = true;
							}

							if ((internalParameter.Direction & ParameterDirection.Output) != 0){
								hasOnlyInputParameters = false;
							}
						}

						if (hasOnlyInputParameters) {
							_statement = jdbcCon.prepareStatement(_javaCommandText);	
						}
						else {						
							_statement = jdbcCon.prepareCall(_javaCommandText);
						}
					}
					else {
						if (isExplicit) {
							_statement = jdbcCon.prepareStatement(_javaCommandText);				
						}
						else {
							_statement = jdbcCon.createStatement();					
						}
					}
				}
				IsCommandPrepared = true;
			}
		}

		protected void BindParameters(ArrayList parameters)
		{
			for(int parameterIndex = 0; parameterIndex < parameters.Count; parameterIndex++) {
				AbstractDbParameter parameter = (AbstractDbParameter)parameters[parameterIndex];
				switch (parameter.Direction) {
					case ParameterDirection.Input :
						BindInputParameter(parameter,parameterIndex);
						break;
					case ParameterDirection.InputOutput:
						BindInputParameter(parameter,parameterIndex);
						BindOutputParameter(parameter,parameterIndex);
						break;
					case ParameterDirection.Output :
						BindOutputParameter(parameter,parameterIndex);
						break;
					case ParameterDirection.ReturnValue :
						BindOutputParameter(parameter,parameterIndex);
						break;
				}
			}
		}
		
		protected virtual void BindInputParameter(AbstractDbParameter parameter, int parameterIndex)
		{
			object value = parameter.ConvertedValue;			
			// java parameters are 1 based, while .net are 0 based
			parameterIndex++; 
			PreparedStatement preparedStatement = ((PreparedStatement)_statement);

			switch (parameter.JdbcType) {
				case DbTypes.JavaSqlTypes.DATALINK:
				case DbTypes.JavaSqlTypes.DISTINCT:
				case DbTypes.JavaSqlTypes.JAVA_OBJECT:
				case DbTypes.JavaSqlTypes.OTHER:
				case DbTypes.JavaSqlTypes.REF:
				case DbTypes.JavaSqlTypes.STRUCT: {
					preparedStatement.setObject(parameterIndex, value, (int)parameter.JdbcType);
					return;
				}
			}

			if ((value is DBNull) || (value == null)) {
				preparedStatement.setNull(parameterIndex, (int)((AbstractDbParameter)parameter).JdbcType);
			}
			else if (value is long) {
				preparedStatement.setLong(parameterIndex, (long)value);
			}
			else if (value is byte[]) {
				if (((byte[])value).Length <= 4000) {
					preparedStatement.setBytes(parameterIndex, vmw.common.TypeUtils.ToSByteArray((byte[]) value));
				}
				else {
					InputStream iStream=new ByteArrayInputStream(vmw.common.TypeUtils.ToSByteArray((byte[]) value));
					preparedStatement.setBinaryStream(parameterIndex,iStream,((byte[])value).Length);
				}
			}
			else if (value is byte) {
				preparedStatement.setByte(parameterIndex, (sbyte)(byte)value);
			}
			else if (value is char[]) {
				Reader reader = new CharArrayReader((char[])value);
				preparedStatement.setCharacterStream(parameterIndex,reader,((char[])value).Length);
			}
			else if (value is bool) {
				preparedStatement.setBoolean(parameterIndex, (bool) value);
			}
			else if (value is char) {
				preparedStatement.setString(parameterIndex, ((char)value).ToString());
			}
			else if (value is DateTime) {
				switch (parameter.JdbcType) {
					default:
					case DbTypes.JavaSqlTypes.TIMESTAMP:
						preparedStatement.setTimestamp(parameterIndex,DbConvert.ClrTicksToJavaTimestamp(((DateTime)value).Ticks));
						break;
					case DbTypes.JavaSqlTypes.TIME:
						preparedStatement.setTime(parameterIndex,DbConvert.ClrTicksToJavaTime(((DateTime)value).Ticks));
						break;
					case DbTypes.JavaSqlTypes.DATE:
						preparedStatement.setDate(parameterIndex,DbConvert.ClrTicksToJavaDate(((DateTime)value).Ticks));
						break;
				}
			}
			else if (value is TimeSpan) {
				if (parameter.JdbcType == DbTypes.JavaSqlTypes.TIMESTAMP)
					preparedStatement.setTimestamp(parameterIndex,DbConvert.ClrTicksToJavaTimestamp(((TimeSpan)value).Ticks));
				else
					preparedStatement.setTime(parameterIndex,DbConvert.ClrTicksToJavaTime(((TimeSpan)value).Ticks));
			}
			else if (value is Decimal) {
				preparedStatement.setBigDecimal(parameterIndex, vmw.common.PrimitiveTypeUtils.DecimalToBigDecimal((Decimal) value));
			}
			else if (value is double) {
				preparedStatement.setDouble(parameterIndex, (double)value);
			}
			else if (value is float) {
				preparedStatement.setFloat(parameterIndex, (float)value);
			}
			else if (value is int) {
				preparedStatement.setInt(parameterIndex, (int)value);
			}
			else if (value is string) {
				preparedStatement.setString(parameterIndex, (string)value);
			}
			else if (value is Guid) {
				preparedStatement.setString(parameterIndex, value.ToString());
			}
			else if (value is short) {
				preparedStatement.setShort(parameterIndex, (short)value);
			}
			else if (value is sbyte) {
				preparedStatement.setByte(parameterIndex, (sbyte)value);
			}
			else {
				preparedStatement.setObject(parameterIndex, value);
			}
		}

		protected virtual void BindOutputParameter(AbstractDbParameter parameter, int parameterIndex)
		{
			parameter.Validate();
			int jdbcType = (int)parameter.JdbcType;		
			// java parameters are 1 based, while .net are 0 based
			parameterIndex++;

			CallableStatement callableStatement = ((CallableStatement)_statement);

			// the scale has a meening only in DECIMAL and NUMERIC parameters
			if (jdbcType == Types.DECIMAL || jdbcType == Types.NUMERIC) {
				if(parameter.DbType == DbType.Currency) {
					callableStatement.registerOutParameter(parameterIndex, jdbcType, 4);
				}
				else {
					callableStatement.registerOutParameter(parameterIndex, jdbcType, parameter.Scale);
				}
			}
			else {
				callableStatement.registerOutParameter(parameterIndex, jdbcType);
			}
		}

		private void FillOutputParameters()
		{	
			if  (!(_statement is CallableStatement)) {
				return;
			}
			for(int i = 0; i < InternalParameters.Count; i++) {
				AbstractDbParameter parameter = (AbstractDbParameter)InternalParameters[i];
				ParameterDirection direction = parameter.Direction;
				if (((direction & ParameterDirection.Output) != 0) && !SkipParameter(parameter)) {					
					FillOutputParameter(parameter, i);
				}
				// drop jdbc type of out parameter, since it possibly was updated in ExecuteReader
				parameter.IsJdbcTypeSet = false;
			}
		}

		protected virtual void FillOutputParameter(DbParameter parameter, int index)
		{			
			CallableStatement callableStatement = (CallableStatement)_statement;
			ParameterMetadataWrapper parameterMetadataWrapper = null; 
			// FIXME wait for other drivers to implement
//			try {
//				parameterMetadataWrapper = new ParameterMetadataWrapper(callableStatement.getParameterMetaData());
//			}
//			catch {
//				// suppress error : ms driver for sql server does not implement getParameterMetaData
//				// suppress exception : ms driver for sql server does not implement getParameterMetaData
//			}
			DbTypes.JavaSqlTypes javaSqlType = (DbTypes.JavaSqlTypes)((AbstractDbParameter)parameter).JdbcType;
			try {
				parameter.Value = DbConvert.JavaResultSetToClrWrapper(callableStatement,index,javaSqlType,parameter.Size,parameterMetadataWrapper);
			}
			catch(java.sql.SQLException e) {
				throw CreateException(e);
			}
		}

		// AbstractDbCommand acts as IEnumerator over JDBC statement
		// AbstractDbCommand.NextResultSet corresponds to IEnumerator.MoveNext
		internal virtual bool NextResultSet()
		{
			if (!_hasResultSet)
				return false;

			try {
				for(;;) {
					_hasResultSet = _statement.getMoreResults();
					if (_hasResultSet)
						return true;
					int updateCount = _statement.getUpdateCount();
					if (updateCount < 0)
						return false;

					AccumulateRecordsAffected(updateCount);	
				}
			}
			catch (SQLException e) {
				throw CreateException(e);
			}
			finally {
				_currentResultSet = null;
			}
		}

		private void AccumulateRecordsAffected(int updateCount)
		{ 
			if (_recordsAffected < 0) {
				_recordsAffected = updateCount;
			}
			else {
				_recordsAffected += updateCount;
			}
		}

		internal void OnReaderClosed(object reader)
		{
			CloseInternal();
			if (Connection != null) {
				((AbstractDBConnection)Connection).RemoveReference(reader);
				((AbstractDBConnection)Connection).IsFetching = false;
				if ((Behavior & CommandBehavior.CloseConnection) != 0) {
					Connection.Close();
				}
			}			
		}

		internal void CloseInternal()
		{
			if (Behavior != CommandBehavior.SchemaOnly) {
				if (_statement != null) {
					while (NextResultSet()) {
					}							
					FillOutputParameters();				
				}
			}
			_currentReader = null;
			CleanUp();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				CleanUp();
			}
			base.Dispose(disposing);
		}

		private void CleanUp()
		{
			if (_currentReader != null) {
				// we must preserve statement object until we have an associated reader object that might access it.
				return;
			}
			if (Connection != null) {
				((AbstractDBConnection)Connection).RemoveReference(this);
			}
			if (_statement != null) {
				_statement.close();
				_statement = null;
			}				
			IsCommandPrepared = false;
			_internalParameters = null;
			_currentResultSet = null;
		}

		internal void OnSchemaChanging()
		{
		}

		#endregion // Methods
	}
}
