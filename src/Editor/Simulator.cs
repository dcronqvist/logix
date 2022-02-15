using LogiX.Components;

namespace LogiX.Editor;

public class Simulator
{
    public List<Component> Components { get; private set; }
    public List<Wire> Wires { get; private set; }
    public List<Component> SelectedComponents { get; private set; }
    public List<(Wire, int)> SelectedWirePoints { get; private set; }
    public float SimulationSpeed { get; set; } = 1f;
    public float UpdatesPerFrame => 1f / this.SimulationSpeed;
    public bool Simulating { get; set; } = true;
    private float currentSimCounter = 0f;
    public Action<Component, Exception> OnComponentCausedError { get; set; }

    public Simulator()
    {
        this.Components = new List<Component>();
        this.Wires = new List<Wire>();
        this.SelectedComponents = new List<Component>();
        this.SelectedWirePoints = new List<(Wire, int)>();
    }

    public void SingleUpdate(Vector2 mousePosInWorld)
    {
        bool oldSimulate = this.Simulating;
        this.Simulating = true;
        for (int i = this.Components.Count - 1; i >= 0; i--)
        {
            Component component = this.Components[i];

            try
            {
                component.Update(mousePosInWorld, this);
            }
            catch (Exception e)
            {
                OnComponentCausedError?.Invoke(component, e);
            }
        }

        for (int i = this.Wires.Count - 1; i >= 0; i--)
        {
            Wire wire = this.Wires[i];
            wire.Update(mousePosInWorld, this);
        }
        this.Simulating = oldSimulate;
    }

    public void Update(Vector2 mousePosInWorld)
    {
        //this.Components.Shuffle();
        if (Simulating)
        {
            this.currentSimCounter += this.SimulationSpeed;

            while (this.currentSimCounter >= 1f)
            {
                this.currentSimCounter -= 1f;
                this.SingleUpdate(mousePosInWorld);
            }
        }
    }

    public void Interact(Vector2 mousePosInWorld)
    {
        foreach (Component component in this.Components)
        {
            component.Interact(mousePosInWorld, this);
        }

        foreach (Wire w in this.Wires)
        {
            w.Interact(mousePosInWorld, this);
        }
    }

    public void Render(Vector2 mousePosInWorld)
    {
        foreach (Wire wire in this.Wires)
        {
            wire.Render(mousePosInWorld);
        }

        foreach (Component component in this.Components)
        {
            component.Render(mousePosInWorld);
        }

        foreach (Component component in this.SelectedComponents)
        {
            component.RenderSelected();
        }

        foreach ((Wire w, int i) in this.SelectedWirePoints)
        {
            Vector2 p = w.IntermediatePoints[i];
            Raylib.DrawCircleV(p, 7f, Color.ORANGE);
            Raylib.DrawCircleV(p, 5f, Util.InterpolateColors(Color.WHITE, Color.BLUE, w.GetHighFraction()));
        }
    }

    public void AddComponents(List<Component> components)
    {
        foreach (Component c in components)
        {
            this.AddComponent(c);
        }
    }

    public void AddComponent(Component c)
    {
        this.Components.Add(c);
    }

    public void DeleteComponent(Component c)
    {
        // Delete all input wires
        // Delete all output wires
        // Delete component

        List<Wire> toDelete = new List<Wire>();

        foreach (ComponentInput ci in c.Inputs)
        {
            if (ci.Signal != null)
            {
                toDelete.Add(ci.Signal);
            }
        }

        foreach (ComponentOutput co in c.Outputs)
        {
            if (co.Signals.Count > 0)
            {
                co.Signals.ForEach((wire) =>
                {
                    toDelete.Add(wire);
                });
            }
        }

        for (int i = 0; i < toDelete.Count; i++)
        {
            this.DeleteWire(toDelete[i]);
        }
        this.Components.Remove(c);
        if (this.SelectedComponents.Contains(c))
        {
            this.SelectedComponents.Remove(c);
        }
    }

    public void AddWires(List<Wire> wires)
    {
        foreach (Wire w in wires)
        {
            this.AddWire(w);
        }
    }

    public void AddWire(Wire wire)
    {
        this.Wires.Add(wire);
    }

    public void DeleteWire(Wire wire)
    {
        wire.From.RemoveOutputWire(wire.FromIndex, wire);
        wire.To.RemoveInputWire(wire.ToIndex);

        this.Wires.Remove(wire);

        List<(Wire, int)> selected = this.SelectedWirePoints.Where((w, i) => w.Item1 == wire).ToList();
        foreach ((Wire w, int i) in selected)
        {
            this.SelectedWirePoints.Remove((w, i));
        }
    }

    public Component? GetComponentFromWorldPos(Vector2 posInWorld)
    {
        foreach (Component c in this.Components)
        {
            if (Raylib.CheckCollisionPointRec(posInWorld, c.Box))
            {
                return c;
            }
        }

        return null;
    }

    public Wire? GetWireFromWorldPos(Vector2 posInWorld)
    {
        foreach (Wire w in this.Wires)
        {
            if (w.IsPositionOnWire(posInWorld, out Vector2 lStart, out Vector2 lEnd))
            {
                return w;
            }
        }
        return null;
    }

    public (Wire?, int) GetWireAndPointFromWorldPos(Vector2 posInWorld)
    {
        foreach (Wire w in this.Wires)
        {
            if (w.IsPositionOnIntermediatePoint(posInWorld, out Vector2 p))
            {
                return (w, w.IntermediatePoints.IndexOf(p));
            }
        }
        return (null, -1);
    }

    public void SelectWirePoint(Wire w, int index)
    {
        this.SelectedWirePoints.Add((w, index));
    }

    public void MoveSelection(Camera2D cam)
    {
        foreach (Component c in this.SelectedComponents)
        {
            c.Position += UserInput.GetMouseDelta(cam);
        }

        if (this.SelectedWirePoints.Count == 1 && Raylib.IsKeyReleased(KeyboardKey.KEY_LEFT_SHIFT))
        {
            (Wire w, int i) = this.SelectedWirePoints[0];
            w.IntermediatePoints[i] = UserInput.GetMousePositionInWorld(cam);
        }

        if (this.SelectedWirePoints.Count == 1 && Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
        {
            // Snap to adjacent positions
            (Wire w, int i) = this.SelectedWirePoints[0];
            Vector2 p = w.IntermediatePoints[i];
            (Vector2 p1, Vector2 p2) = w.GetAdjacentPositionsToIntermediate(p);

            // Snap to closest to c1 or c2
            (Vector2 c1, Vector2 c2) = Util.GetIntersectingCornersOfPoints(p1, p2);

            w.IntermediatePoints[i] = Util.GetClosestPoint(UserInput.GetMousePositionInWorld(cam), c1, c2);
        }
        else
        {
            foreach ((Wire w, int i) in this.SelectedWirePoints)
            {
                w.IntermediatePoints[i] += UserInput.GetMouseDelta(cam);
            }
        }
    }

    public void SelectComponent(Component c)
    {
        this.SelectedComponents.Add(c);
    }

    public void SelectComponentsInRectangle(Rectangle rec)
    {
        foreach (Component c in this.Components)
        {
            if (Raylib.CheckCollisionRecs(c.Box, rec))
            {
                this.SelectComponent(c);
            }
        }
    }

    public void SelectWirePointsInRectangle(Rectangle rec)
    {
        foreach (Wire w in this.Wires)
        {
            for (int i = 0; i < w.IntermediatePoints.Count; i++)
            {
                if (Raylib.CheckCollisionPointRec(w.IntermediatePoints[i], rec))
                {
                    this.SelectWirePoint(w, i);
                }
            }
        }
    }

    public void SelectAllComponents()
    {
        this.ClearSelection();

        this.Components.ForEach(comp =>
        {
            this.SelectComponent(comp);
        });
    }

    public void DeselectComponent(Component c)
    {
        this.SelectedComponents.Remove(c);
    }

    public void ToggleComponentSelected(Component c)
    {
        if (this.SelectedComponents.Contains(c))
        {
            this.DeselectComponent(c);
        }
        else
        {
            this.SelectComponent(c);
        }
    }

    public bool IsComponentSelected(Component c)
    {
        return this.SelectedComponents.Contains(c);
    }

    public void DeleteSelection()
    {
        for (int i = this.SelectedComponents.Count - 1; i >= 0; i--)
        {
            Component c = this.SelectedComponents[i];
            this.DeleteComponent(c);
        }
        this.ClearSelection();
    }

    public void ClearSelection()
    {
        this.SelectedComponents.Clear();
        //this.SelectedWirePoints.Clear();
    }

    public ComponentInput? GetInputFromWorldPos(Vector2 posInWorld)
    {
        foreach (Component c in this.Components)
        {
            if (c.TryGetInputFromPosition(posInWorld, out ComponentInput? input))
            {
                return input;
            }
        }

        return null;
    }

    public ComponentOutput? GetOutputFromWorldPos(Vector2 posInWorld)
    {
        foreach (Component c in this.Components)
        {
            if (c.TryGetOutputFromPosition(posInWorld, out ComponentOutput? output))
            {
                return output;
            }
        }

        return null;
    }
}