using LogiX.Components;

namespace LogiX.SaveSystem;

public class MemoryDescription : ComponentDescription
{
    [JsonPropertyName("memory")]
    public List<LogicValue>[] Memory { get; set; }

    public MemoryDescription(Vector2 position, int rotation, List<LogicValue>[] memory, List<IODescription> inputs, List<IODescription> outputs) : base(position, inputs, outputs, rotation, ComponentType.Memory)
    {
        this.Memory = memory;
    }

    public override Component ToComponent(bool preserveIDs)
    {
        int addressBits = 0;
        int dataBits = 0;
        bool multibitAddress = false;
        bool multibitOutput = false;
        if (this.Outputs[0].Bits > 1)
        {
            // Multibit output
            dataBits = this.Outputs[0].Bits;
            multibitOutput = true;
        }
        else
        {
            // Not multibit output
            dataBits = this.Outputs.Count;
            multibitOutput = false;
        }

        if (this.Inputs[0].Bits > 1)
        {
            // Multibit input
            addressBits = this.Inputs[0].Bits;
            multibitAddress = true;
        }
        else
        {
            // Not multibit input
            addressBits = this.Inputs.Count - dataBits - 3;
            multibitAddress = false;
        }

        MemoryComponent c = new MemoryComponent(addressBits, multibitAddress, dataBits, multibitOutput, this.Position);
        List<LogicValue>[] memory = new List<LogicValue>[c.Memory.Length];
        for (int i = 0; i < memory.Length; i++)
        {
            memory[i] = new List<LogicValue>();
            for (int j = 0; j < dataBits; j++)
            {
                memory[i].Add(this.Memory[i][j]);
            }
        }
        c.Memory = memory;
        if (preserveIDs)
            c.SetUniqueID(this.ID);

        c.Rotation = Rotation;
        return c;
    }
}