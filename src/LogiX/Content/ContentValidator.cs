using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Symphony;

namespace GoodGame.Content;

public class ContentValidator : IContentStructureValidator<ContentMeta>
{
    public bool TryValidateStructure(IContentStructure structure, [NotNullWhen(true)] out ContentMeta metadata, [NotNullWhen(false)] out string error)
    {
        if (!structure.HasEntry("meta.json"))
        {
            error = "No meta.json found";
            metadata = null;
            return false;
        }

        using (var stream = structure.GetEntryStream("meta.json", out var entry))
        {
            var options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            metadata = JsonSerializer.Deserialize<ContentMeta>(stream, options);
            error = null;
            return true;
        }
    }
}