\ Example 8 KB ROM System for 
\ A Typist's 65c02 Assembler in Forth
\ Scot W. Stevenson <scot.stevenson@gmail.com>
\ This version: 05. Jan 2015

\ After assembly, this creates an 8 kb binary file that can (for example)
\ be loaded to $E000 a simulator such as the py65mon for testing 

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

   hex
   cr .( Starting assembly ... )

\ ----------------------------------- 

   0e000 origin 

   0f001 value putchr  \ py65mon address for character output
   0f004 value getchr  \ py65mon address to receive character input

   \ Macro to print one linefeed 
   : .lf ( -- ) 0a lda.#   putchr sta ; 

   \ All of our vectors go here to the start of the monitor
   -> vectors 

   \ Print the intro string
      lsb>  intro lda.#   00 sta.z
      msb>  intro lda.#   01 sta.z
       j>  prtstr jsr
                  .lf

   \ Print 10 x 'a' so we have at least one loop
               09 ldy.# 
           char a lda.#      
   -> nxta
           putchr sta
                  dey
             nxta bne

   \ done with all of this 
                  brk 


   \ Subroutine: Print a zero-terminated string. Assumes address in $00, $01
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


   \ Intro Strings
   -> intro
      s" ------------------------------------------------" strLF, 
      s" Testfile for A Typist's 65c02 Assembler in Forth" strLF, 
      s" Scot W. Stevenson <scot.stevenson@gmail.com>" strLF, 
      s" ------------------------------------------------" str0, 


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
