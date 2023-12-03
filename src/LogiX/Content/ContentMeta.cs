using System.Collections.Generic;

namespace LogiX.Content;

public record ContentDependency(string Identifier, string Version);

public class ContentMeta
{
    public string Identifier { get; set; }
    public string Author { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }

    public IEnumerable<string> Overwrites { get; set; }
    public IEnumerable<ContentDependency> Dependencies { get; set; }
}
