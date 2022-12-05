using System.Numerics;

namespace LogiX.Architecture;

public class Vector2i
{
    public int X { get; set; }
    public int Y { get; set; }

    public static readonly Vector2i Zero = new Vector2i(0, 0);

    public Vector2i(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object obj)
    {
        if (obj is Vector2i pos)
        {
            return pos.X == X && pos.Y == Y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public Vector2 ToVector2(float scale = 1f)
    {
        return new Vector2(X * scale, Y * scale);
    }

    public static Vector2i operator +(Vector2i left, Vector2i right)
    {
        return new Vector2i(left.X + right.X, left.Y + right.Y);
    }

    public static Vector2i operator -(Vector2i left, Vector2i right)
    {
        return new Vector2i(left.X - right.X, left.Y - right.Y);
    }

    public static Vector2i operator -(Vector2i x)
    {
        return new Vector2i(-x.X, -x.Y);
    }

    public static Vector2i operator /(Vector2i v, int x)
    {
        return new Vector2i(v.X / x, v.Y / x);
    }

    public static Vector2i operator *(Vector2i v, int x)
    {
        return new Vector2i(v.X * x, v.Y * x);
    }

    public static bool operator ==(Vector2i left, Vector2i right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector2i left, Vector2i right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public float Length()
    {
        return (float)Math.Sqrt(X * X + Y * Y);
    }

    public Vector2 Normalized()
    {
        var length = Length();
        return new Vector2(X / length, Y / length);
    }
}