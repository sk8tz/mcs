/**
 * Namespace: System.Web.UI.WebControls
 * Class:     BaseCompareValidator
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Implementation: yes
 * Contact: <gvaish@iitk.ac.in>
 * Status:  30%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public abstract class BaseCompareValidator: BaseValidator
	{
		protected BaseCompareValidator(): base()
		{
		}
		
		public static bool CanConvert(string text, ValidationDataType type)
		{
			object o = null;
			return Convert(text, type, out o);
		}
		
		public ValidationDataType Type
		{
			get
			{
				object o = ViewState["Type"];
				if(o!=null)
					return (ValidationDataType)o;
				return ValidationDataType.String;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(ValidationDataType), value))
					throw new ArgumentException();
				ViewState["Type"] = value;
			}
		}
		
		protected static int CutoffYear
		{
			get
			{
				return DateTimeFormatInfo.CurrentInfo.Calendar.TwoDigitYearMax;
			}
		}

		protected static int GetFullYear(int shortYear)
		{
			int century = DateTime.Today.Year - (DateTime.Today.Year % 100);			
			if(century < CutoffYear)
			{
				return (shortYear + century);
			}
			return (shortYear + century - 100);
		}
		
		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			/*
			ValidationDataType vdt;
			NumberFormatInfo   nfi;
			DateTime           dt;
			*/
			base.AddAttributesToRender(writer);
			if(base.RenderUplevel)
			{
				// TODO: The Lost World
			}
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Undocumented
		/// </summary>
		protected static bool Compare(string leftText, string rightText, ValidationCompareOperator op, ValidationDataType type)
		{
			object left = null, right = null;
			if(!Convert(leftText, type, out left))
			{
				return false;
			}
			if(op == ValidationCompareOperator.DataTypeCheck)
			{
				return true;
			}
			if(!Convert(rightText, type, out right))
			{
				return true;
			}
			int compareResult;
			switch(type)
			{
				case ValidationDataType.String:
					compareResult = ((String)left).CompareTo(right);
					break;
			}
			throw new NotImplementedException();
			return false;
		}
		
		/// <summary>
		/// Undocumented
		/// </summary>
		protected static string GetDateElementOrder()
		{
			string pattern = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
			
			//TODO: What are the various possibilities?
			// I can think of only y*/M*/d*, d*/M*/y*, M*/d*/y*
			if(pattern.IndexOf('y') < pattern.IndexOf('M'))
			{
				return "ymd";
			}
			if(pattern.IndexOf('M') < pattern.IndexOf('d'))
			{
				return "mdy";
			}
			return "dmy";
		}
		
		// Uncodumented
		protected static bool Convert(string text, ValidationDataType type, out object convertedValue)
		{
			//throw new NotImplementedException();
			convertedValue = null;
			try
			{
				switch(type)
				{
					case ValidationDataType.String:	convertedValue = text;
						break;
					case ValidationDataType.Integer: convertedValue = Int32.Parse(text, CultureInfo.InvariantCulture);
						break;
					case ValidationDataType.Double:
						Match matchDouble = Regex.Match(text, @"^\s*([-\+])?(\d+)?(\"
			            + NumberFormatInfo.CurrentInfo.NumberDecimalSeparator
			            + @"(\d+))?\s*$");
						if(matchDouble.Success)
						{
							string sign     = (matchDouble.Groups[1].Success ? matchDouble.Groups[1].Value : "+");
							string decPart  = (matchDouble.Groups[2].Success ? matchDouble.Groups[2].Value : "0");
							string mantissa = (matchDouble.Groups[4].Success ? matchDouble.Groups[4].Value : "0");
							convertedValue  = Double.Parse(sign + decPart + "." + mantissa, CultureInfo.InvariantCulture);
						}
						break;
					case ValidationDataType.Date:
						if(DateTimeFormatInfo.CurrentInfo.Calendar.GetType() != typeof(GregorianCalendar))
						{
							convertedValue = DateTime.Parse(text);
							break;
						}
						string order = GetDateElementOrder();
						int date = 0, mth = 0, year = 0;
						Match  matchDate = Regex.Match(text, @"^\s*((\d{4})|(\d{2}))([\.\/-])(\d{1,2})\4(\d{1,2})\s*$");
						if(matchDate.Success && order == "ymd")
						{
							date = Int32.Parse(matchDate.Groups[6].Value, CultureInfo.InvariantCulture);
							mth  = Int32.Parse(matchDate.Groups[5].Value, CultureInfo.InvariantCulture);
							year = Int32.Parse((matchDate.Groups[2].Success ? matchDate.Groups[2].Value : matchDate.Groups[3].Value), CultureInfo.InvariantCulture);
						} else
						{
							matchDate = Regex.Match(text, @"^\s*(\d{1,2})([\.\/-])(\d{1,2})\2((\d{4}|\d{2}))\s*$");
							if(matchDate.Success)
							{
								if(order == "dmy")
								{
									date = Int32.Parse(matchDate.Groups[1].Value, CultureInfo.InvariantCulture);
									mth  = Int32.Parse(matchDate.Groups[3].Value, CultureInfo.InvariantCulture);
									year = Int32.Parse((matchDate.Groups[5].Success ? matchDate.Groups[5].Value : matchDate.Groups[6].Value), CultureInfo.InvariantCulture);
								}
								if(order == "mdy")
								{
									date = Int32.Parse(matchDate.Groups[3].Value, CultureInfo.InvariantCulture);
									mth  = Int32.Parse(matchDate.Groups[1].Value, CultureInfo.InvariantCulture);
									year = Int32.Parse((matchDate.Groups[5].Success ? matchDate.Groups[5].Value : matchDate.Groups[6].Value), CultureInfo.InvariantCulture);
								}
							}
						}
						year = (year < 100 ? GetFullYear(year) : year);
						if(matchDate.Success && date!=0 && mth!=0 && year!=0)
						{
							convertedValue = new DateTime(year, mth, date);
						}
						break;
					case  ValidationDataType.Currency:
						string decSep = NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator;
						string grpSep = NumberFormatInfo.CurrentInfo.CurrencyGroupSeparator;
						int    decDig = NumberFormatInfo.CurrentInfo.CurrencyDecimalDigits;
						if(grpSep[0] > 0xA0)
						{
							grpSep = " ";
						}
						string[] patternArray = new string[5];
						patternArray[0] = "^\\s*([-\\+])?(((\\d+)\\";
						patternArray[1] = grpSep;
						patternArray[2] = @")*)(\d+)";
						if(decDig > 0)
						{
							string[] decPattern = new string[5];
							decPattern[0] = "(\\";
							decPattern[1] = decSep;
							decPattern[2] = @"(\d{1,";
							decPattern[3] = decDig.ToString(NumberFormatInfo.InvariantInfo);
							decPattern[4] = @"}))";
							patternArray[3] = String.Concat(decPattern);
							
						} else
						{
							patternArray[3] = String.Empty;
						}
						patternArray[4] = @"?\s*$";
						Match matchCurrency = Regex.Match(text, String.Concat(patternArray));
						if(matchCurrency.Success)
						{
							StringBuilder sb = new StringBuilder();
							sb.Append(matchCurrency.Groups[1]);
							CaptureCollection cc = matchCurrency.Groups[4].Captures;
							foreach(IEnumerable current in cc)
							{
								sb.Append((Capture)current);
							}
							sb.Append(matchCurrency.Groups[5]);
							if(decDig > 0)
							{
								sb.Append(".");
								sb.Append(matchCurrency.Groups[7]);
							}
							convertedValue = Decimal.Parse(sb.ToString(), CultureInfo.InvariantCulture);
						}
						break;
				}
			} catch(Exception e)
			{
				convertedValue = null;
			}
			return (convertedValue != null);
		}
	}
}
