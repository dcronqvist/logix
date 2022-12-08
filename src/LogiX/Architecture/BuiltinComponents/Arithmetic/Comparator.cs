using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class ComparatorData : INodeDescriptionData
{
    [NodeDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 32)]
    public int DataBits { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new ComparatorData()
        {
            DataBits = 1
        };
    }
}

[ScriptType("COMPARATOR"), NodeInfo("Comparator", "Arithmetic", "core.markdown.comparator")]
public class Comparator : BoxNode<ComparatorData>
{
    private ComparatorData _data;

    public override string Text => "CMP";
    public override float TextScale => 1f;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var A = pins.Get("A").Read();
        var B = pins.Get("B").Read();

        var eq = pins.Get("A=B");
        var lt = pins.Get("A<B");
        var gt = pins.Get("A>B");

        if (A.AnyUndefined() || B.AnyUndefined())
        {
            yield return (eq, LogicValue.Z.Multiple(this._data.DataBits), 1);
            yield return (lt, LogicValue.Z.Multiple(this._data.DataBits), 1);
            yield return (gt, LogicValue.Z.Multiple(this._data.DataBits), 1);
        }
        else
        {
            var eqVal = A.SequenceEqual(B) ? LogicValue.HIGH : LogicValue.LOW;
            var ltVal = (A.GetAsUInt() < B.GetAsUInt()) ? LogicValue.HIGH : LogicValue.LOW;
            var gtVal = (A.GetAsUInt() > B.GetAsUInt()) ? LogicValue.HIGH : LogicValue.LOW;

            yield return (eq, eqVal.Multiple(1), 1);
            yield return (lt, ltVal.Multiple(1), 1);
            yield return (gt, gtVal.Multiple(1), 1);
        }
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("A", this._data.DataBits, true, new Vector2i(0, 1));
        yield return new PinConfig("B", this._data.DataBits, true, new Vector2i(0, 3));

        yield return new PinConfig("A=B", 1, false, new Vector2i(3, 1));
        yield return new PinConfig("A<B", 1, false, new Vector2i(3, 2));
        yield return new PinConfig("A>B", 1, false, new Vector2i(3, 3));
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(3, 4);
    }

    public override void Initialize(ComparatorData data)
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