//
// System.Drawing.Image.cs
//
// (C) 2002/3 Ximian, Inc.  http://www.ximian.com
// Authors:
//   Christian Meyer <Christian.Meyer@cs.tum.edu>
//   Jason Perkins <jason@379.com>
//   Dennis Hayes <dennish@raytek.com>
//   Alexandre Pigolkine <pigolkine@gmx.de>

using System;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Drawing.Cairo {
	//[Serializable]
	//[ComVisible(true)]

	internal abstract class Image : MarshalByRefObject, IImage /*, ICloneable, ISerializable */
	{

		internal IntPtr state;
		internal Format cairo_format;
		internal System.Drawing.Cairo.Graphics selected_into_graphics = null;
		internal Size size;
		internal PixelFormat pixelFormat;
		protected ImageFormat imageFormat;
			
		// constructor
		public Image () {}

		[MonoTODO]
		public virtual object Clone()
		{
			throw new NotImplementedException ();
		}
    
		// public methods
		// static
		public static Image FromFile (string filename)
		{
			// Fixme: implement me
			throw new NotImplementedException ();
		}
	
		public static Image FromFile (string filename, bool useEmbeddedColorManagement)
		{
				
			// Fixme: implement me
			throw new NotImplementedException ();
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
		
		[MonoTODO]
		public static Image FromStream (Stream stream)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public static Image FromStream (Stream stream, bool useIcm)
		{
			throw new NotImplementedException ();
		}
	
		public static int GetPixelFormatSize (PixelFormat pixfmt)
		{
			// Fixme: implement me
			throw new NotImplementedException ();
		}
		
		public static bool IsAlphaPixelFormat (PixelFormat pixfmt)
		{
			// Fixme: implement me
			throw new NotImplementedException ();
		}
			
		public static bool IsCanonicalPixelFormat (PixelFormat pixfmt)
		{
			// Fixme: implement me
			throw new NotImplementedException ();
		}
	
		public static bool IsExtendedPixelFormat (PixelFormat pixfmt)
		{
			// Fixme: implement me
			throw new NotImplementedException ();
		}

		// non-static
		RectangleF IImage.GetBounds (ref GraphicsUnit pageUnit)
		{
			// Fixme: implement me
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		int IImage.GetFrameCount (FrameDimension dimension)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		PropertyItem IImage.GetPropertyItem (int propid)
		{
			throw new NotImplementedException();
		}

		void IImage.RemovePropertyItem (int propid)
		{
			// Fixme: implement me
			throw new NotImplementedException ();
		}
	
		void IImage.RotateFlip (RotateFlipType rotateFlipType)
		{
			// Fixme: implement me
			throw new NotImplementedException ();
		}

		protected InternalImageInfo sourceImageInfo = null;
		unsafe InternalImageInfo IImage.ConvertToInternalImageInfo ()
		{

			if (sourceImageInfo == null) {
				sourceImageInfo = new InternalImageInfo();
				sourceImageInfo.Size = size;
				sourceImageInfo.RawFormat = imageFormat;
				sourceImageInfo.PixelFormat = PixelFormat.Format32bppArgb;
				sourceImageInfo.Stride = Gdk.Pixbuf.GetRowstride (state);
				sourceImageInfo.RawImageBytes = new byte[sourceImageInfo.Stride * size.Height];
				IntPtr memptr = Gdk.Pixbuf.GetPixels (state);
				Marshal.Copy( memptr, sourceImageInfo.RawImageBytes, 0, sourceImageInfo.RawImageBytes.Length);
				sourceImageInfo.ChangePixelFormat (pixelFormat);
			}
			return sourceImageInfo;
		}

		void IImage.Save (string filename)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		void IImage.Save(Stream stream, ImageFormat format)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		void IImage.Save (string filename, ImageFormat format)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		int IImage.SelectActiveFrame (FrameDimension dimension, int frameIndex)
		{
			throw new NotImplementedException();
		}
	
		[MonoTODO]
		void IImage.SetPropertyItem (PropertyItem item)
		{
			throw new NotImplementedException();
		}
		// destructor
		~Image() {}

		// properties
		int IImage.Flags {
			get {
				throw new NotImplementedException ();
			}
		}
	
		Guid [] IImage.FrameDimensionsList {
			get {
				throw new NotImplementedException ();
			}
		}
	
		int IImage.Height {
			get {
				return size.Height;
			}
		}
	
		float IImage.HorizontalResolution {
			get {
				throw new NotImplementedException ();
			}
		}
	
		ColorPalette IImage.Palette {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
	
		SizeF IImage.PhysicalDimension {
			get {
				throw new NotImplementedException ();
			}
		}
	
		PixelFormat IImage.PixelFormat {
			get {
				return pixelFormat;
			}
		}
	
		int [] IImage.PropertyIdList {
			get {
				throw new NotImplementedException ();
			}
		}
	
		PropertyItem [] IImage.PropertyItems {
			[MonoTODO]
			get {
				throw new NotImplementedException();
			}
		}

		ImageFormat IImage.RawFormat {
			[MonoTODO]
			get {
				return imageFormat;
			}
		}

		Size IImage.Size {
			get {
				return size;
			}
		}
	
		float IImage.VerticalResolution {
			get {
				throw new NotImplementedException ();
			}
		}
	
		int IImage.Width {
			get {
				return size.Width;
			}
		}

		[MonoTODO]
		public void Dispose ()
		{
		}

		[MonoTODO]
		protected virtual void Dispose (bool disposing)
		{
			throw new NotImplementedException();
		}
	}
}
