//
// System.Drawing.Graphics.cs
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com) (stubbed out)
//      Alexandre Pigolkine(pigolkine@gmx.de)
//		Jordi Mas i Hernandez (jordi@ximian.com)
//
using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Drawing
{
	[ComVisible(false)]
	public sealed class Graphics : MarshalByRefObject, IDisposable
	{
		internal IntPtr nativeObject = IntPtr.Zero;

		private static float defDpiX = 0;
		private static float defDpiY = 0;
	
		[ComVisible(false)]
		public delegate bool EnumerateMetafileProc (EmfPlusRecordType recordType,
							    int flags,
							    int dataSize,
							    IntPtr data,
							    PlayRecordCallback callbackData);
		
		[ComVisible (false)]
		public delegate bool DrawImageAbort (IntPtr callbackData);		
		
		private Graphics (IntPtr nativeGraphics)
		{
			if (nativeGraphics == IntPtr.Zero)
				Console.WriteLine ("Here: " + Environment.StackTrace);
			nativeObject = nativeGraphics;
		}
		
		static internal float systemDpiX {
			get {					
					if (defDpiX==0) {
						Bitmap bmp = new Bitmap(1,1);
						Graphics g = Graphics.FromImage(bmp);	
       					defDpiX = g.DpiX;
       				}
       				return defDpiX;
			}
		}

		static internal float systemDpiY {
			get {
					if (defDpiY==0) {
						Bitmap bmp = new Bitmap(1,1);
						Graphics g = Graphics.FromImage(bmp);	
       					defDpiY = g.DpiY;
       				}
       				return defDpiY;
			}
		}
		
		internal IntPtr NativeObject {
			get{
				return nativeObject;
			}

			set {
				nativeObject = value;
			}
		}

		[MonoTODO]
		public void AddMetafileComment (byte [] data)
		{
			throw new NotImplementedException ();
		}

		
		public GraphicsContainer BeginContainer ()
		{
			int state;
			Status status;
			status = GDIPlus.GdipBeginContainer2 (nativeObject, out state);
        		GDIPlus.CheckStatus (status);

                        return new GraphicsContainer(state);
		}
		
		public GraphicsContainer BeginContainer (Rectangle dstrect, Rectangle srcrect, GraphicsUnit unit)
		{
			int state;
			Status status;
			status = GDIPlus.GdipBeginContainerI (nativeObject, dstrect, srcrect, unit, out state);
			GDIPlus.CheckStatus (status);

			return new GraphicsContainer (state);
		}

		
		public GraphicsContainer BeginContainer (RectangleF dstrect, RectangleF srcrect, GraphicsUnit unit)
		{
			int state;
			Status status;
			status = GDIPlus.GdipBeginContainer (nativeObject, dstrect, srcrect, unit, out state);
			GDIPlus.CheckStatus (status);

			return new GraphicsContainer (state);
		}

		
		public void Clear (Color color)
		{
			Status status;
 			status = GDIPlus.GdipGraphicsClear (nativeObject, color.ToArgb ()); 				GDIPlus.CheckStatus (status);
		}

		[MonoTODO]
		public void Dispose ()
		{
		}

		
		public void DrawArc (Pen pen, Rectangle rect, float startAngle, float sweepAngle)
		{
			DrawArc (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}

		
		public void DrawArc (Pen pen, RectangleF rect, float startAngle, float sweepAngle)
		{
			DrawArc (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}

		
		public void DrawArc (Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			Status status;
			status = GDIPlus.GdipDrawArc (nativeObject, pen.nativeObject,
                                        x, y, width, height, startAngle, sweepAngle);
			GDIPlus.CheckStatus (status);
		}

		// Microsoft documentation states that the signature for this member should be
		// public void DrawArc( Pen pen,  int x,  int y,  int width,  int height,   int startAngle,
   		// int sweepAngle. However, GdipDrawArcI uses also float for the startAngle and sweepAngle params
   		public void DrawArc (Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
		{
			Status status;
			status = GDIPlus.GdipDrawArcI (nativeObject, pen.nativeObject,
						x, y, width, height, startAngle, sweepAngle);
			GDIPlus.CheckStatus (status);
		}

		public void DrawBezier (Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4)
		{
			Status status;
			status = GDIPlus.GdipDrawBezier (nativeObject, pen.nativeObject,
							pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X,
							pt3.Y, pt4.X, pt4.Y);
			GDIPlus.CheckStatus (status);
		}

		public void DrawBezier (Pen pen, Point pt1, Point pt2, Point pt3, Point pt4)
		{
			Status status;
			status = GDIPlus.GdipDrawBezierI (nativeObject, pen.nativeObject,
							pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X,
							pt3.Y, pt4.X, pt4.Y);
			GDIPlus.CheckStatus (status);
		}

		public void DrawBezier (Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
		{
			Status status;
			status = GDIPlus.GdipDrawBezier (nativeObject, pen.nativeObject, x1,
							y1, x2, y2, x3, y3, x4, y4);
			GDIPlus.CheckStatus (status);
		}

		public void DrawBeziers (Pen pen, Point [] points)
		{
                        int length = points.Length;
			Status status;

                        if (length < 3)
                                return;

			for (int i = 0; i < length; i += 3) {
                                Point p1 = points [i];
                                Point p2 = points [i + 1];
                                Point p3 = points [i + 2];
                                Point p4 = points [i + 3];

                                status = GDIPlus.GdipDrawBezier (nativeObject, 
							pen.nativeObject,
                                                        p1.X, p1.Y, p2.X, p2.Y, 
                                                        p3.X, p3.Y, p4.X, p4.Y);
				GDIPlus.CheckStatus (status);
                        }
		}

		public void DrawBeziers (Pen pen, PointF [] points)
		{
			int length = points.Length;
			Status status;

                        if (length < 3)
                                return;

			for (int i = 0; i < length; i += 3) {
                                PointF p1 = points [i];
                                PointF p2 = points [i + 1];
                                PointF p3 = points [i + 2];
                                PointF p4 = points [i + 3];

                                status = GDIPlus.GdipDrawBezier (nativeObject, 
							pen.nativeObject,
                                                        p1.X, p1.Y, p2.X, p2.Y, 
                                                        p3.X, p3.Y, p4.X, p4.Y);
				GDIPlus.CheckStatus (status);
                        }
		}

		
		public void DrawClosedCurve (Pen pen, PointF [] points)
		{
			Status status;
			status = GDIPlus.GdipDrawClosedCurve (nativeObject, pen.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawClosedCurve (Pen pen, Point [] points)
		{
			Status status;
			status = GDIPlus.GdipDrawClosedCurveI (nativeObject, pen.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}
 			
		public void DrawClosedCurve (Pen pen, Point [] points, float tension, FillMode fillmode)
		{
			Status status;
			status = GDIPlus.GdipDrawClosedCurve2I (nativeObject, pen.nativeObject, points, points.Length, tension);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawClosedCurve (Pen pen, PointF [] points, float tension, FillMode fillmode)
		{
			Status status;
			status = GDIPlus.GdipDrawClosedCurve2 (nativeObject, pen.nativeObject, points, points.Length, tension);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawCurve (Pen pen, Point [] points)
		{
			Status status;
			status = GDIPlus.GdipDrawCurveI (nativeObject, pen.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawCurve (Pen pen, PointF [] points)
		{
			Status status;
			status = GDIPlus.GdipDrawCurve (nativeObject, pen.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawCurve (Pen pen, PointF [] points, float tension)
		{
			Status status;
			status = GDIPlus.GdipDrawCurve2 (nativeObject, pen.nativeObject, points, points.Length, tension);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawCurve (Pen pen, Point [] points, float tension)
		{
			Status status;
			status = GDIPlus.GdipDrawCurve2I (nativeObject, pen.nativeObject, points, points.Length, tension);		
			GDIPlus.CheckStatus (status);
		}
		
		
		public void DrawCurve (Pen pen, PointF [] points, int offset, int numberOfSegments)
		{
			Status status;
			status = GDIPlus.GdipDrawCurve3 (nativeObject, pen.nativeObject,
							points, points.Length, offset,
							numberOfSegments, 0.5f);
			GDIPlus.CheckStatus (status);
		}

		public void DrawCurve (Pen pen, Point [] points, int offset, int numberOfSegments, float tension)
		{
			Status status;
			status = GDIPlus.GdipDrawCurve3I (nativeObject, pen.nativeObject,
							points, points.Length, offset,
							numberOfSegments, tension);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawCurve (Pen pen, PointF [] points, int offset, int numberOfSegments, float tension)
		{
			Status status;
			status = GDIPlus.GdipDrawCurve3 (nativeObject, pen.nativeObject,
							points, points.Length, offset,
							numberOfSegments, tension);
			GDIPlus.CheckStatus (status);
		}

		public void DrawEllipse (Pen pen, Rectangle rect)
		{
			DrawEllipse (pen, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void DrawEllipse (Pen pen, RectangleF rect)
		{
			DrawEllipse (pen, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void DrawEllipse (Pen pen, int x, int y, int width, int height)
		{
			Status status;
			status = GDIPlus.GdipDrawEllipseI (nativeObject, pen.nativeObject, x, y, width, height);
			GDIPlus.CheckStatus (status);
		}

		public void DrawEllipse (Pen pen, float x, float y, float width, float height)
		{
			Status status = GDIPlus.GdipDrawEllipse (nativeObject, pen.nativeObject, x, y, width, height);
			GDIPlus.CheckStatus (status);
		}

		[MonoTODO]
		public void DrawIcon (Icon icon, Rectangle targetRect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawIcon (Icon icon, int x, int y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawIconUnstretched (Icon icon, Rectangle targetRect)
		{
			throw new NotImplementedException ();
		}
		
		public void DrawImage (Image image, RectangleF rect)
		{
			Status status = GDIPlus.GdipDrawImageRect(nativeObject, image.NativeObject, rect.X, rect.Y, rect.Width, rect.Height);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, PointF point)
		{
			Status status = GDIPlus.GdipDrawImage (nativeObject, image.NativeObject, point.X, point.Y);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Point [] destPoints)
		{
			Status status = GDIPlus.GdipDrawImagePointsI (nativeObject, image.NativeObject, destPoints, destPoints.Length);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Point point)
		{
			DrawImage (image, point.X, point.Y);
		}

		
		public void DrawImage (Image image, Rectangle rect)
		{
			DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);
		}

		
		public void DrawImage (Image image, PointF [] destPoints)
		{
			Status status = GDIPlus.GdipDrawImagePoints (nativeObject, image.NativeObject, destPoints, destPoints.Length);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, int x, int y)
		{
			Status status = GDIPlus.GdipDrawImageI (nativeObject, image.NativeObject, x, y);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, float x, float y)
		{
			Status status = GDIPlus.GdipDrawImage (nativeObject, image.NativeObject, x, y);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit)
		{
			Status status = GDIPlus.GdipDrawImageRectRectI (nativeObject, image.NativeObject,
				destRect.X, destRect.Y, destRect.Width, destRect.Height,
				srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height,
				srcUnit, IntPtr.Zero, null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawImage (Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit)
		{			
			Status status = GDIPlus.GdipDrawImageRectRect (nativeObject, image.NativeObject,
				destRect.X, destRect.Y, destRect.Width, destRect.Height,
				srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height,
				srcUnit, IntPtr.Zero, null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit)
		{
			Status status = GDIPlus.GdipDrawImagePointsRectI (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y, 
				srcRect.Width, srcRect.Height, srcUnit, IntPtr.Zero, 
				null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit)
		{
			
			Status status = GDIPlus.GdipDrawImagePointsRect (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y, 
				srcRect.Width, srcRect.Height, srcUnit, IntPtr.Zero, 
				null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, 
                                ImageAttributes imageAttr)
		{
			Status status = GDIPlus.GdipDrawImagePointsRectI (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y,
				srcRect.Width, srcRect.Height, srcUnit,
				imageAttr.NativeObject, null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawImage (Image image, float x, float y, float width, float height)
		{
			Status status = GDIPlus.GdipDrawImageRect(nativeObject, image.NativeObject, x, y,
                           width, height);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, 
                                ImageAttributes imageAttr)
		{
			Status status = GDIPlus.GdipDrawImagePointsRect (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y,
				srcRect.Width, srcRect.Height, srcUnit, 
				imageAttr.NativeObject, null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, int x, int y, Rectangle srcRect, GraphicsUnit srcUnit)
		{			
			Status status = GDIPlus.GdipDrawImagePointRectI(nativeObject, image.NativeObject, x, y, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, srcUnit);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawImage (Image image, int x, int y, int width, int height)
		{
			Status status = GDIPlus.GdipDrawImageRectI (nativeObject, image.nativeObject, x, y, width, height);
			GDIPlus.CheckStatus (status);
		}

		public void DrawImage (Image image, float x, float y, RectangleF srcRect, GraphicsUnit srcUnit)
		{			
			Status status = GDIPlus.GdipDrawImagePointRect (nativeObject, image.nativeObject, x, y, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, srcUnit);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			Status status = GDIPlus.GdipDrawImagePointsRect (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y,
				srcRect.Width, srcRect.Height, srcUnit, 
				imageAttr.NativeObject, callback, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			
			Status status = GDIPlus.GdipDrawImagePointsRectI (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y,
				srcRect.Width, srcRect.Height, srcUnit, 
				imageAttr.NativeObject, callback, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, int callbackData)
		{

			Status status = GDIPlus.GdipDrawImagePointsRectI (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y, 
				srcRect.Width, srcRect.Height, srcUnit, 
				imageAttr.NativeObject, callback, (IntPtr) callbackData);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit)
		{
			Status status = GDIPlus.GdipDrawImageRectRect (nativeObject, image.NativeObject,
                                destRect.X, destRect.Y, destRect.Width, destRect.Height,
                       		srcX, srcY, srcWidth, srcHeight, srcUnit, IntPtr.Zero, 
                       		null, IntPtr.Zero);
			GDIPlus.CheckStatus (status); 					
		}
		
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, int callbackData)
		{
			Status status = GDIPlus.GdipDrawImagePointsRect (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y,
				srcRect.Width, srcRect.Height, srcUnit, 
				imageAttr.NativeObject, callback, (IntPtr) callbackData);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit)
		{
			Status status = GDIPlus.GdipDrawImageRectRectI (nativeObject, image.NativeObject,
                                destRect.X, destRect.Y, destRect.Width, destRect.Height,
                       		srcX, srcY, srcWidth, srcHeight, srcUnit, IntPtr.Zero, 
                       		null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs)
		{
			Status status = GDIPlus.GdipDrawImageRectRect (nativeObject, image.NativeObject,
                                destRect.X, destRect.Y, destRect.Width, destRect.Height,
                       		srcX, srcY, srcWidth, srcHeight, srcUnit,
				imageAttrs.NativeObject, null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr)
		{			
			Status status = GDIPlus.GdipDrawImageRectRectI (nativeObject, image.NativeObject, 
                                        destRect.X, destRect.Y, destRect.Width, 
					destRect.Height, srcX, srcY, srcWidth, srcHeight,
					srcUnit, imageAttr.NativeObject, null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			Status status = GDIPlus.GdipDrawImageRectRectI (nativeObject, image.NativeObject, 
                                        destRect.X, destRect.Y, destRect.Width, 
					destRect.Height, srcX, srcY, srcWidth, srcHeight,
					srcUnit, imageAttr.NativeObject, callback,
					IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			Status status = GDIPlus.GdipDrawImageRectRect (nativeObject, image.NativeObject, 
                                        destRect.X, destRect.Y, destRect.Width, 
					destRect.Height, srcX, srcY, srcWidth, srcHeight,
					srcUnit, imageAttr.NativeObject, 
					callback, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, IntPtr callbackData)
		{
			Status status = GDIPlus.GdipDrawImageRectRect (nativeObject, image.NativeObject, 
				destRect.X, destRect.Y, destRect.Width, destRect.Height,
				srcX, srcY, srcWidth, srcHeight, srcUnit, 
				imageAttr.NativeObject, callback, callbackData);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, IntPtr callbackData)
		{
			Status status = GDIPlus.GdipDrawImageRectRect (nativeObject, image.NativeObject, 
                       		destRect.X, destRect.Y, destRect.Width, destRect.Height,
				srcX, srcY, srcWidth, srcHeight, srcUnit,
				imageAttr.NativeObject, callback, callbackData);
			GDIPlus.CheckStatus (status);
		}		
		
		public void DrawImageUnscaled (Image image, Point point)
		{
			DrawImage(image, point.X, point.Y);
		}
		
		public void DrawImageUnscaled (Image image, Rectangle rect)
		{
			DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);
		}
		
		public void DrawImageUnscaled (Image image, int x, int y)
		{
			DrawImage(image, x, y);
		}

		public void DrawImageUnscaled (Image image, int x, int y, int width, int height)
		{
			DrawImage(image, x, y, width, height);
		}

		public void DrawLine (Pen pen, PointF pt1, PointF pt2)
		{
                        Status status = GDIPlus.GdipDrawLine (nativeObject, pen.nativeObject,
		                                pt1.X, pt1.Y, pt2.X, pt2.Y);
			GDIPlus.CheckStatus (status);
		}

		public void DrawLine (Pen pen, Point pt1, Point pt2)
		{
                        Status status = GDIPlus.GdipDrawLineI (nativeObject, pen.nativeObject,
		                                pt1.X, pt1.Y, pt2.X, pt2.Y);
			GDIPlus.CheckStatus (status);
		}

		public void DrawLine (Pen pen, int x1, int y1, int x2, int y2)
		{
			Status status = GDIPlus.GdipDrawLineI (nativeObject, pen.nativeObject, x1, y1, x2, y2);
			GDIPlus.CheckStatus (status);
		}

		public void DrawLine (Pen pen, float x1, float y1, float x2, float y2)
		{
			Status status = GDIPlus.GdipDrawLine (nativeObject, pen.nativeObject, x1, y1, x2, y2);
			GDIPlus.CheckStatus (status);
		}

		public void DrawLines (Pen pen, PointF [] points)
		{
			Status status = GDIPlus.GdipDrawLines (nativeObject, pen.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}

		public void DrawLines (Pen pen, Point [] points)
		{
			Status status = GDIPlus.GdipDrawLinesI (nativeObject, pen.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}

		public void DrawPath (Pen pen, GraphicsPath path)
		{
			Status status = GDIPlus.GdipDrawPath (nativeObject, pen.nativeObject, path.nativePath);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawPie (Pen pen, Rectangle rect, float startAngle, float sweepAngle)
		{
			DrawPie (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}
		
		public void DrawPie (Pen pen, RectangleF rect, float startAngle, float sweepAngle)
		{
			DrawPie (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}
		
		public void DrawPie (Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			Status status = GDIPlus.GdipDrawPie (nativeObject, pen.nativeObject, x, y, width, height, startAngle, sweepAngle);
			GDIPlus.CheckStatus (status);
		}
		
		// Microsoft documentation states that the signature for this member should be
		// public void DrawPie(Pen pen, int x,  int y,  int width,   int height,   int startAngle
   		// int sweepAngle. However, GdipDrawPieI uses also float for the startAngle and sweepAngle params
   		public void DrawPie (Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
		{
			Status status = GDIPlus.GdipDrawPieI (nativeObject, pen.nativeObject, x, y, width, height, startAngle, sweepAngle);
			GDIPlus.CheckStatus (status);
		}

		public void DrawPolygon (Pen pen, Point [] points)
		{
			Status status = GDIPlus.GdipDrawPolygonI (nativeObject, pen.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}

		public void DrawPolygon (Pen pen, PointF [] points)
		{
			Status status = GDIPlus.GdipDrawPolygon (nativeObject, pen.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}

		internal void DrawRectangle (Pen pen, RectangleF rect)
		{
			DrawRectangle (pen, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void DrawRectangle (Pen pen, Rectangle rect)
		{
			DrawRectangle (pen, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void DrawRectangle (Pen pen, float x, float y, float width, float height)
		{
			Status status = GDIPlus.GdipDrawRectangle (nativeObject, pen.nativeObject, x, y, width, height);
			GDIPlus.CheckStatus (status);
		}

		public void DrawRectangle (Pen pen, int x, int y, int width, int height)
		{
			Status status = GDIPlus.GdipDrawRectangleI (nativeObject, pen.nativeObject, x, y, width, height);
			GDIPlus.CheckStatus (status);
		}

		public void DrawRectangles (Pen pen, RectangleF [] rects)
		{
			foreach (RectangleF rc in rects)
				DrawRectangle (pen, rc.Left, rc.Top, rc.Width, rc.Height);
		}

		public void DrawRectangles (Pen pen, Rectangle [] rects)
		{
			foreach (RectangleF rc in rects)
				DrawRectangle(pen, rc.Left, rc.Top, rc.Width, rc.Height);
		}

		
		public void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle)
		{			
			Status status = GDIPlus.GdipDrawString (nativeObject, s, s.Length, font.NativeObject, ref layoutRectangle, IntPtr.Zero, brush.nativeObject);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawString (string s, Font font, Brush brush, PointF point)
		{
			RectangleF rc = new RectangleF (point.X, point.Y, 0, 0);
			Status status = GDIPlus.GdipDrawString (nativeObject, s, s.Length, font.NativeObject, ref rc, IntPtr.Zero, brush.nativeObject);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawString (string s, Font font, Brush brush, PointF point, StringFormat format)
		{
			RectangleF rc = new RectangleF (point.X, point.Y, 0, 0);
			Status status = GDIPlus.GdipDrawString (nativeObject, s, s.Length, font.NativeObject, ref rc, format.NativeObject, brush.nativeObject);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
		{
			Status status = GDIPlus.GdipDrawString (nativeObject, s, s.Length, font.NativeObject, ref layoutRectangle, format.NativeObject, brush.nativeObject);
			GDIPlus.CheckStatus (status);
		}

		public void DrawString (string s, Font font, Brush brush, float x, float y)
		{
			RectangleF rc = new RectangleF (x, y, 0, 0);
			
			Status status = GDIPlus.GdipDrawString (nativeObject, s, s.Length, font.NativeObject, 
				ref rc, IntPtr.Zero, brush.nativeObject);
			GDIPlus.CheckStatus (status);
		}

		public void DrawString (string s, Font font, Brush brush, float x, float y, StringFormat format)
		{
			RectangleF rc = new RectangleF (x, y, 0, 0);

			Status status = GDIPlus.GdipDrawString (nativeObject, s, s.Length, font.NativeObject,
				ref rc, format.NativeObject, brush.nativeObject);
			GDIPlus.CheckStatus (status);
		}

		
		public void EndContainer (GraphicsContainer container)
		{
			Status status = GDIPlus.GdipEndContainer(nativeObject, container.NativeObject);
			GDIPlus.CheckStatus (status);
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point destPoint, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF destPoint, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF destPoint, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point destPoint, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point destPoint, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF destPoint, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}
	
		public void ExcludeClip (Rectangle rect)
		{
			Status status = GDIPlus.GdipSetClipRectI (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, CombineMode.Exclude);
			GDIPlus.CheckStatus (status);
		}

		[MonoTODO]
		public void ExcludeClip (Region region)
		{
			throw new NotImplementedException ();
		}

		
		public void FillClosedCurve (Brush brush, PointF [] points)
		{
		       Status status = GDIPlus.GdipFillClosedCurve (nativeObject, brush.NativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}

		
		public void FillClosedCurve (Brush brush, Point [] points)
		{
			Status status = GDIPlus.GdipFillClosedCurveI (nativeObject, brush.NativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}

		
		public void FillClosedCurve (Brush brush, PointF [] points, FillMode fillmode)
		{
			FillClosedCurve (brush, points, fillmode, 0.5f);
		}
		
		public void FillClosedCurve (Brush brush, Point [] points, FillMode fillmode)
		{
			FillClosedCurve (brush, points, fillmode, 0.5f);
		}

		public void FillClosedCurve (Brush brush, PointF [] points, FillMode fillmode, float tension)
		{
			Status status = GDIPlus.GdipFillClosedCurve2 (nativeObject, brush.NativeObject, points, points.Length, tension, fillmode);
			GDIPlus.CheckStatus (status);
		}

		public void FillClosedCurve (Brush brush, Point [] points, FillMode fillmode, float tension)
		{
			Status status = GDIPlus.GdipFillClosedCurve2I (nativeObject, brush.NativeObject, points, points.Length, tension, fillmode);
			GDIPlus.CheckStatus (status);
		}

		public void FillEllipse (Brush brush, Rectangle rect)
		{
			FillEllipse (brush, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void FillEllipse (Brush brush, RectangleF rect)
		{
			FillEllipse (brush, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void FillEllipse (Brush brush, float x, float y, float width, float height)
		{
                        Status status = GDIPlus.GdipFillEllipse (nativeObject, brush.nativeObject, x, y, width, height);
			GDIPlus.CheckStatus (status);
		}

		public void FillEllipse (Brush brush, int x, int y, int width, int height)
		{
			Status status = GDIPlus.GdipFillEllipseI (nativeObject, brush.nativeObject, x, y, width, height);
			GDIPlus.CheckStatus (status);
		}

		public void FillPath (Brush brush, GraphicsPath path)
		{
			Status status = GDIPlus.GdipFillPath (nativeObject, brush.NativeObject,  path.NativeObject);
			GDIPlus.CheckStatus (status);
		}

		public void FillPie (Brush brush, Rectangle rect, float startAngle, float sweepAngle)
		{
			Status status = GDIPlus.GdipFillPie (nativeObject, brush.NativeObject, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
			GDIPlus.CheckStatus (status);
		}

		public void FillPie (Brush brush, int x, int y, int width, int height, int startAngle, int sweepAngle)
		{
			Status status = GDIPlus.GdipFillPieI (nativeObject, brush.NativeObject, x, y, width, height, startAngle, sweepAngle);
			GDIPlus.CheckStatus (status);
		}

		public void FillPie (Brush brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			Status status = GDIPlus.GdipFillPie (nativeObject, brush.NativeObject, x, y, width, height, startAngle, sweepAngle);
			GDIPlus.CheckStatus (status);
		}

		public void FillPolygon (Brush brush, PointF [] points)
		{
			Status status = GDIPlus.GdipFillPolygon2 (nativeObject, brush.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}

		public void FillPolygon (Brush brush, Point [] points)
		{
			Status status = GDIPlus.GdipFillPolygon2I (nativeObject, brush.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}

		public void FillPolygon (Brush brush, Point [] points, FillMode fillMode)
		{
			Status status = GDIPlus.GdipFillPolygonI (nativeObject, brush.nativeObject, points, points.Length, fillMode);
			GDIPlus.CheckStatus (status);
		}

		public void FillPolygon (Brush brush, PointF [] points, FillMode fillMode)
		{
			Status status = GDIPlus.GdipFillPolygon (nativeObject, brush.nativeObject, points, points.Length, fillMode);
			GDIPlus.CheckStatus (status);
		}

		public void FillRectangle (Brush brush, RectangleF rect)
		{
                        FillRectangle (brush, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void FillRectangle (Brush brush, Rectangle rect)
		{
                        FillRectangle (brush, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void FillRectangle (Brush brush, int x, int y, int width, int height)
		{
			Status status = GDIPlus.GdipFillRectangle (nativeObject, brush.nativeObject, (float)x, (float)y, (float)width, (float)height);
			GDIPlus.CheckStatus (status);
		}

		public void FillRectangle (Brush brush, float x, float y, float width, float height)
		{
			Status status = GDIPlus.GdipFillRectangle (nativeObject, brush.nativeObject, x, y, width, height);
			GDIPlus.CheckStatus (status);
		}

		public void FillRectangles (Brush brush, Rectangle [] rects)
		{
                        foreach (Rectangle rc in rects)
                                FillRectangle(brush, rc);
		}

		public void FillRectangles (Brush brush, RectangleF [] rects)
		{
			foreach (RectangleF rc in rects)
                                FillRectangle(brush, rc);
		}

		
		public void FillRegion (Brush brush, Region region)
		{
			Status status = GDIPlus.GdipFillRegion (nativeObject, brush.NativeObject, region.NativeObject);                  
                        GDIPlus.CheckStatus(status);
		}

		
		public void Flush ()
		{
			Flush (FlushIntention.Flush);
		}

		
		public void Flush (FlushIntention intention)
		{
			Status status = GDIPlus.GdipFlush (nativeObject, intention);
                        GDIPlus.CheckStatus (status);                     
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]		
		public static Graphics FromHdc (IntPtr hdc)
		{
			int graphics;
			Status status = GDIPlus.GdipCreateFromHDC (hdc, out graphics); 				GDIPlus.CheckStatus (status);
			    
			Graphics result = new Graphics ((IntPtr) graphics);
			return result;
		}

		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static Graphics FromHdc (IntPtr hdc, IntPtr hdevice)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static Graphics FromHdcInternal (IntPtr hdc)
		{
			throw new NotImplementedException ();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]		
		public static Graphics FromHwnd (IntPtr hwnd)
		{
			IntPtr graphics;
			
			Status status = GDIPlus.GdipCreateFromHWND (hwnd, out graphics); 				GDIPlus.CheckStatus (status);
 			
			return new Graphics (graphics); 
		}

		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static Graphics FromHwndInternal (IntPtr hwnd)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Graphics FromImage (Image image)
		{
			if (image == null) throw new ArgumentException ();
			int graphics;
			Status status = GDIPlus.GdipGetImageGraphicsContext (image.nativeObject, out graphics);
			GDIPlus.CheckStatus (status);
			Graphics result = new Graphics ((IntPtr) graphics);
			return result;
		}

		internal static Graphics FromXDrawable (IntPtr drawable, IntPtr display)
		{
		  IntPtr graphics;
		  Status s = GDIPlus.GdipCreateFromXDrawable_linux (drawable, display, out graphics);
		  GDIPlus.CheckStatus (s);
		  return new Graphics (graphics);
		}

		[MonoTODO]
		public static IntPtr GetHalftonePalette ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public IntPtr GetHdc ()
		{
			int hdc;
			Status status = GDIPlus.GdipGetDC (nativeObject, out hdc);
			GDIPlus.CheckStatus (status);

			return (IntPtr) hdc;
		}

		
		public Color GetNearestColor (Color color)
		{
			int argb;
			
			Status status = GDIPlus.GdipGetNearestColor (nativeObject, out argb);
			GDIPlus.CheckStatus (status);

			return Color.FromArgb (argb);
		}

		[MonoTODO]
		public void IntersectClip (Region region)
		{
			throw new NotImplementedException ();
		}
		
		public void IntersectClip (RectangleF rect)
		{
			Status status = GDIPlus.GdipSetClipRect (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, CombineMode.Intersect);
			GDIPlus.CheckStatus (status);
		}

		public void IntersectClip (Rectangle rect)
		{			
			Status status = GDIPlus.GdipSetClipRectI (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, CombineMode.Intersect);
			GDIPlus.CheckStatus (status);
		}

		public bool IsVisible (Point point)
		{
			bool isVisible = false;

			Status status = GDIPlus.GdipIsVisiblePointI (nativeObject, point.X, point.Y, out isVisible);
			GDIPlus.CheckStatus (status);

                        return isVisible;
		}

		
		public bool IsVisible (RectangleF rect)
		{
			bool isVisible = false;

			Status status = GDIPlus.GdipIsVisibleRect (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, out isVisible);
			GDIPlus.CheckStatus (status);

                        return isVisible;
		}

		public bool IsVisible (PointF point)
		{
			bool isVisible = false;

			Status status = GDIPlus.GdipIsVisiblePoint (nativeObject, point.X, point.Y, out isVisible);
			GDIPlus.CheckStatus (status);

                        return isVisible;
		}
		
		public bool IsVisible (Rectangle rect)
		{
			bool isVisible = false;

			Status status = GDIPlus.GdipIsVisibleRectI (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, out isVisible);
			GDIPlus.CheckStatus (status);

                        return isVisible;
		}
		
		public bool IsVisible (float x, float y)
		{
			return IsVisible (new PointF (x, y));
		}
		
		public bool IsVisible (int x, int y)
		{
			return IsVisible (new Point (x, y));
		}
		
		public bool IsVisible (float x, float y, float width, float height)
		{
			return IsVisible (new RectangleF (x, y, width, height));
		}

		
		public bool IsVisible (int x, int y, int width, int height)
		{
			return IsVisible (new Rectangle (x, y, width, height));
		}

		[MonoTODO]
		public Region [] MeasureCharacterRanges (string text, Font font, RectangleF layoutRect, StringFormat stringFormat)
		{
			throw new NotImplementedException ();
		}

		
		public SizeF MeasureString (string text, Font font)
		{
			return MeasureString (text, font, new Size(0,0));
		}

		
		public SizeF MeasureString (string text, Font font, SizeF layoutArea)
		{
			int charactersFitted, linesFilled;			
			RectangleF boundingBox = new RectangleF ();
			RectangleF rect = new RectangleF (0,0,layoutArea.Width, layoutArea.Height);
					
			Status status = GDIPlus.GdipMeasureString (nativeObject, text, text.Length, 
				font.NativeObject, ref rect, IntPtr.Zero, out boundingBox,
				out charactersFitted, out linesFilled);
			GDIPlus.CheckStatus (status);

			return new SizeF(boundingBox.Width, boundingBox.Height);
		}

		[MonoTODO]
		public SizeF MeasureString (string text, Font font, int width)
		{
			throw new NotImplementedException ();
		}

		
		public SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat stringFormat)
		{
			int charactersFitted, linesFilled;			
			return MeasureString (text, font, layoutArea, stringFormat, out charactersFitted, out linesFilled);
		}

		[MonoTODO]
		public SizeF MeasureString (string text, Font font, int width, StringFormat format)
		{
			throw new NotImplementedException ();
		}

		
		public SizeF MeasureString (string text, Font font, PointF origin, StringFormat stringFormat)
		{
			RectangleF boundingBox = new RectangleF ();
			RectangleF rect = new RectangleF (origin.X, origin.Y, 0,0);
			int charactersFitted, linesFilled;

			Status status = GDIPlus.GdipMeasureString (nativeObject, text, text.Length, 
				font.NativeObject, ref rect, stringFormat.NativeObject, 
				out boundingBox, out charactersFitted, out linesFilled);
			GDIPlus.CheckStatus (status);

			return new SizeF (boundingBox.Width, boundingBox.Height);
		}

		
		public SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat stringFormat, out int charactersFitted, out int linesFilled)
		{	
			RectangleF boundingBox = new RectangleF ();
			RectangleF rect = new RectangleF (0,0,layoutArea.Width, layoutArea.Height);
			Status status = GDIPlus.GdipMeasureString (nativeObject, text, text.Length, 
				font.NativeObject, ref rect, stringFormat.NativeObject,
				out boundingBox, out charactersFitted, out linesFilled);
			GDIPlus.CheckStatus (status);

			return new SizeF (boundingBox.Width, boundingBox.Height);
		}

		public void MultiplyTransform (Matrix matrix)
		{
			MultiplyTransform (matrix, MatrixOrder.Prepend);
		}

		public void MultiplyTransform (Matrix matrix, MatrixOrder order)
		{
			Status status = GDIPlus.GdipMultiplyWorldTransform (nativeObject, matrix.nativeMatrix, order);
			GDIPlus.CheckStatus (status);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public void ReleaseHdc (IntPtr hdc)
		{
			Status status = GDIPlus.GdipReleaseDC (nativeObject, hdc);
			GDIPlus.CheckStatus (status);
		}

		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public void ReleaseHdcInternal (IntPtr hdc)
		{
			throw new NotImplementedException ();
		}

		
		public void ResetClip ()
		{
			Status status = GDIPlus.GdipResetClip (nativeObject);
			GDIPlus.CheckStatus (status);
		}

		public void ResetTransform ()
		{
			Status status = GDIPlus.GdipResetWorldTransform (nativeObject);
			GDIPlus.CheckStatus (status);
		}

		public void Restore (GraphicsState gstate)
		{
			Transform = gstate.matrix.Clone ();
			Status status = GDIPlus.GdipRestoreGraphics (nativeObject, gstate.nativeState);
			GDIPlus.CheckStatus (status);
		}


		public void RotateTransform (float angle)
		{
			RotateTransform (angle, MatrixOrder.Prepend);
		}

		public void RotateTransform (float angle, MatrixOrder order)
		{

			Status status = GDIPlus.GdipRotateWorldTransform (nativeObject, angle, order);
			GDIPlus.CheckStatus (status);
		}

		public GraphicsState Save ()
		{
			GraphicsState state = new GraphicsState ();
			state.matrix = Transform.Clone ();
			uint saveState;
			Status status = GDIPlus.GdipSaveGraphics (nativeObject, out saveState);
			GDIPlus.CheckStatus (status);

			state.nativeState = saveState;
			return state;
		}

		public void ScaleTransform (float sx, float sy)
		{
			ScaleTransform (sx, sy, MatrixOrder.Prepend);
		}

		public void ScaleTransform (float sx, float sy, MatrixOrder order)
		{
                        Status status = GDIPlus.GdipScaleWorldTransform (nativeObject, sx, sy, order);
			GDIPlus.CheckStatus (status);
		}

		
		public void SetClip (RectangleF rect)
		{
                        SetClip (rect, CombineMode.Replace);
		}

		
		public void SetClip (GraphicsPath path)
		{
			SetClip (path, CombineMode.Replace);
		}

		
		public void SetClip (Rectangle rect)
		{
			SetClip (rect, CombineMode.Replace);
		}

		
		public void SetClip (Graphics g)
		{
			SetClip (g, CombineMode.Replace);
		}

		
		public void SetClip (Graphics g, CombineMode combineMode)
		{
			Status status = GDIPlus.GdipSetClipGraphics (nativeObject, g.NativeObject, combineMode);
			GDIPlus.CheckStatus (status);
		}

		
		public void SetClip (Rectangle rect, CombineMode combineMode)
		{
			Status status = GDIPlus.GdipSetClipRectI (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, combineMode);
			GDIPlus.CheckStatus (status);
		}

		
		public void SetClip (RectangleF rect, CombineMode combineMode)
		{
			Status status = GDIPlus.GdipSetClipRect (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, combineMode);
			GDIPlus.CheckStatus (status);
		}

		[MonoTODO]
		public void SetClip (Region region, CombineMode combineMode)
		{
			//GDIPlus.GdipSetClipRegion(nativeObject,  region.NativeObject, combineMode); //TODO: Region not implemented yet
			throw new NotImplementedException();
		}

		
		public void SetClip (GraphicsPath path, CombineMode combineMode)
		{
			Status status = GDIPlus.GdipSetClipPath (nativeObject, path.NativeObject, combineMode);
			GDIPlus.CheckStatus (status);
		}

		
		public void TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, PointF [] pts)
		{
			IntPtr ptrPt =  GDIPlus.FromPointToUnManagedMemory (pts);
            
                        Status status = GDIPlus.GdipTransformPoints (nativeObject, destSpace, srcSpace,  ptrPt, pts.Length);
			
			GDIPlus.FromUnManagedMemoryToPoint (ptrPt, pts);
		}


		public void TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, Point [] pts)
		{						
                        IntPtr ptrPt =  GDIPlus.FromPointToUnManagedMemoryI (pts);
            
                        Status status = GDIPlus.GdipTransformPointsI (nativeObject, destSpace, srcSpace, ptrPt, pts.Length);
			GDIPlus.CheckStatus (status);
			
			GDIPlus.FromUnManagedMemoryToPointI (ptrPt, pts);
		}

		
		public void TranslateClip (int dx, int dy)
		{
			Status status = GDIPlus.GdipTranslateClipI (nativeObject, dx, dy);
			GDIPlus.CheckStatus (status);
		}

		
		public void TranslateClip (float dx, float dy)
		{
			Status status = GDIPlus.GdipTranslateClip (nativeObject, dx, dy);
			GDIPlus.CheckStatus (status);
		}

		public void TranslateTransform (float dx, float dy)
		{
			TranslateTransform (dx, dy, MatrixOrder.Prepend);
		}

		
		public void TranslateTransform (float dx, float dy, MatrixOrder order)
		{			
			Status status = GDIPlus.GdipTranslateWorldTransform (nativeObject, dx, dy, order);
			GDIPlus.CheckStatus (status);
		}

		public Region Clip {
			get {
				throw new NotImplementedException ();
			}
			set {
				//throw new NotImplementedException ();
			}
		}

		public RectangleF ClipBounds {
			get {
                                RectangleF rect = new RectangleF ();
                                Status status = GDIPlus.GdipGetClipBounds (nativeObject, out rect);
				GDIPlus.CheckStatus (status);
				return rect;
			}
		}

		public CompositingMode CompositingMode {
			get {
                                CompositingMode mode;
                                Status status = GDIPlus.GdipGetCompositingMode (nativeObject, out mode);
				GDIPlus.CheckStatus (status);

				return mode;
			}
			set {
                                Status status = GDIPlus.GdipSetCompositingMode (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}

		}

		public CompositingQuality CompositingQuality {
			get {
                                CompositingQuality quality;

                                Status status = GDIPlus.GdipGetCompositingQuality (nativeObject, out quality);
				GDIPlus.CheckStatus (status);
        			return quality;
			}
			set {
                                Status status = GDIPlus.GdipSetCompositingQuality (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public float DpiX {
			get {
                                float x;

       				Status status = GDIPlus.GdipGetDpiX (nativeObject, out x);
				GDIPlus.CheckStatus (status);
        			return x;
			}
		}

		public float DpiY {
			get {
                                float y;

       				Status status = GDIPlus.GdipGetDpiY (nativeObject, out y);
				GDIPlus.CheckStatus (status);
        			return y;
			}
		}

		public InterpolationMode InterpolationMode {
			get {				
                                InterpolationMode imode = InterpolationMode.Invalid;
        			Status status = GDIPlus.GdipGetInterpolationMode (nativeObject, out imode);
				GDIPlus.CheckStatus (status);
        			return imode;
			}
			set {
                                Status status = GDIPlus.GdipSetInterpolationMode (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public bool IsClipEmpty {
			get {
                                bool isEmpty = false;

        			Status status = GDIPlus.GdipIsClipEmpty (nativeObject, out isEmpty);
				GDIPlus.CheckStatus (status);
        			return isEmpty;
			}
		}

		public bool IsVisibleClipEmpty {
			get {
                                bool isEmpty = false;

        			Status status = GDIPlus.GdipIsVisibleClipEmpty (nativeObject, out isEmpty);
				GDIPlus.CheckStatus (status);
        			return isEmpty;
			}
		}

		public float PageScale {
			get {
                                float scale;

        			Status status = GDIPlus.GdipGetPageScale (nativeObject, out scale);
				GDIPlus.CheckStatus (status);
        			return scale;
			}
			set {
                                Status status = GDIPlus.GdipSetPageScale (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public GraphicsUnit PageUnit {
			get {
                                GraphicsUnit unit;
                                
                                Status status = GDIPlus.GdipGetPageUnit (nativeObject, out unit);
				GDIPlus.CheckStatus (status);
        			return unit;
			}
			set {
                                Status status = GDIPlus.GdipSetPageUnit (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public PixelOffsetMode PixelOffsetMode {
			get {
			        PixelOffsetMode pixelOffset = PixelOffsetMode.Invalid;
                                
                                Status status = GDIPlus.GdipGetPixelOffsetMode (nativeObject, out pixelOffset);
				GDIPlus.CheckStatus (status);
        			return pixelOffset;
			}
			set {
                                Status status = GDIPlus.GdipSetPixelOffsetMode (nativeObject, value); 
				GDIPlus.CheckStatus (status);
			}
		}

		public Point RenderingOrigin {
			get {
                                int x, y;
				Status status = GDIPlus.GdipGetRenderingOrigin (nativeObject, out x, out y);
				GDIPlus.CheckStatus (status);
                                return new Point (x, y);
			}

			set {
                                Status status = GDIPlus.GdipSetRenderingOrigin (nativeObject, value.X, value.Y);
				GDIPlus.CheckStatus (status);
			}
		}

		public SmoothingMode SmoothingMode {
			get {
                                SmoothingMode mode = SmoothingMode.Invalid;

				Status status = GDIPlus.GdipGetSmoothingMode (nativeObject, out mode);
				GDIPlus.CheckStatus (status);
                                return mode;
			}

			set {
                                Status status = GDIPlus.GdipSetSmoothingMode (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public int TextContrast {
			get {	
                                int contrast;
					
                                Status status = GDIPlus.GdipGetTextContrast (nativeObject, out contrast);
				GDIPlus.CheckStatus (status);
                                return contrast;
			}

                        set {
                                Status status = GDIPlus.GdipSetTextContrast (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public TextRenderingHint TextRenderingHint {
			get {
                                TextRenderingHint hint;

                                Status status = GDIPlus.GdipGetTextRenderingHint (nativeObject, out hint);
				GDIPlus.CheckStatus (status);
                                return hint;        
			}

			set {
                                Status status = GDIPlus.GdipSetTextRenderingHint (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public Matrix Transform {
			get {
                                Matrix matrix = new Matrix ();
                                Status status = GDIPlus.GdipGetWorldTransform (nativeObject, matrix.nativeMatrix);
				GDIPlus.CheckStatus (status);
                                return matrix;
			}
			set {
                                Status status = GDIPlus.GdipSetWorldTransform (nativeObject, value.nativeMatrix);
				GDIPlus.CheckStatus (status);
			}
		}

		public RectangleF VisibleClipBounds {
			get {
                                RectangleF rect;
					
                                Status status = GDIPlus.GdipGetVisibleClipBounds (nativeObject, out rect);
				GDIPlus.CheckStatus (status);
                                return rect;
			}
		}
	}
}

