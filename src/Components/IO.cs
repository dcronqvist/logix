using System.Diagnostics.CodeAnalysis;

namespace LogiX.Components;

public class IO
{
    public int BitWidth { get; set; }
    public LogicValue[] Values { get; }
    public LogicValue[] PushedValues { get; private set; }
    public Component OnComponent { get; }
    public Wire? Wire { get; set; }

    public IO(int bitWidth, Component onComponent)
    {
        this.BitWidth = bitWidth;
        this.Values = new LogicValue[bitWidth];
        this.PushedValues = new LogicValue[bitWidth];
        this.OnComponent = onComponent;
    }

    public void PushValues(params LogicValue[] values)
    {
        for (int i = 0; i < this.BitWidth; i++)
        {
            this.PushedValues[i] = values[i];
        }
    }

    public bool TryGetIOWireNode(out IOWireNode? node)
    {
        if (this.Wire is not null)
        {
            if (this.Wire.TryGetIOWireNodeFromPosition(this.GetPosition(), out node))
            {
                return true;
            }
        }
        node = null;
        return false;
    }

    public Vector2 GetPosition()
    {
        return this.OnComponent.GetIOPosition(this);
    }

    public void PushError()
    {
        this.PushedValues = Util.NValues(LogicValue.ERROR, this.BitWidth).ToArray();
    }

    public void PushUnknown()
    {
        this.PushedValues = Util.NValues(LogicValue.UNKNOWN, this.BitWidth).ToArray();
    }

    public void SetValues(params LogicValue[] values)
    {
        for (int i = 0; i < this.BitWidth; i++)
        {
            this.Values[i] = values[i];
        }
    }

    public bool HasLogicValue(LogicValue value)
    {
        return this.Values.Contains(value);
    }

    public bool IsPushingLogicValue(LogicValue value)
    {
        return this.PushedValues.Contains(value);
    }

    public bool HasError()
    {
        return this.HasLogicValue(LogicValue.ERROR);
    }

    public bool IsPushingError()
    {
        return this.IsPushingLogicValue(LogicValue.ERROR);
    }

    public bool HasUnknown()
    {
        return this.HasLogicValue(LogicValue.UNKNOWN);
    }

    public bool IsPushingUnknown()
    {
        return this.IsPushingLogicValue(LogicValue.UNKNOWN);
    }

    public bool IsPushing()
    {
        return this.PushedValues.Any(x => x != LogicValue.UNKNOWN);
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

        return (float)highCount / (float)this.BitWidth;
    }

    public Color GetColor()
    {
        if (this.HasError())
        {
            return Color.RED;
        }
        if (this.HasUnknown())
        {
            int n = 150;
            return new Color(n, n, n, 255);
        }

        if (this.BitWidth == 1)
        {
            return this.Values[0] == LogicValue.HIGH ? Color.BLUE : Color.WHITE;
        }
        else
        {
            return Util.InterpolateColors(Color.GREEN, Color.DARKGREEN, this.GetHighFraction());
        }
    }
}