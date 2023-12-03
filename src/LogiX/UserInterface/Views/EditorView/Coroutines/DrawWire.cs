using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using DotGLFW;
using LogiX.Graphics;
using LogiX.Model.Circuits;
using LogiX.Model.Commands;
using LogiX.UserInterface.Coroutines;

namespace LogiX.UserInterface.Views.EditorView;

public partial class EditorView
{
    public IEnumerator DrawWire()
    {
        var startPosition = GetGridAlignedMousePositionInWorkspace();
        var cornerPosition = startPosition;
        var direction = Vector2.Zero;

        yield return CoroutineHelpers.Render(renderer =>
        {
            var endPosition = GetGridAlignedMousePositionInWorkspace();

            if ((endPosition - startPosition).Length() >= 1f && direction == Vector2.Zero)
            {
                direction = (endPosition - startPosition).Normalize();
                direction = GetClosestPoint(direction, Vector2.UnitX, Vector2.UnitY, -Vector2.UnitX, -Vector2.UnitY);
            }

            if (MathF.Abs(direction.X) == 1f)
            {
                cornerPosition = new Vector2i(endPosition.X, startPosition.Y);
                if (direction.X == 1f && endPosition.X <= startPosition.X)
                    direction = Vector2i.Zero;
                else if (direction.X == -1f && endPosition.X >= startPosition.X)
                    direction = Vector2i.Zero;
            }
            else
            {
                cornerPosition = new Vector2i(startPosition.X, endPosition.Y);
                if (direction.Y == 1f && endPosition.Y <= startPosition.Y)
                    direction = Vector2i.Zero;
                else if (direction.Y == -1f && endPosition.Y >= startPosition.Y)
                    direction = Vector2i.Zero;
            }

            renderer.Primitives.RenderLine(
                startPosition * _gridSize,
                cornerPosition * _gridSize,
                2f,
                ColorF.Orange
            );

            renderer.Primitives.RenderLine(
                cornerPosition * _gridSize,
                endPosition * _gridSize,
                2f,
                ColorF.Orange
            );
        });

        bool wasCancelled = false;
        yield return CoroutineHelpers.WaitUntilCancelable(
            predicate: () => _mouse.IsMouseButtonReleased(MouseButton.Left),
            whileWaiting: () => { },
            shouldCancel: () => _keyboard.IsKeyPressed(Keys.Escape),
            finishedSuccessfully: (finished) => wasCancelled = !finished);

        if (wasCancelled)
            yield break;

        var endPosition = GetGridAlignedMousePositionInWorkspace();

        if (startPosition == endPosition)
            yield break;

        IssueCommand(new LambdaCommand<(List<SignalSegment>, List<SignalSegment>)>("Draw wire", () =>
        {
            List<SignalSegment> segmentsAdded = [];
            List<SignalSegment> segmentsRemoved = [];

            if (_currentlySimulatedCircuitViewModel.PointExistsOnAnySegment(startPosition, out var onSegment))
            {
                segmentsRemoved.Add(onSegment);
                _currentlySimulatedCircuitViewModel.RemoveSignalSegment(onSegment.Start, onSegment.End);

                _currentlySimulatedCircuitViewModel.AddSignalSegment(onSegment.Start, startPosition);
                _currentlySimulatedCircuitViewModel.AddSignalSegment(startPosition, onSegment.End);
                segmentsAdded.Add(new SignalSegment(onSegment.Start, startPosition));
                segmentsAdded.Add(new SignalSegment(startPosition, onSegment.End));
            }

            if (_currentlySimulatedCircuitViewModel.PointExistsOnAnySegment(endPosition, out onSegment))
            {
                segmentsRemoved.Add(onSegment);
                _currentlySimulatedCircuitViewModel.RemoveSignalSegment(onSegment.Start, onSegment.End);

                _currentlySimulatedCircuitViewModel.AddSignalSegment(onSegment.Start, endPosition);
                _currentlySimulatedCircuitViewModel.AddSignalSegment(endPosition, onSegment.End);
                segmentsAdded.Add(new SignalSegment(onSegment.Start, endPosition));
                segmentsAdded.Add(new SignalSegment(endPosition, onSegment.End));
            }

            if (cornerPosition != startPosition && cornerPosition != endPosition)
            {
                _currentlySimulatedCircuitViewModel.AddSignalSegment(startPosition, cornerPosition);
                _currentlySimulatedCircuitViewModel.AddSignalSegment(cornerPosition, endPosition);
                segmentsAdded.Add(new SignalSegment(startPosition, cornerPosition));
                segmentsAdded.Add(new SignalSegment(cornerPosition, endPosition));
            }
            else
            {
                _currentlySimulatedCircuitViewModel.AddSignalSegment(startPosition, endPosition);
                segmentsAdded.Add(new SignalSegment(startPosition, endPosition));
            }

            return (segmentsAdded, segmentsRemoved);
        },
        (addedAndRemoved) =>
        {
            foreach (var segment in addedAndRemoved.Item1)
                _currentlySimulatedCircuitViewModel.RemoveSignalSegment(segment.Start, segment.End);

            foreach (var segment in addedAndRemoved.Item2)
                _currentlySimulatedCircuitViewModel.AddSignalSegment(segment.Start, segment.End);
        }));
    }

    private static Vector2 GetClosestPoint(Vector2 point, params Vector2[] points)
    {
        var closestPoint = points[0];
        float closestDistance = (point - closestPoint).LengthSquared();

        foreach (var p in points)
        {
            float distance = (point - p).LengthSquared();

            if (distance < closestDistance)
            {
                closestPoint = p;
                closestDistance = distance;
            }
        }

        return closestPoint;
    }
}
