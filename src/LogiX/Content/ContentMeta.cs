using Symphony;

namespace LogiX.Content;

public class ContentMeta : ContentMetadata
{
    public string Name { get; set; }
    public string Identifier { get; set; }
    public string Version { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
}