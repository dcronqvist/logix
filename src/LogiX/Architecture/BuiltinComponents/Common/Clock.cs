using ImGuiNET;
using LogiX;
using LogiX.Architecture;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class ClockData : INodeDescriptionData
{
    [NodeDescriptionProperty("High Duration", HelpTooltip = "Amount of ticks the clock will remain HIGH", IntMinValue = 1)]
    public int HighDuration { get; set; }

    [NodeDescriptionProperty("Low Duration", HelpTooltip = "Amount of ticks the clock will remain LOW", IntMinValue = 1)]
    public int LowDuration { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new ClockData
        {
            HighDuration = 100,
            LowDuration = 100
        };
    }
}

[ScriptType("CLOCK"), NodeInfo("Clock", "Common", "logix_core:docs/components/clock.md")]
public class Clock : BoxNode<ClockData>
{
    public override string Text => "CLK";
    public override float TextScale => 1f;

    private ClockData _data;

    public override void Initialize(ClockData data)
    {
        this._data = data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("Q", 1, true, new Vector2i(this.GetSize().X, 1));
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        yield return (pins.Get("Q"), LogicValue.LOW.Multiple(1));
    }

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var q = pins.Get("Q");

        if (q.Read(1).First() == LogicValue.LOW)
        {
            yield return (q, LogicValue.HIGH.Multiple(1), this._data.LowDuration);
        }
        else
        {
            yield return (q, LogicValue.LOW.Multiple(1), this._data.HighDuration);
        }
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        return false;
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(3, 2);
    }
}