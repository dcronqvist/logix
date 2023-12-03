ifeq ($(OS),Windows_NT)
SHELL := pwsh.exe
.SHELLFLAGS := -NoProfile -Command
endif

current_directory := $(shell pwsh -c "(Get-Location | Foreach Path) -replace '\\', '/'")
debug_args := {"type":"coreclr","request":"launch","program":"$(current_directory)/src/LogiX/bin/Debug/net8.0/LogiX.dll","cwd":"$(current_directory)","just_my_code":false}
encoded_debug_args := $(shell pwsh .tools/scripts/url-encode.ps1 '$(debug_args)')

.PHONY: debug run clean build test
debug: build
	@echo "Starting debug session..."
	@Start-Process "vscode-insiders://fabiospampinato.vscode-debug-launcher/launch?args=$(encoded_debug_args)"

run: build
	@echo "Running Debug build of LogiX..."
	@dotnet run --project src/LogiX/LogiX.csproj --configuration Debug --framework net8.0

watch: build
	@echo "Running Watch build of LogiX..."
	@dotnet watch --project src/LogiX/LogiX.csproj run

build: src/LogiX/_static/icon.ico
	@echo "Building LogiX..."
	@dotnet build src/LogiX/LogiX.csproj --configuration Debug --framework net8.0

publish: src/LogiX/_embeds/logix-core.zip src/LogiX/_static/icon.ico
	@echo "Publishing LogiX..."
	@dotnet publish src/LogiX/LogiX.csproj --configuration Release --framework net8.0 --output build/win-x64 --self-contained true --runtime win-x64 -p:PublishSingleFile=true /p:DebugType=None /p:DebugSymbols=false

clean:
	@echo "Cleaning LogiX..."
	@Remove-Item -Recurse -Force src/LogiX/bin
	@Remove-Item -Recurse -Force src/LogiX/obj
	@Remove-Item -Recurse -Force src/LogiX.Tests/bin
	@Remove-Item -Recurse -Force src/LogiX.Tests/obj

test:
	@echo "Testing LogiX..."
	@dotnet test src/

format:
	@dotnet format src/

src/LogiX/_embeds/logix-core.zip: $(wildcard assets/**/*)
	@echo "Creating 'logix-core' embeddable assets..."
	@Compress-Archive -Force -Path assets/* -DestinationPath src/LogiX/_embeds/logix-core.zip

src/LogiX/_static/icon.ico: assets/textures/icon.png
	@echo "Creating 'icon.ico' from 'icon.png'..."
	@pwsh -NonInteractive -executionpolicy Unrestricted -command "./.tools/scripts/pngtoico.ps1 assets/textures/icon.png src/LogiX/_static/icon.ico"

# Utility targets
.PHONY: create-font
create-font:
	@echo "Creating font using TTF file=$(TTF_FILE)..."
	@./.tools/scripts/createfont.ps1 $(TTF_FILE)
