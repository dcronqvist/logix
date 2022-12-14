using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class CounterData : INodeDescriptionData
{
    [NodeDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 32)]
    public int DataBits { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new CounterData()
        {
            DataBits = 4,
        };
    }
}

[ScriptType("COUNTER"), NodeInfo("Counter", "Memory", "core.markdown.counter")]
public class Counter : BoxNode<CounterData>
{
    public override string Text => "CNT";
    public override float TextScale => 1f;

    private CounterData _data;

    private LogicValue _prevCLK = LogicValue.LOW;
    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var D = pins.Get("D").Read(this._data.DataBits);
        var CLK = pins.Get("CLK").Read(1).First();

        var EN = pins.Get("EN").Read(1).First();
        var CNT = pins.Get("CNT").Read(1).First();
        var LOAD = pins.Get("LOAD").Read(1).First();

        var Q = pins.Get("Q");
        var R = pins.Get("R").Read(1).First();

        if (R == LogicValue.HIGH)
        {
            yield return (Q, LogicValue.LOW.Multiple(this._data.DataBits), 1);
        }
        else if (this._prevCLK == LogicValue.LOW && CLK == LogicValue.HIGH && EN == LogicValue.HIGH)
        {
            if (LOAD == LogicValue.HIGH)
            {
                var dVal = D.Reverse().GetAsUInt();
                yield return (Q, dVal.GetAsLogicValues(this._data.DataBits), 1);
            }
            else if (CNT == LogicValue.HIGH)
            {
                var qVal = Q.Read(this._data.DataBits).Reverse().GetAsUInt();
                qVal++;
                yield return (Q, qVal.GetAsLogicValues(this._data.DataBits), 1);
            }
            else if (CNT == LogicValue.LOW)
            {
                var qVal = Q.Read(this._data.DataBits).Reverse().GetAsUInt();
                qVal--;
                yield return (Q, qVal.GetAsLogicValues(this._data.DataBits), 1);
            }
        }

        this._prevCLK = CLK;
        yield break;
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("D", this._data.DataBits, false, new Vector2i(0, 1));
        yield return new PinConfig("CLK", 1, true, new Vector2i(0, 2));

        yield return new PinConfig("EN", 1, false, new Vector2i(1, 0));
        yield return new PinConfig("CNT", 1, false, new Vector2i(2, 0));
        yield return new PinConfig("LOAD", 1, false, new Vector2i(3, 0));

        yield return new PinConfig("Q", this._data.DataBits, false, new Vector2i(this.GetSize().X, 1));
        yield return new PinConfig("R", 1, true, new Vector2i(1, this.GetSize().Y));
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(4, 4);
    }

    public override void Initialize(CounterData data)
    {
        this._data = data;
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        return false;
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        yield return (pins.Get("Q"), LogicValue.LOW.Multiple(this._data.DataBits));
    }
}