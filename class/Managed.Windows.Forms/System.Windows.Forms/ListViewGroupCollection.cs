//
//  ListViewGroupCollection.cs
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
// Copyright (c) 2006 Daniel Nauck
//
// Author:
//      Daniel Nauck    (dna(at)mono-project(dot)de)

#if NET_2_0

using System;
using System.Text;
using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms
{
	[ListBindable(false)]
	public class ListViewGroupCollection : IList, ICollection, IEnumerable
	{
		private ArrayList list = null;
		private ListView list_view_owner = null;

		ListViewGroupCollection()
		{
			list = new ArrayList();
		}

		internal ListViewGroupCollection(ListView listViewOwner) : this()
		{
			list_view_owner = listViewOwner;
		}

		internal ListView ListViewOwner {
			get { return list_view_owner; }
			set { list_view_owner = value; }
		}

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return list.GetEnumerator();
		}

		#endregion

		#region ICollection Members

		public void CopyTo(Array array, int index)
		{
			list.CopyTo(array, index);
		}

		public int Count {
			get { return list.Count; }
		}

		bool ICollection.IsSynchronized {
			get { return true; }
		}

		object ICollection.SyncRoot {
			get { return this; }
		}

		#endregion

		#region IList Members

		int IList.Add(object value)
		{
			if (!(value is ListViewGroup))
				throw new ArgumentException("value");

			return Add((ListViewGroup)value);               
		}

		public int Add(ListViewGroup value)
		{
			if (Contains(value))
				return -1;

            		CheckListViewItemsInGroup(value);
			value.ListViewOwner = this.list_view_owner;
			int index = list.Add(value);

			if (this.list_view_owner != null)
				list_view_owner.Redraw(true);

			return index;
		}

		public ListViewGroup Add(string key, string headerText)
		{
			ListViewGroup newGroup = new ListViewGroup(key, headerText);
			Add(newGroup);

			return newGroup;
		}

		public void Clear()
		{
			foreach (ListViewGroup group in list)
				group.ListViewOwner = null;

			list.Clear ();

			if(list_view_owner != null)
				list_view_owner.Redraw(true);
		}

		bool IList.Contains(object value)
		{
			if (value is ListViewGroup)
				return Contains((ListViewGroup)value);
			else
				return false;
		}

		public bool Contains(ListViewGroup value)
		{
			return list.Contains(value);
		}

		int IList.IndexOf(object value)
		{
			if (value is ListViewGroup)
				return IndexOf((ListViewGroup)value);
			else
				return -1;
		}

		public int IndexOf(ListViewGroup value)
		{
			return list.IndexOf(value);
		}

		void IList.Insert(int index, object value)
		{
			if (value is ListViewGroup)
				Insert(index, (ListViewGroup)value);
		}

		public void Insert(int index, ListViewGroup group)
		{
			if (Contains(group))
				return;

            		CheckListViewItemsInGroup(group);
			group.ListViewOwner = list_view_owner;
			list.Insert(index, group);

			if(list_view_owner != null)
				list_view_owner.Redraw(true);
		}

		bool IList.IsFixedSize {
			get { return false; }
		}

		bool IList.IsReadOnly {
			get { return false; }
		}

		void IList.Remove(object value)
		{
			Remove((ListViewGroup)value);
		}

		public void Remove (ListViewGroup value)
		{
			int idx = list.IndexOf (value);
			if (idx != -1)
				RemoveAt (idx);
		}

		public void RemoveAt (int index)
		{
			if (list.Count <= index || index < 0)
				return;

			ListViewGroup group = (ListViewGroup) list [index];
			group.ListViewOwner = null;

			list.RemoveAt (index);
			if (list_view_owner != null)
				list_view_owner.Redraw (true);
		}

		object IList.this[int index] {
			get { return this[index]; }
			set {
				if (value is ListViewGroup)
					this[index] = (ListViewGroup)value;
			}
		}

		public ListViewGroup this[int index] {
			get {
				if (list.Count <= index || index < 0)
					throw new ArgumentOutOfRangeException("index");

				return (ListViewGroup)list[index];
			}
			set {
				if (list.Count <= index || index < 0)
					throw new ArgumentOutOfRangeException("index");

				if (!Contains(value)) {
                    			CheckListViewItemsInGroup(value);
					list[index] = value;
                    
					if (list_view_owner != null)
						list_view_owner.Redraw(true);
				}
			}
		}

		public ListViewGroup this[string key] {
			get {
				foreach (ListViewGroup item in list)
				{
					if (item.Name.Equals(key))
						return item;
				}

				return null;
			}
			set {
				for(int i = 0; i < list.Count; i++)
				{
					if ((this[i].Name != null) && (this[i].Name.Equals(key))) {
						this[i] = value;
						break;
					}
				}
			}
		}

		#endregion

		public void AddRange(ListViewGroup[] groups)
		{
			foreach (ListViewGroup item in groups)
			{
				Add(item);
			}
		}

		public void AddRange(ListViewGroupCollection groups)
		{
			foreach (ListViewGroup item in groups)
			{
				Add(item);
			}
		}

		private void CheckListViewItemsInGroup(ListViewGroup value)
		{
			//check for correct ListView
			foreach (ListViewItem item in value.Items)
			{
				if (item.ListView != null && item.ListView != this.list_view_owner)
					throw new ArgumentException("ListViewItem belongs to a ListView control other than the one that owns this ListViewGroupCollection.",
						"ListViewGroup");
			}
		}
	}
}
#endif
