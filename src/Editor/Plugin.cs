using System.IO.Compression;
using System.Reflection;
using LogiX.Components;
using LogiX.SaveSystem;

namespace LogiX.Editor;

public delegate bool Execution(Editor editor, out string? error);

public abstract class PluginMethod
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract Execution OnRun { get; }
    public abstract Func<Editor, bool> CanRun { get; }
}

public abstract class AdditionalComponentContext<TComp> where TComp : Component
{
    public abstract Action Submit { get; }
}

public class CustomComponentData { }

public class Plugin
{
    public static string PluginsPath => $"{Util.EnvironmentPath}/plugins";

    public static bool TryLoadAllPlugins(out List<Plugin> plugins, out Dictionary<string, string> failedPlugins)
    {
        Directory.CreateDirectory(PluginsPath);
        plugins = new List<Plugin>();
        failedPlugins = new Dictionary<string, string>();
        // Plugins are zipfiles
        foreach (string file in Directory.GetFiles(PluginsPath, "*.zip"))
        {
            try
            {
                if (!TryLoadFromFile(file, out Plugin? plugin, out string? error))
                {
                    failedPlugins.Add(file, error);
                    continue;
                }
                else
                {
                    plugins.Add(plugin!);
                }
            }
            catch (Exception e)
            {
                failedPlugins.Add(file, e.Message);
            }
        }
        return true;
    }

    public static bool TryGetPluginInfo(string file, out Plugin? plugin, out string? error, out ZipArchive? archive)
    {
        string pluginInfoFile = "plugin.json";

        ZipArchive a = ZipFile.OpenRead(file);

        if (!a.Entries.Any(x => x.FullName == pluginInfoFile))
        {
            error = $"Plugin does not contain a {pluginInfoFile} file.";
            plugin = null;
            archive = null;
            return false;
        }

        ZipArchiveEntry pluginInfo = a.Entries.FirstOrDefault(entry => entry.FullName == pluginInfoFile);

        try
        {
            using (StreamReader sr = new StreamReader(pluginInfo.Open()))
            {
                Dictionary<string, string> pluginInfoDict = JsonSerializer.Deserialize<Dictionary<string, string>>(sr.ReadToEnd());

                string version = pluginInfoDict["version"];
                string author = pluginInfoDict["author"];
                string description = pluginInfoDict["description"];
                string name = pluginInfoDict["name"];
                string website = pluginInfoDict["website"];

                plugin = new Plugin(version, author, description, name, website, file);
                error = null;
                archive = a;
                return true;
            }
        }
        catch (Exception e)
        {
            error = $"Error reading plugin info: {e.Message}";
            plugin = null;
            archive = null;
            return false;
        }
    }

    public static bool TryGetPluginAssembly(ZipArchive zip, Plugin p, out Plugin? plugin, out string? error)
    {
        if (!zip.Entries.Any(x => x.FullName.EndsWith(".dll")))
        {
            error = "Plugin does not contain a .dll file.";
            plugin = null;
            return false;
        }

        if (zip.Entries.Where(x => x.FullName.EndsWith(".dll")).Count() > 1)
        {
            error = "Plugin contains multiple .dll files.";
            plugin = null;
            return false;
        }

        ZipArchiveEntry assemblyFile = zip.Entries.FirstOrDefault(x => x.FullName != "logix.dll" && x.FullName.EndsWith(".dll"))!;

        // Read the plugin.dll file
        using (Stream s = assemblyFile.Open())
        {
            byte[] buffer = new byte[assemblyFile.Length];
            s.Read(buffer, 0, buffer.Length);
            Assembly assembly = Assembly.Load(buffer);

            // Get all classes that have base type "CustomComponent"
            List<Type> types = assembly.GetTypes().Where(t => t.BaseType == typeof(CustomComponent)).ToList();

            foreach (Type type in types)
            {
                // Make sure that all types have a method "GetDefaultComponentData"
                MethodInfo? method = type.GetMethod("GetDefaultComponentData");
                if (method == null || method.IsStatic == false)
                {
                    error = $"Type {type.Name} does not have a static method GetDefaultComponentData.";
                    plugin = null;
                    return false;
                }

                // All classes that have base type "CustomComponent" may ONLY take in a Vector2 in the constructor
                CustomComponentData data = (CustomComponentData)type.GetMethod("GetDefaultComponentData").Invoke(null, null);

                // Make sure that all types have a constructor which takes in a Vector2 and JObject
                ConstructorInfo? constructor = null;
                foreach (ConstructorInfo ci in type.GetConstructors())
                {
                    if (ci.GetParameters().Length == 2)
                    {
                        ParameterInfo[] parameters = ci.GetParameters();
                        if (parameters[0].ParameterType == typeof(Vector2) && parameters[1].ParameterType.IsSubclassOf(typeof(CustomComponentData)))
                        {
                            constructor = ci;
                            break;
                        }
                    }
                }
                if (constructor == null)
                {
                    error = $"Type {type.Name} does not have a constructor which takes in a Vector2 and CustomComponentData.";
                    plugin = null;
                    return false;
                }

                CustomComponent cc = (CustomComponent)constructor.Invoke(BindingFlags.CreateInstance, null, new object[] { Vector2.Zero, data }, null);
                CustomDescription cd = cc.ToDescription();
                cd.Plugin = p.name;
                cd.PluginVersion = p.version;
                p.AddCustomComponent(cd);
                p.customComponentTypes.Add(cc.ComponentIdentifier, (type, constructor, data.GetType()));
            }

            // Get all classes that have base type "PluginMethod"
            types = assembly.GetTypes().Where(t => t.BaseType == typeof(PluginMethod)).ToList();
            foreach (Type type in types)
            {
                PluginMethod pm = (PluginMethod)assembly.CreateInstance(type.FullName, true, BindingFlags.CreateInstance, null, null, null, null);
                p.AddCustomMethod(pm.Name, pm);
            }
        }

        plugin = p;
        error = null;
        return true;
    }

    public static bool TryLoadFromFile(string file, out Plugin? plugin, out string? error)
    {
        ZipArchive? archive;
        if (!TryGetPluginInfo(file, out plugin, out error, out archive))
        {
            return false;
        }

        if (!TryGetPluginAssembly(archive!, plugin!, out plugin, out error))
        {
            return false;
        }

        return true;
    }

    public static bool TryInstall(string file, out List<Plugin> plugins, out string? error)
    {
        if (!TryLoadFromFile(file, out Plugin? plugin, out error))
        {
            // Copy file to plugins folder
            plugins = null;
            return false;
        }

        string newFile = $"{PluginsPath}/{Path.GetFileName(file)}";
        File.Copy(file, newFile);
        plugins = Util.Plugins;
        plugins.Add(plugin!);
        return true;
    }

    // Plugin information
    public string version;
    public string author;
    public string description;
    public string name;
    public string website;
    public string file;

    // Stuff plugins should be able to do
    // contain custom components
    // run void methods by clicking them in the editor

    public Dictionary<string, (Type, ConstructorInfo, Type)> customComponentTypes;
    public Dictionary<string, CustomDescription> customComponents;
    public Dictionary<string, PluginMethod> customMethods;

    public Plugin(string version, string author, string description, string name, string website, string file)
    {
        this.version = version;
        this.author = author;
        this.description = description;
        this.name = name;
        this.website = website;
        this.customComponents = new Dictionary<string, CustomDescription>();
        this.customMethods = new Dictionary<string, PluginMethod>();
        this.customComponentTypes = new Dictionary<string, (Type, ConstructorInfo, Type)>();
        this.file = file;
    }

    public string GetAboutInfo()
    {
        return $"{name} by {author}\nVersion: {version}\nWebsite: {website}\n\n{description}";
    }

    public void AddCustomComponent(CustomDescription customComponent)
    {
        customComponents.Add(customComponent.ComponentIdentifier, customComponent);
    }

    public void AddCustomMethod(string name, PluginMethod method)
    {
        customMethods.Add(name, method);
    }

    public Component CreateComponent(string identifier, Vector2 position, int rotation)
    {
        (Type t, ConstructorInfo ci, Type dt) = customComponentTypes[identifier];
        return CreateComponent(identifier, position, rotation, (CustomComponentData)t.GetMethod("GetDefaultComponentData").Invoke(null, null));
    }

    public Component CreateComponent(string identifier, Vector2 position, int rotation, CustomComponentData data)
    {
        (Type t, ConstructorInfo ci, Type dt) = customComponentTypes[identifier];
        CustomComponent c = (CustomComponent)ci.Invoke(new object[] { position, data });
        c.Plugin = this.name;
        c.PluginVersion = this.version;
        c.Rotation = rotation;
        return c;
    }

    public bool CanRunMethod(Editor editor, string name)
    {
        return customMethods[name].CanRun(editor);
    }
}