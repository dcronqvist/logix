$pluginName = "d2-plugin"

dotnet build -c Release

Copy-Item -Path "./bin/Release/net7.0/$pluginName.dll" -Destination "./$pluginName.dll" -Force

Remove-Item ./bin -Force -Recurse -ErrorAction SilentlyContinue
Remove-Item ./obj -Force -Recurse -ErrorAction SilentlyContinue

Remove-Item "../../../../assets/plugins/$pluginName" -Force -Recurse -ErrorAction SilentlyContinue
Copy-Item -Recurse -Path "../../$pluginName" -Destination "../../../../assets/plugins/$pluginName"