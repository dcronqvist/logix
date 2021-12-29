namespace LogiX.Components;

public class CCPUSimple : IUISubmitter<bool, Editor.Editor>
{
    bool inputBits, inputMultibit, outputBits, outputMultibit;
    int selectedInputBits, selectedOutputBits;
    bool selectedInputMultibit, selectedOutputMultibit;
    Func<int, bool, int, bool, Component> createComponent;

    public CCPUSimple(bool inputBits, bool inputMultibit, bool outputBits, bool outputMultibit, Func<int, bool, int, bool, Component> createComponent)
    {
        this.inputBits = inputBits;
        this.inputMultibit = inputMultibit;
        this.outputBits = outputBits;
        this.outputMultibit = outputMultibit;
        this.createComponent = createComponent;
    }

    public bool SubmitUI(Editor.Editor editor)
    {
        if (inputBits)
        {
            ImGui.SetNextItemWidth(80);
            ImGui.InputInt("Input Bits", ref this.selectedInputBits);
        }
        if (inputMultibit)
        {
            ImGui.Checkbox("Input Multibit", ref this.selectedInputMultibit);
        }
        if (outputBits)
        {
            ImGui.SetNextItemWidth(80);
            ImGui.InputInt("Output Bits", ref this.selectedOutputBits);
        }
        if (outputMultibit)
        {
            ImGui.Checkbox("Output Multibit", ref this.selectedOutputMultibit);
        }
        ImGui.Separator();
        ImGui.Button("Create");

        if (ImGui.IsItemClicked())
        {
            Component c = createComponent(this.selectedInputBits, this.selectedInputMultibit, this.selectedOutputBits, this.selectedOutputMultibit);
            editor.NewComponent(c);
            return true;
        }
        return false;
    }
}