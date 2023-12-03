using System.Diagnostics.CodeAnalysis;
using DotGLFW;
using Symphony;
using static DotGL.GL;

namespace LogiX.Graphics.Textures;

public class Texture2D : Content<Texture2D>, ITexture2D
{
    public uint OpenGLID { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public static bool TryCreateTexture2D(
        int width,
        int height,
        int minFilter,
        int magFilter,
        int wrapS,
        int wrapT,
        int format,
        byte[] imageData,
        out Texture2D texture,
        out string error)
    {
        uint id = glGenTexture();
        glBindTexture(GL_TEXTURE_2D, id);

        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, minFilter);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, magFilter);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, wrapS);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, wrapT);

        glTexImage2D(GL_TEXTURE_2D, 0, format, width, height, 0, format, GL_UNSIGNED_BYTE, imageData);

        if (minFilter == GL_NEAREST_MIPMAP_NEAREST ||
            minFilter == GL_NEAREST_MIPMAP_LINEAR ||
            minFilter == GL_LINEAR_MIPMAP_NEAREST ||
            minFilter == GL_LINEAR_MIPMAP_LINEAR ||
            magFilter == GL_NEAREST_MIPMAP_NEAREST ||
            magFilter == GL_NEAREST_MIPMAP_LINEAR ||
            magFilter == GL_LINEAR_MIPMAP_NEAREST ||
            magFilter == GL_LINEAR_MIPMAP_LINEAR)
        {
            glGenerateMipmap(GL_TEXTURE_2D);
        }

        glBindTexture(GL_TEXTURE_2D, 0);

        texture = new Texture2D
        {
            OpenGLID = id,
            Width = width,
            Height = height
        };

        error = null;
        return true;
    }

    protected override void OnContentUpdated(Texture2D newContent)
    {
        this.OpenGLID = newContent.OpenGLID;
        this.Width = newContent.Width;
        this.Height = newContent.Height;
    }

    public override void Unload()
    {
        glDeleteTextures(OpenGLID);
    }

    public byte[] GetPixelData()
    {
        glBindTexture(GL_TEXTURE_2D, OpenGLID);
        byte[] data = new byte[Width * Height * 4];
        glGetTexImage(GL_TEXTURE_2D, 0, GL_RGBA, GL_UNSIGNED_BYTE, ref data);
        glBindTexture(GL_TEXTURE_2D, 0);
        return data;
    }
}
