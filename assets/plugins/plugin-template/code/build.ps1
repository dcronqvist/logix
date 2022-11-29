$pluginName = "plugin-template"

dotnet build -c Release

Copy-Item -Path "./bin/Release/net7.0/$pluginName.dll" -Destination "./$pluginName.dll" -Force

Remove-Item ./bin -Force -Recurse -ErrorAction SilentlyContinue
Remove-Item ./obj -Force -Recurse -ErrorAction SilentlyContinue