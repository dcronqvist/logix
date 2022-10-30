using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class ConstantData : IComponentDescriptionData
{
    public int DataBits { get; set; }
    public uint Value { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new ConstantData()
        {
            DataBits = 1,
            Value = 1
        };
    }
}

[ScriptType("CONSTANT"), ComponentInfo("Constant", "Wiring")]
public class Constant : Component<ConstantData>
{
    public override string Name => this._data.Value.ToString($"X{(int)Math.Ceiling(this._data.DataBits / 4f)}");
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private ConstantData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(ConstantData data)
    {
        this.ClearIOs();
        this._data = data;

        this.RegisterIO("Y", data.DataBits, ComponentSide.RIGHT);
    }

    public override void PerformLogic()
    {
        var asBits = this._data.Value.GetAsLogicValues(this._data.DataBits);
        var y = this.GetIOFromIdentifier("Y");

        y.Push(asBits);
    }

    public override void SubmitUISelected(int componentIndex)
    {
        var id = this.GetUniqueIdentifier();
        var symbols = (int)Math.Ceiling(this._data.DataBits / 4f);
        var value = this._data.Value.ToString($"X{symbols}");
        if (ImGui.InputText($"Value##{id}", ref value, (uint)symbols, ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.CharsUppercase))
        {
            this._data.Value = Convert.ToUInt32(value, 16);
            this.Initialize(this._data);
        }
        var currBits = this._data.DataBits;
        if (ImGui.InputInt($"Data Bits##{id}", ref currBits))
        {
            this._data.DataBits = currBits;
            this.Initialize(this._data);
        }
    }
}