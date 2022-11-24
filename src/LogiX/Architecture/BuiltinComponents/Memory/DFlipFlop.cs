using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class DFlipFlopData : IComponentDescriptionData
{
    public static IComponentDescriptionData GetDefault()
    {
        return new DFlipFlopData();
    }
}

[ScriptType("DFLIPFLOP"), ComponentInfo("D Flip Flop", "Memory", "core.markdown.dflipflop")]
public class DFlipFlop : Component<DFlipFlopData>
{
    public override string Name => "D Latch";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => false;

    private DFlipFlopData _data;
    private LogicValue _currentState;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(DFlipFlopData data)
    {
        this.ClearIOs();
        this._data = data;

        this.RegisterIO("D", 1, ComponentSide.LEFT, "data");
        this.RegisterIO(">", 1, ComponentSide.LEFT, "clk");
        this.RegisterIO("R", 1, ComponentSide.LEFT, "reset");

        this.RegisterIO("Q", 1, ComponentSide.RIGHT);
        this.RegisterIO("!Q", 1, ComponentSide.RIGHT);

        this.TriggerSizeRecalculation();
        this._currentState = LogicValue.UNDEFINED;
    }

    private LogicValue previousClk;
    public override void PerformLogic()
    {
        var data = this.GetIOFromIdentifier("D").GetValues().First();
        var clk = this.GetIOFromIdentifier(">").GetValues().First();
        var reset = this.GetIOFromIdentifier("R").GetValues().First();

        var q = this.GetIOFromIdentifier("Q");
        var qNot = this.GetIOFromIdentifier("!Q");

        if (data == LogicValue.UNDEFINED || clk == LogicValue.UNDEFINED || reset == LogicValue.UNDEFINED)
        {
            return;
        }

        if (clk == LogicValue.HIGH && previousClk == LogicValue.LOW)
        {
            this._currentState = data;
        }

        if (reset == LogicValue.HIGH)
        {
            this._currentState = LogicValue.LOW;
        }
        q.Push(this._currentState);
        qNot.Push(this._currentState == LogicValue.HIGH ? LogicValue.LOW : LogicValue.HIGH);
        previousClk = clk;
    }
}