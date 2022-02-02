using LogiX.SaveSystem;

namespace LogiX.Components;

public class ICComponent : Component
{
    public ICDescription Description { get; set; }
    public override string Text => Description.Name;
    public override bool DrawIOIdentifiers => true;
    public override bool HasContextMenu => true;

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

        (List<Component> comps, List<Wire> ws) = description.Circuit.CreateComponentsAndWires(Vector2.Zero, true);
        this.Components = comps;
        this.Wires = ws;
    }

    public Lamp GetLampWithID(string id)
    {
        /*
        string name = "";
        foreach (SLDescription sl in this.Description.Circuit.GetLamps())
        {
            if (sl.ID == id)
            {
                name = sl.Name;
            }
        }*/

        foreach (Lamp sw in this.Components.Where(x => x is Lamp))
        {
            if (sw.uniqueID == id)
            {
                return sw;
            }
        }

        return null;
    }

    public Switch GetSwitchWithID(string id)
    {
        /*
        string name = "";
        foreach (SLDescription sl in this.Description.Circuit.GetSwitches())
        {
            if (sl.ID == id)
            {
                name = sl.Name;
            }
        }*/

        foreach (Switch sw in this.Components.Where(x => x is Switch))
        {
            if (sw.uniqueID == id)
            {
                return sw;
            }
        }

        return null;
    }

    public override Dictionary<string, int> GetGateAmount()
    {
        Dictionary<string, int> containedGates = new Dictionary<string, int>();
        foreach (Component c in this.Components)
        {
            containedGates = Util.ConcatGateAmounts(containedGates, c.GetGateAmount());
        }

        return Util.ConcatGateAmounts(containedGates, Util.GateAmount((this.Description.Name, 1)));
    }

    public override void PerformLogic()
    {
        for (int i = 0; i < this.Description.InputOrder.Count; i++)
        {
            List<string> inputs = this.Description.InputOrder[i];

            int cumBits = 0;
            for (int j = 0; j < inputs.Count; j++)
            {
                Switch s = GetSwitchWithID(inputs[j]);

                for (int k = 0; k < s.Outputs[0].Bits; k++)
                {
                    s.Values[k] = this.Inputs[i].Values[cumBits];
                    cumBits += 1;
                    s.UpdateOutputs();
                }
            }
        }

        foreach (Component c in this.Components)
        {
            c.Update(Vector2.Zero);
        }

        // No need to update wires inside of ICComponents, since you can't see them.
        // foreach (Wire w in this.Wires)
        // {
        //     w.Update(Vector2.Zero, null);
        // }

        // for (int i = 0; i < this.Outputs.Count; i++)
        // {
        //     Lamp lamp = this.GetLampForOutput(this.Outputs[i]);

        //     this.Outputs[i].SetValues(lamp.Values);
        // }

        for (int i = 0; i < this.Description.OutputOrder.Count; i++)
        {
            List<string> outputs = this.Description.OutputOrder[i];

            int cumBits = 0;
            for (int j = 0; j < outputs.Count; j++)
            {
                Lamp s = GetLampWithID(outputs[j]);

                for (int k = 0; k < s.Inputs[0].Bits; k++)
                {
                    this.Outputs[i].Values[cumBits] = s.Values[k];
                    cumBits += 1;
                }
            }
        }

        //throw new NotImplementedException();
    }

    public override ICDescription ToDescription()
    {
        ICDescription icd = this.Description.Copy();
        icd.Position = this.Position;
        icd.ID = this.uniqueID;
        icd.Rotation = this.Rotation;
        return icd;
    }

    public override void SubmitContextPopup(Editor.Editor editor)
    {
        if (ImGui.Button("Paste inner circuit"))
        {
            CircuitDescription cd = this.Description.Circuit;
            editor.PasteComponentsAndWires(cd, UserInput.GetMousePositionInWorld(editor.editorCamera), true);
        }

        if (ImGui.TreeNodeEx("Contains gates"))
        {
            foreach (KeyValuePair<string, int> kvp in this.GetGateAmount())
            {
                ImGui.Text($"{kvp.Key}: {kvp.Value}");
            }
            ImGui.TreePop();
        }
        base.SubmitContextPopup(editor);
    }
}