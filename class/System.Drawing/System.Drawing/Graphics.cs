//
// System.Drawing.Graphics.cs
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com) (stubbed out)
//      Alexandre Pigolkine(pigolkine@gmx.de)
//
using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace System.Drawing
{
	[ComVisible(false)]
	public sealed class Graphics : MarshalByRefObject, IDisposable
	{
		internal IntPtr nativeObject = IntPtr.Zero;
		
		public delegate bool EnumerateMetafileProc (EmfPlusRecordType recordType,
							    int flags,
							    int dataSize,
							    IntPtr data,
							    PlayRecordCallback callbackData);
		
		public delegate bool DrawImageAbort (IntPtr callbackData);

		private Graphics (IntPtr nativeGraphics)
		{
			nativeObject = nativeGraphics;
		}

		[MonoTODO]
		public void AddMetafileComment (byte [] data)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public GraphicsContainer BeginContainer ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public GraphicsContainer BeginContainer (Rectangle dstrect, Rectangle srcrect, GraphicsUnit unit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public GraphicsContainer BeginContainer (RectangleF dstrect, RectangleF srcrect, GraphicsUnit unit)
		{
			throw new NotImplementedException ();
		}

		
		public void Clear (Color color)
		{			
 			Status status = GDIPlus.GdipGraphicsClear(nativeObject, color.ToArgb());
 			
 			if (status != Status.Ok)
				throw new ArgumentException ("Error calling GDIPlus.GdipGraphicsClear:" +status);
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
			Status status = GDIPlus.GdipDrawArc (nativeObject, pen.nativeObject,
                                        x, y, width, height, startAngle, sweepAngle);
                                        
			if (status != Status.Ok)
				throw new ArgumentException ("Error calling GDIPlus.GdipGraphicsClear:" +status);                                        
		}

		public void DrawArc (Pen pen, int x, int y, int width, int height, float startAngle, float sweepAngle)
		{			
			Status status = GDIPlus.GdipDrawArcI (nativeObject, pen.nativeObject,
                                        x, y, width, height, startAngle, sweepAngle);
                                                    
            if (status != Status.Ok)                            
				throw new ArgumentException ("Error calling GDIPlus.GdipGraphicsClear:" +status);                                        
		                                        
		}

		public void DrawBezier (Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4)
		{
			Status status = GDIPlus.GdipDrawBezier (nativeObject, pen.nativeObject, 
                                        pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
                                        
			if (status != Status.Ok)                            
				throw new ArgumentException ("Error calling GDIPlus.DrawBezier:" +status);                                                                                
		}

		public void DrawBezier (Pen pen, Point pt1, Point pt2, Point pt3, Point pt4)
		{
			Status status = GDIPlus.GdipDrawBezierI (nativeObject, pen.nativeObject, 
                                        pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
                                        
			if (status != Status.Ok)                            
				throw new ArgumentException ("Error calling GDIPlus.GdipDrawBezierI:" +status);                                                                                                                        
		}

		public void DrawBezier (Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
		{
			Status status = GDIPlus.GdipDrawBezier (nativeObject, pen.nativeObject, x1, y1, x2, y2, x3, y3, x4, y4);
			
			if (status != Status.Ok)                            
				throw new ArgumentException ("Error calling GDIPlus.GdipDrawBezier:" +status);                                                                                                                        
		}

        [MonoTODO]
		public void DrawBeziers (Pen pen, Point [] points)
		{
                        int length = points.Length;

                        if (length < 3)
                                return;

			for (int i = 0; i < length; i += 3) {
                                Point p1 = points [i];
                                Point p2 = points [i + 1];
                                Point p3 = points [i + 2];
                                Point p4 = points [i + 3];

                                GDIPlus.GdipDrawBezier (nativeObject, pen.nativeObject,
                                                        p1.X, p1.Y, p2.X, p2.Y, 
                                                        p3.X, p3.Y, p4.X, p4.Y);
                        }
		}

                [MonoTODO]
		public void DrawBeziers (Pen pen, PointF [] points)
		{
			int length = points.Length;

                        if (length < 3)
                                return;

			for (int i = 0; i < length; i += 3) {
                                PointF p1 = points [i];
                                PointF p2 = points [i + 1];
                                PointF p3 = points [i + 2];
                                PointF p4 = points [i + 3];

                                GDIPlus.GdipDrawBezier (nativeObject, pen.nativeObject,
                                                        p1.X, p1.Y, p2.X, p2.Y, 
                                                        p3.X, p3.Y, p4.X, p4.Y);
                        }
		}

		
		public void DrawClosedCurve (Pen pen, PointF [] points)
		{
	 		Status status =  GDIPlus.GdipDrawClosedCurve(nativeObject, pen.nativeObject,  points, points.Length);
	 		
			if (status != Status.Ok)                            
				throw new ArgumentException ("Error calling GDIPlus.GdipDrawClosedCurve:" +status);                                                                                                                        
		}

		
		public void DrawClosedCurve (Pen pen, Point [] points)
		{
			Status status =  GDIPlus.GdipDrawClosedCurveI(nativeObject, pen.nativeObject,  points, points.Length);
	 		
			if (status != Status.Ok)                            
				throw new ArgumentException ("Error calling GDIPlus.GdipDrawClosedCurveI:" +status);                                                                                                                        
		}
 			
		public void DrawClosedCurve (Pen pen, Point [] points, float tension, FillMode fillmode)
		{
			Status status =  GDIPlus.GdipDrawClosedCurve2I(nativeObject, pen.nativeObject,  points, points.Length, tension);
	 		
			if (status != Status.Ok)                            
				throw new ArgumentException ("Error calling GDIPlus.GdipDrawClosedCurve2I:" +status);                                                                                                                        
		}

		
		public void DrawClosedCurve (Pen pen, PointF [] points, float tension, FillMode fillmode)
		{
			Status status =  GDIPlus.GdipDrawClosedCurve2(nativeObject, pen.nativeObject,  points, points.Length, tension);
	 		
			if (status != Status.Ok)                            
				throw new ArgumentException ("Error calling GDIPlus.GdipDrawClosedCurve2:" +status);                                                                                                                        
			
		}
		
		public void DrawCurve (Pen pen, Point [] points)
		{
			Status status =  GDIPlus.GdipDrawCurveI(nativeObject, pen.nativeObject,  points, points.Length);
	 		
			if (status != Status.Ok)                            
				throw new ArgumentException ("Error calling GDIPlus.GdipDrawCurveI:" +status);                                                                                                                        
		}
		
		public void DrawCurve (Pen pen, PointF [] points)
		{
			Status status =  GDIPlus.GdipDrawCurve(nativeObject, pen.nativeObject,  points, points.Length); 		
	 		
			if (status != Status.Ok)                            
				throw new ArgumentException ("Error calling GDIPlus.GdipDrawCurve:" +status);                                                                                                                        
		}
		
		public void DrawCurve (Pen pen, PointF [] points, float tension)
		{			
			Status status =  GDIPlus.GdipDrawCurve2(nativeObject, pen.nativeObject,  points, points.Length, tension);
	 		
			if (status != Status.Ok)                            
				throw new ArgumentException ("Error calling GDIPlus.GdipDrawCurve2:" +status);                                                                                                                        
		}
		
		public void DrawCurve (Pen pen, Point [] points, float tension)
		{
			Status status =  GDIPlus.GdipDrawCurve2I(nativeObject, pen.nativeObject,  points, points.Length, tension);
	 		
			if (status != Status.Ok)                            
				throw new ArgumentException ("Error calling GDIPlus.GdipDrawCurve2I:" +status);                                                                                                                        
		}
		
		[MonoTODO]
		public void DrawCurve (Pen pen, PointF [] points, int offset, int numberOfSegments)
		{
			
		}

		public void DrawCurve (Pen pen, Point [] points, int offset, int numberOfSegments, float tension)
		{
			Status status =  GDIPlus.GdipDrawCurve3I(nativeObject, pen.nativeObject, points, points.Length, offset,  numberOfSegments, tension);
	 		
			if (status != Status.Ok)                            
				throw new ArgumentException ("Error calling GDIPlus.GdipDrawCurve3:" +status);                                                                                                                        
		}

		
		public void DrawCurve (Pen pen, PointF [] points, int offset, int numberOfSegments, float tension)
		{
			Status status =  GDIPlus.GdipDrawCurve3(nativeObject, pen.nativeObject, points, points.Length, offset,  numberOfSegments, tension);
	 		
			if (status != Status.Ok)                            
				throw new ArgumentException ("Error calling GDIPlus.GdipDrawCurve3:" +status);                                                                                                                        
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
			GDIPlus.GdipDrawEllipseI (nativeObject, pen.nativeObject, x, y, width, height);
		}

		public void DrawEllipse (Pen pen, float x, float y, float width, float height)
		{
			GDIPlus.GdipDrawEllipse (nativeObject, pen.nativeObject, x, y, width, height); 
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

		[MonoTODO]
		public void DrawImage (Image image, RectangleF rect)
		{
			throw new NotImplementedException ();
		}

		
		public void DrawImage (Image image, PointF point)
		{
			Status status = GDIPlus.GdipDrawImage(nativeObject, image.NativeObject, point.X, point.Y);
 			
 			if (status != Status.Ok)
				throw new ArgumentException ("Error calling GDIPlus.DrawImage (Image image, PointF point):" +status);					
		}

		[MonoTODO]
		public void DrawImage (Image image, Point [] destPoints)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Point point)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, PointF [] destPoints)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, int x, int y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, float x, float y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, float x, float y, float width, float height)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, int x, int y, Rectangle srcRect, GraphicsUnit srcUnit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, int x, int y, int width, int height)
		{
			GDIPlus.GdipDrawImageRectI (nativeObject, image.nativeObject, x, y, width, height);
		}

		[MonoTODO]
		public void DrawImage (Image image, float x, float y, RectangleF srcRect, GraphicsUnit srcUnit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, int callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, int callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs)
		{
			throw new NotImplementedException ();
		}
		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr)
		{			
			Status status =  GDIPlus.GdipDrawImageRectRectI(nativeObject, image.NativeObject, //aki
								destRect.X, destRect.Y, destRect.Width, destRect.Height,
								srcX, srcY, srcWidth, srcHeight,
								srcUnit, imageAttr.NativeObject, IntPtr.Zero, 0);
								 			
 			if (status != Status.Ok)
				throw new ArgumentException ("Error calling GDIPlus.DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr)" +status);		
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, DrawImageAbort callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, DrawImageAbort callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, DrawImageAbort callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImageUnscaled (Image image, Point point)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImageUnscaled (Image image, Rectangle rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImageUnscaled (Image image, int x, int y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImageUnscaled (Image image, int x, int y, int width, int height)
		{
			throw new NotImplementedException ();
		}

		public void DrawLine (Pen pen, PointF pt1, PointF pt2)
		{
			
                Status status = GDIPlus.GdipDrawLine (
                                nativeObject, pen.nativeObject,
                                pt1.X, pt1.Y,
                                pt2.X, pt2.Y);
                                
				if (status != Status.Ok)
					throw new ArgumentException ("Error calling GDIPlus.GdipDrawLine:" +status);                                
		}

		public void DrawLine (Pen pen, Point pt1, Point pt2)
		{
            Status status =  GDIPlus.GdipDrawLineI (
                                nativeObject, pen.nativeObject,
                                pt1.X, pt1.Y,
                                pt2.X, pt2.Y);
                                
			if (status != Status.Ok)
				throw new ArgumentException ("Error calling GDIPlus.GdipDrawLineI:" +status);                                
		}

		public void DrawLine (Pen pen, int x1, int y1, int x2, int y2)
		{
			Status status = GDIPlus.GdipDrawLineI (nativeObject, pen.nativeObject, x1, y1, x2, y2);
			
			if (status != Status.Ok)
				throw new ArgumentException ("Error calling GDIPlus.GdipDrawLineI:" +status);                                
		}

		public void DrawLine (Pen pen, float x1, float y1, float x2, float y2)
		{
			Status status = GDIPlus.GdipDrawLine (nativeObject, pen.nativeObject, x1, y1, x2, y2);
			
			if (status != Status.Ok)
				throw new ArgumentException ("Error calling GDIPlus.GdipDrawLine:" +status);                                
		}

		public void DrawLines (Pen pen, PointF [] points)
		{
			Status status = GDIPlus.GdipDrawLines (nativeObject, pen.nativeObject, points, points.Length);
			
			if (status != Status.Ok)
				throw new ArgumentException ("Error calling GDIPlus.GdipDrawLines:" +status);                                
		}

		public void DrawLines (Pen pen, Point [] points)
		{
			Status status = GDIPlus.GdipDrawLinesI (nativeObject, pen.nativeObject, points, points.Length);
			
			if (status != Status.Ok)
				throw new ArgumentException ("Error calling GDIPlus.GdipDrawLinesI:" +status);                                
		}

		public void DrawPath (Pen pen, GraphicsPath path)
		{
			GDIPlus.GdipDrawPath (nativeObject, pen.nativeObject, path.nativePath);
		}

		[MonoTODO]
		public void DrawPie (Pen pen, Rectangle rect, float startAngle, float sweepAngle)
		{
			DrawPie (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}

		[MonoTODO]
		public void DrawPie (Pen pen, RectangleF rect, float startAngle, float sweepAngle)
		{
			DrawPie (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}

		[MonoTODO]
		public void DrawPie (Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			GDIPlus.GdipDrawPie (nativeObject, pen.nativeObject, x, y, width, height, startAngle, sweepAngle);
		}

		[MonoTODO]
		public void DrawPie (Pen pen, int x, int y, int width, int height, float startAngle, float sweepAngle)
		{
			GDIPlus.GdipDrawPieI (nativeObject, pen.nativeObject, x, y, width, height, startAngle, sweepAngle);
		}

		public void DrawPolygon (Pen pen, Point [] points)
		{
			GDIPlus.GdipDrawPolygonI (nativeObject, pen.nativeObject, points, points.Length);
		}

		public void DrawPolygon (Pen pen, PointF [] points)
		{
			GDIPlus.GdipDrawPolygon (nativeObject, pen.nativeObject, points, points.Length);
		}

		public void DrawRectangle (Pen pen, RectangleF rect)
		{
			DrawRectangle (pen, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void DrawRectangle (Pen pen, Rectangle rect)
		{
			DrawRectangle (pen, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void DrawRectangle (Pen pen, float x, float y, float width, float height)
		{
			GDIPlus.GdipDrawRectangle (nativeObject, pen.nativeObject, x, y, width, height);
		}

		public void DrawRectangle (Pen pen, int x, int y, int width, int height)
		{
			Console.WriteLine(" DrawRectangle (Pen pen, int x, int y, int width, int height)");
			
			Console.WriteLine("Native->" + nativeObject);
			
			Status status = GDIPlus.GdipDrawRectangleI (nativeObject, pen.nativeObject, x, y, width, height);
			
			if (status != Status.Ok)
				throw new ArgumentException ("Error calling GDIPlus.DrawRectangle:" +status);
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

		[MonoTODO("Ignores the font")]
		public void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle)
		{
			GDIPlus.GdipDrawString (nativeObject, s, s.Length, IntPtr.Zero, ref layoutRectangle, IntPtr.Zero, brush.nativeObject);
		}

		[MonoTODO("This ignores the font")]
		public void DrawString (string s, Font font, Brush brush, PointF point)
		{
			RectangleF rc = new RectangleF (point.X, point.Y, 0, 0);
			GDIPlus.GdipDrawString (nativeObject, s, s.Length, IntPtr.Zero, ref rc, IntPtr.Zero, brush.nativeObject);
		}

		[MonoTODO ("This ignores the font and format")]
		public void DrawString (string s, Font font, Brush brush, PointF point, StringFormat format)
		{
			RectangleF rc = new RectangleF (point.X, point.Y, 0, 0);
			GDIPlus.GdipDrawString (nativeObject, s, s.Length, IntPtr.Zero, ref rc, IntPtr.Zero, brush.nativeObject);
		}

		[MonoTODO ("This ignores the font and the format")]
		public void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
		{
			GDIPlus.GdipDrawString (nativeObject, s, s.Length, IntPtr.Zero, ref layoutRectangle, IntPtr.Zero, brush.nativeObject);
		}

		[MonoTODO("This ignores the font")]
		public void DrawString (string s, Font font, Brush brush, float x, float y)
		{
			Console.WriteLine("DrawString!");
			RectangleF rc = new RectangleF (x, y, 0, 0);
			
			Status status = GDIPlus.GdipDrawString (nativeObject, s, s.Length, IntPtr.Zero, 
				ref rc, IntPtr.Zero, brush.nativeObject);
			
			if (status != Status.Ok)				
				throw new ArgumentException ("Error calling GDIPlus.GdipDrawString(string s, Font font, Brush brush, float x, float y):" +s);
		}

		[MonoTODO]
		public void DrawString (string s, Font font, Brush brush, float x, float y, StringFormat format)
		{
			//throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EndContainer (GraphicsContainer container)
		{
			throw new NotImplementedException ();
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

		[MonoTODO]
		public void ExcludeClip (Rectangle rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ExcludeClip (Region region)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillClosedCurve (Brush brush, PointF [] points)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillClosedCurve (Brush brush, Point [] points)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillClosedCurve (Brush brush, PointF [] points, FillMode fillmode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillClosedCurve (Brush brush, Point [] points, FillMode fillmode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillClosedCurve (Brush brush, PointF [] points, FillMode fillmode, float tension)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillClosedCurve (Brush brush, Point [] points, FillMode fillmode, float tension)
		{
			throw new NotImplementedException ();
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
                        GDIPlus.GdipFillEllipse (nativeObject, brush.nativeObject, x, y, width, height);
		}

		public void FillEllipse (Brush brush, int x, int y, int width, int height)
		{
			GDIPlus.GdipFillEllipseI (nativeObject, brush.nativeObject, x, y, width, height);
		}

		[MonoTODO]
		public void FillPath (Brush brush, GraphicsPath path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillPie (Brush brush, Rectangle rect, float startAngle, float sweepAngle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillPie (Brush brush, int x, int y, int width, int height, int startAngle, int sweepAngle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillPie (Brush brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			throw new NotImplementedException ();
		}

		public void FillPolygon (Brush brush, PointF [] points)
		{
			GDIPlus.GdipFillPolygon2 (nativeObject, brush.nativeObject, points, points.Length);
		}

		public void FillPolygon (Brush brush, Point [] points)
		{
			GDIPlus.GdipFillPolygon2I (nativeObject, brush.nativeObject, points, points.Length);
		}

		public void FillPolygon (Brush brush, Point [] points, FillMode fillMode)
		{
			GDIPlus.GdipFillPolygonI (nativeObject, brush.nativeObject, points, points.Length, fillMode);
		}

		public void FillPolygon (Brush brush, PointF [] points, FillMode fillMode)
		{
			GDIPlus.GdipFillPolygon (nativeObject, brush.nativeObject, points, points.Length, fillMode);
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
			GDIPlus.GdipFillRectangle (nativeObject, brush.nativeObject, (float)x, (float)y, (float)width, (float)height);
		}

		public void FillRectangle (Brush brush, float x, float y, float width, float height)
		{
			GDIPlus.GdipFillRectangle (nativeObject, brush.nativeObject, x, y, width, height);
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

		[MonoTODO]
		public void FillRegion (Brush brush, Region region)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Flush ()
		{
			Flush(FlushIntention.Flush);
		}

		[MonoTODO]
		public void Flush (FlushIntention intention)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Graphics FromHdc (IntPtr hdc)
		{
			int graphics;
			GDIPlus.GdipCreateFromHDC (hdc, out graphics);
			Graphics result = new Graphics ((IntPtr)graphics);
			return result;
		}

		[MonoTODO]
		public static Graphics FromHdc (IntPtr hdc, IntPtr hdevice)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Graphics FromHdcInternal (IntPtr hdc)
		{
			throw new NotImplementedException ();
		}
		
		public static Graphics FromHwnd (IntPtr hwnd)
		{
			IntPtr graphics;
			
			Status status = GDIPlus.GdipCreateFromHWND(hwnd, out graphics);
 			
 			if (status != Status.Ok)
				throw new ArgumentException ("Error calling GDIPlus.GdipCreateFromHWND:" +status);				
			
			return new Graphics(graphics);			
		}

		[MonoTODO]
		public static Graphics FromHwndInternal (IntPtr hwnd)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Graphics FromImage (Image image)
		{
			if (image == null) throw new ArgumentException ();
			int graphics;
			GDIPlus.GdipGetImageGraphicsContext (image.nativeObject, out graphics);
			Graphics result = new Graphics ((IntPtr)graphics);
			return result;
		}

		[MonoTODO]
		public static IntPtr GetHalftonePalette ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IntPtr GetHdc ()
		{
			int hdc;
			GDIPlus.GdipGetDC (nativeObject, out hdc);
			return (IntPtr)hdc;
		}

		[MonoTODO]
		public Color GetNearestColor (Color color)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void IntersectClip (Region region)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void IntersectClip (RectangleF rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void IntersectClip (Rectangle rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsVisible (Point point)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsVisible (RectangleF rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsVisible (PointF point)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsVisible (Rectangle rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsVisible (float x, float y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsVisible (int x, int y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsVisible (float x, float y, float width, float height)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsVisible (int x, int y, int width, int height)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Region [] MeasureCharacterRanges (string text, Font font, RectangleF layoutRect, StringFormat stringFormat)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SizeF MeasureString (string text, Font font)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SizeF MeasureString (string text, Font font, SizeF layoutArea)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SizeF MeasureString (string text, Font font, int width)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat stringFormat)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SizeF MeasureString (string text, Font font, int width, StringFormat format)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SizeF MeasureString (string text, Font font, PointF origin, StringFormat stringFormat)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat stringFormat, out int charactersFitted, out int linesFilled)
		{
			throw new NotImplementedException ();
		}

		public void MultiplyTransform (Matrix matrix)
		{
			MultiplyTransform (matrix, MatrixOrder.Prepend);
		}

		public void MultiplyTransform (Matrix matrix, MatrixOrder order)
		{
			GDIPlus.GdipMultiplyWorldTransform (nativeObject, matrix.nativeMatrix, order);
		}

		[MonoTODO]
		public void ReleaseHdc (IntPtr hdc)
		{
			GDIPlus.GdipReleaseDC (nativeObject, hdc);
		}

		[MonoTODO]
		public void ReleaseHdcInternal (IntPtr hdc)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ResetClip ()
		{
			throw new NotImplementedException ();
		}

		public void ResetTransform ()
		{
			GDIPlus.GdipResetWorldTransform (nativeObject);
		}

		[MonoTODO]
		public void Restore (GraphicsState gstate)
		{
			Transform = gstate.matrix.Clone();
			GDIPlus.GdipRestoreGraphics (nativeObject, gstate.nativeState);
		}

		[MonoTODO]
		public void RotateTransform (float angle)
		{
			RotateTransform(angle, MatrixOrder.Prepend);
		}

		[MonoTODO]
		public void RotateTransform (float angle, MatrixOrder order)
		{
			//transform.Rotate(angle, order);
			GDIPlus.GdipRotateWorldTransform (nativeObject, angle, order);
		}

		[MonoTODO]
		public GraphicsState Save ()
		{
			//return implementation.Save();
			GraphicsState state = new GraphicsState();
			state.matrix = Transform.Clone();
			uint saveState;
			GDIPlus.GdipSaveGraphics (nativeObject, out saveState);
			state.nativeState = saveState;
			return state;
		}

		public void ScaleTransform (float sx, float sy)
		{
			ScaleTransform (sx, sy, MatrixOrder.Prepend);
		}

		public void ScaleTransform (float sx, float sy, MatrixOrder order)
		{
                        GDIPlus.GdipScaleWorldTransform (nativeObject, sx, sy, order);
		}

		[MonoTODO]
		public void SetClip (RectangleF rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetClip (GraphicsPath path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetClip (Rectangle rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetClip (Graphics g)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetClip (Graphics g, CombineMode combineMode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetClip (Rectangle rect, CombineMode combineMode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetClip (RectangleF rect, CombineMode combineMode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetClip (Region region, CombineMode combineMode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetClip (GraphicsPath path, CombineMode combineMode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, PointF [] pts)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, Point [] pts)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void TranslateClip (int dx, int dy)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void TranslateClip (float dx, float dy)
		{
			throw new NotImplementedException ();
		}

		public void TranslateTransform (float dx, float dy)
		{
			TranslateTransform (dx, dy, MatrixOrder.Prepend);
		}

		[MonoTODO]
		public void TranslateTransform (float dx, float dy, MatrixOrder order)
		{
			//transform.Translate(dx, dy, order);
			GDIPlus.GdipTranslateWorldTransform (nativeObject, dx, dy, order);
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
				throw new NotImplementedException ();
			}
		}

		public CompositingMode CompositingMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}

		}

		public CompositingQuality CompositingQuality {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public float DpiX {
			get {
				throw new NotImplementedException ();
			}
		}

		public float DpiY {
			get {
				throw new NotImplementedException ();
			}
		}

		public InterpolationMode InterpolationMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public bool IsClipEmpty {
			get {
				throw new NotImplementedException ();
			}
		}

		public bool IsVisibleClipEmpty {
			get {
				throw new NotImplementedException ();
			}
		}

		public float PageScale {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public GraphicsUnit PageUnit {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public PixelOffsetMode PixelOffsetMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public Point RenderingOrigin {
			get {
                                int x, y;
				GDIPlus.GdipGetRenderingOrigin (
                                        nativeObject, out x, out y);

                                return new Point (x, y);
			}

			set {
                                GDIPlus.GdipSetRenderingOrigin (nativeObject, value.X, value.Y);
			}
		}

		public SmoothingMode SmoothingMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public int TextContrast {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public TextRenderingHint TextRenderingHint {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public Matrix Transform {
			get {
                                Matrix matrix = new Matrix ();
                                GDIPlus.GdipGetWorldTransform (nativeObject, matrix.nativeMatrix);

                                return matrix;
			}
			set {
                                GDIPlus.GdipSetWorldTransform (nativeObject, value.nativeMatrix);
			}
		}

		public RectangleF VisibleClipBounds {
			get {
				throw new NotImplementedException ();
			}
		}
	}
}

