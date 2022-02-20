using LogiX.Components;

namespace LogiX.SaveSystem;

public class HexViewerDescription : ComponentDescription
{
    [JsonPropertyName("bits")]
    public int Bits { get; set; }

    [JsonPropertyName("multibit")]
    public bool Multibit { get; set; }

    [JsonPropertyName("includeRepBits")]
    public bool IncludeRepBits { get; set; }

    public HexViewerDescription(Vector2 position, int rotation, int bits, bool multibit, bool includeRepBits, List<IODescription> inputs, List<IODescription> outputs) : base(position, inputs, outputs, rotation, ComponentType.HexViewer)
    {
        this.Bits = bits;
        this.Multibit = multibit;
        this.IncludeRepBits = includeRepBits;
    }

    public override Component ToComponent(bool preserveIDs)
    {
        Component c = new HexViewer(this.Bits, this.Multibit, this.IncludeRepBits, this.Position);
        if (preserveIDs)
            c.SetUniqueID(this.ID);
        c.Rotation = Rotation;
        return c;
    }
}