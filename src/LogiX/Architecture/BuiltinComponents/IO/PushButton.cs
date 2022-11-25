using System.Numerics;
using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class PushButtonData : IComponentDescriptionData
{
    [ComponentDescriptionProperty("Label", StringHint = "e.g. BTN_RESET", StringMaxLength = 16, StringRegexFilter = "^[a-zA-Z0-9_]*$")]
    public string Label { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new PushButtonData()
        {
            Label = ""
        };
    }
}

[ScriptType("PUSHBUTTON"), ComponentInfo("Button", "Input/Output", "core.markdown.pushbutton")]
public class PushButton : Component<PushButtonData>
{
    public override string Name => "";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private PushButtonData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(PushButtonData data)
    {
        this.ClearIOs();
        this._data = data;

        if (this._data.Label is null)
        {
            this._data.Label = "";
        }

        this.RegisterIO("Y", 1, ComponentSide.RIGHT);
        this.TriggerSizeRecalculation();
    }

    internal LogicValue _value;
    public override void PerformLogic()
    {
        var y = this.GetIOFromIdentifier("Y");
        y.Push(_value);
    }

    public override void Interact(Camera2D cam)
    {
        var bounds = this.GetBoundingBox(out var _);
        var mousePos = Input.GetMousePosition(cam);

        this._value = LogicValue.LOW;

        if (Input.IsMouseButtonDown(MouseButton.Right))
        {
            if (bounds.Contains(mousePos))
            {
                _value = LogicValue.HIGH;
            }
        }
    }

    public override void Render(Camera2D camera)
    {
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");

        var pos = this.Position.ToVector2(Constants.GRIDSIZE);
        var rect = this.GetBoundingBox(out var textSize);
        var size = rect.GetSize().ToVector2i(Constants.GRIDSIZE);
        var realSize = size.ToVector2(Constants.GRIDSIZE);

        var ios = this.IOs;
        for (int i = 0; i < ios.Length; i++)
        {
            var io = ios[i];
            var ioPos = this.GetPositionForIO(io, out var lineEnd);
            var lineEndPos = new Vector2(lineEnd.X * Constants.GRIDSIZE, lineEnd.Y * Constants.GRIDSIZE);

            // Draw the group
            var gPos = new Vector2(ioPos.X * Constants.GRIDSIZE, ioPos.Y * Constants.GRIDSIZE);
            int lineThickness = 2;
            var groupCol = this.GetIOColor(i);

            PrimitiveRenderer.RenderLine(gPos, lineEndPos, lineThickness, groupCol.Darken(0.5f));
            PrimitiveRenderer.RenderCircle(gPos, Constants.IO_GROUP_RADIUS, 0f, groupCol);
        }

        var isHovered = rect.Contains(Input.GetMousePosition(camera));
        var isClicked = isHovered && Input.IsMouseButtonDown(MouseButton.Right);

        var color = isHovered ? (isClicked ? ColorF.White : ColorF.LightGray) : ColorF.Gray;

        PrimitiveRenderer.RenderCircle(pos + realSize / 2f, 10f, 0f, ColorF.White, sides: 20);
        PrimitiveRenderer.RenderCircle(pos + realSize / 2f, 8.5f, 0f, color.Darken(0.5f), sides: 20);
        PrimitiveRenderer.RenderCircle(pos + realSize / 2f, 8 - (this._value == LogicValue.HIGH ? 1 : 0f), 0f, color, sides: 20);
    }

    public override void RenderSelected(Camera2D camera)
    {
        // Position of component
        var font = Utilities.GetFont("core.font.default", 8); //LogiX.ContentManager.GetContentItem<Font>("core.font.default-regular-8");
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");
        var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.text");

        var pos = this.Position.ToVector2(Constants.GRIDSIZE);
        var rect = this.GetBoundingBox(out _);
        var size = rect.GetSize().ToVector2i(Constants.GRIDSIZE);
        var realSize = size.ToVector2(Constants.GRIDSIZE);

        // Draw the component
        PrimitiveRenderer.RenderCircle(pos + realSize / 2f, 11f, 0f, Constants.COLOR_SELECTED, sides: 20);
    }
}