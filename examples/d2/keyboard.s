.org $8000          ; Must start at $8000, since IO
                    ; is reserved for lower half

DDRA = $0001        ; Port A data direction register
PORTA = $0000       ; Port A data register
DDRB = $0003        ; Port B data direction register
PORTB = $0002       ; Port B data register

message:
    .asciiz "Hello World!"

printchar:
    ora #%10000000  ; XOR 128 to value to enable 7th bit
    sta PORTB       ; Store value in PORTB to add char on TTY
    lda #$00
    sta PORTB
    rts

irq:                ; Interrupt routine
    lda PORTA       ; Read value from PORTA
    jsr printchar   ; Call printchar subroutine
    lda #%10000000
    sta PORTA
    lda #$00
    sta PORTA
    rti             ; Return from interrupt

psptr = $B000
printstring:
    phx
    ldx #$00        ; Load X with $00
printstringloop:
    lda (psptr),x   ; x'th character in message
    bit (psptr),x   ; Check value
    jeq exit        ; if zero, exit
    jsr printchar   ; Call printchar subroutine
    inx             ; Increment X 
    jmp printstringloop ; Jump to printstringloop
exit:
    plx
    rts

reset:
    ldx #$A0        
    ldy #$00        ; Load X and Y with $A000
    lsp             ; Set stack pointer to $A000 

    lda #%10000000  ; all pins input
    sta DDRA        ; Set

    lda #$ff        ; Load value representing all pins as output
    sta DDRB        ; Set all pins as output

    lda #<message
    sta psptr
    lda #>message
    sta psptr + 1
    jsr printstring
    lda #$0A
    jsr printchar

    sei             ; Enable interrupts (we wait until having printed the message)

loop:
    jmp loop        ; Loop forever

.org $fffc
.word irq
.word reset
