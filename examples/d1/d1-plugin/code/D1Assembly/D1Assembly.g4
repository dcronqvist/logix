grammar D1Assembly;

program: line*;

line: instrline | symbolline;

instrline: symbol? LABEL arglist? NEWLINE+;
symbolline: symbol;

symbol: LABEL ':';

arglist: args += argument (',' args += argument)*;
argument:
	REGISTER
	| LABEL
	| HEXADECIMAL
	| DECIMAL
	| IMDECIMAL
	| IMHEXADECIMAL;

REGISTER:
	'%' ('A' | 'a' | 'B' | 'b' | 'SP' | 'sp' | 'PC' | 'pc');

LABEL: [a-zA-Z][a-zA-Z0-9]*;
IMHEXADECIMAL: '#' HEXADECIMAL;
HEXADECIMAL: '$' [0-9a-fA-F]+;
IMDECIMAL: '#' DECIMAL;
DECIMAL: [0-9]+;

NEWLINE: [\r\n]+;
WS: [ \t\r] -> skip;