//
// Strings.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic {
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Auto)] 
	sealed public class Strings {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.Int32 Asc (System.Char String) { return 0;}
		public static System.Int32 Asc (System.String String) { return 0;}
		public static System.Int32 AscW (System.String String) { return 0;}
		public static System.Int32 AscW (System.Char String) { return 0;}
		public static System.Char Chr (System.Int32 CharCode) { return '\0';}
		public static System.Char ChrW (System.Int32 CharCode) { return '\0';}
		public static System.String[] Filter (System.Object[] Source, System.String Match, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(true)] System.Boolean Include, [Microsoft.VisualBasic.CompilerServices.OptionCompareAttribute] [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.CompareMethod Compare) { return null;}
		public static System.String[] Filter (System.String[] Source, System.String Match, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(true)] System.Boolean Include, [Microsoft.VisualBasic.CompilerServices.OptionCompareAttribute] [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.CompareMethod Compare) { return null;}
		public static System.Int32 InStr (System.String String1, System.String String2, [Microsoft.VisualBasic.CompilerServices.OptionCompareAttribute] [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.CompareMethod Compare) { return 0;}
		public static System.Int32 InStr (System.Int32 Start, System.String String1, System.String String2, [Microsoft.VisualBasic.CompilerServices.OptionCompareAttribute] [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.CompareMethod Compare) { return 0;}
		public static System.Int32 InStrRev (System.String StringCheck, System.String StringMatch, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int32 Start, [Microsoft.VisualBasic.CompilerServices.OptionCompareAttribute] [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.CompareMethod Compare) { return 0;}
		public static System.String Join (System.Object[] SourceArray, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(" ")] System.String Delimiter) { return "";}
		public static System.String Join (System.String[] SourceArray, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(" ")] System.String Delimiter) { return "";}
		public static System.String LCase (System.String Value) { return "";}
		public static System.Char LCase (System.Char Value) { return '\0';}
		public static System.Int32 Len (System.String Expression) { return 0;}
		public static System.Int32 Len (System.Byte Expression) { return 0;}
		public static System.Int32 Len (System.Char Expression) { return 0;}
		public static System.Int32 Len (System.Int16 Expression) { return 0;}
		public static System.Int32 Len (System.Int32 Expression) { return 0;}
		public static System.Int32 Len (System.Int64 Expression) { return 0;}
		public static System.Int32 Len (System.Single Expression) { return 0;}
		public static System.Int32 Len (System.Double Expression) { return 0;}
		public static System.Int32 Len (System.Boolean Expression) { return 0;}
		public static System.Int32 Len (System.Decimal Expression) { return 0;}
		public static System.Int32 Len (System.DateTime Expression) { return 0;}
		public static System.Int32 Len (System.Object Expression) { return 0;}
		public static System.String Replace (System.String Expression, System.String Find, System.String Replacement, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] System.Int32 Start, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int32 Count, [Microsoft.VisualBasic.CompilerServices.OptionCompareAttribute] [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.CompareMethod Compare) { return "";}
		public static System.String Space (System.Int32 Number) { return "";}
		public static System.String[] Split (System.String Expression, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(" ")] System.String Delimiter, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int32 Limit, [Microsoft.VisualBasic.CompilerServices.OptionCompareAttribute] [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.CompareMethod Compare) { return null;}
		public static System.String LSet (System.String Source, System.Int32 Length) { return "";}
		public static System.String RSet (System.String Source, System.Int32 Length) { return "";}
		public static System.Object StrDup (System.Int32 Number, System.Object Character) { return null;}
		public static System.String StrDup (System.Int32 Number, System.Char Character) { return "";}
		public static System.String StrDup (System.Int32 Number, System.String Character) { return "";}
		public static System.String StrReverse (System.String Expression) { return "";}
		public static System.String UCase (System.String Value) { return "";}
		public static System.Char UCase (System.Char Value) { return '\0';}
		public static System.String Format (System.Object Expression, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue("")] System.String Style) { return "";}
		public static System.String FormatCurrency (System.Object Expression, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int32 NumDigitsAfterDecimal, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-2)] Microsoft.VisualBasic.TriState IncludeLeadingDigit, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-2)] Microsoft.VisualBasic.TriState UseParensForNegativeNumbers, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-2)] Microsoft.VisualBasic.TriState GroupDigits) { return "";}
		public static System.String FormatDateTime (System.DateTime Expression, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.DateFormat NamedFormat) { return "";}
		public static System.String FormatNumber (System.Object Expression, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int32 NumDigitsAfterDecimal, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-2)] Microsoft.VisualBasic.TriState IncludeLeadingDigit, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-2)] Microsoft.VisualBasic.TriState UseParensForNegativeNumbers, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-2)] Microsoft.VisualBasic.TriState GroupDigits) { return "";}
		public static System.String FormatPercent (System.Object Expression, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int32 NumDigitsAfterDecimal, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-2)] Microsoft.VisualBasic.TriState IncludeLeadingDigit, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-2)] Microsoft.VisualBasic.TriState UseParensForNegativeNumbers, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-2)] Microsoft.VisualBasic.TriState GroupDigits) { return "";}
		public static System.Char GetChar (System.String str, System.Int32 Index) { return '\0';}
		public static System.String Left (System.String str, System.Int32 Length) { return "";}
		public static System.String LTrim (System.String str) { return "";}
		public static System.String Mid (System.String str, System.Int32 Start) { return "";}
		public static System.String Mid (System.String str, System.Int32 Start, System.Int32 Length) { return "";}
		public static System.String Right (System.String str, System.Int32 Length) { return "";}
		public static System.String RTrim (System.String str) { return "";}
		public static System.String Trim (System.String str) { return "";}
		public static System.Int32 StrComp (System.String String1, System.String String2, [Microsoft.VisualBasic.CompilerServices.OptionCompareAttribute] [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.CompareMethod Compare) { return 0;}
		public static System.String StrConv (System.String str, Microsoft.VisualBasic.VbStrConv Conversion, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] System.Int32 LocaleID) { return "";}
		// Events
	};
}
