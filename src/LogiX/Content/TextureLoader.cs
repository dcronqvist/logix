using System.Diagnostics.CodeAnalysis;
using GoodGame.Graphics;
using StbImageSharp;
using Symphony;

namespace GoodGame.Content;

public class TextureLoader : IContentItemLoader
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
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        ms.Seek(0, SeekOrigin.Begin);

                        var result = ImageResult.FromStream(ms, ColorComponents.RedGreenBlueAlpha);
                        var tex = new Texture2D($"{source.GetIdentifier()}.texture.{fileName}", source, result);
                        return LoadEntryResult.CreateSuccess(tex);
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