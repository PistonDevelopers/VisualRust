/*
 * Very simple module parsing grammar.
 * Works like rustc --dep-info, but handles broken files.
 */

grammar Module;


fragment
XID_start       : [_a-zA-Z] ;
fragment
XID_continue    : [_a-zA-Z0-9] ;
MOD             : 'mod';
LBRACE          : '{' ;
RBRACE          : '}' ;
SEMI            : ';' ;
IDENT           : XID_start XID_continue* ;
LINE_COMMENT    : '//' ~[\n]* { Skip(); };
BLOCK_COMMENT   : '/*' (BLOCK_COMMENT | .)*? '*/' { Skip(); };

ANY_CHAR: . { Skip(); };

body: (item | block | mod_import)*;
item: (IDENT | SEMI)+;
block: (MOD IDENT)? LBRACE body RBRACE;
mod_import: MOD IDENT SEMI;