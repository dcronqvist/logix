using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class NegatorData : IComponentDescriptionData
{
    public int DataBits { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new NegatorData()
        {
            DataBits = 1
        };
    }
}

[ScriptType("NEGATOR"), ComponentInfo("Negator", "Arithmetic")]
public class Negator : Component<NegatorData>
{
    public override string Name => "NEG";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private NegatorData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(NegatorData data)
    {
        this.ClearIOs();
        this._data = data;

        this.RegisterIO("X", data.DataBits, ComponentSide.LEFT);
        this.RegisterIO("Y", data.DataBits, ComponentSide.RIGHT);
    }

    public override void PerformLogic()
    {
        var x = this.GetIOFromIdentifier("X");
        var y = this.GetIOFromIdentifier("Y");

        var xVal = x.GetValues();

        if (xVal.AnyUndefined())
        {
            return; // Can't do anything if we don't have all the values
        }

        var xAsUint = xVal.Reverse().GetAsUInt();

        // Get two's complement of x
        var yAsUint = ~xAsUint + 1;

        // 1000 -> 0111 + 1 = 1000

        // Convert back to bits
        var yVal = yAsUint.GetAsLogicValues(this._data.DataBits);

        y.Push(yVal);
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