namespace LogiX.Components;

public class ComponentOutput : ComponentIO
{
    public List<Wire> Signals { get; private set; }

    public ComponentOutput(int bits, string identifier) : base(bits, identifier) { this.Signals = new List<Wire>(); }
    public ComponentOutput(int bits, string identifier, IEnumerable<LogicValue> values) : base(bits, identifier, values) { this.Signals = new List<Wire>(); }

    public void AddOutputWire(Wire wire)
    {
        this.Signals.Add(wire);
    }

    public void RemoveOutputSignal(int index)
    {
        this.Signals.RemoveAt(index);
    }

    public bool HasAnySignal()
    {
        return this.Signals.Count != 0;
    }

    public void SetSignals()
    {
        for (int i = 0; i < this.Signals.Count; i++)
        {
            // Get values from this output and send to all wires.
            this.Signals[i].SetValues(this.Values);
        }
    }
}