# File System Ideas

## Idea 1: Single File Read and Write

An initial idea is to create a component which is only able to read and write to a single "mounted" file, where it would allow you to specify an address inside the file and then write bytes at those addresses. Since it would instantly communicate with the real file on the host computer, this might take more or less than a single cycle so some kind of busy-flag would be needed.

This would be the most flexible way to do it I think, since then the user can decide how to structure the insides of this file, and potentially be able to make it so that this mounted file is the "hard drive" of a computer.

Perhaps it could also be modified such that you must read and write a specific number of bytes (blocks) at a time, so that it would more closely resemble a real hard drive. This could be a parameter inside the component, with a default value of 1 for single byte read/write.

The file that is mounted into the component must have a predetermined size, which is determined by the "address size" of the component. This would be a parameter inside the component, with a default value of 24 for 16MB files.
