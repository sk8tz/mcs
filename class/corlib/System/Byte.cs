//
// System.Byte.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System
{
	[Serializable]
	public struct Byte : IComparable, IFormattable, IConvertible
	{
		public const byte MinValue = 0;
		public const byte MaxValue = 255;

		internal byte m_value;

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;

			if (!(value is System.Byte))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Byte."));

			byte xv = (byte) value;

			if (m_value == xv)
				return 0;
			if (m_value > xv)
				return 1;
			else
				return -1;
		}

		public override bool Equals (object obj)
		{
			if (!(obj is System.Byte))
				return false;

			return ((byte) obj) == m_value;
		}

		public override int GetHashCode ()
		{
			return m_value;
		}

		public static byte Parse (string s)
		{
			byte val = 0;
			int len;
			int i;
			bool digits_seen = false;
			bool negative = false;

			if (s == null)
				throw new ArgumentNullException ("s");

			len = s.Length;

			// look for the first non-whitespace character
			char c;
			for (i = 0; i < len; i++){
				c = s [i];
				if (!Char.IsWhiteSpace (c))
					break;
			}

			// if it's all whitespace, then throw exception
			if (i == len)
				throw new FormatException ();

			// look for the optional '+' sign
			if (s [i] == '+')
				i++;
			else if (s [i] == '-') {
				negative = true;
				i++;
			}

			// we should just have numerals followed by whitespace now
			for (; i < len; i++){
				c = s [i];

				if (c >= '0' && c <= '9'){
					// shift left and accumulate every time we find a numeral
					byte d = (byte) (c - '0');

					val = checked ((byte) (val * 10 + d));
					digits_seen = true;
				} else {
					// after the last numeral, only whitespace is allowed
					if (Char.IsWhiteSpace (c)){
						for (i++; i < len; i++){
							if (!Char.IsWhiteSpace (s [i]))
								throw new FormatException ();
						}
						break;
					} else
						throw new FormatException ();
				}
			}

			// -0 is legal but other negative values are not
			if (negative && (val > 0)) {
				throw new OverflowException (
					Locale.GetText ("Negative number"));
			}

			// if all we had was a '+' sign, then throw exception
			if (!digits_seen)
				throw new FormatException ();

			return val;
		}

		public static byte Parse (string s, IFormatProvider provider)
		{
			return Parse (s, NumberStyles.Integer, provider);
		}

		public static byte Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		public static byte Parse (string s, NumberStyles style, IFormatProvider provider)
		{
			uint tmpResult = UInt32.Parse (s, style, provider);
			if (tmpResult > Byte.MaxValue || tmpResult < Byte.MinValue)
				throw new OverflowException (Locale.GetText ("Value too large or too small."));

			return (byte) tmpResult;
		}

		public override string ToString ()
		{
			return ToString (null, null);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
		}

		public string ToString (IFormatProvider provider)
		{
			return ToString (null, provider);
		}

		public string ToString (string format, IFormatProvider provider)
		{
			NumberFormatInfo nfi = NumberFormatInfo.GetInstance (provider);

			// null or empty ("")
			if ((format == null) || (format.Length == 0))
				format = "G";

			return IntegerFormatter.NumberToString (format, nfi, m_value);
		}

		// =========== IConvertible Methods =========== //
		public TypeCode GetTypeCode ()
		{
			return TypeCode.Byte;
		}

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			return System.Convert.ToType (m_value, conversionType, provider);
		}

		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			return System.Convert.ToBoolean (m_value);
		}

		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return m_value;
		}

		char IConvertible.ToChar (IFormatProvider provider)
		{
			return System.Convert.ToChar (m_value);
		}

		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			throw new InvalidCastException ();
		}

		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{
			return System.Convert.ToDecimal (m_value);
		}

		double IConvertible.ToDouble (IFormatProvider provider)
		{
			return System.Convert.ToDouble (m_value);
		}

		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			return System.Convert.ToInt16 (m_value);
		}

		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			return System.Convert.ToInt32 (m_value);
		}

		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			return System.Convert.ToInt64 (m_value);
		}

		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return System.Convert.ToSByte (m_value);
		}

		float IConvertible.ToSingle (IFormatProvider provider)
		{
			return System.Convert.ToSingle (m_value);
		}

/*
		string IConvertible.ToString (IFormatProvider provider)
		{
			return ToString("G", provider);
		}
*/

		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return System.Convert.ToUInt16 (m_value);
		}

		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return System.Convert.ToUInt32 (m_value);
		}

		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return System.Convert.ToUInt64 (m_value);
		}
	}
}
