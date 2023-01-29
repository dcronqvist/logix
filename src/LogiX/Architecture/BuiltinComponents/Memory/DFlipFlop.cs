using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class DFlipFlopData : INodeDescriptionData
{
    public INodeDescriptionData GetDefault()
    {
        return new DFlipFlopData();
    }
}

[ScriptType("DFLIPFLOP"), NodeInfo("D Flip Flop", "Memory", "core.markdown.dflipflop")]
public class DFlipFlop : BoxNode<DFlipFlopData>
{
    public override string Text => "D FF";
    public override float TextScale => 1f;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var D = pins.Get("D").Read(1).First();
        var CLK = pins.Get("CLK").Read(1).First();
        var AR = pins.Get("AR").Read(1).First();

        var Q = pins.Get("Q");
        var Qn = pins.Get("Q'");

        if (AR == LogicValue.HIGH)
        {
            yield return (Q, LogicValue.LOW.Multiple(1), 1);
            yield return (Qn, LogicValue.HIGH.Multiple(1), 1);
        }

        if (CLK == LogicValue.HIGH)
        {
            if (D == LogicValue.HIGH)
            {
                yield return (Q, LogicValue.HIGH.Multiple(1), 1);
                yield return (Qn, LogicValue.LOW.Multiple(1), 1);
            }
            else if (D == LogicValue.LOW)
            {
                yield return (Q, LogicValue.LOW.Multiple(1), 1);
                yield return (Qn, LogicValue.HIGH.Multiple(1), 1);
            }
        }
    }

    public override INodeDescriptionData GetNodeData()
    {
        return new DFlipFlopData();
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("D", 1, false, new Vector2i(0, 1));
        yield return new PinConfig("CLK", 1, true, new Vector2i(0, 2));

        yield return new PinConfig("Q", 1, false, new Vector2i(3, 1));
        yield return new PinConfig("Q'", 1, false, new Vector2i(3, 2));

        // Asynchronous reset
        yield return new PinConfig("AR", 1, true, new Vector2i(1, 3));
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(3, 3);
    }

    public override void Initialize(DFlipFlopData data)
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