//
// System.Windows.Forms.Menu.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Dennis Hayes (dennish@raytek.com)
//	Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) 2002 Ximian, Inc
//

using System;
using System.Reflection;
using System.Globalization;
//using System.Windows.Forms.AccessibleObject.IAccessible;
using System.Drawing;
using System.Collections;
using System.Runtime.Remoting;

namespace System.Windows.Forms  {


	/// <summary>
	/// </summary>
	using System.ComponentModel;
	public abstract class Menu : Component 	{

		//
		// -- Public Methods
		//

		[MonoTODO]
		public ContextMenu GetContextMenu() {
			throw new NotImplementedException();
		}

		[MonoTODO]
		public MainMenu GetMainMenu() {
			if ( parent_ == null )
				return this as MainMenu;

			Menu parent = parent_;

			while ( parent != null ) {
				if ( parent.parent_ != null )
					parent = parent.parent_;
				else
					break;
			}

			return parent as MainMenu;
		}

		[MonoTODO]
		public virtual void MergeMenu(Menu menuSrc) {
			// FIXME:
		}
		
		[MonoTODO]
		public override string ToString() {
			//do our own ToString here.
			// this is overrridden
			return base.ToString();
		}

		//
		// -- Protected Methods
		//

		protected void CloneMenu(Menu menuSrc) {
			// FIXME:
		}

		protected Menu( MenuItem[] items) {
			MenuItems.AddRange ( items);
		}

		//
		// -- Public Properties
		//

		private bool menuStructureModified_ = true;

		internal bool MenuStructureModified {
			set {
				menuStructureModified_ = value;
			}
		}

		
		internal void BuildMenuStructure () {
			if( menuStructureModified_) {
				Win32.SetMenuDefaultItem(menuHandle_, -1, 0);
				while( Win32.RemoveMenu( menuHandle_, 0, (uint)MF_.MF_BYPOSITION) != 0);
				foreach(MenuItem mi in MenuItems) {
					//System.Console.WriteLine("MenuItem {0} Parent {1}", mi.Text, mi.IsParent);
					if( mi.IsParent){
						Win32.AppendMenuA( menuHandle_, (int)MF_.MF_ENABLED | (int)MF_.MF_STRING | (int)MF_.MF_POPUP,
															mi.Handle, mi.Text);
					}
					else {
						Win32.AppendMenuA( menuHandle_, mi.MenuItemFlags,
								   (IntPtr) mi.GetID(), mi.Text);
						if(mi.DefaultItem) {
							Win32.SetMenuDefaultItem(menuHandle_, mi.GetID(), 0);
						}
					}
				}
				menuStructureModified_ = false;
			}
		}
		
        internal Menu parent_ = null;
        
		internal IntPtr menuHandle_ = IntPtr.Zero;
		internal bool   isPopupMenu = false;

		internal void CreateMenuHandle() {
			if( menuHandle_ == IntPtr.Zero) {
				if ( !isPopupMenu )
					menuHandle_ = Win32.CreateMenu();
				else
					menuHandle_ = Win32.CreatePopupMenu ( );
				//System.Console.WriteLine("Create menu {0}", menuHandle_);
				BuildMenuStructure();
				allMenus_[menuHandle_] = this;
			}
		}
		
		public IntPtr Handle {
			get {
				CreateMenuHandle();
				return menuHandle_;
			}
		}

		public virtual bool IsParent {

			get {
				return MenuItems.Count != 0;
			}
		}

		public MenuItem MdiListItem {
			get {
				MenuItem mdiListItem = null;
				foreach( MenuItem mi in MenuItems) {
					if ( mi.MdiList )
						return mi;

					mdiListItem = mi.MdiListItem;
					if ( mdiListItem != null ) break;
				}
				return mdiListItem;
			}
		}

		private Menu.MenuItemCollection  menuCollection_ = null;

		public Menu.MenuItemCollection MenuItems {
			get {
				if( menuCollection_ == null) {
					menuCollection_ = new Menu.MenuItemCollection( this);
				}
				return menuCollection_;
			}
		}


		// Library interface

		// Recursively searches for specified item in menu.
		// Goes immediately into child, when mets one.
		internal MenuItem GetMenuItemByID (uint id) {
			foreach( MenuItem mi in MenuItems) {
				if( mi.IsParent) {
					MenuItem submi = mi.GetMenuItemByID(id);
					if( submi != null) return submi;
				}
				else {
					if( mi.GetID() == id){
						return mi;
					}
				}
			}
			return null;
		}
		
		private static Hashtable allMenus_ = new Hashtable();
		
		internal static Menu GetMenuByHandle (IntPtr hMenu) {
			Menu result = null;
			try {
				result = allMenus_[hMenu] as Menu;
			}
			catch(ArgumentNullException) {
			}
			catch(NotSupportedException) {
			}
			return result;
		}
		
		internal void OnNewMenuItemAdd (MenuItem mi){
			menuStructureModified_ = true;
			mi.SetParent( this);
		}
		
		internal void OnRemoveMenuItem (MenuItem mi)
		{
			if(menuHandle_ != IntPtr.Zero) {
				menuStructureModified_ = true;
			}
			mi.SetParent( null);
		}
		
		internal void OnLastSubItemRemoved ()
		{
			if( menuHandle_ != IntPtr.Zero) {
				//System.Console.WriteLine("Delete menu {0}", menuHandle_);
				Win32.DestroyMenu(menuHandle_);
				allMenus_.Remove(menuHandle_);
				menuHandle_ = IntPtr.Zero;
				
				if( parent_ != null) {
					parent_.MenuStructureModified = true;
				}
			}
		}
		
		internal void OnWmInitMenu ()
		{
		}
		
		internal void OnWmInitMenuPopup ()
		{
			BuildMenuStructure();
		}

		//
		// System.Windows.Forms.Menu.MenuItemCollection.cs
		//
		// Author:
		//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
		//
		// (C) 2002 Ximian, Inc
		//
		/// <summary>
		/// </summary>

		public class MenuItemCollection : IList, ICollection, IEnumerable {
			private ArrayList		items_ = new ArrayList();
			private Menu 			parentMenu_ = null;
			//
			// -- Constructor
			//

			public MenuItemCollection (Menu m) {
				parentMenu_ = m;
			}

			internal void MoveItemToIndex( int index, MenuItem mi) {
				if( index >= items_.Count){
					// FIXME: Set exception parameters
					throw new ArgumentException();
				}
				else if( items_.Count != 1){
					items_.Remove (mi);
					items_.Insert (index, mi);
					mi.SetIndex(index);
				}
			}

			//
			// -- Public Methods
			//

			public virtual int Add (MenuItem mi) {
				int result = -1;
				if( mi != null && parentMenu_ != null){
					parentMenu_.OnNewMenuItemAdd(mi);
					items_.Add(mi);
					result = items_.Count - 1;
					mi.SetIndex(result);
				}
				return result;
			}
			
			private MenuItem AddMenuItemCommon (MenuItem mi) {
				return ( -1 != Add (mi)) ? mi : null;
			}

			public virtual MenuItem Add ( string s) {
				return AddMenuItemCommon( new MenuItem (s));
			}

			public virtual int Add ( int i, MenuItem mi) {
				if( i > items_.Count){
					// FIXME: Set exception details
					throw new System.ArgumentException();
				}
				int result = -1;
				if( mi != null && parentMenu_ != null){
					parentMenu_.OnNewMenuItemAdd(mi);
					items_.Insert(i, mi);
					result = i;
					mi.SetIndex(result);
				}
				return result;
			}

			public virtual MenuItem Add (string s, EventHandler e) {
				return AddMenuItemCommon(new MenuItem ( s, e));
			}

			public virtual MenuItem Add (string s, MenuItem[] items) {
				return AddMenuItemCommon(new MenuItem ( s, items));
			}

			public virtual void AddRange(MenuItem[] items) {
				if( items != null) {
					foreach( MenuItem mi in items) {
						Add(mi);
					}
				}
			}

			private void DoClear() {
				if( parentMenu_ != null) {
					foreach( MenuItem mi in items_) {
						parentMenu_.OnRemoveMenuItem( mi);
					}
				}
				items_.Clear();
				if( parentMenu_ != null) {
					parentMenu_.OnLastSubItemRemoved();
				}				
			}

			public virtual void Clear() {
				DoClear();
			}

			public bool Contains(MenuItem m) {
				return items_.Contains(m);
			}

			public void CopyTo(Array a, int i) {
				int targetIdx = i;
				foreach( MenuItem mi in items_) {
					MenuItem newMi = mi.CloneMenu();
					a.SetValue(newMi,targetIdx++);
				}
			}

			public override bool Equals(object o) {
				return base.Equals(o);
			}

			[MonoTODO]
			public override int GetHashCode() {
				//FIXME add our proprities
				return base.GetHashCode();
			}

			public IEnumerator GetEnumerator() {
				return items_.GetEnumerator();
			}

			public int IndexOf(MenuItem m) {
				return items_.IndexOf(m);
			}

			public virtual void Remove(MenuItem m) {
				if( m != null && parentMenu_ != null){
					if( Contains(m)){
						parentMenu_.OnRemoveMenuItem(m);
						items_.Remove(m);
						if( items_.Count == 0){
							parentMenu_.OnLastSubItemRemoved();
						}				
					}
				}
			}

			public virtual void RemoveAt(int i) {
				Remove(items_[i] as MenuItem);
			}

			public override string ToString() {
				throw new NotImplementedException ();
			}

			//
			// -- Protected Methods
			//

			~MenuItemCollection() {
				Clear();
			}

			//inherited
			//protected object MemberwiseClone() {
			//	throw new NotImplementedException ();
			//}

			//
			// -- Public Properties
			//

			public int Count {

				get {
					return items_.Count;					
				}
			}

			//		public virtual MenuItem this(int i)
			//		{
			//			get
			//			{
			//				throw new NotImplementedException ();
			//			}
			//		}
			/// <summary>
			/// IList Interface implmentation.
			/// </summary>
			bool IList.IsReadOnly {
				get {
					// We allow addition, removeal, and editing of items after creation of the list.
					return false;
				}
			}

			bool IList.IsFixedSize {
				get {
					// We allow addition and removeal of items after creation of the list.
					return false;
				}
			}

			public MenuItem this[int index] {
				get {
					return items_[index] as MenuItem;
				}
			}

			//[MonoTODO]
			object IList.this[int index] {
				get {
					return items_[index];
				}
				set {
					// FIXME: Set exception members
					throw new System.NotSupportedException();
				}
			}
		
			[MonoTODO]
			void IList.Clear() {
				DoClear();
			}

			private MenuItem Object2MenuItem( object value) {
				MenuItem result = value as MenuItem;
				if( result == null) {
					// FIXME: Set exception parameters
					throw new System.ArgumentException();
				}
				return result;
			}

			[MonoTODO]
			int IList.Add( object value) {
				return Add( Object2MenuItem(value));
			}

			[MonoTODO]
			bool IList.Contains( object value) {
				return Contains(Object2MenuItem(value));
			}

			[MonoTODO]
			int IList.IndexOf( object value) {
				return IndexOf(Object2MenuItem(value));
			}

			[MonoTODO]
			void IList.Insert(int index, object value) {
				Add( index, Object2MenuItem(value));
			}

			[MonoTODO]
			void IList.Remove( object value) {
				Remove( Object2MenuItem(value));
			}

			[MonoTODO]
			void IList.RemoveAt( int index){
				RemoveAt(index);
			}
			// End of IList interface

			/// <summary>
			/// ICollection Interface implmentation.
			/// </summary>
			int ICollection.Count {
				get {
					return Count;
				}
			}
			bool ICollection.IsSynchronized {
				get {
					throw new NotImplementedException ();
				}
			}
			object ICollection.SyncRoot {
				get {
					throw new NotImplementedException ();
				}
			}
			void ICollection.CopyTo(Array array, int index){
				CopyTo(array, index);
			}
			// End Of ICollection
		}
	}
}



