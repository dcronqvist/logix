using System.Text.Json.Serialization;

namespace LogiX.Architecture.Serialization;

public class WireDescription
{
    public List<Vector2i[]> Segments { get; set; }

    public WireDescription(List<(Vector2i, Vector2i)> segments)
    {
        this.Segments = segments.Select(s => new Vector2i[] { s.Item1, s.Item2 }).ToList();
    }

    [JsonConstructor]
    public WireDescription()
    {

    }

    public Wire CreateWire()
    {
        var wire = new Wire();
        wire.Segments = this.Segments.Select(s => (s[0], s[1])).ToList();
        return wire;
    }
}