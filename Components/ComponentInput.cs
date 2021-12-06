namespace LogiX.Components;

public class ComponentInput : ComponentIO
{
    public Wire? Signal { get; private set; }

    public ComponentInput(int bits, string identifier, Component component, int index) : base(bits, identifier, component, index) { }
    public ComponentInput(int bits, string identifier, Component component, int index, IEnumerable<LogicValue> values) : base(bits, identifier, component, index, values) { }

    public void SetSignal(Wire signal)
    {
        if (this.Bits != signal.Bits)
        {
            throw new ArgumentException($"Cannot connect a {signal.Bits} bit wire to a {this.Bits} bit IO.");
        }
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