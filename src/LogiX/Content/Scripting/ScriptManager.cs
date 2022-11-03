using System.Reflection;
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

        _scriptTypes.AddRange(GetScriptTypesInAssembly(Assembly.GetExecutingAssembly()));
    }

    private static ScriptType[] GetScriptTypesInAssembly(Assembly assembly)
    {
        var types = assembly.GetTypes();
        var scriptTypes = new List<ScriptType>();
        for (int i = 0; i < types.Length; i++)
        {
            var type = types[i];
            var attr = type.GetCustomAttribute<ScriptTypeAttribute>();
            if (attr is not null)
            {
                scriptTypes.Add(new ScriptType("logix_builtin.script_type." + attr.Identifier, type));
            }
        }
        return scriptTypes.ToArray();
    }

    public static ScriptType GetScriptType(string identifier)
    {
        return _scriptTypes.FirstOrDefault(x => x.Identifier == identifier);
    }

    public static ScriptType[] GetScriptTypes()
    {
        return _scriptTypes.ToArray();
    }
}