using LogiX.Components;

namespace LogiX.SaveSystem;

public class CustomDescription : ComponentDescription
{
    [JsonPropertyName("componentIdentifier")]
    public string ComponentIdentifier { get; private set; }

    [JsonPropertyName("componentName")]
    public string ComponentName { get; set; }

    [JsonPropertyName("plugin")]
    public string Plugin { get; set; }

    [JsonPropertyName("pluginVersion")]
    public string PluginVersion { get; set; }

    public JsonDocument Data { get; set; }

    public CustomDescription(string componentIdentifier, string componentName, JsonDocument data, Vector2 position, List<IODescription> inputs, List<IODescription> outputs) : base(position, inputs, outputs, ComponentType.Custom)
    {
        this.ComponentIdentifier = componentIdentifier;
        this.ComponentName = componentName;
        this.Data = data;
    }

    public override Component ToComponent(bool preserveID)
    {
        // Here we want to create a new instance of the given custom component
        // of type that corresponds to the identifier.
        // These components come from different plugins, so we need to find
        // the correct assembly and the correct type.
        // We can do this by using the ComponentIdentifier.

        return Util.CreateComponentWithPluginIdentifier(this.ComponentIdentifier, this.Position, this.Data);
    }
}