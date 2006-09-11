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

namespace System.Windows.Forms
{
	[DefaultEvent ("SelectedIndexChanged")]
	[DefaultProperty ("Items")]
	[Designer ("System.Windows.Forms.Design.ListViewDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class ListView : Control
	{
		private ItemActivation activation = ItemActivation.Standard;
		private ListViewAlignment alignment = ListViewAlignment.Top;
		private bool allow_column_reorder = false;
		private bool auto_arrange = true;
		private bool check_boxes = false;
		private CheckedIndexCollection checked_indices;
		private CheckedListViewItemCollection checked_items;
		private ColumnHeaderCollection columns;
		internal ListViewItem focused_item;
		private bool full_row_select = false;
		private bool grid_lines = false;
		private ColumnHeaderStyle header_style = ColumnHeaderStyle.Clickable;
		private bool hide_selection = true;
		private bool hover_selection = false;
		private IComparer item_sorter;
		private ListViewItemCollection items;
		private bool label_edit = false;
		private bool label_wrap = true;
		private bool multiselect = true;
		private bool scrollable = true;
		private SelectedIndexCollection selected_indices;
		private SelectedListViewItemCollection selected_items;
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

		// internal variables
		internal ImageList large_image_list;
		internal ImageList small_image_list;
		internal Size text_size = Size.Empty;

		#region Events
		public event LabelEditEventHandler AfterLabelEdit;

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		public event LabelEditEventHandler BeforeLabelEdit;
		public event ColumnClickEventHandler ColumnClick;
		public event EventHandler ItemActivate;
		public event ItemCheckEventHandler ItemCheck;
		public event ItemDragEventHandler ItemDrag;

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint {
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}

		public event EventHandler SelectedIndexChanged;

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
			checked_indices = new CheckedIndexCollection (this);
			checked_items = new CheckedListViewItemCollection (this);
			columns = new ColumnHeaderCollection (this);
			foreground_color = SystemColors.WindowText;
			items = new ListViewItemCollection (this);
			selected_indices = new SelectedIndexCollection (this);
			selected_items = new SelectedListViewItemCollection (this);

			border_style = BorderStyle.Fixed3D;

			header_control = new HeaderControl (this);
			header_control.Visible = false;
			Controls.AddImplicit (header_control);

			item_control = new ItemControl (this);
			Controls.AddImplicit (item_control);

			h_scroll = new HScrollBar ();
			Controls.AddImplicit (this.h_scroll);

			v_scroll = new VScrollBar ();
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

			this.SetStyle (ControlStyles.UserPaint | ControlStyles.StandardClick, false);
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

		bool multiselecting;

		bool CanMultiselect {
			get {
				if (multiselecting)
					return true;
				else if (multiselect && (XplatUI.State.ModifierKeys & (Keys.Control | Keys.Shift)) != 0)
					return true;
				else
					return false;
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
			get { return background_image; }
			set {
				if (value == background_image)
					return;

				background_image = value;
				OnBackgroundImageChanged (EventArgs.Empty);
			}
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
		[MonoTODO("Implement")]
		public bool ShowGroups {
			get {
				return false;
			}

			set {
			}
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
			get { return text; } 
			set {
				if (value == text)
					return;

				text = value;
				this.Redraw (true);

				OnTextChanged (EventArgs.Empty);
			}
		}

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
						if (Items[i].Bounds.X > ClientRectangle.Right)
							return i - 1;					
					} else {
						if (Items[i].Bounds.Y > ClientRectangle.Bottom)
							return i - 1;					
					}
				}
				
				return Items.Count - 1;
			}
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

		internal Size GetChildColumnSize (int index)
		{
			Size ret_size = Size.Empty;
			ColumnHeader col = this.columns [index];

			if (col.Width == -2) { // autosize = max(items, columnheader)
				Size size = Size.Ceiling (this.DeviceContext.MeasureString
							  (col.Text, this.Font));
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
				if (large_image_list != null)
					temp.Width += large_image_list.ImageSize.Width;
				if (temp.Width == 0)
					temp.Width = 43;
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

			if (h_scroll.Visible) {
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

			if (header_control.Visible)
				header_control.Width = ClientRectangle.Width;
			item_control.Width = ClientRectangle.Width;

			if (v_scroll.Visible) {
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
		
		ColumnHeader GetReorderedColumn (int index)
		{
			if (reordered_column_indices == null)
				return Columns [index];
			else
				return Columns [reordered_column_indices [index]];
		}

		void ReorderColumn (ColumnHeader col, int index)
		{
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

		int rows;
		int cols;
		ListViewItem[,] item_matrix;

		void LayoutIcons (bool large_icons, bool left_aligned, int x_spacing, int y_spacing)
		{
			header_control.Visible = false;
			header_control.Size = Size.Empty;
			item_control.Visible = true;
			item_control.Location = Point.Empty;

			if (items.Count == 0)
				return;

			Size sz = large_icons ? LargeIconItemSize : SmallIconItemSize;

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
				LayoutIcons (false, alignment == ListViewAlignment.Left, 4, 2);
				break;

			case View.LargeIcon:
				LayoutIcons (true, alignment == ListViewAlignment.Left,
					     ThemeEngine.Current.ListViewHorizontalSpacing,
					     ThemeEngine.Current.ListViewVerticalSpacing);
				break;

			case View.List:
				LayoutIcons (false, true, 4, 2);
				break;
			}

                        CalculateScrollBars ();
		}

		internal void UpdateSelection (ListViewItem item)
		{
			if (item.Selected) {

				if (!CanMultiselect && SelectedItems.Count > 0) {
					SelectedItems.Clear ();
				}

				if (!SelectedItems.list.Contains (item)) {
					SelectedItems.list.Add (item);
				}
			} else {
				SelectedItems.list.Remove (item);
			}
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
				if (key == Keys.Up)
					result = FocusedItem.Index - 1;
				else if (key == Keys.Down) {
					result = FocusedItem.Index + 1;
					if (result == items.Count)
						result = -1;
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
				while (item_matrix [row, col + 1] == null)
				       row--;	
				return item_matrix [row, col + 1].Index;

			case Keys.Up:
				if (row == 0)
					return -1;
				return item_matrix [row - 1, col].Index;

			case Keys.Down:
				if (row == (rows - 1) || row == Items.Count - 1)
					return -1;
				while (item_matrix [row + 1, col] == null)
				       col--;	
				return item_matrix [row + 1, col].Index;

			default:
				return -1;
			}
		}

		ListViewItem selection_start;

		private bool SelectItems (ArrayList sel_items)
		{
			bool changed = false;
			multiselecting = true;
			ArrayList curr_items = (ArrayList) SelectedItems.list.Clone ();
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
			multiselecting = false;
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
			} else if (!ctrl_pressed) {
				SelectedItems.Clear ();
				item.Selected = true;
				selection_start = item;
				OnSelectedIndexChanged (EventArgs.Empty);
			}
		}

		private void ListView_KeyDown (object sender, KeyEventArgs ke)
		{			
			if (ke.Handled || Items.Count == 0 || !item_control.Visible)
				return;

			int index = -1;
			ke.Handled = true;

			if (FocusedItem == null)
				SetFocusedItem (Items [0]);

			switch (ke.KeyCode) {

			case Keys.End:
				index = Items.Count - 1;
				break;

			case Keys.Home:			
				index = 0;
				break;

			case Keys.Left:
			case Keys.Right:
			case Keys.Up:				
			case Keys.Down:
				index = GetAdjustedIndex (ke.KeyCode);
				break;

			default:
				ke.Handled = KeySearchString (ke);
				return;
			}
			
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

				
		internal class ItemControl : Control {

			ListView owner;
			ListViewItem clicked_item;
			ListViewItem last_clicked_item;
			bool hover_processed = false;
			bool checking = false;

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
				if (owner.activation == ItemActivation.Standard && owner.ItemActivate != null)
					owner.ItemActivate (this, e);
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
				if (owner.items.Count == 0)
					return;

				Point pt = new Point (me.X, me.Y);
				foreach (ListViewItem item in owner.items) {
					if (me.Clicks == 1 && item.CheckRectReal.Contains (pt)) {
						checking = true;
						if (me.Clicks > 1)
							return;
						ToggleCheckState (item);
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
					if (owner.MultiSelect && (XplatUI.State.ModifierKeys & Keys.Control) == 0)
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
					} else if (me.Clicks == 1)
						owner.OnClick (EventArgs.Empty);
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
						prev_selection = (ArrayList) owner.SelectedItems.list.Clone ();
					} else if (owner.selected_indices.Count > 0) {
						owner.SelectedItems.Clear ();
						owner.OnSelectedIndexChanged (EventArgs.Empty);
					}
				}
			}

			private void ItemsMouseMove (object sender, MouseEventArgs me)
			{
				if (PerformBoxSelection (new Point (me.X, me.Y)))
					return;

				if (owner.HoverSelection && hover_processed) {

					Point pt = PointToClient (Control.MousePosition);
					ListViewItem item = owner.GetItemAt (pt.X, pt.Y);
					if (item == null || item.Selected)
				       		return;

					hover_processed = false;
					XplatUI.ResetMouseHover (Handle);
				}
			}


			private void ItemsMouseHover (object sender, EventArgs e)
			{
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
				if (owner.Items.Count == 0)
					return;

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
			}

			internal override void OnPaintInternal (PaintEventArgs pe)
			{
				ThemeEngine.Current.DrawListViewItems (pe.Graphics, pe.ClipRectangle, owner);
			}

			internal override void OnGotFocusInternal (EventArgs e)
			{
				owner.Focus ();
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
			if (AfterLabelEdit != null)
				AfterLabelEdit (this, e);
		}

		protected virtual void OnBeforeLabelEdit (LabelEditEventArgs e)
		{
			if (BeforeLabelEdit != null)
				BeforeLabelEdit (this, e);
		}

		protected virtual void OnColumnClick (ColumnClickEventArgs e)
		{
			if (ColumnClick != null)
				ColumnClick (this, e);
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
			if (ItemActivate != null)
				ItemActivate (this, e);
		}

		protected virtual void OnItemCheck (ItemCheckEventArgs ice)
		{
			if (ItemCheck != null)
				ItemCheck (this, ice);
		}

		protected virtual void OnItemDrag (ItemDragEventArgs e)
		{
			if (ItemDrag != null)
				ItemDrag (this, e);
		}

		protected virtual void OnSelectedIndexChanged (EventArgs e)
		{
			if (SelectedIndexChanged != null)
				SelectedIndexChanged (this, e);
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

		protected override void WndProc (ref Message m)
		{
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

			if (bounds.Left < 0)
				h_scroll.Value += bounds.Left;
			else if (bounds.Right > view_rect.Right)
				h_scroll.Value += (bounds.Right - view_rect.Right);

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
			
			items.list.Sort (item_sorter);
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
						drag_column.column_rect = clicked_column.Rect;
						drag_to_index = GetReorderedIndex (clicked_column);
					}
					clicked_column.pressed = true;
					Rectangle bounds = clicked_column.Rect;
					bounds.X -= owner.h_marker;
					Invalidate (bounds);
					return;
				}
			}

			private void HeaderMouseMove (object sender, MouseEventArgs me)
			{
				Point pt = new Point (me.X + owner.h_marker, me.Y);

				if (column_resize_active)  {
					resize_column.Width = pt.X - resize_column.X;
					if (resize_column.Width < 0)
						resize_column.Width = 0;
					return;
				}

				resize_column = null;

				if (clicked_column != null) {
					if (owner.AllowColumnReorder) {
						Rectangle r;

						r = drag_column.column_rect;
						r.X = clicked_column.Rect.X + me.X - drag_x;
						drag_column.column_rect = r;

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
						bool pressed = clicked_column.pressed;
						clicked_column.pressed = over == clicked_column;
						if (clicked_column.pressed ^ pressed) {
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
					column_resize_active = false;
					resize_column = null;
					Cursor = Cursors.Default;
					return;
				}

				if (clicked_column != null && clicked_column.pressed) {
					clicked_column.pressed = false;
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
				int [] indices = new int [Count];
				for (int i = 0; i < owner.CheckedItems.Count; i++) {
					ListViewItem item = owner.CheckedItems [i];
					indices [i] = item.Index;
				}
				return indices;
			}
		}	// CheckedIndexCollection

		public class CheckedListViewItemCollection : IList, ICollection, IEnumerable
		{
			internal readonly ArrayList list;
			private readonly ListView owner;

			#region Public Constructor
			public CheckedListViewItemCollection (ListView owner)
			{
				list = new ArrayList ();
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get {
					if (!owner.CheckBoxes)
						return 0;
					return list.Count;
				}
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public ListViewItem this [int index] {
				get {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("index");
					return (ListViewItem) list [index];
				}
			}

			bool ICollection.IsSynchronized {
				get { return list.IsSynchronized; }
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
				return list.Contains (item);
			}

			public void CopyTo (Array dest, int index)
			{
				if (!owner.CheckBoxes)
					return;
				list.CopyTo (dest, index);
			}

			public IEnumerator GetEnumerator ()
			{
				if (!owner.CheckBoxes)
					return (new ListViewItem [0]).GetEnumerator ();
				return list.GetEnumerator ();
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
				return list.IndexOf (item);
			}
			#endregion	// Public Methods
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
				value.owner = this.owner;
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

			public virtual void AddRange (ColumnHeader [] values)
			{
				foreach (ColumnHeader colHeader in values) {
					colHeader.owner = this.owner;
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

			public void Insert (int index, ColumnHeader value)
			{
				// LAMESPEC: MSDOCS say greater than or equal to the value of the Count property
				// but it's really only greater.
				if (index < 0 || index > list.Count)
					throw new ArgumentOutOfRangeException ("index");

				value.owner = this.owner;
				list.Insert (index, value);
				owner.Redraw (true);
			}

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
			internal ArrayList list;
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

					value.Owner = owner;
					list [displayIndex] = value;

					owner.Redraw (true);
				}
			}

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
				}
			}
			#endregion	// Public Properties

			#region Public Methods
			public virtual ListViewItem Add (ListViewItem value)
			{
				if (list.Contains (value))
					throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "value");

				value.Owner = owner;
				list.Add (value);

				owner.Sort (false);
				owner.Redraw (true);
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

			public void AddRange (ListViewItem [] values)
			{
				list.Clear ();
				owner.SelectedItems.list.Clear ();
				owner.CheckedItems.list.Clear ();

				foreach (ListViewItem item in values) {
					item.Owner = owner;
					list.Add (item);
				}

				owner.Sort (false);
				owner.Redraw (true);
			}

			public virtual void Clear ()
			{
				owner.SetFocusedItem (null);
				owner.h_scroll.Value = owner.v_scroll.Value = 0;
				list.Clear ();
				owner.SelectedItems.list.Clear ();
				owner.CheckedItems.list.Clear ();
				owner.Redraw (true);
			}

			public bool Contains (ListViewItem item)
			{
				return list.Contains (item);
			}

			public void CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
			}

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
				}
				else
					li = new ListViewItem (item.ToString ());

				li.Owner = owner;
				result = list.Add (li);
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

			public ListViewItem Insert (int index, ListViewItem item)
			{
				if (index < 0 || index > list.Count)
					throw new ArgumentOutOfRangeException ("index");

				if (list.Contains (item))
					throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "item");

				item.Owner = owner;
				list.Insert (index, item);
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

			public virtual void Remove (ListViewItem item)
			{
				if (!list.Contains (item))
					return;
	 				
				owner.SelectedItems.list.Remove (item);
				owner.CheckedItems.list.Remove (item);
				list.Remove (item);
				owner.Redraw (true);				
			}

			public virtual void RemoveAt (int index)
			{
				ListViewItem item = this [index];
				list.RemoveAt (index);
				owner.SelectedItems.list.Remove (item);
				owner.CheckedItems.list.Remove (item);
				owner.Redraw (false);
			}
			#endregion	// Public Methods

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
			#endregion	// Public Methods

			private int [] GetIndices ()
			{
				int [] indices = new int [Count];
				for (int i = 0; i < owner.SelectedItems.Count; i++) {
					ListViewItem item = owner.SelectedItems [i];
					indices [i] = item.Index;
				}
				return indices;
			}

		}	// SelectedIndexCollection

		public class SelectedListViewItemCollection : IList, ICollection, IEnumerable
		{
			internal ArrayList list;
			private readonly ListView owner;

			#region Public Constructor
			public SelectedListViewItemCollection (ListView owner)
			{
				list = new ArrayList ();
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get {
					if (!owner.IsHandleCreated)
						return 0;
					return list.Count;
				}
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public ListViewItem this [int index] {
				get {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("index");
					return (ListViewItem) list [index];
				}
			}

			bool ICollection.IsSynchronized {
				get { return list.IsSynchronized; }
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

				ArrayList copy = (ArrayList) list.Clone ();
				for (int i = 0; i < copy.Count; i++)
					((ListViewItem) copy [i]).Selected = false;

				list.Clear ();
			}

			public bool Contains (ListViewItem item)
			{
				if (!owner.IsHandleCreated)
					return false;
				return list.Contains (item);
			}

			public void CopyTo (Array dest, int index)
			{
				if (!owner.IsHandleCreated)
					return;
				list.CopyTo (dest, index);
			}

			public IEnumerator GetEnumerator ()
			{
				if (!owner.IsHandleCreated)
					return (new ListViewItem [0]).GetEnumerator ();
				return list.GetEnumerator ();
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
				return list.IndexOf (item);
			}
			#endregion	// Public Methods

		}	// SelectedListViewItemCollection

		#endregion // Subclasses
	}
}
