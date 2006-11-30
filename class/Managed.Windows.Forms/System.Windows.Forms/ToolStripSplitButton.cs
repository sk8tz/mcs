//
// ToolStripSplitButton.cs
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

using System;
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms
{
	public class ToolStripSplitButton : ToolStripDropDownItem
	{
		private bool button_pressed;
		private bool button_selected;
		private ToolStripItem default_item;
		private bool drop_down_button_selected;
		private int drop_down_button_width;
		
		#region Public Constructors
		public ToolStripSplitButton()
			: this (string.Empty, null, null, string.Empty)
		{
		}
		
		public ToolStripSplitButton (Image image)
			: this (string.Empty, image, null, string.Empty)
		{
		}
		
		public ToolStripSplitButton (string text)
			: this (text, null, null, string.Empty)
		{
		}
		
		public ToolStripSplitButton (string text, Image image)
			: this (text, image, null, string.Empty)
		{
		}
		
		public ToolStripSplitButton (string text, Image image, EventHandler onClick)
			: this (text, image, onClick, string.Empty)
		{
		}
		
		public ToolStripSplitButton (string text, Image image, params ToolStripItem[] dropDownItems)
			: base (text, image, dropDownItems)
		{
			this.ResetDropDownButtonWidth ();
		}

		public ToolStripSplitButton (string text, Image image, EventHandler onClick, string name)
			: base (text, image, onClick, name)
		{
			this.ResetDropDownButtonWidth ();
		}
		#endregion

		#region Public Properties
		public bool AutoToolTip
		{
			get { return base.AutoToolTip; }
			set { base.AutoToolTip = value; }
		}

		public Rectangle ButtonBounds {
			get { return new Rectangle (this.Bounds.Left, this.Bounds.Top, this.Bounds.Width - this.drop_down_button_width - 1, this.Height); }
		}
		
		public bool ButtonPressed {
			get { return this.button_pressed; }
		}
		
		public bool ButtonSelected {
			get { return base.Selected; }
		}
		
		public Rectangle DropDownButtonBounds {
			get { return new Rectangle (this.Bounds.Right - this.drop_down_button_width, this.Bounds.Top, this.drop_down_button_width, this.Bounds.Height); }
		}
		
		public bool DropDownButtonPressed {
			get { return this.drop_down_button_selected || this.DropDown.Visible; }
		}
		
		public bool DropDownButtonSelected {
			get { return base.Selected; }
		}
		
		[DefaultValue (11)]
		public int DropDownButtonWidth {
			get { return this.drop_down_button_width; }
			set { 
				if (value < 0)
					throw new ArgumentOutOfRangeException ();
					
				this.drop_down_button_width = value;
			}
		}
		
		public Rectangle SplitterBounds {
			get { return new Rectangle (this.Bounds.Width - this.drop_down_button_width - 1, this.Bounds.Top, 1, this.Height); }
		}
		#endregion

		#region Protected Properties
		protected override bool DefaultAutoToolTip {
			get { return true; }
		}

		protected internal override bool DismissWhenClicked {
			get { return true; }
		}
		#endregion

		#region Public Methods
		public override Size GetPreferredSize (Size constrainingSize)
		{
			// base should calculate the button part for us, add the splitter
			// and drop down arrow part to that
			Size s = base.GetPreferredSize (constrainingSize);
			
			s.Width += (this.drop_down_button_width - 2);
			
			return s;
		}
		
		public virtual void OnButtonDoubleClick (EventArgs e)
		{
			if (ButtonDoubleClick != null) ButtonDoubleClick (this, e);
		}
		
		public void PerformButtonClick ()
		{
			if (this.Enabled)
				this.OnButtonClick (EventArgs.Empty);
		}
		
		public virtual void ResetDropDownButtonWidth ()
		{
			this.DropDownButtonWidth = 11;
		}
		#endregion

		#region Protected Methods
		protected override ToolStripDropDown CreateDefaultDropDown ()
		{
			return new ToolStripDropDownMenu ();
		}
		
		protected virtual void OnButtonClick (EventArgs e)
		{
			if (ButtonClick != null) ButtonClick (this, e);
		}
		
		protected virtual void OnDefaultItemChanged (EventArgs e)
		{
			if (DefaultItemChanged != null) DefaultItemChanged (this, e);
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			if (this.ButtonBounds.Contains (e.Location))
			{
				this.button_pressed = true;
				this.Invalidate ();
				base.OnMouseDown (e);
			}
			else if (this.DropDownButtonBounds.Contains (e.Location))
			{
				if (this.DropDown.Visible)
					this.HideDropDown (ToolStripDropDownCloseReason.ItemClicked);
				else
					this.ShowDropDown ();
			
				this.Invalidate ();
			}
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			this.button_selected = false;
			this.drop_down_button_selected = false;
			this.button_pressed = false;
			
			this.Invalidate ();
			
			base.OnMouseLeave (e);
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			this.button_pressed = false;
			this.Invalidate ();
			
			if (this.ButtonBounds.Contains (e.Location))
				base.OnMouseUp (e);
		}
		
		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);

			if (this.Owner != null) {
				Color font_color = this.Enabled ? this.ForeColor : SystemColors.GrayText;
				Image draw_image = this.Enabled ? this.Image : ToolStripRenderer.CreateDisabledImage (this.Image);

				this.Owner.Renderer.DrawSplitButton (new System.Windows.Forms.ToolStripItemRenderEventArgs (e.Graphics, this));

				Rectangle text_layout_rect;
				Rectangle image_layout_rect;

				Rectangle r = this.ContentRectangle;
				r.Width -= (this.drop_down_button_width + 1);
				
				this.CalculateTextAndImageRectangles (r, out text_layout_rect, out image_layout_rect);

				if (text_layout_rect != Rectangle.Empty)
					this.Owner.Renderer.DrawItemText (new System.Windows.Forms.ToolStripItemTextRenderEventArgs (e.Graphics, this, this.Text, text_layout_rect, font_color, this.Font, this.TextAlign));
				if (image_layout_rect != Rectangle.Empty)
					this.Owner.Renderer.DrawItemImage (new System.Windows.Forms.ToolStripItemImageRenderEventArgs (e.Graphics, this, draw_image, image_layout_rect));

				this.Owner.Renderer.DrawArrow (new ToolStripArrowRenderEventArgs (e.Graphics, this, new Rectangle (this.Width - 9, 1, 6, this.Height), Color.Black, ArrowDirection.Down));
				
				return;
			}
		}

		#endregion
		
		#region Public Events
		public event EventHandler ButtonClick;
		public event EventHandler ButtonDoubleClick;
		public event EventHandler DefaultItemChanged;
		#endregion
	}
}
#endif