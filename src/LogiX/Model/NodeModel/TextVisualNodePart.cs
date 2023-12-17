using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using LogiX.Extensions;
using LogiX.Graphics;
using LogiX.Model.Simulation;

namespace LogiX.Model.NodeModel;

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
