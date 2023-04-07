using System.Diagnostics.CodeAnalysis;
using System.Text;
using Symphony;

namespace LogiX.Content;

public class ShaderLoader : IContentItemLoader
{
    public async IAsyncEnumerable<LoadEntryResult> TryLoadAsync(IContentSource source, IContentStructure structure, string pathToItem)
    {
        var fileName = Path.GetFileNameWithoutExtension(pathToItem);
        var extension = Path.GetExtension(pathToItem);
        using (var stream = structure.GetEntryStream(pathToItem, out var entry))
        {
            using (StreamReader sr = new StreamReader(stream))
            {
                var code = sr.ReadToEnd();

                if (extension == ".vs")
                {
                    var vs = new VertexShader(source, code);
                    yield return await LoadEntryResult.CreateSuccessAsync(pathToItem, vs);
                }
                else if (extension == ".fs")
                {
                    var fs = new FragmentShader(source, code);
                    yield return await LoadEntryResult.CreateSuccessAsync(pathToItem, fs);
                }
                else
                {
                    yield return await LoadEntryResult.CreateFailureAsync($"Unknown shader type {extension}");
                }
            }
        }
    }
}