# The D2 processor

Little endian.

Buses:
1x 16-bit address bus
1x 8-bit data bus

Registers:
1x 8-bit accumulator (a)
1x 8-bit temporary register (t)
1x 8-bit result register (r)
1x 8-bit status register (s) (flags: zero, carry, negative, overflow, interrupt mask, unused, unused, unused)
1x 16-bit program counter (pc)
1x 16-bit stack pointer (sp)
2x 8-bit index register (x, y)
1x 16-bit memory address register (mar)

ALU has data bus and temporary register as inputs, result register as output, also status register as flag output.
ALU functions:
zero (output 0)
ff (output 0xff)
fe (output 0xfe)
fc (output 0xfc)
add (bus + t)
subtract (bus - t)
bitwise and (bus & t)
bitwise or (bus | t)
bitwise exclusive or (bus ^ t)
rotate left (bus << 1)
rotate right (bus >> 1)

Addressing modes:

implicit (instruction has no operands)
immediate (instruction has an 8-bit or 16-bit immediate operand)
absolute (instruction has a 16-bit absolute address operand)
absolute,%(x/y) (instruction has a 16-bit absolute address operand and a register, final operand is the sum of the two)

Instructions:

00: nop (no operation)

01: lda #imm (load accumulator with immediate)
02: lda abs (load accumulator with M[abs])
03: lda abs,x (load accumulator with M[abs + x])
04: lda abs,y (load accumulator with M[abs + y])
05: lda (abs), x (load accumulator with M[M[abs + x + 1] << 8 | M[abs + x]]])
06: lda (abs), y (load accumulator with M[M[abs + y + 1] << 8 | M[abs + y]]])
07: sta abs (store accumulator in M[abs])
08: sta abs,x (store accumulator in M[abs + x])
09: sta abs,y (store accumulator in M[abs + y])

08: ldx #imm (load x with immediate)
09: ldx abs (load x with M[abs])
0A: ldx abs,x (load x with M[abs + x])
0B: ldx abs,y (load x with M[abs + y])
0C: stx abs (store x in M[abs])
0D: stx abs,x (store x in M[abs + x])
0E: stx abs,y (store x in M[abs + y])

0F: ldy #imm (load y with immediate)
10: ldy abs (load y with M[abs])
11: ldy abs,x (load y with M[abs + x])
12: ldy abs,y (load y with M[abs + y])
13: sty abs (store y in M[abs])
14: sty abs,x (store y in M[abs + x])
15: sty abs,y (store y in M[abs + y])

16: tax (transfer accumulator to x)
17: tay (transfer accumulator to y)
18: txa (transfer x to accumulator)
19: tya (transfer y to accumulator)

1A: ina (increment accumulator)
1B: inx (increment x)
1C: iny (increment y)
1D: inc abs (increment M[abs])
1E: inc abs,x (increment M[abs + x])
1F: inc abs,y (increment M[abs + y])

20: dea (decrement accumulator)
21: dex (decrement x)
22: dey (decrement y)
23: dec abs (decrement M[abs])
24: dec abs,x (decrement M[abs + x])
25: dec abs,y (decrement M[abs + y])

26: adc #imm (add immediate to accumulator with carry)
27: adc abs (add M[abs] to accumulator with carry)
28: adc abs,x (add M[abs + x] to accumulator with carry)
29: adc abs,y (add M[abs + y] to accumulator with carry)

2A: sbc #imm (subtract immediate from accumulator with carry)
2B: sbc abs (subtract M[abs] from accumulator with carry)
2C: sbc abs,x (subtract M[abs + x] from accumulator with carry)
2D: sbc abs,y (subtract M[abs + y] from accumulator with carry)

2E: and #imm (bitwise and accumulator with immediate)
2F: and abs (bitwise and accumulator with M[abs])
30: and abs,x (bitwise and accumulator with M[abs + x])
31: and abs,y (bitwise and accumulator with M[abs + y])

32: ora #imm (bitwise or accumulator with immediate)
33: ora abs (bitwise or accumulator with M[abs])
34: ora abs,x (bitwise or accumulator with M[abs + x])
35: ora abs,y (bitwise or accumulator with M[abs + y])

36: eor #imm (bitwise exclusive or accumulator with immediate)
37: eor abs (bitwise exclusive or accumulator with M[abs])
38: eor abs,x (bitwise exclusive or accumulator with M[abs + x])
39: eor abs,y (bitwise exclusive or accumulator with M[abs + y])

3A: rol (rotate accumulator left)
3B: ror (rotate accumulator right)

3C: pha (push accumulator on stack)
3D: pla (pull accumulator from stack)
3E: phx (push x on stack)
3F: plx (pull x from stack)
40: phy (push y on stack)
41: ply (pull y from stack)
42: php (push flags)
43: plp (pull flags)

44: jmp abs (jump to absolute address)
45: jsr abs (jump to subroutine at absolute address)
46: rts (return from subroutine)

47: jeq abs (jump to address if zero flag is set)
48: jne abs (jump to address if zero flag is not set)
49: jcs abs (jump to address if carry flag is set)
4A: jcc abs (jump to address if carry flag is not set)
4B: jns abs (jump to address if negative flag is set)
4C: jnc abs (jump to address if negative flag is not set)
4D: jvs abs (jump to address if overflow flag is set)
4E: jvc abs (jump to address if overflow flag is not set)

4F: cmp #imm (compare accumulator with immediate)
50: cmp abs (compare accumulator with M[abs])
51: cmp abs,x (compare accumulator with M[abs + x])
52: cmp abs,y (compare accumulator with M[abs + y])

53: bit #imm (bitwise and accumulator with immediate, set flags)
54: bit abs (bitwise and accumulator with M[abs], set flags)
55: bit abs,x (bitwise and accumulator with M[abs + x], set flags)
56: bit abs,y (bitwise and accumulator with M[abs + y], set flags)

57: clz (clear zero flag)
58: sez (set zero flag)
59: clc (clear carry flag)
5A: sec (set carry flag)
5B: cln (clear negative flag)
5C: sen (set negative flag)
5D: clv (clear overflow flag)
5E: sev (set overflow flag)
5F: cli (clear interrupt enable)
60: sei (set interrupt enable)

61: rti (return from interrupt)
62: lsp #imm (16 bit) (load sp with immediate)
63: brk (break, interrupt)
64: lda (x,y) (load accumulator with M[(x << 8) | y])
