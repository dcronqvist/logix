using System;
using System.Drawing;
using System.Numerics;
using LogiX.Graphics.Cameras;
using LogiX.Graphics.Textures;
using static DotGL.GL;

namespace LogiX.Graphics.Rendering;

[Flags]
public enum TextureRenderEffects
{
    None = 1 << 0,
    FlipHorizontal = 1 << 1,
    FlipVertical = 1 << 2,
}

public class TextureRenderer
{
    private static uint quadVAO;
    private static uint quadVBO;

    public TextureRenderer()
    {
        InitGL();
    }

    public unsafe void Render(ShaderProgram shader, ITexture2D texture, Vector2 position, Vector2 scale, float rotation, ColorF color, Vector2 origin, RectangleF sourceRectangle, ICamera2D camera, TextureRenderEffects effects)
    {
        if (!IsShaderValid(shader, out var missingAttribs, out var missingUniforms))
        {
            throw new ArgumentException($"Shader is invalid! Missing attributes: {string.Join(", ", missingAttribs)}, missing uniforms: {string.Join(", ", missingUniforms)}");
        }

        shader.UseWith(() =>
        {
            Matrix4x4 modelMatrix = CreateModelMatrixFromPosition(position, rotation, origin, scale * new Vector2(sourceRectangle.Width, sourceRectangle.Height));

            shader.SetUniformMatrix4f("projection", false, camera.GetProjectionMatrix());
            shader.SetUniform4f("textureColor", color.R, color.G, color.B, color.A);
            shader.SetUniform1i("image", 0);
            shader.SetUniformfv("uvCoords", GetUVCoordinateData(texture.Width, texture.Height, sourceRectangle, effects));
            shader.SetUniformMatrix4f("model", false, modelMatrix);

            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, texture.OpenGLID);

            glBindVertexArray(quadVAO);
            glDrawArrays(GL_TRIANGLES, 0, 6);
            glBindVertexArray(0);
        });
    }

    private static Matrix4x4 CreateModelMatrixFromPosition(Vector2 position, float rotation, Vector2 origin, Vector2 scale)
    {
        Matrix4x4 translate = Matrix4x4.CreateTranslation(new Vector3(position, 0));
        Matrix4x4 rotate = Matrix4x4.CreateRotationZ(rotation);
        Matrix4x4 scaleM = Matrix4x4.CreateScale(new Vector3(scale, 0));
        Matrix4x4 originT = Matrix4x4.CreateTranslation(new Vector3(origin * scale, 0));
        Matrix4x4 originNeg = Matrix4x4.CreateTranslation(new Vector3(-origin * scale, 0));

        return scaleM * originNeg * rotate * originT * translate;
    }

    private static bool IsShaderValid(ShaderProgram shader, out string[] missingAttribs, out string[] missingUniforms)
    {
        bool hasAttribs = shader.HasAttribs(out missingAttribs, ("position", "vec2", 0));
        bool hasUniforms = shader.HasUniforms(out missingUniforms, ("projection", "mat4"), ("model", "mat4"));

        return hasAttribs && hasUniforms;
    }

    private static float[] GetUVCoordinateData(int textureWidth, int textureHeight, RectangleF rec, TextureRenderEffects effects)
    {
        float sourceX = rec.X / textureWidth;
        float sourceY = rec.Y / textureHeight;
        float sourceWidth = rec.Width / textureWidth;
        float sourceHeight = rec.Height / textureHeight;

        float[] data = {
            // tex
            sourceX, sourceY + sourceHeight, //downLeft
            sourceX + sourceWidth, sourceY, //topRight
            sourceX, sourceY, //topLeft

            sourceX, sourceY + sourceHeight, //downLeft
            sourceX + sourceWidth, sourceY + sourceHeight, //downRight
            sourceX + sourceWidth, sourceY  //topRight
        };

        if (effects.HasFlag(TextureRenderEffects.FlipHorizontal) && effects.HasFlag(TextureRenderEffects.FlipVertical))
        {
            data = new float[] {
                sourceX + sourceWidth, sourceY, // topRight
                sourceX, sourceY + sourceHeight, // downLeft
                sourceX + sourceWidth, sourceY + sourceHeight, // downRight

                sourceX + sourceWidth, sourceY, // topRight
                sourceX, sourceY, // topLeft
                sourceX, sourceY + sourceHeight, // downLeft
            };
        }
        else if (effects.HasFlag(TextureRenderEffects.FlipHorizontal))
        {
            data = new float[] {
                sourceX + sourceWidth, sourceY + sourceHeight, // downRight
                sourceX, sourceY, // topLeft
                sourceX + sourceWidth, sourceY, // topRight

                sourceX + sourceWidth, sourceY + sourceHeight, // downRight
                sourceX, sourceY + sourceHeight, // downLeft
                sourceX, sourceY, // topLeft
            };
        }
        else if (effects.HasFlag(TextureRenderEffects.FlipVertical))
        {
            data = new float[] {
                sourceX, sourceY, // topLeft
                sourceX + sourceWidth, sourceY + sourceHeight, // downRight
                sourceX, sourceY + sourceHeight, // downLeft

                sourceX, sourceY, // topLeft
                sourceX + sourceWidth, sourceY, // topRight
                sourceX + sourceWidth, sourceY + sourceHeight, // downRight
            };
        }

        return data;
    }

    private static unsafe void InitGL()
    {
        if (quadVAO != 0)
            return;

        // Configure VAO, VBO
        quadVAO = glGenVertexArray(); // Created vertex array object
        glBindVertexArray(quadVAO);

        quadVBO = glGenBuffer();

        float[] vertices = {
            // pos
            0.0f, 1.0f, // down left
            1.0f, 0.0f, // top right
            0.0f, 0.0f, // top left

            0.0f, 1.0f, // down left
            1.0f, 1.0f, // down right
            1.0f, 0.0f, // top right
        };

        glBindBuffer(GL_ARRAY_BUFFER, quadVBO);

        fixed (float* v = &vertices[0])
        {
            glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_STATIC_DRAW);
        }

        glEnableVertexAttribArray(0);
        glVertexAttribPointer(0, 2, GL_FLOAT, false, 2 * sizeof(float), (void*)0);

        glBindBuffer(GL_ARRAY_BUFFER, 0);
        glBindVertexArray(0);
    }
}
