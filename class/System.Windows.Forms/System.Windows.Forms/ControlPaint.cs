//
// System.Windows.Forms.ControlPaint.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//	 Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) Ximian, Inc 2002
//


using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Provides methods used to paint common Windows controls and their elements.
	/// </summary>
	
	[MonoTODO]
	public sealed class ControlPaint {

		#region Properties
		[MonoTODO]
		public static Color ContrastControlDark {

			get { throw new NotImplementedException (); }
		}
		#endregion
		
		#region Helpers
		/*
		internal static HISColorCheck() {		
			Color[] cArr = new Color[] { SystemColors.ControlText, SystemColors.Control, SystemColors.GrayText};
			foreach( Color c in cArr) {
				double H1 = c.GetHue();
				double I1 = c.GetBrightness();
				double S1 = c.GetSaturation();
				double H2, I2, S2;
				ControlPaint.Color2HIS(c, out H2, out I2, out S2);

				Color c2 = ControlPaint.HIS2Color( H2, I2, S2);
			}
		}
		*/				
		internal static void Color2HIS(Color col, out double Hue, out double Intensity, out double Saturation) {
			Hue = 0.0;
			Saturation = 0.0;

			double red = (double)col.R / 255.0;
			double green = (double)col.G / 255.0;
			double blue = (double)col.B / 255.0;

			Intensity = Math.Max(red, green);
			Intensity = Math.Max(Intensity, blue);

			if( Intensity != 0.0) {
				double IntensityDiff = Intensity - Math.Min( Math.Min( red, green), blue);
				if( IntensityDiff != 0.0) {
					Saturation = IntensityDiff / Intensity;
					if( Intensity == red) {
						double b = ( Intensity - blue) / IntensityDiff;
						double g = ( Intensity - green) / IntensityDiff;
						Hue = 10.0 * ( b - g);
					}
					else if( Intensity == green) {
						double r = ( Intensity - red) / IntensityDiff;
						double b = ( Intensity - blue) / IntensityDiff;
						Hue = 10.0 * ( 2 + r - b);
					}
					else {
						double g = ( Intensity - green) / IntensityDiff;
						double r = ( Intensity - red) / IntensityDiff;
						Hue = 10.0 * ( 4 + g - r);
					}
					Hue = Hue * 60.0;
					Hue = Math.Round(Hue);
				}
			}
		}

		internal static Color HIS2Color(double Hue, double Intensity, double Saturation) {
			double R = Intensity, G = Intensity, B = Intensity;
			if( Saturation != 0) {
				double Hue1 = (Hue * 60.0) / 360.0;
				double X = Hue1 / 10.0;
				double h = Math.Floor(X);
				double P = Intensity * ( 1 - Saturation);
				double Q = Intensity * ( Saturation - ( X - h));
				double T = Intensity * ( 1 - Saturation * ( 1 - X + h));
				switch( (int)Math.Round(h)) {
					case 0:
						R = Intensity;
						G = T;
						B = P;
						break;
					case 1:
						R = Q;
						G = Intensity;
						B = P;
						break;
					case 2:
						R = P;
						G = Intensity;
						B = T;
						break;
					case 3:
						R = P;
						G = Q;
						B = Intensity;
						break;
					case 4:
						R = T;
						G = P;
						B = Intensity;
						break;
					case 5:
						R = Intensity;
						G = P;
						B = Q;
						break;
				}
			}
			int red = (int)(R * 255.0);
			int gree = (int)(G * 255.0);
			int blue = (int)(B * 255.0);
			return Color.FromArgb(red, gree, blue);
		}

		#endregion

		#region Methods
		/// following methods were not stubbed out, because they only support .NET framework:
		
		[MonoTODO]
		public static IntPtr CreateHBitmap16Bit(Bitmap bitmap,Color background){
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static IntPtr CreateHBitmapColorMask(Bitmap bitmap,IntPtr monochromeMask){
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static IntPtr CreateHBitmapTransparencyMask(Bitmap bitmap){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Color Dark(Color baseColor) {
			return Dark(baseColor, 10.0f);
		}
		
		[MonoTODO]
		public static Color Dark(Color baseColor,float percOfDarkDark) {
			double H, I, S;
			ControlPaint.Color2HIS(baseColor, out H, out I, out S);
			double NewIntensity = Math.Max( 0.0, I - (percOfDarkDark / 100.0));
			return ControlPaint.HIS2Color(H, NewIntensity, S);
		}
		
		[MonoTODO]
		public static Color DarkDark(Color baseColor) {
			return Dark(baseColor, 20.0f);
		}
		
		[MonoTODO]
		public static void DrawBorder(Graphics graphics, Rectangle bounds, Color color, ButtonBorderStyle style) {
			DrawBorder(graphics, bounds, color, 1, style, color, 1, style, color, 1, style, color, 1, style);
		}
		
		[MonoTODO]
		public static void DrawBorder( Graphics graphics, Rectangle bounds, Color leftColor, int leftWidth,
			ButtonBorderStyle leftStyle, Color topColor, int topWidth, ButtonBorderStyle topStyle,
			Color rightColor, int rightWidth, ButtonBorderStyle rightStyle, Color bottomColor, int bottomWidth,
			ButtonBorderStyle bottomStyle) {

			IntPtr hdc = graphics.GetHdc();

			RECT rc = new RECT();

			// Top side
			Win32.SetBkColor(hdc, (uint)Win32.RGB(topColor));
			rc.left = bounds.Left;
			rc.top = bounds.Top;
			rc.right = bounds.Right - rightWidth;
			rc.bottom = bounds.Top + topWidth;
			Win32.ExtTextOut(hdc, 0, 0, ExtTextOutFlags.ETO_OPAQUE, ref rc, 0, 0, IntPtr.Zero);
			// Left side
			Win32.SetBkColor(hdc, (uint)Win32.RGB(leftColor));
			rc.right = bounds.Left + leftWidth;
			rc.bottom = bounds.Bottom - bottomWidth;
			Win32.ExtTextOut(hdc, 0, 0, ExtTextOutFlags.ETO_OPAQUE, ref rc, 0, 0, IntPtr.Zero);
			// Right side
			Win32.SetBkColor(hdc, (uint)Win32.RGB(rightColor));
			rc.left = bounds.Right - rightWidth;
			rc.right = bounds.Right;
			Win32.ExtTextOut(hdc, 0, 0, ExtTextOutFlags.ETO_OPAQUE, ref rc, 0, 0, IntPtr.Zero);
			// Bottom side
			Win32.SetBkColor(hdc, (uint)Win32.RGB(bottomColor));
			rc.left = bounds.Left;
			rc.top = bounds.Bottom - bottomWidth;
			rc.bottom = bounds.Bottom;
			Win32.ExtTextOut(hdc, 0, 0, ExtTextOutFlags.ETO_OPAQUE, ref rc, 0, 0, IntPtr.Zero);

			graphics.ReleaseHdc(hdc);
		}
		
		[MonoTODO]
		public static void DrawBorder3D(Graphics graphics, Rectangle rectangle) {
			DrawBorder3D(graphics, rectangle, Border3DStyle.Etched, Border3DSide.All);
		}
		
		[MonoTODO]
		public static void DrawBorder3D(Graphics graphics, Rectangle rectangle, Border3DStyle style) {
			DrawBorder3D(graphics, rectangle, style, Border3DSide.All);
		}
		
		[MonoTODO]
		public static void DrawBorder3D( Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides) {
			RECT rc = new RECT();
			rc.left = rectangle.Left;
			rc.top = rectangle.Top;
			rc.right = rectangle.Right;
			rc.bottom = rectangle.Bottom;
			IntPtr hdc = graphics.GetHdc();
			int res = Win32.DrawEdge( hdc, ref rc, style, sides);
			graphics.ReleaseHdc(hdc);
		}

		[MonoTODO]
		public static void DrawBorder3D(
			Graphics graphics, int x) {
			//FIXME:
		}
		//is this part of spec? I do not think so.
		//[MonoTODO]
		//public static void DrawBorder3D(
		//	Graphics graphics, int x) {
		//	throw new NotImplementedException ();
		//}

		[MonoTODO]
		public static void DrawBorder3D( Graphics graphics, int x, int y, int width, int height) {
			DrawBorder3D( graphics, new Rectangle(x, y, width, height));
		}

		[MonoTODO]
		public static void DrawBorder3D(Graphics graphics, int x, int y, int width, int height, Border3DStyle style) {
			DrawBorder3D( graphics, new Rectangle(x, y, width, height), style);
		}

		[MonoTODO]
		public static void DrawBorder3D( Graphics graphics, int x, int y, int width, int height,
			Border3DStyle style,Border3DSide sides) {
			DrawBorder3D( graphics, new Rectangle(x, y, width, height), style, sides);
		}

		[MonoTODO]
		public static void DrawButton( Graphics graphics, Rectangle rectangle, ButtonState state) {
			RECT rc = new RECT();
			rc.left = rectangle.Left;
			rc.top = rectangle.Top;
			rc.right = rectangle.Right;
			rc.bottom = rectangle.Bottom;
			IntPtr hdc = graphics.GetHdc();
			int res = Win32.DrawFrameControl( hdc, ref rc, (uint)DrawFrameControl.DFC_BUTTON,
				(uint)state | (uint)DrawFrameControl.DFCS_BUTTONPUSH);
			graphics.ReleaseHdc(hdc);
		}

		[MonoTODO]
		public static void DrawButton( Graphics graphics, int x, int y, int width, int height, ButtonState state) {
			DrawButton( graphics, new Rectangle(x, y, width, height), state);
		}

		[MonoTODO]
		public static void DrawCaptionButton(
			Graphics graphics,
			Rectangle rectangle,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawCaptionButton(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			CaptionButton button,
			ButtonState state) {
			//FIXME:
		}
		
		public static void DrawCheckBox( Graphics graphics, Rectangle rectangle, ButtonState state) {
			DrawFrameControlHelper (graphics, rectangle, (uint)DrawFrameControl.DFC_BUTTON, (uint)state | (uint)DrawFrameControl.DFCS_BUTTONCHECK);
		}
		
		[MonoTODO]
		public static void DrawCheckBox(Graphics graphics, int x, int y, int width, int height, ButtonState state) {
			DrawCheckBox(graphics, new Rectangle(x, y, width, height), state);
		}
		
		[MonoTODO]
		public static void DrawComboButton(
			Graphics graphics,
			Rectangle rectangle,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawComboButton(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawContainerGrabHandle(Graphics graphics,Rectangle bounds) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawFocusRectangle( Graphics graphics, Rectangle rectangle) {
			RECT rc = new RECT();
			rc.left = rectangle.Left;
			rc.top = rectangle.Top;
			rc.right = rectangle.Right;
			rc.bottom = rectangle.Bottom;
			IntPtr hdc = graphics.GetHdc();
			int res = Win32.DrawFocusRect( hdc, ref rc);
			graphics.ReleaseHdc(hdc);
		}
		
		[MonoTODO]
		public static void DrawFocusRectangle( Graphics graphics, Rectangle rectangle, Color foreColor, Color backColor) {
			//FIXME: what to do with colors ?
			DrawFocusRectangle( graphics, rectangle);			
		}
		
		[MonoTODO]
		public static void DrawGrabHandle(
			Graphics graphics,
			Rectangle rectangle,
			bool primary,
			bool enabled) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawGrid(
			Graphics graphics,
			Rectangle area,
			Size pixelsBetweenDots,
			Color backColor) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawImageDisabled(
			Graphics graphics,
			Image image,
			int x,
			int y,
			Color background) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawLockedFrame(
			Graphics graphics,
			Rectangle rectangle,
			bool primary) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawMenuGlyph(
			Graphics graphics,
			Rectangle rectangle,
			MenuGlyph glyph) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawMenuGlyph(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			MenuGlyph glyph) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawMixedCheckBox(
			Graphics graphics,
			Rectangle rectangle,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawMixedCheckBox(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			ButtonState state) {
			//FIXME:
		}

		internal static void CopyImageTransparent (IntPtr targetDC, IntPtr sourceDC, Rectangle rectangle, Color transparentColor) {
			// Monochrome mask
			IntPtr maskDC = Win32.CreateCompatibleDC (sourceDC);
			IntPtr maskBmp = Win32.CreateBitmap (rectangle.Width, rectangle.Height, 1, 1, IntPtr.Zero);
			IntPtr oldMaskBmp = Win32.SelectObject (maskDC, maskBmp);

			uint oldColor = Win32.SetBkColor (sourceDC, (uint)Win32.RGB (transparentColor));
			Win32.StretchBlt (maskDC, 0, 0, rectangle.Width, rectangle.Height, sourceDC, 
				0, 0, rectangle.Width, rectangle.Height, PatBltTypes.SRCCOPY);
			Win32.SetBkColor (sourceDC, oldColor);

			Win32.StretchBlt (targetDC, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height,
				sourceDC, 0, 0, rectangle.Width, rectangle.Height, PatBltTypes.SRCINVERT);

			uint oldBkClr = Win32.SetBkColor (targetDC, 0xFFFFFF);
			int oldTextClr = Win32.SetTextColor (targetDC, 0);
			Win32.StretchBlt (targetDC, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height,
				maskDC, 0, 0, rectangle.Width, rectangle.Height, PatBltTypes.SRCAND);
			Win32.SetTextColor (targetDC, oldTextClr);
			Win32.SetBkColor (targetDC, oldBkClr);

			Win32.StretchBlt (targetDC, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height,
				sourceDC, 0, 0, rectangle.Width, rectangle.Height, PatBltTypes.SRCINVERT);

			Win32.SelectObject (maskDC, oldMaskBmp);
			Win32.DeleteDC (maskDC);
			Win32.DeleteObject (maskBmp);
		}

		internal static void DrawFrameControlHelper (Graphics graphics, Rectangle rectangle, uint type, uint state) {

			IntPtr targetDC = graphics.GetHdc ();
			Bitmap bmp = new Bitmap (rectangle.Width, rectangle.Height, graphics);
			Graphics g = Graphics.FromImage (bmp);
			IntPtr memDC = g.GetHdc ();

			RECT rc = new RECT();
			rc.left = 0;
			rc.top = 0;
			rc.right = rectangle.Width;
			rc.bottom = rectangle.Height;

			Color transparentColor = Color.FromArgb (0, 0, 1);
			uint oldBk = Win32.SetBkColor (memDC, (uint)Win32.RGB(transparentColor));
			Win32.ExtTextOut (memDC, 0, 0, ExtTextOutFlags.ETO_OPAQUE, ref rc, 0, 0, IntPtr.Zero);
			Win32.SetBkColor (memDC, oldBk);

			int res = Win32.DrawFrameControl( memDC, ref rc, type, state);

			CopyImageTransparent (targetDC, memDC, rectangle, transparentColor);

			g.ReleaseHdc(memDC);
			g.Dispose();
			bmp.Dispose();
			graphics.ReleaseHdc (targetDC);
		}

		public static void DrawRadioButton (Graphics graphics, Rectangle rectangle, ButtonState state) {
			DrawFrameControlHelper (graphics,  rectangle, (uint)DrawFrameControl.DFC_BUTTON, (uint)state | (uint)DrawFrameControl.DFCS_BUTTONRADIO);
		}
		
		[MonoTODO]
		public static void DrawRadioButton(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			ButtonState state) {
			DrawRadioButton(graphics, new Rectangle(x, y, width, height), state);
		}
		
		[MonoTODO]
		public static void DrawReversibleFrame(
			Rectangle rectangle,
			Color backColor,
			FrameStyle style) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawReversibleLine(
			Point start,
			Point end,
			Color backColor) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawScrollButton(
			Graphics graphics,
			Rectangle rectangle,
			ScrollButton button,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawScrollButton(
			Graphics graphics,
			int x,
			int y,
			int width,
			int height,
			ScrollButton button,
			ButtonState state) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawSelectionFrame(
			Graphics graphics,
			bool active,
			Rectangle outsideRect,
			Rectangle insideRect,
			Color backColor) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawSizeGrip(
			Graphics graphics,
			Color backColor,
			Rectangle bounds) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawSizeGrip(
			Graphics graphics,
			Color backColor,
			int x,
			int y,
			int width,
			int height) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawStringDisabled(Graphics graphics, string s, Font font, Color color, RectangleF layoutRectangle, StringFormat format) {
			Rectangle rect = new Rectangle((int)layoutRectangle.Left, (int)layoutRectangle.Top, (int)layoutRectangle.Width, (int)layoutRectangle.Height);
			RECT rc = new RECT();
			
			rect.Offset(1,1);
			rc.left = rect.Left;
			rc.top = rect.Top;
			rc.right = rect.Right;
			rc.bottom = rect.Bottom;
			
			IntPtr hdc = graphics.GetHdc();
			
			int prevColor = Win32.SetTextColor(hdc, Win32.GetSysColor(GetSysColorIndex.COLOR_3DHILIGHT));
			BackgroundMode prevBkMode = Win32.SetBkMode(hdc, BackgroundMode.TRANSPARENT);
			IntPtr prevFont = Win32.SelectObject(hdc, font.ToHfont());
			
			Win32.DrawText(hdc, s, s.Length, ref rc, Win32.StringFormat2DrawTextFormat(format));
			
			rect.Offset(-1,-1);
			rc.left = rect.Left;
			rc.top = rect.Top;
			rc.right = rect.Right;
			rc.bottom = rect.Bottom;
			Win32.SetTextColor(hdc, Win32.GetSysColor(GetSysColorIndex.COLOR_3DSHADOW));
			Win32.DrawText(hdc, s, s.Length, ref rc,  Win32.StringFormat2DrawTextFormat(format));
			
			Win32.SelectObject(hdc, prevFont);
			Win32.SetBkMode(hdc, prevBkMode);
			Win32.SetTextColor(hdc, prevColor);
			
			graphics.ReleaseHdc(hdc);
		}
		
		[MonoTODO]
		public static void FillReversibleRectangle(
			Rectangle rectangle,
			Color backColor) {
			//FIXME:
		}
		
		[MonoTODO]
		public static Color Light(Color baseColor) {
			return Light( baseColor, 10.0f);
		}
		
		[MonoTODO]
		public static Color Light(Color baseColor,float percOfLightLight) {
			double H, I, S;
			ControlPaint.Color2HIS(baseColor, out H, out I, out S);
			double NewIntensity = Math.Min( 1.0, I + (percOfLightLight / 100.0));
			return ControlPaint.HIS2Color(H, NewIntensity, S);
		}
		[MonoTODO]
		public static Color LightLight(Color baseColor) {
			return Light( baseColor, 20.0f);
		}
		#endregion
	}
}
