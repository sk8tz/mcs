//
// Bitmap class testing unit
//
// Authors:
// 	Jordi Mas i Hernàndez (jmas@softcatala.org>
//	Jonathan Gilbert <logic@deltaq.org>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004,2006-2007 Novell, Inc (http://www.novell.com)
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
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Xml.Serialization;
using NUnit.Framework;

namespace MonoTests.System.Drawing {

	[TestFixture]
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
#if TARGET_JVM
	[Category ("NotWorking")]
#endif
	public class TestBitmap {
		
		[Test]
		public void TestPixels() 
		{		
			// Tests GetSetPixel/SetPixel			
			Bitmap bmp= new Bitmap(100,100, PixelFormat.Format32bppRgb);
			bmp.SetPixel(0,0,Color.FromArgb(255,128,128,128));					
			Color color = bmp.GetPixel(0,0);				
						
			Assert.AreEqual (Color.FromArgb(255,128,128,128), color);
			
			bmp.SetPixel(99,99,Color.FromArgb(255,255,0,155));					
			Color color2 = bmp.GetPixel(99,99);										
			Assert.AreEqual (Color.FromArgb(255,255,0,155), color2);			
		}

		[Test]
		public void LockBits_32_32_NonIndexedWrite ()
		{
			using (Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format32bppRgb)) {
				Rectangle rect = new Rectangle (0, 0, bmp.Width, bmp.Height);
				BitmapData data = bmp.LockBits (rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
				Assert.AreEqual (100, data.Height, "Height");
				Assert.AreEqual (PixelFormat.Format32bppRgb, data.PixelFormat, "PixelFormat");
				Assert.AreEqual (400, data.Stride, "Stride");
				Assert.AreEqual (100, data.Width, "Width");
				bmp.UnlockBits (data);
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void LockBits_32_24_NonIndexedWrite ()
		{
			using (Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format32bppRgb)) {
				Rectangle rect = new Rectangle (0, 0, bmp.Width, bmp.Height);
				BitmapData data = bmp.LockBits (rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
				Assert.AreEqual (100, data.Height, "Height");
				Assert.AreEqual (PixelFormat.Format24bppRgb, data.PixelFormat, "PixelFormat");
				Assert.AreEqual (300, data.Stride, "Stride");
				Assert.AreEqual (100, data.Width, "Width");
				bmp.UnlockBits (data);
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void LockBits_24_24_NonIndexedWrite ()
		{
			using (Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format24bppRgb)) {
				Rectangle rect = new Rectangle (0, 0, bmp.Width, bmp.Height);
				BitmapData data = bmp.LockBits (rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
				Assert.AreEqual (100, data.Height, "Height");
				Assert.AreEqual (PixelFormat.Format24bppRgb, data.PixelFormat, "PixelFormat");
				Assert.AreEqual (300, data.Stride, "Stride");
				Assert.AreEqual (100, data.Width, "Width");
				bmp.UnlockBits (data);
			}
		}

		[Test]
		public void LockBits_24_32_NonIndexedWrite ()
		{
			using (Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format24bppRgb)) {
				Rectangle rect = new Rectangle (0, 0, bmp.Width, bmp.Height);
				BitmapData data = bmp.LockBits (rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
				Assert.AreEqual (100, data.Height, "Height");
				Assert.AreEqual (PixelFormat.Format32bppRgb, data.PixelFormat, "PixelFormat");
				Assert.AreEqual (400, data.Stride, "Stride");
				Assert.AreEqual (100, data.Width, "Width");
				bmp.UnlockBits (data);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void LockBits_IndexedWrite_NonIndexed ()
		{
			using (Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format8bppIndexed)) {
				Rectangle rect = new Rectangle (0, 0, bmp.Width, bmp.Height);
				bmp.LockBits (rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void LockBits_NonIndexedWrite_ToIndexed ()
		{
			using (Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format32bppRgb)) {
				Rectangle rect = new Rectangle (0, 0, bmp.Width, bmp.Height);
				bmp.LockBits (rect, ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
			}
		}

		[Test]
		public void LockBits_IndexedWrite_SameIndexedFormat ()
		{
			using (Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format8bppIndexed)) {
				Rectangle rect = new Rectangle (0, 0, bmp.Width, bmp.Height);
				BitmapData data = bmp.LockBits (rect, ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
				Assert.AreEqual (100, data.Height, "Height");
				Assert.AreEqual (PixelFormat.Format8bppIndexed, data.PixelFormat, "PixelFormat");
				Assert.AreEqual (100, data.Stride, "Stride");
				Assert.AreEqual (100, data.Width, "Width");
				bmp.UnlockBits (data);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void LockBits_Disposed ()
		{
			Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format32bppRgb);
			Rectangle rect = new Rectangle (0, 0, bmp.Width, bmp.Height);
			bmp.Dispose ();
			bmp.LockBits (rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void UnlockBits_Disposed ()
		{
			Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format32bppRgb);
			Rectangle rect = new Rectangle (0, 0, bmp.Width, bmp.Height);
			BitmapData data = bmp.LockBits (rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
			bmp.Dispose ();
			bmp.UnlockBits (data);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void UnlockBits_Null ()
		{
			using (Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format32bppRgb)) {
				bmp.UnlockBits (null);
			}
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (ArgumentException))]
#if TARGET_JVM
		[Ignore ("Bitmap.LockBits is not implemented")]
#endif
		public void LockBits_BitmapData_Null ()
		{
#if !TARGET_JVM
			using (Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format32bppRgb)) {
				Rectangle rect = new Rectangle (0, 0, bmp.Width, bmp.Height);
				bmp.LockBits (rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb, null);
			}
#endif
		}

		[Test]
#if TARGET_JVM
		[Ignore ("Bitmap.LockBits is not implemented")]
#endif
		public void LockBits_32_32_BitmapData ()
		{
#if !TARGET_JVM
			BitmapData data = new BitmapData ();
			using (Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format32bppRgb)) {
				Rectangle rect = new Rectangle (0, 0, bmp.Width, bmp.Height);
				bmp.LockBits (rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb, data);
				Assert.AreEqual (100, data.Height, "Height");
				Assert.AreEqual (PixelFormat.Format32bppRgb, data.PixelFormat, "PixelFormat");
				Assert.AreEqual (400, data.Stride, "Stride");
				Assert.AreEqual (100, data.Width, "Width");
				bmp.UnlockBits (data);
			}
#endif
		}

		[Test]
#if TARGET_JVM
		[Ignore ("Bitmap.LockBits is not implemented")]
#else
		[Category ("NotWorking")]
#endif
		public void LockBits_32_24_BitmapData ()
		{
#if !TARGET_JVM
			BitmapData data = new BitmapData ();
			using (Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format32bppRgb)) {
				Rectangle rect = new Rectangle (0, 0, bmp.Width, bmp.Height);
				bmp.LockBits (rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb, data);
				Assert.AreEqual (100, data.Height, "Height");
				Assert.AreEqual (PixelFormat.Format24bppRgb, data.PixelFormat, "PixelFormat");
				Assert.AreEqual (300, data.Stride, "Stride");
				Assert.AreEqual (100, data.Width, "Width");
				bmp.UnlockBits (data);
			}
#endif
		}

		[Test]
#if TARGET_JVM
		[Ignore ("Bitmap.LockBits is not implemented")]
#else
		[Category ("NotWorking")]
#endif
		public void LockBits_24_24_BitmapData ()
		{
#if !TARGET_JVM
			BitmapData data = new BitmapData ();
			using (Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format24bppRgb)) {
				Rectangle rect = new Rectangle (0, 0, bmp.Width, bmp.Height);
				bmp.LockBits (rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb, data);
				Assert.AreEqual (100, data.Height, "Height");
				Assert.AreEqual (PixelFormat.Format24bppRgb, data.PixelFormat, "PixelFormat");
				Assert.AreEqual (300, data.Stride, "Stride");
				Assert.AreEqual (100, data.Width, "Width");
				bmp.UnlockBits (data);
			}
#endif
		}

		[Test]
#if TARGET_JVM
		[Ignore ("Bitmap.LockBits is not implemented")]
#endif
		public void LockBits_24_32_BitmapData ()
		{
#if !TARGET_JVM
			BitmapData data = new BitmapData ();
			using (Bitmap bmp = new Bitmap (100, 100, PixelFormat.Format24bppRgb)) {
				Rectangle rect = new Rectangle (0, 0, bmp.Width, bmp.Height);
				bmp.LockBits (rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb, data);
				Assert.AreEqual (100, data.Height, "Height");
				Assert.AreEqual (PixelFormat.Format32bppRgb, data.PixelFormat, "PixelFormat");
				Assert.AreEqual (400, data.Stride, "Stride");
				Assert.AreEqual (100, data.Width, "Width");
				bmp.UnlockBits (data);
			}
#endif
		}
#endif

		/* Get the output directory depending on the runtime and location*/
		public static string getOutSubDir()
		{				
			string sSub, sRslt;			
			
			if (Environment.GetEnvironmentVariable("MSNet")==null)
				sSub = "mono/";
			else
				sSub = "MSNet/";			
			
			sRslt = Path.GetFullPath (sSub);
				
			if (Directory.Exists(sRslt) == 	false) 
				sRslt = "Test/System.Drawing/" + sSub;							
			
			if (sRslt.Length > 0)
				if (sRslt[sRslt.Length-1] != '\\' && sRslt[sRslt.Length-1] != '/')
					sRslt += "/";					
			
			return sRslt;
		}
		
		/* Get the input directory depending on the runtime*/
		public static string getInFile(string file)
		{				
			string sRslt = Path.GetFullPath ("../System.Drawing/" + file);
			if (!File.Exists (sRslt))
				sRslt = "Test/System.Drawing/" + file;
			return sRslt;
		}

		// note: this test fails when saving (for the same reason) on Mono and MS.NET
		//[Test]
		public void MakeTransparent() 
		{
			string sInFile =   getInFile("bitmaps/maketransparent.bmp");
			string sOutFile =  getOutSubDir() + "transparent.bmp";
						
			Bitmap	bmp = new Bitmap(sInFile);
					
			bmp.MakeTransparent();
			bmp.Save(sOutFile);							
			
			Color color = bmp.GetPixel(1,1);							
			Assert.AreEqual (Color.Black.R, color.R);											
			Assert.AreEqual (Color.Black.G, color.G);											
			Assert.AreEqual (Color.Black.B, color.B);										
		}
		
		[Test]
		public void Clone()
		{
			string sInFile = getInFile ("bitmaps/almogaver24bits.bmp");
			Rectangle rect = new Rectangle(0,0,50,50);						
			Bitmap	bmp = new Bitmap(sInFile);			
			
			Bitmap bmpNew = bmp.Clone (rect, PixelFormat.Format32bppArgb);									
			Color colororg0 = bmp.GetPixel(0,0);		
			Color colororg50 = bmp.GetPixel(49,49);					
			Color colornew0 = bmpNew.GetPixel(0,0);		
			Color colornew50 = bmpNew.GetPixel(49,49);				
			
			Assert.AreEqual (colororg0, colornew0);											
			Assert.AreEqual (colororg50, colornew50);				
		}	
		
		[Test]
		public void CloneImage()
		{
			string sInFile = getInFile ("bitmaps/almogaver24bits.bmp");			
			Bitmap	bmp = new Bitmap(sInFile);			
			
			Bitmap bmpNew = (Bitmap) bmp.Clone ();			
			
			Assert.AreEqual (bmp.Width, bmpNew.Width);
			Assert.AreEqual (bmp.Height, bmpNew.Height);		
			Assert.AreEqual (bmp.PixelFormat, bmpNew.PixelFormat);
			
		}	

		[Test]
		public void Frames()
		{
			string sInFile = getInFile ("bitmaps/almogaver24bits.bmp");			
			Bitmap	bmp = new Bitmap(sInFile);						
			int cnt = bmp.GetFrameCount(FrameDimension.Page);			
			int active = bmp.SelectActiveFrame (FrameDimension.Page, 0);
			
			Assert.AreEqual (1, cnt);								
			Assert.AreEqual (0, active);											
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void FileDoesNotExists ()
		{			
			Bitmap	bmp = new Bitmap ("FileDoesNotExists.jpg");			
		}

		static string ByteArrayToString(byte[] arrInput)
		{
			int i;
			StringBuilder sOutput = new StringBuilder(arrInput.Length);
			for (i=0;i < arrInput.Length -1; i++) 
			{
				sOutput.Append(arrInput[i].ToString("X2"));
			}
			return sOutput.ToString();
		}


		public string RotateBmp (Bitmap src, RotateFlipType rotate)
		{			
			int width = 150, height = 150, index = 0;			
			byte[] pixels = new byte [width * height * 3];
			Bitmap bmp_rotate;
			byte[] hash;
			Color clr;

			bmp_rotate = src.Clone (new RectangleF (0,0, width, height), PixelFormat.Format32bppArgb);	
			bmp_rotate.RotateFlip (rotate);			

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					clr = bmp_rotate.GetPixel (x,y);
					pixels[index++] = clr.R; pixels[index++] = clr.G; pixels[index++]  = clr.B;	
				}				
			}
		
			hash = new MD5CryptoServiceProvider().ComputeHash (pixels);
			return ByteArrayToString (hash);
		}
#if !TARGET_JVM
		public string RotateIndexedBmp (Bitmap src, RotateFlipType type)
		{
			int pixels_per_byte;

			switch (src.PixelFormat)
			{
				case PixelFormat.Format1bppIndexed: pixels_per_byte = 8; break;
				case PixelFormat.Format4bppIndexed: pixels_per_byte = 2; break;
				case PixelFormat.Format8bppIndexed: pixels_per_byte = 1; break;

				default: throw new Exception("Cannot pass a bitmap of format " + src.PixelFormat + " to RotateIndexedBmp");
			}

			Bitmap test = src.Clone () as Bitmap;

			test.RotateFlip (type);

			BitmapData data = null;
			byte[] pixel_data;

			try
			{
				data = test.LockBits (new Rectangle (0, 0, test.Width, test.Height), ImageLockMode.ReadOnly, test.PixelFormat);

				int scan_size = (data.Width + pixels_per_byte - 1) / pixels_per_byte;
				pixel_data = new byte[data.Height * scan_size];

				for (int y=0; y < data.Height; y++) {
					IntPtr src_ptr = (IntPtr)(y * data.Stride + data.Scan0.ToInt64 ());
					int dest_offset = y * scan_size;
					for (int x=0; x < scan_size; x++)
						pixel_data[dest_offset + x] = Marshal.ReadByte (src_ptr, x);
				}
			}
			finally
			{
				if (test != null) {
					if (data != null)
						try { test.UnlockBits(data); } catch {}

					try { test.Dispose(); } catch {}
				}
			}

			if (pixel_data == null)
				return "--ERROR--";

			byte[] hash = new MD5CryptoServiceProvider().ComputeHash (pixel_data);
			return ByteArrayToString (hash);
		}
#endif		
		
		
		/*
			Rotate bitmap in diffent ways, and check the result
			pixels using MD5
		*/
		[Test]
		public void Rotate()
		{
			string sInFile = getInFile ("bitmaps/almogaver24bits.bmp");	
			Bitmap	bmp = new Bitmap(sInFile);
			
			Assert.AreEqual ("312958A3C67402E1299413794988A3", RotateBmp (bmp, RotateFlipType.Rotate90FlipNone));	
			Assert.AreEqual ("BF70D8DA4F1545AEDD77D0296B47AE", RotateBmp (bmp, RotateFlipType.Rotate180FlipNone));
			Assert.AreEqual ("15AD2ADBDC7090C0EC744D0F7ACE2F", RotateBmp (bmp, RotateFlipType.Rotate270FlipNone));
			Assert.AreEqual ("2E10FEC1F4FD64ECC51D7CE68AEB18", RotateBmp (bmp, RotateFlipType.RotateNoneFlipX));
			Assert.AreEqual ("E63204779B566ED01162B90B49BD9E", RotateBmp (bmp, RotateFlipType.Rotate90FlipX));
			Assert.AreEqual ("B1ECB17B5093E13D04FF55CFCF7763", RotateBmp (bmp, RotateFlipType.Rotate180FlipX));
			Assert.AreEqual ("71A173882C16755D86F4BC26532374", RotateBmp (bmp, RotateFlipType.Rotate270FlipX));

		}

#if !TARGET_JVM
		/*
			Rotate 1- and 4-bit bitmaps in different ways and check the
			resulting pixels using MD5
		*/
		[Test]
		public void Rotate1bit4bit()
		{
			if ((Environment.OSVersion.Platform != (PlatformID)4)
			 && (Environment.OSVersion.Platform != (PlatformID)128))
				Assert.Ignore("This does not work with Microsoft's GDIPLUS.DLL due to off-by-1 errors in their GdipBitmapRotateFlip function.");

			string[] files = {
			                   getInFile ("bitmaps/1bit.png"),
			                   getInFile ("bitmaps/4bit.png")
			                 };

			StringBuilder md5s = new StringBuilder();

			foreach (string file in files)
				using (Bitmap bmp = new Bitmap(file))
					foreach (RotateFlipType type in Enum.GetValues (typeof(RotateFlipType)))
						md5s.Append (RotateIndexedBmp (bmp, type));

			using (StreamWriter writer = new StreamWriter("/tmp/md5s.txt"))
				writer.WriteLine(md5s);

			Assert.AreEqual (
				"A4DAF507C92BDE10626BC7B34FEFE5" + // 1-bit RotateNoneFlipNone
				"A4DAF507C92BDE10626BC7B34FEFE5" + // 1-bit Rotate180FlipXY
				"C0975EAFD2FC1CC9CC7AF20B92FC9F" + // 1-bit Rotate90FlipNone
				"C0975EAFD2FC1CC9CC7AF20B92FC9F" + // 1-bit Rotate270FlipXY
				"64AE60858A02228F7B1B18C7812FB6" + // 1-bit Rotate180FlipNone
				"64AE60858A02228F7B1B18C7812FB6" + // 1-bit RotateNoneFlipXY
				"E96D3390938350F9DE2608C4364424" + // 1-bit Rotate270FlipNone
				"E96D3390938350F9DE2608C4364424" + // 1-bit Rotate90FlipXY
				"23947CE822C1DDE6BEA69C01F8D0D9" + // 1-bit RotateNoneFlipX
				"23947CE822C1DDE6BEA69C01F8D0D9" + // 1-bit Rotate180FlipY
				"BE45F685BDEBD7079AA1B2CBA46723" + // 1-bit Rotate90FlipX
				"BE45F685BDEBD7079AA1B2CBA46723" + // 1-bit Rotate270FlipY
				"353E937CFF31B1BF6C3DD0A031ACB5" + // 1-bit Rotate180FlipX
				"353E937CFF31B1BF6C3DD0A031ACB5" + // 1-bit RotateNoneFlipY
				"AEA18A770A845E25B6A8CE28DD6DCB" + // 1-bit Rotate270FlipX
				"AEA18A770A845E25B6A8CE28DD6DCB" + // 1-bit Rotate90FlipY
				"3CC874B571902366AACED5D619E87D" + // 4-bit RotateNoneFlipNone
				"3CC874B571902366AACED5D619E87D" + // 4-bit Rotate180FlipXY
				"8DE25C7E1BE4A3B535DB5D83198D83" + // 4-bit Rotate90FlipNone
				"8DE25C7E1BE4A3B535DB5D83198D83" + // 4-bit Rotate270FlipXY
				"27CF5E9CE70BE9EBC47FB996721B95" + // 4-bit Rotate180FlipNone
				"27CF5E9CE70BE9EBC47FB996721B95" + // 4-bit RotateNoneFlipXY
				"A919CCB8F97CAD7DC1F01026D11A5D" + // 4-bit Rotate270FlipNone
				"A919CCB8F97CAD7DC1F01026D11A5D" + // 4-bit Rotate90FlipXY
				"545876C99ACF833E69FBFFBF436034" + // 4-bit RotateNoneFlipX
				"545876C99ACF833E69FBFFBF436034" + // 4-bit Rotate180FlipY
				"5DB56687757CDEFC52D89C77CA9223" + // 4-bit Rotate90FlipX
				"5DB56687757CDEFC52D89C77CA9223" + // 4-bit Rotate270FlipY
				"05A77EDDCDF20D5B0AC0169E95D7D7" + // 4-bit Rotate180FlipX
				"05A77EDDCDF20D5B0AC0169E95D7D7" + // 4-bit RotateNoneFlipY
				"B6B6245796C836923ABAABDF368B29" + // 4-bit Rotate270FlipX
				"B6B6245796C836923ABAABDF368B29",  // 4-bit Rotate90FlipY
				md5s.ToString ());
		}
		
		public void LockBmp (PixelFormat fmt, PixelFormat fmtlock, string output, 
			int lwidth , int lheight, ref string hash1, ref string hash2)
		{			
			int width = 100, height = 100, bbps, cur, pos;
			Bitmap	bmp = new Bitmap (width, height, fmt);										
			Graphics gr = Graphics.FromImage (bmp);			
			byte[] hash;
			Color clr;
			byte[] btv = new byte[1];   						
			int y, x, len = width * height * 4, index = 0;			
			byte[] pixels = new byte [len];
			hash1 = hash2 ="";
			
			bbps = Image.GetPixelFormatSize (fmt);			
				 
			Pen p = new Pen (Color.FromArgb (255, 100, 200, 250), 2);				
			gr.DrawRectangle(p, 1.0F, 1.0F, 80.0F, 80.0F);				
			
			BitmapData bd = bmp.LockBits (new Rectangle (0, 0, lwidth, lheight), ImageLockMode.ReadOnly,  fmtlock);
			
			pos = bd.Scan0.ToInt32();			
			for (y = 0; y < bd.Height; y++) {			
				for (x = 0; x < bd.Width; x++) {
					
					/* Read the pixels*/
					for (int bt =0; bt < bbps/8; bt++, index++) {
						cur = pos;
						cur+= y * bd.Stride;
						cur+= x * bbps/8;					
						cur+= bt;					
						Marshal.Copy ((IntPtr)cur, btv, 0, 1);
						pixels[index] = btv[0];
						
						/* Make change of all the colours = 250 to 10*/						
						if (btv[0] == 250) {
							btv[0] = 10;
							Marshal.Copy (btv, 0, (IntPtr)cur, 1);
						}
					}
				}
			}					
			
			for (int i = index; i < len; i++)
				pixels[index] = 0;
		
			hash = new MD5CryptoServiceProvider().ComputeHash (pixels);			
			bmp.UnlockBits (bd);							
						
			hash1 = ByteArrayToString (hash);
			
			/* MD5 of the changed bitmap*/
			for (y = 0, index = 0; y < height; y++) {
				for (x = 0; x < width; x++) {				
					clr = bmp.GetPixel (x,y);
					pixels[index++] = clr.R; pixels[index++] = clr.G; pixels[index++]  = clr.B;	
				}				
			}
			
			hash = new MD5CryptoServiceProvider().ComputeHash (pixels);						
			hash2 = ByteArrayToString (hash);
			
			/*bmp.Save (output, ImageFormat.Bmp);*/
		}
		/*
			Tests the LockBitmap functions. Makes a hash of the block of pixels that it returns
			firsts, changes them, and then using GetPixel does another check of the changes.
			The results match the .Net framework
		*/
		[Test]
		[Category ("NotWorking")]
		public void LockBitmap ()
		{	
			string hash = "";		
			string hashchg = "";				
							
			/* Locks the whole bitmap*/			
			LockBmp (PixelFormat.Format32bppArgb, PixelFormat.Format32bppArgb, "output32bppArgb.bmp", 100, 100, ref hash, ref hashchg);				
			Assert.AreEqual ("AF5BFD4E98D6708FF4C9982CC9C68F", hash);			
			Assert.AreEqual ("BBEE27DC85563CB58EE11E8951230F", hashchg);			
			
			LockBmp (PixelFormat.Format32bppPArgb, PixelFormat.Format32bppPArgb, "output32bppPArgb.bmp", 100, 100, ref hash, ref hashchg);			
			Assert.AreEqual ("AF5BFD4E98D6708FF4C9982CC9C68F", hash);			
			Assert.AreEqual ("BBEE27DC85563CB58EE11E8951230F", hashchg);			
			
			LockBmp (PixelFormat.Format32bppRgb, PixelFormat.Format32bppRgb, "output32bppRgb.bmp", 100, 100, ref hash, ref hashchg);
			Assert.AreEqual ("AF5BFD4E98D6708FF4C9982CC9C68F", hash);			
			Assert.AreEqual ("BBEE27DC85563CB58EE11E8951230F", hashchg);		
			
			LockBmp (PixelFormat.Format24bppRgb, PixelFormat.Format24bppRgb, "output24bppRgb.bmp", 100, 100, ref hash, ref hashchg);
			Assert.AreEqual ("A8A071D0B3A3743905B4E193A62769", hash);			
			Assert.AreEqual ("EEE846FA8F892339C64082DFF775CF", hashchg);					
			
			/* Locks a portion of the bitmap*/		
			LockBmp (PixelFormat.Format32bppArgb, PixelFormat.Format32bppArgb, "output32bppArgb.bmp", 50, 50, ref hash, ref hashchg);				
			Assert.AreEqual ("C361FBFD82A4F3C278605AE9EC5385", hash);			
			Assert.AreEqual ("8C2C04B361E1D5875EE8ACF5073F4E", hashchg);			
			
			LockBmp (PixelFormat.Format32bppPArgb, PixelFormat.Format32bppPArgb, "output32bppPArgb.bmp", 50, 50, ref hash, ref hashchg);			
			Assert.AreEqual ("C361FBFD82A4F3C278605AE9EC5385", hash);			
			Assert.AreEqual ("8C2C04B361E1D5875EE8ACF5073F4E", hashchg);			
		
			LockBmp (PixelFormat.Format32bppRgb, PixelFormat.Format32bppRgb, "output32bppRgb.bmp", 50, 50, ref hash, ref hashchg);
			Assert.AreEqual ("C361FBFD82A4F3C278605AE9EC5385", hash);			
			Assert.AreEqual ("8C2C04B361E1D5875EE8ACF5073F4E", hashchg);			
			
			LockBmp (PixelFormat.Format24bppRgb, PixelFormat.Format24bppRgb, "output24bppRgb.bmp", 50, 50, ref hash, ref hashchg);
			Assert.AreEqual ("FFE86628478591D1A1EB30E894C34F", hash);			
			Assert.AreEqual ("8C2C04B361E1D5875EE8ACF5073F4E", hashchg);				
						
		}

		/*
			Tests the LockBitmap and UnlockBitmap functions, specifically the copying
			of bitmap data in the directions indicated by the ImageLockMode.
		*/
		[Test]
		public void LockUnlockBitmap()
		{
			BitmapData data;
			int pixel_value;
			Color pixel_colour;

			Color red  = Color.FromArgb (Color.Red.A,  Color.Red.R,  Color.Red.G,  Color.Red.B);
			Color blue = Color.FromArgb (Color.Blue.A, Color.Blue.R, Color.Blue.G, Color.Blue.B);

			using (Bitmap bmp = new Bitmap (1, 1, PixelFormat.Format32bppRgb))
			{
				bmp.SetPixel (0, 0, red);
				pixel_colour = bmp.GetPixel (0, 0);
				Assert.AreEqual (red, pixel_colour, "Set/Get-Red");

				data = bmp.LockBits (new Rectangle (0, 0, 1, 1), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

				// Marshal follows CPU endianess
				if (BitConverter.IsLittleEndian) {
					pixel_value = Marshal.ReadInt32 (data.Scan0);
				} else {
					pixel_value = Marshal.ReadByte (data.Scan0, 0);
					pixel_value |= Marshal.ReadByte (data.Scan0, 1) << 8;
					pixel_value |= Marshal.ReadByte (data.Scan0, 2) << 16;
					pixel_value |= Marshal.ReadByte (data.Scan0, 3) << 24;
				}
				pixel_colour = Color.FromArgb (pixel_value);

				// Disregard alpha information in the test
				pixel_colour = Color.FromArgb(red.A, pixel_colour.R, pixel_colour.G, pixel_colour.B);

				Assert.AreEqual (red, pixel_colour, "Red-FromLockedBitmap");

				Marshal.WriteInt32 (data.Scan0, blue.ToArgb ());

				bmp.UnlockBits (data);

				pixel_colour = bmp.GetPixel (0, 0);

				// Disregard alpha information in the test
				pixel_colour = Color.FromArgb(red.A, pixel_colour.R, pixel_colour.G, pixel_colour.B);

				Assert.AreEqual (red, pixel_colour);

				data = bmp.LockBits (new Rectangle (0, 0, 1, 1), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

				Marshal.WriteInt32 (data.Scan0, blue.ToArgb ());

				bmp.UnlockBits (data);

				pixel_colour = bmp.GetPixel (0, 0);

				// Disregard alpha information in the test
				pixel_colour = Color.FromArgb(blue.A, pixel_colour.R, pixel_colour.G, pixel_colour.B);

				Assert.AreEqual (blue, pixel_colour);
			}

			using (Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format32bppArgb))
			{
				bmp.SetPixel (0, 0, red);

				data = bmp.LockBits (new Rectangle (0, 0, 1, 1), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

				int r, g, b;

				b = Marshal.ReadByte (data.Scan0, 0);
				g = Marshal.ReadByte (data.Scan0, 1);
				r = Marshal.ReadByte (data.Scan0, 2);
				pixel_colour = Color.FromArgb (red.A, r, g, b);

				Assert.AreEqual (red, pixel_colour);

				Marshal.WriteByte (data.Scan0, 0, blue.B);
				Marshal.WriteByte (data.Scan0, 1, blue.G);
				Marshal.WriteByte (data.Scan0, 2, blue.R);

				bmp.UnlockBits (data);

				pixel_colour = bmp.GetPixel (0, 0);

				// Disregard alpha information in the test
				pixel_colour = Color.FromArgb(red.A, pixel_colour.R, pixel_colour.G, pixel_colour.B);

				Assert.AreEqual (red, bmp.GetPixel (0, 0));

				data = bmp.LockBits (new Rectangle (0, 0, 1, 1), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

				Marshal.WriteByte (data.Scan0, 0, blue.B);
				Marshal.WriteByte (data.Scan0, 1, blue.G);
				Marshal.WriteByte (data.Scan0, 2, blue.R);

				bmp.UnlockBits(data);

				pixel_colour = bmp.GetPixel (0, 0);

				// Disregard alpha information in the test
				pixel_colour = Color.FromArgb(blue.A, pixel_colour.R, pixel_colour.G, pixel_colour.B);

				Assert.AreEqual (blue, bmp.GetPixel (0, 0));
			}
		}
#endif		
		[Test]
		public void DefaultFormat1 ()
		{
			using (Bitmap bmp = new Bitmap (20, 20)) {
				Assert.AreEqual (ImageFormat.MemoryBmp, bmp.RawFormat);
			}
		}

		[Test]
		public void DefaultFormat2 ()
		{
			string filename =  Path.GetTempFileName ();
			using (Bitmap bmp = new Bitmap (20, 20)) {
				bmp.Save (filename);
			}

			using (Bitmap other = new Bitmap (filename)) {
				Assert.AreEqual (ImageFormat.Png, other.RawFormat);
			}
			File.Delete (filename);
		}

		[Test]
		public void BmpDataStride1 ()
		{
			Bitmap bmp = new Bitmap (184, 184, PixelFormat.Format1bppIndexed);
			BitmapData data = bmp.LockBits (new Rectangle (0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
			try {
				Assert.AreEqual (24, data.Stride);
			} finally {
				bmp.UnlockBits (data);
				bmp.Dispose ();
			}
		}

		private Stream Serialize (object o)
		{
			MemoryStream ms = new MemoryStream ();
			IFormatter formatter = new BinaryFormatter ();
			formatter.Serialize (ms, o);
			ms.Position = 0;
			return ms;
		}

		private object Deserialize (Stream s)
		{
			return new BinaryFormatter ().Deserialize (s);
		}

		[Test]
		public void Serialize_Icon ()
		{
			// this cause a problem with resgen, see http://bugzilla.ximian.com/show_bug.cgi?id=80565
			string filename = getInFile ("bitmaps/16x16x16.ico");
			using (Bitmap icon = new Bitmap (filename)) {
				using (Stream s = Serialize (icon)) {
					using (Bitmap copy = (Bitmap)Deserialize (s)) {
						Assert.AreEqual (icon.Height, copy.Height, "Height");
						Assert.AreEqual (icon.Width, copy.Width, "Width");
						Assert.AreEqual (icon.PixelFormat, copy.PixelFormat, "PixelFormat");
						Assert.IsTrue (icon.RawFormat.Equals (ImageFormat.Icon), "Icon");
						Assert.IsTrue (copy.RawFormat.Equals (ImageFormat.Png), "Png");
					}
				}
			}
		}

		private Stream SoapSerialize (object o)
		{
			MemoryStream ms = new MemoryStream ();
			IFormatter formatter = new SoapFormatter ();
			formatter.Serialize (ms, o);
			ms.Position = 0;
			return ms;
		}

		private object SoapDeserialize (Stream s)
		{
			return new SoapFormatter ().Deserialize (s);
		}

		[Test]
		public void SoapSerialize_Icon ()
		{
			string filename = getInFile ("bitmaps/16x16x16.ico");
			using (Bitmap icon = new Bitmap (filename)) {
				using (Stream s = SoapSerialize (icon)) {
					using (Bitmap copy = (Bitmap) SoapDeserialize (s)) {
						Assert.AreEqual (icon.Height, copy.Height, "Height");
						Assert.AreEqual (icon.Width, copy.Width, "Width");
						Assert.AreEqual (icon.PixelFormat, copy.PixelFormat, "PixelFormat");
						Assert.AreEqual (16, icon.Palette.Entries.Length, "icon Palette");
						Assert.IsTrue (icon.RawFormat.Equals (ImageFormat.Icon), "Icon");
						Assert.AreEqual (0, copy.Palette.Entries.Length, "copy Palette");
						Assert.IsTrue (copy.RawFormat.Equals (ImageFormat.Png), "Png");
					}
				}
			}
		}

		[Test]
		public void SoapSerialize_Bitmap8 ()
		{
			string filename = getInFile ("bitmaps/almogaver8bits.bmp");
			using (Bitmap bmp = new Bitmap (filename)) {
				using (Stream s = SoapSerialize (bmp)) {
					using (Bitmap copy = (Bitmap) SoapDeserialize (s)) {
						Assert.AreEqual (bmp.Height, copy.Height, "Height");
						Assert.AreEqual (bmp.Width, copy.Width, "Width");
						Assert.AreEqual (bmp.PixelFormat, copy.PixelFormat, "PixelFormat");
						Assert.AreEqual (256, copy.Palette.Entries.Length, "Palette");
						Assert.AreEqual (bmp.RawFormat, copy.RawFormat, "RawFormat");
					}
				}
			}
		}

		[Test]
		public void SoapSerialize_Bitmap24 ()
		{
			string filename = getInFile ("bitmaps/almogaver24bits.bmp");
			using (Bitmap bmp = new Bitmap (filename)) {
				using (Stream s = SoapSerialize (bmp)) {
					using (Bitmap copy = (Bitmap) SoapDeserialize (s)) {
						Assert.AreEqual (bmp.Height, copy.Height, "Height");
						Assert.AreEqual (bmp.Width, copy.Width, "Width");
						Assert.AreEqual (bmp.PixelFormat, copy.PixelFormat, "PixelFormat");
						Assert.AreEqual (bmp.Palette.Entries.Length, copy.Palette.Entries.Length, "Palette");
						Assert.AreEqual (bmp.RawFormat, copy.RawFormat, "RawFormat");
					}
				}
			}
		}

		[Test]
#if NET_2_0
		[Category ("NotWorking")]	// http://bugzilla.ximian.com/show_bug.cgi?id=80558
#else
		[ExpectedException (typeof (InvalidOperationException))]
#endif
		public void XmlSerialize ()
		{
			new XmlSerializer (typeof (Bitmap));
		}

		static int[] palette1 = {
			-16777216,
			-1,
		};

		[Test]
		public void Format1bppIndexed_Palette ()
		{
			using (Bitmap bmp = new Bitmap (1, 1, PixelFormat.Format1bppIndexed)) {
				ColorPalette pal = bmp.Palette;
				Assert.AreEqual (2, pal.Entries.Length, "Length");
				for (int i = 0; i < pal.Entries.Length; i++) {
					Assert.AreEqual (palette1[i], pal.Entries[i].ToArgb (), i.ToString ());
				}
				Assert.AreEqual (2, pal.Flags, "Flags");
			}
		}

		static int[] palette16 = {
			-16777216,
			-8388608,
			-16744448,
			-8355840,
			-16777088,
			-8388480,
			-16744320,
			-8355712,
			-4144960,
			-65536,
			-16711936,
			-256,
			-16776961,
			-65281,
			-16711681,
			-1,
		};

		[Test]
		public void Format4bppIndexed_Palette ()
		{
			using (Bitmap bmp = new Bitmap (1, 1, PixelFormat.Format4bppIndexed)) {
				ColorPalette pal = bmp.Palette;
				Assert.AreEqual (16, pal.Entries.Length, "Length");
				for (int i = 0; i < pal.Entries.Length; i++) {
					Assert.AreEqual (palette16 [i], pal.Entries[i].ToArgb (), i.ToString ());
				}
				Assert.AreEqual (0, pal.Flags, "Flags");
			}
		}

		static int[] palette256 = {
			-16777216,
			-8388608,
			-16744448,
			-8355840,
			-16777088,
			-8388480,
			-16744320,
			-8355712,
			-4144960,
			-65536,
			-16711936,
			-256,
			-16776961,
			-65281,
			-16711681,
			-1,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			-16777216,
			-16777165,
			-16777114,
			-16777063,
			-16777012,
			-16776961,
			-16764160,
			-16764109,
			-16764058,
			-16764007,
			-16763956,
			-16763905,
			-16751104,
			-16751053,
			-16751002,
			-16750951,
			-16750900,
			-16750849,
			-16738048,
			-16737997,
			-16737946,
			-16737895,
			-16737844,
			-16737793,
			-16724992,
			-16724941,
			-16724890,
			-16724839,
			-16724788,
			-16724737,
			-16711936,
			-16711885,
			-16711834,
			-16711783,
			-16711732,
			-16711681,
			-13434880,
			-13434829,
			-13434778,
			-13434727,
			-13434676,
			-13434625,
			-13421824,
			-13421773,
			-13421722,
			-13421671,
			-13421620,
			-13421569,
			-13408768,
			-13408717,
			-13408666,
			-13408615,
			-13408564,
			-13408513,
			-13395712,
			-13395661,
			-13395610,
			-13395559,
			-13395508,
			-13395457,
			-13382656,
			-13382605,
			-13382554,
			-13382503,
			-13382452,
			-13382401,
			-13369600,
			-13369549,
			-13369498,
			-13369447,
			-13369396,
			-13369345,
			-10092544,
			-10092493,
			-10092442,
			-10092391,
			-10092340,
			-10092289,
			-10079488,
			-10079437,
			-10079386,
			-10079335,
			-10079284,
			-10079233,
			-10066432,
			-10066381,
			-10066330,
			-10066279,
			-10066228,
			-10066177,
			-10053376,
			-10053325,
			-10053274,
			-10053223,
			-10053172,
			-10053121,
			-10040320,
			-10040269,
			-10040218,
			-10040167,
			-10040116,
			-10040065,
			-10027264,
			-10027213,
			-10027162,
			-10027111,
			-10027060,
			-10027009,
			-6750208,
			-6750157,
			-6750106,
			-6750055,
			-6750004,
			-6749953,
			-6737152,
			-6737101,
			-6737050,
			-6736999,
			-6736948,
			-6736897,
			-6724096,
			-6724045,
			-6723994,
			-6723943,
			-6723892,
			-6723841,
			-6711040,
			-6710989,
			-6710938,
			-6710887,
			-6710836,
			-6710785,
			-6697984,
			-6697933,
			-6697882,
			-6697831,
			-6697780,
			-6697729,
			-6684928,
			-6684877,
			-6684826,
			-6684775,
			-6684724,
			-6684673,
			-3407872,
			-3407821,
			-3407770,
			-3407719,
			-3407668,
			-3407617,
			-3394816,
			-3394765,
			-3394714,
			-3394663,
			-3394612,
			-3394561,
			-3381760,
			-3381709,
			-3381658,
			-3381607,
			-3381556,
			-3381505,
			-3368704,
			-3368653,
			-3368602,
			-3368551,
			-3368500,
			-3368449,
			-3355648,
			-3355597,
			-3355546,
			-3355495,
			-3355444,
			-3355393,
			-3342592,
			-3342541,
			-3342490,
			-3342439,
			-3342388,
			-3342337,
			-65536,
			-65485,
			-65434,
			-65383,
			-65332,
			-65281,
			-52480,
			-52429,
			-52378,
			-52327,
			-52276,
			-52225,
			-39424,
			-39373,
			-39322,
			-39271,
			-39220,
			-39169,
			-26368,
			-26317,
			-26266,
			-26215,
			-26164,
			-26113,
			-13312,
			-13261,
			-13210,
			-13159,
			-13108,
			-13057,
			-256,
			-205,
			-154,
			-103,
			-52,
			-1,
		};

		[Test]
		public void Format8bppIndexed_Palette ()
		{
			using (Bitmap bmp = new Bitmap (1, 1, PixelFormat.Format8bppIndexed)) {
				ColorPalette pal = bmp.Palette;
				Assert.AreEqual (256, pal.Entries.Length, "Length");
				for (int i = 0; i < pal.Entries.Length; i++) {
					Assert.AreEqual (palette256[i], pal.Entries[i].ToArgb (), i.ToString ());
				}
				Assert.AreEqual (4, pal.Flags, "Flags");
			}
		}
	}

	[TestFixture]
#if TARGET_JVM
	[Ignore ("Unsafe code is not supported")]
#endif
	public class BitmapFullTrustTest {
#if !TARGET_JVM
		// BitmapFromHicon## is *almost* the same as IconTest.Icon##ToBitmap except
		// for the Flags property

		private void HiconTest (string msg, Bitmap b, int size)
		{
			Assert.AreEqual (PixelFormat.Format32bppArgb, b.PixelFormat, msg + ".PixelFormat");
			// unlike the GDI+ icon decoder the palette isn't kept
			Assert.AreEqual (0, b.Palette.Entries.Length, msg + ".Palette");
			Assert.AreEqual (size, b.Height, msg + ".Height");
			Assert.AreEqual (size, b.Width, msg + ".Width");
			Assert.IsTrue (b.RawFormat.Equals (ImageFormat.MemoryBmp), msg + ".RawFormat");
			Assert.AreEqual (335888, b.Flags, msg + ".Flags");
		}

		[Test]
		public void Hicon16 ()
		{
			IntPtr hicon;
			int size;
			using (Icon icon = new Icon (TestBitmap.getInFile ("bitmaps/16x16x16.ico"))) {
				size = icon.Width;
				using (Bitmap bitmap = Bitmap.FromHicon (icon.Handle)) {
					HiconTest ("Icon.Handle/FromHicon", bitmap, size);
					hicon = bitmap.GetHicon ();
				}
			}
			using (Bitmap bitmap2 = Bitmap.FromHicon (hicon)) {
				// hicon survives bitmap and icon disposal
				HiconTest ("GetHicon/FromHicon", bitmap2, size);
			}
		}

		[Test]
		public void Hicon32 ()
		{
			IntPtr hicon;
			int size;
			using (Icon icon = new Icon (TestBitmap.getInFile ("bitmaps/32x32x16.ico"))) {
				size = icon.Width;
				using (Bitmap bitmap = Bitmap.FromHicon (icon.Handle)) {
					HiconTest ("Icon.Handle/FromHicon", bitmap, size);
					hicon = bitmap.GetHicon ();
				}
			}
			using (Bitmap bitmap2 = Bitmap.FromHicon (hicon)) {
				// hicon survives bitmap and icon disposal
				HiconTest ("GetHicon/FromHicon", bitmap2, size);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotWorking")] // libgdiplus has lost track of the original 1bpp state
		public void Hicon48 ()
		{
			using (Icon icon = new Icon (TestBitmap.getInFile ("bitmaps/48x48x1.ico"))) {
				// looks like 1bbp icons aren't welcome as bitmaps ;-)
				Bitmap.FromHicon (icon.Handle);
			}
		}

		[Test]
		public void Hicon64 ()
		{
			IntPtr hicon;
			int size;
			using (Icon icon = new Icon (TestBitmap.getInFile ("bitmaps/64x64x256.ico"))) {
				size = icon.Width;
				using (Bitmap bitmap = Bitmap.FromHicon (icon.Handle)) {
					HiconTest ("Icon.Handle/FromHicon", bitmap, size);
					hicon = bitmap.GetHicon ();
				}
			}
			using (Bitmap bitmap2 = Bitmap.FromHicon (hicon)) {
				// hicon survives bitmap and icon disposal
				HiconTest ("GetHicon/FromHicon", bitmap2, size);
			}
		}

		[Test]
		public void Hicon96 ()
		{
			IntPtr hicon;
			int size;
			using (Icon icon = new Icon (TestBitmap.getInFile ("bitmaps/96x96x256.ico"))) {
				size = icon.Width;
				using (Bitmap bitmap = Bitmap.FromHicon (icon.Handle)) {
					HiconTest ("Icon.Handle/FromHicon", bitmap, size);
					hicon = bitmap.GetHicon ();
				}
			}
			using (Bitmap bitmap2 = Bitmap.FromHicon (hicon)) {
				// hicon survives bitmap and icon disposal
				HiconTest ("GetHicon/FromHicon", bitmap2, size);
			}
		}

		[Test]
		public void HBitmap ()
		{
			IntPtr hbitmap;
			string sInFile = TestBitmap.getInFile ("bitmaps/almogaver24bits.bmp");
			using (Bitmap bitmap = new Bitmap (sInFile)) {
				Assert.AreEqual (PixelFormat.Format24bppRgb, bitmap.PixelFormat, "Original.PixelFormat");
				Assert.AreEqual (0, bitmap.Palette.Entries.Length, "Original.Palette");
				Assert.AreEqual (183, bitmap.Height, "Original.Height");
				Assert.AreEqual (173, bitmap.Width, "Original.Width");
				Assert.AreEqual (73744, bitmap.Flags, "Original.Flags");
				Assert.IsTrue (bitmap.RawFormat.Equals (ImageFormat.Bmp), "Original.RawFormat");
				hbitmap = bitmap.GetHbitmap ();
			}
			// hbitmap survives original bitmap disposal
			using (Image image = Image.FromHbitmap (hbitmap)) {
				//Assert.AreEqual (PixelFormat.Format32bppRgb, image.PixelFormat, "FromHbitmap.PixelFormat");
				Assert.AreEqual (0, image.Palette.Entries.Length, "FromHbitmap.Palette");
				Assert.AreEqual (183, image.Height, "FromHbitmap.Height");
				Assert.AreEqual (173, image.Width, "FromHbitmap.Width");
				Assert.AreEqual (335888, image.Flags, "FromHbitmap.Flags");
				Assert.IsTrue (image.RawFormat.Equals (ImageFormat.MemoryBmp), "FromHbitmap.RawFormat");
			}
		}

		[Test]
		public void CreateMultipleBitmapFromSameHBITMAP ()
		{
			IntPtr hbitmap;
			string sInFile = TestBitmap.getInFile ("bitmaps/almogaver24bits.bmp");
			using (Bitmap bitmap = new Bitmap (sInFile)) {
				Assert.AreEqual (PixelFormat.Format24bppRgb, bitmap.PixelFormat, "Original.PixelFormat");
				Assert.AreEqual (0, bitmap.Palette.Entries.Length, "Original.Palette");
				Assert.AreEqual (183, bitmap.Height, "Original.Height");
				Assert.AreEqual (173, bitmap.Width, "Original.Width");
				Assert.AreEqual (73744, bitmap.Flags, "Original.Flags");
				Assert.IsTrue (bitmap.RawFormat.Equals (ImageFormat.Bmp), "Original.RawFormat");
				hbitmap = bitmap.GetHbitmap ();
			}
			// hbitmap survives original bitmap disposal
			using (Image image = Image.FromHbitmap (hbitmap)) {
				//Assert.AreEqual (PixelFormat.Format32bppRgb, image.PixelFormat, "1.PixelFormat");
				Assert.AreEqual (0, image.Palette.Entries.Length, "1.Palette");
				Assert.AreEqual (183, image.Height, "1.Height");
				Assert.AreEqual (173, image.Width, "1.Width");
				Assert.AreEqual (335888, image.Flags, "1.Flags");
				Assert.IsTrue (image.RawFormat.Equals (ImageFormat.MemoryBmp), "1.RawFormat");
			}
			using (Image image2 = Image.FromHbitmap (hbitmap)) {
				//Assert.AreEqual (PixelFormat.Format32bppRgb, image2.PixelFormat, "2.PixelFormat");
				Assert.AreEqual (0, image2.Palette.Entries.Length, "2.Palette");
				Assert.AreEqual (183, image2.Height, "2.Height");
				Assert.AreEqual (173, image2.Width, "2.Width");
				Assert.AreEqual (335888, image2.Flags, "2.Flags");
				Assert.IsTrue (image2.RawFormat.Equals (ImageFormat.MemoryBmp), "2.RawFormat");
			}
		}
#endif
	}
}

