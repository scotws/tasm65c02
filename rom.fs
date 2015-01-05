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
   0a value AscLF      \ ASCII linefeed

   \ All of our vectors go here to the start of the monitor
   -> vectors 

   \ print lines around string
           j> prtln jsr
           j> prtln jsr
                    brk

   \ Subroutine: Print a line
   -> prtln
             char - lda.#
                    phy
                 28 ldy.#
      -> nxtchr
             putchr sta
                    dey
             nxtchr bne
                    ply

              AscLF lda.#
             putchr sta

                    rts


   \ Strings
   -> str1
      s" Testfile for A Typist's 65c02 Assembler in Forth" str, 0 b,
   -> str2
      s" Scot W. Stevenson <scot.stevenson@gmail.com>" str, 0 b,

   \ move to interrupt vectors, filling rest of the image with zeros
   0fffa advance 
   
   vectors w, \ NMI vector
   vectors w, \ Reset vector
   vectors w, \ IRQ vector

   end            

\ ----------------------------------- 
   cr .( ... assembly finished. ) 

   \ uncomment next line to save the hex dump to the file "rom.bin"
   2dup save rom.bin 
