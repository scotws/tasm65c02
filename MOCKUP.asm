\ Testing area for various formats

\ Original loop
        lda #$00
        tax
loop    sta $1000,x
        dex
        bne loop
        brk


\ just written down 

        00 lda.i
        tax
        1000 sta.x
        dex
        loop bne
        brk


\ Center spaced (pretty printed)

          00 lda.i
             tax
.l loop
        1000 sta.x
             dex
        loop bne
             brk 



\ Extract from Tali Forth 

.l f_toupper

        char a cmp.#
        _done bcc
        char z 1+ cmp.#
        _done bcs

        sec
        20 sbc.i
        rts
