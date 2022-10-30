using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class NoData : IComponentDescriptionData
{
    public static IComponentDescriptionData GetDefault()
    {
        return new NoData();
    }
}

[ScriptType("TRISTATE_BUFFER"), ComponentInfo("TriState Buffer", "Wiring")]
public class TriStateBuffer : Component<NoData>
{
    public override string Name => "TSB";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => false;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return NoData.GetDefault();
    }

    public override void Initialize(NoData data)
    {
        this.ClearIOs();

        this.RegisterIO("in", 1, ComponentSide.LEFT);
        this.RegisterIO("out", 1, ComponentSide.RIGHT);
        this.RegisterIO("enabled", 1, ComponentSide.TOP);
    }

    public override void PerformLogic()
    {
        var enabled = this.GetIOFromIdentifier("enabled").GetValues().First() == LogicValue.HIGH;
        var input = this.GetIOFromIdentifier("in").GetValues().First();

        if (enabled)
        {
            this.GetIOFromIdentifier("out").Push(input);
        }
        else
        {
            this.GetIOFromIdentifier("out").Push(LogicValue.UNDEFINED);
        }

        this.TriggerSizeRecalculation();
    }

    public override void SubmitUISelected(int componentIndex)
    {
        // Nothing
    }
}