using LogiX.Components;

namespace LogiX.SaveSystem;

public class GateDescription : ComponentDescription
{
    [JsonProperty(PropertyName = "gateLogic")]
    public string GateLogic { get; set; }

    public GateDescription(Vector2 position, List<IODescription> inputs, List<IODescription> outputs, IGateLogic gateLogic) : base(position, inputs, outputs, ComponentType.Gate)
    {
        this.GateLogic = gateLogic.GetLogicText();
    }

    public IGateLogic GetLogicImplementation()
    {
        switch (this.GateLogic)
        {
            case "AND":
                return new ANDLogic();
            case "NAND":
                return new NANDLogic();
            case "OR":
                return new ORLogic();
            case "NOR":
                return new NORLogic();
            case "XOR":
                return new XORLogic();
            case "NOT":
                return new NOTLogic();
        }

        return null;
    }

    public override Component ToComponent()
    {
        if (this.Inputs.Count == 1)
        {
            // Multibit
            return new LogicGate(this.Inputs[0].Bits, true, this.GetLogicImplementation(), this.Position);
        }
        else
        {
            // not multibit
            return new LogicGate(this.Inputs.Count, false, this.GetLogicImplementation(), this.Position);
        }

    }
}