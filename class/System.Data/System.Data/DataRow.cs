//
// System.Data.DataRow.cs
//
// Author:
//   Rodrigo Moya <rodrigo@ximian.com>
//   Daniel Morgan <danmorg@sc.rr.com>
//
// (C) Ximian, Inc
//

namespace System.Data
{
	/// <summary>
	/// Represents a row of data in a DataTable.
	/// </summary>
	public class DataRow
	{
		#region Fields

		private ArrayList columns = new ArrayList();
		private ArrayList columnNames = new ArrayList();

		#endregion

		#region Methods

		[MonoTODO]
		public void AcceptChanges() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void BeginEdit() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CancelEdit() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ClearErrors() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Delete() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EndEdit() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow[] GetChildRows(DataRelation dr) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow[] GetChildRows(string s) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow[] GetChildRows(DataRelation dr, DataRowVersion version) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow[] GetChildRows(string s, DataRowVersion version) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetColumnError(DataColumn col) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetColumnError(int index) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetColumnError(string s) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataColumn[] GetColumnsInError() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow GetParentRow(DataRelation dr) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow GetParentRow(string s) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow GetParentRow(DataRelation dr, DataRowVersion version) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow GetParentRow(string s, DataRowVersion version) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow[] GetParentRows(DataRelation dr) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow[] GetParentRows(string s) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow[] GetParentRows(DataRelation dr, DataRowVersion version) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow[] GetParentRows(string s, DataRowVersion version) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool HasVersion(DataRowVersion version) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsNull(DataColumn dc) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsNull(int i) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsNull(string s) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsNull(DataColumn dc, DataRowVersion version) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RejectChanges() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetColumnError(DataColumn dc, string err) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetColumnError(int i, string err) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetColumnError(string a, string err) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetParentRow(DataRow row) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetParentRow(DataRow row, DataRelation rel) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void SetNull(DataColumn column) {
			throw new NotImplementedException ();
		}
		
		#endregion // Methods

		#region Properties

		[MonoTODO]
		public bool HasErrors {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public object this[string s] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public object this[DataColumn dc] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public object this[int i] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public object this[string s, DataRowVersion version] {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public object this[DataColumn dc, DataRowVersion version] {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public object this[int i, DataRowVersion version] {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public object[] ItemArray {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string RowError {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DataRowState RowState {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public DataTable Table {
			get { throw new NotImplementedException (); }
		}

		#endregion // Public Properties

		internal void SetTable(DataTable table) {
			this.table = table; 
			// FIXME: called by DataTable
		}
	}
}
