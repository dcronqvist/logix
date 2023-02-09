using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cyotek.Drawing.BitmapFont;
using LogiX.Graphics;
using StbImageSharp;
using Symphony;

namespace LogiX.Content;

public class FontLoader : IContentItemLoader
{
    public async IAsyncEnumerable<LoadEntryResult> TryLoadAsync(IContentSource source, IContentStructure structure, string pathToItem)
    {
        var fileName = Path.GetFileNameWithoutExtension(pathToItem);
        using (var stream = structure.GetEntryStream(pathToItem, out var entry))
        {
            var zip = new ZipArchive(stream, ZipArchiveMode.Read);

            var bmfontinfo = zip.GetEntry("bmfontinfo.fnt");
            if (bmfontinfo is null) throw new Exception("bmfontinfo.fnt not found");

            BitmapFont bmfont = new();
            using (var bmfontinfoStream = bmfontinfo.Open())
            {
                using (var ms = new MemoryStream())
                {
                    bmfontinfoStream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    bmfont.Load(ms);
                }
            }

            var regularFile = zip.GetEntry("regular.ttf");
            if (regularFile is null) throw new Exception("regular.ttf not found");
            var boldFile = zip.GetEntry("bold.ttf");
            if (boldFile is null) throw new Exception("bold.ttf not found");
            var italicFile = zip.GetEntry("italic.ttf");
            if (italicFile is null) throw new Exception("italic.ttf not found");
            var boldItalicFile = zip.GetEntry("bold-italic.ttf");
            if (boldItalicFile is null) throw new Exception("bold-italic.ttf not found");

            var sdfFile = zip.GetEntry("sdf.png");
            if (sdfFile is null) throw new Exception("sdf.png not found");

            ImageResult sdfTextureData;

            using (var sdfStream = sdfFile.Open())
            {
                using (var ms = new MemoryStream())
                {
                    sdfStream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    sdfTextureData = ImageResult.FromStream(ms, ColorComponents.RedGreenBlueAlpha);
                }
            }

            byte[] regularFontData;
            using (var regularStream = regularFile.Open())
            {
                using (var ms = new MemoryStream())
                {
                    regularStream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    regularFontData = ms.ToArray();
                }
            }

            byte[] boldFontData;
            using (var boldStream = boldFile.Open())
            {
                using (var ms = new MemoryStream())
                {
                    boldStream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    boldFontData = ms.ToArray();
                }
            }

            byte[] italicFontData;
            using (var italicStream = italicFile.Open())
            {
                using (var ms = new MemoryStream())
                {
                    italicStream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    italicFontData = ms.ToArray();
                }
            }

            byte[] boldItalicFontData;
            using (var boldItalicStream = boldItalicFile.Open())
            {
                using (var ms = new MemoryStream())
                {
                    boldItalicStream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    boldItalicFontData = ms.ToArray();
                }
            }

            var data = new FontData(bmfont, sdfTextureData, regularFontData, boldFontData, italicFontData, boldItalicFontData);
            var font = new Font($"{source.GetIdentifier()}.font.{fileName}", source, data);
            yield return await LoadEntryResult.CreateSuccessAsync(font);
        }
    }
}