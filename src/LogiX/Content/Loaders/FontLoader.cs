using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using DotGL;
using LogiX.Graphics.Text;
using LogiX.Graphics.Textures;
using LogiX.UserInterfaceContext;
using StbImageSharp;
using Symphony;

namespace LogiX.Content.Loaders;

public class FontLoader : ILoader
{
    private readonly IAsyncGLContextProvider _gLContextProvider;

    public FontLoader(IAsyncGLContextProvider gLContextProvider)
    {
        _gLContextProvider = gLContextProvider;
    }

    public bool IsEntryAffectedByStage(string entryPath)
    {
        return entryPath.EndsWith(".font");
    }

    public async IAsyncEnumerable<LoadEntryResult> LoadEntry(ContentEntry entry, Stream stream)
    {
        ZipArchive fontArchive = new ZipArchive(stream);

        var fontInfoEntry = fontArchive.GetEntry("mtsdf.json");
        var sdfTextureEntry = fontArchive.GetEntry("mtsdf.png");
        var ttfFileEntry = fontArchive.GetEntry("ttf-file.ttf");

        byte[] ttfFileBytes = ReadBytesFromEntry(ttfFileEntry);

        string fontInfoJsonString = ReadStringContentsFromEntry(fontInfoEntry);
        byte[] sdfPNGTextureBytes = ReadBytesFromEntry(sdfTextureEntry);

        ImageResult sdfTextureImageResult = ImageResult.FromMemory(
            sdfPNGTextureBytes,
            ColorComponents.RedGreenBlueAlpha
        );

        var fontInfo = FontInfo.ParseFromJson(fontInfoJsonString);

        var (textureSuccess, sdfTexture, textureError) = await _gLContextProvider.PerformInGLContext(() =>
        {
            bool success = Texture2D.TryCreateTexture2D(
                sdfTextureImageResult.Width,
                sdfTextureImageResult.Height,
                GL.GL_LINEAR,
                GL.GL_LINEAR,
                GL.GL_CLAMP_TO_EDGE,
                GL.GL_CLAMP_TO_EDGE,
                GL.GL_RGBA,
                sdfTextureImageResult.Data,
                out var sdfTexture,
                out string errorMessage
            );

            return (success, sdfTexture, errorMessage);
        });

        if (!textureSuccess)
        {
            yield return await LoadEntryResult.CreateFailureAsync(entry.EntryPath, $"Failed to create font SDF texture atlas: {textureError}");
            yield break;
        }

        var font = new Font(fontInfo, sdfTexture, ttfFileBytes);

        yield return await LoadEntryResult.CreateSuccessAsync(entry.EntryPath, font);
    }

    private string ReadStringContentsFromEntry(ZipArchiveEntry entry)
    {
        using var entryStream = entry.Open();
        using var entryStreamReader = new StreamReader(entryStream);
        return entryStreamReader.ReadToEnd();
    }

    private byte[] ReadBytesFromEntry(ZipArchiveEntry entry)
    {
        using var entryStream = entry.Open();
        using var entryMemoryStream = new MemoryStream();
        entryStream.CopyTo(entryMemoryStream);
        return entryMemoryStream.ToArray();
    }
}
