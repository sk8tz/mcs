//
// System.UInt32.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;
using System.Threading;

namespace System {

	[CLSCompliant(false)]
	[Serializable]
	public struct UInt32 : IComparable, IFormattable, IConvertible {

		public const uint MaxValue = 0xffffffff;
		public const uint MinValue = 0;
		
		public uint value;

		public int CompareTo (object v)
		{
			if (v == null)
				return 1;
			
			if (!(v is System.UInt32))
				throw new ArgumentException (Locale.GetText ("Value is not a System.UInt32"));

			if (value == (uint) v)
				return 0;

			if (value < (uint) v)
				return -1;

			return 1;
		}

		public override bool Equals (object o)
		{
			if (!(o is System.UInt32))
				return false;

			return ((uint) o) == value;
		}

		public override int GetHashCode ()
		{
			return (int) value;
		}

		[CLSCompliant(false)]
		public static uint Parse (string s)
		{
			uint val = 0;
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
					uint d = (uint) (c - '0');
					
					val = checked (val * 10 + d);
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

		[CLSCompliant(false)]
		public static uint Parse (string s, IFormatProvider fp)
		{
			return Parse (s, NumberStyles.Integer, fp);
		}

		[CLSCompliant(false)]
		public static uint Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		private static void CheckStyle (NumberStyles style)
		{
			if ((style & NumberStyles.AllowHexSpecifier) != 0) {
				NumberStyles ne = style ^ NumberStyles.AllowHexSpecifier;
				if ((ne & NumberStyles.AllowLeadingWhite) != 0)
					ne ^= NumberStyles.AllowLeadingWhite;
				if ((ne & NumberStyles.AllowTrailingWhite) != 0)
					ne ^= NumberStyles.AllowTrailingWhite;
				if (ne != 0)
					throw new ArgumentException (
						"With AllowHexSpecifier only " + 
						"AllowLeadingWhite and AllowTrailingWhite " + 
						"are permitted.");
			}
		}
	
		private static int JumpOverWhite (int pos, string s, bool excp)
		{
			while (pos < s.Length && Char.IsWhiteSpace (s [pos]))
				pos++;

			if (excp && pos >= s.Length)
				throw new FormatException ("Input string was not in the correct format.");

			return pos;
		}

		private static void FindSign (ref int pos, string s, NumberFormatInfo nfi, 
				      ref bool foundSign, ref bool negative)
		{
			if ((pos + nfi.NegativeSign.Length) <= s.Length &&
			     s.Substring (pos, nfi.NegativeSign.Length) == nfi.NegativeSign) {
				negative = true;
				foundSign = true;
				pos += nfi.NegativeSign.Length;
			} 
			else if ((pos + nfi.PositiveSign.Length) < s.Length &&
			     s.Substring (pos, nfi.PositiveSign.Length) == nfi.PositiveSign) {
				negative = false;
				pos += nfi.PositiveSign.Length;
				foundSign = true;
			} 
		}

		private static void FindCurrency (ref int pos,
						  string s, 
						  NumberFormatInfo nfi,
						  ref bool foundCurrency)
		{
			if ((pos + nfi.CurrencySymbol.Length) <= s.Length &&
			     s.Substring (pos, nfi.CurrencySymbol.Length) == nfi.CurrencySymbol) {
				foundCurrency = true;
				pos += nfi.CurrencySymbol.Length;
			} 
		}

		private static bool FindOther (ref int pos,
					       string s, 
					       string other)
		{
			if ((pos + other.Length) <= s.Length &&
			     s.Substring (pos, other.Length) == other) {
				pos += other.Length;
				return true;
			} 

			return false;
		}

		private static bool ValidDigit (char e, bool allowHex)
		{
			if (allowHex)
				return Char.IsDigit (e) || (e >= 'A' && e <= 'F') || (e >= 'a' && e <= 'f');

			return Char.IsDigit (e);
		}
		
		[CLSCompliant(false)]
		public static uint Parse (string s, NumberStyles style, IFormatProvider fp)
		{
			if (s == null)
				throw new ArgumentNullException ();

			if (s.Length == 0)
				throw new FormatException ("Input string was not in the correct format.");

			NumberFormatInfo nfi;
			if (fp != null) {
				Type typeNFI = typeof (System.Globalization.NumberFormatInfo);
				nfi = (NumberFormatInfo) fp.GetFormat (typeNFI);
			}
			else
				nfi = Thread.CurrentThread.CurrentCulture.NumberFormat;

			CheckStyle (style);

			bool AllowCurrencySymbol = (style & NumberStyles.AllowCurrencySymbol) != 0;
			bool AllowExponent = (style & NumberStyles.AllowExponent) != 0;
			bool AllowHexSpecifier = (style & NumberStyles.AllowHexSpecifier) != 0;
			bool AllowThousands = (style & NumberStyles.AllowThousands) != 0;
			bool AllowDecimalPoint = (style & NumberStyles.AllowDecimalPoint) != 0;
			bool AllowParentheses = (style & NumberStyles.AllowParentheses) != 0;
			bool AllowTrailingSign = (style & NumberStyles.AllowTrailingSign) != 0;
			bool AllowLeadingSign = (style & NumberStyles.AllowLeadingSign) != 0;
			bool AllowTrailingWhite = (style & NumberStyles.AllowTrailingWhite) != 0;
			bool AllowLeadingWhite = (style & NumberStyles.AllowLeadingWhite) != 0;

			int pos = 0;

			if (AllowLeadingWhite)
				pos = JumpOverWhite (pos, s, true);

			bool foundOpenParentheses = false;
			bool negative = false;
			bool foundSign = false;
			bool foundCurrency = false;

			// Pre-number stuff
			if (AllowParentheses && s [pos] == '(') {
				foundOpenParentheses = true;
				foundSign = true;
				negative = true; // MS always make the number negative when there parentheses
						 // even when NumberFormatInfo.NumberNegativePattern != 0!!!
				pos++;
				if (AllowLeadingWhite)
					pos = JumpOverWhite (pos, s, true);

				if (s.Substring (pos, nfi.NegativeSign.Length) == nfi.NegativeSign)
					throw new FormatException ("Input string was not in the correct format.");
				if (s.Substring (pos, nfi.PositiveSign.Length) == nfi.PositiveSign)
					throw new FormatException ("Input string was not in the correct format.");
			}

			if (AllowLeadingSign && !foundSign) {
				// Sign + Currency
				FindSign (ref pos, s, nfi, ref foundSign, ref negative);
				if (foundSign) {
					if (AllowLeadingWhite)
						pos = JumpOverWhite (pos, s, true);
					if (AllowCurrencySymbol) {
						FindCurrency (ref pos, s, nfi,
							      ref foundCurrency);
						if (foundCurrency && AllowLeadingWhite)
							pos = JumpOverWhite (pos, s, true);
					}
				}
			}
			
			if (AllowCurrencySymbol && !foundCurrency) {
				// Currency + sign
				FindCurrency (ref pos, s, nfi, ref foundCurrency);
				if (foundCurrency) {
					if (AllowLeadingWhite)
						pos = JumpOverWhite (pos, s, true);
					if (foundCurrency) {
						if (!foundSign && AllowLeadingSign) {
							FindSign (ref pos, s, nfi, ref foundSign,
								  ref negative);
							if (foundSign && AllowLeadingWhite)
								pos = JumpOverWhite (pos, s, true);
						}
					}
				}
			}
			
			uint number = 0;
			int nDigits = 0;
			bool decimalPointFound = false;
			uint digitValue;
			char hexDigit;
				
			// Number stuff
			// Just the same as Int32, but this one adds instead of substract
			do {

				if (!ValidDigit (s [pos], AllowHexSpecifier)) {
					if (AllowThousands &&
					    FindOther (ref pos, s, nfi.NumberGroupSeparator))
					    continue;
					else
					if (!decimalPointFound && AllowDecimalPoint &&
					    FindOther (ref pos, s, nfi.NumberDecimalSeparator)) {
					    decimalPointFound = true;
					    continue;
					}

					break;
				}
				else if (AllowHexSpecifier) {
					nDigits++;
					hexDigit = s [pos++];
					if (Char.IsDigit (hexDigit))
						digitValue = (uint) (hexDigit - '0');
					else if (Char.IsLower (hexDigit))
						digitValue = (uint) (hexDigit - 'a' + 10);
					else
						digitValue = (uint) (hexDigit - 'A' + 10);

					number = checked (number * 16 + digitValue);
				}
				else if (decimalPointFound) {
					nDigits++;
					// Allows decimal point as long as it's only 
					// followed by zeroes.
					if (s [pos++] != '0')
						throw new OverflowException ("Value too large or too small.");
				}
				else {
					nDigits++;

					try {
						number = checked (
							number * 10 + 
							(uint) (s [pos++] - '0')
							);
					} catch (OverflowException) {
						throw new OverflowException ("Value too large or too small.");
					}
				}
			} while (pos < s.Length);

			// Post number stuff
			if (nDigits == 0)
				throw new FormatException ("Input string was not in the correct format.");

			if (AllowTrailingSign && !foundSign) {
				// Sign + Currency
				FindSign (ref pos, s, nfi, ref foundSign, ref negative);
				if (foundSign) {
					if (AllowTrailingWhite)
						pos = JumpOverWhite (pos, s, true);
					if (AllowCurrencySymbol)
						FindCurrency (ref pos, s, nfi,
							      ref foundCurrency);
				}
			}
			
			if (AllowCurrencySymbol && !foundCurrency) {
				// Currency + sign
				FindCurrency (ref pos, s, nfi, ref foundCurrency);
				if (foundCurrency) {
					if (AllowTrailingWhite)
						pos = JumpOverWhite (pos, s, true);
					if (!foundSign && AllowTrailingSign)
						FindSign (ref pos, s, nfi, ref foundSign,
							  ref negative);
				}
			}
			
			if (AllowTrailingWhite && pos < s.Length)
				pos = JumpOverWhite (pos, s, false);

			if (foundOpenParentheses) {
				if (pos >= s.Length || s [pos++] != ')')
					throw new FormatException ("Input string was not in the correct " + 
								   "format.");
				if (AllowTrailingWhite && pos < s.Length)
					pos = JumpOverWhite (pos, s, false);
			}

			if (pos < s.Length)
				throw new FormatException ("Input string was not in the correct format.");

			if (negative)
				throw new OverflowException ( "Value too large or too small.");

			return number;
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
			return TypeCode.UInt32;
		}

		public bool ToBoolean (IFormatProvider provider)
		{
			return System.Convert.ToBoolean (value);
		}

		public byte ToByte (IFormatProvider provider)
		{
			return System.Convert.ToByte (value);
		}

		public char ToChar (IFormatProvider provider)
		{
			return System.Convert.ToChar (value);
		}

		public DateTime ToDateTime (IFormatProvider provider)
		{
			return System.Convert.ToDateTime (value);
		}

		public decimal ToDecimal (IFormatProvider provider)
		{
			return System.Convert.ToDecimal (value);
		}

		public double ToDouble (IFormatProvider provider)
		{
			return System.Convert.ToDouble (value);
		}

		public short ToInt16 (IFormatProvider provider)
		{
			return System.Convert.ToInt16 (value);
		}

		public int ToInt32 (IFormatProvider provider)
		{
			return System.Convert.ToInt32 (value);
		}

		public long ToInt64 (IFormatProvider provider)
		{
			return System.Convert.ToInt64 (value);
		}

		[CLSCompliant (false)]
		public sbyte ToSByte (IFormatProvider provider)
		{
			return System.Convert.ToSByte (value);
		}
		
		public float ToSingle (IFormatProvider provider)
		{
			return System.Convert.ToSingle (value);
		}

		public object ToType (Type conversionType, IFormatProvider provider)
		{
			return System.Convert.ToType (value, conversionType, provider);
		}

		[CLSCompliant (false)]
		public ushort ToUInt16 (IFormatProvider provider)
		{
			return System.Convert.ToUInt16 (value);
		}

		[CLSCompliant (false)]
		public uint ToUInt32 (IFormatProvider provider)
		{
			return System.Convert.ToUInt32 (value);
		}

		[CLSCompliant (false)]
		public ulong ToUInt64 (IFormatProvider provider)
		{
			return System.Convert.ToUInt64 (value);
		}
	}
}
