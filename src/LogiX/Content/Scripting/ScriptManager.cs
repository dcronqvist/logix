using Symphony;

namespace LogiX.Content.Scripting;

public static class ScriptManager
{
    private static List<ScriptType> _scriptTypes = new List<ScriptType>();

    public static void Initialize(ContentManager<ContentMeta> manager)
    {
        _scriptTypes.Clear();

        var assemblies = manager.GetContentItems().Where(x => x is AssemblyContentItem).Cast<AssemblyContentItem>().ToList();

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetScriptTypes();
            _scriptTypes.AddRange(types);
        }
    }

    public static ScriptType GetScriptType(string identifier)
    {
        return _scriptTypes.FirstOrDefault(x => x.Identifier == identifier);
    }
}