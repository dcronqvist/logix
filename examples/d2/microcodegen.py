from copy import deepcopy

# Control signals
FSEL_H = 1 << 0
FSEL_SL0 = 1 << 1
FSEL_SL1 = 1 << 2
FSEL_SL2 = 1 << 3
FSEL_SL3 = 1 << 4
FSEL_SL4 = 1 << 5
FSEL_SL5 = 1 << 6
FSEL_SL6 = 1 << 7
FSEL_SL7 = 1 << 8
FSEL_SL8 = 1 << 9
FSEL_SL9 = 1 << 10
ALU_CIN_SL0 = 1 << 11
ALU_CIN_SL1 = 1 << 12
ALU_F0 = 1 << 13
ALU_F1 = 1 << 14
ALU_F2 = 1 << 15
ALU_F3 = 1 << 16
FREG_WE = 1 << 17
FREG_OE = 1 << 18
RREG_WE = 1 << 19
RREG_OE = 1 << 20
TREG_CLR = 1 << 21
TREG_WE = 1 << 22
TREG_OE = 1 << 23
TREG_INC = 1 << 24
TREG_DEC = 1 << 25
AREG_WE = 1 << 26
AREG_OE = 1 << 27
AREG_INC = 1 << 28
AREG_DEC = 1 << 29
RST_IMODE = 1 << 30
NEW_FETCH = 1 << 31
IREG_OE = 1 << 32
IREG_WE = 1 << 33
IDX_SL0 = 1 << 34
IDX_SL1 = 1 << 35
XREG_WE = 1 << 36
XREG_OE = 1 << 37
XREG_INC = 1 << 38
XREG_DEC = 1 << 39
YREG_WE = 1 << 40
YREG_OE = 1 << 41
YREG_INC = 1 << 42
YREG_DEC = 1 << 43
PC_HIGH = 1 << 44
PC_WE = 1 << 45
PC_OE = 1 << 46
PC_INC = 1 << 47
PC_DEC = 1 << 48
SP_HIGH = 1 << 49
SP_WE = 1 << 50
SP_OE = 1 << 51
SP_INC = 1 << 52
SP_DEC = 1 << 53
MW = 1 << 54
MR = 1 << 55
MAS_0 = 1 << 56
MAS_1 = 1 << 57
MAR_HIGH = 1 << 58
MAR_WE = 1 << 59
MAR_OE = 1 << 60
MAR_INC = 1 << 61
MAR_DEC = 1 << 62
BRK = 1 << 63

# UTILITY COMBINATIONS
# ALU FUNCTIONS
ALU_F_00 = 0
ALU_F_FF = ALU_F0
ALU_F_FE = ALU_F1
ALU_F_FC = ALU_F1 | ALU_F0
ALU_F_ADD = ALU_F2
ALU_F_SUB = ALU_F2 | ALU_F0
ALU_F_AND = ALU_F2 | ALU_F1
ALU_F_OR = ALU_F2 | ALU_F1 | ALU_F0
ALU_F_XOR = ALU_F3
ALU_F_LSH = ALU_F3 | ALU_F0
ALU_F_RSH = ALU_F3 | ALU_F1

# ALU CIN
ALU_CIN_0 = 0
ALU_CIN_1 = ALU_CIN_SL0
ALU_CIN_C = ALU_CIN_SL1
ALU_CIN_CB = ALU_CIN_SL1 | ALU_CIN_SL0

# FSEL
FSL_Z_ALU = 0
FSL_Z_REG = FSEL_SL8
FSL_Z_BUS = FSEL_SL9
FSL_Z_SET = FSEL_SL9 | FSEL_SL8 | FSEL_H
FSL_Z_CLR = FSEL_SL9 | FSEL_SL8
FSL_C_ALU = 0
FSL_C_REG = FSEL_SL6
FSL_C_BUS = FSEL_SL7
FSL_C_SET = FSEL_SL7 | FSEL_SL6 | FSEL_H
FSL_C_CLR = FSEL_SL7 | FSEL_SL6
FSL_N_ALU = 0
FSL_N_REG = FSEL_SL4
FSL_N_BUS = FSEL_SL5
FSL_N_SET = FSEL_SL5 | FSEL_SL4 | FSEL_H
FSL_N_CLR = FSEL_SL5 | FSEL_SL4
FSL_V_ALU = 0
FSL_V_REG = FSEL_SL2
FSL_V_BUS = FSEL_SL3
FSL_V_SET = FSEL_SL3 | FSEL_SL2 | FSEL_H
FSL_V_CLR = FSEL_SL3 | FSEL_SL2
FSL_I_ALU = 0
FSL_I_REG = FSEL_SL0
FSL_I_BUS = FSEL_SL1
FSL_I_SET = FSEL_SL1 | FSEL_SL0 | FSEL_H
FSL_I_CLR = FSEL_SL1 | FSEL_SL0
FSL_ALL_BUS = FSL_Z_BUS | FSL_C_BUS | FSL_N_BUS | FSL_V_BUS | FSL_I_BUS

# INDEX
IDX_0 = 0
IDX_T = IDX_SL0
IDX_Y = IDX_SL1
IDX_X = IDX_SL1 | IDX_SL0

# MEMORY ADDRESSING MODE
MAM_MAR = 0
MAM_PC = MAS_0
MAM_SP = MAS_1
MAM_MAR_PLUS_IDX = MAS_1 | MAS_0

# REGISTERS
LD_A = AREG_WE
OE_A = AREG_OE
INC_A = AREG_INC
DEC_A = AREG_DEC
LD_X = XREG_WE
OE_X = XREG_OE
INC_X = XREG_INC
DEC_X = XREG_DEC
LD_Y = YREG_WE
OE_Y = YREG_OE
INC_Y = YREG_INC
DEC_Y = YREG_DEC
LD_T = TREG_WE
OE_T = TREG_OE
INC_T = TREG_INC
DEC_T = TREG_DEC
CLR_T = TREG_CLR | TREG_WE
LD_SPL = SP_WE
LD_SPH = SP_WE | SP_HIGH
OE_SPL = SP_OE
OE_SPH = SP_OE | SP_HIGH
INC_SP = SP_INC
DEC_SP = SP_DEC
LD_PCL = PC_WE 
LD_PCH = PC_WE | PC_HIGH
OE_PCL = PC_OE
OE_PCH = PC_OE | PC_HIGH
INC_PC = PC_INC
DEC_PC = PC_DEC
LD_MARL = MAR_WE
LD_MARH = MAR_WE | MAR_HIGH
OE_MARL = MAR_OE
OE_MARH = MAR_OE | MAR_HIGH
INC_MAR = MAR_INC
DEC_MAR = MAR_DEC
LD_I = IREG_WE
OE_I = IREG_OE # does nothing
LD_R = RREG_WE
OE_R = RREG_OE
LD_F = FREG_WE
OE_F = FREG_OE

# INSTRUCTIONS

Q0 = ALU_F_FF | LD_R
Q1 = OE_R | LD_MARH
Q2 = OE_R | LD_MARL
Q3 = MAM_MAR | MR | LD_PCH | MAR_DEC
Q4 = MAM_MAR | MR | LD_PCL | MAR_DEC
FETCH = MR | MAM_PC | LD_I | PC_INC | CLR_T
NF = NEW_FETCH | MAM_PC

NOP_STATES = [Q0, Q1, Q2, Q3, Q4, FETCH, NF, NF, NF, NF, NF, NF, NF, NF, NF, NF]

# 1 bit interrupt mode, 5 bit flags, 8 bit opcode, 4 bit states
# ucode[interrupt][flags][opcode][state]
template_ucode = [[[NOP_STATES for _ in range(256)] for _ in range(2**5)] for _ in range(2)]

# interrupt code
IRQ_STATES = [
    Q0,
    Q1,
    Q2,
    Q3,
    Q4,
    CLR_T,
    FSL_Z_REG | FSL_C_REG | FSL_N_REG | FSL_V_REG | FSL_I_CLR | LD_F | SP_DEC, # clear I flag to prevent infinite irq loop
    MAM_SP | ALU_F_FF | LD_R | OE_PCH | MW | SP_DEC, # write PCH to stack, decrement SP
    MAM_SP | MW | OE_PCL, # write PCL to stack
    MAM_SP | OE_R | LD_MARH | ALU_F_FC | LD_R, # load MARH with 0xff, load R with 0xfc
    MAM_PC | OE_R | LD_MARL, # load MARL with 0xfc
    MAM_MAR | MR | LD_PCL | MAR_INC,
    MAM_MAR | MR | LD_PCH,
    NF | RST_IMODE,
    NF,
    NF,
    NF
]

template_ucode[1] = [[IRQ_STATES for _ in range(256)] for _ in range(2**5)]

ucode = deepcopy(template_ucode)

CURR_OPCODE = 0

def instr(states):
    global CURR_OPCODE
    for flag in range(2**5):
        ucode[0][flag][CURR_OPCODE] = states
    CURR_OPCODE += 1

F_Z_0 = 1 << 0
F_Z_1 = 1 << 1
F_C_0 = 1 << 2
F_C_1 = 1 << 3
F_N_0 = 1 << 4
F_N_1 = 1 << 5
F_V_0 = 1 << 6
F_V_1 = 1 << 7

def instr_flags(flags, states, nomatchstates):
    global CURR_OPCODE

    for flag in range(2**5):
        if (flags & F_Z_0) and (flag & 0b10000) == 0:
            ucode[0][flag][CURR_OPCODE] = states
        elif (flags & F_Z_1) and (flag & 0b10000) != 0:
            ucode[0][flag][CURR_OPCODE] = states
        elif (flags & F_C_0) and (flag & 0b01000) == 0:
            ucode[0][flag][CURR_OPCODE] = states
        elif (flags & F_C_1) and (flag & 0b01000) != 0:
            ucode[0][flag][CURR_OPCODE] = states
        elif (flags & F_N_0) and (flag & 0b00100) == 0:
            ucode[0][flag][CURR_OPCODE] = states
        elif (flags & F_N_1) and (flag & 0b00100) != 0:
            ucode[0][flag][CURR_OPCODE] = states
        elif (flags & F_V_0) and (flag & 0b00010) == 0:
            ucode[0][flag][CURR_OPCODE] = states
        elif (flags & F_V_1) and (flag & 0b00010) != 0:
            ucode[0][flag][CURR_OPCODE] = states
        else:
            ucode[0][flag][CURR_OPCODE] = nomatchstates

    CURR_OPCODE += 1


def ldimm_reg(reg):
    states = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | reg | PC_INC, NF, NF, NF, NF, NF, NF, NF, NF, NF]
    instr(states)

def ldabs_reg(reg):
    states = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, MAM_MAR | MR | reg, NF, NF, NF, NF, NF, NF, NF, NF]
    instr(states)

def ldidx_reg(reg, idx):
    states = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, MAM_MAR_PLUS_IDX | idx | MR | reg, NF, NF, NF, NF, NF, NF, NF, NF]
    instr(states)

def ldind_reg(reg):
    states = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, MAM_MAR | MR | LD_T | MAR_INC, MAM_MAR | MR | LD_MARH, OE_T | LD_MARL, MAM_MAR | MR | reg, NF, NF, NF, NF, NF]
    instr(states)

def ldindidx_reg(reg, idx):
    states = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, MAM_MAR | MR | LD_T | MAR_INC, MAM_MAR | MR | LD_MARH, OE_T | LD_MARL, MAM_MAR_PLUS_IDX | idx | MR | reg, NF, NF, NF, NF, NF]
    instr(states)

def stabs_reg(reg):
    states = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, MAM_MAR | reg | MW, NF, NF, NF, NF, NF, NF, NF, NF]
    instr(states)

def staidx_reg(reg, idx):
    states = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, MAM_MAR_PLUS_IDX | idx | reg | MW, NF, NF, NF, NF, NF, NF, NF, NF]
    instr(states)

def stind_reg(reg):
    states = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, MAM_MAR | MR | LD_T | MAR_INC, MAM_MAR | MR | LD_MARH, OE_T | LD_MARL, MAM_MAR | MW | reg, NF, NF, NF, NF, NF]
    instr(states)

def stindidx_reg(reg, idx):
    states = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, MAM_MAR | MR | LD_T | MAR_INC, MAM_MAR | MR | LD_MARH, OE_T | LD_MARL, MAM_MAR_PLUS_IDX | idx | MW | reg, NF, NF, NF, NF, NF]
    instr(states)

def trf_reg(reg1, reg2):
    states = [Q0, Q1, Q2, Q3, Q4, FETCH, reg1 | reg2, NF, NF, NF, NF, NF, NF, NF, NF, NF]
    instr(states)

def ireg(reg):
    states = [Q0, Q1, Q2, Q3, Q4, FETCH, reg, NF, NF, NF, NF, NF, NF, NF, NF, NF]
    instr(states)

def alu_inh(alu_f, on_bus, before, cin, load_result):
    states = [Q0, Q1, Q2, Q3, Q4, FETCH, before, on_bus | alu_f | cin | LD_R | LD_F | FSL_Z_ALU | FSL_C_ALU | FSL_N_ALU | FSL_I_REG, OE_R | load_result, NF, NF, NF, NF, NF, NF, NF]
    instr(states)

def alu_imm(alu_f, before, cin):
    states = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_T | INC_PC, before, MAM_PC | OE_A | alu_f | cin | LD_R | LD_F | FSL_Z_ALU | FSL_C_ALU | FSL_N_ALU | FSL_V_ALU | FSL_I_REG, MAM_PC | OE_R | LD_A, NF, NF, NF, NF, NF, NF, NF]
    instr(states)

def alu_abs(alu_f, before, cin, store_in_a):
    states_abs = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, before, MAM_MAR | MR | cin | alu_f | LD_R | LD_F | FSL_Z_ALU | FSL_C_ALU | FSL_N_ALU | FSL_I_REG, OE_R | MW | MAM_MAR, NF, NF, NF, NF, NF, NF, NF]
    states_a = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, before, MAM_MAR | MR | cin | alu_f | LD_R | LD_F | FSL_Z_ALU | FSL_C_ALU | FSL_N_ALU | FSL_I_REG, MAM_PC | OE_R | LD_A, NF, NF, NF, NF, NF, NF, NF]
    if store_in_a:
        instr(states_a)
    else:
        instr(states_abs)

def alu_ind(alu_f, before, cin, store_in_a):
    states_ind = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, MAM_MAR | MR | LD_T | MAR_INC, MAM_MAR | MR | LD_MARH, OE_T | LD_MARL, before, MAM_MAR | MR | cin | alu_f | LD_R | LD_F | FSL_Z_ALU | FSL_C_ALU | FSL_N_ALU | FSL_I_REG, OE_R | MW | MAM_MAR, NF, NF, NF, NF, NF]
    states_a = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, MAM_MAR | MR | LD_T | MAR_INC, MAM_MAR | MR | LD_MARH, OE_T | LD_MARL, before, MAM_MAR | MR | cin | alu_f | LD_R | LD_F | FSL_Z_ALU | FSL_C_ALU | FSL_N_ALU | FSL_I_REG, MAM_PC | OE_R | LD_A, NF, NF, NF, NF, NF]
    if store_in_a:
        instr(states_a)
    else:
        instr(states_ind)

def alu_indidx(alu_f, before, cin, store_in_a, idx):
    states_ind = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, MAM_MAR | MR | LD_T | MAR_INC, MAM_MAR | MR | LD_MARH, OE_T | LD_MARL, before, MAM_MAR_PLUS_IDX | idx | MR | cin | alu_f | LD_R | LD_F | FSL_Z_ALU | FSL_C_ALU | FSL_N_ALU | FSL_I_REG, OE_R | MW | MAM_MAR_PLUS_IDX | idx, NF, NF, NF, NF, NF]
    states_a = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, MAM_MAR | MR | LD_T | MAR_INC, MAM_MAR | MR | LD_MARH, OE_T | LD_MARL, before, MAM_MAR_PLUS_IDX | idx | MR | cin | alu_f | LD_R | LD_F | FSL_Z_ALU | FSL_C_ALU | FSL_N_ALU | FSL_I_REG, MAM_PC | OE_R | LD_A, NF, NF, NF, NF, NF]
    if store_in_a:
        instr(states_a)
    else:
        instr(states_ind)

def alu_absidx(alu_f, idx, before, cin, store_in_a):
    states_absidx = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, before, MAM_MAR_PLUS_IDX | idx | MR | cin | alu_f | LD_R | LD_F | FSL_Z_ALU | FSL_C_ALU | FSL_N_ALU | FSL_I_REG, OE_R | MW | MAM_MAR_PLUS_IDX | idx, NF, NF, NF, NF, NF, NF, NF]
    states_a = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, before, MAM_MAR_PLUS_IDX | idx | MR | cin | alu_f | LD_R | LD_F | FSL_Z_ALU | FSL_C_ALU | FSL_N_ALU | FSL_I_REG, MAM_PC | OE_R | LD_A, NF, NF, NF, NF, NF, NF, NF]
    if store_in_a:
        instr(states_a)
    else:
        instr(states_absidx)

def sp_push(reg):
    states = [Q0, Q1, Q2, Q3, Q4, FETCH, DEC_SP, MAM_SP | reg | MW, NF, NF, NF, NF, NF, NF, NF, NF]
    instr(states)

def sp_pop(reg):
    states = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_SP | MR | reg, INC_SP, NF, NF, NF, NF, NF, NF, NF, NF]
    instr(states)

def jmp_flags(flags):
    states_match = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | INC_PC, MAM_PC | MR | LD_MARH | INC_PC, MAM_PC | OE_MARL | LD_PCL, MAM_PC | OE_MARH | LD_PCH, NF, NF, NF, NF, NF, NF, NF]
    states_nomatch = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | INC_PC, MAM_PC | MR | LD_MARH | INC_PC, NF, NF, NF, NF, NF, NF, NF, NF]
    instr_flags(flags, states_match, states_nomatch)

def flag_modify(flag):
    states = [Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | LD_F | flag, NF, NF, NF, NF, NF, NF, NF, NF, NF, NF]
    instr(states)

# ALL INSTRUCTIONS
# nop
instr(NOP_STATES)

# lda #imm (load accumulator with immediate)
ldimm_reg(LD_A)
# lda abs (load accumulator with M[abs])
ldabs_reg(LD_A)
# lda abs,x (load accumulator with M[abs + X])
ldidx_reg(LD_A, IDX_X)
# lda abs,y (load accumulator with M[abs + Y])
ldidx_reg(LD_A, IDX_Y)
# lda (abs) (load accumulator with M[M[abs]] (indirect))
ldind_reg(LD_A)
# lda (abs),x (load accumulator with M[M[abs] + x] (indirect))
ldindidx_reg(LD_A, IDX_X)
# lda (abs),y (load accumulator with M[M[abs] + y] (indirect))
ldindidx_reg(LD_A, IDX_Y)
# sta abs (store accumulator to M[abs])
stabs_reg(OE_A)
# sta abs,x (store accumulator to M[abs + X])
staidx_reg(OE_A, IDX_X)
# sta abs,y (store accumulator to M[abs + Y])
staidx_reg(OE_A, IDX_Y)
# sta (abs) (store accumulator to M[M[abs]]) (indirect)
stind_reg(OE_A)
# sta (abs), x
stindidx_reg(OE_A, IDX_X)
# sta (abs), y
stindidx_reg(OE_A, IDX_Y)

# ldx #imm (load X with immediate)
ldimm_reg(LD_X)
# ldx abs (load X with M[abs])
ldabs_reg(LD_X)
# ldx abs,x (load X with M[abs + X])
ldidx_reg(LD_X, IDX_X)
# ldx abs,y (load X with M[abs + Y])
ldidx_reg(LD_X, IDX_Y)
# ldx (abs) (load X with M[M[abs]] (indirect))
ldind_reg(LD_X)
# ldx (abs),x (load X with M[M[abs] + x] (indirect))
ldindidx_reg(LD_X, IDX_X)
# ldx (abs),y (load X with M[M[abs] + y] (indirect))
ldindidx_reg(LD_X, IDX_Y)
# stx abs (store X to M[abs])
stabs_reg(OE_X)
# stx abs,x (store X to M[abs + X])
staidx_reg(OE_X, IDX_X)
# stx abs,y (store X to M[abs + Y])
staidx_reg(OE_X, IDX_Y)
# stx (abs) (store X to M[M[abs]]) (indirect)
stind_reg(OE_X)
# stx (abs), x
stindidx_reg(OE_X, IDX_X)
# stx (abs), y
stindidx_reg(OE_X, IDX_Y)

# ldy #imm (load Y with immediate)
ldimm_reg(LD_Y)
# ldy abs (load Y with M[abs])
ldabs_reg(LD_Y)
# ldy abs,x (load Y with M[abs + X])
ldidx_reg(LD_Y, IDX_X)
# ldy abs,y (load Y with M[abs + Y])
ldidx_reg(LD_Y, IDX_Y)
# ldy (abs) (load Y with M[M[abs]] (indirect))
ldind_reg(LD_Y)
# ldy (abs),x (load Y with M[M[abs] + x] (indirect))
ldindidx_reg(LD_Y, IDX_X)
# ldy (abs),y (load Y with M[M[abs] + y] (indirect))
ldindidx_reg(LD_Y, IDX_Y)
# sty abs (store Y to M[abs])
stabs_reg(OE_Y)
# sty abs,x (store Y to M[abs + X])
staidx_reg(OE_Y, IDX_X)
# sty abs,y (store Y to M[abs + Y])
staidx_reg(OE_Y, IDX_Y)
# sty (abs) (store Y to M[M[abs]]) (indirect)
stind_reg(OE_Y)
# sty (abs), x
stindidx_reg(OE_Y, IDX_X)
# sty (abs), y
stindidx_reg(OE_Y, IDX_Y)

# tax (transfer accumulator to X)
trf_reg(OE_A, LD_X)
# tay (transfer accumulator to Y)
trf_reg(OE_A, LD_Y)
# txa (transfer X to accumulator)
trf_reg(OE_X, LD_A)
# tya (transfer Y to accumulator)
trf_reg(OE_Y, LD_A)

# ina (increment accumulator)
ireg(INC_A)
# inx (increment X)
ireg(INC_X)
# iny (increment Y)
ireg(INC_Y)
# inc abs (increment M[abs])
alu_abs(ALU_F_ADD, INC_T, ALU_CIN_0, False)
# inc abs,x (increment M[abs + X])
alu_absidx(ALU_F_ADD, IDX_X, INC_T, ALU_CIN_0, False)
# inc abs,y (increment M[abs + Y])
alu_absidx(ALU_F_ADD, IDX_Y, INC_T, ALU_CIN_0, False)
# inc (abs) (increment M[M[abs]]) (indirect)
alu_ind(ALU_F_ADD, INC_T, ALU_CIN_0, False)
# inc (abs),x (increment M[M[abs] + x]) (indirect)
alu_indidx(ALU_F_ADD, INC_T, ALU_CIN_0, False, IDX_X)
# inc (abs),y (increment M[M[abs] + y]) (indirect)
alu_indidx(ALU_F_ADD, INC_T, ALU_CIN_0, False, IDX_Y)

# dea (decrement accumulator)
ireg(DEC_A)
# dex (decrement X)
ireg(DEC_X)
# dey (decrement Y)
ireg(DEC_Y)
# dec abs (decrement M[abs])
alu_abs(ALU_F_SUB, INC_T, ALU_CIN_0, False)
# dec abs,x (decrement M[abs + X])
alu_absidx(ALU_F_SUB, IDX_X, INC_T, ALU_CIN_0, False)
# dec abs,y (decrement M[abs + Y])
alu_absidx(ALU_F_SUB, IDX_Y, INC_T, ALU_CIN_0, False)
# dec (abs) (decrement M[M[abs]]) (indirect)
alu_ind(ALU_F_SUB, INC_T, ALU_CIN_0, False)
# dec (abs),x (decrement M[M[abs] + x]) (indirect)
alu_indidx(ALU_F_SUB, INC_T, ALU_CIN_0, False, IDX_X)
# dec (abs),y (decrement M[M[abs] + y]) (indirect)
alu_indidx(ALU_F_SUB, INC_T, ALU_CIN_0, False, IDX_Y)

# adc #imm (add with carry immediate)
alu_imm(ALU_F_ADD, MAM_PC, ALU_CIN_C)
# adc abs (add with carry M[abs])
alu_abs(ALU_F_ADD, MAM_PC, ALU_CIN_C, True)
# adc abs,x (add with carry M[abs + X])
alu_absidx(ALU_F_ADD, IDX_X, MAM_PC, ALU_CIN_C, True)
# adc abs,y (add with carry M[abs + Y])
alu_absidx(ALU_F_ADD, IDX_Y, MAM_PC, ALU_CIN_C, True)
# adc (abs) (add with carry M[M[abs]]) (indirect)
alu_ind(ALU_F_ADD, MAM_PC, ALU_CIN_C, True)
# adc (abs),x (add with carry M[M[abs] + x]) (indirect)
alu_indidx(ALU_F_ADD, MAM_PC, ALU_CIN_C, True, IDX_X)
# adc (abs),y (add with carry M[M[abs] + y]) (indirect)
alu_indidx(ALU_F_ADD, MAM_PC, ALU_CIN_C, True, IDX_Y)

# sbc #imm (subtract with carry immediate)
alu_imm(ALU_F_SUB, MAM_PC, ALU_CIN_C)
# sbc abs (subtract with carry M[abs])
alu_abs(ALU_F_SUB, MAM_PC, ALU_CIN_C, True)
# sbc abs,x (subtract with carry M[abs + X])
alu_absidx(ALU_F_SUB, IDX_X, MAM_PC, ALU_CIN_C, True)
# sbc abs,y (subtract with carry M[abs + Y])
alu_absidx(ALU_F_SUB, IDX_Y, MAM_PC, ALU_CIN_C, True)
# sbc (abs) (subtract with carry M[M[abs]]) (indirect)
alu_ind(ALU_F_SUB, MAM_PC, ALU_CIN_C, True)
# sbc (abs),x (subtract with carry M[M[abs] + x]) (indirect)
alu_indidx(ALU_F_SUB, MAM_PC, ALU_CIN_C, True, IDX_X)
# sbc (abs),y (subtract with carry M[M[abs] + y]) (indirect)
alu_indidx(ALU_F_SUB, MAM_PC, ALU_CIN_C, True, IDX_Y)

# and #imm (and immediate)
alu_imm(ALU_F_AND, MAM_PC, ALU_CIN_0)
# and abs (and M[abs])
alu_abs(ALU_F_AND, MAM_PC, ALU_CIN_0, True)
# and abs,x (and M[abs + X])
alu_absidx(ALU_F_AND, IDX_X, MAM_PC, ALU_CIN_0, True)
# and abs,y (and M[abs + Y])
alu_absidx(ALU_F_AND, IDX_Y, MAM_PC, ALU_CIN_0, True)
# and (abs) (and M[M[abs]]) (indirect)
alu_ind(ALU_F_AND, MAM_PC, ALU_CIN_0, True)
# and (abs),x (and M[M[abs] + x]) (indirect)
alu_indidx(ALU_F_AND, MAM_PC, ALU_CIN_0, True, IDX_X)
# and (abs),y (and M[M[abs] + y]) (indirect)
alu_indidx(ALU_F_AND, MAM_PC, ALU_CIN_0, True, IDX_Y)

# ora #imm (or immediate)
alu_imm(ALU_F_OR, MAM_PC, ALU_CIN_0)
# ora abs (or M[abs])
alu_abs(ALU_F_OR, MAM_PC, ALU_CIN_0, True)
# ora abs,x (or M[abs + X])
alu_absidx(ALU_F_OR, IDX_X, MAM_PC, ALU_CIN_0, True)
# ora abs,y (or M[abs + Y])
alu_absidx(ALU_F_OR, IDX_Y, MAM_PC, ALU_CIN_0, True)
# ora (abs) (or M[M[abs]]) (indirect)
alu_ind(ALU_F_OR, MAM_PC, ALU_CIN_0, True)
# ora (abs),x (or M[M[abs] + x]) (indirect)
alu_indidx(ALU_F_OR, MAM_PC, ALU_CIN_0, True, IDX_X)
# ora (abs),y (or M[M[abs] + y]) (indirect)
alu_indidx(ALU_F_OR, MAM_PC, ALU_CIN_0, True, IDX_Y)

# eor #imm (exclusive or immediate)
alu_imm(ALU_F_XOR, MAM_PC, ALU_CIN_0)
# eor abs (exclusive or M[abs])
alu_abs(ALU_F_XOR, MAM_PC, ALU_CIN_0, True)
# eor abs,x (exclusive or M[abs + X])
alu_absidx(ALU_F_XOR, IDX_X, MAM_PC, ALU_CIN_0, True)
# eor abs,y (exclusive or M[abs + Y])
alu_absidx(ALU_F_XOR, IDX_Y, MAM_PC, ALU_CIN_0, True)
# eor (abs) (exclusive or M[M[abs]]) (indirect)
alu_ind(ALU_F_XOR, MAM_PC, ALU_CIN_0, True)
# eor (abs),x (exclusive or M[M[abs] + x]) (indirect)
alu_indidx(ALU_F_XOR, MAM_PC, ALU_CIN_0, True, IDX_X)
# eor (abs),y (exclusive or M[M[abs] + y]) (indirect)
alu_indidx(ALU_F_XOR, MAM_PC, ALU_CIN_0, True, IDX_Y)

# rol (rotate accumulator left)
alu_inh(ALU_F_LSH, OE_A, MAM_PC, ALU_CIN_C, LD_A)
# ror (rotate accumulator right)
alu_inh(ALU_F_RSH, OE_A, MAM_PC, ALU_CIN_C, LD_A)

# pha (push accumulator)
sp_push(OE_A)
# pla (pull accumulator)
sp_pop(LD_A)
# phx (push X)
sp_push(OE_X)
# plx (pull X)
sp_pop(LD_X)
# phy (push Y)
sp_push(OE_Y)
# ply (pull Y)
sp_pop(LD_Y)
# php (push flags)
sp_push(OE_F)
# plp (pull flags) (a little bit special)
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_SP | MR | LD_F | FSL_ALL_BUS | INC_SP, NF, NF, NF, NF, NF, NF, NF, NF, NF])

# jmp abs (jump to abs)
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | INC_PC, MAM_PC | MR | LD_MARH | INC_PC, MAM_PC | OE_MARL | LD_PCL, MAM_PC | OE_MARH | LD_PCH, NF, NF, NF, NF, NF, NF])
# jsr abs (jump to subroutine at abs)
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | INC_PC, MAM_PC | MR | LD_MARH | INC_PC | DEC_SP, OE_PCH | MAM_SP | MW | DEC_SP, OE_PCL | MAM_SP | MW, OE_MARL | LD_PCL, OE_MARH | LD_PCH, NF, NF, NF, NF, NF])
# rts (return from subroutine)
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_SP | MR | LD_PCL | INC_SP, MAM_SP | MR | LD_PCH | INC_SP, NF, NF, NF, NF, NF, NF, NF, NF, NF])

# jeq abs (jump if zero flag is set)
jmp_flags(F_Z_1)
# jne abs (jump if zero flag is clear)
jmp_flags(F_Z_0)
# jcs abs (jump if carry flag is set)
jmp_flags(F_C_1)
# jcc abs (jump if carry flag is clear)
jmp_flags(F_C_0)
# jns abs (jump if negative flag is set)
jmp_flags(F_N_1)
# jnc abs (jump if negative flag is clear)
jmp_flags(F_N_0)
# jvs abs (jump if overflow flag is set)
jmp_flags(F_V_1)
# jvc abs (jump if overflow flag is clear)
jmp_flags(F_V_0)

# cmp #imm (compare immediate)
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_T | INC_PC, MAM_PC | OE_A | ALU_F_SUB | ALU_CIN_0 | LD_F | FSL_Z_ALU | FSL_C_ALU | FSL_N_ALU | FSL_V_ALU | FSL_I_REG, NF, NF, NF, NF, NF, NF, NF, NF, NF])
# cmp abs (compare M[abs])
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | INC_PC, MAM_PC | MR | LD_MARH | INC_PC, MAM_MAR | MR | LD_T, MAM_PC | OE_A | ALU_F_SUB | ALU_CIN_0 | LD_F | FSL_Z_ALU | FSL_C_ALU | FSL_N_ALU | FSL_V_ALU | FSL_I_REG, NF, NF, NF, NF, NF, NF, NF, NF, NF])
# cmp abs,x (compare M[abs + X])
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | INC_PC, MAM_PC | MR | LD_MARH | INC_PC, MAM_MAR_PLUS_IDX | IDX_X | MR | LD_T, MAM_PC | OE_A | ALU_F_SUB | ALU_CIN_0 | LD_F | FSL_Z_ALU | FSL_C_ALU | FSL_N_ALU | FSL_V_ALU | FSL_I_REG, NF, NF, NF, NF, NF, NF, NF, NF, NF])
# cmp abs,y (compare M[abs + Y])
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | INC_PC, MAM_PC | MR | LD_MARH | INC_PC, MAM_MAR_PLUS_IDX | IDX_Y | MR | LD_T, MAM_PC | OE_A | ALU_F_SUB | ALU_CIN_0 | LD_F | FSL_Z_ALU | FSL_C_ALU | FSL_N_ALU | FSL_V_ALU | FSL_I_REG, NF, NF, NF, NF, NF, NF, NF, NF, NF])
# cmp (abs)
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, MAM_MAR | MR | LD_T | MAR_INC, MAM_MAR | MR | LD_MARH, OE_T | LD_MARL, MAM_MAR | MR | LD_T, OE_A | ALU_CIN_0 | ALU_F_SUB | LD_R | LD_F | FSL_Z_ALU | FSL_C_ALU | FSL_N_ALU | FSL_I_REG, NF, NF, NF])
# cmp (abs),x
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, MAM_MAR | MR | LD_T | MAR_INC, MAM_MAR | MR | LD_MARH, OE_T | LD_MARL, MAM_MAR_PLUS_IDX | IDX_X | MR | LD_T, OE_A | ALU_CIN_0 | ALU_F_SUB | LD_R | LD_F | FSL_Z_ALU | FSL_C_ALU | FSL_N_ALU | FSL_I_REG, NF, NF, NF])
# cmp (abs),y
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | PC_INC, MAM_PC | MR | LD_MARH | PC_INC, MAM_MAR | MR | LD_T | MAR_INC, MAM_MAR | MR | LD_MARH, OE_T | LD_MARL, MAM_MAR_PLUS_IDX | IDX_Y | MR | LD_T, OE_A | ALU_CIN_0 | ALU_F_SUB | LD_R | LD_F | FSL_Z_ALU | FSL_C_ALU | FSL_N_ALU | FSL_I_REG, NF, NF, NF])

# bit #imm (bit test immediate)
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_T | INC_PC, MAM_PC | OE_A | ALU_F_AND | ALU_CIN_0 | LD_F | FSL_Z_ALU | FSL_N_ALU | FSL_V_ALU | FSL_I_REG, NF, NF, NF, NF, NF, NF, NF, NF, NF])
# bit abs (bit test M[abs])
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | INC_PC, MAM_PC | MR | LD_MARH | INC_PC, MAM_MAR | MR | LD_T, MAM_PC | OE_A | ALU_F_AND | ALU_CIN_0 | LD_F | FSL_Z_ALU | FSL_N_ALU | FSL_V_ALU | FSL_I_REG, NF, NF, NF, NF, NF, NF, NF, NF, NF])
# bit abs,x (bit test M[abs + X])
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | INC_PC, MAM_PC | MR | LD_MARH | INC_PC, MAM_MAR_PLUS_IDX | IDX_X | MR | LD_T, MAM_PC | OE_A | ALU_F_AND | ALU_CIN_0 | LD_F | FSL_Z_ALU | FSL_N_ALU | FSL_V_ALU | FSL_I_REG, NF, NF, NF, NF, NF, NF, NF, NF, NF])
# bit abs,y (bit test M[abs + Y])
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | INC_PC, MAM_PC | MR | LD_MARH | INC_PC, MAM_MAR_PLUS_IDX | IDX_Y | MR | LD_T, MAM_PC | OE_A | ALU_F_AND | ALU_CIN_0 | LD_F | FSL_Z_ALU | FSL_N_ALU | FSL_V_ALU | FSL_I_REG, NF, NF, NF, NF, NF, NF, NF, NF, NF])
# bit (abs) (bit test M[M[abs]])
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | INC_PC, MAM_PC | MR | LD_MARH | INC_PC, MAM_MAR | MR | LD_T | MAR_INC, MAM_MAR | MR | LD_MARH, OE_T | LD_MARL, MAM_MAR | MR | LD_T, MAM_PC | OE_A | ALU_F_AND | ALU_CIN_0 | LD_F | FSL_Z_ALU | FSL_N_ALU | FSL_V_ALU | FSL_I_REG, NF, NF, NF, NF, NF, NF, NF, NF, NF])
# bit (abs),x
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | INC_PC, MAM_PC | MR | LD_MARH | INC_PC, MAM_MAR | MR | LD_T | MAR_INC, MAM_MAR | MR | LD_MARH, OE_T | LD_MARL, MAM_MAR_PLUS_IDX | IDX_X | MR | LD_T, MAM_PC | OE_A | ALU_F_AND | ALU_CIN_0 | LD_F | FSL_Z_ALU | FSL_N_ALU | FSL_V_ALU | FSL_I_REG, NF, NF, NF, NF, NF, NF, NF, NF, NF])
# bit (abs),y
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | MR | LD_MARL | INC_PC, MAM_PC | MR | LD_MARH | INC_PC, MAM_MAR | MR | LD_T | MAR_INC, MAM_MAR | MR | LD_MARH, OE_T | LD_MARL, MAM_MAR_PLUS_IDX | IDX_Y | MR | LD_T, MAM_PC | OE_A | ALU_F_AND | ALU_CIN_0 | LD_F | FSL_Z_ALU | FSL_N_ALU | FSL_V_ALU | FSL_I_REG, NF, NF, NF, NF, NF, NF, NF, NF, NF])

# clz (clear zero flag)
flag_modify(FSL_Z_CLR | FSL_C_REG | FSL_N_REG | FSL_V_REG | FSL_I_REG)
# sez (set zero flag)
flag_modify(FSL_Z_SET | FSL_C_REG | FSL_N_REG | FSL_V_REG | FSL_I_REG)
# clc (clear carry flag)
flag_modify(FSL_Z_REG | FSL_C_CLR | FSL_N_REG | FSL_V_REG | FSL_I_REG)
# sec (set carry flag)
flag_modify(FSL_Z_REG | FSL_C_SET | FSL_N_REG | FSL_V_REG | FSL_I_REG)
# cln (clear negative flag)
flag_modify(FSL_Z_REG | FSL_C_REG | FSL_N_CLR | FSL_V_REG | FSL_I_REG)
# sen (set negative flag)
flag_modify(FSL_Z_REG | FSL_C_REG | FSL_N_SET | FSL_V_REG | FSL_I_REG)
# clv (clear overflow flag)
flag_modify(FSL_Z_REG | FSL_C_REG | FSL_N_REG | FSL_V_CLR | FSL_I_REG)
# sev (set overflow flag)
flag_modify(FSL_Z_REG | FSL_C_REG | FSL_N_REG | FSL_V_SET | FSL_I_REG)
# cli (clear interrupt enable)
flag_modify(FSL_Z_REG | FSL_C_REG | FSL_N_REG | FSL_V_REG | FSL_I_CLR)
# sei (set interrupt enable)
flag_modify(FSL_Z_REG | FSL_C_REG | FSL_N_REG | FSL_V_REG | FSL_I_SET)

# rti (return from interrupt)
instr([
        Q0, Q1, Q2, Q3, Q4, FETCH, 
        FSL_Z_REG | FSL_C_REG | FSL_N_REG | FSL_V_REG | FSL_I_SET | LD_F,
        MAM_SP | MR | LD_PCL | SP_INC,
        MAM_SP | MR | LD_PCH | SP_INC,
        NF,
        NF,
        NF,
        NF,
        NF,
        NF,
        NF
    ])

# lsp (sp = x,y, x as high byte and y as low byte)
instr([
        Q0, Q1, Q2, Q3, Q4, FETCH, 
        OE_X | LD_SPH,
        OE_Y | LD_SPL,
        NF,
        NF,
        NF,
        NF,
        NF,
        NF,
        NF,
        NF
    ])

# brk (software interrupt)
instr([Q0, Q1, Q2, Q3, Q4, FETCH, BRK | NF, NF, NF, NF, NF, NF, NF, NF, NF, NF, NF, NF])

# lda (x,y) (load accumulator with M[(x << 8) | y])
instr([Q0, Q1, Q2, Q3, Q4, FETCH, MAM_PC | OE_X | LD_MARH, MAM_PC | OE_Y | LD_MARL, MAM_MAR | MR | LD_A, NF, NF, NF, NF, NF, NF, NF, NF, NF])

all_bytes = [0x00] * 2**21

for interrupt in range(2):
    for flag in range(2**5):
        for opcode in range(2**8):
            for state in range(16):
                address = (interrupt << 17) | (flag << 12) | (opcode << 4) | state
                st = ucode[interrupt][flag][opcode][state]
                all_bytes[address * 8] = st & 0xFF
                all_bytes[address * 8 + 1] = (st >> 8) & 0xFF
                all_bytes[address * 8 + 2] = (st >> 16) & 0xFF
                all_bytes[address * 8 + 3] = (st >> 24) & 0xFF
                all_bytes[address * 8 + 4] = (st >> 32) & 0xFF
                all_bytes[address * 8 + 5] = (st >> 40) & 0xFF
                all_bytes[address * 8 + 6] = (st >> 48) & 0xFF
                all_bytes[address * 8 + 7] = (st >> 56) & 0xFF
                

with open("microcode.bin", "wb") as f:
    f.write(bytes(all_bytes))