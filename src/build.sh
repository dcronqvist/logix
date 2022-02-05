rm -rf ./build

mkdir -p build/win-x64/assets
mkdir -p build/osx-x64/assets
mkdir -p build/linux-x64/assets

dotnet publish . --self-contained true --runtime win-x64 -c Release -o ./build/win-x64
dotnet publish . --self-contained true --runtime osx-x64 -c Release -o ./build/osx-x64
dotnet publish . --self-contained true --runtime linux-x64 -c Release -o ./build/linux-x64

cp -r ./assets ./build/win-x64/assets
cp -r ./assets ./build/osx-x64/assets
cp -r ./assets ./build/linux-x64/assets