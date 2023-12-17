using System.Collections.Generic;
using System.IO;
using System.Linq;
using LogiX.Graphics;
using LogiX.UserInterfaceContext;
using Symphony;

namespace LogiX.Content.Loaders;

public class ShaderLoader : ILoader
{
    private readonly IAsyncGLContextProvider _gLContextProvider;

    public ShaderLoader(IAsyncGLContextProvider gLContextProvider)
    {
        _gLContextProvider = gLContextProvider;
    }

    public bool IsEntryAffectedByStage(string entryPath)
    {
        return entryPath.EndsWith(".shader");
    }

    public async IAsyncEnumerable<LoadEntryResult> LoadEntry(ContentEntry entry, Stream stream)
    {
        string shaderCodeInEntry = await new StreamReader(stream).ReadToEndAsync();

        string[] requiredKeywordsInEntry = {
            "#VERTEXBEGIN",
            "#VERTEXEND",
            "#FRAGMENTBEGIN",
            "#FRAGMENTEND"
        };

        string[] missingRequiredKeywords = requiredKeywordsInEntry
            .Where(keyword => !shaderCodeInEntry.Contains(keyword))
            .ToArray();

        if (missingRequiredKeywords.Length > 0)
        {
            yield return await LoadEntryResult.CreateFailureAsync(entry.EntryPath, $"Missing required keywords: {string.Join(", ", missingRequiredKeywords)} in shader file.");
            yield break;
        }

        string vertexShaderSource = GetSubstringBetweenDelimiters(shaderCodeInEntry, "#VERTEXBEGIN", "#VERTEXEND");
        string fragmentShaderSource = GetSubstringBetweenDelimiters(shaderCodeInEntry, "#FRAGMENTBEGIN", "#FRAGMENTEND");

        var (success, createdProgram, shaderCreationErrors) = await _gLContextProvider.PerformInGLContext(() =>
        {
            bool success = ShaderProgram.TryCreateShader(vertexShaderSource, fragmentShaderSource, out var createdProgram, out string[] shaderCreationErrors);
            return (success, createdProgram, shaderCreationErrors);
        });

        if (!success)
        {
            yield return await LoadEntryResult.CreateFailureAsync(entry.EntryPath, $"Failed to create shader: {string.Join(", ", shaderCreationErrors)}");
            yield break;
        }

        yield return await LoadEntryResult.CreateSuccessAsync(entry.EntryPath, createdProgram);
    }

    private static string GetSubstringBetweenDelimiters(string str, string startDelimiter, string endDelimiter)
    {
        int startIndex = str.IndexOf(startDelimiter) + startDelimiter.Length;
        int endIndex = str.IndexOf(endDelimiter, startIndex);

        return str.Substring(startIndex, endIndex - startIndex);
    }
}
