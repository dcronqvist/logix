using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using GoodGame.Graphics;
using static GoodGame.OpenGL.GL;

namespace GoodGame.Rendering;

[Flags]
public enum TextureRenderEffects
{
    None = 1 << 0,
    FlipHorizontal = 1 << 1,
    FlipVertical = 1 << 2,
}

public static class TextureRenderer
{
    private static uint quadVAO;
    private static uint quadVBO;
    public static RectangleF currentSourceRectangle;

    static TextureRenderer()
    {

    }

    public static void Render(ShaderProgram shader, Texture2D texture, Vector2 position, Vector2 scale, float rotation, ColorF color, Camera2D camera, TextureRenderEffects effects = TextureRenderEffects.None)
    {
        if (texture != null)
            Render(shader, texture, position, scale, rotation, color, Vector2.Zero, new RectangleF(0, 0, texture.Width, texture.Height), camera, effects);
    }

    public static void Render(ShaderProgram shader, Texture2D texture, Vector2 position, Vector2 scale, float rotation, ColorF color, Vector2 origin, Camera2D camera, TextureRenderEffects effects = TextureRenderEffects.None)
    {
        if (texture != null)
            Render(shader, texture, position, scale, rotation, color, origin, new Rectangle(0, 0, texture.Width, texture.Height), camera, effects);
    }

    public static unsafe void Render(ShaderProgram shader, Texture2D texture, Vector2 position, Vector2 scale, float rotation, ColorF color, Vector2 origin, RectangleF sourceRectangle, Camera2D camera, TextureRenderEffects effects)
    {
        if (!IsShaderValid(shader, out var missingAttribs, out var missingUniforms))
        {
            throw new ArgumentException($"Shader is invalid! Missing attributes: {string.Join(", ", missingAttribs)}, missing uniforms: {string.Join(", ", missingUniforms)}");
        }

        shader.Use(() =>
        {
            Matrix4x4 modelMatrix = Utilities.CreateModelMatrixFromPosition(position, rotation, origin, scale * new Vector2(sourceRectangle.Width, sourceRectangle.Height));

            shader.SetMatrix4x4("projection", camera.GetProjectionMatrix());
            shader.SetVec4("textureColor", color.R, color.G, color.B, color.A);
            shader.SetInt("image", 0);
            shader.SetFloatArray("uvCoords", GetUVCoordinateData(texture.Width, texture.Height, sourceRectangle, effects));
            shader.SetMatrix4x4("model", modelMatrix);

            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, texture.GLID);

            glBindVertexArray(quadVAO);
            glDrawArrays(GL_TRIANGLES, 0, 6);
            glBindVertexArray(0);
        });
    }

    private static bool IsShaderValid(ShaderProgram shader, out string[] missingAttribs, out string[] missingUniforms)
    {
        bool hasAttribs = shader.HasAttribs(out missingAttribs, ("position", "vec2", 0));
        bool hasUniforms = shader.HasUniforms(out missingUniforms, ("projection", "mat4"), ("model", "mat4"));

        return hasAttribs && hasUniforms;
    }

    private static float[] GetUVCoordinateData(int textureWith, int textureHeight, RectangleF rec, TextureRenderEffects effects)
    {
        float sourceX = rec.X / textureWith;
        float sourceY = rec.Y / textureHeight;
        float sourceWidth = rec.Width / textureWith;
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

    public static unsafe void InitGL()
    {
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
