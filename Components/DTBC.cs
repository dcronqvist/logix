using LogiX.SaveSystem;

namespace LogiX.Components;

public class DTBC : Component
{
    int decimals;

    public override string Text => "DBTC";
    public override bool DrawIOIdentifiers => true;

    public DTBC(int decimals, bool multibit, Vector2 position) : base(multibit ? Util.Listify(decimals - 1) : Util.NValues(1, decimals - 1), multibit ? Util.Listify((int)Math.Round(Math.Log2(decimals - 1))) : Util.NValues(1, (int)Math.Round(Math.Log2(decimals - 1))), position)
    {
        this.decimals = decimals;

        if (multibit)
        {
            this.InputAt(0).Identifier = $"D{decimals - 1}-D1";

            this.OutputAt(0).Identifier = $"Q{(int)Math.Round(Math.Log2(decimals - 1))}-Q0";
        }
        else
        {
            for (int i = 0; i < decimals - 1; i++)
            {
                this.InputAt(i).Identifier = $"D{i + 1}";
            }

            for (int i = 0; i < (int)Math.Round(Math.Log2(decimals - 1)); i++)
            {
                this.OutputAt(i).Identifier = $"Q{i}";
            }
        }
    }

    public override Dictionary<string, int> GetGateAmount()
    {
        return Util.GateAmount(("DBTC", 1));
    }

    public override void PerformLogic()
    {
        // Convert decimal to binary from input to output
        int dec = 0;

        if (this.InputAt(0).Bits > 1)
        {
            for (int i = 0; i < this.InputAt(0).Bits; i++)
            {
                if (this.InputAt(0).Values[i] == LogicValue.HIGH)
                {
                    if (dec != 0)
                    {
                        dec = 0;
                        break;
                    }
                    else
                    {
                        dec = i + 1;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < this.Inputs.Count; i++)
            {
                if (this.InputAt(i).Values[0] == LogicValue.HIGH)
                {
                    if (dec != 0)
                    {
                        dec = 0;
                        break;
                    }
                    else
                    {
                        dec = i + 1;
                    }
                }
            }
        }

        // Reset outputs
        for (int i = 0; i < this.Outputs.Count; i++)
        {
            this.OutputAt(i).SetAllValues(LogicValue.LOW);
        }

        // Set output
        if (this.OutputAt(0).Bits > 1)
        {
            // Multibit
            this.OutputAt(0).SetValues(Util.GetLogicValuesRepresentingDecimal(dec, this.OutputAt(0).Bits));
        }
        else
        {
            // Single bit
            List<LogicValue> vals = Util.GetLogicValuesRepresentingDecimal(dec, this.Outputs.Count);
            for (int i = 0; i < this.Outputs.Count; i++)
            {
                this.OutputAt(i).SetValues(vals[i]);
            }
        }

    }

    public override ComponentDescription ToDescription()
    {
        return new DTBCDescription(this.decimals, this.Inputs.Count == 1, this.Position);
    }

}