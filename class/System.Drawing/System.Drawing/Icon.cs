//
// System.Drawing.Icon.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sanjay Gupta (gsanjay@novell.com)
//   Peter Dennis Bartok (pbartok@novell.com)
//
// Copyright (C) 2002 Ximian, Inc. http://www.ximian.com
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace System.Drawing
{
#if !NET_2_0
	[ComVisible (false)] 
#endif 
	[Serializable]	
	[Editor ("System.Drawing.Design.IconEditor, " + Consts.AssemblySystem_Drawing_Design, typeof (System.Drawing.Design.UITypeEditor))]
	[TypeConverter(typeof(IconConverter))]
	public sealed class Icon : MarshalByRefObject, ISerializable, ICloneable, IDisposable
	{
		[StructLayout(LayoutKind.Sequential)]
		internal struct IconDirEntry {
			internal byte	width;		// Width of icon
			internal byte	height;		// Height of icon
			internal byte	colorCount;	// colors in icon 
			internal byte	reserved;	// Reserved
			internal ushort planes;         // Color Planes
			internal ushort	bitCount;       // Bits per pixel
			internal uint	bytesInRes;     // bytes in resource
			internal uint	imageOffset;	// position in file 
		}; 

		[StructLayout(LayoutKind.Sequential)]
		internal struct IconDir {
			internal ushort			idReserved;   // Reserved
			internal ushort			idType;       // resource type (1 for icons)
			internal ushort			idCount;      // how many images?
			internal IconDirEntry []	idEntries;    // the entries for each image
		};
		
		[StructLayout(LayoutKind.Sequential)]
		internal struct BitmapInfoHeader {
            		internal uint	biSize; 
			internal int	biWidth; 
			internal int	biHeight; 
			internal ushort	biPlanes; 
			internal ushort	biBitCount; 
			internal uint	biCompression; 
			internal uint	biSizeImage; 
			internal int	biXPelsPerMeter; 
			internal int	biYPelsPerMeter; 
			internal uint	biClrUsed; 
			internal uint	biClrImportant; 
		};

		[StructLayout(LayoutKind.Sequential)]
		internal struct IconImage {
			internal BitmapInfoHeader	iconHeader;	//image header
			internal uint []		iconColors;	//colors table
			internal byte []		iconXOR;	// bits for XOR mask
			internal byte []		iconAND;	//bits for AND mask
		};	

		private Size iconSize;
		private IntPtr winHandle = IntPtr.Zero;
		private IconDir	iconDir;
		private ushort id;
		private IconImage [] imageData;
		bool destroyIcon = true;
		bool undisposable;
			
		private Icon ()
		{
		}
#if INTPTR_SUPPORTED
		// FIXME - Implement fully (well implement inside libgdiplus as unmanaged code)
		private Icon (IntPtr handle)
		{
			this.winHandle = handle;

			IconInfo ii;
			GDIPlus.GetIconInfo (winHandle, out ii);
			if (ii.IsIcon) {
				// If this structure defines an icon, the hot spot is always in the center of the icon
				iconSize = new Size (ii.xHotspot * 2, ii.yHotspot * 2);
			}
			else {
				throw new NotImplementedException ();
			}

			this.destroyIcon = false;
		}
#endif
		public Icon (Icon original, int width, int height) : this (original, new Size(width, height))
		{			
		}

		public Icon (Icon original, Size size)
		{
			this.iconSize = size;
			this.winHandle = original.winHandle;
			this.iconDir = original.iconDir;
			this.imageData = original.imageData;
			
			int count = iconDir.idCount;
			bool sizeObtained = false;
			for (int i=0; i<count; i++){
				IconDirEntry ide = iconDir.idEntries [i];
				if (!sizeObtained)   
					if (ide.height==size.Height && ide.width==size.Width) {
						this.id = (ushort) i;
						sizeObtained = true;
						this.iconSize.Height = ide.height;
						this.iconSize.Width = ide.width;
						break;
					}
			}

			if (!sizeObtained){
				uint largestSize = 0;
				for (int j=0; j<count; j++){
					if (iconDir.idEntries [j].bytesInRes >= largestSize){
						largestSize = iconDir.idEntries [j].bytesInRes;
						this.id = (ushort) j;
						this.iconSize.Height = iconDir.idEntries [j].height;
						this.iconSize.Width = iconDir.idEntries [j].width;
					}
				}
			}
			winHandle = (IntPtr) 1; // fake handle
		}

		public Icon (Stream stream) : this (stream, 32, 32) 
		{
		}

		public Icon (Stream stream, int width, int height)
		{
			InitFromStreamWithSize (stream, width, height);
		}

		public Icon (string fileName) : this (new FileStream (fileName, FileMode.Open))
		{			
		}

		public Icon (Type type, string resource)
		{
			using (Stream s = type.Assembly.GetManifestResourceStream (type, resource)) {
				if (s == null) {
					string msg = Locale.GetText ("Resource '{0}' was not found.", resource);
					throw new FileNotFoundException (msg);
				}
				InitFromStreamWithSize (s, 32, 32);		// 32x32 is default
			}
		}

		private Icon (SerializationInfo info, StreamingContext context)
		{
			MemoryStream dataStream = null;
			int width=0;
			int height=0;
			foreach (SerializationEntry serEnum in info) {
				if (String.Compare(serEnum.Name, "IconData", true) == 0) {
					dataStream = new MemoryStream ((byte []) serEnum.Value);
				}
				if (String.Compare(serEnum.Name, "IconSize", true) == 0) {
					Size iconSize = (Size) serEnum.Value;
					width = iconSize.Width;
					height = iconSize.Height;
				}
			}
			if ((dataStream != null) && (width == height)) {
				dataStream.Seek (0, SeekOrigin.Begin);
				InitFromStreamWithSize (dataStream, width, height);
			}
                }

		internal Icon (string resourceName, bool undisposable)
		{
			using (Stream s = typeof (Icon).Assembly.GetManifestResourceStream (resourceName)) {
				if (s == null) {
					string msg = Locale.GetText ("Resource '{0}' was not found.", resourceName);
					throw new FileNotFoundException (msg);
				}
				InitFromStreamWithSize (s, 32, 32);		// 32x32 is default
			}
			this.undisposable = true;
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			MemoryStream ms = new MemoryStream ();
			Save (ms);
			info.AddValue ("IconSize", this.Size, typeof (Size));
			info.AddValue ("IconData", ms.ToArray ());
		}

#if NET_2_0
		public Icon (Stream stream, Size size) : 
			this (stream, size.Width, size.Height)
		{
		}
		
		public Icon (string fileName, int width, int height) : 
			this (new FileStream (fileName, FileMode.Open), width, height)
		{
		}
	
		public Icon (string fileName, Size size) : 
			this (new FileStream (fileName, FileMode.Open), size)
		{
		}

		[MonoLimitation ("The same icon, SystemIcons.WinLogo, is returned for all file types.")]
		public static Icon ExtractAssociatedIcon (string filePath)
		{
			if ((filePath == null) || (filePath.Length == 0))
				throw new ArgumentException (Locale.GetText ("Null or empty path."), "filePath");
			if (!File.Exists (filePath))
				throw new FileNotFoundException (Locale.GetText ("Couldn't find specified file."), filePath);

			return SystemIcons.WinLogo;
		}	
#endif

		public void Dispose ()
		{
#if !TARGET_JVM
			if (!undisposable) {
				DisposeIcon ();
				GC.SuppressFinalize(this);
			}
#endif
		}
#if !TARGET_JVM
		void DisposeIcon ()
		{
			if (winHandle ==IntPtr.Zero)
				return;

			if (destroyIcon) {
				//TODO: will have to call some win32 icon stuff
				winHandle = IntPtr.Zero;
			}
		}
#endif

		public object Clone ()
		{
			return new Icon (this, this.Width, this.Height);
		}
#if INTPTR_SUPPORTED
		public static Icon FromHandle (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				throw new ArgumentException ("handle");

			return new Icon (handle);
		}
#else
		public static Icon FromHandle (IntPtr handle)
		{
			throw new NotImplementedException ();
		}
#endif
		public void Save (Stream outputStream)
		{
			if (iconDir.idEntries!=null){
				BinaryWriter bw = new BinaryWriter (outputStream);
				//write icondir
				bw.Write (iconDir.idReserved);
				bw.Write (iconDir.idType);
				ushort count = iconDir.idCount;
				bw.Write (count);
				
				//now write iconDirEntries
				for (int i=0; i<(int)count; i++){
					IconDirEntry ide = iconDir.idEntries [i];
					bw.Write (ide.width);
					bw.Write (ide.height);
					bw.Write (ide.colorCount);
					bw.Write (ide.reserved);
					bw.Write (ide.planes);
					bw.Write (ide.bitCount);
					bw.Write (ide.bytesInRes);
					bw.Write (ide.imageOffset);				
				}
				
				//now write iconImage data
				for (int i=0; i<(int)count; i++){
					BitmapInfoHeader bih = imageData [i].iconHeader;
					bw.Write (bih.biSize);
					bw.Write (bih.biWidth);
					bw.Write (bih.biHeight);
					bw.Write (bih.biPlanes);
					bw.Write (bih.biBitCount);
					bw.Write (bih.biCompression);
					bw.Write (bih.biSizeImage);
					bw.Write (bih.biXPelsPerMeter);
					bw.Write (bih.biYPelsPerMeter);
					bw.Write (bih.biClrUsed);
					bw.Write (bih.biClrImportant);

					//now write color table
					int colCount = imageData [i].iconColors.Length;
					for (int j=0; j<colCount; j++)
						bw.Write (imageData [i].iconColors [j]);

					//now write XOR Mask
					bw.Write (imageData [i].iconXOR);
					
					//now write AND Mask
					bw.Write (imageData [i].iconAND);
				}
				bw.Flush();				
			}
		}

		public Bitmap ToBitmap ()
		{
			IconImage		ii;
			BitmapInfoHeader	bih;
			int			ncolors;
			Bitmap			bmp;
			BitmapData		bits;
			ColorPalette		pal;
			int			biHeight;
			int			bytesPerLine;

			if (winHandle == IntPtr.Zero)
				throw new ObjectDisposedException ("handle");

			if (imageData == null) {
				return new Bitmap(32, 32);
			}

			ii = imageData[this.id];
			bih = ii.iconHeader;
			biHeight = bih.biHeight / 2;

			ncolors = (int)bih.biClrUsed;
			if (ncolors == 0) {
				if (bih.biBitCount < 24) {
					ncolors = (int)(1 << bih.biBitCount);
				}
			}

			switch(bih.biBitCount) {
				case 1: {	// Monochrome
					bmp = new Bitmap(bih.biWidth, biHeight, PixelFormat.Format1bppIndexed);
					break;
				}

				case 4: {	// 4bpp
					bmp = new Bitmap(bih.biWidth, biHeight, PixelFormat.Format4bppIndexed);
					break;
				}

				case 8: {	// 8bpp
					bmp = new Bitmap(bih.biWidth, biHeight, PixelFormat.Format8bppIndexed);
					break;
				}

				case 24: {
					bmp = new Bitmap(bih.biWidth, biHeight, PixelFormat.Format24bppRgb);
					break;
				}
				case 32: {	// 32bpp
					bmp = new Bitmap(bih.biWidth, biHeight, PixelFormat.Format32bppArgb);
					break;
				}

				default: {
					throw new Exception("Unexpected number of bits:" + bih.biBitCount.ToString());
				}
			}

			if (bih.biBitCount < 24) {
				pal = bmp.Palette;				// Managed palette

				for (int i = 0; i < ii.iconColors.Length; i++) {
					pal.Entries[i] = Color.FromArgb((int)ii.iconColors[i] | unchecked((int)0xff000000));
				}
				bmp.Palette = pal;
			}

			bytesPerLine = (int)((((bih.biWidth * bih.biBitCount) + 31) & ~31) >> 3);
			bits = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

			for (int y = 0; y < biHeight; y++) {
				Marshal.Copy(ii.iconXOR, bytesPerLine * y, (IntPtr)(bits.Scan0.ToInt64() + bits.Stride * (biHeight - 1 - y)), bytesPerLine);
			}
			
			bmp.UnlockBits(bits);

			bmp = new Bitmap (bmp);// This makes a 32bpp image out of an indexed one

			// Apply the mask to make properly transparent
			bytesPerLine = (int)((((bih.biWidth) + 31) & ~31) >> 3);
			for (int y = 0; y < biHeight; y++) {
				for (int x = 0; x < bih.biWidth / 8; x++) {
					for (int bit = 7; bit >= 0; bit--) {
						if (((ii.iconAND[y * bytesPerLine +x] >> bit) & 1) != 0) {
							bmp.SetPixel(x*8 + 7-bit, biHeight - y - 1, Color.Transparent);
						}
					}
				}
			}

			return bmp;
		}

		public override string ToString ()
		{
			//is this correct, this is what returned by .Net
			return "<Icon>";			
		}

		[Browsable (false)]
		public IntPtr Handle {
			get { 
				return winHandle;
			}
		}

		[Browsable (false)]
		public int Height {
			get {
				return iconSize.Height;
			}
		}

		public Size Size {
			get {
				return iconSize;
			}
		}

		[Browsable (false)]
		public int Width {
			get {
				return iconSize.Width;
			}
		}

#if !TARGET_JVM
		~Icon ()
		{
			DisposeIcon ();
		}
#endif
			
		private void InitFromStreamWithSize (Stream stream, int width, int height)
		{
			//read the icon header
			if (stream == null || stream.Length == 0)
				throw new System.ArgumentException ("The argument 'stream' must be a picture that can be used as a Icon", "stream");
			
			BinaryReader reader = new BinaryReader (stream);

			//iconDir = new IconDir ();
			iconDir.idReserved = reader.ReadUInt16();
			if (iconDir.idReserved != 0) //must be 0
				throw new System.ArgumentException ("Invalid Argument", "stream");
			
			iconDir.idType = reader.ReadUInt16();
			if (iconDir.idType != 1) //must be 1
				throw new System.ArgumentException ("Invalid Argument", "stream");

			ushort dirEntryCount = reader.ReadUInt16();
			iconDir.idCount = dirEntryCount;
			iconDir.idEntries = new IconDirEntry [dirEntryCount];
			imageData = new IconImage [dirEntryCount];
			bool sizeObtained = false;
			//now read in the IconDirEntry structures
			for (int i=0; i<dirEntryCount; i++){
				IconDirEntry ide;
				ide.width = reader.ReadByte ();
				ide.height = reader.ReadByte ();
				ide.colorCount = reader.ReadByte ();
				ide.reserved = reader.ReadByte ();
				ide.planes = reader.ReadUInt16 ();
				ide.bitCount = reader.ReadUInt16 ();
				ide.bytesInRes = reader.ReadUInt32 ();
				ide.imageOffset = reader.ReadUInt32 ();
				iconDir.idEntries [i] = ide;
				//is this is the best fit??
				if (!sizeObtained)
					if (ide.height==height && ide.width==width) {
						this.id = (ushort) i;
						sizeObtained = true;
						this.iconSize.Height = ide.height;
						this.iconSize.Width = ide.width;
					}			
			}
			//if we havent found the best match, return the one with the
			//largest size. Is this approach correct??
			if (!sizeObtained){
				uint largestSize = 0;
				for (int j=0; j<dirEntryCount; j++){
					if (iconDir.idEntries [j].bytesInRes >= largestSize)	{
						largestSize = iconDir.idEntries [j].bytesInRes;
						this.id = (ushort) j;
						this.iconSize.Height = iconDir.idEntries [j].height;
						this.iconSize.Width = iconDir.idEntries [j].width;
					}
				}
			}
			
			//now read in the icon data
			for (int j = 0; j<dirEntryCount; j++)
			{
				IconImage iidata = new IconImage();
				BitmapInfoHeader bih = new BitmapInfoHeader();
				stream.Seek (iconDir.idEntries [j].imageOffset, SeekOrigin.Begin);
				byte [] buffer = new byte [iconDir.idEntries [j].bytesInRes];
				stream.Read (buffer, 0, buffer.Length);
				BinaryReader bihReader = new BinaryReader (new MemoryStream(buffer));
				bih.biSize = bihReader.ReadUInt32 ();
				bih.biWidth = bihReader.ReadInt32 ();
				bih.biHeight = bihReader.ReadInt32 ();
				bih.biPlanes = bihReader.ReadUInt16 ();
				bih.biBitCount = bihReader.ReadUInt16 ();
				bih.biCompression = bihReader.ReadUInt32 ();
				bih.biSizeImage = bihReader.ReadUInt32 ();
				bih.biXPelsPerMeter = bihReader.ReadInt32 ();
				bih.biYPelsPerMeter = bihReader.ReadInt32 ();
				bih.biClrUsed = bihReader.ReadUInt32 ();
				bih.biClrImportant = bihReader.ReadUInt32 ();

				iidata.iconHeader = bih;
				//Read the number of colors used and corresponding memory occupied by
				//color table. Fill this memory chunk into rgbquad[]
				int numColors;
				switch (bih.biBitCount){
					case 1: numColors = 2;
						break;
					case 4: numColors = 16;
						break;
					case 8: numColors = 256;
						break;
					default: numColors = 0;
						break;
				}
				
				iidata.iconColors = new uint [numColors];
				for (int i=0; i<numColors; i++)
					iidata.iconColors [i] = bihReader.ReadUInt32 ();

				//XOR mask is immediately after ColorTable and its size is 
				//icon height* no. of bytes per line
				
				//icon height is half of BITMAPINFOHEADER.biHeight, since it contains
				//both XOR as well as AND mask bytes
				int iconHeight = bih.biHeight/2;
				
				//bytes per line should should be uint aligned
				int numBytesPerLine = ((((bih.biWidth * bih.biPlanes * bih.biBitCount)+ 31)>>5)<<2);
				
				//Determine the XOR array Size
				int xorSize = numBytesPerLine * iconHeight;
				iidata.iconXOR = new byte [xorSize];
				int nread = bihReader.Read (iidata.iconXOR, 0, xorSize);
				if (nread != xorSize)
					throw new Exception ("Short file");
				
				//Determine the AND array size
				numBytesPerLine = (int)((((bih.biWidth) + 31) & ~31) >> 3);
				int andSize = numBytesPerLine * iconHeight;
				iidata.iconAND = new byte [andSize];
				nread = bihReader.Read (iidata.iconAND, 0, andSize);
				if (nread != andSize)
					throw new Exception ("Short file");
				
				imageData [j] = iidata;
				bihReader.Close();
			}			

			reader.Close();
			winHandle = (IntPtr) 1; // fake handle
		}
	}
}
