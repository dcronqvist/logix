using System.Drawing;
using System.Numerics;
using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public enum ConsumeBehaviour : int
{
    RisingEdge = 0,
    FallingEdge = 1,
}

public class KeyboardData : IComponentDescriptionData
{
    [ComponentDescriptionProperty("Label", StringHint = "e.g. MAIN_KBD", StringMaxLength = 16)]
    public string Label { get; set; }

    [ComponentDescriptionProperty("Max Buffer Size", IntMinValue = 1, IntMaxValue = 256, HelpTooltip = "The maximum number of keys that can be stored in the buffer at once.")]
    public int MaxBufferSize { get; set; }

    [ComponentDescriptionProperty("Background Color")]
    public ColorF BackgroundColor { get; set; }

    [ComponentDescriptionProperty("Text Color")]
    public ColorF TextColor { get; set; }

    [ComponentDescriptionProperty("Consume Behaviour")]
    public ConsumeBehaviour ConsumeBehaviour { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new KeyboardData()
        {
            Label = "",
            MaxBufferSize = 32,
            BackgroundColor = ColorF.White,
            TextColor = ColorF.Black,
            ConsumeBehaviour = ConsumeBehaviour.FallingEdge,
        };
    }
}

[ScriptType("KEYBOARD"), ComponentInfo("Keyboard", "Input/Output", "core.markdown.keyboard")]
public class Keyboard : Component<KeyboardData>
{
    public override string Name => "";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private KeyboardData _data;
    private bool _captureKeys = false;

    public Keyboard()
    {
        Input.OnChar += (sender, c) =>
        {
            if (_captureKeys && c < 128)
            {
                this.RegisterChar(c);
            }
        };

        Input.OnEnterPressed += (sender, e) =>
        {
            if (_captureKeys)
            {
                this.RegisterChar('\n');
            }
        };

        Input.OnBackspace += (sender, e) =>
        {
            if (_captureKeys)
            {
                this.RegisterChar('\b');
            }
        };

        Input.OnCharMods += (sender, e) =>
        {
            var c = e.Item1;
            var mods = e.Item2;
            if (_captureKeys && mods.HasFlag(ModifierKeys.Control) && c == 'l')
            {
                this.RegisterChar('\f');
            }
        };
    }

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    public override void Initialize(KeyboardData data)
    {
        this.ClearIOs();
        this._data = data;

        this.RegisterIO("CLK", 1, ComponentSide.LEFT);
        this.RegisterIO("CONSUME", 1, ComponentSide.LEFT);

        this.RegisterIO("CLEAR", 1, ComponentSide.TOP);

        this.RegisterIO("AVAILABLE", 1, ComponentSide.RIGHT);
        this.RegisterIO("ASCII", 8, ComponentSide.RIGHT);

        this._buffer = new(new Queue<char>(data.MaxBufferSize));

        this.TriggerSizeRecalculation();
    }

    private ThreadSafe<Queue<char>> _buffer;
    private LogicValue _previousClk = LogicValue.LOW;
    public override void PerformLogic()
    {
        var clk = this.GetIOFromIdentifier("CLK");
        var enable = this.GetIOFromIdentifier("CONSUME");
        var clear = this.GetIOFromIdentifier("CLEAR");
        var available = this.GetIOFromIdentifier("AVAILABLE");
        var ascii = this.GetIOFromIdentifier("ASCII");

        var vClk = clk.GetValues().First();
        var vEnable = enable.GetValues().First();
        var vClear = clear.GetValues().First();

        var enabled = vEnable == LogicValue.HIGH;

        this._buffer.LockedAction(b =>
        {
            if (vClear == LogicValue.HIGH)
            {
                b.Clear();
            }

            if (vClk.IsUndefined() || vEnable.IsUndefined() || vClear.IsUndefined())
            {
                return;
            }

            if (b.TryPeek(out var c))
            {
                available.Push(LogicValue.HIGH);
                ascii.Push(Utilities.GetAsLogicValues((byte)c, 8));
            }
            else
            {
                available.Push(LogicValue.LOW);
                ascii.Push(Utilities.GetAsLogicValues(0, 8));
            }

            if (enabled)
            {
                if (this._data.ConsumeBehaviour == ConsumeBehaviour.RisingEdge)
                {
                    if (vClk == LogicValue.HIGH && _previousClk == LogicValue.LOW)
                    {
                        b.TryDequeue(out var _);
                    }
                }
                else if (this._data.ConsumeBehaviour == ConsumeBehaviour.FallingEdge)
                {
                    if (vClk == LogicValue.LOW && _previousClk == LogicValue.HIGH)
                    {
                        b.TryDequeue(out var _);
                    }
                }
            }
        });

        this._previousClk = vClk;
    }

    public override void Interact(Camera2D cam)
    {

    }

    private float textScale = 0.25f;
    public override RectangleF GetBoundingBox(out Vector2 textSize)
    {
        var pos = this.Position.ToVector2(Constants.GRIDSIZE);

        var text = this._buffer.LockedAction(b => new string(b.ToArray()));
        var font = Utilities.GetFont("core.font.pixeloperator", 48);

        var charWidth = font.MeasureString("_", textScale).X;

        textSize = font.MeasureString("_", textScale);

        return new RectangleF(pos.X, pos.Y, Utilities.CeilToMultipleOf(charWidth * this._data.MaxBufferSize, Constants.GRIDSIZE), Constants.GRIDSIZE).Inflate(1);
    }

    public override void Render(Camera2D camera)
    {
        var font = Utilities.GetFont("core.font.pixeloperator", 48);
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");

        var pos = this.Position.ToVector2(Constants.GRIDSIZE);
        var rect = this.GetBoundingBox(out var textSize);
        var size = rect.GetSize().ToVector2i(Constants.GRIDSIZE);
        var realSize = size.ToVector2(Constants.GRIDSIZE);

        // Draw the component
        var textPos = (this.Rotation == 0 || this.Rotation == 2) ? pos + new Vector2(0, (realSize.Y - textSize.Y) / 2f) : pos + new Vector2((realSize.X + textSize.Y) / 2f, (realSize.Y - textSize.X) / 2f);

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

        PrimitiveRenderer.RenderRectangle(rect, Vector2.Zero, 0f, this._data.BackgroundColor);

        var text = this._buffer.LockedAction(b => b.Count == 0 ? "" : b.Select(c => c.ToString()).Aggregate((a, b) => a + b));
        TextRenderer.RenderText(font, text, textPos, textScale, this.Rotation == 0 || this.Rotation == 2 ? 0f : MathF.PI / 2f, this._data.TextColor, camera);
    }

    public void RegisterChar(char c)
    {
        this._buffer.LockedAction(b =>
        {
            if (b.Count < this._data.MaxBufferSize && c < 128)
            {
                b.Enqueue(c);
            }
        });
    }

    public override void SubmitUISelected(Editor editor, int componentIndex)
    {
        base.SubmitUISelected(editor, componentIndex);
        var id = this.GetUniqueIdentifier();
        ImGui.Checkbox($"Capture Keyboard Input##{id}", ref this._captureKeys); // Is only part of the current simulation
    }
}