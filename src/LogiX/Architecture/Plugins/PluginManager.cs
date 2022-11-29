using LogiX.Content;
using Symphony;

namespace LogiX.Architecture.Plugins;

public static class PluginManager
{
    public static List<PluginContainer> Plugins { get; } = new();

    public static void LoadPlugins(ContentManager<ContentMeta> manager)
    {
        Plugins.Clear();

        foreach (var source in manager.CollectValidSources())
        {
            Plugins.Add(new PluginContainer(source));
        }
    }

    public static IEnumerable<PluginContainer> GetPlugins()
    {
        return Plugins;
    }
}