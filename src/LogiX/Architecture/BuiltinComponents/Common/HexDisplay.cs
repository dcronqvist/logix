using System.Drawing;
using System.Numerics;
using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class HexDisplayData : INodeDescriptionData
{
    [NodeDescriptionProperty("Bits", IntMinValue = 1, IntMaxValue = 256)]
    public int DataBits { get; set; }

    [NodeDescriptionProperty("Segment Color")]
    public ColorF SegmentColor { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new HexDisplayData()
        {
            DataBits = 4,
            SegmentColor = ColorF.Red
        };
    }
}

[ScriptType("HEXDISPLAY"), NodeInfo("Hex Display", "Common", "logix_core:docs/components/hexdisplay.md")]
public class HexDisplay : BoxNode<HexDisplayData>
{
    public override string Text => "";
    public override float TextScale => 1f;

    private HexDisplayData _data;

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        var values = pins.Get("in").Read(this._data.DataBits);
        this._values = values.ToArray();

        return Enumerable.Empty<(ObservableValue, LogicValue[], int)>();
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        yield return new PinConfig("in", this._data.DataBits, true, new Vector2i(0, 1));
    }

    public override Vector2i GetSize()
    {
        return new Vector2i(3 * (int)Math.Ceiling(this._data.DataBits / 4f), 5);
    }

    public override Vector2i GetSizeRotated()
    {
        return this.GetSize();
    }

    private Dictionary<uint, (Vector2, Vector2)[]> _segments;
    private LogicValue[] _values;

    public override void Initialize(HexDisplayData data)
    {
        this._data = data;

        // RENDER SEGMENTS
        var top = (new Vector2(0.65f, 0.5f), new Vector2(2.35f, 0.5f));
        var bottom = (new Vector2(0.65f, 4.5f), new Vector2(2.35f, 4.5f));
        var leftTop = (new Vector2(0.5f, 0.65f), new Vector2(0.5f, 2.35f));
        var leftBottom = (new Vector2(0.5f, 2.65f), new Vector2(0.5f, 4.35f));
        var rightTop = (new Vector2(2.5f, 0.65f), new Vector2(2.5f, 2.35f));
        var rightBottom = (new Vector2(2.5f, 2.65f), new Vector2(2.5f, 4.35f));
        var middle = (new Vector2(0.65f, 2.5f), new Vector2(2.35f, 2.5f));

        this._segments = new Dictionary<uint, (Vector2, Vector2)[]>()
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

        this._values = LogicValue.Z.Multiple(this._data.DataBits);
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        return false; // No interaction
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        yield break; // No preparation needed
    }

    public override void Render(PinCollection pins, Camera2D camera)
    {
        base.Render(pins, camera);

        uint val = this._values.Reverse().GetAsUInt();
        var digits = (int)Math.Ceiling(this._data.DataBits / 4f);
        var pos = this.Position.ToVector2(Constants.GRIDSIZE);

        for (int i = 0; i < digits; i++)
        {
            var digit = (val >> ((digits - i - 1) * 4)) & 0xFu;
            var segmentsForValue = this._segments[digit];

            foreach (var segment in segmentsForValue)
            {
                var start = segment.Item1 * Constants.GRIDSIZE + pos + (i * new Vector2(3 * Constants.GRIDSIZE, 0));
                var end = segment.Item2 * Constants.GRIDSIZE + pos + (i * new Vector2(3 * Constants.GRIDSIZE, 0));

                PrimitiveRenderer.RenderLine(start, end, 3, this._data.SegmentColor);
            }
        }
    }
}