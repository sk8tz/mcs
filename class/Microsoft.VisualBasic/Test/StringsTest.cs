//
// StringsTest.cs
//
// Authors:
//   Jochen Wezel (jwezel+AEA-compumaster.de)
//
// (C) 2003 Jochen Wezel, CompuMaster GmbH (http://www.compumaster.de/)
//

using Microsoft.VisualBasic;
using System;
using System.Text;
using System.Globalization;
using NUnit.Framework;

namespace Microsoft.VisualBasic
{
	[TestFixture]
	public class StringsTest : Assertion 
	{
		private string TextStringOfMultipleLanguages;
		private string TextStringUninitialized;
		const string TextStringEmpty = "";

		//Disclaimer: I herewith distance me and the whole Mono project of text written in this test strings - they are really only for testing purposes and are copy and pasted of randomly found test parts of several suriously looking websites
		const string MSWebSiteContent_English = "Choose the location for which you want contact information:";
		const string MSWebSiteContent_Japanese = "電子メールをはじめ、予定表やアドレス帳などの個人情報、さらには Office v. X の各アプリケーションで作成した書類やその他のファイルをリンクさせて統合的に管理することができるコミュニケーション／個人情報管理ソフトウェアです。インターフェースが一新され、高度で多彩な Entourage X の機能がより直感的に利用できるようになりました。以前お使いになっていた電子メール アプリケーションからの情報のインポートも容易にできます。";
		const string MSWebSiteContent_Russian = "будете автоматически перенаправлены";
		const string MSWebSiteContent_Slovakian = "čníci náročných používateľovNové portfólio Microsoft Hardware. špičkové optické bezdrôtové myšky s novou technológiou - nakláňacím kolieskom. Nezáleží na tom, aký stôl máte, pokiaľ je na ňom elegantná";
		const string MSWebSiteContent_Korean = "보안 캠페인 - 스스로 지킨 당신의 PC! 더욱 안전해집니다!";
		const string ArabynetComWebSiteContent_Arabic = "كل الحقوق محفوظة ليديعوت إنترنت";
		const string GermanUmlauts_German = "äöüÄÖÜß";

		private char Letter_Empty;
		private char Letter_English;
		private char Letter_Japanese;
		private char Letter_Russian;
		private char Letter_Slovakian;
		private char Letter_Korean;
		private char Letter_Arabic;
		private char Letter_German;

		[SetUp]
		public void Setup()
		{		
			TextStringOfMultipleLanguages = 
				MSWebSiteContent_Japanese + 
				MSWebSiteContent_Russian +
				MSWebSiteContent_Slovakian +
				MSWebSiteContent_Korean +
				ArabynetComWebSiteContent_Arabic +
				GermanUmlauts_German;
			Letter_English = MSWebSiteContent_English[0];
			Letter_Japanese = MSWebSiteContent_Japanese[0];
			Letter_Russian = MSWebSiteContent_Russian[0];
			Letter_Slovakian = MSWebSiteContent_Slovakian[0];
			Letter_Korean = MSWebSiteContent_Korean[0];
			Letter_Arabic = ArabynetComWebSiteContent_Arabic[0];
			Letter_German = GermanUmlauts_German[0];
		}


		
		[Test]
		public void Asc_Char() 
		{
			NUnit.Framework.Assertion.AssertEquals ("JW#26", 0, Strings.Asc(Letter_Empty));
			NUnit.Framework.Assertion.AssertEquals ("JW#27 - Quotation mark test", 34, Strings.Asc("\""[0]));
			NUnit.Framework.Assertion.AssertEquals ("JW#28 - JapaneseCharacter", 63, Strings.Asc(MSWebSiteContent_Japanese[0]));
			NUnit.Framework.Assertion.AssertEquals ("JW#28a - GermanCharacter", 228, Strings.Asc(Letter_German));

			/*
			//FIXME: Check the docs, it says something about Locales, DBCS, etc.
			return (int)Char;
			*/
		}


		[Test]
		public void Asc_String() 
		{
			NUnit.Framework.Assertion.AssertEquals ("JW#01", 63, Strings.Asc(this.TextStringOfMultipleLanguages));
			NUnit.Framework.Assertion.AssertEquals ("JW#02", 99, Strings.Asc(MSWebSiteContent_Slovakian));
			NUnit.Framework.Assertion.AssertEquals ("JW#03", 63, Strings.Asc(MSWebSiteContent_Japanese));
			NUnit.Framework.Assertion.AssertEquals ("JW#04", 63, Strings.Asc(ArabynetComWebSiteContent_Arabic));
			NUnit.Framework.Assertion.AssertEquals ("JW#05", 63, Strings.Asc(MSWebSiteContent_Korean));
			NUnit.Framework.Assertion.AssertEquals ("JW#06", 63, Strings.Asc(MSWebSiteContent_Russian));
			NUnit.Framework.Assertion.AssertEquals ("JW#07", 67, Strings.Asc(MSWebSiteContent_English));
			try
			{
				object buffer = Strings.Asc(TextStringEmpty);
				NUnit.Framework.Assertion.Fail ("JW#08 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#08", true);
			}
			try
			{
				object buffer = Strings.Asc(null);
				NUnit.Framework.Assertion.Fail ("JW#09 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#09", true);
			}
			try
			{
				object buffer = Strings.Asc(TextStringUninitialized);
				NUnit.Framework.Assertion.Fail ("JW#10 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#10", true);
			}
			NUnit.Framework.Assertion.AssertEquals ("JW#11 - Quotation mark test", 34, Strings.Asc("\""));
			
				
			/*
			if ((String +AD0APQ- null) +AHwAfA- (String.Length +ADw- 1))
				throw new ArgumentException("Length of argument 'String' must be at least one.", "String");

			//FIXME: Check the docs, it says something about Locales, DBCS, etc.
			return (int) String.ToCharArray(0, 1)[0+AF0AOw-
			//why? check http://bugzilla.ximian.com/show_bug.cgi?id=23540
			*/
		}


		[Test]
		public void AscW_Char() 
		{
			NUnit.Framework.Assertion.AssertEquals ("JW#12", 0, Strings.AscW(Letter_Empty));
			NUnit.Framework.Assertion.AssertEquals ("JW#13 - Quotation mark test", 34, Strings.AscW("\""[0]));
			NUnit.Framework.Assertion.AssertEquals ("JW#14 - JapaneseCharacter", 38651, Strings.AscW(Letter_Japanese));
			NUnit.Framework.Assertion.AssertEquals ("JW#14a - ArabicCharacter", 1603, Strings.AscW(Letter_Arabic));
			NUnit.Framework.Assertion.AssertEquals ("JW#14b - GermanCharacter", 228, Strings.AscW(Letter_German));

			/*
			 
			// * AscW returns the Unicode code point for the input character. 
			// * This can be 0 through 65535. The returned value is independent 
			// * of the culture and code page settings for the current thread.

			return (int) String;
			*/
		}
		
		[Test]
		public void AscW_String() 
		{
			NUnit.Framework.Assertion.AssertEquals ("JW#15", 38651, Strings.AscW(this.TextStringOfMultipleLanguages));
			NUnit.Framework.Assertion.AssertEquals ("JW#16", 269, Strings.AscW(MSWebSiteContent_Slovakian));
			NUnit.Framework.Assertion.AssertEquals ("JW#17", 38651, Strings.AscW(MSWebSiteContent_Japanese));
			NUnit.Framework.Assertion.AssertEquals ("JW#18", 1603, Strings.AscW(ArabynetComWebSiteContent_Arabic));
			NUnit.Framework.Assertion.AssertEquals ("JW#19", 48372, Strings.AscW(MSWebSiteContent_Korean));
			NUnit.Framework.Assertion.AssertEquals ("JW#20", 1073, Strings.AscW(MSWebSiteContent_Russian));
			NUnit.Framework.Assertion.AssertEquals ("JW#21", 67, Strings.AscW(MSWebSiteContent_English));
			try
			{
				object buffer = Strings.AscW(TextStringEmpty);
				NUnit.Framework.Assertion.Fail ("JW#22 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#22", true);
			}
			try
			{
				object buffer = Strings.AscW(null);
				NUnit.Framework.Assertion.Fail ("JW#23 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#23", true);
			}
			try
			{
				object buffer = Strings.AscW(TextStringUninitialized);
				NUnit.Framework.Assertion.Fail ("JW#24 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#24", true);
			}
			NUnit.Framework.Assertion.AssertEquals ("JW#25 - Quotation mark test", 34, Strings.AscW("\""));

			/*

			// * AscW returns the Unicode code point for the input character. 
			// * This can be 0 through 65535. The returned value is independent 
			// * of the culture and code page settings for the current thread.
			if ((String +AD0APQ- null) +AHwAfA- (String.Length +AD0APQ- 0))
				throw new ArgumentException("Length of argument 'String' must be at leasr one.", "String");
			return (int) String.ToCharArray(0, 1)[0+AF0AOw-
			*/
		}

		[Test]
		public void Chr() 
		{
			NUnit.Framework.Assertion.AssertEquals ("JW#29", "@"[0], Strings.Chr(64));
			try
			{
				object buffer = Strings.Chr(38651);
				NUnit.Framework.Assertion.Fail ("JW#30 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#30", true);
			}
			NUnit.Framework.Assertion.AssertEquals ("JW#31 - Quotation mark test", "\""[0], Strings.Chr(34));
			/*
			// According to docs (ms-help://MS.VSCC/MS.MSDNVS/vblr7/html/vafctchr.htm)
			// Chr and ChrW should throw ArgumentException if ((CharCode +ADw- -32768) +AHwAfA- (CharCode +AD4- 65535))
			// Instead, VB.net throws an OverflowException. I'm following the implementation
			// instead of the docs. 

			if ((CharCode +ADw- -32768) +AHwAfA- (CharCode +AD4- 65535))
				throw new OverflowException("Value was either too large or too small for a character.");

			//FIXME: Check the docs, it says something about Locales, DBCS, etc.
			return System.Convert.ToChar(CharCode);
			*/
		}

		[Test]
		public void ChrW() 
		{
			NUnit.Framework.Assertion.AssertEquals ("JW#32", "@"[0], Strings.ChrW(64));
			NUnit.Framework.Assertion.AssertEquals ("JW#33", "電"[0], Strings.ChrW(38651));
			NUnit.Framework.Assertion.AssertEquals ("JW#34 - Quotation mark test", "\""[0], Strings.ChrW(34));
			/*
			// * According to docs ()
			// * Chr and ChrW should throw ArgumentException if ((CharCode +ADw- -32768) +AHwAfA- (CharCode +AD4- 65535))
			// * Instead, VB.net throws an OverflowException. I'm following the implementation
			// * instead of the docs
			if ((CharCode +ADw- -32768) +AHwAfA- (CharCode +AD4- 65535))
				throw new OverflowException("Value was either too large or too small for a character.");

			// * ChrW takes CharCode as a Unicode code point. The range is independent of the 
			// * culture and code page settings for the current thread. Values from -32768 through 
			// * -1 are treated the same as values in the range +32768 through +65535.
			if (CharCode +ADw- 0)
				CharCode += 0x10000;

			return System.Convert.ToChar(CharCode);
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void Filter_Objects()
		{
			/*
			if (Source +AD0APQ- null)
				throw new ArgumentException("Argument 'Source' can not be null.", "Source");
			if (Source.Rank +AD4- 1)
				throw new ArgumentException("Argument 'Source' can have only one dimension.", "Source");

			string+AFsAXQ- strings;
			strings = new string[Source.Length+AF0AOw-

			Source.CopyTo(strings, 0);
			return Filter(strings, Match, Include, Compare);
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void Filter_Strings()
		{
			/*
			if (Source +AD0APQ- null)
				throw new ArgumentException("Argument 'Source' can not be null.", "Source");
			if (Source.Rank +AD4- 1)
				throw new ArgumentException("Argument 'Source' can have only one dimension.", "Source");

			 //* Well, I don't like it either. But I figured that two iterations
			 //* on the array would be better than many aloocations. Besides, this
			 //* way I can isolate the special cases.
			 //* I'd love to hear from a different approach.
			 
			int count = Source.Length;
			bool+AFsAXQ- matches = new bool[count+AF0AOw-
			int matchesCount = 0;

			for (int i = 0; i +ADw- count; i++)
			{
				if (InStr(1, Match, Source[i], Compare) +ACEAPQ- 0)
				{
					//found one more
					matches[i] = true;
					matchesCount ++;
				}
				else
				{
					matches[i] = false;
				}
			}

			if (matchesCount +AD0APQ- 0)
			{
				if (Include)
					return new string[0+AF0AOw-
				else
					return Source;
			}
			else
			{
				if (matchesCount +AD0APQ- count)
				{
					if (Include)
						return Source;
					else
						return new string[0+AF0AOw-
				}
				else
				{
					string+AFsAXQ- ret;
					int j = 0;
					if (Include)
						ret = new string [matchesCount+AF0AOw-
					else
						ret = new string [count - matchesCount+AF0AOw-

					for (int i=0; i +ADw- count; i++)
					{
						if ((matches[i] +ACYAJg- Include) +AHwAfA- +ACE-(matches[i] +AHwAfA- Include))
						{
							ret[j] = Source[i+AF0AOw-
							j++;
						}
					}
					return ret;
				}
			}
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void Format()
		{
			/*
			string returnstr=null;
			string expstring=expression.GetType().ToString()+ADsAOw-
			switch(expstring)
			{
				case "System.Char":
					if ( style+ACEAPQAiACI-)
						throw new System.ArgumentException("'expression' argument has a not valid value");
					returnstr=Convert.ToChar(expression).ToString();
					break;
				case "System.String":
					if (style +AD0APQ- +ACIAIg-)
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
								case "On":
									if (style.ToLower ()+AD0APQAi-yes/no")
										returnstr+AD0AIg-Yes+ACIAOw- // TODO : must be translated
									else
										returnstr+AD0AIg-On+ACIAOw- // TODO : must be translated
									break;
								case "false":
								case "off":
									if (style.ToLower ()+AD0APQAi-yes/no")
										returnstr+AD0AIg-No+ACIAOw- // TODO : must be translated
									else
										returnstr+AD0AIg-Off+ACIAOw- // TODO : must be translated
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
					if ( style+AD0APQAiACI-)
					{
						if ( Convert.ToBoolean(expression)+AD0APQ-true)
							returnstr+AD0AIg-True+ACIAOw- // must not be translated
						else
							returnstr+AD0AIg-False+ACIAOw- // must not be translated
					}
					else
						returnstr=style;
					break;
				case "System.DateTime":
					returnstr=Convert.ToDateTime(expression).ToString (style) ;
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
						if (dblbuffer +AD0APQ- 0)
						{
							switch (style)
							{
								case "on/off":
									returnstr= "Off+ACIAOw-break; // TODO : must be translated
								case "yes/no":
									returnstr= "No+ACIAOw-break; // TODO : must be translated
								case "true":
								case "false":
									returnstr= "False+ACIAOw-break; // must not be translated
							}
						}
						else
						{
							switch (style)
							{
								case "on/off":
									returnstr+AD0AIg-On+ACIAOw-break; // TODO : must be translated
								case "yes/no":
									returnstr+AD0AIg-Yes+ACIAOw-break; // TODO : must be translated
								case "true":
								case "false":
									returnstr+AD0AIg-True+ACIAOw-break; // must not be translated
							}
						}
						break;
					default:
					switch (expstring)
					{
						case "System.Byte": returnstr=Convert.ToByte(expression).ToString (style);break;
						case "System.SByte": returnstr=Convert.ToSByte(expression).ToString (style);break;
						case "System.Int16": returnstr=Convert.ToInt16(expression).ToString (style);break;
						case "System.UInt16": returnstr=Convert.ToUInt16(expression).ToString (style);break;
						case "System.Int32":  returnstr=Convert.ToInt32(expression).ToString (style);break;
						case "System.UInt32":  returnstr=Convert.ToUInt32(expression).ToString (style);break;
						case "System.Int64":  returnstr=Convert.ToUInt64(expression).ToString (style);break;
						case "System.UInt64":returnstr=Convert.ToUInt64(expression).ToString (style);break;
						case "System.Single": returnstr=Convert.ToSingle(expression).ToString (style);break;
						case "System.Double":  returnstr=Convert.ToDouble(expression).ToString (style);break;
						case "System.Decimal": returnstr=Convert.ToDecimal(expression).ToString (style);break;

					}
						break;
				}
					break;
			}
			if (returnstr+AD0APQ-null)
				throw new System.ArgumentException();
			return returnstr;
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void FormatCurrency()
		{
			/*
			//FIXME
			throw new NotImplementedException();
			//throws InvalidCastException
			//throws ArgumentException
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void FormatDateTime()
		{
			/*
			switch(NamedFormat)
			{
				case DateFormat.GeneralDate:
					//FIXME: WTF should I do with it?
					throw new NotImplementedException(); 	
				case DateFormat.LongDate:  
					return Expression.ToLongDateString();
				case DateFormat.ShortDate:
					return Expression.ToShortDateString();
				case DateFormat.LongTime:
					return Expression.ToLongTimeString();
				case DateFormat.ShortTime:
					return Expression.ToShortTimeString();
				default:
					throw new ArgumentException("Argument 'NamedFormat' must be a member of DateFormat", "NamedFormat");
			}
			*/
		}

		[Test]
		public void FormatNumber()
		{
			// buffer current culture
			System.Globalization.CultureInfo CurCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

			// do testings
			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
			NUnit.Framework.Assertion.AssertEquals ("JW#60", "1.000", Strings.FormatNumber(1000,0,TriState.False,TriState.False,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#61", "1000", Strings.FormatNumber(1000,0,TriState.False,TriState.False,TriState.False));
			NUnit.Framework.Assertion.AssertEquals ("JW#62", "1.000", Strings.FormatNumber(1000,0,TriState.True,TriState.False,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#63", "1.000", Strings.FormatNumber(1000,0,TriState.False,TriState.True,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#64", "1.000", Strings.FormatNumber(1000,0,TriState.True,TriState.True,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#65", "1000", Strings.FormatNumber(1000,0,TriState.False,TriState.False,TriState.False));
			NUnit.Framework.Assertion.AssertEquals ("JW#66", "1.000", Strings.FormatNumber(1000,0,TriState.False,TriState.False,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#67", "-1000", Strings.FormatNumber(-1000,0,TriState.False,TriState.False,TriState.False));
			NUnit.Framework.Assertion.AssertEquals ("JW#68", "-1.000", Strings.FormatNumber(-1000,0,TriState.True,TriState.False,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#69", "(1.000)", Strings.FormatNumber(-1000,0,TriState.False,TriState.True,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#70", "(1.000)", Strings.FormatNumber(-1000,0,TriState.True,TriState.True,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#71", "-1000", Strings.FormatNumber(-1000,0,TriState.False,TriState.False,TriState.False));
			NUnit.Framework.Assertion.AssertEquals ("JW#72", "(1.000,0000)", Strings.FormatNumber(-1000,4,TriState.True,TriState.True,TriState.True));

			System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			NUnit.Framework.Assertion.AssertEquals ("JW#80", "1,000", Strings.FormatNumber(1000,0,TriState.False,TriState.False,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#81", "1000", Strings.FormatNumber(1000,0,TriState.False,TriState.False,TriState.False));
			NUnit.Framework.Assertion.AssertEquals ("JW#82", "1,000", Strings.FormatNumber(1000,0,TriState.True,TriState.False,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#83", "1,000", Strings.FormatNumber(1000,0,TriState.False,TriState.True,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#84", "1,000", Strings.FormatNumber(1000,0,TriState.True,TriState.True,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#85", "1000", Strings.FormatNumber(1000,0,TriState.False,TriState.False,TriState.False));
			NUnit.Framework.Assertion.AssertEquals ("JW#86", "1,000", Strings.FormatNumber(1000,0,TriState.False,TriState.False,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#87", "-1000", Strings.FormatNumber(-1000,0,TriState.False,TriState.False,TriState.False));
			NUnit.Framework.Assertion.AssertEquals ("JW#88", "-1,000", Strings.FormatNumber(-1000,0,TriState.True,TriState.False,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#89", "(1,000)", Strings.FormatNumber(-1000,0,TriState.False,TriState.True,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#90", "(1,000)", Strings.FormatNumber(-1000,0,TriState.True,TriState.True,TriState.True));
			NUnit.Framework.Assertion.AssertEquals ("JW#91", "-1000", Strings.FormatNumber(-1000,0,TriState.False,TriState.False,TriState.False));
			NUnit.Framework.Assertion.AssertEquals ("JW#92", "(1,000.0000)", Strings.FormatNumber(-1000,4,TriState.True,TriState.True,TriState.True));

			// restore buffered culture
			System.Threading.Thread.CurrentThread.CurrentCulture = CurCulture;

			/*
			//FIXME
			throw new NotImplementedException();
			//throws InvalidCastException
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void FormatPercent()
		{
			/*
			//FIXME
			throw new NotImplementedException();
			//throws InvalidCastException
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void GetChar()
		{
			/*
			if ((Str +AD0APQ- null) +AHwAfA- (Str.Length +AD0APQ- 0))
				throw new ArgumentException("Length of argument 'Str' must be greater than zero.", "Sre");
			if (Index +ADw- 1) 
				throw new ArgumentException("Argument 'Index' must be greater than or equal to 1.", "Index");
			if (Index +AD4- Str.Length)
				throw new ArgumentException("Argument 'Index' must be less than or equal to the length of argument 'String'.", "Index");

			return Str.ToCharArray(Index -1, 1)[0+AF0AOw-
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void InStr_WithOutStartParameter()
		{
			/*
			return InStr(1, String1, String2, Compare);
			*/
		}
		
		// [MonoToDo("Not implemented")]
		[Test]
		public void InStr_WithStartParameter()
		{
			/*
			if (Start +ADw- 1)
				throw new ArgumentException("Argument 'Start' must be non-negative.", "Start");

			 //* FIXME: ms-help://MS.VSCC/MS.MSDNVS/vblr7/html/vafctinstr.htm
			 //* If Compare is omitted, the Option Compare setting determines the type of comparison. Specify 
			 //* a valid LCID (LocaleID) to use locale-specific rules in the comparison.
			 //* How do I do this?
			
			 //* If									InStr returns 
			 //*
			 //* String1 is zero length or Nothing	0 
			 //* String2 is zero length or Nothing	start 
			 //* String2 is not found					0 
			 //* String2 is found within String1		Position where match begins 
			 //* Start +AD4- String2						0 
			 

			//FIXME: someone with a non US setup should test this.
			switch (Compare)
			{
				case CompareMethod.Text:
					return System.Globalization.CultureInfo.CurrentCulture.CompareInfo.IndexOf(String2, String1, Start - 1) + 1;

				case CompareMethod.Binary:
					return String1.IndexOf(String2, Start - 1) + 1;
				default:
					throw new System.ArgumentException("Argument 'Compare' must be CompareMethod.Binary or CompareMethod.Text.", "Compare");
			}
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void InStrRev_4Parameters()
		{

			// 2 InStrRev functions exists+ACEAIQ- Create tests for both versions+ACE-

			/*
			if ((Start +AD0APQ- 0) +AHwAfA- (Start +ADw- -1))
				throw new ArgumentException("Argument 'Start' must be greater than 0 or equal to -1", "Start");
 
			//FIXME: Use LastIndexOf()
			throw new NotImplementedException();
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void InStrRev_5Parameters()
		{
		}
			
		// [MonoToDo("Not implemented")]
		[Test]
		public void Join_Strings()
		{
			/*
			if (SourceArray +AD0APQ- null)
				throw new ArgumentException("Argument 'SourceArray' can not be null.", "SourceArray");
			if (SourceArray.Rank +AD4- 1)
				throw new ArgumentException("Argument 'SourceArray' can have only one dimension.", "SourceArray");

			return string.Join(Delimiter, SourceArray);
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void Join_Objects()
		{
			/*
			if (SourceArray +AD0APQ- null)
				throw new ArgumentException("Argument 'SourceArray' can not be null.", "SourceArray");
			if (SourceArray.Rank +AD4- 1)
				throw new ArgumentException("Argument 'SourceArray' can have only one dimension.", "SourceArray");

			string+AFsAXQ- dest;
			dest = new string[SourceArray.Length+AF0AOw-

			SourceArray.CopyTo(dest, 0);
			return string.Join(Delimiter, dest);
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void LCase_Char() 
		{
			/*
			return char.ToLower(Value);
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void LCase_String() 
		{
			/*
			if ((Value +AD0APQ- null) +AHwAfA- (Value.Length +AD0APQ- 0))
				return String.Empty; // VB.net does this.

			return Value.ToLower();
			*/
		}

		
		[Test]
		public void Left()
		{
			NUnit.Framework.Assertion.AssertEquals ("JW#40", "ä電電", Strings.Left("ä電電jklmeh",3));
			NUnit.Framework.Assertion.AssertEquals ("JW#41", "jk", Strings.Left("jklmeh",2));
			NUnit.Framework.Assertion.AssertEquals ("JW#42", "", Strings.Left("jklmeh",0));
			try
			{
				object buffer = Strings.Left("jklmeh",-1);
				NUnit.Framework.Assertion.Fail ("JW#43 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#43", true);
			}
			NUnit.Framework.Assertion.AssertEquals ("JW#44", "j", Strings.Left("j",2));
			/*
			if (Length +ADw- 0)
				throw new ArgumentException("Argument 'Length' must be non-negative.", "Length");
			if ((Str +AD0APQ- null) +AHwAfA- (Str.Length +AD0APQ- 0))
				return String.Empty; // VB.net does this.

			return Str.Substring(0, Length);
			*/
		}

		[Test]
		public void Len_Bool()
		{
			try
			{
				object buffer = Strings.Len(null);
				NUnit.Framework.Assertion.Fail ("JW#50 hasn't thrown an error");
			}
			catch
			{
				NUnit.Framework.Assertion.Assert ("JW#50", true);
			}
			NUnit.Framework.Assertion.AssertEquals ("JW#51", 2, Strings.Len(true));
			NUnit.Framework.Assertion.AssertEquals ("JW#52", 2, Strings.Len(false));
			/*
			return 2; //sizeof(bool)
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void Len_Byte()
		{
			/*
			return 1; //sizeof(byte)
			*/
		}
		
		// [MonoToDo("Not implemented")]
		[Test]
		public void Len_Char()
		{
			/*
			return 2; //sizeof(char)
			*/
		}
		
		// [MonoToDo("Not implemented")]
		[Test]
		public void Len_Double()
		{
			/*
			return 8; //sizeof(double)
			*/
		}
		
		// [MonoToDo("Not implemented")]
		[Test]
		public void Len_Int()
		{
			/*
			return 4; //sizeof(int)
			*/
		}
		
		// [MonoToDo("Not implemented")]
		[Test]
		public void Len_Long()
		{
			/*
			return 8; //sizeof(long)
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void Len_Object()
		{
			/*
			// FIXME: 
			// With user-defined types and Object variables, the Len function returns the size as it will 
			// be written to the file. If an Object contains a String, it will return the length of the string. 
			// If an Object contains any other type, it will return the size of the object as it will be written 
			// to the file.
			throw new NotImplementedException(); 
			*/
		}
		
		// [MonoToDo("Not implemented")]
		[Test]
		public void Len_Short()
		{
			/*
			return 2; //sizeof(short)
			*/
		}
		
		// [MonoToDo("Not implemented")]
		[Test]
		public void Len_Single()
		{
			/*
			return 4; //sizeof(Single)
			*/
		}
		
		// [MonoToDo("Not implemented")]
		[Test]
		public void Len_String()
		{
			/*
			return Expression.Length; //length of the string
			*/
		}
		
		// [MonoToDo("Not implemented")]
		[Test]
		public void Len_DateTime()
		{
			/*
			return 8; //sizeof(DateTime)
			*/
		}
		
		// [MonoToDo("Not implemented")]
		[Test]
		public void Len_Decimal()
		{
			/*
			return 16; //sizeof(decimal)
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void LSet()
		{
			/*
			if (Length +ADw- 0)
				throw new ArgumentOutOfRangeException("Length", "Length must be must be non-negative.");
			if (Source +AD0APQ- null)
				Source = String.Empty;

			return Source.PadRight(Length);
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void LTrim()
		{
			/*
			if ((Str +AD0APQ- null) +AHwAfA- (Str.Length +AD0APQ- 0))
				return String.Empty; // VB.net does this.

			return Str.TrimStart(null);
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void RTrim()
		{
			/*
			if ((Str +AD0APQ- null) +AHwAfA- (Str.Length +AD0APQ- 0))
				return String.Empty; // VB.net does this.

			return Str.TrimEnd(null);
			*/
		}
	
		// [MonoToDo("Not implemented")]
		[Test]
		public void Trim() 
		{
			/*
			if ((Str +AD0APQ- null) +AHwAfA- (Str.Length +AD0APQ- 0))
				return String.Empty; // VB.net does this.
			
			return Str.Trim();
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void Mid_WithLengthParameter()
		{
			/*
			if (Length +ADw- 0)
				throw new System.ArgumentException("Argument 'Length' must be greater or equal to zero.", "Length");
			if (Start +ADwAPQ- 0)
				throw new System.ArgumentException("Argument 'Start' must be greater than zero.", "Start");
			if ((Str +AD0APQ- null) +AHwAfA- (Str.Length +AD0APQ- 0))
				return String.Empty; // VB.net does this.

			if ((Length +AD0APQ- 0) +AHwAfA- (Start +AD4- Str.Length))
				return String.Empty;

			if (Length +AD4- (Str.Length - Start))
				Length = (Str.Length - Start) + 1;

			return Str.Substring(Start - 1, Length);
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void Mid_WithOutLengthParameter ()
		{
			/*
			if (Start +ADwAPQ- 0)
				throw new System.ArgumentException("Argument 'Start' must be greater than zero.", "Start");
			if ((Str +AD0APQ- null) +AHwAfA- (Str.Length +AD0APQ- 0))
				return String.Empty; // VB.net does this.

			if (Start +AD4- Str.Length)
				return String.Empty;

			return Str.Substring(Start - 1);
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void Replace()
		{
			/*
			if (Count +ADw- -1)
				throw new ArgumentException("Argument 'Count' must be greater than or equal to -1.", "Count");
			if (Start +ADwAPQ- 0)
				throw new ArgumentException("Argument 'Start' must be greater than zero.", "Start");

			if ((Expression +AD0APQ- null) +AHwAfA- (Expression.Length +AD0APQ- 0))
				return String.Empty; // VB.net does this.
			if ((Find +AD0APQ- null) +AHwAfA- (Find.Length +AD0APQ- 0))
				return Expression; // VB.net does this.
			if (Replacement +AD0APQ- null)
				Replacement = String.Empty; // VB.net does this.

			return Expression.Replace(Find, Replacement);
			*/
		}
 
		// [MonoToDo("Not implemented")]
		[Test]
		public void Right()
		{
			/*
			if (Length +ADw- 0)
				throw new ArgumentException("Argument 'Length' must be greater or equal to zero.", "Length");

			return Str.Substring (Str.Length - Length);
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void RSet()
		{
			/*		
			if (Source +AD0APQ- null)
				Source = String.Empty;
			if (Length +ADw- 0)
				throw new ArgumentOutOfRangeException("Length", "Length must be non-negative.");

			return Source.PadLeft(Length);
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void Space() 
		{
			/*
			if (Number +ADw- 0)
				throw new ArgumentException("Argument 'Number' must be greater or equal to zero.", "Number");

			return new string((char) ' ', Number);
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void Split()
		{
			/*			
			if (Expression +AD0APQ- null)
				return new string[0+AF0AOw-
			if ((Delimiter +AD0APQ- null) +AHwAfA- (Delimiter.Length +AD0APQ- 0))
			{
				string +AFsAXQ- ret = new string[0+AF0AOw-
				ret[0] = Expression;
				return ret;
			}
			if (Limit +AD0APQ- 0)
				Limit = 1; // VB.net does this. I call it a bug.

			//
			// * FIXME: VB.net does NOT do this. It simply fails with AritmethicException.
			// * What should I do?
			//
			if (Limit +ADw- -1)
				throw new ArgumentOutOfRangeException("Limit", "Argument 'Limit' must be -1 or greater than zero.");

			switch (Compare)
			{
				case CompareMethod.Binary:
					return Expression.Split(Delimiter.ToCharArray(0, 1), Limit);
				case CompareMethod.Text:
					//FIXME
					throw new NotImplementedException();
				default:
					throw new System.ArgumentException("Argument 'Compare' must be CompareMethod.Binary or CompareMethod.Text.", "Compare");
			}
			*/			
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void StrComp()
		{
			/*
			switch (Compare)
			{
				case CompareMethod.Binary:
					return string.Compare(String1, String2, true);
				case CompareMethod.Text:
					//FIXME: someone with a non US setup should test this.
					return System.Globalization.CultureInfo.CurrentCulture.CompareInfo.Compare(String1, String2);
				default:
					throw new System.ArgumentException("Argument 'Compare' must be CompareMethod.Binary or CompareMethod.Text", "Compare");
			}
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void StrConv ()
		{
			/*
			//FIXME
			throw new NotImplementedException(); 
			//throws ArgumentException
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void StrDup_Char()
		{
			/*
			if (Number +ADw- 0)
				throw new ArgumentException("Argument 'Number' must be non-negative.", "Number");

			return new string(Character, Number);
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void StrDup_String()
		{
			/*
			if (Number +ADw- 0)
				throw new ArgumentException("Argument 'Number' must be greater or equal to zero.", "Number");
			if ((Character +AD0APQ- null) +AHwAfA- (Character.Length +AD0APQ- 0))
				throw new ArgumentNullException("Character", "Length of argument 'Character' must be greater than zero.");

			return new string(Character.ToCharArray()[0], Number);
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void StrDup_Object()
		{
			/*
			if (Number +ADw- 0)
				throw new ArgumentException("Argument 'Number' must be non-negative.", "Number");
			
			if (Character is string)
			{
				string sCharacter = (string) Character;
				if ((sCharacter +AD0APQ- null) +AHwAfA- (sCharacter.Length +AD0APQ- 0))
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
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void StrReverse()
		{
			/*
			// patched by Daniel Campos 
			// danielcampos+AEA-myway.com
			if (Expression +ACEAPQ- null)
			{
				if ( Expression.Length+AD4-0)
				{
					int counter=0;
					char+AFsAXQ- buf=new char[Expression.Length+AF0AOw-

					for (int backwardsCounter=Expression.Length - 1;
						backwardsCounter+AD4APQ-0;
						backwardsCounter--)
						buf[counter+++AF0APQ-Expression[backwardsCounter+AF0AOw-
					return new string(buf);
				}
				else
					return String.Empty;
			}
			else
				return String.Empty;
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void UCase_Char()
		{
			/*
			return char.ToUpper(Value);
			*/
		}

		// [MonoToDo("Not implemented")]
		[Test]
		public void UCase_String()
		{
			/*
			if ((Value +AD0APQ- null) +AHwAfA- (Value.Length +AD0APQ- 0))
				return String.Empty; // VB.net does this. 

			return Value.ToUpper();
			*/
		}
	}
}
