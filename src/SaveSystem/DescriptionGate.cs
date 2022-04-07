using LogiX.Components;

namespace LogiX.SaveSystem;

public class DescriptionGate : ComponentDescription
{
    public int Bits { get; set; }
    public string Logic { get; set; }

    public DescriptionGate(Vector2 position, int rotation, string uniqueID, int bits, string logic) : base(position, rotation, uniqueID, ComponentType.LOGIC_GATE)
    {
        this.Bits = bits;
        this.Logic = logic;
    }

    public override Component ToComponent()
    {
        LogicGate s = new LogicGate(this.Position, this.Bits, Util.GetGateLogicFromName(this.Logic), this.UniqueID);
        return s;
    }
}