namespace LogiX.Components;

public class CCPUSplitter : IUISubmitter<bool, Editor.Editor>
{
    int bits = 2;
    bool multiToSingle = true;

    public CCPUSplitter()
    {

    }

    public bool SubmitUI(Editor.Editor editor)
    {
        ImGui.SetNextItemWidth(80);
        ImGui.InputInt("Bits", ref this.bits);
        if (this.bits < 1)
        {
            this.bits = 1;
        }
        ImGui.SetNextItemWidth(80);

        if (ImGui.RadioButton("Multibit to single bit", multiToSingle))
        {
            multiToSingle = true;
        }
        if (ImGui.RadioButton("Single bit to multibit", !multiToSingle))
        {
            multiToSingle = false;
        }

        ImGui.Separator();
        ImGui.Button("Create");

        if (ImGui.IsItemClicked())
        {
            Component c = new Splitter(this.bits, this.bits, multiToSingle, !multiToSingle, UserInput.GetMousePositionInWorld(editor.editorCamera));
            NewComponentCommand ncc = new NewComponentCommand(c, UserInput.GetMousePositionInWorld(editor.editorCamera));
            editor.Execute(ncc, editor);
            return true;
        }
        return false;
    }
}