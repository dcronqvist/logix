using LogiX.SaveSystem;

namespace LogiX.Components;

public class ROM : Component
{
    private string romfile_;
    public string ROMFile
    {
        get
        {
            return romfile_;
        }
        set
        {
            romfile_ = value;
            ReloadROM();
        }
    }
    public List<List<LogicValue>> ROMValues { get; set; }
    public override bool DrawIOIdentifiers => true;
    public override string Text => "ROM: " + Path.GetFileNameWithoutExtension(this.ROMFile);
    public override Vector2 Size => this.Inputs.Count == 0 && this.Outputs.Count == 0 ? new Vector2(100, 100) : base.Size;
    private bool multibitAddress;
    private bool multibitOutput;

    public ROM(bool multibitAddress, int addressBits, bool multibitOutput, int outputBits, Vector2 position) : base(multibitAddress ? Util.Listify(addressBits) : Util.NValues(1, addressBits), multibitOutput ? Util.Listify(outputBits) : Util.NValues(1, outputBits), position)
    {
        this.ROMFile = null;
        this.ROMValues = new List<List<LogicValue>>();

        for (int i = 0; i < Inputs.Count(); i++)
        {
            this.InputAt(i).Identifier = $"A{i}";
        }

        for (int i = 0; i < Outputs.Count(); i++)
        {
            this.OutputAt(i).Identifier = $"M{i}";
        }

    }

    public void ReloadROM()
    {
        if (ROMFile != null)
        {
            if (!File.Exists(this.ROMFile))
            {
                return;
            }

            // Load the file and create the ROMValues
            List<List<LogicValue>> values = new List<List<LogicValue>>();
            using (StreamReader sr = new StreamReader(this.ROMFile))
            {
                while (sr.Peek() != -1)
                {
                    string line = sr.ReadLine();

                    List<LogicValue> lineValues = Util.BinaryStringToLogicValues(line);
                    values.Add(lineValues);
                }
            }

            this.ROMValues = values;
        }
    }

    public override void PerformLogic()
    {
        if (this.ROMFile != null)
        {
            int address = 0;

            if (this.Inputs.Count == 1)
            {
                ComponentInput ci = this.InputAt(0);

                for (int i = 0; i < ci.Bits; i++)
                {
                    address += ci.Values[i] == LogicValue.HIGH ? (1 << (i)) : 0;
                }
            }
            else
            {
                for (int i = 0; i < this.Inputs.Count; i++)
                {
                    address += this.InputAt(i).Values[0] == LogicValue.HIGH ? (1 << (i)) : 0;
                }
            }

            List<LogicValue> valuesAtAddress;
            if (address > this.ROMValues.Count)
            {
                valuesAtAddress = Util.NValues(LogicValue.LOW, this.ROMValues[0].Count);
            }
            else
            {
                valuesAtAddress = this.ROMValues[address];
            }


            if (this.Outputs.Count == 1)
            {
                // Multibit
                this.Outputs[0].SetValues(valuesAtAddress);
            }
            else
            {
                // Multiple single bits
                for (int i = 0; i < this.Outputs.Count; i++)
                {
                    ComponentOutput co = this.OutputAt(i);
                    co.SetValues(Util.Listify(valuesAtAddress[i]));
                }
            }
        }
    }

    public override void SubmitContextPopup(Editor.Editor editor)
    {
        base.SubmitContextPopup(editor);
        if (ImGui.Button("Select file"))
        {
            editor.SelectFile(Directory.GetCurrentDirectory(), (file) =>
            {
                this.ROMFile = file;
            }, () => { }, ".txt");
        }
        if (ImGui.Button("Reload"))
        {
            this.ReloadROM();
        }
    }

    public override ComponentDescription ToDescription()
    {
        List<IODescription> inputs = this.Inputs.Select((ci) =>
        {
            return new IODescription(ci.Bits);
        }).ToList();

        List<IODescription> outputs = this.Outputs.Select((co) =>
        {
            return new IODescription(co.Bits);
        }).ToList();

        return new ROMDescription(this.Position, this.ROMFile, inputs, outputs);
    }
}