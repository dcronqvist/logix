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
}
