//
// Strings.cs
//
// Authors:
//   Martin Adoue (martin@cwanet.com)
//   Chris J Breisch (cjbreisch@altavista.net)
//   Francesco Delfino (pluto@tipic.com)
//   Daniel Campos (danielcampos@netcourrier.com)
//   Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//   Jochen Wezel (jwezel@compumaster.de)
//   Dennis Hayes (dennish@raytek.com)
//   Pablo Cardona (pcardona37@hotmail.com) CRL Team
// 
// (C) 2002 Ximian Inc.
//     2002 Tipic, Inc. (http://www.tipic.com)
//     2003 CompuMaster GmbH (http://www.compumaster.de)
//     2004 Novell
//

//
// Copyright (c) 2002-2003 Mainsoft Corporation.
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
using System.Text;
using System.ComponentModel;
using System.Globalization;

using System.Runtime.InteropServices;
using Microsoft.VisualBasic.CompilerServices;

namespace Microsoft.VisualBasic
{
	[StandardModule] 
	[StructLayout(LayoutKind.Auto)] 
	public sealed class Strings
	{
		private Strings()
		{
		}
		public static int Asc(char String) 
		{
			//FIXME: Check the docs, it says something about Locales, DBCS, etc.

			//2003-12-29 JW
			//TODO: for some ideas/further documentation (currently not much), see the Strings test unit
			//		1. Byte count (switching of CurrentCulture isn't relevant but current machine's setting)
			//		2. Little or big endian
			//Tipp: use a western OS and at least a japanese Windows to do the testings!
			//

			return (int)String;
		}

		public static int Asc(string String)
		{
			if ((String == null) || (String.Length < 1))
				throw new ArgumentException("Length of argument 'String' must be at least one.", "String");

			return Asc(String[0]);
		}
		public static int AscW(char String) 
		{
			/*
			 * AscW returns the Unicode code point for the input character. 
			 * This can be 0 through 65535. The returned value is independent 
			 * of the culture and code page settings for the current thread.
			 */
			return (int) String;
		}
		public static int AscW(string String) 
		{
			/*
			 * AscW returns the Unicode code point for the input character. 
			 * This can be 0 through 65535. The returned value is independent 
			 * of the culture and code page settings for the current thread.
			 */
			if ((String == null) || (String.Length == 0))
				throw new ArgumentException("Length of argument 'String' must be at least one.", "String");

			return AscW(String[0]);
		}

		public static char Chr(int CharCode) 
		{

			// According to docs (ms-help://MS.VSCC/MS.MSDNVS/vblr7/html/vafctchr.htm)
			// Chr and ChrW should throw ArgumentException if ((CharCode < -32768) || (CharCode > 65535))
			// Instead, VB.net throws an OverflowException. I'm following the implementation
			// instead of the docs. 

			if ((CharCode < -32768) || (CharCode > 65535))
				throw new OverflowException("Value was either too large or too small for a character.");

			//FIXME: Check the docs, it says something about Locales, DBCS, etc.
			return System.Convert.ToChar(CharCode);
		}

		public static char ChrW(int CharCode ) 
		{
			/*
			 * According to docs ()
			 * Chr and ChrW should throw ArgumentException if ((CharCode < -32768) || (CharCode > 65535))
			 * Instead, VB.net throws an OverflowException. I'm following the implementation
			 * instead of the docs
			 */
			if ((CharCode < -32768) || (CharCode > 65535))
				throw new OverflowException("Value was either too large or too small for a character.");

			/*
			 * ChrW takes CharCode as a Unicode code point. The range is independent of the 
			 * culture and code page settings for the current thread. Values from -32768 through 
			 * -1 are treated the same as values in the range +32768 through +65535.
			 */
			if (CharCode < 0)
				CharCode += 0x10000;

			return System.Convert.ToChar(CharCode);
		}

		public static string[] Filter(object[] Source, 
					      string Match, 
					      [Optional, __DefaultArgumentValue(true)] 
					      bool Include,
					      [Optional, __DefaultArgumentValue((int)CompareMethod.Binary), OptionCompare] 
					      CompareMethod Compare)
		{
			if (Source == null)
				throw new ArgumentException("Argument 'Source' can not be null.", "Source");
			if (Source.Rank > 1)
				throw new ArgumentException("Argument 'Source' can have only one dimension.", "Source");

			string[] strings;
			strings = new string[Source.Length];

			Source.CopyTo(strings, 0);
			return Filter(strings, Match, Include, Compare);

		}

		public static string[] Filter(string[] Source, 
					      string Match, 
					      [Optional, __DefaultArgumentValue(true)] 
					      bool Include,
					      [Optional, __DefaultArgumentValue((int)CompareMethod.Binary)] 
					      CompareMethod Compare)
		{
			if (Source == null)
				throw new ArgumentException("Argument 'Source' can not be null.", "Source");
			if (Source.Rank > 1)
				throw new ArgumentException("Argument 'Source' can have only one dimension.", "Source");

			/*
			 * Well, I don't like it either. But I figured that two iterations
			 * on the array would be better than many aloocations. Besides, this
			 * way I can isolate the special cases.
			 * I'd love to hear from a different approach.
			 */

			int count = Source.Length;
			bool[] matches = new bool[count];
			int matchesCount = 0;

			for (int i = 0; i < count; i++)
			{
				int result = InStr(1, Source[i], Match, Compare);
				if (Include) {
					if (result != 0) {
						matches[i] = true;
						matchesCount ++;
					} else
						matches[i] = false;
				} else if (result == 0) {
					matches[i] = true;
					matchesCount ++;
				} else
					matches[i] = false;
			}

			if (matchesCount == count)
			{
				if (Include)
					return Source;
				else
					return new string[0];
			}

			string[] ret;
			int cnt = 0;
			for (int i = 0; i < count; i++) {
				if (matches [i] && Source [i] != null)
					cnt ++;
			}
			ret = new string [cnt];
			cnt = 0;
			for (int i=0; i < count; i++)
			{
				if (matches [i] && Source [i] != null)
				{
					ret[cnt] = Source[i];
					cnt++;
				}
			}
			return ret;
		}

		public static string Format(object expression, 
					    [Optional, __DefaultArgumentValue("")]string style)
		{
			string returnstr=null;
			string expstring=expression.GetType().ToString();
			switch(expstring)
			{
			case "System.Char":
				if ( style!="")
					throw new System.ArgumentException("'expression' argument has a not valid value");
				returnstr=Convert.ToChar(expression).ToString();
				break;
			case "System.String":
				if (style == "")
					returnstr=expression.ToString();
				else
				{
					switch ( style.ToLower ())
					{
					case "yes/no":
					case "on/off":
						switch (expression.ToString().ToLower())
						{
						case "true":
						case "on":
							if (style.ToLower ()=="yes/no")
								returnstr="Yes";
							else
								returnstr="On";
							break;
						case "false":
						case "off":
							if (style.ToLower ()=="yes/no")
								returnstr="No";
							else
								returnstr="Off";
							break;
						default:
							throw new System.ArgumentException();

						}
						break;
					default:
						returnstr=style.ToString();
						break;
					}
				}
				break;
			case "System.Boolean":
				if ( style=="")
				{
					if ( Convert.ToBoolean(expression)==true)
						returnstr="True"; 
					else
						returnstr="False";
				}
				else
					returnstr=style;
				break;
			case "System.DateTime":
				switch (style.ToLower ()){
				case "general date":
					style = "G"; break;
				case "long date":
					style = "D"; break;
				case "medium date":
					style = "D"; break;
				case "short date":
					style = "d"; break;
				case "long time":
					style = "T"; break;
				case "medium time":
					style = "T"; break;
				case "short time":
					style = "t"; break;
				}
				returnstr=Convert.ToDateTime(expression).ToString(style) ;
				break;
			case "System.Decimal":	case "System.Byte":	case "System.SByte":
			case "System.Int16":	case "System.Int32":	case "System.Int64":
			case "System.Double":	case "System.Single":	case "System.UInt16":
			case "System.UInt32":	case "System.UInt64":
				switch (style.ToLower ())
				{
				case "yes/no": case "true":	case "false": case "on/off":
					style=style.ToLower();
					double dblbuffer=Convert.ToDouble(expression);
					if (dblbuffer == 0)
					{
						switch (style)
						{
						case "on/off":
							returnstr= "Off";break; 
						case "yes/no":
							returnstr= "No";break; 
						case "true/false":
							returnstr= "False";break;
						}
					}
					else
					{
						switch (style)
						{
						case "on/off":
							returnstr="On";break;
						case "yes/no":
							returnstr="Yes";break;
						case "true/false":
							returnstr="True";break;
						}
					}
					break;
				default:
					if (style.IndexOf("X") != -1 
					    || style.IndexOf("x") != -1 ) {
						returnstr = Microsoft.VisualBasic.Conversion.Hex(Convert.ToInt64(expression));
					}
					else
						try 
						{
							returnstr=Convert.ToDouble(expression).ToString (style);
						}
					catch (Exception ex){
						style = "0" + style;
						returnstr=Convert.ToDouble(expression).ToString (style);
					}


					break;
				}
				break;
			}
			if (returnstr==null)
				throw new System.ArgumentException();
			return returnstr;
		}

		public static string FormatCurrency(object Expression, 
						    [Optional, __DefaultArgumentValue(-1)] 
						    int NumDigitsAfterDecimal, 
						    [Optional, __DefaultArgumentValue((int)TriState.UseDefault)] 
						    TriState IncludeLeadingDigit, 
						    [Optional, __DefaultArgumentValue((int)TriState.UseDefault)] 
						    TriState UseParensForNegativeNumbers, 
						    [Optional, __DefaultArgumentValue((int)TriState.UseDefault)] 
						    TriState GroupDigits)
		{
			if (NumDigitsAfterDecimal > 99 || NumDigitsAfterDecimal < -1 )
				throw new ArgumentException(
							    VBUtils.GetResourceString("Argument_Range0to99_1",
										      "NumDigitsAfterDecimal" ));       
											      
			if (Expression == null)
				return "";
															     
			if (!(Expression is IFormattable))
				throw new InvalidCastException(
							       VBUtils.GetResourceString("InvalidCast_FromStringTo",Expression.ToString(),"Double"));

			String formatStr = "00";

			if (GroupDigits == TriState.True)
				formatStr = formatStr + ",00";

			if (NumDigitsAfterDecimal > -1)	{
				string decStr = ".";
				for (int count=1; count<=NumDigitsAfterDecimal; count ++)
					decStr = decStr + "0";
			
				formatStr = formatStr + decStr;
			}

			if (UseParensForNegativeNumbers == TriState.True) {
				String temp = formatStr;
				formatStr = formatStr + ";(" ;
				formatStr = formatStr + temp;
				formatStr = formatStr + ")";
			}

			//Console.WriteLine("formatStr : " + formatStr);	

			string returnstr=null;
			string expstring= Expression.GetType().ToString();
			switch(expstring) {
			case "System.Decimal":	case "System.Byte":	case "System.SByte":
			case "System.Int16":	case "System.Int32":	case "System.Int64":
			case "System.Double":	case "System.Single":	case "System.UInt16":
			case "System.UInt32":	case "System.UInt64":
				returnstr = Convert.ToDouble(Expression).ToString (formatStr);
				break;
			default:
				throw new InvalidCastException(
							       VBUtils.GetResourceString("InvalidCast_FromStringTo",Expression.ToString(),"Double"));
			}
			String curSumbol = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
			returnstr = curSumbol + returnstr;
			
			return returnstr;
		}

		public static string FormatDateTime(DateTime Expression, 
						    [Optional, __DefaultArgumentValue((int)DateFormat.GeneralDate)] 
						    DateFormat NamedFormat)
		{
			switch(NamedFormat) {
			case DateFormat.GeneralDate:
				return Expression.ToString("G");
			case DateFormat.LongDate:  
				return Expression.ToString("D");
			case DateFormat.ShortDate:
				return Expression.ToString("d");
			case DateFormat.LongTime:
				return Expression.ToString("T");
			case DateFormat.ShortTime:
				return String.Format ("{0}:{1}", Expression.Hour, Expression.Minute);
			default:
				throw new ArgumentException("Argument 'NamedFormat' must be a member of DateFormat", "NamedFormat");
			}
		}

		public static string FormatNumber(object Expression, 
						  [Optional, __DefaultArgumentValue(-1)] 
						  int NumDigitsAfterDecimal, 
						  [Optional, __DefaultArgumentValue((int)TriState.UseDefault)] 
						  TriState IncludeLeadingDigit, 
						  [Optional, __DefaultArgumentValue((int)TriState.UseDefault)] 
						  TriState UseParensForNegativeNumbers, 
						  [Optional, __DefaultArgumentValue((int)TriState.UseDefault)] 
						  TriState GroupDigits)
		{
			if (NumDigitsAfterDecimal > 99 || NumDigitsAfterDecimal < -1 )
				throw new ArgumentException(
							    VBUtils.GetResourceString("Argument_Range0to99_1",
										      "NumDigitsAfterDecimal" ));       
											      
			if (Expression == null)
				return "";

			if (Expression is string)
				Expression = DoubleType.FromString ( (string) Expression);
			if (Expression is bool)
				Expression = - (Convert.ToDouble ((bool) Expression));

			if (!(Expression is IFormattable))
				throw new InvalidCastException(
							       VBUtils.GetResourceString("InvalidCast_FromStringTo",Expression.ToString(),"Double"));

			CultureInfo currentCulture = Utils.GetCultureInfo ();
			NumberFormatInfo ninfo = currentCulture.NumberFormat;

			if (NumDigitsAfterDecimal == -1)
				NumDigitsAfterDecimal = ninfo.NumberDecimalDigits;

			String formatStr = "";
			if (IncludeLeadingDigit == TriState.True)
				formatStr = "0";

			if (GroupDigits == TriState.UseDefault) {
				if (ninfo.NumberGroupSizes == null || ninfo.NumberGroupSizes.Length == 0)
					GroupDigits = TriState.False;
				else
					GroupDigits = TriState.True;
			}

			if (GroupDigits == TriState.True)
				formatStr = "0,0";

			if (NumDigitsAfterDecimal > -1)	{
				string decStr = ".";
				for (int count=1; count<=NumDigitsAfterDecimal; count ++)
					decStr = decStr + "0";
			
				formatStr = formatStr + decStr;
			}

			if (UseParensForNegativeNumbers == TriState.True) {
				String temp = formatStr;
				formatStr = formatStr + ";(" ;
				formatStr = formatStr + temp;
				formatStr = formatStr + ")";
			}

			//Console.WriteLine("formatStr : " + formatStr);	

			string returnstr=null;
			string expstring= Expression.GetType().ToString();
			switch(expstring) {
			case "System.Decimal":	case "System.Byte":	case "System.SByte":
			case "System.Int16":	case "System.Int32":	case "System.Int64":
			case "System.Double":	case "System.Single":	case "System.UInt16":
			case "System.UInt32":	case "System.UInt64":
				if (formatStr != "")
					returnstr = Convert.ToDouble(Expression).ToString (formatStr);
				else
					returnstr = Convert.ToDouble(Expression).ToString ();
				break;
			default:
				throw new InvalidCastException(
							       VBUtils.GetResourceString("InvalidCast_FromStringTo",Expression.ToString(),"Double"));
			}
			
			return returnstr;
		}

		public static string FormatPercent(object Expression, 
						   [Optional, __DefaultArgumentValue(-1)] 
						   int NumDigitsAfterDecimal, 
						   [Optional, __DefaultArgumentValue((int)TriState.UseDefault)] 
						   TriState IncludeLeadingDigit, 
						   [Optional, __DefaultArgumentValue((int)TriState.UseDefault)] 
						   TriState UseParensForNegativeNumbers, 
						   [Optional, __DefaultArgumentValue((int)TriState.UseDefault)] 
						   TriState GroupDigits)
		{
			if (NumDigitsAfterDecimal > 99 || NumDigitsAfterDecimal < -1 )
				throw new ArgumentException(
							    VBUtils.GetResourceString("Argument_Range0to99_1",
										      "NumDigitsAfterDecimal" ));       
											      
			if (Expression == null)
				return "";

			if (Expression is string)
				Expression = DoubleType.FromString ((string) Expression);

			if (!(Expression is IFormattable))
				throw new InvalidCastException(
							       VBUtils.GetResourceString("InvalidCast_FromStringTo",Expression.ToString(),"Double"));

			CultureInfo currentCulture = Utils.GetCultureInfo ();
			NumberFormatInfo ninfo = currentCulture.NumberFormat;

			if (NumDigitsAfterDecimal == -1)
				NumDigitsAfterDecimal = ninfo.NumberDecimalDigits;

			String formatStr = "";
			if (IncludeLeadingDigit == TriState.True)
				formatStr = "0";
			if (GroupDigits == TriState.UseDefault) {
				if (ninfo.NumberGroupSizes == null || ninfo.NumberGroupSizes.Length == 0)
					GroupDigits = TriState.False;
				else
					GroupDigits = TriState.True;
			}

			if (GroupDigits == TriState.True)
				formatStr = "0,0";

			if (NumDigitsAfterDecimal > -1) {
				string decStr = ".";
				for (int count=1; count<=NumDigitsAfterDecimal; count ++)
					decStr = decStr + "0";
			
				formatStr = formatStr + decStr;
			}

			if (UseParensForNegativeNumbers == TriState.True) {
				String temp = formatStr;
				formatStr = formatStr + ";(" ;
				formatStr = formatStr + temp;
				formatStr = formatStr + ")";
			}

			if (formatStr != "")
				formatStr = formatStr + "%";

			string returnstr=null;
			string expstring= Expression.GetType().ToString();
			switch(expstring) {
			case "System.Decimal":	case "System.Byte":	case "System.SByte":
			case "System.Int16":	case "System.Int32":	case "System.Int64":
			case "System.Double":	case "System.Single":	case "System.UInt16":
			case "System.UInt32":	case "System.UInt64":
				if (formatStr != "")
					returnstr = Convert.ToDouble(Expression).ToString (formatStr);
				else
					returnstr = Convert.ToDouble(Expression).ToString ();
				break;
			default:
				throw new InvalidCastException(
							       VBUtils.GetResourceString("InvalidCast_FromStringTo",Expression.ToString(),"Double"));
			}
			
			return returnstr;
		}

		public static char GetChar(string Str, 
					   int Index)
		{

			if ((Str == null) || (Str.Length == 0))
				throw new ArgumentException("Length of argument 'Str' must be greater than zero.", "Sre");
			if (Index < 1) 
				throw new ArgumentException("Argument 'Index' must be greater than or equal to 1.", "Index");
			if (Index > Str.Length)
				throw new ArgumentException("Argument 'Index' must be less than or equal to the length of argument 'String'.", "Index");

			return Str.ToCharArray(Index -1, 1)[0];
		}

		public static int InStr(string String1, 
					string String2, 
					[Optional, __DefaultArgumentValue((int)CompareMethod.Binary), OptionCompare] 
					CompareMethod Compare)
		{
			return InStr(1, String1, String2, Compare);
		}
		
		public static int InStr(int Start, 
					string String1, 
					string String2, 
					[Optional, __DefaultArgumentValue((int)CompareMethod.Binary), OptionCompare] 
					CompareMethod Compare)
		{
			if (Start < 1)
				throw new ArgumentException("Argument 'Start' must be non-negative.", "Start");
			
			int leng = 0;
			if (String1 != null) {
				leng = String1.Length;
			}
			if (Start > leng || leng == 0){
				return 0;
			}
			if (String2 == null || String2.Length == 0) {
				return Start;
			}

			switch (Compare) {
			case CompareMethod.Text:
				return System.Globalization.CultureInfo.CurrentCulture.CompareInfo.IndexOf(
													   String1.ToLower(System.Globalization.CultureInfo.CurrentCulture), 
													   String2.ToLower(System.Globalization.CultureInfo.CurrentCulture)
													   , Start - 1) + 1;
			case CompareMethod.Binary:
				return (String1.IndexOf(String2, Start - 1)) + 1;
			default:
				throw new System.ArgumentException("Argument 'Compare' must be CompareMethod.Binary or CompareMethod.Text.", "Compare");
			}
		}

		public static int InStrRev(string StringCheck, 
					   string StringMatch, 
					   [Optional, __DefaultArgumentValue(-1)] 
					   int Start,
					   [Optional, __DefaultArgumentValue((int)CompareMethod.Binary), OptionCompare] 
					   CompareMethod Compare)
		{
			if ((Start == 0) || (Start < -1))
				throw new ArgumentException("Argument 'Start' must be greater than 0 or equal to -1", "Start");

			if (StringCheck == null)
				return 0;
							
			if (Start == -1)
				Start = StringCheck.Length;
										
			if (StringMatch == null || StringMatch.Length == 0)
				return Start;

			if (Start > StringCheck.Length || StringCheck.Length == 0)
				return 0;																		  

			if (Compare == CompareMethod.Text) {
				// FIXME: this wastes memory and time, remove when CompareInfo.LastIndexOf works correctly 
				StringCheck = StringCheck.ToLower();
				StringMatch = StringMatch.ToLower();
				// FIXME: depends on Managed Collation being ready to be able to use with CompareOptions.IgnoreCase
				// return CultureInfo.CurrentCulture.CompareInfo.LastIndexOf(StringCheck, StringMatch, Start - 1, CompareOptions.IgnoreCase) + 1; 
			}
			return StringCheck.LastIndexOf(StringMatch, Start - 1) + 1;
		}

		public static string Join(string[] SourceArray, 
					  [Optional, __DefaultArgumentValue(" ")] 
					  string Delimiter)
		{
			if (SourceArray == null)
				throw new ArgumentException("Argument 'SourceArray' can not be null.", "SourceArray");
			if (SourceArray.Rank > 1)
				throw new ArgumentException("Argument 'SourceArray' can have only one dimension.", "SourceArray");

			return string.Join(Delimiter, SourceArray);
		}

		public static string Join(object[] SourceArray, 
					  [Optional, __DefaultArgumentValue(" ")] 
					  string Delimiter)
		{
			try 
			{
				if (SourceArray == null)
					throw new ArgumentException("Argument 'SourceArray' can not be null.", "SourceArray");
				if (SourceArray.Rank > 1)
					throw new ArgumentException("Argument 'SourceArray' can have only one dimension.", "SourceArray");

				string[] dest;
				dest = new string[SourceArray.Length];

				SourceArray.CopyTo(dest, 0);
				return string.Join(Delimiter, dest);
			}
			catch (System.InvalidCastException ie){
				throw new System.ArgumentException("Invalid argument");
			}
		}

		public static char LCase(char Value) 
		{
			return char.ToLower(Value);
		}

		public static string LCase(string Value) 
		{
			if ((Value == null) || (Value.Length == 0)) 
				return Value; // comparing nunit test results say this is an exception to the return String.Empty rule

			return Value.ToLower();
		}

		public static string Left(string Str, int Length) 
		{
			if (Length < 0)
				throw new ArgumentException("Argument 'Length' must be non-negative.", "Length");
			if ((Str == null) || (Str.Length == 0) || Length == 0)
				return String.Empty; // VB.net does this.
			if (Length < Str.Length)
				return Str.Substring(0, Length);
			return Str;
		}

		public static int Len(bool Expression) 
		{
			return 2; //sizeof(bool)
		}

		public static int Len(byte Expression) 
		{
			return 1; //sizeof(byte)
		}
		
		public static int Len(char Expression) 
		{
			return 2; //sizeof(char)
		}
		
		public static int Len(double Expression) 
		{
			return 8; //sizeof(double)
		}
		
		public static int Len(int Expression) 
		{
			return 4; //sizeof(int)
		}
		
		public static int Len(long Expression) 
		{
			return 8; //sizeof(long)
		}

		public static int Len(object expression) 
		{
			IConvertible convertible = null;
				
			if (expression == null)
				return 0;
							       
			if (expression is String)
				return ((String)expression).Length;
												
			if (expression is char[])
				return ((char[])expression).Length;
																 
			if (expression is IConvertible)
				convertible = (IConvertible)expression;
																					
			if (convertible != null) {
				switch (convertible.GetTypeCode()) {
				case TypeCode.String :
					return expression.ToString().Length;
				case TypeCode.Int16 :
					return 2;
				case TypeCode.Byte :
					return 1;
				case TypeCode.Int32 :
					return 4;
				case TypeCode.Int64 :
					return 8;
				case TypeCode.Single :
					return 4;
				case TypeCode.Double :
					return 8;
				case TypeCode.Boolean :
					return 2;
				case TypeCode.Decimal :
					return 16;
				case TypeCode.Char :
					return 2;
				case TypeCode.DateTime :
					return 8;
				}
					   
			}
			if (expression is ValueType)
				return System.Runtime.InteropServices.Marshal.SizeOf(expression);
																													     
			throw new InvalidCastException(VBUtils.GetResourceString(13));
		}
		
		public static int Len(short Expression) 
		{
			return 2; //sizeof(short)
		}
		
		public static int Len(Single Expression) 
		{
			return 4; //sizeof(Single)
		}
		
		public static int Len(string Expression) {
			if (Expression == null)return 0;
			return Expression.Length;
		}

		public static int Len(DateTime Expression) 
		{
			return 8; //sizeof(DateTime)
		}
		
		public static int Len(decimal Expression) 
		{
			return 8; //sizeof(decimal)
		}

		public static string LSet(string Source, 
					  int Length) 
		{
			if (Length < 0)
				throw new ArgumentOutOfRangeException("Length", "Length must be must be non-negative.");

			if (Source == null)
				Source = String.Empty;

			if (Length > Source.Length)
				return Source.PadRight(Length);

			return Source.Substring(0, Length);
		}

		public static string LTrim(string Str) 
		{
			if ((Str == null) || (Str.Length == 0))
				return String.Empty; // VB.net does this.

			return Str.TrimStart(null);
		}

		public static string RTrim(string Str) 
		{
			if ((Str == null) || (Str.Length == 0))
				return String.Empty; // VB.net does this.

			return Str.TrimEnd(null);
		}

		public static string Trim(string Str) 
		{
			if ((Str == null) || (Str.Length == 0))
				return String.Empty; // VB.net does this.
			
			return Str.Trim();
		}

		public static string Mid(string Str, 
					 int Start, 
					 int Length)
		{

			if (Length < 0)
				throw new System.ArgumentException("Argument 'Length' must be greater or equal to zero.", "Length");
			if (Start <= 0)
				throw new System.ArgumentException("Argument 'Start' must be greater than zero.", "Start");
			if ((Str == null) || (Str.Length == 0))
				return String.Empty; // VB.net does this.

			if ((Length == 0) || (Start > Str.Length))
				return String.Empty;

			if (Length > (Str.Length - Start))
				Length = (Str.Length - Start) + 1;

			return Str.Substring(Start - 1, Length);

		}

		public static string Mid (string Str, int Start) 
		{
			if (Start <= 0)
				throw new System.ArgumentException("Argument 'Start' must be greater than zero.", "Start");

			if ((Str == null) || (Str.Length == 0))
				return String.Empty; // VB.net does this.

			if (Start > Str.Length)
				return String.Empty;

			return Str.Substring(Start - 1);
		}

		public static string Replace(string Expression, 
					     string Find, 
					     string Replacement, 
					     [Optional, __DefaultArgumentValue(1)] 
					     int Start,
					     [Optional, __DefaultArgumentValue(-1)] 
					     int Count,
					     [Optional, __DefaultArgumentValue((int)CompareMethod.Binary), OptionCompare] 
					     CompareMethod Compare)
		{

			if (Count < -1)
				throw new ArgumentException("Argument 'Count' must be greater than or equal to -1.", "Count");
			if (Start <= 0)
				throw new ArgumentException("Argument 'Start' must be greater than zero.", "Start");

			if ((Expression == null) || (Expression.Length == 0))
				return String.Empty; // VB.net does this.
			if ((Find == null) || (Find.Length == 0))
				return Expression; // VB.net does this.
			if (Replacement == null)
				Replacement = String.Empty; // VB.net does this.

			return Expression.Replace(Find, Replacement);
		}

		public static string Right(string Str, 
					   int Length) 
		{
			if (Length < 0)
				throw new ArgumentException("Argument 'Length' must be greater or equal to zero.", "Length");

			// Fixing Bug #49660 - Start
			if ((Str == null) || (Str.Length == 0))
				return String.Empty; // VB.net does this.

			if (Length >= Str.Length)
				return Str;
			// Fixing Bug #49660 - End

			return Str.Substring (Str.Length - Length);
		}

		public static string RSet(string Source, int Length) 
		{
		
			if (Source == null)
				Source = String.Empty;
			if (Length < 0)
				throw new ArgumentOutOfRangeException("Length", "Length must be non-negative.");
			if (Length > Source.Length)
				return Source.PadLeft(Length);
			return Source.Substring(0, Length);
		}

		public static string Space(int Number) 
		{
			if (Number < 0)
				throw new ArgumentException("Argument 'Number' must be greater or equal to zero.", "Number");

			return new string((char) ' ', Number);
		}

		public static string[] Split(string Expression, 
					     [Optional, __DefaultArgumentValue(" ")] 
					     string Delimiter,
					     [Optional, __DefaultArgumentValue(-1)] 
					     int Limit,
					     [Optional, __DefaultArgumentValue((int)CompareMethod.Binary), OptionCompare] 
					     CompareMethod Compare)
		{
			if (Expression == null)
				return new string[1];

			if ((Delimiter == null) || (Delimiter.Length == 0)){
				string [] ret = new string[1];
				ret[0] = Expression;
				return ret;
			}
			if (Limit == 0)
				Limit = 1; 
			else if (Limit < -1)
				throw new OverflowException("Arithmetic operation resulted in an overflow.");

			if (Limit != -1) {
				switch (Compare){
				case CompareMethod.Binary:
					return Expression.Split(Delimiter.ToCharArray(0, 1), Limit);
				case CompareMethod.Text:
					return Expression.Split(Delimiter.ToCharArray(0, 1), Limit);
				default:
					throw new System.ArgumentException("Argument 'Compare' must be CompareMethod.Binary or CompareMethod.Text.", "Compare");
				}
			} else {
				switch (Compare) {
				case CompareMethod.Binary:
					return Expression.Split(Delimiter.ToCharArray(0, 1));
				case CompareMethod.Text:
					return Expression.Split(Delimiter.ToCharArray(0, 1));
				default:
					throw new System.ArgumentException("Argument 'Compare' must be CompareMethod.Binary or CompareMethod.Text.", "Compare");
				}
			}
		}

		public static int StrComp(string String1, 
					  string String2,
					  [Optional, __DefaultArgumentValue((int)CompareMethod.Binary), OptionCompare] 
					  CompareMethod Compare)
		{
			if (String1 == null)
				String1 = string.Empty;
			if (String2 == null)
				String2 = string.Empty;

			switch (Compare)
			{
			case CompareMethod.Binary:
				return string.Compare(String1, String2, false);
			case CompareMethod.Text:
				CultureInfo curCulture = CultureInfo.CurrentCulture;
				return curCulture.CompareInfo.Compare(String1.ToLower(curCulture), String2.ToLower(curCulture));
			default:
				throw new System.ArgumentException("Argument 'Compare' must be CompareMethod.Binary or CompareMethod.Text", "Compare");
			}
		}

		public static string StrConv (string str, 
					      VbStrConv Conversion, 
					      [Optional, __DefaultArgumentValue(0)]
					      int LocaleID)
		{
			if (str == null)
				throw new ArgumentNullException("str");
					
			if (Conversion == VbStrConv.None){
				return str;
			}
			else if (Conversion == VbStrConv.UpperCase) {
				return str.ToUpper();
			}
			else if (Conversion == VbStrConv.LowerCase) {
				return str.ToLower();
			}
			else if (Conversion == VbStrConv.ProperCase) {
				String[] arr = str.Split(null);
				String tmp = "" ;
				for (int i =0 ; i < (arr.Length - 1) ; i++){
					arr[i] =  arr[i].ToLower();
					tmp +=  arr[i].Substring(0,1).ToUpper() + arr[i].Substring(1) + " ";
				}
				arr[arr.Length - 1] =  arr[arr.Length - 1].ToLower();
				tmp +=  arr[arr.Length - 1].Substring(0,1).ToUpper() + arr[arr.Length - 1].Substring(1);
											
				return tmp;
			}         
			else if (Conversion == VbStrConv.SimplifiedChinese || 
				 Conversion == VbStrConv.TraditionalChinese ) 
				return str;
			else
				throw new ArgumentException("Unsuported conversion in StrConv");	
		}

		public static string StrDup(int Number, 
					    char Character)
		{
			if (Number < 0)
				throw new ArgumentException("Argument 'Number' must be non-negative.", "Number");

			return new string(Character, Number);
		}

		public static string StrDup(int Number, 
					    string Character)
		{
			if (Number < 0)
				throw new ArgumentException("Argument 'Number' must be greater or equal to zero.", "Number");
			if ((Character == null) || (Character.Length == 0))
				throw new ArgumentNullException("Character", "Length of argument 'Character' must be greater than zero.");

			return new string(Character[0], Number);
		}

		public static object StrDup(int Number, 
					    object Character)
		{
			if (Number < 0)
				throw new ArgumentException("Argument 'Number' must be non-negative.", "Number");
			
			if (Character is string)
			{
				string sCharacter = (string) Character;
				if ((sCharacter == null) || (sCharacter.Length == 0))
					throw new ArgumentNullException("Character", "Length of argument 'Character' must be greater than zero.");

				return StrDup(Number, sCharacter);
			}
			else
			{
				if (Character is char)
				{
					return StrDup(Number, (char) Character);
				}
				else
				{
					// "If Character is of type Object, it must contain either a Char or a String value."
					throw new ArgumentException("Argument 'Character' is not a valid value.", "Character");
				}
			}
		}

		public static string StrReverse(string Expression)
		{
			// Patched by Daniel Campos (danielcampos@myway.com)
			// Simplified by Rafael Teixeira (2003-12-02)
			if (Expression == null || Expression.Length < 1)
				return String.Empty;
			else {
				int length = Expression.Length;
				char[] buf = new char[length];
				int counter = 0;
				int backwards = length - 1;
				while (counter < length)
					buf[counter++] = Expression[backwards--];
				return new string(buf);
			}
		}

		public static char UCase(char Value) 
		{
			return char.ToUpper(Value);
		}

		public static string UCase(string Value) 
		{
			if ((Value == null) || (Value.Length == 0))
				return String.Empty; // VB.net does this. 

			return Value.ToUpper();
		}
	}
}
