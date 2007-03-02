//
// ToolStripMenuItem.cs
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
using System.Windows.Forms.Design;

namespace System.Windows.Forms
{
	[ToolStripItemDesignerAvailability (ToolStripItemDesignerAvailability.MenuStrip | ToolStripItemDesignerAvailability.ContextMenuStrip)]
	public class ToolStripMenuItem : ToolStripDropDownItem
	{
		private CheckState checked_state;
		private bool check_on_click;
		private string shortcut_display_string;
		private Keys shortcut_keys = Keys.None;
		private bool show_shortcut_keys = true;
		private Form mdi_client_form;

		#region Public Constructors
		public ToolStripMenuItem ()
			: this (null, null, null, string.Empty)
		{
		}

		public ToolStripMenuItem (Image image)
			: this (null, image, null, string.Empty)
		{
		}

		public ToolStripMenuItem (string text)
			: this (text, null, null, string.Empty)
		{
		}

		public ToolStripMenuItem (string text, Image image)
			: this (text, image, null, string.Empty)
		{
		}

		public ToolStripMenuItem (string text, Image image, EventHandler onClick)
			: this (text, image, onClick, string.Empty)
		{
		}

		public ToolStripMenuItem (string text, Image image, params ToolStripItem[] dropDownItems)
			: this (text, image, null, string.Empty)
		{
			if (dropDownItems != null)
				foreach (ToolStripItem tsi in dropDownItems)
					this.DropDownItems.Add (tsi);
		}

		public ToolStripMenuItem (string text, Image image, EventHandler onClick, Keys shortcutKeys)
			: this (text, image, onClick, string.Empty)
		{
		}

		public ToolStripMenuItem (string text, Image image, EventHandler onClick, string name)
			: base (text, image, onClick, name)
		{
			base.Overflow = ToolStripItemOverflow.Never;
		}
		#endregion

		#region Public Properties
		[Bindable (true)]
		[DefaultValue (false)]
		[RefreshProperties (RefreshProperties.All)]
		public bool Checked {
			get {
				switch (this.checked_state) {
					case CheckState.Unchecked:
					default:
						return false;
					case CheckState.Checked:
					case CheckState.Indeterminate:
						return true;
				}
			}
			set {
				if (this.checked_state != (value ? CheckState.Checked : CheckState.Unchecked)) {
					this.checked_state = value ? CheckState.Checked : CheckState.Unchecked;
					this.Invalidate ();
					this.OnCheckedChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue (false)]
		public bool CheckOnClick {
			get { return this.check_on_click; }
			set { this.check_on_click = value; }
		}

		[Bindable (true)]
		[DefaultValue (CheckState.Unchecked)]
		[RefreshProperties (RefreshProperties.All)]
		public CheckState CheckState {
			get { return this.checked_state; }
			set
			{
				if (!Enum.IsDefined (typeof (CheckState), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for CheckState", value));

				this.checked_state = value;
				this.Invalidate ();
				this.OnCheckStateChanged (EventArgs.Empty);
			}
		}

		public override bool Enabled {
			get { return base.Enabled; }
			set { base.Enabled = value; }
		}

		[Browsable (false)]
		public bool IsMdiWindowListEntry {
			get { return this.mdi_client_form != null; }
		}
		
		[DefaultValue (ToolStripItemOverflow.Never)]
		public new ToolStripItemOverflow Overflow {
			get { return base.Overflow; }
			set { base.Overflow = value; }
		}
		
		[MonoTODO ("Renderer doesn't support shortcut keys yet, they will never show.")]
		[Localizable (true)]
		[DefaultValue (true)]
		public bool ShowShortcutKeys {
			get { return this.show_shortcut_keys; }
			set { this.show_shortcut_keys = value; }
		}
		
		[MonoTODO ("Keyboard navigation not implemented.")]
		[Localizable (true)]
		[DefaultValue (null)]
		public string ShortcutKeyDisplayString {
			get { return this.shortcut_display_string; }
			set { this.shortcut_display_string = value; }
		}
		
		[MonoTODO ("Keyboard navigation not implemented.")]
		[Localizable (true)]
		[DefaultValue (Keys.None)]
		public Keys ShortcutKeys {
			get { return this.shortcut_keys; }
			set { this.shortcut_keys = value; }
		}
		#endregion

		#region Protected Properties
		protected internal override Padding DefaultMargin {
			get { return new Padding (0); }
		}

		protected override Padding DefaultPadding {
			get { return new Padding (4, 0, 4, 0); }
		}

		protected override Size DefaultSize {
			get { return new Size (32, 19); }
		}
		#endregion

		#region Protected Methods
		protected override ToolStripDropDown CreateDefaultDropDown ()
		{
			ToolStripDropDownMenu tsddm = new ToolStripDropDownMenu ();
			tsddm.OwnerItem = this;
			return tsddm;
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		protected virtual void OnCheckedChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)Events [CheckedChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCheckStateChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)Events [CheckStateChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected override void OnClick (EventArgs e)
		{
			if (!this.Enabled)
				return;
				
			if (this.IsOnDropDown) {
				if (this.HasDropDownItems)
					return;

				this.HideDropDown (ToolStripDropDownCloseReason.ItemClicked);
				
				(this.Parent as ToolStripDropDown).Close (ToolStripDropDownCloseReason.ItemClicked);
				ToolStripManager.FireAppFocusChanged (this.Parent);

				Object parent = this.Parent;

				// Find the top level MenuStrip to inform it to close all open
				// dropdowns because this one was clicked
				while (parent != null) {
					if (parent is MenuStrip)
						(parent as MenuStrip).HideMenus (true, ToolStripDropDownCloseReason.ItemClicked);

					if (parent is ToolStripDropDown)
						parent = (parent as ToolStripDropDown).OwnerItem;
					else if (parent is ToolStripItem)
						parent = (parent as ToolStripItem).Parent;
					else
						break;
				}
			}

			if (this.IsMdiWindowListEntry) {
				this.mdi_client_form.MdiParent.MdiContainer.ActivateChild (this.mdi_client_form);
				return;
			}
			
			if (this.check_on_click)
				this.Checked = !this.Checked;

			base.OnClick (e);
		}

		protected override void OnDropDownHide (EventArgs e)
		{
			base.OnDropDownHide (e);
		}

		protected override void OnDropDownShow (EventArgs e)
		{
			base.OnDropDownShow (e);
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			if (this.HasDropDownItems && Enabled)
				if (!this.DropDown.Visible)
					this.ShowDropDown ();

			base.OnMouseDown (e);
		}

		protected override void OnMouseEnter (EventArgs e)
		{
			if (this.IsOnDropDown && this.HasDropDownItems && Enabled)
				this.ShowDropDown ();

			base.OnMouseEnter (e);
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave (e);
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			if (!this.HasDropDownItems && Enabled)
				base.OnMouseUp (e);
		}

		protected override void OnOwnerChanged (EventArgs e)
		{
			base.OnOwnerChanged (e);
		}

		protected override void OnPaint (System.Windows.Forms.PaintEventArgs e)
		{
			base.OnPaint (e);

			if (this.Owner != null) {
				Color font_color = this.Enabled ? this.ForeColor : SystemColors.GrayText;
				Image draw_image = this.Enabled ? this.Image : ToolStripRenderer.CreateDisabledImage (this.Image);

				if (this.IsOnDropDown)
					this.Owner.Renderer.DrawMenuItemBackground (new System.Windows.Forms.ToolStripItemRenderEventArgs (e.Graphics, this));
				else
					this.Owner.Renderer.DrawButtonBackground (new System.Windows.Forms.ToolStripItemRenderEventArgs (e.Graphics, this));

				Rectangle text_layout_rect;
				Rectangle image_layout_rect;

				this.CalculateTextAndImageRectangles (out text_layout_rect, out image_layout_rect);

				if (this.IsOnDropDown) {
					text_layout_rect = new Rectangle (35, text_layout_rect.Top, text_layout_rect.Width, text_layout_rect.Height);
					if (image_layout_rect != Rectangle.Empty)
						image_layout_rect = new Rectangle (4, 3, draw_image.Width, draw_image.Height);

					if (this.Checked)
						this.Owner.Renderer.DrawItemCheck (new ToolStripItemImageRenderEventArgs (e.Graphics, this, new Rectangle (2, 1, 19, 19)));
				}
				if (text_layout_rect != Rectangle.Empty)
					this.Owner.Renderer.DrawItemText (new System.Windows.Forms.ToolStripItemTextRenderEventArgs (e.Graphics, this, this.Text, text_layout_rect, font_color, this.Font, this.TextAlign));

				if (image_layout_rect != Rectangle.Empty)
					this.Owner.Renderer.DrawItemImage (new System.Windows.Forms.ToolStripItemImageRenderEventArgs (e.Graphics, this, draw_image, image_layout_rect));

				if (this.IsOnDropDown && this.HasDropDownItems)
					this.Owner.Renderer.DrawArrow (new ToolStripArrowRenderEventArgs (e.Graphics, this, new Rectangle (this.Bounds.Width - 17, 2, 10, 20), Color.Black, ArrowDirection.Right));
				return;
			}
		}

		protected internal override void SetBounds (Rectangle bounds)
		{
			base.SetBounds (bounds);
		}
		#endregion

		#region Public Events
		static object CheckedChangedEvent = new object ();
		static object CheckStateChangedEvent = new object ();

		public event EventHandler CheckedChanged {
			add { Events.AddHandler (CheckedChangedEvent, value); }
			remove {Events.RemoveHandler (CheckedChangedEvent, value); }
		}

		public event EventHandler CheckStateChanged {
			add { Events.AddHandler (CheckStateChangedEvent, value); }
			remove {Events.RemoveHandler (CheckStateChangedEvent, value); }
		}
		#endregion

		#region Internal Properties
		internal Form MdiClientForm {
			get { return this.mdi_client_form; }
			set { this.mdi_client_form = value; }
		}
		#endregion
	}
}
#endif
