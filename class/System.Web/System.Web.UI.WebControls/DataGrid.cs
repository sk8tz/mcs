/**
 * Namespace: System.Web.UI.WebControls
 * Class:     DataGrid
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  95%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.ComponentModel;
using System.Reflection;

namespace System.Web.UI.WebControls
{
	//TODO: [Designer("??")]
	//TODO: [Editor("??")]
	[DefaultEvent("SelectedIndexChanged")]
	[DefaultProperty("DataSource")]
	[ParseChildren(true)]
	[PersistChildren(false)]
	public class DataGrid : BaseDataList, INamingContainer
	{
		public const string CancelCommandName       = "Cancel";
		public const string DeleteCommandName       = "Delete";
		public const string EditCommandName         = "Edit";
		public const string NextPageCommandArgument = "Next";
		public const string PageCommandName         = "Page";
		public const string PrevPageCommandArgument = "Prev";
		public const string SelectCommandName       = "Select";
		public const string SortCommandName         = "Sort";
		public const string UpdateCommandName       = "Update";

		private TableItemStyle alternatingItemStyle;
		private TableItemStyle editItemStyle;
		private TableItemStyle headerStyle;
		private TableItemStyle footerStyle;
		private TableItemStyle itemStyle;
		private TableItemStyle selectedItemStyle;
		private DataGridPagerStyle pagerStyle;

		private DataGridColumnCollection columns;
		private ArrayList                columnsArrayList;
		private DataGridItemCollection   items;
		private ArrayList                itemsArrayList;
		private PagedDataSource          pagedDataSource;

		private ArrayList   autoGenColsArrayList;
		private IEnumerator storedData;
		private object      storedDataFirst;
		private bool        storedDataValid;

		private static readonly object CancelCommandEvent    = new object();
		private static readonly object DeleteCommandEvent    = new object();
		private static readonly object EditCommandEvent      = new object();
		private static readonly object ItemCommandEvent      = new object();
		private static readonly object ItemCreatedEvent      = new object();
		private static readonly object ItemDataBoundEvent    = new object();
		private static readonly object PageIndexChangedEvent = new object();
		private static readonly object SortCommandEvent      = new object();
		private static readonly object UpdateCommandEvent    = new object();

		public DataGrid(): base()
		{
		}

		public virtual bool AllowCustomPaging
		{
			get
			{
				object o = ViewState["AllowCustomPaging"];
				if(o != null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["AllowCustomPaging"] = value;
			}
		}

		public virtual bool AllowPaging
		{
			get
			{
				object o = ViewState["AllowPaging"];
				if(o != null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["AllowPaging"] = value;
			}
		}

		public virtual bool AllowSorting
		{
			get
			{
				object o = ViewState["AllowSorting"];
				if(o != null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["AllowSorting"] = value;
			}
		}

		public virtual TableItemStyle AlternatingItemStyle
		{
			get
			{
				if(alternatingItemStyle == null)
				{
					alternatingItemStyle = new TableItemStyle();
				}
				if(IsTrackingViewState)
				{
					alternatingItemStyle.TrackViewState();
				}
				return alternatingItemStyle;
			}
		}

		public virtual bool AutoGenerateColumns
		{
			get
			{
				object o = ViewState["AutoGenerateColumns"];
				if(o != null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["AutoGenerateColumns"] = value;
			}
		}

		public virtual string BackImageUrl
		{
			get
			{
				object o = ViewState["BackImageUrl"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["BackImageUrl"] = value;
			}
		}

		public virtual DataGridColumnCollection Columns
		{
			get
			{
				if(columns == null)
				{
					columnsArrayList = new ArrayList();
					columns = new DataGridColumnCollection(this, columnsArrayList);
					if(IsTrackingViewState)
					{
						((IStateManager)columns).TrackViewState();
					}
				}
				return columns;
			}
		}

		public int CurrentPageIndex
		{
			get
			{
				object o = ViewState["CurrentPageIndex"];
				if(o != null)
					return (int)o;
				return 0;
			}
			set
			{
				if(value < 0)
					throw new ArgumentOutOfRangeException();
				ViewState["CurrentPageIndex"] = value;
			}
		}

		public virtual int EditItemIndex
		{
			get
			{
				object o = ViewState["EditItemIndex"];
				if(o != null)
					return (int)o;
				return -1;
			}
			set
			{
				if(value < -1)
					throw new ArgumentOutOfRangeException();
				ViewState["EditItemIndex"] = value;
			}
		}

		public virtual TableItemStyle EditItemStyle
		{
			get
			{
				if(editItemStyle == null)
				{
					editItemStyle = new TableItemStyle();
					if(IsTrackingViewState)
					{
						editItemStyle.TrackViewState();
					}
				}
				return editItemStyle;
			}
		}

		public virtual TableItemStyle FooterStyle
		{
			get
			{
				if(footerStyle == null)
				{
					footerStyle = new TableItemStyle();
					if(IsTrackingViewState)
					{
						footerStyle.TrackViewState();
					}
				}
				return footerStyle;
			}
		}

		public virtual TableItemStyle HeaderStyle
		{
			get
			{
				if(headerStyle == null)
				{
					headerStyle = new TableItemStyle();
					if(IsTrackingViewState)
					{
						headerStyle.TrackViewState();
					}
				}
				return headerStyle;
			}
		}

		public virtual DataGridItemCollection Items
		{
			get
			{
				if(items == null)
				{
					if(itemsArrayList == null)
						EnsureChildControls();
					if(itemsArrayList == null)
					{
						itemsArrayList = new ArrayList();
					}
					items = new DataGridItemCollection(itemsArrayList);
				}
				return items;
			}
		}

		public virtual TableItemStyle ItemStyle
		{
			get
			{
				if(itemStyle == null)
				{
					itemStyle = new TableItemStyle();
					if(IsTrackingViewState)
					{
						itemStyle.TrackViewState();
					}
				}
				return itemStyle;
			}
		}

		public int PageCount
		{
			get
			{
				if(pagedDataSource != null)
				{
					return pagedDataSource.PageCount;
				}
				object o = ViewState["PageCount"];
				if(o != null)
					return (int)o;
				return 0;
			}
		}

		public virtual DataGridPagerStyle PagerStyle
		{
			get
			{
				if(pagerStyle == null)
				{
					pagerStyle = new DataGridPagerStyle(this);
					if(IsTrackingViewState)
					{
						pagerStyle.TrackViewState();
					}
				}
				return pagerStyle;
			}
		}

		public virtual int PageSize
		{
			get
			{
				object o = ViewState["PageSize"];
				if(o != null)
					return (int)o;
				return 10;
			}
			set
			{
				if(value < 1)
					throw new ArgumentOutOfRangeException();
				ViewState["PageSize"] = value;
			}
		}

		public virtual int SelectedIndex
		{
			get
			{
				object o = ViewState["SelectedIndex"];
				if(o != null)
					return (int)o;
				return -1;
			}
			set
			{
				if(value < -1)
					throw new ArgumentOutOfRangeException();
				int prevVal = SelectedIndex;
				ViewState["SelectedIndex"] = value;
				if(items != null)
				{
					if(prevVal !=-1 && prevVal < items.Count)
					{
						DataGridItem prev = (DataGridItem)items[prevVal];
						if(prev.ItemType != ListItemType.EditItem)
						{
							ListItemType newType = ListItemType.Item;
							if( (prevVal % 2) != 0)
							{
								newType = ListItemType.AlternatingItem;
							}
							prev.SetItemType(newType);
						}
					}
				}
			}
		}

		public virtual DataGridItem SelectedItem
		{
			get
			{
				if(SelectedIndex == -1)
					return null;
				return Items[SelectedIndex];
			}
		}

		public virtual TableItemStyle SelectedItemStyle
		{
			get
			{
				if(selectedItemStyle == null)
				{
					selectedItemStyle = new TableItemStyle();
					if(IsTrackingViewState)
					{
						selectedItemStyle.TrackViewState();
					}
				}
				return selectedItemStyle;
			}
		}

		public virtual bool ShowFooter
		{
			get
			{
				object o = ViewState["ShowFooter"];
				if(o != null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["ShowFooter"] = value;
			}
		}

		public virtual bool ShowHeader
		{
			get
			{
				object o = ViewState["ShowHeader"];
				if(o != null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["ShowHeader"] = value;
			}
		}

		public virtual int VirtualItemCount
		{
			get
			{
				object o = ViewState["VirtualItemCount"];
				if(o != null)
					return (int)o;
				return 0;
			}
			set
			{
				if(value < 0)
					throw new ArgumentOutOfRangeException();
				ViewState["VirtualItemCount"] = value;
			}
		}

		public event DataGridCommandEventHandler CancelCommand
		{
			add
			{
				Events.AddHandler(CancelCommandEvent, value);
			}
			remove
			{
				Events.RemoveHandler(CancelCommandEvent, value);
			}
		}

		public event DataGridCommandEventHandler DeleteCommand
		{
			add
			{
				Events.AddHandler(DeleteCommandEvent, value);
			}
			remove
			{
				Events.RemoveHandler(DeleteCommandEvent, value);
			}
		}

		public event DataGridCommandEventHandler EditCommand
		{
			add
			{
				Events.AddHandler(EditCommandEvent, value);
			}
			remove
			{
				Events.RemoveHandler(EditCommandEvent, value);
			}
		}

		public event DataGridCommandEventHandler ItemCommand
		{
			add
			{
				Events.AddHandler(ItemCommandEvent, value);
			}
			remove
			{
				Events.RemoveHandler(ItemCommandEvent, value);
			}
		}

		public event DataGridCommandEventHandler ItemCreated
		{
			add
			{
				Events.AddHandler(ItemCreatedEvent, value);
			}
			remove
			{
				Events.RemoveHandler(ItemCreatedEvent, value);
			}
		}

		public event DataGridCommandEventHandler ItemDataBound
		{
			add
			{
				Events.AddHandler(ItemDataBoundEvent, value);
			}
			remove
			{
				Events.RemoveHandler(ItemDataBoundEvent, value);
			}
		}

		public event DataGridCommandEventHandler PageIndexChanged
		{
			add
			{
				Events.AddHandler(PageIndexChangedEvent, value);
			}
			remove
			{
				Events.RemoveHandler(PageIndexChangedEvent, value);
			}
		}

		public event DataGridCommandEventHandler SortCommand
		{
			add
			{
				Events.AddHandler(SortCommandEvent, value);
			}
			remove
			{
				Events.RemoveHandler(SortCommandEvent, value);
			}
		}

		public event DataGridCommandEventHandler UpdateCommand
		{
			add
			{
				Events.AddHandler(UpdateCommandEvent, value);
			}
			remove
			{
				Events.RemoveHandler(UpdateCommandEvent, value);
			}
		}

		protected override Style CreateControlStyle()
		{
			TableStyle style = new TableStyle(ViewState);
			style.GridLines = GridLines.Both;
			style.CellSpacing = 0;
			return style;
		}

		protected override void LoadViewState(object savedState)
		{
			if(savedState != null)
			{
				object[] states = (object[])savedState;
				if(states != null)
				{
					base.LoadViewState(states[0]);
					if(columns != null)
						((IStateManager)columns).LoadViewState(states[1]);
					if(pagerStyle != null)
						pagerStyle.LoadViewState(states[2]);
					if(headerStyle != null)
						headerStyle.LoadViewState(states[3]);
					if(footerStyle != null)
						footerStyle.LoadViewState(states[4]);
					if(itemStyle != null)
						itemStyle.LoadViewState(states[5]);
					if(alternatingItemStyle != null)
						alternatingItemStyle.LoadViewState(states[6]);
					if(selectedItemStyle != null)
						selectedItemStyle.LoadViewState(states[7]);
					if(editItemStyle != null)
						editItemStyle.LoadViewState(states[8]);
				}
			}
		}

		protected override object SaveViewState()
		{
			object[] states = new object[9];
			states[0] = SaveViewState();
			states[1] = (columns == null ? null : ((IStateManager)columns).SaveViewState());
			states[2] = (pagerStyle == null ? null : pagerStyle.SaveViewState());
			states[3] = (headerStyle == null ? null : headerStyle.SaveViewState());
			states[4] = (footerStyle == null ? null : footerStyle.SaveViewState());
			states[5] = (itemStyle == null ? null : itemStyle.SaveViewState());
			states[6] = (alternatingItemStyle == null ? null : alternatingItemStyle.SaveViewState());
			states[7] = (selectedItemStyle == null ? null : selectedItemStyle.SaveViewState());
			states[8] = (editItemStyle == null ? null : editItemStyle.SaveViewState());
			return states;
		}

		protected override void TrackViewState()
		{
			base.TrackViewState();
			if(alternatingItemStyle != null)
			{
				alternatingItemStyle.TrackViewState();
			}
			if(editItemStyle != null)
			{
				editItemStyle.TrackViewState();
			}
			if(headerStyle != null)
			{
				headerStyle.TrackViewState();
			}
			if(footerStyle != null)
			{
				footerStyle.TrackViewState();
			}
			if(itemStyle != null)
			{
				itemStyle.TrackViewState();
			}
			if(selectedItemStyle != null)
			{
				selectedItemStyle.TrackViewState();
			}
			if(pagerStyle != null)
			{
				pagerStyle.TrackViewState();
			}

			if(columns != null)
			{
				((IStateManager)columns).TrackViewState();
			}
		}

		protected override bool OnBubbleEvent(object source, EventArgs e)
		{
			bool retVal = false;
			if(e is DataGridCommandEventArgs)
			{
				DataGridCommandEventArgs ea = (DataGridCommandEventArgs)e;
				retVal = true;
				OnItemCommand(ea);
				string cmd = ea.CommandName;
				if(String.Compare(cmd, "select", true) == 0)
				{
					SelectedIndex = ea.Item.ItemIndex;
					OnSelectedIndexChanged(EventArgs.Empty);
				} else if(String.Compare(cmd,"page", true) == 0)
				{
					int    cIndex = CurrentPageIndex;
					string cea = (string) ea.CommandArgument;
					if(String.Compare(cea, "prev", true) == 0)
					{
						cIndex--;
					} else if(String.Compare(cea, "next", true) == 0)
					{
						cIndex++;
					}
					OnPageIndexChanged(new DataGridPageChangedEventArgs(source, cIndex));
				} else if(String.Compare(cmd, "sort", true) == 0)
				{
					OnSortCommand(new DataGridSortCommandEventArgs(source, ea));
				} else if(String.Compare(cmd, "edit", true) == 0)
				{
					OnEditCommand(ea);
				} else if(String.Compare(cmd, "update", true) == 0)
				{
					OnUpdateCommand(ea);
				} else if(String.Compare(cmd, "cancel", true) == 0)
				{
					OnCancelCommand(ea);
				} else if(String.Compare(cmd, "delete", true) == 0)
				{
					OnDeleteCommand(ea);
				}
			}
			return retVal;
		}

		protected virtual void OnCancelCommand(DataGridCommandEventArgs e)
		{
			if(Events != null)
			{
				DataGridCommandEventHandler dceh = (DataGridCommandEventHandler)(Events[CancelCommandEvent]);
				if(dceh != null)
					dceh(this, e);
			}
		}

		protected virtual void OnDeleteCommand(DataGridCommandEventArgs e)
		{
			if(Events != null)
			{
				DataGridCommandEventHandler dceh = (DataGridCommandEventHandler)(Events[DeleteCommandEvent]);
				if(dceh != null)
					dceh(this, e);
			}
		}

		protected virtual void OnEditCommand(DataGridCommandEventArgs e)
		{
			if(Events != null)
			{
				DataGridCommandEventHandler dceh = (DataGridCommandEventHandler)(Events[EditCommandEvent]);
				if(dceh != null)
					dceh(this, e);
			}
		}

		protected virtual void OnItemCommand(DataGridCommandEventArgs e)
		{
			if(Events != null)
			{
				DataGridCommandEventHandler dceh = (DataGridCommandEventHandler)(Events[ItemCommandEvent]);
				if(dceh != null)
					dceh(this, e);
			}
		}

		protected virtual void OnItemCreated(DataGridItemEventArgs e)
		{
			if(Events != null)
			{
				DataGridItemEventHandler dceh = (DataGridItemEventHandler)(Events[ItemCreatedEvent]);
				if(dceh != null)
					dceh(this, e);
			}
		}

		protected virtual void OnItemDataBound(DataGridItemEventArgs e)
		{
			if(Events != null)
			{
				DataGridItemEventHandler dceh = (DataGridItemEventHandler)(Events[ItemDataBoundEvent]);
				if(dceh != null)
					dceh(this, e);
			}
		}

		protected virtual void OnPageIndexChanged(DataGridPageChangedEventArgs e)
		{
			if(Events != null)
			{
				DataGridPageChangedEventHandler dceh = (DataGridPageChangedEventHandler)(Events[PageIndexChangedEvent]);
				if(dceh != null)
					dceh(this, e);
			}
		}

		protected virtual void OnSortCommand(DataGridSortCommandEventArgs e)
		{
			if(Events != null)
			{
				DataGridSortCommandEventHandler dceh = (DataGridSortCommandEventHandler)(Events[SortCommandEvent]);
				if(dceh != null)
					dceh(this, e);
			}
		}

		protected virtual void OnUpdateCommand(DataGridCommandEventArgs e)
		{
			if(Events != null)
			{
				DataGridCommandEventHandler dceh = (DataGridCommandEventHandler)(Events[UpdateCommandEvent]);
				if(dceh != null)
					dceh(this, e);
			}
		}

		protected override void PrepareControlHierarchy()
		{
			if(Controls.Count > 0)
			{
				Table display = (Table)Controls[0];
				display.CopyBaseAttributes(this);
				if(ControlStyleCreated)
				{
					display.ApplyStyle(ControlStyle);
				} else
				{
					display.GridLines   = GridLines.Both;
					display.CellSpacing = 0;
				}
				TableRowCollection rows = display.Rows;
				if(rows.Count > 0)
				{
					int              nCols = Columns.Count;
					DataGridColumn[] cols = new DataGridColumn[nCols];
					Style            deployStyle;
					int              counter;
					if(nCols > 0)
					{
						Columns.CopyTo(cols, 0);
					}
					deployStyle = null;
					if(alternatingItemStyle != null)
					{
						deployStyle = new TableItemStyle();
						deployStyle.CopyFrom(itemStyle);
						deployStyle.CopyFrom(alternatingItemStyle);
					} else
					{
						deployStyle = itemStyle;
					}
					for(counter = 0; counter < rows.Count; counter++)
					{
						PrepareControlHierarchyForItem(cols, (DataGridItem) rows[counter], counter, deployStyle);
					}
				}
			}
		}

		private void PrepareControlHierarchyForItem(DataGridColumn[] cols, DataGridItem item, int index, Style deployStyle)
		{
			switch(item.ItemType)
			{
				case ListItemType.Header: if(!ShowHeader)
				                          {
				                          	item.Visible = false;
				                          	break;
				                          }
				                          if(headerStyle != null)
				                          {
				                          	item.MergeStyle(headerStyle);
				                          	goto case ListItemType.Separator;
				                          }
				                          break;
				case ListItemType.Footer: if(!ShowFooter)
				                          {
				                          	item.Visible = false;
				                          	break;
				                          }
				                          if(footerStyle != null)
				                          {
				                          	item.MergeStyle(footerStyle);
				                          	goto case ListItemType.Separator;
				                          }
				                          break;
				case ListItemType.Item  : item.MergeStyle(itemStyle);
				                          goto case ListItemType.Separator;
				case ListItemType.AlternatingItem:
				                          item.MergeStyle(deployStyle);
				                          goto case ListItemType.Separator;
				case ListItemType.SelectedItem:
				                          Style selStyle = new TableItemStyle();
				                          if( (item.ItemIndex % 2) == 0)
				                          {
				                          	selStyle.CopyFrom(itemStyle);
				                          } else
				                          {
				                          	selStyle.CopyFrom(deployStyle);
				                          }
				                          selStyle.CopyFrom(selectedItemStyle);
				                          item.MergeStyle(selStyle);
				                          goto case ListItemType.Separator;
				case ListItemType.EditItem:
				                          Style edStyle = new TableItemStyle();
				                          if( (item.ItemIndex % 2) == 0)
				                          {
				                          	edStyle.CopyFrom(itemStyle);
				                          } else
				                          {
				                          	edStyle.CopyFrom(deployStyle);
				                          }
				                          edStyle.CopyFrom(editItemStyle);
				                          item.MergeStyle(edStyle);
				                          goto case ListItemType.Separator;
				case ListItemType.Pager : if(pagerStyle == null)
				                          {
				                          	break;
				                          }
				                          if(!pagerStyle.Visible)
				                          {
				                          	item.Visible = false;
				                          }
				                          if(index == 0)
				                          {
				                          	if(!pagerStyle.IsPagerOnTop)
				                          	{
				                          		item.Visible = false;
				                          		break;
				                          	}
				                          } else
				                          {
				                          	if(!pagerStyle.IsPagerOnBottom)
				                          	{
				                          		item.Visible = false;
				                          		break;
				                          	}
				                          }
				                          item.MergeStyle(pagerStyle);
				                          goto case ListItemType.Separator;
				case ListItemType.Separator:
				                          TableCellCollection cells = item.Cells;
				                          int cellCount = cells.Count;
				                          if(cellCount > 0 && item.ItemType != ListItemType.Pager)
				                          {
				                          	for(int i = 0; i < cellCount; i++)
				                          	{
				                          		Style colStyle = null;
				                          		if(cols[i].Visible)
				                          		{
				                          			switch (item.ItemType)
				                          			{
				                          				case ListItemType.Header : colStyle = cols[i].HeaderStyleInternal;
				                          				                           break;
				                          				case ListItemType.Footer : colStyle = cols[i].FooterStyleInternal;
				                          				                           break;
				                          				default                  : colStyle = cols[i].ItemStyleInternal;
				                          				                           break;
				                          			}
				                          			item.MergeStyle(colStyle);
				                          		} else
				                          		{
				                          			cells[i].Visible = false;
				                          		}
				                          	}
				                          }
				                          break;
				default                 : goto case ListItemType.Separator;
			}
		}

		[MonoTODO]
		protected override void CreateControlHierarchy(bool useDataSource)
		{
			IEnumerator pageSource;
			int         itemCount;
			int         dsItemCount;
			ArrayList   dataKeys;
			ArrayList   columns;
			IEnumerable resolvedDS;
			ICollection collResolvedDS;
			int         pageDSCount;
			int         colCount;
			DataGridColumn[] cols;
			Table       deployTable;
			TableRowCollection deployRows;
			ListItemType deployType;
			int         indexCounter;
			string      dkField;
			bool        dsUse;
			bool        pgEnabled;
			int         editIndex;
			int         selIndex;

			pagedDataSource = CreatePagedDataSource();
			pageSource      = null;
			itemCount       = -1;
			dsItemCount     = -1;
			dataKeys        = DataKeysArray;
			columns         = null;
			if(itemsArrayList != null)
			{
				itemsArrayList.Clear();
			} else
			{
				itemsArrayList = new ArrayList();
			}
			if(!useDataSource)
			{
				itemCount    = (int) ViewState["_DataGrid_ItemCount"];
				pageDSCount  = (int) ViewState["_DataGrid_DataSource_Count"];
				if(itemCount != -1)
				{
					if(pagedDataSource.IsCustomPagingEnabled)
					{
						// I may need a dummy pagedDS
					}
				}
			} else
			{
				//TODO: Use Data Source
			}
			throw new NotImplementedException();
		}

		private PagedDataSource CreatePagedDataSource()
		{
			PagedDataSource retVal;

			retVal = new PagedDataSource();
			retVal.CurrentPageIndex = CurrentPageIndex;
			retVal.PageSize         = PageSize;
			retVal.AllowPaging      = AllowPaging;
			retVal.AllowCustomPaging = AllowCustomPaging;
			retVal.VirtualCount      = VirtualItemCount;

			return retVal;
		}

		///<summary>
		/// UnDocumented method
		/// </summary>
		[MonoTODO]
		protected ArrayList CreateColumnSet(PagedDataSource source, bool useDataSource)
		{
			DataGridColumn[] cols = new DataGridColumn[Columns.Count];
			Columns.CopyTo(cols, 0);
			ArrayList l_columns = new ArrayList();
			ArrayList auto_columns = null;

			foreach(DataGridColumn current in cols)
			{
				l_columns.Add(current);
			}
			if(AutoGenerateColumns)
			{
				l_columns = null;
				if(useDataSource)
				{
					auto_columns = AutoCreateCoumns(source);
					autoGenColsArrayList = auto_columns;
				} else
				{
					auto_columns = autoGenColsArrayList;
				}
				if(auto_columns != null)
				{
					foreach(object current in auto_columns)
					{
						l_columns.Add(current);
					}
				}
			}
			return l_columns;
		}

		/// <summary>
		/// Generates the columns when AutoGenerateColumns is true.
		/// This method is called by CreateColumnSet when dataSource
		/// is to be used and columns need to be generated automatically.
		/// </summary>
		private ArrayList AutoCreateCoumns(PagedDataSource source)
		{
			if(source != null)
			{
				ArrayList retVal = new ArrayList();
				bool      flag   = true;
				PropertyDescriptorCollection props = source.GetItemProperties(new PropertyDescriptor[0]);
				Type      prop_type;
				BoundColumn b_col;
				if(props == null)
				{
					prop_type   = null;
					cval = null;
					PropertyInfo prop_item =  source.DataSource.GetType().GetProperty("Item",
					          BindingFlags.Instance | BindingFlags.Static |
					          BindingFlags.Public, null, null,
					          new Type[] { typeof(int) }, null);
					if(prop_item != null)
					{
						prop_type = prop_item.GetType();
					}
					if(prop_type != null && prop_type == typeof(object))
					{
						object fitem = null;
						IEnumerator en = source.GetEnumerator();
						if(en.MoveNext())
							fitem = en.Current;
						else
							flag = false;
						if(fitem != null)
						{
							prop_type = fitem.GetType();
						}
						StoreEnumerator(en, fitem);
						if(fitem != null && fitem is ICustomTypeDescriptor)
						{
							props = TypeDescriptor.GetProperties(fitem);
						} else if(prop_type != null)
						{
							if(IsBindableType(prop_type))
							{
								b_col = new BoundColumn();
								b_col.TrackViewState();
								b_col.HeaderText = "Item";
								b_col.SortExpression = "Item";
								b_col.DataField  = BoundColumn.thisExpr;
								//b_col.SetOwner(this);
								retVal.Add(b_col);
							} else
							{
								props = TypeDescriptor.GetProperties(prop_type);
							}
						}
					}
				}
				if(props != null && props.Count > 0)
				{
					IEnumerator p_en = props.GetEnumerator();
					try
					{
						foreach(PropertyDescriptor current in p_en)
						{
							if(IsBindableType(current.PropertyType))
							{
								b_col = new BoundColumn();
								b_col.TrackViewState();
								b_col.HeaderText     = current.Name;
								b_col.SortExpression = current.Name;
								b_col.DataField      = current.Name;
								b_col.IsReadOnly     = current.IsReadOnly;
								//b_col.SetOwner(this);
								retVal.Add(b_col);
							}
						}
					} finally
					{
						if(p_en is IDisposable)
							((IDisposable)p_en).Dispose();
					}
				}
				if(retVal.Count > 0)
				{
					return retVal;
				}
				throw new HttpException(HttpRuntime.FormatResourceString("DataGrid_NoAutoGenColumns", ID));
			}
			return null;
		}

		internal void StoreEnumerator(IEnumerator source, object firstItem)
		{
			storedData      = source;
			storedDataFirst = firstItem;
			storedDataValid = true;
		}

		internal void OnColumnsChanged()
		{
		}

		internal void OnPagerChanged()
		{
		}
	}
}
