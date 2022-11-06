using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class DecoderData : IComponentDescriptionData
{
    public int SelectBits { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new DecoderData()
        {
            SelectBits = 4
        };
    }
}

[ScriptType("DECODER"), ComponentInfo("Decoder", "Plexers")]
public class Decoder : Component<DecoderData>
{
    public override string Name => "DEC";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private DecoderData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(DecoderData data)
    {
        this.ClearIOs();
        this._data = data;

        var outputs = Math.Pow(2, data.SelectBits);

        for (int i = 0; i < outputs; i++)
        {
            this.RegisterIO($"O{i}", 1, ComponentSide.RIGHT, "output");
        }

        for (int i = 0; i < this._data.SelectBits; i++)
        {
            this.RegisterIO($"S{i}", 1, ComponentSide.TOP, "select");
        }

        this.TriggerSizeRecalculation();
    }

    public override void PerformLogic()
    {
        var selects = this.GetIOsWithTag("select");

        if (selects.Select(v => v.GetValues().First()).Any(v => v == LogicValue.UNDEFINED))
        {
            return;
        }

        var select = selects.Select(v => v.GetValues().First()).GetAsInt();

        var outputs = this.GetIOsWithTag("output");
        for (int i = 0; i < outputs.Length; i++)
        {
            var val = i == select ? LogicValue.HIGH : LogicValue.LOW;
            outputs[i].Push(val);
        }
    }

    public override void SubmitUISelected(Editor editor, int componentIndex)
    {
        // Nothing yet.
        var id = this.GetUniqueIdentifier();
        var currSelectBits = this._data.SelectBits;
        if (ImGui.InputInt($"Select Bits##{id}", ref currSelectBits, 1, 1))
        {
            this._data.SelectBits = Math.Clamp(currSelectBits, 1, 8);
            this.Initialize(this._data);
        }
    }
}