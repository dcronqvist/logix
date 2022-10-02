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

    private WireNode CreateWireNode()
    {
        WireNode node = new WireNode(this.Position);

        foreach (var child in this.Children)
        {
            node.AddChild(child.CreateWireNode());
        }

        return node;
    }

    public Wire CreateWire()
    {
        Wire wire = new();
        wire.RootNode = this.CreateWireNode();
        return wire;
    }
}