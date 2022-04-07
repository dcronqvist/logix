using LogiX.Components;

namespace LogiX.SaveSystem;

public enum ComponentType
{
    LOGIC_GATE,
    SWITCH,
    LAMP,
    INTEGRATED,
    BUFFER,
    TRI_STATE,
}

public abstract class ComponentDescription
{
    public Vector2 Position { get; set; }
    public int Rotation { get; set; }
    public string UniqueID { get; set; }
    public ComponentType Type { get; set; }

    [JsonConstructor]
    public ComponentDescription(Vector2 position, int rotation, string uniqueID, ComponentType type)
    {
        this.Position = position;
        this.Rotation = rotation;
        this.UniqueID = uniqueID;
        this.Type = type;
    }

    public abstract Component ToComponent();
}