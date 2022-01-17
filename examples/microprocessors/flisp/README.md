# FLISP

This is the micro processor that is described in the course [EDA452 Introduction to computer engineering](https://student.portal.chalmers.se/en/chalmersstudies/courseinformation/Pages/SearchCourse.aspx?course_id=31745&parsergrp=3) at Chalmers. The course resources contains software to simulate the FLISP micro processor, but for someone who might want to look further into how the FLISP processor works, all the way to the gate level, this might be something you want to check out.

## Information

Both the data path and control unit have been implemented, however, only a very small subset of the instruction set has been implemented.

Instructions that have been implemented are in [instruction_addresses.txt](rom/instruction_addresses.txt), and their control sequences can be seen in [instructions.txt](rom/instructions.txt). These files are loaded as ROMs into the project for *instant* lookup of instructions during execution.

Operates at 1 instruction per 20 ticks, which is a lot faster than it was before (which was 1 instruction per 100 ticks). Using new predefined multiplexer and demultiplexer components, I was able to bring down the total gate amount by about 50%.