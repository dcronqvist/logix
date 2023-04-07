using LogiX.Content;
using LogiX.GLFW;
using StbImageSharp;
using Symphony;
using static LogiX.OpenGL.GL;

namespace LogiX.Graphics;

public class Texture2D : GLContentItem<ImageResult>
{
    public uint GLID { get; private set; }
    public int Width => this.Content.Width;
    public int Height => this.Content.Height;

    public Texture2D(IContentSource source, ImageResult content) : base(source, content)
    {

    }

    public Texture2D(int width, int height, byte[] data) : base(null, new ImageResult() { Width = width, Height = height, Data = data, Comp = ColorComponents.RedGreenBlueAlpha, SourceComp = ColorComponents.RedGreenBlueAlpha })
    {
        DisplayManager.LockedGLContext(() =>
        {
            this.InitGL(width, height, data);
        });
    }

    public byte[] GetPixelData()
    {
        return this.Content.Data;
    }

    public unsafe override void InitGL(ImageResult newContent)
    {
        this.InitGL(newContent.Width, newContent.Height, newContent.Data);
    }

    public unsafe void InitGL(int width, int height, byte[] pixelData)
    {
        // Create texture object
        uint id = glGenTexture();
        glBindTexture(GL_TEXTURE_2D, id);

        var wrapS = GL_CLAMP_TO_EDGE;
        var wrapT = GL_CLAMP_TO_EDGE;
        var minFilter = GL_LINEAR;
        var magFilter = GL_LINEAR;

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

    public unsafe Image GetAsGLFWImage()
    {
        byte[] pixelData = this.Content.Data;
        fixed (byte* pix = &pixelData[0])
        {
            IntPtr ip = new IntPtr(pix);
            return new Image(this.Content.Width, this.Content.Height, ip);
        }
    }

    public static Texture2D FromPixelData(int width, int height, ColorF[,] pixels)
    {
        byte[] pixelData = new byte[width * height * 4];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width + x) * 4;
                pixelData[index] = (byte)(pixels[x, y].R * 255);
                pixelData[index + 1] = (byte)(pixels[x, y].G * 255);
                pixelData[index + 2] = (byte)(pixels[x, y].B * 255);
                pixelData[index + 3] = (byte)(pixels[x, y].A * 255);
            }
        }

        return new Texture2D(width, height, pixelData);
    }
}