using System.Collections.Generic;
using System.IO;
using LogiX.Graphics.Textures;
using LogiX.UserInterfaceContext;
using StbImageSharp;
using Symphony;
using static DotGL.GL;

namespace LogiX.Content.Loaders;

public class TextureLoader : ILoader
{
    private readonly IAsyncGLContextProvider _gLContextProvider;

    public TextureLoader(IAsyncGLContextProvider gLContextProvider)
    {
        _gLContextProvider = gLContextProvider;
    }

    public bool IsEntryAffectedByStage(string entryPath)
    {
        return entryPath.EndsWith(".png");
    }

    public async IAsyncEnumerable<LoadEntryResult> LoadEntry(ContentEntry entry, Stream stream)
    {
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        var imageResult = ImageResult.FromMemory(memoryStream.ToArray(), ColorComponents.RedGreenBlueAlpha);

        var textureWidth = imageResult.Width;
        var textureHeight = imageResult.Height;
        var textureData = imageResult.Data;

        var (success, createdTexture, textureCreationError) = await _gLContextProvider.PerformInGLContext(() =>
        {
            bool success = Texture2D.TryCreateTexture2D(
                textureWidth,
                textureHeight,
                GL_LINEAR,
                GL_NEAREST,
                GL_CLAMP_TO_EDGE,
                GL_CLAMP_TO_EDGE,
                GL_RGBA,
                textureData,
                out var createdTexture,
                out string textureCreationError);

            return (success, createdTexture, textureCreationError);
        });

        if (!success)
        {
            yield return await LoadEntryResult.CreateFailureAsync(entry.EntryPath, $"Failed to create texture: {textureCreationError}");
            yield break;
        }

        yield return await LoadEntryResult.CreateSuccessAsync(entry.EntryPath, createdTexture);
    }
}
