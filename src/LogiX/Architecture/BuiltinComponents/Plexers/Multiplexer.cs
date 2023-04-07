using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class MultiplexerData : INodeDescriptionData
{
    [NodeDescriptionProperty("Data Bits", IntMinValue = 1, IntMaxValue = 256)]
    public int DataBits { get; set; }

    [NodeDescriptionProperty("Select Bits", IntMinValue = 1, IntMaxValue = 12)]
    public int SelectBits { get; set; }

    [NodeDescriptionProperty("Select Pins Mode", HelpTooltip = "When set to 'Combined', the pin will be a single pin\nwith the select bits combined into a single value.\n\nWhen set to 'Separate', the pin will be a set of pins\nwith each pin representing a single select bit.")]
    public PinModeMulti SelectBitsMode { get; set; }

    [NodeDescriptionProperty("Data Pins Mode", HelpTooltip = "When set to 'Combined', the data pins will be a single pin\nwith the data bits combined into a single value.\n\nWhen set to 'Separate', the pins will be a set of pins\nwith each pin representing a single data bit.")]
    public PinModeMulti DataBitsMode { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new MultiplexerData()
        {
            DataBits = 1,
            SelectBits = 1,
            SelectBitsMode = PinModeMulti.Separate,
            DataBitsMode = PinModeMulti.Combined,
        };
    }
}

[ScriptType("MULTIPLEXER"), NodeInfo("Multiplexer", "Plexers", "logix_core:docs/components/multiplexer.md")]
public class Multiplexer : BoxNode<MultiplexerData>
{
    public override string Text => "MUX";
    public override float TextScale => 1f;

    private MultiplexerData _data;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var selection = 0;

        if (this._data.SelectBitsMode == PinModeMulti.Combined)
        {
            var selectPin = pins.Get("S");
            var selectionBits = selectPin.Read(this._data.SelectBits);
            selection = selectionBits.Reverse().GetAsInt();
        }
        else
        {
            var selectionBits = new LogicValue[this._data.SelectBits];
            for (int i = 0; i < this._data.SelectBits; i++)
            {
                selectionBits[i] = pins.Get($"S{i}").Read(1).First();
            }
            selection = selectionBits.Reverse().GetAsInt();
        }

        if (this._data.DataBitsMode == PinModeMulti.Combined)
        {
            var inputPin = pins.Get($"I{selection}");
            var outputPin = pins.Get("O");

            yield return (outputPin, inputPin.Read(this._data.DataBits), 1);
        }
        else
        {
            for (int i = 0; i < this._data.DataBits; i++)
            {
                var inputPin = pins.Get($"I{selection}_{i}");
                var outputPin = pins.Get($"O{i}");

                yield return (outputPin, inputPin.Read(1), 1);
            }
        }
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        if (this._data.SelectBitsMode == PinModeMulti.Separate)
        {
            for (int i = 0; i < this._data.SelectBits; i++)
            {
                yield return new PinConfig($"S{i}", 1, true, new Vector2i(i + 1, 0));
            }
        }
        else
        {
            yield return new PinConfig("S", this._data.SelectBits, true, new Vector2i(1, 0));
        }

        if (this._data.DataBitsMode == PinModeMulti.Separate)
        {
            for (int i = 0; i < this._data.DataBits; i++)
            {
                for (int j = 0; j < Math.Pow(2, this._data.SelectBits); j++)
                {
                    yield return new PinConfig($"I{i}_{j}", 1, true, new Vector2i(0, i * (int)Math.Pow(2, this._data.SelectBits) + j + 1));
                }

                yield return new PinConfig($"O{i}", 1, false, new Vector2i(this.GetSize().X, i + 1));
            }
        }
        else
        {
            for (int i = 0; i < Math.Pow(2, this._data.SelectBits); i++)
            {
                yield return new PinConfig($"I{i}", this._data.DataBits, true, new Vector2i(0, i + 1));
            }

            yield return new PinConfig("O", this._data.DataBits, false, new Vector2i(this.GetSize().X, 1));
        }
    }

    public override Vector2i GetSize()
    {
        var width = 3;
        var height = 2;

        if (this._data.SelectBitsMode == PinModeMulti.Separate)
        {
            width = Math.Max(width, this._data.SelectBits + 1);
        }
        if (this._data.DataBitsMode == PinModeMulti.Combined)
        {
            height = Math.Max(height, (int)Math.Pow(2, this._data.SelectBits) + 1);
        }
        if (this._data.DataBitsMode == PinModeMulti.Separate)
        {
            height = Math.Max(height, ((int)Math.Pow(2, this._data.SelectBits) * this._data.DataBits) + 1);
        }

        return new Vector2i(width, height);
    }

    public override void Initialize(MultiplexerData data)
    {
        this._data = data;
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        return false;
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        yield break;
    }
}