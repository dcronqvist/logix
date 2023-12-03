using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using Symphony;

namespace LogiX.Content;

public class ContentStructureValidator : IContentStructureValidator<ContentMeta>
{
    public bool TryValidateStructure(IContentStructure structure, [NotNullWhen(true)] out ContentMeta metadata, [NotNullWhen(false)] out string error)
    {
        string metadataFilePath = "meta.json";

        if (!structure.HasEntry(metadataFilePath))
        {
            metadata = default;
            error = $"Missing {metadataFilePath}";
            return false;
        }

        metadata = DeserializeJson<ContentMeta>(structure.GetEntryStream(metadataFilePath));
        error = null;
        return true;
    }

    private T DeserializeJson<T>(Stream stream)
    {
        using var reader = new StreamReader(stream);
        string jsonText = reader.ReadToEnd();

        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        return JsonSerializer.Deserialize<T>(jsonText, options);
    }
}
