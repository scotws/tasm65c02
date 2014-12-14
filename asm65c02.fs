\ The Übersquirrel 65c02 Forth Cross-Assembler
\ Scot W. Stevenson <scot.stevenson@gmail.com>
\ First version: 7. Nov 2014 ("N7 Day")
\ This version: 14. Dez 2014

\ Written for gforth 0.7.0 

hex

variable lc0  \ initial target address on 65c02 machine

create staging 0ffff allot  \ 64k area to store assembled machine code
staging 0ffff erase 

variable bc  0 bc !  \ buffer counter, offset  TODO see if this should be TOS 

: swapbytes ( u -- u u )  \ convert to little-endian format
   dup 0ff00 and  8 rshift   
   swap 0ff and ; 

: branchable? ( n -- f ) \ make sure branch offset is the right size
   -80 7f within ;

\ Calculate location counter from target address and buffer offset
: .lc  ( -- )  lc0 @  bc @  + ; 

\ Save one byte in buffer. Don't use C, because we are not in Dictionary
: b,  ( c -- )  staging  bc @  +  c!  1 bc +! ; 

\ Save one word in buffer memory area, convert to little-endian
: w,  ( w -- )  swapbytes b, b, ; 

\ Save ASCII character string provided by S" instruction 
\ S, is reserved by gforth; note OVER + SWAP is also BOUNDS in gforth 
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

\ set a label 
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

\ Handel branch instructions TODO
\ BRANCH is reserved for Forth
: twig  ( -- ) 
   create c,
   does> ." Not coded yet!" ;

\ Handle BBR instructions TODO
: testbranch ( -- ) 
   create c,
   does> ." Not coded yet!" ; 


\ -----------------------

\ OPCODES 00 - 0F 
00 1byte brk       01 2byte ora.zxi   02 1byte nop       03 1byte nop 
04 2byte tsb.z     05 2byte ora.z     06 2byte asl.z     07 2byte rmb0.z
08 1byte php       09 2byte ora.#     0a 1byte asl.a     0b 1byte nop 
0c 3byte tsb       0d 3byte ora       0e 3byte asl       0f testbranch bbr0 

\ OPCODES 10 - 1F 
10 twig bpl        11 2byte ora.ziy   12 2byte ora.zi    13 1byte nop 
14 2byte trb.z     15 2byte ora.zx    16 2byte asl.zx    17 2byte rmb1.z
18 1byte clc       19 3byte ora.y     1a 1byte inc.a     1b 1byte nop
1c 3byte trb       1d 3byte ora.x     1e 3byte asl.x     1f testbranch bbr1

\ OPCODES 20 - 2F 
20 3byte jsr       21 2byte and.zxi   22 1byte nop       23 1byte nop
24 2byte bit.z     25 2byte and.z     26 2byte rol.z     27 2byte rmb2.z
28 1byte plp       29 2byte and.#     2a 1byte rol.a     2b 1byte nop 
2c 3byte bit       2d 3byte and.      2e 3byte rol       2f testbranch bbr2

\ OPCODES 30 - 3F 
30 twig bmi        31 2byte and.ziy   32 2byte and.zi    33 1byte nop
34 2byte bit.zxi   35 2byte and.zx    36 2byte rol.zx    37 2byte rmb3.z
38 1byte sec       39 3byte and.y     3a 1byte dec.a     3b 1byte nop 
3c 3byte bit.x     3d 3byte and.x     3e 3byte rol.x     3f testbranch bbr3

\ OPCODES 40 - 4F 
40 1byte rti       41 2byte eor.zxi   42 1byte nop       43 1byte nop 
44 1byte nop       45 2byte eor.z     46 2byte lsr.z     47 2byte rbm4.z
48 1byte pha       49 2byte eor.#     4a 1byte lsr.a     4b 1byte nop
4c 3byte jmp       4d 3byte eor       4e 3byte lsr       4f testbranch bbr4

\ OPCODES 50 - 5F 
50 twig bvc        51 2byte eor.ziy   52 2byte eor.zi    53 1byte nop 
54 1byte nop       55 2byte eor.zx    56 2byte lsr.zx    57 2byte rmb5.z
58 1byte cli       59 3byte eor.y     5a 1byte phy       5b 1byte nop 
5c 1byte nop       5d 3byte eor.x     5e 3byte lsr.x     5f testbranch bbr5

\ OPCODES 60 - 6F 
60 1byte rts       61 2byte adc.zxi   62 1byte nop       63 1byte nop 
64 2byte stz.z     65 2byte adc.z     66 2byte ror.z     67 2byte rmb6.z
68 1byte pla       69 2byte adc.#     6a 1byte ror.a     6b 1byte nop 
6c 3byte jmp.i     6d 3byte adc       6e 3byte ror       6f testbranch bbr6

\ OPCODES 70 - 7F 
70 twig bvc        71 2byte adc.ziy   72 2byte adc.zi    73 1byte nop 
74 2byte stz.zx    75 2byte adc.zx    76 2byte ror.zx    77 2byte rmb7.z
78 1byte sei       79 3byte adc.y     7a 1byte ply       7b 1byte nop
7c 3byte jmp.xi    7d 3byte adc.x     7e 3byte ror.x     7f testbranch bbr7

\ OPCODES 80 - 8F 
80 twig bra        81 2byte sta.zxi   82 1byte nop       83 1byte nop 
84 2byte sty.z     85 2byte sta.z     86 2byte stx.z     87 2byte smb0.z
88 1byte dey       89 2byte bit.#     8a 1byte txa       8b 1byte nop
8c 3byte sty       8d 3byte sta       8e 3byte stx       8f testbranch bbs0

\ OPCODES 90 - 9F 
90 twig bcc        91 2byte sta.ziy   92 2byte sta.zi    93 1byte nop 
94 2byte sty.zx    95 2byte sta.zx    96 2byte stx.zy    97 2byte smb1.z
98 1byte tya       99 3byte sta.y     9a 1byte txs       9b 1byte nop 
9c 3byte stz       9d 3byte sta.x     9e 3byte stz.x     9f testbranch bbs1

\ OPCODES A0 - AF 
0a0 2byte ldy.#   0a1 2byte lda.zxi  0a2 2byte ldx.#    0a3 1byte nop
0a4 2byte ldy.z   0a5 2byte lda.z    0a6 2byte ldx.z    0a7 2byte smb2.z 
0a8 1byte tay     0a9 2byte lda.#    0aa 1byte tax      0ab 1byte nop 
0ac 3byte ldy     0ad 3byte lda      0ae 3byte ldx      0af testbranch bbs2

\ OPCODES B0 - BF 
0b0 twig bcs      0b1 2byte lda.ziy  0b2 2byte lda.zi   0b3 1byte nop 
0b4 2byte ldy.zx  0b5 2byte lda.zx   0b6 2byte ldx.zy   0b7 2byte smb3.z
0b8 1byte clv     0b9 3byte lda.y    0ba 1byte tsx      0bb 1byre nop 
0bc 3byte ldy.x   0bd 3byte lda.x    0be 3byte ldx.y    0bf testbranch bbs3

\ OPCODES C0 - CF 
0c0 2byte cpy.#   0c1 2byte cmp.zxi  0c2 1byte nop      0c3 1byte nop 
0c4 2byte cpy.z   0c5 2byte cmp.z    0c6 2byte dec.z    0c7 2byte smb4.z
0c8 1byte iny     0c9 2byte cmp.#    0ca 1byte dex      0cb 1byte stp
0cc 3byte cpy     0cd 3byte cmp      0ce 3byte dec      0cf testbranch bbs4

\ OPCODES D0 - DF 
0d0 twig bne      0d1 2byte cmp.z    0d2 2byte cmp.zi   0d3 1byte nop 
0d4 1byte nop     0d5 2byte cmp.zx   0d6 2byte dec.zx   0d7 2byte smb5.z
0d8 1byte cld     0d9 3byte cmp.y    0da 1byte phx      0db 1byte wai
0dc 1byte nop     0dd 3byte cmp.x    0de 3byte dec.x    0df testbranch bbs5

\ OPCODES E0 - EF 
0e0 2byte cpx.#   0e1 2byte sbc.zxi  0e2 1byte nop      0e3 1byte nop  
0e4 2byte cpx.z   0e5 2byte sbc.z    0e6 2byte inc.z    0e7 2byte smb6.z
0e8 1byte inx     0e9 2byte sbc.#    0ea 1byte nop      0eb 1byte nop 
0ec 3byte cpx     0ed 3byte sbc      0ee 3byte inc      0ef testbranch bbs6

\ OPCODES F0 - FF 
0f0 twig beq      0f1 2byte sbc.ziy  0f2 2byte sbc.zi   0f3 1byte nop 
0f4 1byte nop     0f5 2byte sbc.zx   0f6 2byte inc.zx   0f7 2byte smb7.z
0f8 1byte sed     0f9 3byte sbc.y    0fa 1byte plx      0fb 1byte nop 
0fc 1byte nop     0fd 3byte sbc.x    0fe 3byte inc.x    0ff testbranch bbs7

\ END
\ -----------------------
