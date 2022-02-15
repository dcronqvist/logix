using LogiX.SaveSystem;

namespace LogiX.Components;

public class AdderComponent : Component
{
    /*
        CIN
        A(bits)      S(bits)
        B(bits)      COUT
    */

    public int Bits { get; set; }
    public bool MultiBit { get; set; }

    public override string Text => "Adder";
    public override bool DrawIOIdentifiers => true;
    public override string? Documentation => @"
# Adder Component

This component adds the values of the inputs and outputs the result.

SN-S0 = AN-A0 + BN-B0 + CIN

COUT = AN-A0 + BN-B0 + CIN > 2^N - 1
";

    public AdderComponent(int bits, bool multibit, Vector2 position) : base(multibit ? Util.Listify(1, bits, bits) : Util.NValues(1, 1 + bits * 2), multibit ? Util.Listify(bits, 1) : Util.NValues(1, bits + 1), position)
    {
        this.Bits = bits;
        this.MultiBit = multibit;

        if (!multibit)
        {
            this.InputAt(0).Identifier = "CIN";
            for (int i = 0; i < bits; i++)
            {
                this.InputAt(i + 1).Identifier = "A" + i;
                this.InputAt(i + 1 + bits).Identifier = "B" + i;
                this.OutputAt(i).Identifier = "S" + i;
            }
            this.OutputAt(bits).Identifier = "COUT";
        }
        else
        {
            this.InputAt(0).Identifier = "CIN";
            this.InputAt(1).Identifier = $"A{bits - 1}-A0";
            this.InputAt(2).Identifier = $"B{bits - 1}-B0";
            this.OutputAt(0).Identifier = $"S{bits - 1}-S0";
            this.OutputAt(1).Identifier = "COUT";
        }
    }

    public override Dictionary<string, int> GetGateAmount()
    {
        // For every bit there is, one full adder exists
        // Each full adder contains 2 XOR gates, 1 OR and 2 AND gates
        return Util.GateAmount(("Full Adder", this.Bits), ("XOR", this.Bits * 2), ("OR", this.Bits), ("AND", this.Bits * 2));
    }

    public override void PerformLogic()
    {
        int carryIn = this.InputAt(0).Values[0].GetAsInt();
        long a = 0;
        long b = 0;

        if (!this.MultiBit)
        {
            // Get a, index 1 -> (bits+1)
            for (int i = 1; i < this.Bits + 1; i++)
            {
                a += this.InputAt(i).Values[0].GetAsInt() * (int)Math.Pow(2, i - 1);
            }
            // Get b
            for (int i = this.Bits + 1; i < this.Bits * 2 + 1; i++)
            {
                b += this.InputAt(i).Values[0].GetAsInt() * (int)Math.Pow(2, i - this.Bits - 1);
            }
        }
        else
        {
            a = this.InputAt(1).Values.GetAsLong();
            b = this.InputAt(2).Values.GetAsLong();
        }

        long sum = (a + b + carryIn) % (long)Math.Pow(2, this.Bits);
        long carryOut = (a + b + carryIn) > (long)Math.Pow(2, this.Bits) - 1 ? 1 : 0;

        if (!this.MultiBit)
        {
            List<LogicValue> values = sum.GetAsLogicValues(this.Bits);
            for (int i = 0; i < this.Bits; i++)
            {
                this.OutputAt(i).SetValues(values[i]);
            }
            this.OutputAt(this.Bits).SetValues(carryOut.GetAsLogicValues(1));
        }
        else
        {
            this.OutputAt(0).SetValues(sum.GetAsLogicValues(this.Bits));
            this.OutputAt(1).SetValues(carryOut.GetAsLogicValues(1));
        }
    }

    public override ComponentDescription ToDescription()
    {
        return new ArithmeticDescription(this.Position, this.Rotation, this.Inputs.Select(x => new IODescription(x.Bits)).ToList(), this.Outputs.Select(x => new IODescription(x.Bits)).ToList(), ArithmeticType.ADDER, this.Bits, this.MultiBit);
    }
}