//
// System.FloatingPointFormatter.cs
//
// Author:
//   Pedro Martinez Juli�  <yoros@wanadoo.es>
//
// Copyright (C) 2003 Pedro Mart�nez Juli� <yoros@wanadoo.es>
//

using System;
using System.Text;
using System.Collections;
using System.Globalization;


namespace System {

	internal class FloatingPointFormatter {

		NumberFormatInfo nfi;
		string format;
		double value;
		string number_as_string;
		
		double p;
		double p10;
		int dec_len;
		int dec_len_min;

		public FloatingPointFormatter
			(string format, NumberFormatInfo nfi, double value,
			 double p, double p10, int dec_len, int dec_len_min) {
			this.format = format;
			this.nfi = nfi;
			this.value = value;
			this.p = p;
			this.p10 = p10;
			this.dec_len = dec_len;
			this.dec_len_min = dec_len_min;
			number_as_string = NumberToString();
		}

		public string String {
			get { return number_as_string; }
		}

		private string NumberToString () {
			if (format == null)
				format = "G";
			if (nfi == null)
				nfi = NumberFormatInfo.CurrentInfo;
			char specifier;
			int precision;
			if (!ParseFormat(out specifier, out precision)) {
				try {
					return FormatCustom();
				}
				catch (Exception e) {
					return FormatGeneral(-1);
				}
			}
			switch (specifier) {
			case 'C':
				return FormatCurrency(precision);
			case 'D':
				throw new FormatException(Locale.GetText(
					"The specified format is invalid") + ": " + format);
			case 'E':
				return FormatExponential(precision);
			case 'F':
				return FormatFixedPoint(precision);
			case 'G':
				return FormatGeneral(precision);
			case 'N':
				return FormatNumber(precision);
			case 'P':
				return FormatPercent(precision);
			case 'R':
				return FormatReversible(precision);
			case 'X':
				throw new FormatException(Locale.GetText(
					"The specified format is invalid") + ": " + format);
			default:
				throw new FormatException(Locale.GetText(
					"The specified format is invalid") + ": " + format);
			}
		}

		private bool ParseFormat (out char specifier, out int precision) {
			specifier = '\0';
			precision = -1;
			switch (format.Length) {
			case 1:
				specifier = Char.ToUpper(format[0]);
				return true;
			case 2:
				if (Char.IsLetter(format[0]) && Char.IsDigit(format[1])) {
					specifier = Char.ToUpper(format[0]);
					precision = Convert.ToInt32(format[1] - '0');
					return true;
				}
				break;
			case 3:
				if (Char.IsLetter(format[0]) && Char.IsDigit(format[1])
						&& Char.IsDigit(format[2])) {
					specifier = Char.ToUpper(format[0]);
					precision = Convert.ToInt32(format.Substring(1, 2));
					return true;
				}
				break;
			}
			return false;
		}

		private void Normalize
				(double value, out long mantissa, out int exponent) {
			mantissa = 0;
			exponent = 0;
			if (value == 0.0 ||
				Double.IsInfinity(value) ||
				Double.IsNaN(value)) {
				return;
			}
			value = Math.Abs(value);
			if (value > p10) {
				while (value > p10) {
					value /= 10;
					exponent++;
				}
			}
			else if (value < p) {
				while (value < p) {
					value *= 10;
					exponent--;
				}
			}
			mantissa = (long) Math.Round(value);
		}

		private char[] Digits =
			{'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};

		private string FormatCurrency (int precision) {
			StringBuilder sb = new StringBuilder();
			if (Double.IsNaN(value)) {
				sb.Append(nfi.NaNSymbol);
			}
			else if (Double.IsInfinity(value)) {
				sb.Append(nfi.PositiveInfinitySymbol);
			}
			else {
				int decimals = (precision >= 0) ?
					precision : nfi.CurrencyDecimalDigits;
				long mantissa;
				int exponent;
				Normalize(value, out mantissa, out exponent);
				if (exponent >= 0) {
					while (decimals > 0) {
						sb.Append("0");
						decimals--;
					}
				}
				else {
					while (exponent < 0) {
						if (exponent > -(decimals+1)) {
							sb.Insert(0, Digits[mantissa % 10]);
						}
						mantissa /= 10;
						exponent++;
					}
				}
				sb.Insert(0, nfi.NumberDecimalSeparator);
				if (mantissa == 0) {
					sb.Insert(0, "0");
				}
				else {
					int i = 0;
					while (exponent > 0) {
						int fin = nfi.NumberGroupSizes[i];
						for (int j = 0; (j < fin) && (exponent > 0);
								j++, exponent--) {
							sb.Insert(0, "0");
						}
						sb.Insert(0, nfi.NumberGroupSeparator);
						if (i < nfi.NumberGroupSizes.Length - 1)
							i++;
					}
					while (mantissa != 0) {
						int fin = nfi.NumberGroupSizes[i];
						for (int j = 0; (j < fin) && (mantissa != 0); j++) {
							sb.Insert(0, Digits[mantissa % 10]);
							mantissa /= 10;
						}
						if (mantissa != 0)
							sb.Insert(0, nfi.NumberGroupSeparator);
						if (i < nfi.NumberGroupSizes.Length - 1)
							i++;
					}
				}
			}
			string numb = sb.ToString();
			if (value < 0) {
				switch (nfi.CurrencyNegativePattern) {
				case 0:
					return "(" + nfi.CurrencySymbol + numb + ")";
				case 1:
					return nfi.NegativeSign + nfi.CurrencySymbol + numb;
				case 2:
					return nfi.CurrencySymbol + nfi.NegativeSign + numb;
				case 3:
					return nfi.CurrencySymbol + numb + nfi.NegativeSign;
				case 4:
					return "(" + numb + nfi.CurrencySymbol + ")";
				case 5:
					return nfi.NegativeSign + numb + nfi.CurrencySymbol;
				case 6:
					return numb + nfi.NegativeSign + nfi.CurrencySymbol;
				case 7:
					return numb + nfi.CurrencySymbol + nfi.NegativeSign;
				case 8:
					return nfi.NegativeSign + numb + " " + nfi.CurrencySymbol;
				case 9:
					return nfi.NegativeSign + nfi.CurrencySymbol + " " + numb;
				case 10:
					return numb + " " + nfi.CurrencySymbol + nfi.NegativeSign;
				case 11:
					return nfi.CurrencySymbol + " " + numb + nfi.NegativeSign;
				case 12:
					return nfi.CurrencySymbol + " " + nfi.NegativeSign + numb;
				case 13:
					return numb + nfi.NegativeSign + " " + nfi.CurrencySymbol;
				case 14:
					return "(" + nfi.CurrencySymbol + " " + numb + ")";
				case 15:
					return "(" + numb + " " + nfi.CurrencySymbol + ")";
				default:
					throw new ArgumentException(Locale.GetText(
						"Invalid CurrencyNegativePattern"));
				}
			}
			else {
				switch (nfi.CurrencyPositivePattern) {
				case 0:
					return nfi.CurrencySymbol + numb ;
				case 1:
					return numb + nfi.CurrencySymbol;
				case 2:
					return nfi.CurrencySymbol + " " + numb;
				case 3:
					return numb + " " + nfi.CurrencySymbol;
				default:
					throw new ArgumentException(Locale.GetText(
						"invalid CurrencyPositivePattern"));
				}
			}
		}

		[MonoTODO]
		private string FormatExponential (int precision) {
			StringBuilder sb = new StringBuilder();
			if (value == 0.0) {
				sb.Append("0");
			}
			else if (Double.IsNaN(value)) {
				sb.Append(nfi.NaNSymbol);
			}
			else if (Double.IsPositiveInfinity(value)) {
				sb.Append(nfi.PositiveInfinitySymbol);
			}
			else if (Double.IsNegativeInfinity(value)) {
				sb.Append(nfi.NegativeInfinitySymbol);
			}
			else {
				int decimals = (precision >= 0) ?
					precision : nfi.NumberDecimalDigits;
				long mantissa;
				int exponent;
				Normalize(value, out mantissa, out exponent);
				bool not_null = false;
				for (int i = 0; i < dec_len; i++) {
					if ((not_null == false) && ((mantissa % 10) != 0)) {
						not_null = true;
					}
					if (not_null) {
						sb.Insert(0,Digits[mantissa % 10]);
					}
					mantissa /= 10;
					exponent++;
				}
				if (sb.Length == 0) {
					sb.Insert(0, "0");
				}
				sb.Insert(0,
					Digits[mantissa % 10] + nfi.NumberDecimalSeparator);
				if (exponent > 0) {
					sb.Append("E" + nfi.PositiveSign);
				}
				else {
					sb.Append("E" + nfi.NegativeSign);
				}
				sb.Append(Math.Abs(exponent).ToString());
				if (value < 0.0) {
					sb.Insert(0, nfi.NegativeSign);
				}
			}
			return sb.ToString();
		}

		private string FormatFixedPoint (int precision) {
			StringBuilder sb = new StringBuilder();
			if (Double.IsNaN(value)) {
				sb.Append(nfi.NaNSymbol);
			}
			else if (Double.IsPositiveInfinity(value)) {
				sb.Append(nfi.PositiveInfinitySymbol);
			}
			else if (Double.IsNegativeInfinity(value)) {
				sb.Append(nfi.NegativeInfinitySymbol);
			}
			else {
				int decimals = (precision >= 0) ?
					precision : nfi.NumberDecimalDigits;
				long mantissa;
				int exponent;
				Normalize(value, out mantissa, out exponent);
				if (exponent >= 0) {
					while (decimals > 0) {
						sb.Append("0");
						decimals--;
					}
				}
				else {
					while (exponent < 0) {
						if (exponent > -(decimals+1)) {
							sb.Insert(0, Digits[mantissa % 10]);
						}
						mantissa /= 10;
						exponent++;
					}
				}
				sb.Insert(0, nfi.NumberDecimalSeparator);
				if (mantissa == 0) {
					sb.Insert(0, "0");
				}
				else {
					while (exponent > 0) {
						sb.Insert(0, "0");
						exponent--;
					}
					while (mantissa != 0) {
						sb.Insert(0, Digits[mantissa % 10]);
						mantissa /= 10;
					}
				}
				if (value < 0.0) {
					sb.Insert(0, nfi.NegativeSign);
				}
			}
			return sb.ToString();
		}

		private string FormatGeneral (int precision) {
			StringBuilder sb = new StringBuilder();
			if (value == 0.0) {
				sb.Append("0");
			}
			else if (Double.IsNaN(value)) {
				sb.Append(nfi.NaNSymbol);
			}
			else if (Double.IsPositiveInfinity(value)) {
				sb.Append(nfi.PositiveInfinitySymbol);
			}
			else if (Double.IsNegativeInfinity(value)) {
				sb.Append(nfi.NegativeInfinitySymbol);
			}
			else {
				int decimals = (precision >= 0) ?
					precision : nfi.NumberDecimalDigits;
				long mantissa;
				int exponent;
				Normalize(value, out mantissa, out exponent);
				if (exponent > dec_len_min && exponent <= 0) {
					bool not_null = false;
					while (exponent < 0) {
						if ((not_null == false) && ((mantissa % 10) != 0)) {
							not_null = true;
						}
						if (not_null) {
							sb.Insert(0,Digits[mantissa % 10]);
						}
						mantissa /= 10;
						exponent++;
					}
					if (sb.Length != 0) {
						sb.Insert(0, nfi.NumberDecimalSeparator);
					}
					if (mantissa == 0) {
						sb.Insert(0, "0");
					}
					else {
						while (mantissa > 0) {
							sb.Insert(0, Digits[mantissa % 10]);
							mantissa /= 10;
						}
					}
				}
				else {
					bool not_null = false;
					for (int i = 0; i < dec_len; i++) {
						if ((not_null == false) && ((mantissa % 10) != 0)) {
							not_null = true;
						}
						if (not_null) {
							sb.Insert(0,Digits[mantissa % 10]);
						}
						mantissa /= 10;
						exponent++;
					}
					sb.Insert(0,
						Digits[mantissa % 10] + nfi.NumberDecimalSeparator);
					if (exponent > 0) {
						sb.Append("E" + nfi.PositiveSign);
					}
					else {
						sb.Append("E" + nfi.NegativeSign);
					}
					sb.Append(Math.Abs(exponent).ToString());
				}
				if (value < 0.0) {
					sb.Insert(0, nfi.NegativeSign);
				}
			}
			return sb.ToString();
		}

		private string FormatNumber (int precision) {
			StringBuilder sb = new StringBuilder();
			if (Double.IsNaN(value)) {
				sb.Append(nfi.NaNSymbol);
			}
			else if (Double.IsInfinity(value)) {
				sb.Append(nfi.PositiveInfinitySymbol);
			}
			else {
				int decimals = (precision >= 0) ?
					precision : nfi.NumberDecimalDigits;
				long mantissa;
				int exponent;
				Normalize(value, out mantissa, out exponent);
				if (exponent >= 0) {
					while (decimals > 0) {
						sb.Append("0");
						decimals--;
					}
				}
				else {
					while (exponent < 0) {
						if (exponent > -(decimals+1)) {
							sb.Insert(0, Digits[mantissa % 10]);
						}
						mantissa /= 10;
						exponent++;
					}
				}
				sb.Insert(0, nfi.NumberDecimalSeparator);
				if (mantissa == 0) {
					sb.Insert(0, "0");
				}
				else {
					int i = 0;
					while (exponent > 0) {
						int fin = nfi.NumberGroupSizes[i];
						for (int j = 0; (j < fin) && (exponent > 0);
								j++, exponent--) {
							sb.Insert(0, "0");
						}
						sb.Insert(0, nfi.NumberGroupSeparator);
						if (i < nfi.NumberGroupSizes.Length - 1)
							i++;
					}
					while (mantissa != 0) {
						int fin = nfi.NumberGroupSizes[i];
						for (int j = 0; (j < fin) && (mantissa != 0); j++) {
							sb.Insert(0, Digits[mantissa % 10]);
							mantissa /= 10;
						}
						if (mantissa != 0)
							sb.Insert(0, nfi.NumberGroupSeparator);
						if (i < nfi.NumberGroupSizes.Length - 1)
							i++;
					}
				}
			}
			string numb = sb.ToString();
			if (value < 0) {
				switch (nfi.NumberNegativePattern) {
				case 0:
					return "(" + numb + ")";
				case 1:
					return nfi.NegativeSign + numb;
				case 2:
					return nfi.NegativeSign + " " + numb;
				case 3:
					return numb + nfi.NegativeSign;
				case 4:
					return numb + " " + nfi.NegativeSign;
				default:
					throw new ArgumentException(Locale.GetText(
						"Invalid NumberNegativePattern"));
				}
			}
			return numb;
		}

		[MonoTODO]
		private string FormatPercent (int precision) {
			StringBuilder sb = new StringBuilder();
			if (Double.IsNaN(value)) {
				sb.Append(nfi.NaNSymbol);
			}
			else if (Double.IsInfinity(value)) {
				sb.Append(nfi.PositiveInfinitySymbol);
			}
			else {
				int decimals = (precision >= 0) ?
					precision : nfi.PercentDecimalDigits;
				long mantissa;
				int exponent;
				Normalize(value, out mantissa, out exponent);
				exponent += 2;
				if (exponent >= 0) {
					while (decimals > 0) {
						sb.Append("0");
						decimals--;
					}
				}
				else {
					while (exponent < 0) {
						if (exponent > -(decimals+1)) {
							sb.Insert(0, Digits[mantissa % 10]);
						}
						mantissa /= 10;
						exponent++;
					}
				}
				sb.Insert(0, nfi.NumberDecimalSeparator);
				if (mantissa == 0) {
					sb.Insert(0, "0");
				}
				else {
					int i = 0;
					while (exponent > 0) {
						int fin = nfi.NumberGroupSizes[i];
						for (int j = 0; (j < fin) && (exponent > 0);
								j++, exponent--) {
							sb.Insert(0, "0");
						}
						sb.Insert(0, nfi.NumberGroupSeparator);
						if (i < nfi.NumberGroupSizes.Length - 1)
							i++;
					}
					while (mantissa != 0) {
						int fin = nfi.NumberGroupSizes[i];
						for (int j = 0; (j < fin) && (mantissa != 0); j++) {
							sb.Insert(0, Digits[mantissa % 10]);
							mantissa /= 10;
						}
						if (mantissa != 0)
							sb.Insert(0, nfi.NumberGroupSeparator);
						if (i < nfi.NumberGroupSizes.Length - 1)
							i++;
					}
				}
			}
			string numb = sb.ToString();
			if (value < 0) {
				switch (nfi.PercentNegativePattern) {
				case 0:
					return nfi.NegativeSign + numb + " " + nfi.PercentSymbol;
				case 1:
					return nfi.NegativeSign + numb + nfi.PercentSymbol;
				case 2:
					return nfi.NegativeSign + nfi.PercentSymbol + numb;
				default:
					throw new ArgumentException(Locale.GetText(
						"Invalid PercentNegativePattern"));
				}
			}
			else {
				switch (nfi.PercentPositivePattern) {
				case 0:
					return numb + " " + nfi.PercentSymbol;
				case 1:
					return numb + nfi.PercentSymbol;
				case 2:
					return nfi.PercentSymbol + numb;
				default:
					throw new ArgumentException(Locale.GetText(
						"invalid PercehtPositivePattern"));
				}
			}
		}

		[MonoTODO]
		private string FormatReversible (int precision) {
			return FormatGeneral(precision);
		}

		private string FormatCustom () {
			string first, middle, last;
			int first_semicolon, last_semicolon;
			first_semicolon = format.IndexOf(';');
			last_semicolon = format.LastIndexOf(';');
			if (first_semicolon == last_semicolon) {
				if (first_semicolon == -1) {
					return FormatCustomParser(format);
				}
				if (value >= 0.0) {
					return FormatCustomParser
						(format.Substring(0, first_semicolon - 1));
				}
				if (value < 0.0) {
					return FormatCustomParser
						(format.Substring(first_semicolon + 1));
				}
			}
			if (value > 0.0) {
				return FormatCustomParser
					(format.Substring(0, first_semicolon - 1));
			}
			if (value == 0.0) {
				return FormatCustomParser
					(format.Substring
					(first_semicolon + 1, last_semicolon - first_semicolon));
			}
			if (value < 0.0) {
				return FormatCustomParser
					(format.Substring(last_semicolon + 1));
			}
			return "";
		}

		private struct Flags {
			public int NumberOfColons;
			public bool Groupping;
			public bool Percent;
			public int DotPos;
			public int ExpPos;
			public int FirstFormatPos;
		}

		private Flags AnalizeFormat (string format) {
			Flags f;
			f.NumberOfColons = 0;
			f.DotPos = -1;
			f.ExpPos = -1;
			f.Groupping = false;
			f.Percent = false;
			f.FirstFormatPos = -1;
			int aux = 0, i = 0;
			foreach (char c in format) {
				switch (c) {
				case ',':
					aux++;
					break;
				case '0':
				case '#':
					if (f.FirstFormatPos < 0) {
						f.FirstFormatPos = i;
					}
					if (aux > 0) {
						f.Groupping = true;
						aux = 0;
					}
					break;
				case '.':
					f.DotPos = i;
					if (aux > 0) {
						f.NumberOfColons = aux;
						aux = 0;
					}
					break;
				case '%':
					f.Percent = true;
					break;
				case 'e':
				case 'E':
					f.ExpPos = i;
					break;
				}
				i++;
			}
			if (aux > 0) {
				f.NumberOfColons = aux;
			}
			return f;
		}

		private string FormatCustomParser (string format) {
			if (Double.IsInfinity(value)) {
				throw new FormatException(
					"Invalid number for this format: infinity");
			}
			if (Double.IsNaN(value)) {
				throw new FormatException(
					"Invalid number for this format: NaN");
			}
			long mantissa;
			int exponent;
			Normalize(value, out mantissa, out exponent);
			Flags f = AnalizeFormat(format);
			if (f.Percent) {
				exponent += 2;
			}
			if (f.NumberOfColons > 0) {
				exponent -= 3 * f.NumberOfColons;
			}
			StringBuilder sb = new StringBuilder();
			if (f.ExpPos > 0) {
				StringBuilder sb_exp = new StringBuilder();
				while (mantissa > 0) {
					sb_exp.Insert(0, Digits[mantissa % 10]);
					mantissa /= 10;
					exponent++;
				}
				exponent--;
				int k;
				for (k = sb_exp.Length - 1;
					k > 0 && sb_exp[k] == '0'; k--);
				sb_exp.Remove(k + 1, sb_exp.Length - k - 1);
				for (int i = f.DotPos - 2; i >= 0; i--) {
					char c = format[i];
					if (i > 0 && format[i-1] == '\\') {
						sb.Insert(0, c);
						i -= 2;
						continue;
					}
					switch (c) {
					case ',':
					case '#':
						break;
					case '0':
						sb.Insert(0, '0');
						break;
					default:
						sb.Insert(0, c);
						break;
					}
				}
				sb.Append(sb_exp[0]);
				sb.Append(nfi.NumberDecimalSeparator);
				for (int j = 1, i = f.DotPos + 1; i < f.ExpPos; i++) {
					char c = format[i];
					switch (c) {
					case '\\':
						sb.Append(format[++i]);
						break;
					case '0':
						if (j >= sb_exp.Length) {
							sb.Append('0');
							break;
						}
						goto case '#';
					case '#':
						if (j < sb_exp.Length) {
							if (i == f.ExpPos-1) {
								int a = sb_exp[j] - '0';
								int b = sb_exp[j+1] - '0';
								if (((b == 5) && ((a % 2) == 0)) || (b > 5)) {
									a++;
								}
								sb.Append(Digits[a]);
							}
							else {
								sb.Append(sb_exp[j++]);
							}
						}
						break;
					default:
						sb.Append(c);
						break;
					}
				}
				sb.Append(format[f.ExpPos]);
				if (exponent < 0) {
					sb.Append('-');
				}
				int fin, inicio;
				inicio = f.ExpPos + 1;
				if (format[inicio] == '-') {
					inicio++;
				}
				else if (format[inicio] == '+') {
					if (exponent > 0) {
						sb.Append('+');
					}
					inicio++;
				}
				fin = inicio;
				while (fin < format.Length && format[fin++] == '0');
				StringBuilder sb_exp_num = new StringBuilder();
				exponent = Math.Abs(exponent);
				while (exponent > 0) {
					sb_exp_num.Insert(0, Digits[exponent % 10]);
					exponent /= 10;
				}
				while (sb_exp_num.Length < (fin - inicio)) {
					sb_exp_num.Insert(0, '0');
				}
				sb.Append(sb_exp_num.ToString());
				for (int i = fin; i < format.Length; i++) {
					sb.Append(format[i]);
				}
				return sb.ToString();
			}
			else {
				f.ExpPos = format.Length;
			}
			if (f.DotPos < 0) {
				while (exponent < 0) {
					mantissa = (long) Math.Round((double)mantissa / 10);
					exponent++;
				}
				f.DotPos = format.Length;
			}
			else {
				StringBuilder sb_decimal = new StringBuilder();
				while (exponent < 0) {
					sb_decimal.Insert(0, Digits[mantissa % 10]);
					mantissa /= 10;
					exponent++;
				}
				if (sb_decimal.Length > 0 || format[f.DotPos + 1] == '0') {
					sb.Append(nfi.NumberDecimalSeparator);
				}
				int k;
				for (k = sb_decimal.Length - 1;
					k > 0 && sb_decimal[k] == '0'; k--);
				sb_decimal.Remove(k + 1, sb_decimal.Length - k - 1);
				for (int j = 0, i = f.DotPos + 1; i < f.ExpPos; i++) {
					if (format[i] == '0' || format[i] == '#') {
						if (j < sb_decimal.Length) {
							if (i == format.Length-1) {
								int a = sb_decimal[j] - '0';
								int b = sb_decimal[j+1] - '0';
								if (((b == 5) && ((a % 2) == 0)) || (b > 5)) {
									a++;
								}
								sb.Append(Digits[a]);
							}
							else {
								sb.Append(sb_decimal[j++]);
							}
						}
						else if (format[i] == '0') {
							sb.Append('0');
						}
					}
					else if (format[i] == '\\') {
						sb.Append(format[++i]);
					}
					else {
						sb.Append(format[i]);
					}
				}
			}
			int gro = 0;
			for (int i = f.DotPos - 1; i >= f.FirstFormatPos; i--) {
				if (format[i] == '#' || format[i] == '0') {
					if (f.Groupping && gro == nfi.NumberGroupSizes[0]) {
						sb.Insert(0, nfi.NumberGroupSeparator);
						gro = 0;
					}
					gro++;
					if (exponent > 0) {
						sb.Insert(0, '0');
						exponent--;
					}
					else if (mantissa > 0) {
						sb.Insert(0, Digits[mantissa % 10]);
						mantissa /= 10;
					}
					else if (format[i] == '0') {
						sb.Insert(0, '0');
					}
				}
				else if (format[i] != ',') {
					sb.Insert(0, format[i]);
				}
				else if (i > 0 && format[i-1] == '\\') {
					sb.Insert(0, format[i]);
					i -= 2;
				}
			}
			while (exponent > 0) {
				if (f.Groupping && gro == nfi.NumberGroupSizes[0]) {
					sb.Insert(0, nfi.NumberGroupSeparator);
					gro = 0;
				}
				gro++;
				sb.Insert(0, '0');
				exponent--;
			}
			while (mantissa > 0) {
				if (f.Groupping && gro == nfi.NumberGroupSizes[0]) {
					sb.Insert(0, nfi.NumberGroupSeparator);
					gro = 0;
				}
				gro++;
				sb.Insert(0, Digits[mantissa % 10]);
				mantissa /= 10;
			}
			for (int i = f.FirstFormatPos - 1; i >= 0; i--) {
				sb.Insert(0, format[i]);
			}
			return sb.ToString();
		}

	}

}
