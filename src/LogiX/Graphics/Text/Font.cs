using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DotGL;
using LogiX.Graphics.Rendering;
using LogiX.Graphics.Textures;
using Symphony;

namespace LogiX.Graphics.Text;

public class Font : Content<Font>, IFont
{
    private FontInfo _fontInfo;
    private ITexture2D _sdfTexture2D;
    private byte[] _ttfBytes;

    public Font(FontInfo fontInfo, ITexture2D sdfTexture2D, byte[] ttfBytes)
    {
        _fontInfo = fontInfo;
        _sdfTexture2D = sdfTexture2D;
        _ttfBytes = ttfBytes;
    }

    public FontInfo GetFontInfo()
    {
        return _fontInfo;
    }

    public ITexture2D GetSDFTexture2D()
    {
        return _sdfTexture2D;
    }

    protected override void OnContentUpdated(Font newContent)
    {
        _fontInfo = newContent._fontInfo;
        _sdfTexture2D = newContent._sdfTexture2D;
    }

    public override void Unload()
    {
        GL.glDeleteTextures(_sdfTexture2D.OpenGLID);
    }

    public Vector2 MeasureString(string text)
    {
        var advanceDirection = new Vector2(1, 0);
        var verticalDirection = -1 * new Vector2(advanceDirection.Y, -advanceDirection.X);

        var cursorStart = Vector2.Zero;
        var cursor = cursorStart;
        var lineHeight = GetFontInfo().Metrics.LineHeight;

        List<GlyphVertex> vertices = new List<GlyphVertex>();

        for (int i = 0; i < text.Length; i++)
        {
            char character = text[i];
            var glyph = GetFontInfo().Glyphs.FirstOrDefault(x => x.Unicode == character);

            if (!char.IsWhiteSpace(character))
            {
                foreach (var vertex in AddGlyph(this, glyph, cursor, advanceDirection))
                {
                    vertices.Add(vertex);
                }
            }

            cursor += advanceDirection * glyph.Advance;
        }

        var minX = vertices.Min(x => x.Position.X);
        var maxX = vertices.Max(x => x.Position.X);
        var minY = vertices.Min(x => x.Position.Y);
        var maxY = vertices.Max(x => x.Position.Y);

        return new Vector2(maxX - minX, lineHeight);
    }

    private IEnumerable<GlyphVertex> AddGlyph(IFont font, FontInfo.FontInfoGlyph glyph, Vector2 cursor, Vector2 scaledAdvance)
    {
        var fontInfo = font.GetFontInfo();

        var atlasBounds = glyph.AtlasBounds;
        float texelLeft = atlasBounds.Left / fontInfo.Atlas.Width;
        float texelRight = atlasBounds.Right / fontInfo.Atlas.Width;
        float texelTop = 1f - (atlasBounds.Top / fontInfo.Atlas.Height);
        float texelBottom = 1f - (atlasBounds.Bottom / fontInfo.Atlas.Height);

        Vector2 verticalAdvance = -1 * new Vector2(scaledAdvance.Y, -scaledAdvance.X);
        var planeBounds = glyph.PlaneBounds;
        Vector2 positionLeft = scaledAdvance * planeBounds.Left;
        Vector2 positionRight = scaledAdvance * planeBounds.Right;
        Vector2 positionTop = verticalAdvance * (1f - planeBounds.Top);
        Vector2 positionBottom = verticalAdvance * (1f - planeBounds.Bottom);

        var vertexTopLeft = new GlyphVertex(
            cursor + positionTop + positionLeft,
            ColorF.White,
            new Vector2(texelLeft, texelTop)
        );

        var vertexTopRight = new GlyphVertex(
            cursor + positionTop + positionRight,
            ColorF.White,
            new Vector2(texelRight, texelTop)
        );

        var vertexBottomLeft = new GlyphVertex(
            cursor + positionBottom + positionLeft,
            ColorF.White,
            new Vector2(texelLeft, texelBottom)
        );

        var vertexBottomRight = new GlyphVertex(
            cursor + positionBottom + positionRight,
            ColorF.White,
            new Vector2(texelRight, texelBottom)
        );

        yield return vertexTopLeft;
        yield return vertexBottomLeft;
        yield return vertexTopRight;
        yield return vertexTopRight;
    }

    public byte[] GetTTFBytes() => _ttfBytes;
}
