using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public enum ShiftDirection
{
    LEFT = 0,
    RIGHT = 1
}

public class ShifterData : INodeDescriptionData
{
    [NodeDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 32)]
    public int DataBits { get; set; }

    [NodeDescriptionProperty("Direction")]
    public ShiftDirection Direction { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new ShifterData()
        {
            DataBits = 4,
            Direction = ShiftDirection.LEFT
        };
    }
}

[ScriptType("SHIFTER"), NodeInfo("Shifter", "Arithmetic", "core.markdown.shifter")]
public class Shifter : BoxNode<ShifterData>
{
    private ShifterData _data;

    public override string Text => "SHIFT";
    public override float TextScale => 1f;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var X = pins.Get("X").Read(this._data.DataBits).Reverse();
        var IN = pins.Get("IN").Read(1).First();
        var Y = pins.Get("Y");

        if (X.AnyUndefined() || IN.IsUndefined())
        {
            yield return (Y, LogicValue.Z.Multiple(this._data.DataBits), 1);
        }
        else
        {
            var x = X.GetAsUInt();
            uint i = IN == LogicValue.HIGH ? 1u : 0u;

            uint y = this._data.Direction == ShiftDirection.LEFT ? (uint)(x << 1) | i : (uint)(x >> 1) | (uint)(i << (this._data.DataBits - 1));

            yield return (Y, y.GetAsLogicValues(this._data.DataBits), 1);
        }
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("X", this._data.DataBits, true, new Vector2i(0, 1));
        yield return new PinConfig("Y", this._data.DataBits, false, new Vector2i(4, 1));

        yield return new PinConfig("IN", 1, true, new Vector2i(1, 0));
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(4, 2);
    }

    public override void Initialize(ShifterData data)
    {
        this._data = data;
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        return false;
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        yield break;
    }
}