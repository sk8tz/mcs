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
//
// TODO:
//   - Item text editing
//   - Column resizing/reodering
//   - Feedback for item activation, change in cursor types as mouse moves.
//   - HideSelection
//   - LabelEdit
//   - Manual column resizing
//   - Drag and drop


// NOT COMPLETE


using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Runtime.InteropServices;

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
		private ColumnHeader clicked_column;
		private ListViewItem clicked_item;
		private ListViewItem last_clicked_item;
		private ColumnHeaderCollection columns;
		private bool ctrl_pressed;
		private bool shift_pressed;
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
		internal ScrollBar h_scroll; // used for scrolling horizontally
		internal ScrollBar v_scroll; // used for scrolling vertically
		internal int h_marker;		// Position markers for scrolling
		internal int v_marker;
		internal Rectangle client_area; // ClientRectangle - scrollbars

		// internal variables
		internal ImageList large_image_list;
		internal ImageList small_image_list;
		internal Size text_size = Size.Empty;

		#region Events
		public event LabelEditEventHandler AfterLabelEdit;

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged;

		public event LabelEditEventHandler BeforeLabelEdit;
		public event ColumnClickEventHandler ColumnClick;
		public event EventHandler ItemActivate;
		public event ItemCheckEventHandler ItemCheck;
		public event ItemDragEventHandler ItemDrag;

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint;

		public event EventHandler SelectedIndexChanged;

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged;
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

			// we are mostly scrollable
			h_scroll = new HScrollBar ();
			v_scroll = new VScrollBar ();
			h_marker = v_marker = 0;

			// scroll bars are disabled initially
			h_scroll.Visible = false;
			h_scroll.ValueChanged += new EventHandler(HorizontalScroller);
			v_scroll.Visible = false;
			v_scroll.ValueChanged += new EventHandler(VerticalScroller);

			// event handlers
			base.DoubleClick += new EventHandler(ListView_DoubleClick);
			base.KeyDown += new KeyEventHandler(ListView_KeyDown);
			base.KeyUp += new KeyEventHandler(ListView_KeyUp);
			base.MouseDown += new MouseEventHandler(ListView_MouseDown);
			base.MouseHover += new EventHandler(ListView_MouseHover);
			base.MouseUp += new MouseEventHandler(ListView_MouseUp);
			base.MouseMove += new MouseEventHandler(ListView_MouseMove);
			base.Paint += new PaintEventHandler (ListView_Paint);
			SizeChanged += new EventHandler (ListView_SizeChanged);

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

		internal bool CanMultiselect {
			get {
				if (this.multiselect &&
					(this.ctrl_pressed || this.shift_pressed))
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
			set {
				if (this.allow_column_reorder != value) {
					allow_column_reorder = value;
					// column reorder does not matter in Details view
					if (this.view != View.Details)
						this.Redraw (true);
				}
			}
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
				if (BackgroundImageChanged != null)
					BackgroundImageChanged (this, new EventArgs ());
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
			get { return focused_item; }
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
				if (value != ColumnHeaderStyle.Clickable && value != ColumnHeaderStyle.Nonclickable  && 
					value != ColumnHeaderStyle.None) {
					throw new InvalidEnumArgumentException (string.Format 
						("Enum argument value '{0}' is not valid for ColumnHeaderStyle", value));
				}
				
				if (header_style != value) {
					header_style = value;
					// header style matters only in Details view
					if (this.view == View.Details)
						this.Redraw (false);
				}
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
			get { return item_sorter; }
			set { item_sorter = value; }
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
				if (value != SortOrder.Ascending && value != SortOrder.Descending  && 
					value != SortOrder.None) {
					throw new InvalidEnumArgumentException (string.Format
						("Enum argument value '{0}' is not valid for Sorting", value));
				}
				
				if (sort_order != value)  {			
					sort_order = value; 
					this.Redraw (false);
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

				if (TextChanged != null)
					TextChanged (this, new EventArgs ());
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
						if (item.EntireRect.X >= h_marker && item.EntireRect.Y >= v_marker)
							return item;
					}
					return null;
				}
			}
		}

		[DefaultValue (View.LargeIcon)]
		public View View {
			get { return view; }
			set { 
				if (value != View.Details && value != View.LargeIcon  && 
					value != View.List  && value != View.SmallIcon  ) {
					throw new InvalidEnumArgumentException (string.Format
						("Enum argument value '{0}' is not valid for View", value));
				}
				
				if (view != value) {
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
					if (item.EntireRect.X + item.EntireRect.Width >= h_marker 
						&& item.EntireRect.Y + item.EntireRect.Height >= v_marker)
						return item.Index;
				}
				return 0;

			}
		}

		
		internal int LastItemIndex {			
			get {							
				for (int i = FirstVisibleIndex; i < Items.Count; i++) {						
					if (Items[i].EntireRect.Y > v_marker + ClientRectangle.Bottom)						
							return i -1;					
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

		private void CalculateScrollBars ()
		{
			client_area = ClientRectangle;
			
			if (!this.scrollable || this.items.Count <= 0) {
				h_scroll.Visible = false;
				v_scroll.Visible = false;
				return;
			}

			// making a scroll bar visible might make
			// other scroll bar visible			
			if (layout_wd > client_area.Right) {
				h_scroll.Visible = true;
				if ((layout_ht + h_scroll.Height) > client_area.Bottom) {
					v_scroll.Visible = true;					
				}
			} else if (layout_ht > client_area.Bottom) {				
				v_scroll.Visible = true;
				if ((layout_wd + v_scroll.Width) > client_area.Right)
					h_scroll.Visible = true;
			}

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
				client_area.Height -= h_scroll.Height;
			}

			// vertical scrollbar
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
				client_area.Width -= v_scroll.Width;
			}
		}
		
		
		// Sets the location of every item on
		// the ListView as per the view
		private void CalculateListView (ListViewAlignment align)
		{
			int current_pos_x = 0; // our x-position marker
			int current_pos_y = 0; // our y-position marker
			int item_ht;
			int item_wd;
			int max; 	 // max x_pos or y_pos depending on the alignment
			int current = 0; // current row or column
			int vertical_spacing = ThemeEngine.Current.ListViewVerticalSpacing;
			int horizontal_spacing = ThemeEngine.Current.ListViewHorizontalSpacing;

			CalcTextSize ();

			switch (view) {

			case View.Details:
				// ColumnHeaders are not drawn if headerstyle is none
				int ht = (this.header_style == ColumnHeaderStyle.None) ? 
					0 : this.Font.Height + 3;
				
				if (columns.Count > 0) {
					foreach (ColumnHeader col in columns) {
						col.X = current_pos_x;
						col.Y = current_pos_y;
						col.CalcColumnHeader ();
						current_pos_x += col.Wd;
					}
					this.layout_wd = current_pos_x;
				}
				// set the position marker for placing items
				// vertically down
				current_pos_y = ht;

				if (items.Count > 0) {
					foreach (ListViewItem item in items) {
						item.location.X = 0;
						item.location.Y = current_pos_y;
						item.CalcListViewItem ();
						current_pos_y += item.EntireRect.Height;
					}
					this.layout_ht = current_pos_y;

					// some space for bottom gridline
					if (this.grid_lines)
						this.layout_ht += 2;
				}
				break;

			case View.SmallIcon:
				vertical_spacing = 0;
				horizontal_spacing = 0;
				goto case View.LargeIcon;

			case View.LargeIcon:
				if (items.Count > 0) {
					items [0].CalcListViewItem ();
					item_ht = items [0].EntireRect.Height;
					item_wd = items [0].EntireRect.Width;

					// top (default) and snaptogrid alignments are handled same way
					if (align == ListViewAlignment.Left) {
						max = client_area.Height;
						foreach (ListViewItem item in items) {
							item.location.X = current_pos_x +
								horizontal_spacing;
							item.location.Y = 0;
							item.CalcListViewItem ();
							current_pos_y += item_ht;

							current ++; // just to know about the last element
							// we just did the last item
							if (current == items.Count) {
								if (max < current_pos_y)
									max = current_pos_y;
								current_pos_x = item.EntireRect.Right;
								break;
							}
							else {
								// is there enough space for another row ?
								if ((current_pos_y + vertical_spacing
								     + item_ht) <= client_area.Height)
									current_pos_y += vertical_spacing;
								else {
									// start another column
									// make current_pos_y as the
									// max value and reset
									// current_pos_y value.
									max = current_pos_y;
									current_pos_x += item_wd;
									current_pos_y = 0;
								}
							}
						}
						// adjust the layout dimensions
						this.layout_ht = max;
						this.layout_wd = current_pos_x;
					}
					else { // other default/top alignment
						max = client_area.Width;
						foreach (ListViewItem item in items) {
							item.location.X = current_pos_x +
								horizontal_spacing;

							item.location.Y = current_pos_y;
							item.CalcListViewItem ();
							current_pos_x += item_wd;

							current ++; // just to know about the last element
							// we just did the last item
							if (current == items.Count) {
								if (max < current_pos_x)
									max = current_pos_x;
								current_pos_y = item.EntireRect.Bottom;
								break;
							}
							else {
								// is there enough space for another column?
								if ((current_pos_x + horizontal_spacing
								     + item_wd) <= client_area.Width)
									continue;
								else {
									// start another row
									// make current_pos_x as the
									// max value and reset
									// current_pos_x value.
									max = current_pos_x;
									current_pos_y += (item_ht +
											  vertical_spacing);
									current_pos_x = 0;
								}
							}
						}
						// adjust the layout dimensions
						this.layout_wd = max;
						this.layout_ht = current_pos_y;
					}
				}
				break;

			case View.List:
				if (items.Count > 0) {
					items [0].CalcListViewItem ();
					item_ht = items [0].EntireRect.Height;
					item_wd = items [0].EntireRect.Width;

					max = client_area.Height / item_ht;
					if (max == 0)
						max = 1; // we draw at least one row

					foreach (ListViewItem item in items) {
						item.location.X = current_pos_x;
						item.location.Y = current_pos_y;
						item.CalcListViewItem ();
						current ++;
						if (current == max) {
							current_pos_x += item_wd;
							current_pos_y = 0;
							current = 0;
						}
						else
							current_pos_y += item_ht;
					}

					// adjust the layout dimensions
					this.layout_ht = max * item_ht;
					if (current == 0) // we have fully filled layout
						this.layout_wd = current_pos_x;
					else
						this.layout_wd = current_pos_x + item_wd;
				}
				break;
			}

                        CalculateScrollBars ();
                        
		}
				
		// Event Handlers
		private void ListView_DoubleClick (object sender, EventArgs e)
		{
			if (this.activation == ItemActivation.Standard
			    && this.ItemActivate != null)
				this.ItemActivate (this, e);
		}

		private void ListView_KeyDown (object sender, KeyEventArgs ke)
		{			
			int index = -1;
			if (ke.Handled || Items.Count == 0)
				return;

			ke.Handled = true;

			switch (ke.KeyCode) {

			case Keys.ControlKey:
				ctrl_pressed = true;
				break;

			case Keys.Down:
				if (focused_item != null && focused_item.Index + 1 < Items.Count) {
					index = focused_item.Index + 1;
				}
				break;

			case Keys.End:
				index = Items.Count - 1;
				break;

			case Keys.Home:			
				index = 0;
				break;

			case Keys.Left:
				index = -1;
				if (focused_item != null)
					index = focused_item.Index;
				else
					break;

				if (index > 0)
					index -= 1;
									
				break;

			case Keys.Right:
				if (focused_item != null)
					index = focused_item.Index + 1;
				else
					index = 1;

				if (index == items.Count)
					index = -1;

				break;

			case Keys.ShiftKey:
				shift_pressed = true;
				break;

			case Keys.Up:				
				if (focused_item != null)
					index = focused_item.Index;
				else
					break;

				if (index > 0)
					index--;

				if (index < 0) {
					index = -1;
				}
				break;

			default:
				ke.Handled = false;
				break;
			}
			
			if (index != -1) {
				items [index].Selected = true;
				SetFocusedItem (items [index]);				
				EnsureVisible (index);
			}
		}

		private void ListView_KeyUp (object sender, KeyEventArgs ke)
		{
			if (!ke.Handled) {
				if (ke.KeyCode == Keys.ControlKey)
					this.ctrl_pressed = false;

				if (ke.KeyCode == Keys.ShiftKey)
					this.shift_pressed = false;
				ke.Handled = true;
			}
		}

		private void ListView_MouseDown (object sender, MouseEventArgs me)
		{
			if (items.Count == 0)
				return;

			Point hit = Point.Empty;
			if (this.HeaderStyle != ColumnHeaderStyle.None) {
				// take horizontal scrolling into account
				hit = new Point (me.X + h_marker, me.Y);

				// hit test on columns
				if (this.view == View.Details && this.columns.Count > 0) {
					foreach (ColumnHeader col in this.columns) {
						if (col.Rect.Contains (hit)) {
							this.clicked_column = col;
							this.Capture = true;
							break;
						}
					}

					if (this.clicked_column != null) {
						this.clicked_column.pressed = true;
						this.Redraw (false);
						return;
					}
				}
			}

			// hit test on items
			// we need to take scrolling into account
			hit = new Point (me.X + h_marker, me.Y + v_marker);
			foreach (ListViewItem item in this.items) {
				if (item.CheckRect.Contains (hit)) {
					CheckState curr_state = item.Checked ?
						CheckState.Checked : CheckState.Unchecked;
					if (item.Checked)
						item.Checked = false;
					else
						item.Checked = true;

					CheckState new_state = item.Checked ?
						CheckState.Checked : CheckState.Unchecked;
					this.Redraw (false);

					// Raise the ItemCheck event
					ItemCheckEventArgs ice = new ItemCheckEventArgs (item.Index,
											 curr_state,
											 new_state);
					this.OnItemCheck (ice);
					break;
				}

				if (this.view == View.Details &&
				    this.FullRowSelect == false) {
					if (item.LabelRect.Contains (hit)) {
						this.clicked_item = item;
						break;
					}
				}
				else {
					if (item.EntireRect.Contains (hit)) {
						this.clicked_item = item;
						break;
					}
				}
			}

			// set the FocusedItem to be the current clicked_item
			SetFocusedItem (clicked_item);

			if (clicked_item != null) {
				bool changed = !clicked_item.Selected;
				clicked_item.Selected = true;
				
				// Only Raise the event if the selected item has changed
				if (changed)
					OnSelectedIndexChanged (EventArgs.Empty);

				// Raise double click if the item was clicked. On MS the
				// double click is only raised if you double click an item
				if (me.Clicks > 1 && this.clicked_item != null)
					OnDoubleClick (EventArgs.Empty);

				this.Redraw (false);
			} else if (selected_indices.Count > 0) {
				// NOTE: selected_indices isn't computed properly so
				// this doesn't actually work
				
				// Raise the event if there was at least one item
				// selected and the user click on a dead area (unselecting all)
				OnSelectedIndexChanged (EventArgs.Empty);
				Redraw (false);
			}
		}

		private void ListView_MouseHover (object sender, EventArgs e)
		{
			// handle the hover events only when the mouse
			// is not captured.
			if (this.hover_selection == false || this.Capture)
				return;

			// hit test for the items
			Point hit = this.PointToClient (Control.MousePosition);
			ListViewItem item = this.GetItemAt (hit.X, hit.Y);

			if (item != null) {
				item.Selected = true;
				// Raise the event
				this.OnSelectedIndexChanged (new EventArgs ());

				this.Redraw (false);
			}
		}

		private void ListView_MouseMove (object sender, MouseEventArgs me)
		{
			// Column header is always at the top. It can
			// scroll only horizontally. So, we have to take
			// only horizontal scrolling into account
			Point hit = new Point (me.X + h_marker, me.Y);

			// non-null clicked_col means mouse down has happened
			// on a column
			if (this.clicked_column != null) {
				if (this.clicked_column.pressed == false &&
				    this.clicked_column.Rect.Contains (hit)) {
					this.clicked_column.pressed = true;
					this.Redraw (false);
				}
				else if (this.clicked_column.pressed && 
					 ! this.clicked_column.Rect.Contains (hit)) {
					this.clicked_column.pressed = false;
					this.Redraw (false);
				}
			}
		}

		private void ListView_MouseUp (object sender, MouseEventArgs me)
		{
			this.Capture = false;
			if (items.Count == 0)
				return;

			Point hit = new Point (me.X, me.Y);

			if (this.clicked_column != null) {
				if (this.clicked_column.pressed) {
					this.clicked_column.pressed = false;
					this.Redraw (false);

					// Raise the ColumnClick event
					this.OnColumnClick (new ColumnClickEventArgs
							    (this.clicked_column.Index));
				}
			}

			// Raise the ItemActivate event
			Rectangle rect = Rectangle.Empty;
			if (this.clicked_item != null) {
				if (this.view == View.Details && !this.full_row_select)
					rect = this.clicked_item.LabelRect;
				else
					rect = this.clicked_item.EntireRect;

				// We handle double click in a separate handler
				if (this.activation != ItemActivation.Standard &&
				    rect.Contains (hit)) {
					if (this.activation == ItemActivation.OneClick)
						this.ItemActivate (this, EventArgs.Empty);

					// ItemActivate is raised on the second click on the same item
					else if (this.activation == ItemActivation.TwoClick) {
						if (this.last_clicked_item == this.clicked_item) {
							this.ItemActivate (this, EventArgs.Empty);
							this.last_clicked_item = null;
						}
						else
							this.last_clicked_item = this.clicked_item;
					}
				}
			}

			this.clicked_column = null;
			this.clicked_item = null;
		}

		private void ListView_Paint (object sender, PaintEventArgs pe)
		{
			if (this.Width <= 0 || this.Height <=  0 ||
			    this.Visible == false || this.updating == true)
				return;	
				
			CalculateScrollBars ();

			ThemeEngine.Current.DrawListView (pe.Graphics,
					pe.ClipRectangle, this);
					
			// Raise the Paint event
			if (Paint != null)
				Paint (this, pe);
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
				Rectangle area = client_area;
				
				if (View == View.Details && Columns.Count > 0) {
					area.Y += Columns[0].Ht;
					area.Height -= Columns[0].Ht;
				}
				
				h_marker = h_scroll.Value;
				XplatUI.ScrollWindow (Handle, area, pixels, 0, false);
			}
		}

		private void VerticalScroller (object sender, EventArgs e)
		{
			// Avoid unnecessary flickering, when button is
			// kept pressed at the end
			if (v_marker != v_scroll.Value) {
				int pixels =  v_marker - v_scroll.Value;
				Rectangle area = client_area;
				
				if (View == View.Details && Columns.Count > 0) {
					area.Y += Columns[0].Ht;
					area.Height -= Columns[0].Ht;
				}
				
				v_marker = v_scroll.Value;
				XplatUI.ScrollWindow (Handle, area, 0, pixels, false);
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
				
				if (large_image_list != null)
					large_image_list.Dispose ();
					
				if (small_image_list != null)
					small_image_list.Dispose ();
					
				if (state_image_list != null)
					state_image_list.Dispose ();			
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
			SuspendLayout ();
			Controls.AddImplicit (this.v_scroll);
			Controls.AddImplicit (this.h_scroll);
			ResumeLayout ();
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
			SetFocusedItem (null);
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
			if (index < 0 || index >= this.items.Count || this.scrollable == false)
				return;

			// dimensions of visible area
			int view_wd = client_area.Width;
			int view_ht = client_area.Height;
			// visible area is decided by the h_marker and v_marker
			Rectangle view_rect = new Rectangle (h_marker, v_marker, view_wd, view_ht);
			
			// an item's bounding rect
			Rectangle rect = this.items [index].EntireRect;

			// we don't need to do anything if item is visible.
			// visible area is represented by (0,0,view_wd,view_ht)
			if (view_rect.Contains (rect))
				return;

			// Scroll Left or Up
			if ((rect.Left < view_rect.Left) || (rect.Top < view_rect.Top)) {
				if (rect.Left < view_rect.Left)
					this.h_scroll.Value -= (view_rect.Left - rect.Left);
				if (rect.Top < view_rect.Top)
					this.v_scroll.Value -= (view_rect.Top - rect.Top);
			}
			// Scroll Right or Down
			else {
				if (rect.Right > view_rect.Right)
					this.h_scroll.Value += (rect.Right - view_rect.Right);
				if (rect.Bottom > view_rect.Bottom)
					this.v_scroll.Value += (rect.Bottom - view_rect.Bottom);
			}
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
				throw new IndexOutOfRangeException ("Invalid Index");

			return items [index].GetBounds (portion);
		}

		public void Sort ()
		{
			if (sort_order != SortOrder.None)
				items.list.Sort (item_sorter);

			if (sort_order == SortOrder.Descending)
				items.list.Reverse ();

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
		public class CheckedIndexCollection : IList, ICollection, IEnumerable
		{
			internal ArrayList list;
			private ListView owner;

			#region Public Constructor
			public CheckedIndexCollection (ListView owner)
			{
				list = new ArrayList ();
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public virtual int Count {
				get { return list.Count; }
			}

			public virtual bool IsReadOnly {
				get { return true; }
			}

			public int this [int index] {
				get {
					if (index < 0 || index >= list.Count)
						throw new ArgumentOutOfRangeException ("Index out of range.");
					return (int) list [index];
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
				return list.Contains (checkedIndex);
			}

			public virtual IEnumerator GetEnumerator ()
			{
				return list.GetEnumerator ();
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
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
				return list.Contains (checkedIndex);
			}

			int IList.IndexOf (object checkedIndex)
			{
				return list.IndexOf (checkedIndex);
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
				return list.IndexOf (checkedIndex);
			}
			#endregion	// Public Methods

		}	// CheckedIndexCollection

		public class CheckedListViewItemCollection : IList, ICollection, IEnumerable
		{
			internal ArrayList list;
			private ListView owner;

			#region Public Constructor
			public CheckedListViewItemCollection (ListView owner)
			{
				list = new ArrayList ();
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public virtual int Count {
				get { return list.Count; }
			}

			public virtual bool IsReadOnly {
				get { return true; }
			}

			public ListViewItem this [int index] {
				get {
					if (index < 0 || index >= list.Count)
						throw new ArgumentOutOfRangeException ("Index out of range.");
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
				return list.Contains (item);
			}

			public virtual void CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
			}

			public virtual IEnumerator GetEnumerator ()
			{
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
				return list.Contains (item);
			}

			int IList.IndexOf (object item)
			{
				return list.IndexOf (item);
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
			public virtual int Count {
				get { return list.Count; }
			}

			public virtual bool IsReadOnly {
				get { return false; }
			}

			public virtual ColumnHeader this [int index] {
				get {
					if (index < 0 || index >= list.Count)
						throw new ArgumentOutOfRangeException ("Index out of range.");
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
				owner.Redraw (true); 
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

			public virtual IEnumerator GetEnumerator ()
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
					throw new ArgumentOutOfRangeException ("Index out of range.");

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
					throw new ArgumentOutOfRangeException ("Index out of range.");

				// TODO: Update Column internal index ?
				list.RemoveAt (index);
				owner.Redraw (true);
			}
			#endregion	// Public Methods
			

		}	// ColumnHeaderCollection

		public class ListViewItemCollection : IList, ICollection, IEnumerable
		{
			internal ArrayList list;
			private ListView owner;

			#region Public Constructor
			public ListViewItemCollection (ListView owner)
			{
				list = new ArrayList ();
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public virtual int Count {
				get { return list.Count; }
			}

			public virtual bool IsReadOnly {
				get { return false; }
			}

			public virtual ListViewItem this [int displayIndex] {
				get {
					if (displayIndex < 0 || displayIndex >= list.Count)
						throw new ArgumentOutOfRangeException ("Index out of range.");
					return (ListViewItem) list [displayIndex];
				}

				set {
					if (displayIndex < 0 || displayIndex >= list.Count)
						throw new ArgumentOutOfRangeException ("Index out of range.");

					if (list.Contains (value))
						throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "value");

					value.owner = this.owner;
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

				value.owner = this.owner;
				list.Add (value);

				if (owner.Sorting != SortOrder.None)
					owner.Sort ();

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
				owner.SelectedIndices.list.Clear ();
				owner.CheckedItems.list.Clear ();
				owner.CheckedIndices.list.Clear ();

				foreach (ListViewItem item in values) {
					item.owner = this.owner;
					list.Add (item);
				}

				if (owner.Sorting != SortOrder.None)
					owner.Sort ();

				owner.Redraw (true);
			}

			public virtual void Clear ()
			{
				list.Clear ();
				owner.SelectedItems.list.Clear ();
				owner.SelectedIndices.list.Clear ();
				owner.CheckedItems.list.Clear ();
				owner.CheckedIndices.list.Clear ();
				owner.Redraw (true);
			}

			public bool Contains (ListViewItem item)
			{
				return list.Contains (item);
			}

			public virtual void CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
			}

			public virtual IEnumerator GetEnumerator ()
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

				li.owner = this.owner;
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
				// LAMESPEC: MSDOCS say greater than or equal to the value of the Count property
				// but it's really only greater.
				if (index < 0 || index > list.Count)
					throw new ArgumentOutOfRangeException ("Index out of range.");

				if (list.Contains (item))
					throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "item");

				item.owner = this.owner;
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
				owner.SelectedIndices.list.Remove (item.Index);
				owner.CheckedItems.list.Remove (item);
				owner.CheckedIndices.list.Remove (item.Index);
				list.Remove (item);
				owner.Redraw (true);				
			}

			public virtual void RemoveAt (int index)
			{
				if (index < 0 || index >= list.Count)
					throw new ArgumentOutOfRangeException ("Index out of range.");

				list.RemoveAt (index);
				owner.SelectedItems.list.RemoveAt (index);
				owner.SelectedIndices.list.RemoveAt (index);
				owner.CheckedItems.list.RemoveAt (index);
				owner.CheckedIndices.list.RemoveAt (index);
				owner.Redraw (false);
			}
			#endregion	// Public Methods

		}	// ListViewItemCollection

		public class SelectedIndexCollection : IList, ICollection, IEnumerable
		{
			internal ArrayList list;
			private ListView owner;

			#region Public Constructor
			public SelectedIndexCollection (ListView owner)
			{
				list = new ArrayList ();
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public virtual int Count {
				get { return list.Count; }
			}

			public virtual bool IsReadOnly {
				get { return true; }
			}

			public int this [int index] {
				get {
					if (index < 0 || index >= list.Count)
						throw new ArgumentOutOfRangeException ("Index out of range.");
					return (int) list [index];
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
			public bool Contains (int selectedIndex)
			{
				return list.Contains (selectedIndex);
			}

			public virtual void CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
			}

			public virtual IEnumerator GetEnumerator ()
			{
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

			bool IList.Contains (object selectedIndex)
			{
				return list.Contains (selectedIndex);
			}

			int IList.IndexOf (object selectedIndex)
			{
				return list.IndexOf (selectedIndex);
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
				return list.IndexOf (selectedIndex);
			}
			#endregion	// Public Methods

		}	// SelectedIndexCollection

		public class SelectedListViewItemCollection : IList, ICollection, IEnumerable
		{
			internal ArrayList list;
			private ListView owner;

			#region Public Constructor
			public SelectedListViewItemCollection (ListView owner)
			{
				list = new ArrayList ();
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public virtual int Count {
				get { return list.Count; }
			}

			public virtual bool IsReadOnly {
				get { return true; }
			}

			public ListViewItem this [int index] {
				get {
					if (index < 0 || index >= list.Count)
						throw new ArgumentOutOfRangeException ("Index out of range.");
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
			public virtual void Clear ()
			{
				// mark the items as unselected before clearing the list
				for (int i = 0; i < list.Count; i++)
					((ListViewItem) list [i]).selected = false;

				list.Clear ();
				
				if (owner != null)
					owner.Invalidate ();
			}

			public bool Contains (ListViewItem item)
			{
				return list.Contains (item);
			}

			public virtual void CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
			}

			public virtual IEnumerator GetEnumerator ()
			{
				return list.GetEnumerator ();
			}

			int IList.Add (object value)
			{
				throw new NotSupportedException ("Add operation is not supported.");
			}

			bool IList.Contains (object item)
			{
				return list.Contains (item);
			}

			int IList.IndexOf (object item)
			{
				return list.IndexOf (item);
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
				return list.IndexOf (item);
			}
			#endregion	// Public Methods

		}	// SelectedListViewItemCollection

		#endregion // Subclasses
	}
}
