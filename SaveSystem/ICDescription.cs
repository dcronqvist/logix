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
}