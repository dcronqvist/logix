using System.Drawing;
using System.Numerics;
using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class GateData : INodeDescriptionData
{
    [NodeDescriptionProperty("Bits", IntMinValue = 2, IntMaxValue = 32)]
    public int DataBits { get; set; } // Setting this to something other than 1 will create a bitwise gate

    public static INodeDescriptionData GetDefault()
    {
        return new GateData()
        {
            DataBits = 2
        };
    }
}

public interface IGateLogic
{
    public string Name { get; }
    public LogicValue GetValueToPush(LogicValue a, LogicValue b);
}

public abstract class LogicGate<TData> : Node<TData> where TData : GateData
{
    public IGateLogic Logic { get; set; }
    private TData _data;

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        for (int i = 0; i < this._data.DataBits; i++)
        {
            yield return new PinConfig($"X{i}", 1, true, new Vector2i(0, i * 2));
        }

        yield return new PinConfig("Y", 1, false, new Vector2i(3, this._data.DataBits - 1));
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        return Enumerable.Empty<(ObservableValue, LogicValue[])>(); // Gates don't have any initial values for any pins
    }

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var pin1 = pins.Get("X0");
        var y = pins.Get("Y");
        LogicValue currVal = pin1.Read().First();
        for (int i = 1; i < this._data.DataBits; i++)
        {
            var p = pins.Get($"X{i}");
            currVal = this.Logic.GetValueToPush(currVal, p.Read().First());
        }

        yield return (y, currVal.Multiple(1), 1);
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override Vector2i GetSize()
    {
        var width = 3;
        var height = this._data.DataBits * 2 - 2;

        if (this.Rotation == 1 || this.Rotation == 3)
        {
            var tmp = width;
            width = height;
            height = tmp;
        }

        return new Vector2i(width, height);
    }

    public override bool IsNodeInRect(RectangleF rect)
    {
        var middle = this.GetMiddleOffset();
        var doubleMiddle = (middle * 2);

        var pos = this.Position.ToVector2(Constants.GRIDSIZE);

        return pos.CreateRect(doubleMiddle).IntersectsWith(rect);
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        // Nothing
        return false;
    }

    public override void RenderSelected(Camera2D camera)
    {
        var pos = this.Position;

        var size = this.GetSize();

        var rect = pos.ToVector2(Constants.GRIDSIZE).CreateRect(size.ToVector2(Constants.GRIDSIZE));

        PrimitiveRenderer.RenderRectangle(rect.Inflate(2), Vector2.Zero, 0f, Constants.COLOR_SELECTED);
    }

    public override void Render(PinCollection pins, Camera2D camera)
    {
        var pos = this.Position;
        var size = this.GetSize();

        var rect = pos.ToVector2(Constants.GRIDSIZE).CreateRect(size.ToVector2(Constants.GRIDSIZE));

        PrimitiveRenderer.RenderRectangleWithBorder(rect, Vector2.Zero, 0f, 1, ColorF.White, ColorF.Black);

        var root = pos.ToVector2(Constants.GRIDSIZE);
        var sizeReal = size.ToVector2(Constants.GRIDSIZE);
        var font = Utilities.GetFont("core.font.default", 8);
        var scale = 0.75f;
        var measure = font.MeasureString(this.Logic.Name, scale);

        var rot = this.Rotation switch
        {
            1 => MathF.PI / 2f,
            3 => MathF.PI / 2f,
            _ => 0
        };

        var offset = this.Rotation switch
        {
            1 => new Vector2(measure.Y / 2f, -measure.X / 2f),
            3 => new Vector2(measure.Y / 2f, -measure.X / 2f),
            _ => -measure / 2f
        };

        TextRenderer.RenderText(Utilities.GetFont("core.font.default", 8), this.Logic.Name, root + sizeReal / 2f + offset, scale, rot, ColorF.Black);

        base.Render(pins, camera);
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

    public LogicValue GetValueToPush(LogicValue a, LogicValue b)
    {
        if (a == LogicValue.Z || b == LogicValue.Z)
        {
            return LogicValue.Z;
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

[ScriptType("AND_GATE"), NodeInfo("AND Gate", "Gates", "core.markdown.logicgate")]
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
        else if (a == LogicValue.Z || b == LogicValue.Z)
        {
            return LogicValue.Z;
        }
        else
        {
            return LogicValue.LOW;
        }
    }
}

[ScriptType("OR_GATE"), NodeInfo("OR Gate", "Gates", "core.markdown.logicgate")]
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
        if (a == LogicValue.Z || b == LogicValue.Z)
        {
            return LogicValue.Z;
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

[ScriptType("XOR_GATE"), NodeInfo("XOR Gate", "Gates", "core.markdown.logicgate")]
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
        if (a == LogicValue.HIGH || b == LogicValue.HIGH)
        {
            return LogicValue.LOW;
        }
        else if (a == LogicValue.Z || b == LogicValue.Z)
        {
            return LogicValue.Z;
        }
        else
        {
            return LogicValue.HIGH;
        }
    }
}

[ScriptType("NOR_GATE"), NodeInfo("NOR Gate", "Gates", "core.markdown.logicgate")]
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
        if (a == LogicValue.Z || b == LogicValue.Z)
        {
            return LogicValue.Z;
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

[ScriptType("NAND_GATE"), NodeInfo("NAND Gate", "Gates", "core.markdown.logicgate")]
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
        if (a == LogicValue.Z || b == LogicValue.Z)
        {
            return LogicValue.Z;
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

[ScriptType("XNOR_GATE"), NodeInfo("XNOR Gate", "Gates", "core.markdown.logicgate")]
public class XNORGate : LogicGate<GateData>
{
    public override IGateLogic GetLogic()
    {
        return new XNORGateLogic();
    }
}