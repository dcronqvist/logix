pluginName="flisp-plugin"

dotnet build -c Release

rm ./$pluginName.dll
cp bin/Release/net7.0/$pluginName.dll ./$pluginName.dll

rm -rf bin
rm -rf obj