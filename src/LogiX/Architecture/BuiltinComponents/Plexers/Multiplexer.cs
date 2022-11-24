using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;

namespace LogiX.Architecture.BuiltinComponents;

public class MultiplexerData : IComponentDescriptionData
{
    [ComponentDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 32)]
    public int DataBits { get; set; }

    [ComponentDescriptionProperty("Select Bits", IntMinValue = 1, IntMaxValue = 32)]
    public int SelectBits { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new MultiplexerData()
        {
            DataBits = 1,
            SelectBits = 1
        };
    }
}

[ScriptType("MULTIPLEXER"), ComponentInfo("Multiplexer", "Plexers", "core.markdown.multiplexer")]
public class Multiplexer : Component<MultiplexerData>
{
    public override string Name => "MUX";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private MultiplexerData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(MultiplexerData data)
    {
        this.ClearIOs();
        this._data = data;

        var inputs = Math.Pow(2, data.SelectBits);

        for (int i = 0; i < inputs; i++)
        {
            this.RegisterIO($"I{i}", data.DataBits, ComponentSide.LEFT, "input");
        }

        for (int i = 0; i < this._data.SelectBits; i++)
        {
            this.RegisterIO($"S{i}", 1, ComponentSide.TOP, "select");
        }

        this.RegisterIO("O", data.DataBits, ComponentSide.RIGHT, "output");

        this.TriggerSizeRecalculation();
    }

    public override void PerformLogic()
    {
        var selects = this.GetIOsWithTag("select");
        var values = selects.Select(s => s.GetValues().First());

        if (values.Any(v => v == LogicValue.UNDEFINED))
        {
            return;
        }

        var select = values.GetAsInt();

        var inputs = this.GetIOsWithTag("input");
        var input = inputs[select].GetValues();

        this.GetIOFromIdentifier("O").Push(input);
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