using LogiX.SaveSystem;

namespace LogiX.Components;

public class BitwiseComponent : Component
{
    private IGateLogic gateLogic;
    private int inputBits;
    private bool multibit;

    public override string Text => "Bitwise " + gateLogic.GetLogicText();
    public override bool HasContextMenu => true;

    public override string? Documentation => @"
# Bitwise Gate Component

Performs bitwise gate operations on the input bits.
";

    public BitwiseComponent(IGateLogic gateLogic, int inputBits, bool multibit, Vector2 position) : base(multibit ? Util.Listify(inputBits, inputBits) : Util.NValues(1, inputBits * 2), multibit ? Util.Listify(inputBits) : Util.NValues(1, inputBits), position)
    {
        this.gateLogic = gateLogic;
        this.inputBits = inputBits;
        this.multibit = multibit;
    }

    public override Dictionary<string, int> GetGateAmount()
    {
        return Util.GateAmount(("Bitwise " + gateLogic.GetLogicText(), 1));
    }

    public override void PerformLogic()
    {
        if (multibit)
        {
            List<LogicValue> a = this.InputAt(0).Values;
            List<LogicValue> b = this.InputAt(1).Values;

            this.OutputAt(0).SetValues(this.PerformBitwiseLogic(gateLogic, a, b));
        }
        else
        {
            List<LogicValue> a = this.Inputs.Take(new Range(0, inputBits)).Select(i => i.Values).Aggregate((a, b) => a.Concat(b).ToList());
            List<LogicValue> b = this.Inputs.Take(new Range(inputBits, inputBits * 2)).Select(i => i.Values).Aggregate((a, b) => a.Concat(b).ToList());
            List<LogicValue> output = this.PerformBitwiseLogic(gateLogic, a, b);
            int i = 0;
            this.Outputs.ForEach(o => o.SetValues(output[i++]));
        }
    }

    public override void SubmitContextPopup(Editor.Editor editor)
    {
        ImGui.SetNextItemWidth(80);
        string[] types = new string[] { "AND", "NAND", "OR", "NOR", "XOR" };
        int curr = types.ToList().IndexOf(this.gateLogic.GetLogicText());
        ImGui.Combo("Logic Type", ref curr, types, types.Length);
        this.gateLogic = Util.GetGateLogicFromName(types[curr]);
        base.SubmitContextPopup(editor);
    }

    public List<LogicValue> PerformBitwiseLogic(IGateLogic logic, List<LogicValue> a, List<LogicValue> b)
    {
        List<LogicValue> result = new List<LogicValue>();
        for (int i = 0; i < a.Count; i++)
        {
            result.Add(logic.PerformLogic(new List<LogicValue>() { a[i], b[i] }));
        }
        return result;
    }

    public override ComponentDescription ToDescription()
    {
        return new BitwiseDescription(this.Position, this.Rotation, this.Inputs.Select(i => new IODescription(i.Bits)).ToList(), this.Outputs.Select(i => new IODescription(i.Bits)).ToList(), this.gateLogic.GetLogicText(), this.inputBits, this.multibit);
    }
}