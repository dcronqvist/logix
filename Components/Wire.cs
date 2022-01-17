using LogiX.SaveSystem;

namespace LogiX.Components;

public class Wire
{
    public int Bits { get; private set; }
    public List<LogicValue> Values { get; private set; }

    public Component To { get; private set; }
    public int ToIndex { get; private set; }
    public Component From { get; private set; }
    public int FromIndex { get; private set; }

    public Wire(int bits, Component to, int toIndex, Component from, int fromIndex)
    {
        this.Bits = bits;
        this.Values = Util.NValues(LogicValue.LOW, bits);
        this.To = to;
        this.From = from;
        this.ToIndex = toIndex;
        this.FromIndex = fromIndex;
    }

    public void SetValues(IEnumerable<LogicValue> values)
    {
        if (values.Count() != this.Bits)
        {
            throw new ArgumentException("The number of values does not match the number of bits.");
        }

        this.Values = new List<LogicValue>(values);
    }

    public void SetAllValues(LogicValue value)
    {
        this.Values = Enumerable.Repeat(value, this.Bits).ToList();
    }

    public float GetHighFraction()
    {
        int highCount = 0;

        foreach (LogicValue value in this.Values)
        {
            if (value == LogicValue.HIGH)
            {
                highCount++;
            }
        }

        return (float)highCount / (float)this.Bits;
    }

    public bool IsPositionOnWire(Vector2 pos)
    {
        float width = 4f;
        Vector2 fromPos = this.From.GetOutputLinePositions(this.FromIndex).Item1 - new Vector2(0, width / 2f);
        Vector2 toPos = this.To.GetInputLinePositions(this.ToIndex).Item1 + new Vector2(0, width / 2f);

        if (!Raylib.CheckCollisionPointRec(pos, Util.CreateRecFromTwoCorners(fromPos, toPos)))
        {
            return false;
        }

        Tuple<float, float, float> line = Util.LineFromTwoPoints(fromPos, toPos);

        float px = (line.Item2 * (line.Item2 * pos.X - line.Item1 * pos.Y) - line.Item1 * line.Item3) / (MathF.Pow(line.Item1, 2) + MathF.Pow(line.Item2, 2));
        float py = (line.Item1 * (-line.Item2 * pos.X + line.Item1 * pos.Y) - line.Item2 * line.Item3) / (MathF.Pow(line.Item1, 2) + MathF.Pow(line.Item2, 2));

        Vector2 endPos = new Vector2(px, py);
        return (pos - endPos).Length() < (width / 2f);
    }

    public void Update(Vector2 mousePosInWorld)
    {

    }

    public void Render(Vector2 mousePosInWorld)
    {
        Vector2 fromPos = this.From.GetOutputLinePositions(this.FromIndex).Item1;
        Vector2 toPos = this.To.GetInputLinePositions(this.ToIndex).Item1;

        Raylib.DrawLineBezier(fromPos, toPos, 4f, Color.BLACK);
        Raylib.DrawLineBezier(fromPos, toPos, 2, Util.InterpolateColors(Color.WHITE, Color.BLUE, this.GetHighFraction()));
    }

    public void RenderSelected()
    {
        Vector2 fromPos = this.From.GetOutputLinePositions(this.FromIndex).Item1;
        Vector2 toPos = this.To.GetInputLinePositions(this.ToIndex).Item1;
        Raylib.DrawLineBezier(fromPos, toPos, 6f, Color.ORANGE);
        Raylib.DrawLineBezier(fromPos, toPos, 2, Util.InterpolateColors(Color.WHITE, Color.BLUE, this.GetHighFraction()));
    }
}