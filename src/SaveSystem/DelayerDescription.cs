using LogiX.Components;

namespace LogiX.SaveSystem;

public class DelayerDescription : ComponentDescription
{
    [JsonPropertyName("ticks")]
    public int Ticks { get; set; }

    public DelayerDescription(Vector2 position, int ticks, int bits, bool multibit) : base(position, multibit ? Util.Listify(new IODescription(bits)) : Util.NValues(new IODescription(1), bits), multibit ? Util.Listify(new IODescription(bits)) : Util.NValues(new IODescription(1), bits), ComponentType.Delayer)
    {
        this.Ticks = ticks;
    }

    public override Component ToComponent(bool preserveID)
    {
        Delayer d;
        if (this.Inputs.Count == 1)
        {
            // Multibit
            d = new Delayer(this.Ticks, this.Inputs[0].Bits, true, this.Position);
        }
        else
        {
            // Single bit
            d = new Delayer(this.Ticks, this.Inputs.Count, false, this.Position);
        }
        if (preserveID)
        {
            d.SetUniqueID(this.ID);
        }
        return d;
    }
}