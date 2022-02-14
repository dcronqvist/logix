using LogiX.SaveSystem;

namespace LogiX.Components;

public class LogicGate : Component
{
    private IGateLogic Logic { get; set; }
    public override string Text => this.Logic.GetLogicText();
    public override bool HasContextMenu => true;
    public override string? Documentation => @"

# Logic Gate

These are the basic building blocks of all logic circuits.

The below gates can be configured to have between 2 and a virtually infinite amount of inputs.
When configured to have 2 inputs, the gate will behave as expected.

When configured to have more than 2 inputs, the gates will instead behave as follows:

* AND: The gate will return HIGH if all inputs are HIGH, otherwise LOW.
* NAND: The gate will return LOW if all inputs are HIGH, otherwise HIGH.
* OR: The gate will return HIGH if at least one input is HIGH, otherwise LOW.
* NOR: The gate will return LOW if at least one input is HIGH, otherwise HIGH.
* XOR: The gate will return HIGH if an uneven amount of inputs are HIGH, otherwise LOW.

There is also a NOT gate, which is a special gate since it only has one input. It cannot be configured to have more than 1 input, and therefore always behaves as an inverter of the single input.

";

    public LogicGate(int inputBits, bool multibit, IGateLogic gateLogic, Vector2 position) : base(multibit ? Util.Listify(inputBits) : Util.NValues(1, inputBits), Util.Listify(1), position)
    {
        if (inputBits < gateLogic.MinBits() || inputBits > gateLogic.MaxBits())
        {
            throw new Exception($"Amount of bits must be between {gateLogic.MinBits()} and {gateLogic.MaxBits()} for {gateLogic.GetLogicText()} gates.");
        }
        this.Logic = gateLogic;
    }

    public override void PerformLogic()
    {
        this.OutputAt(0).SetValues(this.Logic.PerformLogic(this.Inputs.Select(i => i.Values).Aggregate((a, b) => a.Concat(b).ToList())));
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

        return new GateDescription(this.Position, this.Rotation, inputs, outputs, this.Logic);
    }

    private int currentlySelectedLogic = 0;
    public override void SubmitContextPopup(Editor.Editor editor)
    {
        base.SubmitContextPopup(editor);
        string[] items = new string[5] {
            "AND",
            "NAND",
            "OR",
            "NOR",
            "XOR"
        };

        if (ImGui.Combo("Change Logic", ref this.currentlySelectedLogic, items, items.Length))
        {
            this.Logic = Util.GetGateLogicFromName(items[this.currentlySelectedLogic]);
        }
    }

    public override Dictionary<string, int> GetGateAmount()
    {
        return new Dictionary<string, int>() {
            { this.Logic.GetLogicText(), 1 }
        };
    }
}