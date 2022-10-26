using System;

namespace LogiX;

public class IO
{
    public string Identifier { get; set; }
    public string[] Tags { get; }
    public ComponentSide Side { get; }
    public int Bits { get; }

    private LogicValue[] TrueValues { get; set; }
    private LogicValue[] PushedValues { get; set; }

    private bool Pushing { get; set; }

    public IO(string identifier, int bits, ComponentSide side, string[] tags)
    {
        this.Identifier = identifier;
        this.Bits = bits;
        this.Side = side;
        this.Tags = tags;
        this.TrueValues = Enumerable.Repeat(LogicValue.UNDEFINED, bits).ToArray();
        this.PushedValues = Enumerable.Repeat(LogicValue.UNDEFINED, bits).ToArray();
        this.Pushing = false;
    }

    public void Push(params LogicValue[] values)
    {
        if (values.Length != Bits)
            throw new ArgumentException("The number of values pushed must match the number of bits in the IO.");

        if (values.All(v => v == LogicValue.UNDEFINED))
            return;

        this.PushedValues = values;
        this.Pushing = true;
    }

    public void ResetPushed()
    {
        this.PushedValues = Enumerable.Repeat(LogicValue.UNDEFINED, this.Bits).ToArray();
        this.Pushing = false;
    }

    public LogicValue[] GetValues()
    {
        return this.TrueValues;
    }

    public LogicValue[] GetPushedValues()
    {
        return this.PushedValues;
    }

    public void SetValues(params LogicValue[] values)
    {
        this.TrueValues = values;
    }

    public bool IsPushing()
    {
        return this.Pushing;
    }

    public bool HasTag(string tag)
    {
        return Tags.Contains(tag);
    }
}