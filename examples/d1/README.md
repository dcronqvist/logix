# D1 Processor

The D1 processor is a 16-bit microprocessor.

## Instruction Set

The D1 processor has an 8-bit opcode, allowing for 256 unique instructions.

```
0x00: NOP

# Load and store (0x01 - 0x30)

0x01: LDA #$imm
0x02: LDA $addr
0x03: LDA %sp, $off
0x04: LDA $addr, $off

0x05: STA $addr
0x06: STA %sp, $off
0x07: STA $addr, $off

0x08: LDB #$imm
0x09: LDB $addr
0x0A: LDB %sp, $off
0x0B: LDB $addr, $off

0x0C: STB $addr
0x0D: STB %sp, $off
0x0E: STB $addr, $off

0x0F: LDX #$imm
0x10: LDX $addr
0x11: LDX %sp, $off
0x12: LDX $addr, $off

0x13: STX $addr
0x14: STX %sp, $off
0x15: STX $addr, $off

0x16: LDSP #$imm

# Register operations (0x30 - 0x40)

0x30: MOV %A, %B
0x31: MOV %A, %X
0x32: MOV %B, %A
0x33: MOV %B, %X
0x34: MOV %X, %A
0x35: MOV %X, %B

# Arithmetic and logic (0x40 - 0x70)

0x40: ADD %A (A = A + B)
0x41: ADD %B (B = A + B)
0x42: ADD %X (X = A + B)
0x43: ADD $addr ($addr = A + B)

0x44: SUB %A (A = A - B)
0x45: SUB %B (B = A - B)
0x46: SUB %X (X = A - B)
0x47: SUB $addr ($addr = A - B)

0x48: MUL %A (A = A * B)
0x49: MUL %B (B = A * B)
0x4A: MUL %X (X = A * B)
0x4B: MUL $addr ($addr = A * B)

0x4C: DIV %A (A = A / B)
0x4D: DIV %B (B = A / B)
0x4E: DIV %X (X = A / B)
0x4F: DIV $addr ($addr = A / B)

0x50: MOD %A (A = A % B)
0x51: MOD %B (B = A % B)
0x52: MOD %X (X = A % B)
0x53: MOD $addr ($addr = A % B)

0x54: AND %A (A = A & B)
0x55: AND %B (B = A & B)
0x56: AND %X (X = A & B)
0x57: AND $addr ($addr = A & B)

0x58: OR %A (A = A | B)
0x59: OR %B (B = A | B)
0x5A: OR %X (X = A | B)
0x5B: OR $addr ($addr = A | B)

0x5C: XOR %A (A = A ^ B)
0x5D: XOR %B (B = A ^ B)
0x5E: XOR %X (X = A ^ B)
0x5F: XOR $addr ($addr = A ^ B)

0x60: NOT %A (A = ~A)

0x61: LSH %A (A = A << 1)
0x62: RSH %A (A = A >> 1)

# Stack operations (0x70 - 0x90)

0x70: PUSH %A
0x71: PUSH %B
0x72: PUSH %X
0x73: PUSH %PC
0x74: PUSH $addr

0x75: POP %A
0x76: POP %B
0x77: POP %X
0x78: POP %PC
0x79: POP $addr

# Control flow (0x90 - 0xA0)

0x90: JMP $addr (unconditional jump)
0x91: JZS $addr (jump if zero flag set)
0x92: JZC $addr (jump if zero flag clear)
0x93: JCS $addr (jump if carry flag set)
0x94: JCC $addr (jump if carry flag clear)
0x95: JNS $addr (jump if negative flag set)
0x96: JNC $addr (jump if negative flag clear)

# Interrupts (0xA0 - 0xB0)

0xA0: INT $addr
0xA1: SINT #$imm (set interrupt with code)
0xA2: RINT (return from interrupt)

# Miscellaneous (0xF0 - 0xFF)

0xF0: HALT (halt execution)

```
