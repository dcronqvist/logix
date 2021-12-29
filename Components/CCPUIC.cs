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
            arg.SelectFolder(Directory.GetCurrentDirectory(), (folder) =>
            {
                desc.SaveToFile(folder);
            });
            return true;
        }

        if (ImGui.Button("Exclude"))
        {
            this.exclude(this.desc);
            return true;
        }

        return false;
    }
}