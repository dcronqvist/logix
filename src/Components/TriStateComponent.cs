using LogiX.SaveSystem;

namespace LogiX.Components;

public class TriStateComponent : Component
{
    private int _bits;
    [ComponentProp("Bits", IntMin = 1)]
    public int Bits
    {
        get => _bits;
        set
        {
            _bits = value;
            this.GetIO(0).UpdateBitWidth(value);
            this.GetIO(1).UpdateBitWidth(value);
        }
    }

    public TriStateComponent(Vector2 position, int bits, string? uniqueID = null) : base(position, ComponentType.TRI_STATE, uniqueID)
    {
        this._bits = bits;
        this.AddIO(bits, new IOConfig(ComponentSide.LEFT));
        this.AddIO(bits, new IOConfig(ComponentSide.RIGHT));

        this.AddIO(1, new IOConfig(ComponentSide.TOP));
    }

    public override void PerformLogic()
    {
        LogicValue[] input = this.GetIO(0).Values;

        bool enabled = this.GetIO(2).Values[0] == LogicValue.HIGH;

        if (enabled)
        {
            this.GetIO(1).PushValues(input);
        }
        else
        {
            this.GetIO(1).PushUnknown();
        }
    }

    public override ComponentDescription ToDescription()
    {
        return new DescriptionTriState(this.Position, this.Bits, this.Rotation, this.UniqueID);
    }
}