\ Example assembly source file for the Übersquirrel 65c02 Assembler
\ Scot W. Stevenson <scot.stevenson@gmail.com>
\ This version: 10 Dec 2014

\ Remember this is assembler source file is actually a Forth programm listing
\ as far as Forth is concerned. As such, the file type should be .fs instead
\ of .asm if you want correct syntax highlighting with an editor such as vi

\ Tool chain: Start Forth, INCLUDE asm65c02.fs, INCLUDE the file to be assembled

        \ we can use all the normal Forth commands; HEX should actually be 
        \ redundant
        hex

        \ comments marked with .( will be printed during assembly
        cr .( Starting assembly ... )

        \ .org sets target address on 65c02 machine
        \ use leading zeros with hex numbers to make sure they are not seen
        \ as words by the Forth interpreter
        0c000 .org      

        \ we can put more than one instruction in a row
        nop nop         

        \ store bytes with assembler commands, not (!) normal Forth C, 
        \ instructions
        0ff b, 0ff b, ff b,     

        \ store words in correct little-endian format
        1122 w, 3344 w, 

        \ more code
        brk 

        \ more comments printed to the screen
        .( all done.) cr 

        \ end assembly, put buffer address and length of compiled machine 
        .end            

        \ or have the machine print out the hex code at the end itself
        cr 2dup dump



