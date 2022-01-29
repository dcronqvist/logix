using LogiX.SaveSystem;

namespace LogiX.Components;

public class ConstantComponent : Component
{
    private LogicValue value;
    public LogicValue Value
    {
        get { return this.value; }
        set
        {
            this.value = value;
            this.Outputs[0].SetValues(Util.Listify(value));
        }
    }
    public override string Text => this.Value == LogicValue.HIGH ? "1" : "0";

    public ConstantComponent(LogicValue initialValue, Vector2 position) : base(Util.EmptyList<int>(), Util.Listify(1), position)
    {
        this.Value = initialValue;
    }

    public override void PerformLogic()
    {

    }

    public override ComponentDescription ToDescription()
    {
        return new ConstantDescription(this.Position, this.Rotation, this.Value);
    }

    public override void OnSingleSelectedSubmitUI()
    {
        ImGui.Begin("Constant");

        int value = this.Value == LogicValue.HIGH ? 1 : 0;
        if (ImGui.Combo("Value", ref value, "0\01\0"))
        {
            this.Value = value == 1 ? LogicValue.HIGH : LogicValue.LOW;
        }
    }

    public override Dictionary<string, int> GetGateAmount()
    {
        return Util.EmptyGateAmount();
    }
}