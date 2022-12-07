using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class NoData : INodeDescriptionData
{
    [NodeDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 256)]
    public int DataBits { get; set; }

    public static INodeDescriptionData GetDefault()
    {
        return new NoData()
        {
            DataBits = 1
        };
    }
}

[ScriptType("TRISTATE_BUFFER"), NodeInfo("TriState Buffer", "Common", "core.markdown.tristatebuffer")]
public class TriStateBuffer : BoxNode<NoData>
{
    public override string Text => "TRI";
    public override float TextScale => 1f;

    private NoData _data;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var pIn = pins.Get("in");
        var pOut = pins.Get("out");
        var pEnabled = pins.Get("enabled");

        if (pEnabled.Read().First() == LogicValue.HIGH)
        {
            yield return (pOut, pIn.Read(), 1);
        }
        else
        {
            yield return (pOut, LogicValue.Z.Multiple(this._data.DataBits), 1);
        }
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("in", this._data.DataBits, true, new Vector2i(0, 1));
        yield return new PinConfig("out", this._data.DataBits, false, new Vector2i(3, 1));
        yield return new PinConfig("enabled", 1, true, new Vector2i(1, 0));
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(3, 2).ApplyRotation(this.Rotation);
    }

    public override void Initialize(NoData data)
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

    // public override string Name => "TSB";
    // public override bool DisplayIOGroupIdentifiers => true;
    // public override bool ShowPropertyWindow => true;

    // private NoData _data;

    // public override INodeDescriptionData GetDescriptionData()
    // {
    //     return this._data;
    // }

    // public override void Initialize(NoData data)
    // {
    //     this.ClearIOs();
    //     this._data = data;

    //     this.RegisterIO("in", data.DataBits, ComponentSide.LEFT);
    //     this.RegisterIO("out", data.DataBits, ComponentSide.RIGHT);
    //     this.RegisterIO("enabled", 1, ComponentSide.TOP);

    //     this.TriggerSizeRecalculation();
    // }

    // public override void PerformLogic()
    // {
    //     var enabled = this.GetIOFromIdentifier("enabled").GetValues().First() == LogicValue.HIGH;
    //     var input = this.GetIOFromIdentifier("in").GetValues();

    //     if (enabled)
    //     {
    //         this.GetIOFromIdentifier("out").Push(input);
    //     }
    //     else
    //     {
    //         this.GetIOFromIdentifier("out").Push(Enumerable.Repeat(LogicValue.Z, this._data.DataBits).ToArray());
    //     }
    // }
}