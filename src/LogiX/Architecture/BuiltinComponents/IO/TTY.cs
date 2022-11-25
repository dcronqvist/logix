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

public class TTYData : IComponentDescriptionData
{
    [ComponentDescriptionProperty("Label", StringHint = "e.g. MAIN_TTY", StringMaxLength = 16, StringRegexFilter = "^[a-zA-Z0-9_]*$")]
    public string Label { get; set; }

    [ComponentDescriptionProperty("Columns", IntMinValue = 1, IntMaxValue = 256, HelpTooltip = "The number of columns in the terminal.")]
    public int Width { get; set; }

    [ComponentDescriptionProperty("Rows", IntMinValue = 1, IntMaxValue = 256, HelpTooltip = "The number of rows in the terminal.")]
    public int Height { get; set; }

    [ComponentDescriptionProperty("Background Color")]
    public ColorF BackgroundColor { get; set; }

    [ComponentDescriptionProperty("Text Color")]
    public ColorF TextColor { get; set; }

    public static IComponentDescriptionData GetDefault()
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

[ScriptType("TTY"), ComponentInfo("TTY", "Input/Output", "core.markdown.tty")]
public class TTY : Component<TTYData>
{
    public override string Name => "";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private TTYData _data;
    internal EventHandler<char> OnCharReceived;

    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    private StringBuilder _buffer;
    public override void Initialize(TTYData data)
    {
        this.ClearIOs();
        this._data = data;

        this.RegisterIO("CLK", 1, ComponentSide.LEFT);
        this.RegisterIO("ADD", 1, ComponentSide.LEFT);

        this.RegisterIO("ASCII", 8, ComponentSide.LEFT);
        this.RegisterIO("CLEAR", 1, ComponentSide.BOTTOM);

        this._buffer = new StringBuilder(data.Width * data.Height);

        this.TriggerSizeRecalculation();
    }

    private LogicValue _previousClk = LogicValue.LOW;
    public override void PerformLogic()
    {
        var clk = this.GetIOFromIdentifier("CLK");
        var add = this.GetIOFromIdentifier("ADD");

        var ascii = this.GetIOFromIdentifier("ASCII");
        var clear = this.GetIOFromIdentifier("CLEAR");

        var vClk = clk.GetValues().First();
        var vAdd = add.GetValues().First();
        var vClear = clear.GetValues().First();

        if (vClear == LogicValue.HIGH)
        {
            this._buffer.Clear();
        }

        if (vClk == LogicValue.HIGH && this._previousClk == LogicValue.LOW)
        {
            if (vAdd == LogicValue.HIGH)
            {
                var vAscii = ascii.GetValues();

                char c = (char)vAscii.Reverse().GetAsByte();
                this.RegisterChar(c);
            }
        }

        this._previousClk = vClk;
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
    }

    public override void Interact(Camera2D cam)
    {

    }

    public override RectangleF GetBoundingBox(out Vector2 textSize)
    {
        var pos = this.Position.ToVector2(Constants.GRIDSIZE);

        var font = Utilities.GetFont("core.font.pixeloperator", 48);
        var charSize = font.MeasureString("_", 0.25f).X;

        textSize = font.MeasureString("_", 0.25f);

        return new RectangleF(pos.X, pos.Y, Utilities.CeilToMultipleOf(charSize * this._data.Width, Constants.GRIDSIZE), this._data.Height * Constants.GRIDSIZE).Inflate(1);
    }

    private float _caretInterval = 1f;
    private float _caretTimer = 0;
    private bool _caretVisible => this._caretTimer < this._caretInterval / 2;

    public override void Render(Camera2D camera)
    {
        var font = Utilities.GetFont("core.font.pixeloperator", 48);
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");
        var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.text");

        var pos = this.Position.ToVector2(Constants.GRIDSIZE);
        var rect = this.GetBoundingBox(out var textSize);
        var size = rect.GetSize().ToVector2i(Constants.GRIDSIZE);
        var realSize = size.ToVector2(Constants.GRIDSIZE);

        // Draw the component
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

        var lines = GetLines(this._buffer);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var linePos = new Vector2(pos.X, pos.Y + (i * Constants.GRIDSIZE));

            TextRenderer.RenderText(font, line, linePos, 0.25f, 0f, this._data.TextColor, camera);
            if (this._caretVisible && i == lines.Length - 1)
            {
                var caretPos = new Vector2(linePos.X + line.Length * textSize.X, linePos.Y);
                TextRenderer.RenderText(font, "_", caretPos, 0.25f, 0f, this._data.TextColor, camera);
            }
        }


        this._caretTimer += GameTime.DeltaTime;
        if (this._caretTimer > this._caretInterval)
        {
            this._caretTimer = 0;
        }
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