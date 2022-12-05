using System.Text.Json;
using System.Text.Json.Serialization;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Graphics;

namespace LogiX.Architecture.Serialization;


public class Circuit
{
    public Guid ID { get; set; }
    public Guid IterationID { get; set; }
    public string Name { get; set; }
    public List<NodeDescription> Nodes { get; set; }
    public List<WireDescription> Wires { get; set; }

    [JsonConstructor]
    public Circuit()
    {

    }

    public Circuit(string name)
    {
        this.Name = name;
        this.Nodes = new List<NodeDescription>();
        this.Wires = new List<WireDescription>();
        this.ID = Guid.NewGuid();
        this.IterationID = Guid.NewGuid();
    }

    public Circuit(string name, List<Node> nodes, List<Wire> wires)
    {
        this.Name = name;
        this.Nodes = this.GetNodeDescriptions(nodes);
        this.Wires = this.GetWireDescriptions(wires);
        this.ID = Guid.NewGuid();
        this.IterationID = Guid.NewGuid();
    }

    private List<NodeDescription> GetNodeDescriptions(List<Node> nodes)
    {
        List<NodeDescription> descriptions = new List<NodeDescription>();

        foreach (var node in nodes)
        {
            descriptions.Add(node.GetDescriptionOfInstance());
        }

        return descriptions;
    }

    private List<WireDescription> GetWireDescriptions(List<Wire> wires)
    {
        List<WireDescription> descriptions = new List<WireDescription>();

        foreach (Wire wire in wires)
        {
            descriptions.Add(wire.GetDescriptionOfInstance());
        }

        return descriptions;
    }

    public void SaveToFile(string path)
    {
        var options = new JsonSerializerOptions()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new INodeDescriptionDataConverter() }
        };

        string json = JsonSerializer.Serialize(this, options);

        using (var file = new StreamWriter(path))
        {
            file.Write(json);
        }
    }

    public static Circuit LoadFromFile(string path)
    {
        using (var file = new StreamReader(path))
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new INodeDescriptionDataConverter() }
            };

            return JsonSerializer.Deserialize<Circuit>(file.ReadToEnd(), options);
        }
    }

    public NodeDescription[] GetAllPins()
    {
        return this.Nodes.Where(c => c.NodeTypeID == "logix_builtin.script_type.PIN" && (c.Data as PinData).IsExternal).ToArray();
    }

    public NodeDescription[] GetAllComponentsOfType(string typeID)
    {
        return this.Nodes.Where(c => c.NodeTypeID == typeID).ToArray();
    }

    public void Update(Circuit other)
    {
        this.Nodes = other.Nodes;
        this.Wires = other.Wires;
    }

    public Vector2i GetMiddleOfCircuit()
    {
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        foreach (var desc in this.Nodes)
        {
            if (desc.Position.X < minX)
            {
                minX = desc.Position.X;
            }

            if (desc.Position.Y < minY)
            {
                minY = desc.Position.Y;
            }

            if (desc.Position.X > maxX)
            {
                maxX = desc.Position.X;
            }

            if (desc.Position.Y > maxY)
            {
                maxY = desc.Position.Y;
            }
        }

        return new Vector2i((minX + maxX) / 2, (minY + maxY) / 2);
    }

    public void RemoveOccurencesOf(Guid id)
    {
        //this.Nodes.RemoveAll(c => c.NodeTypeID == "logix_builtin.script_type.INTEGRATED" && (c.Data as IntegratedData).CircuitID == id);
    }
}