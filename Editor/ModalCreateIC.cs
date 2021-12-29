using LogiX.SaveSystem;

namespace LogiX.Editor;

public class ModalCreateIC : Modal
{
    private string icName = "";
    private List<SLDescription> icSwitches;
    private Dictionary<SLDescription, int> icSwitchGroup;
    private List<SLDescription> icLamps;
    private Dictionary<SLDescription, int> icLampGroup;
    private CircuitDescription cd;

    public ModalCreateIC(CircuitDescription cd)
    {
        this.cd = cd;
        icSwitchGroup = new Dictionary<SLDescription, int>();
        icSwitches = cd.GetSwitches();
        icSwitches = icSwitches.OrderBy(x => x.Position.Y).ToList();
        for (int i = 0; i < icSwitches.Count; i++)
        {
            icSwitchGroup.Add(icSwitches[i], i);
        }
        icLampGroup = new Dictionary<SLDescription, int>();
        icLamps = cd.GetLamps();
        icLamps = icLamps.OrderBy(x => x.Position.Y).ToList();
        for (int i = 0; i < icLamps.Count; i++)
        {
            icLampGroup.Add(icLamps[i], i);
        }
        icName = "";
    }

    public override bool SubmitContent(Editor editor)
    {
        ImGui.InputText("Circuit name", ref this.icName, 25);
        ImGui.Separator();

        ImGui.Columns(2);
        ImGui.Text("Inputs");
        ImGui.NextColumn();
        ImGui.Text("Outputs");
        ImGui.Separator();
        ImGui.NextColumn();

        for (int i = 0; i < icSwitches.Count; i++)
        {
            SLDescription sw = icSwitches[i];
            ImGui.PushID(sw.ID);
            ImGui.SetNextItemWidth(80);
            int gr = this.icSwitchGroup[sw];
            ImGui.InputInt("", ref gr, 1, 1);
            this.icSwitchGroup[sw] = gr;
            ImGui.PopID();
            ImGui.SameLine();
            int group = this.icSwitchGroup[sw];
            ImGui.Text(sw.Name);
            ImGui.SameLine();
            ImGui.PushID(sw.ID + "up");
            if (ImGui.Button("^"))
            {
                int nNext = i - 1;
                if (nNext >= 0)
                {
                    icSwitches[i] = icSwitches[nNext];
                    icSwitches[nNext] = sw;
                }
            }
            ImGui.PopID();
            ImGui.PushID(sw.ID + "down");
            ImGui.SameLine();
            if (ImGui.Button("v"))
            {
                int nNext = i + 1;
                if (nNext < icSwitches.Count)
                {
                    icSwitches[i] = icSwitches[nNext];
                    icSwitches[nNext] = sw;
                }
            }
            ImGui.PopID();
            /*
            if (ImGui.IsItemActive() && !ImGui.IsItemHovered())
            {
                int nNext = i + (ImGui.GetMouseDragDelta(ImGuiMouseButton.Left).Y < 0f ? -1 : 1);
                if (nNext >= 0 && nNext < icSwitches.Count)
                {
                    icSwitches[i] = icSwitches[nNext];
                    icSwitches[nNext] = sw;
                }
            }*/
        }

        ImGui.NextColumn();

        for (int i = 0; i < icLamps.Count; i++)
        {
            SLDescription sw = icLamps[i];
            ImGui.PushID(sw.ID);
            ImGui.SetNextItemWidth(80);
            int gr = this.icLampGroup[sw];
            ImGui.InputInt("", ref gr, 1, 1);
            this.icLampGroup[sw] = gr;
            ImGui.PopID();
            ImGui.SameLine();
            int group = this.icLampGroup[sw];
            ImGui.Text(sw.Name);
            ImGui.SameLine();
            ImGui.PushID(sw.ID + "up");
            if (ImGui.Button("^"))
            {
                int nNext = i - 1;
                if (nNext >= 0)
                {
                    icLamps[i] = icLamps[nNext];
                    icLamps[nNext] = sw;
                }
            }
            ImGui.PopID();
            ImGui.PushID(sw.ID + "down");
            ImGui.SameLine();
            if (ImGui.Button("v"))
            {
                int nNext = i + 1;
                if (nNext < icLamps.Count)
                {
                    icLamps[i] = icLamps[nNext];
                    icLamps[nNext] = sw;
                }
            }
            ImGui.PopID();
        }

        ImGui.Columns(1);
        ImGui.Separator();

        if (ImGui.Button("Create"))
        {
            if (this.cd.ValidForIC())
            {
                List<List<string>> inputOrder = new List<List<string>>();
                List<List<string>> outputOrder = new List<List<string>>();

                int max = 0;
                foreach (KeyValuePair<SLDescription, int> kvp in this.icSwitchGroup)
                {
                    max = Math.Max(max, kvp.Value);
                }

                for (int i = 0; i <= max; i++)
                {
                    if (this.icSwitchGroup.ContainsValue(i))
                    {
                        List<SLDescription> inGroup = this.icSwitchGroup.Where(x => x.Value == i).Select(x => x.Key).ToList();
                        inputOrder.Add(inGroup.Select(x => x.ID).ToList());
                    }
                }

                foreach (KeyValuePair<SLDescription, int> kvp in this.icLampGroup)
                {
                    max = Math.Max(max, kvp.Value);
                }

                for (int i = 0; i <= max; i++)
                {
                    if (this.icLampGroup.ContainsValue(i))
                    {
                        List<SLDescription> inGroup = this.icLampGroup.Where(x => x.Value == i).Select(x => x.Key).ToList();
                        outputOrder.Add(inGroup.Select(x => x.ID).ToList());
                    }
                }

                ICDescription icd = new ICDescription(this.icName, Vector2.Zero, this.cd, inputOrder, outputOrder);

                //this.simulator.AddComponent(icd.ToComponent());
                editor.loadedProject.AddProjectCreatedIC(icd);
                editor.LoadComponentButtons();
                return true;
            }
        }

        return false;
    }
}