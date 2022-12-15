using ImGuiNET;
using LogiX.Architecture.Commands;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class ConstantData : INodeDescriptionData
{
    [NodeDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 32)]
    public int DataBits { get; set; }

    // Will be rendered by this component.
    public uint Value { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new ConstantData()
        {
            DataBits = 1,
            Value = 1
        };
    }
}

[ScriptType("CONSTANT"), NodeInfo("Constant", "Common", "core.markdown.constant")]
public class Constant : BoxNode<ConstantData>
{
    private ConstantData _data;

    public override string Text => this._data.Value.ToString($"X{(int)Math.Ceiling(this._data.DataBits / 4f)}");
    public override float TextScale => 1f;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        yield break;
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        var size = this.GetSize();
        yield return new PinConfig("Y", this._data.DataBits, false, new Vector2i(size.X, 1));
    }

    public override Vector2i GetSize()
    {
        var font = Utilities.GetFont("core.font.default", 8);
        var text = this.Text;
        var scale = this.TextScale;

        var textWidth = (font.MeasureString(text, scale).X);

        return new Vector2i(Math.Max((int)(textWidth.CeilToMultipleOf(Constants.GRIDSIZE) / Constants.GRIDSIZE), 2), 2);
    }

    public override void Initialize(ConstantData data)
    {
        this._data = data;
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        return false; // No interaction.
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        yield return (pins.Get("Y"), this._data.Value.GetAsLogicValues(this._data.DataBits));
    }

    private bool IsInputValid(uint value, int bits)
    {
        return value <= (uint)Math.Pow(2, bits) - 1;
    }

    public override void SubmitUISelected(Editor editor, int componentIndex)
    {
        base.SubmitUISelected(editor, componentIndex);

        var id = this.ID.ToString();
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