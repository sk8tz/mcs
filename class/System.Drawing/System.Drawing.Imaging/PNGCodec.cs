//
// System.Drawing.Imaging.PNGCodec.cs
//
// Author: 
//		Alexandre Pigolkine (pigolkine@gmx.de)
//

namespace System.Drawing.Imaging
{
	using System;
	using System.IO;
	using System.Drawing.Imaging;
	using System.Runtime.InteropServices;
	using cdeclCallback;

	/// <summary>
	/// Summary description for PNGCodec.
	/// </summary>
	internal class PNGCodec
	{
		
		enum PNG_LIB : int {
			PNG_COLOR_MASK_PALETTE    =1,
			PNG_COLOR_MASK_COLOR      =2,
			PNG_COLOR_MASK_ALPHA      =4,
			
			PNG_COLOR_TYPE_GRAY =0,
			PNG_COLOR_TYPE_PALETTE  =(PNG_COLOR_MASK_COLOR | PNG_COLOR_MASK_PALETTE),
			PNG_COLOR_TYPE_RGB        =(PNG_COLOR_MASK_COLOR),
			PNG_COLOR_TYPE_RGB_ALPHA  =(PNG_COLOR_MASK_COLOR | PNG_COLOR_MASK_ALPHA),
			PNG_COLOR_TYPE_GRAY_ALPHA =(PNG_COLOR_MASK_ALPHA),
			
			PNG_COLOR_TYPE_RGBA  =PNG_COLOR_TYPE_RGB_ALPHA,
			PNG_COLOR_TYPE_GA  =PNG_COLOR_TYPE_GRAY_ALPHA,
			
			PNG_COMPRESSION_TYPE_BASE =0, /* Deflate method 8, 32K window */
			PNG_COMPRESSION_TYPE_DEFAULT =PNG_COMPRESSION_TYPE_BASE,
			
			PNG_FILTER_TYPE_BASE      =0, /* Single row per-byte filtering */
			PNG_INTRAPIXEL_DIFFERENCING =64, /* Used only in MNG datastreams */
			PNG_FILTER_TYPE_DEFAULT   =PNG_FILTER_TYPE_BASE,
			
			PNG_INTERLACE_NONE        =0, /* Non-interlaced image */
			PNG_INTERLACE_ADAM7       =1, /* Adam7 interlacing */
			PNG_INTERLACE_LAST        =2, /* Not a valid value */
			
			PNG_OFFSET_PIXEL          =0, /* Offset in pixels */
			PNG_OFFSET_MICROMETER     =1, /* Offset in micrometers (1/10^6 meter) */
			PNG_OFFSET_LAST           =2, /* Not a valid value */
			
			PNG_EQUATION_LINEAR       =0, /* Linear transformation */
			PNG_EQUATION_BASE_E       =1, /* Exponential base e transform */
			PNG_EQUATION_ARBITRARY    =2, /* Arbitrary base exponential transform */
			PNG_EQUATION_HYPERBOLIC   =3, /* Hyperbolic sine transformation */
			PNG_EQUATION_LAST         =4, /* Not a valid value */
			
			PNG_SCALE_UNKNOWN         =0, /* unknown unit (image scale) */
			PNG_SCALE_METER           =1, /* meters per pixel */
			PNG_SCALE_RADIAN          =2, /* radians per pixel */
			PNG_SCALE_LAST            =3, /* Not a valid value */
			
			PNG_RESOLUTION_UNKNOWN    =0, /* pixels/unknown unit (aspect ratio) */
			PNG_RESOLUTION_METER      =1, /* pixels/meter */
			PNG_RESOLUTION_LAST       =2 /* Not a valid value */
		}
		
		const string PNG_LIBPNG_VER_STRING = "1.2.2";
		const string PNGLibrary = "png";
		
		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern IntPtr png_create_read_struct (string user_png_ver, IntPtr error_ptr, 
							cdeclCallback.cdeclRedirector.MethodVoidIntPtrIntPtr error_fn, 
							cdeclCallback.cdeclRedirector.MethodVoidIntPtrIntPtr warn_fn);

		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern IntPtr png_create_write_struct (string user_png_ver, IntPtr error_ptr, 
							cdeclCallback.cdeclRedirector.MethodVoidIntPtrIntPtr error_fn, 
							cdeclCallback.cdeclRedirector.MethodVoidIntPtrIntPtr warn_fn);
		
		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern IntPtr png_create_info_struct (IntPtr png_ptr);
		
		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern void png_destroy_read_struct (ref IntPtr png_ptr, ref IntPtr info_ptr_ptr, ref IntPtr end_info_ptr_ptr);
		
		[DllImport(PNGLibrary, EntryPoint="png_destroy_read_struct", CallingConvention=CallingConvention.Cdecl)]
		internal static extern void png_destroy_read_struct1 (ref IntPtr png_ptr, IntPtr info_ptr_ptr, IntPtr end_info_ptr_ptr);
		
		[DllImport(PNGLibrary, EntryPoint="png_destroy_read_struct", CallingConvention=CallingConvention.Cdecl)]
		internal static extern void png_destroy_read_struct2 (ref IntPtr png_ptr, ref IntPtr info_ptr_ptr, IntPtr end_info_ptr_ptr);

		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern void png_destroy_write_struct (ref IntPtr png_ptr, ref IntPtr info_ptr_ptr);
		
		[DllImport(PNGLibrary, EntryPoint="png_destroy_write_struct", CallingConvention=CallingConvention.Cdecl)]
		internal static extern void png_destroy_write_struct1 (ref IntPtr png_ptr, IntPtr info_ptr_ptr);
		
		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern void png_set_read_fn (IntPtr png_ptr, IntPtr io_ptr, cdeclCallback.cdeclRedirector.MethodVoidIntPtrIntPtrInt read_data_fn);
		
		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern void png_set_write_fn (IntPtr png_ptr, IntPtr io_ptr, 
							cdeclCallback.cdeclRedirector.MethodVoidIntPtrIntPtrInt write_data_fn,
							cdeclCallback.cdeclRedirector.MethodVoidIntPtr output_flush_fn);
		
		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern void png_read_info (IntPtr png_ptr, IntPtr info_ptr);
		
		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern void png_write_info (IntPtr png_ptr, IntPtr info_ptr);
		
		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern void png_read_end (IntPtr png_ptr, IntPtr end_info);

		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern void png_write_end (IntPtr png_ptr, IntPtr end_info);
		
		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern int png_get_rowbytes (IntPtr png_ptr, IntPtr info_ptr);
		
		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern int png_get_image_width (IntPtr png_ptr, IntPtr info_ptr);
		
		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern int png_get_image_height (IntPtr png_ptr, IntPtr info_ptr);
		
		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern byte png_get_bit_depth (IntPtr png_ptr, IntPtr info_ptr);
		
		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern byte png_get_color_type (IntPtr png_ptr, IntPtr info_ptr);
		
		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern void png_set_IHDR (IntPtr png_ptr, IntPtr info_ptr, int width, int height, 
								int bit_depth, int color_type, int interlace_method, int compression_method,
   								int filter_method);
		
		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern void png_read_row (IntPtr png_ptr, IntPtr row_data, IntPtr display_row);

		[DllImport(PNGLibrary, CallingConvention=CallingConvention.Cdecl)]
		internal static extern void png_write_row (IntPtr png_ptr, IntPtr row_data);
		
		
		internal PNGCodec() {
		}

		internal static ImageCodecInfo CodecInfo {
			get {
				ImageCodecInfo info = new ImageCodecInfo();
				info.Flags = ImageCodecFlags.Encoder | ImageCodecFlags.Decoder | ImageCodecFlags.Builtin | ImageCodecFlags.SupportBitmap;
				info.FormatDescription = "PNG file format";
				info.FormatID = System.Drawing.Imaging.ImageFormat.Png.Guid;
				info.MimeType = "image/png";
				info.Version = 1;
				byte[][] signaturePatterns = new byte[1][];
				signaturePatterns[0] = new byte[]{0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a};
				info.SignaturePatterns = signaturePatterns;
				byte[][] signatureMasks = new byte[1][];
				signatureMasks[0] = new byte[]{0xff,0xff,0xff,0xff,0xff,0xff};
				info.SignatureMasks = signatureMasks;
				info.decode += new ImageCodecInfo.DecodeFromStream(PNGCodec.DecodeDelegate);
				info.encode += new ImageCodecInfo.EncodeToStream(PNGCodec.EncodeDelegate);
				return info;
			}
		}

		void error_function (IntPtr png_structp, IntPtr png_const_charp) 
		{
			// FIXME: set exception parameters
			throw new Exception();
		}

		void warning_function (IntPtr png_structp, IntPtr png_const_charp) 
		{
			// FIXME: dump error somewhere
		}

		Stream fs;

		void read_data_fn (IntPtr png_structp, IntPtr bytep, int size)
		{
			byte[] result = new byte[size];
			int readed = fs.Read(result, 0, size);
			Marshal.Copy(result, 0, bytep, readed);
		}
		
		void write_data_fn (IntPtr png_structp, IntPtr bytep, int size)
		{
			// FIXME: shall we have a buffer as a member variable here ?
			byte[] result = new byte[size];
			Marshal.Copy(bytep, result, 0, size);
			fs.Write(result, 0, size);
		}
		
		void output_flush_fn (IntPtr png_structp)
		{
			fs.Flush();
		}
		
		internal static void DecodeDelegate (Image image, Stream stream, BitmapData info) 
		{
			PNGCodec png = new PNGCodec();
			png.Decode (image, stream, info);
		}

		internal static void EncodeDelegate (Image image, Stream stream) 
		{
			PNGCodec png = new PNGCodec();
			BitmapData info = ((Bitmap)image).LockBits (new Rectangle (new Point (0,0), image.Size),
									  ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
			png.Encode (image, stream, info);
			((Bitmap)image).UnlockBits (info);
		}

		internal unsafe bool Decode (Image image, Stream stream, BitmapData info) 
		{
			fs = stream;
		
			IntPtr png_ptr = png_create_read_struct (PNG_LIBPNG_VER_STRING, IntPtr.Zero, 
								new cdeclCallback.cdeclRedirector.MethodVoidIntPtrIntPtr(this.error_function), 
								new cdeclCallback.cdeclRedirector.MethodVoidIntPtrIntPtr(this.warning_function));
								
			if (png_ptr == IntPtr.Zero) return false;
			
			IntPtr info_ptr = png_create_info_struct (png_ptr);
			if (info_ptr == IntPtr.Zero) {
				IntPtr dummy = IntPtr.Zero;
				png_destroy_read_struct1 (ref png_ptr, IntPtr.Zero, IntPtr.Zero);
				return false;
			}

			IntPtr end_info = png_create_info_struct (png_ptr);
			if (end_info == IntPtr.Zero) {
				IntPtr dummy = IntPtr.Zero;
				png_destroy_read_struct2 (ref png_ptr, ref info_ptr, IntPtr.Zero);
				return false;
			}
			
			png_set_read_fn (png_ptr, IntPtr.Zero, new cdeclCallback.cdeclRedirector.MethodVoidIntPtrIntPtrInt(this.read_data_fn));
			
			png_read_info (png_ptr, info_ptr);
			
			int height = png_get_image_height (png_ptr, info_ptr);
			int stride = png_get_rowbytes (png_ptr, info_ptr);
			stride = (stride + 3) & ~3;
			
			info.Width = png_get_image_width (png_ptr, info_ptr);
			info.Height = height;
			info.Stride = stride;
			// FIXME: do a real palette processing
			//info.Palette = new ColorPalette(1, cinfo.ColorMap);
			// FIXME: get color information from png info structure
			info.PixelFormat = PixelFormat.Format24bppRgb;
			info.Scan0 = Marshal.AllocHGlobal (height * stride);
			
			byte *start = (byte *) (void *) info.Scan0;
			IntPtr scanline;
			for (int row = 0; row < height; row++) {
				scanline = (IntPtr) start;
				png_read_row (png_ptr, scanline, IntPtr.Zero);
				start += stride;
			}
			
			png_read_end (png_ptr, end_info);
			
 			png_destroy_read_struct (ref png_ptr, ref info_ptr, ref end_info);			
 			
			info.swap_red_blue_bytes();
			
			return true;
		}

		internal unsafe bool Encode (Image image, Stream stream, BitmapData info) 
		{
			int bpp = Image.GetPixelFormatSize(info.PixelFormat) / 8;
			if (bpp != 3 && bpp != 4) {
				throw new ArgumentException(String.Format("Supplied pixel format is not yet supported: {0}, {1} bpp", info.PixelFormat, Image.GetPixelFormatSize(info.PixelFormat)));
			}

			fs = stream;
		
			IntPtr png_ptr = png_create_write_struct (PNG_LIBPNG_VER_STRING, IntPtr.Zero, 
								new cdeclCallback.cdeclRedirector.MethodVoidIntPtrIntPtr(this.error_function), 
								new cdeclCallback.cdeclRedirector.MethodVoidIntPtrIntPtr(this.warning_function));
								
			if (png_ptr == IntPtr.Zero) return false;
			
			IntPtr info_ptr = png_create_info_struct (png_ptr);
			if (info_ptr == IntPtr.Zero) {
				IntPtr dummy = IntPtr.Zero;
				png_destroy_write_struct1 (ref png_ptr, IntPtr.Zero);
				return false;
			}
			
			png_set_write_fn (png_ptr, IntPtr.Zero, 
						new cdeclCallback.cdeclRedirector.MethodVoidIntPtrIntPtrInt(this.write_data_fn),
						new cdeclCallback.cdeclRedirector.MethodVoidIntPtr(this.output_flush_fn));
						
			png_set_IHDR (png_ptr, info_ptr, info.Width, info.Height, 8, 
							(int)PNG_LIB.PNG_COLOR_TYPE_RGB/*(Image.IsAlphaPixelFormat(info.Format) ? (int)PNG_LIB.PNG_COLOR_TYPE_RGB_ALPHA : (int)PNG_LIB.PNG_COLOR_TYPE_RGB)*/,
							(int)PNG_LIB.PNG_INTERLACE_NONE, (int)PNG_LIB.PNG_COMPRESSION_TYPE_DEFAULT, (int)PNG_LIB.PNG_FILTER_TYPE_DEFAULT);
							
			png_write_info (png_ptr, info_ptr);

			info.swap_red_blue_bytes ();

			byte *start = (byte *) (void *) info.Scan0;
			IntPtr scanline;
			int stride = info.Stride;
			for (int row = 0; row < info.Height; row++) {
				scanline = (IntPtr) start;
				png_write_row (png_ptr,  scanline);
				start += stride;
			}
			
			png_write_end (png_ptr, info_ptr);
 			png_destroy_write_struct (ref png_ptr, ref info_ptr);
			
			info.swap_red_blue_bytes ();

			return true;
		}
	}
}
