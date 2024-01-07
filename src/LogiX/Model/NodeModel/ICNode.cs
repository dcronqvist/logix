using System.Collections.Generic;
using System.Numerics;
using LogiX.Graphics;
using LogiX.Model.Projects;
using LogiX.Model.Simulation;

namespace LogiX.Model.NodeModel;

public class ICNodeState : INodeState
{
    public string CircuitDefinitionID { get; set; } = string.Empty;

    public INodeState Clone() => new ICNodeState() { CircuitDefinitionID = CircuitDefinitionID };
    public bool HasEditorGUI() => false;
    public bool SubmitStateEditorGUI() => false;
}

public class ICNode(IProjectService projectService) : INode<ICNodeState>
{
    public IEnumerable<PinConfig> GetPinConfigs(ICNodeState state)
    {
        var currentProject = projectService.GetCurrentProject();
        var circuitDef = currentProject.GetProjectCircuitTree().RecursivelyGetFileContents(state.CircuitDefinitionID);

        yield return new PinConfig("a", 1, true, new Vector2i(0, 0), PinSide.Left, true);
    }

    public IEnumerable<PinEvent> Evaluate(ICNodeState state, IPinCollection pins)
    {
        yield break;
    }

    public void ConfigureUIHandlers(ICNodeState state, INodeUIHandlerConfigurer configurer)
    {

    }

    public ICNodeState CreateInitialState() => new();

    public Vector2 GetMiddleRelativeToOrigin(ICNodeState state) => new(1, 1);

    public string GetNodeName() => "IC";

    public IEnumerable<INodePart> GetParts(ICNodeState state, IPinCollection pins)
    {
        yield return new RectangleVisualNodePart(new Vector2(0, 0), new Vector2(2, 2), ColorF.White);
    }

    public IEnumerable<PinEvent> Initialize(ICNodeState state)
    {
        yield break;
    }
}
