//		
//			System.Windows.Forms Demo2 app
//
//			Author: 
//						Joel Basson		(jstrike@mweb.co.za)
//
//

using System;
using System.Drawing;
using System.Windows.Forms;
 
namespace demo2
{
	
	public class GtkForm : System.Windows.Forms.Form
  	{
	    	private Button button1 = new Button(); 
		private Button button2 = new Button();
		private ColorDialog color1 = new ColorDialog();
		private ComboBox combo1 = new ComboBox();
		private MainMenu mainMenu1 = new MainMenu();
		private MenuItem menuItem1 = new MenuItem();
		private MenuItem menuItem2 = new MenuItem();
		private MenuItem menuItem3 = new MenuItem();
		private MenuItem menuItem4 = new MenuItem();
		private MenuItem menuItem5 = new MenuItem();
 
		private void InitializeWidgets()
		{
			button1.Location = new Point(150, 28);
			button1.Name = "button1";
			button1.Size = new Size(128, 44);
			button1.Text = "Color";
			button1.Click += new EventHandler(this.button1_Click);    
		 	button1.Enabled = true;

			button2.Location = new Point(150, 80);
			button2.Name = "button2";
			button2.Size = new Size(128, 44);
			button2.Text = "Add to ComboBox";
			button2.Click += new EventHandler(this.button2_Click);    
		 	button2.Enabled = true;

			menuItem1.Text = "File";
			menuItem2.Text = "Exit";
			menuItem3.Text = "Edit";
			menuItem4.Text = "&Stop";
			menuItem4.Shortcut = Shortcut.CtrlN;
			menuItem5.Text = "&Start";
			menuItem5.Checked = true;
			menuItem1.MenuItems.Add(menuItem2);
			menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {menuItem4,
							       menuItem5});	
			mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {menuItem1,
							    menuItem3});
    
			combo1.Location = new Point(150, 150);
			this.combo1.Items.AddRange(new object[] {"Item 1",
				"Item 2",
				"Item 3",
				"Item 4",
				"Item 5"});

			this.Controls.AddRange(new System.Windows.Forms.Control[] { 
									    this.button1,
									    this.button2,
									    this.color1,
									    this.combo1});

			this.Size = new Size(512, 250);
			this.Menu = mainMenu1;
		}

    	public GtkForm()
    	{
    	   	InitializeWidgets();

    	}

		private void button1_Click(object sender, EventArgs e){ 
			color1.ShowDialog();			
  		}

		private void button2_Click(object sender, EventArgs e){ 
			combo1.BeginUpdate();
			for (int i = 1; i < 5; i++) {combo1.Items.Add("Joel");}
			combo1.EndUpdate();	
  		}

	 }
	
	public class GtkMain
	{
		public static void Main()
		{
			GtkForm form1 = new GtkForm ();
			form1.Text = "System.Windows.Forms at work!";			
			Application.Run(form1);
		}
	}
}
