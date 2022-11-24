using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class BufferData : IComponentDescriptionData
{
    [ComponentDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 256)]
    public int DataBits { get; set; }

    [ComponentDescriptionProperty("Buffer Size", HelpTooltip = "The amount of ticks before the input is visible on the output.", IntMinValue = 1)]
    public int BufferSize { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new BufferData()
        {
            DataBits = 1,
            BufferSize = 1
        };
    }
}

[ScriptType("BUFFER"), ComponentInfo("Buffer", "Common", "core.markdown.tristatebuffer")]
public class Buffer : Component<BufferData>
{
    public override string Name => "BUF";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private BufferData _data;
    private Queue<LogicValue[]> _buffer;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return this._data;
    }

    public override void Initialize(BufferData data)
    {
        this.ClearIOs();
        this._data = data;

        this.RegisterIO("in", data.DataBits, ComponentSide.LEFT);
        this.RegisterIO("out", data.DataBits, ComponentSide.RIGHT);

        this._buffer = new();
    }

    public override void PerformLogic()
    {
        var input = this.GetIOFromIdentifier("in").GetValues();
        var output = this.GetIOFromIdentifier("out");

        this._buffer.Enqueue(input);

        if (this._buffer.Count == this._data.BufferSize)
        {
            output.Push(this._buffer.Dequeue());
        }
        else
        {
            output.Push(Enumerable.Repeat(LogicValue.UNDEFINED, this._data.DataBits).ToArray());
        }
    }
}