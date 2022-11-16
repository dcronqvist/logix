# Random Generator

The Random Generator component is a simple way to generate a random number. If the **EN** (enable) pin is high, the output value at the **Q** pin will update on the rising edge of **CLK**. The output value will be a random number between 0 and 2^N-1, where N is the number of bits in the output, which can be specified in the properties panel.

The **R** (reset) pin is an asynchronous reset pin, and when it is high, the output value will be reset to the first value in the random generator's sequence.

In the properties, you can eaither choose to expose a **SEED** pin, or to use a static seed value. If you choose to expose a seed pin, the value you input at the **SEED** pin will be used as a seed for the random number generator. If you choose to use a static seed value, the value you input in the properties panel will be used as a seed for the random number generator.
