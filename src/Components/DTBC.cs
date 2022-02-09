using LogiX.SaveSystem;

namespace LogiX.Components;

public class DTBC : Component
{
    int decimals;

    public override string Text => "DTBC";
    public override bool DrawIOIdentifiers => true;
    public override string? Documentation => @"
# Decimal to Binary Component

This component converts a decimal number (base 10) to binary (base 2), and outputs the result.
The component can be configured to have a certain number of decimal numbers, and will output the binary result in the least amount of bits needed to represent that maximum decimal number.

To output a binary 0, you must not have any of the inputs HIGH.
You may only set one input HIGH at a time, if more are set, the component will output a binary 0.

";

    public DTBC(int decimals, bool multibit, Vector2 position) : base(multibit ? Util.Listify(decimals - 1) : Util.NValues(1, decimals - 1), multibit ? Util.Listify((int)Math.Ceiling(Math.Log2(decimals - 1))) : Util.NValues(1, (int)Math.Ceiling(Math.Log2(decimals - 1))), position)
    {
        this.decimals = decimals;

        if (multibit)
        {
            this.InputAt(0).Identifier = $"D{decimals - 1}-D1";

            this.OutputAt(0).Identifier = $"Q{(int)Math.Ceiling(Math.Log2(decimals - 1))}-Q0";
        }
        else
        {
            for (int i = 0; i < decimals - 1; i++)
            {
                this.InputAt(i).Identifier = $"D{i + 1}";
            }

            for (int i = 0; i < (int)Math.Ceiling(Math.Log2(decimals - 1)); i++)
            {
                this.OutputAt(i).Identifier = $"Q{i}";
            }
        }
    }

    public override Dictionary<string, int> GetGateAmount()
    {
        return Util.GateAmount(("DTBC", 1));
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
        return new DTBCDescription(this.decimals, this.Inputs.Count == 1, this.Position, this.Rotation);
    }

}