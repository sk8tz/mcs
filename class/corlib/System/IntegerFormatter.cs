//
// System.IntegerFormatter.cs
//
// Author:
//   Derek Holden  (dholden@draper.com)
//
// (C) Derek Holden  dholden@draper.com
//

//
// Format integer types. Completely based off ECMA docs
// for IFormattable specification. Has been tested w/ 
// all integral types, from boundry to boundry, w/ all 
// formats A## ("G", "G0" ... "G99", "P", "P0" ... "P99").
//
// If you make any changes, please make sure to check the
// boundry format precisions (0, 99) and the min / max values
// of the data types (Int32.[Max/Min]Value).
//
// Using int as an example, it is currently set up as
//
// Int32 {
//   int value;
//   public string ToString (string format, NumberFormatInfo nfi) {
//      return IntegerFormatter.NumberToString (format, nfi, value);
//   }
//
// IntegerFormatter {
//   public string NumberToString (string format, NumberFormatInfo nfi, int value) {
//      ParseFormat (format);
//      switch (format type) {
//        case 'G' FormatGeneral(value, precision);
//        case 'R' throw Exception("Invalid blah blah");
//        case 'C' FromatCurrency(value, precision, nfi);
//        etc...
//      }
//   }
// }
//
// There is a property in NumberFormatInfo for NegativeSign, though the 
// definition of IFormattable just uses '-' in context. So all the 
// hardcoded uses of '-' in here may need to be changed to nfi.NegativeSign
//
// For every integral type.
//
// Before every Format<Format Type> block there is a small paragraph
// detailing its requirements, and a blurb of what I was thinking
// at the time.
//
// Some speedup suggestions to be done when after this appears
// to be working properly:
//
//   * Deal w/ out of range numbers better. Specifically with
//     regards to boundry cases such as Long.MinValue etc.
//     The previous way of if (value < 0) value = -value;
//     fails under this assumption, since the largest
//     possible MaxValue is < absolute value of the MinValue.
//     I do the first iteration outside of the loop, and then
//     convert the number to positive, then continue in the loop.
//
//   * Replace all occurances of max<Type>Length with their 
//     numerical values. Plus the places where things are set
//     to max<Type>Length - 1. Hardcode these to numbers.
//
//   * Move the code for all the NumberToString()'s into the
//     the main ToString (string, NumberFormatInfo) method in
//     the data types themselves. That way they'd be throwing
//     their own exceptions on error and it'd save a function
//     call.
//
//   * For integer to char buffer transformation, you could
//     implement the calculations of the 10's and 100's place
//     the same time w/ another table to shorten loop time.
//
//   * Someone smarter can prolly find a much more efficient 
//     way of formatting the exponential notation. It's still
//     done in pass, just may have too many repositioning
//     calculations.
//   
//   * Decide whether it be better to have functions that
//     handle formatting for all types, or just cast their
//     values out and format them. Just if library size is
//     more important than speed in saving a cast and a 
//     function call.
//

using System;
using System.Collections;
using System.Globalization;

namespace System {

	public sealed class IntegerFormatter {

		private static int maxByteLength = 4;
		private static int maxShortLength = 6;
		private static int maxIntLength = 12;
		private static int maxLongLength = 22;

		private static char[] digitLowerTable;
/**
 * This makes a TypeNotInitialized exception be thrown.
 *		{ '0', '1', '2', '3', '4', '5', '6', '7', 
 *		  '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
 */

		private static char[] digitUpperTable;
/*
 *		{ '0', '1', '2', '3', '4', '5', '6', '7', 
 *		  '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
 */

		static IntegerFormatter ()
		{
			int i;

			digitLowerTable = new char[16];
			digitUpperTable = new char[16];

			for (i = 0; i < 10; i++){
				digitLowerTable[i] = (char) ('0' + i);
				digitUpperTable[i] = (char) ('0' + i);
			}

			char lc = (char ) ('a' - i);
			char uc = (char ) ('A' - i);
			while (i < 16){
				digitLowerTable[i] = (char) (lc + i);
				digitUpperTable[i] = (char) (uc + i);
				i++;
			}
		}

		private static bool IsDigit (char c)
		{
			return !(c < '0' || c > '9'); 
		}
		
		private static bool IsLetter (char c)
		{
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'); 
		}
 
		private static bool ParseFormat (string format, out char specifier,  out int precision, out bool custom)
		{		 		 
			precision = -1;
			specifier = '\0';
			custom = false;
			
			int length = format.Length;
			// TODO: Could an empty string be a custom format string?
			if (length < 1)
				return false;
			
			char[] chars = format.ToCharArray ();
			specifier = chars[0];

			// TODO: IsLetter() and IsDigit() should be replaced by Char.Is*()
			if (IsLetter(specifier) && length <= 3) {
				switch (length){
				case 1:
					return true;
				case 2:
					if (IsDigit(chars[1])) {
						precision = chars[1] - '0';
						return true;
					}
					break;
				case 3:
					if (IsDigit(chars[1]) && IsDigit(chars[2])) {
						precision = chars[1] - '0';
						precision = precision * 10 + chars[2] - '0';
						return true;
					}
					break;
				}
				
			}
			
			// We've got a custom format string.
			custom = true;
			return true;
		}	 

		// ============ Public Interface to all the integer types ============ //
		
		public static string NumberToString (string format, NumberFormatInfo nfi, byte value)
		{
			char specifier;
			int precision;
			bool custom;

			if (!ParseFormat (format, out specifier, out precision, out custom))
				throw new FormatException (Locale.GetText ("The specified format '" + format + "' is invalid"));
			
			if (custom){
				return FormatCustom (format, value, nfi);
			}

			switch(specifier) {
			case 'c': return FormatCurrency (value, precision, nfi);
			case 'C': return FormatCurrency (value, precision, nfi);
			case 'd': return FormatDecimal (value, precision);
			case 'D': return FormatDecimal (value, precision);
			case 'e': return FormatExponential (value, precision, false);
			case 'E': return FormatExponential (value, precision, true);
			case 'f': return FormatFixedPoint (value, precision, nfi);	
			case 'F': return FormatFixedPoint (value, precision, nfi);	
			case 'g': return FormatGeneral (value, precision, nfi, false);
			case 'G': return FormatGeneral (value, precision, nfi, true);
			case 'n': return FormatNumber (value, precision, nfi);
			case 'N': return FormatNumber (value, precision, nfi);
			case 'p': return FormatPercent (value, precision, nfi);
			case 'P': return FormatPercent (value, precision, nfi);
			case 'r': throw new FormatException (Locale.GetText ("The specified format cannot be used in this instance"));
			case 'R': throw new FormatException (Locale.GetText ("The specified format cannot be used in this instance"));
			case 'x': return FormatHexadecimal (value, precision, false);
			case 'X': return FormatHexadecimal (value, precision, true);
			default: 
				throw new FormatException (Locale.GetText ("The specified format '" + format + "' is invalid"));
			}
		}		

		public static string NumberToString (string format, NumberFormatInfo nfi, short value)
		{
			char specifier;
			int precision;
			bool custom;
			
			if (!ParseFormat (format, out specifier, out precision, out custom))
				throw new FormatException (Locale.GetText ("The specified format '" + format + "' is invalid"));
			
			if (custom){
				return FormatCustom (format, value, nfi);
			}

			switch(specifier) {
			case 'c': return FormatCurrency (value, precision, nfi);
			case 'C': return FormatCurrency (value, precision, nfi);
			case 'd': return FormatDecimal (value, precision);
			case 'D': return FormatDecimal (value, precision);
			case 'e': return FormatExponential (value, precision, false);
			case 'E': return FormatExponential (value, precision, true);
			case 'f': return FormatFixedPoint (value, precision, nfi);	
			case 'F': return FormatFixedPoint (value, precision, nfi);	
			case 'g': return FormatGeneral (value, precision, nfi, false);
			case 'G': return FormatGeneral (value, precision, nfi, true);
			case 'n': return FormatNumber (value, precision, nfi);
			case 'N': return FormatNumber (value, precision, nfi);
			case 'p': return FormatPercent (value, precision, nfi);
			case 'P': return FormatPercent (value, precision, nfi);
			case 'r': throw new FormatException (Locale.GetText ("The specified format cannot be used in this instance"));
			case 'R': throw new FormatException (Locale.GetText ("The specified format cannot be used in this insance"));
			case 'x': return FormatHexadecimal (value, precision, false);
			case 'X': return FormatHexadecimal (value, precision, true);
			default: 
				throw new FormatException (Locale.GetText ("The specified format '" + format + "' is invalid"));
			}
		}

		public static string NumberToString (string format, NumberFormatInfo nfi, int value)
		{
			char specifier;
			int precision;
			bool custom;
			
			if (!ParseFormat (format, out specifier, out precision, out custom))
				throw new FormatException (Locale.GetText ("The specified format '" + format + "' is invalid"));
			
			if (custom){
				return FormatCustom (format, value, nfi);
			}

			switch(specifier) {
			case 'c': return FormatCurrency (value, precision, nfi);	
			case 'C': return FormatCurrency (value, precision, nfi);	
			case 'd': return FormatDecimal (value, precision);
			case 'D': return FormatDecimal (value, precision);
			case 'e': return FormatExponential (value, precision, false);
			case 'E': return FormatExponential (value, precision, true);
			case 'f': return FormatFixedPoint (value, precision, nfi);	
			case 'F': return FormatFixedPoint (value, precision, nfi);	
			case 'g': return FormatGeneral (value, precision, nfi, false);
			case 'G': return FormatGeneral (value, precision, nfi, true);
			case 'n': return FormatNumber (value, precision, nfi);
			case 'N': return FormatNumber (value, precision, nfi);
			case 'p': return FormatPercent (value, precision, nfi);
			case 'P': return FormatPercent (value, precision, nfi);
			case 'r': throw new FormatException (Locale.GetText ("The specified format cannot be used in this instance"));
			case 'R': throw new FormatException (Locale.GetText ("The specified format cannot be used in this instance"));
			case 'x': return FormatHexadecimal (value, precision, false);
			case 'X': return FormatHexadecimal (value, precision, true);
			default: 
				throw new FormatException (Locale.GetText ("The specified format '" + format + "' is invalid"));
			}
		}

		public static string NumberToString (string format, NumberFormatInfo nfi, long value)
		{
			char specifier;
			int precision;
			bool custom;
			
			if (!ParseFormat (format, out specifier, out precision, out custom))
				throw new FormatException (Locale.GetText ("The specified format '" + format + "' is invalid"));
			
			if (custom){
				return FormatCustom (format, value, nfi);
			}

			switch(specifier) {
			case 'c': return FormatCurrency (value, precision, nfi);
			case 'C': return FormatCurrency (value, precision, nfi);
			case 'd': return FormatDecimal (value, precision);
			case 'D': return FormatDecimal (value, precision);
			case 'e': return FormatExponential (value, precision, false);
			case 'E': return FormatExponential (value, precision, true);
			case 'f': return FormatFixedPoint (value, precision, nfi);	
			case 'F': return FormatFixedPoint (value, precision, nfi);	
			case 'g': return FormatGeneral (value, precision, nfi, false);
			case 'G': return FormatGeneral (value, precision, nfi, true);
			case 'n': return FormatNumber (value, precision, nfi);
			case 'N': return FormatNumber (value, precision, nfi);
			case 'p': return FormatPercent (value, precision, nfi);
			case 'P': return FormatPercent (value, precision, nfi);
			case 'r': throw new FormatException (Locale.GetText ("The specified format cannot be used in this instance"));
			case 'R': throw new FormatException (Locale.GetText ("The specified format cannot be used in this instance"));
			case 'x': return FormatHexadecimal (value, precision, false);
			case 'X': return FormatHexadecimal (value, precision, true);
			default: 
				throw new FormatException (Locale.GetText ("The specified format '" + format + "' is invalid"));
			}			
		}

		[CLSCompliant (false)]
		public static string NumberToString (string format, NumberFormatInfo nfi, sbyte value)
		{
			char specifier;
			int precision;
			bool custom;
			
			if (!ParseFormat (format, out specifier, out precision, out custom))
				throw new FormatException (Locale.GetText ("The specified format '" + format + "' is invalid"));
			
			if (custom){
				return FormatCustom (format, value, nfi);
			}
			
			switch(specifier) {
			case 'c': return FormatCurrency (value, precision, nfi);
			case 'C': return FormatCurrency (value, precision, nfi);
			case 'd': return FormatDecimal (value, precision);
			case 'D': return FormatDecimal (value, precision);
			case 'e': return FormatExponential (value, precision, false);
			case 'E': return FormatExponential (value, precision, true);
			case 'f': return FormatFixedPoint (value, precision, nfi);	
			case 'F': return FormatFixedPoint (value, precision, nfi);	
			case 'g': return FormatGeneral (value, precision, nfi, false);
			case 'G': return FormatGeneral (value, precision, nfi, true);
			case 'n': return FormatNumber (value, precision, nfi);
			case 'N': return FormatNumber (value, precision, nfi);
			case 'p': return FormatPercent (value, precision, nfi);
			case 'P': return FormatPercent (value, precision, nfi);
			case 'r': throw new FormatException (Locale.GetText ("The specified format cannot be used in this instance"));
			case 'R': throw new FormatException (Locale.GetText ("The specified format cannot be used in this instance"));
			case 'x': return FormatHexadecimal (value, precision, false);
			case 'X': return FormatHexadecimal (value, precision, true);
			default: 
				throw new FormatException (Locale.GetText ("The specified format '" + format + "' is invalid"));
			}
		}

		[CLSCompliant (false)]
		public static string NumberToString (string format, NumberFormatInfo nfi, ushort value)
		{
			char specifier;
			int precision;
			bool custom;
			
			if (!ParseFormat (format, out specifier, out precision, out custom))
				throw new FormatException (Locale.GetText ("The specified format '" + format + "' is invalid"));
			
			if (custom){
				return FormatCustom (format, value, nfi);
			}
			
			switch(specifier) {
			case 'c': return FormatCurrency (value, precision, nfi);
			case 'C': return FormatCurrency (value, precision, nfi);
			case 'd': return FormatDecimal (value, precision);
			case 'D': return FormatDecimal (value, precision);
			case 'e': return FormatExponential (value, precision, false);
			case 'E': return FormatExponential (value, precision, true);
			case 'f': return FormatFixedPoint (value, precision, nfi);	
			case 'F': return FormatFixedPoint (value, precision, nfi);	
			case 'g': return FormatGeneral (value, precision, nfi, false);
			case 'G': return FormatGeneral (value, precision, nfi, true);
			case 'n': return FormatNumber (value, precision, nfi);
			case 'N': return FormatNumber (value, precision, nfi);
			case 'p': return FormatPercent (value, precision, nfi);
			case 'P': return FormatPercent (value, precision, nfi);
			case 'r': throw new FormatException (Locale.GetText ("The specified format cannot be used in this instance"));
			case 'R': throw new FormatException (Locale.GetText ("The specified format cannot be used in this instance"));
			case 'x': return FormatHexadecimal (value, precision, false);
			case 'X': return FormatHexadecimal (value, precision, true);
			default: 
				throw new FormatException (Locale.GetText ("The specified format '" + format + "' is invalid"));
			}
		}

		[CLSCompliant (false)]
		public static string NumberToString (string format, NumberFormatInfo nfi, uint value)
		{
			char specifier;
			int precision;
			bool custom;
			
			if (!ParseFormat (format, out specifier, out precision, out custom))
				throw new FormatException (Locale.GetText ("The specified format '" + format + "' is invalid"));
			
			if (custom){
				return FormatCustom (format, value, nfi);
			}
			
			switch(specifier) {
			case 'c': return FormatCurrency (value, precision, nfi);
			case 'C': return FormatCurrency (value, precision, nfi);
			case 'd': return FormatDecimal (value, precision);
			case 'D': return FormatDecimal (value, precision);
			case 'e': return FormatExponential (value, precision, false);
			case 'E': return FormatExponential (value, precision, true);
			case 'f': return FormatFixedPoint (value, precision, nfi);	
			case 'F': return FormatFixedPoint (value, precision, nfi);	
			case 'g': return FormatGeneral (value, precision, nfi, false);
			case 'G': return FormatGeneral (value, precision, nfi, true);
			case 'n': return FormatNumber (value, precision, nfi);
			case 'N': return FormatNumber (value, precision, nfi);
			case 'p': return FormatPercent (value, precision, nfi);
			case 'P': return FormatPercent (value, precision, nfi);
			case 'r': throw new FormatException (Locale.GetText ("The specified format cannot be used in this instance"));
			case 'R': throw new FormatException (Locale.GetText ("The specified format cannot be used in this instance"));
			case 'x': return FormatHexadecimal (value, precision, false);
			case 'X': return FormatHexadecimal (value, precision, true);
			default: 
				throw new FormatException (Locale.GetText ("The specified format '" + format + "' is invalid"));
			}
		}

		[CLSCompliant (false)]
		public static string NumberToString (string format, NumberFormatInfo nfi, ulong value)
		{
			char specifier;
			int precision;
			bool custom;
			
			if (!ParseFormat (format, out specifier, out precision, out custom))
				throw new FormatException (Locale.GetText ("The specified format '" + format + "' is invalid"));
			
			if (custom){
				return FormatCustom (format, value, nfi);
			}
			
			switch(specifier) {
			case 'c': return FormatCurrency (value, precision, nfi);
			case 'C': return FormatCurrency (value, precision, nfi);
			case 'd': return FormatDecimal (value, precision);
			case 'D': return FormatDecimal (value, precision);
			case 'e': return FormatExponential (value, precision, false);
			case 'E': return FormatExponential (value, precision, true);
			case 'f': return FormatFixedPoint (value, precision, nfi);	
			case 'F': return FormatFixedPoint (value, precision, nfi);	
			case 'g': return FormatGeneral (value, precision, nfi, false);
			case 'G': return FormatGeneral (value, precision, nfi, true);
			case 'n': return FormatNumber (value, precision, nfi);
			case 'N': return FormatNumber (value, precision, nfi);
			case 'p': return FormatPercent (value, precision, nfi);
			case 'P': return FormatPercent (value, precision, nfi);
			case 'r': throw new FormatException (Locale.GetText ("The specified format cannot be used in this instance"));
			case 'R': throw new FormatException (Locale.GetText ("The specified format cannot be used in this instance"));
			case 'x': return FormatHexadecimal (value, precision, false);
			case 'X': return FormatHexadecimal (value, precision, true);
			default: 
				throw new FormatException (Locale.GetText ("The specified format '" + format + "' is invalid"));
			}
		}

		// ============ Currency Type Formating ============ //

		//
		//  Currency Format: Used for strings containing a monetary value. The
		//  CurrencySymbol, CurrencyGroupSizes, CurrencyGroupSeparator, and
		//  CurrencyDecimalSeparator members of a NumberFormatInfo supply
		//  the currency symbol, size and separator for digit groupings, and
		//  decimal separator, respectively.
		//  CurrencyNegativePattern and CurrencyPositivePattern determine the
		//  symbols used to represent negative and positive values. For example,
		//  a negative value may be prefixed with a minus sign, or enclosed in
		//  parentheses.
		//  If the precision specifier is omitted
		//  NumberFormatInfo.CurrencyDecimalDigits determines the number of
		//  decimal places in the string. Results are rounded to the nearest
		//  representable value when necessary.
		//
		//  The pattern of the NumberFormatInfo determines how the output looks, where
		//  the dollar sign goes, where the negative sign goes, etc.
		//  IFormattable documentation lists the patterns and their values,
		//  I have them commented out in the large switch statement
		//

		private static string FormatCurrency (byte value, int precision, NumberFormatInfo nfi) 
		{
			return FormatCurrency ((uint)value, precision, nfi);
		}

		private static string FormatCurrency (short value, int precision, NumberFormatInfo nfi) 
		{
			return FormatCurrency ((int)value, precision, nfi);			
		}
			
		private static string FormatCurrency (int value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;
			bool negative = (value < 0);

			char[] groupSeparator = nfi.CurrencyGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.CurrencyDecimalSeparator.ToCharArray ();
			char[] currencySymbol = nfi.CurrencySymbol.ToCharArray ();
			int[] groupSizes = nfi.CurrencyGroupSizes;
			int pattern = negative ? nfi.CurrencyNegativePattern : nfi.CurrencyPositivePattern;
			int symbolLength = currencySymbol.Length;
			
			int padding = (precision >= 0) ? precision : nfi.CurrencyDecimalDigits;	     
			int size = maxIntLength + (groupSeparator.Length * maxIntLength) + padding + 2 + 
			decimalSeparator.Length + symbolLength;	
			char[] buffy = new char[size];
			int position = size;

			// set up the pattern from IFormattible
			if (negative) {
				i = symbolLength; 
 
				switch (pattern) {
				case 0: // ($nnn)
					buffy[--position] = ')'; 
					break;
				// case 1: // -$nnn
				//	break;
				// case 2: // $-nnn
				//	break;
				case 3: // $nnn-
					buffy[--position] = '-';
					break;
				case 4:	// (nnn$)
					buffy[--position] = ')'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 5:	// -nnn$
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 6:	// nnn-$
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = '-'; 
					break;
				case 7: // nnn$-
					buffy[--position] = '-'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 8: // -nnn $
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					break;
				// case 9: // -$ nnn
				//	break;
				case 10: // nnn $-
					buffy[--position] = '-'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					break;
				case 11: // $ nnn-
					buffy[--position] = '-'; 
					break;
				// case 12: // $ -nnn
				//	break;
				case 13: // nnn- $
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					buffy[--position] = '-'; 
					break;
				case 14: // ($ nnn)
					buffy[--position] = ')'; 
					break;
				case 15: // (nnn $)
					buffy[--position] = ')'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;				
				}
			} else {
				i = symbolLength; 
				switch (pattern) {
				// case 0: // $nnn
				//	break;
				case 1: // nnn$
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				// case 2: // $ nnn
				//	break;
				case 3: // nnn $
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					break;
				}
			}
			
			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';

			// put on decimal separator if we moved over and put a 0 
			if (position < size && buffy[position] == '0') {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];

			// just in place to take care of the negative boundries (Int32.MinValue)
			if (negative) {
				if (value <= -10) {
					buffy[--position] = digitLowerTable[-(value % 10)];
					value = value / -10;
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				} else value = -value;
			}

			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			// end the pattern on the left hand side
			if (negative) {
				i = symbolLength; 
 
				switch (pattern) {
				case 0: // ($nnn)
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = '('; 
					break;
				case 1: // -$nnn
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = '-'; 
					break;
				case 2: // $-nnn
					buffy[--position] = '-'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 3: // $nnn-
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 4:	// (nnn$)
					buffy[--position] = '('; 
					break;
				case 5:	// -nnn$
					buffy[--position] = '-'; 
					break;
				// case 6: // nnn-$
				//	break;
				// case 7: // nnn$-
				//	break;
				case 8: // -nnn $
					buffy[--position] = '-'; 
					break;
				// case 9: // -$ nnn
				//	break;
				// case 10: // nnn $-
				//	break;
				case 11: // $ nnn-
					buffy[--position] = ' '; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 12: // $ -nnn
					buffy[--position] = '-'; 
					buffy[--position] = ' '; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				// case 13: // nnn- $
				//	break;
				case 14: // ($ nnn)
					buffy[--position] = ' '; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = '('; 
					break;
				case 15: // (nnn $)
					buffy[--position] = '('; 
					break;				
				}
			} else {
				i = symbolLength; 
				switch (pattern) {
				case 0: // $nnn
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				// case 1: // nnn$
				//	break;
				case 2: // $ nnn
					buffy[--position] = ' '; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				// case 3: // nnn $
				//	break;
				}
			}
			
			return new string (buffy, position, (size - position));
		}

		private static string FormatCurrency (long value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;
			bool negative = (value < 0);

			char[] groupSeparator = nfi.CurrencyGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.CurrencyDecimalSeparator.ToCharArray ();
			char[] currencySymbol = nfi.CurrencySymbol.ToCharArray ();
			int[] groupSizes = nfi.CurrencyGroupSizes;
			int pattern = negative ? nfi.CurrencyNegativePattern : nfi.CurrencyPositivePattern;
			int symbolLength = currencySymbol.Length;
			
			int padding = (precision >= 0) ? precision : nfi.CurrencyDecimalDigits;	     
			int size = maxLongLength + (groupSeparator.Length * maxLongLength) + padding + 2 + 
			decimalSeparator.Length + symbolLength;	
			char[] buffy = new char[size];
			int position = size;

			// set up the pattern from IFormattible
			if (negative) {
				i = symbolLength; 
 
				switch (pattern) {
				case 0: // ($nnn)
					buffy[--position] = ')'; 
					break;
				// case 1: // -$nnn
				//	break;
				// case 2: // $-nnn
				//	break;
				case 3: // $nnn-
					buffy[--position] = '-';
					break;
				case 4:	// (nnn$)
					buffy[--position] = ')'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 5:	// -nnn$
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 6:	// nnn-$
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = '-'; 
					break;
				case 7: // nnn$-
					buffy[--position] = '-'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 8: // -nnn $
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					break;
				// case 9: // -$ nnn
				//	break;
				case 10: // nnn $-
					buffy[--position] = '-'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					break;
				case 11: // $ nnn-
					buffy[--position] = '-'; 
					break;
				// case 12: // $ -nnn
				//	break;
				case 13: // nnn- $
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					buffy[--position] = '-'; 
					break;
				case 14: // ($ nnn)
					buffy[--position] = ')'; 
					break;
				case 15: // (nnn $)
					buffy[--position] = ')'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;				
				}
			} else {
				i = symbolLength; 
				switch (pattern) {
				// case 0: // $nnn
				//	break;
				case 1: // nnn$
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				// case 2: // $ nnn
				//	break;
				case 3: // nnn $
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					break;
				}
			}
			
			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';

			// put on decimal separator if we moved over and put a 0 
			if (position < size && buffy[position] == '0') {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];
		       
			if (negative) {
				if (value <= -10) {
					buffy[--position] = digitLowerTable[-(value % 10)];
					value = value / -10;
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				} else value = -value;
			}

			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			// end the pattern on the left hand side
			if (negative) {
				i = symbolLength; 
 
				switch (pattern) {
				case 0: // ($nnn)
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = '('; 
					break;
				case 1: // -$nnn
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = '-'; 
					break;
				case 2: // $-nnn
					buffy[--position] = '-'; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 3: // $nnn-
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 4:	// (nnn$)
					buffy[--position] = '('; 
					break;
				case 5:	// -nnn$
					buffy[--position] = '-'; 
					break;
				// case 6: // nnn-$
				//	break;
				// case 7: // nnn$-
				//	break;
				case 8: // -nnn $
					buffy[--position] = '-'; 
					break;
				// case 9: // -$ nnn
				//	break;
				// case 10: // nnn $-
				//	break;
				case 11: // $ nnn-
					buffy[--position] = ' '; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				case 12: // $ -nnn
					buffy[--position] = '-'; 
					buffy[--position] = ' '; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				// case 13: // nnn- $
				//	break;
				case 14: // ($ nnn)
					buffy[--position] = ' '; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					buffy[--position] = '('; 
					break;
				case 15: // (nnn $)
					buffy[--position] = '('; 
					break;				
				}
			} else {
				i = symbolLength; 
				switch (pattern) {
				case 0: // $nnn
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				// case 1: // nnn$
				//	break;
				case 2: // $ nnn
					buffy[--position] = ' '; 
					do {
						buffy[--position] = currencySymbol[--i];
					} while (i > 0);
					break;
				// case 3: // nnn $
				//	break;
				}
			}
			
			return new string (buffy, position, (size - position));
		}

		private static string FormatCurrency (sbyte value, int precision, NumberFormatInfo nfi) 
		{
			return FormatCurrency ((int)value, precision, nfi);
		}

		private static string FormatCurrency (ushort value, int precision, NumberFormatInfo nfi) 
		{
			return FormatCurrency ((uint)value, precision, nfi);			
		}

		private static string FormatCurrency (uint value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;

			char[] groupSeparator = nfi.CurrencyGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.CurrencyDecimalSeparator.ToCharArray ();
			char[] currencySymbol = nfi.CurrencySymbol.ToCharArray ();
			int[] groupSizes = nfi.CurrencyGroupSizes;
			int pattern = nfi.CurrencyPositivePattern;
			int symbolLength = currencySymbol.Length;
			
			int padding = (precision >= 0) ? precision : nfi.CurrencyDecimalDigits;	     
			int size = maxIntLength + (groupSeparator.Length * maxIntLength) + padding + 2 + 
			decimalSeparator.Length + symbolLength;	
			char[] buffy = new char[size];
			int position = size;

			// set up the pattern from IFormattible, no negative
			i = symbolLength; 
			switch (pattern) {
			// case 0: // $nnn
			//	break;
			case 1: // nnn$
				do {
					buffy[--position] = currencySymbol[--i];
				} while (i > 0);
				break;
			// case 2: // $ nnn
			//	break;
			case 3: // nnn $
				do {
					buffy[--position] = currencySymbol[--i];
				} while (i > 0);
				buffy[--position] = ' '; 
				break;
			}
			
			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';

			// put on decimal separator if we moved over and put a 0 
			if (position < size && buffy[position] == '0') {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];
		       
			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			// end the pattern on the left hand side
			i = symbolLength; 
			switch (pattern) {
			case 0: // $nnn
				do {
					buffy[--position] = currencySymbol[--i];
				} while (i > 0);
				break;
			// case 1: // nnn$
			//	break;
			case 2: // $ nnn
				buffy[--position] = ' '; 
				do {
					buffy[--position] = currencySymbol[--i];
				} while (i > 0);
				break;
			// case 3: // nnn $
				//	break;
			}
			
			return new string (buffy, position, (size - position));
		}

		private static string FormatCurrency (ulong value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;

			char[] groupSeparator = nfi.CurrencyGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.CurrencyDecimalSeparator.ToCharArray ();
			char[] currencySymbol = nfi.CurrencySymbol.ToCharArray ();
			int[] groupSizes = nfi.CurrencyGroupSizes;
			int pattern = nfi.CurrencyPositivePattern;
			int symbolLength = currencySymbol.Length;
			
			int padding = (precision >= 0) ? precision : nfi.CurrencyDecimalDigits;	     
			int size = maxLongLength + (groupSeparator.Length * maxLongLength) + padding + 2 + 
			decimalSeparator.Length + symbolLength;	
			char[] buffy = new char[size];
			int position = size;

			// set up the pattern from IFormattible, no negative
			i = symbolLength; 
			switch (pattern) {
			// case 0: // $nnn
			//	break;
			case 1: // nnn$
				do {
					buffy[--position] = currencySymbol[--i];
				} while (i > 0);
				break;
			// case 2: // $ nnn
			//	break;
			case 3: // nnn $
				do {
					buffy[--position] = currencySymbol[--i];
				} while (i > 0);
				buffy[--position] = ' '; 
				break;
			}
			
			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';

			// put on decimal separator if we moved over and put a 0 
			if (position < size && buffy[position] == '0') {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];
		       
			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			// end the pattern on the left hand side
			i = symbolLength; 
			switch (pattern) {
			case 0: // $nnn
				do {
					buffy[--position] = currencySymbol[--i];
				} while (i > 0);
				break;
			// case 1: // nnn$
			//	break;
			case 2: // $ nnn
				buffy[--position] = ' '; 
				do {
					buffy[--position] = currencySymbol[--i];
				} while (i > 0);
				break;
			// case 3: // nnn $
				//	break;
			}
			
			return new string (buffy, position, (size - position));
		}
		
		// ============ Format Decimal Types ============ //

		//
		// Used only for integral data types. Negative values are 
		// represented by using a '-' sign. The precision specifies
		// how many digits are to appear in the string. If it is >
		// how many digits we need, the left side is padded w/ 0's.
		// If it is smaller than what we need, it is discarded.
		//
		// Fairly simple implementation. Fill the buffer from right
		// to left w/ numbers, then if we still have precision left
		// over, pad w/ zeros.
		//

		private static string FormatDecimal (byte value, int precision) 
		{
			return FormatDecimal ((uint)value, precision);
		}

		private static string FormatDecimal (short value, int precision) 
		{
			return FormatDecimal ((int)value, precision);
		}
	
		private static string FormatDecimal (int value, int precision)
		{
			int size = (precision > 0) ? (maxIntLength + precision) : maxIntLength;
			char[] buffy = new char[size];
			int position = size;
			bool negative = (value < 0);
			
			if (negative) 
				if (value <= -10) {
					buffy[--position] = digitLowerTable[-(value % 10)];
					value = value / -10;
				} else value = -value;
			
			// get our value into a buffer from right to left
			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;
			}
				
			buffy[--position] = digitLowerTable[value];

			// if we have precision left over, fill with 0's
			precision -= (size - position); 
			while (precision-- > 0 && position > 1) 
				buffy[--position] = '0';

			if (negative) 
				buffy[--position] = '-';
			
			return new string (buffy, position, (size - position));  
		}

		private static string FormatDecimal (long value, int precision)
		{
			int size = (precision > 0) ? (maxLongLength + precision) : maxLongLength;
			char[] buffy = new char[size];
			int position = size;
			bool negative = (value < 0);

			if (negative) 
				if (value <= -10) {
					buffy[--position] = digitLowerTable[-(value % 10)];
					value = value / -10;
				} else value = -value;

			// get our value into a buffer from right to left
			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;
			}
				
			buffy[--position] = digitLowerTable[value];
			
			// if we have precision left over, fill with 0's
			precision -= (size - position); 
			while (precision-- > 0 && position > 1)
				buffy[--position] = '0';

			if (negative) 
				buffy[--position] = '-';
			
			return new string (buffy, position, (size - position));  
		}

		private static string FormatDecimal (sbyte value, int precision) 
		{
			return FormatDecimal ((int)value, precision);
		}

		private static string FormatDecimal (ushort value, int precision) 
		{
			return FormatDecimal ((uint)value, precision);
		}

		private static string FormatDecimal (uint value, int precision)
		{
			int size = (precision > 0) ? (maxIntLength + precision) : maxIntLength;
			char[] buffy = new char[size];
			int position = size;

			// get our value into a buffer from right to left
			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;
			}
				
			buffy[--position] = digitLowerTable[value];
			
			// if we have precision left over, fill with 0's
			precision -= (size - position); 
			while (precision-- > 0 && position > 1) 
				buffy[--position] = '0';

			return new string (buffy, position, (size - position));  
		}

		private static string FormatDecimal (ulong value, int precision)
		{
			int size = (precision > 0) ? (maxLongLength + precision) : maxLongLength;
			char[] buffy = new char[size];
			int position = size;

			// get our value into a buffer from right to left
			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;
			}
				
			buffy[--position] = digitLowerTable[value];
			
			// if we have precision left over, fill with 0's
			precision -= (size - position); 
			while (precision-- > 0 && position > 1)
				buffy[--position] = '0';

			return new string (buffy, position, (size - position));  
		}

		// ============ Format Exponentials ============ //

		//
		// Used for strings in the format [-]M.DDDDDDe+XXX.
		// Exaclty one non-zero digit must appear in M, w/ 
		// a '-' sign if negative. The precision determines 
		// number of decimal places, if not given go 6 places.
		// If precision > the number of places we need, it
		// is right padded w/ 0's. If it is smaller than what
		// we need, we cut off and round. The format specifier
		// decides whether we use an uppercase E or lowercase e.
		// 
		// Tried to do this in one pass of one buffer, but it
		// wasn't happening. Get a buffer + 7 extra slots for
		// the -, ., E, +, and XXX. Parse the value into another
		// temp buffer, then build the new string. For the
		// integral data types, there are a couple things that
		// can be hardcoded. Since an int and a long can't be
		// larger than 20 something spaces, the first X w/ 
		// always be 0, and the the exponential value will only
		// be 2 digits long. Also integer types w/ always
		// have a positive exponential.
		//
		
		private static string FormatExponential (byte value, int precision, bool upper) 
		{
			return FormatExponential ((uint)value, precision, upper);
		}

		private static string FormatExponential (short value, int precision, bool upper) 
		{
			return FormatExponential ((int)value, precision, upper);
		}

		private static string FormatExponential (int value, int precision, bool upper)
		{
			bool negative = (value < 0);
			int padding = (precision >= 0) ? precision : 6;
			char[] buffy = new char[(padding + 8)];
			char[] tmp = new char [maxIntLength];
			int exponent = 0, position = maxIntLength;
			int exp = 0, idx = 0;
			ulong pow = 10;

			// ugly, but doing it since abs(Int32.MinValue) > Int.MaxValue
			uint number = (negative) ? (uint)((-(value + 1)) + 1) : (uint)value;

			// need to calculate the number of places to know if we need to round later
			if (negative && value <= -10) {
				value /= -10;
				exp++;
			}

			while (value >= 10) {
				value /= 10;
				exp++;
			}
							
			if (exp > padding) {

				// highest number we should goto before we round
				while (idx++ <= padding)
					pow *= 10;

				// get our value into a buffer
				while (number > pow) {
					tmp[--position] = digitLowerTable[(number % 10)];
					number /= 10;
					exponent++;
				}
			
				number += 5;
			}

			while (number >= 10) {
				tmp[--position] = digitLowerTable[(number% 10)];
				number /= 10;
				exponent++;
			}		       

			tmp[--position] = digitLowerTable[number];
			idx = 0;
			
			// go left to right in filling up new string
			if (negative)
				buffy[idx++] = '-';

			// we know we have at least one in there, followed 
			// by a decimal point
			buffy[idx++] = tmp[position++];
			if (precision != 0)
				buffy[idx++] = '.';

			// copy over the remaining digits until we run out,
			// or we've passed our specified precision
			while (padding > 0 && position < maxIntLength) {
				buffy[idx++] = tmp[position++];
				padding--;
			}
			
			// if we still have more precision to go, add some
			// zeros
			while (padding > 0) {
				buffy[idx++] = '0';
				padding--;
			}
			
			// we know these next 3 spots
			buffy[idx++] = upper ? 'E' : 'e';
			buffy[idx++] = '+';
			buffy[idx++] = '0';
			
			// next two digits depend on our length
			if (exponent >= 10) {
				buffy[idx++] = digitLowerTable[(exponent / 10)];
				buffy[idx] = digitLowerTable[(exponent % 10)];
			} else { 
				buffy[idx++] = '0';
				buffy[idx] = digitLowerTable[exponent];
			}

			return new string(buffy, 0, ++idx); 
		}

		private static string FormatExponential (long value, int precision, bool upper)
		{
			bool negative = (value < 0);
			int padding = (precision >= 0) ? precision : 6;
			char[] buffy = new char[(padding + 8)];
			char[] tmp = new char [maxLongLength];
			int exponent = 0, position = maxLongLength;
			int exp = 0, idx = 0;
			ulong pow = 10;

			// ugly, but doing it since abs(Int32.MinValue) > Int.MaxValue
			ulong number = (negative) ? (ulong)((-(value + 1)) + 1) : (ulong)value;

			// need to calculate the number of places to know if we need to round later
			if (negative && value <= -10) {
				value /= -10;
				exp++;
			}

			while (value >= 10) {
				value /= 10;
				exp++;
			}
							
			if (exp > padding) {
				
				// highest number we should goto before we round
				while (idx++ <= padding)
					pow *= 10;
				
				// get our value into a buffer
				while (number > pow) {
					tmp[--position] = digitLowerTable[(number % 10)];
					number /= 10;
					exponent++;
				}
			
				number += 5;
			}

			while (number >= 10) {
				tmp[--position] = digitLowerTable[(number% 10)];
				number /= 10;
				exponent++;
			}		       

			tmp[--position] = digitLowerTable[number];
			idx = 0;

			// go left to right in filling up new string
			if (negative)
				buffy[idx++] = '-';

			// we know we have at least one in there, followed 
			// by a decimal point
			buffy[idx++] = tmp[position++];
			if (precision != 0)
				buffy[idx++] = '.';

			// copy over the remaining digits until we run out,
			// or we've passed our specified precision
			while (padding > 0 && position < maxLongLength) {
				buffy[idx++] = tmp[position++];
				padding--;
			}
			
			// if we still have more precision to go, add some
			// zeros
			while (padding > 0) {
				buffy[idx++] = '0';
				padding--;
			}
			
			// we know these next 3 spots
			buffy[idx++] = upper ? 'E' : 'e';
			buffy[idx++] = '+';
			buffy[idx++] = '0';
			
			// next two digits depend on our length
			if (exponent >= 10) {
				buffy[idx++] = digitLowerTable[(exponent / 10)];
				buffy[idx] = digitLowerTable[(exponent % 10)];
			} else { 
				buffy[idx++] = '0';
				buffy[idx] = digitLowerTable[exponent];
			}

			return new string(buffy, 0, ++idx); 
		}

		private static string FormatExponential (sbyte value, int precision, bool upper) 
		{
			return FormatExponential ((int)value, precision, upper);
		}

		private static string FormatExponential (ushort value, int precision, bool upper) 
		{
			return FormatExponential ((uint)value, precision, upper);
		}

		private static string FormatExponential (uint value, int precision, bool upper)
		{
			int padding = (precision >= 0) ? precision : 6;
			char[] buffy = new char[(padding + 8)];
			char[] tmp = new char [maxIntLength];
			int exponent = 0, position = maxIntLength;
			int exp = 0, idx = 0;
			ulong pow = 10;
			ulong number = (ulong)value;

			// need to calculate the number of places to know if we need to round later
			while (value >= 10) {
				value /= 10;
				exp++;
			}
							
			if (exp > padding) {

				// highest number we should goto before we round
				while (idx++ <= padding)
					pow *= 10;
				
				// get our value into a buffer
				while (number > pow) {
					tmp[--position] = digitLowerTable[(number % 10)];
					number /= 10;
					exponent++;
				}
				
				number += 5;
			}

			while (number >= 10) {
				tmp[--position] = digitLowerTable[(number% 10)];
				number /= 10;
				exponent++;
			}		       

			tmp[--position] = digitLowerTable[number];
			idx = 0;

			// we know we have at least one in there, followed 
			// by a decimal point
			buffy[idx++] = tmp[position++];
			if (precision != 0)
				buffy[idx++] = '.';

			// copy over the remaining digits until we run out,
			// or we've passed our specified precision
			while (padding > 0 && position < maxIntLength) {
				buffy[idx++] = tmp[position++];
				padding--;
			}
			
			// if we still have more precision to go, add some
			// zeros
			while (padding > 0) {
				buffy[idx++] = '0';
				padding--;
			}
			
			// we know these next 3 spots
			buffy[idx++] = upper ? 'E' : 'e';
			buffy[idx++] = '+';
			buffy[idx++] = '0';
			
			// next two digits depend on our length
			if (exponent >= 10) {
				buffy[idx++] = digitLowerTable[(exponent / 10)];
				buffy[idx] = digitLowerTable[(exponent % 10)];
			} else { 
				buffy[idx++] = '0';
				buffy[idx] = digitLowerTable[exponent];
			}

			return new string(buffy, 0, ++idx); 
		}

		private static string FormatExponential (ulong value, int precision, bool upper)
		{
			int padding = (precision >= 0) ? precision : 6;
			char[] buffy = new char[(padding + 8)];
			char[] tmp = new char [maxLongLength];
			int exponent = 0, position = maxLongLength;
			int exp = 0, idx = 0;
			ulong pow = 10;
			ulong number = value;

			// need to calculate the number of places to know if we need to round later
			while (value >= 10) {
				value /= 10;
				exp++;
			}
							
			if (exp > padding) {

				// highest number we should goto before we round
				while (idx++ <= padding)
					pow *= 10;

				// get our value into a buffer
				while (number > pow) {
					tmp[--position] = digitLowerTable[(number % 10)];
					number /= 10;
					exponent++;
				}
			
				number += 5;
			}

			while (number >= 10) {
				tmp[--position] = digitLowerTable[(number% 10)];
				number /= 10;
				exponent++;
			}		       

			tmp[--position] = digitLowerTable[number];
			idx = 0;

			// we know we have at least one in there, followed 
			// by a decimal point
			buffy[idx++] = tmp[position++];
			if (precision != 0)
				buffy[idx++] = '.';

			// copy over the remaining digits until we run out,
			// or we've passed our specified precision
			while (padding > 0 && position < maxLongLength) {
				buffy[idx++] = tmp[position++];
				padding--;
			}
			
			// if we still have more precision to go, add some
			// zeros
			while (padding > 0) {
				buffy[idx++] = '0';
				padding--;
			}
			
			// we know these next 3 spots
			buffy[idx++] = upper ? 'E' : 'e';
			buffy[idx++] = '+';
			buffy[idx++] = '0';
			
			// next two digits depend on our length
			if (exponent >= 10) {
				buffy[idx++] = digitLowerTable[(exponent / 10)];
				buffy[idx] = digitLowerTable[(exponent % 10)];
			} else { 
				buffy[idx++] = '0';
				buffy[idx] = digitLowerTable[exponent];
			}

			return new string(buffy, 0, ++idx); 
		}

		// ============ Format Fixed Points ============ //

		//
		// Used for strings in the following form "[-]M.DD...D"
		// At least one non-zero digit precedes the '.', w/ a 
		// '-' before that if negative. Precision specifies number
		// of decimal places 'D' to go. If not given, use
		// NumberFormatInfo.NumbeDecimalDigits. Results are rounded
		// if necessary. 
		//
		// Fairly simple implementation for integral types. Going
		// from right to left, fill up precision number of 0's,
		// plop a . down, then go for our number. 
		//

		private static string FormatFixedPoint (byte value, int precision, NumberFormatInfo nfi)
		{
			return FormatFixedPoint ((uint)value, precision, nfi);
		}

		private static string FormatFixedPoint (short value, int precision, NumberFormatInfo nfi)
		{
			return FormatFixedPoint ((int)value, precision, nfi);
		}

		private static string FormatFixedPoint (int value, int precision, NumberFormatInfo nfi)
		{
			int padding = (precision >= 0) ? (precision + maxIntLength) : (nfi.NumberDecimalDigits + maxIntLength);
			char[] buffy = new char[padding];
			int position = padding;
			bool negative = (value < 0);
			
			// fill up w/ precision # of 0's
			while (position > (maxIntLength - 1)) 
				buffy[--position] = '0';

			if (precision != 0)
				buffy[position--] = '.';

			if (negative)
				if (value <= -10) {
					buffy[position--] = digitLowerTable[-(value % 10)];
					value = value / -10;
				} else value = -value;
			
			// fill up w/ the value
			while (value >= 10) {
				buffy[position--] = digitLowerTable[(value % 10)];
				value = value / 10;
			}

			buffy[position] = digitLowerTable[value];

			if (negative) 
				buffy[--position] = '-';
			
			return new string (buffy, position, (padding - position));
		}

		private static string FormatFixedPoint (long value, int precision, NumberFormatInfo nfi)
		{
			int padding = (precision >= 0) ? (precision + maxLongLength) : (nfi.NumberDecimalDigits + maxLongLength);
			char[] buffy = new char[padding];
			int position = padding;
			bool negative = (value < 0);
			
			// fill up w/ precision # of 0's
			while (position > (maxLongLength - 1)) 
				buffy[--position] = '0';

			if (precision != 0)
				buffy[position--] = '.';

			if (negative)
				if (value <= -10) {
					buffy[position--] = digitLowerTable[-(value % 10)];
					value = value / -10;
				} else value = -value;
			
			// fill up w/ the value
			while (value >= 10) {
				buffy[position--] = digitLowerTable[(value % 10)];
				value = value / 10;
			}

			buffy[position] = digitLowerTable[value];

			if (negative) 
				buffy[--position] = '-';
			
			return new string (buffy, position, (padding - position));
		}

		private static string FormatFixedPoint (sbyte value, int precision, NumberFormatInfo nfi)
		{
			return FormatFixedPoint ((int)value, precision, nfi);
		}

		private static string FormatFixedPoint (ushort value, int precision, NumberFormatInfo nfi)
		{
			return FormatFixedPoint ((uint)value, precision, nfi);
		}

		private static string FormatFixedPoint (uint value, int precision, NumberFormatInfo nfi)
		{
			int padding = (precision >= 0) ? (precision + maxIntLength) : (nfi.NumberDecimalDigits + maxIntLength);
			char[] buffy = new char[padding];
			int position = padding;

			// fill up w/ precision # of 0's
			while (position > (maxIntLength - 1)) 
				buffy[--position] = '0';

			if (precision != 0)
				buffy[position--] = '.';

			// fill up w/ the value
			while (value >= 10) {
				buffy[position--] = digitLowerTable[(value % 10)];
				value = value / 10;
			}

			buffy[position] = digitLowerTable[value];
			
			return new string (buffy, position, (padding - position));
		}

		private static string FormatFixedPoint (ulong value, int precision, NumberFormatInfo nfi)
		{
			int padding = (precision >= 0) ? (precision + maxLongLength) : (nfi.NumberDecimalDigits + maxLongLength);
			char[] buffy = new char[padding];
			int position = padding;

			// fill up w/ precision # of 0's
			while (position > (maxLongLength - 1)) 
				buffy[--position] = '0';

			if (precision != 0)
				buffy[position--] = '.';

			// fill up w/ the value
			while (value >= 10) {
				buffy[position--] = digitLowerTable[(value % 10)];
				value = value / 10;
			}

			buffy[position] = digitLowerTable[value];
			
			return new string (buffy, position, (padding - position));
		}

		// ============ Format General ============ //
		
		//
		// Strings are formatted in either Fixed Point or Exponential
		// format. Results are rounded when needed. If no precision is
		// given, the defaults are:
		//
		// short & ushort: 5
		// int & uint: 10
		// long & ulong: 19
		// float: 7
		// double: 15
		// decimal: 29
		//
		// The value is formatted using fixed-point if exponent >= -4
		// and exponent < precision, where exponent is he exponenent of
		// the value in exponential format. The decimal point and trailing
		// zeros are removed when possible.
		//
		// For all other values, exponential format is used. The case of
		// the format specifier determines whether 'e' or 'E' prefixes
		// the exponent.
		// 
		// In either case, the number of digits that appear in the result
		// (not including the exponent) will not exceed the value of the
		// precision. The result is rounded as needed.
		//
		// Integral values are formatted using Fixed Point whenever
		// precision is omitted. (This actually doesn't make sense when
		// coupled w/ the 1st paragraph).
		//		
		// Okay, so the decimal point is removed along with any trailing
		// zeros. So, ignoring the last paragraph, we can consider an int
		// ToString() to format it w/ exponential format w/ a default
		// precision of 10, but since it will just be .00000000, it's
		// discarded.
		//

		private static string FormatGeneral (byte value, int precision, NumberFormatInfo nfi, bool upper) {
			return FormatGeneral ((uint)value, precision, nfi, upper);
		}

		private static string FormatGeneral (short value, int precision, NumberFormatInfo nfi, bool upper) {
			return FormatGeneral ((int)value, precision, nfi, upper);
		}

		private static string FormatGeneral (int value, int precision, NumberFormatInfo nfi, bool upper) 
		{
			bool negative = (value < 0);
			char[] tmp = new char [maxIntLength];
			int exponent = 0;
			int position = maxIntLength;
			
			// ugly, but doing it since abs(Int32.MinValue) > Int.MaxValue
			uint number = (negative) ? (uint)((-(value + 1)) + 1) : (uint)value;

			// get number into a buffer, going to be doing this no matter what
			if (negative)
				if (value <= -10) {
					tmp[--position] = digitLowerTable[-(value % 10)];
					value /= -10;
					exponent++;
				} else value = -value;

			while (value >= 10) {
				tmp[--position] = digitLowerTable[(value % 10)];
				value /= 10;
				exponent++;
			}
			
			tmp[--position] = digitLowerTable[value];

			// integral values are formatted using fixed point when precision
			// is not specified. But also trailing decimal point and zeros are
			// discared. So for int's it will always be .00, so just compute
			// here and save the call to FormatFixedPoint & trim.
			if (precision <= 0 || exponent < precision) {
				if (negative) 
					tmp[--position] = '-';
				
				return new string (tmp, position, (maxIntLength - position)); 
			}

			// else our exponent was > precision, use exponential format
			// precision = number of digits to show. 
			int idx = 0;
			ulong pow = 1;

			exponent = 0;
			position = maxIntLength;
			
			// Loop through while our number is less than the 10 ^ precision, then
			// add 5 to that to round it out, and keep continuing
			while (idx++ <= precision)
				pow *= 10;
			
			while (number > pow) {
				tmp[--position] = digitLowerTable[(number % 10)];
				number /= 10;
				exponent++;
			}

			number += 5;

			while (number >= 10) {
				tmp[--position] = digitLowerTable[(number % 10)];
				number /= 10;
				exponent++;
			}

			tmp[--position] = digitLowerTable[number];

			// finally, make our final buffer, at least precision + 6 for 'E+XX' and '-'
			// and reuse pow for size
			idx = position;
			position = 0;
			pow = (ulong)(precision + 6);
			char[] buffy = new char[pow];

			if (negative)
				buffy[position++] = '-';
			
			buffy[position++] = tmp[idx++];
			buffy[position] = '.';

			// for the remaining precisions copy over rounded tmp
			precision--;
			while (precision-- > 0)
				buffy[++position] = tmp[idx++];

			// get rid of ending zeros
			while (buffy[position] == '0')
				position--;

			// if we backed up all the way to the ., over write it
			if (buffy[position] != '.')
				position++;			

			// ints can only be +, e or E depending on format, plus XX
			buffy[position++] = upper ? 'E' : 'e';
			buffy[position++] = '+';

			if (exponent >= 10) {
				buffy[position++] = digitLowerTable[(exponent / 10)];
				buffy[position++] = digitLowerTable[(exponent % 10)];
			} else { 
				buffy[position++] = '0';
				buffy[position++] = digitLowerTable[exponent];
			}
			
			return new string (buffy, 0, position);
		}

		private static string FormatGeneral (long value, int precision, NumberFormatInfo nfi, bool upper) 
		{
			bool negative = (value < 0);
			char[] tmp = new char [maxLongLength];
			int exponent = 0;
			int position = maxLongLength;

			// ugly, but doing it since abs(Int32.MinValue) > Int.MaxValue
			ulong number = (negative) ? (ulong)(-(value + 1) + 1) : (ulong)value;

			// get number into a buffer, going to be doing this no matter what
			if (negative)
				if (value <= -10) {
					tmp[--position] = digitLowerTable[-(value % 10)];
					value /= -10;
				} else value = -value;

			while (value >= 10) {
				tmp[--position] = digitLowerTable[(value % 10)];
				value /= 10;
			}
			
			tmp[--position] = digitLowerTable[value];
			exponent = (maxLongLength - position) - 1;

			// integral values are formatted using fixed point when precision
			// is not specified. But also trailing decimal point and zeros are
			// discared. So for int's it will always be .00, so just compute
			// here and save the call to FormatFixedPoint & trim.
			if (precision <= 0 || exponent < precision) {
				if (negative) 
					tmp[--position] = '-';
				
				return new string (tmp, position, (maxLongLength - position)); 
			}

			// else our exponent was > precision, use exponential format
			// precision = number of digits to show. 
			int idx = 0;
			ulong pow = 1;

			exponent = 0;
			position = maxLongLength;

			// Loop through while our number is less than the 10 ^ precision, then
			// add 5 to that to round it out, and keep continuing
			while (idx++ <= precision)
				pow *= 10;
			
			while (number > pow) {
				tmp[--position] = digitLowerTable[(number % 10)];
				number /= 10;
				exponent++;
			}

			number += 5;

			while (number >= 10) {
				tmp[--position] = digitLowerTable[(number % 10)];
				number /= 10;
				exponent++;
			}

			tmp[--position] = digitLowerTable[number];

			// finally, make our final buffer, at least precision + 6 for 'E+XX' and '-'
			// and reuse pow for size
			idx = position;
			position = 0;
			pow = (ulong)precision + 6;
			char[] buffy = new char[pow];

			if (negative)
				buffy[position++] = '-';
			
			buffy[position++] = tmp[idx++];
			buffy[position] = '.';

			// for the remaining precisions copy over rounded tmp
			precision--;
			while (precision-- > 0)
				buffy[++position] = tmp[idx++];

			// get rid of ending zeros
			while (buffy[position] == '0')
				position--;

			// if we backed up all the way to the ., over write it
			if (buffy[position] != '.')
				position++;			

			// ints can only be +, e or E depending on format, plus XX
			buffy[position++] = upper ? 'E' : 'e';
			buffy[position++] = '+';

			if (exponent >= 10) {
				buffy[position++] = digitLowerTable[(exponent / 10)];
				buffy[position++] = digitLowerTable[(exponent % 10)];
			} else { 
				buffy[position++] = '0';
				buffy[position++] = digitLowerTable[exponent];
			}
			
			return new string (buffy, 0, position);
		}

		private static string FormatGeneral (sbyte value, int precision, NumberFormatInfo nfi, bool upper) {
			return FormatGeneral ((int)value, precision, nfi, upper);
		}

		private static string FormatGeneral (ushort value, int precision, NumberFormatInfo nfi, bool upper) {
			return FormatGeneral ((uint)value, precision, nfi, upper);
		}

		private static string FormatGeneral (uint value, int precision, NumberFormatInfo nfi, bool upper) 
		{
			char[] tmp = new char [maxIntLength];
			int exponent = 0;
			int position = maxIntLength;
			ulong number = (ulong)value;

			// get number into a buffer, going to be doing this no matter what
			while (value >= 10) {
				tmp[--position] = digitLowerTable[(value % 10)];
				value /= 10;
			}
			
			tmp[--position] = digitLowerTable[value];
			exponent = (maxIntLength - position) - 1;

			// integral values are formatted using fixed point when precision
			// is not specified. But also trailing decimal point and zeros are
			// discared. So for int's it will always be .00, so just compute
			// here and save the call to FormatFixedPoint & trim.
			if (precision <= 0 || exponent < precision) 
				return new string (tmp, position, (maxIntLength - position)); 

			// else our exponent was > precision, use exponential format
			// precision = number of digits to show. 
			int idx = 0;
			ulong pow = 1;

			exponent = 0;
			position = maxIntLength;
						
			// Loop through while our number is less than the 10 ^ precision, then
			// add 5 to that to round it out, and keep continuing
			while (idx++ <= precision)
				pow *= 10;

			while (number > pow) {
				tmp[--position] = digitLowerTable[(number % 10)];
				number /= 10;
				exponent++;
			}

			number += 5;

			while (number >= 10) {
				tmp[--position] = digitLowerTable[(number % 10)];
				number /= 10;
				exponent++;
			}

			tmp[--position] = digitLowerTable[number]; 	

			// finally, make our final buffer, at least precision + 6 for 'E+XX' and '-'
			// and reuse pow for size
			idx = position;
			position = 0;
			pow = (ulong)(precision + 6);
			char[] buffy = new char[pow];

			buffy[position++] = tmp[idx++];
			buffy[position] = '.';

			// for the remaining precisions copy over rounded tmp
			precision--;
			while (precision-- > 0)
				buffy[++position] = tmp[idx++];

			// get rid of ending zeros
			while (buffy[position] == '0')
				position--;

			// if we backed up all the way to the ., over write it
			if (buffy[position] != '.')
				position++;			

			// ints can only be +, e or E depending on format, plus XX
			buffy[position++] = upper ? 'E' : 'e';
			buffy[position++] = '+';

			if (exponent >= 10) {
				buffy[position++] = digitLowerTable[(exponent / 10)];
				buffy[position++] = digitLowerTable[(exponent % 10)];
			} else { 
				buffy[position++] = '0';
				buffy[position++] = digitLowerTable[exponent];
			}
			
			return new string (buffy, 0, position);
		}

		private static string FormatGeneral (ulong value, int precision, NumberFormatInfo nfi, bool upper) 
		{
			char[] tmp = new char [maxLongLength];
			int exponent = 0;
			int position = maxLongLength;
			ulong number = value;

			// get number into a buffer, going to be doing this no matter what
			while (value >= 10) {
				tmp[--position] = digitLowerTable[(value % 10)];
				value /= 10;
				exponent++;
			}
			
			tmp[--position] = digitLowerTable[value];

			// integral values are formatted using fixed point when precision
			// is not specified. But also trailing decimal point and zeros are
			// discared. So for int's it will always be .00, so just compute
			// here and save the call to FormatFixedPoint & trim.
			if (precision <= 0 || exponent < precision) 
				return new string (tmp, position, (maxLongLength - position)); 

			// else our exponent was > precision, use exponential format
			// precision = number of digits to show. 
			int idx = 0;
			ulong pow = 1;

			exponent = 0;
			position = maxLongLength;

			// Loop through while our number is less than the 10 ^ precision, then
			// add 5 to that to round it out, and keep continuing
			while (idx++ <= precision)
				pow *= 10;
			
			while (number > pow) {
				tmp[--position] = digitLowerTable[(number % 10)];
				number /= 10;
				exponent++;
			}

			number += 5;

			while (number >= 10) {
				tmp[--position] = digitLowerTable[(number % 10)];
				number /= 10;
				exponent++;
			}

			tmp[--position] = digitLowerTable[number];

			// finally, make our final buffer, at least precision + 6 for 'E+XX' and '-'
			// and reuse pow for size
			idx = position;
			position = 0;
			pow = (ulong)precision + 6;
			char[] buffy = new char[pow];

			buffy[position++] = tmp[idx++];
			buffy[position] = '.';

			// for the remaining precisions copy over rounded tmp
			precision--;
			while (precision-- > 0)
				buffy[++position] = tmp[idx++];

			// get rid of ending zeros
			while (buffy[position] == '0')
				position--;

			// if we backed up all the way to the ., over write it
			if (buffy[position] != '.')
				position++;			

			// ints can only be +, e or E depending on format, plus XX
			buffy[position++] = upper ? 'E' : 'e';
			buffy[position++] = '+';

			if (exponent >= 10) {
				buffy[position++] = digitLowerTable[(exponent / 10)];
				buffy[position++] = digitLowerTable[(exponent % 10)];
			} else { 
				buffy[position++] = '0';
				buffy[position++] = digitLowerTable[exponent];
			}
			
			return new string (buffy, 0, position);
		}

		// ============ Format Number ============ //

		// 
		// Used for strings in the following form "[-]d,ddd,ddd.dd...d"
		// The minus sign only appears if it is negative. At least one
		// non-zero digit preceeds the decimal separator. The precision
		// specifier determines the number of decimal places. If it is 
		// not given, use NumberFormatInfo.NumberDecimalDigits.
		// The NumberGroupSizes, NumberGroupSeparator, and NumberDecimalSeparator
		// members of NumberFormatInfo supply the size and separator
		// for digit groupings. See IFormattable.
		//
		// The group sizes is an array of ints that determine the grouping
		// of numbers. All digits are in the range 1-9, with the last digit
		// being between 0-9. The number formats the string backwards, with
		// the last digit being the group size for the rest of (leftmost) the
		// the string, 0 being none.
		//
		// For instance:
		//		groupSizes = { 3, 2, 1, 0 }; 
		//		int n = 1234567890 => "1234,5,67,890"
		//		groupSizes = { 3, 2, 1 }; 
		//		int n = 1234567890 => "1,2,3,4,5,67,890"
		//		groupSizes = { 2, 0 };
		//		int n = 1234567890 => "1234567,90";
		//
		// Not too difficult, jsut keep track of where you are in the array
		// and when to print the separator
		//
		// The max size of the buffer is assume we have a separator every 
		// number, plus the precision on the end, plus a spot for the negative
		// and a spot for decimal separator.
		//

		private static string FormatNumber (byte value, int precision, NumberFormatInfo nfi) {
			return FormatNumber ((uint)value, precision, nfi);
		}

		private static string FormatNumber (short value, int precision, NumberFormatInfo nfi) {
			return FormatNumber ((int)value, precision, nfi);
		}

		private static string FormatNumber (int value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;
			char[] groupSeparator = nfi.NumberGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.NumberDecimalSeparator.ToCharArray ();
			int[] groupSizes = nfi.NumberGroupSizes;

			int padding = (precision >= 0) ? precision : nfi.NumberDecimalDigits;
			int pattern = nfi.NumberNegativePattern;
			int size = maxIntLength + (maxIntLength * groupSeparator.Length) + padding +
			decimalSeparator.Length + 4;
			char[] buffy = new char[size];
			int position = size;
			bool negative = (value < 0);
			
			// pattern for negative values, defined in NumberFormatInfo
			if (negative) {
				switch (pattern) {
				case 0: // (nnn)
					buffy[--position] = ')'; 
					break;
				// case 1: // -nnn
				//	break;
				// case 2: // - nnn
				//	break;
				case 3: // nnn-
					buffy[--position] = '-'; 
					break;
				case 4:	// nnn -
					buffy[--position] = '-'; 
					buffy[--position] = ' '; 
					break;
				}
			}

			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';

			// put on decimal separator
			if (position != size) {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];

			// negative hack for numbers past MinValue
			if (negative)
				if (value <= -10) {
					buffy[--position] = digitLowerTable[-(value % 10)];
					value = value / -10;
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				} else value = -value;

			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			// pattern for negative values, defined in NumberFormatInfo
			if (negative) {
				switch (pattern) {
				case 0: // (nnn)
					buffy[--position] = '('; 
					break;
				case 1: // -nnn
					buffy[--position] = '-'; 
					break;
				case 2: // - nnn
					buffy[--position] = ' '; 
					buffy[--position] = '-'; 
					break;
				// case 3: // nnn-
				//	break;
				// case 4: // nnn -
				//	break;
				}
			}
			
			return new string (buffy, position, (size - position));
		}

		private static string FormatNumber (long value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;
			char[] groupSeparator = nfi.NumberGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.NumberDecimalSeparator.ToCharArray ();
			int[] groupSizes = nfi.NumberGroupSizes;

			int padding = (precision >= 0) ? precision : nfi.NumberDecimalDigits;
			int pattern = nfi.NumberNegativePattern;
			int size = maxLongLength + (maxLongLength * groupSeparator.Length) + padding +
			decimalSeparator.Length + 4;
			char[] buffy = new char[size];
			int position = size;
			bool negative = (value < 0);
			
			// pattern for negative values, defined in NumberFormatInfo
			if (negative) {
				switch (pattern) {
				case 0: // (nnn)
					buffy[--position] = ')'; 
					break;
				// case 1: // -nnn
				//	break;
				// case 2: // - nnn
				//	break;
				case 3: // nnn-
					buffy[--position] = '-'; 
					break;
				case 4:	// nnn -
					buffy[--position] = '-'; 
					buffy[--position] = ' '; 
					break;
				}
			}

			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';
						
			// put on decimal separator
			if (position != size) {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];

			// negative hack for numbers past MinValue
			if (negative)
				if (value <= -10) {
					buffy[--position] = digitLowerTable[-(value % 10)];
					value = value / -10;
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				} else value = -value;

			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			// pattern for negative values, defined in NumberFormatInfo
			if (negative) {
				switch (pattern) {
				case 0: // (nnn)
					buffy[--position] = '('; 
					break;
				case 1: // -nnn
					buffy[--position] = '-'; 
					break;
				case 2: // - nnn
					buffy[--position] = ' '; 
					buffy[--position] = '-'; 
					break;
				// case 3: // nnn-
				//	break;
				// case 4: // nnn -
				//	break;
				}
			}
			
			return new string (buffy, position, (size - position));
		}

		private static string FormatNumber (sbyte value, int precision, NumberFormatInfo nfi) {
			return FormatNumber ((int)value, precision, nfi);
		}

		private static string FormatNumber (ushort value, int precision, NumberFormatInfo nfi) {
			return FormatNumber ((uint)value, precision, nfi);
		}

		private static string FormatNumber (uint value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;
			char[] groupSeparator = nfi.NumberGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.NumberDecimalSeparator.ToCharArray ();
			int[] groupSizes = nfi.NumberGroupSizes;

			int padding = (precision >= 0) ? precision : nfi.NumberDecimalDigits;
			int size = maxIntLength + (maxIntLength * groupSeparator.Length) + padding +
			decimalSeparator.Length + 2;
			char[] buffy = new char[size];
			int position = size;
			
			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';
						
			// put on decimal separator
			if (position != size) {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];

			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			return new string (buffy, position, (size - position));
		}

		private static string FormatNumber (ulong value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;
			char[] groupSeparator = nfi.NumberGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.NumberDecimalSeparator.ToCharArray ();
			int[] groupSizes = nfi.NumberGroupSizes;

			int padding = (precision >= 0) ? precision : nfi.NumberDecimalDigits;
			int size = maxLongLength + (maxLongLength * groupSeparator.Length) + padding +
			decimalSeparator.Length + 2;
			char[] buffy = new char[size];
			int position = size;
			
			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';
			
			// put on decimal separator
			if (position != size) {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];

			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			return new string (buffy, position, (size - position));
		}		

		// ============ Percent Formatting ============ //

		//
		//  Percent Format: Used for strings containing a percentage. The
		//  PercentSymbol, PercentGroupSizes, PercentGroupSeparator, and
		//  PercentDecimalSeparator members of a NumberFormatInfo supply
		//  the Percent symbol, size and separator for digit groupings, and
		//  decimal separator, respectively.
		//  PercentNegativePattern and PercentPositivePattern determine the
		//  symbols used to represent negative and positive values. For example,
		//  a negative value may be prefixed with a minus sign, or enclosed in
		//  parentheses.
		//  If no precision is specified, the number of decimal places in the result
		//  is set by NumberFormatInfo.PercentDecimalDigits. Results are
		//  rounded to the nearest representable value when necessary.
		//  The result is scaled by 100 (.99 becomes 99%).
		//
		//  The pattern of the number determines how the output looks, where
		//  the percent sign goes, where the negative sign goes, etc.
		//  IFormattable documentation lists the patterns and their values,
		//  I have them commented out in the switch statement
		//

		private static string FormatPercent (byte value, int precision, NumberFormatInfo nfi) 
		{
			return FormatPercent ((uint)value, precision, nfi);
		}

		private static string FormatPercent (short value, int precision, NumberFormatInfo nfi) 
		{
			return FormatPercent ((int)value, precision, nfi);
		}

		private static string FormatPercent (int value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;
			bool negative = (value < 0);

			char[] groupSeparator = nfi.PercentGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.PercentDecimalSeparator.ToCharArray ();
			char[] percentSymbol = nfi.PercentSymbol.ToCharArray ();
			int[] groupSizes = nfi.PercentGroupSizes;
			int pattern = negative ? nfi.PercentNegativePattern : nfi.PercentPositivePattern;
			int symbolLength = percentSymbol.Length;
			
			int padding = (precision >= 0) ? precision : nfi.PercentDecimalDigits;	     
			int size = maxIntLength + (groupSeparator.Length * maxIntLength) + padding + 2 + 
			decimalSeparator.Length + symbolLength;	
			char[] buffy = new char[size];
			int position = size;

			// set up the pattern from IFormattible
			if (negative) {
				i = symbolLength; 
 
				switch (pattern) {
				case 0: // -nnn %
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					break;
				case 1: // -nnn%
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					break;
				// case 2: // -%nnn
				//	break;
				}
			} else {
				i = symbolLength; 
				switch (pattern) {
				case 0: // nnn %
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					buffy[--position] = ' ';					
					break;
				case 1: // nnn%
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					break;
				// case 2: // %nnn
				//	break;
				}
			}
			
			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';

			// put on decimal separator if we moved over and put a 0 
			if (position < size && buffy[position] == '0') {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];

			// all values are multiplied by 100, so tack on two 0's
			if (value != 0) 
				for (int c = 0; c < 2; c++) {
					buffy[--position] = '0';
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				}

			// negative hack for numbers past MinValue
			if (negative)
				if (value <= -10) {
					buffy[--position] = digitLowerTable[-(value % 10)];
					value = value / -10;
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				} else value = -value;

			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			// end the pattern on the left hand side
			if (negative) {
				i = symbolLength; 
 
				switch (pattern) {
				case 0: // -nnn %
					buffy[--position] = '-'; 
					break;
				case 1: // -nnn%
					buffy[--position] = '-'; 
					break;
				case 2: // -%nnn
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					buffy[--position] = '-'; 
					break;
				}
			} else {
				i = symbolLength; 
				switch (pattern) {
				// case 0: // nnn %
				//	break;
				// case 1: // nnn%
				//	break;
				case 2: // %nnn
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					break;
				}
			}
			
			return new string (buffy, position, (size - position));
		}

		private static string FormatPercent (long value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;
			bool negative = (value < 0);

			char[] groupSeparator = nfi.PercentGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.PercentDecimalSeparator.ToCharArray ();
			char[] percentSymbol = nfi.PercentSymbol.ToCharArray ();
			int[] groupSizes = nfi.PercentGroupSizes;
			int pattern = negative ? nfi.PercentNegativePattern : nfi.PercentPositivePattern;
			int symbolLength = percentSymbol.Length;
			
			int padding = (precision >= 0) ? precision : nfi.PercentDecimalDigits;	     
			int size = maxLongLength + (groupSeparator.Length * maxLongLength) + padding + 2 + 
			decimalSeparator.Length + symbolLength;	
			char[] buffy = new char[size];
			int position = size;

			// set up the pattern from IFormattible
			if (negative) {
				i = symbolLength; 
 
				switch (pattern) {
				case 0: // -nnn %
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					buffy[--position] = ' '; 
					break;
				case 1: // -nnn%
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					break;
				// case 2: // -%nnn
				//	break;
				}
			} else {
				i = symbolLength; 
				switch (pattern) {
				case 0: // nnn %
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					buffy[--position] = ' ';					
					break;
				case 1: // nnn%
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					break;
				// case 2: // %nnn
				//	break;
				}
			}
			
			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';

			// put on decimal separator if we moved over and put a 0 
			if (position < size && buffy[position] == '0') {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];

			// all values are multiplied by 100, so tack on two 0's
			if (value != 0) 
				for (int c = 0; c < 2; c++) {
					buffy[--position] = '0';
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				}

			// negative hack for numbers past MinValue
			if (negative)
				if (value <= -10) {
					buffy[--position] = digitLowerTable[-(value % 10)];
					value = value / -10;
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				} else value = -value;

			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			// end the pattern on the left hand side
			if (negative) {
				i = symbolLength; 
 
				switch (pattern) {
				case 0: // -nnn %
					buffy[--position] = '-'; 
					break;
				case 1: // -nnn%
					buffy[--position] = '-'; 
					break;
				case 2: // -%nnn
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					buffy[--position] = '-'; 
					break;
				}
			} else {
				i = symbolLength; 
				switch (pattern) {
				// case 0: // nnn %
				//	break;
				// case 1: // nnn%
				//	break;
				case 2: // %nnn
					do {
						buffy[--position] = percentSymbol[--i];
					} while (i > 0);
					break;
				}
			}
			
			return new string (buffy, position, (size - position));
		}

		private static string FormatPercent (sbyte value, int precision, NumberFormatInfo nfi) 
		{
			return FormatPercent ((int)value, precision, nfi);
		}

		private static string FormatPercent (ushort value, int precision, NumberFormatInfo nfi) 
		{
			return FormatPercent ((uint)value, precision, nfi);
		}

		private static string FormatPercent (uint value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;

			char[] groupSeparator = nfi.PercentGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.PercentDecimalSeparator.ToCharArray ();
			char[] percentSymbol = nfi.PercentSymbol.ToCharArray ();
			int[] groupSizes = nfi.PercentGroupSizes;
			int pattern = nfi.PercentPositivePattern;
			int symbolLength = percentSymbol.Length;
			
			int padding = (precision >= 0) ? precision : nfi.PercentDecimalDigits;	     
			int size = maxIntLength + (groupSeparator.Length * maxIntLength) + padding + 2 + 
			decimalSeparator.Length + symbolLength;	
			char[] buffy = new char[size];
			int position = size;

			// set up the pattern from IFormattible
			i = symbolLength; 			
			switch (pattern) {
			case 0: // -nnn %
				do {
					buffy[--position] = percentSymbol[--i];
				} while (i > 0);
				buffy[--position] = ' '; 
				break;
			case 1: // -nnn%
				do {
					buffy[--position] = percentSymbol[--i];
				} while (i > 0);
				break;
			// case 2: // -%nnn
			//	break;
			}

			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';

			// put on decimal separator if we moved over and put a 0 
			if (position < size && buffy[position] == '0') {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];

			if (value != 0) 
				for (int c = 0; c < 2; c++) {
					buffy[--position] = '0';
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				}

			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			i = symbolLength; 
			switch (pattern) {
			// case 0: // nnn %
			//	break;
			// case 1: // nnn%
			//	break;
			case 2: // %nnn
				do {
					buffy[--position] = percentSymbol[--i];
				} while (i > 0);
				break;
			}
			
			return new string (buffy, position, (size - position));
		}

		private static string FormatPercent (ulong value, int precision, NumberFormatInfo nfi) 
		{
			int i, j, k;

			char[] groupSeparator = nfi.PercentGroupSeparator.ToCharArray ();
			char[] decimalSeparator = nfi.PercentDecimalSeparator.ToCharArray ();
			char[] percentSymbol = nfi.PercentSymbol.ToCharArray ();
			int[] groupSizes = nfi.PercentGroupSizes;
			int pattern = nfi.PercentPositivePattern;
			int symbolLength = percentSymbol.Length;
			
			int padding = (precision >= 0) ? precision : nfi.PercentDecimalDigits;	     
			int size = maxLongLength + (groupSeparator.Length * maxLongLength) + padding + 2 + 
			decimalSeparator.Length + symbolLength;	
			char[] buffy = new char[size];
			int position = size;

			// set up the pattern from IFormattible
			i = symbolLength; 			
			switch (pattern) {
			case 0: // -nnn %
				do {
					buffy[--position] = percentSymbol[--i];
				} while (i > 0);
				buffy[--position] = ' '; 
				break;
			case 1: // -nnn%
				do {
					buffy[--position] = percentSymbol[--i];
				} while (i > 0);
				break;
			// case 2: // -%nnn
			//	break;
			}

			// right pad it w/ precision 0's
			while (padding-- > 0)
				buffy[--position] = '0';

			// put on decimal separator if we moved over and put a 0 
			if (position < size && buffy[position] == '0') {
				i = decimalSeparator.Length; 
				do {
					buffy[--position] = decimalSeparator[--i];
				} while (i > 0);			
			}

			// loop through, keeping track of where you are in the
			// group sizes array and putting out the group separator
			// when needed
			j = 0;
			k = groupSizes[j++];

			if (value != 0) 
				for (int c = 0; c < 2; c++) {
					buffy[--position] = '0';
					
					if (--k == 0) {
						i = groupSeparator.Length; 
						do {
							buffy[--position] = groupSeparator[--i];
						} while (i > 0);
						
						k = (j < groupSizes.Length) ? 
						groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
					}
				}
			
			while (value >= 10) {
				buffy[--position] = digitLowerTable[(value % 10)];
				value /= 10;

				if (--k == 0) {
					i = groupSeparator.Length; 
					do {
						buffy[--position] = groupSeparator[--i];
					} while (i > 0);
					
					k = (j < groupSizes.Length) ? 
					groupSizes[j++] : groupSizes[(groupSizes.Length - 1)];
				}
			}		 

			buffy[--position] = digitLowerTable[value];

			i = symbolLength; 
			switch (pattern) {
			// case 0: // nnn %
			//	break;
			// case 1: // nnn%
			//	break;
			case 2: // %nnn
				do {
					buffy[--position] = percentSymbol[--i];
				} while (i > 0);
				break;
			}
			
			return new string (buffy, position, (size - position));
		}

		// ============ Format Hexadecimal ============ //

		// 
		// For strings in base 16. Only valid w/ integers. Precision 
		// specifies number of digits in the string, if it specifies
		// more digits than we need, left pad w/ 0's. The case of the
		// the format specifier 'X' or 'x' determines lowercase or
		// capital digits in the output.
		//
		// Whew. Straight forward Hex formatting, however only
		// go 8 places max when dealing with an int (not counting
		// precision padding) and 16 when dealing with a long. This
		// is to cut off the loop when dealing with negative values,
		// which will loop forever when you hit -1;
		//

		private static string FormatHexadecimal (byte value, int precision, bool upper)
		{		     
			if (precision < 0) precision = 0;
			int size = maxByteLength + precision;
			char[] buffy = new char[size];
			char[] table = upper ? digitUpperTable : digitLowerTable;
			int position = size;
			ushort mask = (1 << 4) - 1;

			// loop through right to left, shifting and looking up
			// our value. Don't worry about negative
			do {
				buffy[--position] = table[(value & mask)];
				value = (byte)(value >> 4);
			} while (value != 0);

			// pad w/ 0's if they want more length, if not, ignore
			precision -= (size - position);				
			while (precision > 0 && position > 1) {
				buffy[--position] = '0';
				precision--;
			}
			
			return new string(buffy, position, (size - position));
		}

		private static string FormatHexadecimal (short value, int precision, bool upper)
		{
			if (precision < 0) precision = 0;
			int size = maxShortLength + precision;
			char[] buffy = new char[size];
			char[] table = upper ? digitUpperTable : digitLowerTable;
			int position = size;
			short mask = (1 << 4) - 1;

			// loop through right to left, shifting and looking up
			// our value. If value is negavite stop after 4 F's
			do {
				buffy[--position] = table[(value & mask)];
				value = (short)(value >> 4);
			} while (value != 0 && position > (size - 4));

			// pad w/ 0's if they want more length, if not, ignore
			precision -= (size - position);				
			while (precision > 0 && position > 1) {
				buffy[--position] = '0';
				precision--;
			}
			
			return new string(buffy, position, (size - position));
		}

		private static string FormatHexadecimal (int value, int precision, bool upper)
		{
			if (precision < 0) precision = 0;
			int size = maxIntLength + precision;
			char[] buffy = new char[size];
			char[] table = upper ? digitUpperTable : digitLowerTable;
			int position = size;
			int mask = (1 << 4) - 1;

			// loop through right to left, shifting and looking up
			// our value. If value is negavite stop after 8 F's
			do {
				buffy[--position] = table[(value & mask)];
				value = value >> 4;
			} while (value != 0 && position > (size - 8));

			// pad w/ 0's if they want more length, if not, ignore
			precision -= (size - position);				
			while (precision > 0 && position > 1) {
				buffy[--position] = '0';
				precision--;
			}
			
			return new string(buffy, position, (size - position));
		}

		private static string FormatHexadecimal (long value, int precision, bool upper)
		{
			if (precision < 0) precision = 0;
			int size = maxLongLength + precision;
			char[] buffy = new char[size];
			char[] table = upper ? digitUpperTable : digitLowerTable;
			int position = size;
			long mask = (1 << 4) - 1;
			
			// loop through right to left, shifting and looking up
			// our value. If value is negavite stop after 16 F's
			do {
				buffy[--position] = table[(value & mask)];
				value = value >> 4;
			} while (value != 0 && position > (size - 16));

			// pad w/ 0's if they want more length, if not, ignore
			precision -= (size - position);				
			while (precision > 0 && position > 1) {
				buffy[--position] = '0';
				precision--;
			}
			
			return new string(buffy, position, (size - position));
		}

		private static string FormatHexadecimal (sbyte value, int precision, bool upper)
		{
			if (precision < 0) precision = 0;
			int size = maxByteLength + precision;
			char[] buffy = new char[size];
			char[] table = upper ? digitUpperTable : digitLowerTable;
			int position = size;
			short mask = (1 << 4) - 1;

			// loop through right to left, shifting and looking up
			// our value. If value is negavite stop after 2 F's
			do {
				buffy[--position] = table[(value & mask)];
				value = (sbyte)(value >> 4);
			} while (value != 0 && position > (size - 2));

			// pad w/ 0's if they want more length, if not, ignore
			precision -= (size - position);				
			while (precision > 0 && position > 1) {
				buffy[--position] = '0';
				precision--;
			}
			
			return new string(buffy, position, (size - position));
		}

		private static string FormatHexadecimal (ushort value, int precision, bool upper)
		{			
			if (precision < 0) precision = 0;
			int size = maxShortLength + precision;
			char[] buffy = new char[size];
			char[] table = upper ? digitUpperTable : digitLowerTable;
			int position = size;
			int mask = (1 << 4) - 1;

			// loop through right to left, shifting and looking up
			// our value. Don't worry about negative
			do {
				buffy[--position] = table[(value & mask)];
				value = (ushort)(value >> 4);
			} while (value != 0);

			// pad w/ 0's if they want more length, if not, ignore
			precision -= (size - position);				
			while (precision > 0 && position > 1) {
				buffy[--position] = '0';
				precision--;
			}
			
			return new string(buffy, position, (size - position));
		}

		private static string FormatHexadecimal (uint value, int precision, bool upper)
		{			
			if (precision < 0) precision = 0;
			int size = maxIntLength + precision;
			char[] buffy = new char[size];
			char[] table = upper ? digitUpperTable : digitLowerTable;
			int position = size;
			uint mask = (1 << 4) - 1;

			// loop through right to left, shifting and looking up
			// our value. Don't worry about negative
			do {
				buffy[--position] = table[(value & mask)];
				value = value >> 4;
			} while (value != 0);

			// pad w/ 0's if they want more length, if not, ignore
			precision -= (size - position);				
			while (precision > 0 && position > 1) {
				buffy[--position] = '0';
				precision--;
			}
			
			return new string(buffy, position, (size - position));
		}

		private static string FormatHexadecimal (ulong value, int precision, bool upper)
		{			
			if (precision < 0) precision = 0;
			int size = maxLongLength + precision;
			char[] buffy = new char[size];
			char[] table = upper ? digitUpperTable : digitLowerTable;
			int position = size;
			ulong mask = (1 << 4) - 1;
			
			// loop through right to left, shifting and looking up
			// our value. Don't worry about negative
			do {
				buffy[--position] = table[value & mask];
				value = value >> 4;
			} while (value != 0);
			
			// pad w/ 0's if they want more length, if not, ignore
			precision -= (size - position);
			while (precision > 0 && position > 1) {
				buffy[--position] = '0';
				precision--;
			}

			return new string(buffy, position, (size - position));
		}

		// ============ Format Custom ============ //

		private static string FormatCustom (string format, sbyte number, NumberFormatInfo nfi)
		{
			string strnum = FormatGeneral (number, -1, nfi, true);
			FormatParse fp = new FormatParse (format); // FIXME: use nfi!
			int sign = (number < 0) ? -1 : (number > 0) ? 1 : 0;
			return fp.FormatNumber (strnum, sign);
		}

		private static string FormatCustom (string format, short number, NumberFormatInfo nfi)
		{
			string strnum = FormatGeneral (number, -1, nfi, true);
			FormatParse fp = new FormatParse (format); // FIXME: use nfi!
			int sign = (number < 0) ? -1 : (number > 0) ? 1 : 0;
			return fp.FormatNumber (strnum, sign);
		}

		private static string FormatCustom (string format, int number, NumberFormatInfo nfi)
		{
			string strnum = FormatGeneral (number, -1, nfi, true);
			FormatParse fp = new FormatParse (format); // FIXME: use nfi!
			int sign = (number < 0) ? -1 : (number > 0) ? 1 : 0;
			return fp.FormatNumber (strnum, sign);
		}

		private static string FormatCustom (string format, long number, NumberFormatInfo nfi)
		{
			string strnum = FormatGeneral (number, -1, nfi, true);
			FormatParse fp = new FormatParse (format); // FIXME: use nfi!
			int sign = (number < 0) ? -1 : (number > 0) ? 1 : 0;
			return fp.FormatNumber (strnum, sign);
		}

		private static string FormatCustom (string format, byte number, NumberFormatInfo nfi)
		{
			string strnum = FormatGeneral (number, -1, nfi, true);
			FormatParse fp = new FormatParse (format); // FIXME: use nfi!
			return fp.FormatNumber (strnum, (number == 0) ? 0 : 1);
		}

		private static string FormatCustom (string format, ushort number, NumberFormatInfo nfi)
		{
			string strnum = FormatGeneral (number, -1, nfi, true);
			FormatParse fp = new FormatParse (format); // FIXME: use nfi!
			return fp.FormatNumber (strnum, (number == 0) ? 0 : 1);
		}

		private static string FormatCustom (string format, uint number, NumberFormatInfo nfi)
		{
			string strnum = FormatGeneral (number, -1, nfi, true);
			FormatParse fp = new FormatParse (format); // FIXME: use nfi!
			return fp.FormatNumber (strnum, (number == 0) ? 0 : 1);
		}

		private static string FormatCustom (string format, ulong number, NumberFormatInfo nfi)
		{
			string strnum = FormatGeneral (number, -1, nfi, true);
			FormatParse fp = new FormatParse (format); // FIXME: use nfi!
			return fp.FormatNumber (strnum, (number == 0) ? 0 : 1);
		}
	}

class FormatSection {
	public int nph;
	public int nphPreDot;
	public int npercent;
	public int ndividers;
	public int ntokens;
	public string [] tokens;
	public int [] TokenTypes;
	public bool HaveDot;
	public bool HaveSci;
	public bool sciSignAlways = false;
	public int sciDigits;
	public int numCommas;
}

class FormatParse {
	const int AS_IS = 0;
	const int PH_0 = 1;
	const int PH_NUMBER = 2;
	const int COMMA = 3;
	const int PERCENT = 4;
	const int DIVIDERS = 5;
	const int DOT = 6;
	const int ESCAPE_SEQ = 7;
	const int SCIENTIFIC = 8;
	const int NEW_SECTION = 9;
	private FormatSection [] sections = new FormatSection[3];
	private int nsections = 0;
	private int pos; // Position in the format string
	private int group = 0; // Used in FormatPlain to insert a comma between groups of digits
	private bool isNegative;

	private FormatParse ()
	{
	}
	
	public FormatParse (string format)
	{
		parseFormat (format);
	}

	private void FormatSci (char [] digits, ArrayList outputList, FormatSection sec)
	{
		int tokidx = sec.ntokens - 1;

		// Output everything until we get to the SCIENTIFIC
		while (tokidx >= 0 && sec.TokenTypes [tokidx] != SCIENTIFIC){
			outputList.Add ((string) sec.tokens [tokidx--]);
		}

		// Exponent
		int exponent = digits.Length - sec.nph;
		outputList.Add ((string) exponent.ToString ());
		if (sec.sciSignAlways && exponent > 0)
			outputList.Add ("+");
		outputList.Add ((string) sec.tokens [tokidx--]);

		if (exponent < 0) {
			char [] newDigits;
			exponent = -exponent;
			newDigits = new char [digits.Length + exponent];
			Array.Copy (digits, 0, newDigits, exponent, digits.Length);
			for (int i = 0; i < exponent; i++)
				newDigits[i] = '0';
			digits = newDigits;
		}

		// Now format the rest
		int digitIdx = 0;
		if (sec.HaveDot)
			FormatDot (digits, ref digitIdx, outputList, sec, tokidx, 0);
		else
			FormatPlain (digits, ref digitIdx, outputList, sec, tokidx, 0, sec.numCommas > 0);
	}

	private void FormatDot (char [] digits, ref int digitIdx, ArrayList outputList, 
				FormatSection sec, int lastToken, int firstToken)
	{
		int tokidx = lastToken;
		int type;

		while (tokidx >= firstToken) {
			type = sec.TokenTypes [tokidx];
			if (type == DOT || type == PH_NUMBER || type == PH_0)
				break;
			tokidx--;
		}

		if (tokidx > 0) {
			char [] postDotDigits = new char [sec.nph - sec.nphPreDot];
			int max = (postDotDigits.Length > digits.Length) ? digits.Length : postDotDigits.Length;
			Array.Copy (digits, 0, postDotDigits, 0, max);
			int postDotDigitsIdx = 0;
			FormatPlain (postDotDigits, ref postDotDigitsIdx, outputList, sec, lastToken, tokidx, false);
			tokidx--;
			digitIdx += max;
			FormatPlain (digits, ref digitIdx, outputList, sec, tokidx, 0, sec.numCommas > 0);
		}
	}

	private void FormatPlain (char [] digits, ref int digitIdx, ArrayList outputList, 
				FormatSection sec, int lastToken, int firstToken, bool insertComma)
	{
		int tokidx = lastToken;
		int type;

		while (tokidx >= firstToken) {
			type = sec.TokenTypes [tokidx];
			if (type == PH_0 || type == PH_NUMBER) {
				//FIXME: PH_NUMBER should also check for significant digits
				// Console.WriteLine ("group : {0}", group);
				int i = sec.tokens [tokidx].Length - 1;
				while (i >= 0) {
					if (insertComma && group == 3) {
						outputList.Add (","); // FIXME: from NumberFormatInfo
						group = 0;
					}

					if (digitIdx < digits.Length)
						outputList.Add ((string) digits[digitIdx++].ToString ());
					else
						outputList.Add ("0");
					i--;
					if (insertComma)
						group++;
					sec.nph--;
					while (sec.nph == 0 && digitIdx < digits.Length) {
						// Flush the numbers left
						if (insertComma && group == 3){
							outputList.Add (","); // FIXME: from NumberFormatInfo
							group = 0;
						}
						outputList.Add ((string) digits [digitIdx++].ToString ());
						if (insertComma)
							group++;
					}

					if (sec.nph == 0 && isNegative)
						outputList.Add ("-");
				}
			} else {
				outputList.Add ((string) sec.tokens [tokidx]);
			}
			tokidx--;
		}

	}

	private char [] AdjustDigits (string number, FormatSection sec)
	{
		char [] digits = number.ToCharArray ();
		char [] newDigits = digits;
		int decPointIdx = 0;
		int postDot = 0;
		
		decPointIdx -= sec.ndividers * 3;
		decPointIdx += sec.npercent * 2;
		if (sec.HaveDot){
			postDot = sec.nph - sec.nphPreDot;
			decPointIdx += postDot;
		}

		if (decPointIdx > 0) {
			newDigits = new char [digits.Length + decPointIdx];
			Array.Copy (digits, 0, newDigits, 0, digits.Length);
			for (int i = 0; i < decPointIdx; i++)
				newDigits[digits.Length + i] = '0';
		} else if (decPointIdx < 0) {
			decPointIdx = -decPointIdx;
			if (decPointIdx >= digits.Length) {
				if (sec.HaveSci){
				} else {
					// The numbers turns into 0 when formatting applied
					digits = new char [1] {'0'};
				}
			} else {
				int newLength = digits.Length - decPointIdx + postDot - 1;
				newDigits = new char [newLength];
				int max = digits.Length >= newLength ? newLength : digits.Length;
				Array.Copy (digits, 0, newDigits, 0, max);
				if (newLength > digits.Length)
					for (int i = 0; i < decPointIdx; i++)
						newDigits[digits.Length + i] = '0';
			}
		}

		return newDigits;
	}

	public string FormatNumber (string number, int signValue)
	{
		char [] digits;

		isNegative = signValue < 0;
		int section = 0;
		if (signValue < 0 && nsections > 0)
			section = 1;
		if (signValue == 0 && nsections > 1)
			section = 2;

		FormatSection sec = sections [section];
		digits = AdjustDigits (number.ToString (), sec);
		if (digits.Length == 1 && digits [0] == '0')
			if (nsections > 2)
				sec = sections [2]; // Format as a 0
			else
				sec = sections [0]; // Format as positive

		ArrayList outputList = new ArrayList ();

		int digitIdx = 0;
		Array.Reverse (digits);

		if (sec.HaveSci)
			FormatSci (digits, outputList, sec);
		else if (sec.HaveDot)
			FormatDot (digits, ref digitIdx, outputList, sec, sec.ntokens - 1, 0);
		else
			FormatPlain (digits, ref digitIdx, outputList, sec, sec.ntokens - 1, 0, sec.numCommas > 0);

		string result = "";
		for (int i = outputList.Count - 1; i >= 0; i--) {
			result += (string) outputList[i];
		}

		return result;
	}

	private void parseFormat (string format)
	{
		char [] fmt_chars = format.ToCharArray ();
		int fmtlen = fmt_chars.Length;
		int type = AS_IS;
		int prevType = AS_IS;
		string token;

		sections[0] = new FormatSection();
		while (pos < fmtlen) {

			token = getNextToken (fmt_chars, fmtlen, out type);
			if (type == NEW_SECTION) {
				nsections++;
				if (nsections > 3)
					break;
				sections[nsections] = new FormatSection();
			} else {
				prevType = AddToken (token, type, prevType);
			}			
		}
	}

	private int AddToken (string token, int type, int prevType)
	{
		FormatSection sec = sections[nsections];
		string [] newTokens = new string [sec.ntokens + 1];
		int [] newTokenTypes = new int [sec.ntokens + 1];
		for (int i = 0; i < sec.ntokens; i++) {
			newTokens[i] = sec.tokens[i];
			newTokenTypes[i] = sec.TokenTypes[i];
		}

		switch (type) {
		case ESCAPE_SEQ :
			type = AS_IS;
			break;
		case COMMA :
			if (!sec.HaveDot && (prevType == PH_0 || prevType == PH_NUMBER)) {
				sec.numCommas++;
			} else
				type = AS_IS;

			token = "";
			break;
		case DOT :
			if (!sec.HaveDot && (prevType == PH_0 || prevType == PH_NUMBER ||
			    prevType == DIVIDERS || prevType == COMMA)) {
				sec.HaveDot = true;
				sec.nphPreDot = sec.nph;
			} else
				type = AS_IS;

			break;
		case PERCENT :
			sec.npercent++;
			break;
		case DIVIDERS :
			token = "";
			if (!sec.HaveDot)
				sec.ndividers = token.Length;
			else
				type = AS_IS;
			break;
		case PH_0 :
			if (!sec.HaveSci)
				sec.nph += token.Length;
			else
				type = AS_IS;
			break;
		case PH_NUMBER :
			if (!sec.HaveSci)
				sec.nph += token.Length;
			else
				type = AS_IS;
			break;
		case SCIENTIFIC :
			if (!sec.HaveSci && sec.nph > 0) {
				sec.HaveSci = true;
				char [] sci = token.ToCharArray ();
				sec.sciSignAlways = sci[1] == '+' ? true : false;
				int expLen = sci[1] == '0' ? token.Length - 1 : token.Length - 2;
				sec.sciDigits = expLen;
				token = sci[0].ToString ();
			} else {
				type = AS_IS;
			}
			break;
		}

		newTokens[sec.ntokens] = token;
		newTokenTypes[sec.ntokens] = type;
		sec.tokens = newTokens;
		sec.TokenTypes = newTokenTypes;
		sec.ntokens++;
		return type;
	}
	
	private string getNextToken (char [] fmt_chars, int fmtlen, out int type)
	{
		int curpos = pos;
		string result = null;
		char current;
		
		type = AS_IS; // Default
		current = fmt_chars[curpos];
		if (current == ';'){
			type = NEW_SECTION;
			result = "NEW_SECTION";
			pos++;
		}
		else if (current == '\'' || current == '"') {
			char Quote = current;
			curpos++;
			int endpos = Array.IndexOf (fmt_chars, current, curpos);
			if (endpos == -1)
				endpos = fmtlen;
			result = new string (fmt_chars, curpos, endpos - curpos);
			pos = endpos + 1;
		} 
		else if (current == '\\') { //MS seems not to translate escape seqs!
			type = ESCAPE_SEQ;
			current = fmt_chars[++pos];
			result = current.ToString ();
			pos++;
		}
		else if (current == '%') {
			type = PERCENT;
			result = "%";
			pos++;
		}
		else if (current == '.') {
			type = DOT;
			result = ".";
			pos++;
		}
		else if (current == ',') {
			int begpos = curpos;

			while (++curpos < fmtlen && fmt_chars[curpos] == ',');
			if (curpos == fmtlen || fmt_chars[curpos] == '.') {
				// ,,,,
				result = new string (fmt_chars, begpos, curpos - begpos);
				type = DIVIDERS;
				pos = curpos;
			} else {
				result = ",";
				type = COMMA;
				pos++;
			}
		}
		else if (current == '0' || current == '#') {
			char placeHolder = current;
			int begpos = curpos;
			type = placeHolder == '0' ? PH_0 : PH_NUMBER;
			curpos++;
			while (curpos < fmtlen && fmt_chars [curpos] == placeHolder)
				curpos++;
			result = new string (fmt_chars, begpos, curpos - begpos);
			pos = curpos;
		}
		else if (current == 'e' || current == 'E') {
			if (fmtlen <= curpos + 1){
				result = current.ToString ();
				pos++;
			}
			else {
				char next1 = fmt_chars [curpos + 1];

				if (next1 != '-' && next1 != '+' && next1 != '0') {
					result = new string (fmt_chars, curpos, 2);
					pos += 2;
				}
				else {
					int begpos = curpos;

					if (next1 == '-' || next1 == '+')
						curpos++;

					curpos++;

					if (curpos < fmtlen && fmt_chars [curpos] == '0'){
						type = SCIENTIFIC;
						while (curpos < fmtlen && fmt_chars [curpos] == '0')
							curpos++;
					}

					result = new string (fmt_chars, begpos, curpos - begpos);
					pos = curpos;
				}
			}
		}
		else {
			char [] format_spec = { '0', '#', ',', '.', '%', 'E', 'e', '"', '\'', '\\' };
			int nextFE;

			while (curpos < fmtlen) {
				current = fmt_chars[curpos];
				nextFE = Array.IndexOf (format_spec, current);
				if (nextFE != -1)
					break;
				curpos++;
			}

			result = new string (fmt_chars, pos, curpos - pos);
			pos = curpos;
		}

		return result;
	}
}

}
