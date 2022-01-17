namespace LogiX.Components;

public class CCPUSimple : IUISubmitter<bool, Editor.Editor>
{
    bool inputBits, inputMultibit, outputBits, outputMultibit;
    int selectedInputBits, selectedOutputBits;
    bool selectedInputMultibit, selectedOutputMultibit;
    string inputBitsString, inputMultibitString, outputBitsString, outputMultibitString;
    Func<int, bool, int, bool, Component> createComponent;

    public CCPUSimple(bool inputBits, bool inputMultibit, bool outputBits, bool outputMultibit, Func<int, bool, int, bool, Component> createComponent, string inputBitsString = "Input Bits", string inputMultibitString = "Input Multibit", string outputBitsString = "Output Bits", string outputMultibitString = "Output Multibit")
    {
        this.inputBits = inputBits;
        this.inputMultibit = inputMultibit;
        this.outputBits = outputBits;
        this.outputMultibit = outputMultibit;
        this.createComponent = createComponent;
        this.inputBitsString = inputBitsString;
        this.inputMultibitString = inputMultibitString;
        this.outputBitsString = outputBitsString;
        this.outputMultibitString = outputMultibitString;
    }

    public bool SubmitUI(Editor.Editor editor)
    {
        if (inputBits)
        {
            ImGui.SetNextItemWidth(80);
            ImGui.InputInt(inputBitsString, ref this.selectedInputBits);
        }
        if (inputMultibit)
        {
            ImGui.Checkbox(inputMultibitString, ref this.selectedInputMultibit);
        }
        if (outputBits)
        {
            ImGui.SetNextItemWidth(80);
            ImGui.InputInt(outputBitsString, ref this.selectedOutputBits);
        }
        if (outputMultibit)
        {
            ImGui.Checkbox(outputMultibitString, ref this.selectedOutputMultibit);
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