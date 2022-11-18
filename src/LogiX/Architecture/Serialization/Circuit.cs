using System.Text.Json;
using System.Text.Json.Serialization;
using LogiX.Architecture.BuiltinComponents;

namespace LogiX.Architecture.Serialization;

public class Circuit
{
    public Guid ID { get; set; }
    public Guid IterationID { get; set; }
    public string Name { get; set; }
    public List<ComponentDescription> Components { get; set; }
    public List<WireDescription> Wires { get; set; }

    [JsonConstructor]
    public Circuit()
    {

    }

    public Circuit(string name)
    {
        this.Name = name;
        this.Components = new List<ComponentDescription>();
        this.Wires = new List<WireDescription>();
        this.ID = Guid.NewGuid();
        this.IterationID = Guid.NewGuid();
    }

    public Circuit(string name, List<Component> components, List<Wire> wires)
    {
        this.Name = name;
        this.Components = this.GetComponentDescriptions(components);
        this.Wires = this.GetWireDescriptions(wires);
        this.ID = Guid.NewGuid();
        this.IterationID = Guid.NewGuid();
    }

    private List<ComponentDescription> GetComponentDescriptions(List<Component> comps)
    {
        List<ComponentDescription> descriptions = new List<ComponentDescription>();

        foreach (Component comp in comps)
        {
            descriptions.Add(comp.GetDescriptionOfInstance());
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
            Converters = { new IComponentDescriptionDataConverter() }
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
                Converters = { new IComponentDescriptionDataConverter() }
            };

            return JsonSerializer.Deserialize<Circuit>(file.ReadToEnd(), options);
        }
    }

    public ComponentDescription[] GetAllPins()
    {
        return this.Components.Where(c => c.ComponentTypeID == "logix_builtin.script_type.PIN" && (c.Data as PinData).IsExternal).ToArray();
    }

    public ComponentDescription[] GetAllComponentsOfType(string typeID)
    {
        return this.Components.Where(c => c.ComponentTypeID == typeID).ToArray();
    }

    // UNUSED AS OF NOW
    public CircuitDependencyNode GetDependencies()
    {
        var root = new CircuitDependencyNode(this);

        foreach (var desc in this.Components)
        {
            if (desc.ComponentTypeID == "logix_builtin.script_type.INTEGRATED")
            {
                var integratedData = desc.Data as IntegratedData;
                //root.AddDependency(integratedData.Circuit.GetDependencies());
            }
        }

        return root;
    }

    public void Update(Circuit other)
    {
        this.Components = other.Components;
        this.Wires = other.Wires;
    }

    public Vector2i GetMiddleOfCircuit()
    {
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        foreach (var desc in this.Components)
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
        this.Components.RemoveAll(c => c.ComponentTypeID == "logix_builtin.script_type.INTEGRATED" && (c.Data as IntegratedData).CircuitID == id);
    }
}