using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public enum PullerType
{
    PullDown,
    PullUp,
}

public class PullerData : INodeDescriptionData
{
    [NodeDescriptionProperty("Puller Type", HelpTooltip = "The type of puller to use.")]
    public PullerType PullerType { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new PullerData()
        {
            PullerType = PullerType.PullDown
        };
    }
}

[ScriptType("PULLER"), NodeInfo("Puller", "Common", "core.markdown.puller")]
public class Puller : BoxNode<PullerData>
{
    public override string Text => $"P{(this._data.PullerType == PullerType.PullDown ? 'D' : 'U')}";
    public override float TextScale => 1f;
    public override bool DisableSelfEvaluation => true;

    private PullerData _data;

    public override void Initialize(PullerData data)
    {
        this._data = data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("pin", 1, true, new Vector2i(0, 1));
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        return this.Evaluate(pins).Select(x => (x.Item1, x.Item2));
    }

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var pin = pins.Get("pin").Read(1).First();

        if (pin.IsUndefined())
        {
            if (this._data.PullerType == PullerType.PullDown)
            {
                yield return (pins.Get("pin"), LogicValue.LOW.Multiple(1), 1);
                yield break;
            }
            else
            {
                yield return (pins.Get("pin"), LogicValue.HIGH.Multiple(1), 1);
                yield break;
            }
        }

        yield return (pins.Get("pin"), pin.Multiple(1), 1);
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        return false; // No interaction
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(3, 2);
    }
}