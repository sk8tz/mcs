//
// Test application for the ListView class implementation
//
// Author:
//   Jordi Mas i Hern�ndez, jmas@softcatala.org
//	


using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;



public class MyListViewForm : System.Windows.Forms.Form
{
	ColumnHeader	column1 = null;
	ColumnHeader	column2 = null;
	ColumnHeader	column3 = null;
	ColumnHeader	column4 = null;
	myListView listViewCtrl = null;
	ListView.SelectedListViewItemCollection sel = null;
	int nColInserted = 100;
	
	public static void Main(string[] args)
	{
		Application.Run(new MyListViewForm());
	}
	
	// Clear all columns
	public void ClearColumnsButton()
	{
		listViewCtrl.Columns.Clear();
	}
	
	public void ShowColumnsButton()
	{
		string sTxt = "";
		// How the elements are order once an element in deleted
		for (int i=0; i < listViewCtrl.Columns.Count; i++)
			sTxt+=("Column: \"" +  listViewCtrl.Columns[i].Text + "\" idx: " + listViewCtrl.Columns[i].Index + " witdh:"+listViewCtrl.Columns[i].Width + "\r");				
			
		MessageBox.Show(sTxt);
	}
	
	
	public void AddColumnsButton()
	{		
		string sColText;		
		sColText = "Column " + nColInserted;					
		
		Console.WriteLine ("AddColumnsButton->" + sColText);
		
		listViewCtrl.Columns.Insert(0, sColText, 150, HorizontalAlignment.Left);		
		nColInserted++;		
	}
	
	public void ClearButton()
	{	
			
		Console.WriteLine ("MyListViewForm.Clear");				
		listViewCtrl.Clear();		
		
		// How the elements are order once an element in deleted
		for (int i=0; i < listViewCtrl.Columns.Count; i++)
			Console.WriteLine ("Column " +  listViewCtrl.Columns[i].Text + " idx: " + listViewCtrl.Columns[i].Index);		
					
		// Items
		for (int i=0; i < listViewCtrl.Items.Count; i++)
			Console.WriteLine ("Item->" +  listViewCtrl.Items[i].Text + " idx: " + listViewCtrl.Items[i].Index);
	
		// Selected Items
		for (int i=0; i < listViewCtrl.SelectedItems.Count; i++)
			Console.WriteLine ("Sel Item->" +  listViewCtrl.SelectedItems[i].Text + " idx: " + listViewCtrl.SelectedItems[i].Index);
	}
	
	
	public void DelColumnButton()
	{				
		listViewCtrl.Columns.RemoveAt(1); /*Base on 0 index*/		
	}
	
	public void DumpSelButton()
	{				
		// Show selected items
		if (sel==null)
		{
			Console.WriteLine ("Col init");
			sel = listViewCtrl.SelectedItems;
		}
		
		Console.WriteLine ("Selected---------------");
		
		for (int i=0; i < sel.Count; i++)
			Console.WriteLine ("Item->" +  sel[i].Text + " idx: " + sel[i].Index);
		
		
	}
	
	public void DelItemButton()
	{	
		Console.WriteLine ("Elements ");
			
		listViewCtrl.Items.RemoveAt(2);			
				
		// How the elements are order once an element in deleted
		for (int i=0; i < listViewCtrl.Items.Count; i++)
			Console.WriteLine ("Items " +  listViewCtrl.Items[i].Text + " idx: " + listViewCtrl.Items[i].Index);
			
		
		
	}
	
	public void ShowClassDefaults()
	{
	
		Console.WriteLine ("Sorting " + listViewCtrl.Sorting);
		Console.WriteLine ("Label Edit " + listViewCtrl.LabelEdit);
		Console.WriteLine ("FullRowSelect " + listViewCtrl.FullRowSelect);
		Console.WriteLine ("GridLines " + listViewCtrl.GridLines);		
		Console.WriteLine ("AutoArrange " + listViewCtrl.AutoArrange);		
		Console.WriteLine ("LabelWrap " + listViewCtrl.LabelWrap);				
		Console.WriteLine ("MultiSelect " + listViewCtrl.MultiSelect);				
		Console.WriteLine ("ForeColor " + listViewCtrl.ForeColor);				
		Console.WriteLine ("BackColor " + listViewCtrl.BackColor);						
		Console.WriteLine ("ItemActivation " + listViewCtrl.Activation);				
		Console.WriteLine ("ColumnHeaderStyle " + listViewCtrl.HeaderStyle);				
		
				
	}
	
	public MyListViewForm()
	{
		InitializeComponent();
	}
	
	private void ColumnSample()
	{
		listViewCtrl = new myListView();
		
		ShowClassDefaults();
		
		listViewCtrl.HeaderStyle = ColumnHeaderStyle.None;
		
		// Set params
		listViewCtrl.View = View.Details;			
		//listViewCtrl.LabelEdit = true;			
		listViewCtrl.AllowColumnReorder=true;
		listViewCtrl.FullRowSelect = true;	
		listViewCtrl.GridLines = true;
		listViewCtrl.Activation = ItemActivation.OneClick;
			 
		
    	listViewCtrl.Bounds = new Rectangle(new Point(10,60), new Size(600, 550));
    	ListViewItem item1 = new ListViewItem("item1");
    	ListViewItem item2 = new ListViewItem("item2");
    	ListViewItem item3 = new ListViewItem("item3");
    	ListViewItem item4 = new ListViewItem("item4");
    	ListViewItem item5 = new ListViewItem("item5");
    	ListViewItem item6 = new ListViewItem("item6");
    	ListViewItem item7 = new ListViewItem("item7");
    	ListViewItem item8 = new ListViewItem("item8");
    	ListViewItem item9 = new ListViewItem("item9");
    	ListViewItem item10 = new ListViewItem("item10 aaaaaaaaaaaaaaaaaaaaaaaaa");

		Console.WriteLine ("*Column 1");			    	
   	    column1 = listViewCtrl.Columns.Add("Column 1", -1, HorizontalAlignment.Left);
   	    Console.WriteLine ("*Column 2");			    	
   	   	column2 =  listViewCtrl.Columns.Add("Column 2", -2, HorizontalAlignment.Right);
   	   	column3 =  listViewCtrl.Columns.Add("Column 3", 50, HorizontalAlignment.Right);
   	   	column4 =  new ColumnHeader();
   	   	
   	   	column4.Text="Column 4";
   	   	column4.Width= 150;
   	   	
   	   	//listViewCtrl.Columns.AddRange(new ColumnHeader[]{column4});
   	    
		listViewCtrl.Items.Add(item1);					
		listViewCtrl.Items.Add(item2);					
		
		listViewCtrl.Items.AddRange(new ListViewItem[]{item3,item4,item5,item6,item7,item8,item9,item10});
				
    	item1.SubItems.Add("sub item 1");        
    	item1.SubItems.Add("sub item 2");        
    	
    	listViewCtrl.Items.Add( new ListViewItem(new string[]{"boy 1", "boy 2", "boy 3"}));
    	
   		DelColumnButton button = new DelColumnButton(this);		
		button.Location = new System.Drawing.Point(5, 10);
		button.Name = "button1";
		button.Size = new System.Drawing.Size(100, 30);		
		button.Text = "Delete Column 2";
		button.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
		Controls.Add(button); 
		
		ClearColumnsButton button5 = new ClearColumnsButton(this);		
		button5.Location = new System.Drawing.Point(115, 10);
		button5.Name = "button5";
		button5.Size = new System.Drawing.Size(100, 30);		
		button5.Text = "Clear Columns";
		button5.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
		Controls.Add(button5); 		
		
		AddColumnsButton button6 = new AddColumnsButton(this);		
		button6.Location = new System.Drawing.Point(225, 10);
		button6.Name = "button6";
		button6.Size = new System.Drawing.Size(100, 30);		
		button6.Text = "Add Column";
		button6.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
		Controls.Add(button6); 		
		
		ShowColumnsButton button7 = new ShowColumnsButton(this);		
		button7.Location = new System.Drawing.Point(335, 10);
		button7.Name = "button7";
		button7.Size = new System.Drawing.Size(100, 30);		
		button7.Text = "Show Columns";
		button7.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
		Controls.Add(button7); 		
		
		DelItemButton button2 = new DelItemButton(this);		
		button2.Location = new System.Drawing.Point(630, 90);
		button2.Name = "button2";
		button2.Size = new System.Drawing.Size(100, 30);		
		button2.Text = "Delete Item 3";
		button2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
		Controls.Add(button2); 
		
		DumpSelButton button3 = new DumpSelButton(this);		
		button3.Location = new System.Drawing.Point(630, 120);
		button3.Name = "button3";
		button3.Size = new System.Drawing.Size(100, 30);		
		button3.Text = "Show selection";
		button3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
		Controls.Add(button3); 		
		
		ClearButton button4 = new ClearButton(this);		
		button4.Location = new System.Drawing.Point(630, 150);
		button4.Name = "button4";
		button4.Size = new System.Drawing.Size(100, 30);		
		button4.Text = "Clear";
		button4.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
		Controls.Add(button4);    	
		
    	
    	Controls.Add(listViewCtrl);
	}
	
	private void InitializeComponent()
	{		
		ClientSize = new System.Drawing.Size(750, 650);				
		ColumnSample();		
		return;   
	}
}

// Delete column
public class DelColumnButton : System.Windows.Forms.Button{
		MyListViewForm form = null;

		public DelColumnButton(MyListViewForm frm) : base()
		{
			form =  frm;
			
		}
		
		/* User clicks the button*/
		protected override void OnClick(EventArgs e) 
		{	
			form.DelColumnButton();
		}
}

// Delete item
public class DelItemButton : System.Windows.Forms.Button{
		MyListViewForm form = null;

		public DelItemButton(MyListViewForm frm) : base()
		{
			form =  frm;
			
		}
		
		/* User clicks the button*/
		protected override void OnClick(EventArgs e) 
		{	
			form.DelItemButton();
		}
}

// Show selection
public class DumpSelButton : System.Windows.Forms.Button{
		MyListViewForm form = null;

		public DumpSelButton(MyListViewForm frm) : base()
		{
			form =  frm;
			
		}
		
		/* User clicks the button*/
		protected override void OnClick(EventArgs e) 
		{	
			form.DumpSelButton();
		}
}


// Show columns
public class ShowColumnsButton : System.Windows.Forms.Button{
		MyListViewForm form = null;

		public ShowColumnsButton(MyListViewForm frm) : base()
		{
			form =  frm;
			
		}
		
		/* User clicks the button*/
		protected override void OnClick(EventArgs e) 
		{	
			form.ShowColumnsButton();
		}
}



// ClearColumnsButton
public class ClearColumnsButton : System.Windows.Forms.Button{
		MyListViewForm form = null;

		public ClearColumnsButton(MyListViewForm frm) : base()
		{
			form =  frm;
			
		}
		
		/* User clicks the button*/
		protected override void OnClick(EventArgs e) 
		{	
			form.ClearColumnsButton();
		}
}


// AddColumnsButton
public class AddColumnsButton : System.Windows.Forms.Button{
		MyListViewForm form = null;

		public AddColumnsButton(MyListViewForm frm) : base()
		{
			form =  frm;
			
		}
		
		/* User clicks the button*/
		protected override void OnClick(EventArgs e) 
		{	
			form.AddColumnsButton();
		}
}


// ClearButton
public class ClearButton : System.Windows.Forms.Button{
		MyListViewForm form = null;

		public ClearButton(MyListViewForm frm) : base()
		{
			form =  frm;
			
		}
		
		/* User clicks the button*/
		protected override void OnClick(EventArgs e) 
		{	
			form.ClearButton();
		}
}

public class myListView : System.Windows.Forms.ListView
{		

		protected override  void  OnColumnClick(ColumnClickEventArgs e) {				
			Console.WriteLine ("Column " +  Columns[e.Column].Text + " idx: " + Columns[e.Column].Index);			
			

		}
		
		protected override  void  OnBeforeLabelEdit(LabelEditEventArgs e){
			
			Console.WriteLine ("OnBeforeLabelEdit. CancelEdit->" +  e.CancelEdit + " Item-> "+e.Item + " Label->"+e.Label  );						
			
			//e.CancelEdit = true;
			
		}
		
		protected override  void  OnAfterLabelEdit(LabelEditEventArgs e){
			
			Console.WriteLine ("OnAfterLabelEdit. CancelEdit->" +  e.CancelEdit + " Item-> "+e.Item + " Label->"+e.Label  );						
			
			e.CancelEdit = true;
			
		}
		
		protected override  void  OnItemActivate(EventArgs ice){
			
			Console.WriteLine ("OnItemActivate");						
			
		}
			

}