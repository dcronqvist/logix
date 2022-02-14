namespace LogiX.Components;

public class CCPUBitwise : IUISubmitter<bool, Editor.Editor>
{
    int currentType = 0;
    int bits = 4;
    bool multibit = true;
    string[] types = new string[] {
        "AND",
        "NAND",
        "OR",
        "NOR",
        "XOR",
    };

    public CCPUBitwise()
    {

    }

    public bool SubmitUI(Editor.Editor editor)
    {
        ImGui.SetNextItemWidth(80);
        ImGui.Combo("Gate Type", ref currentType, this.types, this.types.Length);

        ImGui.SetNextItemWidth(80);
        ImGui.InputInt("Input Bits", ref this.bits);
        if (this.bits < 1)
        {
            this.bits = 1;
        }
        ImGui.Checkbox("Multibit", ref this.multibit);
        ImGui.Separator();
        ImGui.Button("Create");

        if (ImGui.IsItemClicked())
        {
            Component c = new BitwiseComponent(Util.GetGateLogicFromName(this.types[this.currentType]), this.bits, this.multibit, UserInput.GetMousePositionInWorld(editor.editorCamera));
            editor.NewComponent(c);
            return true;
        }
        return false;
    }
}