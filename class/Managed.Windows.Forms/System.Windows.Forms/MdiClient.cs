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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//

// NOT COMPLETE

using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
#if NET_2_0
	[ComVisible (true)]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
#endif
	[DesignTimeVisible(false)]
	[ToolboxItem(false)]
	public sealed class MdiClient : Control {
		#region Local Variables
		private int mdi_created;
		private ImplicitHScrollBar hbar;
		private ImplicitVScrollBar vbar;
		private SizeGrip sizegrip;
		private int hbar_value;
		private int vbar_value;
		private bool lock_sizing;
		private bool initializing_scrollbars;
		private int prev_bottom;
		private bool setting_windowstates = false;
		internal ArrayList mdi_child_list;
		private string form_text;
		private bool setting_form_text;
		private Form active_child;

		#endregion	// Local Variables

		#region Public Classes
#if NET_2_0
		[ComVisible (false)]
#endif
		public new class ControlCollection : Control.ControlCollection {

			private MdiClient owner;
			
			public ControlCollection(MdiClient owner) : base(owner) {
				this.owner = owner;
			}

			public override void Add(Control value) {
				if ((value is Form) == false || !(((Form)value).IsMdiChild)) {
					throw new ArgumentException("Form must be MdiChild");
				}
				owner.mdi_child_list.Add (value);
				base.Add (value);

				// newest member is the active one
				Form form = (Form) value;
				owner.ActiveMdiChild = form;
			}

			public override void Remove(Control value)
			{
				Form form = value as Form;
				if (form != null) {
					MdiWindowManager wm = form.WindowManager as MdiWindowManager;
					if (wm != null) {
						form.Closed -= wm.form_closed_handler;
					}
				}

				owner.mdi_child_list.Remove (value);
				base.Remove (value);
			}
		}
		#endregion	// Public Classes

		#region Public Constructors
		public MdiClient()
		{
			mdi_child_list = new ArrayList ();
			BackColor = SystemColors.AppWorkspace;
			Dock = DockStyle.Fill;
			SetStyle (ControlStyles.Selectable, false);
		}
		#endregion	// Public Constructors

		internal void SendFocusToActiveChild ()
		{
			Form active = this.ActiveMdiChild;
			if (active == null) {
				ParentForm.SendControlFocus (this);
			} else {
				active.SendControlFocus (active);
				ParentForm.ActiveControl = active;
			}
		}

		internal bool HorizontalScrollbarVisible {
			get { return hbar != null && hbar.Visible; }
		}
		internal bool VerticalScrollbarVisible {
			get { return vbar != null && vbar.Visible; }
		}

		internal void SetParentText(bool text_changed)
		{
			if (setting_form_text)
				return;

			setting_form_text = true;

			if (text_changed)
				form_text = ParentForm.Text;

			if (ParentForm.ActiveMaximizedMdiChild == null) {
				ParentForm.Text = form_text;
			} else {
				string childText = ParentForm.ActiveMaximizedMdiChild.form.Text;
				if (childText.Length > 0) {
					ParentForm.Text = form_text + " - [" + ParentForm.ActiveMaximizedMdiChild.form.Text + "]";
				} else {
					ParentForm.Text = form_text;
				}
			}

			setting_form_text = false;
		}

		internal override void OnPaintBackgroundInternal (PaintEventArgs pe)
		{
			if (BackgroundImage != null)
				return;

			if (Parent == null || Parent.BackgroundImage == null)
				return;
			Parent.PaintControlBackground (pe);
		}

		internal Form ParentForm {
			get { return (Form) Parent; }
		}

		protected override Control.ControlCollection CreateControlsInstance ()
		{
			return new MdiClient.ControlCollection (this);
		}

		protected override void WndProc(ref Message m) {
			switch ((Msg)m.Msg) {
			case Msg.WM_NCCALCSIZE:
				XplatUIWin32.NCCALCSIZE_PARAMS	ncp;

				if (m.WParam == (IntPtr) 1) {
					ncp = (XplatUIWin32.NCCALCSIZE_PARAMS) Marshal.PtrToStructure (m.LParam,
							typeof (XplatUIWin32.NCCALCSIZE_PARAMS));

					int bw = 2;

					ncp.rgrc1.top += bw;
					ncp.rgrc1.bottom -= bw;
					ncp.rgrc1.left += bw;
					ncp.rgrc1.right -= bw;
					
					Marshal.StructureToPtr (ncp, m.LParam, true);
				}

				break;

			case Msg.WM_NCPAINT:
				PaintEventArgs pe = XplatUI.PaintEventStart (Handle, false);

				Rectangle clip;
				clip = new Rectangle (0, 0, Width, Height);

				ControlPaint.DrawBorder3D (pe.Graphics, clip, Border3DStyle.Sunken);
				XplatUI.PaintEventEnd (Handle, false);
				m.Result = IntPtr.Zero;
				return ;
			}

			base.WndProc (ref m);
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);

			if (Parent != null)
				XplatUI.InvalidateNC (Parent.Handle);
			// Should probably make this into one loop
			SizeScrollBars ();
			ArrangeWindows ();
		}
#if NET_2_0
		[System.ComponentModel.EditorBrowsable (EditorBrowsableState.Never)]
#endif
		protected override void ScaleCore (float dx, float dy)
		{
			base.ScaleCore (dx, dy);
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);
		}

		#region Public Instance Properties
		[Localizable(true)]
		public override System.Drawing.Image BackgroundImage {
			get {
				return base.BackgroundImage;
			}
			set {
				base.BackgroundImage = value;
			}
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public override ImageLayout BackgroundImageLayout {
			get {
				return base.BackgroundImageLayout;
			}
			set {
				base.BackgroundImageLayout = value;
			}
		}
#endif

		public Form [] MdiChildren {
			get {
				if (mdi_child_list == null)
					return new Form [0];
				return (Form []) mdi_child_list.ToArray (typeof (Form));
			}
		}
		#endregion	// Public Instance Properties

#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public void LayoutMdi (MdiLayout value) {

			int max_width = Int32.MaxValue;
			int max_height = Int32.MaxValue;

			if (Parent != null) {
				max_width = Parent.Width;
				max_height = Parent.Height;
			}

			switch (value) {
			case MdiLayout.Cascade: {
				int i = 0;
				for (int c = Controls.Count - 1; c >= 0; c--) {
					Form form = (Form) Controls [c];

					if (form.WindowState == FormWindowState.Minimized)
						continue;
		
					int l = 22 * i;
					int t = 22 * i;

					if (i != 0 && (l + form.Width > max_width || t + form.Height > max_height)) {
						i = 0;
						l = 22 * i;
						t = 22 * i;
					}

					form.Left = l;
					form.Top = t;

					i++;
				}
				break;
				}
			case MdiLayout.ArrangeIcons:
				ArrangeIconicWindows (true);
				break;
			case MdiLayout.TileHorizontal:
			case MdiLayout.TileVertical: {
				// First count number of windows to tile
				int total = 0;
				for (int i = 0; i < Controls.Count; i++) {
					Form form = Controls [i] as Form;
					
					if (form == null)
						continue;
					
					if (!form.Visible)
						continue;
					
					if (form.WindowState == FormWindowState.Minimized)
						continue;
						
					total++;
				}
				if (total <= 0)
					return;

				// Calculate desired height and width
				Size newSize;
				Size offset;
				if (value == MdiLayout.TileHorizontal) {
					newSize = new Size (ClientSize.Width, ClientSize.Height / total);
					offset = new Size (0, newSize.Height);
				} else {
					newSize = new Size (ClientSize.Width / total, ClientSize.Height);
					offset = new Size (newSize.Width, 0);
				}
				
				// Loop again and set the size and location.
				Point nextLocation = Point.Empty;
				
				for (int i = 0; i < Controls.Count; i++) {
					Form form = Controls [i] as Form;

					if (form == null)
						continue;

					if (!form.Visible)
						continue;

					if (form.WindowState == FormWindowState.Minimized)
						continue;

					form.Size = newSize;
					form.Location = nextLocation;
					nextLocation += offset;
				}
				
				break;
				}
			}
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		#endregion	// Protected Instance Methods

		internal void SizeScrollBars ()
		{
			if (lock_sizing)
				return;

			if (Controls.Count == 0 || ((Form) Controls [0]).WindowState == FormWindowState.Maximized) {
				if (hbar != null)
					hbar.Visible = false;
				if (vbar != null)
					vbar.Visible = false;
				if (sizegrip != null)
					sizegrip.Visible = false;
				return;
			}

			int right = 0;
			int left = 0;
			int top = 0;
			int bottom = 0;

			foreach (Form child in Controls) {
				if (!child.Visible)
					continue;
				if (child.Right > right)
					right = child.Right;
				if (child.Left < left) {
					left = child.Left;
				}
				
				if (child.Bottom > bottom)
					bottom = child.Bottom;
				if (child.Top < 0) {
					top = child.Top;
				}
			}

			int available_width = ClientSize.Width;
			int available_height = ClientSize.Height;

			bool need_hbar = false;
			bool need_vbar = false;

			if (right - left > available_width || left < 0) {
				need_hbar = true;
				available_height -= SystemInformation.HorizontalScrollBarHeight;
			}
			if (bottom - top > available_height || top < 0) {
				need_vbar = true;
				available_width -= SystemInformation.VerticalScrollBarWidth;

				if (!need_hbar && (right - left > available_width || left < 0)) {
					need_hbar = true;
					available_height -= SystemInformation.HorizontalScrollBarHeight;
				}
			}
			
			if (need_hbar) {
				if (hbar == null) {
					hbar = new ImplicitHScrollBar ();
					Controls.AddImplicit (hbar);
				}
				hbar.Visible = true;
				CalcHBar (left, right, need_vbar);
			} else if (hbar != null)
				hbar.Visible = false;

			if (need_vbar) {
				if (vbar == null) {
					vbar = new ImplicitVScrollBar ();
					Controls.AddImplicit (vbar);
				}
				vbar.Visible = true;
				CalcVBar (top, bottom, need_hbar);
			} else if (vbar != null)
				vbar.Visible = false;

			if (need_hbar && need_vbar) {
				if (sizegrip == null) {
					sizegrip = new SizeGrip ();
					sizegrip.CapturedControl = this.ParentForm;
					Controls.AddImplicit (sizegrip);
				}
				sizegrip.Location = new Point (hbar.Right, vbar.Bottom);
				sizegrip.Width = vbar.Width;
				sizegrip.Height = hbar.Height;
				sizegrip.Visible = true;
				XplatUI.SetZOrder (sizegrip.Handle, vbar.Handle, false, false);
			} else if (sizegrip != null) {
				sizegrip.Visible = false;
			}
		}

		private void CalcHBar (int left, int right, bool vert_vis)
		{
			initializing_scrollbars = true;

			hbar.Left = 0;
			hbar.Top = ClientRectangle.Bottom - hbar.Height;
			hbar.Width = ClientRectangle.Width - (vert_vis ? SystemInformation.VerticalScrollBarWidth : 0);
			hbar.LargeChange = 50;
			hbar.Minimum = Math.Min (left, 0);
			hbar.Maximum = Math.Max (right - ClientSize.Width + 51 + (vert_vis ? SystemInformation.VerticalScrollBarWidth : 0), 0);
			hbar.Value = 0;
			hbar_value = 0;
			hbar.ValueChanged += new EventHandler (HBarValueChanged);
			XplatUI.SetZOrder (hbar.Handle, IntPtr.Zero, true, false);
			
			initializing_scrollbars = false;
		}

		private void CalcVBar (int top, int bottom, bool horz_vis)
		{
			initializing_scrollbars = true;
			
			vbar.Top = 0;
			vbar.Left = ClientRectangle.Right - vbar.Width;
			vbar.Height = ClientRectangle.Height - (horz_vis ? SystemInformation.HorizontalScrollBarHeight : 0);
			vbar.LargeChange = 50;
			vbar.Minimum = Math.Min (top, 0);
			vbar.Maximum = Math.Max (bottom - ClientSize.Height + 51 + (horz_vis ? SystemInformation.HorizontalScrollBarHeight : 0), 0);
			vbar.Value = 0;
			vbar_value = 0;
			vbar.ValueChanged += new EventHandler (VBarValueChanged);
			XplatUI.SetZOrder (vbar.Handle, IntPtr.Zero, true, false);
			
			initializing_scrollbars = false;
		}

		private void HBarValueChanged (object sender, EventArgs e)
		{
			if (initializing_scrollbars)
				return;
			
			if (hbar.Value == hbar_value)
				return;

			lock_sizing = true;

			try {
				int diff = hbar_value - hbar.Value;
				foreach (Form child in Controls) {
					child.Left += diff;
				}
			} finally {
				lock_sizing = false;
			}

			hbar_value = hbar.Value;
		}

		private void VBarValueChanged (object sender, EventArgs e)
		{
			if (initializing_scrollbars)
				return;
				
			if (vbar.Value == vbar_value)
				return;

			lock_sizing = true;

			try {
				int diff = vbar_value - vbar.Value;
				foreach (Form child in Controls) {
					child.Top += diff;
				}
			} finally {
				lock_sizing = false;
			}

			vbar_value = vbar.Value;
		}

		private void ArrangeWindows ()
		{
			int change = 0;
			if (prev_bottom != -1)
				change = Bottom - prev_bottom;

			foreach (Control c in Controls) {
				Form child = c as Form;

				if (c == null || !child.Visible)
					continue;

				MdiWindowManager wm = child.WindowManager as MdiWindowManager;
				if (wm.GetWindowState () == FormWindowState.Maximized)
					wm.SizeMaximized ();

				if (wm.GetWindowState () == FormWindowState.Minimized) {
					child.Top += change;
				}
					
			}

			prev_bottom = Bottom;
		}

		internal void ArrangeIconicWindows (bool rearrange_all)
		{
			int xspacing = 160;
			int yspacing = 25;

			Rectangle rect = new Rectangle (0, 0, xspacing, yspacing);

			lock_sizing = true;
			foreach (Form form in Controls) {
				if (form.WindowState != FormWindowState.Minimized)
					continue;

				MdiWindowManager wm = (MdiWindowManager) form.WindowManager;
				
				if (wm.IconicBounds != Rectangle.Empty && !rearrange_all) {
					if (form.Bounds != wm.IconicBounds)
						form.Bounds = wm.IconicBounds;
					continue;
				}
				
				// Need to get the width in the loop cause some themes might have
				// different widths for different styles
				int bw = ThemeEngine.Current.ManagedWindowBorderWidth (wm);
				
				// The extra one pixel is a cheap hack for now until we
				// handle 0 client sizes properly in the driver
				int height = wm.TitleBarHeight + (bw * 2) + 1;
				
				bool success = true;
				int startx, starty, currentx, currenty;
				
				startx = 0;
				starty = Bottom - yspacing - bw - 2;
				currentx = startx;
				currenty = starty;
				
				do {
					rect.X = currentx;
					rect.Y = currenty;
					rect.Height = height;
					success = true;
					foreach (Form form2 in Controls) {
						if (form2 == form || form2.window_state != FormWindowState.Minimized)
							continue;
						
						if (form2.Bounds.IntersectsWith(rect)) {
							success = false;
							break;
						}
					}
					if (!success) {	
						currentx += xspacing;
						if (currentx + xspacing > Right) {
							currentx = startx;
							currenty -= Math.Max(yspacing, height);
						} 
					}
				} while (!success);
				wm.IconicBounds = rect;
				form.Bounds = wm.IconicBounds;
			}
			lock_sizing = false;
		}

		internal void CloseChildForm (Form form)
		{
			if (Controls.Count > 1) {
				Form next = (Form) Controls [1];
				if (form.WindowState == FormWindowState.Maximized)
					next.WindowState = FormWindowState.Maximized;
				ActivateChild (next);
			}

			Controls.Remove (form);
			form.Close ();

			XplatUI.RequestNCRecalc (Handle);
			if (Controls.Count == 0) {
				XplatUI.RequestNCRecalc (Parent.Handle);
				ParentForm.PerformLayout ();
			}
			SizeScrollBars ();
			SetParentText (false);
		}

		internal void ActivateNextChild ()
		{
			if (Controls.Count < 1)
				return;
			if (Controls.Count == 1 && Controls[0] == ActiveMdiChild)
				return;
				
			Form front = (Form) Controls [0];
			Form form = (Form) Controls [1];

			front.SendToBack ();
			ActivateChild (form);
		}

		internal void ActivateChild (Form form)
		{
			if (Controls.Count < 1)
				return;
			
			if (ParentForm.is_changing_visible_state)
				return;
				
			Form current = (Form) Controls [0];
			form.SuspendLayout ();
			form.BringToFront ();
			form.SendControlFocus (form);
			form.ResumeLayout(false);
			SetWindowStates ((MdiWindowManager) form.window_manager);
			if (current != form) {
				form.has_focus = false;
				XplatUI.InvalidateNC (current.Handle);
				XplatUI.InvalidateNC (form.Handle);
			}
			active_child = (Form) Controls [0];
			ParentForm.ActiveControl = active_child;
		}

		internal override IntPtr AfterTopMostControl ()
		{
			// order of scrollbars:
			// top = vertical
			//       sizegrid
			// bottom = horizontal
			if (hbar != null && hbar.Visible)
				return hbar.Handle;
			// no need to check for sizegrip since it will only
			// be visible if hbar is visible.
			if (vbar != null && vbar.Visible)
				return vbar.Handle;
				
			return base.AfterTopMostControl ();
		}
		
		internal bool SetWindowStates (MdiWindowManager wm)
		{
		/*
			MDI WindowState behaviour:
			- If the active window is maximized, all other maximized windows are normalized.
			- If a normal window gets focus and the original active window was maximized, 
			  the normal window gets maximized and the original window gets normalized.
			- If a minimized window gets focus and the original window was maximized, 
			  the minimzed window gets maximized and the original window gets normalized. 
			  If the ex-minimized window gets deactivated, it will be normalized.
		*/
			Form form = wm.form;

			if (setting_windowstates) {
				return false;
			}
			
			if (!form.Visible)
				return false;
			
			bool is_active = wm.IsActive();
			bool maximize_this = false;
			
			if (!is_active){
				return false;
			}

			setting_windowstates = true;
			foreach (Form frm in mdi_child_list) {
				if (frm == form) {
					continue;
				} else if (!frm.Visible){
					continue;
				}
				if (frm.WindowState == FormWindowState.Maximized && is_active) {
					maximize_this = true;	
					if (((MdiWindowManager) frm.window_manager).was_minimized)
						frm.WindowState = FormWindowState.Minimized;
					else
						frm.WindowState = FormWindowState.Normal;//
				}
			}
			if (maximize_this) {
				wm.was_minimized = form.window_state == FormWindowState.Minimized;
				form.WindowState = FormWindowState.Maximized;
			}
			SetParentText(false);
			
			XplatUI.RequestNCRecalc(ParentForm.Handle);
			XplatUI.RequestNCRecalc (Handle);

			SizeScrollBars ();

			setting_windowstates = false;

#if NET_2_0
			if (form.MdiParent.MainMenuStrip != null)
				form.MdiParent.MainMenuStrip.RefreshMdiItems ();
#endif

			return maximize_this;
		}

		internal int ChildrenCreated {
			get { return mdi_created; }
			set { mdi_created = value; }
		}

		internal Form ActiveMdiChild {
			get {
#if NET_2_0
				if (!ParentForm.Visible)
					return null;
#endif
				if (Controls.Count < 1)
					return null;
					
				if (!ParentForm.IsHandleCreated)
					return null;
				
				if (!ParentForm.has_been_visible)
					return null;
					
				if (!ParentForm.Visible)
					return active_child;
				
				active_child = null;
				for (int i = 0; i < Controls.Count; i++) {
					if (Controls [i].Visible) {
						active_child = (Form) Controls [i];
						break;
					}
				}
				return active_child;
			}
			set {
				ActivateChild (value);
			}
		}
		
		internal void ActivateActiveMdiChild ()
		{
			if (ParentForm.is_changing_visible_state)
				return;
				
			for (int i = 0; i < Controls.Count; i++) {
				if (Controls [i].Visible) {
					ActivateChild ((Form) Controls [i]);
					return;
				}
			}
		}
	}
}

