namespace LogiX.Components;

public class CCPUMemory : IUISubmitter<bool, Editor.Editor>
{
    int addressBits;
    int dataBits;
    bool addressMultibit;
    bool dataMultibit;

    public CCPUMemory()
    {
        this.addressBits = 4;
        this.dataBits = 8;
        this.addressMultibit = true;
        this.dataMultibit = true;
    }

    public bool SubmitUI(Editor.Editor editor)
    {
        ImGui.SetNextItemWidth(80);
        ImGui.InputInt("Address Bits", ref this.addressBits);
        if (this.addressBits < 1)
        {
            this.addressBits = 1;
        }
        ImGui.Checkbox("Address Multibit", ref this.addressMultibit);
        ImGui.SetNextItemWidth(80);
        ImGui.InputInt("Data Bits", ref this.dataBits, 8);
        if (this.dataBits % 8 != 0)
        {
            this.dataBits = 8;
        }
        ImGui.Checkbox("Data Multibit", ref this.dataMultibit);
        ImGui.Separator();
        ImGui.Button("Create");

        if (ImGui.IsItemClicked())
        {
            MemoryComponent mc = new MemoryComponent(this.addressBits, this.addressMultibit, this.dataBits, this.dataMultibit, UserInput.GetMousePositionInWorld(editor.editorCamera));
            editor.NewComponent(mc);
            return true;
        }
        return false;
    }
}