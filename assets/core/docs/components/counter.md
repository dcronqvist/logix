# Counter

The counter component allows you to sequentially count either up or down in binary. Just like many other components, the counter has an enable pin (**EN**), which must be high for the counter to count. The counter will count on the rising edge of the **CLK** pin. The counter will count up if the **C** pin is high, and down if the **C** pin is low. The counter will reset to 0 if the **R** pin is high, asynchronously.

The counter's current value will always be outputted at the **Q** pin. The counter's current value can also be overwritten using the **D** pin, and if the **LD** pin is high (and the **EN** pin), the counter will be set to the value at the **D** pin on the rising edge of the **CLK** pin.
