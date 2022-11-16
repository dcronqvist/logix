# RAM (Random Access Memory)

The RAM component is very similar to the ROM component, with the only difference being that you are now able to write to the memory as well. For any action (read or write) to occur, the **EN** (enable) pin must be high. When the **EN** pin is high, the **ADDR** (address) pin will be used to select the memory location to read from or write to.

When **LOAD** is high, the component will read the value at the **DATA** pin and on a rising edge of **CLOCK**, write that value to the memory location specified by the **ADDR** pin. When **LOAD** is low, the component will read the value at the memory location specified by the **ADDR** pin and output it at the **DATA** pin, asynchronously.
