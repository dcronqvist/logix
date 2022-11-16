# ROM (Read Only Memory)

The ROM component is a simple type of memory that can be used to store a set of values. The **address** pin is used to select which byte in the memory to output at the **data** pin. The **enable** pin is used to enable the output of the data, and only outputs the data when **enable** is high, otherwise it outputs a high impedance value, which is represented by a grey wire.

The ROM is often useful for storing a set of values that you want to use in your circuit, such as a lookup table for a digital circuit, or a set of instructions for a microcontroller. The ROM can also be loaded from a selected file, which can be useful when the ROM values are created outside of LogiX.

The file that you load must be a binary file, and the size of the file must be a power of 2, so that the address bits can be used to select the bytes in the file.

Finally, you can modify the ROM contents by using the hexadecimal binary editor when selecting the ROM component, and you can also dump the ROM contents to a file.
