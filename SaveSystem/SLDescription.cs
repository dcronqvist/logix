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

    public override Component ToComponent()
    {
        switch (this.Type)
        {
            case ComponentType.Switch:
                return new Switch(this.Outputs[0].Bits, this.Position, this.Name);

            case ComponentType.Lamp:
                return new Lamp(this.Inputs[0].Bits, this.Position, this.Name);
        }

        return null;
    }
}