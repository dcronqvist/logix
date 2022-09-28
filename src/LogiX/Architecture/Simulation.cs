using System.Diagnostics.CodeAnalysis;
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
            if (w.RootNode.CollectPositions().Contains(position))
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
            var positions = wire.RootNode.CollectPositions();
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

    public void AddWire(Wire wire)
    {
        this.Wires.Add(wire);
    }

    public void Tick()
    {
        NewValues = new Dictionary<Vector2i, ValueCollection>();

        // Allow components to perform their logic
        foreach (Component component in Components)
        {
            component.Update(this);
        }

        // Swap pushed values
        CurrentValues = NewValues;
    }

    public void Render(Camera2D cam)
    {
        // Allow components to render themselves
        foreach (Component component in Components)
        {
            component.Render(cam);
        }

        // Allow wires to render themselves
        foreach (Wire wire in Wires)
        {
            wire.Render(this, cam);
        }
    }
}