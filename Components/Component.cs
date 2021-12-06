namespace LogiX.Components;

public abstract class Component
{
    public List<ComponentInput> Inputs { get; private set; }
    public List<ComponentOutput> Outputs { get; private set; }

    public Component(IEnumerable<int> bitsPerInput, IEnumerable<int> bitsPerOutput)
    {
        // Creating inputs
        this.Inputs = new List<ComponentInput>();
        for (int i = 0; i < bitsPerInput.Count(); i++)
        {
            ComponentInput ci = new ComponentInput(bitsPerInput.ElementAt(i), $"{i}");
            this.Inputs.Add(ci);
        }

        // Creating outputs
        this.Outputs = new List<ComponentOutput>();
        for (int i = 0; i < bitsPerOutput.Count(); i++)
        {
            ComponentOutput ci = new ComponentOutput(bitsPerOutput.ElementAt(i), $"{i}");
            this.Outputs.Add(ci);
        }
    }

    public void SetInputWire(int index, Wire wire)
    {
        this.Inputs[index].SetSignal(wire);
    }

    public void RemoveInputWire(int index)
    {
        this.Inputs[index].RemoveSignal();
    }

    public void AddOutputWire(int index, Wire wire)
    {
        this.Outputs[index].AddOutputWire(wire);
    }

    public void RemoveOutputWire(int index, Wire wire)
    {
        ComponentOutput co = this.Outputs[index];
        for (int i = 0; i < co.Signals.Count; i++)
        {
            if (wire == co.Signals[i])
            {
                co.RemoveOutputSignal(i);
                return;
            }
        }
    }

    public ComponentInput InputAt(int index)
    {
        return this.Inputs[index];
    }

    public ComponentOutput OutputAt(int index)
    {
        return this.Outputs[index];
    }

    public void UpdateInputs()
    {
        for (int i = 0; i < this.Inputs.Count; i++)
        {
            this.Inputs[i].GetSignalValue();
        }
    }

    public void UpdateOutputs()
    {
        for (int i = 0; i < this.Outputs.Count; i++)
        {
            this.Outputs[i].SetSignals();
        }
    }

    public void Update()
    {
        this.UpdateInputs();
        this.PerformLogic();
        this.UpdateOutputs();
    }

    public abstract void PerformLogic();
}