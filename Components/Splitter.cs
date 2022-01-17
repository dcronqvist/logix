using LogiX.SaveSystem;

namespace LogiX.Components;

public class Splitter : Component
{
    public int InBits { get; set; }
    public int OutBits { get; set; }
    public bool MultibitIn { get; set; }
    public bool MultibitOut { get; set; }

    public override bool DrawIOIdentifiers => true;
    public override bool TextVisible => false;
    public override string Text => "";

    public Splitter(int inBits, int outBits, bool min, bool mout, Vector2 position) : base(min ? Util.Listify(inBits) : Util.NValues(1, inBits), mout ? Util.Listify(outBits) : Util.NValues(1, outBits), position)
    {
        this.InBits = inBits;
        this.OutBits = outBits;

        this.MultibitIn = min;
        this.MultibitOut = mout;

        if (min)
        {
            this.Inputs[0].Identifier = $"D{inBits - 1}-D0";
        }
        else
        {
            for (int i = 0; i < inBits; i++)
            {
                this.Inputs[i].Identifier = $"D{i}";
            }
        }

        if (mout)
        {
            this.Outputs[0].Identifier = $"D{outBits - 1}-D0";
        }
        else
        {
            for (int i = 0; i < outBits; i++)
            {
                this.Outputs[i].Identifier = $"D{i}";
            }
        }
    }

    public override void PerformLogic()
    {
        List<LogicValue> inValues;
        if (this.MultibitIn)
        {
            inValues = this.InputAt(0).Values;
        }
        else
        {
            inValues = this.Inputs.Select(x => x.Values[0]).ToList();
        }

        if (this.MultibitOut)
        {
            this.OutputAt(0).SetValues(inValues);
        }
        else
        {
            for (int i = 0; i < this.OutBits; i++)
            {
                this.Outputs[i].SetValues(inValues[i]);
            }
        }
    }

    public override ComponentDescription ToDescription()
    {
        return new SplitterDescription(this.Position, this.Inputs.Select(x => new IODescription(x.Bits)).ToList(), this.Outputs.Select(x => new IODescription(x.Bits)).ToList());
    }

    public override Dictionary<string, int> GetGateAmount()
    {
        return Util.GateAmount(("Splitter", 1));
    }
}