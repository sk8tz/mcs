//
// System.Double.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Bob Smith       (bob@thestuff.net)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) Bob Smith.    http://www.thestuff.net
//

using System.Globalization;
using System.Runtime.CompilerServices;

namespace System {
	
	[Serializable]
	public struct Double : IComparable, IFormattable, IConvertible {
		public const double Epsilon = 4.9406564584124650e-324;
		public const double MaxValue =  1.7976931348623157e308;
		public const double MinValue = -1.7976931348623157e308;
		public const double NaN = 0.0d / 0.0d;
		public const double NegativeInfinity = -1.0d / 0.0d;
		public const double PositiveInfinity = 1.0d / 0.0d;
		
		// VES needs to know about value.  public is workaround
		// so source will compile
		public double value;

		public int CompareTo (object v)
		{
			if (v == null)
				return 1;
			
			if (!(v is System.Double))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Double"));

			if (IsPositiveInfinity(value) && IsPositiveInfinity((double) v)){
				return 0;
			}

			if (IsNegativeInfinity(value) && IsNegativeInfinity((double) v)){
				return 0;
			}

			if (IsNaN((double) v)) {
				if (IsNaN(value))
					return 0;
				else
					return 1;
			}

			return (int) (value - ((double) v));
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Double))
				return false;

			if (IsNaN ((double)o)) {
				if (IsNaN(value))
					return true;
				else
					return false;
			}

			return ((double) o) == value;
		}

		public override int GetHashCode ()
		{
			return (int) value;
		}

		public static bool IsInfinity (double d)
		{
			return (d == PositiveInfinity || d == NegativeInfinity);
		}

		public static bool IsNaN (double d)
		{
			return (d != d);
		}

		public static bool IsNegativeInfinity (double d)
		{
			return (d < 0.0d && (d == NegativeInfinity || d == PositiveInfinity));
		}

		public static bool IsPositiveInfinity (double d)
		{
			return (d > 0.0d && (d == NegativeInfinity || d == PositiveInfinity));
		}

		public static double Parse (string s)
		{
			return Parse (s, (NumberStyles.Float | NumberStyles.AllowThousands), null);
		}

		public static double Parse (string s, IFormatProvider fp)
		{
			return Parse (s, (NumberStyles.Float | NumberStyles.AllowThousands), fp);
		}

		public static double Parse (string s, NumberStyles style) 
		{
			return Parse (s, style, null);
		}

		enum State {
			AllowSign, Digits, Decimal, ExponentSign, Exponent, ConsumeWhiteSpace
		}
		
		public static double Parse (string s, NumberStyles style, IFormatProvider provider)
		{
			if (s == null) throw new ArgumentNullException();
			if (style > NumberStyles.Any)
			{
				throw new ArgumentException();
			}
			NumberFormatInfo format = NumberFormatInfo.GetInstance(provider);
			if (format == null) throw new Exception("How did this happen?");
			if (s == format.NaNSymbol) return Double.NaN;
			if (s == format.PositiveInfinitySymbol) return Double.PositiveInfinity;
			if (s == format.NegativeInfinitySymbol) return Double.NegativeInfinity;

			//
			// validate and prepare string for C
			//
			int len = s.Length;
			byte [] b = new byte [len + 1];
			int didx = 0;
			int sidx = 0;
			char c;
			
			if ((style & NumberStyles.AllowLeadingWhite) != 0){
				while (sidx < len && Char.IsWhiteSpace (c = s [sidx]))
				       sidx++;

				if (sidx == len)
					throw new FormatException();
			}

			bool allow_trailing_white = ((style & NumberStyles.AllowTrailingWhite) != 0);

			//
			// Machine state
			//
			State state = State.AllowSign;

			//
			// Setup
			//
			string decimal_separator = null;
			int decimal_separator_len = 0;
			if ((style & NumberStyles.AllowDecimalPoint) != 0){
				decimal_separator = format.NumberDecimalSeparator;
				decimal_separator_len = decimal_separator.Length;
			}
			string positive = format.PositiveSign;
			string negative = format.NegativeSign;
			
			for (; sidx < len; sidx++){
				c = s [sidx];

				switch (state){
				case State.AllowSign:
					if ((style & NumberStyles.AllowLeadingSign) != 0){
						if (c == positive [0] &&
						    s.Substring (sidx, positive.Length) == positive){
							state = State.Digits;
							sidx += positive.Length-1;
							continue;
						}

						if (c == negative [0] &&
						    s.Substring (sidx, negative.Length) == negative){
							state = State.Digits;
							b [didx++] = (byte) '-';
							sidx += negative.Length-1;
							continue;
						}
					}
					state = State.Digits;
					goto case State.Digits;
					
				case State.Digits:
					if (Char.IsDigit (c)){
						b [didx++] = (byte) c;
						break;
					}
					if (c == 'e' || c == 'E')
						goto case State.Decimal;
					
					if (decimal_separator != null &&
					    decimal_separator [0] == c){
						if (s.Substring (sidx, decimal_separator_len) ==
						    decimal_separator){
							b [didx++] = (byte) '.';
							sidx += decimal_separator_len-1;
							state = State.Decimal; 
							break;
						}
					}
					
					if (Char.IsWhiteSpace (c))
						goto case State.ConsumeWhiteSpace;

					throw new FormatException ("Unknown char: " + c);

				case State.Decimal:
					if (Char.IsDigit (c)){
						b [didx++] = (byte) c;
						break;
					}

					if (c == 'e' || c == 'E'){
						if ((style & NumberStyles.AllowExponent) == 0)
							throw new FormatException ("Unknown char: " + c);
						b [didx++] = (byte) c;
						state = State.ExponentSign;
						break;
					}
					
					if (Char.IsWhiteSpace (c))
						goto case State.ConsumeWhiteSpace;
					throw new FormatException ("Unknown char: " + c);

				case State.ExponentSign:
					if (Char.IsDigit (c)){
						state = State.Exponent;
						goto case State.Exponent;
					}

					if (c == positive [0] &&
					    s.Substring (sidx, positive.Length) == positive){
						state = State.Digits;
						sidx += positive.Length-1;
						continue;
					}

					if (c == negative [0] &&
					    s.Substring (sidx, negative.Length) == negative){
						state = State.Digits;
						b [didx++] = (byte) '-';
						sidx += negative.Length-1;
						continue;
					}

					if (Char.IsWhiteSpace (c))
						goto case State.ConsumeWhiteSpace;
					
					throw new FormatException ("Unknown char: " + c);

				case State.Exponent:
					if (Char.IsDigit (c)){
						b [didx++] = (byte) c;
						break;
					}
					
					if (Char.IsWhiteSpace (c))
						goto case State.ConsumeWhiteSpace;
					throw new FormatException ("Unknown char: " + c);

				case State.ConsumeWhiteSpace:
					if (allow_trailing_white && Char.IsWhiteSpace (c))
						break;
					throw new FormatException ("Unknown char");
				}
			}

			b [didx] = 0;
			unsafe {
				fixed (byte *p = &b [0]){
					return ParseImpl (p);
				}
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		unsafe private static extern double ParseImpl (byte *byte_ptr);
		
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

		[MonoTODO]
		public string ToString (string format, IFormatProvider fp)
		{
			// FIXME: Need to pass format and provider info to this call too.
			return ToStringImpl(value);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern string ToStringImpl (double value);

		// =========== IConvertible Methods =========== //

		public TypeCode GetTypeCode ()
		{
			return TypeCode.Double;
		}

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			return System.Convert.ToType(value, conversionType, provider);
		}
		
		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			return System.Convert.ToBoolean(value);
		}
		
		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return System.Convert.ToByte(value);
		}
		
		char IConvertible.ToChar (IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		[CLSCompliant(false)]
		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{
			return System.Convert.ToDecimal(value);
		}
		
		double IConvertible.ToDouble (IFormatProvider provider)
		{
			return System.Convert.ToDouble(value);
		}
		
		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			return System.Convert.ToInt16(value);
		}
		
		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			return System.Convert.ToInt32(value);
		}
		
		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			return System.Convert.ToInt64(value);
		}
		
		[CLSCompliant(false)] 
		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return System.Convert.ToSByte(value);
		}
		
		float IConvertible.ToSingle (IFormatProvider provider)
		{
			return System.Convert.ToSingle(value);
		}
		
/*
		string IConvertible.ToString (IFormatProvider provider)
		{
			return ToString(provider);
		}
*/

		[CLSCompliant(false)]
		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return System.Convert.ToUInt16(value);
		}
		
		[CLSCompliant(false)]
		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return System.Convert.ToUInt32(value);
		}
		
		[CLSCompliant(false)]
		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return System.Convert.ToUInt64(value);
		}
	}
}
