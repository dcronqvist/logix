using LogiX.Components;
using LogiX.SaveSystem;
using Newtonsoft.Json.Linq;

namespace LogiX;

public static class Util
{
    public static Font OpenSans { get; set; }
    public static string EnvironmentPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"/LogiX";
    public static string FileDialogStartDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    public static List<Plugin> Plugins { get; set; }

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

        s = s.Split(";;").First();

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

    public static string LogicValuesToBinaryString(List<LogicValue> values)
    {
        string s = "";
        foreach (LogicValue v in values)
        {
            if (v == LogicValue.HIGH)
            {
                s = "1" + s;
            }
            else if (v == LogicValue.LOW)
            {
                s = "0" + s;
            }
        }
        return s;
    }

    public static Vector2 GetMiddleOfListOfVectors(List<Vector2> vectors)
    {
        float x = 0;
        float y = 0;
        foreach (Vector2 v in vectors)
        {
            x += v.X;
            y += v.Y;
        }
        return new Vector2(x / vectors.Count, y / vectors.Count);
    }

    public static List<List<LogicValue>> ReadROM(string file)
    {
        // Load the file and create the ROMValues
        List<List<LogicValue>> values = new List<List<LogicValue>>();
        using (StreamReader sr = new StreamReader(file))
        {
            sr.ReadLine();
            sr.ReadLine();
            string? line = "";
            while ((line = sr.ReadLine()) != null)
            {
                List<LogicValue> lineValues = Util.BinaryStringToLogicValues(line);
                values.Add(lineValues);
            }
        }
        return values;
    }

    public static Dictionary<string, int> ConcatGateAmounts(Dictionary<string, int> a1, Dictionary<string, int> a2)
    {
        Dictionary<string, int> newDict = new Dictionary<string, int>();
        foreach (KeyValuePair<string, int> kvp in a1)
        {
            newDict.Add(kvp.Key, kvp.Value);
        }
        foreach (KeyValuePair<string, int> kvp in a2)
        {
            if (newDict.ContainsKey(kvp.Key))
            {
                newDict[kvp.Key] += kvp.Value;
            }
            else
            {
                newDict.Add(kvp.Key, kvp.Value);
            }
        }
        return newDict;
    }

    public static Dictionary<string, int> EmptyGateAmount()
    {
        return new Dictionary<string, int>();
    }

    public static Dictionary<string, int> GateAmount(params (string, int)[] amounts)
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        foreach ((string, int) t in amounts)
        {
            dict.Add(t.Item1, t.Item2);
        }
        return dict;
    }

    public static List<LogicValue> GetLogicValuesRepresentingDecimal(int dec, int bits)
    {
        List<LogicValue> values = Util.NValues(LogicValue.LOW, bits);
        int i = 0;
        while (dec > 0)
        {
            if (dec % 2 == 1)
            {
                values[bits - 1 - i] = LogicValue.HIGH;
            }
            else
            {
                values[bits - 1 - i] = LogicValue.LOW;
            }
            dec /= 2;
            i++;
        }
        values.Reverse();
        return values;
    }

    public static Component CreateComponentWithPluginIdentifier(string identifier, Vector2 position, JObject data)
    {
        foreach (Plugin p in Plugins)
        {
            foreach (KeyValuePair<string, CustomDescription> cd in p.customComponents)
            {
                if (cd.Key == identifier)
                {
                    return p.CreateComponent(identifier, position, data);
                }
            }
        }
        return null;
    }

    public static List<(string, string)> GetMissingPluginsFromDescriptions(List<CustomDescription> descriptions)
    {
        List<(string, string)> missingPlugins = new List<(string, string)>();
        foreach (CustomDescription cd in descriptions)
        {
            if (!Plugins.Any(p => p.customComponents.ContainsKey(cd.ComponentIdentifier)))
            {
                missingPlugins.Add((cd.Plugin + ", v" + cd.PluginVersion, "Component " + cd.ComponentIdentifier + " is from the plugin " + cd.Plugin + " but that plugin is not installed."));
            }
            else
            {
                // Now check that the plugin version is the same as the description version
                Plugin p = Plugins.First(p => p.customComponents.ContainsKey(cd.ComponentIdentifier));
                if (p.version != cd.PluginVersion)
                {
                    missingPlugins.Add((p.name + ", v" + p.version, "Component uses version " + cd.PluginVersion + " but the installed version is " + p.version + ". \nTo load the circuit, you need to install version " + cd.PluginVersion));
                }
            }
        }
        return missingPlugins;
    }
}