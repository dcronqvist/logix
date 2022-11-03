using LogiX;
using LogiX.Architecture;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace content_1;

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

[ScriptType("CLOCK"), ComponentInfo("Clock", "Wiring")]
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
        this._data = data;
        this.RegisterIO("c", 1, LogiX.ComponentSide.RIGHT);
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

    public override void SubmitUISelected(int componentIndex)
    {
        // Nothing
    }
}