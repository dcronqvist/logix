using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Numerics;
using LogiX.Architecture;
using LogiX.Architecture.BuiltinComponents;
using LogiX.Architecture.Serialization;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX;

public class ValueCollection : List<(LogicValue[], IO, Component)>
{
    public bool AnyPushed()
    {
        return this.Count > 0;
    }

    public bool AllAgree(out LogicValue[] correctValues)
    {
        if (this.Count == 0)
        {
            correctValues = null;
            return false;
        }

        LogicValue[] agreedValues = this[0].Item1;

        for (int i = 1; i < this.Count; i++)
        {
            var arr = this[i].Item1;

            for (int j = 0; j < arr.Length; j++)
            {
                if (arr[j] != agreedValues[j])
                {
                    if (arr[j] == LogicValue.UNDEFINED)
                    {
                        agreedValues[j] = agreedValues[j];
                    }
                    else if (agreedValues[j] == LogicValue.UNDEFINED)
                    {
                        agreedValues[j] = arr[j];
                    }
                    else
                    {
                        correctValues = null;
                        return false;
                    }
                }
            }
        }

        correctValues = agreedValues;
        return true;
    }

    public bool AllSameWidth()
    {
        if (this.Count == 0)
        {
            return false;
        }

        int width = this[0].Item1.Length;

        foreach ((LogicValue[] pushedValues, _, _) in this)
        {
            if (pushedValues.Length != width)
            {
                return false;
            }
        }

        return true;
    }
}

public enum LogicValueRetrievalStatus
{
    SUCCESS,
    DIFF_WIDTH,
    NOTHING_PUSHED,
    DISAGREE,
}
public abstract class SimulationError
{
    public string Message { get; set; }

    public SimulationError(string message)
    {
        this.Message = message;
    }

    public abstract void Render(Camera2D cam);
}

public class ReadWrongAmountOfBitsError : SimulationError
{
    public Component Comp { get; set; }
    public Vector2i Pos { get; set; }
    public int Expected { get; set; }
    public int Actual { get; set; }

    public ReadWrongAmountOfBitsError(Component comp, Vector2i pos, int expected, int actual) : base($"DIFF_BITWIDTH")
    {
        this.Comp = comp;
        this.Pos = pos;
        this.Expected = expected;
        this.Actual = actual;
    }

    public override void Render(Camera2D cam)
    {
        var shader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        PrimitiveRenderer.RenderCircle(this.Pos.ToVector2(Constants.GRIDSIZE), 8, 0f, ColorF.Red);
        var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.text");
        var font = LogiX.ContentManager.GetContentItem<Font>("content_1.font.default");
        var measure = font.MeasureString(this.Message, 1f);
        TextRenderer.RenderText(tShader, font, this.Message, this.Pos.ToVector2(Constants.GRIDSIZE) - measure / 2f, 1f, ColorF.Black, cam);
    }
}
public class PushingDifferentValuesError : SimulationError
{
    public Wire Wire { get; set; }

    public PushingDifferentValuesError(Wire wire) : base($"DISAGREE")
    {
        this.Wire = wire;
    }

    public override void Render(Camera2D cam)
    {
        var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");
        var color = ColorF.Red;

        for (int i = 0; i < Wire.Segments.Count; i++)
        {
            var segment = Wire.Segments[i];
            var a = segment.Item1.ToVector2(Constants.GRIDSIZE);
            var b = segment.Item2.ToVector2(Constants.GRIDSIZE);

            PrimitiveRenderer.RenderLine(a, b, Constants.WIRE_WIDTH, color);
        }

        var segmentPoints = Wire.Segments.SelectMany(s => new Vector2i[] { s.Item1, s.Item2 }).Distinct().ToArray();

        foreach (var point in segmentPoints)
        {
            var worldPos = point.ToVector2(Constants.GRIDSIZE);
            //PrimitiveRenderer.RenderCircle(pShader, worldPos, Constants.WIRE_POINT_RADIUS, 0, color, cam);
            PrimitiveRenderer.RenderRectangle(new RectangleF(worldPos.X, worldPos.Y, 0, 0).Inflate(Constants.WIRE_WIDTH / 2f), Vector2.Zero, 0, color);
        }

        var firstSegment = Wire.Segments[0];
        var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.text");
        var font = LogiX.ContentManager.GetContentItem<Font>("content_1.font.default");
        var measure = font.MeasureString(this.Message, 0.5f);
        TextRenderer.RenderText(tShader, font, this.Message, Utilities.GetMiddleOfVec2(firstSegment.Item1.ToVector2(Constants.GRIDSIZE), firstSegment.Item2.ToVector2(Constants.GRIDSIZE)) - measure / 2f, 0.5f, ColorF.Black, cam);
    }
}

public class ICIsOldError : SimulationError
{
    public Integrated IC { get; set; }

    public ICIsOldError(Integrated ic) : base($"IC_IS_OLD")
    {
        this.IC = ic;
    }

    public override void Render(Camera2D cam)
    {
        var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.text");
        var font = LogiX.ContentManager.GetContentItem<Font>("content_1.font.default");
        var size = this.IC.GetBoundingBox(out _).GetSize();
        var pos = this.IC.Position.ToVector2(Constants.GRIDSIZE);

        var measure = font.MeasureString(this.Message, 1f);
        TextRenderer.RenderText(tShader, font, this.Message, pos + new Vector2(size.X / 2f, -15) - measure / 2f, 1f, ColorF.Red, cam);
    }
}

public class Simulation
{
    public List<Component> Components { get; private set; } = new();
    public Dictionary<Vector2i, ValueCollection> NewValues { get; set; } = new();
    public Dictionary<Vector2i, ValueCollection> CurrentValues { get; set; } = new();
    public List<Wire> Wires { get; private set; } = new();
    public List<Component> SelectedComponents { get; set; } = new();
    public List<(Vector2i, Vector2i)> SelectedWireSegments { get; set; } = new();
    public List<SimulationError> PreviousErrors { get; set; } = new();
    public List<SimulationError> Errors { get; set; } = new();
    public Dictionary<Vector2i, (Component, IO)> ComponentIOPositions { get; set; } = new();
    public Dictionary<Vector2i, Wire> WirePositions { get; set; } = new();

    public Simulation()
    {
    }

    public bool TryGetLogicValuesAtPosition(Vector2i position, int expectedWidth, [NotNullWhen(true)] out LogicValue[] values, [NotNullWhen(false)] out LogicValueRetrievalStatus status, out IO fromIO, out Component fromComp)
    {
        // Find all driven values at that position, if any.
        // If there are no driven values, return UNDEFINED.

        if (!CurrentValues.ContainsKey(position))
        {
            values = Enumerable.Repeat(LogicValue.UNDEFINED, expectedWidth).ToArray();
            status = LogicValueRetrievalStatus.NOTHING_PUSHED;
            fromIO = null;
            fromComp = null;
            return false;
        }

        var pushedValues = CurrentValues[position];

        if (!pushedValues.AnyPushed())
        {
            values = Enumerable.Repeat(LogicValue.UNDEFINED, expectedWidth).ToArray();
            status = LogicValueRetrievalStatus.NOTHING_PUSHED;
            fromIO = null;
            fromComp = null;
            return false;
        }

        if (pushedValues.AllAgree(out var agreedValues) && pushedValues.AllSameWidth())
        {
            if (agreedValues.Length == expectedWidth)
            {
                values = agreedValues;
                status = LogicValueRetrievalStatus.SUCCESS;
                fromIO = pushedValues[0].Item2;
                fromComp = pushedValues[0].Item3;
                return true;
            }
            else
            {
                values = agreedValues;
                status = LogicValueRetrievalStatus.DIFF_WIDTH;
                fromIO = null;
                fromComp = null;
                return false;
            }
        }

        if (!pushedValues.AllSameWidth())
        {
            values = Enumerable.Repeat(LogicValue.UNDEFINED, expectedWidth).ToArray();
            status = LogicValueRetrievalStatus.DIFF_WIDTH;
            fromIO = null;
            fromComp = null;
            return false;
        }

        if (!pushedValues.AllAgree(out _))
        {
            values = Enumerable.Repeat(LogicValue.UNDEFINED, expectedWidth).ToArray();
            status = LogicValueRetrievalStatus.DISAGREE;
            fromIO = null;
            fromComp = null;
            return false;
        }

        throw new Exception("This should never happen.");
    }

    public bool TryGetLogicValuesAtPosition(Vector2i position, [NotNullWhen(true)] out LogicValue[] values, [NotNullWhen(false)] out LogicValueRetrievalStatus status)
    {
        // Find all driven values at that position, if any.
        // If there are no driven values, return UNDEFINED.

        if (!CurrentValues.ContainsKey(position))
        {
            values = null;
            status = LogicValueRetrievalStatus.NOTHING_PUSHED;
            return false;
        }

        var pushedValues = CurrentValues[position];
        if (pushedValues.AllAgree(out var agreedValues) && pushedValues.AllSameWidth())
        {
            values = agreedValues;
            status = LogicValueRetrievalStatus.SUCCESS;
            return true;
        }

        if (!pushedValues.AllSameWidth())
        {
            values = null;
            status = LogicValueRetrievalStatus.DIFF_WIDTH;
            return false;
        }

        if (!pushedValues.AllAgree(out _))
        {
            values = null;
            status = LogicValueRetrievalStatus.DISAGREE;
            return false;
        }

        throw new Exception("This should never happen.");
    }

    public bool TryGetWireAtPos(Vector2i position, [NotNullWhen(true)] out Wire wire)
    {
        if (this.WirePositions.TryGetValue(position, out var w))
        {
            wire = w;
            return true;
        }

        foreach (Wire ww in Wires)
        {
            if (ww.GetPoints().Contains(position))
            {
                wire = ww;
                this.WirePositions[position] = ww;
                return true;
            }
        }

        wire = null;
        return false;
    }

    public bool TryGetWireSegmentAtPos(Vector2 worldPosition, out (Vector2i, Vector2i) edge, out Wire wire)
    {
        foreach (var w in this.Wires)
        {
            foreach (var seg in w.Segments)
            {
                var start = seg.Item1;
                var end = seg.Item2;
                var rect = Utilities.GetWireRectangle(start, end);

                if (rect.Contains(worldPosition))
                {
                    edge = seg;
                    wire = w;
                    return true;
                }
            }
        }

        edge = default;
        wire = null;
        return false;
    }

    public bool TryGetWireVertexAtPos(Vector2 worldPosition, out Vector2i pos, out Wire wire)
    {
        foreach (var w in this.Wires)
        {
            foreach (var seg in w.Segments)
            {
                var start = seg.Item1.ToVector2(Constants.GRIDSIZE);
                var end = seg.Item2.ToVector2(Constants.GRIDSIZE);

                if (Vector2.Distance(start, worldPosition) <= Constants.WIRE_POINT_RADIUS)
                {
                    wire = w;
                    pos = seg.Item1;
                    return true;
                }

                if (Vector2.Distance(end, worldPosition) <= Constants.WIRE_POINT_RADIUS)
                {
                    wire = w;
                    pos = seg.Item2;
                    return true;
                }
            }
        }

        wire = null;
        pos = default;
        return false;
    }

    public void PushValuesAt(Vector2i position, IO fromIO, Component fromComponent, params LogicValue[] values)
    {
        // Attempt to push values to this grid position.
        if (!NewValues.ContainsKey(position))
        {
            NewValues[position] = new ValueCollection();
        }

        if (this.TryGetWireAtPos(position, out var wire))
        {
            var positions = wire.GetPoints();
            foreach (var pos in positions)
            {
                if (this.TryGetIOFromPosition(pos, out var group, out var comp) || wire.HasEdgeVertexAt(pos))
                {
                    if (!NewValues.ContainsKey(pos))
                    {
                        NewValues[pos] = new ValueCollection();
                    }

                    NewValues[pos].Add((values, fromIO, fromComponent));

                    if (!NewValues[pos].AllAgree(out _))
                    {
                        this.AddError(new PushingDifferentValuesError(wire));
                    }
                }
            }
        }
        else
        {
            NewValues[position].Add((values, fromIO, fromComponent));
        }
    }

    public void AddError(SimulationError error)
    {
        this.Errors.Add(error);
    }

    public void AddComponent(Component component, Vector2i position)
    {
        component.Position = position;
        this.Components.Add(component);

        foreach (var io in component.IOs)
        {
            var ioPos = component.GetPositionForIO(io, out _);
            this.ComponentIOPositions.Add(ioPos, (component, io));
        }
    }

    public void RemoveComponent(Component component)
    {
        this.Components.Remove(component);
        if (this.SelectedComponents.Contains(component))
        {
            this.SelectedComponents.Remove(component);
        }
    }

    public void AddWire(Wire wire)
    {
        this.Wires.Add(wire);
    }

    public T[] GetComponentsOfType<T>()
    {
        return this.Components.OfType<T>().ToArray();
    }

    public void Tick(params Type[] noUpdate)
    {
        this.PreviousErrors = Errors;
        this.Errors.Clear();

        // Allow components to perform their logic
        foreach (Component component in Components)
        {
            if (!noUpdate.Contains(component.GetType()))
            {
                component.Update(this);
            }
        }

        // Swap pushed values
        CurrentValues = NewValues;
        NewValues = new Dictionary<Vector2i, ValueCollection>();
    }

    public void Interact(Camera2D cam)
    {
        foreach (var component in this.Components)
        {
            component.Interact(cam);
        }
    }

    public void Render(Camera2D cam)
    {
        // Render selected components
        foreach (Component component in SelectedComponents)
        {
            component.RenderSelected(cam);
        }

        // Allow wires to render themselves
        foreach (Wire wire in Wires)
        {
            wire.Render(this, cam);
        }

        //Allow components to render themselves
        foreach (Component component in Components)
        {
            component.Render(cam);
        }

        // Render errors
        foreach (var error in this.Errors)
        {
            error.Render(cam);
        }
    }

    public Circuit GetCircuitInSimulation(string name)
    {
        return new Circuit(name, this.Components, this.Wires);
    }

    public static Simulation FromCircuit(Circuit circuit, params string[] excludeComps)
    {
        var sim = new Simulation();
        foreach (var component in circuit.Components)
        {
            if (excludeComps.Contains(component.ComponentTypeID))
                continue;

            var c = component.CreateComponent();
            sim.AddComponent(c, c.Position);
        }

        foreach (var wire in circuit.Wires)
        {
            sim.AddWire(wire.CreateWire());
        }

        return sim;
    }

    public void SetCircuitInSimulation(Circuit circuit)
    {
        this.Components.Clear();
        this.Wires.Clear();

        foreach (var component in circuit.Components)
        {
            var c = component.CreateComponent();
            this.AddComponent(c, c.Position);
        }

        foreach (var wire in circuit.Wires)
        {
            this.AddWire(wire.CreateWire());
        }
    }

    public bool TryGetIOFromPosition(Vector2i gridPosition, out IO io, out Component component)
    {
        if (this.ComponentIOPositions.TryGetValue(gridPosition, out var found))
        {
            io = found.Item2;
            component = found.Item1;
            return true;
        }

        io = null;
        component = null;
        return false;

        // foreach (var c in this.Components)
        // {
        //     foreach (var i in c.IOs)
        //     {
        //         if (c.GetPositionForIO(i, out _) == gridPosition)
        //         {
        //             io = i;
        //             component = c;
        //             this.ComponentIOPositions.Add(gridPosition, (c, i));
        //             return true;
        //         }
        //     }
        // }

        // io = null;
        // component = null;
        // return false;
    }

    public bool TryGetIOFromPosition(Vector2 worldPosition, out IO io, out Component component)
    {
        foreach (var c in this.Components)
        {
            var positions = c.GetAllIOPositions();

            int i = 0;
            foreach (var p in positions)
            {
                var pos = p.ToVector2(Constants.GRIDSIZE);
                var _io = c.GetIO(i);

                if (Vector2.Distance(pos, worldPosition) <= Constants.IO_GROUP_RADIUS)
                {
                    io = _io;
                    component = c;
                    return true;
                }

                i++;
            }
        }

        io = null;
        component = null;
        return false;
    }

    public void ConnectPointsWithWire(Vector2i point1, Vector2i point2)
    {
        if (point1 == point2)
        {
            return;
        }

        if (this.TryGetWireAtPos(point1, out var w1))
        {
            if (this.TryGetWireAtPos(point2, out var w2))
            {
                if (w1 == w2)
                {
                    // Same wire? Just add the segment
                    w1.AddSegment(point1, point2);
                }
                else
                {
                    // Different wires? Merge them
                    w1.MergeWith(w2);
                    w1.AddSegment(point1, point2);
                    this.Wires.Remove(w2);
                }
            }
            else
            {
                // Add segment to wire
                w1.AddSegment(point1, point2);
            }
        }
        else
        {
            if (this.TryGetWireAtPos(point2, out var w2))
            {
                // Add segment to wire
                w2.AddSegment(point2, point1);
            }
            else
            {
                // Create new wire
                var wire = new Wire(point1, point2);
                this.AddWire(wire);
            }
        }

        this.WirePositions.Clear();
    }

    public bool TryGetWireSegmentAtPos(Vector2i pos, out (Vector2i, Vector2i) edge, out Wire wire)
    {
        foreach (var w in this.Wires)
        {
            if (w.TryGetSegmentAtPos(pos, out edge))
            {
                wire = w;
                return true;
            }
        }

        edge = (Vector2i.Zero, Vector2i.Zero);
        wire = null;
        return false;
    }

    public bool TryGetWireVertexAtPos(Vector2i pos, out Wire wire)
    {
        foreach (var w in this.Wires)
        {
            if (Utilities.IsPointInGraph(w.Segments, pos))
            {
                wire = w;
                return true;
            }
        }

        wire = null;
        return false;
    }

    public bool TryGetComponentAtPos(Vector2i pos, out Component component)
    {
        foreach (var c in this.Components)
        {
            if (c.GetBoundingBox(out var tz).Contains(pos.ToVector2(Constants.GRIDSIZE)))
            {
                component = c;
                return true;
            }
        }

        component = null;
        return false;
    }

    public bool TryGetComponentAtPos(Vector2 pos, out Component component)
    {
        foreach (var c in this.Components)
        {
            if (c.GetBoundingBox(out var tz).Contains(pos))
            {
                component = c;
                return true;
            }
        }

        component = null;
        return false;
    }

    public void DisconnectPoints(Vector2i point1, Vector2i point2)
    {
        if (this.TryGetWireAtPos(point1, out var w1))
        {
            if (this.TryGetWireAtPos(point2, out var w2))
            {
                if (w1 == w2)
                {
                    // Same wire? Remove the segment
                    Wire[] newWires = Wire.RemoveSegmentFromWire(w1, (point1, point2));
                    this.Wires.Remove(w1);
                    this.Wires.AddRange(newWires);
                    this.WirePositions.Clear();
                }
                else
                {
                    // Should never be different wires? Two points are only connected if they are on the same wire
                    throw new Exception("Two points are only connected if they are on the same wire");
                }
            }
            else
            {
                throw new Exception("No wire at point 2");
            }
        }
        else
        {
            throw new Exception("No wire at point 1");
        }
    }

    public void RemoveWirePoint(Vector2i point)
    {
        if (this.TryGetWireVertexAtPos(point, out var wire))
        {
            Wire[] newWires = wire.RemoveVertex(point);
            this.Wires.Remove(wire);
            this.Wires.AddRange(newWires);
        }
    }

    public void SelectComponent(Component component)
    {
        if (!this.SelectedComponents.Contains(component))
        {
            this.SelectedComponents.Add(component);
        }
    }

    public void DeselectComponent(Component component)
    {
        if (this.SelectedComponents.Contains(component))
        {
            this.SelectedComponents.Remove(component);
        }
    }

    public void ToggleSelection(Component component)
    {
        if (this.SelectedComponents.Contains(component))
        {
            this.SelectedComponents.Remove(component);
        }
        else
        {
            this.SelectedComponents.Add(component);
        }
    }

    public void ClearSelection()
    {
        this.SelectedComponents.Clear();
        this.SelectedWireSegments.Clear();
    }

    public void SelectAllComponents()
    {
        this.SelectedComponents.Clear();
        this.SelectedComponents.AddRange(this.Components);
    }

    public void SelectComponentsInRectangle(RectangleF rectangle)
    {
        this.SelectedComponents.Clear();

        foreach (var c in this.Components)
        {
            if (c.GetBoundingBox(out var tz).IntersectsWith(rectangle))
            {
                this.SelectedComponents.Add(c);
            }
        }
    }

    public bool IsComponentSelected(Component component)
    {
        return this.SelectedComponents.Contains(component);
    }

    public void MoveSelection(Vector2i delta)
    {
        foreach (var c in this.SelectedComponents)
        {
            c.Move(delta);
        }
        this.ComponentIOPositions.Clear();
    }
}