//
// System.Data.DataRow.cs
//
// Author:
//   Rodrigo Moya <rodrigo@ximian.com>
//   Daniel Morgan <danmorg@sc.rr.com>
//   Tim Coleman <tim@timcoleman.com>
//   Ville Palo <vi64pa@koti.soon.fi>
//   Alan Tam Siu Lung <Tam@SiuLung.com>
//
// (C) Ximian, Inc 2002
// (C) Daniel Morgan 2002, 2003
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.Collections;
using System.Globalization;

namespace System.Data {
	/// <summary>
	/// Represents a row of data in a DataTable.
	/// </summary>
	[Serializable]
	public class DataRow
	{
		#region Fields

		private DataTable _table;

		private object[] original;
		private object[] proposed;
		private object[] current;

		private string[] columnErrors;
		private string rowError;
		private DataRowState rowState;
		internal int xmlRowID = 0;
		internal bool _nullConstraintViolation;
		private string _nullConstraintMessage;
		private bool editing = false;
		private bool _hasParentCollection;
		private bool _inChangingEvent;
		private int _rowId;

		#endregion

		#region Constructors

		/// <summary>
		/// This member supports the .NET Framework infrastructure and is not intended to be 
		/// used directly from your code.
		/// </summary>
		protected internal DataRow (DataRowBuilder builder)
		{
			_table = builder.Table;
			// Get the row id from the builder.
			_rowId = builder._rowId;

			original = null; 
			
			proposed = new object[_table.Columns.Count];
			// Initialise the data coloumns of the row with the dafault values, if any 
			for (int c = 0; c < _table.Columns.Count; c++) 
			{
				if(_table.Columns [c].DefaultValue == null)
					proposed[c] = DBNull.Value;
				else
					proposed [c] = _table.Columns[c].DefaultValue;
			}
			
			columnErrors = new string[_table.Columns.Count];
			rowError = String.Empty;

			//on first creating a DataRow it is always detached.
			rowState = DataRowState.Detached;
			
			foreach (DataColumn Col in _table.Columns) {
				
				if (Col.AutoIncrement) {
					this [Col] = Col.AutoIncrementValue();
				}
			}
			_table.Columns.CollectionChanged += new System.ComponentModel.CollectionChangeEventHandler(CollectionChanged);
		}

		
		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating whether there are errors in a row.
		/// </summary>
		public bool HasErrors {
			get {
				if (RowError != string.Empty)
					return true;

				for (int i= 0; i < columnErrors.Length; i++){
					if (columnErrors[i] != null && columnErrors[i] != string.Empty)
						return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Gets or sets the data stored in the column specified by name.
		/// </summary>
		public object this[string columnName] {
			get { return this[columnName, DataRowVersion.Default]; }
			set {
				int columnIndex = _table.Columns.IndexOf (columnName);
				if (columnIndex == -1)
					throw new IndexOutOfRangeException ();
				this[columnIndex] = value;
			}
		}

		/// <summary>
		/// Gets or sets the data stored in specified DataColumn
		/// </summary>
		public object this[DataColumn column] {

			get {
				return this[column, DataRowVersion.Default];} 
			set {
				int columnIndex = _table.Columns.IndexOf (column);
				if (columnIndex == -1)
					throw new ArgumentException ("The column does not belong to this table.");
				this[columnIndex] = value;
			}
		}

		/// <summary>
		/// Gets or sets the data stored in column specified by index.
		/// </summary>
		public object this[int columnIndex] {
			get { return this[columnIndex, DataRowVersion.Default]; }
			set {
				if (columnIndex < 0 || columnIndex > _table.Columns.Count)
					throw new IndexOutOfRangeException ();
				if (rowState == DataRowState.Deleted)
					throw new DeletedRowInaccessibleException ();
				DataColumn column = _table.Columns[columnIndex];
				_table.ChangingDataColumn (this, column, value);
				
				
				bool orginalEditing = editing;
				if (!orginalEditing) BeginEdit ();
				object v = SetColumnValue (value, columnIndex);
				proposed[columnIndex] = v;
				_table.ChangedDataColumn (this, column, v);
				if (!orginalEditing) EndEdit ();
			}
		}

		/// <summary>
		/// Gets the specified version of data stored in the named column.
		/// </summary>
		public object this[string columnName, DataRowVersion version] {
			get {
				int columnIndex = _table.Columns.IndexOf (columnName);
				if (columnIndex == -1)
					throw new IndexOutOfRangeException ();
				return this[columnIndex, version];
			}
		}

		/// <summary>
		/// Gets the specified version of data stored in the specified DataColumn.
		/// </summary>
		public object this[DataColumn column, DataRowVersion version] {
			get {
				if (column.Table != Table)
					throw new ArgumentException ("The column does not belong to this table.");
				int columnIndex = column.Ordinal;
				return this[columnIndex, version];
			}
		}

		/// <summary>
		/// Gets the data stored in the column, specified by index and version of the data to
		/// retrieve.
		/// </summary>
		public object this[int columnIndex, DataRowVersion version] {
			get {
				if (columnIndex < 0 || columnIndex > _table.Columns.Count)
					throw new IndexOutOfRangeException ();
				// Accessing deleted rows
				if (rowState == DataRowState.Deleted && version != DataRowVersion.Original)
					throw new DeletedRowInaccessibleException ("Deleted row information cannot be accessed through the row.");
				
				DataColumn col = _table.Columns[columnIndex];
				if (col.Expression != String.Empty) {
					object o = col.CompiledExpression.Eval (this);
					return Convert.ChangeType (o, col.DataType);
				}
				
				if (HasVersion(version))
				{
					switch (version) 
					{
						case DataRowVersion.Default:
							if (editing || rowState == DataRowState.Detached)
								return proposed[columnIndex];
							return current[columnIndex];
						case DataRowVersion.Proposed:
							return proposed[columnIndex];
						case DataRowVersion.Current:
							return current[columnIndex];
						case DataRowVersion.Original:
							return original[columnIndex];
						default:
							throw new ArgumentException ();
					}
				}
				if (rowState == DataRowState.Detached && version == DataRowVersion.Default && proposed == null)
					throw new RowNotInTableException("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.");
				
				throw new VersionNotFoundException (Locale.GetText ("There is no " + version.ToString () + " data to access."));
			}
		}
		
		internal void SetOriginalValue (string columnName, object val)
		{
			int columnIndex = _table.Columns.IndexOf (columnName);
			DataColumn column = _table.Columns[columnIndex];
			_table.ChangingDataColumn (this, column, val);
				
			if (original == null) original = new object [_table.Columns.Count];
			val = SetColumnValue (val, columnIndex);
			original[columnIndex] = val;
			rowState = DataRowState.Modified;
		}

		/// <summary>
		/// Gets or sets all of the values for this row through an array.
		/// </summary>
		public object[] ItemArray {
			get { 
				// row not in table
				if (rowState == DataRowState.Detached)
					throw new RowNotInTableException("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.");
				// Accessing deleted rows
				if (rowState == DataRowState.Deleted)
					throw new DeletedRowInaccessibleException ("Deleted row information cannot be accessed through the row.");
				
				return current; 
			}
			set {
				if (value.Length > _table.Columns.Count)
					throw new ArgumentException ();

				if (rowState == DataRowState.Deleted)
					throw new DeletedRowInaccessibleException ();
				
				object[] newItems = new object[_table.Columns.Count];			
				object v = null;
				for (int i = 0; i < _table.Columns.Count; i++) {

					if (i < value.Length)
						v = value[i];
					else
						v = null;

					newItems[i] = SetColumnValue (v, i);
				}

				bool orginalEditing = editing;
				if (!orginalEditing) BeginEdit ();
				proposed = newItems;
				if (!orginalEditing) EndEdit ();
			}
		}

		private object SetColumnValue (object v, int index) 
		{		
			object newval = null;
			DataColumn col = _table.Columns[index];
			
			if (_hasParentCollection && col.ReadOnly && v != this[index])
				throw new ReadOnlyException ();

			if (v == null)
			{
				if (col.DataType.ToString().Equals("System.Guid"))
	                                throw new ArgumentException("Cannot set column to be null, Please use DBNull instead");

				if(col.DefaultValue != DBNull.Value) 
				{
					newval = col.DefaultValue;
				}
				else if(col.AutoIncrement == true) 
				{
					newval = this [index];
				}
				else 
				{
					if (!col.AllowDBNull)
					{
						//Constraint violations during data load is raise in DataTable EndLoad
						this._nullConstraintViolation = true;
						if (this.Table._duringDataLoad) {
							this.Table._nullConstraintViolationDuringDataLoad = true;
						}
						_nullConstraintMessage = "Column '" + col.ColumnName + "' does not allow nulls.";
					}

					newval = DBNull.Value;
				}
			}		
			else if (v == DBNull.Value) 
			{
				
				if (!col.AllowDBNull)
				{
					//Constraint violations during data load is raise in DataTable EndLoad
					this._nullConstraintViolation = true;
					if (this.Table._duringDataLoad) {
							this.Table._nullConstraintViolationDuringDataLoad = true;
					}
					_nullConstraintMessage = "Column '" + col.ColumnName + "' does not allow nulls.";
				}
				
				newval = DBNull.Value;
			}
			else 
			{	
				Type vType = v.GetType(); // data type of value
				Type cType = col.DataType; // column data type
				if (cType != vType) 
				{
					TypeCode typeCode = Type.GetTypeCode(cType);
					switch(typeCode) {
						case TypeCode.Boolean :
							v = Convert.ToBoolean (v);
							break;
						case TypeCode.Byte  :
							v = Convert.ToByte (v);
							break;
						case TypeCode.Char  :
							v = Convert.ToChar (v);
							break;
						case TypeCode.DateTime  :
							v = Convert.ToDateTime (v);
							break;
						case TypeCode.Decimal  :
							v = Convert.ToDecimal (v);
							break;
						case TypeCode.Double  :
							v = Convert.ToDouble (v);
							break;
						case TypeCode.Int16  :
							v = Convert.ToInt16 (v);
							break;
						case TypeCode.Int32  :
							v = Convert.ToInt32 (v);
							break;
						case TypeCode.Int64  :
							v = Convert.ToInt64 (v);
							break;
						case TypeCode.SByte  :
							v = Convert.ToSByte (v);
							break;
						case TypeCode.Single  :
							v = Convert.ToSingle (v);
							break;
						case TypeCode.String  :
							v = Convert.ToString (v);
							break;
						case TypeCode.UInt16  :
							v = Convert.ToUInt16 (v);
							break;
						case TypeCode.UInt32  :
							v = Convert.ToUInt32 (v);
							break;
						case TypeCode.UInt64  :
							v = Convert.ToUInt64 (v);
							break;
						default :
								
						switch(cType.ToString()) {
							case "System.TimeSpan" :
								v = (System.TimeSpan) v;
								break;
							case "System.Type" :
								v = (System.Type) v;
								break;
							case "System.Object" :
								//v = (System.Object) v;
								break;
							default:
								if (!cType.IsArray)
									throw new InvalidCastException("Type not supported.");
								break;
						}
							break;
					}
					vType = v.GetType();
				}

				// The MaxLength property is ignored for non-text columns
				if ((Type.GetTypeCode(vType) == TypeCode.String) && (this.Table.Columns[index].MaxLength != -1) && 
					(this.Table.Columns[index].MaxLength < ((string)v).Length)) {
					throw new ArgumentException("Cannot set column '" + this.Table.Columns[index].ColumnName + "' to '" + v + "'. The value violates the MaxLength limit of this column.");
				}
				newval = v;
				if(col.AutoIncrement == true) {
					long inc = Convert.ToInt64(v);
					col.UpdateAutoIncrementValue (inc);
				}
			}
			col.DataHasBeenSet = true;
			return newval;
		}

		/// <summary>
		/// Gets or sets the custom error description for a row.
		/// </summary>
		public string RowError {
			get { return rowError; }
			set { rowError = value; }
		}

		/// <summary>
		/// Gets the current state of the row in regards to its relationship to the
		/// DataRowCollection.
		/// </summary>
		public DataRowState RowState {
			get { return rowState; }
		}

		//FIXME?: Couldn't find a way to set the RowState when adding the DataRow
		//to a Datatable so I added this method. Delete if there is a better way.
		internal void AttachRow() {
			current = proposed;
			proposed = null;
			rowState = DataRowState.Added;
		}

		//FIXME?: Couldn't find a way to set the RowState when removing the DataRow
		//from a Datatable so I added this method. Delete if there is a better way.
		internal void DetachRow() {
			proposed = null;
			_rowId = -1;
			_hasParentCollection = false;
			rowState = DataRowState.Detached;
		}

		/// <summary>
		/// Gets the DataTable for which this row has a schema.
		/// </summary>
		public DataTable Table {
			get { return _table; }
		}

		/// <summary>
		/// Gets and sets index of row. This is used from 
		/// XmlDataDocument.
		// </summary>
		internal int XmlRowID {
			get { return xmlRowID; }
			set { xmlRowID = value; }
		}
		
		/// <summary>
		/// Gets and sets index of row.
		// </summary>
		internal int RowID {
			get { return _rowId; }
			set { _rowId = value; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Commits all the changes made to this row since the last time AcceptChanges was
		/// called.
		/// </summary>
		public void AcceptChanges () 
		{
			EndEdit(); // in case it hasn't been called
			switch (rowState) {
			case DataRowState.Added:
			case DataRowState.Modified:
				rowState = DataRowState.Unchanged;
				break;
			case DataRowState.Deleted:
				_table.Rows.RemoveInternal (this);
				DetachRow();
				break;
			case DataRowState.Detached:
				throw new RowNotInTableException("Cannot perform this operation on a row not in the table.");
			}
			// Accept from detached
			if (original == null)
				original = new object[_table.Columns.Count];
			Array.Copy (current, original, _table.Columns.Count);
		}

		/// <summary>
		/// Begins an edit operation on a DataRow object.
		/// </summary>
		[MonoTODO]
		public void BeginEdit () 
		{
			if (rowState == DataRowState.Deleted)
				throw new DeletedRowInaccessibleException ();
			if (!HasVersion (DataRowVersion.Proposed)) {
				proposed = new object[_table.Columns.Count];
				Array.Copy (current, proposed, current.Length);
			}
			//TODO: Suspend validation
			editing = true;
		}

		/// <summary>
		/// Cancels the current edit on the row.
		/// </summary>
		[MonoTODO]
		public void CancelEdit () 
		{
			editing = false;
			//TODO: Events
			if (HasVersion (DataRowVersion.Proposed)) {
				proposed = null;
				if (rowState == DataRowState.Modified)
				    rowState = DataRowState.Unchanged;
			}
		}

		/// <summary>
		/// Clears the errors for the row, including the RowError and errors set with
		/// SetColumnError.
		/// </summary>
		public void ClearErrors () 
		{
			rowError = String.Empty;
			columnErrors = new String[_table.Columns.Count];
		}

		/// <summary>
		/// Deletes the DataRow.
		/// </summary>
		[MonoTODO]
		public void Delete () 
		{
			_table.DeletingDataRow(this, DataRowAction.Delete);
			switch (rowState) {
			case DataRowState.Added:
				// check what to do with child rows
				CheckChildRows(DataRowAction.Delete);
				_table.DeleteRowFromIndexes (this);
				Table.Rows.RemoveInternal (this);

				// if row was in Added state we move it to Detached.
				DetachRow();
				break;
			case DataRowState.Deleted:
				break;
			default:
				// check what to do with child rows
				CheckChildRows(DataRowAction.Delete);
				_table.DeleteRowFromIndexes (this);
				rowState = DataRowState.Deleted;
				break;
			}
			_table.DeletedDataRow(this, DataRowAction.Delete);
		}

		// check the child rows of this row before deleting the row.
		private void CheckChildRows(DataRowAction action)
		{
			
			// in this method we find the row that this row is in a relation with them.
			// in shortly we find all child rows of this row.
			// then we function according to the DeleteRule of the foriegnkey.


			// 1. find if this row is attached to dataset.
			// 2. find if EnforceConstraints is true.
			// 3. find if there are any constraint on the table that the row is in.
			if (_table.DataSet != null && _table.DataSet.EnforceConstraints && _table.Constraints.Count > 0)
			{
				foreach (DataTable table in _table.DataSet.Tables)
				{
					// loop on all ForeignKeyConstrain of the table.
					foreach (ForeignKeyConstraint fk in table.Constraints.ForeignKeyConstraints)
					{
						if (fk.RelatedTable == _table)
						{
							Rule rule;
							if (action == DataRowAction.Delete)
								rule = fk.DeleteRule;
							else
								rule = fk.UpdateRule;
							CheckChildRows(fk, action, rule);
						}			
					}
				}
			}
		}

		private void CheckChildRows(ForeignKeyConstraint fkc, DataRowAction action, Rule rule)
		{				
			DataRow[] childRows = GetChildRows(fkc, DataRowVersion.Default);
			switch (rule)
			{
				case Rule.Cascade:  // delete or change all relted rows.
					if (childRows != null)
					{
						for (int j = 0; j < childRows.Length; j++)
						{
							// if action is delete we delete all child rows
							if (action == DataRowAction.Delete)
							{
								if (childRows[j].RowState != DataRowState.Deleted)
									childRows[j].Delete();
							}
							// if action is change we change the values in the child row
							else if (action == DataRowAction.Change)
							{
								// change only the values in the key columns
								// set the childcolumn value to the new parent row value
								for (int k = 0; k < fkc.Columns.Length; k++)
									childRows[j][fkc.Columns[k]] = this[fkc.RelatedColumns[k], DataRowVersion.Proposed];
							}
						}
					}
					break;
				case Rule.None: // throw an exception if there are any child rows.
					if (childRows != null)
					{
						for (int j = 0; j < childRows.Length; j++)
						{
							if (childRows[j].RowState != DataRowState.Deleted)
							{
								string changeStr = "Cannot change this row because constraints are enforced on relation " + fkc.ConstraintName +", and changing this row will strand child rows.";
								string delStr = "Cannot delete this row because constraints are enforced on relation " + fkc.ConstraintName +", and deleting this row will strand child rows.";
								string message = action == DataRowAction.Delete ? delStr : changeStr;
								throw new InvalidConstraintException(message);
							}
						}
					}
					break;
				case Rule.SetDefault: // set the values in the child rows to the defult value of the columns.
					if (childRows != null)
					{
						for (int j = 0; j < childRows.Length; j++)
						{
							DataRow child = childRows[j];
							if (childRows[j].RowState != DataRowState.Deleted)
							{
								//set only the key columns to default
								for (int k = 0; k < fkc.Columns.Length; k++)
									child[fkc.Columns[k]] = fkc.Columns[k].DefaultValue;
							}
						}
					}
					break;
				case Rule.SetNull: // set the values in the child row to null.
					if (childRows != null)
					{
						for (int j = 0; j < childRows.Length; j++)
						{
							DataRow child = childRows[j];
							if (childRows[j].RowState != DataRowState.Deleted)
							{
								// set only the key columns to DBNull
								for (int k = 0; k < fkc.Columns.Length; k++)
									child.SetNull(fkc.Columns[k]);
							}
						}
					}
					break;
			}

		}

		/// <summary>
		/// Ends the edit occurring on the row.
		/// </summary>
		[MonoTODO]
		public void EndEdit () 
		{
			if (_inChangingEvent)
				throw new InRowChangingEventException("Cannot call EndEdit inside an OnRowChanging event.");
			if (rowState == DataRowState.Detached)
			{
				editing = false;
				return;
			}

			if (HasVersion (DataRowVersion.Proposed))
			{
				_inChangingEvent = true;
				try
				{
					_table.ChangingDataRow(this, DataRowAction.Change);
				}
				finally
				{
					_inChangingEvent = false;
				}
				if (rowState == DataRowState.Unchanged)
					rowState = DataRowState.Modified;
				
				//Calling next method validates UniqueConstraints
				//and ForeignKeys.
				try
				{
					if ((_table.DataSet == null || _table.DataSet.EnforceConstraints) && !_table._duringDataLoad)
						_table.Rows.ValidateDataRowInternal(this);
				}
				catch (Exception e)
				{
					editing = false;
					proposed = null;
					throw e;
				}

				// Now we are going to check all child rows of current row.
				// In the case the cascade is true the child rows will look up for
				// parent row. since lookup in index is always on current,
				// we have to move proposed version of current row to current
				// in the case of check child row failure we are rolling 
				// current row state back.
				object[] backup = current;
				current = proposed;
				bool editing_backup = editing;
				editing = false;
				try {
					// check all child rows.
					CheckChildRows(DataRowAction.Change);
					proposed = null;
				}
				catch (Exception ex) {
					// if check child rows failed - rollback to previous state
					// i.e. restore proposed and current versions
					proposed = current;
					current = backup;
					editing = editing_backup;
					// since we failed - propagate an exception
					throw ex;
				}
				_table.ChangedDataRow(this, DataRowAction.Change);
			}
		}

		/// <summary>
		/// Gets the child rows of this DataRow using the specified DataRelation.
		/// </summary>
		public DataRow[] GetChildRows (DataRelation relation) 
		{
			return GetChildRows (relation, DataRowVersion.Current);
		}

		/// <summary>
		/// Gets the child rows of a DataRow using the specified RelationName of a
		/// DataRelation.
		/// </summary>
		public DataRow[] GetChildRows (string relationName) 
		{
			return GetChildRows (Table.DataSet.Relations[relationName]);
		}

		/// <summary>
		/// Gets the child rows of a DataRow using the specified DataRelation, and
		/// DataRowVersion.
		/// </summary>
		public DataRow[] GetChildRows (DataRelation relation, DataRowVersion version) 
		{
			if (relation == null)
				return new DataRow[0];

			if (this.Table == null || RowState == DataRowState.Detached)
				throw new RowNotInTableException("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.");

			if (relation.DataSet != this.Table.DataSet)
				throw new ArgumentException();

			if (relation.ChildKeyConstraint != null)
				return GetChildRows (relation.ChildKeyConstraint, version);

			ArrayList rows = new ArrayList();
			DataColumn[] parentColumns = relation.ParentColumns;
			DataColumn[] childColumns = relation.ChildColumns;
			int numColumn = parentColumns.Length;
			if (HasVersion(version))
			{
				object[] vals = new object[parentColumns.Length];
				for (int i = 0; i < vals.Length; i++)
					vals[i] = this[parentColumns[i], version];
				
				foreach (DataRow row in relation.ChildTable.Rows) 
				{
					bool allColumnsMatch = false;
					if (row.HasVersion(DataRowVersion.Default))
					{
						allColumnsMatch = true;
						for (int columnCnt = 0; columnCnt < numColumn; ++columnCnt) 
						{
							if (!vals[columnCnt].Equals(
								row[childColumns[columnCnt], DataRowVersion.Default])) 
							{
								allColumnsMatch = false;
								break;
							}
						}
					}
					if (allColumnsMatch) rows.Add(row);
				}
			}
			DataRow[] result = relation.ChildTable.NewRowArray(rows.Count);
			rows.CopyTo(result, 0);
			return result;
		}

		/// <summary>
		/// Gets the child rows of a DataRow using the specified RelationName of a
		/// DataRelation, and DataRowVersion.
		/// </summary>
		public DataRow[] GetChildRows (string relationName, DataRowVersion version) 
		{
			return GetChildRows (Table.DataSet.Relations[relationName], version);
		}

		private DataRow[] GetChildRows (ForeignKeyConstraint fkc, DataRowVersion version) 
		{
			ArrayList rows = new ArrayList();
			DataColumn[] parentColumns = fkc.RelatedColumns;
			DataColumn[] childColumns = fkc.Columns;
			int numColumn = parentColumns.Length;
			if (HasVersion(version))
			{
				object[] vals = new object[parentColumns.Length];
				for (int i = 0; i < vals.Length; i++)
					vals[i] = this[parentColumns[i], version];

				Index index = fkc.Index;
				if (index != null) {
					// get the child rows from the index
					Node[] childNodes = index.FindAllSimple (vals);
					for (int i = 0; i < childNodes.Length; i++) {
						rows.Add (childNodes[i].Row);
					}
				}
				else { // if there is no index we search manualy.
					foreach (DataRow row in fkc.Table.Rows) 
					{
						bool allColumnsMatch = false;
						if (row.HasVersion(DataRowVersion.Default))
						{
							allColumnsMatch = true;
							for (int columnCnt = 0; columnCnt < numColumn; ++columnCnt) 
							{
								if (!vals[columnCnt].Equals(
									row[childColumns[columnCnt], DataRowVersion.Default])) 
								{
									allColumnsMatch = false;
									break;
								}
							}
						}
						if (allColumnsMatch) rows.Add(row);
					}
				}
			}

			DataRow[] result = fkc.Table.NewRowArray(rows.Count);
			rows.CopyTo(result, 0);
			return result;
		}

		/// <summary>
		/// Gets the error description of the specified DataColumn.
		/// </summary>
		public string GetColumnError (DataColumn column) 
		{
			return GetColumnError (_table.Columns.IndexOf(column));
		}

		/// <summary>
		/// Gets the error description for the column specified by index.
		/// </summary>
		public string GetColumnError (int columnIndex) 
		{
			if (columnIndex < 0 || columnIndex >= columnErrors.Length)
				throw new IndexOutOfRangeException ();

			string retVal = columnErrors[columnIndex];
			if (retVal == null)
				retVal = string.Empty;
			return retVal;
		}

		/// <summary>
		/// Gets the error description for the column, specified by name.
		/// </summary>
		public string GetColumnError (string columnName) 
		{
			return GetColumnError (_table.Columns.IndexOf(columnName));
		}

		/// <summary>
		/// Gets an array of columns that have errors.
		/// </summary>
		public DataColumn[] GetColumnsInError () 
		{
			ArrayList dataColumns = new ArrayList ();

			for (int i = 0; i < columnErrors.Length; i += 1)
			{
				if (columnErrors[i] != null && columnErrors[i] != String.Empty)
					dataColumns.Add (_table.Columns[i]);
			}

			return (DataColumn[])(dataColumns.ToArray (typeof(DataColumn)));
		}

		/// <summary>
		/// Gets the parent row of a DataRow using the specified DataRelation.
		/// </summary>
		public DataRow GetParentRow (DataRelation relation) 
		{
			return GetParentRow (relation, DataRowVersion.Current);
		}

		/// <summary>
		/// Gets the parent row of a DataRow using the specified RelationName of a
		/// DataRelation.
		/// </summary>
		public DataRow GetParentRow (string relationName) 
		{
			return GetParentRow (relationName, DataRowVersion.Current);
		}

		/// <summary>
		/// Gets the parent row of a DataRow using the specified DataRelation, and
		/// DataRowVersion.
		/// </summary>
		public DataRow GetParentRow (DataRelation relation, DataRowVersion version) 
		{
			DataRow[] rows = GetParentRows(relation, version);
			if (rows.Length == 0) return null;
			return rows[0];
		}

		/// <summary>
		/// Gets the parent row of a DataRow using the specified RelationName of a 
		/// DataRelation, and DataRowVersion.
		/// </summary>
		public DataRow GetParentRow (string relationName, DataRowVersion version) 
		{
			return GetParentRow (Table.DataSet.Relations[relationName], version);
		}

		/// <summary>
		/// Gets the parent rows of a DataRow using the specified DataRelation.
		/// </summary>
		public DataRow[] GetParentRows (DataRelation relation) 
		{
			return GetParentRows (relation, DataRowVersion.Current);
		}

		/// <summary>
		/// Gets the parent rows of a DataRow using the specified RelationName of a 
		/// DataRelation.
		/// </summary>
		public DataRow[] GetParentRows (string relationName) 
		{
			return GetParentRows (relationName, DataRowVersion.Current);
		}

		/// <summary>
		/// Gets the parent rows of a DataRow using the specified DataRelation, and
		/// DataRowVersion.
		/// </summary>
		public DataRow[] GetParentRows (DataRelation relation, DataRowVersion version) 
		{
			// TODO: Caching for better preformance
			if (relation == null)
				return new DataRow[0];

			if (this.Table == null || RowState == DataRowState.Detached)
				throw new RowNotInTableException("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.");

			if (relation.DataSet != this.Table.DataSet)
				throw new ArgumentException();

			ArrayList rows = new ArrayList();
			DataColumn[] parentColumns = relation.ParentColumns;
			DataColumn[] childColumns = relation.ChildColumns;
			int numColumn = parentColumns.Length;
			if (HasVersion(version))
			{
				object[] vals = new object[childColumns.Length];
				for (int i = 0; i < vals.Length; i++)
					vals[i] = this[childColumns[i], version];
				
				Index indx = relation.ParentTable.GetIndexByColumns (parentColumns);
				if (indx != null) { // get the child rows from the index
					Node[] childNodes = indx.FindAllSimple (vals);
					for (int i = 0; i < childNodes.Length; i++) {
						rows.Add (childNodes[i].Row);
					}
				}
				else { // no index so we have to search manualy.
					foreach (DataRow row in relation.ParentTable.Rows) 
					{
						bool allColumnsMatch = false;
						if (row.HasVersion(DataRowVersion.Default))
						{
							allColumnsMatch = true;
							for (int columnCnt = 0; columnCnt < numColumn; columnCnt++) 
							{
								if (!this[childColumns[columnCnt], version].Equals(
									row[parentColumns[columnCnt], DataRowVersion.Default])) 
								{
									allColumnsMatch = false;
									break;
								}
							}
						}
						if (allColumnsMatch) rows.Add(row);
					}
				}
			}

			DataRow[] result = relation.ParentTable.NewRowArray(rows.Count);
			rows.CopyTo(result, 0);
			return result;
		}

		/// <summary>
		/// Gets the parent rows of a DataRow using the specified RelationName of a 
		/// DataRelation, and DataRowVersion.
		/// </summary>
		public DataRow[] GetParentRows (string relationName, DataRowVersion version) 
		{
			return GetParentRows (Table.DataSet.Relations[relationName], version);
		}

		/// <summary>
		/// Gets a value indicating whether a specified version exists.
		/// </summary>
		public bool HasVersion (DataRowVersion version) 
		{
			switch (version)
			{
				case DataRowVersion.Default:
					if (rowState == DataRowState.Deleted)
						return false;
					if (rowState == DataRowState.Detached)
						return proposed != null;
					return true;
				case DataRowVersion.Proposed:
					if (rowState == DataRowState.Deleted)
						return false;
					return (proposed != null);
				case DataRowVersion.Current:
					if (rowState == DataRowState.Deleted || rowState == DataRowState.Detached)
						return false;
					return (current != null);
				case DataRowVersion.Original:
					if (rowState == DataRowState.Detached)
						return false;
					return (original != null);
			}
			return false;
		}

		/// <summary>
		/// Gets a value indicating whether the specified DataColumn contains a null value.
		/// </summary>
		public bool IsNull (DataColumn column) 
		{
			object o = this[column];
			return (o == DBNull.Value);
		}

		/// <summary>
		/// Gets a value indicating whether the column at the specified index contains a null
		/// value.
		/// </summary>
		public bool IsNull (int columnIndex) 
		{
			object o = this[columnIndex];
			return (o == DBNull.Value);
		}

		/// <summary>
		/// Gets a value indicating whether the named column contains a null value.
		/// </summary>
		public bool IsNull (string columnName) 
		{
			object o = this[columnName];
			return (o == DBNull.Value);
		}

		/// <summary>
		/// Gets a value indicating whether the specified DataColumn and DataRowVersion
		/// contains a null value.
		/// </summary>
		public bool IsNull (DataColumn column, DataRowVersion version) 
		{
			object o = this[column, version];
			return (o == DBNull.Value);
		}

		/// <summary>
		/// Returns a value indicating whether all of the row columns specified contain a null value.
		/// </summary>
		internal bool IsNullColumns(DataColumn[] columns)
		{
			bool allNull = true;
			for (int i = 0; i < columns.Length; i++) 
			{
				if (!IsNull(columns[i])) 
				{
					allNull = false;
					break;
				}
			}
			return allNull;
		}

		/// <summary>
		/// Rejects all changes made to the row since AcceptChanges was last called.
		/// </summary>
		public void RejectChanges () 
		{
			if (RowState == DataRowState.Detached)
				throw new RowNotInTableException("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.");
			// If original is null, then nothing has happened since AcceptChanges
			// was last called.  We have no "original" to go back to.
			if (original != null)
			{
				Array.Copy (original, current, _table.Columns.Count);
			       
				_table.ChangedDataRow (this, DataRowAction.Rollback);
				CancelEdit ();
				switch (rowState)
				{
					case DataRowState.Added:
						_table.DeleteRowFromIndexes (this);
						_table.Rows.RemoveInternal (this);
						break;
					case DataRowState.Modified:
						if ((_table.DataSet == null || _table.DataSet.EnforceConstraints) && !_table._duringDataLoad)
							_table.Rows.ValidateDataRowInternal(this);
						rowState = DataRowState.Unchanged;
						break;
					case DataRowState.Deleted:
						rowState = DataRowState.Unchanged;
						if ((_table.DataSet == null || _table.DataSet.EnforceConstraints) && !_table._duringDataLoad)
							_table.Rows.ValidateDataRowInternal(this);
						break;
				} 
				
			} 			
			else {
				// If rows are just loaded via Xml the original values are null.
				// So in this case we have to remove all columns.
				// FIXME: I'm not realy sure, does this break something else, but
				// if so: FIXME ;)
				
				if ((rowState & DataRowState.Added) > 0)
				{
					_table.DeleteRowFromIndexes (this);
					_table.Rows.RemoveInternal (this);
					// if row was in Added state we move it to Detached.
					DetachRow();
				}
			}
		}

		/// <summary>
		/// Sets the error description for a column specified as a DataColumn.
		/// </summary>
		public void SetColumnError (DataColumn column, string error) 
		{
			SetColumnError (_table.Columns.IndexOf (column), error);
		}

		/// <summary>
		/// Sets the error description for a column specified by index.
		/// </summary>
		public void SetColumnError (int columnIndex, string error) 
		{
			if (columnIndex < 0 || columnIndex >= columnErrors.Length)
				throw new IndexOutOfRangeException ();
			columnErrors[columnIndex] = error;
		}

		/// <summary>
		/// Sets the error description for a column specified by name.
		/// </summary>
		public void SetColumnError (string columnName, string error) 
		{
			SetColumnError (_table.Columns.IndexOf (columnName), error);
		}

		/// <summary>
		/// Sets the value of the specified DataColumn to a null value.
		/// </summary>
		protected void SetNull (DataColumn column) 
		{
			this[column] = DBNull.Value;
		}

		/// <summary>
		/// Sets the parent row of a DataRow with specified new parent DataRow.
		/// </summary>
		public void SetParentRow (DataRow parentRow) 
		{
			SetParentRow(parentRow, null);
		}

		/// <summary>
		/// Sets the parent row of a DataRow with specified new parent DataRow and
		/// DataRelation.
		/// </summary>
		public void SetParentRow (DataRow parentRow, DataRelation relation) 
		{
			if (_table == null || parentRow.Table == null || RowState == DataRowState.Detached)
				throw new RowNotInTableException("This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.");

			if (parentRow != null && _table.DataSet != parentRow.Table.DataSet)
				throw new ArgumentException();
			
			BeginEdit();
			if (relation == null)
			{
				foreach (DataRelation parentRel in _table.ParentRelations)
				{
					DataColumn[] childCols = parentRel.ChildKeyConstraint.Columns;
					DataColumn[] parentCols = parentRel.ChildKeyConstraint.RelatedColumns;
					
					for (int i = 0; i < parentCols.Length; i++)
					{
						if (parentRow == null)
							this[childCols[i].Ordinal] = DBNull.Value;
						else
							this[childCols[i].Ordinal] = parentRow[parentCols[i]];
					}
					
				}
			}
			else
			{
				DataColumn[] childCols = relation.ChildKeyConstraint.Columns;
				DataColumn[] parentCols = relation.ChildKeyConstraint.RelatedColumns;
					
				for (int i = 0; i < parentCols.Length; i++)
				{
					if (parentRow == null)
						this[childCols[i].Ordinal] = DBNull.Value;
					else
						this[childCols[i].Ordinal] = parentRow[parentCols[i]];
				}
			}
			EndEdit();
		}
		
		//Copy all values of this DataaRow to the row parameter.
		internal void CopyValuesToRow(DataRow row)
		{
						
			if (row == null)
				throw new ArgumentNullException("row");
			if (row == this)
				throw new ArgumentException("'row' is the same as this object");

			DataColumnCollection columns = Table.Columns;
			
			for(int i = 0; i < columns.Count; i++){

				string columnName = columns[i].ColumnName;
				int index = row.Table.Columns.IndexOf(columnName);
				//if a column with the same name exists in both rows copy the values
				if(index != -1) {
					if (HasVersion(DataRowVersion.Original))
					{
						if (row.original == null)
							row.original = new object[row.Table.Columns.Count];
						row.original[index] = row.SetColumnValue(original[i], index);
					}
					if (HasVersion(DataRowVersion.Current))
					{
						if (row.current == null)
							row.current = new object[row.Table.Columns.Count];
						row.current[index] = row.SetColumnValue(current[i], index);
					}
					if (HasVersion(DataRowVersion.Proposed))
					{
						if (row.proposed == null)
							row.proposed = new object[row.Table.Columns.Count];
						row.proposed[index] = row.SetColumnValue(proposed[i], index);
					}
					
					//Saving the current value as the column value
					row[index] = row.current[index];
					
				}
			}

			row.rowState = RowState;
			row.RowError = RowError;
			row.columnErrors = columnErrors;
		}

		
		public void CollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs args)
		{
			// if a column is added we hava to add an additional value the 
			// the priginal, current and propoed arrays.
			// this scenario can happened in merge operation.

			if (args.Action == System.ComponentModel.CollectionChangeAction.Add)
			{
				object[] tmp;
				if (current != null)
				{
					tmp = new object[current.Length + 1];
					Array.Copy (current, tmp, current.Length);
					tmp[tmp.Length - 1] = SetColumnValue(null, this.Table.Columns.Count - 1);
					current = tmp;
				}
				if (proposed != null)
				{
					tmp = new object[proposed.Length + 1];
					Array.Copy (proposed, tmp, proposed.Length);
					tmp[tmp.Length - 1] = SetColumnValue(null, this.Table.Columns.Count - 1);
					proposed = tmp;
				}
				if(original != null)
				{
					tmp = new object[original.Length + 1];
					Array.Copy (original, tmp, original.Length);
					tmp[tmp.Length - 1] = SetColumnValue(null, this.Table.Columns.Count - 1);
					original = tmp;
				}

			}
		}

		internal bool IsRowChanged(DataRowState rowState) {
			if((RowState & rowState) != 0)
				return true;

			//we need to find if child rows of this row changed.
			//if yes - we should return true

			// if the rowState is deleted we should get the original version of the row
			// else - we should get the current version of the row.
			DataRowVersion version = (rowState == DataRowState.Deleted) ? DataRowVersion.Original : DataRowVersion.Current;
			int count = Table.ChildRelations.Count;
			for (int i = 0; i < count; i++){
				DataRelation rel = Table.ChildRelations[i];
				DataRow[] childRows = GetChildRows(rel, version);
				for (int j = 0; j < childRows.Length; j++){
					if (childRows[j].IsRowChanged(rowState))
						return true;
				}
			}

			return false;
		}

		internal bool HasParentCollection
		{
			get
			{
				return _hasParentCollection;
			}
			set
			{
				_hasParentCollection = value;
			}
		}

		internal void CheckNullConstraints()
		{
			if (_nullConstraintViolation)
			{
				for (int i = 0; i < proposed.Length; i++)
				{
					if (this[i] == DBNull.Value && !_table.Columns[i].AllowDBNull)
						throw new NoNullAllowedException(_nullConstraintMessage);
				}
				_nullConstraintViolation = false;
			}
		}

		#endregion // Methods
	}

	

}
