using System.Drawing;
using System.Numerics;
using System.Reflection;
using LogiX.Architecture;
using LogiX.Graphics;

namespace LogiX;

public static class Utilities
{
    static Random RNG = new();

    public static void CopyPropsAndFields<T1, T2>(T1 a, ref T2 b)
    {
        var aProps = a.GetType().GetProperties();
        var bProps = b.GetType().GetProperties();
        var aFields = a.GetType().GetFields();
        var bFields = b.GetType().GetFields();

        foreach (var aProp in aProps)
        {
            var bProp = bProps.FirstOrDefault(x => x.Name == aProp.Name);
            if (bProp is not null)
            {
                bProp.SetValue(b, aProp.GetValue(a));
            }
        }

        foreach (var aField in aFields)
        {
            var bField = bFields.FirstOrDefault(x => x.Name == aField.Name);
            if (bField is not null)
            {
                bField.SetValue(b, aField.GetValue(a));
            }
        }
    }

    public static float[] GetMatrix4x4Values(Matrix4x4 m)
    {
        return new float[]
        {
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44
        };
    }

    public static Matrix4x4 CreateModelMatrixFromPosition(Vector2 position, float rotation, Vector2 origin, Vector2 scale)
    {
        Matrix4x4 translate = Matrix4x4.CreateTranslation(new Vector3(position, 0));
        Matrix4x4 rotate = Matrix4x4.CreateRotationZ(rotation);
        Matrix4x4 scaleM = Matrix4x4.CreateScale(new Vector3(scale, 0));
        Matrix4x4 originT = Matrix4x4.CreateTranslation(new Vector3(origin * scale, 0));
        Matrix4x4 originNeg = Matrix4x4.CreateTranslation(new Vector3(-origin * scale, 0));

        return scaleM * originNeg * rotate * originT * translate;
    }

    public static float Normalize(this float value, float min, float max)
    {
        return (value - min) / (max - min);
    }

    public static T[] Arrayify<T>(params T[] values)
    {
        return values;
    }

    public static Vector2 RotateAround(this Vector2 v, Vector2 pivot, float angle)
    {
        Vector2 dir = v - pivot;
        return new Vector2(dir.X * MathF.Cos(angle) + dir.Y * MathF.Sin(angle),
                           -dir.X * MathF.Sin(angle) + dir.Y * MathF.Cos(angle)) + pivot;
    }

    public static bool Contains(this RectangleF rect, Vector2 vec)
    {
        return rect.Contains(vec.X, vec.Y);
    }

    public static RectangleF Inflate(this RectangleF r, Vector2 v)
    {
        return new RectangleF(r.X - v.X, r.Y - v.Y, r.Width + v.X * 2, r.Height + v.Y * 2);
    }

    public static Rectangle Inflate(this RectangleF r, float f)
    {
        return new Rectangle((int)(r.X - f), (int)(r.Y - f), (int)(r.Width + f * 2), (int)(r.Height + f * 2));
    }

    public static RectangleF Inflate(this RectangleF r, float x, float y)
    {
        return new RectangleF(r.X - x, r.Y - y, r.Width + x * 2, r.Height + y * 2);
    }

    public static Vector2 PixelAlign(this Vector2 v)
    {
        return new Vector2(MathF.Round(v.X), MathF.Round(v.Y));
    }

    public static Vector2 GetMiddleOfString(this Font font, string text, float scale)
    {
        var size = font.MeasureString(text, scale);
        return new Vector2(size.X / 2, size.Y / 2);
    }

    public static Vector2 GetMiddleOfRectangle(this RectangleF rect)
    {
        return new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
    }

    public static ColorF Darken(this ColorF c, float amnt)
    {
        return ColorF.Darken(c, amnt);
    }

    public static RectangleF CreateRect(this Vector2 position, Vector2 size)
    {
        return new RectangleF(position.X, position.Y, size.X, size.Y);
    }

    public static float Clamp(float value, float min, float max)
    {
        return MathF.Min(MathF.Max(value, min), max);
    }

    public static string RepeatChar(this char c, int count)
    {
        return new string(c, count);
    }

    public static bool SameAs(LogicValue[] a, LogicValue[] b)
    {
        if (a.Length != b.Length)
            return false;

        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
                return false;
        }

        return true;
    }

    public static float CeilToMultipleOf(this float value, float multiple)
    {
        return MathF.Ceiling(value / multiple) * multiple;
    }

    public static T Choose<T>(params T[] values)
    {
        return values[RNG.Next(values.Length)];
    }

    public static ColorF GetValueColor(LogicValue value)
    {
        return value switch
        {
            LogicValue.LOW => ColorF.White,
            LogicValue.HIGH => ColorF.RoyalBlue,
            _ => ColorF.Gray
        };
    }

    public static ColorF GetValueColor(LogicValue[] values)
    {
        if (values.Length == 1)
            return GetValueColor(values[0]);

        var color = GetValueColor(values[0]);
        foreach (var value in values)
        {
            color = ColorF.Lerp(color, GetValueColor(value), 0.5f);
        }

        return color;
    }

    public static string GetAsHertzString(this int ticksPerSeconds)
    {
        if (ticksPerSeconds < 1000)
            return $"{ticksPerSeconds} Hz";
        else if (ticksPerSeconds < 1000000)
            return $"{Math.Round(ticksPerSeconds / 1000D, 1)} kHz";
        else
            return $"{Math.Round(ticksPerSeconds / 1000000D, 1)} MHz";
    }

    public static string GetAsHertzString(this float ticksPerSeconds)
    {
        if (ticksPerSeconds < 1000)
            return $"{Math.Round(ticksPerSeconds)} Hz";
        else if (ticksPerSeconds < 1000000)
            return $"{Math.Round(ticksPerSeconds / 1000D, 2)} kHz";
        else
            return $"{Math.Round(ticksPerSeconds / 1000000D, 2)} MHz";
    }

    public static IEnumerable<Type> FindDerivedTypesInAssembly(Assembly assembly, Type baseType)
    {
        return assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t));
    }

    public static IEnumerable<Type> FindDerivedTypes(Type baseType)
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(ass =>
        {
            return FindDerivedTypesInAssembly(ass, baseType);
        });
    }

    public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
    {
        while (toCheck != null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur)
            {
                return true;
            }
            toCheck = toCheck.BaseType;
        }
        return false;
    }

    public static Vector2i ToVector2i(this Vector2 worldPosition, int gridSize)
    {
        float x = worldPosition.X;
        float y = worldPosition.Y;
        int signX = Math.Sign(x);
        int signY = Math.Sign(y);

        x = Math.Abs(x);
        y = Math.Abs(y);

        int gridX = (int)MathF.Round(x / gridSize);
        int gridY = (int)MathF.Round(y / gridSize);

        return new Vector2i(gridX * signX, gridY * signY);
    }

    public static Vector2 GetClosestPoint(Vector2 point, params Vector2[] points)
    {
        Vector2 closest = points[0];
        float closestDist = (point - closest).Length();
        for (int i = 1; i < points.Length; i++)
        {
            float dist = (point - points[i]).Length();
            if (dist < closestDist)
            {
                closest = points[i];
                closestDist = dist;
            }
        }
        return closest;
    }

    public static Vector2i[] GetAllGridPointsBetween(Vector2i v1, Vector2i v2)
    {
        // Assume to be aligned on the x or y axis
        if (v1.X == v2.X)
        {
            int minY = Math.Min(v1.Y, v2.Y);
            int maxY = Math.Max(v1.Y, v2.Y);
            var points = new Vector2i[maxY - minY + 1];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Vector2i(v1.X, minY + i);
            }
            return points;
        }
        else if (v1.Y == v2.Y)
        {
            int minX = Math.Min(v1.X, v2.X);
            int maxX = Math.Max(v1.X, v2.X);
            var points = new Vector2i[maxX - minX + 1];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Vector2i(minX + i, v1.Y);
            }
            return points;
        }
        else
        {
            throw new Exception("Points are not aligned on the x or y axis");
        }
    }
}