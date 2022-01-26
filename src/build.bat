IF EXIST .\bin RMDIR /S /Q .\bin
IF EXIST .\build RMDIR /S /Q .\build


dotnet publish . --self-contained true --use-current-runtime -c Release -o .\build

XCOPY .\assets .\build\assets /S/E/I