using LogiX.SaveSystem;

namespace LogiX.Components;

public class MultiplierComponent : Component
{
    /*
        CIN
        A(bits)      S(bits)
        B(bits)      COUT
    */

    public int Bits { get; set; }
    public bool MultiBit { get; set; }

    public override string Text => "Multiplier";
    public override bool DrawIOIdentifiers => true;

    public MultiplierComponent(int bits, bool multibit, Vector2 position) : base(multibit ? Util.Listify(bits, bits) : Util.NValues(1, bits * 2), multibit ? Util.Listify(bits * 2) : Util.NValues(1, bits * 2), position)
    {
        this.Bits = bits;
        this.MultiBit = multibit;

        if (!multibit)
        {
            for (int i = 0; i < bits; i++)
            {
                this.InputAt(i).Identifier = "A" + i;
                this.InputAt(i + bits).Identifier = "B" + i;
                this.OutputAt(i).Identifier = "P" + i;
                this.OutputAt(i + bits).Identifier = "P" + (i + bits);
            }
        }
        else
        {
            this.InputAt(0).Identifier = $"A{bits - 1}-A0";
            this.InputAt(1).Identifier = $"B{bits - 1}-B0";
            this.OutputAt(0).Identifier = $"P{bits * 2 - 1}-P0";
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
        int a = 0;
        int b = 0;

        if (!this.MultiBit)
        {
            // Get a, index 1 -> (bits+1)
            for (int i = 0; i < this.Bits; i++)
            {
                a += this.InputAt(i).Values[0].GetAsInt() * (int)Math.Pow(2, i);
            }
            // Get b
            for (int i = this.Bits; i < this.Bits * 2; i++)
            {
                b += this.InputAt(i).Values[0].GetAsInt() * (int)Math.Pow(2, i - this.Bits);
            }
        }
        else
        {
            a = this.InputAt(0).Values.GetAsInt();
            b = this.InputAt(1).Values.GetAsInt();
        }

        int product = (a * b) % (int)Math.Pow(2, this.Bits * 2);

        if (!this.MultiBit)
        {
            List<LogicValue> values = product.GetAsLogicValues(this.Bits * 2);
            for (int i = 0; i < this.Bits * 2; i++)
            {
                this.OutputAt(i).SetValues(values[i]);
            }
        }
        else
        {
            this.OutputAt(0).SetValues(product.GetAsLogicValues(this.Bits * 2));
        }
    }

    public override ComponentDescription ToDescription()
    {
        return new ArithmeticDescription(this.Position, this.Rotation, this.Inputs.Select(x => new IODescription(x.Bits)).ToList(), this.Outputs.Select(x => new IODescription(x.Bits)).ToList(), ArithmeticType.MULTIPLIER, this.Bits, this.MultiBit);
    }
}