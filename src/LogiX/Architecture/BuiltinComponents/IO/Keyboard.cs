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

public enum KeyboardBehaviour : int
{
    ScanCode = 0,
    ASCII = 1,
}

public class KeyboardData : INodeDescriptionData
{
    [NodeDescriptionProperty("Label", StringHint = "e.g. KBD_MAIN", StringMaxLength = 16, StringRegexFilter = "^[a-zA-Z0-9_]*$")]
    public string Label { get; set; }

    [NodeDescriptionProperty("Max Buffer Size", IntMinValue = 1, IntMaxValue = 256, HelpTooltip = "The maximum number of keys that can be stored in the buffer at once.")]
    public int MaxBufferSize { get; set; }

    [NodeDescriptionProperty("Background Color")]
    public ColorF BackgroundColor { get; set; }

    [NodeDescriptionProperty("Text Color")]
    public ColorF TextColor { get; set; }

    [NodeDescriptionProperty("Consume Behaviour")]
    public ConsumeBehaviour ConsumeBehaviour { get; set; }

    [NodeDescriptionProperty("Keyboard Behaviour")]
    public KeyboardBehaviour KeyboardBehaviour { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new KeyboardData()
        {
            Label = "",
            MaxBufferSize = 32,
            BackgroundColor = ColorF.White,
            TextColor = ColorF.Black,
            ConsumeBehaviour = ConsumeBehaviour.FallingEdge,
            KeyboardBehaviour = KeyboardBehaviour.ASCII,
        };
    }
}

[ScriptType("KEYBOARD"), NodeInfo("Keyboard", "Input/Output", "core.markdown.keyboard")]
public class Keyboard : Node<KeyboardData>
{
    private KeyboardData _data;
    private ThreadSafe<Queue<char>> _buffer;

    private bool _lastCLK = false;
    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var clr = pins.Get("CLR").Read(1).First().GetAsBool();
        var clk = pins.Get("CLK").Read(1).First().GetAsBool();
        var consume = pins.Get("CONSUME").Read(1).First().GetAsBool();
        var available = pins.Get("AVAILABLE");
        var key = this._data.KeyboardBehaviour == KeyboardBehaviour.ASCII ? pins.Get("ASCII") : pins.Get("SCANCODE");

        (LogicValue[] a, LogicValue[] k) = this._buffer.LockedAction(b =>
        {
            if (clr)
            {
                b.Clear();
                return (LogicValue.LOW.Multiple(1), LogicValue.LOW.Multiple(8));
            }

            if (consume)
            {
                var shouldConsume = this._data.ConsumeBehaviour switch
                {
                    ConsumeBehaviour.RisingEdge => !this._lastCLK && clk,
                    ConsumeBehaviour.FallingEdge => this._lastCLK && !clk,
                    _ => false,
                };

                if (shouldConsume)
                {
                    b.TryDequeue(out _);
                }
            }

            var peek = (uint)(b.Count > 0 ? b.Peek() : 0);
            _lastCLK = clk;
            return ((peek == 0 ? LogicValue.LOW : LogicValue.HIGH).Multiple(1), peek.GetAsLogicValues(8));
        });

        yield return (available, a, 1);
        yield return (key, k, 1);
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("CLR", 1, true, new Vector2i(1, 0));
        yield return new PinConfig("CLK", 1, true, new Vector2i(2, 0));

        yield return new PinConfig("CONSUME", 1, false, new Vector2i(3, 0));
        yield return new PinConfig("AVAILABLE", 1, false, new Vector2i(4, 0));

        if (this._data.KeyboardBehaviour == KeyboardBehaviour.ScanCode)
        {
            yield return new PinConfig("SCANCODE", 8, false, new Vector2i(5, 0));
        }
        else
        {
            yield return new PinConfig("ASCII", 8, false, new Vector2i(5, 0));
        }
    }

    public void EnterPress(object sender, EventArgs e)
    {
        if (_capture)
            this.RegisterChar('\n');
    }

    public void BackspacePress(object sender, EventArgs e)
    {
        if (_capture)
            this.RegisterChar('\b');
    }

    public void SpecialPress(object sender, Tuple<char, ModifierKeys> e)
    {
        if (_capture)
        {
            var c = e.Item1;
            var mods = e.Item2;
            if (mods.HasFlag(ModifierKeys.Control) && c == 'l')
            {
                this.RegisterChar('\f');
            }
        }
    }

    public void SetAsciiDown(object sender, char c)
    {
        if (_capture)
            this.RegisterChar(c);
    }

    public void SetScanCodeDown(object sender, int scanCode)
    {
        if (_capture)
            this.RegisterChar((char)scanCode);
    }

    public void RegisterChar(char c)
    {
        this._buffer.LockedAction(b =>
        {
            if (b.Count < this._data.MaxBufferSize && c < 128)
            {
                b.Enqueue(c);
                this.TriggerEvaluationNextTick();
            }
        });
    }

    public override void Register(Scheduler scheduler)
    {
        Input.OnChar -= SetAsciiDown;
        Input.OnEnterPressed -= EnterPress;
        Input.OnBackspace -= BackspacePress;
        Input.OnCharMods -= SpecialPress;
        Input.OnKeyPressOrRepeatScanCode -= SetScanCodeDown;

        if (this._data.KeyboardBehaviour == KeyboardBehaviour.ASCII)
        {
            Input.OnChar += SetAsciiDown;
            Input.OnEnterPressed += EnterPress;
            Input.OnBackspace += BackspacePress;
            Input.OnCharMods += SpecialPress;
        }
        else
        {
            Input.OnKeyPressOrRepeatScanCode += SetScanCodeDown;
        }
    }

    public override Vector2i GetSize()
    {
        var font = Constants.NODE_FONT_REAL;
        var width = 8f * this._data.MaxBufferSize;
        return new Vector2i((int)Math.Max(2, (width.CeilToMultipleOf(Constants.GRIDSIZE) / Constants.GRIDSIZE) + 1), 2);
    }

    public override void Initialize(KeyboardData data)
    {
        this._data = data;
        this._buffer = new(new());
    }

    public override bool IsNodeInRect(RectangleF rect)
    {
        return this.Position.ToVector2(Constants.GRIDSIZE).CreateRect(this.GetSizeRotated().ToVector2(Constants.GRIDSIZE)).IntersectsWith(rect);
    }

    public override void RenderSelected(Camera2D camera)
    {
        var pos = this.Position.ToVector2(Constants.GRIDSIZE);
        var size = this.GetSizeRotated().ToVector2(Constants.GRIDSIZE);
        var rect = pos.CreateRect(size);

        PrimitiveRenderer.RenderRectangle(rect.Inflate(2), Vector2.Zero, 0f, Constants.COLOR_SELECTED);
    }

    public override void Render(PinCollection pins, Camera2D camera)
    {
        var pos = this.Position.ToVector2(Constants.GRIDSIZE);
        var size = this.GetSizeRotated().ToVector2(Constants.GRIDSIZE);
        var rect = pos.CreateRect(size);

        PrimitiveRenderer.RenderRectangleWithBorder(rect, Vector2.Zero, 0f, 1, this._data.BackgroundColor, ColorF.Black);

        this._buffer.LockedAction(b =>
        {
            var s = b.Count > 0 ? b.Select(c => c.GetLegibleString()).Aggregate((a, b) => a + b) : "";
            var font = Constants.NODE_FONT_REAL;
            var measure = font.MeasureString(s, 1f);
            var p = pos + new Vector2(5, size.Y / 2f) - new Vector2(0, measure.Y / 2f);
            //TextRenderer.RenderText(font, s, p, 1f, 0f, this._data.TextColor, fixedAdvance: 8f);
        });

        base.Render(pins, camera);
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        return false;
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        yield return (pins.Get("AVAILABLE"), LogicValue.LOW.Multiple(1));
        yield return (pins.Get(this._data.KeyboardBehaviour == KeyboardBehaviour.ScanCode ? "SCANCODE" : "ASCII"), LogicValue.LOW.Multiple(8));
    }

    internal bool _capture = false;
    public override void SubmitUISelected(Editor editor, int componentIndex)
    {
        base.SubmitUISelected(editor, componentIndex);
        ImGui.Checkbox("Capture Keyboard", ref this._capture);
    }
}