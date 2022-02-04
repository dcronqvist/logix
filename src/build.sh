rm -rf ./build

dotnet publish . --self-contained true --use-current-runtime -c Release -o .\build\win-x64 --os win
dotnet publish . --self-contained true --use-current-runtime -c Release -o .\build\osx-x64 --os osx
dotnet publish . --self-contained true --use-current-runtime -c Release -o .\build\linux-x64 --os linux

cp -r ./assets ./build/win-x64/assets
cp -r ./assets ./build/osx-x64/assets
cp -r ./assets ./build/linux-x64/assets