/*
 * Very simple module parsing grammar.
 * Works like rustc --dep-info, but handles broken files.
 */

grammar Module;


PUB             : 'pub';
MOD             : 'mod';
LBRACE          : '{' ;
RBRACE          : '}' ;
SEMI            : ';' ;
POUND           : '#' ;
LBRACKET        : '[' ;
RBRACKET        : ']' ;
EQ              : '=' ;

fragment
XID_start       : [_a-zA-Z] ;
fragment
XID_continue    : [_a-zA-Z0-9] ;
IDENT           : XID_start XID_continue* ;

LINE_COMMENT    : '//' ~[\n]* { Skip(); };
BLOCK_COMMENT   : '/*' (BLOCK_COMMENT | .)*? '*/' { Skip(); };

fragment HEXIT      : [0-9a-fA-F] ;
fragment CHAR_ESCAPE: [nrt\\'"0]
                    | [xX] HEXIT HEXIT
                    | 'u' HEXIT HEXIT HEXIT HEXIT
                    | 'U' HEXIT HEXIT HEXIT HEXIT HEXIT HEXIT HEXIT HEXIT ;
LIT_STR             : '"' ('\\\n' | '\\\r\n' | '\\' CHAR_ESCAPE | .)*? '"' ;


ANY_CHAR: . { Skip(); };

mod_block: attrs? PUB? MOD IDENT LBRACE body RBRACE;
mod_import: attrs? PUB? MOD IDENT SEMI;
body: (mod_import | mod_block | item)*?;
item: ( IDENT | SEMI | LBRACE | RBRACE | POUND | LBRACKET | IDENT | EQ | LIT_STR | RBRACKET | PUB);
attrs: attr+;
attr: POUND LBRACKET IDENT EQ LIT_STR RBRACKET;
