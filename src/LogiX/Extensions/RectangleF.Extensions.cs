using System;
using System.Drawing;
using System.Numerics;

namespace LogiX.Extensions;

public static class RectangleFExtensions
{
    public static RectangleF InflateRect(this RectangleF rect, float width, float height)
    {
        float x = rect.X;
        float y = rect.Y;
        float w = rect.Width;
        float h = rect.Height;

        if (w > 0)
        {
            x -= width;
            w += width * 2;
        }
        else
        {
            x += width;
            w -= width * 2;
        }

        if (h > 0)
        {
            y -= height;
            h += height * 2;
        }
        else
        {
            y += height;
            h -= height * 2;
        }

        return new RectangleF(x, y, w, h);
    }

    public static RectangleF EnsurePositiveSize(this RectangleF rect)
    {
        float minX = MathF.Min(rect.Left, rect.Right);
        float minY = MathF.Min(rect.Top, rect.Bottom);
        float maxX = MathF.Max(rect.Left, rect.Right);
        float maxY = MathF.Max(rect.Top, rect.Bottom);

        return new RectangleF(minX, minY, maxX - minX, maxY - minY);
    }

    public static RectangleF CreateRectangleFromCornerPoints(Vector2 corner1, Vector2 corner2)
    {
        float minX = MathF.Min(corner1.X, corner2.X);
        float minY = MathF.Min(corner1.Y, corner2.Y);
        float maxX = MathF.Max(corner1.X, corner2.X);
        float maxY = MathF.Max(corner1.Y, corner2.Y);

        return new RectangleF(minX, minY, maxX - minX, maxY - minY);
    }

    public static bool Contains(this RectangleF rect, Vector2 position) => position.X >= rect.Left && position.X <= rect.Right && position.Y >= rect.Top && position.Y <= rect.Bottom;
}
