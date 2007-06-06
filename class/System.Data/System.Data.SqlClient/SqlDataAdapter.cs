//
// System.Data.SqlClient.SqlDataAdapter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) 2002 Tim Coleman
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
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.SqlClient {
	[DefaultEvent ("RowUpdated")]
	[DesignerAttribute ("Microsoft.VSDesigner.Data.VS.SqlDataAdapterDesigner, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.IDesigner")]
	[ToolboxItemAttribute ("Microsoft.VSDesigner.Data.VS.SqlDataAdapterToolboxItem, "+ Consts.AssemblyMicrosoft_VSDesigner)] 
	public sealed class SqlDataAdapter : DbDataAdapter, IDbDataAdapter 
	{
		#region Fields

		bool disposed = false;	
		SqlCommand deleteCommand;
		SqlCommand insertCommand;
		SqlCommand selectCommand;
		SqlCommand updateCommand;
#if NET_2_0
		int updateBatchSize;
#endif
		#endregion

		#region Constructors
		
		public SqlDataAdapter () 	
			: this (new SqlCommand ())
		{
		}

		public SqlDataAdapter (SqlCommand selectCommand) 
		{
			DeleteCommand = null;
			InsertCommand = null;
			SelectCommand = selectCommand;
			UpdateCommand = null;
#if NET_2_0
			UpdateBatchSize = 1;
#endif
		}

		public SqlDataAdapter (string selectCommandText, SqlConnection selectConnection) 
			: this (new SqlCommand (selectCommandText, selectConnection))
		{ 
		}

		public SqlDataAdapter (string selectCommandText, string selectConnectionString)
			: this (selectCommandText, new SqlConnection (selectConnectionString))
		{
		}

		#endregion

		#region Properties

#if !NET_2_0
		[DataSysDescription ("Used during Update for deleted rows in DataSet.")]
#endif
		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public new SqlCommand DeleteCommand {
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}

#if !NET_2_0
		[DataSysDescription ("Used during Update for new rows in DataSet.")]
#endif
		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public new SqlCommand InsertCommand {
			get { return insertCommand; }
			set { insertCommand = value; }
		}

#if !NET_2_0
		[DataSysDescription ("Used during Fill/FillSchema.")]
#endif
		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public new SqlCommand SelectCommand {
			get { return selectCommand; }
			set { selectCommand = value; }
		}

#if !NET_2_0
		[DataSysDescription ("Used during Update for modified rows in DataSet.")]
#endif
		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBCommandEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public new SqlCommand UpdateCommand {
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		IDbCommand IDbDataAdapter.DeleteCommand {
			get { return DeleteCommand; }
			set { 
				if (!(value is SqlCommand)) 
					throw new ArgumentException ();
				DeleteCommand = (SqlCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.InsertCommand {
			get { return InsertCommand; }
			set { 
				if (!(value is SqlCommand)) 
					throw new ArgumentException ();
				InsertCommand = (SqlCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.SelectCommand {
			get { return SelectCommand; }
			set {
				if (!(value is SqlCommand)) 
					throw new ArgumentException ();
				SelectCommand = (SqlCommand)value;
			}
		}

		IDbCommand IDbDataAdapter.UpdateCommand {
			get { return UpdateCommand; }
			set { 
				if (!(value is SqlCommand)) 
					throw new ArgumentException ();
				UpdateCommand = (SqlCommand)value;
			}
		}


		ITableMappingCollection IDataAdapter.TableMappings {
			get { return TableMappings; }
		}

#if NET_2_0
		public override int UpdateBatchSize { 
			get { return updateBatchSize; }
			set { 
				if (value < 0)
					throw new ArgumentOutOfRangeException ();				
				updateBatchSize = value; 
			}
		}
#endif

		#endregion // Properties

		#region Methods

		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new SqlRowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new SqlRowUpdatingEventArgs (dataRow, command, statementType, tableMapping);
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					// Release managed resources
				}
				// Release unmanaged resources
				disposed = true;
			}
		}

		protected override void OnRowUpdated (RowUpdatedEventArgs value) 
		{
			if (RowUpdated != null)
				RowUpdated (this, (SqlRowUpdatedEventArgs) value);
		}

		protected override void OnRowUpdating (RowUpdatingEventArgs value) 
		{
			if (RowUpdating != null)
				RowUpdating (this, (SqlRowUpdatingEventArgs) value);
		}

		#endregion // Methods

		#region Events and Delegates

#if !NET_2_0
		[DataSysDescription ("Event triggered before every DataRow during Update.")]
#endif
		public event SqlRowUpdatedEventHandler RowUpdated;

#if !NET_2_0
		[DataSysDescription ("Event triggered after every DataRow during Update.")]
#endif
		public event SqlRowUpdatingEventHandler RowUpdating;

		#endregion // Events and Delegates

	}
}
