using System.Reflection;
using LogiX.Content.Scripting;
using Symphony;

namespace LogiX.Content;

public class AssemblyLoader : IContentItemLoader
{
    public async IAsyncEnumerable<LoadEntryResult> TryLoadAsync(IContentSource source, IContentStructure structure, string pathToItem)
    {
        LoadEntryResult result = LoadEntryResult.CreateFailure("Failed to load assembly");

        try
        {
            await Task.Delay(1000);

            using var stream = structure.GetEntryStream(pathToItem, out var entry);

            var assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(stream);

            var defs = assembly.DefinedTypes;

            var fileName = Path.GetFileNameWithoutExtension(pathToItem);
            result = await LoadEntryResult.CreateSuccessAsync(new AssemblyContentItem($"{source.GetIdentifier()}.assembly.{fileName}", source, assembly));
        }
        catch (System.Exception ex)
        {
            result = await LoadEntryResult.CreateFailureAsync(ex.Message);
        }

        yield return result;
    }
}