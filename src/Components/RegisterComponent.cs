using LogiX.SaveSystem;

namespace LogiX.Components;

public class RegisterComponent : Component
{
    public int DataBits { get; set; }
    public bool Multibit { get; set; }

    public override bool DrawIOIdentifiers => true;
    public override string Text => "Register";

    public List<LogicValue> StoredValue { get; set; }

    public override string? Documentation => @"
# Register Component

This component is used to store values. It can store between 1 and a virtually infinite amount of bits.
As the amount of bits increase however, the performance of the simulation will decrease.

A register with N data bits functions in the following way:

* Connect the bits you want to store to DN-D0.
* If at any point RST is set to HIGH, all stored bits will be set to LOW.
* On rising edges of CLK, the bits inputted at DN-D0 will be stored in the register, instantly. However, this only happens if LD is HIGH.
* The stored bits are always outputted at QN-Q0.

Important to note is that the DN-D0 and QN-Q0 will consider the Nth bit to be the most significant bit, and the 0th bit to be the least significant bit. When connected to a switch of N bits, the most
significant bit will be the leftmost one, which matches our convention of writing binary.
";

    public RegisterComponent(int databits, bool multibit, Vector2 position) : base(multibit ? Util.Listify(databits, 1, 1, 1) : Util.NValues(1, databits + 3), multibit ? Util.Listify(databits) : Util.NValues(1, databits), position)
    {
        DataBits = databits;
        Multibit = multibit;
        this.StoredValue = Util.NValues(LogicValue.LOW, databits);
        this.NameIOs();
    }

    public void NameIOs()
    {
        if (this.Multibit)
        {
            this.InputAt(0).Identifier = $"D{this.DataBits - 1}-D0";
            this.InputAt(1).Identifier = "LD";
            this.InputAt(2).Identifier = "CLK";
            this.InputAt(3).Identifier = "RST";
            this.OutputAt(0).Identifier = $"Q{this.DataBits - 1}-Q0";
        }
        else
        {
            Enumerable.Range(0, this.DataBits).ToList().ForEach(x => this.InputAt(x).Identifier = $"D{x}");
            this.InputAt(this.DataBits).Identifier = "LD";
            this.InputAt(this.DataBits + 1).Identifier = "CLK";
            this.InputAt(this.DataBits + 2).Identifier = "RST";
            Enumerable.Range(0, this.DataBits).ToList().ForEach(x => this.OutputAt(x).Identifier = $"Q{x}");
        }
    }

    public override Dictionary<string, int> GetGateAmount()
    {
        throw new NotImplementedException();
    }

    LogicValue previousClock;

    public bool RisingEdgeClock(LogicValue value)
    {
        if (this.previousClock == LogicValue.LOW && value == LogicValue.HIGH)
        {
            this.previousClock = value;
            return true;
        }
        this.previousClock = value;
        return false;
    }

    public override void PerformLogic()
    {
        List<LogicValue> inputValue = this.Multibit ? this.InputAt(0).Values : this.Inputs.GetRange(0, this.DataBits).Select(x => x.Values[0]).ToList();
        LogicValue loadInput = this.Multibit ? this.InputAt(1).Values[0] : this.Inputs[this.DataBits].Values[0];
        LogicValue clockInput = this.Multibit ? this.InputAt(2).Values[0] : this.Inputs[this.DataBits + 1].Values[0];
        LogicValue resetInput = this.Multibit ? this.InputAt(3).Values[0] : this.Inputs[this.DataBits + 2].Values[0];

        if (RisingEdgeClock(clockInput))
        {
            if (loadInput == LogicValue.HIGH)
            {
                this.StoredValue = inputValue;
            }
        }

        if (resetInput == LogicValue.HIGH)
        {
            this.StoredValue = Util.NValues(LogicValue.LOW, this.DataBits);
        }

        if (this.Multibit)
        {
            this.OutputAt(0).SetValues(this.StoredValue);
        }
        else
        {
            Enumerable.Range(0, this.DataBits).ToList().ForEach(x => this.Outputs[x].SetValues(this.StoredValue[x]));
        }
    }

    public override ComponentDescription ToDescription()
    {
        return new RegisterDescription(this.Position, this.DataBits, this.Multibit, this.Rotation);
    }
}