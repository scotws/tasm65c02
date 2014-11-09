\ The Übersquirrel 65c02 Forth Assembler
\ Scot W. Stevenson <scot.stevenson@gmail.com>
\ First version: 7. Nov 2014 ("N7 Day")
\ This version: 7. Nov 2014


wordlist >order definitions

hex 

variable startaddr
variable startcode


\ --- Helper Routines --- 
\ : store1 ( addr opcode-- addr )  \ Store single byte instruction
   \ over c! 1 + ; 
\ : store2 ( addr opcode -- addr )  \ Store double byte instruction
   \ rot tuck c! 1+ tuck c! 1+ ; 


\ --- Assembler Instructions ---


: origin ( addr -- )  \ Location of assembled program in memory
   startaddr ! ; 


\ --- Opcodes ---

: brk ( -- ) 00 c, ;  : nop ( -- ) 0ea c, ; 


: assemble ( -- addr u )   \ takes filename as parameter

   ." (TODO load file given )" cr

   pad 0ff accept
   pad swap type

   

   ." (TODO create symbol table)" cr

   here startcode !

   ." (TODO assemble)" 



