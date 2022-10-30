using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class ComparatorData : IComponentDescriptionData
{
    public int DataBits { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new ComparatorData()
        {
            DataBits = 1
        };
    }
}

[ScriptType("COMPARATOR"), ComponentInfo("Comparator", "Arithmetic")]
public class Comparator : Component<ComparatorData>
{
    public override string Name => "COMP";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private ComparatorData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(ComparatorData data)
    {
        this.ClearIOs();
        this._data = data;

        this.RegisterIO("A", data.DataBits, ComponentSide.LEFT, "divisor");
        this.RegisterIO("B", data.DataBits, ComponentSide.LEFT, "dividend");

        this.RegisterIO("A>B", 1, ComponentSide.RIGHT);
        this.RegisterIO("A=B", 1, ComponentSide.RIGHT);
        this.RegisterIO("A<B", 1, ComponentSide.RIGHT);
    }

    public override void PerformLogic()
    {
        var a = this.GetIOFromIdentifier("A");
        var b = this.GetIOFromIdentifier("B");
        var agb = this.GetIOFromIdentifier("A>B");
        var aeb = this.GetIOFromIdentifier("A=B");
        var alb = this.GetIOFromIdentifier("A<B");

        var aValues = a.GetValues();
        var bValues = b.GetValues();

        if (aValues.AnyUndefined() || bValues.AnyUndefined())
        {
            return; // Can't do anything if we don't have all the values
        }

        var aAsuint = aValues.Reverse().GetAsUInt();
        var bAsuint = bValues.Reverse().GetAsUInt();

        if (aAsuint > bAsuint)
        {
            agb.Push(LogicValue.HIGH);
            aeb.Push(LogicValue.LOW);
            alb.Push(LogicValue.LOW);
        }
        else if (aAsuint == bAsuint)
        {
            agb.Push(LogicValue.LOW);
            aeb.Push(LogicValue.HIGH);
            alb.Push(LogicValue.LOW);
        }
        else
        {
            agb.Push(LogicValue.LOW);
            aeb.Push(LogicValue.LOW);
            alb.Push(LogicValue.HIGH);
        }
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