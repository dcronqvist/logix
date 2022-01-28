using LogiX.Components;

namespace LogiX.SaveSystem;

public class ICDescription : ComponentDescription
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("circuit")]
    public CircuitDescription Circuit { get; set; }
    [JsonPropertyName("inputOrder")]
    public List<List<string>> InputOrder { get; set; }
    [JsonPropertyName("outputOrder")]
    public List<List<string>> OutputOrder { get; set; }

    [JsonIgnore]
    public const string EXTENSION = ".lgxic";

    public ICDescription(string name, Vector2 position, CircuitDescription circuit, List<List<string>> inputOrder, List<List<string>> outputOrder) : base(position, inputOrder.Select(x => new IODescription(x.Count)).ToList(), outputOrder.Select(x => new IODescription(x.Count)).ToList(), ComponentType.Integrated)
    {
        this.Name = name;
        this.Circuit = circuit;
        this.InputOrder = inputOrder;
        this.OutputOrder = outputOrder;
    }

    public List<int> GetBitsPerInput()
    {
        List<int> inputBits = new List<int>();

        for (int i = 0; i < this.InputOrder.Count; i++)
        {
            List<string> inputs = this.InputOrder[i];
            int bitSum = 0;
            foreach (string input in inputs)
            {
                bitSum += this.Circuit.GetSwitchWithID(input).Outputs[0].Bits;
            }

            inputBits.Add(bitSum);
        }

        return inputBits;
    }

    public List<int> GetBitsPerOutput()
    {
        List<int> outputBits = new List<int>();

        for (int i = 0; i < this.OutputOrder.Count; i++)
        {
            List<string> outputs = this.OutputOrder[i];
            int bitSum = 0;
            foreach (string output in outputs)
            {
                bitSum += this.Circuit.GetLampWithID(output).Inputs[0].Bits;
            }

            outputBits.Add(bitSum);
        }

        return outputBits;
    }

    public string GetInputIdentifier(int index)
    {
        List<string> inputs = this.InputOrder[index];

        if (inputs.Count == 1)
        {
            SLDescription sl = this.Circuit.GetSwitchWithID(inputs.First());
            return sl.Name;
        }
        else
        {
            SLDescription first = this.Circuit.GetSwitchWithID(inputs.First());
            SLDescription last = this.Circuit.GetSwitchWithID(inputs.Last());
            return last.Name + "-" + first.Name;
        }
    }

    public string GetOutputIdentifier(int index)
    {
        List<string> outputs = this.OutputOrder[index];

        if (outputs.Count == 1)
        {
            SLDescription sl = this.Circuit.GetLampWithID(outputs.First());
            return sl.Name;
        }
        else
        {
            SLDescription first = this.Circuit.GetLampWithID(outputs.First());
            SLDescription last = this.Circuit.GetLampWithID(outputs.Last());
            return last.Name + "-" + first.Name;
        }
    }

    public override Component ToComponent(bool preserveIDs)
    {
        Component c = new ICComponent(this, this.Position);
        if (preserveIDs)
            c.SetUniqueID(this.ID);
        return c;
    }

    public ICDescription Copy()
    {
        ICDescription icd = new ICDescription(this.Name, this.Position, this.Circuit, this.InputOrder, this.OutputOrder);
        icd.ID = this.ID;
        return icd;
    }

    public void SaveToFile(string file)
    {
        string finalFile = file.Contains(".") ? file : file + EXTENSION;
        using (StreamWriter sw = new StreamWriter(finalFile))
        {
            sw.Write(JsonSerializer.Serialize(this));
        }
    }
}