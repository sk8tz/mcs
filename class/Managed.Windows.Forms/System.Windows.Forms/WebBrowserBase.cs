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
// Copyright (c) 2007, 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita	<avidigal@novell.com>

#if NET_2_0

#undef debug

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace System.Windows.Forms
{
	[MonoTODO ("Needs Implementation")]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    [ComVisible(true)]
    public abstract class WebBrowserBase : Control
	{
		internal bool documentReady;

		#region Public Properties
		[MonoTODO ("Stub, not implemented")]
		[Browsable (false)] 
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Object ActiveXInstance {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO ("Stub, not implemented")]
		[Browsable (false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public override bool AllowDrop {
			get { return false; }
			set { throw new NotSupportedException (); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override Color BackColor {
			get { return base.BackColor; }
			set { base.BackColor = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Cursor Cursor {
			get { return null; }
			set { throw new NotSupportedException (); }
		}

		[Browsable (false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new bool Enabled {
			get { return true; }
			set { throw new NotSupportedException (); }
		}

		[Browsable (false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Font Font {
			get { return base.Font; }
			set { base.Font = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Color ForeColor {
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ImeMode ImeMode {
			get { return base.ImeMode; }
			set { base.ImeMode = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Localizable (true)]
		public new virtual RightToLeft RightToLeft {
			get { return base.RightToLeft; }
			set { base.RightToLeft = value; }
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public override ISite Site {
			set { base.Site = value; }
		}

		[BindableAttribute(true)] 
		[Browsable (false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string Text {
			get { return String.Empty; }
			set { throw new NotSupportedException (); }
		}

		[Browsable (false)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new bool UseWaitCursor {
			get { return false; }
			set { throw new NotSupportedException (); }
		}

		#endregion

		#region Protected Properties
		protected override Size DefaultSize {
			get { return new Size (100, 100); }
		}
		#endregion

		#region Public Methods
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new void DrawToBitmap (Bitmap bitmap, Rectangle targetBounds) 
		{
			throw new NotImplementedException ();
		}

		public override bool  PreProcessMessage(ref Message msg)
		{
 			 return base.PreProcessMessage(ref msg);
		}

		#endregion

		#region Protected Virtual Methods
		protected virtual void AttachInterfaces (object nativeActiveXObject) 
		{
			throw new NotImplementedException ();
		}

		protected virtual void CreateSink ()
		{
			throw new NotImplementedException ();
		}

		protected virtual WebBrowserSiteBase CreateWebBrowserSiteBase ()
		{
			throw new NotImplementedException ();
		}

		protected virtual void DetachInterfaces ()
		{
			throw new NotImplementedException ();
		}

		protected virtual void DetachSink ()
		{
			throw new NotImplementedException ();
		}
		#endregion

		#region Protected Overriden Methods
		protected override void Dispose (bool disposing)
		{
			WebHost.Shutdown ();
			base.Dispose (disposing);
		}

		protected override bool IsInputChar (char charCode)
		{
			return base.IsInputChar (charCode);
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			base.OnBackColorChanged (e);
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
		}

		protected override void OnForeColorChanged (EventArgs e)
		{
			base.OnForeColorChanged (e);
		}

		protected override void OnGotFocus (EventArgs e)
		{
#if debug
			Console.Error.WriteLine ("WebBrowserBase: OnGotFocus");
#endif
			base.OnGotFocus (e);
//			WebHost.FocusIn (Mono.WebBrowser.FocusOption.FocusFirstElement);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
		}

		protected override void OnLostFocus (EventArgs e)
		{
#if debug
			Console.Error.WriteLine ("WebBrowserBase: OnLostFocus");
#endif
			base.OnLostFocus (e);
			WebHost.FocusOut ();
		}

		protected override void OnParentChanged (EventArgs e)
		{
			base.OnParentChanged (e);
		}
		
		protected override void OnRightToLeftChanged (EventArgs e)
		{
			base.OnRightToLeftChanged (e);
		}

		protected override void OnVisibleChanged (EventArgs e)
		{
			base.OnVisibleChanged (e);
			if (Visible && !Disposing && !IsDisposed && state == State.Loaded) {
				state = State.Active;
				webHost.Activate ();
			} else if (!Visible && state == State.Active) {
				state = State.Loaded;
				webHost.Deactivate ();
			}
		}

		protected override bool ProcessMnemonic (char charCode)
		{
			return base.ProcessMnemonic (charCode);
		}

		#endregion

		#region Internal Properties
		enum State
		{
			Unloaded,
			Loaded,
			Active
		}
		private State state;

		private Mono.WebBrowser.IWebBrowser webHost;
		internal Mono.WebBrowser.IWebBrowser WebHost {
			get	{ return webHost; }
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);
			this.webHost.Resize (width, height);
		}
		#endregion

		#region Internal Methods
		internal WebBrowserBase ()
		{
			webHost = Mono.WebBrowser.Manager.GetNewInstance ();
			bool loaded = webHost.Load (this.Handle, this.Width, this.Height);
			if (!loaded)
				return;
				
			state = State.Loaded;

			webHost.MouseClick += new Mono.WebBrowser.DOM.NodeEventHandler (OnWebHostMouseClick);
			webHost.Focus += new EventHandler (OnWebHostFocus);
			webHost.CreateNewWindow += new Mono.WebBrowser.CreateNewWindowEventHandler (OnWebHostCreateNewWindow);
			webHost.Alert += new Mono.WebBrowser.AlertEventHandler (OnWebHostAlert);
			webHost.Completed += new EventHandler (OnWebHostCompleted);
		}

		void OnWebHostAlert (object sender, Mono.WebBrowser.AlertEventArgs e)
		{
			switch (e.Type) {
				case Mono.WebBrowser.DialogType.Alert:
					MessageBox.Show (e.Text, e.Title);
					break;
				case Mono.WebBrowser.DialogType.AlertCheck:
					WebBrowserDialogs.AlertCheck form1 = new WebBrowserDialogs.AlertCheck (e.Title, e.Text, e.CheckMessage, e.CheckState);
					form1.Show ();
					e.CheckState = form1.Checked;
					e.BoolReturn = true;
					break;
				case Mono.WebBrowser.DialogType.Confirm:
					DialogResult r1 = MessageBox.Show (e.Text, e.Title, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
					e.BoolReturn = (r1 == DialogResult.OK);
					break;
				case Mono.WebBrowser.DialogType.ConfirmCheck:
					WebBrowserDialogs.ConfirmCheck form2 = new WebBrowserDialogs.ConfirmCheck (e.Title, e.Text, e.CheckMessage, e.CheckState);
					DialogResult r2 = form2.Show ();
					e.CheckState = form2.Checked;
					e.BoolReturn = (r2 == DialogResult.OK);
					break;
				case Mono.WebBrowser.DialogType.ConfirmEx:
					MessageBox.Show (e.Text, e.Title);
					break;
				case Mono.WebBrowser.DialogType.Prompt:
					WebBrowserDialogs.Prompt form4 = new WebBrowserDialogs.Prompt (e.Title, e.Text, e.Text2);
					DialogResult r4 = form4.Show ();
					e.StringReturn = form4.Text;
					e.BoolReturn = (r4 == DialogResult.OK);
					break;
				case Mono.WebBrowser.DialogType.PromptPassword:
					MessageBox.Show (e.Text, e.Title);
					break;
				case Mono.WebBrowser.DialogType.PromptUsernamePassword:
					MessageBox.Show (e.Text, e.Title);
					break;
				case Mono.WebBrowser.DialogType.Select:
					MessageBox.Show (e.Text, e.Title);
					break;
			}
			
		}



		#region Events raised by the embedded web browser
		bool OnWebHostCreateNewWindow (object sender, Mono.WebBrowser.CreateNewWindowEventArgs e)
		{
			return OnNewWindowInternal ();
		}


		protected override void OnResize (EventArgs e)
		{

			base.OnResize (e);

			if (state == State.Active)
				this.webHost.Resize (this.Width, this.Height);

		}

		private void OnWebHostMouseClick (object sender, EventArgs e)
		{
			//MessageBox.Show ("clicked");
		}

		/// <summary>
		/// Event raised from the embedded webbrowser control, saying that it has received focus
		/// (via a mouse click, for instance).
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnWebHostFocus (object sender, EventArgs e)
		{
#if debug
			Console.Error.WriteLine ("WebBrowserBase: OnWebHostFocus");
#endif
			this.Focus ();
		}
		
		#endregion

		internal abstract bool OnNewWindowInternal ();
		internal abstract void OnWebHostCompleted (object sender, EventArgs e);
		#endregion

		
		
#region Events
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackColorChanged {
			add { throw new NotSupportedException ("Invalid event handler for BackColorChanged"); }
			remove { }
		}

		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { throw new NotSupportedException ("Invalid event handler for BackgroundImageChanged"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add { throw new NotSupportedException ("Invalid event handler for BackgroundImageLayoutChanged"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BindingContextChanged {
			add { throw new NotSupportedException ("Invalid event handler for BindingContextChanged"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event UICuesEventHandler ChangeUICues {
			add { throw new NotSupportedException ("Invalid event handler for ChangeUICues"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler Click {
			add { throw new NotSupportedException ("Invalid event handler for Click"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler CursorChanged {
			add { throw new NotSupportedException ("Invalid event handler for CursorChanged"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DoubleClick {
			add { throw new NotSupportedException ("Invalid event handler for DoubleClick"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event DragEventHandler DragDrop {
			add { throw new NotSupportedException ("Invalid event handler for DragDrop"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event DragEventHandler DragEnter {
			add { throw new NotSupportedException ("Invalid event handler for DragEnter"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DragLeave {
			add { throw new NotSupportedException ("Invalid event handler for DragLeave"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event DragEventHandler DragOver {
			add { throw new NotSupportedException ("Invalid event handler for DragOver"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler EnabledChanged {
			add { throw new NotSupportedException ("Invalid event handler for EnabledChanged"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler Enter {
			add { throw new NotSupportedException ("Invalid event handler for Enter"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler FontChanged {
			add { throw new NotSupportedException ("Invalid event handler for FontChanged"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add { throw new NotSupportedException ("Invalid event handler for ForeColorChanged"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event GiveFeedbackEventHandler GiveFeedback {
			add { throw new NotSupportedException ("Invalid event handler for GiveFeedback"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event HelpEventHandler HelpRequested {
			add { throw new NotSupportedException ("Invalid event handler for HelpRequested"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged {
			add { throw new NotSupportedException ("Invalid event handler for ImeModeChanged"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyDown {
			add { throw new NotSupportedException ("Invalid event handler for KeyDown"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event KeyPressEventHandler KeyPress {
			add { throw new NotSupportedException ("Invalid event handler for KeyPress"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyUp {
			add { throw new NotSupportedException ("Invalid event handler for KeyUp"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event LayoutEventHandler Layout {
			add { throw new NotSupportedException ("Invalid event handler for Layout"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler Leave {
			add { throw new NotSupportedException ("Invalid event handler for Leave"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler MouseCaptureChanged {
			add { throw new NotSupportedException ("Invalid event handler for MouseCaptureChanged"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseClick {
			add { throw new NotSupportedException ("Invalid event handler for MouseClick"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseDoubleClick {
			add { throw new NotSupportedException ("Invalid event handler for MouseDoubleClick"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseDown {
			add { throw new NotSupportedException ("Invalid event handler for MouseDown"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler MouseEnter {
			add { throw new NotSupportedException ("Invalid event handler for MouseEnter"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler MouseHover {
			add { throw new NotSupportedException ("Invalid event handler for MouseHover"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler MouseLeave {
			add { throw new NotSupportedException ("Invalid event handler for MouseLeave"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseMove {
			add { throw new NotSupportedException ("Invalid event handler for MouseMove"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseUp {
			add { throw new NotSupportedException ("Invalid event handler for MouseUp"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MouseEventHandler MouseWheel {
			add { throw new NotSupportedException ("Invalid event handler for MouseWheel"); }
			remove { }
		}
		
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint {
			add { throw new NotSupportedException ("Invalid event handler for Paint"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event QueryAccessibilityHelpEventHandler QueryAccessibilityHelp {
			add { throw new NotSupportedException ("Invalid event handler for QueryAccessibilityHelp"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event QueryContinueDragEventHandler QueryContinueDrag {
			add { throw new NotSupportedException ("Invalid event handler for QueryContinueDrag"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler RightToLeftChanged {
			add { throw new NotSupportedException ("Invalid event handler for RightToLeftChanged"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler StyleChanged {
			add { throw new NotSupportedException ("Invalid event handler for StyleChanged"); }
			remove { }
		}
		
		[BrowsableAttribute(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { throw new NotSupportedException ("Invalid event handler for TextChanged"); }
			remove { }
		}
		
		
		
		
#endregion
	}
}

#endif