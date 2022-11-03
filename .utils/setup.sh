mkdir tmp && # Create a temporary directory
cd tmp && # Go to the temporary directory

# Download the latest win64 binaries for glfw and store as winglfw.zip
curl -sSL https://github.com/glfw/glfw/releases/download/3.3.4/glfw-3.3.4.bin.WIN64.zip > winglfw.zip &&
curl -sSL https://www.openal-soft.org/openal-binaries/openal-soft-1.22.1-bin.zip > openalwin.zip &&

# Unzip the downloaded zip file
unzip winglfw.zip &&
unzip openalwin.zip &&

# Remove the zip file
rm winglfw.zip &&
rm openalwin.zip &&

# Create a new directory for the glfw binaries in the project
mkdir -p ../libs/win &&

# Copy the glfw binaries to the project's glfw directory
cp ./glfw-3.3.4.bin.WIN64/lib-vc2019/glfw3.dll ../libs/win/glfw3.dll &&
cp ./openal-soft-1.22.1-bin/bin/Win64/soft_oal.dll ../libs/win/openal32.dll &&

# Go back to the original directory
cd .. &&

# Remove the temporary directory
rm -rf ./tmp 

