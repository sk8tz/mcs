using System;
using NUnit.Framework;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using DrawingTestHelper;
using System.IO;

namespace Test.Sys.Drawing {
	[TestFixture]
	public class ImageFixture {
		DrawingTest t;
		[SetUp]
		public void SetUp () {
			t = DrawingTest.Create (256, 256);
			Image im=new Bitmap (@"..\..\Bitmap1.png"); 
			t.Graphics.DrawImageUnscaled (im, 0, 0);
		}
		[Test]
		public void Clone () {
			Image im1 = (Image) t.Bitmap.Clone ();
			Assert.IsFalse (Object.ReferenceEquals (im1, t.Bitmap));
			Assert.AreEqual (DrawingTest.CalculateNorm ((Bitmap)im1),
				DrawingTest.CalculateNorm (t.Bitmap));
			t.Graphics.FillRectangle (Brushes.Beige, 0, 0, 100, 100);
			Assert.IsFalse (DrawingTest.CalculateNorm ((Bitmap)im1) ==
				DrawingTest.CalculateNorm (t.Bitmap));
		}
		[Test]
		public void GetHashCodeTest () {
			Assert.IsFalse (t.Bitmap.GetHashCode () == 0);
			Bitmap im1 = new Bitmap (10, 20);
			Assert.IsFalse (t.Bitmap.GetHashCode () == im1.GetHashCode ());
		}
		[Test]
		public void GetBounds () {
			GraphicsUnit unit = new GraphicsUnit();
			Assert.AreEqual (new RectangleF (0, 0, 256, 256),
				t.Bitmap.GetBounds (ref unit));
			Assert.AreEqual (GraphicsUnit.Pixel, unit);
		}
		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void GetEncoderParameterList () {
			Assert.Fail ("Test not implemented - undocumented parameter");
		}
		[Test]
		public void GetFrameCount () {
			Assert.AreEqual (1, t.Bitmap.GetFrameCount (FrameDimension.Page));
			Assert.AreEqual (1, t.Bitmap.GetFrameCount (FrameDimension.Time));
			Assert.AreEqual (1, t.Bitmap.GetFrameCount (new FrameDimension (Guid.NewGuid ())));
		}
		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void GetPropertyItem () {
			Assert.Fail ("Test not implemented - undocumented parameter");
		}
		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void RemovePropertyItem () {
			Assert.Fail ("Test not implemented - undocumented parameter");
		}
		static bool ThumbnailCallback() {
			return false;
		}
		[Test]
#if TARGET_JVM
		[Category ("NotWorking")] // FIXME: IntPtr
#endif
		public void GetThumbnailImage() {
			t.Show ();
			Image.GetThumbnailImageAbort myCallback =
				new Image.GetThumbnailImageAbort(ThumbnailCallback);
			Image myThumbnail = t.Bitmap.GetThumbnailImage(
				10, 10, myCallback, IntPtr.Zero);
			t.Graphics.DrawImageUnscaled (myThumbnail, 100, 75);
			t.Show ();
		}
		[Test]
#if TARGET_JVM
		[Category ("NotWorking")] // FIXME: Image.RotateFlip is not implemented for now.
#endif
		public void RotateFlip () {
			t.Show ();
			t.Bitmap.RotateFlip (RotateFlipType.Rotate90FlipY);
			t.Show ();
			Assert.IsTrue (t.Compare (10));
		}
		[Test]
		public void Save_string () {
			t.Bitmap.Save ("test.png");
			using (FileStream r = new FileStream ("test.png", FileMode.Open)) {
				Bitmap b1 = new Bitmap (r);
				Assert.AreEqual (DrawingTest.CalculateNorm (t.Bitmap),
					DrawingTest.CalculateNorm (b1));
			}
			File.Delete ("test.png");
		}
		[Test]
		public void Save_Stream_ImageFormat () {
			using (FileStream w = new FileStream ("test.png", FileMode.OpenOrCreate)) {
				t.Bitmap.Save (w, ImageFormat.Png);
			}
			using (FileStream r = new FileStream ("test.png", FileMode.Open)) {
				Bitmap b1 = new Bitmap (r);
				Assert.AreEqual (DrawingTest.CalculateNorm (t.Bitmap),
					DrawingTest.CalculateNorm (b1));
			}
			File.Delete ("test.png");
		}
		[Test]
		public void Save_string_ImageFormat () {
			t.Bitmap.Save ("test.png", ImageFormat.Png);
			using (FileStream r = new FileStream ("test.png", FileMode.Open)) {
				Bitmap b1 = new Bitmap (r);
				Assert.AreEqual (DrawingTest.CalculateNorm (t.Bitmap),
					DrawingTest.CalculateNorm (b1));
			}
			File.Delete ("test.png");
		}
		[Test]
#if TARGET_JVM
		[Category("NotWorking")]
#endif
		public void Save_Stream_ImageCodecInfo_EncoderParameters () {
			using (FileStream w = new FileStream ("test.png", FileMode.OpenOrCreate)) {
				foreach (ImageCodecInfo i in ImageCodecInfo.GetImageEncoders()) {
					if (i.FilenameExtension.IndexOf ("png") != -1) {
						t.Bitmap.Save (w, i, new EncoderParameters ());
						break;
					}
				}
			}
			using (FileStream r = new FileStream ("test.png", FileMode.Open)) {
				Bitmap b1 = new Bitmap (r);
				Assert.AreEqual (DrawingTest.CalculateNorm (t.Bitmap),
					DrawingTest.CalculateNorm (b1));
			}
			File.Delete ("test.png");
		}
		[Test]
#if TARGET_JVM
		[Category("NotWorking")]
#endif
		public void Save_string_ImageCodecInfo_EncoderParameters () {
			foreach (ImageCodecInfo i in ImageCodecInfo.GetImageEncoders()) {
				if (i.FilenameExtension.IndexOf ("png") != -1) {
					t.Bitmap.Save ("test.png", i, new EncoderParameters ());
					break;
				}
			}
			using (FileStream r = new FileStream ("test.png", FileMode.Open)) {
				Bitmap b1 = new Bitmap (r);
				Assert.AreEqual (DrawingTest.CalculateNorm (t.Bitmap),
					DrawingTest.CalculateNorm (b1));
			}
			File.Delete ("test.png");
		}
		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void SaveAdd () {
			Assert.Fail ("Test not implemented");
		}
		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void SelectActiveFrame () {
			Assert.Fail ("Test not implemented");
		}
		[Test]
		[Category ("NotWorking")]
		public void SetPropertyItem () {
			Assert.Fail ("Test not implemented - undocumented parameter");
		}
		[Test]
		public new void ToString () {
			Assert.IsTrue (t.Bitmap.ToString().ToLower().StartsWith("system.drawing.bitmap"));
		}
		[Test]
		public void Height () {
			Assert.AreEqual (256, t.Bitmap.Height);
		}
		[Test]
		public void Width () {
			Assert.AreEqual (256, t.Bitmap.Width);
		}
		[Test]
#if TARGET_JVM
		[Category ("NotWorking")] // FIXME: Image.HorizontalResolution is not implemented for now.
#endif
		public void HorizontalResolution () {
			Assert.AreEqual (96, t.Bitmap.HorizontalResolution);
		}
		[Test]
#if TARGET_JVM
		[Category ("NotWorking")] // FIXME: Image.VerticalResolution is not implemented for now.
#endif
		public void VerticalResolution () {
			Assert.AreEqual (96, t.Bitmap.VerticalResolution);
		}
		[Test]
		public void PixelFormatTest () {
			Assert.AreEqual (PixelFormat.Format32bppArgb, t.Bitmap.PixelFormat);
		}
		[Test]
#if TARGET_JVM
		[Category ("NotWorking")] // FIXME: Image.PropertyIdList is not implemented for now.
#endif
		public void PropertyIdList () {
			Assert.AreEqual (new int [0], t.Bitmap.PropertyIdList);
		}
		[Test]
#if TARGET_JVM
		[Category ("NotWorking")] // FIXME: Image.PropertyItems is not implemented for now.
#endif
		public void PropertyItems () {
			Assert.AreEqual (new PropertyItem [0], t.Bitmap.PropertyItems);
		}
		[Test]
		public void FrameDimensionsList () {
			Assert.AreEqual (new Guid [] {FrameDimension.Page.Guid},
				t.Bitmap.FrameDimensionsList);
		}
		[Test]
		[Category ("Create")]
		public void PNG_Interop()
		{

			string file_name = @"..\..\bitmap_gh.png";
			using (FileStream r = new FileStream (file_name, FileMode.Open)) 
			{
				Image im = new Bitmap (r);
				t.Graphics.DrawImageUnscaled (im, 0, 0);
				Assert.IsTrue(t.Compare(2));
			}

			file_name = @"..\..\bitmap_net.png";
			using (FileStream r = new FileStream (file_name, FileMode.Open)) 
			{
				Image im = new Bitmap (r);
				t.Graphics.DrawImageUnscaled (im, 0, 0);
				Assert.IsTrue(t.Compare(2));
			}
		}
		[Test]
		public void DefaultSaveFormat()
		{
			Bitmap b = new Bitmap(64, 64);
			b.Save("saveFormat.xxx");

			StreamReader sr = new StreamReader("saveFormat.xxx");
			char [] buffer = new char[4];
			sr.Read(buffer, 0, 4);
			sr.Close();

			Assert.AreEqual('P', buffer[0]);
			Assert.AreEqual('N', buffer[1]);
			Assert.AreEqual('G', buffer[2]);
		}
	}
}
