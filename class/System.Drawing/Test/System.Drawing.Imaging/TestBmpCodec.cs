//
// BMPCodec class testing unit
//
// Author:
//
// 	 Jordi Mas i Hern�ndez (jordi@ximian.com)
//
// (C) 2004 Ximian, Inc.  http://www.ximian.com
//
using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Drawing
{

	[TestFixture]	
	public class TestBmpCodec
	{
		
		[TearDown]
		public void Clean() {}
		
		[SetUp]
		public void GetReady()		
		{
		
		}
		
		/* Get the output directory depending on the runtime and location*/
		internal string getOutSubDir()
		{				
			string sSub, sRslt;			
			
			if (Environment.GetEnvironmentVariable("MSNet")==null)
				sSub = "mono/";
			else
				sSub = "MSNet/";			
			
			sRslt = Path.GetFullPath ("../System.Drawing/" +  sSub);
				
			if (Directory.Exists(sRslt) == 	false) 
				sRslt = "Test/System.Drawing/" + sSub;							
			
			if (sRslt.Length > 0)
				if (sRslt[sRslt.Length-1] != '\\' && sRslt[sRslt.Length-1] != '/')
					sRslt += "/";					
			return sRslt;
		}

		/* Get the input directory depending on the runtime*/
		internal string getInFile(string file)
		{				
			string sRslt, local;						

			local = "../System.Drawing/" + file;
			
			sRslt = Path.GetFullPath (local);
				
			if (File.Exists(sRslt)==false) 
				sRslt = "Test/System.Drawing/" + file;							
			
			return sRslt;
		}
		
		/* Checks bitmap features on a know 24-bits bitmap */
		[Test]
		public void Bitmap24bitFeatures()
		{
			string sInFile = getInFile ("bitmaps/almogaver24bits.bmp");
			Bitmap	bmp = new Bitmap(sInFile);						
			RectangleF rect;
			GraphicsUnit unit = GraphicsUnit.World;
			
			rect = bmp.GetBounds(ref unit);
			
			Assert.AreEqual (PixelFormat.Format24bppRgb, bmp.PixelFormat);
			Assert.AreEqual (173, bmp.Width);
			Assert.AreEqual (183, bmp.Height);		
			
			Assert.AreEqual (0, rect.X);
			Assert.AreEqual (0, rect.Y);		
			Assert.AreEqual (173, rect.Width);
			Assert.AreEqual (183, rect.Height);					
			
			Assert.AreEqual (173, bmp.Size.Width);
			Assert.AreEqual (183, bmp.Size.Height);					
		}

		

		/* Checks bitmap features on a know 32-bits bitmap (codec)*/
		[Test]
		public void Bitmap32bitFeatures()
		{
			string sInFile = getInFile ("bitmaps/almogaver32bits.bmp");
			Bitmap	bmp = new Bitmap(sInFile);						
			RectangleF rect;
			GraphicsUnit unit = GraphicsUnit.World;
			
			rect = bmp.GetBounds(ref unit);
			
			//Assert.AreEqual (PixelFormat.Format32bppArgb, bmp.PixelFormat);
			Assert.AreEqual (173, bmp.Width);
			Assert.AreEqual (183, bmp.Height);		
			
			Assert.AreEqual (0, rect.X);
			Assert.AreEqual (0, rect.Y);		
			Assert.AreEqual (173, rect.Width);
			Assert.AreEqual (183, rect.Height);					
			
			Assert.AreEqual (173, bmp.Size.Width);
			Assert.AreEqual (183, bmp.Size.Height);					
		}

		[Test]
		public void Save() 
		{				
			string sOutFile =  getOutSubDir() + "linerect.bmp";
						
			// Save		
			Bitmap	bmp = new Bitmap(100,100, PixelFormat.Format32bppRgb);						
			Graphics gr = Graphics.FromImage(bmp);		
			
			Pen p = new Pen(Color.Red, 2);
			gr.DrawLine(p, 10.0F, 10.0F, 90.0F, 90.0F);
			gr.DrawRectangle(p, 10.0F, 10.0F, 80.0F, 80.0F);
			p.Dispose();					
			bmp.Save(sOutFile, ImageFormat.Bmp);
			gr.Dispose();
			bmp.Dispose();							
			
			// Load			
			Bitmap	bmpLoad = new Bitmap(sOutFile);					
			
			Color color = bmpLoad.GetPixel(10,10);					
			
			Assert.AreEqual (Color.FromArgb(255,255,0,0), color);											
		}

		
	}
}
