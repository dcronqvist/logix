using System.Drawing;
using System.Numerics;
using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class HexDisplayData : IComponentDescriptionData
{
    [ComponentDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 256)]
    public int DataBits { get; set; }

    public static IComponentDescriptionData GetDefault()
    {
        return new HexDisplayData()
        {
            DataBits = 4,
        };
    }
}

[ScriptType("HEXDISPLAY"), ComponentInfo("Hex Display", "Common", "core.markdown.hexdisplay")]
public class HexDisplay : Component<HexDisplayData>
{
    public override string Name => "";
    public override bool DisplayIOGroupIdentifiers => true;
    public override bool ShowPropertyWindow => true;

    private HexDisplayData _data;

    private LogicValue[] _values;
    public override IComponentDescriptionData GetDescriptionData()
    {
        return _data;
    }

    private Dictionary<uint, (Vector2, Vector2)[]> segments;
    public override void Initialize(HexDisplayData data)
    {
        this.ClearIOs();
        this._data = data;

        this.RegisterIO("IN", this._data.DataBits, ComponentSide.LEFT);
        this.TriggerSizeRecalculation();

        // RENDER SEGMENTS
        var top = (new Vector2(0.65f, 0.5f), new Vector2(2.35f, 0.5f));
        var bottom = (new Vector2(0.65f, 4.5f), new Vector2(2.35f, 4.5f));
        var leftTop = (new Vector2(0.5f, 0.65f), new Vector2(0.5f, 2.35f));
        var leftBottom = (new Vector2(0.5f, 2.65f), new Vector2(0.5f, 4.35f));
        var rightTop = (new Vector2(2.5f, 0.65f), new Vector2(2.5f, 2.35f));
        var rightBottom = (new Vector2(2.5f, 2.65f), new Vector2(2.5f, 4.35f));
        var middle = (new Vector2(0.65f, 2.5f), new Vector2(2.35f, 2.5f));

        segments = new Dictionary<uint, (Vector2, Vector2)[]>()
        {
            { 0x0u, Utilities.Arrayify(top, bottom, leftBottom, leftTop, rightBottom, rightTop) },
            { 0x1u, Utilities.Arrayify(rightBottom, rightTop) },
            { 0x2u, Utilities.Arrayify(top, bottom, middle, leftBottom, rightTop) },
            { 0x3u, Utilities.Arrayify(top, bottom, middle, rightBottom, rightTop) },
            { 0x4u, Utilities.Arrayify(middle, leftTop, rightBottom, rightTop) },
            { 0x5u, Utilities.Arrayify(top, bottom, middle, leftTop, rightBottom) },
            { 0x6u, Utilities.Arrayify(top, bottom, middle, leftTop, leftBottom, rightBottom) },
            { 0x7u, Utilities.Arrayify(top, rightBottom, rightTop) },
            { 0x8u, Utilities.Arrayify(top, bottom, middle, leftBottom, leftTop, rightBottom, rightTop) },
            { 0x9u, Utilities.Arrayify(top, bottom, middle, leftTop, rightBottom, rightTop) },
            { 0xAu, Utilities.Arrayify(top, middle, leftBottom, leftTop, rightBottom, rightTop) },
            { 0xBu, Utilities.Arrayify(bottom, middle, leftBottom, rightBottom, leftTop) },
            { 0xCu, Utilities.Arrayify(top, bottom, leftBottom, leftTop) },
            { 0xDu, Utilities.Arrayify(bottom, leftBottom, rightBottom, rightTop, middle) },
            { 0xEu, Utilities.Arrayify(top, bottom, middle, leftBottom, leftTop) },
            { 0xFu, Utilities.Arrayify(top, middle, leftBottom, leftTop) },
        };

        this._values = Enumerable.Repeat(LogicValue.UNDEFINED, this._data.DataBits).ToArray();
    }

    public override void PerformLogic()
    {
        var io = this.GetIOFromIdentifier("IN");
        this._values = io.GetValues();
    }

    public override RectangleF GetBoundingBox(out Vector2 textSize)
    {
        if (this._bounds != RectangleF.Empty)
        {
            textSize = _textSize;
            return this._bounds;
        }

        var gridSize = Constants.GRIDSIZE;
        var digits = (int)Math.Ceiling(this._data.DataBits / 4f);
        var spacing = 1 * gridSize;
        var digitWidth = 2 * gridSize;
        var digitHeight = 4 * gridSize;
        var size = new Vector2(digitWidth * digits + 1 * gridSize + spacing * (digits - 1), digitHeight + 1 * gridSize);

        this._bounds = this.Position.ToVector2(Constants.GRIDSIZE).CreateRect(size);
        textSize = Vector2.Zero;
        return this._bounds;
    }

    public override void Render(Camera2D camera)
    {
        //this.TriggerSizeRecalculation();
        // Position of component

        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("core.shader_program.primitive");

        var pos = this.Position.ToVector2(Constants.GRIDSIZE);
        var rect = this.GetBoundingBox(out var textSize);
        var size = rect.GetSize().ToVector2i(Constants.GRIDSIZE);
        var realSize = size.ToVector2(Constants.GRIDSIZE);

        PrimitiveRenderer.RenderRectangle(rect, Vector2.Zero, 0f, ColorF.White);

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

        uint val = this._values.Reverse().GetAsUInt();
        var digits = (int)Math.Ceiling(this._data.DataBits / 4f);

        for (int i = 0; i < digits; i++)
        {
            var digit = (val >> ((digits - i - 1) * 4)) & 0xFu;
            var segmentsForValue = segments[digit];

            foreach (var segment in segmentsForValue)
            {
                var start = segment.Item1 * Constants.GRIDSIZE + pos + (i * new Vector2(3 * Constants.GRIDSIZE, 0));
                var end = segment.Item2 * Constants.GRIDSIZE + pos + (i * new Vector2(3 * Constants.GRIDSIZE, 0));

                PrimitiveRenderer.RenderLine(start, end, 3, ColorF.Red);
            }
        }
    }
}