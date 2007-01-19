//
// Direct GDI+ API unit tests
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006-2007 Novell, Inc (http://www.novell.com)
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
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace MonoTests.System.Drawing {

	[TestFixture]
	public class GDIPlusTest {

		// for the moment this LOGFONT is different (and ok) from the one defined internally in SD
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

		// FontFamily
		[Test]
		public void DeleteFontFamily ()
		{
			// invalid image pointer (null)
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDeleteFontFamily (IntPtr.Zero), "null");

			IntPtr font_family;
			GDIPlus.GdipCreateFontFamilyFromName ("Arial", IntPtr.Zero, out font_family);
			if (font_family != IntPtr.Zero)
				Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteFontFamily (font_family), "first");
			else
				Assert.Ignore ("Arial isn't available on this platform");
		}

		[Test]
		[Category ("NotWorking")]
		public void DeleteFontFamily_DoubleDispose ()
		{
			IntPtr font_family;
			GDIPlus.GdipGetGenericFontFamilySerif (out font_family);
			// first dispose
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteFontFamily (font_family), "first");
			// second dispose
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteFontFamily (font_family), "second");
		}

		// Font
		[Test]
		public void CreateFont ()
		{
			IntPtr family;
			GDIPlus.GdipCreateFontFamilyFromName ("Arial", IntPtr.Zero, out family);
			if (family == IntPtr.Zero)
				Assert.Ignore ("Arial isn't available on this platform");

			IntPtr font;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateFont (family, 10f, FontStyle.Regular, GraphicsUnit.Point, out font), "GdipCreateFont");
			Assert.IsTrue (font != IntPtr.Zero, "font");

			LOGFONT lf = new LOGFONT ();
			lf.lfCharSet = 1;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetLogFont (font, IntPtr.Zero, (object) lf), "GdipGetLogFont-null-graphics");
			Assert.AreEqual (0, lf.lfCharSet, "lfCharSet-null-graphics");
			// other lf members looks like garbage

			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			Assert.IsTrue (image != IntPtr.Zero, "image");

			IntPtr graphics;
			GDIPlus.GdipGetImageGraphicsContext (image, out graphics);
			Assert.IsTrue (graphics != IntPtr.Zero, "graphics");

			lf.lfCharSet = 1;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetLogFont (IntPtr.Zero, graphics, (object) lf), "GdipGetLogFont-null");
			Assert.AreEqual (0, lf.lfCharSet, "lfCharSet-null");

			lf.lfCharSet = 1;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetLogFont (font, graphics, (object) lf), "GdipGetLogFont");
			Assert.AreEqual (0, lf.lfCharSet, "lfCharSet");
			// strangely this is 1 in the managed side

			lf.lfCharSet = 2;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetLogFont (font, graphics, (object) lf), "GdipGetLogFont-2");
			Assert.AreEqual (0, lf.lfCharSet, "lfCharSet");
			// strangely this is 2 in the managed side

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteFont (font), "GdipDeleteFont");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDeleteFont (IntPtr.Zero), "GdipDeleteFont-null");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteFontFamily (family), "GdipDeleteFontFamily");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image), "GdipDisposeImage");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteGraphics (graphics), "GdipDeleteGraphics");
		}

		// Bitmap
		[Test]
		public void CreateBitmapFromScan0 ()
		{
			IntPtr bmp;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipCreateBitmapFromScan0 (-1, 10, 10, PixelFormat.Format32bppArgb, IntPtr.Zero, out bmp), "negative width");
		}

		[Test]
		public void Unlock ()
		{
			IntPtr bmp;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out bmp);
			Assert.IsTrue (bmp != IntPtr.Zero, "bmp");

			BitmapData bd = null;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipBitmapUnlockBits (bmp, bd), "BitmapData");

			bd = new BitmapData ();
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipBitmapUnlockBits (IntPtr.Zero, bd), "handle");

			Assert.AreEqual (Status.Win32Error, GDIPlus.GdipBitmapUnlockBits (bmp, bd), "not locked");

			Rectangle rect = new Rectangle (2, 2, 5, 5);
			Assert.AreEqual (Status.Ok, GDIPlus.GdipBitmapLockBits (bmp, ref rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb, bd), "locked");
			Assert.AreEqual (rect.Width, bd.Width, "Width");
			Assert.AreEqual (rect.Height, bd.Height, "Height");
			Assert.AreEqual (PixelFormat.Format24bppRgb, bd.PixelFormat, "PixelFormat");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipBitmapUnlockBits (bmp, bd), "unlocked");

			Assert.AreEqual (Status.Win32Error, GDIPlus.GdipBitmapUnlockBits (bmp, bd), "unlocked-twice");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (bmp), "GdipDisposeImage");
		}

		// Brush
		[Test]
		public void DeleteBrush ()
		{
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDeleteBrush (IntPtr.Zero), "GdipDeleteBrush");
		}

		// Graphics
		[Test]
		public void Graphics ()
		{
			IntPtr graphics;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetImageGraphicsContext (IntPtr.Zero, out graphics), "GdipGetImageGraphicsContext");

			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			Assert.IsTrue (image != IntPtr.Zero, "image");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImageGraphicsContext (image, out graphics), "GdipGetImageGraphicsContext");
			Assert.IsTrue (graphics != IntPtr.Zero, "graphics");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteGraphics (graphics), "GdipDeleteGraphics");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDeleteGraphics (IntPtr.Zero), "GdipDeleteGraphics-null");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image), "GdipDisposeImage");
		}

		[Test]
		public void MeasureCharacterRanges ()
		{
			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			Assert.IsTrue (image != IntPtr.Zero, "image");

			IntPtr graphics;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImageGraphicsContext (image, out graphics), "GdipGetImageGraphicsContext");
			Assert.IsTrue (graphics != IntPtr.Zero, "graphics");

			IntPtr family;
			GDIPlus.GdipCreateFontFamilyFromName ("Arial", IntPtr.Zero, out family);
			if (family == IntPtr.Zero)
				Assert.Ignore ("Arial isn't available on this platform");

			IntPtr font;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateFont (family, 10f, FontStyle.Regular, GraphicsUnit.Point, out font), "GdipCreateFont");
			Assert.IsTrue (font != IntPtr.Zero, "font");

			RectangleF layout = new RectangleF ();
			IntPtr[] regions = new IntPtr[1];
			IntPtr format;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipStringFormatGetGenericDefault (out format), "GdipStringFormatGetGenericDefault");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipMeasureCharacterRanges (IntPtr.Zero, "a", 1, font, ref layout, format, 1, out regions[0]), "GdipMeasureCharacterRanges-null");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipMeasureCharacterRanges (graphics, null, 0, font, ref layout, format, 1, out regions[0]), "GdipMeasureCharacterRanges-null string");

			int count;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatMeasurableCharacterRangeCount (format, out count), "GdipGetStringFormatMeasurableCharacterRangeCount");
			Assert.AreEqual (0, count, "count");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipMeasureCharacterRanges (graphics, "a", 1, font, ref layout, format, 1, out regions[0]), "GdipMeasureCharacterRanges");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteStringFormat (format), "GdipDeleteStringFormat");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteGraphics (graphics), "GdipDeleteGraphics");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteFont (font), "GdipDeleteFont");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteFontFamily (family), "GdipDeleteFontFamily");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image), "GdipDisposeImage");
		}

		// GraphicsPath
		[Test]
		public void GetPointCount_Zero ()
		{
			IntPtr path;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePath (FillMode.Alternate, out path), "GdipCreatePath");
			Assert.IsTrue (path != IntPtr.Zero, "Handle");

			int count;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPointCount (IntPtr.Zero, out count), "GdipGetPointCount-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPointCount (path, out count), "GdipGetPointCount");
			Assert.AreEqual (0, count, "Count");

			PointF[] points = new PointF[count];
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPathPoints (IntPtr.Zero, points, count), "GdipGetPathPoints-null-1");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPathPoints (path, null, count), "GdipGetPathPoints-null-2");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPathPoints (path, points, count), "GdipGetPathPoints");
			// can't get the points if the count is zero!

			byte[] types = new byte[count];
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPathTypes (IntPtr.Zero, types, count), "GdipGetPathTypes-null-1");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPathTypes (path, null, count), "GdipGetPathTypes-null-2");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPathTypes (path, types, count), "GdipGetPathTypes");
			// can't get the types if the count is zero!

			PointF[] pts_2f = new PointF[2] { new PointF (2f, 4f), new PointF (10f, 30f) };
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipAddPathLine2 (IntPtr.Zero, pts_2f, pts_2f.Length), "GdipAddPathLine2-null-path");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipAddPathLine2 (path, null, pts_2f.Length), "GdipAddPathLine2-null-points");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipAddPathLine2 (path, pts_2f, -1), "GdipAddPathLine2-negative-count");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipAddPathLine2 (path, pts_2f, pts_2f.Length), "GdipAddPathLine2");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPointCount (path, out count), "GdipGetPointCount");
			Assert.AreEqual (2, count, "Count");

			points = new PointF[count];
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPathPoints (path, points, count), "GdipGetPathPoints-ok");

			types = new byte[count];
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPathTypes (path, types, count), "GdipGetPathTypes-ok");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipResetPath (path), "GdipResetPath");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipResetPath (IntPtr.Zero), "GdipResetPath-null");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePath (path), "GdipDeletePath");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDeletePath (IntPtr.Zero), "GdipDeletePath-null");
		}

		[Test]
		public void Widen ()
		{
			IntPtr pen;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePen1 (0, 0f, Unit.UnitWorld, out pen), "GdipCreatePen1");

			IntPtr path;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePath (FillMode.Alternate, out path), "GdipCreatePath");

			IntPtr matrix;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateMatrix (out matrix), "GdipCreateMatrix");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipWidenPath (IntPtr.Zero, pen, matrix, 1.0f), "GdipWidenPath-null-path");
			// empty path
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipWidenPath (path, pen, matrix, 1.0f), "GdipWidenPath");

			// add something to the path
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipAddPathLine (IntPtr.Zero, 1, 1, 10, 10), "GdipAddPathLine");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipAddPathLine (path, 1, 1, 10, 10), "GdipAddPathLine");

			int count;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPointCount (path, out count), "GdipGetPointCount");
			Assert.AreEqual (2, count, "Count");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipWidenPath (path, pen, matrix, 1.0f), "GdipWidenPath-2");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteMatrix (matrix), "GdipDeleteMatrix");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePath (path), "GdipDeletePath");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePen (pen), "GdipDeletePen");
		}

		// GraphicsPathIterator
		[Test]
		public void GraphicsPathIterator ()
		{
			IntPtr path;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePath (FillMode.Alternate, out path), "GdipCreatePath");
			
			IntPtr iter;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePathIter (out iter, path), "GdipCreatePathIter");

			int count = -1;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterGetCount (IntPtr.Zero, out count), "GdipPathIterGetCount-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterGetCount (iter, out count), "GdipPathIterGetCount");
			Assert.AreEqual (0, count, "count-1");

			count = -1;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterGetSubpathCount (IntPtr.Zero, out count), "GdipPathIterGetSubpathCount-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterGetSubpathCount (iter, out count), "GdipPathIterGetSubpathCount");
			Assert.AreEqual (0, count, "count-2");

			bool curve;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterHasCurve (IntPtr.Zero, out curve), "GdipPathIterHasCurve-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterHasCurve (iter, out curve), "GdipPathIterHasCurve");
			Assert.IsFalse (curve, "curve");

			int result;
			PointF[] points = new PointF[count];
			byte[] types = new byte[count];
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterEnumerate (IntPtr.Zero, out result, points, types, count), "GdipPathIterEnumerate-iter");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterEnumerate (iter, out result, null, types, count), "GdipPathIterEnumerate-points");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterEnumerate (iter, out result, points, null, count), "GdipPathIterEnumerate-types");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterEnumerate (iter, out result, points, types, -1), "GdipPathIterEnumerate-count");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterEnumerate (iter, out result, points, types, count), "GdipPathIterEnumerate");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterCopyData (IntPtr.Zero, out result, points, types, 0, 0), "GdipPathIterCopyData-iter");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterCopyData (iter, out result, null, types, 0, 0), "GdipPathIterCopyData-points");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterCopyData (iter, out result, points, null, 0, 0), "GdipPathIterCopyData-types");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterCopyData (iter, out result, points, types, -1, 0), "GdipPathIterCopyData-start");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterCopyData (iter, out result, points, types, 0, -1), "GdipPathIterCopyData-end");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterCopyData (iter, out result, points, types, 0, 0), "GdipPathIterCopyData");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterNextMarkerPath (IntPtr.Zero, out result, path), "GdipPathIterNextMarkerPath-iter");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterNextMarkerPath (iter, out result, IntPtr.Zero), "GdipPathIterNextMarkerPath-path");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterNextMarkerPath (iter, out result, path), "GdipPathIterNextMarkerPath");

			result = -1;
			int start = -1;
			int end = -1;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterNextMarker (IntPtr.Zero, out result, out start, out end), "GdipPathIterNextMarker-iter");
			start = -1;
			end = -1;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterNextMarker (iter, out result, out start, out end), "GdipPathIterNextMarker");
			Assert.AreEqual (0, result, "result-4");
			Assert.AreEqual (-1, start, "start-1");
			Assert.AreEqual (-1, end, "end-1");

			byte pathType = 255;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterNextPathType (IntPtr.Zero, out result, out pathType, out start, out end), "GdipPathIterNextPathType-iter");
			pathType = 255;
			start = -1;
			end = -1;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterNextPathType (iter, out result, out pathType, out start, out end), "GdipPathIterNextPathType");
			Assert.AreEqual (0, result, "result-5");
			Assert.AreEqual (255, pathType, "pathType");
			Assert.AreEqual (-1, start, "start-2");
			Assert.AreEqual (-1, end, "end-2");

			bool closed = false;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterNextSubpathPath (IntPtr.Zero, out result, IntPtr.Zero, out closed), "GdipPathIterNextSubpathPath-iter");
			closed = false;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterNextSubpathPath (iter, out result, IntPtr.Zero, out closed), "GdipPathIterNextSubpathPath-path");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterNextSubpathPath (iter, out result, path, out closed), "GdipPathIterNextSubpathPath");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterNextSubpath (IntPtr.Zero, out result, out start, out end, out closed), "GdipPathIterNextSubpath-iter");
			start = -1;
			end = -1;
			closed = false;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterNextSubpath (iter, out result, out start, out end, out closed), "GdipPathIterNextSubpath");
			Assert.AreEqual (-1, start, "start-3");
			Assert.AreEqual (-1, end, "end-3");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipPathIterRewind (IntPtr.Zero), "GdipPathIterRewind-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterRewind (iter), "GdipPathIterRewind");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDeletePathIter (IntPtr.Zero), "GdipDeletePathIter-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePathIter (iter), "GdipDeletePathIter");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePath (path), "GdipDeletePath");
		}

		[Test]
		public void GraphicsPathIterator_WithoutPath ()
		{
			// a path isn't required to create an iterator - ensure we never crash for any api
			IntPtr iter;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePathIter (out iter, IntPtr.Zero), "GdipCreatePathIter-null");

			int count = -1;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterGetCount (iter, out count), "GdipPathIterGetCount");
			Assert.AreEqual (0, count, "count-1");

			count = -1;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterGetSubpathCount (iter, out count), "GdipPathIterGetSubpathCount");
			Assert.AreEqual (0, count, "count-2");

			bool curve;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterHasCurve (iter, out curve), "GdipPathIterHasCurve");

			int result = -1;
			PointF[] points = new PointF[count];
			byte[] types = new byte[count];
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterEnumerate (iter, out result, points, types, count), "GdipPathIterEnumerate");
			Assert.AreEqual (0, result, "result-1");

			result = -1;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterCopyData (iter, out result, points, types, 0, 0), "GdipPathIterCopyData");
			Assert.AreEqual (0, result, "result-2");

			result = -1;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterNextMarkerPath (iter, out result, IntPtr.Zero), "GdipPathIterNextMarkerPath");
			Assert.AreEqual (0, result, "result-3");

			result = -1;
			int start = -1;
			int end = -1;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterNextMarker (iter, out result, out start, out end), "GdipPathIterNextMarker");
			Assert.AreEqual (0, result, "result-4");
			Assert.AreEqual (-1, start, "start-1");
			Assert.AreEqual (-1, end, "end-1");

			result = -1;
			byte pathType = 255;
			start = -1;
			end = -1;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterNextPathType (iter, out result, out pathType, out start, out end), "GdipPathIterNextPathType");
			Assert.AreEqual (0, result, "result-5");
			Assert.AreEqual (255, pathType, "pathType");
			Assert.AreEqual (-1, start, "start-2");
			Assert.AreEqual (-1, end, "end-2");

			bool closed = false;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterNextSubpathPath (iter, out result, IntPtr.Zero, out closed), "GdipPathIterNextSubpathPath");

			start = -1;
			end = -1;
			closed = false;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterNextSubpath (iter, out result, out start, out end, out closed), "GdipPathIterNextSubpath");
			Assert.AreEqual (-1, start, "start-3");
			Assert.AreEqual (-1, end, "end-3");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipPathIterRewind (iter), "GdipPathIterRewind");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePathIter (iter), "GdipDeletePathIter");
		}

		// Matrix
		[Test]
		public void Matrix ()
		{
			IntPtr matrix;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateMatrix (out matrix), "GdipCreateMatrix");
			Assert.IsTrue (matrix != IntPtr.Zero, "Handle");

			bool result;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipIsMatrixIdentity (IntPtr.Zero, out result), "GdipIsMatrixIdentity-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipIsMatrixIdentity (matrix, out result), "GdipIsMatrixIdentity");
			Assert.IsTrue (result, "identity");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipIsMatrixInvertible (IntPtr.Zero, out result), "GdipIsMatrixInvertible-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipIsMatrixInvertible (matrix, out result), "GdipIsMatrixInvertible");
			Assert.IsTrue (result, "invertible");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipInvertMatrix (IntPtr.Zero), "GdipInvertMatrix-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipInvertMatrix (matrix), "GdipInvertMatrix");

			PointF[] points = new PointF[1];
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipTransformMatrixPoints (IntPtr.Zero, points, 1), "GdipTransformMatrixPoints-null-points-1");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipTransformMatrixPoints (matrix, null, 1), "GdipTransformMatrixPoints-matrix-points-1");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipTransformMatrixPoints (matrix, points, 0), "GdipTransformMatrixPoints-matrix-points-0");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipTransformMatrixPoints (matrix, points, 1), "GdipTransformMatrixPoints");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipVectorTransformMatrixPoints (IntPtr.Zero, points, 1), "GdipVectorTransformMatrixPoints-null-points-1");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipVectorTransformMatrixPoints (matrix, null, 1), "GdipVectorTransformMatrixPoints-matrix-points-1");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipVectorTransformMatrixPoints (matrix, points, 0), "GdipVectorTransformMatrixPoints-matrix-points-0");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipVectorTransformMatrixPoints (matrix, points, 1), "GdipVectorTransformMatrixPoints");

			IntPtr clone;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipCloneMatrix (IntPtr.Zero, out clone), "GdipCloneMatrix");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCloneMatrix (matrix, out clone), "GdipCloneMatrix");
			Assert.IsTrue (clone != IntPtr.Zero, "Handle-clone");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipIsMatrixEqual (IntPtr.Zero, clone, out result), "GdipIsMatrixEqual-null-clone");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipIsMatrixEqual (matrix, IntPtr.Zero, out result), "GdipIsMatrixEqual-matrix-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipIsMatrixEqual (matrix, clone, out result), "GdipIsMatrixEqual-matrix-clone");
			Assert.IsTrue (result, "equal");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipMultiplyMatrix (IntPtr.Zero, clone, MatrixOrder.Append), "GdipMultiplyMatrix-null-clone-append");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipMultiplyMatrix (matrix, IntPtr.Zero, MatrixOrder.Prepend), "GdipMultiplyMatrix-matrix-null-prepend");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipMultiplyMatrix (matrix, clone, (MatrixOrder)Int32.MinValue), "GdipMultiplyMatrix-matrix-clone-invalid");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipMultiplyMatrix (matrix, clone, MatrixOrder.Append), "GdipMultiplyMatrix-matrix-clone-append");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipTranslateMatrix (IntPtr.Zero, 1f, 2f, MatrixOrder.Append), "GdipTranslateMatrix-null-append");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipTranslateMatrix (matrix, Single.NaN, Single.NegativeInfinity, MatrixOrder.Prepend), "GdipTranslateMatrix-nan-append");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipTranslateMatrix (matrix, 1f, 2f, (MatrixOrder) Int32.MinValue), "GdipTranslateMatrix-matrix-invalid");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipTranslateMatrix (matrix, 1f, 2f, MatrixOrder.Append), "GdipTranslateMatrix-matrix-append");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipScaleMatrix (IntPtr.Zero, 1f, 2f, MatrixOrder.Append), "GdipScaleMatrix-null-append");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipScaleMatrix (matrix, Single.NaN, Single.NegativeInfinity, MatrixOrder.Prepend), "GdipScaleMatrix-nan-append");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipScaleMatrix (matrix, 1f, 2f, (MatrixOrder) Int32.MinValue), "GdipScaleMatrix-matrix-invalid");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipScaleMatrix (matrix, 1f, 2f, MatrixOrder.Append), "GdipScaleMatrix-matrix-append");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipShearMatrix (IntPtr.Zero, 1f, 2f, MatrixOrder.Append), "GdipShearMatrix-null-append");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipShearMatrix (matrix, Single.NaN, Single.NegativeInfinity, MatrixOrder.Prepend), "GdipShearMatrix-nan-append");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipShearMatrix (matrix, 1f, 2f, (MatrixOrder) Int32.MinValue), "GdipShearMatrix-matrix-invalid");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipShearMatrix (matrix, 1f, 2f, MatrixOrder.Append), "GdipShearMatrix-matrix-append");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDeleteMatrix (IntPtr.Zero), "GdipDeleteMatrix-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteMatrix (matrix), "GdipDeleteMatrix");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteMatrix (clone), "GdipDeleteMatrix-clone");
		}

		[Test]
		public void Matrix2 ()
		{
			IntPtr matrix;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateMatrix2 (Single.MinValue, Single.MaxValue, Single.NegativeInfinity, 
				Single.PositiveInfinity, Single.NaN, Single.Epsilon, out matrix), "GdipCreateMatrix2");
			Assert.IsTrue (matrix != IntPtr.Zero, "Handle");

			// check data
			float[] elements = new float[6];
			IntPtr data = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (float)) * 6);
			try {
				Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetMatrixElements (IntPtr.Zero, data), "GdipSetMatrixElements-null-matrix");
				Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetMatrixElements (matrix, IntPtr.Zero), "GdipSetMatrixElements-null-data");
				Assert.AreEqual (Status.Ok, GDIPlus.GdipGetMatrixElements (matrix, data), "GdipSetMatrixElements-null-matrix");
				Marshal.Copy (data, elements, 0, 6);
				Assert.AreEqual (Single.MinValue, elements [0], "0");
				Assert.AreEqual (Single.MaxValue, elements [1], "1");
				Assert.AreEqual (Single.NegativeInfinity, elements [2], "2");
				Assert.AreEqual (Single.PositiveInfinity, elements [3], "3");
				Assert.AreEqual (Single.NaN, elements [4], "4");
				Assert.AreEqual (Single.Epsilon, elements [5], "5");
			}
			finally {
				Marshal.FreeHGlobal (data);
			}

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetMatrixElements (IntPtr.Zero, 0f, 0f, 0f, 0f, 0f, 0f), "GdipSetMatrixElements-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipSetMatrixElements (matrix, 0f, 0f, 0f, 0f, 0f, 0f), "GdipSetMatrixElements");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteMatrix (matrix), "GdipDeleteMatrix");
		}

		[Test]
		public void Matrix3 ()
		{
			RectangleF rect = new RectangleF ();
			IntPtr matrix;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipCreateMatrix3 (ref rect, null, out matrix), "GdipCreateMatrix3-null");

			// provding less than 3 points would results in AccessViolationException under MS 2.0 but can't happen using the managed SD code
			PointF[] points = new PointF[3];
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateMatrix3 (ref rect, points, out matrix), "GdipCreateMatrix3-empty-rect");
			rect = new RectangleF (10f, 20f, 0f, 40f);
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateMatrix3 (ref rect, points, out matrix), "GdipCreateMatrix3-empty-width");
			rect = new RectangleF (10f, 20f, 30f, 0f);
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateMatrix3 (ref rect, points, out matrix), "GdipCreateMatrix3-empty-height");

			rect = new RectangleF (0f, 0f, 30f, 40f);
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateMatrix3 (ref rect, points, out matrix), "GdipCreateMatrix3-3");
			Assert.IsTrue (matrix != IntPtr.Zero, "Handle");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteMatrix (matrix), "GdipDeleteMatrix");
		}

		// Image
		[Test]
		public void DisposeImage ()
		{
			// invalid image pointer (null)
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDisposeImage (IntPtr.Zero), "null");

			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image), "first");
		}

		[Test]
		[Category ("NotWorking")]
		public void DisposeImage_Dual ()
		{
			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			// first dispose
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image), "first");
			// second dispose
			Assert.AreEqual (Status.ObjectBusy, GDIPlus.GdipDisposeImage (image), "second");
		}

		[Test]
		[Category ("NotWorking")] // libgdiplus doesn't implement GdipGetImageThumbnail (it is done inside S.D)
		public void GetImageThumbnail ()
		{
			IntPtr ptr;

			// invalid image pointer (null)
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetImageThumbnail (IntPtr.Zero, 10, 10, out ptr, IntPtr.Zero, IntPtr.Zero));

			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);
			try {
				// invalid width (0)
				Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipGetImageThumbnail (image, 0, 10, out ptr, IntPtr.Zero, IntPtr.Zero));
				// invalid width (negative)
				Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipGetImageThumbnail (image, 0x8000000, 10, out ptr, IntPtr.Zero, IntPtr.Zero));
				// invalid height (0)
				Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipGetImageThumbnail (image, 10, 0, out ptr, IntPtr.Zero, IntPtr.Zero));
				// invalid height (negative)
				Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipGetImageThumbnail (image, 10, 0x8000000, out ptr, IntPtr.Zero, IntPtr.Zero));
			}
			finally {
				GDIPlus.GdipDisposeImage (image);
			}
		}

		[Test]
		public void Icon ()
		{
			string filename = TestBitmap.getInFile ("bitmaps/64x64x256.ico");
			IntPtr bitmap;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateBitmapFromFile (filename, out bitmap), "GdipCreateBitmapFromFile");
			try {
				int size;
				Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImagePaletteSize (bitmap, out size), "GdipGetImagePaletteSize");
				Assert.AreEqual (1032, size, "size");

				IntPtr clone;
				Assert.AreEqual (Status.Ok, GDIPlus.GdipCloneImage (bitmap, out clone), "GdipCloneImage");
				try {
					Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImagePaletteSize (clone, out size), "GdipGetImagePaletteSize/Clone");
					Assert.AreEqual (1032, size, "size/clone");
				}
				finally {
					GDIPlus.GdipDisposeImage (clone);
				}

				IntPtr palette = Marshal.AllocHGlobal (size);
				try {

					Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetImagePalette (IntPtr.Zero, palette, size), "GdipGetImagePalette(null,palette,size)");
					Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetImagePalette (bitmap, IntPtr.Zero, size), "GdipGetImagePalette(bitmap,null,size)");
					Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetImagePalette (bitmap, palette, 0), "GdipGetImagePalette(bitmap,palette,0)");
					Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImagePalette (bitmap, palette, size), "GdipGetImagePalette");

					Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetImagePalette (IntPtr.Zero, palette), "GdipSetImagePalette(null,palette)");
					Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetImagePalette (bitmap, IntPtr.Zero), "GdipSetImagePalette(bitmap,null)");
					Assert.AreEqual (Status.Ok, GDIPlus.GdipSetImagePalette (bitmap, palette), "GdipSetImagePalette");

					// change palette to 0 entries
					int flags = Marshal.ReadInt32 (palette);
					Marshal.WriteInt64 (palette, flags << 32);
					Assert.AreEqual (Status.Ok, GDIPlus.GdipSetImagePalette (bitmap, palette), "GdipSetImagePalette/Empty");

					Assert.AreEqual (Status.Ok, GDIPlus.GdipGetImagePaletteSize (bitmap, out size), "GdipGetImagePaletteSize/Empty");
					Assert.AreEqual (8, size, "size");
				}
				finally {
					Marshal.FreeHGlobal (palette);
				}
			}
			finally {
				GDIPlus.GdipDisposeImage (bitmap);
			}
		}

		[Test]
		public void FromFile_IndexedBitmap ()
		{
			// despite it's name it's a 4bpp indexed bitmap
			string filename = TestBitmap.getInFile ("bitmaps/almogaver1bit.bmp");
			IntPtr graphics;

			IntPtr image;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipLoadImageFromFile (filename, out image), "GdipLoadImageFromFile");
			try {
				Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipGetImageGraphicsContext (image, out graphics), "GdipGetImageGraphicsContext/image");
				Assert.AreEqual (IntPtr.Zero, graphics, "image/graphics");
			}
			finally {
				GDIPlus.GdipDisposeImage (image);
			}

			IntPtr bitmap;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateBitmapFromFile (filename, out bitmap), "GdipCreateBitmapFromFile");
			try {
				Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipGetImageGraphicsContext (bitmap, out graphics), "GdipGetImageGraphicsContext/bitmap");
				Assert.AreEqual (IntPtr.Zero, graphics, "bitmap/graphics");
			}
			finally {
				GDIPlus.GdipDisposeImage (bitmap);
			}
		}

		[Test]
		[ExpectedException (typeof (FileNotFoundException))]
		public void GdipLoadImageFromFile_FileNotFound ()
		{
			string filename = "filenotfound";

			IntPtr image;
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipLoadImageFromFile (filename, out image), "GdipLoadImageFromFile");
			Assert.AreEqual (IntPtr.Zero, image, "image handle");

			// this doesn't throw a OutOfMemoryException
			Image.FromFile (filename);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GdipCreateBitmapFromFile_FileNotFound ()
		{
			string filename = "filenotfound";

			IntPtr bitmap;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipCreateBitmapFromFile (filename, out bitmap), "GdipCreateBitmapFromFile");
			Assert.AreEqual (IntPtr.Zero, bitmap, "bitmap handle");

			new Bitmap (filename);
		}

		[Test]
		public void Encoder ()
		{
			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);

			Guid g = new Guid ();
			uint size = UInt32.MaxValue;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetEncoderParameterListSize (IntPtr.Zero, ref g, out size), "GdipGetEncoderParameterListSize-null-guid-uint");
			Assert.AreEqual (UInt32.MaxValue, size, "size-1");
			// note: can't test a null Guid (it's a struct)
#if false
			Assert.AreEqual (Status. FileNotFound, GDIPlus.GdipGetEncoderParameterListSize (image, ref g, out size), "GdipGetEncoderParameterListSize-image-badguid-uint");
			Assert.AreEqual (UInt32.MaxValue, size, "size-2");

			g = new Guid ("{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetEncoderParameterListSize (image, ref g, out size), "GdipGetEncoderParameterListSize-image-guid-uint");
			Assert.AreEqual (UInt32.MaxValue, size, "size-3");
#endif
			GDIPlus.GdipDisposeImage (image);
		}

		// ImageAttribute
		[Test]
		public void ImageAttribute ()
		{
			IntPtr attr;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateImageAttributes (out attr), "GdipCreateImageAttributes");

			IntPtr clone;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipCloneImageAttributes (IntPtr.Zero, out clone), "GdipCloneImageAttributes");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCloneImageAttributes (attr, out clone), "GdipCloneImageAttributes");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetImageAttributesColorMatrix (attr, ColorAdjustType.Default, true, IntPtr.Zero, IntPtr.Zero, ColorMatrixFlag.Default), "GdipSetImageAttributesColorMatrix-true-matrix1");
			// the first color matrix can be null if enableFlag is false
			Assert.AreEqual (Status.Ok, GDIPlus.GdipSetImageAttributesColorMatrix (attr, ColorAdjustType.Default, false, IntPtr.Zero, IntPtr.Zero, ColorMatrixFlag.Default), "GdipSetImageAttributesColorMatrix-false-matrix1");
			ColorMatrix cm = new ColorMatrix ();
			IntPtr color = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (ColorMatrix)));
			try {
				Marshal.StructureToPtr (cm, color, false);
				Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetImageAttributesColorMatrix (IntPtr.Zero, ColorAdjustType.Default, true, color, IntPtr.Zero, ColorMatrixFlag.Default), "GdipSetImageAttributesColorMatrix-null");
				Assert.AreEqual (Status.Ok, GDIPlus.GdipSetImageAttributesColorMatrix (attr, ColorAdjustType.Default, true, color, IntPtr.Zero, ColorMatrixFlag.Default), "GdipCloneImageAttributes");
			}
			finally {
				Marshal.FreeHGlobal (color);
			}

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDisposeImageAttributes (IntPtr.Zero), "GdipDisposeImageAttributes-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImageAttributes (attr), "GdipDisposeImageAttributes");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImageAttributes (clone), "GdipDisposeImageAttributes-clone");
		}

		// PathGradientBrush
		[Test]
		public void CreatePathGradient ()
		{
			PointF[] points = null;
			IntPtr brush;
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreatePathGradient (points, 0, WrapMode.Clamp, out brush), "null");

			points = new PointF [0];
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreatePathGradient (points, 0, WrapMode.Clamp, out brush), "empty");

			points = new PointF[1];
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreatePathGradient (points, 1, WrapMode.Clamp, out brush), "one");

			points = null;
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreatePathGradient (points, 2, WrapMode.Clamp, out brush), "null/two");

			points = new PointF[2] { new PointF (1, 2), new PointF (20, 30) };
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePathGradient (points, 2, WrapMode.Clamp, out brush), "two");
			Assert.IsTrue (brush != IntPtr.Zero, "Handle");

			int count;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPathGradientBlendCount (brush, out count), "GdipGetPathGradientBlendCount");
			Assert.AreEqual (1, count, "blend count");

			int[] colors = new int[count];
			float[] positions = new float[count];
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPathGradientPresetBlend (brush, colors, positions, count), "GdipGetPathGradientBlend");
			// can't call that for 1 count!

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteBrush (brush), "GdipDeleteBrush");
		}

		[Test]
		public void CreatePathGradient_FromPath_Line ()
		{
			IntPtr path;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePath (FillMode.Alternate, out path), "GdipCreatePath");

			IntPtr brush;
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreatePathGradientFromPath (IntPtr.Zero, out brush), "null");
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreatePathGradientFromPath (path, out brush), "empty path");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipAddPathLine (path, 1, 1, 10, 10), "GdipAddPathLine");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePathGradientFromPath (path, out brush), "path");
			Assert.IsTrue (brush != IntPtr.Zero, "Handle");

			int count;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPathGradientBlendCount (brush, out count), "GdipGetPathGradientBlendCount");
			Assert.AreEqual (1, count, "blend count");

			int[] colors = new int[count];
			float[] positions = new float[count];
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPathGradientPresetBlend (brush, colors, positions, count), "GdipGetPathGradientBlend");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteBrush (brush), "GdipDeleteBrush");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePath (path), "GdipDeletePath");
		}

		[Test]
		public void CreatePathGradient_FromPath_Lines ()
		{
			IntPtr path;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePath (FillMode.Alternate, out path), "GdipCreatePath");

			PointF[] pts_2f = new PointF[2] { new PointF (2f, 4f), new PointF (10f, 30f) };
			Assert.AreEqual (Status.Ok, GDIPlus.GdipAddPathLine2 (path, pts_2f, pts_2f.Length), "GdipAddPathLine2");

			IntPtr brush;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePathGradientFromPath (path, out brush), "path");
			Assert.IsTrue (brush != IntPtr.Zero, "Handle");

			int count;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPathGradientBlendCount (brush, out count), "GdipGetPathGradientBlendCount");
			Assert.AreEqual (1, count, "blend count");

			int[] colors = new int[count];
			float[] positions = new float[count];
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPathGradientPresetBlend (brush, colors, positions, count), "GdipGetPathGradientBlend");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteBrush (brush), "GdipDeleteBrush");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePath (path), "GdipDeletePath");
		}

		// Pen
		[Test]
		public void CreatePen ()
		{
			IntPtr pen;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreatePen1 (0, 0f, Unit.UnitWorld, out pen), "GdipCreatePen1");
			Assert.IsTrue (pen != IntPtr.Zero, "pen");

			DashStyle ds;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPenDashStyle (pen, out ds), "GdipGetPenDashStyle");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPenDashStyle (IntPtr.Zero, out ds), "GdipGetPenDashStyle-null");

			ds = DashStyle.Custom;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipSetPenDashStyle (pen, ds), "GdipSetPenDashStyle");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetPenDashStyle (IntPtr.Zero, ds), "GdipSetPenDashStyle-null");

			int count;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetPenDashCount (pen, out count), "GdipGetPenDashCount");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPenDashCount (IntPtr.Zero, out count), "GdipGetPenDashCount-null");
			Assert.AreEqual (0, count, "count");

			float[] dash = new float[count];
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipGetPenDashArray (pen, dash, count), "GdipGetPenDashArray");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPenDashArray (IntPtr.Zero, dash, count), "GdipGetPenDashArray-null-pen");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetPenDashArray (pen, null, count), "GdipGetPenDashArray-null-dash");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeletePen (pen), "GdipDeletePen");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDeletePen (IntPtr.Zero), "GdipDeletePen-null");
		}

		// Region
		[Test]
		public void CreateRegionRgnData ()
		{
			IntPtr region;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipCreateRegionRgnData (null, 0, out region));

			byte[] data = new byte[0];
			Assert.AreEqual (Status.GenericError, GDIPlus.GdipCreateRegionRgnData (data, 0, out region));
		}

		[Test]
		public void DrawingOperations ()
		{
			IntPtr graphics, image;

			IntPtr pen;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero,
							   out image);

			GDIPlus.GdipGetImageGraphicsContext (image, out graphics);
			GDIPlus.GdipCreatePen1 (0, 0f, Unit.UnitWorld, out pen);

			// DrawCurve

			Assert.AreEqual (Status.InvalidParameter,
					 GDIPlus.GdipDrawCurveI (IntPtr.Zero, IntPtr.Zero, null, 0));

			Assert.AreEqual (Status.InvalidParameter, 
					 GDIPlus.GdipDrawCurveI (graphics, pen, new Point [] {}, 0),
					 "DrawCurve with no pts");
			Assert.AreEqual (Status.InvalidParameter,
					 GDIPlus.GdipDrawCurveI (graphics, pen,
								 new Point [] { new Point (1, 1) }, 1),
					 "DrawCurve with 1 pt");
			Assert.AreEqual (Status.Ok,
					 GDIPlus.GdipDrawCurveI (graphics, pen,
								 new Point [] { new Point (1, 1),
										new Point (2, 2) }, 2),
					 "DrawCurve with 2 pts");

			// DrawClosedCurve

			Assert.AreEqual (Status.InvalidParameter, 
					 GDIPlus.GdipDrawClosedCurveI (graphics, pen, new Point [] {}, 0),
					 "DrawClosedCurve with no pts");
			Assert.AreEqual (Status.InvalidParameter,
					 GDIPlus.GdipDrawClosedCurveI (graphics, pen,
								       new Point [] { new Point (1, 1) }, 1),
					 "DrawClosedCurve with 1 pt");
			Assert.AreEqual (Status.InvalidParameter,
					 GDIPlus.GdipDrawClosedCurveI (graphics, pen,
								       new Point [] { new Point (1, 1),
										      new Point (2, 2) }, 2),
					 "DrawClosedCurve with 2 pt2");

			// DrawPolygon

			Assert.AreEqual (Status.InvalidParameter,
					 GDIPlus.GdipDrawPolygonI (graphics, pen, new Point [] {}, 0),
					 "DrawPolygon with no pts");
			Assert.AreEqual (Status.InvalidParameter,
					 GDIPlus.GdipDrawPolygonI (graphics, pen,
								   new Point [] { new Point (1, 1) }, 1),
					 "DrawPolygon with only one pt");

			GDIPlus.GdipDeletePen (pen);			

			// FillClosedCurve

			IntPtr brush;
			GDIPlus.GdipCreateSolidFill (0, out brush);


			Assert.AreEqual (Status.InvalidParameter,
					 GDIPlus.GdipFillClosedCurveI (graphics, brush, new Point [] {}, 0),
					 "FillClosedCurve with no pts");
			Assert.AreEqual (Status.Ok,
					 GDIPlus.GdipFillClosedCurveI (graphics, brush, 
								       new Point [] { new Point (1, 1) }, 1),
					 "FillClosedCurve with 1 pt");
			Assert.AreEqual (Status.Ok,
					 GDIPlus.GdipFillClosedCurveI (graphics, brush,
								       new Point [] { new Point (1, 1),
										      new Point (2, 2) }, 2),
					 "FillClosedCurve with 2 pts");
			
			GDIPlus.GdipDeleteBrush (brush);
			
			GDIPlus.GdipDeleteGraphics (graphics);
			GDIPlus.GdipDisposeImage (image);
		}

		// StringFormat
		private void CheckStringFormat (IntPtr sf, StringFormatFlags exepcted_flags, StringTrimming expected_trimmings)
		{
			StringAlignment sa = StringAlignment.Center;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetStringFormatAlign (IntPtr.Zero, out sa), "GdipGetStringFormatAlign-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatAlign (sf, out sa), "GdipGetStringFormatAlign");
			Assert.AreEqual (StringAlignment.Near, sa, "StringAlignment-1");

			StringAlignment la = StringAlignment.Center;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetStringFormatLineAlign (IntPtr.Zero, out la), "GdipGetStringFormatLineAlign-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatLineAlign (sf, out la), "GdipGetStringFormatLineAlign");
			Assert.AreEqual (StringAlignment.Near, la, "StringAlignment-2");

			StringFormatFlags flags = StringFormatFlags.DirectionRightToLeft;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetStringFormatFlags (IntPtr.Zero, out flags), "GdipGetStringFormatFlags-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatFlags (sf, out flags), "GdipGetStringFormatFlags");
			Assert.AreEqual (exepcted_flags, flags, "StringFormatFlags");

			HotkeyPrefix hotkey = HotkeyPrefix.Show;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetStringFormatHotkeyPrefix (IntPtr.Zero, out hotkey), "GdipGetStringFormatHotkeyPrefix-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatHotkeyPrefix (sf, out hotkey), "GdipGetStringFormatHotkeyPrefix");
			Assert.AreEqual (HotkeyPrefix.None, hotkey, "HotkeyPrefix");

			StringTrimming trimming = StringTrimming.None;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetStringFormatTrimming (IntPtr.Zero, out trimming), "GdipGetStringFormatTrimming-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatTrimming (sf, out trimming), "GdipGetStringFormatTrimming");
			Assert.AreEqual (expected_trimmings, trimming, "StringTrimming");

			StringDigitSubstitute sub = StringDigitSubstitute.Traditional;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetStringFormatDigitSubstitution (IntPtr.Zero, 0, out sub), "GdipGetStringFormatDigitSubstitution-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatDigitSubstitution (sf, 0, out sub), "GdipGetStringFormatDigitSubstitution");
			Assert.AreEqual (StringDigitSubstitute.User, sub, "StringDigitSubstitute");

			int count;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetStringFormatMeasurableCharacterRangeCount (IntPtr.Zero, out count), "GdipGetStringFormatMeasurableCharacterRangeCount-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatMeasurableCharacterRangeCount (sf, out count), "GdipGetStringFormatMeasurableCharacterRangeCount");
			Assert.AreEqual (0, count, "count");
		}

		[Test]
		public void StringFormat ()
		{
			IntPtr sf;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateStringFormat (Int32.MinValue, Int32.MinValue, out sf), "GdipCreateStringFormat");

			CheckStringFormat (sf, (StringFormatFlags) Int32.MinValue, StringTrimming.Character);

			CharacterRange[] ranges = new CharacterRange[32];
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetStringFormatMeasurableCharacterRanges (IntPtr.Zero, 1, ranges), "GdipSetStringFormatMeasurableCharacterRanges-null");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetStringFormatMeasurableCharacterRanges (IntPtr.Zero, -1, ranges), "GdipSetStringFormatMeasurableCharacterRanges-negative");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipSetStringFormatMeasurableCharacterRanges (sf, 1, ranges), "GdipSetStringFormatMeasurableCharacterRanges");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipSetStringFormatMeasurableCharacterRanges (sf, 32, ranges), "GdipSetStringFormatMeasurableCharacterRanges-32");
			Assert.AreEqual (Status.ValueOverflow, GDIPlus.GdipSetStringFormatMeasurableCharacterRanges (sf, 33, ranges), "GdipSetStringFormatMeasurableCharacterRanges-33");

			float first = Single.MinValue;
			float[] tabs = new float[1];
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetStringFormatTabStops (IntPtr.Zero, 1.0f, 1, tabs), "GdipSetStringFormatTabStops-null");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetStringFormatTabStops (sf, 1.0f, 1, null), "GdipSetStringFormatTabStops-null/tabs");
			
			Assert.AreEqual (Status.Ok, GDIPlus.GdipSetStringFormatTabStops (sf, 1.0f, -1, tabs), "GdipSetStringFormatTabStops-negative");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatTabStops (sf, 1, out first, tabs), "GdipGetStringFormatTabStops-negative");
			Assert.AreEqual (0.0f, first, "first-negative");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipSetStringFormatTabStops (sf, 1.0f, 1, tabs), "GdipSetStringFormatTabStops");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetStringFormatTabStops (sf, 1, out first, tabs), "GdipGetStringFormatTabStops");
			Assert.AreEqual (1.0f, first, "first");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipDeleteStringFormat (IntPtr.Zero), "GdipDeleteStringFormat-null");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteStringFormat (sf), "GdipDeleteStringFormat");
		}

		[Test]
		public void StringFormat_Clone ()
		{
			IntPtr sf;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateStringFormat (Int32.MinValue, Int32.MinValue, out sf), "GdipCreateStringFormat");

			IntPtr clone;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipCloneStringFormat (IntPtr.Zero, out clone), "GdipCloneStringFormat");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCloneStringFormat (sf, out clone), "GdipCloneStringFormat");

			CheckStringFormat (clone, (StringFormatFlags) Int32.MinValue, StringTrimming.Character);

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteStringFormat (clone), "GdipDeleteStringFormat-clone");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteStringFormat (sf), "GdipDeleteStringFormat");
		}

		[Test]
		public void StringFormat_GenericDefault ()
		{
			IntPtr sf;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipStringFormatGetGenericDefault (out sf), "GdipStringFormatGetGenericDefault");

			CheckStringFormat (sf, (StringFormatFlags) 0, StringTrimming.Character);

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteStringFormat (sf), "GdipDeleteStringFormat");
		}

		[Test]
		public void StringFormat_GenericTypographic ()
		{
			IntPtr sf;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipStringFormatGetGenericTypographic (out sf), "GdipStringFormatGetGenericTypographic");

			StringFormatFlags flags = StringFormatFlags.NoClip | StringFormatFlags.LineLimit | StringFormatFlags.FitBlackBox;
			CheckStringFormat (sf, flags , StringTrimming.None);

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteStringFormat (sf), "GdipDeleteStringFormat");
		}

		// TextureBrush
		[Test]
		public void Texture ()
		{
			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);

			IntPtr brush;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipCreateTexture (IntPtr.Zero, WrapMode.Tile, out brush), "GdipCreateTexture-image");
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateTexture (image, (WrapMode)Int32.MinValue, out brush), "GdipCreateTexture-wrapmode");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateTexture (image, WrapMode.Tile, out brush), "GdipCreateTexture");

			IntPtr image2;
// this would throw an AccessViolationException under MS 2.0 (missing null check?)
//			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetTextureImage (IntPtr.Zero, out image2), "GdipGetTextureImage-brush");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetTextureImage (brush, out image2), "GdipGetTextureImage");
			Assert.IsFalse (image == image2, "image");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteBrush (brush), "GdipDeleteBrush");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image), "GdipDisposeImage");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image2), "GdipDisposeImage-image2");
		}

		[Test]
		public void Texture2 ()
		{
			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);

			IntPtr brush;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipCreateTexture2 (IntPtr.Zero, WrapMode.Tile, 0, 0, 10, 10, out brush), "GdipCreateTexture2-image");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipCreateTexture2 (IntPtr.Zero, (WrapMode) Int32.MinValue, 0, 0, 10, 10, out brush), "GdipCreateTexture2-wrapmode");
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateTexture2 (image, WrapMode.Tile, 0, 0, 0, 10, out brush), "GdipCreateTexture2-width");
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateTexture2 (image, WrapMode.Tile, 0, 0, 10, 0, out brush), "GdipCreateTexture2-height");
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateTexture2 (image, WrapMode.Tile, -1, 0, 0, 10, out brush), "GdipCreateTexture2-x");
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateTexture2 (image, WrapMode.Tile, 0, -1, 10, 0, out brush), "GdipCreateTexture2-y");
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateTexture2 (image, WrapMode.Tile, 1, 0, 10, 10, out brush), "GdipCreateTexture2-too-wide");
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateTexture2 (image, WrapMode.Tile, 0, 1, 10, 10, out brush), "GdipCreateTexture2-too-tall");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateTexture2 (image, WrapMode.Tile, 0, 0, 10, 10, out brush), "GdipCreateTexture2");

			WrapMode wm;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetTextureWrapMode (IntPtr.Zero, out wm), "GdipGetTextureWrapMode-brush");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetTextureWrapMode (brush, out wm), "GdipGetTextureWrapMode");
			Assert.AreEqual (WrapMode.Tile, wm, "WrapMode");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetTextureWrapMode (IntPtr.Zero, WrapMode.Clamp), "GdipSetTextureWrapMode-brush");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipSetTextureWrapMode (brush, WrapMode.Clamp), "GdipSetTextureWrapMode");
			GDIPlus.GdipGetTextureWrapMode (brush, out wm);
			Assert.AreEqual (WrapMode.Clamp, wm, "WrapMode.Clamp");

			// an invalid WrapMode is ignored
			Assert.AreEqual (Status.Ok, GDIPlus.GdipSetTextureWrapMode (brush, (WrapMode) Int32.MinValue), "GdipSetTextureWrapMode-wrapmode");
			GDIPlus.GdipGetTextureWrapMode (brush, out wm);
			Assert.AreEqual (WrapMode.Clamp, wm, "WrapMode/Invalid");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteBrush (brush), "GdipDeleteBrush");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image), "GdipDisposeImage");
		}

		[Test]
		public void TextureIA ()
		{
			IntPtr image;
			GDIPlus.GdipCreateBitmapFromScan0 (10, 10, 0, PixelFormat.Format32bppArgb, IntPtr.Zero, out image);

			IntPtr brush;
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipCreateTextureIA (IntPtr.Zero, IntPtr.Zero, 0, 0, 10, 10, out brush), "GdipCreateTexture2-image");
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateTextureIA (image, IntPtr.Zero, 0, 0, 0, 10, out brush), "GdipCreateTexture2-width");
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateTextureIA (image, IntPtr.Zero, 0, 0, 10, 0, out brush), "GdipCreateTexture2-height");
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateTextureIA (image, IntPtr.Zero, -1, 0, 10, 10, out brush), "GdipCreateTexture2-x");
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateTextureIA (image, IntPtr.Zero, 0, -1, 10, 10, out brush), "GdipCreateTexture2-y");
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateTextureIA (image, IntPtr.Zero, 1, 0, 10, 10, out brush), "GdipCreateTexture2-too-wide");
			Assert.AreEqual (Status.OutOfMemory, GDIPlus.GdipCreateTextureIA (image, IntPtr.Zero, 0, 1, 10, 10, out brush), "GdipCreateTexture2-too-tall");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateTextureIA (image, IntPtr.Zero, 0, 0, 10, 10, out brush), "GdipCreateTexture2");

			// TODO - handle ImageAttribute in the tests

			IntPtr matrix;
			Assert.AreEqual (Status.Ok, GDIPlus.GdipCreateMatrix (out matrix), "GdipCreateMatrix");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetTextureTransform (IntPtr.Zero, matrix), "GdipGetTextureTransform-brush");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipGetTextureTransform (brush, IntPtr.Zero), "GdipGetTextureTransform-matrix");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipGetTextureTransform (brush, matrix), "GdipGetTextureTransform");

			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetTextureTransform (IntPtr.Zero, matrix), "GdipSetTextureTransform-brush");
			Assert.AreEqual (Status.InvalidParameter, GDIPlus.GdipSetTextureTransform (brush, IntPtr.Zero), "GdipSetTextureTransform-matrix");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipSetTextureTransform (brush, matrix), "GdipSetTextureTransform");

			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteMatrix (matrix), "GdipDeleteMatrix");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDeleteBrush (brush), "GdipDeleteBrush");
			Assert.AreEqual (Status.Ok, GDIPlus.GdipDisposeImage (image), "GdipDisposeImage");
		}
	}
}
