using LogiX.SaveSystem;

namespace LogiX.Components;

public interface IGateLogic
{
    LogicValue GetOutput(params LogicValue[] inputs);
    string GetLogicText();
}

public class LogicGate : Component
{
    public IGateLogic Logic { get; set; }
    public int Bits { get; set; }
    public override string Text => this.Logic.GetLogicText();

    public LogicGate(Vector2 position, int bits, IGateLogic logic, string? uniqueID = null) : base(position, ComponentType.LOGIC_GATE, uniqueID)
    {
        this.Logic = logic;
        this.Bits = bits;
        for (int i = 0; i < bits; i++)
        {
            this.AddIO(1, new IOConfig(ComponentSide.LEFT)); // Inputs
        }
        this.AddIO(1, new IOConfig(ComponentSide.RIGHT)); // Output
    }

    public override void PerformLogic()
    {
        LogicValue[] inputValues = this.IOs.Take(this.Bits).Select(io => io.Item1.Values[0]).ToArray();
        this.GetIO(this.IOs.Count - 1).PushValues(this.Logic.GetOutput(inputValues));
    }

    public override ComponentDescription ToDescription()
    {
        return new DescriptionGate(this.Position, this.Rotation, this.UniqueID, this.Bits, this.Logic.GetLogicText());
    }
}

public class ANDLogic : IGateLogic
{
    public string GetLogicText()
    {
        return "AND";
    }

    public LogicValue GetOutput(params LogicValue[] inputs)
    {
        if (inputs.Any(v => v == LogicValue.ERROR))
        {
            return LogicValue.ERROR;
        }
        else if (inputs.Any(v => v == LogicValue.UNKNOWN))
        {
            return LogicValue.UNKNOWN;
        }
        else
        {
            return inputs.All(v => v == LogicValue.HIGH) ? LogicValue.HIGH : LogicValue.LOW;
        }
    }
}

public class ORLogic : IGateLogic
{
    public string GetLogicText()
    {
        return "OR";
    }

    public LogicValue GetOutput(params LogicValue[] inputs)
    {
        if (inputs.Any(v => v == LogicValue.ERROR))
        {
            return LogicValue.ERROR;
        }
        else if (inputs.Any(v => v == LogicValue.HIGH))
        {
            return LogicValue.HIGH;
        }
        else if (inputs.Any(v => v == LogicValue.UNKNOWN))
        {
            return LogicValue.UNKNOWN;
        }
        else
        {
            return LogicValue.LOW;
        }
    }
}

public class XORLogic : IGateLogic
{
    public string GetLogicText()
    {
        return "XOR";
    }

    public LogicValue GetOutput(params LogicValue[] inputs)
    {
        if (inputs.Any(v => v == LogicValue.ERROR))
        {
            return LogicValue.ERROR;
        }
        else if (inputs.Any(v => v == LogicValue.UNKNOWN))
        {
            return LogicValue.UNKNOWN;
        }
        else
        {
            return inputs.Count(v => v == LogicValue.HIGH) % 2 == 1 ? LogicValue.HIGH : LogicValue.LOW;
        }
    }
}

public class NORLogic : IGateLogic
{
    public string GetLogicText()
    {
        return "NOR";
    }

    public LogicValue GetOutput(params LogicValue[] inputs)
    {
        if (inputs.Any(v => v == LogicValue.ERROR))
        {
            return LogicValue.ERROR;
        }
        else if (inputs.Any(v => v == LogicValue.HIGH))
        {
            return LogicValue.LOW;
        }
        else if (inputs.Any(v => v == LogicValue.UNKNOWN))
        {
            return LogicValue.UNKNOWN;
        }
        else
        {
            return LogicValue.HIGH;
        }
    }
}