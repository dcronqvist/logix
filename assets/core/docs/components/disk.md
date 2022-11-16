# Disk

The disk component will allow you to read and write to a file on your host computer. The disk component is therefore useful in cases where you might want to be able to persist data between sessions, or if you want to be able to read or write data to a file, to be read or written to by other programs. It works as a sort of bridge between your simulation and your host computer.

### Pins

- **ADDR**: The address of the byte to read or write to. The width of this address can be specified in the properties panel. When mounting a file, the address width will be automatically set to log2(file size).

- **BUSY**: The busy pin will be high when the disk component is busy either reading or writing to the connected file. This pin can be used to know when the disk component is ready to read or write to the file.

- **DATAWRITE** / **DATAREAD**: The data pins will be used to write to or read from the file. The width of these pins can be specified in the properties panel. These will always be a multiple of 8 bits.

- **WRITE**: On a rising edge of this pin, the disk component will write the value at the **DATAWRITE** pin to the address specified by the **ADDR** pin. The **BUSY** pin will be high while the disk component is writing to the file.

- **READ**: On a rising edge of this pin, the disk component will read the value at the address specified by the **ADDR** pin and output it at the **DATAREAD** pin. The **BUSY** pin will be high while the disk component is reading from the file.
