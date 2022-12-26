# generator for microcode to the D1 processor
from copy import deepcopy

HLT = 1 << 0
NF = 1 << 1
LD_I = 1 << 2
MW = 1 << 3
MR = 1 << 4
G0 = 1 << 5
G1 = 1 << 6
LD_MAR = 1 << 7
LD_SP = 1 << 8
DEC_SP = 1 << 9
INC_SP = 1 << 10
CLR_OF = 1 << 11
LD_OF = 1 << 12
INC_PC = 1 << 13
OE_PC = 1 << 14
LD_PC = 1 << 15
LD_OUT = 1 << 16
OE_B = 1 << 17
LD_B = 1 << 18
FI = 1 << 19
SU = 1 << 20
OE_ALU = 1 << 21
OE_A = 1 << 22
LD_A = 1 << 23

MA_PC = 0
MA_SP = G0
MA_MAR = G1
MA_FF = G0 | G1

# 2b flags 8b opcode 4b state
# A single state has 24 control lines

# template_ucode[flags][opcode][state] = control lines
template_ucode = [ [ [ 0 for i in range(16) ] for j in range(256) ] for k in range(4) ]
ucode = deepcopy(template_ucode)

Q0 = MA_FF | MR | LD_PC
FETCH = MA_PC | MR | LD_I | INC_PC | CLR_OF
NOP = 0

F_Z0 = 1 << 0
F_Z1 = 1 << 1
F_C0 = 1 << 2
F_C1 = 1 << 3

def instr(opcode, states):
    ucode[0b00][opcode] = states
    ucode[0b01][opcode] = states
    ucode[0b10][opcode] = states
    ucode[0b11][opcode] = states

def instr_flag(flags, opcode, states):
    for i in range(4):
        if (i >> 1) == 0 and (flags & F_Z0):
            ucode[i][opcode] = states
        elif (i >> 1) == 1 and (flags & F_Z1):
            ucode[i][opcode] = states
        elif (i & 0b01) == 0 and (flags & F_C0):
            ucode[i][opcode] = states
        elif (i & 0b01) == 1 and (flags & F_C1):
            ucode[i][opcode] = states


# 0x00 - NOP
instr(0x00, [Q0, FETCH, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x01 - LDA #$imm
instr(0x01, [Q0, FETCH, MA_PC | MR | LD_A | INC_PC, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x02 - LDA $addr
instr(0x02, [Q0, FETCH, MA_PC | MR | LD_MAR | INC_PC, MA_MAR | MR | LD_A, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x03 - LDA %sp, $off
instr(0x03, [Q0, FETCH, MA_PC | MR | LD_OF | INC_PC, MA_SP | MR | LD_A, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x04 - LDA $base, $off
instr(0x04, [Q0, FETCH, MA_PC | MR | LD_MAR | INC_PC, MA_PC | MR | LD_OF | INC_PC, MA_MAR | MR | LD_A, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x05 - STA $addr
instr(0x05, [Q0, FETCH, MA_PC | MR | LD_MAR | INC_PC, MA_MAR | OE_A | MW, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x06 - STA %sp, $off
instr(0x06, [Q0, FETCH, MA_PC | MR | LD_OF | INC_PC, MA_SP | OE_A | MW, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x07 - STA $base, $off
instr(0x07, [Q0, FETCH, MA_PC | MR | LD_MAR | INC_PC, MA_PC | MR | LD_OF | INC_PC, MA_MAR | OE_A | MW, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x08 - LDB $imm
instr(0x08, [Q0, FETCH, MA_PC | MR | LD_B | INC_PC, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x09 - LDB $addr
instr(0x09, [Q0, FETCH, MA_PC | MR | LD_MAR | INC_PC, MA_MAR | MR | LD_B, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x0A - LDB %sp, $off
instr(0x0A, [Q0, FETCH, MA_PC | MR | LD_OF | INC_PC, MA_SP | MR | LD_B, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x0B - LDB $base, $off
instr(0x0B, [Q0, FETCH, MA_PC | MR | LD_MAR | INC_PC, MA_PC | MR | LD_OF | INC_PC, MA_MAR | MR | LD_B, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x0C - STB $addr
instr(0x0C, [Q0, FETCH, MA_PC | MR | LD_MAR | INC_PC, MA_MAR | OE_B | MW, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x0D - STB %sp, $off
instr(0x0D, [Q0, FETCH, MA_PC | MR | LD_OF | INC_PC, MA_SP | OE_B | MW, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x0E - STB $base, $off
instr(0x0E, [Q0, FETCH, MA_PC | MR | LD_MAR | INC_PC, MA_PC | MR | LD_OF | INC_PC, MA_MAR | OE_B | MW, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x0F - LDSP $imm
instr(0x0F, [Q0, FETCH, MA_PC | MR | LD_SP | INC_PC, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x10 - ADD %A (A + B -> A)
instr(0x10, [Q0, FETCH, OE_ALU | LD_A | FI, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x11 - ADD %B (A + B -> B)
instr(0x11, [Q0, FETCH, OE_ALU | LD_B | FI, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x12 - ADD $addr
instr(0x12, [Q0, FETCH, MA_PC | MR | LD_MAR | INC_PC, MA_MAR | OE_ALU | MW | FI, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF]) 

# 0x13 - SUB %A (A - B -> A)
instr(0x13, [Q0, FETCH, OE_ALU | LD_A | SU | FI, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x14 - SUB %B (A - B -> B)
instr(0x14, [Q0, FETCH, OE_ALU | LD_B | SU | FI, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x15 - SUB $addr
instr(0x15, [Q0, FETCH, MA_PC | MR | LD_MAR | INC_PC, MA_MAR | OE_ALU | MW | SU | FI, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x16 - PUSH %A
instr(0x16, [Q0, FETCH, DEC_SP, MA_SP | OE_A | MW, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x17 - PUSH %B
instr(0x17, [Q0, FETCH, DEC_SP, MA_SP | OE_B | MW, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x18 - PUSH %pc
instr(0x18, [Q0, FETCH, DEC_SP, MA_SP | OE_PC | MW, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x19 - POP %A
instr(0x19, [Q0, FETCH, MA_SP | MR | LD_A, INC_SP, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x1A - POP %B
instr(0x1A, [Q0, FETCH, MA_SP | MR | LD_B, INC_SP, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x1B - POP %pc
instr(0x1B, [Q0, FETCH, MA_SP | MR | LD_PC, INC_SP, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x1C - JMP $addr
instr(0x1C, [Q0, FETCH, MA_PC | MR | LD_PC, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x1D - JEZ $addr
instr(0x1D, [Q0, FETCH, INC_PC | NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
instr_flag(F_Z1, 0x1D, [Q0, FETCH, MA_PC | MR | LD_PC, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x1E - JNZ $addr
instr(0x1E, [Q0, FETCH, INC_PC | NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
instr_flag(F_Z0, 0x1E, [Q0, FETCH, MA_PC | MR | LD_PC, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x1F - JCC $addr
instr(0x1F, [Q0, FETCH, INC_PC | NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
instr_flag(F_C0, 0x1F, [Q0, FETCH, MA_PC | MR | LD_PC, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x20 - JCS $addr
instr(0x20, [Q0, FETCH, INC_PC | NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
instr_flag(F_C1, 0x20, [Q0, FETCH, MA_PC | MR | LD_PC, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])

# 0x21 - STST (sub test)
instr(0x21, [Q0, FETCH, SU | FI, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])
# 0x22 - ATST (add test)
instr(0x22, [Q0, FETCH, FI, NF, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NOP, NF])


all_bytes = [0] * 2**14 * 4
for flag in range(4):
    for opcode in range(256):
        for state in range(16):
            address = (flag << 12) | (opcode << 4) | state
            all_bytes[address * 3] = ucode[flag][opcode][state] & 0xFF
            all_bytes[address * 3 + 1] = (ucode[flag][opcode][state] >> 8) & 0xFF
            all_bytes[address * 3 + 2] = (ucode[flag][opcode][state] >> 16) & 0xFF

with open("microcode.bin", "wb") as f:
    f.write(bytes(all_bytes))
