// $ANTLR 2.7.3: "jscript-lexer-parser.g" -> "JScriptLexer.cs"$

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

	using System.Collections;

namespace Microsoft.JScript
{
	public class JScriptParserTokenTypes
	{
		public const int EOF = 1;
		public const int NULL_TREE_LOOKAHEAD = 3;
		public const int LITERAL_function = 4;
		public const int IDENTIFIER = 5;
		public const int OPEN_PARENS = 6;
		public const int CLOSE_PARENS = 7;
		public const int COLON = 8;
		public const int OPEN_BRACE = 9;
		public const int CLOSE_BRACE = 10;
		public const int COMMA = 11;
		public const int SEMI_COLON = 12;
		public const int LITERAL_try = 13;
		public const int LITERAL_catch = 14;
		public const int LITERAL_finally = 15;
		public const int LITERAL_throw = 16;
		public const int LITERAL_switch = 17;
		public const int LITERAL_default = 18;
		public const int LITERAL_case = 19;
		public const int LITERAL_with = 20;
		public const int LITERAL_return = 21;
		public const int LITERAL_break = 22;
		public const int LITERAL_continue = 23;
		public const int LITERAL_do = 24;
		public const int LITERAL_while = 25;
		public const int LITERAL_for = 26;
		public const int LITERAL_var = 27;
		public const int IN = 28;
		public const int LITERAL_if = 29;
		public const int LITERAL_else = 30;
		public const int ASSIGN = 31;
		public const int LITERAL_new = 32;
		public const int DOT = 33;
		public const int OPEN_BRACKET = 34;
		public const int CLOSE_BRACKET = 35;
		public const int INCREMENT = 36;
		public const int DECREMENT = 37;
		public const int LITERAL_delete = 38;
		public const int LITERAL_void = 39;
		public const int LITERAL_typeof = 40;
		public const int PLUS = 41;
		public const int MINUS = 42;
		public const int BITWISE_NOT = 43;
		public const int LOGICAL_NOT = 44;
		public const int MULT = 45;
		public const int DIVISION = 46;
		public const int MODULE = 47;
		public const int SHIFT_LEFT = 48;
		public const int SHIFT_RIGHT = 49;
		public const int UNSIGNED_SHIFT_RIGHT = 50;
		public const int LESS_THAN = 51;
		public const int GREATER_THAN = 52;
		public const int LESS_EQ = 53;
		public const int GREATER_EQ = 54;
		public const int INSTANCE_OF = 55;
		public const int EQ = 56;
		public const int NEQ = 57;
		public const int STRICT_EQ = 58;
		public const int STRICT_NEQ = 59;
		public const int BITWISE_AND = 60;
		public const int BITWISE_XOR = 61;
		public const int BITWISE_OR = 62;
		public const int LOGICAL_AND = 63;
		public const int LOGICAL_OR = 64;
		public const int INTERR = 65;
		public const int MULT_ASSIGN = 66;
		public const int DIV_ASSIGN = 67;
		public const int MOD_ASSIGN = 68;
		public const int ADD_ASSIGN = 69;
		public const int SUB_ASSIGN = 70;
		public const int SHIFT_LEFT_ASSIGN = 71;
		public const int SHIFT_RIGHT_ASSIGN = 72;
		public const int AND_ASSIGN = 73;
		public const int XOR_ASSIGN = 74;
		public const int OR_ASSIGN = 75;
		public const int LITERAL_this = 76;
		public const int LITERAL_null = 77;
		public const int LITERAL_true = 78;
		public const int LITERAL_false = 79;
		public const int STRING_LITERAL = 80;
		public const int DECIMAL_LITERAL = 81;
		public const int HEX_INTEGER_LITERAL = 82;
		public const int LINE_FEED = 83;
		public const int CARRIAGE_RETURN = 84;
		public const int LINE_SEPARATOR = 85;
		public const int PARAGRAPH_SEPARATOR = 86;
		public const int TAB = 87;
		public const int VERTICAL_TAB = 88;
		public const int FORM_FEED = 89;
		public const int SPACE = 90;
		public const int NO_BREAK_SPACE = 91;
		public const int SL_COMMENT = 92;
		public const int ML_COMMENT = 93;
		
	}
}
