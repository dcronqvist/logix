using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using LogiX.Architecture;
using LogiX.Architecture.Serialization;
using LogiX.Graphics;
using LogiX.Rendering;

namespace LogiX;

public class ValueCollection : List<LogicValue[]>
{
    public bool AnyPushed()
    {
        return this.Count > 0;
    }

    public bool AllAgree()
    {
        if (this.Count == 0)
        {
            return false;
        }

        LogicValue[] values = this[0];

        foreach (LogicValue[] pushedValues in this)
        {
            if (!Utilities.SameAs(pushedValues, values))
            {
                return false;
            }
        }

        return true;
    }

    public bool AllSameWidth()
    {
        if (this.Count == 0)
        {
            return false;
        }

        int width = this[0].Length;

        foreach (LogicValue[] pushedValues in this)
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

public class Simulation
{
    public List<Component> Components { get; private set; } = new();
    public Dictionary<Vector2i, ValueCollection> NewValues { get; private set; } = new();
    public Dictionary<Vector2i, ValueCollection> CurrentValues { get; private set; } = new();
    public List<Wire> Wires { get; private set; } = new();

    public Simulation()
    {
    }

    public bool TryGetLogicValuesAtPosition(Vector2i position, int expectedWidth, [NotNullWhen(true)] out LogicValue[] values, [NotNullWhen(false)] out LogicValueRetrievalStatus status)
    {
        // Find all driven values at that position, if any.
        // If there are no driven values, return UNDEFINED.

        if (!CurrentValues.ContainsKey(position))
        {
            values = Enumerable.Repeat(LogicValue.UNDEFINED, expectedWidth).ToArray();
            status = LogicValueRetrievalStatus.NOTHING_PUSHED;
            return false;
        }

        var pushedValues = CurrentValues[position];
        if (pushedValues.AllAgree() && pushedValues.AllSameWidth())
        {
            if (pushedValues[0].Length == expectedWidth)
            {
                values = pushedValues[0];
                status = LogicValueRetrievalStatus.SUCCESS;
                return true;
            }
            else
            {
                values = Enumerable.Repeat(LogicValue.UNDEFINED, expectedWidth).ToArray();
                status = LogicValueRetrievalStatus.DIFF_WIDTH;
                return false;
            }
        }

        if (!pushedValues.AllSameWidth())
        {
            values = Enumerable.Repeat(LogicValue.UNDEFINED, expectedWidth).ToArray();
            status = LogicValueRetrievalStatus.DIFF_WIDTH;
            return false;
        }

        if (!pushedValues.AllAgree())
        {
            values = Enumerable.Repeat(LogicValue.UNDEFINED, expectedWidth).ToArray();
            status = LogicValueRetrievalStatus.DISAGREE;
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
        if (pushedValues.AllAgree() && pushedValues.AllSameWidth())
        {
            values = pushedValues[0];
            status = LogicValueRetrievalStatus.SUCCESS;
            return true;
        }

        if (!pushedValues.AllSameWidth())
        {
            values = null;
            status = LogicValueRetrievalStatus.DIFF_WIDTH;
            return false;
        }

        if (!pushedValues.AllAgree())
        {
            values = null;
            status = LogicValueRetrievalStatus.DISAGREE;
            return false;
        }

        throw new Exception("This should never happen.");
    }

    public bool TryGetWireAtPos(Vector2i position, [NotNullWhen(true)] out Wire wire)
    {
        foreach (Wire w in Wires)
        {
            if (w.GetPoints().Contains(position))
            {
                wire = w;
                return true;
            }
        }

        wire = null;
        return false;
    }

    public void PushValuesAt(Vector2i position, params LogicValue[] values)
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
                if (!NewValues.ContainsKey(pos))
                {
                    NewValues[pos] = new ValueCollection();
                }

                NewValues[pos].Add(values);
            }
        }
        else
        {
            NewValues[position].Add(values);
        }
    }

    public void Reset()
    {
        // Reset pushed values
        this.NewValues.Clear();
    }

    public void AddComponent(Component component, Vector2i position)
    {
        component.Position = position;
        this.Components.Add(component);
    }

    public void RemoveComponent(Component component)
    {
        this.Components.Remove(component);
    }

    public void AddWire(Wire wire)
    {
        this.Wires.Add(wire);
    }

    public void Tick()
    {
        // Allow components to perform their logic
        foreach (Component component in Components)
        {
            component.Update(this);
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
        //Allow components to render themselves
        foreach (Component component in Components)
        {
            component.Render(cam);
        }

        // Allow wires to render themselves
        foreach (Wire wire in Wires)
        {
            wire.Render(this, cam);
        }

        // var font = LogiX.ContentManager.GetContentItem<Font>("content_1.font.default");
        // var tShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.text");
        // var pShader = LogiX.ContentManager.GetContentItem<ShaderProgram>("content_1.shader_program.primitive");

        // foreach (var kvp in this.CurrentValues)
        // {
        //     var pos = kvp.Key;
        //     var values = kvp.Value;

        //     var worldPos = pos.ToVector2(16);

        //     if (this.TryGetLogicValuesAtPosition(pos, out var vs, out var status))
        //     {
        //         var color = Utilities.GetValueColor(vs);
        //         var amountOfValues = values.Count;

        //         var text = vs.Select(x => x.ToString().Substring(0, 1)).Aggregate((x, y) => x + y);

        //         PrimitiveRenderer.RenderCircle(pShader, worldPos, 7f, 0f, color, cam);
        //         TextRenderer.RenderText(tShader, font, text, worldPos - new Vector2(5, 8), 1f, ColorF.Black, cam);
        //     }
        // }
    }

    public Circuit GetCircuitInSimulation()
    {
        return new Circuit(this.Components, this.Wires);
    }

    public static Simulation FromCircuit(Circuit circuit)
    {
        var sim = new Simulation();
        foreach (var component in circuit.Components)
        {
            var c = component.CreateComponent();
            sim.AddComponent(c, c.Position);
        }

        foreach (var wire in circuit.Wires)
        {

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

    public bool TryGetIOGroupFromPosition(Vector2 worldPosition, out IOGroup ioGroup, out Component component)
    {
        foreach (var c in this.Components)
        {
            var positions = c.GetAllGroupPositions().Select(x => x.ToVector2(16));

            for (int i = 0; i < positions.Count(); i++)
            {
                var pos = positions.ElementAt(i);
                if (Vector2.Distance(pos, worldPosition) < 4f)
                {
                    ioGroup = c.GetIOGroup(i);
                    component = c;
                    return true;
                }
            }
        }

        ioGroup = null;
        component = null;
        return false;
    }

    public void ConnectPointsWithWire(Vector2i point1, Vector2i point2)
    {
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
}