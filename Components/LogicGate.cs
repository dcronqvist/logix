using LogiX.SaveSystem;

namespace LogiX.Components;

public class LogicGate : Component
{
    private IGateLogic Logic { get; set; }
    public override string Text => this.Logic.GetLogicText();

    private int newGateBits;
    private bool newGateMultibit;

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
        this.OutputAt(0).SetValues(this.Logic.PerformLogic(this.Inputs));
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

        return new GateDescription(this.Position, inputs, outputs, this.Logic);
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