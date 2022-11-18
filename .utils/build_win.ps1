Remove-Item -Path build/win-x64 -Recurse -Force
mkdir -Force build/win-x64

dotnet clean src/LogiX -c Release
dotnet publish src/LogiX --output "build/win-x64" --runtime win-x64 --self-contained true --configuration Release -p:LogiXPlatform=WINDOWS_NT -p:LogiXType=GUI

dotnet clean src/LogiX -c Release
dotnet publish src/LogiX --output "build/win-x64-tmp" --runtime win-x64 --self-contained true --configuration Release -p:LogiXPlatform=WINDOWS_NT -p:LogiXType=CLI

Copy-Item -Path build/win-x64-tmp/LogiX.exe -Destination build/win-x64/logix-cli.exe
rm -Recurse -Force build/win-x64-tmp

Copy-Item src/LogiX/libs/win/*.dll build/win-x64

mkdir -Force build/win-x64/assets

Copy-Item -Recurse assets/* build/win-x64/assets

