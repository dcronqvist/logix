grammar GateAlgebra;

component: assignment+;

assignment: variable '=' primary ';';

variable: ALPHA;

primary: expression (gate expression)*;

expression: variable | '(' primary ')' | 'not' expression;

gate: 'and' | 'or' | 'xor' | 'nor' | 'nand';

ALPHA: [A-Za-z]+;