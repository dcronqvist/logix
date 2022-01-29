using LogiX.SaveSystem;

namespace LogiX.Components;

public class Multiplexer : Component
{
    public int selectorBits;
    public int dataBits;

    public override bool DrawIOIdentifiers => true;
    public override string Text => "MUX";

    public Multiplexer(int selectorBits, bool selectorMultibit, int dataBits, bool dataMultibit, Vector2 position) : base(GetBitsPerInput(selectorBits, selectorMultibit, dataBits, dataMultibit), dataMultibit ? Util.Listify(dataBits) : Util.NValues(1, dataBits), position)
    {
        this.selectorBits = selectorBits;
        this.dataBits = dataBits;
        int startInput = selectorMultibit ? 1 : selectorBits;

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

        for (int i = startInput; i < Inputs.Count(); i++)
        {
            this.InputAt(i).Identifier = $"{i - startInput}";
        }

        if (dataMultibit)
        {
            this.OutputAt(0).Identifier = $"Q{dataBits - 1}-Q0";
        }
        else
        {
            for (int i = 0; i < dataBits; i++)
            {
                this.OutputAt(i).Identifier = $"Q{i}";
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
            for (int i = 0; i < allowedDatas; i++)
            {
                bitsPerInput.Add(dataBits);
            }
        }
        else
        {
            for (int i = 0; i < allowedDatas; i++)
            {
                for (int j = 0; j < dataBits; j++)
                {
                    bitsPerInput.Add(1);
                }
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

        if (this.OutputAt(0).Bits > 1)
        {
            // Multibit output => multibit input
            this.OutputAt(0).SetValues(this.Inputs[address + offsetAddress].Values);
        }
        else
        {
            // Singlebit output => singlebit input
            for (int i = 0; i < this.Outputs.Count; i++)
            {
                this.OutputAt(i).SetValues(Inputs[address + offsetAddress + i].Values);
            }
        }
    }

    public override ComponentDescription ToDescription()
    {
        return new MUXDescription(this.Position, this.Rotation, this.selectorBits, this.InputAt(0).Bits > 1, this.dataBits, this.OutputAt(0).Bits > 1, ComponentType.Mux);
    }

    public override Dictionary<string, int> GetGateAmount()
    {
        return Util.GateAmount(("Multiplexer", 1));
    }
}