using LogiX.Components;

namespace LogiX.SaveSystem;

public class BitwiseDescription : ComponentDescription
{
    [JsonPropertyName("gateType")]
    public string GateType { get; set; }

    [JsonPropertyName("bits")]
    public int Bits { get; set; }

    [JsonPropertyName("multibit")]
    public bool Multibit { get; set; }

    public BitwiseDescription(Vector2 position, int rotation, List<IODescription> inputs, List<IODescription> outputs, string gateType, int bits, bool multibit) : base(position, inputs, outputs, rotation, ComponentType.Bitwise)
    {
        this.GateType = gateType;
        this.Bits = bits;
        this.Multibit = multibit;
    }

    public override Component ToComponent(bool preserveIDs)
    {
        Component c = new BitwiseComponent(Util.GetGateLogicFromName(this.GateType), this.Bits, this.Multibit, this.Position);
        if (preserveIDs)
            c.SetUniqueID(this.ID);
        c.Rotation = Rotation;
        return c;
    }
}