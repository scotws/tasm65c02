\ The Übersquirrel 65c02 Forth Cross-Assembler
\ Scot W. Stevenson <scot.stevenson@gmail.com>
\ First version: 7. Nov 2014 ("N7 Day")
\ This version: 10. Dez 2014

\ Written for gforth 0.7.0 

\ wordlist >order definitions

hex

variable lc0  \ initial target address on 65c02 machine

create staging 0ffff allot  \ 64k area to store assembled machine code
staging 0ffff erase 

variable bc  0 bc !  \ buffer counter, offset  TODO see if this should be TOS 

: swapbytes ( u -- u u )  \ convert to little-endian format
   dup 0ff00 and  8 rshift   
   swap 0ff and ; 

\ Calculate location counter from target address and buffer offset
\ TODO see if this should be closer to classical "*" variable
: .lc  ( -- )  lc0 @  bc @  + ; 

\ Save one byte in buffer. Don't use C, because we are not in Dictionary
: b,  ( c -- )  staging  bc @  +  c!  1 bc +! ; 

\ Save one word in buffer memory area, convert to little-endian
: w,  ( w -- )  swapbytes b, b, ; 

\ Save ASCII character string provided by S" instruction 
\ S, is reserved by gforth; note OVER + SWAP is BOUNDS in gforth 
: str, ( addr u -- ) over + swap  ?do i c@ b, loop ; 


\ -----------------------
\ assembler instructions

\ set intial target address on 65c02 machine
: .org  ( 65addr -- )  lc0 ! ; 

\ mark end of assembler source text, return buffer location and size
: .end  ( -- addr u )  staging  bc @ ; 

\ set a variable 
: .equ  ( u "name" -- ) ( -- u ) 
   create ,
   does> @ ; 

\ -----------------------
\ labels and branching 

\ Set a label 
: .l ( "n" -- ) ( -- u ) 
   create .lc ,
   does> @ ; 

  
\ -----------------------
\ Handle conversion and storage depending on instruction size 

: 1byte  ( opcode -- ) ( -- ) 
   create c,
   does> c@ b, ; 

: 2byte ( opcode -- ) ( c -- )
   create c,
   does> c@ b, b, ; 

: 3byte ( opcode -- ) ( w -- )
   create c,
   does> c@ b, w, ; 


\ -----------------------
\ one byte opcodes 

00 1byte brk   
08 1byte php   
0a 1byte asl 
0ea 1byte nop  

18 1byte clc
1a 1byte inc 

3a 1byte dec

40 1byte rti   

48 1byte inx

5a 1byte phy

60 1byte rts   

7a 1byte ply

0aa 1byte tax

0d8 1byte cld
0da 1byte phx

0fa 1byte plx


\ -----------------------
\ two byte opcodes 

12 1byte ora.zi 

32 2byte and.zi

52 2byte eor.zi

72 2byte adc.zi 

89 2byte bit.#

92 2byte sta.zi 

0a9 2byte lda.# 

0b2 2byte lda.zi

0d2 2byte cmp.zi 

0f2 2byte sbc.zi 


\ -----------------------
\ three byte opcodes 

4c 3byte jmp   

7c 3byte jmp.xi

8d 3byte sta

9d 3byte sta.x


   
\   TODO previous definitions
