using System.Drawing;
using System.Numerics;
using ImGuiNET;
using LogiX.Architecture;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Graphics.UI;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public enum PinBehaviour
{
    INPUT = 0,
    OUTPUT = 1
}

public class PinData : INodeDescriptionData
{
    [NodeDescriptionProperty("Bits", IntMaxValue = 32, IntMinValue = 1)]
    public int Bits { get; set; }
    public LogicValue[] Values { get; set; }

    [NodeDescriptionProperty("Label", StringMaxLength = 16, StringHint = "PIN_ID", StringRegexFilter = "^[a-zA-Z0-9_]*$")]
    public string Label { get; set; }

    [NodeDescriptionProperty("Behaviour", HelpTooltip = "Only affects the behaviour of a pin in the current circuit.\nDoes not affect the behaviour of the pin when used in another circuit.")]
    public PinBehaviour Behaviour { get; set; }

    [NodeDescriptionProperty("Side of component", HelpTooltip = "Only applies if \"Visible on component\" is enabled")]
    public ComponentSide Side { get; set; }

    [NodeDescriptionProperty("Visible on component")]
    public bool IsExternal { get; set; }

    public static INodeDescriptionData GetDefault()
    {
        return new PinData()
        {
            Bits = 1,
            Values = new LogicValue[] { LogicValue.Z },
            Label = "",
            Behaviour = PinBehaviour.INPUT,
            Side = ComponentSide.LEFT,
            IsExternal = true,
        };
    }
}

[ScriptType("PIN"), NodeInfo("Pin", "Common", "core.markdown.pin")]
public class Pin : Node<PinData>
{
    private PinData _data;
    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override void Initialize(PinData data)
    {
        this._data = data;

        if (data.Bits == data.Values.Length)
        {
            if (data.Behaviour == PinBehaviour.INPUT)
            {
                for (int i = 0; i < data.Values.Length; i++)
                {
                    if (data.Values[i] == LogicValue.Z)
                    {
                        data.Values[i] = LogicValue.LOW;
                    }
                }
            }
            else
            {
                data.Values = LogicValue.Z.Multiple(data.Bits);
            }

            return;
        }
        else
        {
            this._data.Values = data.Behaviour == PinBehaviour.INPUT ? LogicValue.LOW.Multiple(this._data.Bits) : LogicValue.Z.Multiple(this._data.Bits);
        }
    }

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var q = pins.Get("Q");
        var vals = q.Read();

        if (vals.Length != this._data.Bits)
        {
            q.Error = ObservableValueError.PIN_WIDTHS_MISMATCH;
        }
        else
        {
            this._data.Values = vals;
        }

        return Enumerable.Empty<(ObservableValue, LogicValue[], int)>();
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        bool precedence = false;

        if (this._data.Behaviour == PinBehaviour.INPUT)
        {
            var pos = this.Position.ToVector2(Constants.GRIDSIZE);
            var width = this._data.Bits * 2;

            for (int i = 0; i < this._data.Bits; i++)
            {
                var value = this._data.Values[i];
                var x = i * 2 * Constants.GRIDSIZE;

                var r = new RectangleF(pos.X + x, pos.Y, Constants.GRIDSIZE * 2, Constants.GRIDSIZE * 2).Inflate(-2);

                if (r.Contains(Input.GetMousePosition(camera)))
                {
                    if (Input.IsMouseButtonPressed(MouseButton.Right))
                    {
                        this._data.Values[i] = value == LogicValue.LOW ? LogicValue.HIGH : LogicValue.LOW;
                        scheduler.Schedule(this, pins.Get("Q"), this._data.Values, 1);
                    }

                    precedence = true;
                }
            }
        }

        return precedence;
    }

    public override bool IsNodeInRect(RectangleF rect)
    {
        var pos = this.Position;
        var width = this._data.Bits * 2;
        var height = 2;

        var r = pos.ToVector2(Constants.GRIDSIZE).CreateRect(new Vector2(width, height) * Constants.GRIDSIZE);

        return rect.IntersectsWith(r);
    }

    public override void Render(PinCollection pins, Camera2D camera)
    {
        var pos = this.Position;
        var width = this._data.Bits * 2;
        var height = 2;

        var rect = pos.ToVector2(Constants.GRIDSIZE).CreateRect(new Vector2(width, height) * Constants.GRIDSIZE);

        PrimitiveRenderer.RenderRectangleWithBorder(rect, Vector2.Zero, 0f, 1, ColorF.White, ColorF.Black);

        for (int i = 0; i < Math.Min(this._data.Bits, this._data.Values.Length); i++)
        {
            var value = this._data.Values[i];
            var x = i * 2 * Constants.GRIDSIZE;

            var r = new RectangleF(rect.X + x, rect.Y, Constants.GRIDSIZE * 2, Constants.GRIDSIZE * 2).Inflate(-3);

            if (this._data.Behaviour == PinBehaviour.INPUT)
            {
                PrimitiveRenderer.RenderRectangle(r, Vector2.Zero, 0f, Utilities.GetValueColor(value));
            }
            else
            {
                PrimitiveRenderer.RenderCircle(r.GetMiddleOfRectangle(), Constants.GRIDSIZE - 3, 0f, Utilities.GetValueColor(value), 1f);
            }
        }

        base.Render(pins, camera);
    }

    public override void RenderSelected(Camera2D camera)
    {
        var pos = this.Position;
        var width = this._data.Bits * 2;
        var height = 2;

        var rect = pos.ToVector2(Constants.GRIDSIZE).CreateRect(new Vector2(width, height) * Constants.GRIDSIZE);

        PrimitiveRenderer.RenderRectangle(rect.Inflate(2), Vector2.Zero, 0f, Constants.COLOR_SELECTED);
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("Q", this._data.Bits, this._data.Behaviour == PinBehaviour.INPUT ? false : true, new Vector2i(0, 1));
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        if (this._data.Behaviour == PinBehaviour.INPUT)
        {
            yield return (pins.Get("Q"), this._data.Values);
        }
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(this._data.Bits * 2, 2);
    }
}