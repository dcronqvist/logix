using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class RegisterData : IComponentDescriptionData
{
    public int DataBits { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new RegisterData()
        {
            DataBits = 8
        };
    }
}

[ScriptType("REGISTER"), ComponentInfo("Register", "Memory")]
public class Register : Component<RegisterData>
{
    public override string Name => $"{this._currentState.Reverse().GetAsHexString()}";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => false;

    private RegisterData _data;
    private LogicValue[] _currentState;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(RegisterData data)
    {
        this.ClearIOs();
        this._data = data;
        this._currentState = Enumerable.Repeat(LogicValue.UNDEFINED, data.DataBits).ToArray();

        this.RegisterIO("D", data.DataBits, ComponentSide.LEFT, "data");
        this.RegisterIO(">", 1, ComponentSide.LEFT, "clk");
        this.RegisterIO("EN", 1, ComponentSide.TOP, "enable");
        this.RegisterIO("R", 1, ComponentSide.TOP, "reset");

        this.RegisterIO("Q", data.DataBits, ComponentSide.RIGHT);

        this.TriggerSizeRecalculation();
    }

    private LogicValue previousClk;
    public override void PerformLogic()
    {
        var data = this.GetIOFromIdentifier("D").GetValues();
        var clk = this.GetIOFromIdentifier(">").GetValues().First();
        var enable = this.GetIOFromIdentifier("EN").GetValues().First();
        var reset = this.GetIOFromIdentifier("R").GetValues().First();

        var q = this.GetIOFromIdentifier("Q");

        if (enable == LogicValue.HIGH)
        {
            if (clk == LogicValue.HIGH && previousClk == LogicValue.LOW)
            {
                _currentState = data;
            }
        }

        if (reset == LogicValue.HIGH)
        {
            this._currentState = Enumerable.Repeat(LogicValue.LOW, this._data.DataBits).ToArray();
        }

        if (clk == LogicValue.UNDEFINED || enable == LogicValue.UNDEFINED || reset == LogicValue.UNDEFINED)
        {
            this._currentState = Enumerable.Repeat(LogicValue.UNDEFINED, this._data.DataBits).ToArray();
            return;
        }

        q.Push(this._currentState);
        previousClk = clk;
    }

    public override void SubmitUISelected(Editor editor, int componentIndex)
    {
        // Nothing yet.
        var id = this.GetUniqueIdentifier();
        var databits = this._data.DataBits;
        if (ImGui.InputInt($"Data Bits##{id}", ref databits, 1, 1))
        {
            this._data.DataBits = databits;
            this.Initialize(this._data);
        }
    }
}