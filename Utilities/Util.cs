using LogiX.Components;

namespace LogiX;

public static class Util
{
    public static Font OpenSans { get; set; }

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

    public static Vector4 ToVector4(this Color c)
    {
        return new Vector4(c.r, c.g, c.b, c.a);
    }

    public static Tuple<float, float, float> LineFromTwoPoints(Vector2 a, Vector2 b)
    {
        // y = kx + m
        float slope = (b.Y - a.Y) / (b.X - a.X);

        // y = slope * x + m
        // slope intercept
        // y - (slope * x) = m
        float m = a.Y - (slope * a.X);

        // y = slope * x + m
        // slope * x - y + m = 0

        // ax + by + c = 0
        return new Tuple<float, float, float>(slope, -1f, m);
    }

    public static Rectangle CreateRecFromTwoCorners(Vector2 a, Vector2 b)
    {
        if (a.Y < b.Y)
        {
            if (a.X < b.X)
            {
                return new Rectangle(a.X, a.Y, (b.X - a.X), (b.Y - a.Y));
            }
            else
            {
                return new Rectangle(b.X, a.Y, (a.X - b.X), (b.Y - a.Y));
            }
        }
        else
        {
            if (a.X < b.X)
            {
                return new Rectangle(a.X, b.Y, (b.X - a.X), (a.Y - b.Y));
            }
            else
            {
                return new Rectangle(b.X, b.Y, (a.X - b.X), (a.Y - b.Y));
            }
        }
    }

    public static Color Opacity(this Color c, float opacity)
    {
        return new Color(c.r, c.g, c.b, (int)(c.a * opacity));
    }

    public static string Pretty(this KeyboardKey key)
    {
        return new Dictionary<KeyboardKey, string>() {
            { KeyboardKey.KEY_LEFT_CONTROL, "Ctrl" },
            { KeyboardKey.KEY_LEFT_SUPER, "Cmd"}
        }.GetValueOrDefault(key, key.ToString().Replace("KEY_", ""));
    }

    public static bool TryGetFileInfo(string filePath, out FileInfo info)
    {
        if (File.Exists(filePath))
        {
            info = new FileInfo(filePath);
            return true;
        }
        info = null;
        return false;
    }

    public static string BytesToString(long byteCount)
    {
        string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
        if (byteCount == 0)
            return "0" + suf[0];
        long bytes = Math.Abs(byteCount);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return (Math.Sign(byteCount) * num).ToString() + " " + suf[place];
    }

    public static string ToSuitableFileName(this string s)
    {
        return s.Replace(" ", "-").ToLower();
    }

    public static IGateLogic GetGateLogicFromName(string name)
    {
        switch (name)
        {
            case "AND":
                return new ANDLogic();
            case "NAND":
                return new NANDLogic();
            case "OR":
                return new ORLogic();
            case "NOR":
                return new NORLogic();
            case "XOR":
                return new XORLogic();
            case "NOT":
                return new NOTLogic();
        }

        return null;
    }

    public static List<LogicValue> BinaryStringToLogicValues(string s)
    {
        List<LogicValue> values = new List<LogicValue>();

        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '1')
            {
                values.Add(LogicValue.HIGH);
            }
            else if (s[i] == '0')
            {
                values.Add(LogicValue.LOW);
            }
        }
        values.Reverse();
        return values;
    }
}