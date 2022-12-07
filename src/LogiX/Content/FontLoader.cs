using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using LogiX.Graphics;
using StbImageSharp;
using Symphony;

namespace LogiX.Content;

public class FontFileData
{
    // The 4 different types of font files that are needed to load a font
    public string FileRegular { get; set; }
    public string FileBold { get; set; }
    public string FileItalic { get; set; }
    public string FileBoldItalic { get; set; }
    public bool ApplyIconRange { get; set; }

    // Which sizes that should be available for this font
    public int[] Sizes { get; set; }

    // Mag and min filter
    public FontData.FontFilter MagFilter { get; set; }
    public FontData.FontFilter MinFilter { get; set; }
}

public class FontLoader : IContentItemLoader
{
    public async IAsyncEnumerable<LoadEntryResult> TryLoadAsync(IContentSource source, IContentStructure structure, string pathToItem)
    {
        var fileName = Path.GetFileNameWithoutExtension(pathToItem);
        using (var stream = structure.GetEntryStream(pathToItem, out var entry))
        {
            using (StreamReader sr = new StreamReader(stream))
            {
                var json = sr.ReadToEnd();
                var options = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter() }
                };
                var fontFileDesc = JsonSerializer.Deserialize<FontFileData>(json, options);

                var directory = Path.GetDirectoryName(pathToItem);
                var regularFile = Path.Combine(directory, fontFileDesc.FileRegular);
                var boldFile = Path.Combine(directory, fontFileDesc.FileBold);
                var italicFile = Path.Combine(directory, fontFileDesc.FileItalic);
                var boldItalicFile = Path.Combine(directory, fontFileDesc.FileBoldItalic);

                var files = new (string, string)[] { (regularFile, "regular"), (boldFile, "bold"), (italicFile, "italic"), (boldItalicFile, "bold-italic") };

                foreach (var (file, id) in files)
                {
                    using (var fontFileStream = structure.GetEntryStream(file, out var fontFileEntry))
                    {
                        using (var br = new BinaryReader(fontFileStream))
                        {
                            byte[] data = br.ReadBytes((int)fontFileStream.Length);

                            foreach (var size in fontFileDesc.Sizes)
                            {
                                var fd = new FontData(data, (uint)size, fontFileDesc.MagFilter, fontFileDesc.MinFilter);
                                var font = new Font($"{source.GetIdentifier()}.font.{fileName}-{id}-{size}", source, fd, fontFileDesc.ApplyIconRange);
                                yield return await LoadEntryResult.CreateSuccessAsync(font);
                            }
                        }
                    }
                }
            }
        }
    }
}