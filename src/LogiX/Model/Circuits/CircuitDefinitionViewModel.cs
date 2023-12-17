using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using LogiX.Extensions;
using LogiX.Graphics;
using LogiX.Graphics.Cameras;
using LogiX.Graphics.Text;
using LogiX.Model.Commands;
using LogiX.Model.NodeModel;
using LogiX.Model.Simulation;

namespace LogiX.Model.Circuits;

public class CircuitDefinitionViewModel
{
    private readonly IThreadSafe<ICircuitDefinition> _circuitDefinition;
    private readonly IThreadSafe<ISimulator> _simulator;

    private readonly List<SignalSegment> _signalSegments = [];
    private readonly List<Guid> _selectedNodes = [];
    private readonly List<SignalSegment> _selectedSignalSegments = [];

    public CircuitDefinitionViewModel(
        IThreadSafe<ICircuitDefinition> circuitDefinition,
        IThreadSafe<ISimulator> simulator)
    {
        _circuitDefinition = circuitDefinition;
        _simulator = simulator;

        _signalSegments = _circuitDefinition.Locked(circDef => circDef.GetSignals().SelectMany(x => circDef.GetSignalSegments(x.Key)).ToList());
        UpdateSignals();
    }

    public Guid AddNode(Guid nodeID, INode node, Vector2i position, int rotation, INodeState nodeState) => _circuitDefinition.Locked(circDef =>
    {
        var currentNodes = circDef.GetNodes().ToDictionary();
        currentNodes.Add(nodeID, node);
        circDef.SetNodes(currentNodes);
        circDef.SetNodePosition(nodeID, position);
        circDef.SetNodeRotation(nodeID, rotation);
        circDef.SetNodeState(nodeID, nodeState);

        UpdateSignals();
        return nodeID;
    });

    public Guid AddNode<TState>(Guid nodeID, INode<TState> node, Vector2i position, int rotation, TState nodeState) where TState : INodeState => _circuitDefinition.Locked(circDef =>
    {
        var currentNodes = circDef.GetNodes().ToDictionary();
        currentNodes.Add(nodeID, node);
        circDef.SetNodes(currentNodes);
        circDef.SetNodePosition(nodeID, position);
        circDef.SetNodeRotation(nodeID, rotation);
        circDef.SetNodeState(nodeID, nodeState);

        UpdateSignals();
        return nodeID;
    });

    public void RemoveNode(Guid nodeID) => _circuitDefinition.Locked(circDef =>
    {
        var currentNodes = circDef.GetNodes().ToDictionary();
        currentNodes.Remove(nodeID);
        circDef.SetNodes(currentNodes);
        _selectedNodes.Remove(nodeID);

        UpdateSignals();
    });

    public void ClearNodes() => _circuitDefinition.Locked(circDef =>
    {
        circDef.SetNodes(new Dictionary<Guid, INode>());
        _selectedNodes.Clear();
        UpdateSignals();
    });

    public void AddSignalSegment(Vector2i start, Vector2i end)
    {
        if (start.X == end.X && start.Y == end.Y)
            throw new ArgumentException("Start and end cannot be the same");

        if (start.X != end.X && start.Y != end.Y)
            throw new ArgumentException("Start and end must be on the same axis");

        _signalSegments.Add(new SignalSegment(start, end));
        UpdateSignals();
    }

    public void RemoveSignalSegment(Vector2i start, Vector2i end)
    {
        if (start.X != end.X && start.Y != end.Y)
            throw new ArgumentException("Start and end must be on the same axis");

        _signalSegments.Remove(new SignalSegment(start, end));
        _selectedSignalSegments.Remove(new SignalSegment(start, end));
        UpdateSignals();
    }

    public bool PointExistsOnAnySegment(Vector2i point, out SignalSegment onSegment)
    {
        foreach (var segment in _signalSegments)
        {
            int minX = Math.Min(segment.Start.X, segment.End.X);
            int maxX = Math.Max(segment.Start.X, segment.End.X);
            int minY = Math.Min(segment.Start.Y, segment.End.Y);
            int maxY = Math.Max(segment.Start.Y, segment.End.Y);

            if ((segment.Start.X == segment.End.X && segment.Start.X == point.X && point.Y > minY && point.Y < maxY) ||
                   (segment.Start.Y == segment.End.Y && segment.Start.Y == point.Y && point.X > minX && point.X < maxX))
            {
                onSegment = segment;
                return true;
            }
        }

        onSegment = default;
        return false;
    }

    public IReadOnlyCollection<SignalSegment> GetSignalSegmentsForSignal(Guid signalID) => _circuitDefinition.Locked(circDef => circDef.GetSignalSegments(signalID));

    public void ClearSignals()
    {
        _signalSegments.Clear();

        _circuitDefinition.Locked(circDef =>
        {
            circDef.ClearSignalSegmentsForAllSignals();
            circDef.SetSignals(new Dictionary<Guid, CircuitSignalDefinition>());

            circDef.NotifyObservers();
        });
    }

    public void SelectNode(Guid nodeID)
    {
        if (!_selectedNodes.Contains(nodeID))
            _selectedNodes.Add(nodeID);
    }

    public void DeselectNode(Guid nodeID) => _selectedNodes.Remove(nodeID);

    public void SelectAllNodes() => _circuitDefinition.Locked(circDef =>
    {
        _selectedNodes.Clear();
        _selectedNodes.AddRange(circDef.GetNodes().Keys);
    });

    public void ClearSelectedNodes() => _selectedNodes.Clear();

    public void SelectAllNodesThatIntersectRectangle(RectangleF rectangle)
    {
        _selectedNodes.Clear();

        var pinCollections = _simulator.Locked(sim => sim.GetNodes().Select(node => (node.Key, sim.GetPinCollectionForNode(node.Key))).ToDictionary());

        _circuitDefinition.Locked(circDef =>
        {
            foreach (var (nodeID, node) in circDef.GetNodes())
            {
                var nodePosition = circDef.GetNodePosition(nodeID);
                int nodeRotation = circDef.GetNodeRotation(nodeID);
                var nodeState = circDef.GetNodeState(nodeID);
                var nodeMiddle = node.GetMiddleRelativeToOrigin(nodeState);
                var nodePinCollection = pinCollections[nodeID];
                var nodeParts = node.GetParts(nodeState, nodePinCollection);

                if (nodeParts.Any(part => part.IntersectsWith(nodePosition, nodeMiddle, nodeRotation, 20, rectangle)))
                    _selectedNodes.Add(nodeID);
            }
        });
    }

    public void SelectSignalSegment(SignalSegment segment)
    {
        if (!_selectedSignalSegments.Contains(segment))
            _selectedSignalSegments.Add(segment);
    }

    public void DeselectSignalSegment(SignalSegment segment) => _selectedSignalSegments.Remove(segment);

    public void SelectAllSignalSegments() => _circuitDefinition.Locked(circDef =>
    {
        _selectedSignalSegments.Clear();
        _selectedSignalSegments.AddRange(circDef.GetSignals().SelectMany(x => circDef.GetSignalSegments(x.Key)));
    });

    public void ClearSelectedSignalSegments() => _selectedSignalSegments.Clear();

    public IReadOnlyCollection<SignalSegment> GetSelectedSignalSegments() => _selectedSignalSegments;

    public void SelectAllSignalSegmentsThatIntersectRectangle(RectangleF rectangle)
    {
        _selectedSignalSegments.Clear();

        _circuitDefinition.Locked(circDef =>
        {
            foreach (var (signalID, signal) in circDef.GetSignals())
            {
                var signalSegments = circDef.GetSignalSegments(signalID);

                foreach (var segment in signalSegments)
                {
                    var wireRectangle = GetWireSegmentRectangle(segment.Start * 20, segment.End * 20);

                    if (wireRectangle.IntersectsWith(rectangle))
                        _selectedSignalSegments.Add(segment);
                }
            }
        });
    }

    public bool IsPositionOnAnyNode(Vector2 position, out Guid nodeID)
    {
        nodeID = Guid.Empty;

        var pinCollections = _simulator.Locked(sim => sim.GetNodes().Select(node => (node.Key, sim.GetPinCollectionForNode(node.Key))).ToDictionary());

        nodeID = _circuitDefinition.Locked(circDef =>
        {
            foreach (var (id, node) in circDef.GetNodes())
            {
                var nodePosition = circDef.GetNodePosition(id);
                int nodeRotation = circDef.GetNodeRotation(id);
                var nodeState = circDef.GetNodeState(id);
                var nodeMiddle = node.GetMiddleRelativeToOrigin(nodeState);
                var nodePinCollection = pinCollections[id];
                var nodeParts = node.GetParts(nodeState, nodePinCollection);

                if (nodeParts.Any(part => part.IsPointInside(nodePosition, nodeMiddle, nodeRotation, 20, position)))
                {
                    return id;
                }
            }

            return Guid.Empty;
        });

        return nodeID != Guid.Empty;
    }

    public bool IsNodeSelected(Guid nodeID) => _selectedNodes.Contains(nodeID);

    public bool IsPositionOnAnyPin(Vector2 position, out NodePin nodePin)
    {
        nodePin = default;

        var pinCollections = _simulator.Locked(sim => sim.GetNodes().Select(node => (node.Key, sim.GetPinCollectionForNode(node.Key))).ToDictionary());

        nodePin = _circuitDefinition.Locked(circDef =>
        {
            foreach (var (id, node) in circDef.GetNodes())
            {
                var nodePosition = circDef.GetNodePosition(id);
                int nodeRotation = circDef.GetNodeRotation(id);
                var nodeState = circDef.GetNodeState(id);
                var nodeMiddle = node.GetMiddleRelativeToOrigin(nodeState);
                var nodePinCollection = pinCollections[id];

                var pinConfigs = node.GetPinConfigs(nodeState);

                foreach (var pinConfig in pinConfigs)
                {
                    var rotatedPosition = (((Vector2)pinConfig.Position).RotateAround(nodeMiddle, nodeRotation * MathF.PI / 2f) + (Vector2)nodePosition) * 20;

                    if (Vector2.Distance(rotatedPosition, position) < (0.2f * 20))
                    {
                        return new NodePin(id, pinConfig.ID);
                    }
                }
            }

            return null;
        });

        return nodePin is not null;
    }

    public IReadOnlyCollection<Guid> GetSelectedNodes() => _selectedNodes;

    public void MoveNode(Guid nodeID, Vector2i moveOffset) => _circuitDefinition.Locked(circDef =>
    {
        var nodePosition = circDef.GetNodePosition(nodeID);
        circDef.SetNodePosition(nodeID, nodePosition + moveOffset);

        UpdateSignals();
    });

    public void RotateNode(Guid nodeID, int rotation) => _circuitDefinition.Locked(circDef =>
    {
        int nodeRotation = circDef.GetNodeRotation(nodeID);
        circDef.SetNodeRotation(nodeID, (nodeRotation + rotation) % 4);

        UpdateSignals();
    });

    public bool IsPositionOnAnyWireSegment(Vector2 position, out SignalSegment wireSegment, out Guid signalID)
    {
        var pinCollections = _simulator.Locked(sim => sim.GetNodes().Select(node => (node.Key, sim.GetPinCollectionForNode(node.Key))).ToDictionary());

        (wireSegment, signalID) = _circuitDefinition.Locked(circDef =>
        {
            foreach (var (signalID, signal) in circDef.GetSignals())
            {
                var signalSegments = circDef.GetSignalSegments(signalID);

                foreach (var segment in signalSegments)
                {
                    var wireRectangle = GetWireSegmentRectangle(segment.Start * 20, segment.End * 20);

                    if (wireRectangle.Contains(position))
                    {
                        return (segment, signalID);
                    }
                }
            }

            return (default, default);
        });

        return signalID != default;
    }

    public bool IsPositionOnAnyWireSegmentPoint(Vector2 position, out Vector2i wireSegmentPoint, out IReadOnlyCollection<SignalSegment> adjSegments)
    {
        wireSegmentPoint = default;
        adjSegments = default;

        (wireSegmentPoint, adjSegments) = _circuitDefinition.Locked(circDef =>
        {
            foreach (var (signalID, signal) in circDef.GetSignals())
            {
                var signalSegments = circDef.GetSignalSegments(signalID);

                foreach (var segment in signalSegments)
                {
                    IEnumerable<Vector2i> points = [segment.Start, segment.End];

                    foreach (var point in points)
                    {
                        if ((position - ((Vector2)point * 20)).Length() < 0.2f * 20f)
                        {
                            return (point, GetAdjacentSignalSegmentsToSegmentPoint(point, signalSegments));
                        }
                    }
                }
            }

            return (default, default);
        });

        return wireSegmentPoint != default;
    }

    private static IReadOnlyCollection<SignalSegment> GetAdjacentSignalSegmentsToSegmentPoint(Vector2i segmentPoint, IReadOnlyCollection<SignalSegment> signalSegments)
    {
        var adjacentSegments = new List<SignalSegment>();

        foreach (var segment in signalSegments)
        {
            if (segment.Start == segmentPoint || segment.End == segmentPoint)
                adjacentSegments.Add(segment);
        }

        return adjacentSegments;
    }

    private void UpdateSignals()
    {
        var connectedComponents = GetConnectedComponents();
        var pinPositions = GetPinPositions();

        var signals = new Dictionary<Guid, CircuitSignalDefinition>();
        var signalSegments = new Dictionary<Guid, IReadOnlyCollection<SignalSegment>>();

        _circuitDefinition.Locked(circDef => { circDef.ClearSignalSegmentsForAllSignals(); circDef.NotifyObservers(); });

        foreach (var connectedComponentPositions in connectedComponents)
        {
            var nodePinsConnectedToSignal = new List<NodePin>();

            foreach (var (segmentStart, segmentEnd) in connectedComponentPositions)
            {
                if (!pinPositions.Any(x => x.positionInCircuit == segmentStart || x.positionInCircuit == segmentEnd))
                    continue;

                var (nodeIDStart, pinIDStart, positionInCircuitStart) = pinPositions.FirstOrDefault(x => x.positionInCircuit == segmentStart, (Guid.Empty, "", default));
                if (nodeIDStart != Guid.Empty)
                    nodePinsConnectedToSignal.Add(new NodePin(nodeIDStart, pinIDStart));
                var (nodeIDEnd, pinIDEnd, positionInCircuitEnd) = pinPositions.FirstOrDefault(x => x.positionInCircuit == segmentEnd, (Guid.Empty, "", default));
                if (nodeIDEnd != Guid.Empty)
                    nodePinsConnectedToSignal.Add(new NodePin(nodeIDEnd, pinIDEnd));
            }

            var signalID = Guid.NewGuid();
            signals.Add(signalID, new CircuitSignalDefinition(nodePinsConnectedToSignal));

            signalSegments.Add(signalID, connectedComponentPositions.Select(x => new SignalSegment(x.Start, x.End)).ToList());
        }

        _circuitDefinition.Locked(circDef =>
        {
            circDef.SetSignals(signals);

            foreach (var (signalID, segments) in signalSegments)
                circDef.SetSignalSegments(signalID, segments);

            circDef.NotifyObservers();
        });
    }

    private static RectangleF GetWireSegmentRectangle(Vector2 lineStart, Vector2 lineEnd)
    {
        float wireWidth = 0.2f * 20;
        float wireHalfWidth = wireWidth / 2f;

        bool isHorizontal = lineStart.Y == lineEnd.Y;

        if (isHorizontal)
        {
            float minX = Math.Min(lineStart.X, lineEnd.X);
            float maxX = Math.Max(lineStart.X, lineEnd.X);
            float minY = Math.Min(lineStart.Y, lineEnd.Y);

            return new RectangleF(minX - wireHalfWidth, minY - wireHalfWidth, maxX - minX + wireWidth, wireWidth);
        }
        else
        {
            float minY = Math.Min(lineStart.Y, lineEnd.Y);
            float maxY = Math.Max(lineStart.Y, lineEnd.Y);
            float minX = Math.Min(lineStart.X, lineEnd.X);

            return new RectangleF(minX - wireHalfWidth, minY - wireHalfWidth, wireWidth, maxY - minY + wireWidth);
        }
    }

    // Returns a list of connected components, where each connected component is a list of signal segments
    private List<List<SignalSegment>> GetConnectedComponents()
    {
        var signalSegments = _signalSegments.ToList();
        var connectedComponents = new List<List<SignalSegment>>();

        while (signalSegments.Count > 0)
        {
            var connectedComponent = new List<SignalSegment>();
            var queue = new Queue<SignalSegment>();

            queue.Enqueue(signalSegments[0]);
            signalSegments.RemoveAt(0);

            while (queue.Count > 0)
            {
                var segment = queue.Dequeue();
                connectedComponent.Add(segment);

                var connectedSegments = signalSegments.Where(x => x.Start == segment.Start || x.Start == segment.End || x.End == segment.Start || x.End == segment.End).ToList();

                foreach (var connectedSegment in connectedSegments)
                {
                    queue.Enqueue(connectedSegment);
                    signalSegments.Remove(connectedSegment);
                }
            }

            connectedComponents.Add(connectedComponent);
        }

        return connectedComponents;
    }

    private List<(Guid nodeID, string pinID, Vector2i positionInCircuit)> GetPinPositions() => _circuitDefinition.Locked(circDef =>
    {
        var nodes = circDef.GetNodes();
        var listToReturn = new List<(Guid nodeID, string pinID, Vector2i positionInCircuit)>();

        foreach (var (nodeID, node) in nodes)
        {
            var nodeState = circDef.GetNodeState(nodeID);
            var nodePosition = circDef.GetNodePosition(nodeID);
            int nodeRotation = circDef.GetNodeRotation(nodeID);
            var middleOfNode = node.GetMiddleRelativeToOrigin(nodeState);

            foreach (var pinConfig in node.GetPinConfigs(nodeState))
            {
                var rotatedPosition = Vector2i.RotateAround(pinConfig.Position, middleOfNode, nodeRotation) + nodePosition;
                listToReturn.Add((nodeID, pinConfig.ID, rotatedPosition));
            }
        }

        return listToReturn;
    });

    public void Render(
        INodePresenter presentation,
        ShaderProgram primitivesShader,
        IFont font,
        IRenderer renderer,
        int gridSize,
        ICamera2D camera,
        SignalSegment highlightedWireSegment = null
    )
    {
        var signalIDToValuesAndError = _simulator.Locked(sim => sim.GetSignals().Select(x =>
        {
            var signalID = x.Key;
            var signal = x.Value;
            if (signal.TryGetBitWidth(out int bitWidth))
            {
                return (signalID, signal.GetValue(bitWidth), signal.HasError(bitWidth));
            }
            else
            {
                return (signalID, LogicValue.UNDEFINED.Repeat(1), true);
            }
        })).ToDictionary(x => x.signalID, x => (x.Item2, x.Item3));

        var nodeIDToPinCollection = _simulator.Locked(sim => sim.GetNodes().ToDictionary(x => x.Key, x => sim.GetPinCollectionForNode(x.Key)));
        var nodeIDToIsSelected = _circuitDefinition.Locked(circDef => circDef.GetNodes().ToDictionary(x => x.Key, x => _selectedNodes.Contains(x.Key)));

        var nodePinEvents = _circuitDefinition.Locked(circDef =>
        {
            List<(Guid nodeID, PinEvent pinEvent)> eventsToReturn = [];

            foreach (var (signalID, _) in circDef.GetSignals())
            {
                var signalSegments = circDef.GetSignalSegments(signalID);
                var (signalValue, signalError) = signalIDToValuesAndError[signalID];
                var signalColor = signalValue.GetValueColor();

                if (signalError)
                {
                    signalColor = ColorF.Red;
                }

                foreach (var segment in signalSegments)
                {
                    var segStart = segment.Start;
                    var segEnd = segment.End;

                    if (segment == highlightedWireSegment)
                    {
                        renderer.Primitives.RenderLine(segStart * gridSize, segEnd * gridSize, 8f, ColorF.Darken(ColorF.Orange, 1.5f));
                    }
                    if (_selectedSignalSegments.Contains(segment))
                    {
                        renderer.Primitives.RenderLine(segStart * gridSize, segEnd * gridSize, 6f, ColorF.Orange);
                    }

                    renderer.Primitives.RenderLine(segStart * gridSize, segEnd * gridSize, 3.5f, signalColor);
                    renderer.Primitives.RenderCircle(segStart * gridSize, 3.5f, 0f, signalColor, 1f, 12);
                    renderer.Primitives.RenderCircle(segEnd * gridSize, 3.5f, 0f, signalColor, 1f, 12);
                }
            }

            foreach (var nodeKVP in circDef.GetNodes())
            {
                var nodeID = nodeKVP.Key;
                var node = nodeKVP.Value;

                var nodePosition = circDef.GetNodePosition(nodeID);
                int nodeRotation = circDef.GetNodeRotation(nodeID);
                var nodeState = circDef.GetNodeState(nodeID);
                var pinCollection = nodeIDToPinCollection[nodeID];
                var pinEvents = presentation.Render(node, nodeState, pinCollection, nodePosition, nodeRotation, gridSize, camera, 1f, nodeIDToIsSelected[nodeID]).ToArray();

                eventsToReturn.AddRange(pinEvents.Select(x => (nodeID, x)));
            }

            return eventsToReturn;
        }, (ex) => throw ex);

        _simulator.Locked(sim => nodePinEvents.ToList().ForEach((tuple) => sim.EnqueueEvent(tuple.nodeID, tuple.pinEvent)));

        renderer.Primitives.FinalizeRender(primitivesShader, camera);
        renderer.Text.FinalizeRender(font, camera);
    }

    public void SubmitGUI(IInvoker commandInvoker)
    {
        if (_selectedNodes.Count != 1)
            return;

        var nodeID = _selectedNodes[0];

        _circuitDefinition.Locked(circDef =>
        {
            var nodeState = circDef.GetNodeState(nodeID);
            var node = circDef.GetNodes()[nodeID];
            if (!nodeState.HasEditorGUI())
                return;

            string nodeStateEditorTitle = $"Edit {node.GetNodeName()}";

            if (ImGui.Begin($"{nodeStateEditorTitle}###NODESTATEEDITOR"))
            {
                var stateBefore = nodeState.Clone();
                if (nodeState.SubmitStateEditorGUI())
                {
                    var stateAfter = nodeState.Clone();
                    commandInvoker.ExecuteCommand(new LambdaCommand("Edit node", () =>
                    {
                        circDef.SetNodeState(nodeID, stateAfter);
                        circDef.NotifyObservers();
                    }, () =>
                    {
                        circDef.SetNodeState(nodeID, stateBefore);
                        circDef.NotifyObservers();
                    }));
                }

                ImGui.End();
            }
        });
    }
}
