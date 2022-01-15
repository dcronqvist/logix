namespace LogiX.Components;

public class ComponentOutput : ComponentIO
{
    public List<Wire> Signals { get; private set; }
    public override Vector2 Position => this.OnComponent.GetOutputLinePositions(this.OnComponentIndex).Item1;

    public ComponentOutput(int bits, string identifier, Component component, int index) : base(bits, identifier, component, index) { this.Signals = new List<Wire>(); }
    public ComponentOutput(int bits, string identifier, Component component, int index, IEnumerable<LogicValue> values) : base(bits, identifier, component, index, values) { this.Signals = new List<Wire>(); }

    public bool AddOutputWire(Wire wire)
    {
        if (this.Bits != wire.Bits)
        {
            return false;
        }
        this.Signals.Add(wire);
        return true;
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