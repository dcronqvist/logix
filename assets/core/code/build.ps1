$modName = "core"

dotnet build -c Release

#New-Item -ItemType Directory -Path "../assets/_core" -Force
Copy-Item -Path "./bin/Release/net6.0/$modName.dll" -Destination "./$modName.dll" -Force

Remove-Item ./bin -Force -Recurse -ErrorAction SilentlyContinue
Remove-Item ./obj -Force -Recurse -ErrorAction SilentlyContinue