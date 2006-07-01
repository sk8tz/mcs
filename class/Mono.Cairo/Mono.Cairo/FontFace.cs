using System;

namespace Cairo
{
	public class FontFace
	{
		IntPtr handle;

		public FontFace (IntPtr handle)
		{
			this.handle = handle;
		}

		public IntPtr Handle {
			get { return handle; }
		}

#if CAIRO_1_2
		public FontType FontType {
			get { return CairoAPI.cairo_font_face_get_type (handle); }
		}
#endif
	}
}

