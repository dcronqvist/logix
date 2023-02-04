pluginName="d2-plugin"

dotnet build -c Release

rm ./$pluginName.dll
cp bin/Release/net7.0/$pluginName.dll ./$pluginName.dll

rm -rf bin
rm -rf obj

rm -rf ../../../../assets/plugins/d2-plugin
cp -r ../../d2-plugin ../../../../assets/plugins/d2-plugin