using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class DemultiplexerData : IComponentDescriptionData
{
    public int DataBits { get; set; }
    public int SelectBits { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new DemultiplexerData()
        {
            DataBits = 1,
            SelectBits = 1
        };
    }
}

[ScriptType("DEMULTIPLEXER"), ComponentInfo("Demultiplexer", "Plexers")]
public class Demultiplexer : Component<DemultiplexerData>
{
    public override string Name => "DEMUX";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private DemultiplexerData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(DemultiplexerData data)
    {
        this.ClearIOs();
        this._data = data;

        var outputs = Math.Pow(2, data.SelectBits);

        for (int i = 0; i < outputs; i++)
        {
            this.RegisterIO($"O{i}", data.DataBits, ComponentSide.RIGHT, "output");
        }

        for (int i = 0; i < this._data.SelectBits; i++)
        {
            this.RegisterIO($"S{i}", 1, ComponentSide.TOP, "select");
        }

        this.RegisterIO("input", data.DataBits, ComponentSide.LEFT, "input");

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
        var input = this.GetIOFromIdentifier("input").GetValues();

        var outputs = this.GetIOsWithTag("output");
        outputs[select].Push(input);
    }

    public override void SubmitUISelected(Editor editor, int componentIndex)
    {
        // Nothing yet.
        var id = this.GetUniqueIdentifier();
        var currSelectBits = this._data.SelectBits;
        if (ImGui.InputInt($"Select Bits##{id}", ref currSelectBits, 1, 1))
        {
            this._data.SelectBits = currSelectBits;
            this.Initialize(this._data);
        }
        var databits = this._data.DataBits;
        if (ImGui.InputInt($"Data Bits##{id}", ref databits, 1, 1))
        {
            this._data.DataBits = databits;
            this.Initialize(this._data);
        }
    }
}