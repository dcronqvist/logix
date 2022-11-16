mkdir tmp
Set-Location tmp

Invoke-WebRequest "https://github.com/glfw/glfw/releases/download/3.3.4/glfw-3.3.4.bin.WIN64.zip" -OutFile winglfw.zip
Invoke-WebRequest "https://github.com/glfw/glfw/releases/download/3.3.4/glfw-3.3.4.bin.MACOS.zip" -OutFile osxglfw.zip

Expand-Archive winglfw.zip
Expand-Archive osxglfw.zip

Remove-Item winglfw.zip
Remove-Item osxglfw.zip

mkdir ../src/LogiX/libs/win -Force
mkdir ../src/LogiX/libs/osx -Force

Copy-Item ./winglfw/glfw-3.3.4.bin.WIN64/lib-vc2019/glfw3.dll ../src/LogiX/libs/win/glfw3.dll
Copy-Item ./osxglfw/glfw-3.3.4.bin.MACOS/lib-universal/libglfw.3.dylib ../src/LogiX/libs/osx/libglfw.3.dylib

Set-Location ..

Remove-Item tmp -Force -Recurse

Copy-Item ./.utils/omnisharp.json ./omnisharp.json