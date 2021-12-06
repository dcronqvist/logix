namespace LogiX.Components;

public class ComponentIO
{
    public int Bits { get; private set; }
    public List<LogicValue> Values { get; private set; }
    public string Identifier { get; set; }

    public ComponentIO(int bits, string identifier)
    {
        this.Values = Util.NValues(LogicValue.LOW, bits);
        this.Bits = bits;
        this.Identifier = identifier;
    }

    public ComponentIO(int bits, string identifier, IEnumerable<LogicValue> values)
    {
        if (bits != values.Count())
        {
            throw new ArgumentException("The number of bits must match the number of values.");
        }

        this.Values = new List<LogicValue>(values);
        this.Bits = bits;
        this.Identifier = identifier;
    }

    public void SetValues(List<LogicValue> values)
    {
        Values = values;
    }

    public void SetValues(params LogicValue[] values)
    {
        Values = new List<LogicValue>(values);
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
}