# Ideas for cool IO nodes

## Network IO

Allow for a node to listen for TCP/UDP packets on a specified port, and output the received data on a pin.

Perhaps also have a node that can listen for HTTP requests on a specified port, and output the received data on a pin.

Should also allow for communication with other devices on the network, such as sending TCP/UDP packets, or sending HTTP requests.

Potential applications:

- Sending data to/from a webserver
- Create GUI interfaces for your circuits that can be accessed from a web browser
- Have a separate program for controlling your circuit, like an IDE that can have breakpoints, step through code, etc.

## Mouse IO

Allow for a node to listen for mouse input, and output the mouse position, and mouse button presses on a pin.

Potential applications:

- Make games that use the mouse as an input device

## Oscilloscope

Allow for a node to listen for an input signal and and a trigger signal. Display some kind of diagram of LOW/HIGH of the input signal.

Potential applications:

- Debugging circuits
