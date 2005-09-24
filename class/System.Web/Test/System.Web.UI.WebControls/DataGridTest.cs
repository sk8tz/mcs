//
// Tests for System.Web.UI.WebControls.DataGrid.cs 
//
// Author:
//	Jackson Harper (jackson@ximian.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;
using AttributeCollection = System.ComponentModel.AttributeCollection;
using System;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Data;
using System.ComponentModel;
using System.Diagnostics;

namespace MonoTests.System.Web.UI.WebControls {

	public class DataGridPoker : DataGrid {

		public DataGridPoker ()
		{
			TrackViewState ();
		}

		public string GetTagName ()
		{
			return TagName;
		}

		public string Render ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);

			Render (tw);
			return sw.ToString ();
		}

		public StateBag GetViewState ()
		{
			return ViewState;
		}

		public Style ControlStyle ()
		{
			return CreateControlStyle ();
		}

		public void DoCancelCommand (DataGridCommandEventArgs e)
		{
			OnCancelCommand (e);
		}

		public void DoDeleteCommand (DataGridCommandEventArgs e)
		{
			OnDeleteCommand (e);
		}

		public void DoEditCommand (DataGridCommandEventArgs e)
		{
			OnEditCommand (e);
		}

		public void DoItemCommand (DataGridCommandEventArgs e)
		{
			OnItemCommand (e);
		}

		public void DoUpdateCommand (DataGridCommandEventArgs e)
		{
			OnUpdateCommand (e);
		}

		public void DoItemCreated (DataGridItemEventArgs e)
		{
			OnItemCreated (e);
		}

		public void DoItemDataBound (DataGridItemEventArgs e)
		{
			OnItemDataBound (e);
		}

		public void DoPageIndexChanged (DataGridPageChangedEventArgs e)
		{
			OnPageIndexChanged (e);
		}

		public void DoSortCommand (DataGridSortCommandEventArgs e)
		{
			OnSortCommand (e);
		}

		public void DoBubbleEvent (object source, EventArgs e)
		{
			OnBubbleEvent (source, e);
		}

		public void TrackState ()
		{
			TrackViewState ();
		}

		public object SaveState ()
		{
			return SaveViewState ();
		}

		public ArrayList CreateColumns (PagedDataSource data_source, bool use_data_source)
		{
			return CreateColumnSet (data_source, use_data_source);
		}

		public void CreateControls (bool use_data_source)
		{
			CreateControlHierarchy (use_data_source);
		}

		public void InitPager (DataGridItem item, int columnSpan,
				PagedDataSource pagedDataSource)
		{
			InitializePager (item, columnSpan, pagedDataSource);
		}
	}

	public class AmazingEnumerable : IEnumerable {

		private IList list;
		public int CallCount;

		public AmazingEnumerable (IList list)
		{
			this.list = list;
		}

	        public IEnumerator GetEnumerator ()
		{
			CallCount++;
			return list.GetEnumerator ();
		}
		
	}

	[TestFixture]
	public class DataGridTest {

		[Test]
		public void Defaults ()
		{
			DataGridPoker p = new DataGridPoker ();

			Assert.AreEqual (DataGrid.CancelCommandName, "Cancel", "A1");
			Assert.AreEqual (DataGrid.DeleteCommandName, "Delete", "A2");
			Assert.AreEqual (DataGrid.EditCommandName, "Edit", "A3");
			Assert.AreEqual (DataGrid.NextPageCommandArgument, "Next", "A4");
			Assert.AreEqual (DataGrid.PageCommandName, "Page", "A5");
			Assert.AreEqual (DataGrid.PrevPageCommandArgument, "Prev", "A6");
			Assert.AreEqual (DataGrid.SelectCommandName, "Select", "A7");
			Assert.AreEqual (DataGrid.SortCommandName, "Sort", "A8");
			Assert.AreEqual (DataGrid.UpdateCommandName, "Update", "A9");

			Assert.AreEqual (p.AllowCustomPaging, false, "A10");
			Assert.AreEqual (p.AllowPaging, false, "A11");
			Assert.AreEqual (p.AllowSorting, false, "A12");
			Assert.AreEqual (p.AutoGenerateColumns, true, "A13");
			Assert.AreEqual (p.BackImageUrl, String.Empty, "A14");
			Assert.AreEqual (p.CurrentPageIndex, 0, "A15");
			Assert.AreEqual (p.EditItemIndex, -1, "A16");
			Assert.AreEqual (p.PageCount, 0, "A17");
			Assert.AreEqual (p.PageSize, 10, "A18");
			Assert.AreEqual (p.SelectedIndex, -1, "A19");
			Assert.AreEqual (p.SelectedItem, null, "A20");
			Assert.AreEqual (p.ShowFooter, false, "A21");
			Assert.AreEqual (p.ShowHeader, true, "A22");
			Assert.AreEqual (p.VirtualItemCount, 0, "A23");
		}

		[Test]
		public void TagName ()
		{
			DataGridPoker p = new DataGridPoker ();
#if NET_2_0
			Assert.AreEqual (p.GetTagName (), "table", "A1");
#else
			Assert.AreEqual (p.GetTagName (), "span", "A1");
#endif
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullBackImage ()
		{
			DataGridPoker p = new DataGridPoker ();

			p.BackImageUrl = null;
			Assert.AreEqual (p.BackImageUrl, String.Empty, "A1");
		}

		[Test]
		public void CleanProperties ()
		{
			DataGridPoker p = new DataGridPoker ();

			p.AllowCustomPaging = true;
			Assert.IsTrue (p.AllowCustomPaging, "A1");
			p.AllowCustomPaging = false;
			Assert.IsFalse (p.AllowCustomPaging, "A2");

			p.AllowPaging = true;
			Assert.IsTrue (p.AllowPaging, "A3");
			p.AllowPaging = false;
			Assert.IsFalse (p.AllowPaging, "A4");

			p.AllowSorting = true;
			Assert.IsTrue (p.AllowSorting, "A5");
			p.AllowSorting = false;
			Assert.IsFalse (p.AllowSorting, "A6");

			p.AutoGenerateColumns = true;
			Assert.IsTrue (p.AutoGenerateColumns, "A7");
			p.AutoGenerateColumns = false;
			Assert.IsFalse (p.AutoGenerateColumns, "A8");

			p.BackImageUrl = "foobar";
			Assert.AreEqual (p.BackImageUrl, "foobar", "A9");

			p.CurrentPageIndex = 0;
			Assert.AreEqual (p.CurrentPageIndex, 0, "A10");
			p.CurrentPageIndex = Int32.MaxValue;
			Assert.AreEqual (p.CurrentPageIndex, Int32.MaxValue, "A11");

			p.EditItemIndex = 0;
			Assert.AreEqual (p.EditItemIndex, 0, "A12");
			p.EditItemIndex = -1;
			Assert.AreEqual (p.EditItemIndex, -1, "A13");
			p.EditItemIndex = Int32.MaxValue;
			Assert.AreEqual (p.EditItemIndex, Int32.MaxValue, "A14");

			p.PageSize = 1;
			Assert.AreEqual (p.PageSize, 1, "A15");
			p.PageSize = Int32.MaxValue;

			p.SelectedIndex = 0;
			Assert.AreEqual (p.SelectedIndex, 0, "A16");
			p.SelectedIndex = -1;
			Assert.AreEqual (p.SelectedIndex, -1, "A17");
			p.SelectedIndex = Int32.MaxValue;
			Assert.AreEqual (p.SelectedIndex, Int32.MaxValue, "A18");

			p.ShowFooter = true;
			Assert.IsTrue (p.ShowFooter, "A19");
			p.ShowFooter = false;
			Assert.IsFalse (p.ShowFooter, "A20");

			p.ShowHeader = true;
			Assert.IsTrue (p.ShowHeader, "A21");
			p.ShowHeader = false;
			Assert.IsFalse (p.ShowHeader, "A22");

			p.VirtualItemCount = 0;
			Assert.AreEqual (p.VirtualItemCount, 0, "A23");
			p.VirtualItemCount = Int32.MaxValue;
			Assert.AreEqual (p.VirtualItemCount, Int32.MaxValue, "A24");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CurrentPageIndexTooLow ()
		{
			DataGridPoker p = new DataGridPoker ();
			p.CurrentPageIndex = -1;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void EditItemIndexTooLow ()
		{
			DataGridPoker p = new DataGridPoker ();
			p.EditItemIndex = -2;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void PageSizeTooLow ()
		{
			DataGridPoker p = new DataGridPoker ();
			p.PageSize = 0;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SelectedIndexTooLow ()
		{
			DataGridPoker p = new DataGridPoker ();
			p.SelectedIndex = -2;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void VirtualItemCountTooLow ()
		{
			DataGridPoker p = new DataGridPoker ();
			p.VirtualItemCount = -1;
		}
			
		[Test]
		public void ViewState ()
		{
			DataGridPoker p = new DataGridPoker ();
			StateBag vs = p.GetViewState ();

			Assert.AreEqual (vs.Count, 0, "A1");

			p.AllowCustomPaging = true;
			Assert.AreEqual (vs.Count, 1, "A2");
			Assert.AreEqual (vs ["AllowCustomPaging"], true, "A3");
			p.AllowCustomPaging = false;
			Assert.AreEqual (vs.Count, 1, "A4");
			Assert.AreEqual (vs ["AllowCustomPaging"], false, "A5");

			p.AllowPaging = true;
			Assert.AreEqual (vs.Count, 2, "A6");
			Assert.AreEqual (vs ["AllowPaging"], true, "A7");
			p.AllowPaging = false;
			Assert.AreEqual (vs.Count, 2, "A8");
			Assert.AreEqual (vs ["AllowPaging"], false, "A9");

			p.AllowSorting = true;
			Assert.AreEqual (vs.Count, 3, "A10");
			Assert.AreEqual (vs ["AllowSorting"], true, "A11");
			p.AllowSorting = false;
			Assert.AreEqual (vs.Count, 3, "A12");
			Assert.AreEqual (vs ["AllowSorting"], false, "A13");

			p.AutoGenerateColumns = true;
			Assert.AreEqual (vs.Count, 4, "A14");
			Assert.AreEqual (vs ["AutoGenerateColumns"], true, "A15");
			p.AutoGenerateColumns = false;
			Assert.AreEqual (vs.Count, 4, "A16");
			Assert.AreEqual (vs ["AutoGenerateColumns"], false, "A17");

			p.CurrentPageIndex = 1;
			Assert.AreEqual (vs.Count, 5, "A18");
			Assert.AreEqual (vs ["CurrentPageIndex"], 1, "A19");

			p.EditItemIndex = 1;
			Assert.AreEqual (vs.Count, 6, "A20");
			Assert.AreEqual (vs ["EditItemIndex"], 1, "A20");

			p.PageSize = 25;
			Assert.AreEqual (vs.Count, 7, "A21");
			Assert.AreEqual (vs ["PageSize"], 25, "A22");

			p.SelectedIndex = 25;
			Assert.AreEqual (vs.Count, 8, "A23");
			Assert.AreEqual (vs ["SelectedIndex"], 25, "A24");

			p.ShowFooter = false;
			Assert.AreEqual (vs.Count, 9, "A25");
			Assert.AreEqual (vs ["ShowFooter"], false, "A26");
			p.ShowFooter = true;
			Assert.AreEqual (vs ["ShowFooter"], true, "A27");

			p.ShowHeader = false;
			Assert.AreEqual (vs.Count, 10, "A28");
			Assert.AreEqual (vs ["ShowHeader"], false, "A29");
			p.ShowHeader = true;
			Assert.AreEqual (vs ["ShowHeader"], true, "A30");

			p.VirtualItemCount = 100;
			Assert.AreEqual (vs.Count, 11, "A31");
			Assert.AreEqual (vs ["VirtualItemCount"], 100, "A32");
		}

		[Test]
		public void SelectIndexOutOfRange ()
		{
			DataGridPoker p = new DataGridPoker ();

			// No exception is thrown
			p.SelectedIndex = 25;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SelectItemOutOfRange ()
		{
			DataGridPoker p = new DataGridPoker ();
			DataGridItem d;

			p.SelectedIndex = 25;
			d = p.SelectedItem;
		}

		[Test]
		public void ControlStyle ()
		{
			DataGridPoker p = new DataGridPoker ();

			Assert.AreEqual (p.ControlStyle ().GetType (),
					typeof (TableStyle), "A1");

			TableStyle t = (TableStyle) p.ControlStyle ();
			Assert.AreEqual (t.GridLines, GridLines.Both, "A2");
			Assert.AreEqual (t.CellSpacing, 0, "A3");
		}

		[Test]
		public void Styles ()
		{
			DataGridPoker p = new DataGridPoker ();
			StateBag vs = p.GetViewState ();
			
			p.BackImageUrl = "foobar url";

			// The styles get stored in the view state
#if NET_2_0
			Assert.AreEqual (vs.Count, 0, "A1");
			Assert.IsNull (vs ["BackImageUrl"], "A2");
			Assert.IsNull (vs ["GridLines"], "A3");
			Assert.IsNull (vs ["CellSpacing"], "A4");
#else
			Assert.AreEqual (vs.Count, 3, "A1");
			Assert.AreEqual (vs ["BackImageUrl"], "foobar url", "A2");
			Assert.AreEqual (vs ["GridLines"], GridLines.Both, "A3");
			Assert.AreEqual (vs ["CellSpacing"], 0, "A4");
#endif
		}

		private bool cancel_command;
		private bool delete_command;
		private bool edit_command;
		private bool item_command;
		private bool update_command;
		private bool item_created;
		private bool item_data_bound;
		private bool page_index_changed;
		private bool sort_command;
		private bool selected_changed;

		private int new_page_index;

		private void ResetEvents ()
		{
			cancel_command =
			delete_command =
			edit_command =
			item_command =
			update_command =
			item_created = 
			item_data_bound = 
			page_index_changed =
			sort_command =
			selected_changed = false;

			new_page_index = Int32.MinValue;
		}
				
		private void CancelCommandHandler (object sender, DataGridCommandEventArgs e)
		{
			cancel_command = true;
		}

		private void DeleteCommandHandler (object sender, DataGridCommandEventArgs e)
		{
			delete_command = true;
		}
		
		private void EditCommandHandler (object sender, DataGridCommandEventArgs e)
		{
			edit_command = true;
		}

		private void ItemCommandHandler (object sender, DataGridCommandEventArgs e)
		{
			item_command = true;
		}

		private void UpdateCommandHandler (object sender, DataGridCommandEventArgs e)
		{
			update_command = true;
		}

		private void ItemCreatedHandler (object sender, DataGridItemEventArgs e)
		{
			item_created = true;
		}

		private void ItemDataBoundHandler (object sender, DataGridItemEventArgs e)
		{
			item_data_bound = true;
		}
		
		private void PageIndexChangedHandler (object sender, DataGridPageChangedEventArgs e)
		{
			page_index_changed = true;
			new_page_index = e.NewPageIndex;
		}
		
		private void SortCommandHandler (object sender, DataGridSortCommandEventArgs e)
		{
			sort_command = true;
		}

		private void SelectedIndexChangedHandler (object sender, EventArgs e)
		{
			selected_changed = true;
		}

		[Test]
		public void Events ()
		{
			DataGridPoker p = new DataGridPoker ();
			DataGridCommandEventArgs command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs (String.Empty, String.Empty));
			DataGridItemEventArgs item_args = new DataGridItemEventArgs (null);
			DataGridPageChangedEventArgs page_args = new DataGridPageChangedEventArgs (null, 0);
			DataGridSortCommandEventArgs sort_args = new DataGridSortCommandEventArgs (null,
					command_args);

			ResetEvents ();
			p.CancelCommand += new DataGridCommandEventHandler (CancelCommandHandler);
			p.DoCancelCommand (command_args);
			Assert.IsTrue (cancel_command, "A1");

			ResetEvents ();
			p.DeleteCommand += new DataGridCommandEventHandler (DeleteCommandHandler);
			p.DoDeleteCommand (command_args);
			Assert.IsTrue (delete_command, "A2");

			ResetEvents ();
			p.EditCommand += new DataGridCommandEventHandler (EditCommandHandler);
			p.DoEditCommand (command_args);
			Assert.IsTrue (edit_command, "A3");

			ResetEvents ();
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.DoItemCommand (command_args);
			Assert.IsTrue (item_command, "A4");

			ResetEvents ();
			p.UpdateCommand += new DataGridCommandEventHandler (UpdateCommandHandler);
			p.DoUpdateCommand (command_args);
			Assert.IsTrue (update_command, "A5");

			ResetEvents ();
			p.ItemCreated += new DataGridItemEventHandler (ItemCreatedHandler);
			p.DoItemCreated (item_args);
			Assert.IsTrue (item_created, "A6");

			ResetEvents ();
			p.ItemDataBound += new DataGridItemEventHandler (ItemDataBoundHandler);
			p.DoItemDataBound (item_args);
			Assert.IsTrue (item_data_bound, "A7");

			ResetEvents ();
			p.PageIndexChanged += new DataGridPageChangedEventHandler (PageIndexChangedHandler);
			p.DoPageIndexChanged (page_args);
			Assert.IsTrue (page_index_changed, "A8");

			ResetEvents ();
			p.SortCommand += new DataGridSortCommandEventHandler (SortCommandHandler);
			p.DoSortCommand (sort_args);
			Assert.IsTrue (sort_command, "A9");
		}

		[Test]
		public void BubbleEvent ()
		{
			DataGridPoker p = new DataGridPoker ();
			DataGridCommandEventArgs command_args;

			//
			// Cancel
			//
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("Cancel", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.CancelCommand += new DataGridCommandEventHandler (CancelCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (cancel_command, "A1");
			Assert.IsTrue (item_command, "#01");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("cancel", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.CancelCommand += new DataGridCommandEventHandler (CancelCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (cancel_command, "A2");
			Assert.IsTrue (item_command, "#02");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("CANCEL", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.CancelCommand += new DataGridCommandEventHandler (CancelCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (cancel_command, "A3");
			Assert.IsTrue (item_command, "#03");

			//
			// Delete
			//
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("Delete", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.DeleteCommand += new DataGridCommandEventHandler (DeleteCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (delete_command, "A4");
			Assert.IsTrue (item_command, "#04");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("delete", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.DeleteCommand += new DataGridCommandEventHandler (DeleteCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (delete_command, "A5");
			Assert.IsTrue (item_command, "#05");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("DELETE", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.DeleteCommand += new DataGridCommandEventHandler (DeleteCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (delete_command, "A6");
			Assert.IsTrue (item_command, "#06");

			//
			// Edit
			//
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("Edit", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.EditCommand += new DataGridCommandEventHandler (EditCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (edit_command, "A7");
			Assert.IsTrue (item_command, "#07");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("edit", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.EditCommand += new DataGridCommandEventHandler (EditCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (edit_command, "A8");
			Assert.IsTrue (item_command, "#08");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("EDIT", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.EditCommand += new DataGridCommandEventHandler (EditCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (edit_command, "A9");
			Assert.IsTrue (item_command, "#09");

			//
			// Item
			//
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("Item", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (item_command, "A10");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("item", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (item_command, "A11");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("ITEM", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (item_command, "A12");

			//
			// Sort
			//
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("Sort", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.SortCommand += new DataGridSortCommandEventHandler (SortCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (sort_command, "A13");
			Assert.IsTrue (item_command, "#10");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("sort", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.SortCommand += new DataGridSortCommandEventHandler (SortCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (sort_command, "A14");
			Assert.IsTrue (item_command, "#11");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("SORT", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.SortCommand += new DataGridSortCommandEventHandler (SortCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (sort_command, "A15");
			Assert.IsTrue (item_command, "#12");

			//
			// Update
			//
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("Update", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.UpdateCommand += new DataGridCommandEventHandler (UpdateCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (update_command, "A16");
			Assert.IsTrue (item_command, "#13");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("update", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.UpdateCommand += new DataGridCommandEventHandler (UpdateCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (update_command, "A17");
			Assert.IsTrue (item_command, "#14");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (null,
					null, new CommandEventArgs ("UPDATE", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.UpdateCommand += new DataGridCommandEventHandler (UpdateCommandHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (update_command, "A18");
			Assert.IsTrue (item_command, "#15");

			//
			// Select
			//
			DataGridItem item = new DataGridItem (0, 0, ListItemType.Item);
			
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("Select", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.SelectedIndexChanged += new EventHandler (SelectedIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (selected_changed, "A19");
			Assert.IsTrue (item_command, "#16");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("select", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.SelectedIndexChanged += new EventHandler (SelectedIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (selected_changed, "A20");
			Assert.IsTrue (item_command, "#17");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("SELECT", String.Empty));
			p.ItemCommand += new DataGridCommandEventHandler (ItemCommandHandler);
			p.SelectedIndexChanged += new EventHandler (SelectedIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (selected_changed, "A21");
			Assert.IsTrue (item_command, "#18");
		}

		[Test]
		public void BubblePageCommand ()
		{
			DataGridPoker p = new DataGridPoker ();
			DataGridItem item = new DataGridItem (0, 0, ListItemType.Item);
			DataGridCommandEventArgs command_args;


			//
			// Prev
			//
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("Page", "Prev"));
			p.CurrentPageIndex = 10;
			p.PageIndexChanged += new DataGridPageChangedEventHandler (PageIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (page_index_changed, "A1");
			Assert.AreEqual (new_page_index, 9, "A2");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("page", "prev"));
			p.CurrentPageIndex = 10;
			p.PageIndexChanged += new DataGridPageChangedEventHandler (PageIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (page_index_changed, "A3");
			Assert.AreEqual (new_page_index, 9, "A4");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("PAGE", "PREV"));
			p.CurrentPageIndex = 10;
			p.PageIndexChanged += new DataGridPageChangedEventHandler (PageIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (page_index_changed, "A5");
			Assert.AreEqual (new_page_index, 9, "A6");

			
			//
			// Next
			//
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("Page", "Next"));
			p.CurrentPageIndex = 10;
			p.PageIndexChanged += new DataGridPageChangedEventHandler (PageIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (page_index_changed, "A5");
			Assert.AreEqual (new_page_index, 11, "A6");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("page", "next"));
			p.CurrentPageIndex = 10;
			p.PageIndexChanged += new DataGridPageChangedEventHandler (PageIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (page_index_changed, "A7");
			Assert.AreEqual (new_page_index, 11, "A8");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("PAGE", "NEXT"));
			p.CurrentPageIndex = 10;
			p.PageIndexChanged += new DataGridPageChangedEventHandler (PageIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (page_index_changed, "A9");
			Assert.AreEqual (new_page_index, 11, "A10");


			//
			// Specific
			//
			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("Page", "25"));
			p.CurrentPageIndex = 10;
			p.PageIndexChanged += new DataGridPageChangedEventHandler (PageIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (page_index_changed, "A11");
			Assert.AreEqual (new_page_index, 24, "A12");

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("Page", "0"));
			p.CurrentPageIndex = 10;
			p.PageIndexChanged += new DataGridPageChangedEventHandler (PageIndexChangedHandler);
			p.DoBubbleEvent (this, command_args);
			Assert.IsTrue (page_index_changed, "A11");
			Assert.AreEqual (new_page_index, -1, "A12");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void BadBubblePageArg ()
		{
			DataGridPoker p = new DataGridPoker ();
			DataGridItem item = new DataGridItem (0, 0, ListItemType.Item);
			DataGridCommandEventArgs command_args;

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("Page", "i am bad"));

			p.DoBubbleEvent (this, command_args);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void BadBubblePageArg2 ()
		{
			DataGridPoker p = new DataGridPoker ();
			DataGridItem item = new DataGridItem (0, 0, ListItemType.Item);
			DataGridCommandEventArgs command_args;

			ResetEvents ();
			command_args = new DataGridCommandEventArgs (item, null,
					new CommandEventArgs ("Page", new object ()));

			p.DoBubbleEvent (this, command_args);
		}

		[Test]
		public void SaveViewState ()
		{
			DataGridPoker p = new DataGridPoker ();

			p.TrackState ();

			object [] vs = (object []) p.SaveState ();
#if NET_2_0
			Assert.AreEqual (vs.Length, 11, "A1");
#else
			Assert.AreEqual (vs.Length, 10, "A1");
#endif

			// By default the viewstate is all null
			for (int i = 0; i < vs.Length; i++)
				Assert.IsNull (vs [i], "A2-" + i);

			//
			// TODO: What goes in the [1] and [9] slots?
			//

			p.AllowPaging = true;
			vs = (object []) p.SaveState ();
			Assert.IsNotNull (vs [0], "A3");

			/*
			  This test doesn't work right now. It must be an issue
			  in the DataGridPagerStyle
			  
			p.PagerStyle.Visible = true;
			vs = (object []) p.SaveState ();
			Assert.IsNotNull (vs [2], "A5");
			*/
			
			p.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) p.SaveState ();
			Assert.IsNotNull (vs [3], "A6");

			p.FooterStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) p.SaveState ();
			Assert.IsNotNull (vs [4], "A7");

			p.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) p.SaveState ();
			Assert.IsNotNull (vs [5], "A8");

			p.AlternatingItemStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) p.SaveState ();
			Assert.IsNotNull (vs [6], "A9");

			p.SelectedItemStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) p.SaveState ();
			Assert.IsNotNull (vs [7], "A10");

			p.EditItemStyle.HorizontalAlign = HorizontalAlign.Center;
			vs = (object []) p.SaveState ();
			Assert.IsNotNull (vs [8], "A11");

			PagedDataSource source = new PagedDataSource ();
			DataTable table = new DataTable ();
			ArrayList columns;

			table.Columns.Add (new DataColumn ("one", typeof (string)));
			table.Columns.Add (new DataColumn ("two", typeof (string)));
			table.Columns.Add (new DataColumn ("three", typeof (string)));
			source.DataSource = new DataView (table);
			columns = p.CreateColumns (source, true);

			vs = (object []) p.SaveState ();
#if NET_2_0
			Assert.IsNull (vs [9], "A12");
#else
			Assert.IsNotNull (vs [9], "A12");
			Assert.AreEqual (vs [9].GetType (), typeof (object []), "A12");

			object [] cols = (object []) vs [9];
			Assert.AreEqual (cols.Length, 3, "A13");
#endif
		}

		[Test]
		public void CreateColumnSet ()
		{
			DataGridPoker p = new DataGridPoker ();
			PagedDataSource source = new PagedDataSource ();
			DataTable table = new DataTable ();
			ArrayList columns;

			table.Columns.Add (new DataColumn ("one", typeof (string)));
			table.Columns.Add (new DataColumn ("two", typeof (string)));
			table.Columns.Add (new DataColumn ("three", typeof (string)));

			source.DataSource = new DataView (table);

			columns = p.CreateColumns (source, true);
			Assert.AreEqual (columns.Count, 3, "A1");
			Assert.AreEqual (((DataGridColumn) columns [0]).HeaderText, "one", "A2");
			Assert.AreEqual (((DataGridColumn) columns [1]).HeaderText, "two", "A3");
			Assert.AreEqual (((DataGridColumn) columns [2]).HeaderText, "three", "A4");

			// AutoGenerated columns are not added to the ColumnsCollection
			Assert.AreEqual (p.Columns.Count, 0, "A5");
			
			// Without allowing data dinding,
			columns = p.CreateColumns (source, false);
			Assert.AreEqual (columns.Count, 3, "A6");
			Assert.AreEqual (((DataGridColumn) columns [0]).HeaderText, "one", "A7");
			Assert.AreEqual (((DataGridColumn) columns [1]).HeaderText, "two", "A8");
			Assert.AreEqual (((DataGridColumn) columns [2]).HeaderText, "three", "A9");


			// Mixing with already added columns
			p = new DataGridPoker ();
			DataGridColumn a = new ButtonColumn ();
			DataGridColumn b = new ButtonColumn ();

			a.HeaderText = "A";
			b.HeaderText = "B";
			p.Columns.Add (a);
			p.Columns.Add (b);

			columns = p.CreateColumns (source, true);
			Assert.AreEqual (columns.Count, 5, "A6");
			Assert.AreEqual (((DataGridColumn) columns [0]).HeaderText, "A", "A10");
			Assert.AreEqual (((DataGridColumn) columns [1]).HeaderText, "B", "A11");
			Assert.AreEqual (((DataGridColumn) columns [2]).HeaderText, "one", "A12");
			Assert.AreEqual (((DataGridColumn) columns [3]).HeaderText, "two", "A13");
			Assert.AreEqual (((DataGridColumn) columns [4]).HeaderText, "three", "A14");

			// Assigned properties of the newly created columns
			BoundColumn one = (BoundColumn) columns [2];

			Assert.AreEqual (one.HeaderText, "one", "A15");
			Assert.AreEqual (one.DataField, "one", "A16");
			Assert.AreEqual (one.DataFormatString, String.Empty, "A17");
			Assert.AreEqual (one.SortExpression, "one", "A18");
			Assert.AreEqual (one.HeaderImageUrl, String.Empty, "A19");
			Assert.AreEqual (one.FooterText, String.Empty, "A20");
		}

		[Test]
		public void CreateColumnsBinding ()
		{
			DataGridPoker p = new DataGridPoker ();
			PagedDataSource source = new PagedDataSource ();
			DataTable table = new DataTable ();
			ArrayList columns;

			table.Columns.Add (new DataColumn ("one", typeof (string)));
			table.Columns.Add (new DataColumn ("two", typeof (string)));
			table.Columns.Add (new DataColumn ("three", typeof (string)));

			source.DataSource = new DataView (table);

			columns = p.CreateColumns (source, true);
			Assert.AreEqual (columns.Count, 3, "A1");
			Assert.AreEqual (((DataGridColumn) columns [0]).HeaderText, "one", "A2");
			Assert.AreEqual (((DataGridColumn) columns [1]).HeaderText, "two", "A3");
			Assert.AreEqual (((DataGridColumn) columns [2]).HeaderText, "three", "A4");

			table.Columns.Add (new DataColumn ("four", typeof (string)));
			table.Columns.Add (new DataColumn ("five", typeof (string)));
			table.Columns.Add (new DataColumn ("six", typeof (string)));

			// Just gets the old columns
			columns = p.CreateColumns (source, false);
			Assert.AreEqual (columns.Count, 3, "A5");
			Assert.AreEqual (((DataGridColumn) columns [0]).HeaderText, "one", "A6");
			Assert.AreEqual (((DataGridColumn) columns [1]).HeaderText, "two", "A7");
			Assert.AreEqual (((DataGridColumn) columns [2]).HeaderText, "three", "A8");

			columns = p.CreateColumns (source, true);
			Assert.AreEqual (columns.Count, 6, "A9");
			Assert.AreEqual (((DataGridColumn) columns [0]).HeaderText, "one", "A10");
			Assert.AreEqual (((DataGridColumn) columns [1]).HeaderText, "two", "A11");
			Assert.AreEqual (((DataGridColumn) columns [2]).HeaderText, "three", "A12");
			Assert.AreEqual (((DataGridColumn) columns [3]).HeaderText, "four", "A13");
			Assert.AreEqual (((DataGridColumn) columns [4]).HeaderText, "five", "A14");
			Assert.AreEqual (((DataGridColumn) columns [5]).HeaderText, "six", "A15");

			// Assigned properties of the newly created columns
			BoundColumn one = (BoundColumn) columns [0];

			Assert.AreEqual (one.HeaderText, "one", "A16");
			Assert.AreEqual (one.DataField, "one", "A17");
			Assert.AreEqual (one.DataFormatString, String.Empty, "A18");
			Assert.AreEqual (one.SortExpression, "one", "A19");
			Assert.AreEqual (one.HeaderImageUrl, String.Empty, "A20");
			Assert.AreEqual (one.FooterText, String.Empty, "A21");
		}

		[Test]
		public void CreateSimpleColumns ()
		{
			DataGridPoker p = new DataGridPoker ();
			PagedDataSource source = new PagedDataSource ();
			ArrayList list = new ArrayList ();
			ArrayList columns;
			
			list.Add ("One");
			list.Add ("Two");
			list.Add ("Three");

			source.DataSource = list;
			columns = p.CreateColumns (source, true);
			Assert.AreEqual (1, columns.Count, "A1");
			Assert.AreEqual ("Item", ((DataGridColumn) columns [0]).HeaderText, "A2");

			AmazingEnumerable amazing = new AmazingEnumerable (list);

			source.DataSource = amazing;
			columns = p.CreateColumns (source, true);
			Assert.AreEqual (1, columns.Count, "A3");

			BoundColumn one = (BoundColumn) columns [0];

			Assert.AreEqual ("Item", one.HeaderText, "A4");

			// I guess this makes it bind to itself ?
			Assert.AreEqual (BoundColumn.thisExpr, one.DataField, "A5"); 

			Assert.AreEqual (String.Empty, one.DataFormatString, "A6");
			Assert.AreEqual ("Item", one.SortExpression, "A7");
			Assert.AreEqual (String.Empty, one.HeaderImageUrl, "A8");
			Assert.AreEqual (String.Empty, one.FooterText, "A9");
			Assert.AreEqual ("Item", one.HeaderText, "A10");

			source.DataSource = new ArrayList ();
			columns = p.CreateColumns (source, true);
			Assert.AreEqual (0, columns.Count, "A11");
		}

		[Test]
		public void DataBindingEnumerator ()
		{
			DataGridPoker p = new DataGridPoker ();
			PagedDataSource source = new PagedDataSource ();
			ArrayList list = new ArrayList ();
			ArrayList columns;
			
			list.Add ("One");
			list.Add ("Two");
			list.Add ("Three");

			AmazingEnumerable amazing = new AmazingEnumerable (list);
			source.DataSource = amazing;
			columns = p.CreateColumns (source, true);
			Assert.AreEqual (1, columns.Count, "A1");
			Assert.AreEqual ("Item", ((DataGridColumn) columns [0]).HeaderText, "A2");
			Assert.AreEqual (1, amazing.CallCount, "A3");
			Assert.AreEqual (0, p.DataKeys.Count, "A4");
		}

		class Custom : ICustomTypeDescriptor {
			public AttributeCollection GetAttributes ()
			{
				throw new Exception ();
			}

			public string GetClassName()
			{
				throw new Exception ();
			}

			public string GetComponentName()
			{
				throw new Exception ();
			}

			public TypeConverter GetConverter()
			{
				throw new Exception ();
			}

			public EventDescriptor GetDefaultEvent()
			{
				throw new Exception ();
			}

			public PropertyDescriptor GetDefaultProperty()
			{
				throw new Exception ();
			}

			public object GetEditor (Type editorBaseType)
			{
				throw new Exception ();
			}

			public EventDescriptorCollection GetEvents ()
			{
				throw new Exception ();
			}

			public EventDescriptorCollection GetEvents (Attribute[] arr)
			{
				throw new Exception ();
			}

			public int CallCount;
			public PropertyDescriptorCollection GetProperties()
			{
				// MS calls this one
				if (CallCount++ > 0)
					throw new Exception ("This should not happen");
				PropertyDescriptorCollection coll = new PropertyDescriptorCollection (null);
				coll.Add (new MyPropertyDescriptor ());
				return coll;
			}

			public PropertyDescriptorCollection GetProperties (Attribute[] arr)
			{
				// We call this one
				return GetProperties ();
			}

			public object GetPropertyOwner (PropertyDescriptor pd)
			{
				throw new Exception ();
			}
		}

		class MyPropertyDescriptor : PropertyDescriptor {
			int val;

			public MyPropertyDescriptor () : base ("CustomName", null)
			{
			}

			public override Type ComponentType {
				get { return typeof (MyPropertyDescriptor); }
			}

			public override bool IsReadOnly {
				get { return true; }
			}

			public override Type PropertyType {
				get { return typeof (int); }
			}

			public override object GetValue (object component)
			{
				return val++;
			}

			public override void SetValue (object component, object value)
			{
			}

			public override void ResetValue (object component)
			{
			}

			public override bool CanResetValue (object component)
			{
				return false;
			}

			public override bool ShouldSerializeValue (object component)
			{
				return false;
			}
		}

		class MyEnumerable : IEnumerable {
			public object Item;
			public IEnumerator GetEnumerator ()
			{
				ArrayList list = new ArrayList ();
				list.Add (Item);
				return list.GetEnumerator ();
			}
		}

		[Test]
		public void DataBindingCustomElement ()
		{
			DataGridPoker p = new DataGridPoker ();
			p.DataKeyField = "CustomName";
			PagedDataSource source = new PagedDataSource ();
			MyEnumerable myenum = new MyEnumerable ();
			myenum.Item = new Custom ();
			source.DataSource = myenum;
			ArrayList columns = p.CreateColumns (source, true);
			Assert.AreEqual (1, columns.Count, "A1");
			Assert.AreEqual ("CustomName", ((DataGridColumn) columns [0]).HeaderText, "A2");
			Assert.AreEqual (0, p.DataKeys.Count, "A3");
		}

		[Test]
		public void CreateControls ()
		{
			DataGridPoker p = new DataGridPoker ();
			DataTable table = new DataTable ();

			table.Columns.Add (new DataColumn ("one", typeof (string)));
			table.Columns.Add (new DataColumn ("two", typeof (string)));
			table.Columns.Add (new DataColumn ("three", typeof (string)));
			table.Rows.Add (new object [] { "1", "2", "3" });
			
			p.DataSource = new DataView (table);

			p.CreateControls (true);
			Assert.AreEqual (p.Controls.Count, 1, "A1");

			ShowControlsRecursive (p.Controls [0], 1);
		}

		[Test]
		public void CreationEvents ()
		{
			DataGridPoker p = new DataGridPoker ();
			DataTable table = new DataTable ();

			table.Columns.Add (new DataColumn ("one", typeof (string)));
			table.Columns.Add (new DataColumn ("two", typeof (string)));
			table.Columns.Add (new DataColumn ("three", typeof (string)));
			
			p.DataSource = new DataView (table);

			p.ItemCreated += new DataGridItemEventHandler (ItemCreatedHandler);
			p.ItemDataBound += new DataGridItemEventHandler (ItemDataBoundHandler);

			// No items added yet
			ResetEvents ();
			p.CreateControls (true);
			Assert.IsTrue (item_created, "A1");
			Assert.IsTrue (item_data_bound, "A2");

			table.Rows.Add (new object [] { "1", "2", "3" });

			ResetEvents ();
			p.CreateControls (true);
			Assert.IsTrue (item_created, "A3");
			Assert.IsTrue (item_data_bound, "A4");

			// no databinding
			ResetEvents ();
			p.CreateControls (false);
			Assert.IsTrue (item_created, "A5");
			Assert.IsFalse (item_data_bound, "A6");
		}

		[Test]
		public void InitializePager ()
		{
			DataGridPoker p = new DataGridPoker ();
			PagedDataSource paged = new PagedDataSource ();
			DataTable table = new DataTable ();
			DataGridItem item = new DataGridItem (-1, -1, ListItemType.Pager);
			ArrayList columns;
			LinkButton next;
			LinkButton prev;

			table.Columns.Add (new DataColumn ("one", typeof (string)));
			table.Columns.Add (new DataColumn ("two", typeof (string)));
			table.Columns.Add (new DataColumn ("three", typeof (string)));

			for (int i = 0; i < 25; i++)
				table.Rows.Add (new object [] { "1", "2", "3" });
			paged.DataSource = new DataView (table);

			columns = p.CreateColumns (paged, true);
			p.InitPager (item, columns.Count, paged);

			//
			// No where to go
			//

			Assert.AreEqual (item.Controls.Count, 1, "A1");
			Assert.AreEqual (item.Controls [0].GetType (), typeof (TableCell), "A2");
			Assert.AreEqual (item.Controls [0].Controls.Count, 3, "A3");
			Assert.AreEqual (item.Controls [0].Controls [0].GetType (), typeof (Label), "A4");
			Assert.AreEqual (item.Controls [0].Controls [1].GetType (),
					typeof (LiteralControl), "A5");
			Assert.AreEqual (item.Controls [0].Controls [2].GetType (), typeof (Label), "A6");
			Assert.AreEqual (((Label) item.Controls [0].Controls [0]).Text, "&lt;", "A7");
			Assert.AreEqual (((LiteralControl) item.Controls [0].Controls [1]).Text,
					"&nbsp;", "A7");
			Assert.AreEqual (((Label) item.Controls [0].Controls [2]).Text, "&gt;", "A8");

			//
			// Next
			//

			item = new DataGridItem (-1, -1, ListItemType.Pager);
			paged.PageSize = 5;
			paged.VirtualCount = 25;
			paged.AllowPaging = true;
			p.InitPager (item, columns.Count, paged);

			Assert.AreEqual (item.Controls.Count, 1, "A9");
			Assert.AreEqual (item.Controls [0].GetType (), typeof (TableCell), "A10");
			Assert.AreEqual (item.Controls [0].Controls.Count, 3, "A11");
			Assert.AreEqual (item.Controls [0].Controls [0].GetType (), typeof (Label), "A12");
			Assert.AreEqual (item.Controls [0].Controls [1].GetType (),
					typeof (LiteralControl), "A13");
			Assert.AreEqual (((Label) item.Controls [0].Controls [0]).Text, "&lt;", "A14");
			Assert.AreEqual (((LiteralControl) item.Controls [0].Controls [1]).Text,
					"&nbsp;", "A16");

			next = (LinkButton) item.Controls [0].Controls [2];
			Assert.AreEqual (next.Text, "&gt;", "A17");
			Assert.AreEqual (next.CommandName, "Page", "A18");
			Assert.AreEqual (next.CommandArgument, "Next", "A19");


			//
			// Both
			//

			item = new DataGridItem (-1, -1, ListItemType.Pager);
			paged.PageSize = 5;
			paged.VirtualCount = 25;
			paged.AllowPaging = true;
			paged.CurrentPageIndex = 2;
			p.InitPager (item, columns.Count, paged);

			Assert.AreEqual (item.Controls.Count, 1, "A20");
			Assert.AreEqual (item.Controls [0].GetType (), typeof (TableCell), "A21");
			Assert.AreEqual (item.Controls [0].Controls.Count, 3, "A22");
			Assert.AreEqual (item.Controls [0].Controls [1].GetType (),
					typeof (LiteralControl), "A23");
			Assert.AreEqual (((LiteralControl) item.Controls [0].Controls [1]).Text,
					"&nbsp;", "A24");

			// This is failing with an invalidcast right now. It's something related to
			// the pager thinking that it's on the last page and rendering a label instead
			next = (LinkButton) item.Controls [0].Controls [2];
			Assert.AreEqual (next.Text, "&gt;", "A25");
			Assert.AreEqual (next.CommandName, "Page", "A26");
			Assert.AreEqual (next.CommandArgument, "Next", "A27");

			prev = (LinkButton) item.Controls [0].Controls [0];
			Assert.AreEqual (prev.Text, "&lt;", "A28");
			Assert.AreEqual (prev.CommandName, "Page", "A29");
			Assert.AreEqual (prev.CommandArgument, "Prev", "A30");

			//
			// Back only
			//

			item = new DataGridItem (-1, -1, ListItemType.Pager);
			paged.PageSize = 5;
			paged.VirtualCount = 25;
			paged.AllowPaging = true;
			paged.CurrentPageIndex = 4;
			p.InitPager (item, columns.Count, paged);

			Assert.AreEqual (item.Controls.Count, 1, "A31");
			Assert.AreEqual (item.Controls [0].GetType (), typeof (TableCell), "A32");
			Assert.AreEqual (item.Controls [0].Controls.Count, 3, "A33");
			Assert.AreEqual (item.Controls [0].Controls [1].GetType (),
					typeof (LiteralControl), "A34");
			Assert.AreEqual (item.Controls [0].Controls [2].GetType (), typeof (Label), "A35");
			Assert.AreEqual (((LiteralControl) item.Controls [0].Controls [1]).Text,
					"&nbsp;", "A36");
			Assert.AreEqual (((Label) item.Controls [0].Controls [2]).Text, "&gt;", "A37");

			prev = (LinkButton) item.Controls [0].Controls [0];
			Assert.AreEqual (prev.Text, "&lt;", "A38");
			Assert.AreEqual (prev.CommandName, "Page", "A39");
			Assert.AreEqual (prev.CommandArgument, "Prev", "A40");

		}

		[Conditional ("VERBOSE_DATAGRID")]
		private void ShowControlsRecursive (Control c, int depth)
		{
			for (int i = 0; i < depth; i++)
				Console.Write ("-");

			// StringWriter sw = new StringWriter ();
			// HtmlTextWriter tw = new HtmlTextWriter (sw);

			// c.RenderControl (tw);
			// Console.WriteLine (sw.ToString ());

			Console.WriteLine (c);

			foreach (Control child in c.Controls)
				ShowControlsRecursive (child, depth + 5);
		}

		[Test]
		public void Render ()
		{
			DataGridPoker p = new DataGridPoker ();

			Assert.AreEqual (p.Render (), String.Empty, "A1");
		}
	}
}

