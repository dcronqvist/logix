using System.Numerics;
using LogiX.Graphics;
using static LogiX.OpenGL.GL;

namespace LogiX.Rendering;

public static class BezierRenderer
{
    private static uint VAO;
    private static uint VBO;

    public unsafe static void InitGL()
    {
        VAO = glGenVertexArray();
        glBindVertexArray(VAO);

        VBO = glGenBuffer();
        glBindBuffer(GL_ARRAY_BUFFER, VBO);

        // Bezier curve from 3 v2 + v2 vertices
        // Fill VBO with empty data for now

        glEnableVertexAttribArray(0);
        glVertexAttribPointer(0, 2, GL_FLOAT, false, 4 * sizeof(float), (void*)0);

        glEnableVertexAttribArray(1);
        glVertexAttribPointer(1, 2, GL_FLOAT, false, 4 * sizeof(float), (void*)(sizeof(float) * 2));

        glBindBuffer(GL_ARRAY_BUFFER, 0);

        glBindVertexArray(0);
    }

    public unsafe static void RenderBezier(bool inner, Vector2 v1, Vector2 v2, Vector2 v3, Camera2D camera)
    {
        glBindVertexArray(VAO);

        glBindBuffer(GL_ARRAY_BUFFER, VBO);

        float[] vertices = {
            v1.X, v1.Y, 0f, 0f,
            v2.X, v2.Y, 0.5f, 0f,
            v3.X, v3.Y, 1f, 1f
        };

        fixed (float* v = &vertices[0])
        {
            glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_STREAM_DRAW);
        }

        var shader = Utilities.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.bezier");

        shader.Use(() =>
        {
            shader.SetMatrix4x4("projection", camera.GetProjectionMatrix());
            shader.SetBool("inner", inner);
            glDrawArrays(GL_TRIANGLES, 0, 3);
        });

        glBindBuffer(GL_ARRAY_BUFFER, 0);

        glBindVertexArray(0);
    }
}