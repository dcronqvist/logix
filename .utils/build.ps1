Remove-Item -Path build -Recurse -Force
mkdir -Force build

dotnet clean src/LogiX
dotnet publish src/LogiX --self-contained -c Release -r win-x64 -o build/win-x64 -p:LogiXOutputType=WinExe

Copy-Item src/LogiX/libs/win/*.dll build/win-x64

mkdir -Force build/win-x64/assets

Copy-Item -Recurse assets/* build/win-x64/assets
