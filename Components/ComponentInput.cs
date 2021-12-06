namespace LogiX.Components;

public class ComponentInput : ComponentIO
{
    public Wire? Signal { get; private set; }

    public ComponentInput(int bits, string identifier) : base(bits, identifier) { }
    public ComponentInput(int bits, string identifier, IEnumerable<LogicValue> values) : base(bits, identifier, values) { }

    public void SetSignal(Wire signal)
    {
        Signal = signal;
    }

    public void GetSignalValue()
    {
        if (this.Signal != null)
        {
            this.SetValues(this.Signal.Values);
        }
        else
        {
            this.SetAllValues(LogicValue.LOW);
        }
    }

    public void RemoveSignal()
    {
        this.Signal = null;
    }

    public bool HasSignal()
    {
        return this.Signal != null;
    }
}