using System;
using System.Numerics;

namespace LogiX.Graphics;

public struct ColorF
{
    public float R { get; set; }

    public float G { get; set; }

    public float B { get; set; }

    public float A { get; set; }

    public ColorF()
    {
        R = 0;
        G = 0;
        B = 0;
        A = 1;
    }

    public ColorF(float r, float g, float b, float a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public ColorF(byte r, byte g, byte b, byte a)
    {
        R = r / 255f;
        G = g / 255f;
        B = b / 255f;
        A = a / 255f;
    }

    public ColorF(int r, int g, int b, int a)
    {
        R = (byte)r / 255f;
        G = (byte)g / 255f;
        B = (byte)b / 255f;
        A = (byte)a / 255f;
    }

    public ColorF(int hex)
    {
        R = ((hex & 0xFF0000) >> 16) / 255f;
        G = ((hex & 0x00FF00) >> 8) / 255f;
        B = (hex & 0x0000FF) / 255f;
        A = 1;
    }

    public ColorF(int hex, byte alpha)
    {
        R = ((hex & 0xFF0000) >> 16) / 255f;
        G = ((hex & 0x00FF00) >> 8) / 255f;
        B = (hex & 0x0000FF) / 255f;
        A = alpha / 255f;
    }

    public static ColorF operator *(ColorF left, float right) => new(left.R * right, left.G * right, left.B * right, left.A * right);

    public readonly float[] ToFloatArray() => [R, G, B, A];

    public readonly Vector4 ToVector4() => new(R, G, B, A);

    public static ColorF Lerp(ColorF from, ColorF to, float amt) => new(
        from.R + ((to.R - from.R) * amt),
        from.G + ((to.G - from.G) * amt),
        from.B + ((to.B - from.B) * amt),
        from.A + ((to.A - from.A) * amt)
    );

    public static ColorF FromString(string s)
    {
        int i = Convert.ToInt32(s, 16);
        return new ColorF(i);
    }

    public static ColorF Darken(ColorF c, float f) => new(c.R * f, c.G * f, c.B * f, c.A);

    public static ColorF operator +(ColorF left, ColorF right) => new(
        left.R + right.R,
        left.G + right.G,
        left.B + right.B,
        left.A + right.A
    );

    public static ColorF FromHue(float hue) => new(
        (MathF.Sin(hue) / 2f) + 0.5f,
        (MathF.Sin(hue + 2f) / 2f) + 0.5f,
        (MathF.Sin(hue + 4f) / 2f) + 0.5f,
        1f
    );

    public static ColorF White => new(1f, 1f, 1f, 1f);
    public static ColorF Black => new(0, 0, 0, 1f);
    public static ColorF Gray => new(0.5f, 0.5f, 0.5f, 1f);
    public static ColorF LightGray => new(0.75f, 0.75f, 0.75f, 1f);
    public static ColorF DarkGray => new(0.25f, 0.25f, 0.25f, 1f);
    public static ColorF Transparent => new(0, 0, 0, 0);
    public static ColorF PearlGray => new(0xf6f5f5);
    public static ColorF BlueGray => new(0x145374);
    public static ColorF Beige => new(0xf5f5dc);
    public static ColorF DeepBlue => new(0x00334e);
    public static ColorF Shrimp => new(0xee6f57);
    public static ColorF Orange => new(0xFA8601);
    public static ColorF RoyalBlue => new(0x4876ff);
    public static ColorF BrightBlue => new(0x0a86ea);
    public static ColorF DarkGoldenRod => new(0xffb90f);
    public static ColorF Red => new(0xff0000);
    public static ColorF Green => new(0x00ff00);
    public static ColorF LimeGreen => new(0x32cd32);
    public static ColorF SoftGreen => new(0x00d200);
    public static ColorF Blue => new(0x0000ff);
    public static ColorF Yellow => new(0xffff00);
    public static ColorF Cyan => new(0x00ffff);
    public static ColorF Magenta => new(0xff00ff);
    public static ColorF Silver => new(0xc0c0c0);
    public static ColorF Maroon => new(0x800000);
    public static ColorF Olive => new(0x808000);
    public static ColorF Purple => new(0x800080);
    public static ColorF Teal => new(0x008080);
    public static ColorF Navy => new(0x000080);
    public static ColorF Brown => new(0xa52a2a);
    public static ColorF Pink => new(0xffc0cb);
    public static ColorF Peach => new(0xffdab9);
    public static ColorF Coral => new(0xff7f50);
    public static ColorF Salmon => new(0xfa8072);
    public static ColorF Tomato => new(0xff6347);
    public static ColorF CoralRed => new(0xff4040);
    public static ColorF Crimson => new(0xdc143c);
    public static ColorF RedOrange => new(0xff4500);
    public static ColorF RedViolet => new(0x8b008b);
    public static ColorF DeepPink => new(0xff1493);
    public static ColorF HotPink => new(0xff69b4);
    public static ColorF DeepSkyBlue => new(0x00bfff);
    public static ColorF DodgerBlue => new(0x1e90ff);
    public static ColorF LightBlue => new(0xadd8e6);
    public static ColorF LightSkyBlue => new(0x87cefa);
    public static ColorF LightSteelBlue => new(0xb0c4de);
    public static ColorF LightSlateGray => new(0x778899);
    public static ColorF LightCyan => new(0xe0ffff);
    public static ColorF PaleTurquoise => new(0xafeeee);
    public static ColorF PaleGreen => new(0x98fb98);
}
