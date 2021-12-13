namespace LogiX.SaveSystem;

public class ICDescription
{
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }
    [JsonProperty(PropertyName = "circuit")]
    public CircuitDescription Circuit { get; set; }
    [JsonProperty(PropertyName = "inputOrder")]
    public List<List<string>> InputOrder { get; set; }
    [JsonProperty(PropertyName = "outputOrder")]
    public List<List<string>> OutputOrder { get; set; }

    public ICDescription(string name, CircuitDescription cd, List<List<string>> inputOrder, List<List<string>> outputOrder)
    {
        this.Name = name;
        this.Circuit = cd;
        this.InputOrder = inputOrder;
        this.OutputOrder = outputOrder;
    }
}