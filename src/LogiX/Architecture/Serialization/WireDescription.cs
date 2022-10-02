namespace LogiX.Architecture.Serialization;

public class WireDescription
{
    public Vector2i Position { get; set; }
    public List<WireDescription> Children { get; set; }

    public WireDescription(Vector2i position)
    {
        Position = position;
        Children = new List<WireDescription>();
    }

    public void AddChild(WireDescription child)
    {
        Children.Add(child);
    }

    public Wire CreateWire()
    {
        throw new NotImplementedException();

        // Wire wire = new();
        // wire.RootNode = this.CreateWireNode();
        // return wire;
    }
}