//
// System.DateTime.cs
//
// author:
//   Marcel Narings (marcel@narings.nl)
//   Martin Baulig (martin@gnome.org)
//
//   (C) 2001 Marcel Narings

using System;
using System.Globalization;
using System.Runtime.CompilerServices;


namespace System
{
	/// <summary>
	/// The DateTime structure represents dates and time ranging from
	/// 1-1-0001 12:00:00 AM to 31-12-9999 23:59:00 Common Era.
	/// </summary>
	/// 
	public struct DateTime : IComparable , IFormattable  , IConvertible
	{
		private TimeSpan ticks;

		private const int dp400 = 146097;
		private const int dp100 = 36524;
		private const int dp4 = 1461;

		// w32 file time starts counting from 1/1/1601 00:00 GMT
		// which is the constant ticks from the .NET epoch
		private const long w32file_epoch = 504911232000000000L;

		//
		// The UnixEpoch, it begins on Jan 1, 1970 at 0:0:0, expressed
		// in Ticks
		//
		internal const long UnixEpoch = 621355968000000000L;
		
		public static readonly DateTime MaxValue = new DateTime (false,TimeSpan.MaxValue);
		public static readonly DateTime MinValue = new DateTime (false,TimeSpan.MinValue);
		
		private enum Which 
		{
			Day,
			DayYear,
			Month,
			Year
		};
	
		private static int[] daysmonth = { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };	
		private static int[] daysmonthleap = { 0, 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };	

		private static int AbsoluteDays (int year, int month, int day)
		{
			int[] days;
			int temp = 0, m=1 ;
		
			days = (IsLeapYear(year) ? daysmonthleap  : daysmonth);
			
			while (m < month)
				temp += days[m++];
			return ((day-1) + temp + (365* (year-1)) + ((year-1)/4) - ((year-1)/100) + ((year-1)/400));
		}

		private int FromTicks(Which what)
		{
			int num400, num100, num4, numyears; 
			int M =1;

			int[] days = daysmonth;
			int totaldays = this.ticks.Days;

			num400 = (totaldays / dp400);
			totaldays -=  num400 * dp400;
		
			num100 = (totaldays / dp100);
			if (num100 == 4)   // leap
				num100 = 3;
			totaldays -= (num100 * dp100);

			num4 = totaldays / dp4;
			totaldays -= (num4 * dp4);

			numyears = totaldays / 365 ;

			if (numyears == 4)  //leap
				numyears =3 ;
			if (what == Which.Year )
				return num400*400 + num100*100 + num4*4 + numyears + 1;

			totaldays -= (numyears * 365) ;
			if (what == Which.DayYear )
				return totaldays + 1;
			
			if  ((numyears==3) && ((num100 == 3) || !(num4 == 24)) ) //31 dec leapyear
				days = daysmonthleap;
			        
			while (totaldays >= days[M])
				totaldays -= days[M++];

			if (what == Which.Month )
				return M;

			return totaldays +1; 
		}


		// Constructors
		
		/// <summary>
		/// Constructs a DateTime for specified ticks
		/// </summary>
		/// 
		public DateTime (long newticks)
			// `local' must default to false here to avoid
			// a recursion loop.
			: this (false, newticks) {}

		internal DateTime (bool local, long newticks)
			: this (true, new TimeSpan (newticks))
		{
			if (local) {
				TimeZone tz = TimeZone.CurrentTimeZone;

				TimeSpan utcoffset = tz.GetUtcOffset (this);

				ticks = ticks + utcoffset;
			}
		}

		public DateTime (int year, int month, int day)
			: this (year, month, day,0,0,0,0) {}

		public DateTime (int year, int month, int day, int hour, int minute, int second)
			: this (year, month, day, hour, minute, second, 0)	{}

		public DateTime (int year, int month, int day, int hour, int minute, int second, int millisecond)
			{
			if ( year < 1 || year > 9999 || 
				month < 1 || month >12  ||
				day < 1 || day > DaysInMonth(year, month) ||
				hour < 0 || hour > 23 ||
				minute < 0 || minute > 59 ||
				second < 0 || second > 59 )
				throw new ArgumentOutOfRangeException() ;

			ticks = new TimeSpan (AbsoluteDays(year,month,day), hour, minute, second, millisecond);
		}

		public DateTime (int year, int month, int day, Calendar calendar)
			: this (year, month, day, 0, 0, 0, 0, calendar)	{}

		
		public DateTime (int year, int month, int day, int hour, int minute, int second, Calendar calendar)
			: this (year, month, day, hour, minute, second, 0, calendar)	{}


		public DateTime (int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar)
			: this (year, month, day, hour, minute, second, millisecond) 
		{
			if (calendar == null)
				throw new ArgumentNullException();
		}

		internal DateTime (bool check, TimeSpan value)
		{
			if (check && (value.Ticks < MinValue.Ticks || value.Ticks > MaxValue.Ticks))
				throw new ArgumentOutOfRangeException ();

			ticks = value;
		}

		/* Properties  */
		 
		public DateTime Date 
		{
			get	
			{ 
				return new DateTime (Year, Month, Day);
			}
		}
        
		public int Month 
		{
			get	
			{ 
				return FromTicks(Which.Month); 
			}
		}

	       
		public int Day
		{
			get 
			{ 
				return FromTicks(Which.Day); 
			}
		}

		public DayOfWeek DayOfWeek 
		{
			get 
			{ 
				return ( (DayOfWeek) ((ticks.Days+1) % 7) ); 
			}
		}

		public int DayOfYear 
		{
			get 
			{ 
				return FromTicks(Which.DayYear); 
			}
		}

		public TimeSpan TimeOfDay 
		{
			get	
			{ 
				return new TimeSpan(ticks.Ticks % TimeSpan.TicksPerDay );
			}
			
		}

		public int Hour 
		{
			get 
			{ 
				return ticks.Hours;
			}
		}

		public int Minute 
		{
			get 
			{ 
				return ticks.Minutes;
			}
		}

		public int Second 
		{
			get	
			{ 
				return ticks.Seconds;
			}
		}

		public int Millisecond 
		{
			get 
			{ 
				return ticks.Milliseconds;
			}
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern long GetNow ();

		public static DateTime Now 
		{
			get	
			{
				return new DateTime (true, GetNow ());
			}
		}

		public long Ticks
		{ 
			get	
			{ 
				return ticks.Ticks;
			}
		}
	
		public static DateTime Today 
		{
			get	
			{
				return new DateTime (true, (GetNow () / TimeSpan.TicksPerDay) * TimeSpan.TicksPerDay);
			}
		}

		public static DateTime UtcNow 
		{
			get {
				return new DateTime (GetNow ());
			}
		}

		public int Year 
		{
			get 
			{ 
				return FromTicks(Which.Year); 
			}
		}

		/* methods */

		public DateTime Add (TimeSpan ts)
		{
			return new DateTime (true, ticks) + ts;
		}

		public DateTime AddDays (double days)
		{
			return AddMilliseconds (days * 86400000);
		}
		
		public DateTime AddTicks (long t)
		{
			return Add (new TimeSpan (t));
		}

		public DateTime AddHours (double hours)
		{
			return AddMilliseconds (hours * 3600000);
		}

		public DateTime AddMilliseconds (double ms)
		{
			long msticks;
			
			msticks = (long) (ms += ms > 0 ? 0.5 : -0.5) * TimeSpan.TicksPerMillisecond ; 

			return AddTicks (msticks);
		}

		public DateTime AddMinutes (double minutes)
		{
			return AddMilliseconds (minutes * 60000);
		}
		
		public DateTime AddMonths (int months)
		{
			int day, month, year,  maxday ;
			DateTime temp ;

			day = this.Day;
			month = this.Month + (months % 12);
			year = this.Year + months/12 ;
			
			if (month < 1)
			{
				month = 12 + month ;
				year -- ;
			}
			else if (month>12) 
			{
				month = month -12;
				year ++;
			}
			maxday = DaysInMonth(year, month);
			if (day > maxday)
				day = maxday;

			temp = new DateTime (year, month, day);
			return  temp.Add (this.TimeOfDay);
		}

		public DateTime AddSeconds (double seconds)
		{
			return AddMilliseconds (seconds*1000);
		}

		public DateTime AddYears (int years )
		{
			return AddMonths(years * 12);
		}

		public static int Compare (DateTime t1,	DateTime t2)
		{
			if (t1.ticks < t2.ticks) 
				return -1;
			else if (t1.ticks > t2.ticks) 
				return 1;
			else
				return 0;
		}

		public int CompareTo (object v)
		{
			if ( v == null)
				return 1;

			if (!(v is System.DateTime))
				throw new ArgumentException (Locale.GetText (
					"Value is not a System.DateTime"));

			return Compare (this, (DateTime) v);
		}

		public static int DaysInMonth (int year, int month)
		{
			int[] days ;

			if (month < 1 || month >12)
				throw new ArgumentOutOfRangeException ();

			days = (IsLeapYear(year) ? daysmonthleap  : daysmonth);
			return days[month];			
		}
		
		public override bool Equals (object o)
		{
			if (!(o is System.DateTime))
				return false;

			return ((DateTime) o).ticks == ticks;
		}

		public static bool Equals (DateTime t1, DateTime t2 )
		{
			return (t1.ticks == t2.ticks );
		}

		public static DateTime FromFileTime (long fileTime) 
		{
			return new DateTime (w32file_epoch + fileTime);
		}

		// TODO: Implement me.
		[MonoTODO]
		public static DateTime FromOADate (double d)
		{
				return new DateTime(0);
		}
		
		// TODO: Implement me.
		[MonoTODO]
		public string[] GetDateTimeFormats() 
		{
			return null;
		}

		//TODO: implement me
		[MonoTODO]
		public string[] GetDateTimeFormats(char format)
		{
			return null;
		}
		
		// TODO: implement me
		[MonoTODO]
		public string[] GetDateTimeFormats(IFormatProvider provider)
		{
			return null;
		}

		//TODO: implement me 
		[MonoTODO]
		public string[] GetDateTimeFormats(char format,IFormatProvider provider	)
		{
			return null;
		}

		public override int GetHashCode ()
		{
			return (int) ticks.Ticks;
		}

		public TypeCode GetTypeCode ()
		{
			return TypeCode.DateTime;
		}

		public static bool IsLeapYear (int year)
		{
			return  ( (year % 4 == 0 && year % 100 != 0) || year % 400 == 0) ;
		}

		[MonoTODO]
		public static DateTime Parse (string s)
		{
			// TODO: Implement me
			return new DateTime (0);
		}

		[MonoTODO]
		public static DateTime Parse (string s, IFormatProvider fp)
		{
			// TODO: Implement me
			return new DateTime (0);
		}

		[MonoTODO]
		public static DateTime Parse (string s, NumberStyles style, IFormatProvider fp)
		{
			// TODO: Implement me
			return new DateTime (0);
		}

		[MonoTODO]
		public static DateTime ParseExact (string s, string format, IFormatProvider fp)
		{
			return ParseExact (s, format, fp, DateTimeStyles.None);
		}

		internal static int _ParseNumber (string s, int digits, bool leadingzero, out int num_parsed)
		{
			int number = 0, i;

			if (!leadingzero)
			{
				int real_digits = 0;
				for (i = 0; i < digits; i++)
				{
					if (!Char.IsDigit (s[i]))
						break;

					real_digits++;
				}

				digits = real_digits;
			}

			for (i = 0; i < digits; i++)
			{
				char c = s[i];
				if (!Char.IsDigit (c))
				{
					num_parsed = -1;
					return 0;
				}

				number = number * 10 + (byte) (c - '0');
			}

			num_parsed = digits;
			return number;
		}

		internal static int _ParseEnum (string s, string[] values, out int num_parsed)
		{
			int i;

			for (i = 0; i < values.Length; i++)
			{
				String tmp = s.Substring (0, values[i].Length);
				if (String.Compare (tmp, values[i], true) == 0)
				{
					num_parsed = values[i].Length;
					return i;
				}
			}

			num_parsed = -1;
			return -1;
		}

		internal static bool _ParseString (string s, int maxlength, string value, out int num_parsed)
		{
			if (maxlength > 0)
				value = value.Substring (0, maxlength);

			s = s.Substring (0, value.Length);

			if (String.Compare (s, value, true) == 0)
			{
				num_parsed = value.Length;
				return true;
			}

			num_parsed = -1;
			return false;
		}

		internal static bool _DoParse (string s, string format, bool exact,
					       out DateTime result,
					       DateTimeFormatInfo dfi,
					       DateTimeStyles style)
		{
			bool useutc = false;

			if (format.Length == 1)
				format = _GetStandardPattern (format[0], dfi, out useutc);

			char[] chars = format.ToCharArray ();
			int len = format.Length, pos = 0, num = 0;

			int day = -1, dayofweek = -1, month = -1, year = -1;
			int hour = -1, minute = -1, second = -1, millisecond = -1;
			int ampm = -1, century = (Now.Year / 100) * 100;
			int tzsign = -1, tzoffset = -1, tzoffmin = -1;

			result = new DateTime (0);

			while (pos+num < len)
			{
				if (chars[pos] == '\'')
				{
					num = 1;
					s = s.Substring (1);
					while (pos+num < len)
					{
						if (s[0] != chars[pos+num])
							return false;
						s = s.Substring (1);
						if (s.Length == 0)
							return false;

						if (chars[pos+num] == '\'')
							break;

						num = num + 1;
					}
					if (pos+num > len)
						return false;

					pos = pos + num + 1;
					num = 0;
					continue;
				}
				else if (chars[pos] == '\\')
				{
					if (pos+1 >= len)
						return false;

					if (s[0] != chars[pos+num])
						return false;
					s = s.Substring (1);
					if (s.Length == 0)
						return false;

					pos = pos + 1;
					continue;
				}
				else if (chars[pos] == '%')
				{
					pos = pos + 1;
					continue;
				}

				if ((pos+num+1 < len) && (chars[pos+num+1] == chars[pos+num]))
				{
					num = num + 1;
					continue;
				}

				int num_parsed = 0;

				switch (chars[pos])
				{
				case 'd':
					if (day != -1)
						return false;
					if (num == 0)
						day = _ParseNumber (s, 2, false, out num_parsed);
					else if (num == 1)
						day = _ParseNumber (s, 2, true, out num_parsed);
					else if (num == 2)
						dayofweek = _ParseEnum (s, dfi.AbbreviatedDayNames, out num_parsed);
					else
					{
						dayofweek = _ParseEnum (s, dfi.DayNames, out num_parsed);
						num = 3;
					}
					break;
				case 'M':
					if (month != -1)
						return false;
					if (num == 0)
						month = _ParseNumber (s, 2, false, out num_parsed);
					else if (num == 1)
						month = _ParseNumber (s, 2, false, out num_parsed);
					else if (num == 2)
						month = _ParseEnum (s, dfi.AbbreviatedMonthNames , out num_parsed);
					else
					{
						month = _ParseEnum (s, dfi.MonthNames, out num_parsed);
						num = 3;
					}
					if ((month < 1) || (month > 12))
						return false;
					break;
				case 'y':
					if (year != -1)
						return false;
					if (num == 0)
						year = _ParseNumber (s, 2, false, out num_parsed) + century;
					else if (num < 3)
						year = _ParseNumber (s, 2, true, out num_parsed) + century;
					else
					{
						year = _ParseNumber (s, 4, false, out num_parsed);
						num = 3;
					}
					break;
				case 'h':
					if (hour != -1)
						return false;
					if (num == 0)
						hour = _ParseNumber (s, 2, false, out num_parsed);
					else
					{
						hour = _ParseNumber (s, 2, true, out num_parsed);
						num = 1;
					}
					if (hour >= 12)
						return false;
					break;
				case 'H':
					if ((hour != -1) || (ampm >= 0))
						return false;
					if (num == 0)
						hour = _ParseNumber (s, 2, false, out num_parsed);
					else
					{
						hour = _ParseNumber (s, 2, true, out num_parsed);
						num = 1;
					}
					if (hour >= 24)
						return false;
					ampm = -2;
					break;
				case 'm':
					if (minute != -1)
						return false;
					if (num == 0)
						minute = _ParseNumber (s, 2, false, out num_parsed);
					else
					{
						minute = _ParseNumber (s, 2, true, out num_parsed);
						num = 1;
					}
					if (minute >= 60)
						return false;
					break;
				case 's':
					if (second != -1)
						throw new FormatException ();
					if (num == 0)
						second = _ParseNumber (s, 2, false, out num_parsed);
					else
					{
						second = _ParseNumber (s, 2, true, out num_parsed);
						num = 1;
					}
					if (second >= 60)
						throw new FormatException (Locale.GetText ("The DateTime represented by the string is out of range."));
					break;
				case 'f':
					if (millisecond != -1)
						throw new FormatException ();
					num = Math.Min (num, 6);
					millisecond = _ParseNumber (s, num+1, true, out num_parsed);
					break;
				case 't':
					if (ampm != -1)
						throw new FormatException ();
					if (num == 0)
					{
						if (_ParseString (s, 1, dfi.AMDesignator, out num_parsed))
							ampm = 0;
						else if (_ParseString (s, 1, dfi.PMDesignator, out num_parsed))
							ampm = 1;
						else
							throw new FormatException ();
					}
					else
					{
						if (_ParseString (s, 0, dfi.AMDesignator, out num_parsed))
							ampm = 0;
						else if (_ParseString (s, 0, dfi.PMDesignator, out num_parsed))
							ampm = 1;
						else
							throw new FormatException ();
						num = 1;
					}
					break;
				case 'z':
					if (tzsign != -1)
						throw new FormatException ();
					if (s[0] == '+')
						tzsign = 0;
					else if (s[0] == '-')
						tzsign = 1;
					else
						throw new FormatException ();
					s = s.Substring (1);
					if (num == 0)
						tzoffset = _ParseNumber (s, 2, false, out num_parsed);
					else if (num == 1)
						tzoffset = _ParseNumber (s, 2, true, out num_parsed);
					else
					{
						tzoffset = _ParseNumber (s, 2, true, out num_parsed);
						if (num_parsed < 0)
							throw new FormatException ();
						s = s.Substring (num_parsed);
						if (!_ParseString (s, 0, dfi.TimeSeparator, out num_parsed))
							throw new FormatException ();
						s = s.Substring (num_parsed);
						tzoffmin = _ParseNumber (s, 2, true, out num_parsed);
						if (num_parsed < 0)
							throw new FormatException ();
						num = 2;
					}
					break;
				case ':':
					if (!_ParseString (s, 0, dfi.TimeSeparator, out num_parsed))
						throw new FormatException ();
					break;
				case '/':
					if (!_ParseString (s, 0, dfi.DateSeparator, out num_parsed))
						throw new FormatException ();
					break;
				default:
					if (s[0] != chars[pos])
						throw new FormatException ();
					num = 0;
					num_parsed = 1;
					break;
				}

				if (num_parsed < 0)
					throw new FormatException ();

				s = s.Substring (num_parsed);

				pos = pos + num + 1;
				num = 0;
			}

			if (hour == -1)
				hour = 0;
			if (minute == -1)
				minute = 0;
			if (second == -1)
				second = 0;
			if (millisecond == -1)
				millisecond = 0;
			if (day == -1)
				day = 0;
			if (month == -1)
				month = 0;
			if (year == -1)
				year = 0;

			if (ampm == 1)
				hour = hour + 12;

			if (tzoffmin == -1)
				tzoffmin = 0;
			if (tzoffset == -1)
				tzoffset = 0;
			if (tzsign == 1)
				tzoffset = -tzoffset;

			TimeSpan utcoffset = new TimeSpan (tzoffset, tzoffmin, 0);

			result = new DateTime (year, month, day-1, hour, minute, second, millisecond);

			if ((dayofweek != -1) && (dayofweek != (int) result.DayOfWeek))
				throw new FormatException (Locale.GetText ("String was not recognized as valid DateTime because the day of week was incorrect."));

			long newticks = (result.ticks - utcoffset).Ticks;

			result = new DateTime (true, newticks);

			return true;
		}


		[MonoTODO]
		public static DateTime ParseExact (string s, string format,
						   IFormatProvider fp, DateTimeStyles style)
		{
			string[] formats;

			formats = new string [1];
			formats[0] = format;

			return ParseExact (s, formats, fp, style);
		}

		[MonoTODO]
		public static DateTime ParseExact (string s, string[] formats,
						   IFormatProvider fp,
						   DateTimeStyles style)
		{
			DateTimeFormatInfo dfi = DateTimeFormatInfo.GetInstance (fp);

			if (s == null)
				throw new ArgumentNullException (Locale.GetText ("s is null"));
			if (formats.Length == 0)
				throw new ArgumentNullException (Locale.GetText ("format is null"));

			int i;
			for (i = 0; i < formats.Length; i++)
			{
				DateTime result;

				if (_DoParse (s, formats[i], true, out result, dfi, style))
					return result;
			}

			throw new FormatException ();
		}
		
		public TimeSpan Subtract(DateTime dt)
		{   
			return new TimeSpan(ticks.Ticks) - dt.ticks;
		}

		public DateTime Subtract(TimeSpan ts)
		{
			TimeSpan newticks;

			newticks = (new TimeSpan (ticks.Ticks)) - ts;
			return new DateTime(true,newticks);
		}

		public long ToFileTime()
		{
			if(ticks.Ticks < w32file_epoch) {
				throw new ArgumentOutOfRangeException("file time is not valid");
			}
			
			return(ticks.Ticks - w32file_epoch);
		}

		public string ToLongDateString()
		{
			return ToString ("D");
		}

		public string ToLongTimeString()
		{
			return ToString ("T");
		}

		[MonoTODO]
		public double ToOADate()
		{
			// TODO implement me 
			return 0;
		}

		public string ToShortDateString()
		{
			return ToString ("d");
		}

		public string ToShortTimeString()
		{
			return ToString ("t");
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

		internal static string _GetStandardPattern (char format, DateTimeFormatInfo dfi, out bool useutc)
		{
			String pattern;

			useutc = false;

			switch (format)
			{
			case 'd':
				pattern = dfi.ShortDatePattern;
				break;
			case 'D':
				pattern = dfi.LongDatePattern;
				break;
			case 'f':
				pattern = dfi.LongDatePattern + " " + dfi.ShortTimePattern;
				break;
			case 'F':
				pattern = dfi.FullDateTimePattern;
				break;
			case 'g':
				pattern = dfi.ShortDatePattern + " " + dfi.ShortTimePattern;
				break;
			case 'G':
				pattern = dfi.ShortDatePattern + " " + dfi.LongTimePattern;
				break;
			case 'm':
			case 'M':
				pattern = dfi.MonthDayPattern;
				break;
			case 'r':
			case 'R':
				pattern = dfi.RFC1123Pattern;
				break;
			case 's':
				pattern = dfi.SortableDateTimePattern;
				break;
			case 't':
				pattern = dfi.ShortTimePattern;
				break;
			case 'T':
				pattern = dfi.LongTimePattern;
				break;
			case 'u':
				pattern = dfi.UniversalSortableDateTimePattern;
				useutc = true;
				break;
			case 'U':
				pattern = dfi.LongDatePattern + " " + dfi.LongTimePattern;
				useutc = true;
				break;
			case 'y':
			case 'Y':
				pattern = dfi.YearMonthPattern;
				break;
			default:
				pattern = null;
				break;
			}

			return pattern;
		}

		internal string _ToString (string format, DateTimeFormatInfo dfi)
		{
			String str = null, result = null;
			char[] chars = format.ToCharArray ();
			int len = format.Length, pos = 0, num = 0;

			TimeZone tz = TimeZone.CurrentTimeZone;
			TimeSpan utcoffset = tz.GetUtcOffset (this);

			while (pos < len)
			{
				if (chars[pos] == '\'') {
					num = 1;
					while (pos+num <= len) {
						if (chars[pos+num] == '\'')
							break;

						result += chars[pos+num];
						num++;
					}
					if (pos+num > len)
						throw new FormatException (Locale.GetText ("The specified format is invalid"));

					pos += num+1;
					num = 0;
					continue;
				} else if (chars[pos] == '\\') {
					if (pos+1 >= len)
						throw new FormatException (Locale.GetText ("The specified format is invalid"));

					result += chars[pos+1];
					pos += 2;
					continue;
				} else if (chars[pos] == '%') {
					pos++;
					continue;
				}

				if ((pos+num+1 < len) && (chars[pos+num+1] == chars[pos+num])) {
					num++;
					continue;
				}

				switch (chars[pos])
				{
				case 'd':
					if (num == 0)
						str = Day.ToString ("d");
					else if (num == 1)
						str = Day.ToString ("d02");
					else if (num == 2)
						str = dfi.GetAbbreviatedDayName (DayOfWeek);
					else {
						str = dfi.GetDayName (DayOfWeek);
						num = 3;
					}
					break;
				case 'M':
					if (num == 0)
						str = Month.ToString ("d");
					else if (num == 1)
						str = Month.ToString ("d02");
					else if (num == 2)
						str = dfi.GetAbbreviatedMonthName (Month);
					else {
						str = dfi.GetMonthName (Month);
						num = 3;
					}
					break;
				case 'y':
					if (num == 0) {
						int shortyear = Year % 100;
						str = shortyear.ToString ("d");
					} else if (num == 1) {
						int shortyear = Year % 100;
						str = shortyear.ToString ("d02");
					} else {
						str = Year.ToString ("d");
						num = 3;
					}
					break;
				case 'g':
					// FIXME
					break;
				case 'f':
					num = Math.Min (num, 6);

					long ms = (long) Millisecond;
					long exp = 10;
					for (int i = 0; i < num; i++)
						exp = exp * 10;
					long maxexp = TimeSpan.TicksPerMillisecond;

					exp = Math.Min (exp, maxexp);
					ms = ms * exp / maxexp;

					String prec = (num+1).ToString ("d02");
					str = ms.ToString (String.Concat ("d", prec));

					break;
				case 'h':
					if (num == 0) {
						int shorthour = Hour % 12;
						str = shorthour.ToString ("d");
					} else {
						int shorthour = Hour % 12;
						str = shorthour.ToString ("d02");
						num = 1;
					}
					break;
				case 'H':
					if (num == 0)
						str = Hour.ToString ("d");
					else {
						str = Hour.ToString ("d02");
						num = 1;
					}
					break;
				case 'm':
					if (num == 0)
						str = Minute.ToString ("d");
					else {
						str = Minute.ToString ("d02");
						num = 1;
					}
					break;
				case 's':
					if (num == 0)
						str = Second.ToString ("d");
					else {
						str = Second.ToString ("d02");
						num = 1;
					}
					break;
				case 't':
					if (Hour < 12)
						str = dfi.AMDesignator;
					else
						str = dfi.PMDesignator;

					if (num == 0)
						str = str.Substring (0,1);
					else
						num = 1;
					break;
				case 'z':
					if (num == 0) {
						int offset = utcoffset.Hours;
						str = offset.ToString ("d");
						if (offset > 0)
							str = String.Concat ("+", str);
					} else if (num == 1) {
						int offset = utcoffset.Hours;
						str = offset.ToString ("d02");
						if (offset > 0)
							str = String.Concat ("+", str);
					} else if (num == 2) {
						int offhour = utcoffset.Hours;
						int offminute = utcoffset.Minutes;
						str = offhour.ToString ("d02");
						str = String.Concat (str, dfi.TimeSeparator);
						str = String.Concat (str, offminute.ToString ("d02"));
						if (offhour > 0)
							str = String.Concat ("+", str);
						num = 2;
					}
					break;
				case ':':
					str = dfi.TimeSeparator;
					num = 0;
					break;
				case '/':
					str = dfi.DateSeparator;
					num = 0;
					break;
				default:
					str = String.Concat (chars [pos]);
					num = 0;
					break;
				}

				result = String.Concat (result, str);
						
				pos += num + 1;
				num = 0;
			}

			return result;
		}

		public string ToString (string format, IFormatProvider fp)
		{
			DateTimeFormatInfo dfi = DateTimeFormatInfo.GetInstance(fp);

			if (format == null)
				format = dfi.FullDateTimePattern;

			bool useutc = false;

			if (format.Length == 1) {
				char fchar = (format.ToCharArray ())[0];
				format = _GetStandardPattern (fchar, dfi, out useutc);
			}

			if (useutc)
				return this.ToUniversalTime ()._ToString (format, dfi);
			else
				return this._ToString (format, dfi);
		}

		public DateTime ToLocalTime()
		{
			TimeZone tz = TimeZone.CurrentTimeZone;

			TimeSpan offset = tz.GetUtcOffset (this);

			return new DateTime (true, ticks + offset);
		}

		public DateTime ToUniversalTime()
		{
			TimeZone tz = TimeZone.CurrentTimeZone;

			TimeSpan offset = tz.GetUtcOffset (this);

			return new DateTime (true, ticks - offset);
		}

		/*  OPERATORS */

		public static DateTime operator +(DateTime d, TimeSpan t)
		{
			return new DateTime (true, d.ticks + t);
		}

		public static bool operator ==(DateTime d1, DateTime d2)
		{
			return (d1.ticks == d2.ticks);
		}

		public static bool operator >(DateTime t1,DateTime t2)
		{
			return (t1.ticks > t2.ticks);
		}

		public static bool operator >=(DateTime t1,DateTime t2)
		{
			return (t1.ticks >= t2.ticks);
		}

		public static bool operator !=(DateTime d1, DateTime d2)
		{
			return (d1.ticks != d2.ticks);
		}

		public static bool operator <(DateTime t1,	DateTime t2)
		{
			return (t1.ticks < t2.ticks );
		}

		public static bool operator <=(DateTime t1,DateTime t2)
		{
			return (t1.ticks <= t2.ticks);
		}

		public static TimeSpan operator -(DateTime d1,DateTime d2)
		{
			return new TimeSpan((d1.ticks - d2.ticks).Ticks);
		}

		public static DateTime operator -(DateTime d,TimeSpan t)
		{
			return new DateTime (true, d.ticks - t);
		}

		public bool ToBoolean(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		public byte ToByte(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		public char ToChar(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		// TODO Implement me
		[MonoTODO]
		public System.DateTime ToDateTime(IFormatProvider provider)
		{
			return new System.DateTime(true,this.ticks);
		} 
		
		public decimal ToDecimal(IFormatProvider provider)
		{
			 throw new InvalidCastException();
		}

		public double ToDouble(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		public Int16 ToInt16(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		public Int32 ToInt32(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		public Int64 ToInt64(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		[CLSCompliant(false)]
		public SByte ToSByte(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		public Single ToSingle(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		public object ToType(Type conversionType,IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		UInt16 System.IConvertible.ToUInt16(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		[CLSCompliant(false)]
		public UInt32 ToUInt32(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}

		[CLSCompliant(false)]
		public UInt64 ToUInt64(IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
	}
}

namespace System
{
	public enum DayOfWeek
	{
		Sunday,
		Monday,
		Tuesday,
		Wednesday,
		Thursday,
		Friday,
		Saturday
	}
}
