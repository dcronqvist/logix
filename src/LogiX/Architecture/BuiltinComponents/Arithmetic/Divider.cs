using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class DividerData : IComponentDescriptionData
{
    public int DataBits { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new DividerData()
        {
            DataBits = 1
        };
    }
}

[ScriptType("DIVIDER"), ComponentInfo("Divider", "Arithmetic")]
public class Divider : Component<DividerData>
{
    public override string Name => "DIV";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private DividerData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(DividerData data)
    {
        this.ClearIOs();
        this._data = data;

        this.RegisterIO("A", data.DataBits, ComponentSide.LEFT, "divisor");
        this.RegisterIO("B", data.DataBits, ComponentSide.LEFT, "dividend");
        this.RegisterIO("DUP", data.DataBits, ComponentSide.TOP, "divisorUpper");
        this.RegisterIO("REM", data.DataBits, ComponentSide.BOTTOM, "remainder");
        this.RegisterIO("S", data.DataBits, ComponentSide.RIGHT, "output");
        this.RegisterIO("Z", 1, ComponentSide.RIGHT, "zero");
    }

    public override void PerformLogic()
    {
        var a = this.GetIOFromIdentifier("A");
        var b = this.GetIOFromIdentifier("B");
        var dup = this.GetIOFromIdentifier("DUP");
        var rem = this.GetIOFromIdentifier("REM");
        var s = this.GetIOFromIdentifier("S");
        var z = this.GetIOFromIdentifier("Z");

        var aValues = a.GetValues();
        var bValues = b.GetValues();
        var dupValues = dup.GetValues();

        if (aValues.AnyUndefined() || bValues.AnyUndefined() || dupValues.AnyUndefined())
        {
            return; // Can't do anything if we don't have all the values
        }

        var aAsuint = aValues.Reverse().GetAsUInt() + (dupValues.Reverse().GetAsUInt() << this._data.DataBits);
        var bAsuint = bValues.Reverse().GetAsUInt();

        if (bAsuint == 0)
        {
            z.Push(LogicValue.HIGH);
            return;
        }

        var sum = aAsuint / bAsuint;

        var sumAsBits = sum.GetAsLogicValues(this._data.DataBits);
        var remainder = aAsuint - (sum * bAsuint);

        s.Push(sumAsBits);
        rem.Push(remainder.GetAsLogicValues(this._data.DataBits));
        z.Push(LogicValue.LOW);
    }

    public override void SubmitUISelected(int componentIndex)
    {
        var id = this.GetUniqueIdentifier();
        var databits = this._data.DataBits;
        if (ImGui.InputInt($"Data Bits##{id}", ref databits, 1, 1))
        {
            this._data.DataBits = databits;
            this.Initialize(this._data);
        }
    }
}