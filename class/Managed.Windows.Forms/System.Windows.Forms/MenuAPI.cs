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

// NOT COMPLETE

using System.Drawing;
using System.Drawing.Text;
using System.Collections;

namespace System.Windows.Forms
{

	/*
		This class mimics the Win32 API Menu functionality

		When writing this code the Wine project was of great help to
		understand the logic behind some Win32 issues. Thanks to them. Jordi,
	*/
	internal class MenuAPI
	{
		static ArrayList menu_list = new ArrayList ();		
		const int SEPARATOR_HEIGHT = 5;
		const int SM_CXBORDER = 1;
		const int SM_CYBORDER = 1;		
    		const int MENU_TAB_SPACE = 8;		// Pixels added to the width of an item because of a tab
    		const int MENU_BAR_ITEMS_SPACE = 8;	// Space between menu bar items

		public class MENU
		{
			public MF		Flags;		// Menu flags (MF_POPUP, MF_SYSMENU)
			public int		Width;		// Width of the whole menu
			public int		Height;		// Height of the whole menu
			public Control		Wnd;		// In a Popup menu is the PopupWindow and in a MenuBar the Form
			public ArrayList	items;		// Array of menu items
			public int		FocusedItem;	// Currently focused item
			public IntPtr		hParent;			
			public MENUITEM		SelectedItem;	// Currently selected item
			public bool 		bMenubar;
			public bool		bTracking;
			public Menu		menu;		// SWF.Menu 

			public MENU (Menu menu_obj)
			{
				Wnd = null;
				hParent = IntPtr.Zero;
				items = new ArrayList ();
				Flags = MF.MF_INSERT;
				Width = Height = FocusedItem = 0;				
				bMenubar = false;
				bTracking = false;
				menu = menu_obj;
			}
		}

		public class MENUITEM
		{
			public	MenuItem 	item;
			public  Rectangle	rect;			
			public 	int    		wID;
			public	IntPtr		hSubMenu;			
			public  int 		pos;	/* Position in the menuitems array*/

			public MENUITEM ()
			{				
				wID = 0;
				pos = 0;
				rect = new Rectangle ();
			}

		};

		public class TRACKER
		{
		    	public 	IntPtr	hCurrentMenu;
		    	public	IntPtr	hTopMenu;

		    	public TRACKER ()
			{
				hCurrentMenu = hTopMenu = IntPtr.Zero;
			}
		};

		public enum MenuMouseEvent
		{
			Down,
			Move,
		}

		internal enum ItemNavigation
		{
			First,
			Last,
			Next,
			Previous,
		}

		internal enum MF
		{
			MF_INSERT           = 0x0,
			MF_APPEND           = 0x100,
			MF_DELETE           = 0x200,
			MF_REMOVE           = 0x1000,
			MF_BYCOMMAND        = 0,
			MF_BYPOSITION       = 0x400,
			MF_SEPARATOR        = 0x800,
			MF_ENABLED          = 0,
			MF_GRAYED           = 1,
			MF_DISABLED         = 2,
			MF_UNCHECKED        = 0,
			MF_CHECKED          = 8,
			MF_USECHECKBITMAPS  = 0x200,
			MF_STRING           = 0,
			MF_BITMAP           = 4,
			MF_OWNERDRAW        = 0x100,
			MF_POPUP            = 0x10,
			MF_MENUBARBREAK     = 0x20,
			MF_MENUBREAK        = 0x40,
			MF_UNHILITE         = 0,
			MF_HILITE           = 0x80,
			MF_DEFAULT          = 0x1000,
			MF_SYSMENU          = 0x2000,
			MF_HELP             = 0x4000,
			MF_RIGHTJUSTIFY     = 0x4000,
			MF_MENUBAR	    = 0x8000	// Internal
		}

		static MenuAPI ()
		{
			
		}		
		
		static public IntPtr StoreMenuID (MENU menu)
		{
			int id = menu_list.Add (menu);
			return (IntPtr)(id + 1);
		}

		static public MENU GetMenuFromID (IntPtr ptr)
		{
			int id = (int)ptr;
			id = id - 1;

			if (menu_list[id] == null) 	// It has been delete it
				return null;

			return (MENU) menu_list[id];
		}

		static public IntPtr CreateMenu (Menu menu_obj)
		{
			MENU menu = new MENU (menu_obj);			
			return StoreMenuID (menu);
		}

		static public IntPtr CreatePopupMenu (Menu menu_obj)
		{
			MENU popMenu = new MENU (menu_obj);
			popMenu.Flags |= MF.MF_POPUP;			
			return StoreMenuID (popMenu);
		}

		static public int InsertMenuItem (IntPtr hMenu, int uItem, bool fByPosition, MenuItem item, ref IntPtr hSubMenu)
		{
			int id;
			
			if (fByPosition == false)
				throw new NotImplementedException ();

			MENU menu = GetMenuFromID (hMenu);
			if ((uint)uItem > menu.items.Count)
				uItem =  menu.items.Count;

			MENUITEM menu_item = new MENUITEM ();
			menu_item.item = item;

			if (item.IsPopup) {
				menu_item.hSubMenu = CreatePopupMenu (menu_item.item);
				MENU submenu = GetMenuFromID (menu_item.hSubMenu);
				submenu.hParent = hMenu;
			}
			else
				menu_item.hSubMenu = IntPtr.Zero;

			hSubMenu = menu_item.hSubMenu;
			id = menu.items.Count;
			menu_item.pos = menu.items.Count;
			menu.items.Insert (uItem, menu_item);

			return id;
		}

		// The Point object contains screen coordinates
		static public bool TrackPopupMenu (IntPtr hTopMenu, IntPtr hMenu, Point pnt, bool bMenubar, Control Wnd)
		{
			TRACKER	tracker = new TRACKER ();			
			MENU top_menu = GetMenuFromID (hTopMenu);
			MENU menu = null;

			if (hMenu == IntPtr.Zero)	// No submenus to track
				return true;				

			menu = GetMenuFromID (hMenu);			
			
			menu.Wnd = new PopUpWindow (hMenu, tracker);
			tracker.hCurrentMenu = hMenu;
			tracker.hTopMenu = hTopMenu;

			MENUITEM select_item = GetNextItem (hMenu, ItemNavigation.First);

			if (select_item != null) {
				MenuAPI.SelectItem (hMenu, select_item, false, tracker);
			}

			// Make sure the menu is always visible and does not 'leave' the screen
			// What is menu.Width/Height? It seemed to be 0/0
			if ((pnt.X + menu.Wnd.Width) > SystemInformation.WorkingArea.Width) {
				pnt.X -= menu.Wnd.Width;
			}

			if ((pnt.X + menu.Wnd.Height) > SystemInformation.WorkingArea.Height) {
				pnt.Y -= menu.Wnd.Height;
			}

			menu.Wnd.Location =  menu.Wnd.PointToClient (pnt);
				
			if (menu.menu.IsDirty) {				
				menu.items.Clear ();
				menu.menu.CreateItems ();
				((PopUpWindow)menu.Wnd).RefreshItems ();
				menu.menu.IsDirty = false;
			}
			
			((PopUpWindow)menu.Wnd).ShowWindow ();

			Application.Run ();

			if (menu.Wnd == null) {
				menu.Wnd.Dispose ();
				menu.Wnd = null;
			}

			return true;
		}

		/*
			Menu drawing API
		*/

		static public void CalcItemSize (Graphics dc, MENUITEM item, int y, int x, bool menuBar)
		{
			item.rect.Y = y;
			item.rect.X = x;		

			if (item.item.Visible == false)
				return;

			if (item.item.Separator == true) {
				item.rect.Height = SEPARATOR_HEIGHT / 2;
				item.rect.Width = -1;
				return;
			}
			
			if (item.item.MeasureEventDefined) {
				MeasureItemEventArgs mi = new MeasureItemEventArgs (dc, item.pos);
				item.item.PerformMeasureItem (mi);
				item.rect.Height = mi.ItemHeight;
				item.rect.Width = mi.ItemWidth;
				return;
			} else {		

				SizeF size;
				size =  dc.MeasureString (item.item.Text, ThemeEngine.Current.MenuFont);
				item.rect.Width = (int) size.Width;
				item.rect.Height = (int) size.Height;
	
				if (!menuBar) {
	
					if (item.item.Shortcut != Shortcut.None && item.item.ShowShortcut) {
						item.item.XTab = ThemeEngine.Current.MenuCheckSize.Width + MENU_TAB_SPACE + (int) size.Width;
						size =  dc.MeasureString (" " + item.item.GetShortCutText (), ThemeEngine.Current.MenuFont);
						item.rect.Width += MENU_TAB_SPACE + (int) size.Width;
					}
	
					item.rect.Width += 4 + (ThemeEngine.Current.MenuCheckSize.Width * 2);
				}
				else {
					item.rect.Width += MENU_BAR_ITEMS_SPACE;
					x += item.rect.Width;
				}
	
				if (item.rect.Height < ThemeEngine.Current.MenuHeight)
					item.rect.Height = ThemeEngine.Current.MenuHeight;
				}
		}

		static public void CalcPopupMenuSize (Graphics dc, IntPtr hMenu)
		{
			int x = 3;
			int start = 0;
			int i, n, y, max;

			MENU menu = GetMenuFromID (hMenu);
			menu.Height = 0;

			while (start < menu.items.Count) {
				y = 2;
				max = 0;
				for (i = start; i < menu.items.Count; i++) {
					MENUITEM item = (MENUITEM) menu.items[i];

					if ((i != start) && (item.item.Break || item.item.BarBreak))
						break;

					CalcItemSize (dc, item, y, x, false);
					y += item.rect.Height;

					if (item.rect.Width > max)
						max = item.rect.Width;
				}

				// Reemplace the -1 by the menu width (separators)
				for (n = start; n < i; n++, start++) {
					MENUITEM item = (MENUITEM) menu.items[n];
					item.rect.Width = max;
				}

				if (y > menu.Height)
					menu.Height = y;

				x+= max;
			}

			menu.Width = x;

			//space for border
			menu.Width += 2;
			menu.Height += 2;

			menu.Width += SM_CXBORDER;
    			menu.Height += SM_CYBORDER;
		}


		static public void DrawPopupMenu (Graphics dc, IntPtr hMenu, Rectangle cliparea, Rectangle rect)
		{
			MENU menu = GetMenuFromID (hMenu);

			dc.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush
				(ThemeEngine.Current.ColorMenu), cliparea);

			/* Draw menu borders */
			dc.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorHilightText),
				rect.X, rect.Y, rect.X + rect.Width, rect.Y);

			dc.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorHilightText),
				rect.X, rect.Y, rect.X, rect.Y + rect.Height);

			dc.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonShadow),
				rect.X + rect.Width - 1 , rect.Y , rect.X + rect.Width - 1, rect.Y + rect.Height);

			dc.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonDkShadow),
				rect.X + rect.Width, rect.Y , rect.X + rect.Width, rect.Y + rect.Height);

			dc.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonShadow),
				rect.X , rect.Y + rect.Height - 1 , rect.X + rect.Width - 1, rect.Y + rect.Height -1);

			dc.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonDkShadow),
				rect.X , rect.Y + rect.Height, rect.X + rect.Width - 1, rect.Y + rect.Height);

			for (int i = 0; i < menu.items.Count; i++)
				if (cliparea.IntersectsWith (((MENUITEM) menu.items[i]).rect)) {
					MENUITEM it = (MENUITEM) menu.items[i];
					it.item.MenuHeight = menu.Height;
					it.item.PerformDrawItem (new DrawItemEventArgs (dc, ThemeEngine.Current.MenuFont,
						it.rect, i, it.item.Status));
			}
		}

		// Updates the menu rect and returns the height
		static public int MenuBarCalcSize (Graphics dc, IntPtr hMenu, int width)
		{
			int x = 0;
			int i = 0;
			int y = 0;
			MENU menu = GetMenuFromID (hMenu);
			menu.Height = 0;
			MENUITEM item;

			while (i < menu.items.Count) {

				item = (MENUITEM) menu.items[i];
				CalcItemSize (dc, item, y, x, true);
				i = i + 1;

				if (x + item.rect.Width > width) {
					item.rect.X = 0;
					y += item.rect.Height;
					item.rect.Y = y;
					x = 0;
				}

				x += item.rect.Width;
				item.item.MenuBar = true;				

				if (y + item.rect.Height > menu.Height)
					menu.Height = item.rect.Height + y;
			}

			menu.Width = width;			
			return menu.Height;
		}

		// Draws a menu bar in a Window
		static public void DrawMenuBar (Graphics dc, IntPtr hMenu, Rectangle rect)
		{
			MENU menu = GetMenuFromID (hMenu);			
			Rectangle item_rect;

			if (menu.Height == 0)
				MenuBarCalcSize (dc, hMenu, rect.Width);								
			
			for (int i = 0; i < menu.items.Count; i++) {
				MENUITEM it = (MENUITEM) menu.items[i];
				item_rect = it.rect;
				item_rect.X += rect.X;
				item_rect.Y += rect.Y;
				it.item.MenuHeight = menu.Height;
				it.item.PerformDrawItem (new DrawItemEventArgs (dc, ThemeEngine.Current.MenuFont,
						item_rect, i, it.item.Status));			
				
			}				
		}

		/*
			Menu handeling API
		*/
		
		static public Point ClientAreaPointToScreen (Control wnd, Point pnt)		
		{
			Point rslt;
			pnt.Y -= ThemeEngine.Current.CaptionHeight;
			rslt = wnd.PointToScreen (pnt);			
			return rslt;
		}
		
		static public Point ClientAreaPointToClient (Control wnd, Point pnt)		
		{
			Point rslt;			
			rslt = wnd.PointToClient (pnt);			
			rslt.Y += ThemeEngine.Current.CaptionHeight;
			return rslt;
		}	

		static public MENUITEM FindItemByCoords (IntPtr hMenu, Point pt)
		{
			MENU menu = GetMenuFromID (hMenu);

			for (int i = 0; i < menu.items.Count; i++) {
				MENUITEM item = (MENUITEM) menu.items[i];
				if (item.rect.Contains (pt)) {
					return item;
				}
			}

			return null;
		}
		
		// Get the current selected item
		static public MENUITEM GetSelected (IntPtr hMenu)
		{
			MENU menu = GetMenuFromID (hMenu);			
			MENUITEM it;
			
			/* Loop all items */
			for (int i = 0; i < menu.items.Count; i++) {
				it = (MENUITEM) menu.items[i];			
				if ((it.item.Status & DrawItemState.Selected) == DrawItemState.Selected) {
					return it;
				}				
			}
			
			return null;
		}
		
		static public void UnSelectItem (IntPtr hMenu, MENUITEM item)
		{			
			MENU menu = GetMenuFromID (hMenu);
			
			if (item == null)
				return;				
			
			item.item.Status = item.item.Status &~ DrawItemState.Selected;
			menu.Wnd.Invalidate (item.rect);
		}

		// Select the item and unselect the previous selecte item
		static public void SelectItem (IntPtr hMenu, MENUITEM item, bool execute, TRACKER tracker)
		{
			MENU menu = GetMenuFromID (hMenu);
			MENUITEM previous_selitem = GetSelected (hMenu);
			
			/* Already selected */
			if (previous_selitem != null && item.rect == previous_selitem.rect) {
				return;
			}

			UnSelectItem (hMenu, previous_selitem);
			
			// If the previous item had subitems, hide them
			if (previous_selitem != null && previous_selitem.item.IsPopup)
				HideSubPopups (hMenu);

			if (tracker.hCurrentMenu != hMenu) {
				menu.Wnd.Capture = true;
				tracker.hCurrentMenu = hMenu;
			}

			menu.SelectedItem = item;
			item.item.Status |= DrawItemState.Selected;			
			menu.Wnd.Invalidate (item.rect);

			item.item.PerformSelect ();					
			
			if (execute)
				ExecFocusedItem (hMenu, item, tracker);
		}


		//	Used when the user executes the action of an item (press enter, shortcut)
		//	or a sub-popup menu has to be shown
		static public void ExecFocusedItem (IntPtr hMenu, MENUITEM item, TRACKER tracker)
		{
			if (item.item.Enabled == false)
			 	return;			 	
			
			if (item.item.IsPopup) {				
				ShowSubPopup (hMenu, item.hSubMenu, item, tracker);
			}
			else {
				// Execute function
			}
		}

		// Create a popup window and show it or only show it if it is already created
		static public void ShowSubPopup (IntPtr hParent, IntPtr hMenu, MENUITEM item, TRACKER tracker)
		{
			MENU menu = GetMenuFromID (hMenu);
			Point pnt = new Point ();

			if (item.item.Enabled == false)
				return;

			MENU menu_parent = GetMenuFromID (hParent);
			((PopUpWindow)menu_parent.Wnd).LostFocus ();
			tracker.hCurrentMenu = hMenu;

			if (menu.Wnd == null)
				menu.Wnd = new PopUpWindow (hMenu, tracker);
			
			pnt.X = item.rect.X + item.rect.Width;
			pnt.Y = item.rect.Y + 1;
			pnt = menu_parent.Wnd.PointToScreen (pnt);
			menu.Wnd.Location = pnt;

			MENUITEM select_item = GetNextItem (hMenu, ItemNavigation.First);

			if (select_item != null)
				MenuAPI.SelectItem (hMenu, select_item, false, tracker);

			((PopUpWindow)menu.Wnd).ShowWindow ();
		}

		/* Hides all the submenus open in a menu */
		static public void HideSubPopups (IntPtr hMenu)
		{
			MENU menu = GetMenuFromID (hMenu);
			MENUITEM item;						
			
			for (int i = 0; i < menu.items.Count; i++) {
				item = (MENUITEM) menu.items[i];
				if (!item.item.IsPopup)
					continue;

				MENU sub_menu = GetMenuFromID (item.hSubMenu);

				if (sub_menu.Wnd != null) {
					HideSubPopups (item.hSubMenu);					
					((PopUpWindow)sub_menu.Wnd).Hide ();
				}
			}
		}

		static public void DestroyMenu (IntPtr hMenu)
		{
			if (hMenu == IntPtr.Zero)
				return;				

			MENU menu = GetMenuFromID (hMenu);
			MENUITEM item;

			for (int i = 0; i < menu.items.Count; i++) {
				item = (MENUITEM) menu.items[i];
				if (item.item.IsPopup) {
					MENU sub_menu = GetMenuFromID (item.hSubMenu);
					if (sub_menu != null && sub_menu.Wnd != null) 
						HideSubPopups (item.hSubMenu);
						
					DestroyMenu (item.hSubMenu);					
				}
			}

			// Do not destroy the window of a Menubar
			if (menu.Wnd != null && menu.bMenubar == false) {
				((PopUpWindow)menu.Wnd).Dispose ();
				menu.Wnd = null;			
			}
			
			/* Unreference from the array list */
			menu_list[((int)hMenu)-1] = null;
		}
		
		// Find item by screen coordinates
		static public bool FindSubItemByCoord (IntPtr hMenu, Point pnt, ref IntPtr hMenuItem, ref MENUITEM itemfound)
		{		
			Point pnt_client;	
			Rectangle rect;
			MENU menu = GetMenuFromID (hMenu);
			MENUITEM item;
			
			for (int i = 0; i < menu.items.Count; i++) {
				item = (MENUITEM) menu.items[i];
				
				if (item.item.IsPopup)
					if (FindSubItemByCoord (item.hSubMenu, pnt, ref hMenuItem, ref itemfound) == true)
						return true;
					
				if (menu.Wnd == null) // Menu has not been created yet
					continue;					
								
				rect = item.rect;
				pnt_client = menu.Wnd.PointToScreen (new Point (item.rect.X, item.rect.Y));
				rect.X = pnt_client.X;
				rect.Y = pnt_client.Y;
				
				if (rect.Contains (pnt) == true) {
					itemfound = item;
					hMenuItem = hMenu;
					return true;
				}
			}			
			
			return false;
		}

		static public void SetMenuBarWindow (IntPtr hMenu, Control wnd)
		{
			MENU menu = GetMenuFromID (hMenu);
			menu.Wnd = wnd;
			menu.bMenubar = true;
		}

		static private void MenuBarMove (IntPtr hMenu, MENUITEM item, TRACKER tracker)
		{
			MENU menu = GetMenuFromID (hMenu);
			Point pnt = new Point (item.rect.X, item.rect.Y + item.rect.Height);
			pnt = ClientAreaPointToScreen (menu.Wnd, pnt);
			MenuAPI.SelectItem (hMenu, item, false, tracker);
			HideSubPopups (tracker.hCurrentMenu);
			tracker.hCurrentMenu = hMenu;
			MenuAPI.TrackPopupMenu (hMenu, item.hSubMenu, pnt, false, null);
		}

		// Function that process all menubar mouse events
		static public void TrackBarMouseEvent (IntPtr hMenu, Control wnd, MouseEventArgs e, MenuMouseEvent eventype, TRACKER tracker)
		{
			MENU menu = GetMenuFromID (hMenu);

			switch (eventype) {
				case MenuMouseEvent.Down: {/* Coordinates in screen position*/

					Point pnt = new Point (e.X, e.Y);
					pnt = ClientAreaPointToClient (menu.Wnd, pnt);

					MenuAPI.MENUITEM item = MenuAPI.FindItemByCoords (hMenu, pnt);

					if (item != null) {
						MENU top_menu = GetMenuFromID (tracker.hTopMenu);
						
						top_menu.bTracking = true;
						//item.Y += ThemeEngine.Current.CaptionHeight;
						MenuBarMove (hMenu, item, tracker);
		
						if (item != null) {
							item.item.PerformClick ();			
						}
					}

					break;
				}

				case MenuMouseEvent.Move: { /* Coordinates in screen position*/

					if (tracker.hTopMenu != IntPtr.Zero && tracker.hCurrentMenu != IntPtr.Zero) {
						
						MENU top_menu = GetMenuFromID (tracker.hTopMenu);
						
						if (top_menu.bTracking == false)
							break;

						Point pnt = new Point (e.X, e.Y);
						//pnt = menu.Wnd.PointToClient (pnt);
						pnt = ClientAreaPointToClient (menu.Wnd, pnt);

						MenuAPI.MENUITEM item = MenuAPI.FindItemByCoords (hMenu, pnt);

						if (item != null && menu.SelectedItem != item)
							MenuBarMove (hMenu, item, tracker);
					}
					break;
				}

				default:
					break;
			}
		}

		static public MENUITEM FindItemByKey (IntPtr hMenu, IntPtr key)
		{
			MENU menu = GetMenuFromID (hMenu);
			MENUITEM item;

			char key_char = (char ) (key.ToInt32() & 0xff);
			key_char = Char.ToUpper (key_char);

			for (int i = 0; i < menu.items.Count; i++) {
				item = (MENUITEM) menu.items[i];

				if (item.item.Mnemonic == '\0')
					continue;

				if (item.item.Mnemonic == key_char)
					return item;
			}

			return null;
		}

		// Get the next or previous selectable item on a menu
		static public MENUITEM GetNextItem (IntPtr hMenu, ItemNavigation navigation)
		{
			MENU menu = GetMenuFromID (hMenu);
			int pos = 0;
			bool selectable_items = false;
			MENUITEM item;

			// Check if there is at least a selectable item
			for (int i = 0; i < menu.items.Count; i++) {
				item = (MENUITEM)menu.items[i];
				if (item.item.Separator == false && item.item.Visible == true) {
					selectable_items = true;
					break;
				}
			}

			if (selectable_items == false)
				return null;

			switch (navigation) {
			case ItemNavigation.First: {
				pos = 0;

				/* Next item that is not separator and it is visible*/
				for (; pos < menu.items.Count; pos++) {
					item = (MENUITEM)menu.items[pos];
					if (item.item.Separator == false && item.item.Visible == true)
						break;
				}

				if (pos >= menu.items.Count) { /* Jump at the start of the menu */
					pos = 0;
					/* Next item that is not separator and it is visible*/
					for (; pos < menu.items.Count; pos++) {
						item = (MENUITEM)menu.items[pos];
						if (item.item.Separator == false && item.item.Visible == true)
							break;
					}
				}

				break;
			}

			case ItemNavigation.Last: { // Not used
				break;
			}

			case ItemNavigation.Next: {

				if (menu.SelectedItem != null)
					pos = menu.SelectedItem.pos;

				/* Next item that is not separator and it is visible*/
				for (pos++; pos < menu.items.Count; pos++) {
					item = (MENUITEM)menu.items[pos];
					if (item.item.Separator == false && item.item.Visible == true)
						break;
				}

				if (pos >= menu.items.Count) { /* Jump at the start of the menu */
					pos = 0;
					/* Next item that is not separator and it is visible*/
					for (; pos < menu.items.Count; pos++) {
						item = (MENUITEM)menu.items[pos];
						if (item.item.Separator == false && item.item.Visible == true)
							break;
					}
				}
				break;
			}

			case ItemNavigation.Previous: {

				if (menu.SelectedItem != null)
					pos = menu.SelectedItem.pos;

				/* Previous item that is not separator and it is visible*/
				for (pos--; pos >= 0; pos--) {
					item = (MENUITEM)menu.items[pos];
					if (item.item.Separator == false && item.item.Visible == true)
						break;
				}

				if (pos < 0 ) { /* Jump at the end of the menu*/
					pos = menu.items.Count - 1;
					/* Previous item that is not separator and it is visible*/
					for (; pos >= 0; pos--) {
						item = (MENUITEM)menu.items[pos];
						if (item.item.Separator == false && item.item.Visible == true)
							break;
					}
				}

				break;
			}

			default:
				break;
			}

			return (MENUITEM)menu.items[pos];
		}

		static public bool ProcessKeys (IntPtr hMenu, ref Message msg, Keys keyData, TRACKER tracker)
		{
			MENU menu = GetMenuFromID (hMenu);
			MENUITEM item;

			switch (keyData) {
				case Keys.Up: {
					item = GetNextItem (hMenu, ItemNavigation.Previous);
					if (item != null)
						MenuAPI.SelectItem (hMenu, item, false, tracker);

					break;
				}

				case Keys.Down: {
					item = GetNextItem (hMenu, ItemNavigation.Next);

					if (item != null)
						MenuAPI.SelectItem (hMenu, item, false, tracker);
					break;
				}

				/* Menubar selects and opens next. Popups next or open*/
				case Keys.Right: {

					// Try to Expand popup first
					if (menu.SelectedItem.item.IsPopup) {
						ShowSubPopup (hMenu, menu.SelectedItem.hSubMenu, menu.SelectedItem, tracker);
					} else {

						MENU parent = null;
						if (menu.hParent != IntPtr.Zero)
							parent = GetMenuFromID (menu.hParent);

						if (parent != null && parent.bMenubar == true) {
							MENUITEM select_item = GetNextItem (menu.hParent, ItemNavigation.Next);
							MenuBarMove (menu.hParent, select_item, tracker);
						}
					}

					break;
				}

				case Keys.Left: {

					// Try to Collapse popup first
					if (menu.SelectedItem.item.IsPopup) {

					} else {

						MENU parent = null;
						if (menu.hParent != IntPtr.Zero)
							parent = GetMenuFromID (menu.hParent);

						if (parent != null && parent.bMenubar == true) {
							MENUITEM select_item = GetNextItem (menu.hParent, ItemNavigation.Previous);
							MenuBarMove (menu.hParent, select_item, tracker);
						}
					}

					break;
				}
				
				case Keys.Return: {					
					MenuAPI.ExecFocusedItem (hMenu, menu.SelectedItem, tracker);
					break;
				}

				default:
					break;
			}

			/* Try if it is a menu hot key */
			item = MenuAPI.FindItemByKey (hMenu, msg.WParam);

			if (item != null) {
				MenuAPI.SelectItem (hMenu, item, false, tracker);
				return true;
			}

			return false;
		}
	}

	/*

		class PopUpWindow

	*/
	internal class PopUpWindow : Control
	{
		private IntPtr hMenu;
		private MenuAPI.TRACKER tracker;

		public PopUpWindow (IntPtr hMenu, MenuAPI.TRACKER tracker): base ()
		{
			this.hMenu = hMenu;
			this.tracker = tracker;
			MouseDown += new MouseEventHandler (OnMouseDownPUW);
			MouseMove += new MouseEventHandler (OnMouseMovePUW);
			MouseUp += new MouseEventHandler (OnMouseUpPUW);
			Paint += new PaintEventHandler (OnPaintPUW);
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
				cp.ExStyle |= (int)(WindowStyles.WS_EX_TOOLWINDOW | WindowStyles.WS_EX_TOPMOST);
				return cp;
			}
		}

		public void ShowWindow ()
		{
			Show ();
			Capture = true;
			Refresh ();
		}
		
		public new void LostFocus ()
		{			
			Capture = false;
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
		}

		private void OnPaintPUW (Object o, PaintEventArgs pevent)
		{
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;

			Draw (pevent.ClipRectangle);
			pevent.Graphics.DrawImage (ImageBuffer, pevent.ClipRectangle, pevent.ClipRectangle, GraphicsUnit.Pixel);
		}
		
		public void HideWindow ()
		{
			Capture = false;
    			Hide ();
    			MenuAPI.MENU top_menu = MenuAPI.GetMenuFromID (tracker.hTopMenu);
			top_menu.bTracking = false;
			
			MenuAPI.HideSubPopups (tracker.hTopMenu);
			
			if (top_menu.bMenubar) {
				MenuAPI.MENUITEM item = MenuAPI.GetSelected (tracker.hTopMenu);
			
				if (item != null) {
					MenuAPI.UnSelectItem (tracker.hTopMenu, item);
				}
			} else { // Context Menu				
				((PopUpWindow)top_menu.Wnd).Hide ();
			}
		}

		private void OnMouseDownPUW (object sender, MouseEventArgs e)
    		{    			
    			/* Click outside the client area*/
    			if (ClientRectangle.Contains (e.X, e.Y) == false) {
    				HideWindow ();
    			}
		}

		private void OnMouseUpPUW (object sender, MouseEventArgs e)
    		{
    			/* Click in an item area*/
			MenuAPI.MENUITEM item = MenuAPI.FindItemByCoords (hMenu, new Point (e.X, e.Y));

			if (item != null) {
				item.item.PerformClick ();
				HideWindow ();				
			}
		}

		private void OnMouseMovePUW (object sender, MouseEventArgs e)
		{	
			MenuAPI.MENUITEM item = MenuAPI.FindItemByCoords (hMenu, new Point (e.X, e.Y));
			
			if (item != null) {				
				MenuAPI.SelectItem (hMenu, item, true, tracker);
			} else {					

				MenuAPI.MENU menu_parent = null;

				if (tracker.hTopMenu != IntPtr.Zero)
					menu_parent = MenuAPI.GetMenuFromID (tracker.hTopMenu);
					
				if (menu_parent == null) 
					return;
				
				if (menu_parent.bMenubar) {
					MenuAPI.TrackBarMouseEvent (tracker.hTopMenu,
						this, new MouseEventArgs(e.Button, e.Clicks, MousePosition.X, MousePosition.Y, e.Delta),
						MenuAPI.MenuMouseEvent.Move, tracker);
				}				
					
				IntPtr hMenuItem = IntPtr.Zero;
				MenuAPI.MENUITEM item_found = null;
				
				if (MenuAPI.FindSubItemByCoord (tracker.hTopMenu, MousePosition, ref hMenuItem, ref item_found) == false)
					return;
				
				MenuAPI.SelectItem (hMenuItem, item_found, false, tracker);
			}
		}

		protected override bool ProcessCmdKey (ref Message msg, Keys keyData)
		{	
			return MenuAPI.ProcessKeys (hMenu, ref msg, keyData, tracker);
		}

		protected override void CreateHandle ()
		{
			base.CreateHandle ();
			RefreshItems ();			
		}		
		
		// Called when the number of items has changed
		internal void RefreshItems ()
		{
			MenuAPI.MENU menu = MenuAPI.GetMenuFromID (hMenu);
			MenuAPI.CalcPopupMenuSize (DeviceContext, hMenu);

			Width = menu.Width;
			Height = menu.Height;			
		}

		private void Draw (Rectangle clip)
		{
			MenuAPI.DrawPopupMenu  (DeviceContext, hMenu, clip, ClientRectangle);
		}
	}

}

