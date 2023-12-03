using System.Collections;
using System.Linq;
using LogiX.Model.Commands;

namespace LogiX.UserInterface.Views.EditorView;

public partial class EditorView
{
    public IEnumerator DeleteSelection()
    {
        if (_currentlySimulatedCircuitViewModel.GetSelectedNodes().Count == 0 && _currentlySimulatedCircuitViewModel.GetSelectedSignalSegments().Count == 0)
            yield break;

        var selectedNodes = _currentlySimulatedCircuitViewModel.GetSelectedNodes().ToArray();
        var nodes = selectedNodes.Select(nodeID => _currentlySimulatedCircuitDefinition.Locked(circDef => (nodeID, circDef.GetNodes()[nodeID]))).ToDictionary(x => x.nodeID, x => x.Item2);
        var nodeStates = selectedNodes.Select(nodeID => _currentlySimulatedCircuitDefinition.Locked(circDef => (nodeID, circDef.GetNodeState(nodeID)))).ToDictionary(x => x.nodeID, x => x.Item2);
        var nodeRotations = selectedNodes.Select(nodeID => _currentlySimulatedCircuitDefinition.Locked(circDef => (nodeID, circDef.GetNodeRotation(nodeID)))).ToDictionary(x => x.nodeID, x => x.Item2);
        var nodePositions = selectedNodes.Select(nodeID => _currentlySimulatedCircuitDefinition.Locked(circDef => (nodeID, circDef.GetNodePosition(nodeID)))).ToDictionary(x => x.nodeID, x => x.Item2);

        var selectedSignalSegments = _currentlySimulatedCircuitViewModel.GetSelectedSignalSegments().ToArray();

        IssueCommand(new LambdaCommand("Delete selection", () =>
        {
            foreach (var nodeID in selectedNodes)
            {
                _currentlySimulatedCircuitViewModel.RemoveNode(nodeID);
            }

            foreach (var segment in selectedSignalSegments)
            {
                _currentlySimulatedCircuitViewModel.RemoveSignalSegment(segment.Start, segment.End);
            }
        }, () =>
        {
            foreach (var nodeID in selectedNodes)
            {
                _currentlySimulatedCircuitViewModel.AddNode(nodeID, nodes[nodeID], nodePositions[nodeID], nodeRotations[nodeID], nodeStates[nodeID]);
            }

            foreach (var segment in selectedSignalSegments)
            {
                _currentlySimulatedCircuitViewModel.AddSignalSegment(segment.Start, segment.End);
            }
        }));

        yield break;
    }
}
