using LogiX.Components;

namespace LogiX.SaveSystem;

public class Circuit
{
    public List<ComponentDescription> Components { get; set; }
    public List<WireDescription> Wires { get; set; }
    public string UniqueID { get; set; }
    public string UpdateID { get; set; }
    public string Name { get; set; }

    [JsonConstructor]
    public Circuit(string name, string uniqueID, string updateID)
    {
        this.Name = name;
        this.UniqueID = uniqueID;
        this.UpdateID = updateID;
    }

    public Circuit(string name)
    {
        this.Name = name;
        this.UniqueID = Guid.NewGuid().ToString();
        this.UpdateID = Guid.NewGuid().ToString();
    }

    public Circuit(string name, List<Component> comps, List<Wire> wires)
    {
        this.Name = name;
        this.Components = this.GatherComponents(comps);
        this.Wires = this.GatherWires(wires);
        this.UniqueID = Guid.NewGuid().ToString();
        this.UpdateID = Guid.NewGuid().ToString();
    }

    public void Update(List<Component> comps, List<Wire> wires)
    {
        List<(int, IOConfig, string)> oldIOConfigs = this.GetIOConfigs();

        this.Components = this.GatherComponents(comps);
        this.Wires = this.GatherWires(wires);
        this.UpdateID = Guid.NewGuid().ToString();

        List<(int, IOConfig, string)> newIOConfigs = this.GetIOConfigs();
    }

    public List<ComponentDescription> GatherComponents(List<Component> comps)
    {
        return comps.Select(c => c.ToDescription()).ToList();
    }

    public List<WireDescription> GatherWires(List<Wire> wires)
    {
        return wires.Select(w => w.ToDescription()).ToList();
    }

    public (List<Component>, List<Wire>) GetComponentsAndWires()
    {
        return this.GetComponentsAndWires(Vector2.Zero);
    }

    public (List<Component>, List<Wire>) GetComponentsAndWires(Vector2 newMiddlePos)
    {
        List<Component> comps = new List<Component>();
        List<Wire> wires = new List<Wire>();

        if (this.Components is null)
        {
            return (comps, wires);
        }

        foreach (ComponentDescription desc in this.Components)
        {
            comps.Add(desc.ToComponent());
        }

        foreach (WireDescription desc in this.Wires)
        {
            wires.Add(desc.ToWire(comps));
        }

        return (comps, wires);
    }

    public Simulator GetSimulatorForCircuit()
    {
        Simulator simulator = new Simulator();

        (List<Component> comps, List<Wire> wires) = this.GetComponentsAndWires();
        simulator.AllComponents = comps;
        simulator.AllWires = wires;

        return simulator;
    }

    public List<(int, IOConfig, string)> GetIOConfigs()
    {
        Simulator sim = this.GetSimulatorForCircuit();
        return sim.GetIOConfigs();
    }

    public List<CircuitDependency> GetDependencyCircuits()
    {
        if (this.Components is null)
        {
            return Util.EmptyList<CircuitDependency>();
        }

        List<CircuitDependency> circuits = new List<CircuitDependency>();

        foreach (ComponentDescription compDesc in this.Components)
        {
            if (compDesc is DescriptionIntegrated descIntegrated)
            {
                circuits.Add(descIntegrated.Circuit.GetAsDependency());
            }
        }

        return circuits.RemoveDuplicates();
    }

    public bool MatchesDependency(CircuitDependency dependency)
    {
        return this.UniqueID == dependency.CircuitID && this.UpdateID == dependency.CircuitUpdateID;
    }

    public CircuitDependency GetAsDependency()
    {
        return new CircuitDependency()
        {
            CircuitID = this.UniqueID,
            CircuitUpdateID = this.UpdateID
        };
    }

    public void UpdateDependency(CircuitDependency dependency, Circuit circuit)
    {
        List<DescriptionIntegrated> descInt = this.Components.Where(x => x is DescriptionIntegrated di && di.Circuit.UniqueID == dependency.CircuitID).Cast<DescriptionIntegrated>().ToList();
        descInt.ForEach(di => di.Circuit = circuit.Clone());
    }

    public Circuit Clone()
    {
        (List<Component> comps, List<Wire> wires) = this.GetComponentsAndWires();
        Circuit newCirc = new Circuit(this.Name, comps, wires);
        newCirc.UniqueID = this.UniqueID;
        newCirc.UpdateID = this.UpdateID;
        return newCirc;
    }
}