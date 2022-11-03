using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class AdderData : IComponentDescriptionData
{
    public int DataBits { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new AdderData()
        {
            DataBits = 1
        };
    }
}

[ScriptType("ADDER"), ComponentInfo("Adder", "Arithmetic")]
public class Adder : Component<AdderData>
{
    public override string Name => "ADD";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private AdderData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(AdderData data)
    {
        this.ClearIOs();
        this._data = data;

        this.RegisterIO("A", data.DataBits, ComponentSide.LEFT, "input");
        this.RegisterIO("B", data.DataBits, ComponentSide.LEFT, "input");
        this.RegisterIO("CIN", 1, ComponentSide.TOP, "carryin");
        this.RegisterIO("COUT", 1, ComponentSide.BOTTOM, "carryout");
        this.RegisterIO("S", data.DataBits, ComponentSide.RIGHT, "output");
    }

    public override void PerformLogic()
    {
        var a = this.GetIOFromIdentifier("A");
        var b = this.GetIOFromIdentifier("B");
        var cin = this.GetIOFromIdentifier("CIN");
        var cout = this.GetIOFromIdentifier("COUT");
        var s = this.GetIOFromIdentifier("S");

        var aValues = a.GetValues();
        var bValues = b.GetValues();
        var cinValues = cin.GetValues();

        if (aValues.AnyUndefined() || bValues.AnyUndefined() || cinValues.AnyUndefined())
        {
            return; // Can't do anything if we don't have all the values
        }

        var aAsuint = aValues.Reverse().GetAsUInt();
        var bAsuint = bValues.Reverse().GetAsUInt();
        var cinAsuint = cinValues.Reverse().GetAsUInt();

        var sum = aAsuint + bAsuint + cinAsuint;

        var sumAsBits = sum.GetAsLogicValues(this._data.DataBits);
        var coutAsBits = (sum >> this._data.DataBits).GetAsLogicValues(1);

        s.Push(sumAsBits);
        cout.Push(coutAsBits);
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
    }
}