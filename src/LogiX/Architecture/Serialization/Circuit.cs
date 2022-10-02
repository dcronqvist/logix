using System.Text.Json;
using System.Text.Json.Serialization;

namespace LogiX.Architecture.Serialization;

public class Circuit
{
    public List<ComponentDescription> Components { get; set; }
    public List<WireDescription> Wires { get; set; }

    [JsonConstructor]
    public Circuit()
    {

    }

    public Circuit(List<Component> components, List<Wire> wires)
    {
        Components = this.GetComponentDescriptions(components);
        Wires = this.GetWireDescriptions(wires);
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
}