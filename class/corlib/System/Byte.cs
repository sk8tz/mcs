//
// System.Byte.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;
using System.Text.RegularExpressions;

namespace System {
	
	public struct Byte : IComparable, IFormattable { //, IConvertible {
		private static Type Type = typeof (byte);
		
		public const byte MinValue = 0;
		public const byte MaxValue = 255;
		
		// VES needs to know about value.  public is workaround
		// so source will compile
		public byte value;

		public int CompareTo (object v)
		{
			if (v == null)
				return 1;

			if (!(v is System.Byte))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Byte"));

			byte xv = (byte) v;

			if (value == xv)
				return 0;
			if (value > xv)
				return 1;
			else
				return -1;
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Byte))
				return false;

			return ((byte) o) == value;
		}

		public override int GetHashCode ()
		{
			return value;
		}

		public static byte Parse (string s)
		{
			return Parse (s, NumberStyles.Integer, null);
		}

		public static byte Parse (string s, IFormatProvider fp)
		{
			return Parse (s, NumberStyles.Integer, fp);
		}

		public static byte Parse (string s, NumberStyles style)
		{
			return Parse (s, style, null);
		}

		public static byte Parse (string s, NumberStyles style, IFormatProvider fp)
		{
			if (null == s){
				throw new ArgumentNullException();
			}

			// TODO: Handle other styles and FormatProvider properties
			if (NumberStyles.Integer == style){
				// the pattern allowed is: [ws][+]digits[ws]
				// [ws] = optional whitespace (any amount)
				// [+] = one optional plus sign
				// digits = one or more characters '0' through '9'
				Regex bytePattern = new Regex("^\\s*\\+?\\d+\\s*$");
				if (bytePattern.IsMatch(s)){
					int retVal = 0;
					int endIndex;
					// extract the digits from the string
					Regex byteDigitPattern = new Regex("\\d+");
					Match m = byteDigitPattern.Match(s);
					// find out the index number where the digits end
					endIndex = m.Index+m.Length-1;
					// work back-to-front through the digits and
					// multiply each digit by the next power of 10 (start at 0)
					for(int j = endIndex; j>=m.Index; j--){
						retVal += (s[j]-'0')*(int)Math.Pow(10,endIndex-j);
						if (retVal > Byte.MaxValue || retVal < Byte.MinValue){    
							throw new OverflowException();
						}
					}
					return (byte)retVal;
				}
				else {
				    throw new FormatException();
				}
				
			}
			else {
				throw new NotImplementedException();
			}
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
			string fmt;
			NumberFormatInfo nfi;
			
			fmt = (format == null) ? "G" : format;
			
			if (fp == null)
				nfi = NumberFormatInfo.CurrentInfo;
			else {
				nfi = (NumberFormatInfo) fp.GetFormat (Type);
				
				if (nfi == null)
					nfi = NumberFormatInfo.CurrentInfo;
			}

			return IntegerFormatter.NumberToString (fmt, nfi, value);
		}

		// =========== IConvertible Methods =========== //
		
		public TypeCode GetTypeCode ()
		{
			return TypeCode.Byte;
		}
	}
}
