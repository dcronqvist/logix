namespace LogiX.SaveSystem;

public enum ComponentType
{
    LOGIC_GATE,
    SWITCH,
}

public abstract class ComponentDescription
{
    public ComponentType Type { get; set; }

    public ComponentDescription(ComponentType type)
    {
        this.Type = type;
    }
}