# This is supposed to be a script to generate micro instruction code for the Beneater 8-bit computer.

from copy import deepcopy


def instr(opcode, step1, step2, step3):
    control_lines = {
        "HLT": 15,
        "MI": 14,
        "RI": 13,
        "RO": 12,
        "IO": 11,
        "II": 10,
        "AI": 9,
        "AO": 8,
        "EO": 7,
        "SU": 6,
        "BI": 5,
        "OI": 4,
        "CE": 3,
        "CO": 2,
        "J":  1,
        "FI": 0
    }

    s1 = s1 = sum(1 << control_lines[s] for s in ["MI", "CO"])
    s2 = s2 = sum(1 << control_lines[s] for s in ["RO", "II", "CE"])
    s3 = s3 = sum(1 << control_lines[s] for s in step1)
    s4 = s4 = sum(1 << control_lines[s] for s in step2)
    s5 = s5 = sum(1 << control_lines[s] for s in step3)

    # Return 8 16-bit, with their respective offsets instruction words
    return [
        ((opcode << 3) | 0, s1),
        ((opcode << 3) | 1, s2),
        ((opcode << 3) | 2, s3),
        ((opcode << 3) | 3, s4),
        ((opcode << 3) | 4, s5),
        ((opcode << 3) | 5, 0),
        ((opcode << 3) | 6, 0),
        ((opcode << 3) | 7, 0),
    ]

# print example in binary
def print_instr(instr):
    for i in instr:
        print(f"{i[0]:08b} {i[1]:016b}")

instructions = [
    instr(0b0000, [], [], []),                                              # NOP
    instr(0b0001, ["MI", "IO"], ["RO", "AI"], []),                          # LDA <addr>
    instr(0b0010, ["MI", "IO"], ["RO", "BI"], ["EO", "AI", "FI"]),          # ADD <addr>
    instr(0b0011, ["MI", "IO"], ["RO", "BI"], ["EO", "SU", "AI", "FI"]),    # SUB <addr>
    instr(0b0100, ["MI", "IO"], ["AO", "RI"], []),                          # STA <addr>
    instr(0b0101, ["IO", "AI"], [], []),                                    # LDI <data>
    instr(0b0110, ["IO", "J"], [], []),                                     # JMP <addr>
    instr(0b0111, [], [], []),                                              # JC <addr>
    instr(0b1000, [], [], []),                                              # JZ <addr>
    instr(0b1001, [], [], []),                                              # NOP
    instr(0b1010, [], [], []),                                              # NOP
    instr(0b1011, [], [], []),                                              # NOP
    instr(0b1100, [], [], []),                                              # NOP
    instr(0b1101, [], [], []),                                              # NOP
    instr(0b1110, ["AO", "OI"], [], []),                                    # OUT
    instr(0b1111, ["HLT"], [], []),                                         # HLT 
]

all_bytes = [0x0] * 2**11 # want 11 address lines

microcode = [
    deepcopy(instructions),   # Carry 0, Zero 0
    deepcopy(instructions),   # Carry 0, Zero 1
    deepcopy(instructions),   # Carry 1, Zero 0
    deepcopy(instructions),   # Carry 1, Zero 1
]

microcode[0b01][0b1000] = instr(0b1000, ["IO", "J"], [], []) # JZ <addr>
microcode[0b10][0b0111] = instr(0b0111, ["IO", "J"], [], []) # JC <addr>
microcode[0b11][0b1000] = instr(0b1000, ["IO", "J"], [], []) # JZ <addr>
microcode[0b11][0b0111] = instr(0b0111, ["IO", "J"], [], []) # JC <addr>

for address in range(0, 2**10):
    flag_set = (address >> 8) & 0b11
    opcode = (address >> 3) & 0b1111
    sel_bit = (address >> 7) & 0b1
    step = address & 0b111

    if sel_bit == 0:
        all_bytes[address] = microcode[flag_set][opcode][step][1] >> 8
    else:
        all_bytes[address] = microcode[flag_set][opcode][step][1] & 0xFF 

# output to file
with open("microcode.bin", "wb") as f:
    f.write(bytes(all_bytes))