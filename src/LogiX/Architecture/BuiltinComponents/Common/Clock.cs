using ImGuiNET;
using LogiX;
using LogiX.Architecture;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class ClockData : IComponentDescriptionData
{
    public int HighDuration { get; set; }
    public int LowDuration { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new ClockData
        {
            HighDuration = 1000,
            LowDuration = 1000
        };
    }
}

[ScriptType("CLOCK"), ComponentInfo("Clock", "Common", "core.markdown.clock")]
public class Clock : Component<ClockData>
{
    public override string Name => "CLK";
    public override bool DisplayIOGroupIdentifiers => false;
    public override bool ShowPropertyWindow => true;

    private ClockData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(ClockData data)
    {
        this.ClearIOs();

        this._data = data;
        this.RegisterIO("c", 1, ComponentSide.RIGHT);
    }

    private int _counter = 0;
    public override void PerformLogic()
    {
        var output = this.GetIOFromIdentifier("c");

        if (_counter < _data.HighDuration)
        {
            output.Push(LogicValue.HIGH);
            _counter++;
        }
        else if (_counter < _data.HighDuration + _data.LowDuration)
        {
            output.Push(LogicValue.LOW);
            _counter++;
        }
        else
        {
            _counter = 0;
            output.Push(LogicValue.LOW);
        }
    }

    public override void SubmitUISelected(Editor editor, int componentIndex)
    {
        var id = this.GetUniqueIdentifier();
        var highdur = this._data.HighDuration;
        if (ImGui.InputInt($"High Duration##{id}", ref highdur, 20, 100))
        {
            this._data.HighDuration = highdur;
            this.Initialize(this._data);
        }
        var lowdur = this._data.LowDuration;
        if (ImGui.InputInt($"Low Duration##{id}", ref lowdur, 20, 100))
        {
            this._data.LowDuration = lowdur;
            this.Initialize(this._data);
        }
    }
}