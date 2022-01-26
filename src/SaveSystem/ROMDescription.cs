using LogiX.Components;

namespace LogiX.SaveSystem;

public class ROMDescription : ComponentDescription
{
    [JsonProperty(PropertyName = "romFile")]
    public string ROMFile { get; set; }

    public ROMDescription(Vector2 position, string romFile, List<IODescription> inputs, List<IODescription> outputs) : base(position, inputs, outputs, ComponentType.ROM)
    {
        this.ROMFile = romFile;
    }

    public override Component ToComponent(bool preserveID)
    {
        int addressBits = 0;
        bool multibitInput = false;
        if (this.Inputs.Count == 1)
        {
            multibitInput = true;
            addressBits = this.Inputs[0].Bits;
        }
        else
        {
            addressBits = this.Inputs.Count;
        }
        int outputBits = 0;
        bool multibitOutput = false;
        if (this.Outputs.Count == 1)
        {
            multibitOutput = true;
            outputBits = this.Outputs[0].Bits;
        }
        else
        {
            outputBits = this.Outputs.Count;
        }

        ROM c = new ROM(multibitInput, addressBits, multibitOutput, outputBits, this.Position);
        if (preserveID)
        {
            c.SetUniqueID(this.ID);
        }
        c.ROMFile = this.ROMFile;

        return c;
    }
}