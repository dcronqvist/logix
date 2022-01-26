using LogiX.Components;

namespace LogiX.SaveSystem;

public class DTBCDescription : ComponentDescription
{
    [JsonProperty(PropertyName = "decimals")]
    public int decimals;
    public bool multibit;

    public DTBCDescription(int decimals, bool multibit, Vector2 position) : base(position, multibit ? Util.Listify(new IODescription(decimals - 1)) : Util.NValues(new IODescription(1), decimals - 1), multibit ? Util.Listify(new IODescription((int)Math.Round(Math.Log2(decimals - 1)))) : Util.NValues(new IODescription(1), (int)Math.Round(Math.Log2(decimals - 1))), ComponentType.DTBC)
    {
        this.decimals = decimals;
        this.multibit = multibit;
    }

    public override Component ToComponent(bool preserveIDs)
    {
        DTBC c = new DTBC(this.decimals, this.multibit, this.Position);
        if (preserveIDs)
            c.SetUniqueID(this.ID);

        return c;
    }
}