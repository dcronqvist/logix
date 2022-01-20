using LogiX.SaveSystem;

namespace LogiX.Components;

public class CCPUIC : IUISubmitter<bool, Editor.Editor>
{
    private ICDescription desc;
    private Action<ICDescription> exclude;

    public CCPUIC(ICDescription icd, Action<ICDescription> exclude)
    {
        this.desc = icd;
        this.exclude = exclude;
    }

    public bool SubmitUI(Editor.Editor arg)
    {
        if (ImGui.Button("Export to file..."))
        {
            arg.SelectFolder(Util.FileDialogStartDir, (folder) =>
            {
                desc.SaveToFile(folder);
            });
            return true;
        }

        if (ImGui.Button("Exclude"))
        {
            arg.ModalError("Are you sure you want to exclude this IC?", ErrorModalType.YesNo, (result) =>
            {
                if (result == ErrorModalResult.Yes)
                {
                    exclude(desc);
                }
            });
            return true;
        }

        string name = desc.Name;
        if (ImGui.InputText("Name", ref name, 100, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            desc.Name = name;
            return true;
        }
        desc.Name = name;

        return false;
    }
}