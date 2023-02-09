using System.Drawing;
using System.Numerics;
using LogiX.Architecture.Serialization;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX.Architecture;

public abstract class BoxNode<TData> : Node<TData> where TData : INodeDescriptionData
{
    public abstract string Text { get; }
    public abstract float TextScale { get; }

    public override bool IsNodeInRect(RectangleF rect)
    {
        var middle = this.GetMiddleOffset();
        var doubleMiddle = (middle * 2);

        var pos = this.Position.ToVector2(Constants.GRIDSIZE);

        return pos.CreateRect(doubleMiddle).IntersectsWith(rect);
    }

    public override void RenderSelected(Camera2D camera)
    {
        var pos = this.Position;

        var size = this.GetSizeRotated();

        var rect = pos.ToVector2(Constants.GRIDSIZE).CreateRect(size.ToVector2(Constants.GRIDSIZE));

        PrimitiveRenderer.RenderRectangle(rect.Inflate(2), Vector2.Zero, 0f, Constants.COLOR_SELECTED);
    }

    public override void Render(PinCollection pins, Camera2D camera)
    {
        var pos = this.Position;
        var size = this.GetSizeRotated();

        var rect = pos.ToVector2(Constants.GRIDSIZE).CreateRect(size.ToVector2(Constants.GRIDSIZE));

        PrimitiveRenderer.RenderRectangleWithBorder(rect, Vector2.Zero, 0f, 1, ColorF.White, ColorF.Black);

        var root = pos.ToVector2(Constants.GRIDSIZE);
        var sizeReal = size.ToVector2(Constants.GRIDSIZE);
        var font = Constants.NODE_FONT_REAL;
        var scale = 0.17f;
        var measure = font.MeasureString(this.Text, scale);

        var rot = this.Rotation switch
        {
            1 => MathF.PI / 2f,
            3 => MathF.PI / 2f,
            _ => 0
        };

        var x = 0;
        var y = 0;
        var offset = this.Rotation switch
        {
            1 => new Vector2(measure.Y / 2f + y, -measure.X / 2f + x),
            3 => new Vector2(measure.Y / 2f + y, -measure.X / 2f + x),
            _ => -measure / 2f + new Vector2(x, y)
        };

        TextRenderer.RenderText(Constants.NODE_FONT_REAL, this.Text, root + sizeReal / 2f + offset, scale, rot, ColorF.Black, true, 0.38f, 0.12f, -1f, -1f);

        base.Render(pins, camera);
    }
}