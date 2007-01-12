//
// Icon class testing unit
//
// Authors:
// 	Sanjay Gupta <gsanjay@novell.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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
using System.Reflection;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing {

	[TestFixture]	
	[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
	public class IconTest {
		
		Icon icon;
		Icon icon16, icon32, icon48, icon64, icon96;
		FileStream fs1;

		static string filename_dll;

		// static ctor are executed outside the Deny
		static IconTest ()
		{
			filename_dll = Assembly.GetExecutingAssembly ().Location;
		}
		
		[SetUp]
		public void SetUp ()		
		{
			String path = TestBitmap.getInFile ("bitmaps/smiley.ico");
			icon = new Icon (path);			
			fs1 = new FileStream (path, FileMode.Open);

			icon16 = new Icon (TestBitmap.getInFile ("bitmaps/16x16x16.ico"));
			icon32 = new Icon (TestBitmap.getInFile ("bitmaps/32x32x16.ico"));
			icon48 = new Icon (TestBitmap.getInFile ("bitmaps/48x48x1.ico"));
			icon64 = new Icon (TestBitmap.getInFile ("bitmaps/64x64x256.ico"));
			icon96 = new Icon (TestBitmap.getInFile ("bitmaps/96x96x256.ico"));
		}

		[TearDown]
		public void TearDown ()
		{
			if (fs1 != null)
				fs1.Close ();
			if (File.Exists ("newIcon.ico"))
				File.Delete ("newIcon.ico");
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void TestConstructors ()
		{
			Assert.AreEqual (32, icon.Height, "C#0a");
			Assert.AreEqual (32, icon.Width, "C#0b");

			Icon newIcon = new Icon (fs1, 48, 48);
			Assert.AreEqual (48, newIcon.Height, "C#1a"); 			
			Assert.AreEqual (48, newIcon.Width, "C#1b");

			newIcon = new Icon (icon, 16, 16);
			Assert.AreEqual (16, newIcon.Height, "C#2a"); 			
			Assert.AreEqual (16, newIcon.Width, "C#2b");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_IconNull_Int_Int ()
		{
			new Icon ((Icon)null, 32, 32);
		}

		[Test]
		public void Constructor_Icon_IntNegative_Int ()
		{
			Icon neg = new Icon (icon, -32, 32);
			Assert.AreEqual (32, neg.Height, "Height");
			Assert.AreEqual (32, neg.Width, "Width");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_IconNull_Size ()
		{
			new Icon ((Icon) null, new Size (32, 32));
		}

		[Test]
		public void Constructor_Icon_Size_Negative ()
		{
			Icon neg = new Icon (icon, new Size (-32, -32));
			Assert.AreEqual (16, neg.Height, "Height");
			Assert.AreEqual (16, neg.Width, "Width");
		}

		[Test]
		public void Constructor_Icon_Int_Int_NonSquare ()
		{
			Icon non_square = new Icon (icon, 32, 16);
			Assert.AreEqual (32, non_square.Height, "Height");
			Assert.AreEqual (32, non_square.Width, "Width");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_StreamNull ()
		{
			new Icon ((Stream) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_StreamNull_Int_Int ()
		{
			new Icon ((Stream) null, 32, 32);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_StringNull ()
		{
			new Icon ((string) null);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Constructor_TypeNull_String ()
		{
			new Icon ((Type) null, "mono.ico");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_Type_StringNull ()
		{
			new Icon (typeof (Icon), null);
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_StreamNull_Size ()
		{
			new Icon ((Stream) null, new Size (32, 32));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_StringNull_Size ()
		{
			new Icon ((string) null, new Size (32, 32));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_StringNull_Int_Int ()
		{
			new Icon ((string) null, 32, 32);
		}
#endif

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void TestProperties ()
		{
			Assert.AreEqual (32, icon.Height, "P#1");
			Assert.AreEqual (32, icon.Width, "P#2");
			Assert.AreEqual (32, icon.Size.Width, "P#3");
			Assert.AreEqual (32, icon.Size.Height, "P#4");
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void Clone ()
		{
			Icon clone = (Icon) icon.Clone ();
			Assert.AreEqual (32, clone.Height, "Height");
			Assert.AreEqual (32, clone.Width, "Width");
			Assert.AreEqual (32, clone.Size.Width, "Size.Width");
			Assert.AreEqual (32, clone.Size.Height, "Size.Height");
		}

		internal static void SaveAndCompare (string msg, Icon icon)
		{
			using (MemoryStream ms = new MemoryStream ()) {
				icon.Save (ms);
				ms.Position = 0;

				using (Icon loaded = new Icon (ms)) {
					Assert.AreEqual (icon.Height, loaded.Height, msg + ".Loaded.Height");
					Assert.AreEqual (icon.Width, loaded.Width, msg + ".Loaded.Width");
				}
			}
		}

		[Test]
#if TARGET_JVM
		[Category ("NotWorking")]
#endif
		public void Save ()
		{
			SaveAndCompare ("16", icon16);
			SaveAndCompare ("32", icon32);
			SaveAndCompare ("48", icon48);
			SaveAndCompare ("64", icon64);
			SaveAndCompare ("96", icon96);
		}

		[Test]
		public void Icon16ToBitmap ()
		{
			using (Bitmap b = icon16.ToBitmap ()) {
				Assert.AreEqual (PixelFormat.Format32bppArgb, b.PixelFormat, "PixelFormat");
				// unlike the GDI+ icon decoder the palette isn't kept
				Assert.AreEqual (0, b.Palette.Entries.Length, "Palette");
				Assert.AreEqual (icon16.Height, b.Height, "Height");
				Assert.AreEqual (icon16.Width, b.Width, "Width");
				Assert.IsTrue (b.RawFormat.Equals (ImageFormat.MemoryBmp), "RawFormat");
				Assert.AreEqual (2, b.Flags, "Flags");
			}
		}

		[Test]
		public void Icon32ToBitmap ()
		{
			using (Bitmap b = icon32.ToBitmap ()) {
				Assert.AreEqual (PixelFormat.Format32bppArgb, b.PixelFormat, "PixelFormat");
				// unlike the GDI+ icon decoder the palette isn't kept
				Assert.AreEqual (0, b.Palette.Entries.Length, "Palette");
				Assert.AreEqual (icon32.Height, b.Height, "Height");
				Assert.AreEqual (icon32.Width, b.Width, "Width");
				Assert.IsTrue (b.RawFormat.Equals (ImageFormat.MemoryBmp), "RawFormat");
				Assert.AreEqual (2, b.Flags, "Flags");
			}
		}

		[Test]
		public void Icon48ToBitmap ()
		{
			using (Bitmap b = icon48.ToBitmap ()) {
				Assert.AreEqual (PixelFormat.Format32bppArgb, b.PixelFormat, "PixelFormat");
				// unlike the GDI+ icon decoder the palette isn't kept
				Assert.AreEqual (0, b.Palette.Entries.Length, "Palette");
				Assert.AreEqual (icon48.Height, b.Height, "Height");
				Assert.AreEqual (icon48.Width, b.Width, "Width");
				Assert.IsTrue (b.RawFormat.Equals (ImageFormat.MemoryBmp), "RawFormat");
				Assert.AreEqual (2, b.Flags, "Flags");
			}
		}

		[Test]
		public void Icon64ToBitmap ()
		{
			using (Bitmap b = icon64.ToBitmap ()) {
				Assert.AreEqual (PixelFormat.Format32bppArgb, b.PixelFormat, "PixelFormat");
				// unlike the GDI+ icon decoder the palette isn't kept
				Assert.AreEqual (0, b.Palette.Entries.Length, "Palette");
				Assert.AreEqual (icon64.Height, b.Height, "Height");
				Assert.AreEqual (icon64.Width, b.Width, "Width");
				Assert.IsTrue (b.RawFormat.Equals (ImageFormat.MemoryBmp), "RawFormat");
				Assert.AreEqual (2, b.Flags, "Flags");
			}
		}

		[Test]
		public void Icon96ToBitmap ()
		{
			using (Bitmap b = icon96.ToBitmap ()) {
				Assert.AreEqual (PixelFormat.Format32bppArgb, b.PixelFormat, "PixelFormat");
				// unlike the GDI+ icon decoder the palette isn't kept
				Assert.AreEqual (0, b.Palette.Entries.Length, "Palette");
				Assert.AreEqual (icon96.Height, b.Height, "Height");
				Assert.AreEqual (icon96.Width, b.Width, "Width");
				Assert.IsTrue (b.RawFormat.Equals (ImageFormat.MemoryBmp), "RawFormat");
				Assert.AreEqual (2, b.Flags, "Flags");
			}
		}

#if NET_2_0
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ExtractAssociatedIcon_Null ()
		{
			Icon.ExtractAssociatedIcon (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ExtractAssociatedIcon_Empty ()
		{
			Icon.ExtractAssociatedIcon (String.Empty);
		}

		[Test]
		[ExpectedException (typeof (FileNotFoundException))]
		public void ExtractAssociatedIcon_DoesNotExists ()
		{
			Icon.ExtractAssociatedIcon ("does-not-exists.png");
		}
#endif
	}

	[TestFixture]	
	public class IconFullTrustTest {
#if NET_2_0
		[Test]
		public void ExtractAssociatedIcon ()
		{
			string filename_dll = Assembly.GetExecutingAssembly ().Location;
			Assert.IsNotNull (Icon.ExtractAssociatedIcon (filename_dll), "dll");
		}
#endif

		[Test]
		public void HandleRoundtrip ()
		{
			IntPtr handle;
			using (Icon icon = new Icon (TestBitmap.getInFile ("bitmaps/16x16x16.ico"))) {
				Assert.AreEqual (16, icon.Height, "Original.Height");
				Assert.AreEqual (16, icon.Width, "Original.Width");
				handle = icon.Handle;
				using (Icon icon2 = Icon.FromHandle (handle)) {
					Assert.AreEqual (16, icon2.Height, "FromHandle.Height");
					Assert.AreEqual (16, icon2.Width, "FromHandle.Width");
					Assert.AreEqual (handle, icon2.Handle, "FromHandle.Handle");
// enable when Save is enabled for re-constructed icons
//					IconTest.SaveAndCompare ("Handle", icon2);
				}
			}
			// unlike other cases (HICON, HBITMAP) handle DOESN'T survives original icon disposal
			// commented / using freed memory is risky ;-)
			/*using (Icon icon3 = Icon.FromHandle (handle)) {
				Assert.AreEqual (0, icon3.Height, "Survivor.Height");
				Assert.AreEqual (0, icon3.Width, "Survivor.Width");
				Assert.AreEqual (handle, icon3.Handle, "Survivor.Handle");
			}*/
		}

		[Test]
		public void HiconRoundtrip ()
		{
			IntPtr handle;
			using (Icon icon = new Icon (TestBitmap.getInFile ("bitmaps/16x16x16.ico"))) {
				Assert.AreEqual (16, icon.Height, "Original.Height");
				Assert.AreEqual (16, icon.Width, "Original.Width");
				handle = icon.ToBitmap ().GetHicon ();
			}
			// HICON survives
			using (Icon icon2 = Icon.FromHandle (handle)) {
				Assert.AreEqual (16, icon2.Height, "Survivor.Height");
				Assert.AreEqual (16, icon2.Width, "Survivor.Width");
				Assert.AreEqual (handle, icon2.Handle, "Survivor.Handle");
// enable when Save is enabled for re-constructed icons
//				IconTest.SaveAndCompare ("HICON", icon2);
			}
		}
	}
}
