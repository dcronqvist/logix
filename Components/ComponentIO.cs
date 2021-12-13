namespace LogiX.Components;

public class ComponentIO
{
    public int Bits { get; private set; }
    public List<LogicValue> Values { get; private set; }
    public string Identifier { get; set; }
    public Component OnComponent { get; private set; }
    public int OnComponentIndex { get; private set; }
    public virtual Vector2 Position => Vector2.Zero;

    public ComponentIO(int bits, string identifier, Component component, int index) : this(bits, identifier, component, index, Util.NValues(LogicValue.LOW, bits)) { }

    public ComponentIO(int bits, string identifier, Component component, int index, IEnumerable<LogicValue> values)
    {
        if (bits != values.Count())
        {
            throw new ArgumentException("The number of bits must match the number of values.");
        }

        this.Values = new List<LogicValue>(values);
        this.Bits = bits;
        this.Identifier = identifier;
        this.OnComponent = component;
        this.OnComponentIndex = index;
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