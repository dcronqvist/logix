using System.Drawing;
using System.Numerics;
using FreeTypeSharp;
using LogiX.Content;
using Symphony;
using static LogiX.OpenGL.GL;
using static FreeTypeSharp.Native.FT;
using FreeTypeSharp.Native;
using Cyotek.Drawing.BitmapFont;
using StbImageSharp;

namespace LogiX.Graphics;

public enum FontStyle
{
    Regular,
    Bold,
    Italic,
    BoldItalic
}

public class FontData
{
    public BitmapFont BitmapFontInfo { get; set; }
    public ImageResult SDFTextureData { get; set; }

    public byte[] RegularFontData { get; set; }
    public byte[] BoldFontData { get; set; }
    public byte[] ItalicFontData { get; set; }
    public byte[] BoldItalicFontData { get; set; }

    public FontData(BitmapFont bitmapFontInfo, ImageResult sdfTextureData, byte[] regularFontData, byte[] boldFontData, byte[] italicFontData, byte[] boldItalicFontData)
    {
        this.BitmapFontInfo = bitmapFontInfo;
        this.SDFTextureData = sdfTextureData;
        this.RegularFontData = regularFontData;
        this.BoldFontData = boldFontData;
        this.ItalicFontData = italicFontData;
        this.BoldItalicFontData = boldItalicFontData;
    }
}

public class Font : GLContentItem<FontData>
{
    public float MaxY => this.Content.BitmapFontInfo.LineHeight;
    public uint TextureID => this.Texture.GLID;
    public int AtlasWidth => this.Texture.Width;
    public int AtlasHeight => this.Texture.Height;
    public Texture2D Texture { get; private set; }

    public Font(string identifier, IContentSource source, FontData content) : base(identifier, source, content)
    {
        this.InitNoGL();
    }

    public unsafe void InitNoGL()
    {
        var fontData = this.Content;
    }

    public override void DestroyGL()
    {
        glDeleteTexture(this.TextureID);
    }

    public unsafe override void InitGL(FontData newContent)
    {
        var fontData = this.Content;
        var texture = new Texture2D(fontData.BitmapFontInfo.TextureSize.Width, fontData.BitmapFontInfo.TextureSize.Height, fontData.SDFTextureData.Data);

        this.Texture = texture;
    }

    public override bool IsGLInitialized()
    {
        return this.TextureID != 0;
    }

    public Vector2 MeasureString(string text, float scale)
    {
        var fontData = this.Content;
        var font = fontData.BitmapFontInfo;

        float height = this.MaxY - font.Padding.Top - font.Padding.Bottom;
        float width = 0;

        foreach (var c in text)
        {
            var glyph = font.Characters[c];
            width += glyph.XAdvance - font.Padding.Left;
        }

        return new Vector2(width * scale, height * scale);
    }
}