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
	protected ColorPalette colorPalette;
	protected ImageFormat raw_format;
	
	// constructor
	internal  Image()
	{		
		colorPalette = new ColorPalette();
	}
	
	[MonoTODO]	
	private Image (SerializationInfo info, StreamingContext context)
	{
		
	}
	
	[MonoTODO]	
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
	public RectangleF GetBounds (ref GraphicsUnit pageUnit)
	{	
		RectangleF source;			
		
		Status status = GDIPlus.GdipGetImageBounds (nativeObject, out source, ref pageUnit);
		GDIPlus.CheckStatus (status);		
		
		return source;
	}
	
	[MonoTODO]	
	public EncoderParameters GetEncoderParameterList(Guid encoder)
	{
		throw new NotImplementedException ();
	}
	
	public int GetFrameCount(FrameDimension dimension)
	{
		int count;
		Guid guid = dimension.Guid;
		
		Status status = GDIPlus.GdipImageGetFrameCount (nativeObject, ref guid, out  count);
		GDIPlus.CheckStatus (status);		
		
		return count;
		
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
		if (Environment.OSVersion.Platform == (PlatformID) 128) {
			byte[] g = format.Guid.ToByteArray();
			GDIPlus.GdiPlusStreamHelper sh = new GDIPlus.GdiPlusStreamHelper (stream);
			Status st = GDIPlus.GdipSaveImageToDelegate_linux (nativeObject, sh.PutBytesDelegate, g, IntPtr.Zero);
		} else {
			throw new NotImplementedException ("Image.Save(Stream) (win32)");
		}
	}

	public void Save(string filename, ImageFormat format) 
	{
		byte[] g = format.Guid.ToByteArray();
		Status st = GDIPlus.GdipSaveImageToFile (nativeObject, filename, g, IntPtr.Zero);
		GDIPlus.CheckStatus (st);
	}
	
	internal void setGDIPalette() 
	{
		IntPtr gdipalette;

		gdipalette = colorPalette.getGDIPalette ();
		Status st = GDIPlus.GdipSetImagePalette (NativeObject, gdipalette);
		Marshal.FreeHGlobal (gdipalette);

		GDIPlus.CheckStatus (st);
	}

	[MonoTODO ("Ignoring EncoderParameters")]
	public void Save(Stream stream, ImageCodecInfo encoder, EncoderParameters encoderParams)
	{
		Save (stream, new ImageFormat (encoder.FormatID));
	}
	
	[MonoTODO ("Ignoring EncoderParameters")]	
	public void Save(string filename, ImageCodecInfo encoder, EncoderParameters encoderParams)
	{
		Save (filename, new ImageFormat (encoder.FormatID));
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
	
	
	public int SelectActiveFrame(FrameDimension dimension, int frameIndex)
	{
		Guid guid = dimension.Guid;		
				
		Status status = GDIPlus.GdipImageSelectActiveFrame (nativeObject, ref guid, frameIndex);			
		GDIPlus.CheckStatus (status);			
		
		return frameIndex;		
	}
	
	[MonoTODO]	
	public void SetPropertyItem(PropertyItem propitem)
	{
		throw new NotImplementedException ();
	}

	// properties	
	public int Flags {
		get {
			int flags;
			
			Status status = GDIPlus.GdipGetImageFlags (nativeObject, out flags);			
			GDIPlus.CheckStatus (status);						
			return flags;			
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
			int height;			
			Status status = GDIPlus.GdipGetImageHeight (nativeObject, out height);		
			GDIPlus.CheckStatus (status);			
			
			return height;
		}
	}
	
	public float HorizontalResolution {
		get {
			float resolution;
			
			Status status = GDIPlus.GdipGetImageHorizontalResolution (nativeObject, out resolution);			
			GDIPlus.CheckStatus (status);			
			
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
	
	
	public SizeF PhysicalDimension {
		get {
			float width,  height;
			
			Status status = GDIPlus.GdipGetImageDimension (nativeObject, out width, out height);		
			GDIPlus.CheckStatus (status);			
			
			return new SizeF (width, height);
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
			return new Size(Width, Height);
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
			int width;			
			Status status = GDIPlus.GdipGetImageWidth (nativeObject, out width);		
			GDIPlus.CheckStatus (status);			
			
			return width;
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
	
	
	public virtual object Clone()
	{				
		IntPtr newimage = IntPtr.Zero;
		
		if (!(this is Bitmap)) 
			throw new NotImplementedException (); 
		
		Status status = GDIPlus.GdipCloneImage (NativeObject, out newimage);			
		
		GDIPlus.CheckStatus (status);			
		
		if (this is Bitmap)
			return new Bitmap (newimage);
		
		throw new NotImplementedException (); 
	}

}

}
