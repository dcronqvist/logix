using LogiX.Components;

namespace LogiX.SaveSystem;

public class DescriptionSwitch : ComponentDescription
{
    public int Bits { get; set; }
    public ComponentSide Side { get; set; }
    public string Identifier { get; set; }

    public DescriptionSwitch(Vector2 position, int rotation, string uniqueID, int bits, string identifier, ComponentSide side) : base(position, rotation, uniqueID, ComponentType.SWITCH)
    {
        this.Bits = bits;
        this.Side = side;
        this.Identifier = identifier;
    }

    public override Component ToComponent()
    {
        Switch s = new Switch(this.Bits, this.Position, this.UniqueID);
        s.Bits = Bits;
        s.Side = this.Side;
        s.Identifier = this.Identifier;
        return s;
    }
}