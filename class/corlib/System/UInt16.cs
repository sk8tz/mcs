//
// System.UInt16.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {

	[CLSCompliant(false)]
	public struct UInt16 : IComparable, IFormattable { //, IConvertible {

		public const ushort MaxValue = 0xffff;
		public const ushort MinValue = 0;
		
		public ushort value;

		public int CompareTo (object v)
		{
			if (v == null)
				return 1;
			
			if(!(v is System.UInt16))
				throw new ArgumentException (Locale.GetText ("Value is not a System.UInt16"));

			return value - ((ushort) v);
		}

		public override bool Equals (object o)
		{
			if (!(o is System.UInt16))
				return false;

			return ((ushort) o) == value;
		}

		public override int GetHashCode ()
		{
			return value;
		}

		public static ushort Parse (string s)
		{
			ushort val = 0;
			int len;
			int i;
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

			if (s [i] == '+')
				i++;

			for (; i < len; i++){
				c = s [i];

				if (c >= '0' && c <= '9'){
					ushort d = (ushort) (c - '0');
					
					val = checked ((ushort) (val * 10 + d));
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
			
			return val;

		}

		public static ushort Parse (string s, IFormatProvider fp)
		{
			return Parse (s, NumberStyles.Integer, fp);
		}

		public static ushort Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		[MonoTODO]
		public static ushort Parse (string s, NumberStyles style, IFormatProvider fp)
		{
			// TODO: Implement me
			throw new NotImplementedException ();
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

		// =========== IConvertible Methods =========== //

		public TypeCode GetTypeCode ()
		{
			return TypeCode.UInt16;
		}
	}
}
