using LogiX.Components;

namespace LogiX.SaveSystem;

public enum ComponentType
{
    Gate,
    Integrated,
    Switch,
    Lamp
}

public class IODescription
{
    [JsonProperty(PropertyName = "bits")]
    public int Bits { get; set; }

    public IODescription(int bits)
    {
        this.Bits = bits;
    }
}

public abstract class ComponentDescription
{
    [JsonProperty(PropertyName = "position")]
    public Vector2 Position { get; set; }
    [JsonProperty(PropertyName = "inputs")]
    public List<IODescription> Inputs { get; set; }
    [JsonProperty(PropertyName = "outputs")]
    public List<IODescription> Outputs { get; set; }
    [JsonProperty(PropertyName = "type")]
    public ComponentType Type { get; set; }
    [JsonProperty(PropertyName = "id")]
    public string ID { get; set; }

    public ComponentDescription(Vector2 position, List<IODescription> inputs, List<IODescription> outputs, ComponentType ct)
    {
        this.Inputs = inputs;
        this.Outputs = outputs;
        this.Type = ct;
        this.ID = Guid.NewGuid().ToString();
        this.Position = position;
    }

    public abstract Component ToComponent();
}