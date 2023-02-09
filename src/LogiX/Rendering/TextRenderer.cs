using System.Numerics;
using LogiX.Graphics;
using System.Drawing;
using static LogiX.OpenGL.GL;
using Cyotek.Drawing.BitmapFont;

namespace LogiX.Rendering;

public class CharacterInstance
{
    public Font Font { get; set; }
    public Vector2 Position { get; set; }
    public RectangleF UVRectangle { get; set; }
    public char Character { get; set; }
    public Matrix4x4 ModelMatrix { get; set; }
    public float Height { get; set; }

    public float Thickness { get; set; }
    public float Softness { get; set; }
    public ColorF Color { get; set; }
    public ColorF OutlineColor { get; set; }
    public float OutlineThickness { get; set; }
    public float OutlineSoftness { get; set; }

    public float[] GetData()
    {
        var x = this.Position.X;
        var y = this.Position.Y;

        Character ch = Font.Content.BitmapFontInfo.Characters[this.Character];

        float xPos = x + ch.XOffset;
        float yPos = y + ch.YOffset;

        float w = ch.Width;
        float h = ch.Height;

        float uvXLeft = ch.X / (float)Font.AtlasWidth;
        float uvXRight = (ch.X + ch.Width) / (float)Font.AtlasWidth;
        float uvYTop = ch.Y / (float)Font.AtlasHeight;
        float uvYBottom = (ch.Y + ch.Height) / (float)Font.AtlasHeight;

        var data = new float[40 * 2]; // 2 tris

        float[] firstTri = new float[]
        {
            xPos + w, yPos, uvXRight, uvYTop,
            xPos, yPos, uvXLeft, uvYTop,
            xPos, yPos + h, uvXLeft, uvYBottom,

            // xPos + w, yPos + h, uvXRight, uvYBottom, this.Color.R, this.Color.G, this.Color.B, this.Color.A, // bottom right
            // xPos + w, yPos, uvXRight, uvYTop, color.R, color.G, color.B, color.A, // top right
            // xPos, yPos + h, uvXLeft, uvYBottom, color.R, color.G, color.B, color.A, // bottom left
        };

        for (int j = 0; j < 12; j++) // 3 * 4, (x, y, u, v) * 3
        {
            data[j] = firstTri[j];
        }

        var modelMatrixData = Utilities.GetMatrix4x4Values(this.ModelMatrix);
        for (int j = 0; j < 16; j++)
        {
            data[j + 12] = modelMatrixData[j];
        }

        data[28] = this.Thickness;
        data[29] = this.Softness;
        data[30] = this.Color.R;
        data[31] = this.Color.G;
        data[32] = this.Color.B;
        data[33] = this.Color.A;
        data[34] = this.OutlineColor.R;
        data[35] = this.OutlineColor.G;
        data[36] = this.OutlineColor.B;
        data[37] = this.OutlineColor.A;
        data[38] = this.OutlineThickness;
        data[39] = this.OutlineSoftness;

        float[] secondTri = new float[]
        {
            xPos + w, yPos + h, uvXRight, uvYBottom,
            xPos + w, yPos, uvXRight, uvYTop,
            xPos, yPos + h, uvXLeft, uvYBottom,
        };

        for (int j = 0; j < 4 * 3; j++)
        {
            data[j + 40] = secondTri[j];
        }

        for (int j = 0; j < 16; j++)
        {
            data[j + 52] = modelMatrixData[j];
        }

        data[68] = this.Thickness;
        data[69] = this.Softness;
        data[70] = this.Color.R;
        data[71] = this.Color.G;
        data[72] = this.Color.B;
        data[73] = this.Color.A;
        data[74] = this.OutlineColor.R;
        data[75] = this.OutlineColor.G;
        data[76] = this.OutlineColor.B;
        data[77] = this.OutlineColor.A;
        data[78] = this.OutlineThickness;
        data[79] = this.OutlineSoftness;

        return data;
    }
}

public static class TextRenderer
{
    private static uint vao;
    private static uint vbo;

    private static List<CharacterInstance> _instances = new();

    public static unsafe void InitGL()
    {
        // VBO should contain each character instance
        // x1, y1, u1, v1
        // x2, y2, u2, v2
        // x3, y3, u3, v3
        // r,  g,  b,  a
        // model matrix, 16 floats 
        // smoothness (float) = 33 floats per character instance

        // Create VAO
        vao = glGenVertexArray();

        // Bind VAO
        glBindVertexArray(vao);

        // Create VBO
        vbo = glGenBuffer();
        // BIND VBO
        glBindBuffer(GL_ARRAY_BUFFER, vbo);
        glBufferData(GL_ARRAY_BUFFER, 0, (void*)0, GL_STREAM_DRAW); // Fill VBO with 0 bytes to initialize memory

        int stride = 40;

        // Enable vertex attributes
        glEnableVertexAttribArray(0); // x1, y1, u1, v1
        glVertexAttribPointer(0, 4, GL_FLOAT, false, stride * sizeof(float), (void*)0);
        glVertexAttribDivisor(0, 1);

        glEnableVertexAttribArray(1); // x2, y2, u2, v2
        glVertexAttribPointer(1, 4, GL_FLOAT, false, stride * sizeof(float), (void*)(4 * sizeof(float)));
        glVertexAttribDivisor(1, 1);

        glEnableVertexAttribArray(2); // x3, y3, u3, v3
        glVertexAttribPointer(2, 4, GL_FLOAT, false, stride * sizeof(float), (void*)(8 * sizeof(float)));
        glVertexAttribDivisor(2, 1);

        // 16 floats of model matrix
        glEnableVertexAttribArray(3); // model matrix
        glVertexAttribPointer(3, 4, GL_FLOAT, false, stride * sizeof(float), (void*)(12 * sizeof(float)));
        glVertexAttribDivisor(3, 1);

        glEnableVertexAttribArray(4); // model matrix
        glVertexAttribPointer(4, 4, GL_FLOAT, false, stride * sizeof(float), (void*)(16 * sizeof(float)));
        glVertexAttribDivisor(4, 1);

        glEnableVertexAttribArray(5); // model matrix
        glVertexAttribPointer(5, 4, GL_FLOAT, false, stride * sizeof(float), (void*)(20 * sizeof(float)));
        glVertexAttribDivisor(5, 1);

        glEnableVertexAttribArray(6); // model matrix
        glVertexAttribPointer(6, 4, GL_FLOAT, false, stride * sizeof(float), (void*)(24 * sizeof(float)));
        glVertexAttribDivisor(6, 1);

        // public float Thickness { get; set; }
        glEnableVertexAttribArray(7);
        glVertexAttribPointer(7, 1, GL_FLOAT, false, stride * sizeof(float), (void*)(28 * sizeof(float)));
        glVertexAttribDivisor(7, 1);

        // public float Softness { get; set; }
        glEnableVertexAttribArray(8);
        glVertexAttribPointer(8, 1, GL_FLOAT, false, stride * sizeof(float), (void*)(29 * sizeof(float)));
        glVertexAttribDivisor(8, 1);

        // public ColorF Color { get; set; }
        glEnableVertexAttribArray(9);
        glVertexAttribPointer(9, 4, GL_FLOAT, false, stride * sizeof(float), (void*)(30 * sizeof(float)));
        glVertexAttribDivisor(9, 1);

        // public ColorF OutlineColor { get; set; }
        glEnableVertexAttribArray(10);
        glVertexAttribPointer(10, 4, GL_FLOAT, false, stride * sizeof(float), (void*)(34 * sizeof(float)));
        glVertexAttribDivisor(10, 1);

        // public float OutlineThickness { get; set; }
        glEnableVertexAttribArray(11);
        glVertexAttribPointer(11, 1, GL_FLOAT, false, stride * sizeof(float), (void*)(38 * sizeof(float)));
        glVertexAttribDivisor(11, 1);

        // public float OutlineSoftness { get; set; }
        glEnableVertexAttribArray(12);
        glVertexAttribPointer(12, 1, GL_FLOAT, false, stride * sizeof(float), (void*)(39 * sizeof(float)));
        glVertexAttribDivisor(12, 1);

        // Unbind VBO
        glBindBuffer(GL_ARRAY_BUFFER, 0);
        // Unbind VAO
        glBindVertexArray(0);
    }

    // Write a method that takes a string as input and outputs a list of character instances
    public static void AddCharacterInstances(string text, Font font, Vector2 position, float scale, float rotation, float thickness, float softness, ColorF color, float outlineThickness, float outlineSoftness, ColorF outlineColor, float fixedAdvance, float fixedHeight)
    {
        List<CharacterInstance> characterInstances = new();

        var height = fixedHeight == -1f ? font.MaxY : fixedHeight;

        var pos = position;

        // Loop through each character in the string
        for (int i = 0; i < text.Length; i++)
        {
            // Get the character
            char character = text[i];

            // Get the character's glyph
            Character glyph = font.Content.BitmapFontInfo.Characters[character];

            var advance = fixedAdvance == -1f ? glyph.XAdvance + glyph.XOffset : fixedAdvance;

            // public float Thickness { get; set; } // 
            // public float Softness { get; set; }
            // public ColorF Color { get; set; }
            // public ColorF OutlineColor { get; set; }
            // public float OutlineThickness { get; set; }
            // public float OutlineSoftness { get; set; }

            // Create a character instance
            CharacterInstance characterInstance = new()
            {
                Font = font,
                Position = Vector2.Zero,
                UVRectangle = new RectangleF(glyph.X, glyph.Y, glyph.Width, glyph.Height),
                Character = character,
                ModelMatrix = Utilities.CreateModelMatrixFromPosition(pos.PixelAlign(), rotation, new Vector2(0, 0), new Vector2(scale, scale)),
                Height = height,

                Color = color,
                Thickness = thickness,
                Softness = softness,
                OutlineColor = outlineColor,
                OutlineThickness = outlineThickness,
                OutlineSoftness = outlineSoftness
            };

            // Add the character instance to the list
            characterInstances.Add(characterInstance);

            // Move the position to the next character
            pos.X += MathF.Cos(rotation) * advance * scale;
            pos.Y += MathF.Sin(rotation) * advance * scale;
        }

        // Add the character instances to the list of instances
        _instances.AddRange(characterInstances);
    }

    private static List<(Font, CharacterInstance[])> GetInstancesForFonts()
    {
        return _instances.GroupBy(x => x.Font).Select(x => (x.Key, x.ToArray())).ToList();
    }

    public static unsafe void FinalizeRender(ShaderProgram shader, Camera2D camera)
    {
        glBindVertexArray(vao);
        glBindBuffer(GL_ARRAY_BUFFER, vbo);

        var fontInstances = GetInstancesForFonts();

        foreach (var (font, instances) in fontInstances)
        {
            var instanceData = new List<float>();
            foreach (var instance in instances)
            {
                instanceData.AddRange(instance.GetData());
            }

            var data = instanceData.ToArray();

            fixed (float* ptr = data)
            {
                glBufferData(GL_ARRAY_BUFFER, data.Length * sizeof(float), (void*)ptr, GL_STREAM_DRAW);
            }

            shader.Use(() =>
            {
                shader.SetMatrix4x4("projection", camera.GetProjectionMatrix());
                shader.SetInt("text", 0);
                glActiveTexture(GL_TEXTURE0);
                glBindTexture(GL_TEXTURE_2D, font.TextureID);
                glDrawArraysInstanced(GL_TRIANGLES, 0, 3, _instances.Count * 2);
            });
        }

        glBindBuffer(GL_ARRAY_BUFFER, 0);
        glBindVertexArray(0);

        _instances.Clear();
    }

    public static unsafe void RenderText(Font f, string s, Vector2 position, float scale, float rotation, bool pixelAlign, float thickness, float softness, ColorF color, float outlineThickness, float outlineSoftness, ColorF outlineColor, float fixedAdvance, float fixedHeight)
    {
        AddCharacterInstances(s, f, pixelAlign ? position.PixelAlign() : position, scale, rotation, thickness, softness, color, outlineThickness, outlineSoftness, outlineColor, fixedAdvance, fixedHeight);
    }

    public static unsafe void RenderText(Font f, string s, Vector2 position, float scale, float rotation, float thickness, float softness, ColorF color)
    {
        RenderText(f, s, position, scale, rotation, true, thickness, softness, color, 0, 0, ColorF.Transparent, -1, -1);
    }

    // private static unsafe void RenderTextInternal(ShaderProgram shader, Font f, string s, Vector2 position, float scale, float rotation, ColorF color, Camera2D cam, bool pixelAlign = true)
    // {
    //     if (s.Length == 0)
    //     {
    //         return;
    //     }

    //     shader.Use(() =>
    //     {
    //         shader.SetMatrix4x4("projection", cam.GetProjectionMatrix());

    //         var origin = Vector2.Zero;
    //         var model = Utilities.CreateModelMatrixFromPosition(position, rotation, origin, new Vector2(scale, scale));

    //         shader.SetMatrix4x4("model", model);

    //         shader.SetInt("text", 0);
    //         shader.SetVec4("textColor", color.R, color.G, color.B, color.A);

    //         glActiveTexture(GL_TEXTURE0);
    //         glBindVertexArray(fontVAO);

    //         position = pixelAlign ? position.PixelAlign() : position;

    //         float x = 0;
    //         float y = 0;

    //         float[] data = new float[6 * 8 * s.Length];

    //         for (int i = 0; i < s.Length; i++)
    //         {
    //             char c = s[i];

    //             FontCharacter ch = f.Characters[c];

    //             float xPos = x + ch.Bearing.X;
    //             float yPos = y + (f.MaxY - ch.Bearing.Y);

    //             float w = ch.Size.X;
    //             float h = ch.Size.Y;

    //             float uvXLeft = ch.Rectangle.X / f.AtlasWidth;
    //             float uvXRight = (ch.Rectangle.X + ch.Rectangle.Width) / f.AtlasWidth;
    //             float uvYTop = ch.Rectangle.Y / f.AtlasHeight;
    //             float uvYBottom = (ch.Rectangle.Y + ch.Rectangle.Height) / f.AtlasHeight;

    //             float[] verticesForCharacter = new float[]
    //             {
    //                 xPos + w, yPos, uvXRight, uvYTop, color.R, color.G, color.B, color.A, // top right
    //                 xPos, yPos, uvXLeft, uvYTop, color.R, color.G, color.B, color.A, // top left
    //                 xPos, yPos + h, uvXLeft, uvYBottom, color.R, color.G, color.B, color.A, // bottom left

    //                 xPos + w, yPos + h, uvXRight, uvYBottom, color.R, color.G, color.B, color.A, // bottom right
    //                 xPos + w, yPos, uvXRight, uvYTop, color.R, color.G, color.B, color.A, // top right
    //                 xPos, yPos + h, uvXLeft, uvYBottom, color.R, color.G, color.B, color.A, // bottom left
    //             };

    //             for (int j = 0; j < 8 * 6; j++)
    //             {
    //                 data[i * 8 * 6 + j] = verticesForCharacter[j];
    //             }

    //             x += ch.Advance;
    //         }

    //         glBindBuffer(GL_ARRAY_BUFFER, fontVBO);

    //         fixed (float* vert = &data[0])
    //         {
    //             glBufferData(GL_ARRAY_BUFFER, sizeof(float) * data.Length, vert, GL_STREAM_DRAW);
    //         }

    //         glBindBuffer(GL_ARRAY_BUFFER, 0);

    //         glBindTexture(GL_TEXTURE_2D, f.TextureID);

    //         glDrawArrays(GL_TRIANGLES, 0, 6 * s.Length);
    //         glBindVertexArray(0);
    //         glBindTexture(GL_TEXTURE_2D, 0);
    //     });
    // }
}