using LogiX.Components;
using LogiX.Editor;
using System.Text.Json;

namespace LogiX.SaveSystem;

public class Project
{
    public List<Circuit> Circuits { get; set; }

    static JsonSerializerOptions _options = new JsonSerializerOptions
    {
        WriteIndented = true,
        IncludeFields = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new DescriptionConverter() }
    };

    public Project()
    {
        this.Circuits = new List<Circuit>();
    }

    public Circuit NewCircuit(string name)
    {
        Circuit circuit = new Circuit(name);
        this.Circuits.Add(circuit);
        return circuit;
    }

    public void SaveToFile(string file)
    {
        string json = JsonSerializer.Serialize(this, _options);

        using (StreamWriter sw = new StreamWriter(file))
        {
            sw.Write(json);
        }
    }

    public static Project LoadFromFile(string file)
    {
        using (StreamReader sr = new StreamReader(file))
        {
            string json = sr.ReadToEnd();
            return JsonSerializer.Deserialize<Project>(json, _options);
        }
    }
}