using System.IO.Compression;
using System.Reflection;
using LogiX.Components;
using LogiX.SaveSystem;
using Newtonsoft.Json.Linq;

namespace LogiX.Editor;

public abstract class PluginMethod
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract Action<Editor> OnRun { get; }
}

public class Plugin
{
    public static string PluginsPath => $"{Util.EnvironmentPath}/plugins";

    public static List<Plugin> LoadAllPlugins()
    {
        Directory.CreateDirectory(PluginsPath);
        List<Plugin> plugins = new List<Plugin>();

        // Plugins are zipfiles
        foreach (string file in Directory.GetFiles(PluginsPath, "*.zip"))
        {
            Plugin plugin = LoadFromFile(file);
            if (plugin != null)
            {
                plugins.Add(plugin);
            }
        }
        return plugins;
    }

    public static Plugin LoadFromFile(string file)
    {
        Plugin p;

        // Unzip zip file and check the contents
        ZipArchive zip = ZipFile.OpenRead(file);

        ZipArchiveEntry pluginInfo = zip.Entries.FirstOrDefault(entry => entry.FullName == "plugin.json");

        if (pluginInfo == null)
        {
            return null;
        }
        else
        {
            // Read the plugin.json file
            using (StreamReader sr = new StreamReader(pluginInfo.Open()))
            {
                Dictionary<string, string> pluginInfoDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(sr.ReadToEnd());

                string version = pluginInfoDict["version"];
                string author = pluginInfoDict["author"];
                string description = pluginInfoDict["description"];
                string name = pluginInfoDict["name"];
                string website = pluginInfoDict["website"];

                p = new Plugin(version, author, description, name, website, file);
            }
        }

        ZipArchiveEntry pluginDll = zip.Entries.FirstOrDefault(entry => Path.GetExtension(entry.FullName) == ".dll");

        if (pluginDll == null)
        {
            return null;
        }
        else
        {
            // Read the plugin.dll file
            using (Stream s = pluginDll.Open())
            {
                byte[] buffer = new byte[pluginDll.Length];
                s.Read(buffer, 0, buffer.Length);
                Assembly assembly = Assembly.Load(buffer);

                // Get all classes that have base type "CustomComponent"
                List<Type> types = assembly.GetTypes().Where(t => t.BaseType == typeof(CustomComponent)).ToList();

                foreach (Type type in types)
                {
                    // All classes that have base type "CustomComponent" may ONLY take in a Vector2 in the constructor
                    JObject data = (JObject)type.GetMethod("GetDefaultComponentData").Invoke(null, null);

                    CustomComponent cc = (CustomComponent)assembly.CreateInstance(type.FullName, true, BindingFlags.CreateInstance, null, new object[] { Vector2.Zero, data }, null, null);
                    CustomDescription cd = cc.ToDescription();
                    cd.Plugin = p.name;
                    cd.PluginVersion = p.version;
                    p.AddCustomComponent(cd);
                    p.customComponentTypes.Add(cc.ComponentIdentifier, type);
                }

                // Get all classes that have base type "PluginMethod"
                types = assembly.GetTypes().Where(t => t.BaseType == typeof(PluginMethod)).ToList();
                foreach (Type type in types)
                {
                    PluginMethod pm = (PluginMethod)assembly.CreateInstance(type.FullName, true, BindingFlags.CreateInstance, null, null, null, null);
                    p.AddCustomMethod(pm.Name, pm);
                }
            }
        }

        return p;
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

    public Dictionary<string, Type> customComponentTypes;
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
        this.customComponentTypes = new Dictionary<string, Type>();
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

    public Component CreateComponent(string identifier, Vector2 position)
    {
        Type t = customComponentTypes[identifier];
        return CreateComponent(identifier, position, (JObject)t.GetMethod("GetDefaultComponentData").Invoke(null, null));
    }

    public Component CreateComponent(string identifier, Vector2 position, JObject data)
    {
        Type t = customComponentTypes[identifier];
        CustomComponent c = (CustomComponent)t.GetConstructor(new Type[] { typeof(Vector2), typeof(JObject) }).Invoke(new object[] { position, data });
        c.Plugin = this.name;
        c.PluginVersion = this.version;
        return c;
    }

    public void RunMethod(Editor editor, string name)
    {
        customMethods[name].OnRun(editor);
    }
}