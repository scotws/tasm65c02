\ A Typist's 65c02 Assembler in Forth 
\ Copyright 2015 Scot W. Stevenson <scot.stevenson@gmail.com>
\ Written with gforth 0.7
\ First version: 07. Nov 2014 ("N7 Day")
\ This version: 06. Jan 2015 (BETA 0.1) 

\ This program is free software: you can redistribute it and/or modify
\ it under the terms of the GNU General Public License as published by
\ the Free Software Foundation, either version 3 of the License, or
\ (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program.  If not, see <http://www.gnu.org/licenses/>.

hex

variable lc0  \ initial target address on 65c02 machine

0ffff 1+ constant maxmemory     \ 65c02 has 16 bit address space 
create staging maxmemory allot  \ buffer to store assembled machine code
staging maxmemory erase 

variable bc  0 bc !  \ buffer counter, offset to start of staging area

\ -----------------------
\ LOW LEVEL ASSEMBLER INSTRUCTIONS

\ Return least significant byte of 16-bit number
: lsb ( u -- u )  0ff and ; 

\ Return most significant byte of 16-bit number
: msb ( u -- u )  0ff00 and  8 rshift ; 

\ convert 16 bit address to little-endian
: swapbytes ( u -- uh ul )  dup msb swap lsb ; 

\ take a little-endian 16 bit address and turn it into a "normal"
\ big-endian number. Note stack order is reverse of swapbytes  
: unswapbytes ( ul uh - u ) 
   8 lshift  0ff00 and  or 
   0ffff and ;           \ paranoid 

: branchable? ( n -- f ) \ make sure branch offset is the right size
   -80 7f within ;

\ Calculate location counter from target address and buffer offset
: lc  ( -- )  lc0 @  bc @  + ; 

\ Save one byte in staging area
: b,  ( c -- )  staging  bc @  +  c!  1 bc +! ; 

\ Save one word in staging area, converting to little-endian
: w,  ( w -- )  swapbytes b, b, ; 

\ Save ASCII string provided by S" instruction (S, is reserved by gforth) 
\ Note OVER + SWAP is also BOUNDS in gforth 
: str, ( addr u -- )  over + swap  ?do  i c@ b,  loop ; 

\ Save zero-terminated ASCII string provided by S" instruction
: str0, ( addr u -- ) str, 0 b, ; 

\ Save linefeed-terminated  ASCII string provided by S" instruction
: strlf, ( addr u -- ) str, 0a b, ; 

\ -----------------------
\ HIGH LEVEL ASSEMBLER INSTRUCTIONS

\ set intial target address on 65c02 machine
: origin ( 65addr -- )  lc0 ! ; 

\ move to a given address, filling the space inbetween with zeros
: advance ( 65addr -- ) 
   staging bc @ +  ( 65addr addr )
   over lc -       ( 65addr addr u ) 
   erase           ( 65addr )
   lc0 @ -  bc ! ;

\ mark end of assembler source text, return buffer location and size
: end  ( -- addr u )  staging  bc @ ; 

\ save assembled program to file, overwriting any file with same name
: save ( addr u "name" -- )
   parse-name w/o create-file
   drop write-file if 
      ." Error writing file" then ; 

\ -----------------------
\ LABELS 

\ get current offset to start of staging area, adding one further byte 
\ for the JSR/JMP/BRA opcode in label definitions
: bc+1  ( -- offset)  bc @ 1+ ; 

\ replace dummy references to an ABSOLUTE address word in list of 
\ unresolved forward references by JMP/JSR with the real address. 
\ Used by "->" once we know what the actual address is 
\ We add the content of the dummy values to the later ones so we can do
\ addition and subtraction on the label before we know what it is
\ (eg. "j>  mylabel 1+ jmp" ) 
: addr>dummy          ( buffer-offset )
   staging +  dup c@  ( addr ul ) 
   over char+ c@      ( addr ul uh ) 
   unswapbytes        ( addr u ) 
   lc +  swapbytes    ( addr 65addr-h 65addr-l ) 
   rot tuck           ( 65addr-h addr 65addr-l addr ) 
   c!                 ( 65addr-h addr ) 
   char+              ( 65addr-h addr+1 ) 
   c! ; 

\ replace dummy references to an RELATIVE offset byte in list of 
\ unresolved forward references by BRA with the real offset.  
: rel>dummy  ( buffer-offset -- )
   dup staging +     ( b-off addr ) 
   bc @              ( b-off addr bc ) 
   rot -  1-         ( addr 65off ) 
   dup branchable? invert if
      cr ." Offset out of branching range (" . ." )" then 
   swap c! ; 

\ replace dummy reference to the MOST SIGNIFICANT BYTE in a list of 
\ forward references with the real MSB of the label
: msb>dummy  ( buffer-offset -- ) 
   staging +  lc msb  ( addr msb ) 
   swap c! ; 

\ replace dummy reference to the LEAST SIGNIFICANT BYTE in a list of 
\ forward references with the real LSB of the label
: lsb>dummy  ( buffer-offset -- ) 
   staging +  lc lsb  ( addr lsb ) 
   swap c! ; 

\ Use a jump table to handle replacement of dummy values, which should make 
\ it easier to add other addresing forms for other processors
create replacedummy  
      ' rel>dummy ,  ' addr>dummy , ' msb>dummy , ' lsb>dummy ,            


\ Handle forward unresolved references by either creating a new linked list
\ of locations in the staging area where they need to be inserted later, or
\ by adding a new entry to the list. This is a common routine for all 
\ forward references, which add their own offsets to the dummy replacement 
\ jump table and provide different dummy addresses for the following 
\ instructions (JSR/JMP, BRA, MSB>, LSB> etc)
: addlabel  ( "name" -- ) 
   parse-name 2dup find-name    ( addr u nt|0 )
   dup if
      \ address already defined, add another entry to the list 
      name>int    ( addr u xt )  \ gforth uses "name token" (nt), need xt
      >body       ( addr u l-addr ) 
      bc+1  swap  ( addr u offset l-addr ) 
      dup @       ( addr u offset l-addr old ) 
      swap here   ( addr u offset old l-addr here ) 
      swap !      ( addr u offset old )
      , ,         ( addr u )  \ save link to previous entry and data
      2drop       \ we didn't need name string after all
   else 
     \ new name, create new list. NEXTNAME is specific to 
     \ gforth and provides a string for a defining word such as CREATE
     drop nextname create 0 , bc+1 , 
   then ; 

 \ Create an unresolved RELATIVE forward label reference (for branches) 
: b>  ( "name" -- addr )  
   addlabel 
   0 ,           \ save zero as offset to replacement jump table 
   lc ;          \ use lc as dummy so we stay inside branch range 

\ Create an unresolved ABSOLUTE forward label reference (for jumps)
: j> ( "name" -- addr )
   addlabel 
   cell ,        \ save cell size as offset to replacement jump table
   0 ;           \ save 0000 as dummy so we can do addition and subtraction
                 \ on label even if we don't know what it is yet

\ Create an unresolved forward reference to the MOST SIGNIFICANT BYTE
\ of an unresolved forward label reference
: msb> ( "name" -- adr ) 
   addlabel 
   cell 2* ,     \ save 2* cell size as offset to replacement jump table
   0 ;           \ save 0000 as dummy value 

\ Create an unresolved forward reference to the LEAST SIGNIFICANT BYTE
\ of an unresolved forward label reference
: lsb> ( "name" -- adr ) 
   addlabel 
   cell 3 * ,    \ save 3* cell size as offset to replacement jump table
   0 ;           \ save 0000 as dummy value 


\ Define a label. Assume that the user knows what they are doing and
\ doesn't try to name label twice. If there were unresolved forward 
\ references, resolve them here and replace the complicated label handling
\ routine with simple new one. Yes, "-->" would be easer to read, but it 
\ is used by old block syntax of Forth
: ->  ( "name" -- )
   parse-name 2dup find-name    ( addr u nt|0 )

   \ if we have already used that name, it must be an unresolved 
   \ forward reference. Now we can replace the dummy values we have been 
   \ collecting with the real stuff 
   dup if      
      name>int    ( addr u xt )  \ gforth uses "name token" (nt), need xt
      >body       ( addr u l-addr ) 
      \ walk through the list and replace dummy addresses and offsets
      begin
         dup      ( addr u l-addr l-addr ) 
      while 
         dup cell+ @         ( addr u l-addr data ) 
         over 2 cells +  @   ( addr u l-addr data cell|0 ) 
         replacedummy + @  execute
         @                   ( addr u next-l-addr ) 
      repeat 
   then       ( addr u ) 

   \ (re)define the label
   drop nextname create lc , 
      does> @ ; 
  
\ -----------------------
\ DEFINE OPCODES 

: 1byte  ( opcode -- ) ( -- ) 
   create c,
   does> c@ b, ; 

: 2byte ( opcode -- ) ( c -- )
   create c,
   does> c@ b, b, ; 

: 3byte ( opcode -- ) ( w -- )
   create c,
   does> c@ b, w, ; 

\ caclulate branch
: makebranch ( w -- u ) 
   lc -  1-
   dup branchable? if 
      b, else
      drop ." Error: Branch out of range" then ; 

\ handel backward branch instructions 
\ Note BRANCH is reserved for Forth
: twig  ( opcode -- )  ( w -- ) 
   create c,
   does> c@ b, makebranch ; 

\ handle BBR/BBS instructions 
: testbranch ( opcode -- ) ( w u -- ) 
   create c,
   does> c@ b, b, makebranch ; 
   

\ -----------------------
\ OPCODE TABLES 
\ Brute force listing of each possible opcode. Leave undefined entries
\ empty so it is easier to port this program to other processors

\ OPCODES 00 - 0F 
00 1byte brk       01 2byte ora.zxi
04 2byte tsb.z     05 2byte ora.z     06 2byte asl.z     07 2byte rmb0.z
08 1byte php       09 2byte ora.#     0a 1byte asl.a
0c 3byte tsb       0d 3byte ora       0e 3byte asl       0f testbranch bbr0 

\ OPCODES 10 - 1F 
10 twig bpl        11 2byte ora.ziy   12 2byte ora.zi
14 2byte trb.z     15 2byte ora.zx    16 2byte asl.zx    17 2byte rmb1.z
18 1byte clc       19 3byte ora.y     1a 1byte inc.a
1c 3byte trb       1d 3byte ora.x     1e 3byte asl.x     1f testbranch bbr1

\ OPCODES 20 - 2F 
20 3byte jsr       21 2byte and.zxi
24 2byte bit.z     25 2byte and.z     26 2byte rol.z     27 2byte rmb2.z
28 1byte plp       29 2byte and.#     2a 1byte rol.a
2c 3byte bit       2d 3byte and.      2e 3byte rol       2f testbranch bbr2

\ OPCODES 30 - 3F 
30 twig bmi        31 2byte and.ziy   32 2byte and.zi
34 2byte bit.zxi   35 2byte and.zx    36 2byte rol.zx    37 2byte rmb3.z
38 1byte sec       39 3byte and.y     3a 1byte dec.a
3c 3byte bit.x     3d 3byte and.x     3e 3byte rol.x     3f testbranch bbr3

\ OPCODES 40 - 4F 
40 1byte rti       41 2byte eor.zxi
                   45 2byte eor.z     46 2byte lsr.z     47 2byte rbm4.z
48 1byte pha       49 2byte eor.#     4a 1byte lsr.a
4c 3byte jmp       4d 3byte eor       4e 3byte lsr       4f testbranch bbr4

\ OPCODES 50 - 5F 
50 twig bvc        51 2byte eor.ziy   52 2byte eor.zi
                   55 2byte eor.zx    56 2byte lsr.zx    57 2byte rmb5.z
58 1byte cli       59 3byte eor.y     5a 1byte phy
                   5d 3byte eor.x     5e 3byte lsr.x     5f testbranch bbr5

\ OPCODES 60 - 6F 
60 1byte rts       61 2byte adc.zxi 
64 2byte stz.z     65 2byte adc.z     66 2byte ror.z     67 2byte rmb6.z
68 1byte pla       69 2byte adc.#     6a 1byte ror.a
6c 3byte jmp.i     6d 3byte adc       6e 3byte ror       6f testbranch bbr6

\ OPCODES 70 - 7F 
70 twig bvs        71 2byte adc.ziy   72 2byte adc.zi
74 2byte stz.zx    75 2byte adc.zx    76 2byte ror.zx    77 2byte rmb7.z
78 1byte sei       79 3byte adc.y     7a 1byte ply
7c 3byte jmp.xi    7d 3byte adc.x     7e 3byte ror.x     7f testbranch bbr7

\ OPCODES 80 - 8F 
80 twig bra        81 2byte sta.zxi
84 2byte sty.z     85 2byte sta.z     86 2byte stx.z     87 2byte smb0.z
88 1byte dey       89 2byte bit.#     8a 1byte txa
8c 3byte sty       8d 3byte sta       8e 3byte stx       8f testbranch bbs0

\ OPCODES 90 - 9F 
90 twig bcc        91 2byte sta.ziy   92 2byte sta.zi
94 2byte sty.zx    95 2byte sta.zx    96 2byte stx.zy    97 2byte smb1.z
98 1byte tya       99 3byte sta.y     9a 1byte txs
9c 3byte stz       9d 3byte sta.x     9e 3byte stz.x     9f testbranch bbs1

\ OPCODES A0 - AF 
0a0 2byte ldy.#   0a1 2byte lda.zxi  0a2 2byte ldx.#
0a4 2byte ldy.z   0a5 2byte lda.z    0a6 2byte ldx.z    0a7 2byte smb2.z 
0a8 1byte tay     0a9 2byte lda.#    0aa 1byte tax
0ac 3byte ldy     0ad 3byte lda      0ae 3byte ldx      0af testbranch bbs2

\ OPCODES B0 - BF 
0b0 twig bcs      0b1 2byte lda.ziy  0b2 2byte lda.zi
0b4 2byte ldy.zx  0b5 2byte lda.zx   0b6 2byte ldx.zy   0b7 2byte smb3.z
0b8 1byte clv     0b9 3byte lda.y    0ba 1byte tsx
0bc 3byte ldy.x   0bd 3byte lda.x    0be 3byte ldx.y    0bf testbranch bbs3

\ OPCODES C0 - CF 
0c0 2byte cpy.#   0c1 2byte cmp.zxi
0c4 2byte cpy.z   0c5 2byte cmp.z    0c6 2byte dec.z    0c7 2byte smb4.z
0c8 1byte iny     0c9 2byte cmp.#    0ca 1byte dex      0cb 1byte stp
0cc 3byte cpy     0cd 3byte cmp      0ce 3byte dec      0cf testbranch bbs4

\ OPCODES D0 - DF 
0d0 twig bne      0d1 2byte cmp.ziy  0d2 2byte cmp.zi
                  0d5 2byte cmp.zx   0d6 2byte dec.zx   0d7 2byte smb5.z
0d8 1byte cld     0d9 3byte cmp.y    0da 1byte phx      0db 1byte wai
                  0dd 3byte cmp.x    0de 3byte dec.x    0df testbranch bbs5

\ OPCODES E0 - EF 
0e0 2byte cpx.#   0e1 2byte sbc.zxi
0e4 2byte cpx.z   0e5 2byte sbc.z    0e6 2byte inc.z    0e7 2byte smb6.z
0e8 1byte inx     0e9 2byte sbc.#    0ea 1byte nop
0ec 3byte cpx     0ed 3byte sbc      0ee 3byte inc      0ef testbranch bbs6

\ OPCODES F0 - FF 
0f0 twig beq      0f1 2byte sbc.ziy  0f2 2byte sbc.zi
                  0f5 2byte sbc.zx   0f6 2byte inc.zx   0f7 2byte smb7.z
0f8 1byte sed     0f9 3byte sbc.y    0fa 1byte plx
                  0fd 3byte sbc.x    0fe 3byte inc.x    0ff testbranch bbs7

\ END
