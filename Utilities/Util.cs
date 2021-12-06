using LogiX.Components;

namespace LogiX;

public static class Util
{
    public static List<T> NValues<T>(T value, int n)
    {
        return Enumerable.Repeat(value, n).ToList();
    }

    public static List<T> Listify<T>(params T[] args)
    {
        return args.ToList();
    }

    public static List<T> EmptyList<T>()
    {
        return new List<T>();
    }

    public static Color InterpolateColors(Color src, Color target, float val)
    {
        return new Color(src.r + (int)((target.r - src.r) * val),
                        src.g + (int)((target.g - src.g) * val),
                        src.b + (int)((target.b - src.b) * val),
                        src.a + (int)((target.a - src.a) * val));
    }
}