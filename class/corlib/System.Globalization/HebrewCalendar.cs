// HebrewCalendar.cs
//
// (C) Ulrich Kunitz 2002
//

namespace System.Globalization {

using System;
using System.IO;

/// <summary>
/// This is the Hebrew calendar.
/// </summary>
/// <remarks>
/// <para>The Hebrew calendar supports only years between 5343 A.M. and
/// 6000 A.M. This has been done to be compatible with .NET.
/// </para>
/// <para>The implementation uses the
/// <see cref="N:CalendricalCalculations"/> namespace.
/// </para>
/// </remarks>
[Serializable]
public class HebrewCalendar : Calendar {
	/// <summary>
	/// Constructor.
	/// </summary>
	public HebrewCalendar() {
		M_AbbrEraNames = new string[] {"A.M."};
		M_EraNames = new string[] {"Anno Mundi"};
		if (M_TwoDigitYearMax == 99)
			M_TwoDigitYearMax = 5790;
	}

	/// <summary>
	/// The era number for the Anno Mundi (A.M.) era, called
	/// plain HebrewEra.
	/// </summary>
	public const int HebrewEra = 1;

	/// <summary>
	/// The
	/// <see cref="T:System.DateTime"/> ticks for first day of year
	/// 5343 A.M.
	/// </summary>
	internal const long M_MinTicks = 499147488000000000L;
	/// <summary>
	/// The number of
	/// <see cref="T:System.DateTime"/> ticks for the last day of year
	/// 6000 A.M.
	/// </summary>
	internal const long M_MaxTicks = 706783967999999999L;
	/// <summary>
	/// The minimum year in the A.M. era supported.
	/// </summary>
	internal const int M_MinYear = 5343;
	/// <summary>
	/// The maximum year supported in the A.M. era.
	/// </summary>
	internal override int M_MaxYear {
		get { return 6000; }
	}

	/// <value>Overridden. Gives the eras supported by the Gregorian
	/// calendar as an array of integers.
	/// </value>
	public override int[] Eras {
		get {
			return new int[] { HebrewEra }; 
		}
	}

	/// <summary>
	/// A protected member checking a
	/// <see cref="T:System.DateTime"/> value.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/>
	/// to check.
	/// </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	internal void M_CheckDateTime(DateTime time) {
		if (time.Ticks < M_MinTicks || time.Ticks > M_MaxTicks)
			throw new ArgumentOutOfRangeException(
				"time",
				"Only hebrew years between 5343 and 6000," +
				" inclusive, are supported.");
	}

	/// <summary>
	/// A protected method checking the era number.
	/// </summary>
	/// <param name="era">The era number.</param>
	/// <exception name="T:System.ArgumentException">
	/// The exception is thrown if the era is not equal
	/// <see cref="F:HebrewEra"/>.
	/// </exception>
	internal void M_CheckEra(ref int era) {
		if (era == CurrentEra)
			era = HebrewEra;
		if (era != HebrewEra)
			throw new ArgumentException("Era value was not valid.");
	}

	/// <summary>
	/// A protected method checking calendar year and the era number.
	/// </summary>
	/// <param name="year">An integer representing the calendar year.
	/// </param>
	/// <param name="era">The era number.</param>
	/// <exception cref="T:System.ArgumentException">
	/// The exception is thrown if the era is not equal
	/// <see cref="F:HebrewEra"/>.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the calendar year is outside of
	/// the allowed range.
	/// </exception>
	internal override void M_CheckYE(int year, ref int era) {
		M_CheckEra(ref era);
		if (year < M_MinYear || year > M_MaxYear)
			throw new ArgumentOutOfRangeException(
				"year",
				"Only hebrew years between 5343 and 6000," +
				" inclusive, are supported.");
	}

	/// <summary>
	/// A protected method checking the calendar year, month, and
	/// era number.
	/// </summary>
	/// <param name="year">An integer representing the calendar year.
	/// </param>
	/// <param name="month">An integer giving the calendar month.
	/// </param>
	/// <param name="era">The era number.</param>
	/// <exception cref="T:System.ArgumentException">
	/// The exception is thrown if the era is not equal
	/// <see cref="F:HebrewEra"/>.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the calendar year or month is
	/// outside of the allowed range.
	/// </exception>
	internal void M_CheckYME(int year, int month, ref int era) {
		M_CheckYE(year, ref era);
		int l = CCHebrewCalendar.last_month_of_year(year);
		if (month < 1 || month > l) {
			StringWriter sw = new StringWriter();
			sw.Write("Month must be between 1 and {0}.", l);
			throw new ArgumentOutOfRangeException("month",
				sw.ToString());
		}
	}

	/// <summary>
	/// A protected method checking the calendar day, month, and year
	/// and the era number.
	/// </summary>
	/// <param name="year">An integer representing the calendar year.
	/// </param>
	/// <param name="month">An integer giving the calendar month.
	/// </param>
	/// <param name="day">An integer giving the calendar day.
	/// </param>
	/// <param name="era">The era number.</param>
	/// <exception cref="T:System.ArgumentException">
	/// The exception is thrown if the era is not equal
	/// <see cref="F:HebrewEra"/>.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the calendar year, month, or day is
	/// outside of the allowed range.
	/// </exception>
	internal void M_CheckYMDE(int year, int month, int day,
		ref int era)
	{
		M_CheckYME(year, month, ref era);
		M_ArgumentInRange("day", day, 1, GetDaysInMonth(year, month,
			era));
	}

	/// <summary>
	/// Overridden. Adds days to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// days.
	/// </param>
	/// <param name="days">The number of days to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="days"/> to the specified
	/// DateTime.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> return value is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	public override DateTime AddDays(DateTime time, int days) {
		DateTime t = base.AddDays(time, days);
		M_CheckDateTime(t);
		return t;
	}

	/// <summary>
	/// Overridden. Adds hours to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// hours.
	/// </param>
	/// <param name="hours">The number of hours to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="hours"/> to the specified
	/// DateTime.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> return value is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	public override DateTime AddHours(DateTime time, int hours) {
		DateTime t = base.AddHours(time, hours);
		M_CheckDateTime(t);
		return t;
	}

	/// <summary>
	/// Overridden. Adds milliseconds to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// milliseconds.
	/// </param>
	/// <param name="milliseconds">The number of milliseconds given as
	/// double to add. Keep in mind the 100 nanosecond resolution of 
	/// <see cref="T:System.DateTime"/>.
	/// </param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="milliseconds"/> to the specified
	/// DateTime.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> return value is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	public override DateTime AddMilliseconds(DateTime time,
		double milliseconds)
	{
		DateTime t = base.AddMilliseconds(time, milliseconds);
		M_CheckDateTime(t);
		return t;
	}

	/// <summary>
	/// Overridden. Adds minutes to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// minutes.
	/// </param>
	/// <param name="minutes">The number of minutes to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="minutes"/> to the specified
	/// DateTime.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> return value is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	public override DateTime AddMinutes(DateTime time, int minutes) {
		DateTime t = base.AddMinutes(time, minutes);
		M_CheckDateTime(t);
		return t;
	}

	/// <summary>
	/// Overridden. Adds seconds to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// seconds.
	/// </param>
	/// <param name="seconds">The number of seconds to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="seconds"/> to the specified
	/// DateTime.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> return value is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	public override DateTime AddSeconds(DateTime time, int seconds) {
		DateTime t = base.AddSeconds(time, seconds);
		M_CheckDateTime(t);
		return t;
	}

	/// <summary>
	/// Overridden. Adds weeks to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// weeks.
	/// </param>
	/// <param name="weeks">The number of weeks to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="weeks"/> to the specified
	/// DateTime.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> return value is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	public override DateTime AddWeeks(DateTime time, int weeks) {
		DateTime t = base.AddWeeks(time, weeks);
		M_CheckDateTime(t);
		return t;
	}

	/// <summary>
	/// Overridden. Gives the hour of the specified time.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies the
	/// time.
	/// </param>
	/// <returns>An integer that gives the hour of the specified time,
	/// starting with 0.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	public override int GetHour(DateTime time) {
		M_CheckDateTime(time);
		return base.GetHour(time);
	}

	/// <summary>
	/// Overridden. Gives the milliseconds in the current second
	/// of the specified time.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies the
	/// time.
	/// </param>
	/// <returns>An integer that gives the milliseconds in the seconds
	/// of the specified time, starting with 0.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	public override double GetMilliseconds(DateTime time) {
		M_CheckDateTime(time);
		return base.GetMilliseconds(time);
	}

	/// <summary>
	/// Overridden. Gives the minute of the specified time.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies the
	/// time.
	/// </param>
	/// <returns>An integer that gives the minute of the specified time,
	/// starting with 0.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	public override int GetMinute(DateTime time) {
		M_CheckDateTime(time);
		return base.GetMinute(time);
	}

	/// <summary>
	/// Overridden. Gives the second of the specified time.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies the
	/// time.
	/// </param>
	/// <returns>An integer that gives the second of the specified time,
	/// starting with 0.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	public override int GetSecond(DateTime time) {
		M_CheckDateTime(time);
		return base.GetMinute(time);
	}

	/// <summary>
	/// Overrideden. Adds months to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// months.
	/// </param>
	/// <param name="months">The number of months to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="months"/> to the specified
	/// DateTime.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> return value is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	public override DateTime AddMonths(DateTime time, int months) {
		int y, m, d;
		DateTime t;

		if (months == 0) {
			t = time;
		} else {
			int rd = CCFixed.FromDateTime(time);
			CCHebrewCalendar.dmy_from_fixed(
				out d, out m, out y, rd);
			m = M_Month(m, y);
			if (months < 0) {
				while (months < 0) {
					if (m+months > 0) {
						m += months;
						months = 0;
					} else {
						months += m;
						y -= 1;
						m = GetMonthsInYear(y);
					}
				}
			}
			else {
				while (months > 0) {
					int my = GetMonthsInYear(y);
					if (m+months <= my) {
						m += months;
						months = 0;
					} else {
						months -= my-m+1;
						m = 1;
						y += 1;
					}
				}
			}
			t = ToDateTime(y, m, d, 0, 0, 0, 0);
			t = t.Add(time.TimeOfDay);
		}
		M_CheckDateTime(t);
		return t;
	}

	/// <summary>
	/// Overridden. Adds years to a given date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> to which to add
	/// years.
	/// </param>
	/// <param name="years">The number of years to add.</param>
	/// <returns>A new <see cref="T:System.DateTime"/> value, that
	/// results from adding <paramref name="years"/> to the specified
	/// DateTime.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> return value is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	public override DateTime AddYears(DateTime time, int years) {
		int rd = CCFixed.FromDateTime(time);
		int day, month, year;
		CCHebrewCalendar.dmy_from_fixed(
			out day, out month, out year, rd);
		year += years;
		rd = CCHebrewCalendar.fixed_from_dmy(day, month, year);
		DateTime t = CCFixed.ToDateTime(rd);
		t = t.Add(time.TimeOfDay);
		M_CheckDateTime(t);
		return t;
	}
		
	/// <summary>
	/// Overriden. Gets the day of the month from
	/// <paramref name="time"/>.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer giving the day of months, starting with 1.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	public override int GetDayOfMonth(DateTime time) {
		M_CheckDateTime(time);
		int rd = CCFixed.FromDateTime(time);
		return CCHebrewCalendar.day_from_fixed(rd);
	}

	/// <summary>
	/// Overriden. Gets the day of the week from the specified date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer giving the day of months, starting with 1.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	public override DayOfWeek GetDayOfWeek(DateTime time) {
		M_CheckDateTime(time);
		int rd = CCFixed.FromDateTime(time);
		return (DayOfWeek)CCFixed.day_of_week(rd);
	}

	/// <summary>
	/// Overridden. Gives the number of the day in the year.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer representing the day of the year,
	/// starting with 1.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	public override int GetDayOfYear(DateTime time) {
		M_CheckDateTime(time);
		int rd = CCFixed.FromDateTime(time);
		int year = CCHebrewCalendar.year_from_fixed(rd);
		int rd1_7 = CCHebrewCalendar.fixed_from_dmy(1, 7, year);
		return rd - rd1_7 + 1;
	}

	/// <summary>
	/// The method maps a .NET Hebrew month to a Calencdrical
	/// Calculations Hebrew month. 
	/// </summary>
	/// <param name="month">An integer representing a month in .NET
	/// counting (starting with Tishri).
	/// </param>
	/// <param name="year">An integer representing the Hebrew year.
	/// </param>
	/// <returns>The Hebrew month in Calendrical Calculations counting,
	/// staring with the Hebrew month Nisan.
	/// </returns>
	/// <remarks>
	/// <para>
	/// In .NET the month counting starts with the Hebrew month Tishri.
	/// Calendrical Calculations starts with the month Tisan. So we must
	/// map here.
	/// </para>
	/// </remarks>
	internal int M_CCMonth(int month, int year) {
		if (month <= 6) {
			return 6+month;
		}
		else {
			int l = CCHebrewCalendar.last_month_of_year(year);
			if (l == 12) {
				return month-6;
			}
			else {
				return month <= 7 ? 6+month : month-7;  
			}
		}
	}

	/// <summary>
	/// The method maps a Calendrical Calculations Hebrew month
	/// to a .NET Hebrew month. 
	/// </summary>
	/// <param name="ccmonth">An integer representing a month in
	/// Calendrical Calculations counting, starting with Nisan.
	/// </param>
	/// <param name="year">An integer representing the Hebrew year.
	/// </param>
	/// <returns>The Hebrew month in .NET counting,
	/// staring with the Hebrew month Tishri.
	/// </returns>
	/// <remarks>
	/// <para>
	/// In .NET the month counting starts with the Hebrew month Tishri.
	/// Calendrical Calculations starts with the month Tisan. So we must
	/// map here.
	/// </para>
	/// </remarks>
	internal int M_Month(int ccmonth, int year) {
		if (ccmonth >= 7) {
			return ccmonth - 6;
		} else {
			int l = CCHebrewCalendar.last_month_of_year(year);
			return ccmonth + (l == 12 ? 6 : 7);
		}
	}

	/// <summary>
	/// Overridden. Gives the number of days in the specified month
	/// of the given year and era.
	/// </summary>
	/// <param name="year">An integer that gives the year.
	/// </param>
	/// <param name="month">An integer that gives the month, starting
	/// with 1.</param>
	/// <param name="era">An integer that gives the era of the specified
	/// year.</param>
	/// <returns>An integer that gives the number of days of the
	/// specified month.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if <paramref name="month"/>,
	/// <paramref name="year"/> ,or <paramref name="era"/> is outside
	/// the allowed range.
	/// </exception>
	public override int GetDaysInMonth(int year, int month, int era) {
		M_CheckYME(year, month, ref era);
		int ccmonth = M_CCMonth(month, year); 
		int rd1 = CCHebrewCalendar.fixed_from_dmy(1, ccmonth, year);
		int rd2 = CCHebrewCalendar.fixed_from_dmy(1, ccmonth+1, year);
		return rd2 - rd1;
	}

	/// <summary>
	/// Overridden. Gives the number of days of the specified
	/// year of the given era. 
	/// </summary>
	/// <param name="year">An integer that specifies the year. 
	/// </param>
	/// <param name="era">An ineger that specifies the era.
	/// </param>
	/// <returns>An integer that gives the number of days of the
	/// specified year.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeExceiption">
	/// The exception is thrown, if
	/// <paramref name="year"/> or <paramref name="era"/> are outside the
	/// allowed range.
	/// </exception>
	public override int GetDaysInYear(int year, int era) {
		M_CheckYE(year, ref era);
		int rd1 = CCHebrewCalendar.fixed_from_dmy(1, 7, year);
		int rd2 = CCHebrewCalendar.fixed_from_dmy(1, 7, year+1);
		return rd2 - rd1;
	}
		

	/// <summary>
	/// Overridden. Gives the era of the specified date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer representing the era of the calendar.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	public override int GetEra(DateTime time) {
		M_CheckDateTime(time);
		return HebrewEra;
	}

	/// <summary>
	/// Overridden. Gives the number of the month of the specified
	/// date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer representing the month, 
	/// starting with 1.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	public override int GetMonth(DateTime time) {
		M_CheckDateTime(time);
		int rd = CCFixed.FromDateTime(time);
		int ccmonth, year;
		CCHebrewCalendar.my_from_fixed(out ccmonth, out year, rd);
		return M_Month(ccmonth, year);
	}

	/// <summary>
	/// Overridden. Gives the number of months in the specified year 
	/// and era.
	/// </summary>
	/// <param name="year">An integer that specifies the year.
	/// </param>
	/// <param name="era">An integer that specifies the era.
	/// </param>
	/// <returns>An integer that gives the number of the months in the
	/// specified year.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if the year or the era are not valid.
	/// </exception>
	public override int GetMonthsInYear(int year, int era) {
		M_CheckYE(year, ref era);
		return CCHebrewCalendar.last_month_of_year(year);
	}

	/// <summary>
	/// Overridden. Gives the number of the year of the specified
	/// date.
	/// </summary>
	/// <param name="time">The
	/// <see cref="T:System.DateTime"/> that specifies a
	/// date.
	/// </param>
	/// <returns>An integer representing the year, 
	/// starting with 1.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown if the
	/// <see cref="T:System.DateTime"/> parameter is not in the years
	/// between 5343 A.M. and 6000 A.M., inclusive.
	/// </exception>
	public override int GetYear(DateTime time) {
		M_CheckDateTime(time);
		int rd = CCFixed.FromDateTime(time);
		return CCHebrewCalendar.year_from_fixed(rd);
	}

	/// <summary>
	/// Overridden. Tells whether the given day 
	/// is a leap day.
	/// </summary>
	/// <param name="year">An integer that specifies the year in the
	/// given era.
	/// </param>
	/// <param name="month">An integer that specifies the month.
	/// </param>
	/// <param name="day">An integer that specifies the day.
	/// </param>
	/// <param name="era">An integer that specifies the era.
	/// </param>
	/// <returns>A boolean that tells whether the given day is a leap
	/// day.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if the year, month, day, or era is not
	/// valid.
	/// </exception>
	/// <remarks>All days in Adar II are viewed as leap days and the
	/// last day of Adar I.
	/// </remarks>
	public override bool IsLeapDay(int year, int month, int day, int era)
	{
		M_CheckYMDE(year, month, day, ref era);
		return IsLeapYear(year) &&
			(month == 7 || (month == 6 && day == 30)); 
	}

	/// <summary>
	/// Overridden. Tells whether the given month 
	/// is a leap month.
	/// </summary>
	/// <param name="year">An integer that specifies the year in the
	/// given era.
	/// </param>
	/// <param name="month">An integer that specifies the month.
	/// </param>
	/// <param name="era">An integer that specifies the era.
	/// </param>
	/// <returns>A boolean that tells whether the given month is a leap
	/// month.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if the year, month, or era is not
	/// valid.
	/// </exception>
	/// <remarks>
	/// Adar II is viewed as leap month.
	/// </remarks>
	public override bool IsLeapMonth(int year, int month, int era) {
		M_CheckYME(year, month, ref era);
		return IsLeapYear(year) && month == 7; 
	}

	/// <summary>
	/// Overridden. Tells whether the given year
	/// is a leap year.
	/// </summary>
	/// <param name="year">An integer that specifies the year in the
	/// given era.
	/// </param>
	/// <param name="era">An integer that specifies the era.
	/// </param>
	/// <returns>A boolean that tells whether the given year is a leap
	/// year.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if the year or era is not
	/// valid.
	/// </exception>
	public override bool IsLeapYear(int year, int era) {
		M_CheckYE(year, ref era);
		return CCHebrewCalendar.is_leap_year(year);
	}

	/// <summary>
	/// Overridden. Creates the
	/// <see cref="T:System.DateTime"/> from the parameters.
	/// </summary>
	/// <param name="year">An integer that gives the year in the
	/// <paramref name="era"/>.
	/// </param>
	/// <param name="month">An integer that specifies the month.
	/// </param>
	/// <param name="day">An integer that specifies the day.
	/// </param>
	/// <param name="hour">An integer that specifies the hour.
	/// </param>
	/// <param name="minute">An integer that specifies the minute.
	/// </param>
	/// <param name="second">An integer that gives the second.
	/// </param>
	/// <param name="milliseconds">An integer that gives the
	/// milliseconds.
	/// </param>
	/// <param name="era">An integer that specifies the era.
	/// </param>
	/// <returns>A
	/// <see cref="T:system.DateTime"/> representig the date and time.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// The exception is thrown, if at least one of the parameters
	/// is out of range.
	/// </exception>
	public override DateTime ToDateTime(int year, int month, int day,
		int hour, int minute, int second, int milliseconds,
		int era)
	{
		M_CheckYMDE(year, month, day, ref era);
		M_CheckHMSM(hour, minute, second, milliseconds);
		int ccm = M_CCMonth(month, year);
		int rd = CCHebrewCalendar.fixed_from_dmy(day, ccm, year);
		return CCFixed.ToDateTime(rd,
			hour, minute, second, milliseconds);
	}
} // class HebrewCalendar
	
} // namespace System.Globalization
