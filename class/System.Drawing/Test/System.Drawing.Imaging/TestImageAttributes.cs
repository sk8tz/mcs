//
// Copyright (C) 2005-2006 Novell, Inc (http://www.novell.com)
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
//
// Author:
//   Jordi Mas i Hernandez (jordi@ximian.com)
//

using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;
using System.IO;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;

namespace MonoTests.System.Drawing.Imaging {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class ImageAttributesTest {

		private static Color ProcessColorMatrix (Color color, ColorMatrix colorMatrix)
		{
			using (Bitmap bmp = new Bitmap (64, 64)) {
				using (Graphics gr = Graphics.FromImage (bmp)) {
					ImageAttributes imageAttr = new ImageAttributes ();
					bmp.SetPixel (0,0, color);
					imageAttr.SetColorMatrix (colorMatrix);
					gr.DrawImage (bmp, new Rectangle (0, 0, 64, 64), 0, 0, 64, 64, GraphicsUnit.Pixel, imageAttr);		
					return bmp.GetPixel (0,0);
				}
			}
		}


		// Text Color Matrix processing
		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void ColorMatrix1 ()
		{			
			Color clr_src, clr_rslt;
			
			ColorMatrix cm = new ColorMatrix (new float[][] {
				new float[] 	{2,	0,	0, 	0, 	0}, //R
				new float[] 	{0,	1,	0, 	0, 	0}, //G
				new float[] 	{0,	0,	1, 	0, 	0}, //B
				new float[] 	{0,	0,	0, 	1, 	0}, //A
				new float[] 	{0.2f,	0,	0, 	0, 	0}, //Translation
			  });

			clr_src = Color.FromArgb (255, 100, 20, 50);
			clr_rslt = ProcessColorMatrix (clr_src, cm);

			Assert.AreEqual (Color.FromArgb (255, 251, 20, 50), clr_rslt, "Color");
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void ColorMatrix2 ()
		{
			Color clr_src, clr_rslt;

			ColorMatrix cm = new ColorMatrix (new float[][] {
				new float[] 	{1,	0,	0, 	0, 	0}, //R
				new float[] 	{0,	1,	0, 	0, 	0}, //G
				new float[] 	{0,	0,	1.5f, 	0, 	0}, //B
				new float[] 	{0,	0,	0.5f, 	1, 	0}, //A
				new float[] 	{0,	0,	0, 	0, 	0}, //Translation
			  });

			clr_src = Color.FromArgb (255, 100, 40, 25);
			clr_rslt = ProcessColorMatrix (clr_src, cm);
			Assert.AreEqual (Color.FromArgb (255, 100, 40, 165), clr_rslt, "Color");
		}

		private void Bug80323 (Color c)
		{
			// test case from bug #80323
			ColorMatrix cm = new ColorMatrix (new float[][] {
				new float[] 	{1,	0,	0, 	0, 	0}, //R
				new float[] 	{0,	1,	0, 	0, 	0}, //G
				new float[] 	{0,	0,	1, 	0, 	0}, //B
				new float[] 	{0,	0,	0, 	0.5f, 	0}, //A
				new float[] 	{0,	0,	0, 	0, 	1}, //Translation
			  });

			using (SolidBrush sb = new SolidBrush (c)) {
				using (Bitmap bmp = new Bitmap (100, 100)) {
					using (Graphics gr = Graphics.FromImage (bmp)) {
						gr.FillRectangle (Brushes.White, 0, 0, 100, 100);
						gr.FillEllipse (sb, 0, 0, 100, 100);
					}
					using (Bitmap b = new Bitmap (200, 100)) {
						using (Graphics g = Graphics.FromImage (b)) {
							g.FillRectangle (Brushes.White, 0, 0, 200, 100);

							ImageAttributes ia = new ImageAttributes ();
							ia.SetColorMatrix (cm);
							g.DrawImage (bmp, new Rectangle (0, 0, 100, 100), 0, 0, 100, 100, GraphicsUnit.Pixel, null);
							g.DrawImage (bmp, new Rectangle (100, 0, 100, 100), 0, 0, 100, 100, GraphicsUnit.Pixel, ia);
						}
						b.Save (String.Format ("80323-{0}.png", c.ToArgb ().ToString ("X")));
						Assert.AreEqual (Color.FromArgb (255, 255, 155, 155), b.GetPixel (50, 50), "50,50");
						Assert.AreEqual (Color.FromArgb (255, 255, 205, 205), b.GetPixel (150, 50), "150,50");
					}
				}
			}
		}
	
		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void ColorMatrix_80323_UsingAlpha ()
		{
			Bug80323 (Color.FromArgb (100, 255, 0, 0));
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void ColorMatrix_80323_WithoutAlpha ()
		{
			// this color is identical, once drawn over the bitmap, to Color.FromArgb (100, 255, 0, 0)
			Bug80323 (Color.FromArgb (255, 255, 155, 155));
		}
	}
}
