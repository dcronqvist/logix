# 6 * 8 bits control word

# All different control word bits
from copy import deepcopy


LD_A = 1 << 42
LD_I = 1 << 41
OE_A = 1 << 40
LD_T = 1 << 39
CLR_T = 1 << 38
LD_R = 1 << 37
OE_R = 1 << 36
LD_CC = 1 << 35
OE_CC = 1 << 34
LD_X = 1 << 33
OE_X = 1 << 32
LD_Y = 1 << 31
OE_Y = 1 << 30
LD_PC = 1 << 29
OE_PC = 1 << 28
INC_PC = 1 << 27
LD_SP = 1 << 26
OE_SP = 1 << 25
INC_SP = 1 << 24
DEC_SP = 1 << 23
LD_TA = 1 << 22
MR = 1 << 21
MW = 1 << 20
F3 = 1 << 19
F2 = 1 << 18
F1 = 1 << 17
F0 = 1 << 16
G14 = 1 << 15
G13 = 1 << 14
G12 = 1 << 13
G11 = 1 << 12
G10 = 1 << 11
G9 = 1 << 10
G8 = 1 << 9
G7 = 1 << 8
G6 = 1 << 7
G5 = 1 << 6
G4 = 1 << 5
G3 = 1 << 4
G2 = 1 << 3
G1 = 1 << 2
G0 = 1 << 1
NF = 1 << 0

C_ALU = 0
C_BUS = G2
C_ZERO = G3
C_REG = G3 | G2

V_ALU = 0
V_BUS = G4
V_ZERO = G5
V_REG = G5 | G4

Z_ALU = 0
Z_BUS = G6
Z_ZERO = G7
Z_REG = G7 | G6

N_ALU = 0
N_BUS = G8
N_ZERO = G9
N_REG = G9 | G8

I_REG = G11 | G10
I_BUS = G10
I_ONE = G11

ALU_CIN_ZERO = 0
ALU_CIN_ONE = G0
ALU_CIN_REG = G1
ALU_CIN_NEGREG = G1 | G0

MA_PC = 0
MA_SP_T = G12
MA_Y_T = G13
MA_X_T = G13 | G12
MA_TA = G14

CC_ALU = LD_CC | I_REG | N_ALU | Z_ALU | V_ALU | C_ALU

ALU_F_ZERO = 0
ALU_F_FD = F0
ALU_F_FE = F1
ALU_F_FF = F1 | F0
ALU_F_E = F2
ALU_F_D1K_PLUS_CIN = F2 | F0
ALU_F_D_OR_E = F2 | F1
ALU_F_D_AND_E = F2 | F1 | F0
ALU_F_D_XOR_E = F3
ALU_F_D_PLUS_CIN = F3 | F0
ALU_F_D_PLUS_FF = F3 | F1
ALU_F_D_PLUS_E_CIN = F3 | F1 | F0
ALU_F_D_PLUS_E1K_CIN = F3 | F2
ALU_F_D_LS_CIN = F3 | F2 | F0
ALU_F_D_RS_CIN = F3 | F2 | F1
ALU_F_D_RS_D7 = F3 | F2 | F1 | F0

EMPTY = 0b000000000000000000000000000000000000000000000000

Q0 = F1 | F0 | LD_R
Q1 = OE_R | LD_TA
Q2 = MR | G14 | LD_PC
FETCH = MR | LD_I | INC_PC | CLR_T

# ucode[flags(5)][opcode(8)][state(4))
# filled with empty Q0, Q1, FETCH, EMPTY... for now
template_opcode_states = [[Q0, Q1, Q2, FETCH, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF] for i in range(256)]
template_ucode = [deepcopy(template_opcode_states) for i in range(32)]

def set_instr(opcode, ucode):
    for i in range(32):
        template_ucode[i][opcode] = ucode

F_C0 = 1 << 0
F_C1 = 1 << 1
F_V0 = 1 << 2
F_V1 = 1 << 3
F_Z0 = 1 << 4
F_Z1 = 1 << 5
F_N0 = 1 << 6
F_N1 = 1 << 7
F_I0 = 1 << 8
F_I1 = 1 << 9

def set_flag_instr(flag, opcode, ucode):
    for j in range(32):
        if ((j >> 4) & 1) == 1 and flag & F_I1:
            template_ucode[j][opcode] = ucode
        elif ((j >> 4) & 1) == 0 and flag & F_I0:
            template_ucode[j][opcode] = ucode
        elif ((j >> 3) & 1) == 1 and flag & F_N1:
            template_ucode[j][opcode] = ucode
        elif ((j >> 3) & 1) == 0 and flag & F_N0:
            template_ucode[j][opcode] = ucode
        elif ((j >> 2) & 1) == 1 and flag & F_Z1:
            template_ucode[j][opcode] = ucode
        elif ((j >> 2) & 1) == 0 and flag & F_Z0:
            template_ucode[j][opcode] = ucode
        elif ((j >> 1) & 1) == 1 and flag & F_V1:
            template_ucode[j][opcode] = ucode
        elif ((j >> 1) & 1) == 0 and flag & F_V0:
            template_ucode[j][opcode] = ucode
        elif ((j >> 0) & 1) == 1 and flag & F_C1:
            template_ucode[j][opcode] = ucode
        elif ((j >> 0) & 1) == 0 and flag & F_C0:
            template_ucode[j][opcode] = ucode

NOP = [Q0, Q1, Q2, FETCH, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]

# 0x00 - NOP, inherent
set_instr(0x00, NOP)
# 0x01 - ANDCC, immediate
set_instr(0x01, [Q0, Q1, Q2, FETCH, MR | LD_T | INC_PC, CLR_T | OE_CC | ALU_F_D_AND_E | CC_ALU | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x02 - ORCC, immediate
set_instr(0x02, [Q0, Q1, Q2, FETCH, MR | LD_T | INC_PC, CLR_T | OE_CC | ALU_F_D_OR_E | CC_ALU | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])

# template_ucode[0x03] and template_ucode[0x04] should generate an exception

# 0x05 - CLRA, inherent
set_instr(0x05, [Q0, Q1, Q2, FETCH, LD_R, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY])
# 0x06 - NEGA, inherent
set_instr(0x06, [Q0, Q1, Q2, FETCH, OE_A | LD_R | ALU_F_D1K_PLUS_CIN | ALU_CIN_ONE | CC_ALU, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY])
# 0x07 - INCA, inherent
set_instr(0x07, [Q0, Q1, Q2, FETCH, OE_A | ALU_F_D_PLUS_CIN | LD_R | ALU_CIN_ONE | CC_ALU, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x08 - DECA, inherent
set_instr(0x08, [Q0, Q1, Q2, FETCH, OE_A | ALU_F_D_PLUS_FF | LD_R | ALU_CIN_ZERO | CC_ALU, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x09 - TSTA, inherent
set_instr(0x09, [Q0, Q1, Q2, FETCH, OE_A | ALU_F_D_PLUS_E1K_CIN | ALU_CIN_ONE | LD_R | LD_CC | NF | CC_ALU, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x0A - COMA, inherent
set_instr(0x0A, [Q0, Q1, Q2, FETCH, OE_A | ALU_F_D1K_PLUS_CIN | ALU_CIN_ZERO | LD_R | N_ALU | Z_ALU | V_ZERO | C_REG | LD_CC, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x0B - LSLA, inherent
set_instr(0x0B, [Q0, Q1, Q2, FETCH, OE_A | ALU_F_D_LS_CIN | ALU_CIN_ZERO | N_ALU | Z_ALU | V_ALU | C_ALU | LD_R | LD_CC, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x0C - LSRA, inherent
set_instr(0x0C, [Q0, Q1, Q2, FETCH, OE_A | ALU_F_D_RS_CIN | ALU_CIN_ZERO | N_ALU | Z_ALU | V_ALU | C_ALU | LD_R | LD_CC, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x0D - ROLA, inherent
set_instr(0x0D, [Q0, Q1, Q2, FETCH, OE_A | ALU_F_D_LS_CIN | ALU_CIN_REG | N_ALU | Z_ALU | V_ALU | C_ALU | LD_R | LD_CC, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x0E - RORA, inherent
set_instr(0x0E, [Q0, Q1, Q2, FETCH, OE_A | ALU_F_D_RS_CIN | ALU_CIN_REG | N_ALU | Z_ALU | V_ALU | C_ALU | LD_R | LD_CC, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x0F - ASRA, inherent
set_instr(0x0F, [Q0, Q1, Q2, FETCH, OE_A | ALU_F_D_RS_D7 | N_ALU | Z_ALU | V_ZERO | C_ALU | LD_R | LD_CC, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])

# 0x10 - PSHA, inherent
set_instr(0x10, [Q0, Q1, Q2, FETCH, DEC_SP, OE_A | MW | MA_SP_T | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x11 - PSHX, inherent
set_instr(0x11, [Q0, Q1, Q2, FETCH, DEC_SP, OE_X | MW | MA_SP_T | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x12 - PSHY, inherent
set_instr(0x12, [Q0, Q1, Q2, FETCH, DEC_SP, OE_Y | MW | MA_SP_T | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x13 - PSHC, inherent
set_instr(0x13, [Q0, Q1, Q2, FETCH, DEC_SP, OE_CC | MW | MA_SP_T | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x14 - PULA, inherent
set_instr(0x14, [Q0, Q1, Q2, FETCH, MA_SP_T | MR | LD_A, INC_SP | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x15 - PULX, inherent
set_instr(0x15, [Q0, Q1, Q2, FETCH, MA_SP_T | MR | LD_X, INC_SP | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x16 - PULY, inherent
set_instr(0x16, [Q0, Q1, Q2, FETCH, MA_SP_T | MR | LD_Y, INC_SP | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x17 - PULC, inherent
set_instr(0x17, [Q0, Q1, Q2, FETCH, MA_SP_T | MR | LD_CC | N_BUS | C_BUS | I_BUS | V_BUS | Z_BUS, INC_SP | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])

# 0x18 - TFR A,CC, inherent
set_instr(0x18, [Q0, Q1, Q2, FETCH, OE_A | LD_CC | N_BUS | V_BUS | Z_BUS | C_BUS | I_BUS | LD_CC | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x19 - TFR CC,A, inherent
set_instr(0x19, [Q0, Q1, Q2, FETCH, OE_CC | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x1A - TFR X,Y, inherent
set_instr(0x1A, [Q0, Q1, Q2, FETCH, OE_X | LD_Y | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x1B - TFR Y,X, inherent
set_instr(0x1B, [Q0, Q1, Q2, FETCH, OE_Y | LD_X | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x1C - TFR X,SP, inherent
set_instr(0x1C, [Q0, Q1, Q2, FETCH, OE_X | LD_SP | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x1D - TFR SP,X, inherent
set_instr(0x1D, [Q0, Q1, Q2, FETCH, OE_SP | LD_X | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x1E - TFR Y,SP, inherent
set_instr(0x1E, [Q0, Q1, Q2, FETCH, OE_Y | LD_SP | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x1F - TFR SP,Y, inherent
set_instr(0x1F, [Q0, Q1, Q2, FETCH, OE_SP | LD_Y | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])

# 0x20 - BSR, relative
set_instr(0x20, [Q0, Q1, Q2, FETCH, DEC_SP, MR | MA_PC | LD_T, OE_PC | ALU_F_D_PLUS_E_CIN | ALU_CIN_ZERO | LD_R | CLR_T | INC_PC, OE_PC | MW | MA_SP_T, OE_R | LD_PC | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x21 - BRA, relative
set_instr(0x21, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_T, LD_R | ALU_F_D_PLUS_E_CIN | ALU_CIN_ZERO | OE_PC | CLR_T, OE_R | LD_PC | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])

CONSUME_NEXT = [Q0, Q1, Q2, FETCH, INC_PC | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY]
JUMP = [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_T, OE_PC | ALU_F_D_PLUS_E_CIN | ALU_CIN_ZERO | LD_R, CLR_T | OE_R | LD_PC, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]

# 0x22 - BMI, relative
set_instr(0x22, CONSUME_NEXT)
set_flag_instr(F_N1, 0x22, JUMP)
# 0x23 - BPL, relative
set_instr(0x23, CONSUME_NEXT)
set_flag_instr(F_N0, 0x23, JUMP)
# 0x24 - BEQ, relative
set_instr(0x24, CONSUME_NEXT)
set_flag_instr(F_Z1, 0x24, JUMP)
# 0x25 - BNE, relative
set_instr(0x25, CONSUME_NEXT)
set_flag_instr(F_Z0, 0x25, JUMP)
# 0x26 - BVS, relative
set_instr(0x26, CONSUME_NEXT)
set_flag_instr(F_V1, 0x26, JUMP)
# 0x27 - BVC, relative
set_instr(0x27, CONSUME_NEXT)
set_flag_instr(F_V0, 0x27, JUMP)
# 0x28 - BCS, relative
set_instr(0x28, CONSUME_NEXT)
set_flag_instr(F_C1, 0x28, JUMP)
# 0x29 - BCC, relative
set_instr(0x29, CONSUME_NEXT)
set_flag_instr(F_C0, 0x29, JUMP)
# 0x2A - BHI, relative
set_instr(0x2A, CONSUME_NEXT)
set_flag_instr(F_C0 | F_Z0, 0x2A, JUMP)
# 0x2B - BLS, relative
set_instr(0x2B, CONSUME_NEXT)
set_flag_instr(F_C0 | F_Z1, 0x2B, JUMP)
set_flag_instr(F_C1 | F_Z0, 0x2B, JUMP)
set_flag_instr(F_C1 | F_Z1, 0x2B, JUMP)
# 0x2C - BGT, relative (N XOR V) OR Z = 0
set_instr(0x2C, CONSUME_NEXT)
set_flag_instr(F_N0 | F_V0 | F_Z0, 0x2C, JUMP)
set_flag_instr(F_N1 | F_V1 | F_Z0, 0x2C, JUMP)
# 0x2D - BGE, relative (N XOR V) = 0
set_instr(0x2D, CONSUME_NEXT)
set_flag_instr(F_N0 | F_V0, 0x2D, JUMP)
set_flag_instr(F_N1 | F_V1, 0x2D, JUMP)
# 0x2E - BLE, relative (N XOR V) OR Z = 1
set_instr(0x2E, CONSUME_NEXT)
set_flag_instr(F_N0 | F_V0 | F_Z1, 0x2E, JUMP)
set_flag_instr(F_N1 | F_V1 | F_Z1, 0x2E, JUMP)
set_flag_instr(F_N0 | F_V1 | F_Z1, 0x2E, JUMP)
set_flag_instr(F_N1 | F_V0 | F_Z1, 0x2E, JUMP)
set_flag_instr(F_N0 | F_V1 | F_Z0, 0x2E, JUMP)
set_flag_instr(F_N1 | F_V0 | F_Z0, 0x2E, JUMP)
# 0x2F - BLT, relative (N XOR V) = 1
set_instr(0x2F, CONSUME_NEXT)
set_flag_instr(F_N0 | F_V1, 0x2F, JUMP)
set_flag_instr(F_N1 | F_V0, 0x2F, JUMP)

# 0x30 - STX, absolute
set_instr(0x30, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, OE_X | MA_TA | MW, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x31 - STY, absolute
set_instr(0x31, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, OE_Y | MA_TA | MW, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x32 - STSP, absolute
set_instr(0x32, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, OE_SP | MA_TA | MW, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x33 - JMP, absolute
set_instr(0x33, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_PC, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x34 - JSR, absolute
set_instr(0x34, [Q0, Q1, Q2, FETCH, DEC_SP, MR | MA_PC | LD_T | INC_PC, LD_R | ALU_F_E | CLR_T, OE_PC | MA_SP_T | MW, OE_R | LD_PC, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x35 - CLR, absolute
set_instr(0x35, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC | ALU_F_ZERO | LD_R, OE_R | MA_TA | MW, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x36 - NEG, absolute
set_instr(0x36, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, MR | MA_TA | ALU_F_D1K_PLUS_CIN | ALU_CIN_ONE | LD_R, OE_R | MA_TA | MW, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x37 - INC, absolute
set_instr(0x37, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, MR | MA_TA | ALU_F_D_PLUS_CIN | ALU_CIN_ONE | LD_R, OE_R | MA_TA | MW, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x38 - DEC, absolute
set_instr(0x38, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, MR | MA_TA | ALU_F_D_PLUS_FF | LD_R, OE_R | MA_TA | MW, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x39 - TST, absolute
set_instr(0x39, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, MR | MA_TA | ALU_F_D_PLUS_E1K_CIN | ALU_CIN_ONE | LD_R, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x3A - COM, absolute
set_instr(0x3A, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, MR | MA_TA | ALU_F_D1K_PLUS_CIN | ALU_CIN_ZERO | LD_R, OE_R | MA_TA | MW, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x3B - LSL, absolute
set_instr(0x3B, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, MR | MA_TA | ALU_F_D_LS_CIN | ALU_CIN_ZERO | LD_R, OE_R | MA_TA | MW, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x3C - LSR, absolute
set_instr(0x3C, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, MR | MA_TA | ALU_F_D_RS_CIN | ALU_CIN_ZERO | LD_R, OE_R | MA_TA | MW, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x3D - ROL, absolute
set_instr(0x3D, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, MR | MA_TA | ALU_F_D_LS_CIN | ALU_CIN_REG | LD_R, OE_R | MA_TA | MW, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x3E - ROR, absolute
set_instr(0x3E, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, MR | MA_TA | ALU_F_D_RS_CIN | ALU_CIN_REG | LD_R, OE_R | MA_TA | MW, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x3F - ASR, absolute
set_instr(0x3F, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, MR | MA_TA | ALU_F_D_RS_D7 | LD_R, OE_R | MA_TA | MW, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])


# 0x43 - RTS, inherent
set_instr(0x43, [Q0, Q1, Q2, FETCH, MR | MA_SP_T | LD_PC | INC_SP | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])

# 0x90 - LDX, immediate
set_instr(0x90, [Q0, Q1, Q2, FETCH, MR | LD_X | INC_PC, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x91 - LDY, immediate
set_instr(0x91, [Q0, Q1, Q2, FETCH, MR | LD_Y | INC_PC, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0x92 - LDSP, immediate
set_instr(0x92, [Q0, Q1, Q2, FETCH, MR | LD_SP | INC_PC, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])

# 0x94 - SUBA, immediate
set_instr(0x94, [Q0, Q1, Q2, FETCH, MR | LD_T | INC_PC, CLR_T | OE_A | ALU_F_D_PLUS_E1K_CIN | ALU_CIN_ONE | LD_R, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])

# 0x96 - ADDA, immediate
set_instr(0x96, [Q0, Q1, Q2, FETCH, MR | LD_T | INC_PC, CLR_T | OE_A | ALU_F_D_PLUS_E_CIN | ALU_CIN_ZERO | LD_R, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])

# 0x92 - LDSP, immediate
set_instr(0x92, [Q0, Q1, Q2, FETCH, MR | LD_SP | INC_PC | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])

# 0xA0 - LDX, absolute
set_instr(0xA0, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, MR | MA_TA | LD_X | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0xA1 - LDY, absolute
set_instr(0xA1, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, MR | MA_TA | LD_Y | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0xA2 - LDSP, absolute
set_instr(0xA2, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, MR | MA_TA | LD_SP | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0xA4 - SUBA, absolute
set_instr(0xA4, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, MR | MA_TA | LD_T, CLR_T | OE_A | ALU_F_D_PLUS_E1K_CIN | ALU_CIN_ONE | LD_R, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0xA6 - ADDA, absolute
set_instr(0xA6, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, MR | MA_TA | LD_T, CLR_T | OE_A | ALU_F_D_PLUS_E_CIN | ALU_CIN_ZERO | LD_R, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])

# 0xE1 - STA, absolute
set_instr(0xE1, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, OE_A | MA_TA | MW, NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])

# 0xF0 - LDA, immediate
set_instr(0xF0, [Q0, Q1, Q2, FETCH, MR | LD_A | INC_PC | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])
# 0xF1 - LDA, absolute
set_instr(0xF1, [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_TA | INC_PC, MR | MA_TA | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF])

all_bytes = [0] * (2**17 * 8)

for address in range(2**17):
    flags = (address >> 12) & 0b11111
    opcode = (address >> 4) & 0b11111111
    state = address & 0b1111

    b = template_ucode[flags][opcode][state]

    b0_7 = b & 0b11111111
    b8_15 = (b >> 8) & 0b11111111
    b16_23 = (b >> 16) & 0b11111111
    b24_31 = (b >> 24) & 0b11111111
    b32_39 = (b >> 32) & 0b11111111
    b40_47 = (b >> 40) & 0b11111111
    
    all_bytes[address * 6 + 0] = b0_7
    all_bytes[address * 6 + 1] = b8_15
    all_bytes[address * 6 + 2] = b16_23
    all_bytes[address * 6 + 3] = b24_31
    all_bytes[address * 6 + 4] = b32_39
    all_bytes[address * 6 + 5] = b40_47


with open("ucode.bin", "wb") as f:
    f.write(bytes(all_bytes))
