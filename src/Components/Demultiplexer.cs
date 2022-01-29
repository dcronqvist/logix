using LogiX.SaveSystem;

namespace LogiX.Components;

public class Demultiplexer : Component
{

    public int selectorBits;
    public int dataBits;

    public override bool DrawIOIdentifiers => true;
    public override string Text => "DEMUX";

    public Demultiplexer(int selectorBits, bool selectorMultibit, int dataBits, bool dataMultibit, Vector2 position) : base(GetBitsPerInput(selectorBits, selectorMultibit, dataBits, dataMultibit), dataMultibit ? Util.NValues(dataBits, (int)Math.Pow(2, selectorBits)) : Util.NValues(1, dataBits * (int)Math.Pow(2, selectorBits)), position)
    {
        this.selectorBits = selectorBits;
        this.dataBits = dataBits;

        if (selectorMultibit)
        {
            this.InputAt(0).Identifier = $"S{selectorBits - 1}-S0";
        }
        else
        {
            for (int i = 0; i < selectorBits; i++)
            {
                this.InputAt(i).Identifier = $"S{i}";
            }
        }

        if (dataMultibit)
        {
            this.InputAt(selectorMultibit ? 1 : selectorBits).Identifier = $"D{dataBits - 1}-D0";

            for (int i = 0; i < (int)Math.Pow(2, selectorBits); i++)
            {
                this.OutputAt(i).Identifier = $"{i}";
            }
        }
        else
        {
            for (int i = 0; i < (int)Math.Pow(2, selectorBits); i++)
            {
                this.InputAt(selectorMultibit ? 1 : selectorBits + i).Identifier = $"D{i}";

                for (int j = 0; j < dataBits; j++)
                {
                    this.OutputAt(i * dataBits + j).Identifier = $"{i}-{j}";
                }
            }
        }
    }

    public static List<int> GetBitsPerInput(int selectorBits, bool selectorMultibit, int dataBits, bool dataMultibit)
    {
        List<int> bitsPerInput = new List<int>();

        if (selectorMultibit)
        {
            bitsPerInput.Add(selectorBits);
        }
        else
        {
            for (int i = 0; i < selectorBits; i++)
            {
                bitsPerInput.Add(1);
            }
        }

        int allowedDatas = (int)Math.Pow(2, selectorBits);

        if (dataMultibit)
        {
            bitsPerInput.Add(dataBits);
        }
        else
        {
            for (int j = 0; j < dataBits; j++)
            {
                bitsPerInput.Add(1);
            }
        }

        return bitsPerInput;
    }

    public override void PerformLogic()
    {
        int address = 0;
        int offsetAddress = 0;

        if (this.Inputs[0].Bits > 1)
        {
            // Multibit address

            ComponentInput ci = this.InputAt(0);

            for (int i = 0; i < ci.Bits; i++)
            {
                address += ci.Values[i] == LogicValue.HIGH ? (1 << (i)) : 0;
            }
            offsetAddress = 1;
        }
        else
        {
            for (int i = 0; i < this.selectorBits; i++)
            {
                address += this.InputAt(i).Values[0] == LogicValue.HIGH ? (1 << (i)) : 0;
            }
            offsetAddress = this.selectorBits;
        }

        for (int i = 0; i < this.Outputs.Count; i++)
        {
            this.OutputAt(i).SetAllValues(LogicValue.LOW);
        }

        if (this.OutputAt(0).Bits > 1)
        {
            // Multibit output => multibit input
            this.OutputAt(address).SetValues(this.Inputs[offsetAddress].Values);
        }
        else
        {
            // Singlebit output => singlebit input
            for (int i = 0; i < this.Outputs[0].Bits; i++)
            {
                this.OutputAt(address * this.dataBits + i).SetValues(Inputs[offsetAddress].Values);
            }
        }
    }

    public override ComponentDescription ToDescription()
    {
        return new MUXDescription(this.Position, this.Rotation, this.selectorBits, this.InputAt(0).Bits > 1, this.dataBits, this.OutputAt(0).Bits > 1, ComponentType.Demux);
    }

    public override Dictionary<string, int> GetGateAmount()
    {
        return Util.GateAmount(("Demultiplexer", 1));
    }

}