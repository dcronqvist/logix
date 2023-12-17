using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LogiX.Extensions;
using LogiX.Graphics.Cameras;
using LogiX.Graphics.Text;
using LogiX.Rendering;
using static DotGL.GL;

namespace LogiX.Graphics.Rendering;

public record GlyphVertex(Vector2 Position, ColorF Color, Vector2 UV);

public class TextRenderer
{
    private readonly PrimitiveRenderer _primitiveRenderer;
    private readonly Stack<IFont> _fontStack = new();

    private uint _vao;
    private uint _vbo;
    private ShaderProgram _shader;

    public TextRenderer(PrimitiveRenderer primitiveRenderer)
    {
        _primitiveRenderer = primitiveRenderer;
        InitGL();
    }

    private void InitGL()
    {
        _vao = glGenVertexArray();
        glBindVertexArray(_vao);

        _vbo = glGenBuffer();
        glBindBuffer(GL_ARRAY_BUFFER, _vbo);

        // Vertex = x,y, r,g,b,a, u,v
        int stride = 8;

        glEnableVertexAttribArray(0);
        glVertexAttribPointer(0, 2, GL_FLOAT, false, stride * sizeof(float), 0);

        glEnableVertexAttribArray(1);
        glVertexAttribPointer(1, 4, GL_FLOAT, false, stride * sizeof(float), 2 * sizeof(float));

        glEnableVertexAttribArray(2);
        glVertexAttribPointer(2, 2, GL_FLOAT, false, stride * sizeof(float), 6 * sizeof(float));

        glBindBuffer(GL_ARRAY_BUFFER, 0);
        glBindVertexArray(0);

        ShaderProgram.TryCreateShader("""
        #version 330 core
        layout (location = 0) in vec2 position;
        layout (location = 1) in vec4 color;
        layout (location = 2) in vec2 texCoord;

        out vec2 TexCoords;
        out vec4 Color;

        uniform mat4 projection;

        void main()
        {
            TexCoords = texCoord;
            Color = color;
            gl_Position = projection * vec4(position.xy, 0.0, 1.0);
        }
        """,
        """
        #version 330 core
        out vec4 FragColor;

        in vec2 TexCoords;
        in vec4 Color;

        uniform sampler2D msdfTexture;

        // Compute median of the four color channels
        float median(float r, float g, float b) {
            return max(min(r, g), min(max(r, g), b));
        }

        float screenPxRange() {
            vec2 unitRange = vec2(8)/vec2(textureSize(msdfTexture, 0));
            vec2 screenTexSize = vec2(1)/fwidth(TexCoords);
            return max(0.5*dot(unitRange, screenTexSize), 1.0);
        }

        void main()
        {
            float threshold = 1.0 - 0.6;

            vec3 msd = texture(msdfTexture, TexCoords).rgb;
            float sd = median(msd.r, msd.g, msd.b);
            float screenPxDistance = screenPxRange() * (sd - threshold);
            float opacity = clamp(screenPxDistance + threshold, 0.0, 1.0);
            FragColor = mix(vec4(Color.rgb, 0), Color, opacity);
        }
        """,
        out _shader, out string[] errors);

        System.Diagnostics.Debug.Assert(errors.Length == 0, string.Join("\n", errors));
    }

    public void PushFont(IFont font) => _fontStack.Push(font);
    public void PopFont() => _fontStack.Pop();
    public IFont PeekFont() => _fontStack.Peek();

    public void AddText(string text, Vector2 position, float scale, float rotation, ColorF color) =>
        AddText(_fontStack.Peek(), text, position, scale, rotation, color);

    public void AddText(string text, Vector2 position, float scale, float rotation, Func<char, int, Vector2> getCharOffset, Func<char, int, ColorF> getCharColor) =>
        AddText(_fontStack.Peek(), text, position, scale, rotation, getCharOffset, getCharColor);

    public void AddText(IFont font, string text, Vector2 position, float scale, float rotation, ColorF color) =>
        AddText(font, text, position, scale, rotation, (c, i) => Vector2.Zero, (c, i) => color);

    public void AddText(IFont font, string text, Vector2 position, float scale, float rotation, Func<char, int, Vector2> getCharOffset, Func<char, int, ColorF> getCharColor)
    {
        var advanceDirection = new Vector2(1, 0).RotateAround(Vector2.Zero, rotation);
        var verticalDirection = -1 * new Vector2(advanceDirection.Y, -advanceDirection.X);

        var cursorStart = position;
        var cursor = cursorStart;
        var scaledAdvance = advanceDirection * scale;

        for (int i = 0; i < text.Length; i++)
        {
            char character = text[i];
            var glyph = font.GetFontInfo().Glyphs.FirstOrDefault(x => x.Unicode == character);

            var charOffset = getCharOffset(character, i);
            var charColor = getCharColor(character, i);

            if (!char.IsWhiteSpace(character))
                AddGlyph(font, glyph, cursor + charOffset, scaledAdvance, charColor);

            cursor += advanceDirection * glyph.Advance * scale;
        }
    }

    private void AddGlyph(IFont font, FontInfo.FontInfoGlyph glyph, Vector2 cursor, Vector2 scaledAdvance, ColorF color)
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
            color,
            new Vector2(texelLeft, texelTop)
        );

        var vertexTopRight = new GlyphVertex(
            cursor + positionTop + positionRight,
            color,
            new Vector2(texelRight, texelTop)
        );

        var vertexBottomLeft = new GlyphVertex(
            cursor + positionBottom + positionLeft,
            color,
            new Vector2(texelLeft, texelBottom)
        );

        var vertexBottomRight = new GlyphVertex(
            cursor + positionBottom + positionRight,
            color,
            new Vector2(texelRight, texelBottom)
        );

        AddGlyphVertices(vertexTopLeft, vertexTopRight, vertexBottomLeft, vertexBottomRight);
    }

    private List<GlyphVertex> _glyphVertices = new();

    private void AddGlyphVertices(GlyphVertex topLeft, GlyphVertex topRight, GlyphVertex bottomLeft, GlyphVertex bottomRight)
    {
        _glyphVertices.Add(topLeft);
        _glyphVertices.Add(topRight);
        _glyphVertices.Add(bottomLeft);

        _glyphVertices.Add(bottomLeft);
        _glyphVertices.Add(topRight);
        _glyphVertices.Add(bottomRight);
    }

    public unsafe void FinalizeRender(IFont font, ICamera2D camera)
    {
        if (_glyphVertices.Count == 0)
            return;

        float[] vertexData = new float[_glyphVertices.Count * 8];

        for (int i = 0; i < _glyphVertices.Count; i++)
        {
            var vertex = _glyphVertices[i];
            vertexData[i * 8 + 0] = vertex.Position.X;
            vertexData[i * 8 + 1] = vertex.Position.Y;
            vertexData[i * 8 + 2] = vertex.Color.R;
            vertexData[i * 8 + 3] = vertex.Color.G;
            vertexData[i * 8 + 4] = vertex.Color.B;
            vertexData[i * 8 + 5] = vertex.Color.A;
            vertexData[i * 8 + 6] = vertex.UV.X;
            vertexData[i * 8 + 7] = vertex.UV.Y;
        }

        glBindVertexArray(_vao);
        glBindBuffer(GL_ARRAY_BUFFER, _vbo);

        fixed (float* ptr = &vertexData[0])
        {
            glBufferData(GL_ARRAY_BUFFER, vertexData.Length * sizeof(float), ptr, GL_DYNAMIC_DRAW);
        }

        _shader.UseWith(() =>
        {
            _shader.SetUniformMatrix4f("projection", false, camera.GetProjectionMatrix());
            _shader.SetUniform1i("msdfTexture", 0);

            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, font.GetSDFTexture2D().OpenGLID);
            glDrawArrays(GL_TRIANGLES, 0, _glyphVertices.Count);
        });

        glBindBuffer(GL_ARRAY_BUFFER, 0);
        glBindVertexArray(0);

        _glyphVertices.Clear();
    }
}
