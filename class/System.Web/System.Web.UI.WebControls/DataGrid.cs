//
// System.Web.UI.WebControls.DataGrid.cs
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.Web.Util;
using System.Collections;
using System.Globalization;
using System.ComponentModel;

namespace System.Web.UI.WebControls {
	[Editor("System.Web.UI.Design.WebControls.DataGridComponentEditor, " + Consts.AssemblySystem_Design, typeof(System.ComponentModel.ComponentEditor))]
	[Designer("System.Web.UI.Design.WebControls.DataGridDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class DataGrid : BaseDataList, INamingContainer {

		public const string CancelCommandName = "Cancel";
		public const string DeleteCommandName = "Delete";
		public const string EditCommandName = "Edit";
		public const string SelectCommandName = "Select";
		public const string SortCommandName = "Sort";
		public const string UpdateCommandName = "Update";

		public const string PageCommandName = "Page";
		public const string NextPageCommandArgument = "Next";
		public const string PrevPageCommandArgument = "Prev";

		private static readonly object CancelCommandEvent = new object ();
		private static readonly object DeleteCommandEvent = new object ();
		private static readonly object EditCommandEvent = new object ();
		private static readonly object ItemCommandEvent = new object ();
		private static readonly object ItemCreatedEvent = new object ();
		private static readonly object ItemDataBoundEvent = new object ();
		private static readonly object PageIndexChangedEvent = new object ();
		private static readonly object SortCommandEvent = new object ();
		private static readonly object UpdateCommandEvent = new object ();

		private TableItemStyle alt_item_style;
		private TableItemStyle edit_item_style;
		private TableItemStyle footer_style;
		private TableItemStyle header_style;
		private TableItemStyle item_style;
		private TableItemStyle selected_style;
		private DataGridPagerStyle pager_style;
		
		private int page_count = 0;

		private ArrayList items_list;
		private DataGridItemCollection items;

		private ArrayList columns_list;
		private DataGridColumnCollection columns;

		private ArrayList data_source_columns_list;
		private DataGridColumnCollection data_source_columns;

		private Table render_table;
		private DataGridColumn [] render_columns;
		private TableCell pager_cell;
		private PagedDataSource paged_data_source;
		
		[DefaultValue(false)]
		[WebSysDescription ("")]
		[WebCategory ("Paging")]
		public virtual bool AllowCustomPaging {
			get { return ViewState.GetBool ("AllowCustomPaging", false); }
			set { ViewState ["AllowCustomPaging"] = value; }
		}

		[DefaultValue(false)]
 		[WebSysDescription ("")]
		[WebCategory ("Paging")]
		public virtual bool AllowPaging {
			get { return ViewState.GetBool ("AllowPaging", false); }
			set { ViewState ["AllowPaging"] = value; }
		}

		[DefaultValue(false)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual bool AllowSorting {
			get { return ViewState.GetBool ("AllowSorting", false); }
			set { ViewState ["AllowSorting"] = value; }
		}

		[DefaultValue(true)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual bool AutoGenerateColumns {
			get { return ViewState.GetBool ("AutoGenerateColumns", true); }
			set { ViewState ["AutoGenerateColumns"] = value; }
		}

		[Bindable(true)]
		[DefaultValue("")]
		[Editor("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
 		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual string BackImageUrl {
			get { return TableStyle.BackImageUrl; }
			set { TableStyle.BackImageUrl = value; }
		}

		
		[Bindable(true)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public int CurrentPageIndex {
			get { return ViewState.GetInt ("CurrentPageIndex", 0); }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["CurrentPageIndex"] = value;
			}
		}

		[DefaultValue(-1)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual int EditItemIndex {
			get { return ViewState.GetInt ("EditItemIndex", -1); }
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["EditItemIndex"] = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public int PageCount {
			get { return page_count; }
		}

		[DefaultValue(10)]
		[WebSysDescription ("")]
		[WebCategory ("Paging")]
		public virtual int PageSize {
			get { return ViewState.GetInt ("PageSize", 10); }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["PageSize"] = value;
			}
		}

		[Bindable(true)]
		[DefaultValue(-1)]
		[WebSysDescription ("")]
		[WebCategory ("Paging")]
		public virtual int SelectedIndex {
			get { return ViewState.GetInt ("SelectedIndex", -1); }
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["SelectedIndex"] = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle AlternatingItemStyle {
			get {
				if (alt_item_style == null) {
					alt_item_style = new TableItemStyle ();
					if (IsTrackingViewState)
						alt_item_style.TrackViewState ();
				}
				return alt_item_style;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle EditItemStyle {
			get {
				if (edit_item_style == null) {
					edit_item_style = new TableItemStyle ();
					if (IsTrackingViewState)
						edit_item_style.TrackViewState ();
				}
				return edit_item_style;
			}
		}

		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle FooterStyle {
			get {
				if (footer_style == null) {
					footer_style = new TableItemStyle ();
					if (IsTrackingViewState)
						footer_style.TrackViewState ();
				}
				return footer_style;
			}
		}

		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle HeaderStyle {
			get {
				if (header_style == null) {
					header_style = new TableItemStyle ();
					if (IsTrackingViewState)
						header_style.TrackViewState ();
				}
				return header_style;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle ItemStyle {
			get {
				if (item_style == null) {
					item_style = new TableItemStyle ();
					if (IsTrackingViewState)
						item_style.TrackViewState ();
				}
				return item_style;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual TableItemStyle SelectedItemStyle {
			get {
				if (selected_style == null) {
					selected_style = new TableItemStyle ();
					if (IsTrackingViewState)
						selected_style.TrackViewState ();
				}
				return selected_style;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual DataGridPagerStyle PagerStyle {
			get {
				if (pager_style == null) {
					pager_style = new DataGridPagerStyle ();
					if (IsTrackingViewState)
						pager_style.TrackViewState ();
				}
				return pager_style;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Style")]
		public virtual DataGridItemCollection Items {
			get {
				if (items == null) {
					items_list = new ArrayList ();
					items = new DataGridItemCollection (items_list);
				}
				return items;
			}
		}

		[DefaultValue (null)]
		[Editor ("System.Web.UI.Design.WebControls.DataGridColumnCollectionEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[MergableProperty (false)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual DataGridColumnCollection Columns {
			get {
				if (columns == null) {
					columns_list = new ArrayList ();
					columns = new DataGridColumnCollection (this, columns_list);
					if (IsTrackingViewState) {
						IStateManager manager = (IStateManager) columns;
						manager.TrackViewState ();
					}
				}
				return columns;
			}
		}

		private DataGridColumnCollection DataSourceColumns {
			get {
				if (data_source_columns == null) {
					data_source_columns_list = new ArrayList ();
					data_source_columns = new DataGridColumnCollection (this,
							data_source_columns_list);
					if (IsTrackingViewState) {
						IStateManager manager = (IStateManager) data_source_columns;
						manager.TrackViewState ();
					}
				}
				return data_source_columns;
			}
		}

		private Table RenderTable {
			get {
				if (render_table == null) {
					render_table = new Table ();
					if (ControlStyleCreated)
						render_table.ControlStyle.MergeWith (TableStyle);
					else
						render_table.ControlStyle.MergeWith (CreateControlStyle ());
//					Controls.Add (render_table);
				}
				return render_table;
			}
		}

		private void CreateRenderColumns (PagedDataSource paged, bool useDataSource)
		{
			if (useDataSource) {
				ArrayList columns_list = CreateColumnSet (paged, useDataSource);
				render_columns = new DataGridColumn [columns_list.Count];
				
				for (int c = 0; c < render_columns.Length; c++) {
					DataGridColumn col = (DataGridColumn) columns_list [c];
					col.Set_Owner (this);
					col.Initialize ();
					render_columns [c] = col;
				}
			} else {
				render_columns = new DataGridColumn [DataSourceColumns.Count];
				DataSourceColumns.CopyTo (render_columns, 0);
			}
		}

		[MonoTODO]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Paging")]
		public virtual DataGridItem SelectedItem {
			get {
				if (SelectedIndex == -1)
					return null;
				return Items [SelectedIndex];
			}
		}

		[Bindable(true)]
		[DefaultValue(false)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual bool ShowFooter {
			get { return ViewState.GetBool ("ShowFooter", false); }
			set { ViewState ["ShowFooter"] = value; }
		}

		[Bindable(true)]
		[DefaultValue(true)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual bool ShowHeader {
			get { return ViewState.GetBool ("ShowHeader", true); }
			set { ViewState ["ShowHeader"] = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual int VirtualItemCount {
			get { return ViewState.GetInt ("VirtualItemCount", 0); }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["VirtualItemCount"] = value;
			}
		}

		private TableStyle TableStyle {
			get { return (TableStyle) ControlStyle; }
		}
		
		protected virtual ArrayList CreateColumnSet (PagedDataSource dataSource,
				bool useDataSource)
		{
			ArrayList res = new ArrayList ();

			if (columns_list != null)
				res.AddRange (columns_list);

			if (AutoGenerateColumns) {
				if (useDataSource) {
					PropertyDescriptorCollection props = dataSource.GetItemProperties (null);
					DataSourceColumns.Clear ();
					if (props != null) {

						foreach (PropertyDescriptor d in props)
							AddPropertyToColumns (d, false);
					} else {

						//
						// The docs say that if the enumerator wasn't an ITypedList
						// that we should check for a strongly typed Item property.
						// however you can use ArrayList as your DataSource so I
						// don't think it has to be strongly typed. I think it
						// just needs to exist
						//

						props = TypeDescriptor.GetProperties (dataSource.DataSource);
						PropertyDescriptor item = props.Find ("Item", false);

						if (item != null)
							AddPropertyToColumns (item, true);
					}
				}

				if (data_source_columns != null)
					res.AddRange (data_source_columns);
			}

			return res;
		}

		private void AddPropertyToColumns (PropertyDescriptor prop, bool tothis)
		{
			BoundColumn b = new BoundColumn ();
			if (IsTrackingViewState) {
				IStateManager m = (IStateManager) b;
				m.TrackViewState ();
			}
			b.HeaderText = prop.Name;
			b.DataField = (tothis ? BoundColumn.thisExpr : prop.Name);
			b.SortExpression = prop.Name;
			DataSourceColumns.Add (b);
		}

		protected override void TrackViewState ()
		{
			base.TrackViewState ();

			if (pager_style != null)
				pager_style.TrackViewState ();
			if (header_style != null)
				header_style.TrackViewState ();
			if (footer_style != null)
				footer_style.TrackViewState ();
			if (item_style != null)
				item_style.TrackViewState ();
			if (alt_item_style != null)
				alt_item_style.TrackViewState ();
			if (selected_style != null)
				selected_style.TrackViewState ();
			if (edit_item_style != null)
				edit_item_style.TrackViewState ();

			IStateManager manager = (IStateManager) data_source_columns;
			if (manager != null)
				manager.TrackViewState ();
		}

		protected override object SaveViewState ()
		{
			object [] res = new object [10];

			res [0] = base.SaveViewState ();

			if (pager_style != null)
				res [2] = pager_style.SaveViewState ();
			if (header_style != null)
				res [3] = header_style.SaveViewState ();
			if (footer_style != null)
				res [4] = footer_style.SaveViewState ();
			if (item_style != null)
				res [5] = item_style.SaveViewState ();
			if (alt_item_style != null)
				res [6] = alt_item_style.SaveViewState ();
			if (selected_style != null)
				res [7] = selected_style.SaveViewState ();
			if (edit_item_style != null)
				res [8] = edit_item_style.SaveViewState ();

			if (data_source_columns != null) {
				IStateManager m = (IStateManager) data_source_columns;
				res [9] = m.SaveViewState ();
			}
			
			return res;
		}

		protected override void LoadViewState (object savedState)
		{
			object [] pieces = savedState as object [];

			if (pieces == null)
				return;

			base.LoadViewState (pieces [0]);

			if (pieces [2] != null)
				PagerStyle.LoadViewState (pieces [2]);
			if (pieces [3] != null)
				HeaderStyle.LoadViewState (pieces [3]);
			if (pieces [4] != null)
				FooterStyle.LoadViewState (pieces [4]);
			if (pieces [5] != null)
				ItemStyle.LoadViewState (pieces [5]);
			if (pieces [6] != null)
				AlternatingItemStyle.LoadViewState (pieces [6]);
			if (pieces [7] != null)
				SelectedItemStyle.LoadViewState (pieces [7]);
			if (pieces [8] != null)
				EditItemStyle.LoadViewState (pieces [8]);

			if (pieces [9] != null) {
				// IStateManager manager = (IStateManager) DataSourceColumns;
				// manager.LoadViewState (pieces [9]);
				object [] cols = (object []) pieces [9];
				foreach (object o in cols) {
					BoundColumn c = new BoundColumn ();
					((IStateManager) c).LoadViewState (o);
					DataSourceColumns.Add (c);
				}
			}
		}

		protected override Style CreateControlStyle ()
		{
			TableStyle res = new TableStyle (ViewState);
			res.GridLines = GridLines.Both;
			res.CellSpacing = 0;
			return res;
		}

		protected virtual void InitializeItem (DataGridItem item, DataGridColumn [] columns)
		{
			for (int i = 0; i < columns.Length; i++) {
				TableCell cell  = new TableCell ();
				columns [i].InitializeCell (cell, i, item.ItemType);
				item.Cells.Add (cell);
			}
		}

		protected virtual void InitializePager (DataGridItem item, int columnSpan,
				PagedDataSource pagedDataSource)
		{
			if (pager_cell == null) {
				if (PagerStyle.Mode == PagerMode.NextPrev)
					pager_cell = InitializeNextPrevPager (item,
							columnSpan, pagedDataSource);
				else
					pager_cell = InitializeNumericPager (item,
							columnSpan, pagedDataSource);
			}

			item.Controls.Add (pager_cell);
		}

		private TableCell InitializeNumericPager (DataGridItem item, int columnSpan,
				PagedDataSource paged)
		{
			TableCell res = new TableCell ();
			res.ColumnSpan = columnSpan;
			int start;
			int end;
			int button_count = PagerStyle.PageButtonCount;
			int t = paged.CurrentPageIndex / button_count;
			start = t * button_count;
			end = start + button_count;

			if (end > paged.PageCount)
				end = paged.PageCount;

			if (start > 0) {
				LinkButton link = new LinkButton ();
				link.Text = "...";
				link.CommandName = PageCommandName;
				link.CommandArgument = (start - 1).ToString (CultureInfo.InvariantCulture);
				link.CausesValidation = false;
				res.Controls.Add (link);
				res.Controls.Add (new LiteralControl ("&nbsp;"));
			}

			for (int i = start; i < end; i++) {
				Control number = null;
				string page = (i + 1).ToString (CultureInfo.InvariantCulture);
				if (i != paged.CurrentPageIndex) {
					LinkButton link = new LinkButton ();
					link.Text = page;
					link.CommandName = PageCommandName;
					link.CommandArgument = page;
					link.CausesValidation = false;
					number = link;
				} else {
					number = new LiteralControl (page);
				}

				res.Controls.Add (number);
				if (i < end - 1)
					res.Controls.Add (new LiteralControl ("&nbsp;"));
			}

			if (end < paged.PageCount) {
				res.Controls.Add (new LiteralControl ("&nbsp;"));
				LinkButton link = new LinkButton ();
				link.Text = "...";
				link.CommandName = PageCommandName;
				link.CommandArgument = (end + 1).ToString (CultureInfo.InvariantCulture);
				link.CausesValidation = false;
				res.Controls.Add (link);
			}

			return res;
		}

		private TableCell InitializeNextPrevPager (DataGridItem item, int columnSpan,
				PagedDataSource paged)
		{
			TableCell res = new TableCell ();
			res.ColumnSpan = columnSpan;

			Control prev;
			Control next;

			if (paged.IsFirstPage) {
				Label l = new Label ();
				l.Text = PagerStyle.PrevPageText;
				prev = l;
			} else {
				LinkButton l = new LinkButton ();
				l.Text = PagerStyle.PrevPageText;
				l.CommandName = PageCommandName;
				l.CommandArgument = PrevPageCommandArgument;
				l.CausesValidation = false;
				prev = l;
			}

			if (paged.Count > 0 && !paged.IsLastPage) {
				LinkButton l = new LinkButton ();
				l.Text = PagerStyle.NextPageText;
				l.CommandName = PageCommandName;
				l.CommandArgument = NextPageCommandArgument;
				l.CausesValidation = false;
				next = l;
			} else {
				Label l = new Label ();
				l.Text = PagerStyle.NextPageText;
				next = l;
			}

			res.Controls.Add (prev);
			res.Controls.Add (new LiteralControl ("&nbsp;"));
			res.Controls.Add (next);

			return res;
		}
				
		protected virtual DataGridItem CreateItem (int itemIndex, int dataSourceIndex,
				ListItemType itemType)
		{
			DataGridItem res = new DataGridItem (itemIndex, dataSourceIndex, itemType);
			return res;
		}

		private DataGridItem CreateItem (int item_index, int data_source_index,
				ListItemType type, bool data_bind, object data_item,
				PagedDataSource paged)
		{
			DataGridItem res = CreateItem (item_index, data_source_index, type);
			DataGridItemEventArgs args = new DataGridItemEventArgs (res);

			if (type != ListItemType.Pager) {
				OnItemCreated (args);
				InitializeItem (res, render_columns);
			} else {
				InitializePager (res, render_columns.Length, paged);
				if (pager_style != null)
					res.ApplyStyle (pager_style);
			}

			// Add before the column is bound, so that the
			// value is saved in the viewstate
			RenderTable.Controls.Add (res);

			if (data_bind) {
				res.DataItem = data_item;
				if (data_item != null)
					res.DataBind ();
				OnItemDataBound (args);
			}

			return res;
		}

		protected override void CreateControlHierarchy (bool useDataSource)
		{
			DataGridItem item;

			RenderTable.Controls.Clear ();

			IEnumerable data_source;
			if (useDataSource) {
				data_source = DataSourceResolver.ResolveDataSource (DataSource,
						DataMember);
			} else {
				// This is a massive waste
				data_source = new object [ViewState.GetInt ("Items", 0)];
			}

			paged_data_source = new PagedDataSource ();
			paged_data_source.AllowPaging = AllowPaging;
			paged_data_source.AllowCustomPaging = AllowCustomPaging;
			paged_data_source.DataSource = data_source;
			paged_data_source.CurrentPageIndex = CurrentPageIndex;
			paged_data_source.PageSize = PageSize;
			paged_data_source.VirtualCount = VirtualItemCount;

			CreateRenderColumns (paged_data_source, useDataSource);

			item = CreateItem (-1, -1, ListItemType.Pager, false, null,
					paged_data_source);
			item = CreateItem (-1, -1, ListItemType.Header, useDataSource, null,
					paged_data_source);

			// No indexer on PagedDataSource so we have to do
			// this silly foreach and index++
			int index = 0;
			foreach (object ds in paged_data_source) {
				ListItemType type = ListItemType.Item;

				if (this.EditItemIndex == index) 
					type = ListItemType.EditItem;
				else if (index % 2 != 0) 
					type = ListItemType.AlternatingItem;

				item = CreateItem (index, index, type, useDataSource, ds, paged_data_source);
				index++;
			}

			item = CreateItem (-1, -1, ListItemType.Footer, useDataSource, null, paged_data_source);
			item = CreateItem (-1, -1, ListItemType.Pager, false, null, paged_data_source);

			Controls.Add (RenderTable);
			ViewState ["Items"] = paged_data_source.DataSourceCount;

			pager_cell = null;
		}

		protected override void PrepareControlHierarchy ()
		{
			if (render_table == null)
				return;

			bool top_pager = true;
			foreach (DataGridItem item in render_table.Rows) {
				
				switch (item.ItemType) {
				case ListItemType.Item:
					item.ApplyStyle (ItemStyle);
					break;
				case ListItemType.AlternatingItem:
					item.ApplyStyle (AlternatingItemStyle);
					break;
				case ListItemType.EditItem:
					item.ApplyStyle (EditItemStyle);
					break;
				case ListItemType.Footer:
					if (!ShowFooter)
						item.Visible = false;
					item.ApplyStyle (FooterStyle);
					break;
				case ListItemType.Header:
					if (!ShowHeader)
						item.Visible = false;
					item.ApplyStyle (HeaderStyle);
					break;
				case ListItemType.SelectedItem:
					item.ApplyStyle (SelectedItemStyle);
					break;
				case ListItemType.Pager:

					if (!paged_data_source.IsPagingEnabled) {
						item.Visible = false;
					} else {
						if (top_pager)
							item.Visible = (PagerStyle.Position !=
									PagerPosition.Bottom);
						else
							item.Visible = (PagerStyle.Position !=
									PagerPosition.Top);
						top_pager = false;
					}

					item.ApplyStyle (PagerStyle);
					break;
				}
			}
		}

		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			DataGridCommandEventArgs de = e as DataGridCommandEventArgs;

			if (de == null)
				return false;

			string cn = de.CommandName;
			CultureInfo inv = CultureInfo.InvariantCulture;

			if (String.Compare (cn, CancelCommandName, true, inv) == 0) {
				OnCancelCommand (de);
				return true;
			} else if (String.Compare (cn, DeleteCommandName, true, inv) == 0) {
				OnDeleteCommand (de);
				return true;
			} else if (String.Compare (cn, EditCommandName, true, inv) == 0) {
				OnEditCommand (de);
				return true;
			} else if (String.Compare (cn, "Item", true, inv) == 0) {
				OnItemCommand (de);
				return true;
			} else if (String.Compare (cn, SelectCommandName, true, inv) == 0) {
				OnSelectedIndexChanged (de);
				return true;
			} else if (String.Compare (cn, SortCommandName, true, inv) == 0) {
				DataGridSortCommandEventArgs se = new DataGridSortCommandEventArgs (de.CommandSource, de);
				OnSortCommand (se);
				return true;
			} else if (String.Compare (cn, UpdateCommandName, true, inv) == 0) {
				OnUpdateCommand (de);
			} else if (String.Compare (cn, PageCommandName, true, inv) == 0) {
				int new_index;
				if (String.Compare ((string) de.CommandArgument,
						    NextPageCommandArgument, true, inv) == 0) {
					new_index = CurrentPageIndex + 1;
				} else if (String.Compare ((string) de.CommandArgument,
							   PrevPageCommandArgument, true, inv) == 0) {
					new_index = CurrentPageIndex - 1;
				} else {
					// It seems to just assume it's an int and parses, no
					// checks to make sure its valid or anything.
					//  also it's always one less then specified, not sure
					// why that is.
					new_index = Int32.Parse ((string) de.CommandArgument, inv) - 1;
				}
				DataGridPageChangedEventArgs pc = new DataGridPageChangedEventArgs (
					de.CommandSource, new_index);
				OnPageIndexChanged (pc);
				return true;
			}

			return false;
		}

		protected virtual void OnCancelCommand (DataGridCommandEventArgs e)
		{
			DataGridCommandEventHandler handler = (DataGridCommandEventHandler) Events [CancelCommandEvent];
			if (handler != null)
				handler (this, e);
		}

		protected virtual void OnDeleteCommand (DataGridCommandEventArgs e)
		{
			DataGridCommandEventHandler handler = (DataGridCommandEventHandler) Events [DeleteCommandEvent];
			if (handler != null)
				handler (this, e);
		}

		protected virtual void OnEditCommand (DataGridCommandEventArgs e)
		{
			DataGridCommandEventHandler handler = (DataGridCommandEventHandler) Events [EditCommandEvent];
			if (handler != null)
				handler (this, e);
		}

		protected virtual void OnItemCommand (DataGridCommandEventArgs e)
		{
			DataGridCommandEventHandler handler = (DataGridCommandEventHandler) Events [ItemCommandEvent];
			if (handler != null)
				handler (this, e);
		}

		protected virtual void OnItemCreated (DataGridItemEventArgs e)
		{
			DataGridItemEventHandler handler = (DataGridItemEventHandler) Events [ItemCreatedEvent];
			if (handler != null)
				handler (this, e);
		}

		protected virtual void OnItemDataBound (DataGridItemEventArgs e)
		{
			DataGridItemEventHandler handler = (DataGridItemEventHandler) Events [ItemDataBoundEvent];
			if (handler != null)
				handler (this, e);
		}

		protected virtual void OnPageIndexChanged (DataGridPageChangedEventArgs e)
		{
			DataGridPageChangedEventHandler handler = (DataGridPageChangedEventHandler) Events [PageIndexChangedEvent];
			if (handler != null)
				handler (this, e);
		}

		protected virtual void OnSortCommand (DataGridSortCommandEventArgs e)
		{
			DataGridSortCommandEventHandler handler = (DataGridSortCommandEventHandler) Events [SortCommandEvent];
			if (handler != null)
				handler (this, e);
		}

		protected virtual void OnUpdateCommand (DataGridCommandEventArgs e)
		{
			DataGridCommandEventHandler handler = (DataGridCommandEventHandler) Events [UpdateCommandEvent];
			if (handler != null)
				handler (this, e);
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataGridCommandEventHandler CancelCommand {
			add { Events.AddHandler (CancelCommandEvent, value); }
			remove { Events.RemoveHandler (CancelCommandEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataGridCommandEventHandler DeleteCommand {
			add { Events.AddHandler (DeleteCommandEvent, value); }
			remove { Events.RemoveHandler (DeleteCommandEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataGridCommandEventHandler EditCommand {
			add { Events.AddHandler (EditCommandEvent, value); }
			remove { Events.RemoveHandler (EditCommandEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataGridCommandEventHandler ItemCommand {
			add { Events.AddHandler (ItemCommandEvent, value); }
			remove { Events.RemoveHandler (ItemCommandEvent, value); }
			
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataGridItemEventHandler ItemCreated {
			add { Events.AddHandler (ItemCreatedEvent, value); }
			remove { Events.RemoveHandler (ItemCreatedEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataGridItemEventHandler ItemDataBound {
			add { Events.AddHandler (ItemDataBoundEvent, value); }
			remove { Events.RemoveHandler (ItemDataBoundEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataGridPageChangedEventHandler PageIndexChanged {
			add { Events.AddHandler (PageIndexChangedEvent, value); }
			remove { Events.RemoveHandler (PageIndexChangedEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataGridSortCommandEventHandler SortCommand {
			add { Events.AddHandler (SortCommandEvent, value); }
			remove { Events.RemoveHandler (SortCommandEvent, value); }
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event DataGridCommandEventHandler UpdateCommand {
			add { Events.AddHandler (UpdateCommandEvent, value); }
			remove { Events.AddHandler (UpdateCommandEvent, value); }
		}

		[Obsolete]
		internal void OnColumnsChanged ()
		{
		}

		[Obsolete]
		internal void OnPagerChanged ()
		{
		}
	}
}

