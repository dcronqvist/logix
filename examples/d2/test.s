.org $8000          ; Must start at $8000, since IO
                    ; is reserved for lower half

DDRA = $0001        ; Port A data direction register
PORTA = $0000       ; Port A data register

reset:
    lda #$ff        ; Load value representing all pins as output
    sta DDRA        ; Set all pins as output

    lda #$01        ; Load inital value for rotating LED

loop:
    rol             ; Rotate left
    sta PORTA       ; Store value in PORTA to display on LEDs
    jmp loop        ; Loop back to rotate again

irq:                ; Interrupt routine
    rti             ; Do nothing, return from interrupt
    
.org $fffc
.word irq
.word reset
