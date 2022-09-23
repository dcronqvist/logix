using System.Drawing;
using System.Numerics;
using LogiX.Graphics;

namespace LogiX;

public static class Utilities
{
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

    public static float Clamp(float value, float min, float max)
    {
        return MathF.Min(MathF.Max(value, min), max);
    }

    public static string RepeatChar(this char c, int count)
    {
        return new string(c, count);
    }
}