using System;

namespace LogiX.Graphics;

public struct ColorF
{
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }

    public ColorF(float r, float g, float b, float a)
    {
        this.R = r;
        this.G = g;
        this.B = b;
        this.A = a;
    }

    public ColorF(byte r, byte g, byte b, byte a)
    {
        this.R = r / 255f;
        this.G = g / 255f;
        this.B = b / 255f;
        this.A = a / 255f;
    }

    public ColorF(int hex)
    {
        this.R = ((hex & 0xFF0000) >> 16) / 255f;
        this.G = ((hex & 0x00FF00) >> 8) / 255f;
        this.B = (hex & 0x0000FF) / 255f;
        this.A = 1;
    }

    public ColorF(int hex, byte alpha)
    {
        this.R = ((hex & 0xFF0000) >> 16) / 255f;
        this.G = ((hex & 0x00FF00) >> 8) / 255f;
        this.B = (hex & 0x0000FF) / 255f;
        this.A = alpha / 255f;
    }

    public static ColorF operator *(ColorF left, float right)
    {
        return new ColorF(left.R, left.G, left.B, left.A * right);
    }

    public float[] ToFloatArray()
    {
        return new float[] { R, G, B, A };
    }

    public static ColorF Lerp(ColorF from, ColorF to, float amt)
    {
        return new ColorF(
            from.R + (to.R - from.R) * amt,
            from.G + (to.G - from.G) * amt,
            from.B + (to.B - from.B) * amt,
            from.A + (to.A - from.A) * amt
        );
    }

    public static ColorF FromString(string s)
    {
        int i = Convert.ToInt32(s, 16);
        return new ColorF(i);
    }

    public static ColorF Darken(ColorF c, float f)
    {
        return new ColorF(c.R * f, c.G * f, c.B * f, c.A);
    }

    public static ColorF operator +(ColorF left, ColorF right)
    {
        return new ColorF(
            left.R + right.R,
            left.G + right.G,
            left.B + right.B,
            left.A + right.A
        );
    }

    public static ColorF White { get { return new ColorF(1f, 1f, 1f, 1f); } }
    public static ColorF Black { get { return new ColorF(0, 0, 0, 1f); } }
    public static ColorF Gray { get { return new ColorF(0.5f, 0.5f, 0.5f, 1f); } }
    public static ColorF LightGray { get { return new ColorF(0.75f, 0.75f, 0.75f, 1f); } }
    public static ColorF DarkGray { get { return new ColorF(0.25f, 0.25f, 0.25f, 1f); } }
    public static ColorF Transparent { get { return new ColorF(0, 0, 0, 0); } }
    public static ColorF PearlGray { get { return new ColorF(0xf6f5f5); } }
    public static ColorF BlueGray { get { return new ColorF(0x145374); } }
    public static ColorF DeepBlue { get { return new ColorF(0x00334e); } }
    public static ColorF Shrimp { get { return new ColorF(0xee6f57); } }
    public static ColorF Orange { get { return new ColorF(0xFA8601); } }
    public static ColorF RoyalBlue { get { return new ColorF(0x4876ff); } }
    public static ColorF DarkGoldenRod { get { return new ColorF(0xffb90f); } }
    public static ColorF Red { get { return new ColorF(0xff0000); } }
    public static ColorF Green { get { return new ColorF(0x00ff00); } }
    public static ColorF Blue { get { return new ColorF(0x0000ff); } }
    public static ColorF Yellow { get { return new ColorF(0xffff00); } }
    public static ColorF Cyan { get { return new ColorF(0x00ffff); } }
    public static ColorF Magenta { get { return new ColorF(0xff00ff); } }
    public static ColorF Silver { get { return new ColorF(0xc0c0c0); } }
    public static ColorF Maroon { get { return new ColorF(0x800000); } }
    public static ColorF Olive { get { return new ColorF(0x808000); } }
    public static ColorF Purple { get { return new ColorF(0x800080); } }
    public static ColorF Teal { get { return new ColorF(0x008080); } }
    public static ColorF Navy { get { return new ColorF(0x000080); } }
    public static ColorF Brown { get { return new ColorF(0xa52a2a); } }
    public static ColorF Pink { get { return new ColorF(0xffc0cb); } }
    public static ColorF Peach { get { return new ColorF(0xffdab9); } }
    public static ColorF Coral { get { return new ColorF(0xff7f50); } }
    public static ColorF Salmon { get { return new ColorF(0xfa8072); } }
    public static ColorF Tomato { get { return new ColorF(0xff6347); } }
    public static ColorF CoralRed { get { return new ColorF(0xff4040); } }
    public static ColorF Crimson { get { return new ColorF(0xdc143c); } }
    public static ColorF RedOrange { get { return new ColorF(0xff4500); } }
    public static ColorF RedViolet { get { return new ColorF(0x8b008b); } }
    public static ColorF DeepPink { get { return new ColorF(0xff1493); } }
    public static ColorF HotPink { get { return new ColorF(0xff69b4); } }
    public static ColorF DeepSkyBlue { get { return new ColorF(0x00bfff); } }
    public static ColorF DodgerBlue { get { return new ColorF(0x1e90ff); } }
    public static ColorF LightBlue { get { return new ColorF(0xadd8e6); } }
    public static ColorF LightSkyBlue { get { return new ColorF(0x87cefa); } }
    public static ColorF LightSteelBlue { get { return new ColorF(0xb0c4de); } }
    public static ColorF LightSlateGray { get { return new ColorF(0x778899); } }
    public static ColorF LightCyan { get { return new ColorF(0xe0ffff); } }
    public static ColorF PaleTurquoise { get { return new ColorF(0xafeeee); } }
    public static ColorF PaleGreen { get { return new ColorF(0x98fb98); } }
}
