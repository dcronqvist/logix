using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class SRFlipFlopData : INodeDescriptionData
{
    public INodeDescriptionData GetDefault()
    {
        return new SRFlipFlopData();
    }
}

[ScriptType("SRFF"), NodeInfo("SR Flip-Flop", "Memory", "core.markdown.srff")]
public class SRFlipFlop : BoxNode<SRFlipFlopData>
{
    public override string Text => "SR FF";
    public override float TextScale => 1f;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var S = pins.Get("S").Read().First();
        var R = pins.Get("R").Read().First();
        var CLK = pins.Get("CLK").Read().First();

        var Q = pins.Get("Q");
        var Qn = pins.Get("Q'");

        if (CLK == LogicValue.HIGH)
        {
            if (S == LogicValue.HIGH && R == LogicValue.LOW)
            {
                yield return (Q, LogicValue.HIGH.Multiple(1), 1);
                yield return (Qn, LogicValue.LOW.Multiple(1), 1);
            }
            else if (R == LogicValue.HIGH && S == LogicValue.LOW)
            {
                yield return (Q, LogicValue.LOW.Multiple(1), 1);
                yield return (Qn, LogicValue.HIGH.Multiple(1), 1);
            }
        }
    }

    public override INodeDescriptionData GetNodeData()
    {
        return new SRFlipFlopData();
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("S", 1, false, new Vector2i(0, 1));
        yield return new PinConfig("R", 1, false, new Vector2i(0, 2));
        yield return new PinConfig("CLK", 1, true, new Vector2i(0, 3));

        yield return new PinConfig("Q", 1, false, new Vector2i(4, 1));
        yield return new PinConfig("Q'", 1, false, new Vector2i(4, 3));
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(4, 4);
    }

    public override void Initialize(SRFlipFlopData data)
    {
        // Nothing to do here
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