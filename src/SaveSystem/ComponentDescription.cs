using LogiX.Components;

namespace LogiX.SaveSystem;

public enum ComponentType
{
    Gate,
    Integrated,
    Switch,
    Lamp,
    Button,
    HexViewer,
    ROM,
    TextLabel,
    Memory,
    Constant,
    Splitter,
    Clock,
    Delayer,
    Mux,
    Demux,
    DTBC,
    Custom
}

public class IODescription
{
    [JsonPropertyName("bits")]
    public int Bits { get; set; }

    public IODescription(int bits)
    {
        this.Bits = bits;
    }
}

public class ComponentDescription
{
    [JsonPropertyName("position")]
    public Vector2 Position { get; set; }
    [JsonPropertyName("inputs")]
    public List<IODescription> Inputs { get; set; }
    [JsonPropertyName("outputs")]
    public List<IODescription> Outputs { get; set; }
    [JsonPropertyName("type")]
    public ComponentType Type { get; set; }
    [JsonPropertyName("id")]
    public string ID { get; set; }

    public ComponentDescription(Vector2 position, List<IODescription> inputs, List<IODescription> outputs, ComponentType ct)
    {
        this.Inputs = inputs;
        this.Outputs = outputs;
        this.Type = ct;
        this.ID = Guid.NewGuid().ToString();
        this.Position = position;
    }

    public virtual Component ToComponent(bool preserveID)
    {
        throw new Exception($"Component of type {this.Type} has not yet been implemented to be deserialized.");
    }
}