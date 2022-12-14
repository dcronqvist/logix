using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class DividerData : INodeDescriptionData
{
    [NodeDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 32)]
    public int DataBits { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new DividerData()
        {
            DataBits = 1
        };
    }
}

[ScriptType("DIVIDER"), NodeInfo("Divider", "Arithmetic", "core.markdown.divider")]
public class Divider : BoxNode<DividerData>
{
    private DividerData _data;

    public override string Text => "DIV";
    public override float TextScale => 1f;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var dividend = pins.Get("DIVIDEND").Read(this._data.DataBits).Reverse();
        var divisor = pins.Get("DIVISOR").Read(this._data.DataBits).Reverse();

        var quotient = pins.Get("QUOTIENT");
        var remainder = pins.Get("REMAINDER");

        if (dividend.AnyUndefined() || divisor.AnyUndefined())
        {
            yield return (quotient, LogicValue.Z.Multiple(this._data.DataBits), 1);
            yield return (remainder, LogicValue.Z.Multiple(this._data.DataBits), 1);
        }
        else
        {
            var divisorVal = divisor.GetAsUInt();
            var dividendVal = dividend.GetAsUInt();

            if (divisorVal == 0)
            {
                yield return (quotient, LogicValue.LOW.Multiple(this._data.DataBits), 1);
                yield return (remainder, LogicValue.LOW.Multiple(this._data.DataBits), 1);
                yield return (pins.Get("DBZ"), LogicValue.HIGH.Multiple(1), 1);
                yield break;
            }

            var quotientVal = dividendVal / divisorVal;
            var remainderVal = dividendVal % divisorVal;

            yield return (quotient, quotientVal.GetAsLogicValues(this._data.DataBits), 1);
            yield return (remainder, remainderVal.GetAsLogicValues(this._data.DataBits), 1);
            yield return (pins.Get("DBZ"), LogicValue.LOW.Multiple(1), 1);
        }
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("DIVIDEND", this._data.DataBits, true, new Vector2i(0, 1));
        yield return new PinConfig("DIVISOR", this._data.DataBits, true, new Vector2i(0, 2));

        yield return new PinConfig("QUOTIENT", this._data.DataBits, false, new Vector2i(3, 1));
        yield return new PinConfig("REMAINDER", this._data.DataBits, false, new Vector2i(3, 2));

        yield return new PinConfig("DBZ", 1, false, new Vector2i(1, 3));
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(3, 3);
    }

    public override void Initialize(DividerData data)
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