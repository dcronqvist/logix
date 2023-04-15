# Pin

The pin is one of the most fundamental components in LogiX. It allows you to interact with your circuits in a variety of ways, and is the basis for any circuit that you want to be a sub circuit of another circuit.

### Pin Types

There are two types of pins: **Input** and **Output.** It is however very important to note that this pin type only affects the way it interacts with you the user, and the direct circuit that you are currently editing. It does not affect the way that the pin is able to be used when used as a sub circuit. Any pin, regardless of type, can be used as an input or output pin in a sub circuit. This is to make sure that you can use a pin in any way you want, and to allow for bidirectional pins (very useful for components that use buses).

**Input** pins will allow you as the user to interact with the component, by toggling bits on the pin. It will output the value of the pin.

**Output** pins are basically constantly just waiting for some other component to tell it what value it should have, it never outputs a value on its own.
