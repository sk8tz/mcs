//
// System.SByte.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {

	[CLSCompliant(false)]
	[Serializable]
	public struct SByte : IComparable, IFormattable, IConvertible {

		public const sbyte MinValue = -128;
		public const sbyte MaxValue = 127;
		
		// VES needs to know about value.  public is workaround
		// so source will compile
		public sbyte value;

		public int CompareTo (object v)
		{
			if (v == null)
				return 1;
			
			if (!(v is System.SByte))
				throw new ArgumentException (Locale.GetText ("Value is not a System.SByte"));

			sbyte xv = (sbyte) v;
			if (value == xv)
				return 0;
			if (value > xv)
				return 1;
			else
				return -1;
		}

		public override bool Equals (object o)
		{
			if (!(o is System.SByte))
				return false;

			return ((sbyte) o) == value;
		}

		public override int GetHashCode ()
		{
			return value;
		}

		public static sbyte Parse (string s)
		{
			int ival = 0;
			int len;
			int i;
			bool neg = false;
			bool digits_seen = false;
			
			if (s == null)
				throw new ArgumentNullException (Locale.GetText ("s is null"));

			len = s.Length;

			char c;
			for (i = 0; i < len; i++){
				c = s [i];
				if (!Char.IsWhiteSpace (c))
					break;
			}
			
			if (i == len)
				throw new FormatException ();

			c = s [i];
			if (c == '+')
				i++;
			else if (c == '-'){
				neg = true;
				i++;
			}
			
			for (; i < len; i++){
				c = s [i];

				if (c >= '0' && c <= '9'){
					ival = checked (ival * 10 - (int) (c - '0'));
					digits_seen = true;
				} else {
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
			if (!digits_seen)
				throw new FormatException ();
			
			ival = neg ? ival : -ival;
			if (ival < SByte.MinValue || ival > SByte.MaxValue)
				throw new OverflowException ();

			return (sbyte) ival;
		}

		public static sbyte Parse (string s, IFormatProvider fp)
		{
			return Parse (s, NumberStyles.Integer, fp);
		}

		public static sbyte Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		public static sbyte Parse (string s, NumberStyles style, IFormatProvider fp)
		{
			int tmpResult = Int32.Parse (s, style, fp);
			if (tmpResult > SByte.MaxValue || tmpResult < SByte.MinValue)
				throw new OverflowException ("Value too large or too small.");

			return (sbyte) tmpResult;
		}

		public override string ToString ()
		{
			return ToString (null, null);
		}

		public string ToString (IFormatProvider fp)
		{
			return ToString (null, fp);
		}

		public string ToString (string format)
		{
			return ToString (format, null);
		}

		public string ToString (string format, IFormatProvider fp)
		{
			NumberFormatInfo nfi = NumberFormatInfo.GetInstance( fp );
			
			if ( format == null )
				format = "G";
			
			return IntegerFormatter.NumberToString(format, nfi, value);
		}

		// =========== ICovnertible Methods =========== //

		public TypeCode GetTypeCode ()
		{
			return TypeCode.SByte;
		}
		public bool     ToBoolean  (IFormatProvider provider)
		{
			return System.Convert.ToBoolean (value);
		}
		public byte     ToByte     (IFormatProvider provider)
		{
			return System.Convert.ToByte (value);
		}
		public char     ToChar     (IFormatProvider provider)
		{
			return System.Convert.ToChar (value);
		}
		public DateTime ToDateTime (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}
		public decimal  ToDecimal  (IFormatProvider provider)
		{
			return System.Convert.ToDecimal (value);
		}
		public double   ToDouble   (IFormatProvider provider)
		{
			return System.Convert.ToDouble (value);
		}
		public short    ToInt16    (IFormatProvider provider)
		{
			return System.Convert.ToInt16 (value);
		}
		public int      ToInt32    (IFormatProvider provider)
		{
			return System.Convert.ToInt32 (value);
		}
		public long     ToInt64    (IFormatProvider provider)
		{
			return System.Convert.ToInt64 (value);
		}
    		[CLSCompliant(false)]
		public sbyte    ToSByte    (IFormatProvider provider)
		{
			return value;
		}
		public float    ToSingle   (IFormatProvider provider)
		{
			return System.Convert.ToSingle (value);
		}
		public object   ToType     (Type conversionType, IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}
    		[CLSCompliant(false)]
		public ushort   ToUInt16   (IFormatProvider provider)
		{
			return System.Convert.ToUInt16 (value);
		}
    		[CLSCompliant(false)]
		public uint     ToUInt32   (IFormatProvider provider)
		{
			return System.Convert.ToUInt32 (value);
		}
    		[CLSCompliant(false)]
		public ulong    ToUInt64   (IFormatProvider provider)
		{
			return System.Convert.ToUInt64 (value);
		}
	}
}
