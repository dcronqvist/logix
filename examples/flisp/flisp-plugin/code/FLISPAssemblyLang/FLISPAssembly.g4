grammar FLISPAssembly;

program: linelist EOF;

linelist: lines += line (LINEBREAK+ lines += line)*;

line: (SYMB ':')? WS? (directive | instr) WS?;

directive: orgdir | equdir | fcbdir | fcsdir | rmbdir;

orgdir: 'ORG' WS number;
equdir: SYMB ':' WS 'EQU' WS number;
fcbdir: 'FCB' WS numberlist;
fcsdir: 'FCS' WS STRINGLITERAL;
rmbdir: 'RMB' WS number;

instr: ISTRING | ISTRING WS number | ISTRING WS immediate;

numberlist: number | number WS* ',' WS* numberlist;

immediate: '#' number;

number: HEXADECI | DECI | SYMB;

STRINGLITERAL: '"' ~["]* '"';
ISTRING: [A-Z]+;
DECI: DIGITS;
SYMB: [a-zA-Z0-9]+;
HEXADECI: '$' [0-9a-fA-F]+;
WS: [ \t]+;
DIGITS: [0-9]+;
LINEBREAK: [\r\n];