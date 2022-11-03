namespace LogiX.Architecture.Serialization;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ComponentInfoAttribute : Attribute
{
    public string DisplayName { get; set; }
    public string Category { get; set; }
    public bool Hidden { get; set; }

    public ComponentInfoAttribute(string displayName, string category, bool hidden = false)
    {
        DisplayName = displayName;
        Category = category;
        Hidden = hidden;
    }
}
