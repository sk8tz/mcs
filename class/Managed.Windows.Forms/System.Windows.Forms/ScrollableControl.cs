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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//


// NOT COMPLETE

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;

namespace System.Windows.Forms {
	[Designer ("System.Windows.Forms.Design.ScrollableControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class ScrollableControl : Control {
		#region Local Variables
		private bool			auto_vscroll;
		private bool			auto_hscroll;
		private bool			hscroll_visible;
		private bool			vscroll_visible;
		private bool			force_hscroll_visible;
		private bool			force_vscroll_visible;
		private bool			auto_scroll;
		private Size			auto_scroll_margin;
		private Size			auto_scroll_min_size;
		private Point			scroll_position;
		private DockPaddingEdges	dock_padding;
		private SizeGrip		sizegrip;
		private HScrollBar		hscrollbar;
		private VScrollBar		vscrollbar;
		#endregion	// Local Variables

		[MonoTODO("Need to use the edge values when performing the layout")]
		[TypeConverter(typeof(ScrollableControl.DockPaddingEdgesConverter))]
		#region Subclass DockPaddingEdges
		public class DockPaddingEdges : ICloneable {
			#region DockPaddingEdges Local Variables
			private int	all;
			private int	left;
			private int	right;
			private int	top;
			private int	bottom;
			private Control	owner;
			#endregion	// DockPaddingEdges Local Variables

			#region DockPaddingEdges Constructor
			internal DockPaddingEdges(Control owner) {
				all = 0;
				left = 0;
				right = 0;
				top = 0;
				bottom = 0;
				this.owner = owner;
			}
			#endregion	// DockPaddingEdges Constructor

			#region DockPaddingEdges Public Instance Properties
			[RefreshProperties(RefreshProperties.All)]
			public int All {
				get {
					return all;
				}

				set {
					all = value;
					left = value;
					right = value;
					top = value;
					bottom = value;

					owner.PerformLayout();
				}
			}

			[RefreshProperties(RefreshProperties.All)]
			public int Bottom {
				get {
					return bottom;
				}

				set {
					bottom = value;
					all = 0;

					owner.PerformLayout();
				}
			}

			[RefreshProperties(RefreshProperties.All)]
			public int Left {
				get {
					return left;
				}

				set {
					left=value;
					all = 0;

					owner.PerformLayout();
				}
			}

			[RefreshProperties(RefreshProperties.All)]
			public int Right {
				get {
					return right;
				}

				set {
					right=value;
					all = 0;

					owner.PerformLayout();
				}
			}

			[RefreshProperties(RefreshProperties.All)]
			public int Top {
				get {
					return top;
				}

				set {
					top=value;
					all = 0;

					owner.PerformLayout();
				}
			}

			#endregion	// DockPaddingEdges Public Instance Properties

			// Public Instance Methods
			public override bool Equals(object other) {
				if (! (other is DockPaddingEdges)) {
					return false;
				}

				if (	(this.all == ((DockPaddingEdges)other).all) && (this.left == ((DockPaddingEdges)other).left) &&
					(this.right == ((DockPaddingEdges)other).right) && (this.top == ((DockPaddingEdges)other).top) && 
					(this.bottom == ((DockPaddingEdges)other).bottom)) {
					return true;
				}

				return false;
			}

			public override int GetHashCode() {
				return all*top*bottom*right*left;
			}

			public override string ToString() {
				return "All = "+all.ToString()+" Top = "+top.ToString()+" Left = "+left.ToString()+" Bottom = "+bottom.ToString()+" Right = "+right.ToString();
			}

			object ICloneable.Clone() {
				DockPaddingEdges padding_edge;

				padding_edge=new DockPaddingEdges(owner);

				padding_edge.all=all;
				padding_edge.left=left;
				padding_edge.right=right;
				padding_edge.top=top;
				padding_edge.bottom=bottom;

				return padding_edge;
			}
		}
		#endregion	// Subclass DockPaddingEdges

		#region Subclass DockPaddingEdgesConverter
		public class DockPaddingEdgesConverter : System.ComponentModel.TypeConverter {
			// Public Constructors
			public DockPaddingEdgesConverter() {
			}

			// Public Instance Methods
			public override PropertyDescriptorCollection GetProperties(System.ComponentModel.ITypeDescriptorContext context, object value, Attribute[] attributes) {
				return TypeDescriptor.GetProperties(typeof(DockPaddingEdges), attributes);
			}

			public override bool GetPropertiesSupported(System.ComponentModel.ITypeDescriptorContext context) {
				return true;
			}
		}
		#endregion	// Subclass DockPaddingEdgesConverter

		#region Public Constructors
		public ScrollableControl() {
			SetStyle(ControlStyles.ContainerControl, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, false);
			auto_scroll = false;
			auto_hscroll = false;
			auto_vscroll = false;
			hscroll_visible = false;
			vscroll_visible = false;
			force_hscroll_visible = false;
			force_vscroll_visible = false;
			auto_scroll_margin = new Size(0, 0);
			auto_scroll_min_size = new Size(0, 0);
			scroll_position = new Point(0, 0);
			dock_padding = new DockPaddingEdges(this);
			SizeChanged +=new EventHandler(Recalculate);
			VisibleChanged += new EventHandler(Recalculate);
		}
		#endregion	// Public Constructors

		#region Protected Static Fields
		protected const int ScrollStateAutoScrolling	= 1;
		protected const int ScrollStateFullDrag		= 16;
		protected const int ScrollStateHScrollVisible	= 2;
		protected const int ScrollStateUserHasScrolled	= 8;
		protected const int ScrollStateVScrollVisible	= 4;
		#endregion	// Protected Static Fields

		#region Public Instance Properties
		[DefaultValue(false)]
		[Localizable(true)]
		public virtual bool AutoScroll {
			get {
				return	auto_scroll;
			}

			set {
				if (auto_scroll == value) {
					return;
				}

				auto_scroll = value;
				if (!auto_scroll) {
					SuspendLayout ();

					Controls.RemoveImplicit (hscrollbar);
					hscrollbar.Dispose();
					hscrollbar = null;
					hscroll_visible = false;

					Controls.RemoveImplicit (vscrollbar);
					vscrollbar.Dispose();
					vscrollbar = null;
					vscroll_visible = false;

					Controls.RemoveImplicit (sizegrip);
					sizegrip.Dispose();
					sizegrip = null;

					ResumeLayout ();
				} else {
					SuspendLayout ();

					hscrollbar = new HScrollBar();
					hscrollbar.Visible = false;
					hscrollbar.ValueChanged += new EventHandler(HandleScrollBar);
					hscrollbar.Height = SystemInformation.HorizontalScrollBarHeight;
					this.Controls.AddImplicit (hscrollbar);

					vscrollbar = new VScrollBar();
					vscrollbar.Visible = false;
					vscrollbar.ValueChanged += new EventHandler(HandleScrollBar);
					vscrollbar.Width = SystemInformation.VerticalScrollBarWidth;
					this.Controls.AddImplicit (vscrollbar);

					sizegrip = new SizeGrip();
					sizegrip.Visible = false;
					this.Controls.AddImplicit (sizegrip);

					ResumeLayout ();
				}
			}
		}

		[Localizable(true)]
		public Size AutoScrollMargin {
			get {
				return auto_scroll_margin;
			}

			set {
				if (value.Width < 0) {
					throw new ArgumentException("Width is assigned less than 0", "value.Width");
				}

				if (value.Height < 0) {
					throw new ArgumentException("Height is assigned less than 0", "value.Height");
				}

				auto_scroll_margin = value;
			}
		}

		[Localizable(true)]
		public Size AutoScrollMinSize {
			get {
				return auto_scroll_min_size;
			}

			set {
				auto_scroll_min_size = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Point AutoScrollPosition {
			get {
				return new Point(-scroll_position.X, -scroll_position.Y);
			}

			set {
				if ((value.X != scroll_position.X) || (value.Y != scroll_position.Y)) {
					int	shift_x;
					int	shift_y;

					shift_x = 0;
					shift_y = 0;
					if (hscroll_visible) {
						shift_x = value.X - scroll_position.X;
					}

					if (vscroll_visible) {
						shift_y = value.Y - scroll_position.Y;
					}

					ScrollWindow(shift_x, shift_y);

					if (hscroll_visible) {
						hscrollbar.Value = scroll_position.X;
					}

					if (vscroll_visible) {
						vscrollbar.Value = scroll_position.Y;
					}

				}
			}
		}

		public override Rectangle DisplayRectangle {
			get {
				Rectangle rect;
				
				rect = base.DisplayRectangle;
				if (vscroll_visible) {
					rect.Width -= vscrollbar.Width;
					if (rect.Width < 0) {
						rect.Width = 0;
					}
				}
				
				if (hscroll_visible) {
					rect.Height -= hscrollbar.Height;
					if (rect.Height < 0) {
						rect.Height = 0;
					}
				}

				rect.X += dock_padding.Left;
				rect.Y += dock_padding.Top;

				rect.Width -= dock_padding.Right + dock_padding.Left;
				rect.Height -= dock_padding.Bottom + dock_padding.Top;

				return rect;
				//return new Rectangle(-scroll_position.X, -scroll_position.Y, auto_scroll_min_size.Width, auto_scroll_min_size.Height);
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Localizable(true)]
		public DockPaddingEdges DockPadding {
			get {
				return dock_padding;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Methods
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected bool HScroll {
			get {
				return hscroll_visible;
			}

			set {
				if (hscroll_visible != value) {
					force_hscroll_visible = value;
					Recalculate(this, EventArgs.Empty);
				}
			}
		}

		protected bool VScroll {
			get {
				return vscroll_visible;
			}

			set {
				if (vscroll_visible != value) {
					force_vscroll_visible = value;
					Recalculate(this, EventArgs.Empty);
				}
			}
		}
		#endregion	// Protected Instance Methods

		#region Public Instance Methods
		public void ScrollControlIntoView(Control activeControl) {
		}

		public void SetAutoScrollMargin(int x, int y) {
			if (x < 0) {
				x = 0;
			}

			if (y < 0) {
				y = 0;
			}

			auto_scroll_margin = new Size(x, y);
			Recalculate(this, EventArgs.Empty);
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void AdjustFormScrollbars(bool displayScrollbars) {
			// Internal MS
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected bool GetScrollState(int bit) {
			return false;
			// Internal MS
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnLayout(LayoutEventArgs levent) {
			base.OnLayout(levent);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnMouseWheel(MouseEventArgs e) {
			if (vscroll_visible) {
				if (e.Delta > 0) {
					if (vscrollbar.Minimum < (vscrollbar.Value - vscrollbar.LargeChange)) {
						vscrollbar.Value -= vscrollbar.LargeChange;
					} else {
						vscrollbar.Value = vscrollbar.Minimum;
					}
				} else {
					if (vscrollbar.Maximum > (vscrollbar.Value + vscrollbar.LargeChange)) {
							vscrollbar.Value += vscrollbar.LargeChange;
						} else {
							vscrollbar.Value = vscrollbar.Maximum;
						}
					}
			}
			base.OnMouseWheel(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnVisibleChanged(EventArgs e) {
			base.OnVisibleChanged(e);
		}

		protected override void ScaleCore(float dx, float dy) {
			base.ScaleCore(dx, dy);
		}

		protected void SetDisplayRectLocation(int x, int y) {
			throw new NotImplementedException();
		}

		protected void SetScrollState(int bit, bool value) {
			throw new NotImplementedException();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);
		}
		#endregion	// Protected Instance Methods

		#region Internal & Private Methods
		private Size Canvas {
			get {
				int	num_of_children;
				int	width;
				int	height;

				num_of_children = child_controls.Count;
				width = 0;
				height = 0;

				for (int i = 0; i < num_of_children; i++) {
					if ((child_controls[i].Visible == false) || (child_controls[i] == hscrollbar) || (child_controls[i] == vscrollbar) || (child_controls[i] == sizegrip)) {
						continue;
					}
					if (child_controls[i].Right > width) {
						width = child_controls[i].Right;
					}

					if (child_controls[i].Bottom > height) {
						height = child_controls[i].Bottom;
					}
				}

				return new Size(width, height);
			}
		}

		private void Recalculate (object sender, EventArgs e)
		{
			if (!Visible || !this.auto_scroll && !force_hscroll_visible && !force_vscroll_visible) {
				return;
			}

			Size canvas = Canvas;
			Size client = ClientRectangle.Size;

			canvas.Width += auto_scroll_margin.Width;
			canvas.Height += auto_scroll_margin.Height;

			int right_edge = client.Width;
			int bottom_edge = client.Height;
			int prev_right_edge;
			int prev_bottom_edge;

			do {
				prev_right_edge = right_edge;
				prev_bottom_edge = bottom_edge;

				if ((force_hscroll_visible || canvas.Width > right_edge) && client.Width > 0) {
					hscroll_visible = true;
					bottom_edge = client.Height - SystemInformation.HorizontalScrollBarHeight;
				} else {
					hscroll_visible = false;
					bottom_edge = client.Height;
				}

				if ((force_vscroll_visible || canvas.Height > bottom_edge) && client.Height > 0) {
					vscroll_visible = true;
					right_edge = client.Width - SystemInformation.VerticalScrollBarWidth;
				} else {
					vscroll_visible = false;
					right_edge = client.Width;
				}

			} while (right_edge != prev_right_edge || bottom_edge != prev_bottom_edge);

			if (hscroll_visible) {
				hscrollbar.Left = 0;
				hscrollbar.Top = client.Height - SystemInformation.HorizontalScrollBarHeight;
				hscrollbar.LargeChange = DisplayRectangle.Width;
				hscrollbar.SmallChange = DisplayRectangle.Width / 10;
				hscrollbar.Maximum = canvas.Width - 1;
			} else {
				scroll_position.X = 0;
			}

			if (vscroll_visible) {
				vscrollbar.Left = client.Width - SystemInformation.VerticalScrollBarWidth;
				vscrollbar.Top = 0;

				vscrollbar.LargeChange = DisplayRectangle.Height;
				vscrollbar.SmallChange = DisplayRectangle.Height / 10;
				vscrollbar.Maximum = canvas.Height - 1;
			} else {
				scroll_position.Y = 0;
			}

			if (hscroll_visible && vscroll_visible) {
				hscrollbar.Width = ClientRectangle.Width - SystemInformation.VerticalScrollBarWidth;
				vscrollbar.Height = ClientRectangle.Height - SystemInformation.HorizontalScrollBarHeight;

				sizegrip.Left =  hscrollbar.Right;
				sizegrip.Top =  vscrollbar.Bottom;
				sizegrip.Width = SystemInformation.VerticalScrollBarWidth;
				sizegrip.Height = SystemInformation.HorizontalScrollBarHeight;

				hscrollbar.Visible = true;
				vscrollbar.Visible = true;
				sizegrip.Visible = true;
			} else {
				sizegrip.Visible = false;
				if (hscroll_visible) {
					hscrollbar.Width = ClientRectangle.Width;
					hscrollbar.Visible = true;
				} else {
					hscrollbar.Visible = false;
				}

				if (vscroll_visible) {
					vscrollbar.Height = ClientRectangle.Height;
					vscrollbar.Visible = true;
				} else {
					vscrollbar.Visible = false;
				}
			}
		}

		private void HandleScrollBar(object sender, EventArgs e) {
			if (sender == vscrollbar) {
				ScrollWindow(0, vscrollbar.Value- scroll_position.Y);
			} else {
				ScrollWindow(hscrollbar.Value - scroll_position.X, 0);
			}
		}

		private void ScrollWindow(int XOffset, int YOffset) {
			int	num_of_children;

			SuspendLayout();

			num_of_children = child_controls.Count;

			for (int i = 0; i < num_of_children; i++) {
				if (child_controls[i] == hscrollbar || child_controls[i] == vscrollbar || child_controls[i] == sizegrip) {
					continue;
				}
				child_controls[i].Left -= XOffset;
				child_controls[i].Top -= YOffset;
				// Is this faster? child_controls[i].Location -= new Size(XOffset, YOffset);
			}

			scroll_position.X += XOffset;
			scroll_position.Y += YOffset;

			// Should we call XplatUI.ScrollWindow???
			Invalidate();
			ResumeLayout();
		}
		#endregion	// Internal & Private Methods

	}
}
