/**
 * Namespace: System.Web.UI.WebControls
 * Class:     TableRowCollection
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public sealed class TableRowCollection: IList, ICollection, IEnumerable
	{
		TableRow owner;

		internal TableRowCollection(TableRow owner)
		{
			if(owner == null)
			{
				throw new ArgumentNullException();
			}
			this.owner = owner;
		}

		public int Count
		{
			get
			{
				return owner.Controls.Count;
			}
		}
		
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}
		
		public bool IsSynchronized
		{
			get
			{
				return false;				
			}
		}
		
		public TableRow this[int index]
		{
			get
			{
				return (TableRow)owner.Controls[index];
			}
		}
		
		public object SyncRoot
		{
			get
			{
				return this;
			}
		}
		
		public int Add(TableRow row)
		{
			AddAt(-1, row);
			return owner.Controls.Count;
		}
		
		public void AddAt(int index, TableRow row)
		{
			owner.Controls.AddAt(index, row);
		}
		
		public void AddRange(TableRow[] rows)
		{
			foreach(TableRow row in rows)
			{
				Add(row);
			}
		}
		
		public void Clear()
		{
			if(owner.HasControls())
			{
				owner.Controls.Clear();
			}
		}
		
		public void CopyTo(Array array, int index)
		{
			foreach(object current in this)
			{
				array.SetValue(current, index++);
			}
		}
		
		public int GetRowIndex(TableRow row)
		{
			if(!owner.HasControls())
			{
				return -1;
			}
			return owner.Controls.IndexOf(row);
		}
		
		public IEnumerator GetEnumerator()
		{
			return owner.Controls.GetEnumerator();
		}
		
		public void Remove(TableRow row)
		{
			owner.Controls.Remove(row);
		}
		
		public void RemoveAt(int index)
		{
			owner.Controls.RemoveAt(index);
		}
		
		private void IList.Add(object o)
		{
			Add((TableRow)o);
		}
		
		private bool IList.Contains(object o)
		{
			return owner.Controls.Contains((TableRow)o);
		}
		
		private int IList.IndexOf(object o)
		{
			return owner.Controls.IndexOf((TableRow)o);
		}
		
		private void IList.Insert(int index, object o)
		{
			onwer.Controls.Insert(index, (TableRow)o);
		}
		
		private void IList.Remove(object o)
		{
			onwer.Controls.Remove((TableRow)o);
		}
		
		private bool IList.IsFixedSize()
		{
			return false;
		}
		
		private object IList.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
				RemoveAt(index);
				AddAt(index, (TableRow)value);
			}
		}
	}
}
