using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public enum PinModeMulti
{
    Combined,
    Separate
}

public class DecoderData : INodeDescriptionData
{
    [NodeDescriptionProperty("Select Bits", IntMinValue = 1, IntMaxValue = 8)]
    public int SelectBits { get; set; }

    [NodeDescriptionProperty("Select Pins Mode", HelpTooltip = "When set to 'Combined', the pin will be a single pin\nwith the select bits combined into a single value.\n\nWhen set to 'Separate', the pin will be a set of pins\nwith each pin representing a single select bit.")]
    public PinModeMulti SelectBitsMode { get; set; }

    [NodeDescriptionProperty("Output Pins Mode", HelpTooltip = "When set to 'Combined', the pin will be a single pin\nwith the output bits combined into a single value.\n\nWhen set to 'Separate', the pin will be a set of pins\nwith each pin representing a single output bit.")]
    public PinModeMulti OutputBitsMode { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new DecoderData()
        {
            SelectBits = 4,
            SelectBitsMode = PinModeMulti.Separate,
            OutputBitsMode = PinModeMulti.Separate,
        };
    }
}

[ScriptType("DECODER"), NodeInfo("Decoder", "Plexers", "logix_core:docs/components/decoder.md")]
public class Decoder : BoxNode<DecoderData>
{
    public override string Text => "DEC";
    public override float TextScale => 1f;

    private DecoderData _data;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var selection = 0;

        if (this._data.SelectBitsMode == PinModeMulti.Separate)
        {
            var selectBits = new LogicValue[this._data.SelectBits];
            for (int i = 0; i < this._data.SelectBits; i++)
            {
                var pin = pins.Get($"S{i}");
                selectBits[i] = pin.Read(1).First();
            }

            selection = (int)selectBits.Reverse().GetAsUInt();
        }
        else
        {
            var pin = pins.Get("S");
            selection = (int)pin.Read(this._data.SelectBits).Reverse().GetAsUInt();
        }

        if (this._data.OutputBitsMode == PinModeMulti.Separate)
        {
            var outputs = Math.Pow(2, this._data.SelectBits);
            for (int i = 0; i < outputs; i++)
            {
                var pin = pins.Get($"O{i}");
                yield return (pin, new LogicValue[] { i == selection ? LogicValue.HIGH : LogicValue.LOW }, 1);
            }
        }
        else
        {
            var pin = pins.Get("O");
            var outputs = LogicValue.LOW.Multiple((int)Math.Pow(2, this._data.SelectBits));
            outputs[selection] = LogicValue.HIGH;
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
            var outputs = Math.Pow(2, this._data.SelectBits);
            for (int i = 0; i < outputs; i++)
            {
                yield return new PinConfig($"O{i}", 1, false, new Vector2i(3, i + 1));
            }
        }
        else
        {
            yield return new PinConfig("O", (int)Math.Pow(2, this._data.SelectBits), false, new Vector2i(3, 1));
        }

        if (this._data.SelectBitsMode == PinModeMulti.Separate)
        {
            for (int i = 0; i < this._data.SelectBits; i++)
            {
                yield return new PinConfig($"S{i}", 1, true, new Vector2i(0, i + 1));
            }
        }
        else
        {
            yield return new PinConfig("S", this._data.SelectBits, true, new Vector2i(0, 1));
        }
    }

    public override Vector2i GetSize()
    {
        var width = 3;
        var height = 2;

        if (this._data.SelectBitsMode == PinModeMulti.Separate)
        {
            height = Math.Max(height, this._data.SelectBits + 1);
        }
        if (this._data.OutputBitsMode == PinModeMulti.Separate)
        {
            height = Math.Max(height, (int)Math.Pow(2, this._data.SelectBits) + 1);
        }

        return new Vector2i(width, height);
    }

    public override void Initialize(DecoderData data)
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

    // public override IComponentDescriptionData GetDescriptionData()
    // {
    //     return _data;
    // }

    // public override void Initialize(DecoderData data)
    // {
    //     this.ClearIOs();
    //     this._data = data;

    //     var outputs = Math.Pow(2, data.SelectBits);

    //     for (int i = 0; i < outputs; i++)
    //     {
    //         this.RegisterIO($"O{i}", 1, ComponentSide.RIGHT, "output");
    //     }

    //     for (int i = 0; i < this._data.SelectBits; i++)
    //     {
    //         this.RegisterIO($"S{i}", 1, ComponentSide.TOP, "select");
    //     }

    //     this.TriggerSizeRecalculation();
    // }

    // public override void PerformLogic()
    // {
    //     var selects = this.GetIOsWithTag("select");

    //     if (selects.Select(v => v.GetValues().First()).Any(v => v == LogicValue.UNDEFINED))
    //     {
    //         return;
    //     }

    //     var select = selects.Select(v => v.GetValues().First()).GetAsInt();

    //     var outputs = this.GetIOsWithTag("output");
    //     for (int i = 0; i < outputs.Length; i++)
    //     {
    //         var val = i == select ? LogicValue.HIGH : LogicValue.LOW;
    //         outputs[i].Push(val);
    //     }
    // }
}