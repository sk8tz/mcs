//
// TextRenderer.cs
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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing.Text;

namespace System.Windows.Forms
{
#if NET_2_0
	public sealed 
#endif
	class TextRenderer
	{
		private static Bitmap measure_bitmap = new Bitmap (1, 1);

		private TextRenderer ()
		{
		}

		#region Public Methods
#if NET_2_0
		public static void DrawText (IDeviceContext dc, string text, Font font, Point pt, Color foreColor)
		{
			DrawTextInternal (dc, text, font, pt, foreColor, Color.Transparent, TextFormatFlags.Default, false);
		}

		public static void DrawText (IDeviceContext dc, string text, Font font, Rectangle bounds, Color foreColor)
		{
			DrawTextInternal (dc, text, font, bounds, foreColor, Color.Transparent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter, false);
		}

		public static void DrawText (IDeviceContext dc, string text, Font font, Point pt, Color foreColor, Color backColor)
		{
			DrawTextInternal (dc, text, font, pt, foreColor, backColor, TextFormatFlags.Default, false);
		}

		public static void DrawText (IDeviceContext dc, string text, Font font, Point pt, Color foreColor, TextFormatFlags flags)
		{
			DrawTextInternal (dc, text, font, pt, foreColor, Color.Transparent, flags, false);
		}

		public static void DrawText (IDeviceContext dc, string text, Font font, Rectangle bounds, Color foreColor, Color backColor)
		{
			DrawTextInternal (dc, text, font, bounds, foreColor, backColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter, false);
		}

		public static void DrawText (IDeviceContext dc, string text, Font font, Rectangle bounds, Color foreColor, TextFormatFlags flags)
		{
			DrawTextInternal (dc, text, font, bounds, foreColor, Color.Transparent, flags, false);
		}

		public static void DrawText (IDeviceContext dc, string text, Font font, Point pt, Color foreColor, Color backColor, TextFormatFlags flags)
		{
			DrawTextInternal (dc, text, font, pt, foreColor, backColor, flags, false);
		}

		public static void DrawText (IDeviceContext dc, string text, Font font, Rectangle bounds, Color foreColor, Color backColor, TextFormatFlags flags)
		{
			DrawTextInternal (dc, text, font, bounds, foreColor, backColor, flags, false);
		}

		public static Size MeasureText (string text, Font font)
		{
			return MeasureTextInternal (Graphics.FromImage (measure_bitmap), text, font, Size.Empty, TextFormatFlags.Default, false);
		}

		public static Size MeasureText (IDeviceContext dc, string text, Font font)
		{
			return MeasureTextInternal (dc, text, font, Size.Empty, TextFormatFlags.Default, false);
		}

		public static Size MeasureText (string text, Font font, Size proposedSize)
		{
			return MeasureTextInternal (Graphics.FromImage (measure_bitmap), text, font, proposedSize, TextFormatFlags.Default, false);
		}

		public static Size MeasureText (IDeviceContext dc, string text, Font font, Size proposedSize)
		{
			return MeasureTextInternal (dc, text, font, proposedSize, TextFormatFlags.Default, false);
		}

		public static Size MeasureText (string text, Font font, Size proposedSize, TextFormatFlags flags)
		{
			return MeasureTextInternal (Graphics.FromImage (measure_bitmap), text, font, proposedSize, flags, false);
		}

		public static Size MeasureText (IDeviceContext dc, string text, Font font, Size proposedSize, TextFormatFlags flags)
		{
			return MeasureTextInternal (dc, text, font, proposedSize, flags, false);
		}
#endif
		#endregion

		#region Internal Methods That Do Stuff
#if NET_2_0
		internal static void DrawTextInternal (IDeviceContext dc, string text, Font font, Rectangle bounds, Color foreColor, Color backColor, TextFormatFlags flags, bool useDrawString)
#else
		internal static void DrawTextInternal (Graphics dc, string text, Font font, Rectangle bounds, Color foreColor, Color backColor, TextFormatFlags flags, bool useDrawString)
#endif
		{
#if NET_2_0
			if (dc == null)
				throw new ArgumentNullException ("dc");

			if (string.IsNullOrEmpty (text))
				return;

			// We use MS GDI API's unless told not to, or we aren't on Windows
			if (!useDrawString && (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32Windows)) {
				if ((flags & TextFormatFlags.VerticalCenter) == TextFormatFlags.VerticalCenter || (flags & TextFormatFlags.Bottom) == TextFormatFlags.Bottom)
					flags |= TextFormatFlags.SingleLine;

				// Calculate the text bounds (there is often padding added)
				Rectangle new_bounds = PadRectangle (bounds, flags);
				new_bounds.Offset ((int)(dc as Graphics).Transform.OffsetX, (int)(dc as Graphics).Transform.OffsetY);

				IntPtr hdc = IntPtr.Zero;
				bool clear_clip_region = false;
				
				// If we need to use the graphics clipping region, add it to our hdc
				if ((flags & TextFormatFlags.PreserveGraphicsClipping) == TextFormatFlags.PreserveGraphicsClipping) {
					Graphics graphics = (Graphics)dc;
					Region clip_region = graphics.Clip;
					
					if (!clip_region.IsInfinite (graphics)) {
						IntPtr hrgn = clip_region.GetHrgn (graphics);
						hdc = dc.GetHdc ();
						SelectClipRgn (hdc, hrgn);
						clip_region.ReleaseHrgn (hrgn);
						clear_clip_region = true;
					}
				}
				
				if (hdc == IntPtr.Zero)
					hdc = dc.GetHdc ();
					
				// Set the fore color
				if (foreColor != Color.Empty)
					SetTextColor (hdc, ColorTranslator.ToWin32 (foreColor));

				// Set the back color
				if (backColor != Color.Transparent && backColor != Color.Empty) {
					SetBkMode (hdc, 2);	//1-Transparent, 2-Opaque
					SetBkColor (hdc, ColorTranslator.ToWin32 (backColor));
				}
				else {
					SetBkMode (hdc, 1);	//1-Transparent, 2-Opaque
				}

				XplatUIWin32.RECT r = XplatUIWin32.RECT.FromRectangle (new_bounds);

				IntPtr prevobj;

				if (font != null) {
					prevobj = SelectObject (hdc, font.ToHfont ());
					Win32DrawText (hdc, text, text.Length, ref r, (int)flags);
					prevobj = SelectObject (hdc, prevobj);
					DeleteObject (prevobj);
				}
				else {
					Win32DrawText (hdc, text, text.Length, ref r, (int)flags);
				}

				if (clear_clip_region)
					SelectClipRgn (hdc, IntPtr.Zero);

				dc.ReleaseHdc ();
			}
			// Use Graphics.DrawString as a fallback method
			else {
#endif
				Graphics g;

#if NET_2_0
				if (dc is Graphics)
					g = (Graphics)dc;
				else
					g = Graphics.FromHdc (dc.GetHdc ());
#else
					g = (Graphics)dc;

#endif

				StringFormat sf = FlagsToStringFormat (flags);

				Rectangle new_bounds = PadDrawStringRectangle (bounds, flags);

				g.DrawString (text, font, ThemeEngine.Current.ResPool.GetSolidBrush (foreColor), new_bounds, sf);

#if NET_2_0
				if (!(dc is Graphics)) {
					g.Dispose ();
					dc.ReleaseHdc ();
				}
			}
#endif
		}

#if NET_2_0
		internal static Size MeasureTextInternal (IDeviceContext dc, string text, Font font, Size proposedSize, TextFormatFlags flags, bool useMeasureString)
#else
		internal static Size MeasureTextInternal (Graphics dc, string text, Font font, Size proposedSize, TextFormatFlags flags, bool useMeasureString)
#endif
		{
#if NET_2_0
			if (!useMeasureString && (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32Windows)) {
				// Tell DrawText to calculate size instead of draw
				flags |= (TextFormatFlags)1024;		// DT_CALCRECT

				IntPtr hdc = dc.GetHdc ();

				XplatUIWin32.RECT r = XplatUIWin32.RECT.FromRectangle (new Rectangle (Point.Empty, proposedSize));

				IntPtr prevobj;

				if (font != null) {
					prevobj = SelectObject (hdc, font.ToHfont ());
					Win32DrawText (hdc, text, text.Length, ref r, (int)flags);
					prevobj = SelectObject (hdc, prevobj);
					DeleteObject (prevobj);
				}
				else {
					Win32DrawText (hdc, text, text.Length, ref r, (int)flags);
				}

				dc.ReleaseHdc ();

				// Really, I am just making something up here, which as far as I can tell, MS
				// just makes something up as well.  This will require lots of tweaking to match MS.  :(
				Size retval = r.ToRectangle ().Size;

				if (retval.Width > 0 && (flags & TextFormatFlags.NoPadding) == 0) {
					retval.Width += 6;
					retval.Width += (int)retval.Height / 8;
				}

				return retval;
			}
			else {
#endif
				Graphics g = Graphics.FromImage (measure_bitmap);
				StringFormat sf = FlagsToStringFormat (flags);

				Size retval = g.MeasureString (text, font, Int32.MaxValue, sf).ToSize ();

				if (retval.Width > 0 && (flags & TextFormatFlags.NoPadding) == 0)
					retval.Width += 9;

				return retval;
			}
#if NET_2_0
		}
#endif
		#endregion

#region Internal Methods That Are Just Overloads
#if NET_2_0
		internal static void DrawTextInternal (IDeviceContext dc, string text, Font font, Point pt, Color foreColor, bool useDrawString)
		{
			DrawTextInternal (dc, text, font, pt, foreColor, Color.Transparent, TextFormatFlags.Default, useDrawString);
		}

		internal static void DrawTextInternal (IDeviceContext dc, string text, Font font, Rectangle bounds, Color foreColor, bool useDrawString)
		{
			DrawTextInternal (dc, text, font, bounds, foreColor, Color.Transparent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter, useDrawString);
		}

		internal static void DrawTextInternal (IDeviceContext dc, string text, Font font, Point pt, Color foreColor, Color backColor, bool useDrawString)
		{
			DrawTextInternal (dc, text, font, pt, foreColor, backColor, TextFormatFlags.Default, useDrawString);
		}

		internal static void DrawTextInternal (IDeviceContext dc, string text, Font font, Point pt, Color foreColor, TextFormatFlags flags, bool useDrawString)
		{
			DrawTextInternal (dc, text, font, pt, foreColor, Color.Transparent, flags, useDrawString);
		}

		internal static void DrawTextInternal (IDeviceContext dc, string text, Font font, Rectangle bounds, Color foreColor, Color backColor, bool useDrawString)
		{
			DrawTextInternal (dc, text, font, bounds, foreColor, backColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter, useDrawString);
		}

		internal static void DrawTextInternal (IDeviceContext dc, string text, Font font, Rectangle bounds, Color foreColor, TextFormatFlags flags, bool useDrawString)
#else
		internal static void DrawTextInternal (Graphics dc, string text, Font font, Rectangle bounds, Color foreColor, TextFormatFlags flags, bool useDrawString)
#endif
		{
			DrawTextInternal (dc, text, font, bounds, foreColor, Color.Transparent, flags, useDrawString);
		}

		internal static Size MeasureTextInternal (string text, Font font, bool useMeasureString)
		{
			return MeasureTextInternal (Graphics.FromImage (measure_bitmap), text, font, Size.Empty, TextFormatFlags.Default, useMeasureString);
		}

#if NET_2_0
		internal static void DrawTextInternal (IDeviceContext dc, string text, Font font, Point pt, Color foreColor, Color backColor, TextFormatFlags flags, bool useDrawString)
		{
			Size sz = MeasureTextInternal (dc, text, font, useDrawString);
			DrawTextInternal (dc, text, font, new Rectangle (pt, sz), foreColor, backColor, flags, useDrawString);
		}

		internal static Size MeasureTextInternal (IDeviceContext dc, string text, Font font, bool useMeasureString)
		{
			return MeasureTextInternal (dc, text, font, Size.Empty, TextFormatFlags.Default, useMeasureString);
		}

		internal static Size MeasureTextInternal (string text, Font font, Size proposedSize, bool useMeasureString)
		{
			return MeasureTextInternal (Graphics.FromImage (measure_bitmap), text, font, proposedSize, TextFormatFlags.Default, useMeasureString);
		}

		internal static Size MeasureTextInternal (IDeviceContext dc, string text, Font font, Size proposedSize, bool useMeasureString)
		{
			return MeasureTextInternal (dc, text, font, proposedSize, TextFormatFlags.Default, useMeasureString);
		}

#endif
		internal static Size MeasureTextInternal (string text, Font font, Size proposedSize, TextFormatFlags flags, bool useMeasureString)
		{
			return MeasureTextInternal (Graphics.FromImage (measure_bitmap), text, font, proposedSize, flags, useMeasureString);
		}
#endregion

#region Private Methods
		private static StringFormat FlagsToStringFormat (TextFormatFlags flags)
		{
			StringFormat sf = new StringFormat ();

			// Translation table: http://msdn.microsoft.com/msdnmag/issues/06/03/TextRendering/default.aspx?fig=true#fig4

			// Horizontal Alignment
			if ((flags & TextFormatFlags.HorizontalCenter) == TextFormatFlags.HorizontalCenter)
				sf.Alignment = StringAlignment.Center;
			else if ((flags & TextFormatFlags.Right) == TextFormatFlags.Right)
				sf.Alignment = StringAlignment.Far;
			else
				sf.Alignment = StringAlignment.Near;

			// Vertical Alignment
			if ((flags & TextFormatFlags.Bottom) == TextFormatFlags.Bottom)
				sf.LineAlignment = StringAlignment.Far;
			else if ((flags & TextFormatFlags.VerticalCenter) == TextFormatFlags.VerticalCenter)
				sf.LineAlignment = StringAlignment.Center;
			else
				sf.LineAlignment = StringAlignment.Near;

			// Ellipsis
			if ((flags & TextFormatFlags.EndEllipsis) == TextFormatFlags.EndEllipsis)
				sf.Trimming = StringTrimming.EllipsisCharacter;
			else if ((flags & TextFormatFlags.PathEllipsis) == TextFormatFlags.PathEllipsis)
				sf.Trimming = StringTrimming.EllipsisPath;
			else if ((flags & TextFormatFlags.WordEllipsis) == TextFormatFlags.WordEllipsis)
				sf.Trimming = StringTrimming.EllipsisWord;
			else
				sf.Trimming = StringTrimming.Character;

			// Hotkey Prefix
			if ((flags & TextFormatFlags.NoPrefix) == TextFormatFlags.NoPrefix)
				sf.HotkeyPrefix = HotkeyPrefix.None;
			else if ((flags & TextFormatFlags.HidePrefix) == TextFormatFlags.HidePrefix)
				sf.HotkeyPrefix = HotkeyPrefix.Hide;
			else
				sf.HotkeyPrefix = HotkeyPrefix.Show;

			// Text Padding
			if ((flags & TextFormatFlags.NoPadding) == TextFormatFlags.NoPadding)
				sf.FormatFlags |= StringFormatFlags.FitBlackBox;

			// Text Wrapping
			if ((flags & TextFormatFlags.SingleLine) == TextFormatFlags.SingleLine)
				sf.FormatFlags |= StringFormatFlags.NoWrap;
			else if ((flags & TextFormatFlags.TextBoxControl) == TextFormatFlags.TextBoxControl)
				sf.FormatFlags |= StringFormatFlags.LineLimit;

			// Other Flags
			//if ((flags & TextFormatFlags.RightToLeft) == TextFormatFlags.RightToLeft)
			//        sf.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			if ((flags & TextFormatFlags.NoClipping) == TextFormatFlags.NoClipping)
				sf.FormatFlags |= StringFormatFlags.NoClip;

			if ((flags & TextFormatFlags.WordBreak) == 0)
				sf.FormatFlags |= StringFormatFlags.LineLimit;

			return sf;
		}

		private static Rectangle PadRectangle (Rectangle r, TextFormatFlags flags)
		{
			if ((flags & TextFormatFlags.NoPadding) == 0 && (flags & TextFormatFlags.Right) == 0 && (flags & TextFormatFlags.HorizontalCenter) == 0) {
				r.X += 3;
				r.Width -= 3;
			}
			if ((flags & TextFormatFlags.NoPadding) == 0 && (flags & TextFormatFlags.Right) == TextFormatFlags.Right) {
				r.Width -= 4;
			}
			if ((flags & TextFormatFlags.LeftAndRightPadding) == TextFormatFlags.LeftAndRightPadding) {
				r.X += 2;
				r.Width -= 2;
			}
			if ((flags & TextFormatFlags.WordEllipsis) == TextFormatFlags.WordEllipsis || (flags & TextFormatFlags.EndEllipsis) == TextFormatFlags.EndEllipsis || (flags & TextFormatFlags.WordBreak) == TextFormatFlags.WordBreak) {
				r.Width -= 4;
			}
			if ((flags & TextFormatFlags.VerticalCenter) == TextFormatFlags.VerticalCenter) {
				r.Y += 1;
			}

			return r;
		}

		private static Rectangle PadDrawStringRectangle (Rectangle r, TextFormatFlags flags)
		{
			if ((flags & TextFormatFlags.NoPadding) == 0 && (flags & TextFormatFlags.Right) == 0 && (flags & TextFormatFlags.HorizontalCenter) == 0) {
				r.X += 1;
				r.Width -= 1;
			}
			if ((flags & TextFormatFlags.NoPadding) == 0 && (flags & TextFormatFlags.Right) == TextFormatFlags.Right) {
				r.Width -= 4;
			}
			if ((flags & TextFormatFlags.NoPadding) == TextFormatFlags.NoPadding) {
				r.X -= 2;
			}
			if ((flags & TextFormatFlags.NoPadding) == 0 && (flags & TextFormatFlags.Bottom) == TextFormatFlags.Bottom) {
				r.Y += 1;
			}
			if ((flags & TextFormatFlags.LeftAndRightPadding) == TextFormatFlags.LeftAndRightPadding) {
				r.X += 2;
				r.Width -= 2;
			}
			if ((flags & TextFormatFlags.WordEllipsis) == TextFormatFlags.WordEllipsis || (flags & TextFormatFlags.EndEllipsis) == TextFormatFlags.EndEllipsis || (flags & TextFormatFlags.WordBreak) == TextFormatFlags.WordBreak) {
				r.Width -= 4;
			}
			if ((flags & TextFormatFlags.VerticalCenter) == TextFormatFlags.VerticalCenter) {
				r.Y += 1;
			}

			return r;
		}
#endregion

#region DllImports (Windows)
#if NET_2_0
		[DllImport ("user32", CharSet = CharSet.Unicode, EntryPoint = "DrawText")]
		static extern int Win32DrawText (IntPtr hdc, string lpStr, int nCount, ref XplatUIWin32.RECT lpRect, int wFormat);

		[DllImport ("gdi32")]
		static extern int SetTextColor (IntPtr hdc, int crColor);

		[DllImport ("gdi32")]
		static extern IntPtr SelectObject (IntPtr hDC, IntPtr hObject);

		[DllImport ("gdi32")]
		static extern int SetBkColor (IntPtr hdc, int crColor);

		[DllImport ("gdi32")]
		static extern int SetBkMode (IntPtr hdc, int iBkMode);

		[DllImport ("gdi32")]
		static extern bool DeleteObject (IntPtr objectHandle);

		[DllImport("gdi32")]
		static extern bool SelectClipRgn(IntPtr hdc, IntPtr hrgn);
#endif
#endregion
	}
}
