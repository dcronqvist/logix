using System.Reflection;
using LogiX.Content.Scripting;
using Symphony;

namespace LogiX.Content;

public class AssemblyLoader : IContentItemLoader
{
    public async Task<LoadEntryResult> TryLoad(IContentSource source, IContentStructure structure, string pathToItem)
    {
        try
        {
            await Task.Delay(1000);

            using var stream = structure.GetEntryStream(pathToItem, out var entry);

            var assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(stream);

            var defs = assembly.DefinedTypes;

            var fileName = Path.GetFileNameWithoutExtension(pathToItem);
            return LoadEntryResult.CreateSuccess(new AssemblyContentItem($"{source.GetIdentifier()}.assembly.{fileName}", source, assembly));
        }
        catch (System.Exception ex)
        {
            return LoadEntryResult.CreateFailure(ex.Message);
        }
    }
}