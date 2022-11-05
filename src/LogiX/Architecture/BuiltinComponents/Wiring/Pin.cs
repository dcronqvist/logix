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

public enum PinBehaviour : byte
{
    INPUT = 0,
    OUTPUT = 1
}

public class PinData : IComponentDescriptionData
{
    public int Bits { get; set; }
    public LogicValue[] Values { get; set; }
    public string Label { get; set; }
    public PinBehaviour Behaviour { get; set; }
    public ComponentSide Side { get; set; }
    public bool IsExternal { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new PinData()
        {
            Bits = 1,
            Values = new LogicValue[] { LogicValue.UNDEFINED },
            Label = "",
            Behaviour = PinBehaviour.INPUT,
            Side = ComponentSide.LEFT,
            IsExternal = true
        };
    }
}

[ScriptType("PIN"), ComponentInfo("Pin", "Wiring", "core.markdown.pin")]
public class Pin : Component<PinData>
{
    public override string Name => this.CurrentValues.Select(x => x.ToString().Substring(0, 1)).Aggregate((x, y) => x + y);
    public override bool DisplayIOGroupIdentifiers => false;
    public override bool ShowPropertyWindow => true;

    public LogicValue[] CurrentValues { get; set; }
    private PinData _data;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return new PinData()
        {
            Bits = CurrentValues.Length,
            Values = CurrentValues,
            Label = this._data.Label,
            Behaviour = this._data.Behaviour,
            Side = this._data.Side,
            IsExternal = this._data.IsExternal
        };
    }

    public override void Initialize(PinData data)
    {
        this.ClearIOs();

        this._data = data;
        this.CurrentValues = data.Values;
        this.RegisterIO($"io", data.Bits, ComponentSide.RIGHT, "io");
    }

    public override void PerformLogic()
    {
        if (this._data.Behaviour == PinBehaviour.INPUT)
        {
            this.CurrentValues = this.CurrentValues.Select(v => v == LogicValue.UNDEFINED ? LogicValue.LOW : v).ToArray();

            var io = this.GetIOFromIdentifier("io");
            io.Push(this.CurrentValues);
        }
        else
        {
            var io = this.GetIOFromIdentifier("io");
            this.CurrentValues = io.GetValues();
        }
    }

    public override void SubmitUISelected(Editor editor, int componentIndex)
    {
        int bits = this.CurrentValues.Length;
        var label = this._data.Label;
        if (ImGui.InputTextWithHint($"Label##{this.GetUniqueIdentifier()}", "Label", ref label, 10))
        {
            this._data.Label = label;
            this.Initialize(this._data);
        }
        var currentBehaviour = (int)this._data.Behaviour;
        ImGui.Combo($"Behaviour##{this.GetUniqueIdentifier()}", ref currentBehaviour, new string[] { "Input", "Output" }, 2);
        this._data.Behaviour = (PinBehaviour)currentBehaviour;
        var currentSide = (int)this._data.Side;
        ImGui.Combo($"Side##{this.GetUniqueIdentifier}", ref currentSide, new string[] { "Top", "Bottom", "Left", "Right" }, 4);
        this._data.Side = (ComponentSide)currentSide;
        var currentBits = this._data.Bits;
        ImGui.InputInt($"Bits##{this.GetUniqueIdentifier()}", ref currentBits, 1, 1);
        currentBits = Math.Clamp(currentBits, 1, 64);
        if (currentBits != this._data.Bits)
        {
            this._data.Bits = currentBits;
            this._data.Values = Enumerable.Repeat(LogicValue.UNDEFINED, currentBits).ToArray();
            this.Initialize(this._data);
            this.TriggerSizeRecalculation();
        }
        var external = this._data.IsExternal;
        if (ImGui.Checkbox($"External##{this.GetUniqueIdentifier()}", ref external))
        {
            this._data.IsExternal = external;
            this.TriggerSizeRecalculation();
        }
    }

    public override void Interact(Camera2D cam)
    {
        if (this._data.Behaviour == PinBehaviour.INPUT)
        {
            var rect = this.GetBoundingBox(out _);
            var mousePos = Input.GetMousePosition(cam);
            var pos = this.Position.ToVector2(Constants.GRIDSIZE);

            if (rect.Contains(mousePos))
            {
                if (Input.IsMouseButtonPressed(MouseButton.Right))
                {
                    // Get which bit was clicked
                    for (int i = 0; i < this._data.Bits; i++)
                    {
                        var bitPos = pos + new Vector2(i * Constants.GRIDSIZE, 0);
                        var bitRect = bitPos.CreateRect(new Vector2(Constants.GRIDSIZE, Constants.GRIDSIZE)).Inflate(-1);
                        if (bitRect.Contains(mousePos))
                        {
                            this.CurrentValues[i] = this.CurrentValues[i] == LogicValue.HIGH ? LogicValue.LOW : LogicValue.HIGH;
                            break;
                        }
                    }
                }
            }
        }
    }

    public override RectangleF GetBoundingBox(out Vector2 textSize)
    {
        if (this._bounds != RectangleF.Empty)
        {
            textSize = this._textSize;
            return this._bounds;
        }

        var amountOfBits = this._data.Bits;
        var gridSize = Constants.GRIDSIZE;

        var needsWidth = amountOfBits * gridSize;
        var position = this.Position.ToVector2(gridSize); ;
        var rect = position.CreateRect(new Vector2(needsWidth, gridSize)).Inflate(1);

        textSize = Vector2.Zero;
        this._textSize = textSize;
        this._bounds = rect;
        return rect;
    }

    public override void Render(Camera2D camera)
    {
        if (this._data.Behaviour == PinBehaviour.INPUT)
        {
            var font = LogiX.ContentManager.GetContentItem<Font>("core.font.default");
            var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");
            var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.text");

            var pos = this.Position.ToVector2(Constants.GRIDSIZE);
            var rect = this.GetBoundingBox(out var textSize);
            var size = rect.GetSize().ToVector2i(Constants.GRIDSIZE);
            var realSize = size.ToVector2(Constants.GRIDSIZE);
            var measure = font.MeasureString(this._data.Label, 1f);

            // Draw the component
            var textPos = pos - new Vector2(measure.X + 2, 0) + new Vector2(0, realSize.Y / 2f - measure.Y / 2f);

            var io = this.IOs[0];
            var ioPos = this.GetPositionForIO(io, out var lineEnd);
            var lineEndPos = new Vector2(lineEnd.X * Constants.GRIDSIZE, lineEnd.Y * Constants.GRIDSIZE);

            // Draw the group
            var gPos = new Vector2(ioPos.X * Constants.GRIDSIZE, ioPos.Y * Constants.GRIDSIZE);
            int lineThickness = 2;
            var groupCol = this.GetIOColor(0);

            PrimitiveRenderer.RenderLine(gPos, lineEndPos, lineThickness, groupCol.Darken(0.5f));
            PrimitiveRenderer.RenderCircle(gPos, Constants.IO_GROUP_RADIUS, 0f, groupCol);

            PrimitiveRenderer.RenderRectangle(rect, Vector2.Zero, 0f, ColorF.White);

            for (int i = 0; i < this._data.Bits; i++)
            {
                var bitPos = pos + new Vector2(i * Constants.GRIDSIZE, 0);
                var bitRect = bitPos.CreateRect(new Vector2(Constants.GRIDSIZE, Constants.GRIDSIZE)).Inflate(-1);
                var bitCol = Utilities.GetValueColor(this.CurrentValues[i]);
                PrimitiveRenderer.RenderRectangle(bitRect, Vector2.Zero, 0f, bitCol);
            }

            TextRenderer.RenderText(tShader, font, this._data.Label, textPos, 1f, ColorF.Black, camera);
        }
        else
        {
            var font = LogiX.ContentManager.GetContentItem<Font>("core.font.default");
            var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");
            var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.text");

            var pos = this.Position.ToVector2(Constants.GRIDSIZE);
            var rect = this.GetBoundingBox(out var textSize);
            var size = rect.GetSize().ToVector2i(Constants.GRIDSIZE);
            var realSize = size.ToVector2(Constants.GRIDSIZE);
            var measure = font.MeasureString(this._data.Label, 1f);

            // Draw the component
            var textPos = pos - new Vector2(measure.X + 2, 0) + new Vector2(0, realSize.Y / 2f - measure.Y / 2f);

            var io = this.IOs[0];
            var ioPos = this.GetPositionForIO(io, out var lineEnd);
            var lineEndPos = new Vector2(lineEnd.X * Constants.GRIDSIZE, lineEnd.Y * Constants.GRIDSIZE);

            // Draw the group
            var gPos = new Vector2(ioPos.X * Constants.GRIDSIZE, ioPos.Y * Constants.GRIDSIZE);
            int lineThickness = 2;
            var groupCol = this.GetIOColor(0);

            PrimitiveRenderer.RenderLine(gPos, lineEndPos, lineThickness, groupCol.Darken(0.5f));
            PrimitiveRenderer.RenderCircle(gPos, Constants.IO_GROUP_RADIUS, 0f, groupCol);

            PrimitiveRenderer.RenderRectangle(rect, Vector2.Zero, 0f, ColorF.White);

            for (int i = 0; i < this._data.Bits; i++)
            {
                var bitPos = pos + new Vector2(i * Constants.GRIDSIZE, 0);
                var bitRect = bitPos.CreateRect(new Vector2(Constants.GRIDSIZE, Constants.GRIDSIZE));
                var bitCol = Utilities.GetValueColor(this.CurrentValues[i]);
                PrimitiveRenderer.RenderCircle(bitPos + new Vector2(Constants.GRIDSIZE / 2f), Constants.GRIDSIZE / 2f - 1, 0f, bitCol);
            }

            TextRenderer.RenderText(tShader, font, this._data.Label, textPos, 1f, ColorF.Black, camera);
        }
    }
}