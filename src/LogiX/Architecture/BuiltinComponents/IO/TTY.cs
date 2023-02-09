using System.Drawing;
using System.Numerics;
using System.Text;
using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class TTYData : INodeDescriptionData
{
    [NodeDescriptionProperty("Label", StringHint = "e.g. TTY_MAIN", StringMaxLength = 16, StringRegexFilter = "^[a-zA-Z0-9_]*$")]
    public string Label { get; set; }

    [NodeDescriptionProperty("Columns", IntMinValue = 1, IntMaxValue = 256, HelpTooltip = "The number of columns in the terminal.")]
    public int Width { get; set; }

    [NodeDescriptionProperty("Rows", IntMinValue = 5, IntMaxValue = 256, HelpTooltip = "The number of rows in the terminal.")]
    public int Height { get; set; }

    [NodeDescriptionProperty("Background Color")]
    public ColorF BackgroundColor { get; set; }

    [NodeDescriptionProperty("Text Color")]
    public ColorF TextColor { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new TTYData()
        {
            Label = "",
            Width = 30,
            Height = 10,
            BackgroundColor = ColorF.White,
            TextColor = ColorF.Black
        };
    }
}

[ScriptType("TTY"), NodeInfo("TTY", "Input/Output", "core.markdown.tty")]
public class TTY : Node<TTYData>
{
    private TTYData _data;
    private StringBuilder _buffer = new StringBuilder();
    internal EventHandler<char> OnCharReceived;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var clk = pins.Get("CLK").Read(1).First().GetAsBool();
        var clr = pins.Get("CLR").Read(1).First().GetAsBool();
        var add = pins.Get("ADD").Read(1).First().GetAsBool();
        var ascii = pins.Get("ASCII").Read(8).Reverse();

        if (clr)
        {
            this.RegisterChar('\f');
        }
        else if (clk && add)
        {
            var c = (char)ascii.GetAsByte();
            this.RegisterChar(c);
        }

        yield break;
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("CLK", 1, true, new Vector2i(0, 1));
        yield return new PinConfig("CLR", 1, true, new Vector2i(0, 2));
        yield return new PinConfig("ADD", 1, false, new Vector2i(0, 3));
        yield return new PinConfig("ASCII", 8, false, new Vector2i(0, 4));
    }

    private void RegisterChar(char c)
    {
        if (c == '\b')
        {
            if (this._buffer.Length > 0)
            {
                this._buffer.Remove(this._buffer.Length - 1, 1);
            }
        }
        else if (c == 0x0C)
        {
            this._buffer.Clear();
        }
        else
        {
            this._buffer.Append(c);
        }
        this.OnCharReceived?.Invoke(this, c);
        this._caretTimer = 0;
    }

    public override Vector2i GetSize()
    {
        var font = Utilities.GetFont("core.font.inconsolata");
        float scale = 0.30f;
        var measure = font.MeasureString("M", scale);
        var width = (this._data.Width * measure.X);
        var widthAligned = width.CeilToMultipleOf(Constants.GRIDSIZE);
        var widthI = (int)widthAligned / Constants.GRIDSIZE;

        var height = (this._data.Height * measure.Y);
        var heightAligned = height.CeilToMultipleOf(Constants.GRIDSIZE);
        var heightI = (int)heightAligned / Constants.GRIDSIZE;

        return new Vector2i(widthI + 3, heightI + 3);
    }

    public override void Initialize(TTYData data)
    {
        this._data = data;
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

    private float _caretInterval = 1f;
    private float _caretTimer = 0;
    private bool _caretVisible => this._caretTimer < this._caretInterval / 2;

    public override void Render(PinCollection pins, Camera2D camera)
    {
        var pos = this.Position.ToVector2(Constants.GRIDSIZE);
        var size = this.GetSizeRotated().ToVector2(Constants.GRIDSIZE);
        var rect = pos.CreateRect(size);
        var font = Utilities.GetFont("core.font.inconsolata");

        PrimitiveRenderer.RenderRectangleWithBorder(rect, Vector2.Zero, 0f, 1, this._data.BackgroundColor, ColorF.Black);

        var lines = GetLines(this._buffer);
        float scale = 0.30f;

        var lineHeight = size.Y / this._data.Height - 2;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var linePos = new Vector2(pos.X + Constants.GRIDSIZE, pos.Y + (i * lineHeight) + Constants.GRIDSIZE);

            if (this._caretVisible && i == lines.Length - 1)
            {
                line = line + "_";
            }

            TextRenderer.RenderText(font, line, linePos, scale, 0f, this._data.TextColor, true, 0.4f, 0.1f, -1f, Constants.GRIDSIZE);
        }


        this._caretTimer += GameTime.DeltaTime;
        if (this._caretTimer > this._caretInterval)
        {
            this._caretTimer = 0;
        }

        base.Render(pins, camera);
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        return false;
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        yield break;
    }

    private string[] GetLines(StringBuilder stringBuilder)
    {
        // every line must be at most this._data.Width characters long
        // and we must have at most this._data.Height lines
        var lines = new List<string>();
        string currentLine = "";
        for (int i = 0; i < stringBuilder.Length; i++)
        {
            var currentChar = (char)stringBuilder[i];
            var newlineChars = "\r\n";
            if (currentLine.Length >= this._data.Width || newlineChars.Contains((char)currentChar))
            {
                lines.Add(currentLine);
                currentLine = "";
            }

            if (!newlineChars.Contains((char)currentChar))
            {
                currentLine += currentChar;
            }

            if (i == stringBuilder.Length - 1)
            {
                lines.Add(currentLine);

                if (currentLine.Length == this._data.Width)
                {
                    lines.Add("");
                }
            }
        }

        return lines.TakeLast(this._data.Height).ToArray();
    }
}