using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using LogiX.Extensions;
using LogiX.Graphics;
using LogiX.Model.Simulation;

namespace LogiX.Model.NodeModel;

public class RectangleVisualNodePart(Vector2 position, Vector2 size, ColorF color, Func<IEnumerable<PinEvent>> onRightClicked = null, bool renderSelected = false) : INodePart
{
    private readonly Vector2 _position = position;
    private readonly Vector2 _size = size;
    private readonly ColorF _color = color;

    public Vector2 GetSize(int gridSize) => _size * gridSize;

    public bool IntersectsWith(Vector2i nodePosition, Vector2 nodeMiddleRelativeToOrigin, int nodeRotation, int gridSize, RectangleF rectangle)
    {
        var rotatedPosition = (_position.RotateAround(nodeMiddleRelativeToOrigin, nodeRotation * MathF.PI / 2f) + (Vector2)nodePosition) * gridSize;
        var rotatedSize = _size.RotateAround(Vector2i.Zero, nodeRotation * MathF.PI / 2f) * gridSize;
        var rectangleOfPart = new RectangleF(rotatedPosition.X, rotatedPosition.Y, rotatedSize.X, rotatedSize.Y).EnsurePositiveSize();

        return rectangleOfPart.IntersectsWith(rectangle);
    }

    public bool IsPointInside(Vector2i nodePosition, Vector2 nodeMiddleRelativeToOrigin, int nodeRotation, int gridSize, Vector2 point)
    {
        var rotatedPosition = (_position.RotateAround(nodeMiddleRelativeToOrigin, nodeRotation * MathF.PI / 2f) + (Vector2)nodePosition) * gridSize;
        var rotatedSize = _size.RotateAround(Vector2i.Zero, nodeRotation * MathF.PI / 2f) * gridSize;
        var rectangleOfPart = new RectangleF(rotatedPosition.X, rotatedPosition.Y, rotatedSize.X, rotatedSize.Y).EnsurePositiveSize();

        return rectangleOfPart.Contains(new PointF(point.X, point.Y));
    }

    public IEnumerable<PinEvent> NodePartRightClicked() => onRightClicked?.Invoke() ?? Enumerable.Empty<PinEvent>();

    public void Render(
        IRenderer renderer,
        Vector2i nodePosition,
        Vector2 nodeMiddleRelativeToOrigin,
        int nodeRotation,
        int gridSize,
        float opacity)
    {
        var rotatedPosition = (_position.RotateAround(nodeMiddleRelativeToOrigin, nodeRotation * MathF.PI / 2f) + (Vector2)nodePosition) * gridSize;
        var rotatedSize = _size.RotateAround(Vector2i.Zero, nodeRotation * MathF.PI / 2f) * gridSize;
        var rectangleOfPart = new RectangleF(rotatedPosition.X, rotatedPosition.Y, rotatedSize.X, rotatedSize.Y).EnsurePositiveSize();

        renderer.Primitives.RenderRectangle(rectangleOfPart, Vector2.Zero, 0f, _color * opacity);
    }

    public void RenderAsSelected(
        IRenderer renderer,
        Vector2i nodePosition,
        Vector2 nodeMiddleRelativeToOrigin,
        int nodeRotation,
        int gridSize,
        float opacity)
    {
        if (!renderSelected)
            return;

        var rotatedPosition = (_position.RotateAround(nodeMiddleRelativeToOrigin, nodeRotation * MathF.PI / 2f) + (Vector2)nodePosition) * gridSize;
        var rotatedSize = _size.RotateAround(Vector2i.Zero, nodeRotation * MathF.PI / 2f) * gridSize;
        var rectangleOfPart = new RectangleF(rotatedPosition.X, rotatedPosition.Y, rotatedSize.X, rotatedSize.Y).EnsurePositiveSize();

        renderer.Primitives.RenderRectangle(rectangleOfPart.InflateRect(2, 2), Vector2.Zero, 0f, ColorF.Orange * opacity);
    }
}
