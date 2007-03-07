//
// ToolStripDropDown.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

#if NET_2_0
using System.Drawing;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace System.Windows.Forms
{
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public class ToolStripDropDown : ToolStrip
	{
		private bool allow_transparency;
		private bool auto_close;
		private bool drop_shadow_enabled = true;
		private double opacity = 1D;
		private ToolStripItem owner_item;

		#region Public Constructor
		public ToolStripDropDown () : base ()
		{
			SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			SetStyle (ControlStyles.ResizeRedraw, true);

			this.auto_close = true;
			is_visible = false;
			this.GripStyle = ToolStripGripStyle.Hidden;
		}
		#endregion

		#region Public Properties
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool AllowTransparency {
			get { return allow_transparency; }
			set {
				if (value == allow_transparency)
					return;

				if ((XplatUI.SupportsTransparency () & TransparencySupport.Set) != 0) {
					allow_transparency = value;

					if (this.IsHandleCreated) {
						if (value) 
							XplatUI.SetWindowTransparency (Handle, Opacity, Color.Empty);
						else
							UpdateStyles (); // Remove the WS_EX_LAYERED style
					}
				}
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override AnchorStyles Anchor {
			get { return base.Anchor; }
			set { base.Anchor = value; }
		}

		[DefaultValue (true)]
		public bool AutoClose
		{
			get { return this.auto_close; }
			set { this.auto_close = value; }
		}

		[DefaultValue (true)]
		public override bool AutoSize {
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ContextMenu ContextMenu {
			get { return null; }
			set { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ContextMenuStrip ContextMenuStrip {
			get { return null; }
			set { }
		}

		public override ToolStripDropDownDirection DefaultDropDownDirection {
			get { return base.DefaultDropDownDirection; }
			set { base.DefaultDropDownDirection = value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DefaultValue (DockStyle.None)]
		public override DockStyle Dock {
			get { return base.Dock; }
			set { base.Dock = value; }
		}
		
		public bool DropShadowEnabled {
			get { return this.drop_shadow_enabled; }
			set {
				if (this.drop_shadow_enabled == value)
					return;
					
				this.drop_shadow_enabled = value;
				UpdateStyles ();	// Re-CreateParams
			}
		}

		public override Font Font {
			get { return base.Font; }
			set { base.Font = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ToolStripGripDisplayStyle GripDisplayStyle {
			get { return ToolStripGripDisplayStyle.Vertical; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Padding GripMargin {
			get { return Padding.Empty; }
			set { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new Rectangle GripRectangle {
			get { return Rectangle.Empty; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DefaultValue (ToolStripGripStyle.Hidden)]
		public new ToolStripGripStyle GripStyle {
			get { return base.GripStyle; }
			set { base.GripStyle = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Point Location {
			get { return base.Location; }
			set { base.Location = value; }
		}

		[DefaultValue (1D)]
		[TypeConverter (typeof (OpacityConverter))]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public double Opacity {
			get { return this.opacity; }
			set {
					if (this.opacity == value)
						return;
						
					this.opacity = value;
					this.allow_transparency = true;
					
					if (this.IsHandleCreated) {
						UpdateStyles ();
						XplatUI.SetWindowTransparency (Handle, opacity, Color.Empty);
					}
			}
		}

		[Browsable (false)]
		[DefaultValue (null)]
		public ToolStripItem OwnerItem {
			get { return this.owner_item; }
			set { this.owner_item = value; 
				
				if (this.owner_item != null)
					if (this.owner_item.Owner != null)
						this.Renderer = this.owner_item.Owner.Renderer;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new Region Region {
			get { return base.Region; }
			set { base.Region = value; }
		}

		[Localizable (true)]
		[AmbientValue (RightToLeft.Inherit)]
		public override RightToLeft RightToLeft {
			get { return base.RightToLeft; }
			set { base.RightToLeft = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new bool Stretch {
			get { return false; }
			set { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new int TabIndex {
			get { return 0; }
			set { }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public bool TopLevel {
			get { return GetTopLevel (); }
			set { SetTopLevel (value); }
		}
		
		[Browsable (false)]
		[Localizable (true)]
		[DefaultValue (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new bool Visible {
			get { return base.Visible; }
			set { base.Visible = value; }
		}
		#endregion

		#region Protected Properties
		protected override CreateParams CreateParams {
			get {
				CreateParams cp = base.CreateParams;

				cp.Style = unchecked ((int)(WindowStyles.WS_POPUP | WindowStyles.WS_CLIPCHILDREN));
				cp.ClassStyle |= (int)XplatUIWin32.ClassStyle.CS_DROPSHADOW;
				cp.ExStyle |= (int)(WindowExStyles.WS_EX_TOOLWINDOW | WindowExStyles.WS_EX_TOPMOST);

				if (Opacity < 1.0 && allow_transparency)
					cp.ExStyle |= (int)WindowExStyles.WS_EX_LAYERED;
				if (TopMost)
					cp.ExStyle |= (int) WindowExStyles.WS_EX_TOPMOST;

				return cp;
			}
		}

		protected override DockStyle DefaultDock {
			get { return DockStyle.None; }
		}

		protected override Padding DefaultPadding {
			get { return new Padding (1, 2, 1, 2); }
		}

		protected override bool DefaultShowItemToolTips {
			get { return true; }
		}

		//protected internal override Size MaxItemSize {
		//        get {  return new Size (Screen.PrimaryScreen.Bounds.Width - 2, Screen.PrimaryScreen.Bounds.Height - 34); }
		//}

		protected virtual bool TopMost {
			get { return true; }
		}
		#endregion

		#region Public Methods
		public void Close ()
		{
			this.Close (ToolStripDropDownCloseReason.CloseCalled);
		}

		public void Close (ToolStripDropDownCloseReason reason)
		{
			// Give users a chance to cancel the close
			ToolStripDropDownClosingEventArgs e = new ToolStripDropDownClosingEventArgs (reason);
			this.OnClosing (e);

			if (e.Cancel)
				return;

			// Don't actually close if AutoClose == true unless explicitly called
			if (!this.auto_close && reason != ToolStripDropDownCloseReason.CloseCalled)
				return;

			// Detach from the tracker
			ToolStripManager.AppClicked -= new EventHandler (ToolStripMenuTracker_AppClicked); ;
			ToolStripManager.AppFocusChange -= new EventHandler (ToolStripMenuTracker_AppFocusChange);

			// Hide this dropdown
			this.Hide ();

			// Owner MenuItem needs to be told to redraw (it's no longer selected)
			if (owner_item != null)
				owner_item.Invalidate ();

			// Recursive hide all child dropdowns
			foreach (ToolStripItem tsi in this.Items)
				if (tsi is ToolStripMenuItem)
					(tsi as ToolStripMenuItem).HideDropDown (reason);
			
			this.OnClosed (new ToolStripDropDownClosedEventArgs (reason));
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new void Show ()
		{
			CancelEventArgs e = new CancelEventArgs ();
			this.OnOpening (e);
			
			if (e.Cancel)
				return;

			// The tracker lets us know when the form is clicked or loses focus
			ToolStripManager.AppClicked += new EventHandler (ToolStripMenuTracker_AppClicked);
			ToolStripManager.AppFocusChange += new EventHandler (ToolStripMenuTracker_AppFocusChange);

			base.Show ();
			
			this.OnOpened (EventArgs.Empty);
		}
		
		public void Show (Point screenLocation)
		{
			this.Location = screenLocation;
			Show ();
		}
		
		public void Show (Control control, Point position)
		{
			if (control == null)
				throw new ArgumentNullException ("control");
				
			this.Location = control.PointToScreen (position);
			Show ();
		}
		
		public void Show (int x, int y)
		{
			this.Location = new Point (x, y);
			Show ();
		}
		
		public void Show (Point position, ToolStripDropDownDirection direction)
		{
			this.PerformLayout ();
			
			Point show_point = position;

			switch (direction) {
				case ToolStripDropDownDirection.AboveLeft:
					show_point.Y -= this.Height;
					show_point.X -= this.Width;
					break;
				case ToolStripDropDownDirection.AboveRight:
					show_point.Y -= this.Height;
					break;
				case ToolStripDropDownDirection.BelowLeft:
					show_point.X -= this.Width;
					break;
				case ToolStripDropDownDirection.Left:
					show_point.X -= this.Width;
					break;
				case ToolStripDropDownDirection.Right:
					break;
			}
			
			if (this.Location != show_point)
				this.Location = show_point;
				
			Show ();
		}
		
		public void Show (Control control, int x, int y)
		{
			if (control == null)
				throw new ArgumentNullException ("control");

			Show (control, new Point (x, y));
		}
		
		public void Show (Control control, Point position, ToolStripDropDownDirection direction)
		{
			if (control == null)
				throw new ArgumentNullException ("control");

			Show (control.PointToScreen (position), direction);
		}
		#endregion

		#region Protected Methods
		protected override void CreateHandle ()
		{
			base.CreateHandle ();
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		protected virtual void OnClosed (ToolStripDropDownClosedEventArgs e)
		{
			ToolStripDropDownClosedEventHandler eh = (ToolStripDropDownClosedEventHandler)(Events [ClosedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnClosing (ToolStripDropDownClosingEventArgs e)
		{
			ToolStripDropDownClosingEventHandler eh = (ToolStripDropDownClosingEventHandler)(Events [ClosingEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
		}

		protected override void OnItemClicked (ToolStripItemClickedEventArgs e)
		{
			base.OnItemClicked (e);
		}

		protected override void OnLayout (LayoutEventArgs e)
		{
			// Find the widest menu item
			int widest = 0;

			foreach (ToolStripItem tsi in this.Items) {
				if (!tsi.Available) 
					continue;
					
				tsi.SetPlacement (ToolStripItemPlacement.Main);
				
				if (tsi.GetPreferredSize (Size.Empty).Width > widest)
					widest = tsi.GetPreferredSize (Size.Empty).Width;
			}
			
			int x = this.Padding.Left;
			widest += 68 - this.Padding.Horizontal;
			int y = this.Padding.Top;

			foreach (ToolStripItem tsi in this.Items) {
				if (!tsi.Available)
					continue;

				y += tsi.Margin.Top;

				int height = 0;

				if (tsi is ToolStripSeparator)
					height = 7;
				else
					height = 22;

				tsi.SetBounds (new Rectangle (x, y, widest, height));
				y += tsi.Height + tsi.Margin.Bottom;
			}

			this.Size = new Size (widest + this.Padding.Horizontal, y + this.Padding.Bottom);// + 2);
			this.SetDisplayedItems ();
			this.OnLayoutCompleted (EventArgs.Empty);
			this.Invalidate ();
		}

		protected override void OnMouseUp (MouseEventArgs mea)
		{
			base.OnMouseUp (mea);
		}

		protected virtual void OnOpened (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [OpenedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnOpening (CancelEventArgs e)
		{
			CancelEventHandler eh = (CancelEventHandler)(Events [OpeningEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnParentChanged (EventArgs e)
		{
			base.OnParentChanged (e);
			
			if (Parent is ToolStrip)
				this.Renderer = (Parent as ToolStrip).Renderer;
		}

		protected override void OnVisibleChanged (EventArgs e)
		{
			base.OnVisibleChanged (e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override bool ProcessDialogChar (char charCode)
		{
			return base.ProcessDialogChar (charCode);
		}

		protected override bool ProcessDialogKey (Keys keyData)
		{
			return base.ProcessDialogKey (keyData);
		}

		protected override bool ProcessMnemonic (char charCode)
		{
			return base.ProcessMnemonic (charCode);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		protected override void ScaleCore (float dx, float dy)
		{
			base.ScaleCore (dx, dy);
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);
		}

		protected override void SetVisibleCore (bool value)
		{
			base.SetVisibleCore (value);
		}

		protected override void WndProc (ref Message m)
		{
			const int MA_NOACTIVATE = 0x0003;

			// Don't activate when the WM tells us to
			if ((Msg)m.Msg == Msg.WM_MOUSEACTIVATE) {
				m.Result = (IntPtr)MA_NOACTIVATE;
				return;
			}

			base.WndProc (ref m);
		}
		#endregion

		#region Public Events
		static object ClosedEvent = new object ();
		static object ClosingEvent = new object ();
		static object OpenedEvent = new object ();
		static object OpeningEvent = new object ();
		static object ScrollEvent = new object ();

		[Browsable (false)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		[Browsable (false)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}

		[Browsable (false)]
		public new event EventHandler BindingContextChanged {
			add { base.BindingContextChanged += value; }
			remove { base.BindingContextChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event UICuesEventHandler ChangeUICues {
			add { base.ChangeUICues += value; }
			remove { base.ChangeUICues -= value; }
		}

		public event ToolStripDropDownClosedEventHandler Closed {
			add { Events.AddHandler (ClosedEvent, value); }
			remove { Events.RemoveHandler (ClosedEvent, value); }
		}

		public event ToolStripDropDownClosingEventHandler Closing {
			add { Events.AddHandler (ClosingEvent, value); }
			remove { Events.RemoveHandler (ClosingEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ContextMenuChanged {
			add { base.ContextMenuChanged += value; }
			remove { base.ContextMenuChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler ContextMenuStripChanged {
			add { base.ContextMenuStripChanged += value; }
			remove { base.ContextMenuStripChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler DockChanged {
			add { base.DockChanged += value; }
			remove { base.DockChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler Enter {
			add { base.Enter += value; }
			remove { base.Enter -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler FontChanged {
			add { base.FontChanged += value; }
			remove { base.FontChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event GiveFeedbackEventHandler GiveFeedback {
			add { base.GiveFeedback += value; }
			remove { base.GiveFeedback -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event HelpEventHandler HelpRequested {
			add { base.HelpRequested += value; }
			remove { base.HelpRequested -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler ImeModeChanged {
			add { base.ImeModeChanged += value; }
			remove { base.ImeModeChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event KeyEventHandler KeyDown {
			add { base.KeyDown += value; }
			remove { base.KeyDown -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event KeyPressEventHandler KeyPress {
			add { base.KeyPress += value; }
			remove { base.KeyPress -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event KeyEventHandler KeyUp {
			add { base.KeyUp += value; }
			remove { base.KeyUp -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler Leave {
			add { base.Leave += value; }
			remove { base.Leave -= value; }
		}

		public event EventHandler Opened {
			add { Events.AddHandler (OpenedEvent, value); }
			remove { Events.RemoveHandler (OpenedEvent, value); }
		}

		public event CancelEventHandler Opening {
			add { Events.AddHandler (OpeningEvent, value); }
			remove { Events.RemoveHandler (OpeningEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler RegionChanged {
			add { base.RegionChanged += value; }
			remove { base.RegionChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event ScrollEventHandler Scroll {
			add { Events.AddHandler (ScrollEvent, value); }
			remove { Events.RemoveHandler (ScrollEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler StyleChanged {
			add { base.StyleChanged += value; }
			remove { base.StyleChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TabIndexChanged {
			add { base.TabIndexChanged += value; }
			remove { base.TabIndexChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TabStopChanged {
			add { base.TabStopChanged += value; }
			remove { base.TabStopChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler Validated {
			add { base.Validated += value; }
			remove { base.Validated -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event CancelEventHandler Validating {
			add { base.Validating += value; }
			remove { base.Validating -= value; }
		}
		#endregion

		#region Private Methods
		private void ToolStripMenuTracker_AppFocusChange (object sender, EventArgs e)
		{
			this.Close (ToolStripDropDownCloseReason.AppFocusChange);
		}

		private void ToolStripMenuTracker_AppClicked (object sender, EventArgs e)
		{
			this.Close (ToolStripDropDownCloseReason.AppClicked);
		}
		#endregion

		#region Internal Properties
		internal override bool ActivateOnShow { get { return false; } }
		#endregion
	}
}
#endif
