using System.Runtime.Serialization;

namespace System.Drawing {

	[Serializable]
	public sealed class Font : MarshalByRefObject, ISerializable, ICloneable, IDisposable
	{
		internal IFont implementation;
		internal static IFontFactory factory = Factories.GetFontFactory();

        	private Font (SerializationInfo info, StreamingContext context)
		{
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
		}

		public void Dispose ()
		{
			implementation.Dispose();
		}

		public static Font FromHfont(IntPtr font)
		{
			// FIXME: 
			Font result = new Font("Arial", (float)12.0, FontStyle.Regular);
			result.implementation = factory.FromHfont(font);
			return result;
		}

		public IntPtr ToHfont () { return implementation.ToHfont(); }

		public Font(Font original, FontStyle style)
		{
			throw new NotImplementedException();
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
			implementation = factory.Font(family, emSize, style, unit, charSet, isVertical);
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
			implementation = factory.Font(familyName, emSize, style, unit, charSet, isVertical);
		}
		
		object ICloneable.Clone()
		{
			throw new NotImplementedException ();
		}
		
		public bool Bold {
			get {
				return implementation.Bold;
			}
		}
		
		public FontFamily FontFamily {
			get {
				return implementation.FontFamily;
			}
		}
		
		public byte GdiCharSet {
			get {
				return implementation.GdiCharSet;
			}
		}
		
		public bool GdiVerticalFont {
			get {
				return implementation.GdiVerticalFont;
			}
		}
		
		public int Height {
			get {
				return implementation.Height;
			}
		}

		public bool Italic {
			get {
				return implementation.Italic;
			}
		}

		public string Name {
			get {
				return implementation.Name;
			}
		}

		public float Size {
			get {
				return implementation.Size;
			}
		}

		public float SizeInPoints {
			get {
				return implementation.SizeInPoints;
			}
		}

		public bool Strikeout {
			get {
				return implementation.Strikeout;
			}
		}
		
		public FontStyle Style {
			get {
				return implementation.Style;
			}
		}

		public bool Underline {
			get {
				return implementation.Underline;
			}
		}

		public GraphicsUnit Unit {
			get {
				return implementation.Unit;
			}
		}
		
	}
}
