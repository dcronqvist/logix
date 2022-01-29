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

    [JsonPropertyName("componentData")]
    public CustomComponentData Data { get; set; }

    [JsonConstructor]
    public CustomDescription(string componentIdentifier, string componentName, Vector2 position, int rotation, List<IODescription> inputs, List<IODescription> outputs) : base(position, inputs, outputs, rotation, ComponentType.Custom)
    {
        this.ComponentIdentifier = componentIdentifier;
        this.ComponentName = componentName;
    }

    public CustomDescription(string componentIdentifier, string componentName, CustomComponentData componentData, Vector2 position, int rotation, List<IODescription> inputs, List<IODescription> outputs) : base(position, inputs, outputs, rotation, ComponentType.Custom)
    {
        this.ComponentIdentifier = componentIdentifier;
        this.ComponentName = componentName;
        this.Data = componentData;
    }

    public override Component ToComponent(bool preserveID)
    {
        // Here we want to create a new instance of the given custom component
        // of type that corresponds to the identifier.
        // These components come from different plugins, so we need to find
        // the correct assembly and the correct type.
        // We can do this by using the ComponentIdentifier.

        return Util.CreateComponentWithPluginIdentifier(this.ComponentIdentifier, this.Position, this.Rotation, this.Data);
    }
}