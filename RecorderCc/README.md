# Recorder[Cc]
Records mouse, keyboard, and HID input for Windows.

## Build
The program is best compiled cross compiled from a Unix-like OS or
using MSYS2. For MSYS2, install the following packages:
`zip base-devel protobuf-devel mingw-w64-x86_64-gcc mingw-w64-x86_64-protobuf mingw-w64-x86_64-boost`

Debian based distros will need `build-essential` and the relevant
cross-compiler.

## Improvements
My initial inclination was to dump the raw bytes of the `RAWINPUT`
structure to stream. This may be tractable with stream compression, but
makes it more tedious to recover the data.