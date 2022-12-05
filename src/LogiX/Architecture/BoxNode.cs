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

        var size = this.GetSize();

        var rect = pos.ToVector2(Constants.GRIDSIZE).CreateRect(size.ToVector2(Constants.GRIDSIZE));

        PrimitiveRenderer.RenderRectangle(rect.Inflate(2), Vector2.Zero, 0f, Constants.COLOR_SELECTED);
    }

    public override void Render(PinCollection pins, Camera2D camera)
    {
        var pos = this.Position;
        var size = this.GetSize();

        var rect = pos.ToVector2(Constants.GRIDSIZE).CreateRect(size.ToVector2(Constants.GRIDSIZE));

        PrimitiveRenderer.RenderRectangleWithBorder(rect, Vector2.Zero, 0f, 1, ColorF.White, ColorF.Black);

        var root = pos.ToVector2(Constants.GRIDSIZE);
        var sizeReal = size.ToVector2(Constants.GRIDSIZE);
        var font = Utilities.GetFont("core.font.default", 8);
        var scale = this.TextScale;
        var measure = font.MeasureString(this.Text, scale);

        var rot = this.Rotation switch
        {
            1 => MathF.PI / 2f,
            3 => MathF.PI / 2f,
            _ => 0
        };

        var offset = this.Rotation switch
        {
            1 => new Vector2(measure.Y / 2f, -measure.X / 2f),
            3 => new Vector2(measure.Y / 2f, -measure.X / 2f),
            _ => -measure / 2f
        };

        TextRenderer.RenderText(Utilities.GetFont("core.font.default", 8), this.Text, root + sizeReal / 2f + offset, scale, rot, ColorF.Black);

        base.Render(pins, camera);
    }
}