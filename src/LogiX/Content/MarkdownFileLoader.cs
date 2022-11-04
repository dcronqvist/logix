using Symphony;

namespace LogiX.Content;

public class MarkdownFileLoader : IContentItemLoader
{
    public async Task<LoadEntryResult> TryLoad(IContentSource source, IContentStructure structure, string pathToItem)
    {
        try
        {
            var fileName = Path.GetFileNameWithoutExtension(pathToItem);
            using (var stream = structure.GetEntryStream(pathToItem, out var entry))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    var text = await sr.ReadToEndAsync();
                    return LoadEntryResult.CreateSuccess(new MarkdownFile($"{source.GetIdentifier()}.markdown.{fileName}", source, text));
                }
            }
        }
        catch (System.Exception ex)
        {
            return LoadEntryResult.CreateFailure(ex.Message);
        }
    }
}