using LogiX.SaveSystem;
using Newtonsoft.Json.Linq;

namespace LogiX.Components;

public abstract class CustomComponent : Component
{
    public string ComponentIdentifier { get; private set; }
    public string Plugin { get; set; }
    public string PluginVersion { get; set; }

    // Custom components must ONLY take in position and data
    public CustomComponent(string identifier, IEnumerable<int> bitsPerInput, IEnumerable<int> bitsPerOutput, Vector2 position) : base(bitsPerInput, bitsPerOutput, position)
    {
        this.ComponentIdentifier = identifier;
    }

    public abstract override CustomDescription ToDescription();
}