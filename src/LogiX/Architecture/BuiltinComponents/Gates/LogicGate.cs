using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class GateData : IComponentDescriptionData
{
    public static IComponentDescriptionData GetDefault()
    {
        return new GateData();
    }
}

public interface IGateLogic
{
    public string Name { get; }
    public LogicValue GetValueToPush(LogicValue[] inputs);
}

public abstract class LogicGate<TData> : Component<TData> where TData : IComponentDescriptionData
{
    public IGateLogic Logic { get; set; }
    public override string Name => this.Logic.Name;
    public override bool DisplayIOGroupIdentifiers => false;
    public override bool ShowPropertyWindow => true;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return new GateData();
    }

    public override void Initialize(TData data)
    {
        this.RegisterIO("A", 1, ComponentSide.LEFT);
        this.RegisterIO("B", 1, ComponentSide.LEFT);
        this.RegisterIO("O", 1, ComponentSide.RIGHT);
        this.Logic = this.GetLogic();
    }

    public override void PerformLogic()
    {
        var a = this.GetIOFromIdentifier("A");
        var b = this.GetIOFromIdentifier("B");
        var o = this.GetIOFromIdentifier("O");

        var aVal = a.GetValues().First();
        var bVal = b.GetValues().First();

        o.Push(this.Logic.GetValueToPush(new LogicValue[] { aVal, bVal }));
    }

    public override void SubmitUISelected(Editor editor, int componentIndex)
    {

    }

    public abstract IGateLogic GetLogic();
}


public class ANDGateLogic : IGateLogic
{
    public string Name => "AND";

    public LogicValue GetValueToPush(LogicValue[] inputs)
    {
        if (inputs.Any(v => v == LogicValue.UNDEFINED))
        {
            return LogicValue.UNDEFINED;
        }
        else if (inputs.All(v => v == LogicValue.HIGH))
        {
            return LogicValue.HIGH;
        }
        else
        {
            return LogicValue.LOW;
        }
    }
}

[ScriptType("AND_GATE"), ComponentInfo("AND Gate", "Gates")]
public class ANDGate : LogicGate<GateData>
{
    public override IGateLogic GetLogic()
    {
        return new ANDGateLogic();
    }
}

public class ORGateLogic : IGateLogic
{
    public string Name => "OR";

    public LogicValue GetValueToPush(LogicValue[] inputs)
    {
        if (inputs.Any(v => v == LogicValue.HIGH))
        {
            return LogicValue.HIGH;
        }
        else if (inputs.Any(v => v == LogicValue.UNDEFINED))
        {
            return LogicValue.UNDEFINED;
        }
        else
        {
            return LogicValue.LOW;
        }
    }
}

[ScriptType("OR_GATE"), ComponentInfo("OR Gate", "Gates")]
public class ORGate : LogicGate<GateData>
{
    public override IGateLogic GetLogic()
    {
        return new ORGateLogic();
    }
}

public class XORGateLogic : IGateLogic
{
    public string Name => "XOR";

    public LogicValue GetValueToPush(LogicValue[] inputs)
    {
        if (inputs.Any(v => v == LogicValue.UNDEFINED))
        {
            return LogicValue.UNDEFINED;
        }
        else if (inputs.Count(v => v == LogicValue.HIGH) % 2 == 1)
        {
            return LogicValue.HIGH;
        }
        else
        {
            return LogicValue.LOW;
        }
    }
}

[ScriptType("XOR_GATE"), ComponentInfo("XOR Gate", "Gates")]
public class XORGate : LogicGate<GateData>
{
    public override IGateLogic GetLogic()
    {
        return new XORGateLogic();
    }
}

public class NORGateLogic : IGateLogic
{
    public string Name => "NOR";

    public LogicValue GetValueToPush(LogicValue[] inputs)
    {
        if (inputs.Any(v => v == LogicValue.HIGH))
        {
            return LogicValue.LOW;
        }
        else if (inputs.Any(v => v == LogicValue.UNDEFINED))
        {
            return LogicValue.UNDEFINED;
        }
        else
        {
            return LogicValue.HIGH;
        }
    }
}

[ScriptType("NOR_GATE"), ComponentInfo("NOR Gate", "Gates")]
public class NORGate : LogicGate<GateData>
{
    public override IGateLogic GetLogic()
    {
        return new NORGateLogic();
    }
}

public class NANDGateLogic : IGateLogic
{
    public string Name => "NAND";

    public LogicValue GetValueToPush(LogicValue[] inputs)
    {
        if (inputs.Any(v => v == LogicValue.UNDEFINED))
        {
            return LogicValue.UNDEFINED;
        }
        else if (inputs.All(v => v == LogicValue.HIGH))
        {
            return LogicValue.LOW;
        }
        else
        {
            return LogicValue.HIGH;
        }
    }
}

[ScriptType("NAND_GATE"), ComponentInfo("NAND Gate", "Gates")]
public class NANDGate : LogicGate<GateData>
{
    public override IGateLogic GetLogic()
    {
        return new NANDGateLogic();
    }
}

public class XNORGateLogic : IGateLogic
{
    public string Name => "XNOR";

    public LogicValue GetValueToPush(LogicValue[] inputs)
    {
        if (inputs.Any(v => v == LogicValue.UNDEFINED))
        {
            return LogicValue.UNDEFINED;
        }
        else if (inputs.Count(v => v == LogicValue.HIGH) % 2 == 1)
        {
            return LogicValue.LOW;
        }
        else
        {
            return LogicValue.HIGH;
        }
    }
}

[ScriptType("XNOR_GATE"), ComponentInfo("XNOR Gate", "Gates")]
public class XNORGate : LogicGate<GateData>
{
    public override IGateLogic GetLogic()
    {
        return new XNORGateLogic();
    }
}

[ScriptType("NOT_GATE"), ComponentInfo("NOT Gate", "Gates")]
public class NOTGate : Component<GateData>
{
    public override string Name => "NOT";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => false;

    private GateData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(GateData data)
    {
        this.ClearIOs();
        this._data = data;

        this.RegisterIO("X", 1, ComponentSide.LEFT);
        this.RegisterIO("Y", 1, ComponentSide.RIGHT);
    }

    public override void PerformLogic()
    {
        var x = this.GetIOFromIdentifier("X");
        var y = this.GetIOFromIdentifier("Y");

        var xVal = x.GetValues().First();

        if (xVal == LogicValue.UNDEFINED)
        {
            return;
        }

        y.Push(xVal == LogicValue.HIGH ? LogicValue.LOW : LogicValue.HIGH);
    }

    public override void SubmitUISelected(Editor editor, int componentIndex)
    {

    }
}
