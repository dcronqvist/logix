using LogiX.Components;

namespace LogiX.SaveSystem;

public class DescriptionIntegrated : ComponentDescription
{
    public Circuit Circuit { get; set; }

    [JsonConstructor]
    public DescriptionIntegrated(Vector2 position, int rotation, string uniqueID, Circuit circuit) : base(position, rotation, uniqueID, ComponentType.INTEGRATED)
    {
        this.Circuit = circuit.Clone();
    }

    public override Component ToComponent()
    {
        return new IntegratedComponent(this.Position, this.Circuit, this.UniqueID);
    }
}