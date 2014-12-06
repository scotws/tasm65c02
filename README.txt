The Übersquirrel 65c02 Forth Assembler
Scot W. Stevenson <scot.stevenson@gmail.com>
First version: 7. Nov 2014 ("N7 Day")
This version: 06. Dez 2014

This is an assembler for the 65c02 8-bit MPU written in gforth. Technically, this makes it a cross assembler. 

WHAT YOU SHOULD KNOW


This assembler makes a few assumptions:

- It is written for people who have at least some basic familiarity with Forth. The Manual will do some hand-holding for people who are fairly new to the language, but not much.

- It assumes you are using gforth (LINK FEHLT). The source code points out where gforth-specific words are used.

- It is written for the 65c02 (more specifically, the WDC65c02) only. It shouldn't be a problem to adapt it to, say, the origional 6502, but this version makes no such attempt. 

- It is written by and primary for a person who is a touch-typist -- me. This is why there are few special characters (like $ or [) in the syntax: They take too much time to reach. 



