using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using LogiX.Graphics;
using Symphony;

namespace LogiX.Content;

public class ShaderProgramLoader : IContentItemLoader
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
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        var json = sr.ReadToEnd();
                        var options = new JsonSerializerOptions()
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };
                        var programDescription = JsonSerializer.Deserialize<ShaderProgramDescription>(json, options);

                        var sp = new ShaderProgram($"{source.GetIdentifier()}.shader_program.{fileName}", source, programDescription);
                        return LoadEntryResult.CreateSuccess(sp);
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