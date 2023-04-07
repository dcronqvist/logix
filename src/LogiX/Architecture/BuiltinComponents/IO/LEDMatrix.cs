using System.Drawing;
using System.Numerics;
using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public enum LEDMatrixMode
{
    Circular,
    Squares
}

public class LEDMatrixData : INodeDescriptionData
{
    [NodeDescriptionProperty("Label", StringHint = "e.g. MATX_MAIN", StringMaxLength = 16, StringRegexFilter = "^[a-zA-Z0-9_]*$")]
    public string Label { get; set; }

    [NodeDescriptionProperty("Columns", IntMinValue = 1, IntMaxValue = 256, HelpTooltip = "The number of columns in the matrix.")]
    public int Columns { get; set; }

    [NodeDescriptionProperty("Rows", IntMinValue = 1, IntMaxValue = 256, HelpTooltip = "The number of rows in the matrix.")]
    public int Rows { get; set; }

    [NodeDescriptionProperty("LED Off Color")]
    public ColorF OffColor { get; set; }

    [NodeDescriptionProperty("LED On Color")]
    public ColorF OnColor { get; set; }

    [NodeDescriptionProperty("Background Color")]
    public ColorF BackgroundColor { get; set; }

    [NodeDescriptionProperty("Mode", HelpTooltip = "Setting to square will make the LED matrix look like a grid of squares. Setting to circular will make the LED matrix look like a grid of circles.")]
    public LEDMatrixMode Mode { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new LEDMatrixData()
        {
            Label = "",
            OffColor = ColorF.Red.Darken(0.2f),
            OnColor = ColorF.Red,
            BackgroundColor = ColorF.White,
            Columns = 12,
            Rows = 8,
            Mode = LEDMatrixMode.Circular
        };
    }
}

[ScriptType("LEDMATRIX"), NodeInfo("LED Matrix", "Input/Output", "logix_core:docs/components/ledmatrix.md")]
public class LEDMatrix : Node<LEDMatrixData>
{
    private LEDMatrixData _data;

    internal LogicValue[,] _matrix;
    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        for (int i = 0; i < this._data.Columns; i++)
        {
            var pin = pins.Get($"C{i}");
            var column = pin.Read(this._data.Rows);
            for (int j = 0; j < this._data.Rows; j++)
            {
                this._matrix[i, j] = column[j];
            }
        }

        yield break;
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        for (int i = 0; i < this._data.Columns; i++)
        {
            yield return new PinConfig($"C{i}", this._data.Rows, true, new Vector2i((i * 2) + 2, this.GetSize().Y));
        }
    }

    public override Vector2i GetSize()
    {
        return new Vector2i((this._data.Columns * 2) + 2, (this._data.Rows * 2) + 2);
    }

    public override void Initialize(LEDMatrixData data)
    {
        this._data = data;
        this._matrix = new LogicValue[data.Columns, data.Rows];
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

        var ledSize = Constants.GRIDSIZE;
        var mode = this._data.Mode;

        for (int i = 0; i < this._data.Columns; i++)
        {
            for (int j = 0; j < this._data.Rows; j++)
            {
                var x = pos.X + (i * 2 + 1) * Constants.GRIDSIZE + ledSize;
                var y = pos.Y + (j * 2 + 1) * Constants.GRIDSIZE + ledSize;

                if (mode == LEDMatrixMode.Circular)
                {
                    PrimitiveRenderer.RenderCircle(new Vector2(x, y), ledSize, 0f, this._matrix[i, j] == LogicValue.HIGH ? this._data.OnColor : this._data.OffColor, 1f, sides: 20);
                }
                else
                {
                    PrimitiveRenderer.RenderRectangle(new RectangleF(x - ledSize, y - ledSize, ledSize * 2, ledSize * 2), Vector2.Zero, 0f, this._matrix[i, j] == LogicValue.HIGH ? this._data.OnColor : this._data.OffColor);
                }
                //PrimitiveRenderer.RenderRectangle(ledRect, Vector2.Zero, 0f, this._matrix[i, j] == LogicValue.HIGH ? this._data.ForegroundColor : this._data.BackgroundColor);
            }
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
}