using LogiX.Components;

namespace LogiX.SaveSystem;

public class Project
{
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }

    [JsonProperty(PropertyName = "currentWorkspace")]
    public CircuitDescription CurrentWorkspace { get; set; }

    public Project(string name)
    {
        this.Name = name;
    }

    public void SaveComponentsInWorkspace(List<Component> components)
    {
        this.CurrentWorkspace = new CircuitDescription(components);
    }

    public void SaveToFile(string directory)
    {
        string fileName = this.Name.ToLower().Replace(" ", "_");
        using (StreamWriter sw = new StreamWriter($"{directory}/{fileName}.lgxproj"))
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            sw.Write(json);
        }
    }
}