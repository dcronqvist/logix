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

    public override int GetMaxStepsToOtherComponent(Component other)
    {
        if (other == this)
        {
            return 1;
        }
        else
        {
            int max = 0;
            for (int i = 0; i < this.Inputs.Count; i++)
            {
                for (int j = 0; j < this.Outputs.Count; j++)
                {
                    int steps = this.GetMaxStepsBetweenInputOutput(i, j);

                    foreach (Wire w in this.OutputAt(j).Signals)
                    {
                        int stepsFromOutputToOther = w.To.GetMaxStepsToOtherComponent(other);

                        max = Math.Max(max, steps + stepsFromOutputToOther - 1);
                    }
                }
            }

            return max;
        }
    }

    public int GetMaxStepsBetweenInputOutput(int input, int output)
    {
        List<string> switchIds = this.Description.InputOrder[input];
        List<string> lampIds = this.Description.OutputOrder[output];

        int max = 0;

        foreach (string sid in switchIds)
        {
            Switch s = this.GetSwitchWithID(sid);

            foreach (string lid in lampIds)
            {
                Lamp l = this.GetLampWithID(lid);

                int steps = s.GetMaxStepsToOtherComponent(l);
                max = Math.Max(max, steps);
            }
        }

        return max;
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

    private float currentSimCounter = 0f;

    public void SingleLogic(Simulator simulator)
    {
        if (simulator.Simulating)
        {
            currentSimCounter += simulator.SimulationSpeed;

            while (currentSimCounter >= 1f)
            {
                foreach (Component c in this.Components)
                {
                    c.Update(Vector2.Zero, simulator);
                }
                currentSimCounter -= 1f;
            }
        }
    }

    public override void Update(Vector2 mousePosInWorld, Simulator simulator)
    {
        base.UpdateInputs();

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

        SingleLogic(simulator);

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

        base.UpdateOutputs();
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

    public override void PerformLogic()
    {

    }
}