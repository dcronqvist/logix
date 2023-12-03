namespace LogiX.Graphics.Textures;

public interface ITexture2D
{
    public uint OpenGLID { get; }
    public int Width { get; }
    public int Height { get; }

    byte[] GetPixelData();
}
