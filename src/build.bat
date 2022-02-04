IF EXIST .\bin RMDIR /S /Q .\bin
IF EXIST .\build RMDIR /S /Q .\build


dotnet publish . --self-contained true --use-current-runtime -c Release -o .\build\win-x64 --os win
dotnet publish . --self-contained true --use-current-runtime -c Release -o .\build\osx-x64 --os osx
dotnet publish . --self-contained true --use-current-runtime -c Release -o .\build\linux-x64 --os linux

XCOPY .\assets .\build\win-x64\assets /S/E/I
XCOPY .\assets .\build\osx-x64\assets /S/E/I
XCOPY .\assets .\build\linux-x64\assets /S/E/I