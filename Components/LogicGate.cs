namespace LogiX.Components;

public class LogicGate : Component
{
    private IGateLogic Logic { get; set; }
    public override string Text => this.Logic.GetLogicText();

    public LogicGate(int inputBits, bool multibit, IGateLogic gateLogic, Vector2 position) : base(multibit ? Util.Listify(inputBits) : Util.NValues(1, inputBits), Util.Listify(1), position)
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