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
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                var bytes = ms.ToArray();
                //var assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(ms);
                var assembly = Assembly.Load(bytes);

                var defs = assembly.DefinedTypes;

                var fileName = Path.GetFileNameWithoutExtension(pathToItem);
                result = await LoadEntryResult.CreateSuccessAsync(pathToItem, new AssemblyContentItem(source, assembly));
            }
        }
        catch (System.Exception ex)
        {
            result = await LoadEntryResult.CreateFailureAsync(ex.Message);
        }

        yield return result;
    }
}