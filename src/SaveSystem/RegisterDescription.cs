using LogiX.Components;

namespace LogiX.SaveSystem;

public class RegisterDescription : ComponentDescription
{
    public int DataBits { get; set; }
    public bool Multibit { get; set; }

    public RegisterDescription(Vector2 position, int databits, bool multibit, int rotation) : base(position, multibit ? Util.Listify(new IODescription(databits), new IODescription(1), new IODescription(1), new IODescription(1)) : Util.NValues(new IODescription(1), databits + 3), multibit ? Util.Listify(new IODescription(databits)) : Util.NValues(new IODescription(1), databits), rotation, ComponentType.Register)
    {
        this.DataBits = databits;
        this.Multibit = multibit;
    }

    public override Component ToComponent(bool preserveID)
    {
        RegisterComponent rc = new RegisterComponent(this.DataBits, this.Multibit, this.Position);
        rc.Rotation = this.Rotation;

        if (preserveID)
            rc.SetUniqueID(this.ID);

        return rc;
    }
}