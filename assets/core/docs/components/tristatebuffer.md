# Tristate buffer

This component allows the user to select whether or not to allow the input to pass through to the output. It is useful for creating busses, or similar circuits where multiple components might be connected to the same wire, where only a single component should be active at a time.

When the **enabled** pin is high, the **inpu** pin is connected to the **output** pin, outputting whatever is at **input**. When the **enabled** pin is low, the **input** pin is disconnected from the **output** pin, outputting a high-impedance, disconnected, state.
