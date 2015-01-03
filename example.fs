\ Example assembly source file for 
\ A Typist's 65c02 Assembler in Forth
\ Scot W. Stevenson <scot.stevenson@gmail.com>
\ This version: 03. Jan 2015

\ Remember this is assembler source file is actually a Forth programm listing
\ as far as Forth is concerned. As such, the file type should be .fs instead
\ of .asm if you want correct syntax highlighting with an editor such as vi

\ To test: Start gforth, INCLUDE tasm65c02.fs, INCLUDE example.fs (this file)

        \ we can use all the normal Forth commands; HEX should actually be 
        \ redundant
        hex

        \ comments marked with .( will be printed during assembly
        cr .( Starting assembly ... )

        \ .org sets target address on 65c02 machine. This is REQUIRED. 
        \ use leading zeros with hex numbers to make double sure they are 
        \ not interpreted as not interpreted as words by Forth 
        0c000 .org      

        \ because this is actually a Forth file, we can put more than one 
        \ instruction in a row
        nop nop 

        \ instructions that have an operand put it before the opcode (the 
        \ Forth "reverse polish notation" (RPN) or "postfix" thing). See 
        \ MANUAL.txt for the syntax of various addressing modes
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


        \ .LC gives us the current address being assembled (the "*"
        \ of other assemblers). Use normal Forth math functions to 
        \ manipulate it
        .lc 2 +  jmp 
                 nop 

        \ we define labels with .L 
        .l comehere 

        \ backward jumps: just put the label (or absolute address) first
        comehere jmp 
                 nop

        \ backward branches: work the same, becasuse they assume labels
        \ (or absolute addresses) 
        .l cat1
                 nop 
            cat1 bra
                 nop 

        \ if we want to enter the relative address by hand, we trick the
        \ assembler by adding the offset to the current address (in bytes)
                nop 
        .lc 1-  bra 
                nop 


        brk 

        \ more comments printed to the screen
        .( all done.) cr 

        \ end assembly, put buffer address and length of compiled machine 
        .end            

        \ or have the machine print out the hex code at the end itself
        cr 2dup dump

        \ uncomment next line to save the hex dump to the file "example.bin"
        \ .save example.bin 
