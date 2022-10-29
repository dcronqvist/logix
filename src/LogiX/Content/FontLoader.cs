using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using LogiX.Graphics;
using StbImageSharp;
using Symphony;

namespace LogiX.Content;

public class FontFileData
{
    public string File { get; set; }
    public int Size { get; set; }
    public FontData.FontFilter MagFilter { get; set; }
    public FontData.FontFilter MinFilter { get; set; }
}

public class FontLoader : IContentItemLoader
{
    public async Task<LoadEntryResult> TryLoad(IContentSource source, IContentStructure structure, string pathToItem)
    {
        return await Task.Run(() =>
        {
            try
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
                        var fontFile = Path.Combine(directory, fontFileDesc.File);

                        using (var fontFileStream = structure.GetEntryStream(fontFile, out var fontFileEntry))
                        {
                            using (var br = new BinaryReader(fontFileStream))
                            {
                                byte[] data = br.ReadBytes((int)fontFileStream.Length);
                                var fd = new FontData(data, (uint)fontFileDesc.Size, fontFileDesc.MagFilter, fontFileDesc.MinFilter);
                                var font = new Font($"{source.GetIdentifier()}.font.{fileName}", source, fd);
                                return LoadEntryResult.CreateSuccess(font);
                            }
                        }
                    }

                }
            }
            catch (System.Exception ex)
            {
                return LoadEntryResult.CreateFailure(ex.Message);
            }
        });
    }
}