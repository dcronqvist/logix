using System.Drawing;
using System.Numerics;

namespace LogiX.Graphics.Cameras;

public interface ICamera2D
{
    Vector2 FocusPosition { get; }
    RectangleF VisibleArea { get; }
    Matrix4x4 GetProjectionMatrix();
}
