using LogiX.SaveSystem;

namespace LogiX.Components;

public class MemoryComponent : Component
{
    public int AddressBits { get; set; }
    public int DataBits { get; set; }
    public bool MultibitAddress { get; set; }
    public bool MultibitOutput { get; set; }

    public List<LogicValue>[] Memory { get; set; }

    public override bool DrawIOIdentifiers => true;
    public override string Text => $"{1 << this.AddressBits}x{this.DataBits} Bit Memory";

    public bool previousClock = false;

    public MemoryComponent(int addressBits, bool multibitAddress, int dataBits, bool multibitOutput, Vector2 position) : base(multibitAddress ? Util.Listify(addressBits, dataBits, 1, 1, 1) : Util.NValues(1, addressBits + dataBits + 3), multibitOutput ? Util.Listify(dataBits) : Util.NValues(1, dataBits), position)
    {
        this.AddressBits = addressBits;
        this.DataBits = dataBits;

        this.MultibitAddress = multibitAddress;
        this.MultibitOutput = multibitOutput;

        this.Memory = new List<LogicValue>[1 << addressBits];

        if (multibitAddress)
        {
            this.Inputs[0].Identifier = $"A{addressBits - 1}-A0";
            this.Inputs[1].Identifier = $"D{dataBits - 1}-D0";
            this.Inputs[2].Identifier = "LOAD";
            this.Inputs[3].Identifier = "CLK";
            this.Inputs[4].Identifier = "R";
        }
        else
        {
            for (int i = 0; i < addressBits; i++)
            {
                this.Inputs[i].Identifier = $"A{i}";
            }
            for (int i = 0; i < dataBits; i++)
            {
                this.Inputs[addressBits + i].Identifier = $"D{i}";
            }
            this.Inputs[addressBits + dataBits].Identifier = "LOAD";
            this.Inputs[addressBits + dataBits + 1].Identifier = "CLK";
            this.Inputs[addressBits + dataBits + 2].Identifier = "R";
        }

        if (multibitOutput)
        {
            this.Outputs[0].Identifier = $"D{dataBits - 1}-D0";
        }
        else
        {
            for (int i = 0; i < dataBits; i++)
            {
                this.Outputs[i].Identifier = $"D{i}";
            }
        }

        this.ResetMemory();
    }

    public void ResetMemory()
    {
        for (int i = 0; i < this.Memory.Length; i++)
        {
            this.Memory[i] = Util.NValues(LogicValue.LOW, this.DataBits);
        }
    }

    public override void PerformLogic()
    {
        int address = 0;
        LogicValue[] data = new LogicValue[this.DataBits];
        bool loading = false;
        bool clk = false;
        bool r = false;
        if (this.MultibitAddress)
        {
            for (int i = 0; i < this.AddressBits; i++)
            {
                address += this.Inputs[0].Values[i] == LogicValue.HIGH ? (1 << (i)) : 0;
            }
            for (int i = 0; i < this.DataBits; i++)
            {
                data[i] = this.Inputs[1].Values[i];
            }
            loading = this.Inputs[2].Values[0] == LogicValue.HIGH;
            clk = this.Inputs[3].Values[0] == LogicValue.HIGH;
            r = this.Inputs[4].Values[0] == LogicValue.HIGH;
        }
        else
        {
            for (int i = 0; i < this.AddressBits; i++)
            {
                address += this.Inputs[i].Values[0] == LogicValue.HIGH ? (1 << (i)) : 0;
            }
            for (int i = 0; i < this.DataBits; i++)
            {
                data[i] = this.Inputs[this.AddressBits + i].Values[0];
            }
            loading = this.Inputs[this.AddressBits + this.DataBits].Values[0] == LogicValue.HIGH;
            clk = this.Inputs[this.AddressBits + this.DataBits + 1].Values[0] == LogicValue.HIGH;
            r = this.Inputs[this.AddressBits + this.DataBits + 2].Values[0] == LogicValue.HIGH;
        }

        if (r)
        {
            this.ResetMemory();
            if (this.MultibitOutput)
            {
                for (int i = 0; i < this.DataBits; i++)
                {
                    this.Outputs[0].Values[i] = this.Memory[address][i];
                }
            }
            else
            {
                for (int i = 0; i < this.DataBits; i++)
                {
                    this.Outputs[i].Values[0] = this.Memory[address][i];
                }
            }
            return;
        }

        if (loading && clk && !previousClock)
        {
            this.Memory[address] = data.ToList();
        }

        // Set output to data att current address
        if (this.MultibitOutput)
        {
            for (int i = 0; i < this.DataBits; i++)
            {
                this.Outputs[0].Values[i] = this.Memory[address][i];
            }
        }
        else
        {
            for (int i = 0; i < this.DataBits; i++)
            {
                this.Outputs[i].Values[0] = this.Memory[address][i];
            }
        }

        previousClock = clk;
    }

    public override ComponentDescription ToDescription()
    {
        List<IODescription> inputs = this.Inputs.Select(i => new IODescription(i.Bits)).ToList();
        List<IODescription> outputs = this.Outputs.Select(i => new IODescription(i.Bits)).ToList();
        return new MemoryDescription(this.Position, this.Memory, inputs, outputs);
    }

    public override void SubmitContextPopup(Editor.Editor editor)
    {
        base.SubmitContextPopup(editor);

        if (ImGui.Button("Dump Memory to File..."))
        {
            editor.SelectFolder(Util.FileDialogStartDir, folder =>
            {
                using (StreamWriter sw = new StreamWriter(folder + "/memory.txt"))
                {
                    for (int i = 0; i < this.Memory.Length; i++)
                    {
                        sw.WriteLine(Util.LogicValuesToBinaryString(this.Memory[i]));
                    }
                }
            });
        }

        if (ImGui.Button("Load Memory From File..."))
        {
            editor.SelectFile(Util.FileDialogStartDir, file =>
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    this.ResetMemory();
                    this.Memory = Util.ReadROM(file).ToArray();
                }
            }, ".txt");
        }
    }

    public override Dictionary<string, int> GetGateAmount()
    {
        return Util.GateAmount(("Memory", 1));
    }
}