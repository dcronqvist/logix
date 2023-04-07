using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class RegisterData : INodeDescriptionData
{
    [NodeDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 32)]
    public int DataBits { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new RegisterData()
        {
            DataBits = 8
        };
    }
}

[ScriptType("REGISTER"), NodeInfo("Register", "Memory", "logix_core:docs/components/register.md")]
public class Register : BoxNode<RegisterData>
{
    public override string Text => this._currV.ToString($"X{(int)Math.Ceiling(this._data.DataBits / 4f)}");
    public override float TextScale => 2f;

    private RegisterData _data;

    private LogicValue _prevCLK = LogicValue.LOW;
    private uint _currV = 0;
    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var D = pins.Get("D").Read(this._data.DataBits);
        var WE = pins.Get("WE").Read(1).First();
        var CLK = pins.Get("CLK").Read(1).First();
        var R = pins.Get("R").Read(1).First();

        var Q = pins.Get("Q");

        if (R == LogicValue.HIGH)
        {
            _prevCLK = CLK;
            _currV = 0;
            yield return (Q, LogicValue.LOW.Multiple(this._data.DataBits), 1);
            yield break;
        }

        var prevCLK = _prevCLK;
        _prevCLK = CLK;
        if (CLK == LogicValue.HIGH && prevCLK == LogicValue.LOW && WE == LogicValue.HIGH)
        {
            _currV = D.Reverse().GetAsUInt();
            yield return (Q, D, 1);
        }
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        var size = this.GetSize();

        yield return new PinConfig("D", this._data.DataBits, false, new Vector2i(0, 1));
        yield return new PinConfig("WE", 1, false, new Vector2i(0, 2));
        yield return new PinConfig("CLK", 1, true, new Vector2i(0, 3));

        yield return new PinConfig("Q", this._data.DataBits, true, new Vector2i(size.X, 1));
        yield return new PinConfig("R", 1, true, new Vector2i(1, size.Y));
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(4, 4);
    }

    public override void Initialize(RegisterData data)
    {
        this._data = data;
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        return false;
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        yield return (pins.Get("Q"), this._currV.GetAsLogicValues(this._data.DataBits));
    }
}