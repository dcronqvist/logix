# Shifter

The shifter allows you to shift bits in a specified direction (specify in properties panel). The width of the input and output pins can also be specified in the properties panel.

### Pins

- **X**: The input pin. The width of this pin can be specified in the properties panel.

- **Y**: The output pin. The width of this pin will be the same as the input pin.

- **S**: The shift pin. Determines how many steps to shift the bits, in the direction specified in the properties panel. Will always be log2(widht of input pin) bits wide.

- **IN**: The value to shift in. Will always be a single bit.
