# Plugin Template

If you would like to create your own plugin, doing so is very easy using this plugin template.

## Steps

### 1. Write Custom Components or Plugin Methods

Modify [`Plugin.cs`](Plugin.cs) by creating new custom components or new plugin methods. More information about what you can do with these can be found in the [plugins folder](/examples/plugins).

### 2. Write a `plugin.json` file

Modify the [`plugin.json`](plugin.json) file and add some information about the plugin, like a description and name for the plugin.

### 3. Build the project and zip the plugin

Run a `dotnet build` to build the dll representing the plugin, and create a zipfile with that dll (should be `bin/Debug|Release/net6.0/plugin_template.dll`) together with the `plugin.json`. 

### 4. Install the plugin!

That's all! You should now be able to install your new plugin by navigating to the `Plugins` menu at the top and clicking `Install plugin from file...`. Select your newly created zip-file, and you're good to go.