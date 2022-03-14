using LogiX.Components;

namespace LogiX.SaveSystem;

public class Circuit
{
    public List<ComponentDescription> Components { get; set; }
    public List<WireDescription> Wires { get; set; }
    public string UniqueID { get; set; }
    public string UpdateID { get; set; }
    public string Name { get; set; }

    public Circuit(string name)
    {
        this.Name = name;
        this.UniqueID = Guid.NewGuid().ToString();
        this.UpdateID = Guid.NewGuid().ToString();
    }

    public Circuit(string name, Simulator simulator)
    {
        this.Name = name;
        this.Components = this.GatherComponents(simulator);
        this.Wires = this.GatherWires(simulator);
        this.UniqueID = Guid.NewGuid().ToString();
        this.UpdateID = Guid.NewGuid().ToString();
    }

    public void Update(Simulator simulator)
    {
        this.Components = this.GatherComponents(simulator);
        this.Wires = this.GatherWires(simulator);
        this.UpdateID = Guid.NewGuid().ToString();
    }

    public List<ComponentDescription> GatherComponents(Simulator simulator)
    {
        return simulator.AllComponents.Select(c => c.ToDescription()).ToList();
    }

    public List<WireDescription> GatherWires(Simulator simulator)
    {
        return simulator.AllWires.Select(w => w.ToDescription()).ToList();
    }

    public Simulator GetSimulatorForCircuit()
    {
        Simulator simulator = new Simulator();

        foreach (ComponentDescription compDesc in this.Components)
        {
            simulator.AddComponent(compDesc.ToComponent());
        }

        foreach (WireDescription wireDesc in this.Wires)
        {
            simulator.AddWire(wireDesc.ToWire(simulator.AllComponents));
        }

        return simulator;
    }

    public List<(int, IOConfig, string)> GetIOConfigs()
    {
        Simulator sim = this.GetSimulatorForCircuit();
        return sim.GetIOConfigs();
    }

    public List<string> GetDependencyCircuits()
    {
        if (this.Components is null)
        {
            return new List<string>();
        }

        List<string> circuits = new List<string>();

        foreach (ComponentDescription compDesc in this.Components)
        {
            if (compDesc is DescriptionIntegrated descIntegrated)
            {
                circuits.Add(descIntegrated.Circuit.UniqueID);
            }
        }

        return circuits.RemoveDuplicates();
    }
}