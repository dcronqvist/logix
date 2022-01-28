using LogiX.Components;

namespace LogiX.SaveSystem;

public class CircuitDescription
{
    [JsonPropertyName("components")]
    public List<ComponentDescription> Components { get; set; }

    [JsonPropertyName("wires")]
    public List<WireDescription> Wires { get; set; }

    [JsonConstructor]
    public CircuitDescription()
    {
        this.Components = new List<ComponentDescription>();
        this.Wires = new List<WireDescription>();
    }

    public CircuitDescription(List<Component> components)
    {
        this.Components = components.Select((comp) =>
        {
            ComponentDescription cd = comp.ToDescription();
            if (cd is CustomDescription)
            {
                (cd as CustomDescription).Plugin = ((CustomComponent)comp).Plugin;
                (cd as CustomDescription).PluginVersion = ((CustomComponent)comp).PluginVersion;
            }
            return cd;
        }).ToList();

        Vector2 weighted = this.WeightedMiddlePosition();

        foreach (ComponentDescription cd in this.Components)
        {
            cd.Position -= weighted;
        }

        this.Wires = GetAllWiresInCircuit(components, this.Components, weighted);
    }

    public List<WireDescription> GetAllWiresInCircuit(List<Component> components, List<ComponentDescription> componentDescriptions, Vector2 weighted)
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
            WireDescription wd = new WireDescription(w.Bits, componentDescriptions[components.IndexOf(w.From)].ID, w.FromIndex, componentDescriptions[components.IndexOf(w.To)].ID, w.ToIndex, w.IntermediatePoints.Select(p => p - weighted).ToList());
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

    public Tuple<List<Component>, List<Wire>> CreateComponentsAndWires(Vector2 basePosition, bool preservIds)
    {
        // Check if circuit contains components from a plugin
        // make sure that the plugin exists

        List<CustomDescription> pluginDescriptions = this.Components.Where(x => x is CustomDescription).Cast<CustomDescription>().ToList();
        List<(string, string)> missingPlugins = Util.GetMissingPluginsFromDescriptions(pluginDescriptions);

        if (missingPlugins.Count > 0)
        {
            throw new Exception("The circuit you are trying to load contains components from plugins. When loading these components\n" +
                "one or more errors occured.\n\n" +
                string.Join("\n", missingPlugins.Select(x => $"{x.Item1}, {x.Item2}")));
        }

        List<Component> components = this.Components.Select((cd) =>
        {
            Component c = cd.ToComponent(preservIds);
            c.Position += basePosition;
            return c;
        }).ToList();

        List<Wire> wires = new List<Wire>();

        foreach (WireDescription wd in this.Wires)
        {
            Component to = components[IndexOfComponentWithID(this.Components, wd.To)];
            Component from = components[IndexOfComponentWithID(this.Components, wd.From)];

            Wire w = new Wire(wd.Bits, to, wd.ToInputIndex, from, wd.FromOutputIndex);
            if (wd.IntermediatePoints != null)
                w.IntermediatePoints = wd.IntermediatePoints.Select(p => p + basePosition).ToList();
            else
                w.IntermediatePoints = new List<Vector2>();

            to.SetInputWire(wd.ToInputIndex, w);
            from.AddOutputWire(wd.FromOutputIndex, w);

            wires.Add(w);
        }

        return new Tuple<List<Component>, List<Wire>>(components, wires);
    }

    public SLDescription GetSwitchWithID(string id)
    {
        foreach (SLDescription sl in GetSwitches())
        {
            if (sl.ID == id)
            {
                return sl;
            }
        }

        return null;
    }

    public SLDescription GetLampWithID(string id)
    {
        foreach (SLDescription sl in GetLamps())
        {
            if (sl.ID == id)
            {
                return sl;
            }
        }

        return null;
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

    public bool ValidForIC()
    {
        foreach (SLDescription sw in GetSwitches())
        {
            if (sw.Name == "")
            {
                return false;
            }
        }

        foreach (SLDescription l in GetLamps())
        {
            if (l.Name == "")
            {
                return false;
            }
        }

        return true;
    }
}