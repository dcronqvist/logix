using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class GateData : IComponentDescriptionData
{
    public int DataBits { get; set; } // Setting this to something other than 1 will create a bitwise gate

    public static IComponentDescriptionData GetDefault()
    {
        return new GateData()
        {
            DataBits = 1
        };
    }
}

public interface IGateLogic
{
    public string Name { get; }
    public LogicValue GetValueToPush(LogicValue a, LogicValue b);
}

public abstract class LogicGate<TData> : Component<TData> where TData : GateData
{
    public IGateLogic Logic { get; set; }
    public override string Name => this.Logic.Name;
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private TData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return this._data;
    }

    public override void Initialize(TData data)
    {
        this.ClearIOs();
        this._data = data;

        this.RegisterIO("A", data.DataBits, ComponentSide.LEFT);
        this.RegisterIO("B", data.DataBits, ComponentSide.LEFT);
        this.RegisterIO("O", data.DataBits, ComponentSide.RIGHT);

        this.Logic = this.GetLogic();
    }

    public override void PerformLogic()
    {
        var a = this.GetIOFromIdentifier("A");
        var b = this.GetIOFromIdentifier("B");
        var o = this.GetIOFromIdentifier("O");

        var aVal = a.GetValues();
        var bVal = b.GetValues();

        var oVals = Enumerable.Range(0, aVal.Length).Select(i => this.Logic.GetValueToPush(aVal[i], bVal[i])).ToArray();

        o.Push(oVals);
    }

    public override void SubmitUISelected(Editor editor, int componentIndex)
    {
        var id = this.GetUniqueIdentifier();
        var currBits = this._data.DataBits;
        if (ImGui.InputInt($"Data Bits##{id}", ref currBits))
        {
            this._data.DataBits = Math.Clamp(currBits, 1, 32);
            this.Initialize(this._data);
        }
    }

    public abstract IGateLogic GetLogic();
}


public class ANDGateLogic : IGateLogic
{
    public string Name => "AND";

    public LogicValue GetValueToPush(LogicValue a, LogicValue b)
    {
        if (a == LogicValue.UNDEFINED || b == LogicValue.UNDEFINED)
        {
            return LogicValue.UNDEFINED;
        }
        else if (a == LogicValue.HIGH && b == LogicValue.HIGH)
        {
            return LogicValue.HIGH;
        }
        else
        {
            return LogicValue.LOW;
        }
    }
}

[ScriptType("AND_GATE"), ComponentInfo("AND Gate", "Gates", "core.markdown.logicgate")]
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

    public LogicValue GetValueToPush(LogicValue a, LogicValue b)
    {
        if (a == LogicValue.HIGH || b == LogicValue.HIGH)
        {
            return LogicValue.HIGH;
        }
        else if (a == LogicValue.UNDEFINED || b == LogicValue.UNDEFINED)
        {
            return LogicValue.UNDEFINED;
        }
        else
        {
            return LogicValue.LOW;
        }
    }
}

[ScriptType("OR_GATE"), ComponentInfo("OR Gate", "Gates", "core.markdown.logicgate")]
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

    public LogicValue GetValueToPush(LogicValue a, LogicValue b)
    {
        if (a == LogicValue.UNDEFINED || b == LogicValue.UNDEFINED)
        {
            return LogicValue.UNDEFINED;
        }
        else if (a == LogicValue.HIGH && b == LogicValue.LOW)
        {
            return LogicValue.HIGH;
        }
        else if (a == LogicValue.LOW && b == LogicValue.HIGH)
        {
            return LogicValue.HIGH;
        }
        else
        {
            return LogicValue.LOW;
        }
    }
}

[ScriptType("XOR_GATE"), ComponentInfo("XOR Gate", "Gates", "core.markdown.logicgate")]
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

    public LogicValue GetValueToPush(LogicValue a, LogicValue b)
    {
        if (a == LogicValue.UNDEFINED || b == LogicValue.UNDEFINED)
        {
            return LogicValue.UNDEFINED;
        }
        else if (a == LogicValue.HIGH || b == LogicValue.HIGH)
        {
            return LogicValue.LOW;
        }
        else
        {
            return LogicValue.HIGH;
        }
    }
}

[ScriptType("NOR_GATE"), ComponentInfo("NOR Gate", "Gates", "core.markdown.logicgate")]
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

    public LogicValue GetValueToPush(LogicValue a, LogicValue b)
    {
        if (a == LogicValue.UNDEFINED || b == LogicValue.UNDEFINED)
        {
            return LogicValue.UNDEFINED;
        }
        else if (a == LogicValue.HIGH && b == LogicValue.HIGH)
        {
            return LogicValue.LOW;
        }
        else
        {
            return LogicValue.HIGH;
        }
    }
}

[ScriptType("NAND_GATE"), ComponentInfo("NAND Gate", "Gates", "core.markdown.logicgate")]
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

    public LogicValue GetValueToPush(LogicValue a, LogicValue b)
    {
        if (a == LogicValue.UNDEFINED || b == LogicValue.UNDEFINED)
        {
            return LogicValue.UNDEFINED;
        }
        else if (a == LogicValue.HIGH && b == LogicValue.LOW)
        {
            return LogicValue.LOW;
        }
        else if (a == LogicValue.LOW && b == LogicValue.HIGH)
        {
            return LogicValue.LOW;
        }
        else
        {
            return LogicValue.HIGH;
        }
    }
}

[ScriptType("XNOR_GATE"), ComponentInfo("XNOR Gate", "Gates", "core.markdown.logicgate")]
public class XNORGate : LogicGate<GateData>
{
    public override IGateLogic GetLogic()
    {
        return new XNORGateLogic();
    }
}

[ScriptType("NOT_GATE"), ComponentInfo("NOT Gate", "Gates", "core.markdown.logicgate")]
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
