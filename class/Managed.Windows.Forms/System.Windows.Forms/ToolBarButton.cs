// System.Windows.Forms.ToolBarButton.cs
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
// Copyright (C) 2004-2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Ravindra (rkumar@novell.com)
//	Mike Kestner <mkestner@novell.com>


using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Imaging;

namespace System.Windows.Forms
{
	[DefaultProperty ("Text")]
	[Designer ("System.Windows.Forms.Design.ToolBarButtonDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DesignTimeVisible (false)]
	[ToolboxItem (false)]
	public class ToolBarButton : Component
	{
		#region instance variable
		private bool enabled = true;
		private int image_index = -1;
		private ContextMenu menu;
		private ToolBar parent;
		private bool partial_push = false;
		private bool pushed = false;
		private ToolBarButtonStyle style = ToolBarButtonStyle.PushButton;
		private object tag;
		private string text = "";
		private string tooltip = "";
		private bool visible = true;
		internal bool dd_pressed = false; // to check for a mouse down on dropdown rect
		internal bool hilight = false;    // to hilight buttons in flat style
		internal bool inside = false;     // to handle the mouse move event with mouse pressed
		internal bool pressed = false;    // this is to check for mouse down on a button
		#endregion

		#region constructors
		public ToolBarButton () { }

		public ToolBarButton (string text)
		{
			this.text = text;
		}
		#endregion

		#region internal properties
		internal bool Hilight {
			get { return hilight; }
			set {
				if (! pushed)
					hilight = value;
				else
					hilight = false;	
			}
		}

		internal Image Image {
			get {
				if (Parent == null || Parent.ImageList == null)
					return null;

				ImageList list = Parent.ImageList;
				if (ImageIndex > -1 && ImageIndex < list.Images.Count)
					return list.Images [ImageIndex];

				return null;
			}
		}

		Rectangle image_rect;
		internal Rectangle ImageRectangle {
			get {
				Rectangle result = image_rect;
				result.X += bounds.X;
				result.Y += bounds.Y;
				return result; 
			}
		}

		Rectangle text_rect;
		internal Rectangle TextRectangle {
			get { 
				Rectangle result = text_rect;
				result.X += bounds.X;
				result.Y += bounds.Y;
				return result; 
			}
		}

		Rectangle bounds;
		internal Point Location {
			//get { return location; }
			set { 
				if (bounds.Location == value)
					return;

				if (bounds != Rectangle.Empty)
					Invalidate ();

				bounds.Location = value;
				Invalidate ();
			}
		}

		internal bool Pressed {
			get {
				if (pressed && inside)
					return true;
				else
					return false;
			}
			set { pressed = value; }
		}
		#endregion internal properties

		#region properties
		[DefaultValue (null)]
		[TypeConverter (typeof (ReferenceConverter))]
		public Menu DropDownMenu {
			get { return menu; }

			set {
				if (value is ContextMenu)
					menu = (ContextMenu) value;
				else
					throw new ArgumentException ("DropDownMenu must be of type ContextMenu.");
			}
		}

		[DefaultValue (true)]
		[Localizable (true)]
		public bool Enabled {
			get { return enabled; }
			set {
				if (value == enabled)
					return;

				enabled = value;
				Invalidate ();
			}
		}

		[DefaultValue (-1)]
		[Editor ("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[Localizable (true)]
		[TypeConverter (typeof (ImageIndexConverter))]
		public int ImageIndex {
			get { return image_index; }
			set {
				if (value < -1)
					throw new ArgumentException ("ImageIndex value must be above or equal to -1.");

				if (value == image_index)
					return;

				image_index = value;
				Invalidate ();
			}
		}

		[Browsable (false)]
		public ToolBar Parent {
			get { return parent; }
		}

		[DefaultValue (false)]
		public bool PartialPush {
			get { return partial_push; }
			set {
				if (value == partial_push)
					return;

				partial_push = value;
				Invalidate ();
			}
		}

		[DefaultValue (false)]
		public bool Pushed {
			get { return pushed; }
			set {
				if (value == pushed)
					return;

				pushed = value;
				if (pushed)
					hilight = false;
				Invalidate ();
			}
		}

		public Rectangle Rectangle {
			get {
				if (Visible && Parent != null && Parent.Visible) {
					Rectangle result = bounds;
					if (Style == ToolBarButtonStyle.DropDownButton && Parent.DropDownArrows)
						result.Width += ThemeEngine.Current.ToolBarDropDownWidth;
					return result;
				} else
					return Rectangle.Empty;
			}
		}

		[DefaultValue (ToolBarButtonStyle.PushButton)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public ToolBarButtonStyle Style {
			get { return style; }
			set {
				if (value == style)
					return;

				style = value;
				Invalidate ();
			}
		}

		[Bindable (true)]
		[DefaultValue (null)]
		[Localizable (false)]
		[TypeConverter (typeof (StringConverter))]
		public object Tag {
			get { return tag; }
			set { tag = value; }
		}

		[DefaultValue ("")]
		[Localizable (true)]
		public string Text {
			get { return text; }
			set {
				if (value == text)
					return;

				text = value;
				Invalidate ();
			}
		}

		[DefaultValue ("")]
		[Localizable (true)]
		public string ToolTipText {
			get { return tooltip; }
			set { tooltip = value; }
		}

		[DefaultValue (true)]
		[Localizable (true)]
		public bool Visible {
			get { return visible; }
			set {
				if (value == visible)
					return;

				visible = value;
				if (Parent != null)
					Parent.Redraw (true);
			}
		}
		#endregion

		#region internal methods
		internal void SetParent (ToolBar parent)
		{
			if (Parent == parent)
				return;

			if (Parent != null)
				Parent.Buttons.Remove (this);

			this.parent = parent;
		}

		internal void Layout ()
		{
			if (Parent == null || !Visible)
				return;

			Size psize = Parent.ButtonSize;
			Size size = psize;
			if (!Parent.SizeSpecified) {
				size = CalculateSize ();
				if (size.Width == 0 || size.Height == 0)
					size = psize;
			}
			Layout (size);
		}

		internal void Layout (Size size)
		{
			if (Parent == null || !Visible)
				return;

			bounds.Size = size;

			Image image = Image;
			if (image == null) {
				text_rect = new Rectangle (Point.Empty, size);
				image_rect = Rectangle.Empty;
				return;
			}

			int grip = ThemeEngine.Current.ToolBarImageGripWidth;

			if (Parent.TextAlign == ToolBarTextAlign.Underneath) {
				image_rect = new Rectangle ((size.Width - image.Width) / 2 - grip, 0, image.Width + 2 + grip, image.Height + 2 * grip);

				text_rect = new Rectangle (0, image_rect.Bottom, size.Width, size.Height - image_rect.Height);
			} else {
				image_rect = new Rectangle (0, 0, image.Width + 2 * grip, image.Height + 2 * grip);

				text_rect = new Rectangle (image_rect.Right, 0, size.Width - image_rect.Width, size.Height);
			}
		}

		const int text_padding = 3;

		Size CalculateSize ()
		{
			Theme theme = ThemeEngine.Current;

			int ht = Parent.ButtonSize.Height + 2 * theme.ToolBarGripWidth;

			if (Style == ToolBarButtonStyle.Separator)
				return new Size (theme.ToolBarSeparatorWidth, ht);

			SizeF sz = Parent.DeviceContext.MeasureString (Text, Parent.Font);
			Size size = new Size ((int) Math.Ceiling (sz.Width) + 2 * text_padding, 
					      (int) Math.Ceiling (sz.Height));

			Image image = Image;

			if (image == null)
				return size;

			int image_width = image.Width + 2 * theme.ToolBarImageGripWidth; 
			int image_height = image.Height + 2 * theme.ToolBarImageGripWidth; 

			if (Parent.TextAlign == ToolBarTextAlign.Right) {
				size.Width =  image_width + size.Width;
				size.Height = (size.Height > image_height) ? size.Height : image_height;
			} else {
				size.Height = image_height + size.Height;
				size.Width = (size.Width > image_width) ? size.Width : image_width;
			}

			size.Width += theme.ToolBarGripWidth;
			size.Height += theme.ToolBarGripWidth;
			return size;
		}

		void Invalidate ()
		{
			if (Parent != null)
				Parent.Invalidate (Rectangle);
		}

		#endregion Internal Methods

		#region methods
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		public override string ToString ()
		{
			return string.Format ("ToolBarButton: {0}, Style: {1}", text, style);
		}
		#endregion
	}
}
