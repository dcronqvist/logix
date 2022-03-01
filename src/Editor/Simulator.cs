using LogiX.Components;
using System.Diagnostics.CodeAnalysis;

namespace LogiX.Editor;

public class Simulator
{
    public List<Component> AllComponents { get; set; }
    public List<Wire> AllWires { get; set; }
    public List<ISelectable> Selection { get; set; }

    public Simulator()
    {
        this.AllComponents = new List<Component>();
        this.AllWires = new List<Wire>();
        this.Selection = new List<ISelectable>();
    }

    public void AddComponent(Component component)
    {
        this.AllComponents.Add(component);
    }

    public void AddWire(Wire wire)
    {
        this.AllWires.Add(wire);
    }

    public void RemoveComponent(Component component, bool disconnectIOs = true)
    {
        this.AllComponents.Remove(component);

        if (this.Selection.Contains(component))
        {
            this.Selection.Remove(component);
        }

        if (disconnectIOs)
        {
            foreach (Wire w in this.AllWires)
            {
                foreach (IO io in component.IOs.Select(x => x.Item1))
                {
                    if (w.IsConnectedTo(io))
                    {
                        w.DisconnectIO(io);
                    }

                    if (w.IOs.Count == 0)
                    {
                        this.AllWires.Remove(w);
                    }
                }
            }
        }
    }

    public void MoveSelection(Vector2 delta)
    {
        foreach (ISelectable selectable in this.Selection)
        {
            selectable.Move(delta);
        }
    }

    public void Select(ISelectable c)
    {
        if (!this.Selection.Contains(c))
            this.Selection.Add(c);
    }

    public void SelectInRect(Rectangle rec)
    {
        foreach (Component c in this.AllComponents)
        {
            if (Raylib.CheckCollisionRecs(rec, c.GetRectangle()))
            {
                this.Select(c);
            }
        }

        foreach (Wire wire in this.AllWires)
        {
            List<WireNode> wireNodes = wire.Root!.CollectChildrenRecursively();

            foreach (WireNode wn in wireNodes)
            {
                if (wn.Parent != null)
                {
                    if (Raylib.CheckCollisionCircleRec(wn.GetPosition(), 5, rec))
                    {
                        this.Select(wn);
                    }
                }
            }
        }
    }

    public bool IsPositionOnSelected(Vector2 position)
    {
        foreach (ISelectable selectable in this.Selection)
        {
            if (selectable.IsPositionOn(position))
            {
                return true;
            }
        }
        return false;
    }

    public void DeselectComponent(Component c)
    {
        if (this.Selection.Contains(c))
            this.Selection.Remove(c);
    }

    public bool IsComponentSelected(Component c)
    {
        return this.Selection.Contains(c);
    }

    public bool IsSelected(ISelectable selectable)
    {
        return this.Selection.Contains(selectable);
    }

    public bool TryGetComponentFromWorldPosition(Vector2 position, [NotNullWhen(true)] out Component? comp)
    {
        foreach (Component c in this.AllComponents)
        {
            if (Raylib.CheckCollisionPointRec(position, c.GetRectangle()))
            {
                comp = c;
                return true;
            }
        }
        comp = null;
        return false;
    }

    public bool TryGetIOFromWorldPosition(Vector2 position, [NotNullWhen(true)] out (IO, int)? io)
    {
        foreach (Component c in this.AllComponents)
        {
            foreach ((IO i, IOConfig conf) in c.IOs)
            {
                Vector2 ioPos = c.GetIOPosition(i);
                if ((ioPos - position).Length() < c.IORadius)
                {
                    io = (i, c.GetIndexOfIO(i));
                    return true;
                }
            }
        }

        io = null;
        return false;
    }

    public void RemoveWire(Wire wire)
    {
        this.AllWires.Remove(wire);
    }

    public void PerformLogic()
    {
        foreach (Component component in this.AllComponents)
        {
            component.UpdateLogic();
        }

        foreach (Wire wire in this.AllWires)
        {
            wire.Propagate();
        }
    }

    public void Render()
    {
        foreach (ISelectable selectable in this.Selection)
        {
            selectable.RenderSelected();
        }

        foreach (Wire wire in this.AllWires)
        {
            wire.Render();
        }

        foreach (Component component in this.AllComponents)
        {
            component.Render();
        }
    }

    public bool TryGetChildWireNodeFromPosition(Vector2 position, [NotNullWhen(true)] out WireNode? node)
    {
        foreach (Wire wire in this.AllWires)
        {
            if (wire.TryGetChildWireNodeFromPosition(position, out node))
            {
                return true;
            }
        }
        node = null;
        return false;
    }

    public bool TryGetJunctionWireNodeFromPosition(Vector2 position, [NotNullWhen(true)] out JunctionWireNode? node)
    {
        foreach (Wire wire in this.AllWires)
        {
            if (wire.TryGetJunctionWireNodeFromPosition(position, out node))
            {
                return true;
            }
        }
        node = null;
        return false;
    }

    public void Interact(Editor editor)
    {
        foreach (Component component in this.AllComponents)
        {
            component.Interact(editor);
        }
    }
}