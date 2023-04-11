grammar D2C;

program: preprocessordir* funcdef*;

preprocessordir: HASH preprodirs;

preprodirs: DEFINE ID rvalue;

funcdef: type ID LPAREN paramlist RPAREN block;
block: LCURLY stms = statement* RCURLY;

paramlist: decls += paramdecl? (COMMA decls += paramdecl)*;
paramdecl: type ID;

statement:
	vardecl SEMI
	| assignment SEMI
	| retstm SEMI
	| ifstatement
	| whileloop
	| BREAK SEMI
	| rvalue SEMI;

vardecl: type ID (EQ rvalue)?;
assignment: lvalue EQ rvalue;
funccall: ID LPAREN arglist RPAREN;
arglist: args += rvalue? (',' args += rvalue)*;

ifstatement: IF LPAREN rvalue RPAREN block ifstatementelse?;
ifstatementelse:
	ELSE block
	| ELSEIF LPAREN rvalue RPAREN block ifstatementelse?;

retstm: RETURN rvalue?;

whileloop: WHILE LPAREN rvalue RPAREN block;

lvalue:
	ID
	| STAR ptrlabel = ID
	| STAR ptrhex = HEXADECILIT
	| STAR ptrdec = DECIMALLIT;

rvalue:
	ID
	| LPAREN casted = type RPAREN rvalue
	| LPAREN paren = rvalue RPAREN
	| prefixop rvalue
	| rvalue postfixop
	| rvalue binop rvalue
	| funccall
	| DECIMALLIT
	| HEXADECILIT
	| BINARYLIT
	| STRINGLIT;

prefixop: MINUS | INCREMENT | DECREMENT | STAR | AMPERSAND;

postfixop: INCREMENT | DECREMENT;

binop:
	PLUS
	| MINUS
	| EQEQ
	| NEQ
	| AMPERSAND
	| BOR
	| BXOR
	| LESSTHAN
	| GREATHAN;

type: puretype | pointertype;

puretype: ID;
pointertype: puretype STAR;

BOR: '|';
BXOR: '^';
PLUS: '+';
INCREMENT: '++';
DECREMENT: '--';
MINUS: '-';
STAR: '*';
HASH: '#';
AMPERSAND: '&';
DIV: '/';
EQ: '=';
EQEQ: '==';
NEQ: '!=';
LESSTHAN: '<';
GREATHAN: '>';
COMMA: ',';
SEMI: ';';
LPAREN: '(';
RPAREN: ')';
LCURLY: '{';
RCURLY: '}';
IF: 'if';
ELSE: 'else';
ELSEIF: 'elseif';
RETURN: 'return';
FOR: 'for';
WHILE: 'while';
BREAK: 'break';
DEFINE: 'define';
ORG: 'org';

DECIMALLIT: [0-9]+;
HEXADECILIT: '0x' [a-fA-F0-9]+;
BINARYLIT: '0b' [0-1]+;
STRINGLIT: '"' (ESC | SAFECODEPOINT)* '"';

COMMENT: '//' ~[\r\n]* -> skip;

ID: [a-zA-Z_][a-zA-Z_0-9]*;
WS: [ \t\n\r\f]+ -> skip;

fragment ESC: '\\' (["\\/bfnrt]);
fragment SAFECODEPOINT: ~ ["\\\u0000-\u001F];