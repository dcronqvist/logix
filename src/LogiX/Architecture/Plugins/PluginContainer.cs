using LogiX.Content;
using LogiX.Content.Scripting;
using Symphony;

namespace LogiX.Architecture.Plugins;

public class PluginContainer
{
    public IContentSource ContentSource { get; }

    public PluginContainer(IContentSource source)
    {
        this.ContentSource = source;
    }

    public IEnumerable<IPluginAction> GetActions()
    {
        var identifier = this.ContentSource.GetIdentifier();
        var actionTypes = ScriptManager.GetScriptTypes().Where(t => t.Type.IsAssignableTo(typeof(IPluginAction))).ToList();
        return actionTypes.Where(t => t.Identifier.Split('.').First() == identifier).Select(t => t.CreateInstance<IPluginAction>());
    }

    public ContentMeta GetMeta()
    {
        var config = Utilities.ContentManager.GetConfiguration();
        var validator = config.StructureValidator;

        validator.TryValidateStructure(this.ContentSource.GetStructure(), out var meta, out var error);
        return meta;
    }
}