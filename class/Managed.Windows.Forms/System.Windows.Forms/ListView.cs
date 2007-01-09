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
// Copyright (c) 2004-2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Ravindra Kumar (rkumar@novell.com)
//	Jordi Mas i Hernandez, jordi@ximian.com
//	Mike Kestner (mkestner@novell.com)
//	Daniel Nauck (dna(at)mono-project(dot)de)
//
// TODO:
//   - Feedback for item activation, change in cursor types as mouse moves.
//   - LabelEdit
//   - Drag and drop


// NOT COMPLETE


using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Globalization;
#if NET_2_0
using System.Collections.Generic;
#endif

namespace System.Windows.Forms
{
	[DefaultEvent ("SelectedIndexChanged")]
	[DefaultProperty ("Items")]
	[Designer ("System.Windows.Forms.Design.ListViewDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
#if NET_2_0
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	[Docking (DockingBehavior.Ask)]
#endif
	public class ListView : Control
	{
		private ItemActivation activation = ItemActivation.Standard;
		private ListViewAlignment alignment = ListViewAlignment.Top;
		private bool allow_column_reorder = false;
		private bool auto_arrange = true;
		private bool check_boxes = false;
		private readonly CheckedIndexCollection checked_indices;
		private readonly CheckedListViewItemCollection checked_items;
		private readonly ColumnHeaderCollection columns;
		internal ListViewItem focused_item;
		private bool full_row_select = false;
		private bool grid_lines = false;
		private ColumnHeaderStyle header_style = ColumnHeaderStyle.Clickable;
		private bool hide_selection = true;
		private bool hover_selection = false;
		private IComparer item_sorter;
		private readonly ListViewItemCollection items;
#if NET_2_0
		private readonly ListViewGroupCollection groups;
        	private bool show_groups = true;
#endif
		private bool label_edit = false;
		private bool label_wrap = true;
		private bool multiselect = true;
		private bool scrollable = true;
		private readonly SelectedIndexCollection selected_indices;
		private readonly SelectedListViewItemCollection selected_items;
		private SortOrder sort_order = SortOrder.None;
		private ImageList state_image_list;
		private bool updating = false;
		private View view = View.LargeIcon;
		private int layout_wd;    // We might draw more than our client area
		private int layout_ht;    // therefore we need to have these two.
		//private TextBox editor;   // Used for editing an item text
		HeaderControl header_control;
		internal ItemControl item_control;
		internal ScrollBar h_scroll; // used for scrolling horizontally
		internal ScrollBar v_scroll; // used for scrolling vertically
		internal int h_marker;		// Position markers for scrolling
		internal int v_marker;
		private int keysearch_tickcnt;
		private string keysearch_text;
		static private readonly int keysearch_keydelay = 1000;
		private int[] reordered_column_indices;
#if NET_2_0
		private Size tile_size;
#endif

		// internal variables
		internal ImageList large_image_list;
		internal ImageList small_image_list;
		internal Size text_size = Size.Empty;

		#region Events
		static object AfterLabelEditEvent = new object ();
		static object BeforeLabelEditEvent = new object ();
		static object ColumnClickEvent = new object ();
		static object ItemActivateEvent = new object ();
		static object ItemCheckEvent = new object ();
		static object ItemDragEvent = new object ();
		static object SelectedIndexChangedEvent = new object ();

		public event LabelEditEventHandler AfterLabelEdit {
			add { Events.AddHandler (AfterLabelEditEvent, value); }
			remove { Events.RemoveHandler (AfterLabelEditEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		public event LabelEditEventHandler BeforeLabelEdit {
			add { Events.AddHandler (BeforeLabelEditEvent, value); }
			remove { Events.RemoveHandler (BeforeLabelEditEvent, value); }
		}

		public event ColumnClickEventHandler ColumnClick {
			add { Events.AddHandler (ColumnClickEvent, value); }
			remove { Events.RemoveHandler (ColumnClickEvent, value); }
		}

		public event EventHandler ItemActivate {
			add { Events.AddHandler (ItemActivateEvent, value); }
			remove { Events.RemoveHandler (ItemActivateEvent, value); }
		}

		public event ItemCheckEventHandler ItemCheck {
			add { Events.AddHandler (ItemCheckEvent, value); }
			remove { Events.RemoveHandler (ItemCheckEvent, value); }
		}

		public event ItemDragEventHandler ItemDrag {
			add { Events.AddHandler (ItemDragEvent, value); }
			remove { Events.RemoveHandler (ItemDragEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint {
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}

		public event EventHandler SelectedIndexChanged {
			add { Events.AddHandler (SelectedIndexChangedEvent, value); }
			remove { Events.RemoveHandler (SelectedIndexChangedEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		#endregion // Events

		#region Public Constructors
		public ListView ()
		{
			background_color = ThemeEngine.Current.ColorWindow;
			items = new ListViewItemCollection (this);
#if NET_2_0
			groups = new ListViewGroupCollection (this);
#endif
			checked_indices = new CheckedIndexCollection (this);
			checked_items = new CheckedListViewItemCollection (this);
			columns = new ColumnHeaderCollection (this);
			foreground_color = SystemColors.WindowText;
			selected_indices = new SelectedIndexCollection (this);
			selected_items = new SelectedListViewItemCollection (this);

			border_style = BorderStyle.Fixed3D;

			header_control = new HeaderControl (this);
			header_control.Visible = false;
			Controls.AddImplicit (header_control);

			item_control = new ItemControl (this);
			Controls.AddImplicit (item_control);

			h_scroll = new ImplicitHScrollBar ();
			Controls.AddImplicit (this.h_scroll);

			v_scroll = new ImplicitVScrollBar ();
			Controls.AddImplicit (this.v_scroll);

			h_marker = v_marker = 0;
			keysearch_tickcnt = 0;

			// scroll bars are disabled initially
			h_scroll.Visible = false;
			h_scroll.ValueChanged += new EventHandler(HorizontalScroller);
			v_scroll.Visible = false;
			v_scroll.ValueChanged += new EventHandler(VerticalScroller);

			// event handlers
			base.KeyDown += new KeyEventHandler(ListView_KeyDown);
			SizeChanged += new EventHandler (ListView_SizeChanged);
			GotFocus += new EventHandler (FocusChanged);
			LostFocus += new EventHandler (FocusChanged);
			MouseWheel += new MouseEventHandler(ListView_MouseWheel);

			this.SetStyle (ControlStyles.UserPaint | ControlStyles.StandardClick
#if NET_2_0
				| ControlStyles.UseTextForAccessibility
#endif
				, false);
		}
		#endregion	// Public Constructors

		#region Private Internal Properties
		internal Size CheckBoxSize {
			get {
				if (this.check_boxes) {
					if (this.state_image_list != null)
						return this.state_image_list.ImageSize;
					else
						return ThemeEngine.Current.ListViewCheckBoxSize;
				}
				return Size.Empty;
			}
		}

		#endregion	// Private Internal Properties

		#region	 Protected Properties
		protected override CreateParams CreateParams {
			get { return base.CreateParams; }
		}

		protected override Size DefaultSize {
			get { return ThemeEngine.Current.ListViewDefaultSize; }
		}
		#endregion	// Protected Properties

		#region Public Instance Properties
		[DefaultValue (ItemActivation.Standard)]
		public ItemActivation Activation {
			get { return activation; }
			set { 
				if (value != ItemActivation.Standard && value != ItemActivation.OneClick && 
					value != ItemActivation.TwoClick) {
					throw new InvalidEnumArgumentException (string.Format
						("Enum argument value '{0}' is not valid for Activation", value));
				}
				  
				activation = value;
			}
		}

		[DefaultValue (ListViewAlignment.Top)]
		[Localizable (true)]
		public ListViewAlignment Alignment {
			get { return alignment; }
			set {
				if (value != ListViewAlignment.Default && value != ListViewAlignment.Left && 
					value != ListViewAlignment.SnapToGrid && value != ListViewAlignment.Top) {
					throw new InvalidEnumArgumentException (string.Format 
						("Enum argument value '{0}' is not valid for Alignment", value));
				}
				
				if (this.alignment != value) {
					alignment = value;
					// alignment does not matter in Details/List views
					if (this.view == View.LargeIcon ||
					    this.View == View.SmallIcon)
						this.Redraw (true);
				}
			}
		}

		[DefaultValue (false)]
		public bool AllowColumnReorder {
			get { return allow_column_reorder; }
			set { allow_column_reorder = value; }
		}

		[DefaultValue (true)]
		public bool AutoArrange {
			get { return auto_arrange; }
			set {
				if (auto_arrange != value) {
					auto_arrange = value;
					// autoarrange does not matter in Details/List views
					if (this.view == View.LargeIcon || this.View == View.SmallIcon)
						this.Redraw (true);
				}
			}
		}

		public override Color BackColor {
			get {
				if (background_color.IsEmpty)
					return ThemeEngine.Current.ColorWindow;
				else
					return background_color;
			}
			set { background_color = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}

		[DefaultValue (BorderStyle.Fixed3D)]
		[DispId (-504)]
		public BorderStyle BorderStyle {
			get { return InternalBorderStyle; }
			set { InternalBorderStyle = value; }
		}

		[DefaultValue (false)]
		public bool CheckBoxes {
			get { return check_boxes; }
			set {
				if (check_boxes != value) {
#if NET_2_0
					if (value && View == View.Tile)
						throw new NotSupportedException ("CheckBoxes are not"
							+ " supported in Tile view. Choose a different"
							+ " view or set CheckBoxes to false.");
#endif

					check_boxes = value;
					this.Redraw (true);
				}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public CheckedIndexCollection CheckedIndices {
			get { return checked_indices; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public CheckedListViewItemCollection CheckedItems {
			get { return checked_items; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Localizable (true)]
		[MergableProperty (false)]
		public ColumnHeaderCollection Columns {
			get { return columns; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ListViewItem FocusedItem {
			get {
				return focused_item;
			}
		}

		public override Color ForeColor {
			get {
				if (foreground_color.IsEmpty)
					return ThemeEngine.Current.ColorWindowText;
				else
					return foreground_color;
			}
			set { foreground_color = value; }
		}

		[DefaultValue (false)]
		public bool FullRowSelect {
			get { return full_row_select; }
			set { full_row_select = value; }
		}

		[DefaultValue (false)]
		public bool GridLines {
			get { return grid_lines; }
			set {
				if (grid_lines != value) {
					grid_lines = value;
					this.Redraw (false);
				}
			}
		}

		[DefaultValue (ColumnHeaderStyle.Clickable)]
		public ColumnHeaderStyle HeaderStyle {
			get { return header_style; }
			set {
				if (header_style == value)
					return;

				switch (value) {
				case ColumnHeaderStyle.Clickable:
				case ColumnHeaderStyle.Nonclickable:
				case ColumnHeaderStyle.None:
					break;
				default:
					throw new InvalidEnumArgumentException (string.Format 
						("Enum argument value '{0}' is not valid for ColumnHeaderStyle", value));
				}
				
				header_style = value;
				if (view == View.Details)
					Redraw (true);
			}
		}

		[DefaultValue (true)]
		public bool HideSelection {
			get { return hide_selection; }
			set {
				if (hide_selection != value) {
					hide_selection = value;
					this.Redraw (false);
				}
			}
		}

		[DefaultValue (false)]
		public bool HoverSelection {
			get { return hover_selection; }
			set { hover_selection = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Localizable (true)]
		[MergableProperty (false)]		
		public ListViewItemCollection Items {
			get { return items; }
		}

		[DefaultValue (false)]
		public bool LabelEdit {
			get { return label_edit; }
			set { label_edit = value; }
		}

		[DefaultValue (true)]
		[Localizable (true)]
		public bool LabelWrap {
			get { return label_wrap; }
			set {
				if (label_wrap != value) {
					label_wrap = value;
					this.Redraw (true);
				}
			}
		}

		[DefaultValue (null)]
		public ImageList LargeImageList {
			get { return large_image_list; }
			set {
				large_image_list = value;
				this.Redraw (true);
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public IComparer ListViewItemSorter {
			get {
				if (View != View.SmallIcon && View != View.LargeIcon && item_sorter is ItemComparer)
					return null;
				return item_sorter;
			}
			set {
				if (item_sorter != value) {
					item_sorter = value;
					Sort ();
				}
			}
		}

		[DefaultValue (true)]
		public bool MultiSelect {
			get { return multiselect; }
			set { multiselect = value; }
		}

		[DefaultValue (true)]
		public bool Scrollable {
			get { return scrollable; }
			set {
				if (scrollable != value) {
					scrollable = value;
					this.Redraw (true);
				}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public SelectedIndexCollection SelectedIndices {
			get { return selected_indices; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public SelectedListViewItemCollection SelectedItems {
			get { return selected_items; }
		}

#if NET_2_0
        	[DefaultValue(true)]
		public bool ShowGroups {
			get { return show_groups; }
			set 
			{
                		if (show_groups != value)
                		{
					show_groups = value;
					Redraw(true);
				}
			}
		}

		[LocalizableAttribute(true)]
		public ListViewGroupCollection Groups {
			get { return groups;	}
		}
#endif

		[DefaultValue (null)]
		public ImageList SmallImageList {
			get { return small_image_list; }
			set {
				small_image_list = value;
				this.Redraw (true);
			}
		}

		[DefaultValue (SortOrder.None)]
		public SortOrder Sorting {
			get { return sort_order; }
			set { 
				if (!Enum.IsDefined (typeof (SortOrder), value)) {
					throw new InvalidEnumArgumentException ("value", (int) value,
						typeof (SortOrder));
				}
				
				if (sort_order == value)
					return;

				sort_order = value;

				if (value == SortOrder.None) {
					if (item_sorter != null) {
						// ListViewItemSorter should never be reset for SmallIcon
						// and LargeIcon view
						if (View != View.SmallIcon && View != View.LargeIcon)
#if NET_2_0
							item_sorter = null;
#else
							// in .NET 1.1, only internal IComparer would be
							// set to null
							if (item_sorter is ItemComparer)
								item_sorter = null;
#endif
					}
					this.Redraw (false);
				} else {
					if (item_sorter == null)
						item_sorter = new ItemComparer (value);
					if (item_sorter is ItemComparer) {
#if NET_2_0
						item_sorter = new ItemComparer (value);
#else
						// in .NET 1.1, the sort order is not updated for
						// SmallIcon and LargeIcon views if no custom IComparer
						// is set
						if (View != View.SmallIcon && View != View.LargeIcon)
							item_sorter = new ItemComparer (value);
#endif
					}
					Sort ();
				}
			}
		}

		[DefaultValue (null)]
		public ImageList StateImageList {
			get { return state_image_list; }
			set {
				state_image_list = value;
				this.Redraw (true);
			}
		}

		[Bindable (false)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string Text {
			get { return base.Text; } 
			set {
				if (value == base.Text)
					return;

				base.Text = value;
				this.Redraw (true);
			}
		}

#if NET_2_0
		public Size TileSize {
			get {
				return tile_size;
			}
			set {
				if (value.Width <= 0 || value.Height <= 0)
					throw new ArgumentOutOfRangeException ("value");

				tile_size = value;
				Redraw (true);
			}
		}
#endif

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ListViewItem TopItem {
			get {
				// there is no item
				if (this.items.Count == 0)
					return null;
				// if contents are not scrolled
				// it is the first item
				else if (h_marker == 0 && v_marker == 0)
					return this.items [0];
				// do a hit test for the scrolled position
				else {
					foreach (ListViewItem item in this.items) {
						if (item.Bounds.X >= 0 && item.Bounds.Y >= 0)
							return item;
					}
					return null;
				}
			}
		}

#if NET_2_0
		[MonoTODO("Implement")]
		public bool UseCompatibleStateImageBehavior {
			get {
				return false;
			}

			set {
			}
		}
#endif

		[DefaultValue (View.LargeIcon)]
		public View View {
			get { return view; }
			set { 
				if (!Enum.IsDefined (typeof (View), value))
					throw new InvalidEnumArgumentException ("value", (int) value,
						typeof (View));

				if (view != value) {
#if NET_2_0
					if (CheckBoxes && value == View.Tile)
						throw new NotSupportedException ("CheckBoxes are not"
							+ " supported in Tile view. Choose a different"
							+ " view or set CheckBoxes to false.");
#endif

					h_scroll.Value = v_scroll.Value = 0;
					view = value; 
					Redraw (true);
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Internal Methods Properties
		
		internal int FirstVisibleIndex {
			get {
				// there is no item
				if (this.items.Count == 0)
					return 0;
									
				if (h_marker == 0 && v_marker == 0)
					return 0;					
				
				foreach (ListViewItem item in this.items) {
					if (item.Bounds.Right >= 0 && item.Bounds.Bottom >= 0)
						return item.Index;
				}
				return 0;

			}
		}

		
		internal int LastVisibleIndex {			
			get {							
				for (int i = FirstVisibleIndex; i < Items.Count; i++) {
					if (View == View.List || Alignment == ListViewAlignment.Left) {
						if (Items[i].Bounds.X > item_control.ClientRectangle.Right)
							return i - 1;					
					} else {
						if (Items[i].Bounds.Y > item_control.ClientRectangle.Bottom)
							return i - 1;					
					}
				}
				
				return Items.Count - 1;
			}
		}
		
		internal void OnSelectedIndexChanged ()
		{
			if (IsHandleCreated)
				OnSelectedIndexChanged (EventArgs.Empty);
		}

		internal int TotalWidth {
			get { return Math.Max (this.Width, this.layout_wd); }
		}

		internal int TotalHeight {
			get { return Math.Max (this.Height, this.layout_ht); }
		}

		internal void Redraw (bool recalculate)
		{
			// Avoid calculations when control is being updated
			if (this.updating)
				return;

			if (recalculate)
				CalculateListView (this.alignment);

			Refresh ();
		}

		const int text_padding = 5;

		internal Size GetChildColumnSize (int index)
		{
			Size ret_size = Size.Empty;
			ColumnHeader col = this.columns [index];

			if (col.Width == -2) { // autosize = max(items, columnheader)
				Size size = Size.Ceiling (this.DeviceContext.MeasureString
							  (col.Text, this.Font));
				size.Width += text_padding;
				ret_size = BiggestItem (index);
				if (size.Width > ret_size.Width)
					ret_size = size;
			}
			else { // -1 and all the values < -2 are put under one category
				ret_size = BiggestItem (index);
				// fall back to empty columns' width if no subitem is available for a column
				if (ret_size.IsEmpty) {
					ret_size.Width = ThemeEngine.Current.ListViewEmptyColumnWidth;
					if (col.Text.Length > 0)
						ret_size.Height = Size.Ceiling (this.DeviceContext.MeasureString
										(col.Text, this.Font)).Height;
					else
						ret_size.Height = this.Font.Height;
				}
			}

			ret_size.Height += text_padding;

			// adjust the size for icon and checkbox for 0th column
			if (index == 0) {
				ret_size.Width += (this.CheckBoxSize.Width + 4);
				if (this.small_image_list != null)
					ret_size.Width += this.small_image_list.ImageSize.Width;
			}
			return ret_size;
		}

		// Returns the size of biggest item text in a column.
		private Size BiggestItem (int col)
		{
			Size temp = Size.Empty;
			Size ret_size = Size.Empty;

			// 0th column holds the item text, we check the size of
			// the various subitems falling in that column and get
			// the biggest one's size.
			foreach (ListViewItem item in items) {
				if (col >= item.SubItems.Count)
					continue;

				temp = Size.Ceiling (this.DeviceContext.MeasureString
						     (item.SubItems [col].Text, this.Font));
				if (temp.Width > ret_size.Width)
					ret_size = temp;
			}

			// adjustment for space
			if (!ret_size.IsEmpty)
				ret_size.Width += 4;

			return ret_size;
		}

		const int max_wrap_padding = 38;

		// Sets the size of the biggest item text as per the view
		private void CalcTextSize ()
		{			
			// clear the old value
			text_size = Size.Empty;

			if (items.Count == 0)
				return;

			text_size = BiggestItem (0);

			if (view == View.LargeIcon && this.label_wrap) {
				Size temp = Size.Empty;
				if (this.check_boxes)
					temp.Width += 2 * this.CheckBoxSize.Width;
				int icon_w = LargeImageList == null ? 12 : LargeImageList.ImageSize.Width;
				temp.Width += icon_w + max_wrap_padding;
				// wrapping is done for two lines only
				if (text_size.Width > temp.Width) {
					text_size.Width = temp.Width;
					text_size.Height *= 2;
				}
			}
			else if (view == View.List) {
				// in list view max text shown in determined by the
				// control width, even if scolling is enabled.
				int max_wd = this.Width - (this.CheckBoxSize.Width - 2);
				if (this.small_image_list != null)
					max_wd -= this.small_image_list.ImageSize.Width;

				if (text_size.Width > max_wd)
					text_size.Width = max_wd;
			}

			// we do the default settings, if we have got 0's
			if (text_size.Height <= 0)
				text_size.Height = this.Font.Height;
			if (text_size.Width <= 0)
				text_size.Width = this.Width;

			// little adjustment
			text_size.Width += 4;
			text_size.Height += 2;
		}

		private void Scroll (ScrollBar scrollbar, int delta)
		{
			if (delta == 0 || !scrollbar.Visible)
				return;

			int max;
			if (scrollbar == h_scroll)
				max = h_scroll.Maximum - item_control.Width;
			else
				max = v_scroll.Maximum - item_control.Height;

			int val = scrollbar.Value + delta;
			if (val > max)
				val = max;
			else if (val < scrollbar.Minimum)
				val = scrollbar.Minimum;
			scrollbar.Value = val;
		}

		private void CalculateScrollBars ()
		{
			Rectangle client_area = ClientRectangle;
			
			if (!this.scrollable || this.items.Count <= 0) {
				h_scroll.Visible = false;
				v_scroll.Visible = false;
				item_control.Location = new Point (0, header_control.Height);
				item_control.Height = ClientRectangle.Width - header_control.Height;
				item_control.Width = ClientRectangle.Width;
				header_control.Width = ClientRectangle.Width;
				return;
			}

			// Don't calculate if the view is not displayable
			if (client_area.Height < 0 || client_area.Width < 0)
				return;

			// making a scroll bar visible might make
			// other scroll bar visible			
			if (layout_wd > client_area.Right) {
				h_scroll.Visible = true;
				if ((layout_ht + h_scroll.Height) > client_area.Bottom)
					v_scroll.Visible = true;					
				else
					v_scroll.Visible = false;
			} else if (layout_ht > client_area.Bottom) {				
				v_scroll.Visible = true;
				if ((layout_wd + v_scroll.Width) > client_area.Right)
					h_scroll.Visible = true;
				else
					h_scroll.Visible = false;
			} else {
				h_scroll.Visible = false;
				v_scroll.Visible = false;
			}			

			item_control.Height = ClientRectangle.Height - header_control.Height;

			if (h_scroll.is_visible) {
				h_scroll.Location = new Point (client_area.X, client_area.Bottom - h_scroll.Height);
				h_scroll.Minimum = 0;

				// if v_scroll is visible, adjust the maximum of the
				// h_scroll to account for the width of v_scroll
				if (v_scroll.Visible) {
					h_scroll.Maximum = layout_wd + v_scroll.Width;
					h_scroll.Width = client_area.Width - v_scroll.Width;
				}
				else {
					h_scroll.Maximum = layout_wd;
					h_scroll.Width = client_area.Width;
				}
   
				h_scroll.LargeChange = client_area.Width;
				h_scroll.SmallChange = Font.Height;
				item_control.Height -= h_scroll.Height;
			}

			if (header_control.is_visible)
				header_control.Width = ClientRectangle.Width;
			item_control.Width = ClientRectangle.Width;

			if (v_scroll.is_visible) {
				v_scroll.Location = new Point (client_area.Right - v_scroll.Width, client_area.Y);
				v_scroll.Minimum = 0;

				// if h_scroll is visible, adjust the maximum of the
				// v_scroll to account for the height of h_scroll
				if (h_scroll.Visible) {
					v_scroll.Maximum = layout_ht + h_scroll.Height;
					v_scroll.Height = client_area.Height; // - h_scroll.Height already done 
				} else {
					v_scroll.Maximum = layout_ht;
					v_scroll.Height = client_area.Height;
				}

				v_scroll.LargeChange = client_area.Height;
				v_scroll.SmallChange = Font.Height;
				if (header_control.Visible)
					header_control.Width -= v_scroll.Width;
				item_control.Width -= v_scroll.Width;
			}
		}

#if NET_2_0
		internal int GetReorderedColumnIndex (ColumnHeader column)
		{
			if (reordered_column_indices == null)
				return column.Index;

			for (int i = 0; i < Columns.Count; i++)
				if (reordered_column_indices [i] == column.Index)
					return i;

			return -1;
		}
#endif
		
		internal ColumnHeader GetReorderedColumn (int index)
		{
			if (reordered_column_indices == null)
				return Columns [index];
			else
				return Columns [reordered_column_indices [index]];
		}

		internal void ReorderColumn (ColumnHeader col, int index)
		{
#if NET_2_0
			ColumnReorderedEventHandler eh = (ColumnReorderedEventHandler) (Events [ColumnReorderedEvent]);
			if (eh != null){
				ColumnReorderedEventArgs args = new ColumnReorderedEventArgs (col.Index, index, col);

				eh (this, args);
				if (args.Cancel){
					header_control.Invalidate ();
					item_control.Invalidate ();
					return;
				}
			}
#endif
			if (reordered_column_indices == null) {
				reordered_column_indices = new int [Columns.Count];
				for (int i = 0; i < Columns.Count; i++)
					reordered_column_indices [i] = i;
			}

			if (reordered_column_indices [index] == col.Index)
				return;

			int[] curr = reordered_column_indices;
			int[] result = new int [Columns.Count];
			int curr_idx = 0;
			for (int i = 0; i < Columns.Count; i++) {
				if (curr_idx < Columns.Count && curr [curr_idx] == col.Index)
					curr_idx++;

				if (i == index)
					result [i] = col.Index;
				else
					result [i] = curr [curr_idx++];
			}

			reordered_column_indices = result;
			LayoutDetails ();
			header_control.Invalidate ();
			item_control.Invalidate ();
		}

		Size LargeIconItemSize {
			get {
				int image_w = LargeImageList == null ? 12 : LargeImageList.ImageSize.Width;
				int image_h = LargeImageList == null ? 2 : LargeImageList.ImageSize.Height;
				int w = CheckBoxSize.Width + 2 + Math.Max (text_size.Width, image_w);
				int h = text_size.Height + 2 + Math.Max (CheckBoxSize.Height, image_h);
				return new Size (w, h);
			}
		}

		Size SmallIconItemSize {
			get {
				int image_w = SmallImageList == null ? 0 : SmallImageList.ImageSize.Width;
				int image_h = SmallImageList == null ? 0 : SmallImageList.ImageSize.Height;
				int w = text_size.Width + 2 + CheckBoxSize.Width + image_w;
				int h = Math.Max (text_size.Height, Math.Max (CheckBoxSize.Height, image_h));
				return new Size (w, h);
			}
		}

#if NET_2_0
		Size TileItemSize {
			get {
				// Calculate tile size if needed
				// It appears that using Font.Size instead of a SizeF value can give us
				// a slightly better approach to the proportions defined in .Net
				if (tile_size == Size.Empty) {
					int image_w = LargeImageList == null ? 0 : LargeImageList.ImageSize.Width;
					int image_h = LargeImageList == null ? 0 : LargeImageList.ImageSize.Height;
					int w = (int)Font.Size * ThemeEngine.Current.ListViewTileWidthFactor + image_w + 4;
					int h = Math.Max ((int)Font.Size * ThemeEngine.Current.ListViewTileHeightFactor, image_h);

					tile_size = new Size (w, h);
				}

				return tile_size;
			}
		}
#endif

		int rows;
		int cols;
		ListViewItem[,] item_matrix;

		void LayoutIcons (Size item_size, bool left_aligned, int x_spacing, int y_spacing)
		{
			header_control.Visible = false;
			header_control.Size = Size.Empty;
			item_control.Visible = true;
			item_control.Location = Point.Empty;

			if (items.Count == 0)
				return;

			Size sz = item_size;
			Rectangle area = ClientRectangle;

			if (left_aligned) {
				rows = (int) Math.Floor ((double)(area.Height - h_scroll.Height + y_spacing) / (double)(sz.Height + y_spacing));
				if (rows <= 0)
					rows = 1;
				cols = (int) Math.Ceiling ((double)items.Count / (double)rows);
			} else {
				cols = (int) Math.Floor ((double)(area.Width - v_scroll.Width + x_spacing) / (double)(sz.Width + x_spacing));
				if (cols <= 0)
					cols = 1;
				rows = (int) Math.Ceiling ((double)items.Count / (double)cols);
			}

			layout_ht = rows * (sz.Height + y_spacing) - y_spacing;
			layout_wd = cols * (sz.Width + x_spacing) - x_spacing;
			item_matrix = new ListViewItem [rows, cols];
			int row = 0;
			int col = 0;
			foreach (ListViewItem item in items) {
				int x = col * (sz.Width + x_spacing);
				int y = row * (sz.Height + y_spacing);
				item.Location = new Point (x, y);
				item.Layout ();
				item.row = row;
				item.col = col;
				item_matrix [row, col] = item;
				if (left_aligned) {
					if (++row == rows) {
						row = 0;
						col++;
					}
				} else {
					if (++col == cols) {
						col = 0;
						row++;
					}
				}
			}

			item_control.Size = new Size (layout_wd, layout_ht);
		}

		void LayoutHeader ()
		{
			int x = 0;
			for (int i = 0; i < Columns.Count; i++) {
			       	ColumnHeader col = GetReorderedColumn (i);
				col.X = x;
				col.Y = 0;
				col.CalcColumnHeader ();
				x += col.Wd;
			}

			if (x < ClientRectangle.Width)
				x = ClientRectangle.Width;

			if (header_style == ColumnHeaderStyle.None) {
				header_control.Visible = false;
				header_control.Size = Size.Empty;
			} else {
				header_control.Width = x;
				header_control.Height = columns [0].Ht;
				header_control.Visible = true;
			}
		}

		void LayoutDetails ()
		{
			if (columns.Count == 0) {
				header_control.Visible = false;
				item_control.Visible = false;
				return;
			}

			LayoutHeader ();

			item_control.Visible = true;
			item_control.Location = new Point (0, header_control.Height);

			int y = 0; 
			if (items.Count > 0) {
				foreach (ListViewItem item in items) {
					item.Layout ();
					item.Location = new Point (0, y);
					y += item.Bounds.Height + 2;
				}

				// some space for bottom gridline
				if (grid_lines)
					y += 2;
			}

			layout_wd = Math.Max (header_control.Width, item_control.Width);
			layout_ht = y + header_control.Height;
		}

		private void CalculateListView (ListViewAlignment align)
		{
			CalcTextSize ();

			switch (view) {
			case View.Details:
				LayoutDetails ();
				break;

			case View.SmallIcon:
				LayoutIcons (SmallIconItemSize, alignment == ListViewAlignment.Left, 4, 2);
				break;

			case View.LargeIcon:
				LayoutIcons (LargeIconItemSize, alignment == ListViewAlignment.Left,
					     ThemeEngine.Current.ListViewHorizontalSpacing,
					     ThemeEngine.Current.ListViewVerticalSpacing);
				break;

			case View.List:
				LayoutIcons (SmallIconItemSize, true, 4, 2);
				break;
#if NET_2_0
			case View.Tile:
				LayoutIcons (TileItemSize, alignment == ListViewAlignment.Left, 
						ThemeEngine.Current.ListViewHorizontalSpacing,
						ThemeEngine.Current.ListViewVerticalSpacing);
				break;
#endif
			}

                        CalculateScrollBars ();
		}

		private bool KeySearchString (KeyEventArgs ke)
		{
			int current_tickcnt = Environment.TickCount;
			if (keysearch_tickcnt > 0 && current_tickcnt - keysearch_tickcnt > keysearch_keydelay) {
				keysearch_text = string.Empty;
			}
			
			keysearch_text += (char) ke.KeyData;
			keysearch_tickcnt = current_tickcnt;

			int start = FocusedItem == null ? 0 : FocusedItem.Index;
			int i = start;
			while (true) {
				if (CultureInfo.CurrentCulture.CompareInfo.IsPrefix (Items[i].Text, keysearch_text,
					CompareOptions.IgnoreCase)) {
					SetFocusedItem (Items [i]);
					items [i].Selected = true;
					EnsureVisible (i);
					break;
				}
				i = (i + 1  < Items.Count) ? i+1 : 0;

				if (i == start)
					break;
			}
			return true;
		}

		int GetAdjustedIndex (Keys key)
		{
			int result = -1;

			if (View == View.Details) {
				switch (key) {
				case Keys.Up:
					result = FocusedItem.Index - 1;
					break;
				case Keys.Down:
					result = FocusedItem.Index + 1;
					if (result == items.Count)
						result = -1;
					break;
				case Keys.PageDown:
					int last_index = LastVisibleIndex;
					if (Items [last_index].Bounds.Bottom > item_control.ClientRectangle.Bottom)
						last_index--;
					if (FocusedItem.Index == last_index) {
						if (FocusedItem.Index < Items.Count - 1) {
							int page_size = item_control.Height / items [0].Bounds.Height - 1;
							result = FocusedItem.Index + page_size - 1;
							if (result >= Items.Count)
								result = Items.Count - 1;
						}
					} else
						result = last_index;
					break;
				case Keys.PageUp:
					int first_index = FirstVisibleIndex;
					if (Items [first_index].Bounds.Y < 0)
						first_index++;
					if (FocusedItem.Index == first_index) {
						if (first_index > 0) {
							int page_size = item_control.Height / items [0].Bounds.Height - 1;
							result = first_index - page_size + 1;
							if (result < 0)
								result = 0;
						}
					} else
						result = first_index;
					break;
				}
				return result;
			}

			int row = FocusedItem.row;
			int col = FocusedItem.col;

			switch (key) {
			case Keys.Left:
				if (col == 0)
					return -1;
				return item_matrix [row, col - 1].Index;

			case Keys.Right:
				if (col == (cols - 1))
					return -1;
				while (item_matrix [row, col + 1] == null) {
					row--;	
					if (row < 0)
						return -1;
				}
				return item_matrix [row, col + 1].Index;

			case Keys.Up:
				if (row == 0)
					return -1;
				return item_matrix [row - 1, col].Index;

			case Keys.Down:
				if (row == (rows - 1) || row == Items.Count - 1)
					return -1;
				while (item_matrix [row + 1, col] == null) {
					col--;	
					if (col < 0)
						return -1;
				}
				return item_matrix [row + 1, col].Index;

			default:
				return -1;
			}
		}

		ListViewItem selection_start;

		private bool SelectItems (ArrayList sel_items)
		{
			bool changed = false;
			ArrayList curr_items = SelectedItems.List;
			foreach (ListViewItem item in curr_items)
				if (!sel_items.Contains (item)) {
					item.Selected = false;
					changed = true;
				}
			foreach (ListViewItem item in sel_items)
				if (!item.Selected) {
					item.Selected = true;
					changed = true;
				}
			return changed;
		}

		private void UpdateMultiSelection (int index)
		{
			bool shift_pressed = (XplatUI.State.ModifierKeys & Keys.Shift) != 0;
			bool ctrl_pressed = (XplatUI.State.ModifierKeys & Keys.Control) != 0;
			ListViewItem item = items [index];

			if (shift_pressed && selection_start != null) {
				ArrayList list = new ArrayList ();
				int start = Math.Min (selection_start.Index, index);
				int end = Math.Max (selection_start.Index, index);
				if (View == View.Details) {
					for (int i = start; i <= end; i++)
						list.Add (items [i]);
				} else {
					int left = Math.Min (items [start].col, items [end].col);
					int right = Math.Max (items [start].col, items [end].col);
					int top = Math.Min (items [start].row, items [end].row);
					int bottom = Math.Max (items [start].row, items [end].row);
					foreach (ListViewItem curr in items)
						if (curr.row >= top && curr.row <= bottom && 
						    curr.col >= left && curr.col <= right)
							list.Add (curr);
				}
				if (SelectItems (list))
					OnSelectedIndexChanged (EventArgs.Empty);
			} else  if (ctrl_pressed) {
				item.Selected = !item.Selected;
				selection_start = item;
				OnSelectedIndexChanged (EventArgs.Empty);
			} else {
				SelectedItems.Clear ();
				item.Selected = true;
				selection_start = item;
				OnSelectedIndexChanged (EventArgs.Empty);
			}
		}

		internal override bool InternalPreProcessMessage (ref Message msg)
		{
			if (msg.Msg == (int)Msg.WM_KEYDOWN) {
				Keys key_data = (Keys)msg.WParam.ToInt32();
				if (HandleNavKeys (key_data))
					return true;
			} 
			return base.InternalPreProcessMessage (ref msg);
		}

		bool HandleNavKeys (Keys key_data)
		{
			if (Items.Count == 0 || !item_control.Visible)
				return false;

			if (FocusedItem == null)
				SetFocusedItem (Items [0]);

			switch (key_data) {
			case Keys.End:
				SelectIndex (Items.Count - 1);
				break;

			case Keys.Home:			
				SelectIndex (0);
				break;

			case Keys.Left:
			case Keys.Right:
			case Keys.Up:				
			case Keys.Down:
			case Keys.PageUp:
			case Keys.PageDown:
				SelectIndex (GetAdjustedIndex (key_data));
				break;

			default:
				return false;
			}

			return true;
		}

		void SelectIndex (int index)
		{
			if (index == -1)
				return;

			if (MultiSelect)
				UpdateMultiSelection (index);
			else if (!items [index].Selected) {
				items [index].Selected = true;
				OnSelectedIndexChanged (EventArgs.Empty);
			}

			SetFocusedItem (items [index]);				
			EnsureVisible (index);
		}

		private void ListView_KeyDown (object sender, KeyEventArgs ke)
		{			
			if (ke.Handled || Items.Count == 0 || !item_control.Visible)
				return;

			ke.Handled = KeySearchString (ke);
		}

		private MouseEventArgs TranslateMouseEventArgs (MouseEventArgs args)
		{
			Point loc = PointToClient (Control.MousePosition);
			return new MouseEventArgs (args.Button, args.Clicks, loc.X, loc.Y, args.Delta);
		}

		internal class ItemControl : Control {

			ListView owner;
			ListViewItem clicked_item;
			ListViewItem last_clicked_item;
			bool hover_processed = false;
			bool checking = false;
			
			ListViewLabelEditTextBox edit_text_box;
			internal ListViewItem edit_item;
			LabelEditEventArgs edit_args;

			public ItemControl (ListView owner)
			{
				this.owner = owner;
				DoubleClick += new EventHandler(ItemsDoubleClick);
				MouseDown += new MouseEventHandler(ItemsMouseDown);
				MouseMove += new MouseEventHandler(ItemsMouseMove);
				MouseHover += new EventHandler(ItemsMouseHover);
				MouseUp += new MouseEventHandler(ItemsMouseUp);
			}

			void ItemsDoubleClick (object sender, EventArgs e)
			{
				if (owner.activation == ItemActivation.Standard)
					owner.OnItemActivate (EventArgs.Empty);
			}

			enum BoxSelect {
				None,
				Normal,
				Shift,
				Control
			}

			BoxSelect box_select_mode = BoxSelect.None;
			ArrayList prev_selection;
			Point box_select_start;

			Rectangle box_select_rect;
			internal Rectangle BoxSelectRectangle {
				get { return box_select_rect; }
				set {
					if (box_select_rect == value)
						return;

					InvalidateBoxSelectRect ();
					box_select_rect = value;
					InvalidateBoxSelectRect ();
				}
			}

			void InvalidateBoxSelectRect ()
			{
				if (BoxSelectRectangle.Size.IsEmpty)
					return;

				Rectangle edge = BoxSelectRectangle;
				edge.X -= 1;
				edge.Y -= 1;
				edge.Width += 2;
				edge.Height = 2;
				Invalidate (edge);
				edge.Y = BoxSelectRectangle.Bottom - 1;
				Invalidate (edge);
				edge.Y = BoxSelectRectangle.Y - 1;
				edge.Width = 2;
				edge.Height = BoxSelectRectangle.Height + 2;
				Invalidate (edge);
				edge.X = BoxSelectRectangle.Right - 1;
				Invalidate (edge);
			}

			private Rectangle CalculateBoxSelectRectangle (Point pt)
			{
				int left = Math.Min (box_select_start.X, pt.X);
				int right = Math.Max (box_select_start.X, pt.X);
				int top = Math.Min (box_select_start.Y, pt.Y);
				int bottom = Math.Max (box_select_start.Y, pt.Y);
				return Rectangle.FromLTRB (left, top, right, bottom);
			}

			ArrayList BoxSelectedItems {
				get {
					ArrayList result = new ArrayList ();
					foreach (ListViewItem item in owner.Items) {
						Rectangle r = item.Bounds;
						r.X += r.Width / 4;
						r.Y += r.Height / 4;
						r.Width /= 2;
						r.Height /= 2;
						if (BoxSelectRectangle.IntersectsWith (r))
							result.Add (item);
					}
					return result;
				}
			}

			private bool PerformBoxSelection (Point pt)
			{
				if (box_select_mode == BoxSelect.None)
					return false;

				BoxSelectRectangle = CalculateBoxSelectRectangle (pt);
				
				ArrayList box_items = BoxSelectedItems;

				ArrayList items;

				switch (box_select_mode) {

				case BoxSelect.Normal:
					items = box_items;
					break;

				case BoxSelect.Control:
					items = new ArrayList ();
					foreach (ListViewItem item in prev_selection)
						if (!box_items.Contains (item))
							items.Add (item);
					foreach (ListViewItem item in box_items)
						if (!prev_selection.Contains (item))
							items.Add (item);
					break;

				case BoxSelect.Shift:
					items = box_items;
					foreach (ListViewItem item in box_items)
						prev_selection.Remove (item);
					foreach (ListViewItem item in prev_selection)
						items.Add (item);
					break;

				default:
					throw new Exception ("Unexpected Selection mode: " + box_select_mode);
				}

				SuspendLayout ();
				owner.SelectItems (items);
				ResumeLayout ();

				return true;
			}

			private void ToggleCheckState (ListViewItem item)
			{
				CheckState curr_state = item.Checked ?  CheckState.Checked : CheckState.Unchecked;
				item.Checked = !item.Checked;
				CheckState new_state = item.Checked ?  CheckState.Checked : CheckState.Unchecked;

				ItemCheckEventArgs ice = new ItemCheckEventArgs (item.Index, curr_state, new_state);
				owner.OnItemCheck (ice);
			}

			private void ItemsMouseDown (object sender, MouseEventArgs me)
			{
				if (owner.items.Count == 0) {
					owner.OnMouseDown (owner.TranslateMouseEventArgs (me));
					return;
				}

				Point pt = new Point (me.X, me.Y);
				foreach (ListViewItem item in owner.items) {
					if (me.Clicks == 1 && item.CheckRectReal.Contains (pt)) {
						checking = true;
						ToggleCheckState (item);
						owner.OnMouseDown (owner.TranslateMouseEventArgs (me));
						return;
					}

					if (owner.View == View.Details && !owner.FullRowSelect) {
						if (item.GetBounds (ItemBoundsPortion.Label).Contains (pt)) {
							clicked_item = item;
							break;
						}
					} else {
						if (item.Bounds.Contains (pt)) {
							clicked_item = item;
							break;
						}
					}
				}


				if (clicked_item != null) {
					owner.SetFocusedItem (clicked_item);
					bool changed = !clicked_item.Selected;
					if (owner.MultiSelect)
						owner.UpdateMultiSelection (clicked_item.Index);
					else
						clicked_item.Selected = true;
				
					if (changed)
						owner.OnSelectedIndexChanged (EventArgs.Empty);

					// Raise double click if the item was clicked. On MS the
					// double click is only raised if you double click an item
					if (me.Clicks > 1) {
						owner.OnDoubleClick (EventArgs.Empty);
						if (owner.CheckBoxes)
							ToggleCheckState (clicked_item);
					} else if (me.Clicks == 1) {
						owner.OnClick (EventArgs.Empty);
						if (owner.LabelEdit && !changed)
							BeginEdit (clicked_item); // this is probably not the correct place to execute BeginEdit
					}
				} else {
					if (owner.MultiSelect) {
						Keys mods = XplatUI.State.ModifierKeys;
						if ((mods & Keys.Shift) != 0)
							box_select_mode = BoxSelect.Shift;
						else if ((mods & Keys.Control) != 0)
							box_select_mode = BoxSelect.Control;
						else
							box_select_mode = BoxSelect.Normal;
						box_select_start = pt; 
						prev_selection = owner.SelectedItems.List;
					} else if (owner.SelectedItems.Count > 0) {
						owner.SelectedItems.Clear ();
						owner.OnSelectedIndexChanged (EventArgs.Empty);
					}
				}

				owner.OnMouseDown (owner.TranslateMouseEventArgs (me));
			}

			private void ItemsMouseMove (object sender, MouseEventArgs me)
			{
				bool done = PerformBoxSelection (new Point (me.X, me.Y));

				if (!done && owner.HoverSelection && hover_processed) {

					Point pt = PointToClient (Control.MousePosition);
					ListViewItem item = owner.GetItemAt (pt.X, pt.Y);
					if (item != null && !item.Selected) {
						hover_processed = false;
						XplatUI.ResetMouseHover (Handle);
					}
				}

				owner.OnMouseMove (owner.TranslateMouseEventArgs (me));
			}


			private void ItemsMouseHover (object sender, EventArgs e)
			{
				owner.OnMouseHover(e);

				if (Capture || !owner.HoverSelection)
					return;

				hover_processed = true;
				Point pt = PointToClient (Control.MousePosition);
				ListViewItem item = owner.GetItemAt (pt.X, pt.Y);

				if (item == null)
					return;

				item.Selected = true;
				owner.OnSelectedIndexChanged (new EventArgs ());
			}

			private void ItemsMouseUp (object sender, MouseEventArgs me)
			{
				Capture = false;
				if (owner.Items.Count == 0) {
					owner.OnMouseUp (owner.TranslateMouseEventArgs (me));
					return;
				}

				Point pt = new Point (me.X, me.Y);

				Rectangle rect = Rectangle.Empty;
				if (clicked_item != null) {
					if (owner.view == View.Details && !owner.full_row_select)
						rect = clicked_item.GetBounds (ItemBoundsPortion.Label);
					else
						rect = clicked_item.Bounds;

					if (rect.Contains (pt)) {
						switch (owner.activation) {
						case ItemActivation.OneClick:
							owner.OnItemActivate (EventArgs.Empty);
							break;

						case ItemActivation.TwoClick:
							if (last_clicked_item == clicked_item) {
								owner.OnItemActivate (EventArgs.Empty);
								last_clicked_item = null;
							} else
								last_clicked_item = clicked_item;
							break;
						default:
							// DoubleClick activation is handled in another handler
							break;
						}
					}
				} else if (!checking && owner.SelectedItems.Count > 0 && BoxSelectRectangle.Size.IsEmpty) {
					// Need this to clean up background clicks
					owner.SelectedItems.Clear ();
					owner.OnSelectedIndexChanged (EventArgs.Empty);
				}

				clicked_item = null;
				box_select_start = Point.Empty;
				BoxSelectRectangle = Rectangle.Empty;
				prev_selection = null;
				box_select_mode = BoxSelect.None;
				checking = false;
				owner.OnMouseUp (owner.TranslateMouseEventArgs (me));
			}
			
			internal void LabelEditFinished (object sender, EventArgs e)
			{
				EndEdit (edit_item);
			}
			
			internal void BeginEdit (ListViewItem item)
			{
				if (edit_item != null)
					EndEdit (edit_item);
				
				if (edit_text_box == null) {
					edit_text_box = new ListViewLabelEditTextBox ();
					edit_text_box.BorderStyle = BorderStyle.FixedSingle;
					edit_text_box.EditingFinished += new EventHandler (LabelEditFinished);
					edit_text_box.Visible = false;
					Controls.Add (edit_text_box);
				}
				
				item.EnsureVisible();
				
				edit_text_box.Reset ();
				
				switch (owner.view) {
					case View.List:
					case View.SmallIcon:
					case View.Details:
						edit_text_box.TextAlign = HorizontalAlignment.Left;
						edit_text_box.Bounds = item.GetBounds (ItemBoundsPortion.Label);
						SizeF sizef = DeviceContext.MeasureString (item.Text, item.Font);
						edit_text_box.Width = (int)sizef.Width + 4;
						edit_text_box.MaxWidth = owner.ClientRectangle.Width - edit_text_box.Bounds.X;
						edit_text_box.WordWrap = false;
						edit_text_box.Multiline = false;
						break;
					case View.LargeIcon:
						edit_text_box.TextAlign = HorizontalAlignment.Center;
						edit_text_box.Bounds = item.GetBounds (ItemBoundsPortion.Label);
						sizef = DeviceContext.MeasureString (item.Text, item.Font);
						edit_text_box.Width = (int)sizef.Width + 4;
						edit_text_box.MaxWidth = item.GetBounds(ItemBoundsPortion.Entire).Width;
						edit_text_box.MaxHeight = owner.ClientRectangle.Height - edit_text_box.Bounds.Y;
						edit_text_box.WordWrap = true;
						edit_text_box.Multiline = true;
						break;
				}
				
				edit_text_box.Text = item.Text;
				edit_text_box.Font = item.Font;
				edit_text_box.Visible = true;
				edit_text_box.Focus ();
				edit_text_box.SelectAll ();
				
				edit_args = new LabelEditEventArgs (owner.Items.IndexOf(edit_item));
				owner.OnBeforeLabelEdit (edit_args);
				
				if (edit_args.CancelEdit)
					EndEdit (item);
				
				edit_item = item;
			}
			
			internal void EndEdit (ListViewItem item)
			{
				if (edit_text_box != null && edit_text_box.Visible) {
					edit_text_box.Visible = false;
				}
				
				if (edit_item != null && edit_item == item) {
					owner.OnAfterLabelEdit (edit_args);
					
					if (!edit_args.CancelEdit) {
						if (edit_args.Label != null)
							edit_item.Text = edit_args.Label;
						else
							edit_item.Text = edit_text_box.Text;
					}
					
				}
				
				
				edit_item = null;
			}

			internal override void OnPaintInternal (PaintEventArgs pe)
			{
				ThemeEngine.Current.DrawListViewItems (pe.Graphics, pe.ClipRectangle, owner);
			}

			internal override void OnGotFocusInternal (EventArgs e)
			{
				owner.Select (false, true);
			}

			internal override void OnLostFocusInternal (EventArgs e)
			{
				owner.Select (false, true);
			}
		}
		
		internal class ListViewLabelEditTextBox : TextBox
		{
			int max_width = -1;
			int min_width = -1;
			
			int max_height = -1;
			int min_height = -1;
			
			int old_number_lines = 1;
			
			SizeF text_size_one_char;
			
			public ListViewLabelEditTextBox ()
			{
				min_height = DefaultSize.Height;
				text_size_one_char = DeviceContext.MeasureString ("B", Font);
			}
			
			public int MaxWidth {
				set {
					if (value < min_width)
						max_width = min_width;
					else
						max_width = value;
				}
			}
			
			public int MaxHeight {
				set {
					if (value < min_height)
						max_height = min_height;
					else
						max_height = value;
				}
			}
			
			public new int Width {
				get {
					return base.Width;
				}
				set {
					min_width = value;
					base.Width = value;
				}
			}
			
			public override Font Font {
				get {
					return base.Font;
				}
				set {
					base.Font = value;
					text_size_one_char = DeviceContext.MeasureString ("B", Font);
				}
			}
			
			protected override void OnTextChanged (EventArgs e)
			{
				SizeF text_size = DeviceContext.MeasureString (Text, Font);
				
				int new_width = (int)text_size.Width + 8;
				
				if (!Multiline)
					ResizeTextBoxWidth (new_width);
				else {
					if (Width != max_width)
						ResizeTextBoxWidth (new_width);
					
					int number_lines = Lines.Length;
					
					if (number_lines != old_number_lines) {
						int new_height = number_lines * (int)text_size_one_char.Height + 4;
						old_number_lines = number_lines;
						
						ResizeTextBoxHeight (new_height);
					}
				}
				
				base.OnTextChanged (e);
			}
			
			protected override bool IsInputKey (Keys key_data)
			{
				if ((key_data & Keys.Alt) == 0) {
					switch (key_data & Keys.KeyCode) {
						case Keys.Enter:
							return true;
					}
				}
				return base.IsInputKey (key_data);
			}
			
			protected override void OnKeyDown (KeyEventArgs e)
			{
				if (e.KeyCode == Keys.Return && Visible) {
					this.Visible = false;
					OnEditingFinished (e);
				}
			}
			
			protected override void OnLostFocus (EventArgs e)
			{
				if (Visible) {
					OnEditingFinished (e);
				}
			}
			
			protected void OnEditingFinished (EventArgs e)
			{
				EventHandler eh = (EventHandler)(Events [EditingFinishedEvent]);
				if (eh != null)
					eh (this, e);
			}
			
			private void ResizeTextBoxWidth (int new_width)
			{
				if (new_width > max_width)
					base.Width = max_width;
				else 
				if (new_width >= min_width)
					base.Width = new_width;
				else
					base.Width = min_width;
			}
			
			private void ResizeTextBoxHeight (int new_height)
			{
				if (new_height > max_height)
					base.Height = max_height;
				else 
				if (new_height >= min_height)
					base.Height = new_height;
				else
					base.Height = min_height;
			}
			
			public void Reset ()
			{
				max_width = -1;
				min_width = -1;
				
				max_height = -1;
				
				old_number_lines = 1;
				
				Text = String.Empty;
				
				Size = DefaultSize;
			}
			
			static object EditingFinishedEvent = new object ();
			public event EventHandler EditingFinished {
				add { Events.AddHandler (EditingFinishedEvent, value); }
				remove { Events.RemoveHandler (EditingFinishedEvent, value); }
			}
		}

		internal override void OnPaintInternal (PaintEventArgs pe)
		{
			if (updating)
				return;	
				
			CalculateScrollBars ();
		}

		void FocusChanged (object o, EventArgs args)
		{
			if (Items.Count == 0)
				return;

			if (FocusedItem == null)
				SetFocusedItem (Items [0]);

			item_control.Invalidate (FocusedItem.Bounds);
		}

		private void ListView_MouseWheel (object sender, MouseEventArgs me)
		{
			if (Items.Count == 0)
				return;

			int lines = me.Delta / 120;

			if (lines == 0)
				return;

			switch (View) {
			case View.Details:
			case View.SmallIcon:
				Scroll (v_scroll, -Items [0].Bounds.Height * SystemInformation.MouseWheelScrollLines * lines);
				break;
			case View.LargeIcon:
				Scroll (v_scroll, -(Items [0].Bounds.Height + ThemeEngine.Current.ListViewVerticalSpacing)  * lines);
				break;
			case View.List:
				Scroll (h_scroll, -Items [0].Bounds.Width * lines);
				break;
			}
		}

		private void ListView_SizeChanged (object sender, EventArgs e)
		{
			CalculateListView (alignment);
		}
		
		private void SetFocusedItem (ListViewItem item)
		{
			if (focused_item != null)
				focused_item.Focused = false;
			
			if (item != null)
				item.Focused = true;
				
			focused_item = item;
		}

		private void HorizontalScroller (object sender, EventArgs e)
		{
			item_control.EndEdit (item_control.edit_item);
			
			// Avoid unnecessary flickering, when button is
			// kept pressed at the end
			if (h_marker != h_scroll.Value) {
				
				int pixels =  h_marker - h_scroll.Value;
				
				h_marker = h_scroll.Value;
				if (header_control.Visible)
					XplatUI.ScrollWindow (header_control.Handle, pixels, 0, false);

				XplatUI.ScrollWindow (item_control.Handle, pixels, 0, false);
			}
		}

		private void VerticalScroller (object sender, EventArgs e)
		{
			item_control.EndEdit (item_control.edit_item);
			
			// Avoid unnecessary flickering, when button is
			// kept pressed at the end
			if (v_marker != v_scroll.Value) {
				int pixels =  v_marker - v_scroll.Value;
				Rectangle area = item_control.ClientRectangle;
				v_marker = v_scroll.Value;
				XplatUI.ScrollWindow (item_control.Handle, area, 0, pixels, false);
			}
		}
		#endregion	// Internal Methods Properties

		#region Protected Methods
		protected override void CreateHandle ()
		{
			base.CreateHandle ();
			for (int i = 0; i < SelectedItems.Count; i++)
				OnSelectedIndexChanged (EventArgs.Empty);
		}

		protected override void Dispose (bool disposing)
		{			
			if (disposing) {			
				h_scroll.Dispose ();
				v_scroll.Dispose ();
				
				large_image_list = null;
				small_image_list = null;
				state_image_list = null;
			}
			
			base.Dispose (disposing);
		}

		protected override bool IsInputKey (Keys keyData)
		{
			switch (keyData) {
			case Keys.Up:
			case Keys.Down:
			case Keys.PageUp:
			case Keys.PageDown:
			case Keys.Right:
			case Keys.Left:
			case Keys.End:
			case Keys.Home:				
				return true;

			default:
				break;
			}
			
			return base.IsInputKey (keyData);
		}

		protected virtual void OnAfterLabelEdit (LabelEditEventArgs e)
		{
			LabelEditEventHandler eh = (LabelEditEventHandler)(Events [AfterLabelEditEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnBeforeLabelEdit (LabelEditEventArgs e)
		{
			LabelEditEventHandler eh = (LabelEditEventHandler)(Events [BeforeLabelEditEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnColumnClick (ColumnClickEventArgs e)
		{
			ColumnClickEventHandler eh = (ColumnClickEventHandler)(Events [ColumnClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnEnabledChanged (EventArgs e)
		{
			base.OnEnabledChanged (e);
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
			Redraw (true);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
			Sort ();
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected virtual void OnItemActivate (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ItemActivateEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnItemCheck (ItemCheckEventArgs ice)
		{
			EventHandler eh = (EventHandler)(Events [ItemCheckEvent]);
			if (eh != null)
				eh (this, ice);
		}

		protected virtual void OnItemDrag (ItemDragEventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ItemDragEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnSelectedIndexChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [SelectedIndexChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnSystemColorsChanged (EventArgs e)
		{
			base.OnSystemColorsChanged (e);
		}

		protected void RealizeProperties ()
		{
			// FIXME: TODO
		}

		protected void UpdateExtendedStyles ()
		{
			// FIXME: TODO
		}

		bool refocusing = false;

		protected override void WndProc (ref Message m)
		{
			switch ((Msg)m.Msg) {
			case Msg.WM_KILLFOCUS:
				Control receiver = Control.FromHandle (m.WParam);
				if (receiver == item_control) {
					has_focus = false;
					refocusing = true;
					return;
				}
				break;
			case Msg.WM_SETFOCUS:
				if (refocusing) {
					has_focus = true;
					refocusing = false;
					return;
				}
				break;
			default:
				break;
			}
			base.WndProc (ref m);
		}
		#endregion // Protected Methods

		#region Public Instance Methods
		public void ArrangeIcons ()
		{
			ArrangeIcons (this.alignment);
		}

		public void ArrangeIcons (ListViewAlignment alignment)
		{
			// Icons are arranged only if view is set to LargeIcon or SmallIcon
			if (view == View.LargeIcon || view == View.SmallIcon) {
				this.CalculateListView (alignment);
				// we have done the calculations already
				this.Redraw (false);
			}
		}

		public void BeginUpdate ()
		{
			// flag to avoid painting
			updating = true;
		}

		public void Clear ()
		{
			columns.Clear ();
			items.Clear ();	// Redraw (true) called here			
		}

		public void EndUpdate ()
		{
			// flag to avoid painting
			updating = false;

			// probably, now we need a redraw with recalculations
			this.Redraw (true);
		}

		public void EnsureVisible (int index)
		{
			if (index < 0 || index >= items.Count || scrollable == false)
				return;

			Rectangle view_rect = item_control.ClientRectangle;
			Rectangle bounds = items [index].Bounds;

			if (view_rect.Contains (bounds))
				return;

			if (View != View.Details) {
				if (bounds.Left < 0)
					h_scroll.Value += bounds.Left;
				else if (bounds.Right > view_rect.Right)
					h_scroll.Value += (bounds.Right - view_rect.Right);
			}

			if (bounds.Top < 0)
				v_scroll.Value += bounds.Top;
			else if (bounds.Bottom > view_rect.Bottom)
				v_scroll.Value += (bounds.Bottom - view_rect.Bottom);
		}
		
		public ListViewItem GetItemAt (int x, int y)
		{
			foreach (ListViewItem item in items) {
				if (item.Bounds.Contains (x, y))
					return item;
			}
			return null;
		}

		public Rectangle GetItemRect (int index)
		{
			return GetItemRect (index, ItemBoundsPortion.Entire);
		}

		public Rectangle GetItemRect (int index, ItemBoundsPortion portion)
		{
			if (index < 0 || index >= items.Count)
				throw new IndexOutOfRangeException ("index");

			return items [index].GetBounds (portion);
		}

		public void Sort ()
		{
			Sort (true);
		}

		// we need this overload to reuse the logic for sorting, while allowing
		// redrawing to be done by caller or have it done by this method when
		// sorting is really performed
		//
		// ListViewItemCollection's Add and AddRange methods call this overload
		// with redraw set to false, as they take care of redrawing themselves
		// (they even want to redraw the listview if no sort is performed, as 
		// an item was added), while ListView.Sort () only wants to redraw if 
		// sorting was actually performed
		private void Sort (bool redraw)
		{
			if (!IsHandleCreated || item_sorter == null) {
				return;
			}
			
			items.Sort (item_sorter);
			if (redraw)
				this.Redraw (true);
		}

		public override string ToString ()
		{
			int count = this.Items.Count;

			if (count == 0)
				return string.Format ("System.Windows.Forms.ListView, Items.Count: 0");
			else
				return string.Format ("System.Windows.Forms.ListView, Items.Count: {0}, Items[0]: {1}", count, this.Items [0].ToString ());
		}
		#endregion	// Public Instance Methods


		#region Subclasses

		class HeaderControl : Control {

			ListView owner;
			bool column_resize_active = false;
			ColumnHeader resize_column;
			ColumnHeader clicked_column;
			ColumnHeader drag_column;
			int drag_x;
			int drag_to_index = -1;

			public HeaderControl (ListView owner)
			{
				this.owner = owner;
				MouseDown += new MouseEventHandler (HeaderMouseDown);
				MouseMove += new MouseEventHandler (HeaderMouseMove);
				MouseUp += new MouseEventHandler (HeaderMouseUp);
			}

			private ColumnHeader ColumnAtX (int x)
			{
				Point pt = new Point (x, 0);
				ColumnHeader result = null;
				foreach (ColumnHeader col in owner.Columns) {
					if (col.Rect.Contains (pt)) {
						result = col;
						break;
					}
				}
				return result;
			}

			private int GetReorderedIndex (ColumnHeader col)
			{
				if (owner.reordered_column_indices == null)
					return col.Index;
				else
					for (int i = 0; i < owner.Columns.Count; i++)
						if (owner.reordered_column_indices [i] == col.Index)
							return i;
				throw new Exception ("Column index missing from reordered array");
			}

			private void HeaderMouseDown (object sender, MouseEventArgs me)
			{
				if (resize_column != null) {
					column_resize_active = true;
					Capture = true;
					return;
				}

				clicked_column = ColumnAtX (me.X + owner.h_marker);

				if (clicked_column != null) {
					Capture = true;
					if (owner.AllowColumnReorder) {
						drag_x = me.X;
						drag_column = (ColumnHeader) (clicked_column as ICloneable).Clone ();
						drag_column.Rect = clicked_column.Rect;
						drag_to_index = GetReorderedIndex (clicked_column);
					}
					clicked_column.Pressed = true;
					Rectangle bounds = clicked_column.Rect;
					bounds.X -= owner.h_marker;
					Invalidate (bounds);
					return;
				}
			}

			void StopResize ()
			{
				column_resize_active = false;
				resize_column = null;
				Capture = false;
				Cursor = Cursors.Default;
			}
			
			private void HeaderMouseMove (object sender, MouseEventArgs me)
			{
				Point pt = new Point (me.X + owner.h_marker, me.Y);

				if (column_resize_active)  {
					int width = pt.X - resize_column.X;
					if (width < 0)
						width = 0;

					if (!owner.CanProceedWithResize (resize_column, width)){
						StopResize ();
						return;
					}
					resize_column.Width = width;
					return;
				}

				resize_column = null;

				if (clicked_column != null) {
					if (owner.AllowColumnReorder) {
						Rectangle r;

						r = drag_column.Rect;
						r.X = clicked_column.Rect.X + me.X - drag_x;
						drag_column.Rect = r;

						int x = me.X + owner.h_marker;
						ColumnHeader over = ColumnAtX (x);
						if (over == null)
							drag_to_index = owner.Columns.Count;
						else if (x < over.X + over.Width / 2)
							drag_to_index = GetReorderedIndex (over);
						else
							drag_to_index = GetReorderedIndex (over) + 1;
						Invalidate ();
					} else {
						ColumnHeader over = ColumnAtX (me.X + owner.h_marker);
						bool pressed = clicked_column.Pressed;
						clicked_column.Pressed = over == clicked_column;
						if (clicked_column.Pressed ^ pressed) {
							Rectangle bounds = clicked_column.Rect;
							bounds.X -= owner.h_marker;
							Invalidate (bounds);
						}
					}
					return;
				}

				for (int i = 0; i < owner.Columns.Count; i++) {
					Rectangle zone = owner.Columns [i].Rect;
					zone.X = zone.Right - 5;
					zone.Width = 10;
					if (zone.Contains (pt)) {
						if (i < owner.Columns.Count - 1 && owner.Columns [i + 1].Width == 0)
							i++;
						resize_column = owner.Columns [i];
						break;
					}
				}

				if (resize_column == null)
					Cursor = Cursors.Default;
				else
					Cursor = Cursors.VSplit;
			}

			void HeaderMouseUp (object sender, MouseEventArgs me)
			{
				Capture = false;

				if (column_resize_active) {
					int column_idx = resize_column.Index;
					StopResize ();
					owner.RaiseColumnWidthChanged (column_idx);
					return;
				}

				if (clicked_column != null && clicked_column.Pressed) {
					clicked_column.Pressed = false;
					Rectangle bounds = clicked_column.Rect;
					bounds.X -= owner.h_marker;
					Invalidate (bounds);
					owner.OnColumnClick (new ColumnClickEventArgs (clicked_column.Index));
				}

				if (drag_column != null && owner.AllowColumnReorder) {
					drag_column = null;
					if (drag_to_index > GetReorderedIndex (clicked_column))
						drag_to_index--;
					if (owner.GetReorderedColumn (drag_to_index) != clicked_column)
						owner.ReorderColumn (clicked_column, drag_to_index);
					drag_to_index = -1;
					Invalidate ();
				}

				clicked_column = null;
			}

			internal override void OnPaintInternal (PaintEventArgs pe)
			{
				if (owner.updating)
					return;	
				
				Theme theme = ThemeEngine.Current;
				theme.DrawListViewHeader (pe.Graphics, pe.ClipRectangle, this.owner);

				if (drag_column == null)
					return;

				int target_x;
				if (drag_to_index == owner.Columns.Count)
					target_x = owner.GetReorderedColumn (drag_to_index - 1).Rect.Right - owner.h_marker;
				else
					target_x = owner.GetReorderedColumn (drag_to_index).Rect.X - owner.h_marker;
				theme.DrawListViewHeaderDragDetails (pe.Graphics, owner, drag_column, target_x);
			}

			protected override void WndProc (ref Message m)
			{
				switch ((Msg)m.Msg) {
				case Msg.WM_SETFOCUS:
					owner.Focus ();
					break;
				default:
					base.WndProc (ref m);
					break;
				}
			}
		}

		private class ItemComparer : IComparer {
			readonly SortOrder sort_order;

			public ItemComparer (SortOrder sortOrder)
			{
				sort_order = sortOrder;
			}

			public int Compare (object x, object y)
			{
				ListViewItem item_x = x as ListViewItem;
				ListViewItem item_y = y as ListViewItem;
				if (sort_order == SortOrder.Ascending)
					return String.Compare (item_x.Text, item_y.Text);
				else
					return String.Compare (item_y.Text, item_x.Text);
			}
		}

		public class CheckedIndexCollection : IList, ICollection, IEnumerable
		{
			private readonly ListView owner;

			#region Public Constructor
			public CheckedIndexCollection (ListView owner)
			{
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get { return owner.CheckedItems.Count; }
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public int this [int index] {
				get {
					int [] indices = GetIndices ();
					if (index < 0 || index >= indices.Length)
						throw new ArgumentOutOfRangeException ("index");
					return indices [index];
				}
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return true; }
			}

			object IList.this [int index] {
				get { return this [index]; }
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
			public bool Contains (int checkedIndex)
			{
				int [] indices = GetIndices ();
				for (int i = 0; i < indices.Length; i++) {
					if (indices [i] == checkedIndex)
						return true;
				}
				return false;
			}

			public IEnumerator GetEnumerator ()
			{
				int [] indices = GetIndices ();
				return indices.GetEnumerator ();
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				int [] indices = GetIndices ();
				Array.Copy (indices, 0, dest, index, indices.Length);
			}

			int IList.Add (object value)
			{
				throw new NotSupportedException ("Add operation is not supported.");
			}

			void IList.Clear ()
			{
				throw new NotSupportedException ("Clear operation is not supported.");
			}

			bool IList.Contains (object checkedIndex)
			{
				if (!(checkedIndex is int))
					return false;
				return Contains ((int) checkedIndex);
			}

			int IList.IndexOf (object checkedIndex)
			{
				if (!(checkedIndex is int))
					return -1;
				return IndexOf ((int) checkedIndex);
			}

			void IList.Insert (int index, object value)
			{
				throw new NotSupportedException ("Insert operation is not supported.");
			}

			void IList.Remove (object value)
			{
				throw new NotSupportedException ("Remove operation is not supported.");
			}

			void IList.RemoveAt (int index)
			{
				throw new NotSupportedException ("RemoveAt operation is not supported.");
			}

			public int IndexOf (int checkedIndex)
			{
				int [] indices = GetIndices ();
				for (int i = 0; i < indices.Length; i++) {
					if (indices [i] == checkedIndex)
						return i;
				}
				return -1;
			}
			#endregion	// Public Methods

			private int [] GetIndices ()
			{
				ArrayList checked_items = owner.CheckedItems.List;
				int [] indices = new int [checked_items.Count];
				for (int i = 0; i < checked_items.Count; i++) {
					ListViewItem item = (ListViewItem) checked_items [i];
					indices [i] = item.Index;
				}
				return indices;
			}
		}	// CheckedIndexCollection

		public class CheckedListViewItemCollection : IList, ICollection, IEnumerable
		{
			private readonly ListView owner;
			private ArrayList list;

			#region Public Constructor
			public CheckedListViewItemCollection (ListView owner)
			{
				this.owner = owner;
				this.owner.Items.Changed += new CollectionChangedHandler (
					ItemsCollection_Changed);
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get {
					if (!owner.CheckBoxes)
						return 0;
					return List.Count;
				}
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public ListViewItem this [int index] {
				get {
					ArrayList checked_items = List;
					if (index < 0 || index >= checked_items.Count)
						throw new ArgumentOutOfRangeException ("index");
					return (ListViewItem) checked_items [index];
				}
			}

#if NET_2_0
			public virtual ListViewItem this [string key] {
				get {
					int idx = IndexOfKey (key);
					return idx == -1 ? null : (ListViewItem) List [idx];
				}
			}
#endif

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return true; }
			}

			object IList.this [int index] {
				get { return this [index]; }
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
			public bool Contains (ListViewItem item)
			{
				if (!owner.CheckBoxes)
					return false;
				return List.Contains (item);
			}

#if NET_2_0
			public virtual bool ContainsKey (string key)
			{
				return IndexOfKey (key) != -1;
			}
#endif

			public void CopyTo (Array dest, int index)
			{
				if (!owner.CheckBoxes)
					return;
				List.CopyTo (dest, index);
			}

			public IEnumerator GetEnumerator ()
			{
				if (!owner.CheckBoxes)
					return (new ListViewItem [0]).GetEnumerator ();
				return List.GetEnumerator ();
			}

			int IList.Add (object value)
			{
				throw new NotSupportedException ("Add operation is not supported.");
			}

			void IList.Clear ()
			{
				throw new NotSupportedException ("Clear operation is not supported.");
			}

			bool IList.Contains (object item)
			{
				if (!(item is ListViewItem))
					return false;
				return Contains ((ListViewItem) item);
			}

			int IList.IndexOf (object item)
			{
				if (!(item is ListViewItem))
					return -1;
				return IndexOf ((ListViewItem) item);
			}

			void IList.Insert (int index, object value)
			{
				throw new NotSupportedException ("Insert operation is not supported.");
			}

			void IList.Remove (object value)
			{
				throw new NotSupportedException ("Remove operation is not supported.");
			}

			void IList.RemoveAt (int index)
			{
				throw new NotSupportedException ("RemoveAt operation is not supported.");
			}

			public int IndexOf (ListViewItem item)
			{
				if (!owner.CheckBoxes)
					return -1;
				return List.IndexOf (item);
			}

#if NET_2_0
			public virtual int IndexOfKey (string key)
			{
				if (key == null || key.Length == 0)
					return -1;

				ArrayList checked_items = List;
				for (int i = 0; i < checked_items.Count; i++) {
					ListViewItem item = (ListViewItem) checked_items [i];
					if (String.Compare (key, item.Name, true) == 0)
						return i;
				}

				return -1;
			}
#endif
			#endregion	// Public Methods

			internal ArrayList List {
				get {
					if (list == null) {
						list = new ArrayList ();
						foreach (ListViewItem item in owner.Items) {
							if (item.Checked)
								list.Add (item);
						}
					}
					return list;
				}
			}

			internal void Reset ()
			{
				// force re-population of list
				list = null;
			}

			private void ItemsCollection_Changed ()
			{
				Reset ();
			}
		}	// CheckedListViewItemCollection

		public class ColumnHeaderCollection : IList, ICollection, IEnumerable
		{
			internal ArrayList list;
			private ListView owner;

			#region Public Constructor
			public ColumnHeaderCollection (ListView owner)
			{
				list = new ArrayList ();
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get { return list.Count; }
			}

			public bool IsReadOnly {
				get { return false; }
			}

			public virtual ColumnHeader this [int index] {
				get {
					if (index < 0 || index >= list.Count)
						throw new ArgumentOutOfRangeException ("index");
					return (ColumnHeader) list [index];
				}
			}

#if NET_2_0
			public virtual ColumnHeader this [string key] {
				get {
					int idx = IndexOfKey (key);
					if (idx == -1)
						return null;

					return (ColumnHeader) list [idx];
				}
			}
#endif

			bool ICollection.IsSynchronized {
				get { return true; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return list.IsFixedSize; }
			}

			object IList.this [int index] {
				get { return this [index]; }
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
			public virtual int Add (ColumnHeader value)
			{
				int idx;
				value.SetListView (this.owner);
				idx = list.Add (value);
				if (owner.IsHandleCreated) {
					owner.Redraw (true); 
				}
				return idx;
			}

			public virtual ColumnHeader Add (string str, int width, HorizontalAlignment textAlign)
			{
				ColumnHeader colHeader = new ColumnHeader (this.owner, str, textAlign, width);
				this.Add (colHeader);									
				return colHeader;
			}

#if NET_2_0
			public virtual ColumnHeader Add (string text)
			{
				return Add (String.Empty, text);
			}

			public virtual ColumnHeader Add (string text, int iwidth)
			{
				return Add (String.Empty, text, iwidth);
			}

			public virtual ColumnHeader Add (string key, string text)
			{
				ColumnHeader colHeader = new ColumnHeader ();
				colHeader.Name = key;
				colHeader.Text = text;
				Add (colHeader);
				return colHeader;
			}

			public virtual ColumnHeader Add (string key, string text, int iwidth)
			{
				return Add (key, text, iwidth, HorizontalAlignment.Left, -1);
			}

			public virtual ColumnHeader Add (string key, string text, int iwidth, HorizontalAlignment textAlign, int imageIndex)
			{
				ColumnHeader colHeader = new ColumnHeader (key, text, iwidth, textAlign);
				colHeader.ImageIndex = imageIndex;
				Add (colHeader);
				return colHeader;
			}

			public virtual ColumnHeader Add (string key, string text, int iwidth, HorizontalAlignment textAlign, string imageKey)
			{
				ColumnHeader colHeader = new ColumnHeader (key, text, iwidth, textAlign);
				colHeader.ImageKey = imageKey;
				Add (colHeader);
				return colHeader;
			}
#endif

			public virtual void AddRange (ColumnHeader [] values)
			{
				foreach (ColumnHeader colHeader in values) {
					colHeader.SetListView (this.owner);
					Add (colHeader);
				}
				
				owner.Redraw (true); 
			}

			public virtual void Clear ()
			{
				list.Clear ();
				owner.Redraw (true);
			}

			public bool Contains (ColumnHeader value)
			{
				return list.Contains (value);
			}

#if NET_2_0
			public virtual bool ContainsKey (string key)
			{
				return IndexOfKey (key) != -1;
			}
#endif

			public IEnumerator GetEnumerator ()
			{
				return list.GetEnumerator ();
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
			}

			int IList.Add (object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException ("Not of type ColumnHeader", "value");
				}

				return this.Add ((ColumnHeader) value);
			}

			bool IList.Contains (object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException ("Not of type ColumnHeader", "value");
				}

				return this.Contains ((ColumnHeader) value);
			}

			int IList.IndexOf (object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException ("Not of type ColumnHeader", "value");
				}

				return this.IndexOf ((ColumnHeader) value);
			}

			void IList.Insert (int index, object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException ("Not of type ColumnHeader", "value");
				}

				this.Insert (index, (ColumnHeader) value);
			}

			void IList.Remove (object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException ("Not of type ColumnHeader", "value");
				}

				this.Remove ((ColumnHeader) value);
			}

			public int IndexOf (ColumnHeader value)
			{
				return list.IndexOf (value);
			}

#if NET_2_0
			public virtual int IndexOfKey (string key)
			{
				if (key == null || key.Length == 0)
					return -1;

				for (int i = 0; i < list.Count; i++) {
					ColumnHeader col = (ColumnHeader) list [i];
					if (String.Compare (key, col.Name, true) == 0)
						return i;
				}

				return -1;
			}
#endif

			public void Insert (int index, ColumnHeader value)
			{
				// LAMESPEC: MSDOCS say greater than or equal to the value of the Count property
				// but it's really only greater.
				if (index < 0 || index > list.Count)
					throw new ArgumentOutOfRangeException ("index");

				value.SetListView (owner);
				list.Insert (index, value);
				owner.Redraw (true);
			}

#if NET_2_0
			public void Insert (int index, string text)
			{
				Insert (index, String.Empty, text);
			}

			public void Insert (int index, string text, int width)
			{
				Insert (index, String.Empty, text, width);
			}

			public void Insert (int index, string key, string text)
			{
				ColumnHeader colHeader = new ColumnHeader ();
				colHeader.Name = key;
				colHeader.Text = text;
				Insert (index, colHeader);
			}

			public void Insert (int index, string key, string text, int width)
			{
				ColumnHeader colHeader = new ColumnHeader (key, text, width, HorizontalAlignment.Left);
				Insert (index, colHeader);
			}

			public void Insert (int index, string key, string text, int width, HorizontalAlignment textAlign, int imageIndex)
			{
				ColumnHeader colHeader = new ColumnHeader (key, text, width, textAlign);
				colHeader.ImageIndex = imageIndex;
				Insert (index, colHeader);
			}

			public void Insert (int index, string key, string text, int width, HorizontalAlignment textAlign, string imageKey)
			{
				ColumnHeader colHeader = new ColumnHeader (key, text, width, textAlign);
				colHeader.ImageKey = imageKey;
				Insert (index, colHeader);
			}
#endif

			public void Insert (int index, string str, int width, HorizontalAlignment textAlign)
			{
				ColumnHeader colHeader = new ColumnHeader (this.owner, str, textAlign, width);
				this.Insert (index, colHeader);
			}

			public virtual void Remove (ColumnHeader column)
			{
				// TODO: Update Column internal index ?
				list.Remove (column);
				owner.Redraw (true);
			}

#if NET_2_0
			public virtual void RemoveByKey (string key)
			{
				int idx = IndexOfKey (key);
				if (idx != -1)
					RemoveAt (idx);
			}
#endif

			public virtual void RemoveAt (int index)
			{
				if (index < 0 || index >= list.Count)
					throw new ArgumentOutOfRangeException ("index");

				// TODO: Update Column internal index ?
				list.RemoveAt (index);
				owner.Redraw (true);
			}
			#endregion	// Public Methods
			

		}	// ColumnHeaderCollection

		public class ListViewItemCollection : IList, ICollection, IEnumerable
		{
			private readonly ArrayList list;
			private readonly ListView owner;

			#region Public Constructor
			public ListViewItemCollection (ListView owner)
			{
				list = new ArrayList ();
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get { return list.Count; }
			}

			public bool IsReadOnly {
				get { return false; }
			}

			public virtual ListViewItem this [int displayIndex] {
				get {
					if (displayIndex < 0 || displayIndex >= list.Count)
						throw new ArgumentOutOfRangeException ("displayIndex");
					return (ListViewItem) list [displayIndex];
				}

				set {
					if (displayIndex < 0 || displayIndex >= list.Count)
						throw new ArgumentOutOfRangeException ("displayIndex");

					if (list.Contains (value))
						throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "value");

					if (value.ListView != null && value.ListView != owner)
						throw new ArgumentException ("Cannot add or insert the item '" + value.Text + "' in more than one place. You must first remove it from its current location or clone it.", "value");

					value.Owner = owner;
					list [displayIndex] = value;
					OnChange ();

					owner.Redraw (true);
				}
			}

#if NET_2_0
			public virtual ListViewItem this [string key] {
				get {
					int idx = IndexOfKey (key);
					if (idx == -1)
						return null;

					return (ListViewItem) list [idx];
				}
			}
#endif

			bool ICollection.IsSynchronized {
				get { return true; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return list.IsFixedSize; }
			}

			object IList.this [int index] {
				get { return this [index]; }
				set {
					if (value is ListViewItem)
						this [index] = (ListViewItem) value;
					else
						this [index] = new ListViewItem (value.ToString ());
					OnChange ();
				}
			}
			#endregion	// Public Properties

			#region Public Methods
			public virtual ListViewItem Add (ListViewItem value)
			{
				if (list.Contains (value))
					throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "value");

				if (value.ListView != null && value.ListView != owner)
					throw new ArgumentException ("Cannot add or insert the item '" + value.Text + "' in more than one place. You must first remove it from its current location or clone it.", "value");

				value.Owner = owner;
				list.Add (value);

				if (this.owner != null)
				{
					owner.Sort (false);
					OnChange ();
					owner.Redraw (true);
				}

				return value;
			}

			public virtual ListViewItem Add (string text)
			{
				ListViewItem item = new ListViewItem (text);
				return this.Add (item);
			}

			public virtual ListViewItem Add (string text, int imageIndex)
			{
				ListViewItem item = new ListViewItem (text, imageIndex);
				return this.Add (item);
			}

#if NET_2_0
			public virtual ListViewItem Add (string text, string imageKey)
			{
				ListViewItem item = new ListViewItem (text, imageKey);
				return this.Add (item);
			}

			public virtual ListViewItem Add (string key, string text, int imageIndex)
			{
				ListViewItem item = new ListViewItem (text, imageIndex);
				item.Name = key;
				return this.Add (item);
			}

			public virtual ListViewItem Add (string key, string text, string imageKey)
			{
				ListViewItem item = new ListViewItem (text, imageKey);
				item.Name = key;
				return this.Add (item);
			}
#endif

			public void AddRange (ListViewItem [] values)
			{
				if (values == null)
					throw new ArgumentNullException ("Argument cannot be null!", "values");

				foreach (ListViewItem item in values) {
					this.Add (item);
				}
			}

#if NET_2_0
			public void AddRange (ListViewItemCollection items)
			{
				if (items == null)
					throw new ArgumentNullException ("Argument cannot be null!", "items");

				ListViewItem[] itemArray = new ListViewItem[items.Count];
				items.CopyTo (itemArray,0);
				this.AddRange (itemArray);
			}
#endif

			public virtual void Clear ()
			{
				owner.SetFocusedItem (null);
				owner.h_scroll.Value = owner.v_scroll.Value = 0;
				list.Clear ();
				OnChange ();
				owner.Redraw (true);
			}

			public bool Contains (ListViewItem item)
			{
				return list.Contains (item);
			}

#if NET_2_0
			public virtual bool ContainsKey (string key)
			{
				return IndexOfKey (key) != -1;
			}
#endif

			public void CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
			}

#if NET_2_0
			public ListViewItem [] Find (string key, bool searchAllSubitems)
			{
				if (key == null)
					return new ListViewItem [0];

				List<ListViewItem> temp_list = new List<ListViewItem> ();
				
				for (int i = 0; i < list.Count; i++) {
					ListViewItem lvi = (ListViewItem) list [i];
					if (String.Compare (key, lvi.Name, true) == 0)
						temp_list.Add (lvi);
				}

				ListViewItem [] retval = new ListViewItem [temp_list.Count];
				temp_list.CopyTo (retval);

				return retval;
			}
#endif

			public IEnumerator GetEnumerator ()
			{
				return list.GetEnumerator ();
			}

			int IList.Add (object item)
			{
				int result;
				ListViewItem li;

				if (item is ListViewItem) {
					li = (ListViewItem) item;
					if (list.Contains (li))
						throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "item");

					if (li.ListView != null && li.ListView != owner)
						throw new ArgumentException ("Cannot add or insert the item '" + li.Text + "' in more than one place. You must first remove it from its current location or clone it.", "item");
				}
				else
					li = new ListViewItem (item.ToString ());

				li.Owner = owner;
				result = list.Add (li);
				OnChange ();
				owner.Redraw (true);

				return result;
			}

			bool IList.Contains (object item)
			{
				return list.Contains (item);
			}

			int IList.IndexOf (object item)
			{
				return list.IndexOf (item);
			}

			void IList.Insert (int index, object item)
			{
				if (item is ListViewItem)
					this.Insert (index, (ListViewItem) item);
				else
					this.Insert (index, item.ToString ());
			}

			void IList.Remove (object item)
			{
				Remove ((ListViewItem) item);
			}

			public int IndexOf (ListViewItem item)
			{
				return list.IndexOf (item);
			}

#if NET_2_0
			public virtual int IndexOfKey (string key)
			{
				if (key == null || key.Length == 0)
					return -1;

				for (int i = 0; i < list.Count; i++) {
					ListViewItem lvi = (ListViewItem) list [i];
					if (String.Compare (key, lvi.Name, true) == 0)
						return i;
				}

				return -1;
			}
#endif

			public ListViewItem Insert (int index, ListViewItem item)
			{
				if (index < 0 || index > list.Count)
					throw new ArgumentOutOfRangeException ("index");

				if (list.Contains (item))
					throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "item");

				if (item.ListView != null && item.ListView != owner)
					throw new ArgumentException ("Cannot add or insert the item '" + item.Text + "' in more than one place. You must first remove it from its current location or clone it.", "item");

				item.Owner = owner;
				list.Insert (index, item);
				OnChange ();
				owner.Redraw (true);
				return item;
			}

			public ListViewItem Insert (int index, string text)
			{
				return this.Insert (index, new ListViewItem (text));
			}

			public ListViewItem Insert (int index, string text, int imageIndex)
			{
				return this.Insert (index, new ListViewItem (text, imageIndex));
			}

#if NET_2_0
			public ListViewItem Insert (int index, string key, string text, int imageIndex)
			{
				ListViewItem lvi = new ListViewItem (text, imageIndex);
				lvi.Name = key;
				return Insert (index, lvi);
			}
#endif

			public virtual void Remove (ListViewItem item)
			{
				if (!list.Contains (item))
					return;
	 				
				bool selection_changed = owner.SelectedItems.Contains (item);
				list.Remove (item);
				OnChange ();
				owner.Redraw (true);				
				if (selection_changed)
					owner.OnSelectedIndexChanged (EventArgs.Empty);
			}

			public virtual void RemoveAt (int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("index");
				bool selection_changed = owner.SelectedIndices.Contains (index);
				list.RemoveAt (index);
				OnChange ();
				owner.Redraw (false);
				if (selection_changed)
					owner.OnSelectedIndexChanged (EventArgs.Empty);
			}

#if NET_2_0
			public virtual void RemoveByKey (string key)
			{
				int idx = IndexOfKey (key);
				if (idx != -1)
					RemoveAt (idx);
			}
#endif

			#endregion	// Public Methods

			internal event CollectionChangedHandler Changed;

			internal void Sort (IComparer comparer)
			{
				list.Sort (comparer);
				OnChange ();
			}

			internal void OnChange ()
			{
				if (Changed != null)
					Changed ();
			}
		}	// ListViewItemCollection

		public class SelectedIndexCollection : IList, ICollection, IEnumerable
		{
			private readonly ListView owner;

			#region Public Constructor
			public SelectedIndexCollection (ListView owner)
			{
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get {
					return owner.SelectedItems.Count;
				}
			}

			public bool IsReadOnly {
				get { 
#if NET_2_0
					return false;
#else
					return true; 
#endif
				}
			}

			public int this [int index] {
				get {
					int [] indices = GetIndices ();
					if (index < 0 || index >= indices.Length)
						throw new ArgumentOutOfRangeException ("index");
					return indices [index];
				}
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { 
#if NET_2_0
					return false;
#else
					return true;
#endif
				}
			}

			object IList.this [int index] {
				get { return this [index]; }
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
#if NET_2_0
			public int Add (int itemIndex)
			{
				if (itemIndex < 0 || itemIndex >= owner.Items.Count)
					throw new ArgumentOutOfRangeException ("index");

				owner.Items [itemIndex].Selected = true;
				if (!owner.IsHandleCreated)
					return 0;

				return owner.SelectedItems.Count;
			}

			public void Clear ()
			{
				owner.SelectedItems.Clear ();
			}
#endif
			public bool Contains (int selectedIndex)
			{
				int [] indices = GetIndices ();
				for (int i = 0; i < indices.Length; i++) {
					if (indices [i] == selectedIndex)
						return true;
				}
				return false;
			}

			public void CopyTo (Array dest, int index)
			{
				int [] indices = GetIndices ();
				Array.Copy (indices, 0, dest, index, indices.Length);
			}

			public IEnumerator GetEnumerator ()
			{
				int [] indices = GetIndices ();
				return indices.GetEnumerator ();
			}

			int IList.Add (object value)
			{
				throw new NotSupportedException ("Add operation is not supported.");
			}

			void IList.Clear ()
			{
				throw new NotSupportedException ("Clear operation is not supported.");
			}

			bool IList.Contains (object selectedIndex)
			{
				if (!(selectedIndex is int))
					return false;
				return Contains ((int) selectedIndex);
			}

			int IList.IndexOf (object selectedIndex)
			{
				if (!(selectedIndex is int))
					return -1;
				return IndexOf ((int) selectedIndex);
			}

			void IList.Insert (int index, object value)
			{
				throw new NotSupportedException ("Insert operation is not supported.");
			}

			void IList.Remove (object value)
			{
				throw new NotSupportedException ("Remove operation is not supported.");
			}

			void IList.RemoveAt (int index)
			{
				throw new NotSupportedException ("RemoveAt operation is not supported.");
			}

			public int IndexOf (int selectedIndex)
			{
				int [] indices = GetIndices ();
				for (int i = 0; i < indices.Length; i++) {
					if (indices [i] == selectedIndex)
						return i;
				}
				return -1;
			}

#if NET_2_0
			public void Remove (int itemIndex)
			{
				if (itemIndex < 0 || itemIndex >= owner.Items.Count)
					throw new ArgumentOutOfRangeException ("itemIndex");

				owner.Items [itemIndex].Selected = false;
			}
#endif
			#endregion	// Public Methods

			private int [] GetIndices ()
			{
				ArrayList selected_items = owner.SelectedItems.List;
				int [] indices = new int [selected_items.Count];
				for (int i = 0; i < selected_items.Count; i++) {
					ListViewItem item = (ListViewItem) selected_items [i];
					indices [i] = item.Index;
				}
				return indices;
			}
		}	// SelectedIndexCollection

		public class SelectedListViewItemCollection : IList, ICollection, IEnumerable
		{
			private readonly ListView owner;
			private ArrayList list;

			#region Public Constructor
			public SelectedListViewItemCollection (ListView owner)
			{
				this.owner = owner;
				this.owner.Items.Changed += new CollectionChangedHandler (
					ItemsCollection_Changed);
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get {
					if (!owner.IsHandleCreated)
						return 0;
					return List.Count;
				}
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public ListViewItem this [int index] {
				get {
					ArrayList selected_items = List;
					if (!owner.IsHandleCreated || index < 0 || index >= selected_items.Count)
						throw new ArgumentOutOfRangeException ("index");
					return (ListViewItem) selected_items [index];
				}
			}

#if NET_2_0
			public virtual ListViewItem this [string key] {
				get {
					int idx = IndexOfKey (key);
					if (idx == -1)
						return null;

					return (ListViewItem) List [idx];
				}
			}
#endif

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return true; }
			}

			object IList.this [int index] {
				get { return this [index]; }
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
			public void Clear ()
			{
				if (!owner.IsHandleCreated)
					return;

				foreach (ListViewItem item in List)
					item.Selected = false;
			}

			public bool Contains (ListViewItem item)
			{
				if (!owner.IsHandleCreated)
					return false;
				return List.Contains (item);
			}

#if NET_2_0
			public virtual bool ContainsKey (string key)
			{
				return IndexOfKey (key) != -1;
			}
#endif

			public void CopyTo (Array dest, int index)
			{
				if (!owner.IsHandleCreated)
					return;
				List.CopyTo (dest, index);
			}

			public IEnumerator GetEnumerator ()
			{
				if (!owner.IsHandleCreated)
					return (new ListViewItem [0]).GetEnumerator ();
				return List.GetEnumerator ();
			}

			int IList.Add (object value)
			{
				throw new NotSupportedException ("Add operation is not supported.");
			}

			bool IList.Contains (object item)
			{
				if (!(item is ListViewItem))
					return false;
				return Contains ((ListViewItem) item);
			}

			int IList.IndexOf (object item)
			{
				if (!(item is ListViewItem))
					return -1;
				return IndexOf ((ListViewItem) item);
			}

			void IList.Insert (int index, object value)
			{
				throw new NotSupportedException ("Insert operation is not supported.");
			}

			void IList.Remove (object value)
			{
				throw new NotSupportedException ("Remove operation is not supported.");
			}

			void IList.RemoveAt (int index)
			{
				throw new NotSupportedException ("RemoveAt operation is not supported.");
			}

			public int IndexOf (ListViewItem item)
			{
				if (!owner.IsHandleCreated)
					return -1;
				return List.IndexOf (item);
			}

#if NET_2_0
			public virtual int IndexOfKey (string key)
			{
				if (!owner.IsHandleCreated || key == null || key.Length == 0)
					return -1;

				ArrayList selected_items = List;
				for (int i = 0; i < selected_items.Count; i++) {
					ListViewItem item = (ListViewItem) selected_items [i];
					if (String.Compare (item.Name, key, true) == 0)
						return i;
				}

				return -1;
			}
#endif
			#endregion	// Public Methods

			internal ArrayList List {
				get {
					if (list == null) {
						list = new ArrayList ();
						foreach (ListViewItem item in owner.Items) {
							if (item.Selected)
								list.Add (item);
						}
					}
					return list;
				}
			}

			internal void Reset ()
			{
				// force re-population of list
				list = null;
			}

			private void ItemsCollection_Changed ()
			{
				Reset ();
			}
		}	// SelectedListViewItemCollection

		internal delegate void CollectionChangedHandler ();

		#endregion // Subclasses
#if NET_2_0
		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave (e);
		}

		//
		// ColumnReorder event
		//
		static object ColumnReorderedEvent = new object ();
		public event ColumnReorderedEventHandler ColumnReordered {
			add { Events.AddHandler (ColumnReorderedEvent, value); }
			remove { Events.RemoveHandler (ColumnReorderedEvent, value); }
		}

		protected virtual void OnColumnReordered (ColumnReorderedEventArgs e)
		{
			ColumnReorderedEventHandler creh = (ColumnReorderedEventHandler) (Events [ColumnReorderedEvent]);

			if (creh != null)
				creh (this, e);
		}

		//
		// ColumnWidthChanged
		//
		static object ColumnWidthChangedEvent = new object ();
		public event ColumnWidthChangedEventHandler ColumnWidthChanged {
			add { Events.AddHandler (ColumnWidthChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnWidthChangedEvent, value); }
		}

		protected virtual void OnColumnWidthChanged (ColumnWidthChangedEventArgs e)
		{
			ColumnWidthChangedEventHandler eh = (ColumnWidthChangedEventHandler) (Events[ColumnWidthChangedEvent]);
			if (eh != null)
				eh (this, e);
		}
		
		void RaiseColumnWidthChanged (int resize_column)
		{
			ColumnWidthChangedEventArgs n = new ColumnWidthChangedEventArgs (resize_column);

			OnColumnWidthChanged (n);
		}
		
		//
		// ColumnWidthChanging
		//
		static object ColumnWidthChangingEvent = new object ();
		public event ColumnWidthChangingEventHandler ColumnWidthChanging {
			add { Events.AddHandler (ColumnWidthChangingEvent, value); }
			remove { Events.RemoveHandler (ColumnWidthChangingEvent, value); }
		}

		protected virtual void OnColumnWidthChanging (ColumnWidthChangingEventArgs e)
		{
			ColumnWidthChangingEventHandler cwceh = (ColumnWidthChangingEventHandler) (Events[ColumnWidthChangingEvent]);
			if (cwceh != null)
				cwceh (this, e);
		}
		
		//
		// 2.0 profile based implementation
		//
		bool CanProceedWithResize (ColumnHeader col, int width)
		{
			ColumnWidthChangingEventHandler cwceh = (ColumnWidthChangingEventHandler) (Events[ColumnWidthChangingEvent]);
			if (cwceh == null)
				return true;
			
			ColumnWidthChangingEventArgs changing = new ColumnWidthChangingEventArgs (col.Index, width);
			cwceh (this, changing);
			return !changing.Cancel;
		}
#else
		//
		// 1.0 profile based implementation
		//
		bool CanProceedWithResize (ColumnHeader col, int width)
		{
			return true;
		}

		void RaiseColumnWidthChanged (int resize_column)
		{
		}
#endif
	}
}
