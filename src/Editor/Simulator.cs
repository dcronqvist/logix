using LogiX.Components;
using QuikGraph;
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

    public List<Switch> GetAllSwitches()
    {
        return AllComponents.Where(x => x is Switch).Cast<Switch>().ToList();
    }

    public List<Lamp> GetAllLamps()
    {
        return AllComponents.Where(x => x is Lamp).Cast<Lamp>().ToList();
    }

    public bool TryGetComponentByID(string id, [NotNullWhen(true)] out Component? comp)
    {
        comp = AllComponents.FirstOrDefault(x => x.UniqueID == id, null);
        return comp != null;
    }

    public List<(int, IOConfig, string)> GetIOConfigs()
    {
        List<Switch> switches = GetAllSwitches().OrderBy(x => x.Position.X).ThenBy(x => x.Position.Y).ToList();
        List<Lamp> lamps = GetAllLamps().OrderBy(x => x.Position.X).ThenBy(x => x.Position.Y).ToList();

        List<(int, IOConfig, string)> ioConfigs = new List<(int, IOConfig, string)>();

        foreach (Switch s in switches)
        {
            ioConfigs.Add((s.Bits, new IOConfig(s.Side, s.Identifier), s.UniqueID));
        }

        foreach (Lamp l in lamps)
        {
            ioConfigs.Add((l.Bits, new IOConfig(l.Side, l.Identifier), l.UniqueID));
        }

        return ioConfigs;
    }

    public void RotateSelection(int i)
    {
        foreach (Component item in Selection.Where(x => x is Component))
        {
            item.Rotation += i;
        }
    }

    public void AddComponent(Component component)
    {
        this.AllComponents.Add(component);
    }

    public void AddWire(Wire wire)
    {
        if (!this.AllWires.Contains(wire))
        {
            this.AllWires.Add(wire);
        }
    }

    public void RemoveComponent(Component component)
    {
        this.AllComponents.Remove(component);

        if (this.Selection.Contains(component))
        {
            this.Selection.Remove(component);
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
            List<WireNode> wireNodes = wire.Graph.Vertices.ToList();

            foreach (WireNode wn in wireNodes)
            {
                if (wn is JunctionWireNode)
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

    public bool TryGetIOWireNodeFromWorldPosition(Vector2 position, [NotNullWhen(true)] out IOWireNode? node, [NotNullWhen(true)] out Wire? wire)
    {
        foreach (Wire w in this.AllWires)
        {
            if (w.TryGetIOWireNodeFromPosition(position, out node))
            {
                wire = w;
                return true;
            }
        }

        wire = null;
        node = null;
        return false;
    }

    public bool TryGetWireNodeFromWorldPosition(Vector2 position, [NotNullWhen(true)] out WireNode? node, [NotNullWhen(true)] out Wire? wire)
    {
        if (this.TryGetIOWireNodeFromWorldPosition(position, out IOWireNode? iowireNode, out wire))
        {
            node = iowireNode;
            return true;
        }
        else if (this.TryGetJunctionFromPosition(position, out JunctionWireNode? junctionWireNode, out wire))
        {
            node = junctionWireNode;
            return true;
        }

        node = null;
        wire = null;
        return false;
    }

    public void RemoveWire(Wire wire)
    {
        // wire.DisconnectAllIOs();

        this.AllWires.Remove(wire);

        // foreach (WireNode n in wire.Graph.Vertices)
        // {
        //     if (this.Selection.Contains(n))
        //     {
        //         this.Selection.Remove(n);
        //     }
        // }
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

    public bool TryGetJunctionFromPosition(Vector2 position, [NotNullWhen(true)] out JunctionWireNode? node, [NotNullWhen(true)] out Wire? nodeOnWire)
    {
        foreach (Wire wire in this.AllWires)
        {
            if (wire.TryGetJunctionFromPosition(position, out node))
            {
                nodeOnWire = wire;
                return true;
            }
        }

        nodeOnWire = null;
        node = null;
        return false;
    }

    public bool TryGetEdgeFromPosition(Vector2 position, [NotNullWhen(true)] out Edge<WireNode>? edge, [NotNullWhen(true)] out Wire? nodeOnWire)
    {
        foreach (Wire wire in this.AllWires)
        {
            if (wire.TryGetEdgeFromPosition(position, out edge))
            {
                nodeOnWire = wire;
                return true;
            }
        }

        nodeOnWire = null;
        edge = null;
        return false;
    }

    public void Interact(Editor editor)
    {
        foreach (Component component in this.AllComponents)
        {
            component.Interact(editor);
        }
    }

    public List<TComp> GetComponents<TComp>(Func<TComp, bool> func) where TComp : Component
    {
        List<TComp> components = new List<TComp>();

        foreach (Component c in this.AllComponents)
        {
            if (c is TComp tcomp && func(tcomp))
            {
                components.Add(tcomp);
            }
        }

        return components;
    }

    public Simulator Copy()
    {
        Simulator copy = new Simulator();

        foreach (Component component in this.AllComponents)
        {
            copy.AddComponent(component);
        }

        foreach (Wire wire in this.AllWires)
        {
            copy.AddWire(wire);
        }

        return copy;
    }
}