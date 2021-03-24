# logix
🔌simulator for logic gates and integrated circuits 

This project is built using [raylib 3.5](https://github.com/raysan5/raylib), which means that to build LogiX from source, you're going to either have to build raylib 3.5 from source as well and include during the build of LogiX, or you can download a prebuilt raylib 3.5 static library from [here](https://github.com/raysan5/raylib/releases/tag/3.5.0). 

Put the downloaded static library file (.a) into the respective win/osx directory in `libs/raylib-cpp/` depending on which platform you're building on. 

After that it's just a matter of performing a simple `mkdir build && cd build && cmake .. && make` and then you should have an executable! (currently only supports Windows and MacOS, Linux support is coming)
