//
// System.Int64.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {
	
	public struct Int64 : IComparable, IFormattable { //, IConvertible {
		public const long MaxValue = 0x7fffffffffffffff;
		public const long MinValue = -9223372036854775808;
		
		public long value;

		public int CompareTo (object v)
		{
			if (!(v is System.Int64))
				throw new ArgumentException ("Value is not a System.Int64");

			if (value == (long) v)
				return 0;

			if (value < (long) v)
				return -1;

			return 1;
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Int64))
				return false;

			return ((long) o) == value;
		}

		public override int GetHashCode ()
		{
			return (int)(value & 0xffffffff) ^ (int)(value >> 32);
		}

		public static long Parse (string s)
		{
			return Parse (s, NumberStyles.Integer, null);
		}

		public static long Parse (string s, IFormatProvider fp)
		{
			return Parse (s, NumberStyles.Integer, fp);
		}

		public static long Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		public static long Parse (string s, NumberStyles style, IFormatProvider fp)
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
			return TypeCode.Int64;
		}
	}
}
