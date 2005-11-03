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
//	Jackson Harper (jackson@ximian.com)
//	Kazuki Oikawa (kazuki@panicode.com)

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[DefaultProperty("Nodes")]
	[DefaultEvent("AfterSelect")]
	[Designer("System.Windows.Forms.Design.TreeViewDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class TreeView : Control {
		#region Fields
		private string path_separator = "\\";
		private int item_height = -1;
		private bool sorted;
		private TreeNode top_node;
		internal TreeNode root_node;
		private TreeNodeCollection nodes;
		private int total_node_count;

		private TreeNode selected_node = null;
		private TreeNode focused_node = null;
		private bool select_mmove = false;

		private ImageList image_list;
		private int image_index = -1;
		private int selected_image_index = -1;

		private bool full_row_select;
		private bool hot_tracking;
		private int indent = 19;

		private TextBox edit_text_box;
		private TreeNode edit_node;
		
		private bool checkboxes;
		private bool label_edit;
		private bool scrollable;
		private bool show_lines = true;
		private bool show_root_lines = true;
		private bool show_plus_minus = true;
		private bool hide_selection = true;

		private bool add_hscroll;
		private bool add_vscroll;
		private int max_node_width;
		private VScrollBar vbar;
		private int skipped_nodes;
		private HScrollBar hbar;
		private int hbar_offset;
		private int used_height;
		private bool update_node_bounds;
		
		private int update_stack;
		private bool update_needed;
		
		private TreeViewEventHandler on_after_check;
		private TreeViewEventHandler on_after_collapse;
		private TreeViewEventHandler on_after_expand;
		private NodeLabelEditEventHandler on_after_label_edit;
		private TreeViewEventHandler on_after_select;
		private TreeViewCancelEventHandler on_before_check;
		private TreeViewCancelEventHandler on_before_collapse;
		private TreeViewCancelEventHandler on_before_expand;
		private NodeLabelEditEventHandler on_before_label_edit;
		private TreeViewCancelEventHandler on_before_select;

		private Pen dash;
		private StringFormat string_format;
		private int open_node_count = -1;

		private int drag_begin_x = 0;
		private int drag_begin_y = 0;
		private long handle_count = 1;

		#endregion	// Fields

		#region Public Constructors	
		public TreeView ()
		{
			base.background_color = ThemeEngine.Current.ColorWindow;
			base.foreground_color = ThemeEngine.Current.ColorWindowText;

			root_node = new TreeNode (this);
			root_node.Text = "ROOT NODE";
			nodes = new TreeNodeCollection (root_node);
			root_node.SetNodes (nodes);

			MouseDown += new MouseEventHandler (MouseDownHandler);
			MouseUp += new MouseEventHandler(MouseUpHandler);
			MouseMove += new MouseEventHandler(MouseMoveHandler);
			SizeChanged += new EventHandler (SizeChangedHandler);
			FontChanged += new EventHandler (FontChangedHandler);
			LostFocus += new EventHandler (LostFocusHandler);

			SetStyle (ControlStyles.UserPaint | ControlStyles.StandardClick, false);

			dash = new Pen (SystemColors.ControlLight, 1);
			dash.DashStyle = DashStyle.Dash;

			string_format = new StringFormat ();
			string_format.LineAlignment = StringAlignment.Center;
			string_format.Alignment = StringAlignment.Center;
		}

		#endregion	// Public Constructors

		#region Public Instance Properties
		public override Color BackColor {
			get { return base.BackColor;}
			set { base.BackColor = value; }
		}


		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}

		[DefaultValue(BorderStyle.Fixed3D)]
		[DispId(-504)]
		public BorderStyle BorderStyle {
			get { return InternalBorderStyle; }
			set { InternalBorderStyle  = value; }
		}

		[DefaultValue(false)]
		public bool CheckBoxes {
			get { return checkboxes; }
			set {
				if (value == checkboxes)
					return;
				checkboxes = value;

				// Match a "bug" in the MS implementation where disabling checkboxes
				// collapses the entire tree, but enabling them does not affect the
				// state of the tree.
				if (!checkboxes)
					root_node.CollapseAllUncheck ();

				Refresh ();
			}
		}

		public override Color ForeColor {
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}
		[DefaultValue(false)]
		public bool FullRowSelect {
			get { return full_row_select; }
			set {
				if (value == full_row_select)
					return;
				full_row_select = value;
				Refresh ();
			}
		}
		[DefaultValue(true)]
		public bool HideSelection {
			get { return hide_selection; }
			set {
				if (hide_selection == value)
					return;
				hide_selection = value;
				this.Refresh ();
			}
		}

		[DefaultValue(false)]
		public bool HotTracking {
			get { return hot_tracking; }
			set { hot_tracking = value; }
		}

		[DefaultValue(0)]
		[Editor("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[Localizable(true)]
		[TypeConverter(typeof(TreeViewImageIndexConverter))]
		public int ImageIndex {
			get { return image_index; }
			set {
				if (value < -1) {
					throw new ArgumentException ("'" + value + "' is not a valid value for 'value'. " +
						"'value' must be greater than or equal to 0.");
				}
				if (image_index == value)
					return;
				image_index = value;
				Refresh ();
			}
		}

		[DefaultValue(null)]
		public ImageList ImageList {
			get { return image_list; }
			set {
				image_list = value;
				Refresh ();
			}
		}

		[Localizable(true)]
		public int Indent {
			get { return indent; }
			set {
				if (indent == value)
					return;
				if (value > 32000) {
					throw new ArgumentException ("'" + value + "' is not a valid value for 'Indent'. " +
						"'Indent' must be less than or equal to 32000");
				}	
				if (value < 0) {
					throw new ArgumentException ("'" + value + "' is not a valid value for 'Indent'. " +
						"'Indent' must be greater than or equal to 0.");
				}
				indent = value;
				Refresh ();
			}
		}

		[Localizable(true)]
		public int ItemHeight {
			get {
				if (item_height == -1)
					return FontHeight + 3;
				return item_height;
			}
			set {
				if (value == item_height)
					return;
				item_height = value;
				Refresh ();
			}
		}

		[DefaultValue(false)]
		public bool LabelEdit {
			get { return label_edit; }
			set { label_edit = value; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[MergableProperty(false)]
		[Localizable(true)]
		public TreeNodeCollection Nodes {
			get { return nodes; }
		}

		[DefaultValue("\\")]
		public string PathSeparator {
			get { return path_separator; }
			set { path_separator = value; }
		}

		[DefaultValue(true)]
		public bool Scrollable {
			get { return scrollable; }
			set {
				if (scrollable == value)
					return;
				scrollable = value;
			}
		}

		[Editor("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[TypeConverter(typeof(TreeViewImageIndexConverter))]
		[Localizable(true)]
		[DefaultValue(0)]
		public int SelectedImageIndex {
			get { return selected_image_index; }
			set {
				if (value < -1) {
					throw new ArgumentException ("'" + value + "' is not a valid value for 'value'. " +
						"'value' must be greater than or equal to 0.");
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public TreeNode SelectedNode {
			get { return selected_node; }
			set {
				if (selected_node == value)
					return;

				TreeViewCancelEventArgs e = new TreeViewCancelEventArgs (value, false, TreeViewAction.Unknown);
				OnBeforeSelect (e);

				if (e.Cancel)
					return;

				Rectangle invalid = Rectangle.Empty;

				if (selected_node != null) {
					invalid = Bloat (selected_node.Bounds);
				}
				if (focused_node != null) {
					invalid = Rectangle.Union (invalid,
							Bloat (focused_node.Bounds));
				}

				invalid = Rectangle.Union (invalid, Bloat (value.Bounds));

				selected_node = value;
				focused_node = value;

				if (full_row_select) {
					invalid.X = 0;
					invalid.Width = ViewportRectangle.Width;
				}

				Invalidate (invalid);

				// We ensure its visible after we update because
				// scrolling is used for insure visible
				selected_node.EnsureVisible ();

				OnAfterSelect (new TreeViewEventArgs (value, TreeViewAction.Unknown));
			}
		}

		private Rectangle Bloat (Rectangle rect)
		{
			rect.Y--;
			rect.X--;
			rect.Height += 2;
			rect.Width += 2;
			return rect;
		}

		[DefaultValue(true)]
		public bool ShowLines {
			get { return show_lines; }
			set {
				if (show_lines == value)
					return;
				show_lines = value;
				Refresh ();
			}
		}

		[DefaultValue(true)]
		public bool ShowPlusMinus {
			get { return show_plus_minus; }
			set {
				if (show_plus_minus == value)
					return;
				show_plus_minus = value;
				Refresh ();
			}
		}

		[DefaultValue(true)]
		public bool ShowRootLines {
			get { return show_root_lines; }
			set {
				if (show_root_lines == value)
					return;
				show_root_lines = value;
				Refresh ();
			}
		}

		[DefaultValue(false)]
		public bool Sorted {
			get { return sorted; }
			set {
				if (sorted != value)
					sorted = value;
				if (sorted) {
					Nodes.Sort ();
					Refresh ();
				}
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Bindable(false)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public TreeNode TopNode {
			get { return top_node; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int VisibleCount {
			get {
				return ClientRectangle.Height / ItemHeight;
			}
		}

		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				CreateParams cp = base.CreateParams;
				return cp;
			}
		}

		protected override Size DefaultSize {
			get { return new Size (121, 97); }
		}

		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public void BeginUpdate () {
			update_stack++;
		}

		public void CollapseAll ()
		{
			root_node.CollapseAll ();
		}

		public void EndUpdate ()
		{
			if (update_stack > 1) {
				update_stack--;
			} else {
				update_stack = 0;
				if (update_needed) {
					Invalidate (ViewportRectangle);
					update_needed = false;
				}
			}
		}

		public void ExpandAll () {
			root_node.ExpandAll ();
		}

		public TreeNode GetNodeAt (Point pt) {
			return GetNodeAt (pt.X, pt.Y);
		}

		public TreeNode GetNodeAt (int x, int y) {
			TreeNode node = GetNodeAt (y);
			if (node == null || !(IsTextArea (node, x) || full_row_select))
				return null;
			return node;
					
		}

		public int GetNodeCount (bool include_subtrees) {
			return root_node.GetNodeCount (include_subtrees);
		}

		public override string ToString () {
			int count = Nodes.Count;
			if (count <= 0)
				return String.Concat (base.ToString (), "Node Count: 0");
			return String.Concat (base.ToString (), "Node Count: ", count, " Nodes[0]: ", Nodes [0]);
						
		}

		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void CreateHandle () {
			base.CreateHandle ();
		}

		protected override void Dispose (bool disposing) {
			if (disposing)
				image_list = null;

			base.Dispose (disposing);
		}

		protected OwnerDrawPropertyBag GetItemRenderStyles (TreeNode node, int state) {
			return node.prop_bag;
		}

		protected override bool IsInputKey (Keys key_data) {

			if ((key_data & Keys.Alt) == 0) {
				switch (key_data & Keys.KeyCode) {
				case Keys.Enter:
				case Keys.Escape:
				case Keys.Prior:
				case Keys.Next:
				case Keys.End:
				case Keys.Home:
				case Keys.Left:
				case Keys.Up:
				case Keys.Right:
				case Keys.Down:
					return true;
				}
			}
			return base.IsInputKey (key_data);
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
			OpenTreeNodeEnumerator ne;

			switch (e.KeyData & Keys.KeyCode) {
			case Keys.Add:
				if (selected_node != null && selected_node.IsExpanded)
					selected_node.Expand ();
				break;
			case Keys.Subtract:
				if (selected_node != null && selected_node.IsExpanded)
					selected_node.Collapse ();
				break;
			case Keys.Left:
				if (selected_node != null) {
					if (selected_node.IsExpanded)
						selected_node.Collapse ();
					else {
						ne = new OpenTreeNodeEnumerator (selected_node);
						if (ne.MovePrevious () && ne.MovePrevious ())
							SelectedNode = ne.CurrentNode;
					}
				}
				break;
			case Keys.Right:
				if (selected_node != null) {
					if (!selected_node.IsExpanded)
						selected_node.Expand ();
					else {
						ne = new OpenTreeNodeEnumerator (selected_node);
						if (ne.MoveNext () && ne.MoveNext ())
							SelectedNode = ne.CurrentNode;
					}
				}
				break;
			case Keys.Up:
				if (selected_node != null) {
					ne = new OpenTreeNodeEnumerator (selected_node);
					if (ne.MovePrevious () && ne.MovePrevious ())
						SelectedNode = ne.CurrentNode;
				}
				break;
			case Keys.Down:
				if (selected_node != null) {
					ne = new OpenTreeNodeEnumerator (selected_node);
					if (ne.MoveNext () && ne.MoveNext ())
						SelectedNode = ne.CurrentNode;
				}
				break;
			case Keys.Home:
				if (root_node.Nodes.Count > 0) {
					ne = new OpenTreeNodeEnumerator (root_node.Nodes [0]);
					if (ne.MoveNext ())
						SelectedNode = ne.CurrentNode;
				}
				break;
			case Keys.End:
				if (root_node.Nodes.Count > 0) {
					ne = new OpenTreeNodeEnumerator (root_node.Nodes [0]);
					while (ne.MoveNext ())
					{ }
					SelectedNode = ne.CurrentNode;
				}
				break;
			case Keys.PageDown:
				if (selected_node != null) {
					ne = new OpenTreeNodeEnumerator (selected_node);
					int move = ViewportRectangle.Height / ItemHeight;
					for (int i = 0; i < move && ne.MoveNext (); i++) {
						
					}
					SelectedNode = ne.CurrentNode;
				}
				break;
			case Keys.PageUp:
				if (selected_node != null) {
					ne = new OpenTreeNodeEnumerator (selected_node);
					int move = ViewportRectangle.Height / ItemHeight;
					for (int i = 0; i < move && ne.MovePrevious (); i++)
					{ }
					SelectedNode = ne.CurrentNode;
				}
				break;
			case Keys.Multiply:
				if (selected_node != null)
					selected_node.ExpandAll ();
				break;
			}
			base.OnKeyDown (e);

			if (!e.Handled && checkboxes &&
					selected_node != null &&
					(e.KeyData & Keys.KeyCode) == Keys.Space) {
				TreeViewCancelEventArgs args = new TreeViewCancelEventArgs (
					selected_node, false, TreeViewAction.ByKeyboard);
				OnBeforeCheck (args);
				if (!args.Cancel) {
					selected_node.Checked = !selected_node.Checked;
					OnAfterCheck (new TreeViewEventArgs (selected_node, TreeViewAction.ByKeyboard));
				}
				e.Handled = true;
			}
		}

		protected virtual void OnItemDrag (ItemDragEventArgs e)
		{
			if (ItemDrag != null)
				ItemDrag (this, e);
		}

		protected virtual void OnAfterCheck (TreeViewEventArgs e) {
			if (on_after_check != null)
				on_after_check (this, e);
		}

		protected internal virtual void OnAfterCollapse (TreeViewEventArgs e) {
			if (on_after_collapse != null)
				on_after_collapse (this, e);
		}

		protected internal virtual void OnAfterExpand (TreeViewEventArgs e) {
			if (on_after_expand != null)
				on_after_expand (this, e);
		}

		protected virtual void OnAfterLabelEdit (NodeLabelEditEventArgs e) {
			if (on_after_label_edit != null)
				on_after_label_edit (this, e);
		}

		protected virtual void OnAfterSelect (TreeViewEventArgs e) {
			if (on_after_select != null)
				on_after_select (this, e);
		}

		protected virtual void OnBeforeCheck (TreeViewCancelEventArgs e) {
			if (on_before_check != null)
				on_before_check (this, e);
		}

		protected internal virtual void OnBeforeCollapse (TreeViewCancelEventArgs e) {
			if (on_before_collapse != null)
				on_before_collapse (this, e);
		}

		protected internal virtual void OnBeforeExpand (TreeViewCancelEventArgs e) {
			if (on_before_expand != null)
				on_before_expand (this, e);
		}

		protected virtual void OnBeforeLabelEdit (NodeLabelEditEventArgs e) {
			if (on_before_label_edit != null)
				on_before_label_edit (this, e);
		}

		protected virtual void OnBeforeSelect (TreeViewCancelEventArgs e) {
			if (on_before_select != null)
				on_before_select (this, e);
		}

		protected override void OnHandleCreated (EventArgs e) {
			base.OnHandleCreated (e);

			vbar = new VScrollBar ();
			hbar = new HScrollBar ();

			vbar.Visible = false;
			hbar.Visible = false;
			vbar.ValueChanged += new EventHandler (VScrollBarValueChanged);
			hbar.ValueChanged += new EventHandler (HScrollBarValueChanged);

			SuspendLayout ();
			Controls.AddImplicit (vbar);
			Controls.AddImplicit (hbar);
			ResumeLayout ();
		}

		protected override void OnHandleDestroyed (EventArgs e) {
			base.OnHandleDestroyed (e);
		}

		protected override void WndProc(ref Message m) {
			switch ((Msg) m.Msg) {
			case Msg.WM_PAINT: {				
				PaintEventArgs	paint_event;

				paint_event = XplatUI.PaintEventStart (Handle, true);
				DoPaint (paint_event);
				XplatUI.PaintEventEnd (Handle, true);
				return;
			}
			case Msg.WM_LBUTTONDBLCLK:
				int val = m.LParam.ToInt32();
				DoubleClickHandler (null, new
						MouseEventArgs (MouseButtons.Left,
								2, val & 0xffff,
								(val>>16) & 0xffff, 0));
					break;
			}
			base.WndProc (ref m);
		}

		#endregion	// Protected Instance Methods

		#region	Internal & Private Methods and Properties
		internal string LabelEditText {
			get {
				if (edit_text_box == null)
					return String.Empty;
				return edit_text_box.Text;
			}
		}

		internal IntPtr CreateNodeHandle ()
		{
			return (IntPtr) handle_count++;
		}

		internal TreeNode NodeFromHandle (IntPtr handle)
		{
			// This method is called rarely, so instead of maintaining a table
			// we just walk the tree nodes to find the matching handle
			return NodeFromHandleRecursive (root_node,  handle);
		}

		private TreeNode NodeFromHandleRecursive (TreeNode node, IntPtr handle)
		{
			if (node.handle == handle)
				return node;
			foreach (TreeNode child in node.Nodes) {
				TreeNode match = NodeFromHandleRecursive (child, handle);
				if (match != null)
					return match;
			}
			return null;
		}

		// TODO: we shouldn't have to compute this on the fly
	        internal Rectangle ViewportRectangle {
			get {
				Rectangle res = ClientRectangle;

				if (vbar != null && vbar.Visible)
					res.Width -= vbar.Width;
				if (hbar != null && hbar.Visible)
					res.Height -= hbar.Height;
				return res;
			}
		}

		[MonoTODO ("Need to know if we are editing, not if editing is enabled")]
		private TreeNode GetNodeAt (int y)
		{
			if (nodes.Count <= 0)
				return null;

			if (top_node == null)
				top_node = nodes [0];

			OpenTreeNodeEnumerator o = new OpenTreeNodeEnumerator (TopNode);
			int move = y / ItemHeight;
			for (int i = -1; i < move; i++) {
				if (!o.MoveNext ())
					return null;
			}

			return o.CurrentNode;
		}

		private bool IsTextArea (TreeNode node, int x) {
			return node != null && node.Bounds.Left <= x && node.Bounds.Right >= x;
		}

		private bool IsPlusMinusArea (TreeNode node, int x)
		{
			if (node.Nodes.Count == 0 || (node.parent == root_node && !show_root_lines))
				return false;

			int l = node.Bounds.Left + 5;

			if (show_root_lines || node.Parent != null)
				l -= indent;
			if (ImageList != null)
				l -= ImageList.ImageSize.Width + 3;
			if (checkboxes)
				l -= 19;
			return (x > l && x < l + 8);
		}

		private bool IsCheckboxArea (TreeNode node, int x)
		{
			int l = node.Bounds.Left + 5;

			if (show_root_lines || node.Parent != null)
				l -= indent;
			if (ImageList != null)
				l -= ImageList.ImageSize.Width + 3;
			return (x > l && x < l + 10);
		}

		internal void SetTop (TreeNode node)
		{
			OpenTreeNodeEnumerator walk = new OpenTreeNodeEnumerator (root_node);
			int offset = 0;

			while (walk.CurrentNode != node && walk.MoveNext ())
				offset++;

			vbar.Value = offset;
		}

		internal void SetBottom (TreeNode node)
		{
			OpenTreeNodeEnumerator walk = new OpenTreeNodeEnumerator (node);

			int bottom = ViewportRectangle.Bottom;
			int offset = 0;
			while (walk.MovePrevious ()) {
				if (walk.CurrentNode.Bounds.Bottom <= bottom)
					break;
				offset++;
			}
			vbar.Value += offset;
		}

		internal void UpdateBelow (TreeNode node)
		{
			if (update_stack > 0) {
				update_needed = true;
				return;
			}

			if (node == root_node) {
				Refresh ();
				return;
			}
				
			// We need to update the current node so the plus/minus block gets update too
			Rectangle invalid = new Rectangle (0, node.Bounds.Top - 1,
					Width, Height - node.Bounds.Top + 1);
			Invalidate (invalid);
		}

		internal void UpdateNode (TreeNode node)
		{
			if (update_stack > 0) {
				update_needed = true;
				return;
			}

			if (node == root_node) {
				Refresh ();
				return;
			}

			Rectangle invalid = new Rectangle (0, node.Bounds.Top - 1, Width,
					node.Bounds.Height + 1);
			Invalidate (invalid);
		}

		internal void UpdateNodePlusMinus (TreeNode node)
		{
			if (update_stack > 0) {
				update_needed = true;
				return;
			}

			int l = node.Bounds.Left + 5;

			if (show_root_lines || node.Parent != null)
				l -= indent;
			if (ImageList != null)
				l -= ImageList.ImageSize.Width + 3;
			if (checkboxes)
				l -= 19;

			Invalidate (new Rectangle (l, node.Bounds.Top, 8, node.Bounds.Height));
		}

		private void DoPaint (PaintEventArgs pe)
		{
			if (Width <= 0 || Height <=  0 || Visible == false)
				return;

			Draw (pe.ClipRectangle, pe.Graphics);
		}

		private void Draw (Rectangle clip, Graphics dc)
		{
			if (top_node == null && Nodes.Count > 0)
				top_node = nodes [0];
			// Decide if we need a scrollbar
			int old_open_node_count = open_node_count;

			//Rectangle fill = ClientRectangle;
			add_vscroll = false;
			add_hscroll = false;
			
			dc.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (BackColor), clip);

			Color dash_color = ControlPaint.Dark (BackColor);
			if (dash_color == BackColor)
				dash_color = ControlPaint.Light (BackColor);
			dash = new Pen (dash_color, 1);
			dash.DashStyle = DashStyle.Dash;

			int depth = 0;
			int item_height = ItemHeight;
			int height = ClientRectangle.Height;

			open_node_count = 0;
			used_height = 0;
			foreach (TreeNode node in nodes) {
				DrawNode (node, dc, clip, ref depth, item_height, height);
				depth = 0;
			}

			add_vscroll = (open_node_count * ItemHeight) > ClientRectangle.Height;

			if (max_node_width > ClientRectangle.Width)
				add_hscroll = true;

			if (add_vscroll)
				add_hscroll = max_node_width > ClientRectangle.Width - ThemeEngine.Current.VScrollBarDefaultSize.Width;
			if (add_hscroll)
				add_vscroll = (open_node_count * ItemHeight) > ClientRectangle.Height - ThemeEngine.Current.HScrollBarDefaultSize.Height;
			
			if (add_hscroll) {
				AddHorizontalScrollBar ();
			} else if (hbar != null) {
				hbar_offset = 0;
				hbar.Visible = false;
			}

			if (add_vscroll) {
				AddVerticalScrollBar (open_node_count, old_open_node_count != open_node_count);
			} else if (vbar != null) {
				vbar.Visible = false;
				skipped_nodes = 0;
			}

			if (add_hscroll && add_vscroll) {
				Rectangle corner = new Rectangle (hbar.Right, vbar.Bottom, vbar.Width, hbar.Height);
				if (clip.IntersectsWith (corner))
					dc.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (ThemeEngine.Current.ColorControl), corner);
			}
		}

		private void DrawNodePlusMinus (TreeNode node, Graphics dc, int x, int middle)
		{
			dc.DrawRectangle (SystemPens.ControlDark, x, middle - 4, 8, 8);

			if (node.IsExpanded) {
				dc.DrawLine (SystemPens.ControlDarkDark, x + 2, middle, x + 6, middle); 
			} else {
				dc.DrawLine (SystemPens.ControlDarkDark, x + 2, middle, x + 6, middle);
				dc.DrawLine (SystemPens.ControlDarkDark, x + 4, middle - 2, x + 4, middle + 2);
			}
		}

		private void DrawNodeCheckBox (TreeNode node, Graphics dc, int x, int middle)
		{
			using(Pen pen = new Pen (Color.Black, 2) ) 
				dc.DrawRectangle (pen, x+ 3, middle - 4, 11, 11);

			if (node.Checked) {
				using(Pen check_pen = new Pen (Color.Black, 1)) {
					dc.DrawLine (check_pen, x + 6, middle + 0, x + 8, middle + 3);
					dc.DrawLine (check_pen, x + 6, middle + 1, x + 8, middle + 4);
	
					dc.DrawLine (check_pen, x + 7, middle + 3, x + 13, middle - 2);
					dc.DrawLine (check_pen, x + 7, middle + 4, x + 13, middle - 1);
				}
			}
		}

		private void DrawNodeLines (TreeNode node, Graphics dc, bool visible, Pen dash, int x, int y,
				int middle, int item_height, int node_count)
		{
			int ladjust = 9; // left adjust
			int radjust = 0; // right adjust

			if (node_count > 0 && show_plus_minus)
				ladjust = 13;
			if (checkboxes)
				radjust = 3;

			dc.DrawLine (dash, x - indent + ladjust, middle, x + radjust, middle);

			//if (!visible)
			//	return;

			int ly = 0;
			if (node.PrevNode != null) {
				int prevadjust = (node.Nodes.Count > 0 && show_plus_minus ? (node.PrevNode.Nodes.Count == 0 ? 0 : 4) :
						(node.PrevNode.Nodes.Count == 0 ? 0 : 4));
				int myadjust = (node.Nodes.Count > 0 && show_plus_minus ? 4 : 0);
				ly = node.PrevNode.Bounds.Bottom - (item_height / 2) + prevadjust;
				dc.DrawLine (dash, x - indent + 9, middle - myadjust, x - indent + 9, ly);
			} else if (node.Parent != null) {
				int myadjust = (node.Nodes.Count > 0 && show_plus_minus ? 4 : 0);
				ly = node.Parent.Bounds.Bottom + 1;
				dc.DrawLine (dash, x - indent + 9, middle - myadjust, x - indent + 9, ly);
			}
		}

		private void DrawNodeImage (TreeNode node, Graphics dc, Rectangle clip, int x, int y)
		{
			// Rectangle r = new Rectangle (x, y + 2, ImageList.ImageSize.Width, ImageList.ImageSize.Height);

			if (!RectsIntersect (clip, x, y + 2, ImageList.ImageSize.Width, ImageList.ImageSize.Height))
				return;

			if (node.ImageIndex > -1 && ImageList != null && node.ImageIndex < ImageList.Images.Count) {
				ImageList.Draw (dc, x, y + 2, ImageList.ImageSize.Width, 
						ImageList.ImageSize.Height, node.ImageIndex);
			} else if (ImageIndex > -1 && ImageList != null && ImageIndex < ImageList.Images.Count) {
				ImageList.Draw (dc, x, y + 2, ImageList.ImageSize.Width, 
						ImageList.ImageSize.Height, ImageIndex);
			}
		}

		private void DrawEditNode (TreeNode node)
		{
			SuspendLayout ();

			if (edit_text_box == null) {
				edit_text_box = new TextBox ();
				edit_text_box.BorderStyle = BorderStyle.FixedSingle;
				edit_text_box.KeyUp += new KeyEventHandler (EditTextBoxKeyDown);
				edit_text_box.Leave += new EventHandler (EditTextBoxLeave);
				Controls.AddImplicit (edit_text_box);
			}

			edit_text_box.Bounds = node.Bounds;
			edit_text_box.Width += 4;

			edit_text_box.Text = node.Text;
			edit_text_box.Visible = true;
			edit_text_box.Focus ();
			edit_text_box.SelectAll ();

			ResumeLayout ();
		}

		private void EditTextBoxKeyDown (object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
				EndEdit ();
		}

		private void EditTextBoxLeave (object sender, EventArgs e)
		{
			EndEdit ();
		}

		private void EndEdit ()
		{
			edit_text_box.Visible = false;
			edit_node.EndEdit (false);
			UpdateNode(edit_node);
		}

		private void UpdateNodeBounds (TreeNode node, int x, int y, int item_height, Graphics dc)
		{
			Font font = node.NodeFont;
			if (node.NodeFont == null)
				font = Font;

			if (node.NeedsWidth || update_node_bounds)
				node.SetWidth ((int) dc.MeasureString (node.Text, font, 0,
							       string_format).Width + 3);
			node.SetHeight (item_height);
			node.SetPosition (x, y);
		}
		
		private void DrawSelectionAndFocus(TreeNode node, Graphics dc, Rectangle r)
		{
			if ((!HideSelection || Focused) && SelectedNode == node)
				dc.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (ThemeEngine.Current.ColorHighlight), r);
			else
				dc.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (node.BackColor), r);
			if (Focused && focused_node == node)
				ControlPaint.DrawFocusRectangle (dc, r, ForeColor, BackColor);
		}
		 
		private void DrawStaticNode (TreeNode node, Graphics dc)
		{
			if (!full_row_select)
				DrawSelectionAndFocus(node, dc, node.Bounds);

			Font font = node.NodeFont;
			if (node.NodeFont == null)
				font = Font;
			Color text_color = ((Focused || !HideSelection) && SelectedNode == node ?
					ThemeEngine.Current.ColorHighlightText : node.ForeColor);
			dc.DrawString (node.Text, font,
					ThemeEngine.Current.ResPool.GetSolidBrush (text_color),
					node.Bounds, string_format);
		}

		private void DrawNode (TreeNode node, Graphics dc, Rectangle clip, ref int depth, int item_height, int max_height)
		{
			open_node_count++;
			int x = (!show_root_lines && node.Parent != null ? depth  - 1 : depth) * indent - hbar_offset;
			int y = item_height * (open_node_count - skipped_nodes - 1);
			bool visible = (y >= 0 && y < max_height);
			int _n_count = node.nodes.Count;
			int middle = y + (item_height / 2);

			// The thing is totally out of the clipping rectangle
			if (clip.Top > y + ItemHeight || clip.Bottom < y)
				visible = false;

			if (visible && full_row_select) {
				Rectangle r = new Rectangle(1, y+2, ViewportRectangle.Width-2, item_height);
				DrawSelectionAndFocus(node, dc, r);
			}

			if (show_root_lines || node.Parent != null) {
				x += 5;
				if (_n_count > 0) {
					if (show_plus_minus && visible) {
						DrawNodePlusMinus (node, dc, x, middle);
					}
				}
				x += indent - 5; 
			}

			int ox = x;

			if (checkboxes) {
				if (visible)
					DrawNodeCheckBox (node, dc, ox, middle);
				ox += 19;
			}

			if (show_lines)
				DrawNodeLines (node, dc, visible, dash, x, y, middle, item_height, _n_count);

			if (ImageList != null) {
                                if (visible)
                                        DrawNodeImage (node, dc, clip, ox, y);
				// MS leaves the space for the image if the ImageList is
				// non null regardless of whether or not an image is drawn
				ox += ImageList.ImageSize.Width + 3; // leave a little space so the text isn't against the image
			}

			UpdateNodeBounds (node, ox, y, item_height, dc);

			bool bounds_in_clip = clip.IntersectsWith (node.Bounds) || full_row_select;
			if (visible &&	bounds_in_clip) {
				if (node.IsEditing)
					DrawEditNode (node);
				else
					DrawStaticNode (node, dc);
			}
			
			if (node.Bounds.Right > max_node_width) {
				max_node_width = node.Bounds.Right;
				if (max_node_width > ClientRectangle.Width && !add_hscroll) {
					max_height -= ItemHeight;
					add_hscroll = true;
				}
			}

			if (node.Bounds.Bottom > used_height)
				used_height = node.Bounds.Bottom;

			depth++;
			if (node.IsExpanded) {
				for (int i = 0; i < _n_count; i++) {
					int tdepth = depth;
					DrawNode (node.nodes [i], dc, clip, ref tdepth, item_height, max_height);
				}
			}

		}

		private void AddVerticalScrollBar (int total_nodes, bool count_changed)
		{
			vbar.Bounds = new Rectangle (ClientRectangle.Width - vbar.Width,
				0, vbar.Width, (add_hscroll ? Height - ThemeEngine.Current.HScrollBarDefaultSize.Height : Height));

			if (count_changed) {
				vbar.Maximum = total_nodes;
				int height = ClientRectangle.Height;
				vbar.LargeChange = height / ItemHeight;
			}

			vbar.Visible = true;
		}

		private void AddHorizontalScrollBar ()
		{
			hbar.Bounds = new Rectangle (ClientRectangle.Left,
					ClientRectangle.Bottom - hbar.Height,
					(add_vscroll ? Width - ThemeEngine.Current.VScrollBarDefaultSize.Width : Width), hbar.Height);

			int width = ClientRectangle.Width;
			if (vbar.Visible)
				width -= vbar.Width;

			hbar.SmallChange = 15;
			hbar.LargeChange = 50;
			int num_pixels = max_node_width - width;
			hbar.Maximum = num_pixels + 50;
			hbar.Visible = true;
		}

		private void SizeChangedHandler (object sender, EventArgs e)
		{
			SuspendLayout ();

			if (max_node_width > ClientRectangle.Width && ClientRectangle.Width >= 0) {
				add_hscroll = true;
				AddHorizontalScrollBar ();
				return;
			}

			if (used_height > ClientRectangle.Height && ClientRectangle.Height >= 0) {
				add_vscroll = true;
				AddVerticalScrollBar (open_node_count, true);
				return;
			}

			if (vbar != null && vbar.Visible) {
				int height = (hbar != null && hbar.Visible ? Height - hbar.Height : Height);
				vbar.SetBounds (Right - vbar.Width, 0, 0, height, BoundsSpecified.X | BoundsSpecified.Height);
			}

			if (hbar != null && hbar.Visible) {
				int width = (vbar != null && vbar.Visible ? Width - vbar.Width : Width);
				hbar.SetBounds (0, Bottom - hbar.Height, width, 0, BoundsSpecified.Y | BoundsSpecified.Width);
			}

			ResumeLayout ();
		}

		private void VScrollBarValueChanged (object sender, EventArgs e)
		{
			SetVScrollPos (vbar.Value, null);
		}

		private void SetVScrollPos (int pos, TreeNode new_top)
		{
			if (pos < 0)
				pos = 0;

			if (skipped_nodes == pos)
				return;

			int old_skip = skipped_nodes;
			skipped_nodes = pos;
			int diff = old_skip - skipped_nodes;

			// Determine the new top node if we have to
			if (new_top == null) {
				if (top_node == null)
					top_node = nodes [0];

				OpenTreeNodeEnumerator walk = new OpenTreeNodeEnumerator (TopNode);
				if (diff < 0) {
					for (int i = diff; i <= 0; i++)
						walk.MoveNext ();
					new_top = walk.CurrentNode;
				} else {
					for (int i = 0; i <= diff; i++)
						walk.MovePrevious ();
					new_top = walk.CurrentNode;
				}
			}

			top_node = new_top;
			int y_move = diff * ItemHeight;
			XplatUI.ScrollWindow (Handle, ViewportRectangle, 0, y_move, false);
		}

		private void HScrollBarValueChanged(object sender, EventArgs e)
		{
			int old_offset = hbar_offset;
			hbar_offset = hbar.Value;

			if (hbar_offset < 0)
				hbar_offset = 0;

			XplatUI.ScrollWindow (Handle, ViewportRectangle, old_offset - hbar_offset, 0, false);
		}

		private void FontChangedHandler (object sender, EventArgs e)
		{
			update_node_bounds = true;
		}

		private void LostFocusHandler (object sender, EventArgs e)
		{
			if (selected_node != null)
				UpdateNode (selected_node);
		}

		private void MouseDownHandler (object sender, MouseEventArgs e)
		{
			TreeNode node = GetNodeAt (e.Y);
			if (node == null)
				return;

			if (show_plus_minus && IsPlusMinusArea (node, e.X)) {
				node.Toggle ();
				return;
			} else if (checkboxes && IsCheckboxArea (node, e.X)) {
				node.Checked = !node.Checked;
				UpdateNode(node);
				return;
			} else if (IsTextArea (node, e.X) || full_row_select) {
				TreeNode old_selected = selected_node;
				selected_node = node;
				if (label_edit && e.Clicks == 1 && selected_node == old_selected) {
					node.BeginEdit ();
					if (edit_node != null) {
						edit_node.EndEdit (false);
						UpdateNode (edit_node);
					}
					edit_node = node;
					UpdateNode (edit_node);
				} else if (selected_node != focused_node) {
					select_mmove = true;
					
					if (old_selected != null) {
						Invalidate (Rectangle.Union (Bloat (old_selected.Bounds),
									    Bloat (selected_node.Bounds)));
					} else {
						Invalidate (Bloat (selected_node.Bounds));
					}
				}

				// We ensure its visible after we update because
				// scrolling is used for insure visible
				selected_node.EnsureVisible ();
			} 
		}

		private void MouseUpHandler (object sender, MouseEventArgs e) {

			drag_begin_x = -1;
			drag_begin_y = -1;

			if (!select_mmove)
				return;
				
			select_mmove = false;

			TreeViewCancelEventArgs ce = new TreeViewCancelEventArgs (selected_node, false, TreeViewAction.ByMouse);
			OnBeforeSelect (ce);

			Rectangle invalid;
			if (!ce.Cancel) {
				if (focused_node != null) {
					invalid = Rectangle.Union (Bloat (focused_node.Bounds),
							Bloat (selected_node.Bounds));
				} else {
					invalid = Bloat (selected_node.Bounds);
				}
				focused_node = selected_node;
				OnAfterSelect (new TreeViewEventArgs (selected_node, TreeViewAction.ByMouse));

				Invalidate (invalid);
			} else {
				selected_node = focused_node;
			}

			
		}

		private void MouseMoveHandler (object sender, MouseEventArgs e) {

			if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) {
				if (drag_begin_x == -1 && drag_begin_y == -1) {
					drag_begin_x = e.X;
					drag_begin_y = e.Y;
				} else {
					double rise = Math.Pow (drag_begin_x - e.X, 2);
					double run = Math.Pow (drag_begin_y - e.Y, 2);
					double move = Math.Sqrt (rise + run);
					if (move > 3) {
						TreeNode drag = GetNodeAt (e.X, e.Y);
						
						if (drag != null) {
							OnItemDrag (new ItemDragEventArgs (e.Button, drag));
						}
						drag_begin_x = -1;
						drag_begin_y = -1;
					}
				}
				
			}
			if(!select_mmove)
				return;
			TreeNode node = GetNodeAt(e.X,e.Y);
			if(node == selected_node)
				return;
			
			selected_node = focused_node;
			select_mmove = false;
			Refresh();
		}

		private void DoubleClickHandler (object sender, MouseEventArgs e) {
			TreeNode node = GetNodeAt(e.X,e.Y);
			if(node != null) {
				node.Toggle();
			}
		}

		
		private bool RectsIntersect (Rectangle r, int left, int top, int width, int height)
		{
			return !((r.Left > left + width) || (r.Right < left) ||
					(r.Top > top + height) || (r.Bottom < top));
		}

		#endregion	// Internal & Private Methods and Properties

		#region Events
		public event ItemDragEventHandler ItemDrag;

		public event TreeViewEventHandler AfterCheck {
			add { on_after_check += value; }
			remove { on_after_check -= value; }
		}

		public event TreeViewEventHandler AfterCollapse {
			add { on_after_collapse += value; }
			remove { on_after_collapse -= value; }
		}

		public event TreeViewEventHandler AfterExpand {
			add { on_after_expand += value; }
			remove { on_after_expand -= value; }
		}

		public event NodeLabelEditEventHandler AfterLabelEdit {
			add { on_after_label_edit += value; }
			remove { on_after_label_edit -= value; }
		}

		public event TreeViewEventHandler AfterSelect {
			add { on_after_select += value; }
			remove { on_after_select -= value; }
		}

		public event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		public event TreeViewCancelEventHandler BeforeCheck {
			add { on_before_check += value; }
			remove { on_before_check -= value; }
		}

		public event TreeViewCancelEventHandler BeforeCollapse {
			add { on_before_collapse += value; }
			remove { on_before_collapse -= value; }
		}

		public event TreeViewCancelEventHandler BeforeExpand {
			add { on_before_expand += value; }
			remove { on_before_expand -= value; }
		}

		public event NodeLabelEditEventHandler BeforeLabelEdit {
			add { on_before_label_edit += value; }
			remove { on_before_label_edit -= value; }
		}

		public event TreeViewCancelEventHandler BeforeSelect {
			add { on_before_select += value; }
			remove { on_before_select -= value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	
		[Browsable (false)]
		public new event PaintEventHandler Paint {
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	
		[Browsable (false)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}
		#endregion	// Events
	}
}

