$fontFile = $args[0]

# Check if $fontFile is empty
if (-not $fontFile) {
    Write-Host "No font file specified"
    exit 1
}

# Check if the font file exists
if (-not (Test-Path $fontFile)) {
    Write-Host "Font file not found: $fontFile"
    exit 1
}

# Get only the file name without the extension
$fontFileName = (Get-Item $fontFile).BaseName

# Write CWD
Write-Host "CWD: $pwd"

# 72 / 32 * 2 = 4.5, output size / 64 * 2 = screenPxRange
.tools\dist\msdf-atlas-gen.exe -font $fontFile -fontname $fontFileName -type msdf -format png -square4 -imageout mtsdf.png -json mtsdf.json -size 32 -pxrange 8 -scanline

Copy-Item -Path $fontFile -Destination "ttf-file.ttf"
# Package up the font files into a zip file
Compress-Archive -Path "mtsdf.png", "mtsdf.json", "ttf-file.ttf" -DestinationPath "$fontFileName.zip"

# Rename .zip to .font
Rename-Item "$fontFileName.zip" "$fontFileName.font"

# Remove the font files
Remove-Item "mtsdf.png", "mtsdf.json", "ttf-file.ttf"
