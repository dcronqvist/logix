using LogiX.SaveSystem;

namespace LogiX.Components;

public interface IBufferLogic
{
    string GetLogicText();
    LogicValue[] GetOutput(LogicValue[] input);
}

public class BufferComponent : Component
{
    public IBufferLogic Logic { get; set; }

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
    public override string Text => this.Logic.GetLogicText();

    public BufferComponent(Vector2 position, int bits, IBufferLogic logic, string? uniqueID = null) : base(position, ComponentType.BUFFER, uniqueID)
    {
        this.Logic = logic;

        this.AddIO(bits, new IOConfig(ComponentSide.LEFT));
        this.AddIO(bits, new IOConfig(ComponentSide.RIGHT));

        this.Bits = bits;
    }

    public override void PerformLogic()
    {
        LogicValue[] input = this.GetIO(0).Values;

        if (this.GetIO(0).HasUnknown())
        {
            this.GetIO(1).PushUnknown();
            return;
        }

        LogicValue[] output = this.Logic.GetOutput(input);
        this.GetIO(1).PushValues(output);
    }

    public override ComponentDescription ToDescription()
    {
        return new DescriptionBuffer(this.Position, this.Bits, this.Logic, this.Rotation, this.UniqueID);
    }
}

public class BufferLogic : IBufferLogic
{
    public string GetLogicText()
    {
        return "BUF";
    }

    public LogicValue[] GetOutput(LogicValue[] input)
    {
        return input;
    }
}

public class InverterLogic : IBufferLogic
{
    public string GetLogicText()
    {
        return "NOT";
    }

    public LogicValue[] GetOutput(LogicValue[] input)
    {
        return input.Select(x => x == LogicValue.HIGH ? LogicValue.LOW : LogicValue.HIGH).ToArray();
    }
}

