grammar ActionSequence;

@header {#pragma warning disable 3021}

program: actionSequence EOF;

// Allow newlines
actionSequence: (action WHITESPACE*)+;

action: (
		assignment
		| wait
		| end
		| continue
		| print
		| push
		| connectKeyboard
		| connectTTY
		| mountDisk
		| connectLEDMatrix
	) ';';

wait: 'wait' ' ' (boolexp | DECIMAL_LITERAL);
assignment: 'set' ' ' (pinexp | ramexp) '=' exp;
end: 'end';
continue: 'continue';
print: 'print' ' ' STRING_LITERAL;
push:
	'push' (' ')* PIN_ID ',' (' ')* (boolexp | DECIMAL_LITERAL);
connectKeyboard: 'connect_keyboard' (' ')* PIN_ID;
connectTTY: 'connect_tty' (' ')* PIN_ID;
mountDisk:
	'mount_disk' (' ')* PIN_ID (' ')* ',' (' ')* STRING_LITERAL;
connectLEDMatrix:
	'connect_ledmatrix' (' ')* PIN_ID (' ')* ',' (' ')* DECIMAL_LITERAL;

exp: pinexp | ramexp | literalexp;
literalexp: BINARY_LITERAL | HEX_LITERAL;
ramexp: PIN_ID '[' (HEX_LITERAL | BINARY_LITERAL) ']';
pinexp: PIN_ID;

boolexp:
	exp '==' exp
	| exp '!=' exp
	| boolexp ' ' '&&' ' ' boolexp
	| boolexp ' ' '||' ' ' boolexp
	| '(' boolexp ')';

BINARY_LITERAL: '0b' ([01]+);
HEX_LITERAL: '0x' ([0-9a-fA-F]+);
DECIMAL_LITERAL: [0-9]+;
STRING_LITERAL: '"' ~[^"\r\n]* '"';
PIN_ID: [a-zA-Z0-9_]+;
WHITESPACE: [\t\r\n ]+ -> skip;
LINECOMMENT: '//' ~[\r\n]* -> skip;