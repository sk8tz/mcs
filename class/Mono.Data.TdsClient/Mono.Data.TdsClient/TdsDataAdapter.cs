//
// Mono.Data.TdsClient.TdsDataAdapter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace Mono.Data.TdsClient
{
	/// <summary>
	/// Represents a set of command-related properties that are used 
	/// to fill the DataSet and update a data source, all this 
	/// from a SQL database.
	/// </summary>
	public sealed class TdsDataAdapter : DbDataAdapter, IDbDataAdapter 
	{
		#region Fields
	
		TdsCommand deleteCommand;
		TdsCommand insertCommand;
		TdsCommand selectCommand;
		TdsCommand updateCommand;

		static readonly object EventRowUpdated = new object(); 
		static readonly object EventRowUpdating = new object(); 

		#endregion

		#region Constructors
		
		public TdsDataAdapter () 	
			: this (new TdsCommand ())
		{
		}

		public TdsDataAdapter (TdsCommand selectCommand) 
		{
			DeleteCommand = new TdsCommand ();
			InsertCommand = new TdsCommand ();
			SelectCommand = selectCommand;
			UpdateCommand = new TdsCommand ();
		}

		public TdsDataAdapter (string selectCommandText, TdsConnection selectConnection) 
			: this (new TdsCommand (selectCommandText, selectConnection))
		{ 
		}

		public TdsDataAdapter (string selectCommandText, string selectConnectionString)
			: this (selectCommandText, new TdsConnection (selectConnectionString))
		{
		}

		#endregion

		#region Properties

		public TdsCommand DeleteCommand {
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}

		public TdsCommand InsertCommand {
			get { return insertCommand; }
			set { insertCommand = value; }
		}

		public TdsCommand SelectCommand {
			get { return selectCommand; }
			set { selectCommand = value; }
		}

		public TdsCommand UpdateCommand {
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		IDbCommand IDbDataAdapter.DeleteCommand {
			get { return DeleteCommand; }
			set { 
				if (!(value is TdsCommand)) 
					throw new ArgumentException ();
				DeleteCommand = (TdsCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.InsertCommand {
			get { return InsertCommand; }
			set { 
				if (!(value is TdsCommand)) 
					throw new ArgumentException ();
				InsertCommand = (TdsCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.SelectCommand {
			get { return SelectCommand; }
			set { 
				if (!(value is TdsCommand)) 
					throw new ArgumentException ();
				SelectCommand = (TdsCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.UpdateCommand {
			get { return UpdateCommand; }
			set { 
				if (!(value is TdsCommand)) 
					throw new ArgumentException ();
				UpdateCommand = (TdsCommand)value;
			}
		}


		ITableMappingCollection IDataAdapter.TableMappings {
			get { return TableMappings; }
		}

		#endregion // Properties

		#region Methods

		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new TdsRowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new TdsRowUpdatingEventArgs (dataRow, command, statementType, tableMapping);
		}

		protected override void OnRowUpdated (RowUpdatedEventArgs value) 
		{
         		TdsRowUpdatedEventHandler handler = (TdsRowUpdatedEventHandler) Events[EventRowUpdated];
			if ((handler != null) && (value is TdsRowUpdatedEventArgs))
            			handler (this, (TdsRowUpdatedEventArgs) value);
		}

		protected override void OnRowUpdating (RowUpdatingEventArgs value) 
		{
         		TdsRowUpdatingEventHandler handler = (TdsRowUpdatingEventHandler) Events[EventRowUpdating];
			if ((handler != null) && (value is TdsRowUpdatingEventArgs))
            			handler (this, (TdsRowUpdatingEventArgs) value);
		}

		#endregion // Methods

		#region Events and Delegates

		public event TdsRowUpdatedEventHandler RowUpdated {
			add { Events.AddHandler (EventRowUpdated, value); }
			remove { Events.RemoveHandler (EventRowUpdated, value); }
		}

		public event TdsRowUpdatingEventHandler RowUpdating {
			add { Events.AddHandler (EventRowUpdating, value); }
			remove { Events.RemoveHandler (EventRowUpdating, value); }
		}

		#endregion // Events and Delegates

	}
}
