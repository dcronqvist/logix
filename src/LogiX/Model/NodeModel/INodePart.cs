using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using LogiX.Graphics;
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
