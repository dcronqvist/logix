using System.Numerics;
using LogiX.Graphics.Textures;

namespace LogiX.Graphics.Text;

public interface IFont
{
    FontInfo GetFontInfo();
    ITexture2D GetSDFTexture2D();
    Vector2 MeasureString(string text);
    byte[] GetTTFBytes();
}
