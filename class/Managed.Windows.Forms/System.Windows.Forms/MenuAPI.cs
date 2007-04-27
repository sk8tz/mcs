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
//	Mike Kestner  <mkestner@novell.com>
//	Everaldo Canuto  <ecanuto@novell.com>
//

using System.Collections;
using System.Drawing;
using System.Threading;

namespace System.Windows.Forms {

	/*
		When writing this code the Wine project was of great help to
		understand the logic behind some Win32 issues. Thanks to them. Jordi,
	*/
	internal class MenuTracker {

		internal bool active;
		internal bool popup_active;
		internal bool popdown_menu;
		internal bool hotkey_active;
		public Menu CurrentMenu;
		public Menu TopMenu;
		Form grab_control;

	    public MenuTracker (Menu top_menu)
		{
			TopMenu = CurrentMenu = top_menu;
			foreach (MenuItem item in TopMenu.MenuItems)
				AddShortcuts (item);

			if (top_menu is ContextMenu) {
				grab_control = (top_menu as ContextMenu).SourceControl.FindForm ();
				grab_control.ActiveTracker = this;
			} else
				grab_control = top_menu.Wnd.FindForm ();
		}

		enum KeyNavState {
			Idle,
			Startup,
			NoPopups,
			Navigating
		}

		KeyNavState keynav_state = KeyNavState.Idle;

		public bool Navigating {
			get { return keynav_state != KeyNavState.Idle || active; }
		}

		internal static Point ScreenToMenu (Menu menu, Point pnt)		
		{
			int x = pnt.X;
			int y = pnt.Y;
			XplatUI.ScreenToMenu (menu.Wnd.window.Handle, ref x, ref y);
			return new Point (x, y);
		}	

		private void UpdateCursor ()
		{
			Control child_control = grab_control.GetRealChildAtPoint (Cursor.Position);
			if (child_control != null) {
				if (active)
					XplatUI.SetCursor (child_control.Handle, Cursors.Default.handle);
				else
					XplatUI.SetCursor (child_control.Handle, child_control.Cursor.handle);
			}
		}

		void Deactivate ()
		{
			active = false;
			popup_active = false;
			hotkey_active = false;
			grab_control.ActiveTracker = null;
			keynav_state = KeyNavState.Idle;
			if (TopMenu is ContextMenu) {
				PopUpWindow puw = TopMenu.Wnd as PopUpWindow;
				DeselectItem (TopMenu.SelectedItem);
				puw.HideWindow ();
			} else {
				DeselectItem (TopMenu.SelectedItem);
			}
			CurrentMenu = TopMenu;
		}

		MenuItem FindItemByCoords (Menu menu, Point pt)
		{
			if (menu is MainMenu)
				pt = ScreenToMenu (menu, pt);
			else
				pt = menu.Wnd.PointToClient (pt);
			foreach (MenuItem item in menu.MenuItems) {
				Rectangle rect = item.bounds;
				if (rect.Contains (pt))
					return item;
			}

			return null;
		}

		MenuItem GetItemAtXY (int x, int y)
		{
			Point pnt = new Point (x, y);
			MenuItem item = null;
			if (TopMenu.SelectedItem != null)
				item = FindSubItemByCoord (TopMenu.SelectedItem, Control.MousePosition);
			if (item == null)
				item = FindItemByCoords (TopMenu, pnt);
			return item;
		}

		public bool OnMouseDown (MouseEventArgs args)
		{
			MenuItem item = GetItemAtXY (args.X, args.Y);

			if (item == null) {
				Deactivate ();
				return false;
			}

			if ((args.Button & MouseButtons.Left) == 0)
				return true;

			if (!item.Enabled)
				return true;
			
			popdown_menu = active && item.VisibleItems;
			
			if (item.IsPopup || (item.Parent is MainMenu)) {
				active = true;
				item.Parent.InvalidateItem (item);
			}
			
			if ((CurrentMenu == TopMenu) && !popdown_menu)
				SelectItem (item.Parent, item, item.IsPopup);
			
			grab_control.ActiveTracker = this;
			return true;
		}

		public void OnMotion (MouseEventArgs args)
		{
			MenuItem item = GetItemAtXY (args.X, args.Y);

			UpdateCursor ();

			if (CurrentMenu.SelectedItem == item)
				return;

			grab_control.ActiveTracker = (active || item != null) ? this : null;

			if (item == null) {
				MenuItem old_item = CurrentMenu.SelectedItem;
				
				// Return when is a popup with visible subitems for MainMenu 
				if  ((active && old_item.VisibleItems && old_item.IsPopup && (CurrentMenu is MainMenu)))
					return;

				// Also returns when keyboard navigating
				if (keynav_state == KeyNavState.Navigating)
					return;
				
				keynav_state = KeyNavState.Navigating;
				
				// Select parent menu when move outside of menu item
				if (old_item.Parent is MenuItem) {
					MenuItem new_item = (old_item.Parent as MenuItem);
					if (new_item.IsPopup) {
						SelectItem (new_item.Parent, new_item, false);
						return;
					}
				}
				if (CurrentMenu != TopMenu)
					CurrentMenu = CurrentMenu.parent_menu;
								
				DeselectItem (old_item);
			} else {
				keynav_state = KeyNavState.Idle;
				SelectItem (item.Parent, item, active && item.IsPopup && popup_active && (CurrentMenu.SelectedItem != item));
			}
		}

		public void OnMouseUp (MouseEventArgs args)
		{
			if ((args.Button & MouseButtons.Left) == 0)
				return;
			
			MenuItem item = GetItemAtXY (args.X, args.Y);

			/* the user released the mouse button outside the menu */
			if (item == null) {
				Deactivate ();
				return;
			}
			
			if (!item.Enabled)
				return;
			
			/* Deactivate the menu when is topmenu and popdown and */
			if (((CurrentMenu == TopMenu) && !(CurrentMenu is ContextMenu) && popdown_menu) || !item.IsPopup) {
				Deactivate ();
				UpdateCursor ();
			}
			
			/* Perform click when is not a popup */
			if (!item.IsPopup) {
				DeselectItem (item);
				item.PerformClick ();
			}
		}

		static public bool TrackPopupMenu (Menu menu, Point pnt)
		{
			Object	queue_id;

			if (menu.MenuItems.Count <= 0)	// No submenus to track
				return true;				

			MenuTracker tracker = new MenuTracker (menu);
			tracker.active = true;
			tracker.popup_active = true;
			menu.tracker = tracker;

			menu.Wnd = new PopUpWindow (tracker.grab_control, menu);
			menu.Wnd.Location =  menu.Wnd.PointToClient (pnt);

			((PopUpWindow)menu.Wnd).ShowWindow ();

			bool no_quit = true;

			queue_id = XplatUI.StartLoop(Thread.CurrentThread);

			while ((menu.Wnd != null) && menu.Wnd.Visible && no_quit) {
				MSG msg = new MSG ();
				no_quit = XplatUI.GetMessage(queue_id, ref msg, IntPtr.Zero, 0, 0);
				XplatUI.TranslateMessage(ref msg);
				XplatUI.DispatchMessage(ref msg);				
			}

			if (!no_quit)
				XplatUI.PostQuitMessage(0);

			if (menu.Wnd != null) {
				menu.Wnd.Dispose ();
				menu.Wnd = null;
			}

			return true;
		}
	
		void DeselectItem (MenuItem item)
		{
			if (item == null)
				return;				
			
			item.Selected = false;

			/* When popup item then close all sub popups and unselect all sub items */
			if (item.IsPopup) {
				HideSubPopups (item);
				
				/* Unselect all selected sub itens */
				foreach (MenuItem subitem in item.MenuItems)
					if (subitem.Selected)
						DeselectItem (subitem);
			}

			Menu menu = item.Parent;
			menu.InvalidateItem (item);
		}

		void SelectItem (Menu menu, MenuItem item, bool execute)
		{
			MenuItem prev_item = CurrentMenu.SelectedItem;
			
			if (prev_item != item.Parent) {
				DeselectItem (prev_item);
				if ((CurrentMenu != menu) && (prev_item.Parent != item) && (prev_item.Parent is MenuItem)) {
					DeselectItem (prev_item.Parent as MenuItem);
				}
			}

			if (CurrentMenu != menu)
				CurrentMenu = menu;
			
			item.Selected = true;
			menu.InvalidateItem (item);
			
			if (((CurrentMenu == TopMenu) && execute) || ((CurrentMenu != TopMenu) && popup_active))
				item.PerformSelect ();

			if ((execute) && ((prev_item == null) || (item != prev_item.Parent)))
				ExecFocusedItem (menu, item);
		}

		//	Used when the user executes the action of an item (press enter, shortcut)
		//	or a sub-popup menu has to be shown
		void ExecFocusedItem (Menu menu, MenuItem item)
		{
			if (item == null)
				return;

			if (!item.Enabled)
			 	return;
			 	
			if (item.IsPopup) {
				ShowSubPopup (menu, item);
			} else {
				Deactivate ();
				item.PerformClick ();
			}
		}

		// Create a popup window and show it or only show it if it is already created
		void ShowSubPopup (Menu menu, MenuItem item)
		{
			if (item.Enabled == false)
				return;

			if (!popdown_menu || !item.VisibleItems) 
				item.PerformPopup ();
			
			if (item.VisibleItems == false)
				return;

			if (item.Wnd != null) {
				item.Wnd.Dispose ();
			}

			popup_active = true;
			PopUpWindow puw = new PopUpWindow (grab_control, item);
			
			Point pnt;
			if (menu is MainMenu)
				pnt = new Point (item.X, item.Y + item.Height - 2 - menu.Height);
			else
				pnt = new Point (item.X + item.Width - 3, item.Y - 3);
			pnt = menu.Wnd.PointToScreen (pnt);
			puw.Location = pnt;
			item.Wnd = puw;

			puw.ShowWindow ();
		}

		static public void HideSubPopups (Menu menu)
		{
			foreach (MenuItem item in menu.MenuItems)
				if (item.IsPopup)
					HideSubPopups (item);

			if (menu.Wnd == null)
				return;

			PopUpWindow puw = menu.Wnd as PopUpWindow;
			puw.Hide ();
			menu.Wnd = null;
		}

		MenuItem FindSubItemByCoord (Menu menu, Point pnt)
		{		
			foreach (MenuItem item in menu.MenuItems) {

				if (item.IsPopup && item.Wnd != null && item.Wnd.Visible && item == menu.SelectedItem) {
					MenuItem result = FindSubItemByCoord (item, pnt);
					if (result != null)
						return result;
				}
					
				if (menu.Wnd == null || !menu.Wnd.Visible)
					continue;

				Rectangle rect = item.bounds;
				Point pnt_client = menu.Wnd.PointToScreen (new Point (item.X, item.Y));
				rect.X = pnt_client.X;
				rect.Y = pnt_client.Y;
				
				if (rect.Contains (pnt) == true)
					return item;
			}			
			
			return null;
		}

		static MenuItem FindItemByKey (Menu menu, IntPtr key)
		{
			char key_char = (char ) (key.ToInt32() & 0xff);
			key_char = Char.ToUpper (key_char);

			foreach (MenuItem item in menu.MenuItems) {
				if (item.Mnemonic == '\0')
					continue;

				if (item.Mnemonic == key_char)
					return item;
			}

			return null;
		}

		enum ItemNavigation {
			First,
			Last,
			Next,
			Previous,
		}

		static MenuItem GetNextItem (Menu menu, ItemNavigation navigation)
		{
			int pos = 0;
			bool selectable_items = false;
			MenuItem item;

			// Check if there is at least a selectable item
			for (int i = 0; i < menu.MenuItems.Count; i++) {
				item = menu.MenuItems [i];
				if (item.Separator == false && item.Visible == true) {
					selectable_items = true;
					break;
				}
			}

			if (selectable_items == false)
				return null;

			switch (navigation) {
			case ItemNavigation.First:

				/* First item that is not separator and it is visible*/
				for (pos = 0; pos < menu.MenuItems.Count; pos++) {
					item = menu.MenuItems [pos];
					if (item.Separator == false && item.Visible == true)
						break;
				}

				break;

			case ItemNavigation.Last: // Not used
				break;

			case ItemNavigation.Next:

				if (menu.SelectedItem != null)
					pos = menu.SelectedItem.Index;

				/* Next item that is not separator and it is visible*/
				for (pos++; pos < menu.MenuItems.Count; pos++) {
					item = menu.MenuItems [pos];
					if (item.Separator == false && item.Visible == true)
						break;
				}

				if (pos >= menu.MenuItems.Count) { /* Jump at the start of the menu */
					pos = 0;
					/* Next item that is not separator and it is visible*/
					for (; pos < menu.MenuItems.Count; pos++) {
						item = menu.MenuItems [pos];
						if (item.Separator == false && item.Visible == true)
							break;
					}
				}
				break;

			case ItemNavigation.Previous:

				if (menu.SelectedItem != null)
					pos = menu.SelectedItem.Index;

				/* Previous item that is not separator and it is visible*/
				for (pos--; pos >= 0; pos--) {
					item = menu.MenuItems [pos];
					if (item.Separator == false && item.Visible == true)
						break;
				}

				if (pos < 0 ) { /* Jump at the end of the menu*/
					pos = menu.MenuItems.Count - 1;
					/* Previous item that is not separator and it is visible*/
					for (; pos >= 0; pos--) {
						item = menu.MenuItems [pos];
						if (item.Separator == false && item.Visible == true)
							break;
					}
				}

				break;

			default:
				break;
			}

			return menu.MenuItems [pos];
		}

		void ProcessMenuKey (Msg msg_type)
		{
			if (TopMenu.MenuItems.Count == 0)
				return;

			MainMenu main_menu = TopMenu as MainMenu;

			switch (msg_type) {
			case Msg.WM_SYSKEYDOWN:
				switch (keynav_state) {
				case KeyNavState.Idle:
					keynav_state = KeyNavState.Startup;
					hotkey_active = true;
					grab_control.ActiveTracker = this;
					CurrentMenu = TopMenu;
					main_menu.Draw ();
					break;
				case KeyNavState.Startup:
					break;
				default:
					Deactivate ();
					main_menu.Draw ();
					break;
				}
				break;

			case Msg.WM_SYSKEYUP:
				switch (keynav_state) {
				case KeyNavState.Idle:
				case KeyNavState.Navigating:
					break;
				case KeyNavState.Startup:
					keynav_state = KeyNavState.NoPopups;
					SelectItem (TopMenu, TopMenu.MenuItems [0], false);
					break;
				default:
					Deactivate ();
					main_menu.Draw ();
					break;
				}
				break;
			}
		}

		bool ProcessMnemonic (Message msg, Keys key_data)
		{
			keynav_state = KeyNavState.Navigating;
			MenuItem item = FindItemByKey (CurrentMenu, msg.WParam);
			if (item == null)
				return false;

			active = true;
			grab_control.ActiveTracker = this;
			
			SelectItem (CurrentMenu, item, true);
			if (item.IsPopup) {
				CurrentMenu = item;
				SelectItem (item, item.MenuItems [0], false);
			}
			return true;
		}

		Hashtable shortcuts = new Hashtable ();
		
		public void AddShortcuts (MenuItem item)
		{
			foreach (MenuItem child in item.MenuItems) {
				AddShortcuts (child);
				if (child.Shortcut != Shortcut.None)
					shortcuts [(int)child.Shortcut] = child;
			}

			if (item.Shortcut != Shortcut.None)
				shortcuts [(int)item.Shortcut] = item;
		}

		public void RemoveShortcuts (MenuItem item)
		{
			foreach (MenuItem child in item.MenuItems) {
				RemoveShortcuts (child);
				if (child.Shortcut != Shortcut.None)
					shortcuts.Remove ((int)child.Shortcut);
			}

			if (item.Shortcut != Shortcut.None)
				shortcuts.Remove ((int)item.Shortcut);
		}

		bool ProcessShortcut (Keys keyData)
		{
			MenuItem item = shortcuts [(int)keyData] as MenuItem;
			if (item == null)
				return false;

			Deactivate ();
			item.PerformClick ();
			return true;
		}

		public bool ProcessKeys (ref Message msg, Keys keyData)
		{
			if ((Msg)msg.Msg != Msg.WM_SYSKEYUP && ProcessShortcut (keyData))
				return true;
			else if ((keyData & Keys.KeyCode) == Keys.Menu && TopMenu is MainMenu) {
				ProcessMenuKey ((Msg) msg.Msg);
				return true;
			} else if ((keyData & Keys.Alt) == Keys.Alt)
				return ProcessMnemonic (msg, keyData);
			else if ((Msg)msg.Msg == Msg.WM_SYSKEYUP)
				return false;
			else if (!Navigating)
				return false;

			MenuItem item;
			
			switch (keyData) {
			case Keys.Up:
				if (CurrentMenu is MainMenu)
					return true;
				else if (CurrentMenu.MenuItems.Count == 1 && CurrentMenu.parent_menu == TopMenu) {
					DeselectItem (CurrentMenu.SelectedItem);
					CurrentMenu = TopMenu;
					return true;
				}
				item = GetNextItem (CurrentMenu, ItemNavigation.Previous);
				if (item != null)
					SelectItem (CurrentMenu, item, false);
				break;
			
			case Keys.Down:
				if (CurrentMenu is MainMenu) {
					if (CurrentMenu.SelectedItem != null && CurrentMenu.SelectedItem.IsPopup) {
						keynav_state = KeyNavState.Navigating;
						item = CurrentMenu.SelectedItem;
						ShowSubPopup (CurrentMenu, item);
						SelectItem (item, item.MenuItems [0], false);
						CurrentMenu = item;
						active = true;
						grab_control.ActiveTracker = this;
					}
					return true;
				}
				item = GetNextItem (CurrentMenu, ItemNavigation.Next);
				if (item != null)
					SelectItem (CurrentMenu, item, false);
				break;
			
			case Keys.Right:
				if (CurrentMenu is MainMenu) {
					item = GetNextItem (CurrentMenu, ItemNavigation.Next);
					bool popup = item.IsPopup && keynav_state != KeyNavState.NoPopups;
					SelectItem (CurrentMenu, item, popup);
					if (popup) {
						SelectItem (item, item.MenuItems [0], false);
						CurrentMenu = item;
					}
				} else if (CurrentMenu.SelectedItem.IsPopup) {
					item = CurrentMenu.SelectedItem;
					ShowSubPopup (CurrentMenu, item);
					SelectItem (item, item.MenuItems [0], false);
					CurrentMenu = item;
				} else if (CurrentMenu.parent_menu is MainMenu) {
					item = GetNextItem (CurrentMenu.parent_menu, ItemNavigation.Next);
					SelectItem (CurrentMenu.parent_menu, item, item.IsPopup);
					if (item.IsPopup) {
						SelectItem (item, item.MenuItems [0], false);
						CurrentMenu = item;
					}
				}
				break;
			
			case Keys.Left:
				if (CurrentMenu is MainMenu) {
					item = GetNextItem (CurrentMenu, ItemNavigation.Previous);
					bool popup = item.IsPopup && keynav_state != KeyNavState.NoPopups;
					SelectItem (CurrentMenu, item, popup);
					if (popup) {
						SelectItem (item, item.MenuItems [0], false);
						CurrentMenu = item;
					}
				} else if (CurrentMenu.parent_menu is MainMenu) {
					item = GetNextItem (CurrentMenu.parent_menu, ItemNavigation.Previous);
					SelectItem (CurrentMenu.parent_menu, item, item.IsPopup);
					if (item.IsPopup) {
						SelectItem (item, item.MenuItems [0], false);
						CurrentMenu = item;
					}
				} else {
					HideSubPopups (CurrentMenu);
					CurrentMenu = CurrentMenu.parent_menu;
				}
				break;

			case Keys.Return:
				if (CurrentMenu is MainMenu) {
					if (CurrentMenu.SelectedItem != null && CurrentMenu.SelectedItem.IsPopup) {
						keynav_state = KeyNavState.Navigating;
						item = CurrentMenu.SelectedItem;
						ShowSubPopup (CurrentMenu, item);
						SelectItem (item, item.MenuItems [0], false);
						CurrentMenu = item;
						active = true;
						grab_control.ActiveTracker = this;
					}
					return true;
				}
			
				ExecFocusedItem (CurrentMenu, CurrentMenu.SelectedItem);
				break;
				
			case Keys.Escape:
				Deactivate ();
				break;

			default:
				ProcessMnemonic (msg, keyData);
				break;
			}

			return false;
		}
	}

	internal class PopUpWindow : Control
	{
		private Menu menu;
		private Form form;

		public PopUpWindow (Form form, Menu menu): base ()
		{
			this.menu = menu;
			this.form = form;
			SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			SetStyle (ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);
			is_visible = false;
		}

		protected override CreateParams CreateParams
		{
			get {
				CreateParams cp = base.CreateParams;
				cp.Caption = "Menu PopUp";
				cp.Style = unchecked ((int)(WindowStyles.WS_POPUP));
				cp.ExStyle |= (int)(WindowExStyles.WS_EX_TOOLWINDOW | WindowExStyles.WS_EX_TOPMOST);
				return cp;
			}
		}

		public void ShowWindow ()
		{
			XplatUI.SetCursor(form.Handle, Cursors.Default.handle);
			RefreshItems ();
			Show ();
		}
		
		internal override void OnPaintInternal (PaintEventArgs args)
		{
			ThemeEngine.Current.DrawPopupMenu (args.Graphics, menu, args.ClipRectangle, ClientRectangle);
		}
		
		public void HideWindow ()
		{
			XplatUI.SetCursor (form.Handle, form.Cursor.handle);
			MenuTracker.HideSubPopups (menu);
    		Hide ();
		}

#if false
		protected override bool ProcessCmdKey (ref Message msg, Keys keyData)
		{	
			return MenuTracker.ProcessKeys (menu, ref msg, keyData, tracker);
		}
#endif

		protected override void CreateHandle ()
		{
			base.CreateHandle ();
			RefreshItems ();			
		}		
		
		// Called when the number of items has changed
		internal void RefreshItems ()
		{
			ThemeEngine.Current.CalcPopupMenuSize (DeviceContext, menu);

			if ((Location.X + menu.Rect.Width) > SystemInformation.WorkingArea.Width) {
				Location = new Point (Location.X - menu.Rect.Width, Location.Y);
			}
			if ((Location.Y + menu.Rect.Height) > SystemInformation.WorkingArea.Height) {
				Location = new Point (Location.X, Location.Y - menu.Rect.Height);
			}

			Width = menu.Rect.Width;
			Height = menu.Rect.Height;			
		}
		
		internal override bool ActivateOnShow { get { return false; } }
	}
}


