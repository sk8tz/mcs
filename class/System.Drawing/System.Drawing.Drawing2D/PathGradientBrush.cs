//
// System.Drawing.Drawing2D.PathGradientBrush.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Ravindra (rkumar@novell.com)
//
// (C) 2002/3 Ximian, Inc
// (C) 2004, Novell, Inc.
//

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Drawing.Drawing2D
{
	/// <summary>
	/// Summary description for PathGradientBrush.
	/// </summary>
	public sealed class PathGradientBrush : Brush
	{
		Blend blend;
		Color centerColor;
		PointF center;
		PointF focus;
		RectangleF rectangle;
		Color[] surroundColors;
		ColorBlend interpolationColors;
		Matrix transform;
		WrapMode wrapMode;
		
		internal PathGradientBrush (IntPtr native) : base (native)
		{
		}

		public PathGradientBrush (GraphicsPath path)
		{
			Status status = GDIPlus.GdipCreatePathGradientFromPath (path.NativeObject, out nativeObject);
			GDIPlus.CheckStatus (status);
 
//			IntPtr rect;
			status = GDIPlus.GdipGetPathGradientRect (nativeObject, out rectangle);
			GDIPlus.CheckStatus (status);
//			rectangle = (RectangleF) Marshal.PtrToStructure (rect, typeof (RectangleF));
		}

		public PathGradientBrush (Point[] points) : this (points, WrapMode.Clamp)
		{
		}

		public PathGradientBrush (PointF[] points) : this (points, WrapMode.Clamp)
		{
		}

		public PathGradientBrush (Point[] points, WrapMode wrapMode)
		{
			Status status = GDIPlus.GdipCreatePathGradientI (points, points.Length, wrapMode, out nativeObject);
			GDIPlus.CheckStatus (status);

//			IntPtr rect;
			status = GDIPlus.GdipGetPathGradientRect (nativeObject, out rectangle);
			GDIPlus.CheckStatus (status);
//			rectangle = (RectangleF) Marshal.PtrToStructure (rect, typeof (RectangleF));
		}

		public PathGradientBrush (PointF[] points, WrapMode wrapMode)
		{
			Status status = GDIPlus.GdipCreatePathGradient (points, points.Length, wrapMode, out nativeObject);
			GDIPlus.CheckStatus (status);

//			IntPtr rect;
			status = GDIPlus.GdipGetPathGradientRect (nativeObject, out rectangle);
			GDIPlus.CheckStatus (status);
//			rectangle = (RectangleF) Marshal.PtrToStructure (rect, typeof (RectangleF));
		}

		// properties
		public Blend Blend {
			get {
				return blend;
			}
			set {
				Status status = GDIPlus.GdipSetPathGradientBlend (nativeObject, value.Factors, value.Positions, value.Factors.Length);
				GDIPlus.CheckStatus (status);
				blend = value;
			}
		}

		public Color CenterColor {
			get {
				return centerColor;
			}
			set {
				Status status = GDIPlus.GdipSetPathGradientCenterColor (nativeObject, value.ToArgb ());
				GDIPlus.CheckStatus (status);
				centerColor = value;
			}
		}

		public PointF CenterPoint {
			get {
				return center;
			}
			set {
				Status status = GDIPlus.GdipSetPathGradientCenterPoint (nativeObject, value);
				GDIPlus.CheckStatus (status);
				center = value;
			}
		}

		public PointF FocusScales {
			get {
				return focus;
			}
			set {
				Status status = GDIPlus.GdipSetPathGradientFocusScales (nativeObject, value.X, value.Y);
				GDIPlus.CheckStatus (status);
				focus = value;
			}
		}

		public ColorBlend InterpolationColors {
			get {
				return interpolationColors;
			}
			set {
				Color[] colors = value.Colors;
				int[] blend = new int [colors.Length];
				for (int i = 0; i < colors.Length; i++)
					blend [i] = colors [i].ToArgb ();

				Status status = GDIPlus.GdipSetPathGradientPresetBlend (nativeObject, blend, value.Positions, blend.Length);
				GDIPlus.CheckStatus (status);
				interpolationColors = value;
			}
		}

		public RectangleF Rectangle {
			get {
				
				return rectangle;
			}
		}

		public Color[] SurroundColors {
			get {
				return surroundColors;
			}
			set {
				int[] colors = new int [value.Length];
				for (int i = 0; i < value.Length; i++)
					colors [i] = value [i].ToArgb ();

				Status status = GDIPlus.GdipSetPathGradientSurroundColorsWithCount (nativeObject, colors, colors.Length);
				GDIPlus.CheckStatus (status);
				surroundColors = value;
			}
		}

		public Matrix Transform {
			get {
				return transform;
			}
			set {

				Status status = GDIPlus.GdipSetPathGradientTransform (nativeObject, value.nativeMatrix);
				GDIPlus.CheckStatus (status);
				transform = value;
			}
		}

		public WrapMode WrapMode {
			get {
				return wrapMode;
			}
			set {
				Status status = GDIPlus.GdipSetPathGradientWrapMode (nativeObject, value);
				GDIPlus.CheckStatus (status);
				wrapMode = value;
			}
		}

		//methods
		public override object Clone ()
		{
			PathGradientBrush clone = new PathGradientBrush (nativeObject);
			clone.blend = this.blend;
			clone.centerColor = this.centerColor;
			clone.center = this.center;
			clone.focus = this.focus;
			clone.rectangle = this.rectangle;
			clone.surroundColors = this.surroundColors;
			clone.interpolationColors = this.interpolationColors;
			clone.transform = this.transform;
			clone.wrapMode = this.wrapMode;

			return clone;
		}
	}
}
