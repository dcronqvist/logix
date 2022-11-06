using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public enum ShiftDirection
{
    Left = 0,
    Right = 1
}

public class ShifterData : IComponentDescriptionData
{
    public int DataBits { get; set; }
    public ShiftDirection Direction { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new ShifterData()
        {
            DataBits = 4,
            Direction = ShiftDirection.Left
        };
    }
}

[ScriptType("SHIFTER"), ComponentInfo("Shifter", "Arithmetic")]
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

        // 0000, 4 bits input, how many bits required to shift 4 bits? 2 bits, calculated by log2(4) = 2
        // 00000, 5 bits input, how many bits required to shift 5 bits? 3 bits, calculated by log2(5) = 2.3219280948873623478703194294894

        this.RegisterIO("X", data.DataBits, ComponentSide.LEFT, "input");
        this.RegisterIO("Y", data.DataBits, ComponentSide.RIGHT, "output");
        this.RegisterIO("S", (int)Math.Ceiling(Math.Log2(data.DataBits)), ComponentSide.LEFT, "shift");
    }

    public override void PerformLogic()
    {
        var x = this.GetIOFromIdentifier("X");
        var y = this.GetIOFromIdentifier("Y");
        var s = this.GetIOFromIdentifier("S");

        var xBits = x.GetValues();
        var sBits = s.GetValues();

        if (xBits.AnyUndefined() || sBits.AnyUndefined())
        {
            return; // Can't do anything if we don't have all the values
        }

        var xint = xBits.Reverse().GetAsUInt();
        var shift = sBits.Reverse().GetAsUInt();

        if (this._data.Direction == ShiftDirection.Left)
        {
            var yint = xint << (int)shift;

            var yBits = yint.GetAsLogicValues(y.Bits);

            y.Push(yBits);
        }
        else
        {
            var yint = xint >> (int)shift;

            var yBits = yint.GetAsLogicValues(y.Bits);

            y.Push(yBits);
        }
    }

    public override void SubmitUISelected(Editor editor, int componentIndex)
    {
        var id = this.GetUniqueIdentifier();
        var databits = this._data.DataBits;
        if (ImGui.InputInt($"Data Bits##{id}", ref databits, 1, 1))
        {
            this._data.DataBits = databits;
            this.Initialize(this._data);
        }
        var currIndex = (int)this._data.Direction;
        ImGui.Combo($"Shift Direction##{id}", ref currIndex, new string[] { "Left", "Right" }, 2);
        this._data.Direction = (ShiftDirection)currIndex;
    }
}