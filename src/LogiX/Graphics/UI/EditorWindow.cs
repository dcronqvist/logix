using ImGuiNET;
using LogiX.Architecture;

namespace LogiX.Graphics.UI;

public abstract class EditorWindow
{
    public abstract void SubmitUI(Editor editor);
}