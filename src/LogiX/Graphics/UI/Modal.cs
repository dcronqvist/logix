using ImGuiNET;
using LogiX.Architecture;

namespace LogiX.Graphics.UI;

public abstract class Modal
{
    public ImGuiPopupFlags PopupFlags { get; set; }
    public ImGuiWindowFlags WindowFlags { get; set; }
    public string Title { get; set; }

    public Modal(string title, ImGuiWindowFlags windowFlags, ImGuiPopupFlags popupFlags)
    {
        this.Title = title;
        this.WindowFlags = windowFlags;
        this.PopupFlags = popupFlags;
    }

    public abstract void SubmitUI(Editor editor);
}

public class DynamicModal : Modal
{
    private Action<Editor> SubmitUIAction { get; set; }

    public DynamicModal(string title, ImGuiWindowFlags windowFlags, ImGuiPopupFlags popupFlags, Action<Editor> submitUIAction) : base(title, windowFlags, popupFlags)
    {
        this.SubmitUIAction = submitUIAction;
    }

    public override void SubmitUI(Editor editor)
    {
        this.SubmitUIAction(editor);
    }
}
