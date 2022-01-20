using LogiX.SaveSystem;
using Newtonsoft.Json.Linq;

namespace LogiX.Components;

public abstract class CustomComponent : Component
{
    public string ComponentIdentifier { get; private set; }
    public string ComponentName { get; private set; }
    public string Plugin { get; set; }
    public string PluginVersion { get; set; }
    public override string Text => ComponentName;

    // Custom components must ONLY take in position and data
    public CustomComponent(string identifier, string name, IEnumerable<int> bitsPerInput, IEnumerable<int> bitsPerOutput, Vector2 position) : base(bitsPerInput, bitsPerOutput, position)
    {
        this.ComponentIdentifier = identifier;
        this.ComponentName = name;
    }

    public abstract override CustomDescription ToDescription();
}