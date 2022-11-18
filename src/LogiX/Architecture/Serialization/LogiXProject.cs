using System.Text.Json;
using System.Text.Json.Serialization;
using LogiX.Architecture.Serialization;

namespace LogiX.Architecture.Serialization;

public class LogiXProject
{
    [JsonIgnore]
    public string LoadedFromPath { get; set; }
    public string Name { get; set; }
    public List<Circuit> Circuits { get; set; }
    public Guid LastOpenedCircuit { get; set; }

    public LogiXProject()
    {
        this.Circuits = new List<Circuit>();
    }

    public static LogiXProject New(string name)
    {
        var proj = new LogiXProject()
        {
            Name = name,
            LoadedFromPath = "",
        };

        proj.AddCircuit(new Circuit("main"));
        return proj;
    }

    public void AddCircuit(Circuit circuit)
    {
        if (this.Circuits.Any(c => c.Name == circuit.Name))
        {
            throw new Exception($"A circuit with the name {circuit.Name} already exists in this project.");
        }

        this.Circuits.Add(circuit);
    }

    public void RemoveCircuit(Guid id)
    {
        var circuit = this.Circuits.FirstOrDefault(c => c.ID == id);
        if (circuit == null)
        {
            throw new Exception($"A circuit with the ID {id} does not exist in this project.");
        }

        this.Circuits.Remove(circuit);

        foreach (var c in this.Circuits)
        {
            c.RemoveOccurencesOf(id);
        }
    }

    public void SetLastOpenedCircuit(Guid circuitID)
    {
        this.LastOpenedCircuit = circuitID;
    }

    public Circuit GetCircuit(Guid id)
    {
        return this.Circuits.Find(c => c.ID == id);
    }

    public void UpdateCircuit(Circuit circuit)
    {
        var index = this.Circuits.FindIndex(c => c.ID == circuit.ID);
        var oldCircuit = this.Circuits[index];
        this.Circuits[index].Update(circuit);
        circuit.IterationID = Guid.NewGuid();
    }

    public void SaveProjectToFile(string path)
    {
        var options = new JsonSerializerOptions()
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new IComponentDescriptionDataConverter() }
        };

        string json = JsonSerializer.Serialize(this, options);

        using (var file = new StreamWriter(path))
        {
            file.Write(json);
        }

        this.LoadedFromPath = Path.GetFullPath(path);
    }

    public void Quicksave()
    {
        this.SaveProjectToFile(this.LoadedFromPath);
    }

    public bool HasFileToSaveTo()
    {
        return this.LoadedFromPath != "" && File.Exists(this.LoadedFromPath);
    }

    public static LogiXProject FromFile(string path)
    {
        using (var file = new StreamReader(path))
        {
            var options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new IComponentDescriptionDataConverter() }
            };

            var proj = JsonSerializer.Deserialize<LogiXProject>(file.ReadToEnd(), options);
            proj.LoadedFromPath = Path.GetFullPath(path);
            return proj;
        }
    }

    public bool HasCircuitWithName(string name)
    {
        return this.Circuits.Any(c => c.Name == name);
    }

    public Circuit GetCircuitWithName(string name)
    {
        return this.Circuits.FirstOrDefault(c => c.Name == name);
    }
}