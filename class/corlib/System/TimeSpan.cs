//
// System.TimeSpan.cs
//
// Authors:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2001 Duco Fijma
// (C) 2004 Andreas Nahr
// Copyright (C) 2004 Novell (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Text;
using System.Threading;
using System.Globalization;

namespace System
{
	[Serializable]
	[System.Runtime.InteropServices.ComVisible (true)]
	public struct TimeSpan : IComparable, IComparable<TimeSpan>, IEquatable <TimeSpan>
#if NET_4_0
				 , IFormattable
#endif
	{
#if MONOTOUCH
		static TimeSpan () {
			if (MonoTouchAOTHelper.FalseFlag) {
				var comparer = new System.Collections.Generic.GenericComparer <TimeSpan> ();
				var eqcomparer = new System.Collections.Generic.GenericEqualityComparer <TimeSpan> ();
			}
		}
#endif
		public static readonly TimeSpan MaxValue = new TimeSpan (long.MaxValue);
		public static readonly TimeSpan MinValue = new TimeSpan (long.MinValue);
		public static readonly TimeSpan Zero = new TimeSpan (0L);

		public const long TicksPerDay = 864000000000L;
		public const long TicksPerHour = 36000000000L;
		public const long TicksPerMillisecond = 10000L;
		public const long TicksPerMinute = 600000000L;
		public const long TicksPerSecond = 10000000L;

		private long _ticks;

		public TimeSpan (long ticks)
		{
			_ticks = ticks;
		}

		public TimeSpan (int hours, int minutes, int seconds)
		{
			CalculateTicks (0, hours, minutes, seconds, 0, true, out _ticks);
		}

		public TimeSpan (int days, int hours, int minutes, int seconds)
		{
			CalculateTicks (days, hours, minutes, seconds, 0, true, out _ticks);
		}

		public TimeSpan (int days, int hours, int minutes, int seconds, int milliseconds)
		{
			CalculateTicks (days, hours, minutes, seconds, milliseconds, true, out _ticks);
		}

		internal static bool CalculateTicks (int days, int hours, int minutes, int seconds, int milliseconds, bool throwExc, out long result)
		{
			// there's no overflow checks for hours, minutes, ...
			// so big hours/minutes values can overflow at some point and change expected values
			int hrssec = (hours * 3600); // break point at (Int32.MaxValue - 596523)
			int minsec = (minutes * 60);
			long t = ((long)(hrssec + minsec + seconds) * 1000L + (long)milliseconds);
			t *= 10000;

			result = 0;

			bool overflow = false;
			// days is problematic because it can overflow but that overflow can be 
			// "legal" (i.e. temporary) (e.g. if other parameters are negative) or 
			// illegal (e.g. sign change).
			if (days > 0) {
				long td = TicksPerDay * days;
				if (t < 0) {
					long ticks = t;
					t += td;
					// positive days -> total ticks should be lower
					overflow = (ticks > t);
				}
				else {
					t += td;
					// positive + positive != negative result
					overflow = (t < 0);
				}
			}
			else if (days < 0) {
				long td = TicksPerDay * days;
				if (t <= 0) {
					t += td;
					// negative + negative != positive result
					overflow = (t > 0);
				}
				else {
					long ticks = t;
					t += td;
					// negative days -> total ticks should be lower
					overflow = (t > ticks);
				}
			}

			if (overflow) {
				if (throwExc)
					throw new ArgumentOutOfRangeException (Locale.GetText ("The timespan is too big or too small."));
				return false;
			}

			result = t;
			return true;
		}

		public int Days {
			get {
				return (int) (_ticks / TicksPerDay);
			}
		}

		public int Hours {
			get {
				return (int) (_ticks % TicksPerDay / TicksPerHour);
			}
		}

		public int Milliseconds {
			get {
				return (int) (_ticks % TicksPerSecond / TicksPerMillisecond);
			}
		}

		public int Minutes {
			get {
				return (int) (_ticks % TicksPerHour / TicksPerMinute);
			}
		}

		public int Seconds {
			get {
				return (int) (_ticks % TicksPerMinute / TicksPerSecond);
			}
		}

		public long Ticks {
			get {
				return _ticks;
			}
		}

		public double TotalDays {
			get {
				return (double) _ticks / TicksPerDay;
			}
		}

		public double TotalHours {
			get {
				return (double) _ticks / TicksPerHour;
			}
		}

		public double TotalMilliseconds {
			get {
				return (double) _ticks  / TicksPerMillisecond;
			}
		}

		public double TotalMinutes {
			get {
				return (double) _ticks / TicksPerMinute;
			}
		}

		public double TotalSeconds {
			get {
				return (double) _ticks / TicksPerSecond;
			}
		}

		public TimeSpan Add (TimeSpan ts)
		{
			try {
				checked {
					return new TimeSpan (_ticks + ts.Ticks);
				}
			}
			catch (OverflowException) {
				throw new OverflowException (Locale.GetText ("Resulting timespan is too big."));
			}
		}

		public static int Compare (TimeSpan t1, TimeSpan t2)
		{
			if (t1._ticks < t2._ticks)
				return -1;
			if (t1._ticks > t2._ticks)
				return 1;
			return 0;
		}

		public int CompareTo (object value)
		{
			if (value == null)
				return 1;

			if (!(value is TimeSpan)) {
				throw new ArgumentException (Locale.GetText ("Argument has to be a TimeSpan."), "value");
			}

			return Compare (this, (TimeSpan) value);
		}

		public int CompareTo (TimeSpan value)
		{
			return Compare (this, value);
		}

		public bool Equals (TimeSpan obj)
		{
			return obj._ticks == _ticks;
		}

		public TimeSpan Duration ()
		{
			try {
				checked {
					return new TimeSpan (Math.Abs (_ticks));
				}
			}
			catch (OverflowException) {
				throw new OverflowException (Locale.GetText (
					"This TimeSpan value is MinValue so you cannot get the duration."));
			}
		}

		public override bool Equals (object value)
		{
			if (!(value is TimeSpan))
				return false;

			return _ticks == ((TimeSpan) value)._ticks;
		}

		public static bool Equals (TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks == t2._ticks;
		}

		public static TimeSpan FromDays (double value)
		{
			return From (value, TicksPerDay);
		}

		public static TimeSpan FromHours (double value)
		{
			return From (value, TicksPerHour);
		}

		public static TimeSpan FromMinutes (double value)
		{
			return From (value, TicksPerMinute);
		}

		public static TimeSpan FromSeconds (double value)
		{
			return From (value, TicksPerSecond);
		}

		public static TimeSpan FromMilliseconds (double value)
		{
			return From (value, TicksPerMillisecond);
		}

		private static TimeSpan From (double value, long tickMultiplicator) 
		{
			if (Double.IsNaN (value))
				throw new ArgumentException (Locale.GetText ("Value cannot be NaN."), "value");
			if (Double.IsNegativeInfinity (value) || Double.IsPositiveInfinity (value) ||
				(value < MinValue.Ticks) || (value > MaxValue.Ticks))
				throw new OverflowException (Locale.GetText ("Outside range [MinValue,MaxValue]"));

			try {
				value = (value * (tickMultiplicator / TicksPerMillisecond));

				checked {
					long val = (long) Math.Round(value);
					return new TimeSpan (val * TicksPerMillisecond);
				}
			}
			catch (OverflowException) {
				throw new OverflowException (Locale.GetText ("Resulting timespan is too big."));
			}
		}

		public static TimeSpan FromTicks (long value)
		{
			return new TimeSpan (value);
		}

		public override int GetHashCode ()
		{
			return _ticks.GetHashCode ();
		}

		public TimeSpan Negate ()
		{
			if (_ticks == MinValue._ticks)
				throw new OverflowException (Locale.GetText (
					"This TimeSpan value is MinValue and cannot be negated."));
			return new TimeSpan (-_ticks);
		}

		public static TimeSpan Parse (string s)
		{
			if (s == null) {
				throw new ArgumentNullException ("s");
			}

			TimeSpan result;
			Parser p = new Parser (s);
			p.Execute (false, out result);
			return result;
		}

		public static bool TryParse (string s, out TimeSpan result)
		{
			if (s == null) {
				result = TimeSpan.Zero;
				return false;
			}

			Parser p = new Parser (s);
			return p.Execute (true, out result);
		}

#if NET_4_0
		public static TimeSpan Parse (string s, IFormatProvider formatProvider)
		{
			if (s == null)
				throw new ArgumentNullException ("s");

			TimeSpan result;
			Parser p = new Parser (s, formatProvider);
			p.Execute (false, out result);
			return result;
		}

		public static bool TryParse (string s, IFormatProvider formatProvider, out TimeSpan result)
		{
			if (s == null || s.Length == 0) {
				result = TimeSpan.Zero;
				return false;
			}

			Parser p = new Parser (s, formatProvider);
			return p.Execute (true, out result);
		}
#endif

		public TimeSpan Subtract (TimeSpan ts)
		{
			try {
				checked {
					return new TimeSpan (_ticks - ts.Ticks);
				}
			}
			catch (OverflowException) {
				throw new OverflowException (Locale.GetText ("Resulting timespan is too big."));
			}
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder (14);
			
			if (_ticks < 0)
				sb.Append ('-');

			// We need to take absolute values of all components.
			// Can't handle negative timespans by negating the TimeSpan
			// as a whole. This would lead to an overflow for the 
			// degenerate case "TimeSpan.MinValue.ToString()".
			if (Days != 0) {
				sb.Append (Math.Abs (Days));
				sb.Append ('.');
			}

			sb.Append (Math.Abs (Hours).ToString ("D2"));
			sb.Append (':');
			sb.Append (Math.Abs (Minutes).ToString ("D2"));
			sb.Append (':');
			sb.Append (Math.Abs (Seconds).ToString ("D2"));

			int fractional = (int) Math.Abs (_ticks % TicksPerSecond);
			if (fractional != 0) {
				sb.Append ('.');
				sb.Append (fractional.ToString ("D7"));
			}

			return sb.ToString ();
		}

#if NET_4_0
		public string ToString (string format)
		{
			return ToString (format, null);
		}

		public string ToString (string format, IFormatProvider formatProvider)
		{
			if (format == null || format.Length == 0 || format == "c") // Default version
				return ToString ();

			if (format != "g" && format != "G")
				throw new FormatException ("The format is not recognized.");

			NumberFormatInfo number_info = null;
			if (formatProvider != null)
				number_info = (NumberFormatInfo)formatProvider.GetFormat (typeof (NumberFormatInfo));
			if (number_info == null)
				number_info = Thread.CurrentThread.CurrentCulture.NumberFormat;

			string decimal_separator = number_info.NumberDecimalSeparator;
			int days, hours, minutes, seconds, milliseconds, fractional;

			days = Math.Abs (Days);
			hours = Math.Abs (Hours);
			minutes = Math.Abs (Minutes);
			seconds = Math.Abs (Seconds);
			milliseconds = Math.Abs (Milliseconds);
			fractional = (int) Math.Abs (_ticks % TicksPerSecond);

			// Set Capacity depending on whether it's long or shot format
			StringBuilder sb = new StringBuilder (format == "g" ? 16 : 32);
			if (_ticks < 0)
				sb.Append ('-');

			switch (format) {
				case "g": // short version
					if (days != 0) {
						sb.Append (days.ToString ());
						sb.Append (':');
					}
					sb.Append (hours.ToString ());
					sb.Append (':');
					sb.Append (minutes.ToString ("D2"));
					sb.Append (':');
					sb.Append (seconds.ToString ("D2"));
					if (milliseconds != 0) {
						sb.Append (decimal_separator);
						sb.Append (milliseconds.ToString ("D3"));
					}
					break;
				case "G": // long version
					sb.Append (days.ToString ("D1"));
					sb.Append (':');
					sb.Append (hours.ToString ("D2"));
					sb.Append (':');
					sb.Append (minutes.ToString ("D2"));
					sb.Append (':');
					sb.Append (seconds.ToString ("D2"));
					sb.Append (decimal_separator);
					sb.Append (fractional.ToString ("D7"));
					break;
			}

			return sb.ToString ();
		}
#endif

		public static TimeSpan operator + (TimeSpan t1, TimeSpan t2)
		{
			return t1.Add (t2);
		}

		public static bool operator == (TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks == t2._ticks;
		}

		public static bool operator > (TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks > t2._ticks;
		}

		public static bool operator >= (TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks >= t2._ticks;
		}

		public static bool operator != (TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks != t2._ticks;
		}

		public static bool operator < (TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks < t2._ticks;
		}

		public static bool operator <= (TimeSpan t1, TimeSpan t2)
		{
			return t1._ticks <= t2._ticks;
		}

		public static TimeSpan operator - (TimeSpan t1, TimeSpan t2)
		{
			return t1.Subtract (t2);
		}

		public static TimeSpan operator - (TimeSpan t)
		{
			return t.Negate ();
		}

		public static TimeSpan operator + (TimeSpan t)
		{
			return t;
		}

		enum ParseError {
			None,
			Format,
			Overflow
		}

		// Class Parser implements parser for TimeSpan.Parse
		private class Parser
		{
			private string _src;
			private int _cur = 0;
			private int _length;
			ParseError parse_error;

			public Parser (string src)
			{
				_src = src;
				_length = _src.Length;
#if NET_4_0
				number_format = GetNumberFormatInfo (null);
#endif
			}

#if NET_4_0
			NumberFormatInfo number_format;

			public Parser (string src, IFormatProvider formatProvider) :
				this (src)
			{
				number_format = GetNumberFormatInfo (formatProvider);
			}

			NumberFormatInfo GetNumberFormatInfo (IFormatProvider formatProvider)
			{
				NumberFormatInfo format = null;
				if (formatProvider != null)
					format = (NumberFormatInfo) formatProvider.GetFormat (typeof (NumberFormatInfo));
				if (format == null)
					format = Thread.CurrentThread.CurrentCulture.NumberFormat;

				return format;
			}
#endif
	
			public bool AtEnd {
				get {
					return _cur >= _length;
				}
			}

			// All "Parse" functions throw a FormatException on syntax error.
			// Their return value is semantic value of the item parsed.

			// Range checking is spread over three different places:
			// 1) When parsing "int" values, an exception is thrown immediately
			//    when the value parsed exceeds the maximum value for an int.
			// 2) An explicit check is built in that checks for hours > 23 and
			//    for minutes and seconds > 59.
			// 3) Throwing an exceptions for a final TimeSpan value > MaxValue
			//    or < MinValue is left to the TimeSpan constructor called.

			// Parse zero or more whitespace chars.
			private void ParseWhiteSpace ()
			{
				while (!AtEnd && Char.IsWhiteSpace (_src, _cur)) {
					_cur++;
				}
			}

			// Parse optional sign character.
			private bool ParseSign ()
			{
				bool res = false;

				if (!AtEnd && _src[_cur] == '-') {
					res = true;
					_cur++;
				}

				return res;
			}

			// Parse simple int value
			private int ParseInt (bool optional)
			{
				if (optional && AtEnd)
					return 0;

				long res = 0;
				int count = 0;

				while (!AtEnd && Char.IsDigit (_src, _cur)) {
					res = res * 10 + _src[_cur] - '0';
					if (res > Int32.MaxValue) {
						SetParseError (ParseError.Overflow);
						break;
					}
					_cur++;
					count++;
				}

				if (!optional && (count == 0))
					SetParseError (ParseError.Format);

				return (int)res;
			}

			// Parse optional dot
			private bool ParseOptDot ()
			{
				if (AtEnd)
					return false;

				if (_src[_cur] == '.') {
					_cur++;
					return true;
				}
				return false;
			}	

#if NET_4_0
			// Just as ParseOptDot, but for decimal separator
			private bool ParseOptDecimalSeparator ()
			{
				if (AtEnd)
					return false;

				// we need to provide compatibility with old versions using '.'
				if (_src [_cur] == '.') {
					_cur++;
					return true;
				}

				string decimal_separator = number_format.NumberDecimalSeparator;
				if (String.Compare (_src, _cur, decimal_separator, 0, decimal_separator.Length) == 0) {
					_cur += decimal_separator.Length;
					return true;
				}

				return false;
			}
#endif

			private void ParseColon (bool optional)
			{
				if (!AtEnd) {
					if (_src[_cur] == ':')
						_cur++;
					else if (!optional)
						SetParseError (ParseError.Format);
				}
			}

			// Parse [1..7] digits, representing fractional seconds (ticks)
			private long ParseTicks ()
			{
				long mag = 1000000;
				long res = 0;
				bool digitseen = false;
				
				while (mag > 0 && !AtEnd && Char.IsDigit (_src, _cur)) {
					res = res + (_src[_cur] - '0') * mag;
					_cur++;
					mag = mag / 10;
					digitseen = true;
				}

				if (!digitseen)
					SetParseError (ParseError.Format);

				return res;
			}

			void SetParseError (ParseError error)
			{
				// We preserve the very first error.
				if (parse_error != ParseError.None)
					return;

				parse_error = error;
			}

			bool CheckParseSuccess (int hours, int minutes, int seconds, bool tryParse)
			{
				// FormatException has precedence over OverflowException starting with 4.0
				// so put the block before/after properly.
#if NET_4_0
				if (parse_error == ParseError.Format) {
					if (tryParse)
						return false;
					throw new FormatException (
						Locale.GetText ("Invalid format for TimeSpan.Parse."));
				}
#endif
				if (parse_error == ParseError.Overflow || hours > 23 || minutes > 59 || seconds > 59) {
					if (tryParse)
						return false;
					throw new OverflowException (
						Locale.GetText ("Invalid time data."));
				}
#if !NET_4_0
				// Respect the Overflow precedence for 2.0, putting the format check last.
				if (parse_error == ParseError.Format) {
					if (tryParse)
						return false;
					throw new FormatException (
						Locale.GetText ("Invalid format for TimeSpan.Parse."));
				}
#endif

				return true;
			}

			public bool Execute (bool tryParse, out TimeSpan result)
			{
				bool sign;
				int days;
				int hours = 0;
				int minutes;
				int seconds;
				long ticks;

				result = TimeSpan.Zero;

				// documented as...
				// Parse [ws][-][dd.]hh:mm:ss[.ff][ws]
				// ... but not entirely true as an lonely 
				// integer will be parsed as a number of days
				ParseWhiteSpace ();
				sign = ParseSign ();
				days = ParseInt (false);
				if (ParseOptDot ()) {
					hours = ParseInt (true);
				}
#if NET_4_0
				// if the value that was going to be used as 'hours' exceeds the range,
				// .net keeps it as days, even if there's a colon instead of a dot ahead
				else if (days > 23) {
					ParseColon (false);
					hours = ParseInt (true);
				}
#endif
				else if (!AtEnd) {
					hours = days;
					days = 0;
				}
				ParseColon(false);
				minutes = ParseInt (true);
				ParseColon (true);
				seconds = ParseInt (true);
#if NET_4_0
				if ( ParseOptDecimalSeparator () ) {
#else
				if ( ParseOptDot () ) {
#endif
					ticks = ParseTicks ();
				}
				else {
					ticks = 0;
				}
				ParseWhiteSpace ();
	
				if (!AtEnd)
					SetParseError (ParseError.Format);

				if (!CheckParseSuccess (hours, minutes, seconds, tryParse))
					return false;

				long t;
				if (!TimeSpan.CalculateTicks (days, hours, minutes, seconds, 0, false, out t))
					return false;

				try {
					t = checked ((sign) ? (-t - ticks) : (t + ticks));
				} catch (OverflowException) {
					if (tryParse)
						return false;
					throw;
				}

				result = new TimeSpan (t);
				return true;
			}
		}
	}
}
