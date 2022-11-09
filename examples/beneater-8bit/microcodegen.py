# This is supposed to be a script to generate micro instruction code for the Beneater 8-bit computer.

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

i_nop = instr(0b0000, [], [], [])                                           # NOP
i_lda = instr(0b0001, ["MI", "IO"], ["RO", "AI"], [])                       # LDA <addr>
i_add = instr(0b0010, ["MI", "IO"], ["RO", "BI"], ["EO", "AI", "FI"])       # ADD <addr>
i_sub = instr(0b0011, ["MI", "IO"], ["RO", "BI"], ["EO", "SU", "AI", "FI"]) # SUB <addr>
i_sta = instr(0b0100, ["MI", "IO"], ["AO", "RI"], [])                       # STA <addr>
i_ldi = instr(0b0101, ["IO", "AI"], [], [])                                 # LDI <data>
i_jmp = instr(0b0110, ["IO", "J"], [], [])                                  # JMP <addr>
i_hlt = instr(0b1111, ["HLT"], [], [])                                      # HLT 
i_out = instr(0b1110, ["AO", "OI"], [], [])                                 # OUT

all_bytes = [0x0] * 2**11 # want 11 address lines

instructions = [
    i_nop,
    i_lda,
    i_add,
    i_sub,
    i_sta,
    i_ldi,
    i_jmp,
    i_hlt,
    i_out
]

# carry 0, zero 0
for i in instructions:
    for j in i:
        all_bytes[j[0]] = j[1] >> 8
        all_bytes[j[0] + (1 << 7)] = j[1] & 0xFF

# carry 1, zero 0
for i in instructions:
    for j in i:
        all_bytes[j[0] + (1 << 8)] = j[1] >> 8
        all_bytes[j[0] + (1 << 8) + (1 << 7)] = j[1] & 0xFF

# carry 0, zero 1
for i in instructions:
    for j in i:
        all_bytes[j[0] + (1 << 9)] = j[1] >> 8
        all_bytes[j[0] + (1 << 9) + (1 << 7)] = j[1] & 0xFF

# carry 1, zero 1
for i in instructions:
    for j in i:
        all_bytes[j[0] + (1 << 8) + (1 << 9)] = j[1] >> 8
        all_bytes[j[0] + (1 << 8) + (1 << 9) + (1 << 7)] = j[1] & 0xFF

# output to file
with open("microcode.bin", "wb") as f:
    f.write(bytes(all_bytes))