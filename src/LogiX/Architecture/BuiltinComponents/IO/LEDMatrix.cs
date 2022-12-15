using System.Drawing;
using System.Numerics;
using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.GLFW;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

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

    public INodeDescriptionData GetDefault()
    {
        return new LEDMatrixData()
        {
            Label = "",
            OffColor = ColorF.Red.Darken(0.2f),
            OnColor = ColorF.Red,
            BackgroundColor = ColorF.White,
            Columns = 12,
            Rows = 8
        };
    }
}

[ScriptType("LEDMATRIX"), NodeInfo("LED Matrix", "Input/Output", "core.markdown.ledmatrix")]
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

        for (int i = 0; i < this._data.Columns; i++)
        {
            for (int j = 0; j < this._data.Rows; j++)
            {
                var x = pos.X + (i * 2 + 1) * Constants.GRIDSIZE + ledSize;
                var y = pos.Y + (j * 2 + 1) * Constants.GRIDSIZE + ledSize;

                PrimitiveRenderer.RenderCircle(new Vector2(x, y), ledSize, 0f, this._matrix[i, j] == LogicValue.HIGH ? this._data.OnColor : this._data.OffColor, 1f, sides: 20);
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

// [ScriptType("LEDMATRIX"), ComponentInfo("LED Matrix", "Input/Output", "core.markdown.pushbutton")]
// public class LEDMatrix : Component<LEDMatrixData>
// {
//     public override string Name => "";
//     public override bool DisplayIOGroupIdentifiers => true;
//     public override bool ShowPropertyWindow => true;

//     private LEDMatrixData _data;

//     public override IComponentDescriptionData GetDescriptionData()
//     {
//         return _data;
//     }

//     public override void Initialize(LEDMatrixData data)
//     {
//         this.ClearIOs();
//         this._data = data;

//         for (int i = 0; i < data.Columns; i++)
//         {
//             this.RegisterIO($"C{i}", data.Rows, ComponentSide.BOTTOM, "columns");
//         }

//         this.TriggerSizeRecalculation();
//     }

//     public override RectangleF GetBoundingBox(out Vector2 textSize)
//     {
//         textSize = Vector2.Zero;
//         var pos = this.Position.ToVector2(Constants.GRIDSIZE);
//         return pos.CreateRect(new Vector2(_data.Columns * Constants.GRIDSIZE, _data.Rows * Constants.GRIDSIZE)).Inflate(1);
//     }

//     internal LogicValue[][] _matrix = new LogicValue[0][];
//     public override void PerformLogic()
//     {
//         var columnIOs = this.GetIOsWithTag("columns");
//         _matrix = new LogicValue[columnIOs.Length][];
//         for (int i = 0; i < columnIOs.Length; i++)
//         {
//             _matrix[i] = columnIOs[i].GetValues();
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

//         PrimitiveRenderer.RenderRectangle(rect, Vector2.Zero, 0f, ColorF.Black);
//         PrimitiveRenderer.RenderRectangle(rect.Inflate(-1), Vector2.Zero, 0f, _data.BackgroundColor);

//         for (int i = 0; i < this._matrix.Length; i++)
//         {
//             var column = this._matrix[i];
//             for (int j = 0; j < column.Length; j++)
//             {
//                 var value = column[j];
//                 if (value == LogicValue.HIGH)
//                 {
//                     var x = pos.X + i * Constants.GRIDSIZE;
//                     var y = pos.Y + j * Constants.GRIDSIZE;
//                     PrimitiveRenderer.RenderRectangle(new RectangleF(x, y, Constants.GRIDSIZE, Constants.GRIDSIZE), Vector2.Zero, 0f, _data.ForegroundColor);
//                 }
//             }
//         }
//     }

//     // public override void RenderSelected(Camera2D camera)
//     // {
//     //     // Position of component
//     //     var font = Utilities.GetFont("core.font.default", 8); //LogiX.ContentManager.GetContentItem<Font>("core.font.default-regular-8");
//     //     var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");
//     //     var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.text");

//     //     var pos = this.Position.ToVector2(Constants.GRIDSIZE);
//     //     var rect = this.GetBoundingBox(out _);
//     //     var size = rect.GetSize().ToVector2i(Constants.GRIDSIZE);
//     //     var realSize = size.ToVector2(Constants.GRIDSIZE);

//     //     // Draw the component
//     //     PrimitiveRenderer.RenderRectangle(rect.Inflate(3), Vector2.Zero, 0f, _data.BackgroundColor);
//     // }
// }