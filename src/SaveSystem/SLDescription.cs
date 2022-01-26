using LogiX.Components;

namespace LogiX.SaveSystem;

public class SLDescription : ComponentDescription
{
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }

    public SLDescription(Vector2 position, List<IODescription> inputs, List<IODescription> outputs, ComponentType ct, string name) : base(position, inputs, outputs, ct)
    {
        this.Name = name;
    }

    public override Component ToComponent(bool preserveIDs)
    {
        Component c = null;
        switch (this.Type)
        {
            case ComponentType.Switch:
                c = new Switch(this.Outputs[0].Bits, this.Position, this.Name);
                break;

            case ComponentType.Lamp:
                c = new Lamp(this.Inputs[0].Bits, this.Position, this.Name);
                break;
        }
        if (preserveIDs)
            c.SetUniqueID(this.ID);
        return c;
    }
}