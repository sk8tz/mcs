//
// System.Drawing.Image.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: 	Christian Meyer (Christian.Meyer@cs.tum.edu)
// 		Alexandre Pigolkine (pigolkine@gmx.de)
//		Jordi Mas i Hernandez (jordi@ximian.com)
//
namespace System.Drawing {

using System;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;

[Serializable]
[ComVisible (true)]
[Editor ("System.Drawing.Design.ImageEditor, " + Consts.AssemblySystem_Drawing_Design, typeof (System.Drawing.Design.UITypeEditor))]
[TypeConverter (typeof(ImageConverter))]
[ImmutableObject (true)]
public abstract class Image : MarshalByRefObject, IDisposable , ICloneable, ISerializable 
{
	public delegate bool GetThumbnailImageAbort();
	
	internal IntPtr nativeObject = IntPtr.Zero;
	protected Size image_size;	
	protected ColorPalette colorPalette;
	protected ImageFormat raw_format;
	
	// constructor
	internal  Image()
	{		
		colorPalette = new ColorPalette();
	}
	

	private Image (SerializationInfo info, StreamingContext context)
	{
		
	}
	

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
	}
    
	// public methods
	// static
	public static Image FromFile(string filename)
	{
		return new Bitmap (filename);
	}
	
	public static Image FromFile(string filename, bool useEmbeddedColorManagement)
	{
		return new Bitmap (filename, useEmbeddedColorManagement);
	}

	[MonoTODO]	
	public static Bitmap FromHbitmap(IntPtr hbitmap)
	{		
		throw new NotImplementedException ();
	}

	[MonoTODO]	
	public static Bitmap FromHbitmap(IntPtr hbitmap, IntPtr hpalette)
	{		
		throw new NotImplementedException ();
	}

	public static Image FromStream (Stream stream)
	{
		return new Bitmap (stream);
	}
	
	public static Image FromStream (Stream stream, bool useECM)
	{
		return new Bitmap (stream, useECM);
	}
	
	internal BitmapData Decode (Stream streamIn) 
	{
		Stream stream = streamIn;
		BitmapData result = new BitmapData ();
		if (!stream.CanSeek) {
			// FIXME: if stream.CanSeek == false, copy to a MemoryStream and read nicely 
		}
		ImageCodecInfo[] availableDecoders = ImageCodecInfo.GetImageDecoders();
		long pos = stream.Position;
		ImageCodecInfo codecToUse = null;
		foreach (ImageCodecInfo info in availableDecoders) {
			for (int i = 0; i < info.SignaturePatterns.Length; i++) {
				stream.Seek(pos, SeekOrigin.Begin);
				bool codecFound = true;
				for (int iPattern = 0; iPattern < info.SignaturePatterns[i].Length; iPattern++) {
					byte pattern = (byte)stream.ReadByte();
					pattern &= info.SignatureMasks[i][iPattern];
					if (pattern != info.SignaturePatterns[i][iPattern]) {
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
			codecToUse.decode (this, stream, result);
		}
		return result;
	}
	
	public static int GetPixelFormatSize(PixelFormat pixfmt)
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

	public static bool IsAlphaPixelFormat(PixelFormat pixfmt)
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
	
	public static bool IsCanonicalPixelFormat (PixelFormat pixfmt)
	{
		return ((pixfmt & PixelFormat.Canonical) != 0);
	}
	
	public static bool IsExtendedPixelFormat (PixelFormat pixfmt)
	{
		return ((pixfmt & PixelFormat.Extended) != 0);
	}

	// non-static
	[MonoTODO]	
	public RectangleF GetBounds (ref GraphicsUnit pageUnit)
	{		
		throw new NotImplementedException ();
	}
	
	[MonoTODO]	
	public EncoderParameters GetEncoderParameterList(Guid encoder)
	{
		throw new NotImplementedException ();
	}
	
	public int GetFrameCount(FrameDimension dimension)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]	
	public PropertyItem GetPropertyItem(int propid)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]	
	public Image GetThumbnailImage(int thumbWidth, int thumbHeight, Image.GetThumbnailImageAbort callback, IntPtr callbackData)
	{
		throw new NotImplementedException ();				
	}
	
	[MonoTODO]	
	public void RemovePropertyItem (int propid)
	{		
		throw new NotImplementedException ();
	}
	
	[MonoTODO]	
	public void RotateFlip (RotateFlipType rotateFlipType)
	{		
		throw new NotImplementedException ();
	}

	public void Save (string filename)
	{
		Save (filename, RawFormat);
	}

	public void Save (Stream stream, ImageFormat format)
	{
		ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

		foreach (ImageCodecInfo encoder in encoders) {
			if (encoder.FormatID != format.Guid)
				continue;
			
			if (encoder.encode == null)
				continue;
			if (!(this is Bitmap))
				continue;
			encoder.encode(this, stream);
			break;
		}
	}

	public void Save(string filename, ImageFormat format) 
	{
		FileStream fs = new FileStream (filename, FileMode.Create);
		Save(fs, format);
		fs.Flush();
		fs.Close();
	}
	
	internal void setGDIPalette() 
	{
		IntPtr gdipalette;			
		
		gdipalette = colorPalette.getGDIPalette ();
		
		Status status = GDIPlus.GdipSetImagePalette (NativeObject, gdipalette);			
		
		if (status != Status.Ok)
			Console.WriteLine("Error calling GDIPlus.GdipSetImagePalette");			
			
		Marshal.FreeHGlobal (gdipalette);           					
	}

	[MonoTODO]	
	public void Save(Stream stream, ImageCodecInfo encoder, EncoderParameters encoderParams)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]	
	public void Save(string filename, ImageCodecInfo encoder, EncoderParameters encoderParams)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]	
	public void SaveAdd(EncoderParameters encoderParams)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]	
	public void SaveAdd(Image image, EncoderParameters encoderParams)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]	
	public int SelectActiveFrame(FrameDimension dimension, int frameIndex)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]	
	public void SetPropertyItem(PropertyItem propitem)
	{
		throw new NotImplementedException ();
	}

	// properties
	[MonoTODO]	
	public int Flags {
		get {
			throw new NotImplementedException ();
		}
	}
	
	[MonoTODO]	
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
			float resolution;
			
			GDIPlus.GdipGetImageHorizontalResolution (nativeObject, out resolution);			
			return resolution;
		}
	}
	
	public ColorPalette Palette {
		get {							
			
			return colorPalette;
		}
		set {
			colorPalette = value;		
			
		}
	}
	
	[MonoTODO]	
	public SizeF PhysicalDimension {
		get {
			throw new NotImplementedException ();
		}
	}
	
	public PixelFormat PixelFormat {
		get {
			
			PixelFormat value;				
			Status status = GDIPlus.GdipGetImagePixelFormat (nativeObject, out value);		
			GDIPlus.CheckStatus (status);			
			
			return value;
		}		
	}
	
	
	[MonoTODO]	
	public int[] PropertyIdList {
		get {
			throw new NotImplementedException ();
		}
	}
	
	[MonoTODO]	
	public PropertyItem[] PropertyItems {
		get {
			throw new NotImplementedException ();
		}
	}

	public ImageFormat RawFormat {
		get {
			return raw_format;
		}
	}

	internal void SetRawFormat (ImageFormat format)
	{
		raw_format = format;
	}

	public Size Size {
		get {
			return image_size;
		}
	}
	
	public float VerticalResolution {
		get {
			float resolution;
			
			GDIPlus.GdipGetImageVerticalResolution (nativeObject, out resolution);			
			return resolution;
		}
	}
	
	public int Width {
		get {
			return image_size.Width;
		}
	}
	
	internal IntPtr NativeObject{
		get{
			return nativeObject;
		}
		set	{
			nativeObject = value;
		}
	}
	
	public void Dispose ()
	{
		Dispose (true);
	}

	~Image ()
	{
		Dispose (false);
	}

	protected virtual void DisposeResources ()
	{
		GDIPlus.GdipDisposeImage (nativeObject);
	}
	
	protected virtual void Dispose (bool disposing)
	{
		if (nativeObject != (IntPtr) 0){
			DisposeResources ();
			nativeObject=IntPtr.Zero;
		}
	}
	
	[MonoTODO]
	object ICloneable.Clone()
	{
		throw new NotImplementedException ();
	}
	
	
	public virtual object Clone()
	{		
		IntPtr newimage = IntPtr.Zero;
		
		if (!(this is Bitmap)) 
			throw new NotImplementedException (); 
		
		Status status = GDIPlus.GdipCloneImage (NativeObject, out newimage);			
		
		GDIPlus.CheckStatus (status);			
		
		if (!(this is Bitmap))
			return new Bitmap (newimage);
		
		throw new NotImplementedException (); 
	}

}

}
