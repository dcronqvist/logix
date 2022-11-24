using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public enum ShiftDirection
{
    LEFT = 0,
    RIGHT = 1
}

public class ShifterData : IComponentDescriptionData
{
    [ComponentDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 32)]
    public int DataBits { get; set; }

    [ComponentDescriptionProperty("Direction")]
    public ShiftDirection Direction { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new ShifterData()
        {
            DataBits = 4,
            Direction = ShiftDirection.LEFT
        };
    }
}

[ScriptType("SHIFTER"), ComponentInfo("Shifter", "Arithmetic", "core.markdown.shifter")]
public class Shifter : Component<ShifterData>
{
    public override string Name => "SHIFT";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private ShifterData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(ShifterData data)
    {
        this.ClearIOs();
        this._data = data;

        this.RegisterIO("X", data.DataBits, ComponentSide.LEFT, "input");
        this.RegisterIO("Y", data.DataBits, ComponentSide.RIGHT, "output");
        this.RegisterIO("S", (int)Math.Ceiling(Math.Log2(data.DataBits)), ComponentSide.LEFT, "shift");
        this.RegisterIO("IN", 1, ComponentSide.LEFT, "input");
    }

    public override void PerformLogic()
    {
        var x = this.GetIOFromIdentifier("X");
        var y = this.GetIOFromIdentifier("Y");
        var s = this.GetIOFromIdentifier("S");
        var shiftIn = this.GetIOFromIdentifier("IN");

        var xBits = x.GetValues();
        var sBits = s.GetValues();
        var shiftInBits = shiftIn.GetValues().First();

        if (xBits.AnyUndefined() || sBits.AnyUndefined() || shiftInBits.IsUndefined())
        {
            return; // Can't do anything if we don't have all the values
        }

        var xint = xBits.Reverse().GetAsUInt();
        var shift = sBits.Reverse().GetAsUInt();

        if (this._data.Direction == ShiftDirection.LEFT)
        {
            var yint = xint << (int)shift;

            if (shiftInBits == LogicValue.HIGH)
            {
                yint |= 1;
            }

            var yBits = yint.GetAsLogicValues(y.Bits);

            y.Push(yBits);
        }
        else
        {
            var yint = xint >> (int)shift;

            if (shiftInBits == LogicValue.HIGH)
            {
                var highestValue = (uint)Math.Pow(2, y.Bits - 1);
                yint |= highestValue;
            }

            var yBits = yint.GetAsLogicValues(y.Bits);

            y.Push(yBits);
        }
    }
}