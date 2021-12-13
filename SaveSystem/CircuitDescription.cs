using LogiX.Components;
using Newtonsoft.Json;

namespace LogiX.SaveSystem;

public class CircuitDescription
{
    [JsonProperty(PropertyName = "components")]
    public List<ComponentDescription> Components { get; set; }

    [JsonProperty(PropertyName = "wires")]
    public List<WireDescription> Wires { get; set; }

    public CircuitDescription(List<Component> components)
    {
        this.Components = components.Select((comp) =>
        {
            return comp.ToDescription();
        }).ToList();

        Vector2 weighted = this.WeightedMiddlePosition();

        foreach (ComponentDescription cd in this.Components)
        {
            cd.Position -= weighted;
        }

        this.Wires = GetAllWiresInCircuit(components, this.Components);
    }

    public List<WireDescription> GetAllWiresInCircuit(List<Component> components, List<ComponentDescription> componentDescriptions)
    {
        List<Wire> wires = new List<Wire>();

        foreach (Component c in components)
        {
            foreach (ComponentInput ci in c.Inputs)
            {
                if (ci.Signal != null && !wires.Contains(ci.Signal))
                    wires.Add(ci.Signal);
            }

            foreach (ComponentOutput co in c.Outputs)
            {
                foreach (Wire w in co.Signals)
                {
                    if (!wires.Contains(w))
                        wires.Add(w);
                }
            }
        }

        wires = wires.Where(wire => components.Contains(wire.From) && components.Contains(wire.To)).ToList();

        List<WireDescription> wireDescriptions = new List<WireDescription>();

        foreach (Wire w in wires)
        {
            WireDescription wd = new WireDescription(w.Bits, componentDescriptions[components.IndexOf(w.From)].ID, w.FromIndex, componentDescriptions[components.IndexOf(w.To)].ID, w.ToIndex);
            wireDescriptions.Add(wd);
        }

        return wireDescriptions;
    }

    public int IndexOfComponentWithID(List<ComponentDescription> cds, string id)
    {
        for (int i = 0; i < cds.Count; i++)
        {
            if (cds[i].ID == id)
            {
                return i;
            }
        }

        return -1;
    }

    public Tuple<List<Component>, List<Wire>> CreateComponentsAndWires(Vector2 basePosition)
    {
        List<Component> components = this.Components.Select((cd) =>
        {
            Component c = cd.ToComponent();
            c.Position += basePosition;
            return c;
        }).ToList();

        List<Wire> wires = new List<Wire>();

        foreach (WireDescription wd in this.Wires)
        {
            Component to = components[IndexOfComponentWithID(this.Components, wd.To)];
            Component from = components[IndexOfComponentWithID(this.Components, wd.From)];

            Wire w = new Wire(wd.Bits, to, wd.ToInputIndex, from, wd.FromOutputIndex);

            to.SetInputWire(wd.ToInputIndex, w);
            from.AddOutputWire(wd.FromOutputIndex, w);

            wires.Add(w);
        }

        return new Tuple<List<Component>, List<Wire>>(components, wires);
    }

    public List<SLDescription> GetSwitches()
    {
        return this.Components.Where(x => (x is SLDescription && ((SLDescription)x).Type == ComponentType.Switch)).Cast<SLDescription>().ToList();
    }

    public List<SLDescription> GetLamps()
    {
        return this.Components.Where(x => (x is SLDescription && ((SLDescription)x).Type == ComponentType.Lamp)).Cast<SLDescription>().ToList();
    }

    public Vector2 WeightedMiddlePosition()
    {
        float x = 0;
        float y = 0;

        foreach (ComponentDescription cd in this.Components)
        {
            x += cd.Position.X;
            y += cd.Position.Y;
        }

        return new Vector2(x, y) / this.Components.Count;
    }
}