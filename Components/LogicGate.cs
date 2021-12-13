using LogiX.SaveSystem;

namespace LogiX.Components;

public class LogicGate : Component
{
    private IGateLogic Logic { get; set; }
    public override string Text => this.Logic.GetLogicText();

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
}