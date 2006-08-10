//
// Direct GDI+ API unit tests
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace MonoTests.System.Drawing {

	// copied from Mono's System.Drawing.dll gdiEnums.cs
	internal enum Status {
		Ok = 0,
		GenericError = 1,
		InvalidParameter = 2,
		OutOfMemory = 3,
		ObjectBusy = 4,
		InsufficientBuffer = 5,
		NotImplemented = 6,
		Win32Error = 7,
		WrongState = 8,
		Aborted = 9,
		FileNotFound = 10,
		ValueOverflow = 11,
		AccessDenied = 12,
		UnknownImageFormat = 13,
		FontFamilyNotFound = 14,
		FontStyleNotFound = 15,
		NotTrueTypeFont = 16,
		UnsupportedGdiplusVersion = 17,
		GdiplusNotInitialized = 18,
		PropertyNotFound = 19,
		PropertyNotSupported = 20,
		ProfileNotFound = 21
	}

	// copied from Mono's System.Drawing.dll gdiEnums.cs
	internal enum Unit {
		UnitWorld = 0,
		UnitDisplay = 1,
		UnitPixel = 2,
		UnitPoint = 3,
		UnitInch = 4,
		UnitDocument = 5,
		UnitMillimeter = 6
	}

	[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public class LOGFONT {
		public int lfHeight = 0;
		public int lfWidth = 0;
		public int lfEscapement = 0;
		public int lfOrientation = 0;
		public int lfWeight = 0;
		public byte lfItalic = 0;
		public byte lfUnderline = 0;
		public byte lfStrikeOut = 0;
		public byte lfCharSet = 0;
		public byte lfOutPrecision = 0;
		public byte lfClipPrecision = 0;
		public byte lfQuality = 0;
		public byte lfPitchAndFamily = 0;
		[MarshalAs (UnmanagedType.ByValTStr, SizeConst = 32)]
		public string lfFaceName = null;
	}

	[TestFixture]
	public class GDIPlusTest {

		// FontFamily

		[DllImport ("gdiplus.dll", CharSet=CharSet.Auto)]
		internal static extern Status GdipCreateFontFamilyFromName (
			[MarshalAs (UnmanagedType.LPWStr)] string fName, IntPtr collection, out IntPtr fontFamily);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetGenericFontFamilySerif (out IntPtr fontFamily);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipDeleteFontFamily (IntPtr fontfamily);

		[Test]
		public void DeleteFontFamily ()
		{
			// invalid image pointer (null)
			Assert.AreEqual (Status.InvalidParameter, GdipDeleteFontFamily (IntPtr.Zero), "null");

			IntPtr font_family;
			GdipCreateFontFamilyFromName ("Arial", IntPtr.Zero, out font_family);
			Assert.IsTrue (font_family != IntPtr.Zero, "GdipCreateFontFamilyFromName");
			Assert.AreEqual (Status.Ok, GdipDeleteFontFamily (font_family), "first");
		}

		[Test]
		[Category ("NotWorking")]
		public void DeleteFontFamily_DoubleDispose ()
		{
			IntPtr font_family;
			GdipGetGenericFontFamilySerif (out font_family);
			// first dispose
			Assert.AreEqual (Status.Ok, GdipDeleteFontFamily (font_family), "first");
			// second dispose
			Assert.AreEqual (Status.Ok, GdipDeleteFontFamily (font_family), "second");
		}

		// Font

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipCreateFont (IntPtr fontFamily, float emSize, FontStyle style, GraphicsUnit unit, out IntPtr font);

		[DllImport ("gdiplus.dll", CharSet = CharSet.Auto)]
		internal static extern Status GdipGetLogFont (IntPtr font, IntPtr graphics, [MarshalAs (UnmanagedType.AsAny), Out] object logfontA);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipDeleteFont (IntPtr font);		

		[Test]
		public void CreateFont ()
		{
			IntPtr family;
			GdipCreateFontFamilyFromName ("Arial", IntPtr.Zero, out family);
			Assert.IsTrue (family != IntPtr.Zero, "family");

			IntPtr font;
			Assert.AreEqual (Status.Ok, GdipCreateFont (family, 10f, FontStyle.Regular, GraphicsUnit.Point, out font), "GdipCreateFont");
			Assert.IsTrue (font != IntPtr.Zero, "font");

			LOGFONT lf = new LOGFONT ();
			lf.lfCharSet = 1;
			Assert.AreEqual (Status.InvalidParameter, GdipGetLogFont (font, IntPtr.Zero, (object) lf), "GdipGetLogFont-null-graphics");
			Assert.AreEqual (0, lf.lfCharSet, "lfCharSet-null-graphics");

			IntPtr image;
			GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			Assert.IsTrue (image != IntPtr.Zero, "image");

			IntPtr graphics;
			GdipGetImageGraphicsContext (image, out graphics);
			Assert.IsTrue (graphics != IntPtr.Zero, "graphics");

			lf.lfCharSet = 1;
			Assert.AreEqual (Status.InvalidParameter, GdipGetLogFont (IntPtr.Zero, graphics, (object) lf), "GdipGetLogFont-null");
			Assert.AreEqual (0, lf.lfCharSet, "lfCharSet-null");

			lf.lfCharSet = 1;
			Assert.AreEqual (Status.Ok, GdipGetLogFont (font, graphics, (object) lf), "GdipGetLogFont");
			Assert.AreEqual (0, lf.lfCharSet, "lfCharSet");
			// strangely this is 1 in the managed side

			Assert.AreEqual (Status.Ok, GdipDeleteFont (font), "GdipDeleteFont");
			Assert.AreEqual (Status.InvalidParameter, GdipDeleteFont (IntPtr.Zero), "GdipDeleteFont-null");

			Assert.AreEqual (Status.Ok, GdipDeleteFontFamily (family), "GdipDeleteFontFamily");
			Assert.AreEqual (Status.Ok, GdipDisposeImage (image), "GdipDisposeImage");
			Assert.AreEqual (Status.Ok, GdipDeleteGraphics (graphics), "GdipDeleteGraphics");
		}

		// Bitmap

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipCreateBitmapFromScan0 (int width, int height, int stride, PixelFormat format, IntPtr scan0, out IntPtr bmp);

		[Test]
		public void CreateBitmapFromScan0 ()
		{
			IntPtr bmp;
			Assert.AreEqual (Status.InvalidParameter, GdipCreateBitmapFromScan0 (-1, 10, 10, PixelFormat.Format32bppArgb, IntPtr.Zero, out bmp), "negative width");
		}

		// Brush

		[DllImport ("gdiplus.dll")]
		static internal extern Status GdipDeleteBrush (IntPtr brush);

		[Test]
		public void DeleteBrush ()
		{
			Assert.AreEqual (Status.InvalidParameter, GdipDeleteBrush (IntPtr.Zero), "GdipDeleteBrush");
		}

		// Graphics

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetImageGraphicsContext (IntPtr image, out IntPtr graphics);

		[DllImport ("gdiplus.dll")]
		static internal extern Status GdipDeleteGraphics (IntPtr graphics);

		[Test]
		public void Graphics ()
		{
			IntPtr graphics;
			Assert.AreEqual (Status.InvalidParameter, GdipGetImageGraphicsContext (IntPtr.Zero, out graphics), "GdipGetImageGraphicsContext");

			IntPtr image;
			GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			Assert.IsTrue (image != IntPtr.Zero, "image");

			Assert.AreEqual (Status.Ok, GdipGetImageGraphicsContext (image, out graphics), "GdipGetImageGraphicsContext");
			Assert.IsTrue (graphics != IntPtr.Zero, "graphics");

			Assert.AreEqual (Status.Ok, GdipDeleteGraphics (graphics), "GdipDeleteGraphics");
			Assert.AreEqual (Status.InvalidParameter, GdipDeleteGraphics (IntPtr.Zero), "GdipDeleteGraphics-null");

			Assert.AreEqual (Status.Ok, GdipDisposeImage (image), "GdipDisposeImage");
		}

		// GraphicsPath

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipCreatePath (FillMode brushMode, out IntPtr path);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetPointCount (IntPtr path, out int count);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetPathPoints (IntPtr path, [Out] PointF[] points, int count);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetPathTypes (IntPtr path, [Out] byte[] types, int count);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipResetPath (IntPtr path);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipAddPathLine (IntPtr path, float x1, float y1, float x2, float y2);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipAddPathLine2 (IntPtr path, PointF[] points, int count);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipWidenPath (IntPtr path, IntPtr pen, IntPtr matrix, float flatness);

		[DllImport ("gdiplus.dll")]                
		internal static extern Status GdipDeletePath (IntPtr path);

		[Test]
		public void GetPointCount_Zero ()
		{
			IntPtr path;
			Assert.AreEqual (Status.Ok, GdipCreatePath (FillMode.Alternate, out path), "GdipCreatePath");
			Assert.IsTrue (path != IntPtr.Zero, "Handle");

			int count;
			Assert.AreEqual (Status.InvalidParameter, GdipGetPointCount (IntPtr.Zero, out count), "GdipGetPointCount-null");
			Assert.AreEqual (Status.Ok, GdipGetPointCount (path, out count), "GdipGetPointCount");
			Assert.AreEqual (0, count, "Count");

			PointF[] points = new PointF[count];
			Assert.AreEqual (Status.InvalidParameter, GdipGetPathPoints (IntPtr.Zero, points, count), "GdipGetPathPoints-null-1");
			Assert.AreEqual (Status.InvalidParameter, GdipGetPathPoints (path, null, count), "GdipGetPathPoints-null-2");
			Assert.AreEqual (Status.InvalidParameter, GdipGetPathPoints (path, points, count), "GdipGetPathPoints");
			// can't get the points if the count is zero!

			byte[] types = new byte[count];
			Assert.AreEqual (Status.InvalidParameter, GdipGetPathTypes (IntPtr.Zero, types, count), "GdipGetPathTypes-null-1");
			Assert.AreEqual (Status.InvalidParameter, GdipGetPathTypes (path, null, count), "GdipGetPathTypes-null-2");
			Assert.AreEqual (Status.InvalidParameter, GdipGetPathTypes (path, types, count), "GdipGetPathTypes");
			// can't get the types if the count is zero!

			PointF[] pts_2f = new PointF[2] { new PointF (2f, 4f), new PointF (10f, 30f) };
			Assert.AreEqual (Status.InvalidParameter, GdipAddPathLine2 (IntPtr.Zero, pts_2f, pts_2f.Length), "GdipAddPathLine2-null-path");
			Assert.AreEqual (Status.InvalidParameter, GdipAddPathLine2 (path, null, pts_2f.Length), "GdipAddPathLine2-null-points");
			Assert.AreEqual (Status.InvalidParameter, GdipAddPathLine2 (path, pts_2f, -1), "GdipAddPathLine2-negative-count");
			Assert.AreEqual (Status.Ok, GdipAddPathLine2 (path, pts_2f, pts_2f.Length), "GdipAddPathLine2");

			Assert.AreEqual (Status.Ok, GdipGetPointCount (path, out count), "GdipGetPointCount");
			Assert.AreEqual (2, count, "Count");

			points = new PointF[count];
			Assert.AreEqual (Status.Ok, GdipGetPathPoints (path, points, count), "GdipGetPathPoints-ok");

			types = new byte[count];
			Assert.AreEqual (Status.Ok, GdipGetPathTypes (path, types, count), "GdipGetPathTypes-ok");

			Assert.AreEqual (Status.Ok, GdipResetPath (path), "GdipResetPath");
			Assert.AreEqual (Status.InvalidParameter, GdipResetPath (IntPtr.Zero), "GdipResetPath-null");

			Assert.AreEqual (Status.Ok, GdipDeletePath (path), "GdipDeletePath");
			Assert.AreEqual (Status.InvalidParameter, GdipDeletePath (IntPtr.Zero), "GdipDeletePath-null");
		}

		[Test]
		public void Widen ()
		{
			IntPtr pen;
			Assert.AreEqual (Status.Ok, GdipCreatePen1 (0, 0f, Unit.UnitWorld, out pen), "GdipCreatePen1");

			IntPtr path;
			Assert.AreEqual (Status.Ok, GdipCreatePath (FillMode.Alternate, out path), "GdipCreatePath");

			IntPtr matrix;
			Assert.AreEqual (Status.Ok, GdipCreateMatrix (out matrix), "GdipCreateMatrix");

			Assert.AreEqual (Status.InvalidParameter, GdipWidenPath (IntPtr.Zero, pen, matrix, 1.0f), "GdipWidenPath-null-path");
			// empty path
			Assert.AreEqual (Status.OutOfMemory, GdipWidenPath (path, pen, matrix, 1.0f), "GdipWidenPath");

			// add something to the path
			Assert.AreEqual (Status.InvalidParameter, GdipAddPathLine (IntPtr.Zero, 1, 1, 10, 10), "GdipAddPathLine");
			Assert.AreEqual (Status.Ok, GdipAddPathLine (path, 1, 1, 10, 10), "GdipAddPathLine");

			int count;
			Assert.AreEqual (Status.Ok, GdipGetPointCount (path, out count), "GdipGetPointCount");
			Assert.AreEqual (2, count, "Count");

			Assert.AreEqual (Status.Ok, GdipWidenPath (path, pen, matrix, 1.0f), "GdipWidenPath-2");

			Assert.AreEqual (Status.Ok, GdipDeleteMatrix (matrix), "GdipDeleteMatrix");
			Assert.AreEqual (Status.Ok, GdipDeletePath (path), "GdipDeletePath");
			Assert.AreEqual (Status.Ok, GdipDeletePen (pen), "GdipDeletePen");
		}

		// Matrix

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipCreateMatrix (out IntPtr matrix);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipDeleteMatrix (IntPtr matrix);

		[Test]
		public void Matrix ()
		{
			IntPtr matrix;
			Assert.AreEqual (Status.Ok, GdipCreateMatrix (out matrix), "GdipCreateMatrix");
			Assert.IsTrue (matrix != IntPtr.Zero, "Handle");

			Assert.AreEqual (Status.InvalidParameter, GdipDeleteMatrix (IntPtr.Zero), "GdipDeleteMatrix-null");
			Assert.AreEqual (Status.Ok, GdipDeleteMatrix (matrix), "GdipDeleteMatrix");
		}

		// Image

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipDisposeImage (IntPtr image);

		[Test]
		public void DisposeImage ()
		{
			// invalid image pointer (null)
			Assert.AreEqual (Status.InvalidParameter, GdipDisposeImage (IntPtr.Zero), "null");

			IntPtr image;
			GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			Assert.AreEqual (Status.Ok, GdipDisposeImage (image), "first");
		}

		[Test]
		[Category ("NotWorking")]
		public void DisposeImage_Dual ()
		{
			IntPtr image;
			GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			// first dispose
			Assert.AreEqual (Status.Ok, GdipDisposeImage (image), "first");
			// second dispose
			Assert.AreEqual (Status.ObjectBusy, GdipDisposeImage (image), "second");
		}


		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetImageThumbnail (IntPtr image, uint width, uint height, out IntPtr thumbImage, IntPtr callback, IntPtr callBackData);

		[Test]
		[Category ("NotWorking")] // libgdiplus doesn't implement GdipGetImageThumbnail (it is done inside S.D)
		public void GetImageThumbnail ()
		{
			IntPtr ptr;

			// invalid image pointer (null)
			Assert.AreEqual (Status.InvalidParameter, GdipGetImageThumbnail (IntPtr.Zero, 10, 10, out ptr, IntPtr.Zero, IntPtr.Zero));

			IntPtr image;
			GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			try {
				// invalid width (0)
				Assert.AreEqual (Status.OutOfMemory, GdipGetImageThumbnail (image, 0, 10, out ptr, IntPtr.Zero, IntPtr.Zero));
				// invalid width (negative)
				Assert.AreEqual (Status.OutOfMemory, GdipGetImageThumbnail (image, 0x8000000, 10, out ptr, IntPtr.Zero, IntPtr.Zero));
				// invalid height (0)
				Assert.AreEqual (Status.OutOfMemory, GdipGetImageThumbnail (image, 10, 0, out ptr, IntPtr.Zero, IntPtr.Zero));
				// invalid height (negative)
				Assert.AreEqual (Status.OutOfMemory, GdipGetImageThumbnail (image, 10, 0x8000000, out ptr, IntPtr.Zero, IntPtr.Zero));
			}
			finally {
				GdipDisposeImage (image);
			}
		}

		// PathGradientBrush

		[DllImport ("gdiplus.dll")]
		static internal extern Status GdipCreatePathGradient (PointF[] points, int count, WrapMode wrapMode, out IntPtr brush);

		[DllImport ("gdiplus.dll")]
		static internal extern Status GdipCreatePathGradientFromPath (IntPtr path, out IntPtr brush);

		[DllImport ("gdiplus.dll")]
		static internal extern Status GdipGetPathGradientBlendCount (IntPtr brush, out int count);

		[DllImport ("gdiplus.dll")]
		static internal extern Status GdipGetPathGradientPresetBlend (IntPtr brush, int[] blend, float[] positions, int count);


		[Test]
		public void CreatePathGradient ()
		{
			PointF[] points = null;
			IntPtr brush;
			Assert.AreEqual (Status.OutOfMemory, GdipCreatePathGradient (points, 0, WrapMode.Clamp, out brush), "null");

			points = new PointF [0];
			Assert.AreEqual (Status.OutOfMemory, GdipCreatePathGradient (points, 0, WrapMode.Clamp, out brush), "empty");

			points = new PointF[1];
			Assert.AreEqual (Status.OutOfMemory, GdipCreatePathGradient (points, 1, WrapMode.Clamp, out brush), "one");

			points = new PointF[2] { new PointF (1, 2), new PointF (20, 30) };
			Assert.AreEqual (Status.Ok, GdipCreatePathGradient (points, 2, WrapMode.Clamp, out brush), "two");
			Assert.IsTrue (brush != IntPtr.Zero, "Handle");

			int count;
			Assert.AreEqual (Status.Ok, GdipGetPathGradientBlendCount (brush, out count), "GdipGetPathGradientBlendCount");
			Assert.AreEqual (1, count, "blend count");

			int[] colors = new int[count];
			float[] positions = new float[count];
			Assert.AreEqual (Status.InvalidParameter, GdipGetPathGradientPresetBlend (brush, colors, positions, count), "GdipGetPathGradientBlend");
			// can't call that for 1 count!

			Assert.AreEqual (Status.Ok, GdipDeleteBrush (brush), "GdipDeleteBrush");
		}

		[Test]
		public void CreatePathGradient_FromPath_Line ()
		{
			IntPtr path;
			Assert.AreEqual (Status.Ok, GdipCreatePath (FillMode.Alternate, out path), "GdipCreatePath");

			IntPtr brush;
			Assert.AreEqual (Status.OutOfMemory, GdipCreatePathGradientFromPath (IntPtr.Zero, out brush), "null");
			Assert.AreEqual (Status.OutOfMemory, GdipCreatePathGradientFromPath (path, out brush), "empty path");

			Assert.AreEqual (Status.Ok, GdipAddPathLine (path, 1, 1, 10, 10), "GdipAddPathLine");

			Assert.AreEqual (Status.Ok, GdipCreatePathGradientFromPath (path, out brush), "path");
			Assert.IsTrue (brush != IntPtr.Zero, "Handle");

			int count;
			Assert.AreEqual (Status.Ok, GdipGetPathGradientBlendCount (brush, out count), "GdipGetPathGradientBlendCount");
			Assert.AreEqual (1, count, "blend count");

			int[] colors = new int[count];
			float[] positions = new float[count];
			Assert.AreEqual (Status.InvalidParameter, GdipGetPathGradientPresetBlend (brush, colors, positions, count), "GdipGetPathGradientBlend");

			Assert.AreEqual (Status.Ok, GdipDeleteBrush (brush), "GdipDeleteBrush");
			Assert.AreEqual (Status.Ok, GdipDeletePath (path), "GdipDeletePath");
		}

		[Test]
		public void CreatePathGradient_FromPath_Lines ()
		{
			IntPtr path;
			Assert.AreEqual (Status.Ok, GdipCreatePath (FillMode.Alternate, out path), "GdipCreatePath");

			PointF[] pts_2f = new PointF[2] { new PointF (2f, 4f), new PointF (10f, 30f) };
			Assert.AreEqual (Status.Ok, GdipAddPathLine2 (path, pts_2f, pts_2f.Length), "GdipAddPathLine2");

			IntPtr brush;
			Assert.AreEqual (Status.Ok, GdipCreatePathGradientFromPath (path, out brush), "path");
			Assert.IsTrue (brush != IntPtr.Zero, "Handle");

			int count;
			Assert.AreEqual (Status.Ok, GdipGetPathGradientBlendCount (brush, out count), "GdipGetPathGradientBlendCount");
			Assert.AreEqual (1, count, "blend count");

			int[] colors = new int[count];
			float[] positions = new float[count];
			Assert.AreEqual (Status.InvalidParameter, GdipGetPathGradientPresetBlend (brush, colors, positions, count), "GdipGetPathGradientBlend");

			Assert.AreEqual (Status.Ok, GdipDeleteBrush (brush), "GdipDeleteBrush");
			Assert.AreEqual (Status.Ok, GdipDeletePath (path), "GdipDeletePath");
		}

		// Pen

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipCreatePen1 (int argb, float width, Unit unit, out IntPtr pen);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetPenDashStyle (IntPtr pen, out DashStyle dashStyle);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipSetPenDashStyle (IntPtr pen, DashStyle dashStyle);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetPenDashCount (IntPtr pen, out int count);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetPenDashArray (IntPtr pen, float[] dash, int count);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipDeletePen (IntPtr pen);

		[Test]
		public void CreatePen ()
		{
			IntPtr pen;
			Assert.AreEqual (Status.Ok, GdipCreatePen1 (0, 0f, Unit.UnitWorld, out pen), "GdipCreatePen1");
			Assert.IsTrue (pen != IntPtr.Zero, "pen");

			DashStyle ds;
			Assert.AreEqual (Status.Ok, GdipGetPenDashStyle (pen, out ds), "GdipGetPenDashStyle");
			Assert.AreEqual (Status.InvalidParameter, GdipGetPenDashStyle (IntPtr.Zero, out ds), "GdipGetPenDashStyle-null");

			ds = DashStyle.Custom;
			Assert.AreEqual (Status.Ok, GdipSetPenDashStyle (pen, ds), "GdipSetPenDashStyle");
			Assert.AreEqual (Status.InvalidParameter, GdipSetPenDashStyle (IntPtr.Zero, ds), "GdipSetPenDashStyle-null");

			int count;
			Assert.AreEqual (Status.Ok, GdipGetPenDashCount (pen, out count), "GdipGetPenDashCount");
			Assert.AreEqual (Status.InvalidParameter, GdipGetPenDashCount (IntPtr.Zero, out count), "GdipGetPenDashCount-null");
			Assert.AreEqual (0, count, "count");

			float[] dash = new float[count];
			Assert.AreEqual (Status.OutOfMemory, GdipGetPenDashArray (pen, dash, count), "GdipGetPenDashArray");
			Assert.AreEqual (Status.InvalidParameter, GdipGetPenDashArray (IntPtr.Zero, dash, count), "GdipGetPenDashArray-null-pen");
			Assert.AreEqual (Status.InvalidParameter, GdipGetPenDashArray (pen, null, count), "GdipGetPenDashArray-null-dash");

			Assert.AreEqual (Status.Ok, GdipDeletePen (pen), "GdipDeletePen");
			Assert.AreEqual (Status.InvalidParameter, GdipDeletePen (IntPtr.Zero), "GdipDeletePen-null");
		}

		// Region

		[DllImport ("gdiplus.dll")]
		static internal extern Status GdipCreateRegionRgnData (byte[] data, int size, out IntPtr region);

		[Test]
		public void CreateRegionRgnData ()
		{
			IntPtr region;
			Assert.AreEqual (Status.InvalidParameter, GdipCreateRegionRgnData (null, 0, out region));

			byte[] data = new byte[0];
			Assert.AreEqual (Status.GenericError, GdipCreateRegionRgnData (data, 0, out region));
		}
	}
}
