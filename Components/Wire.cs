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

    public void Render(Vector2 mousePosInWorld)
    {
        Vector2 fromPos = this.From.GetOutputLinePositions(this.FromIndex).Item1;
        Vector2 toPos = this.To.GetInputLinePositions(this.ToIndex).Item1;

        Raylib.DrawLineBezier(fromPos, toPos, 4f, Color.BLACK);
        Raylib.DrawLineBezier(fromPos, toPos, 2, Util.InterpolateColors(Color.WHITE, Color.BLUE, this.GetHighFraction()));
    }
}