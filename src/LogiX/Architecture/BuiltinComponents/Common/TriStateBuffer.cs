using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class NoData : IComponentDescriptionData
{
    [ComponentDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 256)]
    public int DataBits { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new NoData()
        {
            DataBits = 1
        };
    }
}

[ScriptType("TRISTATE_BUFFER"), ComponentInfo("TriState Buffer", "Common", "core.markdown.tristatebuffer")]
public class TriStateBuffer : Component<NoData>
{
    public override string Name => "TSB";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private NoData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return this._data;
    }

    public override void Initialize(NoData data)
    {
        this.ClearIOs();
        this._data = data;

        this.RegisterIO("in", data.DataBits, ComponentSide.LEFT);
        this.RegisterIO("out", data.DataBits, ComponentSide.RIGHT);
        this.RegisterIO("enabled", 1, ComponentSide.TOP);

        this.TriggerSizeRecalculation();
    }

    public override void PerformLogic()
    {
        var enabled = this.GetIOFromIdentifier("enabled").GetValues().First() == LogicValue.HIGH;
        var input = this.GetIOFromIdentifier("in").GetValues();

        if (enabled)
        {
            this.GetIOFromIdentifier("out").Push(input);
        }
        else
        {
            this.GetIOFromIdentifier("out").Push(Enumerable.Repeat(LogicValue.UNDEFINED, this._data.DataBits).ToArray());
        }
    }
}