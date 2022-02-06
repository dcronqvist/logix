namespace LogiX.Editor;

public class ModalGateAlgebra : Modal
{
    string icName = "";
    string gateAlgebra = "";
    bool isValid = false;
    string currentError = "";

    public ModalGateAlgebra() : base(ImGuiPopupFlags.None, ImGuiWindowFlags.AlwaysAutoResize)
    {

    }

    public override bool SubmitContent(Editor editor)
    {
        ImGui.InputText("Circuit name", ref this.icName, 25);
        ImGui.InputTextMultiline("Gate algebra", ref this.gateAlgebra, 1024, new Vector2(500, 200), ImGuiInputTextFlags.AllowTabInput);
        isValid = Util.TryValidateGateAlgebra(this.gateAlgebra, out currentError);

        if (ImGui.Button("Create") && this.isValid)
        {
            editor.loadedProject.AddProjectCreatedIC(Util.CreateICDescriptionFromGateAlgebra(icName, gateAlgebra));
            editor.LoadComponentButtons();
            return true;
        }
        ImGui.SameLine();

        if (this.isValid)
        {
            ImGui.TextColored(Color.GREEN.ToVector4(), "Valid!");
        }
        else
        {
            ImGui.TextColored(Color.RED.ToVector4(), currentError);
        }

        return false;
    }
}