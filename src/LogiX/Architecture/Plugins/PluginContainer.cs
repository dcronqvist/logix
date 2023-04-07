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

    public IEnumerable<IPluginAction> GetActions(ContentManager<ContentMeta> contentManager)
    {
        var identifier = contentManager.GetConfiguration().Loader.GetIdentifierForSource(this.ContentSource);

        var actionTypes = ScriptManager.GetScriptTypes().Where(t => t.Content.IsAssignableTo(typeof(IPluginAction))).ToList();
        return actionTypes.Where(t => t.Identifier.Split(':').First() == identifier).Select(t => t.CreateInstance<IPluginAction>());
    }

    public ContentMeta GetMeta()
    {
        var config = Utilities.ContentManager.GetConfiguration();
        var validator = config.StructureValidator;

        validator.TryValidateStructure(this.ContentSource.GetStructure(), out var meta, out var error);
        return meta;
    }
}