mkdir tmp
Set-Location tmp

Invoke-WebRequest "https://github.com/glfw/glfw/releases/download/3.3.4/glfw-3.3.4.bin.WIN64.zip" -OutFile winglfw.zip

Expand-Archive winglfw.zip

Remove-Item winglfw.zip

mkdir ../src/GoodGame/libs/win

Copy-Item ./winglfw/glfw-3.3.4.bin.WIN64/lib-vc2019/glfw3.dll ../src/GoodGame/libs/win/glfw3.dll

Set-Location ..

Remove-Item tmp -Force -Recurse

Copy-Item ./.utils/omnisharp.json ./omnisharp.json