//
// System.Web.UI.WebControls.SqlDataSourceView
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2003 Ben Maurer
// (C) Novell, Inc. (http://www.novell.com)
//

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

#if NET_2_0
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Data;
using System.ComponentModel;
using System.Data.Common;

namespace System.Web.UI.WebControls {
	public class SqlDataSourceView : DataSourceView, IStateManager {

		HttpContext context;
		DbProviderFactory factory;
		DbConnection connection;

		public SqlDataSourceView (SqlDataSource owner, string name, HttpContext context)
			: base (owner, name)
		{
			this.owner = owner;
			this.name = name;
			this.context = context;
		}

		void InitConnection ()
		{
			if (factory == null) factory = owner.GetDbProviderFactoryInternal ();
			if (connection == null) {
				connection = factory.CreateConnection ();
				connection.ConnectionString = owner.ConnectionString;
			}
		}

		public int Delete (IDictionary keys, IDictionary oldValues)
		{
			return ExecuteDelete (keys, oldValues);
		}
		
		protected override int ExecuteDelete (IDictionary keys, IDictionary oldValues)
		{
			if (!CanDelete)
				throw new NotSupportedException("Delete operation is not supported");
			if (oldValues == null && ConflictDetection == ConflictOptions.CompareAllValues)
				throw new InvalidOperationException ("oldValues parameters should be specified when ConflictOptions is set to CompareAllValues");

			InitConnection ();

			DbCommand command = factory.CreateCommand ();
			command.CommandText = DeleteCommand;
			command.Connection = connection;
			if (DeleteCommandType == SqlDataSourceCommandType.Text)
				command.CommandType = CommandType.Text;
			else
				command.CommandType = CommandType.StoredProcedure;

			IDictionary oldDataValues;
			if (ConflictDetection == ConflictOptions.CompareAllValues) {
				oldDataValues = new Hashtable ();
				if (keys != null) {
					foreach (DictionaryEntry de in keys)
						oldDataValues [de.Key] = de.Value;
				}
				if (oldValues != null) {
					foreach (DictionaryEntry de in oldValues)
						oldDataValues [de.Key] = de.Value;
				}
			}
			else
				oldDataValues = keys;
			
			InitializeParameters (command, DeleteParameters, null, oldDataValues, false);

			SqlDataSourceCommandEventArgs args = new SqlDataSourceCommandEventArgs (command);
			OnDeleting (args);
			if (args.Cancel)
				return -1; 

			bool closed = connection.State == ConnectionState.Closed;

			if (closed)
				connection.Open();
			Exception exception = null; 
			int result = -1;;
			try {
				result = command.ExecuteNonQuery();
			} catch (Exception e) {
				exception = e;
			}

			if (closed)
				connection.Close ();

			OnDataSourceViewChanged (EventArgs.Empty);

			SqlDataSourceStatusEventArgs deletedArgs =
				new SqlDataSourceStatusEventArgs (command, result, exception);
			OnDeleted (deletedArgs);

			if (exception != null && !deletedArgs.ExceptionHandled)
				throw exception;

			return result;
		}
		
		public int Insert (IDictionary values)
		{
			return ExecuteInsert (values);
		}

		protected override int ExecuteInsert (IDictionary values)
		{
			if (!CanInsert)
				throw new NotSupportedException ("Insert operation is not supported");

			InitConnection ();

			DbCommand command = factory.CreateCommand ();
			command.CommandText = InsertCommand;
			command.Connection = connection;
			if (InsertCommandType == SqlDataSourceCommandType.Text)
				command.CommandType = CommandType.Text;
			else
				command.CommandType = CommandType.StoredProcedure;

			InitializeParameters (command, InsertParameters, values, null, true);

			SqlDataSourceCommandEventArgs args = new SqlDataSourceCommandEventArgs (command);
			OnInserting (args);
			if (args.Cancel)
				return -1;

			bool closed = connection.State == ConnectionState.Closed;
			if (closed)
				connection.Open ();
			Exception exception = null;
			int result = -1;
			try {
				result = command.ExecuteNonQuery ();
			}
			catch (Exception e) {
				exception = e;
			}

			if (closed)
				connection.Close ();

			OnDataSourceViewChanged (EventArgs.Empty);

			OnInserted (new SqlDataSourceStatusEventArgs (command, result, exception));

			if (exception != null)
				throw exception;
			return result;
		}
				
		public IEnumerable Select (DataSourceSelectArguments arguments)
		{
			return ExecuteSelect (arguments);
		}

		protected internal override IEnumerable ExecuteSelect (DataSourceSelectArguments arguments)
		{
			if (SortParameterName.Length > 0 && SelectCommandType == SqlDataSourceCommandType.Text)
				throw new NotSupportedException ("The SortParameterName property is only supported with stored procedure commands in SqlDataSource");

			if (arguments.SortExpression.Length > 0 && owner.DataSourceMode == SqlDataSourceMode.DataReader)
				throw new NotSupportedException ("SqlDataSource cannot sort. Set DataSourceMode to DataSet to enable sorting.");

			if (arguments.StartRowIndex > 0 || arguments.MaximumRows > 0)
				throw new NotSupportedException ("SqlDataSource does not have paging enabled. Set the DataSourceMode to DataSet to enable paging.");

			if (FilterExpression.Length > 0 && owner.DataSourceMode == SqlDataSourceMode.DataReader)
				throw new NotSupportedException ("SqlDataSource only supports filtering when the data source's DataSourceMode is set to DataSet.");

			InitConnection ();

			DbCommand command = factory.CreateCommand ();
			command.CommandText = SelectCommand;
			command.Connection = connection;
			if (SelectCommandType == SqlDataSourceCommandType.Text)
				command.CommandType = CommandType.Text;
			else {
				command.CommandType = CommandType.StoredProcedure;
				if (SortParameterName.Length > 0 && arguments.SortExpression.Length > 0)
					command.Parameters.Add (CreateDbParameter (SortParameterName, arguments.SortExpression));
			}

			if (SelectParameters.Count > 0)
				InitializeParameters (command, SelectParameters, null, null, true);

			Exception exception = null;
			if (owner.DataSourceMode == SqlDataSourceMode.DataSet) {
				DataView dataView = null;

				if (owner.EnableCaching)
					dataView = (DataView) owner.Cache.GetCachedObject (SelectCommand, SelectParameters);

				if (dataView == null) {
					SqlDataSourceSelectingEventArgs selectingArgs = new SqlDataSourceSelectingEventArgs (command, arguments);
					OnSelecting (selectingArgs);
					if (selectingArgs.Cancel) {
						return null;
					}
					try {
						DbDataAdapter adapter = factory.CreateDataAdapter ();
						DataSet dataset = new DataSet ();

						adapter.SelectCommand = command;
						adapter.Fill (dataset, name);

						dataView = dataset.Tables [0].DefaultView;
						if (dataView == null)
							throw new InvalidOperationException ();
					}
					catch (Exception e) {
						exception = e;
					}
					int rowsAffected = (dataView == null) ? 0 : dataView.Count;
					SqlDataSourceStatusEventArgs selectedArgs = new SqlDataSourceStatusEventArgs (command, rowsAffected, exception);
					OnSelected (selectedArgs);

					if (exception != null && !selectedArgs.ExceptionHandled)
						throw exception;

					if (owner.EnableCaching)
						owner.Cache.SetCachedObject (SelectCommand, selectParameters, dataView);
				}

				if (SortParameterName.Length == 0 || SelectCommandType == SqlDataSourceCommandType.Text)
					dataView.Sort = arguments.SortExpression;

				if (FilterExpression.Length > 0) {
					IOrderedDictionary fparams = FilterParameters.GetValues (context, owner);
					SqlDataSourceFilteringEventArgs fargs = new SqlDataSourceFilteringEventArgs (fparams);
					OnFiltering (fargs);
					if (!fargs.Cancel) {
						object [] formatValues = new object [fparams.Count];
						for (int n = 0; n < formatValues.Length; n++) {
							formatValues [n] = fparams [n];
							if (formatValues [n] == null) return dataView;
						}
						dataView.RowFilter = string.Format (FilterExpression, formatValues);
					}
				}

				return dataView;
			}
			else {
				SqlDataSourceSelectingEventArgs selectingArgs = new SqlDataSourceSelectingEventArgs (command, arguments);
				OnSelecting (selectingArgs);
				if (selectingArgs.Cancel) {
					return null;
				}

				DbDataReader reader = null;
				bool closed = connection.State == ConnectionState.Closed;

				if (closed)
					connection.Open ();
				try {
					reader = command.ExecuteReader (closed ? CommandBehavior.CloseConnection : CommandBehavior.Default);
				}
				catch (Exception e) {
					exception = e;
				}
				SqlDataSourceStatusEventArgs selectedArgs =
					new SqlDataSourceStatusEventArgs (command, reader.RecordsAffected, exception);
				OnSelected (selectedArgs);
				if (exception != null && !selectedArgs.ExceptionHandled)
					throw exception;

				return reader;
			}
		}

		public int Update (IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			return ExecuteUpdate (keys, values, oldValues);
		}

		protected override int ExecuteUpdate (IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			if (!CanUpdate)
				throw new NotSupportedException ("Update operation is not supported");
			if (oldValues == null && ConflictDetection == ConflictOptions.CompareAllValues)
				throw new InvalidOperationException ("oldValues parameters should be specified when ConflictOptions is set to CompareAllValues");

			InitConnection ();

			DbCommand command = factory.CreateCommand ();
			command.CommandText = UpdateCommand;
			command.Connection = connection;
			if (UpdateCommandType == SqlDataSourceCommandType.Text)
				command.CommandType = CommandType.Text;
			else
				command.CommandType = CommandType.StoredProcedure;

			IDictionary oldDataValues;
			if (ConflictDetection == ConflictOptions.CompareAllValues) {
				oldDataValues = new OrderedDictionary ();
				if (keys != null) {
					foreach (DictionaryEntry de in keys)
						oldDataValues [de.Key] = de.Value;
				}
				if (oldValues != null) {
					foreach (DictionaryEntry de in oldValues)
						oldDataValues [de.Key] = de.Value;
				}
			}
			else {
				oldDataValues = keys;
			}

			IDictionary dataValues = values;

			InitializeParameters (command, UpdateParameters, dataValues, oldDataValues, ConflictDetection == ConflictOptions.CompareAllValues);

			SqlDataSourceCommandEventArgs args = new SqlDataSourceCommandEventArgs (command);
			OnUpdating (args);
			if (args.Cancel)
				return -1;

			bool closed = connection.State == ConnectionState.Closed;
			if (closed)
				connection.Open ();
			Exception exception = null;
			int result = -1;
			try {
				result = command.ExecuteNonQuery ();
			}
			catch (Exception e) {
				exception = e;
			}

			if (closed)
				connection.Close ();

			OnDataSourceViewChanged (EventArgs.Empty);

			SqlDataSourceStatusEventArgs updatedArgs =
				new SqlDataSourceStatusEventArgs (command, result, exception);
			OnUpdated (updatedArgs);

			if (exception != null && !updatedArgs.ExceptionHandled)
				throw exception;

			return result;
		}

		string FormatOldParameter (string name)
		{
			string f = OldValuesParameterFormatString;
			if (f.Length > 0)
				return String.Format (f, name);
			else
				return name;
		}

		object FindValueByName (Parameter p, IDictionary values, bool format)
		{
			if (values == null)
				return null;

			foreach (DictionaryEntry de in values) {
				string valueName = format == true ? FormatOldParameter (de.Key.ToString ()) : de.Key.ToString ();
				if (p.Name == valueName)
					return values [de.Key];
			}
			foreach (DictionaryEntry de in values) {
				string valueName = format == true ? FormatOldParameter (de.Key.ToString ()) : de.Key.ToString ();
				valueName = valueName.ToLower ();
				if (p.Name.ToLower () == valueName)
					return values [de.Key];
			}
			return null;
		}

		void InitializeParameters (DbCommand command, ParameterCollection parameters, IDictionary values, IDictionary oldValues, bool alwaysAddParameters)
		{
			foreach (Parameter p in parameters) {
				object value = FindValueByName (p, values, false);
				string valueName = p.Name;
				if (value == null)
					value = FindValueByName (p, oldValues, true);
				if (value == null && !alwaysAddParameters) {
					value = FindValueByName (p, oldValues, false);
					valueName = FormatOldParameter (p.Name);
				}

				if (value != null) {
					object dbValue = p.ConvertValue (value);
					DbParameter newParameter = CreateDbParameter (valueName, dbValue, p.Direction, p.Size);
					if (!command.Parameters.Contains (newParameter.ParameterName)) {
						command.Parameters.Add (newParameter);
					}
				}
				else {
					object dbValue = p.GetValue (context, owner);
					command.Parameters.Add (CreateDbParameter (p.Name, dbValue, p.Direction, p.Size));
				}
			}

			if (values != null) {
				foreach (DictionaryEntry de in values)
					if (!command.Parameters.Contains (ParameterPrefix + (string) de.Key))
						command.Parameters.Add (CreateDbParameter ((string) de.Key, de.Value));
			}

			if (oldValues != null) {
				foreach (DictionaryEntry de in oldValues)
					if (!command.Parameters.Contains (ParameterPrefix + FormatOldParameter ((string) de.Key)))
						command.Parameters.Add (CreateDbParameter (FormatOldParameter ((string) de.Key), de.Value));
			}
		}

		private DbParameter CreateDbParameter (string name, object value)
		{
			return CreateDbParameter (name, value, ParameterDirection.Input, -1);
		}
		
		private DbParameter CreateDbParameter (string name, object value, ParameterDirection dir, int size)
		{
			DbParameter dbp = factory.CreateParameter ();
			dbp.ParameterName = ParameterPrefix + name;
			if (value == null)
				dbp.Value = DBNull.Value;
			else
				dbp.Value = value;
			dbp.Direction = dir;
			if (size != -1)
				dbp.Size = size;

			return dbp;
		}

		void IStateManager.LoadViewState (object savedState)
		{
			LoadViewState (savedState);
		}
		
		object IStateManager.SaveViewState ()
		{
			return SaveViewState ();
		}
		
		void IStateManager.TrackViewState ()
		{
			TrackViewState ();
		}
		
		protected virtual void LoadViewState (object savedState)
		{
			object [] vs = savedState as object [];
			if (vs == null)
				return;
			
			if (vs [0] != null) ((IStateManager) deleteParameters).LoadViewState (vs [0]);
			if (vs [1] != null) ((IStateManager) filterParameters).LoadViewState (vs [1]);
			if (vs [2] != null) ((IStateManager) insertParameters).LoadViewState (vs [2]);
			if (vs [3] != null) ((IStateManager) selectParameters).LoadViewState (vs [3]);
			if (vs [4] != null) ((IStateManager) updateParameters).LoadViewState (vs [4]);
		}

		protected virtual object SaveViewState ()
		{
			object [] vs = new object [5];
			
			if (deleteParameters != null) vs [0] = ((IStateManager) deleteParameters).SaveViewState ();
			if (filterParameters != null) vs [1] = ((IStateManager) filterParameters).SaveViewState ();
			if (insertParameters != null) vs [2] = ((IStateManager) insertParameters).SaveViewState ();
			if (selectParameters != null) vs [3] = ((IStateManager) selectParameters).SaveViewState ();
			if (updateParameters != null) vs [4] = ((IStateManager) updateParameters).SaveViewState ();
				
			foreach (object o in vs)
				if (o != null) return vs;
			return null;
		}
		
		protected virtual void TrackViewState ()
		{
			tracking = true;
			
			if (filterParameters != null) ((IStateManager) filterParameters).TrackViewState ();
			if (selectParameters != null) ((IStateManager) selectParameters).TrackViewState ();
		}
		
		bool IStateManager.IsTrackingViewState {
			get { return IsTrackingViewState; }
		}

		// LAME SPEC: MSDN says value should be saved in ViewState but tests show otherwise.
		private bool cancelSelectOnNullParameter = true;
		public bool CancelSelectOnNullParameter {
			get { return cancelSelectOnNullParameter; }
			set { cancelSelectOnNullParameter = value; }
		}

		public override bool CanDelete {
			get { return DeleteCommand != null && DeleteCommand != ""; }
		}

		public override bool CanInsert {
			get { return InsertCommand != null && InsertCommand != ""; }
		}
		
		public override bool CanPage {
			/* according to MS, this is false in all cases */
			get { return false; }
		}

		public override bool CanRetrieveTotalRowCount {
			/* according to MS, this is false in all cases */
			get { return false; }
		}

		public override bool CanSort {
			get {
				/* we can sort if we're a DataSet, regardless of sort parameter name.
				   we can sort if we're a DataReader, if the sort parameter name is not null/"".
				*/
				return (owner.DataSourceMode == SqlDataSourceMode.DataSet
					|| (SortParameterName != null && SortParameterName != ""));
			}
		}
		
		public override bool CanUpdate {
			get { return UpdateCommand != null && UpdateCommand != ""; }
		}

		// LAME SPEC: MSDN says value should be saved in ViewState but tests show otherwise.
		private ConflictOptions conflictDetection = ConflictOptions.OverwriteChanges;
		public ConflictOptions ConflictDetection {
			get { return conflictDetection; }
			set { conflictDetection = value; }
		}

		// LAME SPEC: MSDN says value should be saved in ViewState but tests show otherwise.
		private string deleteCommand = "";
		public string DeleteCommand {
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}

		// LAME SPEC: MSDN says value should be saved in ViewState but tests show otherwise.
		private SqlDataSourceCommandType deleteCommandType = SqlDataSourceCommandType.Text;
		public SqlDataSourceCommandType DeleteCommandType {
			get { return deleteCommandType; }
			set { deleteCommandType = value; }
		}

		[DefaultValueAttribute (null)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[EditorAttribute ("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public ParameterCollection DeleteParameters {
			get { return GetParameterCollection (ref deleteParameters, false); }
		}

		// LAME SPEC: MSDN says value should be saved in ViewState but tests show otherwise.
		private string filterExpression = "";
		public string FilterExpression {
			get { return filterExpression; }
			set { filterExpression = value; }
		}

		[EditorAttribute ("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DefaultValueAttribute (null)]
		public ParameterCollection FilterParameters {
			get { return GetParameterCollection (ref filterParameters, true); }
		}

		// LAME SPEC: MSDN says value should be saved in ViewState but tests show otherwise.
		private string insertCommand = "";
		public string InsertCommand {
			get { return insertCommand; }
			set { insertCommand = value; }
		}

		// LAME SPEC: MSDN says value should be saved in ViewState but tests show otherwise.
		private SqlDataSourceCommandType insertCommandType = SqlDataSourceCommandType.Text;
		public SqlDataSourceCommandType InsertCommandType {
			get { return insertCommandType; }
			set { insertCommandType = value; }
		}

		[EditorAttribute ("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DefaultValueAttribute (null)]
		public ParameterCollection InsertParameters {
			get { return GetParameterCollection (ref insertParameters, false); }
		}

		protected bool IsTrackingViewState {
			get { return tracking; }
		}

		// LAME SPEC: MSDN says value should be saved in ViewState but tests show otherwise.
		private string oldValuesParameterFormatString = "{0}";
		[DefaultValue ("{0}")]
		public string OldValuesParameterFormatString {
			get { return oldValuesParameterFormatString; }
			set { oldValuesParameterFormatString = value; }
		}

		// LAME SPEC: MSDN says value should be saved in ViewState but tests show otherwise.
		private string selectCommand = "";
		public string SelectCommand {
			get { return selectCommand; }
			set { selectCommand = value; }
		}

		// LAME SPEC: MSDN says value should be saved in ViewState but tests show otherwise.
		private SqlDataSourceCommandType selectCommandType = SqlDataSourceCommandType.Text;
		public SqlDataSourceCommandType SelectCommandType {
			get { return selectCommandType; }
			set { selectCommandType = value; }
		}
		
		public ParameterCollection SelectParameters {
			get { return GetParameterCollection (ref selectParameters, true); }
		}

		// LAME SPEC: MSDN says value should be saved in ViewState but tests show otherwise.
		private string sortParameterName = "";
		public string SortParameterName {
			get { return sortParameterName; }
			set { sortParameterName = value; }
		}

		// LAME SPEC: MSDN says value should be saved in ViewState but tests show otherwise.
		private string updateCommand = "";
		public string UpdateCommand {
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		// LAME SPEC: MSDN says value should be saved in ViewState but tests show otherwise.
		private SqlDataSourceCommandType updateCommandType = SqlDataSourceCommandType.Text;
		public SqlDataSourceCommandType UpdateCommandType {
			get { return updateCommandType; }
			set { updateCommandType = value; }
		}

		[EditorAttribute ("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[PersistenceModeAttribute (PersistenceMode.InnerProperty)]
		[DefaultValueAttribute (null)]
		public ParameterCollection UpdateParameters {
			get { return GetParameterCollection (ref updateParameters, false); }
		}
		
		void ParametersChanged (object source, EventArgs args)
		{
			OnDataSourceViewChanged (EventArgs.Empty);
		}
		
		ParameterCollection GetParameterCollection (ref ParameterCollection output, bool propagateTrackViewState)
		{
			if (output != null)
				return output;
			
			output = new ParameterCollection ();
			output.ParametersChanged += new EventHandler (ParametersChanged);
			
			if (IsTrackingViewState && propagateTrackViewState)
				((IStateManager) output).TrackViewState ();
			
			return output;
		}
		
		protected virtual string ParameterPrefix {
			get {
				switch (owner.ProviderName) {
					case "":
					case "System.Data.SqlClient": return "@";
					case "System.Data.OracleClient": return ":";
				}
				return "";
			}
		}

		ParameterCollection deleteParameters;
		ParameterCollection filterParameters;
		ParameterCollection insertParameters;
		ParameterCollection selectParameters;
		ParameterCollection updateParameters;

		bool tracking;
	
		string name;
		SqlDataSource owner;

		#region OnDelete
		static readonly object EventDeleted = new object ();
		protected virtual void OnDeleted (SqlDataSourceStatusEventArgs e)
		{
			if (!HasEvents ()) return;
			SqlDataSourceStatusEventHandler h = Events [EventDeleted] as SqlDataSourceStatusEventHandler;
			if (h != null)
				h (this, e);
		}
		
		public event SqlDataSourceStatusEventHandler Deleted {
			add { Events.AddHandler (EventDeleted, value); }
			remove { Events.RemoveHandler (EventDeleted, value); }
		}
		
		static readonly object EventDeleting = new object ();
		protected virtual void OnDeleting (SqlDataSourceCommandEventArgs e)
		{
			if (!HasEvents ()) return;
			SqlDataSourceCommandEventHandler h = Events [EventDeleting] as SqlDataSourceCommandEventHandler;
			if (h != null)
				h (this, e);
		}
		public event SqlDataSourceCommandEventHandler Deleting {
			add { Events.AddHandler (EventDeleting, value); }
			remove { Events.RemoveHandler (EventDeleting, value); }
		}
		#endregion

		#region OnFiltering
		static readonly object EventFiltering = new object ();
		protected virtual void OnFiltering (SqlDataSourceFilteringEventArgs e)
		{
			if (!HasEvents ()) return;
			SqlDataSourceFilteringEventHandler h = Events [EventFiltering] as SqlDataSourceFilteringEventHandler;
			if (h != null)
				h (this, e);
		}
		public event SqlDataSourceFilteringEventHandler Filtering {
			add { Events.AddHandler (EventFiltering, value); }
			remove { Events.RemoveHandler (EventFiltering, value); }
		}
		#endregion
		
		#region OnInsert
		static readonly object EventInserted = new object ();
		protected virtual void OnInserted (SqlDataSourceStatusEventArgs e)
		{
			if (!HasEvents ()) return;
			SqlDataSourceStatusEventHandler h = Events [EventInserted] as SqlDataSourceStatusEventHandler;
			if (h != null)
				h (this, e);
		}
		
		public event SqlDataSourceStatusEventHandler Inserted {
			add { Events.AddHandler (EventInserted, value); }
			remove { Events.RemoveHandler (EventInserted, value); }
		}
		
		static readonly object EventInserting = new object ();
		protected virtual void OnInserting (SqlDataSourceCommandEventArgs e)
		{
			if (!HasEvents ()) return;
			SqlDataSourceCommandEventHandler h = Events [EventInserting] as SqlDataSourceCommandEventHandler;
			if (h != null)
				h (this, e);
		}
		public event SqlDataSourceCommandEventHandler Inserting {
			add { Events.AddHandler (EventInserting, value); }
			remove { Events.RemoveHandler (EventInserting, value); }
		}
		#endregion
		
		#region OnSelect
		static readonly object EventSelected = new object ();
		protected virtual void OnSelected (SqlDataSourceStatusEventArgs e)
		{
			if (!HasEvents ()) return;
			SqlDataSourceStatusEventHandler h = Events [EventSelected] as SqlDataSourceStatusEventHandler;
			if (h != null)
				h (this, e);
		}
		
		public event SqlDataSourceStatusEventHandler Selected {
			add { Events.AddHandler (EventSelected, value); }
			remove { Events.RemoveHandler (EventSelected, value); }
		}
		
		static readonly object EventSelecting = new object ();
		protected virtual void OnSelecting (SqlDataSourceSelectingEventArgs e)
		{
			if (!HasEvents ()) return;
			SqlDataSourceSelectingEventHandler h = Events [EventSelecting] as SqlDataSourceSelectingEventHandler;
			if (h != null)
				h (this, e);
		}
		public event SqlDataSourceSelectingEventHandler Selecting {
			add { Events.AddHandler (EventSelecting, value); }
			remove { Events.RemoveHandler (EventSelecting, value); }
		}
		#endregion
		
		#region OnUpdate
		static readonly object EventUpdated = new object ();
		protected virtual void OnUpdated (SqlDataSourceStatusEventArgs e)
		{
			if (owner.EnableCaching)
				owner.Cache.Expire ();

			if (!HasEvents ()) return;
			SqlDataSourceStatusEventHandler h = Events [EventUpdated] as SqlDataSourceStatusEventHandler;
			if (h != null)
				h (this, e);
		}
		
		public event SqlDataSourceStatusEventHandler Updated {
			add { Events.AddHandler (EventUpdated, value); }
			remove { Events.RemoveHandler (EventUpdated, value); }
		}
		
		static readonly object EventUpdating = new object ();
		protected virtual void OnUpdating (SqlDataSourceCommandEventArgs e)
		{
			if (!HasEvents ()) return;
			SqlDataSourceCommandEventHandler h = Events [EventUpdating] as SqlDataSourceCommandEventHandler;
			if (h != null)
				h (this, e);
		}
		public event SqlDataSourceCommandEventHandler Updating {
			add { Events.AddHandler (EventUpdating, value); }
			remove { Events.RemoveHandler (EventUpdating, value); }
		}
		#endregion
				
	}
	
}
#endif

