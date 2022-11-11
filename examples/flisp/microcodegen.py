# 6 * 8 bits control word

# All different control word bits
LD_A = 1 << 47
LD_I = 1 << 46
OE_A = 1 << 45
LD_T = 1 << 44
CLR_T = 1 << 43
LD_R = 1 << 42
OE_R = 1 << 41
LD_CC = 1 << 40
OE_CC = 1 << 39
LD_X = 1 << 38
OE_X = 1 << 37
LD_Y = 1 << 36
OE_Y = 1 << 35
LD_PC = 1 << 34
OE_PC = 1 << 33
INC_PC = 1 << 32
LD_SP = 1 << 31
OE_SP = 1 << 30
INC_SP = 1 << 29
DEC_SP = 1 << 28
LD_TA = 1 << 27
MR = 1 << 26
MW = 1 << 25
F3 = 1 << 24
F2 = 1 << 23
F1 = 1 << 22
F0 = 1 << 21
G14 = 1 << 20
G13 = 1 << 19
G12 = 1 << 18
G11 = 1 << 17
G10 = 1 << 16
G9 = 1 << 15
G8 = 1 << 14
G7 = 1 << 13
G6 = 1 << 12
G5 = 1 << 11
G4 = 1 << 10
G3 = 1 << 9
G2 = 1 << 8
G1 = 1 << 7
G0 = 1 << 6
NF = 1 << 5

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

# template_ucode[256][16], 256 opcodes, 16 states
template_ucode = [[Q0, Q1, Q2, FETCH, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF] for j in range(256)]

template_ucode[0x00][4] = NF # 0x00 - NOP, immediate new fetch
# 0x01 - ANDCC, immediate
template_ucode[0x01] = [Q0, Q1, Q2, FETCH, MR | LD_T | INC_PC, OE_CC | ALU_F_D_AND_E | LD_CC | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x02 - ORCC, immediate
template_ucode[0x01] = [Q0, Q1, Q2, FETCH, MR | LD_T | INC_PC, OE_CC | ALU_F_D_OR_E | LD_CC | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]

# template_ucode[0x03] and template_ucode[0x04] should generate an exception

# 0x05 - CLRA, inherent
template_ucode[0x05] = [Q0, Q1, Q2, FETCH, LD_R, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY]  
# 0x06 - NEGA, inherent
template_ucode[0x06] = [Q0, Q1, Q2, FETCH, OE_A | LD_R | ALU_F_D1K_PLUS_CIN | ALU_CIN_ONE | N_ALU | Z_ALU | V_ALU | C_ALU, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY]
# 0x07 - INCA, inherent
template_ucode[0x07] = [Q0, Q1, Q2, FETCH, OE_A | ALU_F_D_PLUS_CIN | LD_R | ALU_CIN_ONE, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x08 - DECA, inherent
template_ucode[0x08] = [Q0, Q1, Q2, FETCH, OE_A | ALU_F_D_PLUS_FF | LD_R | ALU_CIN_ZERO, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x09 - TSTA, inherent
template_ucode[0x09] = [Q0, Q1, Q2, FETCH, OE_A | ALU_F_D_PLUS_E1K_CIN | ALU_CIN_ONE | LD_R | LD_CC | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x0A - COMA, inherent
template_ucode[0x0A] = [Q0, Q1, Q2, FETCH, OE_A | ALU_F_D1K_PLUS_CIN | ALU_CIN_ZERO | LD_R | N_ALU | Z_ALU | V_ZERO | C_REG | LD_CC, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x0B - LSLA, inherent
template_ucode[0x0B] = [Q0, Q1, Q2, FETCH, OE_A | ALU_F_D_LS_CIN | ALU_CIN_ZERO | N_ALU | Z_ALU | V_ALU | C_ALU | LD_R | LD_CC, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x0C - LSRA, inherent
template_ucode[0x0C] = [Q0, Q1, Q2, FETCH, OE_A | ALU_F_D_RS_CIN | ALU_CIN_ZERO | N_ALU | Z_ALU | V_ALU | C_ALU | LD_R | LD_CC, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x0D - ROLA, inherent
template_ucode[0x0C] = [Q0, Q1, Q2, FETCH, OE_A | ALU_F_D_LS_CIN | ALU_CIN_REG | N_ALU | Z_ALU | V_ALU | C_ALU | LD_R | LD_CC, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x0E - RORA, inherent
template_ucode[0x0D] = [Q0, Q1, Q2, FETCH, OE_A | ALU_F_D_RS_CIN | ALU_CIN_REG | N_ALU | Z_ALU | V_ALU | C_ALU | LD_R | LD_CC, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x0F - ASRA, inherent
template_ucode[0x0F] = [Q0, Q1, Q2, FETCH, OE_A | ALU_F_D_RS_D7 | N_ALU | Z_ALU | V_ZERO | C_ALU | LD_R | LD_CC, OE_R | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]

# 0x10 - PSHA, inherent
template_ucode[0x10] = [Q0, Q1, Q2, FETCH, DEC_SP, OE_A | MW | MA_SP_T | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x11 - PSHX, inherent
template_ucode[0x11] = [Q0, Q1, Q2, FETCH, DEC_SP, OE_X | MW | MA_SP_T | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x12 - PSHY, inherent
template_ucode[0x12] = [Q0, Q1, Q2, FETCH, DEC_SP, OE_Y | MW | MA_SP_T | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x13 - PSHC, inherent
template_ucode[0x13] = [Q0, Q1, Q2, FETCH, DEC_SP, OE_CC | MW | MA_SP_T | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x14 - PULA, inherent
template_ucode[0x14] = [Q0, Q1, Q2, FETCH, MA_SP_T | MR | LD_A, INC_SP | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x15 - PULX, inherent
template_ucode[0x15] = [Q0, Q1, Q2, FETCH, MA_SP_T | MR | LD_X, INC_SP | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x16 - PULY, inherent
template_ucode[0x16] = [Q0, Q1, Q2, FETCH, MA_SP_T | MR | LD_Y, INC_SP | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x17 - PULC, inherent
template_ucode[0x17] = [Q0, Q1, Q2, FETCH, MA_SP_T | MR | LD_CC | N_BUS | C_BUS | I_BUS | V_BUS | Z_BUS, INC_SP | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]

# 0x18 - TFR A,CC, inherent
template_ucode[0x18] = [Q0, Q1, Q2, FETCH, OE_A | LD_CC | N_BUS | V_BUS | Z_BUS | C_BUS | I_BUS | LD_CC | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x19 - TFR CC,A, inherent
template_ucode[0x19] = [Q0, Q1, Q2, FETCH, OE_CC | LD_A | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x1A - TFR X,Y, inherent
template_ucode[0x1A] = [Q0, Q1, Q2, FETCH, OE_X | LD_Y | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x1B - TFR Y,X, inherent
template_ucode[0x1B] = [Q0, Q1, Q2, FETCH, OE_Y | LD_X | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x1C - TFR X,SP, inherent
template_ucode[0x1C] = [Q0, Q1, Q2, FETCH, OE_X | LD_SP | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x1D - TFR SP,X, inherent
template_ucode[0x1D] = [Q0, Q1, Q2, FETCH, OE_SP | LD_X | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x1E - TFR Y,SP, inherent
template_ucode[0x1E] = [Q0, Q1, Q2, FETCH, OE_Y | LD_SP | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x1F - TFR SP,Y, inherent
template_ucode[0x1F] = [Q0, Q1, Q2, FETCH, OE_SP | LD_Y | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]

# 0x20 - BSR, relative
template_ucode[0x20] = [Q0, Q1, Q2, FETCH, DEC_SP, MR | MA_PC | LD_T, OE_PC | ALU_F_D_PLUS_E_CIN | ALU_CIN_ZERO | LD_R | CLR_T | INC_PC, OE_PC | MW | MA_SP_T, OE_R | LD_PC | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]
# 0x21 - BRA, relative
template_ucode[0x21] = [Q0, Q1, Q2, FETCH, MR | MA_PC | LD_T, LD_R | ALU_F_D_PLUS_E_CIN | ALU_CIN_ZERO | OE_PC | CLR_T, OE_R | LD_PC | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]

# 0x43 - RTS, inherent
template_ucode[0x43] = [Q0, Q1, Q2, FETCH, MR | MA_SP_T | LD_PC | INC_SP | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]

# 0x92 - LDSP, immediate
template_ucode[0x92] = [Q0, Q1, Q2, FETCH, MR | LD_SP | INC_PC | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]

# 0xF0 - LDA, immediate
template_ucode[0xF0] = [Q0, Q1, Q2, FETCH, MR | LD_A | INC_PC | NF, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, EMPTY, NF]


with_flags = [
    template_ucode, # 0000
    template_ucode, # 0001
    template_ucode, # 0010
    template_ucode, # 0011
    template_ucode, # 0100
    template_ucode, # 0101
    template_ucode, # 0110
    template_ucode, # 0111
    template_ucode, # 1000
    template_ucode, # 1001
    template_ucode, # 1010
    template_ucode, # 1011
    template_ucode, # 1100
    template_ucode, # 1101
    template_ucode, # 1110
    template_ucode, # 1111
]

all_bytes_0_7 = [0x00] * 2**16
all_bytes_8_15 = [0x00] * 2**16
all_bytes_16_23 = [0x00] * 2**16
all_bytes_24_31 = [0x00] * 2**16
all_bytes_32_39 = [0x00] * 2**16
all_bytes_40_47 = [0x00] * 2**16

for address in range(2**16):
    flags = (address >> 12) & 0b1111
    opcode = (address >> 4) & 0b11111111
    state = address & 0b1111

    b = with_flags[flags][opcode][state]
    #print(f"{address:04x} {b:048b}")
    
    all_bytes_0_7[address] = (b >> 0) & 0xff
    all_bytes_8_15[address] = (b >> 8) & 0xff
    all_bytes_16_23[address] = (b >> 16) & 0xff
    all_bytes_24_31[address] = (b >> 24) & 0xff
    all_bytes_32_39[address] = (b >> 32) & 0xff
    all_bytes_40_47[address] = (b >> 40) & 0xff

with open("ucode_0_7.bin", "wb") as f:
    f.write(bytes(all_bytes_0_7))
with open("ucode_8_15.bin", "wb") as f:
    f.write(bytes(all_bytes_8_15))
with open("ucode_16_23.bin", "wb") as f:
    f.write(bytes(all_bytes_16_23))
with open("ucode_24_31.bin", "wb") as f:
    f.write(bytes(all_bytes_24_31))
with open("ucode_32_39.bin", "wb") as f:
    f.write(bytes(all_bytes_32_39))
with open("ucode_40_47.bin", "wb") as f:
    f.write(bytes(all_bytes_40_47))
