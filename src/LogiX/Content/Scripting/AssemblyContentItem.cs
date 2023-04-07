using System.Reflection;
using Symphony;

namespace LogiX.Content.Scripting;

public class AssemblyContentItem : ContentItem<Assembly>
{
    public AssemblyContentItem(IContentSource source, Assembly content) : base(source, content)
    {
    }

    public override void Unload()
    {
        // Cannot unload an assembly, is unloaded when program exits
    }

    protected override void OnContentUpdated(Assembly newContent)
    {

    }

    public ScriptType[] GetScriptTypes()
    {
        var types = Content.GetTypes();
        var scriptTypes = new List<ScriptType>();
        for (int i = 0; i < types.Length; i++)
        {
            var type = types[i];
            var attr = type.GetCustomAttribute<ScriptTypeAttribute>();
            if (attr is not null)
            {
                //scriptTypes.Add(new ScriptType(this.Source.GetIdentifier() + ".script_type." + attr.Identifier, type));
            }
        }
        return scriptTypes.ToArray();
    }
}
