# FLISP

This is the micro processor that is described in the course [EDA452 Introduction to computer engineering](https://student.portal.chalmers.se/en/chalmersstudies/courseinformation/Pages/SearchCourse.aspx?course_id=31745&parsergrp=3) at Chalmers. The course resources contains software to simulate the FLISP micro processor, but for someone who might want to look further into how the FLISP processor works, all the way to the gate level, this might be something you want to check out.

## Information

Both the data path and control unit have been implemented, however, only a very small subset of the instruction set has been implemented.

Instructions that have been implemented are in [instruction_addresses.txt](rom/instruction_addresses.txt), and their control sequences can be seen in [instructions.txt](rom/instructions.txt). These files are loaded as ROMs into the project for *instant* lookup of instructions during execution.

Currently operates at 1 instruction per 100 ticks (ticks correspond to one update frame = FPS). Because of poor optimisation of the logic simulation, the project might be **VERY** laggy, but I'm looking into how to optimise the logic simulation, if anyone has suggestions, let me know.