using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace System.Drawing {

	[Serializable]
	[ComVisible (true)]
	[Editor ("System.Drawing.Design.FontEditor, " + Consts.AssemblySystem_Drawing_Design, typeof (System.Drawing.Design.UITypeEditor))]
	[TypeConverter(typeof(FontConverter))]
	public sealed class Font : MarshalByRefObject, ISerializable, ICloneable, IDisposable
	{
       	private Font (SerializationInfo info, StreamingContext context)
		{
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
		}

		public void Dispose ()
		{
		}

		public static Font FromHfont(IntPtr font)
		{
			// FIXME: 
			Font result = new Font("Arial", (float)12.0, FontStyle.Regular);
			return result;
		}

		public IntPtr ToHfont () { 	/*throw new NotImplementedException ();*/ return (IntPtr)100; }

		public Font(Font original, FontStyle style)
		{
			_bold = original.Bold;
			_fontFamily = original.FontFamily;
			_gdiCharSet = original.GdiCharSet;
			_gdiVerticalFont = original.GdiVerticalFont;
			_height = original.Height;
			_italic = original.Italic;
			_name = original.Name;
			_size = original.Size;
			_sizeInPoints = original.SizeInPoints;
			_strikeout = original.Strikeout;
			_underline = original.Underline;
			_unit = original.Unit;
			_style = style;
		}

		public Font(FontFamily family, float emSize)
			: this(family, emSize, FontStyle.Regular, GraphicsUnit.Point, (byte)0, false)
		{
		}

		public Font(FontFamily family, float emSize, FontStyle style)
			: this(family, emSize, style, GraphicsUnit.Point, (byte)0, false)
		{
		}

		public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit)
			: this(family, emSize, style, unit, (byte)0, false)
		{
		}

		public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte charSet)
			: this(family, emSize, style, unit, charSet, false)
		{
		}

		public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte charSet, bool isVertical)
		{
		}

		public Font(string familyName, float emSize)
			: this(familyName, emSize, FontStyle.Regular, GraphicsUnit.Point, (byte)0, false)
		{
		}

		public Font(string familyName, float emSize, FontStyle style)
			: this(familyName, emSize, style, GraphicsUnit.Point, (byte)0, false)
		{
		}
		
		public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit)
			: this(familyName, emSize, style, unit, (byte)0, false)
		{
		}
		
		public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte charSet)
			: this(familyName, emSize, style, unit, charSet, false)
		{
		}
		
		public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte charSet, bool isVertical)
		{
			//throw new NotImplementedException ();
		}
		
		object ICloneable.Clone()
		{
			throw new NotImplementedException ();
		}
		
		private bool _bold;
		public bool Bold {
			get {
				return _bold;
			}
		}
		
		private FontFamily _fontFamily;
		public FontFamily FontFamily {
			get {
				return _fontFamily;
			}
		}
		
		private byte _gdiCharSet;
		public byte GdiCharSet {
			get {
				return _gdiCharSet;
			}
		}
		
		private bool _gdiVerticalFont;
		public bool GdiVerticalFont {
			get {
				return _gdiVerticalFont;
			}
		}
		
		private int _height;
		public int Height {
			get {
				return _height;
			}
		}

		private bool _italic;
		public bool Italic {
			get {
				return _italic;
			}
		}

		private string _name;
		public string Name {
			get {
				return _name;
			}
		}

		private float _size;
		public float Size {
			get {
				return _size;
			}
		}

		private float _sizeInPoints;
		public float SizeInPoints {
			get {
				return _sizeInPoints;
			}
		}

		private bool _strikeout;
		public bool Strikeout {
			get {
				return _strikeout;
			}
		}
		
		private FontStyle _style;
		public FontStyle Style {
			get {
				return _style;
			}
		}

		private bool _underline;
		public bool Underline {
			get {
				return _underline;
			}
		}

		private GraphicsUnit _unit;
		public GraphicsUnit Unit {
			get {
				return _unit;
			}
		}
	}
}
