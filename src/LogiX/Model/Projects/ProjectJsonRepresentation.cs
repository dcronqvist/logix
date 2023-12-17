using LogiX.Model.Circuits;

namespace LogiX.Model.Projects;

public record ProjectJsonRepresentation
{
    public ProjectMetadata Metadata { get; init; }
    public IVirtualFileTree<string, ICircuitDefinition> CircuitDefinitions { get; init; }
}
