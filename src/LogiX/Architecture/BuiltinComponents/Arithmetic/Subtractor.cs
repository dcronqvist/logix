using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class SubtractorData : IComponentDescriptionData
{
    public int DataBits { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new SubtractorData()
        {
            DataBits = 1
        };
    }
}

[ScriptType("SUBTRACTOR"), ComponentInfo("Subtractor", "Arithmetic")]
public class Subtractor : Component<SubtractorData>
{
    public override string Name => "SUB";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private SubtractorData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(SubtractorData data)
    {
        this.ClearIOs();
        this._data = data;

        this.RegisterIO("A", data.DataBits, ComponentSide.LEFT, "input");
        this.RegisterIO("B", data.DataBits, ComponentSide.LEFT, "input");
        this.RegisterIO("BIN", 1, ComponentSide.TOP, "borrowin");
        this.RegisterIO("BOUT", 1, ComponentSide.BOTTOM, "borrowout");
        this.RegisterIO("S", data.DataBits, ComponentSide.RIGHT, "output");
    }

    public override void PerformLogic()
    {
        var a = this.GetIOFromIdentifier("A");
        var b = this.GetIOFromIdentifier("B");
        var bin = this.GetIOFromIdentifier("BIN");
        var bout = this.GetIOFromIdentifier("BOUT");
        var s = this.GetIOFromIdentifier("S");

        var aValues = a.GetValues();
        var bValues = b.GetValues();
        var binValues = bin.GetValues();

        if (aValues.AnyUndefined() || bValues.AnyUndefined() || binValues.AnyUndefined())
        {
            return; // Can't do anything if we don't have all the values
        }

        var aAsuint = aValues.Reverse().GetAsUInt();
        var bAsuint = bValues.Reverse().GetAsUInt();
        var binAsuint = binValues.Reverse().GetAsUInt();

        var sum = aAsuint - bAsuint - binAsuint;

        var sumAsBits = sum.GetAsLogicValues(this._data.DataBits);
        var boutAsBits = (sum >> this._data.DataBits).GetAsLogicValues(1);

        s.Push(sumAsBits);
        bout.Push(boutAsBits);
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