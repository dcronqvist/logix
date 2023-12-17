using System.Collections.Generic;
using System.Text.Json;

namespace LogiX.Graphics.Text;

public class FontInfo
{
    public record FontInfoAtlas(string Type, int DistanceRange, int Size, int Width, int Height, string YOrigin);
    public record FontInfoMetrics(int EmSize, float LineHeight, float Ascender, float Descender, float UnderlineY, float UnderlineThickness);
    public record FontInfoGlyphBounds(float Left, float Right, float Bottom, float Top);
    public record FontInfoGlyph(int Unicode, float Advance, FontInfoGlyphBounds PlaneBounds, FontInfoGlyphBounds AtlasBounds);

    public FontInfoAtlas Atlas { get; init; }
    public string Name { get; init; }
    public FontInfoMetrics Metrics { get; init; }
    public IEnumerable<FontInfoGlyph> Glyphs { get; init; }

    public static FontInfo ParseFromJson(string json)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        var fontInfo = JsonSerializer.Deserialize<FontInfo>(json, jsonOptions);

        return fontInfo;
    }
}
