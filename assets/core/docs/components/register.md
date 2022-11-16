# Register

The register component is very useful for storing a value. The register has an input pin **>** which is the clock pin, and another input pin **D** which is the input data pin.

The register component will store the value on the **D** pin when the **>** pin goes high (only stores on a rising edge), and that value will be visible on the **Q** pin. The **Q** pin is the output pin, and it will output the value that is stored in the register currently.

However, the register also has an input pin **EN** which is the enable pin, and when this pin is low, the register will not store the value on the **D** pin.

Finally, the register has an input pin **R** pin which is the reset pin, and when this pin is high, the register will asynchronously reset to the value 0.
