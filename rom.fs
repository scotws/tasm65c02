\ Example 8 KB ROM System for 
\ A Typist's 65c02 Assembler in Forth
\ Scot W. Stevenson <scot.stevenson@gmail.com>
\ This version: 05. Jan 2015

\ After assembly, this creates an 8 kb binary file that can be 
\ loaded to $E000 a simulator such as the py65mon for testing 
\ You can use this as template for your own system

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

   hex
   cr .( Starting assembly ... )

\ --- DEFINITIONS ---

   0e000 origin  \ required  

   0f001 value putchr  \ py65mon address for character output
   0f004 value getchr  \ py65mon address to receive character input

\ --- STRINGS ---
\ Life is easier if you put these at the beginning of the text: It's
\ tricky to put unresolved forward references in macros

   -> intro
      s" ------------------------------------------------" strlf, 
      s" Testfile for A Typist's 65c02 Assembler in Forth" strlf, 
      s" Scot W. Stevenson <scot.stevenson@gmail.com>" strlf, 
      s" ------------------------------------------------" str0, 


\ --- SUBROUTINES --- 
\ These, too, should go before the main code if at all possible

   \ Print a zero-terminated string. Assumes address in $00, $01
   -> prtstr
                  phy 
               00 ldy.#
   -> nxtchr
               00 lda.ziy
         b>  fini beq
           putchr sta
                  iny
           nxtchr bra
   -> fini 
                  ply
                  rts

\ --- MACROS ---
\ In contrast to normal assemblers, our macros don't do so well if 
\ they are first in the file. In this case, putting .STR here lets 
\ us access the strings and call the prtstr subroutine without 
\ much hassle

   \ Macro to print one linefeed
   : .linefeed  ( -- )   0a lda.#   putchr sta ; 

   \ Macro to print a string. Note this doesn't work with strings
   \ that were defined lower down because it gets tricky with
   \ unresolved links. Gforth already uses .STRING 
   : .str ( link -- ) 
      dup lsb  lda.#   00 sta.z
          msb  lda.#   01 sta.z
        prtstr jsr ; 


\ --- MAIN CODE --- 

   \ All of our vectors go here because we're cheap 
   -> vectors 

   \ Print the intro string
   intro .str  .linefeed 

   \ Print 10 x 'a' so we have at least one loop

               09 ldy.# 
           char a lda.#      
   -> nxta
           putchr sta
                  dey
             nxta bne

   \ done with all of this 
                  brk 


\ --- INTERRUPT VECTORS --- 
   
   \ skip to interrupt vectors, filling rest of the image with zeros
   0fffa advance 
   
   vectors w, \ NMI vector
   vectors w, \ Reset vector
   vectors w, \ IRQ vector

   end            

\ ----------------------------------- 
   cr .( ... assembly finished. ) 

   \ uncomment next line to save the hex dump to the file "rom.bin"
   \ 2dup save rom.bin 
