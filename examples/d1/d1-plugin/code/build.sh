pluginName="d1-plugin"

dotnet build -c Release

rm ./$pluginName.dll
cp bin/Release/net7.0/$pluginName.dll ./$pluginName.dll

rm -rf bin
rm -rf obj

rm -rf ../../../../assets/plugins/d1-plugin
cp -r ../../d1-plugin ../../../../assets/plugins/d1-plugin