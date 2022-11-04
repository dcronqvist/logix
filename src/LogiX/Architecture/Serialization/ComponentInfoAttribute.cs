namespace LogiX.Architecture.Serialization;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ComponentInfoAttribute : Attribute
{
    public string DisplayName { get; set; }
    public string Category { get; set; }
    public string DocumentationAsset { get; set; }
    public bool Hidden { get; set; }

    public ComponentInfoAttribute(string displayName, string category, string documentationAsset = null, bool hidden = false)
    {
        this.DocumentationAsset = documentationAsset;
        DisplayName = displayName;
        Category = category;
        Hidden = hidden;
    }
}
