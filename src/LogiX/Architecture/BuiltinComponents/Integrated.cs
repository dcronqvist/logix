using System.Drawing;
using System.Numerics;
using ImGuiNET;
using LogiX.Architecture.Serialization;
using LogiX.Content.Scripting;
using LogiX.Graphics;
using LogiX.Graphics.UI;
using LogiX.Rendering;

namespace LogiX.Architecture.BuiltinComponents;

public class IntegratedData : INodeDescriptionData
{
    public Guid CircuitID { get; set; }

    [NodeDescriptionProperty("Chip Color", HelpTooltip = "The color of the chip in the circuit editor")]
    public ColorF Color { get; set; }

    public INodeDescriptionData GetDefault()
    {
        return new IntegratedData()
        {
            CircuitID = Guid.Empty,
            Color = ColorF.White,
        };
    }

    public static INodeDescriptionData GetDefaultData()
    {
        return new IntegratedData()
        {
            CircuitID = Guid.Empty,
            Color = ColorF.White,
        };
    }
}

[ScriptType("INTEGRATED"), NodeInfo("INTEGRATED", "BuiltIn", null, true)]
public class Integrated : Node<IntegratedData>
{
    private IntegratedData _data;
    private string _name;

    private List<Node> _evaluateNodes = new();
    private Simulation _simulation;
    private Dictionary<string, Vector2i> _pinPositions;
    private Dictionary<string, Guid> _pinToNodeID;
    public override void Register(Scheduler scheduler)
    {
        if (_simulation is null)
        {
            var circ = NodeDescription.GetIntegratedProjectCircuitByID(this._data.CircuitID);
            var simulation = Simulation.FromCircuit(circ);
            var circPins = circ.GetAllPins().Select(p => (p.Data as PinData, p)).ToList();

            var pinToNodeID = circ.GetAllPins().ToDictionary(x => (x.Data as PinData).Label, x => x.ID);
            var pinPositions = new Dictionary<string, Vector2i>();

            foreach (var (ident, id) in pinToNodeID)
            {
                var node = simulation.GetNodeFromID(id);
                var internalPins = simulation.Scheduler.GetPinCollectionForNode(node);
                var pinPos = node.GetPinPosition(internalPins, "Q");
                pinPositions.Add(ident, pinPos);
            }

            foreach (var (ident, id) in pinToNodeID)
            {
                var node = simulation.GetNodeFromID(id);
                simulation.RemoveNode(node);
            }

            this._simulation = simulation;
            this._pinPositions = pinPositions;
            this._pinToNodeID = pinToNodeID;
        }

        var nodes = this._simulation.Scheduler.Nodes;
        foreach (var node in nodes)
        {
            scheduler.AddNode(node, false);
        }

        foreach (var (ident, pos) in this._pinPositions)
        {
            if (this._simulation.TryGetWireAtPos(pos, out var wire))
            {
                var pinsConnectedToNode = this._simulation.GetPinsConnectedToWire(wire);

                foreach (var (n, i) in pinsConnectedToNode)
                {
                    if (!this._evaluateNodes.Contains(n))
                        _evaluateNodes.Add(n);

                    scheduler.AddConnection(this, ident, n, i, false);
                }
            }
        }

        foreach (var (n1, p1, n2, p2) in this._simulation.Scheduler.NodePinConnections)
        {
            scheduler.AddConnection(n1, p1, n2, p2, false);
        }
    }

    public override IEnumerable<(ObservableValue, LogicValue[], int)> Evaluate(PinCollection pins)
    {
        yield break;
    }

    public static NodeDescription CreateDescriptionFromCircuit(string circuitName, Circuit circuit)
    {
        var data = IntegratedData.GetDefaultData() as IntegratedData;
        data.CircuitID = circuit.ID;

        var integrated = new Integrated();
        integrated.Initialize(data);
        return integrated.GetDescriptionOfInstance();
    }

    public override Vector2i GetSize()
    {
        var pinLayout = this.GetPinLayout();
        var topPins = pinLayout[ComponentSide.TOP];
        var bottomPins = pinLayout[ComponentSide.BOTTOM];
        var leftPins = pinLayout[ComponentSide.LEFT];
        var rightPins = pinLayout[ComponentSide.RIGHT];

        var leftPinsMaxLabel = leftPins.Count > 0 ? leftPins.Select(x => x.Label.Length).Max() : 0;
        var rightPinsMaxLabel = rightPins.Count > 0 ? rightPins.Select(x => x.Label.Length).Max() : 0;

        var font = Utilities.GetFont("core.font.default", 8);
        var scale = 1f;
        var measure = font.MeasureString("_", scale).X;
        var leftMax = (int)((leftPinsMaxLabel * measure).CeilToMultipleOf(Constants.GRIDSIZE) / Constants.GRIDSIZE);
        var rightMax = (int)((rightPinsMaxLabel * measure).CeilToMultipleOf(Constants.GRIDSIZE) / Constants.GRIDSIZE);

        var widthMax = Math.Max(leftMax, rightMax);

        var width = Math.Max(widthMax * 2, 4);
        var height = Math.Max(leftPins.Count, rightPins.Count) + 1;

        return new Vector2i(width, height);
    }

    public override INodeDescriptionData GetNodeData()
    {
        return this._data;
    }

    private Dictionary<ComponentSide, List<PinData>> GetPinLayout()
    {
        var data = this._data;
        var circ = NodeDescription.GetIntegratedProjectCircuitByID(this._data.CircuitID);
        var pinDescs = circ.GetAllPins().Select(x => (x.Data as PinData, x.Position)).ToList();

        var pinLayout = new Dictionary<ComponentSide, List<PinData>>();

        // Top
        var topPins = pinDescs.Where(x => (x.Item1.Side == ComponentSide.TOP)).OrderBy(x => x.Position.X).Select(x => x.Item1).ToList();
        var buttomPins = pinDescs.Where(x => (x.Item1.Side == ComponentSide.BOTTOM)).OrderBy(x => x.Position.X).Select(x => x.Item1).ToList();
        var leftPins = pinDescs.Where(x => (x.Item1.Side == ComponentSide.LEFT)).OrderBy(x => x.Position.Y).Select(x => x.Item1).ToList();
        var rightPins = pinDescs.Where(x => (x.Item1.Side == ComponentSide.RIGHT)).OrderBy(x => x.Position.Y).Select(x => x.Item1).ToList();

        pinLayout.Add(ComponentSide.TOP, topPins);
        pinLayout.Add(ComponentSide.BOTTOM, buttomPins);
        pinLayout.Add(ComponentSide.LEFT, leftPins);
        pinLayout.Add(ComponentSide.RIGHT, rightPins);

        return pinLayout;
    }

    public override IEnumerable<PinConfig> GetPinConfiguration()
    {
        var pinLayout = this.GetPinLayout();
        var topPins = pinLayout[ComponentSide.TOP];
        var bottomPins = pinLayout[ComponentSide.BOTTOM];
        var leftPins = pinLayout[ComponentSide.LEFT];
        var rightPins = pinLayout[ComponentSide.RIGHT];

        var getLabel = (PinData pin, int i) => pin.Label == "" ? $"{i}" : pin.Label;

        int i = 1;
        foreach (var pin in topPins)
        {
            yield return new PinConfig(getLabel(pin, i), pin.Bits, true, new Vector2i(i++, 0));
        }

        i = 1;
        foreach (var pin in bottomPins)
        {
            yield return new PinConfig(getLabel(pin, i), pin.Bits, true, new Vector2i(i++, this.GetSize().Y));
        }

        i = 1;
        foreach (var pin in leftPins)
        {
            yield return new PinConfig(getLabel(pin, i), pin.Bits, true, new Vector2i(0, i++));
        }

        i = 1;
        foreach (var pin in rightPins)
        {
            yield return new PinConfig(getLabel(pin, i), pin.Bits, true, new Vector2i(this.GetSize().X, i++));
        }
    }

    public override void Initialize(IntegratedData data)
    {
        this._data = data;
        var circ = NodeDescription.GetIntegratedProjectCircuitByID(this._data.CircuitID);
        this._name = circ.Name;
    }

    public override bool IsNodeInRect(RectangleF rect)
    {
        var size = this.GetSizeRotated();
        var width = size.X;
        var height = size.Y;

        return this.Position.ToVector2(Constants.GRIDSIZE).CreateRect(new Vector2(width, height) * Constants.GRIDSIZE).IntersectsWith(rect);
    }

    public override void RenderSelected(Camera2D camera)
    {
        var size = this.GetSizeRotated();
        var width = size.X;
        var height = size.Y;

        PrimitiveRenderer.RenderRectangle(this.Position.ToVector2(Constants.GRIDSIZE).CreateRect(new Vector2(width, height) * Constants.GRIDSIZE).Inflate(2), Vector2.Zero, 0f, Constants.COLOR_SELECTED);
    }

    protected override bool Interact(Scheduler scheduler, PinCollection pins, Camera2D camera)
    {
        return false;
    }

    protected override IEnumerable<(ObservableValue, LogicValue[])> Prepare(PinCollection pins)
    {
        return Enumerable.Empty<(ObservableValue, LogicValue[])>();
    }

    public override void Render(PinCollection pins, Camera2D camera)
    {
        var size = this.GetSizeRotated();
        var width = size.X;
        var height = size.Y;

        var rect = this.Position.ToVector2(Constants.GRIDSIZE).CreateRect(new Vector2(width, height) * Constants.GRIDSIZE);

        PrimitiveRenderer.RenderRectangleWithBorder(rect, Vector2.Zero, 0f, 1, this._data.Color, this._data.Color.Darken(0.5f));

        var pos = this.Position;

        if (pins is not null)
        {
            foreach (var (side, ps) in this.GetPinLayout())
            {
                foreach (var p in ps)
                {
                    var ident = p.Label;
                    var (config, value) = pins[ident];

                    var iFont = Utilities.GetFont("core.font.default", 8);
                    var iScale = 0.65f;
                    var iMeasure = iFont.MeasureString(ident, iScale);
                    var pinPos = this.GetPinPosition(pins, ident);
                    var color = value is null ? ColorF.Black : value.Read().GetValueColor();
                    PrimitiveRenderer.RenderCircle(pinPos.ToVector2(Constants.GRIDSIZE), Constants.PIN_RADIUS, 0f, color, 1f);

                    var onside = side.ApplyRotation(this.Rotation);

                    var offset = (onside) switch
                    {
                        ComponentSide.LEFT => new Vector2(2, -iMeasure.Y / 2f),
                        ComponentSide.RIGHT => new Vector2(-iMeasure.X - 2, -iMeasure.Y / 2f),
                        ComponentSide.TOP => new Vector2(iMeasure.Y / 2f, 2),
                        ComponentSide.BOTTOM => new Vector2(iMeasure.Y / 2f, -iMeasure.X - 2),
                        _ => new Vector2(0, 0)
                    };

                    var rot = (onside) switch
                    {
                        ComponentSide.LEFT => 0f,
                        ComponentSide.RIGHT => 0f,
                        ComponentSide.TOP => MathF.PI / 2f,
                        ComponentSide.BOTTOM => MathF.PI / 2f,
                        _ => 0f
                    };

                    TextRenderer.RenderText(iFont, ident, pinPos.ToVector2(Constants.GRIDSIZE) + offset, iScale, rot, ColorF.Black);
                }
            }
        }

        var name = this._name;
        var font = Utilities.GetFont("core.font.default", 8);
        var realPos = this.Position.ToVector2(Constants.GRIDSIZE) + this.GetMiddleOffset();
        var scale = 1f;
        var measure = font.MeasureString(name, scale);
        var textPos = this.Rotation switch
        {
            0 => realPos + new Vector2(measure.Y / 2f, -measure.X / 2f),
            1 => realPos + new Vector2(-measure.X / 2f, -measure.Y / 2f),
            2 => realPos + new Vector2(measure.Y / 2f, -measure.X / 2f),
            3 => realPos + new Vector2(-measure.X / 2f, -measure.Y / 2f),
            _ => realPos
        };
        var rotation = (MathF.PI / 2f) * ((this.Rotation + 1) % 2);

        TextRenderer.RenderText(font, name, textPos, scale, rotation, ColorF.Black);
    }
}