using System.Reflection;
using GoodGame.Content.Scripting;
using Symphony;

namespace GoodGame.Content;

public class AssemblyLoader : IContentItemLoader
{
    public async Task<LoadEntryResult> TryLoad(IContentSource source, IContentStructure structure, string pathToItem)
    {
        try
        {
            using var stream = structure.GetEntryStream(pathToItem, out var entry);

            var assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(stream);
            var fileName = Path.GetFileNameWithoutExtension(pathToItem);
            return LoadEntryResult.CreateSuccess(new AssemblyContentItem($"{source.GetIdentifier()}.assembly.{fileName}", source, assembly));
        }
        catch (System.Exception ex)
        {
            return LoadEntryResult.CreateFailure(ex.Message);
        }
    }
}