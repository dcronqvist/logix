using System.Drawing;
using System.Numerics;
using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public enum GateBehaviour
{
    SingleOutput = 0,
    Bitwise = 1
}

public class GateData : INodeDescriptionData
{
    [NodeDescriptionProperty("Bits", IntMinValue = 2, IntMaxValue = 32)]
    public int DataBits { get; set; } // Setting this to something other than 1 will create a bitwise gate

    [NodeDescriptionProperty("Behaviour")]
    public GateBehaviour Behaviour { get; set; }

    [NodeDescriptionProperty("Pin Mode")]
    public PinModeMulti PinMode { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new GateData()
        {
            DataBits = 2,
            Behaviour = GateBehaviour.SingleOutput,
            PinMode = PinModeMulti.Separate
        };
    }
}

public interface IGateLogic
{
    public string Name { get; }
    public LogicValue GetValueToPush(LogicValue[] inputs);
}

public abstract class LogicGate<TData> : BoxNode<TData> where TData : GateData
{
    public IGateLogic Logic { get; set; }
    public override string Text => this.Logic.Name;
    public override float TextScale => 1f;
    private TData _data;

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        if (this._data.Behaviour == GateBehaviour.SingleOutput)
        {
            if (this._data.PinMode == PinModeMulti.Separate)
            {
                for (int i = 0; i < this._data.DataBits; i++)
                {
                    yield return new PinConfig($"X{i}", 1, true, new Vector2i(0, i + 1));
                }
            }
            else
            {
                yield return new PinConfig("X", this._data.DataBits, true, new Vector2i(0, this.GetSize().Y / 2));
            }

            yield return new PinConfig("Y", 1, false, new Vector2i(3, this.GetSize().Y / 2));
        }
        else
        {
            if (this._data.PinMode == PinModeMulti.Separate)
            {
                for (int i = 0; i < this._data.DataBits; i++)
                {
                    yield return new PinConfig($"A{i}", 1, true, new Vector2i(0, i + 1));
                    yield return new PinConfig($"B{i}", 1, true, new Vector2i(0, (i + this._data.DataBits) + 1));
                }

                yield return new PinConfig("Y", this._data.DataBits, false, new Vector2i(3, this.GetSize().Y / 2));
            }
            else
            {
                yield return new PinConfig("A", this._data.DataBits, true, new Vector2i(0, 1));
                yield return new PinConfig("B", this._data.DataBits, true, new Vector2i(0, 2));
                yield return new PinConfig("Y", this._data.DataBits, false, new Vector2i(3, this.GetSize().Y / 2));
            }
        }
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        return Enumerable.Empty<(ObservableValue, LogicValue[])>(); // Gates don't have any initial values for any pins
    }

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        if (this._data.Behaviour == GateBehaviour.SingleOutput)
        {
            if (this._data.PinMode == PinModeMulti.Separate)
            {
                var y = pins.Get("Y");
                var values = Enumerable.Range(0, this._data.DataBits).Select(i => pins.Get($"X{i}").Read(1).First()).ToArray();

                yield return (y, this.Logic.GetValueToPush(values).Multiple(1), 1);
            }
            else
            {
                var y = pins.Get("Y");
                var values = pins.Get("X").Read(this._data.DataBits).ToArray();

                yield return (y, this.Logic.GetValueToPush(values).Multiple(1), 1);
            }
        }
        else
        {
            if (this._data.PinMode == PinModeMulti.Separate)
            {
                var y = pins.Get("Y");
                var valuesA = Enumerable.Range(0, this._data.DataBits).Select(i => pins.Get($"A{i}").Read(1).First()).ToArray();
                var valuesB = Enumerable.Range(0, this._data.DataBits).Select(i => pins.Get($"B{i}").Read(1).First()).ToArray();

                var valuesY = Enumerable.Range(0, this._data.DataBits).Select(i => this.Logic.GetValueToPush(new[] { valuesA[i], valuesB[i] })).ToArray();

                yield return (y, valuesY, 1);
            }
            else
            {
                var y = pins.Get("Y");
                var valuesA = pins.Get("A").Read(this._data.DataBits).ToArray();
                var valuesB = pins.Get("B").Read(this._data.DataBits).ToArray();

                var valuesY = Enumerable.Range(0, this._data.DataBits).Select(i => this.Logic.GetValueToPush(new[] { valuesA[i], valuesB[i] })).ToArray();

                yield return (y, valuesY, 1);
            }
        }
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override Vector2i GetSize()
    {
        var width = 3;
        var height = 3;

        if (this._data.Behaviour == GateBehaviour.SingleOutput)
        {
            if (this._data.PinMode == PinModeMulti.Separate)
            {
                height = this._data.DataBits + 1;
            }
            else
            {
                height = 2;
            }
        }
        else
        {
            if (this._data.PinMode == PinModeMulti.Separate)
            {
                height = this._data.DataBits * 2 + 1;
            }
            else
            {
                height = 3;
            }
        }

        return new Vector2i(width, height);
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        // Nothing
        return false;
    }

    public override void Initialize(TData data)
    {
        this._data = data;
        this.Logic = this.GetLogic();
    }

    public abstract IGateLogic GetLogic();
}

public class ANDGateLogic : IGateLogic
{
    public string Name => "AND";

    public LogicValue GetValueToPush(LogicValue[] inputs)
    {
        if (inputs.Any(x => x == LogicValue.Z))
        {
            return LogicValue.Z;
        }
        else if (inputs.All(x => x == LogicValue.HIGH))
        {
            return LogicValue.HIGH;
        }
        else
        {
            return LogicValue.LOW;
        }
    }
}

[ScriptType("AND_GATE"), NodeInfo("AND Gate", "Gates", "logix_core:docs/components/logicgate.md")]
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
        if (inputs.Any(x => x == LogicValue.HIGH))
        {
            return LogicValue.HIGH;
        }
        else if (inputs.Any(x => x == LogicValue.Z))
        {
            return LogicValue.Z;
        }
        else
        {
            return LogicValue.LOW;
        }
    }
}

[ScriptType("OR_GATE"), NodeInfo("OR Gate", "Gates", "logix_core:docs/components/logicgate.md")]
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
        if (inputs.Count(x => x == LogicValue.HIGH) % 2 == 1)
        {
            return LogicValue.HIGH;
        }
        else if (inputs.Any(x => x == LogicValue.Z))
        {
            return LogicValue.Z;
        }
        else
        {
            return LogicValue.LOW;
        }
    }
}

[ScriptType("XOR_GATE"), NodeInfo("XOR Gate", "Gates", "logix_core:docs/components/logicgate.md")]
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
        if (inputs.Any(x => x == LogicValue.HIGH))
        {
            return LogicValue.LOW;
        }
        else if (inputs.Any(x => x == LogicValue.Z))
        {
            return LogicValue.Z;
        }
        else
        {
            return LogicValue.HIGH;
        }
    }
}

[ScriptType("NOR_GATE"), NodeInfo("NOR Gate", "Gates", "logix_core:docs/components/logicgate.md")]
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
        if (inputs.Any(x => x == LogicValue.Z))
        {
            return LogicValue.Z;
        }
        else if (inputs.All(x => x == LogicValue.HIGH))
        {
            return LogicValue.LOW;
        }
        else
        {
            return LogicValue.HIGH;
        }
    }
}

[ScriptType("NAND_GATE"), NodeInfo("NAND Gate", "Gates", "logix_core:docs/components/logicgate.md")]
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
        if (inputs.Count(x => x == LogicValue.HIGH) % 2 == 1)
        {
            return LogicValue.LOW;
        }
        else if (inputs.Any(x => x == LogicValue.Z))
        {
            return LogicValue.Z;
        }
        else
        {
            return LogicValue.HIGH;
        }
    }
}

[ScriptType("XNOR_GATE"), NodeInfo("XNOR Gate", "Gates", "logix_core:docs/components/logicgate.md")]
public class XNORGate : LogicGate<GateData>
{
    public override IGateLogic GetLogic()
    {
        return new XNORGateLogic();
    }
}