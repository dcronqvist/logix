# ALU Functions
# 0x0 - Zero
# 0x1 - A+B+Cin
# 0x2 - A-B-Cin
# 0x3 - A*B+Cin
# 0x4 - A//B (integer division)
# 0x5 - A%B  (integer modulus)
# 0x6 - A&B
# 0x7 - A|B
# 0x8 - A^B
# 0x9 - ~A
# 0xA - A<<1
# 0xB - A>>1
# 0xC - FFFD
# 0xD - FFFE
# 0xE - 0001

# generator for microcode to the D1 processor
from copy import deepcopy

HLT = 1 << 0
NF = 1 << 1
OE_X = 1 << 2
LD_X = 1 << 3
OE_B = 1 << 4
LD_B = 1 << 5
ALU_F0 = 1 << 6
ALU_F1 = 1 << 7
ALU_F2 = 1 << 8
ALU_F3 = 1 << 9
OE_ALU_IC = 1 << 10
OE_ALU = 1 << 11
LD_ALU_IC = 1 << 12
ALU_CIN0 = 1 << 13
ALU_CIN1 = 1 << 14
LD_ALU_IF = 1 << 15
LD_ALU_F = 1 << 16
ALU_I_MAN = 1 << 17
OE_A = 1 << 18
LD_A = 1 << 19
OE_T = 1 << 20
LD_TL = 1 << 21
LD_TH = 1 << 22

LD_PC = 1 << 23
OE_PC = 1 << 24
INC_PC = 1 << 25
LD_OFR = 1 << 26
CLR_OFR = 1 << 27
INC_SP = 1 << 28
DEC_SP = 1 << 29
LD_SP = 1 << 30
OE_MAR = 1 << 31
MA_ADD1 = 1 << 32

LD_MAR = 1 << 33
M_G0 = 1 << 34
M_G1 = 1 << 35
MR = 1 << 36
MW = 1 << 37
M_HL = 1 << 38
LD_I = 1 << 39

MA_FFFF = M_G1 | M_G0
MA_PC = 0
MA_SP = M_G0
MA_MAR = M_G1

ALU_F_ZERO = 0
ALU_F_ADD = ALU_F0
ALU_F_SUB = ALU_F1
ALU_F_MUL = ALU_F0 | ALU_F1
ALU_F_DIV = ALU_F2
ALU_F_MOD = ALU_F2 | ALU_F0
ALU_F_AND = ALU_F2 | ALU_F1
ALU_F_OR = ALU_F2 | ALU_F1 | ALU_F0
ALU_F_XOR = ALU_F3
ALU_F_NOT_A = ALU_F3 | ALU_F0
ALU_F_SHL_A = ALU_F3 | ALU_F1
ALU_F_SHR_A = ALU_F3 | ALU_F1 | ALU_F0
ALU_F_FFFD = ALU_F3 | ALU_F2
ALU_F_FFFE = ALU_F3 | ALU_F2 | ALU_F0
ALU_F_0001 = ALU_F3 | ALU_F2 | ALU_F1

# 4b flags 16b opcode 4b state
# A single state has 40 control lines (5 bytes)
Q0 = MA_FFFF | MR | LD_PC
NEWFETCH = NF | CLR_OFR
FETCH = MA_PC | MR | LD_I | INC_PC
NOP = 0
NOP_STATES = [Q0, FETCH, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF]

# template_ucode[flags][opcode][state] = control lines
template_ucode = [ [ NOP_STATES for j in range(2**16) ] for k in range(16) ]
ucode = deepcopy(template_ucode)


# Flags CNZID
# Carry
# Negative
# Zero
# Interrupt
# Division by zero ERROR

F_C0 = 1 << 0
F_C1 = 1 << 1
F_N0 = 1 << 2
F_N1 = 1 << 3
F_Z0 = 1 << 4
F_Z1 = 1 << 5
F_E0 = 1 << 6
F_E1 = 1 << 7

def instr(opcode, states):
    for flag in range(16):
        ucode[flag][opcode] = states

def instr_flag(flags, opcode, states):
    for i in range(16):
        c_ = i & 0b1000
        n_ = i & 0b0100
        z_ = i & 0b0010

        if flags & F_C0 and c_ == 0:
            ucode[i][opcode] = states
        if flags & F_C1 and c_ != 0:
            ucode[i][opcode] = states
        if flags & F_N0 and n_ == 0:
            ucode[i][opcode] = states
        if flags & F_N1 and n_ != 0:
            ucode[i][opcode] = states
        if flags & F_Z0 and z_ == 0:
            ucode[i][opcode] = states
        if flags & F_Z1 and z_ != 0:
            ucode[i][opcode] = states
            


# 0x00: NOP
instr(0x00, NOP_STATES)

# Load and store (0x01 - 0x30)

# 0x01: LDA #$imm
instr(0x01, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_A, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x02: LDA $addr
instr(0x02, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, MR | MA_MAR | LD_TL, MR | MA_MAR | MA_ADD1 | LD_TH | M_HL, OE_T | LD_A, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF]) 
# 0x03: LDA %sp, #$off
instr(0x03, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_OFR, MA_SP | MR | LD_TL, MA_SP | MR | MA_ADD1 | LD_TH | M_HL, OE_T | LD_A, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x04: LDA $addr, #$off
instr(0x04, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, MR | MA_PC | LD_TL | INC_PC, MR | MA_PC | LD_TH | INC_PC, OE_T | LD_OFR, MA_MAR | MR | LD_TL, MA_MAR | MR | MA_ADD1 | LD_TH | M_HL, OE_T | LD_A, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x05: STA $addr
instr(0x05, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, MW | MA_MAR | OE_A, MW | MA_MAR | MA_ADD1 | OE_A | M_HL, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x06: STA %sp, #$off
instr(0x06, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_OFR, MW | MA_SP | OE_A, MW | MA_SP | MA_ADD1 | OE_A | M_HL, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x07: STA $addr, #$off
instr(0x07, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, MR | MA_PC | LD_TL | INC_PC, MR | MA_PC | LD_TH | INC_PC, OE_T | LD_OFR, MW | MA_MAR | OE_A, MW | MA_MAR | MA_ADD1 | OE_A | M_HL, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x08: LDB #$imm
instr(0x08, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_B, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x09: LDB $addr
instr(0x09, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, MR | MA_MAR | LD_TL, MR | MA_MAR | MA_ADD1 | LD_TH | M_HL, OE_T | LD_B, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x0A: LDB %sp, #$off
instr(0x0A, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_OFR, MA_SP | MR | LD_TL, MA_SP | MR | MA_ADD1 | LD_TH | M_HL, OE_T | LD_B, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x0B: LDB $addr, #$off
instr(0x0B, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, MR | MA_PC | LD_TL | INC_PC, MR | MA_PC | LD_TH | INC_PC, OE_T | LD_OFR, MA_MAR | MR | LD_TL, MA_MAR | MR | MA_ADD1 | LD_TH | M_HL, OE_T | LD_B, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x0C: STB $addr
instr(0x0C, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, MW | MA_MAR | OE_B, MW | MA_MAR | MA_ADD1 | OE_B | M_HL, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x0D: STB %sp, #$off
instr(0x0D, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_OFR, MW | MA_SP | OE_B, MW | MA_SP | MA_ADD1 | OE_B | M_HL, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x0E: STB $addr, #$off
instr(0x0E, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, MR | MA_PC | LD_TL | INC_PC, MR | MA_PC | LD_TH | INC_PC, OE_T | LD_OFR, MW | MA_MAR | OE_B, MW | MA_MAR | MA_ADD1 | OE_B | M_HL, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x0F: LDX #$imm
instr(0x0F, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_X, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x10: LDX $addr
instr(0x10, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, MR | MA_MAR | LD_TL, MR | MA_MAR | MA_ADD1 | LD_TH | M_HL, OE_T | LD_X, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x11: LDX %sp, #$off
instr(0x11, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_OFR, MA_SP | MR | LD_TL, MA_SP | MR | MA_ADD1 | LD_TH | M_HL, OE_T | LD_X, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x12: LDX $addr, #$off
instr(0x12, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, MR | MA_PC | LD_TL | INC_PC, MR | MA_PC | LD_TH | INC_PC, OE_T | LD_OFR, MA_MAR | MR | LD_TL, MA_MAR | MR | MA_ADD1 | LD_TH | M_HL, OE_T | LD_X, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x13: STX $addr
instr(0x13, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, MW | MA_MAR | OE_X, MW | MA_MAR | MA_ADD1 | OE_X | M_HL, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x14: STX %sp, #$off
instr(0x14, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_OFR, MW | MA_SP | OE_X, MW | MA_SP | MA_ADD1 | OE_X | M_HL, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x15: STX $addr, #$off
instr(0x15, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, MR | MA_PC | LD_TL | INC_PC, MR | MA_PC | LD_TH | INC_PC, OE_T | LD_OFR, MW | MA_MAR | OE_X, MW | MA_MAR | MA_ADD1 | OE_X | M_HL, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x16: LDSP #$imm
instr(0x16, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_SP, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# Register operations (0x30 - 0x40)

# 0x30: MOV %A, %B
instr(0x30, [Q0, FETCH, OE_A | LD_B, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x31: MOV %A, %X
instr(0x31, [Q0, FETCH, OE_A | LD_X, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x32: MOV %B, %A
instr(0x32, [Q0, FETCH, OE_B | LD_A, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x33: MOV %B, %X
instr(0x33, [Q0, FETCH, OE_B | LD_X, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x34: MOV %X, %A
instr(0x34, [Q0, FETCH, OE_X | LD_A, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x35: MOV %X, %B
instr(0x35, [Q0, FETCH, OE_X | LD_B, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# Arithmetic and logic (0x40 - 0x70)

# 0x40: ADD %A (A = A + B)
instr(0x40, [Q0, FETCH, ALU_F_ADD | LD_ALU_F | OE_ALU | LD_A, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x41: ADD %B (B = A + B)
instr(0x41, [Q0, FETCH, ALU_F_ADD | LD_ALU_F | OE_ALU | LD_B, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x42: ADD %X (X = A + B)
instr(0x42, [Q0, FETCH, ALU_F_ADD | LD_ALU_F | OE_ALU | LD_X, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x43: ADD $addr ($addr = A + B)
instr(0x43, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, ALU_F_ADD | LD_ALU_F | OE_ALU | LD_TL | LD_TH, MW | MA_MAR | OE_T, MW | MA_MAR | MA_ADD1 | OE_T | M_HL, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x44: SUB %A (A = A - B)
instr(0x44, [Q0, FETCH, ALU_F_SUB | LD_ALU_F | OE_ALU | LD_A, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x45: SUB %B (B = A - B)
instr(0x45, [Q0, FETCH, ALU_F_SUB | LD_ALU_F | OE_ALU | LD_B, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x46: SUB %X (X = A - B)
instr(0x46, [Q0, FETCH, ALU_F_SUB | LD_ALU_F | OE_ALU | LD_X, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x47: SUB $addr ($addr = A - B)
instr(0x47, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, ALU_F_SUB | LD_ALU_F | OE_ALU | LD_TL | LD_TH, MW | MA_MAR | OE_T, MW | MA_MAR | MA_ADD1 | OE_T | M_HL, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x48: MUL %A (A = A * B)
instr(0x48, [Q0, FETCH, ALU_F_MUL | LD_ALU_F | OE_ALU | LD_A, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x49: MUL %B (B = A * B)
instr(0x49, [Q0, FETCH, ALU_F_MUL | LD_ALU_F | OE_ALU | LD_B, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x4A: MUL %X (X = A * B)
instr(0x4A, [Q0, FETCH, ALU_F_MUL | LD_ALU_F | OE_ALU | LD_X, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x4B: MUL $addr ($addr = A * B)
instr(0x4B, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, ALU_F_MUL | LD_ALU_F | OE_ALU | LD_TL | LD_TH, MW | MA_MAR | OE_T, MW | MA_MAR | MA_ADD1 | OE_T | M_HL, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x4C: DIV %A (A = A / B)
instr(0x4C, [Q0, FETCH, ALU_F_DIV | LD_ALU_F | OE_ALU | LD_A, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x4D: DIV %B (B = A / B)
instr(0x4D, [Q0, FETCH, ALU_F_DIV | LD_ALU_F | OE_ALU | LD_B, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x4E: DIV %X (X = A / B)
instr(0x4E, [Q0, FETCH, ALU_F_DIV | LD_ALU_F | OE_ALU | LD_X, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x4F: DIV $addr ($addr = A / B)
instr(0x4F, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, ALU_F_DIV | LD_ALU_F | OE_ALU | LD_TL | LD_TH, MW | MA_MAR | OE_T, MW | MA_MAR | MA_ADD1 | OE_T | M_HL, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x50: MOD %A (A = A % B)
instr(0x50, [Q0, FETCH, ALU_F_MOD | LD_ALU_F | OE_ALU | LD_A, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x51: MOD %B (B = A % B)
instr(0x51, [Q0, FETCH, ALU_F_MOD | LD_ALU_F | OE_ALU | LD_B, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x52: MOD %X (X = A % B)
instr(0x52, [Q0, FETCH, ALU_F_MOD | LD_ALU_F | OE_ALU | LD_X, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x53: MOD $addr ($addr = A % B)
instr(0x53, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, ALU_F_MOD | LD_ALU_F | OE_ALU | LD_TL | LD_TH, MW | MA_MAR | OE_T, MW | MA_MAR | MA_ADD1 | OE_T | M_HL, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x54: AND %A (A = A & B)
instr(0x54, [Q0, FETCH, ALU_F_AND | LD_ALU_F | OE_ALU | LD_A, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x55: AND %B (B = A & B)
instr(0x55, [Q0, FETCH, ALU_F_AND | LD_ALU_F | OE_ALU | LD_B, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x56: AND %X (X = A & B)
instr(0x56, [Q0, FETCH, ALU_F_AND | LD_ALU_F | OE_ALU | LD_X, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x57: AND $addr ($addr = A & B)
instr(0x57, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, ALU_F_AND | LD_ALU_F | OE_ALU | LD_TL | LD_TH, MW | MA_MAR | OE_T, MW | MA_MAR | MA_ADD1 | OE_T | M_HL, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x58: OR %A (A = A | B)
instr(0x58, [Q0, FETCH, ALU_F_OR | LD_ALU_F | OE_ALU | LD_A, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x59: OR %B (B = A | B)
instr(0x59, [Q0, FETCH, ALU_F_OR | LD_ALU_F | OE_ALU | LD_B, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x5A: OR %X (X = A | B)
instr(0x5A, [Q0, FETCH, ALU_F_OR | LD_ALU_F | OE_ALU | LD_X, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x5B: OR $addr ($addr = A | B)
instr(0x5B, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, ALU_F_OR | LD_ALU_F | OE_ALU | LD_TL | LD_TH, MW | MA_MAR | OE_T, MW | MA_MAR | MA_ADD1 | OE_T | M_HL, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x5C: XOR %A (A = A ^ B)
instr(0x5C, [Q0, FETCH, ALU_F_XOR | LD_ALU_F | OE_ALU | LD_A, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x5D: XOR %B (B = A ^ B)
instr(0x5D, [Q0, FETCH, ALU_F_XOR | LD_ALU_F | OE_ALU | LD_B, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x5E: XOR %X (X = A ^ B)
instr(0x5E, [Q0, FETCH, ALU_F_XOR | LD_ALU_F | OE_ALU | LD_X, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x5F: XOR $addr ($addr = A ^ B)
instr(0x5F, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_MAR, ALU_F_XOR | LD_ALU_F | OE_ALU | LD_TL | LD_TH, MW | MA_MAR | OE_T, MW | MA_MAR | MA_ADD1 | OE_T | M_HL, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x60: NOT %A (A = ~A)
instr(0x60, [Q0, FETCH, ALU_F_NOT_A | LD_ALU_F | OE_ALU | LD_A, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x61: LSH %A (A = A << 1)
instr(0x61, [Q0, FETCH, ALU_F_SHL_A | LD_ALU_F | OE_ALU | LD_A, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x62: RSH %A (A = A >> 1)
instr(0x62, [Q0, FETCH, ALU_F_SHR_A | LD_ALU_F | OE_ALU | LD_A, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# Stack operations (0x70 - 0x90)

# 0x70: PUSH %A
instr(0x70, [Q0, FETCH, DEC_SP, MW | OE_A | MA_SP | M_HL, DEC_SP, MW | OE_A | MA_SP, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x71: PUSH %B
instr(0x71, [Q0, FETCH, DEC_SP, MW | OE_B | MA_SP | M_HL, DEC_SP, MW | OE_B | MA_SP, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x72: PUSH %X
instr(0x72, [Q0, FETCH, DEC_SP, MW | OE_X | MA_SP | M_HL, DEC_SP, MW | OE_X | MA_SP, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x73: PUSH %PC
instr(0x73, [Q0, FETCH, DEC_SP, MW | OE_PC | MA_SP | M_HL, DEC_SP, MW | OE_PC | MA_SP, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x74: PUSH $addr
instr(0x74, [Q0, FETCH, MR | LD_TL | MA_PC | INC_PC, MR | LD_TH | MA_PC | INC_PC, OE_T | LD_MAR, MR | MA_MAR | LD_TL, MR | MA_MAR | MA_ADD1 | LD_TH | M_HL, DEC_SP, MW | MA_SP | M_HL | OE_T | DEC_SP, MW | MA_SP | OE_T, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x75: POP %A
instr(0x75, [Q0, FETCH, MA_SP | MR | LD_TL | INC_SP, MA_SP | MR | LD_TH | INC_SP | M_HL, OE_T | LD_A, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x76: POP %B
instr(0x76, [Q0, FETCH, MA_SP | MR | LD_TL | INC_SP, MA_SP | MR | LD_TH | INC_SP | M_HL, OE_T | LD_B, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x77: POP %X
instr(0x77, [Q0, FETCH, MA_SP | MR | LD_TL | INC_SP, MA_SP | MR | LD_TH | INC_SP | M_HL, OE_T | LD_X, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x78: POP %PC
instr(0x78, [Q0, FETCH, MA_SP | MR | LD_TL | INC_SP, MA_SP | MR | LD_TH | INC_SP | M_HL, OE_T | LD_PC, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x79: POP $addr
instr(0x79, [Q0, FETCH, MR | LD_TL | MA_PC | INC_PC, MR | LD_TH | MA_PC | INC_PC | M_HL, OE_T | LD_MAR, MR | MA_SP | LD_TL | INC_SP, MR | MA_SP | LD_TH | INC_SP | M_HL, MW | MA_MAR | OE_T, MW | MA_MAR | MA_ADD1 | OE_T | M_HL, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# Control flow (0x90 - 0xA0)

# 0x90: JMP $addr (unconditional jump)
instr(0x90, [Q0, FETCH, MR | LD_TL | MA_PC | INC_PC, MR | LD_TH | MA_PC | INC_PC | M_HL, OE_T | LD_PC, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x91: JZS $addr (jump if zero set)
instr(0x91, [Q0, FETCH, INC_PC, INC_PC, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NEWFETCH])
instr_flag(F_Z1, 0x91, [Q0, FETCH, MR | LD_TL | MA_PC | INC_PC, MR | LD_TH | MA_PC | INC_PC | M_HL, OE_T | LD_PC, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x92: JZC $addr (jump if zero clear)
instr(0x92, [Q0, FETCH, INC_PC, INC_PC, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NEWFETCH])
instr_flag(F_Z0, 0x92, [Q0, FETCH, MR | LD_TL | MA_PC | INC_PC, MR | LD_TH | MA_PC | INC_PC | M_HL, OE_T | LD_PC, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x93: JCS $addr (jump if carry set)
instr(0x93, [Q0, FETCH, INC_PC, INC_PC, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NEWFETCH])
instr_flag(F_C1, 0x93, [Q0, FETCH, MR | LD_TL | MA_PC | INC_PC, MR | LD_TH | MA_PC | INC_PC | M_HL, OE_T | LD_PC, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x94: JCC $addr (jump if carry clear)
instr(0x94, [Q0, FETCH, INC_PC, INC_PC, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NEWFETCH])
instr_flag(F_C0, 0x94, [Q0, FETCH, MR | LD_TL | MA_PC | INC_PC, MR | LD_TH | MA_PC | INC_PC | M_HL, OE_T | LD_PC, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x95: JNS $addr (jump if negative set)
instr(0x95, [Q0, FETCH, INC_PC, INC_PC, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NEWFETCH])
instr_flag(F_N1, 0x95, [Q0, FETCH, MR | LD_TL | MA_PC | INC_PC, MR | LD_TH | MA_PC | INC_PC | M_HL, OE_T | LD_PC, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x96: JNC $addr (jump if negative clear)
instr(0x96, [Q0, FETCH, INC_PC, INC_PC, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NEWFETCH])
instr_flag(F_N0, 0x96, [Q0, FETCH, MR | LD_TL | MA_PC | INC_PC, MR | LD_TH | MA_PC | INC_PC | M_HL, OE_T | LD_PC, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# Interrupts (0xA0 - 0xAF)

# 0xA1: SINT #$imm
instr(0xA1, [Q0, FETCH, MR | MA_PC | LD_TL | INC_PC, M_HL | MR | MA_PC | LD_TH | INC_PC, OE_T | LD_ALU_F | LD_ALU_IC | LD_ALU_IF | ALU_I_MAN, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0xA2: RINT 
instr(0xA2, [Q0, FETCH, INC_SP, INC_SP, MR | MA_SP | LD_TL | INC_SP, M_HL | MR | MA_SP | LD_TH | INC_SP, OE_T | LD_PC, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# Miscellaneous (0xF0 - 0xFF)

# 0xF0: HALT
instr(0xF0, [Q0, FETCH, HLT, NEWFETCH, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])


all_bytes = [0] * 2**(16+3)
for flag in range(16):
    for opcode in range(256):
        for state in range(16):
            address = (flag << 12) | (opcode << 4) | state
            all_bytes[address * 5] = ucode[flag][opcode][state] & 0xFF
            all_bytes[address * 5 + 1] = (ucode[flag][opcode][state] >> 8) & 0xFF
            all_bytes[address * 5 + 2] = (ucode[flag][opcode][state] >> 16) & 0xFF
            all_bytes[address * 5 + 3] = (ucode[flag][opcode][state] >> 24) & 0xFF
            all_bytes[address * 5 + 4] = (ucode[flag][opcode][state] >> 32) & 0xFF

with open("microcode.bin", "wb") as f:
    f.write(bytes(all_bytes))


# interrupt handling ROM (4 address bits for 16 states, 5 bytes per state)
# iucode[state] = control lines

iucode = [   
                NOP,
                DEC_SP | CLR_OFR, # Decrement stack pointer
                OE_PC | MA_SP | MW | M_HL  | DEC_SP, # write high byte of PC to stack
                OE_PC | MA_SP | MW | DEC_SP, # write low byte of PC to stack
                ALU_F_FFFD | OE_ALU | LD_MAR, # load low byte of interrupt vector into MAR
                MR | LD_TL | MA_MAR, # read low byte of interrupt vector into T
                ALU_F_FFFE | OE_ALU | LD_MAR, # load high byte of interrupt vector into MAR
                MR | LD_TH | M_HL | MA_MAR, # read high byte of interrupt vector into T
                OE_T | LD_PC, # put interrupt vector into PC
                OE_ALU | OE_ALU_IC | MA_SP | MW | M_HL | DEC_SP, # write high byte of error code to stack
                OE_ALU | OE_ALU_IC | MA_SP | MW, # write low byte of error code to stack
                LD_ALU_F | ALU_I_MAN,
                NEWFETCH,
                NEWFETCH,
                NEWFETCH,
                NEWFETCH
            ]

iall_bytes = [0] * 2**(4+4)
for state in range(16):
    address = state
    iall_bytes[address * 5] = iucode[state] & 0xFF
    iall_bytes[address * 5 + 1] = (iucode[state] >> 8) & 0xFF
    iall_bytes[address * 5 + 2] = (iucode[state] >> 16) & 0xFF
    iall_bytes[address * 5 + 3] = (iucode[state] >> 24) & 0xFF
    iall_bytes[address * 5 + 4] = (iucode[state] >> 32) & 0xFF

with open("interrupt.bin", "wb") as f:
    f.write(bytes(iall_bytes))
