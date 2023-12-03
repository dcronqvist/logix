using System;
using System.Numerics;

namespace LogiX.Extensions;

public static class Vector2Extensions
{
    public static Vector2 RotateAround(this Vector2 v, Vector2 pivot, float angle)
    {
        Vector2 dir = v - pivot;
        return new Vector2(dir.X * MathF.Cos(angle) + dir.Y * MathF.Sin(angle),
                           -dir.X * MathF.Sin(angle) + dir.Y * MathF.Cos(angle)) + pivot;
    }
}
