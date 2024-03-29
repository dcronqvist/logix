namespace LogiX.Architecture.Serialization;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class NodeInfoAttribute : Attribute
{
    public string DisplayName { get; set; }
    public string Category { get; set; }
    public string DocumentationAsset { get; set; }
    public bool Hidden { get; set; }

    public NodeInfoAttribute(string displayName, string category, string documentationAsset = "logix_core:core/docs/components/template.md", bool hidden = false)
    {
        this.DocumentationAsset = documentationAsset;
        DisplayName = displayName;
        Category = category;
        Hidden = hidden;
    }
}
