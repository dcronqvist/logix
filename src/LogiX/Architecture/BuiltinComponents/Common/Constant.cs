using ImGuiNET;
using LogiX.Architecture.Commands;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class ConstantData : IComponentDescriptionData
{
    [ComponentDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 32)]
    public int DataBits { get; set; }

    // Will be rendered by this component.
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

[ScriptType("CONSTANT"), ComponentInfo("Constant", "Common", "core.markdown.constant")]
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
        this.TriggerSizeRecalculation();
    }

    public override void PerformLogic()
    {
        var asBits = this._data.Value.GetAsLogicValues(this._data.DataBits);
        var y = this.GetIOFromIdentifier("Y");

        y.Push(asBits);
    }

    private bool IsInputValid(uint value, int bits)
    {
        return value <= (uint)Math.Pow(2, bits) - 1;
    }

    public override void SubmitUISelected(Editor editor, int componentIndex)
    {
        base.SubmitUISelected(editor, componentIndex);

        var id = this.GetUniqueIdentifier();
        var symbols = (int)Math.Ceiling(this._data.DataBits / 4f);
        var currVal = (int)this._data.Value;
        if (ImGui.InputInt($"Value##{id}", ref currVal, 1, 1, ImGuiInputTextFlags.CharsHexadecimal | ImGuiInputTextFlags.CharsUppercase))
        {
            if (IsInputValid((uint)currVal, this._data.DataBits))
            {
                editor.Execute(new CModifyComponentDataProp(this.ID, this._data.GetType().GetProperty(nameof(this._data.Value)), (uint)currVal), editor);
            }
        }
    }
}