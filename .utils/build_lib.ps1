Remove-Item -Path build/lib -Recurse -Force
mkdir -Force build/lib

dotnet publish src/LogiX --output "build/lib" --p:OutputType=Library -p:PublishSingleFile=false --configuration Release

