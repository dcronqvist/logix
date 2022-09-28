using System;

namespace LogiX;

public class IO
{
    public string[] Tags { get; }

    private LogicValue TrueValue { get; set; }
    private LogicValue PushedValue { get; set; }

    private bool Pushing { get; set; }

    public IO(params string[] tags)
    {
        Tags = tags;
        TrueValue = LogicValue.UNDEFINED;
        PushedValue = LogicValue.UNDEFINED;
        Pushing = false;
    }

    public void Push(LogicValue value)
    {
        PushedValue = value;
        Pushing = true;
    }

    public void ResetPushed()
    {
        PushedValue = LogicValue.UNDEFINED;
        Pushing = false;
    }

    public LogicValue GetValue()
    {
        return this.TrueValue;
    }

    public LogicValue GetPushedValue()
    {
        return this.PushedValue;
    }

    public void SetValue(LogicValue value)
    {
        this.TrueValue = value;
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