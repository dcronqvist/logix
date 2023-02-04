grammar D2Assembly;

prog: line* EOF;

line: symbolline | instrline | directiveline | constantline;

constantline:
	LABEL '=' (BINARY | HEXADECIMAL | DECIMAL) NEWLINE+;

directiveline: symbol? directive NEWLINE+;
directive: '.' (orgdir | worddir | asciizdir | dbdir);

orgdir: 'org' number;
worddir: 'word' number;
asciizdir: 'asciiz' STRINGLITERAL;
dbdir: 'db' nums += number (',' nums += number)*;

instrline: symbol? INSTRUCTION argument? NEWLINE+;
symbolline: symbol NEWLINE+;

symbol: LABEL ':';

argument:
	immediate
	| indirect = '(' number ')'
	| '(' number ')' ',' indirectx = 'x'
	| '(' number ')' ',' indirecty = 'y'
	| number ',' x = 'x'
	| number ',' y = 'y'
	| number;

immediate: '#' number;
number:
	bin = BINARY
	| hex = HEXADECIMAL
	| dec = DECIMAL
	| lab = LABEL
	| lowbyte = '<' number
	| highbyte = '>' number
	| number plus = '+' number
	| number minus = '-' number
	| number mult = '*' number
	| number and = '&' number
	| number or = '|' number;

INSTRUCTION:
	'lda'
	| 'sta'
	| 'ldx'
	| 'stx'
	| 'ldy'
	| 'sty'
	| 'tax'
	| 'tay'
	| 'txa'
	| 'tya'
	| 'ina'
	| 'inx'
	| 'iny'
	| 'inc'
	| 'dea'
	| 'dex'
	| 'dey'
	| 'dec'
	| 'adc'
	| 'sbc'
	| 'and'
	| 'ora'
	| 'eor'
	| 'rol'
	| 'ror'
	| 'pha'
	| 'pla'
	| 'phx'
	| 'plx'
	| 'phy'
	| 'ply'
	| 'php'
	| 'plp'
	| 'jmp'
	| 'jsr'
	| 'rts'
	| 'jeq'
	| 'jne'
	| 'jcs'
	| 'jcc'
	| 'jns'
	| 'jnc'
	| 'jvs'
	| 'jvc'
	| 'cmp'
	| 'bit'
	| 'clz'
	| 'sez'
	| 'clc'
	| 'sec'
	| 'cln'
	| 'sen'
	| 'clv'
	| 'sev'
	| 'cli'
	| 'sei'
	| 'rti'
	| 'lsp'
	| 'brk';

LABEL: [a-zA-Z][a-zA-Z0-9]*;
BINARY: '%' [0-1]+;
HEXADECIMAL: '$' [0-9a-fA-F]+;
DECIMAL: [0-9]+;

NEWLINE: [\r\n]+;
WS: [ \t\r] -> skip;

STRINGLITERAL: '"' (ESC | SAFECODEPOINT)* '"';

COMMENT: ';' ~[\r\n]* -> skip;

fragment ESC: '\\' (["\\/bfnrt]);

fragment SAFECODEPOINT: ~ ["\\\u0000-\u001F];
