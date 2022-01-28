namespace LogiX.SaveSystem;

public class ICCollection
{
    [JsonPropertyName("ics")]
    public List<ICDescription> ICs { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonIgnore]
    public const string EXTENSION = ".lgxcoll";

    [JsonConstructor]
    public ICCollection(string name)
    {
        this.Name = name;
        this.ICs = new List<ICDescription>();
    }

    public ICCollection(string name, List<ICDescription> ics)
    {
        this.Name = name;
        this.ICs = ics;
    }

    public void AddIC(ICDescription icd)
    {
        this.ICs.Add(icd);
    }

    public void RemoveIC(ICDescription icd)
    {
        this.ICs.Remove(icd);
    }

    public void SaveToFile(string directory)
    {
        using (StreamWriter sw = new StreamWriter($"{directory}/{this.Name.ToSuitableFileName()}{ICCollection.EXTENSION}"))
        {
            sw.Write(JsonSerializer.Serialize(this));
        }
    }
}