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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//
//	TODO:
//		- MDI integration
//		- ShortCut navigation
//
// NOT COMPLETE

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[Designer ("Microsoft.VisualStudio.Windows.Forms.MenuDesigner, " + Consts.AssemblyMicrosoft_VisualStudio, "System.ComponentModel.Design.IDesigner")]
	[ToolboxItemFilter("System.Windows.Forms", ToolboxItemFilterType.Allow)]
	[ListBindable(false)]
	public abstract class Menu : Component
	{
		internal MenuItemCollection menu_items;
		internal IntPtr menu_handle = IntPtr.Zero;
		internal Menu parent_menu = null;
		System.Drawing.Rectangle rect;
		internal Control Wnd;
		internal MenuTracker tracker;

		public const int FindHandle = 0;
		public const int FindShortcut = 1;

 		protected Menu (MenuItem[] items)
		{
			menu_items = new MenuItemCollection (this);

			if (items != null)
				menu_items.AddRange (items);
		}


		#region Public Properties
		[BrowsableAttribute(false)]
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public IntPtr Handle {
			get { return menu_handle; }
		}

		internal virtual void OnMenuChanged (EventArgs e)
		{
			if (MenuChanged != null)
				MenuChanged (this, e);
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public virtual bool IsParent {
			get {
				if (menu_items != null && menu_items.Count > 0)
					return true;
				else
					return false;
			}
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public MenuItem MdiListItem {
			get {
				throw new NotImplementedException ();
			}
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content)]
		[MergableProperty(false)]
		public MenuItemCollection MenuItems {
			get { return menu_items; }
		}

		#endregion Public Properties

		#region Private Properties

		internal System.Drawing.Rectangle Rect {
			get {
				return rect;
			}
		}

		internal MenuItem SelectedItem  {
			get {
				foreach (MenuItem item in MenuItems)
					if ((item.Status & DrawItemState.Selected) == DrawItemState.Selected)
						return item;

				return null;
			}
		}

		internal int Height {
			get { return rect.Height; }
			set { rect.Height = value; }
		}

		internal int Width {
			get { return rect.Width; }
			set { rect.Width = value; }
		}

		internal int X {
			get { return rect.X; }
			set { rect.X = value; }
		}

		internal int Y {
			get { return rect.Y; }
			set { rect.Y = value; }
		}

		internal MenuTracker Tracker {
			get {
				Menu top = this;
				while (top.parent_menu != null)
					top = top.parent_menu;

				return top.tracker;
			}
		}
		#endregion Private Properties

		#region Public Methods

		protected void CloneMenu (Menu menuSrc)
		{
			Dispose (true);

			menu_items = new MenuItemCollection (this);

			for (int i = 0; i < menuSrc.MenuItems.Count ; i++)
				menu_items.Add (menuSrc.MenuItems [i].CloneMenu ());
		}

		protected override void Dispose (bool disposing)
		{		
			if (disposing) {
				if (menu_handle != IntPtr.Zero) {
					menu_handle = IntPtr.Zero;
				}
			}
		}

		// From Microsoft documentation is impossible to guess that 
		// this method is supossed to do
		public MenuItem FindMenuItem (int type, IntPtr value)
		{
			return null;
		}

		protected int FindMergePosition (int mergeOrder)
		{
			int cnt = MenuItems.Count, cur, pos;
			
			for (pos = 0; pos < cnt; ) {
				cur = (pos + cnt) /2;
				if (MenuItems[cur].MergeOrder > mergeOrder) {
					cnt = cur;
				} else	{
					pos = cur +1;
				}
			}
			
			return pos;
		}

		public ContextMenu GetContextMenu ()
		{
			for (Menu item = this; item != null; item = item.parent_menu) {
				if (item is ContextMenu) {
					return (ContextMenu) item;
				}
			}
			
			return null;
		}

		public MainMenu GetMainMenu ()
		{				
			for (Menu item = this; item != null; item = item.parent_menu) {
				if (item is MainMenu) {
					return (MainMenu) item;
				}				
			}
			
			return null;
		}

		public virtual void MergeMenu (Menu menuSrc)
		{
			if (menuSrc == this)
				throw new ArgumentException ("The menu cannot be merged with itself");
			
			for (int i = 0; i < menuSrc.MenuItems.Count; i++){
								
				switch (menuSrc.MenuItems[i].MergeType) {
					case MenuMerge.Remove:	// Item not included
						break;
						
					case MenuMerge.Add:
					{
						int pos = FindMergePosition (menuSrc.MenuItems[i].MergeOrder);						
						MenuItems.Add (pos, menuSrc.MenuItems[i].CloneMenu ());
						break;					
					}
					
					case MenuMerge.Replace:
					case MenuMerge.MergeItems:
					{
						int pos = FindMergePosition (menuSrc.MenuItems[i].MergeOrder - 1);						
						MenuItems.Insert (pos, menuSrc.MenuItems[i].CloneMenu ());
						
						break;
					}
					
					default:
						break;
				}			
			}		
		}

		internal bool ProcessCmdKey (ref Message msg, Keys keyData)
		{
			if (tracker == null)
				return false;
			return tracker.ProcessKeys (ref msg, keyData);
		}

		public override string ToString ()
		{
			return base.ToString () + ", Items.Count: " + MenuItems.Count;
		}

		#endregion Public Methods

		internal event EventHandler MenuChanged;

		[ListBindable(false)]
		public class MenuItemCollection : IList, ICollection, IEnumerable
		{
			private Menu owner;
			private ArrayList items = new ArrayList ();

			public MenuItemCollection (Menu owner)
			{
				this.owner = owner;
			}

			#region Public Properties

			public virtual int Count {
				get { return items.Count;}
			}

			public virtual bool IsReadOnly {
				get { return false;}
			}

			bool ICollection.IsSynchronized {
				get { return false;}
			}

			object ICollection.SyncRoot {
				get { return this;}
			}

			bool IList.IsFixedSize {
				get { return false;}
			}

			public virtual MenuItem this [int index] {
				get {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("Index of out range");

					return (MenuItem) items[index];
				}
			}

			object IList.this[int index] {
				get { return items[index]; }
				set { throw new NotSupportedException (); }
			}

			#endregion Public Properties

			#region Public Methods

			public virtual int Add (MenuItem mi)
			{
				if (mi.Parent != null)
					mi.Parent.MenuItems.Remove (mi);
				
				items.Add (mi);
				mi.Index = items.Count - 1;
				UpdateItem (mi);
				
				owner.OnMenuChanged (EventArgs.Empty);
				if (owner.parent_menu != null)
					owner.parent_menu.OnMenuChanged (EventArgs.Empty);
				return items.Count - 1;
			}

			internal void AddNoEvents (MenuItem mi)
			{
				if (mi.Parent != null)
					mi.Parent.MenuItems.Remove (mi);
				
				items.Add (mi);
				mi.Index = items.Count - 1;
				mi.parent_menu = owner;
			}

			public virtual MenuItem Add (string s)
			{
				MenuItem item = new MenuItem (s);
				Add (item);
				return item;
			}

			public virtual int Add (int index, MenuItem mi)
			{
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				ArrayList new_items = new ArrayList (Count + 1);

				for (int i = 0; i < index; i++)
					new_items.Add (items[i]);

				new_items.Add (mi);

				for (int i = index; i < Count; i++)
					new_items.Add (items[i]);

				items = new_items;
				UpdateItemsIndices ();				
				UpdateItem (mi);

				return index;
			}

			private void UpdateItem (MenuItem mi)
			{
				mi.parent_menu = owner;
				owner.OnMenuChanged (EventArgs.Empty);
				if (owner.parent_menu != null)
					owner.parent_menu.OnMenuChanged (EventArgs.Empty);
				if (owner.Tracker != null)
					owner.Tracker.AddShortcuts (mi);
			}

			internal void Insert (int index, MenuItem mi)
			{
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ("Index of out range");
				UpdateItemsIndices ();
				UpdateItem (mi);
			}

			public virtual MenuItem Add (string s, EventHandler e)
			{
				MenuItem item = new MenuItem (s, e);
				Add (item);

				return item;
			}

			public virtual MenuItem Add (string s, MenuItem[] items)
			{
				MenuItem item = new MenuItem (s, items);
				Add (item);

				return item;
			}

			public virtual void AddRange (MenuItem[] items)
			{
				if (items == null)
					throw new ArgumentNullException ("items");

				foreach (MenuItem mi in items)
					Add (mi);
			}

			public virtual void Clear ()
			{
				MenuTracker tracker = owner.Tracker;
				foreach (MenuItem item in items) {
					if (tracker != null)
						tracker.RemoveShortcuts (item);
					item.parent_menu = null;
				}
				items.Clear ();
				owner.OnMenuChanged (EventArgs.Empty);
			}

			public bool Contains (MenuItem value)
			{
				return items.Contains (value);
			}

			public virtual void CopyTo (Array dest, int index)
			{
				items.CopyTo (dest, index);
			}

			public virtual IEnumerator GetEnumerator ()
			{
				return items.GetEnumerator ();
			}

			int IList.Add (object value)
			{
				return Add ((MenuItem)value);
			}

			bool IList.Contains (object value)
			{
				return Contains ((MenuItem)value);
			}

			int IList.IndexOf (object value)
			{
				return IndexOf ((MenuItem)value);
			}

			void IList.Insert (int index, object value)
			{
				Insert (index, (MenuItem) value);
			}

			void IList.Remove (object value)
			{
				Remove ((MenuItem) value);
			}

			public int IndexOf (MenuItem value)
			{
				return items.IndexOf (value);
			}

			public virtual void Remove (MenuItem item)
			{
				RemoveAt (item.Index);
			}

			public virtual void RemoveAt (int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				MenuItem item = (MenuItem) items [index];
				MenuTracker tracker = owner.Tracker;
				if (tracker != null)
					tracker.RemoveShortcuts (item);
				item.parent_menu = null;

				items.RemoveAt (index);

				UpdateItemsIndices ();
				owner.OnMenuChanged (EventArgs.Empty);
			}

			#endregion Public Methods

			#region Private Methods

			private void UpdateItemsIndices ()
			{
				for (int i = 0; i < Count; i++)	// Recalculate indeces
					((MenuItem)items[i]).Index = i;
			}

			#endregion Private Methods
		}
	}
}


