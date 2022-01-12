# mikro

The first micro processor created in LogiX, uses a very simplified pipelined design, operating at ~0.25Hz.

There is a very simple example program for it called `infinite_counter.txt` in [programs/](programs/), which counts from 2 to 255 and fills the entire 256 byte memory with these numbers, at their corresponding address. Nothing interesting really, but proof of concept.

In the future, there will be a table describing its instruction set here. For now, the given `infinite_counter.txt` program contains some hints about which bits in the instruction does what.

## ALU

Opcodes:

- `0x0`: `Q = A + B`
- `0x1`: `Q = A - B`
- `0x2`: `Q = A * B`
- `0x3`: `Q = A << B[0]`
- `0x4`: `Q = A & B` (bitwise AND)
- `0x5`: `Q = A | B` (bitwise OR)

Flags:

- `COUT`: carry/overflow
- `EQ`: `A == B`
- `A>B`
- `A<B`
- `ZERO`