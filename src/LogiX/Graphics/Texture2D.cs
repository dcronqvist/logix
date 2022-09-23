using GoodGame.Content;
using StbImageSharp;
using Symphony;
using static GoodGame.OpenGL.GL;

namespace GoodGame.Graphics;

public class Texture2D : GLContentItem<ImageResult>
{
    public uint GLID { get; private set; }
    public int Width => this.Content.Width;
    public int Height => this.Content.Height;

    public Texture2D(string identifier, IContentSource source, ImageResult content) : base(identifier, source, content)
    {

    }

    public byte[] GetPixelData()
    {
        return this.Content.Data;
    }

    public unsafe override void InitGL(ImageResult newContent)
    {
        // Create texture object
        uint id = glGenTexture();
        glBindTexture(GL_TEXTURE_2D, id);

        var pixelData = newContent.Data;
        var width = newContent.Width;
        var height = newContent.Height;

        var wrapS = GL_CLAMP_TO_EDGE;
        var wrapT = GL_CLAMP_TO_EDGE;
        var minFilter = GL_NEAREST;
        var magFilter = GL_NEAREST;

        // Set texture data
        fixed (byte* pix = &pixelData[0])
        {
            glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, pix);
        }

        // Set a bunch of texture parameters
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, wrapS);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, wrapT);

        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, minFilter);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, magFilter);

        // Generate mip maps
        //glGenerateMipmap(GL_TEXTURE_2D);

        // Done! unbind
        glBindTexture(GL_TEXTURE_2D, 0);

        this.GLID = id;
    }

    public override void DestroyGL()
    {
        if (this.GLID != 0)
        {
            glDeleteTexture(this.GLID);
            this.GLID = 0;
        }
    }

    public override bool IsGLInitialized()
    {
        return this.GLID != 0;
    }
}