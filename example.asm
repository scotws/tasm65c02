\ Example assembler file for the Übersquirrel 65c02 Assembler
\ Scot W. Stevenson <scot.stevenson@gmail.com>
\ This version: 07. Nov 2014

\ Comments start with a backslash like in all of Forth instead of a ; 

        hex     \ we can use all the normal Forth commands

        nop     \ opcode-only instructions are the same

        0ff c, 0ff c, ff c,     \ Store data with normal Forth commands

        nop
        brk


