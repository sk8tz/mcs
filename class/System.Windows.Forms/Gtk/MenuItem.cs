//		
//			System.Windows.Forms.MenuItem
//
//			Author: 
//						Joel Basson		(jstrike@mweb.co.za)
//
//

using System.Drawing;

namespace System.Windows.Forms{

	public class MenuItem : Control {

		public MenuItemCollection MenuItems;
		internal Gtk.MenuItem file_item;
		String text, text2;
		int check1;
		bool blChecked;
		
		public class MenuItemCollection{

			MenuItem owner;
			Gtk.Menu file_menu;

			public MenuItemCollection (MenuItem owner) {
		
				this.owner = owner;
				file_menu = new Gtk.Menu(); 
			}

			public void Add (MenuItem items) {

				file_menu.Append (items.file_item);
				owner.file_item.Submenu = file_menu;
			}
		
			public void AddRange(MenuItem[] items) {
				
				foreach (MenuItem m in items)
					{file_menu.Append (m.file_item);
					owner.file_item.Submenu = file_menu;}
				
			}

		}

		public MenuItem() : base (){
			this.MenuItems = new MenuItemCollection(this);
	
			CreateMenuItem();
		}

		public override String Text{
			get{
				return text2;
			}
			set{
				text2 = value; 
				if (text == null){
					text = value.Replace("&", "_");
					CreateMenuItem();
				}	
			}
		}	
		
		private void CreateMenuItem (){
			if (check1 != 1) {
				file_item = new Gtk.MenuItem(text);			
				file_item.Show();
			}
			else{
			CreateCheckMenu();	
			}				
		}
		
		public Shortcut Shortcut {
			get{
				throw new NotImplementedException ();
			}
			set{
				ShortcutHelper.AddShortcutToWidget (file_item, new Gtk.AccelGroup(), Shortcut.CtrlN, "activate");
			}
		}
		
		public bool Checked {	
			get{
				return blChecked;
			}
			set{
				if (check1 != 1){  
					file_item = new Gtk.CheckMenuItem(text);
					file_item.Show();
					check1 = 1;
				}
				((Gtk.CheckMenuItem)file_item).Active = value;
				blChecked = value;
			}
		}

		private void CreateCheckMenu () {
			file_item = new Gtk.CheckMenuItem(text);
			file_item.Show();
			((Gtk.CheckMenuItem)file_item).Active = blChecked;
			check1 = 1;
		}

	}

}
