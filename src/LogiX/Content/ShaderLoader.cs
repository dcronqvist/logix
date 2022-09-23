using System.Diagnostics.CodeAnalysis;
using System.Text;
using Symphony;

namespace LogiX.Content;

public class ShaderLoader : IContentItemLoader
{
    public async Task<LoadEntryResult> TryLoad(IContentSource source, IContentStructure structure, string pathToItem)
    {
        return await Task.Run(() =>
        {
            try
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
                            var vs = new VertexShader($"{source.GetIdentifier()}.vertex_shader.{fileName}", source, code);
                            return LoadEntryResult.CreateSuccess(vs);
                        }
                        else if (extension == ".fs")
                        {
                            var fs = new FragmentShader($"{source.GetIdentifier()}.fragment_shader.{fileName}", source, code);
                            return LoadEntryResult.CreateSuccess(fs);
                        }

                        return LoadEntryResult.CreateFailure($"Unknown shader type {extension}");
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