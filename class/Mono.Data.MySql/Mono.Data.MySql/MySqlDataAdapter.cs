//
// Mono.Data.MySql.MySqlDataAdapter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Daniel Morgan <danmorg@sc.rr.com>
//
// Copyright (C) Tim Coleman, 2002
// Copyright (C) Daniel Morgan, 2002
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

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace Mono.Data.MySql
{
	/// <summary>
	/// Represents a set of command-related properties that are used 
	/// to fill the DataSet and update a data source, all this 
	/// from a SQL database.
	/// </summary>
	public sealed class MySqlDataAdapter : DbDataAdapter, IDbDataAdapter 
	{
		#region Fields
	
		MySqlCommand deleteCommand;
		MySqlCommand insertCommand;
		MySqlCommand selectCommand;
		MySqlCommand updateCommand;

		static readonly object EventRowUpdated = new object(); 
		static readonly object EventRowUpdating = new object(); 

		#endregion

		#region Constructors
		
		public MySqlDataAdapter () 	
			: this (new MySqlCommand ())
		{
		}

		public MySqlDataAdapter (MySqlCommand selectCommand) 
		{
			DeleteCommand = new MySqlCommand ();
			InsertCommand = new MySqlCommand ();
			SelectCommand = selectCommand;
			UpdateCommand = new MySqlCommand ();
		}

		public MySqlDataAdapter (string selectCommandText, MySqlConnection selectConnection) 
			: this (new MySqlCommand (selectCommandText, selectConnection))
		{ 
		}

		public MySqlDataAdapter (string selectCommandText, string selectConnectionString)
			: this (selectCommandText, new MySqlConnection (selectConnectionString))
		{
		}

		#endregion

		#region Properties

		public MySqlCommand DeleteCommand {
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}

		public MySqlCommand InsertCommand {
			get { return insertCommand; }
			set { insertCommand = value; }
		}

		public MySqlCommand SelectCommand {
			get { return selectCommand; }
			set { selectCommand = value; }
		}

		public MySqlCommand UpdateCommand {
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		IDbCommand IDbDataAdapter.DeleteCommand {
			get { return DeleteCommand; }
			set { 
				if (!(value is MySqlCommand)) 
					throw new ArgumentException ();
				DeleteCommand = (MySqlCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.InsertCommand {
			get { return InsertCommand; }
			set { 
				if (!(value is MySqlCommand)) 
					throw new ArgumentException ();
				InsertCommand = (MySqlCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.SelectCommand {
			get { return SelectCommand; }
			set { 
				if (!(value is MySqlCommand)) 
					throw new ArgumentException ();
				SelectCommand = (MySqlCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.UpdateCommand {
			get { return UpdateCommand; }
			set { 
				if (!(value is MySqlCommand)) 
					throw new ArgumentException ();
				UpdateCommand = (MySqlCommand)value;
			}
		}


		ITableMappingCollection IDataAdapter.TableMappings {
			get { return TableMappings; }
		}

		#endregion // Properties

		#region Methods

		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new MySqlRowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new MySqlRowUpdatingEventArgs (dataRow, command, statementType, tableMapping);
		}

		protected override void OnRowUpdated (RowUpdatedEventArgs value) 
		{
         		MySqlRowUpdatedEventHandler handler = (MySqlRowUpdatedEventHandler) Events[EventRowUpdated];
			if ((handler != null) && (value is MySqlRowUpdatedEventArgs))
            			handler (this, (MySqlRowUpdatedEventArgs) value);
		}

		protected override void OnRowUpdating (RowUpdatingEventArgs value) 
		{
         		MySqlRowUpdatingEventHandler handler = (MySqlRowUpdatingEventHandler) Events[EventRowUpdating];
			if ((handler != null) && (value is MySqlRowUpdatingEventArgs))
            			handler (this, (MySqlRowUpdatingEventArgs) value);
		}

		#endregion // Methods

		#region Events and Delegates

		public event MySqlRowUpdatedEventHandler RowUpdated {
			add { Events.AddHandler (EventRowUpdated, value); }
			remove { Events.RemoveHandler (EventRowUpdated, value); }
		}

		public event MySqlRowUpdatingEventHandler RowUpdating {
			add { Events.AddHandler (EventRowUpdating, value); }
			remove { Events.RemoveHandler (EventRowUpdating, value); }
		}

		#endregion // Events and Delegates

	}
}
