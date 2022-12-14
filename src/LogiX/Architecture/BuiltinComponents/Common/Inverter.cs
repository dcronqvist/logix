using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class InverterData : INodeDescriptionData
{
    [NodeDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 32)]
    public int DataBits { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new InverterData()
        {
            DataBits = 1
        };
    }
}

[ScriptType("INVERTER"), NodeInfo("Inverter", "Common", "core.markdown.inverter")]
public class Inverter : BoxNode<InverterData>
{
    private InverterData _data;

    public override string Text => "INV";
    public override float TextScale => 1f;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var i = pins.Get("in").Read(this._data.DataBits);

        if (i.Any(x => x == LogicValue.Z))
        {
            yield return (pins.Get("out"), LogicValue.Z.Multiple(this._data.DataBits), 1);
        }
        else
        {
            yield return (pins.Get("out"), i.Select(x => x == LogicValue.HIGH ? LogicValue.LOW : LogicValue.HIGH).ToArray(), 1);
        }
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("in", this._data.DataBits, true, new Vector2i(0, 1));
        yield return new PinConfig("out", this._data.DataBits, false, new Vector2i(3, 1));
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(3, 2);
    }

    public override void Initialize(InverterData data)
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