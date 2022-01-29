using LogiX.Components;

namespace LogiX.Editor;

public class Simulator
{
    public List<Component> Components { get; private set; }
    public List<Wire> Wires { get; private set; }
    public List<Component> SelectedComponents { get; private set; }
    public List<(Wire, int)> SelectedWirePoints { get; private set; }

    public Simulator()
    {
        this.Components = new List<Component>();
        this.Wires = new List<Wire>();
        this.SelectedComponents = new List<Component>();
        this.SelectedWirePoints = new List<(Wire, int)>();
    }

    public void Update(Vector2 mousePosInWorld)
    {
        foreach (Component component in this.Components)
        {
            component.Update(mousePosInWorld);
        }

        foreach (Wire wire in this.Wires)
        {
            wire.Update(mousePosInWorld, this);
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
            //if (Raylib.CheckCollisionPointRec(c))
            // TODO: ADD context menu to component in world
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

        foreach ((Wire w, int i) in this.SelectedWirePoints)
        {
            w.IntermediatePoints[i] += UserInput.GetMouseDelta(cam);
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
        foreach (Component c in this.SelectedComponents)
        {
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