using System.Diagnostics.CodeAnalysis;
using LogiX.Graphics;
using StbImageSharp;
using Symphony;

namespace LogiX.Content;

public class TextureLoader : IContentItemLoader
{
    public async IAsyncEnumerable<LoadEntryResult> TryLoadAsync(IContentSource source, IContentStructure structure, string pathToItem)
    {
        var fileName = Path.GetFileNameWithoutExtension(pathToItem);
        using (var stream = structure.GetEntryStream(pathToItem, out var entry))
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);

                var result = ImageResult.FromStream(ms, ColorComponents.RedGreenBlueAlpha);
                var tex = new Texture2D($"{source.GetIdentifier()}.texture.{fileName}", source, result);
                yield return await LoadEntryResult.CreateSuccessAsync(tex);
            }
        }
    }
}