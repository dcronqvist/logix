namespace LogiX.Components;

public class LogicGate : Component
{
    private IGateLogic Logic { get; set; }

    public LogicGate(IGateLogic gateLogic) : base(Util.Listify(1, 1), Util.Listify(1))
    {
        this.Logic = gateLogic;
    }

    public LogicGate(int inputBits, IGateLogic gateLogic) : base(Util.Listify(inputBits), Util.Listify(1))
    {
        if (inputBits < 2)
        {
            throw new ArgumentException("Amount of bits for multibit gate must be at least 2.");
        }

        this.Logic = gateLogic;
    }

    public override void PerformLogic()
    {
        this.OutputAt(0).SetValues(this.Logic.PerformLogic(this.Inputs));
    }
}