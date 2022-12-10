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

// [ScriptType("PUSHBUTTON"), NodeInfo("Button", "Input/Output", "core.markdown.pushbutton")]
// public class PushButton : Component<PushButtonData>
// {
//     public override string Name => "";
//     public override bool DisplayIOGroupIdentifiers => true;
//     public override bool ShowPropertyWindow => true;

//     private PushButtonData _data;

//     public PushButton()
//     {
//         Input.OnKeyPressOrRepeat += (sender, e) =>
//         {
//             if (e.Item1 == _data.Hotkey && _hotkeyDown == false)
//             {
//                 this.SetKeyDown();
//             }
//         };

//         Input.OnKeyRelease += (sender, e) =>
//         {
//             if (e.Item1 == _data.Hotkey && _hotkeyDown == true)
//             {
//                 this.SetKeyUp();
//             }
//         };
//     }

//     public override INodeDescriptionData GetDescriptionData()
//     {
//         return _data;
//     }

//     internal bool _hotkeyDown = false;
//     public void SetKeyDown()
//     {
//         _hotkeyDown = true;
//     }

//     public void SetKeyUp()
//     {
//         _hotkeyDown = false;
//     }

//     public override void Initialize(PushButtonData data)
//     {
//         this.ClearIOs();
//         this._data = data;

//         if (this._data.Label is null)
//         {
//             this._data.Label = "";
//         }

//         this.RegisterIO("Y", 1, ComponentSide.RIGHT);
//         this.TriggerSizeRecalculation();
//     }

//     internal LogicValue _value;
//     public override void PerformLogic()
//     {
//         var y = this.GetIOFromIdentifier("Y");

//         if (_hotkeyDown || this._mouseClicking)
//         {
//             _value = LogicValue.HIGH;
//         }
//         else
//         {
//             _value = LogicValue.LOW;
//         }

//         y.Push(_value);
//     }

//     private bool _mouseClicking = false;
//     public override void Interact(Camera2D cam)
//     {
//         var bounds = this.GetBoundingBox(out var _);
//         var mousePos = Input.GetMousePosition(cam);

//         _mouseClicking = false;
//         if (Input.IsMouseButtonDown(MouseButton.Right))
//         {
//             if (bounds.Contains(mousePos))
//             {
//                 _mouseClicking = true;
//             }
//         }
//     }

//     public override void Render(Camera2D camera)
//     {
//         var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");

//         var pos = this.Position.ToVector2(Constants.GRIDSIZE);
//         var rect = this.GetBoundingBox(out var textSize);
//         var size = rect.GetSize().ToVector2i(Constants.GRIDSIZE);
//         var realSize = size.ToVector2(Constants.GRIDSIZE);

//         var ios = this.IOs;
//         for (int i = 0; i < ios.Length; i++)
//         {
//             var io = ios[i];
//             var ioPos = this.GetPositionForIO(io, out var lineEnd);
//             var lineEndPos = new Vector2(lineEnd.X * Constants.GRIDSIZE, lineEnd.Y * Constants.GRIDSIZE);

//             // Draw the group
//             var gPos = new Vector2(ioPos.X * Constants.GRIDSIZE, ioPos.Y * Constants.GRIDSIZE);
//             int lineThickness = 2;
//             var groupCol = this.GetIOColor(i);

//             PrimitiveRenderer.RenderLine(gPos, lineEndPos, lineThickness, groupCol.Darken(0.5f));
//             PrimitiveRenderer.RenderCircle(gPos, Constants.IO_GROUP_RADIUS, 0f, groupCol);
//         }

//         var isHovered = rect.Contains(Input.GetMousePosition(camera));
//         var isClicked = _mouseClicking || _hotkeyDown;

//         var color = isClicked ? ColorF.White : (isHovered ? ColorF.LightGray : ColorF.Gray);

//         PrimitiveRenderer.RenderCircle(pos + realSize / 2f, 10f, 0f, ColorF.White, sides: 20);
//         PrimitiveRenderer.RenderCircle(pos + realSize / 2f, 8.5f, 0f, color.Darken(0.5f), sides: 20);
//         PrimitiveRenderer.RenderCircle(pos + realSize / 2f, 8 - (this._value == LogicValue.HIGH ? 1 : 0f), 0f, color, sides: 20);
//     }

//     public override void RenderSelected(Camera2D camera)
//     {
//         // Position of component
//         var font = Utilities.GetFont("core.font.default", 8); //LogiX.ContentManager.GetContentItem<Font>("core.font.default-regular-8");
//         var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");
//         var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.text");

//         var pos = this.Position.ToVector2(Constants.GRIDSIZE);
//         var rect = this.GetBoundingBox(out _);
//         var size = rect.GetSize().ToVector2i(Constants.GRIDSIZE);
//         var realSize = size.ToVector2(Constants.GRIDSIZE);

//         // Draw the component
//         PrimitiveRenderer.RenderCircle(pos + realSize / 2f, 11f, 0f, Constants.COLOR_SELECTED, sides: 20);
//     }
// }