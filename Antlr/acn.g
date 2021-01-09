/*
* Copyright (c) 2008-2012 Semantix and (c) 2012-2015 Neuropublic
*
* This file is part of the ASN1SCC tool.
*
* Licensed under the terms of GNU General Public Licence as published by
* the Free Software Foundation.
*
*  For more informations see License.txt file
*/



grammar acn;
options {
	output=AST;
	language=CSharp2;
}

tokens {
	MODULE_DEF;
	ASN1_FILE;
	UNARY_MINUS;
	PARAM_LIST;
	PARAM;
	CHILDREN_ENC_SPEC;
	CHILD;
	CHILD_NEW;
	ARGUMENTS;
	ENCODING_PROPERTIES;
	TYPE_ENCODING;
	ENCODING_SPEC;
	LONG_FIELD;
	REFERENCED_TYPE;
    PRESENT_WHEN_STR_EQUAL;
}

@parser::namespace { Antlr.Acn } // Or just @namespace { ... }

@lexer::namespace { Antlr.Acn }


@header {
	#pragma warning disable 0219
}

@members {

public override void EmitErrorMessage(string msg)
{
    throw new  Antlr.Asn1.asn1Parser.SyntaxErrorException(msg);
}
public override string GetErrorHeader(RecognitionException e)
{
    return ErrorFormatter.GetErrorHeader(input.SourceName, e);
}
}


@lexer::members {
System.Collections.Generic.List<IToken> tokens = new System.Collections.Generic.List<IToken>();
public override void Emit(IToken tok) {
        state.token = tok;
       	tokens.Add(tok);
}
public override IToken NextToken() {
    	base.NextToken();
        if ( tokens.Count==0 ) {
            return Token.EOF_TOKEN;
        }
        IToken ret = tokens[0];
        tokens.Remove(ret);
        return ret;
}

public override void EmitErrorMessage(string msg)
{
    throw new Antlr.Asn1.asn1Parser.SyntaxErrorException(msg);
}
public override string GetErrorHeader(RecognitionException e)
{
    return ErrorFormatter.GetErrorHeader(input.SourceName, e);
}

}


/* ********************************************************************************************************************* */
/* *************************************    PARSER         ************************************************************* */
/* ********************************************************************************************************************* */

moduleDefinitions 
	:	moduleDefinition+	->^(ASN1_FILE moduleDefinition+)
	;	

moduleDefinition :  	a=modulereference	
			d=DEFINITIONS
			ASSIG_OP BEGIN
			constant*
			typeEncoding*
			END
			->  ^(MODULE_DEF[$d] modulereference  constant* typeEncoding*)
			;

constant
	:	CONSTANT^ UID ASSIG_OP! intExpression;			


typeEncoding
	:	typereference paramList? encodingSpec -> ^(TYPE_ENCODING typereference paramList? encodingSpec) ;				
	
	
paramList: '<' singleParam (',' singleParam)*  '>'		-> ^(PARAM_LIST singleParam+);
	
singleParam
	:	asn1Type ':' asn1LID	->^(PARAM asn1Type asn1LID);	
	
asn1Type
	:	asn1UID ('.' asn1UID)	-> ^(REFERENCED_TYPE asn1UID asn1UID)
	|	asn1UID 				-> ^(REFERENCED_TYPE asn1UID )
	|	INTEGER
	|   BOOLEAN
	|   NULL
	;	
	
encodingSpec
	:	a='[' propertyList? ']' childrenSpec?	->^(ENCODING_SPEC[$a] propertyList? childrenSpec?)
	;	
	
	
propertyList
	:	property (',' property)*	->^(ENCODING_PROPERTIES  property+)
	;	
	
property
	:	encodingProp
	| 	sizeProp
//	| 	adjustProp
	|	alignToNextProp
	| 	encodeValuesProp
	|	presentWhenProp
	|	trueValProp
	|	falseValProp
	|	patternProp
	|	determinantProp
	|	endiannessProp
	|   enumSetValue
    |   terminationPatternProp
    |   mappingFunctionProp
    |   postEncodingFunctionProp
    |   preDecodingFunctionProp
    |   savePositionProp
	;	

endiannessProp 	
	: ENDIANNES^  BIG 	
	| ENDIANNES^  LITTLE 	
	;

encodingProp	
	: ENCODING^	POS_INT  		
	| ENCODING^	TWOSCOMPLEMENT  
	| ENCODING^	BCD				
	| ENCODING^	ASCII 			
	| ENCODING^	IEEE754_1985_32 
	| ENCODING^	IEEE754_1985_64 
	;


sizeProp
	: 
	  SIZE^ NULL_TERMINATED 
	| SIZE^ INT
	| SIZE^ UID			//UID referes to a constant declared in the constant section
	| SIZE^ longFld
	;

/*	
adjustProp 
	: ADJUST^ INT
	| ADJUST^ UID			//UID referes to a constant declared in the constant section
	;
*/	

longFld 	:	asn1LID ('.' asn1LID)*		-> ^(LONG_FIELD asn1LID+);

mappingFunctionProp         : MAPPING_FUNCTION^ (asn1LID|asn1UID);
postEncodingFunctionProp    : POST_ENCODING_FUNCTION^ (asn1LID|asn1UID);
preDecodingFunctionProp     : POST_DECODING_VALIDATOR^ (asn1LID|asn1UID);

alignToNextProp	
	:	ALIGNTONEXT^ BYTE
	|	ALIGNTONEXT^ WORD
	|	ALIGNTONEXT^ DWORD
	;

encodeValuesProp	
	:  ENCODE_VALUES
	;

trueValProp 	: TRUE_VALUE^	  BitStringLiteral 
	;

falseValProp 	: FALSE_VALUE^  BitStringLiteral
	;

patternProp 	: PATTERN^  (BitStringLiteral | OctectStringLiteral)
	;

savePositionProp    : SAVE_POSITION;

presentWhenProp	
	:	PRESENT_WHEN^ presentWhenCond+
	;

terminationPatternProp
    :  TERMINATION_PATTERN^ (BitStringLiteral | OctectStringLiteral)
    ;

presentWhenCond
	: longFld EQUAL^ (INT|UID)
	| longFld a=EQUAL  StringLiteral	-> ^(PRESENT_WHEN_STR_EQUAL[$a] longFld StringLiteral)
	| longFld 
	;

determinantProp
	:	DETERMINANT^ longFld	
	;

enumSetValue
	: INT|UID	
	;

childrenSpec
	:	'{'! '}'!
	|	'{' childSpec (',' childSpec)* '}'	->^(CHILDREN_ENC_SPEC childSpec+)
	;	
	
childSpec	
	:	asn1LID? argumentList? encodingSpec	->^(CHILD asn1LID? encodingSpec argumentList?)
	|	asn1LID asn1Type encodingSpec		->^(CHILD_NEW asn1LID asn1Type encodingSpec)
	;	
	
argumentList
	:	'<' longFld (',' longFld)*'>' ->^(ARGUMENTS longFld+);
		


intExpression 	:	multiplicative_expression (PLUS^ multiplicative_expression  | MINUS^ multiplicative_expression )*
	;

multiplicative_expression	: power_expression 
	(
			MULTIPLICATION^ power_expression
		|  	DIVISION^ power_expression
		| 	MODULO^ power_expression
	)*
	;

power_expression	:	unary_expression (POWER_SYMBOL^ power_expression)?	// right associative via tail recursion
	;
	
unary_expression
	: atom
	| PLUS! atom		
	| MINUS atom	->^(UNARY_MINUS atom)
	;		
	
atom
	:	INT	
	|	UID 			//CONSTANT
	| '('! intExpression ')'!	
;
	


asn1LID	
	: LID
	| a=ENDIANNES		-> LID[$a]
	| a=BIG				-> LID[$a]
	| a=LITTLE			-> LID[$a]
	| a=ENCODING		-> LID[$a]
	| a=POS_INT  		-> LID[$a]
	| a=TWOSCOMPLEMENT	-> LID[$a]
	| a=SIZE			-> LID[$a]	
	| a=NULL_TERMINATED -> LID[$a]	
//	| a=ADJUST			-> LID[$a]		
	| a=ALIGNTONEXT		-> LID[$a]		
	| a=BYTE			-> LID[$a]		
	| a=WORD			-> LID[$a]		
	| a=DWORD			-> LID[$a]		
	| a=ENCODE_VALUES	-> LID[$a]		
	| a=TRUE_VALUE		-> LID[$a]
	| a=FALSE_VALUE		-> LID[$a]
	| a=PATTERN			-> LID[$a]
	| a=PRESENT_WHEN	-> LID[$a]
	| a=DETERMINANT		-> LID[$a]
	;
	
modulereference	:	asn1UID;

typereference	:	asn1UID;

asn1UID
	:	UID
	| a=BCD				-> UID[$a]
	| a=ASCII 			-> UID[$a]
	| a=IEEE754_1985_32 -> UID[$a]
	| a=IEEE754_1985_64 -> UID[$a]
	;




/* ***************************************************************************************************************** */
/* ***************************************************************************************************************** */
/* **************************************      LEXER    ************************************************************ */
/* ***************************************************************************************************************** */
/* ***************************************************************************************************************** */
ENDIANNES :	'endianness';

BIG 				: 'big';
LITTLE				: 'little';

ENCODING			: 'encoding';
POS_INT  			: 'pos-int';
TWOSCOMPLEMENT		: 'twos-complement';
BCD					: 'BCD';
ASCII 				: 'ASCII';
IEEE754_1985_32 	: 'IEEE754-1985-32';
IEEE754_1985_64 	: 'IEEE754-1985-64';

SIZE				: 'size';
NULL_TERMINATED 	: 'null-terminated';
TERMINATION_PATTERN	: 'termination-pattern';

//ADJUST				: 'adjust';

ALIGNTONEXT			: 'align-to-next';
BYTE				: 'byte';
WORD				: 'word';
DWORD				: 'dword';

ENCODE_VALUES		: 'encode-values';

SAVE_POSITION       : 'save-position';

TRUE_VALUE			: 'true-value';
FALSE_VALUE			: 'false-value';
PATTERN				: 'pattern';
MAPPING_FUNCTION    : 'mapping-function';
POST_ENCODING_FUNCTION : 'post-encoding-function';
POST_DECODING_VALIDATOR : 'post-decoding-validator';

PRESENT_WHEN		: 'present-when';
DETERMINANT			: 'determinant';

DEFINITIONS 	:	'DEFINITIONS';
BEGIN	:	'BEGIN';
END	:	'END';
CONSTANT 	:	'CONSTANT';
NOT 	:	'NOT';

INTEGER				: 'INTEGER';
BOOLEAN				: 'BOOLEAN';
NULL				: 'NULL';

EQUAL 	:	'==';



PLUS 	:	'+';
MINUS	:	'-';
MULTIPLICATION	:	'*';
DIVISION	:	'/';
MODULO	:	'%';

POWER_SYMBOL 	:	'^^';


ASSIG_OP		: '::=';
DOUBLE_COLON	: '::';
L_BRACKET	:	'{';	
R_BRACKET	:	'}';	
L_PAREN		:	'(';
R_PAREN		:	')';
COMMA		:	',';

DOUBLE_DOT  : '..';

DOT  : '.';



BitStringLiteral	:
	'\'' ('0'|'1')* '\'B'
	;

OctectStringLiteral	:
	'\'' ('0'..'9'|'a'..'f'|'A'..'F'|' ' | '\t' | '\r' | '\n')* '\'H'
	;

HexByteOrNible	:
	'0x' ('0'..'9'|'a'..'f'|'A'..'F') ('0'..'9'|'a'..'f'|'A'..'F')? 
	| '\'' ( options {greedy=false;} : .) '\''
	;	


StringLiteral 	: 	STR+ ;

fragment
STR 	:	'"' ( options {greedy=false;} : .)* '"' ;
			
UID  :   ('A'..'Z') ('a'..'z'|'A'..'Z'|'0'..'9'|'-')*
    ;

LID  :   ('a'..'z') ('a'..'z'|'A'..'Z'|'0'..'9'|'-')*
    ;

REAL    :	INT DOT (DIGITS)? (Exponent)?  ;

INT	:	( '0' | ('1'..'9') ('0'..'9')*);


	
fragment
DIGITS
	:	('0'..'9')+;



fragment
Exponent : ('e'|'E') ('+'|'-')? ('0'..'9')+ ;


WS  :   (' ' | '\t' | '\r' | '\n')+ {$channel=HIDDEN;}
    ;    

COMMENT
    :   '/*' ( options {greedy=false;} : . )* '*/' {$channel=HIDDEN;}
    ;

COMMENT2
    : '--' ~('\n'|'\r')* '\r'? '\n' {$channel=HIDDEN;}
    ;




