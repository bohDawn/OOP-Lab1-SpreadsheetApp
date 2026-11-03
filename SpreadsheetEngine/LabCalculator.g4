grammar LabCalculator;

compileUnit : expression EOF;

expression :
	operatorToken=(ADD | SUBTRACT) expression #UnaryExpr
	| <assoc=right> expression EXPONENT expression #ExponentialExpr
	| expression operatorToken=(MULTIPLY | DIVIDE) expression #MultiplicativeExpr
	| expression operatorToken=(ADD | SUBTRACT) expression #AdditiveExpr
	| functionToken=(MAX | MIN) LPAREN expression COMMA expression RPAREN #FunctionExpr
	| LPAREN expression RPAREN #ParenthesizedExpr
	| IDENTIFIER #IdentifierExpr
	| NUMBER #NumberExpr
	;

MAX : [mM][aA][xX];
MIN : [mM][iI][nN];

IDENTIFIER : [a-zA-Z]+[0-9]+;
NUMBER : [0-9]+;

EXPONENT : '^';
MULTIPLY : '*';
DIVIDE : '/';
SUBTRACT : '-';
ADD : '+';
LPAREN : '(';
RPAREN : ')';
COMMA : ',';

WS : [ \t\r\n]+ -> skip;
