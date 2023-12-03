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
using LogiX.UserInterfaceContext;

namespace LogiX.UserInterface.Views.EditorView;

public partial class EditorView
{
    private IEnumerator PlaceNode(INode node)
    {
        // Render the node while following the mouse
        yield return CoroutineHelpers.Render((renderer, _, _) =>
        {
            var mousePos = GetGridAlignedMousePositionInWorkspace();
            var nodeMiddle = node.GetMiddleRelativeToOrigin(node.CreateInitialState());
            var roundedMiddle = new Vector2i((int)Math.Round(nodeMiddle.X), (int)Math.Round(nodeMiddle.Y));

            _presentation.Render(node, node.CreateInitialState(), new PinCollection(), mousePos - roundedMiddle, 0, _gridSize, _camera, 0.3f, false);
        });

        // Wait until we release the mouse button (or press escape to cancel)
        bool wasCancelled = false;
        yield return CoroutineHelpers.WaitUntilCancelable(
            predicate: () => _mouse.IsMouseButtonReleased(MouseButton.Left),
            whileWaiting: () => { },
            shouldCancel: () => _keyboard.IsKeyPressed(Keys.Escape),
            finishedSuccessfully: (finished) => wasCancelled = !finished);

        // If we cancelled, we don't want to place the node
        if (wasCancelled)
            yield break;

        // Get the position of the mouse in the grid
        var mousePos = GetGridAlignedMousePositionInWorkspace();
        var nodeMiddle = node.GetMiddleRelativeToOrigin(node.CreateInitialState());
        var roundedMiddle = new Vector2i((int)Math.Round(nodeMiddle.X), (int)Math.Round(nodeMiddle.Y));

        var newNodeID = Guid.NewGuid();

        // Add the node to the circuit
        IssueCommand(new LambdaCommand<Guid>("Add node", () =>
        {
            return _currentlySimulatedCircuitViewModel.AddNode(
                newNodeID,
                node,
                mousePos - roundedMiddle,
                0,
                node.CreateInitialState()
            );
        }, addedNodeID => _currentlySimulatedCircuitViewModel.RemoveNode(addedNodeID)));

        _currentlySimulatedCircuitViewModel.ClearSelectedNodes();
        _currentlySimulatedCircuitViewModel.SelectNode(newNodeID);

        yield break;
    }

    public IEnumerator WireSegmentContextMenu(SignalSegment segment, Guid signalID)
    {
        var clickedPoint = GetGridAlignedMousePositionInWorkspace();

        ICommand commandToRun = null;
        yield return CoroutineHelpers.ShowContextMenu(() =>
        {
            if (ImGui.MenuItem("Split segment"))
            {
                commandToRun = new LambdaCommand("Split wire segment", () =>
                {
                    var (firstSegment, secondSegment) = SplitSegment(segment, clickedPoint);
                    _currentlySimulatedCircuitViewModel.RemoveSignalSegment(segment.Start, segment.End);
                    _currentlySimulatedCircuitViewModel.AddSignalSegment(firstSegment.Start, firstSegment.End);
                    _currentlySimulatedCircuitViewModel.AddSignalSegment(secondSegment.Start, secondSegment.End);
                }, () =>
                {
                    _currentlySimulatedCircuitViewModel.RemoveSignalSegment(segment.Start, segment.End);
                    _currentlySimulatedCircuitViewModel.AddSignalSegment(segment.Start, segment.End);
                });
            }
            if (ImGui.MenuItem("Delete segment"))
            {
                commandToRun = new LambdaCommand("Delete wire segment", () =>
                {
                    _currentlySimulatedCircuitViewModel.RemoveSignalSegment(segment.Start, segment.End);
                }, () =>
                {
                    _currentlySimulatedCircuitViewModel.AddSignalSegment(segment.Start, segment.End);
                });
            }
            if (ImGui.MenuItem("Delete wire"))
            {
                var signalSegmentsInWire = _currentlySimulatedCircuitViewModel.GetSignalSegmentsForSignal(signalID);
                commandToRun = new LambdaCommand("Delete wire", () =>
                {
                    foreach (var segmentToDelete in signalSegmentsInWire)
                        _currentlySimulatedCircuitViewModel.RemoveSignalSegment(segmentToDelete.Start, segmentToDelete.End);
                }, () =>
                {
                    foreach (var segmentToAdd in signalSegmentsInWire)
                        _currentlySimulatedCircuitViewModel.AddSignalSegment(segmentToAdd.Start, segmentToAdd.End);
                });
            }
        });

        if (commandToRun != null)
            IssueCommand(commandToRun);
    }

    public IEnumerator WireSegmentPointContextMenu(Vector2i point, IReadOnlyCollection<SignalSegment> adjacentSegments)
    {
        ICommand commandToRun = null;
        yield return CoroutineHelpers.ShowContextMenu(() =>
        {
            bool allOnSameAxis = adjacentSegments.All(s => s.Start.X == s.End.X) || adjacentSegments.All(s => s.Start.Y == s.End.Y);
            bool canMergeAdjacent = adjacentSegments.Count == 2 && allOnSameAxis;

            if (ImGui.MenuItem("Merge adjacent", canMergeAdjacent))
            {
                var firstSegment = adjacentSegments.First();
                var lastSegment = adjacentSegments.Last();
                var combinedSegment = CombineSegments(adjacentSegments.First(), adjacentSegments.Last());
                commandToRun = new LambdaCommand("Merge adjacent wire segments", () =>
                {
                    _currentlySimulatedCircuitViewModel.RemoveSignalSegment(firstSegment.Start, firstSegment.End);
                    _currentlySimulatedCircuitViewModel.RemoveSignalSegment(lastSegment.Start, lastSegment.End);
                    _currentlySimulatedCircuitViewModel.AddSignalSegment(combinedSegment.Start, combinedSegment.End);
                }, () =>
                {
                    _currentlySimulatedCircuitViewModel.RemoveSignalSegment(combinedSegment.Start, combinedSegment.End);
                    _currentlySimulatedCircuitViewModel.AddSignalSegment(firstSegment.Start, firstSegment.End);
                    _currentlySimulatedCircuitViewModel.AddSignalSegment(lastSegment.Start, lastSegment.End);
                });
            }
            if (!canMergeAdjacent)
            {
                ImGui.SameLine();
                ImGuiExt.ImGuiHelp("Can only merge 2 adjacent segments that are on the same axis");
            }
        });

        if (commandToRun != null)
            IssueCommand(commandToRun);
    }

    private SignalSegment CombineSegments(SignalSegment first, SignalSegment second)
    {
        if (first.Start == second.Start)
            return new SignalSegment(first.End, second.End);
        else if (first.Start == second.End)
            return new SignalSegment(first.End, second.Start);
        else if (first.End == second.Start)
            return new SignalSegment(first.Start, second.End);
        else if (first.End == second.End)
            return new SignalSegment(first.Start, second.Start);
        else
            throw new InvalidOperationException("Cannot combine segments that don't share a point");
    }

    private (SignalSegment, SignalSegment) SplitSegment(SignalSegment segment, Vector2i splitPoint)
    {
        if (segment.Start == splitPoint || segment.End == splitPoint)
            throw new InvalidOperationException("Cannot split a segment at one of its endpoints");

        return (new SignalSegment(segment.Start, splitPoint), new SignalSegment(splitPoint, segment.End));
    }
}
