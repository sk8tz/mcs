//
// System.Drawing.Icon.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2002 Ximian, Inc
//
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace System.Drawing
{

	[Serializable]
	[ComVisible (false)]
	[Editor ("System.Drawing.Design.IconEditor, " + Consts.AssemblySystem_Drawing_Design, typeof (System.Drawing.Design.UITypeEditor))]
	[TypeConverter(typeof(IconConverter))]
	public sealed class Icon : MarshalByRefObject, ISerializable, ICloneable, IDisposable
	{
		[StructLayout(LayoutKind.Sequential)]
		internal struct IconDirEntry {
			internal byte	width;				// Width of icon
			internal byte	height;				// Height of icon
			internal byte	colorCount;			// colors in icon 
			internal byte	reserved;			// Reserved
			internal ushort planes;             // Color Planes
			internal ushort	bitCount;           // Bits per pixel
			internal uint	bytesInRes;         // bytes in resource
			internal uint	imageOffset;        // position in file 
		}; 

		[StructLayout(LayoutKind.Sequential)]
		internal struct IconDir {
			internal ushort			idReserved;   // Reserved
			internal ushort			idType;       // resource type (1 for icons)
			internal ushort			idCount;      // how many images?
			internal IconDirEntry []	idEntries;	  // the entries for each image
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
			internal uint []			iconColors;	//colors table
			internal byte []			iconXOR;	// bits for XOR mask
			internal byte []			iconAND;	//bits for AND mask
		};	

		Size iconSize;
		IntPtr winHandle = IntPtr.Zero;
		IconDir	iconDir;
		ushort id;
		IconImage [] imageData;
			
		private Icon ()
		{
		}
		
		public Icon (Icon original, int width, int height) : this (original, new Size(width, height))
		{			
		}

		[MonoTODO ("Implement")]
		public Icon (Icon original, Size size)
		{
			iconSize = size;
			throw new NotImplementedException ();
		}

		public Icon (Stream stream) : this (stream, 32, 32) 
		{
		}

		[MonoTODO ("Implement")]
		public Icon (Stream stream, int width, int height)
		{
			//read the icon header
			if (stream == null || stream.Length == 0)
				throw new System.ArgumentException ("The argument 'stream' must be a picture that can be used as a Icon", "stream");
			
			//Console.WriteLine("Icon.cs StreamLength is "+stream.Length);

			BinaryReader reader = new BinaryReader (stream);
            			
			iconDir.idReserved = reader.ReadUInt16();
			if (iconDir.idReserved != 0) //must be 0
				throw new System.ArgumentException ("Invalid Argument", "stream");
			
			iconDir.idType = reader.ReadUInt16();
			if (iconDir.idType != 1) //must be 1
				throw new System.ArgumentException ("Invalid Argument", "stream");

			ushort dirEntryCount = reader.ReadUInt16();
			iconDir.idCount = dirEntryCount;
			//Console.WriteLine("Icon.cs iconDir.idCount "+iconDir.idCount);
			iconDir.idEntries = new IconDirEntry [dirEntryCount];
			imageData = new IconImage [dirEntryCount];
			bool sizeObtained = false;
			//now read in the IconDirEntry structures
			for (int i=0; i<dirEntryCount; i++){
				IconDirEntry ide;
				ide.width = reader.ReadByte ();
				//Console.WriteLine("Icon.cs ide.width "+ide.width);
				//Console.WriteLine("Icon.cs position of binareReader is " + reader.BaseStream.Position);
				ide.height = reader.ReadByte ();
				//Console.WriteLine("Icon.cs ide.height"+ide.height);
				ide.colorCount = reader.ReadByte ();
				ide.reserved = reader.ReadByte ();
				ide.planes = reader.ReadUInt16 ();
				ide.bitCount = reader.ReadUInt16 ();
				ide.bytesInRes = reader.ReadUInt32 ();
				ide.imageOffset = reader.ReadUInt32 ();
				//Console.WriteLine("Icon.cs ide.imageOffset"+ide.imageOffset);
				//Console.WriteLine("Icon.cs ide.bytesInRes"+ide.bytesInRes);
				iconDir.idEntries [i] = ide;
				//is this is the best fit??
				if (!sizeObtained)   
					if (ide.height>=height && ide.width>=width) {
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
				//Console.WriteLine("Icon.cs position of stream is" +stream.Position);
				byte [] buffer = new byte [iconDir.idEntries [j].bytesInRes];
				//Console.WriteLine("Icon.cs reading icondata bytes in res is "+buffer.Length);
				stream.Read (buffer, 0, buffer.Length);
				BinaryReader bihReader = new BinaryReader (new MemoryStream(buffer));
				bih.biSize = bihReader.ReadUInt32 ();
				bih.biWidth = bihReader.ReadInt32 ();
				//Console.WriteLine("Icon.cs reading icondata bih.width "+bih.biWidth);
				bih.biHeight = bihReader.ReadInt32 ();
				//Console.WriteLine("Icon.cs reading icondata bih.height "+bih.biHeight);
				bih.biPlanes = bihReader.ReadUInt16 ();
				bih.biBitCount = bihReader.ReadUInt16 ();
				bih.biCompression = bihReader.ReadUInt32 ();
				bih.biSizeImage = bihReader.ReadUInt32 ();
				bih.biXPelsPerMeter = bihReader.ReadInt32 ();
				bih.biYPelsPerMeter = bihReader.ReadInt32 ();
				bih.biClrUsed = bihReader.ReadUInt32 ();
				bih.biClrImportant = bihReader.ReadUInt32 ();
				//TODO read RGBQUADS and XOR and AND masks
				iidata.iconHeader = bih;
				imageData [j] = iidata;
			}
			
		}

		public Icon (string fileName) : this (new FileStream (fileName, FileMode.Open))
		{			
		}

		[MonoTODO ("Implement")]
		public Icon (Type type, string resource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
        	private Icon (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public void Dispose ()
		{
		}

		[MonoTODO ("Implement")]
		public object Clone ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public static Icon FromHandle (IntPtr handle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public void Save (Stream outputStream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public Bitmap ToBitmap ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implement")]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		public IntPtr Handle {
			get { 
				return winHandle;
			}
		}

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

		public int Width {
			get {
				return iconSize.Width;
			}
		}

		~Icon ()
		{
			Dispose ();
		}
			
	}
}
