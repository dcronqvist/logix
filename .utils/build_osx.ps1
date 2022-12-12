Remove-Item -Path build/osx-x64 -Recurse -Force
mkdir -Force build/osx-x64

dotnet clean src/LogiX -c Release
dotnet publish src/LogiX --output "build/osx-x64" --runtime osx-x64 --self-contained true --configuration Release -p:LogiXPlatform=OSX -p:LogiXType=GUI

dotnet clean src/LogiX -c Release
dotnet publish src/LogiX --output "build/osx-x64-tmp" --runtime osx-x64 --self-contained true --configuration Release -p:LogiXPlatform=OSX -p:LogiXType=GUI

Copy-Item -Path build/osx-x64-tmp/LogiX -Destination build/osx-x64/logix-cli
rm -Recurse -Force build/osx-x64-tmp

Copy-Item src/LogiX/libs/osx/*.dll build/osx-x64

mkdir -Force build/osx-x64/assets

Copy-Item -Recurse assets/* build/osx-x64/assets

# Zip the build/osx folder and name it logix-osx-x64-{date}.zip in the format of 20221210
mkdir -Force build/zip
Compress-Archive -Path build/osx-x64 -DestinationPath "build/zip/logix-osx-x64-$(Get-Date -Format 'yyyyMMdd').zip"
