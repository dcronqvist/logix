using Symphony;

namespace LogiX.Content;

public class MarkdownFileLoader : IContentItemLoader
{
    public async IAsyncEnumerable<LoadEntryResult> TryLoadAsync(IContentSource source, IContentStructure structure, string pathToItem)
    {
        var fileName = Path.GetFileNameWithoutExtension(pathToItem);
        using (var stream = structure.GetEntryStream(pathToItem, out var entry))
        {
            using (StreamReader sr = new StreamReader(stream))
            {
                var text = await sr.ReadToEndAsync();
                yield return await LoadEntryResult.CreateSuccessAsync(pathToItem, new MarkdownFile(source, text));
            }
        }
    }
}