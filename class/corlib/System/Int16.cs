//
// System.Int16.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {
	
	public struct Int16 : IComparable, IFormattable { //, IConvertible {
		public const short MaxValue =  32767;
		public const short MinValue = -32768;
		
		// VES needs to know about value.  public is workaround
		// so source will compile
		public short value;

		public int CompareTo (object v)
		{
			if (!(v is System.Int16))
				throw new ArgumentException ("Value is not a System.Int16");

			return value - ((short) v);
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Int16))
				return false;

			return ((short) o) == value;
		}

		public override int GetHashCode ()
		{
			return value;
		}

		public static short Parse (string s)
		{
			return Parse (s, NumberStyles.Integer, null);
		}

		public static short Parse (string s, IFormatProvider fp)
		{
			return Parse (s, NumberStyles.Integer, fp);
		}

		public static short Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		public static short Parse (string s, NumberStyles style, IFormatProvider fp)
		{
			// TODO: Implement me
			return 0;
		}

		public override string ToString ()
		{
			return ToString ("G", null);
		}

		public string ToString (IFormatProvider fp)
		{
			return ToString ("G", fp);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
		}

		public string ToString (string format, IFormatProvider fp)
		{
			// TODO: Implement me.
			return "";
		}

		// =========== IConvertible Methods =========== //

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Int16;
		}
	}
}
