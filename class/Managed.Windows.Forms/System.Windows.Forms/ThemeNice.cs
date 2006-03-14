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
//	Alexander Olk, xenomorph2@onlinehome.de
//
//	based on ThemeWin32Classic
//
//		- You can activate this Theme with export MONO_THEME=nice


using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace System.Windows.Forms
{
	internal class ThemeNice : ThemeWin32Classic
	{
		public override Version Version
		{
			get {
				return new Version( 0, 0, 0, 2 );
			}
		}
		
		static readonly Color NormalColor = Color.LightGray;
		static readonly Color MouseOverColor = Color.DarkGray;
		static readonly Color PressedColor = Color.Gray;
		static readonly Color FocusColor = Color.FromArgb( System.Convert.ToInt32( "0xff00c0ff", 16 ) );
		static readonly Color LightColor = Color.LightGray;
		static readonly Color BorderColor = MouseOverColor;
		static readonly Color NiceBackColor  = Color.FromArgb( System.Convert.ToInt32( "0xffefebe7", 16 ) );
		
		static Bitmap size_grip_bmp = CreateSizegripDot();
		
		#region	Principal Theme Methods
		public ThemeNice( )
		{
			ColorControl = NiceBackColor;
			always_draw_hotkeys = true;
		}
		
		public override Color DefaultControlBackColor
		{
			get { return NiceBackColor; }
		}
		
		public override Color DefaultWindowBackColor
		{
			get { return NiceBackColor; }			
		}
		
		public override Color ColorControl {
			get { return NiceBackColor;}
		}
		
		static Bitmap CreateSizegripDot()
		{
			Bitmap bmp = new Bitmap( 4, 4 );
			using ( Graphics dc = Graphics.FromImage( bmp ) )
			{
				SmoothingMode old_smoothing_mode = dc.SmoothingMode;
				dc.SmoothingMode = SmoothingMode.AntiAlias;
				
				using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( 0, 0 ), new Point( 4, 4 ), PressedColor, Color.White ) )
						dc.FillEllipse( lgbr, new Rectangle( 0, 0, 4, 4 ) );
				
				dc.SmoothingMode = old_smoothing_mode;
			}
			
			return bmp;
		}
		#endregion	// Internal Methods
		
		#region ButtonBase
		protected override void ButtonBase_DrawButton( ButtonBase button, Graphics dc )
		{
			Rectangle buttonRectangle;
			
			int width = button.ClientSize.Width;
			int height = button.ClientSize.Height;
			
			dc.FillRectangle( ResPool.GetSolidBrush( button.BackColor ), button.ClientRectangle );
			
			// set up the button rectangle
			buttonRectangle = button.ClientRectangle;
			
			Color use_color;
			Color first_color = button.has_focus ? Color.LightYellow : Color.White;
			
			if ( ( ( button is CheckBox ) && ( ( (CheckBox)button ).check_state == CheckState.Checked ) ) ||
			    ( ( button is RadioButton ) && ( ( (RadioButton)button ).check_state == CheckState.Checked ) ) )
			{
				use_color = PressedColor;
			}
			else
			if ( !button.is_enabled )
			{
				use_color = NormalColor;
				button.is_entered = false;
			}
			else
			if ( !button.is_entered )
			{
				use_color = NormalColor;
			}
			else
			{
				if ( !button.is_pressed )
					use_color = MouseOverColor;
				else
					use_color = PressedColor;
			}
			
			// Fill button with a nice linear gradient brush
			Rectangle lgbRectangle = Rectangle.Inflate( buttonRectangle, -1, -1 );
			
			if ( button.flat_style != FlatStyle.Popup || ( ( button.flat_style == FlatStyle.Popup ) && button.is_entered ) )
			{
				LinearGradientBrush lgbr;
				if ( button.flat_style == FlatStyle.Flat )
					lgbr = new LinearGradientBrush( new Point( 0, 0 ), new Point( 0, height - 1 ), use_color, first_color );
				else
					lgbr = new LinearGradientBrush( new Point( 0, 0 ), new Point( 0, height - 1 ), first_color, use_color );
				dc.FillRectangle( lgbr, lgbRectangle );
				lgbr.Dispose( );
				
				if ( button.has_focus )
					return; 
				
				Point[] points = new Point[] {
					new Point( 2, 0 ),
					new Point( width - 3, 0 ),
					new Point( width - 1, 2 ),
					new Point( width - 1, height - 3 ),
					new Point( width - 3, height - 1 ),
					new Point( 2, height - 1 ),
					new Point( 0, height - 3 ),
					new Point( 0, 2 ),
					new Point( 2, 0 )
				};
			
				Pen pen = ResPool.GetPen( BorderColor );
				dc.DrawLines( pen, points );
			}
		}
		
		protected override void ButtonBase_DrawFocus( ButtonBase button, Graphics dc )
		{
			int width = button.ClientSize.Width;
			int height = button.ClientSize.Height;
			
			Point[] points = new Point[] {
				new Point( 2, 0 ),
				new Point( width - 3, 0 ),
				new Point( width - 1, 2 ),
				new Point( width - 1, height - 3 ),
				new Point( width - 3, height - 1 ),
				new Point( 2, height - 1 ),
				new Point( 0, height - 3 ),
				new Point( 0, 2 ),
				new Point( 2, 0 )
			};
			
			Pen pen = ResPool.GetPen( FocusColor );
			dc.DrawLines( pen, points );
		}
		
		protected override void ButtonBase_DrawText( ButtonBase button, Graphics dc )
		{
			if ( !( button is CheckBox ) && !(button is RadioButton ) )
			{
				base.ButtonBase_DrawText( button, dc );
			}
		}
		#endregion	// ButtonBase
		
		#region CheckBox
		protected override void CheckBox_DrawCheckBox( Graphics dc, CheckBox checkbox, ButtonState state, Rectangle checkbox_rectangle )
		{
			dc.FillRectangle( ResPool.GetSolidBrush( checkbox.BackColor ), checkbox.ClientRectangle );
			// render as per normal button
			if ( checkbox.appearance == Appearance.Button )
			{
				DrawButtonBase( dc, checkbox.ClientRectangle, checkbox );
			}
			else
			{
				// establish if we are rendering a flat style of some sort
				if ( checkbox.FlatStyle == FlatStyle.Flat || checkbox.FlatStyle == FlatStyle.Popup )
				{
					DrawFlatStyleCheckBox( dc, checkbox_rectangle, checkbox );
				}
				else
				{
					ControlPaint.DrawCheckBox( dc, checkbox_rectangle, state );
				}
			}
		}
		#endregion	// CheckBox
		
		#region ComboBox
		
		// Drawing
		
		public override void DrawComboBoxEditDecorations( Graphics dc, ComboBox ctrl, Rectangle cl )
		{
			if ( !ctrl.Focused )
			{
				dc.DrawLine (ResPool.GetPen (BorderColor), cl.X, cl.Y, cl.X + cl.Width, cl.Y); //top 
				dc.DrawLine (ResPool.GetPen (ColorControl), cl.X + 1, cl.Y + 1, cl.X + cl.Width - 2, cl.Y + 1);
				dc.DrawLine (ResPool.GetPen (BorderColor), cl.X, cl.Y, cl.X, cl.Y + cl.Height); //left
				dc.DrawLine (ResPool.GetPen (ColorControl), cl.X + 1, cl.Y + 1, cl.X + 1, cl.Y + cl.Height - 2); 
				dc.DrawLine (ResPool.GetPen (BorderColor), cl.X + cl.Width - 1, cl.Y, cl.X + cl.Width - 1, cl.Y + cl.Height); //right
				dc.DrawLine (ResPool.GetPen (ColorControl), cl.X + cl.Width - 2, cl.Y + 1 , cl.X + cl.Width - 2, cl.Y + cl.Height - 1);
				dc.DrawLine (ResPool.GetPen (BorderColor), cl.X, cl.Y + cl.Height - 2, cl.X + cl.Width, cl.Y + cl.Height - 2); //down
				dc.DrawLine (ResPool.GetPen (ColorControl), cl.X, cl.Y + cl.Height - 1, cl.X + cl.Width, cl.Y + cl.Height - 1);
			}
			else
			{
				dc.DrawLine (ResPool.GetPen (FocusColor), cl.X, cl.Y, cl.X + cl.Width, cl.Y); //top 
				dc.DrawLine (ResPool.GetPen (ColorControl), cl.X + 1, cl.Y + 1, cl.X + cl.Width - 2, cl.Y + 1);
				dc.DrawLine (ResPool.GetPen (FocusColor), cl.X, cl.Y, cl.X, cl.Y + cl.Height); //left
				dc.DrawLine (ResPool.GetPen (ColorControl), cl.X + 1, cl.Y + 1, cl.X + 1, cl.Y + cl.Height - 2); 
				dc.DrawLine (ResPool.GetPen (FocusColor), cl.X + cl.Width - 1, cl.Y, cl.X + cl.Width - 1, cl.Y + cl.Height); //right
				dc.DrawLine (ResPool.GetPen (ColorControl), cl.X + cl.Width - 2, cl.Y + 1 , cl.X + cl.Width - 2, cl.Y + cl.Height - 1);
				dc.DrawLine (ResPool.GetPen (FocusColor), cl.X, cl.Y + cl.Height - 2, cl.X + cl.Width, cl.Y + cl.Height - 2); //down
				dc.DrawLine (ResPool.GetPen (FocusColor), cl.X, cl.Y + cl.Height - 1, cl.X + cl.Width, cl.Y + cl.Height - 1);
			}
		}
		
		public override void DrawComboListBoxDecorations( Graphics dc, ComboBox ctrl, Rectangle cl )
		{
			if ( ctrl.DropDownStyle == ComboBoxStyle.Simple )
			{
				DrawComboBoxEditDecorations( dc, ctrl, cl );
			}
			else
			{
				dc.DrawRectangle( ResPool.GetPen( ColorWindowFrame ), cl.X, cl.Y, cl.Width - 1, cl.Height - 1 );
			}
		}		
		#endregion ComboBox
		
		#region Menus
		public override void DrawMenuItem( MenuItem item, DrawItemEventArgs e )
		{
			StringFormat string_format;
			Rectangle rect_text = e.Bounds;
			
			if ( item.Visible == false )
				return;
			
			if ( item.MenuBar )
			{
				string_format = string_format_menu_menubar_text;
			}
			else
			{
				string_format = string_format_menu_text;
			}
			
			if ( item.Separator )
			{
				e.Graphics.DrawLine( ResPool.GetPen( BorderColor ),
						    e.Bounds.X, e.Bounds.Y, e.Bounds.X + e.Bounds.Width, e.Bounds.Y );
				
				e.Graphics.DrawLine( ResPool.GetPen( Color.White ),
						    e.Bounds.X + 1, e.Bounds.Y + 1, e.Bounds.X + e.Bounds.Width, e.Bounds.Y + 1 );
				
				return;
			}
			
			if ( !item.MenuBar )
				rect_text.X += MenuCheckSize.Width;
			
			if ( item.BarBreak )
			{ /* Draw vertical break bar*/
				Rectangle rect = e.Bounds;
				rect.Y++;
	        		rect.Width = 3;
	        		rect.Height = item.MenuHeight - 6;
				
				e.Graphics.DrawLine( ResPool.GetPen( LightColor ),
						    rect.X, rect.Y , rect.X, rect.Y + rect.Height );
				
				e.Graphics.DrawLine( ResPool.GetPen( ColorControlLight ),
						    rect.X + 1, rect.Y , rect.X + 1, rect.Y + rect.Height );
			}
			
			Color color_text = ColorMenuText;
			Color color_back;
			
			/* Draw background */
			Rectangle rect_back = e.Bounds;
			rect_back.X++;
			rect_back.Width -= 2;
			
			if ( ( e.State & DrawItemState.Selected ) == DrawItemState.Selected )
			{
				color_text = ColorMenuText;
				color_back = NormalColor;
				
				using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( rect_back.X, rect_back.Y ), new Point( rect_back.Right, rect_back.Y ), Color.White, NormalColor ) )//NormalColor, Color.White ) )
				{
					e.Graphics.FillRectangle( lgbr, rect_back );
				}
				
				rect_back.Height--;
				e.Graphics.DrawRectangle( ResPool.GetPen( BorderColor ), rect_back );
			}
			else
			{
				color_text = ColorMenuText;
				color_back = NiceBackColor;
				
				e.Graphics.FillRectangle( ResPool.GetSolidBrush( NiceBackColor ), rect_back );
			}
			
			if ( item.Enabled )
			{
				e.Graphics.DrawString( item.Text, e.Font,
						      ResPool.GetSolidBrush( color_text ),
						      rect_text, string_format );
				
				if ( !item.MenuBar && item.Shortcut != Shortcut.None && item.ShowShortcut )
				{
					string str = item.GetShortCutText( );
					Rectangle rect = rect_text;
					rect.X = item.XTab;
					rect.Width -= item.XTab;
					
					e.Graphics.DrawString( str, e.Font, ResPool.GetSolidBrush( color_text ),
							      rect, string_format_menu_shortcut );
				}
			}
			else
			{
				ControlPaint.DrawStringDisabled( e.Graphics, item.Text, e.Font,
								Color.Black, rect_text, string_format );
			}
			
			/* Draw arrow */
			if ( item.MenuBar == false && item.IsPopup )
			{
				int cx = MenuCheckSize.Width;
				int cy = MenuCheckSize.Height;
				using ( Bitmap	bmp = new Bitmap( cx, cy ) )
				{
					using ( Graphics dc = Graphics.FromImage( bmp ) )
					{
						SmoothingMode old_smoothing_mode = dc.SmoothingMode;
						dc.SmoothingMode = SmoothingMode.AntiAlias;
						
						Rectangle rect_arrow = new Rectangle( 0, 0, cx, cy );
						ControlPaint.DrawMenuGlyph( dc, rect_arrow, MenuGlyph.Arrow );
						bmp.MakeTransparent( );
						
						if ( item.Enabled )
						{
							e.Graphics.DrawImage( bmp, e.Bounds.X + e.Bounds.Width - cx,
									     e.Bounds.Y + ( ( e.Bounds.Height - cy ) / 2 ) );
						}
						else
						{
							ControlPaint.DrawImageDisabled( e.Graphics, bmp, e.Bounds.X + e.Bounds.Width - cx,
										       e.Bounds.Y + ( ( e.Bounds.Height - cy ) / 2 ),  color_back );
						}
						
						dc.SmoothingMode = old_smoothing_mode;
					}
				}
			}
			
			/* Draw checked or radio */
			if ( item.MenuBar == false && item.Checked )
			{
				
				Rectangle area = e.Bounds;
				int cx = MenuCheckSize.Width;
				int cy = MenuCheckSize.Height;
				using ( Bitmap bmp = new Bitmap( cx, cy ) )
				{
					using ( Graphics gr = Graphics.FromImage( bmp ) )
					{
						Rectangle rect_arrow = new Rectangle( 0, 0, cx, cy );
						
						if ( item.RadioCheck )
							ControlPaint.DrawMenuGlyph( gr, rect_arrow, MenuGlyph.Bullet );
						else
							ControlPaint.DrawMenuGlyph( gr, rect_arrow, MenuGlyph.Checkmark );
						
						bmp.MakeTransparent( );
						e.Graphics.DrawImage( bmp, area.X, e.Bounds.Y + ( ( e.Bounds.Height - cy ) / 2 ) );
					}
				}
			}
		}
		
		public override void DrawPopupMenu (Graphics dc, Menu menu, Rectangle cliparea, Rectangle rect)
		{
			
			dc.FillRectangle (ResPool.GetSolidBrush
					  (NiceBackColor), cliparea);
			
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
		
		#region ProgressBar
		public override void DrawProgressBar( Graphics dc, Rectangle clip_rect, ProgressBar ctrl )
		{
			Rectangle	client_area = ctrl.client_area;
			int		barpos_pixels;
			Rectangle bar = ctrl.client_area;
			
			barpos_pixels = ( ( ctrl.Value - ctrl.Minimum ) * client_area.Width ) / ( ctrl.Maximum - ctrl.Minimum );
			
			bar.Width = barpos_pixels;
//			bar.Height += 1;
			
			// Draw bar background
			using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( client_area.Left, client_area.Top ), new Point( client_area.Left, client_area.Bottom ), LightColor, Color.White ) )
			{
				dc.FillRectangle( lgbr, client_area );
			}
			
			// Draw bar
			using ( LinearGradientBrush lgbr = new LinearGradientBrush( bar.Location, new Point( bar.X, bar.Bottom ), Color.White, FocusColor ) )
			{
				dc.FillRectangle( lgbr, bar );
			}
			
			/* Draw border */
			dc.DrawRectangle( ResPool.GetPen( BorderColor ), ctrl.ClientRectangle.X, ctrl.ClientRectangle.Y, ctrl.ClientRectangle.Width - 1, ctrl.ClientRectangle.Height - 1 );
			dc.DrawRectangle( ResPool.GetPen( LightColor ), ctrl.ClientRectangle.X + 1, ctrl.ClientRectangle.Y + 1, ctrl.ClientRectangle.Width - 2, ctrl.ClientRectangle.Height - 2 );
		}
		#endregion	// ProgressBar
		
		#region RadioButton
		protected override void RadioButton_DrawButton( RadioButton radio_button, Graphics dc, ButtonState state, Rectangle radiobutton_rectangle )
		{
			SolidBrush sb = new SolidBrush( radio_button.BackColor );
			dc.FillRectangle( sb, radio_button.ClientRectangle );
			sb.Dispose( );
			
			if (radio_button.appearance==Appearance.Button) {
				if (radio_button.FlatStyle == FlatStyle.Flat || radio_button.FlatStyle == FlatStyle.Popup) {
					DrawFlatStyleButton(dc, radio_button.ClientRectangle, radio_button);
				} else {
					DrawButtonBase( dc, radio_button.ClientRectangle, radio_button );
				}
			} else {
				// establish if we are rendering a flat style of some sort
				if ( radio_button.FlatStyle == FlatStyle.Flat || radio_button.FlatStyle == FlatStyle.Popup ) {
					DrawFlatStyleRadioButton (dc, radiobutton_rectangle, radio_button);
				} else {
					ControlPaint.DrawRadioButton( dc, radiobutton_rectangle, state );
				}
			}
		}
		
		protected override void RadioButton_DrawFocus(RadioButton radio_button, Graphics dc, Rectangle text_rectangle)
		{
			if (radio_button.Focused && radio_button.appearance != Appearance.Button) 
			{
				if (radio_button.FlatStyle != FlatStyle.Flat && radio_button.FlatStyle != FlatStyle.Popup) 
				{
					DrawInnerFocusRectangle (dc, text_rectangle, radio_button.BackColor);
				} 
			}
		}
		#endregion	// RadioButton
		
		#region ScrollBar
		protected override void ScrollBar_DrawThumb( ScrollBar bar, Rectangle thumb_pos, Rectangle clip, Graphics dc )
		{
			if ( bar.Enabled && thumb_pos.Width > 0 && thumb_pos.Height > 0 && clip.IntersectsWith( thumb_pos ) )
				DrawScrollBarThumb( dc, thumb_pos, bar );
		}
		
		protected override void ScrollBar_Vertical_Draw_ThumbMoving_None( int scrollbutton_height, ScrollBar bar, Rectangle clip, Graphics dc )
		{
			Rectangle r = new Rectangle( 0,
						    scrollbutton_height, bar.ClientRectangle.Width, bar.ClientRectangle.Height - ( scrollbutton_height * 2 ) );
			Rectangle intersect = Rectangle.Intersect( clip, r );
			
			if ( intersect != Rectangle.Empty  )
			{
				using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( 0, 0 ), new Point( r. Width - 1, 0 ), LightColor, Color.White ) )
				{
					dc.FillRectangle( lgbr, intersect );
				}
			}
		}
		
		protected override void ScrollBar_Vertical_Draw_ThumbMoving_Forward( int scrollbutton_height, ScrollBar bar, Rectangle thumb_pos, Rectangle clip, Graphics dc )
		{
			Rectangle r = new Rectangle( 0,	 scrollbutton_height,
						    bar.ClientRectangle.Width, thumb_pos.Y  - scrollbutton_height );
			Rectangle intersect = Rectangle.Intersect( clip, r );
			
			using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( 0, 0 ), new Point( r. Width - 1, 0 ), LightColor, Color.White ) )
			{
				if ( intersect != Rectangle.Empty )
					dc.FillRectangle( lgbr, intersect );
				
				r.X = 0;
				r.Y = thumb_pos.Y + thumb_pos.Height;
				r.Width = bar.ClientRectangle.Width;
				r.Height = bar.ClientRectangle.Height -	 ( thumb_pos.Y + thumb_pos.Height ) - scrollbutton_height;
				
				intersect = Rectangle.Intersect( clip, r );
				if ( intersect != Rectangle.Empty )
					dc.FillRectangle( lgbr, intersect );
			}
		}
		
		protected override void ScrollBar_Vertical_Draw_ThumbMoving_Backwards( int scrollbutton_height, ScrollBar bar, Rectangle thumb_pos, Rectangle clip, Graphics dc )
		{
			Rectangle r = new Rectangle( 0,	 scrollbutton_height,
						    bar.ClientRectangle.Width, thumb_pos.Y - scrollbutton_height );
			Rectangle intersect = Rectangle.Intersect( clip, r );
			
			using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( 0, 0 ), new Point( r. Width - 1, 0 ), LightColor, Color.White ) )
			{
				if ( intersect != Rectangle.Empty )
					dc.FillRectangle( lgbr, intersect );
				
				r.X = 0;
				r.Y = thumb_pos.Y + thumb_pos.Height;
				r.Width = bar.ClientRectangle.Width;
				r.Height = bar.ClientRectangle.Height -	 ( thumb_pos.Y + thumb_pos.Height ) - scrollbutton_height;
				
				intersect = Rectangle.Intersect( clip, r );
				if ( intersect != Rectangle.Empty )
					dc.FillRectangle( lgbr, intersect );
			}
		}
		
		protected override void ScrollBar_Horizontal_Draw_ThumbMoving_None( int scrollbutton_width, ScrollBar bar, Rectangle clip, Graphics dc )
		{
			Rectangle r = new Rectangle( scrollbutton_width,
						    0, bar.ClientRectangle.Width - ( scrollbutton_width * 2 ), bar.ClientRectangle.Height );
			Rectangle intersect = Rectangle.Intersect( clip, r );
			
			if ( intersect != Rectangle.Empty )
			{
				using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( 0, 0 ), new Point( 0, r.Height - 1 ), LightColor, Color.White ) )
				{
					dc.FillRectangle( lgbr, intersect );
				}
			}
		}
		
		protected override void ScrollBar_Horizontal_Draw_ThumbMoving_Forward( int scrollbutton_width, Rectangle thumb_pos, ScrollBar bar, Rectangle clip, Graphics dc )
		{
			Rectangle r = new Rectangle( scrollbutton_width,  0,
						    thumb_pos.X - scrollbutton_width, bar.ClientRectangle.Height );
			Rectangle intersect = Rectangle.Intersect( clip, r );
			
			using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( 0, 0 ), new Point( 0, r.Height - 1 ), LightColor, Color.White ) )
			{
				if ( intersect != Rectangle.Empty )
					dc.FillRectangle( lgbr, intersect );
				
				r.X = thumb_pos.X + thumb_pos.Width;
				r.Y = 0;
				r.Width = bar.ClientRectangle.Width -  ( thumb_pos.X + thumb_pos.Width ) - scrollbutton_width;
				r.Height = bar.ClientRectangle.Height;
				
				intersect = Rectangle.Intersect( clip, r );
				if ( intersect != Rectangle.Empty )
					dc.FillRectangle( lgbr, intersect );
			}
		}
		
		protected override void ScrollBar_Horizontal_Draw_ThumbMoving_Backwards( int scrollbutton_width, Rectangle thumb_pos, ScrollBar bar, Rectangle clip, Graphics dc )
		{
			Rectangle r = new Rectangle( scrollbutton_width,  0,
						    thumb_pos.X - scrollbutton_width, bar.ClientRectangle.Height );
			Rectangle intersect = Rectangle.Intersect( clip, r );
			
			using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( 0, 0 ), new Point( 0, r.Height - 1 ), LightColor, Color.White ) )
			{
				if ( intersect != Rectangle.Empty )
					dc.FillRectangle( lgbr, intersect );
				
				r.X = thumb_pos.X + thumb_pos.Width;
				r.Y = 0;
				r.Width = bar.ClientRectangle.Width -  ( thumb_pos.X + thumb_pos.Width ) - scrollbutton_width;
				r.Height = bar.ClientRectangle.Height;
				
				intersect = Rectangle.Intersect( clip, r );
				if ( intersect != Rectangle.Empty )
					dc.FillRectangle( lgbr, intersect );
			}
		}
		#endregion	// ScrollBar
		
		#region StatusBar
		protected override void DrawStatusBarPanel( Graphics dc, Rectangle area, int index,
							   SolidBrush br_forecolor, StatusBarPanel panel )
		{
			int border_size = 3; // this is actually const, even if the border style is none
			
			area.Height -= border_size;
			if ( panel.BorderStyle != StatusBarPanelBorderStyle.None )
			{
				DrawNiceRoundedBorder( dc, area, BorderColor );
			}
			
			if ( panel.Style == StatusBarPanelStyle.OwnerDraw )
			{
				StatusBarDrawItemEventArgs e = new StatusBarDrawItemEventArgs(
					dc, panel.Parent.Font, area, index, DrawItemState.Default,
					panel, panel.Parent.ForeColor, panel.Parent.BackColor );
				panel.Parent.OnDrawItemInternal( e );
				return;
			}
			
			int left = area.Left;
			if ( panel.Icon != null )
			{
				left += 2;
				dc.DrawIcon( panel.Icon, left, area.Top );
				left += panel.Icon.Width;
			}
			
			if ( panel.Text == String.Empty )
				return;
			
			string text = panel.Text;
			StringFormat string_format = new StringFormat( );
			string_format.Trimming = StringTrimming.Character;
			string_format.FormatFlags = StringFormatFlags.NoWrap;
			
			if ( text[ 0 ] == '\t' )
			{
				string_format.Alignment = StringAlignment.Center;
				text = text.Substring( 1 );
				if ( text[ 0 ] == '\t' )
				{
					string_format.Alignment = StringAlignment.Far;
					text = text.Substring( 1 );
				}
			}
			
			int x = left + border_size;
			int y = border_size + 2;
			Rectangle r = new Rectangle( x, y,
						    area.Right - x - border_size,
						    area.Bottom - y - border_size );
			
			dc.DrawString( text, panel.Parent.Font, br_forecolor, r, string_format );
		}
		
		private void DrawNiceRoundedBorder( Graphics dc, Rectangle area, Color color )
		{
			Pen pen = ResPool.GetPen( color ); 
			
			Point[] points = new Point[] {
				new Point( area.Left + 2, area.Top ),
				new Point( area.Right - 2, area.Top ),
				new Point( area.Right, area.Top + 2 ),
				new Point( area.Right, area.Bottom - 2 ),
				new Point( area.Right - 2, area.Bottom ),
				new Point( area.Left + 2, area.Bottom ),
				new Point( area.Left, area.Bottom - 2 ),
				new Point( area.Left, area.Top + 2 ),
				new Point( area.Left + 2, area.Top )
			};
			
			dc.DrawLines( pen, points );
		}
		#endregion	// StatusBar
		
		public override void DrawTabControl( Graphics dc, Rectangle area, TabControl tab )
		{
			// Do we need to fill the back color? It can't be changed...
			dc.FillRectangle( ResPool.GetSolidBrush( NiceBackColor ), area );
			Rectangle panel_rect = GetTabPanelRectExt( tab );
			
			if ( tab.Appearance == TabAppearance.Normal )
			{
				CPDrawBorder( dc, panel_rect, BorderColor, 1, ButtonBorderStyle.Solid, BorderColor, 1, ButtonBorderStyle.Solid,
					     BorderColor, 1, ButtonBorderStyle.Solid, BorderColor, 1, ButtonBorderStyle.Solid );
			}
			
			if ( tab.Alignment == TabAlignment.Top )
			{
				for ( int r = tab.TabPages.Count; r > 0; r-- )
				{
					for ( int i = tab.SliderPos; i < tab.TabPages.Count; i++ )
					{
						if ( i == tab.SelectedIndex )
							continue;
						if ( r != tab.TabPages[ i ].Row )
							continue;
						Rectangle rect = tab.GetTabRect( i );
						if ( !rect.IntersectsWith( area ) )
							continue;
						DrawTab( dc, tab.TabPages[ i ], tab, rect, false );
					}
				}
			}
			else
			{
				for ( int r = 0; r < tab.TabPages.Count; r++ )
				{
					for ( int i = tab.SliderPos; i < tab.TabPages.Count; i++ )
					{
						if ( i == tab.SelectedIndex )
							continue;
						if ( r != tab.TabPages[ i ].Row )
							continue;
						Rectangle rect = tab.GetTabRect( i );
						if ( !rect.IntersectsWith( area ) )
							continue;
						DrawTab( dc, tab.TabPages[ i ], tab, rect, false );
					}
				}
			}
			
			if ( tab.SelectedIndex != -1 && tab.SelectedIndex >= tab.SliderPos )
			{
				Rectangle rect = tab.GetTabRect( tab.SelectedIndex );
				if ( rect.IntersectsWith( area ) )
					DrawTab( dc, tab.TabPages[ tab.SelectedIndex ], tab, rect, true );
			}
			
			if ( tab.ShowSlider )
			{
				Rectangle right = GetTabControlRightScrollRect( tab );
				Rectangle left = GetTabControlLeftScrollRect( tab );
				CPDrawScrollButton( dc, right, ScrollButton.Right, tab.RightSliderState );
				CPDrawScrollButton( dc, left, ScrollButton.Left, tab.LeftSliderState );
			}
		}
		
		protected override int DrawTab( Graphics dc, TabPage page, TabControl tab, Rectangle bounds, bool is_selected )
		{
			int FlatButtonSpacing = 8;
			Rectangle interior;
			int res = bounds.Width;
			
			// we can't fill the background right away because the bounds might be adjusted if the tab is selected
			
			if ( tab.Appearance == TabAppearance.Buttons || tab.Appearance == TabAppearance.FlatButtons )
			{
				dc.FillRectangle( ResPool.GetSolidBrush( NiceBackColor ), bounds );
				
				// Separators
				if ( tab.Appearance == TabAppearance.FlatButtons )
				{
					int width = bounds.Width;
					bounds.Width += ( FlatButtonSpacing - 2 );
					res = bounds.Width;
					CPDrawBorder3D( dc, bounds, Border3DStyle.Etched, Border3DSide.Right );
					bounds.Width = width;
				}
				
				if ( is_selected )
				{
					CPDrawBorder3D( dc, bounds, Border3DStyle.Sunken, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom);
				}
				else if ( tab.Appearance != TabAppearance.FlatButtons )
				{
					CPDrawBorder3D( dc, bounds, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom);
				}
				
				interior = new Rectangle( bounds.Left + 2, bounds.Top + 2, bounds.Width - 4, bounds.Height - 4 );
				
                                
				StringFormat string_format = new StringFormat( );
				string_format.Alignment = StringAlignment.Center;
				string_format.LineAlignment = StringAlignment.Center;
				string_format.FormatFlags = StringFormatFlags.NoWrap;
				
				interior.Y++;
				dc.DrawString( page.Text, page.Font, ResPool.GetSolidBrush( SystemColors.ControlText ), interior, string_format );
				interior.Y--;
			}
			else
			{
				Pen border_pen = ResPool.GetPen( BorderColor );
				
				dc.FillRectangle( ResPool.GetSolidBrush( NiceBackColor ), bounds );
				
				switch ( tab.Alignment )
				{
					case TabAlignment.Top:
						
						if ( !is_selected )
						{
							interior = new Rectangle( bounds.Left + 2, bounds.Top + 2, bounds.Width - 2, bounds.Height - 2 );
							
							using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( bounds.Left + 2, bounds.Top + 2  ), new Point( bounds.Left + 2, bounds.Bottom ), Color.White, LightColor ) )
							{
								dc.FillRectangle( lgbr, interior );
							}
						}
						
						dc.DrawLine( border_pen, bounds.Left, bounds.Bottom, bounds.Left, bounds.Top + 3 );
						dc.DrawLine( border_pen, bounds.Left, bounds.Top + 3, bounds.Left + 3, bounds.Top );
						dc.DrawLine( border_pen, bounds.Left + 3, bounds.Top, bounds.Right - 3, bounds.Top );
						dc.DrawLine( border_pen, bounds.Right - 3, bounds.Top, bounds.Right, bounds.Top + 3 );
						dc.DrawLine( border_pen, bounds.Right, bounds.Top + 3, bounds.Right, bounds.Bottom );
						
						if ( page.Focused )
						{
							dc.DrawLine( ResPool.GetPen( Color.DarkOrange ), bounds.Left - 1 , bounds.Top, bounds.Right - 1, bounds.Top );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Left , bounds.Top + 1, bounds.Right , bounds.Top + 1 );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Left , bounds.Top + 2, bounds.Right , bounds.Top + 2 );
						}
						
						interior = new Rectangle( bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8 );
						
						if ( page.Text != String.Empty )
						{
							StringFormat string_format = new StringFormat( );
							string_format.Alignment = StringAlignment.Center;
							string_format.LineAlignment = StringAlignment.Center;
							string_format.FormatFlags = StringFormatFlags.NoWrap;
							interior.Y++;
							dc.DrawString( page.Text, page.Font, ResPool.GetSolidBrush( SystemColors.ControlText ), interior, string_format );
							interior.Y--;
						}
						
						break;
						
					case TabAlignment.Bottom:
						
						if ( !is_selected )
						{
							interior = new Rectangle( bounds.Left + 3, bounds.Top, bounds.Width - 3, bounds.Height );
							
							using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( bounds.Left + 3, bounds.Top  ), new Point( bounds.Left + 3, bounds.Bottom  ), Color.White, LightColor ) )
							{
								dc.FillRectangle( lgbr, interior );
							}
						}
						
						dc.DrawLine( border_pen, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom - 3 );
						dc.DrawLine( border_pen, bounds.Left, bounds.Bottom - 3, bounds.Left + 3, bounds.Bottom );
						dc.DrawLine( border_pen, bounds.Left + 3, bounds.Bottom, bounds.Right - 3, bounds.Bottom );
						dc.DrawLine( border_pen, bounds.Right - 3, bounds.Bottom, bounds.Right, bounds.Bottom - 3 );
						dc.DrawLine( border_pen, bounds.Right, bounds.Bottom - 3, bounds.Right, bounds.Top );
						
						if ( page.Focused )
						{
							dc.DrawLine( ResPool.GetPen( Color.DarkOrange ), bounds.Left - 1 , bounds.Bottom, bounds.Right - 1, bounds.Bottom );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Left , bounds.Bottom - 1, bounds.Right , bounds.Bottom - 1 );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Left , bounds.Bottom - 2, bounds.Right , bounds.Bottom - 2 );
						}
						
						interior = new Rectangle( bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8 );
						
						if ( page.Text != String.Empty )
						{
							StringFormat string_format = new StringFormat( );
							string_format.Alignment = StringAlignment.Center;
							string_format.LineAlignment = StringAlignment.Center;
							string_format.FormatFlags = StringFormatFlags.NoWrap;
							interior.Y++;
							dc.DrawString( page.Text, page.Font, ResPool.GetSolidBrush( SystemColors.ControlText ), interior, string_format );
							interior.Y--;
						}
						
						break;
						
					case TabAlignment.Left:
						
						if ( !is_selected )
						{
							interior = new Rectangle( bounds.Left + 2, bounds.Top + 2, bounds.Width - 2, bounds.Height - 2 );
							
							using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( bounds.Left + 2, bounds.Top + 2  ), new Point( bounds.Right, bounds.Top + 2 ), LightColor, Color.White ) )
							{
								dc.FillRectangle( lgbr, interior );
							}
						}
						
						dc.DrawLine( border_pen, bounds.Right, bounds.Top, bounds.Left + 3, bounds.Top );
						dc.DrawLine( border_pen, bounds.Left + 3, bounds.Top, bounds.Left, bounds.Top + 3 );
						dc.DrawLine( border_pen, bounds.Left, bounds.Top + 3, bounds.Left, bounds.Bottom - 3 );
						dc.DrawLine( border_pen, bounds.Left, bounds.Bottom - 3, bounds.Left + 3, bounds.Bottom );
						dc.DrawLine( border_pen, bounds.Left + 3, bounds.Bottom, bounds.Right, bounds.Bottom );
						
						if ( page.Focused )
						{
							dc.DrawLine( ResPool.GetPen( Color.DarkOrange ), bounds.Left , bounds.Top + 1, bounds.Left , bounds.Bottom - 1 );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Left + 1 , bounds.Top, bounds.Left + 1 , bounds.Bottom );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Left + 2 , bounds.Top, bounds.Left + 2 , bounds.Bottom );
						}
						
						interior = new Rectangle( bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8 );
						
						if ( page.Text != String.Empty )
						{
							StringFormat string_format = new StringFormat( );
							// Flip the text around
							string_format.Alignment = StringAlignment.Center;
							string_format.LineAlignment = StringAlignment.Center;
							string_format.FormatFlags = StringFormatFlags.NoWrap;
							string_format.FormatFlags = StringFormatFlags.DirectionVertical;
							int wo = interior.Width / 2;
							int ho = interior.Height / 2;
							dc.TranslateTransform( interior.X + wo, interior.Y + ho );
							dc.RotateTransform( 180 );
							dc.DrawString( page.Text, page.Font, ResPool.GetSolidBrush( SystemColors.ControlText ), 0, 0, string_format );
							dc.ResetTransform( );
						}
						
						break;
						
					default:
						// TabAlignment.Right
						
						if ( !is_selected )
						{
							interior = new Rectangle( bounds.Left + 2, bounds.Top + 2, bounds.Width - 2, bounds.Height - 2 );
							
							using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( bounds.Left + 2, bounds.Top + 2  ), new Point( bounds.Right, bounds.Top + 2 ), Color.White, LightColor ) )
							{
								dc.FillRectangle( lgbr, interior );
							}
						}
						
						dc.DrawLine( border_pen, bounds.Left, bounds.Top, bounds.Right - 3, bounds.Top );
						dc.DrawLine( border_pen, bounds.Right - 3, bounds.Top, bounds.Right, bounds.Top + 3 );
						dc.DrawLine( border_pen, bounds.Right, bounds.Top + 3, bounds.Right, bounds.Bottom - 3 );
						dc.DrawLine( border_pen, bounds.Right, bounds.Bottom - 3, bounds.Right - 3, bounds.Bottom );
						dc.DrawLine( border_pen, bounds.Right - 3, bounds.Bottom, bounds.Left, bounds.Bottom );
						
						if ( page.Focused )
						{
							dc.DrawLine( ResPool.GetPen( Color.DarkOrange ), bounds.Right , bounds.Top + 1, bounds.Right , bounds.Bottom - 1 );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Right - 1 , bounds.Top, bounds.Right - 1 , bounds.Bottom );
							dc.DrawLine( ResPool.GetPen( Color.Orange ), bounds.Right - 2 , bounds.Top, bounds.Right - 2 , bounds.Bottom );
						}
						
						interior = new Rectangle( bounds.Left + 4, bounds.Top + 4, bounds.Width - 8, bounds.Height - 8 );
						
						if ( page.Text != String.Empty )
						{
							StringFormat string_format = new StringFormat( );
							string_format.Alignment = StringAlignment.Center;
							string_format.LineAlignment = StringAlignment.Center;
							string_format.FormatFlags = StringFormatFlags.NoWrap;
							string_format.FormatFlags = StringFormatFlags.DirectionVertical;
							interior.X++;
							dc.DrawString( page.Text, page.Font, ResPool.GetSolidBrush( SystemColors.ControlText ), interior, string_format );
							interior.X--;
						}
						
						break;
				}
			}
			
			return res;
		}		
		
		public override void CPDrawComboButton( Graphics dc, Rectangle rectangle, ButtonState state )
		{
			Point[]			arrow = new Point[ 3 ];
			Point				P1;
			Point				P2;
			Point				P3;
			int				centerX;
			int				centerY;
			int				shiftX;
			int				shiftY;
			Rectangle		rect;
			
			Color first_color = Color.White;
			Color second_color = NormalColor;
			
//			rectangle.Width += 1;
			
			if ( ( state & ButtonState.Checked ) != 0 )
			{
				dc.FillRectangle( ResPool.GetHatchBrush( HatchStyle.Percent50, ColorControlLightLight, ColorControlLight ), rectangle );
			}
			else
				dc.FillRectangle( ResPool.GetSolidBrush( Color.White ), rectangle );
			
			if ( ( state & ButtonState.Flat ) != 0 )
			{
				first_color = NormalColor;
				second_color = Color.White;
			}
			else
			{
				if ( ( state & ( ButtonState.Pushed | ButtonState.Checked ) ) != 0 )
				{
					first_color = Color.White;
					second_color = PressedColor;
				}
//				else
//				{
//					CPDrawBorder3D( graphics, rectangle, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom, ColorControl );
//				}
			}
			
			using ( LinearGradientBrush lgbr = new LinearGradientBrush( new Point( rectangle.X + 1, rectangle.Y + 1 ), new Point( rectangle.X + 1, rectangle.Bottom - 2 ), first_color, second_color ) )
			{
				dc.FillRectangle( lgbr, rectangle.X + 2, rectangle.Y + 1, rectangle.Width - 4, rectangle.Height - 3 );
			}
			
			Point[] points = new Point[] {
				new Point( rectangle.X + 3, rectangle.Y + 1 ),
				new Point( rectangle.Right - 4, rectangle.Y + 1 ),
				new Point( rectangle.Right - 2, rectangle.Y + 3 ),
				new Point( rectangle.Right - 2, rectangle.Bottom - 4 ),
				new Point( rectangle.Right - 4, rectangle.Bottom - 2 ),
				new Point( rectangle.X + 3, rectangle.Bottom - 2 ),
				new Point( rectangle.X + 1, rectangle.Bottom - 4 ),
				new Point( rectangle.X + 1, rectangle.Y + 3 ),
				new Point( rectangle.X + 3, rectangle.Y + 1 )
			};
			
			dc.DrawPolygon( ResPool.GetPen( BorderColor ), points );
			
			rect = new Rectangle( rectangle.X + rectangle.Width / 4, rectangle.Y + rectangle.Height / 4, rectangle.Width / 2, rectangle.Height / 2 );
			centerX = rect.Left + rect.Width / 2;
			centerY = rect.Top + rect.Height / 2;
			shiftX = Math.Max( 1, rect.Width / 8 );
			shiftY = Math.Max( 1, rect.Height / 8 );
			
			if ( ( state & ButtonState.Pushed ) != 0 )
			{
				shiftX--;
				shiftY--;
			}
			
			rect.Y -= shiftY;
			centerY -= shiftY;
			
			P1 = new Point( rect.Left, centerY );
			P2 = new Point( centerX, rect.Bottom );
			P3 = new Point( rect.Right, centerY );
			
			arrow[ 0 ] = P1;
			arrow[ 1 ] = P2;
			arrow[ 2 ] = P3;
			
			SmoothingMode old_smoothing_mode = dc.SmoothingMode;
			dc.SmoothingMode = SmoothingMode.AntiAlias;
			
			/* Draw the arrow */
			if ( ( state & ButtonState.Inactive ) != 0 )
			{
				using ( Pen pen = new Pen( SystemColors.ControlLightLight, 2 ) )
				{
					dc.DrawLines( pen, arrow );
				}
				
				/* Move away from the shadow */
				P1.X -= 1;		P1.Y -= 1;
				P2.X -= 1;		P2.Y -= 1;
				P3.X -= 1;		P3.Y -= 1;
				
				arrow[ 0 ] = P1;
				arrow[ 1 ] = P2;
				arrow[ 2 ] = P3;
				
				using ( Pen pen = new Pen( SystemColors.ControlDark, 2 ) )
				{
					dc.DrawLines( pen, arrow );
				}
			}
			else
			{
				using ( Pen pen = new Pen( SystemColors.ControlText, 2 ) )
				{
					dc.DrawLines( pen, arrow );
				}
			}
			
			dc.SmoothingMode = old_smoothing_mode;
		}
		
		/* Scroll button: regular button + direction arrow */
		public override void CPDrawScrollButton( Graphics dc, Rectangle area, ScrollButton scroll_button_type, ButtonState state )
		{
			bool enabled = ( state == ButtonState.Inactive ) ? false: true;
			
			DrawScrollButtonPrimitive( dc, area, state, scroll_button_type );
			
			if ( area.Width < 12 || area.Height < 12 ) /* Cannot see a thing at smaller sizes */
				return;
			
			Pen pen = null;
			
			if ( enabled )
				pen = new Pen( arrow_color, 2 );
			else
				pen = new Pen( ColorGrayText, 2 );
			
			/* Paint arrows */
			
			int centerX = area.Left + area.Width / 2;
			int centerY = area.Top + area.Height / 2;
			
			int shift = 0;
			
			if ( ( state & ButtonState.Pushed ) != 0 )
				shift = 1;
			
			Point[]	arrow = new Point[ 3 ];
			
			switch ( scroll_button_type )
			{
				case ScrollButton.Down:
					centerY += shift;
					arrow[ 0 ] = new Point( centerX - 3, centerY - 2 );
					arrow[ 1 ] = new Point( centerX, centerY + 2 );
					arrow[ 2 ] = new Point( centerX + 3, centerY - 2 );
					break;
				case ScrollButton.Up:
					centerY -= shift;
					arrow[ 0 ] = new Point( centerX - 3, centerY + 2 );
					arrow[ 1 ] = new Point( centerX, centerY - 2 );
					arrow[ 2 ] = new Point( centerX + 3, centerY + 2 );
					break;
				case ScrollButton.Left:
					centerX -= shift;
					arrow[ 0 ] = new Point( centerX + 2, centerY - 3 );
					arrow[ 1 ] = new Point( centerX - 2, centerY );
					arrow[ 2 ] = new Point( centerX + 2, centerY + 3 );
					break;
				case ScrollButton.Right:
					centerX += shift;
					arrow[ 0 ] = new Point( centerX - 2, centerY - 3 );
					arrow[ 1 ] = new Point( centerX + 2, centerY );
					arrow[ 2 ] = new Point( centerX - 2, centerY + 3 );
					break;
				default:
					break;
			}

			SmoothingMode old_smoothing_mode = dc.SmoothingMode;
			dc.SmoothingMode = SmoothingMode.AntiAlias;
			
			dc.DrawLines( pen, arrow );
			
			dc.SmoothingMode = old_smoothing_mode;
			
			pen.Dispose( );
		}
		
		public override void CPDrawSizeGrip( Graphics dc, Color backColor, Rectangle bounds )
		{
			Point pt = new Point( bounds.Right - 4, bounds.Bottom - 4 );
			
			dc.DrawImage( size_grip_bmp, pt );
			dc.DrawImage( size_grip_bmp, pt.X, pt.Y - 5 );
			dc.DrawImage( size_grip_bmp, pt.X, pt.Y - 10 );
			dc.DrawImage( size_grip_bmp, pt.X - 5, pt.Y );
			dc.DrawImage( size_grip_bmp, pt.X - 10, pt.Y );
			dc.DrawImage( size_grip_bmp, pt.X - 5, pt.Y - 5 );
		}
		
		private void DrawScrollBarThumb( Graphics dc, Rectangle area, ScrollBar bar )
		{
			LinearGradientBrush lgbr = null;
			
			if ( bar.vert )
				lgbr = new LinearGradientBrush( new Point( area.X, area.Y ), new Point( area.Right, area.Y ), Color.White, NormalColor );
			else
				lgbr = new LinearGradientBrush( new Point( area.X, area.Y ), new Point( area.X, area.Bottom ), Color.White, NormalColor );
			
			Pen pen = ResPool.GetPen( BorderColor );
			
			Point[] points = new Point[] {
				new Point( area.X + 2, area.Y ),
				new Point( area.Right - 3, area.Y ),
				new Point( area.Right - 1, area.Y + 2 ),
				new Point( area.Right - 1, area.Bottom - 3 ),
				new Point( area.Right - 3, area.Bottom - 1 ),
				new Point( area.X + 2, area.Bottom - 1 ),
				new Point( area.X, area.Bottom - 3 ),
				new Point( area.X, area.Y + 2 ),
				new Point( area.X + 2, area.Y )
			};
			
			if ( bar.vert )
			{
				dc.FillRectangle( lgbr, area.X + 1, area.Y + 1, area.Width - 2, area.Height - 2 );
				dc.DrawPolygon( pen, points );
				
				// draw grip lines only if stere is enough space
				if ( area.Height > 20 )
				{
					int mid_y = area.Y + ( area.Height / 2 );
					int mid_x = area.X + ( area.Width / 2 );
					
					using ( Pen lpen = new Pen( MouseOverColor, 2 ) )
					{
						dc.DrawLine( lpen, mid_x - 3, mid_y, mid_x + 3, mid_y );
						dc.DrawLine( lpen, mid_x - 3, mid_y - 4, mid_x + 3, mid_y - 4 );
						dc.DrawLine( lpen, mid_x - 3, mid_y + 4, mid_x + 3, mid_y + 4 );
					}
					
					Pen spen = ResPool.GetPen( Color.White );
					dc.DrawLine( spen, mid_x - 3, mid_y - 1, mid_x + 3, mid_y - 1 );
					dc.DrawLine( spen, mid_x - 3, mid_y - 5, mid_x + 3, mid_y - 5 );
					dc.DrawLine( spen, mid_x - 3, mid_y + 3, mid_x + 3, mid_y + 3 );
				}
			}
			else
			{
				dc.FillRectangle( lgbr, area.X + 1, area.Y + 1, area.Width - 2, area.Height - 2 );
				dc.DrawPolygon( pen, points );
				
				// draw grip lines only if stere is enough space
				if ( area.Width > 20 )
				{
					int mid_x = area.X +  ( area.Width / 2 );
					int mid_y = area.Y +  ( area.Height / 2 );
					
					using ( Pen lpen = new Pen( MouseOverColor, 2 ) )
					{
						dc.DrawLine( lpen, mid_x, mid_y - 3, mid_x, mid_y + 3 );
						dc.DrawLine( lpen, mid_x - 4, mid_y - 3, mid_x - 4, mid_y + 3 );
						dc.DrawLine( lpen, mid_x + 4, mid_y - 3, mid_x + 4, mid_y + 3 );
					}
					
					Pen spen = ResPool.GetPen( Color.White );
					dc.DrawLine( spen, mid_x - 1, mid_y - 3, mid_x - 1, mid_y + 3 );
					dc.DrawLine( spen, mid_x - 5, mid_y - 3, mid_x - 5, mid_y + 3 );
					dc.DrawLine( spen, mid_x + 3, mid_y - 3, mid_x + 3, mid_y + 3 );
				}
			}
			
			lgbr.Dispose( );
		}
		
		/* Nice scroll button */
		public void DrawScrollButtonPrimitive( Graphics dc, Rectangle area, ButtonState state, ScrollButton scroll_button_type )
		{
			Pen pen = ResPool.GetPen( BorderColor );
			
			dc.FillRectangle( ResPool.GetSolidBrush( NiceBackColor ), area );
			
			Color use_color;
			
			if ( ( state & ButtonState.Pushed ) == ButtonState.Pushed )
				use_color = PressedColor;
			else
				use_color = NormalColor;
			
			Point[] points = null;
			
			LinearGradientBrush lgbr = null;
			
			switch ( scroll_button_type )
			{
				case ScrollButton.Left:
					lgbr = new LinearGradientBrush( new Point( area.X, area.Y ), new Point( area.Right - 2, area.Y ), use_color, Color.White );
					dc.FillRectangle( lgbr, area.X + 1, area.Y + 1, area.Width - 4, area.Height - 2 );
					
					points = new Point[] {
						new Point( area.X + 2, area.Y ),
						new Point( area.Right - 1, area.Y ),
						new Point( area.Right - 1, area.Bottom - 1 ),
						new Point( area.X + 2, area.Bottom - 1 ),
						new Point( area.X, area.Bottom - 3 ),
						new Point( area.X, area.Y + 2 ),
						new Point( area.X + 2, area.Y )
					};
					dc.DrawPolygon( pen, points );
					break;
				case ScrollButton.Right:
					lgbr = new LinearGradientBrush( new Point( area.X, area.Y ), new Point( area.Right - 1, area.Y ), Color.White, use_color );
					dc.FillRectangle( lgbr, area.X, area.Y + 1, area.Width - 1, area.Height - 2 );
					
					points = new Point[] {
						new Point( area.X, area.Y ),
						new Point( area.Right - 3, area.Y ),
						new Point( area.Right - 1, area.Y + 2 ),
						new Point( area.Right - 1, area.Bottom - 3 ),
						new Point( area.Right - 3, area.Bottom - 1 ),
						new Point( area.X, area.Bottom - 1 ),
						new Point( area.X, area.Y ),
					};
					dc.DrawPolygon( pen, points );
					break;
				case ScrollButton.Up:
					lgbr = new LinearGradientBrush( new Point( area.X, area.Y ), new Point( area.X, area.Bottom - 1 ), use_color, Color.White );
					dc.FillRectangle( lgbr, area.X + 1, area.Y + 1, area.Width - 2, area.Height - 2 );
					
					points = new Point[] {
						new Point( area.X + 2, area.Y ),
						new Point( area.Right - 3, area.Y ),
						new Point( area.Right - 1, area.Y + 2 ),
						new Point( area.Right - 1, area.Bottom - 1 ),
						new Point( area.X, area.Bottom - 1 ),
						new Point( area.X, area.Y + 2 ),
						new Point( area.X + 2, area.Y )
					};
					dc.DrawPolygon( pen, points );
					break;
				case ScrollButton.Down:
					lgbr = new LinearGradientBrush( new Point( area.X, area.Y ), new Point( area.X, area.Bottom - 1 ), Color.White, use_color );
					dc.FillRectangle( lgbr, area.X + 1, area.Y + 1, area.Width - 2, area.Height - 2 );
					
					points = new Point[] {
						new Point( area.X, area.Y ),
						new Point( area.Right - 1, area.Y ),
						new Point( area.Right - 1, area.Bottom - 3 ),
						new Point( area.Right - 3, area.Bottom - 1 ),
						new Point( area.X + 2, area.Bottom - 1 ),
						new Point( area.X, area.Bottom - 3 ),
						new Point( area.X, area.Y )
					};
					dc.DrawPolygon( pen, points );
					break;
			}
			
			lgbr.Dispose( );
		}
		
		#region GroupBox
		public override void DrawGroupBox (Graphics dc,  Rectangle area, GroupBox box) 
		{
			StringFormat	text_format;
			SizeF		size;
			int		width;
			int		y;
			Rectangle	rect;
			
			rect = box.ClientRectangle;
			
			dc.FillRectangle (ResPool.GetSolidBrush (box.BackColor), rect);
			
			text_format = new StringFormat();
			text_format.HotkeyPrefix = HotkeyPrefix.Show;
			
			size = dc.MeasureString (box.Text, box.Font);
			width = (int) size.Width;
			
			if (width > box.Width - 16)
				width = box.Width - 16;
			
			y = box.Font.Height / 2;
			
			Pen pen = ResPool.GetPen( BorderColor );
			
			/* Draw group box*/
			Point[] points = new Point[] {
				new Point( 8 + width, y ),
				new Point( box.Width - 3, y ),
				new Point( box.Width - 1, y + 2 ),
				new Point( box.Width - 1, box.Height - 3 ),
				new Point( box.Width - 3, box.Height - 1 ),
				new Point( 2, box.Height - 1 ),
				new Point( 0, box.Height - 3 ),
				new Point( 0, y + 2 ),
				new Point( 2, y ),
				new Point( 8, y )
			};
			dc.DrawLines( pen, points );
			
			/* Text */
			if (box.Enabled) {
				dc.DrawString (box.Text, box.Font, ResPool.GetSolidBrush (box.ForeColor), 10, 0, text_format);
			} else {
				CPDrawStringDisabled (dc, box.Text, box.Font, box.ForeColor, 
						      new RectangleF (10, 0, width,  box.Font.Height), text_format);
			}
			text_format.Dispose ();	
		}
		#endregion
	} //class
}

