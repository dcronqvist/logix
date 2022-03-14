using LogiX.Components;

namespace LogiX.SaveSystem;

public class DescriptionIntegrated : ComponentDescription
{
    public Circuit Circuit { get; set; }

    public DescriptionIntegrated(Vector2 position, int rotation, string uniqueID, Circuit circuit) : base(position, rotation, uniqueID, ComponentType.INTEGRATED)
    {
        this.Circuit = circuit;
    }

    public override Component ToComponent()
    {
        return new IntegratedComponent(this.Position, this.Circuit, this.UniqueID);
    }
}