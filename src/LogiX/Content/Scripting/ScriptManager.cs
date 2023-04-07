using System.Reflection;
using Symphony;

namespace LogiX.Content.Scripting;

public static class ScriptManager
{
    private static List<ScriptType> _scriptTypes = new List<ScriptType>();

    public static void Initialize(ContentManager<ContentMeta> manager)
    {
        _scriptTypes.Clear();

        var scriptTypes = manager.GetContentItems().Where(x => x is ScriptType).Cast<ScriptType>().ToList();
        _scriptTypes.AddRange(scriptTypes);
        _scriptTypes.AddRange(GetCoreScriptTypes());
    }

    private static ScriptType[] GetCoreScriptTypes()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        var scriptTypes = new List<ScriptType>();
        for (int i = 0; i < types.Length; i++)
        {
            var type = types[i];
            var attr = type.GetCustomAttribute<ScriptTypeAttribute>();
            if (attr is not null)
            {
                var st = new ScriptType(null, type);
                st.SetIdentifier($"logix_core:script/{attr.Identifier}");
                scriptTypes.Add(st);
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