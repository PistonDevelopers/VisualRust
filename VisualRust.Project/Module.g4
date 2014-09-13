/*
 * Very simple module parsing grammar.
 * Servers the same purpose as rustc --dep-info, but handles broken files.
 */

grammar Module;

MOD     : 'mod';
LBRACE  : '{' ;
RBRACE  : '}' ;
SEMI    : ';' ;

fragment XID_start : [_a-zA-Z] ;
fragment XID_continue : [_a-zA-Z0-9] ;
IDENT : XID_start XID_continue* ;

ANY_CHAR: . { Skip(); };

body: (item | block | mod_import)*;
item: (IDENT | SEMI)+;
block: (MOD IDENT)? LBRACE body RBRACE;
mod_import: MOD IDENT SEMI;