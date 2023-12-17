using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DotGLFW;
using LogiX.Extensions;
using LogiX.Graphics;
using LogiX.Graphics.Cameras;
using LogiX.Graphics.Text;
using LogiX.Input;
using LogiX.Model.Simulation;

namespace LogiX.Model.NodeModel;

public interface INodePresenter
{
    IEnumerable<PinEvent> Render(INode node, INodeState nodeState, IPinCollection pinCollection, Vector2i position, int rotation, int gridSize, ICamera2D camera, float globalAlpha, bool isSelected);
}

public class SimplePresentation(
    IRenderer renderer,
    Func<Vector2> getMousePositionInWorkspace,
    IMouse<MouseButton> mouse,
    IFont font) : INodePresenter
{
    private readonly IRenderer _renderer = renderer;
    private readonly IFont _font = font;

    public IEnumerable<PinEvent> Render(INode node, INodeState nodeState, IPinCollection pinCollection, Vector2i position, int rotation, int gridSize, ICamera2D camera, float globalAlpha, bool isSelected)
    {
        var nodeParts = node.GetParts(nodeState, pinCollection);
        var pinConfigs = node.GetPinConfigs(nodeState);

        var pinEvents = RenderVisualNodeParts(nodeParts, position, node.GetMiddleRelativeToOrigin(nodeState), rotation, gridSize, globalAlpha, isSelected).ToArray();
        RenderIOPositions(pinConfigs, pinCollection, position, node.GetMiddleRelativeToOrigin(nodeState), rotation, gridSize, camera, globalAlpha, isSelected);
        return pinEvents;
    }

    private IEnumerable<PinEvent> RenderVisualNodeParts(IEnumerable<INodePart> parts, Vector2i position, Vector2 middleOfNode, int rotation, int gridSize, float globalAlpha, bool isSelected)
    {
        var mousePos = getMousePositionInWorkspace();

        foreach (var part in parts)
        {
            if (isSelected)
                part.RenderAsSelected(_renderer, position, middleOfNode, rotation, gridSize, globalAlpha);

            if (mouse.IsMouseButtonPressed(MouseButton.Right) && part.IsPointInside(position, middleOfNode, rotation, gridSize, mousePos))
            {
                foreach (var @event in part.NodePartRightClicked())
                    yield return @event;
            }

            part.Render(_renderer, position, middleOfNode, rotation, gridSize, globalAlpha);
        }
    }

    private void RenderIOPositions(IEnumerable<PinConfig> pinConfigs, IPinCollection pinCollection, Vector2i position, Vector2 middleOfNode, int rotation, int gridSize, ICamera2D camera, float globalAlpha, bool isSelected)
    {
        foreach (var pinConfig in pinConfigs)
        {
            var pinValues = pinCollection.Read(pinConfig.ID);

            var ioColor = pinValues.GetValueColor();

            float ioRadius = gridSize * 0.2f;
            float innerRadius = ioRadius * 0.9f;

            var rotatedPosition = (((Vector2)pinConfig.Position).RotateAround(middleOfNode, rotation * MathF.PI / 2f) + (Vector2)position) * gridSize;

            _renderer.Primitives.RenderCircle(new Vector2(rotatedPosition.X, rotatedPosition.Y), ioRadius, 0f, ColorF.Black * globalAlpha, 1f, 12);
            _renderer.Primitives.RenderCircle(new Vector2(rotatedPosition.X, rotatedPosition.Y), innerRadius, 0f, ioColor * globalAlpha, 1f, 12);

            if (!pinConfig.DisplayPinName)
                continue;

            Action addTextForPinFunction = pinConfig.Side switch
            {
                PinSide.Left => () => AddIOTextForLeftSidePin(pinConfig.ID, rotatedPosition, rotation, gridSize, ioRadius, false, ColorF.Black * globalAlpha),
                PinSide.Right => () => AddIOTextForRightSidePin(pinConfig.ID, rotatedPosition, rotation, gridSize, ioRadius, false, ColorF.Black * globalAlpha),
                PinSide.Bottom => () => AddIOTextForBottomSidePin(pinConfig.ID, rotatedPosition, rotation, gridSize, ioRadius, false, ColorF.Black * globalAlpha),
                PinSide.Top => () => AddIOTextForTopSidePin(pinConfig.ID, rotatedPosition, rotation, gridSize, ioRadius, false, ColorF.Black * globalAlpha),
                _ => () => { }
            };
            addTextForPinFunction.Invoke();
        }
    }

    private void AddIOTextForLeftSidePin(string pinID, Vector2 finalIOPosition, int rotation, int gridSize, float ioRadius, bool addBar, ColorF color)
    {
        var textDirection = Vector2i.RotateAround(new Vector2i(1, 0), Vector2i.Zero, rotation);
        float angleToMiddle = MathF.Atan2(textDirection.Y, textDirection.X);
        float textScale = 6f * gridSize / 20f;
        var textSize = _font.MeasureString(pinID) * textScale;

        var textPosition =
            finalIOPosition +
            (new Vector2(MathF.Cos(angleToMiddle), MathF.Sin(angleToMiddle)) * (ioRadius * 1.2f)) +
            new Vector2(MathF.Sin(angleToMiddle) * textSize.Y / 2f, MathF.Cos(angleToMiddle) * -textSize.Y / 2f);

        _renderer.Text.AddText(_font, pinID, textPosition, textScale, rotation * MathF.PI / 2f, color);

        if (addBar)
        {
            _renderer.Primitives.RenderLine(textPosition, textPosition + (new Vector2(MathF.Cos(angleToMiddle), MathF.Sin(angleToMiddle)) * (textSize.X - 0.8f)), 0.7f, ColorF.Black);
        }
    }

    private void AddIOTextForRightSidePin(string pinID, Vector2 finalIOPosition, int rotation, int gridSize, float ioRadius, bool addBar, ColorF color)
    {
        var textDirection = Vector2i.RotateAround(new Vector2i(-1, 0), Vector2i.Zero, rotation);
        float angleToMiddle = MathF.Atan2(textDirection.Y, textDirection.X);
        float textScale = 6f * gridSize / 20f;
        var textSize = _font.MeasureString(pinID) * textScale;

        var textPosition =
            finalIOPosition +
            (new Vector2(MathF.Cos(angleToMiddle), MathF.Sin(angleToMiddle)) * ((ioRadius * 1.2f) + textSize.X)) +
            new Vector2(MathF.Sin(angleToMiddle) * -textSize.Y / 2f, MathF.Cos(angleToMiddle) * textSize.Y / 2f);

        _renderer.Text.AddText(_font, pinID, textPosition, textScale, rotation * MathF.PI / 2f, color);

        if (addBar)
        {
            _renderer.Primitives.RenderLine(textPosition, textPosition - (new Vector2(MathF.Cos(angleToMiddle), MathF.Sin(angleToMiddle)) * (textSize.X - 0.8f)), 0.7f, ColorF.Black);
        }
    }

    private void AddIOTextForBottomSidePin(string pinID, Vector2 finalIOPosition, int rotation, int gridSize, float ioRadius, bool addBar, ColorF color)
    {
        var textDirection = Vector2i.RotateAround(new Vector2i(0, -1), Vector2i.Zero, rotation);
        float angleToMiddle = MathF.Atan2(textDirection.Y, textDirection.X);
        float textScale = 6f * gridSize / 20f;
        var textSize = _font.MeasureString(pinID) * textScale;

        var textPosition =
            finalIOPosition +
            (new Vector2(MathF.Cos(angleToMiddle), MathF.Sin(angleToMiddle)) * (ioRadius * 1.2f)) +
            new Vector2(MathF.Sin(angleToMiddle) * textSize.Y / 2f, MathF.Cos(angleToMiddle) * -textSize.Y / 2f);

        _renderer.Text.AddText(_font, pinID, textPosition, textScale, (rotation * MathF.PI / 2f) + (MathF.PI / 2f), color);

        if (addBar)
        {
            _renderer.Primitives.RenderLine(textPosition, textPosition + (new Vector2(MathF.Cos(angleToMiddle), MathF.Sin(angleToMiddle)) * (textSize.X - 0.8f)), 0.7f, ColorF.Black);
        }
    }

    private void AddIOTextForTopSidePin(string pinID, Vector2 finalIOPosition, int rotation, int gridSize, float ioRadius, bool addBar, ColorF color)
    {
        var textDirection = Vector2i.RotateAround(new Vector2i(0, 1), Vector2i.Zero, rotation);
        float angleToMiddle = MathF.Atan2(textDirection.Y, textDirection.X);
        float textScale = 6f * gridSize / 20f;
        var textSize = _font.MeasureString(pinID) * textScale;

        var textPosition =
            finalIOPosition +
            (new Vector2(MathF.Cos(angleToMiddle), MathF.Sin(angleToMiddle)) * ((ioRadius * 1.2f) + textSize.X)) +
            new Vector2(MathF.Sin(angleToMiddle) * -textSize.Y / 2f, MathF.Cos(angleToMiddle) * textSize.Y / 2f);

        _renderer.Text.AddText(_font, pinID, textPosition, textScale, (rotation * MathF.PI / 2f) + (MathF.PI / 2f), color);

        if (addBar)
        {
            _renderer.Primitives.RenderLine(textPosition, textPosition - (new Vector2(MathF.Cos(angleToMiddle), MathF.Sin(angleToMiddle)) * (textSize.X - 0.8f)), 0.7f, ColorF.Black);
        }
    }
}

