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

cp -r ./../examples ./build/win-x64/examples
cp -r ./../examples ./build/osx-x64/examples
cp -r ./../examples ./build/linux-x64/examples

cp -r ./../plugins/*.zip ./build/win-x64/examples/plugins
cp -r ./../plugins/*.zip ./build/osx-x64/examples/plugins
cp -r ./../plugins/*.zip ./build/linux-x64/examples/plugins

zip -r ./build/win-x64/win-x64.zip ./build/win-x64
tar -czf ./build/osx-x64/osx-x64.tar.gz ./build/osx-x64
tar -czf ./build/linux-x64/linux-x64.tar.gz ./build/linux-x64