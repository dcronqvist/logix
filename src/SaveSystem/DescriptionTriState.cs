using LogiX.Components;

namespace LogiX.SaveSystem;

public class DescriptionTriState : ComponentDescription
{
    public int Bits { get; set; }

    public DescriptionTriState(Vector2 position, int bits, int rotation, string uniqueID) : base(position, rotation, uniqueID, ComponentType.TRI_STATE)
    {
        this.Bits = bits;
    }

    public override Component ToComponent()
    {
        return new TriStateComponent(this.Position, this.Bits, this.UniqueID);
    }
}