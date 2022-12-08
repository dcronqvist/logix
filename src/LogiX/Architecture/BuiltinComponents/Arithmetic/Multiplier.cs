using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class MultiplierData : INodeDescriptionData
{
    [NodeDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 32)]
    public int DataBits { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new MultiplierData()
        {
            DataBits = 1
        };
    }
}

[ScriptType("MULTIPLIER"), NodeInfo("Multiplier", "Arithmetic", "core.markdown.multiplier")]
public class Multiplier : BoxNode<MultiplierData>
{
    private MultiplierData _data;

    public override string Text => "MUL";
    public override float TextScale => 1f;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var A = pins.Get("A").Read();
        var B = pins.Get("B").Read();
        var CIN = pins.Get("CIN").Read();

        var P = pins.Get("P");
        var COUT = pins.Get("COUT");

        if (A.AnyUndefined() || B.AnyUndefined() || CIN.AnyUndefined())
        {
            yield return (P, LogicValue.Z.Multiple(this._data.DataBits), 1);
            yield return (COUT, LogicValue.Z.Multiple(this._data.DataBits), 1);
        }
        else
        {
            uint product = A.Reverse().GetAsUInt() * B.Reverse().GetAsUInt() + CIN.Reverse().GetAsUInt();
            uint p = product & (uint)((1 << this._data.DataBits) - 1);
            uint cout = (product >> this._data.DataBits) & (uint)((1 << this._data.DataBits) - 1);

            yield return (P, p.GetAsLogicValues(this._data.DataBits), 1);
            yield return (COUT, cout.GetAsLogicValues(this._data.DataBits), 1);
        }
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("A", this._data.DataBits, true, new Vector2i(0, 1));
        yield return new PinConfig("B", this._data.DataBits, true, new Vector2i(0, 2));

        yield return new PinConfig("CIN", 1, true, new Vector2i(1, 0));
        yield return new PinConfig("COUT", this._data.DataBits, false, new Vector2i(1, 3));

        yield return new PinConfig("P", this._data.DataBits, false, new Vector2i(3, 1));
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(3, 3);
    }

    public override void Initialize(MultiplierData data)
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