//
// jscript-lexer-parser.g: EcmaScript Grammar written on antlr.
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren (cesar@ciencias.unam.mx)
//


options {
    language = "CSharp";
    namespace =  "Microsoft.JScript";
}


// Parser
class JScriptParser extends Parser;


// Program, see section 14 from Ecma-262, page 75.
program [Program p]
{ 
    SourceElements elems;
}
    : 
        source_elements [p.SourceElements]
    ;


source_elements [SourceElements elems]
{
    SourceElement se;
}
    : 
        se = source_element { elems.Add (se); } (source_elements [elems] | )
    ;


// See Section 14 from Ecma-262, page 75.
source_element returns [SourceElement se]
{
    se = new SourceElement ();
    Statement stm = null;
    FunctionDeclaration fd = null;
}
    : 
        stm = statement
        { se = stm; }
    |
        fd = function_declaration
        { se = fd; }
    ;


// Statement, see section 12 from Ecma-262, page 61.
statement returns [Statement stm]
{
    stm = null;
}
    : 
	block	    
    |
        variable_statement
    |
	empty_statement
    |    
        stm = print_statement
    ;


block: LBRACE (statement_list | ) RBRACE 
    ;


empty_statement: SEMI_COLON ;

statement_list
    :
	statement (statement_list | )
    ;

// VariableStatement, see section 12.2 from Ecma-262 3td Edition, page 74.
variable_statement
    :
        "var" variable_declaration_list SEMI_COLON
    ;
        

variable_declaration_list
    :
        variable_declaration (COMMA variable_declaration_list | )
    ;

variable_declaration
    :
	IDENTIFIER (initialiser | )
    ;
    

initialiser
    :
        ASSIGNMENT assignment_expression
    ;                           

// FIXME: a conditional_expression can be reduced to a postfixExpression wich 
// is reduced to a left_hand_side_expression
// AssignmentExpression, see section 11.13 from Ecma-262 3td. Edition, page 59.
assignment_expression
    :
        conditional_expression
//    |
//        left_hand_side_expression assignment_operator assignment_expression
    ;

        
assignment_operator
    :
        ASSIGNMENT
    |
        COMPOUND_ASSIGNMENT
    ;


// ConditionalExpression, see section 11.12 from Ecma-262, page 58.
conditional_expression
    :
        logical_or_expression (INTERROGATION assignment_expression COLON assignment_expression | )
    ;
        


// Binary Logical Operators, section 11.11 from Ecma-262 spec, page 58.
logical_or_expression
    :
        logical_and_expression (LOGICAL_OR logical_or_expression | )
    ;


logical_and_expression
    :
        bitwise_or_expression (LOGICAL_AND logical_and_expression | )
    ;


// Binary Bitwise Operators, section 11.10 from Ecma-262 spec, page 57.
bitwise_or_expression
    :
        bitwise_xor_expression (BITWISE_OR bitwise_or_expression | )
    ;

bitwise_xor_expression
    :
        bitwise_and_expression (TRIANGLE bitwise_xor_expression | )
    ;


bitwise_and_expression
    :
        equality_expression (BITWISE_AND bitwise_and_expression | )       
    ;


// Equality Operators, section 11.9 from Ecma-262 spec, page 54.
// FIXME: more options left to implement
equality_expression
    :
        relational_expression
    ;


// Relational Operators, section 11.4 from Ecma-262 spec, page 52.
// FIXME: more options left to implement
relational_expression
    :
        shift_expression
    ;


// Bitwise Shift Operators, section 11.7 from Ecma-262, page 51.
// FIXME: more options left to implement
shift_expression
    :
        additive_expression
    ;



// Additive Operators, section 11.6 from Ecma-262, page 50.
// FIXME: more options left to implement
additive_expression
    :
        multiplicative_expression ((PLUS | MINUS) additive_expression | )
    ;



// Multiplicative Operators, section 11.5 from Ecma-262, page 48.
multiplicative_expression
    :
        unary_expression ((TIMES | SLASH | PERCENT) multiplicative_expression | ) 
    ;


// Unary Operators,  Section 11.4 from Ecma-262, page 46.
unary_expression
    :
        postfix_expression
    |
        ("delete" | "void" | "typeof" | INCREMENT | DECREMENT | PLUS | MINUS | ADMIRATION) unary_expression
    ;


// Postfix Expressions, section 11.3 from Ecma-262, page 45.
// FIXME: more options left to implement
postfix_expression
    :
        left_hand_side_expression
    ;


// FIXME: there's a problem with the NEW member_expression arguments rule from member expression
// section 11.2 from Ecma-262 3td Edition, page 43.
left_hand_side_expression
    :
        new_expression
//    |
//        call_expression
    ;


// FIXME: there's a problem with the NEW member_expression arguments rule from member expression
new_expression
    :
        member_expression
    |
        "new" new_expression
    ;

// FIXME: more options left to implement
call_expression
    :
        member_expression arguments
    ;


// FIXME: more options left to implement
member_expression
    :
        primary_expression
//    |
//        "new" member_expression arguments
    ;


arguments
    :
        LPAREN (argument_list | ) RPAREN
    ;
        

argument_list
    :
        assignment_expression (COMMA argument_list | )
    ;

// Expressions, section 11, from Ecma-262 3d Edition, page 40.
// FIXME: more options left to implement
primary_expression
    :
        THIS
    |
        IDENTIFIER
    |
        literal
    ;


// Literals, section 7.8 from Ecma-262 3d Edition, page 16.
// FIXME: more options left to implement
literal
    :
	boolean_literal
    |
	null_literal
    ;


// Non-Ecma statements
print_statement returns [PrintStatement pn]
{ pn = new PrintStatement (); }
    : 
        "print" LPAREN str:STRING_LITERAL RPAREN SEMI_COLON
        { 
            pn.Message =  str.getText (); 
        }         
    ;




// Function definition, see Section 13 from Ecma-262, page 71.
function_declaration returns [FunctionDeclaration fd]
{
    fd = new FunctionDeclaration ();
}
    :
        "function" IDENTIFIER LPAREN (formal_parameter_list | ) RPAREN LBRACE function_body [fd.elems] RBRACE
    ;


formal_parameter_list
    :
        IDENTIFIER (COMMA formal_parameter_list | )
    ;


function_body [SourceElements elems]
    :
        source_elements [elems]
    ;


boolean_literal
    :
    	"true"
    |
    	"false"
    ;

null_literal
    :
	"null"
    ;

// Lexer
class JScriptLexer extends Lexer;
options {
    charVocabulary='\u0000'..'\uFFFE';
    testLiterals=false;
    k = 2;
}


TAB 
    : 
        '\u0009' 
    ;
        
   
VERTICAL_TAB
    : 
        '\u000B' 
    ;


FORM_FEED
    :
        '\u000C' 
    ;


SPACE
    :
        '\u0020'
        { _ttype =Token.SKIP; }
    ;


NO_BREAK_SPACE
    :
        '\u00A0'
    ;    

// FIXME: find out possibles Unicode "space separator"
// USP: 


LINE_FEED
    :
        '\u000A'
        { newline ();  { _ttype =Token.SKIP; }}
    ;


CARRIGE_RETURN
    :
        '\u000D'
        { newline (); { _ttype =Token.SKIP; }}
    ;


LINE_SEPARATOR
    :
        '\u2028'
        { newline ();  { _ttype =Token.SKIP; }}
    ;


PARAGRAPH_SEPARATOR
    :
        '\u2029'
        { newline (); { _ttype =Token.SKIP; }}
    ;




// Punctuators

LBRACE: '{' ;

RBRACE: '}' ;

LPAREN: '(' ;

RPAREN: ')' ;

LSQUARE: '[' ;

RSQUARE: ']' ;

DOT: '.' ;

SEMI_COLON: ';' ;

COMMA: ',' ;

L_THAN: '<' ;

G_THAN: '>' ;

PLUS: '+' ('=' { $setType (COMPOUND_ASSIGNMENT); })? ;

MINUS: '-' ('=' { $setType (COMPOUND_ASSIGNMENT); })? ;

TIMES: '*' ('=' { $setType (COMPOUND_ASSIGNMENT); })? ;

SLASH: '/' ('=' { $setType (COMPOUND_ASSIGNMENT); })? ;

PERCENT: '%' ('=' { $setType (COMPOUND_ASSIGNMENT); })? ;

BITWISE_AND: '&' ('&' { $setType (LOGICAL_AND); } | '=' { $setType (COMPOUND_ASSIGNMENT); })?;

BITWISE_OR: '|'  ('|' { $setType (LOGICAL_OR); } | '=' { $setType (COMPOUND_ASSIGNMENT); })? ;

ADMIRATION: '!' ;

INTERROGATION: '?' ;

COLON: ':' ;

ASSIGNMENT: '=' ;

TRIANGLE: '^' ('=' { $setType (COMPOUND_ASSIGNMENT); })? ;

INCREMENT: "++" ;

DECREMENT: "--";


// FIXME: this just temporal, in order to get into parsing
STRING_LITERAL
    : 
        '"'!('a'..'z' | 'A'..'Z' | '\u0020')+'"'!
    ;


// Fixme: this a temporal definition. W
//        We must handle the UNICODE charset, see section 7.6 of the Ecma-262 spec
IDENTIFIER
options { testLiterals=true; }
    : 
	('a'..'z' | 'A'..'Z') ('a'..'z' | 'A'..'Z' | '0'..'9')*
    ;
