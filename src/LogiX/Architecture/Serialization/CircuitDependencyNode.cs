namespace LogiX.Architecture.Serialization;

public class CircuitDependencyNode
{
    public Circuit Circuit { get; }
    public List<CircuitDependencyNode> Dependencies { get; }

    public CircuitDependencyNode(Circuit circuit)
    {
        this.Circuit = circuit;
        this.Dependencies = new();
    }

    public void AddDependency(CircuitDependencyNode node)
    {
        this.Dependencies.Add(node);
    }

    public bool HasDependency(Circuit circuit)
    {
        if (this.Dependencies.Any(d => d.Circuit == circuit))
        {
            return true;
        }
        else
        {
            return this.Dependencies.Select(d => d.HasDependency(circuit)).Any();
        }
    }
}