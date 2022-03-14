using System.Text;
using LogiX.Components;
using LogiX.GateAlgebra;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using Markdig;
using Markdig.Syntax;
using LogiX.SaveSystem;
using QuikGraph;
using QuikGraph.Algorithms.ConnectedComponents;
using System.Diagnostics.CodeAnalysis;

using WireGraph = QuikGraph.UndirectedGraph<LogiX.Components.WireNode, QuikGraph.Edge<LogiX.Components.WireNode>>;

namespace LogiX;

public static class Util
{
    public static Font OpenSans { get; set; }
    public static string EnvironmentPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"/LogiX";
    public static string FileDialogStartDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    public static Editor.Editor Editor { get; set; }
    public static int GridSizeX { get; set; } = 10;
    public static int GridSizeY { get; set; } = 10;

    public static Dictionary<string, ImFontPtr> ImGuiFonts { get; set; }

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
        return new Vector4(c.r / 255f, c.g / 255f, c.b / 255f, c.a / 255f);
    }

    public static Vector3 ToVector3(this Color c)
    {
        return new Vector3(c.r / 255f, c.g / 255f, c.b / 255f);
    }

    public static Color ToColor(this Vector4 v)
    {
        return new Color((byte)(v.X * 255), (byte)(v.Y * 255), (byte)(v.Z * 255), (byte)(v.W * 255));
    }

    public static Color ToColor(this Vector3 v)
    {
        return new Color((byte)(v.X * 255), (byte)(v.Y * 255), (byte)(v.Z * 255), (byte)255);
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

    public static float DistanceToLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
    {
        // Line defined by two points
        float numerator = MathF.Abs((lineEnd.X - lineStart.X) * (lineStart.Y - point.Y) - (lineStart.X - point.X) * (lineEnd.Y - lineStart.Y));
        float denominator = MathF.Sqrt(MathF.Pow(lineEnd.X - lineStart.X, 2) + MathF.Pow(lineEnd.Y - lineStart.Y, 2));

        if (numerator < 0.05f)
        {
            return 0f;
        }

        return numerator / denominator;
    }

    public static Vector2 Vector2Towards(this Vector2 start, float dist, Vector2 other)
    {
        return start + Vector2.Normalize(other - start) * dist;
    }

    public static Rectangle CreateRecFromTwoCorners(Vector2 a, Vector2 b, float padding = 0)
    {
        if (a.Y < b.Y)
        {
            if (a.X < b.X)
            {
                return new Rectangle(a.X - padding, a.Y - padding, (b.X - a.X) + padding * 2, (b.Y - a.Y) + padding * 2);
            }
            else
            {
                return new Rectangle(b.X - padding, a.Y - padding, (a.X - b.X) + padding * 2, (b.Y - a.Y) + padding * 2);
            }
        }
        else
        {
            if (a.X < b.X)
            {
                return new Rectangle(a.X - padding, b.Y - padding, (b.X - a.X) + padding * 2, (a.Y - b.Y) + padding * 2);
            }
            else
            {
                return new Rectangle(b.X - padding, b.Y - padding, (a.X - b.X) + padding * 2, (a.Y - b.Y) + padding * 2);
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
            // case "NAND":
            //     return new NANDLogic();
            case "OR":
                return new ORLogic();
            case "NOR":
                return new NORLogic();
            case "XOR":
                return new XORLogic();
                // case "XNOR":
                //     return new XNORLogic();
                // case "NOT":
                //     return new NOTLogic();
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
            string? line = "";
            while ((line = sr.ReadLine()) != null)
            {
                if (line == "" || line.StartsWith(";;"))
                {
                    continue;
                }
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

    public static List<T> RemoveDuplicates<T>(this List<T> list)
    {
        return list.Distinct().ToList();
    }

    // public static Component CreateComponentWithPluginIdentifier(string identifier, Vector2 position, int rotation, CustomComponentData data)
    // {
    //     foreach (Plugin p in Plugins)
    //     {
    //         foreach (KeyValuePair<string, CustomDescription> cd in p.customComponents)
    //         {
    //             if (cd.Key == identifier)
    //             {
    //                 return p.CreateComponent(identifier, position, rotation, data);
    //             }
    //         }
    //     }
    //     return null;
    // }

    // public static List<(string, string)> GetMissingPluginsFromDescriptions(List<CustomDescription> descriptions)
    // {
    //     List<(string, string)> missingPlugins = new List<(string, string)>();
    //     foreach (CustomDescription cd in descriptions)
    //     {
    //         if (!Plugins.Any(p => p.customComponents.ContainsKey(cd.ComponentIdentifier)))
    //         {
    //             missingPlugins.Add((cd.Plugin + ", v" + cd.PluginVersion, "Component " + cd.ComponentIdentifier + " is from the plugin " + cd.Plugin + " but that plugin is not installed."));
    //         }
    //         else
    //         {
    //             // Now check that the plugin version is the same as the description version
    //             Plugin p = Plugins.First(p => p.customComponents.ContainsKey(cd.ComponentIdentifier));
    //             if (p.version != cd.PluginVersion)
    //             {
    //                 missingPlugins.Add((p.name + ", v" + p.version, "Component uses version " + cd.PluginVersion + " but the installed version is " + p.version + ". \nTo load the circuit, you need to install version " + cd.PluginVersion));
    //             }
    //         }
    //     }
    //     return missingPlugins;
    // }

    // public static Plugin GetPluginWithComponent(string componentIdentifier)
    // {
    //     foreach (Plugin p in Plugins)
    //     {
    //         if (p.customComponents.ContainsKey(componentIdentifier))
    //         {
    //             return p;
    //         }
    //     }
    //     return null;
    // }

    // public static Type GetCustomDataTypeOfCustomComponent(string componentIdentifier)
    // {
    //     Plugin p = GetPluginWithComponent(componentIdentifier);
    //     return p.customComponentTypes[componentIdentifier].Item3;
    // }

    public static string GetPathAsRelative(string path)
    {
        // Also replace all \\ with /
        return Path.GetRelativePath(Directory.GetCurrentDirectory(), path).Replace("\\", "/");
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

    public static void Tooltip(string text)
    {
        ImGui.BeginTooltip();
        ImGui.Text(text);
        ImGui.EndTooltip();
    }

    public static (Vector2, Vector2) GetIntersectingCornersOfPoints(Vector2 a, Vector2 b)
    {
        Vector2 a1 = new Vector2(a.X, b.Y);
        Vector2 a2 = new Vector2(b.X, a.Y);
        return (a1, a2);
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

    public static int GetAsInt(this LogicValue value)
    {
        return value == LogicValue.HIGH ? 1 : 0;
    }

    public static long GetAsLong(this LogicValue value)
    {
        return value == LogicValue.HIGH ? 1L : 0L;
    }

    public static int GetAsInt(this IEnumerable<LogicValue> values)
    {
        int sum = 0;
        int i = 0;
        foreach (LogicValue value in values)
        {
            sum += value.GetAsInt() << i;
            i += 1;
        }
        return sum;
    }

    public static long GetAsLong(this IEnumerable<LogicValue> values)
    {
        long sum = 0;
        int i = 0;
        foreach (LogicValue value in values)
        {
            sum += value.GetAsLong() << i;
            i += 1;
        }
        return sum;
    }

    public static byte GetAsByte(this IEnumerable<LogicValue> values)
    {
        return (byte)GetAsInt(values);
    }

    public static byte GetAsByte(this LogicValue value)
    {
        return (byte)value.GetAsInt();
    }

    public static byte[] GetAsByteArray(this IEnumerable<LogicValue> values)
    {
        byte[] bytes = new byte[values.Count() / 8];
        int i = 0;
        foreach (LogicValue value in values)
        {
            bytes[i / 8] |= (byte)(value.GetAsInt() << (i % 8));
            i += 1;
        }
        return bytes;
    }

    public static List<LogicValue> GetAsLogicValues(this long value, int bits)
    {
        List<LogicValue> values = new List<LogicValue>();
        for (int i = 0; i < bits; i++)
        {
            values.Add((value & (1L << i)) == 0 ? LogicValue.LOW : LogicValue.HIGH);
        }
        return values;
    }

    public static List<LogicValue> GetAsLogicValues(this int value, int bits)
    {
        List<LogicValue> values = new List<LogicValue>();
        for (int i = 0; i < bits; i++)
        {
            values.Add((value & (1 << i)) == 0 ? LogicValue.LOW : LogicValue.HIGH);
        }
        return values;
    }

    public static List<LogicValue> GetAsLogicValues(this byte value, int bits)
    {
        return GetAsLogicValues((int)value, bits);
    }

    private static Random rng = new Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static Vector2 FlipVertically(this Vector2 v)
    {
        return new Vector2(v.X, -v.Y);
    }

    public static Vector2 Round(this Vector2 v)
    {
        return new Vector2(MathF.Round(v.X), MathF.Round(v.Y));
    }

    public static List<T> Append<T>(this List<T> list, T item)
    {
        list.Add(item);
        return list;
    }

    public static List<T> Prepend<T>(this List<T> list, T item)
    {
        list.Insert(0, item);
        return list;
    }

    public static string ToStringPretty<T>(this IEnumerable<T> list)
    {
        return "[" + string.Join(", ", list.Select(x => x.ToString())) + "]";
    }

    // public static List<Action<Editor.Editor, Component>> GetAdditionalComponentContexts(Type component)
    // {
    //     if (Plugins == null)
    //         return new List<Action<Editor.Editor, Component>>();

    //     List<Action<Editor.Editor, Component>> actions = new List<Action<Editor.Editor, Component>>();
    //     foreach (Plugin p in Plugins)
    //     {
    //         actions = actions.Concat(p.GetComponentAdditionalContexts(component)).ToList();
    //     }
    //     return actions;
    // }

    // public static ICDescription? CreateICDescriptionFromGateAlgebra(string icName, string gateAlgebra)
    // {
    //     GateAlgebraLexer mgl = new GateAlgebraLexer(CharStreams.fromString(gateAlgebra));
    //     GateAlgebraParser mgp = new GateAlgebraParser(new CommonTokenStream(mgl));
    //     mgp.BuildParseTree = true;
    //     IParseTree tree = mgp.component();
    //     VisitorCreateCircuit vcc = new VisitorCreateCircuit();
    //     CircuitDescription cd = vcc.Visit(tree);
    //     List<List<string>> inputOrder = cd.GetSwitches().Select(x => new List<string>() { x.ID }).ToList();
    //     List<List<string>> outputOrder = cd.GetLamps().Select(x => new List<string>() { x.ID }).ToList();
    //     ICDescription icd = new ICDescription(icName, System.Numerics.Vector2.Zero, 0, cd, inputOrder, outputOrder);
    //     return icd;
    // }

    // public static bool TryValidateGateAlgebra(string gateAlgebra, out string error)
    // {
    //     using (StringWriter sw = new StringWriter())
    //     {
    //         try
    //         {
    //             GateAlgebraLexer mgl = new GateAlgebraLexer(CharStreams.fromString(gateAlgebra));
    //             GateAlgebraParser mgp = new GateAlgebraParser(new CommonTokenStream(mgl), sw, sw);
    //             mgp.BuildParseTree = true;

    //             if (mgp.component() == null)
    //             {
    //                 error = sw.ToString();
    //                 return false;
    //             }

    //             IParseTree tree = mgp.component();
    //             VisitorCreateCircuit vcc = new VisitorCreateCircuit();
    //             CircuitDescription cd = vcc.Visit(tree);
    //             error = "";
    //             return true;
    //         }
    //         catch (Exception e)
    //         {
    //             error = sw.ToString();
    //             return false;
    //         }
    //     }
    // }

    public static void WithFont(string fontName, Action action)
    {
        ImFontPtr oldFont = ImGui.GetFont();
        ImGui.PushFont(ImGuiFonts[fontName]);
        action();
        ImGui.PopFont();
    }

    public static void HelpMarkerLink(string url, string tooltip = null)
    {
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text(tooltip == null ? $"Click to open in browser: {url}" : tooltip + $"Click to open in browser: {url}");
            ImGui.EndTooltip();
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                //Raylib.OpenURL((sbyte*)url);
            }
        }
    }

    public static bool AssetFileExists(string assetFile)
    {
        string assetDir = Directory.GetCurrentDirectory() + "/assets/";
        return File.Exists(Path.Combine(assetDir, assetFile));
    }

    public static Texture2D GetAssetTexture(string assetFile)
    {
        string assetDir = Directory.GetCurrentDirectory() + "/assets/";
        return Raylib.LoadTexture(Path.Combine(assetDir, assetFile));
    }

    public static void RenderMarkdown(string markdown)
    {
        MarkdownDocument md = Markdown.Parse(markdown);
        ImGuiMarkdownRenderer igmr = new ImGuiMarkdownRenderer();
        igmr.Render(md);
    }

    public static Vector2 RotateAround(this Vector2 v, Vector2 pivot, float angle)
    {
        Vector2 dir = v - pivot;
        return new Vector2(dir.X * MathF.Cos(angle) + dir.Y * MathF.Sin(angle),
                           -dir.X * MathF.Sin(angle) + dir.Y * MathF.Cos(angle)) + pivot;
    }

    public static string GetComponentTypeAsString(this ComponentType type)
    {
        return type.ToString().Split("_").Select(x => x.ToLower()).Select(x => x.First().ToString().ToUpper() + x.Substring(1)).Aggregate((x, y) => x + " " + y);
    }

    public static void RenderTextRotated(Vector2 position, Font font, int fontSize, int spacing, string text, float rotation, Color color)
    {
        Rlgl.rlPushMatrix();
        Rlgl.rlTranslatef(position.X, position.Y, 0);
        Rlgl.rlRotatef(rotation, 0, 0, 1);
        Raylib.DrawTextEx(font, text, Vector2.Zero, fontSize, spacing, color);
        Rlgl.rlPopMatrix();
    }

    public static Rectangle Inflate(this Rectangle rec, float amount)
    {
        Rectangle r = new Rectangle(rec.x - amount, rec.y - amount, rec.width + amount * 2, rec.height + amount * 2);
        return r;
    }

    public static Vector2 SnapToGrid(this Vector2 v)
    {
        return SnapToGrid(v, Util.GridSizeX, Util.GridSizeY);
    }

    public static Vector2 SnapToGrid(this Vector2 v, int gridX, int gridY)
    {
        return new Vector2(MathF.Round(v.X / gridX) * gridX, MathF.Round(v.Y / gridY) * gridY);
    }

    public static bool ContainsVector2(this Rectangle rec, Vector2 v)
    {
        return Raylib.CheckCollisionPointRec(v, rec);
    }

    public static Vector2 MeasureText(this string s, int fontSize)
    {
        return MeasureText(s, Util.OpenSans, fontSize);
    }

    public static Vector2 MeasureText(this string s, Font font, int fontSize)
    {
        return Raylib.MeasureTextEx(font, s, fontSize, 0);
    }

    public static float Min(params float[] values)
    {
        return values.Min();
    }

    public static float Max(params float[] values)
    {
        return values.Max();
    }

    public static bool CheckIfLinesIntersect(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2)
    {
        bool firstHorizontal = start1.Y == end1.Y;
        bool secondHorizontal = start2.Y == end2.Y;

        if (firstHorizontal && secondHorizontal)
        {
            return false;
        }

        Rectangle r1 = Util.CreateRecFromTwoCorners(start1, end1, 1);
        Rectangle r2 = Util.CreateRecFromTwoCorners(start2, end2, 1);

        return Raylib.CheckCollisionRecs(r1, r2);
    }

    public static bool ContainsLine(this Rectangle rec, Vector2 start, Vector2 end)
    {
        return Raylib.CheckCollisionRecs(Util.CreateRecFromTwoCorners(start, end, 1), rec);
    }

    // public static List<Component> TopologicallyOrder(List<Component> components)
    // {
    //     List<Component> orderedComponents = new List<Component>();
    //     List<Component> unorderedComponents = new List<Component>();
    //     foreach (Component component in components)
    //     {
    //         if (component.Inputs.Count == 0)
    //         {
    //             orderedComponents.Add(component);
    //         }
    //         else
    //         {
    //             unorderedComponents.Add(component);
    //         }
    //     }

    //     while (unorderedComponents.Count > 0)
    //     {
    //         List<Component> nextUnorderedComponents = new List<Component>();
    //         foreach (Component component in unorderedComponents)
    //         {
    //             bool allInputsOrdered = true;
    //             foreach (Component input in component.Inputs.Select(x => x.Signal.From))
    //             {
    //                 if (!orderedComponents.Contains(input))
    //                 {
    //                     allInputsOrdered = false;
    //                     break;
    //                 }
    //             }
    //             if (allInputsOrdered)
    //             {
    //                 orderedComponents.Add(component);
    //             }
    //             else
    //             {
    //                 nextUnorderedComponents.Add(component);
    //             }
    //         }
    //         unorderedComponents = nextUnorderedComponents;
    //     }

    //     return orderedComponents;
    // }

    public static List<T> Copy<T>(this List<T> list)
    {
        List<T> copy = new List<T>();
        foreach (T item in list)
        {
            copy.Add(item);
        }
        return copy;
    }

    //     public static void ExecuteCommandInEditor(Command<Editor.Editor> command)
    //     {
    //         Editor.Execute(command, Editor);
    //     }

    public static bool SameAs(this LogicValue[] values, LogicValue[] other)
    {
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] != other[i])
            {
                return false;
            }
        }
        return true;
    }

    public static float GetMaxWidthOfStrings(Font font, int fontSize, int spacing, string[] strings)
    {
        float maxWidth = 0;
        foreach (string s in strings)
        {
            float width = Raylib.MeasureTextEx(font, s, fontSize, spacing).X;
            if (width > maxWidth)
            {
                maxWidth = width;
            }
        }
        return maxWidth;
    }

    public static ComponentSide GetRotatedComponentSide(ComponentSide side, int rotation)
    {
        if (side == ComponentSide.LEFT)
        {
            switch (rotation)
            {
                case 0:
                    return ComponentSide.LEFT;
                case 1:
                    return ComponentSide.TOP;
                case 2:
                    return ComponentSide.RIGHT;
                case 3:
                    return ComponentSide.BOTTOM;
            }
        }
        else if (side == ComponentSide.TOP)
        {
            switch (rotation)
            {
                case 0:
                    return ComponentSide.TOP;
                case 1:
                    return ComponentSide.RIGHT;
                case 2:
                    return ComponentSide.BOTTOM;
                case 3:
                    return ComponentSide.LEFT;
            }
        }
        else if (side == ComponentSide.RIGHT)
        {
            switch (rotation)
            {
                case 0:
                    return ComponentSide.RIGHT;
                case 1:
                    return ComponentSide.BOTTOM;
                case 2:
                    return ComponentSide.LEFT;
                case 3:
                    return ComponentSide.TOP;
            }
        }
        else if (side == ComponentSide.BOTTOM)
        {
            switch (rotation)
            {
                case 0:
                    return ComponentSide.BOTTOM;
                case 1:
                    return ComponentSide.LEFT;
                case 2:
                    return ComponentSide.TOP;
                case 3:
                    return ComponentSide.RIGHT;
            }
        }

        return side;
    }

    public static WireGraph ConnectVertices(WireGraph sourceGraph, WireNode source, WireGraph targetGraph, WireNode target, out bool deleteTargetGraph)
    {
        if (sourceGraph == targetGraph)
        {
            // THE SAME GRAPH, JUST ADD AN EDGE
            sourceGraph.AddEdge(new Edge<WireNode>(source, target));
            deleteTargetGraph = false;
        }
        else
        {
            // NOT THE SAME GRAPH, MUST ADD ALL VERTICES AND EDGES OF TARGET GRAPH INTO SOURCE
            sourceGraph.AddVertexRange(targetGraph.Vertices);
            sourceGraph.AddEdgeRange(targetGraph.Edges);
            sourceGraph.AddEdge(new Edge<WireNode>(source, target));
            deleteTargetGraph = true;
        }

        return sourceGraph;
    }

    public static bool DisconnectVertices(ref WireGraph graph, WireNode source, WireNode target, [NotNullWhen(true)] out WireGraph? newGraph)
    {
        // MUST REMOVE EDGE
        // CHECK IF IT CREATED MORE THAN 1 CONNECTED COMPONENT
        // IF IT DID, THEN WE MUST CREATE A NEW GRAPH WITH ONE 
        // OF THESE COMPONENTS

        graph.RemoveEdgeIf(x => x.Source == source && x.Target == target);

        ConnectedComponentsAlgorithm<WireNode, Edge<WireNode>> connComp = new ConnectedComponentsAlgorithm<WireNode, Edge<WireNode>>(graph);
        connComp.Compute();

        int components = connComp.ComponentCount;

        if (components > 1)
        {
            // CREATE NEW GRAPH WITH ONE OF THE COMPONENTS
            newGraph = new WireGraph();

            foreach (KeyValuePair<WireNode, int> kvp in connComp.Components)
            {
                if (kvp.Value == 1)
                {
                    if (graph.AdjacentDegree(kvp.Key) > 0)
                        newGraph.AddVertex(kvp.Key);
                }
            }

            foreach (KeyValuePair<WireNode, int> kvp in connComp.Components)
            {
                if (kvp.Value == 1)
                {
                    newGraph.AddEdgeRange(graph.AdjacentEdges(kvp.Key));
                    graph.RemoveVertex(kvp.Key);
                }
            }

            return true;
        }
        else
        {
            // DID NOT CREATE ANOTHER CONNECTED COMPONENT, SO NO NEW GRAPH
            newGraph = null;
            return false;
        }
    }

    public static JunctionWireNode GetJunctionWireNodeFromPos(Simulator simulator, Vector2 position, out Wire wire)
    {
        simulator.TryGetJunctionFromPosition(position, out JunctionWireNode? junction, out wire!);
        return junction!;
    }

    public static IOWireNode GetIOWireNodeFromPos(Simulator simulator, Vector2 position, out Wire wire)
    {
        simulator.TryGetIOWireNodeFromWorldPosition(position, out IOWireNode? io, out wire!);
        return io!;
    }

    public static Edge<WireNode> GetEdgeFromPos(Simulator simulator, Vector2 position, out Wire wire)
    {
        simulator.TryGetEdgeFromPosition(position, out Edge<WireNode>? edge, out wire!);
        return edge!;
    }

    public static WireNode GetWireNodeFromPos(Simulator simulator, Vector2 position, out Wire wire)
    {
        simulator.TryGetWireNodeFromWorldPosition(position, out WireNode? node, out wire!);
        return node!;
    }

    public static Wire GetWireFromPos(Simulator simulator, Vector2 position)
    {
        if (simulator.TryGetWireNodeFromWorldPosition(position, out WireNode? node, out Wire? wire))
        {
            return wire;
        }
        else
        {
            simulator.TryGetEdgeFromPosition(position, out Edge<WireNode>? edge, out wire);
            return wire!;
        }
    }

    public static Vector2 MiddleOfEdge(this Edge<WireNode> edge)
    {
        return (edge.Source.GetPosition() + edge.Target.GetPosition()) / 2;
    }
}