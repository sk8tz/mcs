//
// System.Data.Common.FieldNameLookup.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;
using System.Data;

namespace System.Data.Common {
#if NET_2_0
// FIXME: Need to clean this up
	public sealed class FieldNameLookup
#else
	internal sealed class FieldNameLookup : ICollection, IEnumerable
#endif
	{
		#region Fields

		ArrayList list;

		#endregion
		
		#region Constructors

		public FieldNameLookup ()
		{
			list = new ArrayList ();
		}

		public FieldNameLookup (DataTable schemaTable)
			: this ()
		{
			foreach (DataRow row in schemaTable.Rows)
				list.Add ((string) row["ColumnName"]);
		}

		#endregion

		#region Properties

		public int Count {
			get { return list.Count; }
		}

		public bool IsFixedSize {
			get { return false; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		public string this [int index] {
			get { return (string) list[index]; }
			set { list[index] = value; }
		}

		public object SyncRoot {	
			get { return list.SyncRoot; }
		}

		#endregion

		#region Methods

		public int Add (object value) 
		{
			return list.Add (value); 
		}

		public void Clear ()
		{
			list.Clear ();
		}

		public bool Contains (object value)
		{
			return list.Contains (value);
		}

		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

#if ONLY_1_0 || ONLY_1_1
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return list.GetEnumerator (); 
		}
#endif

		public int IndexOf (object value)
		{
			return list.IndexOf (value);
		}

		public void Insert (int index, object value)
		{
			list.Insert (index, value);
		}

		public void Remove (object value)
		{ 
			list.Remove (value);
		}

		public void RemoveAt (int index) 
		{
			list.RemoveAt (index);
		}

		#endregion // Methods
	}
}
