using System.Diagnostics.CodeAnalysis;

namespace LogiX.Components;

public class ComponentInput : ComponentIO
{
    public Wire? Signal { get; private set; }
    public override Vector2 Position => this.OnComponent.GetInputLinePositions(this.OnComponentIndex).Item1;

    public ComponentInput(int bits, string identifier, Component component, int index) : base(bits, identifier, component, index) { }
    public ComponentInput(int bits, string identifier, Component component, int index, IEnumerable<LogicValue> values) : base(bits, identifier, component, index, values) { }

    public bool SetSignal(Wire signal)
    {
        if (this.Bits != signal.Bits)
        {
            return false;
        }
        Signal = signal;
        return true;
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

    [MemberNotNullWhen(true, "Signal")]
    public bool HasSignal()
    {
        return this.Signal != null;
    }
}