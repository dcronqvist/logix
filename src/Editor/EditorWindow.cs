namespace LogiX.Editor;

public abstract class EditorWindow
{
    public bool IsOpen { get; private set; }
    public string ID { get; private set; }

    public EditorWindow()
    {
        this.IsOpen = true;
        this.ID = Guid.NewGuid().ToString();
    }

    protected void Close()
    {
        IsOpen = false;
    }

    public abstract void Draw(Editor editor);
}

public class SettingsWindow : EditorWindow
{
    public override void Draw(Editor editor)
    {
        bool isOpen = true;
        ImGui.Begin($"Settings ###{this.ID}", ref isOpen, ImGuiWindowFlags.AlwaysAutoResize);
        if (!isOpen)
        {
            Close();
        }

        Dictionary<string, Setting> settings = Settings.GetAllSettings();

        foreach (KeyValuePair<string, Setting> setting in settings.Where(x => x.Value.VisibleInSettingsEditor))
        {
            SettingInput(setting.Key, setting.Value);
        }

        if (ImGui.Button("Save Settings"))
        {
            Settings.SaveSettings();
            Close();
        }

        ImGui.End();
    }

    public void SettingInput(string key, Setting s)
    {
        switch (s.Value.GetType().ToString())
        {
            case "System.Int32":
                int v = s.GetValue<int>();
                ImGui.SliderInt($"{s.Name}", ref v, 1, 240, $"%d fps");
                s.Value = v;
                break;

            case "Raylib_cs.Color":
                Color c = s.GetValue<Color>();
                Vector3 col = c.ToVector3();
                ImGui.ColorEdit3($"{s.Name}", ref col);
                s.Value = col.ToColor();
                break;
        }

    }
}