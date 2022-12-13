using System.Drawing;
using System.Numerics;
using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class PushButtonData : INodeDescriptionData
{
    [NodeDescriptionProperty("Label", StringHint = "e.g. BTN_RESET", StringMaxLength = 16, StringRegexFilter = "^[a-zA-Z0-9_]*$")]
    public string Label { get; set; }

    [NodeDescriptionProperty("Hotkey")]
    public Keys Hotkey { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new PushButtonData()
        {
            Label = "",
            Hotkey = Keys.Unknown
        };
    }
}

[ScriptType("PUSHBUTTON"), NodeInfo("Button", "Input/Output", "core.markdown.pushbutton")]
public class PushButton : Node<PushButtonData>
{
    private PushButtonData _data;
    private int radius = 1;

    public PushButton()
    {

    }

    internal bool _hotkeyDown = false;
    public void SetKeyDown(object sender, Tuple<Keys, ModifierKeys> e)
    {
        if (e.Item1 == _data.Hotkey && _hotkeyDown == false)
        {
            _hotkeyDown = true;
            this.TriggerEvaluationNextTick();
        }
    }

    public void SetKeyUp(object sender, Tuple<Keys, ModifierKeys> e)
    {
        if (e.Item1 == _data.Hotkey && _hotkeyDown == true)
        {
            _hotkeyDown = false;
            this.TriggerEvaluationNextTick();
        }
    }

    public override void Register(Scheduler scheduler)
    {
        Input.OnKeyPressOrRepeat -= SetKeyDown;
        Input.OnKeyPressOrRepeat += SetKeyDown;

        Input.OnKeyRelease -= SetKeyUp;
        Input.OnKeyRelease += SetKeyUp;
    }

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var o = pins.Get("OUT");

        if (this._hotkeyDown)
        {
            yield return (o, LogicValue.HIGH.Multiple(1), 1);
        }
        else
        {
            yield return (o, LogicValue.LOW.Multiple(1), 1);
        }
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("OUT", 1, false, new Vector2i(this.GetSize().X, this.GetSize().Y / 2));
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(radius * 2, radius * 2);
    }

    public override void Initialize(PushButtonData data)
    {
        this._data = data;
    }

    public override bool IsNodeInRect(RectangleF rect)
    {
        return Utilities.CheckCircleRectangleCollision(this.Position.ToVector2(Constants.GRIDSIZE) + new Vector2(Constants.GRIDSIZE * this.radius), this.radius * Constants.GRIDSIZE, rect);
    }

    public override void RenderSelected(Camera2D camera)
    {
        var pos = this.Position.ToVector2(Constants.GRIDSIZE) + new Vector2(Constants.GRIDSIZE * this.radius);
        PrimitiveRenderer.RenderCircle(pos, Constants.GRIDSIZE * this.radius + 2, 0f, Constants.COLOR_SELECTED, 1f, 20);
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        var pos = this.Position.ToVector2(Constants.GRIDSIZE) + new Vector2(Constants.GRIDSIZE * this.radius);

        var interactable = false;
        if (Input.GetMousePosition(camera).DistanceTo(pos) <= (this.radius * Constants.GRIDSIZE) - 3)
        {
            if (Input.IsMouseButtonPressed(MouseButton.Right))
            {
                scheduler.Schedule(this, pins.Get("OUT"), LogicValue.HIGH.Multiple(1), 1);
            }

            interactable = true;
        }

        if (Input.IsMouseButtonReleased(MouseButton.Right))
            scheduler.Schedule(this, pins.Get("OUT"), LogicValue.LOW.Multiple(1), 1);

        return interactable;
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        yield return (pins.Get("OUT"), LogicValue.LOW.Multiple(1));
    }

    public override void Render(PinCollection pins, Camera2D camera)
    {
        var pos = this.Position.ToVector2(Constants.GRIDSIZE) + new Vector2(Constants.GRIDSIZE * this.radius);
        var radius = this.radius * Constants.GRIDSIZE;

        if (pins.TryGetValue("OUT", out var pin))
        {
            var color = pin.Item2 is null ? ColorF.Black : (pin.Item2.Read().First() == LogicValue.HIGH ? Constants.COLOR_HIGH : Constants.COLOR_LOW);
            PrimitiveRenderer.RenderCircle(pos, radius, 0f, ColorF.Black, 1f, 20);
            PrimitiveRenderer.RenderCircle(pos, radius - 1, 0f, ColorF.White, 1f, 20);
            PrimitiveRenderer.RenderCircle(pos, radius - 3, 0f, color, 1f, 20);
        }

        base.Render(pins, camera);
    }
}