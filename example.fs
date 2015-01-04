\ Example assembly source file for 
\ A Typist's 65c02 Assembler in Forth
\ Scot W. Stevenson <scot.stevenson@gmail.com>
\ This version: 04. Jan 2015

\ Remember this is assembler source file is actually a Forth programm listing
\ as far as Forth is concerned. As such, the file type should be .fs instead
\ of .asm if you want correct syntax highlighting with an editor such as vi

\ To test assembly: Start gforth, then INCLUDE tasm65c02.fs, INCLUDE example.fs
\ However, do not try to run the resulting code, which contains infinite loops!

        \ we can use all the normal Forth commands; HEX should actually be 
        \ redundant
        hex

        \ comments marked with .( will be printed during assembly
        cr .( Starting assembly ... )

        \ .origin sets target address on 65c02 machine. This is REQUIRED. 
        \ use leading zeros with hex numbers to make double sure they are 
        \ not interpreted as words by Forth 
        0c000 origin

        \ because this is actually a Forth file, we can put more than one 
        \ instruction in a row
        nop nop 

        \ instructions that have an operand put it before the opcode (the 
        \ Forth "reverse polish notation" (RPN) or "postfix" thing). See 
        \ MANUAL.txt for the syntax of various addressing modes
          00 lda.#     \ conventional syntax: lda #$00
             tax
        1020 sta.x     \ conventional syntax: sta $1020,x

        \ Special case: AND gets a dot in absolute mode to distinguish it 
        \ from the Forth command of the same name
        1021 and.

        \ store bytes with the B, assembler command, not the normal Forth C, 
        \ instruction because they go in the staging area, not the Forth
        \ dictionary
        0ff b, 

        \ if we want to do lots of this, we can just use a LOOP
        : 4xff ( -- )  4 0 do  0ff b,  loop ; 
        4xff

        \ store words in correct little-endian format with W,
        1122 w, 3344 w,    \ results in 22 11 44 33

        \ store strings with a combination of S" and STR, (S, is reserved 
        \ by gforth) 
        s" cats are cool" str, 

        \ define variables with VALUE instead of a special command such as
        \ ".EQU" because this is Forth
        88 value cat
             cat lda.#

        \ conditional assembly is trivial with Forth 
        : havecat? ( u --- ) 
            88 = if  s" nice" str,  else  s" damn" str,  then ; 

        cat havecat?  \ stores the "nice" string (of course) 

        \ .LC gives us the current address being assembled (the "*"
        \ of other assemblers). Use normal Forth math functions to 
        \ manipulate it
        lc 2 +   jmp 
                 nop 

        \ we define labels with "->" (yes, "-->" would be easier to read, 
        \ but it is already used by ancient Forth BLOCK commands). Put 
        \ the word at the very beginning of the line or the indent level
        -> hither

        \ backward jumps: just put the label (or absolute address) first,
        \ no other commands required
          hither jmp 
                 nop

        \ backward branches: work the same, because branch instructions 
        \ assume they will be handed either absolute addresses or labels 
          hither bra

        \ if we want to enter the relative address by hand, we trick the
        \ assembler by adding the offset in bytes to the current address
        \ (note this might warrent a separate word in future)
                 nop 
        lc 1-    bra 

        \ forward jumps are a pain in the rear for single-pass assemblers
        \ (which is what this is). We deal with this by having all forward
        \ jumps prefixed with "J>" 
        j>  frog jsr

        \ the same is true with all forward branches, we use "B>"
        b>  frog bne

        \ once we have defined the label, we can go back to using normal
        \ links as above
        -> frog
             0ff lda.# 
              00 sta.z 
            frog bra

        \ well, enough of this 
                 brk 

        \ more comments printed to the screen
        .( all done.) cr 

        \ end assembly, put buffer address and length of compiled machine 
        end            

        \ or have the machine print out the hex code at the end itself
        cr 2dup dump

        \ uncomment next line to save the hex dump to the file "example.bin"
        \ .save example.bin 
