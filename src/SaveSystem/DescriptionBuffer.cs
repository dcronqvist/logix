using LogiX.Components;

namespace LogiX.SaveSystem;

public class DescriptionBuffer : ComponentDescription
{
    public int Bits { get; set; }
    public string Logic { get; set; }

    [JsonConstructor]
    public DescriptionBuffer(Vector2 position, int bits, string logic, int rotation, string uniqueID) : base(position, rotation, uniqueID, ComponentType.BUFFER)
    {
        this.Bits = bits;
        this.Logic = logic;
    }

    public DescriptionBuffer(Vector2 position, int bits, IBufferLogic logic, int rotation, string uniqueID) : base(position, rotation, uniqueID, ComponentType.BUFFER)
    {
        this.Bits = bits;
        this.Logic = logic.GetLogicText();
    }

    public override Component ToComponent()
    {
        return new BufferComponent(this.Position, this.Bits, Util.GetBufferLogicFromName(this.Logic), this.UniqueID);
    }
}