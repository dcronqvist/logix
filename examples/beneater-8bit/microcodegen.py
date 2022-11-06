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
        "J": 1,
    }

    s1 = s1 = sum(1 << control_lines[s] for s in ["MI", "CO"])
    s2 = s2 = sum(1 << control_lines[s] for s in ["RO", "II", "CE"])
    s3 = s3 = sum(1 << control_lines[s] for s in step1)
    s4 = s4 = sum(1 << control_lines[s] for s in step2)
    s5 = s5 = sum(1 << control_lines[s] for s in step3)

    # Return 5 16-bit, with their respective offsets instruction words
    return [
        ((opcode << 3) * 2 | 0, s1),
        ((opcode << 3) * 2 | 2, s2),
        ((opcode << 3) * 2 | 4, s3),
        ((opcode << 3) * 2 | 6, s4),
        ((opcode << 3) * 2 | 8, s5),
    ]

# print example in binary
def print_instr(instr):
    for i in instr:
        print(f"{i[0]:08b} {i[1]:016b}")

i_nop = instr(0b0000, [], [], [])                               # NOP
i_lda = instr(0b0001, ["MI", "IO"], ["RO", "AI"], [])           # LDA <addr>
i_add = instr(0b0010, ["MI", "IO"], ["RO", "BI"], ["EO", "AI"]) # ADD <addr>
i_out = instr(0b1110, ["AO", "OI"], [], [])                     # OUT

all_bytes = [0x0] * 256 * 2

instructions = [i_nop, i_lda, i_add, i_out]

for i in instructions:
    for j in i:
        all_bytes[j[0] + 1] = j[1] >> 8
        all_bytes[j[0]] = j[1] & 0xFF

# output to file
with open("microcode.bin", "wb") as f:
    f.write(bytes(all_bytes))