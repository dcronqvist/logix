using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using DotGLFW;
using LogiX.Extensions;
using LogiX.Graphics;
using LogiX.Input;
using LogiX.Model.Simulation;

namespace LogiX.Model.NodeModel;

public interface INodePart
{
    void Render(
        IRenderer renderer,
        Vector2i nodePosition,
        Vector2 nodeMiddleRelativeToOrigin,
        int nodeRotation,
        int gridSize,
        float opacity
    );

    void RenderAsSelected(
        IRenderer renderer,
        Vector2i nodePosition,
        Vector2 nodeMiddleRelativeToOrigin,
        int nodeRotation,
        int gridSize,
        float opacity
    );

    bool IntersectsWith(Vector2i nodePosition, Vector2 nodeMiddleRelativeToOrigin, int nodeRotation, int gridSize, RectangleF rectangle);
    bool IsPointInside(Vector2i nodePosition, Vector2 nodeMiddleRelativeToOrigin, int nodeRotation, int gridSize, Vector2 point);

    IEnumerable<PinEvent> NodePartRightClicked();

    Vector2 GetSize(int gridSize);
}

public class TextVisualNodePart(string text, Vector2 position, float scale) : INodePart
{
    private readonly string _text = text;
    private readonly Vector2 _position = position;
    private readonly float _scale = scale;

    public Vector2 GetSize(int gridSize) => Vector2.Zero;

    public bool IntersectsWith(Vector2i nodePosition, Vector2 nodeMiddleRelativeToOrigin, int nodeRotation, int gridSize, RectangleF rectangle) => false;

    public bool IsPointInside(Vector2i nodePosition, Vector2 nodeMiddleRelativeToOrigin, int nodeRotation, int gridSize, Vector2 point) => false;

    public IEnumerable<PinEvent> NodePartRightClicked() => Enumerable.Empty<PinEvent>();

    public void Render(
        IRenderer renderer,
        Vector2i nodePosition,
        Vector2 nodeMiddleRelativeToOrigin,
        int nodeRotation,
        int gridSize,
        float opacity)
    {
        float angle = nodeRotation * MathF.PI / 2f;
        var textSize = (renderer.Text.PeekFont().MeasureString(_text) * _scale).RotateAround(Vector2.Zero, angle);
        var textPosition = ((_position.RotateAround(nodeMiddleRelativeToOrigin, nodeRotation * MathF.PI / 2f) + (Vector2)nodePosition) * gridSize) - (textSize / 2f);
        renderer.Text.AddText(_text, textPosition, _scale, angle, ColorF.Black * opacity);
    }

    public void RenderAsSelected(IRenderer renderer, Vector2i nodePosition, Vector2 nodeMiddleRelativeToOrigin, int nodeRotation, int gridSize, float opacity) { }
}

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
