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
		
		internal double value;

		public int CompareTo (object v)
		{
			if (v == null)
				return 1;
			
			if (!(v is System.Double))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Double"));

			double dv = (double)v;

			if (IsPositiveInfinity(value) && IsPositiveInfinity(dv))
				return 0;

			if (IsNegativeInfinity(value) && IsNegativeInfinity(dv))
				return 0;

			if (IsNaN(dv))
				if (IsNaN(value))
					return 0;
				else
					return 1;

			if (IsNaN(value))
				if (IsNaN(dv))
					return 0;
				else
					return -1;

			if (value > dv) return 1;
			else if (value < dv) return -1;
			else return 0;
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

		// We're intentionally using constants here to avoid some bigger headaches in mcs.
		// This struct must be compiled before System.Enum so we can't use enums here.
		private const int State_AllowSign = 1;
		private const int State_Digits = 2;
		private const int State_Decimal = 3;
		private const int State_ExponentSign = 4;
		private const int State_Exponent = 5;
		private const int State_ConsumeWhiteSpace = 6;
		
		[MonoTODO("check if digits are group in correct numbers between the group separators")]
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
			int state = State_AllowSign;

			//
			// Setup
			//
			string decimal_separator = null;
			string group_separator = null;
			int decimal_separator_len = 0;
			int group_separator_len = 0;
			if ((style & NumberStyles.AllowDecimalPoint) != 0){
				decimal_separator = format.NumberDecimalSeparator;
				decimal_separator_len = decimal_separator.Length;
			}
			if ((style & NumberStyles.AllowThousands) != 0){
				group_separator = format.NumberGroupSeparator;
				group_separator_len = group_separator.Length;
			}
			string positive = format.PositiveSign;
			string negative = format.NegativeSign;
			
			for (; sidx < len; sidx++){
				c = s [sidx];

				switch (state){
				case State_AllowSign:
					if ((style & NumberStyles.AllowLeadingSign) != 0){
						if (c == positive [0] &&
						    s.Substring (sidx, positive.Length) == positive){
							state = State_Digits;
							sidx += positive.Length-1;
							continue;
						}

						if (c == negative [0] &&
						    s.Substring (sidx, negative.Length) == negative){
							state = State_Digits;
							b [didx++] = (byte) '-';
							sidx += negative.Length-1;
							continue;
						}
					}
					state = State_Digits;
					goto case State_Digits;
					
				case State_Digits:
					if (Char.IsDigit (c)){
						b [didx++] = (byte) c;
						break;
					}
					if (c == 'e' || c == 'E')
						goto case State_Decimal;
					
					if (decimal_separator != null &&
					    decimal_separator [0] == c){
						if (s.Substring (sidx, decimal_separator_len) ==
						    decimal_separator){
							b [didx++] = (byte) '.';
							sidx += decimal_separator_len-1;
							state = State_Decimal; 
							break;
						}
					}
					if (group_separator != null &&
					    group_separator [0] == c){
						if (s.Substring (sidx, group_separator_len) ==
						    group_separator){
							sidx += group_separator_len-1;
							state = State_Digits; 
							break;
						}
					}
					
					if (Char.IsWhiteSpace (c))
						goto case State_ConsumeWhiteSpace;

					throw new FormatException ("Unknown char: " + c);

				case State_Decimal:
					if (Char.IsDigit (c)){
						b [didx++] = (byte) c;
						break;
					}

					if (c == 'e' || c == 'E'){
						if ((style & NumberStyles.AllowExponent) == 0)
							throw new FormatException ("Unknown char: " + c);
						b [didx++] = (byte) c;
						state = State_ExponentSign;
						break;
					}
					
					if (Char.IsWhiteSpace (c))
						goto case State_ConsumeWhiteSpace;
					throw new FormatException ("Unknown char: " + c);

				case State_ExponentSign:
					if (Char.IsDigit (c)){
						state = State_Exponent;
						goto case State_Exponent;
					}

					if (c == positive [0] &&
					    s.Substring (sidx, positive.Length) == positive){
						state = State_Digits;
						sidx += positive.Length-1;
						continue;
					}

					if (c == negative [0] &&
					    s.Substring (sidx, negative.Length) == negative){
						state = State_Digits;
						b [didx++] = (byte) '-';
						sidx += negative.Length-1;
						continue;
					}

					if (Char.IsWhiteSpace (c))
						goto case State_ConsumeWhiteSpace;
					
					throw new FormatException ("Unknown char: " + c);

				case State_Exponent:
					if (Char.IsDigit (c)){
						b [didx++] = (byte) c;
						break;
					}
					
					if (Char.IsWhiteSpace (c))
						goto case State_ConsumeWhiteSpace;
					throw new FormatException ("Unknown char: " + c);

				case State_ConsumeWhiteSpace:
					if (allow_trailing_white && Char.IsWhiteSpace (c))
						break;
					throw new FormatException ("Unknown char");
				}
			}

			b [didx] = 0;
			unsafe {
				fixed (byte *p = &b [0]){
					double retVal = ParseImpl (p);
					if (IsPositiveInfinity(retVal) || IsNegativeInfinity(retVal))
						throw new OverflowException();

					return retVal;
				}
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		unsafe private static extern double ParseImpl (byte *byte_ptr);
		
		public static bool TryParse (string s,
					     NumberStyles style,
					     IFormatProvider provider,
					     ref double result)
		{
			try {
				result = Parse (s, style, provider);
				return true;
			} catch {
				return false;
			}
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
			if (fp is CultureInfo)
				return DoubleFormatter.NumberToString(format,
				((CultureInfo)fp).NumberFormat, value);
			else
				return DoubleFormatter.NumberToString(format,
				(NumberFormatInfo)fp, value);
		}

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
