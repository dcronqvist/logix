grammar FLISPC;

program: funcs += funcdef? (funcs += funcdef)*;

funcdef: 'byte' SYMBOL '(' paramlist? ')' block;

block: '{' lines += statement? (lines += statement)* '}';

statement:
	declaration
	| assignment
	| ifstatement
	| whilestatement
	| return;

whilestatement: 'while' '(' cond = expr ')' block;

ifstatement: 'if' '(' cond = expr ')' block 'else' block;

paramlist:
	params += 'byte' SYMBOL (',' params += 'byte' SYMBOL)*;

funccall: SYMBOL '(' arglist ')';

arglist: exprs += expr (',' exprs += expr)*;

declaration: 'byte' SYMBOL '=' expr ';';

assignment: SYMBOL '=' expr ';';

return: 'return' expr ';';

expr:
	SYMBOL
	| LITERAL
	| expr mult = '*' expr
	| expr add = '+' expr
	| expr sub = '-' expr
	| func = funccall
	| '(' paren = expr ')';

SYMBOL: [a-zA-Z][a-zA-Z0-9]*;
LITERAL: '0x' [0-9a-fA-Z]+;
WS: [ \t\r\n]+ -> skip;