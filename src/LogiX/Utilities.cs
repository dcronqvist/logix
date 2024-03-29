using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using ImGuiNET;
using LogiX.Architecture;
using LogiX.Content;
using LogiX.GLFW;
using LogiX.Graphics;
using Markdig;
using Markdig.Syntax;
using Symphony;

namespace LogiX;

public interface IHashable
{
    public string GetHash();
}

public static class Utilities
{
    static Random RNG = new();
    static Dictionary<(string, int, FontStyle), ImFontPtr> _imguiFonts = new();
    static (Font, int, FontStyle) _currentImGuiFont = (null, 0, FontStyle.Regular);
    public static ContentManager<ContentMeta> ContentManager { get; set; }

    public static void ClearImGuiFonts()
    {
        _imguiFonts.Clear();
    }

    public static void AddImGuiFont(Font font, int size, FontStyle style, ImFontPtr ptr)
    {
        _imguiFonts.Add((font.Identifier, size, style), ptr);
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

    public static object GetCopyOfInstance(object instance)
    {
        var type = instance.GetType();
        var copy = Activator.CreateInstance(type);
        CopyPropsAndFields(instance, ref copy);
        return copy;
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

    public static IEnumerable<float> GetMatrix4x4ValuesIEnumerable(Matrix4x4 m)
    {
        yield return m.M11;
        yield return m.M12;
        yield return m.M13;
        yield return m.M14;
        yield return m.M21;
        yield return m.M22;
        yield return m.M23;
        yield return m.M24;
        yield return m.M31;
        yield return m.M32;
        yield return m.M33;
        yield return m.M34;
        yield return m.M41;
        yield return m.M42;
        yield return m.M43;
        yield return m.M44;
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

    public static Vector2i ApplyRotation(this Vector2i v, int rotation)
    {
        return rotation switch
        {
            1 => new Vector2i(v.Y, v.X),
            3 => new Vector2i(v.Y, v.X),
            _ => v
        };
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

    public static Vector2i Average(this IEnumerable<Vector2i> vecs)
    {
        var sum = Vector2i.Zero;

        foreach (var vec in vecs)
        {
            sum += vec;
        }

        return sum / vecs.Count();
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
            if (a[i] == LogicValue.Z || b[i] == LogicValue.Z)
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

    public static ColorF GetValueColor(this LogicValue[] values)
    {
        if (values.All(v => v == LogicValue.Z))
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
            return $"{Math.Round(ticksPerSeconds / 1000D, 2).ToString("0.00")} kHz";
        else
            return $"{Math.Round(ticksPerSeconds / 1000000D, 2).ToString("0.00")} MHz";
    }

    public static string GetAsHertzString(this double ticksPerSeconds)
    {
        if (ticksPerSeconds < 1000)
            return $"{Math.Round(ticksPerSeconds)} Hz";
        else if (ticksPerSeconds < 1000000)
            return $"{Math.Round(ticksPerSeconds / 1000D, 2).ToString("0.00")} kHz";
        else
            return $"{Math.Round(ticksPerSeconds / 1000000D, 2).ToString("0.00")} MHz";
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

    public static int CeilToOdd(this int value)
    {
        if (value % 2 == 0)
            return value + 1;
        else
            return value;
    }

    public static int CeilToEven(this int value)
    {
        if (value % 2 == 0)
            return value;
        else
            return value + 1;
    }

    public static ComponentSide GetComponentSide(this Vector2i pinOffset, Vector2i size)
    {
        var real = pinOffset.ToVector2(Constants.GRIDSIZE);

        if (pinOffset.X == 0)
        {
            return ComponentSide.LEFT;
        }
        else if (pinOffset.Y == 0)
        {
            return ComponentSide.TOP;
        }
        else if (pinOffset.X == size.X)
        {
            return ComponentSide.RIGHT;
        }
        else if (pinOffset.Y == size.Y)
        {
            return ComponentSide.BOTTOM;
        }
        else
        {
            throw new Exception("Invalid pin offset");
        }
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

    public static Vector2i GetClosestPoint(Vector2i point, params Vector2i[] points)
    {
        Vector2i closest = points[0];
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

    public static Vector2i GetFurthestPoint(Vector2i point, params Vector2i[] points)
    {
        Vector2i furthest = points[0];
        float furthestDist = (point - furthest).Length();
        for (int i = 1; i < points.Length; i++)
        {
            float dist = (point - points[i]).Length();
            if (dist > furthestDist)
            {
                furthest = points[i];
                furthestDist = dist;
            }
        }
        return furthest;
    }

    public static (Vector2i, Vector2i) GetPointsFurthestApart(params Vector2i[] points)
    {
        Vector2i furthest1 = points[0];
        Vector2i furthest2 = points[1];
        float furthestDist = (furthest1 - furthest2).Length();
        for (int i = 0; i < points.Length; i++)
        {
            for (int j = i + 1; j < points.Length; j++)
            {
                float dist = (points[i] - points[j]).Length();
                if (dist > furthestDist)
                {
                    furthest1 = points[i];
                    furthest2 = points[j];
                    furthestDist = dist;
                }
            }
        }
        return (furthest1, furthest2);
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

    public static string GetHash<THash>(IEnumerable<THash> input) where THash : IHashable
    {
        using (var md5 = MD5.Create())
        {
            var inputBytes = input.SelectMany(k => Encoding.ASCII.GetBytes(k.GetHash())).ToArray();
            var hashBytes = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }

    public static string GetFastHashOfFloatArray(float[] array)
    {
        using (var md5 = MD5.Create())
        {
            var inputBytes = array.SelectMany(BitConverter.GetBytes).ToArray();
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
            text.Split('\n').ToList().ForEach(t => ImGui.Text(t));
            ImGui.EndTooltip();
        }
    }

    public static void MouseToolTip(string text)
    {
        ImGui.BeginTooltip();
        text.Split('\n').ToList().ForEach(t => ImGui.Text(t));
        ImGui.EndTooltip();
    }

    public static void MouseToolTip(Action action)
    {
        ImGui.BeginTooltip();
        action();
        ImGui.EndTooltip();
    }

    public static Vector2 GetSize(this RectangleF rec)
    {
        return new Vector2(rec.Width, rec.Height);
    }

    public static int GetGreatestIndex(this IEnumerable<LogicValue> values, LogicValue value)
    {
        for (int i = values.Count() - 1; i >= 0; i--)
        {
            if (values.ElementAt(i) == value)
            {
                return i;
            }
        }
        return -1;
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

    public static byte GetAsByte(this IEnumerable<LogicValue> values)
    {
        byte result = 0;
        for (int i = 0; i < values.Count(); i++)
        {
            result += values.ElementAt(i) == LogicValue.HIGH ? (byte)(1 << i) : (byte)0;
        }
        return result;
    }

    public static byte[] GetAsByteArray(this IEnumerable<LogicValue> values, bool littleEndian = true)
    {
        var result = new byte[values.Count() / 8];
        for (int i = 0; i < result.Length; i++)
        {
            var byteValues = values.Skip(i * 8).Take(8);
            result[i] = byteValues.GetAsByte();
        }
        if (!littleEndian)
        {
            Array.Reverse(result);
        }
        return result;
    }

    public static bool GetAsBool(this LogicValue value)
    {
        return value == LogicValue.HIGH;
    }

    public static int GetAsTwosComplementInt(this IEnumerable<LogicValue> values)
    {
        int result = 0;
        for (int i = 0; i < values.Count(); i++)
        {
            result += values.ElementAt(i) == LogicValue.HIGH ? 1 << i : 0;
        }
        if (values.ElementAt(0) == LogicValue.HIGH)
        {
            result -= (int)Math.Pow(2, values.Count());
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

    public static LogicValue[] GetAsLogicValues(this byte[] value, int bitCount, bool littleEndian = true)
    {
        var result = new LogicValue[bitCount];
        for (int i = 0; i < bitCount; i++)
        {
            var byteIndex = littleEndian ? i / 8 : (bitCount - i - 1) / 8;
            var bitIndex = littleEndian ? i % 8 : 7 - (i % 8);
            result[i] = (value[byteIndex] & (1 << bitIndex)) != 0 ? LogicValue.HIGH : LogicValue.LOW;
        }
        return result;
    }

    public static IEnumerable<T> Slice<T>(this IEnumerable<T> source, int index, int count)
    {
        return source.Skip(index).Take(count);
    }

    public static IEnumerable<LogicValue> PadLSB(this IEnumerable<LogicValue> source, int count)
    {
        var result = source.ToList();
        while (result.Count < count)
        {
            result.Add(LogicValue.Z);
        }
        return result;
    }

    public static IEnumerable<LogicValue> PadMSB(this IEnumerable<LogicValue> source, int count)
    {
        var result = source.ToList();
        while (result.Count < count)
        {
            result.Insert(0, LogicValue.Z);
        }
        return result;
    }

    public static string GetLegibleString(this char c)
    {
        // Only apply to ASCII characters
        if (c < 32 || c > 126)
        {
            return ".";
        }
        else
        {
            return c.ToString();
        }
    }

    public static string GetAsHexString(this IEnumerable<LogicValue> values)
    {
        var symbols = (int)Math.Ceiling(values.Count() / 4f);
        return GetAsUInt(values).ToString($"X{symbols}");
    }

    public static bool AnyUndefined(this IEnumerable<LogicValue> values)
    {
        return values.Any(v => v == LogicValue.Z);
    }

    public static bool IsUndefined(this LogicValue value)
    {
        return value == LogicValue.Z;
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

    public static void WithImGuiFont(Font font, int size, FontStyle style, Action action)
    {
        ImFontPtr oldFont = ImGui.GetFont();
        _currentImGuiFont = (font, size, style);
        ImGui.PushFont(_imguiFonts[(font.Identifier, size, style)]);
        action();
        ImGui.PopFont();
    }

    public static void PushFontBold()
    {
        var (cfont, csize, cstyle) = _currentImGuiFont;
        var newFont = (cfont.Identifier, csize, cstyle == FontStyle.Italic ? FontStyle.BoldItalic : FontStyle.Bold);
        _currentImGuiFont = (cfont, csize, cstyle == FontStyle.Italic ? FontStyle.BoldItalic : FontStyle.Bold);
        ImGui.PushFont(_imguiFonts[newFont]);
    }

    public static void PushFontItalic()
    {
        var (cfont, csize, cstyle) = _currentImGuiFont;
        var newFont = (cfont.Identifier, csize, cstyle == FontStyle.Bold ? FontStyle.BoldItalic : FontStyle.Italic);
        _currentImGuiFont = (cfont, csize, cstyle == FontStyle.Bold ? FontStyle.BoldItalic : FontStyle.Italic);
        ImGui.PushFont(_imguiFonts[newFont]);
    }

    public static void PushFontSize(int size)
    {
        var (cfont, csize, cstyle) = _currentImGuiFont;
        var newFont = (cfont.Identifier, size, cstyle);
        _currentImGuiFont = (cfont, size, cstyle);
        ImGui.PushFont(_imguiFonts[newFont]);
    }

    public unsafe static void PopFontStyle(int n = 1)
    {
        for (int i = 0; i < n; i++)
        {
            ImGui.PopFont();
            var font = ImGui.GetFont();
            var back = _imguiFonts.FirstOrDefault(f => f.Value.NativePtr == font.NativePtr).Key;
            var realFont = GetFont(back.Item1);
            _currentImGuiFont = (realFont, back.Item2, back.Item3);
        }
    }

    public static void RenderMarkdown(string markdown, Action<string> onLinkClicked)
    {
        MarkdownDocument md = Markdown.Parse(markdown, new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());
        ImGuiMarkdownRenderer igmr = new ImGuiMarkdownRenderer(onLinkClicked);

        WithImGuiFont(Constants.UI_FONT_REAL, 20, FontStyle.Regular, () =>
        {
            igmr.Render(md);
        });
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

    public static Font GetFont(string identifier)
    {
        var font = ContentManager.GetContentItem<Font>(identifier);
        return font;
    }

    public static ComponentSide ApplyRotation(this ComponentSide side, int amount)
    {
        if (amount == 0)
            return side;

        var newSide = side switch
        {
            ComponentSide.TOP => ComponentSide.RIGHT,
            ComponentSide.RIGHT => ComponentSide.BOTTOM,
            ComponentSide.BOTTOM => ComponentSide.LEFT,
            ComponentSide.LEFT => ComponentSide.TOP,
            _ => side
        };

        return newSide.ApplyRotation(amount - 1);
    }

    public static string PrettifyKey(this Keys key)
    {
        return new Dictionary<Keys, string>() {
            { Keys.LeftControl, "Ctrl" },
            { Keys.LeftSuper, "Cmd" },
            { Keys.LeftShift, "Shift"},
            { Keys.LeftAlt, "Alt"},
        }.GetValueOrDefault(key, key.ToString());
    }

    public static string PrettifyModifiers(this ModifierKeys mods)
    {
        var result = new List<string>();
        if (mods.HasFlag(ModifierKeys.Control))
            result.Add("Ctrl");
        if (mods.HasFlag(ModifierKeys.Alt))
            result.Add("Alt");
        if (mods.HasFlag(ModifierKeys.Shift))
            result.Add("Shift");
        if (mods.HasFlag(ModifierKeys.Super))
            result.Add("Cmd");

        return string.Join("+", result);
    }

    public static Keys GetAsKey(this ConsoleKey key)
    {
        return key switch
        {
            ConsoleKey.A => Keys.A,
            ConsoleKey.B => Keys.B,
            ConsoleKey.C => Keys.C,
            ConsoleKey.D => Keys.D,
            ConsoleKey.E => Keys.E,
            ConsoleKey.F => Keys.F,
            ConsoleKey.G => Keys.G,
            ConsoleKey.H => Keys.H,
            ConsoleKey.I => Keys.I,
            ConsoleKey.J => Keys.J,
            ConsoleKey.K => Keys.K,
            ConsoleKey.L => Keys.L,
            ConsoleKey.M => Keys.M,
            ConsoleKey.N => Keys.N,
            ConsoleKey.O => Keys.O,
            ConsoleKey.P => Keys.P,
            ConsoleKey.Q => Keys.Q,
            ConsoleKey.R => Keys.R,
            ConsoleKey.S => Keys.S,
            ConsoleKey.T => Keys.T,
            ConsoleKey.U => Keys.U,
            ConsoleKey.V => Keys.V,
            ConsoleKey.W => Keys.W,
            ConsoleKey.X => Keys.X,
            ConsoleKey.Y => Keys.Y,
            ConsoleKey.Z => Keys.Z,
            ConsoleKey.D0 => Keys.Alpha0,
            ConsoleKey.D1 => Keys.Alpha1,
            ConsoleKey.D2 => Keys.Alpha2,
            ConsoleKey.D3 => Keys.Alpha3,
            ConsoleKey.D4 => Keys.Alpha4,
            ConsoleKey.D5 => Keys.Alpha5,
            ConsoleKey.D6 => Keys.Alpha6,
            ConsoleKey.D7 => Keys.Alpha7,
            ConsoleKey.D8 => Keys.Alpha8,
            ConsoleKey.D9 => Keys.Alpha9,
            ConsoleKey.F1 => Keys.F1,
            ConsoleKey.F2 => Keys.F2,
            ConsoleKey.F3 => Keys.F3,
            ConsoleKey.F4 => Keys.F4,
            ConsoleKey.F5 => Keys.F5,
            ConsoleKey.F6 => Keys.F6,
            ConsoleKey.F7 => Keys.F7,
            ConsoleKey.F8 => Keys.F8,
            ConsoleKey.F9 => Keys.F9,
            ConsoleKey.F10 => Keys.F10,
            ConsoleKey.F11 => Keys.F11,
            ConsoleKey.F12 => Keys.F12,
            ConsoleKey.F13 => Keys.F13,
            ConsoleKey.F14 => Keys.F14,
            ConsoleKey.F15 => Keys.F15,
            ConsoleKey.F16 => Keys.F16,
            ConsoleKey.F17 => Keys.F17,
            ConsoleKey.F18 => Keys.F18,
            ConsoleKey.F19 => Keys.F19,
            ConsoleKey.F20 => Keys.F20,
            ConsoleKey.F21 => Keys.F21,
            ConsoleKey.F22 => Keys.F22,
            ConsoleKey.F23 => Keys.F23,
            ConsoleKey.F24 => Keys.F24,
            ConsoleKey.NumPad0 => Keys.Numpad0,
            ConsoleKey.NumPad1 => Keys.Numpad1,
            ConsoleKey.NumPad2 => Keys.Numpad2,
            ConsoleKey.NumPad3 => Keys.Numpad3,
            ConsoleKey.NumPad4 => Keys.Numpad4,
            ConsoleKey.NumPad5 => Keys.Numpad5,
            ConsoleKey.NumPad6 => Keys.Numpad6,
            ConsoleKey.NumPad7 => Keys.Numpad7,
            ConsoleKey.NumPad8 => Keys.Numpad8,
            ConsoleKey.NumPad9 => Keys.Numpad9,
            ConsoleKey.Oem1 => Keys.SemiColon,
            ConsoleKey.Oem2 => Keys.Slash,
            ConsoleKey.Oem3 => Keys.GraveAccent,
            ConsoleKey.Oem4 => Keys.LeftBracket,
            ConsoleKey.Oem5 => Keys.Backslash,
            ConsoleKey.Oem6 => Keys.RightBracket,
            ConsoleKey.Oem7 => Keys.Apostrophe,
            ConsoleKey.Oem8 => Keys.Unknown,
            ConsoleKey.Oem102 => Keys.Unknown,
            ConsoleKey.OemMinus => Keys.Minus,
            // ConsoleKey.OemPlus => Keys.Plus,
            ConsoleKey.OemComma => Keys.Comma,
            ConsoleKey.OemPeriod => Keys.Period,
            ConsoleKey.Spacebar => Keys.Space,
            ConsoleKey.Enter => Keys.Enter,
            ConsoleKey.Tab => Keys.Tab,
            ConsoleKey.Backspace => Keys.Backspace,
            ConsoleKey.Insert => Keys.Insert,
            ConsoleKey.Delete => Keys.Delete,
            ConsoleKey.Home => Keys.Home,
            ConsoleKey.End => Keys.End,
            ConsoleKey.PageUp => Keys.PageUp,
            ConsoleKey.PageDown => Keys.PageDown,
            ConsoleKey.UpArrow => Keys.Up,
            ConsoleKey.DownArrow => Keys.Down,
            ConsoleKey.LeftArrow => Keys.Left,
            ConsoleKey.RightArrow => Keys.Right,
            ConsoleKey.Escape => Keys.Escape,
            _ => throw new NotImplementedException("This key is not implemented for CLI TTY usage yet")
        };
    }

    public static string ToBinaryString(this IEnumerable<LogicValue> values)
    {
        return string.Join("", values.Select(v => v == LogicValue.HIGH ? "1" : (v == LogicValue.Z ? "X" : "0")));
    }

    public static RectangleF GetSegmentBoundingBox((Vector2i, Vector2i) wireSegment)
    {
        var (start, end) = (wireSegment.Item1.ToVector2(Constants.GRIDSIZE), wireSegment.Item2.ToVector2(Constants.GRIDSIZE));
        if (start.X == end.X)
        {
            // VERTICAL
            var width = Constants.WIRE_WIDTH;
            var height = Math.Abs(start.Y - end.Y);
            var x = start.X - width / 2;
            var y = Math.Min(start.Y, end.Y);
            return new RectangleF(x, y, width, height);
        }
        else
        {
            // HORIZONTAL
            var width = Math.Abs(start.X - end.X);
            var height = Constants.WIRE_WIDTH;
            var x = Math.Min(start.X, end.X);
            var y = start.Y - height / 2;
            return new RectangleF(x, y, width, height);
        }
    }

    public static ColorF ToColorF(this Vector4 vec)
    {
        return new ColorF(vec.X, vec.Y, vec.Z, vec.W);
    }

    public static LogicValue[] Multiple(this LogicValue value, int count)
    {
        return Enumerable.Repeat(value, count).ToArray();
    }

    public static bool AllSame(this IEnumerable<LogicValue> values)
    {
        return values.All(v => v == values.First());
    }

    public static bool CheckCircleRectangleCollision(Vector2 circlePosition, float circleRadius, RectangleF rect)
    {
        var closestX = Math.Clamp(circlePosition.X, rect.X, rect.X + rect.Width);
        var closestY = Math.Clamp(circlePosition.Y, rect.Y, rect.Y + rect.Height);

        var distanceX = circlePosition.X - closestX;
        var distanceY = circlePosition.Y - closestY;

        var distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);

        return distanceSquared < (circleRadius * circleRadius);
    }

    public static float DistanceTo(this Vector2 v, Vector2 other)
    {
        return (v - other).Length();
    }

    public static string GetTrailingWhitespace(this string s)
    {
        var whitespace = "";
        for (var i = s.Length - 1; i >= 0; i--)
        {
            if (s[i] == ' ')
            {
                whitespace += " ";
            }
            else
            {
                break;
            }
        }

        return whitespace;
    }

    public static string GetLeadingWhitespace(this string s)
    {
        var whitespace = "";
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] == ' ')
            {
                whitespace += " ";
            }
            else
            {
                break;
            }
        }

        return whitespace;
    }
}