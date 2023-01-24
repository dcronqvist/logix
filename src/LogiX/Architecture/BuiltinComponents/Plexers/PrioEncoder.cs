using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class PrioEncoderData : INodeDescriptionData
{
    [NodeDescriptionProperty("Output Bits", IntMinValue = 1, IntMaxValue = 8)]
    public int OutputBits { get; set; }

    [NodeDescriptionProperty("Select Pins Mode", HelpTooltip = "When set to 'Combined', the pin will be a single pin\nwith the select bits combined into a single value.\n\nWhen set to 'Separate', the pin will be a set of pins\nwith each pin representing a single select bit.")]
    public PinModeMulti SelectBitsMode { get; set; }

    [NodeDescriptionProperty("Output Pins Mode", HelpTooltip = "When set to 'Combined', the pin will be a single pin\nwith the output bits combined into a single value.\n\nWhen set to 'Separate', the pin will be a set of pins\nwith each pin representing a single output bit.")]
    public PinModeMulti OutputBitsMode { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new PrioEncoderData()
        {
            OutputBits = 4,
            SelectBitsMode = PinModeMulti.Separate,
            OutputBitsMode = PinModeMulti.Separate,
        };
    }
}

[ScriptType("PRIOENCODER"), NodeInfo("Priority Encoder", "Plexers", "core.markdown.decoder")]
public class PrioEncoder : BoxNode<PrioEncoderData>
{
    public override string Text => "PENC";
    public override float TextScale => 1f;

    private PrioEncoderData _data;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var selection = 0;

        if (this._data.SelectBitsMode == PinModeMulti.Separate)
        {
            var selectBits = new LogicValue[(int)Math.Pow(2, this._data.OutputBits)];
            for (int i = 0; i < (int)Math.Pow(2, this._data.OutputBits); i++)
            {
                var pin = pins.Get($"S{i}");
                selectBits[i] = pin.Read(1).First();
            }

            selectBits = selectBits.Reverse().ToArray();
            var greatestIndex = selectBits.GetGreatestIndex(LogicValue.HIGH);
            selection = greatestIndex;
        }
        else
        {
            var pin = pins.Get("S");
            var bits = pin.Read((int)Math.Pow(2, this._data.OutputBits)).Reverse();
            var greatestIndex = bits.GetGreatestIndex(LogicValue.HIGH);
            selection = greatestIndex;
        }

        selection = Math.Max(0, selection);

        if (this._data.OutputBitsMode == PinModeMulti.Separate)
        {
            var outputs = this._data.OutputBits;
            var outputValues = Utilities.GetAsLogicValues((uint)selection, this._data.OutputBits);
            for (int i = 0; i < outputs; i++)
            {
                var pin = pins.Get($"O{i}");
                yield return (pin, outputValues[i].Multiple(1), 1);
            }
        }
        else
        {
            var pin = pins.Get("O");
            var outputs = Utilities.GetAsLogicValues((uint)selection, this._data.OutputBits);
            yield return (pin, outputs, 1);
        }
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        if (this._data.OutputBitsMode == PinModeMulti.Separate)
        {
            for (int i = 0; i < this._data.OutputBits; i++)
            {
                yield return new PinConfig($"O{i}", 1, false, new Vector2i(3, i + 1));
            }
        }
        else
        {
            yield return new PinConfig("O", this._data.OutputBits, false, new Vector2i(3, 1));
        }

        if (this._data.SelectBitsMode == PinModeMulti.Separate)
        {
            for (int i = 0; i < (int)Math.Pow(2, this._data.OutputBits); i++)
            {
                yield return new PinConfig($"S{i}", 1, true, new Vector2i(0, i + 1));
            }
        }
        else
        {
            yield return new PinConfig("S", (int)Math.Pow(2, this._data.OutputBits), true, new Vector2i(0, 1));
        }
    }

    public override Vector2i GetSize()
    {
        var width = 3;
        var height = 2;

        if (this._data.SelectBitsMode == PinModeMulti.Separate)
        {
            height = Math.Max(height, (int)Math.Pow(2, this._data.OutputBits) + 1);
        }
        if (this._data.OutputBitsMode == PinModeMulti.Separate)
        {
            height = Math.Max(height, this._data.OutputBits + 1);
        }

        return new Vector2i(width, height);
    }

    public override void Initialize(PrioEncoderData data)
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