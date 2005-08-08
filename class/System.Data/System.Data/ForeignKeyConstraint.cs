//
// System.Data.ForeignKeyConstraint.cs
//
// Author:
//   Franklin Wise <gracenote@earthlink.net>
//   Daniel Morgan <danmorg@sc.rr.com>
//
// (C) 2002 Franklin Wise
// (C) 2002 Daniel Morgan
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

using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Data.Common;

namespace System.Data {
	[Editor ("Microsoft.VSDesigner.Data.Design.ForeignKeyConstraintEditor, " + Consts.AssemblyMicrosoft_VSDesigner,
		 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	[DefaultProperty ("ConstraintName")]
	[Serializable]
	public class ForeignKeyConstraint : Constraint 
	{
		private UniqueConstraint _parentUniqueConstraint;
		//FIXME: create a class which will wrap this collection
		private DataColumn [] _parentColumns;
		//FIXME: create a class which will wrap this collection
		private DataColumn [] _childColumns;
		private Rule _deleteRule = Rule.Cascade;
		private Rule _updateRule = Rule.Cascade;
		private AcceptRejectRule _acceptRejectRule = AcceptRejectRule.None;
	    private string _parentTableName;
        private string _childTableName;
		//FIXME: remove those; and use only DataColumns[]
        private string [] _parentColumnNames;
        private string [] _childColumnNames;
        private bool _dataColsNotValidated = false;
			
		#region Constructors

		public ForeignKeyConstraint(DataColumn parentColumn, DataColumn childColumn) 
		{
			if (null == parentColumn || null == childColumn) {
				throw new NullReferenceException("Neither parentColumn or" +
					" childColumn can be null.");
			}
			_foreignKeyConstraint(null, new DataColumn[] {parentColumn},
					new DataColumn[] {childColumn});
		}

		public ForeignKeyConstraint(DataColumn[] parentColumns, DataColumn[] childColumns) 
		{
			_foreignKeyConstraint(null, parentColumns, childColumns);
		}

		public ForeignKeyConstraint(string constraintName, DataColumn parentColumn, DataColumn childColumn) 
		{
			if (null == parentColumn || null == childColumn) {
				throw new NullReferenceException("Neither parentColumn or" +
					" childColumn can be null.");
			}

			_foreignKeyConstraint(constraintName, new DataColumn[] {parentColumn},
					new DataColumn[] {childColumn});
		}

		public ForeignKeyConstraint(string constraintName, DataColumn[] parentColumns, DataColumn[] childColumns) 
		{
			_foreignKeyConstraint(constraintName, parentColumns, childColumns);
		}
		
		//special case
		[Browsable (false)]
		public ForeignKeyConstraint(string constraintName, string parentTableName, string[] parentColumnNames, string[] childColumnNames, AcceptRejectRule acceptRejectRule, Rule deleteRule, Rule updateRule) 
		{
			_dataColsNotValidated = true;
            base.ConstraintName = constraintName;
                                                                                        
            // "parentTableName" is searched in the "DataSet" to which the "DataTable"
            // from which AddRange() is called
            // childTable is the "DataTable" which calls AddRange()
                                                                                        
            // Keep reference to parentTableName to resolve later
            _parentTableName = parentTableName;
                                                                                        
            // Keep reference to parentColumnNames to resolve later
            _parentColumnNames = parentColumnNames;
                                                                                        
            // Keep reference to childColumnNames to resolve later
            _childColumnNames = childColumnNames;
                                                                                        
            _acceptRejectRule = acceptRejectRule;
            _deleteRule = deleteRule;
            _updateRule = updateRule;

		}

		internal void postAddRange (DataTable childTable)
        {
            // LAMESPEC - Does not say that this is mandatory
            // Check whether childTable belongs to a DataSet
            if (childTable.DataSet == null)
                    throw new InvalidConstraintException ("ChildTable : " + childTable.TableName + " does not belong to any DataSet");
            DataSet dataSet = childTable.DataSet;
            _childTableName = childTable.TableName;
            // Search for the parentTable in the childTable's DataSet
            if (!dataSet.Tables.Contains (_parentTableName))
                    throw new InvalidConstraintException ("Table : " + _parentTableName + "does not exist in DataSet : " + dataSet);
                                                                                        
            // Keep reference to parentTable
            DataTable parentTable = dataSet.Tables [_parentTableName];
                                                                                        
            int i = 0, j = 0;
                                                                                        
            // LAMESPEC - Does not say which Exception is thrown
            if (_parentColumnNames.Length < 0 || _childColumnNames.Length < 0)
                    throw new InvalidConstraintException ("Neither parent nor child columns can be zero length");
            // LAMESPEC - Does not say which Exception is thrown
            if (_parentColumnNames.Length != _childColumnNames.Length)
			        throw new InvalidConstraintException ("Both parent and child columns must be of same length");                                                                                                    
            DataColumn []parentColumns = new DataColumn [_parentColumnNames.Length];
            DataColumn []childColumns = new DataColumn [_childColumnNames.Length];
                                                                                        
            // Search for the parentColumns in parentTable
            foreach (string parentCol in _parentColumnNames){
                    if (!parentTable.Columns.Contains (parentCol))
                            throw new InvalidConstraintException ("Table : " + _parentTableName + "does not contain the column :" + parentCol);
                    parentColumns [i++] = parentTable. Columns [parentCol];
            }
            // Search for the childColumns in childTable
            foreach (string childCol in _childColumnNames){
                    if (!childTable.Columns.Contains (childCol))
                            throw new InvalidConstraintException ("Table : " + _childTableName + "does not contain the column : " + childCol);
                    childColumns [j++] = childTable.Columns [childCol];
            }
            _validateColumns (parentColumns, childColumns);
            _parentColumns = parentColumns;
            _childColumns = childColumns;
		}
			
#if NET_2_0
		[MonoTODO]
		public ForeignKeyConstraint (string constraintName, string parentTableName, string parentTableNamespace, string[] parentColumnNames, string[] childColumnNames, AcceptRejectRule acceptRejectRule, Rule deleteRule, Rule updateRule)
		{
			throw new NotImplementedException ();
		}
#endif

		private void _foreignKeyConstraint(string constraintName, DataColumn[] parentColumns,
				DataColumn[] childColumns)
		{

			//Validate 
			_validateColumns(parentColumns, childColumns);

			//Set Constraint Name
			base.ConstraintName = constraintName;	

			//Keep reference to columns
			_parentColumns = parentColumns;
			_childColumns = childColumns;
		}

		#endregion // Constructors

		#region Helpers

		private void _validateColumns(DataColumn[] parentColumns, DataColumn[] childColumns)
		{
			//not null
			if (null == parentColumns || null == childColumns) 
				throw new ArgumentNullException();
			
			//at least one element in each array
			if (parentColumns.Length < 1 || childColumns.Length < 1)
				throw new ArgumentException("Neither ParentColumns or ChildColumns can't be" +
						" zero length.");
				
			//same size arrays
			if (parentColumns.Length != childColumns.Length)
				throw new ArgumentException("Parent columns and child columns must be the same length.");
			

			DataTable ptable = parentColumns[0].Table;
			DataTable ctable = childColumns[0].Table;

			for (int i = 0; i < parentColumns.Length; i++) {
				DataColumn pc = parentColumns[i];
				DataColumn cc = childColumns[i];

				//not null check
				if (null == pc.Table) 
					throw new ArgumentException("All columns must belong to a table." + 
						" ColumnName: " + pc.ColumnName + " does not belong to a table.");
				
				//All columns must belong to the same table
				if (ptable != pc.Table)
					throw new InvalidConstraintException("Parent columns must all belong to the same table.");

				//not null check
				if (null == cc.Table) 
					throw new ArgumentException("All columns must belong to a table." + 
						" ColumnName: " + pc.ColumnName + " does not belong to a table.");

				//All columns must belong to the same table.
				if (ctable != cc.Table)
					throw new InvalidConstraintException("Child columns must all belong to the same table.");

				if (pc.CompiledExpression != null)
					throw new ArgumentException(String.Format("Cannot create a constraint based on Expression column {0}.", pc.ColumnName));

				if (cc.CompiledExpression != null)
					throw new ArgumentException(String.Format("Cannot create a constraint based on Expression column {0}.", cc.ColumnName));
				
			}

                        //Same dataset.  If both are null it's ok
			if (ptable.DataSet != ctable.DataSet)
			{
				//LAMESPEC: spec says InvalidConstraintExceptoin
				//	impl does InvalidOperationException
				throw new InvalidOperationException("Parent column and child column must belong to" + 
						" tables that belong to the same DataSet.");
						
			}	


			for (int i = 0; i < parentColumns.Length; i++)
			{
				DataColumn pc = parentColumns[i];
				DataColumn cc = childColumns[i];
				
				//Can't be the same column
				if (pc == cc)
					throw new InvalidOperationException("Parent and child columns can't be the same column.");

				if (! pc.DataType.Equals(cc.DataType))
				{
					//LAMESPEC: spec says throw InvalidConstraintException
					//		implementation throws InvalidOperationException
					throw new InvalidConstraintException("Parent column is not type compatible with it's child"
						+ " column.");
				}
					
			}
			
		}
		


		private void _validateRemoveParentConstraint(ConstraintCollection sender, 
				Constraint constraint, ref bool cancel, ref string failReason)
		{
#if !NET_1_1
			//if we hold a reference to the parent then cancel it
			if (constraint == _parentUniqueConstraint) 
			{
				cancel = true;
				failReason = "Cannot remove UniqueConstraint because the"
					+ " ForeignKeyConstraint " + this.ConstraintName + " exists.";
			}
#endif
		}
		
		//Checks to see if a related unique constraint exists
		//if it doesn't then a unique constraint is created.
		//if a unique constraint can't be created an exception will be thrown
		private void _ensureUniqueConstraintExists(ConstraintCollection collection,
				DataColumn [] parentColumns)
		{
			//not null
			if (null == parentColumns) throw new ArgumentNullException(
					"ParentColumns can't be null");

			UniqueConstraint uc = null;
			
			//see if unique constraint already exists
			//if not create unique constraint
			if(parentColumns[0] != null)
				uc = UniqueConstraint.GetUniqueConstraintForColumnSet(parentColumns[0].Table.Constraints, parentColumns);

			if (null == uc)	{
				uc = new UniqueConstraint(parentColumns, false); //could throw
				parentColumns [0].Table.Constraints.Add (uc);
			}

			//keep reference
			_parentUniqueConstraint = uc;
			//parentColumns [0].Table.Constraints.Add (uc);
			//if this unique constraint is attempted to be removed before us
			//we can fail the validation
			//collection.ValidateRemoveConstraint += new DelegateValidateRemoveConstraint(
			//		_validateRemoveParentConstraint);
		}
		
		
		#endregion //Helpers
		
		#region Properties

		[DataCategory ("Data")]
		[DataSysDescription ("For accept and reject changes, indicates what kind of cascading should take place across this relation.")]
		[DefaultValue (AcceptRejectRule.None)]
		public virtual AcceptRejectRule AcceptRejectRule {
			get { return _acceptRejectRule; }
			set { _acceptRejectRule = value; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the child columns of this constraint.")]
		[ReadOnly (true)]
		public virtual DataColumn[] Columns {
			get { return _childColumns; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("For deletions, indicates what kind of cascading should take place across this relation.")]
		[DefaultValue (Rule.Cascade)]
		public virtual Rule DeleteRule {
			get { return _deleteRule; }
			set { _deleteRule = value; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("For updates, indicates what kind of cascading should take place across this relation.")]
		[DefaultValue (Rule.Cascade)]
		public virtual Rule UpdateRule {
			get { return _updateRule; }
			set { _updateRule = value; }
		}

		[DataCategory ("Data")]	
		[DataSysDescription ("Indicates the parent columns of this constraint.")]
		[ReadOnly (true)]
		public virtual DataColumn[] RelatedColumns {
			get { return _parentColumns; }
		}

		[DataCategory ("Data")]	
		[DataSysDescription ("Indicates the child table of this constraint.")]
		[ReadOnly (true)]
		public virtual DataTable RelatedTable {
			get {
				if (_parentColumns != null)
					if (_parentColumns.Length > 0)
						return _parentColumns[0].Table;

				throw new InvalidOperationException ("Property not accessible because 'Object reference not set to an instance of an object'");
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the table of this constraint.")]
		[ReadOnly (true)]
		public override DataTable Table {
			get {
				if (_childColumns != null)
					if (_childColumns.Length > 0)
						return _childColumns[0].Table;

				throw new InvalidOperationException ("Property not accessible because 'Object reference not set to an instance of an object'");
			}
		}

		internal bool DataColsNotValidated
		{
            get { 
				return (_dataColsNotValidated); 
			}
        }


		#endregion // Properties

		#region Methods

		public override bool Equals(object key) 
		{
			ForeignKeyConstraint fkc = key as ForeignKeyConstraint;
			if (null == fkc) return false;

			//if the fk constrains the same columns then they are equal
			if (! DataColumn.AreColumnSetsTheSame( this.RelatedColumns, fkc.RelatedColumns))
				return false;
			if (! DataColumn.AreColumnSetsTheSame( this.Columns, fkc.Columns) )
				return false;

			return true;
		}

		public override int GetHashCode()
		{
			//initialize hash1 and hash2 with default hashes
			//any two DIFFERENT numbers will do here
			int hash1 = 32, hash2 = 88;
			int i;

			//derive the hash code from the columns that way
			//Equals and GetHashCode return Equal objects to be the
			//same

			//Get the first parent column hash
			if (this.Columns.Length > 0)
				hash1 ^= this.Columns[0].GetHashCode();
			
			//get the rest of the parent column hashes if there any
			for (i = 1; i < this.Columns.Length; i++)
			{
				hash1 ^= this.Columns[1].GetHashCode();
				
			}
			
			//Get the child column hash
			if (this.RelatedColumns.Length > 0)
				hash2 ^= this.Columns[0].GetHashCode();
			
			for (i = 1; i < this.RelatedColumns.Length; i++)
			{
				hash2 ^= this.RelatedColumns[1].GetHashCode();
			}

			//combine the two hashes
			return hash1 ^ hash2;
		}

		internal override void AddToConstraintCollectionSetup(
				ConstraintCollection collection)
		{			
			if (collection.Table != Table)
				throw new InvalidConstraintException("This constraint cannot be added since ForeignKey doesn't belong to table " + RelatedTable.TableName + ".");

			//run Ctor rules again
			_validateColumns(_parentColumns, _childColumns);
			
			//we must have a unique constraint on the parent
			_ensureUniqueConstraintExists(collection, _parentColumns);
			
			//Make sure we can create this thing
			//AssertConstraint(); 
			if ( (Table.DataSet != null && Table.DataSet.EnforceConstraints)
			     || (Table.DataSet == null && Table.EnforceConstraints)) {
				if (IsConstraintViolated())
					throw new ArgumentException("This constraint cannot be enabled as not all values have corresponding parent values.");
			}
			//FIXME : if this fails and we created a unique constraint
			//we should probably roll it back
			// and remove index form Table			
		}
					
		internal override void RemoveFromConstraintCollectionCleanup( 
				ConstraintCollection collection)
		{
			Index = null;
		}
		
		internal override bool IsConstraintViolated()
		{
			if (Table.DataSet == null || RelatedTable.DataSet == null) 
				return false;
			
			bool hasErrors = false;
			foreach (DataRow row in Table.Rows) {
				// first we check if all values in _childColumns place are nulls.
				// if yes we return.
				if (row.IsNullColumns(_childColumns))
					continue;

				// check whenever there is (at least one) parent row  in RelatedTable
				if(!RelatedTable.RowsExist(_parentColumns,_childColumns,row)) {	
					// if no parent row exists - constraint is violated
					hasErrors = true;
					string[] values = new string[_childColumns.Length];
					for (int i = 0; i < _childColumns.Length; i++){
						DataColumn col = _childColumns[i];
						values[i] = row[col].ToString();
					}

					row.RowError = String.Format("ForeignKeyConstraint {0} requires the child key values ({1}) to exist in the parent table.",
						ConstraintName, String.Join(",", values));
				}
			}

			if (hasErrors)
				//throw new ConstraintException("Failed to enable constraints. One or more rows contain values violating non-null, unique, or foreign-key constraints.");
				return true;

			return false;
		}
		
		internal override void AssertConstraint(DataRow row)
		{
			// first we check if all values in _childColumns place are nulls.
			// if yes we return.
			if (row.IsNullColumns(_childColumns))
				return;

			// check whenever there is (at least one) parent row  in RelatedTable
			if(!RelatedTable.RowsExist(_parentColumns,_childColumns,row)) {	
				// if no parent row exists - constraint is violated
				throw new InvalidConstraintException(GetErrorMessage(row));
			}
		}

		internal override bool IsColumnContained(DataColumn column)
		{
			for (int i = 0; i < _parentColumns.Length; i++)
				if (column == _parentColumns[i])
					return true;

			for (int i = 0; i < _childColumns.Length; i++)
				if (column == _childColumns[i])
					return true;

			return false;
		}

		internal override bool CanRemoveFromCollection(ConstraintCollection col, bool shouldThrow){
			return true;
		}

		private string GetErrorMessage(DataRow row)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			for (int i = 0; i < _childColumns.Length; i++) {
				sb.Append(row[_childColumns[0]].ToString());
				if (i != _childColumns.Length - 1) {
					sb.Append(',');
				}
			}
			string valStr = sb.ToString();
			return "ForeignKeyConstraint " + ConstraintName + " requires the child key values (" + valStr + ") to exist in the parent table.";
		}		
                
		#endregion // Methods
	}

}
