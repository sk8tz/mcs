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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//	Peter Bartok, pbartok@novell.com
//	John BouAntoun, jba-mono@optusnet.com.au
//	Marek Safar, marek.safar@seznam.cz
//


using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace System.Windows.Forms
{

	internal class ThemeWin32Classic : Theme
	{		
		public override Version Version {
			get {
				return new Version(0, 1, 0, 0);
			}
		}

		/* Hardcoded colour values not exposed in the API constants in all configurations */
		protected static readonly Color arrow_color = Color.Black;
		protected static readonly Color pen_ticks_color = Color.Black;
		protected static readonly Color progressbarblock_color = Color.FromArgb (255, 0, 0, 128);
		protected static StringFormat string_format_menu_text;
		protected static StringFormat string_format_menu_shortcut;
		protected static StringFormat string_format_menu_menubar_text;
		static readonly Rectangle checkbox_rect = new Rectangle (2, 2, 11,11); // Position of the checkbox relative to the item
		static ImageAttributes imagedisabled_attributes = null;
		const int SEPARATOR_HEIGHT = 5;
		const int SM_CXBORDER = 1;
		const int SM_CYBORDER = 1;		
    		const int MENU_TAB_SPACE = 8;		// Pixels added to the width of an item because of a tab
    		const int MENU_BAR_ITEMS_SPACE = 8;	// Space between menu bar items

		#region	Principal Theme Methods
		public ThemeWin32Classic ()
		{			
			defaultWindowBackColor = this.ColorWindow;
			defaultWindowForeColor = this.ColorControlText;
			default_font =	new Font (FontFamily.GenericSansSerif, 8.25f);
			
			/* Menu string formats */
			string_format_menu_text = new StringFormat ();
			string_format_menu_text.LineAlignment = StringAlignment.Center;
			string_format_menu_text.Alignment = StringAlignment.Near;
			string_format_menu_text.HotkeyPrefix = HotkeyPrefix.Show;

			string_format_menu_shortcut = new StringFormat ();	
			string_format_menu_shortcut.LineAlignment = StringAlignment.Center;
			string_format_menu_shortcut.Alignment = StringAlignment.Far;

			string_format_menu_menubar_text = new StringFormat ();
			string_format_menu_menubar_text.LineAlignment = StringAlignment.Center;
			string_format_menu_menubar_text.Alignment = StringAlignment.Center;
			string_format_menu_menubar_text.HotkeyPrefix = HotkeyPrefix.Show;
			always_draw_hotkeys = false;
		}	

		public override void ResetDefaults() {
			throw new NotImplementedException("Need to implement ResetDefaults() for Win32 theme");
		}

		public override bool DoubleBufferingSupported {
			get {return true; }
		}
		#endregion	// Principal Theme Methods

		#region	Internal Methods
		protected SolidBrush GetControlBackBrush (Color c) {
			if (c == DefaultControlBackColor)
				return ResPool.GetSolidBrush (ColorControl);
			return ResPool.GetSolidBrush (c);
		}

		protected SolidBrush GetControlForeBrush (Color c) {
			if (c == DefaultControlForeColor)
				return ResPool.GetSolidBrush (ColorControlText);
			return ResPool.GetSolidBrush (c);
		}
		#endregion	// Internal Methods

		#region OwnerDraw Support
		public  override void DrawOwnerDrawBackground (DrawItemEventArgs e)
		{
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
				e.Graphics.FillRectangle (SystemBrushes.Highlight, e.Bounds);
				return;
			}

			e.Graphics.FillRectangle (GetControlBackBrush (e.BackColor), e.Bounds);
		}

		public  override void DrawOwnerDrawFocusRectangle (DrawItemEventArgs e)
		{
			if (e.State == DrawItemState.Focus)
				CPDrawFocusRectangle (e.Graphics, e.Bounds, e.ForeColor, e.BackColor);
		}
		#endregion	// OwnerDraw Support

		#region ButtonBase
		public override void DrawButtonBase(Graphics dc, Rectangle clip_area, ButtonBase button) {
			// Draw the button: fill rectangle, draw border, etc.
			ButtonBase_DrawButton(button, dc);
			
			// First, draw the image
			if ((button.image != null) || (button.image_list != null))
				ButtonBase_DrawImage(button, dc);
			
			// Draw the focus rectangle
			if (button.has_focus)
				ButtonBase_DrawFocus(button, dc);
			
			// Now the text
			if (button.text != null && button.text != String.Empty)
				ButtonBase_DrawText(button, dc);
		}

		protected virtual void ButtonBase_DrawButton(ButtonBase button, Graphics dc)
		{
			Rectangle buttonRectangle;
			Rectangle borderRectangle;
			
			dc.FillRectangle(ResPool.GetSolidBrush (button.BackColor), button.ClientRectangle);
			
			// set up the button rectangle
			buttonRectangle = button.ClientRectangle;
			if (button.has_focus || button.paint_as_acceptbutton) {
				// shrink the rectangle for the normal button drawing inside the focus rectangle
				borderRectangle = Rectangle.Inflate(buttonRectangle, -1, -1);
			} else {
				borderRectangle = buttonRectangle;
			}

			if (button.FlatStyle == FlatStyle.Flat || button.FlatStyle == FlatStyle.Popup) {
				DrawFlatStyleButton (dc, borderRectangle, button);
			} else {
				CPDrawButton(dc, borderRectangle, button.ButtonState);
				if (button.has_focus || button.paint_as_acceptbutton) {
					dc.DrawRectangle(ResPool.GetPen(button.ForeColor), borderRectangle);
				}
			}
		}

		protected virtual void ButtonBase_DrawImage(ButtonBase button, Graphics dc)
		{
			// Need to draw a picture
			Image	i;
			int	image_x;
			int	image_y;
			int	image_width;
			int	image_height;
			
			int width = button.ClientSize.Width;
			int height = button.ClientSize.Height;
			
			if (button.ImageIndex != -1) {	 // We use ImageIndex instead of image_index since it will return -1 if image_list is null
				i = button.image_list.Images[button.image_index];
			} else {
				i = button.image;
			}
			
			image_width = button.image.Width;
			image_height = button.image.Height;
			
			switch (button.image_alignment) {
				case ContentAlignment.TopLeft: {
					image_x=0;
					image_y=0;
					break;
				}
					
				case ContentAlignment.TopCenter: {
					image_x=(width-image_width)/2;
					image_y=0;
					break;
				}
					
				case ContentAlignment.TopRight: {
					image_x=width-image_width;
					image_y=0;
					break;
				}
					
				case ContentAlignment.MiddleLeft: {
					image_x=0;
					image_y=(height-image_height)/2;
					break;
				}
					
				case ContentAlignment.MiddleCenter: {
					image_x=(width-image_width)/2;
					image_y=(height-image_height)/2;
					break;
				}
					
				case ContentAlignment.MiddleRight: {
					image_x=width-image_width;
					image_y=(height-image_height)/2;
					break;
				}
					
				case ContentAlignment.BottomLeft: {
					image_x=0;
					image_y=height-image_height;
					break;
				}
					
				case ContentAlignment.BottomCenter: {
					image_x=(width-image_width)/2;
					image_y=height-image_height;
					break;
				}
					
				case ContentAlignment.BottomRight: {
					image_x=width-image_width;
					image_y=height-image_height;
					break;
				}
					
				default: {
					image_x=0;
					image_y=0;
					break;
				}
			}
			
			if (button.is_pressed) {
				image_x+=1;
				image_y+=1;
			}
			
			if (button.is_enabled) {
				dc.DrawImage(i, image_x, image_y); 
			}
			else {
				CPDrawImageDisabled(dc, i, image_x, image_y, ColorControl);
			}
		}
		
		protected virtual void ButtonBase_DrawFocus(ButtonBase button, Graphics dc)
		{
			if (button.FlatStyle == FlatStyle.Flat || button.FlatStyle == FlatStyle.Popup) {
				DrawFlatStyleFocusRectangle (dc, button.ClientRectangle, button, button.ForeColor, button.BackColor);
			} else { 
				Rectangle rect = button.ClientRectangle;
				rect.Inflate (-4, -4);
				CPDrawFocusRectangle(dc, rect, button.ForeColor, button.BackColor);
			}
		}
		
		protected virtual void ButtonBase_DrawText(ButtonBase button, Graphics dc)
		{
			Rectangle buttonRectangle = button.ClientRectangle;
			Rectangle text_rect = Rectangle.Inflate(buttonRectangle, -4, -4);
			
			if (button.is_pressed) {
				text_rect.X++;
				text_rect.Y++;
			}
			
			if (button.is_enabled) {					
				dc.DrawString(button.text, button.Font, ResPool.GetSolidBrush (button.ForeColor), text_rect, button.text_format);
			} else {
				if (button.FlatStyle == FlatStyle.Flat || button.FlatStyle == FlatStyle.Popup) {
					dc.DrawString(button.text, button.Font, ResPool.GetSolidBrush (ControlPaint.DarkDark (this.ColorControl)), text_rect, button.text_format);
				} else {
					CPDrawStringDisabled(dc, button.text, button.Font, ColorControlText, text_rect, button.text_format);
				}
			}
		}
		
		// draw the flat style part of the rectangle
		public void DrawFlatStyleButton (Graphics graphics, Rectangle rectangle, ButtonBase button) {
			Color rect_back_color = button.BackColor;
			Color rect_fore_color = button.ForeColor;
			Rectangle trace_rectangle = new Rectangle(rectangle.X, rectangle.Y, Math.Max (rectangle.Width-1, 0), Math.Max (rectangle.Height-1, 0));
				
			if (button.Enabled) {
				if (button.Capture || button.is_entered) {
					if (button.FlatStyle == FlatStyle.Flat) {
						// fill the rectangle
						graphics.FillRectangle (ResPool.GetSolidBrush (rect_back_color), rectangle);
						
						// now draw the outer border
						if (button.Capture && button.is_entered) {
							rect_back_color = ControlPaint.LightLight (rect_back_color);
						} else {
							rect_back_color = ControlPaint.Light (rect_back_color);
						}
						
						// draw rectangle and fill it
						graphics.FillRectangle (ResPool.GetSolidBrush (rect_back_color), rectangle);
						graphics.DrawRectangle(ResPool.GetPen (rect_fore_color), trace_rectangle);
					} else {
						// else it must be a popup button
						
						if (button.Capture && button.is_entered) {
							graphics.DrawRectangle(ResPool.GetPen (this.ColorControlText), trace_rectangle);
						} else {
							// draw a 3d border
							CPDrawBorder3D (graphics, rectangle, Border3DStyle.RaisedInner, Border3DSide.Left | Border3DSide.Top, button.BackColor); 
							graphics.DrawLine ( ResPool.GetPen (this.ColorControlText), trace_rectangle.X, trace_rectangle.Bottom, trace_rectangle.Right, trace_rectangle.Bottom);
							graphics.DrawLine ( ResPool.GetPen (this.ColorControlText), trace_rectangle.Right, trace_rectangle.Y, trace_rectangle.Right, trace_rectangle.Bottom);
						}
					}
					
					// TODO: draw inner focus rectangle
					
				} else {
					// popup has a ButtonColorText forecolor, not a button.ForeCOlor
					if (button.FlatStyle == FlatStyle.Popup) {
						rect_fore_color = this.ColorControlText;
					}
					
					// fill then draw outer rect
					graphics.FillRectangle (ResPool.GetSolidBrush (rect_back_color), rectangle);
					graphics.DrawRectangle(ResPool.GetPen (rect_fore_color), trace_rectangle);
				}
				
				// finally some small tweaks to render radiobutton and checkbox
				CheckBox checkbox = button as CheckBox;
				RadioButton radiobutton = button as RadioButton;
				if ((checkbox != null && checkbox.Checked) ||
					(radiobutton != null && radiobutton.Checked)) {
					if (button.FlatStyle == FlatStyle.Flat && button.is_entered && !button.Capture) {
						// render the hover for flat flatstyle and cheked
						graphics.DrawRectangle(ResPool.GetPen (this.ColorControlText), trace_rectangle);
					} else if (!button.is_entered && !button.Capture) {
						// render the checked state for popup when unhovered
						CPDrawBorder3D (graphics, rectangle, Border3DStyle.SunkenInner, Border3DSide.Right | Border3DSide.Bottom, button.BackColor); 
					}
				}
			} else {
				// rendering checkbox or radio button style buttons
				CheckBox checkbox = button as CheckBox;
				RadioButton radiobutton = button as RadioButton;
				bool draw_popup_checked = false;
				
				if (button.FlatStyle == FlatStyle.Popup) {
					rect_fore_color = this.ColorControlText;
				
					// see if we should draw a disabled checked popup button
					draw_popup_checked = ((checkbox != null && checkbox.Checked) ||
						(radiobutton != null && radiobutton.Checked));
				}
				
				graphics.FillRectangle (ResPool.GetSolidBrush (rect_back_color), rectangle);
				graphics.DrawRectangle(ResPool.GetPen (rect_fore_color), trace_rectangle);
				
				// finally draw the flatstyle checked effect if need
				if (draw_popup_checked) {
					// render the checked state for popup when unhovered
					CPDrawBorder3D (graphics, rectangle, Border3DStyle.SunkenInner, Border3DSide.Right | Border3DSide.Bottom, button.BackColor);
				}
			}
		}

		public override Size ButtonBaseDefaultSize {
			get {
				return new Size (75, 23);
			}
		}
		#endregion	// ButtonBase

		#region CheckBox
		public override void DrawCheckBox(Graphics dc, Rectangle clip_area, CheckBox checkbox) {
			StringFormat		text_format;
			Rectangle		client_rectangle;
			Rectangle		text_rectangle;
			Rectangle		checkbox_rectangle;
			int			checkmark_size=13;
			int			checkmark_space = 4;

			client_rectangle = checkbox.ClientRectangle;
			text_rectangle = client_rectangle;
			checkbox_rectangle = new Rectangle(text_rectangle.X, text_rectangle.Y, checkmark_size, checkmark_size);

			text_format = new StringFormat();
			text_format.Alignment=StringAlignment.Near;
			text_format.LineAlignment=StringAlignment.Center;
			text_format.HotkeyPrefix = HotkeyPrefix.Show;

			/* Calculate the position of text and checkbox rectangle */
			if (checkbox.appearance!=Appearance.Button) {
				switch(checkbox.check_alignment) {
					case ContentAlignment.BottomCenter: {
						checkbox_rectangle.X=(client_rectangle.Right-client_rectangle.Left)/2-checkmark_size/2;
						checkbox_rectangle.Y=client_rectangle.Bottom-checkmark_size;
						text_rectangle.X=client_rectangle.X;
						text_rectangle.Width=client_rectangle.Width;
						text_rectangle.Height=client_rectangle.Height-checkbox_rectangle.Y-checkmark_space;
						break;
					}

					case ContentAlignment.BottomLeft: {
						checkbox_rectangle.X=client_rectangle.Left;
						checkbox_rectangle.Y=client_rectangle.Bottom-checkmark_size;
						text_rectangle.X=client_rectangle.X+checkmark_size+checkmark_space;
						text_rectangle.Width=client_rectangle.Width-checkmark_size-checkmark_space;						
						break;
					}

					case ContentAlignment.BottomRight: {
						checkbox_rectangle.X=client_rectangle.Right-checkmark_size;
						checkbox_rectangle.Y=client_rectangle.Bottom-checkmark_size;
						text_rectangle.X=client_rectangle.X;
						text_rectangle.Width=client_rectangle.Width-checkmark_size-checkmark_space;						
						break;
					}

					case ContentAlignment.MiddleCenter: {
						checkbox_rectangle.X=(client_rectangle.Right-client_rectangle.Left)/2-checkmark_size/2;
						checkbox_rectangle.Y=(client_rectangle.Bottom-client_rectangle.Top)/2-checkmark_size/2;
						text_rectangle.X=client_rectangle.X;
						text_rectangle.Width=client_rectangle.Width;
						break;
					}

					default:
					case ContentAlignment.MiddleLeft: {
						checkbox_rectangle.X=client_rectangle.Left;
						checkbox_rectangle.Y=(client_rectangle.Bottom-client_rectangle.Top)/2-checkmark_size/2;
						text_rectangle.X=client_rectangle.X+checkmark_size+checkmark_space;
						text_rectangle.Width=client_rectangle.Width-checkmark_size-checkmark_space;												
						break;
					}

					case ContentAlignment.MiddleRight: {
						checkbox_rectangle.X=client_rectangle.Right-checkmark_size;
						checkbox_rectangle.Y=(client_rectangle.Bottom-client_rectangle.Top)/2-checkmark_size/2;
						text_rectangle.X=client_rectangle.X;
						text_rectangle.Width=client_rectangle.Width-checkmark_size-checkmark_space;
						break;
					}

					case ContentAlignment.TopCenter: {
						checkbox_rectangle.X=(client_rectangle.Right-client_rectangle.Left)/2-checkmark_size/2;
						checkbox_rectangle.Y=client_rectangle.Top;
						text_rectangle.X=client_rectangle.X;
						text_rectangle.Width=client_rectangle.Width;
						text_rectangle.Y=checkmark_size+checkmark_space;
						text_rectangle.Height=client_rectangle.Height-checkmark_size-checkmark_space;
						break;
					}

					case ContentAlignment.TopLeft: {
						checkbox_rectangle.X=client_rectangle.Left;
						text_rectangle.X=client_rectangle.X+checkmark_size+checkmark_space;
						text_rectangle.Width=client_rectangle.Width-checkmark_size-checkmark_space;
						break;
					}

					case ContentAlignment.TopRight: {
						checkbox_rectangle.X=client_rectangle.Right-checkmark_size;
						text_rectangle.X=client_rectangle.X;
						text_rectangle.Width=client_rectangle.Width-checkmark_size-checkmark_space;
						break;
					}
				}
			} else {
				text_rectangle.X=client_rectangle.X;
				text_rectangle.Width=client_rectangle.Width;
			}
			
			/* Set the horizontal alignment of our text */
			switch(checkbox.text_alignment) {
				case ContentAlignment.BottomLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.TopLeft: {
					text_format.Alignment=StringAlignment.Near;
					break;
				}

				case ContentAlignment.BottomCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.TopCenter: {
					text_format.Alignment=StringAlignment.Center;
					break;
				}

				case ContentAlignment.BottomRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.TopRight: {
					text_format.Alignment=StringAlignment.Far;
					break;
				}
			}

			/* Set the vertical alignment of our text */
			switch(checkbox.text_alignment) {
				case ContentAlignment.TopLeft: 
				case ContentAlignment.TopCenter: 
				case ContentAlignment.TopRight: {
					text_format.LineAlignment=StringAlignment.Near;
					break;
				}

				case ContentAlignment.BottomLeft:
				case ContentAlignment.BottomCenter:
				case ContentAlignment.BottomRight: {
					text_format.LineAlignment=StringAlignment.Far;
					break;
				}

				case ContentAlignment.MiddleLeft:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.MiddleRight: {
					text_format.LineAlignment=StringAlignment.Center;
					break;
				}
			}

			ButtonState state = ButtonState.Normal;
			if (checkbox.FlatStyle == FlatStyle.Flat) {
				state |= ButtonState.Flat;
			}
			
			if (checkbox.Checked) {
				state |= ButtonState.Checked;
			}
			
			if (checkbox.ThreeState && (checkbox.CheckState == CheckState.Indeterminate)) {
				state |= ButtonState.Checked;
				state |= ButtonState.Pushed;				
			}
			
			// finally make sure the pushed and inavtive states are rendered
			if (!checkbox.Enabled) {
				state |= ButtonState.Inactive;
			}
			else if (checkbox.is_pressed) {
				state |= ButtonState.Pushed;
			}
			
			
			// Start drawing
			
			CheckBox_DrawCheckBox(dc, checkbox, state, checkbox_rectangle);
			
			CheckBox_DrawText(checkbox, text_rectangle, dc, text_format);

			CheckBox_DrawFocus(checkbox, dc, text_rectangle);

			text_format.Dispose ();
		}

		protected virtual void CheckBox_DrawCheckBox( Graphics dc, CheckBox checkbox, ButtonState state, Rectangle checkbox_rectangle )
		{
			dc.FillRectangle (ResPool.GetSolidBrush (checkbox.BackColor), checkbox.ClientRectangle);			
			// render as per normal button
			if (checkbox.appearance==Appearance.Button) {
				if (checkbox.FlatStyle == FlatStyle.Flat || checkbox.FlatStyle == FlatStyle.Popup) {
					DrawFlatStyleButton(dc, checkbox.ClientRectangle, checkbox);
				} else {
					CPDrawButton(dc, checkbox.ClientRectangle, state);
				}
			} else {
				// establish if we are rendering a flat style of some sort
				if (checkbox.FlatStyle == FlatStyle.Flat || checkbox.FlatStyle == FlatStyle.Popup) {
					DrawFlatStyleCheckBox (dc, checkbox_rectangle, checkbox);
				} else {
					ControlPaint.DrawCheckBox(dc, checkbox_rectangle, state);
				}
			}
		}
		
		protected virtual void CheckBox_DrawText( CheckBox checkbox, Rectangle text_rectangle, Graphics dc, StringFormat text_format )
		{
			SolidBrush sb;
			
			// offset the text if it's pressed and a button
			if (checkbox.Appearance == Appearance.Button) {
				if (checkbox.Checked || (checkbox.Capture && checkbox.FlatStyle != FlatStyle.Flat)) {
					text_rectangle.X ++;
					text_rectangle.Y ++;
				}
				
				text_rectangle.Inflate(-4, -4);
			}
			
			/* Place the text; to be compatible with Windows place it after the checkbox has been drawn */

			// Windows seems to not wrap text in certain situations, this matches as close as I could get it
			if ((float)(checkbox.Font.Height * 1.5f) > text_rectangle.Height) {
				text_format.FormatFlags |= StringFormatFlags.NoWrap;
			}
			if (checkbox.Enabled) {
				sb = ResPool.GetSolidBrush(checkbox.ForeColor);
				dc.DrawString(checkbox.Text, checkbox.Font, sb, text_rectangle, text_format);			
			} else if (checkbox.FlatStyle == FlatStyle.Flat || checkbox.FlatStyle == FlatStyle.Popup) {
				dc.DrawString(checkbox.Text, checkbox.Font, ResPool.GetSolidBrush (ControlPaint.DarkDark (this.ColorControl)), text_rectangle, text_format);
			} else {
				CPDrawStringDisabled(dc, checkbox.Text, checkbox.Font, ColorControlText, text_rectangle, text_format);
			}
		}
		
		protected virtual void CheckBox_DrawFocus( CheckBox checkbox, Graphics dc, Rectangle text_rectangle )
		{
			if (checkbox.Focused) {
				if (checkbox.FlatStyle != FlatStyle.Flat) {
					DrawInnerFocusRectangle (dc, Rectangle.Inflate (text_rectangle, -1, -1), checkbox.BackColor);
				} else {
					dc.DrawRectangle (ResPool.GetPen (checkbox.ForeColor), Rectangle.Inflate (text_rectangle, -1, -1));
				}
			}
		}

		// renders a checkBox with the Flat and Popup FlatStyle
		protected virtual void DrawFlatStyleCheckBox (Graphics graphics, Rectangle rectangle, CheckBox checkbox)
		{
			Pen			pen;			
			Rectangle	rect;
			Rectangle	checkbox_rectangle;
			Rectangle	fill_rectangle;
			int			lineWidth;
			int			Scale;
			
			// set up our rectangles first
			if (checkbox.FlatStyle == FlatStyle.Popup && checkbox.is_entered) {
				// clip one pixel from bottom right for non popup rendered checkboxes
				checkbox_rectangle = new Rectangle(rectangle.X, rectangle.Y, Math.Max(rectangle.Width-1, 0), Math.Max(rectangle.Height-1,0));
				fill_rectangle = new Rectangle(checkbox_rectangle.X+1, checkbox_rectangle.Y+1, Math.Max(checkbox_rectangle.Width-3, 0), Math.Max(checkbox_rectangle.Height-3,0));
			} else {
				// clip two pixels from bottom right for non popup rendered checkboxes
				checkbox_rectangle = new Rectangle(rectangle.X, rectangle.Y, Math.Max(rectangle.Width-2, 0), Math.Max(rectangle.Height-2,0));
				fill_rectangle = new Rectangle(checkbox_rectangle.X+1, checkbox_rectangle.Y+1, Math.Max(checkbox_rectangle.Width-2, 0), Math.Max(checkbox_rectangle.Height-2,0));
			}	
				
			// if disabled render in disabled state
			if (checkbox.Enabled) {
				// process the state of the checkbox
				if (checkbox.is_entered || checkbox.Capture) {
					// decide on which background color to use
					if (checkbox.FlatStyle == FlatStyle.Popup && checkbox.is_entered && checkbox.Capture) {
						graphics.FillRectangle(ResPool.GetSolidBrush (checkbox.BackColor), fill_rectangle);
					} else if (checkbox.FlatStyle == FlatStyle.Flat && !(checkbox.is_entered && checkbox.Capture)) {
						graphics.FillRectangle(ResPool.GetSolidBrush (ControlPaint.Light(checkbox.BackColor)), fill_rectangle);
					} else {
						// use regular window background color
						graphics.FillRectangle(ResPool.GetSolidBrush (ControlPaint.LightLight (checkbox.BackColor)), fill_rectangle);
					}
					
					// render the outer border
					if (checkbox.FlatStyle == FlatStyle.Flat) {
						ControlPaint.DrawBorder(graphics, checkbox_rectangle, checkbox.ForeColor, ButtonBorderStyle.Solid);
					} else {
						// draw sunken effect
						CPDrawBorder3D (graphics, checkbox_rectangle, Border3DStyle.SunkenInner, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom, checkbox.BackColor);
					}
				} else {
					graphics.FillRectangle(ResPool.GetSolidBrush (ControlPaint.LightLight (checkbox.BackColor)), fill_rectangle);				
					
					if (checkbox.FlatStyle == FlatStyle.Flat) {
						ControlPaint.DrawBorder(graphics, checkbox_rectangle, checkbox.ForeColor, ButtonBorderStyle.Solid);
					} else {
						// draw the outer border
						ControlPaint.DrawBorder(graphics, checkbox_rectangle, ControlPaint.DarkDark (checkbox.BackColor), ButtonBorderStyle.Solid);
					}			
				}
			} else {
				if (checkbox.FlatStyle == FlatStyle.Popup) {
					graphics.FillRectangle(SystemBrushes.Control, fill_rectangle);
				}	
			
				// draw disabled state,
				ControlPaint.DrawBorder(graphics, checkbox_rectangle, ColorControlDark, ButtonBorderStyle.Solid);
			}		
			
			/* Make sure we've got at least a line width of 1 */
			lineWidth = Math.Max(3, fill_rectangle.Width/3);
			Scale=Math.Max(1, fill_rectangle.Width/9);
			
			// flat style check box is rendered inside a rectangle shifted down by one
			rect=new Rectangle(fill_rectangle.X, fill_rectangle.Y+1, fill_rectangle.Width, fill_rectangle.Height);
			if (checkbox.Enabled) {
				pen=ResPool.GetPen(checkbox.ForeColor);
			} else {
				pen=SystemPens.ControlDark;
			}

			if (checkbox.Checked) {
				/* Need to draw a check-mark */
				for (int i=0; i<lineWidth; i++) {
					graphics.DrawLine(pen, rect.Left+lineWidth/2, rect.Top+lineWidth+i, rect.Left+lineWidth/2+2*Scale, rect.Top+lineWidth+2*Scale+i);
					graphics.DrawLine(pen, rect.Left+lineWidth/2+2*Scale, rect.Top+lineWidth+2*Scale+i, rect.Left+lineWidth/2+6*Scale, rect.Top+lineWidth-2*Scale+i);
				}
			}					
		}


		#endregion	// CheckBox
		
		#region CheckedListBox
		
		public override Rectangle CheckedListBoxCheckRectangle ()
		{
			return checkbox_rect;
		}
		
		public override void DrawCheckedListBoxItem (CheckedListBox ctrl, DrawItemEventArgs e)
		{			
			Color back_color, fore_color;
			Rectangle item_rect = e.Bounds;
			ButtonState state;
			StringFormat string_format = ctrl.GetFormatString ();

			/* Draw checkbox */		

			if ((ctrl.Items.GetListBoxItem (e.Index)).State == CheckState.Checked)
				state = ButtonState.Checked;
			else
				state = ButtonState.Normal;

			if (ctrl.ThreeDCheckBoxes == false)
				state |= ButtonState.Flat;

			ControlPaint.DrawCheckBox (e.Graphics,
				item_rect.X + checkbox_rect.X, item_rect.Y + checkbox_rect.Y,
				checkbox_rect.Width, checkbox_rect.Height,
				state);

			item_rect.X += checkbox_rect.Width + checkbox_rect.X * 2;
			item_rect.Width -= checkbox_rect.Width + checkbox_rect.X * 2;
			
			/* Draw text*/
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
				back_color = ColorHighlight;
				fore_color = ColorHighlightText;
			}
			else {
				back_color = e.BackColor;
				fore_color = e.ForeColor;
			}
			
			e.Graphics.FillRectangle (ResPool.GetSolidBrush
				(back_color), item_rect);

			e.Graphics.DrawString (ctrl.GetItemText (ctrl.Items[e.Index]), e.Font,
				ResPool.GetSolidBrush (fore_color),
				item_rect, string_format);
					
			if ((e.State & DrawItemState.Focus) == DrawItemState.Focus) {
				CPDrawFocusRectangle (e.Graphics, item_rect,
					fore_color, back_color);
			}
		}
		
		#endregion // CheckedListBox
		
		#region ComboBox
		
		// Drawing
		
		public override void DrawComboBoxEditDecorations (Graphics dc, ComboBox ctrl, Rectangle cl)
		{				
			dc.DrawLine (ResPool.GetPen (ColorControlDark), cl.X, cl.Y, cl.X + cl.Width, cl.Y); //top 
			dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), cl.X + 1, cl.Y + 1, cl.X + cl.Width - 2, cl.Y + 1);
			dc.DrawLine (ResPool.GetPen (ColorControl), cl.X, cl.Y + cl.Height - 2, cl.X + cl.Width, cl.Y + cl.Height - 2); //down
			dc.DrawLine (ResPool.GetPen (ColorControlLight), cl.X, cl.Y + cl.Height - 1, cl.X + cl.Width, cl.Y + cl.Height - 1);
			dc.DrawLine (ResPool.GetPen (ColorControlDark), cl.X, cl.Y, cl.X, cl.Y + cl.Height); //left
			dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), cl.X + 1, cl.Y + 1, cl.X + 1, cl.Y + cl.Height - 2); 
			dc.DrawLine (ResPool.GetPen (ColorControl), cl.X + cl.Width - 2, cl.Y, cl.X + cl.Width - 2, cl.Y + cl.Height); //right
			dc.DrawLine (ResPool.GetPen (ColorControlLight), cl.X + cl.Width - 1, cl.Y + 1 , cl.X + cl.Width - 1, cl.Y + cl.Height - 1);				
		}		
		
		// Sizing				
		public override int DrawComboBoxEditDecorationTop () { return 2;}
		public override int DrawComboBoxEditDecorationBottom () { return 2;}
		public override int DrawComboBoxEditDecorationRight () { return 2;}
		public override int DrawComboBoxEditDecorationLeft () { return 2;}
		
		private int DrawComboListBoxDecoration (ComboBoxStyle style)
		{
			if (style == ComboBoxStyle.Simple)
				return 2;
			else
				return 1;
		}
				
		public override int DrawComboListBoxDecorationTop (ComboBoxStyle style) 
		{
			return DrawComboListBoxDecoration (style);
		}
		
		public override int DrawComboListBoxDecorationBottom (ComboBoxStyle style)
		{
			return DrawComboListBoxDecoration (style);
		}
		
		public override int DrawComboListBoxDecorationRight (ComboBoxStyle style)
		{
			return DrawComboListBoxDecoration (style);
		}
		
		public override int DrawComboListBoxDecorationLeft (ComboBoxStyle style)
		{
			return DrawComboListBoxDecoration (style);
		}
		
		public override void DrawComboListBoxDecorations (Graphics dc, ComboBox ctrl, Rectangle cl)
		{
			if (ctrl.DropDownStyle == ComboBoxStyle.Simple) {
				DrawComboBoxEditDecorations (dc, ctrl, cl);
			}
			else {			
				dc.DrawRectangle (ResPool.GetPen (ColorWindowFrame),
					cl.X, cl.Y, cl.Width - 1, cl.Height - 1);
			}			
		}
		
		public override void DrawComboBoxItem (ComboBox ctrl, DrawItemEventArgs e)
		{
			Color back_color, fore_color;
			Rectangle text_draw = e.Bounds;
			StringFormat string_format = new StringFormat ();
			string_format.FormatFlags = StringFormatFlags.LineLimit;
			
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
				back_color = ColorHighlight;
				fore_color = ColorHighlightText;
			}
			else {
				back_color = e.BackColor;
				fore_color = e.ForeColor;
			}			
							
			e.Graphics.FillRectangle (ResPool.GetSolidBrush (back_color), e.Bounds);

			if (e.Index != -1) {
				e.Graphics.DrawString (ctrl.GetItemText (ctrl.Items[e.Index]), e.Font,
					ResPool.GetSolidBrush (fore_color),
					text_draw, string_format);
			}
			
			if ((e.State & DrawItemState.Focus) == DrawItemState.Focus) {
				CPDrawFocusRectangle (e.Graphics, e.Bounds, fore_color, back_color);
			}

			string_format.Dispose ();
		}
		
		#endregion ComboBox
		
		#region Datagrid
		public override int DataGridPreferredColumnWidth { get { return 75;} }
		public override int DataGridMinimumColumnCheckBoxHeight { get { return 16;} }
		public override int DataGridMinimumColumnCheckBoxWidth { get { return 16;} }
		public override Color DataGridAlternatingBackColor { get { return ColorWindow;} }
		public override Color DataGridBackColor { get  { return  ColorWindow;} }		
		public override Color DataGridBackgroundColor { get  { return  ColorAppWorkspace;} }
		public override Color DataGridCaptionBackColor { get  { return ColorActiveCaption;} }
		public override Color DataGridCaptionForeColor { get  { return ColorActiveCaptionText;} }
		public override Color DataGridGridLineColor { get { return ColorControl;} }
		public override Color DataGridHeaderBackColor { get  { return ColorControl;} }
		public override Color DataGridHeaderForeColor { get  { return ColorControlText;} }
		public override Color DataGridLinkColor { get  { return ColorHotTrack;} }
		public override Color DataGridLinkHoverColor { get  { return ColorHotTrack;} }
		public override Color DataGridParentRowsBackColor { get  { return ColorControl;} }
		public override Color DataGridParentRowsForeColor { get  { return ColorWindowText;} }
		public override Color DataGridSelectionBackColor { get  { return ColorActiveCaption;} }
		public override Color DataGridSelectionForeColor { get  { return ColorActiveCaptionText;} }
		
		public override void DataGridPaint (PaintEventArgs pe, DataGrid grid)
		{
			// Draw parent rows
			if (pe.ClipRectangle.IntersectsWith (grid.grid_drawing.parent_rows)) {
				pe.Graphics.FillRectangle (ResPool.GetSolidBrush (grid.ParentRowsBackColor), grid.grid_drawing.parent_rows);
			}

			DataGridPaintCaption (pe.Graphics, pe.ClipRectangle, grid);
			DataGridPaintColumnsHdrs (pe.Graphics, pe.ClipRectangle, grid);
			DataGridPaintRowsHeaders (pe.Graphics, pe.ClipRectangle, grid);
			DataGridPaintRows (pe.Graphics, grid.grid_drawing.cells_area, pe.ClipRectangle, grid);

			// Paint scrollBar corner
			if (grid.vert_scrollbar.Visible && grid.horiz_scrollbar.Visible) {

				Rectangle corner = new Rectangle (grid.ClientRectangle.X + grid.ClientRectangle.Width - grid.horiz_scrollbar.Height,
					 grid.ClientRectangle.Y + grid.ClientRectangle.Height - grid.horiz_scrollbar.Height,
					 grid.horiz_scrollbar.Height, grid.horiz_scrollbar.Height);

				if (pe.ClipRectangle.IntersectsWith (corner)) {
					pe.Graphics.FillRectangle (ResPool.GetSolidBrush (grid.ParentRowsBackColor),
						corner);
				}
			}
		}
		
		public override void DataGridPaintCaption (Graphics g, Rectangle clip, DataGrid grid)
		{
			Rectangle modified_area = clip;
			modified_area.Intersect (grid.grid_drawing.caption_area);

			g.FillRectangle (ResPool.GetSolidBrush (grid.CaptionBackColor),
				modified_area);

			g.DrawString (grid.CaptionText, grid.CaptionFont,
				ResPool.GetSolidBrush (grid.CaptionForeColor),
				grid.grid_drawing.caption_area);		
		}

		public override void DataGridPaintColumnsHdrs (Graphics g, Rectangle clip, DataGrid grid)
		{
			Rectangle columns_area = grid.grid_drawing.columnshdrs_area;

			if (grid.CurrentTableStyle.CurrentRowHeadersVisible) { // Paint corner shared between row and column header
				Rectangle rect_bloc = grid.grid_drawing.columnshdrs_area;
				rect_bloc.Width = grid.RowHeaderWidth;
				rect_bloc.Height = grid.grid_drawing.columnshdrs_area.Height;
				if (clip.IntersectsWith (rect_bloc)) {
					if (grid.visiblecolumn_count > 0) {
						g.FillRectangle (ResPool.GetSolidBrush (grid.CurrentTableStyle.CurrentHeaderBackColor), rect_bloc);
					}else {
						g.FillRectangle (ResPool.GetSolidBrush (grid.BackgroundColor), rect_bloc);
					}
				}

				columns_area.X += grid.RowHeaderWidth;
				columns_area.Width -= grid.RowHeaderWidth;
			}

			// Set unused area
			Rectangle columnshdrs_area_complete = columns_area;
			columnshdrs_area_complete.Width = grid.grid_drawing.columnshdrs_maxwidth;
			
			if (grid.CurrentTableStyle.CurrentRowHeadersVisible) {
				columnshdrs_area_complete.Width -= grid.RowHeaderWidth;
			}		

			// Set column painting
			Rectangle rect_columnhdr = new Rectangle ();
			int col_pixel;
			Region current_clip;
			rect_columnhdr.Y = columns_area.Y;
			rect_columnhdr.Height = columns_area.Height;
			
			current_clip = new Region (columns_area);
			g.Clip = current_clip;
			int column_cnt = grid.first_visiblecolumn + grid.visiblecolumn_count;
			for (int column = grid.first_visiblecolumn; column < column_cnt; column++) {
				
				col_pixel = grid.grid_drawing.GetColumnStartingPixel (column);
				rect_columnhdr.X = columns_area.X + col_pixel - grid.horz_pixeloffset;
				rect_columnhdr.Width = grid.CurrentTableStyle.GridColumnStyles[column].Width;

				if (clip.IntersectsWith (rect_columnhdr) == false)
					continue;

				grid.CurrentTableStyle.GridColumnStyles[column].PaintHeader (g, rect_columnhdr, column);

				
			}

			current_clip.Dispose ();
			g.ResetClip ();
				
			Rectangle not_usedarea = columnshdrs_area_complete;				
			not_usedarea.X = rect_columnhdr.X + rect_columnhdr.Width;
			not_usedarea.Width = grid.ClientRectangle.X + grid.ClientRectangle.Width - rect_columnhdr.X - rect_columnhdr.Height;		
			g.FillRectangle (ResPool.GetSolidBrush (grid.BackgroundColor), not_usedarea);
			
		}

		public override void DataGridPaintRowsHeaders (Graphics g, Rectangle clip, DataGrid grid)
		{
			Rectangle rowshdrs_area_complete = grid.grid_drawing.rowshdrs_area;
			rowshdrs_area_complete.Height = grid.grid_drawing.rowshdrs_maxheight;
			Rectangle rect_row = new Rectangle ();
			rect_row.X = grid.grid_drawing.rowshdrs_area.X;			
			int rowcnt = grid.FirstVisibleRow + grid.VisibleRowCount;
			Rectangle not_usedarea = rowshdrs_area_complete;			

			if (rowcnt < grid.RowsCount) { // Paint one row more for partial rows
				rowcnt++;
			}

			g.SetClip (grid.grid_drawing.rowshdrs_area);
			for (int row = grid.FirstVisibleRow; row < rowcnt; row++) {

				rect_row.Width = grid.grid_drawing.rowshdrs_area.Width;
				rect_row.Height = grid.RowHeight;
				rect_row.Y = grid.grid_drawing.rowshdrs_area.Y + ((row - grid.FirstVisibleRow) * grid.RowHeight);

				if (clip.IntersectsWith (rect_row)) {
					DataGridPaintRowHeader (g, rect_row, row, grid);					
				}
			}
			
			g.ResetClip ();
			not_usedarea.Height = grid.grid_drawing.rowshdrs_maxheight - grid.grid_drawing.rowshdrs_area.Height;
			not_usedarea.Y = grid.grid_drawing.rowshdrs_area.Y + grid.grid_drawing.rowshdrs_area.Height;
			g.FillRectangle (ResPool.GetSolidBrush (grid.BackgroundColor), not_usedarea);
		}
		
		public override void DataGridPaintRowHeaderArrow (Graphics g, Rectangle bounds, DataGrid grid) 
		{		
			Point[] arrow = new Point[3];
			Point P1, P2, P3;
			int centerX, centerY, shiftX;			
			Rectangle rect;
			
			rect = new Rectangle (bounds.X + bounds.Width /4, 
				bounds.Y + bounds.Height/4, bounds.Width / 2, bounds.Height / 2);
			
			centerX = rect.Left + rect.Width / 2;
			centerY = rect.Top + rect.Height / 2;
			shiftX = Math.Max (1, rect.Width / 8);			
			rect.X -= shiftX;
			centerX -= shiftX;			
			P1 = new Point (centerX, rect.Top - 1);
			P2 = new Point (centerX, rect.Bottom);
			P3 = new Point (rect.Right, centerY);			
			arrow[0] = P1;
			arrow[1] = P2;
			arrow[2] = P3;
			
			g.FillPolygon (ResPool.GetSolidBrush 
				(grid.CurrentTableStyle.CurrentHeaderForeColor), arrow, FillMode.Winding);
		}

		public override void DataGridPaintRowHeader (Graphics g, Rectangle bounds, int row, DataGrid grid)
		{
			// Background
			g.FillRectangle (ResPool.GetSolidBrush (grid.CurrentTableStyle.CurrentHeaderBackColor),
				bounds);
				
			if (grid.FlatMode == false) {
				
				// Paint Borders
				g.DrawLine (ResPool.GetPen (ColorControlLight),
					bounds.X, bounds.Y, bounds.X + bounds.Width, bounds.Y);
	
				g.DrawLine (ResPool.GetPen (ColorControlLight),
					bounds.X, bounds.Y + 1, bounds.X, bounds.Y + bounds.Height - 1);
	
				g.DrawLine (ResPool.GetPen (ColorControlDark),
					bounds.X + bounds.Width - 1, bounds.Y + 1 , bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);
	
				g.DrawLine (ResPool.GetPen (ColorControlDark),
					bounds.X, bounds.Y + bounds.Height -1, bounds.X + bounds.Width, bounds.Y  + bounds.Height -1);
			}

			if (grid.ShowEditRow && grid.RowsCount > 0 && row == grid.RowsCount  && !(row == grid.CurrentCell.RowNumber && grid.is_changing == true)) {
				
				g.DrawString ("*", grid.grid_drawing.font_newrow, ResPool.GetSolidBrush (grid.CurrentTableStyle.CurrentHeaderForeColor),
					bounds);
				
			} else {
				// Draw arrow
				if (row == grid.CurrentCell.RowNumber) {
	
					if (grid.is_changing == true) {
						g.DrawString ("...", grid.Font,
							ResPool.GetSolidBrush (grid.CurrentTableStyle.CurrentHeaderForeColor),
							bounds);
	
					} else {
						
						Rectangle rect = new Rectangle (bounds.X - 2, bounds.Y, 18, 18);											
						DataGridPaintRowHeaderArrow (g, rect, grid);
					}
				}
			}
		}
		
		public override void DataGridPaintRows (Graphics g, Rectangle cells, Rectangle clip, DataGrid grid)
		{
			Rectangle rect_row = new Rectangle ();
			Rectangle not_usedarea = new Rectangle ();
			rect_row.X = cells.X;

			int rowcnt = grid.FirstVisibleRow + grid.VisibleRowCount;
			
			if (grid.ShowEditRow && grid.RowsCount > 0) {
				rowcnt--;
			}			

			if (rowcnt < grid.RowsCount) { // Paint one row more for partial rows
				rowcnt++;
			}			
			
			rect_row.Height = grid.RowHeight;
			rect_row.Width = cells.Width;
			for (int row = grid.FirstVisibleRow; row < rowcnt; row++) {								
				rect_row.Y = cells.Y + ((row - grid.FirstVisibleRow) * grid.RowHeight);
				if (clip.IntersectsWith (rect_row)) {
					DataGridPaintRow (g, row, rect_row, false, grid);
				}
			}
			
			if (grid.ShowEditRow && grid.RowsCount > 0 && grid.FirstVisibleRow + grid.VisibleRowCount == grid.RowsCount + 1) {
				rect_row.Y = cells.Y + ((rowcnt - grid.FirstVisibleRow) * grid.RowHeight);
				if (clip.IntersectsWith (rect_row)) {
					DataGridPaintRow (g, rowcnt, rect_row, true, grid);
				}
			}			

			not_usedarea.Height = cells.Y + cells.Height - rect_row.Y - rect_row.Height;
			not_usedarea.Y = rect_row.Y + rect_row.Height;
			not_usedarea.Width = rect_row.Width = cells.Width;
			not_usedarea.X = cells.X;
			
			g.FillRectangle (ResPool.GetSolidBrush (grid.BackgroundColor), not_usedarea);
		}
		
		public override void DataGridPaintRow (Graphics g, int row, Rectangle row_rect, bool is_newrow, DataGrid grid)
		{			
			Rectangle rect_cell = new Rectangle ();
			int col_pixel;
			Color backcolor, forecolor;
			Region prev_clip = g.Clip;
			Region current_clip;
			Rectangle not_usedarea = new Rectangle ();

			rect_cell.Y = row_rect.Y;
			rect_cell.Height = row_rect.Height;

			// PaintCells at row, column
			int column_cnt = grid.first_visiblecolumn + grid.visiblecolumn_count;
			for (int column = grid.first_visiblecolumn; column < column_cnt; column++) {

				col_pixel = grid.grid_drawing.GetColumnStartingPixel (column);

				rect_cell.X = row_rect.X + col_pixel - grid.horz_pixeloffset;
				rect_cell.Width = grid.CurrentTableStyle.GridColumnStyles[column].Width;

				current_clip = new Region (row_rect);
				g.Clip = current_clip;

				if (grid.IsSelected (row)) {
					backcolor =  grid.SelectionBackColor;
					forecolor =  grid.SelectionForeColor;
				} else {
					if (row % 2 == 0) {
						backcolor =  grid.BackColor;
					} else {
						backcolor =  grid.AlternatingBackColor;
					}
					
					forecolor =  grid.ForeColor;
				}			

				if (is_newrow) {
					grid.CurrentTableStyle.GridColumnStyles[column].PaintNewRow (g, rect_cell, 
						ResPool.GetSolidBrush (backcolor),
						ResPool.GetSolidBrush (forecolor));						
					
				} else {
					grid.CurrentTableStyle.GridColumnStyles[column].Paint (g, rect_cell, grid.ListManager, row,
						ResPool.GetSolidBrush (backcolor),
						ResPool.GetSolidBrush (forecolor),
						grid.RightToLeft == RightToLeft.Yes);
				}

				g.Clip = prev_clip;
				current_clip.Dispose ();
			}
			
			if (row_rect.X + row_rect.Width > rect_cell.X + rect_cell.Width) {

				not_usedarea.X = rect_cell.X + rect_cell.Width;
				not_usedarea.Width = row_rect.X + row_rect.Width - rect_cell.X - rect_cell.Width;
				not_usedarea.Y = row_rect.Y;
				not_usedarea.Height = row_rect.Height;
				g.FillRectangle (ResPool.GetSolidBrush (grid.BackgroundColor),
					not_usedarea);
			}
		}
		
		#endregion // Datagrid
		
		#region DateTimePicker
	
		public override void DrawDateTimePicker (Graphics dc,  Rectangle clip_rectangle, DateTimePicker dtp) {
			// if not showing the numeric updown control then render border
			if (!dtp.ShowUpDown && clip_rectangle.IntersectsWith (dtp.ClientRectangle)) {
				// draw the outer border
				Rectangle button_bounds = dtp.ClientRectangle;
				this.CPDrawBorder3D (dc, button_bounds, Border3DStyle.Sunken, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom, dtp.BackColor);
				
				// deflate by the border width
				if (clip_rectangle.IntersectsWith (dtp.drop_down_arrow_rect)) {
					button_bounds.Inflate (-2,-2);
					ButtonState state = dtp.is_drop_down_visible ? ButtonState.Pushed : ButtonState.Normal;
					this.CPDrawComboButton ( 
					  dc, 
					  dtp.drop_down_arrow_rect, 
					  state);
				}
			}

			// render the date part
			if (clip_rectangle.IntersectsWith (dtp.date_area_rect)) {
				// fill the background
				dc.FillRectangle (ResPool.GetSolidBrush (ColorWindow), dtp.date_area_rect);
				
				// fill the currently highlighted area
				if (dtp.hilight_date_area != Rectangle.Empty) {
					dc.FillRectangle (ResPool.GetSolidBrush (ColorHighlight), dtp.hilight_date_area);
				}
				
				// draw the text part
				// TODO: if date format is CUstom then we need to draw the dates as separate parts
				StringFormat text_format = new StringFormat();
				text_format.LineAlignment = StringAlignment.Center;
				text_format.Alignment = StringAlignment.Near;					
				dc.DrawString (dtp.Text, dtp.Font, ResPool.GetSolidBrush (dtp.ForeColor), Rectangle.Inflate(dtp.date_area_rect, -1, -1), text_format);
				text_format.Dispose ();
			}
		}
		
		#endregion // DateTimePicker

		#region GroupBox
		public override void DrawGroupBox (Graphics dc,  Rectangle area, GroupBox box) {
			StringFormat	text_format;
			SizeF		size;
			int		width;
			int		y;
			Rectangle	rect;

			rect = box.ClientRectangle;

			// Needed once the Dark/Light code below is enabled again
			//Color disabled = ColorGrayText;
			
			Pen pen_light = ResPool.GetPen (Color.FromArgb (255,255,255,255));
			Pen pen_dark = ResPool.GetPen (Color.FromArgb (255, 128, 128,128));
			
			// TODO: When the Light and Dark methods work this code should be activate it
			//Pen pen_light = new Pen (ControlPaint.Light (disabled, 1));
			//Pen pen_dark = new Pen (ControlPaint.Dark (disabled, 0));

			dc.FillRectangle (ResPool.GetSolidBrush (box.BackColor), rect);

			text_format = new StringFormat();
			text_format.HotkeyPrefix = HotkeyPrefix.Show;

			size = dc.MeasureString (box.Text, box.Font);
			width = (int) size.Width;
			
			if (width > box.Width - 16)
				width = box.Width - 16;
			
			y = box.Font.Height / 2;
			
			/* Draw group box*/
			dc.DrawLine (pen_dark, 0, y, 8, y); // top 
			dc.DrawLine (pen_light, 0, y + 1, 8, y + 1);			
			dc.DrawLine (pen_dark, 8 + width, y, box.Width, y);			
			dc.DrawLine (pen_light, 8 + width, y + 1, box.Width, y + 1);
			
			dc.DrawLine (pen_dark, 0, y + 1, 0, box.Height); // left
			dc.DrawLine (pen_light, 1, y + 1, 1, box.Height);			
			
			dc.DrawLine (pen_dark, 0, box.Height - 2, box.Width,  box.Height - 2); // bottom
			dc.DrawLine (pen_light, 0, box.Height - 1, box.Width,  box.Height - 1);
			
			dc.DrawLine (pen_dark, box.Width - 2, y,  box.Width - 2, box.Height - 2); // right
			dc.DrawLine (pen_light, box.Width - 1, y, box.Width - 1, box.Height - 2);
			
			
			/* Text */
			if (box.Enabled) {
				dc.DrawString (box.Text, box.Font, ResPool.GetSolidBrush (box.ForeColor), 10, 0, text_format);
			} else {
				CPDrawStringDisabled (dc, box.Text, box.Font, box.ForeColor, 
					new RectangleF (10, 0, width,  box.Font.Height), text_format);
			}
			text_format.Dispose ();	
		}

		public override Size GroupBoxDefaultSize {
			get {
				return new Size (200,100);
			}
		}
		#endregion

		#region HScrollBar
		public override Size HScrollBarDefaultSize {
			get {
				return new Size (80, this.ScrollBarButtonSize);
			}
		}

		#endregion	// HScrollBar

		#region Label
		public  override void DrawLabel (Graphics dc, Rectangle clip_rectangle, Label label) 
		{		
			dc.FillRectangle (ResPool.GetSolidBrush (label.BackColor), clip_rectangle);

			if (label.Enabled) {
				dc.DrawString (label.Text, label.Font, ResPool.GetSolidBrush (label.ForeColor), clip_rectangle, label.string_format);
			} else {
				ControlPaint.DrawStringDisabled (dc, label.Text, label.Font, label.ForeColor, clip_rectangle, label.string_format);
			}
		
		}

		public override Size LabelDefaultSize {
			get {
				return new Size (100, 23);
			}
		}
		#endregion	// Label

		#region LinkLabel
		public  override void DrawLinkLabel (Graphics dc, Rectangle clip_rectangle, LinkLabel label)
		{
			Color color;

			dc.FillRectangle (ResPool.GetSolidBrush (label.BackColor), clip_rectangle);

			for (int i = 0; i < label.num_pieces; i++) {
				
				if (clip_rectangle.IntersectsWith (label.pieces[i].rect) == false) {
					continue;
				}				
				
				color = label.GetLinkColor (label.pieces[i], i);

				if (label.pieces[i].link == null)
					dc.DrawString (label.pieces[i].text, label.GetPieceFont (label.pieces[i]), ResPool.GetSolidBrush (Color.Black),
						label.pieces[i].rect.X, label.pieces[i].rect.Y);
				else
					dc.DrawString (label.pieces[i].text, label.GetPieceFont (label.pieces[i]), ResPool.GetSolidBrush (color),
						label.pieces[i].rect.X, label.pieces[i].rect.Y);

				if (label.pieces[i].focused) {					
					CPDrawFocusRectangle (dc, label.pieces[i].rect, label.ForeColor, label.BackColor);
				}
			}			
			
		}
		#endregion	// LinkLabel
		#region ListBox
		
		// Drawing		
		
		private int DrawListBoxDecorationSize (BorderStyle border_style)
		{
			switch (border_style) {
				case BorderStyle.Fixed3D:
					return 2;
				case BorderStyle.FixedSingle:					
					return 1;
				case BorderStyle.None:
				default:
					break;
				}
				
			return 0;
		}			
		
		// Sizing				
		public override void DrawListBoxItem (ListBox ctrl, DrawItemEventArgs e)
		{
			Color back_color, fore_color;
			StringFormat string_format = ctrl.GetFormatString ();
			
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
				back_color = ColorHighlight;
				fore_color = ColorHighlightText;
			}
			else {
				back_color = e.BackColor;
				fore_color = e.ForeColor;
			}

			e.Graphics.FillRectangle (ResPool.GetSolidBrush
				(back_color), e.Bounds);

			e.Graphics.DrawString (ctrl.GetItemText (ctrl.Items[e.Index]), e.Font,
				ResPool.GetSolidBrush (fore_color),
				e.Bounds.X, e.Bounds.Y, string_format);
					
			if ((e.State & DrawItemState.Focus) == DrawItemState.Focus) {
				CPDrawFocusRectangle (e.Graphics, e.Bounds,
					fore_color, back_color);
			}
		}
		
		#endregion ListBox

		#region ListView
		// Drawing
		public override void DrawListViewItems (Graphics dc, Rectangle clip, ListView control)
		{
			bool details = control.View == View.Details;

			dc.FillRectangle (ResPool.GetSolidBrush (control.BackColor), clip);						
			int first = control.FirstVisibleIndex;	

			for (int i = first; i <= control.LastVisibleIndex; i ++) {					
				if (clip.IntersectsWith (control.Items[i].GetBounds (ItemBoundsPortion.Entire)))
					DrawListViewItem (dc, control, control.Items [i]);
			}	
			
			// draw the gridlines
			if (details && control.GridLines) {
				int top = (control.HeaderStyle == ColumnHeaderStyle.None) ?
					2 : control.Font.Height + 2;

				// draw vertical gridlines
				foreach (ColumnHeader col in control.Columns)
					dc.DrawLine (this.ResPool.GetPen (this.ColorControl),
						     col.Rect.Right, top,
						     col.Rect.Right, control.TotalHeight);
				// draw horizontal gridlines
				ListViewItem last_item = null;
				foreach (ListViewItem item in control.Items) {
					dc.DrawLine (this.ResPool.GetPen (this.ColorControl),
						     item.GetBounds (ItemBoundsPortion.Entire).Left, item.GetBounds (ItemBoundsPortion.Entire).Top,
						     control.TotalWidth, item.GetBounds (ItemBoundsPortion.Entire).Top);
					last_item = item;
				}

				// draw a line after at the bottom of the last item
				if (last_item != null) {
					dc.DrawLine (this.ResPool.GetPen (this.ColorControl),
						     last_item.GetBounds (ItemBoundsPortion.Entire).Left,
						     last_item.GetBounds (ItemBoundsPortion.Entire).Bottom,
						     control.TotalWidth,
						     last_item.GetBounds (ItemBoundsPortion.Entire).Bottom);
				}
			}			
			
			dc.ResetClip ();
			
			// Draw corner between the two scrollbars
			if (control.h_scroll.Visible == true && control.h_scroll.Visible == true) {
				Rectangle rect = new Rectangle ();
				rect.X = control.h_scroll.Location.X + control.h_scroll.Width;
				rect.Width = control.v_scroll.Width;
				rect.Y = control.v_scroll.Location.Y + control.v_scroll.Height;
				rect.Height = control.h_scroll.Height;
				dc.FillRectangle (ResPool.GetSolidBrush (ColorControl), rect);
			}

		}
		
		public override void DrawListViewHeader (Graphics dc, Rectangle clip, ListView control)
		{	
			bool details = (control.View == View.Details);
				
			// border is drawn directly in the Paint method
			if (details && control.HeaderStyle != ColumnHeaderStyle.None) {				
				dc.FillRectangle (ResPool.GetSolidBrush (control.BackColor),
						  0, 0, control.TotalWidth, control.Font.Height + 5);
				if (control.Columns.Count > 0) {
					foreach (ColumnHeader col in control.Columns) {
						Rectangle rect = col.Rect;
						rect.X -= control.h_marker;
						ButtonState state;
						if (control.HeaderStyle == ColumnHeaderStyle.Clickable)
							state = col.Pressed ? ButtonState.Pushed : ButtonState.Normal;
						else
							state = ButtonState.Flat;
						this.CPDrawButton (dc, rect, state);
						rect.X += 3;
						rect.Width -= 8;
						if (rect.Width <= 0)
							continue;
						dc.DrawString (col.Text, DefaultFont,
							       ResPool.GetSolidBrush (ColorControlText),
							       rect, col.Format);
					}
				}
			}
		}

		public override void DrawListViewHeaderDragDetails (Graphics dc, ListView view, ColumnHeader col, int target_x)
		{
			Rectangle rect = col.Rect;
			rect.X -= view.h_marker;
			Color color = Color.FromArgb (0x7f, ColorControlDark.R, ColorControlDark.G, ColorControlDark.B);
			dc.FillRectangle (ResPool.GetSolidBrush (color), rect);
			rect.X += 3;
			rect.Width -= 8;
			if (rect.Width <= 0)
				return;
			color = Color.FromArgb (0x7f, ColorControlText.R, ColorControlText.G, ColorControlText.B);
			dc.DrawString (col.Text, DefaultFont, ResPool.GetSolidBrush (color), rect, col.Format);
			Pen pen = new Pen (ColorHighlight, 2);
			dc.DrawLine (pen, target_x, 0, target_x, col.Rect.Height);
		}

		protected virtual void DrawListViewItem (Graphics dc, ListView control, ListViewItem item)
		{				
			int col_offset;
			if (control.View == View.Details && control.Columns.Count > 0)
				col_offset = control.Columns [0].Rect.X;
			else
				col_offset = 0;
			
			Rectangle rect_checkrect = item.CheckRectReal;
			rect_checkrect.X += col_offset;
			Rectangle icon_rect = item.GetBounds (ItemBoundsPortion.Icon);
			icon_rect.X += col_offset;
			Rectangle full_rect = item.GetBounds (ItemBoundsPortion.Entire);
			full_rect.X += col_offset;
			Rectangle text_rect = item.GetBounds (ItemBoundsPortion.Label);			
			text_rect.X += col_offset;

			if (control.CheckBoxes) {
				if (control.StateImageList == null) {
					// Make sure we've got at least a line width of 1
					int check_wd = Math.Max (3, rect_checkrect.Width / 6);
					int scale = Math.Max (1, rect_checkrect.Width / 12);

					// set the checkbox background
					dc.FillRectangle (this.ResPool.GetSolidBrush (this.ColorWindow),
							  rect_checkrect);
					// define a rectangle inside the border area
					Rectangle rect = new Rectangle (rect_checkrect.X + 2,
									rect_checkrect.Y + 2,
									rect_checkrect.Width - 4,
									rect_checkrect.Height - 4);
					Pen pen = new Pen (this.ColorWindowText, 2);
					dc.DrawRectangle (pen, rect);

					// Need to draw a check-mark
					if (item.Checked) {
						pen.Width = 1;
						// adjustments to get the check-mark at the right place
						rect.X ++; rect.Y ++;
						// following logic is taken from DrawFrameControl method
						for (int i = 0; i < check_wd; i++) {
							dc.DrawLine (pen, rect.Left + check_wd / 2,
								     rect.Top + check_wd + i,
								     rect.Left + check_wd / 2 + 2 * scale,
								     rect.Top + check_wd + 2 * scale + i);
							dc.DrawLine (pen,
								     rect.Left + check_wd / 2 + 2 * scale,
								     rect.Top + check_wd + 2 * scale + i,
								     rect.Left + check_wd / 2 + 6 * scale,
								     rect.Top + check_wd - 2 * scale + i);
						}
					}
				}
				else {
					if (item.Checked && control.StateImageList.Images.Count > 1)
						control.StateImageList.Draw (dc,
									     rect_checkrect.Location, 1);
					else if (! item.Checked && control.StateImageList.Images.Count > 0)
						control.StateImageList.Draw (dc,
									     rect_checkrect.Location, 0);
				}
			}

			if (control.View == View.LargeIcon) {
				if (control.LargeImageList == null) {
					Pen pen = new Pen (ColorWindowText, 2);
					dc.DrawLine (pen, icon_rect.Left, icon_rect.Y, icon_rect.Left + 11, icon_rect.Y);
				} else if (item.ImageIndex > -1 && item.ImageIndex < control.LargeImageList.Images.Count)
					control.LargeImageList.Draw (dc, icon_rect.Location, item.ImageIndex);
			} else {
				if (item.ImageIndex > -1 && control.SmallImageList != null &&
				    item.ImageIndex < control.SmallImageList.Images.Count)
					control.SmallImageList.Draw (dc, icon_rect.Location, item.ImageIndex);
			}

			// draw the item text			
			// format for the item text
			StringFormat format = new StringFormat ();
			if (control.View == View.SmallIcon)
				format.LineAlignment = StringAlignment.Near;
			else
				format.LineAlignment = StringAlignment.Center;
			if (control.View == View.LargeIcon)
				format.Alignment = StringAlignment.Center;
			else
				format.Alignment = StringAlignment.Near;
			
			if (!control.LabelWrap)
				format.FormatFlags = StringFormatFlags.NoWrap;
			
			if (item.Selected) {
				if (control.View == View.Details) {
					if (control.FullRowSelect) {
						dc.FillRectangle (ResPool.GetSolidBrush (ColorHighlight), text_rect);
					}
					else {
						Size text_size = Size.Ceiling (dc.MeasureString (item.Text,
												item.Font));
						text_rect.Width = text_size.Width;
						dc.FillRectangle (this.ResPool.GetSolidBrush
								  (this.ColorHighlight), text_rect);
					}
				}
				else {
					/*Size text_size = Size.Ceiling (dc.MeasureString (item.Text,
					  item.Font));
					  Point loc = text_rect.Location;
					  loc.X += (text_rect.Width - text_size.Width) / 2;
					  text_rect.Width = text_size.Width;*/
					dc.FillRectangle (this.ResPool.GetSolidBrush (this.ColorHighlight),
							  text_rect);
				}
			}
			else
				dc.FillRectangle (ResPool.GetSolidBrush (item.BackColor), text_rect);

			if (item.Text != null && item.Text.Length > 0) {
				if (item.Selected)
					dc.DrawString (item.Text, item.Font, this.ResPool.GetSolidBrush
						       (this.ColorHighlightText), text_rect, format);
				else
					dc.DrawString (item.Text, item.Font, this.ResPool.GetSolidBrush
						       (item.ForeColor), text_rect, format);
			}

			if (control.View == View.Details && control.Columns.Count > 0) {
				// draw subitems for details view
				ListViewItem.ListViewSubItemCollection subItems = item.SubItems;
				int count = (control.Columns.Count < subItems.Count ? 
					     control.Columns.Count : subItems.Count);

				if (count > 0) {
					ColumnHeader col;
					ListViewItem.ListViewSubItem subItem;
					Rectangle sub_item_rect = text_rect; 

					// set the format for subitems
					format.FormatFlags = StringFormatFlags.NoWrap;

					// 0th subitem is the item already drawn
					for (int index = 1; index < count; index++) {
						subItem = subItems [index];
						col = control.Columns [index];
						format.Alignment = col.Format.Alignment;
						sub_item_rect.X = col.Rect.X - control.h_marker;
						sub_item_rect.Width = col.Wd;
						Rectangle sub_item_text_rect = sub_item_rect;
						sub_item_text_rect.X += 3;
						sub_item_text_rect.Width -= 6;

						SolidBrush sub_item_back_br = null;
						SolidBrush sub_item_fore_br = null;
						Font sub_item_font = null;

						if (item.UseItemStyleForSubItems) {
							sub_item_back_br = ResPool.GetSolidBrush (item.BackColor);
							sub_item_fore_br = ResPool.GetSolidBrush (item.ForeColor);
							sub_item_font = item.Font;
						} else {
							sub_item_back_br = ResPool.GetSolidBrush (subItem.BackColor);
							sub_item_fore_br = ResPool.GetSolidBrush (subItem.ForeColor);
							sub_item_font = subItem.Font;
						}

						if (item.Selected && control.FullRowSelect) {
							dc.FillRectangle (ResPool.GetSolidBrush (ColorHighlight), sub_item_rect);
							if (subItem.Text != null && subItem.Text.Length > 0)
								dc.DrawString (subItem.Text, sub_item_font,
									       this.ResPool.GetSolidBrush
									       (this.ColorHighlightText),
									       sub_item_text_rect, format);
						} else {
							dc.FillRectangle (sub_item_back_br, sub_item_rect);
							if (subItem.Text != null && subItem.Text.Length > 0)
								dc.DrawString (subItem.Text, sub_item_font,
									       sub_item_fore_br,
									       sub_item_text_rect, format);
						}
					}
				}
			}
			
			if (item.Focused) {				
				Rectangle focus_rect = text_rect;
				if (control.FullRowSelect && control.View == View.Details) {
					int width = 0;
					foreach (ColumnHeader col in control.Columns)
						width += col.Width;
					focus_rect = new Rectangle (0, full_rect.Y, width, full_rect.Height);
				}
				if (item.Selected)
					CPDrawFocusRectangle (dc, focus_rect, ColorHighlightText, ColorHighlight);
				else
					CPDrawFocusRectangle (dc, focus_rect, control.ForeColor, control.BackColor);
			}

			format.Dispose ();
		}

		// Sizing
		public override Size ListViewCheckBoxSize {
			get { return new Size (16, 16); }
		}

		public override int ListViewColumnHeaderHeight {
			get { return 16; }
		}

		public override int ListViewDefaultColumnWidth {
			get { return 60; }
		}

		public override int ListViewVerticalSpacing {
			get { return 22; }
		}

		public override int ListViewEmptyColumnWidth {
			get { return 10; }
		}

		public override int ListViewHorizontalSpacing {
			get { return 10; }
		}

		public override Size ListViewDefaultSize {
			get { return new Size (121, 97); }
		}
		#endregion	// ListView
		
		#region Menus
		public override void CalcItemSize (Graphics dc, MenuItem item, int y, int x, bool menuBar)
		{
			item.X = x;
			item.Y = y;

			if (item.Visible == false) {
				item.Width = 0;
				item.Height = 0;
				return;
			}

			if (item.Separator == true) {
				item.Height = SEPARATOR_HEIGHT / 2;
				item.Width = -1;
				return;
			}
			
			if (item.MeasureEventDefined) {
				MeasureItemEventArgs mi = new MeasureItemEventArgs (dc, item.Index);
				item.PerformMeasureItem (mi);
				item.Height = mi.ItemHeight;
				item.Width = mi.ItemWidth;
				return;
			} else {		
				SizeF size;
				size =  dc.MeasureString (item.Text, MenuFont);
				item.Width = (int) size.Width;
				item.Height = (int) size.Height;
	
				if (!menuBar) {
					if (item.Shortcut != Shortcut.None && item.ShowShortcut) {
						item.XTab = MenuCheckSize.Width + MENU_TAB_SPACE + (int) size.Width;
						size =  dc.MeasureString (" " + item.GetShortCutText (), MenuFont);
						item.Width += MENU_TAB_SPACE + (int) size.Width;
					}
	
					item.Width += 4 + (MenuCheckSize.Width * 2);
				} else {
					item.Width += MENU_BAR_ITEMS_SPACE;
					x += item.Width;
				}
	
				if (item.Height < MenuHeight)
					item.Height = MenuHeight;
			}
		}
		
		// Updates the menu rect and returns the height
		public override int CalcMenuBarSize (Graphics dc, Menu menu, int width)
		{
			int x = 0;
			int y = 0;
			menu.Height = 0;

			foreach (MenuItem item in menu.MenuItems) {

				CalcItemSize (dc, item, y, x, true);

				if (x + item.Width > width) {
					item.X = 0;
					y += item.Height;
					item.Y = y;
					x = 0;
				}

				x += item.Width;
				item.MenuBar = true;				

				if (y + item.Height > menu.Height)
					menu.Height = item.Height + y;
			}

			menu.Width = width;						
			return menu.Height;
		}

		public override void CalcPopupMenuSize (Graphics dc, Menu menu)
		{
			int x = 3;
			int start = 0;
			int i, n, y, max;

			menu.Height = 0;

			while (start < menu.MenuItems.Count) {
				y = 2;
				max = 0;
				for (i = start; i < menu.MenuItems.Count; i++) {
					MenuItem item = menu.MenuItems [i];

					if ((i != start) && (item.Break || item.BarBreak))
						break;

					CalcItemSize (dc, item, y, x, false);
					y += item.Height;

					if (item.Width > max)
						max = item.Width;
				}

				// Replace the -1 by the menu width (separators)
				for (n = start; n < i; n++, start++)
					menu.MenuItems [n].Width = max;

				if (y > menu.Height)
					menu.Height = y;

				x+= max;
			}

			menu.Width = x;

			//space for border
			menu.Width += 2;
			menu.Height += 2;

			menu.Width += SM_CXBORDER;
    			menu.Height += SM_CYBORDER;
		}
		
		// Draws a menu bar in a window
		public override void DrawMenuBar (Graphics dc, Menu menu, Rectangle rect)
		{
			if (menu.Height == 0)
				CalcMenuBarSize (dc, menu, rect.Width);
				
			bool keynav = (menu as MainMenu).tracker.Navigating;
			HotkeyPrefix hp = always_draw_hotkeys || keynav ? HotkeyPrefix.Show : HotkeyPrefix.Hide;
			string_format_menu_menubar_text.HotkeyPrefix = hp;
			string_format_menu_text.HotkeyPrefix = hp;

			rect.Height = menu.Height;
			dc.FillRectangle (ResPool.GetSolidBrush(ColorMenu), rect);
						
			for (int i = 0; i < menu.MenuItems.Count; i++) {
				MenuItem item = menu.MenuItems [i];
				Rectangle item_rect = item.bounds;
				item_rect.X += rect.X;
				item_rect.Y += rect.Y;
				item.MenuHeight = menu.Height;
				item.PerformDrawItem (new DrawItemEventArgs (dc, MenuFont, item_rect, i, item.Status));			
			}				
		}		
		
		Bitmap CreateGlyphBitmap (Size size, MenuGlyph glyph, Color color)
		{
			Color bg_color;
			if (color.R == 0 && color.G == 0 && color.B == 0)
				bg_color = Color.White;
			else
				bg_color = Color.Black;
			Bitmap	bmp = new Bitmap (size.Width, size.Height);
			Graphics gr = Graphics.FromImage (bmp);
			Rectangle rect = new Rectangle (Point.Empty, size);
			gr.FillRectangle (ResPool.GetSolidBrush (bg_color), rect);
			CPDrawMenuGlyph (gr, rect, glyph, color);
			bmp.MakeTransparent (bg_color);
			gr.Dispose ();
			return bmp;
		}

		public override void DrawMenuItem (MenuItem item, DrawItemEventArgs e)
		{
			StringFormat string_format;
			Rectangle rect_text = e.Bounds;

			if (item.Visible == false)
				return;

			if (item.MenuBar)
				string_format = string_format_menu_menubar_text;
			else
				string_format = string_format_menu_text;

			if (item.Separator == true) {
				e.Graphics.DrawLine (ResPool.GetPen (ColorControlDark),
					e.Bounds.X, e.Bounds.Y, e.Bounds.X + e.Bounds.Width, e.Bounds.Y);

				e.Graphics.DrawLine (ResPool.GetPen (ColorControlLight),
					e.Bounds.X, e.Bounds.Y + 1, e.Bounds.X + e.Bounds.Width, e.Bounds.Y + 1);

				return;
			}

			if (!item.MenuBar)
				rect_text.X += MenuCheckSize.Width;

			if (item.BarBreak) { /* Draw vertical break bar*/
				Rectangle rect = e.Bounds;
				rect.Y++;
	        		rect.Width = 3;
	        		rect.Height = item.MenuHeight - 6;

				e.Graphics.DrawLine (ResPool.GetPen (ColorControlDark),
					rect.X, rect.Y , rect.X, rect.Y + rect.Height);

				e.Graphics.DrawLine (ResPool.GetPen (ColorControlLight),
					rect.X + 1, rect.Y , rect.X +1, rect.Y + rect.Height);
			}			
			
			Color color_text;
			Color color_back;
			
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
				color_text = ColorHighlightText;
				color_back = ColorHighlight;
			} else {
				color_text = ColorMenuText;
				color_back = ColorMenu;
			}

			/* Draw background */
			Rectangle rect_back = e.Bounds;
			rect_back.X++;
			rect_back.Width -=2;
			e.Graphics.FillRectangle (ResPool.GetSolidBrush (color_back), rect_back);
			
			if (item.Enabled) {
				e.Graphics.DrawString (item.Text, e.Font,
					ResPool.GetSolidBrush (color_text),
					rect_text, string_format);

				if (!item.MenuBar && item.Shortcut != Shortcut.None && item.ShowShortcut) {
					string str = item.GetShortCutText ();
					Rectangle rect = rect_text;
					rect.X = item.XTab;
					rect.Width -= item.XTab;

					e.Graphics.DrawString (str, e.Font, ResPool.GetSolidBrush (color_text),
						rect, string_format_menu_shortcut);
				}
			} else {
				ControlPaint.DrawStringDisabled (e.Graphics, item.Text, e.Font, 
					Color.Black, rect_text, string_format);
			}

			/* Draw arrow */
			if (item.MenuBar == false && item.IsPopup || item.MdiList) {

				int cx = MenuCheckSize.Width;
				int cy = MenuCheckSize.Height;
				Bitmap	bmp = CreateGlyphBitmap (new Size (cx, cy), MenuGlyph.Arrow, color_text);
				
				if (item.Enabled) {
					e.Graphics.DrawImage (bmp, e.Bounds.X + e.Bounds.Width - cx,
						e.Bounds.Y + ((e.Bounds.Height - cy) /2));
				} else {
					ControlPaint.DrawImageDisabled (e.Graphics, bmp, e.Bounds.X + e.Bounds.Width - cx,
						e.Bounds.Y + ((e.Bounds.Height - cy) /2),  color_back);
				}
 
				bmp.Dispose ();
			}

			/* Draw checked or radio */
			if (item.MenuBar == false && item.Checked) {

				Rectangle area = e.Bounds;
				int cx = MenuCheckSize.Width;
				int cy = MenuCheckSize.Height;
				Bitmap	bmp = CreateGlyphBitmap (new Size (cx, cy), item.RadioCheck ? MenuGlyph.Bullet : MenuGlyph.Checkmark, color_text);

				e.Graphics.DrawImage (bmp, area.X, e.Bounds.Y + ((e.Bounds.Height - cy) / 2));

				bmp.Dispose ();
			}			
		}		
			
		public override void DrawPopupMenu (Graphics dc, Menu menu, Rectangle cliparea, Rectangle rect)
		{

			dc.FillRectangle (ResPool.GetSolidBrush
				(ColorMenu), cliparea);

			/* Draw menu borders */
			dc.DrawLine (ResPool.GetPen (ColorHighlightText),
				rect.X, rect.Y, rect.X + rect.Width, rect.Y);

			dc.DrawLine (ResPool.GetPen (ColorHighlightText),
				rect.X, rect.Y, rect.X, rect.Y + rect.Height);

			dc.DrawLine (ResPool.GetPen (ColorControlDark),
				rect.X + rect.Width - 1 , rect.Y , rect.X + rect.Width - 1, rect.Y + rect.Height);

			dc.DrawLine (ResPool.GetPen (ColorControlDarkDark),
				rect.X + rect.Width, rect.Y , rect.X + rect.Width, rect.Y + rect.Height);

			dc.DrawLine (ResPool.GetPen (ColorControlDark),
				rect.X , rect.Y + rect.Height - 1 , rect.X + rect.Width - 1, rect.Y + rect.Height -1);

			dc.DrawLine (ResPool.GetPen (ColorControlDarkDark),
				rect.X , rect.Y + rect.Height, rect.X + rect.Width - 1, rect.Y + rect.Height);

			for (int i = 0; i < menu.MenuItems.Count; i++)
				if (cliparea.IntersectsWith (menu.MenuItems [i].bounds)) {
					MenuItem item = menu.MenuItems [i];
					item.MenuHeight = menu.Height;
					item.PerformDrawItem (new DrawItemEventArgs (dc, MenuFont,
						item.bounds, i, item.Status));
			}
		}
		
		#endregion // Menus

		#region MonthCalendar

		// draw the month calendar
		public override void DrawMonthCalendar(Graphics dc, Rectangle clip_rectangle, MonthCalendar mc) 
		{
			Rectangle client_rectangle = mc.ClientRectangle;
			Size month_size = mc.SingleMonthSize;
			// cache local copies of Marshal-by-ref internal members (gets around error CS0197)
			Size calendar_spacing = (Size)((object)mc.calendar_spacing);
			Size date_cell_size = (Size)((object)mc.date_cell_size);
			
			// draw the singlecalendars
			int x_offset = 1;
			int y_offset = 1;
			// adjust for the position of the specific month
			for (int i=0; i < mc.CalendarDimensions.Height; i++) 
			{
				if (i > 0) 
				{
					y_offset += month_size.Height + calendar_spacing.Height;
				}
				// now adjust for x position	
				for (int j=0; j < mc.CalendarDimensions.Width; j++) 
				{
					if (j > 0) 
					{
						x_offset += month_size.Width + calendar_spacing.Width;
					} 
					else 
					{
						x_offset = 1;
					}

					Rectangle month_rect = new Rectangle (x_offset, y_offset, month_size.Width, month_size.Height);
					if (month_rect.IntersectsWith (clip_rectangle)) {
						DrawSingleMonth (
							dc,
							clip_rectangle,
							month_rect,
							mc,
							i,
							j);
					}
				}
			}
			
			Rectangle bottom_rect = new Rectangle (
						client_rectangle.X,
						Math.Max(client_rectangle.Bottom - date_cell_size.Height - 3, 0),
						client_rectangle.Width,
						date_cell_size.Height + 2);
			// draw the today date if it's set
			if (mc.ShowToday && bottom_rect.IntersectsWith (clip_rectangle)) 
			{
				dc.FillRectangle (ResPool.GetSolidBrush (mc.BackColor), bottom_rect);
				if (mc.ShowToday) {
					int today_offset = 5;
					if (mc.ShowTodayCircle) 
					{
						Rectangle today_circle_rect = new Rectangle (
							client_rectangle.X + 5,
							Math.Max(client_rectangle.Bottom - date_cell_size.Height - 2, 0),
							date_cell_size.Width,
							date_cell_size.Height);
							DrawTodayCircle (dc, today_circle_rect);
						today_offset += date_cell_size.Width + 5;
					}
					// draw today's date
					StringFormat text_format = new StringFormat();
					text_format.LineAlignment = StringAlignment.Center;
					text_format.Alignment = StringAlignment.Near;
					Font bold_font = new Font (mc.Font.FontFamily, mc.Font.Size, mc.Font.Style | FontStyle.Bold);
					Rectangle today_rect = new Rectangle (
							today_offset + client_rectangle.X,
							Math.Max(client_rectangle.Bottom - date_cell_size.Height, 0),
							Math.Max(client_rectangle.Width - today_offset, 0),
							date_cell_size.Height);
					dc.DrawString ("Today: " + DateTime.Now.ToShortDateString(), bold_font, ResPool.GetSolidBrush (mc.ForeColor), today_rect, text_format);
					text_format.Dispose ();
					bold_font.Dispose ();
				}				
			}
			
			// finally paint the borders of the calendars as required
			for (int i = 0; i <= mc.CalendarDimensions.Width; i++) {
				if (i == 0 && clip_rectangle.X == client_rectangle.X) {
					dc.FillRectangle (ResPool.GetSolidBrush (mc.BackColor), new Rectangle (client_rectangle.X, client_rectangle.Y, 1, client_rectangle.Height));
				} else if (i == mc.CalendarDimensions.Width && clip_rectangle.Right == client_rectangle.Right) {
					dc.FillRectangle (ResPool.GetSolidBrush (mc.BackColor), new Rectangle (client_rectangle.Right-1, client_rectangle.Y, 1, client_rectangle.Height));
				} else { 
					Rectangle rect = new Rectangle (
						client_rectangle.X + (month_size.Width*i) + (calendar_spacing.Width * (i-1)) + 1,
						client_rectangle.Y,
						calendar_spacing.Width,
						client_rectangle.Height);
					if (i < mc.CalendarDimensions.Width && i > 0 && clip_rectangle.IntersectsWith (rect)) {
						dc.FillRectangle (ResPool.GetSolidBrush (mc.BackColor), rect);
					}
				}
			}
			for (int i = 0; i <= mc.CalendarDimensions.Height; i++) {
				if (i == 0 && clip_rectangle.Y == client_rectangle.Y) {
					dc.FillRectangle (ResPool.GetSolidBrush (mc.BackColor), new Rectangle (client_rectangle.X, client_rectangle.Y, client_rectangle.Width, 1));
				} else if (i == mc.CalendarDimensions.Height && clip_rectangle.Bottom == client_rectangle.Bottom) {
					dc.FillRectangle (ResPool.GetSolidBrush (mc.BackColor), new Rectangle (client_rectangle.X, client_rectangle.Bottom-1, client_rectangle.Width, 1));
				} else { 
					Rectangle rect = new Rectangle (
						client_rectangle.X,
						client_rectangle.Y + (month_size.Height*i) + (calendar_spacing.Height*(i-1)) + 1,
						client_rectangle.Width,
						calendar_spacing.Height);
					if (i < mc.CalendarDimensions.Height && i > 0 && clip_rectangle.IntersectsWith (rect)) {
						dc.FillRectangle (ResPool.GetSolidBrush (mc.BackColor), rect);
					}
				}
			}
			
			// draw the drop down border if need
			if (mc.owner != null) {
				Rectangle bounds = mc.ClientRectangle;
				if (clip_rectangle.Contains (mc.Location)) {
					// find out if top or left line to draw
					if(clip_rectangle.Contains (new Point (bounds.Left, bounds.Bottom))) {
					
						dc.DrawLine (SystemPens.ControlText, bounds.X, bounds.Y, bounds.X, bounds.Bottom-1);
					}
					if(clip_rectangle.Contains (new Point (bounds.Right, bounds.Y))) {
						dc.DrawLine (SystemPens.ControlText, bounds.X, bounds.Y, bounds.Right-1, bounds.Y);
					}
				}
				if (clip_rectangle.Contains (new Point(bounds.Right, bounds.Bottom))) {
					// find out if bottom or right line to draw
					if(clip_rectangle.Contains (new Point (bounds.Left, bounds.Bottom))) {
						dc.DrawLine (SystemPens.ControlText, bounds.X, bounds.Bottom-1, bounds.Right-1, bounds.Bottom-1);
					}
					if(clip_rectangle.Contains (new Point (bounds.Right, bounds.Y))) {
						dc.DrawLine (SystemPens.ControlText, bounds.Right-1, bounds.Y, bounds.Right-1, bounds.Bottom-1);
					}
				}
			}
		}

		// darws a single part of the month calendar (with one month)
		private void DrawSingleMonth(Graphics dc, Rectangle clip_rectangle, Rectangle rectangle, MonthCalendar mc, int row, int col) 
		{
			// cache local copies of Marshal-by-ref internal members (gets around error CS0197)
			Size title_size = (Size)((object)mc.title_size);
			Size date_cell_size = (Size)((object)mc.date_cell_size);
			DateTime current_month = (DateTime)((object)mc.current_month);
			
			// set up some standard string formating variables
			StringFormat text_format = new StringFormat();
			text_format.LineAlignment = StringAlignment.Center;
			text_format.Alignment = StringAlignment.Center;
			

			// draw the title back ground
			DateTime this_month = current_month.AddMonths (row*mc.CalendarDimensions.Width+col);
			Rectangle title_rect = new Rectangle(rectangle.X, rectangle.Y, title_size.Width, title_size.Height);
			if (title_rect.IntersectsWith (clip_rectangle)) {
				dc.FillRectangle (ResPool.GetSolidBrush (mc.TitleBackColor), title_rect);
				// draw the title				
				string title_text = this_month.ToString ("MMMM yyyy");
				dc.DrawString (title_text, mc.Font, ResPool.GetSolidBrush (mc.TitleForeColor), title_rect, text_format);

				// draw previous and next buttons if it's time
				if (row == 0 && col == 0) 
				{
					// draw previous button
					DrawMonthCalendarButton (
						dc,
						rectangle,
						mc,
						title_size,
						mc.button_x_offset,
						(System.Drawing.Size)((object)mc.button_size),
						true);
				}
				if (row == 0 && col == mc.CalendarDimensions.Width-1) 
				{
					// draw next button
					DrawMonthCalendarButton (
						dc,
						rectangle,
						mc,
						title_size,
						mc.button_x_offset,
						(System.Drawing.Size)((object)mc.button_size),
						false);
				}
			}
			
			// set the week offset and draw week nums if needed
			int col_offset = (mc.ShowWeekNumbers) ? 1 : 0;
			Rectangle day_name_rect = new Rectangle(
				rectangle.X,
				rectangle.Y + title_size.Height,
				(7 + col_offset) * date_cell_size.Width,
				date_cell_size.Height);
			if (day_name_rect.IntersectsWith (clip_rectangle)) {
				dc.FillRectangle (ResPool.GetSolidBrush (mc.BackColor), day_name_rect);
				// draw the day names 
				DayOfWeek first_day_of_week = mc.GetDayOfWeek(mc.FirstDayOfWeek);
				for (int i=0; i < 7; i++) 
				{
					int position = i - (int) first_day_of_week;
					if (position < 0) 
					{
						position = 7 + position;
					}
					// draw it
					Rectangle day_rect = new Rectangle(
						day_name_rect.X + ((i + col_offset)* date_cell_size.Width),
						day_name_rect.Y,
						date_cell_size.Width,
						date_cell_size.Height);
					dc.DrawString (((DayOfWeek)i).ToString().Substring(0, 3), mc.Font, ResPool.GetSolidBrush (mc.TitleBackColor), day_rect, text_format);
				}
				
				// draw the vertical divider
				int vert_divider_y = Math.Max(title_size.Height+ date_cell_size.Height-1, 0);
				dc.DrawLine (
					ResPool.GetPen (mc.ForeColor),
					rectangle.X + (col_offset * date_cell_size.Width) + mc.divider_line_offset,
					rectangle.Y + vert_divider_y,
					rectangle.Right - mc.divider_line_offset,
					rectangle.Y + vert_divider_y);
			}


			// draw the actual date items in the grid (including the week numbers)
			Rectangle date_rect = new Rectangle (
				rectangle.X,
				rectangle.Y + title_size.Height + date_cell_size.Height,
				date_cell_size.Width,
				date_cell_size.Height);
			int month_row_count = 0;
			bool draw_week_num_divider = false;
			DateTime current_date = mc.GetFirstDateInMonthGrid ( new DateTime (this_month.Year, this_month.Month, 1));
			for (int i=0; i < 6; i++) 
			{
				// establish if this row is in our clip_area
				Rectangle row_rect = new Rectangle (
					rectangle.X,
					rectangle.Y + title_size.Height + (date_cell_size.Height * (i+1)),
					date_cell_size.Width * 7,
					date_cell_size.Height);
				if (mc.ShowWeekNumbers) {
					row_rect.Width += date_cell_size.Width;
				}
		
				bool draw_row = row_rect.IntersectsWith (clip_rectangle);
				if (draw_row) {
					dc.FillRectangle (ResPool.GetSolidBrush (mc.BackColor), row_rect);
				}
				// establish if this is a valid week to draw
				if (mc.IsValidWeekToDraw (this_month, current_date, row, col)) {
					month_row_count = i;
				}
				
				// draw the week number if required
				if (mc.ShowWeekNumbers && month_row_count == i) {
					if (!draw_week_num_divider) {
						draw_week_num_divider = draw_row;
					}
					// get the week for this row
					int week = mc.GetWeekOfYear (current_date);	

					if (draw_row) {
						dc.DrawString (
							week.ToString(),
							mc.Font,
							ResPool.GetSolidBrush (mc.TitleBackColor),
							date_rect,
							text_format);
					}
					date_rect.Offset(date_cell_size.Width, 0);
				}
								
				// only draw the days if we have to
				if(month_row_count == i) {
					for (int j=0; j < 7; j++) 
					{
						if (draw_row) {
							DrawMonthCalendarDate (
								dc,
								date_rect,
								mc,
								current_date,
								this_month,
								row,
								col);
						}

						// move the day on
						current_date = current_date.AddDays(1);
						date_rect.Offset(date_cell_size.Width, 0);
					}

					// shift the rectangle down one row
					int offset = (mc.ShowWeekNumbers) ? -8 : -7;
					date_rect.Offset(offset*date_cell_size.Width, date_cell_size.Height);
				}
			}

			// month_row_count is zero based, so add one
			month_row_count++;

			// draw week numbers if required
			if (draw_week_num_divider) {
				col_offset = 1;
				dc.DrawLine (
					ResPool.GetPen (mc.ForeColor),
					rectangle.X + date_cell_size.Width - 1,
					rectangle.Y + title_size.Height + date_cell_size.Height + mc.divider_line_offset,
					rectangle.X + date_cell_size.Width - 1,
					rectangle.Y + title_size.Height + date_cell_size.Height + (month_row_count * date_cell_size.Height) - mc.divider_line_offset);
			}
			text_format.Dispose ();
		}

		// draws the pervious or next button
		private void DrawMonthCalendarButton (Graphics dc, Rectangle rectangle, MonthCalendar mc, Size title_size, int x_offset, Size button_size, bool is_previous) 
		{
			bool is_clicked = false;
			Rectangle button_rect;
			Rectangle arrow_rect = new Rectangle (rectangle.X, rectangle.Y, 4, 7);
			Point[] arrow_path = new Point[3];
			// prepare the button
			if (is_previous) 
			{
				is_clicked = mc.is_previous_clicked;
				button_rect = new Rectangle (
					rectangle.X + 1 + x_offset,
					rectangle.Y + 1 + ((title_size.Height - button_size.Height)/2),
					Math.Max(button_size.Width - 1, 0),
					Math.Max(button_size.Height - 1, 0));
				arrow_rect.X = button_rect.X + ((button_rect.Width - arrow_rect.Width)/2);
				arrow_rect.Y = button_rect.Y + ((button_rect.Height - arrow_rect.Height)/2);
				if (is_clicked) {
					arrow_rect.Offset(1,1);
				}
				arrow_path[0] = new Point (arrow_rect.Right, arrow_rect.Y);
				arrow_path[1] = new Point (arrow_rect.X, arrow_rect.Y + arrow_rect.Height/2);
				arrow_path[2] = new Point (arrow_rect.Right, arrow_rect.Bottom);
			}
			else
			{
				is_clicked = mc.is_next_clicked;
				button_rect = new Rectangle (
					rectangle.Right - 1 - x_offset - button_size.Width,
					rectangle.Y + 1 + ((title_size.Height - button_size.Height)/2),
					Math.Max(button_size.Width - 1, 0),
					Math.Max(button_size.Height - 1, 0));
				arrow_rect.X = button_rect.X + ((button_rect.Width - arrow_rect.Width)/2);
				arrow_rect.Y = button_rect.Y + ((button_rect.Height - arrow_rect.Height)/2);
				if (is_clicked) {
					arrow_rect.Offset(1,1);
				}
				arrow_path[0] = new Point (arrow_rect.X, arrow_rect.Y);
				arrow_path[1] = new Point (arrow_rect.Right, arrow_rect.Y + arrow_rect.Height/2);
				arrow_path[2] = new Point (arrow_rect.X, arrow_rect.Bottom);				
			}

			// fill the background
			dc.FillRectangle (SystemBrushes.Control, button_rect);
			// draw the border
			if (is_clicked) {
				dc.DrawRectangle (SystemPens.ControlDark, button_rect);
			}
			else {
				CPDrawBorder3D (dc, button_rect, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom);
			}
			// draw the arrow
			dc.FillPolygon (SystemBrushes.ControlText, arrow_path);			
		}
		

		// draws one day in the calendar grid
		private void DrawMonthCalendarDate (Graphics dc, Rectangle rectangle, MonthCalendar mc,	DateTime date, DateTime month, int row, int col) {
			Color date_color = mc.ForeColor;
			Rectangle interior = new Rectangle (rectangle.X, rectangle.Y, Math.Max(rectangle.Width - 1, 0), Math.Max(rectangle.Height - 1, 0));

			// find out if we are the lead of the first calendar or the trail of the last calendar						
			if (date.Year != month.Year || date.Month != month.Month) {
				DateTime check_date = month.AddMonths (-1);
				// check if it's the month before 
				if (check_date.Year == date.Year && check_date.Month == date.Month && row == 0 && col == 0) {
					date_color = mc.TrailingForeColor;
				} else {
					// check if it's the month after
					check_date = month.AddMonths (1);
					if (check_date.Year == date.Year && check_date.Month == date.Month && row == mc.CalendarDimensions.Height-1 && col == mc.CalendarDimensions.Width-1) {
						date_color = mc.TrailingForeColor;
					} else {
						return;
					}
				}
			} else {
				date_color = mc.ForeColor;
			}


			if (date == mc.SelectionStart && date == mc.SelectionEnd) {
				// see if the date is in the start of selection
				date_color = mc.BackColor;
				// draw the left hand of the back ground
				Rectangle selection_rect = Rectangle.Inflate(rectangle, -3, -3);				
				dc.FillPie (ResPool.GetSolidBrush (mc.TitleBackColor), selection_rect, 0, 359);
			} else if (date == mc.SelectionStart) {
				// see if the date is in the start of selection
				date_color = mc.BackColor;
				// draw the left hand of the back ground
				Rectangle selection_rect = Rectangle.Inflate(rectangle, -3, -3);				
				dc.FillPie (ResPool.GetSolidBrush (mc.TitleBackColor), selection_rect, 90, 180);
				// fill the other side as a straight rect
				if (date < mc.SelectionEnd) 
				{
					// use rectangle instead of rectangle to go all the way to edge of rect
					selection_rect.X = (int) Math.Floor((double)(rectangle.X + rectangle.Width / 2));
					selection_rect.Width = Math.Max(rectangle.Right - selection_rect.X, 0);
					dc.FillRectangle (ResPool.GetSolidBrush (mc.TitleBackColor), selection_rect);
				}
			} else if (date == mc.SelectionEnd) {
				// see if it is the end of selection
				date_color = mc.BackColor;
				// draw the left hand of the back ground
				Rectangle selection_rect = Rectangle.Inflate(rectangle, -3, -3);
				dc.FillPie (ResPool.GetSolidBrush (mc.TitleBackColor), selection_rect, 270, 180);
				// fill the other side as a straight rect
				if (date > mc.SelectionStart) {
					selection_rect.X = rectangle.X;
					selection_rect.Width = rectangle.Width - (rectangle.Width / 2);
					dc.FillRectangle (ResPool.GetSolidBrush (mc.TitleBackColor), selection_rect);
				}
			} else if (date > mc.SelectionStart && date < mc.SelectionEnd) {
				// now see if it's in the middle
				date_color = mc.BackColor;
				// draw the left hand of the back ground
				Rectangle selection_rect = Rectangle.Inflate(rectangle, 0, -3);
				dc.FillRectangle (ResPool.GetSolidBrush (mc.TitleBackColor), selection_rect);
			}

			// set up some standard string formating variables
			StringFormat text_format = new StringFormat();
			text_format.LineAlignment = StringAlignment.Center;
			text_format.Alignment = StringAlignment.Center;
			

			// establish if it's a bolded font
			Font font;
			if (mc.IsBoldedDate (date)) {
				font = new Font (mc.Font.FontFamily, mc.Font.Size, mc.Font.Style | FontStyle.Bold);
			} else {
				font = mc.Font;
			}

			// just draw the date now
			dc.DrawString (date.Day.ToString(), font, ResPool.GetSolidBrush (date_color), rectangle, text_format);

			// today circle if needed
			if (mc.ShowTodayCircle && date == DateTime.Now.Date) {
				DrawTodayCircle (dc, interior);
			}

			// draw the selection grid
			if (mc.is_date_clicked && mc.clicked_date == date) {				
				using (Pen pen = new Pen (Color.Black, 1) ) {
					pen.DashStyle = DashStyle.Dot;
					dc.DrawRectangle (pen, interior);
				}
			}
			text_format.Dispose ();
		}

		private void DrawTodayCircle (Graphics dc, Rectangle rectangle) {
			Color circle_color = Color.FromArgb (248, 0, 0);
			// draw the left hand of the circle 
			Rectangle lhs_circle_rect = new Rectangle (rectangle.X + 1, rectangle.Y + 4, Math.Max(rectangle.Width - 2, 0), Math.Max(rectangle.Height - 5, 0));
			Rectangle rhs_circle_rect = new Rectangle (rectangle.X + 1, rectangle.Y + 1, Math.Max(rectangle.Width - 2, 0), Math.Max(rectangle.Height - 2, 0));
			Point [] curve_points = new Point [3];
			curve_points [0] = new Point (lhs_circle_rect.X, rhs_circle_rect.Y + rhs_circle_rect.Height/12);
			curve_points [1] = new Point (lhs_circle_rect.X + lhs_circle_rect.Width/9, rhs_circle_rect.Y);
			curve_points [2] = new Point (lhs_circle_rect.X + lhs_circle_rect.Width/2 + 1, rhs_circle_rect.Y);

			using (Pen pen = new Pen (circle_color, 2)) {
				dc.DrawArc (pen, lhs_circle_rect, 90, 180);
				dc.DrawArc (pen, rhs_circle_rect, 270, 180);					
				dc.DrawCurve (pen, curve_points);
				dc.DrawLine (ResPool.GetPen (circle_color), curve_points [2], new Point (curve_points [2].X, lhs_circle_rect.Y));
			}
		}

		#endregion 	// MonthCalendar

		#region Panel
		public override Size PanelDefaultSize {
			get {
				return new Size (200, 100);
			}
		}
		#endregion	// Panel

		#region PictureBox
		public override void DrawPictureBox (Graphics dc, Rectangle clip, PictureBox pb) {
			Rectangle client = pb.ClientRectangle;

			// FIXME - instead of drawing the whole picturebox every time
			// intersect the clip rectangle with the drawn picture and only draw what's needed,
			// Also, we only need a background fill where no image goes
			if (pb.Image != null) {
				switch (pb.SizeMode) {
				case PictureBoxSizeMode.StretchImage:
					dc.DrawImage (pb.Image, 0, 0, client.Width, client.Height);
					break;

				case PictureBoxSizeMode.CenterImage:
					dc.FillRectangle(ResPool.GetSolidBrush(pb.BackColor), clip);
					dc.DrawImage (pb.Image, (client.Width / 2) - (pb.Image.Width / 2), (client.Height / 2) - (pb.Image.Height / 2));
					break;
				default:
					dc.FillRectangle(ResPool.GetSolidBrush(pb.BackColor), clip);
					// Normal, AutoSize
					dc.DrawImage(pb.Image, 0, 0, pb.Image.Width, pb.Image.Height);
					break;
				}

				return;
			}

			// We only get here if no image is set. At least paint the background
			dc.FillRectangle(ResPool.GetSolidBrush(pb.BackColor), clip);
		}

		public override Size PictureBoxDefaultSize {
			get {
				return new Size (100, 50);
			}
		}
		#endregion	// PictureBox

		#region ProgressBar
		public override void DrawProgressBar (Graphics dc, Rectangle clip_rect, ProgressBar ctrl) 
		{
			Rectangle	block_rect;
			Rectangle	client_area = ctrl.client_area;
			int		space_betweenblocks	= 2;			
			int		block_width;
			int		increment;
			int		barpos_pixels;
			
			block_width = (client_area.Height * 2 ) / 3;
			barpos_pixels = ((ctrl.Value - ctrl.Minimum) * client_area.Width) / (ctrl.Maximum - ctrl.Minimum);
			increment = block_width + space_betweenblocks;

			/* Draw border */
			CPDrawBorder3D (dc, ctrl.ClientRectangle, Border3DStyle.SunkenInner, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom & ~Border3DSide.Middle, ColorControl);
			
			/* Draw Blocks */
			block_rect = new Rectangle (client_area.X, client_area.Y, block_width, client_area.Height);
			while ((block_rect.X - client_area.X) < barpos_pixels) {
				
				if (clip_rect.IntersectsWith (block_rect) == true) {				
					dc.FillRectangle (ResPool.GetSolidBrush (progressbarblock_color), block_rect);
				}				
				
				block_rect.X  += increment;
			}
		}
		
		public override Size ProgressBarDefaultSize {
			get {
				return new Size (100, 23);
			}
		}

		#endregion	// ProgressBar

		#region RadioButton
		public override void DrawRadioButton (Graphics dc, Rectangle clip_rectangle, RadioButton radio_button) {
			StringFormat	text_format;
			Rectangle 	client_rectangle;
			Rectangle	text_rectangle;
			Rectangle 	radiobutton_rectangle;
			int		radiobutton_size = 12;
			int 	radiobutton_space = 4;

			client_rectangle = radio_button.ClientRectangle;
			text_rectangle = client_rectangle;
			radiobutton_rectangle = new Rectangle(text_rectangle.X, text_rectangle.Y, radiobutton_size, radiobutton_size);

			text_format = new StringFormat();
			text_format.Alignment = StringAlignment.Near;
			text_format.LineAlignment = StringAlignment.Center;
			text_format.HotkeyPrefix = HotkeyPrefix.Show;

			/* Calculate the position of text and checkbox rectangle */
			if (radio_button.appearance!=Appearance.Button) {
				switch(radio_button.radiobutton_alignment) {
				case ContentAlignment.BottomCenter: {
					radiobutton_rectangle.X=(client_rectangle.Right-client_rectangle.Left)/2-radiobutton_size/2;
					radiobutton_rectangle.Y=client_rectangle.Bottom-radiobutton_size;
					text_rectangle.X=client_rectangle.X;
					text_rectangle.Width=client_rectangle.Width;
					text_rectangle.Height=client_rectangle.Height-radiobutton_size-radiobutton_space;
					break;
				}

				case ContentAlignment.BottomLeft: {
					radiobutton_rectangle.X=client_rectangle.Left;
					radiobutton_rectangle.Y=client_rectangle.Bottom-radiobutton_size;
					text_rectangle.X=client_rectangle.X+radiobutton_size+radiobutton_space;
					text_rectangle.Width=client_rectangle.Width-radiobutton_size-radiobutton_space;					
					break;
				}

				case ContentAlignment.BottomRight: {
					radiobutton_rectangle.X=client_rectangle.Right-radiobutton_size;
					radiobutton_rectangle.Y=client_rectangle.Bottom-radiobutton_size;
					text_rectangle.X=client_rectangle.X;
					text_rectangle.Width=client_rectangle.Width-radiobutton_size-radiobutton_space;
					break;
				}

				case ContentAlignment.MiddleCenter: {
					radiobutton_rectangle.X=(client_rectangle.Right-client_rectangle.Left)/2-radiobutton_size/2;
					radiobutton_rectangle.Y=(client_rectangle.Bottom-client_rectangle.Top)/2-radiobutton_size/2;
					text_rectangle.X=client_rectangle.X;
					text_rectangle.Width=client_rectangle.Width;
					break;
				}

				default:
				case ContentAlignment.MiddleLeft: {
					radiobutton_rectangle.X=client_rectangle.Left;
					radiobutton_rectangle.Y=(client_rectangle.Bottom-client_rectangle.Top)/2-radiobutton_size/2;
					text_rectangle.X=client_rectangle.X+radiobutton_size+radiobutton_space;
					text_rectangle.Width=client_rectangle.Width-radiobutton_size-radiobutton_space;
					break;
				}

				case ContentAlignment.MiddleRight: {
					radiobutton_rectangle.X=client_rectangle.Right-radiobutton_size;
					radiobutton_rectangle.Y=(client_rectangle.Bottom-client_rectangle.Top)/2-radiobutton_size/2;
					text_rectangle.X=client_rectangle.X;
					text_rectangle.Width=client_rectangle.Width-radiobutton_size-radiobutton_space;
					break;
				}

				case ContentAlignment.TopCenter: {
					radiobutton_rectangle.X=(client_rectangle.Right-client_rectangle.Left)/2-radiobutton_size/2;
					radiobutton_rectangle.Y=client_rectangle.Top;
					text_rectangle.X=client_rectangle.X;
					text_rectangle.Y=radiobutton_size+radiobutton_space;
					text_rectangle.Width=client_rectangle.Width;
					text_rectangle.Height=client_rectangle.Height-radiobutton_size-radiobutton_space;
					break;
				}

				case ContentAlignment.TopLeft: {
					radiobutton_rectangle.X=client_rectangle.Left;
					radiobutton_rectangle.Y=client_rectangle.Top;
					text_rectangle.X=client_rectangle.X+radiobutton_size+radiobutton_space;
					text_rectangle.Width=client_rectangle.Width-radiobutton_size-radiobutton_space;
					break;
				}

				case ContentAlignment.TopRight: {
					radiobutton_rectangle.X=client_rectangle.Right-radiobutton_size;
					radiobutton_rectangle.Y=client_rectangle.Top;
					text_rectangle.X=client_rectangle.X;
					text_rectangle.Width=client_rectangle.Width-radiobutton_size-radiobutton_space;
					break;
				}
				}
			} else {
				text_rectangle.X=client_rectangle.X;
				text_rectangle.Width=client_rectangle.Width;
			}
			
			/* Set the horizontal alignment of our text */
			switch(radio_button.text_alignment) {
				case ContentAlignment.BottomLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.TopLeft: {
					text_format.Alignment=StringAlignment.Near;
					break;
				}

				case ContentAlignment.BottomCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.TopCenter: {
					text_format.Alignment=StringAlignment.Center;
					break;
				}

				case ContentAlignment.BottomRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.TopRight: {
					text_format.Alignment=StringAlignment.Far;
					break;
				}
			}

			/* Set the vertical alignment of our text */
			switch(radio_button.text_alignment) {
				case ContentAlignment.TopLeft: 
				case ContentAlignment.TopCenter: 
				case ContentAlignment.TopRight: {
					text_format.LineAlignment=StringAlignment.Near;
					break;
				}

				case ContentAlignment.BottomLeft:
				case ContentAlignment.BottomCenter:
				case ContentAlignment.BottomRight: {
					text_format.LineAlignment=StringAlignment.Far;
					break;
				}

				case ContentAlignment.MiddleLeft:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.MiddleRight: {
					text_format.LineAlignment=StringAlignment.Center;
					break;
				}
			}

			ButtonState state = ButtonState.Normal;
			if (radio_button.FlatStyle == FlatStyle.Flat) {
				state |= ButtonState.Flat;
			}
			
			if (radio_button.Checked) {
				state |= ButtonState.Checked;
			}

			// Start drawing
			RadioButton_DrawButton(radio_button, dc, state, radiobutton_rectangle);
			
			RadioButton_DrawText(radio_button, text_rectangle, dc, text_format);

			RadioButton_DrawFocus(radio_button, dc, text_rectangle);			
			text_format.Dispose ();
		}

		protected virtual void RadioButton_DrawButton(RadioButton radio_button, Graphics dc, ButtonState state, Rectangle radiobutton_rectangle)
		{
			SolidBrush sb = new SolidBrush(radio_button.BackColor);
			dc.FillRectangle(sb, radio_button.ClientRectangle);
			sb.Dispose();
			
			if (radio_button.appearance==Appearance.Button) {
				if (radio_button.FlatStyle == FlatStyle.Flat || radio_button.FlatStyle == FlatStyle.Popup) {
					DrawFlatStyleButton(dc, radio_button.ClientRectangle, radio_button);
				} else {				
					CPDrawButton(dc, radio_button.ClientRectangle, state);
				}		
			} else {
				// establish if we are rendering a flat style of some sort
				if (radio_button.FlatStyle == FlatStyle.Flat || radio_button.FlatStyle == FlatStyle.Popup) {
					DrawFlatStyleRadioButton (dc, radiobutton_rectangle, radio_button);
				} else {
					ControlPaint.DrawRadioButton (dc, radiobutton_rectangle, state);
				}
			}
		}
		
		protected virtual void RadioButton_DrawText(RadioButton radio_button, Rectangle text_rectangle, Graphics dc, StringFormat text_format)
		{
			// offset the text if it's pressed and a button
			if (radio_button.Appearance == Appearance.Button) {
				if (radio_button.Checked || (radio_button.Capture && radio_button.FlatStyle != FlatStyle.Flat)) {
					text_rectangle.X ++;
					text_rectangle.Y ++;
				}
				
				text_rectangle.Inflate(-4,-4);
			} 
			
			/* Place the text; to be compatible with Windows place it after the radiobutton has been drawn */			

			// Windows seems to not wrap text in certain situations, this matches as close as I could get it
			if ((float)(radio_button.Font.Height * 1.5f) > text_rectangle.Height) {
				text_format.FormatFlags |= StringFormatFlags.NoWrap;
			}

			if (radio_button.Enabled) {
				dc.DrawString (radio_button.Text, radio_button.Font, ResPool.GetSolidBrush (radio_button.ForeColor), text_rectangle, text_format);
			} else if (radio_button.FlatStyle == FlatStyle.Flat) {
				dc.DrawString(radio_button.Text, radio_button.Font, ResPool.GetSolidBrush (ControlPaint.DarkDark (this.ColorControl)), text_rectangle, text_format);
			} else {
				CPDrawStringDisabled(dc, radio_button.Text, radio_button.Font, this.ColorControlText, text_rectangle, text_format);
			}
		}
		
		protected virtual void RadioButton_DrawFocus(RadioButton radio_button, Graphics dc, Rectangle text_rectangle)
		{
			if (radio_button.Focused) {
				if (radio_button.FlatStyle != FlatStyle.Flat) {
					DrawInnerFocusRectangle (dc, text_rectangle, radio_button.BackColor);
				} else {
					dc.DrawRectangle (ResPool.GetPen (radio_button.ForeColor), text_rectangle);
				}
			}
		}

		// renders a radio button with the Flat and Popup FlatStyle
		protected void DrawFlatStyleRadioButton (Graphics graphics, Rectangle rectangle, RadioButton radio_button)
		{
			int	lineWidth;
			
			if (radio_button.Enabled) {
				// draw the outer flatstyle arcs
				if (radio_button.FlatStyle == FlatStyle.Flat) {
					graphics.DrawArc (ResPool.GetPen (radio_button.ForeColor), rectangle, 0, 359);
					
					// fill in the area depending on whether or not the mouse is hovering
					if (radio_button.is_entered && radio_button.Capture) {
						graphics.FillPie (ResPool.GetSolidBrush (ControlPaint.Light (radio_button.BackColor)), rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2, 0, 359);
					} else {
						graphics.FillPie (ResPool.GetSolidBrush (ControlPaint.LightLight (radio_button.BackColor)), rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2, 0, 359);
					}
				} else {
					// must be a popup radio button
					// fill the control
					graphics.FillPie (ResPool.GetSolidBrush (ControlPaint.LightLight (radio_button.BackColor)), rectangle, 0, 359);

					if (radio_button.is_entered || radio_button.Capture) {
						// draw the popup 3d button knob
						graphics.DrawArc (ResPool.GetPen (ControlPaint.Light (radio_button.BackColor)), rectangle.X+1, rectangle.Y+1, rectangle.Width-2, rectangle.Height-2, 0, 359);

						graphics.DrawArc (ResPool.GetPen (ControlPaint.Dark (radio_button.BackColor)), rectangle, 135, 180);
						graphics.DrawArc (ResPool.GetPen (ControlPaint.LightLight (radio_button.BackColor)), rectangle, 315, 180);
						
					} else {
						// just draw lighter flatstyle outer circle
						graphics.DrawArc (ResPool.GetPen (ControlPaint.Dark (this.ColorControl)), rectangle, 0, 359);						
					}										
				}
			} else {
				// disabled
				// fill control background color regardless of actual backcolor
				graphics.FillPie (ResPool.GetSolidBrush (this.ColorControl), rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2, 0, 359);
				// draw the ark as control dark
				graphics.DrawArc (ResPool.GetPen (ControlPaint.Dark(this.ColorControl)), rectangle, 0, 359);
			}

			// draw the check
			lineWidth = Math.Max (1, Math.Min(rectangle.Width, rectangle.Height)/3);
			if (radio_button.Checked) {
				SolidBrush buttonBrush;

				if (!radio_button.Enabled) {
					buttonBrush = ResPool.GetSolidBrush (ControlPaint.Dark (this.ColorControl));
				} else if (radio_button.FlatStyle == FlatStyle.Popup && radio_button.is_entered && radio_button.Capture) {
					buttonBrush = ResPool.GetSolidBrush (this.ColorControlText);
				} else {
					buttonBrush = ResPool.GetSolidBrush (radio_button.ForeColor);
				}
				graphics.FillPie (buttonBrush, rectangle.X+lineWidth, rectangle.Y+lineWidth, rectangle.Width-lineWidth*2, rectangle.Height-lineWidth*2, 0, 359);
			}	
		}

		public override Size RadioButtonDefaultSize {
			get {
				return new Size (104,24);
			}
		}
		#endregion	// RadioButton

		#region ScrollBar
		public override void DrawScrollBar (Graphics dc, Rectangle clip, ScrollBar bar)
		{
			int		scrollbutton_width = bar.scrollbutton_width;
			int		scrollbutton_height = bar.scrollbutton_height;
			Rectangle	first_arrow_area;
			Rectangle	second_arrow_area;			
			Rectangle	thumb_pos;
			
			thumb_pos = bar.ThumbPos;

			if (bar.vert) {
				first_arrow_area = new Rectangle(0, 0, bar.Width, scrollbutton_height);
				bar.FirstArrowArea = first_arrow_area;

				second_arrow_area = new Rectangle(0, bar.ClientRectangle.Height - scrollbutton_height, bar.Width, scrollbutton_height);
				bar.SecondArrowArea = second_arrow_area;

				thumb_pos.Width = bar.Width;
				bar.ThumbPos = thumb_pos;

				/* Buttons */
				if (clip.IntersectsWith (first_arrow_area))
					CPDrawScrollButton (dc, first_arrow_area, ScrollButton.Up, bar.firstbutton_state);
				if (clip.IntersectsWith (second_arrow_area))
					CPDrawScrollButton (dc, second_arrow_area, ScrollButton.Down, bar.secondbutton_state);

				/* Background */
				switch (bar.thumb_moving) {
				case ScrollBar.ThumbMoving.None: {
					ScrollBar_Vertical_Draw_ThumbMoving_None(scrollbutton_height, bar, clip, dc);
					break;
				}
				case ScrollBar.ThumbMoving.Forward: {
					ScrollBar_Vertical_Draw_ThumbMoving_Forward(scrollbutton_height, bar, thumb_pos, clip, dc);
					break;
				}
				
				case ScrollBar.ThumbMoving.Backwards: {
					ScrollBar_Vertical_Draw_ThumbMoving_Backwards(scrollbutton_height, bar, thumb_pos, clip, dc);
					break;
				}
				
				default:
					break;
				}
			} else {
				first_arrow_area = new Rectangle(0, 0, scrollbutton_width, bar.Height);
				bar.FirstArrowArea = first_arrow_area;

				second_arrow_area = new Rectangle (bar.ClientRectangle.Width - scrollbutton_width, 0, scrollbutton_width, bar.Height);
				bar.SecondArrowArea = second_arrow_area;

				thumb_pos.Height = bar.Height;
				bar.ThumbPos = thumb_pos;

				/* Buttons */
				if (clip.IntersectsWith (first_arrow_area))
					CPDrawScrollButton (dc, first_arrow_area, ScrollButton.Left, bar.firstbutton_state);
				if (clip.IntersectsWith (second_arrow_area))
					CPDrawScrollButton (dc, second_arrow_area, ScrollButton.Right, bar.secondbutton_state);

				/* Background */					
				switch (bar.thumb_moving) {
				case ScrollBar.ThumbMoving.None: {
					ScrollBar_Horizontal_Draw_ThumbMoving_None(scrollbutton_width, bar, clip, dc);
					break;
				}
				
				case ScrollBar.ThumbMoving.Forward: {
					ScrollBar_Horizontal_Draw_ThumbMoving_Forward(scrollbutton_width, thumb_pos, bar, clip, dc);
					break;
				}
				
				case ScrollBar.ThumbMoving.Backwards: {
					ScrollBar_Horizontal_Draw_ThumbMoving_Backwards(scrollbutton_width, thumb_pos, bar, clip, dc);
					break;
				}
				}
			}

			/* Thumb */
			ScrollBar_DrawThumb(bar, thumb_pos, clip, dc);				
		}

		protected virtual void ScrollBar_DrawThumb(ScrollBar bar, Rectangle thumb_pos, Rectangle clip, Graphics dc)
		{
			if (bar.Enabled && thumb_pos.Width > 0 && thumb_pos.Height > 0 && clip.IntersectsWith(thumb_pos))
				DrawScrollButtonPrimitive(dc, thumb_pos, ButtonState.Normal);
		}

		protected virtual void ScrollBar_Vertical_Draw_ThumbMoving_None( int scrollbutton_height, ScrollBar bar, Rectangle clip, Graphics dc )
		{
			Rectangle r = new Rectangle( 0,	 
						    scrollbutton_height, bar.ClientRectangle.Width, bar.ClientRectangle.Height - ( scrollbutton_height * 2 ) );
			Rectangle intersect = Rectangle.Intersect( clip, r );
			
			if ( intersect != Rectangle.Empty )
			{
				Brush h = ResPool.GetHatchBrush( HatchStyle.Percent50, ColorScrollBar, Color.White);
				dc.FillRectangle( h, intersect );
			}
		}
		
		protected virtual void ScrollBar_Vertical_Draw_ThumbMoving_Forward( int scrollbutton_height, ScrollBar bar, Rectangle thumb_pos, Rectangle clip, Graphics dc )
		{
			Rectangle r = new Rectangle( 0,	 scrollbutton_height,
						    bar.ClientRectangle.Width, thumb_pos.Y - scrollbutton_height );
			Rectangle intersect = Rectangle.Intersect( clip, r );
			
			if ( intersect != Rectangle.Empty )
				dc.FillRectangle( ResPool.GetHatchBrush( HatchStyle.Percent50, ColorScrollBar, Color.White ), intersect );
			
			r.X = 0;
			r.Y = thumb_pos.Y + thumb_pos.Height;
			r.Width = bar.ClientRectangle.Width;
			r.Height = bar.ClientRectangle.Height -	 ( thumb_pos.Y + thumb_pos.Height ) - scrollbutton_height;
			
			intersect = Rectangle.Intersect( clip, r );
			if ( intersect != Rectangle.Empty )
				dc.FillRectangle( ResPool.GetHatchBrush( HatchStyle.Percent50, Color.FromArgb( 255, 63, 63, 63 ), Color.Black ), intersect );
		}
		
		protected virtual void ScrollBar_Vertical_Draw_ThumbMoving_Backwards( int scrollbutton_height, ScrollBar bar, Rectangle thumb_pos, Rectangle clip, Graphics dc )
		{
			Rectangle r = new Rectangle( 0,	 scrollbutton_height,
						    bar.ClientRectangle.Width, thumb_pos.Y - scrollbutton_height );
			Rectangle intersect = Rectangle.Intersect( clip, r );
			
			if ( intersect != Rectangle.Empty )
				dc.FillRectangle( ResPool.GetHatchBrush( HatchStyle.Percent50, Color.FromArgb( 255, 63, 63, 63 ), Color.Black ), intersect );
			
			r.X = 0;
			r.Y = thumb_pos.Y + thumb_pos.Height;
			r.Width = bar.ClientRectangle.Width; 
			r.Height = bar.ClientRectangle.Height -	 ( thumb_pos.Y + thumb_pos.Height ) - scrollbutton_height;
			
			intersect = Rectangle.Intersect( clip, r );
			if ( intersect != Rectangle.Empty )
				dc.FillRectangle( ResPool.GetHatchBrush( HatchStyle.Percent50, ColorScrollBar, Color.White), intersect );
		}
		
		protected virtual void ScrollBar_Horizontal_Draw_ThumbMoving_None( int scrollbutton_width, ScrollBar bar, Rectangle clip, Graphics dc )
		{
			Rectangle r = new Rectangle( scrollbutton_width,
						    0, bar.ClientRectangle.Width - ( scrollbutton_width * 2 ), bar.ClientRectangle.Height );
			Rectangle intersect = Rectangle.Intersect( clip, r );
			
			if ( intersect != Rectangle.Empty )
				dc.FillRectangle( ResPool.GetHatchBrush( HatchStyle.Percent50, ColorScrollBar, Color.White), intersect );
		}
		
		protected virtual void ScrollBar_Horizontal_Draw_ThumbMoving_Forward( int scrollbutton_width, Rectangle thumb_pos, ScrollBar bar, Rectangle clip, Graphics dc )
		{
			Rectangle r = new Rectangle( scrollbutton_width,  0,
						    thumb_pos.X - scrollbutton_width, bar.ClientRectangle.Height );
			Rectangle intersect = Rectangle.Intersect( clip, r );
			
			if ( intersect != Rectangle.Empty )
				dc.FillRectangle( ResPool.GetHatchBrush( HatchStyle.Percent50, ColorScrollBar, Color.White), intersect );
			
			r.X = thumb_pos.X + thumb_pos.Width;
			r.Y = 0;
			r.Width = bar.ClientRectangle.Width -  ( thumb_pos.X + thumb_pos.Width ) - scrollbutton_width;
			r.Height = bar.ClientRectangle.Height;
			
			intersect = Rectangle.Intersect( clip, r );
			if ( intersect != Rectangle.Empty )
				dc.FillRectangle( ResPool.GetHatchBrush( HatchStyle.Percent50, Color.FromArgb( 255, 63, 63, 63 ), Color.Black ), intersect );
		}
		
		protected virtual void ScrollBar_Horizontal_Draw_ThumbMoving_Backwards( int scrollbutton_width, Rectangle thumb_pos, ScrollBar bar, Rectangle clip, Graphics dc )
		{
			Rectangle r = new Rectangle( scrollbutton_width,  0,
						    thumb_pos.X - scrollbutton_width, bar.ClientRectangle.Height );
			Rectangle intersect = Rectangle.Intersect( clip, r );
			
			if ( intersect != Rectangle.Empty )
				dc.FillRectangle( ResPool.GetHatchBrush( HatchStyle.Percent50, Color.FromArgb( 255, 63, 63, 63 ), Color.Black ), intersect );
			
			r.X = thumb_pos.X + thumb_pos.Width;
			r.Y = 0;
			r.Width = bar.ClientRectangle.Width -  ( thumb_pos.X + thumb_pos.Width ) - scrollbutton_width;
			r.Height = bar.ClientRectangle.Height;
			
			intersect = Rectangle.Intersect( clip, r );
			if ( intersect != Rectangle.Empty )
				dc.FillRectangle( ResPool.GetHatchBrush( HatchStyle.Percent50, ColorScrollBar, Color.White), intersect );
		}

		public override int ScrollBarButtonSize {
			get { return 16; }
		}
		#endregion	// ScrollBar

		#region StatusBar
		public	override void DrawStatusBar (Graphics dc, Rectangle clip, StatusBar sb) {
			Rectangle area = sb.ClientRectangle;
			int horz_border = 2;
			int vert_border = 2;

			dc.FillRectangle (GetControlBackBrush (sb.BackColor), clip);
			
			if (sb.Panels.Count == 0 && sb.Text != String.Empty) {
				string text = sb.Text;
				StringFormat string_format = new StringFormat ();
				string_format.Trimming = StringTrimming.Character;
				string_format.FormatFlags = StringFormatFlags.NoWrap;

				if (text [0] == '\t') {
					string_format.Alignment = StringAlignment.Center;
					text = text.Substring (1);
					if (text [0] == '\t') {
						string_format.Alignment = StringAlignment.Far;
						text = text.Substring (1);
					}
				}
		
				dc.DrawString (text, sb.Font, ResPool.GetSolidBrush (sb.ForeColor),
						new Rectangle(area.X + 2, area.Y + 2, area.Width - 4, area.Height - 4), string_format);
				string_format.Dispose ();
			} else if (sb.ShowPanels) {
				SolidBrush br_forecolor = GetControlForeBrush (sb.ForeColor);
				int prev_x = area.X + horz_border;
				int y = area.Y + vert_border;
				for (int i = 0; i < sb.Panels.Count; i++) {
					Rectangle pr = new Rectangle (prev_x, y,
						sb.Panels [i].Width, area.Height);
					prev_x += pr.Width + StatusBarHorzGapWidth;
					if (pr.IntersectsWith (clip))
						DrawStatusBarPanel (dc, pr, i, br_forecolor, sb.Panels [i]);
				}
			}

			if (sb.SizingGrip)
				CPDrawSizeGrip (dc, ColorControl, area);

		}


		protected virtual void DrawStatusBarPanel (Graphics dc, Rectangle area, int index,
			SolidBrush br_forecolor, StatusBarPanel panel) {
			int border_size = 3; // this is actually const, even if the border style is none

			area.Height -= border_size;
			if (panel.BorderStyle != StatusBarPanelBorderStyle.None) {
				Border3DStyle border_style = Border3DStyle.SunkenInner;
				if (panel.BorderStyle == StatusBarPanelBorderStyle.Raised)
					border_style = Border3DStyle.RaisedOuter;
				CPDrawBorder3D(dc, area, border_style, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom, panel.Parent.BackColor);
			}

			if (panel.Style == StatusBarPanelStyle.OwnerDraw) {
				StatusBarDrawItemEventArgs e = new StatusBarDrawItemEventArgs (
					dc, panel.Parent.Font, area, index, DrawItemState.Default,
					panel, panel.Parent.ForeColor, panel.Parent.BackColor);
				panel.Parent.OnDrawItemInternal (e);
				return;
			}

			int left = area.Left;
			if (panel.Icon != null) {
				left += 2;
				dc.DrawIcon (panel.Icon, left, area.Top);
				left += panel.Icon.Width;
			}

			if (panel.Text == String.Empty)
				return;

			string text = panel.Text;
			StringFormat string_format = new StringFormat ();
			string_format.Trimming = StringTrimming.Character;
			string_format.FormatFlags = StringFormatFlags.NoWrap;

			if (text [0] == '\t') {
				string_format.Alignment = StringAlignment.Center;
				text = text.Substring (1);
				if (text [0] == '\t') {
					string_format.Alignment = StringAlignment.Far;
					text = text.Substring (1);
				}
			}

			int x = left + border_size;
			int y = border_size + 2;
			Rectangle r = new Rectangle (x, y, 
				area.Right - x - border_size,
				area.Bottom - y - border_size);
			
			dc.DrawString (text, panel.Parent.Font, br_forecolor, r, string_format);
			string_format.Dispose ();
		}

		public override int StatusBarSizeGripWidth {
			get { return 15; }
		}

		public override int StatusBarHorzGapWidth {
			get { return 3; }
		}

		public override Size StatusBarDefaultSize {
			get {
				return new Size (100, 22);
			}
		}
		#endregion	// StatusBar

		public override void DrawTabControl (Graphics dc, Rectangle area, TabControl tab)
		{
			// Do we need to fill the back color? It can't be changed...
			dc.FillRectangle (GetControlBackBrush (tab.BackColor), area);
			Rectangle panel_rect = GetTabPanelRectExt (tab);

			if (tab.Appearance == TabAppearance.Normal) {
				CPDrawBorder3D (dc, panel_rect, Border3DStyle.RaisedInner, Border3DSide.Left | Border3DSide.Top, ColorControl);
				CPDrawBorder3D (dc, panel_rect, Border3DStyle.Raised, Border3DSide.Right | Border3DSide.Bottom, ColorControl);
			}

			if (tab.Alignment == TabAlignment.Top) {
				for (int r = tab.TabPages.Count; r > 0; r--) {
					for (int i = tab.SliderPos; i < tab.TabPages.Count; i++) {
						if (i == tab.SelectedIndex)
							continue;
						if (r != tab.TabPages [i].Row)
							continue;
						Rectangle rect = tab.GetTabRect (i);
						if (!rect.IntersectsWith (area))
							continue;
						DrawTab (dc, tab.TabPages [i], tab, rect, false);
					}
				}
			} else {
				for (int r = 0; r < tab.TabPages.Count; r++) {
					for (int i = tab.SliderPos; i < tab.TabPages.Count; i++) {
						if (i == tab.SelectedIndex)
							continue;
						if (r != tab.TabPages [i].Row)
							continue;
						Rectangle rect = tab.GetTabRect (i);
						if (!rect.IntersectsWith (area))
							continue;
						DrawTab (dc, tab.TabPages [i], tab, rect, false);
					}
				}
			}

			if (tab.SelectedIndex != -1 && tab.SelectedIndex >= tab.SliderPos) {
				Rectangle rect = tab.GetTabRect (tab.SelectedIndex);
				if (rect.IntersectsWith (area))
					DrawTab (dc, tab.TabPages [tab.SelectedIndex], tab, rect, true);
			}

			if (tab.ShowSlider) {
				Rectangle right = GetTabControlRightScrollRect (tab);
				Rectangle left = GetTabControlLeftScrollRect (tab);
				CPDrawScrollButton (dc, right, ScrollButton.Right, tab.RightSliderState);
				CPDrawScrollButton (dc, left, ScrollButton.Left, tab.LeftSliderState);
			}
		}

		public override Rectangle GetTabControlLeftScrollRect (TabControl tab)
		{
			switch (tab.Alignment) {
			case TabAlignment.Top:
				return new Rectangle (tab.ClientRectangle.Right - 34, tab.ClientRectangle.Top + 1, 17, 17);
			default:
				Rectangle panel_rect = GetTabPanelRectExt (tab);
				return new Rectangle (tab.ClientRectangle.Right - 34, panel_rect.Bottom + 2, 17, 17);
			}
		}

		public override Rectangle GetTabControlRightScrollRect (TabControl tab)
		{
			switch (tab.Alignment) {
			case TabAlignment.Top:
				return new Rectangle (tab.ClientRectangle.Right - 17, tab.ClientRectangle.Top + 1, 17, 17);
			default:
				Rectangle panel_rect = GetTabPanelRectExt (tab);
				return new Rectangle (tab.ClientRectangle.Right - 17, panel_rect.Bottom + 2, 17, 17);
			}
		}

		public override Size TabControlDefaultItemSize {
			get { return new Size (42, 21); }
		}

		public override Point TabControlDefaultPadding {
			get { return new Point (6, 3); }
		}

		public override int TabControlMinimumTabWidth {
			get { return 42; }
		}

		public override Rectangle GetTabControlDisplayRectangle (TabControl tab)
		{
			Rectangle ext = GetTabPanelRectExt (tab);
			// Account for border size
			return new Rectangle (ext.Left + 2, ext.Top + 1, ext.Width - 6, ext.Height - 4);
		}

		public override Size TabControlGetSpacing (TabControl tab) {
			switch (tab.Appearance) {
				case TabAppearance.Normal:
					return new Size (1, -2);
				case TabAppearance.Buttons:
					return new Size (3, 3);
				case TabAppearance.FlatButtons:
					return new Size (9, 3);
				default:
					throw new Exception ("Invalid Appearance value: " + tab.Appearance);
				}
		}

		protected virtual Rectangle GetTabPanelRectExt (TabControl tab)
		{
			// Offset the tab from the top corner
			Rectangle res = new Rectangle (tab.ClientRectangle.X + 2,
					tab.ClientRectangle.Y,
					tab.ClientRectangle.Width - 2,
					tab.ClientRectangle.Height - 1);

			if (tab.TabCount == 0)
				return res;

			int spacing = TabControlGetSpacing (tab).Height;
			int offset = (tab.ItemSize.Height + spacing) * tab.RowCount + 3;

			switch (tab.Alignment) {
			case TabAlignment.Left:
				res.X += offset;
				res.Width -= offset;
				break;
			case TabAlignment.Right:
				res.Width -= offset;
				break;
			case TabAlignment.Top:
				res.Y += offset;
				res.Height -= offset;
				break;
			case TabAlignment.Bottom:
				res.Height -= offset;
				break;
			}

			return res;
		}

		protected virtual int DrawTab (Graphics dc, TabPage page, TabControl tab, Rectangle bounds, bool is_selected)
		{
			int FlatButtonSpacing = 8;
			Rectangle interior;
			int res = bounds.Width;

			
			
			// we can't fill the background right away because the bounds might be adjusted if the tab is selected

			StringFormat string_format = new StringFormat ();
			if (tab.Appearance == TabAppearance.Buttons || tab.Appearance == TabAppearance.FlatButtons) {
				dc.FillRectangle (GetControlBackBrush (tab.BackColor), bounds);

				// Separators
				if (tab.Appearance == TabAppearance.FlatButtons) {
					int width = bounds.Width;
					bounds.Width += (FlatButtonSpacing - 2);
					res = bounds.Width;
					CPDrawBorder3D (dc, bounds, Border3DStyle.Etched, Border3DSide.Right);
					bounds.Width = width;
				}

				if (is_selected) {
					CPDrawBorder3D (dc, bounds, Border3DStyle.Sunken, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom);
				} else if (tab.Appearance != TabAppearance.FlatButtons) {
					CPDrawBorder3D (dc, bounds, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom);
				}

				interior = new Rectangle (bounds.Left + 2, bounds.Top + 2, bounds.Width - 4, bounds.Height - 4);

                                
				string_format.Alignment = StringAlignment.Center;
				string_format.LineAlignment = StringAlignment.Center;
				string_format.FormatFlags = StringFormatFlags.NoWrap;
			} else {
				Pen light = ResPool.GetPen (ControlPaint.LightLight (tab.BackColor));

				switch (tab.Alignment) {
					
				case TabAlignment.Top:


					dc.FillRectangle (GetControlBackBrush (tab.BackColor), bounds);

					dc.DrawLine (light, bounds.Left, bounds.Bottom, bounds.Left, bounds.Top + 3);
					dc.DrawLine (light, bounds.Left, bounds.Top + 3, bounds.Left + 3, bounds.Top);
					dc.DrawLine (light, bounds.Left + 3, bounds.Top, bounds.Right - 3, bounds.Top);

					dc.DrawLine (SystemPens.ControlDark, bounds.Right - 1, bounds.Top + 1, bounds.Right - 1, bounds.Bottom);
					dc.DrawLine (SystemPens.ControlDarkDark, bounds.Right - 1, bounds.Top + 2, bounds.Right, bounds.Top + 3);
					dc.DrawLine (SystemPens.ControlDarkDark, bounds.Right, bounds.Top + 3, bounds.Right, bounds.Bottom);

					interior = new Rectangle (bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8);

					string_format.Alignment = StringAlignment.Center;
					string_format.LineAlignment = StringAlignment.Center;
					string_format.FormatFlags = StringFormatFlags.NoWrap;

					break;

				case TabAlignment.Bottom:

					dc.FillRectangle (GetControlBackBrush (tab.BackColor), bounds);

					dc.DrawLine (light, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom - 3);
					dc.DrawLine (light, bounds.Left, bounds.Bottom - 3, bounds.Left + 2, bounds.Bottom - 1);

					dc.DrawLine (SystemPens.ControlDark, bounds.Left + 3, bounds.Bottom - 1, bounds.Right - 3, bounds.Bottom - 1);
					dc.DrawLine (SystemPens.ControlDark, bounds.Right - 1, bounds.Bottom - 3, bounds.Right - 1, bounds.Top);

					dc.DrawLine (SystemPens.ControlDarkDark, bounds.Left + 3, bounds.Bottom, bounds.Right - 3, bounds.Bottom);
					dc.DrawLine (SystemPens.ControlDarkDark, bounds.Right - 3, bounds.Bottom, bounds.Right, bounds.Bottom - 3);
					dc.DrawLine (SystemPens.ControlDarkDark, bounds.Right, bounds.Bottom - 3, bounds.Right, bounds.Top);

					interior = new Rectangle (bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8);

					string_format.Alignment = StringAlignment.Center;
					string_format.LineAlignment = StringAlignment.Center;
					string_format.FormatFlags = StringFormatFlags.NoWrap;

					break;

				case TabAlignment.Left:

					dc.FillRectangle (GetControlBackBrush (tab.BackColor), bounds);

					dc.DrawLine (light, bounds.Left, bounds.Bottom - 3, bounds.Left, bounds.Top + 3);
					dc.DrawLine (light, bounds.Left, bounds.Top + 3, bounds.Left + 3, bounds.Top);
					dc.DrawLine (light, bounds.Left + 3, bounds.Top, bounds.Right, bounds.Top);

					dc.DrawLine (SystemPens.ControlDark, bounds.Right, bounds.Bottom - 1, bounds.Left + 2, bounds.Bottom - 1);

					dc.DrawLine (SystemPens.ControlDarkDark, bounds.Right, bounds.Bottom, bounds.Left + 2, bounds.Bottom);
					dc.DrawLine (SystemPens.ControlDarkDark, bounds.Left + 2, bounds.Bottom, bounds.Left, bounds.Bottom - 3);

					interior = new Rectangle (bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8);

					string_format.Alignment = StringAlignment.Center;
					string_format.LineAlignment = StringAlignment.Center;
					string_format.FormatFlags = StringFormatFlags.NoWrap;
					string_format.FormatFlags = StringFormatFlags.DirectionVertical;

					break;

				default:
					// TabAlignment.Right

					dc.FillRectangle (GetControlBackBrush (tab.BackColor), bounds);

					dc.DrawLine (light, bounds.Left, bounds.Top, bounds.Right - 3, bounds.Top);
					dc.DrawLine (light, bounds.Right - 3, bounds.Top, bounds.Right, bounds.Top + 3);

					dc.DrawLine (SystemPens.ControlDark, bounds.Right - 1, bounds.Top + 1, bounds.Right - 1, bounds.Bottom - 1);
					dc.DrawLine (SystemPens.ControlDark, bounds.Left, bounds.Bottom - 1, bounds.Right - 2, bounds.Bottom - 1);

					dc.DrawLine (SystemPens.ControlDarkDark, bounds.Right, bounds.Top + 3, bounds.Right, bounds.Bottom - 3);
					dc.DrawLine (SystemPens.ControlDarkDark, bounds.Left, bounds.Bottom, bounds.Right - 3, bounds.Bottom);

					interior = new Rectangle (bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8);

					string_format.Alignment = StringAlignment.Center;
					string_format.LineAlignment = StringAlignment.Center;
					string_format.FormatFlags = StringFormatFlags.NoWrap;
					string_format.FormatFlags = StringFormatFlags.DirectionVertical;

					break;
				}
			}

			if (tab.DrawMode == TabDrawMode.Normal && page.Text != null) {
				if (tab.Alignment == TabAlignment.Left) {
					int wo = interior.Width / 2;
					int ho = interior.Height / 2;
					dc.TranslateTransform (interior.X + wo, interior.Y + ho);
					dc.RotateTransform (180);
					dc.DrawString (page.Text, page.Font, ResPool.GetSolidBrush (SystemColors.ControlText), 0, 0, string_format);
					dc.ResetTransform ();
				} else {
					dc.DrawString (page.Text, page.Font,
							ResPool.GetSolidBrush (SystemColors.ControlText),
							interior, string_format);
				}
			} else if (page.Text != null) {
				DrawItemState state = DrawItemState.None;
				if (page == tab.SelectedTab)
					state |= DrawItemState.Selected;
				DrawItemEventArgs e = new DrawItemEventArgs (dc,
						tab.Font, bounds, tab.IndexForTabPage (page),
						state, page.ForeColor, page.BackColor);
				tab.OnDrawItemInternal (e);
				return res;
			}

			if (page.parent.Focused && is_selected) {
				CPDrawFocusRectangle (dc, interior, tab.ForeColor, tab.BackColor);
			}

			return res;
		}

		#region ToolBar
		public  override void DrawToolBar (Graphics dc, Rectangle clip_rectangle, ToolBar control) 
		{
			StringFormat format = new StringFormat ();
			format.Trimming = StringTrimming.EllipsisWord;
			format.LineAlignment = StringAlignment.Center;
			if (control.TextAlign == ToolBarTextAlign.Underneath)
				format.Alignment = StringAlignment.Center;
			else
				format.Alignment = StringAlignment.Near;
			
			dc.FillRectangle (ResPool.GetSolidBrush( DefaultControlBackColor ), clip_rectangle);

			foreach (ToolBarButton button in control.Buttons)
				if (button.Visible && clip_rectangle.IntersectsWith (button.Rectangle))
					DrawToolBarButton (dc, control, button, format);

			format.Dispose ();
		}

		void DrawToolBarButton (Graphics dc, ToolBar control, ToolBarButton button, StringFormat format)
		{
			bool is_flat = control.Appearance == ToolBarAppearance.Flat;

			DrawToolBarButtonBorder (dc, button, is_flat);

			switch (button.Style) {
			case ToolBarButtonStyle.DropDownButton:
				if (control.DropDownArrows)
					DrawToolBarDropDownArrow (dc, button, is_flat);
				DrawToolBarButtonContents (dc, control, button, format);
				break;

			case ToolBarButtonStyle.Separator:
				if (is_flat)
					DrawToolBarSeparator (dc, button);
				break;

			case ToolBarButtonStyle.ToggleButton:
				DrawToolBarToggleButtonBackground (dc, button);
				DrawToolBarButtonContents (dc, control, button, format);
				break;

			default:
				DrawToolBarButtonContents (dc, control, button, format);
				break;
			}
		}

		const Border3DSide all_sides = Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom;

		void DrawToolBarButtonBorder (Graphics dc, ToolBarButton button, bool is_flat)
		{
			if (button.Style == ToolBarButtonStyle.Separator)
				return;

			Border3DStyle style;

			if (is_flat) {
				if (button.Pushed || button.Pressed)
					style = Border3DStyle.SunkenOuter;
				else if (button.Hilight)
					style = Border3DStyle.RaisedOuter;
				else
					return;

			} else {
				if (button.Pushed || button.Pressed)
					style = Border3DStyle.Sunken;
				else 
					style = Border3DStyle.Raised;
			}

			CPDrawBorder3D (dc, button.Rectangle, style, all_sides);
		}

		void DrawToolBarSeparator (Graphics dc, ToolBarButton button)
		{
			Rectangle area = button.Rectangle;
			int offset = (int) ResPool.GetPen (ColorControl).Width + 1;
			dc.DrawLine (ResPool.GetPen (ColorControlDark), area.X + 1, area.Y, area.X + 1, area.Bottom);
			dc.DrawLine (ResPool.GetPen (ColorControlLight), area.X + offset, area.Y, area.X + offset, area.Bottom);
		}

		void DrawToolBarToggleButtonBackground (Graphics dc, ToolBarButton button)
		{
			Rectangle area = button.Rectangle;
			area.X += ToolBarImageGripWidth;
			area.Y += ToolBarImageGripWidth;
			area.Width -= 2 * ToolBarImageGripWidth;
			area.Height -= 2 * ToolBarImageGripWidth;

			if (button.Pushed)
				dc.FillRectangle (SystemBrushes.ControlLightLight, area);
			else if (button.PartialPush)
				dc.FillRectangle (SystemBrushes.ControlLight, area);
			else
				dc.FillRectangle (SystemBrushes.Control, area);
		}

		void DrawToolBarDropDownArrow (Graphics dc, ToolBarButton button, bool is_flat)
		{
			Rectangle rect = button.Rectangle;
			rect.X = button.Rectangle.Right - ToolBarDropDownWidth;
			rect.Width = ToolBarDropDownWidth;

			if (button.dd_pressed) {
				CPDrawBorder3D (dc, rect, Border3DStyle.SunkenOuter, all_sides);
				CPDrawBorder3D (dc, rect, Border3DStyle.SunkenInner, Border3DSide.Bottom | Border3DSide.Right);
			} else if (button.Pushed || button.Pressed)
				CPDrawBorder3D (dc, rect, Border3DStyle.Sunken, all_sides);
			else if (is_flat) {
				if (button.Hilight)
					CPDrawBorder3D (dc, rect, Border3DStyle.RaisedOuter, all_sides);
			} else
				CPDrawBorder3D (dc, rect, Border3DStyle.Raised, all_sides);

			PointF [] vertices = new PointF [3];
			PointF ddCenter = new PointF (rect.X + (rect.Width/2.0f), rect.Y + (rect.Height/2.0f));
			vertices [0].X = ddCenter.X - ToolBarDropDownArrowWidth / 2.0f + 0.5f;
			vertices [0].Y = ddCenter.Y;
			vertices [1].X = ddCenter.X + ToolBarDropDownArrowWidth / 2.0f + 0.5f;
			vertices [1].Y = ddCenter.Y;
			vertices [2].X = ddCenter.X + 0.5f; // 0.5 is added for adjustment
			vertices [2].Y = ddCenter.Y + ToolBarDropDownArrowHeight;
			dc.FillPolygon (SystemBrushes.ControlText, vertices);
		}

		void DrawToolBarButtonContents (Graphics dc, ToolBar control, ToolBarButton button, StringFormat format)
		{
			if (button.Image != null) {
				int x = button.ImageRectangle.X + ToolBarImageGripWidth;
				int y = button.ImageRectangle.Y + ToolBarImageGripWidth;
				if (button.Enabled)
					dc.DrawImage (button.Image, x, y);
				else 
					CPDrawImageDisabled (dc, button.Image, x, y, ColorControl);
			}

			if (button.Enabled)
				dc.DrawString (button.Text, control.Font, ResPool.GetSolidBrush (ColorControlText), button.TextRectangle, format);
			else
				CPDrawStringDisabled (dc, button.Text, control.Font, ColorControlLight, button.TextRectangle, format);
		}

		// Grip width for the ToolBar
		public override int ToolBarGripWidth {
			get { return 2;}
		}

		// Grip width for the Image on the ToolBarButton
		public override int ToolBarImageGripWidth {
			get { return 2;}
		}

		// width of the separator
		public override int ToolBarSeparatorWidth {
			get { return 4; }
		}

		// width of the dropdown arrow rect
		public override int ToolBarDropDownWidth {
			get { return 13; }
		}

		// width for the dropdown arrow on the ToolBarButton
		public override int ToolBarDropDownArrowWidth {
			get { return 5;}
		}

		// height for the dropdown arrow on the ToolBarButton
		public override int ToolBarDropDownArrowHeight {
			get { return 3;}
		}

		public override Size ToolBarDefaultSize {
			get {
				return new Size (100, 42);
			}
		}
		#endregion	// ToolBar

		#region ToolTip
		public override void DrawToolTip(Graphics dc, Rectangle clip_rectangle, ToolTip.ToolTipWindow control) {
			dc.FillRectangle(ResPool.GetSolidBrush(this.ColorInfo), control.client_rect);
			dc.DrawRectangle(ResPool.GetPen(this.ColorWindowFrame), 0, 0, control.Width-1, control.Height-1);
			dc.DrawString(control.text, control.Font, ResPool.GetSolidBrush(this.ColorInfoText), control.client_rect, control.string_format);
		}

		public override Size ToolTipSize(ToolTip.ToolTipWindow tt, string text) {
			SizeF	sizef;

			sizef = tt.DeviceContext.MeasureString(text, tt.Font);
			return new Size((int)sizef.Width+2, (int)sizef.Height+3);		// Need space for the border
		}
		#endregion	// ToolTip

		#region	TrackBar
		private void DrawTrackBar_Vertical (Graphics dc, Rectangle clip_rectangle, TrackBar tb,
			ref Rectangle thumb_pos, ref Rectangle thumb_area,  Brush br_thumb,
			float ticks, int value_pos, bool mouse_value) {			

			Point toptick_startpoint = new Point ();
			Point bottomtick_startpoint = new Point ();
			Point channel_startpoint = new Point ();
			float pixel_len;
			float pixels_betweenticks;
			const int space_from_right = 8;
			const int space_from_left = 8;
			const int space_from_bottom = 11;
			Rectangle area = tb.ClientRectangle;
			
			switch (tb.TickStyle) 	{
			case TickStyle.BottomRight:
			case TickStyle.None:
				channel_startpoint.Y = 8;
				channel_startpoint.X = 9;
				bottomtick_startpoint.Y = 13;
				bottomtick_startpoint.X = 24;				
				break;
			case TickStyle.TopLeft:
				channel_startpoint.Y = 8;
				channel_startpoint.X = 19;
				toptick_startpoint.Y = 13;
				toptick_startpoint.X = 8;
				break;
			case TickStyle.Both:
				channel_startpoint.Y = 8;
				channel_startpoint.X = 18;	
				bottomtick_startpoint.Y = 13;
				bottomtick_startpoint.X = 32;				
				toptick_startpoint.Y = 13;
				toptick_startpoint.X = 8;				
				break;
			default:
				break;
			}
			
			thumb_area.X = area.X + channel_startpoint.X;
			thumb_area.Y = area.Y + channel_startpoint.Y;
			thumb_area.Height = area.Height - space_from_right - space_from_left;
			thumb_area.Width = 4;

			/* Draw channel */
			dc.FillRectangle (ResPool.GetSolidBrush (ColorControlDark), channel_startpoint.X, channel_startpoint.Y,
				1, thumb_area.Height);
			
			dc.FillRectangle (ResPool.GetSolidBrush (ColorControlDarkDark), channel_startpoint.X + 1, channel_startpoint.Y,
				1, thumb_area.Height);

			dc.FillRectangle (ResPool.GetSolidBrush (ColorControlLight), channel_startpoint.X + 3, channel_startpoint.Y,
				1, thumb_area.Height);

			pixel_len = thumb_area.Height - 11;
			pixels_betweenticks = pixel_len / (tb.Maximum - tb.Minimum);
			
			/* Convert thumb position from mouse position to value*/
			if (mouse_value) {
				if (value_pos < thumb_area.Bottom)
					value_pos = (int) ((thumb_area.Bottom - value_pos) / pixels_betweenticks);
				else
					value_pos = 0;			

				if (value_pos + tb.Minimum > tb.Maximum)
					value_pos = tb.Maximum - tb.Minimum;

				tb.Value = value_pos + tb.Minimum;
			}			

			// thumb_pos.Y = channel_startpoint.Y ; // + (int) (pixels_betweenticks * (float) value_pos);
			thumb_pos.Y = thumb_area.Bottom - space_from_bottom - (int) (pixels_betweenticks * (float) value_pos);
			
			/* Draw thumb fixed 10x22 size */
			thumb_pos.Width = 10;
			thumb_pos.Height = 22;

			switch (tb.TickStyle) 	{
			case TickStyle.BottomRight:
			case TickStyle.None: {
				thumb_pos.X = channel_startpoint.X - 8;

				dc.DrawLine (ResPool.GetPen (ColorControlLight), thumb_pos.X, thumb_pos.Y, thumb_pos.X , thumb_pos.Y + 10);
				dc.DrawLine (ResPool.GetPen (ColorControlLight), thumb_pos.X, thumb_pos.Y, thumb_pos.X + 16, thumb_pos.Y);
				dc.DrawLine (ResPool.GetPen (ColorControlLight), thumb_pos.X + 16, thumb_pos.Y, thumb_pos.X + 16 + 4, thumb_pos.Y + 4);
				
				dc.DrawLine (ResPool.GetPen (ColorControlDark), thumb_pos.X +1, thumb_pos.Y + 9, thumb_pos.X +15, thumb_pos.Y  +9);
				dc.DrawLine (ResPool.GetPen (ColorControlDark), thumb_pos.X + 16, thumb_pos.Y + 9, thumb_pos.X +16 + 4, thumb_pos.Y  +9 - 4);

				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), thumb_pos.X, thumb_pos.Y  + 10, thumb_pos.X +16, thumb_pos.Y +10);
				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), thumb_pos.X + 16, thumb_pos.Y  + 10, thumb_pos.X  +16 + 5, thumb_pos.Y +10 - 5);

				dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 1, 16, 8);
				dc.FillRectangle (br_thumb, thumb_pos.X + 17, thumb_pos.Y + 2, 1, 6);
				dc.FillRectangle (br_thumb, thumb_pos.X + 18, thumb_pos.Y + 3, 1, 4);
				dc.FillRectangle (br_thumb, thumb_pos.X + 19, thumb_pos.Y + 4, 1, 2);

				break;
			}
			case TickStyle.TopLeft: {
				thumb_pos.X = channel_startpoint.X - 10;

				dc.DrawLine (ResPool.GetPen (ColorControlLight), thumb_pos.X + 4, thumb_pos.Y, thumb_pos.X + 4 + 16, thumb_pos.Y);
				dc.DrawLine (ResPool.GetPen (ColorControlLight), thumb_pos.X + 4, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 4);

				dc.DrawLine (ResPool.GetPen (ColorControlDark), thumb_pos.X  + 4, thumb_pos.Y + 9, thumb_pos.X + 4 + 16 , thumb_pos.Y+ 9);
				dc.DrawLine (ResPool.GetPen (ColorControlDark), thumb_pos.X + 4, thumb_pos.Y  + 9, thumb_pos.X, thumb_pos.Y + 5);
				dc.DrawLine (ResPool.GetPen (ColorControlDark), thumb_pos.X  + 19, thumb_pos.Y + 9, thumb_pos.X  +19 , thumb_pos.Y+ 1);

				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), thumb_pos.X  + 4, thumb_pos.Y+ 10, thumb_pos.X  + 4 + 16, thumb_pos.Y+ 10);
				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), thumb_pos.X  + 4, thumb_pos.Y + 10, thumb_pos.X  -1, thumb_pos.Y+ 5);
				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), thumb_pos.X + 20, thumb_pos.Y, thumb_pos.X+ 20, thumb_pos.Y + 10);

				dc.FillRectangle (br_thumb, thumb_pos.X + 4, thumb_pos.Y + 1, 15, 8);
				dc.FillRectangle (br_thumb, thumb_pos.X + 3, thumb_pos.Y + 2, 1, 6);
				dc.FillRectangle (br_thumb, thumb_pos.X + 2, thumb_pos.Y + 3, 1, 4);
				dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 4, 1, 2);

				break;
			}

			case TickStyle.Both: {
				thumb_pos.X = area.X + 10;
				dc.DrawLine (ResPool.GetPen (ColorControlLight), thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 9);
				dc.DrawLine (ResPool.GetPen (ColorControlLight), thumb_pos.X, thumb_pos.Y, thumb_pos.X + 19, thumb_pos.Y);

				dc.DrawLine (ResPool.GetPen (ColorControlDark), thumb_pos.X + 1, thumb_pos.Y + 9, thumb_pos.X+ 19, thumb_pos.Y  + 9);
				dc.DrawLine (ResPool.GetPen (ColorControlDark), thumb_pos.X  + 10, thumb_pos.Y+ 1, thumb_pos.X + 19, thumb_pos.Y  + 8);

				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), thumb_pos.X, thumb_pos.Y + 10, thumb_pos.X+ 20, thumb_pos.Y  +10);
				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), thumb_pos.X  + 20, thumb_pos.Y, thumb_pos.X  + 20, thumb_pos.Y+ 9);

				dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 1, 18, 8);

				break;
			}

			default:
				break;
			}

			pixel_len = thumb_area.Height - 11;
			pixels_betweenticks = pixel_len / ticks;
			
			thumb_area.X = thumb_pos.X;
			thumb_area.Y = channel_startpoint.Y;
			thumb_area.Width = thumb_pos.Height;
			
			/* Draw ticks*/
			Region outside = new Region (area);
			outside.Exclude (thumb_area);			
			
			if (outside.IsVisible (clip_rectangle)) {				
				if (pixels_betweenticks > 0 && ((tb.TickStyle & TickStyle.BottomRight) == TickStyle.BottomRight ||
					((tb.TickStyle & TickStyle.Both) == TickStyle.Both))) {	
					
					for (float inc = 0; inc < (pixel_len + 1); inc += pixels_betweenticks) 	{					
						if (inc == 0 || (inc +  pixels_betweenticks) >= pixel_len +1)
							dc.DrawLine (ResPool.GetPen (pen_ticks_color), area.X + bottomtick_startpoint.X , area.Y + bottomtick_startpoint.Y  + inc, 
								area.X + bottomtick_startpoint.X  + 3, area.Y + bottomtick_startpoint.Y + inc);
						else
							dc.DrawLine (ResPool.GetPen (pen_ticks_color), area.X + bottomtick_startpoint.X, area.Y + bottomtick_startpoint.Y  + inc, 
								area.X + bottomtick_startpoint.X  + 2, area.Y + bottomtick_startpoint.Y + inc);
					}
				}
	
				if (pixels_betweenticks > 0 &&  ((tb.TickStyle & TickStyle.TopLeft) == TickStyle.TopLeft ||
					((tb.TickStyle & TickStyle.Both) == TickStyle.Both))) {
	
					pixel_len = thumb_area.Height - 11;
					pixels_betweenticks = pixel_len / ticks;
					
					for (float inc = 0; inc < (pixel_len + 1); inc += pixels_betweenticks) {					
						if (inc == 0 || (inc +  pixels_betweenticks) >= pixel_len +1)
							dc.DrawLine (ResPool.GetPen (pen_ticks_color), area.X + toptick_startpoint.X  - 3 , area.Y + toptick_startpoint.Y + inc, 
								area.X + toptick_startpoint.X, area.Y + toptick_startpoint.Y + inc);
						else
							dc.DrawLine (ResPool.GetPen (pen_ticks_color), area.X + toptick_startpoint.X  - 2, area.Y + toptick_startpoint.Y + inc, 
								area.X + toptick_startpoint.X, area.Y + toptick_startpoint.Y  + inc);
					}			
				}
			}
			
			outside.Dispose ();
			
		}

		/* 
			Horizontal trackbar 
		  
			Does not matter the size of the control, Win32 always draws:
				- Ticks starting from pixel 13, 8
				- Channel starting at pos 8, 19 and ends at Width - 8
				- Autosize makes always the control 40 pixels height
				- Ticks are draw at (channel.Witdh - 10) / (Maximum - Minimum)
				
		*/
		private void DrawTrackBar_Horizontal (Graphics dc, Rectangle clip_rectangle, TrackBar tb,
			ref Rectangle thumb_pos, ref Rectangle thumb_area, Brush br_thumb,
			float ticks, int value_pos, bool mouse_value) {			
			Point toptick_startpoint = new Point ();
			Point bottomtick_startpoint = new Point ();
			Point channel_startpoint = new Point ();
			float pixel_len;
			float pixels_betweenticks;
			const int space_from_right = 8;
			const int space_from_left = 8;
			Rectangle area = tb.ClientRectangle;
						
			switch (tb.TickStyle) {
			case TickStyle.BottomRight:
			case TickStyle.None:
				channel_startpoint.X = 8;
				channel_startpoint.Y = 9;
				bottomtick_startpoint.X = 13;
				bottomtick_startpoint.Y = 24;				
				break;
			case TickStyle.TopLeft:
				channel_startpoint.X = 8;
				channel_startpoint.Y = 19;
				toptick_startpoint.X = 13;
				toptick_startpoint.Y = 8;
				break;
			case TickStyle.Both:
				channel_startpoint.X = 8;
				channel_startpoint.Y = 18;	
				bottomtick_startpoint.X = 13;
				bottomtick_startpoint.Y = 32;				
				toptick_startpoint.X = 13;
				toptick_startpoint.Y = 8;				
				break;
			default:
				break;
			}
						
			thumb_area.X = area.X + channel_startpoint.X;
			thumb_area.Y = area.Y + channel_startpoint.Y;
			thumb_area.Width = area.Width - space_from_right - space_from_left;
			thumb_area.Height = 4;
			
			/* Draw channel */
			dc.FillRectangle (ResPool.GetSolidBrush (ColorControlDark), channel_startpoint.X, channel_startpoint.Y,
				thumb_area.Width, 1);
			
			dc.FillRectangle (ResPool.GetSolidBrush (ColorControlDarkDark), channel_startpoint.X, channel_startpoint.Y + 1,
				thumb_area.Width, 1);

			dc.FillRectangle (ResPool.GetSolidBrush (ColorControlLight), channel_startpoint.X, channel_startpoint.Y +3,
				thumb_area.Width, 1);

			pixel_len = thumb_area.Width - 11;
			pixels_betweenticks = pixel_len / (tb.Maximum - tb.Minimum);

			/* Convert thumb position from mouse position to value*/
			if (mouse_value) {			
				if (value_pos >= channel_startpoint.X)
					value_pos = (int)(((float) (value_pos - channel_startpoint.X)) / pixels_betweenticks);
				else
					value_pos = 0;				

				if (value_pos + tb.Minimum > tb.Maximum)
					value_pos = tb.Maximum - tb.Minimum;
                                
				tb.Value = value_pos + tb.Minimum;
			}			
			
			thumb_pos.X = channel_startpoint.X + (int) (pixels_betweenticks * (float) value_pos);
			
			/* Draw thumb fixed 10x22 size */
			thumb_pos.Width = 10;
			thumb_pos.Height = 22;

			switch (tb.TickStyle) {
			case TickStyle.BottomRight:
			case TickStyle.None: {
				thumb_pos.Y = channel_startpoint.Y - 8;

				dc.DrawLine (ResPool.GetPen (ColorControlLight), thumb_pos.X, thumb_pos.Y, thumb_pos.X + 10, thumb_pos.Y);
				dc.DrawLine (ResPool.GetPen (ColorControlLight), thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 16);
				dc.DrawLine (ResPool.GetPen (ColorControlLight), thumb_pos.X, thumb_pos.Y + 16, thumb_pos.X + 4, thumb_pos.Y + 16 + 4);

				dc.DrawLine (ResPool.GetPen (ColorControlDark), thumb_pos.X + 9, thumb_pos.Y + 1, thumb_pos.X +9, thumb_pos.Y +15);
				dc.DrawLine (ResPool.GetPen (ColorControlDark), thumb_pos.X + 9, thumb_pos.Y + 16, thumb_pos.X +9 - 4, thumb_pos.Y +16 + 4);

				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), thumb_pos.X + 10, thumb_pos.Y, thumb_pos.X +10, thumb_pos.Y +16);
				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), thumb_pos.X + 10, thumb_pos.Y + 16, thumb_pos.X +10 - 5, thumb_pos.Y +16 + 5);

				dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 1, 8, 16);
				dc.FillRectangle (br_thumb, thumb_pos.X + 2, thumb_pos.Y + 17, 6, 1);
				dc.FillRectangle (br_thumb, thumb_pos.X + 3, thumb_pos.Y + 18, 4, 1);
				dc.FillRectangle (br_thumb, thumb_pos.X + 4, thumb_pos.Y + 19, 2, 1);
				break;
			}
			case TickStyle.TopLeft:	{
				thumb_pos.Y = channel_startpoint.Y - 10;

				dc.DrawLine (ResPool.GetPen (ColorControlLight), thumb_pos.X, thumb_pos.Y + 4, thumb_pos.X, thumb_pos.Y + 4 + 16);
				dc.DrawLine (ResPool.GetPen (ColorControlLight), thumb_pos.X, thumb_pos.Y + 4, thumb_pos.X + 4, thumb_pos.Y);

				dc.DrawLine (ResPool.GetPen (ColorControlDark), thumb_pos.X + 9, thumb_pos.Y + 4, thumb_pos.X + 9, thumb_pos.Y + 4 + 16);
				dc.DrawLine (ResPool.GetPen (ColorControlDark), thumb_pos.X + 9, thumb_pos.Y + 4, thumb_pos.X + 5, thumb_pos.Y);
				dc.DrawLine (ResPool.GetPen (ColorControlDark), thumb_pos.X + 9, thumb_pos.Y + 19, thumb_pos.X + 1 , thumb_pos.Y +19);

				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), thumb_pos.X + 10, thumb_pos.Y + 4, thumb_pos.X + 10, thumb_pos.Y + 4 + 16);
				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), thumb_pos.X + 10, thumb_pos.Y + 4, thumb_pos.X + 5, thumb_pos.Y -1);
				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), thumb_pos.X, thumb_pos.Y + 20, thumb_pos.X + 10, thumb_pos.Y + 20);

				dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 4, 8, 15);
				dc.FillRectangle (br_thumb, thumb_pos.X + 2, thumb_pos.Y + 3, 6, 1);
				dc.FillRectangle (br_thumb, thumb_pos.X + 3, thumb_pos.Y + 2, 4, 1);
				dc.FillRectangle (br_thumb, thumb_pos.X + 4, thumb_pos.Y + 1, 2, 1);
				break;
			}

			case TickStyle.Both: {
				thumb_pos.Y = area.Y + 10;
				dc.DrawLine (ResPool.GetPen (ColorControlLight), thumb_pos.X, thumb_pos.Y, thumb_pos.X + 9, thumb_pos.Y);
				dc.DrawLine (ResPool.GetPen (ColorControlLight), thumb_pos.X, thumb_pos.Y, thumb_pos.X, thumb_pos.Y + 19);

				dc.DrawLine (ResPool.GetPen (ColorControlDark), thumb_pos.X + 9, thumb_pos.Y + 1, thumb_pos.X + 9, thumb_pos.Y + 19);
				dc.DrawLine (ResPool.GetPen (ColorControlDark), thumb_pos.X + 1, thumb_pos.Y + 10, thumb_pos.X + 8, thumb_pos.Y + 19);

				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), thumb_pos.X + 10, thumb_pos.Y, thumb_pos.X +10, thumb_pos.Y + 20);
				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), thumb_pos.X, thumb_pos.Y + 20, thumb_pos.X + 9, thumb_pos.Y + 20);

				dc.FillRectangle (br_thumb, thumb_pos.X + 1, thumb_pos.Y + 1, 8, 18);

				break;
			}

			default:
				break;
			}

			pixel_len = thumb_area.Width - 11;
			pixels_betweenticks = pixel_len / ticks;

			/* Draw ticks*/
			thumb_area.Y = thumb_pos.Y;
			thumb_area.X = channel_startpoint.X;
			thumb_area.Height = thumb_pos.Height;
			Region outside = new Region (area);
			outside.Exclude (thumb_area);			
			
			if (outside.IsVisible (clip_rectangle)) {				
				if (pixels_betweenticks > 0 && ((tb.TickStyle & TickStyle.BottomRight) == TickStyle.BottomRight ||
					((tb.TickStyle & TickStyle.Both) == TickStyle.Both))) {				
					
					for (float inc = 0; inc < (pixel_len + 1); inc += pixels_betweenticks) {					
						if (inc == 0 || (inc +  pixels_betweenticks) >= pixel_len +1)
							dc.DrawLine (ResPool.GetPen (pen_ticks_color), area.X + bottomtick_startpoint.X + inc , area.Y + bottomtick_startpoint.Y, 
								area.X + bottomtick_startpoint.X + inc , area.Y + bottomtick_startpoint.Y + 3);
						else
							dc.DrawLine (ResPool.GetPen (pen_ticks_color), area.X + bottomtick_startpoint.X + inc, area.Y + bottomtick_startpoint.Y, 
								area.X + bottomtick_startpoint.X + inc, area.Y + bottomtick_startpoint.Y + 2);
					}
				}
	
				if (pixels_betweenticks > 0 && ((tb.TickStyle & TickStyle.TopLeft) == TickStyle.TopLeft ||
					((tb.TickStyle & TickStyle.Both) == TickStyle.Both))) {
					
					for (float inc = 0; inc < (pixel_len + 1); inc += pixels_betweenticks) {					
						if (inc == 0 || (inc +  pixels_betweenticks) >= pixel_len +1)
							dc.DrawLine (ResPool.GetPen (pen_ticks_color), area.X + toptick_startpoint.X + inc , area.Y + toptick_startpoint.Y - 3, 
								area.X + toptick_startpoint.X + inc , area.Y + toptick_startpoint.Y);
						else
							dc.DrawLine (ResPool.GetPen (pen_ticks_color), area.X + toptick_startpoint.X + inc, area.Y + toptick_startpoint.Y - 2, 
								area.X + toptick_startpoint.X + inc, area.Y + toptick_startpoint.Y );
					}			
				}
			}
			
			outside.Dispose ();			
		}

		public override void DrawTrackBar (Graphics dc, Rectangle clip_rectangle, TrackBar tb) 
		{
			Brush		br_thumb;
			int		value_pos;
			bool		mouse_value;
			float		ticks = (tb.Maximum - tb.Minimum) / tb.tickFrequency; /* N of ticks draw*/
			Rectangle	area;
			Rectangle	thumb_pos = tb.ThumbPos;
			Rectangle	thumb_area = tb.ThumbArea;
			
			if (tb.thumb_pressed) {
				value_pos = tb.thumb_mouseclick;
				mouse_value = true;
			} else {
				value_pos = tb.Value - tb.Minimum;
				mouse_value = false;
			}

			area = tb.ClientRectangle;

			if (tb.thumb_pressed == true) {
				br_thumb = (Brush) ResPool.GetHatchBrush (HatchStyle.Percent50, ColorControlLight, ColorControl);
			} else {
				br_thumb = ResPool.GetSolidBrush (ColorControl);
			}

			
			/* Control Background */
			if (tb.BackColor == DefaultControlBackColor) {
				dc.FillRectangle (ResPool.GetSolidBrush (ColorControl), clip_rectangle);
			} else {
				dc.FillRectangle (ResPool.GetSolidBrush (tb.BackColor), clip_rectangle);
			}
			

			if (tb.Focused) {
				dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorControl, Color.Black), area.X, area.Y, area.Width - 1, 1);
				dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorControl, Color.Black), area.X, area.Y + area.Height - 1, area.Width - 1, 1);
				dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorControl, Color.Black), area.X, area.Y, 1, area.Height - 1);
				dc.FillRectangle (ResPool.GetHatchBrush (HatchStyle.Percent50, ColorControl, Color.Black), area.X + area.Width - 1, area.Y, 1, area.Height - 1);
			}

			if (tb.Orientation == Orientation.Vertical) {
				DrawTrackBar_Vertical (dc, clip_rectangle, tb, ref thumb_pos, ref thumb_area,
					br_thumb, ticks, value_pos, mouse_value);
			
			} else {
				DrawTrackBar_Horizontal (dc, clip_rectangle, tb, ref thumb_pos, ref thumb_area,
					br_thumb, ticks, value_pos, mouse_value);
			}

			tb.ThumbPos = thumb_pos;
			tb.ThumbArea = thumb_area;
		}

		public override Size TrackBarDefaultSize {
			get {
				return new Size (104, 42);
			}
		}

		#endregion	// TrackBar

		#region	VScrollBar
		public override Size VScrollBarDefaultSize {
			get {
				return new Size (this.ScrollBarButtonSize, 80);
			}
		}
		#endregion	// VScrollBar

		#region TreeView
		public override Size TreeViewDefaultSize {
			get {
				return new Size (121, 97);
			}
		}

		#endregion

		#region ControlPaint
		private enum DrawFrameControlStates {
			ButtonCheck		= 0x0000,
			ButtonRadioImage	= 0x0001,
			ButtonRadioMask		= 0x0002,
			ButtonRadio		= 0x0004,
			Button3State		= 0x0008,
			ButtonPush		= 0x0010,

			CaptionClose		= 0x0000,
			CaptionMin		= 0x0001,
			CaptionMax		= 0x0002,
			CaptionRestore		= 0x0004,
			CaptionHelp		= 0x0008,

			MenuArrow		= 0x0000,
			MenuCheck		= 0x0001,
			MenuBullet		= 0x0002,
			MenuArrowRight		= 0x0004,

			ScrollUp		= 0x0000,
			ScrollDown		= 0x0001,
			ScrollLeft		= 0x0002,
			ScrollRight		= 0x0003,
			ScrollComboBox		= 0x0005,
			ScrollSizeGrip		= 0x0008,
			ScrollSizeGripRight	= 0x0010,

			Inactive		= 0x0100,
			Pushed			= 0x0200,
			Checked			= 0x0400,
			Transparent		= 0x0800,
			Hot			= 0x1000,
			AdjustRect		= 0x2000,
			Flat			= 0x4000,
			Mono			= 0x8000

		}

		private enum DrawFrameControlTypes {
			Caption	= 1,
			Menu	= 2,
			Scroll	= 3,
			Button	= 4
		}

		public override void CPDrawBorder (Graphics graphics, Rectangle bounds, Color leftColor, int leftWidth,
			ButtonBorderStyle leftStyle, Color topColor, int topWidth, ButtonBorderStyle topStyle,
			Color rightColor, int rightWidth, ButtonBorderStyle rightStyle, Color bottomColor,
			int bottomWidth, ButtonBorderStyle bottomStyle) {
			DrawBorderInternal(graphics, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom-1, leftWidth, leftColor, leftStyle, Border3DSide.Left);
			DrawBorderInternal(graphics, bounds.Left, bounds.Top, bounds.Right-1, bounds.Top, topWidth, topColor, topStyle, Border3DSide.Top);
			DrawBorderInternal(graphics, bounds.Right-1, bounds.Top, bounds.Right-1, bounds.Bottom-1, rightWidth, rightColor, rightStyle, Border3DSide.Right);
			DrawBorderInternal(graphics, bounds.Left, bounds.Bottom-1, bounds.Right-1, bounds.Bottom-1, bottomWidth, bottomColor, bottomStyle, Border3DSide.Bottom);
		}

		public override void CPDrawBorder3D (Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides) {
			CPDrawBorder3D(graphics, rectangle, style, sides, ColorControl);
		}

		private void CPDrawBorder3D (Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides, Color control_color) {
			Pen		penTopLeft;
			Pen		penTopLeftInner;
			Pen		penBottomRight;
			Pen		penBottomRightInner;
			Rectangle	rect= new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
			bool		doInner = false;

			if ((style & Border3DStyle.Adjust)!=0) {
				rect.Y-=2;
				rect.X-=2;
				rect.Width+=4;
				rect.Height+=4;
			}

			/* default to flat */
			penTopLeft=ResPool.GetPen(ControlPaint.Dark(control_color));
			penTopLeftInner=ResPool.GetPen(ControlPaint.Dark(control_color));
			penBottomRight=ResPool.GetPen(ControlPaint.Dark(control_color));
			penBottomRightInner=ResPool.GetPen(ControlPaint.Dark(control_color));

			if ((style & Border3DStyle.RaisedOuter)!=0) {
				penTopLeft=ResPool.GetPen(ControlPaint.LightLight(control_color));
				penBottomRight=ResPool.GetPen(ControlPaint.DarkDark(control_color));
				if ((style & (Border3DStyle.RaisedInner | Border3DStyle.SunkenInner))!=0) {
					doInner=true;
				}
			} else if ((style & Border3DStyle.SunkenOuter)!=0) {
				penTopLeft=ResPool.GetPen(ControlPaint.DarkDark(control_color));
				penBottomRight=ResPool.GetPen(ControlPaint.LightLight(control_color));
				if ((style & (Border3DStyle.RaisedInner | Border3DStyle.SunkenInner))!=0) {
					doInner=true;
				}
			}

			if ((style & Border3DStyle.RaisedInner)!=0) {
				if (doInner) {
					penTopLeftInner=ResPool.GetPen(control_color);
					penBottomRightInner=ResPool.GetPen(ControlPaint.Dark(control_color));
				} else {
					penTopLeft=ResPool.GetPen(ControlPaint.LightLight(control_color));
					penBottomRight=ResPool.GetPen(ControlPaint.DarkDark(control_color));
				}
			} else if ((style & Border3DStyle.SunkenInner)!=0) {
				if (doInner) {
					penTopLeftInner=ResPool.GetPen(ControlPaint.Dark(control_color));
					penBottomRightInner=ResPool.GetPen(control_color);
				} else {
					penTopLeft=ResPool.GetPen(ControlPaint.DarkDark(control_color));
					penBottomRight=ResPool.GetPen(ControlPaint.LightLight(control_color));
				}
			}

			if ((sides & Border3DSide.Middle)!=0) {
				graphics.FillRectangle(ResPool.GetSolidBrush(control_color), rect);
			}

			if ((sides & Border3DSide.Left)!=0) {
				graphics.DrawLine(penTopLeft, rect.Left, rect.Bottom-2, rect.Left, rect.Top);
				if (doInner) {
					graphics.DrawLine(penTopLeftInner, rect.Left+1, rect.Bottom-2, rect.Left+1, rect.Top);
				}
			}

			if ((sides & Border3DSide.Top)!=0) {
				graphics.DrawLine(penTopLeft, rect.Left, rect.Top, rect.Right-2, rect.Top);

				if (doInner) {
					if ((sides & Border3DSide.Left)!=0) {
						graphics.DrawLine(penTopLeftInner, rect.Left+1, rect.Top+1, rect.Right-3, rect.Top+1);
					} else {
						graphics.DrawLine(penTopLeftInner, rect.Left, rect.Top+1, rect.Right-3, rect.Top+1);
					}
				}
			}

			if ((sides & Border3DSide.Right)!=0) {
				graphics.DrawLine(penBottomRight, rect.Right-1, rect.Top, rect.Right-1, rect.Bottom-1);

				if (doInner) {
					if ((sides & Border3DSide.Top)!=0) {
						graphics.DrawLine(penBottomRightInner, rect.Right-2, rect.Top+1, rect.Right-2, rect.Bottom-2);
					} else {
						graphics.DrawLine(penBottomRightInner, rect.Right-2, rect.Top, rect.Right-2, rect.Bottom-2);
					}
				}
			}

			if ((sides & Border3DSide.Bottom)!=0) {
				int	left=rect.Left;

				if ((sides & Border3DSide.Left)!=0) {
					left+=1;
				}

				graphics.DrawLine(penBottomRight, rect.Left, rect.Bottom-1, rect.Right-1, rect.Bottom-1);

				if (doInner) {
					if ((sides & Border3DSide.Right)!=0) {
						graphics.DrawLine(penBottomRightInner, left, rect.Bottom-2, rect.Right-2, rect.Bottom-2);
					} else {
						graphics.DrawLine(penBottomRightInner, left, rect.Bottom-2, rect.Right-2, rect.Bottom-2);
					}
				}
			}

		}


		public override void CPDrawButton (Graphics graphics, Rectangle rectangle, ButtonState state) {
			DrawFrameControlStates	dfcs=DrawFrameControlStates.ButtonPush;

			if ((state & ButtonState.Pushed)!=0) {
				dfcs |= DrawFrameControlStates.Pushed;
			}

			if ((state & ButtonState.Checked)!=0) {
				dfcs |= DrawFrameControlStates.Checked;
			}

			if ((state & ButtonState.Flat)!=0) {
				dfcs |= DrawFrameControlStates.Flat;
			}

			if ((state & ButtonState.Inactive)!=0) {
				dfcs |= DrawFrameControlStates.Inactive;
			}
			DrawFrameControl(graphics, rectangle, DrawFrameControlTypes.Button, dfcs);
		}


		public override void CPDrawCaptionButton (Graphics graphics, Rectangle rectangle, CaptionButton button, ButtonState state) {
			Rectangle	captionRect;
			int			lineWidth;

			CPDrawButton(graphics, rectangle, state);

			if (rectangle.Width<rectangle.Height) {
				captionRect=new Rectangle(rectangle.X+1, rectangle.Y+rectangle.Height/2-rectangle.Width/2+1, rectangle.Width-4, rectangle.Width-4);
			} else {
				captionRect=new Rectangle(rectangle.X+rectangle.Width/2-rectangle.Height/2+1, rectangle.Y+1, rectangle.Height-4, rectangle.Height-4);
			}

			if ((state & ButtonState.Pushed)!=0) {
				captionRect=new Rectangle(rectangle.X+2, rectangle.Y+2, rectangle.Width-3, rectangle.Height-3);
			}

			/* Make sure we've got at least a line width of 1 */
			lineWidth=Math.Max(1, captionRect.Width/7);

			switch(button) {
			case CaptionButton.Close: {
				Pen	pen;

				if ((state & ButtonState.Inactive)!=0) {
					pen=new Pen(ColorControlLight, lineWidth);
					DrawCaptionHelper(graphics, ColorControlLight, pen, lineWidth, 1, captionRect, button);
					pen.Dispose();

					pen=new Pen(ColorControlDark, lineWidth);
					DrawCaptionHelper(graphics, ColorControlDark, pen, lineWidth, 0, captionRect, button);
					pen.Dispose();
					return;
				} else {
					pen=new Pen(ColorControlText, lineWidth);
					DrawCaptionHelper(graphics, ColorControlText, pen, lineWidth, 0, captionRect, button);
					pen.Dispose();
					return;
				}
			}

			case CaptionButton.Help:
			case CaptionButton.Maximize:
			case CaptionButton.Minimize:
			case CaptionButton.Restore: {
				if ((state & ButtonState.Inactive)!=0) {
					DrawCaptionHelper(graphics, ColorControlLight, SystemPens.ControlLightLight, lineWidth, 1, captionRect, button);

					DrawCaptionHelper(graphics, ColorControlDark, SystemPens.ControlDark, lineWidth, 0, captionRect, button);
					return;
				} else {
					DrawCaptionHelper(graphics, ColorControlText, SystemPens.ControlText, lineWidth, 0, captionRect, button);
					return;
				}
			}
			}
		}


		public override void CPDrawCheckBox (Graphics graphics, Rectangle rectangle, ButtonState state) {
			DrawFrameControlStates	dfcs=DrawFrameControlStates.ButtonCheck;

			if ((state & ButtonState.Pushed)!=0) {
				dfcs |= DrawFrameControlStates.Pushed;
			}

			if ((state & ButtonState.Checked)!=0) {
				dfcs |= DrawFrameControlStates.Checked;
			}

			if ((state & ButtonState.Flat)!=0) {
				dfcs |= DrawFrameControlStates.Flat;
			}

			if ((state & ButtonState.Inactive)!=0) {
				dfcs |= DrawFrameControlStates.Inactive;
			}

			DrawFrameControl(graphics, rectangle, DrawFrameControlTypes.Button, dfcs);

		}

		public override void CPDrawComboButton (Graphics graphics, Rectangle rectangle, ButtonState state) {
			Point[]			arrow = new Point[3];
			Point				P1;
			Point				P2;
			Point				P3;
			int				centerX;
			int				centerY;
			int				shiftX;
			int				shiftY;
			Rectangle		rect;

			if ((state & ButtonState.Checked)!=0) {
				graphics.FillRectangle(ResPool.GetHatchBrush (HatchStyle.Percent50, ColorControlLightLight, ColorControlLight),rectangle);				
			}

			if ((state & ButtonState.Flat)!=0) {
				ControlPaint.DrawBorder(graphics, rectangle, ColorControlDark, ButtonBorderStyle.Solid);
			} else {
				if ((state & (ButtonState.Pushed | ButtonState.Checked))!=0) {
					// this needs to render like a pushed button - jba
					// CPDrawBorder3D(graphics, rectangle, Border3DStyle.Sunken, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom, ColorControl);
					Rectangle trace_rectangle = new Rectangle(rectangle.X, rectangle.Y, Math.Max (rectangle.Width-1, 0), Math.Max (rectangle.Height-1, 0));
					graphics.DrawRectangle (ResPool.GetPen (ControlPaint.Dark (ColorControl)), trace_rectangle);
				} else {
					CPDrawBorder3D(graphics, rectangle, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom, ColorControl);
				}
			}

			rect=new Rectangle(rectangle.X+rectangle.Width/4, rectangle.Y+rectangle.Height/4, rectangle.Width/2, rectangle.Height/2);
			centerX=rect.Left+rect.Width/2;
			centerY=rect.Top+rect.Height/2;
			shiftX=Math.Max(1, rect.Width/8);
			shiftY=Math.Max(1, rect.Height/8);

			if ((state & ButtonState.Pushed)!=0) {
				shiftX++;
				shiftY++;
			}

			rect.Y-=shiftY;
			centerY-=shiftY;
			P1=new Point(rect.Left, centerY);
			P2=new Point(rect.Right, centerY);
			P3=new Point(centerX, rect.Bottom);

			arrow[0]=P1;
			arrow[1]=P2;
			arrow[2]=P3;

			/* Draw the arrow */
			if ((state & ButtonState.Inactive)!=0) {
				graphics.FillPolygon(SystemBrushes.ControlLightLight, arrow, FillMode.Winding);

				/* Move away from the shadow */
				P1.X-=1;		P1.Y-=1;
				P2.X-=1;		P2.Y-=1;
				P3.X-=1;		P3.Y-=1;

				arrow[0]=P1;
				arrow[1]=P2;
				arrow[2]=P3;


				graphics.FillPolygon(SystemBrushes.ControlDark, arrow, FillMode.Winding);
			} else {
				graphics.FillPolygon(SystemBrushes.ControlText, arrow, FillMode.Winding);
			}
		}


		public override void CPDrawContainerGrabHandle (Graphics graphics, Rectangle bounds) {
			
			Pen			pen	= new Pen(Color.Black, 1);
			Rectangle	rect	= new Rectangle(bounds.X, bounds.Y, bounds.Width-1, bounds.Height-1);	// Dunno why, but MS does it that way, too
			int			X;
			int			Y;

			graphics.FillRectangle(ResPool.GetSolidBrush (ColorControlText), rect);
			graphics.DrawRectangle(pen, rect);

			X=rect.X+rect.Width/2;
			Y=rect.Y+rect.Height/2;

			/* Draw the cross */
			graphics.DrawLine(pen, X, rect.Y+2, X, rect.Bottom-2);
			graphics.DrawLine(pen, rect.X+2, Y, rect.Right-2, Y);

			/* Draw 'arrows' for vertical lines */
			graphics.DrawLine(pen, X-1, rect.Y+3, X+1, rect.Y+3);
			graphics.DrawLine(pen, X-1, rect.Bottom-3, X+1, rect.Bottom-3);

			/* Draw 'arrows' for horizontal lines */
			graphics.DrawLine(pen, rect.X+3, Y-1, rect.X+3, Y+1);
			graphics.DrawLine(pen, rect.Right-3, Y-1, rect.Right-3, Y+1);
			pen.Dispose ();

		}

		public virtual void DrawFlatStyleFocusRectangle (Graphics graphics, Rectangle rectangle, ButtonBase button, Color foreColor, Color backColor) {
			// make a rectange to trace around border of the button
			Rectangle trace_rectangle = new Rectangle(rectangle.X, rectangle.Y, Math.Max (rectangle.Width-1, 0), Math.Max (rectangle.Height-1, 0));
			
			Color outerColor = foreColor;
			// adjust focus color according to the flatstyle
			if (button.FlatStyle == FlatStyle.Popup && !button.is_pressed) {
				outerColor = (backColor == ColorControl) ? ControlPaint.Dark(ColorControl) : ColorControlText;				
			}
			
			// draw the outer rectangle
			graphics.DrawRectangle (ResPool.GetPen (outerColor), trace_rectangle);			
			
			// draw the inner rectangle						
			if (button.FlatStyle == FlatStyle.Popup) {
				DrawInnerFocusRectangle (graphics, Rectangle.Inflate (rectangle, -4, -4), backColor);
			} else {
				// draw a flat inner rectangle
				Pen pen = ResPool.GetPen (ControlPaint.LightLight (backColor));
				graphics.DrawRectangle(pen, Rectangle.Inflate (trace_rectangle, -4, -4));				
			}
		}
		
		public virtual void DrawInnerFocusRectangle(Graphics graphics, Rectangle rectangle, Color backColor)
		{	
			// make a rectange to trace around border of the button
			Rectangle trace_rectangle = new Rectangle(rectangle.X, rectangle.Y, Math.Max (rectangle.Width-1, 0), Math.Max (rectangle.Height-1, 0));
			
#if NotUntilCairoIsFixed
			Color colorBackInverted = Color.FromArgb (Math.Abs (backColor.R-255), Math.Abs (backColor.G-255), Math.Abs (backColor.B-255));
			DashStyle oldStyle; // used for caching old penstyle
			Pen pen = ResPool.GetPen (colorBackInverted);

			oldStyle = pen.DashStyle; 
			pen.DashStyle = DashStyle.Dot;

			graphics.DrawRectangle (pen, trace_rectangle);
			pen.DashStyle = oldStyle;
#else
			CPDrawFocusRectangle(graphics, trace_rectangle, Color.Wheat, backColor);
#endif
		}
				

		public override void CPDrawFocusRectangle (Graphics graphics, Rectangle rectangle, Color foreColor, Color backColor) 
		{			
			Rectangle rect = rectangle;
			Pen pen;
			HatchBrush brush;
				
			if (backColor.GetBrightness () >= 0.5) {
				foreColor = Color.Transparent;
				backColor = Color.Black;
				
			} else {
				backColor = Color.FromArgb (Math.Abs (backColor.R-255), Math.Abs (backColor.G-255), Math.Abs (backColor.B-255));
				foreColor = Color.Black;
			}
						
			brush = ResPool.GetHatchBrush (HatchStyle.Percent50, backColor, foreColor);
			pen = new Pen (brush, 1);
						
			rect.Width--;
			rect.Height--;			
			
			graphics.DrawRectangle (pen, rect);
			pen.Dispose ();
		}
		
		public override void CPDrawGrabHandle (Graphics graphics, Rectangle rectangle, bool primary, bool enabled) {
			SolidBrush	sb;
			Pen			pen;

			if (primary==true) {
				pen=new Pen(Color.Black, 1);
				if (enabled==true) {
					sb=ResPool.GetSolidBrush (ColorControlText);
				} else {
					sb=ResPool.GetSolidBrush (ColorControl);
				}
			} else {
				pen=new Pen(Color.White, 1);
				if (enabled==true) {
					sb=ResPool.GetSolidBrush (Color.Black);
				} else {
					sb=ResPool.GetSolidBrush (ColorControl);
				}
			}
			graphics.FillRectangle(sb, rectangle);
			graphics.DrawRectangle(pen, rectangle);			
			pen.Dispose();
		}


		public override void CPDrawGrid (Graphics graphics, Rectangle area, Size pixelsBetweenDots, Color backColor) {
			Color	foreColor;
			int	h;
			int	b;
			int	s;

			ControlPaint.Color2HBS(backColor, out h, out b, out s);

			if (b>127) {
				foreColor=Color.Black;
			} else {
				foreColor=Color.White;
			}

#if false
			/* Commented out until I take the time and figure out
				which HatchStyle will match requirements. The code below
				is only correct for Percent50.
			*/
			if (pixelsBetweenDots.Width==pixelsBetweenDots.Height) {
				HatchBrush	brush=null;

				switch(pixelsBetweenDots.Width) {
					case 2: brush=new HatchBrush(HatchStyle.Percent50, foreColor, backColor); break;
					case 4: brush=new HatchBrush(HatchStyle.Percent25, foreColor, backColor); break;
					case 5: brush=new HatchBrush(HatchStyle.Percent20, foreColor, backColor); break;
					default: {
						/* Have to do it the slow way */
						break;
					}
				}
				if (brush!=null) {
					graphics.FillRectangle(brush, area);
					pen.Dispose();
					brush.Dispose();
					return;
				}
			}
#endif
			/* Slow method */

			Bitmap bitmap = new Bitmap(area.Width, area.Height, graphics);

			for (int x=0; x<area.Width; x+=pixelsBetweenDots.Width) {
				for (int y=0; y<area.Height; y+=pixelsBetweenDots.Height) {
					bitmap.SetPixel(x, y, foreColor);
				}
			}
			graphics.DrawImage(bitmap, area.X, area.Y, area.Width, area.Height);
			bitmap.Dispose();
		}

		public override void CPDrawImageDisabled (Graphics graphics, Image image, int x, int y, Color background) {
			/*
				Microsoft seems to ignore the background and simply make
				the image grayscale. At least when having > 256 colors on
				the display.
			*/
			
			if (imagedisabled_attributes == null) {				
				imagedisabled_attributes = new ImageAttributes ();
				ColorMatrix colorMatrix=new ColorMatrix(new float[][] {
					  // This table would create a perfect grayscale image, based on luminance
					  //				new float[]{0.3f,0.3f,0.3f,0,0},
					  //				new float[]{0.59f,0.59f,0.59f,0,0},
					  //				new float[]{0.11f,0.11f,0.11f,0,0},
					  //				new float[]{0,0,0,1,0,0},
					  //				new float[]{0,0,0,0,1,0},
					  //				new float[]{0,0,0,0,0,1}
		
					  // This table generates a image that is grayscaled and then
					  // brightened up. Seems to match MS close enough.
					  new float[]{0.2f,0.2f,0.2f,0,0},
					  new float[]{0.41f,0.41f,0.41f,0,0},
					  new float[]{0.11f,0.11f,0.11f,0,0},
					  new float[]{0.15f,0.15f,0.15f,1,0,0},
					  new float[]{0.15f,0.15f,0.15f,0,1,0},
					  new float[]{0.15f,0.15f,0.15f,0,0,1}
				  });
				  
				 imagedisabled_attributes.SetColorMatrix (colorMatrix);
			}
			
			graphics.DrawImage(image, new Rectangle(x, y, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imagedisabled_attributes);
			
		}


		public override void CPDrawLockedFrame (Graphics graphics, Rectangle rectangle, bool primary) {
			Pen	penBorder;
			Pen	penInside;

			if (primary) {
				penBorder=new Pen(Color.White, 2);
				penInside=new Pen(Color.Black, 1);
			} else {
				penBorder=new Pen(Color.Black, 2);
				penInside=new Pen(Color.White, 1);
			}
			penBorder.Alignment=PenAlignment.Inset;
			penInside.Alignment=PenAlignment.Inset;

			graphics.DrawRectangle(penBorder, rectangle);
			graphics.DrawRectangle(penInside, rectangle.X+2, rectangle.Y+2, rectangle.Width-5, rectangle.Height-5);
			penBorder.Dispose();
			penInside.Dispose();
		}


		public override void CPDrawMenuGlyph (Graphics graphics, Rectangle rectangle, MenuGlyph glyph, Color color) {
			Rectangle	rect;
			int			lineWidth;

			Brush brush = ResPool.GetSolidBrush (color);

			switch(glyph) {
			case MenuGlyph.Arrow: {
				Point[]			arrow = new Point[3];
				Point				P1;
				Point				P2;
				Point				P3;
				int				centerX;
				int				centerY;
				int				shiftX;

				rect=new Rectangle(rectangle.X+rectangle.Width/4, rectangle.Y+rectangle.Height/4, rectangle.Width/2, rectangle.Height/2);
				centerX=rect.Left+rect.Width/2;
				centerY=rect.Top+rect.Height/2;
				shiftX=Math.Max(1, rect.Width/8);

				rect.X-=shiftX;
				centerX-=shiftX;

				P1=new Point(centerX, rect.Top-1);
				P2=new Point(centerX, rect.Bottom);
				P3=new Point(rect.Right, centerY);

				arrow[0]=P1;
				arrow[1]=P2;
				arrow[2]=P3;

				graphics.FillPolygon(brush, arrow, FillMode.Winding);

				return;
			}

			case MenuGlyph.Bullet: {
				
				lineWidth=Math.Max(2, rectangle.Width/3);
				rect=new Rectangle(rectangle.X+lineWidth, rectangle.Y+lineWidth, rectangle.Width-lineWidth*2, rectangle.Height-lineWidth*2);
				
				graphics.FillEllipse(brush, rect);
				
				return;
			}

			case MenuGlyph.Checkmark: {
				int			Scale;
				Pen pen = ResPool.GetPen (color);

				lineWidth=Math.Max(2, rectangle.Width/6);
				Scale=Math.Max(1, rectangle.Width/12);

				rect=new Rectangle(rectangle.X+lineWidth, rectangle.Y+lineWidth, rectangle.Width-lineWidth*2, rectangle.Height-lineWidth*2);

				for (int i=0; i<lineWidth; i++) {
					graphics.DrawLine(pen, rect.Left+lineWidth/2, rect.Top+lineWidth+i, rect.Left+lineWidth/2+2*Scale, rect.Top+lineWidth+2*Scale+i);
					graphics.DrawLine(pen, rect.Left+lineWidth/2+2*Scale, rect.Top+lineWidth+2*Scale+i, rect.Left+lineWidth/2+6*Scale, rect.Top+lineWidth-2*Scale+i);
				}
				return;
			}
			}

		}

		public override void CPDrawRadioButton (Graphics graphics, Rectangle rectangle, ButtonState state) {
			DrawFrameControlStates	dfcs=DrawFrameControlStates.ButtonRadio;

			if ((state & ButtonState.Pushed)!=0) {
				dfcs |= DrawFrameControlStates.Pushed;
			}

			if ((state & ButtonState.Checked)!=0) {
				dfcs |= DrawFrameControlStates.Checked;
			}

			if ((state & ButtonState.Flat)!=0) {
				dfcs |= DrawFrameControlStates.Flat;
			}

			if ((state & ButtonState.Inactive)!=0) {
				dfcs |= DrawFrameControlStates.Inactive;
			}
			DrawFrameControl(graphics, rectangle, DrawFrameControlTypes.Button, dfcs);

		}


		public override void CPDrawReversibleFrame (Rectangle rectangle, Color backColor, FrameStyle style) {

		}


		public override void CPDrawReversibleLine (Point start, Point end, Color backColor) {

		}


		/* Scroll button: regular button + direction arrow */
		public override void CPDrawScrollButton (Graphics dc, Rectangle area, ScrollButton type, ButtonState state) {
			DrawScrollButtonPrimitive (dc, area, state);

			// A lot of the following is adapted from the rewind project
			Rectangle rect = new Rectangle (area.X - 3, area.Y - 3,
					area.Width + 6, area.Height + 6);
			int small_diam = rect.Width > rect.Height ? rect.Height : rect.Width;
			if (rect.Width < rect.Height) {
				rect.Y += (rect.Height - rect.Width) / 2;
				rect.Height = small_diam;
			} else if (rect.Width > rect.Height) {
				rect.X += (rect.Width - rect.Height) / 2;
				rect.Width = small_diam;
			}

			small_diam -= 2;

			int tri = 290 * small_diam / 1000 - 1;
			if (tri == 0)
				tri = 1;

			Point [] arrow = new Point [3];
			for (int i = 0; i < 3; i++)
				arrow [i] = new Point ();

			switch(type) {
			default:
			case ScrollButton.Down:
				arrow [2].X = rect.Left + 470 * small_diam / 1000 + 2;
				arrow [2].Y = rect.Top + 687 * small_diam / 1000 + 1;
				arrow [0].X = arrow [2].X - tri;
				arrow [1].X = arrow [2].X + tri;
				arrow [0].Y = arrow [1].Y = arrow [2].Y - tri;
				break;

			case ScrollButton.Up:
				arrow [2].X = rect.Left + 470 * small_diam / 1000 + 2;
				arrow [2].Y = rect.Bottom - (687 * small_diam / 1000 + 1);
				arrow [0].X = arrow [2].X - tri;
				arrow [1].X = arrow [2].X + tri;
				arrow [0].Y = arrow [1].Y = arrow [2].Y + tri;
				break;

			case ScrollButton.Left:
				arrow [2].X = rect.Right - (687 * small_diam / 1000 + 1);
				arrow [2].Y = rect.Top + 470 * small_diam / 1000 + 2;
				arrow [0].Y = arrow [2].Y - tri;
				arrow [1].Y = arrow [2].Y + tri;
				arrow [0].X = arrow [1].X = arrow [2].X + tri;
				break;
			case ScrollButton.Right:
				arrow [2].X = rect.Left + 687 * small_diam / 1000 + 1;
				arrow [2].Y = rect.Top + 470 * small_diam / 1000 + 2;
				arrow [0].Y = arrow [2].Y - tri;
				arrow [1].Y = arrow [2].Y + tri;
				arrow [0].X = arrow [1].X = arrow [2].X - tri;
				break;
			}

			/* Draw the arrow */
			if ((state & ButtonState.Inactive)!=0) {
				dc.FillPolygon (SystemBrushes.ControlLightLight, arrow, FillMode.Winding);

				for (int i = 0; i < 3; i++) {
					arrow [i].X--;
					arrow [i].Y--;
				}
				
				dc.FillPolygon (SystemBrushes.ControlDark, arrow, FillMode.Winding);
			} else {
				dc.FillPolygon (SystemBrushes.ControlText, arrow, FillMode.Winding);
			}
		}

		public  override void CPDrawSelectionFrame (Graphics graphics, bool active, Rectangle outsideRect, Rectangle insideRect,
			Color backColor) {

		}


		public override void CPDrawSizeGrip (Graphics dc, Color backColor, Rectangle bounds) {
			
			Point pt = new Point (bounds.Right - 2, bounds.Bottom - 2);

			Pen pen = ResPool.GetPen (ColorControlDark);
			
			dc.DrawLine (pen, pt.X - 11, pt.Y, pt.X, pt.Y - 11);
			dc.DrawLine (pen, pt.X - 10, pt.Y, pt.X, pt.Y - 10);
			
			dc.DrawLine (pen, pt.X - 7, pt.Y, pt.X, pt.Y - 7);
			dc.DrawLine (pen, pt.X - 6, pt.Y, pt.X, pt.Y - 6);
			
			dc.DrawLine (pen, pt.X - 3, pt.Y, pt.X, pt.Y - 3);
			dc.DrawLine (pen, pt.X - 2, pt.Y, pt.X, pt.Y - 2);
			
			pen = ResPool.GetPen (ColorControlLight);
			
			dc.DrawLine (pen, pt.X - 12, pt.Y, pt.X, pt.Y - 12);
			dc.DrawLine (pen, pt.X - 8, pt.Y, pt.X, pt.Y - 8);
			dc.DrawLine (pen, pt.X - 4, pt.Y, pt.X, pt.Y - 4);
		}


		public  override void CPDrawStringDisabled (Graphics graphics, string s, Font font, Color color, RectangleF layoutRectangle,
			StringFormat format) {			

			graphics.DrawString(s, font, ResPool.GetSolidBrush (ColorGrayText), layoutRectangle, format);
			
		}

		private static void DrawBorderInternal(Graphics graphics, int startX, int startY, int endX, int endY,
			int width, Color color, ButtonBorderStyle style, Border3DSide side) {

			Pen	pen=new Pen(color, 1);

			switch(style) {
			case ButtonBorderStyle.Solid: {
				pen.DashStyle=DashStyle.Solid;
				break;
			}

			case ButtonBorderStyle.Dashed: {
				pen.DashStyle=DashStyle.Dash;
				break;
			}

			case ButtonBorderStyle.Dotted: {
				pen.DashStyle=DashStyle.Dot;
				break;
			}

			case ButtonBorderStyle.Inset: {
				pen.DashStyle=DashStyle.Solid;
				break;
			}

			case ButtonBorderStyle.Outset: {
				pen.DashStyle=DashStyle.Solid;
				break;
			}

			default:
			case ButtonBorderStyle.None: {
				pen.Dispose();
				return;
			}
			}


			switch(style) {
			case ButtonBorderStyle.Outset: {
				Color		colorGrade;
				int		hue, brightness, saturation;
				int		brightnessSteps;
				int		brightnessDownSteps;

				ControlPaint.Color2HBS(color, out hue, out brightness, out saturation);

				brightnessDownSteps=brightness/width;
				if (brightness>127) {
					brightnessSteps=Math.Max(6, (160-brightness)/width);
				} else {
					brightnessSteps=(127-brightness)/width;
				}

				for (int i=0; i<width; i++) {
					switch(side) {
					case Border3DSide.Left:	{
						pen.Dispose();
						colorGrade=ControlPaint.HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
						pen=new Pen(colorGrade, 1);
						graphics.DrawLine(pen, startX+i, startY+i, endX+i, endY-i);
						break;
					}

					case Border3DSide.Right: {
						pen.Dispose();
						colorGrade=ControlPaint.HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
						pen=new Pen(colorGrade, 1);
						graphics.DrawLine(pen, startX-i, startY+i, endX-i, endY-i);
						break;
					}

					case Border3DSide.Top: {
						pen.Dispose();
						colorGrade=ControlPaint.HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
						pen=new Pen(colorGrade, 1);
						graphics.DrawLine(pen, startX+i, startY+i, endX-i, endY+i);
						break;
					}

					case Border3DSide.Bottom: {
						pen.Dispose();
						colorGrade=ControlPaint.HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
						pen=new Pen(colorGrade, 1);
						graphics.DrawLine(pen, startX+i, startY-i, endX-i, endY-i);
						break;
					}
					}
				}
				break;
			}

			case ButtonBorderStyle.Inset: {
				Color		colorGrade;
				int		hue, brightness, saturation;
				int		brightnessSteps;
				int		brightnessDownSteps;

				ControlPaint.Color2HBS(color, out hue, out brightness, out saturation);

				brightnessDownSteps=brightness/width;
				if (brightness>127) {
					brightnessSteps=Math.Max(6, (160-brightness)/width);
				} else {
					brightnessSteps=(127-brightness)/width;
				}

				for (int i=0; i<width; i++) {
					switch(side) {
					case Border3DSide.Left:	{
						pen.Dispose();
						colorGrade=ControlPaint.HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
						pen=new Pen(colorGrade, 1);
						graphics.DrawLine(pen, startX+i, startY+i, endX+i, endY-i);
						break;
					}

					case Border3DSide.Right: {
						pen.Dispose();
						colorGrade=ControlPaint.HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
						pen=new Pen(colorGrade, 1);
						graphics.DrawLine(pen, startX-i, startY+i, endX-i, endY-i);
						break;
					}

					case Border3DSide.Top: {
						pen.Dispose();
						colorGrade=ControlPaint.HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
						pen=new Pen(colorGrade, 1);
						graphics.DrawLine(pen, startX+i, startY+i, endX-i, endY+i);
						break;
					}

					case Border3DSide.Bottom: {
						pen.Dispose();
						colorGrade=ControlPaint.HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
						pen=new Pen(colorGrade, 1);
						graphics.DrawLine(pen, startX+i, startY-i, endX-i, endY-i);
						break;
					}
					}
				}
				break;
			}

				/*
					I decided to have the for-loop duplicated for speed reasons;
					that way we only have to switch once (as opposed to have the
					for-loop around the switch)
				*/
			default: {
				switch(side) {
				case Border3DSide.Left:	{
					for (int i=0; i<width; i++) {
						graphics.DrawLine(pen, startX+i, startY+i, endX+i, endY-i);
					}
					break;
				}

				case Border3DSide.Right: {
					for (int i=0; i<width; i++) {
						graphics.DrawLine(pen, startX-i, startY+i, endX-i, endY-i);
					}
					break;
				}

				case Border3DSide.Top: {
					for (int i=0; i<width; i++) {
						graphics.DrawLine(pen, startX+i, startY+i, endX-i, endY+i);
					}
					break;
				}

				case Border3DSide.Bottom: {
					for (int i=0; i<width; i++) {
						graphics.DrawLine(pen, startX+i, startY-i, endX-i, endY-i);
					}
					break;
				}
				}
				break;
			}
			}
			pen.Dispose();
		}

		/*
			This function actually draws the various caption elements.
			This way we can scale them nicely, no matter what size, and they
			still look like MS's scaled caption buttons. (as opposed to scaling a bitmap)
		*/

		private void DrawCaptionHelper(Graphics graphics, Color color, Pen pen, int lineWidth, int shift, Rectangle captionRect, CaptionButton button) {
			switch(button) {
			case CaptionButton.Close: {
				pen.StartCap=LineCap.Triangle;
				pen.EndCap=LineCap.Triangle;
				if (lineWidth<2) {
					graphics.DrawLine(pen, captionRect.Left+2*lineWidth+1+shift, captionRect.Top+2*lineWidth+shift, captionRect.Right-2*lineWidth+1+shift, captionRect.Bottom-2*lineWidth+shift);
					graphics.DrawLine(pen, captionRect.Right-2*lineWidth+1+shift, captionRect.Top+2*lineWidth+shift, captionRect.Left+2*lineWidth+1+shift, captionRect.Bottom-2*lineWidth+shift);
				}

				graphics.DrawLine(pen, captionRect.Left+2*lineWidth+shift, captionRect.Top+2*lineWidth+shift, captionRect.Right-2*lineWidth+shift, captionRect.Bottom-2*lineWidth+shift);
				graphics.DrawLine(pen, captionRect.Right-2*lineWidth+shift, captionRect.Top+2*lineWidth+shift, captionRect.Left+2*lineWidth+shift, captionRect.Bottom-2*lineWidth+shift);
				return;
			}

			case CaptionButton.Help: {
				StringFormat	sf = new StringFormat();				
				Font				font = new Font("Microsoft Sans Serif", captionRect.Height, FontStyle.Bold, GraphicsUnit.Pixel);

				sf.Alignment=StringAlignment.Center;
				sf.LineAlignment=StringAlignment.Center;


				graphics.DrawString("?", font, ResPool.GetSolidBrush (color), captionRect.X+captionRect.Width/2+shift, captionRect.Y+captionRect.Height/2+shift+lineWidth/2, sf);

				sf.Dispose();				
				font.Dispose();

				return;
			}

			case CaptionButton.Maximize: {
				/* Top 'caption bar' line */
				for (int i=0; i<Math.Max(2, lineWidth); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Top+2*lineWidth+shift+i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Top+2*lineWidth+shift+i);
				}

				/* Left side line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift+i, captionRect.Top+2*lineWidth+shift, captionRect.Left+lineWidth+shift+i, captionRect.Bottom-lineWidth+shift);
				}

				/* Right side line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Right-lineWidth-lineWidth/2+shift+i, captionRect.Top+2*lineWidth+shift, captionRect.Right-lineWidth-lineWidth/2+shift+i, captionRect.Bottom-lineWidth+shift);
				}

				/* Bottom line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Bottom-lineWidth+shift-i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Bottom-lineWidth+shift-i);
				}
				return;
			}

			case CaptionButton.Minimize: {
				/* Bottom line */
				for (int i=0; i<Math.Max(2, lineWidth); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Bottom-lineWidth+shift-i, captionRect.Right-3*lineWidth+shift, captionRect.Bottom-lineWidth+shift-i);
				}
				return;
			}

			case CaptionButton.Restore: {
				/** First 'window' **/
				/* Top 'caption bar' line */
				for (int i=0; i<Math.Max(2, lineWidth); i++) {
					graphics.DrawLine(pen, captionRect.Left+3*lineWidth+shift, captionRect.Top+2*lineWidth+shift-i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Top+2*lineWidth+shift-i);
				}

				/* Left side line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Left+3*lineWidth+shift+i, captionRect.Top+2*lineWidth+shift, captionRect.Left+3*lineWidth+shift+i, captionRect.Top+4*lineWidth+shift);
				}

				/* Right side line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Right-lineWidth-lineWidth/2+shift-i, captionRect.Top+2*lineWidth+shift, captionRect.Right-lineWidth-lineWidth/2+shift-i, captionRect.Top+5*lineWidth-lineWidth/2+shift);
				}

				/* Bottom line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Right-3*lineWidth-lineWidth/2+shift, captionRect.Top+5*lineWidth-lineWidth/2+shift+1+i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Top+5*lineWidth-lineWidth/2+shift+1+i);
				}

				/** Second 'window' **/
				/* Top 'caption bar' line */
				for (int i=0; i<Math.Max(2, lineWidth); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Top+4*lineWidth+shift+1-i, captionRect.Right-3*lineWidth-lineWidth/2+shift, captionRect.Top+4*lineWidth+shift+1-i);
				}

				/* Left side line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift+i, captionRect.Top+4*lineWidth+shift+1, captionRect.Left+lineWidth+shift+i, captionRect.Bottom-lineWidth+shift);
				}

				/* Right side line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Right-3*lineWidth-lineWidth/2+shift-i, captionRect.Top+4*lineWidth+shift+1, captionRect.Right-3*lineWidth-lineWidth/2+shift-i, captionRect.Bottom-lineWidth+shift);
				}

				/* Bottom line */
				for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
					graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Bottom-lineWidth+shift-i, captionRect.Right-3*lineWidth-lineWidth/2+shift, captionRect.Bottom-lineWidth+shift-i);
				}

				return;
			}

			}
		}

		[MonoTODO("Finish drawing code for Caption, Menu and Scroll")]
		private void DrawFrameControl(Graphics graphics, Rectangle rectangle, DrawFrameControlTypes Type, DrawFrameControlStates State) {
			// make a rectange to trace around border of the button
			Rectangle trace_rectangle = new Rectangle(rectangle.X, rectangle.Y, Math.Max (rectangle.Width-1, 0), Math.Max (rectangle.Height-1, 0));
			switch(Type) {
			case DrawFrameControlTypes.Button: {

				if ((State & DrawFrameControlStates.ButtonPush)!=0) {
// JBA 31 oct 2004 - I don't think that button style should be rendered like this
//					/* Goes first, affects the background */
//					if ((State & DrawFrameControlStates.Checked)!=0) {
//						HatchBrush	hatchBrush=new HatchBrush(HatchStyle.Percent50, ColorControlLightLight, ColorControlLight);
//						graphics.FillRectangle(hatchBrush,rectangle);
//						hatchBrush.Dispose();
//					}

					if ((State & DrawFrameControlStates.Pushed)!=0 || (State & DrawFrameControlStates.Checked)!=0) {
						graphics.DrawRectangle (ResPool.GetPen (ControlPaint.Dark (ColorControl)), trace_rectangle);
					} else if ((State & DrawFrameControlStates.Flat)!=0) {
						ControlPaint.DrawBorder(graphics, rectangle, ColorControlDark, ButtonBorderStyle.Solid);
					} else if ((State & DrawFrameControlStates.Inactive)!=0) {
						/* Same as normal, it would seem */
						CPDrawBorder3D(graphics, rectangle, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom, ColorControl);
					} else {
						CPDrawBorder3D(graphics, rectangle, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom, ColorControl);
					}
				} else if ((State & DrawFrameControlStates.ButtonRadio)!=0) {
					Pen			penFatDark	= new Pen(ColorControlDark, 1);
					Pen			penFatLight	= new Pen(ColorControlLightLight, 1);
					int			lineWidth;

					graphics.FillPie (ResPool.GetSolidBrush (this.ColorWindow), rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 2, rectangle.Height - 2, 0, 359);

					graphics.DrawArc(penFatDark, rectangle.X+1, rectangle.Y+1, rectangle.Width-2, rectangle.Height-2, 135, 180);
					graphics.DrawArc(penFatLight, rectangle.X+1, rectangle.Y+1, rectangle.Width-2, rectangle.Height-2, 315, 180);

					graphics.DrawArc(SystemPens.ControlDark, rectangle, 135, 180);
					graphics.DrawArc(SystemPens.ControlLightLight, rectangle, 315, 180);

					lineWidth=Math.Max(1, Math.Min(rectangle.Width, rectangle.Height)/3);

					if ((State & DrawFrameControlStates.Checked)!=0) {
						SolidBrush	buttonBrush;

						if ((State & DrawFrameControlStates.Inactive)!=0) {
							buttonBrush=(SolidBrush)SystemBrushes.ControlDark;
						} else {
							buttonBrush=(SolidBrush)SystemBrushes.ControlText;
						}
						graphics.FillPie(buttonBrush, rectangle.X+lineWidth, rectangle.Y+lineWidth, rectangle.Width-lineWidth*2, rectangle.Height-lineWidth*2, 0, 359);
					}
					penFatDark.Dispose();
					penFatLight.Dispose();
				} else if ((State & DrawFrameControlStates.ButtonRadioImage)!=0) {
					throw new NotImplementedException () ;
				} else if ((State & DrawFrameControlStates.ButtonRadioMask)!=0) {
					throw new NotImplementedException ();
				} else {	/* Must be Checkbox */
					Pen			pen;
					int			lineWidth;
					Rectangle	rect;
					int			Scale;

					/* Goes first, affects the background */
					if ((State & DrawFrameControlStates.Pushed)!=0 ||
						(State & DrawFrameControlStates.Inactive)!=0) {
						graphics.FillRectangle(SystemBrushes.Control, rectangle);
					} else {
						graphics.FillRectangle(SystemBrushes.Window, rectangle);
					}

					/* Draw the sunken frame */
					if ((State & DrawFrameControlStates.Flat)!=0) {
						ControlPaint.DrawBorder(graphics, rectangle, ColorControlDark, ButtonBorderStyle.Solid);
					} else {
						CPDrawBorder3D(graphics, rectangle, Border3DStyle.Sunken, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom, ColorControl);
					}

					/* Make sure we've got at least a line width of 1 */
					lineWidth=Math.Max(3, rectangle.Width/6);
					Scale=Math.Max(1, rectangle.Width/12);

					// define a rectangle inside the border area
					rect=new Rectangle(rectangle.X+2, rectangle.Y+2, rectangle.Width-4, rectangle.Height-4);
					if ((State & DrawFrameControlStates.Inactive)!=0) {
						pen=SystemPens.ControlDark;
					} else {
						pen=SystemPens.ControlText;
					}

					if ((State & DrawFrameControlStates.Checked)!=0) {
						/* Need to draw a check-mark */
						for (int i=0; i<lineWidth; i++) {
							graphics.DrawLine(pen, rect.Left+lineWidth/2, rect.Top+lineWidth+i, rect.Left+lineWidth/2+2*Scale, rect.Top+lineWidth+2*Scale+i);
							graphics.DrawLine(pen, rect.Left+lineWidth/2+2*Scale, rect.Top+lineWidth+2*Scale+i, rect.Left+lineWidth/2+6*Scale, rect.Top+lineWidth-2*Scale+i);
						}

					}
				}
				return;
			}

			case DrawFrameControlTypes.Caption: {
				// FIXME:
				break;
			}

			case DrawFrameControlTypes.Menu: {
				// FIXME:
				break;
			}

			case DrawFrameControlTypes.Scroll: {
				// FIXME:
				break;
			}
			}
		}

		/* Generic scroll button */
		public void DrawScrollButtonPrimitive (Graphics dc, Rectangle area, ButtonState state) {
			if ((state & ButtonState.Pushed) == ButtonState.Pushed) {
				dc.FillRectangle (ResPool.GetSolidBrush (ColorControl), area.X + 1,
					area.Y + 1, area.Width - 2 , area.Height - 2);

				dc.DrawRectangle (ResPool.GetPen (ColorControlDark), area.X,
					area.Y, area.Width, area.Height);

				return;
			}			
	
			dc.FillRectangle (ResPool.GetSolidBrush (ColorControl), area.X, area.Y, area.Width, 1);
			dc.FillRectangle (ResPool.GetSolidBrush (ColorControl), area.X, area.Y, 1, area.Height);

			dc.FillRectangle (ResPool.GetSolidBrush (ColorControlLight), area.X + 1, area.Y + 1, area.Width - 1, 1);
			dc.FillRectangle (ResPool.GetSolidBrush (ColorControlLight), area.X + 1, area.Y + 2, 1,
				area.Height - 4);

			dc.FillRectangle (ResPool.GetSolidBrush (ColorControlDark), area.X + 1, area.Y + area.Height - 2,
				area.Width - 2, 1);

			dc.FillRectangle (ResPool.GetSolidBrush (ColorControlDarkDark), area.X, area.Y + area.Height -1,
				area.Width , 1);

			dc.FillRectangle (ResPool.GetSolidBrush (ColorControlDark), area.X + area.Width - 2,
				area.Y + 1, 1, area.Height -3);

			dc.FillRectangle (ResPool.GetSolidBrush (ColorControlDarkDark), area.X + area.Width -1,
				area.Y, 1, area.Height - 1);

			dc.FillRectangle (ResPool.GetSolidBrush (ColorControl), area.X + 2,
				area.Y + 2, area.Width - 4, area.Height - 4);
			
		}
		
		public override void CPDrawBorderStyle (Graphics dc, Rectangle area, BorderStyle border_style) {
			switch (border_style){
			case BorderStyle.Fixed3D:
				dc.DrawLine (ResPool.GetPen (ColorControlDark), area.X, area.Y, area.X +area.Width, area.Y);
				dc.DrawLine (ResPool.GetPen (ColorControlDark), area.X, area.Y, area.X, area.Y + area.Height);
				dc.DrawLine (ResPool.GetPen (ColorControlLight), area.X , area.Y + area.Height - 1, area.X + area.Width , 
					area.Y + area.Height - 1);
				dc.DrawLine (ResPool.GetPen (ColorControlLight), area.X + area.Width -1 , area.Y, area.X + area.Width -1, 
					area.Y + area.Height);

				dc.DrawLine (ResPool.GetPen (ColorActiveBorder), area.X + 1, area.Bottom - 2, area.Right - 2, area.Bottom - 2);
				dc.DrawLine (ResPool.GetPen (ColorActiveBorder), area.Right - 2, area.Top + 1, area.Right - 2, area.Bottom - 2);
				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), area.X + 1, area.Top + 1, area.X + 1, area.Bottom - 3);
				dc.DrawLine (ResPool.GetPen (ColorControlDarkDark), area.X + 1, area.Top + 1, area.Right - 3, area.Top + 1);
				break;
			case BorderStyle.FixedSingle:
				dc.DrawRectangle (ResPool.GetPen (ColorWindowFrame), area.X, area.Y, area.Width - 1, area.Height - 1);
				break;
			case BorderStyle.None:
			default:
				break;
			}
			
		}
		#endregion	// ControlPaint


	} //class
}
