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
//

// COMPLETE

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[DefaultProperty("Image")]
	[Designer("System.Windows.Forms.Design.PictureBoxDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class PictureBox : Control {

		private Image image;
		private PictureBoxSizeMode size_mode;
		private bool redraw;
		private bool recalc;
		private bool allow_drop;

		private EventHandler frame_handler;

		public PictureBox ()
		{
			redraw = true;
			recalc = true;
			allow_drop = false;
			SetStyle (ControlStyles.Selectable, false);
			SetStyle (ControlStyles.DoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
		}

		[DefaultValue(PictureBoxSizeMode.Normal)]
		[Localizable(true)]
		[RefreshProperties(RefreshProperties.Repaint)]
		public PictureBoxSizeMode SizeMode {
			get { return size_mode; }
			set {
				if (size_mode == value)
					return;
				size_mode = value;
				UpdateSize ();
				Redraw (true);
				Invalidate ();

				OnSizeModeChanged (EventArgs.Empty);
			}
		}

		[DefaultValue(null)]
		[Localizable(true)]
		public Image Image {
			get { return image; }
			set {
				StopAnimation ();

				image = value;
				UpdateSize ();
				if (image != null && ImageAnimator.CanAnimate (image)) {
					frame_handler = new EventHandler (OnAnimateImage);
					ImageAnimator.Animate (image, frame_handler);
				}
				Redraw (true);
				Invalidate ();
			}
		}

		[DefaultValue(BorderStyle.None)]
		[DispId(-504)]
		public BorderStyle BorderStyle {
			get { return InternalBorderStyle; }
			set { InternalBorderStyle = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool CausesValidation {
			get { return base.CausesValidation; }
			set { base.CausesValidation = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new ImeMode ImeMode {
			get { return base.ImeMode; }
			set { base.ImeMode = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override RightToLeft RightToLeft {
			get { return base.RightToLeft; }
			set { base.RightToLeft = value;	}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new int TabIndex	{
			get { return base.TabIndex; }
			set { base.TabIndex = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		[Bindable(false)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}

		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override ImeMode DefaultImeMode {
			get { return base.DefaultImeMode; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Font Font {
			get { return base.Font;	}
			set { base.Font = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Color ForeColor {
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool AllowDrop {
			get {
				return allow_drop;
			}
			set {
				if (allow_drop != value) {
					allow_drop = value;
				}
			}
		}

		#region	Protected Instance Methods
		protected override Size DefaultSize {
			get { return ThemeEngine.Current.PictureBoxDefaultSize; }
		}

		protected override void Dispose (bool disposing)
		{
			if (image != null) {
				StopAnimation ();
				image = null;
			}
			base.Dispose (disposing);
		}

		protected override void OnPaint (PaintEventArgs pe)
		{
                        ThemeEngine.Current.DrawPictureBox (pe.Graphics, pe.ClipRectangle, this);
			base.OnPaint (pe);
		}

		protected override void OnVisibleChanged (EventArgs e)
		{
			base.OnVisibleChanged (e);
			Redraw (true);
		}

		protected virtual void OnSizeModeChanged (EventArgs e)
		{
			if (SizeModeChanged != null)
				SizeModeChanged (this, e);
		}

		protected override void OnEnabledChanged (EventArgs e)
		{
			base.OnEnabledChanged (e);
		}

		protected override void OnParentChanged (EventArgs e)
		{
			base.OnParentChanged (e);
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
			redraw = true;

			if (size_mode == PictureBoxSizeMode.CenterImage || size_mode == PictureBoxSizeMode.StretchImage)
				Refresh ();
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			if (size_mode == PictureBoxSizeMode.AutoSize && image != null) {
				width = image.Width;
				height = image.Height;
			}
			base.SetBoundsCore (x, y, width, height, specified);
		}
		#endregion	// Protected Instance Methods

		#region	Private Methods
		private void StopAnimation ()
		{
			if (frame_handler == null)
				return;
			ImageAnimator.StopAnimate (image, frame_handler);
			frame_handler = null;
		}

		private void UpdateSize ()
		{
			if (image == null)
				return;
			if (size_mode == PictureBoxSizeMode.AutoSize)
				ClientSize = image.Size; 
		}

		private void Redraw (bool recalc)
		{
			redraw = true;
			this.recalc = recalc;
		}

		private void OnAnimateImage (object sender, EventArgs e)
		{
			// This is called from a worker thread,BeginInvoke is used
			// so the control is updated from the correct thread
			BeginInvoke (new EventHandler (UpdateAnimatedImage), new object [] { this, e });
		}

		private void UpdateAnimatedImage (object sender, EventArgs e)
		{
			ImageAnimator.UpdateFrames (image);
			Redraw (false);
			Refresh ();
		}

		#endregion	// Private Methods

		#region Public Instance Methods
		public override string ToString() {
			return base.ToString ();
		}
		#endregion

		#region Events
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler CausesValidationChanged {
			add { base.CausesValidationChanged += value; }
			remove { base.CausesValidationChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler Enter {
			add { base.Enter += value; }
			remove { base.Enter -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler FontChanged {
			add { base.FontChanged += value; }
			remove { base.FontChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged {
			add { base.ImeModeChanged += value; }
			remove { base.ImeModeChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyDown {
			add { base.KeyDown += value; }
			remove { base.KeyDown -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyPressEventHandler KeyPress {
			add { base.KeyPress += value; }
			remove { base.KeyPress -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyUp {
			add { base.KeyUp += value; }
			remove { base.KeyUp -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler Leave {
			add { base.Leave += value; }
			remove { base.Leave -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler RightToLeftChanged {
			add { base.RightToLeftChanged += value; }
			remove { base.RightToLeftChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TabIndexChanged {
			add { base.TabIndexChanged += value; }
			remove { base.TabIndexChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TabStopChanged {
			add { base.TabStopChanged += value; }
			remove { base.TabStopChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		public event EventHandler SizeModeChanged;
		#endregion	// Events

	}
}

