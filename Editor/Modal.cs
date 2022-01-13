namespace LogiX.Editor;

public abstract class Modal
{
    private ImGuiPopupFlags popupFlags;
    private ImGuiWindowFlags windowFlags;
    private bool isOpen;

    public Modal()
    {
        this.isOpen = true;
    }

    public Modal(ImGuiPopupFlags popupFlags, ImGuiWindowFlags windowFlags) : this()
    {
        this.popupFlags = popupFlags;
        this.windowFlags = windowFlags;
    }

    public bool SubmitUI(Editor editor)
    {
        ImGui.OpenPopup($"###Modal");

        if (ImGui.BeginPopupModal($"###Modal", ref this.isOpen, ImGuiWindowFlags.NoNav))
        {
            if (this.SubmitContent(editor))
            {
                // Content has indicated that it is done.
                ImGui.CloseCurrentPopup();
                return true;
            }

            ImGui.EndPopup();
        }

        if (!this.isOpen)
        {
            return true;
        }

        return false;
    }

    public abstract bool SubmitContent(Editor editor);
}