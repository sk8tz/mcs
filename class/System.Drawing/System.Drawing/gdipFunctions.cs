//
// System.Drawing.gdipFunctions.cs
//
// Author: 
// Alexandre Pigolkine (pigolkine@gmx.de)
// Jordi Mas i Hernàndez (jordi@ximian.com)
// Sanjay Gupta (gsanjay@novell.com)
//

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
//using System.Drawing;

namespace System.Drawing {
	/// <summary>
	/// GDI+ API Functions
	/// </summary>
	internal class GDIPlus {
		
		public const int FACESIZE = 32;
		public const int LANG_NEUTRAL = 0;
		
		#region gdiplus.dll functions

		// startup / shutdown
		[DllImport("gdiplus.dll")]
		static internal extern Status GdiplusStartup(ref ulong token, ref GdiplusStartupInput input, ref GdiplusStartupOutput output);
		[DllImport("gdiplus.dll")]
		static internal extern void GdiplusShutdown(ref ulong token);
		
		static ulong GdiPlusToken;
		static GDIPlus ()
		{
			GdiplusStartupInput input = GdiplusStartupInput.MakeGdiplusStartupInput();
			GdiplusStartupOutput output = GdiplusStartupOutput.MakeGdiplusStartupOutput();
			GdiplusStartup (ref GdiPlusToken, ref input, ref output);
		}
		
		// Copies a Ptr to an array of Points and releases the memory
		static public void FromUnManagedMemoryToPointI(IntPtr prt, Point [] pts)
		{						
			int nPointSize = Marshal.SizeOf(pts[0]);
			int pos = prt.ToInt32();
			for (int i=0; i<pts.Length; i++, pos+=nPointSize)
				pts[i] = (Point) Marshal.PtrToStructure((IntPtr)pos, typeof(Point));
			
			Marshal.FreeHGlobal(prt);			
		}
		
		// Copies an array of Points to unmanaged memory
		static public IntPtr FromPointToUnManagedMemoryI(Point [] pts)
		{
			int nPointSize =  Marshal.SizeOf(pts[0]);
			IntPtr dest = Marshal.AllocHGlobal(nPointSize* pts.Length);			
			int pos = dest.ToInt32();
						
			for (int i=0; i<pts.Length; i++, pos+=nPointSize)
				Marshal.StructureToPtr(pts[i], (IntPtr)pos, false);	
			
			return dest;			
		}
		
		// Copies a Ptr to an array of PointsF and releases the memory
		static public void FromUnManagedMemoryToPoint(IntPtr prt, PointF [] pts)
		{						
			int nPointSize = Marshal.SizeOf(pts[0]);
			int pos = prt.ToInt32();
			for (int i=0; i<pts.Length; i++, pos+=nPointSize)
				pts[i] = (PointF) Marshal.PtrToStructure((IntPtr)pos, typeof(PointF));
			
			Marshal.FreeHGlobal(prt);			
		}
		
		// Copies an array of Points to unmanaged memory
		static public IntPtr FromPointToUnManagedMemory(PointF [] pts)
		{
			int nPointSize =  Marshal.SizeOf(pts[0]);
			IntPtr dest = Marshal.AllocHGlobal(nPointSize* pts.Length);			
			int pos = dest.ToInt32();
						
			for (int i=0; i<pts.Length; i++, pos+=nPointSize)
				Marshal.StructureToPtr(pts[i], (IntPtr)pos, false);	
			
			return dest;			
		}

		// Converts a status into exception
		static internal void CheckStatus (Status status)
		{
			switch (status) {

				case Status.Ok:
					return;

				// TODO: Add more status code mappings here

				case Status.GenericError:
					throw new Exception ("Generic Error.");

				case Status.InvalidParameter:
					throw new ArgumentException ("Invalid Parameter.");

				case Status.OutOfMemory:
					throw new OutOfMemoryException ("Out of memory.");

				case Status.ObjectBusy:
					throw new MemberAccessException ("Object busy.");

				case Status.InsufficientBuffer:
					throw new IO.InternalBufferOverflowException ("Insufficient buffer.");

				case Status.PropertyNotSupported:
					throw new NotSupportedException ("Property not supported.");

				case Status.FileNotFound:
					throw new IO.FileNotFoundException ("File not found.");

				case Status.AccessDenied:
					throw new UnauthorizedAccessException ("Access denied.");

				case Status.UnknownImageFormat:
					throw new NotSupportedException ("Unknown image format.");

				case Status.NotImplemented:
					throw new NotImplementedException ("Feature not implemented.");

				default:
					throw new Exception ("Unknown Error.");
			}
		}

		// Memory functions
		[DllImport("gdiplus.dll")]
		static internal extern IntPtr GdipAlloc (int size);
		[DllImport("gdiplus.dll")]
		static internal extern void GdipFree (IntPtr ptr);

		
		// Brush functions
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipCloneBrush (IntPtr brush, out IntPtr clonedBrush);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipDeleteBrush (IntPtr brush);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipGetBrushType (IntPtr brush, out BrushType type);
		
		// Solid brush functions
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipCreateSolidFill (int color, out int brush);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipGetSolidFillColor (IntPtr brush, out int color);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipSetSolidFillColor (IntPtr brush, int color);
		
		// Hatch Brush functions
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipCreateHatchBrush (HatchStyle hatchstyle, int foreColor, int backColor, out IntPtr brush);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipGetHatchStyle (IntPtr brush, out HatchStyle hatchstyle);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipGetHatchForegroundColor (IntPtr brush, out int foreColor);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipGetHatchBackgroundColor (IntPtr brush, out int backColor);

		// Texture brush functions
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipGetTextureImage (IntPtr texture, out IntPtr image);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipCreateTexture (IntPtr image, WrapMode wrapMode,  out IntPtr texture);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipCreateTextureIAI (IntPtr image, IntPtr imageAttributes, int x, int y, int width, int height, out IntPtr texture);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipCreateTextureIA (IntPtr image, IntPtr imageAttributes, float x, float y, float width, float height, out IntPtr texture);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipCreateTexture2I (IntPtr image, WrapMode wrapMode, int x, int y, int width, int height, out IntPtr texture);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipCreateTexture2 (IntPtr image, WrapMode wrapMode, float x, float y, float width, float height, out IntPtr texture);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipGetTextureTransform (IntPtr texture, out IntPtr matrix);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipSetTextureTransform (IntPtr texture, IntPtr matrix);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipGetTextureWrapMode (IntPtr texture, out WrapMode wrapMode);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipSetTextureWrapMode (IntPtr texture, WrapMode wrapMode);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipMultiplyTextureTransform (IntPtr texture, IntPtr matrix, MatrixOrder order);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipResetTextureTransform (IntPtr texture);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipRotateTextureTransform (IntPtr texture, float angle, MatrixOrder order);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipScaleTextureTransform (IntPtr texture, float sx, float sy, MatrixOrder order);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipTranslateTextureTransform (IntPtr texture, float dx, float dy, MatrixOrder order);

		// Graphics functions
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipCreateFromHDC(IntPtr hDC, out int graphics);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipDeleteGraphics(IntPtr graphics);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipRestoreGraphics(IntPtr graphics, uint graphicsState);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipSaveGraphics(IntPtr graphics, out uint state);
		[DllImport("gdiplus.dll")]                
                static internal extern Status GdipMultiplyWorldTransform (IntPtr graphics, IntPtr matrix, MatrixOrder order);
                
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipRotateWorldTransform(IntPtr graphics, float angle, MatrixOrder order);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipTranslateWorldTransform(IntPtr graphics, float dx, float dy, MatrixOrder order);
		[DllImport("gdiplus.dll")]
                static internal extern Status GdipDrawArc (IntPtr graphics, IntPtr pen, float x, float y, float width, float height, float startAngle, float sweepAngle);
		[DllImport("gdiplus.dll")]
                static internal extern Status GdipDrawArcI (IntPtr graphics, IntPtr pen, int x, int y, int width, int height, float startAngle, float sweepAngle);
		[DllImport("gdiplus.dll")]
                static internal extern Status GdipDrawBezier (IntPtr graphics, IntPtr pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4);
		[DllImport("gdiplus.dll")]
                static internal extern Status GdipDrawBezierI (IntPtr graphics, IntPtr pen, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4);
		[DllImport("gdiplus.dll")]
                static internal extern Status GdipDrawEllipseI (IntPtr graphics, IntPtr pen, int x, int y, int width, int height);
		[DllImport("gdiplus.dll")]
                static internal extern Status GdipDrawEllipse (IntPtr graphics, IntPtr pen, float x, float y, float width, float height);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipDrawLine (IntPtr graphics, IntPtr pen, float x1, float y1, float x2, float y2);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipDrawLineI (IntPtr graphics, IntPtr pen, int x1, int y1, int x2, int y2);
                [DllImport ("gdiplus.dll")]
                static internal extern Status GdipDrawLines (IntPtr graphics, IntPtr pen, PointF [] points, int count);
                [DllImport ("gdiplus.dll")]
                static internal extern Status GdipDrawLinesI (IntPtr graphics, IntPtr pen, Point [] points, int count);
                [DllImport ("gdiplus.dll")]
                static internal extern Status GdipDrawPath (IntPtr graphics, IntPtr pen, IntPtr path);
                [DllImport ("gdiplus.dll")]
                static internal extern Status GdipDrawPie (IntPtr graphics, IntPtr pen, float x, float y, float width, float height, float startAngle, float sweepAngle);
                [DllImport ("gdiplus.dll")]
                static internal extern Status GdipDrawPieI (IntPtr graphics, IntPtr pen, int x, int y, int width, int height, float startAngle, float sweepAngle);
                [DllImport ("gdiplus.dll")]
                static internal extern Status GdipDrawPolygon (IntPtr graphics, IntPtr pen, PointF [] points, int count);
                [DllImport ("gdiplus.dll")]
                static internal extern Status GdipDrawPolygonI (IntPtr graphics, IntPtr pen, Point [] points, int count);
                [DllImport ("gdiplus.dll")]
                static internal extern Status GdipDrawRectangle (IntPtr graphics, IntPtr pen, float x, float y, float width, float height);
                [DllImport ("gdiplus.dll")]
                static internal extern Status GdipDrawRectangleI (IntPtr graphics, IntPtr pen, int x, int y, int width, int height);
		[DllImport("gdiplus.dll")]
                static internal extern Status GdipFillEllipseI (IntPtr graphics, IntPtr pen, int x, int y, int width, int height);
		[DllImport("gdiplus.dll")]
                static internal extern Status GdipFillEllipse (IntPtr graphics, IntPtr pen, float x, float y, float width, float height);
                [DllImport ("gdiplus.dll")]
                static internal extern  Status GdipFillPolygon (IntPtr graphics, IntPtr brush, PointF [] points, int count, FillMode fillMode);
                [DllImport ("gdiplus.dll")]
                static internal extern  Status GdipFillPolygonI (IntPtr graphics, IntPtr brush, Point [] points, int count, FillMode fillMode);
                [DllImport ("gdiplus.dll")]
                static internal extern  Status GdipFillPolygon2 (IntPtr graphics, IntPtr brush, PointF [] points, int count);
                [DllImport ("gdiplus.dll")]
                static internal extern  Status GdipFillPolygon2I (IntPtr graphics, IntPtr brush, Point [] points, int count);
                [DllImport("gdiplus.dll")]
		static internal extern Status GdipFillRectangle (IntPtr graphics, IntPtr brush, float x1, float y1, float x2, float y2);		
		[DllImport("gdiplus.dll", CharSet=CharSet.Unicode)]
		static internal extern Status GdipDrawString (IntPtr graphics, string text, int len, IntPtr font, ref RectangleF rc, IntPtr format, IntPtr brush);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipGetDC (IntPtr graphics, out int hdc);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipReleaseDC (IntPtr graphics, IntPtr hdc);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipDrawImageRectI (IntPtr graphics, IntPtr image, int x, int y, int width, int height);
		[DllImport ("gdiplus.dll")]
		static internal extern Status GdipGetRenderingOrigin (IntPtr graphics, out int x, out int y);
		[DllImport ("gdiplus.dll")]
		static internal extern Status GdipSetRenderingOrigin (IntPtr graphics, int x, int y);
 		[DllImport("gdiplus.dll")]
 		internal static extern Status GdipCloneBitmapArea (float x, float y, float width, float height, PixelFormat format, IntPtr original, out IntPtr bitmap);
 		[DllImport("gdiplus.dll")]
 		internal static extern Status GdipCloneBitmapAreaI (int x, int y, int width, int height, PixelFormat format, IntPtr original, out IntPtr bitmap);
 		[DllImport("gdiplus.dll")]
 		internal static extern Status GdipResetWorldTransform (IntPtr graphics);
 		[DllImport("gdiplus.dll")]
 		internal static extern Status GdipSetWorldTransform (IntPtr graphics, IntPtr matrix);
 		[DllImport("gdiplus.dll")]
 		internal static extern Status GdipGetWorldTransform (IntPtr graphics, IntPtr matrix);
 		[DllImport("gdiplus.dll")]
 		internal static extern Status GdipScaleWorldTransform (IntPtr graphics, float sx, float sy, MatrixOrder order); 		
 		[DllImport("gdiplus.dll")]
 		internal static extern Status GdipGraphicsClear(IntPtr graphics, int argb); 		
 		[DllImport("gdiplus.dll")]
 		internal static extern Status GdipDrawClosedCurve(IntPtr graphics, IntPtr pen, PointF [] points, int  count);
		[DllImport("gdiplus.dll")]
 		internal static extern Status GdipDrawClosedCurveI(IntPtr graphics, IntPtr pen, Point [] points, int  count); 		
 		[DllImport("gdiplus.dll")]
 		internal static extern Status GdipDrawClosedCurve2(IntPtr graphics, IntPtr pen, PointF [] points, int count, float tension);
 		[DllImport("gdiplus.dll")]
 		internal static extern Status GdipDrawClosedCurve2I(IntPtr graphics, IntPtr pen, Point [] points, int count, float tension); 		
 		[DllImport("gdiplus.dll")]
 		internal static extern Status GdipDrawCurve(IntPtr graphics, IntPtr pen, PointF [] points, int count);
		[DllImport("gdiplus.dll")]
 		internal static extern Status GdipDrawCurveI(IntPtr graphics, IntPtr pen, Point [] points, int count);
		[DllImport("gdiplus.dll")]
 		internal static extern Status GdipDrawCurve2(IntPtr graphics, IntPtr pen, PointF [] points, int count, float tension);
		[DllImport("gdiplus.dll")]
 		internal static extern Status GdipDrawCurve2I(IntPtr graphics, IntPtr pen, Point [] points, int count, float tension);
		[DllImport("gdiplus.dll")]
 		internal static extern Status GdipDrawCurve3(IntPtr graphics, IntPtr pen, PointF [] points, int count, int offset, int numberOfSegments, float tension);
		[DllImport("gdiplus.dll")]
 		internal static extern Status GdipDrawCurve3I(IntPtr graphics, IntPtr pen, Point [] points, int count, int offset, int numberOfSegments, float tension); 		
 		[DllImport("gdiplus.dll")] 		
		internal static extern Status GdipSetClipRect(IntPtr graphics, float x, float y, float width, float height, CombineMode combineMode);
		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipSetClipRectI(IntPtr graphics, int x, int y, int width, int height, CombineMode combineMode);
		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipSetClipPath(IntPtr graphics, IntPtr path, CombineMode combineMode);
		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipSetClipRegion(IntPtr graphics, IntPtr region, CombineMode combineMode);
		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipSetClipGraphics(IntPtr graphics, IntPtr srcgraphics, CombineMode combineMode);		
		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipResetClip(IntPtr graphics);		
		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipEndContainer(IntPtr graphics, int state);

		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipFillClosedCurve(IntPtr graphics, IntPtr brush, PointF[] points, int count);
                              
		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipFillClosedCurveI(IntPtr graphics, IntPtr brush, Point[] points, int count);

		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipFillClosedCurve2(IntPtr graphics, IntPtr brush, 
				          PointF[] points, int count, float tension, FillMode fillMode);

		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipFillClosedCurve2I(IntPtr graphics, IntPtr brush,
                              Point[] points, int count, float tension, FillMode fillMode);
                              
		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipFillPie(IntPtr graphics, IntPtr brush, float x, float y,
            	float width, float height, float startAngle, float sweepAngle);

		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipFillPieI(IntPtr graphics, IntPtr brush, int x, int y,
             int width, int height, float startAngle, float sweepAngle);
             
		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipFillPath(IntPtr graphics, IntPtr brush, IntPtr path);
		
		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipGetNearestColor(IntPtr graphics,  out int argb);
		
		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipIsVisiblePoint(IntPtr graphics, float x, float y, out bool result);

		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipIsVisiblePointI(IntPtr graphics, int x, int y, out bool result);

		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipIsVisibleRect(IntPtr graphics, float x, float y,
                           float width, float height, out bool result);

		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipIsVisibleRectI(IntPtr graphics, int x, int y,
                           int width, int height, out bool result);
                           
		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipTransformPoints(IntPtr graphics, CoordinateSpace destSpace,
                             CoordinateSpace srcSpace, IntPtr points,  int count);

		[DllImport("gdiplus.dll")] 	
		internal static extern Status GdipTransformPointsI(IntPtr graphics, CoordinateSpace destSpace,
                             CoordinateSpace srcSpace, IntPtr points, int count);                           
        
        [DllImport("gdiplus.dll")] 	                     
		internal static extern Status GdipTranslateClip(IntPtr graphics, float dx, float dy);
		[DllImport("gdiplus.dll")] 	                     
		internal static extern Status GdipTranslateClipI(IntPtr graphics, int dx, int dy);		
		[DllImport("gdiplus.dll")] 	                     
		internal static extern Status GdipGetClipBounds(IntPtr graphics, out RectangleF rect);		
		[DllImport("gdiplus.dll")] 	                     
		internal static extern Status GdipSetCompositingMode(IntPtr graphics, CompositingMode compositingMode);
		[DllImport("gdiplus.dll")] 	                     
		internal static extern Status GdipGetCompositingMode(IntPtr graphics, out CompositingMode compositingMode);		
		[DllImport("gdiplus.dll")] 	                     
		internal static extern Status GdipSetCompositingQuality(IntPtr graphics, CompositingQuality compositingQuality);
		[DllImport("gdiplus.dll")] 	                     
		internal static extern Status GdipGetCompositingQuality(IntPtr graphics, out CompositingQuality compositingQuality);
		[DllImport("gdiplus.dll")] 	                     
		internal static extern Status GdipSetInterpolationMode(IntPtr graphics, InterpolationMode interpolationMode);
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipGetInterpolationMode(IntPtr graphics, out InterpolationMode interpolationMode);		
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipGetDpiX(IntPtr graphics, out float dpi);
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipGetDpiY(IntPtr graphics, out float dpi);
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipIsClipEmpty(IntPtr graphics, out bool result);
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipIsVisibleClipEmpty(IntPtr graphics, out bool result);		
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipGetPageUnit(IntPtr graphics, out GraphicsUnit unit);		
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipGetPageScale(IntPtr graphics, out float scale);		
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipSetPageUnit(IntPtr graphics, GraphicsUnit unit);
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipSetPageScale(IntPtr graphics, float scale);
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipSetPixelOffsetMode(IntPtr graphics, PixelOffsetMode pixelOffsetMode);
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipGetPixelOffsetMode(IntPtr graphics, out PixelOffsetMode pixelOffsetMode);
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipSetSmoothingMode(IntPtr graphics, SmoothingMode smoothingMode);
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipGetSmoothingMode(IntPtr graphics, out SmoothingMode smoothingMode);
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipSetTextContrast(IntPtr graphics, int contrast);
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipGetTextContrast(IntPtr graphics, out int contrast);
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipSetTextRenderingHint(IntPtr graphics, TextRenderingHint mode);
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipGetTextRenderingHint(IntPtr graphics, out TextRenderingHint mode);
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipGetVisibleClipBounds(IntPtr graphics, out RectangleF rect);

				
		// Pen functions
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipCreatePen1(int argb, float width, Unit unit, out int pen);
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipCreatePen2 (IntPtr brush, float width, Unit unit, out int pen);
                [DllImport("gdiplus.dll")]
                internal static extern Status GdipClonePen (IntPtr pen, out IntPtr clonepen);
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipDeletePen(IntPtr pen);
                [DllImport("gdiplus.dll")]
                internal static extern Status GdipSetPenBrushFill (IntPtr pen, IntPtr brush);
                [DllImport("gdiplus.dll")]
                internal static extern Status GdipGetPenBrushFill (IntPtr pen, out IntPtr brush);
                [DllImport("gdiplus.dll")]
                internal static extern Status GdipSetPenColor (IntPtr pen, int color);
                [DllImport("gdiplus.dll")]                
                internal static extern Status GdipGetPenColor (IntPtr pen, out int color);
//                 [DllImport("gdiplus.dll")]
//                 internal static extern Status GdipSetPenCompoundArray (IntPtr pen, IntPtr dash, int count);
//                 [DllImport("gdiplus.dll")]
//                 internal static extern Status GdipGetPenCompoundArray (IntPtr pen, out IntPtr dash, out int count);
//                 [DllImport("gdiplus.dll")]
//                 internal static extern Status GdipGetPenCompoundArrayCount (IntPtr pen, out int count);
                [DllImport("gdiplus.dll")]
                internal static extern Status GdipSetPenDashCap (IntPtr pen, DashCap dashCap);
                [DllImport("gdiplus.dll")]
                internal static extern Status GdipGetPenDashCap (IntPtr pen, out DashCap dashCap);
                [DllImport("gdiplus.dll")]
                internal static extern Status GdipSetPenDashStyle (IntPtr pen, DashStyle dashStyle);
                [DllImport("gdiplus.dll")]
                internal static extern Status GdipGetPenDashStyle (IntPtr pen, out DashStyle dashStyle);
                [DllImport("gdiplus.dll")]
                internal static extern Status GdipSetPenDashOffset (IntPtr pen, float offset);
                [DllImport("gdiplus.dll")]
                internal static extern Status GdipGetPenDashOffset (IntPtr pen, out float offset);
                [DllImport("gdiplus.dll")]
                internal static extern Status GdipSetPenDashCount (IntPtr pen, int count);
                [DllImport("gdiplus.dll")]
                internal static extern Status GdipGetPenDashCount (IntPtr pen, out int count);
                [DllImport("gdiplus.dll")]
                internal static extern Status GdipSetPenDashArray (IntPtr pen, IntPtr dash, int count);
                [DllImport("gdiplus.dll")]
                internal static extern Status GdipGetPenDashArray (IntPtr pen, out IntPtr dash, out int count);
		[DllImport("gdiplus.dll")]
                internal static extern Status GdipSetPenMiterLimit (IntPtr pen, float miterLimit);
		[DllImport("gdiplus.dll")]
                internal static extern Status GdipGetPenMiterLimit (IntPtr pen, out float miterLimit);
		[DllImport("gdiplus.dll")]
                internal static extern Status GdipSetPenLineJoin (IntPtr pen, LineJoin lineJoin);
		[DllImport("gdiplus.dll")]
                internal static extern Status GdipGetPenLineJoin (IntPtr pen, out LineJoin lineJoin);
// 		[DllImport("gdiplus.dll")]
//                 internal static extern Status GdipSetPenLineCap197819 (IntPtr pen, LineCap startCap, LineCap endCap, DashCap dashCap);
// 		[DllImport("gdiplus.dll")]
//                 internal static extern Status GdipGetPenLineCap197819 (IntPtr pen, out LineCap startCap, out LineCap endCap, out DashCap dashCap);
		[DllImport("gdiplus.dll")]
                internal static extern Status GdipSetPenMode (IntPtr pen, PenAlignment alignment);
		[DllImport("gdiplus.dll")]
                internal static extern Status GdipGetPenMode (IntPtr pen, out PenAlignment alignment);
// 		[DllImport("gdiplus.dll")]
//                 internal static extern Status GdipSetPenStartCap (IntPtr pen, LineCap startCap);
// 		[DllImport("gdiplus.dll")]
//                 internal static extern Status GdipGetPenStartCap (IntPtr pen, out LineCap startCap);
// 		[DllImport("gdiplus.dll")]
//                 internal static extern Status GdipSetPenEndCap (IntPtr pen, LineCap endCap);
// 		[DllImport("gdiplus.dll")]
//                 internal static extern Status GdipGetPenEndCap (IntPtr pen, out LineCap endCap);
		[DllImport("gdiplus.dll")]
                internal static extern Status GdipSetPenTransform (IntPtr pen, IntPtr matrix);
		[DllImport("gdiplus.dll")]
                internal static extern Status GdipGetPenTransform (IntPtr pen, out IntPtr matrix);
		[DllImport("gdiplus.dll")]
                internal static extern Status GdipSetPenWidth (IntPtr pen, float width);
		[DllImport("gdiplus.dll")]
                internal static extern Status GdipGetPenWidth (IntPtr pen, out float width);
                
		[DllImport("gdiplus.dll")]
                internal static extern Status GdipResetPenTransform (IntPtr pen);
		[DllImport("gdiplus.dll")]
                internal static extern Status GdipMultiplyPenTransform (IntPtr pen, IntPtr matrix, MatrixOrder order);
		[DllImport("gdiplus.dll")]
                internal static extern Status GdipRotatePenTransform (IntPtr pen, float angle, MatrixOrder order);
		[DllImport("gdiplus.dll")]
                internal static extern Status GdipScalePenTransform (IntPtr pen, float sx, float sy, MatrixOrder order);
		[DllImport("gdiplus.dll")]
                internal static extern Status GdipTranslatePenTransform (IntPtr pen, float dx, float dy, MatrixOrder order);

		[DllImport("gdiplus.dll")]
		internal static extern Status GdipCreateFromHWND (IntPtr hwnd, out IntPtr graphics);
		
		// Bitmap functions
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipCreateBitmapFromScan0 (int width, int height, int stride, PixelFormat format, IntPtr scan0, out IntPtr bmp);

		[DllImport("gdiplus.dll")]
		internal static extern Status GdipCreateBitmapFromGraphics (int width, int height, IntPtr target, out IntPtr bitmap);

		[DllImport("gdiplus.dll")]
		internal static extern Status GdipBitmapLockBits (IntPtr bmp, ref Rectangle rc, ImageLockMode flags, PixelFormat format, [In, Out] IntPtr bmpData);
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipBitmapSetResolution(IntPtr bmp, float xdpi, float ydpi);

		
		// This an internal GDIPlus Cairo function, not part GDIPlus interface
		//[DllImport("gdiplus.dll")]
		//(internal static extern Status ____BitmapLockBits (IntPtr bmp, ref GpRect  rc, ImageLockMode flags, PixelFormat format, ref int width, ref int height, ref int stride, ref int format2, ref int reserved, ref IntPtr scan0);
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipBitmapUnlockBits (IntPtr bmp, [In,Out] BitmapData bmpData);
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipBitmapGetPixel (IntPtr bmp, int x, int y, out int argb); 
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipBitmapSetPixel (IntPtr bmp, int x, int y, int argb);

		// Image functions
		[DllImport("gdiplus.dll", CharSet=CharSet.Auto)]
		internal static extern Status GdipLoadImageFromFile ( [MarshalAs(UnmanagedType.LPWStr)] string filename, out IntPtr image );
 
		[DllImport("gdiplus.dll", CharSet=CharSet.Auto)]
		internal static extern Status GdipLoadImageFromFileICM ( [MarshalAs(UnmanagedType.LPWStr)] string filename, out IntPtr image );
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipCreateBitmapFromHBITMAP ( IntPtr hBitMap, IntPtr gdiPalette, out IntPtr image );
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipDisposeImage ( IntPtr image );

		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetImageFlags ( IntPtr image, out uint flag );

		[DllImport("gdiplus.dll")]
		internal static extern Status GdipImageGetFrameDimensionsCount ( IntPtr image, out uint count );
												   
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipImageGetFrameDimensionsList ( IntPtr image, out IntPtr dimensionIDs, uint count );
 
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetImageHeight ( IntPtr image, out uint height );
												   
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetImageHorizontalResolution ( IntPtr image, out float resolution );

		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetImagePaletteSize ( IntPtr image, out int size );
												   
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetImagePalette ( IntPtr image, out ColorPalette palette, int size );

		[DllImport("gdiplus.dll")]
		internal static extern Status GdipSetImagePalette ( IntPtr image, ColorPalette palette );
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetImageDimension ( IntPtr image, out float width, out float height );
												   
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetImagePixelFormat ( IntPtr image, out PixelFormat format );

		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetPropertyCount ( IntPtr image, out uint propNumbers );
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetPropertyIdList ( IntPtr image, uint propNumbers, out IntPtr list);
 
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetPropertySize ( IntPtr image, out uint bufferSize, out uint propNumbers );
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetAllPropertyItems ( IntPtr image, uint bufferSize, uint propNumbers, out IntPtr items );
												   
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetImageRawFormat ( IntPtr image, out Guid format );

		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetImageVerticalResolution ( IntPtr image, out float resolution );
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetImageWidth ( IntPtr image, out uint width);
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetImageBounds ( IntPtr image, out RectangleF source, ref GraphicsUnit unit );

		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetEncoderParameterListSize ( IntPtr image, IntPtr encoder, out uint size );

		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetEncoderParameterList ( IntPtr image, IntPtr encoder, uint size, out IntPtr buffer );
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipImageGetFrameCount ( IntPtr image, IntPtr guidDimension, out uint count );
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipImageSelectActiveFrame ( IntPtr image, Guid guidDimension, uint frameIndex );
		
        [DllImport("gdiplus.dll")]
		internal static extern Status GdipGetPropertyItemSize ( IntPtr image, int propertyID, out uint propertySize );

		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetPropertyItem ( IntPtr image, int propertyID, uint propertySize, out IntPtr buffer );
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipRemovePropertyItem ( IntPtr image, int propertyId );
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipSetPropertyItem ( IntPtr image, IntPtr propertyItem );
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetImageThumbnail ( IntPtr image, uint width, uint height, out IntPtr thumbImage, IntPtr callback, IntPtr callBackData );
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipImageRotateFlip ( IntPtr image, RotateFlipType rotateFlipType );
		
		[DllImport("gdiplus.dll", CharSet=CharSet.Auto)]
		internal static extern Status GdipSaveImageToFile ( IntPtr image, [MarshalAs(UnmanagedType.LPWStr)] string filename, IntPtr encoderClsID, IntPtr encoderParameters );
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipSaveAdd ( IntPtr image, IntPtr encoderParameters );
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipDrawImageI (IntPtr graphics, IntPtr image, int x, int y);
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetImageGraphicsContext (IntPtr image, out int graphics);		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipDrawImage (IntPtr graphics, IntPtr image, float x, float y);
		
		[DllImport("gdiplus.dll")]	
		internal static extern Status GdipBeginContainer (IntPtr graphics,  RectangleF dstrect,
                   RectangleF srcrect, GraphicsUnit unit, out int  state);

		[DllImport("gdiplus.dll")]	
		internal static extern Status GdipBeginContainerI (IntPtr graphics, Rectangle dstrect,
                    Rectangle srcrect, GraphicsUnit unit, out int state);

		[DllImport("gdiplus.dll")]	
		internal static extern Status GdipBeginContainer2 (IntPtr graphics, out int state); 
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipDrawImagePoints (IntPtr graphics, IntPtr image, PointF [] destPoints, int count);

		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipDrawImagePointsI (IntPtr graphics, IntPtr image,  Point [] destPoints, int count);
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipDrawImageRectRectI (IntPtr graphics, IntPtr image,
                                int dstx, int dsty, int dstwidth, int dstheight,
                       		int srcx, int srcy, int srcwidth, int srcheight,
                       		GraphicsUnit srcUnit, IntPtr imageattr, IntPtr callback, int callbackData);                      		
                       		
		[DllImport("gdiplus.dll")]                       		
		internal static extern Status GdipDrawImageRect(IntPtr graphics, IntPtr image, float x, float y, float width, float height);
		[DllImport("gdiplus.dll")]                       		
		internal static extern Status GdipDrawImagePointRect(IntPtr graphics, IntPtr image, float x,
                                float y, float srcx, float srcy, float srcwidth, float srcheight, GraphicsUnit srcUnit);
                                
		[DllImport("gdiplus.dll")]                       		
		internal static extern Status GdipCreateStringFormat(StringFormatFlags formatAttributes,  int language, out IntPtr native);
		
		[DllImport("gdiplus.dll")]		
		internal static extern Status GdipCreateHBITMAPFromBitmap (IntPtr bmp, out IntPtr HandleBmp, int clrbackground);
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipCreateHICONFromBitmap (IntPtr bmp, out IntPtr HandleIcon);
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipCreateBitmapFromHICON (IntPtr  hicon,  out IntPtr bitmap);
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipCreateBitmapFromResource (IntPtr hInstance,
                                string lpBitmapName, out IntPtr bitmap);

                // Matrix functions
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipCreateMatrix (out IntPtr matrix);
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipCreateMatrix2 (float m11, float m12, float m21, float m22, float dx, float dy, out IntPtr matrix);
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipCreateMatrix3 (RectangleF rect, PointF[] dstplg, out IntPtr matrix);
                [DllImport ("gdiplus.dll")]                
                internal static extern Status GdipCreateMatrix3I (Rectangle rect, Point[] dstplg, out IntPtr matrix);
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipDeleteMatrix (IntPtr matrix);

                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipCloneMatrix (IntPtr matrix, out IntPtr cloneMatrix);
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipSetMatrixElements (IntPtr matrix, float m11, float m12, float m21, float m22, float dx, float dy);
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipGetMatrixElements (IntPtr matrix, IntPtr matrixOut);
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipMultiplyMatrix (IntPtr matrix, IntPtr matrix2, MatrixOrder order);
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipTranslateMatrix (IntPtr matrix, float offsetX, float offsetY, MatrixOrder order);
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipScaleMatrix (IntPtr matrix, float scaleX, float scaleY, MatrixOrder order);
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipRotateMatrix (IntPtr matrix, float angle, MatrixOrder order);

                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipShearMatrix (IntPtr matrix, float shearX, float shearY, MatrixOrder order);

                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipInvertMatrix (IntPtr matrix);
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipTransformMatrixPoints (IntPtr matrix, PointF[] pts, int count);
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipTransformMatrixPointsI (IntPtr matrix, Point[] pts, int count);                
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipVectorTransformMatrixPoints (IntPtr matrix, PointF[] pts, int count);
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipVectorTransformMatrixPointsI (IntPtr matrix, Point[] pts, int count);
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipIsMatrixInvertible (IntPtr matrix, out bool result);

                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipIsMatrixIdentity (IntPtr matrix, out bool result);
                [DllImport ("gdiplus.dll")]                
                internal static extern Status GdipIsMatrixEqual (IntPtr matrix, IntPtr matrix2, out bool result);

                // GraphicsPath functions
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipCreatePath (FillMode brushMode, out IntPtr path);
                [DllImport ("gdiplus.dll")]                
                internal static extern Status GdipCreatePath2 (PointF points, byte [] types, int count, FillMode brushMode, out IntPtr path);
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipClonePath (IntPtr path, out IntPtr clonePath);
                [DllImport ("gdiplus.dll")]                
                internal static extern Status GdipDeletePath (IntPtr path);
                [DllImport ("gdiplus.dll")]                
                internal static extern Status GdipResetPath (IntPtr path);
                [DllImport ("gdiplus.dll")]                
                internal static extern Status GdipGetPointCount (IntPtr path, out int count);
                [DllImport ("gdiplus.dll")]                
                internal static extern Status GdipGetPathTypes (IntPtr path, [Out] byte [] types, int count);
                [DllImport ("gdiplus.dll")]                
                internal static extern Status GdipGetPathPoints (IntPtr path, [Out] PointF [] points, int count);
                [DllImport ("gdiplus.dll")]                
                internal static extern Status GdipGetPathPointsI (IntPtr path, [Out] Point [] points, int count);
                [DllImport ("gdiplus.dll")]                                
                internal static extern Status GdipGetPathFillMode (IntPtr path, out FillMode fillMode);
                [DllImport ("gdiplus.dll")]                                                
                internal static extern Status GdipSetPathFillMode (IntPtr path, FillMode fillMode);
                [DllImport ("gdiplus.dll")]                                                
                internal static extern Status GdipGetPathData (IntPtr path, out IntPtr pathData);
                [DllImport ("gdiplus.dll")]                                                
                internal static extern Status GdipStartPathFigure (IntPtr path);
                [DllImport ("gdiplus.dll")]                                                
                internal static extern Status GdipClosePathFigure (IntPtr path);
                [DllImport ("gdiplus.dll")]                                                
                internal static extern Status GdipClosePathFigures (IntPtr path);
                [DllImport ("gdiplus.dll")]                                                
                internal static extern Status GdipSetPathMarker (IntPtr path);
                [DllImport ("gdiplus.dll")]                                                
                internal static extern Status GdipClearPathMarkers (IntPtr path);
                [DllImport ("gdiplus.dll")]                                                
                internal static extern Status GdipReversePath (IntPtr path);
                [DllImport ("gdiplus.dll")]                                                
                internal static extern Status GdipGetPathLastPoint (IntPtr path, out PointF lastPoint);
                [DllImport ("gdiplus.dll")]                                                
                internal static extern Status GdipAddPathLine (IntPtr path, float x1, float y1, float x2, float y2);
                [DllImport ("gdiplus.dll")]                                                
                internal static extern Status GdipAddPathArc (IntPtr path, float x, float y, float width, float height, float startAngle, float sweepAngle);
                [DllImport ("gdiplus.dll")]                                                                
                internal static extern Status GdipAddPathBezier (IntPtr path, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4);
                [DllImport ("gdiplus.dll")]                                                                
                internal static extern Status GdipAddPathBeziers (IntPtr path, PointF [] points, int count);
                [DllImport ("gdiplus.dll")]                                                                
                internal static extern Status GdipAddPathCurve (IntPtr path, PointF [] points, int count);
                [DllImport ("gdiplus.dll")]                                                                
                internal static extern Status GdipAddPathRectangle (IntPtr path, float x, float y, float width, float height);
                [DllImport ("gdiplus.dll")]                                                                
                internal static extern Status GdipAddPathRectangles (IntPtr path, RectangleF [] rects, int count);
                [DllImport ("gdiplus.dll")]                                                                
                internal static extern Status GdipAddPathEllipse (IntPtr path, float x, float y, float width, float height);
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipAddPathEllipseI (IntPtr path, int x, int y, int width, int height);
                [DllImport ("gdiplus.dll")]                                                                
                internal static extern Status GdipAddPathPie (IntPtr path, float x, float y, float width, float height, float startAngle, float sweepAngle);
                [DllImport ("gdiplus.dll")]                                                                
                internal static extern Status GdipAddPathPolygon (IntPtr path, PointF [] points, int count);
                [DllImport ("gdiplus.dll")]                                                                
                internal static extern Status GdipAddPathPath (IntPtr path, IntPtr addingPath, bool connect);
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipAddPathLineI (IntPtr path, int x1, int y1, int x2, int y2);
                [DllImport ("gdiplus.dll")]                                                
                internal static extern Status GdipAddPathArcI (IntPtr path, int x, int y, int width, int height, float startAngle, float sweepAngle);
                
                [DllImport ("gdiplus.dll")]                
                internal static extern Status GdipAddPathBezierI (IntPtr path, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4);
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipAddPathBeziersI (IntPtr path, Point [] points, int count);
                                
                [DllImport ("gdiplus.dll")]                
                internal static extern Status GdipAddPathPolygonI (IntPtr path, Point [] points, int count);
                [DllImport ("gdiplus.dll")]                
                internal static extern Status GdipAddPathRectangleI (IntPtr path, int x, int y, int width, int height);
                [DllImport ("gdiplus.dll")]                
                internal static extern Status GdipAddPathRectanglesI (IntPtr path, Rectangle [] rects, int count);
                [DllImport ("gdiplus.dll")]
                internal static extern Status GdipTransformPath (IntPtr path, IntPtr matrix);
                [DllImport ("gdiplus.dll")]                
                internal static extern Status GdipGetPathWorldBoundsI (IntPtr path, IntPtr bounds, IntPtr matrix, IntPtr pen);
                [DllImport ("gdiplus.dll")]                
                internal static extern Status GdipIsVisiblePathPoint (IntPtr path, float x, float y, IntPtr graphics, out bool result);
                [DllImport ("gdiplus.dll")]                
                internal static extern Status GdipIsVisiblePathPointI (IntPtr path, int x, int y, IntPtr graphics, out bool result); 
                [DllImport ("gdiplus.dll")]                
                internal static extern Status GdipIsOutlineVisiblePathPoint (IntPtr path, float x, float y, IntPtr graphics, out bool result);
                [DllImport ("gdiplus.dll")]                
                internal static extern Status GdipIsOutlineVisiblePathPointI (IntPtr path, int x, int y, IntPtr graphics, out bool result); 
                
		// ImageAttributes
		[DllImport ("gdiplus.dll")]     
		internal static extern Status GdipCreateImageAttributes (out IntPtr imageattr);
				
		[DllImport ("gdiplus.dll")]     
		internal static extern Status GdipSetImageAttributesColorKeys (IntPtr imageattr,
                                ColorAdjustType type, bool enableFlag, int colorLow, int colorHigh);
                                
                [DllImport ("gdiplus.dll")]     
                internal static extern Status GdipDisposeImageAttributes (IntPtr imageattr);
				
                [DllImport ("gdiplus.dll")]     
                internal static extern Status GdipSetImageAttributesColorMatrix (IntPtr imageattr,
                                ColorAdjustType type, bool enableFlag, ColorMatrix colorMatrix,
                                ColorMatrix grayMatrix,  ColorMatrixFlag flags);                                
                               
		// Font		
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipCreateFont (IntPtr fontFamily, float emSize, FontStyle style, GraphicsUnit  unit,  out IntPtr font);
		[DllImport("gdiplus.dll")]                   
		internal static extern Status GdipDeleteFont (IntPtr font);		

		// FontCollection
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetFontCollectionFamilyCount (IntPtr collection, out int found);
		
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetFontCollectionFamilyList (IntPtr collection, int getCount, IntPtr dest, out int retCount);
		//internal static extern Status GdipGetFontCollectionFamilyList( IntPtr collection, int getCount, [Out] FontFamily[] familyList, out int retCount );
		
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipNewInstalledFontCollection (out IntPtr collection);
		
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipNewPrivateFontCollection (out IntPtr collection);
		
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipDeletePrivateFontCollection (IntPtr collection);
		
		[DllImport ("gdiplus.dll", CharSet=CharSet.Auto)]
		internal static extern Status GdipPrivateAddFontFile (IntPtr collection,
                                [MarshalAs (UnmanagedType.LPWStr)] string fileName );
		
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipPrivateAddMemoryFont (IntPtr collection, IntPtr mem, int length);

		//FontFamily
		[DllImport ("gdiplus.dll", CharSet=CharSet.Auto)]
		internal static extern Status GdipCreateFontFamilyFromName (
                        [MarshalAs(UnmanagedType.LPWStr)] string fName, IntPtr collection, out IntPtr fontFamily);

		[DllImport ("gdiplus.dll", CharSet=CharSet.Unicode)]
		internal static extern Status GdipGetFamilyName(IntPtr family, StringBuilder fName, int language);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetGenericFontFamilySansSerif (out IntPtr fontFamily);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetGenericFontFamilySerif (out IntPtr fontFamily);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetGenericFontFamilyMonospace (out IntPtr fontFamily);
		
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetCellAscent (IntPtr fontFamily, int style, out uint ascent);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetCellDescent (IntPtr fontFamily, int style, out uint descent);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetLineSpacing (IntPtr fontFamily, int style, out uint spacing);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetEmHeight (IntPtr fontFamily, int style, out uint emHeight);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipIsStyleAvailable (IntPtr fontFamily, int style, out bool styleAvailable);

		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipDeleteFontFamily (IntPtr fontFamily);
		
		// String Format
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipCreateStringFormat(int formatAttributes, int language, out IntPtr  format);
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipStringFormatGetGenericDefault(out IntPtr format);
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipStringFormatGetGenericTypographic(out IntPtr format);
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipDeleteStringFormat(IntPtr format);
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipCloneStringFormat(IntPtr srcformat, out IntPtr format);
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipSetStringFormatFlags(IntPtr format, StringFormatFlags flags);
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetStringFormatFlags(IntPtr format, out StringFormatFlags flags);
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipSetStringFormatAlign(IntPtr format, StringAlignment align);
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetStringFormatAlign(IntPtr format, out StringAlignment align);
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipSetStringFormatLineAlign(IntPtr format, StringAlignment align);
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetStringFormatLineAlign(IntPtr format, out StringAlignment align);
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipSetStringFormatTrimming(IntPtr format, StringTrimming trimming);
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetStringFormatTrimming(IntPtr format, out StringTrimming trimming);
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipSetStringFormatHotkeyPrefix(IntPtr format, HotkeyPrefix hotkeyPrefix);
		[DllImport ("gdiplus.dll")]
		internal static extern Status GdipGetStringFormatHotkeyPrefix(IntPtr format, out HotkeyPrefix hotkeyPrefix);
		
		
		
#endregion      
	}               
}               
