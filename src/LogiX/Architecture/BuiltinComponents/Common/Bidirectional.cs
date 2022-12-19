using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public enum BidirectionalMode : int
{
    SeparatePins = 0,
    SinglePin = 1
}

public class BidirectionalData : INodeDescriptionData
{
    [NodeDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 256)]
    public int Bits { get; set; }

    [NodeDescriptionProperty("Mode", HelpTooltip = "The mode of the bidirectional component.")]
    public BidirectionalMode Mode { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new BidirectionalData()
        {
            Bits = 1,
            Mode = BidirectionalMode.SinglePin
        };
    }
}

[ScriptType("BIDIRECTIONAL"), NodeInfo("Bidirectional", "Common", "core.markdown.template")]
public class Bidirectional : BoxNode<BidirectionalData>
{
    public override string Text => "DIR";
    public override float TextScale => 1f;

    private BidirectionalData _data;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var a = pins.Get("A").Read(this._data.Bits);
        var b = pins.Get("B").Read(this._data.Bits);

        if (this._data.Mode == BidirectionalMode.SinglePin)
        {
            var dir = pins.Get("DIR").Read(1).First();

            if (dir == LogicValue.Z)
            {
                yield break;
            }

            if (dir == LogicValue.HIGH)
            {
                // B -> A
                yield return (pins.Get("A"), b, 1);
            }
            else
            {
                // A -> B
                yield return (pins.Get("B"), a, 1);
            }
        }
        else
        {
            var aToB = pins.Get("A->B").Read(1).First().GetAsBool();
            var bToA = pins.Get("B->A").Read(1).First().GetAsBool();

            if (aToB && bToA)
            {
                // Not allowed
                yield break;
            }

            if (aToB)
            {
                // A -> B
                yield return (pins.Get("B"), a, 1);
            }

            if (bToA)
            {
                // B -> A
                yield return (pins.Get("A"), b, 1);
            }
        }
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("A", this._data.Bits, true, new Vector2i(0, 1));
        yield return new PinConfig("B", this._data.Bits, true, new Vector2i(3, 1));

        if (this._data.Mode == BidirectionalMode.SinglePin)
        {
            yield return new PinConfig("DIR", 1, true, new Vector2i(1, 0));
        }
        else
        {
            yield return new PinConfig("A->B", 1, true, new Vector2i(1, 0));
            yield return new PinConfig("B->A", 1, true, new Vector2i(2, 0));
        }
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(3, 2);
    }

    public override void Initialize(BidirectionalData data)
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