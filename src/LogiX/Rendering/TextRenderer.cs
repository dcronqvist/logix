using System.Numerics;
using LogiX.Graphics;
using static LogiX.OpenGL.GL;

namespace LogiX.Rendering;

public static class TextRenderer
{
    private static uint fontVAO;
    private static uint fontVBO;

    public static unsafe void InitGL()
    {
        // Create VAO
        fontVAO = glGenVertexArray();
        // Bind VAO
        glBindVertexArray(fontVAO);

        // Create VBO
        fontVBO = glGenBuffer();
        // BIND VBO
        glBindBuffer(GL_ARRAY_BUFFER, fontVBO);

        // Add data to VBO that is NULL, nothing
        glBufferData(GL_ARRAY_BUFFER, sizeof(float) * 6 * 8, NULL, GL_STREAM_DRAW);
        // Enable the data
        glEnableVertexAttribArray(0);
        glVertexAttribPointer(0, 4, GL_FLOAT, false, 8 * sizeof(float), (void*)0);
        glEnableVertexAttribArray(1);
        glVertexAttribPointer(1, 4, GL_FLOAT, false, 8 * sizeof(float), (void*)(4 * sizeof(float))); // YOU WERE WORKING ON THIS ONE, INCLUDING COLOR DATA IN
                                                                                                     // VBO INSTEAD OF UNIFORM 

        // Unbind VBO
        glBindBuffer(GL_ARRAY_BUFFER, 0);
        // Unbind VAO
        glBindVertexArray(0);
    }

    public static unsafe void RenderText(ShaderProgram shader, Font f, string s, Vector2 position, float scale, ColorF color, Camera2D cam, bool pixelAlign = true)
    {
        if (s.Length == 0)
        {
            return;
        }

        shader.Use(() =>
        {
            shader.SetMatrix4x4("projection", cam.GetProjectionMatrix());

            Matrix4x4 transPos = Matrix4x4.CreateTranslation(new Vector3(position, 0.0f));
            Matrix4x4 mscale = Matrix4x4.CreateScale(scale);

            shader.SetMatrix4x4("model", mscale * transPos);

            shader.SetInt("text", 0);
            shader.SetVec4("textColor", color.R, color.G, color.B, color.A);

            glActiveTexture(GL_TEXTURE0);
            glBindVertexArray(fontVAO);

            position = pixelAlign ? position.PixelAlign() : position;

            float x = position.X;
            float y = position.Y;

            float[] data = new float[6 * 8 * s.Length];

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];

                FontCharacter ch = f.Characters[c];

                float xPos = x + ch.Bearing.X * scale;
                float yPos = y + (f.MaxY - ch.Bearing.Y) * scale;

                float w = ch.Size.X * scale;
                float h = ch.Size.Y * scale;

                float uvXLeft = ch.Rectangle.X / f.AtlasWidth;
                float uvXRight = (ch.Rectangle.X + ch.Rectangle.Width) / f.AtlasWidth;
                float uvYTop = ch.Rectangle.Y / f.AtlasHeight;
                float uvYBottom = (ch.Rectangle.Y + ch.Rectangle.Height) / f.AtlasHeight;

                float[] verticesForCharacter = new float[]
                {
                    xPos + w, yPos, uvXRight, uvYTop, color.R, color.G, color.B, color.A, // top right
                    xPos, yPos, uvXLeft, uvYTop, color.R, color.G, color.B, color.A, // top left
                    xPos, yPos + h, uvXLeft, uvYBottom, color.R, color.G, color.B, color.A, // bottom left

                    xPos + w, yPos + h, uvXRight, uvYBottom, color.R, color.G, color.B, color.A, // bottom right
                    xPos + w, yPos, uvXRight, uvYTop, color.R, color.G, color.B, color.A, // top right
                    xPos, yPos + h, uvXLeft, uvYBottom, color.R, color.G, color.B, color.A, // bottom left
                };

                for (int j = 0; j < 8 * 6; j++)
                {
                    data[i * 8 * 6 + j] = verticesForCharacter[j];
                }

                x += ch.Advance * scale;
            }

            glBindBuffer(GL_ARRAY_BUFFER, fontVBO);

            fixed (float* vert = &data[0])
            {
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * data.Length, vert, GL_STREAM_DRAW);
            }

            glBindBuffer(GL_ARRAY_BUFFER, 0);

            glBindTexture(GL_TEXTURE_2D, f.TextureID);

            glDrawArrays(GL_TRIANGLES, 0, 6 * s.Length);
            glBindVertexArray(0);
            glBindTexture(GL_TEXTURE_2D, 0);
        });
    }
}