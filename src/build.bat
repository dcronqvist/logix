IF EXIST .\bin RMDIR /S /Q .\bin
IF EXIST .\build RMDIR /S /Q .\build

dotnet publish . --self-contained true --runtime win-x64 -c Release -o .\build\win-x64
dotnet publish . --self-contained true --runtime osx-x64 -c Release -o .\build\osx-x64
dotnet publish . --self-contained true --runtime linux-x64 -c Release -o .\build\linux-x64

XCOPY .\assets .\build\win-x64\assets /S/E/I
XCOPY .\assets .\build\osx-x64\assets /S/E/I
XCOPY .\assets .\build\linux-x64\assets /S/E/I