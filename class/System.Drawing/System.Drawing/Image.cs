//
// System.Drawing.Image.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
//
// Alexandre Pigolkine (pigolkine@gmx.de)
// 
//
namespace System.Drawing {

using System;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;

internal class InternalImageInfo : IDisposable 
{
/*
	Size	     	imageSize;
	PixelFormat  	pixelFormat;
	int	     		stride;
*/
	IntPtr		 	unmanagedImagePtr;	// We own the image memory
	BitmapData		bmpData;	
	ColorPalette 	palette;
	byte[]	     	image;
	ImageFormat  	rawFormat;

	internal InternalImageInfo()
	{
		palette = new ColorPalette();
		bmpData = new BitmapData ();
		bmpData.Height = bmpData.Width = 0;
		//imageSize = new Size(0,0);
		//pixelFormat = PixelFormat.Format32bppArgb;
		bmpData.PixelFormat = PixelFormat.Format32bppArgb;
		image = new byte[0];
		//stride = 0;
		bmpData.Stride = 0;
		unmanagedImagePtr = IntPtr.Zero;
		bmpData.Scan0 = IntPtr.Zero;
		rawFormat = ImageFormat.Bmp;
	}

	internal InternalImageInfo(BitmapData bmpDataSrc)
	{
		palette = new ColorPalette();
		bmpData = new BitmapData ();
		bmpData.Height = bmpDataSrc.Height;
		bmpData.Width = bmpDataSrc.Width;
		//imageSize = new Size(0,0);
		//pixelFormat = PixelFormat.Format32bppArgb;
		bmpData.PixelFormat = bmpDataSrc.PixelFormat;
		image = new byte[0];
		//stride = 0;
		bmpData.Stride = bmpDataSrc.Stride;
		unmanagedImagePtr = IntPtr.Zero;
		bmpData.Scan0 = bmpDataSrc.Scan0;
		rawFormat = ImageFormat.Bmp;
	}

	internal Size Size {
/*
		get { return imageSize; }
		set { imageSize = value; }
*/
		get { return new Size (bmpData.Width, bmpData.Height); }
		set { bmpData.Width = value.Width; bmpData.Height = value.Height; }
	}

	internal PixelFormat PixelFormat {
/*
		get { return pixelFormat; }
		set { pixelFormat = value; }
*/
		get { return bmpData.PixelFormat; }
		set { bmpData.PixelFormat = value; }
	}

	internal ColorPalette Palette {
		get { return palette; }
		set { palette = value; }
	}

	internal byte[] RawImageBytes {
		get { 
			if (image.Length == 0) {
				ReadSoureUnmanagedPtr ();
			}
			return image; 
		}
		set { 
			image = value; 
			FreeUnmanagedPtr();
		}
	}
	
	internal IntPtr UnmanagedImagePtr {
		get { 
			if (unmanagedImagePtr == IntPtr.Zero) {
				unmanagedImagePtr = Marshal.AllocHGlobal (image.Length);
				Marshal.Copy (image, 0, unmanagedImagePtr, image.Length);
			}
			return unmanagedImagePtr; 
		}
	}

	protected void ReadSoureUnmanagedPtr() 
	{
		if (bmpData.Scan0 != IntPtr.Zero) {
			image = new byte[bmpData.Stride * bmpData.Height];
			Marshal.Copy (bmpData.Scan0, image, 0, image.Length);	
		}
	}
	
	protected void FreeUnmanagedPtr() 
	{
		if (unmanagedImagePtr != IntPtr.Zero) {
			Marshal.FreeHGlobal (unmanagedImagePtr);
		}
	}
	
	internal int Stride {
/*
		get { return stride; }
		set { stride = value; }
*/
		get { return bmpData.Stride; }
		set { bmpData.Stride = value; }
	}

	internal ImageFormat RawFormat {
		get { return rawFormat; }
		set { rawFormat = value; }
	}
	
	internal unsafe void ChangePixelFormat (PixelFormat destPixelFormat) {
		//Console.WriteLine ("{0}.ChangePixelFormat to {1}", ToString(), destPixelFormat);
		if (PixelFormat == destPixelFormat) return;
		if (destPixelFormat != PixelFormat.Format32bppArgb &&
			destPixelFormat != PixelFormat.Format24bppRgb) {
			Console.WriteLine ("This format is not supported {0}", destPixelFormat);
			throw new NotImplementedException();
		}
		
		FreeUnmanagedPtr();
		
		int sourcePixelIncrement = (PixelFormat == PixelFormat.Format32bppArgb) ? 1 : 0;
		int destinationPixelIncrement = (destPixelFormat == PixelFormat.Format32bppArgb) ? 1 : 0;

		Size ourSize = Size;
		int destStride = (Image.GetPixelFormatSize(destPixelFormat) >> 3 ) * ourSize.Width;
		byte[] temp = new byte [destStride * ourSize.Height];
		fixed( byte *psrc = image, pbuf = temp) {
			byte* curSrc = psrc;
			byte* curDst = pbuf;
			for ( int i = 0; i < ourSize.Height; i++) {
				for( int j = 0; j < ourSize.Width; j++) {
					*curDst++ = *curSrc++;
					*curDst++ = *curSrc++;
					*curDst++ = *curSrc++;
					curSrc += sourcePixelIncrement;
					curDst += destinationPixelIncrement;
				}
			}
		}
		image = temp;
		PixelFormat = destPixelFormat;
		Stride = destStride;
	}
	
	public override string ToString()
	{
		return String.Format("InternalImageInfo. Size {0}, PixelFormat {1}, Stride {2}, Image size {3}",
			Size, PixelFormat, Stride, image.Length);
	}
	
	public void Dispose ()
	{
		FreeUnmanagedPtr();
	}
}

[Serializable]
//[ComVisible(true)]
public abstract class Image : MarshalByRefObject, IDisposable , ICloneable, ISerializable 
{
	internal IntPtr nativeObject = IntPtr.Zero;
	protected Size image_size;
	protected PixelFormat pixel_format;

	public delegate bool GetThumbnailImageAbort ();	       
	
	// constructor
	public Image () {
		pixel_format = PixelFormat.Format32bppArgb;
	}

	private Image (SerializationInfo info, StreamingContext context)
	{
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
	}
    
	// public methods
	// static
	public static Image FromFile (string filename)
	{
		return new Bitmap (filename);
	}
	
	public static Image FromFile (string filename, bool useEmbeddedColorManagement)
	{
		return new Bitmap (filename, useEmbeddedColorManagement);
	}
	
	public static Bitmap FromHbitmap (IntPtr hbitmap)
	{
		// Fixme: implement me
		throw new NotImplementedException ();
	}

	public static Bitmap FromHbitmap (IntPtr hbitmap, IntPtr hpalette)
	{
		// Fixme: implement me
		throw new NotImplementedException ();
	}

	internal static InternalImageInfo Decode( Stream streamIn) 
	{
		Stream stream = streamIn;
		InternalImageInfo	result = new InternalImageInfo();
		if (!stream.CanSeek) {
			// FIXME: if stream.CanSeek == false, copy to a MemoryStream and read nicely 
		}
		ImageCodecInfo[] availableDecoders = ImageCodecInfo.GetImageDecoders();
		long pos = stream.Position;
		ImageCodecInfo codecToUse = null;
		foreach( ImageCodecInfo info in availableDecoders) {
			for (int i = 0; i < info.SignaturePatterns.Length; i++) {
				stream.Seek(pos, SeekOrigin.Begin);
				bool codecFound = true;
				for (int iPattern = 0; iPattern < info.SignaturePatterns[i].Length; iPattern++) {
					byte pattern = (byte)stream.ReadByte();
					pattern &= info.SignatureMasks[i][iPattern];
					if( pattern != info.SignaturePatterns[i][iPattern]) {
						codecFound = false;
						break;
					}
				}
				if (codecFound) {
					codecToUse = info;
					break;
				}
			}
		}
		stream.Seek (pos, SeekOrigin.Begin);
		if (codecToUse != null && codecToUse.decode != null) {
			codecToUse.decode( stream, result);
		}
		return result;
	}
	
	public static int GetPixelFormatSize (PixelFormat pixfmt)
	{
		int result = 0;
		switch (pixfmt) {
			case PixelFormat.Format16bppArgb1555:
			case PixelFormat.Format16bppGrayScale:
			case PixelFormat.Format16bppRgb555:
			case PixelFormat.Format16bppRgb565:
				result = 16;
				break;
			case PixelFormat.Format1bppIndexed:
				result = 1;
				break;
			case PixelFormat.Format24bppRgb:
				result = 24;
				break;
			case PixelFormat.Format32bppArgb:
			case PixelFormat.Format32bppPArgb:
			case PixelFormat.Format32bppRgb:
				result = 32;
				break;
			case PixelFormat.Format48bppRgb:
				result = 48;
				break;
			case PixelFormat.Format4bppIndexed:
				result = 4;
				break;
			case PixelFormat.Format64bppArgb:
			case PixelFormat.Format64bppPArgb:
				result = 64;
				break;
			case PixelFormat.Format8bppIndexed:
				result = 8;
				break;
		}
		return result;
	}

	public static bool IsAlphaPixelFormat (PixelFormat pixfmt)
	{
		bool result = false;
		switch (pixfmt) {
			case PixelFormat.Format16bppArgb1555:
			case PixelFormat.Format32bppArgb:
			case PixelFormat.Format32bppPArgb:
			case PixelFormat.Format64bppArgb:
			case PixelFormat.Format64bppPArgb:
				result = true;
				break;
			case PixelFormat.Format16bppGrayScale:
			case PixelFormat.Format16bppRgb555:
			case PixelFormat.Format16bppRgb565:
			case PixelFormat.Format1bppIndexed:
			case PixelFormat.Format24bppRgb:
			case PixelFormat.Format32bppRgb:
			case PixelFormat.Format48bppRgb:
			case PixelFormat.Format4bppIndexed:
			case PixelFormat.Format8bppIndexed:
				result = false;
				break;
		}
		return result;
	}
	
//	public static bool IsCanonicalPixelFormat (PixelFormat pixfmt)
//	{
//		// Fixme: implement me
//		throw new NotImplementedException ();
//	}
//	
//	public static bool IsExtendedPixelFormat (PixelFormat pixfmt)
//	{
//		// Fixme: implement me
//		throw new NotImplementedException ();
//	}

	// non-static
	public RectangleF GetBounds (ref GraphicsUnit pageUnit)
	{
		// Fixme: implement me
		throw new NotImplementedException ();
	}
	
	//public EncoderParameters GetEncoderParameterList(Guid encoder);
	//public int GetFrameCount(FrameDimension dimension);
	//public PropertyItem GetPropertyItem(int propid);
	/*
	  public Image GetThumbnailImage(int thumbWidth, int thumbHeight,
	  Image.GetThumbnailImageAbort callback,
	  IntPtr callbackData);
	*/
	
	public void RemovePropertyItem (int propid)
	{
		// Fixme: implement me
		throw new NotImplementedException ();
	}
	
	public void RotateFlip (RotateFlipType rotateFlipType)
	{
		// Fixme: implement me
		throw new NotImplementedException ();
	}

	public void Save (string filename)
	{
		Save( filename, RawFormat);
	}

	internal virtual InternalImageInfo ConvertToInternalImageInfo() 
	{
		//return implementation.ConvertToInternalImageInfo();
		throw new NotImplementedException ();
	}

	public void Save (Stream stream, ImageFormat format)
	{
		ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
		foreach (ImageCodecInfo encoder in encoders) {
			if (encoder.FormatID == format.Guid) {
				if(encoder.encode != null) {
					if (this is Bitmap) {
						BitmapData bmpData = ((Bitmap)this).LockBits (new Rectangle (new Point (0,0), Size), ImageLockMode.ReadOnly, PixelFormat);
						InternalImageInfo imageInfo = new InternalImageInfo(bmpData);
						encoder.encode(stream, imageInfo);
						((Bitmap)this).UnlockBits (bmpData);
					}
				}
				break;
			}
		}
	}

	public void Save(string filename, ImageFormat format) 
	{
		FileStream fs = new FileStream( filename, FileMode.Create);
		Save(fs, format);
		fs.Flush();
		fs.Close();
	}

	//public void Save(Stream stream, ImageCodecInfo encoder, EncoderParameters encoderParams);
	//public void Save(string filename, ImageCodecInfo encoder, EncoderParameters encoderParams);
	//public void SaveAdd(EncoderParameters_ encoderParams);
	//public void SaveAdd(Image image, EncoderParameters_ encoderParams);
	//public int SelectActiveFrame(FrameDimension dimension, int frameIndex);
	//public void SetPropertyItem(PropertyItem propitem);

	// destructor
	~Image() {}

	// properties
	public int Flags {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public Guid[] FrameDimensionsList {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public int Height {
		get {
			return image_size.Height;
		}
	}
	
	public float HorizontalResolution {
		get {
			throw new NotImplementedException ();
		}
	}
	
//	public ColorPalette Palette {
//		get {
//			throw new NotImplementedException ();
//		}
//		set {
//			throw new NotImplementedException ();
//		}
//	}
	
	public SizeF PhysicalDimension {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public PixelFormat PixelFormat {
		get {
			return pixel_format;
		}
	}
	
	public int[] PropertyIdList {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public PropertyItem[] PropertyItems {
		get {
			throw new NotImplementedException ();
		}
	}

	public ImageFormat RawFormat {
		get {
			throw new NotImplementedException ();
		}
	}

	public Size Size {
		get {
			return image_size;
		}
	}
	
	public float VerticalResolution {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public int Width {
		get {
			return image_size.Width;
		}
	}
	
	public void Dispose ()
	{
	}

	[MonoTODO]
	object ICloneable.Clone()
	{
		throw new NotImplementedException ();
	}

}

}
