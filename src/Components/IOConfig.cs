namespace LogiX.Components;

public enum ComponentSide
{
    LEFT,
    RIGHT,
    TOP,
    BOTTOM,
}

public class IOConfig
{
    public ComponentSide Side { get; set; }
    public string? Identifier { get; set; }

    public IOConfig(ComponentSide side, string? identifier = null)
    {
        this.Side = side;
        this.Identifier = identifier;
    }

    public override string ToString()
    {
        return $"{this.Side} {this.Identifier}";
    }
}