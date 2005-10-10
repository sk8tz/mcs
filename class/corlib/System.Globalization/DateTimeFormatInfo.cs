// System.Globalization.DateTimeFormatInfo
//
// Some useful functions are missing in the ECMA specs.
// They have been added following MS SDK Beta2
//
// Martin Weindel (martin.weindel@t-online.de)
//
// (C) Martin Weindel (martin.weindel@t-online.de)

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.Threading;

namespace System.Globalization
{
	[Serializable]
	[MonoTODO ("Fix serialization compatibility with MS.NET")]
	public sealed class DateTimeFormatInfo : ICloneable, IFormatProvider {
		private static readonly string MSG_READONLY = "This instance is read only";
		private static readonly string MSG_ARRAYSIZE_MONTH = "An array with exactly 13 elements is needed";
		private static readonly string MSG_ARRAYSIZE_DAY = "An array with exactly 7 elements is needed";
		private static readonly string[] INVARIANT_ABBREVIATED_DAY_NAMES
			= new string[7] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"};
		private static readonly string[] INVARIANT_DAY_NAMES
			= new string[7] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"};
		private static readonly string[] INVARIANT_ABBREVIATED_MONTH_NAMES
			= new string[13] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", ""};
		private static readonly string[] INVARIANT_MONTH_NAMES
			= new string[13] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December", ""};
		private static readonly string[] INVARIANT_ERA_NAMES = {"A.D."};

		private static DateTimeFormatInfo theInvariantDateTimeFormatInfo;

		private bool readOnly;
		private string _AMDesignator;
		private string _PMDesignator;
		private string _DateSeparator;
		private string _TimeSeparator;
		private string _ShortDatePattern;
		private string _LongDatePattern;
		private string _ShortTimePattern;
		private string _LongTimePattern;
		private string _MonthDayPattern;
		private string _YearMonthPattern;
		private string _FullDateTimePattern;
		private string _RFC1123Pattern;
		private string _SortableDateTimePattern;
		private string _UniversalSortableDateTimePattern;
		private DayOfWeek _FirstDayOfWeek;
		private Calendar _Calendar;
		private CalendarWeekRule _CalendarWeekRule;
		private string[] _AbbreviatedDayNames;
		private string[] _DayNames;
		private string[] _MonthNames;
		private string[] _AbbreviatedMonthNames;

		// FIXME: not supported other than invariant
		private string [] _ShortDatePatterns;
		private string [] _LongDatePatterns;
		private string [] _ShortTimePatterns;
		private string [] _LongTimePatterns;
		private string [] _MonthDayPatterns;
		private string [] _YearMonthPatterns;

		public DateTimeFormatInfo()
		{
			readOnly = false;
			_AMDesignator = "AM";
			_PMDesignator = "PM";
			_DateSeparator = "/";
			_TimeSeparator = ":";
			_ShortDatePattern = "MM/dd/yyyy";
			_LongDatePattern = "dddd, dd MMMM yyyy";
			_ShortTimePattern = "HH:mm";
			_LongTimePattern = "HH:mm:ss";
			_MonthDayPattern = "MMMM dd";
			_YearMonthPattern = "yyyy MMMM";
			_FullDateTimePattern = "dddd, dd MMMM yyyy HH:mm:ss";

			// FIXME: for the following three pattern: "The
			// default value of this property is derived
			// from the calendar that is set for
			// CultureInfo.CurrentCulture or the default
			// calendar of CultureInfo.CurrentCulture."

			// Actually, no predefined culture has different values
			// than those default values.

			_RFC1123Pattern = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'"; 
			_SortableDateTimePattern = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";
			_UniversalSortableDateTimePattern = "yyyy'-'MM'-'dd HH':'mm':'ss'Z'";

			_FirstDayOfWeek = DayOfWeek.Sunday;
			_Calendar = new GregorianCalendar();
			_CalendarWeekRule = CalendarWeekRule.FirstDay;

			_AbbreviatedDayNames = INVARIANT_ABBREVIATED_DAY_NAMES;
			_DayNames = INVARIANT_DAY_NAMES;
			_AbbreviatedMonthNames = INVARIANT_ABBREVIATED_MONTH_NAMES;
			_MonthNames = INVARIANT_MONTH_NAMES;
		}
				
		// LAMESPEC: this is not in ECMA specs
		public static DateTimeFormatInfo GetInstance(IFormatProvider provider)
		{
			if (provider != null) {
				DateTimeFormatInfo dtfi;
				dtfi = (DateTimeFormatInfo)provider.GetFormat(typeof(DateTimeFormatInfo));
				if (dtfi != null)
					return dtfi;
			}
			
			return CurrentInfo;
		}

		public bool IsReadOnly {
			get {
				return readOnly;
			}
		}

		public static DateTimeFormatInfo ReadOnly(DateTimeFormatInfo dtfi)
		{
			DateTimeFormatInfo copy = (DateTimeFormatInfo)dtfi.Clone();
			copy.readOnly = true;
			return copy;
		}			

		public object Clone () 
		{
			DateTimeFormatInfo clone = (DateTimeFormatInfo) MemberwiseClone();
			// clone is not read only
			clone.readOnly = false;
			return clone;
		}

		public object GetFormat(Type formatType)
		{
			return (formatType == GetType()) ? this : null;
		}

		public string GetAbbreviatedEraName (int era)
		{
			if (era < 0 || era >= _Calendar.AbbreviatedEraNames.Length)
				throw new ArgumentOutOfRangeException ("era", era.ToString ());
			return _Calendar.AbbreviatedEraNames [era];
		}

		public string GetAbbreviatedMonthName(int month)
		{
			if (month < 1 || month > 13) throw new ArgumentOutOfRangeException();
			return _AbbreviatedMonthNames[month-1];
		}

		public int GetEra (string eraName)
		{
			if (eraName == null)
				throw new ArgumentNullException ();
			string [] eras = _Calendar.EraNames;
			for (int i = 0; i < eras.Length; i++)
				if (CultureInfo.InvariantCulture.CompareInfo
					.Compare (eraName, eras [i],
					CompareOptions.IgnoreCase) == 0)
					return i;
			return -1;
		}

		public string GetEraName (int era)
		{
			if (era < 0 || era > _Calendar.EraNames.Length)
				throw new ArgumentOutOfRangeException ("era", era.ToString ());
			return _Calendar.EraNames [era - 1];
		}

		public string GetMonthName(int month)
		{
			if (month < 1 || month > 13) throw new ArgumentOutOfRangeException();
			return _MonthNames[month-1];
		}

		public string[] AbbreviatedDayNames
		{
			get
			{
				return (string[]) _AbbreviatedDayNames.Clone();
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				if (value.GetLength(0) != 7) throw new ArgumentException(MSG_ARRAYSIZE_DAY);
				_AbbreviatedDayNames = (string[]) value.Clone();
			}
		}

		public string[] AbbreviatedMonthNames
		{
			get
			{
				return (string[]) _AbbreviatedMonthNames.Clone();
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				if (value.GetLength(0) != 13) throw new ArgumentException(MSG_ARRAYSIZE_MONTH);
				_AbbreviatedMonthNames = (string[]) value.Clone();
			}
		}

		public string[] DayNames
		{
			get
			{
				return (string[]) _DayNames.Clone();
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				if (value.GetLength(0) != 7) throw new ArgumentException(MSG_ARRAYSIZE_DAY);
				_DayNames = (string[]) value.Clone();
			}
		}

		public string[] MonthNames
		{
			get
			{
				return (string[]) _MonthNames.Clone();
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				if (value.GetLength(0) != 13) throw new ArgumentException(MSG_ARRAYSIZE_MONTH);
				_MonthNames = (string[]) value.Clone();
			}
		}

		public string AMDesignator
		{
			get
			{
				return _AMDesignator;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_AMDesignator = value;
			}
		}

		public string PMDesignator
		{
			get
			{
				return _PMDesignator;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_PMDesignator = value;
			}
		}

		public string DateSeparator
		{
			get
			{
				return _DateSeparator;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_DateSeparator = value;
			}
		}

		public string TimeSeparator
		{
			get
			{
				return _TimeSeparator;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_TimeSeparator = value;
			}
		}

		public string LongDatePattern
		{
			get
			{
				return _LongDatePattern;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_LongDatePattern = value;
			}
		}

		public string ShortDatePattern
		{
			get
			{
				return _ShortDatePattern;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_ShortDatePattern = value;
			}
		}

		public string ShortTimePattern
		{
			get
			{
				return _ShortTimePattern;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_ShortTimePattern = value;
			}
		}

		public string LongTimePattern
		{
			get
			{
				return _LongTimePattern;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_LongTimePattern = value;
			}
		}

		public string MonthDayPattern
		{
			get
			{
				return _MonthDayPattern;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_MonthDayPattern = value;
			}
		}

		public string YearMonthPattern
		{
			get
			{
				return _YearMonthPattern;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_YearMonthPattern = value;
			}
		}

		public string FullDateTimePattern
		{
			get
			{
				if(_FullDateTimePattern!=null) {
					return _FullDateTimePattern;
				} else {
					return(_LongDatePattern + " " + _LongTimePattern);
				}
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_FullDateTimePattern = value;
			}
		}

		public static DateTimeFormatInfo CurrentInfo
		{
			get
			{
				return Thread.CurrentThread.CurrentCulture.DateTimeFormat;
			}
		}

		public static DateTimeFormatInfo InvariantInfo
		{
			get
			{
				if (theInvariantDateTimeFormatInfo == null) {
					theInvariantDateTimeFormatInfo = 
						DateTimeFormatInfo.ReadOnly(new DateTimeFormatInfo());
					theInvariantDateTimeFormatInfo.FillInvariantPatterns ();
				}
				return theInvariantDateTimeFormatInfo;
			}
		}

		// LAMESPEC: this is not in ECMA specs
		public DayOfWeek FirstDayOfWeek
		{
			get
			{
				return _FirstDayOfWeek;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if ((int) value < 0 || (int) value > 6) throw new ArgumentOutOfRangeException();
				_FirstDayOfWeek = value;
			}
		}

		// LAMESPEC: this is not in ECMA specs
		public Calendar Calendar
		{
			get
			{
				return _Calendar;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				if (value == null) throw new ArgumentNullException();
				_Calendar = value;
			}
		}

		public CalendarWeekRule CalendarWeekRule
		{
			get
			{
				return _CalendarWeekRule;
			}
			set
			{
				if (IsReadOnly) throw new InvalidOperationException(MSG_READONLY);
				_CalendarWeekRule = value;
			}
		}

		// LAMESPEC: this is not in ECMA specs
		public string RFC1123Pattern
		{
			get
			{
				return _RFC1123Pattern;
			}
		}

		// LAMESPEC: this is not in ECMA specs
		public string SortableDateTimePattern
		{
			get
			{
				return _SortableDateTimePattern;
			}
		}

		// LAMESPEC: this is not in ECMA specs
		public string UniversalSortableDateTimePattern
		{
			get
			{
				return _UniversalSortableDateTimePattern;
			}
		}
		
		// LAMESPEC: this is not in ECMA specs
		[MonoTODO ("Not complete depending on GetAllDateTimePatterns(char)")]
		public string[] GetAllDateTimePatterns()
		{
			ArrayList al = new ArrayList ();
			foreach (string s in GetAllDateTimePatterns ('d'))
				al.Add (s);
			foreach (string s in GetAllDateTimePatterns ('D'))
				al.Add (s);
			foreach (string s in GetAllDateTimePatterns ('g'))
				al.Add (s);
			foreach (string s in GetAllDateTimePatterns ('G'))
				al.Add (s);
			foreach (string s in GetAllDateTimePatterns ('f'))
				al.Add (s);
			foreach (string s in GetAllDateTimePatterns ('F'))
				al.Add (s);
			// Yes, that is very meaningless, but that is what MS
			// is doing (LAMESPEC: Since it is documented that
			// 'M' and 'm' are equal, they should not cosider
			// that there is a possibility that 'M' and 'm' are
			// different.)
			foreach (string s in GetAllDateTimePatterns ('m'))
				al.Add (s);
			foreach (string s in GetAllDateTimePatterns ('M'))
				al.Add (s);
			foreach (string s in GetAllDateTimePatterns ('r'))
				al.Add (s);
			foreach (string s in GetAllDateTimePatterns ('R'))
				al.Add (s);
			foreach (string s in GetAllDateTimePatterns ('s'))
				al.Add (s);
			foreach (string s in GetAllDateTimePatterns ('t'))
				al.Add (s);
			foreach (string s in GetAllDateTimePatterns ('T'))
				al.Add (s);
			foreach (string s in GetAllDateTimePatterns ('u'))
				al.Add (s);
			foreach (string s in GetAllDateTimePatterns ('U'))
				al.Add (s);
			foreach (string s in GetAllDateTimePatterns ('y'))
				al.Add (s);
			foreach (string s in GetAllDateTimePatterns ('Y'))
				al.Add (s);

			return al.ToArray (typeof (string)) as string [];
		}

		// LAMESPEC: this is not in ECMA specs
		[MonoTODO ("We need more culture data in locale-builder")]
		public string[] GetAllDateTimePatterns (char format)
		{
			string [] list;
			switch (format) {
			// Date
			case 'D':
				if (_LongDatePatterns != null && _LongDatePatterns.Length > 0)
					return _LongDatePatterns.Clone () as string [];
				return new string [] {LongDatePattern};
			case 'd':
				if (_ShortDatePatterns != null && _ShortDatePatterns.Length > 0)
					return _ShortDatePatterns.Clone () as string [];
				return new string [] {ShortDatePattern};
			// Time
			case 'T':
				if (_LongTimePatterns != null && _LongTimePatterns.Length > 0)
					return _LongTimePatterns.Clone () as string [];
				return new string [] {LongTimePattern};
			case 't':
				if (_ShortTimePatterns != null && _ShortTimePatterns.Length > 0)
					return _ShortTimePatterns.Clone () as string [];
				return new string [] {ShortTimePattern};
			// {Short|Long}Date + {Short|Long}Time
			// FIXME: they should be the agglegation of the
			// combination of the Date patterns and Time patterns.
			case 'G':
				list = PopulateCombinedList (_ShortDatePatterns, _LongTimePatterns);
				if (list != null && list.Length > 0)
					return list;
				return new string [] {ShortDatePattern + " " + LongTimePattern};
			case 'g':
				list = PopulateCombinedList (_ShortDatePatterns, _ShortTimePatterns);
				if (list != null && list.Length > 0)
					return list;
				return new string [] {ShortDatePattern + " " + ShortTimePattern};
			// The 'U' pattern strings are always the same as 'F'.
			// (only differs in assuming UTC or not.)
			case 'U':
			case 'F':
				list = PopulateCombinedList (_LongDatePatterns, _LongTimePatterns);
				if (list != null && list.Length > 0)
					return list;
				return new string [] {LongDatePattern + " " + LongTimePattern};
			case 'f':
				list = PopulateCombinedList (_LongDatePatterns, _ShortTimePatterns);
				if (list != null && list.Length > 0)
					return list;
				return new string [] {LongDatePattern + " " + ShortTimePattern};
			// MonthDay
			case 'm':
			case 'M':
				if (_MonthDayPatterns != null && _MonthDayPatterns.Length > 0)
					return _MonthDayPatterns.Clone () as string [];
				return new string [] {MonthDayPattern};
			// YearMonth
			case 'Y':
			case 'y':
				if (_YearMonthPatterns != null && _YearMonthPatterns.Length > 0)
					return _YearMonthPatterns.Clone () as string [];
				return new string [] {YearMonthPattern};
			// RFC1123
			case 'r':
			case 'R':
				return new string [] {RFC1123Pattern};
			case 's':
				return new string [] {SortableDateTimePattern};
			case 'u':
				return new string [] {UniversalSortableDateTimePattern};
			}
			throw new ArgumentException ("Format specifier was invalid.");
		}

		// LAMESPEC: this is not in ECMA specs
		public string GetDayName(DayOfWeek dayofweek)
		{
			int index = (int) dayofweek;
			if (index < 0 || index > 6) throw new ArgumentOutOfRangeException();
			return _DayNames[index];
		}

		// LAMESPEC: this is not in ECMA specs
		public string GetAbbreviatedDayName(DayOfWeek dayofweek)
		{
			int index = (int) dayofweek;
			if (index < 0 || index > 6) throw new ArgumentOutOfRangeException();
			return _AbbreviatedDayNames[index];
		}

		private void FillInvariantPatterns ()
		{
			_ShortDatePatterns = new string [] {"MM/dd/yyyy"};
			_LongDatePatterns = new string [] {"dddd, dd MMMM yyyy"};
			_LongTimePatterns = new string [] {"HH:mm:ss"};
			_ShortTimePatterns = new string [] {
				"HH:mm",
				"hh:mm tt",
				"H:mm",
				"h:mm tt"
			};
			_MonthDayPatterns = new string [] {"MMMM dd"};
			_YearMonthPatterns = new string [] {"yyyy MMMM"};
		}

		private string [] PopulateCombinedList (string [] dates, string [] times)
		{
			if (dates != null && times != null) {
				string [] list = new string [dates.Length * times.Length];
				int i = 0;
				foreach (string d in dates)
					foreach (string t in times)
						list [i++] = d + " " + t;
				return list;
			}
			return null;
		}
	}
}
