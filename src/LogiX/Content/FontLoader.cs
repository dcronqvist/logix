using System.Diagnostics.CodeAnalysis;
using LogiX.Graphics;
using StbImageSharp;
using Symphony;

namespace LogiX.Content;

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
                    using (var br = new BinaryReader(stream))
                    {
                        byte[] data = br.ReadBytes((int)stream.Length);
                        var fd = new FontData(data, 16, FontData.FontFilter.NearestNeighbour, FontData.FontFilter.NearestNeighbour);
                        var font = new Font($"{source.GetIdentifier()}.font.{fileName}", source, fd);
                        return LoadEntryResult.CreateSuccess(font);
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