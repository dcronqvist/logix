namespace LogiX.Components;

public class Wire
{
    public int Bits { get; private set; }
    public List<LogicValue> Values { get; private set; }

    public Wire(int bits)
    {
        this.Bits = bits;
        this.Values = Util.NValues(LogicValue.LOW, bits);
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
}