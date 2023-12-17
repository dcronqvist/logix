using System.Collections;
using System.Linq;
using LogiX.Model.Commands;

namespace LogiX.UserInterface.Views.EditorView;

public partial class EditorView
{
    public IEnumerator RotateSelectedNodes(int rotation)
    {
        var selectedNodes = _currentlySimulatedCircuitViewModel.GetSelectedNodes().ToArray();

        IssueCommand(new LambdaCommand("Rotate selected nodes", () =>
        {
            foreach (var nodeID in selectedNodes)
            {
                _currentlySimulatedCircuitViewModel.RotateNode(nodeID, rotation);
            }
        }, () =>
        {
            foreach (var nodeID in selectedNodes)
            {
                _currentlySimulatedCircuitViewModel.RotateNode(nodeID, -rotation);
            }
        }));

        yield break;
    }
}
