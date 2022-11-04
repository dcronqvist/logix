using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using ImGuiNET;
using LogiX.Architecture;
using LogiX.Graphics;
using Markdig;
using Markdig.Syntax;

namespace LogiX;

public static class Utilities
{
    static Random RNG = new();
    static Dictionary<string, ImFontPtr> _imguiFonts = new();

    public static void ClearImGuiFonts()
    {
        _imguiFonts.Clear();
    }

    public static void AddImGuiFont(Font font, ImFontPtr ptr)
    {
        _imguiFonts.Add(font.Identifier, ptr);
    }

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

    public static RectangleF Inflate(this RectangleF r, float f)
    {
        return new Rectangle((int)(r.X - f), (int)(r.Y - f), (int)(r.Width + f * 2), (int)(r.Height + f * 2));
    }

    public static RectangleF Inflate(this RectangleF r, float x, float y)
    {
        return new RectangleF(r.X - x, r.Y - y, r.Width + x * 2, r.Height + y * 2);
    }

    public static RectangleF Inflate(this RectangleF r, float left, float top, float right, float bottom)
    {
        return new RectangleF(r.X - left, r.Y - top, r.Width + left + right, r.Height + top + bottom);
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

    public static Vector2 GetMiddleOfVec2(Vector2 v1, Vector2 v2)
    {
        return new Vector2((v1.X + v2.X) / 2, (v1.Y + v2.Y) / 2);
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
            if (a[i] == LogicValue.UNDEFINED || b[i] == LogicValue.UNDEFINED)
                continue;

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

    public static int GetNextInt(int min, int max)
    {
        return RNG.Next(min, max);
    }

    public static ColorF GetValueColor(LogicValue value)
    {
        return value switch
        {
            LogicValue.LOW => Constants.COLOR_LOW,
            LogicValue.HIGH => Constants.COLOR_HIGH,
            _ => Constants.COLOR_UNDEFINED
        };
    }

    public static ColorF GetValueColor(LogicValue[] values)
    {
        if (values.All(v => v == LogicValue.UNDEFINED))
        {
            return Constants.COLOR_UNDEFINED;
        }

        var highs = values.Count(v => v == LogicValue.HIGH);
        var total = values.Length;

        return ColorF.Lerp(Constants.COLOR_LOW, Constants.COLOR_HIGH, highs / (float)total);
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

    public static bool CanFindPositionInGraph(List<(Vector2i, Vector2i)> edges, Vector2i start, Vector2i end)
    {
        var visited = new HashSet<Vector2i>();
        var queue = new Queue<Vector2i>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == end)
                return true;

            visited.Add(current);

            foreach (var edge in edges)
            {
                if (edge.Item1 == current && !visited.Contains(edge.Item2))
                    queue.Enqueue(edge.Item2);
                else if (edge.Item2 == current && !visited.Contains(edge.Item1))
                    queue.Enqueue(edge.Item1);
            }
        }

        return false;
    }

    public static List<(Vector2i, Vector2i)> FindAllTraversableEdges(List<(Vector2i, Vector2i)> edges, Vector2i start)
    {
        var visited = new HashSet<Vector2i>();
        var queue = new Queue<Vector2i>();
        queue.Enqueue(start);

        var traversableEdges = new List<(Vector2i, Vector2i)>();

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            visited.Add(current);

            foreach (var edge in edges)
            {
                if (edge.Item1 == current && !visited.Contains(edge.Item2))
                {
                    queue.Enqueue(edge.Item2);
                    traversableEdges.Add(edge);
                }
                else if (edge.Item2 == current && !visited.Contains(edge.Item1))
                {
                    queue.Enqueue(edge.Item1);
                    traversableEdges.Add(edge);
                }
            }
        }

        return traversableEdges;
    }

    public static bool IsPositionBetween(Vector2i start, Vector2i end, Vector2i pos)
    {
        if (start.X == end.X)
        {
            int minY = Math.Min(start.Y, end.Y);
            int maxY = Math.Max(start.Y, end.Y);
            return pos.X == start.X && pos.Y > minY && pos.Y < maxY;
        }
        else if (start.Y == end.Y)
        {
            int minX = Math.Min(start.X, end.X);
            int maxX = Math.Max(start.X, end.X);
            return pos.Y == start.Y && pos.X > minX && pos.X < maxX;
        }
        else
        {
            throw new Exception("Points are not aligned on the x or y axis");
        }
    }

    public static bool IsPointInGraph(List<(Vector2i, Vector2i)> edges, Vector2i point)
    {
        foreach (var edge in edges)
        {
            if (edge.Item1 == point || edge.Item2 == point)
                return true;
        }
        return false;
    }

    public static bool TryGetPerpendicularEdgesTo(List<(Vector2i, Vector2i)> edges, (Vector2i, Vector2i) edge, out List<(Vector2i, Vector2i)> perpendicular)
    {
        var perpendicularEdges = new List<(Vector2i, Vector2i)>();
        foreach (var otherEdge in edges)
        {
            if (otherEdge == edge)
                continue;

            if (otherEdge.Item1 == edge.Item1 || otherEdge.Item2 == edge.Item1)
                perpendicularEdges.Add(otherEdge);
            else if (otherEdge.Item1 == edge.Item2 || otherEdge.Item2 == edge.Item2)
                perpendicularEdges.Add(otherEdge);
        }

        perpendicular = perpendicularEdges;
        return perpendicularEdges.Count > 0;
    }

    public static bool AreEdgesParallel((Vector2i, Vector2i) edge1, (Vector2i, Vector2i) edge2)
    {
        if (edge1.Item1.X == edge1.Item2.X && edge2.Item1.X == edge2.Item2.X)
            return true;
        else if (edge1.Item1.Y == edge1.Item2.Y && edge2.Item1.Y == edge2.Item2.Y)
            return true;
        else
            return false;
    }

    public static bool VertexOnlyHasOneEdge(List<(Vector2i, Vector2i)> edges, Vector2i vertex, out (Vector2i, Vector2i) edge)
    {
        int count = 0;
        edge = default((Vector2i, Vector2i));
        foreach (var e in edges)
        {
            if (e.Item1 == vertex || e.Item2 == vertex)
            {
                count++;
                edge = e;
            }
        }
        return count == 1;
    }

    public static string GetHash(string input)
    {
        using (var md5 = MD5.Create())
        {
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }

    public static Vector2 Pad(this Vector2 v, float padding)
    {
        return new Vector2(v.X + padding, v.Y + padding);
    }

    public static Vector2 Max(Vector2 v1, Vector2 v2)
    {
        return new Vector2(Math.Max(v1.X, v2.X), Math.Max(v1.Y, v2.Y));
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

    public static RectangleF GetWireRectangle(Vector2i start, Vector2i end)
    {
        var wireWidth = Constants.WIRE_WIDTH;
        var wStart = start.ToVector2(Constants.GRIDSIZE);
        var wEnd = end.ToVector2(Constants.GRIDSIZE);

        if (start.X == end.X)
        {
            // VERTICAL WIRE
            var minY = Math.Min(wStart.Y, wEnd.Y);
            var maxY = Math.Max(wStart.Y, wEnd.Y);
            return new RectangleF(wStart.X - wireWidth / 2, minY, wireWidth, maxY - minY);
        }
        else if (start.Y == end.Y)
        {
            // HORIZONTAL WIRE
            var minX = Math.Min(wStart.X, wEnd.X);
            var maxX = Math.Max(wStart.X, wEnd.X);
            return new RectangleF(minX, wStart.Y - wireWidth / 2, maxX - minX, wireWidth);
        }
        else
        {
            throw new Exception("Points are not aligned on the x or y axis");
        }
    }

    public static RectangleF CreateRecFromTwoCorners(Vector2 a, Vector2 b, float padding = 0)
    {
        if (a.Y < b.Y)
        {
            if (a.X < b.X)
            {
                return new RectangleF(a.X - padding, a.Y - padding, (b.X - a.X) + padding * 2, (b.Y - a.Y) + padding * 2);
            }
            else
            {
                return new RectangleF(b.X - padding, a.Y - padding, (a.X - b.X) + padding * 2, (b.Y - a.Y) + padding * 2);
            }
        }
        else
        {
            if (a.X < b.X)
            {
                return new RectangleF(a.X - padding, b.Y - padding, (b.X - a.X) + padding * 2, (a.Y - b.Y) + padding * 2);
            }
            else
            {
                return new RectangleF(b.X - padding, b.Y - padding, (a.X - b.X) + padding * 2, (a.Y - b.Y) + padding * 2);
            }
        }
    }

    public static Type RecursivelyCheckBaseclassUntilRawGeneric(Type generic, Type toCheck)
    {
        var previousType = toCheck;
        while (toCheck != null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur)
            {
                return previousType;
            }
            previousType = toCheck;
            toCheck = toCheck.BaseType;
        }
        return null;
    }

    public static void ImGuiHelp(string text)
    {
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text(text);
            ImGui.EndTooltip();
        }
    }

    public static void MouseToolTip(string text)
    {
        ImGui.BeginTooltip();
        ImGui.Text(text);
        ImGui.EndTooltip();
    }

    public static Vector2 GetSize(this RectangleF rec)
    {
        return new Vector2(rec.Width, rec.Height);
    }

    public static int GetAsInt(this IEnumerable<LogicValue> values)
    {
        int result = 0;
        for (int i = 0; i < values.Count(); i++)
        {
            result += values.ElementAt(i) == LogicValue.HIGH ? 1 << i : 0;
        }
        return result;
    }

    public static uint GetAsUInt(this IEnumerable<LogicValue> values)
    {
        uint result = 0;
        for (int i = 0; i < values.Count(); i++)
        {
            result += values.ElementAt(i) == LogicValue.HIGH ? 1u << i : 0;
        }
        return result;
    }

    public static LogicValue[] GetAsLogicValues(this uint value, int bitCount)
    {
        var result = new LogicValue[bitCount];
        for (int i = 0; i < bitCount; i++)
        {
            result[i] = (value & (1u << i)) != 0 ? LogicValue.HIGH : LogicValue.LOW;
        }
        return result.ToList().Reverse<LogicValue>().ToArray();
    }

    public static LogicValue[] GetAsLogicValues(this byte value, int bitCount)
    {
        var result = new LogicValue[bitCount];
        for (int i = 0; i < bitCount; i++)
        {
            result[i] = (value & (1 << i)) != 0 ? LogicValue.HIGH : LogicValue.LOW;
        }
        return result.ToList().Reverse<LogicValue>().ToArray();
    }

    public static string GetAsHexString(this IEnumerable<LogicValue> values)
    {
        var symbols = (int)Math.Ceiling(values.Count() / 4f);
        return GetAsUInt(values).ToString($"X{symbols}");
    }

    public static bool AnyUndefined(this IEnumerable<LogicValue> values)
    {
        return values.Any(v => v == LogicValue.UNDEFINED);
    }

    public static bool IsUndefined(this LogicValue value)
    {
        return value == LogicValue.UNDEFINED;
    }

    public static string GetAsByteString(long byteCount)
    {
        string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
        if (byteCount == 0)
            return "0" + suf[0];
        long bytes = Math.Abs(byteCount);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return (Math.Sign(byteCount) * num).ToString() + " " + suf[place];
    }

    public static byte[] GetByteArray(this IEnumerable<LogicValue> values, int bytes)
    {
        var result = new byte[bytes];
        for (int i = 0; i < bytes; i++)
        {
            result[i] = (byte)values.Skip(i * 8).Take(8).GetAsInt();
        }
        return result;
    }

    public static LogicValue[] GetLogicValues(this byte[] bytes)
    {
        var result = new List<LogicValue>();
        foreach (var b in bytes)
        {
            result.AddRange(b.GetAsLogicValues(8));
        }
        return result.ToArray();
    }

    public static void WithImGuiFont(string identifier, Action action)
    {
        ImFontPtr oldFont = ImGui.GetFont();
        ImGui.PushFont(_imguiFonts[identifier]);
        action();
        ImGui.PopFont();
    }

    public static void RenderMarkdown(string markdown)
    {
        MarkdownDocument md = Markdown.Parse(markdown);
        ImGuiMarkdownRenderer igmr = new ImGuiMarkdownRenderer();
        igmr.Render(md);
    }

    public static void OpenURL(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }
}