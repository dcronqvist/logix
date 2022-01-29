using LogiX.Components;

namespace LogiX.SaveSystem;

public class ConstantDescription : ComponentDescription
{
    [JsonPropertyName("value")]
    public LogicValue Value { get; set; }

    public ConstantDescription(Vector2 position, int rotation, LogicValue value) : base(position, Util.EmptyList<IODescription>(), Util.Listify(new IODescription(1)), rotation, ComponentType.Constant)
    {
        this.Value = value;
    }

    public override Component ToComponent(bool preserveIDs)
    {
        ConstantComponent c = new ConstantComponent(this.Value, this.Position);
        c.Rotation = Rotation;
        if (preserveIDs)
            c.SetUniqueID(this.ID);
        return c;
    }
}