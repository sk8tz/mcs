//
// System.Data.DataRowCollection.cs
//
// Author:
//   Daniel Morgan <danmorg@sc.rr.com>
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Ximian, Inc 2002
// (C) Copyright 2002 Tim Coleman
// (C) Copyright 2002 Daniel Morgan
//

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Data
{
	/// <summary>
	/// Collection of DataRows in a DataTable
	/// </summary>
	[Serializable]
	public class DataRowCollection : InternalDataCollectionBase 
	{
		private DataTable table;

		/// <summary>
		/// Internal constructor used to build a DataRowCollection.
		/// </summary>
		internal DataRowCollection (DataTable table) : base ()
		{
			this.table = table;
		}

		/// <summary>
		/// Gets the row at the specified index.
		/// </summary>
		public DataRow this[int index] 
		{
			get { 
				if (index >= Count)
					throw new IndexOutOfRangeException ("There is no row at position " + index + ".");

				return (DataRow) list[index]; 
			}
		}

		/// <summary>
		/// This member overrides InternalDataCollectionBase.List
		/// </summary>
		protected override ArrayList List 
		{
			get { return list; }
		}		

		/// <summary>
		/// Adds the specified DataRow to the DataRowCollection object.
		/// </summary>
		public void Add (DataRow row) 
		{
			//TODO: validation
			if (row == null)
				throw new ArgumentNullException("row", "'row' argument cannot be null.");

			if (row.Table != this.table)
				throw new ArgumentException ("This row already belongs to another table.");
			
			// If row id is not -1, we know that it is in the collection.
			if (row.RowID != -1)
				throw new ArgumentException ("This row already belongs to this table.");
			

			if ((table.DataSet == null || table.DataSet.EnforceConstraints) && !table._duringDataLoad)
				// we have to check that the new row doesn't colide with existing row
				ValidateDataRowInternal(row);
			
			row.HasParentCollection = true;
			list.Add (row);
			// Set the row id.
			row.RowID = list.Count - 1;
			row.AttachRow ();
			row.Table.ChangedDataRow (row, DataRowAction.Add);
		}

		/// <summary>
		/// Creates a row using specified values and adds it to the DataRowCollection.
		/// </summary>
		public virtual DataRow Add (object[] values) 
		{
			DataRow row = table.NewRow ();
			row.ItemArray = values;
			Add (row);
			return row;
		}

		/// <summary>
		/// Clears the collection of all rows.
		/// </summary>
		public void Clear () 
		{
			if (this.table.DataSet != null)
			{
				foreach (DataTable table in this.table.DataSet.Tables)
				{
					foreach (Constraint c in table.Constraints)
					{
						if (c is ForeignKeyConstraint)
						{
							if (((ForeignKeyConstraint) c).RelatedTable.Equals(this.table))
								throw new InvalidConstraintException("Cannot clear table Parent because ForeignKeyConstraint " + c.ConstraintName + " enforces Child.");
						}
					}
				}
			}
			list.Clear ();
		}

		/// <summary>
		/// Gets a value indicating whether the primary key of any row in the collection contains
		/// the specified value.
		/// </summary>
		public bool Contains (object key) 
		{
			return Find (key) != null;
		}

		/// <summary>
		/// Gets a value indicating whether the primary key column(s) of any row in the 
		/// collection contains the values specified in the object array.
		/// </summary>
		public bool Contains (object[] keys) 
		{
			if (table.PrimaryKey.Length != keys.Length)
				throw new ArgumentException ("Expecting " + table.PrimaryKey.Length + " value(s) for the key " + 
							     "being indexed, but received " + keys.Length + " value(s).");

			return Find (keys) != null;
		}

		/// <summary>
		/// Gets the row specified by the primary key value.
		/// </summary>
		[MonoTODO]
		public DataRow Find (object key) 
		{
			if (table.PrimaryKey.Length == 0)
				throw new MissingPrimaryKeyException ("Table doesn't have a primary key.");
			if (table.PrimaryKey.Length > 1)
				throw new ArgumentException ("Expecting " + table.PrimaryKey.Length + 
							     " value(s) for the key being indexed, but received 1 value(s).");

			string primColumnName = table.PrimaryKey [0].ColumnName;
			Type coltype = null;
			object newKey = null;
			
			foreach (DataRow row in this) {
				
				if (row.RowState != DataRowState.Deleted)
				{
					object primValue = row [primColumnName];
					if (key == null) 
					{
						if (primValue == null)
							return row;
						else 
							continue;
					}
				       
					newKey = Convert.ChangeType (key, Type.GetTypeCode(primValue.GetType ()));

					if (primValue.Equals (newKey))
						return row;
				}
			}
						
			// FIXME: is the correct value null?
			return null;
		}

		/// <summary>
		/// Gets the row containing the specified primary key values.
		/// </summary>
		[MonoTODO]
		public DataRow Find (object[] keys) 
		{
			if (table.PrimaryKey.Length == 0)
				throw new MissingPrimaryKeyException ("Table doesn't have a primary key.");

			string  [] primColumnNames = new string [table.PrimaryKey.Length];
			
			for (int i = 0; i < primColumnNames.Length; i++)
				primColumnNames [i] = table.PrimaryKey [i].ColumnName;

			Type coltype = null;
			object newKey = null;
			
			foreach (DataRow row in this) {
				
				if (row.RowState != DataRowState.Deleted)
				{
					bool eq = true;
					for (int i = 0; i < keys.Length; i++) 
					{
					
						object primValue = row [primColumnNames [i]];
						object keyValue = keys [i];
						if (keyValue == null) 
						{
							if (primValue == null)
								return row;
							else 
								continue;
						}
								       
						newKey = Convert.ChangeType (keyValue, Type.GetTypeCode(primValue.GetType ()));

						if (!primValue.Equals (newKey)) 
						{
							eq = false;
							break;
						}						
					}

					if (eq)
						return row;
				}
			}
						
			// FIXME: is the correct value null?
			return null;
		}

		/// <summary>
		/// Inserts a new row into the collection at the specified location.
		/// </summary>
		public void InsertAt (DataRow row, int pos) 
		{
			if (pos < 0)
				throw new IndexOutOfRangeException ("The row insert position " + pos + " is invalid.");
			
			if (row == null)
				throw new ArgumentNullException("row", "'row' argument cannot be null.");
	
			if (row.Table != this.table)
				throw new ArgumentException ("This row already belongs to another table.");

			// If row id is not -1, we know that it is in the collection.
			if (row.RowID != -1)
				throw new ArgumentException ("This row already belongs to this table.");
			
			if ((table.DataSet == null || table.DataSet.EnforceConstraints) && !table._duringDataLoad)
				// we have to check that the new row doesn't colide with existing row
				ValidateDataRowInternal(row);
				
			if (pos >= list.Count)
				list.Add (row);
			else
				list.Insert (pos, row);
				
			row.HasParentCollection = true;
			row.AttachRow ();
			row.Table.ChangedDataRow (row, DataRowAction.Add);
		}

		/// <summary>
		/// Removes the specified DataRow from the internal list. Used by DataRow to commit the removing.
		/// </summary>
		internal void RemoveInternal (DataRow row) {
			if (row == null) {
				throw new IndexOutOfRangeException ("The given datarow is not in the current DataRowCollection.");
			}
			int index = list.IndexOf(row);
			if (index < 0) {
				throw new IndexOutOfRangeException ("The given datarow is not in the current DataRowCollection.");
			}
			list.RemoveAt(index);
		}

		/// <summary>
		/// Removes the specified DataRow from the collection.
		/// </summary>
		public void Remove (DataRow row) 
		{
			if (row == null)
				throw new IndexOutOfRangeException ("The given datarow is not in the current DataRowCollection.");
			int index = list.IndexOf(row);
			if (index < 0)
				throw new IndexOutOfRangeException ("The given datarow is not in the current DataRowCollection.");
			row.Delete();
			// if the row was in added state it will be in Detached state after the
			// delete operation, so we have to check it.
			if (row.RowState != DataRowState.Detached)
				row.AcceptChanges();
		}

		/// <summary>
		/// Removes the row at the specified index from the collection.
		/// </summary>
		public void RemoveAt (int index) 
		{			
			if (index < 0 || index >= list.Count)
				throw new IndexOutOfRangeException ("There is no row at position " + index + ".");
			DataRow row = (DataRow)list [index];
			row.Delete();
			row.AcceptChanges();
		}

		///<summary>
		///Internal method used to validate a given DataRow with respect
		///to the DataRowCollection
		///</summary>
		[MonoTODO]
		internal void ValidateDataRowInternal(DataRow row)
		{
			//first check for null violations.
			row.CheckNullConstraints();
			// This validates constraints in the specific order : 
			// first unique/primary keys first, then Foreignkeys, etc
			ArrayList uniqueConstraintsDone = new ArrayList();
			ArrayList foreignKeyConstraintsDone = new ArrayList();
			try {
				foreach(Constraint constraint in table.Constraints.UniqueConstraints) {
					constraint.AssertConstraint(row);
					uniqueConstraintsDone.Add(constraint);
				}
			
				foreach(Constraint constraint in table.Constraints.ForeignKeyConstraints) {
					constraint.AssertConstraint(row);
					foreignKeyConstraintsDone.Add(constraint);
				}
			}
			// if one of the AssertConstraint failed - we need to "rollback" all the changes
			// caused by AssertCoinstraint calls already succeeded
			catch(ConstraintException e) {
				RollbackAsserts(row,foreignKeyConstraintsDone,uniqueConstraintsDone);
				throw e;
			}
			catch(InvalidConstraintException e) {	
				RollbackAsserts(row,foreignKeyConstraintsDone,uniqueConstraintsDone);
				throw e;
			}
		}

		private void RollbackAsserts(DataRow row,ICollection foreignKeyConstraintsDone,
			ICollection uniqueConstraintsDone)
		{
			// if any of constraints assert failed - 
			// we have to rollback all the asserts scceeded
			// on order reverse to thier original execution
			foreach(Constraint constraint in foreignKeyConstraintsDone) {
				constraint.RollbackAssert(row);
			}

			foreach(Constraint constraint in uniqueConstraintsDone) {
				constraint.RollbackAssert(row);
			}
		}
	}
}
