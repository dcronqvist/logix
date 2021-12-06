using LogiX.Components;

namespace LogiX.Editor;

public class Simulator
{
    public List<Component> Components { get; private set; }
    public List<Wire> Wires { get; private set; }
    public List<Component> SelectedComponents { get; private set; }

    public Simulator()
    {
        this.Components = new List<Component>();
        this.Wires = new List<Wire>();
        this.SelectedComponents = new List<Component>();
    }

    public void Update(Vector2 mousePosInWorld)
    {
        foreach (Component component in this.Components)
        {
            component.Update(mousePosInWorld);
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
    }

    public void AddComponent(Component c)
    {
        this.Components.Add(c);
    }

    public void AddWire(Wire wire)
    {
        this.Wires.Add(wire);
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

    public void MoveSelection(Camera2D cam)
    {
        foreach (Component c in this.SelectedComponents)
        {
            c.Position += UserInput.GetMouseDelta(cam);
        }
    }

    public void SelectComponent(Component c)
    {
        this.SelectedComponents.Add(c);
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

    public void ClearSelection()
    {
        this.SelectedComponents.Clear();
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