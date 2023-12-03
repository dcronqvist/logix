using System;
using System.Numerics;
using NLua;

namespace LogiX.Graphics;

public struct Vector2i(int x, int y)
{
    [LuaMember(Name = "x")]
    public int X { get; set; } = x;

    [LuaMember(Name = "y")]
    public int Y { get; set; } = y;

    public static Vector2i Zero => new(0, 0);
    public static Vector2i One => new(1, 1);
    public static Vector2i UnitX => new(1, 0);
    public static Vector2i UnitY => new(0, 1);

    public static implicit operator Vector2(Vector2i v) => new(v.X, v.Y);
    public static implicit operator Vector2i(Vector2 v) => new((int)v.X, (int)v.Y);

    public static Vector2i operator -(Vector2i v) => new(-v.X, -v.Y);

    public static Vector2i operator +(Vector2i a, Vector2i b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2i operator -(Vector2i a, Vector2i b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2i operator *(Vector2i a, Vector2i b) => new(a.X * b.X, a.Y * b.Y);
    public static Vector2i operator /(Vector2i a, Vector2i b) => new(a.X / b.X, a.Y / b.Y);

    public static Vector2i operator +(Vector2i a, int b) => new(a.X + b, a.Y + b);
    public static Vector2i operator -(Vector2i a, int b) => new(a.X - b, a.Y - b);
    public static Vector2i operator *(Vector2i a, int b) => new(a.X * b, a.Y * b);
    public static Vector2i operator /(Vector2i a, int b) => new(a.X / b, a.Y / b);

    public static Vector2 operator +(Vector2i a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2i a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator *(Vector2i a, Vector2 b) => new(a.X * b.X, a.Y * b.Y);
    public static Vector2 operator /(Vector2i a, Vector2 b) => new(a.X / b.X, a.Y / b.Y);

    public static bool operator ==(Vector2i a, Vector2i b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Vector2i a, Vector2i b) => !(a == b);

    public static Vector2i RotateAround(Vector2i v, Vector2i pivot, int multipleOfPiOver2)
    {
        float angle = multipleOfPiOver2 * MathF.PI / 2f;
        Vector2 dir = v - pivot;
        return new Vector2i((int)MathF.Round((dir.X * MathF.Cos(angle)) + (dir.Y * MathF.Sin(angle))),
                           (int)MathF.Round((-dir.X * MathF.Sin(angle)) + (dir.Y * MathF.Cos(angle)))) + pivot;
    }

    public readonly float LengthSquared() => (X * X) + (Y * Y);
    public readonly float Length() => MathF.Sqrt(LengthSquared());
    public readonly Vector2 Normalize() => Vector2.Normalize(this);

    public override readonly string ToString() => $"({X}, {Y})";
}
