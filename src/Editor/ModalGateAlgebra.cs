namespace LogiX.Editor;

public class ModalGateAlgebra : Modal
{
    string icName = "";
    string gateAlgebra = "";

    public override bool SubmitContent(Editor editor)
    {
        ImGui.InputText("Circuit name", ref this.icName, 25);
        ImGui.InputTextMultiline("Gate algebra", ref this.gateAlgebra, 1024, new Vector2(500, 200), ImGuiInputTextFlags.AllowTabInput);

        if (ImGui.Button("Create"))
        {
            editor.loadedProject.AddProjectCreatedIC(Util.CreateICDescriptionFromGateAlgebra(icName, gateAlgebra));
            editor.LoadComponentButtons();
            return true;
        }

        return false;
    }
}