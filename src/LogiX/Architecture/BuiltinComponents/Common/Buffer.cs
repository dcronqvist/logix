using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class BufferData : INodeDescriptionData
{
    [NodeDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 256)]
    public int DataBits { get; set; }

    [NodeDescriptionProperty("Buffer Size", HelpTooltip = "The amount of ticks before the input is visible on the output.", IntMinValue = 1)]
    public int BufferSize { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new BufferData()
        {
            DataBits = 1,
            BufferSize = 1
        };
    }
}

[ScriptType("BUFFER"), NodeInfo("Buffer", "Common", "core.markdown.tristatebuffer")]
public class Buffer : BoxNode<BufferData>
{
    public override string Text => "BUF";
    public override float TextScale => 1f;

    private BufferData _data;
    private Queue<LogicValue[]> _buffer;

    public override void Initialize(BufferData data)
    {
        this._data = data;
        this._buffer = new();
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("in", this._data.DataBits, true, new Vector2i(0, 1));
        yield return new PinConfig("out", this._data.DataBits, false, new Vector2i(this.GetSize().X, 1));
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        yield break;
    }

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var input = pins.Get("in").Read();
        var output = pins.Get("out");

        yield return (output, input, this._data.BufferSize);
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