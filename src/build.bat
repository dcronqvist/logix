IF EXIST .\bin RMDIR /S /Q .\bin
IF EXIST .\build RMDIR /S /Q .\build

dotnet publish . --self-contained true --runtime win-x64 -c Release -o .\build\win-x64
dotnet publish . --self-contained true --runtime osx-x64 -c Release -o .\build\osx-x64
dotnet publish . --self-contained true --runtime linux-x64 -c Release -o .\build\linux-x64

XCOPY .\assets .\build\win-x64\assets /S/E/I
XCOPY .\assets .\build\osx-x64\assets /S/E/I
XCOPY .\assets .\build\linux-x64\assets /S/E/I

XCOPY .\..\examples .\build\win-x64\examples /S/E/I
XCOPY .\..\examples .\build\osx-x64\examples /S/E/I
XCOPY .\..\examples .\build\linux-x64\examples /S/E/I

XCOPY .\..\plugins\*.zip .\build\win-x64\examples\plugins /S/I
XCOPY .\..\plugins\*.zip .\build\osx-x64\examples\plugins /S/I
XCOPY .\..\plugins\*.zip .\build\linux-x64\examples\plugins /S/I

RAR a -ep1 -r .\build\win-x64\win-x64.zip .\build\win-x64\*