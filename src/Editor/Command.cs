using LogiX.Components;
using LogiX.SaveSystem;

namespace LogiX.Editor;

public abstract class Command<TArg>
{
    public abstract void Execute(TArg arg);
    public abstract void Undo(TArg arg);
    public virtual void Redo(TArg arg) { Execute(arg); }

    public abstract string ToString();
}

public class NewComponentCommand : Command<Editor>
{
    Component component;

    public NewComponentCommand(Component c, Vector2 position)
    {
        this.component = c;
        this.component.Position = position;
    }

    public override void Execute(Editor arg)
    {
        arg.NewComponent(component);
        arg.fsm.SetState<StateMovingSelection>(arg, 1);
    }

    public override void Redo(Editor arg)
    {
        arg.NewComponent(component);
    }

    public override string ToString()
    {
        return $"Created new Component ({this.component.Text})";
    }

    public override void Undo(Editor arg)
    {
        arg.simulator.DeleteComponent(this.component);
    }
}

public class MovedSelectionCommand : Command<Editor>
{
    List<Component> components;
    List<(Wire, int)> wirePoints;
    Vector2 movement;

    public MovedSelectionCommand(List<Component> components, List<(Wire, int)> wirePoints, Vector2 movement)
    {
        this.components = components;
        this.wirePoints = wirePoints;
        this.movement = movement;
    }

    public override void Execute(Editor arg)
    {
        foreach (Component c in components)
        {
            c.Position += movement;
        }
        foreach ((Wire w, int i) in wirePoints)
        {
            w.IntermediatePoints[i] += movement;
        }
    }

    public override string ToString()
    {
        return $"Moved selection";
    }

    public override void Undo(Editor arg)
    {
        foreach (Component c in components)
        {
            c.Position -= movement;
        }
        foreach ((Wire w, int i) in wirePoints)
        {
            w.IntermediatePoints[i] -= movement;
        }
    }
}

public class CopyCircuitCommand : Command<Editor>
{
    CircuitDescription oldCircuit;
    CircuitDescription circuit;

    public CopyCircuitCommand(CircuitDescription oldCircuit, CircuitDescription newCircuit)
    {
        this.oldCircuit = oldCircuit;
        this.circuit = newCircuit;
    }

    public override void Execute(Editor arg)
    {
        arg.copiedCircuit = circuit;
    }

    public override string ToString()
    {
        return "Copied selected circuit to clipboard";
    }

    public override void Undo(Editor arg)
    {
        arg.copiedCircuit = oldCircuit;
    }
}

public class PasteClipboardCommand : Command<Editor>
{
    CircuitDescription circuit;

    List<Component> comps;
    List<Wire> wires;

    public PasteClipboardCommand(CircuitDescription circuit, Vector2 basePosition, bool preserveIds)
    {
        this.circuit = circuit;
        (comps, wires) = circuit.CreateComponentsAndWires(basePosition, preserveIds);
    }

    public override void Execute(Editor arg)
    {
        arg.simulator.AddComponents(comps);
        arg.simulator.AddWires(wires);

        arg.simulator.ClearSelection();
        arg.simulator.SelectedWirePoints.Clear();

        foreach (Component c in comps)
        {
            arg.simulator.SelectComponent(c);
        }
        foreach (Wire w in wires)
        {
            for (int i = 0; i < w.IntermediatePoints.Count; i++)
            {
                arg.simulator.SelectWirePoint(w, i);
            }
        }
    }

    public override string ToString()
    {
        return "Pasted circuit from clipboard";
    }

    public override void Undo(Editor arg)
    {
        foreach (Component c in this.comps)
        {
            arg.simulator.DeleteComponent(c);
        }
        foreach (Wire w in this.wires)
        {
            arg.simulator.DeleteWire(w);
        }
    }
}

public class DeleteSelectionCommand : Command<Editor>
{
    List<Component> components;

    public override void Execute(Editor arg)
    {
        components = arg.simulator.SelectedComponents.Copy();
        arg.simulator.DeleteSelection();
    }

    public override void Redo(Editor arg)
    {
        foreach (Component c in this.components)
        {
            arg.simulator.DeleteComponent(c);
        }
    }

    public override string ToString()
    {
        return "Deleted selected components";
    }

    public override void Undo(Editor arg)
    {
        arg.simulator.AddComponents(components);
    }
}

public class ConnectWireCommand : Command<Editor>
{
    ComponentOutput co;
    ComponentInput ci;

    Wire wire;

    public ConnectWireCommand(ComponentOutput co, ComponentInput ci)
    {
        this.co = co;
        this.ci = ci;
    }

    public override void Execute(Editor arg)
    {
        this.wire = new Wire(co.Bits, ci.OnComponent, ci.OnComponentIndex, co.OnComponent, co.OnComponentIndex);
        this.co.AddOutputWire(this.wire);
        this.ci.SetSignal(this.wire);
        arg.simulator.AddWire(this.wire);
    }

    public override string ToString()
    {
        return "Connected wire";
    }

    public override void Undo(Editor arg)
    {
        arg.simulator.DeleteWire(this.wire);
        this.co.OnComponent.RemoveOutputWire(this.co.OnComponentIndex, this.wire);
        this.ci.OnComponent.RemoveInputWire(this.ci.OnComponentIndex);
    }
}

public class HorizontallyAlignCommand : Command<Editor>
{
    List<(Component, Vector2)> componentsAndOriginal;

    public HorizontallyAlignCommand(List<Component> components)
    {
        componentsAndOriginal = components.Select(x => (x, x.Position)).ToList();
    }

    public override void Execute(Editor arg)
    {
        Vector2 middle = Util.GetMiddleOfListOfVectors(this.componentsAndOriginal.Select(c => c.Item2).ToList());
        foreach (Component c in this.componentsAndOriginal.Select(x => x.Item1))
        {
            c.Position = new Vector2(middle.X, c.Position.Y);
        }
    }

    public override string ToString()
    {
        return "Horizontally aligned selection";
    }

    public override void Undo(Editor arg)
    {
        foreach ((Component c, Vector2 orig) in this.componentsAndOriginal)
        {
            c.Position = orig;
        }
    }
}

public class VerticallyAlignCommand : Command<Editor>
{
    List<(Component, Vector2)> componentsAndOriginal;

    public VerticallyAlignCommand(List<Component> components)
    {
        componentsAndOriginal = components.Select(x => (x, x.Position)).ToList();
    }

    public override void Execute(Editor arg)
    {
        Vector2 middle = Util.GetMiddleOfListOfVectors(this.componentsAndOriginal.Select(c => c.Item2).ToList());
        foreach (Component c in this.componentsAndOriginal.Select(x => x.Item1))
        {
            c.Position = new Vector2(c.Position.X, middle.Y);
        }
    }

    public override string ToString()
    {
        return "Vertically aligned selection";
    }

    public override void Undo(Editor arg)
    {
        foreach ((Component c, Vector2 orig) in this.componentsAndOriginal)
        {
            c.Position = orig;
        }
    }
}

public class RotateCWCommand : Command<Editor>
{
    List<Component> components;

    public RotateCWCommand(List<Component> components)
    {
        this.components = components;
    }

    public override void Execute(Editor arg)
    {
        foreach (Component c in this.components)
        {
            c.RotateRight();
        }
    }

    public override string ToString()
    {
        return "Rotated selection clockwise";
    }

    public override void Undo(Editor arg)
    {
        foreach (Component c in this.components)
        {
            c.RotateLeft();
        }
    }
}

public class RotateCCWCommand : Command<Editor>
{
    List<Component> components;

    public RotateCCWCommand(List<Component> components)
    {
        this.components = components;
    }

    public override void Execute(Editor arg)
    {
        foreach (Component c in this.components)
        {
            c.RotateLeft();
        }
    }

    public override string ToString()
    {
        return "Rotated selection clockwise";
    }

    public override void Undo(Editor arg)
    {
        foreach (Component c in this.components)
        {
            c.RotateRight();
        }
    }
}