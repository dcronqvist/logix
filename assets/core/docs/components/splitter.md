# Splitter/Combiner

This component allows you to split a single multi-but input into its individual bits, or combine multiple individual bits into a single multi-bit output.

The **bits** property will allow you to set the number of bits that will either be split or combined.

When in **split** mode, the **multi** pin will act as an input and the individual bits will be passed through to the **single_x** pins. However, when in **combine** mode, the **single_x** pins will act as inputs and the combined multi-bit output will be passed to the **multi** pin.
