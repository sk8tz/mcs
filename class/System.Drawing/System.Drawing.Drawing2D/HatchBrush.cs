//
// System.Drawing.Drawing2D.HatchBrush.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Ravindra (rkumar@novell.com)
//
// (C) 2002/3 Ximian, Inc
// (C) 2004  Novell, Inc.
//
using System;

namespace System.Drawing.Drawing2D 
{
	/// <summary>
	/// Summary description for HatchBrush.
	/// </summary>
	public sealed class HatchBrush : Brush 
	{

		internal HatchBrush (IntPtr ptr) : base (ptr)
		{
		}

		public HatchBrush (HatchStyle hatchStyle, Color foreColor)
					: this (hatchStyle, foreColor, Color.Black)
		{
		}

		public HatchBrush(HatchStyle hatchStyle, Color foreColor, Color backColor)
		{
			Status status = GDIPlus.GdipCreateHatchBrush (hatchStyle, foreColor.ToArgb (), backColor.ToArgb (), out nativeObject);
			GDIPlus.CheckStatus (status);
		}

		public Color BackgroundColor {
			get {
				int argb;
				Status status = GDIPlus.GdipGetHatchBackgroundColor (nativeObject, out argb);
				GDIPlus.CheckStatus (status);
				return Color.FromArgb (argb);
			}
		}

		public Color ForegroundColor {
			get {
				int argb;
				Status status = GDIPlus.GdipGetHatchForegroundColor (nativeObject, out argb);
				GDIPlus.CheckStatus (status);
				return Color.FromArgb (argb);
			}
		}

		public HatchStyle HatchStyle {
			get {
				HatchStyle hatchStyle;
				Status status = GDIPlus.GdipGetHatchStyle (nativeObject, out hatchStyle);
				GDIPlus.CheckStatus (status);
				return hatchStyle;
			}
		}

		public override object Clone ()
		{
			return new HatchBrush (NativeObject);
		}

	}
}
