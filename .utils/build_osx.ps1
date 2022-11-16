Remove-Item -Path build -Recurse -Force
mkdir -Force build

dotnet clean src/LogiX
dotnet publish src/LogiX --self-contained -c Release --os osx -o build/osx -p:LogiXOutputType=WinExe -p:LogiXPlatform=OSX

Copy-Item src/LogiX/libs/osx/*.* build/osx

mkdir -Force build/osx/assets

Copy-Item -Recurse assets/* build/osx/assets
