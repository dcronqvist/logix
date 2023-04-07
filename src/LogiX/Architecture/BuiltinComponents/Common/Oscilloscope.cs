using ImGuiNET;
using LogiX.Architecture.Commands;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

// A simple moving average of the time between rising edges from LogicValue.LOW to LogicValue.HIGH
class FrequencyCounter
{
    private readonly int _windowSize;
    private readonly Queue<TimeSpan> _window = new();
    private DateTime _lastTime = DateTime.MinValue;

    public FrequencyCounter(int windowSize)
    {
        this._windowSize = windowSize;
    }

    public void Add(DateTime time)
    {
        if (this._lastTime != DateTime.MinValue)
        {
            this._window.Enqueue(time - this._lastTime);
            if (this._window.Count > this._windowSize)
                this._window.Dequeue();
        }

        this._lastTime = time;
    }

    public double GetAverage()
    {
        if (this._window.Count == 0)
            return 0;

        return this._window.Average(t => t.TotalSeconds);
    }
}

public class OscilloscopeData : INodeDescriptionData
{
    public INodeDescriptionData GetDefault()
    {
        return new OscilloscopeData();
    }
}

[ScriptType("OSCILLOSCOPE"), NodeInfo("Oscilloscope", "Common", "logix_core:docs/components/template.md")]
public class Oscilloscope : BoxNode<OscilloscopeData>
{
    private OscilloscopeData _data;

    private FrequencyCounter _sma = new FrequencyCounter(50);
    public override string Text => (this._sma.GetAverage() > 0 ? (1d / this._sma.GetAverage()).GetAsHertzString() : "?");
    public override float TextScale => 1f;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var x = pins.Get("X").Read(1);

        if (x.First() == LogicValue.HIGH)
        {
            this._sma.Add(DateTime.Now);
        }

        yield break;
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        var size = this.GetSize();
        yield return new PinConfig("X", 1, true, new Vector2i(0, 1));
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(8, 2);
    }

    public override void Initialize(OscilloscopeData data)
    {
        this._data = data;
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        return false; // No interaction.
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        yield break;
    }

    public override void CompleteSubmitUISelected(Editor editor, int componentIndex)
    {
        // No UI for oscilloscope.
    }
}