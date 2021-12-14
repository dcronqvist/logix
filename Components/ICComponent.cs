using LogiX.SaveSystem;

namespace LogiX.Components;

public class ICComponent : Component
{
    private ICDescription Description { get; set; }
    public override string Text => Description.Name;
    public override bool DrawIOIdentifiers => true;

    private List<Component> Components { get; set; }
    private List<Wire> Wires { get; set; }

    public ICComponent(ICDescription description, Vector2 position) : base(description.GetBitsPerInput(), description.GetBitsPerOutput(), position)
    {
        this.Description = description;

        for (int i = 0; i < this.Inputs.Count; i++)
        {
            this.Inputs[i].Identifier = description.GetInputIdentifier(i);
        }

        for (int i = 0; i < this.Outputs.Count; i++)
        {
            this.Outputs[i].Identifier = description.GetOutputIdentifier(i);
        }

        (List<Component> comps, List<Wire> ws) = description.Circuit.CreateComponentsAndWires(Vector2.Zero);
        this.Components = comps;
        this.Wires = ws;
    }

    public Switch GetSwitchForInput(ComponentInput ci)
    {
        foreach (Switch sw in this.Components.Where(x => x is Switch))
        {
            if (sw.ID == ci.Identifier)
            {
                return sw;
            }
        }

        return null;
    }

    public Lamp GetLampForOutput(ComponentOutput co)
    {
        foreach (Lamp lamp in this.Components.Where(x => x is Lamp))
        {
            if (lamp.ID == co.Identifier)
            {
                return lamp;
            }
        }

        return null;
    }

    public override void PerformLogic()
    {
        for (int i = 0; i < this.Inputs.Count; i++)
        {
            Switch sw = this.GetSwitchForInput(this.Inputs[i]);
            sw.Values = this.Inputs[i].Values;
        }

        foreach (Component c in this.Components)
        {
            c.Update(Vector2.Zero);
        }

        foreach (Wire w in this.Wires)
        {
            w.Update(Vector2.Zero);
        }

        for (int i = 0; i < this.Outputs.Count; i++)
        {
            Lamp lamp = this.GetLampForOutput(this.Outputs[i]);

            this.Outputs[i].SetValues(lamp.Values);
        }

        //throw new NotImplementedException();
    }

    public override ComponentDescription ToDescription()
    {
        //throw new NotImplementedException();
        return null;
    }
}