A Typist's 65c02 Assembler in Forth
Scot W. Stevenson <scot.stevenson@gmail.com>
First version: 07. Nov 2014 ("N7 Day")
This version: 03. Jan 2015

This is an assembler for the 65c02 8-bit MPU written in gforth. Technically, this makes it a cross assembler. 


WHAT YOU SHOULD KNOW

This assembler makes a few assumptions:

- It is written for people who have at least some basic familiarity with Forth. The Manual will do some hand-holding for people who are fairly new to the language, but not much.

- It assumes you are using gforth (https://www.gnu.org/software/gforth/). The source code points out where gforth-specific words are used.

- It is written for the 65c02 (more specifically, the WDC 65c02) only. It shouldn't be a problem to adapt it to, say, the origional 6502, but this version makes no such attempt. 

- It is written by and primary for people who touch-type -- hence the name. This is why there are few special characters (like $ or [) in the syntax: They take too much time to reach. 

