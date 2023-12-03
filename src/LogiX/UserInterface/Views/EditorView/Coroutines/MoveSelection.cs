using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DotGLFW;
using ImGuiNET;
using LogiX.Graphics;
using LogiX.Model.Circuits;
using LogiX.Model.Commands;
using LogiX.Model.NodeModel;
using LogiX.UserInterface.Coroutines;

namespace LogiX.UserInterface.Views.EditorView;

public partial class EditorView
{
    private IEnumerator MoveSelection()
    {
        // Get the initial positions of the mouse
        var selectedNodes = _currentlySimulatedCircuitViewModel.GetSelectedNodes().ToArray();
        var selectedSignalSegments = _currentlySimulatedCircuitViewModel.GetSelectedSignalSegments().ToArray();
        var initialMousePosGridAligned = GetGridAlignedMousePositionInWorkspace();

        yield return CoroutineHelpers.Render((renderer) =>
        {
            var tempEndPos = GetGridAlignedMousePositionInWorkspace();
            var tempMoveOffset = tempEndPos - initialMousePosGridAligned;

            foreach (var nodeID in selectedNodes)
            {
                var node = _currentlySimulatedCircuitDefinition.Locked(circDef => circDef.GetNodes()[nodeID]);
                var nodeMiddle = node.GetMiddleRelativeToOrigin(node.CreateInitialState());
                var nodePos = _currentlySimulatedCircuitDefinition.Locked(circDef => circDef.GetNodePosition(nodeID)) + tempMoveOffset;
                int nodeRotation = _currentlySimulatedCircuitDefinition.Locked(circDef => circDef.GetNodeRotation(nodeID));

                _presentation.Render(node, node.CreateInitialState(), new PinCollection(), nodePos, nodeRotation, _gridSize, _camera, 0.5f, false);
            }

            foreach (var segment in selectedSignalSegments)
            {
                var start = segment.Start + tempMoveOffset;
                var end = segment.End + tempMoveOffset;

                renderer.Primitives.RenderLine(start * 20, end * 20, 3.5f, ColorF.Orange * 0.5f);
                renderer.Primitives.RenderCircle(start * 20, 3.5f, 0f, ColorF.Orange * 0.5f, 1f, 12);
                renderer.Primitives.RenderCircle(end * 20, 3.5f, 0f, ColorF.Orange * 0.5f, 1f, 12);
            }
        });

        // Wait until we release the mouse button (or press escape to cancel)
        bool wasCancelled = false;
        yield return CoroutineHelpers.WaitUntilCancelable(
            predicate: () => _mouse.IsMouseButtonReleased(MouseButton.Left),
            whileWaiting: () => { },
            shouldCancel: () => _keyboard.IsKeyPressed(Keys.Escape),
            finishedSuccessfully: (finished) => wasCancelled = !finished);

        // If we cancelled, we don't want to move the nodes
        if (wasCancelled)
            yield break;

        // Get the position of the mouse in the grid
        var endMousePosGridAligned = GetGridAlignedMousePositionInWorkspace();
        var moveOffset = endMousePosGridAligned - initialMousePosGridAligned;

        if (moveOffset == Vector2i.Zero)
            yield break;

        // Move the nodes
        IssueCommand(new LambdaCommand("Move selected nodes", () =>
        {
            foreach (var node in selectedNodes)
            {
                _currentlySimulatedCircuitViewModel.MoveNode(node, moveOffset);
            }

            var currentSelection = _currentlySimulatedCircuitViewModel.GetSelectedSignalSegments().ToArray();
            _currentlySimulatedCircuitViewModel.ClearSelectedSignalSegments();
            foreach (var segment in selectedSignalSegments)
            {
                _currentlySimulatedCircuitViewModel.RemoveSignalSegment(segment.Start, segment.End);
                var movedSegment = new SignalSegment(new Vector2i(segment.Start.X + moveOffset.X, segment.Start.Y + moveOffset.Y), new Vector2i(segment.End.X + moveOffset.X, segment.End.Y + moveOffset.Y));
                _currentlySimulatedCircuitViewModel.AddSignalSegment(movedSegment.Start, movedSegment.End);

                if (currentSelection.Contains(segment))
                    _currentlySimulatedCircuitViewModel.SelectSignalSegment(movedSegment);
            }
        }, () =>
        {
            foreach (var node in selectedNodes)
            {
                _currentlySimulatedCircuitViewModel.MoveNode(node, -moveOffset);
            }

            foreach (var segment in selectedSignalSegments)
            {
                _currentlySimulatedCircuitViewModel.RemoveSignalSegment(new Vector2i(segment.Start.X + moveOffset.X, segment.Start.Y + moveOffset.Y), new Vector2i(segment.End.X + moveOffset.X, segment.End.Y + moveOffset.Y));
                _currentlySimulatedCircuitViewModel.AddSignalSegment(segment.Start, segment.End);
            }
        }));
    }
}
