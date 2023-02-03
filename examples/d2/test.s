.org $8000          ; Must start at $8000, since IO
                    ; is reserved for lower half

DDRA = $0001        ; Port A data direction register
PORTA = $0000       ; Port A data register
DDRB = $0003        ; Port B data direction register
PORTB = $0002       ; Port B data register

reset:
    sei             ; Enable interrupts
    ldx #$A0        
    ldy #$00        ; Load X and Y with $A000
    lsp             ; Set stack pointer to $A000 

    lda #$ff        ; Load value representing all pins as output
    sta DDRA        ; Set all pins as output

    lda #$ff        ; Load value representing all pins as output
    sta DDRB        ; Set all pins as output

loop:
    jsr printstring ; Call printstring subroutine
    lda #$0A        ; Load value representing new line
    jsr printchar   ; Call printchar subroutine
    jmp loop        ; Loop forever

printstring:
    phx
    ldx #$00        ; Load X with $00
printstringloop:
    lda message,x   ; x'th character in message
    bit message,x   ; Check value
    jeq exit        ; if zero, exit
    jsr printchar   ; Call printchar subroutine
    inx             ; Increment X 
    jmp printstringloop ; Jump to printstringloop
exit:
    plx
    rts

printchar:
    sta PORTB       ; Store value in PORTB to display on TTY
    adc #%10000000  ; Add 128 to value to enable 7th bit
    sta PORTB       ; Store value in PORTB to add char on TTY
    sbc #%10000000  ; Subtract 128 to value to disable 7th bit
    sta PORTB
    rts

irq:                ; Interrupt routine
    lda #$0C        ; Load value representing clear screen
    jsr printchar   ; Call printchar subroutine
    rti             ; Return from interrupt

message:
    .asciiz "Hello World!"

.org $fffc
.word irq
.word reset
