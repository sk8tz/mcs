 //
// System.Drawing.SolidBrush.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Alexandre Pigolkine(pigolkine@gmx.de)
//
// (C) 2002 Ximian, Inc
//
using System;

namespace System.Drawing.Cairo {

        internal class SolidBrushFactory : ISolidBrushFactory
        {
                ISolidBrush ISolidBrushFactory.SolidBrush (Color color)
                {
                        return new SolidBrush(color);
                }
        }

        internal class SolidBrush : Brush, ISolidBrush 
        {
		
                Color color;

                public SolidBrush (Color color)
                {
                        this.Color = color;
                }

                public Color Color {

                        get { return color; }

                        set { color = value; }
                }
		
                public override object Clone ()
                {
                        return new SolidBrush (color);
                }
		
                Color IBrush.TextColor 
                {
                        get { return Color; }
                }
			
                internal void initialize (IntPtr cr)
                {
                        Cairo.cairo_set_rgb_color (cr,
                                        (double) color.R,
                                        (double) color.G,
                                        (double) color.B);
                }
        }
}
