\ Example assembly source file for the Übersquirrel 65c02 Assembler
\ Scot W. Stevenson <scot.stevenson@gmail.com>
\ This version: 12. Dec 2014

\ Remember this is assembler source file is actually a Forth programm listing
\ as far as Forth is concerned. As such, the file type should be .fs instead
\ of .asm if you want correct syntax highlighting with an editor such as vi

\ To test: Start gforth, INCLUDE asm65c02.fs, INCLUDE example.fs (this file)

        \ we can use all the normal Forth commands; HEX should actually be 
        \ redundant
        hex

        \ comments marked with .( will be printed during assembly
        cr .( Starting assembly ... )

        \ .org sets target address on 65c02 machine. REQUIRED. 
        \ use leading zeros with hex numbers to make double sure they are 
        \ not interpreted as not interpreted words by Forth 
        0c000 .org      

        \ because this is actually a Forth file, we can put more than one 
        \ instruction in a row
        nop nop nop

        \ instructions that have an operand put it before the opcode (the 
        \ Forth "reverse polish notation" (RPN) thing). See MANUAL.txt for 
        \ the syntax of various addressing modes
          00 lda.#     \ conventional syntax: lda #$00
             tax
        1020 sta.x     \ conventional syntax: sta $1020,x


        \ store bytes with the B, assembler command, not the normal Forth C, 
        \ instruction because they go in the staging area, not the Forth
        \ dictionary
        0ff b, 0ff b, 0ff b,     

        \ store words in correct little-endian format with W,
        1122 w, 3344 w, 

        \ store strings with S" and STR, (S, is reserved by gforth) 
        s" cats are cool" str, 

        \ define variables with .EQU
        88 .equ cat
        cat lda.#


        \ conditional assembly is trivial with Forth commands
        : havecat? ( u --- ) 
            88 = if  s" nice" str,  else  s" damn" str,  then ; 

        cat havecat?  \ stores the "nice" string (of course) 


        \ we define labels with .L 
        .l comehere 

        \ .LC gives us the current address being assembled (the "*"
        \ of other assemblers). Use normal Forth math functions to 
        \ manipulate it
        .lc 2 +  jmp 
        nop nop 

        brk 

        \ more comments printed to the screen
        .( all done.) cr 

        \ end assembly, put buffer address and length of compiled machine 
        .end            

        \ or have the machine print out the hex code at the end itself
        cr 2dup dump
