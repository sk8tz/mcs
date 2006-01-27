//
// System.Data.Common.DataAdapter
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) Tim Coleman, 2002-2003
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

using System.ComponentModel;
using System.Data;

namespace System.Data.Common
{
	/// <summary>
	/// Represents a set of data commands and a database connection that are used to fill the DataSet and update the data source.
	/// </summary>
	public abstract class DataAdapter : Component, IDataAdapter
	{
		#region Fields

		private bool acceptChangesDuringFill;
		private bool continueUpdateOnError;
		private MissingMappingAction missingMappingAction;
		private MissingSchemaAction missingSchemaAction;
		private DataTableMappingCollection tableMappings;

#if NET_2_0
		private bool acceptChangesDuringUpdate;
		private LoadOption fillLoadOption;
		private bool returnProviderSpecificTypes;
#endif

		#endregion

		#region Constructors

		protected DataAdapter () 
		{
			acceptChangesDuringFill = true;
			continueUpdateOnError = false;
			missingMappingAction = MissingMappingAction.Passthrough;
			missingSchemaAction = MissingSchemaAction.Add;
			tableMappings = new DataTableMappingCollection ();
		}

		protected DataAdapter (DataAdapter adapter)
		{
			AcceptChangesDuringFill = adapter.AcceptChangesDuringFill;
			ContinueUpdateOnError = adapter.ContinueUpdateOnError;
			MissingMappingAction = adapter.MissingMappingAction;
			MissingSchemaAction = adapter.MissingSchemaAction;
			if (adapter.tableMappings == null || adapter.TableMappings.Count <= 0) {
				return;
			}
			foreach (ICloneable cloneable in adapter.TableMappings) {
				TableMappings.Add (cloneable.Clone ());
			}
		}

		#endregion

		#region Properties

		[DataCategory ("Fill")]
		[DataSysDescription ("Whether or not Fill will call DataRow.AcceptChanges.")]
		[DefaultValue (true)]
		public bool AcceptChangesDuringFill {
			get { return acceptChangesDuringFill; }
			set { acceptChangesDuringFill = value; }
		}

#if NET_2_0
		public bool AcceptChangesDuringUpdate {
			get { return acceptChangesDuringUpdate; }
			set { acceptChangesDuringUpdate = value; }
		}
#endif

		[DataCategory ("Update")]
		[DataSysDescription ("Whether or not to continue to the next DataRow when the Update events, RowUpdating and RowUpdated, Status is UpdateStatus.ErrorsOccurred.")]
		[DefaultValue (false)]
		public bool ContinueUpdateOnError {
			get { return continueUpdateOnError; }
			set { continueUpdateOnError = value; }
		}

#if NET_2_0
		public LoadOption FillLoadOption {
			get { return fillLoadOption; }
			set { fillLoadOption = value; }
		}
#endif

		ITableMappingCollection IDataAdapter.TableMappings {
			get { return TableMappings; }
		}

		[DataCategory ("Mapping")]
		[DataSysDescription ("The action taken when a table or column in the TableMappings is missing.")]
		[DefaultValue (MissingMappingAction.Passthrough)]
		public MissingMappingAction MissingMappingAction {
			get { return missingMappingAction; }
			set {
				if (!Enum.IsDefined (typeof (MissingMappingAction), value))
					throw ExceptionHelper.InvalidEnumValueException ("MissingMappingAction", value);
				missingMappingAction = value;
			}
		}

		[DataCategory ("Mapping")]
		[DataSysDescription ("The action taken when a table or column in the DataSet is missing.")]
		[DefaultValue (MissingSchemaAction.Add)]
		public MissingSchemaAction MissingSchemaAction {
			get { return missingSchemaAction; }
			set { 
				if (!Enum.IsDefined (typeof (MissingSchemaAction), value))
					throw ExceptionHelper.InvalidEnumValueException ("MissingSchemaAction", value);
				missingSchemaAction = value; 
			}
		}

#if NET_2_0
		public virtual bool ReturnProviderSpecificTypes {
			get { return returnProviderSpecificTypes; }
			set { returnProviderSpecificTypes = value; }
		}
#endif

		[DataCategory ("Mapping")]
		[DataSysDescription ("How to map source table to DataSet table.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataTableMappingCollection TableMappings {
			get { return tableMappings; }
		}

		#endregion

		#region Events

#if NET_2_0
		public event FillErrorEventHandler FillError;
#endif

		#endregion

		#region Methods

#if NET_1_1
                [Obsolete ("Use the protected constructor instead", false)]
#endif
                [MonoTODO]
		protected virtual DataAdapter CloneInternals ()
		{
			throw new NotImplementedException ();
		}

		protected virtual DataTableMappingCollection CreateTableMappings ()
		{
			tableMappings = new DataTableMappingCollection ();
			return tableMappings;
		}

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		public abstract int Fill (DataSet dataSet);

#if NET_2_0
		[MonoTODO]
		protected virtual int Fill (DataTable dataTable, IDataReader dataReader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill (DataTable[] dataTables, IDataReader dataReader, int startRecord, int maxRecords)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual int Fill (DataSet dataSet, string srcTable, IDataReader dataReader, int startRecord, int maxRecords)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int FillDataSet (IDataReader dataReader, LoadOption fillLoadOption, DataSet dataSet)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int FillDataTable (IDataReader dataReader, LoadOption fillLoadOption, DataTable[] dataTables)
		{
			throw new NotImplementedException ();
		}
#endif

		public abstract DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType);

#if NET_2_0
		[MonoTODO]
		protected virtual DataTable FillSchema (DataTable dataTable, SchemaType schemaType, IDataReader dataReader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual DataTable[] FillSchema (DataSet dataSet, SchemaType schemaType, string srcTable, IDataReader dataReader)
		{
			throw new NotImplementedException ();
		}
#endif

		public abstract IDataParameter[] GetFillParameters ();

#if NET_2_0
		[MonoTODO]
		protected bool HasTableMappings ()
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnFillError (FillErrorEventArgs value)
		{
			if (FillError != null)
				FillError (this, value);

		}

		[MonoTODO]
		public void ResetFillLoadOption ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool ShouldSerializeAcceptChangesDuringFill ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool ShouldSerializeFillLoadOption ()
		{
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		protected virtual bool ShouldSerializeTableMappings ()
		{
			throw new NotImplementedException ();
		}

		public abstract int Update (DataSet dataSet);

		#endregion
		
	}
}
